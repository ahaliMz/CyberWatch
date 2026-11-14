using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

[assembly: AssemblyTitle("Cyberwatch")]
[assembly: AssemblyDescription("SOC & Network Diagnostic Toolkit for Windows")]
[assembly: AssemblyProduct("Cyberwatch v3.0 Pro")]
[assembly: AssemblyCopyright("Copyright (c) 2026 ahaliMz")]
[assembly: AssemblyVersion("3.0.0.0")]
[assembly: AssemblyFileVersion("3.0.0.0")]
[assembly: InternalsVisibleTo("Cyberwatch.Tests")]

namespace Cyberwatch
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
            }
            catch { }

            try
            {
                ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;
            }
            catch { }

            EnsureAdminPrivileges(args);
            ConfigureConsoleWindow();
            UITheme.Apply();

            while (true)
            {
                ShowMainMenu();
            }
        }

        static void EnsureAdminPrivileges(string[] args)
        {
            if (SystemMetrics.IsAdministrator()) return;
            if (args != null && args.Any(a => string.Equals(a, "--no-elevate", StringComparison.OrdinalIgnoreCase)))
                return;

            try
            {
                var module = Process.GetCurrentProcess().MainModule;
                if (module != null && !string.IsNullOrEmpty(module.FileName))
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = module.FileName,
                        Arguments = "--no-elevate",
                        UseShellExecute = true,
                        Verb = "runas"
                    };
                    Process.Start(startInfo);
                    Environment.Exit(0);
                }
            }
            catch { }
        }

        static void ConfigureConsoleWindow()
        {
            try
            {
                Console.Title = "CYBERWATCH v3.0";
                if (Console.BufferWidth < 100) Console.BufferWidth = 100;
                if (Console.WindowWidth < 100) Console.WindowWidth = 100;
                if (Console.WindowHeight < 36) Console.WindowHeight = 36;
            }
            catch { }
        }

        static void ShowMainMenu()
        {
            ConsoleUI.DrawHeader();

            Console.WriteLine();
            Console.Write("  ");
            ConsoleUI.WriteColor("SYSTEM & HARDWARE", UITheme.Primary, false);
            Console.Write(new string(' ', 44 - "SYSTEM & HARDWARE".Length));
            ConsoleUI.WriteColor("NETWORK & SECURITY", UITheme.Primary, true);

            Console.Write("  ");
            ConsoleUI.WriteColor(new string('┄', 39), UITheme.Muted, false);
            Console.Write("     ");
            ConsoleUI.WriteColor(new string('┄', 39), UITheme.Muted, true);

            PrintMenuItem("[01]", "Hardware & System Health", "[08]", "Fast Network Scanner");
            PrintMenuItem("[02]", "Remote Client Manager", "[09]", "Public IP & Geo Intel");
            PrintMenuItem("[03]", "Network Diagnostics", "[10]", "Live Bandwidth Meter");
            PrintMenuItem("[04]", "Firewall Status & Rules", "[11]", "System Maintenance");
            PrintMenuItem("[05]", "Security & Event Logs", "[12]", "WiFi Password Viewer");
            PrintMenuItem("[06]", "Live Alerts & Alarm", "[13]", "Export HTML Report");
            PrintMenuItem("[07]", "System Health (SFC/CHK)", "[14]", "Quick Network Table");

            Console.WriteLine();
            Console.Write("  ");
            ConsoleUI.WriteColor(new string('─', ConsoleUI.LineWidth), UITheme.Muted);

            Console.Write("  ");
            ConsoleUI.WriteColor("[T] ", UITheme.Primary, false);
            ConsoleUI.WriteColor("Switch Theme  ", UITheme.Foreground, false);
            ConsoleUI.WriteColor("│  ", UITheme.Muted, false);
            ConsoleUI.WriteColor("[S] ", UITheme.Warning, false);
            ConsoleUI.WriteColor(string.Format("Sound: {0}  ", ConsoleUI.SoundAlertsEnabled ? "ON" : "OFF"), UITheme.Foreground, false);
            ConsoleUI.WriteColor("│  ", UITheme.Muted, false);
            ConsoleUI.WriteColor("[0] ", UITheme.Danger, false);
            ConsoleUI.WriteColor("Exit", UITheme.Danger, true);

            Console.Write("  ");
            ConsoleUI.WriteColor(new string('─', ConsoleUI.LineWidth), UITheme.Muted);

            Console.WriteLine();
            Console.Write("  ");
            ConsoleUI.WriteColor("Select Option > ", UITheme.Primary, false);
            string input = ConsoleUI.ReadInputWithLiveClock();
            if (input == null)
            {
                Environment.Exit(0);
                return;
            }
            input = input.Trim().ToUpperInvariant();

            switch (input)
            {
                case "1":
                case "01":
                    SystemModules.ServerHealthMenu();
                    break;
                case "2":
                case "02":
                    SystemModules.RemoteClientMenu();
                    break;
                case "3":
                case "03":
                    NetworkModules.NetworkDiagnosticsMenu();
                    break;
                case "4":
                case "04":
                    SecurityModules.FirewallMenu();
                    break;
                case "5":
                case "05":
                    SecurityModules.SecurityLogsMenu();
                    break;
                case "6":
                case "06":
                    SecurityModules.AutoMonitoringMenu();
                    break;
                case "7":
                case "07":
                    SystemModules.SystemHealthMenu();
                    break;
                case "8":
                case "08":
                    NetworkModules.FastScannerMenu();
                    break;
                case "9":
                case "09":
                    NetworkModules.PublicIpThreatMenu();
                    break;
                case "10":
                    NetworkModules.LiveBandwidthMonitor();
                    break;
                case "11":
                    SystemModules.MaintenanceMenu();
                    break;
                case "12":
                    SecurityModules.WifiSecurityMenu();
                    break;
                case "13":
                    SecurityModules.ExportHtmlReport();
                    break;
                case "14":
                    NetworkModules.QuickNetworkTable();
                    break;
                case "T":
                    ToggleTheme();
                    break;
                case "S":
                    ConsoleUI.SoundAlertsEnabled = !ConsoleUI.SoundAlertsEnabled;
                    ConsoleUI.SafeBeep(1000, 150);
                    break;
                case "0":
                case "EXIT":
                case "Q":
                    ExitApp();
                    break;
            }
        }

        static void PrintMenuItem(string key1, string label1, string key2, string label2)
        {
            Console.Write("  ");
            ConsoleUI.WriteColor(string.Format("{0,-4} ", key1), UITheme.Primary, false);
            ConsoleUI.WriteColor(string.Format("{0,-34}", label1), UITheme.Foreground, false);

            Console.Write("     ");
            ConsoleUI.WriteColor(string.Format("{0,-4} ", key2), UITheme.Primary, false);
            ConsoleUI.WriteColor(string.Format("{0,-34}", label2), UITheme.Foreground, true);
        }

        static void ToggleTheme()
        {
            switch (UITheme.CurrentTheme)
            {
                case ThemeMode.CyberDark:
                    UITheme.CurrentTheme = ThemeMode.CleanLight;
                    break;
                case ThemeMode.CleanLight:
                    UITheme.CurrentTheme = ThemeMode.MidnightBlue;
                    break;
                case ThemeMode.MidnightBlue:
                    UITheme.CurrentTheme = ThemeMode.MatrixGreen;
                    break;
                case ThemeMode.MatrixGreen:
                default:
                    UITheme.CurrentTheme = ThemeMode.CyberDark;
                    break;
            }
            UITheme.Apply();
        }

        static void ExitApp()
        {
            ConsoleUI.DrawHeader("GOODBYE");
            Console.WriteLine();
            Console.Write("  ");
            ConsoleUI.WriteColor("Thank you for using CYBERWATCH. Stay secure!", UITheme.Success);
            Console.WriteLine();
            Thread.Sleep(600);
            Environment.Exit(0);
        }
    }
}

