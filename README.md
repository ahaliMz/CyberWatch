# CYBERWATCH 🛡️

<div align="center">

```
 ██████╗██╗   ██╗██████╗ ███████╗██████╗ ██╗    ██╗ █████╗ ████████╗ ██████╗██╗  ██╗
██╔════╝╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗██║    ██║██╔══██╗╚══██╔══╝██╔════╝██║  ██║
██║      ╚████╔╝ ██████╔╝█████╗  ██████╔╝██║ █╗ ██║███████║   ██║   ██║     ███████║
██║       ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗██║███╗██║██╔══██║   ██║   ██║     ██╔══██║
╚██████╗   ██║   ██████╔╝███████╗██║  ██║╚███╔███╔╝██║  ██║   ██║   ╚██████╗██║  ██║
 ╚═════╝   ╚═╝   ╚═════╝ ╚══════╝╚═╝  ╚═╝ ╚══╝╚══╝ ╚═╝  ╚═╝   ╚═╝    ╚═════╝╚═╝  ╚═╝
```

**An All-in-One High-Performance Security Operations Center (SOC) & Network Diagnostic Toolkit for Windows.**

[![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011%20%7C%20Server-0078D6?style=for-the-badge&logo=windows)](https://github.com/ahaliMz/CyberWatch)
[![Language](https://img.shields.io/badge/Language-C%23%20%7C%20.NET-512BD4?style=for-the-badge&logo=dotnet)](https://github.com/ahaliMz/CyberWatch)
[![Version](https://img.shields.io/badge/Version-v3.0%20Pro-2ea44f?style=for-the-badge)](https://github.com/ahaliMz/CyberWatch)
[![License](https://img.shields.io/badge/License-MIT-blue.svg?style=for-the-badge)](LICENSE)

</div>

---

## ⚡ Overview

**CYBERWATCH** is a lightweight, fast, standalone CLI security and network diagnostic suite designed for system administrators, security analysts, and network engineers. Built with native C# asynchronous concurrency, CYBERWATCH provides real-time system monitoring, multi-threaded subnet and port scanning, security event log auditing, and diagnostic repair tools with **zero external dependencies**.

---

## ✨ Key Features

| Category | Module | Highlights |
| :--- | :--- | :--- |
| **System & Hardware** | **[01] Hardware Health** | Real-time graphical gauges (`[■■■···]`) for CPU, RAM, Disk partitions, Uptime & Top Resource-consuming processes. |
| **Client Management** | **[02] Remote Control** | Remote ping latency, logged users query (`quser`), shared folders, WMI disk inspection & remote restart/shutdown with confirmation and abort safety. |
| **Network Diagnostics**| **[03] Latency & Route** | ICMP ping tests, continuous real-time ping graph, DNS lookup, 30-hop traceroute & active TCP connections table. |
| **Firewall Management**| **[04] Firewall Profiles** | Instant inspection of Domain, Private, and Public profiles, active rule inspector, and safe enable/disable switches. |
| **Security & Logs** | **[05] Threat Hunter** | Real-time audit of Event 4625 (Failed Logins / Brute Force Hunter), system critical logs, Windows Defender / Antivirus status (`SecurityCenter2`), and listening ports. |
| **Live Automation** | **[06] Alerts & Alarm** | Flicker-free Live Dashboard, configurable CPU/RAM overload thresholds with audio alerts (`Console.Beep`), host heartbeat watcher & `Cyberwatch_Alerts.log` persistence. |
| **System Health** | **[07] Integrity Checks**| Gateway, loopback, and DNS pass/fail verification, critical services monitor (W32Time, DHCP, Spooler), and SFC / CHKDSK launcher. |
| **High-Speed Scanner** | **[08] Async Scanner** | **Subnet Ping Sweep:** Parallel scan of 254 IPs (`1-254`) with optional CSV export.<br>**Port Scanner:** Parallel audit for Top 20 or custom port ranges (`80,443,8000-8010`) with CSV export. |
| **Threat Intelligence**| **[09] Public Identity** | Auto-detects Public IP, ISP/Org, ASN, Region, City, and Country over HTTPS. |
| **Bandwidth Meter** | **[10] Live Traffic** | Flicker-free in-place Download (RX) and Upload (TX) meter with interface picker, peak speed tracking & visual bars (`[▼▼▼...]` / `[▲▲▲...]`). |
| **Maintenance** | **[11] Repair Stack** | DNS Flush, Winsock & TCP/IP stack reset, IP release/renew, and temporary file cache cleaner with confirmation prompts. |
| **WiFi Security** | **[12] Key Auditor** | Lists saved WiFi profiles with numbered selector and reveals cleartext network passwords (`key=clear`). |
| **Audit Reporting** | **[13] HTML Exporter** | Generates a 7-section dark-themed HTML SOC report (Hardware, Drives, NICs, Antivirus, Listening Ports, Firewall & Event 4625) and launches it in the browser. |
| **Network Tables** | **[14] Quick Routing** | Complete `ipconfig /all` and IPv4 routing tables at a glance. |

---

## 🎨 Theme Customization & UI Controls

CYBERWATCH features dynamic theme switching right from the main menu:

- **`T`** : Cycle through themes:
  - 🌙 **Cyber Dark** *(Default dark mode with cyan and neon green accents)*
  - ☀️ **Clean Light** *(Crisp white background with dark blue typography)*
  - 🌌 **Midnight Blue** *(Deep navy SOC aesthetic with golden text)*
  - 🟢 **Matrix Green** *(Classic terminal hacker green)*
- **`S`** : Toggle Audio Beep Alarm on/off.
- **⏱️ Real-time Clock** : Live, second-by-second clock ticker displayed in the top-right corner.

---

## 🚀 Getting Started

### Option 1: Run the Standalone Binary (No installation required)
1. Download or clone this repository:
   ```bash
   git clone https://github.com/ahaliMz/CyberWatch.git
   cd CyberWatch
   ```
2. Double-click **`Cyberwatch.exe`** (or execute from terminal):
   ```cmd
   .\Cyberwatch.exe
   ```
   > *Note: CYBERWATCH will request Administrator privileges on launch (pass `--no-elevate` or decline UAC to run in standard user mode).*

### Option 2: Build & Test from Source
Build using the included script (uses the built-in Windows .NET Framework `csc.exe`, zero SDK required):
```cmd
build.cmd
```
Build **and run unit tests**:
```cmd
build.cmd test
```
Or compile via MSBuild:
```cmd
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe Cyberwatch.csproj /p:Configuration=Release /t:Rebuild
```

---

## 📂 Project Structure

```
CyberWatch/
├── Cyberwatch.exe              # Portable standalone compiled binary
├── Cyberwatch.csproj           # MSBuild project file (.NET Framework 4.5)
├── Cyberwatch.cmd              # Classic batch script fallback
├── app.manifest                # Windows 10/11 compatibility & UAC manifest
├── build.cmd                   # One-click build & unit test runner
├── src/
│   ├── Program.cs              # Entry point, UAC elevation & main menu
│   ├── UITheme.cs              # Themes, header, live clock & console helpers
│   ├── Helpers.cs              # Input validation, process runner & system metrics
│   └── Modules/
│       ├── SystemModules.cs    # Hardware health, remote client, checks & repair
│       ├── NetworkModules.cs   # Diagnostics, async scanner, geo IP & bandwidth
│       └── SecurityModules.cs  # Firewall, logs, AV, alarms, WiFi & HTML report
├── tests/
│   └── Cyberwatch.Tests.cs     # Unit tests for validators, parsers & UI helpers
├── .github/workflows/ci.yml    # GitHub Actions build & test pipeline
├── .gitignore                  # Git ignore configuration
└── README.md                   # Documentation and user manual
```

---

## 🛡️ Requirements
- **OS:** Windows 10, Windows 11, or Windows Server (2016/2019/2022/2025)
- **Framework:** .NET Framework 4.5+ (pre-installed on all modern Windows versions)
- **Permissions:** Run as Administrator (for full firewall, event log, and network stack diagnostics)

---

## ⚖️ License
This project is licensed under the [MIT License](LICENSE) - feel free to modify, distribute, and integrate into your administrative workflows.

---

<div align="center">
  <sub>Built with ❤️ by Abdul Halim</sub>
</div>
