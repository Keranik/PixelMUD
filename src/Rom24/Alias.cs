using static Rom24.Merc;

namespace Rom24
{
    public static class Alias
    {
        public static void substitute_alias(DescriptorData d, string argument)
        {
            argument ??= "";
            var ch = d.original != null ? d.original : d.character;

            if (ch.prefix.Length != 0 && RomString.str_prefix("prefix", argument))
            {
                if (ch.prefix.Length + argument.Length > MAX_INPUT_LENGTH - 2)
                    Comm.send_to_char("Line to long, prefix not processed.\r\n", ch);
                else
                    argument = ch.prefix + " " + argument;
            }

            if (Bit.IS_NPC(ch) || ch.pcdata.alias[0] == null
                || !RomString.str_prefix("alias", argument) || !RomString.str_prefix("una", argument)
                || !RomString.str_prefix("prefix", argument))
            {
                Interp.interpret(d.character, argument);
                return;
            }

            string buf = argument;

            for (int alias = 0; alias < MAX_ALIAS; alias++)
            {
                if (ch.pcdata.alias[alias] == null)
                    break;

                if (!RomString.str_prefix(ch.pcdata.alias[alias], argument))
                {
                    string point = RomString.one_argument(argument, out string name);
                    if (ch.pcdata.alias[alias] == name)
                    {
                        buf = ch.pcdata.alias_sub[alias] ?? "";
                        if (point.Length != 0)
                            buf = buf + " " + point;

                        if (buf.Length > MAX_INPUT_LENGTH - 1)
                        {
                            Comm.send_to_char(
                                "Alias substitution too long. Truncated.\r\n", ch);
                            buf = buf.Substring(0, MAX_INPUT_LENGTH - 1);
                        }
                        break;
                    }
                }
            }
            Interp.interpret(d.character, buf);
        }

        public static void do_alia(CharData ch, string argument)
        {
            Comm.send_to_char("I'm sorry, alias must be entered in full.\n\r", ch);
        }

        public static void do_alias(CharData ch, string argument)
        {
            argument = RomString.smash_tilde(argument);

            CharData rch;
            if (ch.desc == null)
                rch = ch;
            else
                rch = ch.desc.original != null ? ch.desc.original : ch;

            if (Bit.IS_NPC(rch))
                return;

            argument = RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                if (rch.pcdata.alias[0] == null)
                {
                    Comm.send_to_char("You have no aliases defined.\n\r", ch);
                    return;
                }
                Comm.send_to_char("Your current aliases are:\n\r", ch);

                for (int pos = 0; pos < MAX_ALIAS; pos++)
                {
                    if (rch.pcdata.alias[pos] == null
                        || rch.pcdata.alias_sub[pos] == null)
                        break;

                    Comm.send_to_char(RomString.sprintf("    %s:  %s\n\r",
                        rch.pcdata.alias[pos],
                        rch.pcdata.alias_sub[pos]), ch);
                }
                return;
            }

            if (!RomString.str_prefix("una", arg) || !RomString.str_cmp("alias", arg))
            {
                Comm.send_to_char("Sorry, that word is reserved.\n\r", ch);
                return;
            }

            if (arg.IndexOf(' ') >= 0 || arg.IndexOf('"') >= 0 || arg.IndexOf('\'') >= 0)
            {
                Comm.send_to_char("The word to be aliased should not contain a space, "
                    + "a tick or a double-quote.\n\r", ch);
                return;
            }

            if (argument.Length == 0)
            {
                for (int pos = 0; pos < MAX_ALIAS; pos++)
                {
                    if (rch.pcdata.alias[pos] == null
                        || rch.pcdata.alias_sub[pos] == null)
                        break;

                    if (!RomString.str_cmp(arg, rch.pcdata.alias[pos]))
                    {
                        Comm.send_to_char(RomString.sprintf("%s aliases to '%s'.\n\r",
                            rch.pcdata.alias[pos],
                            rch.pcdata.alias_sub[pos]), ch);
                        return;
                    }
                }

                Comm.send_to_char("That alias is not defined.\n\r", ch);
                return;
            }

            if (!RomString.str_prefix(argument, "delete") || !RomString.str_prefix(argument, "prefix"))
            {
                Comm.send_to_char("That shall not be done!\n\r", ch);
                return;
            }

            int slot;
            for (slot = 0; slot < MAX_ALIAS; slot++)
            {
                if (rch.pcdata.alias[slot] == null)
                    break;

                if (!RomString.str_cmp(arg, rch.pcdata.alias[slot]))
                {
                    rch.pcdata.alias_sub[slot] = argument;
                    Comm.send_to_char(RomString.sprintf("%s is now realiased to '%s'.\n\r",
                        arg, argument), ch);
                    return;
                }
            }

            if (slot >= MAX_ALIAS)
            {
                Comm.send_to_char("Sorry, you have reached the alias limit.\n\r", ch);
                return;
            }

            rch.pcdata.alias[slot] = arg;
            rch.pcdata.alias_sub[slot] = argument;
            Comm.send_to_char(RomString.sprintf("%s is now aliased to '%s'.\n\r",
                arg, argument), ch);
        }

        public static void do_unalias(CharData ch, string argument)
        {
            CharData rch;
            if (ch.desc == null)
                rch = ch;
            else
                rch = ch.desc.original != null ? ch.desc.original : ch;

            if (Bit.IS_NPC(rch))
                return;

            argument = RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Unalias what?\n\r", ch);
                return;
            }

            bool found = false;
            for (int pos = 0; pos < MAX_ALIAS; pos++)
            {
                if (rch.pcdata.alias[pos] == null)
                    break;

                if (found)
                {
                    rch.pcdata.alias[pos - 1] = rch.pcdata.alias[pos];
                    rch.pcdata.alias_sub[pos - 1] = rch.pcdata.alias_sub[pos];
                    rch.pcdata.alias[pos] = null;
                    rch.pcdata.alias_sub[pos] = null;
                    continue;
                }

                if (arg == rch.pcdata.alias[pos])
                {
                    Comm.send_to_char("Alias removed.\n\r", ch);
                    rch.pcdata.alias[pos] = null;
                    rch.pcdata.alias_sub[pos] = null;
                    found = true;
                }
            }

            if (!found)
                Comm.send_to_char("No alias of that name to remove.\n\r", ch);
        }
    }
}
