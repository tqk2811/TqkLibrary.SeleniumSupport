using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.SeleniumSupport.Helper.WaitHeplers
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class BaseWaitBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        protected readonly WaitHelper _waitHepler;
        internal BaseWaitBuilder(
            WaitHelper waitHepler
            )
        {
            this._waitHepler = waitHepler ?? throw new ArgumentNullException(nameof(waitHepler));
        }

        internal int? _Timeout = null;
        internal Func<Task> _WorkAsync = null;
        internal bool _IsThrow = false;

        internal int GetTimeout { get { return _Timeout.HasValue ? _Timeout.Value : _waitHepler.DefaultTimeout; } }
        internal Func<Task> GetWorkAsync { get { return _WorkAsync ?? _waitHepler._WorkAsync; } }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class BaseWaitBuilderExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static T WithTimeout<T>(this T t, int? timeout) where T : BaseWaitBuilder
        {
            t._Timeout = timeout;
            return t;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="workAsync"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T Do<T>(this T t, Func<Task> workAsync) where T : BaseWaitBuilder
        {
            t._WorkAsync = workAsync ?? throw new ArgumentNullException(nameof(workAsync));
            return t;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T Do<T>(this T t, Action work) where T : BaseWaitBuilder
        {
            if (work is null) throw new ArgumentNullException(nameof(work));
            t._WorkAsync = () =>
            {
                work.Invoke();
                return Task.CompletedTask;
            };
            return t;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="isThrow"></param>
        /// <returns></returns>
        public static T WithThrow<T>(this T t, bool isThrow = true) where T : BaseWaitBuilder
        {
            t._IsThrow = isThrow;
            return t;
        }
    }
}
