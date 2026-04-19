# WgcfGUI: WireGuard Native Config Generator

WgcfGUI is a native, .NET 8 WPF Fluent-Design standalone desktop application that uses reverse engineered Cloudflare Warp API protocols and WireGuard Ed25519 cryptography to create cross-platform WireGuard configurations `(.conf)`.

Originally designed as a wrapper for the `wgcf` CLI, **WgcfGUI has severed all external binary dependencies** as a standalone application. 

## Features:
- **Zero-Dependency Native Execution:** Pure C# implementation of the Cloudflare Warp Edge handshakes. Fully standalone `.exe`. No CLI needed.
- **WireGuard Configuration Profiler:** Builds standard Wireguard configuration blocks instantly based on your local Warp footprint. 
- **Mobile QR Code Exporter:** Uses Native QRCoder libraries to display an on-screen QR Code so you can scan your tunnel configuration aggressively into iOS or Android without file sharing.
- **Advanced DNS Targeting:** Easily define custom DNS endpoints (Standard, Malware Blocking, Family filters) using routing stacks from Cloudflare, Quad9, Google, and AdGuard.
- **Endpoint Spoofing:** Change the actual target Edge Server mapping to `IPv4 Direct`, `IPv6 Direct`, `Port 500` or bespoke Custom IP domains to override local ISP throttles and deep packet inspections.
- **Smart TV Optimization:** Includes an Android Exception Matrix built for Amazon Fire TV and Android/Google TV (Bypassing Casting, System Updates, Apple AirPlay etc).
- **Enhanced Connection Diagnostics:** Verifies your live Warp status and public IP with a one-click trace, featuring simple export mapping for troubleshooting.

## Getting Started:
1. Hit **Register New Account** to automatically compute your cryptographic pairs and bind to a Cloudflare Free Account. (You can also apply your Warp+ license here if desired).
2. Tweak your desired parameters (Exclude Apps, IPv6 suppression, DNS logic).
3. Hit **Generate & Save (.conf)** to write your profile, **Copy to Clipboard**, or **Generate Mobile QR Code**.

## Compilation
Requires Visual Studio 2022 or the `.NET 8.0 SDK`.

To build the super-lightweight standalone executable:
```powershell
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false -o ../Build
```
This command compiles the project, strips out all debugging junk, and drops the ready-to-use `WgcfGUI.exe` into a new `Build` folder.

## UI/UX
Untilizes the `WPF.UI` library for a Windows 11 Fluent feel (Mica backdrop, pure theming, dynamic animations).

## Acknowledgments
This project natively implements logic and protocols heavily inspired by the original **[wgcf](https://github.com/ViRb3/wgcf)** (an unofficial cross-platform CLI for Cloudflare Warp).

## Disclaimer
This is a **community project** and is **not affiliated with Cloudflare** in any way. WireGuard is a registered trademark of Jason A. Donenfeld.
