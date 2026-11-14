using System;
using System.Text;
using System.Threading;

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
            try
            {
                Console.BackgroundColor = Background;
                Console.ForegroundColor = Foreground;
                Console.Clear();
            }
            catch { }
        }
    }

    internal static class ConsoleUI
    {
        public const int LineWidth = 88;
        public static bool SoundAlertsEnabled = true;

        public static void DrawHeader(string subTitle = "")
        {
            UITheme.Apply();
            Console.WriteLine();

            string dateStr = DateTime.Now.ToString("yyyy-MM-dd");
            string timeStr = DateTime.Now.ToString("HH:mm:ss");
            string dateTimeText = string.Format("{0}  {1}", dateStr, timeStr);

            int spaces = LineWidth - dateTimeText.Length;
            if (spaces < 0) spaces = 0;

            Console.Write("  ");
            Console.Write(new string(' ', spaces));
            WriteColor(dateTimeText, UITheme.Warning, true);

            Console.Write("  ");
            WriteColor(new string('─', LineWidth), UITheme.Muted);

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
            WriteColor(new string('─', LineWidth), UITheme.Muted);

            string host = Environment.MachineName;
            string user = Environment.UserName;
            string localIp = SystemMetrics.GetPrimaryLocalIP();
            string themeName = UITheme.CurrentTheme.ToString();
            bool isAdmin = SystemMetrics.IsAdministrator();

            Console.Write("  ");
            WriteColor("Host: ", UITheme.Muted, false);
            WriteColor(host + "  ", UITheme.Foreground, false);
            WriteColor("User: ", UITheme.Muted, false);
            WriteColor(user + "  ", UITheme.Foreground, false);
            WriteColor("IP: ", UITheme.Muted, false);
            WriteColor(localIp + "  ", UITheme.Success, false);
            WriteColor("Role: ", UITheme.Muted, false);
            WriteColor((isAdmin ? "ADMIN" : "USER") + "  ", isAdmin ? UITheme.Success : UITheme.Warning, false);
            WriteColor("Theme: ", UITheme.Muted, false);
            WriteColor(themeName, UITheme.Primary, true);

            Console.Write("  ");
            WriteColor(new string('─', LineWidth), UITheme.Muted);

            if (!string.IsNullOrEmpty(subTitle))
            {
                Console.WriteLine();
                Console.Write("  ");
                WriteColor("►►► " + subTitle.ToUpperInvariant(), UITheme.Primary, true);
                Console.Write("  ");
                WriteColor(new string('─', LineWidth), UITheme.Muted);
            }
        }

        public static void UpdateTopRightClock()
        {
            try
            {
                if (Console.WindowTop != 0 || Console.WindowWidth < LineWidth + 2)
                    return;

                int curLeft = Console.CursorLeft;
                int curTop = Console.CursorTop;
                bool curVisible = Console.CursorVisible;

                Console.CursorVisible = false;
                string dateStr = DateTime.Now.ToString("yyyy-MM-dd");
                string timeStr = DateTime.Now.ToString("HH:mm:ss");
                string dateTimeText = string.Format("{0}  {1}", dateStr, timeStr);

                int targetLeft = Math.Max(0, LineWidth + 2 - dateTimeText.Length);
                Console.SetCursorPosition(targetLeft, 1);
                WriteColor(dateTimeText, UITheme.Warning, false);

                Console.SetCursorPosition(curLeft, curTop);
                Console.CursorVisible = curVisible;
            }
            catch { }
        }

        public static string ReadInputWithLiveClock()
        {
            try
            {
                if (Console.IsInputRedirected)
                {
                    return Console.ReadLine();
                }
            }
            catch { }

            var sb = new StringBuilder();
            DateTime lastUpdate = DateTime.MinValue;

            while (true)
            {
                if ((DateTime.Now - lastUpdate).TotalMilliseconds >= 250)
                {
                    UpdateTopRightClock();
                    lastUpdate = DateTime.Now;
                }

                bool hasKey = false;
                try
                {
                    hasKey = Console.KeyAvailable;
                }
                catch
                {
                    return Console.ReadLine();
                }

                if (hasKey)
                {
                    ConsoleKeyInfo key;
                    try
                    {
                        key = Console.ReadKey(true);
                    }
                    catch
                    {
                        return Console.ReadLine();
                    }

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
                            try
                            {
                                if (Console.CursorLeft > 0) Console.Write("\b \b");
                            }
                            catch { }
                        }
                    }
                    else if (key.Key == ConsoleKey.Escape)
                    {
                        Console.WriteLine();
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

        public static void ClearLineAt(int row)
        {
            try
            {
                if (row >= 0 && row < Console.BufferHeight)
                {
                    int width = Math.Max(1, Console.BufferWidth - 1);
                    Console.SetCursorPosition(0, row);
                    Console.Write(new string(' ', width));
                    Console.SetCursorPosition(0, row);
                }
            }
            catch { }
        }

        public static void Pause()
        {
            Console.WriteLine();
            WriteColor("  Press any key to return...", UITheme.Muted);
            try
            {
                if (Console.IsInputRedirected)
                {
                    Console.ReadLine();
                    return;
                }
            }
            catch { }

            try
            {
                while (!Console.KeyAvailable)
                {
                    UpdateTopRightClock();
                    Thread.Sleep(200);
                }
                Console.ReadKey(true);
            }
            catch
            {
                try { Console.ReadLine(); } catch { }
            }
        }

        public static bool ConfirmAction(string message)
        {
            Console.WriteLine();
            Console.Write("  ");
            WriteColor(message + " [y/N]: ", UITheme.Warning, false);
            string resp = Console.ReadLine();
            if (string.IsNullOrEmpty(resp)) return false;
            resp = resp.Trim().ToUpperInvariant();
            return resp == "Y" || resp == "YES";
        }

        public static int PromptInt(string prompt, int defaultVal, int minVal, int maxVal)
        {
            Console.Write("  {0} (default {1}): ", prompt, defaultVal);
            string raw = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(raw)) return defaultVal;

            int parsed;
            if (int.TryParse(raw.Trim(), out parsed) && parsed >= minVal && parsed <= maxVal)
            {
                return parsed;
            }
            WriteColor(string.Format("  Invalid input; using default {0}.", defaultVal), UITheme.Warning);
            return defaultVal;
        }

        public static void SafeBeep(int freq, int durationMs)
        {
            if (!SoundAlertsEnabled) return;
            try { Console.Beep(freq, durationMs); } catch { }
        }

        public static string GetProgressBar(int percentage, int width = 18)
        {
            percentage = Math.Max(0, Math.Min(100, percentage));
            if (width < 1) width = 1;
            int filled = (percentage * width) / 100;
            int empty = width - filled;
            return "[" + new string('■', filled) + new string('·', empty) + string.Format("] {0,3}%", percentage);
        }

        public static ConsoleColor GetThresholdColor(int percent)
        {
            if (percent >= 90) return UITheme.Danger;
            if (percent >= 75) return UITheme.Warning;
            return UITheme.Success;
        }

        public static string Truncate(string val, int maxLen)
        {
            if (string.IsNullOrEmpty(val) || maxLen <= 0) return "";
            if (maxLen < 3) return val.Length <= maxLen ? val : val.Substring(0, maxLen);
            return val.Length <= maxLen ? val : val.Substring(0, maxLen - 2) + "..";
        }
    }
}

