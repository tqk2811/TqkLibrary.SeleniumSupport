using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;

namespace TqkLibrary.SeleniumSupport
{
  public class FrameSwitch : IDisposable
  {
    private readonly ChromeDriver chromeDriver;

    internal FrameSwitch(ChromeDriver chromeDriver, IWebElement webElement)
    {
      this.chromeDriver = chromeDriver;
      chromeDriver.SwitchTo().Frame(webElement ?? throw new ArgumentNullException(nameof(webElement)));
    }

    public void Dispose()
    {
      chromeDriver.SwitchTo().ParentFrame();
    }
  }
}