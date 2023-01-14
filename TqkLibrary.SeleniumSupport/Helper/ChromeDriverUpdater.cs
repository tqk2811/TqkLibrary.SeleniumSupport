using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TqkLibrary.SeleniumSupport
{
    /// <summary>
    /// 
    /// </summary>
    public static class ChromeDriverUpdater
    {
        static readonly HttpClient httpClient = new HttpClient(new HttpClientHandler()
        {
            AutomaticDecompression = System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.GZip
        });

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<int> Download(string folderLocation, int MajorPart, CancellationToken cancellationToken = default)
        {
            string path = GetChromePath();
            var version = GetChromeVersion(path);
            if (version.FileMajorPart > MajorPart)
            {
                var verString = version.FileVersion;
                var urlToDownload = await GetURLToDownload(verString, cancellationToken).ConfigureAwait(false);
                KillAllChromeDriverProcesses();
                await DownloadNewVersionOfChrome(folderLocation, urlToDownload, cancellationToken);
            }
            return version.FileMajorPart;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static string GetChromePath()
        {
            //Path originates from here: https://chromedriver.chromium.org/downloads/version-selection            
            string chrome64 = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe";
            string chrome86 = "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe";
            if (File.Exists(chrome64)) return chrome64;
            else if (File.Exists(chrome86)) return chrome86;
            else using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\chrome.exe"))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue("");
                        if (!string.IsNullOrEmpty(o.ToString()))
                        {
                            return o.ToString();
                        }
                    }
                }
            throw new FileNotFoundException("chrome.exe");
        }

        static FileVersionInfo GetChromeVersion(string productVersionPath)
        {
            if (string.IsNullOrEmpty(productVersionPath)) throw new ArgumentException("Unable to get version because path is empty");
            if (!File.Exists(productVersionPath)) throw new FileNotFoundException("Unable to get version because path specifies a file that does not exists");

            return FileVersionInfo.GetVersionInfo(productVersionPath);
        }

        static async Task<string> GetURLToDownload(string version, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(version)) throw new ArgumentException("Unable to get url because version is empty");

            //URL's originates from here: https://chromedriver.chromium.org/downloads/version-selection
            string html = string.Empty;
            string urlToPathLocation = @"https://chromedriver.storage.googleapis.com/LATEST_RELEASE_" + string.Join(".", version.Split('.').Take(3));

            using HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, urlToPathLocation);
            using HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
            html = await httpResponseMessage.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
            return "https://chromedriver.storage.googleapis.com/" + html + "/chromedriver_win32.zip";
        }

        static void KillAllChromeDriverProcesses()
        {
            //It's important to kill all processes before attempting to replace the chrome driver, because if you do not you may still have file locks left over
            var processes = Process.GetProcessesByName("chromedriver");
            foreach (var process in processes)
            {
                try
                {
                    process.Kill();
                }
                catch
                {
                    //We do our best here but if another user account is running the chrome driver we may not be able to kill it unless we run from a elevated user account + various other reasons we don't care about
                }
            }
        }

        static async Task DownloadNewVersionOfChrome(string folderLocation, string urlToDownload, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(urlToDownload)) throw new ArgumentException("Unable to get url because urlToDownload is empty");

            if (File.Exists(folderLocation + "\\chromedriver.zip"))
            {
                File.Delete(folderLocation + "\\chromedriver.zip");
            }

            using HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, urlToDownload);
            using HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            httpResponseMessage.EnsureSuccessStatusCode();

            if (Directory.Exists(folderLocation))
            {
                Directory.Delete(folderLocation, true);
            }
            string zipPath = Path.Combine(folderLocation, "chromedriver.zip");

            using (FileStream fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                using Stream stream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
                await stream.CopyToAsync(fileStream, 81920, cancellationToken).ConfigureAwait(false);
            }

            if (File.Exists(zipPath))
            {
                ZipFile.ExtractToDirectory(zipPath, folderLocation);
            }
        }
    }
}
