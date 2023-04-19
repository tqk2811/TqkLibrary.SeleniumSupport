using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TqkLibrary.SeleniumSupport
{
    /// <summary>
    /// 
    /// </summary>
    public class WaitElementHepler
    {
        /// <summary>
        /// 
        /// </summary>
        public int Delay { get; set; } = 500;
        /// <summary>
        /// When timeout is less than or equal zero
        /// </summary>
        public int DefaultTimeout { get; set; } = 30000;
        readonly ChromeDriver chromeDriver;
        readonly CancellationToken cancellationToken;

        /// <summary>
        /// 
        /// </summary>
        public WaitElementHepler(BaseChromeProfile baseChromeProfile, CancellationToken cancellationToken) : this(baseChromeProfile.chromeDriver, cancellationToken)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        public WaitElementHepler(ChromeDriver chromeDriver, CancellationToken cancellationToken)
        {
            this.chromeDriver = chromeDriver ?? throw new ArgumentNullException(nameof(chromeDriver));
            this.cancellationToken = cancellationToken;
        }



        /// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool WaitUntilUrl(Func<string, bool> func, bool isThrow = true, int timeout = 0)
        {
            using CancellationTokenSource timeoutToken = new CancellationTokenSource(timeout <= 0 ? DefaultTimeout : timeout);
            while (!timeoutToken.IsCancellationRequested)
            {
                if (func(chromeDriver.Url)) return true;
                Task.Delay(Delay, cancellationToken).Wait();
            }
            if (isThrow) throw new ChromeAutoException($"WaitUntilUrl failed");
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ChromeAutoException"></exception>
        public async Task<bool> WaitUntilUrlAsync(Func<string, bool> func, bool isThrow = true, int timeout = 0)
        {
            using CancellationTokenSource timeoutToken = new CancellationTokenSource(timeout <= 0 ? DefaultTimeout : timeout);
            while (!timeoutToken.IsCancellationRequested)
            {
                if (func(chromeDriver.Url)) return true;
                await Task.Delay(Delay, cancellationToken);
            }
            if (isThrow) throw new ChromeAutoException($"WaitUntilUrl failed");
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<IWebElement> WaitUntil(By by, Func<ReadOnlyCollection<IWebElement>, bool> func, bool isThrow = true, int timeout = 0)
            => WaitUntil_(chromeDriver, by, func, isThrow, timeout);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<IWebElement> WaitUntil(string cssSelector, Func<ReadOnlyCollection<IWebElement>, bool> func, bool isThrow = true, int timeout = 0)
            => WaitUntil_(chromeDriver, By.CssSelector(cssSelector), func, isThrow, timeout);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<IWebElement> WaitUntil(IWebElement webElement, By by, Func<ReadOnlyCollection<IWebElement>, bool> func, bool isThrow = true, int timeout = 0)
            => WaitUntil_(webElement, by, func, isThrow, timeout);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<IWebElement> WaitUntil(IWebElement webElement, string cssSelector, Func<ReadOnlyCollection<IWebElement>, bool> func, bool isThrow = true, int timeout = 0)
            => WaitUntil_(webElement, By.CssSelector(cssSelector), func, isThrow, timeout);
        private ReadOnlyCollection<IWebElement> WaitUntil_(ISearchContext searchContext, By by, Func<ReadOnlyCollection<IWebElement>, bool> func,
          bool isThrow = true, int delay = 200, int timeout = 0)
        {
            using CancellationTokenSource timeoutToken = new CancellationTokenSource(timeout <= 0 ? DefaultTimeout : timeout);
            while (!timeoutToken.IsCancellationRequested)
            {
                var eles = searchContext.FindElements(by);
                try { if (func(eles)) return eles; } catch { }
                Task.Delay(delay, cancellationToken).Wait();
            }
            if (isThrow) throw new ChromeAutoException(by.ToString());
            return new ReadOnlyCollection<IWebElement>(new List<IWebElement>());
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<ReadOnlyCollection<IWebElement>> WaitUntilAsync(By by, Func<ReadOnlyCollection<IWebElement>, bool> func, bool isThrow = true, int timeout = 0)
            => WaitUntilAsync_(chromeDriver, by, func, isThrow, timeout);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<ReadOnlyCollection<IWebElement>> WaitUntilAsync(string cssSelector, Func<ReadOnlyCollection<IWebElement>, bool> func, bool isThrow = true, int timeout = 0)
            => WaitUntilAsync_(chromeDriver, By.CssSelector(cssSelector), func, isThrow, timeout);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<ReadOnlyCollection<IWebElement>> WaitUntilAsync(IWebElement webElement, By by, Func<ReadOnlyCollection<IWebElement>, bool> func, bool isThrow = true, int timeout = 0)
            => WaitUntilAsync_(webElement, by, func, isThrow, timeout);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<ReadOnlyCollection<IWebElement>> WaitUntilAsync(IWebElement webElement, string cssSelector, Func<ReadOnlyCollection<IWebElement>, bool> func, bool isThrow = true, int timeout = 0)
            => WaitUntilAsync_(webElement, By.CssSelector(cssSelector), func, isThrow, timeout);

        private async Task<ReadOnlyCollection<IWebElement>> WaitUntilAsync_(ISearchContext searchContext, By by, Func<ReadOnlyCollection<IWebElement>, bool> func,
          bool isThrow = true, int timeout = 0)
        {
            using CancellationTokenSource timeoutToken = new CancellationTokenSource(timeout <= 0 ? DefaultTimeout : timeout);
            while (!timeoutToken.IsCancellationRequested)
            {
                var eles = searchContext.FindElements(by);
                try { if (func(eles)) return eles; } catch { }
                await Task.Delay(Delay, cancellationToken).ConfigureAwait(false);
            }
            if (isThrow) throw new ChromeAutoException(by.ToString());
            return new ReadOnlyCollection<IWebElement>(new List<IWebElement>());
        }
    }
}
