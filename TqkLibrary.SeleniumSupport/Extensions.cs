using OpenQA.Selenium;
using System;
using System.Collections.Generic;
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
        public static async Task SendKeysAsync(this Task<IWebElement> webElement,string text) => (await webElement).SendKeys(text);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static async Task<string> GetAttributeAsync(this Task<IWebElement> webElement,string attributeName) => (await webElement).GetAttribute(attributeName);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        [Obsolete]
        public static async Task<string> GetPropertyAsync(this Task<IWebElement> webElement,string attributeName) => (await webElement).GetProperty(attributeName);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <returns></returns>
        public static async Task SubmitAsync(this Task<IWebElement> webElement) => (await webElement).Submit();
    }
}
