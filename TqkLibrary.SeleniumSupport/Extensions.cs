using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
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
        /// <param name="webElement"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static WebDriver GetWebDriver(this IWebElement webElement)
        {
            if (webElement is null) throw new ArgumentNullException(nameof(webElement));
            IWrapsDriver wrapsDriver = (IWrapsDriver)webElement;
            return (WebDriver)wrapsDriver.WrappedDriver;
        }

        #region JSDropFile
        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <param name="file"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        public static void JsDropFile(this IWebElement webElement, string file, int offsetX, int offsetY)
        {
            IWebElement input = (IWebElement)webElement.GetWebDriver().ExecuteScript(Resource.JsDropFile, webElement, offsetX, offsetY);
            input.SendKeys(file);
        }

        #endregion JSDropFile

        #region JsClick
        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        public static void JsDoubleClick(this IWebElement webElement)
        {
            webElement.GetWebDriver().ExecuteScript(@"var evt = document.createEvent('MouseEvents');
evt.initMouseEvent('dblclick',true, true, window, 0, 0, 0, 0, 0, false, false, false, false, 0,null);
arguments[0].dispatchEvent(evt);", webElement);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        public static void JsClick(this IWebElement webElement)
        {
            webElement.GetWebDriver().ExecuteScript("arguments[0].click();", webElement);
        }

        #endregion JsClick


        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        public static void JsScrollIntoView(this IWebElement webElement)
        {
            webElement.GetWebDriver().ExecuteScript("arguments[0].scrollIntoView();", webElement);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <param name="text"></param>
        public static void JsSetInputText(this IWebElement webElement, string text)
        {
            webElement.GetWebDriver().ExecuteScript($"arguments[0].value = \"{text}\";", webElement);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <returns></returns>
        public static byte[]? JsScreenshot_html2canvas(this IWebElement webElement)
        {
            IJavaScriptExecutor javaScriptExecutor = webElement.GetWebDriver();
            javaScriptExecutor.ExecuteScript(Resource.html2canvas_min);
            string? res = javaScriptExecutor.ExecuteAsyncScript("html2canvas(arguments[0]).then(canvas => { arguments[1](canvas.toDataURL('image/png')); })", webElement) as string;
            if (string.IsNullOrWhiteSpace(res))
            {
                return null;
            }
            else
            {
                return Convert.FromBase64String(res!.Substring("data:image/png;base64,".Length));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webDriver"></param>
        /// <param name="url"></param>
        /// <param name="isCloseTab"></param>
        /// <returns></returns>
        public static TabSwitch TabSwitch(this WebDriver webDriver, string url, bool isCloseTab = true)
        {
            return new TabSwitch(webDriver, url, isCloseTab);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <returns></returns>
        public static FrameSwitch FrameSwitch(this IWebElement webElement)
        {
            return new FrameSwitch(webElement.GetWebDriver(), webElement);
        }

        #region LinQ Async

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElements"></param>
        /// <returns></returns>
        public static async Task<IWebElement> FirstAsync(this Task<ReadOnlyCollection<IWebElement>> webElements)
            => (await webElements).First();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElements"></param>
        /// <returns></returns>
        public static async Task<IWebElement?> FirstOrDefaultAsync(this Task<ReadOnlyCollection<IWebElement>> webElements)
            => (await webElements)?.FirstOrDefault();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElements"></param>
        /// <returns></returns>
        public static async Task<IWebElement> LastAsync(this Task<ReadOnlyCollection<IWebElement>> webElements)
            => (await webElements).Last();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElements"></param>
        /// <returns></returns>
        public static async Task<IWebElement?> LastOrDefaultAsync(this Task<ReadOnlyCollection<IWebElement>> webElements)
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

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <returns></returns>
        public static async Task ClickAsync(this Task<IWebElement> webElement)
            => (await webElement).Click();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <returns></returns>
        public static async Task ClearAsync(this Task<IWebElement> webElement)
            => (await webElement).Clear();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static async Task SendKeysAsync(this Task<IWebElement> webElement, string text)
            => (await webElement).SendKeys(text);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static async Task<string> GetAttributeAsync(this Task<IWebElement> webElement, string attributeName)
            => (await webElement).GetAttribute(attributeName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static async Task<string> GetDomPropertyAsync(this Task<IWebElement> webElement, string attributeName)
            => (await webElement).GetDomProperty(attributeName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static async Task<string> GetDomAttributeAsync(this Task<IWebElement> webElement, string attributeName)
            => (await webElement).GetDomAttribute(attributeName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <returns></returns>
        public static async Task SubmitAsync(this Task<IWebElement> webElement) => (await webElement).Submit();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchContext"></param>
        /// <param name="cssSelector"></param>
        /// <returns></returns>
        public static ReadOnlyCollection<IWebElement> FindElements(this ISearchContext searchContext, string cssSelector)
            => searchContext.FindElements(By.CssSelector(cssSelector));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchContext"></param>
        /// <param name="cssSelector"></param>
        /// <returns></returns>
        public static IWebElement FindElement(this ISearchContext searchContext, string cssSelector)
            => searchContext.FindElement(By.CssSelector(cssSelector));
    }
}
