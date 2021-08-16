using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Text;

namespace TqkLibrary.SeleniumSupport
{
  public static class ProxyLoginExtension
  {
    public static void GenerateExtension(string path, string host, string port, string username, string password, bool isPacked = true)
    {
      if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
      if (string.IsNullOrEmpty(host)) throw new ArgumentNullException(nameof(host));
      if (string.IsNullOrEmpty(port)) throw new ArgumentNullException(nameof(port));
      if (string.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));
      if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));

      string background_ = Resource.ProxyLogin_Ext_background
          .Replace("{host}", host)
          .Replace("{port}", port.ToString())
          .Replace("{username}", username)
          .Replace("{password}", password);

      if (isPacked)
      {
        if (File.Exists(path)) try { File.Delete(path); } catch { }

        using ZipFile zipFile = ZipFile.Create(path);
        zipFile.BeginUpdate();
        using CustomStaticDataSource manifest = new CustomStaticDataSource(Resource.ProxyLogin_Ext_manifest);
        zipFile.Add(manifest, "manifest.json");
        using CustomStaticDataSource background = new CustomStaticDataSource(background_);
        zipFile.Add(background, "background.js");
        zipFile.CommitUpdate();
        zipFile.Close();
      }
      else
      {
        File.WriteAllText(Path.Combine(path, "background.js"), background_);
        File.WriteAllText(Path.Combine(path, "manifest.json"), Resource.ProxyLogin_Ext_manifest);
      }
    }

    public static void GenerateExtension(string filepath, string host, int port, string username, string password, bool isPacked = true) 
      => GenerateExtension(filepath, host, port.ToString(), username, password, isPacked);


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