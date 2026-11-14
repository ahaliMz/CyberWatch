using System;
using System.Collections.Generic;
using Cyberwatch;

namespace Cyberwatch.Tests
{
    class TestRunner
    {
        private static int _passed = 0;
        private static int _failed = 0;

        static int Main()
        {
            Console.WriteLine("=== Cyberwatch Unit Test Suite ===");
            Console.WriteLine();

            TestHostAndIpValidation();
            TestSubnetValidation();
            TestSsidValidation();
            TestPortListParser();
            TestHtmlAndCsvEncoding();
            TestJsonExtraction();
            TestWifiProfileParser();
            TestUiHelpers();

            Console.WriteLine();
            Console.WriteLine(string.Format("Results: {0} passed, {1} failed.", _passed, _failed));
            return _failed == 0 ? 0 : 1;
        }

        static void Assert(bool condition, string name)
        {
            if (condition)
            {
                _passed++;
                Console.WriteLine("  [PASS] " + name);
            }
            else
            {
                _failed++;
                Console.WriteLine("  [FAIL] " + name);
            }
        }

        static void TestHostAndIpValidation()
        {
            Assert(InputValidator.IsValidHostOrIp("127.0.0.1"), "Valid IPv4 loopback");
            Assert(InputValidator.IsValidHostOrIp("8.8.8.8"), "Valid IPv4 public");
            Assert(InputValidator.IsValidHostOrIp("::1"), "Valid IPv6 loopback");
            Assert(InputValidator.IsValidHostOrIp("server-01.corp.local"), "Valid FQDN hostname");
            Assert(InputValidator.IsValidHostOrIp("localhost"), "Valid single-label host");

            Assert(!InputValidator.IsValidHostOrIp(""), "Reject empty host");
            Assert(!InputValidator.IsValidHostOrIp("   "), "Reject whitespace host");
            Assert(!InputValidator.IsValidHostOrIp("8.8.8.8 & calc"), "Reject shell injection &");
            Assert(!InputValidator.IsValidHostOrIp("host|whoami"), "Reject pipe injection |");
            Assert(!InputValidator.IsValidHostOrIp("host;dir"), "Reject semicolon injection");
            Assert(!InputValidator.IsValidHostOrIp("\"quoted\""), "Reject quoted host");
            Assert(!InputValidator.IsValidHostOrIp("-badstart"), "Reject leading hyphen");
            Assert(!InputValidator.IsValidHostOrIp(new string('a', 254)), "Reject host > 253 chars");
        }

        static void TestSubnetValidation()
        {
            Assert(InputValidator.IsValidSubnetPrefix("192.168.1"), "Valid 192.168.1 subnet");
            Assert(InputValidator.IsValidSubnetPrefix("10.0.0"), "Valid 10.0.0 subnet");
            Assert(InputValidator.IsValidSubnetPrefix(" 172.16.254 "), "Trimmed valid subnet");

            Assert(!InputValidator.IsValidSubnetPrefix("192.168.1.1"), "Reject 4 octets");
            Assert(!InputValidator.IsValidSubnetPrefix("192.168"), "Reject 2 octets");
            Assert(!InputValidator.IsValidSubnetPrefix("999.168.1"), "Reject octet > 255");
            Assert(!InputValidator.IsValidSubnetPrefix("192.168.1 & dir"), "Reject injection in subnet");
        }

        static void TestSsidValidation()
        {
            Assert(InputValidator.IsValidSsid("Office WiFi 5G"), "Valid SSID with spaces");
            Assert(InputValidator.IsValidSsid("My-Home_Net!"), "Valid SSID with punctuation");

            Assert(!InputValidator.IsValidSsid(""), "Reject empty SSID");
            Assert(!InputValidator.IsValidSsid("bad\"ssid"), "Reject quote in SSID");
            Assert(!InputValidator.IsValidSsid("line1\nline2"), "Reject newline in SSID");
            Assert(!InputValidator.IsValidSsid(new string('w', 33)), "Reject SSID > 32 chars");
        }

        static void TestPortListParser()
        {
            List<int> ports;
            Assert(InputValidator.ParsePortList("80, 443, 22, 80", out ports) && ports.Count == 3 && ports[0] == 22 && ports[2] == 443,
                "ParsePortList deduplicates and sorts ports");

            Assert(InputValidator.ParsePortList("8000-8003,443", out ports) && ports.Count == 5 && ports[0] == 443 && ports[4] == 8003,
                "ParsePortList parses port range");

            Assert(!InputValidator.ParsePortList("", out ports), "ParsePortList rejects empty");
            Assert(!InputValidator.ParsePortList("0,80", out ports), "ParsePortList rejects port 0");
            Assert(!InputValidator.ParsePortList("80,65536", out ports), "ParsePortList rejects port > 65535");
            Assert(!InputValidator.ParsePortList("100-50", out ports), "ParsePortList rejects reversed range");
            Assert(!InputValidator.ParsePortList("1-5000", out ports), "ParsePortList enforces MaxScanPorts cap");
            Assert(!InputValidator.ParsePortList("http,https", out ports), "ParsePortList rejects non-numeric");
        }

        static void TestHtmlAndCsvEncoding()
        {
            string encoded = InputValidator.HtmlEncode("<script>\"alert\" & 'x'</script>");
            Assert(encoded == "&lt;script&gt;&quot;alert&quot; &amp; &#39;x&#39;&lt;/script&gt;",
                "HtmlEncode escapes XSS characters");

            Assert(InputValidator.CsvEscape("plain") == "plain", "CsvEscape leaves plain string untouched");
            Assert(InputValidator.CsvEscape("a,b") == "\"a,b\"", "CsvEscape quotes comma");
            Assert(InputValidator.CsvEscape("say \"hi\"") == "\"say \"\"hi\"\"\"", "CsvEscape escapes quotes");
        }

        static void TestJsonExtraction()
        {
            string json = "{\"ip\": \"203.0.113.42\", \"city\": \"Dhaka\", \"org\": \"AS12345 Example \\\"ISP\\\"\"}";
            Assert(InputValidator.ExtractJsonString(json, "ip") == "203.0.113.42", "ExtractJsonString extracts IP");
            Assert(InputValidator.ExtractJsonString(json, "city") == "Dhaka", "ExtractJsonString extracts city");
            Assert(InputValidator.ExtractJsonString(json, "org") == "AS12345 Example \"ISP\"", "ExtractJsonString unescapes quotes");
            Assert(InputValidator.ExtractJsonString(json, "missing") == "", "ExtractJsonString returns empty for missing key");
        }

        static void TestWifiProfileParser()
        {
            string sample =
                "Profiles on interface Wi-Fi:\r\n" +
                "Group policy profiles (read only)\r\n" +
                "---------------------------------\r\n" +
                "    <None>\r\n" +
                "User profiles\r\n" +
                "-------------\r\n" +
                "    All User Profile     : HomeNet_5G\r\n" +
                "    All User Profile     : Office Guest\r\n";

            var list = InputValidator.ParseWifiProfiles(sample);
            Assert(list.Count == 2 && list[0] == "HomeNet_5G" && list[1] == "Office Guest",
                "ParseWifiProfiles extracts netsh SSID names");
        }

        static void TestUiHelpers()
        {
            string bar0 = ConsoleUI.GetProgressBar(-10, 10);
            string bar100 = ConsoleUI.GetProgressBar(150, 10);
            Assert(bar0 == "[··········]   0%", "GetProgressBar clamps below 0");
            Assert(bar100 == "[■■■■■■■■■■] 100%", "GetProgressBar clamps above 100");
            Assert(ConsoleUI.Truncate("Cyberwatch", 6) == "Cybe..", "Truncate shortens long strings");
            Assert(ConsoleUI.Truncate("Test", 10) == "Test", "Truncate preserves short strings");
        }
    }
}

