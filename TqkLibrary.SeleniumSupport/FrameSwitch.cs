using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;

namespace TqkLibrary.SeleniumSupport
{
    /// <summary>
    /// 
    /// </summary>
    public class FrameSwitch : IDisposable
    {
        private readonly ChromeDriver chromeDriver;

        internal FrameSwitch(ChromeDriver chromeDriver, IWebElement webElement)
        {
            this.chromeDriver = chromeDriver ?? throw new ArgumentNullException(nameof(chromeDriver));
            chromeDriver.SwitchTo().Frame(webElement ?? throw new ArgumentNullException(nameof(webElement)));
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            chromeDriver.SwitchTo().ParentFrame();
        }
    }
}