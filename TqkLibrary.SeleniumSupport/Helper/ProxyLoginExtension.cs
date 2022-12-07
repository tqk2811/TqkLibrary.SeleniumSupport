using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Text;

namespace TqkLibrary.SeleniumSupport
{
    public static class ProxyLoginExtension
    {
        public static void GenerateExtension(string path, string host, string port, string username = null, string password = null, bool isPacked = true)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            if (string.IsNullOrEmpty(host)) throw new ArgumentNullException(nameof(host));
            if (string.IsNullOrEmpty(port)) throw new ArgumentNullException(nameof(port));
            //if (string.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));
            //if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));

            string background_ = Resource.ProxyLogin_Ext_background
                .Replace("{host}", host)
                .Replace("{port}", port.ToString())
                .Replace("{username}", username ?? string.Empty)
                .Replace("{password}", password ?? string.Empty);

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
                if (Directory.Exists(path)) try { Directory.Delete(path); } catch { }
                Directory.CreateDirectory(path);

                File.WriteAllText(Path.Combine(path, "background.js"), background_);
                File.WriteAllText(Path.Combine(path, "manifest.json"), Resource.ProxyLogin_Ext_manifest);
            }
        }

        public static void GenerateExtension(string filepath, string host, int port, string username = null, string password = null, bool isPacked = true)
          => GenerateExtension(filepath, host, port.ToString(), username, password, isPacked);
    }
}