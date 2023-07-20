﻿using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TqkLibrary.SeleniumSupport.Helper.WaitHeplers
{
    /// <summary>
    /// 
    /// </summary>
    public class WaitElementBuilder : BaseWaitBuilder
    {
        readonly By _by;
        readonly ISearchContext _searchContext;
        internal WaitElementBuilder(
            WaitHelper waitHepler,
            By by,
            ISearchContext searchContext
            ) : base(
                waitHepler
                )
        {
            this._by = by ?? throw new ArgumentNullException(nameof(by));
            this._searchContext = searchContext ?? throw new ArgumentNullException(nameof(searchContext));
        }

        Func<ReadOnlyCollection<IWebElement>, bool> funcCheck = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="funcCheck"></param>
        /// <returns></returns>
        public WaitElementBuilder Until(Func<ReadOnlyCollection<IWebElement>, bool> funcCheck)
        {
            this.funcCheck = funcCheck;
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitElementUntilBuilder Until() => new WaitElementUntilBuilder(this);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ChromeAutoException"></exception>
        public async Task<ReadOnlyCollection<IWebElement>> StartAsync()
        {
            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(GetTimeout);
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                var workAsync = GetWorkAsync;
                if (workAsync is not null)
                {
                    Task task = workAsync.Invoke();
                    if (task is not null)
                    {
                        await task;
                    }
                }
                var eles = _searchContext.FindElements(_by);
                if (funcCheck(eles)) return eles;
                await Task.Delay(this._waitHepler.Delay, this._waitHepler._cancellationToken).ConfigureAwait(false);
            }
            if (_IsThrow) throw new ChromeAutoException(_by.ToString());
            return new ReadOnlyCollection<IWebElement>(new List<IWebElement>());
        }
    }
}