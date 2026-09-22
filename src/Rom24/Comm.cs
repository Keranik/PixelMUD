using System.Net;
using System.Net.Sockets;
using System.Text;
using static Rom24.Merc;

namespace Rom24
{
    public static class Comm
    {
        static readonly byte[] IAC_WILL_ECHO = { 255, 251, 1 };
        static readonly byte[] IAC_WONT_ECHO = { 255, 252, 1 };
        static readonly byte[] GA = { 255, 249 };
        /* telnet.h echo_off_str / echo_on_str */
        public static readonly string echo_off_str =
            new string(new[] { (char)255, (char)251, (char)1 });
        public static readonly string echo_on_str =
            new string(new[] { (char)255, (char)252, (char)1 });
        static Socket control;

        /* Copyover support: let CopyOver hand the listening socket (and fresh
           descriptor ids) across the process boundary. */
        internal static Socket ControlSocket => control;
        internal static void AdoptControlSocket(Socket s) { control = s; }
        internal static int NextDescriptorId() => ++last_desc;

        public static void close_control()
        {
            try { control?.Close(); } catch { }
            control = null;
        }

        public static void write_to_buffer(DescriptorData d, string txt, int length = 0)
        {
            if (txt == null) return;
            if (d.outbuf.Length == 0 && !d.fcommand)
                d.outbuf.Append("\n\r");
            d.outbuf.Append(txt);
        }

        public static bool write_to_descriptor(int desc, string txt, int length)
        {
            if (txt == null)
                return false;
            if (length <= 0)
                length = txt.Length;

            DescriptorData found = null;
            for (var d = Game.descriptor_list; d != null; d = d.next)
            {
                if (d.descriptor == desc)
                {
                    found = d;
                    break;
                }
            }
            if (found?.socket == null || !found.socket.Connected)
                return false;

            try
            {
                int n = length < txt.Length ? length : txt.Length;
                var bytes = Encoding.Latin1.GetBytes(txt.Substring(0, n));
                int iStart = 0;
                while (iStart < bytes.Length)
                {
                    int nBlock = Bit.UMIN(bytes.Length - iStart, 4096);
                    int nWrite = found.socket.Send(bytes, iStart, nBlock, SocketFlags.None);
                    /* Send returns 0 on graceful close; treat as write failure. */
                    if (nWrite <= 0)
                        return false;
                    iStart += nWrite;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void send_to_desc(string txt, DescriptorData d)
        {
            if (d == null || txt == null) return;
            write_to_buffer(d, Colour.colour_string(txt, d.character, d.ansi));
        }

        public static void send_to_char(string txt, CharData ch)
        {
            if (ch?.desc == null || txt == null) return;
            bool colour = Bit.IS_SET(ch.act, PLR_COLOUR);
            write_to_buffer(ch.desc, Colour.colour_string(txt, ch, colour));
        }

        public static void send_to_char_bw(string txt, CharData ch)
        {
            if (txt != null && ch?.desc != null)
                write_to_buffer(ch.desc, txt);
        }

        public static void page_to_char(string txt, CharData ch)
        {
            if (txt == null || ch?.desc == null)
                return;
            bool colour = Bit.IS_SET(ch.act, PLR_COLOUR);
            string buf = Colour.colour_string(txt, ch, colour);
            ch.desc.showstr_head = buf;
            ch.desc.showstr_point = buf;
            show_string(ch.desc, "");
        }

        public static void printf_to_char(CharData ch, string fmt, params object[] args)
        {
            send_to_char(RomString.sprintf(fmt, args), ch);
        }

        /* string pager — comm.c show_string */
        public static void show_string(DescriptorData d, string input)
        {
            RomString.one_argument(input ?? "", out string buf);
            if (buf.Length != 0)
            {
                d.showstr_head = null;
                d.showstr_point = null;
                return;
            }

            int show_lines = d.character != null ? d.character.lines : 0;
            string point = d.showstr_point ?? "";
            var buffer = new StringBuilder();
            int lines = 0;
            int toggle = 1;
            int i = 0;
            for (;;)
            {
                char scan = i < point.Length ? point[i] : '\0';
                if ((scan == '\n' || scan == '\r') && (toggle = -toggle) < 0)
                {
                    buffer.Append(scan);
                    lines++;
                    i++;
                }
                else if (scan == '\0' || (show_lines > 0 && lines >= show_lines))
                {
                    write_to_buffer(d, buffer.ToString(), 0);
                    int chk = i;
                    while (chk < point.Length && Bit.isspace(point[chk]))
                        chk++;
                    if (chk >= point.Length)
                    {
                        d.showstr_head = null;
                        d.showstr_point = null;
                    }
                    else
                        d.showstr_point = point.Substring(i);
                    return;
                }
                else
                {
                    buffer.Append(scan);
                    i++;
                }
            }
        }

        public static bool check_reconnect(DescriptorData d, string name, bool fConn)
        {
            for (var ch = Game.char_list; ch != null; ch = ch.next)
            {
                if (!Bit.IS_NPC(ch)
                    && (!fConn || ch.desc == null)
                    && !RomString.str_cmp(d.character.name, ch.name))
                {
                    if (fConn == false)
                    {
                        d.character.pcdata.pwd = ch.pcdata.pwd;
                    }
                    else
                    {
                        Save.free_char(d.character);
                        d.character = ch;
                        ch.desc = d;
                        ch.timer = 0;
                        send_to_char(
                            "Reconnecting. Type replay to see missed tells.\n\r",
                            ch);
                        act("$n has reconnected.", ch, null, null, TO_ROOM);

                        Db.log_f("%s@%s reconnected.", ch.name, d.host);
                        wiznet("$N groks the fullness of $S link.",
                            ch, null, WIZ_LINKS, 0, 0);
                        d.connected = CON_PLAYING;
                        if (ch.pcdata.in_progress != null)
                            send_to_char("You have a note in progress. Type NWRITE to continue it.\n\r", ch);
                    }
                    return true;
                }
            }

            return false;
        }

        public static bool check_playing(DescriptorData d, string name)
        {
            for (var dold = Game.descriptor_list; dold != null; dold = dold.next)
            {
                if (dold != d
                    && dold.character != null
                    && dold.connected != CON_GET_NAME
                    && dold.connected != CON_GET_OLD_PASSWORD
                    && !RomString.str_cmp(name, dold.original != null
                        ? dold.original.name : dold.character.name))
                {
                    write_to_buffer(d, "That character is already playing.\n\r", 0);
                    write_to_buffer(d, "Do you wish to connect anyway (Y/N)?", 0);
                    d.connected = CON_BREAK_CONNECT;
                    return true;
                }
            }

            return false;
        }

        public static bool check_parse_name(string name)
        {
            name ??= "";
            if (Handler.is_exact_name(name,
                    "all auto immortal self someone something the you loner none"))
            {
                return false;
            }

            for (int clan = 0; clan < MAX_CLAN; clan++)
            {
                var cname = Tables.clan_table[clan].name;
                if (string.IsNullOrEmpty(cname))
                    continue;
                /* comm.c: LOWER(name[0]) on empty is LOWER('\0') and never matches */
                if (name.Length != 0
                    && Bit.LOWER(name[0]) == Bit.LOWER(cname[0])
                    && !RomString.str_cmp(name, cname))
                    return false;
            }

            if (RomString.str_cmp(RomString.capitalize(name), "Alander")
                && (!RomString.str_prefix("Alan", name)
                    || !RomString.str_suffix("Alander", name)))
                return false;

            if (name.Length < 2)
                return false;

            if (name.Length > 12)
                return false;

            {
                bool fIll = true, adjcaps = false, cleancaps = false;
                int total_caps = 0;

                foreach (char pc in name)
                {
                    if (!((pc >= 'A' && pc <= 'Z') || (pc >= 'a' && pc <= 'z')))
                        return false;

                    if (pc >= 'A' && pc <= 'Z')
                    {
                        if (adjcaps)
                            cleancaps = true;
                        total_caps++;
                        adjcaps = true;
                    }
                    else
                        adjcaps = false;

                    if (Bit.LOWER(pc) != 'i' && Bit.LOWER(pc) != 'l')
                        fIll = false;
                }

                if (fIll)
                    return false;

                if (cleancaps
                    || (total_caps > name.Length / 2
                        && name.Length < 3)) return false;
            }

            for (int iHash = 0; iHash < MAX_KEY_HASH; iHash++)
            {
                for (var pMobIndex = Game.mob_index_hash[iHash];
                     pMobIndex != null; pMobIndex = pMobIndex.next)
                {
                    if (Handler.is_name(name, pMobIndex.player_name))
                        return false;
                }
            }

            if (Game.descriptor_list != null)
            {
                int count = 0;
                for (var dd = Game.descriptor_list; dd != null; )
                {
                    var dnext = dd.next;
                    if (dd.connected != CON_PLAYING && dd.character != null
                        && !string.IsNullOrEmpty(dd.character.name)
                        && !RomString.str_cmp(dd.character.name, name))
                    {
                        count++;
                        close_socket(dd);
                    }
                    dd = dnext;
                }
                if (count != 0)
                {
                    Game.log_buf = RomString.sprintf("Double newbie alert (%s)", name);
                    wiznet(Game.log_buf, null, null, WIZ_LOGINS, 0, 0);
                    return false;
                }
            }

            return true;
        }

        public static void stop_idling(CharData ch)
        {
            if (ch == null
                || ch.desc == null
                || ch.desc.connected != CON_PLAYING
                || ch.was_in_room == null
                || ch.in_room != Handler.get_room_index(ROOM_VNUM_LIMBO)) return;

            ch.timer = 0;
            Handler.char_from_room(ch);
            Handler.char_to_room(ch, ch.was_in_room);
            ch.was_in_room = null;
            act("$n has returned from the void.", ch, null, null, TO_ROOM);
        }

        public static void wiznet(string str, CharData ch, ObjData obj,
            long flag, long flag_skip, int min_level)
        {
            for (var d = Game.descriptor_list; d != null; d = d.next)
            {
                if (d.connected == CON_PLAYING && Bit.IS_IMMORTAL(d.character)
                    && Bit.IS_SET(d.character.wiznet, WIZ_ON)
                    && (flag == 0 || Bit.IS_SET(d.character.wiznet, flag))
                    && (flag_skip == 0 || !Bit.IS_SET(d.character.wiznet, flag_skip))
                    && Handler.get_trust(d.character) >= min_level && d.character != ch)
                {
                    if (Bit.IS_SET(d.character.wiznet, WIZ_PREFIX))
                        send_to_char("{Z--> ", d.character);
                    else
                        send_to_char("{Z", d.character);
                    act_new(str, d.character, obj, ch, TO_CHAR, POS_DEAD);
                    send_to_char("{x", d.character);
                }
            }
        }

        static string he_she(CharData ch)
        {
            if (ch == null) return "";
            int sex = Bit.URANGE(0, ch.sex, 2);
            return sex == SEX_MALE ? "he" : sex == SEX_FEMALE ? "she" : "it";
        }

        static string him_her(CharData ch)
        {
            if (ch == null) return "";
            int sex = Bit.URANGE(0, ch.sex, 2);
            return sex == SEX_MALE ? "him" : sex == SEX_FEMALE ? "her" : "it";
        }

        static string his_her(CharData ch)
        {
            if (ch == null) return "";
            int sex = Bit.URANGE(0, ch.sex, 2);
            return sex == SEX_MALE ? "his" : sex == SEX_FEMALE ? "her" : "its";
        }

        public static void act(string format, CharData ch, object arg1, object arg2, int type,
            string gmcpChan = null, string gmcpMsg = null)
            => act_new(format, ch, arg1, arg2, type, POS_RESTING, gmcpChan, gmcpMsg);

        public static void act_new(string format, CharData ch, object arg1, object arg2, int type, int min_pos,
            string gmcpChan = null, string gmcpMsg = null)
        {
            if (string.IsNullOrEmpty(format) || ch?.in_room == null) return;
            CharData vch = arg2 as CharData;
            ObjData obj1 = arg1 as ObjData;
            ObjData obj2 = arg2 as ObjData;
            CharData to = type == TO_VICT ? vch?.in_room?.people : ch.in_room.people;
            if (type == TO_VICT)
            {
                if (vch == null)
                {
                    Db.bug("Act: null vch with TO_VICT.", 0);
                    return;
                }
                if (vch.in_room == null)
                    return;
                to = vch.in_room.people;
            }
            for (; to != null; to = to.next_in_room)
            {
                if ((!Bit.IS_NPC(to) && to.desc == null)
                    || (Bit.IS_NPC(to) && !Bit.HAS_TRIGGER(to, TRIG_ACT))
                    || to.position < min_pos)
                    continue;
                if (type == TO_CHAR && to != ch) continue;
                if (type == TO_VICT && (to != vch || to == ch)) continue;
                if (type == TO_ROOM && to == ch) continue;
                if (type == TO_NOTVICT && (to == ch || to == vch)) continue;
                var sb = new StringBuilder();
                for (int i = 0; i < format.Length; i++)
                {
                    if (format[i] != '$')
                    {
                        sb.Append(format[i]);
                        continue;
                    }
                    i++;
                    char code = i < format.Length ? format[i] : '\0';
                    string iStr = " <@@@> ";
                    if (arg2 == null && code >= 'A' && code <= 'Z')
                    {
                        Db.bug("Act: missing arg2 for code %d.", code);
                        iStr = " <@@@> ";
                    }
                    else
                    {
                        switch (code)
                        {
                            default:
                                Db.bug("Act: bad code %d.", code);
                                iStr = " <@@@> ";
                                break;
                            case 't':
                                if (arg1 != null)
                                    iStr = arg1 as string ?? " <@@@> ";
                                else
                                    Db.bug("Act: bad code $t for 'arg1'", 0);
                                break;
                            case 'T':
                                if (arg2 != null)
                                    iStr = arg2 as string ?? " <@@@> ";
                                else
                                    Db.bug("Act: bad code $T for 'arg2'", 0);
                                break;
                            case 'n':
                                if (ch != null && to != null)
                                    iStr = Handler.PERS(ch, to);
                                else
                                    Db.bug("Act: bad code $n for 'ch' or 'to'", 0);
                                break;
                            case 'N':
                                if (vch != null && to != null)
                                    iStr = Handler.PERS(vch, to);
                                else
                                    Db.bug("Act: bad code $N for 'vch' or 'to'", 0);
                                break;
                            case 'e':
                                if (ch != null)
                                    iStr = he_she(ch);
                                else
                                    Db.bug("Act: bad code $e for 'ch'", 0);
                                break;
                            case 'E':
                                if (vch != null)
                                    iStr = he_she(vch);
                                else
                                    Db.bug("Act: bad code $E for 'vch'", 0);
                                break;
                            case 'm':
                                if (ch != null)
                                    iStr = him_her(ch);
                                else
                                    Db.bug("Act: bad code $m for 'ch'", 0);
                                break;
                            case 'M':
                                if (vch != null)
                                    iStr = him_her(vch);
                                else
                                    Db.bug("Act: bad code $M for 'vch'", 0);
                                break;
                            case 's':
                                if (ch != null)
                                    iStr = his_her(ch);
                                else
                                    Db.bug("Act: bad code $s for 'ch'", 0);
                                break;
                            case 'S':
                                if (vch != null)
                                    iStr = his_her(vch);
                                else
                                    Db.bug("Act: bad code $S for 'vch'", 0);
                                break;
                            case 'p':
                                if (to != null && obj1 != null)
                                    iStr = Handler.can_see_obj(to, obj1)
                                        ? obj1.short_descr : "something";
                                else
                                    Db.bug("Act: bad code $p for 'to' or 'obj1'", 0);
                                break;
                            case 'P':
                                if (to != null && obj2 != null)
                                    iStr = Handler.can_see_obj(to, obj2)
                                        ? obj2.short_descr : "something";
                                else
                                    Db.bug("Act: bad code $P for 'to' or 'obj2'", 0);
                                break;
                            case 'd':
                                if (arg2 is not string dArg || dArg.Length == 0)
                                    iStr = "door";
                                else
                                {
                                    RomString.one_argument(dArg, out string fname);
                                    iStr = fname;
                                }
                                break;
                        }
                    }
                    sb.Append(iStr);
                }
                sb.Append("\n\r");
                string buf = sb.ToString();
                if (buf.Length > 0 && buf[0] == 123)
                {
                    if (buf.Length > 2)
                    {
                        var chars = buf.ToCharArray();
                        chars[2] = Bit.UPPER(chars[2]);
                        buf = new string(chars);
                    }
                }
                else if (buf.Length > 0)
                {
                    var chars = buf.ToCharArray();
                    chars[0] = Bit.UPPER(chars[0]);
                    buf = new string(chars);
                }
                if (to.desc != null && to.desc.connected == CON_PLAYING)
                {
                    write_to_buffer(to.desc, Colour.colourconv(buf, to), 0);
                    if (gmcpChan != null)
                        Gmcp.Channel(to.desc, gmcpChan, ch?.name ?? "", gmcpMsg ?? "");
                }
                else if (Game.MOBtrigger)
                    MobProg.mp_act_trigger(buf, to, ch, arg1, arg2, (int)TRIG_ACT);
            }
        }

        public static void close_socket(DescriptorData dclose)
        {
            Gmcp.Goodbye(dclose);
            if (dclose.outbuf.Length > 0) process_output(dclose, false);
            if (dclose.snoop_by != null)
                write_to_buffer(dclose.snoop_by, "Your victim has left the game.\n\r", 0);
            for (var d = Game.descriptor_list; d != null; d = d.next)
            {
                if (d.snoop_by == dclose)
                    d.snoop_by = null;
            }
            if (dclose.character != null)
            {
                var ch = dclose.character;
                Game.log_buf = RomString.sprintf("Closing link to %s.", ch.name);
                Db.log_string(Game.log_buf);
                if ((dclose.connected == CON_PLAYING && !Game.merc_down)
                    || (dclose.connected >= CON_NOTE_TO
                        && dclose.connected <= CON_NOTE_FINISH))
                {
                    act("$n has lost $s link.", ch, null, null, TO_ROOM);
                    wiznet("Net death has claimed $N.", ch, null, WIZ_LINKS, 0, 0);
                    ch.desc = null;
                }
                else
                {
                    Save.free_char(dclose.original ?? dclose.character);
                }
            }
            try { dclose.socket?.Close(); } catch { }
            if (dclose == Game.descriptor_list)
                Game.descriptor_list = dclose.next;
            else
            {
                DescriptorData d;
                for (d = Game.descriptor_list; d != null && d.next != dclose; d = d.next) ;
                if (d != null)
                    d.next = dclose.next;
                else
                    Db.bug("Close_socket: dclose not found.", 0);
            }
            dclose.valid = false;
        }

        static int last_desc;

        static void init_descriptor(Socket client)
        {
            var dnew = Recycle.new_descriptor();
            dnew.socket = client;
            dnew.descriptor = ++last_desc;

            string ip = "(unknown)";
            if (client.RemoteEndPoint is IPEndPoint ep)
                ip = ep.Address.ToString();
            Game.log_buf = RomString.sprintf("Sock.sinaddr:  %s", ip);
            Db.log_string(Game.log_buf);

            string host = ip;
            try
            {
                var from = Dns.GetHostEntry(ip);
                if (from != null && from.HostName != null && from.HostName.Length != 0)
                    host = from.HostName;
            }
            catch
            {
            }
            dnew.host = host;

            dnew.connected = Game.mud_ansiprompt != 0 ? CON_ANSI : CON_GET_NAME;
            dnew.ansi = Game.mud_ansicolor != 0;

            if (Ban.check_ban(dnew.host, BAN_ALL))
            {
                try
                {
                    var bytes = Encoding.Latin1.GetBytes(
                        "Your site has been banned from this mud.\n\r");
                    client.Send(bytes);
                }
                catch { }
                try { client.Close(); } catch { }
                dnew.valid = false;
                return;
            }

            dnew.next = Game.descriptor_list;
            Game.descriptor_list = dnew;
            if (Game.mud_ansiprompt != 0)
                send_to_desc("Do you want ANSI? (Y/n) ", dnew);
            else
            {
                var greet = Game.help_greeting;
                if (greet.StartsWith(".")) greet = greet.Substring(1);
                send_to_desc(greet, dnew);
            }
            Gmcp.Offer(dnew);
        }

        static bool read_from_socket(DescriptorData d)
        {
            if (d.incomm.Length > 0)
                return true;
            if (d.inbuf.Length >= 4 * MAX_INPUT_LENGTH - 10)
            {
                Game.log_buf = RomString.sprintf("%s input overflow!", d.host);
                Db.log_string(Game.log_buf);
                write_to_descriptor(d.descriptor, "\n\r*** PUT A LID ON IT!!! ***\n\r", 0);
                return false;
            }
            int space = 4 * MAX_INPUT_LENGTH - 10 - d.inbuf.Length;
            if (space < 1)
                space = 1;
            var buf = new byte[space];
            int n;
            try { n = d.socket.Receive(buf); }
            catch { return false; }
            if (n <= 0) return false;
            var sb = new StringBuilder();
            foreach (var ev in d.Telnet.Push(buf, n))
            {
                if (ev.Kind == TelnetParser.EventKind.Text)
                {
                    foreach (byte b in ev.Data)
                    {
                        if (b == 0) continue;
                        sb.Append((char)b);
                    }
                }
                else if (ev.Kind == TelnetParser.EventKind.Negotiate)
                    Gmcp.OnNegotiate(d, ev.Command, ev.Option);
                else if (ev.Kind == TelnetParser.EventKind.Gmcp)
                    Gmcp.OnFrame(d, ev.Data);
            }
            d.inbuf += sb.ToString();
            return true;
        }

        static void read_from_buffer(DescriptorData d)
        {
            if (d.incomm.Length > 0) return;

            int i;
            for (i = 0; i < d.inbuf.Length && d.inbuf[i] != '\n' && d.inbuf[i] != '\r'; i++)
            { }
            if (i >= d.inbuf.Length)
                return;

            var incomm = new StringBuilder();
            int k = 0;
            for (i = 0; i < d.inbuf.Length && d.inbuf[i] != '\n' && d.inbuf[i] != '\r'; i++)
            {
                if (k >= MAX_INPUT_LENGTH - 2)
                {
                    write_to_descriptor(d.descriptor, "Line too long.\n\r", 0);
                    for (; i < d.inbuf.Length; i++)
                    {
                        if (d.inbuf[i] == '\n' || d.inbuf[i] == '\r')
                            break;
                    }
                    if (i >= d.inbuf.Length)
                        d.inbuf += "\n";
                    else
                        d.inbuf = d.inbuf.Substring(0, i) + "\n";
                    i = d.inbuf.Length - 1;
                    break;
                }
                if (d.inbuf[i] == '\b' && k > 0)
                {
                    if (incomm.Length > 0)
                        incomm.Length--;
                    k--;
                }
                else if (d.inbuf[i] < 128 && d.inbuf[i] >= 32 && d.inbuf[i] <= 126)
                {
                    incomm.Append(d.inbuf[i]);
                    k++;
                }
            }

            if (k == 0)
            {
                incomm.Append(' ');
                k++;
            }
            d.incomm = incomm.ToString();

            if (k > 1 || d.incomm[0] == '!')
            {
                if (d.incomm[0] != '!' && d.incomm != d.inlast)
                {
                    d.repeat = 0;
                }
                else
                {
                    if (++d.repeat >= 25 && d.character != null
                        && d.connected == CON_PLAYING)
                    {
                        Game.log_buf = RomString.sprintf("%s input spamming!", d.host);
                        Db.log_string(Game.log_buf);
                        wiznet("Spam spam spam $N spam spam spam spam spam!",
                            d.character, null, WIZ_SPAM, 0,
                            Handler.get_trust(d.character));
                        if (d.incomm[0] == '!')
                            wiznet(d.inlast, d.character, null, WIZ_SPAM, 0,
                                Handler.get_trust(d.character));
                        else
                            wiznet(d.incomm, d.character, null, WIZ_SPAM, 0,
                                Handler.get_trust(d.character));
                        d.repeat = 0;
                    }
                }
            }

            if (d.incomm.Length > 0 && d.incomm[0] == '!')
                d.incomm = d.inlast ?? "";
            else
                d.inlast = d.incomm;

            while (i < d.inbuf.Length && (d.inbuf[i] == '\n' || d.inbuf[i] == '\r'))
                i++;
            d.inbuf = i >= d.inbuf.Length ? "" : d.inbuf.Substring(i);
        }

        public static void game_loop()
        {
            if (control == null)
            {
                var ip = IPAddress.Parse(Game.mud_ipaddress);
                control = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                control.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                /*
                 * After a copyover where the listening socket could not be
                 * handed over, the old process may still be releasing the
                 * port; retry the bind briefly before giving up.
                 */
                bool bound = false;
                for (int i = 0; i < 40 && !bound; i++)
                {
                    try
                    {
                        control.Bind(new IPEndPoint(ip, Game.port));
                        bound = true;
                    }
                    catch (SocketException ex)
                        when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
                    {
                        Thread.Sleep(250);
                    }
                }
                if (!bound)
                {
                    Db.log_f("game_loop: cannot bind port %d, exiting.", Game.port);
                    Environment.Exit(1);
                    return;
                }
                control.Listen(20);
            }
            else
            {
                /* Listening socket adopted from the copyover parent. */
                Db.log_f("game_loop: using adopted listening socket.");
            }
            control.Blocking = false;

            var last = DateTime.UtcNow;
            while (!Game.merc_down)
            {
                try
                {
                    while (control.Poll(0, SelectMode.SelectRead))
                    {
                        var client = control.Accept();
                        client.Blocking = false;
                        init_descriptor(client);
                    }
                }
                catch (SocketException ex)
                {
                    Db.bug("game_loop: Accept SocketException: " + ex.Message, 0);
                }

                for (var d = Game.descriptor_list; d != null; )
                {
                    var d_next = d.next;
                    try
                    {
                        d.fcommand = false;
                        if (d.socket != null && d.socket.Poll(0, SelectMode.SelectRead))
                        {
                            if (d.character != null)
                                d.character.timer = 0;
                            if (!read_from_socket(d))
                            {
                                if (d.character != null && d.connected == CON_PLAYING)
                                    Save.save_char_obj(d.character);
                                close_socket(d);
                                d = d_next;
                                continue;
                            }
                        }
                        if (!d.valid) { d = d_next; continue; }
                        if (d.character != null && d.character.daze > 0) d.character.daze--;
                        if (d.character != null && d.character.wait > 0)
                        {
                            d.character.wait--;
                            d = d_next;
                            continue;
                        }
                        read_from_buffer(d);
                        if (d.incomm.Length > 0)
                        {
                            d.fcommand = true;
                            stop_idling(d.character);

                            /* OLC — comm.c */
                            if (d.showstr_point != null)
                                show_string(d, d.incomm);
                            else if (d.pString != null)
                                RomString.string_add(d.character, d.incomm);
                            else
                                switch (d.connected)
                                {
                                    case CON_PLAYING:
                                        if (!Olc.run_olc_editor(d))
                                            Alias.substitute_alias(d, d.incomm);
                                        break;
                                    default:
                                        Nanny.nanny(d, d.incomm);
                                        break;
                                }

                            d.incomm = "";
                        }
                    }
                    catch (Exception ex)
                    {
                        /* Isolate one bad descriptor so the whole mud keeps running. */
                        Db.bug("game_loop: exception on descriptor: " + ex, 0);
                        try
                        {
                            if (d.valid && d.character != null && d.connected == CON_PLAYING)
                                Save.save_char_obj(d.character);
                        }
                        catch (Exception saveEx)
                        {
                            Db.bug("game_loop: save after exception failed: " + saveEx, 0);
                        }
                        try
                        {
                            if (d.valid)
                                close_socket(d);
                        }
                        catch (Exception closeEx)
                        {
                            Db.bug("game_loop: close_socket after exception failed: " + closeEx, 0);
                        }
                    }
                    d = d_next;
                }

                try
                {
                    Imc.loop();
                }
                catch (Exception ex)
                {
                    Db.bug("game_loop: Imc.loop exception: " + ex, 0);
                }
                try
                {
                    Update.update_handler();
                }
                catch (Exception ex)
                {
                    Db.bug("game_loop: update_handler exception: " + ex, 0);
                }

                for (var d = Game.descriptor_list; d != null; )
                {
                    var d_next = d.next;
                    if (d.fcommand || d.outbuf.Length > 0)
                    {
                        if (!process_output(d, true))
                        {
                            if (d.character != null && d.connected == CON_PLAYING)
                                Save.save_char_obj(d.character);
                            close_socket(d);
                        }
                    }
                    d = d_next;
                }

                var now = DateTime.UtcNow;
                var target = last + TimeSpan.FromMilliseconds(1000.0 / PULSE_PER_SECOND);
                var stall = target - now;
                if (stall > TimeSpan.Zero) Thread.Sleep(stall);
                last = DateTime.UtcNow;
                Game.current_time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
        }

        static readonly string go_ahead_str =
            new string(new[] { (char)255, (char)249 });

        static bool process_output(DescriptorData d, bool fPrompt)
        {
            if (!Game.merc_down)
            {
                if (d.showstr_point != null)
                    write_to_buffer(d, "[Hit Return to continue]\n\r", 0);
                else if (fPrompt && d.pString != null && d.connected == CON_PLAYING)
                    write_to_buffer(d, "> ", 2);
                else if (fPrompt && d.connected == CON_PLAYING)
                {
                    var ch = d.character;

                    if (ch != null && (ch.fighting is CharData victim)
                        && Handler.can_see(ch, victim))
                    {
                        int percent;
                        if (victim.max_hit > 0)
                            percent = victim.hit * 100 / victim.max_hit;
                        else
                            percent = -1;

                        string wound;
                        if (percent >= 100)
                            wound = "is in excellent condition.";
                        else if (percent >= 90)
                            wound = "has a few scratches.";
                        else if (percent >= 75)
                            wound = "has some small wounds and bruises.";
                        else if (percent >= 50)
                            wound = "has quite a few wounds.";
                        else if (percent >= 30)
                            wound = "has some big nasty wounds and scratches.";
                        else if (percent >= 15)
                            wound = "looks pretty hurt.";
                        else if (percent >= 0)
                            wound = "is in awful condition.";
                        else
                            wound = "is bleeding to death.";

                        string buf = RomString.sprintf("%s %s \n\r",
                            Bit.IS_NPC(victim) ? victim.short_descr : victim.name,
                            wound);
                        if (buf.Length > 0)
                        {
                            var chars = buf.ToCharArray();
                            chars[0] = Bit.UPPER(chars[0]);
                            buf = new string(chars);
                        }
                        write_to_buffer(d, Colour.colourconv(buf, d.original ?? d.character), 0);
                    }

                    ch = d.original ?? d.character;
                    if (ch != null && !Bit.IS_SET(ch.comm, COMM_COMPACT))
                        write_to_buffer(d, "\n\r", 2);

                    if (ch != null && Bit.IS_SET(ch.comm, COMM_PROMPT))
                        bust_a_prompt(d.character);

                    if (ch != null && Bit.IS_SET(ch.comm, COMM_TELNET_GA))
                        write_to_buffer(d, go_ahead_str, 0);
                }
            }

            if (d.outbuf.Length == 0)
                return true;

            if (d.snoop_by != null)
            {
                if (d.character != null)
                    write_to_buffer(d.snoop_by, d.character.name, 0);
                write_to_buffer(d.snoop_by, "> ", 2);
                write_to_buffer(d.snoop_by, d.outbuf.ToString(), 0);
            }

            if (d.socket == null || !d.socket.Connected)
                return true;

            if (!write_to_descriptor(d.descriptor, d.outbuf.ToString(), 0))
            {
                d.outbuf.Clear();
                return false;
            }
            d.outbuf.Clear();
            return true;
        }

        static void bust_a_prompt(CharData ch)
        {
            if (ch == null)
                return;
            Gmcp.OnPrompt(ch);

            string str = ch.prompt;
            if (str == null || str.Length == 0)
            {
                send_to_char(RomString.sprintf("{p<%dhp %dm %dmv>{x %s",
                    ch.hit, ch.mana, ch.move, ch.prefix ?? ""), ch);
                return;
            }

            if (Bit.IS_SET(ch.comm, COMM_AFK))
            {
                send_to_char("{p<AFK>{x ", ch);
                return;
            }

            var buf = new StringBuilder();
            for (int p = 0; p < str.Length; p++)
            {
                if (str[p] != '%')
                {
                    buf.Append(str[p]);
                    continue;
                }
                p++;
                char code = p < str.Length ? str[p] : '\0';
                string iStr;
                switch (code)
                {
                    default:
                        iStr = " ";
                        break;
                    case 'e':
                    {
                        var doors = new StringBuilder();
                        bool found = false;
                        string[] dir_name = { "N", "E", "S", "W", "U", "D" };
                        if (ch.in_room != null)
                        {
                            for (int door = 0; door < 6; door++)
                            {
                                var pexit = ch.in_room.exit[door];
                                if (pexit != null
                                    && pexit.to_room != null
                                    && (Handler.can_see_room(ch, pexit.to_room)
                                        || (Bit.IS_AFFECTED(ch, AFF_INFRARED)
                                            && !Bit.IS_AFFECTED(ch, AFF_BLIND)))
                                    && !Bit.IS_SET(pexit.exit_info, EX_CLOSED))
                                {
                                    found = true;
                                    doors.Append(dir_name[door]);
                                }
                            }
                        }
                        if (!found)
                            doors.Append("none");
                        iStr = doors.ToString();
                        break;
                    }
                    case 'c':
                        iStr = "\n\r";
                        break;
                    case 'h':
                        iStr = ch.hit.ToString();
                        break;
                    case 'H':
                        iStr = ch.max_hit.ToString();
                        break;
                    case 'm':
                        iStr = ch.mana.ToString();
                        break;
                    case 'M':
                        iStr = ch.max_mana.ToString();
                        break;
                    case 'v':
                        iStr = ch.move.ToString();
                        break;
                    case 'V':
                        iStr = ch.max_move.ToString();
                        break;
                    case 'x':
                        iStr = ch.exp.ToString();
                        break;
                    case 'X':
                        iStr = Bit.IS_NPC(ch) ? "0" :
                            ((ch.level + 1) * Handler.exp_per_level(ch, ch.pcdata.points) - ch.exp).ToString();
                        break;
                    case 'g':
                        iStr = ch.gold.ToString();
                        break;
                    case 's':
                        iStr = ch.silver.ToString();
                        break;
                    case 'a':
                        if (ch.level > 9)
                            iStr = ch.alignment.ToString();
                        else
                            iStr = Bit.IS_GOOD(ch) ? "good" : Bit.IS_EVIL(ch) ? "evil" : "neutral";
                        break;
                    case 'r':
                        if (ch.in_room != null)
                            iStr = ((!Bit.IS_NPC(ch) && Bit.IS_SET(ch.act, PLR_HOLYLIGHT))
                                    || (!Bit.IS_AFFECTED(ch, AFF_BLIND)
                                        && !Handler.room_is_dark(ch.in_room)))
                                ? ch.in_room.name : "darkness";
                        else
                            iStr = " ";
                        break;
                    case 'R':
                        if (Bit.IS_IMMORTAL(ch) && ch.in_room != null)
                            iStr = ch.in_room.vnum.ToString();
                        else
                            iStr = " ";
                        break;
                    case 'z':
                        if (Bit.IS_IMMORTAL(ch) && ch.in_room != null)
                            iStr = ch.in_room.area.name;
                        else
                            iStr = " ";
                        break;
                    case '%':
                        iStr = "%";
                        break;
                    case 'o':
                        iStr = ch.desc != null ? Olc.olc_ed_name(ch) : " ";
                        break;
                    case 'O':
                        iStr = ch.desc != null ? Olc.olc_ed_vnum(ch) : " ";
                        break;
                }
                buf.Append(iStr);
            }
            send_to_char("{p", ch);
            if (ch.desc != null)
                write_to_buffer(ch.desc, Colour.colourconv(buf.ToString(), ch), 0);
            send_to_char("{x", ch);

            if (ch.prefix != null && ch.prefix.Length != 0 && ch.desc != null)
                write_to_buffer(ch.desc, ch.prefix, 0);
        }
    }
}
