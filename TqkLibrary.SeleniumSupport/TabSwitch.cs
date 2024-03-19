using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

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
        public string? OldWindowHandle { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public string? NewWindowHandle { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="webDriver"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public TabSwitch(WebDriver webDriver)
        {
            this._webDriver = webDriver ?? throw new ArgumentNullException(nameof(webDriver));
            OldWindowHandle = webDriver.CurrentWindowHandle;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="webDriver"></param>
        /// <param name="url"></param>
        /// <param name="isCloseTab"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static TabSwitch FromUrl(WebDriver webDriver, string url, bool isCloseTab = true)
        {
            if (webDriver is null) throw new ArgumentNullException(nameof(webDriver));
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));

            TabSwitch tabSwitch = new TabSwitch(webDriver)
            {
                IsCloseTab = isCloseTab,
            };

            IEnumerable<string> handles = webDriver.WindowHandles.ToList();
            webDriver.ExecuteScript($"open(arguments[0])", url);
            tabSwitch.NewWindowHandle = webDriver.WindowHandles.Except(handles).FirstOrDefault();
            webDriver.SwitchTo().Window(tabSwitch.NewWindowHandle);

            return tabSwitch;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="webDriver"></param>
        /// <param name="tabId"></param>
        /// <param name="isCloseTab"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static TabSwitch FromExistTab(WebDriver webDriver, string tabId, bool isCloseTab = true)
        {
            if (webDriver is null) throw new ArgumentNullException(nameof(webDriver));
            if (string.IsNullOrWhiteSpace(tabId)) throw new ArgumentNullException(nameof(tabId));

            if (!webDriver.WindowHandles.Contains(tabId))
                throw new InvalidOperationException($"'{tabId}' is invalid handle");
            TabSwitch tabSwitch = new TabSwitch(webDriver)
            {
                IsCloseTab = isCloseTab,
            };
            tabSwitch.NewWindowHandle = tabId;
            webDriver.SwitchTo().Window(tabSwitch.NewWindowHandle);
            return tabSwitch;
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
            if (!string.IsNullOrWhiteSpace(OldWindowHandle) && !this._webDriver.CurrentWindowHandle.Equals(OldWindowHandle))
            {
                this._webDriver.SwitchTo().Window(OldWindowHandle);
            }
        }
    }
}