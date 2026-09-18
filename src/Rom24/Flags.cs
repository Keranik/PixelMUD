using static Rom24.Merc;

namespace Rom24
{
    public static class Flags
    {
        public static void do_flag(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg1);
            argument = RomString.one_argument(argument, out string arg2);
            argument = RomString.one_argument(argument, out string arg3);

            char type = argument.Length == 0 ? '\0' : argument[0];

            if (type == '=' || type == '-' || type == '+')
                argument = RomString.one_argument(argument, out _);

            if (arg1.Length == 0)
            {
                Comm.send_to_char("Syntax:\n\r", ch);
                Comm.send_to_char("  flag mob  <name> <field> <flags>\n\r", ch);
                Comm.send_to_char("  flag char <name> <field> <flags>\n\r", ch);
                Comm.send_to_char("  mob  flags: act,aff,off,imm,res,vuln,form,part\n\r",
                    ch);
                Comm.send_to_char("  char flags: plr,comm,aff,imm,res,vuln,\n\r", ch);
                Comm.send_to_char("  +: add flag, -: remove flag, = set equal to\n\r",
                    ch);
                Comm.send_to_char("  otherwise flag toggles the flags listed.\n\r", ch);
                return;
            }

            if (arg2.Length == 0)
            {
                Comm.send_to_char("What do you wish to set flags on?\n\r", ch);
                return;
            }

            if (arg3.Length == 0)
            {
                Comm.send_to_char("You need to specify a flag to set.\n\r", ch);
                return;
            }

            if (argument.Length == 0)
            {
                Comm.send_to_char("Which flags do you wish to change?\n\r", ch);
                return;
            }

            if (!RomString.str_prefix(arg1, "mob") || !RomString.str_prefix(arg1, "char"))
            {
                var victim = Handler.get_char_world(ch, arg2);
                if (victim == null)
                {
                    Comm.send_to_char("You can't find them.\n\r", ch);
                    return;
                }

                FlagType[] flag_table;
                long flags;

                if (!RomString.str_prefix(arg3, "act"))
                {
                    if (!Bit.IS_NPC(victim))
                    {
                        Comm.send_to_char("Use plr for PCs.\n\r", ch);
                        return;
                    }

                    flags = victim.act;
                    flag_table = Tables.act_flags;
                }
                else if (!RomString.str_prefix(arg3, "plr"))
                {
                    if (Bit.IS_NPC(victim))
                    {
                        Comm.send_to_char("Use act for NPCs.\n\r", ch);
                        return;
                    }

                    flags = victim.act;
                    flag_table = Tables.plr_flags;
                }
                else if (!RomString.str_prefix(arg3, "aff"))
                {
                    flags = victim.affected_by;
                    flag_table = Tables.affect_flags;
                }
                else if (!RomString.str_prefix(arg3, "immunity"))
                {
                    flags = victim.imm_flags;
                    flag_table = Tables.imm_flags;
                }
                else if (!RomString.str_prefix(arg3, "resist"))
                {
                    flags = victim.res_flags;
                    flag_table = Tables.imm_flags;
                }
                else if (!RomString.str_prefix(arg3, "vuln"))
                {
                    flags = victim.vuln_flags;
                    flag_table = Tables.imm_flags;
                }
                else if (!RomString.str_prefix(arg3, "form"))
                {
                    if (!Bit.IS_NPC(victim))
                    {
                        Comm.send_to_char("Form can't be set on PCs.\n\r", ch);
                        return;
                    }

                    flags = victim.form;
                    flag_table = Tables.form_flags;
                }
                else if (!RomString.str_prefix(arg3, "parts"))
                {
                    if (!Bit.IS_NPC(victim))
                    {
                        Comm.send_to_char("Parts can't be set on PCs.\n\r", ch);
                        return;
                    }

                    flags = victim.parts;
                    flag_table = Tables.part_flags;
                }
                else if (!RomString.str_prefix(arg3, "comm"))
                {
                    if (Bit.IS_NPC(victim))
                    {
                        Comm.send_to_char("Comm can't be set on NPCs.\n\r", ch);
                        return;
                    }

                    flags = victim.comm;
                    flag_table = Tables.comm_flags;
                }
                else
                {
                    Comm.send_to_char("That's not an acceptable flag.\n\r", ch);
                    return;
                }

                long old = flags;
                victim.zone = null;
                long new_flags = 0;
                if (type != '=')
                    new_flags = old;

                long marked = 0;
                for (;;)
                {
                    argument = RomString.one_argument(argument, out string word);

                    if (word.Length == 0)
                        break;

                    int pos = Lookup.flag_lookup(word, flag_table);

                    if (pos == NO_FLAG)
                    {
                        Comm.send_to_char("That flag doesn't exist!\n\r", ch);
                        return;
                    }
                    else
                        Bit.SET_BIT(ref marked, pos);
                }

                for (int pos = 0; flag_table[pos].name != null; pos++)
                {
                    if (!flag_table[pos].settable
                        && Bit.IS_SET(old, flag_table[pos].bit))
                    {
                        Bit.SET_BIT(ref new_flags, flag_table[pos].bit);
                        continue;
                    }

                    if (Bit.IS_SET(marked, flag_table[pos].bit))
                    {
                        switch (type)
                        {
                            case '=':
                            case '+':
                                Bit.SET_BIT(ref new_flags, flag_table[pos].bit);
                                break;
                            case '-':
                                Bit.REMOVE_BIT(ref new_flags, flag_table[pos].bit);
                                break;
                            default:
                                if (Bit.IS_SET(new_flags, flag_table[pos].bit))
                                    Bit.REMOVE_BIT(ref new_flags, flag_table[pos].bit);
                                else
                                    Bit.SET_BIT(ref new_flags, flag_table[pos].bit);
                                break;
                        }
                    }
                }

                if (!RomString.str_prefix(arg3, "act") || !RomString.str_prefix(arg3, "plr"))
                    victim.act = new_flags;
                else if (!RomString.str_prefix(arg3, "aff"))
                    victim.affected_by = new_flags;
                else if (!RomString.str_prefix(arg3, "immunity"))
                    victim.imm_flags = new_flags;
                else if (!RomString.str_prefix(arg3, "resist"))
                    victim.res_flags = new_flags;
                else if (!RomString.str_prefix(arg3, "vuln"))
                    victim.vuln_flags = new_flags;
                else if (!RomString.str_prefix(arg3, "form"))
                    victim.form = new_flags;
                else if (!RomString.str_prefix(arg3, "parts"))
                    victim.parts = new_flags;
                else if (!RomString.str_prefix(arg3, "comm"))
                    victim.comm = new_flags;
            }
        }
    }
}
