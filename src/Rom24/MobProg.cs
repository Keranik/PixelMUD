using static Rom24.Merc;

namespace Rom24
{
    public static class MobProg
    {
        const int CHK_RAND = 0;
        const int CHK_MOBHERE = 1;
        const int CHK_OBJHERE = 2;
        const int CHK_MOBEXISTS = 3;
        const int CHK_OBJEXISTS = 4;
        const int CHK_PEOPLE = 5;
        const int CHK_PLAYERS = 6;
        const int CHK_MOBS = 7;
        const int CHK_CLONES = 8;
        const int CHK_ORDER = 9;
        const int CHK_HOUR = 10;
        const int CHK_ISPC = 11;
        const int CHK_ISNPC = 12;
        const int CHK_ISGOOD = 13;
        const int CHK_ISEVIL = 14;
        const int CHK_ISNEUTRAL = 15;
        const int CHK_ISIMMORT = 16;
        const int CHK_ISCHARM = 17;
        const int CHK_ISFOLLOW = 18;
        const int CHK_ISACTIVE = 19;
        const int CHK_ISDELAY = 20;
        const int CHK_ISVISIBLE = 21;
        const int CHK_HASTARGET = 22;
        const int CHK_ISTARGET = 23;
        const int CHK_EXISTS = 24;
        const int CHK_AFFECTED = 25;
        const int CHK_ACT = 26;
        const int CHK_OFF = 27;
        const int CHK_IMM = 28;
        const int CHK_CARRIES = 29;
        const int CHK_WEARS = 30;
        const int CHK_HAS = 31;
        const int CHK_USES = 32;
        const int CHK_NAME = 33;
        const int CHK_POS = 34;
        const int CHK_CLAN = 35;
        const int CHK_RACE = 36;
        const int CHK_CLASS = 37;
        const int CHK_OBJTYPE = 38;
        const int CHK_VNUM = 39;
        const int CHK_HPCNT = 40;
        const int CHK_ROOM = 41;
        const int CHK_SEX = 42;
        const int CHK_LEVEL = 43;
        const int CHK_ALIGN = 44;
        const int CHK_MONEY = 45;
        const int CHK_OBJVAL0 = 46;
        const int CHK_OBJVAL1 = 47;
        const int CHK_OBJVAL2 = 48;
        const int CHK_OBJVAL3 = 49;
        const int CHK_OBJVAL4 = 50;
        const int CHK_GRPSIZE = 51;

        const int EVAL_EQ = 0;
        const int EVAL_GE = 1;
        const int EVAL_LE = 2;
        const int EVAL_GT = 3;
        const int EVAL_LT = 4;
        const int EVAL_NE = 5;

        const int MAX_NESTED_LEVEL = 12;
        const int BEGIN_BLOCK = 0;
        const int IN_BLOCK = -1;
        const int END_BLOCK = -2;
        const int MAX_CALL_LEVEL = 5;

        static int call_level;

        static readonly string[] fn_keyword =
        {
            "rand",
            "mobhere",
            "objhere",
            "mobexists",
            "objexists",

            "people",
            "players",
            "mobs",
            "clones",
            "order",
            "hour",

            "ispc",
            "isnpc",
            "isgood",
            "isevil",
            "isneutral",
            "isimmort",
            "ischarm",
            "isfollow",
            "isactive",
            "isdelay",
            "isvisible",
            "hastarget",
            "istarget",
            "exists",

            "affected",
            "act",
            "off",
            "imm",
            "carries",
            "wears",
            "has",
            "uses",
            "name",
            "pos",
            "clan",
            "race",
            "class",
            "objtype",

            "vnum",
            "hpcnt",
            "room",
            "sex",
            "level",
            "align",
            "money",
            "objval0",
            "objval1",
            "objval2",
            "objval3",
            "objval4",
            "grpsize",

            "\n"
        };

        static readonly string[] fn_evals =
        {
            "==",
            ">=",
            "<=",
            ">",
            "<",
            "!=",
            "\n"
        };

        static int keyword_lookup(string[] table, string keyword)
        {
            for (int i = 0; table[i][0] != '\n'; i++)
                if (!RomString.str_cmp(table[i], keyword))
                    return i;
            return -1;
        }

        static int num_eval(int lval, int oper, int rval)
        {
            switch (oper)
            {
                case EVAL_EQ:
                    return lval == rval ? 1 : 0;
                case EVAL_GE:
                    return lval >= rval ? 1 : 0;
                case EVAL_LE:
                    return lval <= rval ? 1 : 0;
                case EVAL_NE:
                    return lval != rval ? 1 : 0;
                case EVAL_GT:
                    return lval > rval ? 1 : 0;
                case EVAL_LT:
                    return lval < rval ? 1 : 0;
                default:
                    Db.bug("num_eval: invalid oper", 0);
                    return 0;
            }
        }

        static CharData get_random_char(CharData mob)
        {
            CharData victim = null;
            int now = 0, highest = 0;
            for (var vch = mob.in_room.people; vch != null; vch = vch.next_in_room)
            {
                if (mob != vch && !Bit.IS_NPC(vch)
                    && Handler.can_see(mob, vch) && (now = RomRandom.number_percent()) > highest)
                {
                    victim = vch;
                    highest = now;
                }
            }
            return victim;
        }

        static int count_people_room(CharData mob, int iFlag)
        {
            int count = 0;
            for (var vch = mob.in_room.people; vch != null; vch = vch.next_in_room)
                if (mob != vch && (iFlag == 0 || (iFlag == 1 && !Bit.IS_NPC(vch))
                                   || (iFlag == 2 && Bit.IS_NPC(vch))
                                   || (iFlag == 3 && Bit.IS_NPC(mob) && Bit.IS_NPC(vch)
                                       && mob.pIndexData.vnum ==
                                       vch.pIndexData.vnum) || (iFlag == 4
                                                                  &&
                                                                  Handler.is_same_group(mob,
                                                                     vch)))
                    && Handler.can_see(mob, vch))
                    count++;
            return count;
        }

        static int get_order(CharData ch)
        {
            if (!Bit.IS_NPC(ch))
                return 0;
            int i = 0;
            for (var vch = ch.in_room.people; vch != null; vch = vch.next_in_room)
            {
                if (vch == ch)
                    return i;
                if (Bit.IS_NPC(vch) && vch.pIndexData.vnum == ch.pIndexData.vnum)
                    i++;
            }
            return 0;
        }

        static bool has_item(CharData ch, int vnum, int item_type, bool fWear)
        {
            for (var obj = ch.carrying; obj != null; obj = obj.next_content)
                if ((vnum < 0 || obj.pIndexData.vnum == vnum)
                    && (item_type < 0 || obj.pIndexData.item_type == item_type)
                    && (!fWear || obj.wear_loc != WEAR_NONE))
                    return true;
            return false;
        }

        static bool get_mob_vnum_room(CharData ch, int vnum)
        {
            for (var mob = ch.in_room.people; mob != null; mob = mob.next_in_room)
                if (Bit.IS_NPC(mob) && mob.pIndexData.vnum == vnum)
                    return true;
            return false;
        }

        static bool get_obj_vnum_room(CharData ch, int vnum)
        {
            for (var obj = ch.in_room.contents; obj != null; obj = obj.next_content)
                if (obj.pIndexData.vnum == vnum)
                    return true;
            return false;
        }

        static int cmd_eval(int vnum, string line, int check,
            CharData mob, CharData ch,
            object arg1, object arg2, CharData rch)
        {
            CharData lval_char = mob;
            CharData vch = arg2 as CharData;
            ObjData obj1 = arg1 as ObjData;
            ObjData obj2 = arg2 as ObjData;
            ObjData lval_obj = null;

            string original;
            char code;
            int lval = 0, oper = 0, rval = -1;

            original = line ?? "";
            line = RomString.one_argument(original, out string buf);
            if (buf.Length == 0 || mob == null)
                return 0;

            if (mob.mprog_target == null)
                mob.mprog_target = ch;

            switch (check)
            {
                case CHK_RAND:
                    return Interp.atoi(buf) < RomRandom.number_percent() ? 1 : 0;
                case CHK_MOBHERE:
                    if (Interp.is_number(buf))
                        return get_mob_vnum_room(mob, Interp.atoi(buf)) ? 1 : 0;
                    else
                        return Handler.get_char_room(mob, buf) != null ? 1 : 0;
                case CHK_OBJHERE:
                    if (Interp.is_number(buf))
                        return get_obj_vnum_room(mob, Interp.atoi(buf)) ? 1 : 0;
                    else
                        return Handler.get_obj_here(mob, buf) != null ? 1 : 0;
                case CHK_MOBEXISTS:
                    return Handler.get_char_world(mob, buf) != null ? 1 : 0;
                case CHK_OBJEXISTS:
                    return Handler.get_obj_world(mob, buf) != null ? 1 : 0;
                case CHK_PEOPLE:
                    rval = count_people_room(mob, 0);
                    break;
                case CHK_PLAYERS:
                    rval = count_people_room(mob, 1);
                    break;
                case CHK_MOBS:
                    rval = count_people_room(mob, 2);
                    break;
                case CHK_CLONES:
                    rval = count_people_room(mob, 3);
                    break;
                case CHK_ORDER:
                    rval = get_order(mob);
                    break;
                case CHK_HOUR:
                    rval = Game.time_info.hour;
                    break;
                default:
                    break;
            }

            if (rval >= 0)
            {
                if ((oper = keyword_lookup(fn_evals, buf)) < 0)
                {
                    Db.bug(RomString.sprintf("Cmd_eval: prog %d syntax error(2) '%s'",
                        vnum, original), 0);
                    return 0;
                }
                RomString.one_argument(line, out buf);
                lval = rval;
                rval = Interp.atoi(buf);
                return num_eval(lval, oper, rval);
            }

            if (buf.Length == 0 || buf[0] != '$' || buf.Length < 2)
            {
                Db.bug(RomString.sprintf("Cmd_eval: prog %d syntax error(3) '%s'",
                    vnum, original), 0);
                return 0;
            }
            else
                code = buf[1];
            switch (code)
            {
                case 'i':
                    lval_char = mob;
                    break;
                case 'n':
                    lval_char = ch;
                    break;
                case 't':
                    lval_char = vch;
                    break;
                case 'r':
                    lval_char = rch == null ? get_random_char(mob) : rch;
                    break;
                case 'o':
                    lval_obj = obj1;
                    break;
                case 'p':
                    lval_obj = obj2;
                    break;
                case 'q':
                    lval_char = mob.mprog_target;
                    break;
                default:
                    Db.bug(RomString.sprintf("Cmd_eval: prog %d syntax error(4) '%s'",
                        vnum, original), 0);
                    return 0;
            }

            if (lval_char == null && lval_obj == null)
                return 0;

            switch (check)
            {
                case CHK_ISPC:
                    return lval_char != null && !Bit.IS_NPC(lval_char) ? 1 : 0;
                case CHK_ISNPC:
                    return lval_char != null && Bit.IS_NPC(lval_char) ? 1 : 0;
                case CHK_ISGOOD:
                    return lval_char != null && Bit.IS_GOOD(lval_char) ? 1 : 0;
                case CHK_ISEVIL:
                    return lval_char != null && Bit.IS_EVIL(lval_char) ? 1 : 0;
                case CHK_ISNEUTRAL:
                    return lval_char != null && Bit.IS_NEUTRAL(lval_char) ? 1 : 0;
                case CHK_ISIMMORT:
                    return lval_char != null && Bit.IS_IMMORTAL(lval_char) ? 1 : 0;
                case CHK_ISCHARM:
                    return lval_char != null && Bit.IS_AFFECTED(lval_char, AFF_CHARM) ? 1 : 0;
                case CHK_ISFOLLOW:
                    return lval_char != null && lval_char.master != null
                        && lval_char.master.in_room == lval_char.in_room ? 1 : 0;
                case CHK_ISACTIVE:
                    return lval_char != null && lval_char.position > POS_SLEEPING ? 1 : 0;
                case CHK_ISDELAY:
                    return lval_char != null && lval_char.mprog_delay > 0 ? 1 : 0;
                case CHK_ISVISIBLE:
                    switch (code)
                    {
                        default:
                        case 'i':
                        case 'n':
                        case 't':
                        case 'r':
                        case 'q':
                            return lval_char != null && Handler.can_see(mob, lval_char) ? 1 : 0;
                        case 'o':
                        case 'p':
                            return lval_obj != null && Handler.can_see_obj(mob, lval_obj) ? 1 : 0;
                    }
                case CHK_HASTARGET:
                    return lval_char != null && lval_char.mprog_target != null
                        && lval_char.in_room ==
                        lval_char.mprog_target.in_room ? 1 : 0;
                case CHK_ISTARGET:
                    return lval_char != null && mob.mprog_target == lval_char ? 1 : 0;
                default:
                    break;
            }

            line = RomString.one_argument(line, out buf);
            switch (check)
            {
                case CHK_AFFECTED:
                    return lval_char != null
                        && Bit.IS_SET(lval_char.affected_by,
                            Lookup.flag_lookup(buf, Tables.affect_flags)) ? 1 : 0;
                case CHK_ACT:
                    return lval_char != null
                        && Bit.IS_SET(lval_char.act, Lookup.flag_lookup(buf, Tables.act_flags)) ? 1 : 0;
                case CHK_IMM:
                    return lval_char != null
                        && Bit.IS_SET(lval_char.imm_flags,
                            Lookup.flag_lookup(buf, Tables.imm_flags)) ? 1 : 0;
                case CHK_OFF:
                    return lval_char != null
                        && Bit.IS_SET(lval_char.off_flags,
                            Lookup.flag_lookup(buf, Tables.off_flags)) ? 1 : 0;
                case CHK_CARRIES:
                    if (Interp.is_number(buf))
                        return lval_char != null
                            && has_item(lval_char, Interp.atoi(buf), -1, false) ? 1 : 0;
                    else
                        return lval_char != null
                            && Handler.get_obj_carry(lval_char, buf, lval_char) !=
                            null ? 1 : 0;
                case CHK_WEARS:
                    if (Interp.is_number(buf))
                        return lval_char != null
                            && has_item(lval_char, Interp.atoi(buf), -1, true) ? 1 : 0;
                    else
                        return lval_char != null
                            && Handler.get_obj_wear(lval_char, buf) != null ? 1 : 0;
                case CHK_HAS:
                    return lval_char != null
                        && has_item(lval_char, -1, Lookup.item_lookup(buf), false) ? 1 : 0;
                case CHK_USES:
                    return lval_char != null
                        && has_item(lval_char, -1, Lookup.item_lookup(buf), true) ? 1 : 0;
                case CHK_NAME:
                    switch (code)
                    {
                        default:
                        case 'i':
                        case 'n':
                        case 't':
                        case 'r':
                        case 'q':
                            return lval_char != null
                                && Handler.is_name(buf, lval_char.name) ? 1 : 0;
                        case 'o':
                        case 'p':
                            return lval_obj != null
                                && Handler.is_name(buf, lval_obj.name) ? 1 : 0;
                    }
                case CHK_POS:
                    return lval_char != null
                        && lval_char.position == Lookup.position_lookup(buf) ? 1 : 0;
                case CHK_CLAN:
                    return lval_char != null
                        && lval_char.clan == Lookup.clan_lookup(buf) ? 1 : 0;
                case CHK_RACE:
                    return lval_char != null
                        && lval_char.race == Lookup.race_lookup(buf) ? 1 : 0;
                case CHK_CLASS:
                    return lval_char != null
                        && lval_char.klass == Lookup.class_lookup(buf) ? 1 : 0;
                case CHK_OBJTYPE:
                    return lval_obj != null
                        && lval_obj.item_type == Lookup.item_lookup(buf) ? 1 : 0;
                default:
                    break;
            }

            if ((oper = keyword_lookup(fn_evals, buf)) < 0)
            {
                Db.bug(RomString.sprintf("Cmd_eval: prog %d syntax error(5): '%s'",
                    vnum, original), 0);
                return 0;
            }
            RomString.one_argument(line, out buf);
            rval = Interp.atoi(buf);

            switch (check)
            {
                case CHK_VNUM:
                    switch (code)
                    {
                        default:
                        case 'i':
                        case 'n':
                        case 't':
                        case 'r':
                        case 'q':
                            if (lval_char != null && Bit.IS_NPC(lval_char))
                                lval = lval_char.pIndexData.vnum;
                            break;
                        case 'o':
                        case 'p':
                            if (lval_obj != null)
                                lval = lval_obj.pIndexData.vnum;
                            break;
                    }
                    break;
                case CHK_HPCNT:
                    if (lval_char != null)
                        lval =
                            (lval_char.hit * 100) / (Bit.UMAX(1, lval_char.max_hit));
                    break;
                case CHK_ROOM:
                    if (lval_char != null && lval_char.in_room != null)
                        lval = lval_char.in_room.vnum;
                    break;
                case CHK_SEX:
                    if (lval_char != null)
                        lval = lval_char.sex;
                    break;
                case CHK_LEVEL:
                    if (lval_char != null)
                        lval = lval_char.level;
                    break;
                case CHK_ALIGN:
                    if (lval_char != null)
                        lval = lval_char.alignment;
                    break;
                case CHK_MONEY:
                    if (lval_char != null)
                        lval = (int)(lval_char.gold + (lval_char.silver * 100));
                    break;
                case CHK_OBJVAL0:
                    if (lval_obj != null)
                        lval = lval_obj.value[0];
                    break;
                case CHK_OBJVAL1:
                    if (lval_obj != null)
                        lval = lval_obj.value[1];
                    break;
                case CHK_OBJVAL2:
                    if (lval_obj != null)
                        lval = lval_obj.value[2];
                    break;
                case CHK_OBJVAL3:
                    if (lval_obj != null)
                        lval = lval_obj.value[3];
                    break;
                case CHK_OBJVAL4:
                    if (lval_obj != null)
                        lval = lval_obj.value[4];
                    break;
                case CHK_GRPSIZE:
                    if (lval_char != null)
                        lval = count_people_room(lval_char, 4);
                    break;
                default:
                    return 0;
            }
            return num_eval(lval, oper, rval);
        }

        static string expand_arg(string format,
            CharData mob, CharData ch,
            object arg1, object arg2, CharData rch)
        {
            string[] he_she = { "it", "he", "she" };
            string[] him_her = { "it", "him", "her" };
            string[] his_her = { "its", "his", "her" };
            const string someone = "someone";
            const string something = "something";
            const string someones = "someone's";

            CharData vch = arg2 as CharData;
            ObjData obj1 = arg1 as ObjData;
            ObjData obj2 = arg2 as ObjData;

            if (format == null || format.Length == 0)
                return format ?? "";

            var point = new System.Text.StringBuilder();
            int stri = 0;
            while (stri < format.Length)
            {
                if (format[stri] != '$')
                {
                    point.Append(format[stri++]);
                    continue;
                }
                ++stri;

                char sc = stri < format.Length ? format[stri] : '\0';
                string i;
                switch (sc)
                {
                    default:
                        Db.bug("Expand_arg: bad code %d.", (int)sc);
                        i = " <@@@> ";
                        break;
                    case 'i':
                        RomString.one_argument(mob.name, out string fnamei);
                        i = fnamei;
                        break;
                    case 'I':
                        i = mob.short_descr;
                        break;
                    case 'n':
                        i = someone;
                        if (ch != null && Handler.can_see(mob, ch))
                        {
                            RomString.one_argument(ch.name, out string fnamen);
                            i = RomString.capitalize(fnamen);
                        }
                        break;
                    case 'N':
                        i = (ch != null && Handler.can_see(mob, ch))
                            ? (Bit.IS_NPC(ch) ? ch.short_descr : ch.name) : someone;
                        break;
                    case 't':
                        i = someone;
                        if (vch != null && Handler.can_see(mob, vch))
                        {
                            RomString.one_argument(vch.name, out string fnamet);
                            i = RomString.capitalize(fnamet);
                        }
                        break;
                    case 'T':
                        i = (vch != null && Handler.can_see(mob, vch))
                            ? (Bit.IS_NPC(vch) ? vch.short_descr : vch.name) : someone;
                        break;
                    case 'r':
                        if (rch == null)
                            rch = get_random_char(mob);
                        i = someone;
                        if (rch != null && Handler.can_see(mob, rch))
                        {
                            RomString.one_argument(rch.name, out string fnamer);
                            i = RomString.capitalize(fnamer);
                        }
                        break;
                    case 'R':
                        if (rch == null)
                            rch = get_random_char(mob);
                        i = (rch != null && Handler.can_see(mob, rch))
                            ? (Bit.IS_NPC(ch) ? ch.short_descr : ch.name) : someone;
                        break;
                    case 'q':
                        i = someone;
                        if (mob.mprog_target != null
                            && Handler.can_see(mob, mob.mprog_target))
                        {
                            RomString.one_argument(mob.mprog_target.name, out string fnameq);
                            i = RomString.capitalize(fnameq);
                        }
                        break;
                    case 'Q':
                        i = (mob.mprog_target != null
                             && Handler.can_see(mob,
                                 mob.mprog_target)) ? (Bit.IS_NPC(mob.mprog_target)
                                                   ? mob.mprog_target.short_descr :
                                                   mob.mprog_target.name) :
                            someone;
                        break;
                    case 'j':
                        i = he_she[Bit.URANGE(0, mob.sex, 2)];
                        break;
                    case 'e':
                        i = (ch != null && Handler.can_see(mob, ch))
                            ? he_she[Bit.URANGE(0, ch.sex, 2)] : someone;
                        break;
                    case 'E':
                        i = (vch != null && Handler.can_see(mob, vch))
                            ? he_she[Bit.URANGE(0, vch.sex, 2)] : someone;
                        break;
                    case 'J':
                        i = (rch != null && Handler.can_see(mob, rch))
                            ? he_she[Bit.URANGE(0, rch.sex, 2)] : someone;
                        break;
                    case 'X':
                        i = (mob.mprog_target != null
                             && Handler.can_see(mob, mob.mprog_target)) ? he_she[Bit.URANGE(0,
                                                                           mob.mprog_target.sex,
                                                                           2)]
                            : someone;
                        break;
                    case 'k':
                        i = him_her[Bit.URANGE(0, mob.sex, 2)];
                        break;
                    case 'm':
                        i = (ch != null && Handler.can_see(mob, ch))
                            ? him_her[Bit.URANGE(0, ch.sex, 2)] : someone;
                        break;
                    case 'M':
                        i = (vch != null && Handler.can_see(mob, vch))
                            ? him_her[Bit.URANGE(0, vch.sex, 2)] : someone;
                        break;
                    case 'K':
                        if (rch == null)
                            rch = get_random_char(mob);
                        i = (rch != null && Handler.can_see(mob, rch))
                            ? him_her[Bit.URANGE(0, rch.sex, 2)] : someone;
                        break;
                    case 'Y':
                        i = (mob.mprog_target != null
                             && Handler.can_see(mob, mob.mprog_target)) ? him_her[Bit.URANGE(0,
                                                                            mob.mprog_target.sex,
                                                                            2)]
                            : someone;
                        break;
                    case 'l':
                        i = his_her[Bit.URANGE(0, mob.sex, 2)];
                        break;
                    case 's':
                        i = (ch != null && Handler.can_see(mob, ch))
                            ? his_her[Bit.URANGE(0, ch.sex, 2)] : someones;
                        break;
                    case 'S':
                        i = (vch != null && Handler.can_see(mob, vch))
                            ? his_her[Bit.URANGE(0, vch.sex, 2)] : someones;
                        break;
                    case 'L':
                        if (rch == null)
                            rch = get_random_char(mob);
                        i = (rch != null && Handler.can_see(mob, rch))
                            ? his_her[Bit.URANGE(0, rch.sex, 2)] : someones;
                        break;
                    case 'Z':
                        i = (mob.mprog_target != null
                             && Handler.can_see(mob, mob.mprog_target)) ? his_her[Bit.URANGE(0,
                                                                            mob.mprog_target.sex,
                                                                            2)]
                            : someones;
                        break;
                    case 'o':
                        i = something;
                        if (obj1 != null && Handler.can_see_obj(mob, obj1))
                        {
                            RomString.one_argument(obj1.name, out string fnameo);
                            i = fnameo;
                        }
                        break;
                    case 'O':
                        i = (obj1 != null && Handler.can_see_obj(mob, obj1))
                            ? obj1.short_descr : something;
                        break;
                    case 'p':
                        i = something;
                        if (obj2 != null && Handler.can_see_obj(mob, obj2))
                        {
                            RomString.one_argument(obj2.name, out string fnamep);
                            i = fnamep;
                        }
                        break;
                    case 'P':
                        i = (obj2 != null && Handler.can_see_obj(mob, obj2))
                            ? obj2.short_descr : something;
                        break;
                }

                if (stri < format.Length)
                    ++stri;
                point.Append(i);
            }

            return point.ToString();
        }

        static bool isspace(char c)
        {
            return c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\f' || c == '\v';
        }

        public static void program_flow(int pvnum,
            string source,
            CharData mob, CharData ch, object arg1,
            object arg2)
        {
            CharData rch = null;
            source ??= "";

            int[] state = new int[MAX_NESTED_LEVEL];
            int[] cond = new int[MAX_NESTED_LEVEL];

            int mvnum = mob.pIndexData.vnum;

            if (++call_level > MAX_CALL_LEVEL)
            {
                Db.bug("MOBprogs: MAX_CALL_LEVEL exceeded, vnum %d",
                    mob.pIndexData.vnum);
                return;
            }

            for (int leveli = 0; leveli < MAX_NESTED_LEVEL; leveli++)
            {
                state[leveli] = IN_BLOCK;
                cond[leveli] = 1;
            }
            int level = 0;

            int codePos = 0;
            while (codePos < source.Length)
            {
                bool first_arg = true;
                var b = new System.Text.StringBuilder();
                var c = new System.Text.StringBuilder();
                var d = new System.Text.StringBuilder();

                while (codePos < source.Length && isspace(source[codePos]))
                    codePos++;
                while (codePos < source.Length)
                {
                    char chs = source[codePos];
                    if (chs == '\n' || chs == '\r')
                        break;
                    else if (isspace(chs))
                    {
                        if (first_arg)
                            first_arg = false;
                        else
                            d.Append(chs);
                    }
                    else
                    {
                        if (first_arg)
                            c.Append(chs);
                        else
                            d.Append(chs);
                    }
                    b.Append(chs);
                    codePos++;
                }

                string buf = b.ToString();
                string control = c.ToString();
                string data = d.ToString();

                if (buf.Length == 0)
                    break;
                if (buf[0] == '*')
                    continue;

                string line = data;

                if (!RomString.str_cmp(control, "if"))
                {
                    if (state[level] == BEGIN_BLOCK)
                    {
                        Db.bug(RomString.sprintf(
                            "Mobprog: misplaced if statement, mob %d prog %d",
                            mvnum, pvnum), 0);
                        return;
                    }
                    state[level] = BEGIN_BLOCK;
                    if (++level >= MAX_NESTED_LEVEL)
                    {
                        Db.bug(RomString.sprintf(
                            "Mobprog: Max nested level exceeded, mob %d prog %d",
                            mvnum, pvnum), 0);
                        return;
                    }
                    if (level != 0 && cond[level - 1] == 0)
                    {
                        cond[level] = 0;
                        continue;
                    }
                    line = RomString.one_argument(line, out control);
                    int check;
                    if ((check = keyword_lookup(fn_keyword, control)) >= 0)
                    {
                        cond[level] =
                            cmd_eval(pvnum, line, check, mob, ch, arg1, arg2, rch);
                    }
                    else
                    {
                        Db.bug(RomString.sprintf(
                            "Mobprog: invalid if_check (if), mob %d prog %d",
                            mvnum, pvnum), 0);
                        return;
                    }
                    state[level] = END_BLOCK;
                }
                else if (!RomString.str_cmp(control, "or"))
                {
                    if (level == 0 || state[level - 1] != BEGIN_BLOCK)
                    {
                        Db.bug(RomString.sprintf("Mobprog: or without if, mob %d prog %d",
                            mvnum, pvnum), 0);
                        return;
                    }
                    if (level != 0 && cond[level - 1] == 0)
                        continue;
                    line = RomString.one_argument(line, out control);
                    int check;
                    int eval;
                    if ((check = keyword_lookup(fn_keyword, control)) >= 0)
                    {
                        eval =
                            cmd_eval(pvnum, line, check, mob, ch, arg1, arg2, rch);
                    }
                    else
                    {
                        Db.bug(RomString.sprintf(
                            "Mobprog: invalid if_check (or), mob %d prog %d",
                            mvnum, pvnum), 0);
                        return;
                    }
                    cond[level] = (eval != 0) ? 1 : cond[level];
                }
                else if (!RomString.str_cmp(control, "and"))
                {
                    if (level == 0 || state[level - 1] != BEGIN_BLOCK)
                    {
                        Db.bug(RomString.sprintf("Mobprog: and without if, mob %d prog %d",
                            mvnum, pvnum), 0);
                        return;
                    }
                    if (level != 0 && cond[level - 1] == 0)
                        continue;
                    line = RomString.one_argument(line, out control);
                    int check;
                    int eval;
                    if ((check = keyword_lookup(fn_keyword, control)) >= 0)
                    {
                        eval =
                            cmd_eval(pvnum, line, check, mob, ch, arg1, arg2, rch);
                    }
                    else
                    {
                        Db.bug(RomString.sprintf(
                            "Mobprog: invalid if_check (and), mob %d prog %d",
                            mvnum, pvnum), 0);
                        return;
                    }
                    cond[level] = (cond[level] != 0)
                        && (eval != 0) ? 1 : 0;
                }
                else if (!RomString.str_cmp(control, "endif"))
                {
                    if (level == 0 || state[level - 1] != BEGIN_BLOCK)
                    {
                        Db.bug(RomString.sprintf("Mobprog: endif without if, mob %d prog %d",
                            mvnum, pvnum), 0);
                        return;
                    }
                    cond[level] = 1;
                    state[level] = IN_BLOCK;
                    state[--level] = END_BLOCK;
                }
                else if (!RomString.str_cmp(control, "else"))
                {
                    if (level == 0 || state[level - 1] != BEGIN_BLOCK)
                    {
                        Db.bug(RomString.sprintf("Mobprog: else without if, mob %d prog %d",
                            mvnum, pvnum), 0);
                        return;
                    }
                    if (level != 0 && cond[level - 1] == 0)
                        continue;
                    state[level] = IN_BLOCK;
                    cond[level] = (cond[level] != 0) ? 0 : 1;
                }
                else if (cond[level] != 0
                         && (!RomString.str_cmp(control, "break")
                             || !RomString.str_cmp(control, "end")))
                {
                    call_level--;
                    return;
                }
                else if ((level == 0 || cond[level] != 0) && buf.Length != 0)
                {
                    state[level] = IN_BLOCK;
                    data = expand_arg(buf, mob, ch, arg1, arg2, rch);
                    if (!RomString.str_cmp(control, "mob"))
                    {
                        line = RomString.one_argument(data, out control);
                        MobCmds.mob_interpret(mob, line);
                    }
                    else
                    {
                        Interp.interpret(mob, data);
                    }
                }
            }
            call_level--;
        }

        public static void mp_act_trigger(
            string argument, CharData mob, CharData ch,
            object arg1, object arg2, int type)
        {
            if (!Bit.IS_NPC(mob)) return;

            argument ??= "";
            for (var prg = mob.pIndexData.mprogs; prg != null; prg = prg.next)
            {
                if (prg.trig_type == type
                    && argument.IndexOf(prg.trig_phrase ?? "", StringComparison.Ordinal) >= 0)
                {
                    program_flow(prg.vnum, prg.code, mob, ch, arg1, arg2);
                    break;
                }
            }
        }

        public static bool mp_percent_trigger(CharData mob, CharData ch,
            object arg1, object arg2, int type)
        {
            for (var prg = mob.pIndexData.mprogs; prg != null; prg = prg.next)
            {
                if (prg.trig_type == type
                    && RomRandom.number_percent() < Interp.atoi(prg.trig_phrase))
                {
                    program_flow(prg.vnum, prg.code, mob, ch, arg1, arg2);
                    return true;
                }
            }
            return false;
        }

        public static void mp_bribe_trigger(CharData mob, CharData ch, int amount)
        {
            for (var prg = mob.pIndexData.mprogs; prg != null; prg = prg.next)
            {
                if (prg.trig_type == (int)TRIG_BRIBE && amount >= Interp.atoi(prg.trig_phrase))
                {
                    program_flow(prg.vnum, prg.code, mob, ch, null, null);
                    break;
                }
            }
        }

        public static bool mp_exit_trigger(CharData ch, int dir)
        {
            for (var mob = ch.in_room.people; mob != null; mob = mob.next_in_room)
            {
                if (Bit.IS_NPC(mob)
                    && (Bit.IS_SET(mob.pIndexData.mprog_flags, TRIG_EXIT)
                        || Bit.IS_SET(mob.pIndexData.mprog_flags, TRIG_EXALL)))
                {
                    for (var prg = mob.pIndexData.mprogs; prg != null; prg = prg.next)
                    {
                        if (prg.trig_type == (int)TRIG_EXIT
                            && dir == Interp.atoi(prg.trig_phrase)
                            && mob.position == mob.pIndexData.default_pos
                            && Handler.can_see(mob, ch))
                        {
                            program_flow(prg.vnum, prg.code, mob, ch, null, null);
                            return true;
                        }
                        else if (prg.trig_type == (int)TRIG_EXALL
                            && dir == Interp.atoi(prg.trig_phrase))
                        {
                            program_flow(prg.vnum, prg.code, mob, ch, null, null);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static void mp_give_trigger(CharData mob, CharData ch, ObjData obj)
        {
            for (var prg = mob.pIndexData.mprogs; prg != null; prg = prg.next)
                if (prg.trig_type == (int)TRIG_GIVE)
                {
                    string p = prg.trig_phrase;
                    if (Interp.is_number(p))
                    {
                        if (obj.pIndexData.vnum == Interp.atoi(p))
                        {
                            program_flow(prg.vnum, prg.code, mob, ch, obj,
                                null);
                            return;
                        }
                    }
                    else
                    {
                        while (p.Length != 0)
                        {
                            p = RomString.one_argument(p, out string buf);

                            if (Handler.is_name(buf, obj.name) || !RomString.str_cmp("all", buf))
                            {
                                program_flow(prg.vnum, prg.code, mob, ch,
                                    obj, null);
                                return;
                            }
                        }
                    }
                }
        }

        public static void mp_greet_trigger(CharData ch)
        {
            for (var mob = ch.in_room.people; mob != null; mob = mob.next_in_room)
            {
                if (Bit.IS_NPC(mob)
                    && (Bit.IS_SET(mob.pIndexData.mprog_flags, TRIG_GREET)
                        || Bit.IS_SET(mob.pIndexData.mprog_flags, TRIG_GRALL)))
                {
                    if (Bit.IS_SET(mob.pIndexData.mprog_flags, TRIG_GREET)
                        && mob.position == mob.pIndexData.default_pos
                        && Handler.can_see(mob, ch))
                        mp_percent_trigger(mob, ch, null, null, (int)TRIG_GREET);
                    else if (Bit.IS_SET(mob.pIndexData.mprog_flags, TRIG_GRALL))
                        mp_percent_trigger(mob, ch, null, null, (int)TRIG_GRALL);
                }
            }
        }

        public static void mp_hprct_trigger(CharData mob, CharData ch)
        {
            for (var prg = mob.pIndexData.mprogs; prg != null; prg = prg.next)
                if ((prg.trig_type == (int)TRIG_HPCNT)
                    && ((100 * mob.hit / mob.max_hit) < Interp.atoi(prg.trig_phrase)))
                {
                    program_flow(prg.vnum, prg.code, mob, ch, null, null);
                    break;
                }
        }
    }
}
