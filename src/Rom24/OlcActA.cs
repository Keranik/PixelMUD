using static Rom24.Merc;

namespace Rom24
{
    public static partial class Olc
    {
        const int MAX_MOB = 1;

        class OlcHelpType
        {
            public string command;
            public object structure;
            public string desc;
            public OlcHelpType(string command, object structure, string desc)
            {
                this.command = command;
                this.structure = structure;
                this.desc = desc;
            }
        }

        public static bool show_version(CharData ch, string argument)
        {
            argument ??= "";
            Comm.send_to_char(VERSION, ch);
            Comm.send_to_char("\n\r", ch);
            Comm.send_to_char(AUTHOR, ch);
            Comm.send_to_char("\n\r", ch);
            Comm.send_to_char(DATE, ch);
            Comm.send_to_char("\n\r", ch);
            Comm.send_to_char(CREDITS, ch);
            Comm.send_to_char("\n\r", ch);

            return false;
        }

        static readonly OlcHelpType[] help_table =
        {
            new OlcHelpType("area", Tables.area_flags, "Area attributes."),
            new OlcHelpType("room", Tables.room_flags, "Room attributes."),
            new OlcHelpType("sector", Tables.sector_flags, "Sector types, terrain."),
            new OlcHelpType("exit", Tables.exit_flags, "Exit types."),
            new OlcHelpType("type", Tables.type_flags, "Types of objects."),
            new OlcHelpType("extra", Tables.extra_flags, "Object attributes."),
            new OlcHelpType("wear", Tables.wear_flags, "Where to wear object."),
            new OlcHelpType("spec", Special.spec_table, "Available special programs."),
            new OlcHelpType("sex", Tables.sex_flags, "Sexes."),
            new OlcHelpType("act", Tables.act_flags, "Mobile attributes."),
            new OlcHelpType("affect", Tables.affect_flags, "Mobile affects."),
            new OlcHelpType("wear-loc", Tables.wear_loc_flags, "Where mobile wears object."),
            new OlcHelpType("spells", Tables.skill_table, "Names of current spells."),
            new OlcHelpType("container", Tables.container_flags, "Container status."),
            new OlcHelpType("armor", Tables.ac_type, "Ac for different attacks."),
            new OlcHelpType("apply", Tables.apply_flags, "Apply flags"),
            new OlcHelpType("form", Tables.form_flags, "Mobile body form."),
            new OlcHelpType("part", Tables.part_flags, "Mobile body parts."),
            new OlcHelpType("imm", Tables.imm_flags, "Mobile immunity."),
            new OlcHelpType("res", Tables.res_flags, "Mobile resistance."),
            new OlcHelpType("vuln", Tables.vuln_flags, "Mobile vulnerability."),
            new OlcHelpType("off", Tables.off_flags, "Mobile offensive behaviour."),
            new OlcHelpType("size", Tables.size_flags, "Mobile size."),
            new OlcHelpType("position", Tables.position_flags, "Mobile positions."),
            new OlcHelpType("wclass", Tables.weapon_class, "Weapon class."),
            new OlcHelpType("wtype", Tables.weapon_type2, "Special weapon type."),
            new OlcHelpType("portal", Tables.portal_flags, "Portal types."),
            new OlcHelpType("furniture", Tables.furniture_flags, "Furniture types."),
            new OlcHelpType("liquid", Tables.liq_table, "Liquid types."),
            new OlcHelpType("apptype", Tables.apply_types, "Apply types."),
            new OlcHelpType("weapon", Tables.attack_table, "Weapon types."),
            new OlcHelpType("mprog", Tables.mprog_flags, "MobProgram flags."),
            new OlcHelpType(null, null, null)
        };

        static void show_flag_cmds(CharData ch, FlagType[] flag_table)
        {
            string buf1 = "";
            int col = 0;
            for (int flag = 0; flag_table[flag].name != null; flag++)
            {
                if (flag_table[flag].settable)
                {
                    string buf = RomString.sprintf("%-19.18s", flag_table[flag].name);
                    buf1 += buf;
                    if (++col % 4 == 0)
                        buf1 += "\n\r";
                }
            }

            if (col % 4 != 0)
                buf1 += "\n\r";

            Comm.send_to_char(buf1, ch);
            return;
        }

        static void show_skill_cmds(CharData ch, int tar)
        {
            string buf1 = "";
            int col = 0;
            for (int sn = 0; sn < MAX_SKILL; sn++)
            {
                if (Tables.skill_table[sn].name == null)
                    break;

                if (!RomString.str_cmp(Tables.skill_table[sn].name, "reserved")
                    || Tables.skill_table[sn].spell_fun == Magic.spell_null)
                    continue;

                if (tar == -1 || Tables.skill_table[sn].target == tar)
                {
                    string buf = RomString.sprintf("%-19.18s", Tables.skill_table[sn].name);
                    buf1 += buf;
                    if (++col % 4 == 0)
                        buf1 += "\n\r";
                }
            }

            if (col % 4 != 0)
                buf1 += "\n\r";

            Comm.send_to_char(buf1, ch);
            return;
        }

        static void show_spec_cmds(CharData ch)
        {
            string buf1 = "";
            int col = 0;
            Comm.send_to_char("Preceed special functions with 'spec_'\n\r\n\r", ch);
            for (int spec = 0; Special.spec_table[spec].function != null; spec++)
            {
                string buf = RomString.sprintf("%-19.18s", Special.spec_table[spec].name.Substring(5));
                buf1 += buf;
                if (++col % 4 == 0)
                    buf1 += "\n\r";
            }

            if (col % 4 != 0)
                buf1 += "\n\r";

            Comm.send_to_char(buf1, ch);
            return;
        }

        public static bool show_help(CharData ch, string argument)
        {
            argument ??= "";
            argument = RomString.one_argument(argument, out string arg);
            RomString.one_argument(argument, out string spell);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Syntax:  ? [command]\n\r\n\r", ch);
                Comm.send_to_char("[command]  [description]\n\r", ch);
                for (int cnt = 0; help_table[cnt].command != null; cnt++)
                {
                    string buf = RomString.sprintf("%-10.10s -%s\n\r",
                        RomString.capitalize(help_table[cnt].command),
                        help_table[cnt].desc);
                    Comm.send_to_char(buf, ch);
                }
                return false;
            }

            for (int cnt = 0; help_table[cnt].command != null; cnt++)
            {
                if (arg[0] == help_table[cnt].command[0]
                    && !RomString.str_prefix(arg, help_table[cnt].command))
                {
                    if (help_table[cnt].structure == Special.spec_table)
                    {
                        show_spec_cmds(ch);
                        return false;
                    }
                    else if (help_table[cnt].structure == Tables.liq_table)
                    {
                        show_liqlist(ch);
                        return false;
                    }
                    else if (help_table[cnt].structure == Tables.attack_table)
                    {
                        show_damlist(ch);
                        return false;
                    }
                    else if (help_table[cnt].structure == Tables.skill_table)
                    {
                        if (spell.Length == 0)
                        {
                            Comm.send_to_char("Syntax:  ? spells "
                                + "[ignore/attack/defend/self/object/all]\n\r",
                                ch);
                            return false;
                        }

                        if (!RomString.str_prefix(spell, "all"))
                            show_skill_cmds(ch, -1);
                        else if (!RomString.str_prefix(spell, "ignore"))
                            show_skill_cmds(ch, TAR_IGNORE);
                        else if (!RomString.str_prefix(spell, "attack"))
                            show_skill_cmds(ch, TAR_CHAR_OFFENSIVE);
                        else if (!RomString.str_prefix(spell, "defend"))
                            show_skill_cmds(ch, TAR_CHAR_DEFENSIVE);
                        else if (!RomString.str_prefix(spell, "self"))
                            show_skill_cmds(ch, TAR_CHAR_SELF);
                        else if (!RomString.str_prefix(spell, "object"))
                            show_skill_cmds(ch, TAR_OBJ_INV);
                        else
                            Comm.send_to_char("Syntax:  ? spell "
                                + "[ignore/attack/defend/self/object/all]\n\r",
                                ch);

                        return false;
                    }
                    else
                    {
                        show_flag_cmds(ch, (FlagType[])help_table[cnt].structure);
                        return false;
                    }
                }
            }

            show_help(ch, "");
            return false;
        }

        public static bool redit_rlist(CharData ch, string argument)
        {
            argument ??= "";
            RomString.one_argument(argument, out string arg);

            var pArea = ch.in_room.area;
            string buf1 = "";
            bool found = false;
            int col = 0;

            for (int vnum = pArea.min_vnum; vnum <= pArea.max_vnum; vnum++)
            {
                RoomIndexData pRoomIndex;
                if ((pRoomIndex = Handler.get_room_index(vnum)) != null)
                {
                    found = true;
                    string buf = RomString.sprintf("[%5d] %-17.16s",
                        vnum, RomString.capitalize(pRoomIndex.name));
                    buf1 += buf;
                    if (++col % 3 == 0)
                        buf1 += "\n\r";
                }
            }

            if (!found)
            {
                Comm.send_to_char("Room(s) not found in this area.\n\r", ch);
                return false;
            }

            if (col % 3 != 0)
                buf1 += "\n\r";

            Comm.page_to_char(buf1, ch);
            return false;
        }

        public static bool redit_mlist(CharData ch, string argument)
        {
            argument ??= "";
            RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                Comm.send_to_char("Syntax:  mlist <all/name>\n\r", ch);
                return false;
            }

            string buf1 = "";
            var pArea = ch.in_room.area;
            bool fAll = !RomString.str_cmp(arg, "all");
            bool found = false;
            int col = 0;

            for (int vnum = pArea.min_vnum; vnum <= pArea.max_vnum; vnum++)
            {
                MobIndexData pMobIndex;
                if ((pMobIndex = Handler.get_mob_index(vnum)) != null)
                {
                    if (fAll || Handler.is_name(arg, pMobIndex.player_name))
                    {
                        found = true;
                        string buf = RomString.sprintf("[%5d] %-17.16s",
                            pMobIndex.vnum,
                            RomString.capitalize(pMobIndex.short_descr));
                        buf1 += buf;
                        if (++col % 3 == 0)
                            buf1 += "\n\r";
                    }
                }
            }

            if (!found)
            {
                Comm.send_to_char("Mobile(s) not found in this area.\n\r", ch);
                return false;
            }

            if (col % 3 != 0)
                buf1 += "\n\r";

            Comm.page_to_char(buf1, ch);
            return false;
        }

        public static bool redit_olist(CharData ch, string argument)
        {
            argument ??= "";
            RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                Comm.send_to_char("Syntax:  olist <all/name/item_type>\n\r", ch);
                return false;
            }

            var pArea = ch.in_room.area;
            string buf1 = "";
            bool fAll = !RomString.str_cmp(arg, "all");
            bool found = false;
            int col = 0;

            for (int vnum = pArea.min_vnum; vnum <= pArea.max_vnum; vnum++)
            {
                ObjIndexData pObjIndex;
                if ((pObjIndex = Handler.get_obj_index(vnum)) != null)
                {
                    if (fAll || Handler.is_name(arg, pObjIndex.name)
                        || Lookup.flag_value(Tables.type_flags, arg) == pObjIndex.item_type)
                    {
                        found = true;
                        string buf = RomString.sprintf("[%5d] %-17.16s",
                            pObjIndex.vnum,
                            RomString.capitalize(pObjIndex.short_descr));
                        buf1 += buf;
                        if (++col % 3 == 0)
                            buf1 += "\n\r";
                    }
                }
            }

            if (!found)
            {
                Comm.send_to_char("Object(s) not found in this area.\n\r", ch);
                return false;
            }

            if (col % 3 != 0)
                buf1 += "\n\r";

            Comm.page_to_char(buf1, ch);
            return false;
        }

        public static bool redit_mshow(CharData ch, string argument)
        {
            argument ??= "";
            if (argument.Length == 0)
            {
                Comm.send_to_char("Syntax:  mshow <vnum>\n\r", ch);
                return false;
            }

            if (!Interp.is_number(argument))
            {
                Comm.send_to_char("REdit: Must be a number.\n\r", ch);
                return false;
            }

            if (Interp.is_number(argument))
            {
                int value = Interp.atoi(argument);
                MobIndexData pMob;
                if ((pMob = Handler.get_mob_index(value)) == null)
                {
                    Comm.send_to_char("REdit:  That mobile does not exist.\n\r", ch);
                    return false;
                }

                ch.desc.pEdit = pMob;
            }

            medit_show(ch, argument);
            ch.desc.pEdit = ch.in_room;
            return false;
        }

        public static bool redit_oshow(CharData ch, string argument)
        {
            argument ??= "";
            if (argument.Length == 0)
            {
                Comm.send_to_char("Syntax:  oshow <vnum>\n\r", ch);
                return false;
            }

            if (!Interp.is_number(argument))
            {
                Comm.send_to_char("REdit: Must be a number.\n\r", ch);
                return false;
            }

            if (Interp.is_number(argument))
            {
                int value = Interp.atoi(argument);
                ObjIndexData pObj;
                if ((pObj = Handler.get_obj_index(value)) == null)
                {
                    Comm.send_to_char("REdit:  That object does not exist.\n\r", ch);
                    return false;
                }

                ch.desc.pEdit = pObj;
            }

            oedit_show(ch, argument);
            ch.desc.pEdit = ch.in_room;
            return false;
        }

        public static bool check_range(int lower, int upper)
        {
            int cnt = 0;

            for (var pArea = Game.area_first; pArea != null; pArea = pArea.next)
            {
                if ((lower <= pArea.min_vnum && pArea.min_vnum <= upper)
                    || (lower <= pArea.max_vnum && pArea.max_vnum <= upper))
                    ++cnt;

                if (cnt > 1)
                    return false;
            }
            return true;
        }

        public static bool aedit_show(CharData ch, string argument)
        {
            argument ??= "";
            var pArea = (AreaData)ch.desc.pEdit;

            string buf = RomString.sprintf("Name:     [%5d] %s\n\r", pArea.vnum, pArea.name);
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("File:     %s\n\r", pArea.file_name);
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Vnums:    [%d-%d]\n\r", pArea.min_vnum, pArea.max_vnum);
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Age:      [%d]\n\r", pArea.age);
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Players:  [%d]\n\r", pArea.nplayer);
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Security: [%d]\n\r", pArea.security);
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Builders: [%s]\n\r", pArea.builders);
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Credits : [%s]\n\r", pArea.credits);
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Flags:    [%s]\n\r",
                Lookup.flag_string(Tables.area_flags, pArea.area_flags));
            Comm.send_to_char(buf, ch);

            return false;
        }

        public static bool aedit_reset(CharData ch, string argument)
        {
            argument ??= "";
            var pArea = (AreaData)ch.desc.pEdit;

            Db.reset_area(pArea);
            Comm.send_to_char("Area reset.\n\r", ch);

            return false;
        }

        public static bool aedit_name(CharData ch, string argument)
        {
            argument ??= "";
            var pArea = (AreaData)ch.desc.pEdit;

            if (argument.Length == 0)
            {
                Comm.send_to_char("Syntax:   name [$name]\n\r", ch);
                return false;
            }

            pArea.name = argument;

            Comm.send_to_char("Name set.\n\r", ch);
            return true;
        }

        public static bool aedit_credits(CharData ch, string argument)
        {
            argument ??= "";
            var pArea = (AreaData)ch.desc.pEdit;

            if (argument.Length == 0)
            {
                Comm.send_to_char("Syntax:   credits [$credits]\n\r", ch);
                return false;
            }

            pArea.credits = argument;

            Comm.send_to_char("Credits set.\n\r", ch);
            return true;
        }

        public static bool aedit_file(CharData ch, string argument)
        {
            argument ??= "";
            var pArea = (AreaData)ch.desc.pEdit;

            RomString.one_argument(argument, out string file);

            if (argument.Length == 0)
            {
                Comm.send_to_char("Syntax:  filename [$file]\n\r", ch);
                return false;
            }

            int length = argument.Length;
            if (length > 8)
            {
                Comm.send_to_char("No more than eight characters allowed.\n\r", ch);
                return false;
            }

            for (int i = 0; i < length; i++)
            {
                char fc = i < file.Length ? file[i] : '\0';
                if (!char.IsAsciiLetterOrDigit(fc))
                {
                    Comm.send_to_char("Only letters and numbers are valid.\n\r", ch);
                    return false;
                }
            }

            file += ".are";
            pArea.file_name = file;

            Comm.send_to_char("Filename set.\n\r", ch);
            return true;
        }

        public static bool aedit_age(CharData ch, string argument)
        {
            argument ??= "";
            var pArea = (AreaData)ch.desc.pEdit;

            RomString.one_argument(argument, out string age);

            if (!Interp.is_number(age) || age.Length == 0)
            {
                Comm.send_to_char("Syntax:  age [#xage]\n\r", ch);
                return false;
            }

            pArea.age = Interp.atoi(age);

            Comm.send_to_char("Age set.\n\r", ch);
            return true;
        }

        public static bool aedit_security(CharData ch, string argument)
        {
            argument ??= "";
            var pArea = (AreaData)ch.desc.pEdit;

            RomString.one_argument(argument, out string sec);

            if (!Interp.is_number(sec) || sec.Length == 0)
            {
                Comm.send_to_char("Syntax:  security [#xlevel]\n\r", ch);
                return false;
            }

            int value = Interp.atoi(sec);

            if (value > ch.pcdata.security || value < 0)
            {
                if (ch.pcdata.security != 0)
                {
                    string buf = RomString.sprintf("Security is 0-%d.\n\r", ch.pcdata.security);
                    Comm.send_to_char(buf, ch);
                }
                else
                    Comm.send_to_char("Security is 0 only.\n\r", ch);
                return false;
            }

            pArea.security = value;

            Comm.send_to_char("Security set.\n\r", ch);
            return true;
        }

        public static bool aedit_builder(CharData ch, string argument)
        {
            argument ??= "";
            var pArea = (AreaData)ch.desc.pEdit;

            RomString.one_argument(argument, out string name);

            if (name.Length == 0)
            {
                Comm.send_to_char("Syntax:  builder [$name]  -toggles builder\n\r", ch);
                Comm.send_to_char("Syntax:  builder All      -allows everyone\n\r", ch);
                return false;
            }

            name = Bit.UPPER(name[0]) + name.Substring(1);

            if (pArea.builders.Contains(name))
            {
                pArea.builders = RomString.string_replace(pArea.builders, name, "");
                pArea.builders = RomString.string_unpad(pArea.builders);

                if (pArea.builders.Length == 0)
                {
                    pArea.builders = "None";
                }
                Comm.send_to_char("Builder removed.\n\r", ch);
                return true;
            }
            else
            {
                string buf = "";
                if (pArea.builders.Contains("None"))
                {
                    pArea.builders = RomString.string_replace(pArea.builders, "None", "");
                    pArea.builders = RomString.string_unpad(pArea.builders);
                }

                if (pArea.builders.Length != 0)
                {
                    buf += pArea.builders;
                    buf += " ";
                }
                buf += name;
                pArea.builders = RomString.string_proper(buf);

                Comm.send_to_char("Builder added.\n\r", ch);
                Comm.send_to_char(pArea.builders, ch);
                return true;
            }

            return false;
        }

        public static bool aedit_vnum(CharData ch, string argument)
        {
            argument ??= "";
            var pArea = (AreaData)ch.desc.pEdit;

            argument = RomString.one_argument(argument, out string lower);
            RomString.one_argument(argument, out string upper);

            if (!Interp.is_number(lower) || lower.Length == 0
                || !Interp.is_number(upper) || upper.Length == 0)
            {
                Comm.send_to_char("Syntax:  vnum [#xlower] [#xupper]\n\r", ch);
                return false;
            }

            int ilower = Interp.atoi(lower);
            int iupper = Interp.atoi(upper);
            if (ilower > iupper)
            {
                Comm.send_to_char("AEdit:  Upper must be larger then lower.\n\r", ch);
                return false;
            }

            if (!check_range(Interp.atoi(lower), Interp.atoi(upper)))
            {
                Comm.send_to_char("AEdit:  Range must include only this area.\n\r", ch);
                return false;
            }

            if (get_vnum_area(ilower) != null && get_vnum_area(ilower) != pArea)
            {
                Comm.send_to_char("AEdit:  Lower vnum already assigned.\n\r", ch);
                return false;
            }

            pArea.min_vnum = ilower;
            Comm.send_to_char("Lower vnum set.\n\r", ch);

            if (get_vnum_area(iupper) != null && get_vnum_area(iupper) != pArea)
            {
                Comm.send_to_char("AEdit:  Upper vnum already assigned.\n\r", ch);
                return true;
            }

            pArea.max_vnum = iupper;
            Comm.send_to_char("Upper vnum set.\n\r", ch);

            return true;
        }

        public static bool aedit_lvnum(CharData ch, string argument)
        {
            argument ??= "";
            var pArea = (AreaData)ch.desc.pEdit;

            RomString.one_argument(argument, out string lower);

            if (!Interp.is_number(lower) || lower.Length == 0)
            {
                Comm.send_to_char("Syntax:  min_vnum [#xlower]\n\r", ch);
                return false;
            }

            int ilower = Interp.atoi(lower);
            int iupper = pArea.max_vnum;
            if (ilower > iupper)
            {
                Comm.send_to_char("AEdit:  Value must be less than the max_vnum.\n\r",
                    ch);
                return false;
            }

            if (!check_range(ilower, iupper))
            {
                Comm.send_to_char("AEdit:  Range must include only this area.\n\r", ch);
                return false;
            }

            if (get_vnum_area(ilower) != null && get_vnum_area(ilower) != pArea)
            {
                Comm.send_to_char("AEdit:  Lower vnum already assigned.\n\r", ch);
                return false;
            }

            pArea.min_vnum = ilower;
            Comm.send_to_char("Lower vnum set.\n\r", ch);
            return true;
        }

        public static bool aedit_uvnum(CharData ch, string argument)
        {
            argument ??= "";
            var pArea = (AreaData)ch.desc.pEdit;

            RomString.one_argument(argument, out string upper);

            if (!Interp.is_number(upper) || upper.Length == 0)
            {
                Comm.send_to_char("Syntax:  max_vnum [#xupper]\n\r", ch);
                return false;
            }

            int ilower = pArea.min_vnum;
            int iupper = Interp.atoi(upper);
            if (ilower > iupper)
            {
                Comm.send_to_char("AEdit:  Upper must be larger then lower.\n\r", ch);
                return false;
            }

            if (!check_range(ilower, iupper))
            {
                Comm.send_to_char("AEdit:  Range must include only this area.\n\r", ch);
                return false;
            }

            if (get_vnum_area(iupper) != null && get_vnum_area(iupper) != pArea)
            {
                Comm.send_to_char("AEdit:  Upper vnum already assigned.\n\r", ch);
                return false;
            }

            pArea.max_vnum = iupper;
            Comm.send_to_char("Upper vnum set.\n\r", ch);

            return true;
        }

        public static bool redit_show(CharData ch, string argument)
        {
            argument ??= "";
            var pRoom = ch.in_room;
            string buf1 = "";

            string buf = RomString.sprintf("Description:\n\r%s", pRoom.description);
            buf1 += buf;

            buf = RomString.sprintf("Name:       [%s]\n\rArea:       [%5d] %s\n\r",
                pRoom.name, pRoom.area.vnum, pRoom.area.name);
            buf1 += buf;

            buf = RomString.sprintf("Vnum:       [%5d]\n\rSector:     [%s]\n\r",
                pRoom.vnum, Lookup.flag_string(Tables.sector_flags, pRoom.sector_type));
            buf1 += buf;

            buf = RomString.sprintf("Room flags: [%s]\n\r",
                Lookup.flag_string(Tables.room_flags, pRoom.room_flags));
            buf1 += buf;

            if (pRoom.heal_rate != 100 || pRoom.mana_rate != 100)
            {
                buf = RomString.sprintf("Health rec: [%d]\n\rMana rec  : [%d]\n\r",
                    pRoom.heal_rate, pRoom.mana_rate);
                buf1 += buf;
            }

            if (pRoom.clan > 0)
            {
                buf = RomString.sprintf("Clan      : [%d] %s\n\r",
                    pRoom.clan, Tables.clan_table[pRoom.clan].name);
                buf1 += buf;
            }

            if (!Bit.IS_NULLSTR(pRoom.owner))
            {
                buf = RomString.sprintf("Owner     : [%s]\n\r", pRoom.owner);
                buf1 += buf;
            }

            if (pRoom.extra_descr != null)
            {
                buf1 += "Desc Kwds:  [";
                for (var ed = pRoom.extra_descr; ed != null; ed = ed.next)
                {
                    buf1 += ed.keyword;
                    if (ed.next != null)
                        buf1 += " ";
                }
                buf1 += "]\n\r";
            }

            buf1 += "Characters: [";
            bool fcnt = false;
            for (var rch = pRoom.people; rch != null; rch = rch.next_in_room)
            {
                RomString.one_argument(rch.name, out buf);
                buf1 += buf;
                buf1 += " ";
                fcnt = true;
            }

            if (fcnt)
            {
                buf1 = buf1.Substring(0, buf1.Length - 1) + "]";
                buf1 += "\n\r";
            }
            else
                buf1 += "none]\n\r";

            buf1 += "Objects:    [";
            fcnt = false;
            for (var obj = pRoom.contents; obj != null; obj = obj.next_content)
            {
                RomString.one_argument(obj.name, out buf);
                buf1 += buf;
                buf1 += " ";
                fcnt = true;
            }

            if (fcnt)
            {
                buf1 = buf1.Substring(0, buf1.Length - 1) + "]";
                buf1 += "\n\r";
            }
            else
                buf1 += "none]\n\r";

            for (int door = 0; door < MAX_DIR; door++)
            {
                ExitData pexit;
                if ((pexit = pRoom.exit[door]) != null)
                {
                    buf = RomString.sprintf("-%-5s to [%5d] Key: [%5d] ",
                        RomString.capitalize(ActMove.dir_name[door]),
                        pexit.to_room != null ? pexit.to_room.vnum : 0,
                        pexit.key);
                    buf1 += buf;

                    string reset_state = Lookup.flag_string(Tables.exit_flags, pexit.rs_flags);
                    string state = Lookup.flag_string(Tables.exit_flags, pexit.exit_info);
                    buf1 += " Exit flags: [";
                    for (;;)
                    {
                        state = RomString.one_argument(state, out string word);

                        if (word.Length == 0)
                        {
                            buf1 = buf1.Substring(0, buf1.Length - 1) + "]";
                            buf1 += "\n\r";
                            break;
                        }

                        if (RomString.str_infix(word, reset_state))
                        {
                            char[] wc = word.ToCharArray();
                            for (int i = 0; i < wc.Length; i++)
                                wc[i] = Bit.UPPER(wc[i]);
                            word = new string(wc);
                        }
                        buf1 += word;
                        buf1 += " ";
                    }

                    if (!Bit.IS_NULLSTR(pexit.keyword))
                    {
                        buf = RomString.sprintf("Kwds: [%s]\n\r", pexit.keyword);
                        buf1 += buf;
                    }
                    if (!Bit.IS_NULLSTR(pexit.description))
                    {
                        buf = RomString.sprintf("%s", pexit.description);
                        buf1 += buf;
                    }
                }
            }

            Comm.send_to_char(buf1, ch);
            return false;
        }

        static bool change_exit(CharData ch, string argument, int door)
        {
            argument ??= "";
            var pRoom = ch.in_room;
            int value;

            if ((value = Lookup.flag_value(Tables.exit_flags, argument)) != NO_FLAG)
            {
                if (pRoom.exit[door] == null)
                {
                    Comm.send_to_char("Exit doesn't exist.\n\r", ch);
                    return false;
                }

                Bit.TOGGLE_BIT(ref pRoom.exit[door].rs_flags, value);
                pRoom.exit[door].exit_info = pRoom.exit[door].rs_flags;

                var pToRoom = pRoom.exit[door].to_room;
                int rev = ActMove.rev_dir[door];

                if (pToRoom.exit[rev] != null)
                {
                    pToRoom.exit[rev].rs_flags = pRoom.exit[door].rs_flags;
                    pToRoom.exit[rev].exit_info = pRoom.exit[door].exit_info;
                }

                Comm.send_to_char("Exit flag toggled.\n\r", ch);
                return true;
            }

            argument = RomString.one_argument(argument, out string command);
            RomString.one_argument(argument, out string arg);

            if (command.Length == 0 && argument.Length == 0)
            {
                ActMove.move_char(ch, door, true);
                return false;
            }

            if (command.Length != 0 && command[0] == '?')
            {
                Interp.do_help(ch, "EXIT");
                return false;
            }

            if (!RomString.str_cmp(command, "delete"))
            {
                if (pRoom.exit[door] == null)
                {
                    Comm.send_to_char("REdit:  Cannot delete a null exit.\n\r", ch);
                    return false;
                }

                int rev = ActMove.rev_dir[door];
                var pToRoom = pRoom.exit[door].to_room;

                if (pToRoom.exit[rev] != null)
                {
                    Recycle.free_exit(pToRoom.exit[rev]);
                    pToRoom.exit[rev] = null;
                }

                Recycle.free_exit(pRoom.exit[door]);
                pRoom.exit[door] = null;

                Comm.send_to_char("Exit unlinked.\n\r", ch);
                return true;
            }

            if (!RomString.str_cmp(command, "link"))
            {
                if (arg.Length == 0 || !Interp.is_number(arg))
                {
                    Comm.send_to_char("Syntax:  [direction] link [vnum]\n\r", ch);
                    return false;
                }

                value = Interp.atoi(arg);

                RoomIndexData toRoom;
                if ((toRoom = Handler.get_room_index(value)) == null)
                {
                    Comm.send_to_char("REdit:  Cannot link to non-existant room.\n\r",
                        ch);
                    return false;
                }

                if (!Bit.IS_BUILDER(ch, toRoom.area))
                {
                    Comm.send_to_char("REdit:  Cannot link to that area.\n\r", ch);
                    return false;
                }

                if (toRoom.exit[ActMove.rev_dir[door]] != null)
                {
                    Comm.send_to_char("REdit:  Remote side's exit already exists.\n\r",
                        ch);
                    return false;
                }

                if (pRoom.exit[door] == null)
                    pRoom.exit[door] = Recycle.new_exit();

                pRoom.exit[door].to_room = toRoom;
                pRoom.exit[door].orig_door = door;

                door = ActMove.rev_dir[door];
                var pExit = Recycle.new_exit();
                pExit.to_room = pRoom;
                pExit.orig_door = door;
                toRoom.exit[door] = pExit;

                Comm.send_to_char("Two-way link established.\n\r", ch);
                return true;
            }

            if (!RomString.str_cmp(command, "dig"))
            {
                if (arg.Length == 0 || !Interp.is_number(arg))
                {
                    Comm.send_to_char("Syntax: [direction] dig <vnum>\n\r", ch);
                    return false;
                }

                redit_create(ch, arg);
                string buf = RomString.sprintf("link %s", arg);
                change_exit(ch, buf, door);
                return true;
            }

            if (!RomString.str_cmp(command, "room"))
            {
                if (arg.Length == 0 || !Interp.is_number(arg))
                {
                    Comm.send_to_char("Syntax:  [direction] room [vnum]\n\r", ch);
                    return false;
                }

                value = Interp.atoi(arg);

                RoomIndexData toRoom;
                if ((toRoom = Handler.get_room_index(value)) == null)
                {
                    Comm.send_to_char("REdit:  Cannot link to non-existant room.\n\r",
                        ch);
                    return false;
                }

                if (pRoom.exit[door] == null)
                    pRoom.exit[door] = Recycle.new_exit();

                pRoom.exit[door].to_room = toRoom;
                pRoom.exit[door].orig_door = door;

                Comm.send_to_char("One-way link established.\n\r", ch);
                return true;
            }

            if (!RomString.str_cmp(command, "key"))
            {
                if (arg.Length == 0 || !Interp.is_number(arg))
                {
                    Comm.send_to_char("Syntax:  [direction] key [vnum]\n\r", ch);
                    return false;
                }

                if (pRoom.exit[door] == null)
                {
                    Comm.send_to_char("Exit doesn't exist.\n\r", ch);
                    return false;
                }

                value = Interp.atoi(arg);

                ObjIndexData key;
                if ((key = Handler.get_obj_index(value)) == null)
                {
                    Comm.send_to_char("REdit:  Key doesn't exist.\n\r", ch);
                    return false;
                }

                if (key.item_type != ITEM_KEY)
                {
                    Comm.send_to_char("REdit:  Object is not a key.\n\r", ch);
                    return false;
                }

                pRoom.exit[door].key = value;

                Comm.send_to_char("Exit key set.\n\r", ch);
                return true;
            }

            if (!RomString.str_cmp(command, "name"))
            {
                if (arg.Length == 0)
                {
                    Comm.send_to_char("Syntax:  [direction] name [string]\n\r", ch);
                    Comm.send_to_char("         [direction] name none\n\r", ch);
                    return false;
                }

                if (pRoom.exit[door] == null)
                {
                    Comm.send_to_char("Exit doesn't exist.\n\r", ch);
                    return false;
                }

                if (RomString.str_cmp(arg, "none"))
                    pRoom.exit[door].keyword = arg;
                else
                    pRoom.exit[door].keyword = "";

                Comm.send_to_char("Exit name set.\n\r", ch);
                return true;
            }

            if (!RomString.str_prefix(command, "description"))
            {
                if (arg.Length == 0)
                {
                    if (pRoom.exit[door] == null)
                    {
                        Comm.send_to_char("Exit doesn't exist.\n\r", ch);
                        return false;
                    }

                    var pexit = pRoom.exit[door];
                    RomString.string_append(ch, new StringPtr(() => pexit.description, v => pexit.description = v));
                    return true;
                }

                Comm.send_to_char("Syntax:  [direction] desc\n\r", ch);
                return false;
            }

            return false;
        }

        public static bool redit_north(CharData ch, string argument)
        {
            argument ??= "";
            if (change_exit(ch, argument, DIR_NORTH))
                return true;

            return false;
        }

        public static bool redit_south(CharData ch, string argument)
        {
            argument ??= "";
            if (change_exit(ch, argument, DIR_SOUTH))
                return true;

            return false;
        }

        public static bool redit_east(CharData ch, string argument)
        {
            argument ??= "";
            if (change_exit(ch, argument, DIR_EAST))
                return true;

            return false;
        }

        public static bool redit_west(CharData ch, string argument)
        {
            argument ??= "";
            if (change_exit(ch, argument, DIR_WEST))
                return true;

            return false;
        }

        public static bool redit_up(CharData ch, string argument)
        {
            argument ??= "";
            if (change_exit(ch, argument, DIR_UP))
                return true;

            return false;
        }

        public static bool redit_down(CharData ch, string argument)
        {
            argument ??= "";
            if (change_exit(ch, argument, DIR_DOWN))
                return true;

            return false;
        }

        public static bool redit_ed(CharData ch, string argument)
        {
            argument ??= "";
            var pRoom = ch.in_room;

            argument = RomString.one_argument(argument, out string command);
            RomString.one_argument(argument, out string keyword);

            if (command.Length == 0 || keyword.Length == 0)
            {
                Comm.send_to_char("Syntax:  ed add [keyword]\n\r", ch);
                Comm.send_to_char("         ed edit [keyword]\n\r", ch);
                Comm.send_to_char("         ed delete [keyword]\n\r", ch);
                Comm.send_to_char("         ed format [keyword]\n\r", ch);
                return false;
            }

            if (!RomString.str_cmp(command, "add"))
            {
                if (keyword.Length == 0)
                {
                    Comm.send_to_char("Syntax:  ed add [keyword]\n\r", ch);
                    return false;
                }

                var ed = Recycle.new_extra_descr();
                ed.keyword = keyword;
                ed.description = "";
                ed.next = pRoom.extra_descr;
                pRoom.extra_descr = ed;

                RomString.string_append(ch, new StringPtr(() => ed.description, v => ed.description = v));

                return true;
            }

            if (!RomString.str_cmp(command, "edit"))
            {
                if (keyword.Length == 0)
                {
                    Comm.send_to_char("Syntax:  ed edit [keyword]\n\r", ch);
                    return false;
                }

                ExtraDescrData ed;
                for (ed = pRoom.extra_descr; ed != null; ed = ed.next)
                {
                    if (Handler.is_name(keyword, ed.keyword))
                        break;
                }

                if (ed == null)
                {
                    Comm.send_to_char("REdit:  Extra description keyword not found.\n\r",
                        ch);
                    return false;
                }

                RomString.string_append(ch, new StringPtr(() => ed.description, v => ed.description = v));

                return true;
            }

            if (!RomString.str_cmp(command, "delete"))
            {
                ExtraDescrData ped = null;

                if (keyword.Length == 0)
                {
                    Comm.send_to_char("Syntax:  ed delete [keyword]\n\r", ch);
                    return false;
                }

                ExtraDescrData ed;
                for (ed = pRoom.extra_descr; ed != null; ed = ed.next)
                {
                    if (Handler.is_name(keyword, ed.keyword))
                        break;
                    ped = ed;
                }

                if (ed == null)
                {
                    Comm.send_to_char("REdit:  Extra description keyword not found.\n\r",
                        ch);
                    return false;
                }

                if (ped == null)
                    pRoom.extra_descr = ed.next;
                else
                    ped.next = ed.next;

                Recycle.free_extra_descr(ed);

                Comm.send_to_char("Extra description deleted.\n\r", ch);
                return true;
            }

            if (!RomString.str_cmp(command, "format"))
            {
                if (keyword.Length == 0)
                {
                    Comm.send_to_char("Syntax:  ed format [keyword]\n\r", ch);
                    return false;
                }

                ExtraDescrData ed;
                for (ed = pRoom.extra_descr; ed != null; ed = ed.next)
                {
                    if (Handler.is_name(keyword, ed.keyword))
                        break;
                }

                if (ed == null)
                {
                    Comm.send_to_char("REdit:  Extra description keyword not found.\n\r",
                        ch);
                    return false;
                }

                ed.description = RomString.format_string(ed.description);

                Comm.send_to_char("Extra description formatted.\n\r", ch);
                return true;
            }

            redit_ed(ch, "");
            return false;
        }

        public static bool redit_name(CharData ch, string argument)
        {
            argument ??= "";
            var pRoom = ch.in_room;

            if (argument.Length == 0)
            {
                Comm.send_to_char("Syntax:  name [name]\n\r", ch);
                return false;
            }

            pRoom.name = argument;

            Comm.send_to_char("Name set.\n\r", ch);
            return true;
        }

        public static bool redit_desc(CharData ch, string argument)
        {
            argument ??= "";
            var pRoom = ch.in_room;

            if (argument.Length == 0)
            {
                RomString.string_append(ch, new StringPtr(() => pRoom.description, v => pRoom.description = v));
                return true;
            }

            Comm.send_to_char("Syntax:  desc\n\r", ch);
            return false;
        }

        public static bool redit_heal(CharData ch, string argument)
        {
            argument ??= "";
            var pRoom = ch.in_room;

            if (Interp.is_number(argument))
            {
                pRoom.heal_rate = Interp.atoi(argument);
                Comm.send_to_char("Heal rate set.\n\r", ch);
                return true;
            }

            Comm.send_to_char("Syntax: heal <#xnumber>\n\r", ch);
            return false;
        }

        public static bool redit_mana(CharData ch, string argument)
        {
            argument ??= "";
            var pRoom = ch.in_room;

            if (Interp.is_number(argument))
            {
                pRoom.mana_rate = Interp.atoi(argument);
                Comm.send_to_char("Mana rate set.\n\r", ch);
                return true;
            }

            Comm.send_to_char("Syntax: mana <#xnumber>\n\r", ch);
            return false;
        }

        public static bool redit_clan(CharData ch, string argument)
        {
            argument ??= "";
            var pRoom = ch.in_room;

            pRoom.clan = Lookup.clan_lookup(argument);

            Comm.send_to_char("Clan set.\n\r", ch);
            return true;
        }

        public static bool redit_format(CharData ch, string argument)
        {
            argument ??= "";
            var pRoom = ch.in_room;

            pRoom.description = RomString.format_string(pRoom.description);

            Comm.send_to_char("String formatted.\n\r", ch);
            return true;
        }

        public static bool redit_mreset(CharData ch, string argument)
        {
            argument ??= "";
            var pRoom = ch.in_room;

            argument = RomString.one_argument(argument, out string arg);
            argument = RomString.one_argument(argument, out string arg2);

            if (arg.Length == 0 || !Interp.is_number(arg))
            {
                Comm.send_to_char("Syntax:  mreset <vnum> <max #x> <mix #x>\n\r", ch);
                return false;
            }

            MobIndexData pMobIndex;
            if ((pMobIndex = Handler.get_mob_index(Interp.atoi(arg))) == null)
            {
                Comm.send_to_char("REdit: No mobile has that vnum.\n\r", ch);
                return false;
            }

            if (pMobIndex.area != pRoom.area)
            {
                Comm.send_to_char("REdit: No such mobile in this area.\n\r", ch);
                return false;
            }

            var pReset = Recycle.new_reset_data();
            pReset.command = 'M';
            pReset.arg1 = pMobIndex.vnum;
            pReset.arg2 = Interp.is_number(arg2) ? Interp.atoi(arg2) : MAX_MOB;
            pReset.arg3 = pRoom.vnum;
            pReset.arg4 = Interp.is_number(argument) ? Interp.atoi(argument) : 1;
            add_reset(pRoom, pReset, 0);

            var newmob = Db.create_mobile(pMobIndex);
            Handler.char_to_room(newmob, pRoom);

            string output = RomString.sprintf("%s (%d) has been loaded and added to resets.\n\r"
                + "There will be a maximum of %d loaded to this room.\n\r",
                RomString.capitalize(pMobIndex.short_descr),
                pMobIndex.vnum, pReset.arg2);
            Comm.send_to_char(output, ch);
            Comm.act("$n has created $N!", ch, null, newmob, TO_ROOM);
            return true;
        }

        static readonly (int wear_loc, long wear_bit)[] wear_table =
        {
            (WEAR_NONE, ITEM_TAKE),
            (WEAR_LIGHT, ITEM_LIGHT),
            (WEAR_FINGER_L, ITEM_WEAR_FINGER),
            (WEAR_FINGER_R, ITEM_WEAR_FINGER),
            (WEAR_NECK_1, ITEM_WEAR_NECK),
            (WEAR_NECK_2, ITEM_WEAR_NECK),
            (WEAR_BODY, ITEM_WEAR_BODY),
            (WEAR_HEAD, ITEM_WEAR_HEAD),
            (WEAR_LEGS, ITEM_WEAR_LEGS),
            (WEAR_FEET, ITEM_WEAR_FEET),
            (WEAR_HANDS, ITEM_WEAR_HANDS),
            (WEAR_ARMS, ITEM_WEAR_ARMS),
            (WEAR_SHIELD, ITEM_WEAR_SHIELD),
            (WEAR_ABOUT, ITEM_WEAR_ABOUT),
            (WEAR_WAIST, ITEM_WEAR_WAIST),
            (WEAR_WRIST_L, ITEM_WEAR_WRIST),
            (WEAR_WRIST_R, ITEM_WEAR_WRIST),
            (WEAR_WIELD, ITEM_WIELD),
            (WEAR_HOLD, ITEM_HOLD),
            (NO_FLAG, NO_FLAG)
        };

        static int wear_loc(int bits, int count)
        {
            for (int flag = 0; wear_table[flag].wear_bit != NO_FLAG; flag++)
            {
                if (Bit.IS_SET(bits, wear_table[flag].wear_bit) && --count < 1)
                    return wear_table[flag].wear_loc;
            }

            return NO_FLAG;
        }

        static int wear_bit(int loc)
        {
            for (int flag = 0; wear_table[flag].wear_loc != NO_FLAG; flag++)
            {
                if (loc == wear_table[flag].wear_loc)
                    return (int)wear_table[flag].wear_bit;
            }

            return 0;
        }

        public static bool redit_oreset(CharData ch, string argument)
        {
            argument ??= "";
            var pRoom = ch.in_room;
            int olevel = 0;
            ObjData newobj = null;

            argument = RomString.one_argument(argument, out string arg1);
            argument = RomString.one_argument(argument, out string arg2);

            if (arg1.Length == 0 || !Interp.is_number(arg1))
            {
                Comm.send_to_char("Syntax:  oreset <vnum> <args>\n\r", ch);
                Comm.send_to_char("        -no_args               = into room\n\r", ch);
                Comm.send_to_char("        -<obj_name>            = into obj\n\r", ch);
                Comm.send_to_char("        -<mob_name> <wear_loc> = into mob\n\r", ch);
                return false;
            }

            ObjIndexData pObjIndex;
            if ((pObjIndex = Handler.get_obj_index(Interp.atoi(arg1))) == null)
            {
                Comm.send_to_char("REdit: No object has that vnum.\n\r", ch);
                return false;
            }

            if (pObjIndex.area != pRoom.area)
            {
                Comm.send_to_char("REdit: No such object in this area.\n\r", ch);
                return false;
            }

            if (arg2.Length == 0)
            {
                var pReset = Recycle.new_reset_data();
                pReset.command = 'O';
                pReset.arg1 = pObjIndex.vnum;
                pReset.arg2 = 0;
                pReset.arg3 = pRoom.vnum;
                pReset.arg4 = 0;
                add_reset(pRoom, pReset, 0);

                newobj = Db.create_object(pObjIndex, RomRandom.number_fuzzy(olevel));
                Handler.obj_to_room(newobj, pRoom);

                string output = RomString.sprintf("%s (%d) has been loaded and added to resets.\n\r",
                    RomString.capitalize(pObjIndex.short_descr), pObjIndex.vnum);
                Comm.send_to_char(output, ch);
            }
            else if (argument.Length == 0
                && Handler.get_obj_list(ch, arg2, pRoom.contents) is ObjData to_obj)
            {
                var pReset = Recycle.new_reset_data();
                pReset.command = 'P';
                pReset.arg1 = pObjIndex.vnum;
                pReset.arg2 = 0;
                pReset.arg3 = to_obj.pIndexData.vnum;
                pReset.arg4 = 1;
                add_reset(pRoom, pReset, 0);

                newobj = Db.create_object(pObjIndex, RomRandom.number_fuzzy(olevel));
                newobj.cost = 0;
                Handler.obj_to_obj(newobj, to_obj);

                string output = RomString.sprintf("%s (%d) has been loaded into "
                    + "%s (%d) and added to resets.\n\r",
                    RomString.capitalize(newobj.short_descr),
                    newobj.pIndexData.vnum,
                    to_obj.short_descr, to_obj.pIndexData.vnum);
                Comm.send_to_char(output, ch);
            }
            else if (Handler.get_char_room(ch, arg2) is CharData to_mob)
            {
                int wear_loc;

                if ((wear_loc = Lookup.flag_value(Tables.wear_loc_flags, argument)) == NO_FLAG)
                {
                    Comm.send_to_char("REdit: Invalid wear_loc.  '? wear-loc'\n\r", ch);
                    return false;
                }

                if (!Bit.IS_SET(pObjIndex.wear_flags, wear_bit(wear_loc)))
                {
                    string output = RomString.sprintf(
                        "%s (%d) has wear flags: [%s]\n\r",
                        RomString.capitalize(pObjIndex.short_descr),
                        pObjIndex.vnum,
                        Lookup.flag_string(Tables.wear_flags, pObjIndex.wear_flags));
                    Comm.send_to_char(output, ch);
                    return false;
                }

                if (Handler.get_eq_char(to_mob, wear_loc) != null)
                {
                    Comm.send_to_char("REdit:  Object already equipped.\n\r", ch);
                    return false;
                }

                var pReset = Recycle.new_reset_data();
                pReset.arg1 = pObjIndex.vnum;
                pReset.arg2 = wear_loc;
                if (pReset.arg2 == WEAR_NONE)
                    pReset.command = 'G';
                else
                    pReset.command = 'E';
                pReset.arg3 = wear_loc;

                add_reset(pRoom, pReset, 0);

                olevel = Bit.URANGE(0, to_mob.level - 2, LEVEL_HERO);
                newobj = Db.create_object(pObjIndex, RomRandom.number_fuzzy(olevel));

                if (to_mob.pIndexData.pShop != null)
                {
                    switch (pObjIndex.item_type)
                    {
                        default:
                            olevel = 0;
                            break;
                        case ITEM_PILL:
                            olevel = RomRandom.number_range(0, 10);
                            break;
                        case ITEM_POTION:
                            olevel = RomRandom.number_range(0, 10);
                            break;
                        case ITEM_SCROLL:
                            olevel = RomRandom.number_range(5, 15);
                            break;
                        case ITEM_WAND:
                            olevel = RomRandom.number_range(10, 20);
                            break;
                        case ITEM_STAFF:
                            olevel = RomRandom.number_range(15, 25);
                            break;
                        case ITEM_ARMOR:
                            olevel = RomRandom.number_range(5, 15);
                            break;
                        case ITEM_WEAPON:
                            if (pReset.command == 'G')
                                olevel = RomRandom.number_range(5, 15);
                            else
                                olevel = RomRandom.number_fuzzy(olevel);
                            break;
                    }

                    newobj = Db.create_object(pObjIndex, olevel);
                    if (pReset.arg2 == WEAR_NONE)
                        Bit.SET_BIT(ref newobj.extra_flags, ITEM_INVENTORY);
                }
                else
                    newobj = Db.create_object(pObjIndex, RomRandom.number_fuzzy(olevel));

                Handler.obj_to_char(newobj, to_mob);
                if (pReset.command == 'E')
                    Handler.equip_char(to_mob, newobj, pReset.arg3);

                string output2 = RomString.sprintf("%s (%d) has been loaded "
                    + "%s of %s (%d) and added to resets.\n\r",
                    RomString.capitalize(pObjIndex.short_descr),
                    pObjIndex.vnum,
                    Lookup.flag_string(Tables.wear_loc_strings, pReset.arg3),
                    to_mob.short_descr, to_mob.pIndexData.vnum);
                Comm.send_to_char(output2, ch);
            }
            else
            {
                Comm.send_to_char("REdit:  That mobile isn't here.\n\r", ch);
                return false;
            }

            Comm.act("$n has created $p!", ch, newobj, null, TO_ROOM);
            return true;
        }

        public static bool redit_owner(CharData ch, string argument)
        {
            argument ??= "";
            var pRoom = ch.in_room;

            if (argument.Length == 0)
            {
                Comm.send_to_char("Syntax:  owner [owner]\n\r", ch);
                Comm.send_to_char("         owner none\n\r", ch);
                return false;
            }

            if (!RomString.str_cmp(argument, "none"))
                pRoom.owner = "";
            else
                pRoom.owner = argument;

            Comm.send_to_char("Owner set.\n\r", ch);
            return true;
        }

        public static bool redit_room(CharData ch, string argument)
        {
            argument ??= "";
            var room = ch.in_room;
            int value;

            if ((value = Lookup.flag_value(Tables.room_flags, argument)) == NO_FLAG)
            {
                Comm.send_to_char("Sintaxis: room [flags]\n\r", ch);
                return false;
            }

            Bit.TOGGLE_BIT(ref room.room_flags, value);
            Comm.send_to_char("Room flags toggled.\n\r", ch);
            return true;
        }

        public static bool redit_sector(CharData ch, string argument)
        {
            argument ??= "";
            var room = ch.in_room;
            int value;

            if ((value = Lookup.flag_value(Tables.sector_flags, argument)) == NO_FLAG)
            {
                Comm.send_to_char("Sintaxis: sector [tipo]\n\r", ch);
                return false;
            }

            room.sector_type = value;
            Comm.send_to_char("Sector type set.\n\r", ch);

            return true;
        }

        public static void show_liqlist(CharData ch)
        {
            string buffer = "";

            for (int liq = 0; Tables.liq_table[liq].liq_name != null; liq++)
            {
                if ((liq % 21) == 0)
                    buffer += "Name                 Color          Proof Full Thirst Food Ssize\n\r";

                string buf = RomString.sprintf("%-20s %-14s %5d %4d %6d %4d %5d\n\r",
                    Tables.liq_table[liq].liq_name, Tables.liq_table[liq].liq_color,
                    Tables.liq_table[liq].liq_affect[0], Tables.liq_table[liq].liq_affect[1],
                    Tables.liq_table[liq].liq_affect[2], Tables.liq_table[liq].liq_affect[3],
                    Tables.liq_table[liq].liq_affect[4]);
                buffer += buf;
            }

            Comm.page_to_char(buffer, ch);

            return;
        }

        public static void show_damlist(CharData ch)
        {
            string buffer = "";

            for (int att = 0; Tables.attack_table[att].name != null; att++)
            {
                if ((att % 21) == 0)
                    buffer += "Name                 Noun\n\r";

                string buf = RomString.sprintf("%-20s %-20s\n\r",
                    Tables.attack_table[att].name, Tables.attack_table[att].noun);
                buffer += buf;
            }

            Comm.page_to_char(buffer, ch);

            return;
        }
    }
}
