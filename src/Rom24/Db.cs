using static Rom24.Merc;

namespace Rom24
{
    public static partial class Db
    {
        static AreaReader fpArea;

        public static void log_string(string str)
        {
            var t = DateTimeOffset.FromUnixTimeSeconds(Game.current_time).LocalDateTime.ToString("ddd MMM dd HH:mm:ss yyyy");
            Console.Error.WriteLine($"{t} :: {str}");
        }

        public static void log_f(string fmt, params object[] args)
            => log_string(RomString.sprintf(fmt, args));

        public static string get_extra_descr(string name, ExtraDescrData ed)
        {
            for (; ed != null; ed = ed.next)
            {
                if (Handler.is_name(name, ed.keyword))
                    return ed.description;
            }
            return null;
        }

        public static bool check_pet_affected(int vnum, AffectData paf)
        {
            var petIndex = Handler.get_mob_index(vnum);
            if (petIndex == null)
                return false;

            if (paf.where == TO_AFFECTS)
                if (Bit.IS_SET(petIndex.affected_by, paf.bitvector))
                    return true;

            return false;
        }

        public static void bug(string str, int param)
        {
            if (fpArea != null)
                log_string($"[*****] FILE: {Game.strArea} LINE: {fpArea.Line}");
            log_string("[*****] BUG: " + RomString.sprintf(str, param));
        }

        public static void bug(string str, object param) => log_string("[*****] BUG: " + RomString.sprintf(str, param));

        public static void append_file(CharData ch, string file, string str)
        {
            if (Bit.IS_NPC(ch) || str.Length == 0)
                return;

            try
            {
                int vnum = ch.in_room != null ? ch.in_room.vnum : 0;
                File.AppendAllText(file, RomString.sprintf("[%5d] %s: %s\n", vnum, ch.name, str));
            }
            catch
            {
                Comm.send_to_char("Could not open the file!\n\r", ch);
            }
        }

        public static string print_flags(long flag)
        {
            var sb = new System.Text.StringBuilder();
            for (int count = 0; count < 32; count++)
            {
                if ((flag & (1L << count)) != 0)
                    sb.Append(count < 26 ? (char)('A' + count) : (char)('a' + (count - 26)));
            }
            if (sb.Length == 0) sb.Append('0');
            return sb.ToString();
        }

        public static void boot_db()
        {
            Game.fBootDb = true;
            RomRandom.init_mm();
            Gsn.assign();
            Magic.assign_spells();

            long lhour = (Game.current_time - 650336715) / (PULSE_TICK / PULSE_PER_SECOND);
            Game.time_info.hour = (int)(lhour % 24);
            long lday = lhour / 24;
            Game.time_info.day = (int)(lday % 35);
            long lmonth = lday / 35;
            Game.time_info.month = (int)(lmonth % 17);
            Game.time_info.year = (int)(lmonth / 17);
            if (Game.time_info.hour < 5) Game.weather_info.sunlight = SUN_DARK;
            else if (Game.time_info.hour < 6) Game.weather_info.sunlight = SUN_RISE;
            else if (Game.time_info.hour < 19) Game.weather_info.sunlight = SUN_LIGHT;
            else if (Game.time_info.hour < 20) Game.weather_info.sunlight = SUN_SET;
            else Game.weather_info.sunlight = SUN_DARK;
            Game.weather_info.change = 0;
            Game.weather_info.mmhg = 960;
            Game.weather_info.mmhg += Game.time_info.month >= 7 && Game.time_info.month <= 12
                ? RomRandom.number_range(1, 50) : RomRandom.number_range(1, 80);
            if (Game.weather_info.mmhg <= 980) Game.weather_info.sky = SKY_LIGHTNING;
            else if (Game.weather_info.mmhg <= 1000) Game.weather_info.sky = SKY_RAINING;
            else if (Game.weather_info.mmhg <= 1020) Game.weather_info.sky = SKY_CLOUDY;
            else Game.weather_info.sky = SKY_CLOUDLESS;

            var listPath = Path.Combine(Game.area_dir, AREA_LIST);
            foreach (var line in File.ReadAllLines(listPath))
            {
                var name = FirstWord(line);
                if (name.Length == 0) continue;
                if (name[0] == '$') break;
                load_area_file(name);
            }

            fix_exits();
            fix_mobprogs();
            Game.fBootDb = false;
            convert_objects();
            area_update();
            Board.load_boards();
            Board.save_notes();
            Ban.load_bans();
            Music.load_songs();
            Imc.startup();
            log_string("Database booted.");
        }

        static string FirstWord(string line)
        {
            if (line == null) return "";
            int i = 0;
            while (i < line.Length && Bit.isspace(line[i])) i++;
            int start = i;
            while (i < line.Length && !Bit.isspace(line[i])) i++;
            return start == i ? "" : line.Substring(start, i - start);
        }

        static void load_area_file(string fileName)
        {
            Game.strArea = fileName;
            AreaReader fp;
            if (fileName.Length > 0 && fileName[0] == '-')
                fp = new AreaReader(Console.OpenStandardInput(), "-", ownsStream: false);
            else
                fp = new AreaReader(Path.Combine(Game.area_dir, fileName));
            using (fp)
            {
                fpArea = fp;
                Game.current_area = null;
                for (;;)
                {
                    if (fp.fread_letter() != '#')
                    {
                        bug("Boot_db: # not found.", 0);
                        Environment.Exit(1);
                    }
                    var word = fp.fread_word();
                    if (word.Length > 0 && word[0] == '$') break;
                    if (!RomString.str_cmp(word, "AREA")) load_area(fp);
                    else if (!RomString.str_cmp(word, "AREADATA")) new_load_area(fp);
                    else if (!RomString.str_cmp(word, "HELPS")) load_helps(fp, fileName);
                    else if (!RomString.str_cmp(word, "MOBILES")) load_mobiles(fp);
                    else if (!RomString.str_cmp(word, "OBJECTS")) load_objects(fp);
                    else if (!RomString.str_cmp(word, "RESETS")) load_resets(fp);
                    else if (!RomString.str_cmp(word, "ROOMS")) load_rooms(fp);
                    else if (!RomString.str_cmp(word, "SHOPS")) load_shops(fp);
                    else if (!RomString.str_cmp(word, "SOCIALS")) load_socials(fp);
                    else if (!RomString.str_cmp(word, "SPECIALS")) load_specials(fp);
                    else if (!RomString.str_cmp(word, "MOBPROGS")) load_mobprogs(fp);
                    else if (!RomString.str_cmp(word, "MOBOLD")) load_old_mob(fp);
                    else if (!RomString.str_cmp(word, "OBJOLD")) load_old_obj(fp);
                    else
                    {
                        bug("Boot_db: bad section name.", 0);
                        Environment.Exit(1);
                    }
                }
                fpArea = null;
            }
        }

        static void load_socials(AreaReader fp)
        {
            for (;;)
            {
                var social = new SocialType();
                var temp = fp.fread_word();
                if (temp == "#0")
                    return;
                social.name = temp;
                fp.fread_to_eol();

                temp = fp.fread_string_eol();
                if (temp == "$")
                    social.char_no_arg = null;
                else if (temp == "#")
                {
                    Game.social_table[Game.social_count] = social;
                    Game.social_count++;
                    continue;
                }
                else
                    social.char_no_arg = temp;

                temp = fp.fread_string_eol();
                if (temp == "$")
                    social.others_no_arg = null;
                else if (temp == "#")
                {
                    Game.social_table[Game.social_count] = social;
                    Game.social_count++;
                    continue;
                }
                else
                    social.others_no_arg = temp;

                temp = fp.fread_string_eol();
                if (temp == "$")
                    social.char_found = null;
                else if (temp == "#")
                {
                    Game.social_table[Game.social_count] = social;
                    Game.social_count++;
                    continue;
                }
                else
                    social.char_found = temp;

                temp = fp.fread_string_eol();
                if (temp == "$")
                    social.others_found = null;
                else if (temp == "#")
                {
                    Game.social_table[Game.social_count] = social;
                    Game.social_count++;
                    continue;
                }
                else
                    social.others_found = temp;

                temp = fp.fread_string_eol();
                if (temp == "$")
                    social.vict_found = null;
                else if (temp == "#")
                {
                    Game.social_table[Game.social_count] = social;
                    Game.social_count++;
                    continue;
                }
                else
                    social.vict_found = temp;

                temp = fp.fread_string_eol();
                if (temp == "$")
                    social.char_not_found = null;
                else if (temp == "#")
                {
                    Game.social_table[Game.social_count] = social;
                    Game.social_count++;
                    continue;
                }
                else
                    social.char_not_found = temp;

                temp = fp.fread_string_eol();
                if (temp == "$")
                    social.char_auto = null;
                else if (temp == "#")
                {
                    Game.social_table[Game.social_count] = social;
                    Game.social_count++;
                    continue;
                }
                else
                    social.char_auto = temp;

                temp = fp.fread_string_eol();
                if (temp == "$")
                    social.others_auto = null;
                else if (temp == "#")
                {
                    Game.social_table[Game.social_count] = social;
                    Game.social_count++;
                    continue;
                }
                else
                    social.others_auto = temp;

                Game.social_table[Game.social_count] = social;
                Game.social_count++;
            }
        }

        static void load_specials(AreaReader fp)
        {
            for (;;)
            {
                char letter = fp.fread_letter();
                switch (letter)
                {
                    default:
                        bug(RomString.sprintf("Load_specials: letter '%c' not *MS.", letter), 0);
                        Environment.Exit(1);
                        return;

                    case 'S':
                        return;

                    case '*':
                        break;

                    case 'M':
                    {
                        var pMobIndex = Handler.get_mob_index(fp.fread_number());
                        pMobIndex.spec_fun = Special.spec_lookup(fp.fread_word());
                        if (pMobIndex.spec_fun == null)
                        {
                            bug("Load_specials: 'M': vnum %d.", pMobIndex.vnum);
                            Environment.Exit(1);
                        }
                        break;
                    }
                }
                fp.fread_to_eol();
            }
        }

        static void load_mobprogs(AreaReader fp)
        {
            if (Game.area_last == null)
            {
                bug("Load_mobprogs: no #AREA seen yet.", 0);
                Environment.Exit(1);
            }
            for (;;)
            {
                if (fp.fread_letter() != '#')
                {
                    bug("Load_mobprogs: # not found.", 0);
                    Environment.Exit(1);
                }
                int vnum = fp.fread_number();
                if (vnum == 0) break;
                var pMprog = new MprogCode();
                pMprog.vnum = vnum;
                pMprog.code = fp.fread_string();
                pMprog.next = Game.mprog_list;
                Game.mprog_list = pMprog;
            }
        }

        static void load_area(AreaReader fp)
        {
            var pArea = new AreaData();
            pArea.file_name = fp.fread_string();
            log_f("Loading area %s", pArea.file_name);
            pArea.area_flags = AREA_LOADING;
            pArea.security = 9;
            pArea.builders = "None";
            pArea.vnum = Game.top_area;
            pArea.name = fp.fread_string();
            pArea.credits = fp.fread_string();
            pArea.min_vnum = fp.fread_number();
            pArea.max_vnum = fp.fread_number();
            pArea.age = 15;
            if (Game.area_first == null) Game.area_first = pArea;
            if (Game.area_last != null)
            {
                Game.area_last.next = pArea;
                Bit.REMOVE_BIT(ref Game.area_last.area_flags, AREA_LOADING);
            }
            Game.area_last = pArea;
            Game.current_area = pArea;
            Game.top_area++;
        }

        static void new_load_area(AreaReader fp)
        {
            var pArea = new AreaData();
            pArea.age = 15;
            pArea.file_name = Game.strArea;
            pArea.vnum = Game.top_area;
            pArea.name = "New Area";
            pArea.builders = "";
            pArea.security = 9;
            for (;;)
            {
                var word = fp.Eof() ? "End" : fp.fread_word();
                if (!RomString.str_cmp(word, "Name")) pArea.name = fp.fread_string();
                else if (!RomString.str_cmp(word, "Security")) pArea.security = fp.fread_number();
                else if (!RomString.str_cmp(word, "VNUMs"))
                {
                    pArea.min_vnum = fp.fread_number();
                    pArea.max_vnum = fp.fread_number();
                }
                else if (!RomString.str_cmp(word, "Builders")) pArea.builders = fp.fread_string();
                else if (!RomString.str_cmp(word, "Credits")) pArea.credits = fp.fread_string();
                else if (!RomString.str_cmp(word, "End"))
                {
                    if (Game.area_first == null) Game.area_first = pArea;
                    if (Game.area_last != null) Game.area_last.next = pArea;
                    Game.area_last = pArea;
                    Game.current_area = pArea;
                    Game.top_area++;
                    return;
                }
                else fp.fread_to_eol();
            }
        }

        static void load_helps(AreaReader fp, string fname)
        {
            for (;;)
            {
                int level = fp.fread_number();
                var keyword = fp.fread_string();
                if (keyword.Length > 0 && keyword[0] == '$') break;
                var pHelp = Recycle.new_help();
                pHelp.level = level;
                pHelp.keyword = keyword;
                pHelp.text = fp.fread_string();
                if (!RomString.str_cmp(pHelp.keyword, "greeting"))
                    Game.help_greeting = pHelp.text;
                if (Game.help_first == null) Game.help_first = pHelp;
                if (Game.help_last != null) Game.help_last.next = pHelp;
                Game.help_last = pHelp;
                Game.top_help++;
            }
        }

        static void load_rooms(AreaReader fp)
        {
            if (Game.area_last == null)
            {
                bug("Load_resets: no #AREA seen yet.", 0);
                Environment.Exit(1);
            }
            for (;;)
            {
                if (fp.fread_letter() != '#')
                {
                    bug("Load_rooms: # not found.", 0);
                    Environment.Exit(1);
                }
                int vnum = fp.fread_number();
                if (vnum == 0) break;
                Game.fBootDb = false;
                if (Handler.get_room_index(vnum) != null)
                {
                    bug("Load_rooms: vnum %d duplicated.", vnum);
                    Environment.Exit(1);
                }
                Game.fBootDb = true;
                var pRoomIndex = new RoomIndexData();
                pRoomIndex.owner = "";
                pRoomIndex.area = Game.area_last;
                pRoomIndex.vnum = vnum;
                pRoomIndex.name = fp.fread_string();
                pRoomIndex.description = fp.fread_string();
                fp.fread_number();
                pRoomIndex.room_flags = fp.fread_flag();
                if (vnum >= 3000 && vnum < 3400)
                    Bit.SET_BIT(ref pRoomIndex.room_flags, ROOM_LAW);
                pRoomIndex.sector_type = fp.fread_number();
                pRoomIndex.heal_rate = 100;
                pRoomIndex.mana_rate = 100;
                for (;;)
                {
                    char letter = fp.fread_letter();
                    if (letter == 'S') break;
                    if (letter == 'H') pRoomIndex.heal_rate = fp.fread_number();
                    else if (letter == 'M') pRoomIndex.mana_rate = fp.fread_number();
                    else if (letter == 'C') pRoomIndex.clan = Lookup.clan_lookup(fp.fread_string());
                    else if (letter == 'D')
                    {
                        int door = fp.fread_number();
                        if (door < 0 || door > 5)
                        {
                            bug("Fread_rooms: vnum %d has bad door number.", vnum);
                            Environment.Exit(1);
                        }
                        var pexit = new ExitData();
                        pexit.description = fp.fread_string();
                        pexit.keyword = fp.fread_string();
                        int locks = fp.fread_number();
                        pexit.key = fp.fread_number();
                        pexit.vnum = fp.fread_number();
                        pexit.orig_door = door;
                        switch (locks)
                        {
                            case 1: pexit.exit_info = pexit.rs_flags = (int)EX_ISDOOR; break;
                            case 2: pexit.exit_info = pexit.rs_flags = (int)(EX_ISDOOR | EX_PICKPROOF); break;
                            case 3: pexit.exit_info = pexit.rs_flags = (int)(EX_ISDOOR | EX_NOPASS); break;
                            case 4: pexit.exit_info = pexit.rs_flags = (int)(EX_ISDOOR | EX_NOPASS | EX_PICKPROOF); break;
                        }
                        pRoomIndex.exit[door] = pexit;
                        Game.top_exit++;
                    }
                    else if (letter == 'E')
                    {
                        var ed = Recycle.new_extra_descr();
                        ed.keyword = fp.fread_string();
                        ed.description = fp.fread_string();
                        ed.next = pRoomIndex.extra_descr;
                        pRoomIndex.extra_descr = ed;
                        Game.top_ed++;
                    }
                    else if (letter == 'O')
                        pRoomIndex.owner = fp.fread_string();
                    else
                    {
                        bug("Load_rooms: vnum %d has flag not 'DES'.", vnum);
                        Environment.Exit(1);
                    }
                }
                int iHash = vnum % MAX_KEY_HASH;
                pRoomIndex.next = Game.room_index_hash[iHash];
                Game.room_index_hash[iHash] = pRoomIndex;
                Game.top_room++;
                if (Game.top_vnum_room < vnum) Game.top_vnum_room = vnum;
                assign_area_vnum(vnum);
            }
        }

        static void assign_area_vnum(int vnum)
        {
            if (Game.area_last.min_vnum == 0 || Game.area_last.max_vnum == 0)
                Game.area_last.min_vnum = Game.area_last.max_vnum = vnum;
            if (vnum < Game.area_last.min_vnum) Game.area_last.min_vnum = vnum;
            if (vnum > Game.area_last.max_vnum) Game.area_last.max_vnum = vnum;
        }

        static void load_resets(AreaReader fp)
        {
            if (Game.area_last == null)
            {
                bug("Load_resets: no #AREA seen yet.", 0);
                Environment.Exit(1);
            }
            int rVnum = -1;
            for (;;)
            {
                char letter = fp.fread_letter();
                if (letter == 'S') break;
                if (letter == '*') { fp.fread_to_eol(); continue; }
                var pReset = Recycle.new_reset_data();
                pReset.command = letter;
                fp.fread_number();
                pReset.arg1 = fp.fread_number();
                pReset.arg2 = fp.fread_number();
                pReset.arg3 = (letter == 'G' || letter == 'R') ? 0 : fp.fread_number();
                pReset.arg4 = (letter == 'P' || letter == 'M') ? fp.fread_number() : 0;
                fp.fread_to_eol();
                switch (pReset.command)
                {
                    case 'M':
                    case 'O':
                        rVnum = pReset.arg3;
                        break;
                    case 'P':
                    case 'G':
                    case 'E':
                        break;
                    case 'D':
                    {
                        rVnum = pReset.arg1;
                        var pRoomIndex = Handler.get_room_index(rVnum);
                        ExitData pexit = null;
                        if (pReset.arg2 < 0
                            || pReset.arg2 >= MAX_DIR
                            || pRoomIndex == null
                            || (pexit = pRoomIndex.exit[pReset.arg2]) == null
                            || !Bit.IS_SET(pexit.rs_flags, EX_ISDOOR))
                        {
                            bug(RomString.sprintf("Load_resets: 'D': exit %d, room %d not door.",
                                pReset.arg2, pReset.arg1), 0);
                            Environment.Exit(1);
                        }
                        switch (pReset.arg3)
                        {
                            default:
                                bug("Load_resets: 'D': bad 'locks': %d.", pReset.arg3);
                                break;
                            case 0:
                                break;
                            case 1:
                                pexit.rs_flags |= (int)EX_CLOSED;
                                pexit.exit_info |= (int)EX_CLOSED;
                                break;
                            case 2:
                                pexit.rs_flags |= (int)(EX_CLOSED | EX_LOCKED);
                                pexit.exit_info |= (int)(EX_CLOSED | EX_LOCKED);
                                break;
                        }
                        break;
                    }
                    case 'R':
                        rVnum = pReset.arg1;
                        break;
                }
                if (rVnum == -1)
                {
                    bug("load_resets : rVnum == -1", 0);
                    Environment.Exit(1);
                }
                if (pReset.command != 'D')
                    new_reset(Handler.get_room_index(rVnum), pReset);
            }
        }

        static void new_reset(RoomIndexData pRoom, ResetData pReset)
        {
            if (pRoom == null) return;
            if (pRoom.reset_first == null) pRoom.reset_first = pReset;
            if (pRoom.reset_last != null) pRoom.reset_last.next = pReset;
            pRoom.reset_last = pReset;
            pReset.next = null;
        }

        static void load_shops(AreaReader fp)
        {
            for (;;)
            {
                int keeper = fp.fread_number();
                if (keeper == 0) break;
                var pShop = new ShopData();
                pShop.keeper = keeper;
                for (int i = 0; i < MAX_TRADE; i++) pShop.buy_type[i] = fp.fread_number();
                pShop.profit_buy = fp.fread_number();
                pShop.profit_sell = fp.fread_number();
                pShop.open_hour = fp.fread_number();
                pShop.close_hour = fp.fread_number();
                fp.fread_to_eol();
                var pMobIndex = Handler.get_mob_index(keeper);
                if (pMobIndex != null) pMobIndex.pShop = pShop;
                if (Game.shop_first == null) Game.shop_first = pShop;
                if (Game.shop_last != null) Game.shop_last.next = pShop;
                Game.shop_last = pShop;
                Game.top_shop++;
            }
        }

        static void load_mobiles(AreaReader fp)
        {
            if (Game.area_last == null)
            {
                bug("Load_mobiles: no #AREA seen yet.", 0);
                Environment.Exit(1);
            }
            for (;;)
            {
                if (fp.fread_letter() != '#')
                {
                    bug("Load_mobiles: # not found.", 0);
                    Environment.Exit(1);
                }
                int vnum = fp.fread_number();
                if (vnum == 0) break;
                Game.fBootDb = false;
                if (Handler.get_mob_index(vnum) != null)
                {
                    bug("Load_mobiles: vnum %d duplicated.", vnum);
                    Environment.Exit(1);
                }
                Game.fBootDb = true;
                var p = new MobIndexData();
                p.vnum = vnum;
                p.area = Game.area_last;
                p.new_format = true;
                Game.newmobs++;
                p.player_name = fp.fread_string();
                p.short_descr = fp.fread_string();
                p.long_descr = fp.fread_string();
                p.description = fp.fread_string();
                p.race = Lookup.race_lookup(fp.fread_string());
                if (p.long_descr.Length > 0)
                    p.long_descr = char.ToUpperInvariant(p.long_descr[0]) + p.long_descr.Substring(1);
                if (p.description.Length > 0)
                    p.description = char.ToUpperInvariant(p.description[0]) + p.description.Substring(1);
                var race = Tables.race_table[p.race];
                p.act = fp.fread_flag() | ACT_IS_NPC | (race?.act ?? 0);
                p.affected_by = fp.fread_flag() | (race?.aff ?? 0);
                p.alignment = fp.fread_number();
                p.group = fp.fread_number();
                p.level = fp.fread_number();
                p.hitroll = fp.fread_number();
                p.hit[DICE_NUMBER] = fp.fread_number(); fp.fread_letter();
                p.hit[DICE_TYPE] = fp.fread_number(); fp.fread_letter();
                p.hit[DICE_BONUS] = fp.fread_number();
                p.mana[DICE_NUMBER] = fp.fread_number(); fp.fread_letter();
                p.mana[DICE_TYPE] = fp.fread_number(); fp.fread_letter();
                p.mana[DICE_BONUS] = fp.fread_number();
                p.damage[DICE_NUMBER] = fp.fread_number(); fp.fread_letter();
                p.damage[DICE_TYPE] = fp.fread_number(); fp.fread_letter();
                p.damage[DICE_BONUS] = fp.fread_number();
                p.dam_type = Lookup.attack_lookup(fp.fread_word());
                p.ac[AC_PIERCE] = fp.fread_number() * 10;
                p.ac[AC_BASH] = fp.fread_number() * 10;
                p.ac[AC_SLASH] = fp.fread_number() * 10;
                p.ac[AC_EXOTIC] = fp.fread_number() * 10;
                p.off_flags = fp.fread_flag() | (race?.off ?? 0);
                p.imm_flags = fp.fread_flag() | (race?.imm ?? 0);
                p.res_flags = fp.fread_flag() | (race?.res ?? 0);
                p.vuln_flags = fp.fread_flag() | (race?.vuln ?? 0);
                p.start_pos = Lookup.position_lookup(fp.fread_word());
                p.default_pos = Lookup.position_lookup(fp.fread_word());
                p.sex = Lookup.sex_lookup(fp.fread_word());
                p.wealth = fp.fread_number();
                p.form = fp.fread_flag() | (race?.form ?? 0);
                p.parts = fp.fread_flag() | (race?.parts ?? 0);
                p.size = Lookup.size_lookup(fp.fread_word());
                if (p.size < 0) p.size = SIZE_MEDIUM;
                p.material = fp.fread_word();
                for (;;)
                {
                    char letter = fp.fread_letter();
                    if (letter == 'F')
                    {
                        var word = fp.fread_word();
                        long vector = fp.fread_flag();
                        if (!RomString.str_prefix(word, "act")) Bit.REMOVE_BIT(ref p.act, vector);
                        else if (!RomString.str_prefix(word, "aff")) Bit.REMOVE_BIT(ref p.affected_by, vector);
                        else if (!RomString.str_prefix(word, "off")) Bit.REMOVE_BIT(ref p.off_flags, vector);
                        else if (!RomString.str_prefix(word, "imm")) Bit.REMOVE_BIT(ref p.imm_flags, vector);
                        else if (!RomString.str_prefix(word, "res")) Bit.REMOVE_BIT(ref p.res_flags, vector);
                        else if (!RomString.str_prefix(word, "vul")) Bit.REMOVE_BIT(ref p.vuln_flags, vector);
                        else if (!RomString.str_prefix(word, "for")) Bit.REMOVE_BIT(ref p.form, vector);
                        else if (!RomString.str_prefix(word, "par")) Bit.REMOVE_BIT(ref p.parts, vector);
                    }
                    else if (letter == 'M')
                    {
                        var pMprog = new MprogList();
                        var word = fp.fread_word();
                        int trigger = Lookup.flag_lookup(word, Tables.mprog_flags);
                        Bit.SET_BIT(ref p.mprog_flags, trigger);
                        pMprog.trig_type = trigger;
                        pMprog.vnum = fp.fread_number();
                        pMprog.trig_phrase = fp.fread_string();
                        pMprog.next = p.mprogs;
                        p.mprogs = pMprog;
                    }
                    else
                    {
                        fp.Ungetc(letter);
                        break;
                    }
                }
                int iHash = vnum % MAX_KEY_HASH;
                p.next = Game.mob_index_hash[iHash];
                Game.mob_index_hash[iHash] = p;
                Game.top_mob_index++;
                if (Game.top_vnum_mob < vnum) Game.top_vnum_mob = vnum;
                assign_area_vnum(vnum);
            }
        }

        static void load_objects(AreaReader fp)
        {
            if (Game.area_last == null)
            {
                bug("Load_objects: no #AREA seen yet.", 0);
                Environment.Exit(1);
            }
            for (;;)
            {
                if (fp.fread_letter() != '#')
                {
                    bug("Load_objects: # not found.", 0);
                    Environment.Exit(1);
                }
                int vnum = fp.fread_number();
                if (vnum == 0) break;
                Game.fBootDb = false;
                if (Handler.get_obj_index(vnum) != null)
                {
                    bug("Load_objects: vnum %d duplicated.", vnum);
                    Environment.Exit(1);
                }
                Game.fBootDb = true;
                var p = new ObjIndexData();
                p.vnum = vnum;
                p.area = Game.area_last;
                p.new_format = true;
                Game.newobjs++;
                p.name = fp.fread_string();
                p.short_descr = fp.fread_string();
                p.description = fp.fread_string();
                p.material = fp.fread_string();
                p.item_type = Lookup.item_lookup(fp.fread_word());
                p.extra_flags = fp.fread_flag();
                p.wear_flags = fp.fread_flag();
                switch (p.item_type)
                {
                    case ITEM_WEAPON:
                        p.value[0] = Lookup.weapon_type(fp.fread_word());
                        p.value[1] = fp.fread_number();
                        p.value[2] = fp.fread_number();
                        p.value[3] = Lookup.attack_lookup(fp.fread_word());
                        p.value[4] = (int)fp.fread_flag();
                        break;
                    case ITEM_CONTAINER:
                        p.value[0] = fp.fread_number();
                        p.value[1] = (int)fp.fread_flag();
                        p.value[2] = fp.fread_number();
                        p.value[3] = fp.fread_number();
                        p.value[4] = fp.fread_number();
                        break;
                    case ITEM_DRINK_CON:
                    case ITEM_FOUNTAIN:
                        p.value[0] = fp.fread_number();
                        p.value[1] = fp.fread_number();
                        p.value[2] = Lookup.liq_lookup(fp.fread_word());
                        p.value[3] = fp.fread_number();
                        p.value[4] = fp.fread_number();
                        break;
                    case ITEM_WAND:
                    case ITEM_STAFF:
                        p.value[0] = fp.fread_number();
                        p.value[1] = fp.fread_number();
                        p.value[2] = fp.fread_number();
                        p.value[3] = Lookup.skill_lookup(fp.fread_word());
                        p.value[4] = fp.fread_number();
                        break;
                    case ITEM_POTION:
                    case ITEM_PILL:
                    case ITEM_SCROLL:
                        p.value[0] = fp.fread_number();
                        p.value[1] = Lookup.skill_lookup(fp.fread_word());
                        p.value[2] = Lookup.skill_lookup(fp.fread_word());
                        p.value[3] = Lookup.skill_lookup(fp.fread_word());
                        p.value[4] = Lookup.skill_lookup(fp.fread_word());
                        break;
                    default:
                        p.value[0] = (int)fp.fread_flag();
                        p.value[1] = (int)fp.fread_flag();
                        p.value[2] = (int)fp.fread_flag();
                        p.value[3] = (int)fp.fread_flag();
                        p.value[4] = (int)fp.fread_flag();
                        break;
                }
                p.level = fp.fread_number();
                p.weight = fp.fread_number();
                p.cost = fp.fread_number();
                char letter = fp.fread_letter();
                p.condition = letter switch
                {
                    'P' => 100, 'G' => 90, 'A' => 75, 'W' => 50, 'D' => 25, 'B' => 10, 'R' => 0, _ => 100
                };
                for (;;)
                {
                    letter = fp.fread_letter();
                    if (letter == 'A')
                    {
                        var paf = Recycle.new_affect();
                        paf.where = TO_OBJECT;
                        paf.type = -1;
                        paf.level = p.level;
                        paf.duration = -1;
                        paf.location = fp.fread_number();
                        paf.modifier = fp.fread_number();
                        paf.next = p.affected;
                        p.affected = paf;
                    }
                    else if (letter == 'F')
                    {
                        var paf = Recycle.new_affect();
                        char w = fp.fread_letter();
                        paf.where = w switch { 'A' => TO_AFFECTS, 'I' => TO_IMMUNE, 'R' => TO_RESIST, 'V' => TO_VULN, _ => TO_OBJECT };
                        paf.type = -1;
                        paf.level = p.level;
                        paf.duration = -1;
                        paf.location = fp.fread_number();
                        paf.modifier = fp.fread_number();
                        paf.bitvector = fp.fread_flag();
                        paf.next = p.affected;
                        p.affected = paf;
                    }
                    else if (letter == 'E')
                    {
                        var ed = Recycle.new_extra_descr();
                        ed.keyword = fp.fread_string();
                        ed.description = fp.fread_string();
                        ed.next = p.extra_descr;
                        p.extra_descr = ed;
                    }
                    else
                    {
                        fp.Ungetc(letter);
                        break;
                    }
                }
                int iHash = vnum % MAX_KEY_HASH;
                p.next = Game.obj_index_hash[iHash];
                Game.obj_index_hash[iHash] = p;
                Game.top_obj_index++;
                if (Game.top_vnum_obj < vnum) Game.top_vnum_obj = vnum;
                assign_area_vnum(vnum);
            }
        }

        static void fix_exits()
        {
            for (int iHash = 0; iHash < MAX_KEY_HASH; iHash++)
            {
                for (var pRoomIndex = Game.room_index_hash[iHash];
                     pRoomIndex != null; pRoomIndex = pRoomIndex.next)
                {
                    RoomIndexData iLastRoom = null, iLastObj = null;

                    for (var pReset = pRoomIndex.reset_first; pReset != null;
                         pReset = pReset.next)
                    {
                        switch (pReset.command)
                        {
                            default:
                                bug(RomString.sprintf(
                                    "fix_exits : room %d with reset cmd %c",
                                    pRoomIndex.vnum, pReset.command), 0);
                                Environment.Exit(1);
                                break;

                            case 'M':
                                Handler.get_mob_index(pReset.arg1);
                                iLastRoom = Handler.get_room_index(pReset.arg3);
                                break;

                            case 'O':
                                Handler.get_obj_index(pReset.arg1);
                                iLastObj = Handler.get_room_index(pReset.arg3);
                                break;

                            case 'P':
                                Handler.get_obj_index(pReset.arg1);
                                if (iLastObj == null)
                                {
                                    bug("fix_exits : reset in room %d with iLastObj NULL",
                                        pRoomIndex.vnum);
                                    Environment.Exit(1);
                                }
                                break;

                            case 'G':
                            case 'E':
                                Handler.get_obj_index(pReset.arg1);
                                if (iLastRoom == null)
                                {
                                    bug("fix_exits : reset in room %d with iLastRoom NULL",
                                        pRoomIndex.vnum);
                                    Environment.Exit(1);
                                }
                                iLastObj = iLastRoom;
                                break;

                            case 'D':
                                bug("???", 0);
                                break;

                            case 'R':
                                Handler.get_room_index(pReset.arg1);
                                if (pReset.arg2 < 0 || pReset.arg2 > MAX_DIR)
                                {
                                    bug(RomString.sprintf(
                                        "fix_exits : reset in room %d with arg2 %d >= MAX_DIR",
                                        pRoomIndex.vnum, pReset.arg2), 0);
                                    Environment.Exit(1);
                                }
                                break;
                        }
                    }

                    bool fexit = false;
                    for (int door = 0; door <= 5; door++)
                    {
                        var pexit = pRoomIndex.exit[door];
                        if (pexit != null)
                        {
                            if (pexit.vnum <= 0
                                || Handler.get_room_index(pexit.vnum) == null)
                                pexit.to_room = null;
                            else
                            {
                                fexit = true;
                                pexit.to_room = Handler.get_room_index(pexit.vnum);
                            }
                        }
                    }
                    if (!fexit)
                        Bit.SET_BIT(ref pRoomIndex.room_flags, ROOM_NO_MOB);
                }
            }

            for (int iHash = 0; iHash < MAX_KEY_HASH; iHash++)
            {
                for (var pRoomIndex = Game.room_index_hash[iHash];
                     pRoomIndex != null; pRoomIndex = pRoomIndex.next)
                {
                    for (int door = 0; door <= 5; door++)
                    {
                        var pexit = pRoomIndex.exit[door];
                        RoomIndexData to_room;
                        ExitData pexit_rev;
                        if (pexit != null
                            && (to_room = pexit.to_room) != null
                            && (pexit_rev = to_room.exit[ActMove.rev_dir[door]]) != null
                            && pexit_rev.to_room != pRoomIndex
                            && (pRoomIndex.vnum < 1200 || pRoomIndex.vnum > 1299))
                        {
                            string buf = RomString.sprintf(
                                "Fix_exits: %d:%d -> %d:%d -> %d.",
                                pRoomIndex.vnum, door,
                                to_room.vnum, ActMove.rev_dir[door],
                                pexit_rev.to_room == null
                                    ? 0 : pexit_rev.to_room.vnum);
                            bug(buf, 0);
                        }
                    }
                }
            }
        }

        static void fix_mobprogs()
        {
            for (int iHash = 0; iHash < MAX_KEY_HASH; iHash++)
            {
                for (var pMobIndex = Game.mob_index_hash[iHash];
                     pMobIndex != null; pMobIndex = pMobIndex.next)
                {
                    for (var list = pMobIndex.mprogs; list != null; list = list.next)
                    {
                        var prog = get_mprog_index(list.vnum);
                        if (prog != null)
                            list.code = prog.code;
                        else
                        {
                            bug("Fix_mobprogs: code vnum %d not found.", list.vnum);
                            Environment.Exit(1);
                        }
                    }
                }
            }
        }

        public static void area_update()
        {
            for (var pArea = Game.area_first; pArea != null; pArea = pArea.next)
            {
                if (++pArea.age < 3) continue;
                if ((!pArea.empty && (pArea.nplayer == 0 || pArea.age >= 15)) || pArea.age >= 31)
                {
                    reset_area(pArea);
                    Comm.wiznet(RomString.sprintf("%s has just been reset.", pArea.name),
                        null, null, WIZ_RESETS, 0, 0);
                    pArea.age = RomRandom.number_range(0, 3);
                    var school = Handler.get_room_index(ROOM_VNUM_SCHOOL);
                    if (school != null && pArea == school.area)
                        pArea.age = 15 - 2;
                    else if (pArea.nplayer == 0)
                        pArea.empty = true;
                }
            }
        }

        public static void reset_area(AreaData pArea)
        {
            for (int vnum = pArea.min_vnum; vnum <= pArea.max_vnum; vnum++)
            {
                var pRoom = Handler.get_room_index(vnum);
                if (pRoom != null) reset_room(pRoom);
            }
        }

        public static void reset_room(RoomIndexData pRoom)
        {
            CharData pMob;
            CharData LastMob = null;
            ObjData LastObj = null;
            int level = 0;
            bool last;

            if (pRoom == null)
                return;

            pMob = null;
            last = false;

            for (int iExit = 0; iExit < MAX_DIR; iExit++)
            {
                var pExit = pRoom.exit[iExit];
                if (pExit != null)
                {
                    pExit.exit_info = pExit.rs_flags;
                    if (pExit.to_room != null
                        && (pExit = pExit.to_room.exit[ActMove.rev_dir[iExit]]) != null)
                    {
                        pExit.exit_info = pExit.rs_flags;
                    }
                }
            }

            for (var pReset = pRoom.reset_first; pReset != null; pReset = pReset.next)
            {
                switch (pReset.command)
                {
                    default:
                        bug(RomString.sprintf("Reset_room: bad command %c.", pReset.command), 0);
                        break;

                    case 'M':
                    {
                        var pMobIndex = Handler.get_mob_index(pReset.arg1);
                        if (pMobIndex == null)
                        {
                            bug("Reset_room: 'M': bad vnum %d.", pReset.arg1);
                            continue;
                        }

                        var pRoomIndex = Handler.get_room_index(pReset.arg3);
                        if (pRoomIndex == null)
                        {
                            bug("Reset_area: 'R': bad vnum %d.", pReset.arg3);
                            continue;
                        }
                        if (pMobIndex.count >= pReset.arg2)
                        {
                            last = false;
                            break;
                        }
                        int count = 0;
                        for (var mob = pRoomIndex.people; mob != null; mob = mob.next_in_room)
                        {
                            if (mob.pIndexData == pMobIndex)
                            {
                                count++;
                                if (count >= pReset.arg4)
                                {
                                    last = false;
                                    break;
                                }
                            }
                        }

                        if (count >= pReset.arg4)
                            break;

                        pMob = create_mobile(pMobIndex);

                        if (Handler.room_is_dark(pRoom))
                            Bit.SET_BIT(ref pMob.affected_by, AFF_INFRARED);

                        {
                            var pRoomIndexPrev = Handler.get_room_index(pRoom.vnum - 1);
                            if (pRoomIndexPrev != null
                                && Bit.IS_SET(pRoomIndexPrev.room_flags, ROOM_PET_SHOP))
                                Bit.SET_BIT(ref pMob.act, ACT_PET);
                        }

                        Handler.char_to_room(pMob, pRoom);

                        LastMob = pMob;
                        level = Bit.URANGE(0, pMob.level - 2, LEVEL_HERO - 1);
                        last = true;
                        break;
                    }

                    case 'O':
                    {
                        var pObjIndex = Handler.get_obj_index(pReset.arg1);
                        if (pObjIndex == null)
                        {
                            bug("Reset_room: 'O' 1 : bad vnum %d", pReset.arg1);
                            bug(RomString.sprintf("%d %d %d %d", pReset.arg1, pReset.arg2,
                                pReset.arg3, pReset.arg4), 1);
                            continue;
                        }

                        var pRoomIndex = Handler.get_room_index(pReset.arg3);
                        if (pRoomIndex == null)
                        {
                            bug("Reset_room: 'O' 2 : bad vnum %d.", pReset.arg3);
                            bug(RomString.sprintf("%d %d %d %d", pReset.arg1, pReset.arg2,
                                pReset.arg3, pReset.arg4), 1);
                            continue;
                        }

                        if (pRoom.area.nplayer > 0
                            || Handler.count_obj_list(pObjIndex, pRoom.contents) > 0)
                        {
                            last = false;
                            break;
                        }

                        var pObj = create_object(pObjIndex,
                            Bit.UMIN(RomRandom.number_fuzzy(level), LEVEL_HERO - 1));
                        pObj.cost = 0;
                        Handler.obj_to_room(pObj, pRoom);
                        last = true;
                        break;
                    }

                    case 'P':
                    {
                        var pObjIndex = Handler.get_obj_index(pReset.arg1);
                        if (pObjIndex == null)
                        {
                            bug("Reset_room: 'P': bad vnum %d.", pReset.arg1);
                            continue;
                        }

                        var pObjToIndex = Handler.get_obj_index(pReset.arg3);
                        if (pObjToIndex == null)
                        {
                            bug("Reset_room: 'P': bad vnum %d.", pReset.arg3);
                            continue;
                        }

                        int limit;
                        if (pReset.arg2 > 50)
                            limit = 6;
                        else if (pReset.arg2 == -1)
                            limit = 999;
                        else
                            limit = pReset.arg2;

                        int count;
                        if (pRoom.area.nplayer > 0
                            || (LastObj = Handler.get_obj_type(pObjToIndex)) == null
                            || (LastObj.in_room == null && !last)
                            || (pObjIndex.count >= limit)
                            || (count = Handler.count_obj_list(pObjIndex, LastObj.contains)) > pReset.arg4)
                        {
                            last = false;
                            break;
                        }

                        while (count < pReset.arg4)
                        {
                            var pObj = create_object(pObjIndex,
                                RomRandom.number_fuzzy(LastObj.level));
                            Handler.obj_to_obj(pObj, LastObj);
                            count++;
                            if (pObjIndex.count >= limit)
                                break;
                        }

                        LastObj.value[1] = LastObj.pIndexData.value[1];
                        last = true;
                        break;
                    }

                    case 'G':
                    case 'E':
                    {
                        var pObjIndex = Handler.get_obj_index(pReset.arg1);
                        if (pObjIndex == null)
                        {
                            bug("Reset_room: 'E' or 'G': bad vnum %d.",
                                pReset.arg1);
                            continue;
                        }

                        if (!last)
                            break;

                        if (LastMob == null)
                        {
                            bug("Reset_room: 'E' or 'G': null mob for vnum %d.",
                                pReset.arg1);
                            last = false;
                            break;
                        }

                        ObjData pObj;
                        if (LastMob.pIndexData.pShop != null)
                        {
                            int olevel = 0;

                            if (!pObjIndex.new_format)
                                switch (pObjIndex.item_type)
                                {
                                    default:
                                        olevel = 0;
                                        break;
                                    case ITEM_PILL:
                                    case ITEM_POTION:
                                    case ITEM_SCROLL:
                                        olevel = 53;
                                        for (int i = 1; i < 5; i++)
                                        {
                                            if (pObjIndex.value[i] > 0)
                                            {
                                                for (int j = 0; j < MAX_CLASS; j++)
                                                {
                                                    olevel = Bit.UMIN(olevel,
                                                        Tables.skill_table[pObjIndex.value[i]].skill_level[j]);
                                                }
                                            }
                                        }

                                        olevel = Bit.UMAX(0, (olevel * 3 / 4) - 2);
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
                                        olevel = RomRandom.number_range(5, 15);
                                        break;
                                    case ITEM_TREASURE:
                                        olevel = RomRandom.number_range(10, 20);
                                        break;
                                }

                            pObj = create_object(pObjIndex, olevel);
                            Bit.SET_BIT(ref pObj.extra_flags, ITEM_INVENTORY);
                        }
                        else
                        {
                            int limit;
                            if (pReset.arg2 > 50)
                                limit = 6;
                            else if (pReset.arg2 == -1 || pReset.arg2 == 0)
                                limit = 999;
                            else
                                limit = pReset.arg2;

                            if (pObjIndex.count < limit || RomRandom.number_range(0, 4) == 0)
                            {
                                pObj = create_object(pObjIndex,
                                    Bit.UMIN(RomRandom.number_fuzzy(level),
                                        LEVEL_HERO - 1));
                                if (pObj.level > LastMob.level + 3
                                    || (pObj.item_type == ITEM_WEAPON
                                        && pReset.command == 'E'
                                        && pObj.level < LastMob.level - 5
                                        && pObj.level < 45))
                                {
                                    log_f("Check Levels:");
                                    log_f("  Object: (VNUM %5d)(Level %2d) %s",
                                        pObj.pIndexData.vnum,
                                        pObj.level,
                                        pObj.short_descr);
                                    log_f("     Mob: (VNUM %5d)(Level %2d) %s",
                                        LastMob.pIndexData.vnum,
                                        LastMob.level,
                                        LastMob.short_descr);
                                }
                            }
                            else
                                break;
                        }

                        Handler.obj_to_char(pObj, LastMob);
                        if (pReset.command == 'E')
                            Handler.equip_char(LastMob, pObj, pReset.arg3);
                        last = true;
                        break;
                    }

                    case 'D':
                        break;

                    case 'R':
                    {
                        var pRoomIndex = Handler.get_room_index(pReset.arg1);
                        if (pRoomIndex == null)
                        {
                            bug("Reset_room: 'R': bad vnum %d.", pReset.arg1);
                            continue;
                        }

                        for (int d0 = 0; d0 < pReset.arg2 - 1; d0++)
                        {
                            int d1 = RomRandom.number_range(d0, pReset.arg2 - 1);
                            var pExit = pRoomIndex.exit[d0];
                            pRoomIndex.exit[d0] = pRoomIndex.exit[d1];
                            pRoomIndex.exit[d1] = pExit;
                        }
                        break;
                    }
                }
            }
        }

        public static CharData create_mobile(MobIndexData pMobIndex)
        {
            Game.mobile_count++;

            if (pMobIndex == null)
            {
                bug("Create_mobile: NULL pMobIndex.", 0);
                Environment.Exit(1);
                return null;
            }

            var mob = Recycle.new_char();

            mob.pIndexData = pMobIndex;
            mob.name = pMobIndex.player_name;
            mob.short_descr = pMobIndex.short_descr;
            mob.long_descr = pMobIndex.long_descr;
            mob.description = pMobIndex.description;
            mob.id = Recycle.get_mob_id();
            mob.spec_fun = pMobIndex.spec_fun;
            mob.prompt = null;
            mob.mprog_target = null;

            if (pMobIndex.wealth == 0)
            {
                mob.silver = 0;
                mob.gold = 0;
            }
            else
            {
                long wealth = RomRandom.number_range((int)(pMobIndex.wealth / 2), (int)(3 * pMobIndex.wealth / 2));
                mob.gold = RomRandom.number_range((int)(wealth / 200), (int)(wealth / 100));
                mob.silver = wealth - mob.gold * 100;
            }

            if (pMobIndex.new_format)
            {
                mob.group = pMobIndex.group;
                mob.act = pMobIndex.act;
                mob.comm = COMM_NOCHANNELS | COMM_NOSHOUT | COMM_NOTELL;
                mob.affected_by = pMobIndex.affected_by;
                mob.alignment = pMobIndex.alignment;
                mob.level = pMobIndex.level;
                mob.hitroll = pMobIndex.hitroll;
                mob.damroll = pMobIndex.damage[DICE_BONUS];
                mob.max_hit = RomRandom.dice(pMobIndex.hit[DICE_NUMBER],
                    pMobIndex.hit[DICE_TYPE])
                    + pMobIndex.hit[DICE_BONUS];
                mob.hit = mob.max_hit;
                mob.max_mana = RomRandom.dice(pMobIndex.mana[DICE_NUMBER],
                    pMobIndex.mana[DICE_TYPE])
                    + pMobIndex.mana[DICE_BONUS];
                mob.mana = mob.max_mana;
                mob.damage[DICE_NUMBER] = pMobIndex.damage[DICE_NUMBER];
                mob.damage[DICE_TYPE] = pMobIndex.damage[DICE_TYPE];
                mob.dam_type = pMobIndex.dam_type;
                if (mob.dam_type == 0)
                    switch (RomRandom.number_range(1, 3))
                    {
                        case 1:
                            mob.dam_type = 3;
                            break;
                        case 2:
                            mob.dam_type = 7;
                            break;
                        case 3:
                            mob.dam_type = 11;
                            break;
                    }
                for (int i = 0; i < 4; i++)
                    mob.armor[i] = pMobIndex.ac[i];
                mob.off_flags = pMobIndex.off_flags;
                mob.imm_flags = pMobIndex.imm_flags;
                mob.res_flags = pMobIndex.res_flags;
                mob.vuln_flags = pMobIndex.vuln_flags;
                mob.start_pos = pMobIndex.start_pos;
                mob.default_pos = pMobIndex.default_pos;
                mob.sex = pMobIndex.sex;
                if (mob.sex == 3)
                    mob.sex = RomRandom.number_range(1, 2);
                mob.race = pMobIndex.race;
                mob.form = pMobIndex.form;
                mob.parts = pMobIndex.parts;
                mob.size = pMobIndex.size;
                mob.material = pMobIndex.material;

                for (int i = 0; i < MAX_STATS; i++)
                    mob.perm_stat[i] = Bit.UMIN(25, 11 + mob.level / 4);

                if (Bit.IS_SET(mob.act, ACT_WARRIOR))
                {
                    mob.perm_stat[STAT_STR] += 3;
                    mob.perm_stat[STAT_INT] -= 1;
                    mob.perm_stat[STAT_CON] += 2;
                }

                if (Bit.IS_SET(mob.act, ACT_THIEF))
                {
                    mob.perm_stat[STAT_DEX] += 3;
                    mob.perm_stat[STAT_INT] += 1;
                    mob.perm_stat[STAT_WIS] -= 1;
                }

                if (Bit.IS_SET(mob.act, ACT_CLERIC))
                {
                    mob.perm_stat[STAT_WIS] += 3;
                    mob.perm_stat[STAT_DEX] -= 1;
                    mob.perm_stat[STAT_STR] += 1;
                }

                if (Bit.IS_SET(mob.act, ACT_MAGE))
                {
                    mob.perm_stat[STAT_INT] += 3;
                    mob.perm_stat[STAT_STR] -= 1;
                    mob.perm_stat[STAT_DEX] += 1;
                }

                if (Bit.IS_SET(mob.off_flags, OFF_FAST))
                    mob.perm_stat[STAT_DEX] += 2;

                mob.perm_stat[STAT_STR] += mob.size - SIZE_MEDIUM;
                mob.perm_stat[STAT_CON] += (mob.size - SIZE_MEDIUM) / 2;

                if (Bit.IS_AFFECTED(mob, AFF_SANCTUARY))
                {
                    var af = Recycle.new_affect();
                    af.where = TO_AFFECTS;
                    af.type = Lookup.skill_lookup("sanctuary");
                    af.level = mob.level;
                    af.duration = -1;
                    af.location = APPLY_NONE;
                    af.modifier = 0;
                    af.bitvector = AFF_SANCTUARY;
                    Handler.affect_to_char(mob, af);
                }

                if (Bit.IS_AFFECTED(mob, AFF_HASTE))
                {
                    var af = Recycle.new_affect();
                    af.where = TO_AFFECTS;
                    af.type = Lookup.skill_lookup("haste");
                    af.level = mob.level;
                    af.duration = -1;
                    af.location = APPLY_DEX;
                    af.modifier = 1 + (mob.level >= 18 ? 1 : 0) + (mob.level >= 25 ? 1 : 0) +
                        (mob.level >= 32 ? 1 : 0);
                    af.bitvector = AFF_HASTE;
                    Handler.affect_to_char(mob, af);
                }

                if (Bit.IS_AFFECTED(mob, AFF_PROTECT_EVIL))
                {
                    var af = Recycle.new_affect();
                    af.where = TO_AFFECTS;
                    af.type = Lookup.skill_lookup("protection evil");
                    af.level = mob.level;
                    af.duration = -1;
                    af.location = APPLY_SAVES;
                    af.modifier = -1;
                    af.bitvector = AFF_PROTECT_EVIL;
                    Handler.affect_to_char(mob, af);
                }

                if (Bit.IS_AFFECTED(mob, AFF_PROTECT_GOOD))
                {
                    var af = Recycle.new_affect();
                    af.where = TO_AFFECTS;
                    af.type = Lookup.skill_lookup("protection good");
                    af.level = mob.level;
                    af.duration = -1;
                    af.location = APPLY_SAVES;
                    af.modifier = -1;
                    af.bitvector = AFF_PROTECT_GOOD;
                    Handler.affect_to_char(mob, af);
                }
            }
            else
            {
                mob.act = pMobIndex.act;
                mob.affected_by = pMobIndex.affected_by;
                mob.alignment = pMobIndex.alignment;
                mob.level = pMobIndex.level;
                mob.hitroll = pMobIndex.hitroll;
                mob.damroll = 0;
                mob.max_hit =
                    mob.level * 8 + RomRandom.number_range(mob.level * mob.level / 4,
                        mob.level * mob.level);
                mob.max_hit = (int)(mob.max_hit * 0.9);
                mob.hit = mob.max_hit;
                mob.max_mana = 100 + RomRandom.dice(mob.level, 10);
                mob.mana = mob.max_mana;
                switch (RomRandom.number_range(1, 3))
                {
                    case 1:
                        mob.dam_type = 3;
                        break;
                    case 2:
                        mob.dam_type = 7;
                        break;
                    case 3:
                        mob.dam_type = 11;
                        break;
                }
                for (int i = 0; i < 3; i++)
                    mob.armor[i] = RomRandom.interpolate(mob.level, 100, -100);
                mob.armor[3] = RomRandom.interpolate(mob.level, 100, 0);
                mob.race = pMobIndex.race;
                mob.off_flags = pMobIndex.off_flags;
                mob.imm_flags = pMobIndex.imm_flags;
                mob.res_flags = pMobIndex.res_flags;
                mob.vuln_flags = pMobIndex.vuln_flags;
                mob.start_pos = pMobIndex.start_pos;
                mob.default_pos = pMobIndex.default_pos;
                mob.sex = pMobIndex.sex;
                mob.form = pMobIndex.form;
                mob.parts = pMobIndex.parts;
                mob.size = SIZE_MEDIUM;
                mob.material = "";

                for (int i = 0; i < MAX_STATS; i++)
                    mob.perm_stat[i] = 11 + mob.level / 4;
            }

            mob.position = mob.start_pos;

            mob.next = Game.char_list;
            Game.char_list = mob;
            pMobIndex.count++;
            return mob;
        }

        public static ObjData create_object(ObjIndexData pObjIndex, int level)
        {
            if (pObjIndex == null)
            {
                bug("Create_object: NULL pObjIndex.", 0);
                Environment.Exit(1);
                return null;
            }

            var obj = Recycle.new_obj();

            obj.pIndexData = pObjIndex;
            obj.in_room = null;
            obj.enchanted = false;

            if (pObjIndex.new_format)
                obj.level = pObjIndex.level;
            else
                obj.level = Bit.UMAX(0, level);
            obj.wear_loc = -1;

            obj.name = pObjIndex.name;
            obj.short_descr = pObjIndex.short_descr;
            obj.description = pObjIndex.description;
            obj.material = pObjIndex.material;
            obj.item_type = pObjIndex.item_type;
            obj.extra_flags = pObjIndex.extra_flags;
            obj.wear_flags = pObjIndex.wear_flags;
            obj.value[0] = pObjIndex.value[0];
            obj.value[1] = pObjIndex.value[1];
            obj.value[2] = pObjIndex.value[2];
            obj.value[3] = pObjIndex.value[3];
            obj.value[4] = pObjIndex.value[4];
            obj.weight = pObjIndex.weight;

            if (level == -1 || pObjIndex.new_format)
                obj.cost = pObjIndex.cost;
            else
                obj.cost = RomRandom.number_fuzzy(10)
                    * RomRandom.number_fuzzy(level) * RomRandom.number_fuzzy(level);

            switch (obj.item_type)
            {
                default:
                    Db.bug("Read_object: vnum %d bad type.", pObjIndex.vnum);
                    break;

                case ITEM_LIGHT:
                    if (obj.value[2] == 999)
                        obj.value[2] = -1;
                    break;

                case ITEM_FURNITURE:
                case ITEM_TRASH:
                case ITEM_CONTAINER:
                case ITEM_DRINK_CON:
                case ITEM_KEY:
                case ITEM_FOOD:
                case ITEM_BOAT:
                case ITEM_CORPSE_NPC:
                case ITEM_CORPSE_PC:
                case ITEM_FOUNTAIN:
                case ITEM_MAP:
                case ITEM_CLOTHING:
                case ITEM_PORTAL:
                    if (!pObjIndex.new_format)
                        obj.cost /= 5;
                    break;

                case ITEM_TREASURE:
                case ITEM_WARP_STONE:
                case ITEM_ROOM_KEY:
                case ITEM_GEM:
                case ITEM_JEWELRY:
                    break;

                case ITEM_JUKEBOX:
                    for (int i = 0; i < 5; i++)
                        obj.value[i] = -1;
                    break;

                case ITEM_SCROLL:
                    if (level != -1 && !pObjIndex.new_format)
                        obj.value[0] = RomRandom.number_fuzzy(obj.value[0]);
                    break;

                case ITEM_WAND:
                case ITEM_STAFF:
                    if (level != -1 && !pObjIndex.new_format)
                    {
                        obj.value[0] = RomRandom.number_fuzzy(obj.value[0]);
                        obj.value[1] = RomRandom.number_fuzzy(obj.value[1]);
                        obj.value[2] = obj.value[1];
                    }
                    if (!pObjIndex.new_format)
                        obj.cost *= 2;
                    break;

                case ITEM_WEAPON:
                    if (level != -1 && !pObjIndex.new_format)
                    {
                        obj.value[1] =
                            RomRandom.number_fuzzy(RomRandom.number_fuzzy(1 * level / 4 + 2));
                        obj.value[2] =
                            RomRandom.number_fuzzy(RomRandom.number_fuzzy(3 * level / 4 + 6));
                    }
                    break;

                case ITEM_ARMOR:
                    if (level != -1 && !pObjIndex.new_format)
                    {
                        obj.value[0] = RomRandom.number_fuzzy(level / 5 + 3);
                        obj.value[1] = RomRandom.number_fuzzy(level / 5 + 3);
                        obj.value[2] = RomRandom.number_fuzzy(level / 5 + 3);
                    }
                    break;

                case ITEM_POTION:
                case ITEM_PILL:
                    if (level != -1 && !pObjIndex.new_format)
                        obj.value[0] = RomRandom.number_fuzzy(RomRandom.number_fuzzy(obj.value[0]));
                    break;

                case ITEM_MONEY:
                    if (!pObjIndex.new_format)
                        obj.value[0] = obj.cost;
                    break;
            }

            for (var paf = pObjIndex.affected; paf != null; paf = paf.next)
                if (paf.location == APPLY_SPELL_AFFECT)
                    Handler.affect_to_obj(obj, paf);

            obj.next = Game.object_list;
            Game.object_list = obj;
            pObjIndex.count++;

            return obj;
        }

        public static void clone_mobile(CharData parent, CharData clone)
        {
            if (parent == null || clone == null || !Bit.IS_NPC(parent))
                return;

            clone.name = parent.name;
            clone.version = parent.version;
            clone.short_descr = parent.short_descr;
            clone.long_descr = parent.long_descr;
            clone.description = parent.description;
            clone.group = parent.group;
            clone.sex = parent.sex;
            clone.klass = parent.klass;
            clone.race = parent.race;
            clone.level = parent.level;
            clone.trust = 0;
            clone.timer = parent.timer;
            clone.wait = parent.wait;
            clone.hit = parent.hit;
            clone.max_hit = parent.max_hit;
            clone.mana = parent.mana;
            clone.max_mana = parent.max_mana;
            clone.move = parent.move;
            clone.max_move = parent.max_move;
            clone.gold = parent.gold;
            clone.silver = parent.silver;
            clone.exp = parent.exp;
            clone.act = parent.act;
            clone.comm = parent.comm;
            clone.imm_flags = parent.imm_flags;
            clone.res_flags = parent.res_flags;
            clone.vuln_flags = parent.vuln_flags;
            clone.invis_level = parent.invis_level;
            clone.affected_by = parent.affected_by;
            clone.position = parent.position;
            clone.practice = parent.practice;
            clone.train = parent.train;
            clone.saving_throw = parent.saving_throw;
            clone.alignment = parent.alignment;
            clone.hitroll = parent.hitroll;
            clone.damroll = parent.damroll;
            clone.wimpy = parent.wimpy;
            clone.form = parent.form;
            clone.parts = parent.parts;
            clone.size = parent.size;
            clone.material = parent.material;
            clone.off_flags = parent.off_flags;
            clone.dam_type = parent.dam_type;
            clone.start_pos = parent.start_pos;
            clone.default_pos = parent.default_pos;
            clone.spec_fun = parent.spec_fun;

            for (int i = 0; i < 4; i++)
                clone.armor[i] = parent.armor[i];

            for (int i = 0; i < MAX_STATS; i++)
            {
                clone.perm_stat[i] = parent.perm_stat[i];
                clone.mod_stat[i] = parent.mod_stat[i];
            }

            for (int i = 0; i < 3; i++)
                clone.damage[i] = parent.damage[i];

            for (var paf = parent.affected; paf != null; paf = paf.next)
                Handler.affect_to_char(clone, paf);
        }

        public static void clone_object(ObjData parent, ObjData clone)
        {
            if (parent == null || clone == null)
                return;

            clone.name = parent.name;
            clone.short_descr = parent.short_descr;
            clone.description = parent.description;
            clone.item_type = parent.item_type;
            clone.extra_flags = parent.extra_flags;
            clone.wear_flags = parent.wear_flags;
            clone.weight = parent.weight;
            clone.cost = parent.cost;
            clone.level = parent.level;
            clone.condition = parent.condition;
            clone.material = parent.material;
            clone.timer = parent.timer;

            for (int i = 0; i < 5; i++)
                clone.value[i] = parent.value[i];

            clone.enchanted = parent.enchanted;

            for (var paf = parent.affected; paf != null; paf = paf.next)
                Handler.affect_to_obj(clone, paf);

            for (var ed = parent.extra_descr; ed != null; ed = ed.next)
            {
                var ed_new = Recycle.new_extra_descr();
                ed_new.keyword = ed.keyword;
                ed_new.description = ed.description;
                ed_new.next = clone.extra_descr;
                clone.extra_descr = ed_new;
            }
        }

        public static MprogCode get_mprog_index(int vnum)
        {
            for (var prg = Game.mprog_list; prg != null; prg = prg.next)
            {
                if (prg.vnum == vnum)
                    return prg;
            }
            return null;
        }

        public static void do_memory(CharData ch, string argument)
        {
            Comm.send_to_char(RomString.sprintf("Affects %5d\n\r", Game.top_affect), ch);
            Comm.send_to_char(RomString.sprintf("Areas   %5d\n\r", Game.top_area), ch);
            Comm.send_to_char(RomString.sprintf("ExDes   %5d\n\r", Game.top_ed), ch);
            Comm.send_to_char(RomString.sprintf("Exits   %5d\n\r", Game.top_exit), ch);
            Comm.send_to_char(RomString.sprintf("Helps   %5d\n\r", Game.top_help), ch);
            Comm.send_to_char(RomString.sprintf("Socials %5d\n\r", Game.social_count), ch);
            Comm.send_to_char(RomString.sprintf("Mobs    %5d(%d new format)\n\r", Game.top_mob_index, Game.newmobs), ch);
            Comm.send_to_char(RomString.sprintf("(in use)%5d\n\r", Game.mobile_count), ch);
            Comm.send_to_char(RomString.sprintf("Objs    %5d(%d new format)\n\r", Game.top_obj_index, Game.newobjs), ch);
            Comm.send_to_char(RomString.sprintf("Resets  %5d\n\r", Game.top_reset), ch);
            Comm.send_to_char(RomString.sprintf("Rooms   %5d\n\r", Game.top_room), ch);
            Comm.send_to_char(RomString.sprintf("Shops   %5d\n\r", Game.top_shop), ch);

            Comm.send_to_char(RomString.sprintf("Strings %5d strings of %7d bytes (max %d).\n\r",
                Game.nAllocString, Game.sAllocString, 1413120), ch);

            Comm.send_to_char(RomString.sprintf("Perms   %5d blocks  of %7d bytes.\n\r",
                Game.nAllocPerm, Game.sAllocPerm), ch);
        }

        public static void do_dump(CharData ch, string argument)
        {
            int count, count2, num_pcs, aff_count;
            int nMatch = 0;

            using (var fp = new StreamWriter("mem.dmp"))
            {
                num_pcs = 0;
                aff_count = 0;

                fp.Write(RomString.sprintf("MobProt    %4d (%8ld bytes)\n",
                    Game.top_mob_index, 0L));

                count = 0;
                for (var fch = Game.char_list; fch != null; fch = fch.next)
                {
                    count++;
                    if (fch.pcdata != null)
                        num_pcs++;
                    for (var af = fch.affected; af != null; af = af.next)
                        aff_count++;
                }
                count2 = 0;

                fp.Write(RomString.sprintf("Mobs    %4d (%8ld bytes), %2d free (%ld bytes)\n",
                    count, 0L, count2, 0L));

                count = 0;
                fp.Write(RomString.sprintf("Pcdata    %4d (%8ld bytes), %2d free (%ld bytes)\n",
                    num_pcs, 0L, count, 0L));

                count = 0;
                count2 = 0;
                for (var d = Game.descriptor_list; d != null; d = d.next)
                    count++;

                fp.Write(RomString.sprintf("Descs    %4d (%8ld bytes), %2d free (%ld bytes)\n",
                    count, 0L, count2, 0L));

                nMatch = 0;
                for (int vnum = 0; nMatch < Game.top_obj_index; vnum++)
                {
                    var pObjIndex = Handler.get_obj_index(vnum);
                    if (pObjIndex != null)
                    {
                        for (var af = pObjIndex.affected; af != null; af = af.next)
                            aff_count++;
                        nMatch++;
                    }
                }

                fp.Write(RomString.sprintf("ObjProt    %4d (%8ld bytes)\n",
                    Game.top_obj_index, 0L));

                count = 0;
                count2 = 0;
                for (var obj = Game.object_list; obj != null; obj = obj.next)
                {
                    count++;
                    for (var af = obj.affected; af != null; af = af.next)
                        aff_count++;
                }

                fp.Write(RomString.sprintf("Objs    %4d (%8ld bytes), %2d free (%ld bytes)\n",
                    count, 0L, count2, 0L));

                count = 0;
                fp.Write(RomString.sprintf("Affects    %4d (%8ld bytes), %2d free (%ld bytes)\n",
                    aff_count, 0L, count, 0L));

                fp.Write(RomString.sprintf("Rooms    %4d (%8ld bytes)\n",
                    Game.top_room, 0L));

                fp.Write(RomString.sprintf("Exits    %4d (%8ld bytes)\n",
                    Game.top_exit, 0L));
            }

            using (var fp = new StreamWriter("mob.dmp"))
            {
                fp.Write("\nMobile Analysis\n");
                fp.Write("---------------\n");
                nMatch = 0;
                for (int vnum = 0; nMatch < Game.top_mob_index; vnum++)
                {
                    var pMobIndex = Handler.get_mob_index(vnum);
                    if (pMobIndex != null)
                    {
                        nMatch++;
                        fp.Write(RomString.sprintf("#%-4d %3d active %3d killed     %s\n",
                            pMobIndex.vnum, pMobIndex.count,
                            pMobIndex.killed, pMobIndex.short_descr));
                    }
                }
            }

            using (var fp = new StreamWriter("obj.dmp"))
            {
                fp.Write("\nObject Analysis\n");
                fp.Write("---------------\n");
                nMatch = 0;
                for (int vnum = 0; nMatch < Game.top_obj_index; vnum++)
                {
                    var pObjIndex = Handler.get_obj_index(vnum);
                    if (pObjIndex != null)
                    {
                        nMatch++;
                        fp.Write(RomString.sprintf("#%-4d %3d active %3d reset      %s\n",
                            pObjIndex.vnum, pObjIndex.count,
                            pObjIndex.reset_num, pObjIndex.short_descr));
                    }
                }
            }
        }

        static void load_old_mob(AreaReader fp)
        {
            if (Game.area_last == null)
            {
                bug("Load_mobiles: no #AREA seen yet.", 0);
                Environment.Exit(1);
            }
            for (;;)
            {
                if (fp.fread_letter() != '#')
                {
                    bug("Load_mobiles: # not found.", 0);
                    Environment.Exit(1);
                }
                int vnum = fp.fread_number();
                if (vnum == 0)
                    break;
                Game.fBootDb = false;
                if (Handler.get_mob_index(vnum) != null)
                {
                    bug("Load_mobiles: vnum %d duplicated.", vnum);
                    Environment.Exit(1);
                }
                Game.fBootDb = true;

                var pMobIndex = new MobIndexData();
                pMobIndex.vnum = vnum;
                pMobIndex.area = Game.area_last;
                pMobIndex.new_format = false;
                pMobIndex.player_name = fp.fread_string() ?? "";
                pMobIndex.short_descr = fp.fread_string() ?? "";
                pMobIndex.long_descr = fp.fread_string() ?? "";
                pMobIndex.description = fp.fread_string() ?? "";
                if (pMobIndex.long_descr.Length > 0)
                    pMobIndex.long_descr = Bit.UPPER(pMobIndex.long_descr[0]) + pMobIndex.long_descr.Substring(1);
                if (pMobIndex.description.Length > 0)
                    pMobIndex.description = Bit.UPPER(pMobIndex.description[0]) + pMobIndex.description.Substring(1);

                pMobIndex.act = fp.fread_flag() | ACT_IS_NPC;
                pMobIndex.affected_by = fp.fread_flag();
                pMobIndex.pShop = null;
                pMobIndex.alignment = fp.fread_number();
                char letter = fp.fread_letter();
                pMobIndex.level = fp.fread_number();

                fp.fread_number();
                fp.fread_number();
                fp.fread_number();
                fp.fread_letter();
                fp.fread_number();
                fp.fread_letter();
                fp.fread_number();
                fp.fread_number();
                fp.fread_letter();
                fp.fread_number();
                fp.fread_letter();
                fp.fread_number();
                pMobIndex.wealth = fp.fread_number() / 20;
                fp.fread_number();
                pMobIndex.start_pos = fp.fread_number();
                pMobIndex.default_pos = fp.fread_number();
                if (pMobIndex.start_pos < POS_SLEEPING)
                    pMobIndex.start_pos = POS_STANDING;
                if (pMobIndex.default_pos < POS_SLEEPING)
                    pMobIndex.default_pos = POS_STANDING;
                pMobIndex.sex = fp.fread_number();

                RomString.one_argument(pMobIndex.player_name, out string name);
                int race;
                if (name.Length == 0 || (race = Lookup.race_lookup(name)) == 0)
                {
                    pMobIndex.race = Lookup.race_lookup("human");
                    pMobIndex.off_flags = OFF_DODGE | OFF_DISARM | OFF_TRIP | ASSIST_VNUM;
                    pMobIndex.imm_flags = 0;
                    pMobIndex.res_flags = 0;
                    pMobIndex.vuln_flags = 0;
                    pMobIndex.form = FORM_EDIBLE | FORM_SENTIENT | FORM_BIPED | FORM_MAMMAL;
                    pMobIndex.parts = PART_HEAD | PART_ARMS | PART_LEGS | PART_HEART | PART_BRAINS | PART_GUTS;
                }
                else
                {
                    pMobIndex.race = race;
                    pMobIndex.off_flags = OFF_DODGE | OFF_DISARM | OFF_TRIP | ASSIST_RACE
                        | Tables.race_table[race].off;
                    pMobIndex.imm_flags = Tables.race_table[race].imm;
                    pMobIndex.res_flags = Tables.race_table[race].res;
                    pMobIndex.vuln_flags = Tables.race_table[race].vuln;
                    pMobIndex.form = Tables.race_table[race].form;
                    pMobIndex.parts = Tables.race_table[race].parts;
                }

                if (letter != 'S')
                {
                    bug("Load_mobiles: vnum %d non-S.", vnum);
                    Environment.Exit(1);
                }

                convert_mobile(pMobIndex);

                int iHash = vnum % MAX_KEY_HASH;
                pMobIndex.next = Game.mob_index_hash[iHash];
                Game.mob_index_hash[iHash] = pMobIndex;
                Game.top_mob_index++;
                if (Game.top_vnum_mob < vnum) Game.top_vnum_mob = vnum;
                assign_area_vnum(vnum);
                Game.kill_table[Bit.URANGE(0, pMobIndex.level, MAX_LEVEL - 1)].number++;
            }
        }

        static void load_old_obj(AreaReader fp)
        {
            if (Game.area_last == null)
            {
                bug("Load_objects: no #AREA seen yet.", 0);
                Environment.Exit(1);
            }
            for (;;)
            {
                if (fp.fread_letter() != '#')
                {
                    bug("Load_objects: # not found.", 0);
                    Environment.Exit(1);
                }
                int vnum = fp.fread_number();
                if (vnum == 0)
                    break;
                Game.fBootDb = false;
                if (Handler.get_obj_index(vnum) != null)
                {
                    bug("Load_objects: vnum %d duplicated.", vnum);
                    Environment.Exit(1);
                }
                Game.fBootDb = true;

                var pObjIndex = new ObjIndexData();
                pObjIndex.vnum = vnum;
                pObjIndex.area = Game.area_last;
                pObjIndex.new_format = false;
                pObjIndex.reset_num = 0;
                pObjIndex.name = fp.fread_string() ?? "";
                pObjIndex.short_descr = fp.fread_string() ?? "";
                pObjIndex.description = fp.fread_string() ?? "";
                fp.fread_string();
                if (pObjIndex.short_descr.Length > 0)
                    pObjIndex.short_descr = Bit.LOWER(pObjIndex.short_descr[0]) + pObjIndex.short_descr.Substring(1);
                if (pObjIndex.description.Length > 0)
                    pObjIndex.description = Bit.UPPER(pObjIndex.description[0]) + pObjIndex.description.Substring(1);
                pObjIndex.material = "";
                pObjIndex.item_type = fp.fread_number();
                pObjIndex.extra_flags = fp.fread_flag();
                pObjIndex.wear_flags = fp.fread_flag();
                pObjIndex.value[0] = fp.fread_number();
                pObjIndex.value[1] = fp.fread_number();
                pObjIndex.value[2] = fp.fread_number();
                pObjIndex.value[3] = fp.fread_number();
                pObjIndex.value[4] = 0;
                pObjIndex.level = 0;
                pObjIndex.condition = 100;
                pObjIndex.weight = fp.fread_number();
                pObjIndex.cost = fp.fread_number();
                fp.fread_number();

                if (pObjIndex.item_type == ITEM_WEAPON)
                {
                    if (Handler.is_name("two", pObjIndex.name)
                        || Handler.is_name("two-handed", pObjIndex.name)
                        || Handler.is_name("claymore", pObjIndex.name))
                    {
                        int v4 = pObjIndex.value[4];
                        Bit.SET_BIT(ref v4, WEAPON_TWO_HANDS);
                        pObjIndex.value[4] = v4;
                    }
                }

                for (;;)
                {
                    char letter = fp.fread_letter();
                    if (letter == 'A')
                    {
                        var paf = Recycle.new_affect();
                        paf.where = TO_OBJECT;
                        paf.type = -1;
                        paf.level = 20;
                        paf.duration = -1;
                        paf.location = fp.fread_number();
                        paf.modifier = fp.fread_number();
                        paf.bitvector = 0;
                        paf.next = pObjIndex.affected;
                        pObjIndex.affected = paf;
                        Game.top_affect++;
                    }
                    else if (letter == 'E')
                    {
                        var ed = Recycle.new_extra_descr();
                        ed.keyword = fp.fread_string();
                        ed.description = fp.fread_string();
                        ed.next = pObjIndex.extra_descr;
                        pObjIndex.extra_descr = ed;
                        Game.top_ed++;
                    }
                    else
                    {
                        fp.Ungetc(letter);
                        break;
                    }
                }

                if (pObjIndex.item_type == ITEM_ARMOR)
                {
                    pObjIndex.value[1] = pObjIndex.value[0];
                    pObjIndex.value[2] = pObjIndex.value[1];
                }

                switch (pObjIndex.item_type)
                {
                    case ITEM_PILL:
                    case ITEM_POTION:
                    case ITEM_SCROLL:
                        pObjIndex.value[1] = Magic.slot_lookup(pObjIndex.value[1]);
                        pObjIndex.value[2] = Magic.slot_lookup(pObjIndex.value[2]);
                        pObjIndex.value[3] = Magic.slot_lookup(pObjIndex.value[3]);
                        pObjIndex.value[4] = Magic.slot_lookup(pObjIndex.value[4]);
                        break;
                    case ITEM_STAFF:
                    case ITEM_WAND:
                        pObjIndex.value[3] = Magic.slot_lookup(pObjIndex.value[3]);
                        break;
                }

                int iHash = vnum % MAX_KEY_HASH;
                pObjIndex.next = Game.obj_index_hash[iHash];
                Game.obj_index_hash[iHash] = pObjIndex;
                Game.top_obj_index++;
                if (Game.top_vnum_obj < vnum) Game.top_vnum_obj = vnum;
                assign_area_vnum(vnum);
            }
        }

        static void convert_objects()
        {
            if (Game.newobjs == Game.top_obj_index)
                return;

            MobIndexData pMob = null;
            for (var pArea = Game.area_first; pArea != null; pArea = pArea.next)
            {
                for (int vnum = pArea.min_vnum; vnum <= pArea.max_vnum; vnum++)
                {
                    var pRoom = Handler.get_room_index(vnum);
                    if (pRoom == null)
                        continue;
                    for (var pReset = pRoom.reset_first; pReset != null; pReset = pReset.next)
                    {
                        switch (pReset.command)
                        {
                            case 'M':
                                pMob = Handler.get_mob_index(pReset.arg1);
                                if (pMob == null)
                                    bug("Convert_objects: 'M': bad vnum %d.", pReset.arg1);
                                break;
                            case 'O':
                            {
                                var pObj = Handler.get_obj_index(pReset.arg1);
                                if (pObj == null)
                                {
                                    bug("Convert_objects: 'O': bad vnum %d.", pReset.arg1);
                                    break;
                                }
                                if (pObj.new_format)
                                    continue;
                                if (pMob == null)
                                {
                                    bug("Convert_objects: 'O': No mob reset yet.", 0);
                                    break;
                                }
                                pObj.level = pObj.level < 1 ? pMob.level - 2
                                    : Bit.UMIN(pObj.level, pMob.level - 2);
                                break;
                            }
                            case 'P':
                            {
                                var pObj = Handler.get_obj_index(pReset.arg1);
                                if (pObj == null)
                                {
                                    bug("Convert_objects: 'P': bad vnum %d.", pReset.arg1);
                                    break;
                                }
                                if (pObj.new_format)
                                    continue;
                                var pObjTo = Handler.get_obj_index(pReset.arg3);
                                if (pObjTo == null)
                                {
                                    bug("Convert_objects: 'P': bad vnum %d.", pReset.arg3);
                                    break;
                                }
                                pObj.level = pObj.level < 1 ? pObjTo.level
                                    : Bit.UMIN(pObj.level, pObjTo.level);
                                break;
                            }
                            case 'G':
                            case 'E':
                            {
                                var pObj = Handler.get_obj_index(pReset.arg1);
                                if (pObj == null)
                                {
                                    bug("Convert_objects: 'E' or 'G': bad vnum %d.", pReset.arg1);
                                    break;
                                }
                                if (pMob == null)
                                {
                                    bug("Convert_objects: 'E' or 'G': null mob for vnum %d.", pReset.arg1);
                                    break;
                                }
                                if (pObj.new_format)
                                    continue;
                                if (pMob.pShop != null)
                                {
                                    switch (pObj.item_type)
                                    {
                                        default:
                                            pObj.level = Bit.UMAX(0, pObj.level);
                                            break;
                                        case ITEM_PILL:
                                        case ITEM_POTION:
                                            pObj.level = Bit.UMAX(5, pObj.level);
                                            break;
                                        case ITEM_SCROLL:
                                        case ITEM_ARMOR:
                                        case ITEM_WEAPON:
                                            pObj.level = Bit.UMAX(10, pObj.level);
                                            break;
                                        case ITEM_WAND:
                                        case ITEM_TREASURE:
                                            pObj.level = Bit.UMAX(15, pObj.level);
                                            break;
                                        case ITEM_STAFF:
                                            pObj.level = Bit.UMAX(20, pObj.level);
                                            break;
                                    }
                                }
                                else
                                    pObj.level = pObj.level < 1 ? pMob.level
                                        : Bit.UMIN(pObj.level, pMob.level);
                                break;
                            }
                        }
                    }
                }
            }

            for (var pArea = Game.area_first; pArea != null; pArea = pArea.next)
            {
                for (int vnum = pArea.min_vnum; vnum <= pArea.max_vnum; vnum++)
                {
                    var pObj = Handler.get_obj_index(vnum);
                    if (pObj != null && !pObj.new_format)
                        convert_object(pObj);
                }
            }
        }

        static void convert_object(ObjIndexData pObjIndex)
        {
            if (pObjIndex == null || pObjIndex.new_format)
                return;
            int level = pObjIndex.level;
            pObjIndex.level = Bit.UMAX(0, pObjIndex.level);
            pObjIndex.cost = 10 * level;
            switch (pObjIndex.item_type)
            {
                default:
                    bug("Obj_convert: vnum %d bad type.", pObjIndex.item_type);
                    break;
                case ITEM_LIGHT:
                case ITEM_TREASURE:
                case ITEM_FURNITURE:
                case ITEM_TRASH:
                case ITEM_CONTAINER:
                case ITEM_DRINK_CON:
                case ITEM_KEY:
                case ITEM_FOOD:
                case ITEM_BOAT:
                case ITEM_CORPSE_NPC:
                case ITEM_CORPSE_PC:
                case ITEM_FOUNTAIN:
                case ITEM_MAP:
                case ITEM_CLOTHING:
                case ITEM_SCROLL:
                    break;
                case ITEM_WAND:
                case ITEM_STAFF:
                    pObjIndex.value[2] = pObjIndex.value[1];
                    break;
                case ITEM_WEAPON:
                {
                    int number = Bit.UMIN(level / 4 + 1, 5);
                    int type = (level + 7) / number;
                    pObjIndex.value[1] = number;
                    pObjIndex.value[2] = type;
                    break;
                }
                case ITEM_ARMOR:
                    pObjIndex.value[0] = level / 5 + 3;
                    pObjIndex.value[1] = pObjIndex.value[0];
                    pObjIndex.value[2] = pObjIndex.value[0];
                    break;
                case ITEM_POTION:
                case ITEM_PILL:
                    break;
                case ITEM_MONEY:
                    pObjIndex.value[0] = pObjIndex.cost;
                    break;
            }
            pObjIndex.new_format = true;
            Game.newobjs++;
        }

        static void convert_mobile(MobIndexData pMobIndex)
        {
            if (pMobIndex == null || pMobIndex.new_format)
                return;
            int level = pMobIndex.level;
            pMobIndex.act |= ACT_WARRIOR;
            int type = level * level * 27 / 40;
            int number = Bit.UMIN(type / 40 + 1, 10);
            type = Bit.UMAX(2, type / number);
            int bonus = Bit.UMAX(0, (int)(level * (8 + level) * 0.9 - number * type));
            pMobIndex.hit[DICE_NUMBER] = number;
            pMobIndex.hit[DICE_TYPE] = type;
            pMobIndex.hit[DICE_BONUS] = bonus;
            pMobIndex.mana[DICE_NUMBER] = level;
            pMobIndex.mana[DICE_TYPE] = 10;
            pMobIndex.mana[DICE_BONUS] = 100;
            type = level * 7 / 4;
            number = Bit.UMIN(type / 8 + 1, 5);
            type = Bit.UMAX(2, type / number);
            bonus = Bit.UMAX(0, level * 9 / 4 - number * type);
            pMobIndex.damage[DICE_NUMBER] = number;
            pMobIndex.damage[DICE_TYPE] = type;
            pMobIndex.damage[DICE_BONUS] = bonus;
            switch (RomRandom.number_range(1, 3))
            {
                case 1: pMobIndex.dam_type = 3; break;
                case 2: pMobIndex.dam_type = 7; break;
                case 3: pMobIndex.dam_type = 11; break;
            }
            for (int i = 0; i < 3; i++)
                pMobIndex.ac[i] = RomRandom.interpolate(level, 100, -100);
            pMobIndex.ac[3] = RomRandom.interpolate(level, 100, 0);
            pMobIndex.wealth /= 100;
            pMobIndex.size = SIZE_MEDIUM;
            pMobIndex.material = "none";
            pMobIndex.new_format = true;
            Game.newmobs++;
        }
    }
}
