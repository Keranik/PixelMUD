using static Rom24.Merc;

namespace Rom24
{
    public static class Fight
    {
        public static void violence_update()
        {
            CharData ch_next;
            for (var ch = Game.char_list; ch != null; ch = ch_next)
            {
                ch_next = ch.next;
                var victim = ch.fighting;
                if (victim == null || ch.in_room == null)
                    continue;
                if (Bit.IS_AWAKE(ch) && ch.in_room == victim.in_room)
                    multi_hit(ch, victim, TYPE_UNDEFINED);
                else
                    stop_fighting(ch, false);
                if ((victim = ch.fighting) == null)
                    continue;
                check_assist(ch, victim);

                if (Bit.IS_NPC(ch))
                {
                    if (Bit.HAS_TRIGGER(ch, TRIG_FIGHT))
                        MobProg.mp_percent_trigger(ch, victim, null, null, (int)TRIG_FIGHT);
                    if (Bit.HAS_TRIGGER(ch, TRIG_HPCNT))
                        MobProg.mp_hprct_trigger(ch, victim);
                }
            }
        }

        public static void check_assist(CharData ch, CharData victim)
        {
            CharData rch_next;
            for (var rch = ch.in_room.people; rch != null; rch = rch_next)
            {
                rch_next = rch.next_in_room;

                if (Bit.IS_AWAKE(rch) && rch.fighting == null)
                {
                    /* quick check for ASSIST_PLAYER */
                    if (!Bit.IS_NPC(ch) && Bit.IS_NPC(rch)
                        && Bit.IS_SET(rch.off_flags, ASSIST_PLAYERS)
                        && rch.level + 6 > victim.level)
                    {
                        Interp.do_function(rch, ActComm.do_emote, "screams and attacks!");
                        multi_hit(rch, victim, TYPE_UNDEFINED);
                        continue;
                    }

                    /* PCs next */
                    if (!Bit.IS_NPC(ch) || Bit.IS_AFFECTED(ch, AFF_CHARM))
                    {
                        if (((!Bit.IS_NPC(rch) && Bit.IS_SET(rch.act, PLR_AUTOASSIST))
                             || Bit.IS_AFFECTED(rch, AFF_CHARM))
                            && Handler.is_same_group(ch, rch) && !is_safe(rch, victim))
                            multi_hit(rch, victim, TYPE_UNDEFINED);

                        continue;
                    }

                    /* now check the NPC cases */

                    if (Bit.IS_NPC(ch) && !Bit.IS_AFFECTED(ch, AFF_CHARM))
                    {
                        if ((Bit.IS_NPC(rch) && Bit.IS_SET(rch.off_flags, ASSIST_ALL))
                            || (Bit.IS_NPC(rch) && rch.group != 0 && rch.group == ch.group)
                            || (Bit.IS_NPC(rch) && rch.race == ch.race
                                && Bit.IS_SET(rch.off_flags, ASSIST_RACE))
                            || (Bit.IS_NPC(rch) && Bit.IS_SET(rch.off_flags, ASSIST_ALIGN)
                                && ((Bit.IS_GOOD(rch) && Bit.IS_GOOD(ch))
                                    || (Bit.IS_EVIL(rch) && Bit.IS_EVIL(ch))
                                    || (Bit.IS_NEUTRAL(rch) && Bit.IS_NEUTRAL(ch))))
                            || (rch.pIndexData == ch.pIndexData
                                && Bit.IS_SET(rch.off_flags, ASSIST_VNUM)))
                        {
                            if (RomRandom.number_bits(1) == 0)
                                continue;

                            CharData target = null;
                            int number = 0;
                            for (var vch = ch.in_room.people; vch != null; vch = vch.next)
                            {
                                if (Handler.can_see(rch, vch)
                                    && Handler.is_same_group(vch, victim)
                                    && RomRandom.number_range(0, number) == 0)
                                {
                                    target = vch;
                                    number++;
                                }
                            }

                            if (target != null)
                            {
                                Interp.do_function(rch, ActComm.do_emote, "screams and attacks!");
                                multi_hit(rch, target, TYPE_UNDEFINED);
                            }
                        }
                    }
                }
            }
        }

        public static void multi_hit(CharData ch, CharData victim, int dt)
        {
            if (ch.desc == null)
                ch.wait = Bit.UMAX(0, ch.wait - PULSE_VIOLENCE);
            if (ch.desc == null)
                ch.daze = Bit.UMAX(0, ch.daze - PULSE_VIOLENCE);
            if (ch.position < POS_RESTING)
                return;
            if (Bit.IS_NPC(ch))
            {
                mob_hit(ch, victim, dt);
                return;
            }
            one_hit(ch, victim, dt);
            if (ch.fighting != victim)
                return;
            if (Bit.IS_AFFECTED(ch, AFF_HASTE))
                one_hit(ch, victim, dt);
            if (ch.fighting != victim || dt == Gsn.backstab)
                return;
            int chance = Handler.get_skill(ch, Gsn.second_attack) / 2;
            if (Bit.IS_AFFECTED(ch, AFF_SLOW))
                chance /= 2;
            if (RomRandom.number_percent() < chance)
            {
                one_hit(ch, victim, dt);
                Skills.check_improve(ch, Gsn.second_attack, true, 5);
                if (ch.fighting != victim)
                    return;
            }
            chance = Handler.get_skill(ch, Gsn.third_attack) / 4;
            if (Bit.IS_AFFECTED(ch, AFF_SLOW))
                chance = 0;
            if (RomRandom.number_percent() < chance)
            {
                one_hit(ch, victim, dt);
                Skills.check_improve(ch, Gsn.third_attack, true, 6);
            }
        }

        public static void mob_hit(CharData ch, CharData victim, int dt)
        {
            one_hit(ch, victim, dt);
            if (ch.fighting != victim)
                return;
            if (Bit.IS_SET(ch.off_flags, OFF_AREA_ATTACK))
            {
                for (var vch = ch.in_room.people; vch != null; )
                {
                    var vch_next = vch.next;
                    if (vch != victim && vch.fighting == ch)
                        one_hit(ch, vch, dt);
                    vch = vch_next;
                }
            }
            if (Bit.IS_AFFECTED(ch, AFF_HASTE)
                || (Bit.IS_SET(ch.off_flags, OFF_FAST) && !Bit.IS_AFFECTED(ch, AFF_SLOW)))
                one_hit(ch, victim, dt);
            if (ch.fighting != victim || dt == Gsn.backstab)
                return;
            int chance = Handler.get_skill(ch, Gsn.second_attack) / 2;
            if (Bit.IS_AFFECTED(ch, AFF_SLOW) && !Bit.IS_SET(ch.off_flags, OFF_FAST))
                chance /= 2;
            if (RomRandom.number_percent() < chance)
            {
                one_hit(ch, victim, dt);
                if (ch.fighting != victim)
                    return;
            }
            chance = Handler.get_skill(ch, Gsn.third_attack) / 4;
            if (Bit.IS_AFFECTED(ch, AFF_SLOW) && !Bit.IS_SET(ch.off_flags, OFF_FAST))
                chance = 0;
            if (RomRandom.number_percent() < chance)
                one_hit(ch, victim, dt);
        }

        public static void one_hit(CharData ch, CharData victim, int dt)
        {
            if (victim == ch || ch == null || victim == null)
                return;
            if (victim.position == POS_DEAD || ch.in_room != victim.in_room)
                return;
            var wield = Handler.get_eq_char(ch, WEAR_WIELD);
            if (dt == TYPE_UNDEFINED)
            {
                dt = TYPE_HIT;
                if (wield != null && wield.item_type == ITEM_WEAPON)
                    dt += wield.value[3];
                else
                    dt += ch.dam_type;
            }
            int dam_type;
            if (dt < TYPE_HIT)
            {
                if (wield != null)
                    dam_type = Tables.attack_table[wield.value[3]].damage;
                else
                    dam_type = Tables.attack_table[ch.dam_type].damage;
            }
            else
                dam_type = Tables.attack_table[dt - TYPE_HIT].damage;
            if (dam_type == -1)
                dam_type = DAM_BASH;
            int sn = Handler.get_weapon_sn(ch);
            int skill = 20 + Handler.get_weapon_skill(ch, sn);
            int thac0_00, thac0_32;
            if (Bit.IS_NPC(ch))
            {
                thac0_00 = 20;
                thac0_32 = -4;
                if (Bit.IS_SET(ch.act, ACT_WARRIOR)) thac0_32 = -10;
                else if (Bit.IS_SET(ch.act, ACT_THIEF)) thac0_32 = -4;
                else if (Bit.IS_SET(ch.act, ACT_CLERIC)) thac0_32 = 2;
                else if (Bit.IS_SET(ch.act, ACT_MAGE)) thac0_32 = 6;
            }
            else
            {
                thac0_00 = Tables.class_table[ch.klass].thac0_00;
                thac0_32 = Tables.class_table[ch.klass].thac0_32;
            }
            int thac0 = RomRandom.interpolate(ch.level, thac0_00, thac0_32);
            if (thac0 < 0) thac0 = thac0 / 2;
            if (thac0 < -5) thac0 = -5 + (thac0 + 5) / 2;
            thac0 -= Handler.GET_HITROLL(ch) * skill / 100;
            thac0 += 5 * (100 - skill) / 100;
            if (dt == Gsn.backstab)
                thac0 -= 10 * (100 - Handler.get_skill(ch, Gsn.backstab));
            int victim_ac = dam_type switch
            {
                DAM_PIERCE => Handler.GET_AC(victim, AC_PIERCE) / 10,
                DAM_BASH => Handler.GET_AC(victim, AC_BASH) / 10,
                DAM_SLASH => Handler.GET_AC(victim, AC_SLASH) / 10,
                _ => Handler.GET_AC(victim, AC_EXOTIC) / 10
            };
            if (victim_ac < -15)
                victim_ac = (victim_ac + 15) / 5 - 15;
            if (!Handler.can_see(ch, victim))
                victim_ac -= 4;
            if (victim.position < POS_FIGHTING)
                victim_ac += 4;
            if (victim.position < POS_RESTING)
                victim_ac += 6;
            int diceroll;
            while ((diceroll = RomRandom.number_bits(5)) >= 20) { }
            if (diceroll == 0 || (diceroll != 19 && diceroll < thac0 - victim_ac))
            {
                damage(ch, victim, 0, dt, dam_type, true);
                return;
            }
            int dam;
            if (Bit.IS_NPC(ch) && (ch.pIndexData == null || !ch.pIndexData.new_format || wield == null))
            {
                if (ch.pIndexData == null || !ch.pIndexData.new_format)
                {
                    dam = RomRandom.number_range(ch.level / 2, ch.level * 3 / 2);
                    if (wield != null) dam += dam / 2;
                }
                else
                    dam = RomRandom.dice(ch.damage[DICE_NUMBER], ch.damage[DICE_TYPE]);
            }
            else
            {
                if (sn != -1)
                    Skills.check_improve(ch, sn, true, 5);
                if (wield != null)
                {
                    if (wield.pIndexData != null && wield.pIndexData.new_format)
                        dam = RomRandom.dice(wield.value[1], wield.value[2]) * skill / 100;
                    else
                        dam = RomRandom.number_range(wield.value[1] * skill / 100, wield.value[2] * skill / 100);
                    if (Handler.get_eq_char(ch, WEAR_SHIELD) == null)
                        dam = dam * 11 / 10;
                    if (Bit.IS_WEAPON_STAT(wield, WEAPON_SHARP))
                    {
                        int percent = RomRandom.number_percent();
                        if (percent <= (skill / 8))
                            dam = 2 * dam + (dam * 2 * percent / 100);
                    }
                }
                else
                    dam = RomRandom.number_range(1 + 4 * skill / 100, 2 * ch.level / 3 * skill / 100);
            }
            if (Handler.get_skill(ch, Gsn.enhanced_damage) > 0)
            {
                diceroll = RomRandom.number_percent();
                if (diceroll <= Handler.get_skill(ch, Gsn.enhanced_damage))
                {
                    Skills.check_improve(ch, Gsn.enhanced_damage, true, 6);
                    dam += 2 * (dam * diceroll / 300);
                }
            }
            if (!Bit.IS_AWAKE(victim))
                dam *= 2;
            else if (victim.position < POS_FIGHTING)
                dam = dam * 3 / 2;
            if (dt == Gsn.backstab && wield != null)
            {
                if (wield.value[0] != 2)
                    dam *= 2 + (ch.level / 10);
                else
                    dam *= 2 + (ch.level / 8);
            }
            dam += Handler.GET_DAMROLL(ch) * Bit.UMIN(100, skill) / 100;
            if (dam <= 0) dam = 1;
            bool result = damage(ch, victim, dam, dt, dam_type, true);

            if (result && wield != null)
            {
                if (ch.fighting == victim && Bit.IS_WEAPON_STAT(wield, WEAPON_POISON))
                {
                    int level;
                    var poison = Handler.affect_find(wield.affected, Gsn.poison);
                    if (poison == null)
                        level = wield.level;
                    else
                        level = poison.level;

                    if (!Magic.saves_spell(level / 2, victim, DAM_POISON))
                    {
                        Comm.send_to_char("You feel poison coursing through your veins.",
                            victim);
                        Comm.act("$n is poisoned by the venom on $p.",
                            victim, wield, null, TO_ROOM);

                        var af = new AffectData();
                        af.where = TO_AFFECTS;
                        af.type = Gsn.poison;
                        af.level = level * 3 / 4;
                        af.duration = level / 2;
                        af.location = APPLY_STR;
                        af.modifier = -1;
                        af.bitvector = AFF_POISON;
                        Handler.affect_join(victim, af);
                    }

                    if (poison != null)
                    {
                        poison.level = Bit.UMAX(0, poison.level - 2);
                        poison.duration = Bit.UMAX(0, poison.duration - 1);

                        if (poison.level == 0 || poison.duration == 0)
                            Comm.act("The poison on $p has worn off.", ch, wield, null,
                                TO_CHAR);
                    }
                }

                if (ch.fighting == victim && Bit.IS_WEAPON_STAT(wield, WEAPON_VAMPIRIC))
                {
                    dam = RomRandom.number_range(1, wield.level / 5 + 1);
                    Comm.act("$p draws life from $n.", victim, wield, null, TO_ROOM);
                    Comm.act("You feel $p drawing your life away.",
                        victim, wield, null, TO_CHAR);
                    damage(ch, victim, dam, 0, DAM_NEGATIVE, false);
                    ch.alignment = Bit.UMAX(-1000, ch.alignment - 1);
                    ch.hit += dam / 2;
                }

                if (ch.fighting == victim && Bit.IS_WEAPON_STAT(wield, WEAPON_FLAMING))
                {
                    dam = RomRandom.number_range(1, wield.level / 4 + 1);
                    Comm.act("$n is burned by $p.", victim, wield, null, TO_ROOM);
                    Comm.act("$p sears your flesh.", victim, wield, null, TO_CHAR);
                    Effects.fire_effect(victim, wield.level / 2, dam, TARGET_CHAR);
                    damage(ch, victim, dam, 0, DAM_FIRE, false);
                }

                if (ch.fighting == victim && Bit.IS_WEAPON_STAT(wield, WEAPON_FROST))
                {
                    dam = RomRandom.number_range(1, wield.level / 6 + 2);
                    Comm.act("$p freezes $n.", victim, wield, null, TO_ROOM);
                    Comm.act("The cold touch of $p surrounds you with ice.",
                        victim, wield, null, TO_CHAR);
                    Effects.cold_effect(victim, wield.level / 2, dam, TARGET_CHAR);
                    damage(ch, victim, dam, 0, DAM_COLD, false);
                }

                if (ch.fighting == victim && Bit.IS_WEAPON_STAT(wield, WEAPON_SHOCKING))
                {
                    dam = RomRandom.number_range(1, wield.level / 5 + 2);
                    Comm.act("$n is struck by lightning from $p.", victim, wield, null,
                        TO_ROOM);
                    Comm.act("You are shocked by $p.", victim, wield, null, TO_CHAR);
                    Effects.shock_effect(victim, wield.level / 2, dam, TARGET_CHAR);
                    damage(ch, victim, dam, 0, DAM_LIGHTNING, false);
                }
            }
        }

        public static bool damage(CharData ch, CharData victim, int dam, int dt, int dam_type, bool show)
        {
            bool immune = false;
            if (victim.position == POS_DEAD)
                return false;

            if (dam > 1200 && dt >= TYPE_HIT)
            {
                Db.bug("Damage: %d: more than 1200 points!", dam);
                dam = 1200;
                if (!Bit.IS_IMMORTAL(ch))
                {
                    var obj = Handler.get_eq_char(ch, WEAR_WIELD);
                    Comm.send_to_char("You really shouldn't cheat.\n\r", ch);
                    if (obj != null)
                        Handler.extract_obj(obj);
                }
            }

            if (dam > 35)
                dam = (dam - 35) / 2 + 35;
            if (dam > 80)
                dam = (dam - 80) / 2 + 80;

            if (victim != ch)
            {
                if (is_safe(ch, victim))
                    return false;
                check_killer(ch, victim);
                if (victim.position > POS_STUNNED)
                {
                    if (victim.fighting == null)
                    {
                        set_fighting(victim, ch);
                        if (Bit.IS_NPC(victim) && Bit.HAS_TRIGGER(victim, TRIG_KILL))
                            MobProg.mp_percent_trigger(victim, ch, null, null, (int)TRIG_KILL);
                    }
                    if (victim.timer <= 4)
                        victim.position = POS_FIGHTING;
                }
                if (victim.position > POS_STUNNED)
                {
                    if (ch.fighting == null)
                        set_fighting(ch, victim);
                }
                if (victim.master == ch)
                    ActComm.stop_follower(victim);
            }

            if (Bit.IS_AFFECTED(ch, AFF_INVISIBLE))
            {
                Handler.affect_strip(ch, Gsn.invis);
                Handler.affect_strip(ch, Gsn.mass_invis);
                Bit.REMOVE_BIT(ref ch.affected_by, AFF_INVISIBLE);
                Comm.act("$n fades into existence.", ch, null, null, TO_ROOM);
            }

            if (dam > 1 && !Bit.IS_NPC(victim)
                && victim.pcdata.condition[COND_DRUNK] > 10)
                dam = 9 * dam / 10;

            if (dam > 1 && Bit.IS_AFFECTED(victim, AFF_SANCTUARY))
                dam /= 2;
            if (dam > 1 && ((Bit.IS_AFFECTED(victim, AFF_PROTECT_EVIL) && Bit.IS_EVIL(ch))
                            || (Bit.IS_AFFECTED(victim, AFF_PROTECT_GOOD) && Bit.IS_GOOD(ch))))
                dam -= dam / 4;
            if (dt >= TYPE_HIT && ch != victim)
            {
                if (check_parry(ch, victim)) return false;
                if (check_dodge(ch, victim)) return false;
                if (check_shield_block(ch, victim)) return false;
            }
            switch (Handler.check_immune(victim, dam_type))
            {
                case IS_IMMUNE:
                    immune = true;
                    dam = 0;
                    break;
                case IS_RESISTANT:
                    dam -= dam / 3;
                    break;
                case IS_VULNERABLE:
                    dam += dam / 2;
                    break;
            }
            if (show)
                dam_message(ch, victim, dam, dt, immune);
            if (dam == 0)
                return false;
            victim.hit -= dam;
            if (!Bit.IS_NPC(victim)
                && victim.level >= LEVEL_IMMORTAL && victim.hit < 1)
                victim.hit = 1;
            update_pos(victim);

            switch (victim.position)
            {
                case POS_MORTAL:
                    Comm.act("$n is mortally wounded, and will die soon, if not aided.",
                        victim, null, null, TO_ROOM);
                    Comm.send_to_char(
                        "You are mortally wounded, and will die soon, if not aided.\n\r",
                        victim);
                    break;

                case POS_INCAP:
                    Comm.act("$n is incapacitated and will slowly die, if not aided.",
                        victim, null, null, TO_ROOM);
                    Comm.send_to_char(
                        "You are incapacitated and will slowly die, if not aided.\n\r",
                        victim);
                    break;

                case POS_STUNNED:
                    Comm.act("$n is stunned, but will probably recover.",
                        victim, null, null, TO_ROOM);
                    Comm.send_to_char("You are stunned, but will probably recover.\n\r",
                        victim);
                    break;

                case POS_DEAD:
                    Comm.act("{R$n is DEAD!!{x", victim, null, null, TO_ROOM);
                    Comm.send_to_char("{RYou have been KILLED!!{x\n\r\n\r", victim);
                    break;

                default:
                    if (dam > victim.max_hit / 4)
                        Comm.send_to_char("{RThat really did HURT!{x\n\r", victim);
                    if (victim.hit < victim.max_hit / 4)
                        Comm.send_to_char("{RYou sure are BLEEDING!{x\n\r", victim);
                    break;
            }

            if (!Bit.IS_AWAKE(victim))
                stop_fighting(victim, false);

            if (victim.position == POS_DEAD)
            {
                group_gain(ch, victim);

                if (!Bit.IS_NPC(victim))
                {
                    Db.log_string($"{victim.name} killed by {(Bit.IS_NPC(ch) ? ch.short_descr : ch.name)} at {ch.in_room.vnum}");

                    if (victim.exp > Handler.exp_per_level(victim, victim.pcdata.points)
                        * victim.level)
                        Update.gain_exp(victim,
                            (2 *
                             (Handler.exp_per_level(victim, victim.pcdata.points) *
                              victim.level - victim.exp) / 3) + 50);
                }

                string log_buf = $"{(Bit.IS_NPC(victim) ? victim.short_descr : victim.name)} got toasted by {(Bit.IS_NPC(ch) ? ch.short_descr : ch.name)} at {ch.in_room.name} [room {ch.in_room.vnum}]";

                if (Bit.IS_NPC(victim))
                    Comm.wiznet(log_buf, null, null, WIZ_MOBDEATHS, 0, 0);
                else
                    Comm.wiznet(log_buf, null, null, WIZ_DEATHS, 0, 0);

                /*
                 * Death trigger
                 */
                if (Bit.IS_NPC(victim) && Bit.HAS_TRIGGER(victim, TRIG_DEATH))
                {
                    victim.position = POS_STANDING;
                    MobProg.mp_percent_trigger(victim, ch, null, null, (int)TRIG_DEATH);
                }

                raw_kill(victim);

                if (ch != victim && !Bit.IS_NPC(ch) && !Handler.is_same_clan(ch, victim))
                {
                    if (Bit.IS_SET(victim.act, PLR_KILLER))
                        Bit.REMOVE_BIT(ref victim.act, PLR_KILLER);
                    else
                        Bit.REMOVE_BIT(ref victim.act, PLR_THIEF);
                }

                if (!Bit.IS_NPC(ch)
                    && Handler.get_obj_list(ch, "corpse", ch.in_room.contents) is ObjData corpse
                    && corpse.item_type == ITEM_CORPSE_NPC
                    && Handler.can_see_obj(ch, corpse))
                {
                    corpse = Handler.get_obj_list(ch, "corpse", ch.in_room.contents);

                    if (Bit.IS_SET(ch.act, PLR_AUTOLOOT) && corpse != null && corpse.contains != null)
                    {
                        Interp.do_function(ch, ActObj.do_get, "all corpse");
                    }

                    if (Bit.IS_SET(ch.act, PLR_AUTOGOLD) && corpse != null && corpse.contains != null &&
                        !Bit.IS_SET(ch.act, PLR_AUTOLOOT))
                    {
                        if (Handler.get_obj_list(ch, "gcash", corpse.contains) != null)
                        {
                            Interp.do_function(ch, ActObj.do_get, "all.gcash corpse");
                        }
                    }

                    if (Bit.IS_SET(ch.act, PLR_AUTOSAC))
                    {
                        if (Bit.IS_SET(ch.act, PLR_AUTOLOOT) && corpse != null
                            && corpse.contains != null)
                        {
                            return true;
                        }
                        else
                        {
                            Interp.do_function(ch, ActObj.do_sacrifice, "corpse");
                        }
                    }
                }

                return true;
            }

            if (victim == ch)
                return true;

            if (!Bit.IS_NPC(victim) && victim.desc == null)
            {
                if (RomRandom.number_range(0, victim.wait) == 0)
                {
                    Interp.do_function(victim, ActMove.do_recall, "");
                    return true;
                }
            }

            if (Bit.IS_NPC(victim) && dam > 0 && victim.wait < PULSE_VIOLENCE / 2)
            {
                if ((Bit.IS_SET(victim.act, ACT_WIMPY) && RomRandom.number_bits(2) == 0
                     && victim.hit < victim.max_hit / 5)
                    || (Bit.IS_AFFECTED(victim, AFF_CHARM) && victim.master != null
                        && victim.master.in_room != victim.in_room))
                {
                    Interp.do_function(victim, do_flee, "");
                }
            }

            if (!Bit.IS_NPC(victim)
                && victim.hit > 0
                && victim.hit <= victim.wimpy && victim.wait < PULSE_VIOLENCE / 2)
            {
                Interp.do_function(victim, do_flee, "");
            }

            return true;
        }

        public static bool is_safe_spell(CharData ch, CharData victim, bool area)
        {
            if (victim.in_room == null || ch.in_room == null)
                return true;

            if (victim == ch && area)
                return true;

            if (victim.fighting == ch || victim == ch)
                return false;

            if (Bit.IS_IMMORTAL(ch) && ch.level > LEVEL_IMMORTAL && !area)
                return false;

            if (Bit.IS_NPC(victim))
            {
                if (Bit.IS_SET(victim.in_room.room_flags, ROOM_SAFE))
                    return true;

                if (victim.pIndexData?.pShop != null)
                    return true;

                if (Bit.IS_SET(victim.act, ACT_TRAIN)
                    || Bit.IS_SET(victim.act, ACT_PRACTICE)
                    || Bit.IS_SET(victim.act, ACT_IS_HEALER)
                    || Bit.IS_SET(victim.act, ACT_IS_CHANGER))
                    return true;

                if (!Bit.IS_NPC(ch))
                {
                    if (Bit.IS_SET(victim.act, ACT_PET))
                        return true;

                    if (Bit.IS_AFFECTED(victim, AFF_CHARM)
                        && (area || ch != victim.master))
                        return true;

                    if (victim.fighting != null
                        && !Handler.is_same_group(ch, victim.fighting)) return true;
                }
                else
                {
                    if (area && !Handler.is_same_group(victim, ch.fighting))
                        return true;
                }
            }
            else
            {
                if (area && Bit.IS_IMMORTAL(victim) && victim.level > LEVEL_IMMORTAL)
                    return true;

                if (Bit.IS_NPC(ch))
                {
                    if (Bit.IS_AFFECTED(ch, AFF_CHARM) && ch.master != null
                        && ch.master.fighting != victim)
                        return true;

                    if (Bit.IS_SET(victim.in_room.room_flags, ROOM_SAFE))
                        return true;

                    if (ch.fighting != null && !Handler.is_same_group(ch.fighting, victim))
                        return true;
                }
                else
                {
                    if (!Handler.is_clan(ch))
                        return true;

                    if (Bit.IS_SET(victim.act, PLR_KILLER)
                        || Bit.IS_SET(victim.act, PLR_THIEF))
                        return false;

                    if (!Handler.is_clan(victim))
                        return true;

                    if (ch.level > victim.level + 8)
                        return true;
                }
            }
            return false;
        }

        public static bool is_safe(CharData ch, CharData victim)
        {
            if (victim.in_room == null || ch.in_room == null)
                return true;

            if (victim.fighting == ch || victim == ch)
                return false;

            if (Bit.IS_IMMORTAL(ch) && ch.level > LEVEL_IMMORTAL)
                return false;

            if (Bit.IS_NPC(victim))
            {
                if (Bit.IS_SET(victim.in_room.room_flags, ROOM_SAFE))
                {
                    Comm.send_to_char("Not in this room.\n\r", ch);
                    return true;
                }

                if (victim.pIndexData.pShop != null)
                {
                    Comm.send_to_char("The shopkeeper wouldn't like that.\n\r", ch);
                    return true;
                }

                if (Bit.IS_SET(victim.act, ACT_TRAIN)
                    || Bit.IS_SET(victim.act, ACT_PRACTICE)
                    || Bit.IS_SET(victim.act, ACT_IS_HEALER)
                    || Bit.IS_SET(victim.act, ACT_IS_CHANGER))
                {
                    Comm.send_to_char("I don't think Mota would approve.\n\r", ch);
                    return true;
                }

                if (!Bit.IS_NPC(ch))
                {
                    if (Bit.IS_SET(victim.act, ACT_PET))
                    {
                        Comm.act("But $N looks so cute and cuddly...",
                            ch, null, victim, TO_CHAR);
                        return true;
                    }

                    if (Bit.IS_AFFECTED(victim, AFF_CHARM) && ch != victim.master)
                    {
                        Comm.send_to_char("You don't own that monster.\n\r", ch);
                        return true;
                    }
                }
            }
            else
            {
                if (Bit.IS_NPC(ch))
                {
                    if (Bit.IS_SET(victim.in_room.room_flags, ROOM_SAFE))
                    {
                        Comm.send_to_char("Not in this room.\n\r", ch);
                        return true;
                    }

                    if (Bit.IS_AFFECTED(ch, AFF_CHARM) && ch.master != null
                        && ch.master.fighting != victim)
                    {
                        Comm.send_to_char("Players are your friends!\n\r", ch);
                        return true;
                    }
                }
                else
                {
                    if (!Handler.is_clan(ch))
                    {
                        Comm.send_to_char("Join a clan if you want to kill players.\n\r",
                            ch);
                        return true;
                    }

                    if (Bit.IS_SET(victim.act, PLR_KILLER)
                        || Bit.IS_SET(victim.act, PLR_THIEF))
                        return false;

                    if (!Handler.is_clan(victim))
                    {
                        Comm.send_to_char("They aren't in a clan, leave them alone.\n\r",
                            ch);
                        return true;
                    }

                    if (ch.level > victim.level + 8)
                    {
                        Comm.send_to_char("Pick on someone your own size.\n\r", ch);
                        return true;
                    }
                }
            }
            return false;
        }

        public static void check_killer(CharData ch, CharData victim)
        {
            while (Bit.IS_AFFECTED(victim, AFF_CHARM) && victim.master != null)
                victim = victim.master;

            if (Bit.IS_NPC(victim)
                || Bit.IS_SET(victim.act, PLR_KILLER)
                || Bit.IS_SET(victim.act, PLR_THIEF))
                return;

            if (Bit.IS_SET(ch.affected_by, AFF_CHARM))
            {
                if (ch.master == null)
                {
                    Db.bug(RomString.sprintf("Check_killer: %s bad AFF_CHARM",
                        Bit.IS_NPC(ch) ? ch.short_descr : ch.name), 0);
                    Handler.affect_strip(ch, Gsn.charm_person);
                    Bit.REMOVE_BIT(ref ch.affected_by, AFF_CHARM);
                    return;
                }
                ActComm.stop_follower(ch);
                return;
            }

            if (Bit.IS_NPC(ch)
                || ch == victim || ch.level >= LEVEL_IMMORTAL || !Handler.is_clan(ch)
                || Bit.IS_SET(ch.act, PLR_KILLER) || ch.fighting == victim)
                return;

            Comm.send_to_char("*** You are now a KILLER!! ***\n\r", ch);
            Bit.SET_BIT(ref ch.act, PLR_KILLER);
            Comm.wiznet(RomString.sprintf("$N is attempting to murder %s", victim.name),
                ch, null, WIZ_FLAGS, 0, 0);
            Save.save_char_obj(ch);
        }

        public static bool check_parry(CharData ch, CharData victim)
        {
            if (!Bit.IS_AWAKE(victim)) return false;
            int chance = Handler.get_skill(victim, Gsn.parry) / 2;
            if (Handler.get_eq_char(victim, WEAR_WIELD) == null)
            {
                if (Bit.IS_NPC(victim)) chance /= 2;
                else return false;
            }
            if (!Handler.can_see(ch, victim)) chance /= 2;
            if (RomRandom.number_percent() >= chance + victim.level - ch.level)
                return false;
            Comm.act("You parry $n's attack.", ch, null, victim, TO_VICT);
            Comm.act("$N parries your attack.", ch, null, victim, TO_CHAR);
            Skills.check_improve(victim, Gsn.parry, true, 6);
            return true;
        }

        public static bool check_shield_block(CharData ch, CharData victim)
        {
            if (!Bit.IS_AWAKE(victim)) return false;
            int chance = Handler.get_skill(victim, Gsn.shield_block) / 5 + 3;
            if (Handler.get_eq_char(victim, WEAR_SHIELD) == null)
                return false;
            if (RomRandom.number_percent() >= chance + victim.level - ch.level)
                return false;
            Comm.act("You block $n's attack with your shield.", ch, null, victim, TO_VICT);
            Comm.act("$N blocks your attack with a shield.", ch, null, victim, TO_CHAR);
            Skills.check_improve(victim, Gsn.shield_block, true, 6);
            return true;
        }

        public static bool check_dodge(CharData ch, CharData victim)
        {
            if (!Bit.IS_AWAKE(victim)) return false;
            int chance = Handler.get_skill(victim, Gsn.dodge) / 2;
            if (!Handler.can_see(victim, ch)) chance /= 2;
            if (RomRandom.number_percent() >= chance + victim.level - ch.level)
                return false;
            Comm.act("You dodge $n's attack.", ch, null, victim, TO_VICT);
            Comm.act("$N dodges your attack.", ch, null, victim, TO_CHAR);
            Skills.check_improve(victim, Gsn.dodge, true, 6);
            return true;
        }

        public static void update_pos(CharData victim)
        {
            if (victim.hit > 0)
            {
                if (victim.position <= POS_STUNNED)
                    victim.position = POS_STANDING;
                return;
            }
            if (Bit.IS_NPC(victim) && victim.hit < 1)
            {
                victim.position = POS_DEAD;
                return;
            }
            if (victim.hit <= -11) victim.position = POS_DEAD;
            else if (victim.hit <= -6) victim.position = POS_MORTAL;
            else if (victim.hit <= -3) victim.position = POS_INCAP;
            else victim.position = POS_STUNNED;
        }

        public static void set_fighting(CharData ch, CharData victim)
        {
            if (ch.fighting != null)
            {
                Db.bug("Set_fighting: already fighting", 0);
                return;
            }
            ch.fighting = victim;
            ch.position = POS_FIGHTING;
        }

        public static void stop_fighting(CharData ch, bool fBoth)
        {
            for (var fch = Game.char_list; fch != null; fch = fch.next)
            {
                if (fch == ch || (fBoth && fch.fighting == ch))
                {
                    fch.fighting = null;
                    fch.position = Bit.IS_NPC(fch) ? fch.default_pos : POS_STANDING;
                    update_pos(fch);
                }
            }
        }

        public static void make_corpse(CharData ch)
        {
            string name;
            ObjData corpse;

            if (Bit.IS_NPC(ch))
            {
                name = ch.short_descr;
                corpse = Db.create_object(Handler.get_obj_index(OBJ_VNUM_CORPSE_NPC), 0);
                corpse.timer = RomRandom.number_range(3, 6);
                if (ch.gold > 0)
                {
                    Handler.obj_to_obj(Handler.create_money((int)ch.gold, (int)ch.silver), corpse);
                    ch.gold = 0;
                    ch.silver = 0;
                }
                corpse.cost = 0;
            }
            else
            {
                name = ch.name;
                corpse = Db.create_object(Handler.get_obj_index(OBJ_VNUM_CORPSE_PC), 0);
                corpse.timer = RomRandom.number_range(25, 40);
                Bit.REMOVE_BIT(ref ch.act, PLR_CANLOOT);
                if (!Handler.is_clan(ch))
                    corpse.owner = ch.name;
                else
                {
                    corpse.owner = null;
                    if (ch.gold > 1 || ch.silver > 1)
                    {
                        Handler.obj_to_obj(Handler.create_money((int)(ch.gold / 2), (int)(ch.silver / 2)),
                            corpse);
                        ch.gold -= ch.gold / 2;
                        ch.silver -= ch.silver / 2;
                    }
                }

                corpse.cost = 0;
            }

            corpse.level = ch.level;

            corpse.short_descr = RomString.sprintf(corpse.short_descr, name);
            corpse.description = RomString.sprintf(corpse.description, name);

            ObjData obj_next;
            for (var obj = ch.carrying; obj != null; obj = obj_next)
            {
                bool floating = false;

                obj_next = obj.next_content;
                if (obj.wear_loc == WEAR_FLOAT)
                    floating = true;
                Handler.obj_from_char(obj);
                if (obj.item_type == ITEM_POTION)
                    obj.timer = RomRandom.number_range(500, 1000);
                if (obj.item_type == ITEM_SCROLL)
                    obj.timer = RomRandom.number_range(1000, 2500);
                if (Bit.IS_SET(obj.extra_flags, ITEM_ROT_DEATH) && !floating)
                {
                    obj.timer = RomRandom.number_range(5, 10);
                    Bit.REMOVE_BIT(ref obj.extra_flags, ITEM_ROT_DEATH);
                }
                Bit.REMOVE_BIT(ref obj.extra_flags, ITEM_VIS_DEATH);

                if (Bit.IS_SET(obj.extra_flags, ITEM_INVENTORY))
                    Handler.extract_obj(obj);
                else if (floating)
                {
                    if (Bit.IS_OBJ_STAT(obj, ITEM_ROT_DEATH))
                    {
                        if (obj.contains != null)
                        {
                            Comm.act("$p evaporates,scattering its contents.",
                                ch, obj, null, TO_ROOM);
                            ObjData in_next;
                            for (var in_obj = obj.contains; in_obj != null; in_obj = in_next)
                            {
                                in_next = in_obj.next_content;
                                Handler.obj_from_obj(in_obj);
                                Handler.obj_to_room(in_obj, ch.in_room);
                            }
                        }
                        else
                            Comm.act("$p evaporates.", ch, obj, null, TO_ROOM);
                        Handler.extract_obj(obj);
                    }
                    else
                    {
                        Comm.act("$p falls to the floor.", ch, obj, null, TO_ROOM);
                        Handler.obj_to_room(obj, ch.in_room);
                    }
                }
                else
                    Handler.obj_to_obj(obj, corpse);
            }

            Handler.obj_to_room(corpse, ch.in_room);
        }

        public static void death_cry(CharData ch)
        {
            int vnum = 0;
            string msg = "You hear $n's death cry.";

            switch (RomRandom.number_bits(4))
            {
                case 0:
                    msg = "$n hits the ground ... DEAD.";
                    break;
                case 1:
                    if (ch.material == null)
                    {
                        msg = "$n splatters blood on your armor.";
                        break;
                    }
                    goto case 2;
                case 2:
                    if (Bit.IS_SET(ch.parts, PART_GUTS))
                    {
                        msg = "$n spills $s guts all over the floor.";
                        vnum = OBJ_VNUM_GUTS;
                    }
                    break;
                case 3:
                    if (Bit.IS_SET(ch.parts, PART_HEAD))
                    {
                        msg = "$n's severed head plops on the ground.";
                        vnum = OBJ_VNUM_SEVERED_HEAD;
                    }
                    break;
                case 4:
                    if (Bit.IS_SET(ch.parts, PART_HEART))
                    {
                        msg = "$n's heart is torn from $s chest.";
                        vnum = OBJ_VNUM_TORN_HEART;
                    }
                    break;
                case 5:
                    if (Bit.IS_SET(ch.parts, PART_ARMS))
                    {
                        msg = "$n's arm is sliced from $s dead body.";
                        vnum = OBJ_VNUM_SLICED_ARM;
                    }
                    break;
                case 6:
                    if (Bit.IS_SET(ch.parts, PART_LEGS))
                    {
                        msg = "$n's leg is sliced from $s dead body.";
                        vnum = OBJ_VNUM_SLICED_LEG;
                    }
                    break;
                case 7:
                    if (Bit.IS_SET(ch.parts, PART_BRAINS))
                    {
                        msg =
                            "$n's head is shattered, and $s brains splash all over you.";
                        vnum = OBJ_VNUM_BRAINS;
                    }
                    break;
            }

            Comm.act(msg, ch, null, null, TO_ROOM);

            if (vnum != 0)
            {
                string name = Bit.IS_NPC(ch) ? ch.short_descr : ch.name;
                var obj = Db.create_object(Handler.get_obj_index(vnum), 0);
                obj.timer = RomRandom.number_range(4, 7);

                obj.short_descr = RomString.sprintf(obj.short_descr, name);
                obj.description = RomString.sprintf(obj.description, name);

                if (obj.item_type == ITEM_FOOD)
                {
                    if (Bit.IS_SET(ch.form, FORM_POISON))
                        obj.value[3] = 1;
                    else if (!Bit.IS_SET(ch.form, FORM_EDIBLE))
                        obj.item_type = ITEM_TRASH;
                }

                Handler.obj_to_room(obj, ch.in_room);
            }

            if (Bit.IS_NPC(ch))
                msg = "You hear something's death cry.";
            else
                msg = "You hear someone's death cry.";

            var was_in_room = ch.in_room;
            for (int door = 0; door <= 5; door++)
            {
                var pexit = was_in_room.exit[door];

                if (pexit != null
                    && pexit.to_room != null && pexit.to_room != was_in_room)
                {
                    ch.in_room = pexit.to_room;
                    Comm.act(msg, ch, null, null, TO_ROOM);
                }
            }
            ch.in_room = was_in_room;
        }

        public static void raw_kill(CharData victim)
        {
            stop_fighting(victim, true);
            death_cry(victim);
            make_corpse(victim);

            if (Bit.IS_NPC(victim))
            {
                victim.pIndexData.killed++;
                Game.kill_table[Bit.URANGE(0, victim.level, MAX_LEVEL - 1)].killed++;
                Handler.extract_char(victim, true);
                return;
            }

            Handler.extract_char(victim, false);
            while (victim.affected != null)
                Handler.affect_remove(victim, victim.affected);
            victim.affected_by = Tables.race_table[victim.race].aff;
            for (int i = 0; i < 4; i++)
                victim.armor[i] = 100;
            victim.position = POS_RESTING;
            victim.hit = Bit.UMAX(1, victim.hit);
            victim.mana = Bit.UMAX(1, victim.mana);
            victim.move = Bit.UMAX(1, victim.move);
        }

        public static void group_gain(CharData ch, CharData victim)
        {
            if (victim == ch)
                return;

            int members = 0;
            int group_levels = 0;
            for (var gch = ch.in_room.people; gch != null; gch = gch.next_in_room)
            {
                if (Handler.is_same_group(gch, ch))
                {
                    members++;
                    group_levels += Bit.IS_NPC(gch) ? gch.level / 2 : gch.level;
                }
            }

            if (members == 0)
            {
                Db.bug("Group_gain: members.", members);
                members = 1;
                group_levels = ch.level;
            }

            for (var gch = ch.in_room.people; gch != null; gch = gch.next_in_room)
            {
                if (!Handler.is_same_group(gch, ch) || Bit.IS_NPC(gch))
                    continue;

                int xp = xp_compute(gch, victim, group_levels);
                Comm.send_to_char($"You receive {xp} experience points.\n\r", gch);
                Update.gain_exp(gch, xp);

                ObjData obj_next;
                for (var obj = ch.carrying; obj != null; obj = obj_next)
                {
                    obj_next = obj.next_content;
                    if (obj.wear_loc == WEAR_NONE)
                        continue;

                    if ((Bit.IS_OBJ_STAT(obj, ITEM_ANTI_EVIL) && Bit.IS_EVIL(ch))
                        || (Bit.IS_OBJ_STAT(obj, ITEM_ANTI_GOOD) && Bit.IS_GOOD(ch))
                        || (Bit.IS_OBJ_STAT(obj, ITEM_ANTI_NEUTRAL) && Bit.IS_NEUTRAL(ch)))
                    {
                        Comm.act("You are zapped by $p.", ch, obj, null, TO_CHAR);
                        Comm.act("$n is zapped by $p.", ch, obj, null, TO_ROOM);
                        Handler.obj_from_char(obj);
                        Handler.obj_to_room(obj, ch.in_room);
                    }
                }
            }
        }

        public static int xp_compute(CharData gch, CharData victim, int total_levels)
        {
            int level_range = victim.level - gch.level;

            int base_exp;
            switch (level_range)
            {
                default:
                    base_exp = 0;
                    break;
                case -9:
                    base_exp = 1;
                    break;
                case -8:
                    base_exp = 2;
                    break;
                case -7:
                    base_exp = 5;
                    break;
                case -6:
                    base_exp = 9;
                    break;
                case -5:
                    base_exp = 11;
                    break;
                case -4:
                    base_exp = 22;
                    break;
                case -3:
                    base_exp = 33;
                    break;
                case -2:
                    base_exp = 50;
                    break;
                case -1:
                    base_exp = 66;
                    break;
                case 0:
                    base_exp = 83;
                    break;
                case 1:
                    base_exp = 99;
                    break;
                case 2:
                    base_exp = 121;
                    break;
                case 3:
                    base_exp = 143;
                    break;
                case 4:
                    base_exp = 165;
                    break;
            }

            if (level_range > 4)
                base_exp = 160 + 20 * (level_range - 4);

            int align = victim.alignment - gch.alignment;
            int change;

            if (Bit.IS_SET(victim.act, ACT_NOALIGN))
            {
            }
            else if (align > 500)
            {
                change = (align - 500) * base_exp / 500 * gch.level / total_levels;
                change = Bit.UMAX(1, change);
                gch.alignment = Bit.UMAX(-1000, gch.alignment - change);
            }
            else if (align < -500)
            {
                change =
                    (-1 * align - 500) * base_exp / 500 * gch.level / total_levels;
                change = Bit.UMAX(1, change);
                gch.alignment = Bit.UMIN(1000, gch.alignment + change);
            }
            else
            {
                change = gch.alignment * base_exp / 500 * gch.level / total_levels;
                gch.alignment -= change;
            }

            int xp;
            if (Bit.IS_SET(victim.act, ACT_NOALIGN))
                xp = base_exp;
            else if (gch.alignment > 500)
            {
                if (victim.alignment < -750)
                    xp = (base_exp * 4) / 3;
                else if (victim.alignment < -500)
                    xp = (base_exp * 5) / 4;
                else if (victim.alignment > 750)
                    xp = base_exp / 4;
                else if (victim.alignment > 500)
                    xp = base_exp / 2;
                else if (victim.alignment > 250)
                    xp = (base_exp * 3) / 4;
                else
                    xp = base_exp;
            }
            else if (gch.alignment < -500)
            {
                if (victim.alignment > 750)
                    xp = (base_exp * 5) / 4;
                else if (victim.alignment > 500)
                    xp = (base_exp * 11) / 10;
                else if (victim.alignment < -750)
                    xp = base_exp / 2;
                else if (victim.alignment < -500)
                    xp = (base_exp * 3) / 4;
                else if (victim.alignment < -250)
                    xp = (base_exp * 9) / 10;
                else
                    xp = base_exp;
            }
            else if (gch.alignment > 200)
            {
                if (victim.alignment < -500)
                    xp = (base_exp * 6) / 5;
                else if (victim.alignment > 750)
                    xp = base_exp / 2;
                else if (victim.alignment > 0)
                    xp = (base_exp * 3) / 4;
                else
                    xp = base_exp;
            }
            else if (gch.alignment < -200)
            {
                if (victim.alignment > 500)
                    xp = (base_exp * 6) / 5;
                else if (victim.alignment < -750)
                    xp = base_exp / 2;
                else if (victim.alignment < 0)
                    xp = (base_exp * 3) / 4;
                else
                    xp = base_exp;
            }
            else
            {
                if (victim.alignment > 500 || victim.alignment < -500)
                    xp = (base_exp * 4) / 3;
                else if (victim.alignment < 200 && victim.alignment > -200)
                    xp = base_exp / 2;
                else
                    xp = base_exp;
            }

            if (gch.level < 6)
                xp = 10 * xp / (gch.level + 4);

            if (gch.level > 35)
                xp = 15 * xp / (gch.level - 25);

            {
                int time_per_level = 4 *
                    (gch.played + (int)(Game.current_time - gch.logon)) / 3600
                    / gch.level;

                time_per_level = Bit.URANGE(2, time_per_level, 12);
                if (gch.level < 15)
                    time_per_level = Bit.UMAX(time_per_level, (15 - gch.level));
                xp = xp * time_per_level / 12;
            }

            xp = RomRandom.number_range(xp * 3 / 4, xp * 5 / 4);

            xp = xp * gch.level / (Bit.UMAX(1, total_levels - 1));

            return xp;
        }

        public static void dam_message(CharData ch, CharData victim, int dam, int dt, bool immune)
        {
            if (ch == null || victim == null) return;
            int dam_percent = victim.max_hit <= 0 ? 100 : (100 * dam) / victim.max_hit;
            string vs, vp;
            if (dam == 0) { vs = "miss"; vp = "misses"; }
            else if (dam_percent <= 5) { vs = "scratch"; vp = "scratches"; }
            else if (dam_percent <= 10) { vs = "graze"; vp = "grazes"; }
            else if (dam_percent <= 15) { vs = "hit"; vp = "hits"; }
            else if (dam_percent <= 20) { vs = "injure"; vp = "injures"; }
            else if (dam_percent <= 25) { vs = "wound"; vp = "wounds"; }
            else if (dam_percent <= 30) { vs = "maul"; vp = "mauls"; }
            else if (dam_percent <= 35) { vs = "decimate"; vp = "decimates"; }
            else if (dam_percent <= 40) { vs = "devastate"; vp = "devastates"; }
            else if (dam_percent <= 45) { vs = "maim"; vp = "maims"; }
            else if (dam_percent <= 50) { vs = "MUTILATE"; vp = "MUTILATES"; }
            else if (dam_percent <= 55) { vs = "DISEMBOWEL"; vp = "DISEMBOWELS"; }
            else if (dam_percent <= 60) { vs = "DISMEMBER"; vp = "DISMEMBERS"; }
            else if (dam_percent <= 65) { vs = "MASSACRE"; vp = "MASSACRES"; }
            else if (dam_percent <= 70) { vs = "MANGLE"; vp = "MANGLES"; }
            else if (dam_percent <= 75) { vs = "*** DEMOLISH ***"; vp = "*** DEMOLISHES ***"; }
            else if (dam_percent <= 80) { vs = "*** DEVASTATE ***"; vp = "*** DEVASTATES ***"; }
            else if (dam_percent <= 85) { vs = "=== OBLITERATE ==="; vp = "=== OBLITERATES ==="; }
            else if (dam_percent <= 90) { vs = ">>> ANNIHILATE <<<"; vp = ">>> ANNIHILATES <<<"; }
            else if (dam_percent <= 95) { vs = "<<< ERADICATE >>>"; vp = "<<< ERADICATES >>>"; }
            else { vs = "do UNSPEAKABLE things to"; vp = "does UNSPEAKABLE things to"; }
            char punct = dam_percent <= 45 ? '.' : '!';
            string buf1, buf2, buf3 = "";
            if (dt == TYPE_HIT)
            {
                if (ch == victim)
                {
                    buf1 = $"{{3$n {vp} $melf{punct}{{x";
                    buf2 = $"{{2You {vs} yourself{punct}{{x";
                }
                else
                {
                    buf1 = $"{{3$n {vp} $N{punct}{{x";
                    buf2 = $"{{2You {vs} $N{punct}{{x";
                    buf3 = $"{{4$n {vp} you{punct}{{x";
                }
            }
            else
            {
                string attack;
                if (dt >= 0 && dt < MAX_SKILL)
                    attack = Tables.skill_table[dt].noun_damage;
                else if (dt >= TYPE_HIT && dt < TYPE_HIT + MAX_DAMAGE_MESSAGE)
                    attack = Tables.attack_table[dt - TYPE_HIT].noun;
                else
                    attack = Tables.attack_table[0].name;
                if (immune)
                {
                    buf1 = $"{{3$N is unaffected by $n's {attack}!{{x";
                    buf2 = $"{{2$N is unaffected by your {attack}!{{x";
                    buf3 = $"{{4$n's {attack} is powerless against you.{{x";
                }
                else if (ch == victim)
                {
                    buf1 = $"{{3$n's {attack} {vp} $m{punct}{{x";
                    buf2 = $"{{2Your {attack} {vp} you{punct}{{x";
                }
                else
                {
                    buf1 = $"{{3$n's {attack} {vp} $N{punct}{{x";
                    buf2 = $"{{2Your {attack} {vp} $N{punct}{{x";
                    buf3 = $"{{4$n's {attack} {vp} you{punct}{{x";
                }
            }
            if (ch == victim)
            {
                Comm.act(buf1, ch, null, null, TO_ROOM);
                Comm.act(buf2, ch, null, null, TO_CHAR);
            }
            else
            {
                Comm.act(buf1, ch, null, victim, TO_NOTVICT);
                Comm.act(buf2, ch, null, victim, TO_CHAR);
                Comm.act(buf3, ch, null, victim, TO_VICT);
            }
        }

        public static void do_kill(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                Comm.send_to_char("Kill whom?\n\r", ch);
                return;
            }
            var victim = Handler.get_char_room(ch, arg);
            if (victim == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }
            if (victim == ch)
            {
                Comm.send_to_char("You hit yourself.  Ouch!\n\r", ch);
                multi_hit(ch, ch, TYPE_UNDEFINED);
                return;
            }
            if (is_safe(ch, victim))
                return;
            if (victim.fighting != null && !Handler.is_same_group(ch, victim.fighting))
            {
                Comm.send_to_char("Kill stealing is not permitted.\n\r", ch);
                return;
            }
            if (Bit.IS_AFFECTED(ch, AFF_CHARM) && ch.master == victim)
            {
                Comm.act("$N is your beloved master.", ch, null, victim, TO_CHAR);
                return;
            }
            if (ch.position == POS_FIGHTING)
            {
                Comm.send_to_char("You do the best you can!\n\r", ch);
                return;
            }
            Bit.WAIT_STATE(ch, 1 * PULSE_VIOLENCE);
            check_killer(ch, victim);
            multi_hit(ch, victim, TYPE_UNDEFINED);
        }

        public static void do_flee(CharData ch, string argument)
        {
            var victim = ch.fighting;
            if (victim == null)
            {
                if (ch.position == POS_FIGHTING)
                    ch.position = POS_STANDING;
                Comm.send_to_char("You aren't fighting anyone.\n\r", ch);
                return;
            }

            var was_in = ch.in_room;
            for (int attempt = 0; attempt < 6; attempt++)
            {
                int door = RomRandom.number_door();
                var pexit = was_in.exit[door];
                if (pexit == null
                    || pexit.to_room == null
                    || Bit.IS_SET(pexit.exit_info, EX_CLOSED)
                    || RomRandom.number_range(0, ch.daze) != 0 || (Bit.IS_NPC(ch)
                                                                   && Bit.IS_SET(pexit.to_room.room_flags,
                                                                       ROOM_NO_MOB)))
                    continue;

                ActMove.move_char(ch, door, false);
                var now_in = ch.in_room;
                if (now_in == was_in)
                    continue;

                ch.in_room = was_in;
                Comm.act("$n has fled!", ch, null, null, TO_ROOM);
                ch.in_room = now_in;

                if (!Bit.IS_NPC(ch))
                {
                    Comm.send_to_char("You flee from combat!\n\r", ch);
                    if ((ch.klass == 2) && (RomRandom.number_percent() < 3 * (ch.level / 2)))
                        Comm.send_to_char("You snuck away safely.\n\r", ch);
                    else
                    {
                        Comm.send_to_char("You lost 10 exp.\n\r", ch);
                        Update.gain_exp(ch, -10);
                    }
                }

                stop_fighting(ch, true);
                return;
            }

            Comm.send_to_char("PANIC! You couldn't escape!\n\r", ch);
        }

        public static void disarm(CharData ch, CharData victim)
        {
            var obj = Handler.get_eq_char(victim, WEAR_WIELD);
            if (obj == null)
                return;

            if (Bit.IS_OBJ_STAT(obj, ITEM_NOREMOVE))
            {
                Comm.act("{5$S weapon won't budge!{x", ch, null, victim, TO_CHAR);
                Comm.act("{5$n tries to disarm you, but your weapon won't budge!{x",
                    ch, null, victim, TO_VICT);
                Comm.act("{5$n tries to disarm $N, but fails.{x", ch, null, victim,
                    TO_NOTVICT);
                return;
            }

            Comm.act("{5$n DISARMS you and sends your weapon flying!{x",
                ch, null, victim, TO_VICT);
            Comm.act("{5You disarm $N!{x", ch, null, victim, TO_CHAR);
            Comm.act("{5$n disarms $N!{x", ch, null, victim, TO_NOTVICT);

            Handler.obj_from_char(obj);
            if (Bit.IS_OBJ_STAT(obj, ITEM_NODROP) || Bit.IS_OBJ_STAT(obj, ITEM_INVENTORY))
                Handler.obj_to_char(obj, victim);
            else
            {
                Handler.obj_to_room(obj, victim.in_room);
                if (Bit.IS_NPC(victim) && victim.wait == 0 && Handler.can_see_obj(victim, obj))
                    ActObj.get_obj(victim, obj, null);
            }
        }

        public static void do_berserk(CharData ch, string argument)
        {
            int chance, hp_percent;

            if ((chance = Handler.get_skill(ch, Gsn.berserk)) == 0
                || (Bit.IS_NPC(ch) && !Bit.IS_SET(ch.off_flags, OFF_BERSERK))
                || (!Bit.IS_NPC(ch)
                    && ch.level < Tables.skill_table[Gsn.berserk].skill_level[ch.klass]))
            {
                Comm.send_to_char("You turn red in the face, but nothing happens.\n\r",
                    ch);
                return;
            }

            if (Bit.IS_AFFECTED(ch, AFF_BERSERK) || Handler.is_affected(ch, Gsn.berserk)
                || Handler.is_affected(ch, Lookup.skill_lookup("frenzy")))
            {
                Comm.send_to_char("You get a little madder.\n\r", ch);
                return;
            }

            if (Bit.IS_AFFECTED(ch, AFF_CALM))
            {
                Comm.send_to_char("You're feeling to mellow to berserk.\n\r", ch);
                return;
            }

            if (ch.mana < 50)
            {
                Comm.send_to_char("You can't get up enough energy.\n\r", ch);
                return;
            }

            if (ch.position == POS_FIGHTING)
                chance += 10;

            hp_percent = 100 * ch.hit / ch.max_hit;
            chance += 25 - hp_percent / 2;

            if (RomRandom.number_percent() < chance)
            {
                var af = new AffectData();

                Bit.WAIT_STATE(ch, PULSE_VIOLENCE);
                ch.mana -= 50;
                ch.move /= 2;

                ch.hit += ch.level * 2;
                ch.hit = Bit.UMIN(ch.hit, ch.max_hit);

                Comm.send_to_char("Your pulse races as you are consumed by rage!\n\r",
                    ch);
                Comm.act("$n gets a wild look in $s eyes.", ch, null, null, TO_ROOM);
                Skills.check_improve(ch, Gsn.berserk, true, 2);

                af.where = TO_AFFECTS;
                af.type = Gsn.berserk;
                af.level = ch.level;
                af.duration = RomRandom.number_fuzzy(ch.level / 8);
                af.modifier = Bit.UMAX(1, ch.level / 5);
                af.bitvector = AFF_BERSERK;

                af.location = APPLY_HITROLL;
                Handler.affect_to_char(ch, af);

                af.location = APPLY_DAMROLL;
                Handler.affect_to_char(ch, af);

                af.modifier = Bit.UMAX(10, 10 * (ch.level / 5));
                af.location = APPLY_AC;
                Handler.affect_to_char(ch, af);
            }
            else
            {
                Bit.WAIT_STATE(ch, 3 * PULSE_VIOLENCE);
                ch.mana -= 25;
                ch.move /= 2;

                Comm.send_to_char("Your pulse speeds up, but nothing happens.\n\r", ch);
                Skills.check_improve(ch, Gsn.berserk, false, 2);
            }
        }

        public static void do_bash(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);
            CharData victim;
            int chance;

            if ((chance = Handler.get_skill(ch, Gsn.bash)) == 0
                || (Bit.IS_NPC(ch) && !Bit.IS_SET(ch.off_flags, OFF_BASH))
                || (!Bit.IS_NPC(ch)
                    && ch.level < Tables.skill_table[Gsn.bash].skill_level[ch.klass]))
            {
                Comm.send_to_char("Bashing? What's that?\n\r", ch);
                return;
            }

            if (arg.Length == 0)
            {
                victim = ch.fighting;
                if (victim == null)
                {
                    Comm.send_to_char("But you aren't fighting anyone!\n\r", ch);
                    return;
                }
            }
            else if ((victim = Handler.get_char_room(ch, arg)) == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }

            if (victim.position < POS_FIGHTING)
            {
                Comm.act("You'll have to let $M get back up first.", ch, null, victim,
                    TO_CHAR);
                return;
            }

            if (victim == ch)
            {
                Comm.send_to_char("You try to bash your brains out, but fail.\n\r", ch);
                return;
            }

            if (is_safe(ch, victim))
                return;

            if (Bit.IS_NPC(victim) &&
                victim.fighting != null && !Handler.is_same_group(ch, victim.fighting))
            {
                Comm.send_to_char("Kill stealing is not permitted.\n\r", ch);
                return;
            }

            if (Bit.IS_AFFECTED(ch, AFF_CHARM) && ch.master == victim)
            {
                Comm.act("But $N is your friend!", ch, null, victim, TO_CHAR);
                return;
            }

            chance += ch.carry_weight / 250;
            chance -= victim.carry_weight / 200;

            if (ch.size < victim.size)
                chance += (ch.size - victim.size) * 15;
            else
                chance += (ch.size - victim.size) * 10;

            chance += Handler.get_curr_stat(ch, STAT_STR);
            chance -= (Handler.get_curr_stat(victim, STAT_DEX) * 4) / 3;
            chance -= Handler.GET_AC(victim, AC_BASH) / 25;
            if (Bit.IS_SET(ch.off_flags, OFF_FAST) || Bit.IS_AFFECTED(ch, AFF_HASTE))
                chance += 10;
            if (Bit.IS_SET(victim.off_flags, OFF_FAST)
                || Bit.IS_AFFECTED(victim, AFF_HASTE))
                chance -= 30;

            chance += (ch.level - victim.level);

            if (!Bit.IS_NPC(victim) && chance < Handler.get_skill(victim, Gsn.dodge))
            {
                chance -= 3 * (Handler.get_skill(victim, Gsn.dodge) - chance);
            }

            if (RomRandom.number_percent() < chance)
            {
                Comm.act("{5$n sends you sprawling with a powerful bash!{x",
                    ch, null, victim, TO_VICT);
                Comm.act("{5You slam into $N, and send $M flying!{x", ch, null, victim,
                    TO_CHAR);
                Comm.act("{5$n sends $N sprawling with a powerful bash.{x", ch, null,
                    victim, TO_NOTVICT);
                Skills.check_improve(ch, Gsn.bash, true, 1);

                Bit.DAZE_STATE(victim, 3 * PULSE_VIOLENCE);
                Bit.WAIT_STATE(ch, Tables.skill_table[Gsn.bash].beats);
                victim.position = POS_RESTING;
                damage(ch, victim, RomRandom.number_range(2, 2 + 2 * ch.size + chance / 20),
                    Gsn.bash, DAM_BASH, false);
            }
            else
            {
                damage(ch, victim, 0, Gsn.bash, DAM_BASH, false);
                Comm.act("{5You fall flat on your face!{x", ch, null, victim, TO_CHAR);
                Comm.act("{5$n falls flat on $s face.{x", ch, null, victim, TO_NOTVICT);
                Comm.act("{5You evade $n's bash, causing $m to fall flat on $s face.{x",
                    ch, null, victim, TO_VICT);
                Skills.check_improve(ch, Gsn.bash, false, 1);
                ch.position = POS_RESTING;
                Bit.WAIT_STATE(ch, Tables.skill_table[Gsn.bash].beats * 3 / 2);
            }
            check_killer(ch, victim);
        }

        public static void do_dirt(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);
            CharData victim;
            int chance;

            if ((chance = Handler.get_skill(ch, Gsn.dirt)) == 0
                || (Bit.IS_NPC(ch) && !Bit.IS_SET(ch.off_flags, OFF_KICK_DIRT))
                || (!Bit.IS_NPC(ch)
                    && ch.level < Tables.skill_table[Gsn.dirt].skill_level[ch.klass]))
            {
                Comm.send_to_char("You get your feet dirty.\n\r", ch);
                return;
            }

            if (arg.Length == 0)
            {
                victim = ch.fighting;
                if (victim == null)
                {
                    Comm.send_to_char("But you aren't in combat!\n\r", ch);
                    return;
                }
            }
            else if ((victim = Handler.get_char_room(ch, arg)) == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }

            if (Bit.IS_AFFECTED(victim, AFF_BLIND))
            {
                Comm.act("$E's already been blinded.", ch, null, victim, TO_CHAR);
                return;
            }

            if (victim == ch)
            {
                Comm.send_to_char("Very funny.\n\r", ch);
                return;
            }

            if (is_safe(ch, victim))
                return;

            if (Bit.IS_NPC(victim) &&
                victim.fighting != null && !Handler.is_same_group(ch, victim.fighting))
            {
                Comm.send_to_char("Kill stealing is not permitted.\n\r", ch);
                return;
            }

            if (Bit.IS_AFFECTED(ch, AFF_CHARM) && ch.master == victim)
            {
                Comm.act("But $N is such a good friend!", ch, null, victim, TO_CHAR);
                return;
            }

            chance += Handler.get_curr_stat(ch, STAT_DEX);
            chance -= 2 * Handler.get_curr_stat(victim, STAT_DEX);

            if (Bit.IS_SET(ch.off_flags, OFF_FAST) || Bit.IS_AFFECTED(ch, AFF_HASTE))
                chance += 10;
            if (Bit.IS_SET(victim.off_flags, OFF_FAST)
                || Bit.IS_AFFECTED(victim, AFF_HASTE))
                chance -= 25;

            chance += (ch.level - victim.level) * 2;

            if (chance % 5 == 0)
                chance += 1;

            switch (ch.in_room.sector_type)
            {
                case SECT_INSIDE:
                    chance -= 20;
                    break;
                case SECT_CITY:
                    chance -= 10;
                    break;
                case SECT_FIELD:
                    chance += 5;
                    break;
                case SECT_FOREST:
                    break;
                case SECT_HILLS:
                    break;
                case SECT_MOUNTAIN:
                    chance -= 10;
                    break;
                case SECT_WATER_SWIM:
                    chance = 0;
                    break;
                case SECT_WATER_NOSWIM:
                    chance = 0;
                    break;
                case SECT_AIR:
                    chance = 0;
                    break;
                case SECT_DESERT:
                    chance += 10;
                    break;
            }

            if (chance == 0)
            {
                Comm.send_to_char("There isn't any dirt to kick.\n\r", ch);
                return;
            }

            if (RomRandom.number_percent() < chance)
            {
                var af = new AffectData();
                Comm.act("{5$n is blinded by the dirt in $s eyes!{x", victim, null, null,
                    TO_ROOM);
                Comm.act("{5$n kicks dirt in your eyes!{x", ch, null, victim, TO_VICT);
                damage(ch, victim, RomRandom.number_range(2, 5), Gsn.dirt, DAM_NONE, false);
                Comm.send_to_char("{5You can't see a thing!{x\n\r", victim);
                Skills.check_improve(ch, Gsn.dirt, true, 2);
                Bit.WAIT_STATE(ch, Tables.skill_table[Gsn.dirt].beats);

                af.where = TO_AFFECTS;
                af.type = Gsn.dirt;
                af.level = ch.level;
                af.duration = 0;
                af.location = APPLY_HITROLL;
                af.modifier = -4;
                af.bitvector = AFF_BLIND;

                Handler.affect_to_char(victim, af);
            }
            else
            {
                damage(ch, victim, 0, Gsn.dirt, DAM_NONE, true);
                Skills.check_improve(ch, Gsn.dirt, false, 2);
                Bit.WAIT_STATE(ch, Tables.skill_table[Gsn.dirt].beats);
            }
            check_killer(ch, victim);
        }

        public static void do_trip(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);
            CharData victim;
            int chance;

            if ((chance = Handler.get_skill(ch, Gsn.trip)) == 0
                || (Bit.IS_NPC(ch) && !Bit.IS_SET(ch.off_flags, OFF_TRIP))
                || (!Bit.IS_NPC(ch)
                    && ch.level < Tables.skill_table[Gsn.trip].skill_level[ch.klass]))
            {
                Comm.send_to_char("Tripping?  What's that?\n\r", ch);
                return;
            }

            if (arg.Length == 0)
            {
                victim = ch.fighting;
                if (victim == null)
                {
                    Comm.send_to_char("But you aren't fighting anyone!\n\r", ch);
                    return;
                }
            }
            else if ((victim = Handler.get_char_room(ch, arg)) == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }

            if (is_safe(ch, victim))
                return;

            if (Bit.IS_NPC(victim) &&
                victim.fighting != null && !Handler.is_same_group(ch, victim.fighting))
            {
                Comm.send_to_char("Kill stealing is not permitted.\n\r", ch);
                return;
            }

            if (Bit.IS_AFFECTED(victim, AFF_FLYING))
            {
                Comm.act("$S feet aren't on the ground.", ch, null, victim, TO_CHAR);
                return;
            }

            if (victim.position < POS_FIGHTING)
            {
                Comm.act("$N is already down.", ch, null, victim, TO_CHAR);
                return;
            }

            if (victim == ch)
            {
                Comm.send_to_char("{5You fall flat on your face!{x\n\r", ch);
                Bit.WAIT_STATE(ch, 2 * Tables.skill_table[Gsn.trip].beats);
                Comm.act("{5$n trips over $s own feet!{x", ch, null, null, TO_ROOM);
                return;
            }

            if (Bit.IS_AFFECTED(ch, AFF_CHARM) && ch.master == victim)
            {
                Comm.act("$N is your beloved master.", ch, null, victim, TO_CHAR);
                return;
            }

            if (ch.size < victim.size)
                chance += (ch.size - victim.size) * 10;

            chance += Handler.get_curr_stat(ch, STAT_DEX);
            chance -= Handler.get_curr_stat(victim, STAT_DEX) * 3 / 2;

            if (Bit.IS_SET(ch.off_flags, OFF_FAST) || Bit.IS_AFFECTED(ch, AFF_HASTE))
                chance += 10;
            if (Bit.IS_SET(victim.off_flags, OFF_FAST)
                || Bit.IS_AFFECTED(victim, AFF_HASTE))
                chance -= 20;

            chance += (ch.level - victim.level) * 2;

            if (RomRandom.number_percent() < chance)
            {
                Comm.act("{5$n trips you and you go down!{x", ch, null, victim, TO_VICT);
                Comm.act("{5You trip $N and $N goes down!{x", ch, null, victim, TO_CHAR);
                Comm.act("{5$n trips $N, sending $M to the ground.{x", ch, null, victim,
                    TO_NOTVICT);
                Skills.check_improve(ch, Gsn.trip, true, 1);

                Bit.DAZE_STATE(victim, 2 * PULSE_VIOLENCE);
                Bit.WAIT_STATE(ch, Tables.skill_table[Gsn.trip].beats);
                victim.position = POS_RESTING;
                damage(ch, victim, RomRandom.number_range(2, 2 + 2 * victim.size), Gsn.trip,
                    DAM_BASH, true);
            }
            else
            {
                damage(ch, victim, 0, Gsn.trip, DAM_BASH, true);
                Bit.WAIT_STATE(ch, Tables.skill_table[Gsn.trip].beats * 2 / 3);
                Skills.check_improve(ch, Gsn.trip, false, 1);
            }
            check_killer(ch, victim);
        }

        public static void do_murde(CharData ch, string argument)
        {
            Comm.send_to_char("If you want to MURDER, spell it out.\n\r", ch);
        }

        public static void do_murder(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Murder whom?\n\r", ch);
                return;
            }

            if (Bit.IS_AFFECTED(ch, AFF_CHARM)
                || (Bit.IS_NPC(ch) && Bit.IS_SET(ch.act, ACT_PET)))
                return;

            var victim = Handler.get_char_room(ch, arg);
            if (victim == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }

            if (victim == ch)
            {
                Comm.send_to_char("Suicide is a mortal sin.\n\r", ch);
                return;
            }

            if (is_safe(ch, victim))
                return;

            if (Bit.IS_NPC(victim) &&
                victim.fighting != null && !Handler.is_same_group(ch, victim.fighting))
            {
                Comm.send_to_char("Kill stealing is not permitted.\n\r", ch);
                return;
            }

            if (Bit.IS_AFFECTED(ch, AFF_CHARM) && ch.master == victim)
            {
                Comm.act("$N is your beloved master.", ch, null, victim, TO_CHAR);
                return;
            }

            if (ch.position == POS_FIGHTING)
            {
                Comm.send_to_char("You do the best you can!\n\r", ch);
                return;
            }

            Bit.WAIT_STATE(ch, 1 * PULSE_VIOLENCE);
            string buf;
            if (Bit.IS_NPC(ch))
                buf = RomString.sprintf("Help! I am being attacked by %s!", ch.short_descr);
            else
                buf = RomString.sprintf("Help!  I am being attacked by %s!", ch.name);
            Interp.do_function(victim, ActComm.do_yell, buf);
            check_killer(ch, victim);
            multi_hit(ch, victim, TYPE_UNDEFINED);
        }

        public static void do_backstab(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Backstab whom?\n\r", ch);
                return;
            }

            if (ch.fighting != null)
            {
                Comm.send_to_char("You're facing the wrong end.\n\r", ch);
                return;
            }
            else if (Handler.get_char_room(ch, arg) is not CharData victim)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }
            else
            {
                if (victim == ch)
                {
                    Comm.send_to_char("How can you sneak up on yourself?\n\r", ch);
                    return;
                }

                if (is_safe(ch, victim))
                    return;

                if (Bit.IS_NPC(victim) &&
                    victim.fighting != null && !Handler.is_same_group(ch, victim.fighting))
                {
                    Comm.send_to_char("Kill stealing is not permitted.\n\r", ch);
                    return;
                }

                if (Handler.get_eq_char(ch, WEAR_WIELD) == null)
                {
                    Comm.send_to_char("You need to wield a weapon to backstab.\n\r", ch);
                    return;
                }

                if (victim.hit < victim.max_hit / 3)
                {
                    Comm.act("$N is hurt and suspicious ... you can't sneak up.",
                        ch, null, victim, TO_CHAR);
                    return;
                }

                check_killer(ch, victim);
                Bit.WAIT_STATE(ch, Tables.skill_table[Gsn.backstab].beats);
                if (RomRandom.number_percent() < Handler.get_skill(ch, Gsn.backstab)
                    || (Handler.get_skill(ch, Gsn.backstab) >= 2 && !Bit.IS_AWAKE(victim)))
                {
                    Skills.check_improve(ch, Gsn.backstab, true, 1);
                    multi_hit(ch, victim, Gsn.backstab);
                }
                else
                {
                    Skills.check_improve(ch, Gsn.backstab, false, 1);
                    damage(ch, victim, 0, Gsn.backstab, DAM_NONE, true);
                }
            }
        }

        public static void do_rescue(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                Comm.send_to_char("Rescue whom?\n\r", ch);
                return;
            }

            var victim = Handler.get_char_room(ch, arg);
            if (victim == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }

            if (victim == ch)
            {
                Comm.send_to_char("What about fleeing instead?\n\r", ch);
                return;
            }

            if (!Bit.IS_NPC(ch) && Bit.IS_NPC(victim))
            {
                Comm.send_to_char("Doesn't need your help!\n\r", ch);
                return;
            }

            if (ch.fighting == victim)
            {
                Comm.send_to_char("Too late.\n\r", ch);
                return;
            }

            var fch = victim.fighting;
            if (fch == null)
            {
                Comm.send_to_char("That person is not fighting right now.\n\r", ch);
                return;
            }

            if (Bit.IS_NPC(fch) && !Handler.is_same_group(ch, victim))
            {
                Comm.send_to_char("Kill stealing is not permitted.\n\r", ch);
                return;
            }

            Bit.WAIT_STATE(ch, Tables.skill_table[Gsn.rescue].beats);
            if (RomRandom.number_percent() > Handler.get_skill(ch, Gsn.rescue))
            {
                Comm.send_to_char("You fail the rescue.\n\r", ch);
                Skills.check_improve(ch, Gsn.rescue, false, 1);
                return;
            }

            Comm.act("{5You rescue $N!{x", ch, null, victim, TO_CHAR);
            Comm.act("{5$n rescues you!{x", ch, null, victim, TO_VICT);
            Comm.act("{5$n rescues $N!{x", ch, null, victim, TO_NOTVICT);
            Skills.check_improve(ch, Gsn.rescue, true, 1);

            stop_fighting(fch, false);
            stop_fighting(victim, false);

            check_killer(ch, fch);
            set_fighting(ch, fch);
            set_fighting(fch, ch);
        }

        public static void do_kick(CharData ch, string argument)
        {
            if (!Bit.IS_NPC(ch)
                && ch.level < Tables.skill_table[Gsn.kick].skill_level[ch.klass])
            {
                Comm.send_to_char("You better leave the martial arts to fighters.\n\r",
                    ch);
                return;
            }

            if (Bit.IS_NPC(ch) && !Bit.IS_SET(ch.off_flags, OFF_KICK))
                return;

            var victim = ch.fighting;
            if (victim == null)
            {
                Comm.send_to_char("You aren't fighting anyone.\n\r", ch);
                return;
            }

            Bit.WAIT_STATE(ch, Tables.skill_table[Gsn.kick].beats);
            if (Handler.get_skill(ch, Gsn.kick) > RomRandom.number_percent())
            {
                damage(ch, victim, RomRandom.number_range(1, ch.level), Gsn.kick, DAM_BASH,
                    true);
                Skills.check_improve(ch, Gsn.kick, true, 1);
            }
            else
            {
                damage(ch, victim, 0, Gsn.kick, DAM_BASH, true);
                Skills.check_improve(ch, Gsn.kick, false, 1);
            }
            check_killer(ch, victim);
        }

        public static void do_disarm(CharData ch, string argument)
        {
            int hth = 0;

            int chance;
            if ((chance = Handler.get_skill(ch, Gsn.disarm)) == 0)
            {
                Comm.send_to_char("You don't know how to disarm opponents.\n\r", ch);
                return;
            }

            if (Handler.get_eq_char(ch, WEAR_WIELD) == null
                && ((hth = Handler.get_skill(ch, Gsn.hand_to_hand)) == 0
                    || (Bit.IS_NPC(ch) && !Bit.IS_SET(ch.off_flags, OFF_DISARM))))
            {
                Comm.send_to_char("You must wield a weapon to disarm.\n\r", ch);
                return;
            }

            var victim = ch.fighting;
            if (victim == null)
            {
                Comm.send_to_char("You aren't fighting anyone.\n\r", ch);
                return;
            }

            if (Handler.get_eq_char(victim, WEAR_WIELD) == null)
            {
                Comm.send_to_char("Your opponent is not wielding a weapon.\n\r", ch);
                return;
            }

            int ch_weapon = Handler.get_weapon_skill(ch, Handler.get_weapon_sn(ch));
            int vict_weapon = Handler.get_weapon_skill(victim, Handler.get_weapon_sn(victim));
            int ch_vict_weapon = Handler.get_weapon_skill(ch, Handler.get_weapon_sn(victim));

            if (Handler.get_eq_char(ch, WEAR_WIELD) == null)
                chance = chance * hth / 150;
            else
                chance = chance * ch_weapon / 100;

            chance += (ch_vict_weapon / 2 - vict_weapon) / 2;

            chance += Handler.get_curr_stat(ch, STAT_DEX);
            chance -= 2 * Handler.get_curr_stat(victim, STAT_STR);

            chance += (ch.level - victim.level) * 2;

            if (RomRandom.number_percent() < chance)
            {
                Bit.WAIT_STATE(ch, Tables.skill_table[Gsn.disarm].beats);
                disarm(ch, victim);
                Skills.check_improve(ch, Gsn.disarm, true, 1);
            }
            else
            {
                Bit.WAIT_STATE(ch, Tables.skill_table[Gsn.disarm].beats);
                Comm.act("{5You fail to disarm $N.{x", ch, null, victim, TO_CHAR);
                Comm.act("{5$n tries to disarm you, but fails.{x", ch, null, victim,
                    TO_VICT);
                Comm.act("{5$n tries to disarm $N, but fails.{x", ch, null, victim,
                    TO_NOTVICT);
                Skills.check_improve(ch, Gsn.disarm, false, 1);
            }
            check_killer(ch, victim);
        }

        public static void do_surrender(CharData ch, string argument)
        {
            var mob = ch.fighting;
            if (mob == null)
            {
                Comm.send_to_char("But you're not fighting!\n\r", ch);
                return;
            }
            Comm.act("You surrender to $N!", ch, null, mob, TO_CHAR);
            Comm.act("$n surrenders to you!", ch, null, mob, TO_VICT);
            Comm.act("$n tries to surrender to $N!", ch, null, mob, TO_NOTVICT);
            stop_fighting(ch, true);

            if (!Bit.IS_NPC(ch) && Bit.IS_NPC(mob)
                && (!Bit.HAS_TRIGGER(mob, TRIG_SURR)
                    || !MobProg.mp_percent_trigger(mob, ch, null, null, (int)TRIG_SURR)))
            {
                Comm.act("$N seems to ignore your cowardly act!", ch, null, mob, TO_CHAR);
                multi_hit(mob, ch, TYPE_UNDEFINED);
            }
        }

        public static void do_sla(CharData ch, string argument)
        {
            Comm.send_to_char("If you want to SLAY, spell it out.\n\r", ch);
        }

        public static void do_slay(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                Comm.send_to_char("Slay whom?\n\r", ch);
                return;
            }

            var victim = Handler.get_char_room(ch, arg);
            if (victim == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }

            if (ch == victim)
            {
                Comm.send_to_char("Suicide is a mortal sin.\n\r", ch);
                return;
            }

            if (!Bit.IS_NPC(victim) && victim.level >= Handler.get_trust(ch))
            {
                Comm.send_to_char("You failed.\n\r", ch);
                return;
            }

            Comm.act("{1You slay $M in cold blood!{x", ch, null, victim, TO_CHAR);
            Comm.act("{1$n slays you in cold blood!{x", ch, null, victim, TO_VICT);
            Comm.act("{1$n slays $N in cold blood!{x", ch, null, victim, TO_NOTVICT);
            raw_kill(victim);
        }
    }
}
