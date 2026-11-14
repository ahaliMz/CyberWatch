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
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Cyberwatch
{
    internal class TimeoutWebClient : WebClient
    {
        private readonly int _timeoutMs;

        public TimeoutWebClient(int timeoutMs = 6000)
        {
            _timeoutMs = timeoutMs;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var req = base.GetWebRequest(address);
            if (req != null) req.Timeout = _timeoutMs;
            return req;
        }
    }

    internal static class InputValidator
    {
        public const int MaxScanPorts = 2048;

        public static bool IsValidHostOrIp(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            value = value.Trim();
            if (value.Length > 253) return false;

            IPAddress ip;
            if (IPAddress.TryParse(value, out ip)) return true;

            return Regex.IsMatch(value, @"^[A-Za-z0-9]([A-Za-z0-9\-\.]*[A-Za-z0-9])?$");
        }

        public static bool IsValidSubnetPrefix(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            var parts = value.Trim().Split('.');
            if (parts.Length != 3) return false;

            foreach (var part in parts)
            {
                byte octet;
                if (!byte.TryParse(part, out octet)) return false;
            }
            return true;
        }

        public static bool IsValidSsid(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            if (value.Length > 32) return false;
            foreach (char c in value)
            {
                if (c == '"' || char.IsControl(c)) return false;
            }
            return true;
        }

        public static bool ParsePortList(string input, out List<int> ports)
        {
            ports = new List<int>();
            if (string.IsNullOrWhiteSpace(input)) return false;

            var seen = new HashSet<int>();
            var tokens = input.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0) return false;

            foreach (var rawToken in tokens)
            {
                string token = rawToken.Trim();
                if (string.IsNullOrEmpty(token)) continue;

                int dashIdx = token.IndexOf('-');
                if (dashIdx >= 0)
                {
                    var bounds = token.Split('-');
                    if (bounds.Length != 2) return false;

                    int start, end;
                    if (!int.TryParse(bounds[0].Trim(), out start) || !int.TryParse(bounds[1].Trim(), out end))
                        return false;
                    if (start < 1 || end > 65535 || start > end) return false;
                    if ((end - start + 1) + seen.Count > MaxScanPorts) return false;

                    for (int p = start; p <= end; p++) seen.Add(p);
                }
                else
                {
                    int port;
                    if (!int.TryParse(token, out port) || port < 1 || port > 65535)
                        return false;
                    if (seen.Count + 1 > MaxScanPorts) return false;
                    seen.Add(port);
                }
            }

            if (seen.Count == 0) return false;
            ports = seen.OrderBy(p => p).ToList();
            return true;
        }

        public static string HtmlEncode(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");
        }

        public static string CsvEscape(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }

        public static string ExtractJsonString(string json, string key)
        {
            if (string.IsNullOrEmpty(json)) return "";
            string pattern = "\"" + Regex.Escape(key) + "\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"";
            var match = Regex.Match(json, pattern);
            if (!match.Success) return "";
            return Regex.Unescape(match.Groups[1].Value);
        }

        public static List<string> ParseWifiProfiles(string netshOutput)
        {
            var profiles = new List<string>();
            if (string.IsNullOrEmpty(netshOutput)) return profiles;

            var lines = netshOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var rawLine in lines)
            {
                string line = rawLine.Trim();
                int colonIdx = line.IndexOf(':');
                if (colonIdx <= 0) continue;

                string left = line.Substring(0, colonIdx).Trim();
                string right = line.Substring(colonIdx + 1).Trim();
                if (left.IndexOf("Profile", StringComparison.OrdinalIgnoreCase) >= 0 && !string.IsNullOrEmpty(right))
                {
                    profiles.Add(right);
                }
            }
            return profiles;
        }
    }

    internal static class ProcessRunner
    {
        public static int RunProcess(string fileName, string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo(fileName, arguments ?? "")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                var consoleLock = new object();
                using (var p = Process.Start(psi))
                {
                    p.OutputDataReceived += (s, e) =>
                    {
                        if (e.Data != null)
                        {
                            lock (consoleLock) { Console.WriteLine("  " + e.Data); }
                        }
                    };
                    p.ErrorDataReceived += (s, e) =>
                    {
                        if (e.Data != null && e.Data.Length > 0)
                        {
                            lock (consoleLock) { ConsoleUI.WriteColor("  " + e.Data, UITheme.Danger); }
                        }
                    };
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                    p.WaitForExit();
                    return p.ExitCode;
                }
            }
            catch (Exception ex)
            {
                ConsoleUI.WriteColor(string.Format("  Command error ({0}): {1}", fileName, ex.Message), UITheme.Danger);
                return -1;
            }
        }

        public static string RunProcessCapture(string fileName, string arguments)
        {
            var psi = new ProcessStartInfo(fileName, arguments ?? "")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (var p = Process.Start(psi))
            {
                var errTask = Task.Run(() => p.StandardError.ReadToEnd());
                string stdout = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                try { errTask.Wait(); } catch { }
                return stdout;
            }
        }

        public static void ObserveFaults(Task task)
        {
            task.ContinueWith(t => { GC.KeepAlive(t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }

    internal class AntivirusInfo
    {
        public string Name;
        public bool Enabled;
        public bool UpToDate;
    }

    internal static class SystemMetrics
    {
        private static PerformanceCounter _cpuCounter = null;
        private static bool _cpuCounterInitFailed = false;
        private static string _cachedLocalIp = null;
        private static DateTime _cachedLocalIpAt = DateTime.MinValue;
        private static readonly object _logLock = new object();

        public static bool IsAdministrator()
        {
            try
            {
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }

        public static int GetCpuUsage()
        {
            if (!_cpuCounterInitFailed)
            {
                try
                {
                    if (_cpuCounter == null)
                    {
                        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                        _cpuCounter.NextValue();
                        Thread.Sleep(250);
                    }
                    int val = (int)Math.Round(_cpuCounter.NextValue());
                    return Math.Max(0, Math.Min(100, val));
                }
                catch
                {
                    _cpuCounterInitFailed = true;
                    if (_cpuCounter != null)
                    {
                        try { _cpuCounter.Dispose(); } catch { }
                        _cpuCounter = null;
                    }
                }
            }

            try
            {
                using (var searcher = new ManagementObjectSearcher("select LoadPercentage from Win32_Processor"))
                using (var results = searcher.Get())
                {
                    int sum = 0;
                    int count = 0;
                    foreach (ManagementObject item in results)
                    {
                        using (item)
                        {
                            if (item["LoadPercentage"] != null)
                            {
                                sum += Convert.ToInt32(item["LoadPercentage"]);
                                count++;
                            }
                        }
                    }
                    if (count > 0) return sum / count;
                }
            }
            catch { }
            return 0;
        }

        public static void GetRamMetrics(out long totalMB, out long freeMB)
        {
            totalMB = 0;
            freeMB = 0;
            try
            {
                using (var searcher = new ManagementObjectSearcher("select TotalVisibleMemorySize, FreePhysicalMemory from Win32_OperatingSystem"))
                using (var results = searcher.Get())
                {
                    foreach (ManagementObject item in results)
                    {
                        using (item)
                        {
                            if (item["TotalVisibleMemorySize"] != null)
                                totalMB = Convert.ToInt64(item["TotalVisibleMemorySize"]) / 1024;
                            if (item["FreePhysicalMemory"] != null)
                                freeMB = Convert.ToInt64(item["FreePhysicalMemory"]) / 1024;
                            return;
                        }
                    }
                }
            }
            catch { }
        }

        public static TimeSpan GetSystemUptime()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem"))
                using (var results = searcher.Get())
                {
                    foreach (ManagementObject item in results)
                    {
                        using (item)
                        {
                            if (item["LastBootUpTime"] != null)
                            {
                                DateTime bootTime = ManagementDateTimeConverter.ToDateTime(item["LastBootUpTime"].ToString());
                                TimeSpan diff = DateTime.Now - bootTime;
                                if (diff >= TimeSpan.Zero) return diff;
                            }
                        }
                    }
                }
            }
            catch { }

            return TimeSpan.FromMilliseconds(unchecked((uint)Environment.TickCount));
        }

        public static string GetPrimaryLocalIP()
        {
            if (_cachedLocalIp != null && (DateTime.UtcNow - _cachedLocalIpAt).TotalSeconds < 15)
            {
                return _cachedLocalIp;
            }

            string result = "127.0.0.1";
            try
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    var endPoint = socket.LocalEndPoint as IPEndPoint;
                    if (endPoint != null) result = endPoint.Address.ToString();
                }
            }
            catch
            {
                try
                {
                    var fallbackAddr = NetworkInterface.GetAllNetworkInterfaces()
                        .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                        .SelectMany(n => n.GetIPProperties().UnicastAddresses)
                        .Select(a => a.Address)
                        .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(a));
                    if (fallbackAddr != null) result = fallbackAddr.ToString();
                }
                catch { }
            }

            _cachedLocalIp = result;
            _cachedLocalIpAt = DateTime.UtcNow;
            return result;
        }

        public static string GetLocalSubnetPrefix()
        {
            string ip = GetPrimaryLocalIP();
            var parts = ip.Split('.');
            if (parts.Length == 4)
            {
                return string.Format("{0}.{1}.{2}", parts[0], parts[1], parts[2]);
            }
            return "192.168.1";
        }

        public static List<AntivirusInfo> GetAntivirusStatuses()
        {
            var list = new List<AntivirusInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher(@"\\.\root\SecurityCenter2", "SELECT displayName, productState FROM AntiVirusProduct"))
                using (var results = searcher.Get())
                {
                    foreach (ManagementObject av in results)
                    {
                        using (av)
                        {
                            string name = av["displayName"] != null ? av["displayName"].ToString() : "Unknown AV";
                            uint state = av["productState"] != null ? Convert.ToUInt32(av["productState"]) : 0;
                            list.Add(new AntivirusInfo
                            {
                                Name = name,
                                Enabled = ((state >> 12) & 0xF) == 1,
                                UpToDate = ((state >> 4) & 0xF) == 0
                            });
                        }
                    }
                }
            }
            catch { }

            if (list.Count == 0)
            {
                try
                {
                    using (var sc = new ServiceController("WinDefend"))
                    {
                        list.Add(new AntivirusInfo
                        {
                            Name = "Windows Defender Antivirus (WinDefend)",
                            Enabled = sc.Status == ServiceControllerStatus.Running,
                            UpToDate = true
                        });
                    }
                }
                catch { }
            }
            return list;
        }

        public static void LogAlert(string category, string message)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cyberwatch_Alerts.log");
                string line = string.Format("[{0:yyyy-MM-dd HH:mm:ss}] [{1}] {2}{3}",
                    DateTime.Now, category, message, Environment.NewLine);
                lock (_logLock)
                {
                    File.AppendAllText(logPath, line, Encoding.UTF8);
                }
            }
            catch { }
        }
    }
}

