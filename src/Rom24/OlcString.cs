using static Rom24.Merc;

namespace Rom24
{
    public static partial class RomString
    {
        public static void string_edit(CharData ch, StringPtr pString)
        {
            Comm.send_to_char("-========- Entering EDIT Mode -=========-\n\r", ch);
            Comm.send_to_char("    Type .h on a new line for help\n\r", ch);
            Comm.send_to_char(" Terminate with a ~ or @ on a blank line.\n\r", ch);
            Comm.send_to_char("-=======================================-\n\r", ch);

            pString.Value = "";
            ch.desc.pString = pString;
        }

        public static void string_append(CharData ch, StringPtr pString)
        {
            Comm.send_to_char("-=======- Entering APPEND Mode -========-\n\r", ch);
            Comm.send_to_char("    Type .h on a new line for help\n\r", ch);
            Comm.send_to_char(" Terminate with a ~ or @ on a blank line.\n\r", ch);
            Comm.send_to_char("-=======================================-\n\r", ch);

            Comm.send_to_char(numlines(pString.Value), ch);
            ch.desc.pString = pString;
        }

        public static string string_replace(string orig, string old, string @new)
        {
            orig ??= "";
            old ??= "";
            @new ??= "";
            string xbuf = orig;
            int i = orig.IndexOf(old, StringComparison.Ordinal);
            if (i >= 0)
                xbuf = orig.Substring(0, i) + @new + orig.Substring(i + old.Length);
            return xbuf;
        }

        public static void string_add(CharData ch, string argument)
        {
            argument ??= "";
            argument = smash_tilde(argument);

            if (argument.Length != 0 && argument[0] == '.')
            {
                argument = one_argument(argument, out string arg1);
                argument = first_arg(argument, out string arg2, false);
                string tmparg3 = argument;
                argument = first_arg(argument, out string arg3, false);

                if (!str_cmp(arg1, ".c"))
                {
                    Comm.send_to_char("String cleared.\n\r", ch);
                    ch.desc.pString.Value = "";
                    return;
                }

                if (!str_cmp(arg1, ".s"))
                {
                    Comm.send_to_char("String so far:\n\r", ch);
                    Comm.send_to_char(numlines(ch.desc.pString.Value), ch);
                    return;
                }

                if (!str_cmp(arg1, ".r"))
                {
                    if (arg2.Length == 0)
                    {
                        Comm.send_to_char("usage:  .r \"old string\" \"new string\"\n\r",
                            ch);
                        return;
                    }

                    ch.desc.pString.Value =
                        string_replace(ch.desc.pString.Value, arg2, arg3);
                    Comm.send_to_char(sprintf("'%s' replaced with '%s'.\n\r", arg2, arg3), ch);
                    return;
                }

                if (!str_cmp(arg1, ".f"))
                {
                    ch.desc.pString.Value = format_string(ch.desc.pString.Value);
                    Comm.send_to_char("String formatted.\n\r", ch);
                    return;
                }

                if (!str_cmp(arg1, ".ld"))
                {
                    ch.desc.pString.Value =
                        string_linedel(ch.desc.pString.Value, Interp.atoi(arg2));
                    Comm.send_to_char("Line deleted.\n\r", ch);
                    return;
                }

                if (!str_cmp(arg1, ".li"))
                {
                    ch.desc.pString.Value =
                        string_lineadd(ch.desc.pString.Value, tmparg3, Interp.atoi(arg2));
                    Comm.send_to_char("Line inserted.\n\r", ch);
                    return;
                }

                if (!str_cmp(arg1, ".lr"))
                {
                    ch.desc.pString.Value =
                        string_linedel(ch.desc.pString.Value, Interp.atoi(arg2));
                    ch.desc.pString.Value =
                        string_lineadd(ch.desc.pString.Value, tmparg3, Interp.atoi(arg2));
                    Comm.send_to_char("Line replaced.\n\r", ch);
                    return;
                }

                if (!str_cmp(arg1, ".h"))
                {
                    Comm.send_to_char("Sedit help (commands on blank line):   \n\r", ch);
                    Comm.send_to_char(".r 'old' 'new'   - replace a substring \n\r", ch);
                    Comm.send_to_char("                   (requires '', \"\") \n\r", ch);
                    Comm.send_to_char(".h               - get help (this info)\n\r", ch);
                    Comm.send_to_char(".s               - show string so far  \n\r", ch);
                    Comm.send_to_char(".f               - (word wrap) string  \n\r", ch);
                    Comm.send_to_char(".c               - clear string so far \n\r", ch);
                    Comm.send_to_char(".ld <num>        - delete line number <num>\n\r",
                        ch);
                    Comm.send_to_char(".li <num> <str>  - insert <str> at line <num>\n\r",
                        ch);
                    Comm.send_to_char
                        (".lr <num> <str>  - replace line <num> with <str>\n\r",
                        ch);
                    Comm.send_to_char("@                - end string          \n\r", ch);
                    return;
                }

                Comm.send_to_char("SEdit:  Invalid dot command.\n\r", ch);
                return;
            }

            if (argument.Length != 0 && (argument[0] == '~' || argument[0] == '@'))
            {
                if (ch.desc.editor == Olc.ED_MPCODE)
                {
                    var mpc = (MprogCode)ch.desc.pEdit;

                    if (mpc != null)
                        for (int hash = 0; hash < MAX_KEY_HASH; hash++)
                            for (var mob = Game.mob_index_hash[hash]; mob != null; mob = mob.next)
                                for (var mpl = mob.mprogs; mpl != null; mpl = mpl.next)
                                    if (mpl.vnum == mpc.vnum)
                                    {
                                        Comm.send_to_char(sprintf("Editting mob %d.\n\r",
                                            mob.vnum), ch);
                                        mpl.code = mpc.code;
                                    }
                }

                ch.desc.pString = null;
                return;
            }

            string cur = ch.desc.pString.Value ?? "";
            if (cur.Length + argument.Length >= (MAX_STRING_LENGTH - 4))
            {
                Comm.send_to_char("String too long, last line skipped.\n\r", ch);
                ch.desc.pString = null;
                return;
            }

            argument = smash_tilde(argument);
            ch.desc.pString.Value = cur + argument + "\n\r";
        }

        public static string format_string(string oldstring)
        {
            oldstring ??= "";
            char[] xbuf = new char[MAX_STRING_LENGTH];
            char[] xbuf2 = new char[MAX_STRING_LENGTH];
            int i = 0;
            bool cap = true;

            xbuf[0] = xbuf2[0] = '\0';
            i = 0;

            for (int rp = 0; rp < oldstring.Length; rp++)
            {
                char rdesc = oldstring[rp];
                if (rdesc == '\n')
                {
                    if (i == 0 || xbuf[i - 1] != ' ')
                    {
                        xbuf[i] = ' ';
                        i++;
                    }
                }
                else if (rdesc == '\r')
                    ;
                else if (rdesc == ' ')
                {
                    if (i == 0 || xbuf[i - 1] != ' ')
                    {
                        xbuf[i] = ' ';
                        i++;
                    }
                }
                else if (rdesc == ')')
                {
                    if (i >= 3 && xbuf[i - 1] == ' ' && xbuf[i - 2] == ' ' &&
                        (xbuf[i - 3] == '.' || xbuf[i - 3] == '?'
                         || xbuf[i - 3] == '!'))
                    {
                        xbuf[i - 2] = rdesc;
                        xbuf[i - 1] = ' ';
                        xbuf[i] = ' ';
                        i++;
                    }
                    else
                    {
                        xbuf[i] = rdesc;
                        i++;
                    }
                }
                else if (rdesc == '.' || rdesc == '?' || rdesc == '!')
                {
                    if (i >= 3 && xbuf[i - 1] == ' ' && xbuf[i - 2] == ' ' &&
                        (xbuf[i - 3] == '.' || xbuf[i - 3] == '?'
                         || xbuf[i - 3] == '!'))
                    {
                        xbuf[i - 2] = rdesc;
                        if (rp + 1 >= oldstring.Length || oldstring[rp + 1] != '"')
                        {
                            xbuf[i - 1] = ' ';
                            xbuf[i] = ' ';
                            i++;
                        }
                        else
                        {
                            xbuf[i - 1] = '"';
                            xbuf[i] = ' ';
                            xbuf[i + 1] = ' ';
                            i += 2;
                            rp++;
                        }
                    }
                    else
                    {
                        xbuf[i] = rdesc;
                        if (rp + 1 >= oldstring.Length || oldstring[rp + 1] != '"')
                        {
                            xbuf[i + 1] = ' ';
                            xbuf[i + 2] = ' ';
                            i += 3;
                        }
                        else
                        {
                            xbuf[i + 1] = '"';
                            xbuf[i + 2] = ' ';
                            xbuf[i + 3] = ' ';
                            i += 4;
                            rp++;
                        }
                    }
                    cap = true;
                }
                else
                {
                    xbuf[i] = rdesc;
                    if (cap)
                    {
                        cap = false;
                        xbuf[i] = Bit.UPPER(xbuf[i]);
                    }
                    i++;
                }
                if (i >= MAX_STRING_LENGTH - 5)
                    break;
            }
            xbuf[i] = '\0';
            for (int k = 0; k <= i; k++)
                xbuf2[k] = xbuf[k];

            int rdescPos = 0;
            xbuf[0] = '\0';
            int xlen = 0;

            for (;;)
            {
                for (i = 0; i < 77; i++)
                {
                    if (xbuf2[rdescPos + i] == '\0')
                        break;
                }
                if (i < 77)
                    break;
                for (i = (xlen != 0 ? 76 : 73); i != 0; i--)
                {
                    if (xbuf2[rdescPos + i] == ' ')
                        break;
                }
                if (i != 0)
                {
                    xbuf2[rdescPos + i] = '\0';
                    int p = rdescPos;
                    while (xbuf2[p] != '\0')
                        xbuf[xlen++] = xbuf2[p++];
                    xbuf[xlen++] = '\n';
                    xbuf[xlen++] = '\r';
                    xbuf[xlen] = '\0';
                    rdescPos += i + 1;
                    while (xbuf2[rdescPos] == ' ')
                        rdescPos++;
                }
                else
                {
                    Db.bug("No spaces", 0);
                    xbuf2[rdescPos + 75] = '\0';
                    int p = rdescPos;
                    while (xbuf2[p] != '\0')
                        xbuf[xlen++] = xbuf2[p++];
                    xbuf[xlen++] = '-';
                    xbuf[xlen++] = '\n';
                    xbuf[xlen++] = '\r';
                    xbuf[xlen] = '\0';
                    rdescPos += 76;
                }
            }
            while (xbuf2[rdescPos + i] != '\0' && (xbuf2[rdescPos + i] == ' ' ||
                            xbuf2[rdescPos + i] == '\n' || xbuf2[rdescPos + i] == '\r'))
                i--;
            xbuf2[rdescPos + i + 1] = '\0';
            {
                int p = rdescPos;
                while (xbuf2[p] != '\0')
                    xbuf[xlen++] = xbuf2[p++];
                xbuf[xlen] = '\0';
            }
            if (xlen < 2 || xbuf[xlen - 2] != '\n')
            {
                xbuf[xlen++] = '\n';
                xbuf[xlen++] = '\r';
                xbuf[xlen] = '\0';
            }

            return new string(xbuf, 0, xlen);
        }

        public static string first_arg(string argument, out string arg_first, bool fCase)
        {
            argument ??= "";
            int i = 0;

            while (i < argument.Length && argument[i] == ' ')
                i++;

            char cEnd = ' ';
            if (i < argument.Length && (argument[i] == '\'' || argument[i] == '"'
                || argument[i] == '%' || argument[i] == '('))
            {
                if (argument[i] == '(')
                {
                    cEnd = ')';
                    i++;
                }
                else
                    cEnd = argument[i++];
            }

            var sb = new System.Text.StringBuilder();
            while (i < argument.Length)
            {
                if (argument[i] == cEnd)
                {
                    i++;
                    break;
                }
                sb.Append(fCase ? Bit.LOWER(argument[i]) : argument[i]);
                i++;
            }
            arg_first = sb.ToString();

            while (i < argument.Length && argument[i] == ' ')
                i++;

            return i >= argument.Length ? "" : argument.Substring(i);
        }

        public static string string_unpad(string argument)
        {
            argument ??= "";
            int s = 0;
            while (s < argument.Length && argument[s] == ' ')
                s++;
            string buf = argument.Substring(s);
            if (buf.Length != 0)
            {
                int e = buf.Length - 1;
                while (e >= 0 && buf[e] == ' ')
                    e--;
                buf = buf.Substring(0, e + 1);
            }
            return buf;
        }

        public static string string_proper(string argument)
        {
            if (string.IsNullOrEmpty(argument))
                return argument ?? "";
            char[] s = argument.ToCharArray();
            int i = 0;
            while (i < s.Length)
            {
                if (s[i] != ' ')
                {
                    s[i] = Bit.UPPER(s[i]);
                    while (i < s.Length && s[i] != ' ')
                        i++;
                }
                else
                    i++;
            }
            return new string(s);
        }

        public static string string_linedel(string str, int line)
        {
            str ??= "";
            var buf = new System.Text.StringBuilder();
            int cnt = 1;

            for (int i = 0; i < str.Length; i++)
            {
                if (cnt != line)
                    buf.Append(str[i]);

                if (str[i] == '\n')
                {
                    if (i + 1 < str.Length && str[i + 1] == '\r')
                    {
                        if (cnt != line)
                            buf.Append(str[++i]);
                        else
                            ++i;
                    }

                    cnt++;
                }
            }

            return buf.ToString();
        }

        public static string string_lineadd(string str, string newstr, int line)
        {
            str ??= "";
            newstr ??= "";
            var buf = new System.Text.StringBuilder();
            int cnt = 1;
            bool done = false;
            int i = 0;

            for (; (i < str.Length) || (!done && cnt == line); i++)
            {
                if (cnt == line && !done)
                {
                    buf.Append(newstr);
                    buf.Append("\n\r");
                    cnt++;
                    done = true;
                }

                char c = i < str.Length ? str[i] : '\0';
                if (c != '\0')
                    buf.Append(c);

                if (done && c == '\0')
                    break;

                if (c == '\n')
                {
                    if (i + 1 < str.Length && str[i + 1] == '\r')
                    {
                        i++;
                        buf.Append('\r');
                    }

                    cnt++;
                }
            }

            return buf.ToString();
        }

        static string merc_getline(string str, ref int pos)
        {
            var buf = new System.Text.StringBuilder();
            bool found = false;

            while (pos < str.Length)
            {
                if (str[pos] == '\n')
                {
                    found = true;
                    break;
                }

                buf.Append(str[pos++]);
            }

            if (found)
            {
                if (pos + 1 < str.Length && str[pos + 1] == '\r')
                    pos += 2;
                else
                    pos += 1;
            }

            return buf.ToString();
        }

        public static string numlines(string str)
        {
            str ??= "";
            int cnt = 1;
            var buf = new System.Text.StringBuilder();
            int pos = 0;

            while (pos < str.Length)
            {
                string tmpb = merc_getline(str, ref pos);
                buf.Append(sprintf("%2d. %s\n\r", cnt++, tmpb));
            }

            return buf.ToString();
        }
    }
}
