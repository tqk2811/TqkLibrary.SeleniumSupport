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
  public delegate void RunningStateChange(bool change);

  public abstract class BaseChromeProfile
  {
    protected static readonly Random rd = new Random();
    protected ChromeDriverService service;
    public string ChromeDriverPath { get; set; }
    public bool HideCommandPromptWindow { get; set; } = true;
    private CancellationTokenRegistration? cancellationTokenRegistration;
    public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromMinutes(3);
    public bool IsOpenChrome
    {
      get { return chromeDriver != null || (process != null && !process.HasExited); }
    }

    public CancellationToken Token { get { return tokenSource.Token; } }

    protected ChromeDriver chromeDriver { get; private set; }
    protected CancellationTokenSource tokenSource { get; private set; }
    protected Process process { get; private set; }

    public event RunningStateChange StateChange;

    public BaseChromeProfile(string ChromeDriverPath)
    {
      if (string.IsNullOrEmpty(ChromeDriverPath))
      {
        ChromeDriverPath = Directory.GetCurrentDirectory() + "\\AppData\\ChromeDriver";
      }
      this.ChromeDriverPath = ChromeDriverPath;
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
      options.AddArgument("--disable-notifications");
      options.AddArgument("--disable-blink-features");
      options.AddArgument("--disable-blink-features=AutomationControlled");
      options.AddArgument("--disable-infobars");
      options.AddArgument("--ignore-certificate-errors");
      options.AddArgument("--allow-running-insecure-content");
      options.AddAdditionalCapability("useAutomationExtension", false);
      options.AddExcludedArgument("enable-automation");
      //options.c
      //disable ask password
      options.AddUserProfilePreference("credentials_enable_service", false);
      options.AddUserProfilePreference("profile.password_manager_enabled", false);
      return options;
    }

    public ChromeOptions LoadFromJsonFile(string filePath)
    {
      ChromeOptionConfig chromeOptionConfig = JsonConvert.DeserializeObject<ChromeOptionConfig>(File.ReadAllText(filePath));
      ChromeOptions chromeOptions = new ChromeOptions();
      chromeOptionConfig.Arguments?.ForEach(x => chromeOptions.AddArgument(x));
      chromeOptionConfig.ExcludedArguments?.ForEach(x => chromeOptions.AddExcludedArgument(x));
      chromeOptionConfig.AdditionalCapabilitys?.ForEach(x => chromeOptions.AddAdditionalCapability(x.Name, x.Value));
      chromeOptionConfig.UserProfilePreferences?.ForEach(x => chromeOptions.AddUserProfilePreference(x.Name, x.Value));
      if (chromeOptionConfig.UserAgents?.Count > 0)
      {
        chromeOptions.AddUserAgent(chromeOptionConfig.UserAgents[new Random().Next(chromeOptionConfig.UserAgents.Count)]);
      }
      return chromeOptions;
    }

    public bool OpenChrome(ChromeOptions chromeOptions, CancellationToken cancellationToken = default)
    {
      if (!IsOpenChrome)
      {
        service = ChromeDriverService.CreateDefaultService(ChromeDriverPath);
        service.HideCommandPromptWindow = HideCommandPromptWindow;

        tokenSource = new CancellationTokenSource();
        cancellationTokenRegistration = cancellationToken.Register(() => { if (tokenSource?.IsCancellationRequested == false) tokenSource.Cancel(); });
        chromeDriver = new ChromeDriver(service, chromeOptions, CommandTimeout);
        StateChange?.Invoke(IsOpenChrome);
        return true;
      }
      return false;
    }

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
      StateChange?.Invoke(IsOpenChrome);
    }

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
        if(callStateChange) StateChange?.Invoke(IsOpenChrome);
        return true;
      }
      return false;
    }

    public void Stop() => tokenSource?.Cancel();

    public void Delay(int min, int max)
    {
      Task.Delay(rd.Next(min, max), tokenSource.Token).Wait();
    }
    public void Delay(int time)
    {
      Task.Delay(time, tokenSource.Token).Wait();
    }

    public void SaveHtml(string path)
    {
      if (IsOpenChrome)
      {
        using StreamWriter streamWriter = new StreamWriter(path, false);
        streamWriter.Write(chromeDriver.PageSource);
        streamWriter.Flush();
      }
    }

    public void SwitchToFrame(By by) => chromeDriver.SwitchTo().Frame(WaitUntil(by, ElementsExists, true).First());

    public ReadOnlyCollection<IWebElement> FindElements(By by) => chromeDriver.FindElements(by);

    public FrameSwitch FrameSwitch(IWebElement webElement) => new FrameSwitch(chromeDriver, webElement);

    #region WaitUntil

    #region Func

    public static bool ElementsExists(ReadOnlyCollection<IWebElement> webElements) => webElements.Count > 0;

    public static bool AllElementsVisible(ReadOnlyCollection<IWebElement> webElements) => webElements.Count > 0 && webElements.All(x => x.Displayed);

    public static bool AnyElementsVisible(ReadOnlyCollection<IWebElement> webElements) => webElements.Any(x => x.Displayed);

    public static bool AllElementsClickable(ReadOnlyCollection<IWebElement> webElements) => webElements.Count > 0 && webElements.All(x => x.Displayed && x.Enabled);

    public static bool AnyElementsClickable(ReadOnlyCollection<IWebElement> webElements) => webElements.Any(x => x.Displayed && x.Enabled);

    public static bool AllElementsSelected(ReadOnlyCollection<IWebElement> webElements) => webElements.Count > 0 && webElements.All(x => x.Selected);

    public static bool AnyElementsSelected(ReadOnlyCollection<IWebElement> webElements) => webElements.Any(x => x.Selected);

    public static bool StartsWith(string url, string check) => url.StartsWith(check);
    public static bool EndsWith(string url, string check) => url.EndsWith(check);
    public static bool Equals(string url, string check) => url.Equals(check);
    public static bool Contains(string url, string check) => url.Contains(check);

    public static bool NotStartsWith(string url, string check) => !url.StartsWith(check);
    public static bool NotEndsWith(string url, string check) => !url.EndsWith(check);
    public static bool NotEquals(string url, string check) => !url.Equals(check);
    public static bool NotContains(string url, string check) => !url.Contains(check);
    #endregion Func

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <param name="func">(string url, string check)</param>
    /// <param name="isThrow"></param>
    /// <param name="delay"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public bool WaitUntilUrl(Func<string,bool> func, bool isThrow = true, int delay = 500, int timeout = 10000)
    {
      using CancellationTokenSource timeoutToken = new CancellationTokenSource(timeout);
      while (!timeoutToken.IsCancellationRequested)
      {
        if (func(chromeDriver.Url)) return true;
        if (tokenSource != null) Task.Delay(delay, tokenSource.Token).Wait();
        else Task.Delay(delay, timeoutToken.Token).Wait();

        tokenSource?.Token.ThrowIfCancellationRequested();
      }
      if (isThrow) throw new ChromeAutoException($"WaitUntilUrl failed");
      return false;
    }

    public ReadOnlyCollection<IWebElement> WaitUntil(By by, Func<ReadOnlyCollection<IWebElement>, bool> func, bool isThrow = true, int delay = 500, int timeout = 10000)
    => WaitUntil_(chromeDriver, by, func, isThrow, delay, timeout);

    public ReadOnlyCollection<IWebElement> WaitUntil(IWebElement webElement, By by, Func<ReadOnlyCollection<IWebElement>, bool> func, bool isThrow = true, int delay = 500, int timeout = 10000)
    => WaitUntil_(webElement, by, func, isThrow, delay, timeout);

    private ReadOnlyCollection<IWebElement> WaitUntil_(ISearchContext searchContext, By by, Func<ReadOnlyCollection<IWebElement>, bool> func,
      bool isThrow = true, int delay = 200, int timeout = 10000)
    {
      using CancellationTokenSource timeoutToken = new CancellationTokenSource(timeout);
      while (!timeoutToken.IsCancellationRequested)
      {
        var eles = searchContext.FindElements(by);
        try { if (func(eles)) return eles; } catch { }
        if (tokenSource != null) Task.Delay(delay, tokenSource.Token).Wait();
        else Task.Delay(delay, timeoutToken.Token).Wait();

        tokenSource?.Token.ThrowIfCancellationRequested();
      }
      if (isThrow) throw new ChromeAutoException(by.ToString());
      return null;
    }


    //protected string WaitUntilUrl(string q,)
    //{

    //}


    #endregion WaitUntil

    #region JSDropFile
    public void JsDropFile(string file, IWebElement webElement, int offsetX, int offsetY)
    {
      IWebElement input = (IWebElement)chromeDriver.ExecuteScript(Resource.JsDropFile, webElement, offsetX, offsetY);
      input.SendKeys(file);
    }

    #endregion JSDropFile

    #region JsClick

    public void JsDoubleClick(IWebElement webElement) => chromeDriver.ExecuteScript(@"var evt = document.createEvent('MouseEvents');
evt.initMouseEvent('dblclick',true, true, window, 0, 0, 0, 0, 0, false, false, false, false, 0,null);
arguments[0].dispatchEvent(evt);", webElement);

    public void JsClick(IWebElement webElement) => chromeDriver.ExecuteScript("arguments[0].click();", webElement);

    #endregion JsClick

    public void JsScrollIntoView(IWebElement webElement) => chromeDriver.ExecuteScript("arguments[0].scrollIntoView();", webElement);

    public void JsSetInputText(IWebElement webElement, string text) => chromeDriver.ExecuteScript($"arguments[0].value = \"{text}\";", webElement);
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