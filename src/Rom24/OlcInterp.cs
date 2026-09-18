using static Rom24.Merc;

namespace Rom24
{
    public static partial class Olc
    {
        public const string VERSION =
            "ILAB Online Creation [Beta 1.0, ROM 2.3 modified]\n\r" +
            "     Port a ROM 2.4 v1.8\n\r";
        public const string AUTHOR =
            "     By Jason(jdinkel@mines.colorado.edu)\n\r" +
            "     Modified for use with ROM 2.3\n\r" +
            "     By Hans Birkeland (hansbi@ifi.uio.no)\n\r" +
            "     Modificado para uso en ROM 2.4b6\n\r" +
            "     Por Ivan Toledo (itoledo@ctcreuna.cl)\n\r";
        public const string DATE =
            "     (Apr. 7, 1995 - ROM mod, Apr 16, 1995)\n\r" +
            "     (Port a ROM 2.4 - Nov 2, 1996)\n\r" +
            "     Version actual : 1.8 - Sep 8, 1998\n\r";
        public const string CREDITS =
            "     Original by Surreality(cxw197@psu.edu) and Locke(locke@lm.com)";

        static readonly (string name, OlcFun olc_fun)[] aedit_table =
        {
            ("age", aedit_age),
            ("builder", aedit_builder),
            ("commands", show_commands),
            ("create", aedit_create),
            ("filename", aedit_file),
            ("name", aedit_name),
            ("reset", aedit_reset),
            ("security", aedit_security),
            ("show", aedit_show),
            ("vnum", aedit_vnum),
            ("lvnum", aedit_lvnum),
            ("uvnum", aedit_uvnum),
            ("credits", aedit_credits),
            ("?", show_help),
            ("version", show_version),
            (null, null)
        };

        static readonly (string name, OlcFun olc_fun)[] redit_table =
        {
            ("commands", show_commands),
            ("create", redit_create),
            ("desc", redit_desc),
            ("ed", redit_ed),
            ("format", redit_format),
            ("name", redit_name),
            ("show", redit_show),
            ("heal", redit_heal),
            ("mana", redit_mana),
            ("clan", redit_clan),
            ("north", redit_north),
            ("south", redit_south),
            ("east", redit_east),
            ("west", redit_west),
            ("up", redit_up),
            ("down", redit_down),
            ("mreset", redit_mreset),
            ("oreset", redit_oreset),
            ("mlist", redit_mlist),
            ("rlist", redit_rlist),
            ("olist", redit_olist),
            ("mshow", redit_mshow),
            ("oshow", redit_oshow),
            ("owner", redit_owner),
            ("room", redit_room),
            ("sector", redit_sector),
            ("?", show_help),
            ("version", show_version),
            (null, null)
        };

        static readonly (string name, OlcFun olc_fun)[] oedit_table =
        {
            ("addaffect", oedit_addaffect),
            ("addapply", oedit_addapply),
            ("commands", show_commands),
            ("cost", oedit_cost),
            ("create", oedit_create),
            ("delaffect", oedit_delaffect),
            ("ed", oedit_ed),
            ("long", oedit_long),
            ("name", oedit_name),
            ("short", oedit_short),
            ("show", oedit_show),
            ("v0", oedit_value0),
            ("v1", oedit_value1),
            ("v2", oedit_value2),
            ("v3", oedit_value3),
            ("v4", oedit_value4),
            ("weight", oedit_weight),
            ("extra", oedit_extra),
            ("wear", oedit_wear),
            ("type", oedit_type),
            ("material", oedit_material),
            ("level", oedit_level),
            ("condition", oedit_condition),
            ("?", show_help),
            ("version", show_version),
            (null, null)
        };

        static readonly (string name, OlcFun olc_fun)[] medit_table =
        {
            ("alignment", medit_align),
            ("commands", show_commands),
            ("create", medit_create),
            ("desc", medit_desc),
            ("level", medit_level),
            ("long", medit_long),
            ("name", medit_name),
            ("shop", medit_shop),
            ("short", medit_short),
            ("show", medit_show),
            ("spec", medit_spec),
            ("sex", medit_sex),
            ("act", medit_act),
            ("affect", medit_affect),
            ("armor", medit_ac),
            ("form", medit_form),
            ("part", medit_part),
            ("imm", medit_imm),
            ("res", medit_res),
            ("vuln", medit_vuln),
            ("material", medit_material),
            ("off", medit_off),
            ("size", medit_size),
            ("hitdice", medit_hitdice),
            ("manadice", medit_manadice),
            ("damdice", medit_damdice),
            ("race", medit_race),
            ("position", medit_position),
            ("wealth", medit_gold),
            ("hitroll", medit_hitroll),
            ("damtype", medit_damtype),
            ("group", medit_group),
            ("addmprog", medit_addmprog),
            ("delmprog", medit_delmprog),
            ("?", show_help),
            ("version", show_version),
            (null, null)
        };

        public static bool run_olc_editor(DescriptorData d)
        {
            switch (d.editor)
            {
                case ED_AREA:
                    aedit(d.character, d.incomm);
                    break;
                case ED_ROOM:
                    redit(d.character, d.incomm);
                    break;
                case ED_OBJECT:
                    oedit(d.character, d.incomm);
                    break;
                case ED_MOBILE:
                    medit(d.character, d.incomm);
                    break;
                case ED_MPCODE:
                    mpedit(d.character, d.incomm);
                    break;
                case ED_HELP:
                    hedit(d.character, d.incomm);
                    break;
                default:
                    return false;
            }
            return true;
        }

        public static string olc_ed_name(CharData ch)
        {
            switch (ch.desc.editor)
            {
                case ED_AREA: return "AEdit";
                case ED_ROOM: return "REdit";
                case ED_OBJECT: return "OEdit";
                case ED_MOBILE: return "MEdit";
                case ED_MPCODE: return "MPEdit";
                case ED_HELP: return "HEdit";
                default: return " ";
            }
        }

        public static string olc_ed_vnum(CharData ch)
        {
            switch (ch.desc.editor)
            {
                case ED_AREA:
                    var pArea = (AreaData)ch.desc.pEdit;
                    return RomString.sprintf("%d", pArea != null ? pArea.vnum : 0);
                case ED_ROOM:
                    var pRoom = ch.in_room;
                    return RomString.sprintf("%d", pRoom != null ? pRoom.vnum : 0);
                case ED_OBJECT:
                    var pObj = (ObjIndexData)ch.desc.pEdit;
                    return RomString.sprintf("%d", pObj != null ? pObj.vnum : 0);
                case ED_MOBILE:
                    var pMob = (MobIndexData)ch.desc.pEdit;
                    return RomString.sprintf("%d", pMob != null ? pMob.vnum : 0);
                case ED_MPCODE:
                    var pMprog = (MprogCode)ch.desc.pEdit;
                    return RomString.sprintf("%d", pMprog != null ? pMprog.vnum : 0);
                case ED_HELP:
                    var pHelp = (HelpData)ch.desc.pEdit;
                    return RomString.sprintf("%s", pHelp != null ? pHelp.keyword : "");
                default:
                    return " ";
            }
        }

        static void show_olc_cmds(CharData ch, (string name, OlcFun olc_fun)[] olc_table)
        {
            string buf1 = "";
            int col = 0;
            for (int cmd = 0; olc_table[cmd].name != null; cmd++)
            {
                buf1 += RomString.sprintf("%-15.15s", olc_table[cmd].name);
                if (++col % 5 == 0)
                    buf1 += "\n\r";
            }

            if (col % 5 != 0)
                buf1 += "\n\r";

            Comm.send_to_char(buf1, ch);
        }

        public static bool show_commands(CharData ch, string argument)
        {
            switch (ch.desc.editor)
            {
                case ED_AREA:
                    show_olc_cmds(ch, aedit_table);
                    break;
                case ED_ROOM:
                    show_olc_cmds(ch, redit_table);
                    break;
                case ED_OBJECT:
                    show_olc_cmds(ch, oedit_table);
                    break;
                case ED_MOBILE:
                    show_olc_cmds(ch, medit_table);
                    break;
                case ED_MPCODE:
                    show_olc_cmds(ch, mpedit_table);
                    break;
                case ED_HELP:
                    show_olc_cmds(ch, hedit_table);
                    break;
            }

            return false;
        }

        public static void aedit(CharData ch, string argument)
        {
            var pArea = (AreaData)ch.desc.pEdit;
            argument = RomString.smash_tilde(argument);
            string arg = argument;
            argument = RomString.one_argument(argument, out string command);

            if (!Bit.IS_BUILDER(ch, pArea))
            {
                Comm.send_to_char("AEdit:  Insufficient security to modify area.\n\r",
                    ch);
                edit_done(ch);
                return;
            }

            if (!RomString.str_cmp(command, "done"))
            {
                edit_done(ch);
                return;
            }

            if (command.Length == 0)
            {
                aedit_show(ch, argument);
                return;
            }

            int value = Lookup.flag_value(Tables.area_flags, command);
            if (value != NO_FLAG)
            {
                Bit.TOGGLE_BIT(ref pArea.area_flags, value);

                Comm.send_to_char("Flag toggled.\n\r", ch);
                return;
            }

            for (int cmd = 0; aedit_table[cmd].name != null; cmd++)
            {
                if (!RomString.str_prefix(command, aedit_table[cmd].name))
                {
                    if (aedit_table[cmd].olc_fun(ch, argument))
                    {
                        Bit.SET_BIT(ref pArea.area_flags, AREA_CHANGED);
                        return;
                    }
                    else
                        return;
                }
            }

            Interp.interpret(ch, arg);
        }

        public static void redit(CharData ch, string argument)
        {
            var pRoom = ch.in_room;
            var pArea = pRoom.area;

            argument = RomString.smash_tilde(argument);
            string arg = argument;
            argument = RomString.one_argument(argument, out string command);

            if (!Bit.IS_BUILDER(ch, pArea))
            {
                Comm.send_to_char("REdit:  Insufficient security to modify room.\n\r",
                    ch);
                edit_done(ch);
                return;
            }

            if (!RomString.str_cmp(command, "done"))
            {
                edit_done(ch);
                return;
            }

            if (command.Length == 0)
            {
                redit_show(ch, argument);
                return;
            }

            for (int cmd = 0; redit_table[cmd].name != null; cmd++)
            {
                if (!RomString.str_prefix(command, redit_table[cmd].name))
                {
                    if (redit_table[cmd].olc_fun(ch, argument))
                    {
                        Bit.SET_BIT(ref pArea.area_flags, AREA_CHANGED);
                        return;
                    }
                    else
                        return;
                }
            }

            Interp.interpret(ch, arg);
        }

        public static void oedit(CharData ch, string argument)
        {
            argument = RomString.smash_tilde(argument);
            string arg = argument;
            argument = RomString.one_argument(argument, out string command);

            var pObj = (ObjIndexData)ch.desc.pEdit;
            var pArea = pObj.area;

            if (!Bit.IS_BUILDER(ch, pArea))
            {
                Comm.send_to_char("OEdit: Insufficient security to modify area.\n\r", ch);
                edit_done(ch);
                return;
            }

            if (!RomString.str_cmp(command, "done"))
            {
                edit_done(ch);
                return;
            }

            if (command.Length == 0)
            {
                oedit_show(ch, argument);
                return;
            }

            for (int cmd = 0; oedit_table[cmd].name != null; cmd++)
            {
                if (!RomString.str_prefix(command, oedit_table[cmd].name))
                {
                    if (oedit_table[cmd].olc_fun(ch, argument))
                    {
                        Bit.SET_BIT(ref pArea.area_flags, AREA_CHANGED);
                        return;
                    }
                    else
                        return;
                }
            }

            Interp.interpret(ch, arg);
        }

        public static void medit(CharData ch, string argument)
        {
            argument = RomString.smash_tilde(argument);
            string arg = argument;
            argument = RomString.one_argument(argument, out string command);

            var pMob = (MobIndexData)ch.desc.pEdit;
            var pArea = pMob.area;

            if (!Bit.IS_BUILDER(ch, pArea))
            {
                Comm.send_to_char("MEdit: Insufficient security to modify area.\n\r", ch);
                edit_done(ch);
                return;
            }

            if (!RomString.str_cmp(command, "done"))
            {
                edit_done(ch);
                return;
            }

            if (command.Length == 0)
            {
                medit_show(ch, argument);
                return;
            }

            for (int cmd = 0; medit_table[cmd].name != null; cmd++)
            {
                if (!RomString.str_prefix(command, medit_table[cmd].name))
                {
                    if (medit_table[cmd].olc_fun(ch, argument))
                    {
                        Bit.SET_BIT(ref pArea.area_flags, AREA_CHANGED);
                        return;
                    }
                    else
                        return;
                }
            }

            Interp.interpret(ch, arg);
        }
    }
}
