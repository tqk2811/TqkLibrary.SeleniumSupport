using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.ObjectModel;

namespace TqkLibrary.SeleniumSupport
{
  public static class SeleniumHelper
  {
    public static ReadOnlyCollection<IWebElement> ThrowIfNull(this ReadOnlyCollection<IWebElement> readOnlyCollection, string throwText)
    {
      if (null == readOnlyCollection) throw new ChromeAutoException(throwText);
      return readOnlyCollection;
    }

    public static ReadOnlyCollection<IWebElement> ThrowIfNullOrCountZero(this ReadOnlyCollection<IWebElement> readOnlyCollection, string throwText)
    {
      if (null == readOnlyCollection || readOnlyCollection.Count == 0) throw new ChromeAutoException(throwText);
      return readOnlyCollection;
    }

    public static ChromeOptions AddProfilePath(this ChromeOptions chromeOptions, string ProfilePath)
    {
      if (string.IsNullOrEmpty(ProfilePath)) throw new ArgumentNullException(nameof(ProfilePath));
      chromeOptions.AddArgument("--user-data-dir=" + ProfilePath);
      return chromeOptions;
    }

    public static ChromeOptions AddUserAgent(this ChromeOptions chromeOptions, string UA)
    {
      if (string.IsNullOrEmpty(UA)) throw new ArgumentNullException(nameof(UA));
      chromeOptions.AddArgument("--user-agent=" + UA);
      return chromeOptions;
    }

    public static ChromeOptions AddProxy(this ChromeOptions chromeOptions, string proxy)
    {
      if (string.IsNullOrEmpty(proxy)) throw new ArgumentNullException(nameof(proxy));
      chromeOptions.AddArguments("--proxy-server=" + string.Format("http://{0}", proxy));
      return chromeOptions;
    }

    public static ChromeOptions AddProxy(this ChromeOptions chromeOptions, string host, int port)
    {
      if (string.IsNullOrEmpty(host)) throw new ArgumentNullException(nameof(host));
      chromeOptions.AddArguments("--proxy-server=" + string.Format("http://{0}:{1}", host, port));
      return chromeOptions;
    }
  }
}