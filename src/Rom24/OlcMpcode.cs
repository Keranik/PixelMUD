using static Rom24.Merc;

namespace Rom24
{
    public static partial class Olc
    {
        static readonly (string name, OlcFun olc_fun)[] mpedit_table =
        {
            ("commands", show_commands),
            ("create", mpedit_create),
            ("code", mpedit_code),
            ("show", mpedit_show),
            ("list", mpedit_list),
            ("?", show_help),
            (null, null)
        };

        public static void mpedit(CharData ch, string argument)
        {
            argument = RomString.smash_tilde(argument);
            string arg = argument;
            argument = RomString.one_argument(argument, out string command);

            var pMcode = (MprogCode)ch.desc.pEdit;

            if (pMcode != null)
            {
                var ad = get_vnum_area(pMcode.vnum);

                if (ad == null)
                {
                    edit_done(ch);
                    return;
                }

                if (!Bit.IS_BUILDER(ch, ad))
                {
                    Comm.send_to_char("MPEdit: Insufficient security to modify code.\n\r",
                        ch);
                    edit_done(ch);
                    return;
                }
            }

            if (command.Length == 0)
            {
                mpedit_show(ch, argument);
                return;
            }

            if (!RomString.str_cmp(command, "done"))
            {
                edit_done(ch);
                return;
            }

            for (int cmd = 0; mpedit_table[cmd].name != null; cmd++)
            {
                if (!RomString.str_prefix(command, mpedit_table[cmd].name))
                {
                    if (mpedit_table[cmd].olc_fun(ch, argument) && pMcode != null)
                    {
                        var ad = get_vnum_area(pMcode.vnum);
                        if (ad != null)
                            Bit.SET_BIT(ref ad.area_flags, AREA_CHANGED);
                    }
                    return;
                }
            }

            Interp.interpret(ch, arg);
        }

        static bool mpedit_show(CharData ch, string argument)
        {
            var pMcode = (MprogCode)ch.desc.pEdit;

            Comm.send_to_char(RomString.sprintf(
                "Vnum:       [%d]\n\r"
                + "Code:\n\r%s\n\r", pMcode.vnum, pMcode.code), ch);

            return false;
        }

        static bool mpedit_code(CharData ch, string argument)
        {
            var pMcode = (MprogCode)ch.desc.pEdit;

            if (argument.Length == 0)
            {
                RomString.string_append(ch, new StringPtr(() => pMcode.code, v => pMcode.code = v));
                return true;
            }

            Comm.send_to_char("Syntax: code\n\r", ch);
            return false;
        }

        static bool mpedit_list(CharData ch, string argument)
        {
            int count = 1;
            bool fAll = !RomString.str_cmp(argument, "all");
            var buffer = new System.Text.StringBuilder();

            for (var mprg = Game.mprog_list; mprg != null; mprg = mprg.next)
                if (fAll
                    || (ch.in_room.area.min_vnum < mprg.vnum
                        && mprg.vnum < ch.in_room.area.max_vnum))
                {
                    var ad = get_vnum_area(mprg.vnum);
                    char blah;
                    if (ad == null)
                        blah = '?';
                    else if (Bit.IS_BUILDER(ch, ad))
                        blah = '*';
                    else
                        blah = ' ';

                    buffer.Append(RomString.sprintf("[%3d] (%c) %5d\n\r", count, blah, mprg.vnum));

                    count++;
                }

            if (count == 1)
            {
                if (fAll)
                    buffer.Append("MobPrograms do not exist!\n\r");
                else
                    buffer.Append("MobPrograms do not exist in this area.\n\r");
            }

            Comm.page_to_char(buffer.ToString(), ch);

            return false;
        }
    }
}
