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
        private readonly IWebDriver _webDriver;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        public FrameSwitch(IWebElement webElement) : this(webElement.GetWebDriver(), webElement)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webDriver"></param>
        /// <param name="webElement"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public FrameSwitch(IWebDriver webDriver, IWebElement webElement)
        {
            this._webDriver = webDriver ?? throw new ArgumentNullException(nameof(webDriver));
            webDriver.SwitchTo().Frame(webElement ?? throw new ArgumentNullException(nameof(webElement)));
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _webDriver.SwitchTo().ParentFrame();
        }
    }
}