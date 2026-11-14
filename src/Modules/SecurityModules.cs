using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace Cyberwatch
{
    internal static class SecurityModules
    {
        public static void FirewallMenu()
        {
            while (true)
            {
                ConsoleUI.DrawHeader("Firewall Status & Profiles");
                Console.WriteLine();
                ProcessRunner.RunProcess("netsh", "advfirewall show allprofiles");

                Console.WriteLine();
                Console.WriteLine("  [1] Show All Firewall Rules");
                Console.WriteLine("  [2] Enable All Firewall Profiles");
                Console.WriteLine("  [3] Disable All Firewall Profiles");
                Console.WriteLine("  [0] Back to Main Menu");
                Console.WriteLine();
                Console.Write("  Select > ");
                string sub = Console.ReadLine();
                if (sub == null) return;
                sub = sub.Trim();

                if (sub == "0") return;

                if (sub == "1")
                {
                    ProcessRunner.RunProcess("netsh", "advfirewall firewall show rule name=all");
                    ConsoleUI.Pause();
                }
                else if (sub == "2")
                {
                    int exit = ProcessRunner.RunProcess("netsh", "advfirewall set allprofiles state on");
                    if (exit == 0) ConsoleUI.WriteColor("  Firewall profiles enabled.", UITheme.Success);
                    ConsoleUI.Pause();
                }
                else if (sub == "3")
                {
                    if (ConsoleUI.ConfirmAction("Disable ALL Windows Firewall profiles? This leaves the host unprotected."))
                    {
                        int exit = ProcessRunner.RunProcess("netsh", "advfirewall set allprofiles state off");
                        if (exit == 0)
                        {
                            ConsoleUI.WriteColor("  Firewall profiles disabled.", UITheme.Danger);
                            SystemMetrics.LogAlert("FIREWALL", "All Windows Firewall profiles were disabled.");
                        }
                    }
                    else
                    {
                        ConsoleUI.WriteColor("  Cancelled. Firewall state unchanged.", UITheme.Muted);
                    }
                    ConsoleUI.Pause();
                }
            }
        }

        public static void SecurityLogsMenu()
        {
            while (true)
            {
                ConsoleUI.DrawHeader("Security & Event Log Auditor");
                Console.WriteLine();
                Console.WriteLine("  [1] Failed Login Attempts (Event 4625)");
                Console.WriteLine("  [2] Critical System Errors");
                Console.WriteLine("  [3] Windows Defender / Antivirus Status");
                Console.WriteLine("  [4] Listening Network Ports");
                Console.WriteLine("  [0] Back to Main Menu");
                Console.WriteLine();
                Console.Write("  Select > ");
                string sub = Console.ReadLine();
                if (sub == null) return;
                sub = sub.Trim();

                if (sub == "0") return;

                if (sub == "1")
                {
                    ConsoleUI.DrawHeader("Failed Logins (Event 4625)");
                    Console.WriteLine();
                    ProcessRunner.RunProcess("wevtutil", "qe Security /c:10 /rd:true /f:text /q:\"*[System[(EventID=4625)]]\"");
                    ConsoleUI.Pause();
                }
                else if (sub == "2")
                {
                    ConsoleUI.DrawHeader("System Errors & Warnings");
                    Console.WriteLine();
                    ProcessRunner.RunProcess("wevtutil", "qe System /c:10 /rd:true /f:text /q:\"*[System[(Level=1 or Level=2)]]\"");
                    ConsoleUI.Pause();
                }
                else if (sub == "3")
                {
                    ConsoleUI.DrawHeader("Antivirus Protection");
                    Console.WriteLine();
                    CheckAntivirusStatus();
                    ConsoleUI.Pause();
                }
                else if (sub == "4")
                {
                    ConsoleUI.DrawHeader("Listening Network Ports");
                    Console.WriteLine();
                    Console.WriteLine("  {0,-6} {1,-26} {2}", "Proto", "Local Endpoint", "State");
                    Console.Write("  ");
                    ConsoleUI.WriteColor(new string('─', 50), UITheme.Muted);
                    var props = IPGlobalProperties.GetIPGlobalProperties();
                    var listeners = props.GetActiveTcpListeners();
                    foreach (var ep in listeners.Take(25))
                    {
                        Console.Write("  {0,-6} {1,-26} ", "TCP", ep);
                        ConsoleUI.WriteColor("LISTENING", UITheme.Success, true);
                    }
                    Console.WriteLine();
                    ConsoleUI.WriteColor(string.Format("  Showing {0} of {1} listening TCP ports.", Math.Min(25, listeners.Length), listeners.Length), UITheme.Muted);
                    ConsoleUI.Pause();
                }
            }
        }

        private static void CheckAntivirusStatus()
        {
            var items = SystemMetrics.GetAntivirusStatuses();
            if (items.Count == 0)
            {
                ConsoleUI.WriteColor("  No antivirus provider reported.", UITheme.Warning);
                return;
            }
            foreach (var av in items)
            {
                Console.Write("  • {0,-34} ", av.Name);
                ConsoleUI.WriteColor(av.Enabled ? "[ENABLED]" : "[DISABLED]", av.Enabled ? UITheme.Success : UITheme.Danger, false);
                Console.Write("  ");
                ConsoleUI.WriteColor(av.UpToDate ? "Signatures: Up-to-date" : "Signatures: Outdated", av.UpToDate ? UITheme.Success : UITheme.Warning, true);
            }
        }

        public static void AutoMonitoringMenu()
        {
            while (true)
            {
                ConsoleUI.DrawHeader("Live Alerts & Threshold Alarm");
                Console.WriteLine();
                Console.WriteLine("  [1] Live All-In-One Status Monitor (In-Place Dashboard)");
                Console.WriteLine("  [2] CPU Overload Alarm (Configurable Threshold)");
                Console.WriteLine("  [3] RAM Exhaustion Alarm (Configurable Threshold)");
                Console.WriteLine("  [4] Host Heartbeat Disconnect Watcher");
                Console.WriteLine("  [0] Back to Main Menu");
                Console.WriteLine();
                Console.Write("  Select > ");
                string sub = Console.ReadLine();
                if (sub == null) return;
                sub = sub.Trim();

                if (sub == "0") return;

                switch (sub)
                {
                    case "1":
                        RunLiveMonitor();
                        break;
                    case "2":
                        int cpuThreshold = ConsoleUI.PromptInt("Enter CPU Alert Threshold % (1-100)", 80, 1, 100);
                        RunResourceAlarm(true, cpuThreshold);
                        break;
                    case "3":
                        int ramThreshold = ConsoleUI.PromptInt("Enter RAM Alert Threshold % (1-100)", 90, 1, 100);
                        RunResourceAlarm(false, ramThreshold);
                        break;
                    case "4":
                        RunHostHeartbeatAlarm();
                        break;
                }
            }
        }

        private static void RunLiveMonitor()
        {
            ConsoleUI.DrawHeader("LIVE MONITOR (Press any key to exit)");
            Console.WriteLine();
            ConsoleUI.WriteColor("  Alerts are automatically logged to Cyberwatch_Alerts.log", UITheme.Muted);
            Console.WriteLine();

            int liveRow = Console.CursorTop;
            int noteRow = liveRow + 2;
            bool prevVisible = true;
            try { prevVisible = Console.CursorVisible; Console.CursorVisible = false; } catch { }

            try
            {
                using (var pinger = new Ping())
                {
                    while (!Console.KeyAvailable)
                    {
                        ConsoleUI.UpdateTopRightClock();
                        int cpu = SystemMetrics.GetCpuUsage();
                        long totalMB, freeMB;
                        SystemMetrics.GetRamMetrics(out totalMB, out freeMB);
                        long usedMB = Math.Max(0, totalMB - freeMB);
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
                                ConsoleUI.SafeBeep(800, 100);
                                SystemMetrics.LogAlert("LIVE_NET", "8.8.8.8 unreachable: " + reply.Status);
                            }
                        }
                        catch
                        {
                            netStatus = "OFFLINE";
                            netCol = UITheme.Danger;
                        }

                        if (cpu >= 90) SystemMetrics.LogAlert("LIVE_CPU", string.Format("CPU at {0}%", cpu));
                        if (ramPercent >= 90) SystemMetrics.LogAlert("LIVE_RAM", string.Format("RAM at {0}%", ramPercent));

                        ConsoleUI.ClearLineAt(liveRow);
                        string ts = DateTime.Now.ToString("HH:mm:ss");
                        Console.Write("  [{0}]  CPU: ", ts);
                        ConsoleUI.WriteColor(ConsoleUI.GetProgressBar(cpu, 12), ConsoleUI.GetThresholdColor(cpu), false);
                        Console.Write("   RAM: ");
                        ConsoleUI.WriteColor(ConsoleUI.GetProgressBar(ramPercent, 12), ConsoleUI.GetThresholdColor(ramPercent), false);
                        Console.Write("   NET: ");
                        ConsoleUI.WriteColor(netStatus, netCol, false);

                        ConsoleUI.ClearLineAt(noteRow);
                        ConsoleUI.WriteColor(string.Format("  RAM Detail: {0:N0} MB / {1:N0} MB  |  Last Sample: {2}", usedMB, totalMB, ts), UITheme.Muted, false);

                        Thread.Sleep(1500);
                    }
                    while (Console.KeyAvailable) Console.ReadKey(true);
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }
            finally
            {
                try { Console.CursorVisible = prevVisible; } catch { }
            }
        }

        private static void RunResourceAlarm(bool isCpu, int threshold)
        {
            string label = isCpu ? "CPU" : "RAM";
            ConsoleUI.DrawHeader(string.Format("ALARM: {0} >= {1}% (Press any key to exit)", label, threshold));
            Console.WriteLine();

            while (!Console.KeyAvailable)
            {
                int val = 0;
                if (isCpu)
                {
                    val = SystemMetrics.GetCpuUsage();
                }
                else
                {
                    long totalMB, freeMB;
                    SystemMetrics.GetRamMetrics(out totalMB, out freeMB);
                    long usedMB = Math.Max(0, totalMB - freeMB);
                    val = (int)((usedMB * 100) / (totalMB > 0 ? totalMB : 1));
                }

                string ts = DateTime.Now.ToString("HH:mm:ss");
                if (val >= threshold)
                {
                    ConsoleUI.SafeBeep(1200, 200);
                    Console.Write("  [{0}] ", ts);
                    string msg = string.Format("{0} at {1}% >= {2}%", label, val, threshold);
                    ConsoleUI.WriteColor("[ALERT] " + msg, UITheme.Danger, true);
                    SystemMetrics.LogAlert(label + "_ALARM", msg);
                }
                else
                {
                    Console.Write("  [{0}] {1}: {2}%  ", ts, label, val);
                    ConsoleUI.WriteColor(ConsoleUI.GetProgressBar(val, 12), ConsoleUI.GetThresholdColor(val), true);
                }
                Thread.Sleep(1500);
            }
            while (Console.KeyAvailable) Console.ReadKey(true);
        }

        private static void RunHostHeartbeatAlarm()
        {
            Console.Write("  Enter Host/IP to watch (default: 192.168.1.1): ");
            string host = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(host)) host = "192.168.1.1";
            else host = host.Trim();

            if (!InputValidator.IsValidHostOrIp(host))
            {
                ConsoleUI.WriteColor("  Invalid hostname or IP address.", UITheme.Danger);
                ConsoleUI.Pause();
                return;
            }

            ConsoleUI.DrawHeader(string.Format("Heartbeat Watcher: {0} (Press any key to exit)", host));
            Console.WriteLine();

            bool? wasOnline = null;
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
                            if (wasOnline == false)
                                SystemMetrics.LogAlert("HEARTBEAT", string.Format("Host {0} reconnected ({1} ms)", host, reply.RoundtripTime));
                            wasOnline = true;
                            Console.Write("  [{0}] ", ts);
                            ConsoleUI.WriteColor(string.Format("[ONLINE] {0} ({1} ms)", host, reply.RoundtripTime), UITheme.Success, true);
                        }
                        else
                        {
                            ConsoleUI.SafeBeep(900, 250);
                            if (wasOnline != false)
                                SystemMetrics.LogAlert("HEARTBEAT", string.Format("Host {0} disconnected ({1})", host, reply.Status));
                            wasOnline = false;
                            Console.Write("  [{0}] ", ts);
                            ConsoleUI.WriteColor(string.Format("[DISCONNECT] Host {0} is unreachable!", host), UITheme.Danger, true);
                        }
                    }
                    catch
                    {
                        ConsoleUI.SafeBeep(900, 250);
                        if (wasOnline != false)
                            SystemMetrics.LogAlert("HEARTBEAT", string.Format("Error reaching host {0}", host));
                        wasOnline = false;
                        ConsoleUI.WriteColor(string.Format("  [{0}] [DISCONNECT] Error reaching {1}", ts, host), UITheme.Danger, true);
                    }
                    Thread.Sleep(2000);
                }
                while (Console.KeyAvailable) Console.ReadKey(true);
            }
        }

        public static void WifiSecurityMenu()
        {
            while (true)
            {
                ConsoleUI.DrawHeader("WiFi Profiles & Password Keys");
                Console.WriteLine();
                Console.WriteLine("  [1] List Saved WiFi Profiles");
                Console.WriteLine("  [2] Select Profile & View Cleartext Password");
                Console.WriteLine("  [0] Back to Main Menu");
                Console.WriteLine();
                Console.Write("  Select > ");
                string sub = Console.ReadLine();
                if (sub == null) return;
                sub = sub.Trim();

                if (sub == "0") return;

                if (sub == "1")
                {
                    ProcessRunner.RunProcess("netsh", "wlan show profiles");
                    ConsoleUI.Pause();
                }
                else if (sub == "2")
                {
                    string raw = "";
                    try { raw = ProcessRunner.RunProcessCapture("netsh", "wlan show profiles"); } catch { }
                    var profiles = InputValidator.ParseWifiProfiles(raw);

                    string selectedSsid = null;
                    if (profiles.Count > 0)
                    {
                        Console.WriteLine();
                        ConsoleUI.WriteColor("  Saved WiFi Profiles:", UITheme.Primary);
                        for (int i = 0; i < profiles.Count; i++)
                        {
                            Console.WriteLine("  [{0}] {1}", i + 1, profiles[i]);
                        }
                        Console.WriteLine();
                        Console.Write("  Enter profile number [1-{0}] or SSID: ", profiles.Count);
                        string choice = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(choice))
                        {
                            choice = choice.Trim();
                            int idx;
                            if (int.TryParse(choice, out idx) && idx >= 1 && idx <= profiles.Count)
                                selectedSsid = profiles[idx - 1];
                            else
                                selectedSsid = choice;
                        }
                    }
                    else
                    {
                        Console.Write("  Enter Exact Profile Name (SSID): ");
                        string ssid = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(ssid)) selectedSsid = ssid.Trim();
                    }

                    if (!string.IsNullOrWhiteSpace(selectedSsid))
                    {
                        if (!InputValidator.IsValidSsid(selectedSsid))
                            ConsoleUI.WriteColor("  Invalid SSID format.", UITheme.Danger);
                        else
                            ProcessRunner.RunProcess("netsh", string.Format("wlan show profile name=\"{0}\" key=clear", selectedSsid));
                    }
                    ConsoleUI.Pause();
                }
            }
        }

        public static void ExportHtmlReport()
        {
            ConsoleUI.DrawHeader("Export SOC HTML Report");
            Console.WriteLine();
            ConsoleUI.WriteColor("  Collecting system, network & security telemetry...", UITheme.Muted);

            string reportFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                string.Format("Cyberwatch_Report_{0:yyyyMMdd_HHmmss}.html", DateTime.Now));

            int cpu = SystemMetrics.GetCpuUsage();
            long totalMB, freeMB;
            SystemMetrics.GetRamMetrics(out totalMB, out freeMB);
            long usedMB = Math.Max(0, totalMB - freeMB);
            int ramPercent = (int)((usedMB * 100) / (totalMB > 0 ? totalMB : 1));
            TimeSpan uptime = SystemMetrics.GetSystemUptime();
            string localIp = SystemMetrics.GetPrimaryLocalIP();

            string cpuBadge = cpu < 80 ? "badge-pass" : (cpu < 95 ? "badge-warn" : "badge-fail");
            string ramBadge = ramPercent < 90 ? "badge-pass" : (ramPercent < 97 ? "badge-warn" : "badge-fail");

            var avItems = SystemMetrics.GetAntivirusStatuses();
            var listeners = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Take(25).ToList();

            string fwSummary = "";
            try { fwSummary = ProcessRunner.RunProcessCapture("netsh", "advfirewall show allprofiles"); }
            catch { fwSummary = "Unavailable"; }

            string failedLogins = "";
            try { failedLogins = ProcessRunner.RunProcessCapture("wevtutil", "qe Security /c:5 /rd:true /f:text /q:\"*[System[(EventID=4625)]]\""); }
            catch { failedLogins = "Requires Administrator privileges or Security log unavailable."; }
            if (string.IsNullOrWhiteSpace(failedLogins))
                failedLogins = "No Event ID 4625 failed login entries returned (or elevated access required).";

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang='en'>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='UTF-8'>");
            sb.AppendLine("<title>Cyberwatch SOC Diagnostic Report</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: 'Segoe UI', system-ui, sans-serif; background: #0f172a; color: #e2e8f0; margin: 0; padding: 25px; }");
            sb.AppendLine(".container { max-width: 920px; margin: auto; background: #1e293b; border: 1px solid #334155; border-radius: 8px; padding: 25px; }");
            sb.AppendLine("h1 { color: #38bdf8; border-bottom: 2px solid #334155; padding-bottom: 8px; margin-top: 0; }");
            sb.AppendLine("h2 { color: #4ade80; margin-top: 22px; font-size: 1.15rem; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 10px; }");
            sb.AppendLine("th, td { border: 1px solid #334155; padding: 8px 12px; text-align: left; }");
            sb.AppendLine("th { background: #0f172a; color: #f8fafc; }");
            sb.AppendLine("pre { background: #0f172a; border: 1px solid #334155; border-radius: 6px; padding: 12px; overflow-x: auto; font-size: 0.85rem; color: #cbd5e1; white-space: pre-wrap; }");
            sb.AppendLine(".badge { padding: 3px 8px; border-radius: 4px; font-weight: 600; font-size: 0.85rem; }");
            sb.AppendLine(".badge-pass { background: #166534; color: #dcfce7; }");
            sb.AppendLine(".badge-warn { background: #854d0e; color: #fef08a; }");
            sb.AppendLine(".badge-fail { background: #991b1b; color: #fee2e2; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class='container'>");
            sb.AppendLine("<h1>CYBERWATCH - System &amp; SOC Diagnostic Report</h1>");
            sb.AppendLine(string.Format("<p style='color:#94a3b8;'><b>Generated:</b> {0:yyyy-MM-dd HH:mm:ss} | <b>Host:</b> {1} | <b>User:</b> {2} | <b>IP:</b> {3}</p>",
                DateTime.Now, InputValidator.HtmlEncode(Environment.MachineName), InputValidator.HtmlEncode(Environment.UserName), InputValidator.HtmlEncode(localIp)));

            sb.AppendLine("<h2>1. Hardware &amp; Performance</h2>");
            sb.AppendLine("<table><tr><th>Parameter</th><th>Value</th><th>Status</th></tr>");
            sb.AppendLine(string.Format("<tr><td>CPU Load</td><td>{0}%</td><td><span class='badge {1}'>{2}</span></td></tr>",
                cpu, cpuBadge, cpu < 80 ? "OPTIMAL" : "HIGH"));
            sb.AppendLine(string.Format("<tr><td>RAM Usage</td><td>{0:N0} MB / {1:N0} MB ({2}%)</td><td><span class='badge {3}'>{4}</span></td></tr>",
                usedMB, totalMB, ramPercent, ramBadge, ramPercent < 90 ? "NORMAL" : "HIGH"));
            sb.AppendLine(string.Format("<tr><td>System Uptime</td><td>{0}d {1}h {2}m {3}s</td><td><span class='badge badge-pass'>ONLINE</span></td></tr>",
                uptime.Days, uptime.Hours, uptime.Minutes, uptime.Seconds));
            sb.AppendLine(string.Format("<tr><td>Operating System</td><td>{0}</td><td><span class='badge badge-pass'>ACTIVE</span></td></tr>",
                InputValidator.HtmlEncode(Environment.OSVersion.ToString())));
            sb.AppendLine("</table>");

            sb.AppendLine("<h2>2. Active Storage Drives</h2>");
            sb.AppendLine("<table><tr><th>Drive</th><th>Format</th><th>Total</th><th>Free Space</th></tr>");
            const double bytesPerGB = 1024.0 * 1024.0 * 1024.0;
            foreach (var d in DriveInfo.GetDrives().Where(d => d.IsReady))
            {
                sb.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2:N1} GB</td><td>{3:N1} GB</td></tr>",
                    InputValidator.HtmlEncode(d.Name), InputValidator.HtmlEncode(d.DriveFormat), d.TotalSize / bytesPerGB, d.AvailableFreeSpace / bytesPerGB));
            }
            sb.AppendLine("</table>");

            sb.AppendLine("<h2>3. Network Adapters</h2>");
            sb.AppendLine("<table><tr><th>Adapter</th><th>Status</th><th>Speed</th></tr>");
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces().Where(n => n.OperationalStatus == OperationalStatus.Up))
            {
                sb.AppendLine(string.Format("<tr><td>{0}</td><td><span class='badge badge-pass'>{1}</span></td><td>{2} Mbps</td></tr>",
                    InputValidator.HtmlEncode(nic.Name), InputValidator.HtmlEncode(nic.OperationalStatus.ToString()), nic.Speed / 1000000));
            }
            sb.AppendLine("</table>");

            sb.AppendLine("<h2>4. Antivirus Protection</h2>");
            sb.AppendLine("<table><tr><th>Product</th><th>State</th><th>Signatures</th></tr>");
            if (avItems.Count == 0)
            {
                sb.AppendLine("<tr><td colspan='3'>No antivirus product reported.</td></tr>");
            }
            else
            {
                foreach (var av in avItems)
                {
                    sb.AppendLine(string.Format("<tr><td>{0}</td><td><span class='badge {1}'>{2}</span></td><td><span class='badge {3}'>{4}</span></td></tr>",
                        InputValidator.HtmlEncode(av.Name),
                        av.Enabled ? "badge-pass" : "badge-fail",
                        av.Enabled ? "ENABLED" : "DISABLED",
                        av.UpToDate ? "badge-pass" : "badge-warn",
                        av.UpToDate ? "UP TO DATE" : "OUTDATED"));
                }
            }
            sb.AppendLine("</table>");

            sb.AppendLine("<h2>5. Listening TCP Ports (Top 25)</h2>");
            sb.AppendLine("<table><tr><th>Protocol</th><th>Local Endpoint</th><th>State</th></tr>");
            foreach (var ep in listeners)
            {
                sb.AppendLine(string.Format("<tr><td>TCP</td><td>{0}</td><td><span class='badge badge-pass'>LISTENING</span></td></tr>",
                    InputValidator.HtmlEncode(ep.ToString())));
            }
            sb.AppendLine("</table>");

            sb.AppendLine("<h2>6. Windows Firewall Profiles</h2>");
            sb.AppendLine("<pre>" + InputValidator.HtmlEncode(fwSummary.Trim()) + "</pre>");

            sb.AppendLine("<h2>7. Recent Failed Logins (Event ID 4625)</h2>");
            sb.AppendLine("<pre>" + InputValidator.HtmlEncode(failedLogins.Trim()) + "</pre>");

            sb.AppendLine("<p style='margin-top: 25px; color: #64748b; text-align: center;'>Generated by Cyberwatch v3.0 Pro</p>");
            sb.AppendLine("</div></body></html>");

            try
            {
                File.WriteAllText(reportFile, sb.ToString(), Encoding.UTF8);
                ConsoleUI.WriteColor(string.Format("  Report created: {0}", reportFile), UITheme.Success);

                try
                {
                    Process.Start(new ProcessStartInfo(reportFile) { UseShellExecute = true });
                }
                catch { }
            }
            catch (Exception ex)
            {
                ConsoleUI.WriteColor(string.Format("  Failed to write report: {0}", ex.Message), UITheme.Danger);
            }
            ConsoleUI.Pause();
        }
    }
}

