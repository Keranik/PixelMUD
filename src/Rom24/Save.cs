using static Rom24.Merc;

namespace Rom24
{
    public static class Save
    {
        const int MAX_NEST = 100;
        static readonly ObjData[] rgObjNest = new ObjData[MAX_NEST];

        public static string PLAYER_DIR => Game.player_dir.TrimEnd(Path.DirectorySeparatorChar, '/') + Path.DirectorySeparatorChar;

        public static bool load_char_obj(DescriptorData d, string name)
        {
            var ch = Recycle.new_char();
            ch.pcdata = Recycle.new_pcdata();

            d.character = ch;
            ch.desc = d;
            ch.name = name;
            ch.id = Recycle.get_pc_id();
            ch.race = Lookup.race_lookup("human");
            ch.act = PLR_NOSUMMON;
            ch.comm = COMM_COMBINE | COMM_PROMPT;
            ch.prompt = "<%hhp %mm %vmv> ";
            ch.pcdata.confirm_delete = false;
            ch.pcdata.board = Board.boards[Board.DEFAULT_BOARD];
            ch.pcdata.pwd = "";
            ch.pcdata.bamfin = "";
            ch.pcdata.bamfout = "";
            ch.pcdata.title = "";
            for (int stat = 0; stat < MAX_STATS; stat++)
                ch.perm_stat[stat] = 13;
            ch.pcdata.condition[COND_THIRST] = 48;
            ch.pcdata.condition[COND_FULL] = 48;
            ch.pcdata.condition[COND_HUNGER] = 48;
            ch.pcdata.security = 0;

            ch.pcdata.text[0] = NORMAL;
            ch.pcdata.text[1] = WHITE;
            ch.pcdata.text[2] = 0;
            ch.pcdata.auction[0] = BRIGHT;
            ch.pcdata.auction[1] = YELLOW;
            ch.pcdata.auction[2] = 0;
            ch.pcdata.auction_text[0] = BRIGHT;
            ch.pcdata.auction_text[1] = WHITE;
            ch.pcdata.auction_text[2] = 0;
            ch.pcdata.gossip[0] = NORMAL;
            ch.pcdata.gossip[1] = MAGENTA;
            ch.pcdata.gossip[2] = 0;
            ch.pcdata.gossip_text[0] = BRIGHT;
            ch.pcdata.gossip_text[1] = MAGENTA;
            ch.pcdata.gossip_text[2] = 0;
            ch.pcdata.music[0] = NORMAL;
            ch.pcdata.music[1] = RED;
            ch.pcdata.music[2] = 0;
            ch.pcdata.music_text[0] = BRIGHT;
            ch.pcdata.music_text[1] = RED;
            ch.pcdata.music_text[2] = 0;
            ch.pcdata.question[0] = BRIGHT;
            ch.pcdata.question[1] = YELLOW;
            ch.pcdata.question[2] = 0;
            ch.pcdata.question_text[0] = BRIGHT;
            ch.pcdata.question_text[1] = WHITE;
            ch.pcdata.question_text[2] = 0;
            ch.pcdata.answer[0] = BRIGHT;
            ch.pcdata.answer[1] = YELLOW;
            ch.pcdata.answer[2] = 0;
            ch.pcdata.answer_text[0] = BRIGHT;
            ch.pcdata.answer_text[1] = WHITE;
            ch.pcdata.answer_text[2] = 0;
            ch.pcdata.quote[0] = NORMAL;
            ch.pcdata.quote[1] = YELLOW;
            ch.pcdata.quote[2] = 0;
            ch.pcdata.quote_text[0] = NORMAL;
            ch.pcdata.quote_text[1] = GREEN;
            ch.pcdata.quote_text[2] = 0;
            ch.pcdata.immtalk_text[0] = NORMAL;
            ch.pcdata.immtalk_text[1] = CYAN;
            ch.pcdata.immtalk_text[2] = 0;
            ch.pcdata.immtalk_type[0] = NORMAL;
            ch.pcdata.immtalk_type[1] = YELLOW;
            ch.pcdata.immtalk_type[2] = 0;
            ch.pcdata.info[0] = BRIGHT;
            ch.pcdata.info[1] = YELLOW;
            ch.pcdata.info[2] = 1;
            ch.pcdata.say[0] = NORMAL;
            ch.pcdata.say[1] = GREEN;
            ch.pcdata.say[2] = 0;
            ch.pcdata.say_text[0] = BRIGHT;
            ch.pcdata.say_text[1] = GREEN;
            ch.pcdata.say_text[2] = 0;
            ch.pcdata.tell[0] = NORMAL;
            ch.pcdata.tell[1] = GREEN;
            ch.pcdata.tell[2] = 0;
            ch.pcdata.tell_text[0] = BRIGHT;
            ch.pcdata.tell_text[1] = GREEN;
            ch.pcdata.tell_text[2] = 0;
            ch.pcdata.reply[0] = NORMAL;
            ch.pcdata.reply[1] = GREEN;
            ch.pcdata.reply[2] = 0;
            ch.pcdata.reply_text[0] = BRIGHT;
            ch.pcdata.reply_text[1] = GREEN;
            ch.pcdata.reply_text[2] = 0;
            ch.pcdata.gtell_text[0] = NORMAL;
            ch.pcdata.gtell_text[1] = GREEN;
            ch.pcdata.gtell_text[2] = 0;
            ch.pcdata.gtell_type[0] = NORMAL;
            ch.pcdata.gtell_type[1] = RED;
            ch.pcdata.gtell_type[2] = 0;
            ch.pcdata.wiznet[0] = NORMAL;
            ch.pcdata.wiznet[1] = GREEN;
            ch.pcdata.wiznet[2] = 0;
            ch.pcdata.room_title[0] = NORMAL;
            ch.pcdata.room_title[1] = CYAN;
            ch.pcdata.room_title[2] = 0;
            ch.pcdata.room_text[0] = NORMAL;
            ch.pcdata.room_text[1] = WHITE;
            ch.pcdata.room_text[2] = 0;
            ch.pcdata.room_exits[0] = NORMAL;
            ch.pcdata.room_exits[1] = GREEN;
            ch.pcdata.room_exits[2] = 0;
            ch.pcdata.room_things[0] = NORMAL;
            ch.pcdata.room_things[1] = CYAN;
            ch.pcdata.room_things[2] = 0;
            ch.pcdata.prompt[0] = NORMAL;
            ch.pcdata.prompt[1] = CYAN;
            ch.pcdata.prompt[2] = 0;
            ch.pcdata.fight_death[0] = BRIGHT;
            ch.pcdata.fight_death[1] = RED;
            ch.pcdata.fight_death[2] = 0;
            ch.pcdata.fight_yhit[0] = NORMAL;
            ch.pcdata.fight_yhit[1] = GREEN;
            ch.pcdata.fight_yhit[2] = 0;
            ch.pcdata.fight_ohit[0] = NORMAL;
            ch.pcdata.fight_ohit[1] = YELLOW;
            ch.pcdata.fight_ohit[2] = 0;
            ch.pcdata.fight_thit[0] = NORMAL;
            ch.pcdata.fight_thit[1] = RED;
            ch.pcdata.fight_thit[2] = 0;
            ch.pcdata.fight_skill[0] = BRIGHT;
            ch.pcdata.fight_skill[1] = WHITE;
            ch.pcdata.fight_skill[2] = 0;
            Imc.initchar(ch);

            bool found = false;
            var path = Path.Combine(Game.player_dir, RomString.capitalize(name));
            if (File.Exists(path))
            {
                found = true;
                for (int iNest = 0; iNest < MAX_NEST; iNest++)
                    rgObjNest[iNest] = null;
                using var fp = new AreaReader(path);
                for (;;)
                {
                    if (fp.Eof()) break;
                    char letter = fp.fread_letter();
                    if (letter == '*')
                    {
                        fp.fread_to_eol();
                        continue;
                    }

                    if (letter != '#')
                    {
                        Db.bug("Load_char_obj: # not found.", 0);
                        break;
                    }

                    var word = fp.fread_word();
                    if (!RomString.str_cmp(word, "PLAYER"))
                        fread_char(ch, fp);
                    else if (!RomString.str_cmp(word, "OBJECT"))
                        fread_obj(ch, fp);
                    else if (!RomString.str_cmp(word, "O"))
                        fread_obj(ch, fp);
                    else if (!RomString.str_cmp(word, "PET"))
                        fread_pet(ch, fp);
                    else if (!RomString.str_cmp(word, "END"))
                        break;
                    else
                    {
                        Db.bug("Load_char_obj: bad section.", 0);
                        break;
                    }
                }
            }

            if (found)
            {
                if (ch.race == 0)
                    ch.race = Lookup.race_lookup("human");

                ch.size = Tables.pc_race_table[ch.race].size;
                ch.dam_type = 17;

                var skills = Tables.pc_race_table[ch.race].skills;
                for (int i = 0; i < 5; i++)
                {
                    if (skills == null || i >= skills.Length || skills[i] == null)
                        break;
                    Skills.group_add(ch, skills[i], false);
                }
                ch.affected_by = ch.affected_by | Tables.race_table[ch.race].aff;
                ch.imm_flags = ch.imm_flags | Tables.race_table[ch.race].imm;
                ch.res_flags = ch.res_flags | Tables.race_table[ch.race].res;
                ch.vuln_flags = ch.vuln_flags | Tables.race_table[ch.race].vuln;
                ch.form = Tables.race_table[ch.race].form;
                ch.parts = Tables.race_table[ch.race].parts;
            }

            if (found && ch.version < 2)
            {
                Skills.group_add(ch, "rom basics", false);
                Skills.group_add(ch, Tables.class_table[ch.klass].base_group, false);
                Skills.group_add(ch, Tables.class_table[ch.klass].default_group, true);
                ch.pcdata.learned[Gsn.recall] = 50;
            }

            if (found && ch.version < 3 && (ch.level > 35 || ch.trust > 35))
            {
                switch (ch.level)
                {
                    case 40:
                        ch.level = 60;
                        break;
                    case 39:
                        ch.level = 58;
                        break;
                    case 38:
                        ch.level = 56;
                        break;
                    case 37:
                        ch.level = 53;
                        break;
                }

                switch (ch.trust)
                {
                    case 40:
                        ch.trust = 60;
                        break;
                    case 39:
                        ch.trust = 58;
                        break;
                    case 38:
                        ch.trust = 56;
                        break;
                    case 37:
                        ch.trust = 53;
                        break;
                    case 36:
                        ch.trust = 51;
                        break;
                }
            }

            if (found && ch.version < 4)
            {
                ch.gold /= 100;
            }
            return found;
        }

        static void fread_char(CharData ch, AreaReader fp)
        {
            int lastlogoff = (int)Game.current_time;
            Db.log_string(RomString.sprintf("Loading %s.", ch.name));

            for (;;)
            {
                /* save.c: word = feof(fp) ? "End" : fread_word(fp); */
                var word = fp.Eof() ? "End" : fp.fread_word();
                bool fMatch = true;
                switch (word.Length == 0 ? '\0' : Bit.UPPER(word[0]))
                {
                    case '*': fp.fread_to_eol(); break;
                    case 'A':
                        if (!RomString.str_cmp(word, "Act")) ch.act = fp.fread_flag();
                        else if (!RomString.str_cmp(word, "AfBy") || !RomString.str_cmp(word, "AffectedBy")) ch.affected_by = fp.fread_flag();
                        else if (!RomString.str_cmp(word, "Alig") || !RomString.str_cmp(word, "Alignment")) ch.alignment = fp.fread_number();
                        else if (!RomString.str_cmp(word, "AC") || !RomString.str_cmp(word, "Armor"))
                            fp.fread_to_eol();
                        else if (!RomString.str_cmp(word, "ACs"))
                        { for (int i = 0; i < 4; i++) ch.armor[i] = fp.fread_number(); }
                        else if (!RomString.str_cmp(word, "Attr") || !RomString.str_cmp(word, "AttrPerm"))
                        { for (int i = 0; i < MAX_STATS; i++) ch.perm_stat[i] = fp.fread_number(); }
                        else if (!RomString.str_cmp(word, "AMod") || !RomString.str_cmp(word, "AttrMod"))
                        { for (int i = 0; i < MAX_STATS; i++) ch.mod_stat[i] = fp.fread_number(); }
                        else if (!RomString.str_cmp(word, "Affc") || !RomString.str_cmp(word, "AffD"))
                        {
                            var paf = Recycle.new_affect();
                            int sn = Lookup.skill_lookup(fp.fread_word());
                            if (sn < 0)
                                Db.bug("Fread_char: unknown skill.", 0);
                            else
                                paf.type = sn;
                            if (!RomString.str_cmp(word, "Affc"))
                                paf.where = fp.fread_number();
                            paf.level = fp.fread_number();
                            paf.duration = fp.fread_number();
                            paf.modifier = fp.fread_number();
                            paf.location = fp.fread_number();
                            paf.bitvector = fp.fread_number();
                            paf.next = ch.affected;
                            ch.affected = paf;
                        }
                        else if (!RomString.str_cmp(word, "Alias") || !RomString.str_cmp(word, "Alia"))
                        {
                            int pos;
                            for (pos = 0; pos < MAX_ALIAS; pos++)
                                if (ch.pcdata.alias[pos] == null) break;
                            if (pos >= MAX_ALIAS)
                                fp.fread_to_eol();
                            else
                            {
                                ch.pcdata.alias[pos] = fp.fread_word();
                                if (!RomString.str_cmp(word, "Alias"))
                                    ch.pcdata.alias_sub[pos] = fp.fread_string();
                                else
                                    ch.pcdata.alias_sub[pos] = fp.fread_word();
                            }
                        }
                        else fMatch = false;
                        break;
                    case 'C':
                        if (!RomString.str_cmp(word, "Cla") || !RomString.str_cmp(word, "Class")) ch.klass = fp.fread_number();
                        else if (!RomString.str_cmp(word, "Clan")) ch.clan = Lookup.clan_lookup(fp.fread_string());
                        else if (!RomString.str_cmp(word, "Comm")) ch.comm = fp.fread_flag();
                        else if (!RomString.str_cmp(word, "Cnd") || !RomString.str_cmp(word, "Cond") || !RomString.str_cmp(word, "Condition"))
                        {
                            ch.pcdata.condition[0] = fp.fread_number();
                            ch.pcdata.condition[1] = fp.fread_number();
                            ch.pcdata.condition[2] = fp.fread_number();
                            if (!RomString.str_cmp(word, "Cnd"))
                                ch.pcdata.condition[3] = fp.fread_number();
                        }
                        else if (!RomString.str_cmp(word, "Coloura"))
                        {
                            LoadColour(ch.pcdata.text, fp);
                            LoadColour(ch.pcdata.auction, fp);
                            LoadColour(ch.pcdata.gossip, fp);
                            LoadColour(ch.pcdata.music, fp);
                            LoadColour(ch.pcdata.question, fp);
                        }
                        else if (!RomString.str_cmp(word, "Colourb"))
                        {
                            LoadColour(ch.pcdata.answer, fp);
                            LoadColour(ch.pcdata.quote, fp);
                            LoadColour(ch.pcdata.quote_text, fp);
                            LoadColour(ch.pcdata.immtalk_text, fp);
                            LoadColour(ch.pcdata.immtalk_type, fp);
                        }
                        else if (!RomString.str_cmp(word, "Colourc"))
                        {
                            LoadColour(ch.pcdata.info, fp);
                            LoadColour(ch.pcdata.tell, fp);
                            LoadColour(ch.pcdata.reply, fp);
                            LoadColour(ch.pcdata.gtell_text, fp);
                            LoadColour(ch.pcdata.gtell_type, fp);
                        }
                        else if (!RomString.str_cmp(word, "Colourd"))
                        {
                            LoadColour(ch.pcdata.room_title, fp);
                            LoadColour(ch.pcdata.room_text, fp);
                            LoadColour(ch.pcdata.room_exits, fp);
                            LoadColour(ch.pcdata.room_things, fp);
                            LoadColour(ch.pcdata.prompt, fp);
                        }
                        else if (!RomString.str_cmp(word, "Coloure"))
                        {
                            LoadColour(ch.pcdata.fight_death, fp);
                            LoadColour(ch.pcdata.fight_yhit, fp);
                            LoadColour(ch.pcdata.fight_ohit, fp);
                            LoadColour(ch.pcdata.fight_thit, fp);
                            LoadColour(ch.pcdata.fight_skill, fp);
                        }
                        else if (!RomString.str_cmp(word, "Colourf"))
                        {
                            LoadColour(ch.pcdata.wiznet, fp);
                            LoadColour(ch.pcdata.say, fp);
                            LoadColour(ch.pcdata.say_text, fp);
                            LoadColour(ch.pcdata.tell_text, fp);
                            LoadColour(ch.pcdata.reply_text, fp);
                        }
                        else if (!RomString.str_cmp(word, "Colourg"))
                        {
                            LoadColour(ch.pcdata.auction_text, fp);
                            LoadColour(ch.pcdata.gossip_text, fp);
                            LoadColour(ch.pcdata.music_text, fp);
                            LoadColour(ch.pcdata.question_text, fp);
                            LoadColour(ch.pcdata.answer_text, fp);
                        }
                        else fMatch = false;
                        break;
                    case 'D':
                        if (!RomString.str_cmp(word, "Dam") || !RomString.str_cmp(word, "Damroll")) ch.damroll = fp.fread_number();
                        else if (!RomString.str_cmp(word, "Desc") || !RomString.str_cmp(word, "Description")) ch.description = fp.fread_string();
                        else fMatch = false;
                        break;
                    case 'E':
                        if (!RomString.str_cmp(word, "End"))
                        {
                            int percent =
                                ((int)Game.current_time - lastlogoff) * 25 / (2 * 60 * 60);

                            percent = Bit.UMIN(percent, 100);

                            if (percent > 0 && !Bit.IS_AFFECTED(ch, AFF_POISON)
                                && !Bit.IS_AFFECTED(ch, AFF_PLAGUE))
                            {
                                ch.hit += (ch.max_hit - ch.hit) * percent / 100;
                                ch.mana += (ch.max_mana - ch.mana) * percent / 100;
                                ch.move += (ch.max_move - ch.move) * percent / 100;
                            }
                            return;
                        }
                        else if (!RomString.str_cmp(word, "Exp")) ch.exp = fp.fread_number();
                        else fMatch = false;
                        break;
                    case 'G':
                        if (!RomString.str_cmp(word, "Gold")) ch.gold = fp.fread_number();
                        else if (!RomString.str_cmp(word, "Gr") || !RomString.str_cmp(word, "Group"))
                        {
                            var temp = fp.fread_word();
                            int gn = Skills.group_lookup(temp);
                            if (gn < 0)
                                Db.bug("Fread_char: unknown group. ", 0);
                            else
                                Skills.gn_add(ch, gn);
                        }
                        else fMatch = false;
                        break;
                    case 'H':
                        if (!RomString.str_cmp(word, "Hit") || !RomString.str_cmp(word, "Hitroll")) ch.hitroll = fp.fread_number();
                        else if (!RomString.str_cmp(word, "HMV") || !RomString.str_cmp(word, "HpManaMove"))
                        {
                            ch.hit = fp.fread_number(); ch.max_hit = fp.fread_number();
                            ch.mana = fp.fread_number(); ch.max_mana = fp.fread_number();
                            ch.move = fp.fread_number(); ch.max_move = fp.fread_number();
                        }
                        else if (!RomString.str_cmp(word, "HMVP") || !RomString.str_cmp(word, "HpManaMovePerm"))
                        {
                            ch.pcdata.perm_hit = fp.fread_number();
                            ch.pcdata.perm_mana = fp.fread_number();
                            ch.pcdata.perm_move = fp.fread_number();
                        }
                        else fMatch = false;
                        break;
                    case 'I':
                        if (!RomString.str_cmp(word, "Id")) ch.id = fp.fread_number();
                        else if (Imc.loadchar(ch, fp, word)) { }
                        else if (!RomString.str_cmp(word, "Invi") || !RomString.str_cmp(word, "InvisLevel")) ch.invis_level = fp.fread_number();
                        else if (!RomString.str_cmp(word, "Inco")) ch.incog_level = fp.fread_number();
                        else fMatch = false;
                        break;
                    case 'L':
                        if (!RomString.str_cmp(word, "Levl") || !RomString.str_cmp(word, "Level") || !RomString.str_cmp(word, "Lev")) ch.level = fp.fread_number();
                        else if (!RomString.str_cmp(word, "LnD") || !RomString.str_cmp(word, "LongDescr")) ch.long_descr = fp.fread_string();
                        else if (!RomString.str_cmp(word, "LastLevel")) ch.pcdata.last_level = fp.fread_number();
                        else if (!RomString.str_cmp(word, "LogO")) lastlogoff = fp.fread_number();
                        else if (!RomString.str_cmp(word, "LLev")) ch.pcdata.last_level = fp.fread_number();
                        else fMatch = false;
                        break;
                    case 'N':
                        if (!RomString.str_cmp(word, "Name")) ch.name = fp.fread_string();
                        else fMatch = false;
                        break;
                    case 'P':
                        if (!RomString.str_cmp(word, "Pass") || !RomString.str_cmp(word, "Password")) ch.pcdata.pwd = fp.fread_string();
                        else if (!RomString.str_cmp(word, "Plyd") || !RomString.str_cmp(word, "Played")) ch.played = fp.fread_number();
                        else if (!RomString.str_cmp(word, "Pos") || !RomString.str_cmp(word, "Position")) ch.position = fp.fread_number();
                        else if (!RomString.str_cmp(word, "Prac") || !RomString.str_cmp(word, "Practice")) ch.practice = fp.fread_number();
                        else if (!RomString.str_cmp(word, "Pnts") || !RomString.str_cmp(word, "Points")) ch.pcdata.points = fp.fread_number();
                        else if (!RomString.str_cmp(word, "Prom") || !RomString.str_cmp(word, "Prompt")) ch.prompt = fp.fread_string();
                        else fMatch = false;
                        break;
                    case 'R':
                        if (!RomString.str_cmp(word, "Race")) ch.race = Lookup.race_lookup(fp.fread_string());
                        else if (!RomString.str_cmp(word, "Room"))
                        {
                            ch.in_room = Handler.get_room_index(fp.fread_number());
                            if (ch.in_room == null) ch.in_room = Handler.get_room_index(ROOM_VNUM_LIMBO);
                        }
                        else fMatch = false;
                        break;
                    case 'S':
                        if (!RomString.str_cmp(word, "Sex")) ch.sex = fp.fread_number();
                        else if (!RomString.str_cmp(word, "Scro")) ch.lines = fp.fread_number();
                        else if (!RomString.str_cmp(word, "Save") || !RomString.str_cmp(word, "SavingThrow")) ch.saving_throw = fp.fread_number();
                        else if (!RomString.str_cmp(word, "Sec")) ch.pcdata.security = fp.fread_number();
                        else if (!RomString.str_cmp(word, "Silv")) ch.silver = fp.fread_number();
                        else if (!RomString.str_cmp(word, "ShD") || !RomString.str_cmp(word, "ShortDescr")) ch.short_descr = fp.fread_string();
                        else if (!RomString.str_cmp(word, "Sk") || !RomString.str_cmp(word, "Skill"))
                        {
                            int value = fp.fread_number();
                            var temp = fp.fread_word();
                            int sn = Lookup.skill_lookup(temp);
                            if (sn < 0)
                                Db.bug("Fread_char: unknown skill. ", 0);
                            else
                                ch.pcdata.learned[sn] = value;
                        }
                        else fMatch = false;
                        break;
                    case 'T':
                        if (!RomString.str_cmp(word, "Title") || !RomString.str_cmp(word, "Titl"))
                        {
                            ch.pcdata.title = fp.fread_string();
                            if (ch.pcdata.title == null)
                                ch.pcdata.title = "";
                            if (ch.pcdata.title.Length > 0
                                && ch.pcdata.title[0] != '.'
                                && ch.pcdata.title[0] != ','
                                && ch.pcdata.title[0] != '!'
                                && ch.pcdata.title[0] != '?')
                            {
                                ch.pcdata.title = " " + ch.pcdata.title;
                            }
                        }
                        else if (!RomString.str_cmp(word, "Trai")) ch.train = fp.fread_number();
                        else if (!RomString.str_cmp(word, "TSex") || !RomString.str_cmp(word, "TrueSex")) ch.pcdata.true_sex = fp.fread_number();
                        else if (!RomString.str_cmp(word, "Tru") || !RomString.str_cmp(word, "Trust")) ch.trust = fp.fread_number();
                        else fMatch = false;
                        break;
                    case 'V':
                        if (!RomString.str_cmp(word, "Vers") || !RomString.str_cmp(word, "Version")) ch.version = fp.fread_number();
                        else if (!RomString.str_cmp(word, "Vnum"))
                            ch.pIndexData = Handler.get_mob_index(fp.fread_number());
                        else fMatch = false;
                        break;
                    case 'W':
                        if (!RomString.str_cmp(word, "Wizn")) ch.wiznet = fp.fread_flag();
                        else if (!RomString.str_cmp(word, "Wimp") || !RomString.str_cmp(word, "Wimpy")) ch.wimpy = fp.fread_number();
                        else fMatch = false;
                        break;
                    case 'B':
                        if (!RomString.str_cmp(word, "Boards"))
                        {
                            int num = fp.fread_number();
                            for (; num > 0; num--)
                            {
                                string boardname = fp.fread_word();
                                int i = Board.board_lookup(boardname);
                                if (i == Board.BOARD_NOTFOUND)
                                {
                                    Db.bug(RomString.sprintf(
                                        "fread_char: %s had unknown board name: %s. Skipped.",
                                        ch.name, boardname), 0);
                                    fp.fread_number();
                                }
                                else
                                    ch.pcdata.last_note[i] = fp.fread_number();
                            }
                        }
                        else if (!RomString.str_cmp(word, "Bamfin") || !RomString.str_cmp(word, "Bin")) ch.pcdata.bamfin = fp.fread_string();
                        else if (!RomString.str_cmp(word, "Bamfout") || !RomString.str_cmp(word, "Bout")) ch.pcdata.bamfout = fp.fread_string();
                        else fMatch = false;
                        break;
                    default:
                        fMatch = false;
                        break;
                }
                if (!fMatch)
                {
                    Db.bug("Fread_char: no match.", 0);
                    Db.bug(word, 0);
                    fp.fread_to_eol();
                }
            }
        }

        static void fread_obj(CharData ch, AreaReader fp)
        {
            ObjData obj = null;
            bool fMatch;
            bool fNest;
            bool fVnum;
            bool first = true;
            bool new_format = false;
            bool make_new = false;
            int iNest = 0;

            fVnum = false;
            string word = fp.Eof() ? "End" : fp.fread_word();
            if (!RomString.str_cmp(word, "Vnum"))
            {
                first = false;

                int vnum = fp.fread_number();
                if (Handler.get_obj_index(vnum) == null)
                {
                    Db.bug("Fread_obj: bad vnum %d.", vnum);
                }
                else
                {
                    obj = Db.create_object(Handler.get_obj_index(vnum), -1);
                    new_format = true;
                }
            }

            if (obj == null)
            {
                obj = Recycle.new_obj();
                obj.name = "";
                obj.short_descr = "";
                obj.description = "";
            }

            fNest = false;
            fVnum = true;

            for (;;)
            {
                if (first)
                    first = false;
                else
                    word = fp.Eof() ? "End" : fp.fread_word();
                fMatch = false;

                switch (word.Length == 0 ? '\0' : Bit.UPPER(word[0]))
                {
                    case '*':
                        fMatch = true;
                        fp.fread_to_eol();
                        break;

                    case 'A':
                        if (!RomString.str_cmp(word, "AffD"))
                        {
                            var paf = Recycle.new_affect();

                            int sn = Lookup.skill_lookup(fp.fread_word());
                            if (sn < 0)
                                Db.bug("Fread_obj: unknown skill.", 0);
                            else
                                paf.type = sn;

                            paf.level = fp.fread_number();
                            paf.duration = fp.fread_number();
                            paf.modifier = fp.fread_number();
                            paf.location = fp.fread_number();
                            paf.bitvector = fp.fread_number();
                            paf.next = obj.affected;
                            obj.affected = paf;
                            fMatch = true;
                            break;
                        }
                        if (!RomString.str_cmp(word, "Affc"))
                        {
                            var paf = Recycle.new_affect();

                            int sn = Lookup.skill_lookup(fp.fread_word());
                            if (sn < 0)
                                Db.bug("Fread_obj: unknown skill.", 0);
                            else
                                paf.type = sn;

                            paf.where = fp.fread_number();
                            paf.level = fp.fread_number();
                            paf.duration = fp.fread_number();
                            paf.modifier = fp.fread_number();
                            paf.location = fp.fread_number();
                            paf.bitvector = fp.fread_number();
                            paf.next = obj.affected;
                            obj.affected = paf;
                            fMatch = true;
                            break;
                        }
                        break;

                    case 'C':
                        if (!RomString.str_cmp(word, "Cond"))
                        { obj.condition = fp.fread_number(); fMatch = true; break; }
                        if (!RomString.str_cmp(word, "Cost"))
                        { obj.cost = fp.fread_number(); fMatch = true; break; }
                        break;

                    case 'D':
                        if (!RomString.str_cmp(word, "Description")
                            || !RomString.str_cmp(word, "Desc"))
                        { obj.description = fp.fread_string(); fMatch = true; break; }
                        break;

                    case 'E':
                        if (!RomString.str_cmp(word, "Enchanted"))
                        {
                            obj.enchanted = true;
                            fMatch = true;
                            break;
                        }

                        if (!RomString.str_cmp(word, "ExtraFlags")
                            || !RomString.str_cmp(word, "ExtF"))
                        { obj.extra_flags = fp.fread_number(); fMatch = true; break; }

                        if (!RomString.str_cmp(word, "ExtraDescr") || !RomString.str_cmp(word, "ExDe"))
                        {
                            var ed = Recycle.new_extra_descr();
                            ed.keyword = fp.fread_string();
                            ed.description = fp.fread_string();
                            ed.next = obj.extra_descr;
                            obj.extra_descr = ed;
                            fMatch = true;
                        }

                        if (!RomString.str_cmp(word, "End"))
                        {
                            if (!fNest || (fVnum && obj.pIndexData == null))
                            {
                                Db.bug("Fread_obj: incomplete object.", 0);
                                Recycle.free_obj(obj);
                                return;
                            }
                            else
                            {
                                if (!fVnum)
                                {
                                    Recycle.free_obj(obj);
                                    obj = Db.create_object(Handler.get_obj_index(OBJ_VNUM_DUMMY),
                                        0);
                                }

                                if (!new_format)
                                {
                                    obj.next = Game.object_list;
                                    Game.object_list = obj;
                                    obj.pIndexData.count++;
                                }

                                if (!obj.pIndexData.new_format
                                    && obj.item_type == ITEM_ARMOR
                                    && obj.value[1] == 0)
                                {
                                    obj.value[1] = obj.value[0];
                                    obj.value[2] = obj.value[0];
                                }
                                if (make_new)
                                {
                                    int wear = obj.wear_loc;
                                    Handler.extract_obj(obj);

                                    obj = Db.create_object(obj.pIndexData, 0);
                                    obj.wear_loc = wear;
                                }
                                if (iNest == 0 || rgObjNest[iNest] == null)
                                    Handler.obj_to_char(obj, ch);
                                else
                                    Handler.obj_to_obj(obj, rgObjNest[iNest - 1]);
                                return;
                            }
                        }
                        break;

                    case 'I':
                        if (!RomString.str_cmp(word, "ItemType")
                            || !RomString.str_cmp(word, "Ityp"))
                        { obj.item_type = fp.fread_number(); fMatch = true; break; }
                        break;

                    case 'L':
                        if (!RomString.str_cmp(word, "Level")
                            || !RomString.str_cmp(word, "Lev"))
                        { obj.level = fp.fread_number(); fMatch = true; break; }
                        break;

                    case 'N':
                        if (!RomString.str_cmp(word, "Name"))
                        { obj.name = fp.fread_string(); fMatch = true; break; }

                        if (!RomString.str_cmp(word, "Nest"))
                        {
                            iNest = fp.fread_number();
                            if (iNest < 0 || iNest >= MAX_NEST)
                            {
                                Db.bug("Fread_obj: bad nest %d.", iNest);
                            }
                            else
                            {
                                rgObjNest[iNest] = obj;
                                fNest = true;
                            }
                            fMatch = true;
                        }
                        break;

                    case 'O':
                        if (!RomString.str_cmp(word, "Oldstyle"))
                        {
                            if (obj.pIndexData != null
                                && obj.pIndexData.new_format)
                                make_new = true;
                            fMatch = true;
                        }
                        break;

                    case 'S':
                        if (!RomString.str_cmp(word, "ShortDescr")
                            || !RomString.str_cmp(word, "ShD"))
                        { obj.short_descr = fp.fread_string(); fMatch = true; break; }

                        if (!RomString.str_cmp(word, "Spell"))
                        {
                            int iValue = fp.fread_number();
                            int sn = Lookup.skill_lookup(fp.fread_word());
                            if (iValue < 0 || iValue > 3)
                            {
                                Db.bug("Fread_obj: bad iValue %d.", iValue);
                            }
                            else if (sn < 0)
                            {
                                Db.bug("Fread_obj: unknown skill.", 0);
                            }
                            else
                            {
                                obj.value[iValue] = sn;
                            }
                            fMatch = true;
                            break;
                        }
                        break;

                    case 'T':
                        if (!RomString.str_cmp(word, "Timer")
                            || !RomString.str_cmp(word, "Time"))
                        { obj.timer = fp.fread_number(); fMatch = true; break; }
                        break;

                    case 'V':
                        if (!RomString.str_cmp(word, "Values") || !RomString.str_cmp(word, "Vals"))
                        {
                            obj.value[0] = fp.fread_number();
                            obj.value[1] = fp.fread_number();
                            obj.value[2] = fp.fread_number();
                            obj.value[3] = fp.fread_number();
                            if (obj.item_type == ITEM_WEAPON && obj.value[0] == 0)
                                obj.value[0] = obj.pIndexData.value[0];
                            fMatch = true;
                            break;
                        }

                        if (!RomString.str_cmp(word, "Val"))
                        {
                            obj.value[0] = fp.fread_number();
                            obj.value[1] = fp.fread_number();
                            obj.value[2] = fp.fread_number();
                            obj.value[3] = fp.fread_number();
                            obj.value[4] = fp.fread_number();
                            fMatch = true;
                            break;
                        }

                        if (!RomString.str_cmp(word, "Vnum"))
                        {
                            int vnum = fp.fread_number();
                            if ((obj.pIndexData = Handler.get_obj_index(vnum)) == null)
                                Db.bug("Fread_obj: bad vnum %d.", vnum);
                            else
                                fVnum = true;
                            fMatch = true;
                            break;
                        }
                        break;

                    case 'W':
                        if (!RomString.str_cmp(word, "WearFlags")
                            || !RomString.str_cmp(word, "WeaF"))
                        { obj.wear_flags = fp.fread_number(); fMatch = true; break; }
                        if (!RomString.str_cmp(word, "WearLoc")
                            || !RomString.str_cmp(word, "Wear"))
                        { obj.wear_loc = fp.fread_number(); fMatch = true; break; }
                        if (!RomString.str_cmp(word, "Weight")
                            || !RomString.str_cmp(word, "Wt"))
                        { obj.weight = fp.fread_number(); fMatch = true; break; }
                        break;
                }

                if (!fMatch)
                {
                    Db.bug("Fread_obj: no match.", 0);
                    fp.fread_to_eol();
                }
            }
        }

        public static void save_char_obj(CharData ch)
        {
            if (Bit.IS_NPC(ch))
                return;

            if (!Bit.IS_VALID(ch))
            {
                Db.bug("save_char_obj: Trying to save an invalidated character.\n", 0);
                return;
            }

            if (ch.desc != null && ch.desc.original != null)
                ch = ch.desc.original;

            if (Bit.IS_IMMORTAL(ch) || ch.level >= LEVEL_IMMORTAL)
            {
                var godDir = Path.GetFullPath(Path.Combine(Game.area_dir, GOD_DIR));
                var godsave = Path.Combine(godDir, RomString.capitalize(ch.name));
                try
                {
                    Directory.CreateDirectory(godDir);
                    File.WriteAllText(godsave, RomString.sprintf("Lev %2d Trust %2d  %s%s\n",
                        ch.level, Handler.get_trust(ch), ch.name, ch.pcdata.title ?? ""));
                }
                catch
                {
                    Db.bug("Save_char_obj: fopen", 0);
                }
            }

            Directory.CreateDirectory(Game.player_dir);
            var strsave = Path.Combine(Game.player_dir, RomString.capitalize(ch.name));
            // Unique temp per save avoids shared-romtmp races and never Move on failed write.
            var tmp = Path.Combine(Game.player_dir,
                $"romtmp.{Environment.ProcessId}.{RomString.capitalize(ch.name)}");
            try
            {
                using (var sw = new StreamWriter(tmp))
                {
                    fwrite_char(ch, sw);
                    if (ch.carrying != null)
                        fwrite_obj(ch, ch.carrying, sw, 0);
                    if (ch.pet != null && ch.pet.in_room == ch.in_room)
                        fwrite_pet(ch.pet, sw);
                    sw.WriteLine("#END");
                }
                // Only replace the live pfile after the write completed successfully.
                File.Move(tmp, strsave, true);
            }
            catch
            {
                Db.bug("Save_char_obj: fopen", 0);
                try
                {
                    if (File.Exists(tmp))
                        File.Delete(tmp);
                }
                catch
                {
                    /* best-effort cleanup of partial temp */
                }
            }
        }

        static void fwrite_char(CharData ch, StreamWriter fp)
        {
            fp.WriteLine("#{0}", Bit.IS_NPC(ch) ? "MOB" : "PLAYER");

            fp.WriteLine("Name {0}~", ch.name);
            fp.WriteLine("Id   {0}", ch.id);
            fp.WriteLine("LogO {0}", Game.current_time);
            fp.WriteLine("Vers {0}", 5);
            if (!string.IsNullOrEmpty(ch.short_descr))
                fp.WriteLine("ShD  {0}~", ch.short_descr);
            if (!string.IsNullOrEmpty(ch.long_descr))
                fp.WriteLine("LnD  {0}~", ch.long_descr);
            if (!string.IsNullOrEmpty(ch.description))
                fp.WriteLine("Desc {0}~", ch.description);
            if (ch.prompt != null || !RomString.str_cmp(ch.prompt, "<%hhp %mm %vmv> ")
                || !RomString.str_cmp(ch.prompt, "{c<%hhp %mm %vmv>{x "))
                fp.WriteLine("Prom {0}~", ch.prompt);
            fp.WriteLine("Race {0}~", Tables.pc_race_table[ch.race].name);
            if (ch.clan != 0)
                fp.WriteLine("Clan {0}~", Tables.clan_table[ch.clan].name);
            fp.WriteLine("Sex  {0}", ch.sex);
            fp.WriteLine("Cla  {0}", ch.klass);
            fp.WriteLine("Levl {0}", ch.level);
            if (ch.trust != 0)
                fp.WriteLine("Tru  {0}", ch.trust);
            fp.WriteLine("Sec  {0}", ch.pcdata.security);
            fp.WriteLine("Plyd {0}", ch.played + (int)(Game.current_time - ch.logon));
            fp.WriteLine("Scro {0}", ch.lines);
            fp.WriteLine("Room {0}", (ch.in_room == Handler.get_room_index(ROOM_VNUM_LIMBO)
                && ch.was_in_room != null)
                ? ch.was_in_room.vnum
                : ch.in_room == null ? 3001 : ch.in_room.vnum);

            fp.WriteLine("HMV  {0} {1} {2} {3} {4} {5}",
                ch.hit, ch.max_hit, ch.mana, ch.max_mana, ch.move,
                ch.max_move);
            if (ch.gold > 0)
                fp.WriteLine("Gold {0}", ch.gold);
            else
                fp.WriteLine("Gold {0}", 0);
            if (ch.silver > 0)
                fp.WriteLine("Silv {0}", ch.silver);
            else
                fp.WriteLine("Silv {0}", 0);
            fp.WriteLine("Exp  {0}", ch.exp);
            if (ch.act != 0)
                fp.WriteLine("Act  {0}", Db.print_flags(ch.act));
            if (ch.affected_by != 0)
                fp.WriteLine("AfBy {0}", Db.print_flags(ch.affected_by));
            fp.WriteLine("Comm {0}", Db.print_flags(ch.comm));
            if (ch.wiznet != 0)
                fp.WriteLine("Wizn {0}", Db.print_flags(ch.wiznet));
            if (ch.invis_level != 0)
                fp.WriteLine("Invi {0}", ch.invis_level);
            if (ch.incog_level != 0)
                fp.WriteLine("Inco {0}", ch.incog_level);
            fp.WriteLine("Pos  {0}",
                ch.position == POS_FIGHTING ? POS_STANDING : ch.position);
            if (ch.practice != 0)
                fp.WriteLine("Prac {0}", ch.practice);
            if (ch.train != 0)
                fp.WriteLine("Trai {0}", ch.train);
            if (ch.saving_throw != 0)
                fp.WriteLine("Save  {0}", ch.saving_throw);
            fp.WriteLine("Alig  {0}", ch.alignment);
            if (ch.hitroll != 0)
                fp.WriteLine("Hit   {0}", ch.hitroll);
            if (ch.damroll != 0)
                fp.WriteLine("Dam   {0}", ch.damroll);
            fp.WriteLine("ACs {0} {1} {2} {3}",
                ch.armor[0], ch.armor[1], ch.armor[2], ch.armor[3]);
            if (ch.wimpy != 0)
                fp.WriteLine("Wimp  {0}", ch.wimpy);
            fp.WriteLine("Attr {0} {1} {2} {3} {4}",
                ch.perm_stat[STAT_STR],
                ch.perm_stat[STAT_INT],
                ch.perm_stat[STAT_WIS],
                ch.perm_stat[STAT_DEX], ch.perm_stat[STAT_CON]);

            fp.WriteLine("AMod {0} {1} {2} {3} {4}",
                ch.mod_stat[STAT_STR],
                ch.mod_stat[STAT_INT],
                ch.mod_stat[STAT_WIS],
                ch.mod_stat[STAT_DEX], ch.mod_stat[STAT_CON]);

            if (Bit.IS_NPC(ch))
            {
                fp.WriteLine("Vnum {0}", ch.pIndexData.vnum);
            }
            else
            {
                fp.WriteLine("Pass {0}~", ch.pcdata.pwd);
                if (!string.IsNullOrEmpty(ch.pcdata.bamfin))
                    fp.WriteLine("Bin  {0}~", ch.pcdata.bamfin);
                if (!string.IsNullOrEmpty(ch.pcdata.bamfout))
                    fp.WriteLine("Bout {0}~", ch.pcdata.bamfout);
                fp.WriteLine("Titl {0}~", ch.pcdata.title);
                fp.WriteLine("Pnts {0}", ch.pcdata.points);
                fp.WriteLine("TSex {0}", ch.pcdata.true_sex);
                fp.WriteLine("LLev {0}", ch.pcdata.last_level);
                fp.WriteLine("HMVP {0} {1} {2}", ch.pcdata.perm_hit,
                    ch.pcdata.perm_mana, ch.pcdata.perm_move);
                fp.WriteLine("Cnd  {0} {1} {2} {3}",
                    ch.pcdata.condition[0],
                    ch.pcdata.condition[1],
                    ch.pcdata.condition[2], ch.pcdata.condition[3]);

                fp.Write(RomString.sprintf("Coloura     %d%d%d %d%d%d %d%d%d %d%d%d %d%d%d\n",
                    ch.pcdata.text[2], ch.pcdata.text[0], ch.pcdata.text[1],
                    ch.pcdata.auction[2], ch.pcdata.auction[0], ch.pcdata.auction[1],
                    ch.pcdata.gossip[2], ch.pcdata.gossip[0], ch.pcdata.gossip[1],
                    ch.pcdata.music[2], ch.pcdata.music[0], ch.pcdata.music[1],
                    ch.pcdata.question[2], ch.pcdata.question[0], ch.pcdata.question[1]));
                fp.Write(RomString.sprintf("Colourb     %d%d%d %d%d%d %d%d%d %d%d%d %d%d%d\n",
                    ch.pcdata.answer[2], ch.pcdata.answer[0], ch.pcdata.answer[1],
                    ch.pcdata.quote[2], ch.pcdata.quote[0], ch.pcdata.quote[1],
                    ch.pcdata.quote_text[2], ch.pcdata.quote_text[0], ch.pcdata.quote_text[1],
                    ch.pcdata.immtalk_text[2], ch.pcdata.immtalk_text[0], ch.pcdata.immtalk_text[1],
                    ch.pcdata.immtalk_type[2], ch.pcdata.immtalk_type[0], ch.pcdata.immtalk_type[1]));
                fp.Write(RomString.sprintf("Colourc     %d%d%d %d%d%d %d%d%d %d%d%d %d%d%d\n",
                    ch.pcdata.info[2], ch.pcdata.info[0], ch.pcdata.info[1],
                    ch.pcdata.tell[2], ch.pcdata.tell[0], ch.pcdata.tell[1],
                    ch.pcdata.reply[2], ch.pcdata.reply[0], ch.pcdata.reply[1],
                    ch.pcdata.gtell_text[2], ch.pcdata.gtell_text[0], ch.pcdata.gtell_text[1],
                    ch.pcdata.gtell_type[2], ch.pcdata.gtell_type[0], ch.pcdata.gtell_type[1]));
                fp.Write(RomString.sprintf("Colourd     %d%d%d %d%d%d %d%d%d %d%d%d %d%d%d\n",
                    ch.pcdata.room_title[2], ch.pcdata.room_title[0], ch.pcdata.room_title[1],
                    ch.pcdata.room_text[2], ch.pcdata.room_text[0], ch.pcdata.room_text[1],
                    ch.pcdata.room_exits[2], ch.pcdata.room_exits[0], ch.pcdata.room_exits[1],
                    ch.pcdata.room_things[2], ch.pcdata.room_things[0], ch.pcdata.room_things[1],
                    ch.pcdata.prompt[2], ch.pcdata.prompt[0], ch.pcdata.prompt[1]));
                fp.Write(RomString.sprintf("Coloure     %d%d%d %d%d%d %d%d%d %d%d%d %d%d%d\n",
                    ch.pcdata.fight_death[2], ch.pcdata.fight_death[0], ch.pcdata.fight_death[1],
                    ch.pcdata.fight_yhit[2], ch.pcdata.fight_yhit[0], ch.pcdata.fight_yhit[1],
                    ch.pcdata.fight_ohit[2], ch.pcdata.fight_ohit[0], ch.pcdata.fight_ohit[1],
                    ch.pcdata.fight_thit[2], ch.pcdata.fight_thit[0], ch.pcdata.fight_thit[1],
                    ch.pcdata.fight_skill[2], ch.pcdata.fight_skill[0], ch.pcdata.fight_skill[1]));
                fp.Write(RomString.sprintf("Colourf     %d%d%d %d%d%d %d%d%d %d%d%d %d%d%d\n",
                    ch.pcdata.wiznet[2], ch.pcdata.wiznet[0], ch.pcdata.wiznet[1],
                    ch.pcdata.say[2], ch.pcdata.say[0], ch.pcdata.say[1],
                    ch.pcdata.say_text[2], ch.pcdata.say_text[0], ch.pcdata.say_text[1],
                    ch.pcdata.tell_text[2], ch.pcdata.tell_text[0], ch.pcdata.tell_text[1],
                    ch.pcdata.reply_text[2], ch.pcdata.reply_text[0], ch.pcdata.reply_text[1]));
                fp.Write(RomString.sprintf("Colourg     %d%d%d %d%d%d %d%d%d %d%d%d %d%d%d\n",
                    ch.pcdata.auction_text[2], ch.pcdata.auction_text[0], ch.pcdata.auction_text[1],
                    ch.pcdata.gossip_text[2], ch.pcdata.gossip_text[0], ch.pcdata.gossip_text[1],
                    ch.pcdata.music_text[2], ch.pcdata.music_text[0], ch.pcdata.music_text[1],
                    ch.pcdata.question_text[2], ch.pcdata.question_text[0], ch.pcdata.question_text[1],
                    ch.pcdata.answer_text[2], ch.pcdata.answer_text[0], ch.pcdata.answer_text[1]));

                for (int pos = 0; pos < MAX_ALIAS; pos++)
                {
                    if (ch.pcdata.alias[pos] == null
                        || ch.pcdata.alias_sub[pos] == null)
                        break;
                    fp.WriteLine("Alias {0} {1}~", ch.pcdata.alias[pos],
                        ch.pcdata.alias_sub[pos]);
                }

                fp.Write("Boards       {0} ", MAX_BOARD);
                for (int i = 0; i < MAX_BOARD; i++)
                    fp.Write("{0} {1} ", Board.boards[i].short_name, ch.pcdata.last_note[i]);
                fp.WriteLine();

                for (int sn = 0; sn < MAX_SKILL && sn < Tables.skill_table.Length; sn++)
                {
                    if (Tables.skill_table[sn].name != null && ch.pcdata.learned[sn] > 0)
                    {
                        fp.WriteLine("Sk {0} '{1}'",
                            ch.pcdata.learned[sn], Tables.skill_table[sn].name);
                    }
                }

                for (int gn = 0; gn < MAX_GROUP && gn < Tables.group_table.Length; gn++)
                {
                    if (Tables.group_table[gn].name != null && ch.pcdata.group_known[gn])
                    {
                        fp.WriteLine("Gr '{0}'", Tables.group_table[gn].name);
                    }
                }
            }

            for (var paf = ch.affected; paf != null; paf = paf.next)
            {
                if (paf.type < 0 || paf.type >= MAX_SKILL)
                    continue;

                fp.WriteLine("Affc '{0}' {1,3} {2,3} {3,3} {4,3} {5,3} {6,10}",
                    Tables.skill_table[paf.type].name,
                    paf.where,
                    paf.level,
                    paf.duration, paf.modifier, paf.location, paf.bitvector);
            }
            Imc.savechar(ch, fp);
            fp.WriteLine("End");
            fp.WriteLine();
        }

        static void LoadColour(int[] field, AreaReader fp)
        {
            field[1] = fp.fread_number();
            /* Packed as [beep*100 + bright*10 + colour]; use >= so 100 and 10 decode. */
            if (field[1] >= 100)
            {
                field[1] -= 100;
                field[2] = 1;
            }
            else
            {
                field[2] = 0;
            }
            if (field[1] >= 10)
            {
                field[1] -= 10;
                field[0] = 1;
            }
            else
            {
                field[0] = 0;
            }
        }

        static void fread_pet(CharData ch, AreaReader fp)
        {
            CharData pet;
            int lastlogoff = (int)Game.current_time;
            int vnum = 0;

            var word = fp.Eof() ? "END" : fp.fread_word();
            if (!RomString.str_cmp(word, "Vnum"))
            {
                vnum = fp.fread_number();
                if (Handler.get_mob_index(vnum) == null)
                {
                    Db.bug("Fread_pet: bad vnum %d.", vnum);
                    pet = Db.create_mobile(Handler.get_mob_index(MOB_VNUM_FIDO));
                }
                else
                    pet = Db.create_mobile(Handler.get_mob_index(vnum));
            }
            else
            {
                Db.bug("Fread_pet: no vnum in file.", 0);
                pet = Db.create_mobile(Handler.get_mob_index(MOB_VNUM_FIDO));
            }

            for (;;)
            {
                word = fp.Eof() ? "END" : fp.fread_word();
                bool fMatch = false;

                switch (word.Length == 0 ? '\0' : Bit.UPPER(word[0]))
                {
                    case '*':
                        fMatch = true;
                        fp.fread_to_eol();
                        break;

                    case 'A':
                        if (!RomString.str_cmp(word, "Act")) { pet.act = fp.fread_flag(); fMatch = true; }
                        else if (!RomString.str_cmp(word, "AfBy")) { pet.affected_by = fp.fread_flag(); fMatch = true; }
                        else if (!RomString.str_cmp(word, "Alig")) { pet.alignment = fp.fread_number(); fMatch = true; }
                        else if (!RomString.str_cmp(word, "ACs"))
                        {
                            for (int i = 0; i < 4; i++)
                                pet.armor[i] = fp.fread_number();
                            fMatch = true;
                        }
                        else if (!RomString.str_cmp(word, "AffD") || !RomString.str_cmp(word, "Affc"))
                        {
                            var paf = Recycle.new_affect();
                            int sn = Lookup.skill_lookup(fp.fread_word());
                            if (sn < 0)
                                Db.bug("Fread_char: unknown skill.", 0);
                            else
                                paf.type = sn;
                            if (!RomString.str_cmp(word, "Affc"))
                                paf.where = fp.fread_number();
                            paf.level = fp.fread_number();
                            paf.duration = fp.fread_number();
                            paf.modifier = fp.fread_number();
                            paf.location = fp.fread_number();
                            paf.bitvector = fp.fread_number();
                            if (!RomString.str_cmp(word, "Affc") && Db.check_pet_affected(vnum, paf))
                                Recycle.free_affect(paf);
                            else
                            {
                                paf.next = pet.affected;
                                pet.affected = paf;
                            }
                            fMatch = true;
                        }
                        else if (!RomString.str_cmp(word, "AMod"))
                        {
                            for (int stat = 0; stat < MAX_STATS; stat++)
                                pet.mod_stat[stat] = fp.fread_number();
                            fMatch = true;
                        }
                        else if (!RomString.str_cmp(word, "Attr"))
                        {
                            for (int stat = 0; stat < MAX_STATS; stat++)
                                pet.perm_stat[stat] = fp.fread_number();
                            fMatch = true;
                        }
                        break;

                    case 'C':
                        if (!RomString.str_cmp(word, "Clan")) { pet.clan = Lookup.clan_lookup(fp.fread_string()); fMatch = true; }
                        else if (!RomString.str_cmp(word, "Comm")) { pet.comm = fp.fread_flag(); fMatch = true; }
                        break;

                    case 'D':
                        if (!RomString.str_cmp(word, "Dam")) { pet.damroll = fp.fread_number(); fMatch = true; }
                        else if (!RomString.str_cmp(word, "Desc")) { pet.description = fp.fread_string(); fMatch = true; }
                        break;

                    case 'E':
                        if (!RomString.str_cmp(word, "End"))
                        {
                            pet.leader = ch;
                            pet.master = ch;
                            ch.pet = pet;
                            int percent =
                                ((int)Game.current_time - lastlogoff) * 25 / (2 * 60 * 60);

                            if (percent > 0 && !Bit.IS_AFFECTED(ch, AFF_POISON)
                                && !Bit.IS_AFFECTED(ch, AFF_PLAGUE))
                            {
                                percent = Bit.UMIN(percent, 100);
                                pet.hit += (pet.max_hit - pet.hit) * percent / 100;
                                pet.mana +=
                                    (pet.max_mana - pet.mana) * percent / 100;
                                pet.move +=
                                    (pet.max_move - pet.move) * percent / 100;
                            }
                            return;
                        }
                        if (!RomString.str_cmp(word, "Exp")) { pet.exp = fp.fread_number(); fMatch = true; }
                        break;

                    case 'G':
                        if (!RomString.str_cmp(word, "Gold")) { pet.gold = fp.fread_number(); fMatch = true; }
                        break;

                    case 'H':
                        if (!RomString.str_cmp(word, "Hit")) { pet.hitroll = fp.fread_number(); fMatch = true; }
                        else if (!RomString.str_cmp(word, "HMV"))
                        {
                            pet.hit = fp.fread_number();
                            pet.max_hit = fp.fread_number();
                            pet.mana = fp.fread_number();
                            pet.max_mana = fp.fread_number();
                            pet.move = fp.fread_number();
                            pet.max_move = fp.fread_number();
                            fMatch = true;
                        }
                        break;

                    case 'L':
                        if (!RomString.str_cmp(word, "Levl")) { pet.level = fp.fread_number(); fMatch = true; }
                        else if (!RomString.str_cmp(word, "LnD")) { pet.long_descr = fp.fread_string(); fMatch = true; }
                        else if (!RomString.str_cmp(word, "LogO")) { lastlogoff = fp.fread_number(); fMatch = true; }
                        break;

                    case 'N':
                        if (!RomString.str_cmp(word, "Name")) { pet.name = fp.fread_string(); fMatch = true; }
                        break;

                    case 'P':
                        if (!RomString.str_cmp(word, "Pos")) { pet.position = fp.fread_number(); fMatch = true; }
                        break;

                    case 'R':
                        if (!RomString.str_cmp(word, "Race")) { pet.race = Lookup.race_lookup(fp.fread_string()); fMatch = true; }
                        break;

                    case 'S':
                        if (!RomString.str_cmp(word, "Save")) { pet.saving_throw = fp.fread_number(); fMatch = true; }
                        else if (!RomString.str_cmp(word, "Sex")) { pet.sex = fp.fread_number(); fMatch = true; }
                        else if (!RomString.str_cmp(word, "ShD")) { pet.short_descr = fp.fread_string(); fMatch = true; }
                        else if (!RomString.str_cmp(word, "Silv")) { pet.silver = fp.fread_number(); fMatch = true; }
                        break;
                }

                if (!fMatch)
                {
                    Db.bug("Fread_pet: no match.", 0);
                    fp.fread_to_eol();
                }
            }
        }

        static void fwrite_pet(CharData pet, StreamWriter fp)
        {
            fp.WriteLine("#PET");
            fp.WriteLine("Vnum {0}", pet.pIndexData.vnum);
            fp.WriteLine("Name {0}~", pet.name);
            fp.WriteLine("LogO {0}", Game.current_time);
            if (pet.short_descr != pet.pIndexData.short_descr)
                fp.WriteLine("ShD  {0}~", pet.short_descr);
            if (pet.long_descr != pet.pIndexData.long_descr)
                fp.WriteLine("LnD  {0}~", pet.long_descr);
            if (pet.description != pet.pIndexData.description)
                fp.WriteLine("Desc {0}~", pet.description);
            if (pet.race != pet.pIndexData.race)
                fp.WriteLine("Race {0}~", Tables.race_table[pet.race].name);
            if (pet.clan != 0)
                fp.WriteLine("Clan {0}~", Tables.clan_table[pet.clan].name);
            fp.WriteLine("Sex  {0}", pet.sex);
            if (pet.level != pet.pIndexData.level)
                fp.WriteLine("Levl {0}", pet.level);
            fp.WriteLine("HMV  {0} {1} {2} {3} {4} {5}",
                pet.hit, pet.max_hit, pet.mana, pet.max_mana, pet.move,
                pet.max_move);
            if (pet.gold > 0)
                fp.WriteLine("Gold {0}", pet.gold);
            if (pet.silver > 0)
                fp.WriteLine("Silv {0}", pet.silver);
            if (pet.exp > 0)
                fp.WriteLine("Exp  {0}", pet.exp);
            if (pet.act != pet.pIndexData.act)
                fp.WriteLine("Act  {0}", Db.print_flags(pet.act));
            if (pet.affected_by != pet.pIndexData.affected_by)
                fp.WriteLine("AfBy {0}", Db.print_flags(pet.affected_by));
            if (pet.comm != 0)
                fp.WriteLine("Comm {0}", Db.print_flags(pet.comm));
            fp.WriteLine("Pos  {0}",
                pet.position == POS_FIGHTING ? POS_STANDING : pet.position);
            if (pet.saving_throw != 0)
                fp.WriteLine("Save {0}", pet.saving_throw);
            if (pet.alignment != pet.pIndexData.alignment)
                fp.WriteLine("Alig {0}", pet.alignment);
            if (pet.hitroll != pet.pIndexData.hitroll)
                fp.WriteLine("Hit  {0}", pet.hitroll);
            if (pet.damroll != pet.pIndexData.damage[DICE_BONUS])
                fp.WriteLine("Dam  {0}", pet.damroll);
            fp.WriteLine("ACs  {0} {1} {2} {3}",
                pet.armor[0], pet.armor[1], pet.armor[2], pet.armor[3]);
            fp.WriteLine("Attr {0} {1} {2} {3} {4}",
                pet.perm_stat[STAT_STR], pet.perm_stat[STAT_INT],
                pet.perm_stat[STAT_WIS], pet.perm_stat[STAT_DEX],
                pet.perm_stat[STAT_CON]);
            fp.WriteLine("AMod {0} {1} {2} {3} {4}",
                pet.mod_stat[STAT_STR], pet.mod_stat[STAT_INT],
                pet.mod_stat[STAT_WIS], pet.mod_stat[STAT_DEX],
                pet.mod_stat[STAT_CON]);

            for (var paf = pet.affected; paf != null; paf = paf.next)
            {
                if (paf.type < 0 || paf.type >= MAX_SKILL)
                    continue;
                fp.WriteLine("Affc '{0}' {1,3} {2,3} {3,3} {4,3} {5,3} {6,10}",
                    Tables.skill_table[paf.type].name,
                    paf.where, paf.level, paf.duration, paf.modifier,
                    paf.location, paf.bitvector);
            }

            fp.WriteLine("End");
        }

        static void fwrite_obj(CharData ch, ObjData obj, StreamWriter fp, int iNest)
        {
            if (obj.next_content != null)
                fwrite_obj(ch, obj.next_content, fp, iNest);

            if ((ch.level < obj.level - 2 && obj.item_type != ITEM_CONTAINER)
                || obj.item_type == ITEM_KEY
                || (obj.item_type == ITEM_MAP && obj.value[0] == 0))
                return;

            fp.WriteLine("#O");
            fp.WriteLine("Vnum {0}", obj.pIndexData.vnum);
            if (!obj.pIndexData.new_format)
                fp.WriteLine("Oldstyle");
            if (obj.enchanted)
                fp.WriteLine("Enchanted");
            fp.WriteLine("Nest {0}", iNest);

            if (obj.name != obj.pIndexData.name)
                fp.WriteLine("Name {0}~", obj.name);
            if (obj.short_descr != obj.pIndexData.short_descr)
                fp.WriteLine("ShD  {0}~", obj.short_descr);
            if (obj.description != obj.pIndexData.description)
                fp.WriteLine("Desc {0}~", obj.description);
            if (obj.extra_flags != obj.pIndexData.extra_flags)
                fp.WriteLine("ExtF {0}", obj.extra_flags);
            if (obj.wear_flags != obj.pIndexData.wear_flags)
                fp.WriteLine("WeaF {0}", obj.wear_flags);
            if (obj.item_type != obj.pIndexData.item_type)
                fp.WriteLine("Ityp {0}", obj.item_type);
            if (obj.weight != obj.pIndexData.weight)
                fp.WriteLine("Wt   {0}", obj.weight);
            if (obj.condition != obj.pIndexData.condition)
                fp.WriteLine("Cond {0}", obj.condition);

            fp.WriteLine("Wear {0}", obj.wear_loc);
            if (obj.level != obj.pIndexData.level)
                fp.WriteLine("Lev  {0}", obj.level);
            if (obj.timer != 0)
                fp.WriteLine("Time {0}", obj.timer);
            fp.WriteLine("Cost {0}", obj.cost);
            if (obj.value[0] != obj.pIndexData.value[0]
                || obj.value[1] != obj.pIndexData.value[1]
                || obj.value[2] != obj.pIndexData.value[2]
                || obj.value[3] != obj.pIndexData.value[3]
                || obj.value[4] != obj.pIndexData.value[4])
                fp.WriteLine("Val  {0} {1} {2} {3} {4}",
                    obj.value[0], obj.value[1], obj.value[2], obj.value[3],
                    obj.value[4]);

            switch (obj.item_type)
            {
                case ITEM_POTION:
                case ITEM_SCROLL:
                case ITEM_PILL:
                    if (obj.value[1] > 0)
                        fp.WriteLine("Spell 1 '{0}'", Tables.skill_table[obj.value[1]].name);
                    if (obj.value[2] > 0)
                        fp.WriteLine("Spell 2 '{0}'", Tables.skill_table[obj.value[2]].name);
                    if (obj.value[3] > 0)
                        fp.WriteLine("Spell 3 '{0}'", Tables.skill_table[obj.value[3]].name);
                    break;
                case ITEM_STAFF:
                case ITEM_WAND:
                    if (obj.value[3] > 0)
                        fp.WriteLine("Spell 3 '{0}'", Tables.skill_table[obj.value[3]].name);
                    break;
            }

            for (var paf = obj.affected; paf != null; paf = paf.next)
            {
                if (paf.type < 0 || paf.type >= MAX_SKILL)
                    continue;
                fp.WriteLine("Affc '{0}' {1,3} {2,3} {3,3} {4,3} {5,3} {6,10}",
                    Tables.skill_table[paf.type].name,
                    paf.where,
                    paf.level,
                    paf.duration, paf.modifier, paf.location, paf.bitvector);
            }

            for (var ed = obj.extra_descr; ed != null; ed = ed.next)
            {
                fp.WriteLine("ExDe {0}~ {1}~", ed.keyword, ed.description);
            }

            fp.WriteLine("End");
            fp.WriteLine();

            if (obj.contains != null)
                fwrite_obj(ch, obj.contains, fp, iNest + 1);
        }

        public static void free_char(CharData ch)
        {
            if (ch == null || !Bit.IS_VALID(ch))
                return;

            if (Bit.IS_NPC(ch))
                Game.mobile_count--;

            ObjData obj_next;
            for (var obj = ch.carrying; obj != null; obj = obj_next)
            {
                obj_next = obj.next_content;
                Handler.extract_obj(obj);
            }

            AffectData paf_next;
            for (var paf = ch.affected; paf != null; paf = paf_next)
            {
                paf_next = paf.next;
                Handler.affect_remove(ch, paf);
            }

            ch.valid = false;
        }
    }
}
