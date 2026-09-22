using static Rom24.Merc;

namespace Rom24
{
    public static class Handler
    {
        public static int get_trust(CharData ch)
        {
            if (ch.desc != null && ch.desc.original != null)
                ch = ch.desc.original;
            if (ch.trust != 0)
                return ch.trust;
            if (Bit.IS_NPC(ch) && ch.level >= LEVEL_HERO)
                return LEVEL_HERO - 1;
            return ch.level;
        }

        public static int get_age(CharData ch)
            => 17 + (ch.played + (int)(Game.current_time - ch.logon)) / 72000;

        public static void reset_char(CharData ch)
        {
            if (Bit.IS_NPC(ch))
                return;

            if (ch.pcdata.perm_hit == 0
                || ch.pcdata.perm_mana == 0
                || ch.pcdata.perm_move == 0 || ch.pcdata.last_level == 0)
            {
                for (int loc = 0; loc < MAX_WEAR; loc++)
                {
                    var obj = get_eq_char(ch, loc);
                    if (obj == null)
                        continue;
                    if (!obj.enchanted)
                        for (var af = obj.pIndexData.affected; af != null; af = af.next)
                        {
                            int mod = af.modifier;
                            switch (af.location)
                            {
                                case APPLY_SEX:
                                    ch.sex -= mod;
                                    if (ch.sex < 0 || ch.sex > 2)
                                        ch.sex = Bit.IS_NPC(ch) ? 0 : ch.pcdata.true_sex;
                                    break;
                                case APPLY_MANA:
                                    ch.max_mana -= mod;
                                    break;
                                case APPLY_HIT:
                                    ch.max_hit -= mod;
                                    break;
                                case APPLY_MOVE:
                                    ch.max_move -= mod;
                                    break;
                            }
                        }

                    for (var af = obj.affected; af != null; af = af.next)
                    {
                        int mod = af.modifier;
                        switch (af.location)
                        {
                            case APPLY_SEX:
                                ch.sex -= mod;
                                break;
                            case APPLY_MANA:
                                ch.max_mana -= mod;
                                break;
                            case APPLY_HIT:
                                ch.max_hit -= mod;
                                break;
                            case APPLY_MOVE:
                                ch.max_move -= mod;
                                break;
                        }
                    }
                }
                ch.pcdata.perm_hit = ch.max_hit;
                ch.pcdata.perm_mana = ch.max_mana;
                ch.pcdata.perm_move = ch.max_move;
                ch.pcdata.last_level = ch.played / 3600;
                if (ch.pcdata.true_sex < 0 || ch.pcdata.true_sex > 2)
                {
                    if (ch.sex > 0 && ch.sex < 3)
                        ch.pcdata.true_sex = ch.sex;
                    else
                        ch.pcdata.true_sex = 0;
                }
            }

            for (int stat = 0; stat < MAX_STATS; stat++)
                ch.mod_stat[stat] = 0;

            if (ch.pcdata.true_sex < 0 || ch.pcdata.true_sex > 2)
                ch.pcdata.true_sex = 0;
            ch.sex = ch.pcdata.true_sex;
            ch.max_hit = ch.pcdata.perm_hit;
            ch.max_mana = ch.pcdata.perm_mana;
            ch.max_move = ch.pcdata.perm_move;

            for (int i = 0; i < 4; i++)
                ch.armor[i] = 100;

            ch.hitroll = 0;
            ch.damroll = 0;
            ch.saving_throw = 0;

            for (int loc = 0; loc < MAX_WEAR; loc++)
            {
                var obj = get_eq_char(ch, loc);
                if (obj == null)
                    continue;
                for (int i = 0; i < 4; i++)
                    ch.armor[i] -= apply_ac(obj, loc, i);

                if (!obj.enchanted)
                    for (var af = obj.pIndexData.affected; af != null; af = af.next)
                        reset_char_apply(ch, af.modifier, af.location);

                for (var af = obj.affected; af != null; af = af.next)
                    reset_char_apply(ch, af.modifier, af.location);
            }

            for (var af = ch.affected; af != null; af = af.next)
                reset_char_apply(ch, af.modifier, af.location);

            if (ch.sex < 0 || ch.sex > 2)
                ch.sex = ch.pcdata.true_sex;
        }

        static void reset_char_apply(CharData ch, int mod, int location)
        {
            switch (location)
            {
                case APPLY_STR:
                    ch.mod_stat[STAT_STR] += mod;
                    break;
                case APPLY_DEX:
                    ch.mod_stat[STAT_DEX] += mod;
                    break;
                case APPLY_INT:
                    ch.mod_stat[STAT_INT] += mod;
                    break;
                case APPLY_WIS:
                    ch.mod_stat[STAT_WIS] += mod;
                    break;
                case APPLY_CON:
                    ch.mod_stat[STAT_CON] += mod;
                    break;
                case APPLY_SEX:
                    ch.sex += mod;
                    break;
                case APPLY_MANA:
                    ch.max_mana += mod;
                    break;
                case APPLY_HIT:
                    ch.max_hit += mod;
                    break;
                case APPLY_MOVE:
                    ch.max_move += mod;
                    break;
                case APPLY_AC:
                    for (int i = 0; i < 4; i++)
                        ch.armor[i] += mod;
                    break;
                case APPLY_HITROLL:
                    ch.hitroll += mod;
                    break;
                case APPLY_DAMROLL:
                    ch.damroll += mod;
                    break;
                case APPLY_SAVES:
                    ch.saving_throw += mod;
                    break;
                case APPLY_SAVING_ROD:
                    ch.saving_throw += mod;
                    break;
                case APPLY_SAVING_PETRI:
                    ch.saving_throw += mod;
                    break;
                case APPLY_SAVING_BREATH:
                    ch.saving_throw += mod;
                    break;
                case APPLY_SAVING_SPELL:
                    ch.saving_throw += mod;
                    break;
            }
        }

        public static int get_curr_stat(CharData ch, int stat)
        {
            int max;
            if (Bit.IS_NPC(ch) || ch.level > LEVEL_IMMORTAL)
                max = 25;
            else
            {
                max = Tables.pc_race_table[ch.race].max_stats[stat] + 4;
                if (Tables.class_table[ch.klass].attr_prime == stat)
                    max += 2;
                if (ch.race == Lookup.race_lookup("human"))
                    max += 1;
                max = Bit.UMIN(max, 25);
            }
            return Bit.URANGE(3, ch.perm_stat[stat] + ch.mod_stat[stat], max);
        }

        public static int get_max_train(CharData ch, int stat)
        {
            if (Bit.IS_NPC(ch) || ch.level > LEVEL_IMMORTAL)
                return 25;

            int max = Tables.pc_race_table[ch.race].max_stats[stat];
            if (Tables.class_table[ch.klass].attr_prime == stat)
            {
                if (ch.race == Lookup.race_lookup("human"))
                    max += 3;
                else
                    max += 2;
            }

            return Bit.UMIN(max, 25);
        }

        public static int can_carry_n(CharData ch)
        {
            if (!Bit.IS_NPC(ch) && ch.level >= LEVEL_IMMORTAL) return 1000;
            if (Bit.IS_NPC(ch) && Bit.IS_SET(ch.act, ACT_PET)) return 0;
            return MAX_WEAR + 2 * get_curr_stat(ch, STAT_DEX) + ch.level;
        }

        public static int can_carry_w(CharData ch)
        {
            if (!Bit.IS_NPC(ch) && ch.level >= LEVEL_IMMORTAL) return 10000000;
            if (Bit.IS_NPC(ch) && Bit.IS_SET(ch.act, ACT_PET)) return 0;
            return Tables.str_app[get_curr_stat(ch, STAT_STR)].carry * 10 + ch.level * 25;
        }

        public static int GET_AC(CharData ch, int type)
            => ch.armor[type] + (Bit.IS_AWAKE(ch) ? Tables.dex_app[get_curr_stat(ch, STAT_DEX)].defensive : 0);

        public static int GET_HITROLL(CharData ch)
            => ch.hitroll + Tables.str_app[get_curr_stat(ch, STAT_STR)].tohit;

        public static int GET_DAMROLL(CharData ch)
            => ch.damroll + Tables.str_app[get_curr_stat(ch, STAT_STR)].todam;

        public static int get_skill(CharData ch, int sn)
        {
            int skill;
            if (sn == -1)
                skill = ch.level * 5 / 2;
            else if (sn < -1 || sn > MAX_SKILL)
            {
                Db.bug("Bad sn %d in get_skill.", sn);
                skill = 0;
            }
            else if (!Bit.IS_NPC(ch))
            {
                if (ch.level < Tables.skill_table[sn].skill_level[ch.klass])
                    skill = 0;
                else
                    skill = ch.pcdata.learned[sn];
            }
            else
            {
                if (Tables.skill_table[sn].spell_fun != Magic.spell_null)
                    skill = 40 + 2 * ch.level;
                else if (sn == Gsn.sneak || sn == Gsn.hide)
                    skill = ch.level * 2 + 20;
                else if ((sn == Gsn.dodge && Bit.IS_SET(ch.off_flags, OFF_DODGE))
                         || (sn == Gsn.parry && Bit.IS_SET(ch.off_flags, OFF_PARRY)))
                    skill = ch.level * 2;
                else if (sn == Gsn.shield_block)
                    skill = 10 + 2 * ch.level;
                else if (sn == Gsn.second_attack && (Bit.IS_SET(ch.act, ACT_WARRIOR) || Bit.IS_SET(ch.act, ACT_THIEF)))
                    skill = 10 + 3 * ch.level;
                else if (sn == Gsn.third_attack && Bit.IS_SET(ch.act, ACT_WARRIOR))
                    skill = 4 * ch.level - 40;
                else if (sn == Gsn.hand_to_hand)
                    skill = 40 + 2 * ch.level;
                else if (sn == Gsn.trip && Bit.IS_SET(ch.off_flags, OFF_TRIP))
                    skill = 10 + 3 * ch.level;
                else if (sn == Gsn.bash && Bit.IS_SET(ch.off_flags, OFF_BASH))
                    skill = 10 + 3 * ch.level;
                else if (sn == Gsn.disarm && (Bit.IS_SET(ch.off_flags, OFF_DISARM) || Bit.IS_SET(ch.act, ACT_WARRIOR) || Bit.IS_SET(ch.act, ACT_THIEF)))
                    skill = 20 + 3 * ch.level;
                else if (sn == Gsn.berserk && Bit.IS_SET(ch.off_flags, OFF_BERSERK))
                    skill = 3 * ch.level;
                else if (sn == Gsn.kick)
                    skill = 10 + 3 * ch.level;
                else if (sn == Gsn.backstab && Bit.IS_SET(ch.act, ACT_THIEF))
                    skill = 20 + 2 * ch.level;
                else if (sn == Gsn.rescue)
                    skill = 40 + ch.level;
                else if (sn == Gsn.recall)
                    skill = 40 + ch.level;
                else if (sn == Gsn.sword || sn == Gsn.dagger || sn == Gsn.spear || sn == Gsn.mace
                         || sn == Gsn.axe || sn == Gsn.flail || sn == Gsn.whip || sn == Gsn.polearm)
                    skill = 40 + 5 * ch.level / 2;
                else
                    skill = 0;
            }
            if (ch.daze > 0)
            {
                if (sn >= 0 && Tables.skill_table[sn].spell_fun != Magic.spell_null)
                    skill /= 2;
                else
                    skill = 2 * skill / 3;
            }
            if (!Bit.IS_NPC(ch) && ch.pcdata.condition[COND_DRUNK] > 10)
                skill = 9 * skill / 10;
            return Bit.URANGE(0, skill, 100);
        }

        public static int get_weapon_sn(CharData ch)
        {
            var wield = get_eq_char(ch, WEAR_WIELD);
            if (wield == null || wield.item_type != ITEM_WEAPON)
                return Gsn.hand_to_hand;
            return wield.value[0] switch
            {
                WEAPON_SWORD => Gsn.sword,
                WEAPON_DAGGER => Gsn.dagger,
                WEAPON_SPEAR => Gsn.spear,
                WEAPON_MACE => Gsn.mace,
                WEAPON_AXE => Gsn.axe,
                WEAPON_FLAIL => Gsn.flail,
                WEAPON_WHIP => Gsn.whip,
                WEAPON_POLEARM => Gsn.polearm,
                _ => -1
            };
        }

        public static int get_weapon_skill(CharData ch, int sn)
        {
            int skill;
            if (Bit.IS_NPC(ch))
            {
                if (sn == -1) skill = 3 * ch.level;
                else if (sn == Gsn.hand_to_hand) skill = 40 + 2 * ch.level;
                else skill = 40 + 5 * ch.level / 2;
            }
            else
            {
                if (sn == -1) skill = 3 * ch.level;
                else skill = ch.pcdata.learned[sn];
            }
            return Bit.URANGE(0, skill, 100);
        }

        public static int check_immune(CharData ch, int dam_type)
        {
            int def = IS_NORMAL;
            if (dam_type == DAM_NONE) return -1;
            if (dam_type <= 3)
            {
                if (Bit.IS_SET(ch.imm_flags, IMM_WEAPON)) def = IS_IMMUNE;
                else if (Bit.IS_SET(ch.res_flags, RES_WEAPON)) def = IS_RESISTANT;
                else if (Bit.IS_SET(ch.vuln_flags, VULN_WEAPON)) def = IS_VULNERABLE;
            }
            else
            {
                if (Bit.IS_SET(ch.imm_flags, IMM_MAGIC)) def = IS_IMMUNE;
                else if (Bit.IS_SET(ch.res_flags, RES_MAGIC)) def = IS_RESISTANT;
                else if (Bit.IS_SET(ch.vuln_flags, VULN_MAGIC)) def = IS_VULNERABLE;
            }
            long bit = dam_type switch
            {
                DAM_BASH => IMM_BASH,
                DAM_PIERCE => IMM_PIERCE,
                DAM_SLASH => IMM_SLASH,
                DAM_FIRE => IMM_FIRE,
                DAM_COLD => IMM_COLD,
                DAM_LIGHTNING => IMM_LIGHTNING,
                DAM_ACID => IMM_ACID,
                DAM_POISON => IMM_POISON,
                DAM_NEGATIVE => IMM_NEGATIVE,
                DAM_HOLY => IMM_HOLY,
                DAM_ENERGY => IMM_ENERGY,
                DAM_MENTAL => IMM_MENTAL,
                DAM_DISEASE => IMM_DISEASE,
                DAM_DROWNING => IMM_DROWNING,
                DAM_LIGHT => IMM_LIGHT,
                DAM_CHARM => IMM_CHARM,
                DAM_SOUND => IMM_SOUND,
                _ => 0
            };
            if (bit != 0)
            {
                if (Bit.IS_SET(ch.imm_flags, bit)) def = IS_IMMUNE;
                else if (Bit.IS_SET(ch.res_flags, bit)) def = IS_RESISTANT;
                else if (Bit.IS_SET(ch.vuln_flags, bit)) def = IS_VULNERABLE;
            }
            return def;
        }

        public static void extract_char(CharData ch, bool fPull)
        {
            ActComm.nuke_pets(ch);
            ch.pet = null;

            if (fPull)
                ActComm.die_follower(ch);

            Fight.stop_fighting(ch, true);

            ObjData obj_next;
            for (var obj = ch.carrying; obj != null; obj = obj_next)
            {
                obj_next = obj.next_content;
                extract_obj(obj);
            }

            if (ch.in_room != null)
                char_from_room(ch);

            if (!fPull)
            {
                char_to_room(ch, get_room_index(Tables.clan_table[ch.clan].hall));
                return;
            }

            if (Bit.IS_NPC(ch))
                --ch.pIndexData.count;

            if (ch.desc != null && ch.desc.original != null)
            {
                Interp.do_function(ch, ActWiz.do_return, "");
                ch.desc = null;
            }

            for (var wch = Game.char_list; wch != null; wch = wch.next)
            {
                if (wch.reply == ch)
                    wch.reply = null;
                if (ch.mprog_target == wch)
                    wch.mprog_target = null;
            }

            if (ch == Game.char_list)
            {
                Game.char_list = ch.next;
            }
            else
            {
                CharData prev;
                for (prev = Game.char_list; prev != null; prev = prev.next)
                {
                    if (prev.next == ch)
                    {
                        prev.next = ch.next;
                        break;
                    }
                }

                if (prev == null)
                {
                    Db.bug("Extract_char: char not found.", 0);
                    return;
                }
            }

            if (ch.desc != null)
                ch.desc.character = null;
            Save.free_char(ch);
        }

        public static bool is_same_group(CharData ach, CharData bch)
        {
            if (ach == null || bch == null) return false;
            if (ach.leader != null) ach = ach.leader;
            if (bch.leader != null) bch = bch.leader;
            return ach == bch;
        }

        public static void affect_strip(CharData ch, int sn)
        {
            AffectData paf_next;
            for (var paf = ch.affected; paf != null; paf = paf_next)
            {
                paf_next = paf.next;
                if (paf.type == sn)
                    affect_remove(ch, paf);
            }
        }

        public static void affect_modify(CharData ch, AffectData paf, bool fAdd)
        {
            int mod = paf.modifier;

            if (fAdd)
            {
                switch (paf.where)
                {
                    case TO_AFFECTS:
                        Bit.SET_BIT(ref ch.affected_by, paf.bitvector);
                        break;
                    case TO_IMMUNE:
                        Bit.SET_BIT(ref ch.imm_flags, paf.bitvector);
                        break;
                    case TO_RESIST:
                        Bit.SET_BIT(ref ch.res_flags, paf.bitvector);
                        break;
                    case TO_VULN:
                        Bit.SET_BIT(ref ch.vuln_flags, paf.bitvector);
                        break;
                }
            }
            else
            {
                switch (paf.where)
                {
                    case TO_AFFECTS:
                        Bit.REMOVE_BIT(ref ch.affected_by, paf.bitvector);
                        break;
                    case TO_IMMUNE:
                        Bit.REMOVE_BIT(ref ch.imm_flags, paf.bitvector);
                        break;
                    case TO_RESIST:
                        Bit.REMOVE_BIT(ref ch.res_flags, paf.bitvector);
                        break;
                    case TO_VULN:
                        Bit.REMOVE_BIT(ref ch.vuln_flags, paf.bitvector);
                        break;
                }
                mod = 0 - mod;
            }

            switch (paf.location)
            {
                default:
                    Db.bug("Affect_modify: unknown location %d.", paf.location);
                    return;

                case APPLY_NONE:
                    break;
                case APPLY_STR:
                    ch.mod_stat[STAT_STR] += mod;
                    break;
                case APPLY_DEX:
                    ch.mod_stat[STAT_DEX] += mod;
                    break;
                case APPLY_INT:
                    ch.mod_stat[STAT_INT] += mod;
                    break;
                case APPLY_WIS:
                    ch.mod_stat[STAT_WIS] += mod;
                    break;
                case APPLY_CON:
                    ch.mod_stat[STAT_CON] += mod;
                    break;
                case APPLY_SEX:
                    ch.sex += mod;
                    break;
                case APPLY_CLASS:
                    break;
                case APPLY_LEVEL:
                    break;
                case APPLY_AGE:
                    break;
                case APPLY_HEIGHT:
                    break;
                case APPLY_WEIGHT:
                    break;
                case APPLY_MANA:
                    ch.max_mana += mod;
                    break;
                case APPLY_HIT:
                    ch.max_hit += mod;
                    break;
                case APPLY_MOVE:
                    ch.max_move += mod;
                    break;
                case APPLY_GOLD:
                    break;
                case APPLY_EXP:
                    break;
                case APPLY_AC:
                    for (int i = 0; i < 4; i++)
                        ch.armor[i] += mod;
                    break;
                case APPLY_HITROLL:
                    ch.hitroll += mod;
                    break;
                case APPLY_DAMROLL:
                    ch.damroll += mod;
                    break;
                case APPLY_SAVES:
                    ch.saving_throw += mod;
                    break;
                case APPLY_SAVING_ROD:
                    ch.saving_throw += mod;
                    break;
                case APPLY_SAVING_PETRI:
                    ch.saving_throw += mod;
                    break;
                case APPLY_SAVING_BREATH:
                    ch.saving_throw += mod;
                    break;
                case APPLY_SAVING_SPELL:
                    ch.saving_throw += mod;
                    break;
                case APPLY_SPELL_AFFECT:
                    break;
            }

            if (!Bit.IS_NPC(ch) && (get_eq_char(ch, WEAR_WIELD)) is ObjData wield
                && get_obj_weight(wield) >
                (Tables.str_app[get_curr_stat(ch, STAT_STR)].wield * 10))
            {
                if (affect_modify_depth == 0)
                {
                    affect_modify_depth++;
                    Comm.act("You drop $p.", ch, wield, null, TO_CHAR);
                    Comm.act("$n drops $p.", ch, wield, null, TO_ROOM);
                    obj_from_char(wield);
                    obj_to_room(wield, ch.in_room);
                    affect_modify_depth--;
                }
            }
        }

        static int affect_modify_depth;

        public static void affect_check(CharData ch, int where, long vector)
        {
            if (where == TO_OBJECT || where == TO_WEAPON || vector == 0)
                return;

            for (var paf = ch.affected; paf != null; paf = paf.next)
                if (paf.where == where && paf.bitvector == vector)
                {
                    switch (where)
                    {
                        case TO_AFFECTS:
                            Bit.SET_BIT(ref ch.affected_by, vector);
                            break;
                        case TO_IMMUNE:
                            Bit.SET_BIT(ref ch.imm_flags, vector);
                            break;
                        case TO_RESIST:
                            Bit.SET_BIT(ref ch.res_flags, vector);
                            break;
                        case TO_VULN:
                            Bit.SET_BIT(ref ch.vuln_flags, vector);
                            break;
                    }
                    return;
                }

            for (var obj = ch.carrying; obj != null; obj = obj.next_content)
            {
                if (obj.wear_loc == -1)
                    continue;

                for (var paf = obj.affected; paf != null; paf = paf.next)
                    if (paf.where == where && paf.bitvector == vector)
                    {
                        switch (where)
                        {
                            case TO_AFFECTS:
                                Bit.SET_BIT(ref ch.affected_by, vector);
                                break;
                            case TO_IMMUNE:
                                Bit.SET_BIT(ref ch.imm_flags, vector);
                                break;
                            case TO_RESIST:
                                Bit.SET_BIT(ref ch.res_flags, vector);
                                break;
                            case TO_VULN:
                                Bit.SET_BIT(ref ch.vuln_flags, vector);
                                break;
                        }
                        return;
                    }

                if (obj.enchanted)
                    continue;

                for (var paf = obj.pIndexData.affected; paf != null; paf = paf.next)
                    if (paf.where == where && paf.bitvector == vector)
                    {
                        switch (where)
                        {
                            case TO_AFFECTS:
                                Bit.SET_BIT(ref ch.affected_by, vector);
                                break;
                            case TO_IMMUNE:
                                Bit.SET_BIT(ref ch.imm_flags, vector);
                                break;
                            case TO_RESIST:
                                Bit.SET_BIT(ref ch.res_flags, vector);
                                break;
                            case TO_VULN:
                                Bit.SET_BIT(ref ch.vuln_flags, vector);
                                break;
                        }
                        return;
                    }
            }
        }

        public static void affect_remove(CharData ch, AffectData paf)
        {
            if (ch.affected == null)
            {
                Db.bug("Affect_remove: no affect.", 0);
                return;
            }

            affect_modify(ch, paf, false);
            int where = paf.where;
            long vector = paf.bitvector;

            if (paf == ch.affected)
            {
                ch.affected = paf.next;
            }
            else
            {
                AffectData prev;
                for (prev = ch.affected; prev != null; prev = prev.next)
                {
                    if (prev.next == paf)
                    {
                        prev.next = paf.next;
                        break;
                    }
                }

                if (prev == null)
                {
                    Db.bug("Affect_remove: cannot find paf.", 0);
                    return;
                }
            }

            affect_check(ch, where, vector);
            Gmcp.Affects(ch);
            Gmcp.Stats(ch);
        }

        public static int get_carry_weight(CharData ch)
            => ch.carry_weight + (int)(ch.silver / 10) + (int)(ch.gold * 2 / 5);

        public static int exp_per_level(CharData ch, int points)
        {
            if (Bit.IS_NPC(ch)) return 1000;
            int expl = 1000, inc = 500;
            var mult = Tables.pc_race_table[ch.race].class_mult[ch.klass];
            if (points < 40)
                return 1000 * (mult != 0 ? mult / 100 : 1);
            points -= 40;
            while (points > 9)
            {
                expl += inc;
                points -= 10;
                if (points > 9)
                {
                    expl += inc;
                    inc *= 2;
                    points -= 10;
                }
            }
            expl += points * inc / 10;
            return expl * mult / 100;
        }

        public static RoomIndexData get_room_index(int vnum)
        {
            if (vnum < 0) return null;
            int iHash = vnum % MAX_KEY_HASH;
            for (var p = Game.room_index_hash[iHash]; p != null; p = p.next)
                if (p.vnum == vnum) return p;
            if (Game.fBootDb)
            {
                Db.bug("Get_room_index: bad vnum %d.", vnum);
                Environment.Exit(1);
            }
            return null;
        }

        public static MobIndexData get_mob_index(int vnum)
        {
            int iHash = vnum % MAX_KEY_HASH;
            if (iHash < 0) return null;
            for (var p = Game.mob_index_hash[iHash]; p != null; p = p.next)
                if (p.vnum == vnum) return p;
            if (Game.fBootDb)
            {
                Db.bug("Get_mob_index: bad vnum %d.", vnum);
                Environment.Exit(1);
            }
            return null;
        }

        public static ObjIndexData get_obj_index(int vnum)
        {
            int iHash = vnum % MAX_KEY_HASH;
            if (iHash < 0) return null;
            for (var p = Game.obj_index_hash[iHash]; p != null; p = p.next)
                if (p.vnum == vnum) return p;
            if (Game.fBootDb)
            {
                Db.bug("Get_obj_index: bad vnum %d.", vnum);
                Environment.Exit(1);
            }
            return null;
        }

        public static void char_to_room(CharData ch, RoomIndexData pRoomIndex)
        {
            if (pRoomIndex == null)
            {
                Db.bug("Char_to_room: NULL.", 0);
                var room = get_room_index(ROOM_VNUM_TEMPLE);
                if (room != null)
                    char_to_room(ch, room);
                return;
            }

            ch.in_room = pRoomIndex;
            ch.next_in_room = pRoomIndex.people;
            pRoomIndex.people = ch;

            if (!Bit.IS_NPC(ch))
            {
                if (ch.in_room.area.empty)
                {
                    ch.in_room.area.empty = false;
                    ch.in_room.area.age = 0;
                }
                ++ch.in_room.area.nplayer;
            }

            var obj = get_eq_char(ch, WEAR_LIGHT);
            if (obj != null
                && obj.item_type == ITEM_LIGHT && obj.value[2] != 0)
                ++ch.in_room.light;

            if (!Bit.IS_NPC(ch) && ch.desc != null && ch.desc.connected == CON_PLAYING)
            {
                Gmcp.RoomInfo(ch);
                Gmcp.AddPlayer(ch);
                Gmcp.Players(ch);
            }

            if (Bit.IS_AFFECTED(ch, AFF_PLAGUE))
            {
                AffectData af;
                for (af = ch.affected; af != null; af = af.next)
                {
                    if (af.type == Gsn.plague)
                        break;
                }

                if (af == null)
                {
                    Bit.REMOVE_BIT(ref ch.affected_by, AFF_PLAGUE);
                    return;
                }

                if (af.level == 1)
                    return;

                var plague = Recycle.new_affect();
                plague.where = TO_AFFECTS;
                plague.type = Gsn.plague;
                plague.level = af.level - 1;
                plague.duration = RomRandom.number_range(1, 2 * plague.level);
                plague.location = APPLY_STR;
                plague.modifier = -5;
                plague.bitvector = AFF_PLAGUE;

                for (var vch = ch.in_room.people; vch != null; vch = vch.next_in_room)
                {
                    if (!Magic.saves_spell(plague.level - 2, vch, DAM_DISEASE)
                        && !Bit.IS_IMMORTAL(vch) &&
                        !Bit.IS_AFFECTED(vch, AFF_PLAGUE) && RomRandom.number_bits(6) == 0)
                    {
                        Comm.send_to_char("You feel hot and feverish.\n\r", vch);
                        Comm.act("$n shivers and looks very ill.", vch, null, null,
                            TO_ROOM);
                        affect_join(vch, plague);
                    }
                }
            }
        }

        public static void char_from_room(CharData ch)
        {
            if (ch.in_room == null)
            {
                Db.bug("Char_from_room: NULL.", 0);
                return;
            }
            Gmcp.RemovePlayer(ch);

            if (!Bit.IS_NPC(ch))
                --ch.in_room.area.nplayer;

            var obj = get_eq_char(ch, WEAR_LIGHT);
            if (obj != null
                && obj.item_type == ITEM_LIGHT
                && obj.value[2] != 0 && ch.in_room.light > 0)
                --ch.in_room.light;

            if (ch == ch.in_room.people)
            {
                ch.in_room.people = ch.next_in_room;
            }
            else
            {
                CharData prev;
                for (prev = ch.in_room.people; prev != null; prev = prev.next_in_room)
                {
                    if (prev.next_in_room == ch)
                    {
                        prev.next_in_room = ch.next_in_room;
                        break;
                    }
                }

                if (prev == null)
                    Db.bug("Char_from_room: ch not found.", 0);
            }

            ch.in_room = null;
            ch.next_in_room = null;
            ch.on = null;
        }

        public static void obj_to_char(ObjData obj, CharData ch)
        {
            obj.next_content = ch.carrying;
            ch.carrying = obj;
            obj.carried_by = ch;
            obj.in_room = null;
            obj.in_obj = null;
            ch.carry_number += get_obj_number(obj);
            ch.carry_weight += get_obj_weight(obj);
            if (obj.wear_loc == WEAR_NONE)
                Gmcp.ItemAdd(ch, obj, "inv");
        }

        public static void obj_from_char(ObjData obj)
        {
            var ch = obj.carried_by;
            if (ch == null)
            {
                Db.bug("Obj_from_char: null ch.", 0);
                return;
            }

            if (obj.wear_loc != WEAR_NONE)
                unequip_char(ch, obj);

            if (ch.carrying == obj)
            {
                ch.carrying = obj.next_content;
            }
            else
            {
                ObjData prev;
                for (prev = ch.carrying; prev != null; prev = prev.next_content)
                {
                    if (prev.next_content == obj)
                    {
                        prev.next_content = obj.next_content;
                        break;
                    }
                }

                if (prev == null)
                    Db.bug("Obj_from_char: obj not in list.", 0);
            }

            obj.carried_by = null;
            obj.next_content = null;
            ch.carry_number -= get_obj_number(obj);
            ch.carry_weight -= get_obj_weight(obj);
            Gmcp.ItemRemove(ch, obj, "inv");
        }

        public static void obj_to_room(ObjData obj, RoomIndexData room)
        {
            obj.next_content = room.contents;
            room.contents = obj;
            obj.in_room = room;
            obj.carried_by = null;
            obj.in_obj = null;
            Gmcp.ItemRoomAdd(room, obj);
        }

        public static void obj_to_obj(ObjData obj, ObjData obj_to)
        {
            obj.next_content = obj_to.contains;
            obj_to.contains = obj;
            obj.in_obj = obj_to;
            obj.in_room = null;
            obj.carried_by = null;
            if (obj_to.pIndexData.vnum == OBJ_VNUM_PIT)
                obj.cost = 0;

            for (; obj_to != null; obj_to = obj_to.in_obj)
            {
                if (obj_to.carried_by != null)
                {
                    obj_to.carried_by.carry_number += get_obj_number(obj);
                    obj_to.carried_by.carry_weight += get_obj_weight(obj)
                        * Bit.WEIGHT_MULT(obj_to) / 100;
                }
            }
        }

        public static ObjData get_eq_char(CharData ch, int iWear)
        {
            if (ch == null)
                return null;

            for (var obj = ch.carrying; obj != null; obj = obj.next_content)
            {
                if (obj.wear_loc == iWear)
                    return obj;
            }

            return null;
        }

        public static int apply_ac(ObjData obj, int iWear, int type)
        {
            if (obj.item_type != ITEM_ARMOR)
                return 0;

            switch (iWear)
            {
                case WEAR_BODY: return 3 * obj.value[type];
                case WEAR_HEAD: return 2 * obj.value[type];
                case WEAR_LEGS: return 2 * obj.value[type];
                case WEAR_FEET: return obj.value[type];
                case WEAR_HANDS: return obj.value[type];
                case WEAR_ARMS: return obj.value[type];
                case WEAR_SHIELD: return obj.value[type];
                case WEAR_NECK_1: return obj.value[type];
                case WEAR_NECK_2: return obj.value[type];
                case WEAR_ABOUT: return 2 * obj.value[type];
                case WEAR_WAIST: return obj.value[type];
                case WEAR_WRIST_L: return obj.value[type];
                case WEAR_WRIST_R: return obj.value[type];
                case WEAR_HOLD: return obj.value[type];
            }
            return 0;
        }

        public static void equip_char(CharData ch, ObjData obj, int iWear)
        {
            if (get_eq_char(ch, iWear) != null)
            {
                Db.bug("Equip_char: already equipped (%d).", iWear);
                return;
            }

            if ((Bit.IS_OBJ_STAT(obj, ITEM_ANTI_EVIL) && Bit.IS_EVIL(ch))
                || (Bit.IS_OBJ_STAT(obj, ITEM_ANTI_GOOD) && Bit.IS_GOOD(ch))
                || (Bit.IS_OBJ_STAT(obj, ITEM_ANTI_NEUTRAL) && Bit.IS_NEUTRAL(ch)))
            {
                Comm.act("You are zapped by $p and drop it.", ch, obj, null, TO_CHAR);
                Comm.act("$n is zapped by $p and drops it.", ch, obj, null, TO_ROOM);
                obj_from_char(obj);
                obj_to_room(obj, ch.in_room);
                return;
            }

            for (int i = 0; i < 4; i++)
                ch.armor[i] -= apply_ac(obj, iWear, i);
            obj.wear_loc = iWear;

            if (!obj.enchanted)
                for (var paf = obj.pIndexData.affected; paf != null; paf = paf.next)
                    if (paf.location != APPLY_SPELL_AFFECT)
                        affect_modify(ch, paf, true);
            for (var paf = obj.affected; paf != null; paf = paf.next)
                if (paf.location == APPLY_SPELL_AFFECT)
                    affect_to_char(ch, paf);
                else
                    affect_modify(ch, paf, true);

            if (obj.item_type == ITEM_LIGHT
                && obj.value[2] != 0 && ch.in_room != null) ++ch.in_room.light;
            Gmcp.ItemRemove(ch, obj, "inv");
            Gmcp.ItemAdd(ch, obj, "eq");
            Gmcp.Stats(ch);
        }

        public static void unequip_char(CharData ch, ObjData obj)
        {
            if (obj.wear_loc == WEAR_NONE)
            {
                Db.bug("Unequip_char: already unequipped.", 0);
                return;
            }

            for (int i = 0; i < 4; i++)
                ch.armor[i] += apply_ac(obj, obj.wear_loc, i);
            Gmcp.ItemRemove(ch, obj, "eq");
            obj.wear_loc = -1;
            Gmcp.ItemAdd(ch, obj, "inv");
            Gmcp.Stats(ch);

            if (!obj.enchanted)
            {
                for (var paf = obj.pIndexData.affected; paf != null; paf = paf.next)
                {
                    if (paf.location == APPLY_SPELL_AFFECT)
                    {
                        AffectData lpaf_next = null;
                        for (var lpaf = ch.affected; lpaf != null; lpaf = lpaf_next)
                        {
                            lpaf_next = lpaf.next;
                            if ((lpaf.type == paf.type) &&
                                (lpaf.level == paf.level) &&
                                (lpaf.location == APPLY_SPELL_AFFECT))
                            {
                                affect_remove(ch, lpaf);
                                lpaf_next = null;
                            }
                        }
                    }
                    else
                    {
                        affect_modify(ch, paf, false);
                        affect_check(ch, paf.where, paf.bitvector);
                    }
                }
            }

            for (var paf = obj.affected; paf != null; paf = paf.next)
                if (paf.location == APPLY_SPELL_AFFECT)
                {
                    Db.bug("Norm-Apply: %d", 0);
                    AffectData lpaf_next = null;
                    for (var lpaf = ch.affected; lpaf != null; lpaf = lpaf_next)
                    {
                        lpaf_next = lpaf.next;
                        if ((lpaf.type == paf.type) &&
                            (lpaf.level == paf.level) &&
                            (lpaf.location == APPLY_SPELL_AFFECT))
                        {
                            Db.bug("location = %d", lpaf.location);
                            Db.bug("type = %d", lpaf.type);
                            affect_remove(ch, lpaf);
                            lpaf_next = null;
                        }
                    }
                }
                else
                {
                    affect_modify(ch, paf, false);
                    affect_check(ch, paf.where, paf.bitvector);
                }

            if (obj.item_type == ITEM_LIGHT
                && obj.value[2] != 0
                && ch.in_room != null
                && ch.in_room.light > 0) --ch.in_room.light;
        }

        public static int count_obj_list(ObjIndexData pObjIndex, ObjData list)
        {
            int n = 0;
            for (var obj = list; obj != null; obj = obj.next_content)
                if (obj.pIndexData == pObjIndex) n++;
            return n;
        }

        public static ObjData get_obj_type(ObjIndexData pObjIndex)
        {
            for (var obj = Game.object_list; obj != null; obj = obj.next)
                if (obj.pIndexData == pObjIndex) return obj;
            return null;
        }

        public static bool can_see(CharData ch, CharData victim)
        {
            if (ch == victim)
                return true;

            if (get_trust(ch) < victim.invis_level)
                return false;

            if (get_trust(ch) < victim.incog_level
                && ch.in_room != victim.in_room) return false;

            if ((!Bit.IS_NPC(ch) && Bit.IS_SET(ch.act, PLR_HOLYLIGHT))
                || (Bit.IS_NPC(ch) && Bit.IS_IMMORTAL(ch)))
                return true;

            if (Bit.IS_AFFECTED(ch, AFF_BLIND))
                return false;

            if (room_is_dark(ch.in_room) && !Bit.IS_AFFECTED(ch, AFF_INFRARED))
                return false;

            if (Bit.IS_AFFECTED(victim, AFF_INVISIBLE)
                && !Bit.IS_AFFECTED(ch, AFF_DETECT_INVIS))
                return false;

            if (Bit.IS_AFFECTED(victim, AFF_SNEAK)
                && !Bit.IS_AFFECTED(ch, AFF_DETECT_HIDDEN) && victim.fighting == null)
            {
                int chance = get_skill(victim, Gsn.sneak);
                chance += get_curr_stat(victim, STAT_DEX) * 3 / 2;
                chance -= get_curr_stat(ch, STAT_INT) * 2;
                chance -= ch.level - victim.level * 3 / 2;

                if (RomRandom.number_percent() < chance)
                    return false;
            }

            if (Bit.IS_AFFECTED(victim, AFF_HIDE)
                && !Bit.IS_AFFECTED(ch, AFF_DETECT_HIDDEN) && victim.fighting == null)
                return false;

            return true;
        }

        public static bool can_see_obj(CharData ch, ObjData obj)
        {
            if (!Bit.IS_NPC(ch) && Bit.IS_SET(ch.act, PLR_HOLYLIGHT))
                return true;

            if (Bit.IS_SET(obj.extra_flags, ITEM_VIS_DEATH))
                return false;

            if (Bit.IS_AFFECTED(ch, AFF_BLIND) && obj.item_type != ITEM_POTION)
                return false;

            if (obj.item_type == ITEM_LIGHT && obj.value[2] != 0)
                return true;

            if (Bit.IS_SET(obj.extra_flags, ITEM_INVIS)
                && !Bit.IS_AFFECTED(ch, AFF_DETECT_INVIS))
                return false;

            if (Bit.IS_OBJ_STAT(obj, ITEM_GLOW))
                return true;

            if (room_is_dark(ch.in_room) && !Bit.IS_AFFECTED(ch, AFF_DARK_VISION))
                return false;

            return true;
        }

        public static bool can_see_room(CharData ch, RoomIndexData pRoomIndex)
        {
            if (Bit.IS_SET(pRoomIndex.room_flags, ROOM_IMP_ONLY)
                && get_trust(ch) < MAX_LEVEL)
                return false;

            if (Bit.IS_SET(pRoomIndex.room_flags, ROOM_GODS_ONLY) && !Bit.IS_IMMORTAL(ch))
                return false;

            if (Bit.IS_SET(pRoomIndex.room_flags, ROOM_HEROES_ONLY)
                && !Bit.IS_IMMORTAL(ch))
                return false;

            if (Bit.IS_SET(pRoomIndex.room_flags, ROOM_NEWBIES_ONLY)
                && ch.level > 5 && !Bit.IS_IMMORTAL(ch))
                return false;

            if (!Bit.IS_IMMORTAL(ch) && pRoomIndex.clan != 0 && ch.clan != pRoomIndex.clan)
                return false;

            return true;
        }

        public static bool is_friend(CharData ch, CharData victim)
        {
            if (is_same_group(ch, victim))
                return true;

            if (!Bit.IS_NPC(ch))
                return false;

            if (!Bit.IS_NPC(victim))
            {
                if (Bit.IS_SET(ch.off_flags, ASSIST_PLAYERS))
                    return true;
                else
                    return false;
            }

            if (Bit.IS_AFFECTED(ch, AFF_CHARM))
                return false;

            if (Bit.IS_SET(ch.off_flags, ASSIST_ALL))
                return true;

            if (ch.group != 0 && ch.group == victim.group)
                return true;

            if (Bit.IS_SET(ch.off_flags, ASSIST_VNUM)
                && ch.pIndexData == victim.pIndexData)
                return true;

            if (Bit.IS_SET(ch.off_flags, ASSIST_RACE) && ch.race == victim.race)
                return true;

            if (Bit.IS_SET(ch.off_flags, ASSIST_ALIGN)
                && !Bit.IS_SET(ch.act, ACT_NOALIGN)
                && !Bit.IS_SET(victim.act, ACT_NOALIGN)
                && ((Bit.IS_GOOD(ch) && Bit.IS_GOOD(victim))
                    || (Bit.IS_EVIL(ch) && Bit.IS_EVIL(victim)) || (Bit.IS_NEUTRAL(ch)
                                                                      &&
                                                                      Bit.IS_NEUTRAL(victim))))
                return true;

            return false;
        }

        public static int material_lookup(string name)
        {
            return 0;
        }

        public static bool is_old_mob(CharData ch)
        {
            if (ch.pIndexData == null)
                return false;
            else if (ch.pIndexData.new_format)
                return false;
            return true;
        }

        public static bool room_is_dark(RoomIndexData pRoomIndex)
        {
            if (pRoomIndex.light > 0) return false;
            if (Bit.IS_SET(pRoomIndex.room_flags, ROOM_DARK)) return true;
            if (pRoomIndex.sector_type == SECT_INSIDE || pRoomIndex.sector_type == SECT_CITY)
                return false;
            if (Game.weather_info.sunlight == SUN_SET || Game.weather_info.sunlight == SUN_DARK)
                return true;
            return false;
        }

        public static bool room_is_private(RoomIndexData pRoomIndex)
        {
            if (!string.IsNullOrEmpty(pRoomIndex.owner))
                return true;

            int count = 0;
            for (var rch = pRoomIndex.people; rch != null; rch = rch.next_in_room)
                count++;

            if (Bit.IS_SET(pRoomIndex.room_flags, ROOM_PRIVATE) && count >= 2)
                return true;

            if (Bit.IS_SET(pRoomIndex.room_flags, ROOM_SOLITARY) && count >= 1)
                return true;

            if (Bit.IS_SET(pRoomIndex.room_flags, ROOM_IMP_ONLY))
                return true;

            return false;
        }

        public static bool is_room_owner(CharData ch, RoomIndexData room)
        {
            if (room.owner == null || room.owner.Length == 0)
                return false;

            return is_name(ch.name, room.owner);
        }

        public static void affect_to_char(CharData ch, AffectData paf)
        {
            var paf_new = Recycle.new_affect();
            paf_new.where = paf.where;
            paf_new.type = paf.type;
            paf_new.level = paf.level;
            paf_new.duration = paf.duration;
            paf_new.location = paf.location;
            paf_new.modifier = paf.modifier;
            paf_new.bitvector = paf.bitvector;
            paf_new.next = ch.affected;
            ch.affected = paf_new;
            affect_modify(ch, paf_new, true);
            Gmcp.Affects(ch);
            Gmcp.Stats(ch);
        }

        public static void affect_to_obj(ObjData obj, AffectData paf)
        {
            var paf_new = Recycle.new_affect();
            paf_new.where = paf.where;
            paf_new.type = paf.type;
            paf_new.level = paf.level;
            paf_new.duration = paf.duration;
            paf_new.location = paf.location;
            paf_new.modifier = paf.modifier;
            paf_new.bitvector = paf.bitvector;
            paf_new.next = obj.affected;
            obj.affected = paf_new;

            /* apply any affect vectors to the object's extra_flags */
            if (paf.bitvector != 0)
                switch (paf.where)
                {
                    case TO_OBJECT:
                        Bit.SET_BIT(ref obj.extra_flags, paf.bitvector);
                        break;
                    case TO_WEAPON:
                        if (obj.item_type == ITEM_WEAPON)
                        {
                            int v4 = obj.value[4];
                            Bit.SET_BIT(ref v4, paf.bitvector);
                            obj.value[4] = v4;
                        }
                        break;
                }
        }

        public static string PERS(CharData ch, CharData looker)
            => can_see(looker, ch) ? (Bit.IS_NPC(ch) ? ch.short_descr : ch.name) : "someone";

        public static bool check_blind(CharData ch)
        {
            if (!Bit.IS_NPC(ch) && Bit.IS_SET(ch.act, PLR_HOLYLIGHT))
                return true;
            if (Bit.IS_AFFECTED(ch, AFF_BLIND))
            {
                Comm.send_to_char("You can't see a thing!\n\r", ch);
                return false;
            }
            return true;
        }

        public static RoomIndexData get_random_room(CharData ch)
        {
            RoomIndexData room;
            for (;;)
            {
                room = get_room_index(RomRandom.number_range(0, 65535));
                if (room != null)
                    if (can_see_room(ch, room)
                        && !room_is_private(room)
                        && !Bit.IS_SET(room.room_flags, ROOM_PRIVATE)
                        && !Bit.IS_SET(room.room_flags, ROOM_SOLITARY)
                        && !Bit.IS_SET(room.room_flags, ROOM_SAFE)
                        && (Bit.IS_NPC(ch) || Bit.IS_SET(ch.act, ACT_AGGRESSIVE)
                            || !Bit.IS_SET(room.room_flags, ROOM_LAW)))
                        break;
            }
            return room;
        }

        public static CharData get_char_world(CharData ch, string argument)
        {
            var wch = get_char_room(ch, argument);
            if (wch != null)
                return wch;

            int number = RomString.number_argument(argument, out string arg);
            int count = 0;
            for (wch = Game.char_list; wch != null; wch = wch.next)
            {
                if (wch.in_room == null || !can_see(ch, wch)
                    || !is_name(arg, wch.name))
                    continue;
                if (++count == number)
                    return wch;
            }

            return null;
        }

        public static ObjData get_obj_world(CharData ch, string argument)
        {
            var obj = get_obj_here(ch, argument);
            if (obj != null)
                return obj;

            int number = RomString.number_argument(argument, out string arg);
            int count = 0;
            for (obj = Game.object_list; obj != null; obj = obj.next)
            {
                if (can_see_obj(ch, obj) && is_name(arg, obj.name))
                {
                    if (++count == number)
                        return obj;
                }
            }
            return null;
        }

        public static CharData get_char_room(CharData ch, string argument)
        {
            int number = RomString.number_argument(argument, out string arg);
            int count = 0;
            if (!RomString.str_cmp(arg, "self"))
                return ch;
            for (var rch = ch.in_room.people; rch != null; rch = rch.next_in_room)
            {
                if (!can_see(ch, rch) || !is_name(arg, rch.name))
                    continue;
                if (++count == number)
                    return rch;
            }
            return null;
        }

        public static void affect_enchant(ObjData obj)
        {
            if (!obj.enchanted)
            {
                obj.enchanted = true;

                for (var paf = obj.pIndexData.affected; paf != null; paf = paf.next)
                {
                    var af_new = Recycle.new_affect();

                    af_new.next = obj.affected;
                    obj.affected = af_new;

                    af_new.where = paf.where;
                    af_new.type = Bit.UMAX(0, paf.type);
                    af_new.level = paf.level;
                    af_new.duration = paf.duration;
                    af_new.location = paf.location;
                    af_new.modifier = paf.modifier;
                    af_new.bitvector = paf.bitvector;
                }
            }
        }

        public static bool can_drop_obj(CharData ch, ObjData obj)
        {
            if (!Bit.IS_SET(obj.extra_flags, ITEM_NODROP))
                return true;

            if (!Bit.IS_NPC(ch) && ch.level >= LEVEL_IMMORTAL)
                return true;

            return false;
        }

        public static bool is_exact_name(string str, string namelist)
        {
            if (namelist == null)
                return false;

            for (;;)
            {
                namelist = RomString.one_argument(namelist, out string name);
                if (name.Length == 0)
                    return false;
                if (!RomString.str_cmp(str, name))
                    return true;
            }
        }

        public static bool is_name(string str, string namelist)
        {
            if (string.IsNullOrEmpty(namelist))
                return false;
            if (string.IsNullOrEmpty(str))
                return false;

            string string_full = str;
            for (;;)
            {
                str = RomString.one_argument(str, out string part);
                if (part.Length == 0)
                    return true;

                var list = namelist;
                for (;;)
                {
                    list = RomString.one_argument(list, out string name);
                    if (name.Length == 0)
                        return false;

                    if (!RomString.str_prefix(string_full, name))
                        return true;

                    if (!RomString.str_prefix(part, name))
                        break;
                }
            }
        }

        public static bool is_full_name(string str, string namelist)
        {
            namelist ??= "";
            for (;;)
            {
                namelist = RomString.one_argument(namelist, out string name);
                if (name.Length == 0)
                    return false;
                if (!RomString.str_cmp(str, name))
                    return true;
            }
        }

        public static int count_users(ObjData obj)
        {
            int count = 0;

            if (obj.in_room == null)
                return 0;

            for (var fch = obj.in_room.people; fch != null; fch = fch.next_in_room)
                if (fch.on == obj)
                    count++;

            return count;
        }

        public static bool is_clan(CharData ch) => ch.clan != 0;

        public static bool is_same_clan(CharData ch, CharData victim)
        {
            if (Tables.clan_table[ch.clan].independent)
                return false;
            return ch.clan == victim.clan;
        }

        public static AffectData affect_find(AffectData paf, int sn)
        {
            for (var paf_find = paf; paf_find != null; paf_find = paf_find.next)
            {
                if (paf_find.type == sn)
                    return paf_find;
            }
            return null;
        }

        public static bool is_affected(CharData ch, int sn)
        {
            for (var paf = ch.affected; paf != null; paf = paf.next)
            {
                if (paf.type == sn)
                    return true;
            }
            return false;
        }

        public static void affect_join(CharData ch, AffectData paf)
        {
            for (var paf_old = ch.affected; paf_old != null; paf_old = paf_old.next)
            {
                if (paf_old.type == paf.type)
                {
                    paf.level = (paf.level += paf_old.level) / 2;
                    paf.duration += paf_old.duration;
                    paf.modifier += paf_old.modifier;
                    affect_remove(ch, paf_old);
                    break;
                }
            }
            affect_to_char(ch, paf);
        }

        public static void affect_remove_obj(ObjData obj, AffectData paf)
        {
            if (obj.affected == null)
            {
                Db.bug("Affect_remove_object: no affect.", 0);
                return;
            }

            if (obj.carried_by != null && obj.wear_loc != -1)
                affect_modify(obj.carried_by, paf, false);

            int where = paf.where;
            long vector = paf.bitvector;

            if (paf.bitvector != 0)
                switch (paf.where)
                {
                    case TO_OBJECT:
                        Bit.REMOVE_BIT(ref obj.extra_flags, paf.bitvector);
                        break;
                    case TO_WEAPON:
                        if (obj.item_type == ITEM_WEAPON)
                            obj.value[4] = (int)(obj.value[4] & ~paf.bitvector);
                        break;
                }

            if (paf == obj.affected)
            {
                obj.affected = paf.next;
            }
            else
            {
                AffectData prev;
                for (prev = obj.affected; prev != null; prev = prev.next)
                {
                    if (prev.next == paf)
                    {
                        prev.next = paf.next;
                        break;
                    }
                }

                if (prev == null)
                {
                    Db.bug("Affect_remove_object: cannot find paf.", 0);
                    return;
                }
            }

            if (obj.carried_by != null && obj.wear_loc != -1)
                affect_check(obj.carried_by, where, vector);
        }

        public static void obj_from_room(ObjData obj)
        {
            var in_room = obj.in_room;
            if (in_room == null)
            {
                Db.bug("obj_from_room: NULL.", 0);
                return;
            }
            Gmcp.ItemRoomRemove(in_room, obj);

            for (var ch = in_room.people; ch != null; ch = ch.next_in_room)
                if (ch.on == obj)
                    ch.on = null;

            if (obj == in_room.contents)
            {
                in_room.contents = obj.next_content;
            }
            else
            {
                ObjData prev;
                for (prev = in_room.contents; prev != null; prev = prev.next_content)
                {
                    if (prev.next_content == obj)
                    {
                        prev.next_content = obj.next_content;
                        break;
                    }
                }

                if (prev == null)
                {
                    Db.bug("Obj_from_room: obj not found.", 0);
                    return;
                }
            }

            obj.in_room = null;
            obj.next_content = null;
        }

        public static void obj_from_obj(ObjData obj)
        {
            var obj_from = obj.in_obj;
            if (obj_from == null)
            {
                Db.bug("Obj_from_obj: null obj_from.", 0);
                return;
            }

            if (obj == obj_from.contains)
            {
                obj_from.contains = obj.next_content;
            }
            else
            {
                ObjData prev;
                for (prev = obj_from.contains; prev != null; prev = prev.next_content)
                {
                    if (prev.next_content == obj)
                    {
                        prev.next_content = obj.next_content;
                        break;
                    }
                }

                if (prev == null)
                {
                    Db.bug("Obj_from_obj: obj not found.", 0);
                    return;
                }
            }

            obj.next_content = null;
            obj.in_obj = null;

            for (; obj_from != null; obj_from = obj_from.in_obj)
            {
                if (obj_from.carried_by != null)
                {
                    obj_from.carried_by.carry_number -= get_obj_number(obj);
                    obj_from.carried_by.carry_weight -= get_obj_weight(obj)
                        * Bit.WEIGHT_MULT(obj_from) / 100;
                }
            }
        }

        public static void extract_obj(ObjData obj)
        {
            if (obj.in_room != null)
                obj_from_room(obj);
            else if (obj.carried_by != null)
                obj_from_char(obj);
            else if (obj.in_obj != null)
                obj_from_obj(obj);

            ObjData obj_next;
            for (var obj_content = obj.contains; obj_content != null; obj_content = obj_next)
            {
                obj_next = obj_content.next_content;
                extract_obj(obj_content);
            }

            if (Game.object_list == obj)
            {
                Game.object_list = obj.next;
            }
            else
            {
                ObjData prev;
                for (prev = Game.object_list; prev != null; prev = prev.next)
                {
                    if (prev.next == obj)
                    {
                        prev.next = obj.next;
                        break;
                    }
                }

                if (prev == null)
                {
                    Db.bug("Extract_obj: obj %d not found.", obj.pIndexData.vnum);
                    return;
                }
            }

            --obj.pIndexData.count;
            Recycle.free_obj(obj);
        }

        public static int get_obj_number(ObjData obj)
        {
            int number;

            if (obj.item_type == ITEM_CONTAINER || obj.item_type == ITEM_MONEY
                || obj.item_type == ITEM_GEM || obj.item_type == ITEM_JEWELRY)
                number = 0;
            else
                number = 1;

            for (obj = obj.contains; obj != null; obj = obj.next_content)
                number += get_obj_number(obj);

            return number;
        }

        public static int get_obj_weight(ObjData obj)
        {
            int weight = obj.weight;
            for (var tobj = obj.contains; tobj != null; tobj = tobj.next_content)
                weight += get_obj_weight(tobj) * Bit.WEIGHT_MULT(obj) / 100;
            return weight;
        }

        public static int get_true_weight(ObjData obj)
        {
            int weight = obj.weight;
            for (obj = obj.contains; obj != null; obj = obj.next_content)
                weight += get_obj_weight(obj);
            return weight;
        }

        public static ObjData get_obj_list(CharData ch, string argument, ObjData list)
        {
            int number = RomString.number_argument(argument, out string arg);
            int count = 0;
            for (var obj = list; obj != null; obj = obj.next_content)
            {
                if (can_see_obj(ch, obj) && is_name(arg, obj.name))
                {
                    if (++count == number)
                        return obj;
                }
            }
            return null;
        }

        public static ObjData get_obj_carry(CharData ch, string argument, CharData viewer)
        {
            int number = RomString.number_argument(argument, out string arg);
            int count = 0;
            for (var obj = ch.carrying; obj != null; obj = obj.next_content)
            {
                if (obj.wear_loc == WEAR_NONE && can_see_obj(viewer, obj)
                    && is_name(arg, obj.name))
                {
                    if (++count == number)
                        return obj;
                }
            }
            return null;
        }

        public static ObjData get_obj_wear(CharData ch, string argument)
        {
            int number = RomString.number_argument(argument, out string arg);
            int count = 0;
            for (var obj = ch.carrying; obj != null; obj = obj.next_content)
            {
                if (obj.wear_loc != WEAR_NONE && can_see_obj(ch, obj)
                    && is_name(arg, obj.name))
                {
                    if (++count == number)
                        return obj;
                }
            }
            return null;
        }

        public static ObjData get_obj_here(CharData ch, string argument)
        {
            var obj = get_obj_list(ch, argument, ch.in_room.contents);
            if (obj != null)
                return obj;
            if ((obj = get_obj_carry(ch, argument, ch)) != null)
                return obj;
            if ((obj = get_obj_wear(ch, argument)) != null)
                return obj;
            return null;
        }

        public static ObjData create_money(int gold, int silver)
        {
            if (gold < 0 || silver < 0 || (gold == 0 && silver == 0))
            {
                Db.bug("Create_money: zero or negative money.", Bit.UMIN(gold, silver));
                gold = Bit.UMAX(1, gold);
                silver = Bit.UMAX(1, silver);
            }

            ObjData obj;
            if (gold == 0 && silver == 1)
            {
                obj = Db.create_object(get_obj_index(OBJ_VNUM_SILVER_ONE), 0);
            }
            else if (gold == 1 && silver == 0)
            {
                obj = Db.create_object(get_obj_index(OBJ_VNUM_GOLD_ONE), 0);
            }
            else if (silver == 0)
            {
                obj = Db.create_object(get_obj_index(OBJ_VNUM_GOLD_SOME), 0);
                obj.short_descr = RomString.sprintf(obj.short_descr, gold);
                obj.value[1] = gold;
                obj.cost = gold;
                obj.weight = gold / 5;
            }
            else if (gold == 0)
            {
                obj = Db.create_object(get_obj_index(OBJ_VNUM_SILVER_SOME), 0);
                obj.short_descr = RomString.sprintf(obj.short_descr, silver);
                obj.value[0] = silver;
                obj.cost = silver;
                obj.weight = silver / 20;
            }
            else
            {
                obj = Db.create_object(get_obj_index(OBJ_VNUM_COINS), 0);
                obj.short_descr = RomString.sprintf(obj.short_descr, silver, gold);
                obj.value[0] = silver;
                obj.value[1] = gold;
                obj.cost = 100 * gold + silver;
                obj.weight = gold / 5 + silver / 20;
            }

            return obj;
        }

        static string FlagJoin(long flags, params (long bit, string name)[] parts)
        {
            string buf = "";
            foreach (var p in parts)
                if ((flags & p.bit) != 0) buf += " " + p.name;
            return buf.Length != 0 ? buf.Substring(1) : "none";
        }

        public static string item_name(int item_type)
        {
            for (int type = 0; Tables.item_table[type].name != null; type++)
                if (item_type == Tables.item_table[type].type)
                    return Tables.item_table[type].name;
            return "none";
        }

        public static string weapon_name(int weapon_type)
        {
            for (int type = 0; Tables.weapon_table[type].name != null; type++)
                if (weapon_type == Tables.weapon_table[type].type)
                    return Tables.weapon_table[type].name;
            return "exotic";
        }

        public static string affect_bit_name(long vector) => FlagJoin(vector,
            (AFF_BLIND, "blind"), (AFF_INVISIBLE, "invisible"), (AFF_DETECT_EVIL, "detect_evil"),
            (AFF_DETECT_GOOD, "detect_good"), (AFF_DETECT_INVIS, "detect_invis"),
            (AFF_DETECT_MAGIC, "detect_magic"), (AFF_DETECT_HIDDEN, "detect_hidden"),
            (AFF_SANCTUARY, "sanctuary"), (AFF_FAERIE_FIRE, "faerie_fire"), (AFF_INFRARED, "infrared"),
            (AFF_CURSE, "curse"), (AFF_POISON, "poison"), (AFF_PROTECT_EVIL, "prot_evil"),
            (AFF_PROTECT_GOOD, "prot_good"), (AFF_SLEEP, "sleep"), (AFF_SNEAK, "sneak"),
            (AFF_HIDE, "hide"), (AFF_CHARM, "charm"), (AFF_FLYING, "flying"),
            (AFF_PASS_DOOR, "pass_door"), (AFF_BERSERK, "berserk"), (AFF_CALM, "calm"),
            (AFF_HASTE, "haste"), (AFF_SLOW, "slow"), (AFF_PLAGUE, "plague"),
            (AFF_DARK_VISION, "dark_vision"));

        public static string wear_bit_name(long wear_flags) => FlagJoin(wear_flags,
            (ITEM_TAKE, "take"), (ITEM_WEAR_FINGER, "finger"), (ITEM_WEAR_NECK, "neck"),
            (ITEM_WEAR_BODY, "torso"), (ITEM_WEAR_HEAD, "head"), (ITEM_WEAR_LEGS, "legs"),
            (ITEM_WEAR_FEET, "feet"), (ITEM_WEAR_HANDS, "hands"), (ITEM_WEAR_ARMS, "arms"),
            (ITEM_WEAR_SHIELD, "shield"), (ITEM_WEAR_ABOUT, "body"), (ITEM_WEAR_WAIST, "waist"),
            (ITEM_WEAR_WRIST, "wrist"), (ITEM_WIELD, "wield"), (ITEM_HOLD, "hold"),
            (ITEM_NO_SAC, "nosac"), (ITEM_WEAR_FLOAT, "float"));

        public static string act_bit_name(long act_flags)
        {
            if (Bit.IS_SET(act_flags, ACT_IS_NPC))
                return FlagJoin(act_flags,
                    (ACT_IS_NPC, "npc"), (ACT_SENTINEL, "sentinel"), (ACT_SCAVENGER, "scavenger"),
                    (ACT_AGGRESSIVE, "aggressive"), (ACT_STAY_AREA, "stay_area"), (ACT_WIMPY, "wimpy"),
                    (ACT_PET, "pet"), (ACT_TRAIN, "train"), (ACT_PRACTICE, "practice"),
                    (ACT_UNDEAD, "undead"), (ACT_CLERIC, "cleric"), (ACT_MAGE, "mage"),
                    (ACT_THIEF, "thief"), (ACT_WARRIOR, "warrior"), (ACT_NOALIGN, "no_align"),
                    (ACT_NOPURGE, "no_purge"), (ACT_IS_HEALER, "healer"), (ACT_IS_CHANGER, "changer"),
                    (ACT_GAIN, "skill_train"), (ACT_UPDATE_ALWAYS, "update_always"));
            string rest = FlagJoin(act_flags,
                (PLR_AUTOASSIST, "autoassist"), (PLR_AUTOEXIT, "autoexit"),
                (PLR_AUTOLOOT, "autoloot"), (PLR_AUTOSAC, "autosac"), (PLR_AUTOGOLD, "autogold"),
                (PLR_AUTOSPLIT, "autosplit"), (PLR_HOLYLIGHT, "holy_light"), (PLR_CANLOOT, "loot_corpse"),
                (PLR_NOSUMMON, "no_summon"), (PLR_NOFOLLOW, "no_follow"), (PLR_FREEZE, "frozen"),
                (PLR_THIEF, "thief"), (PLR_KILLER, "killer"));
            return rest == "none" ? "player" : "player " + rest;
        }

        public static string comm_bit_name(long comm_flags) => FlagJoin(comm_flags,
            (COMM_QUIET, "quiet"), (COMM_DEAF, "deaf"), (COMM_NOWIZ, "no_wiz"),
            (COMM_NOAUCTION, "no_auction"), (COMM_NOGOSSIP, "no_gossip"),
            (COMM_NOQUESTION, "no_question"), (COMM_NOMUSIC, "no_music"),
            (COMM_NOQUOTE, "no_quote"), (COMM_COMPACT, "compact"), (COMM_BRIEF, "brief"),
            (COMM_PROMPT, "prompt"), (COMM_COMBINE, "combine"), (COMM_NOEMOTE, "no_emote"),
            (COMM_NOSHOUT, "no_shout"), (COMM_NOTELL, "no_tell"), (COMM_NOCHANNELS, "no_channels"));

        public static string off_bit_name(long off_flags) => FlagJoin(off_flags,
            (OFF_AREA_ATTACK, "area attack"), (OFF_BACKSTAB, "backstab"), (OFF_BASH, "bash"),
            (OFF_BERSERK, "berserk"), (OFF_DISARM, "disarm"), (OFF_DODGE, "dodge"),
            (OFF_FADE, "fade"), (OFF_FAST, "fast"), (OFF_KICK, "kick"),
            (OFF_KICK_DIRT, "kick_dirt"), (OFF_PARRY, "parry"), (OFF_RESCUE, "rescue"),
            (OFF_TAIL, "tail"), (OFF_TRIP, "trip"), (OFF_CRUSH, "crush"),
            (ASSIST_ALL, "assist_all"), (ASSIST_ALIGN, "assist_align"), (ASSIST_RACE, "assist_race"),
            (ASSIST_PLAYERS, "assist_players"), (ASSIST_GUARD, "assist_guard"),
            (ASSIST_VNUM, "assist_vnum"));

        public static string form_bit_name(long form_flags)
        {
            string buf = "";
            if ((form_flags & FORM_POISON) != 0) buf += " poison";
            else if ((form_flags & FORM_EDIBLE) != 0) buf += " edible";
            if ((form_flags & FORM_MAGICAL) != 0) buf += " magical";
            if ((form_flags & FORM_INSTANT_DECAY) != 0) buf += " instant_rot";
            if ((form_flags & FORM_OTHER) != 0) buf += " other";
            if ((form_flags & FORM_ANIMAL) != 0) buf += " animal";
            if ((form_flags & FORM_SENTIENT) != 0) buf += " sentient";
            if ((form_flags & FORM_UNDEAD) != 0) buf += " undead";
            if ((form_flags & FORM_CONSTRUCT) != 0) buf += " construct";
            if ((form_flags & FORM_MIST) != 0) buf += " mist";
            if ((form_flags & FORM_INTANGIBLE) != 0) buf += " intangible";
            if ((form_flags & FORM_BIPED) != 0) buf += " biped";
            if ((form_flags & FORM_CENTAUR) != 0) buf += " centaur";
            if ((form_flags & FORM_INSECT) != 0) buf += " insect";
            if ((form_flags & FORM_SPIDER) != 0) buf += " spider";
            if ((form_flags & FORM_CRUSTACEAN) != 0) buf += " crustacean";
            if ((form_flags & FORM_WORM) != 0) buf += " worm";
            if ((form_flags & FORM_BLOB) != 0) buf += " blob";
            if ((form_flags & FORM_MAMMAL) != 0) buf += " mammal";
            if ((form_flags & FORM_BIRD) != 0) buf += " bird";
            if ((form_flags & FORM_REPTILE) != 0) buf += " reptile";
            if ((form_flags & FORM_SNAKE) != 0) buf += " snake";
            if ((form_flags & FORM_DRAGON) != 0) buf += " dragon";
            if ((form_flags & FORM_AMPHIBIAN) != 0) buf += " amphibian";
            if ((form_flags & FORM_FISH) != 0) buf += " fish";
            if ((form_flags & FORM_COLD_BLOOD) != 0) buf += " cold_blooded";
            return buf.Length != 0 ? buf.Substring(1) : "none";
        }

        public static string part_bit_name(long part_flags) => FlagJoin(part_flags,
            (PART_HEAD, "head"), (PART_ARMS, "arms"), (PART_LEGS, "legs"),
            (PART_HEART, "heart"), (PART_BRAINS, "brains"), (PART_GUTS, "guts"),
            (PART_HANDS, "hands"), (PART_FEET, "feet"), (PART_FINGERS, "fingers"),
            (PART_EAR, "ears"), (PART_EYE, "eyes"), (PART_LONG_TONGUE, "long_tongue"),
            (PART_EYESTALKS, "eyestalks"), (PART_TENTACLES, "tentacles"), (PART_FINS, "fins"),
            (PART_WINGS, "wings"), (PART_TAIL, "tail"), (PART_CLAWS, "claws"),
            (PART_FANGS, "fangs"), (PART_HORNS, "horns"), (PART_SCALES, "scales"));

        public static string extra_bit_name(long extra_flags) => FlagJoin(extra_flags,
            (ITEM_GLOW, "glow"), (ITEM_HUM, "hum"), (ITEM_DARK, "dark"), (ITEM_LOCK, "lock"),
            (ITEM_EVIL, "evil"), (ITEM_INVIS, "invis"), (ITEM_MAGIC, "magic"), (ITEM_NODROP, "nodrop"),
            (ITEM_BLESS, "bless"), (ITEM_ANTI_GOOD, "anti-good"), (ITEM_ANTI_EVIL, "anti-evil"),
            (ITEM_ANTI_NEUTRAL, "anti-neutral"), (ITEM_NOREMOVE, "noremove"), (ITEM_INVENTORY, "inventory"),
            (ITEM_NOPURGE, "nopurge"), (ITEM_VIS_DEATH, "vis_death"), (ITEM_ROT_DEATH, "rot_death"),
            (ITEM_NOLOCATE, "no_locate"), (ITEM_SELL_EXTRACT, "sell_extract"),
            (ITEM_BURN_PROOF, "burn_proof"), (ITEM_NOUNCURSE, "no_uncurse"));

        public static string imm_bit_name(long imm_flags) => FlagJoin(imm_flags,
            (IMM_SUMMON, "summon"), (IMM_CHARM, "charm"), (IMM_MAGIC, "magic"), (IMM_WEAPON, "weapon"),
            (IMM_BASH, "blunt"), (IMM_PIERCE, "piercing"), (IMM_SLASH, "slashing"), (IMM_FIRE, "fire"),
            (IMM_COLD, "cold"), (IMM_LIGHTNING, "lightning"), (IMM_ACID, "acid"), (IMM_POISON, "poison"),
            (IMM_NEGATIVE, "negative"), (IMM_HOLY, "holy"), (IMM_ENERGY, "energy"), (IMM_MENTAL, "mental"),
            (IMM_DISEASE, "disease"), (IMM_DROWNING, "drowning"), (IMM_LIGHT, "light"),
            (VULN_IRON, "iron"), (VULN_WOOD, "wood"), (VULN_SILVER, "silver"));

        public static string weapon_bit_name(long weapon_flags) => FlagJoin(weapon_flags,
            (WEAPON_FLAMING, "flaming"), (WEAPON_FROST, "frost"), (WEAPON_VAMPIRIC, "vampiric"),
            (WEAPON_SHARP, "sharp"), (WEAPON_VORPAL, "vorpal"), (WEAPON_TWO_HANDS, "two-handed"),
            (WEAPON_SHOCKING, "shocking"), (WEAPON_POISON, "poison"));

        public static string cont_bit_name(int cont_flags) => FlagJoin(cont_flags,
            (CONT_CLOSEABLE, "closable"), (CONT_PICKPROOF, "pickproof"),
            (CONT_CLOSED, "closed"), (CONT_LOCKED, "locked"));

        public static string affect_loc_name(int location)
        {
            switch (location)
            {
                case APPLY_NONE: return "none";
                case APPLY_STR: return "strength";
                case APPLY_DEX: return "dexterity";
                case APPLY_INT: return "intelligence";
                case APPLY_WIS: return "wisdom";
                case APPLY_CON: return "constitution";
                case APPLY_SEX: return "sex";
                case APPLY_CLASS: return "class";
                case APPLY_LEVEL: return "level";
                case APPLY_AGE: return "age";
                case APPLY_MANA: return "mana";
                case APPLY_HIT: return "hp";
                case APPLY_MOVE: return "moves";
                case APPLY_GOLD: return "gold";
                case APPLY_EXP: return "experience";
                case APPLY_AC: return "armor class";
                case APPLY_HITROLL: return "hit roll";
                case APPLY_DAMROLL: return "damage roll";
                case APPLY_SAVES: return "saves";
                case APPLY_SAVING_ROD: return "save vs rod";
                case APPLY_SAVING_PETRI: return "save vs petrification";
                case APPLY_SAVING_BREATH: return "save vs breath";
                case APPLY_SAVING_SPELL: return "save vs spell";
                case APPLY_SPELL_AFFECT: return "none";
            }

            Db.bug("Affect_location_name: unknown location %d.", location);
            return "(unknown)";
        }

        public static void default_colour(CharData ch)
        {
            if (Bit.IS_NPC(ch))
                return;
            if (ch.pcdata == null)
                return;

            ch.pcdata.text[1] = WHITE;
            ch.pcdata.auction[1] = YELLOW;
            ch.pcdata.auction_text[1] = WHITE;
            ch.pcdata.gossip[1] = MAGENTA;
            ch.pcdata.gossip_text[1] = MAGENTA;
            ch.pcdata.music[1] = RED;
            ch.pcdata.music_text[1] = RED;
            ch.pcdata.question[1] = YELLOW;
            ch.pcdata.question_text[1] = WHITE;
            ch.pcdata.answer[1] = YELLOW;
            ch.pcdata.answer_text[1] = WHITE;
            ch.pcdata.quote[1] = GREEN;
            ch.pcdata.quote_text[1] = GREEN;
            ch.pcdata.immtalk_text[1] = CYAN;
            ch.pcdata.immtalk_type[1] = YELLOW;
            ch.pcdata.info[1] = YELLOW;
            ch.pcdata.tell[1] = GREEN;
            ch.pcdata.tell_text[1] = GREEN;
            ch.pcdata.say[1] = GREEN;
            ch.pcdata.say_text[1] = GREEN;
            ch.pcdata.reply[1] = GREEN;
            ch.pcdata.reply_text[1] = GREEN;
            ch.pcdata.gtell_text[1] = GREEN;
            ch.pcdata.gtell_type[1] = RED;
            ch.pcdata.wiznet[1] = GREEN;
            ch.pcdata.room_title[1] = CYAN;
            ch.pcdata.room_text[1] = WHITE;
            ch.pcdata.room_exits[1] = GREEN;
            ch.pcdata.room_things[1] = CYAN;
            ch.pcdata.prompt[1] = CYAN;
            ch.pcdata.fight_death[1] = RED;
            ch.pcdata.fight_yhit[1] = GREEN;
            ch.pcdata.fight_ohit[1] = YELLOW;
            ch.pcdata.fight_thit[1] = RED;
            ch.pcdata.fight_skill[1] = WHITE;
            ch.pcdata.text[0] = NORMAL;
            ch.pcdata.auction[0] = BRIGHT;
            ch.pcdata.auction_text[0] = BRIGHT;
            ch.pcdata.gossip[0] = NORMAL;
            ch.pcdata.gossip_text[0] = BRIGHT;
            ch.pcdata.music[0] = NORMAL;
            ch.pcdata.music_text[0] = BRIGHT;
            ch.pcdata.question[0] = BRIGHT;
            ch.pcdata.question_text[0] = BRIGHT;
            ch.pcdata.answer[0] = BRIGHT;
            ch.pcdata.answer_text[0] = BRIGHT;
            ch.pcdata.quote[0] = NORMAL;
            ch.pcdata.quote_text[0] = BRIGHT;
            ch.pcdata.immtalk_text[0] = NORMAL;
            ch.pcdata.immtalk_type[0] = NORMAL;
            ch.pcdata.info[0] = NORMAL;
            ch.pcdata.say[0] = NORMAL;
            ch.pcdata.say_text[0] = BRIGHT;
            ch.pcdata.tell[0] = NORMAL;
            ch.pcdata.tell_text[0] = BRIGHT;
            ch.pcdata.reply[0] = NORMAL;
            ch.pcdata.reply_text[0] = BRIGHT;
            ch.pcdata.gtell_text[0] = NORMAL;
            ch.pcdata.gtell_type[0] = NORMAL;
            ch.pcdata.wiznet[0] = NORMAL;
            ch.pcdata.room_title[0] = NORMAL;
            ch.pcdata.room_text[0] = NORMAL;
            ch.pcdata.room_exits[0] = NORMAL;
            ch.pcdata.room_things[0] = NORMAL;
            ch.pcdata.prompt[0] = NORMAL;
            ch.pcdata.fight_death[0] = NORMAL;
            ch.pcdata.fight_yhit[0] = NORMAL;
            ch.pcdata.fight_ohit[0] = NORMAL;
            ch.pcdata.fight_thit[0] = NORMAL;
            ch.pcdata.fight_skill[0] = NORMAL;
            ch.pcdata.text[2] = 0;
            ch.pcdata.auction[2] = 0;
            ch.pcdata.auction_text[2] = 0;
            ch.pcdata.gossip[2] = 0;
            ch.pcdata.gossip_text[2] = 0;
            ch.pcdata.music[2] = 0;
            ch.pcdata.music_text[2] = 0;
            ch.pcdata.question[2] = 0;
            ch.pcdata.question_text[2] = 0;
            ch.pcdata.answer[2] = 0;
            ch.pcdata.answer_text[2] = 0;
            ch.pcdata.quote[2] = 0;
            ch.pcdata.quote_text[2] = 0;
            ch.pcdata.immtalk_text[2] = 0;
            ch.pcdata.immtalk_type[2] = 0;
            ch.pcdata.info[2] = 1;
            ch.pcdata.say[2] = 0;
            ch.pcdata.say_text[2] = 0;
            ch.pcdata.tell[2] = 0;
            ch.pcdata.tell_text[2] = 0;
            ch.pcdata.reply[2] = 0;
            ch.pcdata.reply_text[2] = 0;
            ch.pcdata.gtell_text[2] = 0;
            ch.pcdata.gtell_type[2] = 0;
            ch.pcdata.wiznet[2] = 0;
            ch.pcdata.room_title[2] = 0;
            ch.pcdata.room_text[2] = 0;
            ch.pcdata.room_exits[2] = 0;
            ch.pcdata.room_things[2] = 0;
            ch.pcdata.prompt[2] = 0;
            ch.pcdata.fight_death[2] = 0;
            ch.pcdata.fight_yhit[2] = 0;
            ch.pcdata.fight_ohit[2] = 0;
            ch.pcdata.fight_thit[2] = 0;
            ch.pcdata.fight_skill[2] = 0;
        }

        public static bool alter_colour(CharData ch, int[] type, string argument)
        {
            if (!RomString.str_prefix(argument, "red"))
            {
                type[0] = NORMAL;
                type[1] = RED;
            }
            else if (!RomString.str_prefix(argument, "hi-red"))
            {
                type[0] = BRIGHT;
                type[1] = RED;
            }
            else if (!RomString.str_prefix(argument, "green"))
            {
                type[0] = NORMAL;
                type[1] = GREEN;
            }
            else if (!RomString.str_prefix(argument, "hi-green"))
            {
                type[0] = BRIGHT;
                type[1] = GREEN;
            }
            else if (!RomString.str_prefix(argument, "yellow"))
            {
                type[0] = NORMAL;
                type[1] = YELLOW;
            }
            else if (!RomString.str_prefix(argument, "hi-yellow"))
            {
                type[0] = BRIGHT;
                type[1] = YELLOW;
            }
            else if (!RomString.str_prefix(argument, "blue"))
            {
                type[0] = NORMAL;
                type[1] = BLUE;
            }
            else if (!RomString.str_prefix(argument, "hi-blue"))
            {
                type[0] = BRIGHT;
                type[1] = BLUE;
            }
            else if (!RomString.str_prefix(argument, "magenta"))
            {
                type[0] = NORMAL;
                type[1] = MAGENTA;
            }
            else if (!RomString.str_prefix(argument, "hi-magenta"))
            {
                type[0] = BRIGHT;
                type[1] = MAGENTA;
            }
            else if (!RomString.str_prefix(argument, "cyan"))
            {
                type[0] = NORMAL;
                type[1] = CYAN;
            }
            else if (!RomString.str_prefix(argument, "hi-cyan"))
            {
                type[0] = BRIGHT;
                type[1] = CYAN;
            }
            else if (!RomString.str_prefix(argument, "white"))
            {
                type[0] = NORMAL;
                type[1] = WHITE;
            }
            else if (!RomString.str_prefix(argument, "hi-white"))
            {
                type[0] = BRIGHT;
                type[1] = WHITE;
            }
            else if (!RomString.str_prefix(argument, "grey"))
            {
                type[0] = BRIGHT;
                type[1] = BLACK;
            }
            else if (!RomString.str_prefix(argument, "beep"))
            {
                type[2] = 1;
            }
            else if (!RomString.str_prefix(argument, "nobeep"))
            {
                type[2] = 0;
            }
            else
            {
                Comm.send_to_char_bw("Unrecognised colour, unchanged.\n\r", ch);
                return false;
            }
            return true;
        }

        public static void all_colour(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch) || ch.pcdata == null)
                return;

            if (argument.Length == 0)
                return;

            int colour = 0;
            int bright = 0;
            string buf2 = "";

            if (!RomString.str_prefix(argument, "red"))
            {
                colour = RED;
                bright = NORMAL;
                buf2 = "Red";
            }
            if (!RomString.str_prefix(argument, "hi-red"))
            {
                colour = RED;
                bright = BRIGHT;
                buf2 = "Red";
            }
            else if (!RomString.str_prefix(argument, "green"))
            {
                colour = GREEN;
                bright = NORMAL;
                buf2 = "Green";
            }
            else if (!RomString.str_prefix(argument, "hi-green"))
            {
                colour = GREEN;
                bright = BRIGHT;
                buf2 = "Green";
            }
            else if (!RomString.str_prefix(argument, "yellow"))
            {
                colour = YELLOW;
                bright = NORMAL;
                buf2 = "Yellow";
            }
            else if (!RomString.str_prefix(argument, "hi-yellow"))
            {
                colour = YELLOW;
                bright = BRIGHT;
                buf2 = "Yellow";
            }
            else if (!RomString.str_prefix(argument, "blue"))
            {
                colour = BLUE;
                bright = NORMAL;
                buf2 = "Blue";
            }
            else if (!RomString.str_prefix(argument, "hi-blue"))
            {
                colour = BLUE;
                bright = BRIGHT;
                buf2 = "Blue";
            }
            else if (!RomString.str_prefix(argument, "magenta"))
            {
                colour = MAGENTA;
                bright = NORMAL;
                buf2 = "Magenta";
            }
            else if (!RomString.str_prefix(argument, "hi-magenta"))
            {
                colour = MAGENTA;
                bright = BRIGHT;
                buf2 = "Magenta";
            }
            else if (!RomString.str_prefix(argument, "cyan"))
            {
                colour = CYAN;
                bright = NORMAL;
                buf2 = "Cyan";
            }
            else if (!RomString.str_prefix(argument, "hi-cyan"))
            {
                colour = CYAN;
                bright = BRIGHT;
                buf2 = "Cyan";
            }
            else if (!RomString.str_prefix(argument, "white"))
            {
                colour = WHITE;
                bright = NORMAL;
                buf2 = "White";
            }
            else if (!RomString.str_prefix(argument, "hi-white"))
            {
                colour = WHITE;
                bright = BRIGHT;
                buf2 = "White";
            }
            else if (!RomString.str_prefix(argument, "grey"))
            {
                colour = BLACK;
                bright = BRIGHT;
                buf2 = "White";
            }
            else
            {
                Comm.send_to_char_bw("Unrecognised colour, unchanged.\n\r", ch);
                return;
            }

            ch.pcdata.text[1] = colour;
            ch.pcdata.auction[1] = colour;
            ch.pcdata.gossip[1] = colour;
            ch.pcdata.music[1] = colour;
            ch.pcdata.question[1] = colour;
            ch.pcdata.answer[1] = colour;
            ch.pcdata.quote[1] = colour;
            ch.pcdata.quote_text[1] = colour;
            ch.pcdata.immtalk_text[1] = colour;
            ch.pcdata.immtalk_type[1] = colour;
            ch.pcdata.info[1] = colour;
            ch.pcdata.say[1] = colour;
            ch.pcdata.say_text[1] = colour;
            ch.pcdata.tell[1] = colour;
            ch.pcdata.tell_text[1] = colour;
            ch.pcdata.reply[1] = colour;
            ch.pcdata.reply_text[1] = colour;
            ch.pcdata.gtell_text[1] = colour;
            ch.pcdata.gtell_type[1] = colour;
            ch.pcdata.wiznet[1] = colour;
            ch.pcdata.room_title[1] = colour;
            ch.pcdata.room_text[1] = colour;
            ch.pcdata.room_exits[1] = colour;
            ch.pcdata.room_things[1] = colour;
            ch.pcdata.prompt[1] = colour;
            ch.pcdata.fight_death[1] = colour;
            ch.pcdata.fight_yhit[1] = colour;
            ch.pcdata.fight_ohit[1] = colour;
            ch.pcdata.fight_thit[1] = colour;
            ch.pcdata.fight_skill[1] = colour;
            ch.pcdata.text[0] = bright;
            ch.pcdata.auction[0] = bright;
            ch.pcdata.gossip[0] = bright;
            ch.pcdata.music[0] = bright;
            ch.pcdata.question[0] = bright;
            ch.pcdata.answer[0] = bright;
            ch.pcdata.quote[0] = bright;
            ch.pcdata.quote_text[0] = bright;
            ch.pcdata.immtalk_text[0] = bright;
            ch.pcdata.immtalk_type[0] = bright;
            ch.pcdata.info[0] = bright;
            ch.pcdata.say[0] = bright;
            ch.pcdata.say_text[0] = bright;
            ch.pcdata.tell[0] = bright;
            ch.pcdata.tell_text[0] = bright;
            ch.pcdata.reply[0] = bright;
            ch.pcdata.reply_text[0] = bright;
            ch.pcdata.gtell_text[0] = bright;
            ch.pcdata.gtell_type[0] = bright;
            ch.pcdata.wiznet[0] = bright;
            ch.pcdata.room_title[0] = bright;
            ch.pcdata.room_text[0] = bright;
            ch.pcdata.room_exits[0] = bright;
            ch.pcdata.room_things[0] = bright;
            ch.pcdata.prompt[0] = bright;
            ch.pcdata.fight_death[0] = bright;
            ch.pcdata.fight_yhit[0] = bright;
            ch.pcdata.fight_ohit[0] = bright;
            ch.pcdata.fight_thit[0] = bright;
            ch.pcdata.fight_skill[0] = bright;

            Comm.send_to_char_bw(RomString.sprintf("All Colour settings set to %s.\n\r", buf2), ch);
        }

        public static void deduct_cost(CharData ch, int cost)
        {
            int silver = 0, gold = 0;

            silver = Bit.UMIN((int)ch.silver, cost);

            if (silver < cost)
            {
                gold = ((cost - silver + 99) / 100);
                silver = cost - 100 * gold;
            }

            ch.gold -= gold;
            ch.silver -= silver;

            if (ch.gold < 0)
            {
                Db.bug("deduct costs: gold %d < 0", (int)ch.gold);
                ch.gold = 0;
            }
            if (ch.silver < 0)
            {
                Db.bug("deduct costs: silver %d < 0", (int)ch.silver);
                ch.silver = 0;
            }
            Gmcp.Worth(ch);
        }
    }
}
