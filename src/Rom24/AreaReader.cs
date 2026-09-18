using static Rom24.Merc;

namespace Rom24
{
    /// <summary>
    /// Byte-faithful fread_* from db.c, over a seekable stream with ungetc.
    /// Newlines in strings become \n\r exactly as ROM does.
    /// </summary>
    public sealed class AreaReader : IDisposable
    {
        readonly Stream _s;
        readonly bool _ownsStream;
        int _ungot = -1;
        public int Line { get; private set; } = 1;
        public string Filename { get; }

        public AreaReader(string path)
        {
            Filename = path;
            _s = File.OpenRead(path);
            _ownsStream = true;
        }

        public AreaReader(Stream stream, string filename, bool ownsStream = true)
        {
            Filename = filename;
            _s = stream;
            _ownsStream = ownsStream;
        }

        public void Dispose()
        {
            if (_ownsStream)
                _s.Dispose();
        }

        public int Getc()
        {
            int c;
            if (_ungot >= 0) { c = _ungot; _ungot = -1; }
            else c = _s.ReadByte();
            if (c == '\n') Line++;
            return c;
        }

        public void Ungetc(int c)
        {
            _ungot = c;
            if (c == '\n') Line--;
        }

        public bool Eof()
        {
            if (_ungot >= 0) return false;
            int c = _s.ReadByte();
            if (c < 0) return true;
            _ungot = c;
            return false;
        }

        static bool IsSpace(int c) => c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\f' || c == '\v';

        public char fread_letter()
        {
            int c;
            do { c = Getc(); } while (c >= 0 && IsSpace(c));
            if (c < 0) return '\0';
            return (char)c;
        }

        public int fread_number()
        {
            int c;
            do { c = Getc(); } while (c >= 0 && IsSpace(c));
            int number = 0;
            bool sign = false;
            if (c == '+') c = Getc();
            else if (c == '-') { sign = true; c = Getc(); }
            if (c < '0' || c > '9')
            {
                Db.bug("Fread_number: bad format.", 0);
                Environment.Exit(1);
            }
            while (c >= '0' && c <= '9')
            {
                number = number * 10 + c - '0';
                c = Getc();
            }
            if (sign) number = -number;
            if (c == '|') number += fread_number();
            else if (c != ' ') Ungetc(c);
            return number;
        }

        public long fread_flag()
        {
            int c;
            do { c = Getc(); } while (c >= 0 && IsSpace(c));
            bool negative = false;
            if (c == '-') { negative = true; c = Getc(); }
            int number = 0;
            unchecked
            {
                if (c < '0' || c > '9')
                {
                    while ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                    {
                        number += (int)flag_convert((char)c);
                        c = Getc();
                    }
                }
                while (c >= '0' && c <= '9')
                {
                    number = number * 10 + c - '0';
                    c = Getc();
                }
                if (c == '|') number += (int)fread_flag();
                else if (c != ' ') Ungetc(c);
            }
            if (negative)
                return -1 * (long)number;
            return number;
        }

        public static long flag_convert(char letter)
        {
            long bitsum = 0;
            if (letter >= 'A' && letter <= 'Z')
            {
                bitsum = 1;
                for (char i = letter; i > 'A'; i--) bitsum *= 2;
            }
            else if (letter >= 'a' && letter <= 'z')
            {
                bitsum = 67108864; // 2^26
                for (char i = letter; i > 'a'; i--) bitsum *= 2;
            }
            return bitsum;
        }

        public string fread_string()
        {
            int c;
            do { c = Getc(); } while (c >= 0 && IsSpace(c));
            if (c < 0)
            {
                Db.bug("Fread_string: EOF", 0);
                return null;
            }
            if (c == '~') return "";
            var sb = new System.Text.StringBuilder();
            sb.Append((char)c);
            for (;;)
            {
                c = Getc();
                if (c < 0) { Db.bug("Fread_string: EOF", 0); return null; }
                switch (c)
                {
                    case '\n':
                        sb.Append('\n');
                        sb.Append('\r');
                        break;
                    case '\r':
                        break;
                    case '~':
                        return sb.ToString();
                    default:
                        sb.Append((char)c);
                        break;
                }
            }
        }

        public string fread_string_eol()
        {
            int c;
            do { c = Getc(); } while (c >= 0 && IsSpace(c));
            if (c < 0 || c == '\n') return "";
            var sb = new System.Text.StringBuilder();
            sb.Append((char)c);
            for (;;)
            {
                c = Getc();
                if (c < 0 || c == '\n' || c == '\r') return sb.ToString();
                sb.Append((char)c);
            }
        }

        public void fread_to_eol()
        {
            int c;
            do { c = Getc(); } while (c >= 0 && c != '\n' && c != '\r');
            do { c = Getc(); } while (c == '\n' || c == '\r');
            if (c >= 0) Ungetc(c);
        }

        public string fread_word()
        {
            var word = new char[MAX_INPUT_LENGTH];
            int cEnd;
            do { cEnd = Getc(); } while (cEnd >= 0 && IsSpace(cEnd));
            int p;
            if (cEnd == '\'' || cEnd == '"')
            {
                p = 0;
            }
            else
            {
                /* C stores (char)EOF as 0xFF and never treats EOF as a terminator. */
                word[0] = (char)(cEnd < 0 ? 0xFF : cEnd);
                p = 1;
                cEnd = ' ';
            }

            for (; p < MAX_INPUT_LENGTH; p++)
            {
                int c = Getc();
                word[p] = (char)(c < 0 ? 0xFF : c);
                bool ended = cEnd == ' '
                    ? (c >= 0 && IsSpace(c))
                    : (c >= 0 && c == cEnd);
                if (ended)
                {
                    if (cEnd == ' ' && c >= 0)
                        Ungetc(c);
                    return new string(word, 0, p);
                }
            }

            Db.bug("Fread_word: word too long.", 0);
            Environment.Exit(1);
            return null;
        }
    }
}
