using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TqkLibrary.SeleniumSupport
{
    /// <summary>
    /// 
    /// </summary>
    public class TabSwitch : IDisposable
    {
        private readonly WebDriver _webDriver;
        /// <summary>
        /// 
        /// </summary>
        public bool IsCloseTab { get; set; } = true;
        /// <summary>
        /// 
        /// </summary>
        public string OldWindowHandle { get; }
        /// <summary>
        /// 
        /// </summary>
        public string? NewWindowHandle { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="webDriver"></param>
        /// <param name="url"></param>
        /// <param name="isCloseTab"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public TabSwitch(WebDriver webDriver, string url, bool isCloseTab = true)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));

            this._webDriver = webDriver ?? throw new ArgumentNullException(nameof(webDriver));
            OldWindowHandle = webDriver.CurrentWindowHandle;
            IEnumerable<string> handles = webDriver.WindowHandles.ToList();
            this._webDriver.ExecuteScript($"open(arguments[0])", url);
            NewWindowHandle = webDriver.WindowHandles.Except(handles).FirstOrDefault();
            this._webDriver.SwitchTo().Window(NewWindowHandle);
            this.IsCloseTab = isCloseTab;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (IsCloseTab)
            {
                _webDriver.ExecuteScript("window.close();");
            }
            this._webDriver.SwitchTo().Window(OldWindowHandle);
        }
    }
}