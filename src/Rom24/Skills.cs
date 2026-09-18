using static Rom24.Merc;

namespace Rom24
{
    public class GroupType
    {
        public string name;
        public int[] rating;
        public string[] spells;
        public GroupType(string name, int[] rating, string[] spells)
        {
            this.name = name; this.rating = rating; this.spells = spells;
        }
    }

    public static partial class Tables
    {
        public static readonly GroupType[] group_table =
        {
            new GroupType("rom basics", new[] {0, 0, 0, 0},
                new[] {"scrolls", "staves", "wands", "recall"}),
            new GroupType("mage basics", new[] {0, -1, -1, -1},
                new[] {"dagger"}),
            new GroupType("cleric basics", new[] {-1, 0, -1, -1},
                new[] {"mace"}),
            new GroupType("thief basics", new[] {-1, -1, 0, -1},
                new[] {"dagger", "steal"}),
            new GroupType("warrior basics", new[] {-1, -1, -1, 0},
                new[] {"sword", "second attack"}),
            new GroupType("mage default", new[] {40, -1, -1, -1},
                new[] {"lore", "beguiling", "combat", "detection", "enhancement", "illusion",
                    "maladictions", "protective", "transportation", "weather"}),
            new GroupType("cleric default", new[] {-1, 40, -1, -1},
                new[] {"flail", "attack", "creation", "curative", "benedictions",
                    "detection", "healing", "maladictions", "protective", "shield block",
                    "transportation", "weather"}),
            new GroupType("thief default", new[] {-1, -1, 40, -1},
                new[] {"mace", "sword", "backstab", "disarm", "dodge", "second attack",
                    "trip", "hide", "peek", "pick lock", "sneak"}),
            new GroupType("warrior default", new[] {-1, -1, -1, 40},
                new[] {"weaponsmaster", "shield block", "bash", "disarm", "enhanced damage",
                    "parry", "rescue", "third attack"}),
            new GroupType("weaponsmaster", new[] {40, 40, 40, 20},
                new[] {"axe", "dagger", "flail", "mace", "polearm", "spear", "sword", "whip"}),
            new GroupType("attack", new[] {-1, 5, -1, 8},
                new[] {"demonfire", "dispel evil", "dispel good", "earthquake",
                    "flamestrike", "heat metal", "ray of truth"}),
            new GroupType("beguiling", new[] {4, -1, 6, -1},
                new[] {"calm", "charm person", "sleep"}),
            new GroupType("benedictions", new[] {-1, 4, -1, 8},
                new[] {"bless", "calm", "frenzy", "holy word", "remove curse"}),
            new GroupType("combat", new[] {6, -1, 10, 9},
                new[] {"acid blast", "burning hands", "chain lightning", "chill touch",
                    "colour spray", "fireball", "lightning bolt", "magic missile",
                    "shocking grasp"}),
            new GroupType("creation", new[] {4, 4, 8, 8},
                new[] {"continual light", "create food", "create spring", "create water",
                    "create rose", "floating disc"}),
            new GroupType("curative", new[] {-1, 4, -1, 8},
                new[] {"cure blindness", "cure disease", "cure poison"}),
            new GroupType("detection", new[] {4, 3, 6, -1},
                new[] {"detect evil", "detect good", "detect hidden", "detect invis",
                    "detect magic", "detect poison", "farsight", "identify",
                    "know alignment", "locate object"}),
            new GroupType("draconian", new[] {8, -1, -1, -1},
                new[] {"acid breath", "fire breath", "frost breath", "gas breath",
                    "lightning breath"}),
            new GroupType("enchantment", new[] {6, -1, -1, -1},
                new[] {"enchant armor", "enchant weapon", "fireproof", "recharge"}),
            new GroupType("enhancement", new[] {5, -1, 9, 9},
                new[] {"giant strength", "haste", "infravision", "refresh"}),
            new GroupType("harmful", new[] {-1, 3, -1, 6},
                new[] {"cause critical", "cause light", "cause serious", "harm"}),
            new GroupType("healing", new[] {-1, 3, -1, 6},
                new[] {"cure critical", "cure light", "cure serious", "heal",
                    "mass healing", "refresh"}),
            new GroupType("illusion", new[] {4, -1, 7, -1},
                new[] {"invis", "mass invis", "ventriloquate"}),
            new GroupType("maladictions", new[] {5, 5, 9, 9},
                new[] {"blindness", "change sex", "curse", "energy drain", "plague",
                    "poison", "slow", "weaken"}),
            new GroupType("protective", new[] {4, 4, 7, 8},
                new[] {"armor", "cancellation", "dispel magic", "fireproof",
                    "protection evil", "protection good", "sanctuary", "shield",
                    "stone skin"}),
            new GroupType("transportation", new[] {4, 4, 8, 9},
                new[] {"fly", "gate", "nexus", "pass door", "portal", "summon", "teleport",
                    "word of recall"}),
            new GroupType("weather", new[] {4, 4, 8, 8},
                new[] {"call lightning", "control weather", "faerie fire", "faerie fog",
                    "lightning bolt"}),
            new GroupType(null, new[] {0, 0, 0, 0}, new string[0])
        };
    }

    public static class Skills
    {
        public static void check_improve(CharData ch, int sn, bool success, int multiplier)
        {
            if (Bit.IS_NPC(ch))
                return;

            if (ch.level < Tables.skill_table[sn].skill_level[ch.klass]
                || Tables.skill_table[sn].rating[ch.klass] == 0
                || ch.pcdata.learned[sn] == 0 || ch.pcdata.learned[sn] == 100)
                return;

            int chance = 10 * Tables.int_app[Handler.get_curr_stat(ch, STAT_INT)];
            chance /= (multiplier * Tables.skill_table[sn].rating[ch.klass] * 4);
            chance += ch.level;

            if (RomRandom.number_range(1, 1000) > chance)
                return;

            if (success)
            {
                chance = Bit.URANGE(5, 100 - ch.pcdata.learned[sn], 95);
                if (RomRandom.number_percent() < chance)
                {
                    Comm.send_to_char(RomString.sprintf("You have become better at %s!\n\r",
                        Tables.skill_table[sn].name), ch);
                    ch.pcdata.learned[sn]++;
                    Update.gain_exp(ch, 2 * Tables.skill_table[sn].rating[ch.klass]);
                }
            }
            else
            {
                chance = Bit.URANGE(5, ch.pcdata.learned[sn] / 2, 30);
                if (RomRandom.number_percent() < chance)
                {
                    Comm.send_to_char(RomString.sprintf(
                        "You learn from your mistakes, and your %s skill improves.\n\r",
                        Tables.skill_table[sn].name), ch);
                    ch.pcdata.learned[sn] += RomRandom.number_range(1, 3);
                    ch.pcdata.learned[sn] = Bit.UMIN(ch.pcdata.learned[sn], 100);
                    Update.gain_exp(ch, 2 * Tables.skill_table[sn].rating[ch.klass]);
                }
            }
        }

        public static void list_group_costs(CharData ch)
        {
            if (Bit.IS_NPC(ch))
                return;

            int col = 0;
            Comm.send_to_char(RomString.sprintf("%-18s %-5s %-18s %-5s %-18s %-5s\n\r", "group", "cp",
                "group", "cp", "group", "cp"), ch);

            for (int gn = 0; gn < MAX_GROUP; gn++)
            {
                if (Tables.group_table[gn].name == null)
                    break;

                if (!ch.gen_data.group_chosen[gn]
                    && !ch.pcdata.group_known[gn]
                    && Tables.group_table[gn].rating[ch.klass] > 0)
                {
                    Comm.send_to_char(RomString.sprintf("%-18s %-5d ", Tables.group_table[gn].name,
                        Tables.group_table[gn].rating[ch.klass]), ch);
                    if (++col % 3 == 0)
                        Comm.send_to_char("\n\r", ch);
                }
            }
            if (col % 3 != 0)
                Comm.send_to_char("\n\r", ch);
            Comm.send_to_char("\n\r", ch);

            col = 0;
            Comm.send_to_char(RomString.sprintf("%-18s %-5s %-18s %-5s %-18s %-5s\n\r", "skill", "cp",
                "skill", "cp", "skill", "cp"), ch);

            for (int sn = 0; sn < MAX_SKILL; sn++)
            {
                if (Tables.skill_table[sn].name == null)
                    break;

                if (!ch.gen_data.skill_chosen[sn]
                    && ch.pcdata.learned[sn] == 0
                    && Tables.skill_table[sn].spell_fun == (SpellFun)Magic.spell_null
                    && Tables.skill_table[sn].rating[ch.klass] > 0)
                {
                    Comm.send_to_char(RomString.sprintf("%-18s %-5d ", Tables.skill_table[sn].name,
                        Tables.skill_table[sn].rating[ch.klass]), ch);
                    if (++col % 3 == 0)
                        Comm.send_to_char("\n\r", ch);
                }
            }
            if (col % 3 != 0)
                Comm.send_to_char("\n\r", ch);
            Comm.send_to_char("\n\r", ch);

            Comm.send_to_char(RomString.sprintf("Creation points: %d\n\r", ch.pcdata.points), ch);
            Comm.send_to_char(RomString.sprintf("Experience per level: %d\n\r",
                Handler.exp_per_level(ch, ch.gen_data.points_chosen)), ch);
        }

        public static void list_group_chosen(CharData ch)
        {
            if (Bit.IS_NPC(ch))
                return;

            int col = 0;
            Comm.send_to_char(RomString.sprintf("%-18s %-5s %-18s %-5s %-18s %-5s", "group", "cp", "group",
                "cp", "group", "cp\n\r"), ch);

            for (int gn = 0; gn < MAX_GROUP; gn++)
            {
                if (Tables.group_table[gn].name == null)
                    break;

                if (ch.gen_data.group_chosen[gn]
                    && Tables.group_table[gn].rating[ch.klass] > 0)
                {
                    Comm.send_to_char(RomString.sprintf("%-18s %-5d ", Tables.group_table[gn].name,
                        Tables.group_table[gn].rating[ch.klass]), ch);
                    if (++col % 3 == 0)
                        Comm.send_to_char("\n\r", ch);
                }
            }
            if (col % 3 != 0)
                Comm.send_to_char("\n\r", ch);
            Comm.send_to_char("\n\r", ch);

            col = 0;
            Comm.send_to_char(RomString.sprintf("%-18s %-5s %-18s %-5s %-18s %-5s", "skill", "cp", "skill",
                "cp", "skill", "cp\n\r"), ch);

            for (int sn = 0; sn < MAX_SKILL; sn++)
            {
                if (Tables.skill_table[sn].name == null)
                    break;

                if (ch.gen_data.skill_chosen[sn]
                    && Tables.skill_table[sn].rating[ch.klass] > 0)
                {
                    Comm.send_to_char(RomString.sprintf("%-18s %-5d ", Tables.skill_table[sn].name,
                        Tables.skill_table[sn].rating[ch.klass]), ch);
                    if (++col % 3 == 0)
                        Comm.send_to_char("\n\r", ch);
                }
            }
            if (col % 3 != 0)
                Comm.send_to_char("\n\r", ch);
            Comm.send_to_char("\n\r", ch);

            Comm.send_to_char(RomString.sprintf("Creation points: %d\n\r", ch.gen_data.points_chosen), ch);
            Comm.send_to_char(RomString.sprintf("Experience per level: %d\n\r",
                Handler.exp_per_level(ch, ch.gen_data.points_chosen)), ch);
        }

        public static void do_gain(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            CharData trainer;
            for (trainer = ch.in_room.people;
                 trainer != null; trainer = trainer.next_in_room)
                if (Bit.IS_NPC(trainer) && Bit.IS_SET(trainer.act, ACT_GAIN))
                    break;

            if (trainer == null || !Handler.can_see(ch, trainer))
            {
                Comm.send_to_char("You can't do that here.\n\r", ch);
                return;
            }

            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Interp.do_function(trainer, ActComm.do_say, "Pardon me?");
                return;
            }

            if (!RomString.str_prefix(arg, "list"))
            {
                int col = 0;

                Comm.send_to_char(RomString.sprintf("%-18s %-5s %-18s %-5s %-18s %-5s\n\r",
                    "group", "cost", "group", "cost", "group", "cost"), ch);

                for (int gn = 0; gn < MAX_GROUP; gn++)
                {
                    if (Tables.group_table[gn].name == null)
                        break;

                    if (!ch.pcdata.group_known[gn]
                        && Tables.group_table[gn].rating[ch.klass] > 0)
                    {
                        Comm.send_to_char(RomString.sprintf("%-18s %-5d ",
                            Tables.group_table[gn].name,
                            Tables.group_table[gn].rating[ch.klass]), ch);
                        if (++col % 3 == 0)
                            Comm.send_to_char("\n\r", ch);
                    }
                }
                if (col % 3 != 0)
                    Comm.send_to_char("\n\r", ch);

                Comm.send_to_char("\n\r", ch);

                col = 0;

                Comm.send_to_char(RomString.sprintf("%-18s %-5s %-18s %-5s %-18s %-5s\n\r",
                    "skill", "cost", "skill", "cost", "skill", "cost"), ch);

                for (int sn = 0; sn < MAX_SKILL; sn++)
                {
                    if (Tables.skill_table[sn].name == null)
                        break;

                    if (ch.pcdata.learned[sn] == 0
                        && Tables.skill_table[sn].rating[ch.klass] > 0
                        && Tables.skill_table[sn].spell_fun == (SpellFun)Magic.spell_null)
                    {
                        Comm.send_to_char(RomString.sprintf("%-18s %-5d ",
                            Tables.skill_table[sn].name,
                            Tables.skill_table[sn].rating[ch.klass]), ch);
                        if (++col % 3 == 0)
                            Comm.send_to_char("\n\r", ch);
                    }
                }
                if (col % 3 != 0)
                    Comm.send_to_char("\n\r", ch);
                return;
            }

            if (!RomString.str_prefix(arg, "convert"))
            {
                if (ch.practice < 10)
                {
                    Comm.act("$N tells you 'You are not yet ready.'",
                        ch, null, trainer, TO_CHAR);
                    return;
                }

                Comm.act("$N helps you apply your practice to training",
                    ch, null, trainer, TO_CHAR);
                ch.practice -= 10;
                ch.train += 1;
                return;
            }

            if (!RomString.str_prefix(arg, "points"))
            {
                if (ch.train < 2)
                {
                    Comm.act("$N tells you 'You are not yet ready.'",
                        ch, null, trainer, TO_CHAR);
                    return;
                }

                if (ch.pcdata.points <= 40)
                {
                    Comm.act("$N tells you 'There would be no point in that.'",
                        ch, null, trainer, TO_CHAR);
                    return;
                }

                Comm.act("$N trains you, and you feel more at ease with your skills.",
                    ch, null, trainer, TO_CHAR);

                ch.train -= 2;
                ch.pcdata.points -= 1;
                ch.exp = Handler.exp_per_level(ch, ch.pcdata.points) * ch.level;
                return;
            }

            int gnAdd = group_lookup(argument);
            if (gnAdd > 0)
            {
                if (ch.pcdata.group_known[gnAdd])
                {
                    Comm.act("$N tells you 'You already know that group!'",
                        ch, null, trainer, TO_CHAR);
                    return;
                }

                if (Tables.group_table[gnAdd].rating[ch.klass] <= 0)
                {
                    Comm.act("$N tells you 'That group is beyond your powers.'",
                        ch, null, trainer, TO_CHAR);
                    return;
                }

                if (ch.train < Tables.group_table[gnAdd].rating[ch.klass])
                {
                    Comm.act("$N tells you 'You are not yet ready for that group.'",
                        ch, null, trainer, TO_CHAR);
                    return;
                }

                gn_add(ch, gnAdd);
                Comm.act("$N trains you in the art of $t",
                    ch, Tables.group_table[gnAdd].name, trainer, TO_CHAR);
                ch.train -= Tables.group_table[gnAdd].rating[ch.klass];
                return;
            }

            int snAdd = Lookup.skill_lookup(argument);
            if (snAdd > -1)
            {
                if (Tables.skill_table[snAdd].spell_fun != (SpellFun)Magic.spell_null)
                {
                    Comm.act("$N tells you 'You must learn the full group.'",
                        ch, null, trainer, TO_CHAR);
                    return;
                }

                if (ch.pcdata.learned[snAdd] != 0)
                {
                    Comm.act("$N tells you 'You already know that skill!'",
                        ch, null, trainer, TO_CHAR);
                    return;
                }

                if (Tables.skill_table[snAdd].rating[ch.klass] <= 0)
                {
                    Comm.act("$N tells you 'That skill is beyond your powers.'",
                        ch, null, trainer, TO_CHAR);
                    return;
                }

                if (ch.train < Tables.skill_table[snAdd].rating[ch.klass])
                {
                    Comm.act("$N tells you 'You are not yet ready for that skill.'",
                        ch, null, trainer, TO_CHAR);
                    return;
                }

                ch.pcdata.learned[snAdd] = 1;
                Comm.act("$N trains you in the art of $t",
                    ch, Tables.skill_table[snAdd].name, trainer, TO_CHAR);
                ch.train -= Tables.skill_table[snAdd].rating[ch.klass];
                return;
            }

            Comm.act("$N tells you 'I do not understand...'", ch, null, trainer, TO_CHAR);
        }

        public static bool parse_gen_groups(CharData ch, string argument)
        {
            if (argument.Length == 0)
                return false;

            argument = RomString.one_argument(argument, out string arg);

            if (!RomString.str_prefix(arg, "help"))
            {
                if (argument.Length == 0)
                {
                    Interp.do_function(ch, Interp.do_help, "group help");
                    return true;
                }

                Interp.do_function(ch, Interp.do_help, argument);
                return true;
            }

            if (!RomString.str_prefix(arg, "add"))
            {
                if (argument.Length == 0)
                {
                    Comm.send_to_char("You must provide a skill name.\n\r", ch);
                    return true;
                }

                int gn = group_lookup(argument);
                if (gn != -1)
                {
                    if (ch.gen_data.group_chosen[gn] || ch.pcdata.group_known[gn])
                    {
                        Comm.send_to_char("You already know that group!\n\r", ch);
                        return true;
                    }

                    if (Tables.group_table[gn].rating[ch.klass] < 1)
                    {
                        Comm.send_to_char("That group is not available.\n\r", ch);
                        return true;
                    }

                    if (ch.gen_data.points_chosen +
                        Tables.group_table[gn].rating[ch.klass] > 300)
                    {
                        Comm.send_to_char(
                            "You cannot take more than 300 creation points.\n\r",
                            ch);
                        return true;
                    }

                    Comm.send_to_char(RomString.sprintf("%s group added\n\r", Tables.group_table[gn].name), ch);
                    ch.gen_data.group_chosen[gn] = true;
                    ch.gen_data.points_chosen += Tables.group_table[gn].rating[ch.klass];
                    gn_add(ch, gn);
                    ch.pcdata.points += Tables.group_table[gn].rating[ch.klass];
                    return true;
                }

                int sn = Lookup.skill_lookup(argument);
                if (sn != -1)
                {
                    if (ch.gen_data.skill_chosen[sn] || ch.pcdata.learned[sn] > 0)
                    {
                        Comm.send_to_char("You already know that skill!\n\r", ch);
                        return true;
                    }

                    if (Tables.skill_table[sn].rating[ch.klass] < 1
                        || Tables.skill_table[sn].spell_fun != (SpellFun)Magic.spell_null)
                    {
                        Comm.send_to_char("That skill is not available.\n\r", ch);
                        return true;
                    }

                    if (ch.gen_data.points_chosen +
                        Tables.skill_table[sn].rating[ch.klass] > 300)
                    {
                        Comm.send_to_char(
                            "You cannot take more than 300 creation points.\n\r",
                            ch);
                        return true;
                    }
                    Comm.send_to_char(RomString.sprintf("%s skill added\n\r", Tables.skill_table[sn].name), ch);
                    ch.gen_data.skill_chosen[sn] = true;
                    ch.gen_data.points_chosen += Tables.skill_table[sn].rating[ch.klass];
                    ch.pcdata.learned[sn] = 1;
                    ch.pcdata.points += Tables.skill_table[sn].rating[ch.klass];
                    return true;
                }

                Comm.send_to_char("No skills or groups by that name...\n\r", ch);
                return true;
            }

            if (string.Equals(arg, "drop", StringComparison.Ordinal))
            {
                if (argument.Length == 0)
                {
                    Comm.send_to_char("You must provide a skill to drop.\n\r", ch);
                    return true;
                }

                int gn = group_lookup(argument);
                if (gn != -1 && ch.gen_data.group_chosen[gn])
                {
                    Comm.send_to_char("Group dropped.\n\r", ch);
                    ch.gen_data.group_chosen[gn] = false;
                    ch.gen_data.points_chosen -= Tables.group_table[gn].rating[ch.klass];
                    gn_remove(ch, gn);
                    for (int i = 0; i < MAX_GROUP; i++)
                    {
                        if (ch.gen_data.group_chosen[gn])
                            gn_add(ch, gn);
                    }
                    ch.pcdata.points -= Tables.group_table[gn].rating[ch.klass];
                    return true;
                }

                int sn = Lookup.skill_lookup(argument);
                if (sn != -1 && ch.gen_data.skill_chosen[sn])
                {
                    Comm.send_to_char("Skill dropped.\n\r", ch);
                    ch.gen_data.skill_chosen[sn] = false;
                    ch.gen_data.points_chosen -= Tables.skill_table[sn].rating[ch.klass];
                    ch.pcdata.learned[sn] = 0;
                    ch.pcdata.points -= Tables.skill_table[sn].rating[ch.klass];
                    return true;
                }

                Comm.send_to_char("You haven't bought any such skill or group.\n\r", ch);
                return true;
            }

            if (!RomString.str_prefix(arg, "premise"))
            {
                Interp.do_function(ch, Interp.do_help, "premise");
                return true;
            }

            if (!RomString.str_prefix(arg, "list"))
            {
                list_group_costs(ch);
                return true;
            }

            if (!RomString.str_prefix(arg, "learned"))
            {
                list_group_chosen(ch);
                return true;
            }

            if (!RomString.str_prefix(arg, "info"))
            {
                Interp.do_function(ch, do_groups, argument);
                return true;
            }

            return false;
        }

        public static void do_groups(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            int col = 0;

            if (argument.Length == 0)
            {
                for (int gn = 0; gn < MAX_GROUP; gn++)
                {
                    if (Tables.group_table[gn].name == null)
                        break;
                    if (ch.pcdata.group_known[gn])
                    {
                        Comm.send_to_char(RomString.sprintf("%-20s ", Tables.group_table[gn].name), ch);
                        if (++col % 3 == 0)
                            Comm.send_to_char("\n\r", ch);
                    }
                }
                if (col % 3 != 0)
                    Comm.send_to_char("\n\r", ch);
                Comm.send_to_char(RomString.sprintf("Creation points: %d\n\r", ch.pcdata.points), ch);
                return;
            }

            if (!RomString.str_cmp(argument, "all"))
            {
                for (int gn = 0; gn < MAX_GROUP; gn++)
                {
                    if (Tables.group_table[gn].name == null)
                        break;
                    Comm.send_to_char(RomString.sprintf("%-20s ", Tables.group_table[gn].name), ch);
                    if (++col % 3 == 0)
                        Comm.send_to_char("\n\r", ch);
                }
                if (col % 3 != 0)
                    Comm.send_to_char("\n\r", ch);
                return;
            }

            int gnl = group_lookup(argument);
            if (gnl == -1)
            {
                Comm.send_to_char("No group of that name exist.\n\r", ch);
                Comm.send_to_char(
                    "Type 'groups all' or 'info all' for a full listing.\n\r", ch);
                return;
            }

            for (int sn = 0; sn < MAX_IN_GROUP; sn++)
            {
                if (sn >= Tables.group_table[gnl].spells.Length || Tables.group_table[gnl].spells[sn] == null)
                    break;
                Comm.send_to_char(RomString.sprintf("%-20s ", Tables.group_table[gnl].spells[sn]), ch);
                if (++col % 3 == 0)
                    Comm.send_to_char("\n\r", ch);
            }
            if (col % 3 != 0)
                Comm.send_to_char("\n\r", ch);
        }

        public static void gn_remove(CharData ch, int gn)
        {
            ch.pcdata.group_known[gn] = false;

            foreach (var spell in Tables.group_table[gn].spells)
            {
                if (spell == null) break;
                group_remove(ch, spell);
            }
        }

        public static void group_remove(CharData ch, string name)
        {
            int sn = Lookup.skill_lookup(name);

            if (sn != -1)
            {
                ch.pcdata.learned[sn] = 0;
                return;
            }

            int gn = group_lookup(name);

            if (gn != -1 && ch.pcdata.group_known[gn])
            {
                ch.pcdata.group_known[gn] = false;
                gn_remove(ch, gn);
            }
        }

        public static int group_lookup(string name)
        {
            name ??= "";
            for (int gn = 0; gn < Tables.group_table.Length; gn++)
            {
                var n = Tables.group_table[gn].name;
                if (n == null) break;
                if (name.Length == 0)
                    continue;
                if (Bit.LOWER(name[0]) == Bit.LOWER(n[0]) && !RomString.str_prefix(name, n))
                    return gn;
            }
            return -1;
        }

        public static void gn_add(CharData ch, int gn)
        {
            ch.pcdata.group_known[gn] = true;
            foreach (var spell in Tables.group_table[gn].spells)
            {
                if (spell == null) break;
                group_add(ch, spell, false);
            }
        }

        public static void group_add(CharData ch, string name, bool deduct)
        {
            if (Bit.IS_NPC(ch)) return;
            int sn = Lookup.skill_lookup(name);
            if (sn != -1)
            {
                if (ch.pcdata.learned[sn] == 0)
                {
                    ch.pcdata.learned[sn] = 1;
                    if (deduct)
                        ch.pcdata.points += Tables.skill_table[sn].rating[ch.klass];
                }
                return;
            }
            int gn = group_lookup(name);
            if (gn != -1)
            {
                if (!ch.pcdata.group_known[gn])
                {
                    ch.pcdata.group_known[gn] = true;
                    if (deduct)
                        ch.pcdata.points += Tables.group_table[gn].rating[ch.klass];
                }
                gn_add(ch, gn);
            }
        }

        public static void do_skills(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            int min_lev = 1, max_lev = LEVEL_HERO;
            bool fAll = false, found = false;

            if (argument.Length != 0)
            {
                fAll = true;

                if (RomString.str_prefix(argument, "all"))
                {
                    argument = RomString.one_argument(argument, out string arg);
                    if (!Interp.is_number(arg))
                    {
                        Comm.send_to_char("Arguments must be numerical or all.\n\r", ch);
                        return;
                    }
                    max_lev = Interp.atoi(arg);

                    if (max_lev < 1 || max_lev > LEVEL_HERO)
                    {
                        Comm.send_to_char(RomString.sprintf("Levels must be between 1 and %d.\n\r",
                            LEVEL_HERO), ch);
                        return;
                    }

                    if (argument.Length != 0)
                    {
                        argument = RomString.one_argument(argument, out arg);
                        if (!Interp.is_number(arg))
                        {
                            Comm.send_to_char("Arguments must be numerical or all.\n\r",
                                ch);
                            return;
                        }
                        min_lev = max_lev;
                        max_lev = Interp.atoi(arg);

                        if (max_lev < 1 || max_lev > LEVEL_HERO)
                        {
                            Comm.send_to_char(RomString.sprintf(
                                "Levels must be between 1 and %d.\n\r",
                                LEVEL_HERO), ch);
                            return;
                        }

                        if (min_lev > max_lev)
                        {
                            Comm.send_to_char("That would be silly.\n\r", ch);
                            return;
                        }
                    }
                }
            }

            var skill_list = new string[LEVEL_HERO + 1];
            var skill_columns = new int[LEVEL_HERO + 1];
            for (int level = 0; level < LEVEL_HERO + 1; level++)
            {
                skill_columns[level] = 0;
                skill_list[level] = "";
            }

            for (int sn = 0; sn < MAX_SKILL; sn++)
            {
                if (Tables.skill_table[sn].name == null)
                    break;

                int level = Tables.skill_table[sn].skill_level[ch.klass];
                if (level < LEVEL_HERO + 1
                    && (fAll || level <= ch.level)
                    && level >= min_lev && level <= max_lev
                    && Tables.skill_table[sn].spell_fun == (SpellFun)Magic.spell_null
                    && ch.pcdata.learned[sn] > 0)
                {
                    found = true;
                    level = Tables.skill_table[sn].skill_level[ch.klass];
                    string buf;
                    if (ch.level < level)
                        buf = RomString.sprintf("%-18s n/a      ", Tables.skill_table[sn].name);
                    else
                        buf = RomString.sprintf("%-18s %3d%%      ", Tables.skill_table[sn].name,
                            ch.pcdata.learned[sn]);

                    if (skill_list[level].Length == 0)
                        skill_list[level] = RomString.sprintf("\n\rLevel %2d: %s", level, buf);
                    else
                    {
                        if (++skill_columns[level] % 2 == 0)
                            skill_list[level] += "\n\r          ";
                        skill_list[level] += buf;
                    }
                }
            }

            if (!found)
            {
                Comm.send_to_char("No skills found.\n\r", ch);
                return;
            }

            var output = new System.Text.StringBuilder();
            for (int level = 0; level < LEVEL_HERO + 1; level++)
                if (skill_list[level].Length != 0)
                    output.Append(skill_list[level]);
            output.Append("\n\r");
            Comm.page_to_char(output.ToString(), ch);
        }

        public static void do_spells(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            int min_lev = 1, max_lev = LEVEL_HERO;
            bool fAll = false, found = false;

            if (argument.Length != 0)
            {
                fAll = true;

                if (RomString.str_prefix(argument, "all"))
                {
                    argument = RomString.one_argument(argument, out string arg);
                    if (!Interp.is_number(arg))
                    {
                        Comm.send_to_char("Arguments must be numerical or all.\n\r", ch);
                        return;
                    }
                    max_lev = Interp.atoi(arg);

                    if (max_lev < 1 || max_lev > LEVEL_HERO)
                    {
                        Comm.send_to_char(RomString.sprintf("Levels must be between 1 and %d.\n\r",
                            LEVEL_HERO), ch);
                        return;
                    }

                    if (argument.Length != 0)
                    {
                        argument = RomString.one_argument(argument, out arg);
                        if (!Interp.is_number(arg))
                        {
                            Comm.send_to_char("Arguments must be numerical or all.\n\r",
                                ch);
                            return;
                        }
                        min_lev = max_lev;
                        max_lev = Interp.atoi(arg);

                        if (max_lev < 1 || max_lev > LEVEL_HERO)
                        {
                            Comm.send_to_char(RomString.sprintf(
                                "Levels must be between 1 and %d.\n\r",
                                LEVEL_HERO), ch);
                            return;
                        }

                        if (min_lev > max_lev)
                        {
                            Comm.send_to_char("That would be silly.\n\r", ch);
                            return;
                        }
                    }
                }
            }

            var spell_list = new string[LEVEL_HERO + 1];
            var spell_columns = new int[LEVEL_HERO + 1];
            for (int level = 0; level < LEVEL_HERO + 1; level++)
            {
                spell_columns[level] = 0;
                spell_list[level] = "";
            }

            for (int sn = 0; sn < Tables.skill_table.Length; sn++)
            {
                if (Tables.skill_table[sn].name == null)
                    break;

                int level = Tables.skill_table[sn].skill_level[ch.klass];
                if (level < LEVEL_HERO + 1
                    && (fAll || level <= ch.level)
                    && level >= min_lev && level <= max_lev
                    && Tables.skill_table[sn].spell_fun != (SpellFun)Magic.spell_null
                    && ch.pcdata.learned[sn] > 0)
                {
                    found = true;
                    level = Tables.skill_table[sn].skill_level[ch.klass];
                    string buf;
                    if (ch.level < level)
                        buf = RomString.sprintf("%-18s n/a      ", Tables.skill_table[sn].name);
                    else
                    {
                        int mana = Bit.UMAX(Tables.skill_table[sn].min_mana,
                            100 / (2 + ch.level - level));
                        buf = RomString.sprintf("%-18s  %3d mana  ", Tables.skill_table[sn].name,
                            mana);
                    }

                    if (spell_list[level].Length == 0)
                        spell_list[level] = RomString.sprintf("\n\rLevel %2d: %s", level, buf);
                    else
                    {
                        if (++spell_columns[level] % 2 == 0)
                            spell_list[level] += "\n\r          ";
                        spell_list[level] += buf;
                    }
                }
            }

            if (!found)
            {
                Comm.send_to_char("No spells found.\n\r", ch);
                return;
            }

            var output = new System.Text.StringBuilder();
            for (int level = 0; level < LEVEL_HERO + 1; level++)
                if (spell_list[level].Length != 0)
                    output.Append(spell_list[level]);
            output.Append("\n\r");
            Comm.page_to_char(output.ToString(), ch);
        }
    }
}
