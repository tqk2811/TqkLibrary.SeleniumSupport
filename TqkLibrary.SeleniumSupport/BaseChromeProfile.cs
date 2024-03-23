using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TqkLibrary.SeleniumSupport.Helper;
using TqkLibrary.SeleniumSupport.Helper.WaitHeplers;
using TqkLibrary.SeleniumSupport.Interfaces;

namespace TqkLibrary.SeleniumSupport
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="change"></param>
    public delegate void RunningStateChange(bool change);
    /// <summary>
    /// 
    /// </summary>
    public abstract partial class BaseChromeProfile
    {
        /// <summary>
        /// 
        /// </summary>
        public bool HideCommandPromptWindow { get; set; } = true;
        /// <summary>
        /// 
        /// </summary>
        public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromMinutes(3);


        /// <summary>
        /// 
        /// </summary>
        public bool IsOpenChrome
        {
            get { return _chromeDriver != null || (_process != null && !_process.HasExited); }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsOpenProcess { get { return IsOpenChrome && _process != null; } }

        /// <summary>
        /// 
        /// </summary>
        public bool IsOpenSelenium { get { return IsOpenChrome && _chromeDriver != null; } }

        /// <summary>
        /// 
        /// </summary>
        public event RunningStateChange? StateChange;





        /// <summary>
        /// 
        /// </summary>
        protected ChromeDriverService? _service { get; set; }
        /// <summary>
        /// 
        /// </summary>
        protected Process? _process { get; set; }
        /// <summary>
        /// 
        /// </summary>
        protected IControlChromeProcess? _remoteChromeProcess { get; set; }
        /// <summary>
        /// 
        /// </summary>
        internal protected ChromeDriver? _chromeDriver { get; set; }

        /// <summary>
        /// <strong>AddArguments:</strong>
        /// <para>
        /// --no-sandbox<br/>
        /// --disable-notifications<br/>
        /// --disable-web-security<br/>
        /// --disable-translate<br/>
        /// --disable-blink-features<br/>
        /// --disable-blink-features=AutomationControlled<br/>
        /// --disable-popup-blocking<br/>
        /// --disable-infobars<br/>
        /// --ignore-certificate-errors<br/>
        /// --allow-running-insecure-content
        /// </para>
        ///
        /// <strong>AddExcludedArgument:</strong>
        /// <para>
        /// enable-automation
        /// </para>
        ///
        /// <strong>AddAdditionalCapability:</strong>
        /// <para>
        /// useAutomationExtension false
        /// </para>
        ///
        /// <strong>AddUserProfilePreference:</strong>
        /// <para>
        /// credentials_enable_service false<br/>
        /// profile.password_manager_enabled false
        /// </para>
        /// </summary>
        /// <returns></returns>
        public ChromeOptions DefaultChromeOptions(string? BinaryLocation = null)
        {
            ChromeOptions options = new ChromeOptions();
            if (!string.IsNullOrEmpty(BinaryLocation)) options.BinaryLocation = BinaryLocation;
            options.AddArguments("--no-sandbox");
            options.AddArgument("--disable-notifications");//disable noti
            options.AddArgument("--disable-web-security");
            options.AddArgument("--disable-translate");//disable ask for translate
            options.AddArgument("--disable-blink-features");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("--disable-popup-blocking");//disable ask permision for clipboard,....
            options.AddArgument("--disable-infobars");//remove 'Chrome is being controlled by.....'
            options.AddArgument("--ignore-certificate-errors");//ignore ssl error
            options.AddArgument("--allow-running-insecure-content");
            options.AddAdditionalOption("useAutomationExtension", false);
            options.AddExcludedArgument("enable-automation");

            //disable ask password
            options.AddUserProfilePreference("credentials_enable_service", false);
            options.AddUserProfilePreference("profile.password_manager_enabled", false);

            return options;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chromeOptions"></param>
        /// <param name="chromeDriverService"></param>
        /// <returns></returns>
        public virtual bool OpenChrome(ChromeOptions chromeOptions, ChromeDriverService chromeDriverService)
        {
            if (!IsOpenChrome)
            {
                try
                {
                    _service = chromeDriverService;
                    _chromeDriver = new ChromeDriver(_service, chromeOptions, CommandTimeout);
                }
                catch
                {
                    _service?.Dispose();
                    _service = null;
                    throw;
                }
                finally
                {
                    StateChange?.Invoke(IsOpenChrome);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chromeOptions"></param>
        /// <param name="remoteChromeProcess"></param>
        /// <param name="chromeDriverService"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task<bool> OpenChromeConnectExistedDebugAsync(
            IControlChromeProcess remoteChromeProcess,
            ChromeDriverService chromeDriverService,
            CancellationToken cancellationToken = default
            )
        {
            if (!IsOpenChrome)
            {
                if (!await remoteChromeProcess.GetIsOpenChromeAsync())
                    await remoteChromeProcess.OpenChromeAsync();

                ChromeOptions chromeOptions = new ChromeOptions();
                chromeOptions.DebuggerAddress = await remoteChromeProcess.GetDebuggerAddressAsync();

                try
                {
                    _service = chromeDriverService;
                    _chromeDriver = new ChromeDriver(_service, chromeOptions, CommandTimeout);
                }
                catch
                {
                    _service?.Dispose();
                    _service = null;
                    throw;
                }
                finally
                {
                    StateChange?.Invoke(IsOpenChrome);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Arguments"></param>
        /// <param name="ChromePath"></param>
        /// <returns></returns>
        public virtual Process? OpenChromeWithoutSelenium(string Arguments, string? ChromePath = null)
        {
            if (!IsOpenChrome)
            {
                _process = new Process();
                if (!File.Exists(ChromePath)) ChromePath = ChromeDriverUpdater.GetChromePath();
                _process.StartInfo.FileName = ChromePath;
                _process.StartInfo.WorkingDirectory = new FileInfo(_process.StartInfo.FileName).Directory!.FullName;
                _process.StartInfo.Arguments = Arguments;
                _process.EnableRaisingEvents = true;
                _process.Exited += Process_Exited;
                try
                {
                    _process.Start();
                }
                catch
                {
                    _process.Dispose();
                    _process = null;
                    throw;
                }
                StateChange?.Invoke(IsOpenChrome);
                return _process;
            }
            return null;
        }

        private void Process_Exited(object? sender, EventArgs e)
        {
            this._process = null;
            StateChange?.Invoke(IsOpenChrome);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual async Task CloseChromeAsync(CancellationToken cancellationToken = default)
        {
            if (IsOpenChrome)
            {
                bool callStateChange = true;
                if (_process?.HasExited == false)
                {
                    _process?.Kill();
                    callStateChange = false;
                }
                _process?.Dispose();
                _process = null;
                _chromeDriver?.Quit();
                _chromeDriver = null;
                _service?.Dispose();
                _service = null;
                Task? task = _remoteChromeProcess?.CloseChromeAsync(cancellationToken);
                if (task is not null) await task;
                _remoteChromeProcess = null;
                if (callStateChange) StateChange?.Invoke(IsOpenChrome);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        protected static readonly Random rd = new Random();
        /// <summary>
        /// 
        /// </summary>
        public void Delay(int min, int max, CancellationToken cancellationToken = default)
        {
            Delay(rd.Next(min, max), cancellationToken);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Delay(int time, CancellationToken cancellationToken = default)
        {
            DelayAsync(time, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task DelayAsync(int time, CancellationToken cancellationToken = default)
        {
            return Task.Delay(time, cancellationToken);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task DelayAsync(int min, int max, CancellationToken cancellationToken = default)
        {
            return DelayAsync(rd.Next(min, max), cancellationToken);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="by"></param>
        /// <returns></returns>
        public ReadOnlyCollection<IWebElement> FindElements(By by)
        {
            if (_chromeDriver is null) throw new InvalidOperationException($"{nameof(_chromeDriver)} is null, need start chrome first");
            return _chromeDriver.FindElements(by);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cssSelector"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ReadOnlyCollection<IWebElement> FindElements(string cssSelector)
        {
            if (_chromeDriver is null) throw new InvalidOperationException($"{nameof(_chromeDriver)} is null, need start chrome first");
            return _chromeDriver.FindElements(cssSelector);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <returns></returns>
        public FrameSwitch FrameSwitch(IWebElement webElement) => new FrameSwitch(_chromeDriver!, webElement);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TabSwitch TabSwitchFromUrl(string url, bool isCloseOnDispose = true) => TabSwitch.FromUrl(_chromeDriver!, url, isCloseOnDispose);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tabId"></param>
        /// <param name="isCloseOnDispose"></param>
        /// <returns></returns>
        public TabSwitch TabSwitchFromExistTab(string tabId, bool isCloseOnDispose = true) => TabSwitch.FromExistTab(_chromeDriver!, tabId, isCloseOnDispose);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitHelper WaitHepler(CancellationToken cancellationToken) => new WaitHelper(this, cancellationToken);

        /// <summary>
        /// 
        /// </summary>
        public void InitUndectedChromeDriver()
        {
            if (_chromeDriver is null) throw new InvalidOperationException($"{nameof(_chromeDriver)} is null, need start chrome first");

            var parameters = new Dictionary<string, object>
            {
                ["source"] = "Object.defineProperty(navigator, 'webdriver', { get: () => undefined })"
            };
            this._chromeDriver.ExecuteCdpCommand("Page.addScriptToEvaluateOnNewDocument", parameters);

            parameters = new Dictionary<string, object>
            {
                ["source"] = @"
let objectToInspect = window;
let result = [];
while(objectToInspect !== null){ 
    result = result.concat(Object.getOwnPropertyNames(objectToInspect));
    objectToInspect = Object.getPrototypeOf(objectToInspect); 
}
result.forEach(p => p.match(/.+_.+_(Array|Promise|Symbol)/ig) && delete window[p] && console.log('removed',p))"
            };
            this._chromeDriver.ExecuteCdpCommand("Page.addScriptToEvaluateOnNewDocument", parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static int GetFreePort()
        {
            bool IsFree(int port)
            {
                IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] listeners = properties.GetActiveTcpListeners();
                int[] openPorts = listeners.Select(item => item.Port).ToArray<int>();
                return !openPorts.Contains(port);
            }

            int port = 0;
            Random random = new Random(DateTime.Now.Millisecond);
            do
            {
                port = random.Next(10000, 65535);
            } while (!IsFree(port));
            return port;
        }
    }
}