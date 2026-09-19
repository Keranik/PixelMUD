using System.Globalization;
using static Rom24.Merc;

namespace Rom24
{
    public static partial class RomString
    {
        public static bool str_cmp(string astr, string bstr)
        {
            if (astr == null) { Db.bug("Str_cmp: null astr.", 0); return true; }
            if (bstr == null) { Db.bug("Str_cmp: null bstr.", 0); return true; }
            return !string.Equals(astr, bstr, StringComparison.OrdinalIgnoreCase);
        }

        public static bool str_prefix(string astr, string bstr)
        {
            if (astr == null) { Db.bug("Strn_cmp: null astr.", 0); return true; }
            if (bstr == null) { Db.bug("Strn_cmp: null bstr.", 0); return true; }
            if (astr.Length > bstr.Length) return true;
            return !bstr.StartsWith(astr, StringComparison.OrdinalIgnoreCase);
        }

        public static bool str_infix(string astr, string bstr)
        {
            if (string.IsNullOrEmpty(astr)) return false;
            return bstr.IndexOf(astr, StringComparison.OrdinalIgnoreCase) < 0;
        }

        public static bool str_suffix(string astr, string bstr)
        {
            astr ??= "";
            bstr ??= "";
            int sstr1 = astr.Length;
            int sstr2 = bstr.Length;
            if (sstr1 <= sstr2 && !str_cmp(astr, bstr.Substring(sstr2 - sstr1)))
                return false;
            else
                return true;
        }

        /* libc ctime(): "Www Mmm dd hh:mm:ss yyyy\n" with space-padded day */
        public static string ctime(long unix)
        {
            var t = DateTimeOffset.FromUnixTimeSeconds(unix).ToLocalTime();
            string[] wday = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
            string[] mon = { "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            return sprintf("%s %s %2d %02d:%02d:%02d %d\n",
                wday[(int)t.DayOfWeek], mon[t.Month - 1], t.Day,
                t.Hour, t.Minute, t.Second, t.Year);
        }

        public static string capitalize(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            var chars = str.ToLowerInvariant().ToCharArray();
            chars[0] = Bit.UPPER(chars[0]);
            return new string(chars);
        }

        public static string one_argument(string argument, out string arg)
        {
            argument ??= "";
            int i = 0;
            while (i < argument.Length && Bit.isspace(argument[i])) i++;
            if (i >= argument.Length) { arg = ""; return ""; }
            char end = ' ';
            if (argument[i] == '\'' || argument[i] == '"') { end = argument[i]; i++; }
            var sb = new System.Text.StringBuilder();
            while (i < argument.Length)
            {
                if (argument[i] == end)
                {
                    i++;
                    break;
                }
                sb.Append(Bit.LOWER(argument[i]));
                i++;
            }
            arg = sb.ToString();
            while (i < argument.Length && Bit.isspace(argument[i])) i++;
            return i >= argument.Length ? "" : argument.Substring(i);
        }

        public static int number_argument(string argument, out string arg)
        {
            argument ??= "";
            int dot = argument.IndexOf('.');
            if (dot >= 0)
            {
                int number = Interp.atoi(argument.Substring(0, dot));
                arg = argument.Substring(dot + 1);
                return number;
            }
            arg = argument;
            return 1;
        }

        public static string smash_tilde(string str)
        {
            if (string.IsNullOrEmpty(str)) return str ?? "";
            return str.Replace('~', '-');
        }

        public static int mult_argument(string argument, out string arg)
        {
            argument ??= "";
            int star = argument.IndexOf('*');
            if (star >= 0)
            {
                int number = Interp.atoi(argument.Substring(0, star));
                arg = argument.Substring(star + 1);
                return number;
            }
            arg = argument;
            return 1;
        }

        public static string sprintf(string fmt, params object[] args)
        {
            // ROM uses C printf. Handle the common %s %d %ld %2d cases.
            try
            {
                string s = fmt;
                int ai = 0;
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] != '%' || i + 1 >= s.Length) { sb.Append(s[i]); continue; }
                    i++;
                    if (s[i] == '%') { sb.Append('%'); continue; }
                    int width = 0;
                    bool pad0 = false;
                    bool left = false;
                    if (i < s.Length && s[i] == '-') { left = true; i++; }
                    if (i < s.Length && s[i] == '0') { pad0 = true; i++; }
                    while (i < s.Length && char.IsDigit(s[i])) { width = width * 10 + (s[i] - '0'); i++; }
                    int precision = -1;
                    if (i < s.Length && s[i] == '.')
                    {
                        i++;
                        precision = 0;
                        while (i < s.Length && char.IsDigit(s[i])) { precision = precision * 10 + (s[i] - '0'); i++; }
                    }
                    if (i < s.Length && s[i] == 'l') i++;
                    if (i >= s.Length) break;
                    if (ai >= args.Length) break;
                    object v = args[ai++];
                    string piece = s[i] switch
                    {
                        's' => v?.ToString() ?? "",
                        'c' => v is char ch ? ch.ToString() : Convert.ToChar(v).ToString(),
                        'd' or 'i' or 'u' => Convert.ToInt64(v).ToString(CultureInfo.InvariantCulture),
                        'x' => Convert.ToInt64(v).ToString("x", CultureInfo.InvariantCulture),
                        _ => v?.ToString() ?? ""
                    };
                    if (precision >= 0 && (s[i] == 's') && piece.Length > precision)
                        piece = piece.Substring(0, precision);
                    if (width > 0)
                    {
                        if (pad0 && (s[i] == 'd' || s[i] == 'i' || s[i] == 'u'))
                        {
                            bool neg = piece.StartsWith("-");
                            string digits = neg ? piece.Substring(1) : piece;
                            int padWidth = neg ? width - 1 : width;
                            if (padWidth < 0) padWidth = 0;
                            digits = digits.PadLeft(padWidth, '0');
                            piece = neg ? "-" + digits : digits;
                        }
                        else
                            piece = pad0 ? piece.PadLeft(width, '0')
                                : left ? piece.PadRight(width) : piece.PadLeft(width);
                    }
                    sb.Append(piece);
                }
                return sb.ToString();
            }
            catch
            {
                return fmt;
            }
        }
    }
}
