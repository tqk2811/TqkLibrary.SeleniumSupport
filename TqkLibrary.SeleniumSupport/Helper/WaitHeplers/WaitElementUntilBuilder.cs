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
        /// <summary>
        /// 
        /// </summary>
        public class WaitElementUntilBuilderChild
        {
            readonly WaitElementUntilBuilder _waitElementUntilBuilder;
            readonly Func<IEnumerable<IWebElement>, Func<IWebElement, bool>, IEnumerable<IWebElement>> _func;
            internal WaitElementUntilBuilderChild(
                WaitElementUntilBuilder waitElementUntilBuilder,
                Func<IEnumerable<IWebElement>, Func<IWebElement, bool>, IEnumerable<IWebElement>> func
                )
            {
                this._waitElementUntilBuilder = waitElementUntilBuilder;
                this._func = func;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public WaitElementBuilder Visible()
                => _waitElementUntilBuilder._waitElementBuilder.Until((eles) => _func.Invoke(eles, x => x.Displayed));
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public WaitElementBuilder Selected()
                => _waitElementUntilBuilder._waitElementBuilder.Until((eles) => _func.Invoke(eles, x => x.Selected));
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public WaitElementBuilder Clickable()
                => _waitElementUntilBuilder._waitElementBuilder.Until((eles) => _func.Invoke(eles, x => x.Displayed && x.Enabled));
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public WaitElementBuilder NotHidden()
                => _waitElementUntilBuilder._waitElementBuilder.Until((eles) => _func.Invoke(eles, x => !x.JsIsHidden()));
            /// <summary>
            /// 
            /// </summary>
            /// <param name="func"></param>
            /// <returns></returns>
            public WaitElementBuilder Condition(Func<IWebElement,bool> func)
                => _waitElementUntilBuilder._waitElementBuilder.Until((eles) => _func.Invoke(eles, x => func.Invoke(x)));

        }

        readonly WaitElementBuilder _waitElementBuilder;
        internal WaitElementUntilBuilder(WaitElementBuilder waitElementBuilder)
        {
            _waitElementBuilder = waitElementBuilder ?? throw new ArgumentNullException(nameof(waitElementBuilder));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitElementUntilBuilderChild Any()
            => new WaitElementUntilBuilderChild(this, (eles, contition) => eles.Where(contition));
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitElementUntilBuilderChild All()
            => new WaitElementUntilBuilderChild(this, (eles, contition) => eles.Any() && eles.All(contition) ? eles : Enumerable.Empty<IWebElement>());


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WaitElementBuilder ElementsExists() => _waitElementBuilder.Until(x => x);



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public WaitElementBuilder AllElementsVisible() => All().Visible();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public WaitElementBuilder AnyElementsVisible() => Any().Visible();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public WaitElementBuilder AllElementsClickable() => All().Clickable();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public WaitElementBuilder AnyElementsClickable() => Any().Clickable();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public WaitElementBuilder AllElementsSelected() => All().Selected();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public WaitElementBuilder AnyElementsSelected() => Any().Selected();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public WaitElementBuilder AllElementsNotHidden() => All().NotHidden();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public WaitElementBuilder AnyElementsNotHidden() => Any().NotHidden();

    }
}
