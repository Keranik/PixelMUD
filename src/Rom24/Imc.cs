using static Rom24.Merc;

namespace Rom24
{
    /* Player-visible IMC2 surface copied from imc.c / imc.h.
     * Network uplink is environmental (imc.config Autoconnect / ServerPort). */
    public static class Imc
    {
        public const int IMCPERM_NOTSET = 0;
        public const int IMCPERM_NONE = 1;
        public const int IMCPERM_MORT = 2;
        public const int IMCPERM_IMM = 3;
        public const int IMCPERM_ADMIN = 4;
        public const int IMCPERM_IMP = 5;
        public const int IMC_OFFLINE = 0;
        public const int IMC_AUTH1 = 1;
        public const int IMC_AUTH2 = 2;
        public const int IMC_ONLINE = 3;

        public const long IMC_TELL = (1 << 0);
        public const long IMC_DENYTELL = (1 << 1);
        public const long IMC_BEEP = (1 << 2);
        public const long IMC_DENYBEEP = (1 << 3);
        public const long IMC_INVIS = (1 << 4);
        public const long IMC_PRIVACY = (1 << 5);
        public const long IMC_DENYFINGER = (1 << 6);
        public const long IMC_AFK = (1 << 7);
        public const long IMC_COLORFLAG = (1 << 8);
        public const long IMC_PERMOVERRIDE = (1 << 9);
        public const long IMC_NOTIFY = (1 << 10);

        public static readonly string[] perm_names =
            { "Notset", "None", "Mort", "Imm", "Admin", "Imp" };

        public class ImcMud
        {
            public int state = IMC_OFFLINE;
            public int desc = -1;
            public int autoconnect;
            public int minlevel = 10;
            public int immlevel = 101;
            public int adminlevel = 113;
            public int implevel = 115;
            public int iport;
            public int rport;
            public int sha256 = 1;
            public int sha256pass;
            public string localname = "";
            public string fullname = "";
            public string ihost = "";
            public string email = "";
            public string www = "";
            public string @base = "";
            public string details = "";
            public string rhost = "";
            public string clientpw = "";
            public string serverpw = "";
            public string versionid = "IMC2 Freedom CL-2.1a ";
        }

        public class ImcCmd
        {
            public ImcCmd next;
            public string name = "";
            public string code = "";
            public int level;
            public bool connected;
            public List<string> aliases = new();
            public Action<CharData, string> function;
        }

        public class ImcIgnore
        {
            public ImcIgnore next;
            public string name = "";
        }

        class ImcColor
        {
            public ImcColor next;
            public string name = "";
            public string mudtag = "";
            public string imctag = "";
        }

        class ImcHelp
        {
            public ImcHelp next;
            public string name = "";
            public int level;
            public string text = "";
        }

        public static ImcMud this_imcmud;
        static ImcCmd first_imc_command;
        static ImcColor first_imc_color;
        static ImcHelp first_imc_help;
        static bool imcpacketdebug;
        static string imc_dir;
        static string who_template = "";
        static readonly List<ImcCmd> commands = new();

        static string Dir
        {
            get
            {
                if (imc_dir != null) return imc_dir;
                imc_dir = Path.GetFullPath(Path.Combine(Game.area_dir ?? ".", "..", "imc"));
                return imc_dir;
            }
        }

        public static void startup()
        {
            string cfg = Path.Combine(Dir, "imc.config");
            if (!File.Exists(cfg))
                return;
            load_commands();
            load_config();
            load_helps();
            load_colors();
            load_templates();
            if (this_imcmud != null && this_imcmud.autoconnect != 0)
                startup_network();
        }

        static void load_config()
        {
            string path = Path.Combine(Dir, "imc.config");
            if (!File.Exists(path))
                return;
            this_imcmud = new ImcMud();
            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line[0] == '#' || line[0] == '$')
                    continue;
                if (line.Equals("End", StringComparison.OrdinalIgnoreCase))
                    break;
                var sp = line.IndexOf(' ');
                string key = sp < 0 ? line : line.Substring(0, sp);
                string val = sp < 0 ? "" : line.Substring(sp + 1).Trim();
                if (key.Equals("LocalName", StringComparison.OrdinalIgnoreCase)) this_imcmud.localname = val;
                else if (key.Equals("Autoconnect", StringComparison.OrdinalIgnoreCase)) this_imcmud.autoconnect = Interp.atoi(val);
                else if (key.Equals("MinPlayerLevel", StringComparison.OrdinalIgnoreCase)) this_imcmud.minlevel = Interp.atoi(val);
                else if (key.Equals("MinImmLevel", StringComparison.OrdinalIgnoreCase)) this_imcmud.immlevel = Interp.atoi(val);
                else if (key.Equals("AdminLevel", StringComparison.OrdinalIgnoreCase)) this_imcmud.adminlevel = Interp.atoi(val);
                else if (key.Equals("Implevel", StringComparison.OrdinalIgnoreCase)) this_imcmud.implevel = Interp.atoi(val);
                else if (key.Equals("InfoName", StringComparison.OrdinalIgnoreCase)) this_imcmud.fullname = val;
                else if (key.Equals("InfoHost", StringComparison.OrdinalIgnoreCase)) this_imcmud.ihost = val;
                else if (key.Equals("InfoPort", StringComparison.OrdinalIgnoreCase)) this_imcmud.iport = Interp.atoi(val);
                else if (key.Equals("InfoEmail", StringComparison.OrdinalIgnoreCase)) this_imcmud.email = val;
                else if (key.Equals("InfoWWW", StringComparison.OrdinalIgnoreCase)) this_imcmud.www = val;
                else if (key.Equals("InfoBase", StringComparison.OrdinalIgnoreCase)) this_imcmud.@base = val;
                else if (key.Equals("InfoDetails", StringComparison.OrdinalIgnoreCase)) this_imcmud.details = val;
                else if (key.Equals("ServerAddr", StringComparison.OrdinalIgnoreCase)
                    || key.Equals("RouterAddr", StringComparison.OrdinalIgnoreCase)) this_imcmud.rhost = val;
                else if (key.Equals("ServerPort", StringComparison.OrdinalIgnoreCase)) this_imcmud.rport = Interp.atoi(val);
                else if (key.Equals("ClientPwd", StringComparison.OrdinalIgnoreCase)) this_imcmud.clientpw = val;
                else if (key.Equals("ServerPwd", StringComparison.OrdinalIgnoreCase)) this_imcmud.serverpw = val;
                else if (key.Equals("SHA256", StringComparison.OrdinalIgnoreCase)) this_imcmud.sha256 = Interp.atoi(val);
                else if (key.Equals("SHA256Pwd", StringComparison.OrdinalIgnoreCase)) this_imcmud.sha256pass = Interp.atoi(val);
            }
        }

        static void load_commands()
        {
            string path = Path.Combine(Dir, "imc.commands");
            if (!File.Exists(path))
                return;
            commands.Clear();
            first_imc_command = null;
            ImcCmd cur = null;
            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                if (line.Length == 0) continue;
                if (line.StartsWith("#COMMAND", StringComparison.OrdinalIgnoreCase))
                {
                    cur = new ImcCmd();
                    continue;
                }
                if (line[0] == '#') continue;
                if (line.Equals("End", StringComparison.OrdinalIgnoreCase))
                {
                    if (cur != null && cur.name.Length != 0)
                    {
                        cur.function = lookup_function(cur.code);
                        commands.Add(cur);
                        cur.next = first_imc_command;
                        first_imc_command = cur;
                    }
                    cur = null;
                    continue;
                }
                if (cur == null) continue;
                var sp = line.IndexOf(' ');
                string key = sp < 0 ? line : line.Substring(0, sp);
                string val = sp < 0 ? "" : line.Substring(sp + 1).Trim();
                if (key.Equals("Name", StringComparison.OrdinalIgnoreCase)) cur.name = val;
                else if (key.Equals("Code", StringComparison.OrdinalIgnoreCase)) cur.code = val;
                else if (key.Equals("Perm", StringComparison.OrdinalIgnoreCase)) cur.level = perm_value(val);
                else if (key.Equals("Connected", StringComparison.OrdinalIgnoreCase)) cur.connected = Interp.atoi(val) != 0;
                else if (key.Equals("Alias", StringComparison.OrdinalIgnoreCase)) cur.aliases.Add(val);
            }
            /* C links as it reads (head insert in our load is reversed vs file order).
             * imc_other lists in table order; rebuild in file order. */
            first_imc_command = null;
            ImcCmd prev = null;
            foreach (var c in commands)
            {
                c.next = null;
                if (first_imc_command == null) first_imc_command = c;
                else prev.next = c;
                prev = c;
            }
        }

        static void load_colors()
        {
            string path = Path.Combine(Dir, "imc.color");
            if (!File.Exists(path)) return;
            first_imc_color = null;
            ImcColor cur = null;
            ImcColor last = null;
            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                if (line.Length == 0) continue;
                if (line.StartsWith("#COLOR", StringComparison.OrdinalIgnoreCase))
                {
                    cur = new ImcColor();
                    continue;
                }
                if (line[0] == '#') continue;
                if (line.Equals("End", StringComparison.OrdinalIgnoreCase))
                {
                    if (cur != null)
                    {
                        if (first_imc_color == null) first_imc_color = cur;
                        else last.next = cur;
                        last = cur;
                    }
                    cur = null;
                    continue;
                }
                if (cur == null) continue;
                var sp = line.IndexOf(' ');
                string key = sp < 0 ? line : line.Substring(0, sp);
                string val = sp < 0 ? "" : line.Substring(sp + 1).Trim();
                if (key.Equals("Name", StringComparison.OrdinalIgnoreCase)) cur.name = val;
                else if (key.Equals("Mudtag", StringComparison.OrdinalIgnoreCase)) cur.mudtag = val;
                else if (key.Equals("IMCtag", StringComparison.OrdinalIgnoreCase)) cur.imctag = val;
            }
        }

        static void load_helps()
        {
            string path = Path.Combine(Dir, "imc.help");
            if (!File.Exists(path)) return;
            first_imc_help = null;
            byte[] bytes = File.ReadAllBytes(path);
            string text = System.Text.Encoding.GetEncoding(28591).GetString(bytes);
            ImcHelp last = null;
            int i = 0;
            while (i < text.Length)
            {
                int hash = text.IndexOf('#', i);
                if (hash < 0) break;
                i = hash + 1;
                while (i < text.Length && Bit.isspace(text[i])) i++;
                if (i + 3 <= text.Length && text.Substring(i, 3).Equals("END", StringComparison.OrdinalIgnoreCase))
                    break;
                if (i + 4 > text.Length || !text.Substring(i, 4).Equals("HELP", StringComparison.OrdinalIgnoreCase))
                    continue;
                i += 4;
                var help = new ImcHelp();
                while (i < text.Length)
                {
                    while (i < text.Length && (text[i] == '\n' || text[i] == '\r')) i++;
                    if (i >= text.Length) break;
                    int eol = text.IndexOf('\n', i);
                    if (eol < 0) eol = text.Length;
                    string line = text.Substring(i, eol - i).TrimEnd('\r');
                    string tline = line.Trim();
                    if (tline.Equals("End", StringComparison.OrdinalIgnoreCase))
                    {
                        i = eol + 1;
                        break;
                    }
                    if (tline.StartsWith("Name ", StringComparison.OrdinalIgnoreCase))
                        help.name = tline.Substring(5).Trim();
                    else if (tline.StartsWith("Perm ", StringComparison.OrdinalIgnoreCase))
                        help.level = perm_value(tline.Substring(5).Trim());
                    else if (tline.StartsWith("Text", StringComparison.OrdinalIgnoreCase))
                    {
                        int start = i + 4;
                        if (start < text.Length && text[start] == ' ') start++;
                        int term = text.IndexOf((char)0xA2, start);
                        if (term < 0) term = text.Length;
                        help.text = text.Substring(start, term - start).TrimEnd('\r', '\n');
                        i = term + 1;
                        continue;
                    }
                    i = eol + 1;
                }
                if (first_imc_help == null) first_imc_help = help;
                else last.next = help;
                last = help;
            }
        }

        static void load_templates()
        {
            string path = Path.Combine(Dir, "imc.who");
            who_template = File.Exists(path) ? File.ReadAllText(path) : "";
        }

        static bool startup_network()
        {
            if (this_imcmud == null || this_imcmud.rport <= 0
                || string.IsNullOrEmpty(this_imcmud.rhost))
                return false;
            /*
             * IMC network protocol is not implemented here. The previous stub
             * opened a TcpClient, set desc/state as if connected, then dropped
             * the only reference — leaking the socket — while loop() is a no-op.
             * Stay honestly offline until a real IMC client is wired up.
             */
            this_imcmud.state = IMC_OFFLINE;
            this_imcmud.desc = -1;
            return false;
        }

        public static void loop()
        {
            if (this_imcmud == null || this_imcmud.state == IMC_OFFLINE || this_imcmud.desc == -1)
                return;
        }

        public static void initchar(CharData ch)
        {
            if (Bit.IS_NPC(ch) || ch.pcdata == null)
                return;
            ch.pcdata.imc = new ImcCharData();
            ch.pcdata.imc.imcflag = IMC_COLORFLAG;
            ch.pcdata.imc.imcperm = IMCPERM_NOTSET;
        }

        public static void adjust_perms(CharData ch)
        {
            if (this_imcmud == null || ch.pcdata?.imc == null)
                return;
            if ((ch.pcdata.imc.imcflag & IMC_PERMOVERRIDE) != 0)
                return;
            int level = ch.level;
            if (level < this_imcmud.minlevel)
                ch.pcdata.imc.imcperm = IMCPERM_NONE;
            else if (level >= this_imcmud.minlevel && level < this_imcmud.immlevel)
                ch.pcdata.imc.imcperm = IMCPERM_MORT;
            else if (level >= this_imcmud.immlevel && level < this_imcmud.adminlevel)
                ch.pcdata.imc.imcperm = IMCPERM_IMM;
            else if (level >= this_imcmud.adminlevel && level < this_imcmud.implevel)
                ch.pcdata.imc.imcperm = IMCPERM_ADMIN;
            else if (level >= this_imcmud.implevel)
                ch.pcdata.imc.imcperm = IMCPERM_IMP;
        }

        public static bool command_hook(CharData ch, string command, string argument)
        {
            if (Bit.IS_NPC(ch))
                return false;
            if (this_imcmud == null)
                return false;
            if (first_imc_command == null)
                return false;
            if (ch.pcdata?.imc == null)
                initchar(ch);
            if (ch.pcdata.imc.imcperm <= IMCPERM_NONE)
                return false;

            argument ??= "";
            for (var cmd = first_imc_command; cmd != null; cmd = cmd.next)
            {
                if (ch.pcdata.imc.imcperm < cmd.level)
                    continue;
                string used = command;
                foreach (var a in cmd.aliases)
                {
                    if (string.Equals(command, a, StringComparison.OrdinalIgnoreCase))
                    {
                        used = cmd.name;
                        break;
                    }
                }
                if (!string.Equals(used, cmd.name, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (cmd.connected && this_imcmud.state < IMC_ONLINE)
                {
                    to_char("The mud is not currently connected to IMC2.\r\n", ch);
                    return true;
                }
                if (cmd.function == null)
                {
                    to_char("That command has no code set. Inform the administration.\r\n", ch);
                    return true;
                }
                cmd.function(ch, argument);
                return true;
            }
            return false;
        }

        public static void savechar(CharData ch, StreamWriter fp)
        {
            if (Bit.IS_NPC(ch) || ch.pcdata?.imc == null)
                return;
            var imc = ch.pcdata.imc;
            fp.WriteLine("IMCPerm      {0}", imc.imcperm);
            fp.WriteLine("IMCFlags     {0}", imc.imcflag);
            if (!string.IsNullOrEmpty(imc.imc_listen))
                fp.WriteLine("IMCListen    {0}", imc.imc_listen);
            if (!string.IsNullOrEmpty(imc.imc_denied))
                fp.WriteLine("IMCDeny      {0}", imc.imc_denied);
            if (!string.IsNullOrEmpty(imc.email))
                fp.WriteLine("IMCEmail     {0}", imc.email);
            if (!string.IsNullOrEmpty(imc.homepage))
                fp.WriteLine("IMCHomepage  {0}", imc.homepage);
            if (imc.icq != 0)
                fp.WriteLine("IMCICQ       {0}", imc.icq);
            if (!string.IsNullOrEmpty(imc.aim))
                fp.WriteLine("IMCAIM       {0}", imc.aim);
            if (!string.IsNullOrEmpty(imc.yahoo))
                fp.WriteLine("IMCYahoo     {0}", imc.yahoo);
            if (!string.IsNullOrEmpty(imc.msn))
                fp.WriteLine("IMCMSN       {0}", imc.msn);
            if (!string.IsNullOrEmpty(imc.comment))
                fp.WriteLine("IMCComment   {0}", imc.comment);
            for (var t = imc.imcfirst_ignore; t != null; t = t.next)
                fp.WriteLine("IMCignore    {0}", t.name);
        }

        public static bool loadchar(CharData ch, AreaReader fp, string word)
        {
            if (Bit.IS_NPC(ch) || ch.pcdata == null)
                return false;
            if (ch.pcdata.imc == null)
                initchar(ch);
            if (ch.pcdata.imc.imcperm == IMCPERM_NOTSET)
                adjust_perms(ch);
            var imc = ch.pcdata.imc;
            if (word.Length == 0 || word[0] != 'I')
                return false;
            if (!RomString.str_cmp(word, "IMCPerm"))
            {
                imc.imcperm = fp.fread_number();
                return true;
            }
            if (!RomString.str_cmp(word, "IMCEmail")) { imc.email = fread_line(fp); return true; }
            if (!RomString.str_cmp(word, "IMCAIM")) { imc.aim = fread_line(fp); return true; }
            if (!RomString.str_cmp(word, "IMCICQ")) { imc.icq = fp.fread_number(); return true; }
            if (!RomString.str_cmp(word, "IMCYahoo")) { imc.yahoo = fread_line(fp); return true; }
            if (!RomString.str_cmp(word, "IMCMSN")) { imc.msn = fread_line(fp); return true; }
            if (!RomString.str_cmp(word, "IMCHomepage")) { imc.homepage = fread_line(fp); return true; }
            if (!RomString.str_cmp(word, "IMCComment")) { imc.comment = fread_line(fp); return true; }
            if (!RomString.str_cmp(word, "IMCFlags"))
            {
                imc.imcflag = fp.fread_number();
                adjust_perms(ch);
                return true;
            }
            if (!RomString.str_cmp(word, "IMClisten")) { imc.imc_listen = fread_line(fp); return true; }
            if (!RomString.str_cmp(word, "IMCdeny")) { imc.imc_denied = fread_line(fp); return true; }
            if (!RomString.str_cmp(word, "IMCignore"))
            {
                var temp = new ImcIgnore { name = fread_line(fp) };
                temp.next = imc.imcfirst_ignore;
                imc.imcfirst_ignore = temp;
                return true;
            }
            return false;
        }

        static string fread_line(AreaReader fp)
        {
            var sb = new System.Text.StringBuilder();
            int c;
            do { c = fp.Getc(); } while (c >= 0 && (c == ' ' || c == '\t'));
            while (c >= 0 && c != '\n' && c != '\r')
            {
                sb.Append((char)c);
                c = fp.Getc();
            }
            while (c == '\n' || c == '\r')
                c = fp.Getc();
            if (c >= 0)
                fp.Ungetc(c);
            return sb.ToString();
        }

        static int perm_value(string flag)
        {
            for (int x = 0; x < perm_names.Length; x++)
            {
                if (string.Equals(flag, perm_names[x], StringComparison.OrdinalIgnoreCase))
                    return x;
            }
            return -1;
        }

        static Action<CharData, string> lookup_function(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                return null;
            if (name.Equals("imc_other", StringComparison.OrdinalIgnoreCase)) return imc_other;
            if (name.Equals("imchelp", StringComparison.OrdinalIgnoreCase)) return imchelp;
            if (name.Equals("imccedit", StringComparison.OrdinalIgnoreCase)) return imccedit;
            if (name.Equals("imchedit", StringComparison.OrdinalIgnoreCase)) return imchedit;
            if (name.Equals("imcinvis", StringComparison.OrdinalIgnoreCase)) return imcinvis;
            if (name.Equals("imcignore", StringComparison.OrdinalIgnoreCase)) return imcignore;
            if (name.Equals("imcfinger", StringComparison.OrdinalIgnoreCase)) return imcfinger;
            if (name.Equals("imccolor", StringComparison.OrdinalIgnoreCase)) return imccolor;
            if (name.Equals("imcconnect", StringComparison.OrdinalIgnoreCase)) return imcconnect;
            if (name.Equals("imcdisconnect", StringComparison.OrdinalIgnoreCase)) return imcdisconnect;
            if (name.Equals("imcpermstats", StringComparison.OrdinalIgnoreCase)) return imcpermstats;
            if (name.Equals("imcpermset", StringComparison.OrdinalIgnoreCase)) return imcpermset;
            if (name.Equals("imcconfig", StringComparison.OrdinalIgnoreCase)) return imcconfig;
            if (name.Equals("imcdebug", StringComparison.OrdinalIgnoreCase)) return imcdebug;
            if (name.Equals("imcafk", StringComparison.OrdinalIgnoreCase)) return imcafk;
            if (name.Equals("imcnotify", StringComparison.OrdinalIgnoreCase)) return imcnotify;
            if (name.Equals("imctemplates", StringComparison.OrdinalIgnoreCase)) return imctemplates;
            return null;
        }

        static string funcname(Action<CharData, string> fn)
        {
            if (fn == null) return "NULL";
            return fn.Method.Name;
        }

        static string color_itom(string txt, CharData ch)
        {
            if (string.IsNullOrEmpty(txt)) return "";
            string tbuf = txt;
            if (ch.pcdata?.imc != null && (ch.pcdata.imc.imcflag & IMC_COLORFLAG) != 0)
            {
                for (var color = first_imc_color; color != null; color = color.next)
                    if (!string.IsNullOrEmpty(color.imctag))
                        tbuf = tbuf.Replace(color.imctag, color.mudtag);
            }
            else
            {
                for (var color = first_imc_color; color != null; color = color.next)
                {
                    if (!string.IsNullOrEmpty(color.imctag))
                        tbuf = tbuf.Replace(color.imctag, "");
                    if (!string.IsNullOrEmpty(color.mudtag))
                        tbuf = tbuf.Replace(color.mudtag, "");
                }
            }
            return tbuf;
        }

        static void to_char(string txt, CharData ch)
            => Comm.send_to_char(color_itom(txt, ch) + "\x1b[0m", ch);

        static void to_pager(string txt, CharData ch)
            => Comm.page_to_char(color_itom(txt, ch) + "\x1b[0m", ch);

        static void imc_printf(CharData ch, string fmt, params object[] args)
            => to_char(RomString.sprintf(fmt, args), ch);

        static CharData find_user(string name)
        {
            for (var d = Game.descriptor_list; d != null; d = d.next)
            {
                var vch = d.character ?? d.original;
                if (vch != null && string.Equals(vch.name, name, StringComparison.OrdinalIgnoreCase)
                    && d.connected == CON_PLAYING)
                    return vch;
            }
            return null;
        }

        static bool check_permissions(CharData ch, int checkvalue, int targetvalue, bool enforceequal)
        {
            if (checkvalue < 0 || checkvalue > IMCPERM_IMP)
            {
                to_char("Invalid permission setting.\r\n", ch);
                return false;
            }
            if (checkvalue > ch.pcdata.imc.imcperm)
            {
                to_char("You cannot set permissions higher than your own.\r\n", ch);
                return false;
            }
            if (checkvalue == ch.pcdata.imc.imcperm && ch.pcdata.imc.imcperm != IMCPERM_IMP && enforceequal)
            {
                to_char("You cannot set permissions equal to your own. Someone higher up must do this.\r\n", ch);
                return false;
            }
            if (ch.pcdata.imc.imcperm < targetvalue)
            {
                to_char("You cannot alter the permissions of someone or something above your own.\r\n", ch);
                return false;
            }
            return true;
        }

        static void save_config()
        {
            if (this_imcmud == null) return;
            string path = Path.Combine(Dir, "imc.config");
            try
            {
                using var fp = new StreamWriter(path);
                fp.Write("$IMCCONFIG\n\n");
                fp.Write("# {0} config file.\n", this_imcmud.versionid);
                fp.Write("# This file can now support the use of tildes in your strings.\n");
                fp.Write("# This information can be edited online using the 'imcconfig' command.\n");
                fp.Write("LocalName      {0}\n", this_imcmud.localname);
                fp.Write("Autoconnect    {0}\n", this_imcmud.autoconnect);
                fp.Write("MinPlayerLevel {0}\n", this_imcmud.minlevel);
                fp.Write("MinImmLevel    {0}\n", this_imcmud.immlevel);
                fp.Write("AdminLevel     {0}\n", this_imcmud.adminlevel);
                fp.Write("Implevel       {0}\n", this_imcmud.implevel);
                fp.Write("InfoName       {0}\n", this_imcmud.fullname);
                fp.Write("InfoHost       {0}\n", this_imcmud.ihost);
                fp.Write("InfoPort       {0}\n", this_imcmud.iport);
                fp.Write("InfoEmail      {0}\n", this_imcmud.email);
                fp.Write("InfoWWW        {0}\n", this_imcmud.www);
                fp.Write("InfoBase       {0}\n", this_imcmud.@base);
                fp.Write("InfoDetails    {0}\n\n", this_imcmud.details);
                fp.Write("# Your server connection information goes here.\n");
                fp.Write("# This information should be available from the network you plan to join.\n");
                fp.Write("ServerAddr     {0}\n", this_imcmud.rhost);
                fp.Write("ServerPort     {0}\n", this_imcmud.rport);
                fp.Write("ClientPwd      {0}\n", this_imcmud.clientpw);
                fp.Write("ServerPwd      {0}\n", this_imcmud.serverpw);
                fp.Write("#SHA256 auth: 0 = disabled, 1 = enabled\n");
                fp.Write("SHA256         {0}\n", this_imcmud.sha256);
                if (this_imcmud.sha256pass != 0)
                {
                    fp.Write("#Your server is expecting SHA256 authentication now. Do not remove this line unless told to do so.\n");
                    fp.Write("SHA256Pwd      {0}\n", this_imcmud.sha256pass);
                }
                fp.Write("End\n\n");
                fp.Write("$END\n");
            }
            catch
            {
                Db.log_string("Couldn't write to config file.");
            }
        }

        static void save_commands()
        {
            string path = Path.Combine(Dir, "imc.commands");
            try
            {
                using var fp = new StreamWriter(path);
                for (var cmd = first_imc_command; cmd != null; cmd = cmd.next)
                {
                    fp.Write("#COMMAND\n");
                    fp.Write("Name      {0}\n", cmd.name);
                    fp.Write("Code      {0}\n", cmd.function != null ? funcname(cmd.function) : "NULL");
                    fp.Write("Perm      {0}\n", perm_names[cmd.level]);
                    fp.Write("Connected {0}\n", cmd.connected ? 1 : 0);
                    foreach (var a in cmd.aliases)
                        fp.Write("Alias     {0}\n", a);
                    fp.Write("End\n\n");
                }
                fp.Write("#END\n");
            }
            catch
            {
                Db.log_string("Couldn't write to IMC2 command file.");
            }
        }

        static void save_helps()
        {
            string path = Path.Combine(Dir, "imc.help");
            try
            {
                using var fp = new StreamWriter(path, false, System.Text.Encoding.GetEncoding(28591));
                for (var help = first_imc_help; help != null; help = help.next)
                {
                    fp.Write("#HELP\n");
                    fp.Write("Name {0}\n", help.name);
                    fp.Write("Perm {0}\n", perm_names[help.level]);
                    fp.Write("Text {0}\n", help.text);
                    fp.Write((char)0xA2);
                    fp.Write("\nEnd\n\n");
                }
                fp.Write("#END\n");
            }
            catch
            {
                Db.log_string("Couldn't write to IMC2 help file.");
            }
        }

        static void imc_other(CharData ch, string argument)
        {
            var buf = new System.Text.StringBuilder();
            buf.Append("~gThe following commands are available:\r\n");
            buf.Append("~G-------------------------------------\r\n\r\n");
            for (int perm = IMCPERM_MORT; perm <= ch.pcdata.imc.imcperm; perm++)
            {
                int col = 0;
                buf.Append(RomString.sprintf("\r\n~g%s commands:~G\r\n", perm_names[perm]));
                for (var cmd = first_imc_command; cmd != null; cmd = cmd.next)
                {
                    if (cmd.level != perm) continue;
                    buf.Append(RomString.sprintf("%-15s", cmd.name));
                    if (++col % 6 == 0) buf.Append("\r\n");
                }
                if (col % 6 != 0) buf.Append("\r\n");
            }
            to_pager(buf.ToString(), ch);
            to_pager("\r\n~gFor information about a specific command, see ~Wimchelp <command>~g.\r\n", ch);
        }

        static void imchelp(CharData ch, string argument)
        {
            argument ??= "";
            if (argument.Length == 0)
            {
                var buf = new System.Text.StringBuilder();
                buf.Append("~gHelp is available for the following commands:\r\n");
                buf.Append("~G---------------------------------------------\r\n");
                for (int perm = IMCPERM_MORT; perm <= ch.pcdata.imc.imcperm; perm++)
                {
                    int col = 0;
                    buf.Append(RomString.sprintf("\r\n~g%s helps:~G\r\n", perm_names[perm]));
                    for (var help = first_imc_help; help != null; help = help.next)
                    {
                        if (help.level != perm) continue;
                        buf.Append(RomString.sprintf("%-15s", help.name));
                        if (++col % 6 == 0) buf.Append("\r\n");
                    }
                    if (col % 6 != 0) buf.Append("\r\n");
                }
                to_pager(buf.ToString(), ch);
                return;
            }
            for (var help = first_imc_help; help != null; help = help.next)
            {
                if (string.Equals(help.name, argument, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(help.text))
                        imc_printf(ch, "~gNo inforation available for topic ~W%s~g.\r\n", help.name);
                    else
                        imc_printf(ch, "~g%s\r\n", help.text);
                    return;
                }
            }
            imc_printf(ch, "~gNo help exists for topic ~W%s~g.\r\n", argument);
        }

        static void imcinvis(CharData ch, string argument)
        {
            if ((ch.pcdata.imc.imcflag & IMC_INVIS) != 0)
            {
                ch.pcdata.imc.imcflag &= ~IMC_INVIS;
                to_char("You are now imcvisible.\r\n", ch);
            }
            else
            {
                ch.pcdata.imc.imcflag |= IMC_INVIS;
                to_char("You are now imcinvisible.\r\n", ch);
            }
        }

        static void imccolor(CharData ch, string argument)
        {
            if ((ch.pcdata.imc.imcflag & IMC_COLORFLAG) != 0)
            {
                ch.pcdata.imc.imcflag &= ~IMC_COLORFLAG;
                to_char("IMC2 color is now off.\r\n", ch);
            }
            else
            {
                ch.pcdata.imc.imcflag |= IMC_COLORFLAG;
                to_char("~RIMC2 c~Yo~Gl~Bo~Pr ~Ris now on. Enjoy :)\r\n", ch);
            }
        }

        static void imcafk(CharData ch, string argument)
        {
            if ((ch.pcdata.imc.imcflag & IMC_AFK) != 0)
            {
                ch.pcdata.imc.imcflag &= ~IMC_AFK;
                to_char("You are no longer AFK to IMC2.\r\n", ch);
            }
            else
            {
                ch.pcdata.imc.imcflag |= IMC_AFK;
                to_char("You are now AFK to IMC2.\r\n", ch);
            }
        }

        static void imcnotify(CharData ch, string argument)
        {
            if ((ch.pcdata.imc.imcflag & IMC_NOTIFY) != 0)
            {
                ch.pcdata.imc.imcflag &= ~IMC_NOTIFY;
                to_char("You no longer see channel notifications.\r\n", ch);
            }
            else
            {
                ch.pcdata.imc.imcflag |= IMC_NOTIFY;
                to_char("You now see channel notifications.\r\n", ch);
            }
        }

        static void imcdebug(CharData ch, string argument)
        {
            imcpacketdebug = !imcpacketdebug;
            to_char(imcpacketdebug ? "Packet debug enabled.\r\n" : "Packet debug disabled.\r\n", ch);
        }

        static void imctemplates(CharData ch, string argument)
        {
            to_char("Refreshing all templates.\r\n", ch);
            load_templates();
        }

        static void imcconnect(CharData ch, string argument)
        {
            if (this_imcmud != null && this_imcmud.state > IMC_OFFLINE)
            {
                to_char("The IMC2 network connection appears to already be engaged!\r\n", ch);
                return;
            }
            startup_network();
        }

        static void imcdisconnect(CharData ch, string argument)
        {
            if (this_imcmud != null && this_imcmud.state == IMC_OFFLINE)
            {
                to_char("The IMC2 network connection does not appear to be engaged!\r\n", ch);
                return;
            }
            if (this_imcmud != null)
            {
                this_imcmud.state = IMC_OFFLINE;
                this_imcmud.desc = -1;
            }
        }

        static void imcignore(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument ?? "", out string arg);
            if (arg.Length == 0)
            {
                to_char("You currently ignore the following:\r\n", ch);
                int count = 0;
                for (var ign = ch.pcdata.imc.imcfirst_ignore; ign != null; ign = ign.next, count++)
                    imc_printf(ch, "%s\r\n", ign.name);
                if (count == 0)
                    to_char(" none\r\n", ch);
                else
                    imc_printf(ch, "\r\n[total %d]\r\n", count);
                to_char("For help on imcignore, type: IMCIGNORE HELP\r\n", ch);
                return;
            }
            if (string.Equals(arg, "help", StringComparison.OrdinalIgnoreCase))
            {
                to_char("~wTo see your current ignores  : ~GIMCIGNORE\r\n", ch);
                to_char("~wTo add an ignore             : ~GIMCIGNORE ADD <argument>\r\n", ch);
                to_char("~wTo delete an ignore          : ~GIMCIGNORE DELETE <argument>\r\n", ch);
                to_char("~WSee your MUD's help for more information.\r\n", ch);
                return;
            }
            if (string.IsNullOrEmpty(argument))
            {
                to_char("Must specify both action and name.\r\n", ch);
                to_char("Please see IMCIGNORE HELP for details.\r\n", ch);
                return;
            }
            if (string.Equals(arg, "delete", StringComparison.OrdinalIgnoreCase))
            {
                ImcIgnore prev = null;
                for (var ign = ch.pcdata.imc.imcfirst_ignore; ign != null; prev = ign, ign = ign.next)
                {
                    if (string.Equals(ign.name, argument, StringComparison.OrdinalIgnoreCase))
                    {
                        if (prev == null) ch.pcdata.imc.imcfirst_ignore = ign.next;
                        else prev.next = ign.next;
                        to_char("Entry deleted.\r\n", ch);
                        return;
                    }
                }
                to_char("Entry not found.\r\nPlease check your ignores by typing IMCIGNORE with no arguments.\r\n", ch);
                return;
            }
            if (string.Equals(arg, "add", StringComparison.OrdinalIgnoreCase))
            {
                var ign = new ImcIgnore { name = argument, next = ch.pcdata.imc.imcfirst_ignore };
                ch.pcdata.imc.imcfirst_ignore = ign;
                imc_printf(ch, "%s will now be ignored.\r\n", argument);
                return;
            }
            imcignore(ch, "help");
        }

        static void imcfinger(CharData ch, string argument)
        {
            if ((ch.pcdata.imc.imcflag & IMC_DENYFINGER) != 0)
            {
                to_char("You are not authorized to use imcfinger.\r\n", ch);
                return;
            }
            argument = RomString.one_argument(argument ?? "", out string arg);
            if (arg.Length == 0)
            {
                to_char("~wUsage: imcfinger person@mud\r\n", ch);
                to_char("~wUsage: imcfinger <field> <value>\r\n", ch);
                to_char("~wWhere field is one of:\r\n\r\n", ch);
                to_char("~wdisplay email homepage icq aim yahoo msn privacy comment\r\n", ch);
                return;
            }
            var imc = ch.pcdata.imc;
            if (string.Equals(arg, "display", StringComparison.OrdinalIgnoreCase))
            {
                to_char("~GYour current information:\r\n\r\n", ch);
                imc_printf(ch, "~GEmail   : ~g%s\r\n", string.IsNullOrEmpty(imc.email) ? "None" : imc.email);
                imc_printf(ch, "~GHomepage: ~g%s\r\n", string.IsNullOrEmpty(imc.homepage) ? "None" : imc.homepage);
                imc_printf(ch, "~GICQ     : ~g%d\r\n", imc.icq);
                imc_printf(ch, "~GAIM     : ~g%s\r\n", string.IsNullOrEmpty(imc.aim) ? "None" : imc.aim);
                imc_printf(ch, "~GYahoo   : ~g%s\r\n", string.IsNullOrEmpty(imc.yahoo) ? "None" : imc.yahoo);
                imc_printf(ch, "~GMSN     : ~g%s\r\n", string.IsNullOrEmpty(imc.msn) ? "None" : imc.msn);
                imc_printf(ch, "~GComment : ~g%s\r\n", string.IsNullOrEmpty(imc.comment) ? "None" : imc.comment);
                imc_printf(ch, "~GPrivacy : ~g%s\r\n", (imc.imcflag & IMC_PRIVACY) != 0 ? "Enabled" : "Disabled");
                return;
            }
            if (string.Equals(arg, "privacy", StringComparison.OrdinalIgnoreCase))
            {
                if ((imc.imcflag & IMC_PRIVACY) != 0)
                {
                    imc.imcflag &= ~IMC_PRIVACY;
                    to_char("Privacy flag removed. Your information will now be visible on imcfinger.\r\n", ch);
                }
                else
                {
                    imc.imcflag |= IMC_PRIVACY;
                    to_char("Privacy flag enabled. Your information will no longer be visible on imcfinger.\r\n", ch);
                }
                return;
            }
            if (string.IsNullOrEmpty(argument))
            {
                if (this_imcmud.state != IMC_ONLINE)
                {
                    to_char("The mud is not currently connected to IMC2.\r\n", ch);
                    return;
                }
                return;
            }
            if (string.Equals(arg, "email", StringComparison.OrdinalIgnoreCase))
            {
                imc.email = argument;
                imc_printf(ch, "Your email address has changed to: %s\r\n", imc.email);
                return;
            }
            if (string.Equals(arg, "homepage", StringComparison.OrdinalIgnoreCase))
            {
                imc.homepage = argument;
                imc_printf(ch, "Your homepage has changed to: %s\r\n", imc.homepage);
                return;
            }
            if (string.Equals(arg, "icq", StringComparison.OrdinalIgnoreCase))
            {
                imc.icq = Interp.atoi(argument);
                imc_printf(ch, "Your ICQ Number has changed to: %d\r\n", imc.icq);
                return;
            }
            if (string.Equals(arg, "aim", StringComparison.OrdinalIgnoreCase))
            {
                imc.aim = argument;
                imc_printf(ch, "Your AIM Screenname has changed to: %s\r\n", imc.aim);
                return;
            }
            if (string.Equals(arg, "yahoo", StringComparison.OrdinalIgnoreCase))
            {
                imc.yahoo = argument;
                imc_printf(ch, "Your Yahoo Screenname has changed to: %s\r\n", imc.yahoo);
                return;
            }
            if (string.Equals(arg, "msn", StringComparison.OrdinalIgnoreCase))
            {
                imc.msn = argument;
                imc_printf(ch, "Your MSN Screenname has changed to: %s\r\n", imc.msn);
                return;
            }
            if (string.Equals(arg, "comment", StringComparison.OrdinalIgnoreCase))
            {
                if (argument.Length > 78)
                {
                    to_char("You must limit the comment line to 78 characters or less.\r\n", ch);
                    return;
                }
                imc.comment = argument;
                imc_printf(ch, "Your comment line has changed to: %s\r\n", imc.comment);
                return;
            }
            imcfinger(ch, "");
        }

        static void imcpermstats(CharData ch, string argument)
        {
            if (string.IsNullOrEmpty(argument))
            {
                to_char("Usage: imcperms <user>\r\n", ch);
                return;
            }
            var victim = find_user(argument);
            if (victim == null)
            {
                to_char("No such person is currently online.\r\n", ch);
                return;
            }
            if (victim.pcdata?.imc == null)
            {
                to_char("No such person is currently online.\r\n", ch);
                return;
            }
            if (victim.pcdata.imc.imcperm < 0 || victim.pcdata.imc.imcperm > IMCPERM_IMP)
            {
                imc_printf(ch, "%s has an invalid permission setting!\r\n", victim.name);
                return;
            }
            imc_printf(ch, "~GPermissions for %s: %s\r\n", victim.name, perm_names[victim.pcdata.imc.imcperm]);
            imc_printf(ch, "~gThese permissions were obtained %s.\r\n",
                (victim.pcdata.imc.imcflag & IMC_PERMOVERRIDE) != 0
                    ? "manually via imcpermset" : "automatically by level");
        }

        static void imcpermset(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument ?? "", out string arg);
            if (arg.Length == 0)
            {
                to_char("Usage: imcpermset <user> <permission>\r\n", ch);
                to_char("Permission can be one of: None, Mort, Imm, Admin, Imp\r\n", ch);
                return;
            }
            var victim = find_user(arg);
            if (victim?.pcdata?.imc == null)
            {
                to_char("No such person is currently online.\r\n", ch);
                return;
            }
            int permvalue;
            if (string.Equals(argument, "override", StringComparison.OrdinalIgnoreCase))
                permvalue = -1;
            else
            {
                permvalue = perm_value(argument);
                if (!check_permissions(ch, permvalue, victim.pcdata.imc.imcperm, true))
                    return;
            }
            if (victim.pcdata.imc.imcperm == permvalue)
            {
                imc_printf(ch, "%s already has a permission level of %s.\r\n", victim.name, perm_names[permvalue]);
                return;
            }
            if (permvalue == -1)
            {
                victim.pcdata.imc.imcflag &= ~IMC_PERMOVERRIDE;
                imc_printf(ch, "~YPermission flag override has been removed from %s\r\n", victim.name);
                return;
            }
            victim.pcdata.imc.imcperm = permvalue;
            victim.pcdata.imc.imcflag |= IMC_PERMOVERRIDE;
            imc_printf(ch, "~YPermission level for %s has been changed to %s\r\n", victim.name, perm_names[permvalue]);
        }

        static void imcconfig(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument ?? "", out string arg1);
            if (arg1.Length == 0)
            {
                to_char("~wSyntax: &Gimc <field> [value]\r\n\r\n", ch);
                to_char("~wConfiguration info for your mud. Changes save when edited.\r\n", ch);
                to_char("~wYou may set the following:\r\n\r\n", ch);
                to_char("~wShow           : ~GDisplays your current configuration.\r\n", ch);
                to_char("~wLocalname      : ~GThe name IMC2 knows your mud by.\r\n", ch);
                to_char("~wAutoconnect    : ~GToggles automatic connection on reboots.\r\n", ch);
                to_char("~wMinPlayerLevel : ~GSets the minimum level IMC2 can see your players at.\r\n", ch);
                to_char("~wMinImmLevel    : ~GSets the level at which immortal commands become available.\r\n", ch);
                to_char("~wAdminlevel     : ~GSets the level at which administrative commands become available.\r\n", ch);
                to_char("~wImplevel       : ~GSets the level at which immplementor commands become available.\r\n", ch);
                to_char("~wInfoname       : ~GName of your mud, as seen from the imcquery info sheet.\r\n", ch);
                to_char("~wInfohost       : ~GTelnet address of your mud.\r\n", ch);
                to_char("~wInfoport       : ~GTelnet port of your mud.\r\n", ch);
                to_char("~wInfoemail      : ~GEmail address of the mud's IMC administrator.\r\n", ch);
                to_char("~wInfoWWW        : ~GThe Web address of your mud.\r\n", ch);
                to_char("~wInfoBase       : ~GThe codebase your mud uses.\r\n", ch);
                to_char("~wInfoDetails    : ~GSHORT Description of your mud.\r\n", ch);
                to_char("~wServerAddr     : ~GDNS or IP address of the server you mud connects to.\r\n", ch);
                to_char("~wServerPort     : ~GPort of the server your mud connects to.\r\n", ch);
                to_char("~wClientPwd      : ~GClient password for your mud.\r\n", ch);
                to_char("~wServerPwd      : ~GServer password for your mud.\r\n", ch);
                return;
            }
            if (string.Equals(arg1, "sha256", StringComparison.OrdinalIgnoreCase))
            {
                this_imcmud.sha256 = this_imcmud.sha256 != 0 ? 0 : 1;
                to_char(this_imcmud.sha256 != 0 ? "SHA-256 support enabled.\r\n" : "SHA-256 support disabled.\r\n", ch);
                save_config();
                return;
            }
            if (string.Equals(arg1, "sha256pass", StringComparison.OrdinalIgnoreCase))
            {
                this_imcmud.sha256pass = this_imcmud.sha256pass != 0 ? 0 : 1;
                to_char(this_imcmud.sha256pass != 0 ? "SHA-256 Authentication enabled.\r\n" : "SHA-256 Authentication disabled.\r\n", ch);
                save_config();
                return;
            }
            if (string.Equals(arg1, "autoconnect", StringComparison.OrdinalIgnoreCase))
            {
                this_imcmud.autoconnect = this_imcmud.autoconnect != 0 ? 0 : 1;
                to_char(this_imcmud.autoconnect != 0 ? "Autoconnect enabled.\r\n" : "Autoconnect disabled.\r\n", ch);
                save_config();
                return;
            }
            if (string.Equals(arg1, "show", StringComparison.OrdinalIgnoreCase))
            {
                imc_printf(ch, "~wLocalname      : ~G%s\r\n", this_imcmud.localname);
                imc_printf(ch, "~wAutoconnect    : ~G%s\r\n", this_imcmud.autoconnect != 0 ? "Enabled" : "Disabled");
                imc_printf(ch, "~wMinPlayerLevel : ~G%d\r\n", this_imcmud.minlevel);
                imc_printf(ch, "~wMinImmLevel    : ~G%d\r\n", this_imcmud.immlevel);
                imc_printf(ch, "~wAdminlevel     : ~G%d\r\n", this_imcmud.adminlevel);
                imc_printf(ch, "~wImplevel       : ~G%d\r\n", this_imcmud.implevel);
                imc_printf(ch, "~wInfoname       : ~G%s\r\n", this_imcmud.fullname);
                imc_printf(ch, "~wInfohost       : ~G%s\r\n", this_imcmud.ihost);
                imc_printf(ch, "~wInfoport       : ~G%d\r\n", this_imcmud.iport);
                imc_printf(ch, "~wInfoemail      : ~G%s\r\n", this_imcmud.email);
                imc_printf(ch, "~wInfoWWW        : ~G%s\r\n", this_imcmud.www);
                imc_printf(ch, "~wInfoBase       : ~G%s\r\n", this_imcmud.@base);
                imc_printf(ch, "~wInfoDetails    : ~G%s\r\n\r\n", this_imcmud.details);
                imc_printf(ch, "~wServerAddr     : ~G%s\r\n", this_imcmud.rhost);
                imc_printf(ch, "~wServerPort     : ~G%d\r\n", this_imcmud.rport);
                imc_printf(ch, "~wClientPwd      : ~G%s\r\n", this_imcmud.clientpw);
                imc_printf(ch, "~wServerPwd      : ~G%s\r\n", this_imcmud.serverpw);
                to_char(this_imcmud.sha256 != 0
                    ? "~RThis mud has enabled SHA-256 authentication.\r\n"
                    : "~RThis mud has disabled SHA-256 authentication.\r\n", ch);
                to_char(this_imcmud.sha256 != 0 && this_imcmud.sha256pass != 0
                    ? "~RThe mud is using SHA-256 encryption to authenticate.\r\n"
                    : "~RThe mud is using plain text passwords to authenticate.\r\n", ch);
                return;
            }
            if (string.IsNullOrEmpty(argument))
            {
                imcconfig(ch, "");
                return;
            }
            if (string.Equals(arg1, "minplayerlevel", StringComparison.OrdinalIgnoreCase))
            {
                this_imcmud.minlevel = Interp.atoi(argument);
                imc_printf(ch, "Minimum level set to %d\r\n", this_imcmud.minlevel);
                save_config();
                return;
            }
            if (string.Equals(arg1, "minimmlevel", StringComparison.OrdinalIgnoreCase))
            {
                this_imcmud.immlevel = Interp.atoi(argument);
                imc_printf(ch, "Immortal level set to %d\r\n", this_imcmud.immlevel);
                save_config();
                return;
            }
            if (string.Equals(arg1, "adminlevel", StringComparison.OrdinalIgnoreCase))
            {
                this_imcmud.adminlevel = Interp.atoi(argument);
                imc_printf(ch, "Admin level set to %d\r\n", this_imcmud.adminlevel);
                save_config();
                return;
            }
            if (string.Equals(arg1, "implevel", StringComparison.OrdinalIgnoreCase) && ch.pcdata.imc.imcperm == IMCPERM_IMP)
            {
                this_imcmud.implevel = Interp.atoi(argument);
                imc_printf(ch, "Implementor level set to %d\r\n", this_imcmud.implevel);
                save_config();
                return;
            }
            if (string.Equals(arg1, "infoname", StringComparison.OrdinalIgnoreCase))
            {
                this_imcmud.fullname = argument; save_config();
                imc_printf(ch, "Infoname change to %s\r\n", argument); return;
            }
            if (string.Equals(arg1, "infohost", StringComparison.OrdinalIgnoreCase))
            {
                this_imcmud.ihost = argument; save_config();
                imc_printf(ch, "Infohost changed to %s\r\n", argument); return;
            }
            if (string.Equals(arg1, "infoport", StringComparison.OrdinalIgnoreCase))
            {
                this_imcmud.iport = Interp.atoi(argument); save_config();
                imc_printf(ch, "Infoport changed to %d\r\n", this_imcmud.iport); return;
            }
            if (string.Equals(arg1, "infoemail", StringComparison.OrdinalIgnoreCase))
            {
                this_imcmud.email = argument; save_config();
                imc_printf(ch, "Infoemail changed to %s\r\n", argument); return;
            }
            if (string.Equals(arg1, "infowww", StringComparison.OrdinalIgnoreCase))
            {
                this_imcmud.www = argument; save_config();
                imc_printf(ch, "InfoWWW changed to %s\r\n", argument); return;
            }
            if (string.Equals(arg1, "infobase", StringComparison.OrdinalIgnoreCase))
            {
                this_imcmud.@base = argument; save_config();
                imc_printf(ch, "Infobase changed to %s\r\n", argument); return;
            }
            if (string.Equals(arg1, "infodetails", StringComparison.OrdinalIgnoreCase))
            {
                this_imcmud.details = argument; save_config();
                to_char("Infodetails updated.\r\n", ch); return;
            }
            if (this_imcmud.state != IMC_OFFLINE)
            {
                imc_printf(ch, "Cannot alter %s while the mud is connected to IMC.\r\n", arg1);
                return;
            }
            if (string.Equals(arg1, "serveraddr", StringComparison.OrdinalIgnoreCase))
            {
                this_imcmud.rhost = argument; save_config();
                imc_printf(ch, "ServerAddr changed to %s\r\n", argument); return;
            }
            if (string.Equals(arg1, "serverport", StringComparison.OrdinalIgnoreCase))
            {
                this_imcmud.rport = Interp.atoi(argument); save_config();
                imc_printf(ch, "ServerPort changed to %d\r\n", this_imcmud.rport); return;
            }
            if (string.Equals(arg1, "clientpwd", StringComparison.OrdinalIgnoreCase))
            {
                this_imcmud.clientpw = argument; save_config();
                imc_printf(ch, "Clientpwd changed to %s\r\n", argument); return;
            }
            if (string.Equals(arg1, "serverpwd", StringComparison.OrdinalIgnoreCase))
            {
                this_imcmud.serverpw = argument; save_config();
                imc_printf(ch, "Serverpwd changed to %s\r\n", argument); return;
            }
            if (string.Equals(arg1, "localname", StringComparison.OrdinalIgnoreCase))
            {
                this_imcmud.localname = argument;
                this_imcmud.sha256pass = 0;
                save_config();
                imc_printf(ch, "Localname changed to %s\r\n", argument); return;
            }
            imcconfig(ch, "");
        }

        static void imccedit(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument ?? "", out string name);
            argument = RomString.one_argument(argument, out string option);
            if (name.Length == 0 || option.Length == 0)
            {
                to_char("Usage: imccedit <command> <create|delete|alias|rename|code|permission|connected> <field>.\r\n", ch);
                return;
            }
            ImcCmd cmd = null;
            bool found = false, aliasfound = false;
            for (var c = first_imc_command; c != null; c = c.next)
            {
                if (string.Equals(c.name, name, StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                    cmd = c;
                    break;
                }
                foreach (var a in c.aliases)
                    if (string.Equals(a, name, StringComparison.OrdinalIgnoreCase))
                        aliasfound = true;
            }
            if (string.Equals(option, "create", StringComparison.OrdinalIgnoreCase))
            {
                if (found)
                {
                    imc_printf(ch, "~gA command named ~W%s ~galready exists.\r\n", name);
                    return;
                }
                if (aliasfound)
                {
                    imc_printf(ch, "~g%s already exists as an alias for another command.\r\n", name);
                    return;
                }
                cmd = new ImcCmd { name = name, level = ch.pcdata.imc.imcperm, connected = false };
                if (!string.IsNullOrEmpty(argument))
                {
                    cmd.function = lookup_function(argument);
                    cmd.code = argument;
                    if (cmd.function == null)
                        imc_printf(ch, "~gFunction ~W%s ~gdoes not exist - set to NULL.\r\n", argument);
                }
                else
                    to_char("~gFunction set to NULL.\r\n", ch);
                if (first_imc_command == null) first_imc_command = cmd;
                else
                {
                    var t = first_imc_command;
                    while (t.next != null) t = t.next;
                    t.next = cmd;
                }
                commands.Add(cmd);
                imc_printf(ch, "~gCommand ~W%s ~gcreated.\r\n", cmd.name);
                save_commands();
                return;
            }
            if (!found)
            {
                imc_printf(ch, "~gNo command named ~W%s ~gexists.\r\n", name);
                return;
            }
            if (!check_permissions(ch, cmd.level, cmd.level, false))
                return;
            if (string.Equals(option, "delete", StringComparison.OrdinalIgnoreCase))
            {
                imc_printf(ch, "~gCommand ~W%s ~ghas been deleted.\r\n", cmd.name);
                if (first_imc_command == cmd) first_imc_command = cmd.next;
                else
                {
                    for (var t = first_imc_command; t != null; t = t.next)
                        if (t.next == cmd) { t.next = cmd.next; break; }
                }
                commands.Remove(cmd);
                save_commands();
                return;
            }
            if (string.Equals(option, "alias", StringComparison.OrdinalIgnoreCase))
            {
                for (int i = 0; i < cmd.aliases.Count; i++)
                {
                    if (string.Equals(cmd.aliases[i], argument, StringComparison.OrdinalIgnoreCase))
                    {
                        imc_printf(ch, "~W%s ~ghas been removed as an alias for ~W%s\r\n", argument, cmd.name);
                        cmd.aliases.RemoveAt(i);
                        save_commands();
                        return;
                    }
                }
                cmd.aliases.Add(argument);
                imc_printf(ch, "~W%s ~ghas been added as an alias for ~W%s\r\n", argument, cmd.name);
                save_commands();
                return;
            }
            if (string.Equals(option, "connected", StringComparison.OrdinalIgnoreCase))
            {
                cmd.connected = !cmd.connected;
                imc_printf(ch, cmd.connected
                    ? "~gCommand ~W%s ~gwill now require a connection to IMC2 to use.\r\n"
                    : "~gCommand ~W%s ~gwill no longer require a connection to IMC2 to use.\r\n", cmd.name);
                save_commands();
                return;
            }
            if (string.Equals(option, "show", StringComparison.OrdinalIgnoreCase))
            {
                imc_printf(ch, "~gCommand       : ~W%s\r\n", cmd.name);
                imc_printf(ch, "~gPermission    : ~W%s\r\n", perm_names[cmd.level]);
                imc_printf(ch, "~gFunction      : ~W%s\r\n", funcname(cmd.function));
                imc_printf(ch, "~gConnection Req: ~W%s\r\n", cmd.connected ? "Yes" : "No");
                return;
            }
            if (string.IsNullOrEmpty(argument))
            {
                to_char("Required argument missing.\r\n", ch);
                imccedit(ch, "");
                return;
            }
            if (string.Equals(option, "rename", StringComparison.OrdinalIgnoreCase))
            {
                imc_printf(ch, "~gCommand ~W%s ~ghas been renamed to ~W%s.\r\n", cmd.name, argument);
                cmd.name = argument;
                save_commands();
                return;
            }
            if (string.Equals(option, "code", StringComparison.OrdinalIgnoreCase))
            {
                cmd.function = lookup_function(argument);
                cmd.code = argument;
                if (cmd.function == null)
                    imc_printf(ch, "~gFunction ~W%s ~gdoes not exist - set to NULL.\r\n", argument);
                else
                    imc_printf(ch, "~gFunction set to ~W%s.\r\n", argument);
                save_commands();
                return;
            }
            if (string.Equals(option, "perm", StringComparison.OrdinalIgnoreCase)
                || string.Equals(option, "permission", StringComparison.OrdinalIgnoreCase))
            {
                int permvalue = perm_value(argument);
                if (!check_permissions(ch, permvalue, cmd.level, false))
                    return;
                cmd.level = permvalue;
                imc_printf(ch, "~gCommand ~W%s ~gpermission level has been changed to ~W%s.\r\n",
                    cmd.name, perm_names[permvalue]);
                save_commands();
                return;
            }
            imccedit(ch, "");
        }

        static void imchedit(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument ?? "", out string name);
            argument = RomString.one_argument(argument, out string cmd);
            if (name.Length == 0 || cmd.Length == 0 || string.IsNullOrEmpty(argument))
            {
                to_char("Usage: imchedit <topic> [name|perm] <field>\r\n", ch);
                to_char("Where <field> can be either name, or permission level.\r\n", ch);
                return;
            }
            ImcHelp help = null;
            for (var h = first_imc_help; h != null; h = h.next)
            {
                if (string.Equals(h.name, name, StringComparison.OrdinalIgnoreCase))
                {
                    help = h;
                    break;
                }
            }
            if (help == null)
            {
                imc_printf(ch, "~gNo help exists for topic ~W%s~g. You will need to add it to the helpfile manually.\r\n", name);
                return;
            }
            if (string.Equals(cmd, "name", StringComparison.OrdinalIgnoreCase))
            {
                imc_printf(ch, "~W%s ~ghas been renamed to ~W%s.\r\n", help.name, argument);
                help.name = argument;
                save_helps();
                return;
            }
            if (string.Equals(cmd, "perm", StringComparison.OrdinalIgnoreCase))
            {
                int permvalue = perm_value(argument);
                if (!check_permissions(ch, permvalue, help.level, false))
                    return;
                imc_printf(ch, "~gPermission level for ~W%s ~ghas been changed to ~W%s.\r\n",
                    help.name, perm_names[permvalue]);
                help.level = permvalue;
                save_helps();
                return;
            }
            imchedit(ch, "");
        }
    }

    public class ImcCharData
    {
        public long imcflag;
        public int imcperm;
        public string imc_listen = "";
        public string imc_denied = "";
        public string email = "";
        public string homepage = "";
        public string aim = "";
        public string yahoo = "";
        public string msn = "";
        public string comment = "";
        public int icq;
        public Imc.ImcIgnore imcfirst_ignore;
    }
}
