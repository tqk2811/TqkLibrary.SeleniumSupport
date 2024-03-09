using System;

namespace TqkLibrary.SeleniumSupport.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class ChromeAutoException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Message"></param>
        public ChromeAutoException(string Message) : base(Message)
        {
        }
    }
}