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
    public string ChromeDrivePath { get; set; }
    public bool HideCommandPromptWindow { get; set; } = true;
    private CancellationTokenRegistration? cancellationTokenRegistration;

    public bool IsOpenChrome
    {
      get { return chromeDriver != null || (process != null && !process.HasExited); }
    }

    public CancellationToken Token { get { return tokenSource.Token; } }

    protected ChromeDriver chromeDriver { get; private set; }
    protected CancellationTokenSource tokenSource { get; private set; }
    protected Process process { get; private set; }

    public event RunningStateChange StateChange;

    protected BaseChromeProfile() : this(null)
    {
    }

    protected BaseChromeProfile(string ChromeDrivePath)
    {
      if (string.IsNullOrEmpty(ChromeDrivePath))
      {
        ChromeDrivePath = Directory.GetCurrentDirectory() + "\\AppData\\ChromeDriver";
      }
      this.ChromeDrivePath = ChromeDrivePath;
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
    protected virtual ChromeOptions DefaultChromeOptions(string BinaryLocation = null)
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

    protected virtual ChromeOptions LoadFromJsonFile(string filePath)
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

    public virtual bool OpenChrome(ChromeOptions chromeOptions) => OpenChrome(chromeOptions, CancellationToken.None);

    public virtual bool OpenChrome(ChromeOptions chromeOptions, CancellationToken cancellationToken)
    {
      if (!IsOpenChrome)
      {
        service = ChromeDriverService.CreateDefaultService(ChromeDrivePath);
        service.HideCommandPromptWindow = HideCommandPromptWindow;

        tokenSource = new CancellationTokenSource();
        cancellationTokenRegistration = cancellationToken.Register(() => { if (tokenSource?.IsCancellationRequested == false) tokenSource.Cancel(); });
        chromeDriver = new ChromeDriver(service, chromeOptions);
        StateChange?.Invoke(IsOpenChrome);
        return true;
      }
      return false;
    }

    public virtual Process OpenChromeWithoutSelenium(string Arguments, string ChromePath = null)
    {
      if (!IsOpenChrome)
      {
        process = new Process();
        if (!string.IsNullOrEmpty(ChromePath)) process.StartInfo.FileName = ChromePath;
        else process.StartInfo.FileName = ChromeDriverUpdater.GetPath();
        process.StartInfo.Arguments = Arguments;
        process.Start();
        StateChange?.Invoke(IsOpenChrome);
        return process;
      }
      return null;
    }

    public virtual bool CloseChrome()
    {
      if (IsOpenChrome)
      {
        if (process?.HasExited == false) process?.Kill();
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
        StateChange?.Invoke(IsOpenChrome);
        return true;
      }
      return false;
    }

    public virtual void Stop() => tokenSource?.Cancel();

    protected virtual void Delay(int min, int max)
    {
      Task.Delay(rd.Next(min, max), tokenSource.Token).Wait();
    }

    public virtual void SaveHtml(string path)
    {
      if (IsOpenChrome)
      {
        using StreamWriter streamWriter = new StreamWriter(path, false);
        streamWriter.Write(chromeDriver.PageSource);
        streamWriter.Flush();
      }
    }

    protected void SwitchToFrame(By by) => chromeDriver.SwitchTo().Frame(WaitUntil(by, ElementsExists, true).First());

    protected ReadOnlyCollection<IWebElement> FindElements(By by) => chromeDriver.FindElements(by);

    protected FrameSwitch FrameSwitch(IWebElement webElement) => new FrameSwitch(chromeDriver, webElement);

    #region WaitUntil

    #region Func

    protected bool ElementsExists(ReadOnlyCollection<IWebElement> webElements) => webElements?.Count > 0;

    //protected bool ElementsNotExists(ReadOnlyCollection<IWebElement> webElements) => !ElementsExists(webElements);

    protected bool AllElementsVisible(ReadOnlyCollection<IWebElement> webElements) => webElements?.All(x => x.Displayed) == true;

    protected bool AnyElementsVisible(ReadOnlyCollection<IWebElement> webElements) => webElements?.Any(x => x.Displayed) == true;

    protected bool AllElementsClickable(ReadOnlyCollection<IWebElement> webElements) => webElements?.All(x => x.Displayed && x.Enabled) == true;

    protected bool AnyElementsClickable(ReadOnlyCollection<IWebElement> webElements) => webElements?.Any(x => x.Displayed && x.Enabled) == true;

    protected bool AllElementsSelected(ReadOnlyCollection<IWebElement> webElements) => webElements?.All(x => x.Selected) == true;

    protected bool AnyElementsSelected(ReadOnlyCollection<IWebElement> webElements) => webElements?.Any(x => x.Selected) == true;

    #endregion Func

    protected ReadOnlyCollection<IWebElement> WaitUntil(By by, Func<ReadOnlyCollection<IWebElement>, bool> func, bool isThrow = true, int delay = 500, int timeout = 10000)
    => WaitUntil_(chromeDriver, by, func, isThrow, delay, timeout);

    protected ReadOnlyCollection<IWebElement> WaitUntil(IWebElement webElement, By by, Func<ReadOnlyCollection<IWebElement>, bool> func, bool isThrow = true, int delay = 500, int timeout = 10000)
    => WaitUntil_(webElement, by, func, isThrow, delay, timeout);

    private ReadOnlyCollection<IWebElement> WaitUntil_(ISearchContext searchContext, By by, Func<ReadOnlyCollection<IWebElement>, bool> func,
      bool isThrow = true, int delay = 200, int timeout = 10000)
    {
      using CancellationTokenSource timeoutToken = new CancellationTokenSource(timeout);
      while (!timeoutToken.IsCancellationRequested)
      {
        var eles = searchContext.FindElements(by);
        if (func(eles)) return eles;
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

    private const string JsDropFile = @"var target = arguments[0],
offsetX = arguments[1],
offsetY = arguments[2],
document = target.ownerDocument || document,
window = document.defaultView || window;

var input = document.createElement('INPUT');
input.type = 'file';
input.style.display = 'none';
input.onchange = function () {
  var rect = target.getBoundingClientRect(),
    x = rect.left + (offsetX || (rect.width >> 1)),
    y = rect.top + (offsetY || (rect.height >> 1)),
    dataTransfer = { files: this.files };

  ['dragenter', 'dragover', 'drop'].forEach(function (name) {
    var evt = document.createEvent('MouseEvent');
    evt.initMouseEvent(name, !0, !0, window, 0, 0, 0, x, y, !1, !1, !1, !1, 0, null);
    evt.dataTransfer = dataTransfer;
    target.dispatchEvent(evt);
  });
  setTimeout(function () { document.body.removeChild(input); }, 25);
}
document.body.appendChild(input);
return input;";

    protected void DropFile(string file, IWebElement webElement, int offsetX, int offsetY)
    {
      IWebElement input = (IWebElement)chromeDriver.ExecuteScript(JsDropFile, webElement, offsetX, offsetY);
      input.SendKeys(file);
    }

    #endregion JSDropFile

    #region JsClick

    protected void JsDoubleClick(IWebElement webElement) => chromeDriver.ExecuteScript(@"var evt = document.createEvent('MouseEvents');
evt.initMouseEvent('dblclick',true, true, window, 0, 0, 0, 0, 0, false, false, false, false, 0,null);
arguments[0].dispatchEvent(evt);", webElement);

    protected void JsClick(IWebElement webElement) => chromeDriver.ExecuteScript("arguments[0].click();", webElement);

    #endregion JsClick

    protected void JsScrollIntoView(IWebElement webElement) => chromeDriver.ExecuteScript("arguments[0].scrollIntoView();", webElement);

    protected void JsSetInputText(IWebElement webElement, string text) => chromeDriver.ExecuteScript($"arguments[0].value = \"{text}\";", webElement);
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