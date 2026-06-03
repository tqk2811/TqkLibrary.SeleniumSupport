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
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TqkLibrary.SeleniumSupport
{
    /// <summary>
    /// 
    /// </summary>
    public static class ChromeDriverUpdater
    {
        static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        /// <summary>
        /// File name of the chromedriver binary for the current OS (chromedriver.exe on Windows, chromedriver elsewhere).
        /// </summary>
        static string ChromeDriverFileName => IsWindows ? "chromedriver.exe" : "chromedriver";

        /// <summary>
        ///
        /// </summary>
        /// <param name="folderLocation"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Directory contain chromedriver binary</returns>
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
            string versionString = GetChromeVersionString(chromePath);
            if (!Version.TryParse(versionString, out Version? version))
                throw new InvalidOperationException($"invalid version '{versionString}'");

            string[] dirs = Directory.GetDirectories(folderLocation, $"{version.Major}.{version.Minor}.{version.Build}.*");
            string? targetDir = dirs.FirstOrDefault(x => File.Exists(Path.Combine(x, ChromeDriverFileName)));

            if (string.IsNullOrWhiteSpace(targetDir))
            {
                targetDir = Path.Combine(folderLocation, version.ToString());
                using HttpClient httpClient = new HttpClient(new HttpClientHandler()
                {
                    AutomaticDecompression = System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.GZip
                });

                string urlToDownload = string.Empty;
                if (version.Major <= 114)//support for old MajorPart
                {
                    urlToDownload = await httpClient.GetURLToDownloadAsync_Old(version, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    urlToDownload = await httpClient.GetURLToDownloadAsync(version, cancellationToken).ConfigureAwait(false);
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
            if (IsWindows)
            {
                foreach (string path in new[]
                {
                    Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles") ?? "C:\\Program Files",
                                 "Google\\Chrome\\Application\\chrome.exe"),
                    Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles(x86)") ?? "C:\\Program Files (x86)",
                                 "Google\\Chrome\\Application\\chrome.exe"),
                    //per-user install (cài không cần admin)
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                 "Google\\Chrome\\Application\\chrome.exe"),
                })
                    if (File.Exists(path)) return path;
            }
            else if (IsMacOS)
            {
                foreach (string path in new[]
                {
                    "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
                    "/Applications/Chromium.app/Contents/MacOS/Chromium",
                })
                    if (File.Exists(path)) return path;
            }
            else //Linux
            {
                foreach (string path in new[]
                {
                    "/usr/bin/google-chrome",
                    "/usr/bin/google-chrome-stable",
                    "/usr/bin/chromium",
                    "/usr/bin/chromium-browser",
                    "/snap/bin/chromium",
                })
                    if (File.Exists(path)) return path;
            }
            throw new FileNotFoundException("chrome");
        }

        static string GetChromeVersionString(string chromePath)
        {
            if (string.IsNullOrEmpty(chromePath)) throw new ArgumentException("Unable to get version because path is empty");
            if (!File.Exists(chromePath)) throw new FileNotFoundException("Unable to get version because path specifies a file that does not exists");

            if (IsWindows)
            {
                return FileVersionInfo.GetVersionInfo(chromePath).FileVersion
                    ?? throw new InvalidOperationException($"Unable to read chrome FileVersion from '{chromePath}'");
            }

            //Linux/macOS: ELF/Mach-O binaries carry no FileVersionInfo → query the browser itself.
            //e.g. "Google Chrome 126.0.6478.126 " / "Chromium 126.0.6478.126 snap"
            using Process process = new Process();
            process.StartInfo.FileName = chromePath;
            process.StartInfo.Arguments = "--version";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Match match = Regex.Match(output, @"\d+\.\d+\.\d+\.\d+");
            if (!match.Success) match = Regex.Match(output, @"\d+\.\d+\.\d+");
            if (match.Success) return match.Value;
            throw new InvalidOperationException($"Unable to parse chrome version from: '{output.Trim()}'");
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
            if (IsWindows)
            {
                if (Environment.Is64BitOperatingSystem) yield return "win64";
                yield return "win32";
            }
            else if (IsMacOS)
            {
                if (RuntimeInformation.OSArchitecture == Architecture.Arm64) yield return "mac-arm64";
                yield return "mac-x64";
            }
            else //Linux
            {
                yield return "linux64";
            }
        }
        static async Task<string> GetURLToDownloadAsync(this HttpClient httpClient, Version need_version, CancellationToken cancellationToken = default)
        {
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

            throw new NotSupportedException($"chromedrive {need_version}");
        }
        static async Task<string> GetURLToDownloadAsync_Old(this HttpClient httpClient, Version version, CancellationToken cancellationToken = default)
        {
            if (version is null) throw new ArgumentException("Unable to get url because version is empty");

            //URL's originates from here: https://chromedriver.chromium.org/downloads/version-selection
            string html = string.Empty;
            string urlToPathLocation = $"https://chromedriver.storage.googleapis.com/LATEST_RELEASE_{version.Major}.{version.Minor}.{version.Build}";

            using HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, urlToPathLocation);
            using HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
            html = await httpResponseMessage.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
            string oldPlatform = IsWindows ? "win32" : (IsMacOS ? "mac64" : "linux64");
            return $"https://chromedriver.storage.googleapis.com/{html}/chromedriver_{oldPlatform}.zip";
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
            ZipArchiveEntry? entry = zipArchive.Entries.FirstOrDefault(x => x.Name.Equals(ChromeDriverFileName, StringComparison.OrdinalIgnoreCase));
            if (entry is not null)
            {
                string driverPath = Path.Combine(tarGetDir, ChromeDriverFileName);
                entry.ExtractToFile(driverPath, true);
                if (!IsWindows) SetExecutable(driverPath);
                return tarGetDir;
            }
            else
            {
                throw new Exception($"{ChromeDriverFileName} not found in zip file");
            }
        }

        /// <summary>
        /// Mark the extracted chromedriver as executable on Linux/macOS (zip entries lose the +x bit).
        /// </summary>
        static void SetExecutable(string path)
        {
#if NET7_0_OR_GREATER
            if (OperatingSystem.IsWindows()) return;
            File.SetUnixFileMode(path,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
#else
            try
            {
                using Process process = new Process();
                process.StartInfo.FileName = "chmod";
                process.StartInfo.Arguments = $"+x \"{path}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit();
            }
            catch { }
#endif
        }
    }
}
