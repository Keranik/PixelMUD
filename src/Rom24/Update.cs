using static Rom24.Merc;

namespace Rom24
{
    public static class Update
    {
        static int pulse_area, pulse_mobile, pulse_violence, pulse_point, pulse_music;
        static int save_number;

        public static void advance_level(CharData ch, bool hide)
        {
            ch.pcdata.last_level =
                (ch.played + (int)(Game.current_time - ch.logon)) / 3600;

            string buf = $"the {Tables.title_table[ch.klass][ch.level][ch.sex == SEX_FEMALE ? 1 : 0]}";
            ActInfo.set_title(ch, buf);

            int add_hp =
                Tables.con_app[Handler.get_curr_stat(ch, STAT_CON)].hitp +
                RomRandom.number_range(Tables.class_table[ch.klass].hp_min,
                    Tables.class_table[ch.klass].hp_max);
            int add_mana = RomRandom.number_range(2, (2 * Handler.get_curr_stat(ch, STAT_INT)
                + Handler.get_curr_stat(ch, STAT_WIS)) / 5);
            if (!Tables.class_table[ch.klass].fMana)
                add_mana /= 2;
            int add_move = RomRandom.number_range(1, (Handler.get_curr_stat(ch, STAT_CON)
                + Handler.get_curr_stat(ch, STAT_DEX)) / 6);
            int add_prac = Tables.wis_app[Handler.get_curr_stat(ch, STAT_WIS)].practice;

            add_hp = add_hp * 9 / 10;
            add_mana = add_mana * 9 / 10;
            add_move = add_move * 9 / 10;

            add_hp = Bit.UMAX(2, add_hp);
            add_mana = Bit.UMAX(2, add_mana);
            add_move = Bit.UMAX(6, add_move);

            ch.max_hit += add_hp;
            ch.max_mana += add_mana;
            ch.max_move += add_move;
            ch.practice += add_prac;
            ch.train += 1;
            Gmcp.Worth(ch);
            Gmcp.Name(ch);

            ch.pcdata.perm_hit += add_hp;
            ch.pcdata.perm_mana += add_mana;
            ch.pcdata.perm_move += add_move;

            if (!hide)
            {
                buf =
                    $"You gain {add_hp} hit point{(add_hp == 1 ? "" : "s")}, {add_mana} mana, {add_move} move, and {add_prac} practice{(add_prac == 1 ? "" : "s")}.\n\r";
                Comm.send_to_char(buf, ch);
            }
        }

        public static void gain_exp(CharData ch, int gain)
        {
            if (Bit.IS_NPC(ch) || ch.level >= LEVEL_HERO)
                return;

            ch.exp = Bit.UMAX(Handler.exp_per_level(ch, ch.pcdata.points), ch.exp + gain);
            while (ch.level < LEVEL_HERO && ch.exp >=
                   Handler.exp_per_level(ch, ch.pcdata.points) * (ch.level + 1))
            {
                Comm.send_to_char("{GYou raise a level!!  {x", ch);
                ch.level += 1;
                Db.log_string($"{ch.name} gained level {ch.level}");
                Comm.wiznet($"$N has attained level {ch.level}!", ch, null, WIZ_LEVELS, 0, 0);
                advance_level(ch, false);
                Save.save_char_obj(ch);
            }
            Gmcp.Worth(ch);
        }

        static int hit_gain(CharData ch)
        {
            int gain;
            int number;

            if (ch.in_room == null)
                return 0;

            if (Bit.IS_NPC(ch))
            {
                gain = 5 + ch.level;
                if (Bit.IS_AFFECTED(ch, AFF_REGENERATION))
                    gain *= 2;

                switch (ch.position)
                {
                    default:
                        gain /= 2;
                        break;
                    case POS_SLEEPING:
                        gain = 3 * gain / 2;
                        break;
                    case POS_RESTING:
                        break;
                    case POS_FIGHTING:
                        gain /= 3;
                        break;
                }
            }
            else
            {
                gain = Bit.UMAX(3, Handler.get_curr_stat(ch, STAT_CON) - 3 + ch.level / 2);
                gain += Tables.class_table[ch.klass].hp_max - 10;
                number = RomRandom.number_percent();
                if (number < Handler.get_skill(ch, Gsn.fast_healing))
                {
                    gain += number * gain / 100;
                    if (ch.hit < ch.max_hit)
                        Skills.check_improve(ch, Gsn.fast_healing, true, 8);
                }

                switch (ch.position)
                {
                    default:
                        gain /= 4;
                        break;
                    case POS_SLEEPING:
                        break;
                    case POS_RESTING:
                        gain /= 2;
                        break;
                    case POS_FIGHTING:
                        gain /= 6;
                        break;
                }

                if (ch.pcdata.condition[COND_HUNGER] == 0)
                    gain /= 2;

                if (ch.pcdata.condition[COND_THIRST] == 0)
                    gain /= 2;
            }

            gain = gain * ch.in_room.heal_rate / 100;

            if (ch.on != null && ch.on.item_type == ITEM_FURNITURE)
                gain = gain * ch.on.value[3] / 100;

            if (Bit.IS_AFFECTED(ch, AFF_POISON))
                gain /= 4;

            if (Bit.IS_AFFECTED(ch, AFF_PLAGUE))
                gain /= 8;

            if (Bit.IS_AFFECTED(ch, AFF_HASTE) || Bit.IS_AFFECTED(ch, AFF_SLOW))
                gain /= 2;

            return Bit.UMIN(gain, ch.max_hit - ch.hit);
        }

        static int mana_gain(CharData ch)
        {
            int gain;
            int number;

            if (ch.in_room == null)
                return 0;

            if (Bit.IS_NPC(ch))
            {
                gain = 5 + ch.level;
                switch (ch.position)
                {
                    default:
                        gain /= 2;
                        break;
                    case POS_SLEEPING:
                        gain = 3 * gain / 2;
                        break;
                    case POS_RESTING:
                        break;
                    case POS_FIGHTING:
                        gain /= 3;
                        break;
                }
            }
            else
            {
                gain = (Handler.get_curr_stat(ch, STAT_WIS)
                        + Handler.get_curr_stat(ch, STAT_INT) + ch.level) / 2;
                number = RomRandom.number_percent();
                if (number < Handler.get_skill(ch, Gsn.meditation))
                {
                    gain += number * gain / 100;
                    if (ch.mana < ch.max_mana)
                        Skills.check_improve(ch, Gsn.meditation, true, 8);
                }
                if (!Tables.class_table[ch.klass].fMana)
                    gain /= 2;

                switch (ch.position)
                {
                    default:
                        gain /= 4;
                        break;
                    case POS_SLEEPING:
                        break;
                    case POS_RESTING:
                        gain /= 2;
                        break;
                    case POS_FIGHTING:
                        gain /= 6;
                        break;
                }

                if (ch.pcdata.condition[COND_HUNGER] == 0)
                    gain /= 2;

                if (ch.pcdata.condition[COND_THIRST] == 0)
                    gain /= 2;
            }

            gain = gain * ch.in_room.mana_rate / 100;

            if (ch.on != null && ch.on.item_type == ITEM_FURNITURE)
                gain = gain * ch.on.value[4] / 100;

            if (Bit.IS_AFFECTED(ch, AFF_POISON))
                gain /= 4;

            if (Bit.IS_AFFECTED(ch, AFF_PLAGUE))
                gain /= 8;

            if (Bit.IS_AFFECTED(ch, AFF_HASTE) || Bit.IS_AFFECTED(ch, AFF_SLOW))
                gain /= 2;

            return Bit.UMIN(gain, ch.max_mana - ch.mana);
        }

        static int move_gain(CharData ch)
        {
            int gain;

            if (ch.in_room == null)
                return 0;

            if (Bit.IS_NPC(ch))
            {
                gain = ch.level;
            }
            else
            {
                gain = Bit.UMAX(15, ch.level);

                switch (ch.position)
                {
                    case POS_SLEEPING:
                        gain += Handler.get_curr_stat(ch, STAT_DEX);
                        break;
                    case POS_RESTING:
                        gain += Handler.get_curr_stat(ch, STAT_DEX) / 2;
                        break;
                }

                if (ch.pcdata.condition[COND_HUNGER] == 0)
                    gain /= 2;

                if (ch.pcdata.condition[COND_THIRST] == 0)
                    gain /= 2;
            }

            gain = gain * ch.in_room.heal_rate / 100;

            if (ch.on != null && ch.on.item_type == ITEM_FURNITURE)
                gain = gain * ch.on.value[3] / 100;

            if (Bit.IS_AFFECTED(ch, AFF_POISON))
                gain /= 4;

            if (Bit.IS_AFFECTED(ch, AFF_PLAGUE))
                gain /= 8;

            if (Bit.IS_AFFECTED(ch, AFF_HASTE) || Bit.IS_AFFECTED(ch, AFF_SLOW))
                gain /= 2;

            return Bit.UMIN(gain, ch.max_move - ch.move);
        }

        public static void gain_condition(CharData ch, int iCond, int value)
        {
            if (value == 0 || Bit.IS_NPC(ch) || ch.level >= LEVEL_IMMORTAL)
                return;

            int condition = ch.pcdata.condition[iCond];
            if (condition == -1)
                return;
            ch.pcdata.condition[iCond] = Bit.URANGE(0, condition + value, 48);

            if (ch.pcdata.condition[iCond] == 0)
            {
                switch (iCond)
                {
                    case COND_HUNGER:
                        Comm.send_to_char("You are hungry.\n\r", ch);
                        break;

                    case COND_THIRST:
                        Comm.send_to_char("You are thirsty.\n\r", ch);
                        break;

                    case COND_DRUNK:
                        if (condition != 0)
                            Comm.send_to_char("You are sober.\n\r", ch);
                        break;
                }
            }
        }

        static void mobile_update()
        {
            CharData ch_next;
            for (var ch = Game.char_list; ch != null; ch = ch_next)
            {
                ch_next = ch.next;

                if (!Bit.IS_NPC(ch) || ch.in_room == null
                    || Bit.IS_AFFECTED(ch, AFF_CHARM)) continue;

                if (ch.in_room.area.empty && !Bit.IS_SET(ch.act, ACT_UPDATE_ALWAYS))
                    continue;

                if (ch.spec_fun != null)
                {
                    if (ch.spec_fun(ch))
                        continue;
                }

                if (ch.pIndexData.pShop != null)
                    if ((ch.gold * 100 + ch.silver) < ch.pIndexData.wealth)
                    {
                        ch.gold +=
                            ch.pIndexData.wealth * RomRandom.number_range(1, 20) / 5000000;
                        ch.silver +=
                            ch.pIndexData.wealth * RomRandom.number_range(1, 20) / 50000;
                    }

                /*
                 * Check triggers only if mobile still in default position
                 */
                if (ch.position == ch.pIndexData.default_pos)
                {
                    /* Delay */
                    if (Bit.HAS_TRIGGER(ch, TRIG_DELAY) && ch.mprog_delay > 0)
                    {
                        if (--ch.mprog_delay <= 0)
                        {
                            MobProg.mp_percent_trigger(ch, null, null, null, (int)TRIG_DELAY);
                            continue;
                        }
                    }
                    if (Bit.HAS_TRIGGER(ch, TRIG_RANDOM))
                    {
                        if (MobProg.mp_percent_trigger(ch, null, null, null, (int)TRIG_RANDOM))
                            continue;
                    }
                }

                if (ch.position != POS_STANDING)
                    continue;

                if (Bit.IS_SET(ch.act, ACT_SCAVENGER)
                    && ch.in_room.contents != null && RomRandom.number_bits(6) == 0)
                {
                    int max = 1;
                    ObjData obj_best = null;
                    for (var obj = ch.in_room.contents; obj != null; obj = obj.next_content)
                    {
                        if (Bit.CAN_WEAR(obj, ITEM_TAKE) && ActObj.can_loot(ch, obj)
                            && obj.cost > max && obj.cost > 0)
                        {
                            obj_best = obj;
                            max = obj.cost;
                        }
                    }

                    if (obj_best != null)
                    {
                        Handler.obj_from_room(obj_best);
                        Handler.obj_to_char(obj_best, ch);
                        Comm.act("$n gets $p.", ch, obj_best, null, TO_ROOM);
                    }
                }

                int door;
                ExitData pexit;
                if (!Bit.IS_SET(ch.act, ACT_SENTINEL)
                    && RomRandom.number_bits(3) == 0
                    && (door = RomRandom.number_bits(5)) <= 5
                    && (pexit = ch.in_room.exit[door]) != null
                    && pexit.to_room != null
                    && !Bit.IS_SET(pexit.exit_info, EX_CLOSED)
                    && !Bit.IS_SET(pexit.to_room.room_flags, ROOM_NO_MOB)
                    && (!Bit.IS_SET(ch.act, ACT_STAY_AREA)
                        || pexit.to_room.area == ch.in_room.area)
                    && (!Bit.IS_SET(ch.act, ACT_OUTDOORS)
                        || !Bit.IS_SET(pexit.to_room.room_flags, ROOM_INDOORS))
                    && (!Bit.IS_SET(ch.act, ACT_INDOORS)
                        || Bit.IS_SET(pexit.to_room.room_flags, ROOM_INDOORS)))
                {
                    ActMove.move_char(ch, door, false);
                }
            }
        }

        static void weather_update()
        {
            string buf = "";

            switch (++Game.time_info.hour)
            {
                case 5:
                    Game.weather_info.sunlight = SUN_LIGHT;
                    buf += "The day has begun.\n\r";
                    break;

                case 6:
                    Game.weather_info.sunlight = SUN_RISE;
                    buf += "The sun rises in the east.\n\r";
                    break;

                case 19:
                    Game.weather_info.sunlight = SUN_SET;
                    buf += "The sun slowly disappears in the west.\n\r";
                    break;

                case 20:
                    Game.weather_info.sunlight = SUN_DARK;
                    buf += "The night has begun.\n\r";
                    break;

                case 24:
                    Game.time_info.hour = 0;
                    Game.time_info.day++;
                    break;
            }

            if (Game.time_info.day >= 35)
            {
                Game.time_info.day = 0;
                Game.time_info.month++;
            }

            if (Game.time_info.month >= 17)
            {
                Game.time_info.month = 0;
                Game.time_info.year++;
            }

            int diff;
            if (Game.time_info.month >= 9 && Game.time_info.month <= 16)
                diff = Game.weather_info.mmhg > 985 ? -2 : 2;
            else
                diff = Game.weather_info.mmhg > 1015 ? -2 : 2;

            Game.weather_info.change += diff * RomRandom.dice(1, 4) + RomRandom.dice(2, 6) - RomRandom.dice(2, 6);
            Game.weather_info.change = Bit.UMAX(Game.weather_info.change, -12);
            Game.weather_info.change = Bit.UMIN(Game.weather_info.change, 12);

            Game.weather_info.mmhg += Game.weather_info.change;
            Game.weather_info.mmhg = Bit.UMAX(Game.weather_info.mmhg, 960);
            Game.weather_info.mmhg = Bit.UMIN(Game.weather_info.mmhg, 1040);

            switch (Game.weather_info.sky)
            {
                default:
                    Db.bug("Weather_update: bad sky %d.", Game.weather_info.sky);
                    Game.weather_info.sky = SKY_CLOUDLESS;
                    break;

                case SKY_CLOUDLESS:
                    if (Game.weather_info.mmhg < 990
                        || (Game.weather_info.mmhg < 1010 && RomRandom.number_bits(2) == 0))
                    {
                        buf += "The sky is getting cloudy.\n\r";
                        Game.weather_info.sky = SKY_CLOUDY;
                    }
                    break;

                case SKY_CLOUDY:
                    if (Game.weather_info.mmhg < 970
                        || (Game.weather_info.mmhg < 990 && RomRandom.number_bits(2) == 0))
                    {
                        buf += "It starts to rain.\n\r";
                        Game.weather_info.sky = SKY_RAINING;
                    }

                    if (Game.weather_info.mmhg > 1030 && RomRandom.number_bits(2) == 0)
                    {
                        buf += "The clouds disappear.\n\r";
                        Game.weather_info.sky = SKY_CLOUDLESS;
                    }
                    break;

                case SKY_RAINING:
                    if (Game.weather_info.mmhg < 970 && RomRandom.number_bits(2) == 0)
                    {
                        buf += "Lightning flashes in the sky.\n\r";
                        Game.weather_info.sky = SKY_LIGHTNING;
                    }

                    if (Game.weather_info.mmhg > 1030
                        || (Game.weather_info.mmhg > 1010 && RomRandom.number_bits(2) == 0))
                    {
                        buf += "The rain stopped.\n\r";
                        Game.weather_info.sky = SKY_CLOUDY;
                    }
                    break;

                case SKY_LIGHTNING:
                    if (Game.weather_info.mmhg > 1010
                        || (Game.weather_info.mmhg > 990 && RomRandom.number_bits(2) == 0))
                    {
                        buf += "The lightning has stopped.\n\r";
                        Game.weather_info.sky = SKY_RAINING;
                        break;
                    }
                    break;
            }

            if (buf.Length != 0)
            {
                for (var d = Game.descriptor_list; d != null; d = d.next)
                {
                    if (d.connected == CON_PLAYING && Bit.IS_OUTSIDE(d.character)
                        && Bit.IS_AWAKE(d.character))
                        Comm.send_to_char(buf, d.character);
                }
            }
        }

        static void char_update()
        {
            CharData ch_quit = null;

            save_number++;

            if (save_number > 29)
                save_number = 0;

            CharData ch_next;
            for (var ch = Game.char_list; ch != null; ch = ch_next)
            {
                ch_next = ch.next;

                if (ch.timer > 30)
                    ch_quit = ch;

                if (ch.position >= POS_STUNNED)
                {
                    if (Bit.IS_NPC(ch) && ch.zone != null
                        && ch.zone != ch.in_room.area && ch.desc == null
                        && ch.fighting == null && !Bit.IS_AFFECTED(ch, AFF_CHARM)
                        && RomRandom.number_percent() < 5)
                    {
                        Comm.act("$n wanders on home.", ch, null, null, TO_ROOM);
                        Handler.extract_char(ch, true);
                        continue;
                    }

                    if (ch.hit < ch.max_hit)
                        ch.hit += hit_gain(ch);
                    else
                        ch.hit = ch.max_hit;

                    if (ch.mana < ch.max_mana)
                        ch.mana += mana_gain(ch);
                    else
                        ch.mana = ch.max_mana;

                    if (ch.move < ch.max_move)
                        ch.move += move_gain(ch);
                    else
                        ch.move = ch.max_move;
                }

                if (ch.position == POS_STUNNED)
                    Fight.update_pos(ch);

                if (!Bit.IS_NPC(ch) && ch.level < LEVEL_IMMORTAL)
                {
                    ObjData obj;

                    if ((obj = Handler.get_eq_char(ch, WEAR_LIGHT)) != null
                        && obj.item_type == ITEM_LIGHT && obj.value[2] > 0)
                    {
                        if (--obj.value[2] == 0 && ch.in_room != null)
                        {
                            --ch.in_room.light;
                            Comm.act("$p goes out.", ch, obj, null, TO_ROOM);
                            Comm.act("$p flickers and goes out.", ch, obj, null, TO_CHAR);
                            Handler.extract_obj(obj);
                        }
                        else
                        {
                            Gmcp.ItemUpdate(obj);
                            if (obj.value[2] <= 5 && ch.in_room != null)
                                Comm.act("$p flickers.", ch, obj, null, TO_CHAR);
                        }
                    }

                    if (Bit.IS_IMMORTAL(ch))
                        ch.timer = 0;

                    if (++ch.timer >= 12)
                    {
                        if (ch.was_in_room == null && ch.in_room != null)
                        {
                            ch.was_in_room = ch.in_room;
                            if (ch.fighting != null)
                                Fight.stop_fighting(ch, true);
                            Comm.act("$n disappears into the void.",
                                ch, null, null, TO_ROOM);
                            Comm.send_to_char("You disappear into the void.\n\r", ch);
                            if (ch.level > 1)
                                Save.save_char_obj(ch);
                            Handler.char_from_room(ch);
                            Handler.char_to_room(ch, Handler.get_room_index(ROOM_VNUM_LIMBO));
                        }
                    }

                    gain_condition(ch, COND_DRUNK, -1);
                    gain_condition(ch, COND_FULL, ch.size > SIZE_MEDIUM ? -4 : -2);
                    gain_condition(ch, COND_THIRST, -1);
                    gain_condition(ch, COND_HUNGER,
                        ch.size > SIZE_MEDIUM ? -2 : -1);
                }

                AffectData paf_next;
                for (var paf = ch.affected; paf != null; paf = paf_next)
                {
                    paf_next = paf.next;
                    if (paf.duration > 0)
                    {
                        paf.duration--;
                        if (RomRandom.number_range(0, 4) == 0 && paf.level > 0)
                            paf.level--;
                    }
                    else if (paf.duration < 0) { }
                    else
                    {
                        if (paf_next == null
                            || paf_next.type != paf.type || paf_next.duration > 0)
                        {
                            if (paf.type > 0 && Tables.skill_table[paf.type].msg_off != null)
                            {
                                Comm.send_to_char(Tables.skill_table[paf.type].msg_off, ch);
                                Comm.send_to_char("\n\r", ch);
                            }
                        }

                        Handler.affect_remove(ch, paf);
                    }
                }

                if (Handler.is_affected(ch, Gsn.plague) && ch != null)
                {
                    if (ch.in_room == null)
                        continue;

                    Comm.act("$n writhes in agony as plague sores erupt from $s skin.",
                        ch, null, null, TO_ROOM);
                    Comm.send_to_char("You writhe in agony from the plague.\n\r", ch);
                    AffectData af;
                    for (af = ch.affected; af != null; af = af.next)
                    {
                        if (af.type == Gsn.plague)
                            break;
                    }

                    if (af == null)
                    {
                        Bit.REMOVE_BIT(ref ch.affected_by, AFF_PLAGUE);
                        continue;
                    }

                    if (af.level == 1)
                        continue;

                    var plague = new AffectData();
                    plague.where = TO_AFFECTS;
                    plague.type = Gsn.plague;
                    plague.level = af.level - 1;
                    plague.duration = RomRandom.number_range(1, 2 * plague.level);
                    plague.location = APPLY_STR;
                    plague.modifier = -5;
                    plague.bitvector = AFF_PLAGUE;

                    for (var vch = ch.in_room.people; vch != null;
                         vch = vch.next_in_room)
                    {
                        if (!Magic.saves_spell(plague.level - 2, vch, DAM_DISEASE)
                            && !Bit.IS_IMMORTAL(vch)
                            && !Bit.IS_AFFECTED(vch, AFF_PLAGUE) && RomRandom.number_bits(4) == 0)
                        {
                            Comm.send_to_char("You feel hot and feverish.\n\r", vch);
                            Comm.act("$n shivers and looks very ill.", vch, null, null,
                                TO_ROOM);
                            Handler.affect_join(vch, plague);
                        }
                    }

                    int dam = Bit.UMIN(ch.level, af.level / 5 + 1);
                    ch.mana -= dam;
                    ch.move -= dam;
                    Fight.damage(ch, ch, dam, Gsn.plague, DAM_DISEASE, false);
                }
                else if (Bit.IS_AFFECTED(ch, AFF_POISON) && ch != null
                         && !Bit.IS_AFFECTED(ch, AFF_SLOW))
                {
                    var poison = Handler.affect_find(ch.affected, Gsn.poison);

                    if (poison != null)
                    {
                        Comm.act("$n shivers and suffers.", ch, null, null, TO_ROOM);
                        Comm.send_to_char("You shiver and suffer.\n\r", ch);
                        Fight.damage(ch, ch, poison.level / 10 + 1, Gsn.poison,
                            DAM_POISON, false);
                    }
                }

                else if (ch.position == POS_INCAP && RomRandom.number_range(0, 1) == 0)
                {
                    Fight.damage(ch, ch, 1, TYPE_UNDEFINED, DAM_NONE, false);
                }
                else if (ch.position == POS_MORTAL)
                {
                    Fight.damage(ch, ch, 1, TYPE_UNDEFINED, DAM_NONE, false);
                }
            }

            for (var ch = Game.char_list; ch != null; ch = ch_next)
            {
                if (!Bit.IS_VALID(ch))
                {
                    Db.bug("update_char: Trying to work with an invalidated character.\n", 0);
                    break;
                }

                ch_next = ch.next;

                if (ch.desc != null && ch.desc.descriptor % 30 == save_number)
                {
                    Save.save_char_obj(ch);
                }

                if (ch == ch_quit)
                {
                    Interp.do_function(ch, Interp.do_quit, "");
                }
            }
        }

        static void obj_update()
        {
            ObjData obj_next;
            for (var obj = Game.object_list; obj != null; obj = obj_next)
            {
                obj_next = obj.next;

                AffectData paf_next;
                for (var paf = obj.affected; paf != null; paf = paf_next)
                {
                    paf_next = paf.next;
                    if (paf.duration > 0)
                    {
                        paf.duration--;
                        if (RomRandom.number_range(0, 4) == 0 && paf.level > 0)
                            paf.level--;
                    }
                    else if (paf.duration < 0) { }
                    else
                    {
                        if (paf_next == null
                            || paf_next.type != paf.type || paf_next.duration > 0)
                        {
                            if (paf.type > 0 && Tables.skill_table[paf.type].msg_obj != null)
                            {
                                if (obj.carried_by != null)
                                {
                                    var rch = obj.carried_by;
                                    Comm.act(Tables.skill_table[paf.type].msg_obj,
                                        rch, obj, null, TO_CHAR);
                                }
                                if (obj.in_room != null
                                    && obj.in_room.people != null)
                                {
                                    var rch = obj.in_room.people;
                                    Comm.act(Tables.skill_table[paf.type].msg_obj,
                                        rch, obj, null, TO_ALL);
                                }
                            }
                        }

                        Handler.affect_remove_obj(obj, paf);
                    }
                }

                if (obj.timer <= 0 || --obj.timer > 0)
                    continue;

                string message;
                switch (obj.item_type)
                {
                    default:
                        message = "$p crumbles into dust.";
                        break;
                    case ITEM_FOUNTAIN:
                        message = "$p dries up.";
                        break;
                    case ITEM_CORPSE_NPC:
                        message = "$p decays into dust.";
                        break;
                    case ITEM_CORPSE_PC:
                        message = "$p decays into dust.";
                        break;
                    case ITEM_FOOD:
                        message = "$p decomposes.";
                        break;
                    case ITEM_POTION:
                        message = "$p has evaporated from disuse.";
                        break;
                    case ITEM_PORTAL:
                        message = "$p fades out of existence.";
                        break;
                    case ITEM_CONTAINER:
                        if (Bit.CAN_WEAR(obj, ITEM_WEAR_FLOAT))
                            if (obj.contains != null)
                                message =
                                    "$p flickers and vanishes, spilling its contents on the floor.";
                            else
                                message = "$p flickers and vanishes.";
                        else
                            message = "$p crumbles into dust.";
                        break;
                }

                if (obj.carried_by != null)
                {
                    if (Bit.IS_NPC(obj.carried_by)
                        && obj.carried_by.pIndexData.pShop != null)
                        obj.carried_by.silver += obj.cost / 5;
                    else
                    {
                        Comm.act(message, obj.carried_by, obj, null, TO_CHAR);
                        if (obj.wear_loc == WEAR_FLOAT)
                            Comm.act(message, obj.carried_by, obj, null, TO_ROOM);
                    }
                }
                else if (obj.in_room != null && obj.in_room.people != null)
                {
                    var rch = obj.in_room.people;
                    if (!(obj.in_obj != null && obj.in_obj.pIndexData.vnum == OBJ_VNUM_PIT
                          && !Bit.CAN_WEAR(obj.in_obj, ITEM_TAKE)))
                    {
                        Comm.act(message, rch, obj, null, TO_ROOM);
                        Comm.act(message, rch, obj, null, TO_CHAR);
                    }
                }

                if ((obj.item_type == ITEM_CORPSE_PC || obj.wear_loc == WEAR_FLOAT)
                    && obj.contains != null)
                {
                    ObjData next_obj;
                    for (var t_obj = obj.contains; t_obj != null; t_obj = next_obj)
                    {
                        next_obj = t_obj.next_content;
                        Handler.obj_from_obj(t_obj);

                        if (obj.in_obj != null)
                            Handler.obj_to_obj(t_obj, obj.in_obj);

                        else if (obj.carried_by != null)
                            if (obj.wear_loc == WEAR_FLOAT)
                                if (obj.carried_by.in_room == null)
                                    Handler.extract_obj(t_obj);
                                else
                                    Handler.obj_to_room(t_obj, obj.carried_by.in_room);
                            else
                                Handler.obj_to_char(t_obj, obj.carried_by);

                        else if (obj.in_room == null)
                            Handler.extract_obj(t_obj);

                        else
                            Handler.obj_to_room(t_obj, obj.in_room);
                    }
                }

                Handler.extract_obj(obj);
            }
        }

        static void aggr_update()
        {
            CharData wch_next;
            for (var wch = Game.char_list; wch != null; wch = wch_next)
            {
                wch_next = wch.next;
                if (Bit.IS_NPC(wch)
                    || wch.level >= LEVEL_IMMORTAL
                    || wch.in_room == null || wch.in_room.area.empty) continue;

                CharData ch_next;
                for (var ch = wch.in_room.people; ch != null; ch = ch_next)
                {
                    ch_next = ch.next_in_room;

                    if (!Bit.IS_NPC(ch)
                        || !Bit.IS_SET(ch.act, ACT_AGGRESSIVE)
                        || Bit.IS_SET(ch.in_room.room_flags, ROOM_SAFE)
                        || Bit.IS_AFFECTED(ch, AFF_CALM)
                        || ch.fighting != null || Bit.IS_AFFECTED(ch, AFF_CHARM)
                        || !Bit.IS_AWAKE(ch)
                        || (Bit.IS_SET(ch.act, ACT_WIMPY) && Bit.IS_AWAKE(wch))
                        || !Handler.can_see(ch, wch) || RomRandom.number_bits(1) == 0)
                        continue;

                    int count = 0;
                    CharData victim = null;
                    CharData vch_next;
                    for (var vch = wch.in_room.people; vch != null; vch = vch_next)
                    {
                        vch_next = vch.next_in_room;

                        if (!Bit.IS_NPC(vch)
                            && vch.level < LEVEL_IMMORTAL
                            && ch.level >= vch.level - 5
                            && (!Bit.IS_SET(ch.act, ACT_WIMPY) || !Bit.IS_AWAKE(vch))
                            && Handler.can_see(ch, vch))
                        {
                            if (RomRandom.number_range(0, count) == 0)
                                victim = vch;
                            count++;
                        }
                    }

                    if (victim == null)
                        continue;

                    Fight.multi_hit(ch, victim, TYPE_UNDEFINED);
                }
            }
        }

        public static void update_handler()
        {
            if (--pulse_area <= 0)
            {
                pulse_area = PULSE_AREA;
                Db.area_update();
            }

            if (--pulse_music <= 0)
            {
                pulse_music = PULSE_MUSIC;
                Music.song_update();
            }

            if (--pulse_mobile <= 0)
            {
                pulse_mobile = PULSE_MOBILE;
                mobile_update();
            }

            if (--pulse_violence <= 0)
            {
                pulse_violence = PULSE_VIOLENCE;
                Fight.violence_update();
            }

            if (--pulse_point <= 0)
            {
                Comm.wiznet("TICK!", null, null, WIZ_TICKS, 0, 0);
                pulse_point = PULSE_TICK;
                weather_update();
                char_update();
                obj_update();
            }

            aggr_update();
        }
    }
}
