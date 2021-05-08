using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.SeleniumSupport.Helper
{
  //https://www.whatismybrowser.com/guides/the-latest-user-agent/chrome
  public static class UserAgentHelper
  {
    static readonly List<string> Oss = new List<string>()
    {
      "Windows NT 10.0; Win64; x64",
      "Windows NT 10.0; WOW64",
      "Windows NT 6.3; Win64; x64",
      "Windows NT 6.3; WOW64",
      "Windows NT 6.1; Win64; x64",
      "Windows NT 6.1; WOW64",
      "X11; Linux x86_64",
      "X11; Linux i686",
      "X11; Fedora;Linux x86; rv:60.0",
      "X11; Ubuntu; Linux x86_64; rv:47.0",
      "X11; Ubuntu; Linux x86_64; rv:21.0",
      "Macintosh; Intel Mac OS X 10_10_5",
      "Macintosh; Intel Mac OS X 10_10_2",
      "Macintosh; Intel Mac OS X 10_10_3",
      "Macintosh; Intel Mac OS X 10.6; rv:40.0",
      "Macintosh; Intel Mac OS X 10_9_5",
      "Macintosh; Intel Mac OS X 10_10_4",
      "Macintosh; Intel Mac OS X 10.9; rv:40.0",
      "Macintosh; Intel Mac OS X 10_10_3",
    };
    static readonly List<string> Uas = new List<string>()
    {
      "Mozilla/5.0 ({os}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.157 Safari/537.36",
      "Mozilla/5.0 ({os}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36",
      "Mozilla/5.0 ({os}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36",
      "Mozilla/5.0 ({os}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.88 Safari/537.36",
      "Mozilla/5.0 ({os}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.117 Safari/537.36",
      "Mozilla/5.0 ({os}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.87 Safari/537.36",
      "Mozilla/5.0 ({os}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36",
      "Mozilla/5.0 ({os}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.138 Safari/537.36",
      "Mozilla/5.0 ({os}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36",
      "Mozilla/5.0 ({os}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.105 Safari/537.36",
      "Mozilla/5.0 ({os}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.125 Safari/537.36",
      "Mozilla/5.0 ({os}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.83 Safari/537.36",
      "Mozilla/5.0 ({os}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.102 Safari/537.36",
      "Mozilla/5.0 ({os}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.121 Safari/537.36",
      "Mozilla/5.0 ({os}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.183 Safari/537.36",
      "Mozilla/5.0 ({os}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.66 Safari/537.36",      
    };

    public const string Iphone = "Mozilla/5.0 (iPhone; CPU iPhone OS 13_2_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.0.3 Mobile/1SE148 Safari/604.1";
    static readonly Random random = new Random();
    public static string GetRandomUa() => Uas[random.Next(Uas.Count)].Replace("{os}", Oss[random.Next(Oss.Count)]);


    
    public static void AddOs(IEnumerable<string> Oss)
    {
      UserAgentHelper.Oss.AddRange(Oss);
    }

    /// <summary>
    /// UA with {os} is OS<br/>
    /// Ex: Mozilla/5.0 ({os}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.125 Safari/537.36
    /// </summary>
    /// <param name="Uas"></param>
    public static int AddUa(IEnumerable<string> Uas)
    {
      int count = 0;
      foreach(var ua in Uas)
      {
        if(ua.IndexOf("{os}") >= 0)
        {
          UserAgentHelper.Uas.Add(ua);
          count++;
        }
      }

      return count;
    }
  }
}
