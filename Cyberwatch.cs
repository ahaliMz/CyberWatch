using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cyberwatch
{
    public enum ThemeMode
    {
        CyberDark,
        CleanLight,
        MidnightBlue,
        MatrixGreen
    }

    public static class UITheme
    {
        public static ThemeMode CurrentTheme = ThemeMode.CyberDark;

        public static ConsoleColor Background
        {
            get
            {
                switch (CurrentTheme)
                {
                    case ThemeMode.CleanLight: return ConsoleColor.White;
                    case ThemeMode.MidnightBlue: return ConsoleColor.DarkBlue;
                    case ThemeMode.MatrixGreen: return ConsoleColor.Black;
                    default: return ConsoleColor.Black;
                }
            }
        }

        public static ConsoleColor Foreground
        {
            get
            {
                switch (CurrentTheme)
                {
                    case ThemeMode.CleanLight: return ConsoleColor.Black;
                    case ThemeMode.MidnightBlue: return ConsoleColor.White;
                    case ThemeMode.MatrixGreen: return ConsoleColor.Green;
                    default: return ConsoleColor.Gray;
                }
            }
        }

        public static ConsoleColor Primary
        {
            get
            {
                switch (CurrentTheme)
                {
                    case ThemeMode.CleanLight: return ConsoleColor.DarkCyan;
                    case ThemeMode.MidnightBlue: return ConsoleColor.Cyan;
                    case ThemeMode.MatrixGreen: return ConsoleColor.Green;
                    default: return ConsoleColor.Cyan;
                }
            }
        }

        public static ConsoleColor Secondary
        {
            get
            {
                switch (CurrentTheme)
                {
                    case ThemeMode.CleanLight: return ConsoleColor.DarkBlue;
                    case ThemeMode.MidnightBlue: return ConsoleColor.Yellow;
                    case ThemeMode.MatrixGreen: return ConsoleColor.DarkGreen;
                    default: return ConsoleColor.DarkCyan;
                }
            }
        }

        public static ConsoleColor Success
        {
            get
            {
                switch (CurrentTheme)
                {
                    case ThemeMode.CleanLight: return ConsoleColor.DarkGreen;
                    case ThemeMode.MidnightBlue: return ConsoleColor.Green;
                    case ThemeMode.MatrixGreen: return ConsoleColor.Green;
                    default: return ConsoleColor.Green;
                }
            }
        }

        public static ConsoleColor Warning
        {
            get
            {
                switch (CurrentTheme)
                {
                    case ThemeMode.CleanLight: return ConsoleColor.DarkYellow;
                    case ThemeMode.MidnightBlue: return ConsoleColor.Yellow;
                    case ThemeMode.MatrixGreen: return ConsoleColor.DarkYellow;
                    default: return ConsoleColor.Yellow;
                }
            }
        }

        public static ConsoleColor Danger
        {
            get
            {
                switch (CurrentTheme)
                {
                    case ThemeMode.CleanLight: return ConsoleColor.DarkRed;
                    case ThemeMode.MidnightBlue: return ConsoleColor.Red;
                    case ThemeMode.MatrixGreen: return ConsoleColor.Red;
                    default: return ConsoleColor.Red;
                }
            }
        }

        public static ConsoleColor Muted
        {
            get
            {
                switch (CurrentTheme)
                {
                    case ThemeMode.CleanLight: return ConsoleColor.DarkGray;
                    case ThemeMode.MidnightBlue: return ConsoleColor.Gray;
                    case ThemeMode.MatrixGreen: return ConsoleColor.DarkGreen;
                    default: return ConsoleColor.DarkGray;
                }
            }
        }

        public static void Apply()
        {
            Console.BackgroundColor = Background;
            Console.ForegroundColor = Foreground;
            Console.Clear();
        }
    }

    class Program
    {
        public static bool SoundAlertsEnabled = true;
        const int LINE_WIDTH = 88;

        static void Main(string[] args)
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
            }
            catch { }

            EnsureAdminPrivileges();
            ConfigureConsoleWindow();
            UITheme.Apply();

            while (true)
            {
                ShowMainMenu();
            }
        }

        static void EnsureAdminPrivileges()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = Process.GetCurrentProcess().MainModule.FileName,
                        UseShellExecute = true,
                        Verb = "runas"
                    };
                    Process.Start(startInfo);
                    Environment.Exit(0);
                }
                catch { }
            }
        }

        static void ConfigureConsoleWindow()
        {
            try
            {
                Console.Title = "CYBERWATCH";
                if (Console.WindowWidth < 100) Console.WindowWidth = 100;
                if (Console.WindowHeight < 36) Console.WindowHeight = 36;
                Console.BufferWidth = 100;
            }
            catch { }
        }

        #region Clean Live Header & UI Helpers
        static void DrawHeader(string subTitle = "")
        {
            UITheme.Apply();
            Console.WriteLine();

            // Line 1: Top Bar with Right-Aligned Live Date Time
            string dateStr = DateTime.Now.ToString("yyyy-MM-dd");
            string timeStr = DateTime.Now.ToString("HH:mm:ss");
            string dateTimeText = string.Format("{0}  {1}", dateStr, timeStr);

            int spaces = LINE_WIDTH - dateTimeText.Length;
            if (spaces < 0) spaces = 0;

            Console.Write("  ");
            Console.Write(new string(' ', spaces));
            WriteColor(dateTimeText, UITheme.Warning, true);

            Console.Write("  ");
            WriteColor(new string('─', LINE_WIDTH), UITheme.Muted);

            // Big Bold CYBERWATCH ASCII Logo
            string[] logo = new string[]
            {
                " ██████╗██╗   ██╗██████╗ ███████╗██████╗ ██╗    ██╗ █████╗ ████████╗ ██████╗██╗  ██╗",
                "██╔════╝╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗██║    ██║██╔══██╗╚══██╔══╝██╔════╝██║  ██║",
                "██║      ╚████╔╝ ██████╔╝█████╗  ██████╔╝██║ █╗ ██║███████║   ██║   ██║     ███████║",
                "██║       ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗██║███╗██║██╔══██║   ██║   ██║     ██╔══██║",
                "╚██████╗   ██║   ██████╔╝███████╗██║  ██║╚███╔███╔╝██║  ██║   ██║   ╚██████╗██║  ██║",
                " ╚═════╝   ╚═╝   ╚═════╝ ╚══════╝╚═╝  ╚═╝ ╚══╝╚══╝ ╚═╝  ╚═╝   ╚═╝    ╚═════╝╚═╝  ╚═╝"
            };

            foreach (var line in logo)
            {
                Console.Write(" ");
                WriteColor(line, UITheme.Primary);
            }

            Console.Write("  ");
            WriteColor(new string('─', LINE_WIDTH), UITheme.Muted);

            // Status Bar Strip
            string host = Environment.MachineName;
            string user = Environment.UserName;
            string localIp = GetPrimaryLocalIP();
            string themeName = UITheme.CurrentTheme.ToString();

            Console.Write("  ");
            WriteColor("Host: ", UITheme.Muted, false);
            WriteColor(host + "   ", UITheme.Foreground, false);
            WriteColor("User: ", UITheme.Muted, false);
            WriteColor(user + "   ", UITheme.Foreground, false);
            WriteColor("IP: ", UITheme.Muted, false);
            WriteColor(localIp + "   ", UITheme.Success, false);
            WriteColor("Theme: ", UITheme.Muted, false);
            WriteColor(themeName, UITheme.Primary, true);

            Console.Write("  ");
            WriteColor(new string('─', LINE_WIDTH), UITheme.Muted);

            if (!string.IsNullOrEmpty(subTitle))
            {
                Console.WriteLine();
                Console.Write("  ");
                WriteColor("►►► " + subTitle.ToUpper(), UITheme.Primary, true);
                Console.Write("  ");
                WriteColor(new string('─', LINE_WIDTH), UITheme.Muted);
            }
        }

        static void UpdateTopRightClock()
        {
            try
            {
                int curLeft = Console.CursorLeft;
                int curTop = Console.CursorTop;
                bool curVisible = Console.CursorVisible;

                Console.CursorVisible = false;
                string dateStr = DateTime.Now.ToString("yyyy-MM-dd");
                string timeStr = DateTime.Now.ToString("HH:mm:ss");
                string dateTimeText = string.Format("{0}  {1}", dateStr, timeStr);

                int targetLeft = Math.Max(0, LINE_WIDTH + 2 - dateTimeText.Length);
                Console.SetCursorPosition(targetLeft, 1);
                WriteColor(dateTimeText, UITheme.Warning, false);

                Console.SetCursorPosition(curLeft, curTop);
                Console.CursorVisible = curVisible;
            }
            catch { }
        }

        static string ReadInputWithLiveClock()
        {
            StringBuilder sb = new StringBuilder();
            DateTime lastUpdate = DateTime.MinValue;

            while (true)
            {
                if ((DateTime.Now - lastUpdate).TotalMilliseconds >= 250)
                {
                    UpdateTopRightClock();
                    lastUpdate = DateTime.Now;
                }

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine();
                        return sb.ToString();
                    }
                    else if (key.Key == ConsoleKey.Backspace)
                    {
                        if (sb.Length > 0)
                        {
                            sb.Remove(sb.Length - 1, 1);
                            Console.Write("\b \b");
                        }
                    }
                    else if (key.Key == ConsoleKey.Escape)
                    {
                        return "";
                    }
                    else if (!char.IsControl(key.KeyChar))
                    {
                        sb.Append(key.KeyChar);
                        Console.Write(key.KeyChar);
                    }
                }
                Thread.Sleep(30);
            }
        }

        public static void WriteColor(string text, ConsoleColor color, bool newLine = true)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = color;
            if (newLine)
                Console.WriteLine(text);
            else
                Console.Write(text);
            Console.ForegroundColor = prev;
        }

        public static void Pause()
        {
            Console.WriteLine();
            WriteColor("  Press any key to return...", UITheme.Muted);
            while (!Console.KeyAvailable)
            {
                UpdateTopRightClock();
                Thread.Sleep(200);
            }
            Console.ReadKey(true);
        }

        static string GetProgressBar(int percentage, int width = 18)
        {
            percentage = Math.Max(0, Math.Min(100, percentage));
            int filled = (percentage * width) / 100;
            int empty = width - filled;
            return "[" + new string('■', filled) + new string('·', empty) + string.Format("] {0,3}%", percentage);
        }

        static ConsoleColor GetThresholdColor(int percent)
        {
            if (percent >= 90) return UITheme.Danger;
            if (percent >= 75) return UITheme.Warning;
            return UITheme.Success;
        }
        #endregion

        #region Main Menu
        static void ShowMainMenu()
        {
            DrawHeader();

            Console.WriteLine();
            Console.Write("  ");
            WriteColor("SYSTEM & HARDWARE", UITheme.Primary, false);
            Console.Write(new string(' ', 44 - "SYSTEM & HARDWARE".Length));
            WriteColor("NETWORK & SECURITY", UITheme.Primary, true);

            Console.Write("  ");
            WriteColor(new string('┄', 39), UITheme.Muted, false);
            Console.Write("     ");
            WriteColor(new string('┄', 39), UITheme.Muted, true);

            PrintMenuItem("[01]", "Hardware & System Health", "[08]", "Fast Network Scanner");
            PrintMenuItem("[02]", "Remote Client Manager", "[09]", "Public IP & Geo Intel");
            PrintMenuItem("[03]", "Network Diagnostics", "[10]", "Live Bandwidth Meter");
            PrintMenuItem("[04]", "Firewall Status & Rules", "[11]", "System Maintenance");
            PrintMenuItem("[05]", "Security & Event Logs", "[12]", "WiFi Password Viewer");
            PrintMenuItem("[06]", "Live Alerts & Alarm", "[13]", "Export HTML Report");
            PrintMenuItem("[07]", "System Health (SFC/CHK)", "[14]", "Quick Network Table");

            Console.WriteLine();
            Console.Write("  ");
            WriteColor(new string('─', LINE_WIDTH), UITheme.Muted);

            Console.Write("  ");
            WriteColor("[T] ", UITheme.Primary, false);
            WriteColor("Switch Theme  ", UITheme.Foreground, false);
            WriteColor("│  ", UITheme.Muted, false);
            WriteColor("[S] ", UITheme.Warning, false);
            WriteColor(string.Format("Sound: {0}  ", SoundAlertsEnabled ? "ON" : "OFF"), UITheme.Foreground, false);
            WriteColor("│  ", UITheme.Muted, false);
            WriteColor("[0] ", UITheme.Danger, false);
            WriteColor("Exit", UITheme.Danger, true);

            Console.Write("  ");
            WriteColor(new string('─', LINE_WIDTH), UITheme.Muted);

            Console.WriteLine();
            Console.Write("  ");
            WriteColor("Select Option > ", UITheme.Primary, false);
            string input = ReadInputWithLiveClock();
            if (input == null) return;
            input = input.Trim().ToUpper();

            switch (input)
            {
                case "1":
                case "01":
                    ServerHealthMenu();
                    break;
                case "2":
                case "02":
                    RemoteClientMenu();
                    break;
                case "3":
                case "03":
                    NetworkDiagnosticsMenu();
                    break;
                case "4":
                case "04":
                    FirewallMenu();
                    break;
                case "5":
                case "05":
                    SecurityLogsMenu();
                    break;
                case "6":
                case "06":
                    AutoMonitoringMenu();
                    break;
                case "7":
                case "07":
                    SystemHealthMenu();
                    break;
                case "8":
                case "08":
                    FastScannerMenu();
                    break;
                case "9":
                case "09":
                    PublicIpThreatMenu();
                    break;
                case "10":
                    LiveBandwidthMonitor();
                    break;
                case "11":
                    MaintenanceMenu();
                    break;
                case "12":
                    WifiSecurityMenu();
                    break;
                case "13":
                    ExportHtmlReport();
                    break;
                case "14":
                    QuickNetworkTable();
                    break;
                case "T":
                    ToggleTheme();
                    break;
                case "S":
                    SoundAlertsEnabled = !SoundAlertsEnabled;
                    if (SoundAlertsEnabled) Console.Beep(1000, 150);
                    break;
                case "0":
                case "EXIT":
                case "Q":
                    ExitApp();
                    break;
                default:
                    break;
            }
        }

        static void PrintMenuItem(string key1, string label1, string key2, string label2)
        {
            Console.Write("  ");
            WriteColor(string.Format("{0,-4} ", key1), UITheme.Primary, false);
            WriteColor(string.Format("{0,-34}", label1), UITheme.Foreground, false);

            Console.Write("     ");
            WriteColor(string.Format("{0,-4} ", key2), UITheme.Primary, false);
            WriteColor(string.Format("{0,-34}", label2), UITheme.Foreground, true);
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
            DrawHeader("GOODBYE");
            Console.WriteLine();
            Console.Write("  ");
            WriteColor("Thank you for using CYBERWATCH. Stay secure!", UITheme.Success);
            Console.WriteLine();
            Thread.Sleep(800);
            Environment.Exit(0);
        }
        #endregion

        #region Module 1: Server Health
        static void ServerHealthMenu()
        {
            DrawHeader("Hardware & System Health");
            Console.WriteLine();

            // CPU Usage
            int cpuPercent = GetCpuUsage();
            Console.Write("  CPU Usage:      ");
            WriteColor(GetProgressBar(cpuPercent), GetThresholdColor(cpuPercent), true);

            // RAM Usage
            long totalMB, freeMB;
            GetRamMetrics(out totalMB, out freeMB);
            long usedMB = totalMB - freeMB;
            int ramPercent = (int)((usedMB * 100) / (totalMB > 0 ? totalMB : 1));
            Console.Write("  Memory (RAM):   ");
            WriteColor(GetProgressBar(ramPercent), GetThresholdColor(ramPercent), false);
            WriteColor(string.Format("  ({0:N0} MB / {1:N0} MB)", usedMB, totalMB), UITheme.Muted, true);

            Console.WriteLine();
            WriteColor("  Storage Drives", UITheme.Primary);
            foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed))
            {
                long dTotal = drive.TotalSize / (1024 * 1024 * 1024);
                long dFree = drive.AvailableFreeSpace / (1024 * 1024 * 1024);
                long dUsed = dTotal - dFree;
                int dPercent = (int)((dUsed * 100) / (dTotal > 0 ? dTotal : 1));

                Console.Write(string.Format("  Drive {0,-3} [{1,-4}] ", drive.Name, drive.DriveFormat));
                WriteColor(GetProgressBar(dPercent, 15), GetThresholdColor(dPercent), false);
                WriteColor(string.Format("  {0} GB / {1} GB ({2} GB free)", dUsed, dTotal, dFree), UITheme.Muted, true);
            }

            Console.WriteLine();
            WriteColor("  System Info", UITheme.Primary);
            TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount);
            Console.Write("  System Uptime:  ");
            WriteColor(string.Format("{0}d {1}h {2}m {3}s", uptime.Days, uptime.Hours, uptime.Minutes, uptime.Seconds), UITheme.Success, true);
            Console.Write("  OS Version:     ");
            WriteColor(string.Format("{0} ({1})", Environment.OSVersion, Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit"), UITheme.Foreground, true);
            Console.Write("  CPU Cores:      ");
            WriteColor(string.Format("{0} Logical Processors", Environment.ProcessorCount), UITheme.Foreground, true);

            Console.WriteLine();
            WriteColor("  Top Processes by Memory", UITheme.Primary);
            Console.WriteLine("  {0,-8} {1,-28} {2,-15} {3,-10}", "PID", "Process Name", "Memory (MB)", "Threads");
            Console.Write("  ");
            WriteColor(new string('─', 65), UITheme.Muted);

            var topProcesses = Process.GetProcesses()
                .OrderByDescending(p => { try { return p.WorkingSet64; } catch { return 0; } })
                .Take(6);

            foreach (var p in topProcesses)
            {
                try
                {
                    double memMB = p.WorkingSet64 / (1024.0 * 1024.0);
                    Console.WriteLine("  {0,-8} {1,-28} {2,-15:N1} {3,-10}", p.Id, Truncate(p.ProcessName, 26), memMB, p.Threads.Count);
                }
                catch { }
            }

            Pause();
        }
        #endregion

        #region Module 2: Remote Client
        static void RemoteClientMenu()
        {
            DrawHeader("Remote Client Management");
            Console.WriteLine();
            Console.Write("  Enter Remote Target (Host or IP): ");
            string target = Console.ReadLine();
            if (string.IsNullOrEmpty(target)) return;
            target = target.Trim();

            DrawHeader(string.Format("Remote Target: {0}", target));
            Console.WriteLine();
            Console.WriteLine("  [1] Ping Latency Test");
            Console.WriteLine("  [2] Check Logged-in Users (quser)");
            Console.WriteLine("  [3] List Shared Folders (net view)");
            Console.WriteLine("  [4] Query Remote Disks");
            Console.WriteLine("  [5] Remote Restart / Shutdown");
            Console.WriteLine("  [0] Back");
            Console.WriteLine();
            Console.Write("  Select > ");
            string choice = Console.ReadLine();
            if (choice == null) return;
            choice = choice.Trim();

            switch (choice)
            {
                case "1":
                    RunPingTest(target, 4);
                    break;
                case "2":
                    RunShellCmd(string.Format("quser /server:{0}", target));
                    break;
                case "3":
                    RunShellCmd(string.Format("net view \\\\{0}", target));
                    break;
                case "4":
                    RunShellCmd(string.Format("wmic /node:\"{0}\" logicaldisk where drivetype=3 get DeviceID,Size,FreeSpace", target));
                    break;
                case "5":
                    RemoteActionMenu(target);
                    break;
            }
            Pause();
        }

        static void RemoteActionMenu(string target)
        {
            Console.WriteLine();
            WriteColor("  [1] Remote RESTART", UITheme.Warning);
            WriteColor("  [2] Remote SHUTDOWN", UITheme.Danger);
            WriteColor("  [3] Cancel Shutdown", UITheme.Success);
            Console.Write("  Select Action > ");
            string act = Console.ReadLine();
            if (act == null) return;
            act = act.Trim();

            if (act == "1")
            {
                RunShellCmd(string.Format("shutdown /m \\\\{0} /r /t 30 /c \"Remote Restart by Admin\"", target));
                WriteColor("  Restart command dispatched (30s timer).", UITheme.Warning);
            }
            else if (act == "2")
            {
                RunShellCmd(string.Format("shutdown /m \\\\{0} /s /t 30 /c \"Remote Shutdown by Admin\"", target));
                WriteColor("  Shutdown command dispatched (30s timer).", UITheme.Danger);
            }
            else if (act == "3")
            {
                RunShellCmd(string.Format("shutdown /m \\\\{0} /a", target));
                WriteColor("  Abort signal dispatched.", UITheme.Success);
            }
        }
        #endregion

        #region Module 3: Network Diagnostics
        static void NetworkDiagnosticsMenu()
        {
            DrawHeader("Network Diagnostics");
            Console.WriteLine();
            Console.Write("  Enter Host or IP (default: 8.8.8.8): ");
            string host = Console.ReadLine();
            if (string.IsNullOrEmpty(host)) host = "8.8.8.8";
            else host = host.Trim();

            DrawHeader(string.Format("Diagnostics: {0}", host));
            Console.WriteLine();
            Console.WriteLine("  [1] Standard Ping (4 Packets)");
            Console.WriteLine("  [2] Continuous Real-time Ping");
            Console.WriteLine("  [3] DNS Record Lookup");
            Console.WriteLine("  [4] Multi-Hop Traceroute");
            Console.WriteLine("  [5] Active Connections (Netstat)");
            Console.WriteLine("  [0] Back");
            Console.WriteLine();
            Console.Write("  Select > ");
            string sub = Console.ReadLine();
            if (sub == null) return;
            sub = sub.Trim();

            switch (sub)
            {
                case "1":
                    RunPingTest(host, 4);
                    break;
                case "2":
                    RunContinuousPing(host);
                    break;
                case "3":
                    RunDnsLookup(host);
                    break;
                case "4":
                    RunTraceroute(host);
                    break;
                case "5":
                    RunNetstatView();
                    break;
            }
            Pause();
        }

        static void RunPingTest(string host, int count)
        {
            Console.WriteLine();
            WriteColor(string.Format("  Sending {0} ICMP Echo requests to {1}...", count, host), UITheme.Primary);
            Console.WriteLine();

            using (var pinger = new Ping())
            {
                int success = 0;
                long totalRtt = 0;

                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        var reply = pinger.Send(host, 2000);
                        if (reply.Status == IPStatus.Success)
                        {
                            success++;
                            totalRtt += reply.RoundtripTime;
                            Console.WriteLine("  Reply from {0}: bytes={1} time={2}ms TTL={3}",
                                reply.Address, reply.Buffer.Length, reply.RoundtripTime, reply.Options != null ? reply.Options.Ttl : 0);
                        }
                        else
                        {
                            WriteColor(string.Format("  Request timed out or failed ({0})", reply.Status), UITheme.Danger);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteColor(string.Format("  Ping error: {0}", ex.Message), UITheme.Danger);
                    }
                    Thread.Sleep(400);
                }

                int loss = (int)(((count - success) / (double)count) * 100);
                Console.WriteLine();
                WriteColor(string.Format("  Packets: Sent = {0}, Received = {1}, Lost = {2} ({3}% loss)", count, success, count - success, loss),
                    loss > 0 ? UITheme.Danger : UITheme.Success);
                if (success > 0)
                {
                    WriteColor(string.Format("  Average Latency: {0} ms", totalRtt / success), UITheme.Primary);
                }
            }
        }

        static void RunContinuousPing(string host)
        {
            Console.WriteLine();
            WriteColor(string.Format("  Continuous Ping to {0} (Press any key to stop)...", host), UITheme.Warning);
            Console.WriteLine();

            using (var pinger = new Ping())
            {
                while (!Console.KeyAvailable)
                {
                    try
                    {
                        var reply = pinger.Send(host, 1500);
                        string ts = DateTime.Now.ToString("HH:mm:ss");
                        if (reply.Status == IPStatus.Success)
                        {
                            long rtt = reply.RoundtripTime;
                            ConsoleColor col = rtt < 50 ? UITheme.Success : (rtt < 120 ? UITheme.Warning : UITheme.Danger);
                            Console.Write("  [{0}] {1,-15}  ", ts, reply.Address);
                            WriteColor(string.Format("{0,4} ms  ", rtt), col, false);
                            int barLen = (int)Math.Min(25, Math.Max(1, rtt / 6));
                            WriteColor(new string('■', barLen), col, true);
                        }
                        else
                        {
                            if (SoundAlertsEnabled) Console.Beep(700, 100);
                            WriteColor(string.Format("  [{0}] Timeout ({1})", ts, reply.Status), UITheme.Danger);
                        }
                    }
                    catch
                    {
                        WriteColor(string.Format("  [{0}] Unreachable Host", DateTime.Now.ToString("HH:mm:ss")), UITheme.Danger);
                    }
                    Thread.Sleep(800);
                }
                while (Console.KeyAvailable) Console.ReadKey(true);
            }
        }

        static void RunDnsLookup(string host)
        {
            Console.WriteLine();
            WriteColor(string.Format("  DNS Lookup: {0}", host), UITheme.Primary);
            Console.WriteLine();

            try
            {
                var entry = Dns.GetHostEntry(host);
                Console.WriteLine("  Hostname: {0}", entry.HostName);
                Console.WriteLine("  IP Addresses:");
                foreach (var ip in entry.AddressList)
                {
                    WriteColor(string.Format("    • {0} ({1})", ip, ip.AddressFamily), UITheme.Success);
                }
            }
            catch (Exception ex)
            {
                WriteColor(string.Format("  DNS Resolution failed: {0}", ex.Message), UITheme.Danger);
            }
        }

        static void RunTraceroute(string host)
        {
            Console.WriteLine();
            WriteColor(string.Format("  Tracing route to {0}...", host), UITheme.Primary);
            Console.WriteLine();

            using (var pinger = new Ping())
            {
                byte[] buffer = new byte[32];
                new Random().NextBytes(buffer);

                for (int ttl = 1; ttl <= 30; ttl++)
                {
                    var options = new PingOptions(ttl, true);
                    try
                    {
                        var reply = pinger.Send(host, 3000, buffer, options);
                        string ip = reply.Address != null ? reply.Address.ToString() : "*";
                        string time = reply.Status == IPStatus.TimedOut ? "*" : string.Format("{0} ms", reply.RoundtripTime);

                        Console.WriteLine("  {0,2}   {1,-10}   {2}", ttl, time, ip);

                        if (reply.Status == IPStatus.Success)
                        {
                            Console.WriteLine();
                            WriteColor("  Trace complete.", UITheme.Success);
                            break;
                        }
                    }
                    catch
                    {
                        Console.WriteLine("  {0,2}   * (timeout)", ttl);
                    }
                }
            }
        }

        static void RunNetstatView()
        {
            Console.WriteLine();
            WriteColor("  Active TCP Connections", UITheme.Primary);
            Console.WriteLine();
            Console.WriteLine("  {0,-6} {1,-22} {2,-22} {3}", "Proto", "Local Address", "Remote Address", "State");
            Console.Write("  ");
            WriteColor(new string('─', 65), UITheme.Muted);

            var props = IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnections = props.GetActiveTcpConnections();

            foreach (var conn in tcpConnections.Take(20))
            {
                ConsoleColor stateCol = conn.State == TcpState.Established ? UITheme.Success : UITheme.Foreground;
                Console.Write("  {0,-6} {1,-22} {2,-22} ", "TCP", conn.LocalEndPoint, conn.RemoteEndPoint);
                WriteColor(conn.State.ToString(), stateCol, true);
            }
            Console.WriteLine();
            WriteColor(string.Format("  Showing 20 of {0} active connections.", tcpConnections.Length), UITheme.Muted);
        }
        #endregion

        #region Module 4: Firewall
        static void FirewallMenu()
        {
            DrawHeader("Firewall Status & Profiles");
            Console.WriteLine();
            RunShellCmd("netsh advfirewall show allprofiles");

            Console.WriteLine();
            Console.WriteLine("  [1] Show All Firewall Rules");
            Console.WriteLine("  [2] Enable All Firewall Profiles");
            Console.WriteLine("  [3] Disable All Firewall Profiles");
            Console.WriteLine("  [0] Back");
            Console.WriteLine();
            Console.Write("  Select > ");
            string sub = Console.ReadLine();
            if (sub == null) return;
            sub = sub.Trim();

            if (sub == "1")
            {
                RunShellCmd("netsh advfirewall firewall show rule name=all");
            }
            else if (sub == "2")
            {
                RunShellCmd("netsh advfirewall set allprofiles state on");
                WriteColor("  Firewall profiles enabled.", UITheme.Success);
            }
            else if (sub == "3")
            {
                RunShellCmd("netsh advfirewall set allprofiles state off");
                WriteColor("  Firewall profiles disabled.", UITheme.Danger);
            }
            Pause();
        }
        #endregion

        #region Module 5: Security Logs
        static void SecurityLogsMenu()
        {
            DrawHeader("Security & Event Log Auditor");
            Console.WriteLine();
            Console.WriteLine("  [1] Failed Login Attempts (Event 4625)");
            Console.WriteLine("  [2] Critical System Errors");
            Console.WriteLine("  [3] Windows Defender / Antivirus Status");
            Console.WriteLine("  [4] Listening Network Ports");
            Console.WriteLine("  [0] Back");
            Console.WriteLine();
            Console.Write("  Select > ");
            string sub = Console.ReadLine();
            if (sub == null) return;
            sub = sub.Trim();

            if (sub == "1")
            {
                DrawHeader("Failed Logins (Event 4625)");
                Console.WriteLine();
                RunShellCmd("wevtutil qe Security /c:10 /rd:true /f:text /q:\"*[System[(EventID=4625)]]\"");
            }
            else if (sub == "2")
            {
                DrawHeader("System Errors & Warnings");
                Console.WriteLine();
                RunShellCmd("wevtutil qe System /c:10 /rd:true /f:text /q:\"*[System[(Level=1 or Level=2)]]\"");
            }
            else if (sub == "3")
            {
                DrawHeader("Antivirus Protection");
                Console.WriteLine();
                RunShellCmd("WMIC /Namespace:\\\\root\\SecurityCenter2 Path AntiVirusProduct Get displayName,productState");
            }
            else if (sub == "4")
            {
                DrawHeader("Listening Network Ports");
                Console.WriteLine();
                Console.WriteLine("  {0,-6} {1,-26} {2}", "Proto", "Local Endpoint", "State");
                Console.Write("  ");
                WriteColor(new string('─', 50), UITheme.Muted);
                var props = IPGlobalProperties.GetIPGlobalProperties();
                foreach (var ep in props.GetActiveTcpListeners().Take(25))
                {
                    Console.Write("  {0,-6} {1,-26} ", "TCP", ep);
                    WriteColor("LISTENING", UITheme.Success, true);
                }
            }
            Pause();
        }
        #endregion

        #region Module 6: Auto Alerts & Alarms
        static void AutoMonitoringMenu()
        {
            DrawHeader("Live Alerts & Threshold Alarm");
            Console.WriteLine();
            Console.WriteLine("  [1] Live All-In-One Status Monitor");
            Console.WriteLine("  [2] CPU Overload Alarm (> 80%)");
            Console.WriteLine("  [3] RAM Exhaustion Alarm (> 90%)");
            Console.WriteLine("  [4] Host Heartbeat Disconnect Watcher");
            Console.WriteLine("  [0] Back");
            Console.WriteLine();
            Console.Write("  Select > ");
            string sub = Console.ReadLine();
            if (sub == null) return;
            sub = sub.Trim();

            switch (sub)
            {
                case "1":
                    RunLiveMonitor();
                    break;
                case "2":
                    RunResourceAlarm(true, 80);
                    break;
                case "3":
                    RunResourceAlarm(false, 90);
                    break;
                case "4":
                    RunHostHeartbeatAlarm();
                    break;
            }
        }

        static void RunLiveMonitor()
        {
            DrawHeader("LIVE MONITOR (Press any key to exit)");
            Console.WriteLine();

            using (var pinger = new Ping())
            {
                while (!Console.KeyAvailable)
                {
                    int cpu = GetCpuUsage();
                    long totalMB, freeMB;
                    GetRamMetrics(out totalMB, out freeMB);
                    long usedMB = totalMB - freeMB;
                    int ramPercent = (int)((usedMB * 100) / (totalMB > 0 ? totalMB : 1));

                    string netStatus = "CHECKING";
                    ConsoleColor netCol = UITheme.Muted;
                    try
                    {
                        var reply = pinger.Send("8.8.8.8", 1000);
                        if (reply.Status == IPStatus.Success)
                        {
                            netStatus = string.Format("ONLINE ({0}ms)", reply.RoundtripTime);
                            netCol = UITheme.Success;
                        }
                        else
                        {
                            netStatus = "DISCONNECTED";
                            netCol = UITheme.Danger;
                            if (SoundAlertsEnabled) Console.Beep(800, 100);
                        }
                    }
                    catch
                    {
                        netStatus = "OFFLINE";
                        netCol = UITheme.Danger;
                    }

                    string ts = DateTime.Now.ToString("HH:mm:ss");
                    Console.Write("  [{0}]  CPU: ", ts);
                    WriteColor(GetProgressBar(cpu, 12), GetThresholdColor(cpu), false);
                    Console.Write("   RAM: ");
                    WriteColor(GetProgressBar(ramPercent, 12), GetThresholdColor(ramPercent), false);
                    Console.Write("   NET: ");
                    WriteColor(netStatus, netCol, true);

                    Thread.Sleep(1500);
                }
                while (Console.KeyAvailable) Console.ReadKey(true);
            }
        }

        static void RunResourceAlarm(bool isCpu, int threshold)
        {
            string label = isCpu ? "CPU" : "RAM";
            DrawHeader(string.Format("ALARM: {0} >= {1}% (Press any key to exit)", label, threshold));
            Console.WriteLine();

            while (!Console.KeyAvailable)
            {
                int val = 0;
                if (isCpu)
                {
                    val = GetCpuUsage();
                }
                else
                {
                    long totalMB, freeMB;
                    GetRamMetrics(out totalMB, out freeMB);
                    long usedMB = totalMB - freeMB;
                    val = (int)((usedMB * 100) / (totalMB > 0 ? totalMB : 1));
                }

                string ts = DateTime.Now.ToString("HH:mm:ss");
                if (val >= threshold)
                {
                    if (SoundAlertsEnabled) Console.Beep(1200, 200);
                    Console.Write("  [{0}] ", ts);
                    WriteColor(string.Format("[ALERT] {0} at {1}% >= {2}%", label, val, threshold), UITheme.Danger, true);
                }
                else
                {
                    Console.Write("  [{0}] {1}: {2}%  ", ts, label, val);
                    WriteColor(GetProgressBar(val, 12), GetThresholdColor(val), true);
                }
                Thread.Sleep(1500);
            }
            while (Console.KeyAvailable) Console.ReadKey(true);
        }

        static void RunHostHeartbeatAlarm()
        {
            Console.Write("  Enter Host/IP to watch: ");
            string host = Console.ReadLine();
            if (string.IsNullOrEmpty(host)) host = "192.168.1.1";
            else host = host.Trim();

            DrawHeader(string.Format("Heartbeat Watcher: {0} (Press any key to exit)", host));
            Console.WriteLine();

            using (var pinger = new Ping())
            {
                while (!Console.KeyAvailable)
                {
                    string ts = DateTime.Now.ToString("HH:mm:ss");
                    try
                    {
                        var reply = pinger.Send(host, 1200);
                        if (reply.Status == IPStatus.Success)
                        {
                            Console.Write("  [{0}] ", ts);
                            WriteColor(string.Format("[ONLINE] {0} ({1} ms)", host, reply.RoundtripTime), UITheme.Success, true);
                        }
                        else
                        {
                            if (SoundAlertsEnabled) Console.Beep(900, 250);
                            Console.Write("  [{0}] ", ts);
                            WriteColor(string.Format("[DISCONNECT] Host {0} is unreachable!", host), UITheme.Danger, true);
                        }
                    }
                    catch
                    {
                        if (SoundAlertsEnabled) Console.Beep(900, 250);
                        WriteColor(string.Format("  [{0}] [DISCONNECT] Error reaching {1}", ts, host), UITheme.Danger, true);
                    }
                    Thread.Sleep(2000);
                }
                while (Console.KeyAvailable) Console.ReadKey(true);
            }
        }
        #endregion

        #region Module 7: System Health
        static void SystemHealthMenu()
        {
            DrawHeader("System Health & Diagnostic Checks");
            Console.WriteLine();

            Console.WriteLine("  Connectivity Verification");
            TestHost("Loopback Interface", "127.0.0.1");
            TestHost("Internet Gateway", "8.8.8.8");
            TestHost("Cloudflare DNS", "1.1.1.1");
            TestHost("Public DNS Resolution", "google.com");

            Console.WriteLine();
            Console.WriteLine("  Essential Windows Services");
            CheckService("W32Time", "Windows Time Service");
            CheckService("Dhcp", "DHCP Client");
            CheckService("Dnscache", "DNS Cache Resolver");
            CheckService("EventLog", "Windows Event Log");
            CheckService("Spooler", "Print Spooler");

            Console.WriteLine();
            Console.WriteLine("  [1] Run SFC System File Checker (sfc /scannow)");
            Console.WriteLine("  [2] Run CheckDisk Scan (chkdsk)");
            Console.WriteLine("  [0] Back");
            Console.WriteLine();
            Console.Write("  Select > ");
            string sub = Console.ReadLine();
            if (sub == null) return;
            sub = sub.Trim();

            if (sub == "1")
            {
                RunShellCmd("sfc /scannow");
            }
            else if (sub == "2")
            {
                RunShellCmd("chkdsk");
            }
            Pause();
        }

        static void TestHost(string name, string host)
        {
            using (var pinger = new Ping())
            {
                try
                {
                    var reply = pinger.Send(host, 1500);
                    if (reply.Status == IPStatus.Success)
                    {
                        Console.Write("  [PASS] {0,-25} -> {1,-15} ", name, host);
                        WriteColor(string.Format("({0} ms)", reply.RoundtripTime), UITheme.Success, true);
                    }
                    else
                    {
                        Console.Write("  [FAIL] {0,-25} -> {1,-15} ", name, host);
                        WriteColor(string.Format("({0})", reply.Status), UITheme.Danger, true);
                    }
                }
                catch
                {
                    Console.Write("  [FAIL] {0,-25} -> {1,-15} ", name, host);
                    WriteColor("(Unreachable)", UITheme.Danger, true);
                }
            }
        }

        static void CheckService(string svcName, string displayName)
        {
            try
            {
                var sc = new System.ServiceProcess.ServiceController(svcName);
                bool running = sc.Status == System.ServiceProcess.ServiceControllerStatus.Running;
                if (running)
                {
                    Console.Write("  [RUNNING] {0,-28} ", displayName);
                    WriteColor(string.Format("({0})", svcName), UITheme.Success, true);
                }
                else
                {
                    Console.Write("  [STOPPED] {0,-28} ", displayName);
                    WriteColor(string.Format("({0})", svcName), UITheme.Danger, true);
                }
            }
            catch
            {
                Console.WriteLine("  [?] Service {0} not found.", svcName);
            }
        }
        #endregion

        #region Module 8: Fast Scanner
        static void FastScannerMenu()
        {
            DrawHeader("Fast Network Scanner");
            Console.WriteLine();
            Console.WriteLine("  [1] Subnet Ping Sweep (Parallel 1-254 IPs in ~1s)");
            Console.WriteLine("  [2] Common Port Scanner (Top Ports in Parallel)");
            Console.WriteLine("  [3] View ARP Table (IP to MAC Mappings)");
            Console.WriteLine("  [4] Security Misconfiguration Audit");
            Console.WriteLine("  [0] Back");
            Console.WriteLine();
            Console.Write("  Select > ");
            string sub = Console.ReadLine();
            if (sub == null) return;
            sub = sub.Trim();

            if (sub == "1")
            {
                RunSubnetPingSweep();
            }
            else if (sub == "2")
            {
                RunFastPortScanner();
            }
            else if (sub == "3")
            {
                DrawHeader("ARP Cache Table");
                Console.WriteLine();
                RunShellCmd("arp -a");
            }
            else if (sub == "4")
            {
                RunVulnerabilityAudit();
            }
            Pause();
        }

        static void RunSubnetPingSweep()
        {
            string defaultSubnet = GetLocalSubnetPrefix();
            Console.Write("  Enter Subnet Prefix (default: {0}): ", defaultSubnet);
            string subnet = Console.ReadLine();
            if (string.IsNullOrEmpty(subnet)) subnet = defaultSubnet;
            else subnet = subnet.Trim();

            DrawHeader(string.Format("Ping Sweep: {0}.1 to {0}.254", subnet));
            Console.WriteLine();
            WriteColor("  Scanning 254 IP addresses simultaneously...", UITheme.Primary);
            Console.WriteLine();

            var activeHosts = new List<Tuple<string, long, string>>();
            var lockObj = new object();
            var sw = Stopwatch.StartNew();

            var tasks = Enumerable.Range(1, 254).Select(async i =>
            {
                string ip = string.Format("{0}.{1}", subnet, i);
                using (var pinger = new Ping())
                {
                    try
                    {
                        var reply = await pinger.SendPingAsync(ip, 1200);
                        if (reply.Status == IPStatus.Success)
                        {
                            string hostName = "";
                            try
                            {
                                var entry = await Dns.GetHostEntryAsync(ip);
                                hostName = entry.HostName;
                            }
                            catch { }

                            lock (lockObj)
                            {
                                activeHosts.Add(Tuple.Create(ip, reply.RoundtripTime, hostName));
                                Console.Write("  • ");
                                WriteColor(string.Format("{0,-16}", ip), UITheme.Success, false);
                                WriteColor(string.Format(" {0,3} ms ", reply.RoundtripTime), UITheme.Primary, false);
                                if (!string.IsNullOrEmpty(hostName))
                                    WriteColor(string.Format(" ({0})", hostName), UITheme.Muted, true);
                                else
                                    Console.WriteLine();
                            }
                        }
                    }
                    catch { }
                }
            });

            Task.WaitAll(tasks.ToArray());
            sw.Stop();

            Console.WriteLine();
            WriteColor(string.Format("  Finished in {0} ms. Found {1} online hosts.", sw.ElapsedMilliseconds, activeHosts.Count), UITheme.Success);
        }

        static void RunFastPortScanner()
        {
            Console.Write("  Enter Target IP to scan: ");
            string target = Console.ReadLine();
            if (string.IsNullOrEmpty(target)) target = "127.0.0.1";
            else target = target.Trim();

            var commonPorts = new Dictionary<int, string>
            {
                { 21, "FTP" }, { 22, "SSH" }, { 23, "Telnet" }, { 25, "SMTP" },
                { 53, "DNS" }, { 80, "HTTP" }, { 110, "POP3" }, { 135, "RPC" },
                { 139, "NetBIOS" }, { 143, "IMAP" }, { 443, "HTTPS" }, { 445, "SMB" },
                { 1433, "MSSQL" }, { 1521, "Oracle" }, { 3306, "MySQL" }, { 3389, "RDP" },
                { 5432, "PostgreSQL" }, { 5900, "VNC" }, { 8080, "HTTP-Alt" }, { 8443, "HTTPS-Alt" }
            };

            DrawHeader(string.Format("Port Scan: {0}", target));
            Console.WriteLine();
            WriteColor("  Scanning common ports in parallel...", UITheme.Primary);
            Console.WriteLine();

            var openPorts = new List<Tuple<int, string>>();
            var lockObj = new object();
            var sw = Stopwatch.StartNew();

            var tasks = commonPorts.Select(async kvp =>
            {
                int port = kvp.Key;
                string svc = kvp.Value;
                using (var client = new TcpClient())
                {
                    try
                    {
                        var connectTask = client.ConnectAsync(target, port);
                        var timeoutTask = Task.Delay(1000);
                        var completed = await Task.WhenAny(connectTask, timeoutTask);
                        if (completed == connectTask && client.Connected)
                        {
                            lock (lockObj)
                            {
                                openPorts.Add(Tuple.Create(port, svc));
                                Console.Write("  • ");
                                WriteColor(string.Format("Port {0,-5} ", port), UITheme.Success, false);
                                WriteColor(string.Format("[{0}]", svc), UITheme.Primary, true);
                            }
                        }
                    }
                    catch { }
                }
            });

            Task.WaitAll(tasks.ToArray());
            sw.Stop();

            Console.WriteLine();
            WriteColor(string.Format("  Finished in {0} ms. {1} open ports detected.", sw.ElapsedMilliseconds, openPorts.Count),
                openPorts.Count > 0 ? UITheme.Success : UITheme.Warning);
        }

        static void RunVulnerabilityAudit()
        {
            DrawHeader("Security Misconfiguration Audit");
            Console.WriteLine();

            // 1. Admin Shares
            Console.Write("  1. Administrative Shares (C$, ADMIN$): ");
            try
            {
                var psi = new ProcessStartInfo("net", "share") { RedirectStandardOutput = true, UseShellExecute = false };
                var p = Process.Start(psi);
                string outStr = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                if (outStr.Contains("ADMIN$") || outStr.Contains("C$"))
                    WriteColor("[WARNING] Default Admin shares are active", UITheme.Warning);
                else
                    WriteColor("[OK] No default admin shares", UITheme.Success);
            }
            catch { Console.WriteLine("N/A"); }

            // 2. UAC Check
            Console.Write("  2. User Account Control (UAC) Status:  ");
            try
            {
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System"))
                {
                    int uac = (int)(key != null ? key.GetValue("EnableLUA", 0) : 0);
                    if (uac == 1)
                        WriteColor("[OK] UAC is Enabled", UITheme.Success);
                    else
                        WriteColor("[WARNING] UAC is DISABLED!", UITheme.Danger);
                }
            }
            catch { Console.WriteLine("Permission Denied"); }

            // 3. Remote Desktop Status
            Console.Write("  3. Remote Desktop (RDP) Service:       ");
            try
            {
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Terminal Server"))
                {
                    int rdp = (int)(key != null ? key.GetValue("fDenyTSConnections", 1) : 1);
                    if (rdp == 0)
                        WriteColor("[INFO] RDP is Enabled (Port 3389)", UITheme.Warning);
                    else
                        WriteColor("[OK] RDP is Disabled", UITheme.Success);
                }
            }
            catch { Console.WriteLine("N/A"); }
        }
        #endregion

        #region Module 9: Public IP & Threat Intel
        static void PublicIpThreatMenu()
        {
            DrawHeader("Public IP & Geolocation");
            Console.WriteLine();
            WriteColor("  Querying public network endpoints...", UITheme.Muted);
            Console.WriteLine();

            try
            {
                using (var client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "Cyberwatch/3.0");
                    string json = client.DownloadString("https://api.ipify.org?format=json");
                    string publicIp = json.Replace("{\"ip\":\"", "").Replace("\"}", "").Trim();

                    Console.Write("  Public IP Address: ");
                    WriteColor(publicIp, UITheme.Success, true);

                    // Fetch Geo info
                    try
                    {
                        string geo = client.DownloadString(string.Format("http://ip-api.com/line/{0}?fields=status,country,regionName,city,isp,org,as", publicIp));
                        var lines = geo.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        if (lines.Length >= 6 && lines[0] == "success")
                        {
                            Console.WriteLine("  Country:           {0}", lines[1]);
                            Console.WriteLine("  Region / City:     {0}, {1}", lines[2], lines[3]);
                            Console.WriteLine("  ISP Provider:      {0}", lines[4]);
                            Console.WriteLine("  Organization:      {0}", lines[5]);
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                WriteColor(string.Format("  Could not reach public IP API: {0}", ex.Message), UITheme.Danger);
            }
            Pause();
        }
        #endregion

        #region Module 10: Live Bandwidth Monitor
        static void LiveBandwidthMonitor()
        {
            DrawHeader("Live Bandwidth & Traffic Meter (Press any key to exit)");
            Console.WriteLine();

            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .ToList();

            if (!interfaces.Any())
            {
                WriteColor("  No active network interface found.", UITheme.Danger);
                Pause();
                return;
            }

            var selectedNic = interfaces.First();
            Console.WriteLine("  Interface: {0} ({1})", selectedNic.Name, selectedNic.Description);
            Console.WriteLine();

            long prevBytesRecv = selectedNic.GetIPStatistics().BytesReceived;
            long prevBytesSent = selectedNic.GetIPStatistics().BytesSent;

            while (!Console.KeyAvailable)
            {
                Thread.Sleep(1000);
                long curRecv = selectedNic.GetIPStatistics().BytesReceived;
                long curSent = selectedNic.GetIPStatistics().BytesSent;

                double rxSpeedKB = (curRecv - prevBytesRecv) / 1024.0;
                double txSpeedKB = (curSent - prevBytesSent) / 1024.0;

                prevBytesRecv = curRecv;
                prevBytesSent = curSent;

                string ts = DateTime.Now.ToString("HH:mm:ss");
                Console.Write("  [{0}]  ", ts);
                Console.Write("RX: ");
                WriteColor(string.Format("{0,7:N1} KB/s ", rxSpeedKB), UITheme.Success, false);
                int rxBar = (int)Math.Min(15, rxSpeedKB / 60);
                WriteColor("[" + new string('▼', rxBar).PadRight(15) + "] ", UITheme.Success, false);

                Console.Write("  TX: ");
                WriteColor(string.Format("{0,7:N1} KB/s ", txSpeedKB), UITheme.Primary, false);
                int txBar = (int)Math.Min(15, txSpeedKB / 60);
                WriteColor("[" + new string('▲', txBar).PadRight(15) + "]", UITheme.Primary, true);
            }
            while (Console.KeyAvailable) Console.ReadKey(true);
        }
        #endregion

        #region Module 11: Maintenance
        static void MaintenanceMenu()
        {
            DrawHeader("System & Network Maintenance");
            Console.WriteLine();
            Console.WriteLine("  [1] Flush DNS Resolver Cache");
            Console.WriteLine("  [2] Full Network Stack Reset (Winsock + IP Reset)");
            Console.WriteLine("  [3] Clean Temporary Cache Files");
            Console.WriteLine("  [4] Query Hardware Drivers");
            Console.WriteLine("  [0] Back");
            Console.WriteLine();
            Console.Write("  Select > ");
            string sub = Console.ReadLine();
            if (sub == null) return;
            sub = sub.Trim();

            if (sub == "1")
            {
                RunShellCmd("ipconfig /flushdns");
                WriteColor("  DNS Cache flushed successfully.", UITheme.Success);
            }
            else if (sub == "2")
            {
                WriteColor("  Resetting network stack...", UITheme.Warning);
                RunShellCmd("netsh winsock reset");
                RunShellCmd("netsh int ip reset");
                RunShellCmd("ipconfig /release");
                RunShellCmd("ipconfig /renew");
                RunShellCmd("ipconfig /flushdns");
                WriteColor("  Network stack reset complete.", UITheme.Success);
            }
            else if (sub == "3")
            {
                CleanTempFiles();
            }
            else if (sub == "4")
            {
                RunShellCmd("driverquery");
            }
            Pause();
        }

        static void CleanTempFiles()
        {
            WriteColor("  Cleaning temporary directories...", UITheme.Primary);
            string userTemp = Path.GetTempPath();
            int deleted = 0;

            try
            {
                var dir = new DirectoryInfo(userTemp);
                foreach (var file in dir.GetFiles())
                {
                    try { file.Delete(); deleted++; } catch { }
                }
                foreach (var sub in dir.GetDirectories())
                {
                    try { sub.Delete(true); deleted++; } catch { }
                }
                WriteColor(string.Format("  Cleaned {0} temporary items.", deleted), UITheme.Success);
            }
            catch (Exception ex)
            {
                WriteColor(string.Format("  Temp note: {0}", ex.Message), UITheme.Muted);
            }
        }
        #endregion

        #region Module 12: WiFi Security
        static void WifiSecurityMenu()
        {
            DrawHeader("WiFi Profiles & Password Keys");
            Console.WriteLine();
            Console.WriteLine("  [1] List Saved WiFi Profiles");
            Console.WriteLine("  [2] View Cleartext Password for WiFi Profile");
            Console.WriteLine("  [0] Back");
            Console.WriteLine();
            Console.Write("  Select > ");
            string sub = Console.ReadLine();
            if (sub == null) return;
            sub = sub.Trim();

            if (sub == "1")
            {
                RunShellCmd("netsh wlan show profiles");
            }
            else if (sub == "2")
            {
                Console.Write("  Enter Exact Profile Name (SSID): ");
                string ssid = Console.ReadLine();
                if (!string.IsNullOrEmpty(ssid))
                {
                    RunShellCmd(string.Format("netsh wlan show profile name=\"{0}\" key=clear", ssid.Trim()));
                }
            }
            Pause();
        }
        #endregion

        #region Module 13: HTML Report
        static void ExportHtmlReport()
        {
            DrawHeader("Export HTML Report");
            Console.WriteLine();

            string reportFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format("Cyberwatch_Report_{0:yyyyMMdd_HHmmss}.html", DateTime.Now));

            int cpu = GetCpuUsage();
            long totalMB, freeMB;
            GetRamMetrics(out totalMB, out freeMB);
            long usedMB = totalMB - freeMB;
            int ramPercent = (int)((usedMB * 100) / (totalMB > 0 ? totalMB : 1));

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang='en'>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='UTF-8'>");
            sb.AppendLine("<title>Cyberwatch Diagnostic Report</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: 'Segoe UI', system-ui, sans-serif; background: #0f172a; color: #e2e8f0; margin: 0; padding: 25px; }");
            sb.AppendLine(".container { max-width: 850px; margin: auto; background: #1e293b; border: 1px solid #334155; border-radius: 8px; padding: 25px; }");
            sb.AppendLine("h1 { color: #38bdf8; border-bottom: 2px solid #334155; padding-bottom: 8px; margin-top: 0; }");
            sb.AppendLine("h2 { color: #4ade80; margin-top: 20px; font-size: 1.15rem; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 10px; }");
            sb.AppendLine("th, td { border: 1px solid #334155; padding: 8px 12px; text-align: left; }");
            sb.AppendLine("th { background: #0f172a; color: #f8fafc; }");
            sb.AppendLine(".badge { padding: 3px 8px; border-radius: 4px; font-weight: 600; font-size: 0.85rem; }");
            sb.AppendLine(".badge-pass { background: #166534; color: #dcfce7; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class='container'>");
            sb.AppendLine("<h1>CYBERWATCH - System & Network Diagnostic Report</h1>");
            sb.AppendLine(string.Format("<p style='color:#94a3b8;'><b>Generated:</b> {0:yyyy-MM-dd HH:mm:ss} | <b>Host:</b> {1} | <b>User:</b> {2}</p>",
                DateTime.Now, Environment.MachineName, Environment.UserName));

            sb.AppendLine("<h2>1. Hardware & Performance</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Parameter</th><th>Value</th><th>Status</th></tr>");
            sb.AppendLine(string.Format("<tr><td>CPU Load</td><td>{0}%</td><td><span class='badge badge-pass'>{1}</span></td></tr>",
                cpu, cpu < 80 ? "OPTIMAL" : "HIGH"));
            sb.AppendLine(string.Format("<tr><td>RAM Usage</td><td>{0:N0} MB / {1:N0} MB ({2}%)</td><td><span class='badge badge-pass'>{3}</span></td></tr>",
                usedMB, totalMB, ramPercent, ramPercent < 90 ? "NORMAL" : "HIGH"));
            sb.AppendLine(string.Format("<tr><td>Operating System</td><td>{0}</td><td><span class='badge badge-pass'>ACTIVE</span></td></tr>",
                Environment.OSVersion));
            sb.AppendLine("</table>");

            sb.AppendLine("<h2>2. Active Storage Drives</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Drive</th><th>Format</th><th>Total</th><th>Free Space</th></tr>");
            foreach (var d in DriveInfo.GetDrives().Where(d => d.IsReady))
            {
                sb.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2} GB</td><td>{3} GB</td></tr>",
                    d.Name, d.DriveFormat, d.TotalSize / (1024 * 1024 * 1024), d.AvailableFreeSpace / (1024 * 1024 * 1024)));
            }
            sb.AppendLine("</table>");

            sb.AppendLine("<h2>3. Network Adapters</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Adapter</th><th>Status</th><th>Speed</th></tr>");
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces().Where(n => n.OperationalStatus == OperationalStatus.Up))
            {
                sb.AppendLine(string.Format("<tr><td>{0}</td><td><span class='badge badge-pass'>{1}</span></td><td>{2} Mbps</td></tr>",
                    nic.Name, nic.OperationalStatus, nic.Speed / 1000000));
            }
            sb.AppendLine("</table>");

            sb.AppendLine("<p style='margin-top: 25px; color: #64748b; text-align: center;'>Generated by Cyberwatch</p>");
            sb.AppendLine("</div></body></html>");

            File.WriteAllText(reportFile, sb.ToString());
            WriteColor(string.Format("  Report created: {0}", reportFile), UITheme.Success);

            try
            {
                Process.Start(new ProcessStartInfo(reportFile) { UseShellExecute = true });
            }
            catch { }
            Pause();
        }
        #endregion

        #region Module 14: Quick Table
        static void QuickNetworkTable()
        {
            DrawHeader("Quick Network Parameters");
            Console.WriteLine();
            RunShellCmd("ipconfig /all");
            Console.WriteLine();
            Console.Write("  ");
            WriteColor(new string('─', LINE_WIDTH), UITheme.Muted);
            RunShellCmd("route print -4");
            Pause();
        }
        #endregion

        #region System Metric Helpers
        static int GetCpuUsage()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("select LoadPercentage from Win32_Processor"))
                {
                    foreach (var item in searcher.Get())
                    {
                        return Convert.ToInt32(item["LoadPercentage"]);
                    }
                }
            }
            catch { }
            return 0;
        }

        static void GetRamMetrics(out long totalMB, out long freeMB)
        {
            totalMB = 0;
            freeMB = 0;
            try
            {
                using (var searcher = new ManagementObjectSearcher("select TotalVisibleMemorySize, FreePhysicalMemory from Win32_OperatingSystem"))
                {
                    foreach (var item in searcher.Get())
                    {
                        totalMB = Convert.ToInt64(item["TotalVisibleMemorySize"]) / 1024;
                        freeMB = Convert.ToInt64(item["FreePhysicalMemory"]) / 1024;
                        return;
                    }
                }
            }
            catch { }
        }

        static string GetPrimaryLocalIP()
        {
            try
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    var endPoint = socket.LocalEndPoint as IPEndPoint;
                    return endPoint != null ? endPoint.Address.ToString() : "127.0.0.1";
                }
            }
            catch
            {
                return "127.0.0.1";
            }
        }

        static string GetLocalSubnetPrefix()
        {
            string ip = GetPrimaryLocalIP();
            var parts = ip.Split('.');
            if (parts.Length == 4)
            {
                return string.Format("{0}.{1}.{2}", parts[0], parts[1], parts[2]);
            }
            return "192.168.1";
        }

        static string Truncate(string val, int maxLen)
        {
            if (string.IsNullOrEmpty(val)) return "";
            return val.Length <= maxLen ? val : val.Substring(0, maxLen - 3) + "..";
        }

        static void RunShellCmd(string cmd)
        {
            try
            {
                var psi = new ProcessStartInfo("cmd.exe", "/c " + cmd)
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using (var p = Process.Start(psi))
                {
                    while (!p.StandardOutput.EndOfStream)
                    {
                        Console.WriteLine("  " + p.StandardOutput.ReadLine());
                    }
                    p.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                WriteColor(string.Format("  Command error: {0}", ex.Message), UITheme.Danger);
            }
        }
        #endregion
    }
}
