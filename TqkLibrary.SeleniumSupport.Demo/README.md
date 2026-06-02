# TqkLibrary.SeleniumSupport.Demo

Cross-platform smoke test (Windows + Linux/macOS). Targets plain `net8.0` so the *same* build
runs everywhere. It locates Chrome, downloads the matching chromedriver, opens a page headless,
prints the title and saves `screenshot.png`, then closes Chrome.

## Run

```bash
dotnet run --project TqkLibrary.SeleniumSupport.Demo -- [url] [--headed]
```

- `url`       optional, default `https://example.com`
- `--headed`  show a visible window (default: headless — required on a headless/server Ubuntu)

Exit code `0` = all steps passed.

## Ubuntu setup

Need .NET 8 SDK + a Chrome/Chromium binary.

```bash
# .NET 8 SDK
sudo apt-get update && sudo apt-get install -y dotnet-sdk-8.0

# Chrome (option A: Google Chrome stable)
wget -q https://dl.google.com/linux/direct/google-chrome-stable_current_amd64.deb
sudo apt-get install -y ./google-chrome-stable_current_amd64.deb

# Chrome (option B: Chromium from apt)
# sudo apt-get install -y chromium-browser

# run (headless is the default; keep it on a server with no display)
dotnet run --project TqkLibrary.SeleniumSupport.Demo
```

The demo searches these paths on Linux (first match wins):
`/usr/bin/google-chrome`, `/usr/bin/google-chrome-stable`, `/usr/bin/chromium`,
`/usr/bin/chromium-browser`, `/snap/bin/chromium`.

## Notes

- `DefaultChromeOptions()` already adds `--no-sandbox`, and the demo adds
  `--disable-dev-shm-usage` — both needed when running as root / in a container.
- chromedriver is downloaded under `bin/.../chromedriver/<version>/` and `chmod +x`'d
  automatically on Linux/macOS.
- On Linux the Chrome version is read by running `chrome --version` (ELF binaries carry no
  file-version metadata), so the matching chromedriver platform is `linux64`.
