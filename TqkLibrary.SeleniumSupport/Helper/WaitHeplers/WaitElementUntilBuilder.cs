using OpenQA.Selenium;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TqkLibrary.SeleniumSupport.Helper.WaitHeplers
{
    /// <summary>
    /// 
    /// </summary>
    public class WaitElementUntilBuilder
    {
        readonly WaitElementBuilder _waitElementBuilder;
        internal WaitElementUntilBuilder(WaitElementBuilder waitElementBuilder)
        {
            _waitElementBuilder = waitElementBuilder ?? throw new ArgumentNullException(nameof(waitElementBuilder));
        }

















        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitElementBuilder ElementsExists() => _waitElementBuilder.Until(ElementsExists);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitElementBuilder AllElementsVisible() => _waitElementBuilder.Until(AllElementsVisible);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitElementBuilder AnyElementsVisible() => _waitElementBuilder.Until(AnyElementsVisible);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitElementBuilder AllElementsClickable() => _waitElementBuilder.Until(AllElementsClickable);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitElementBuilder AnyElementsClickable() => _waitElementBuilder.Until(AnyElementsClickable);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitElementBuilder AllElementsSelected() => _waitElementBuilder.Until(AllElementsSelected);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitElementBuilder AnyElementsSelected() => _waitElementBuilder.Until(AnyElementsSelected);





#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        public static IEnumerable<IWebElement> ElementsExists(IEnumerable<IWebElement> webElements)
            => webElements;

        public static IEnumerable<IWebElement> AllElementsVisible(IEnumerable<IWebElement> webElements)
            => webElements.Any() && webElements.All(x => x.Displayed) ? webElements : Enumerable.Empty<IWebElement>();

        public static IEnumerable<IWebElement> AnyElementsVisible(IEnumerable<IWebElement> webElements)
            => webElements.Where(x => x.Displayed);

        public static IEnumerable<IWebElement> AllElementsClickable(IEnumerable<IWebElement> webElements)
            => webElements.Any() && webElements.All(x => x.Displayed && x.Enabled) ? webElements : Enumerable.Empty<IWebElement>();

        public static IEnumerable<IWebElement> AnyElementsClickable(IEnumerable<IWebElement> webElements)
            => webElements.Where(x => x.Displayed && x.Enabled);

        public static IEnumerable<IWebElement> AllElementsSelected(IEnumerable<IWebElement> webElements)
            => webElements.Any() && webElements.All(x => x.Selected) ? webElements : Enumerable.Empty<IWebElement>();

        public static IEnumerable<IWebElement> AnyElementsSelected(IEnumerable<IWebElement> webElements)
            => webElements.Where(x => x.Selected);

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
