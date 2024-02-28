using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TqkLibrary.SeleniumSupport.Helper.WaitHeplers
{
    /// <summary>
    /// 
    /// </summary>
    public class WaitHelper
    {
        /// <summary>
        /// 
        /// </summary>
        public int Delay { get; set; } = 500;
        /// <summary>
        /// When timeout is less than or equal zero
        /// </summary>
        public int DefaultTimeout { get; set; } = 30000;
        internal readonly IWebDriver _webDriver;
        internal readonly CancellationToken _cancellationToken;
        /// <summary>
        /// 
        /// </summary>
        public event Action<string>? OnLogReceived;

        internal void WriteLog(string log)
        {
            OnLogReceived?.Invoke(log);
        }

        /// <summary>
        /// 
        /// </summary>
        public WaitHelper(BaseChromeProfile baseChromeProfile, CancellationToken cancellationToken) : this(baseChromeProfile.chromeDriver!, cancellationToken)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        public WaitHelper(IWebDriver webDriver, CancellationToken cancellationToken)
        {
            _webDriver = webDriver ?? throw new ArgumentNullException(nameof(webDriver));
            this._cancellationToken = cancellationToken;
        }

        internal Func<Task>? _WorkAsync = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workAsync"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitHelper Do(Func<Task> workAsync)
        {
            this._WorkAsync = workAsync ?? throw new ArgumentNullException(nameof(workAsync));
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="work"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitHelper Do(Action work)
        {
            if (work is null) throw new ArgumentNullException(nameof(work));
            this._WorkAsync = () =>
            {
                work.Invoke();
                return Task.CompletedTask;
            };
            return this;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cssSelector"></param>
        /// <returns></returns>
        public WaitElementBuilder WaitUntilElements(string cssSelector)
        {
            return new WaitElementBuilder(this, By.CssSelector(cssSelector), _webDriver);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="by"></param>
        /// <returns></returns>
        public WaitElementBuilder WaitUntilElements(By by)
        {
            return new WaitElementBuilder(this, by, _webDriver);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cssSelector"></param>
        /// <param name="parentWebElement"></param>
        /// <returns></returns>
        public WaitElementBuilder WaitUntilElements(IWebElement parentWebElement, string cssSelector)
        {
            return new WaitElementBuilder(this, By.CssSelector(cssSelector), parentWebElement);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="by"></param>
        /// <param name="parentWebElement"></param>
        /// <returns></returns>
        public WaitElementBuilder WaitUntilElements(IWebElement parentWebElement, By by)
        {
            return new WaitElementBuilder(this, by, parentWebElement);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="checkCallback"></param>
        /// <returns></returns>
        public WaitUrlBuilder WaitUntilUrl(Func<string, bool> checkCallback)
        {
            return new WaitUrlBuilder(this, checkCallback);
        }
    }
}
