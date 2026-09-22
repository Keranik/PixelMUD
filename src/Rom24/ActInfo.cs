using static Rom24.Merc;

namespace Rom24
{
    public static partial class ActInfo
    {
        static readonly string[] dir_name = { "north", "east", "south", "west", "up", "down" };

        public static void do_exits(CharData ch, string argument)
        {
            bool fAuto = !RomString.str_cmp(argument, "auto");

            if (!Handler.check_blind(ch))
                return;

            string buf;
            if (fAuto)
                buf = "{o[Exits:";
            else if (Bit.IS_IMMORTAL(ch))
                buf = RomString.sprintf("Obvious exits from room %d:\n\r", ch.in_room.vnum);
            else
                buf = "Obvious exits:\n\r";

            bool found = false;
            for (int door = 0; door <= 5; door++)
            {
                var pexit = ch.in_room.exit[door];
                if (pexit != null
                    && pexit.to_room != null
                    && Handler.can_see_room(ch, pexit.to_room)
                    && !Bit.IS_SET(pexit.exit_info, EX_CLOSED))
                {
                    found = true;
                    if (fAuto)
                    {
                        buf += " ";
                        buf += dir_name[door];
                    }
                    else
                    {
                        buf += RomString.sprintf("%-5s - %s",
                            RomString.capitalize(dir_name[door]),
                            Handler.room_is_dark(pexit.to_room)
                            ? "Too dark to tell" : pexit.to_room.name);
                        if (Bit.IS_IMMORTAL(ch))
                            buf += RomString.sprintf(
                                " (room %d)\n\r", pexit.to_room.vnum);
                        else
                            buf += "\n\r";
                    }
                }
            }

            if (!found)
                buf += fAuto ? " none" : "None.\n\r";

            if (fAuto)
                buf += "]{x\n\r";

            Comm.send_to_char(buf, ch);
        }

        public static void do_score(CharData ch, string argument)
        {
            string buf = RomString.sprintf(
                "You are %s%s, level %d, %d years old (%d hours).\n\r",
                ch.name,
                Bit.IS_NPC(ch) ? "" : ch.pcdata.title,
                ch.level, Handler.get_age(ch),
                (ch.played + (int)(Game.current_time - ch.logon)) / 3600);
            Comm.send_to_char(buf, ch);

            if (Handler.get_trust(ch) != ch.level)
                Comm.send_to_char(RomString.sprintf("You are trusted at level %d.\n\r", Handler.get_trust(ch)), ch);

            Comm.send_to_char(RomString.sprintf("Race: %s  Sex: %s  Class: %s\n\r",
                Tables.race_table[ch.race].name,
                ch.sex == 0 ? "sexless" : ch.sex == 1 ? "male" : "female",
                Bit.IS_NPC(ch) ? "mobile" : Tables.class_table[ch.klass].name), ch);

            Comm.send_to_char(RomString.sprintf(
                "You have %d/%d hit, %d/%d mana, %d/%d movement.\n\r",
                ch.hit, ch.max_hit, ch.mana, ch.max_mana, ch.move, ch.max_move), ch);

            Comm.send_to_char(RomString.sprintf(
                "You have %d practices and %d training sessions.\n\r",
                ch.practice, ch.train), ch);

            Comm.send_to_char(RomString.sprintf(
                "You are carrying %d/%d items with weight %ld/%d pounds.\n\r",
                ch.carry_number, Handler.can_carry_n(ch),
                Handler.get_carry_weight(ch) / 10, Handler.can_carry_w(ch) / 10), ch);

            Comm.send_to_char(RomString.sprintf(
                "Str: %d(%d)  Int: %d(%d)  Wis: %d(%d)  Dex: %d(%d)  Con: %d(%d)\n\r",
                ch.perm_stat[STAT_STR], Handler.get_curr_stat(ch, STAT_STR),
                ch.perm_stat[STAT_INT], Handler.get_curr_stat(ch, STAT_INT),
                ch.perm_stat[STAT_WIS], Handler.get_curr_stat(ch, STAT_WIS),
                ch.perm_stat[STAT_DEX], Handler.get_curr_stat(ch, STAT_DEX),
                ch.perm_stat[STAT_CON], Handler.get_curr_stat(ch, STAT_CON)), ch);

            Comm.send_to_char(RomString.sprintf(
                "You have scored %d exp, and have %ld gold and %ld silver coins.\n\r",
                ch.exp, ch.gold, ch.silver), ch);

            if (!Bit.IS_NPC(ch) && ch.level < LEVEL_HERO)
                Comm.send_to_char(RomString.sprintf("You need %d exp to level.\n\r",
                    ((ch.level + 1) * Handler.exp_per_level(ch, ch.pcdata.points) - ch.exp)), ch);

            Comm.send_to_char(RomString.sprintf("Wimpy set to %d hit points.\n\r", ch.wimpy), ch);

            if (!Bit.IS_NPC(ch) && ch.pcdata.condition[COND_DRUNK] > 10)
                Comm.send_to_char("You are drunk.\n\r", ch);
            if (!Bit.IS_NPC(ch) && ch.pcdata.condition[COND_THIRST] == 0)
                Comm.send_to_char("You are thirsty.\n\r", ch);
            if (!Bit.IS_NPC(ch) && ch.pcdata.condition[COND_HUNGER] == 0)
                Comm.send_to_char("You are hungry.\n\r", ch);

            switch (ch.position)
            {
                case POS_DEAD: Comm.send_to_char("You are DEAD!!\n\r", ch); break;
                case POS_MORTAL: Comm.send_to_char("You are mortally wounded.\n\r", ch); break;
                case POS_INCAP: Comm.send_to_char("You are incapacitated.\n\r", ch); break;
                case POS_STUNNED: Comm.send_to_char("You are stunned.\n\r", ch); break;
                case POS_SLEEPING: Comm.send_to_char("You are sleeping.\n\r", ch); break;
                case POS_RESTING: Comm.send_to_char("You are resting.\n\r", ch); break;
                case POS_SITTING: Comm.send_to_char("You are sitting.\n\r", ch); break;
                case POS_STANDING: Comm.send_to_char("You are standing.\n\r", ch); break;
                case POS_FIGHTING: Comm.send_to_char("You are fighting.\n\r", ch); break;
            }

            if (ch.level >= 25)
            {
                Comm.send_to_char(RomString.sprintf(
                    "Armor: pierce: %d  bash: %d  slash: %d  magic: %d\n\r",
                    Handler.GET_AC(ch, AC_PIERCE),
                    Handler.GET_AC(ch, AC_BASH),
                    Handler.GET_AC(ch, AC_SLASH), Handler.GET_AC(ch, AC_EXOTIC)), ch);
            }

            for (int i = 0; i < 4; i++)
            {
                string temp = i switch
                {
                    AC_PIERCE => "piercing",
                    AC_BASH => "bashing",
                    AC_SLASH => "slashing",
                    AC_EXOTIC => "magic",
                    _ => "error"
                };

                Comm.send_to_char("You are ", ch);

                if (Handler.GET_AC(ch, i) >= 101)
                    buf = RomString.sprintf("hopelessly vulnerable to %s.\n\r", temp);
                else if (Handler.GET_AC(ch, i) >= 80)
                    buf = RomString.sprintf("defenseless against %s.\n\r", temp);
                else if (Handler.GET_AC(ch, i) >= 60)
                    buf = RomString.sprintf("barely protected from %s.\n\r", temp);
                else if (Handler.GET_AC(ch, i) >= 40)
                    buf = RomString.sprintf("slightly armored against %s.\n\r", temp);
                else if (Handler.GET_AC(ch, i) >= 20)
                    buf = RomString.sprintf("somewhat armored against %s.\n\r", temp);
                else if (Handler.GET_AC(ch, i) >= 0)
                    buf = RomString.sprintf("armored against %s.\n\r", temp);
                else if (Handler.GET_AC(ch, i) >= -20)
                    buf = RomString.sprintf("well-armored against %s.\n\r", temp);
                else if (Handler.GET_AC(ch, i) >= -40)
                    buf = RomString.sprintf("very well-armored against %s.\n\r", temp);
                else if (Handler.GET_AC(ch, i) >= -60)
                    buf = RomString.sprintf("heavily armored against %s.\n\r", temp);
                else if (Handler.GET_AC(ch, i) >= -80)
                    buf = RomString.sprintf("superbly armored against %s.\n\r", temp);
                else if (Handler.GET_AC(ch, i) >= -100)
                    buf = RomString.sprintf("almost invulnerable to %s.\n\r", temp);
                else
                    buf = RomString.sprintf("divinely armored against %s.\n\r", temp);

                Comm.send_to_char(buf, ch);
            }

            if (Bit.IS_IMMORTAL(ch))
            {
                Comm.send_to_char("Holy Light: ", ch);
                if (Bit.IS_SET(ch.act, PLR_HOLYLIGHT))
                    Comm.send_to_char("on", ch);
                else
                    Comm.send_to_char("off", ch);

                if (ch.invis_level != 0)
                    Comm.send_to_char($"  Invisible: level {ch.invis_level}", ch);

                if (ch.incog_level != 0)
                    Comm.send_to_char($"  Incognito: level {ch.incog_level}", ch);
                Comm.send_to_char("\n\r", ch);
            }

            if (ch.level >= 15)
            {
                Comm.send_to_char(RomString.sprintf("Hitroll: %d  Damroll: %d.\n\r",
                    Handler.GET_HITROLL(ch), Handler.GET_DAMROLL(ch)), ch);
            }

            if (ch.level >= 10)
            {
                Comm.send_to_char(RomString.sprintf("Alignment: %d.  ", ch.alignment), ch);
            }

            Comm.send_to_char("You are ", ch);
            if (ch.alignment > 900)
                Comm.send_to_char("angelic.\n\r", ch);
            else if (ch.alignment > 700)
                Comm.send_to_char("saintly.\n\r", ch);
            else if (ch.alignment > 350)
                Comm.send_to_char("good.\n\r", ch);
            else if (ch.alignment > 100)
                Comm.send_to_char("kind.\n\r", ch);
            else if (ch.alignment > -100)
                Comm.send_to_char("neutral.\n\r", ch);
            else if (ch.alignment > -350)
                Comm.send_to_char("mean.\n\r", ch);
            else if (ch.alignment > -700)
                Comm.send_to_char("evil.\n\r", ch);
            else if (ch.alignment > -900)
                Comm.send_to_char("demonic.\n\r", ch);
            else
                Comm.send_to_char("satanic.\n\r", ch);

            if (Bit.IS_SET(ch.comm, COMM_SHOW_AFFECTS))
                Interp.do_function(ch, do_affects, "");
        }

        public static void do_affects(CharData ch, string argument)
        {
            if (ch.affected != null)
            {
                Comm.send_to_char("You are affected by the following spells:\n\r", ch);
                AffectData paf_last = null;
                for (var paf = ch.affected; paf != null; paf = paf.next)
                {
                    string buf;
                    if (paf_last != null && paf.type == paf_last.type)
                    {
                        if (ch.level >= 20)
                            buf = "                      ";
                        else
                            continue;
                    }
                    else
                        buf = RomString.sprintf("Spell: %-15s", Tables.skill_table[paf.type].name);

                    Comm.send_to_char(buf, ch);

                    if (ch.level >= 20)
                    {
                        Comm.send_to_char(RomString.sprintf(
                            ": modifies %s by %d ",
                            Handler.affect_loc_name(paf.location), paf.modifier), ch);
                        if (paf.duration == -1)
                            buf = "permanently";
                        else
                            buf = $"for {paf.duration} hours";
                        Comm.send_to_char(buf, ch);
                    }

                    Comm.send_to_char("\n\r", ch);
                    paf_last = paf;
                }
            }
            else
                Comm.send_to_char("You are not affected by any spells.\n\r", ch);
        }

        public static void set_title(CharData ch, string title)
        {
            if (Bit.IS_NPC(ch))
            {
                Db.bug("Set_title: NPC.", 0);
                return;
            }

            if (title.Length == 0 || (title[0] != '.' && title[0] != ',' && title[0] != '!'
                && title[0] != '?'))
            {
                ch.pcdata.title = " " + title;
            }
            else
            {
                ch.pcdata.title = title;
            }
            Gmcp.Name(ch);
        }

        public static void do_who(CharData ch, string argument)
        {
            int iLevelLower = 0;
            int iLevelUpper = MAX_LEVEL;
            bool[] rgfClass = new bool[MAX_CLASS];
            bool[] rgfRace = new bool[MAX_PC_RACE];
            bool[] rgfClan = new bool[MAX_CLAN];
            bool fClassRestrict = false;
            bool fClanRestrict = false;
            bool fClan = false;
            bool fRaceRestrict = false;
            bool fImmortalOnly = false;

            int nNumber = 0;
            for (;;)
            {
                argument = RomString.one_argument(argument, out string arg);
                if (arg.Length == 0)
                    break;

                if (Interp.is_number(arg))
                {
                    switch (++nNumber)
                    {
                        case 1:
                            iLevelLower = Interp.atoi(arg);
                            break;
                        case 2:
                            iLevelUpper = Interp.atoi(arg);
                            break;
                        default:
                            Comm.send_to_char("Only two level numbers allowed.\n\r", ch);
                            return;
                    }
                }
                else
                {
                    if (!RomString.str_prefix(arg, "immortals"))
                    {
                        fImmortalOnly = true;
                    }
                    else
                    {
                        int iClass = Lookup.class_lookup(arg);
                        if (iClass == -1)
                        {
                            int iRace = Lookup.race_lookup(arg);

                            if (iRace == 0 || iRace >= MAX_PC_RACE)
                            {
                                if (!RomString.str_prefix(arg, "clan"))
                                    fClan = true;
                                else
                                {
                                    int iClan = Lookup.clan_lookup(arg);
                                    if (iClan != 0)
                                    {
                                        fClanRestrict = true;
                                        rgfClan[iClan] = true;
                                    }
                                    else
                                    {
                                        Comm.send_to_char
                                            ("That's not a valid race, class, or clan.\n\r",
                                            ch);
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                fRaceRestrict = true;
                                rgfRace[iRace] = true;
                            }
                        }
                        else
                        {
                            fClassRestrict = true;
                            rgfClass[iClass] = true;
                        }
                    }
                }
            }

            int nMatch = 0;
            var output = new System.Text.StringBuilder();
            for (var d = Game.descriptor_list; d != null; d = d.next)
            {
                if (d.connected != CON_PLAYING || !Handler.can_see(ch, d.character))
                    continue;

                var wch = d.original != null ? d.original : d.character;

                if (!Handler.can_see(ch, wch))
                    continue;

                if (wch.level < iLevelLower
                    || wch.level > iLevelUpper
                    || (fImmortalOnly && wch.level < LEVEL_IMMORTAL)
                    || (fClassRestrict && !rgfClass[wch.klass])
                    || (fRaceRestrict && !rgfRace[wch.race])
                    || (fClan && !Handler.is_clan(wch))
                    || (fClanRestrict && !rgfClan[wch.clan]))
                    continue;

                nMatch++;

                string whoClass = Tables.class_table[wch.klass].who_name;
                switch (wch.level)
                {
                    default:
                        break;
                    case MAX_LEVEL - 0:
                        whoClass = "IMP";
                        break;
                    case MAX_LEVEL - 1:
                        whoClass = "CRE";
                        break;
                    case MAX_LEVEL - 2:
                        whoClass = "SUP";
                        break;
                    case MAX_LEVEL - 3:
                        whoClass = "DEI";
                        break;
                    case MAX_LEVEL - 4:
                        whoClass = "GOD";
                        break;
                    case MAX_LEVEL - 5:
                        whoClass = "IMM";
                        break;
                    case MAX_LEVEL - 6:
                        whoClass = "DEM";
                        break;
                    case MAX_LEVEL - 7:
                        whoClass = "ANG";
                        break;
                    case MAX_LEVEL - 8:
                        whoClass = "AVA";
                        break;
                }

                output.Append(RomString.sprintf("[%2d %6s %s] %s%s%s%s%s%s%s%s\n\r",
                    wch.level,
                    wch.race < MAX_PC_RACE ? Tables.pc_race_table[wch.race].who_name
                    : "     ",
                    whoClass,
                    wch.incog_level >= LEVEL_HERO ? "(Incog) " : "",
                    wch.invis_level >= LEVEL_HERO ? "(Wizi) " : "",
                    Tables.clan_table[wch.clan].who_name,
                    Bit.IS_SET(wch.comm, COMM_AFK) ? "[AFK] " : "",
                    Bit.IS_SET(wch.act, PLR_KILLER) ? "(KILLER) " : "",
                    Bit.IS_SET(wch.act, PLR_THIEF) ? "(THIEF) " : "",
                    wch.name, Bit.IS_NPC(wch) ? "" : wch.pcdata.title));
            }

            output.Append(RomString.sprintf("\n\rPlayers found: %d\n\r", nMatch));
            Comm.page_to_char(output.ToString(), ch);
        }

        public static void do_help(CharData ch, string argument)
        {
            argument ??= "";
            if (argument.Length == 0)
                argument = "summary";

            string argall = "";
            while (argument.Length != 0)
            {
                argument = RomString.one_argument(argument, out string argone);
                if (argall.Length != 0)
                    argall += " ";
                argall += argone;
            }

            var output = new System.Text.StringBuilder();
            bool found = false;
            for (var pHelp = Game.help_first; pHelp != null; pHelp = pHelp.next)
            {
                int level = (pHelp.level < 0) ? -1 * pHelp.level - 1 : pHelp.level;

                if (level > Handler.get_trust(ch))
                    continue;

                if (Handler.is_name(argall, pHelp.keyword))
                {
                    if (found)
                        output.Append(
                            "\n\r============================================================\n\r\n\r");
                    if (pHelp.level >= 0 && RomString.str_cmp(argall, "imotd"))
                    {
                        output.Append(pHelp.keyword);
                        output.Append("\n\r");
                    }

                    if (pHelp.text.Length > 0 && pHelp.text[0] == '.')
                        output.Append(pHelp.text.Substring(1));
                    else
                        output.Append(pHelp.text);
                    found = true;
                    if (ch.desc != null && ch.desc.connected != CON_PLAYING
                        && ch.desc.connected != CON_GEN_GROUPS)
                        break;
                }
            }

            if (!found)
            {
                Comm.send_to_char("No help on that word.\n\r", ch);
                if (argall.Length > MAX_CMD_LEN)
                {
                    argall = argall.Substring(0, MAX_CMD_LEN - 1);
                    Db.log_f("Excessive command length: %s requested %s.", ch.name, argall);
                    Comm.send_to_char("That was rude!\n\r", ch);
                }
                else
                {
                    Db.append_file(ch, OHELPS_FILE, argall);
                }
            }
            else
                Comm.page_to_char(output.ToString(), ch);
        }

        static readonly string[] day_name =
        {
            "the Moon", "the Bull", "Deception", "Thunder", "Freedom",
            "the Great Gods", "the Sun"
        };
        static readonly string[] month_name =
        {
            "Winter", "the Winter Wolf", "the Frost Giant", "the Old Forces",
            "the Grand Struggle", "the Spring", "Nature", "Futility", "the Dragon",
            "the Sun", "the Heat", "the Battle", "the Dark Shades", "the Shadows",
            "the Long Shadows", "the Ancient Darkness", "the Great Evil"
        };

        public static void do_time(CharData ch, string argument)
        {
            int day = Game.time_info.day + 1;
            string suf;
            if (day > 4 && day < 20) suf = "th";
            else if (day % 10 == 1) suf = "st";
            else if (day % 10 == 2) suf = "nd";
            else if (day % 10 == 3) suf = "rd";
            else suf = "th";
            string buf = RomString.sprintf(
                "It is %d o'clock %s, Day of %s, %d%s the Month of %s.\n\r",
                (Game.time_info.hour % 12 == 0) ? 12 : Game.time_info.hour % 12,
                Game.time_info.hour >= 12 ? "pm" : "am",
                day_name[day % 7], day, suf, month_name[Game.time_info.month]);
            Comm.send_to_char(buf, ch);
            buf = RomString.sprintf("ROM started up at %s\n\rThe system time is %s.\n\r",
                Game.str_boot_time, RomString.ctime(Game.current_time));
            Comm.send_to_char(buf, ch);
        }

        public static void do_weather(CharData ch, string argument)
        {
            string[] sky_look = { "cloudless", "cloudy", "rainy", "lit by flashes of lightning" };
            if (!Bit.IS_OUTSIDE(ch))
            {
                Comm.send_to_char("You can't see the weather indoors.\n\r", ch);
                return;
            }
            Comm.send_to_char(RomString.sprintf("The sky is %s and %s.\n\r",
                sky_look[Game.weather_info.sky],
                Game.weather_info.change >= 0
                    ? "a warm southerly breeze blows"
                    : "a cold northern gust blows"), ch);
        }

        public static void do_areas(CharData ch, string argument)
        {
            argument ??= "";
            if (argument.Length != 0)
            {
                Comm.send_to_char("No argument is used with this command.\n\r", ch);
                return;
            }

            int iAreaHalf = (Game.top_area + 1) / 2;
            var pArea1 = Game.area_first;
            var pArea2 = Game.area_first;
            for (int iArea = 0; iArea < iAreaHalf; iArea++)
                pArea2 = pArea2.next;

            for (int iArea = 0; iArea < iAreaHalf; iArea++)
            {
                string buf = RomString.sprintf("%-39s%-39s\n\r",
                    pArea1.credits, (pArea2 != null) ? pArea2.credits : "");
                Comm.send_to_char(buf, ch);
                pArea1 = pArea1.next;
                if (pArea2 != null)
                    pArea2 = pArea2.next;
            }
        }

        public static void do_read(CharData ch, string argument)
        {
            Interp.do_function(ch, do_look, argument);
        }

        public static void do_examine(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Examine what?\n\r", ch);
                return;
            }

            Interp.do_function(ch, do_look, arg);

            var obj = Handler.get_obj_here(ch, arg);
            if (obj != null)
            {
                switch (obj.item_type)
                {
                    default:
                        break;

                    case ITEM_JUKEBOX:
                        Interp.do_function(ch, Music.do_play, "list");
                        break;

                    case ITEM_MONEY:
                        string buf;
                        if (obj.value[0] == 0)
                        {
                            if (obj.value[1] == 0)
                                buf = "Odd...there's no coins in the pile.\n\r";
                            else if (obj.value[1] == 1)
                                buf = "Wow. One gold coin.\n\r";
                            else
                                buf = RomString.sprintf(
                                    "There are %d gold coins in the pile.\n\r",
                                    obj.value[1]);
                        }
                        else if (obj.value[1] == 0)
                        {
                            if (obj.value[0] == 1)
                                buf = "Wow. One silver coin.\n\r";
                            else
                                buf = RomString.sprintf(
                                    "There are %d silver coins in the pile.\n\r",
                                    obj.value[0]);
                        }
                        else
                            buf = RomString.sprintf(
                                "There are %d gold and %d silver coins in the pile.\n\r",
                                obj.value[1], obj.value[0]);
                        Comm.send_to_char(buf, ch);
                        break;

                    case ITEM_DRINK_CON:
                    case ITEM_CONTAINER:
                    case ITEM_CORPSE_NPC:
                    case ITEM_CORPSE_PC:
                        Interp.do_function(ch, do_look, "in " + argument);
                        break;
                }
            }
        }

        public static void do_worth(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
            {
                Comm.send_to_char(RomString.sprintf("You have %ld gold and %ld silver.\n\r",
                    ch.gold, ch.silver), ch);
                return;
            }

            Comm.send_to_char(RomString.sprintf(
                "You have %ld gold, %ld silver, and %d experience (%d exp to level).\n\r",
                ch.gold, ch.silver, ch.exp,
                (ch.level + 1) * Handler.exp_per_level(ch,
                    ch.pcdata.points) - ch.exp), ch);
        }

        public static void do_compare(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg1);
            argument = RomString.one_argument(argument, out string arg2);
            if (arg1.Length == 0)
            {
                Comm.send_to_char("Compare what to what?\n\r", ch);
                return;
            }

            var obj1 = Handler.get_obj_carry(ch, arg1, ch);
            if (obj1 == null)
            {
                Comm.send_to_char("You do not have that item.\n\r", ch);
                return;
            }

            ObjData obj2;
            if (arg2.Length == 0)
            {
                for (obj2 = ch.carrying; obj2 != null; obj2 = obj2.next_content)
                {
                    if (obj2.wear_loc != WEAR_NONE && Handler.can_see_obj(ch, obj2)
                        && obj1.item_type == obj2.item_type
                        && (obj1.wear_flags & obj2.wear_flags & ~ITEM_TAKE) != 0)
                        break;
                }

                if (obj2 == null)
                {
                    Comm.send_to_char("You aren't wearing anything comparable.\n\r", ch);
                    return;
                }
            }
            else
            {
                obj2 = Handler.get_obj_carry(ch, arg2, ch);
                if (obj2 == null)
                {
                    Comm.send_to_char("You do not have that item.\n\r", ch);
                    return;
                }
            }

            string msg = null;
            int value1 = 0;
            int value2 = 0;

            if (obj1 == obj2)
            {
                msg = "You compare $p to itself.  It looks about the same.";
            }
            else if (obj1.item_type != obj2.item_type)
            {
                msg = "You can't compare $p and $P.";
            }
            else
            {
                switch (obj1.item_type)
                {
                    default:
                        msg = "You can't compare $p and $P.";
                        break;

                    case ITEM_ARMOR:
                        value1 = obj1.value[0] + obj1.value[1] + obj1.value[2];
                        value2 = obj2.value[0] + obj2.value[1] + obj2.value[2];
                        break;

                    case ITEM_WEAPON:
                        if (obj1.pIndexData.new_format)
                            value1 = (1 + obj1.value[2]) * obj1.value[1];
                        else
                            value1 = obj1.value[1] + obj1.value[2];

                        if (obj2.pIndexData.new_format)
                            value2 = (1 + obj2.value[2]) * obj2.value[1];
                        else
                            value2 = obj2.value[1] + obj2.value[2];
                        break;
                }
            }

            if (msg == null)
            {
                if (value1 == value2)
                    msg = "$p and $P look about the same.";
                else if (value1 > value2)
                    msg = "$p looks better than $P.";
                else
                    msg = "$p looks worse than $P.";
            }

            Comm.act(msg, ch, obj1, obj2, TO_CHAR);
        }

        public static void do_consider(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Consider killing whom?\n\r", ch);
                return;
            }

            var victim = Handler.get_char_room(ch, arg);
            if (victim == null)
            {
                Comm.send_to_char("They're not here.\n\r", ch);
                return;
            }

            if (Fight.is_safe(ch, victim))
            {
                Comm.send_to_char("Don't even think about it.\n\r", ch);
                return;
            }

            int diff = victim.level - ch.level;

            string msg;
            if (diff <= -10)
                msg = "You can kill $N naked and weaponless.";
            else if (diff <= -5)
                msg = "$N is no match for you.";
            else if (diff <= -2)
                msg = "$N looks like an easy kill.";
            else if (diff <= 1)
                msg = "The perfect match!";
            else if (diff <= 4)
                msg = "$N says 'Do you feel lucky, punk?'.";
            else if (diff <= 9)
                msg = "$N laughs at you mercilessly.";
            else
                msg = "Death will thank you for your gift.";

            Comm.act(msg, ch, null, victim, TO_CHAR);
        }

        public static void do_report(CharData ch, string argument)
        {
            string buf = RomString.sprintf(
                "You say 'I have %d/%d hp %d/%d mana %d/%d mv %d xp.'\n\r",
                ch.hit, ch.max_hit,
                ch.mana, ch.max_mana, ch.move, ch.max_move, ch.exp);

            Comm.send_to_char(buf, ch);

            buf = RomString.sprintf("$n says 'I have %d/%d hp %d/%d mana %d/%d mv %d xp.'",
                ch.hit, ch.max_hit,
                ch.mana, ch.max_mana, ch.move, ch.max_move, ch.exp);

            Comm.act(buf, ch, null, null, TO_ROOM);
        }

        public static void do_where(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Players near you:\n\r", ch);
                bool found = false;
                for (var d = Game.descriptor_list; d != null; d = d.next)
                {
                    CharData victim;
                    if (d.connected == CON_PLAYING
                        && (victim = d.character) != null && !Bit.IS_NPC(victim)
                        && victim.in_room != null
                        && !Bit.IS_SET(victim.in_room.room_flags, ROOM_NOWHERE)
                        && (Handler.is_room_owner(ch, victim.in_room)
                            || !Handler.room_is_private(victim.in_room))
                        && victim.in_room.area == ch.in_room.area
                        && Handler.can_see(ch, victim))
                    {
                        found = true;
                        Comm.send_to_char(RomString.sprintf("%-28s %s\n\r",
                            victim.name, victim.in_room.name), ch);
                    }
                }
                if (!found)
                    Comm.send_to_char("None\n\r", ch);
            }
            else
            {
                bool found = false;
                for (var victim = Game.char_list; victim != null; victim = victim.next)
                {
                    if (victim.in_room != null
                        && victim.in_room.area == ch.in_room.area
                        && !Bit.IS_AFFECTED(victim, AFF_HIDE)
                        && !Bit.IS_AFFECTED(victim, AFF_SNEAK)
                        && Handler.can_see(ch, victim) && Handler.is_name(arg, victim.name))
                    {
                        found = true;
                        Comm.send_to_char(RomString.sprintf("%-28s %s\n\r",
                            Handler.PERS(victim, ch), victim.in_room.name), ch);
                        break;
                    }
                }
                if (!found)
                    Comm.act("You didn't find any $T.", ch, null, arg, TO_CHAR);
            }
        }

        public static void do_practice(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            if (argument.Length == 0)
            {
                int col = 0;
                for (int sn = 0; sn < MAX_SKILL; sn++)
                {
                    if (Tables.skill_table[sn].name == null)
                        break;
                    if (ch.level < Tables.skill_table[sn].skill_level[ch.klass]
                        || ch.pcdata.learned[sn] < 1)
                        continue;

                    Comm.send_to_char(RomString.sprintf("%-18s %3d%%  ",
                        Tables.skill_table[sn].name, ch.pcdata.learned[sn]), ch);
                    if (++col % 3 == 0)
                        Comm.send_to_char("\n\r", ch);
                }

                if (col % 3 != 0)
                    Comm.send_to_char("\n\r", ch);

                Comm.send_to_char(RomString.sprintf("You have %d practice sessions left.\n\r",
                    ch.practice), ch);
            }
            else
            {
                if (!Bit.IS_AWAKE(ch))
                {
                    Comm.send_to_char("In your dreams, or what?\n\r", ch);
                    return;
                }

                CharData mob;
                for (mob = ch.in_room.people; mob != null; mob = mob.next_in_room)
                {
                    if (Bit.IS_NPC(mob) && Bit.IS_SET(mob.act, ACT_PRACTICE))
                        break;
                }

                if (mob == null)
                {
                    Comm.send_to_char("You can't do that here.\n\r", ch);
                    return;
                }

                if (ch.practice <= 0)
                {
                    Comm.send_to_char("You have no practice sessions left.\n\r", ch);
                    return;
                }

                int sn = Lookup.find_spell(ch, argument);
                if (sn < 0 || (!Bit.IS_NPC(ch)
                    && (ch.level <
                        Tables.skill_table[sn].skill_level[ch.klass]
                        || ch.pcdata.learned[sn] < 1
                        || Tables.skill_table[sn].rating[ch.klass] == 0)))
                {
                    Comm.send_to_char("You can't practice that.\n\r", ch);
                    return;
                }

                int adept = Bit.IS_NPC(ch) ? 100 : Tables.class_table[ch.klass].skill_adept;

                if (ch.pcdata.learned[sn] >= adept)
                {
                    Comm.send_to_char(RomString.sprintf("You are already learned at %s.\n\r",
                        Tables.skill_table[sn].name), ch);
                }
                else
                {
                    ch.practice--;
                    ch.pcdata.learned[sn] +=
                        Tables.int_app[Handler.get_curr_stat(ch, STAT_INT)] /
                        Tables.skill_table[sn].rating[ch.klass];
                    if (ch.pcdata.learned[sn] < adept)
                    {
                        Comm.act("You practice $T.",
                            ch, null, Tables.skill_table[sn].name, TO_CHAR);
                        Comm.act("$n practices $T.",
                            ch, null, Tables.skill_table[sn].name, TO_ROOM);
                    }
                    else
                    {
                        ch.pcdata.learned[sn] = adept;
                        Comm.act("You are now learned at $T.",
                            ch, null, Tables.skill_table[sn].name, TO_CHAR);
                        Comm.act("$n is now learned at $T.",
                            ch, null, Tables.skill_table[sn].name, TO_ROOM);
                    }
                }
            }
        }

        public static void do_wimpy(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);

            int wimpy;
            if (arg.Length == 0)
                wimpy = ch.max_hit / 5;
            else
                wimpy = Interp.atoi(arg);

            if (wimpy < 0)
            {
                Comm.send_to_char("Your courage exceeds your wisdom.\n\r", ch);
                return;
            }

            if (wimpy > ch.max_hit / 2)
            {
                Comm.send_to_char("Such cowardice ill becomes you.\n\r", ch);
                return;
            }

            ch.wimpy = wimpy;
            Comm.send_to_char(RomString.sprintf("Wimpy set to %d hit points.\n\r", wimpy), ch);
        }

        public static void do_brief(CharData ch, string argument)
        {
            if (Bit.IS_SET(ch.comm, COMM_BRIEF))
            {
                Comm.send_to_char("Full descriptions activated.\n\r", ch);
                Bit.REMOVE_BIT(ref ch.comm, COMM_BRIEF);
            }
            else
            {
                Comm.send_to_char("Short descriptions activated.\n\r", ch);
                Bit.SET_BIT(ref ch.comm, COMM_BRIEF);
            }
        }

        public static void do_compact(CharData ch, string argument)
        {
            if (Bit.IS_SET(ch.comm, COMM_COMPACT))
            {
                Comm.send_to_char("Compact mode removed.\n\r", ch);
                Bit.REMOVE_BIT(ref ch.comm, COMM_COMPACT);
            }
            else
            {
                Comm.send_to_char("Compact mode set.\n\r", ch);
                Bit.SET_BIT(ref ch.comm, COMM_COMPACT);
            }
        }

        public static void do_prompt(CharData ch, string argument)
        {
            string buf;

            if (argument.Length == 0)
            {
                if (Bit.IS_SET(ch.comm, COMM_PROMPT))
                {
                    Comm.send_to_char("You will no longer see prompts.\n\r", ch);
                    Bit.REMOVE_BIT(ref ch.comm, COMM_PROMPT);
                }
                else
                {
                    Comm.send_to_char("You will now see prompts.\n\r", ch);
                    Bit.SET_BIT(ref ch.comm, COMM_PROMPT);
                }
                return;
            }

            if (argument == "all")
                buf = "<%hhp %mm %vmv> ";
            else
            {
                if (argument.Length > 50)
                    argument = argument.Substring(0, 50);
                buf = RomString.smash_tilde(argument);
                if (RomString.str_suffix("%c", buf))
                    buf += " ";
            }

            ch.prompt = buf;
            Comm.send_to_char(RomString.sprintf("Prompt set to %s\n\r", ch.prompt), ch);
        }

        public static void do_combine(CharData ch, string argument)
        {
            if (Bit.IS_SET(ch.comm, COMM_COMBINE))
            {
                Comm.send_to_char("Long inventory selected.\n\r", ch);
                Bit.REMOVE_BIT(ref ch.comm, COMM_COMBINE);
            }
            else
            {
                Comm.send_to_char("Combined inventory selected.\n\r", ch);
                Bit.SET_BIT(ref ch.comm, COMM_COMBINE);
            }
        }

        public static void do_scroll(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                if (ch.lines == 0)
                    Comm.send_to_char("You do not page long messages.\n\r", ch);
                else
                {
                    Comm.send_to_char(RomString.sprintf("You currently display %d lines per page.\n\r",
                        ch.lines + 2), ch);
                }
                return;
            }

            if (!Interp.is_number(arg))
            {
                Comm.send_to_char("You must provide a number.\n\r", ch);
                return;
            }

            int lines = Interp.atoi(arg);

            if (lines == 0)
            {
                Comm.send_to_char("Paging disabled.\n\r", ch);
                ch.lines = 0;
                return;
            }

            if (lines < 10 || lines > 100)
            {
                Comm.send_to_char("You must provide a reasonable number.\n\r", ch);
                return;
            }

            Comm.send_to_char(RomString.sprintf("Scroll set to %d lines.\n\r", lines), ch);
            ch.lines = lines - 2;
        }

        public static void do_title(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            if (argument.Length > 45)
                argument = argument.Substring(0, 45);

            int i = argument.Length;
            if (i >= 2 && argument[i - 1] == '{' && argument[i - 2] != '{')
                argument = argument.Substring(0, i - 1);

            if (argument.Length == 0)
            {
                Comm.send_to_char("Change your title to what?\n\r", ch);
                return;
            }

            argument = RomString.smash_tilde(argument);
            set_title(ch, argument);
            Comm.send_to_char("Ok.\n\r", ch);
        }

        public static void do_noloot(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            if (Bit.IS_SET(ch.act, PLR_CANLOOT))
            {
                Comm.send_to_char("Your corpse is now safe from thieves.\n\r", ch);
                Bit.REMOVE_BIT(ref ch.act, PLR_CANLOOT);
            }
            else
            {
                Comm.send_to_char("Your corpse may now be looted.\n\r", ch);
                Bit.SET_BIT(ref ch.act, PLR_CANLOOT);
            }
        }

        public static void do_nofollow(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            if (Bit.IS_SET(ch.act, PLR_NOFOLLOW))
            {
                Comm.send_to_char("You now accept followers.\n\r", ch);
                Bit.REMOVE_BIT(ref ch.act, PLR_NOFOLLOW);
            }
            else
            {
                Comm.send_to_char("You no longer accept followers.\n\r", ch);
                Bit.SET_BIT(ref ch.act, PLR_NOFOLLOW);
                ActComm.die_follower(ch);
            }
        }

        public static void do_nosummon(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
            {
                if (Bit.IS_SET(ch.imm_flags, IMM_SUMMON))
                {
                    Comm.send_to_char("You are no longer immune to summon.\n\r", ch);
                    Bit.REMOVE_BIT(ref ch.imm_flags, IMM_SUMMON);
                }
                else
                {
                    Comm.send_to_char("You are now immune to summoning.\n\r", ch);
                    Bit.SET_BIT(ref ch.imm_flags, IMM_SUMMON);
                }
            }
            else
            {
                if (Bit.IS_SET(ch.act, PLR_NOSUMMON))
                {
                    Comm.send_to_char("You are no longer immune to summon.\n\r", ch);
                    Bit.REMOVE_BIT(ref ch.act, PLR_NOSUMMON);
                }
                else
                {
                    Comm.send_to_char("You are now immune to summoning.\n\r", ch);
                    Bit.SET_BIT(ref ch.act, PLR_NOSUMMON);
                }
            }
        }

        public static void do_description(CharData ch, string argument)
        {
            if (argument.Length != 0)
            {
                string buf = "";
                argument = RomString.smash_tilde(argument);

                if (argument.Length > 0 && argument[0] == '-')
                {
                    if (string.IsNullOrEmpty(ch.description))
                    {
                        Comm.send_to_char("No lines left to remove.\n\r", ch);
                        return;
                    }

                    buf = ch.description;
                    bool found = false;
                    for (int len = buf.Length; len > 0; len--)
                    {
                        if (len < buf.Length && buf[len] == '\r')
                        {
                            if (!found)
                            {
                                if (len > 0)
                                    len--;
                                found = true;
                            }
                            else
                            {
                                ch.description = buf.Substring(0, len + 1);
                                Comm.send_to_char("Your description is:\n\r", ch);
                                Comm.send_to_char(ch.description != null ? ch.description :
                                    "(None).\n\r", ch);
                                return;
                            }
                        }
                    }
                    ch.description = "";
                    Comm.send_to_char("Description cleared.\n\r", ch);
                    return;
                }
                if (argument.Length > 0 && argument[0] == '+')
                {
                    if (ch.description != null)
                        buf += ch.description;
                    argument = argument.Substring(1);
                    int i = 0;
                    while (i < argument.Length && Bit.isspace(argument[i]))
                        i++;
                    argument = argument.Substring(i);
                }

                if (buf.Length >= 1024)
                {
                    Comm.send_to_char("Description too long.\n\r", ch);
                    return;
                }

                buf += argument;
                buf += "\n\r";
                ch.description = buf;
            }

            Comm.send_to_char("Your description is:\n\r", ch);
            Comm.send_to_char(ch.description != null ? ch.description : "(None).\n\r", ch);
        }

        public static void do_autolist(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            Comm.send_to_char("   action     status\n\r", ch);
            Comm.send_to_char("---------------------\n\r", ch);

            Comm.send_to_char("autoassist     ", ch);
            if (Bit.IS_SET(ch.act, PLR_AUTOASSIST))
                Comm.send_to_char("{GON{x\n\r", ch);
            else
                Comm.send_to_char("{ROFF{x\n\r", ch);

            Comm.send_to_char("autoexit       ", ch);
            if (Bit.IS_SET(ch.act, PLR_AUTOEXIT))
                Comm.send_to_char("{GON{x\n\r", ch);
            else
                Comm.send_to_char("{ROFF{x\n\r", ch);

            Comm.send_to_char("autogold       ", ch);
            if (Bit.IS_SET(ch.act, PLR_AUTOGOLD))
                Comm.send_to_char("{GON{x\n\r", ch);
            else
                Comm.send_to_char("{ROFF{x\n\r", ch);

            Comm.send_to_char("autoloot       ", ch);
            if (Bit.IS_SET(ch.act, PLR_AUTOLOOT))
                Comm.send_to_char("{GON{x\n\r", ch);
            else
                Comm.send_to_char("{ROFF{x\n\r", ch);

            Comm.send_to_char("autosac        ", ch);
            if (Bit.IS_SET(ch.act, PLR_AUTOSAC))
                Comm.send_to_char("{GON{x\n\r", ch);
            else
                Comm.send_to_char("{ROFF{x\n\r", ch);

            Comm.send_to_char("autosplit      ", ch);
            if (Bit.IS_SET(ch.act, PLR_AUTOSPLIT))
                Comm.send_to_char("{GON{x\n\r", ch);
            else
                Comm.send_to_char("{ROFF{x\n\r", ch);

            Comm.send_to_char("telnetga       ", ch);
            if (Bit.IS_SET(ch.comm, COMM_TELNET_GA))
                Comm.send_to_char("{GON{x\n\r", ch);
            else
                Comm.send_to_char("{ROFF{x\n\r", ch);

            Comm.send_to_char("compact mode   ", ch);
            if (Bit.IS_SET(ch.comm, COMM_COMPACT))
                Comm.send_to_char("{GON{x\n\r", ch);
            else
                Comm.send_to_char("{ROFF{x\n\r", ch);

            Comm.send_to_char("prompt         ", ch);
            if (Bit.IS_SET(ch.comm, COMM_PROMPT))
                Comm.send_to_char("{GON{x\n\r", ch);
            else
                Comm.send_to_char("{ROFF{x\n\r", ch);

            Comm.send_to_char("combine items  ", ch);
            if (Bit.IS_SET(ch.comm, COMM_COMBINE))
                Comm.send_to_char("{GON{x\n\r", ch);
            else
                Comm.send_to_char("{ROFF{x\n\r", ch);

            if (!Bit.IS_SET(ch.act, PLR_CANLOOT))
                Comm.send_to_char("Your corpse is safe from thieves.\n\r", ch);
            else
                Comm.send_to_char("Your corpse may be looted.\n\r", ch);

            if (Bit.IS_SET(ch.act, PLR_NOSUMMON))
                Comm.send_to_char("You cannot be summoned.\n\r", ch);
            else
                Comm.send_to_char("You can be summoned.\n\r", ch);

            if (Bit.IS_SET(ch.act, PLR_NOFOLLOW))
                Comm.send_to_char("You do not welcome followers.\n\r", ch);
            else
                Comm.send_to_char("You accept followers.\n\r", ch);
        }

        public static void do_autoassist(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            if (Bit.IS_SET(ch.act, PLR_AUTOASSIST))
            {
                Comm.send_to_char("Autoassist removed.\n\r", ch);
                Bit.REMOVE_BIT(ref ch.act, PLR_AUTOASSIST);
            }
            else
            {
                Comm.send_to_char("You will now assist when needed.\n\r", ch);
                Bit.SET_BIT(ref ch.act, PLR_AUTOASSIST);
            }
        }

        public static void do_autoexit(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            if (Bit.IS_SET(ch.act, PLR_AUTOEXIT))
            {
                Comm.send_to_char("Exits will no longer be displayed.\n\r", ch);
                Bit.REMOVE_BIT(ref ch.act, PLR_AUTOEXIT);
            }
            else
            {
                Comm.send_to_char("Exits will now be displayed.\n\r", ch);
                Bit.SET_BIT(ref ch.act, PLR_AUTOEXIT);
            }
        }

        public static void do_autogold(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            if (Bit.IS_SET(ch.act, PLR_AUTOGOLD))
            {
                Comm.send_to_char("Autogold removed.\n\r", ch);
                Bit.REMOVE_BIT(ref ch.act, PLR_AUTOGOLD);
            }
            else
            {
                Comm.send_to_char("Automatic gold looting set.\n\r", ch);
                Bit.SET_BIT(ref ch.act, PLR_AUTOGOLD);
            }
        }

        public static void do_autoloot(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            if (Bit.IS_SET(ch.act, PLR_AUTOLOOT))
            {
                Comm.send_to_char("Autolooting removed.\n\r", ch);
                Bit.REMOVE_BIT(ref ch.act, PLR_AUTOLOOT);
            }
            else
            {
                Comm.send_to_char("Automatic corpse looting set.\n\r", ch);
                Bit.SET_BIT(ref ch.act, PLR_AUTOLOOT);
            }
        }

        public static void do_autosac(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            if (Bit.IS_SET(ch.act, PLR_AUTOSAC))
            {
                Comm.send_to_char("Autosacrificing removed.\n\r", ch);
                Bit.REMOVE_BIT(ref ch.act, PLR_AUTOSAC);
            }
            else
            {
                Comm.send_to_char("Automatic corpse sacrificing set.\n\r", ch);
                Bit.SET_BIT(ref ch.act, PLR_AUTOSAC);
            }
        }

        public static void do_autosplit(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            if (Bit.IS_SET(ch.act, PLR_AUTOSPLIT))
            {
                Comm.send_to_char("Autosplitting removed.\n\r", ch);
                Bit.REMOVE_BIT(ref ch.act, PLR_AUTOSPLIT);
            }
            else
            {
                Comm.send_to_char("Automatic gold splitting set.\n\r", ch);
                Bit.SET_BIT(ref ch.act, PLR_AUTOSPLIT);
            }
        }

        public static void do_autoall(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            if (argument == "on")
            {
                Bit.SET_BIT(ref ch.act, PLR_AUTOASSIST);
                Bit.SET_BIT(ref ch.act, PLR_AUTOEXIT);
                Bit.SET_BIT(ref ch.act, PLR_AUTOGOLD);
                Bit.SET_BIT(ref ch.act, PLR_AUTOLOOT);
                Bit.SET_BIT(ref ch.act, PLR_AUTOSAC);
                Bit.SET_BIT(ref ch.act, PLR_AUTOSPLIT);

                Comm.send_to_char("All autos turned on.\n\r", ch);
            }
            else if (argument == "off")
            {
                Bit.REMOVE_BIT(ref ch.act, PLR_AUTOASSIST);
                Bit.REMOVE_BIT(ref ch.act, PLR_AUTOEXIT);
                Bit.REMOVE_BIT(ref ch.act, PLR_AUTOGOLD);
                Bit.REMOVE_BIT(ref ch.act, PLR_AUTOLOOT);
                Bit.REMOVE_BIT(ref ch.act, PLR_AUTOSAC);
                Bit.REMOVE_BIT(ref ch.act, PLR_AUTOSPLIT);

                Comm.send_to_char("All autos turned off.\n\r", ch);
            }
            else
                Comm.send_to_char("Usage: autoall [on|off]\n\r", ch);
        }

        public static void do_password(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            argument = PasswordArg(argument, out string arg1);
            PasswordArg(argument, out string arg2);

            if (arg1.Length == 0 || arg2.Length == 0)
            {
                Comm.send_to_char("Syntax: password <old> <new>.\n\r", ch);
                return;
            }

            if (UnixCrypt.crypt(arg1, ch.pcdata.pwd) != ch.pcdata.pwd)
            {
                Bit.WAIT_STATE(ch, 40);
                Comm.send_to_char("Wrong password.  Wait 10 seconds.\n\r", ch);
                return;
            }

            if (arg2.Length < 5)
            {
                Comm.send_to_char(
                    "New password must be at least five characters long.\n\r", ch);
                return;
            }

            string pwdnew = UnixCrypt.crypt(arg2, ch.name);
            for (int p = 0; p < pwdnew.Length; p++)
            {
                if (pwdnew[p] == '~')
                {
                    Comm.send_to_char("New password not acceptable, try again.\n\r", ch);
                    return;
                }
            }

            ch.pcdata.pwd = pwdnew;
            Save.save_char_obj(ch);
            Comm.send_to_char("Ok.\n\r", ch);
        }

        public static void do_socials(CharData ch, string argument)
        {
            int col = 0;

            for (int iSocial = 0; Game.social_table[iSocial].name.Length != 0; iSocial++)
            {
                Comm.send_to_char(RomString.sprintf("%-12s", Game.social_table[iSocial].name), ch);
                if (++col % 6 == 0)
                    Comm.send_to_char("\n\r", ch);
            }

            if (col % 6 != 0)
                Comm.send_to_char("\n\r", ch);
        }

        public static void do_motd(CharData ch, string argument)
        {
            Interp.do_function(ch, do_help, "motd");
        }

        public static void do_imotd(CharData ch, string argument)
        {
            Interp.do_function(ch, do_help, "imotd");
        }

        public static void do_rules(CharData ch, string argument)
        {
            Interp.do_function(ch, do_help, "rules");
        }

        public static void do_story(CharData ch, string argument)
        {
            Interp.do_function(ch, do_help, "story");
        }

        public static void do_wizlist(CharData ch, string argument)
        {
            Interp.do_function(ch, do_help, "wizlist");
        }

        public static void do_show(CharData ch, string argument)
        {
            if (Bit.IS_SET(ch.comm, COMM_SHOW_AFFECTS))
            {
                Comm.send_to_char("Affects will no longer be shown in score.\n\r", ch);
                Bit.REMOVE_BIT(ref ch.comm, COMM_SHOW_AFFECTS);
            }
            else
            {
                Comm.send_to_char("Affects will now be shown in score.\n\r", ch);
                Bit.SET_BIT(ref ch.comm, COMM_SHOW_AFFECTS);
            }
        }

        public static void do_whois(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("You must provide a name.\n\r", ch);
                return;
            }

            var output = new System.Text.StringBuilder();
            bool found = false;

            for (var d = Game.descriptor_list; d != null; d = d.next)
            {
                if (d.connected != CON_PLAYING || !Handler.can_see(ch, d.character))
                    continue;

                var wch = d.original != null ? d.original : d.character;

                if (!Handler.can_see(ch, wch))
                    continue;

                if (!RomString.str_prefix(arg, wch.name))
                {
                    found = true;

                    string whoClass = Tables.class_table[wch.klass].who_name;
                    switch (wch.level)
                    {
                        case MAX_LEVEL - 0:
                            whoClass = "IMP";
                            break;
                        case MAX_LEVEL - 1:
                            whoClass = "CRE";
                            break;
                        case MAX_LEVEL - 2:
                            whoClass = "SUP";
                            break;
                        case MAX_LEVEL - 3:
                            whoClass = "DEI";
                            break;
                        case MAX_LEVEL - 4:
                            whoClass = "GOD";
                            break;
                        case MAX_LEVEL - 5:
                            whoClass = "IMM";
                            break;
                        case MAX_LEVEL - 6:
                            whoClass = "DEM";
                            break;
                        case MAX_LEVEL - 7:
                            whoClass = "ANG";
                            break;
                        case MAX_LEVEL - 8:
                            whoClass = "AVA";
                            break;
                    }

                    string buf = RomString.sprintf("[%2d %6s %s] %s%s%s%s%s%s%s%s\n\r",
                        wch.level,
                        wch.race < MAX_PC_RACE ? Tables.pc_race_table[wch.race].who_name : "     ",
                        whoClass,
                        wch.incog_level >= LEVEL_HERO ? "(Incog) " : "",
                        wch.invis_level >= LEVEL_HERO ? "(Wizi) " : "",
                        Tables.clan_table[wch.clan].who_name,
                        Bit.IS_SET(wch.comm, COMM_AFK) ? "[AFK] " : "",
                        Bit.IS_SET(wch.act, PLR_KILLER) ? "(KILLER) " : "",
                        Bit.IS_SET(wch.act, PLR_THIEF) ? "(THIEF) " : "",
                        wch.name,
                        Bit.IS_NPC(wch) ? "" : wch.pcdata.title);
                    output.Append(buf);
                }
            }

            if (!found)
            {
                Comm.send_to_char("No one of that name is playing.\n\r", ch);
                return;
            }

            Comm.page_to_char(output.ToString(), ch);
        }

        public static void do_count(CharData ch, string argument)
        {
            int count = 0;

            for (var d = Game.descriptor_list; d != null; d = d.next)
                if (d.connected == CON_PLAYING && Handler.can_see(ch, d.character))
                    count++;

            Game.max_on = Bit.UMAX(count, Game.max_on);

            string buf;
            if (Game.max_on == count)
                buf = RomString.sprintf(
                    "There are %d characters on, the most so far today.\n\r",
                    count);
            else
                buf = RomString.sprintf(
                    "There are %d characters on, the most on today was %d.\n\r",
                    count, Game.max_on);

            Comm.send_to_char(buf, ch);
        }

        public static void do_telnetga(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            if (Bit.IS_SET(ch.comm, COMM_TELNET_GA))
            {
                Comm.send_to_char("Telnet GA removed.\n\r", ch);
                Bit.REMOVE_BIT(ref ch.comm, COMM_TELNET_GA);
            }
            else
            {
                Comm.send_to_char("Telnet GA enabled.\n\r", ch);
                Bit.SET_BIT(ref ch.comm, COMM_TELNET_GA);
            }
        }

        static string PasswordArg(string argument, out string arg)
        {
            argument ??= "";
            int i = 0;
            while (i < argument.Length && Bit.isspace(argument[i])) i++;
            char cEnd = ' ';
            if (i < argument.Length && (argument[i] == '\'' || argument[i] == '"'))
            {
                cEnd = argument[i];
                i++;
            }
            var sb = new System.Text.StringBuilder();
            while (i < argument.Length)
            {
                if (argument[i] == cEnd)
                {
                    i++;
                    break;
                }
                sb.Append(argument[i]);
                i++;
            }
            arg = sb.ToString();
            return i >= argument.Length ? "" : argument.Substring(i);
        }
    }
}
