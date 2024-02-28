using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TqkLibrary.SeleniumSupport.Helper;
using TqkLibrary.SeleniumSupport.Helper.WaitHeplers;

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
        protected static readonly Random rd = new Random();
        /// <summary>
        /// 
        /// </summary>
        protected ChromeDriverService? _service;
        /// <summary>
        /// 
        /// </summary>
        public string? ChromeDriverPath { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool HideCommandPromptWindow { get; set; } = true;
        private CancellationTokenRegistration? cancellationTokenRegistration;
        /// <summary>
        /// 
        /// </summary>
        public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromMinutes(3);
        /// <summary>
        /// 
        /// </summary>
        public bool IsOpenChrome
        {
            get { return chromeDriver != null || (process != null && !process.HasExited); }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsOpenProcess { get { return IsOpenChrome && process != null; } }

        /// <summary>
        /// 
        /// </summary>
        public bool IsOpenSelenium { get { return IsOpenChrome && chromeDriver != null; } }

        /// <summary>
        /// 
        /// </summary>
        public CancellationToken? Token { get { return tokenSource?.Token; } }
        /// <summary>
        /// 
        /// </summary>
        internal protected ChromeDriver? chromeDriver { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        protected CancellationTokenSource? tokenSource { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        protected Process? process { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public event RunningStateChange? StateChange;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ChromeDriverPath"></param>
        public BaseChromeProfile(string ChromeDriverPath)
        {
            if (string.IsNullOrEmpty(ChromeDriverPath))
            {
                ChromeDriverPath = Directory.GetCurrentDirectory() + "\\AppData\\ChromeDriver";
            }
            this.ChromeDriverPath = ChromeDriverPath;
            if (!Directory.Exists(ChromeDriverPath)) Directory.CreateDirectory(ChromeDriverPath);
        }

        /// <summary>
        /// <strong>AddArguments:</strong>
        /// <para>--no-sandbox<br/>
        /// --disable-notifications<br/>
        /// --disable-web-security<br/>
        /// --disable-blink-features<br/>
        /// --disable-translate<br/>
        /// --disable-notifications<br/>
        /// --disable-blink-features=AutomationControlled<br/>
        /// --disable-infobars<br/>
        /// --ignore-certificate-errors<br/>
        /// --ignore-certificate-errors<br/>
        /// --allow-running-insecure-content</para>
        ///
        /// <strong>AddExcludedArgument:</strong>
        /// <para>enable-automation</para>
        ///
        /// <strong>AddAdditionalCapability:</strong>
        /// <para>useAutomationExtension false</para>
        ///
        /// <strong>AddUserProfilePreference:</strong>
        /// <para>credentials_enable_service false<br/>
        /// profile.password_manager_enabled false</para>
        /// </summary>
        /// <returns></returns>
        public ChromeOptions DefaultChromeOptions(string? BinaryLocation = null)
        {
            ChromeOptions options = new ChromeOptions();
            if (!string.IsNullOrEmpty(BinaryLocation)) options.BinaryLocation = BinaryLocation;
            options.AddArguments("--no-sandbox");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-web-security");
            options.AddArgument("--disable-translate");
            options.AddArgument("--disable-blink-features");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("--disable-infobars");
            options.AddArgument("--ignore-certificate-errors");
            options.AddArgument("--allow-running-insecure-content");
            options.AddAdditionalOption("useAutomationExtension", false);
            options.AddExcludedArgument("enable-automation");
            //options.c
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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public bool OpenChrome(ChromeOptions chromeOptions, ChromeDriverService? chromeDriverService = null, CancellationToken cancellationToken = default)
        {
            if (!IsOpenChrome)
            {
                if (chromeDriverService != null) _service = chromeDriverService;
                else
                {
                    _service = ChromeDriverService.CreateDefaultService(ChromeDriverPath);
                    _service.HideCommandPromptWindow = HideCommandPromptWindow;
                }

                tokenSource = new CancellationTokenSource();
                cancellationTokenRegistration = cancellationToken.Register(() => { if (tokenSource?.IsCancellationRequested == false) tokenSource.Cancel(); });
                try
                {
                    chromeDriver = new ChromeDriver(_service, chromeOptions, CommandTimeout);
                }
                catch
                {
                    _service.Dispose();
                    _service = null;
                    return false;
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
        public Process? OpenChromeWithoutSelenium(string Arguments, string? ChromePath = null)
        {
            if (!IsOpenChrome)
            {
                process = new Process();
                if (!string.IsNullOrEmpty(ChromePath)) process.StartInfo.FileName = ChromePath;
                else process.StartInfo.FileName = ChromeDriverUpdater.GetChromePath();
                process.StartInfo.WorkingDirectory = new FileInfo(process.StartInfo.FileName).Directory!.FullName;
                process.StartInfo.Arguments = Arguments;
                process.EnableRaisingEvents = true;
                process.Exited += Process_Exited;
                process.Start();
                StateChange?.Invoke(IsOpenChrome);
                return process;
            }
            return null;
        }

        private void Process_Exited(object? sender, EventArgs e)
        {
            this.process = null;
            StateChange?.Invoke(IsOpenChrome);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool CloseChrome()
        {
            if (IsOpenChrome)
            {
                bool callStateChange = true;
                if (process?.HasExited == false)
                {
                    process?.Kill();
                    callStateChange = false;
                }
                process?.Dispose();
                process = null;
                chromeDriver?.Quit();
                chromeDriver = null;
                _service?.Dispose();
                _service = null;
                cancellationTokenRegistration?.Dispose();
                cancellationTokenRegistration = null;
                tokenSource?.Dispose();
                tokenSource = null;
                if (callStateChange) StateChange?.Invoke(IsOpenChrome);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop() => tokenSource?.Cancel();

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
        public async Task DelayAsync(int time, CancellationToken cancellationToken = default)
        {
            TaskCompletionSource<object?> tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var register = cancellationToken.Register(() => tcs.TrySetCanceled());
            using var register2 = tokenSource?.Token.Register(() => tcs.TrySetCanceled());
            _ = Task.Delay(time).ContinueWith((t) => tcs.TrySetResult(null), TaskContinuationOptions.RunContinuationsAsynchronously);
            await tcs.Task.ConfigureAwait(false);
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
            if (chromeDriver is null) throw new InvalidOperationException($"{nameof(chromeDriver)} is null, need start chrome first");
            return chromeDriver.FindElements(by);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <returns></returns>
        public FrameSwitch FrameSwitch(IWebElement webElement) => new FrameSwitch(chromeDriver!, webElement);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TabSwitch TabSwitch(string url, bool isCloseOnDispose = true) => new TabSwitch(chromeDriver!, url) { IsCloseTab = isCloseOnDispose };

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
            if (this.chromeDriver is null) return;

            var parameters = new Dictionary<string, object>
            {
                ["source"] = "Object.defineProperty(navigator, 'webdriver', { get: () => undefined })"
            };
            this.chromeDriver.ExecuteCdpCommand("Page.addScriptToEvaluateOnNewDocument", parameters);

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
            this.chromeDriver.ExecuteCdpCommand("Page.addScriptToEvaluateOnNewDocument", parameters);
        }

    }
}