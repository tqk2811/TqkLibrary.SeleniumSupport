using OpenQA.Selenium;
using System;
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
        public static bool ElementsExists(ReadOnlyCollection<IWebElement> webElements) => webElements.Count > 0;

        public static bool AllElementsVisible(ReadOnlyCollection<IWebElement> webElements) => webElements.Count > 0 && webElements.All(x => x.Displayed);

        public static bool AnyElementsVisible(ReadOnlyCollection<IWebElement> webElements) => webElements.Any(x => x.Displayed);

        public static bool AllElementsClickable(ReadOnlyCollection<IWebElement> webElements) => webElements.Count > 0 && webElements.All(x => x.Displayed && x.Enabled);

        public static bool AnyElementsClickable(ReadOnlyCollection<IWebElement> webElements) => webElements.Any(x => x.Displayed && x.Enabled);

        public static bool AllElementsSelected(ReadOnlyCollection<IWebElement> webElements) => webElements.Count > 0 && webElements.All(x => x.Selected);

        public static bool AnyElementsSelected(ReadOnlyCollection<IWebElement> webElements) => webElements.Any(x => x.Selected);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
