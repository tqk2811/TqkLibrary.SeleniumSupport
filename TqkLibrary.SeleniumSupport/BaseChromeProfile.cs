using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TqkLibrary.SeleniumSupport.Helper;

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
        protected ChromeDriverService service;
        /// <summary>
        /// 
        /// </summary>
        public string ChromeDriverPath { get; set; }
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
        public CancellationToken Token { get { return tokenSource.Token; } }
        /// <summary>
        /// 
        /// </summary>
        internal protected ChromeDriver chromeDriver { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        protected CancellationTokenSource tokenSource { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        protected Process process { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public event RunningStateChange StateChange;
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
        public ChromeOptions DefaultChromeOptions(string BinaryLocation = null)
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
        /// <returns></returns>
        public ChromeOptions LoadFromConfig(ChromeOptionConfig chromeOptionConfig)
        {
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptionConfig.Arguments?.ForEach(x => chromeOptions.AddArgument(x));
            chromeOptionConfig.ExcludedArguments?.ForEach(x => chromeOptions.AddExcludedArgument(x));
            chromeOptionConfig.AdditionalCapabilitys?.ForEach(x => chromeOptions.AddAdditionalOption(x.Name, x.Value));
            chromeOptionConfig.UserProfilePreferences?.ForEach(x => chromeOptions.AddUserProfilePreference(x.Name, x.Value));
            if (chromeOptionConfig.UserAgents?.Count > 0)
            {
                chromeOptions.AddUserAgent(chromeOptionConfig.UserAgents[new Random().Next(chromeOptionConfig.UserAgents.Count)]);
            }
            return chromeOptions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chromeOptions"></param>
        /// <param name="chromeDriverService"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public bool OpenChrome(ChromeOptions chromeOptions, ChromeDriverService chromeDriverService = null, CancellationToken cancellationToken = default)
        {
            if (!IsOpenChrome)
            {
                if (chromeDriverService != null) service = chromeDriverService;
                else
                {
                    service = ChromeDriverService.CreateDefaultService(ChromeDriverPath);
                    service.HideCommandPromptWindow = HideCommandPromptWindow;
                }

                tokenSource = new CancellationTokenSource();
                cancellationTokenRegistration = cancellationToken.Register(() => { if (tokenSource?.IsCancellationRequested == false) tokenSource.Cancel(); });
                try
                {
                    chromeDriver = new ChromeDriver(service, chromeOptions, CommandTimeout);
                }
                catch
                {
                    service.Dispose();
                    service = null;
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
        public Process OpenChromeWithoutSelenium(string Arguments, string ChromePath = null)
        {
            if (!IsOpenChrome)
            {
                process = new Process();
                if (!string.IsNullOrEmpty(ChromePath)) process.StartInfo.FileName = ChromePath;
                else process.StartInfo.FileName = ChromeDriverUpdater.GetChromePath();
                process.StartInfo.WorkingDirectory = new FileInfo(process.StartInfo.FileName).Directory.FullName;
                process.StartInfo.Arguments = Arguments;
                process.EnableRaisingEvents = true;
                process.Exited += Process_Exited;
                process.Start();
                StateChange?.Invoke(IsOpenChrome);
                return process;
            }
            return null;
        }

        private void Process_Exited(object sender, EventArgs e)
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
                service?.Dispose();
                service = null;
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
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var register = cancellationToken.Register(() => tcs.TrySetCanceled());
            using var register2 = tokenSource.Token.Register(() => tcs.TrySetCanceled());
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
        public ReadOnlyCollection<IWebElement> FindElements(By by) => chromeDriver.FindElements(by);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <returns></returns>
        public FrameSwitch FrameSwitch(IWebElement webElement) => new FrameSwitch(chromeDriver, webElement);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TabSwitch TabSwitch(string url, bool isCloseOnDispose = true) => new TabSwitch(chromeDriver, url) { IsCloseTab = isCloseOnDispose };

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitElementHepler WaitElementHepler(CancellationToken cancellationToken) => new WaitElementHepler(this, cancellationToken);

        #region WaitFunc

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool ElementsExists(ReadOnlyCollection<IWebElement> webElements) => webElements.Count > 0;

        public static bool AllElementsVisible(ReadOnlyCollection<IWebElement> webElements) => webElements.Count > 0 && webElements.All(x => x.Displayed);

        public static bool AnyElementsVisible(ReadOnlyCollection<IWebElement> webElements) => webElements.Any(x => x.Displayed);

        public static bool AllElementsClickable(ReadOnlyCollection<IWebElement> webElements) => webElements.Count > 0 && webElements.All(x => x.Displayed && x.Enabled);

        public static bool AnyElementsClickable(ReadOnlyCollection<IWebElement> webElements) => webElements.Any(x => x.Displayed && x.Enabled);

        public static bool AllElementsSelected(ReadOnlyCollection<IWebElement> webElements) => webElements.Count > 0 && webElements.All(x => x.Selected);

        public static bool AnyElementsSelected(ReadOnlyCollection<IWebElement> webElements) => webElements.Any(x => x.Selected);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #endregion

    }
}

//protected virtual ReadOnlyCollection<IWebElement> WaitUntilAll(IWebElement parent, By by, ElementsIs waitFlag = ElementsIs.Exists, int delay = 500, int timeout = 10000, CancellationTokenSource tokenSource = null)
//{
//  if (IsOpenChrome)
//  {
//    using CancellationTokenSource timeoutToken = new CancellationTokenSource(timeout);
//    while (!timeoutToken.IsCancellationRequested && tokenSource?.IsCancellationRequested != true)
//    {
//      Delay(delay, delay);
//      var eles = parent.FindElements(by);
//      if (eles.Count > 0)
//      {
//        switch (waitFlag)
//        {
//          case ElementsIs.Exists: return eles;

//          case ElementsIs.Visible:
//            if (eles.All(x => x.Displayed)) return eles;
//            break;

//          case ElementsIs.Clickable:
//            if (eles.All(x => x.Displayed && x.Enabled)) return eles;
//            break;

//          case ElementsIs.Selected:
//            if (eles.All(x => x.Selected)) return eles;
//            break;
//        }
//      }
//      else
//      {
//        switch (waitFlag)
//        {
//          case ElementsIs.NotExists:
//            return new ReadOnlyCollection<IWebElement>(new List<IWebElement>());
//        }
//      }
//    }
//  }
//  return null;
//}

//protected virtual ReadOnlyCollection<IWebElement> WaitUntilAny(IWebElement parent, By by, ElementsIs waitFlag = ElementsIs.Exists, int delay = 500, int timeout = 10000, CancellationTokenSource tokenSource = null)
//{
//  if (IsOpenChrome)
//  {
//    CancellationTokenSource timeoutToken = new CancellationTokenSource(timeout);
//    while (!timeoutToken.IsCancellationRequested && tokenSource?.IsCancellationRequested != true)
//    {
//      Delay(delay, delay);
//      var eles = parent.FindElements(by);
//      if (eles.Count > 0)
//      {
//        switch (waitFlag)
//        {
//          case ElementsIs.Exists: return eles;

//          case ElementsIs.Visible:
//            if (eles.Any(x => x.Displayed)) return eles;
//            break;

//          case ElementsIs.Clickable:
//            if (eles.Any(x => x.Displayed && x.Enabled)) return eles;
//            break;

//          case ElementsIs.Selected:
//            if (eles.Any(x => x.Selected)) return eles;
//            break;
//        }
//      }
//      else
//      {
//        switch (waitFlag)
//        {
//          case ElementsIs.NotExists:
//            return new ReadOnlyCollection<IWebElement>(new List<IWebElement>());
//        }
//      }
//    }
//  }
//  return null;
//}

//protected virtual ReadOnlyCollection<IWebElement> WaitUntilAll(By by, ElementsIs waitFlag = ElementsIs.Exists, int delay = 500, int timeout = 10000, CancellationTokenSource tokenSource = null)
//{
//  if (IsOpenChrome)
//  {
//    using CancellationTokenSource timeoutToken = new CancellationTokenSource(timeout);
//    while (!timeoutToken.IsCancellationRequested && tokenSource?.IsCancellationRequested != true)
//    {
//      Delay(delay, delay);
//      var eles = chromeDriver.FindElements(by);
//      if (eles.Count > 0)
//      {
//        switch (waitFlag)
//        {
//          case ElementsIs.Exists: return eles;

//          case ElementsIs.Visible:
//            if (eles.All(x => x.Displayed)) return eles;
//            break;

//          case ElementsIs.Clickable:
//            if (eles.All(x => x.Displayed && x.Enabled)) return eles;
//            break;

//          case ElementsIs.Selected:
//            if (eles.All(x => x.Selected)) return eles;
//            break;
//        }
//      }
//      else
//      {
//        switch (waitFlag)
//        {
//          case ElementsIs.NotExists:
//            return new ReadOnlyCollection<IWebElement>(new List<IWebElement>());
//        }
//      }
//    }
//  }
//  return null;
//}

//protected virtual ReadOnlyCollection<IWebElement> WaitUntilAny(By by, ElementsIs waitFlag = ElementsIs.Exists, int delay = 500, int timeout = 10000, CancellationTokenSource tokenSource = null)
//{
//  if (IsOpenChrome)
//  {
//    CancellationTokenSource timeoutToken = new CancellationTokenSource(timeout);
//    while (!timeoutToken.IsCancellationRequested && tokenSource?.IsCancellationRequested != true)
//    {
//      Delay(delay, delay);
//      var eles = chromeDriver.FindElements(by);
//      if (eles.Count > 0)
//      {
//        switch (waitFlag)
//        {
//          case ElementsIs.Exists: return eles;

//          case ElementsIs.Visible:
//            if (eles.Any(x => x.Displayed)) return eles;
//            break;

//          case ElementsIs.Clickable:
//            if (eles.Any(x => x.Displayed && x.Enabled)) return eles;
//            break;

//          case ElementsIs.Selected:
//            if (eles.Any(x => x.Selected)) return eles;
//            break;
//        }
//      }
//      else
//      {
//        switch (waitFlag)
//        {
//          case ElementsIs.NotExists:
//            return new ReadOnlyCollection<IWebElement>(new List<IWebElement>());
//        }
//      }
//    }
//  }
//  return null;
//}