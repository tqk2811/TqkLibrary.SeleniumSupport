using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderLocation"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Directory contain chromedriver.exe</returns>
        public static Task<string> DownloadAsync(string folderLocation, CancellationToken cancellationToken = default)
        {
            return DownloadAsync(folderLocation, GetChromePath(), cancellationToken);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>Directory contain chromedriver.exe</returns>
        public static async Task<string> DownloadAsync(string folderLocation, string chromePath, CancellationToken cancellationToken = default)
        {
            var version = GetChromeVersion(chromePath);
            string targetDir = Path.Combine(folderLocation, version.FileVersion);
            if (!File.Exists(Path.Combine(targetDir, "chromedriver.exe")))
            {
                using HttpClient httpClient = new HttpClient(new HttpClientHandler()
                {
                    AutomaticDecompression = System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.GZip
                });

                string? verString = version.FileVersion;
                if (string.IsNullOrWhiteSpace(verString)) throw new VersionNotFoundException(chromePath);
                string urlToDownload = string.Empty;
                if (version.FileMajorPart <= 114)//support for old MajorPart
                {
                    urlToDownload = await httpClient.GetURLToDownloadAsync_Old(verString, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    urlToDownload = await httpClient.GetURLToDownloadAsync(verString, cancellationToken).ConfigureAwait(false);
                }
                return await httpClient.DownloadNewVersionOfChromeAsync(targetDir, urlToDownload, cancellationToken);
            }
            else
            {
                return targetDir;
            }
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
            else using (RegistryKey? key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\chrome.exe"))
                {
                    object? o = key?.GetValue("");
                    string? chromePath = o?.ToString();
                    if (!string.IsNullOrEmpty(chromePath))
                    {
                        return chromePath!;
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

        class KnownGoodVersionsWithDownloads
        {
            [JsonProperty("timestamp")]
            public DateTime Timestamp { get; set; }

            [JsonProperty("versions")]
            public List<DownloadVersionInfo>? Versions { get; set; }
        }
        class DownloadVersionInfo
        {
            [JsonProperty("version")]
            public string? Version { get; set; }

            [JsonProperty("revision")]
            public string? Revision { get; set; }

            [JsonProperty("downloads")]
            public DownloadList? Downloads { get; set; }


            public Version? GetVersion()
            {
                System.Version? version = null;
                System.Version.TryParse(Version, out version);
                return version;
            }
        }
        class DownloadList
        {
            [JsonProperty("chrome")]
            public List<DownloadInfo>? Chrome { get; set; }

            [JsonProperty("chromedriver")]
            public List<DownloadInfo>? Chromedriver { get; set; }
        }
        class DownloadInfo
        {
            [JsonProperty("platform")]
            public string? Platform { get; set; }

            [JsonProperty("url")]
            public string? Url { get; set; }
        }

        static IEnumerable<string> GetSupportPlatforms()
        {
            if (Environment.Is64BitOperatingSystem) yield return "win64";
            yield return "win32";
        }
        static async Task<string> GetURLToDownloadAsync(this HttpClient httpClient, string version, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(version))
                throw new ArgumentException("Unable to get url because version is empty");

            if (!Version.TryParse(version, out var need_version))
                throw new ArgumentNullException($"invalid version '{version}'");

            KnownGoodVersionsWithDownloads? knownGood = null;
            {
                using HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://googlechromelabs.github.io/chrome-for-testing/known-good-versions-with-downloads.json");
                using HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
                string json = await httpResponseMessage.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
                knownGood = JsonConvert.DeserializeObject<KnownGoodVersionsWithDownloads>(json);
            }

            DownloadVersionInfo? downloadVersionInfo = knownGood?.Versions?
                .Where(x =>
                {
                    var ver = x.GetVersion();
                    return
                        ver is not null &&
                        ver.Major == need_version.Major && ver.Minor == need_version.Minor && ver.Build == need_version.Build &&
                        x?.Downloads?.Chromedriver is not null &&
                        x.Downloads.Chromedriver.Any(y => !string.IsNullOrWhiteSpace(y?.Url) && GetSupportPlatforms().Contains(y?.Platform));
                })
                .OrderBy(x => x.GetVersion()?.Revision)
                .LastOrDefault();

            if (downloadVersionInfo is not null)
            {
                foreach (var platform in GetSupportPlatforms())
                {
                    if (Uri.TryCreate(downloadVersionInfo?.Downloads?.Chromedriver
                        ?.FirstOrDefault(x => platform.Equals(x.Platform))?.Url, UriKind.RelativeOrAbsolute, out Uri? uri))
                    {
                        return uri.ToString();
                    }
                }
            }

            throw new NotSupportedException($"chromedrive {version}");
        }
        static async Task<string> GetURLToDownloadAsync_Old(this HttpClient httpClient, string version, CancellationToken cancellationToken = default)
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

        static async Task<string> DownloadNewVersionOfChromeAsync(this HttpClient httpClient, string tarGetDir, string urlToDownload, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(urlToDownload)) throw new ArgumentException("Unable to get url because urlToDownload is empty");


            using HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, urlToDownload);
            using HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            httpResponseMessage.EnsureSuccessStatusCode();

            Directory.CreateDirectory(tarGetDir);


            using MemoryStream memoryStream = new MemoryStream();

            using Stream stream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
            await stream.CopyToAsync(memoryStream, 81920, cancellationToken).ConfigureAwait(false);

            memoryStream.Seek(0, SeekOrigin.Begin);
            using ZipArchive zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
            ZipArchiveEntry? entry = zipArchive.Entries.FirstOrDefault(x => x.Name.Contains("chromedriver.exe"));
            if (entry is not null)
            {
                entry.ExtractToFile(Path.Combine(tarGetDir, "chromedriver.exe"), true);
                return tarGetDir;
            }
            else
            {
                throw new Exception($"chromedriver.exe not found in zip file");
            }
        }
    }
}
