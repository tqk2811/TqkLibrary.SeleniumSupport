using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.SeleniumSupport
{
  public static class GoogleLoginExtension
  {
    public static void GenerateExtension(string dirPath,string email, string pass, string recovery,int timeout = 60000,int intervalTime = 500)
    {
      string inject_ = Resource.GoogleLogin_Ext_inject
        .Replace("{email}", email)
        .Replace("{pass}", pass)
        .Replace("{recovery}", recovery)
        .Replace("{timeout}",timeout.ToString())
        .Replace("{intervalTime}",intervalTime.ToString());
      if (Directory.Exists(dirPath)) try { Directory.Delete(dirPath); } catch { }

      var dirInfo = Directory.CreateDirectory(dirPath);

      using StreamWriter manifet = new StreamWriter(dirInfo.FullName + "\\manifest.json");
      manifet.Write(Resource.GoogleLogin_Ext_manifest);

      using StreamWriter inject = new StreamWriter(dirInfo.FullName + "\\inject.js");
      inject.Write(inject_);

      using StreamWriter background = new StreamWriter(dirInfo.FullName + "\\background.js");
      background.Write(Resource.GoogleLogin_Ext_background);
    }
  }
}
