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
            get { return ChromeDriver != null || (_process != null && !_process.HasExited); }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsOpenProcess { get { return IsOpenChrome && _process != null; } }

        /// <summary>
        /// 
        /// </summary>
        public bool IsOpenSelenium { get { return IsOpenChrome && ChromeDriver != null; } }

        /// <summary>
        /// 
        /// </summary>
        public event RunningStateChange? StateChange;


        /// <summary>
        /// 
        /// </summary>
        public virtual ChromeDriver? ChromeDriver { get; private set; }



        /// <summary>
        /// 
        /// </summary>
        protected virtual ChromeDriverService? _service { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        protected virtual Process? _process { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        protected virtual IControlChromeProcess? _remoteChromeProcess { get; private set; }


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
        public virtual ChromeOptions DefaultChromeOptions(string? BinaryLocation = null)
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
                if (chromeOptions is null) throw new ArgumentNullException(nameof(chromeOptions));
                if (chromeDriverService is null) throw new ArgumentNullException(nameof(chromeDriverService));
                try
                {
                    ChromeDriver = new ChromeDriver(chromeDriverService, chromeOptions, CommandTimeout);
                    _service = chromeDriverService;
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
                if (remoteChromeProcess is null)
                    throw new ArgumentNullException(nameof(remoteChromeProcess));
                if (chromeDriverService is null)
                    throw new ArgumentNullException(nameof(chromeDriverService));

                if (!await remoteChromeProcess.GetIsOpenChromeAsync(cancellationToken))
                    await remoteChromeProcess.OpenChromeAsync(null, cancellationToken);

                ChromeOptions chromeOptions = await remoteChromeProcess.GetChromeOptionsAsync(cancellationToken);
                if (chromeOptions is null)
                    throw new InvalidOperationException($"{typeof(IControlChromeProcess).FullName}.{nameof(IControlChromeProcess.GetChromeOptionsAsync)} return null");

                try
                {
                    ChromeDriver = new ChromeDriver(chromeDriverService, chromeOptions, CommandTimeout);
                    _remoteChromeProcess = remoteChromeProcess;
                    _service = chromeDriverService;
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
                ChromeDriver?.Quit();
                ChromeDriver = null;
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
        public virtual void Delay(int min, int max, CancellationToken cancellationToken = default)
        {
            Delay(rd.Next(min, max), cancellationToken);
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual void Delay(int time, CancellationToken cancellationToken = default)
        {
            DelayAsync(time, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual Task DelayAsync(int time, CancellationToken cancellationToken = default)
        {
            return Task.Delay(time, cancellationToken);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual Task DelayAsync(int min, int max, CancellationToken cancellationToken = default)
        {
            return DelayAsync(rd.Next(min, max), cancellationToken);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="by"></param>
        /// <returns></returns>
        public virtual ReadOnlyCollection<IWebElement> FindElements(By by)
        {
            if (ChromeDriver is null) throw new InvalidOperationException($"{nameof(ChromeDriver)} is null, need start chrome first");
            return ChromeDriver.FindElements(by);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cssSelector"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual ReadOnlyCollection<IWebElement> FindElements(string cssSelector)
        {
            if (ChromeDriver is null) throw new InvalidOperationException($"{nameof(ChromeDriver)} is null, need start chrome first");
            return ChromeDriver.FindElements(cssSelector);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <returns></returns>
        public virtual FrameSwitch FrameSwitch(IWebElement webElement) => new FrameSwitch(ChromeDriver!, webElement);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual TabSwitch TabSwitchFromUrl(string url, bool isCloseOnDispose = true) => TabSwitch.FromUrl(ChromeDriver!, url, isCloseOnDispose);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tabId"></param>
        /// <param name="isCloseOnDispose"></param>
        /// <returns></returns>
        public virtual TabSwitch TabSwitchFromExistTab(string tabId, bool isCloseOnDispose = true) => TabSwitch.FromExistTab(ChromeDriver!, tabId, isCloseOnDispose);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual WaitHelper WaitHelper(CancellationToken cancellationToken) => new WaitHelper(this, cancellationToken);

        /// <summary>
        /// 
        /// </summary>
        public virtual void InitUndectedChromeDriver()
        {
            if (ChromeDriver is null) throw new InvalidOperationException($"{nameof(ChromeDriver)} is null, need start chrome first");

            var parameters = new Dictionary<string, object>
            {
                ["source"] = "Object.defineProperty(navigator, 'webdriver', { get: () => undefined })"
            };
            this.ChromeDriver.ExecuteCdpCommand("Page.addScriptToEvaluateOnNewDocument", parameters);

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
            this.ChromeDriver.ExecuteCdpCommand("Page.addScriptToEvaluateOnNewDocument", parameters);
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