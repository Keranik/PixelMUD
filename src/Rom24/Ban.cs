using static Rom24.Merc;

namespace Rom24
{
    public static class Ban
    {
        public static void save_bans()
        {
            bool found = false;
            var path = Path.Combine(Game.area_dir, BAN_FILE);
            try
            {
                using var fp = new StreamWriter(path);
                for (var pban = Game.ban_list; pban != null; pban = pban.next)
                {
                    if (Bit.IS_SET(pban.ban_flags, BAN_PERMANENT))
                    {
                        found = true;
                        fp.Write("{0,-20} {1,-2} {2}\n", pban.name, pban.level,
                            Db.print_flags(pban.ban_flags));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{path}: {ex.Message}");
                return;
            }
            if (!found && File.Exists(path))
                File.Delete(path);
        }

        public static void load_bans()
        {
            var path = Path.Combine(Game.area_dir, BAN_FILE);
            if (!File.Exists(path))
                return;
            using var fp = new AreaReader(path);
            BanData ban_last = null;
            for (;;)
            {
                if (fp.Eof())
                    return;
                var pban = new BanData();
                pban.name = fp.fread_word();
                if (pban.name.Length == 0)
                    return;
                pban.level = fp.fread_number();
                pban.ban_flags = fp.fread_flag();
                fp.fread_to_eol();
                if (Game.ban_list == null)
                    Game.ban_list = pban;
                else
                    ban_last.next = pban;
                ban_last = pban;
            }
        }

        public static bool check_ban(string site, long type)
        {
            var host = RomString.capitalize(site);
            if (host.Length > 0)
                host = Bit.LOWER(host[0]) + host.Substring(1);

            for (var pban = Game.ban_list; pban != null; pban = pban.next)
            {
                if (!Bit.IS_SET(pban.ban_flags, type))
                    continue;

                if (Bit.IS_SET(pban.ban_flags, BAN_PREFIX)
                    && Bit.IS_SET(pban.ban_flags, BAN_SUFFIX)
                    && pban.name.IndexOf(host, StringComparison.Ordinal) >= 0)
                    return true;

                if (Bit.IS_SET(pban.ban_flags, BAN_PREFIX)
                    && !RomString.str_suffix(pban.name, host))
                    return true;

                if (Bit.IS_SET(pban.ban_flags, BAN_SUFFIX)
                    && !RomString.str_prefix(pban.name, host))
                    return true;
            }
            return false;
        }

        static void ban_site(CharData ch, string argument, bool fPerm)
        {
            argument = RomString.one_argument(argument, out string arg1);
            argument = RomString.one_argument(argument, out string arg2);

            if (arg1.Length == 0)
            {
                if (Game.ban_list == null)
                {
                    Comm.send_to_char("No sites banned at this time.\n\r", ch);
                    return;
                }
                var buffer = new System.Text.StringBuilder();
                buffer.Append("Banned sites  level  type     status\n\r");
                for (var pban = Game.ban_list; pban != null; pban = pban.next)
                {
                    string buf2 = RomString.sprintf("%s%s%s",
                        Bit.IS_SET(pban.ban_flags, BAN_PREFIX) ? "*" : "",
                        pban.name,
                        Bit.IS_SET(pban.ban_flags, BAN_SUFFIX) ? "*" : "");
                    buffer.Append(RomString.sprintf("%-12s    %-3d  %-7s  %s\n\r",
                        buf2, pban.level,
                        Bit.IS_SET(pban.ban_flags, BAN_NEWBIES) ? "newbies" :
                        Bit.IS_SET(pban.ban_flags, BAN_PERMIT) ? "permit" :
                        Bit.IS_SET(pban.ban_flags, BAN_ALL) ? "all" : "",
                        Bit.IS_SET(pban.ban_flags, BAN_PERMANENT) ? "perm" : "temp"));
                }
                Comm.page_to_char(buffer.ToString(), ch);
                return;
            }

            long type;
            if (arg2.Length == 0 || !RomString.str_prefix(arg2, "all"))
                type = BAN_ALL;
            else if (!RomString.str_prefix(arg2, "newbies"))
                type = BAN_NEWBIES;
            else if (!RomString.str_prefix(arg2, "permit"))
                type = BAN_PERMIT;
            else
            {
                Comm.send_to_char(
                    "Acceptable ban types are all, newbies, and permit.\n\r", ch);
                return;
            }

            string name = arg1;
            bool prefix = false, suffix = false;
            if (name.Length > 0 && name[0] == '*')
            {
                prefix = true;
                name = name.Substring(1);
            }
            if (name.Length > 0 && name[name.Length - 1] == '*')
            {
                suffix = true;
                name = name.Substring(0, name.Length - 1);
            }

            if (name.Length == 0)
            {
                Comm.send_to_char("You have to ban SOMETHING.\n\r", ch);
                return;
            }

            BanData prev = null;
            for (var pban = Game.ban_list; pban != null; prev = pban, pban = pban.next)
            {
                if (!RomString.str_cmp(name, pban.name))
                {
                    if (pban.level > Handler.get_trust(ch))
                    {
                        Comm.send_to_char("That ban was set by a higher power.\n\r", ch);
                        return;
                    }
                    if (prev == null)
                        Game.ban_list = pban.next;
                    else
                        prev.next = pban.next;
                }
            }

            var nban = new BanData();
            nban.name = name;
            nban.level = Handler.get_trust(ch);
            nban.ban_flags = type;
            if (prefix)
                Bit.SET_BIT(ref nban.ban_flags, BAN_PREFIX);
            if (suffix)
                Bit.SET_BIT(ref nban.ban_flags, BAN_SUFFIX);
            if (fPerm)
                Bit.SET_BIT(ref nban.ban_flags, BAN_PERMANENT);
            nban.next = Game.ban_list;
            Game.ban_list = nban;
            save_bans();
            Comm.send_to_char(RomString.sprintf("%s has been banned.\n\r", nban.name), ch);
        }

        public static void do_ban(CharData ch, string argument)
        {
            ban_site(ch, argument, false);
        }

        public static void do_permban(CharData ch, string argument)
        {
            ban_site(ch, argument, true);
        }

        public static void do_allow(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                Comm.send_to_char("Remove which site from the ban list?\n\r", ch);
                return;
            }

            BanData prev = null;
            for (var curr = Game.ban_list; curr != null; prev = curr, curr = curr.next)
            {
                if (!RomString.str_cmp(arg, curr.name))
                {
                    if (curr.level > Handler.get_trust(ch))
                    {
                        Comm.send_to_char(
                            "You are not powerful enough to lift that ban.\n\r", ch);
                        return;
                    }
                    if (prev == null)
                        Game.ban_list = Game.ban_list.next;
                    else
                        prev.next = curr.next;
                    Comm.send_to_char(RomString.sprintf("Ban on %s lifted.\n\r", arg), ch);
                    save_bans();
                    return;
                }
            }
            Comm.send_to_char("Site is not banned.\n\r", ch);
        }
    }
}
