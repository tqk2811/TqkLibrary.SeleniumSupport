using System.IO.Compression;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Text;

namespace TqkLibrary.SeleniumSupport
{
    /// <summary>
    /// 
    /// </summary>
    public static class ProxyLoginExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public static void GenerateExtension(string path, string host, string port, string? username = null, string? password = null, bool isPacked = true)
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

                using FileStream fileStream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
                using ZipArchive zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create);
                using (var stream = zipArchive.CreateEntry("manifest.json").Open())
                {
                    var buffer = Encoding.UTF8.GetBytes(Resource.ProxyLogin_Ext_manifest);
                    stream.Write(buffer, 0, buffer.Length);
                }
                using (var stream = zipArchive.CreateEntry("background.js").Open())
                {
                    var buffer = Encoding.UTF8.GetBytes(background_);
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            else
            {
                if (Directory.Exists(path)) try { Directory.Delete(path); } catch { }
                Directory.CreateDirectory(path);

                File.WriteAllText(Path.Combine(path, "background.js"), background_);
                File.WriteAllText(Path.Combine(path, "manifest.json"), Resource.ProxyLogin_Ext_manifest);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public static void GenerateExtension(string filepath, string host, int port, string? username = null, string? password = null, bool isPacked = true)
          => GenerateExtension(filepath, host, port.ToString(), username, password, isPacked);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chromeOptions"></param>
        /// <param name="filepath"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AddProxyLoginExtension(this ChromeOptions chromeOptions, string filepath, string host, string port, string? username = null, string? password = null)
        {
            GenerateExtension(filepath, host, port, username, password, true);
            chromeOptions.AddExtension(filepath);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="chromeOptions"></param>
        /// <param name="filepath"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AddProxyLoginExtension(this ChromeOptions chromeOptions, string filepath, string host, int port, string? username = null, string? password = null)
            => AddProxyLoginExtension(chromeOptions, filepath, host, port.ToString(), username, password);
    }
}