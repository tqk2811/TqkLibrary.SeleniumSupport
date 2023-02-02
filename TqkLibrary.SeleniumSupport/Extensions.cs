using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.SeleniumSupport
{
    /// <summary>
    /// 
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElements"></param>
        /// <returns></returns>
        public static async Task<IWebElement> FirstAsync(this Task<ReadOnlyCollection<IWebElement>> webElements)
            => (await webElements)?.First();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElements"></param>
        /// <returns></returns>
        public static async Task<IWebElement> FirstOrDefaultAsync(this Task<ReadOnlyCollection<IWebElement>> webElements)
            => (await webElements)?.FirstOrDefault();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElements"></param>
        /// <returns></returns>
        public static async Task<IWebElement> LastAsync(this Task<ReadOnlyCollection<IWebElement>> webElements)
            => (await webElements)?.Last();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElements"></param>
        /// <returns></returns>
        public static async Task<IWebElement> LastOrDefaultAsync(this Task<ReadOnlyCollection<IWebElement>> webElements)
            => (await webElements)?.LastOrDefault();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElements"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static async Task<bool> AnyAsync(this Task<ReadOnlyCollection<IWebElement>> webElements, Func<IWebElement, bool> predicate)
            => (await webElements).Any(predicate);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElements"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static async Task<bool> AllAsync(this Task<ReadOnlyCollection<IWebElement>> webElements, Func<IWebElement, bool> predicate)
            => (await webElements).All(predicate);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElements"></param>
        /// <returns></returns>
        public static async Task<int> CountAsync(this Task<ReadOnlyCollection<IWebElement>> webElements)
            => (await webElements).Count();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElements"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static async Task<int> CountAsync(this Task<ReadOnlyCollection<IWebElement>> webElements, Func<IWebElement, bool> predicate)
            => (await webElements).Count(predicate);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <returns></returns>
        public static async Task ClickAsync(this Task<IWebElement> webElement) => (await webElement).Click();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <returns></returns>
        public static async Task ClearAsync(this Task<IWebElement> webElement) => (await webElement).Clear();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static async Task SendKeysAsync(this Task<IWebElement> webElement, string text) => (await webElement).SendKeys(text);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static async Task<string> GetAttributeAsync(this Task<IWebElement> webElement, string attributeName) => (await webElement).GetAttribute(attributeName);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static async Task<string> GetDomPropertyAsync(this Task<IWebElement> webElement, string attributeName) => (await webElement).GetDomProperty(attributeName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static async Task<string> GetDomAttributeAsync(this Task<IWebElement> webElement, string attributeName) => (await webElement).GetDomAttribute(attributeName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <returns></returns>
        public static async Task SubmitAsync(this Task<IWebElement> webElement) => (await webElement).Submit();
    }
}
