using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Threading;
using TqkLibrary.SeleniumSupport.Exceptions;

namespace TqkLibrary.SeleniumSupport.Helper.WaitHeplers
{
    /// <summary>
    /// 
    /// </summary>
    public class WaitUrlBuilder : BaseWaitBuilder
    {
        readonly Func<string, bool> _checkCallback;
        internal WaitUrlBuilder(
            WaitHelper waitHepler,
            Func<string, bool> checkCallback
            )
            : base(
                  waitHepler
                  )
        {
            this._checkCallback = checkCallback ?? throw new ArgumentNullException(nameof(checkCallback));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ChromeAutoException"></exception>
        public async Task<bool> StartAsync()
        {
            _waitHepler.WriteLog($"WaitUntilUrl");
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
                if (_checkCallback(_waitHepler._webDriver.Url))
                {
                    _waitHepler.WriteLog($"WaitUntilUrl found: {_waitHepler._webDriver.Url}");
                    return true;
                }
                await Task.Delay(this._waitHepler.Delay, this._waitHepler.CancellationToken).ConfigureAwait(false);
            }
            if (_IsThrow) throw new ChromeAutoException($"Wait Url failed, current url: {_waitHepler._webDriver.Url}");
            return false;
        }
    }
}
