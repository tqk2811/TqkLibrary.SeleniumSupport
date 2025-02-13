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
    public static class SeleniumSupportExtensions
    {
        static readonly Random _random =
#if NET6_0_OR_GREATER
            Random.Shared;
#else
            new Random(DateTime.Now.GetHashCode());
#endif


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<T> UnWrapAsync<T>(this Task<T> task, Action<T> action)
        {
            if (task is null) throw new ArgumentNullException(nameof(task));
            if (action is null) throw new ArgumentNullException(nameof(action));
            T t = await task;
            action.Invoke(t);
            return t;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="task"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<TResult> UnWrapAsync<TElement, TResult>(this Task<TElement> task, Func<TElement, TResult> func)
        {
            if (task is null) throw new ArgumentNullException(nameof(task));
            if (func is null) throw new ArgumentNullException(nameof(func));
            TElement t = await task;
            return func.Invoke(t);
        }


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


        /// <summary>
        /// with webElement as first arguments
        /// </summary>
        /// <param name="webElement"></param>
        /// <param name="script"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static object ExecuteScript(this IWebElement webElement, string script, params object[] args)
        {
            if (webElement is null) throw new ArgumentNullException(nameof(webElement));
            if (args is null) args = new object[] { };
            return webElement.GetWebDriver().ExecuteScript(script, new object[] { webElement }.Concat(args).ToArray());
        }

        /// <summary>
        /// with webElement as first arguments
        /// </summary>
        /// <param name="t_webElement"></param>
        /// <param name="script"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Task<object> ExecuteScriptAsync(this Task<IWebElement> t_webElement, string script, params object[] args)
            => t_webElement.UnWrapAsync((e) => e.ExecuteScript(script, args));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <returns></returns>
        [Obsolete]
        public static FrameSwitch FrameSwitch(this IWebElement webElement)
        {
            return new FrameSwitch(webElement.GetWebDriver(), webElement);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <returns></returns>
        public static FrameSwitch GetFrameSwitch(this IWebElement webElement)
            => new FrameSwitch(webElement);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t_webElement"></param>
        /// <returns></returns>
        public static Task<FrameSwitch> GetFrameSwitchAsync(this Task<IWebElement> t_webElement)
            => t_webElement.UnWrapAsync((e) => new FrameSwitch(e));


        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <returns></returns>
        public static IWebElement? GetParentElement(this IWebElement webElement)
            => webElement.ExecuteScript("return arguments[0].parentNode;", new object[] { }) as IWebElement;




        #region JSDropFile
        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <param name="file"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        public static IWebElement JsDropFile(this IWebElement webElement, string file, int offsetX, int offsetY)
        {
            IWebElement input = (IWebElement)webElement.GetWebDriver().ExecuteScript(Resource.JsDropFile, webElement, offsetX, offsetY);
            input.SendKeys(file);
            return webElement;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t_webElement"></param>
        /// <param name="file"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <returns></returns>
        public static Task<IWebElement> JsDropFileAsync(this Task<IWebElement> t_webElement, string file, int offsetX, int offsetY)
            => t_webElement.UnWrapAsync((e) => e.JsDropFile(file, offsetX, offsetY));

        #endregion JSDropFile

        #region JsClick
        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        public static IWebElement JsDoubleClick(this IWebElement webElement)
        {
            webElement.GetWebDriver().ExecuteScript(@"var evt = document.createEvent('MouseEvents');
evt.initMouseEvent('dblclick',true, true, window, 0, 0, 0, 0, 0, false, false, false, false, 0,null);
arguments[0].dispatchEvent(evt);", webElement);
            return webElement;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t_webElement"></param>
        /// <returns></returns>
        public static Task<IWebElement> JsDoubleClickAsync(this Task<IWebElement> t_webElement)
            => t_webElement.UnWrapAsync((e) => e.JsDoubleClick());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        public static IWebElement JsClick(this IWebElement webElement)
        {
            webElement.GetWebDriver().ExecuteScript("arguments[0].click();", webElement);
            return webElement;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t_webElement"></param>
        /// <returns></returns>
        public static Task<IWebElement> JsClickAsync(this Task<IWebElement> t_webElement)
            => t_webElement.UnWrapAsync((e) => e.JsClick());

        #endregion JsClick

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <param name="html"></param>
        /// <returns></returns>
        public static IWebElement JsSetInnerHtml(this IWebElement webElement, string html)
        {
            webElement.ExecuteScript("arguments[0].innerHTML = arguments[1];", html);
            return webElement;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t_webElement"></param>
        /// <param name="html"></param>
        /// <returns></returns>
        public static Task<IWebElement> JsSetInnerHtmlAsync(this Task<IWebElement> t_webElement, string html)
            => t_webElement.UnWrapAsync((e) => e.JsSetInnerHtml(html));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <param name="html"></param>
        /// <returns></returns>
        public static IWebElement JsSetInnerText(this IWebElement webElement, string html)
        {
            webElement.ExecuteScript("arguments[0].innerText = arguments[1];", html);
            return webElement;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t_webElement"></param>
        /// <param name="html"></param>
        /// <returns></returns>
        public static Task<IWebElement> JsSetInnerTextAsync(this Task<IWebElement> t_webElement, string html)
            => t_webElement.UnWrapAsync((e) => e.JsSetInnerText(html));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <returns></returns>
        public static bool JsIsHidden(this IWebElement webElement)
            => (bool)webElement.ExecuteScript("return arguments[0].offsetParent === null;");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t_webElement"></param>
        /// <returns></returns>
        public static Task<bool> JsIsHiddenAsync(this Task<IWebElement> t_webElement)
            => t_webElement.UnWrapAsync((e) => e.JsIsHidden());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        public static IWebElement JsScrollIntoView(this IWebElement webElement)
        {
            webElement.GetWebDriver().ExecuteScript("arguments[0].scrollIntoView();", webElement);
            return webElement;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t_webElement"></param>
        /// <returns></returns>
        public static Task<IWebElement> JsScrollIntoViewAsync(this Task<IWebElement> t_webElement)
            => t_webElement.UnWrapAsync((e) => e.JsScrollIntoView());


        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <param name="text"></param>
        public static IWebElement JsSetInputText(this IWebElement webElement, string text)
        {
            webElement.GetWebDriver().ExecuteScript($"arguments[0].value = \"{text}\";", webElement);
            return webElement;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t_webElement"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static Task<IWebElement> JsSetInputTextAsync(this Task<IWebElement> t_webElement, string text)
            => t_webElement.UnWrapAsync((e) => e.JsSetInputText(text));

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
        public static TabSwitch TabSwitchFromUrl(this WebDriver webDriver, string url, bool isCloseTab = true)
        {
            return TabSwitch.FromUrl(webDriver, url, isCloseTab);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="webDriver"></param>
        /// <param name="tabId"></param>
        /// <param name="isCloseTab"></param>
        /// <returns></returns>
        public static TabSwitch TabSwitchFromExistTab(this WebDriver webDriver, string tabId, bool isCloseTab = true)
        {
            return TabSwitch.FromExistTab(webDriver, tabId, isCloseTab);
        }


        #region LinQ Async

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t_webElements"></param>
        /// <returns></returns>
        public static async Task<IWebElement> FirstAsync(this Task<ReadOnlyCollection<IWebElement>> t_webElements)
            => (await t_webElements).First();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t_webElements"></param>
        /// <returns></returns>
        public static async Task<IWebElement?> FirstOrDefaultAsync(this Task<ReadOnlyCollection<IWebElement>> t_webElements)
            => (await t_webElements)?.FirstOrDefault();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t_webElements"></param>
        /// <returns></returns>
        public static async Task<IWebElement> LastAsync(this Task<ReadOnlyCollection<IWebElement>> t_webElements)
            => (await t_webElements).Last();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t_webElements"></param>
        /// <returns></returns>
        public static async Task<IWebElement?> LastOrDefaultAsync(this Task<ReadOnlyCollection<IWebElement>> t_webElements)
            => (await t_webElements)?.LastOrDefault();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="t_webElements"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static async Task<IWebElement?> TakeRandomAsync(this Task<ReadOnlyCollection<IWebElement>> t_webElements, Random? random = null)
        {
            random = random ?? _random;
            var webElements = await t_webElements;
            return webElements.Skip(random.Next(webElements.Count)).FirstOrDefault();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="t_webElements"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static async Task<bool> AnyAsync(this Task<ReadOnlyCollection<IWebElement>> t_webElements, Func<IWebElement, bool> predicate)
            => (await t_webElements).Any(predicate);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t_webElements"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static async Task<bool> AllAsync(this Task<ReadOnlyCollection<IWebElement>> t_webElements, Func<IWebElement, bool> predicate)
            => (await t_webElements).All(predicate);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t_webElements"></param>
        /// <returns></returns>
        public static async Task<int> CountAsync(this Task<ReadOnlyCollection<IWebElement>> t_webElements)
            => (await t_webElements).Count();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t_webElements"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static async Task<int> CountAsync(this Task<ReadOnlyCollection<IWebElement>> t_webElements, Func<IWebElement, bool> predicate)
            => (await t_webElements).Count(predicate);

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t_webElement"></param>
        /// <returns></returns>
        public static async Task<IWebElement> ClickAsync(this Task<IWebElement> t_webElement)
        {
            IWebElement webElement = await t_webElement;
            webElement.Click();
            return webElement;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="t_webElement"></param>
        /// <returns></returns>
        public static async Task<IWebElement> ClearAsync(this Task<IWebElement> t_webElement)
        {
            IWebElement webElement = await t_webElement;
            webElement.Clear();
            return webElement;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="t_webElement"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static async Task<IWebElement> SendKeysAsync(this Task<IWebElement> t_webElement, string text)
        {
            IWebElement webElement = await t_webElement;
            webElement.SendKeys(text);
            return webElement;
        }

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
        /// <param name="t_webElement"></param>
        /// <returns></returns>
        public static async Task<IWebElement> SubmitAsync(this Task<IWebElement> t_webElement)
        {
            IWebElement webElement = await t_webElement;
            webElement.Submit();
            return webElement;
        }

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
