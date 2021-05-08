using System;

namespace TqkLibrary.SeleniumSupport
{
  public class ChromeAutoException : Exception
  {
    public ChromeAutoException(string Message) : base(Message)
    {
    }
  }
}