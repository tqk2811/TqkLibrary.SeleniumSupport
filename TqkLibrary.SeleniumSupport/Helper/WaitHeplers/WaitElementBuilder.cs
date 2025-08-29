using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TqkLibrary.SeleniumSupport.Exceptions;

namespace TqkLibrary.SeleniumSupport.Helper.WaitHeplers
{
    /// <summary>
    /// 
    /// </summary>
    public class WaitElementBuilder : BaseWaitBuilder
    {
        readonly By _by;
        readonly ISearchContext _searchContext;
        bool _isExpectedNotExist = false;
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

        List<Func<IEnumerable<IWebElement>, IEnumerable<IWebElement>>> _funcFilters = new List<Func<IEnumerable<IWebElement>, IEnumerable<IWebElement>>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isExpectedExist"></param>
        /// <returns></returns>
        public WaitElementBuilder Expected(bool isExpectedExist)
        {
            _isExpectedNotExist = !isExpectedExist;
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitElementBuilder ExpectedNotExist()
        {
            _isExpectedNotExist = true;
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitElementBuilder ExpectedExist()
        {
            _isExpectedNotExist = false;
            return this;
        }

        WaitElementBuilder _Until(params Func<IEnumerable<IWebElement>, IEnumerable<IWebElement>>[] funcFilters)
        {
            if (funcFilters is null || funcFilters.Length == 0 || funcFilters.Any(x => x is null))
                throw new ArgumentNullException(nameof(funcFilters));

            this._funcFilters.AddRange(funcFilters);
            return this;
        }
        WaitElementBuilder _Until(params Func<IEnumerable<IWebElement>, bool>[] funcFilters)
        {
            if (funcFilters is null || funcFilters.Length == 0 || funcFilters.Any(x => x is null))
                throw new ArgumentNullException(nameof(funcFilters));

            this._funcFilters.AddRange(funcFilters
                .Select<Func<IEnumerable<IWebElement>, bool>, Func<IEnumerable<IWebElement>, IEnumerable<IWebElement>>>(
                    x => (eles) => x.Invoke(eles) ? eles : Enumerable.Empty<IWebElement>()
                    )
                );
            return this;
        }
        WaitElementUntilBuilder _Until() => new WaitElementUntilBuilder(this);




        /// <summary>
        /// 
        /// </summary>
        /// <param name="funcFilters"></param>
        /// <returns></returns>
        public WaitElementBuilder Until(params Func<IEnumerable<IWebElement>, IEnumerable<IWebElement>>[] funcFilters)
        {
            _isExpectedNotExist = false;
            return _Until(funcFilters);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="funcFilters"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitElementBuilder Until(params Func<IEnumerable<IWebElement>, bool>[] funcFilters)
        {
            _isExpectedNotExist = false;
            return _Until(funcFilters);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitElementUntilBuilder Until()
        {
            _isExpectedNotExist = false;
            return _Until();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="funcFilters"></param>
        /// <returns></returns>
        public WaitElementBuilder UntilNotExist(params Func<IEnumerable<IWebElement>, IEnumerable<IWebElement>>[] funcFilters)
        {
            _isExpectedNotExist = true;
            return _Until(funcFilters);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="funcFilters"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitElementBuilder UntilNotExist(params Func<IEnumerable<IWebElement>, bool>[] funcFilters)
        {
            _isExpectedNotExist = true;
            return _Until(funcFilters);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitElementUntilBuilder UntilNotExist()
        {
            _isExpectedNotExist = true;
            return _Until();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ChromeAutoException"></exception>
        public async Task<ReadOnlyCollection<IWebElement>> StartAsync()
        {
            if (_funcFilters.Count == 0) throw new InvalidOperationException($"Must call {nameof(Until)} function first");

            _waitHepler.WriteLog($"WaitUntilElements {_by}");
            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(GetTimeout);
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                this._waitHepler.CancellationToken.ThrowIfCancellationRequested();
                var workAsync = GetWorkAsync;
                if (workAsync is not null)
                {
                    Task task = workAsync.Invoke();
                    if (task is not null)
                    {
                        await task;
                    }
                }
                ReadOnlyCollection<IWebElement> eles = _searchContext.FindElements(_by);

                IList<IWebElement> filtered = eles;
                foreach (Func<IEnumerable<IWebElement>, IEnumerable<IWebElement>> funcFilter in _funcFilters)
                {
                    filtered = funcFilter(filtered).ToList();
                }
                if (_isExpectedNotExist)
                {
                    if (!filtered.Any())
                    {
                        _waitHepler.WriteLog($"WaitUntilElements {_by}, disapped");
                        return new ReadOnlyCollection<IWebElement>(filtered);
                    }
                }
                else
                {
                    if (filtered.Any())
                    {
                        _waitHepler.WriteLog($"WaitUntilElements {_by}, founds {eles.Count}");
                        return new ReadOnlyCollection<IWebElement>(filtered);
                    }
                }
                await Task.Delay(this._waitHepler.Delay, this._waitHepler.CancellationToken).ConfigureAwait(false);
            }
            if (_IsThrow) throw new ChromeAutoException(_by.ToString());
            return new ReadOnlyCollection<IWebElement>(new List<IWebElement>());
        }
    }
}
