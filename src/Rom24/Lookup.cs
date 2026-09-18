using static Rom24.Merc;

namespace Rom24
{
    public static class Lookup
    {
        public static int flag_lookup(string name, FlagType[] table)
        {
            name ??= "";
            if (name.Length == 0)
                return NO_FLAG;
            for (int flag = 0; table[flag].name != null; flag++)
            {
                if (Bit.LOWER(name[0]) == Bit.LOWER(table[flag].name[0])
                    && !RomString.str_prefix(name, table[flag].name))
                    return (int)table[flag].bit;
            }
            return NO_FLAG;
        }

        /* bit.c flag_stat_table: stat == TRUE */
        public static bool is_stat(FlagType[] flag_table)
        {
            return flag_table == Tables.sex_flags
                || flag_table == Tables.door_resets
                || flag_table == Tables.sector_flags
                || flag_table == Tables.type_flags
                || flag_table == Tables.apply_flags
                || flag_table == Tables.wear_loc_flags
                || flag_table == Tables.wear_loc_strings
                || flag_table == Tables.ac_type
                || flag_table == Tables.size_flags
                || flag_table == Tables.position_flags
                || flag_table == Tables.weapon_class
                || flag_table == Tables.apply_types;
        }

        public static int flag_value(FlagType[] flag_table, string argument)
        {
            if (is_stat(flag_table))
                return flag_lookup(argument, flag_table);

            int marked = 0;
            bool found = false;
            for (;;)
            {
                argument = RomString.one_argument(argument, out string word);
                if (word.Length == 0)
                    break;
                int bit = flag_lookup(word, flag_table);
                if (bit != NO_FLAG)
                {
                    Bit.SET_BIT(ref marked, bit);
                    found = true;
                }
            }
            return found ? marked : NO_FLAG;
        }

        public static string flag_string(FlagType[] flag_table, int bits)
        {
            string buf = "";
            for (int flag = 0; flag_table[flag].name != null; flag++)
            {
                if (!is_stat(flag_table) && Bit.IS_SET(bits, flag_table[flag].bit))
                    buf += " " + flag_table[flag].name;
                else if (flag_table[flag].bit == bits)
                {
                    buf += " " + flag_table[flag].name;
                    break;
                }
            }
            return buf.Length != 0 ? buf.Substring(1) : "none";
        }

        public static string flag_string(FlagType[] flag_table, long bits)
        {
            return flag_string(flag_table, (int)bits);
        }

        public static int wiznet_lookup(string name)
        {
            name ??= "";
            if (name.Length == 0)
                return -1;
            for (int flag = 0; Tables.wiznet_table[flag].name != null; flag++)
            {
                if (Bit.LOWER(name[0]) == Bit.LOWER(Tables.wiznet_table[flag].name[0])
                    && !RomString.str_prefix(name, Tables.wiznet_table[flag].name))
                    return flag;
            }
            return -1;
        }

        public static int clan_lookup(string name)
        {
            name ??= "";
            if (name.Length == 0)
                return 0;
            for (int clan = 0; clan < MAX_CLAN; clan++)
            {
                var n = Tables.clan_table[clan].name;
                if (!string.IsNullOrEmpty(n)
                    && Bit.LOWER(name[0]) == Bit.LOWER(n[0])
                    && !RomString.str_prefix(name, n))
                    return clan;
            }
            return 0;
        }

        public static int position_lookup(string name)
        {
            name ??= "";
            if (name.Length == 0)
                return -1;
            for (int pos = 0; Tables.position_table[pos].name != null; pos++)
            {
                if (Bit.LOWER(name[0]) == Bit.LOWER(Tables.position_table[pos].name[0])
                    && !RomString.str_prefix(name, Tables.position_table[pos].name))
                    return pos;
            }
            return -1;
        }

        public static int sex_lookup(string name)
        {
            name ??= "";
            if (name.Length == 0)
                return -1;
            for (int sex = 0; Tables.sex_table[sex].name != null; sex++)
            {
                if (Bit.LOWER(name[0]) == Bit.LOWER(Tables.sex_table[sex].name[0])
                    && !RomString.str_prefix(name, Tables.sex_table[sex].name))
                    return sex;
            }
            return -1;
        }

        public static int size_lookup(string name)
        {
            name ??= "";
            if (name.Length == 0)
                return -1;
            for (int size = 0; Tables.size_table[size].name != null; size++)
            {
                if (Bit.LOWER(name[0]) == Bit.LOWER(Tables.size_table[size].name[0])
                    && !RomString.str_prefix(name, Tables.size_table[size].name))
                    return size;
            }
            return -1;
        }

        public static int race_lookup(string name)
        {
            name ??= "";
            if (name.Length == 0)
                return 0;
            for (int race = 0; Tables.race_table[race] != null && Tables.race_table[race].name != null; race++)
            {
                if (Bit.LOWER(name[0]) == Bit.LOWER(Tables.race_table[race].name[0])
                    && !RomString.str_prefix(name, Tables.race_table[race].name))
                    return race;
            }
            return 0;
        }

        public static int item_lookup(string name)
        {
            name ??= "";
            if (name.Length == 0)
                return -1;
            for (int type = 0; Tables.item_table[type].name != null; type++)
            {
                if (Bit.LOWER(name[0]) == Bit.LOWER(Tables.item_table[type].name[0])
                    && !RomString.str_prefix(name, Tables.item_table[type].name))
                    return Tables.item_table[type].type;
            }
            return -1;
        }

        public static int liq_lookup(string name)
        {
            name ??= "";
            if (name.Length == 0)
                return -1;
            for (int liq = 0; Tables.liq_table[liq].liq_name != null; liq++)
            {
                if (Bit.LOWER(name[0]) == Bit.LOWER(Tables.liq_table[liq].liq_name[0])
                    && !RomString.str_prefix(name, Tables.liq_table[liq].liq_name))
                    return liq;
            }
            return -1;
        }

        public static int attack_lookup(string name)
        {
            name ??= "";
            if (name.Length == 0)
                return 0;
            for (int att = 0; Tables.attack_table[att].name != null; att++)
            {
                if (Bit.LOWER(name[0]) == Bit.LOWER(Tables.attack_table[att].name[0])
                    && !RomString.str_prefix(name, Tables.attack_table[att].name))
                    return att;
            }
            return 0;
        }

        public static int find_spell(CharData ch, string name)
        {
            int found = -1;

            if (Bit.IS_NPC(ch))
                return skill_lookup(name);

            name ??= "";
            if (name.Length == 0)
                return -1;

            for (int sn = 0; sn < MAX_SKILL; sn++)
            {
                if (Tables.skill_table[sn].name == null)
                    break;
                if (Bit.LOWER(name[0]) == Bit.LOWER(Tables.skill_table[sn].name[0])
                    && !RomString.str_prefix(name, Tables.skill_table[sn].name))
                {
                    if (found == -1)
                        found = sn;
                    if (ch.level >= Tables.skill_table[sn].skill_level[ch.klass]
                        && ch.pcdata.learned[sn] > 0)
                        return sn;
                }
            }
            return found;
        }

        public static int skill_lookup(string name)
        {
            if (string.IsNullOrEmpty(name) || name == "0") return -1;
            var table = Tables.skill_table;
            for (int sn = 0; sn < table.Length; sn++)
            {
                var n = table[sn].name;
                if (n == null) break;
                if (Bit.LOWER(name[0]) == Bit.LOWER(n[0]) && !RomString.str_prefix(name, n))
                    return sn;
            }
            return -1;
        }

        public static int weapon_lookup(string name)
        {
            name ??= "";
            if (name.Length == 0)
                return -1;
            for (int type = 0; Tables.weapon_table[type].name != null; type++)
            {
                if (Bit.LOWER(name[0]) == Bit.LOWER(Tables.weapon_table[type].name[0])
                    && !RomString.str_prefix(name, Tables.weapon_table[type].name))
                    return type;
            }
            return -1;
        }

        public static int class_lookup(string name)
        {
            name ??= "";
            if (name.Length == 0)
                return -1;
            for (int i = 0; i < MAX_CLASS; i++)
            {
                if (Bit.LOWER(name[0]) == Bit.LOWER(Tables.class_table[i].name[0])
                    && !RomString.str_prefix(name, Tables.class_table[i].name))
                    return i;
            }
            return -1;
        }

        public static SpecFun spec_lookup(string name) => Special.spec_lookup(name);

        public static string spec_name(SpecFun function) => Special.spec_name(function);

        public static int weapon_type(string name)
        {
            name ??= "";
            if (name.Length == 0)
                return WEAPON_EXOTIC;
            for (int type = 0; Tables.weapon_table[type].name != null; type++)
            {
                if (Bit.LOWER(name[0]) == Bit.LOWER(Tables.weapon_table[type].name[0])
                    && !RomString.str_prefix(name, Tables.weapon_table[type].name))
                    return Tables.weapon_table[type].type;
            }
            return WEAPON_EXOTIC;
        }
    }
}
