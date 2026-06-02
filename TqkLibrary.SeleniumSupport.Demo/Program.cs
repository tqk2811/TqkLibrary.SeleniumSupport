using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TqkLibrary.SeleniumSupport;

namespace TqkLibrary.SeleniumSupport.Demo
{
    /// <summary>
    /// Cross-platform smoke test for TqkLibrary.SeleniumSupport (Windows + Linux/macOS).
    ///
    /// Usage:
    ///   dotnet run --project TqkLibrary.SeleniumSupport.Demo -- [url] [--headed]
    ///     url       optional, default https://example.com
    ///     --headed  run with a visible window (default: headless, required on a server/CI Ubuntu)
    /// </summary>
    internal class Program
    {
        // BaseChromeProfile is abstract with no abstract members -> a bare subclass is enough.
        private sealed class ChromeProfile : BaseChromeProfile { }

        private static async Task<int> Main(string[] args)
        {
            bool headed = args.Contains("--headed", StringComparer.OrdinalIgnoreCase);
            string url = args.FirstOrDefault(a => a.StartsWith("http", StringComparison.OrdinalIgnoreCase)) ?? "https://example.com";

            Console.WriteLine("=== TqkLibrary.SeleniumSupport cross-platform demo ===");
            Console.WriteLine($"OS         : {RuntimeInformation.OSDescription}");
            Console.WriteLine($"Arch       : {RuntimeInformation.OSArchitecture}");
            Console.WriteLine($"Headless   : {!headed}");
            Console.WriteLine($"Target url : {url}");
            Console.WriteLine();

            // 1) Locate the installed Chrome/Chromium.
            string chromePath;
            try
            {
                chromePath = ChromeDriverUpdater.GetChromePath();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[FAIL] Chrome/Chromium not found: {ex.Message}");
                Console.Error.WriteLine("       On Ubuntu install it, e.g.: sudo apt-get install -y google-chrome-stable  (or chromium-browser)");
                return 2;
            }
            Console.WriteLine($"[OK] Chrome binary : {chromePath}");

            // 2) Download the chromedriver matching the installed Chrome version.
            string driverRoot = Path.Combine(AppContext.BaseDirectory, "chromedriver");
            Directory.CreateDirectory(driverRoot);
            Console.WriteLine("     Downloading matching chromedriver ...");
            string driverDir;
            try
            {
                driverDir = await ChromeDriverUpdater.DownloadAsync(driverRoot, chromePath);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[FAIL] chromedriver download failed: {ex}");
                return 3;
            }
            Console.WriteLine($"[OK] chromedriver  : {driverDir}");

            // 3) Build options + service.
            var profile = new ChromeProfile();
            ChromeOptions options = profile.DefaultChromeOptions(chromePath);
            if (!headed) options.AddArgument("--headless=new");
            options.AddArgument("--disable-dev-shm-usage"); // avoid /dev/shm exhaustion on Ubuntu/containers
            options.AddArgument("--window-size=1280,800");

            ChromeDriverService service = ChromeDriverService.CreateDefaultService(driverDir);
            service.HideCommandPromptWindow = true; // no-op on Linux

            // 4) Open, navigate, capture, close.
            Console.WriteLine("     Opening Chrome ...");
            try
            {
                profile.OpenChrome(options, service);

                profile.ChromeDriver!.Navigate().GoToUrl(url);
                await profile.DelayAsync(1500);

                Console.WriteLine($"[OK] Page title    : {profile.ChromeDriver.Title}");
                Console.WriteLine($"[OK] Current url   : {profile.ChromeDriver.Url}");

                string shot = Path.Combine(AppContext.BaseDirectory, "screenshot.png");
                ((ITakesScreenshot)profile.ChromeDriver).GetScreenshot().SaveAsFile(shot);
                Console.WriteLine($"[OK] Screenshot    : {shot}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[FAIL] Selenium run failed: {ex}");
                return 4;
            }
            finally
            {
                Console.WriteLine("     Closing Chrome ...");
                await profile.CloseChromeAsync();
            }

            Console.WriteLine();
            Console.WriteLine("[DONE] All steps passed.");
            return 0;
        }
    }
}
