# WgcfGUI: WireGuard Native Config Generator

WgcfGUI is a native, .NET 8 WPF Fluent-Design standalone desktop application that completely reverse-engineers the Cloudflare Warp API protocols and WireGuard Ed25519 cryptography to natively synthesize cross-platform WireGuard configurations `(.conf)`.

Originally a wrapper for the `wgcf` CLI, **WgcfGUI has severed all external binary dependencies.** Everything runs purely in-memory within C#, preventing credential leakage to the disk and significantly stabilizing the tunnel compiling flow.

## Features:
- **Zero-Dependency Native Execution:** Pure C# implementation of the Cloudflare Warp Edge handshakes. Fully standalone `.exe`. No CLI needed.
- **WireGuard Configuration Profiler:** Builds standard Wireguard configuration blocks instantly based on your local Warp footprint. 
- **Mobile QR Code Exporter:** Uses Native QRCoder libraries to display an on-screen QR Code so you can scan your tunnel configuration aggressively into iOS or Android without file sharing.
- **Advanced DNS Targeting:** Easily define custom DNS endpoints (Standard, Malware Blocking, Family filters) using routing stacks from Cloudflare, Quad9, Google, and AdGuard.
- **Leak Protection (Kill Switch):** Built-in native Windows PowerShell routing injections (`PreUp / PostDown`) to aggressively kill all un-tunneled network packets if the VPN tunnel collapses. 
- **Endpoint Spoofing:** Change the actual target Edge Server mapping to `IPv4 Direct`, `IPv6 Direct`, `Port 500` or bespoke Custom IP domains to override local ISP throttles and deep packet inspections.
- **Smart TV Optimization:** Includes a massive Android Exception Matrix built for Amazon Fire TV, Android TV, Nvidia Shield, and others (Bypassing Casting, System Updates, Apple AirPlay etc).
- **MTU Preset Engine:** Quickly toggle between compatibility (1280), standard (1420), and aggressive (1212) MTU presets to solve fragmentation issues.
- **Workflow Automation:** Optional "Auto-Copy to Clipboard" on save ensures your config is ready for immediate pasting into your tunnel client.
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
Fully harnesses the `WPF.UI` library utilizing true Windows 11 Fluent semantics (Mica backdrop, pure theming, dynamic animations).

## Acknowledgments
This project natively implements logic and protocols heavily inspired by the original **[wgcf](https://github.com/ViRb3/wgcf)** (an unofficial cross-platform CLI for Cloudflare Warp).

## Disclaimer
This is a **community project** and is **not affiliated with Cloudflare** in any way. WireGuard is a registered trademark of Jason A. Donenfeld.
