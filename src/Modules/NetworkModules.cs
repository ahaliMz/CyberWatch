using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cyberwatch
{
    internal static class NetworkModules
    {
        private static readonly Dictionary<int, string> KnownPortNames = new Dictionary<int, string>
        {
            { 21, "FTP" }, { 22, "SSH" }, { 23, "Telnet" }, { 25, "SMTP" },
            { 53, "DNS" }, { 80, "HTTP" }, { 110, "POP3" }, { 135, "RPC" },
            { 139, "NetBIOS" }, { 143, "IMAP" }, { 443, "HTTPS" }, { 445, "SMB" },
            { 1433, "MSSQL" }, { 1521, "Oracle" }, { 3306, "MySQL" }, { 3389, "RDP" },
            { 5432, "PostgreSQL" }, { 5900, "VNC" }, { 8080, "HTTP-Alt" }, { 8443, "HTTPS-Alt" }
        };

        public static void NetworkDiagnosticsMenu()
        {
            string host = PromptDiagnosticHost();
            if (host == null) return;

            while (true)
            {
                ConsoleUI.DrawHeader(string.Format("Diagnostics: {0}", host));
                Console.WriteLine();
                Console.WriteLine("  [1] Standard Ping (4 Packets)");
                Console.WriteLine("  [2] Continuous Real-time Ping");
                Console.WriteLine("  [3] DNS Record Lookup");
                Console.WriteLine("  [4] Multi-Hop Traceroute");
                Console.WriteLine("  [5] Active Connections (Netstat)");
                Console.WriteLine("  [6] Change Target Host");
                Console.WriteLine("  [0] Back to Main Menu");
                Console.WriteLine();
                Console.Write("  Select > ");
                string sub = Console.ReadLine();
                if (sub == null) return;
                sub = sub.Trim();

                if (sub == "0") return;
                if (sub == "6")
                {
                    string nextHost = PromptDiagnosticHost();
                    if (nextHost != null) host = nextHost;
                    continue;
                }

                switch (sub)
                {
                    case "1":
                        RunPingTest(host, 4);
                        ConsoleUI.Pause();
                        break;
                    case "2":
                        RunContinuousPing(host);
                        ConsoleUI.Pause();
                        break;
                    case "3":
                        RunDnsLookup(host);
                        ConsoleUI.Pause();
                        break;
                    case "4":
                        RunTraceroute(host);
                        ConsoleUI.Pause();
                        break;
                    case "5":
                        RunNetstatView();
                        ConsoleUI.Pause();
                        break;
                }
            }
        }

        private static string PromptDiagnosticHost()
        {
            ConsoleUI.DrawHeader("Network Diagnostics");
            Console.WriteLine();
            Console.Write("  Enter Host or IP (default: 8.8.8.8): ");
            string host = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(host)) return "8.8.8.8";
            host = host.Trim();

            if (!InputValidator.IsValidHostOrIp(host))
            {
                ConsoleUI.WriteColor("  Invalid hostname or IP address.", UITheme.Danger);
                ConsoleUI.Pause();
                return null;
            }
            return host;
        }

        public static void RunPingTest(string host, int count)
        {
            Console.WriteLine();
            ConsoleUI.WriteColor(string.Format("  Sending {0} ICMP Echo requests to {1}...", count, host), UITheme.Primary);
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
                            ConsoleUI.WriteColor(string.Format("  Request timed out or failed ({0})", reply.Status), UITheme.Danger);
                        }
                    }
                    catch (Exception ex)
                    {
                        ConsoleUI.WriteColor(string.Format("  Ping error: {0}", ex.Message), UITheme.Danger);
                    }
                    if (i < count - 1) Thread.Sleep(400);
                }

                int loss = (int)(((count - success) / (double)count) * 100);
                Console.WriteLine();
                ConsoleUI.WriteColor(string.Format("  Packets: Sent = {0}, Received = {1}, Lost = {2} ({3}% loss)", count, success, count - success, loss),
                    loss > 0 ? UITheme.Danger : UITheme.Success);
                if (success > 0)
                {
                    ConsoleUI.WriteColor(string.Format("  Average Latency: {0} ms", totalRtt / success), UITheme.Primary);
                }
            }
        }

        private static void RunContinuousPing(string host)
        {
            Console.WriteLine();
            ConsoleUI.WriteColor(string.Format("  Continuous Ping to {0} (Press any key to stop)...", host), UITheme.Warning);
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
                            ConsoleUI.WriteColor(string.Format("{0,4} ms  ", rtt), col, false);
                            int barLen = (int)Math.Min(25, Math.Max(1, rtt / 6));
                            ConsoleUI.WriteColor(new string('■', barLen), col, true);
                        }
                        else
                        {
                            ConsoleUI.SafeBeep(700, 100);
                            ConsoleUI.WriteColor(string.Format("  [{0}] Timeout ({1})", ts, reply.Status), UITheme.Danger);
                        }
                    }
                    catch
                    {
                        ConsoleUI.WriteColor(string.Format("  [{0}] Unreachable Host", DateTime.Now.ToString("HH:mm:ss")), UITheme.Danger);
                    }
                    Thread.Sleep(800);
                }
                while (Console.KeyAvailable) Console.ReadKey(true);
            }
        }

        private static void RunDnsLookup(string host)
        {
            Console.WriteLine();
            ConsoleUI.WriteColor(string.Format("  DNS Lookup: {0}", host), UITheme.Primary);
            Console.WriteLine();

            try
            {
                var entry = Dns.GetHostEntry(host);
                Console.WriteLine("  Hostname: {0}", entry.HostName);
                Console.WriteLine("  IP Addresses:");
                foreach (var ip in entry.AddressList)
                {
                    ConsoleUI.WriteColor(string.Format("    • {0} ({1})", ip, ip.AddressFamily), UITheme.Success);
                }
            }
            catch (Exception ex)
            {
                ConsoleUI.WriteColor(string.Format("  DNS Resolution failed: {0}", ex.Message), UITheme.Danger);
            }
        }

        private static void RunTraceroute(string host)
        {
            Console.WriteLine();
            ConsoleUI.WriteColor(string.Format("  Tracing route to {0}...", host), UITheme.Primary);
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
                            ConsoleUI.WriteColor("  Trace complete.", UITheme.Success);
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

        private static void RunNetstatView()
        {
            Console.WriteLine();
            ConsoleUI.WriteColor("  Active TCP Connections", UITheme.Primary);
            Console.WriteLine();
            Console.WriteLine("  {0,-6} {1,-22} {2,-22} {3}", "Proto", "Local Address", "Remote Address", "State");
            Console.Write("  ");
            ConsoleUI.WriteColor(new string('─', 65), UITheme.Muted);

            var props = IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnections = props.GetActiveTcpConnections();

            foreach (var conn in tcpConnections.Take(20))
            {
                ConsoleColor stateCol = conn.State == TcpState.Established ? UITheme.Success : UITheme.Foreground;
                Console.Write("  {0,-6} {1,-22} {2,-22} ", "TCP", conn.LocalEndPoint, conn.RemoteEndPoint);
                ConsoleUI.WriteColor(conn.State.ToString(), stateCol, true);
            }
            Console.WriteLine();
            ConsoleUI.WriteColor(string.Format("  Showing {0} of {1} active connections.", Math.Min(20, tcpConnections.Length), tcpConnections.Length), UITheme.Muted);
        }

        public static void FastScannerMenu()
        {
            while (true)
            {
                ConsoleUI.DrawHeader("Fast Network Scanner");
                Console.WriteLine();
                Console.WriteLine("  [1] Subnet Ping Sweep (Parallel 1-254 IPs + CSV Export)");
                Console.WriteLine("  [2] Port Scanner (Top 20 or Custom Ports + CSV Export)");
                Console.WriteLine("  [3] View ARP Table (IP to MAC Mappings)");
                Console.WriteLine("  [4] Security Misconfiguration Audit");
                Console.WriteLine("  [0] Back to Main Menu");
                Console.WriteLine();
                Console.Write("  Select > ");
                string sub = Console.ReadLine();
                if (sub == null) return;
                sub = sub.Trim();

                if (sub == "0") return;

                if (sub == "1")
                {
                    RunSubnetPingSweep();
                    ConsoleUI.Pause();
                }
                else if (sub == "2")
                {
                    RunFastPortScanner();
                    ConsoleUI.Pause();
                }
                else if (sub == "3")
                {
                    ConsoleUI.DrawHeader("ARP Cache Table");
                    Console.WriteLine();
                    ProcessRunner.RunProcess("arp", "-a");
                    ConsoleUI.Pause();
                }
                else if (sub == "4")
                {
                    RunVulnerabilityAudit();
                    ConsoleUI.Pause();
                }
            }
        }

        private static void RunSubnetPingSweep()
        {
            string defaultSubnet = SystemMetrics.GetLocalSubnetPrefix();
            Console.Write("  Enter Subnet Prefix (default: {0}): ", defaultSubnet);
            string subnet = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(subnet)) subnet = defaultSubnet;
            else subnet = subnet.Trim();

            if (!InputValidator.IsValidSubnetPrefix(subnet))
            {
                ConsoleUI.WriteColor("  Invalid subnet prefix. Expected format: e.g. 192.168.1", UITheme.Danger);
                return;
            }

            ConsoleUI.DrawHeader(string.Format("Ping Sweep: {0}.1 to {0}.254", subnet));
            Console.WriteLine();
            ConsoleUI.WriteColor("  Scanning 254 IP addresses simultaneously...", UITheme.Primary);
            Console.WriteLine();

            var activeHosts = new List<Tuple<int, string, long, string>>();
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
                                var dnsTask = Dns.GetHostEntryAsync(ip);
                                if (await Task.WhenAny(dnsTask, Task.Delay(500)) == dnsTask)
                                {
                                    hostName = (await dnsTask).HostName;
                                }
                            }
                            catch { }

                            lock (lockObj)
                            {
                                activeHosts.Add(Tuple.Create(i, ip, reply.RoundtripTime, hostName));
                                Console.Write("  • ");
                                ConsoleUI.WriteColor(string.Format("{0,-16}", ip), UITheme.Success, false);
                                ConsoleUI.WriteColor(string.Format(" {0,3} ms ", reply.RoundtripTime), UITheme.Primary, false);
                                if (!string.IsNullOrEmpty(hostName))
                                    ConsoleUI.WriteColor(string.Format(" ({0})", hostName), UITheme.Muted, true);
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
            ConsoleUI.WriteColor(string.Format("  Finished in {0} ms. Found {1} online hosts.", sw.ElapsedMilliseconds, activeHosts.Count), UITheme.Success);

            if (activeHosts.Count > 0 && ConsoleUI.ConfirmAction("Export online hosts to CSV?"))
            {
                string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    string.Format("Cyberwatch_Scan_Subnet_{0:yyyyMMdd_HHmmss}.csv", DateTime.Now));
                var sb = new StringBuilder();
                sb.AppendLine("IP,LatencyMs,Hostname");
                foreach (var h in activeHosts.OrderBy(x => x.Item1))
                {
                    sb.AppendLine(string.Format("{0},{1},{2}",
                        InputValidator.CsvEscape(h.Item2), h.Item3, InputValidator.CsvEscape(h.Item4)));
                }
                ExportCsvFile(fileName, sb.ToString());
            }
        }

        private static void RunFastPortScanner()
        {
            Console.Write("  Enter Target IP or Host to scan (default: 127.0.0.1): ");
            string target = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(target)) target = "127.0.0.1";
            else target = target.Trim();

            if (!InputValidator.IsValidHostOrIp(target))
            {
                ConsoleUI.WriteColor("  Invalid hostname or IP address.", UITheme.Danger);
                return;
            }

            Console.Write("  Ports or ranges (e.g. 22,80,443,8000-8010; Enter for Top 20): ");
            string portInput = Console.ReadLine();

            List<int> portsToScan;
            if (string.IsNullOrWhiteSpace(portInput))
            {
                portsToScan = KnownPortNames.Keys.OrderBy(p => p).ToList();
            }
            else if (!InputValidator.ParsePortList(portInput, out portsToScan))
            {
                ConsoleUI.WriteColor(string.Format("  Invalid port list (max {0} ports, range 1-65535).", InputValidator.MaxScanPorts), UITheme.Danger);
                return;
            }

            IPAddress resolvedIp;
            if (!IPAddress.TryParse(target, out resolvedIp))
            {
                try
                {
                    var addrs = Dns.GetHostAddresses(target);
                    resolvedIp = addrs.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork) ?? addrs.FirstOrDefault();
                }
                catch (Exception ex)
                {
                    ConsoleUI.WriteColor(string.Format("  Could not resolve host '{0}': {1}", target, ex.Message), UITheme.Danger);
                    return;
                }
            }

            if (resolvedIp == null)
            {
                ConsoleUI.WriteColor("  No IP address resolved for target.", UITheme.Danger);
                return;
            }

            ConsoleUI.DrawHeader(string.Format("Port Scan: {0} ({1})", target, resolvedIp));
            Console.WriteLine();
            ConsoleUI.WriteColor(string.Format("  Scanning {0} port(s) in parallel...", portsToScan.Count), UITheme.Primary);
            Console.WriteLine();

            var openPorts = new List<Tuple<int, string>>();
            var lockObj = new object();
            var sw = Stopwatch.StartNew();

            using (var throttler = new SemaphoreSlim(128))
            {
                var tasks = portsToScan.Select(async port =>
                {
                    await throttler.WaitAsync();
                    try
                    {
                        string svc = KnownPortNames.ContainsKey(port) ? KnownPortNames[port] : "TCP";
                        using (var client = new TcpClient(resolvedIp.AddressFamily))
                        {
                            try
                            {
                                var connectTask = client.ConnectAsync(resolvedIp, port);
                                ProcessRunner.ObserveFaults(connectTask);
                                var timeoutTask = Task.Delay(1000);
                                var completed = await Task.WhenAny(connectTask, timeoutTask);
                                if (completed == connectTask)
                                {
                                    await connectTask;
                                    if (client.Connected)
                                    {
                                        lock (lockObj)
                                        {
                                            openPorts.Add(Tuple.Create(port, svc));
                                            Console.Write("  • ");
                                            ConsoleUI.WriteColor(string.Format("Port {0,-5} ", port), UITheme.Success, false);
                                            ConsoleUI.WriteColor(string.Format("[{0}]", svc), UITheme.Primary, true);
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                    finally
                    {
                        throttler.Release();
                    }
                });

                Task.WaitAll(tasks.ToArray());
            }
            sw.Stop();

            Console.WriteLine();
            ConsoleUI.WriteColor(string.Format("  Finished in {0} ms. {1} open ports detected.", sw.ElapsedMilliseconds, openPorts.Count),
                openPorts.Count > 0 ? UITheme.Success : UITheme.Warning);

            if (openPorts.Count > 0 && ConsoleUI.ConfirmAction("Export open ports to CSV?"))
            {
                string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    string.Format("Cyberwatch_Scan_Ports_{0:yyyyMMdd_HHmmss}.csv", DateTime.Now));
                var sb = new StringBuilder();
                sb.AppendLine("Target,IP,Port,Service,Status");
                foreach (var op in openPorts.OrderBy(x => x.Item1))
                {
                    sb.AppendLine(string.Format("{0},{1},{2},{3},OPEN",
                        InputValidator.CsvEscape(target), resolvedIp, op.Item1, InputValidator.CsvEscape(op.Item2)));
                }
                ExportCsvFile(fileName, sb.ToString());
            }
        }

        private static void ExportCsvFile(string fileName, string content)
        {
            try
            {
                File.WriteAllText(fileName, content, Encoding.UTF8);
                ConsoleUI.WriteColor(string.Format("  CSV exported: {0}", fileName), UITheme.Success);
            }
            catch (Exception ex)
            {
                ConsoleUI.WriteColor(string.Format("  Failed to write CSV: {0}", ex.Message), UITheme.Danger);
            }
        }

        private static void RunVulnerabilityAudit()
        {
            ConsoleUI.DrawHeader("Security Misconfiguration Audit");
            Console.WriteLine();

            Console.Write("  1. Administrative Shares (C$, ADMIN$): ");
            try
            {
                string outStr = ProcessRunner.RunProcessCapture("net", "share");
                if (outStr.Contains("ADMIN$") || outStr.Contains("C$"))
                    ConsoleUI.WriteColor("[WARNING] Default Admin shares are active", UITheme.Warning);
                else
                    ConsoleUI.WriteColor("[OK] No default admin shares", UITheme.Success);
            }
            catch { Console.WriteLine("N/A"); }

            Console.Write("  2. User Account Control (UAC) Status:  ");
            try
            {
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System"))
                {
                    object val = key != null ? key.GetValue("EnableLUA", 0) : 0;
                    int uac = val != null ? Convert.ToInt32(val) : 0;
                    if (uac == 1)
                        ConsoleUI.WriteColor("[OK] UAC is Enabled", UITheme.Success);
                    else
                        ConsoleUI.WriteColor("[WARNING] UAC is DISABLED!", UITheme.Danger);
                }
            }
            catch { Console.WriteLine("Permission Denied"); }

            Console.Write("  3. Remote Desktop (RDP) Service:       ");
            try
            {
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Terminal Server"))
                {
                    object val = key != null ? key.GetValue("fDenyTSConnections", 1) : 1;
                    int rdp = val != null ? Convert.ToInt32(val) : 1;
                    if (rdp == 0)
                        ConsoleUI.WriteColor("[INFO] RDP is Enabled (Port 3389)", UITheme.Warning);
                    else
                        ConsoleUI.WriteColor("[OK] RDP is Disabled", UITheme.Success);
                }
            }
            catch { Console.WriteLine("N/A"); }
        }

        public static void PublicIpThreatMenu()
        {
            ConsoleUI.DrawHeader("Public IP & Geolocation");
            Console.WriteLine();
            ConsoleUI.WriteColor("  Querying public network endpoints over HTTPS...", UITheme.Muted);
            Console.WriteLine();

            try
            {
                using (var client = new TimeoutWebClient(6000))
                {
                    client.Headers.Add("User-Agent", "Cyberwatch/3.0");
                    string ipJson = client.DownloadString("https://api.ipify.org?format=json");
                    string publicIp = InputValidator.ExtractJsonString(ipJson, "ip");

                    IPAddress parsedIp;
                    if (string.IsNullOrWhiteSpace(publicIp) || !IPAddress.TryParse(publicIp.Trim(), out parsedIp))
                    {
                        ConsoleUI.WriteColor("  Unexpected response from IP lookup endpoint.", UITheme.Danger);
                        ConsoleUI.Pause();
                        return;
                    }
                    publicIp = parsedIp.ToString();

                    Console.Write("  Public IP Address: ");
                    ConsoleUI.WriteColor(publicIp, UITheme.Success, true);

                    bool geoLoaded = false;
                    try
                    {
                        string geoJson = client.DownloadString(string.Format("https://ipinfo.io/{0}/json", publicIp));
                        string country = InputValidator.ExtractJsonString(geoJson, "country");
                        string region = InputValidator.ExtractJsonString(geoJson, "region");
                        string city = InputValidator.ExtractJsonString(geoJson, "city");
                        string org = InputValidator.ExtractJsonString(geoJson, "org");

                        if (!string.IsNullOrEmpty(country) || !string.IsNullOrEmpty(org))
                        {
                            geoLoaded = true;
                            string asn = "";
                            string provider = org;
                            if (!string.IsNullOrEmpty(org) && org.StartsWith("AS", StringComparison.OrdinalIgnoreCase))
                            {
                                int sp = org.IndexOf(' ');
                                if (sp > 0)
                                {
                                    asn = org.Substring(0, sp);
                                    provider = org.Substring(sp + 1).Trim();
                                }
                            }

                            Console.WriteLine("  Country:           {0}", string.IsNullOrEmpty(country) ? "N/A" : country);
                            Console.WriteLine("  Region / City:     {0}, {1}", string.IsNullOrEmpty(region) ? "N/A" : region, string.IsNullOrEmpty(city) ? "N/A" : city);
                            Console.WriteLine("  ISP / Org:         {0}", string.IsNullOrEmpty(provider) ? "N/A" : provider);
                            if (!string.IsNullOrEmpty(asn))
                            {
                                Console.WriteLine("  ASN:               {0}", asn);
                            }
                        }
                    }
                    catch { }

                    if (!geoLoaded)
                    {
                        try
                        {
                            string fallbackJson = client.DownloadString(string.Format("https://ipwho.is/{0}", publicIp));
                            string country = InputValidator.ExtractJsonString(fallbackJson, "country");
                            string region = InputValidator.ExtractJsonString(fallbackJson, "region");
                            string city = InputValidator.ExtractJsonString(fallbackJson, "city");
                            string isp = InputValidator.ExtractJsonString(fallbackJson, "isp");
                            string org = InputValidator.ExtractJsonString(fallbackJson, "org");

                            if (!string.IsNullOrEmpty(country))
                            {
                                Console.WriteLine("  Country:           {0}", country);
                                Console.WriteLine("  Region / City:     {0}, {1}", region, city);
                                Console.WriteLine("  ISP Provider:      {0}", string.IsNullOrEmpty(isp) ? org : isp);
                            }
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleUI.WriteColor(string.Format("  Could not reach public IP API: {0}", ex.Message), UITheme.Danger);
            }
            ConsoleUI.Pause();
        }

        public static void LiveBandwidthMonitor()
        {
            ConsoleUI.DrawHeader("Live Bandwidth & Traffic Meter (Press any key to exit)");
            Console.WriteLine();

            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up
                    && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback
                    && nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                .OrderByDescending(nic => !IsVirtualInterface(nic))
                .ThenByDescending(nic => nic.Speed)
                .ToList();

            if (!interfaces.Any())
            {
                ConsoleUI.WriteColor("  No active network interface found.", UITheme.Danger);
                ConsoleUI.Pause();
                return;
            }

            NetworkInterface selectedNic = interfaces.First();
            if (interfaces.Count > 1)
            {
                ConsoleUI.WriteColor("  Active Network Interfaces:", UITheme.Primary);
                for (int i = 0; i < interfaces.Count; i++)
                {
                    Console.WriteLine("  [{0}] {1} ({2})", i + 1, interfaces[i].Name, ConsoleUI.Truncate(interfaces[i].Description, 50));
                }
                Console.WriteLine();
                Console.Write("  Select interface [1-{0}] (default: 1): ", interfaces.Count);
                string pick = Console.ReadLine();
                int idx;
                if (!string.IsNullOrWhiteSpace(pick) && int.TryParse(pick.Trim(), out idx) && idx >= 1 && idx <= interfaces.Count)
                {
                    selectedNic = interfaces[idx - 1];
                }
                ConsoleUI.DrawHeader("Live Bandwidth & Traffic Meter (Press any key to exit)");
                Console.WriteLine();
            }

            Console.WriteLine("  Interface: {0} ({1})", selectedNic.Name, selectedNic.Description);
            Console.WriteLine();

            int liveRow = Console.CursorTop;
            int peakRow = liveRow + 2;

            long prevBytesRecv = selectedNic.GetIPStatistics().BytesReceived;
            long prevBytesSent = selectedNic.GetIPStatistics().BytesSent;
            double peakRx = 0;
            double peakTx = 0;

            bool prevVisible = true;
            try { prevVisible = Console.CursorVisible; Console.CursorVisible = false; } catch { }

            try
            {
                while (!Console.KeyAvailable)
                {
                    ConsoleUI.UpdateTopRightClock();
                    Thread.Sleep(1000);
                    var stats = selectedNic.GetIPStatistics();
                    long curRecv = stats.BytesReceived;
                    long curSent = stats.BytesSent;

                    double rxSpeedKB = Math.Max(0, curRecv - prevBytesRecv) / 1024.0;
                    double txSpeedKB = Math.Max(0, curSent - prevBytesSent) / 1024.0;
                    if (rxSpeedKB > peakRx) peakRx = rxSpeedKB;
                    if (txSpeedKB > peakTx) peakTx = txSpeedKB;

                    prevBytesRecv = curRecv;
                    prevBytesSent = curSent;

                    ConsoleUI.ClearLineAt(liveRow);
                    string ts = DateTime.Now.ToString("HH:mm:ss");
                    Console.Write("  [{0}]  RX: ", ts);
                    ConsoleUI.WriteColor(string.Format("{0,7:N1} KB/s ", rxSpeedKB), UITheme.Success, false);
                    int rxBar = (int)Math.Min(15, rxSpeedKB / 60);
                    ConsoleUI.WriteColor("[" + new string('▼', rxBar).PadRight(15) + "] ", UITheme.Success, false);
                    Console.Write("  TX: ");
                    ConsoleUI.WriteColor(string.Format("{0,7:N1} KB/s ", txSpeedKB), UITheme.Primary, false);
                    int txBar = (int)Math.Min(15, txSpeedKB / 60);
                    ConsoleUI.WriteColor("[" + new string('▲', txBar).PadRight(15) + "]", UITheme.Primary, false);

                    ConsoleUI.ClearLineAt(peakRow);
                    ConsoleUI.WriteColor(string.Format("  Peak Speed -> Download: {0:N1} KB/s  |  Upload: {1:N1} KB/s", peakRx, peakTx), UITheme.Muted, false);
                }
                while (Console.KeyAvailable) Console.ReadKey(true);
                Console.WriteLine();
                Console.WriteLine();
            }
            finally
            {
                try { Console.CursorVisible = prevVisible; } catch { }
            }
        }

        private static bool IsVirtualInterface(NetworkInterface nic)
        {
            string desc = (nic.Description ?? "").ToLowerInvariant();
            string name = (nic.Name ?? "").ToLowerInvariant();
            return desc.Contains("virtual") || desc.Contains("hyper-v") || desc.Contains("vmware")
                || desc.Contains("vbox") || desc.Contains("tap-") || name.Contains("vethernet") || name.Contains("wsl");
        }

        public static void QuickNetworkTable()
        {
            ConsoleUI.DrawHeader("Quick Network Parameters");
            Console.WriteLine();
            ProcessRunner.RunProcess("ipconfig", "/all");
            Console.WriteLine();
            Console.Write("  ");
            ConsoleUI.WriteColor(new string('─', ConsoleUI.LineWidth), UITheme.Muted);
            ProcessRunner.RunProcess("route", "print -4");
            ConsoleUI.Pause();
        }
    }
}

