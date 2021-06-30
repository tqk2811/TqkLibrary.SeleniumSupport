using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Text;

namespace TqkLibrary.SeleniumSupport
{
  public static class ProxyLoginExtension
  {
    public static void GenerateExtension(string filepath, string host, string port, string username, string password)
    {
      string background_ = Resource.ProxyLogin_Ext_background.Replace("{host}", host).Replace("{port}", port.ToString()).Replace("{username}", username).Replace("{password}", password);
      if (File.Exists(filepath)) try { File.Delete(filepath); } catch (Exception) { }
      ZipFile zipFile = ZipFile.Create(filepath);
      zipFile.BeginUpdate();
      using CustomStaticDataSource manifest = new CustomStaticDataSource(Resource.ProxyLogin_Ext_manifest);
      zipFile.Add(manifest, "manifest.json");
      using CustomStaticDataSource background = new CustomStaticDataSource(background_);
      zipFile.Add(background, "background.js");
      zipFile.CommitUpdate();
      zipFile.Close();
    }

    public static void GenerateExtension(string filepath, string host, int port, string username, string password) => GenerateExtension(filepath, host, port.ToString(), username, password);
  }
}