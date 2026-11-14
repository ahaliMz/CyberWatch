using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.ServiceProcess;

namespace Cyberwatch
{
    internal static class SystemModules
    {
        public static void ServerHealthMenu()
        {
            ConsoleUI.DrawHeader("Hardware & System Health");
            Console.WriteLine();

            int cpuPercent = SystemMetrics.GetCpuUsage();
            Console.Write("  CPU Usage:      ");
            ConsoleUI.WriteColor(ConsoleUI.GetProgressBar(cpuPercent), ConsoleUI.GetThresholdColor(cpuPercent), true);

            long totalMB, freeMB;
            SystemMetrics.GetRamMetrics(out totalMB, out freeMB);
            long usedMB = Math.Max(0, totalMB - freeMB);
            int ramPercent = (int)((usedMB * 100) / (totalMB > 0 ? totalMB : 1));
            Console.Write("  Memory (RAM):   ");
            ConsoleUI.WriteColor(ConsoleUI.GetProgressBar(ramPercent), ConsoleUI.GetThresholdColor(ramPercent), false);
            ConsoleUI.WriteColor(string.Format("  ({0:N0} MB / {1:N0} MB)", usedMB, totalMB), UITheme.Muted, true);

            Console.WriteLine();
            ConsoleUI.WriteColor("  Storage Drives", UITheme.Primary);
            const double bytesPerGB = 1024.0 * 1024.0 * 1024.0;
            foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed))
            {
                double dTotal = drive.TotalSize / bytesPerGB;
                double dFree = drive.AvailableFreeSpace / bytesPerGB;
                double dUsed = Math.Max(0, dTotal - dFree);
                int dPercent = dTotal > 0 ? (int)((dUsed * 100.0) / dTotal) : 0;

                Console.Write(string.Format("  Drive {0,-3} [{1,-4}] ", drive.Name, drive.DriveFormat));
                ConsoleUI.WriteColor(ConsoleUI.GetProgressBar(dPercent, 15), ConsoleUI.GetThresholdColor(dPercent), false);
                ConsoleUI.WriteColor(string.Format("  {0:N1} GB / {1:N1} GB ({2:N1} GB free)", dUsed, dTotal, dFree), UITheme.Muted, true);
            }

            Console.WriteLine();
            ConsoleUI.WriteColor("  System Info", UITheme.Primary);
            TimeSpan uptime = SystemMetrics.GetSystemUptime();
            Console.Write("  System Uptime:  ");
            ConsoleUI.WriteColor(string.Format("{0}d {1}h {2}m {3}s", uptime.Days, uptime.Hours, uptime.Minutes, uptime.Seconds), UITheme.Success, true);
            Console.Write("  OS Version:     ");
            ConsoleUI.WriteColor(string.Format("{0} ({1})", Environment.OSVersion, Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit"), UITheme.Foreground, true);
            Console.Write("  CPU Cores:      ");
            ConsoleUI.WriteColor(string.Format("{0} Logical Processors", Environment.ProcessorCount), UITheme.Foreground, true);

            Console.WriteLine();
            ConsoleUI.WriteColor("  Top Processes by Memory", UITheme.Primary);
            Console.WriteLine("  {0,-8} {1,-28} {2,-15} {3,-10}", "PID", "Process Name", "Memory (MB)", "Threads");
            Console.Write("  ");
            ConsoleUI.WriteColor(new string('─', 65), UITheme.Muted);

            var allProcs = Process.GetProcesses();
            try
            {
                var topProcesses = allProcs
                    .Select(p =>
                    {
                        try
                        {
                            return new
                            {
                                Id = p.Id,
                                Name = p.ProcessName,
                                MemBytes = p.WorkingSet64,
                                Threads = p.Threads.Count
                            };
                        }
                        catch
                        {
                            return null;
                        }
                    })
                    .Where(x => x != null)
                    .OrderByDescending(x => x.MemBytes)
                    .Take(6)
                    .ToList();

                foreach (var p in topProcesses)
                {
                    double memMB = p.MemBytes / (1024.0 * 1024.0);
                    Console.WriteLine("  {0,-8} {1,-28} {2,-15:N1} {3,-10}", p.Id, ConsoleUI.Truncate(p.Name, 26), memMB, p.Threads);
                }
            }
            finally
            {
                foreach (var p in allProcs)
                {
                    try { p.Dispose(); } catch { }
                }
            }

            ConsoleUI.Pause();
        }

        public static void RemoteClientMenu()
        {
            string target = PromptRemoteTarget();
            if (target == null) return;

            while (true)
            {
                ConsoleUI.DrawHeader(string.Format("Remote Target: {0}", target));
                Console.WriteLine();
                Console.WriteLine("  [1] Ping Latency Test");
                Console.WriteLine("  [2] Check Logged-in Users (quser)");
                Console.WriteLine("  [3] List Shared Folders (net view)");
                Console.WriteLine("  [4] Query Remote Disks (WMI)");
                Console.WriteLine("  [5] Remote Restart / Shutdown");
                Console.WriteLine("  [6] Change Remote Target");
                Console.WriteLine("  [0] Back to Main Menu");
                Console.WriteLine();
                Console.Write("  Select > ");
                string choice = Console.ReadLine();
                if (choice == null) return;
                choice = choice.Trim();

                if (choice == "0") return;
                if (choice == "6")
                {
                    string nextTarget = PromptRemoteTarget();
                    if (nextTarget != null) target = nextTarget;
                    continue;
                }

                switch (choice)
                {
                    case "1":
                        NetworkModules.RunPingTest(target, 4);
                        ConsoleUI.Pause();
                        break;
                    case "2":
                        ProcessRunner.RunProcess("quser", string.Format("/server:{0}", target));
                        ConsoleUI.Pause();
                        break;
                    case "3":
                        ProcessRunner.RunProcess("net", string.Format("view \\\\{0}", target));
                        ConsoleUI.Pause();
                        break;
                    case "4":
                        QueryRemoteDisks(target);
                        ConsoleUI.Pause();
                        break;
                    case "5":
                        RemoteActionMenu(target);
                        ConsoleUI.Pause();
                        break;
                }
            }
        }

        private static string PromptRemoteTarget()
        {
            ConsoleUI.DrawHeader("Remote Client Management");
            Console.WriteLine();
            Console.Write("  Enter Remote Target (Host or IP): ");
            string target = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(target)) return null;
            target = target.Trim();

            if (!InputValidator.IsValidHostOrIp(target))
            {
                ConsoleUI.WriteColor("  Invalid hostname or IP address.", UITheme.Danger);
                ConsoleUI.Pause();
                return null;
            }
            return target;
        }

        private static void QueryRemoteDisks(string target)
        {
            Console.WriteLine();
            ConsoleUI.WriteColor(string.Format("  Querying logical disks on {0} via WMI...", target), UITheme.Primary);
            try
            {
                var scope = new ManagementScope(string.Format(@"\\{0}\root\cimv2", target));
                scope.Options.Timeout = TimeSpan.FromSeconds(5);
                scope.Connect();

                var query = new ObjectQuery("SELECT DeviceID, Size, FreeSpace FROM Win32_LogicalDisk WHERE DriveType=3");
                using (var searcher = new ManagementObjectSearcher(scope, query))
                using (var results = searcher.Get())
                {
                    Console.WriteLine();
                    Console.WriteLine("  {0,-8} {1,-14} {2,-14}", "Drive", "Total (GB)", "Free (GB)");
                    Console.Write("  ");
                    ConsoleUI.WriteColor(new string('─', 40), UITheme.Muted);

                    const double bytesPerGB = 1024.0 * 1024.0 * 1024.0;
                    foreach (ManagementObject disk in results)
                    {
                        using (disk)
                        {
                            string dev = disk["DeviceID"] != null ? disk["DeviceID"].ToString() : "?";
                            double totalGb = disk["Size"] != null ? Convert.ToDouble(disk["Size"]) / bytesPerGB : 0;
                            double freeGb = disk["FreeSpace"] != null ? Convert.ToDouble(disk["FreeSpace"]) / bytesPerGB : 0;
                            Console.WriteLine("  {0,-8} {1,-14:N1} {2,-14:N1}", dev, totalGb, freeGb);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleUI.WriteColor(string.Format("  Remote WMI query failed: {0}", ex.Message), UITheme.Danger);
            }
        }

        private static void RemoteActionMenu(string target)
        {
            Console.WriteLine();
            ConsoleUI.WriteColor("  [1] Remote RESTART", UITheme.Warning);
            ConsoleUI.WriteColor("  [2] Remote SHUTDOWN", UITheme.Danger);
            ConsoleUI.WriteColor("  [3] Cancel Pending Shutdown", UITheme.Success);
            Console.Write("  Select Action > ");
            string act = Console.ReadLine();
            if (act == null) return;
            act = act.Trim();

            if (act == "1")
            {
                if (!ConsoleUI.ConfirmAction(string.Format("Confirm remote RESTART of {0}?", target)))
                {
                    ConsoleUI.WriteColor("  Action cancelled.", UITheme.Muted);
                    return;
                }
                int exit = ProcessRunner.RunProcess("shutdown", string.Format("/m \\\\{0} /r /t 30 /c \"Remote Restart by Admin\"", target));
                if (exit == 0) ConsoleUI.WriteColor("  Restart command dispatched (30s timer).", UITheme.Warning);
            }
            else if (act == "2")
            {
                if (!ConsoleUI.ConfirmAction(string.Format("Confirm remote SHUTDOWN of {0}?", target)))
                {
                    ConsoleUI.WriteColor("  Action cancelled.", UITheme.Muted);
                    return;
                }
                int exit = ProcessRunner.RunProcess("shutdown", string.Format("/m \\\\{0} /s /t 30 /c \"Remote Shutdown by Admin\"", target));
                if (exit == 0) ConsoleUI.WriteColor("  Shutdown command dispatched (30s timer).", UITheme.Danger);
            }
            else if (act == "3")
            {
                int exit = ProcessRunner.RunProcess("shutdown", string.Format("/m \\\\{0} /a", target));
                if (exit == 0) ConsoleUI.WriteColor("  Abort signal dispatched.", UITheme.Success);
            }
        }

        public static void SystemHealthMenu()
        {
            while (true)
            {
                ConsoleUI.DrawHeader("System Health & Diagnostic Checks");
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
                Console.WriteLine("  [3] Refresh Checks");
                Console.WriteLine("  [0] Back to Main Menu");
                Console.WriteLine();
                Console.Write("  Select > ");
                string sub = Console.ReadLine();
                if (sub == null) return;
                sub = sub.Trim();

                if (sub == "0") return;
                if (sub == "1")
                {
                    ProcessRunner.RunProcess("sfc", "/scannow");
                    ConsoleUI.Pause();
                }
                else if (sub == "2")
                {
                    ProcessRunner.RunProcess("chkdsk", "");
                    ConsoleUI.Pause();
                }
            }
        }

        public static void CheckService(string svcName, string displayName)
        {
            try
            {
                using (var sc = new ServiceController(svcName))
                {
                    bool running = sc.Status == ServiceControllerStatus.Running;
                    if (running)
                    {
                        Console.Write("  [RUNNING] {0,-28} ", displayName);
                        ConsoleUI.WriteColor(string.Format("({0})", svcName), UITheme.Success, true);
                    }
                    else
                    {
                        Console.Write("  [STOPPED] {0,-28} ", displayName);
                        ConsoleUI.WriteColor(string.Format("({0})", svcName), UITheme.Danger, true);
                    }
                }
            }
            catch
            {
                Console.WriteLine("  [?] Service {0} not found.", svcName);
            }
        }

        private static void TestHost(string name, string host)
        {
            using (var pinger = new Ping())
            {
                try
                {
                    var reply = pinger.Send(host, 1500);
                    if (reply.Status == IPStatus.Success)
                    {
                        Console.Write("  [PASS] {0,-25} -> {1,-15} ", name, host);
                        ConsoleUI.WriteColor(string.Format("({0} ms)", reply.RoundtripTime), UITheme.Success, true);
                    }
                    else
                    {
                        Console.Write("  [FAIL] {0,-25} -> {1,-15} ", name, host);
                        ConsoleUI.WriteColor(string.Format("({0})", reply.Status), UITheme.Danger, true);
                    }
                }
                catch
                {
                    Console.Write("  [FAIL] {0,-25} -> {1,-15} ", name, host);
                    ConsoleUI.WriteColor("(Unreachable)", UITheme.Danger, true);
                }
            }
        }

        public static void MaintenanceMenu()
        {
            while (true)
            {
                ConsoleUI.DrawHeader("System & Network Maintenance");
                Console.WriteLine();
                Console.WriteLine("  [1] Flush DNS Resolver Cache");
                Console.WriteLine("  [2] Full Network Stack Reset (Winsock + IP Reset)");
                Console.WriteLine("  [3] Clean Temporary Cache Files");
                Console.WriteLine("  [4] Query Hardware Drivers");
                Console.WriteLine("  [0] Back to Main Menu");
                Console.WriteLine();
                Console.Write("  Select > ");
                string sub = Console.ReadLine();
                if (sub == null) return;
                sub = sub.Trim();

                if (sub == "0") return;

                if (sub == "1")
                {
                    int exit = ProcessRunner.RunProcess("ipconfig", "/flushdns");
                    if (exit == 0) ConsoleUI.WriteColor("  DNS Cache flushed successfully.", UITheme.Success);
                    ConsoleUI.Pause();
                }
                else if (sub == "2")
                {
                    if (ConsoleUI.ConfirmAction("Reset Winsock & TCP/IP stack and renew IP? Active connections will drop."))
                    {
                        ConsoleUI.WriteColor("  Resetting network stack...", UITheme.Warning);
                        ProcessRunner.RunProcess("netsh", "winsock reset");
                        ProcessRunner.RunProcess("netsh", "int ip reset");
                        ProcessRunner.RunProcess("ipconfig", "/release");
                        ProcessRunner.RunProcess("ipconfig", "/renew");
                        ProcessRunner.RunProcess("ipconfig", "/flushdns");
                        ConsoleUI.WriteColor("  Network stack reset complete.", UITheme.Success);
                    }
                    else
                    {
                        ConsoleUI.WriteColor("  Network stack reset cancelled.", UITheme.Muted);
                    }
                    ConsoleUI.Pause();
                }
                else if (sub == "3")
                {
                    if (ConsoleUI.ConfirmAction(string.Format("Delete temporary files in {0}?", Path.GetTempPath())))
                    {
                        CleanTempFiles();
                    }
                    else
                    {
                        ConsoleUI.WriteColor("  Temp cleanup cancelled.", UITheme.Muted);
                    }
                    ConsoleUI.Pause();
                }
                else if (sub == "4")
                {
                    ProcessRunner.RunProcess("driverquery", "");
                    ConsoleUI.Pause();
                }
            }
        }

        private static void CleanTempFiles()
        {
            ConsoleUI.WriteColor("  Cleaning temporary directories...", UITheme.Primary);
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
                ConsoleUI.WriteColor(string.Format("  Cleaned {0} temporary items.", deleted), UITheme.Success);
            }
            catch (Exception ex)
            {
                ConsoleUI.WriteColor(string.Format("  Temp note: {0}", ex.Message), UITheme.Muted);
            }
        }
    }
}

