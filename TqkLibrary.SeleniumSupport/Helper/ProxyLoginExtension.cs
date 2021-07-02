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
      if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
      if (string.IsNullOrEmpty(host)) throw new ArgumentNullException(nameof(host));
      if (string.IsNullOrEmpty(port)) throw new ArgumentNullException(nameof(port));
      if (string.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));
      if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));

      if (File.Exists(filepath)) try { File.Delete(filepath); } catch (Exception) { }

      string background_ = Resource.ProxyLogin_Ext_background
        .Replace("{host}", host)
        .Replace("{port}", port.ToString())
        .Replace("{username}", username)
        .Replace("{password}", password);
      using ZipFile zipFile = ZipFile.Create(filepath);
      zipFile.BeginUpdate();
      using CustomStaticDataSource manifest = new CustomStaticDataSource(Resource.ProxyLogin_Ext_manifest);
      zipFile.Add(manifest, "manifest.json");
      using CustomStaticDataSource background = new CustomStaticDataSource(background_);
      zipFile.Add(background, "background.js");
      zipFile.CommitUpdate();
      zipFile.Close();
    }

    public static void GenerateExtension(string filepath, string host, int port, string username, string password) 
      => GenerateExtension(filepath, host, port.ToString(), username, password);


    private class CustomStaticDataSource : IStaticDataSource, IDisposable
    {
      private readonly MemoryStream memoryStream;

      public CustomStaticDataSource(string content)
      {
        this.memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
      }

      public void Dispose() => memoryStream.Dispose();

      public Stream GetSource() => memoryStream;
    }
  }
}