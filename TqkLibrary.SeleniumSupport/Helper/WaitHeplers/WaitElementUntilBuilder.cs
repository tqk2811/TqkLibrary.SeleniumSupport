using System;

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
        public WaitElementBuilder ElementsExists() => _waitElementBuilder.Until(BaseChromeProfile.ElementsExists);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitElementBuilder AllElementsVisible() => _waitElementBuilder.Until(BaseChromeProfile.AllElementsVisible);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitElementBuilder AnyElementsVisible() => _waitElementBuilder.Until(BaseChromeProfile.AnyElementsVisible);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitElementBuilder AllElementsClickable() => _waitElementBuilder.Until(BaseChromeProfile.AllElementsClickable);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitElementBuilder AnyElementsClickable() => _waitElementBuilder.Until(BaseChromeProfile.AnyElementsClickable);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitElementBuilder AllElementsSelected() => _waitElementBuilder.Until(BaseChromeProfile.AllElementsSelected);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitElementBuilder AnyElementsSelected() => _waitElementBuilder.Until(BaseChromeProfile.AnyElementsSelected);
    }
}
