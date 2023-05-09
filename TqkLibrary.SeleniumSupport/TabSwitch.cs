﻿using OpenQA.Selenium.Chrome;
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
        private readonly ChromeDriver chromeDriver;
        /// <summary>
        /// 
        /// </summary>
        public bool IsCloseTab { get; set; } = false;
        /// <summary>
        /// 
        /// </summary>
        public string OldWindowHandle { get; }
        /// <summary>
        /// 
        /// </summary>
        public string NewWindowHandle { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="chromeDriver"></param>
        /// <param name="url"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public TabSwitch(ChromeDriver chromeDriver, string url)
        {
            if (chromeDriver is null) throw new ArgumentNullException(nameof(chromeDriver));
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));

            this.chromeDriver = chromeDriver;
            OldWindowHandle = chromeDriver.CurrentWindowHandle;
            IEnumerable<string> handles = chromeDriver.WindowHandles.ToList();
            this.chromeDriver.ExecuteScript($"open('{url}')");
            NewWindowHandle = chromeDriver.WindowHandles.Except(handles).FirstOrDefault();
            this.chromeDriver.SwitchTo().Window(NewWindowHandle);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (IsCloseTab)
            {
                chromeDriver.ExecuteScript("window.close();");
            }
            this.chromeDriver.SwitchTo().Window(OldWindowHandle);
        }
    }
}