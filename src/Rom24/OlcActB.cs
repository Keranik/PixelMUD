using static Rom24.Merc;

namespace Rom24
{
    public static partial class Olc
    {
        static void show_obj_values(CharData ch, ObjIndexData obj)
        {
            string buf;

            switch (obj.item_type)
            {
                default: /* No values. */
                    break;

                case ITEM_LIGHT:
                    if (obj.value[2] == -1 || obj.value[2] == 999) /* ROM OLC */
                        buf = RomString.sprintf("[v2] Light:  Infinite[-1]\n\r");
                    else
                        buf = RomString.sprintf("[v2] Light:  [%d]\n\r", obj.value[2]);
                    Comm.send_to_char(buf, ch);
                    break;

                case ITEM_WAND:
                case ITEM_STAFF:
                    buf = RomString.sprintf(
                        "[v0] Level:          [%d]\n\r" +
                        "[v1] Charges Total:  [%d]\n\r" +
                        "[v2] Charges Left:   [%d]\n\r" +
                        "[v3] Spell:          %s\n\r",
                        obj.value[0],
                        obj.value[1],
                        obj.value[2],
                        obj.value[3] != -1 ? Tables.skill_table[obj.value[3]].name
                        : "none");
                    Comm.send_to_char(buf, ch);
                    break;

                case ITEM_PORTAL:
                    buf = RomString.sprintf(
                        "[v0] Charges:        [%d]\n\r" +
                        "[v1] Exit Flags:     %s\n\r" +
                        "[v2] Portal Flags:   %s\n\r" +
                        "[v3] Goes to (vnum): [%d]\n\r",
                        obj.value[0],
                        Lookup.flag_string(Tables.exit_flags, obj.value[1]),
                        Lookup.flag_string(Tables.portal_flags, obj.value[2]),
                        obj.value[3]);
                    Comm.send_to_char(buf, ch);
                    break;

                case ITEM_FURNITURE:
                    buf = RomString.sprintf(
                        "[v0] Max people:      [%d]\n\r" +
                        "[v1] Max weight:      [%d]\n\r" +
                        "[v2] Furniture Flags: %s\n\r" +
                        "[v3] Heal bonus:      [%d]\n\r" +
                        "[v4] Mana bonus:      [%d]\n\r",
                        obj.value[0],
                        obj.value[1],
                        Lookup.flag_string(Tables.furniture_flags, obj.value[2]),
                        obj.value[3], obj.value[4]);
                    Comm.send_to_char(buf, ch);
                    break;

                case ITEM_SCROLL:
                case ITEM_POTION:
                case ITEM_PILL:
                    buf = RomString.sprintf(
                        "[v0] Level:  [%d]\n\r" +
                        "[v1] Spell:  %s\n\r" +
                        "[v2] Spell:  %s\n\r" +
                        "[v3] Spell:  %s\n\r" +
                        "[v4] Spell:  %s\n\r",
                        obj.value[0],
                        obj.value[1] != -1 ? Tables.skill_table[obj.value[1]].name
                        : "none",
                        obj.value[2] != -1 ? Tables.skill_table[obj.value[2]].name
                        : "none",
                        obj.value[3] != -1 ? Tables.skill_table[obj.value[3]].name
                        : "none",
                        obj.value[4] != -1 ? Tables.skill_table[obj.value[4]].name
                        : "none");
                    Comm.send_to_char(buf, ch);
                    break;

                /* ARMOR for ROM */
                case ITEM_ARMOR:
                    buf = RomString.sprintf(
                        "[v0] Ac pierce       [%d]\n\r" +
                        "[v1] Ac bash         [%d]\n\r" +
                        "[v2] Ac slash        [%d]\n\r" +
                        "[v3] Ac exotic       [%d]\n\r",
                        obj.value[0], obj.value[1], obj.value[2],
                        obj.value[3]);
                    Comm.send_to_char(buf, ch);
                    break;

                /* WEAPON changed in ROM */
                case ITEM_WEAPON:
                    buf = RomString.sprintf("[v0] Weapon class:   %s\n\r",
                        Lookup.flag_string(Tables.weapon_class, obj.value[0]));
                    Comm.send_to_char(buf, ch);
                    buf = RomString.sprintf("[v1] Number of dice: [%d]\n\r", obj.value[1]);
                    Comm.send_to_char(buf, ch);
                    buf = RomString.sprintf("[v2] Type of dice:   [%d]\n\r", obj.value[2]);
                    Comm.send_to_char(buf, ch);
                    buf = RomString.sprintf("[v3] Type:           %s\n\r",
                        Tables.attack_table[obj.value[3]].name);
                    Comm.send_to_char(buf, ch);
                    buf = RomString.sprintf("[v4] Special type:   %s\n\r",
                        Lookup.flag_string(Tables.weapon_type2, obj.value[4]));
                    Comm.send_to_char(buf, ch);
                    break;

                case ITEM_CONTAINER:
                    buf = RomString.sprintf(
                        "[v0] Weight:     [%d kg]\n\r" +
                        "[v1] Flags:      [%s]\n\r" +
                        "[v2] Key:     %s [%d]\n\r" +
                        "[v3] Capacity    [%d]\n\r" +
                        "[v4] Weight Mult [%d]\n\r",
                        obj.value[0],
                        Lookup.flag_string(Tables.container_flags, obj.value[1]),
                        Handler.get_obj_index(obj.value[2]) != null
                        ? Handler.get_obj_index(obj.value[2]).short_descr
                        : "none", obj.value[2], obj.value[3], obj.value[4]);
                    Comm.send_to_char(buf, ch);
                    break;

                case ITEM_DRINK_CON:
                    buf = RomString.sprintf(
                        "[v0] Liquid Total: [%d]\n\r" +
                        "[v1] Liquid Left:  [%d]\n\r" +
                        "[v2] Liquid:       %s\n\r" +
                        "[v3] Poisoned:     %s\n\r",
                        obj.value[0],
                        obj.value[1],
                        Tables.liq_table[obj.value[2]].liq_name,
                        obj.value[3] != 0 ? "Yes" : "No");
                    Comm.send_to_char(buf, ch);
                    break;

                case ITEM_FOUNTAIN:
                    buf = RomString.sprintf(
                        "[v0] Liquid Total: [%d]\n\r" +
                        "[v1] Liquid Left:  [%d]\n\r" +
                        "[v2] Liquid:        %s\n\r",
                        obj.value[0],
                        obj.value[1], Tables.liq_table[obj.value[2]].liq_name);
                    Comm.send_to_char(buf, ch);
                    break;

                case ITEM_FOOD:
                    buf = RomString.sprintf(
                        "[v0] Food hours: [%d]\n\r" +
                        "[v1] Full hours: [%d]\n\r" +
                        "[v3] Poisoned:   %s\n\r",
                        obj.value[0],
                        obj.value[1], obj.value[3] != 0 ? "Yes" : "No");
                    Comm.send_to_char(buf, ch);
                    break;

                case ITEM_MONEY:
                    buf = RomString.sprintf("[v0] Gold:   [%d]\n\r", obj.value[0]);
                    Comm.send_to_char(buf, ch);
                    break;
            }

            return;
        }

        static bool set_obj_values(CharData ch, ObjIndexData pObj, int value_num,
            string argument)
        {
            argument ??= "";
            switch (pObj.item_type)
            {
                default:
                    break;

                case ITEM_LIGHT:
                    switch (value_num)
                    {
                        default:
                            Interp.do_help(ch, "ITEM_LIGHT");
                            return false;
                        case 2:
                            Comm.send_to_char("HOURS OF LIGHT SET.\n\r\n\r", ch);
                            pObj.value[2] = Interp.atoi(argument);
                            break;
                    }
                    break;

                case ITEM_WAND:
                case ITEM_STAFF:
                    switch (value_num)
                    {
                        default:
                            Interp.do_help(ch, "ITEM_STAFF_WAND");
                            return false;
                        case 0:
                            Comm.send_to_char("SPELL LEVEL SET.\n\r\n\r", ch);
                            pObj.value[0] = Interp.atoi(argument);
                            break;
                        case 1:
                            Comm.send_to_char("TOTAL NUMBER OF CHARGES SET.\n\r\n\r", ch);
                            pObj.value[1] = Interp.atoi(argument);
                            break;
                        case 2:
                            Comm.send_to_char("CURRENT NUMBER OF CHARGES SET.\n\r\n\r",
                                ch);
                            pObj.value[2] = Interp.atoi(argument);
                            break;
                        case 3:
                            Comm.send_to_char("SPELL TYPE SET.\n\r", ch);
                            pObj.value[3] = Lookup.skill_lookup(argument);
                            break;
                    }
                    break;

                case ITEM_SCROLL:
                case ITEM_POTION:
                case ITEM_PILL:
                    switch (value_num)
                    {
                        default:
                            Interp.do_help(ch, "ITEM_SCROLL_POTION_PILL");
                            return false;
                        case 0:
                            Comm.send_to_char("SPELL LEVEL SET.\n\r\n\r", ch);
                            pObj.value[0] = Interp.atoi(argument);
                            break;
                        case 1:
                            Comm.send_to_char("SPELL TYPE 1 SET.\n\r\n\r", ch);
                            pObj.value[1] = Lookup.skill_lookup(argument);
                            break;
                        case 2:
                            Comm.send_to_char("SPELL TYPE 2 SET.\n\r\n\r", ch);
                            pObj.value[2] = Lookup.skill_lookup(argument);
                            break;
                        case 3:
                            Comm.send_to_char("SPELL TYPE 3 SET.\n\r\n\r", ch);
                            pObj.value[3] = Lookup.skill_lookup(argument);
                            break;
                        case 4:
                            Comm.send_to_char("SPELL TYPE 4 SET.\n\r\n\r", ch);
                            pObj.value[4] = Lookup.skill_lookup(argument);
                            break;
                    }
                    break;

                /* ARMOR for ROM: */
                case ITEM_ARMOR:
                    switch (value_num)
                    {
                        default:
                            Interp.do_help(ch, "ITEM_ARMOR");
                            return false;
                        case 0:
                            Comm.send_to_char("AC PIERCE SET.\n\r\n\r", ch);
                            pObj.value[0] = Interp.atoi(argument);
                            break;
                        case 1:
                            Comm.send_to_char("AC BASH SET.\n\r\n\r", ch);
                            pObj.value[1] = Interp.atoi(argument);
                            break;
                        case 2:
                            Comm.send_to_char("AC SLASH SET.\n\r\n\r", ch);
                            pObj.value[2] = Interp.atoi(argument);
                            break;
                        case 3:
                            Comm.send_to_char("AC EXOTIC SET.\n\r\n\r", ch);
                            pObj.value[3] = Interp.atoi(argument);
                            break;
                    }
                    break;

                /* WEAPONS changed in ROM */
                case ITEM_WEAPON:
                    switch (value_num)
                    {
                        default:
                            Interp.do_help(ch, "ITEM_WEAPON");
                            return false;
                        case 0:
                            Comm.send_to_char("WEAPON CLASS SET.\n\r\n\r", ch);
                            {
                                int blah = Lookup.flag_value(Tables.weapon_class,
                                    argument);
                                pObj.value[0] = (blah == NO_FLAG) ? 0 : blah;
                            }
                            break;
                        case 1:
                            Comm.send_to_char("NUMBER OF DICE SET.\n\r\n\r", ch);
                            pObj.value[1] = Interp.atoi(argument);
                            break;
                        case 2:
                            Comm.send_to_char("TYPE OF DICE SET.\n\r\n\r", ch);
                            pObj.value[2] = Interp.atoi(argument);
                            break;
                        case 3:
                            Comm.send_to_char("WEAPON TYPE SET.\n\r\n\r", ch);
                            pObj.value[3] = Lookup.attack_lookup(argument);
                            break;
                        case 4:
                            Comm.send_to_char("SPECIAL WEAPON TYPE TOGGLED.\n\r\n\r", ch);
                            {
                                int blah = Lookup.flag_value(Tables.weapon_type2,
                                    argument);
                                pObj.value[4] ^= (blah == NO_FLAG) ? 0 : blah;
                            }
                            break;
                    }
                    break;

                case ITEM_PORTAL:
                    switch (value_num)
                    {
                        default:
                            Interp.do_help(ch, "ITEM_PORTAL");
                            return false;

                        case 0:
                            Comm.send_to_char("CHARGES SET.\n\r\n\r", ch);
                            pObj.value[0] = Interp.atoi(argument);
                            break;
                        case 1:
                            Comm.send_to_char("EXIT FLAGS SET.\n\r\n\r", ch);
                            {
                                int blah = Lookup.flag_value(Tables.exit_flags, argument);
                                pObj.value[1] = (blah == NO_FLAG) ? 0 : blah;
                            }
                            break;
                        case 2:
                            Comm.send_to_char("PORTAL FLAGS SET.\n\r\n\r", ch);
                            {
                                int blah = Lookup.flag_value(Tables.portal_flags,
                                    argument);
                                pObj.value[2] = (blah == NO_FLAG) ? 0 : blah;
                            }
                            break;
                        case 3:
                            Comm.send_to_char("EXIT VNUM SET.\n\r\n\r", ch);
                            pObj.value[3] = Interp.atoi(argument);
                            break;
                    }
                    break;

                case ITEM_FURNITURE:
                    switch (value_num)
                    {
                        default:
                            Interp.do_help(ch, "ITEM_FURNITURE");
                            return false;

                        case 0:
                            Comm.send_to_char("NUMBER OF PEOPLE SET.\n\r\n\r", ch);
                            pObj.value[0] = Interp.atoi(argument);
                            break;
                        case 1:
                            Comm.send_to_char("MAX WEIGHT SET.\n\r\n\r", ch);
                            pObj.value[1] = Interp.atoi(argument);
                            break;
                        case 2:
                            Comm.send_to_char("FURNITURE FLAGS TOGGLED.\n\r\n\r", ch);
                            {
                                int blah = Lookup.flag_value(Tables.furniture_flags,
                                    argument);
                                pObj.value[2] ^= (blah == NO_FLAG) ? 0 : blah;
                            }
                            break;
                        case 3:
                            Comm.send_to_char("HEAL BONUS SET.\n\r\n\r", ch);
                            pObj.value[3] = Interp.atoi(argument);
                            break;
                        case 4:
                            Comm.send_to_char("MANA BONUS SET.\n\r\n\r", ch);
                            pObj.value[4] = Interp.atoi(argument);
                            break;
                    }
                    break;

                case ITEM_CONTAINER:
                    switch (value_num)
                    {
                        default:
                            Interp.do_help(ch, "ITEM_CONTAINER");
                            return false;
                        case 0:
                            Comm.send_to_char("WEIGHT CAPACITY SET.\n\r\n\r", ch);
                            pObj.value[0] = Interp.atoi(argument);
                            break;
                        case 1:
                            int value;
                            if ((value = Lookup.flag_value(Tables.container_flags, argument)) !=
                                NO_FLAG)
                                Bit.TOGGLE_BIT(ref pObj.value[1], value);
                            else
                            {
                                Interp.do_help(ch, "ITEM_CONTAINER");
                                return false;
                            }
                            Comm.send_to_char("CONTAINER TYPE SET.\n\r\n\r", ch);
                            break;
                        case 2:
                            if (Interp.atoi(argument) != 0)
                            {
                                if (Handler.get_obj_index(Interp.atoi(argument)) == null)
                                {
                                    Comm.send_to_char("THERE IS NO SUCH ITEM.\n\r\n\r",
                                        ch);
                                    return false;
                                }

                                if (Handler.get_obj_index(Interp.atoi(argument)).item_type !=
                                    ITEM_KEY)
                                {
                                    Comm.send_to_char("THAT ITEM IS NOT A KEY.\n\r\n\r",
                                        ch);
                                    return false;
                                }
                            }
                            Comm.send_to_char("CONTAINER KEY SET.\n\r\n\r", ch);
                            pObj.value[2] = Interp.atoi(argument);
                            break;
                        case 3:
                            Comm.send_to_char("CONTAINER MAX WEIGHT SET.\n\r", ch);
                            pObj.value[3] = Interp.atoi(argument);
                            break;
                        case 4:
                            Comm.send_to_char("WEIGHT MULTIPLIER SET.\n\r\n\r", ch);
                            pObj.value[4] = Interp.atoi(argument);
                            break;
                    }
                    break;

                case ITEM_DRINK_CON:
                    switch (value_num)
                    {
                        default:
                            Interp.do_help(ch, "ITEM_DRINK");
                            /* OLC            do_help( ch, "liquids" );    */
                            return false;
                        case 0:
                            Comm.send_to_char
                                ("MAXIMUM AMOUT OF LIQUID HOURS SET.\n\r\n\r", ch);
                            pObj.value[0] = Interp.atoi(argument);
                            break;
                        case 1:
                            Comm.send_to_char
                                ("CURRENT AMOUNT OF LIQUID HOURS SET.\n\r\n\r", ch);
                            pObj.value[1] = Interp.atoi(argument);
                            break;
                        case 2:
                            Comm.send_to_char("LIQUID TYPE SET.\n\r\n\r", ch);
                            pObj.value[2] = (Lookup.liq_lookup(argument) != -1 ?
                                Lookup.liq_lookup(argument) : 0);
                            break;
                        case 3:
                            Comm.send_to_char("POISON VALUE TOGGLED.\n\r\n\r", ch);
                            pObj.value[3] = (pObj.value[3] == 0) ? 1 : 0;
                            break;
                    }
                    break;

                case ITEM_FOUNTAIN:
                    switch (value_num)
                    {
                        default:
                            Interp.do_help(ch, "ITEM_FOUNTAIN");
                            /* OLC            do_help( ch, "liquids" );    */
                            return false;
                        case 0:
                            Comm.send_to_char
                                ("MAXIMUM AMOUT OF LIQUID HOURS SET.\n\r\n\r", ch);
                            pObj.value[0] = Interp.atoi(argument);
                            break;
                        case 1:
                            Comm.send_to_char
                                ("CURRENT AMOUNT OF LIQUID HOURS SET.\n\r\n\r", ch);
                            pObj.value[1] = Interp.atoi(argument);
                            break;
                        case 2:
                            Comm.send_to_char("LIQUID TYPE SET.\n\r\n\r", ch);
                            pObj.value[2] = (Lookup.liq_lookup(argument) != -1 ?
                                Lookup.liq_lookup(argument) : 0);
                            break;
                    }
                    break;

                case ITEM_FOOD:
                    switch (value_num)
                    {
                        default:
                            Interp.do_help(ch, "ITEM_FOOD");
                            return false;
                        case 0:
                            Comm.send_to_char("HOURS OF FOOD SET.\n\r\n\r", ch);
                            pObj.value[0] = Interp.atoi(argument);
                            break;
                        case 1:
                            Comm.send_to_char("HOURS OF FULL SET.\n\r\n\r", ch);
                            pObj.value[1] = Interp.atoi(argument);
                            break;
                        case 3:
                            Comm.send_to_char("POISON VALUE TOGGLED.\n\r\n\r", ch);
                            pObj.value[3] = (pObj.value[3] == 0) ? 1 : 0;
                            break;
                    }
                    break;

                case ITEM_MONEY:
                    switch (value_num)
                    {
                        default:
                            Interp.do_help(ch, "ITEM_MONEY");
                            return false;
                        case 0:
                            Comm.send_to_char("GOLD AMOUNT SET.\n\r\n\r", ch);
                            pObj.value[0] = Interp.atoi(argument);
                            break;
                        case 1:
                            Comm.send_to_char("SILVER AMOUNT SET.\n\r\n\r", ch);
                            pObj.value[1] = Interp.atoi(argument);
                            break;
                    }
                    break;
            }

            show_obj_values(ch, pObj);

            return true;
        }

        public static bool oedit_show(CharData ch, string argument)
        {
            argument ??= "";
            var pObj = (ObjIndexData)ch.desc.pEdit;
            string buf;
            int cnt;

            buf = RomString.sprintf("Name:        [%s]\n\rArea:        [%5d] %s\n\r",
                pObj.name,
                pObj.area == null ? -1 : pObj.area.vnum,
                pObj.area == null ? "No Area" : pObj.area.name);
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Vnum:        [%5d]\n\rType:        [%s]\n\r",
                pObj.vnum, Lookup.flag_string(Tables.type_flags, pObj.item_type));
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Level:       [%5d]\n\r", pObj.level);
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Wear flags:  [%s]\n\r",
                Lookup.flag_string(Tables.wear_flags, pObj.wear_flags));
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Extra flags: [%s]\n\r",
                Lookup.flag_string(Tables.extra_flags, pObj.extra_flags));
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Material:    [%s]\n\r", /* ROM */
                pObj.material);
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Condition:   [%5d]\n\r", /* ROM */
                pObj.condition);
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Weight:      [%5d]\n\rCost:        [%5d]\n\r",
                pObj.weight, pObj.cost);
            Comm.send_to_char(buf, ch);

            if (pObj.extra_descr != null)
            {
                Comm.send_to_char("Ex desc kwd: ", ch);

                for (var ed = pObj.extra_descr; ed != null; ed = ed.next)
                {
                    Comm.send_to_char("[", ch);
                    Comm.send_to_char(ed.keyword, ch);
                    Comm.send_to_char("]", ch);
                }

                Comm.send_to_char("\n\r", ch);
            }

            buf = RomString.sprintf("Short desc:  %s\n\rLong desc:\n\r     %s\n\r",
                pObj.short_descr, pObj.description);
            Comm.send_to_char(buf, ch);

            AffectData paf;
            for (cnt = 0, paf = pObj.affected; paf != null; paf = paf.next)
            {
                if (cnt == 0)
                {
                    Comm.send_to_char("Number Modifier Affects\n\r", ch);
                    Comm.send_to_char("------ -------- -------\n\r", ch);
                }
                buf = RomString.sprintf("[%4d] %-8d %s\n\r", cnt,
                    paf.modifier, Lookup.flag_string(Tables.apply_flags, paf.location));
                Comm.send_to_char(buf, ch);
                cnt++;
            }

            show_obj_values(ch, pObj);

            return false;
        }

        /*
         * Need to issue warning if flag isn't valid. -- does so now -- Hugin.
         */
        public static bool oedit_addaffect(CharData ch, string argument)
        {
            argument ??= "";
            int value;
            var pObj = (ObjIndexData)ch.desc.pEdit;

            argument = RomString.one_argument(argument, out string loc);
            RomString.one_argument(argument, out string mod);

            if (loc.Length == 0 || mod.Length == 0 || !Interp.is_number(mod))
            {
                Comm.send_to_char("Syntax:  addaffect [location] [#xmod]\n\r", ch);
                return false;
            }

            if ((value = Lookup.flag_value(Tables.apply_flags, loc)) == NO_FLAG)
            { /* Hugin */
                Comm.send_to_char("Valid affects are:\n\r", ch);
                show_help(ch, "apply");
                return false;
            }

            var pAf = Recycle.new_affect();
            pAf.location = value;
            pAf.modifier = Interp.atoi(mod);
            pAf.where = TO_OBJECT;
            pAf.type = -1;
            pAf.duration = -1;
            pAf.bitvector = 0;
            pAf.level = pObj.level;
            pAf.next = pObj.affected;
            pObj.affected = pAf;

            Comm.send_to_char("Affect added.\n\r", ch);
            return true;
        }

        public static bool oedit_addapply(CharData ch, string argument)
        {
            argument ??= "";
            int value, bv, typ;
            var pObj = (ObjIndexData)ch.desc.pEdit;

            argument = RomString.one_argument(argument, out string type);
            argument = RomString.one_argument(argument, out string loc);
            argument = RomString.one_argument(argument, out string mod);
            RomString.one_argument(argument, out string bvector);

            if (type.Length == 0 || (typ = Lookup.flag_value(Tables.apply_types, type)) == NO_FLAG)
            {
                Comm.send_to_char("Invalid apply type. Valid apply types are:\n\r", ch);
                show_help(ch, "apptype");
                return false;
            }

            if (loc.Length == 0 || (value = Lookup.flag_value(Tables.apply_flags, loc)) == NO_FLAG)
            {
                Comm.send_to_char("Valid applys are:\n\r", ch);
                show_help(ch, "apply");
                return false;
            }

            if (bvector.Length == 0
                || (bv = Lookup.flag_value(Tables.bitvector_type[typ].table, bvector)) == NO_FLAG)
            {
                Comm.send_to_char("Invalid bitvector type.\n\r", ch);
                Comm.send_to_char("Valid bitvector types are:\n\r", ch);
                show_help(ch, Tables.bitvector_type[typ].help);
                return false;
            }

            if (mod.Length == 0 || !Interp.is_number(mod))
            {
                Comm.send_to_char
                    ("Syntax:  addapply [type] [location] [#xmod] [bitvector]\n\r",
                    ch);
                return false;
            }

            var pAf = Recycle.new_affect();
            pAf.location = value;
            pAf.modifier = Interp.atoi(mod);
            pAf.where = (int)Tables.apply_types[typ].bit;
            pAf.type = -1;
            pAf.duration = -1;
            pAf.bitvector = bv;
            pAf.level = pObj.level;
            pAf.next = pObj.affected;
            pObj.affected = pAf;

            Comm.send_to_char("Apply added.\n\r", ch);
            return true;
        }

        /*
         * My thanks to Hans Hvidsten Birkeland and Noam Krendel(Walker)
         * for really teaching me how to manipulate pointers.
         */
        public static bool oedit_delaffect(CharData ch, string argument)
        {
            argument ??= "";
            var pObj = (ObjIndexData)ch.desc.pEdit;
            AffectData pAf;
            AffectData pAf_next;
            int value;
            int cnt = 0;

            RomString.one_argument(argument, out string affect);

            if (!Interp.is_number(affect) || affect.Length == 0)
            {
                Comm.send_to_char("Syntax:  delaffect [#xaffect]\n\r", ch);
                return false;
            }

            value = Interp.atoi(affect);

            if (value < 0)
            {
                Comm.send_to_char("Only non-negative affect-numbers allowed.\n\r", ch);
                return false;
            }

            if ((pAf = pObj.affected) == null)
            {
                Comm.send_to_char("OEdit:  Non-existant affect.\n\r", ch);
                return false;
            }

            if (value == 0)
            { /* First case: Remove first affect */
                pAf = pObj.affected;
                pObj.affected = pAf.next;
                Recycle.free_affect(pAf);
            }
            else
            { /* Affect to remove is not the first */
                while ((pAf_next = pAf.next) != null && (++cnt < value))
                    pAf = pAf_next;

                if (pAf_next != null)
                { /* See if it's the next affect */
                    pAf.next = pAf_next.next;
                    Recycle.free_affect(pAf_next);
                }
                else
                { /* Doesn't exist */
                    Comm.send_to_char("No such affect.\n\r", ch);
                    return false;
                }
            }

            Comm.send_to_char("Affect removed.\n\r", ch);
            return true;
        }

        public static bool oedit_name(CharData ch, string argument)
        {
            argument ??= "";
            var pObj = (ObjIndexData)ch.desc.pEdit;

            if (argument.Length == 0)
            {
                Comm.send_to_char("Syntax:  name [string]\n\r", ch);
                return false;
            }

            pObj.name = argument;

            Comm.send_to_char("Name set.\n\r", ch);
            return true;
        }

        public static bool oedit_short(CharData ch, string argument)
        {
            argument ??= "";
            var pObj = (ObjIndexData)ch.desc.pEdit;

            if (argument.Length == 0)
            {
                Comm.send_to_char("Syntax:  short [string]\n\r", ch);
                return false;
            }

            pObj.short_descr = argument;
            pObj.short_descr = Bit.LOWER(pObj.short_descr[0]) + pObj.short_descr.Substring(1);

            Comm.send_to_char("Short description set.\n\r", ch);
            return true;
        }

        public static bool oedit_long(CharData ch, string argument)
        {
            argument ??= "";
            var pObj = (ObjIndexData)ch.desc.pEdit;

            if (argument.Length == 0)
            {
                Comm.send_to_char("Syntax:  long [string]\n\r", ch);
                return false;
            }

            pObj.description = argument;
            pObj.description = Bit.UPPER(pObj.description[0]) + pObj.description.Substring(1);

            Comm.send_to_char("Long description set.\n\r", ch);
            return true;
        }

        static bool set_value(CharData ch, ObjIndexData pObj, string argument,
            int value)
        {
            argument ??= "";
            if (argument.Length == 0)
            {
                set_obj_values(ch, pObj, -1, ""); /* '\0' changed to "" -- Hugin */
                return false;
            }

            if (set_obj_values(ch, pObj, value, argument))
                return true;

            return false;
        }

        /*****************************************************************************
         Name:        oedit_values
         Purpose:    Finds the object and sets its value.
         Called by:    The four valueX functions below. (now five -- Hugin )
         ****************************************************************************/
        static bool oedit_values(CharData ch, string argument, int value)
        {
            argument ??= "";
            var pObj = (ObjIndexData)ch.desc.pEdit;

            if (set_value(ch, pObj, argument, value))
                return true;

            return false;
        }

        public static bool oedit_value0(CharData ch, string argument)
        {
            argument ??= "";
            if (oedit_values(ch, argument, 0))
                return true;

            return false;
        }

        public static bool oedit_value1(CharData ch, string argument)
        {
            argument ??= "";
            if (oedit_values(ch, argument, 1))
                return true;

            return false;
        }

        public static bool oedit_value2(CharData ch, string argument)
        {
            argument ??= "";
            if (oedit_values(ch, argument, 2))
                return true;

            return false;
        }

        public static bool oedit_value3(CharData ch, string argument)
        {
            argument ??= "";
            if (oedit_values(ch, argument, 3))
                return true;

            return false;
        }

        public static bool oedit_value4(CharData ch, string argument)
        {
            argument ??= "";
            if (oedit_values(ch, argument, 4))
                return true;

            return false;
        }

        public static bool oedit_weight(CharData ch, string argument)
        {
            argument ??= "";
            var pObj = (ObjIndexData)ch.desc.pEdit;

            if (argument.Length == 0 || !Interp.is_number(argument))
            {
                Comm.send_to_char("Syntax:  weight [number]\n\r", ch);
                return false;
            }

            pObj.weight = Interp.atoi(argument);

            Comm.send_to_char("Weight set.\n\r", ch);
            return true;
        }

        public static bool oedit_cost(CharData ch, string argument)
        {
            argument ??= "";
            var pObj = (ObjIndexData)ch.desc.pEdit;

            if (argument.Length == 0 || !Interp.is_number(argument))
            {
                Comm.send_to_char("Syntax:  cost [number]\n\r", ch);
                return false;
            }

            pObj.cost = Interp.atoi(argument);

            Comm.send_to_char("Cost set.\n\r", ch);
            return true;
        }

        public static bool oedit_ed(CharData ch, string argument)
        {
            argument ??= "";
            var pObj = (ObjIndexData)ch.desc.pEdit;
            ExtraDescrData ed;

            argument = RomString.one_argument(argument, out string command);
            RomString.one_argument(argument, out string keyword);

            if (command.Length == 0)
            {
                Comm.send_to_char("Syntax:  ed add [keyword]\n\r", ch);
                Comm.send_to_char("         ed delete [keyword]\n\r", ch);
                Comm.send_to_char("         ed edit [keyword]\n\r", ch);
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

                ed = Recycle.new_extra_descr();
                ed.keyword = keyword;
                ed.next = pObj.extra_descr;
                pObj.extra_descr = ed;

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

                for (ed = pObj.extra_descr; ed != null; ed = ed.next)
                {
                    if (Handler.is_name(keyword, ed.keyword))
                        break;
                }

                if (ed == null)
                {
                    Comm.send_to_char("OEdit:  Extra description keyword not found.\n\r",
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

                for (ed = pObj.extra_descr; ed != null; ed = ed.next)
                {
                    if (Handler.is_name(keyword, ed.keyword))
                        break;
                    ped = ed;
                }

                if (ed == null)
                {
                    Comm.send_to_char("OEdit:  Extra description keyword not found.\n\r",
                        ch);
                    return false;
                }

                if (ped == null)
                    pObj.extra_descr = ed.next;
                else
                    ped.next = ed.next;

                Recycle.free_extra_descr(ed);

                Comm.send_to_char("Extra description deleted.\n\r", ch);
                return true;
            }

            if (!RomString.str_cmp(command, "format"))
            {
                ExtraDescrData ped = null;

                if (keyword.Length == 0)
                {
                    Comm.send_to_char("Syntax:  ed format [keyword]\n\r", ch);
                    return false;
                }

                for (ed = pObj.extra_descr; ed != null; ed = ed.next)
                {
                    if (Handler.is_name(keyword, ed.keyword))
                        break;
                    ped = ed;
                }

                if (ed == null)
                {
                    Comm.send_to_char("OEdit:  Extra description keyword not found.\n\r",
                        ch);
                    return false;
                }

                ed.description = RomString.format_string(ed.description);

                Comm.send_to_char("Extra description formatted.\n\r", ch);
                return true;
            }

            oedit_ed(ch, "");
            return false;
        }

        /* ROM object functions : */

        public static bool oedit_extra(CharData ch, string argument)
        { /* Moved out of oedit() due to naming conflicts -- Hugin */
            argument ??= "";
            int value;

            if (argument.Length != 0)
            {
                var pObj = (ObjIndexData)ch.desc.pEdit;

                if ((value = Lookup.flag_value(Tables.extra_flags, argument)) != NO_FLAG)
                {
                    Bit.TOGGLE_BIT(ref pObj.extra_flags, value);

                    Comm.send_to_char("Extra flag toggled.\n\r", ch);
                    return true;
                }
            }

            Comm.send_to_char("Syntax:  extra [flag]\n\r" +
                "Type '? extra' for a list of flags.\n\r", ch);
            return false;
        }

        public static bool oedit_wear(CharData ch, string argument)
        { /* Moved out of oedit() due to naming conflicts -- Hugin */
            argument ??= "";
            int value;

            if (argument.Length != 0)
            {
                var pObj = (ObjIndexData)ch.desc.pEdit;

                if ((value = Lookup.flag_value(Tables.wear_flags, argument)) != NO_FLAG)
                {
                    Bit.TOGGLE_BIT(ref pObj.wear_flags, value);

                    Comm.send_to_char("Wear flag toggled.\n\r", ch);
                    return true;
                }
            }

            Comm.send_to_char("Syntax:  wear [flag]\n\r" +
                "Type '? wear' for a list of flags.\n\r", ch);
            return false;
        }

        public static bool oedit_type(CharData ch, string argument)
        { /* Moved out of oedit() due to naming conflicts -- Hugin */
            argument ??= "";
            int value;

            if (argument.Length != 0)
            {
                var pObj = (ObjIndexData)ch.desc.pEdit;

                if ((value = Lookup.flag_value(Tables.type_flags, argument)) != NO_FLAG)
                {
                    pObj.item_type = value;

                    Comm.send_to_char("Type set.\n\r", ch);

                    /*
                     * Clear the values.
                     */
                    pObj.value[0] = 0;
                    pObj.value[1] = 0;
                    pObj.value[2] = 0;
                    pObj.value[3] = 0;
                    pObj.value[4] = 0; /* ROM */

                    return true;
                }
            }

            Comm.send_to_char("Syntax:  type [flag]\n\r" +
                "Type '? type' for a list of flags.\n\r", ch);
            return false;
        }

        public static bool oedit_material(CharData ch, string argument)
        {
            argument ??= "";
            var pObj = (ObjIndexData)ch.desc.pEdit;

            if (argument.Length == 0)
            {
                Comm.send_to_char("Syntax:  material [string]\n\r", ch);
                return false;
            }

            pObj.material = argument;

            Comm.send_to_char("Material set.\n\r", ch);
            return true;
        }

        public static bool oedit_level(CharData ch, string argument)
        {
            argument ??= "";
            var pObj = (ObjIndexData)ch.desc.pEdit;

            if (argument.Length == 0 || !Interp.is_number(argument))
            {
                Comm.send_to_char("Syntax:  level [number]\n\r", ch);
                return false;
            }

            pObj.level = Interp.atoi(argument);

            Comm.send_to_char("Level set.\n\r", ch);
            return true;
        }

        public static bool oedit_condition(CharData ch, string argument)
        {
            argument ??= "";
            int value;

            if (argument.Length != 0
                && (value = Interp.atoi(argument)) >= 0 && (value <= 100))
            {
                var pObj = (ObjIndexData)ch.desc.pEdit;

                pObj.condition = value;
                Comm.send_to_char("Condition set.\n\r", ch);

                return true;
            }

            Comm.send_to_char("Syntax:  condition [number]\n\r" +
                "Where number can range from 0 (ruined) to 100 (perfect).\n\r",
                ch);
            return false;
        }

        /*
         * Mobile Editor Functions.
         */
        public static bool medit_show(CharData ch, string argument)
        {
            argument ??= "";
            var pMob = (MobIndexData)ch.desc.pEdit;
            string buf;
            MprogList list;

            buf = RomString.sprintf("Name:        [%s]\n\rArea:        [%5d] %s\n\r",
                pMob.player_name,
                pMob.area == null ? -1 : pMob.area.vnum,
                pMob.area == null ? "No Area" : pMob.area.name);
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Act:         [%s]\n\r",
                Lookup.flag_string(Tables.act_flags, pMob.act));
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Vnum:        [%5d] Sex:   [%s]   Race: [%s]\n\r",
                pMob.vnum,
                pMob.sex == SEX_MALE ? "male   " :
                pMob.sex == SEX_FEMALE ? "female " :
                pMob.sex == 3 ? "random " : "neutral",
                Tables.race_table[pMob.race].name);
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf(
                "Level:       [%2d]    Align: [%4d]      Hitroll: [%2d] Dam Type:    [%s]\n\r",
                pMob.level, pMob.alignment,
                pMob.hitroll, Tables.attack_table[pMob.dam_type].name);
            Comm.send_to_char(buf, ch);

            if (pMob.group != 0)
            {
                buf = RomString.sprintf("Group:       [%5d]\n\r", pMob.group);
                Comm.send_to_char(buf, ch);
            }

            buf = RomString.sprintf("Hit dice:    [%2dd%-3d+%4d] ",
                pMob.hit[DICE_NUMBER],
                pMob.hit[DICE_TYPE], pMob.hit[DICE_BONUS]);
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Damage dice: [%2dd%-3d+%4d] ",
                pMob.damage[DICE_NUMBER],
                pMob.damage[DICE_TYPE], pMob.damage[DICE_BONUS]);
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Mana dice:   [%2dd%-3d+%4d]\n\r",
                pMob.mana[DICE_NUMBER],
                pMob.mana[DICE_TYPE], pMob.mana[DICE_BONUS]);
            Comm.send_to_char(buf, ch);

            /* ROM values end */

            buf = RomString.sprintf("Affected by: [%s]\n\r",
                Lookup.flag_string(Tables.affect_flags, pMob.affected_by));
            Comm.send_to_char(buf, ch);

            /* ROM values: */

            buf = RomString.sprintf(
                "Armor:       [pierce: %d  bash: %d  slash: %d  magic: %d]\n\r",
                pMob.ac[AC_PIERCE], pMob.ac[AC_BASH], pMob.ac[AC_SLASH],
                pMob.ac[AC_EXOTIC]);
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Form:        [%s]\n\r",
                Lookup.flag_string(Tables.form_flags, pMob.form));
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Parts:       [%s]\n\r",
                Lookup.flag_string(Tables.part_flags, pMob.parts));
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Imm:         [%s]\n\r",
                Lookup.flag_string(Tables.imm_flags, pMob.imm_flags));
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Res:         [%s]\n\r",
                Lookup.flag_string(Tables.res_flags, pMob.res_flags));
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Vuln:        [%s]\n\r",
                Lookup.flag_string(Tables.vuln_flags, pMob.vuln_flags));
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Off:         [%s]\n\r",
                Lookup.flag_string(Tables.off_flags, pMob.off_flags));
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Size:        [%s]\n\r",
                Lookup.flag_string(Tables.size_flags, pMob.size));
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Material:    [%s]\n\r", pMob.material);
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Start pos.   [%s]\n\r",
                Lookup.flag_string(Tables.position_flags, pMob.start_pos));
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Default pos  [%s]\n\r",
                Lookup.flag_string(Tables.position_flags, pMob.default_pos));
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Wealth:      [%5ld]\n\r", pMob.wealth);
            Comm.send_to_char(buf, ch);

            /* ROM values end */

            if (pMob.spec_fun != null)
            {
                buf = RomString.sprintf("Spec fun:    [%s]\n\r", Lookup.spec_name(pMob.spec_fun));
                Comm.send_to_char(buf, ch);
            }

            buf = RomString.sprintf("Short descr: %s\n\rLong descr:\n\r%s",
                pMob.short_descr, pMob.long_descr);
            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("Description:\n\r%s", pMob.description);
            Comm.send_to_char(buf, ch);

            if (pMob.pShop != null)
            {
                ShopData pShop;
                int iTrade;

                pShop = pMob.pShop;

                buf = RomString.sprintf(
                    "Shop data for [%5d]:\n\r" +
                    "  Markup for purchaser: %d%%\n\r" +
                    "  Markdown for seller:  %d%%\n\r",
                    pShop.keeper, pShop.profit_buy, pShop.profit_sell);
                Comm.send_to_char(buf, ch);
                buf = RomString.sprintf("  Hours: %d to %d.\n\r",
                    pShop.open_hour, pShop.close_hour);
                Comm.send_to_char(buf, ch);

                for (iTrade = 0; iTrade < MAX_TRADE; iTrade++)
                {
                    if (pShop.buy_type[iTrade] != 0)
                    {
                        if (iTrade == 0)
                        {
                            Comm.send_to_char("  Number Trades Type\n\r", ch);
                            Comm.send_to_char("  ------ -----------\n\r", ch);
                        }
                        buf = RomString.sprintf("  [%4d] %s\n\r", iTrade,
                            Lookup.flag_string(Tables.type_flags, pShop.buy_type[iTrade]));
                        Comm.send_to_char(buf, ch);
                    }
                }
            }

            if (pMob.mprogs != null)
            {
                int cnt;

                buf = RomString.sprintf("\n\rMOBPrograms for [%5d]:\n\r", pMob.vnum);
                Comm.send_to_char(buf, ch);

                for (cnt = 0, list = pMob.mprogs; list != null; list = list.next)
                {
                    if (cnt == 0)
                    {
                        Comm.send_to_char(" Number Vnum Trigger Phrase\n\r", ch);
                        Comm.send_to_char(" ------ ---- ------- ------\n\r", ch);
                    }

                    buf = RomString.sprintf("[%5d] %4d %7s %s\n\r", cnt,
                        list.vnum, MobCmds.mprog_type_to_name(list.trig_type),
                        list.trig_phrase);
                    Comm.send_to_char(buf, ch);
                    cnt++;
                }
            }

            return false;
        }

        public static bool medit_spec(CharData ch, string argument)
        {
            argument ??= "";
            var pMob = (MobIndexData)ch.desc.pEdit;

            if (argument.Length == 0)
            {
                Comm.send_to_char("Syntax:  spec [special function]\n\r", ch);
                return false;
            }

            if (!RomString.str_cmp(argument, "none"))
            {
                pMob.spec_fun = null;

                Comm.send_to_char("Spec removed.\n\r", ch);
                return true;
            }

            if (Lookup.spec_lookup(argument) != null)
            {
                pMob.spec_fun = Lookup.spec_lookup(argument);
                Comm.send_to_char("Spec set.\n\r", ch);
                return true;
            }

            Comm.send_to_char("MEdit: No such special function.\n\r", ch);
            return false;
        }

        public static bool medit_damtype(CharData ch, string argument)
        {
            argument ??= "";
            var pMob = (MobIndexData)ch.desc.pEdit;

            if (argument.Length == 0)
            {
                Comm.send_to_char("Syntax:  damtype [damage message]\n\r", ch);
                Comm.send_to_char
                    ("For a list of damage types, type '? weapon'.\n\r",
                    ch);
                return false;
            }

            pMob.dam_type = Lookup.attack_lookup(argument);
            Comm.send_to_char("Damage type set.\n\r", ch);
            return true;
        }

        public static bool medit_align(CharData ch, string argument)
        {
            argument ??= "";
            var pMob = (MobIndexData)ch.desc.pEdit;

            if (argument.Length == 0 || !Interp.is_number(argument))
            {
                Comm.send_to_char("Syntax:  alignment [number]\n\r", ch);
                return false;
            }

            pMob.alignment = Interp.atoi(argument);

            Comm.send_to_char("Alignment set.\n\r", ch);
            return true;
        }

        public static bool medit_level(CharData ch, string argument)
        {
            argument ??= "";
            var pMob = (MobIndexData)ch.desc.pEdit;

            if (argument.Length == 0 || !Interp.is_number(argument))
            {
                Comm.send_to_char("Syntax:  level [number]\n\r", ch);
                return false;
            }

            pMob.level = Interp.atoi(argument);

            Comm.send_to_char("Level set.\n\r", ch);
            return true;
        }

        public static bool medit_desc(CharData ch, string argument)
        {
            argument ??= "";
            var pMob = (MobIndexData)ch.desc.pEdit;

            if (argument.Length == 0)
            {
                RomString.string_append(ch, new StringPtr(() => pMob.description, v => pMob.description = v));
                return true;
            }

            Comm.send_to_char("Syntax:  desc    - line edit\n\r", ch);
            return false;
        }

        public static bool medit_long(CharData ch, string argument)
        {
            argument ??= "";
            var pMob = (MobIndexData)ch.desc.pEdit;

            if (argument.Length == 0)
            {
                Comm.send_to_char("Syntax:  long [string]\n\r", ch);
                return false;
            }

            argument += "\n\r";
            pMob.long_descr = argument;
            pMob.long_descr = Bit.UPPER(pMob.long_descr[0]) + pMob.long_descr.Substring(1);

            Comm.send_to_char("Long description set.\n\r", ch);
            return true;
        }

        public static bool medit_short(CharData ch, string argument)
        {
            argument ??= "";
            var pMob = (MobIndexData)ch.desc.pEdit;

            if (argument.Length == 0)
            {
                Comm.send_to_char("Syntax:  short [string]\n\r", ch);
                return false;
            }

            pMob.short_descr = argument;

            Comm.send_to_char("Short description set.\n\r", ch);
            return true;
        }

        public static bool medit_name(CharData ch, string argument)
        {
            argument ??= "";
            var pMob = (MobIndexData)ch.desc.pEdit;

            if (argument.Length == 0)
            {
                Comm.send_to_char("Syntax:  name [string]\n\r", ch);
                return false;
            }

            pMob.player_name = argument;

            Comm.send_to_char("Name set.\n\r", ch);
            return true;
        }

        public static bool medit_shop(CharData ch, string argument)
        {
            argument ??= "";
            var pMob = (MobIndexData)ch.desc.pEdit;

            argument = RomString.one_argument(argument, out string command);
            argument = RomString.one_argument(argument, out string arg1);

            if (command.Length == 0)
            {
                Comm.send_to_char("Syntax:  shop hours [#xopening] [#xclosing]\n\r", ch);
                Comm.send_to_char("         shop profit [#xbuying%] [#xselling%]\n\r",
                    ch);
                Comm.send_to_char("         shop type [#x0-4] [item type]\n\r", ch);
                Comm.send_to_char("         shop assign\n\r", ch);
                Comm.send_to_char("         shop remove\n\r", ch);
                return false;
            }

            if (!RomString.str_cmp(command, "hours"))
            {
                if (arg1.Length == 0 || !Interp.is_number(arg1)
                    || argument.Length == 0 || !Interp.is_number(argument))
                {
                    Comm.send_to_char("Syntax:  shop hours [#xopening] [#xclosing]\n\r",
                        ch);
                    return false;
                }

                if (pMob.pShop == null)
                {
                    Comm.send_to_char
                        ("MEdit:  You must create the shop first (shop assign).\n\r",
                        ch);
                    return false;
                }

                pMob.pShop.open_hour = Interp.atoi(arg1);
                pMob.pShop.close_hour = Interp.atoi(argument);

                Comm.send_to_char("Shop hours set.\n\r", ch);
                return true;
            }

            if (!RomString.str_cmp(command, "profit"))
            {
                if (arg1.Length == 0 || !Interp.is_number(arg1)
                    || argument.Length == 0 || !Interp.is_number(argument))
                {
                    Comm.send_to_char("Syntax:  shop profit [#xbuying%] [#xselling%]\n\r",
                        ch);
                    return false;
                }

                if (pMob.pShop == null)
                {
                    Comm.send_to_char
                        ("MEdit:  You must create the shop first (shop assign).\n\r",
                        ch);
                    return false;
                }

                pMob.pShop.profit_buy = Interp.atoi(arg1);
                pMob.pShop.profit_sell = Interp.atoi(argument);

                Comm.send_to_char("Shop profit set.\n\r", ch);
                return true;
            }

            if (!RomString.str_cmp(command, "type"))
            {
                int value;

                if (arg1.Length == 0 || !Interp.is_number(arg1) || argument.Length == 0)
                {
                    Comm.send_to_char("Syntax:  shop type [#x0-4] [item type]\n\r", ch);
                    return false;
                }

                if (Interp.atoi(arg1) >= MAX_TRADE)
                {
                    string buf = RomString.sprintf("MEdit:  May sell %d items max.\n\r", MAX_TRADE);
                    Comm.send_to_char(buf, ch);
                    return false;
                }

                if (pMob.pShop == null)
                {
                    Comm.send_to_char
                        ("MEdit:  You must create the shop first (shop assign).\n\r",
                        ch);
                    return false;
                }

                if ((value = Lookup.flag_value(Tables.type_flags, argument)) == NO_FLAG)
                {
                    Comm.send_to_char("MEdit:  That type of item is not known.\n\r", ch);
                    return false;
                }

                pMob.pShop.buy_type[Interp.atoi(arg1)] = value;

                Comm.send_to_char("Shop type set.\n\r", ch);
                return true;
            }

            /* shop assign && shop delete by Phoenix */

            if (!RomString.str_prefix(command, "assign"))
            {
                if (pMob.pShop != null)
                {
                    Comm.send_to_char("Mob already has a shop assigned to it.\n\r", ch);
                    return false;
                }

                pMob.pShop = new ShopData();
                pMob.pShop.profit_buy = 100;
                pMob.pShop.profit_sell = 100;
                pMob.pShop.open_hour = 0;
                pMob.pShop.close_hour = 23;
                if (Game.shop_first == null)
                    Game.shop_first = pMob.pShop;
                if (Game.shop_last != null)
                    Game.shop_last.next = pMob.pShop;
                Game.shop_last = pMob.pShop;

                pMob.pShop.keeper = pMob.vnum;

                Comm.send_to_char("New shop assigned to mobile.\n\r", ch);
                return true;
            }

            if (!RomString.str_prefix(command, "remove"))
            {
                ShopData pShop;

                pShop = pMob.pShop;
                pMob.pShop = null;

                if (pShop == Game.shop_first)
                {
                    if (pShop.next == null)
                    {
                        Game.shop_first = null;
                        Game.shop_last = null;
                    }
                    else
                        Game.shop_first = pShop.next;
                }
                else
                {
                    ShopData ipShop;

                    for (ipShop = Game.shop_first; ipShop != null; ipShop = ipShop.next)
                    {
                        if (ipShop.next == pShop)
                        {
                            if (pShop.next == null)
                            {
                                Game.shop_last = ipShop;
                                Game.shop_last.next = null;
                            }
                            else
                                ipShop.next = pShop.next;
                        }
                    }
                }

                Comm.send_to_char("Mobile is no longer a shopkeeper.\n\r", ch);
                return true;
            }

            medit_shop(ch, "");
            return false;
        }

        /* ROM medit functions: */

        public static bool medit_sex(CharData ch, string argument)
        { /* Moved out of medit() due to naming conflicts -- Hugin */
            argument ??= "";
            int value;

            if (argument.Length != 0)
            {
                var pMob = (MobIndexData)ch.desc.pEdit;

                if ((value = Lookup.flag_value(Tables.sex_flags, argument)) != NO_FLAG)
                {
                    pMob.sex = value;

                    Comm.send_to_char("Sex set.\n\r", ch);
                    return true;
                }
            }

            Comm.send_to_char("Syntax: sex [sex]\n\r" +
                "Type '? sex' for a list of flags.\n\r", ch);
            return false;
        }

        public static bool medit_act(CharData ch, string argument)
        { /* Moved out of medit() due to naming conflicts -- Hugin */
            argument ??= "";
            int value;

            if (argument.Length != 0)
            {
                var pMob = (MobIndexData)ch.desc.pEdit;

                if ((value = Lookup.flag_value(Tables.act_flags, argument)) != NO_FLAG)
                {
                    pMob.act ^= value;
                    Bit.SET_BIT(ref pMob.act, ACT_IS_NPC);

                    Comm.send_to_char("Act flag toggled.\n\r", ch);
                    return true;
                }
            }

            Comm.send_to_char("Syntax: act [flag]\n\r" +
                "Type '? act' for a list of flags.\n\r", ch);
            return false;
        }

        public static bool medit_affect(CharData ch, string argument)
        { /* Moved out of medit() due to naming conflicts -- Hugin */
            argument ??= "";
            int value;

            if (argument.Length != 0)
            {
                var pMob = (MobIndexData)ch.desc.pEdit;

                if ((value = Lookup.flag_value(Tables.affect_flags, argument)) != NO_FLAG)
                {
                    pMob.affected_by ^= value;

                    Comm.send_to_char("Affect flag toggled.\n\r", ch);
                    return true;
                }
            }

            Comm.send_to_char("Syntax: affect [flag]\n\r" +
                "Type '? affect' for a list of flags.\n\r", ch);
            return false;
        }

        public static bool medit_ac(CharData ch, string argument)
        {
            argument ??= "";
            int pierce, bash, slash, exotic;

            do
            { /* So that I can use break and send the syntax in one place */
                if (argument.Length == 0)
                    break;

                var pMob = (MobIndexData)ch.desc.pEdit;
                argument = RomString.one_argument(argument, out string arg);

                if (!Interp.is_number(arg))
                    break;
                pierce = Interp.atoi(arg);
                argument = RomString.one_argument(argument, out arg);

                if (arg.Length != 0)
                {
                    if (!Interp.is_number(arg))
                        break;
                    bash = Interp.atoi(arg);
                    argument = RomString.one_argument(argument, out arg);
                }
                else
                    bash = pMob.ac[AC_BASH];

                if (arg.Length != 0)
                {
                    if (!Interp.is_number(arg))
                        break;
                    slash = Interp.atoi(arg);
                    argument = RomString.one_argument(argument, out arg);
                }
                else
                    slash = pMob.ac[AC_SLASH];

                if (arg.Length != 0)
                {
                    if (!Interp.is_number(arg))
                        break;
                    exotic = Interp.atoi(arg);
                }
                else
                    exotic = pMob.ac[AC_EXOTIC];

                pMob.ac[AC_PIERCE] = pierce;
                pMob.ac[AC_BASH] = bash;
                pMob.ac[AC_SLASH] = slash;
                pMob.ac[AC_EXOTIC] = exotic;

                Comm.send_to_char("Ac set.\n\r", ch);
                return true;
            }
            while (false); /* Just do it once.. */

            Comm.send_to_char
                ("Syntax:  ac [ac-pierce [ac-bash [ac-slash [ac-exotic]]]]\n\r" +
                "help MOB_AC  gives a list of reasonable ac-values.\n\r", ch);
            return false;
        }

        public static bool medit_form(CharData ch, string argument)
        {
            argument ??= "";
            int value;

            if (argument.Length != 0)
            {
                var pMob = (MobIndexData)ch.desc.pEdit;

                if ((value = Lookup.flag_value(Tables.form_flags, argument)) != NO_FLAG)
                {
                    pMob.form ^= value;
                    Comm.send_to_char("Form toggled.\n\r", ch);
                    return true;
                }
            }

            Comm.send_to_char("Syntax: form [flags]\n\r" +
                "Type '? form' for a list of flags.\n\r", ch);
            return false;
        }

        public static bool medit_part(CharData ch, string argument)
        {
            argument ??= "";
            int value;

            if (argument.Length != 0)
            {
                var pMob = (MobIndexData)ch.desc.pEdit;

                if ((value = Lookup.flag_value(Tables.part_flags, argument)) != NO_FLAG)
                {
                    pMob.parts ^= value;
                    Comm.send_to_char("Parts toggled.\n\r", ch);
                    return true;
                }
            }

            Comm.send_to_char("Syntax: part [flags]\n\r" +
                "Type '? part' for a list of flags.\n\r", ch);
            return false;
        }

        public static bool medit_imm(CharData ch, string argument)
        {
            argument ??= "";
            int value;

            if (argument.Length != 0)
            {
                var pMob = (MobIndexData)ch.desc.pEdit;

                if ((value = Lookup.flag_value(Tables.imm_flags, argument)) != NO_FLAG)
                {
                    pMob.imm_flags ^= value;
                    Comm.send_to_char("Immunity toggled.\n\r", ch);
                    return true;
                }
            }

            Comm.send_to_char("Syntax: imm [flags]\n\r" +
                "Type '? imm' for a list of flags.\n\r", ch);
            return false;
        }

        public static bool medit_res(CharData ch, string argument)
        {
            argument ??= "";
            int value;

            if (argument.Length != 0)
            {
                var pMob = (MobIndexData)ch.desc.pEdit;

                if ((value = Lookup.flag_value(Tables.res_flags, argument)) != NO_FLAG)
                {
                    pMob.res_flags ^= value;
                    Comm.send_to_char("Resistance toggled.\n\r", ch);
                    return true;
                }
            }

            Comm.send_to_char("Syntax: res [flags]\n\r" +
                "Type '? res' for a list of flags.\n\r", ch);
            return false;
        }

        public static bool medit_vuln(CharData ch, string argument)
        {
            argument ??= "";
            int value;

            if (argument.Length != 0)
            {
                var pMob = (MobIndexData)ch.desc.pEdit;

                if ((value = Lookup.flag_value(Tables.vuln_flags, argument)) != NO_FLAG)
                {
                    pMob.vuln_flags ^= value;
                    Comm.send_to_char("Vulnerability toggled.\n\r", ch);
                    return true;
                }
            }

            Comm.send_to_char("Syntax: vuln [flags]\n\r" +
                "Type '? vuln' for a list of flags.\n\r", ch);
            return false;
        }

        public static bool medit_material(CharData ch, string argument)
        {
            argument ??= "";
            var pMob = (MobIndexData)ch.desc.pEdit;

            if (argument.Length == 0)
            {
                Comm.send_to_char("Syntax:  material [string]\n\r", ch);
                return false;
            }

            pMob.material = argument;

            Comm.send_to_char("Material set.\n\r", ch);
            return true;
        }

        public static bool medit_off(CharData ch, string argument)
        {
            argument ??= "";
            int value;

            if (argument.Length != 0)
            {
                var pMob = (MobIndexData)ch.desc.pEdit;

                if ((value = Lookup.flag_value(Tables.off_flags, argument)) != NO_FLAG)
                {
                    pMob.off_flags ^= value;
                    Comm.send_to_char("Offensive behaviour toggled.\n\r", ch);
                    return true;
                }
            }

            Comm.send_to_char("Syntax: off [flags]\n\r" +
                "Type '? off' for a list of flags.\n\r", ch);
            return false;
        }

        public static bool medit_size(CharData ch, string argument)
        {
            argument ??= "";
            int value;

            if (argument.Length != 0)
            {
                var pMob = (MobIndexData)ch.desc.pEdit;

                if ((value = Lookup.flag_value(Tables.size_flags, argument)) != NO_FLAG)
                {
                    pMob.size = value;
                    Comm.send_to_char("Size set.\n\r", ch);
                    return true;
                }
            }

            Comm.send_to_char("Syntax: size [size]\n\r" +
                "Type '? size' for a list of sizes.\n\r", ch);
            return false;
        }

        /* C olc_act.c isdigit / in-place NUL split of 3d8+20 */
        static void split_dice(string argument, out string num, out string type, out string bonus)
        {
            var chars = (argument ?? "").ToCharArray();
            int cp = 0;

            while (cp < chars.Length && char.IsDigit(chars[cp]))
                ++cp;
            while (cp < chars.Length && !char.IsDigit(chars[cp]))
                chars[cp++] = '\0';

            int typePos = cp;

            while (cp < chars.Length && char.IsDigit(chars[cp]))
                ++cp;
            while (cp < chars.Length && !char.IsDigit(chars[cp]))
                chars[cp++] = '\0';

            int bonusPos = cp;

            while (cp < chars.Length && char.IsDigit(chars[cp]))
                ++cp;
            if (cp < chars.Length)
                chars[cp] = '\0';

            num = dice_token(chars, 0);
            type = dice_token(chars, typePos);
            bonus = dice_token(chars, bonusPos);
        }

        static string dice_token(char[] chars, int start)
        {
            if (start >= chars.Length)
                return "";
            int end = start;
            while (end < chars.Length && chars[end] != '\0')
                end++;
            return new string(chars, start, end - start);
        }

        public static bool medit_hitdice(CharData ch, string argument)
        {
            argument ??= "";
            const string syntax = "Syntax:  hitdice <number> d <type> + <bonus>\n\r";
            var pMob = (MobIndexData)ch.desc.pEdit;

            if (argument.Length == 0)
            {
                Comm.send_to_char(syntax, ch);
                return false;
            }

            split_dice(argument, out string num, out string type, out string bonus);

            if ((!Interp.is_number(num) || Interp.atoi(num) < 1)
                || (!Interp.is_number(type) || Interp.atoi(type) < 1)
                || (!Interp.is_number(bonus) || Interp.atoi(bonus) < 0))
            {
                Comm.send_to_char(syntax, ch);
                return false;
            }

            pMob.hit[DICE_NUMBER] = Interp.atoi(num);
            pMob.hit[DICE_TYPE] = Interp.atoi(type);
            pMob.hit[DICE_BONUS] = Interp.atoi(bonus);

            Comm.send_to_char("Hitdice set.\n\r", ch);
            return true;
        }

        public static bool medit_manadice(CharData ch, string argument)
        {
            argument ??= "";
            const string syntax =
                "Syntax:  manadice <number> d <type> + <bonus>\n\r";
            var pMob = (MobIndexData)ch.desc.pEdit;

            if (argument.Length == 0)
            {
                Comm.send_to_char(syntax, ch);
                return false;
            }

            split_dice(argument, out string num, out string type, out string bonus);

            if (!(Interp.is_number(num) && Interp.is_number(type) && Interp.is_number(bonus)))
            {
                Comm.send_to_char(syntax, ch);
                return false;
            }

            if ((!Interp.is_number(num) || Interp.atoi(num) < 1)
                || (!Interp.is_number(type) || Interp.atoi(type) < 1)
                || (!Interp.is_number(bonus) || Interp.atoi(bonus) < 0))
            {
                Comm.send_to_char(syntax, ch);
                return false;
            }

            pMob.mana[DICE_NUMBER] = Interp.atoi(num);
            pMob.mana[DICE_TYPE] = Interp.atoi(type);
            pMob.mana[DICE_BONUS] = Interp.atoi(bonus);

            Comm.send_to_char("Manadice set.\n\r", ch);
            return true;
        }

        public static bool medit_damdice(CharData ch, string argument)
        {
            argument ??= "";
            const string syntax = "Syntax:  damdice <number> d <type> + <bonus>\n\r";
            var pMob = (MobIndexData)ch.desc.pEdit;

            if (argument.Length == 0)
            {
                Comm.send_to_char(syntax, ch);
                return false;
            }

            split_dice(argument, out string num, out string type, out string bonus);

            if (!(Interp.is_number(num) && Interp.is_number(type) && Interp.is_number(bonus)))
            {
                Comm.send_to_char(syntax, ch);
                return false;
            }

            if ((!Interp.is_number(num) || Interp.atoi(num) < 1)
                || (!Interp.is_number(type) || Interp.atoi(type) < 1)
                || (!Interp.is_number(bonus) || Interp.atoi(bonus) < 0))
            {
                Comm.send_to_char(syntax, ch);
                return false;
            }

            pMob.damage[DICE_NUMBER] = Interp.atoi(num);
            pMob.damage[DICE_TYPE] = Interp.atoi(type);
            pMob.damage[DICE_BONUS] = Interp.atoi(bonus);

            Comm.send_to_char("Damdice set.\n\r", ch);
            return true;
        }

        public static bool medit_race(CharData ch, string argument)
        {
            argument ??= "";
            int race;

            if (argument.Length != 0 && (race = Lookup.race_lookup(argument)) != 0)
            {
                var pMob = (MobIndexData)ch.desc.pEdit;

                pMob.race = race;
                pMob.act |= Tables.race_table[race].act;
                pMob.affected_by |= Tables.race_table[race].aff;
                pMob.off_flags |= Tables.race_table[race].off;
                pMob.imm_flags |= Tables.race_table[race].imm;
                pMob.res_flags |= Tables.race_table[race].res;
                pMob.vuln_flags |= Tables.race_table[race].vuln;
                pMob.form |= Tables.race_table[race].form;
                pMob.parts |= Tables.race_table[race].parts;

                Comm.send_to_char("Race set.\n\r", ch);
                return true;
            }

            if (argument.Length != 0 && argument[0] == '?')
            {
                Comm.send_to_char("Available races are:", ch);

                for (race = 0; Tables.race_table[race] != null && Tables.race_table[race].name != null; race++)
                {
                    if ((race % 3) == 0)
                        Comm.send_to_char("\n\r", ch);
                    string buf = RomString.sprintf(" %-15s", Tables.race_table[race].name);
                    Comm.send_to_char(buf, ch);
                }

                Comm.send_to_char("\n\r", ch);
                return false;
            }

            Comm.send_to_char("Syntax:  race [race]\n\r" +
                "Type 'race ?' for a list of races.\n\r", ch);
            return false;
        }

        public static bool medit_position(CharData ch, string argument)
        {
            argument ??= "";
            int value;

            argument = RomString.one_argument(argument, out string arg);

            switch (arg.Length == 0 ? '\0' : arg[0])
            {
                default:
                    break;

                case 'S':
                case 's':
                    if (RomString.str_prefix(arg, "start"))
                        break;

                    if ((value = Lookup.flag_value(Tables.position_flags, argument)) == NO_FLAG)
                        break;

                    {
                        var pMob = (MobIndexData)ch.desc.pEdit;

                        pMob.start_pos = value;
                        Comm.send_to_char("Start position set.\n\r", ch);
                        return true;
                    }

                case 'D':
                case 'd':
                    if (RomString.str_prefix(arg, "default"))
                        break;

                    if ((value = Lookup.flag_value(Tables.position_flags, argument)) == NO_FLAG)
                        break;

                    {
                        var pMob = (MobIndexData)ch.desc.pEdit;

                        pMob.default_pos = value;
                        Comm.send_to_char("Default position set.\n\r", ch);
                        return true;
                    }
            }

            Comm.send_to_char("Syntax:  position [start/default] [position]\n\r" +
                "Type '? position' for a list of positions.\n\r", ch);
            return false;
        }

        public static bool medit_gold(CharData ch, string argument)
        {
            argument ??= "";
            var pMob = (MobIndexData)ch.desc.pEdit;

            if (argument.Length == 0 || !Interp.is_number(argument))
            {
                Comm.send_to_char("Syntax:  wealth [number]\n\r", ch);
                return false;
            }

            pMob.wealth = Interp.atoi(argument);

            Comm.send_to_char("Wealth set.\n\r", ch);
            return true;
        }

        public static bool medit_hitroll(CharData ch, string argument)
        {
            argument ??= "";
            var pMob = (MobIndexData)ch.desc.pEdit;

            if (argument.Length == 0 || !Interp.is_number(argument))
            {
                Comm.send_to_char("Syntax:  hitroll [number]\n\r", ch);
                return false;
            }

            pMob.hitroll = Interp.atoi(argument);

            Comm.send_to_char("Hitroll set.\n\r", ch);
            return true;
        }

        public static bool medit_group(CharData ch, string argument)
        {
            argument ??= "";
            var pMob = (MobIndexData)ch.desc.pEdit;
            bool found = false;

            if (argument.Length == 0)
            {
                Comm.send_to_char("Syntax: group [number]\n\r", ch);
                Comm.send_to_char("        group show [number]\n\r", ch);
                return false;
            }

            if (Interp.is_number(argument))
            {
                pMob.group = Interp.atoi(argument);
                Comm.send_to_char("Group set.\n\r", ch);
                return true;
            }

            argument = RomString.one_argument(argument, out string arg);

            if (arg == "show" && Interp.is_number(argument))
            {
                if (Interp.atoi(argument) == 0)
                {
                    Comm.send_to_char("Are you crazy?\n\r", ch);
                    return false;
                }

                var buffer = new System.Text.StringBuilder();

                for (int temp = 0; temp < 65536; temp++)
                {
                    var pMTemp = Handler.get_mob_index(temp);
                    if (pMTemp != null && (pMTemp.group == Interp.atoi(argument)))
                    {
                        found = true;
                        string buf = RomString.sprintf("[%5d] %s\n\r", pMTemp.vnum,
                            pMTemp.player_name);
                        buffer.Append(buf);
                    }
                }

                if (found)
                    Comm.page_to_char(buffer.ToString(), ch);
                else
                    Comm.send_to_char("No mobs in that group.\n\r", ch);

                return false;
            }

            return false;
        }

        public static bool medit_addmprog(CharData ch, string argument)
        {
            argument ??= "";
            int value;
            var pMob = (MobIndexData)ch.desc.pEdit;
            MprogCode code;

            argument = RomString.one_argument(argument, out string num);
            argument = RomString.one_argument(argument, out string trigger);
            argument = RomString.one_argument(argument, out string phrase);

            if (!Interp.is_number(num) || trigger.Length == 0 || phrase.Length == 0)
            {
                Comm.send_to_char("Syntax:   addmprog [vnum] [trigger] [phrase]\n\r", ch);
                return false;
            }

            if ((value = Lookup.flag_value(Tables.mprog_flags, trigger)) == NO_FLAG)
            {
                Comm.send_to_char("Valid flags are:\n\r", ch);
                show_help(ch, "mprog");
                return false;
            }

            if ((code = Db.get_mprog_index(Interp.atoi(num))) == null)
            {
                Comm.send_to_char("No such MOBProgram.\n\r", ch);
                return false;
            }

            var list = new MprogList();
            list.vnum = Interp.atoi(num);
            list.trig_type = value;
            list.trig_phrase = phrase;
            list.code = code.code;
            Bit.SET_BIT(ref pMob.mprog_flags, value);
            list.next = pMob.mprogs;
            pMob.mprogs = list;

            Comm.send_to_char("Mprog Added.\n\r", ch);
            return true;
        }

        public static bool medit_delmprog(CharData ch, string argument)
        {
            argument ??= "";
            var pMob = (MobIndexData)ch.desc.pEdit;
            MprogList list;
            MprogList list_next;
            int value;
            int cnt = 0;

            RomString.one_argument(argument, out string mprog);
            if (!Interp.is_number(mprog) || mprog.Length == 0)
            {
                Comm.send_to_char("Syntax:  delmprog [#mprog]\n\r", ch);
                return false;
            }

            value = Interp.atoi(mprog);

            if (value < 0)
            {
                Comm.send_to_char("Only non-negative mprog-numbers allowed.\n\r", ch);
                return false;
            }

            if ((list = pMob.mprogs) == null)
            {
                Comm.send_to_char("MEdit:  Non existant mprog.\n\r", ch);
                return false;
            }

            if (value == 0)
            {
                Bit.REMOVE_BIT(ref pMob.mprog_flags, pMob.mprogs.trig_type);
                list = pMob.mprogs;
                pMob.mprogs = list.next;
            }
            else
            {
                while ((list_next = list.next) != null && (++cnt < value))
                    list = list_next;

                if (list_next != null)
                {
                    Bit.REMOVE_BIT(ref pMob.mprog_flags, list_next.trig_type);
                    list.next = list_next.next;
                }
                else
                {
                    Comm.send_to_char("No such mprog.\n\r", ch);
                    return false;
                }
            }

            Comm.send_to_char("Mprog removed.\n\r", ch);
            return true;
        }
    }
}
