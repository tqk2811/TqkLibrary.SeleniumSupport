﻿using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.SeleniumSupport
{
    public partial class BaseChromeProfile
    {
        #region JSDropFile
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="webElement"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        public void JsDropFile(string file, IWebElement webElement, int offsetX, int offsetY)
        {
            if (chromeDriver is null) throw new InvalidOperationException($"{nameof(chromeDriver)} is null, need start chrome first");
            IWebElement input = (IWebElement)chromeDriver.ExecuteScript(Resource.JsDropFile, webElement, offsetX, offsetY);
            input.SendKeys(file);
        }

        #endregion JSDropFile


        #region JsClick
        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        public void JsDoubleClick(IWebElement webElement)
        {
            if (chromeDriver is null) throw new InvalidOperationException($"{nameof(chromeDriver)} is null, need start chrome first");
            chromeDriver.ExecuteScript(@"var evt = document.createEvent('MouseEvents');
evt.initMouseEvent('dblclick',true, true, window, 0, 0, 0, 0, 0, false, false, false, false, 0,null);
arguments[0].dispatchEvent(evt);", webElement);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        public void JsClick(IWebElement webElement)
        {
            if (chromeDriver is null) throw new InvalidOperationException($"{nameof(chromeDriver)} is null, need start chrome first");
            chromeDriver.ExecuteScript("arguments[0].click();", webElement);
        }

        #endregion JsClick




        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        public void JsScrollIntoView(IWebElement webElement)
        {
            if (chromeDriver is null) throw new InvalidOperationException($"{nameof(chromeDriver)} is null, need start chrome first");
            chromeDriver.ExecuteScript("arguments[0].scrollIntoView();", webElement);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <param name="text"></param>
        public void JsSetInputText(IWebElement webElement, string text)
        {
            if (chromeDriver is null) throw new InvalidOperationException($"{nameof(chromeDriver)} is null, need start chrome first");
            chromeDriver.ExecuteScript($"arguments[0].value = \"{text}\";", webElement);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webElement"></param>
        /// <returns></returns>
        public byte[]? JsScreenshot_html2canvas(IWebElement webElement)
        {
            if (chromeDriver is null) throw new InvalidOperationException($"{nameof(chromeDriver)} is null, need start chrome first");
            chromeDriver.ExecuteScript(Resource.html2canvas_min);
            string? res = chromeDriver.ExecuteAsyncScript("html2canvas(arguments[0]).then(canvas => { arguments[1](canvas.toDataURL('image/png')); })", webElement) as string;
            if (string.IsNullOrWhiteSpace(res))
            {
                return null;
            }
            else
            {
                return Convert.FromBase64String(res!.Substring("data:image/png;base64,".Length));
            }
        }
    }
}
