using static Rom24.Merc;

namespace Rom24
{
    public static partial class Olc
    {
        static readonly (string name, OlcFun olc_fun)[] hedit_table =
        {
            ("keyword", hedit_keyword),
            ("text", hedit_text),
            ("new", hedit_new),
            ("level", hedit_level),
            ("commands", show_commands),
            ("delete", hedit_delete),
            ("list", hedit_list),
            ("show", hedit_show),
            ("?", show_help),
            (null, null)
        };

        static HelpArea get_help_area(HelpData help)
        {
            for (var temp = Game.had_list; temp != null; temp = temp.next)
                for (var thelp = temp.first; thelp != null; thelp = thelp.next_area)
                    if (thelp == help)
                        return temp;

            return null;
        }

        static HelpData help_lookup(string keyword)
        {
            keyword ??= "";
            string argall = "";

            while (keyword.Length != 0)
            {
                keyword = RomString.one_argument(keyword, out string temp);
                if (argall.Length != 0)
                    argall += " ";
                argall += temp;
            }

            for (var pHelp = Game.help_first; pHelp != null; pHelp = pHelp.next)
                if (Handler.is_name(argall, pHelp.keyword))
                    return pHelp;

            return null;
        }

        static HelpArea had_lookup(string arg)
        {
            for (var temp = Game.had_list; temp != null; temp = temp.next)
                if (!RomString.str_cmp(arg, temp.filename))
                    return temp;

            return null;
        }

        public static void hedit(CharData ch, string argument)
        {
            argument = RomString.smash_tilde(argument);
            string arg = argument;
            argument = RomString.one_argument(argument, out string command);

            var pHelp = (HelpData)ch.desc.pEdit;

            var had = get_help_area(pHelp);

            if (had == null)
            {
                Db.bug(RomString.sprintf("hedit : had for help %s NULL", pHelp.keyword), 0);
                edit_done(ch);
                return;
            }

            if (ch.pcdata.security < 9)
            {
                Comm.send_to_char("HEdit: Insufficient security to edit helps.\n\r",
                    ch);
                edit_done(ch);
                return;
            }

            if (command.Length == 0)
            {
                hedit_show(ch, argument);
                return;
            }

            if (!RomString.str_cmp(command, "done"))
            {
                edit_done(ch);
                return;
            }

            for (int cmd = 0; hedit_table[cmd].name != null; cmd++)
            {
                if (!RomString.str_prefix(command, hedit_table[cmd].name))
                {
                    if (hedit_table[cmd].olc_fun(ch, argument))
                        had.changed = true;
                    return;
                }
            }

            Interp.interpret(ch, arg);
        }

        static bool hedit_show(CharData ch, string argument)
        {
            var help = (HelpData)ch.desc.pEdit;

            Comm.send_to_char(RomString.sprintf("Keyword : [%s]\n\r"
                + "Level   : [%d]\n\r"
                + "Text    :\n\r"
                + "%s-END-\n\r", help.keyword, help.level, help.text), ch);

            return false;
        }

        static bool hedit_level(CharData ch, string argument)
        {
            var help = (HelpData)ch.desc.pEdit;

            if (Bit.IS_NULLSTR(argument) || !Interp.is_number(argument))
            {
                Comm.send_to_char("Syntax: level [-1..MAX_LEVEL]\n\r", ch);
                return false;
            }

            int lev = Interp.atoi(argument);

            if (lev < -1 || lev > MAX_LEVEL)
            {
                Comm.printf_to_char(ch, "HEdit : levels are between -1 and %d inclusive.\n\r",
                    MAX_LEVEL);
                return false;
            }

            help.level = lev;
            Comm.send_to_char("Ok.\n\r", ch);
            return true;
        }

        static bool hedit_keyword(CharData ch, string argument)
        {
            var help = (HelpData)ch.desc.pEdit;

            if (Bit.IS_NULLSTR(argument))
            {
                Comm.send_to_char("Syntax: keyword [keywords]\n\r", ch);
                return false;
            }

            help.keyword = argument;

            Comm.send_to_char("Ok.\n\r", ch);
            return true;
        }

        static bool hedit_new(CharData ch, string argument)
        {
            if (Bit.IS_NULLSTR(argument))
            {
                Comm.send_to_char("Syntax: new [name]\n\r", ch);
                Comm.send_to_char("         new [area] [name]\n\r", ch);
                return false;
            }

            string fullarg = argument;
            argument = RomString.one_argument(argument, out string arg);

            var had = had_lookup(arg);
            if (had == null)
            {
                had = ch.in_room.area.helps;
                argument = fullarg;
            }

            if (help_lookup(argument) != null)
            {
                Comm.send_to_char("HEdit : help exists.\n\r", ch);
                return false;
            }

            if (had == null)
            {
                had = Recycle.new_had();
                had.filename = ch.in_room.area.file_name;
                had.area = ch.in_room.area;
                had.first = null;
                had.last = null;
                had.changed = true;
                had.next = Game.had_list;
                Game.had_list = had;
                ch.in_room.area.helps = had;
                Bit.SET_BIT(ref ch.in_room.area.area_flags, AREA_CHANGED);
            }

            var help = Recycle.new_help();
            help.level = 0;
            help.keyword = argument;
            help.text = "";

            if (Game.help_last != null)
                Game.help_last.next = help;

            if (Game.help_first == null)
                Game.help_first = help;

            Game.help_last = help;
            help.next = null;

            if (had.first == null)
                had.first = help;
            if (had.last == null)
                had.last = help;

            had.last.next_area = help;
            had.last = help;
            help.next_area = null;

            ch.desc.pEdit = help;
            ch.desc.editor = ED_HELP;

            Comm.send_to_char("Ok.\n\r", ch);
            return false;
        }

        static bool hedit_text(CharData ch, string argument)
        {
            var help = (HelpData)ch.desc.pEdit;

            if (!Bit.IS_NULLSTR(argument))
            {
                Comm.send_to_char("Syntax: text\n\r", ch);
                return false;
            }

            RomString.string_append(ch, new StringPtr(() => help.text, v => help.text = v));

            return true;
        }

        static bool hedit_delete(CharData ch, string argument)
        {
            var pHelp = (HelpData)ch.desc.pEdit;
            bool found = false;

            for (var d = Game.descriptor_list; d != null; d = d.next)
                if (d.editor == ED_HELP && pHelp == (HelpData)d.pEdit)
                    edit_done(d.character);

            if (Game.help_first == pHelp)
                Game.help_first = Game.help_first.next;
            else
            {
                HelpData temp;
                for (temp = Game.help_first; temp != null; temp = temp.next)
                    if (temp.next == pHelp)
                        break;

                if (temp == null)
                {
                    Db.bug(RomString.sprintf("hedit_delete : help %s not found in help_first",
                        pHelp.keyword), 0);
                    return false;
                }

                temp.next = pHelp.next;
            }

            for (var had = Game.had_list; had != null; had = had.next)
                if (pHelp == had.first)
                {
                    found = true;
                    had.first = had.first.next_area;
                }
                else
                {
                    HelpData temp;
                    for (temp = had.first; temp != null; temp = temp.next_area)
                        if (temp.next_area == pHelp)
                            break;

                    if (temp != null)
                    {
                        temp.next_area = pHelp.next_area;
                        found = true;
                        break;
                    }
                }

            if (!found)
            {
                Db.bug(RomString.sprintf("hedit_delete : help %s not found in had_list",
                    pHelp.keyword), 0);
                return false;
            }

            Comm.send_to_char("Ok.\n\r", ch);
            return true;
        }

        static bool hedit_list(CharData ch, string argument)
        {
            int cnt = 0;
            var buffer = new System.Text.StringBuilder();

            if (!RomString.str_cmp(argument, "all"))
            {
                for (var pHelp = Game.help_first; pHelp != null; pHelp = pHelp.next)
                {
                    buffer.Append(RomString.sprintf("%3d. %-14.14s%s", cnt, pHelp.keyword,
                        cnt % 4 == 3 ? "\n\r" : " "));
                    cnt++;
                }

                if (cnt % 4 != 0)
                    buffer.Append("\n\r");

                Comm.page_to_char(buffer.ToString(), ch);
                return false;
            }

            if (!RomString.str_cmp(argument, "area"))
            {
                if (ch.in_room.area.helps == null)
                {
                    Comm.send_to_char("No helps in this area.\n\r", ch);
                    return false;
                }

                for (var pHelp = ch.in_room.area.helps.first; pHelp != null;
                     pHelp = pHelp.next_area)
                {
                    buffer.Append(RomString.sprintf("%3d. %-14.14s%s", cnt, pHelp.keyword,
                        cnt % 4 == 3 ? "\n\r" : " "));
                    cnt++;
                }

                if (cnt % 4 != 0)
                    buffer.Append("\n\r");

                Comm.page_to_char(buffer.ToString(), ch);
                return false;
            }

            if (Bit.IS_NULLSTR(argument))
            {
                Comm.send_to_char("Syntax: list all\n\r", ch);
                Comm.send_to_char("        list area\n\r", ch);
                return false;
            }

            return false;
        }
    }
}
