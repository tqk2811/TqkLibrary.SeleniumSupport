using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using TqkLibrary.SeleniumSupport.Exceptions;

namespace TqkLibrary.SeleniumSupport
{
    /// <summary>
    /// 
    /// </summary>
    public static class SeleniumHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="readOnlyCollection"></param>
        /// <param name="throwText"></param>
        /// <returns></returns>
        /// <exception cref="ChromeAutoException"></exception>
        public static ReadOnlyCollection<IWebElement> ThrowIfNull(this ReadOnlyCollection<IWebElement> readOnlyCollection, string throwText)
        {
            if (null == readOnlyCollection) throw new ChromeAutoException(throwText);
            return readOnlyCollection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="readOnlyCollection"></param>
        /// <param name="throwText"></param>
        /// <returns></returns>
        /// <exception cref="ChromeAutoException"></exception>
        public static ReadOnlyCollection<IWebElement> ThrowIfNullOrCountZero(this ReadOnlyCollection<IWebElement> readOnlyCollection, string throwText)
        {
            if (null == readOnlyCollection || readOnlyCollection.Count == 0) throw new ChromeAutoException(throwText);
            return readOnlyCollection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chromeOptions"></param>
        /// <param name="userDataDir"></param>
        /// <param name="profileDirectory"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ChromeOptions AddUserDataDir(this ChromeOptions chromeOptions, string userDataDir, string? profileDirectory = null)
        {
            if (string.IsNullOrEmpty(userDataDir)) throw new ArgumentNullException(nameof(userDataDir));
            chromeOptions.AddArgument($"--user-data-dir={userDataDir}");
            if (!string.IsNullOrWhiteSpace(profileDirectory)) 
                chromeOptions.AddArgument($"--profile-directory=\"{profileDirectory}\"");
            return chromeOptions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chromeOptions"></param>
        /// <param name="UA"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ChromeOptions AddUserAgent(this ChromeOptions chromeOptions, string UA)
        {
            if (string.IsNullOrEmpty(UA)) throw new ArgumentNullException(nameof(UA));
            chromeOptions.AddArgument("--user-agent=" + UA);
            return chromeOptions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chromeOptions"></param>
        /// <param name="proxy"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ChromeOptions AddProxy(this ChromeOptions chromeOptions, string proxy)
        {
            if (string.IsNullOrEmpty(proxy)) throw new ArgumentNullException(nameof(proxy));
            chromeOptions.AddArguments("--proxy-server=" + string.Format("http://{0}", proxy));
            return chromeOptions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chromeOptions"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ChromeOptions AddProxy(this ChromeOptions chromeOptions, string host, int port)
        {
            if (string.IsNullOrEmpty(host)) throw new ArgumentNullException(nameof(host));
            chromeOptions.AddArguments("--proxy-server=" + string.Format("http://{0}:{1}", host, port));
            return chromeOptions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chromeOptions"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ChromeOptions AppMode(this ChromeOptions chromeOptions, string url)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
            chromeOptions.AddArguments("--app=" + url);
            return chromeOptions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driver"></param>
        public static void RemoveNavigatorWebdriver(this ChromeDriver driver)
        {
            driver.ExecuteScript("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chromeOptions"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static ChromeOptions ForceDeviceScaleFactor(this ChromeOptions chromeOptions, double n)
        {
            chromeOptions.AddArgument($"--force-device-scale-factor={n}");
            return chromeOptions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string? OpenNewTab(this ChromeDriver driver, string url)
        {
            if (driver is null) throw new ArgumentNullException(nameof(driver));
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));
            IEnumerable<string> windows = driver.WindowHandles.ToList();
            driver.ExecuteScript($"window.open(arguments[0], '_blank');", url);
            IEnumerable<string> newWindows = driver.WindowHandles.ToList();
            return newWindows.Except(windows).FirstOrDefault();
        }
    }
}