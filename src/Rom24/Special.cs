using static Rom24.Merc;

namespace Rom24
{
    public static class Special
    {
        public static readonly SpecType[] spec_table =
        {
            new SpecType("spec_breath_any", spec_breath_any),
            new SpecType("spec_breath_acid", spec_breath_acid),
            new SpecType("spec_breath_fire", spec_breath_fire),
            new SpecType("spec_breath_frost", spec_breath_frost),
            new SpecType("spec_breath_gas", spec_breath_gas),
            new SpecType("spec_breath_lightning", spec_breath_lightning),
            new SpecType("spec_cast_adept", spec_cast_adept),
            new SpecType("spec_cast_cleric", spec_cast_cleric),
            new SpecType("spec_cast_judge", spec_cast_judge),
            new SpecType("spec_cast_mage", spec_cast_mage),
            new SpecType("spec_cast_undead", spec_cast_undead),
            new SpecType("spec_executioner", spec_executioner),
            new SpecType("spec_fido", spec_fido),
            new SpecType("spec_guard", spec_guard),
            new SpecType("spec_janitor", spec_janitor),
            new SpecType("spec_mayor", spec_mayor),
            new SpecType("spec_poison", spec_poison),
            new SpecType("spec_thief", spec_thief),
            new SpecType("spec_nasty", spec_nasty),
            new SpecType("spec_troll_member", spec_troll_member),
            new SpecType("spec_ogre_member", spec_ogre_member),
            new SpecType("spec_patrolman", spec_patrolman),
            new SpecType(null, null)
        };

        public static SpecFun spec_lookup(string name)
        {
            /* special.c: LOWER(name[0]) on empty is LOWER('\0') and never matches */
            name ??= "";
            if (name.Length == 0)
                return null;
            for (int i = 0; spec_table[i].name != null; i++)
            {
                if (Bit.LOWER(name[0]) == Bit.LOWER(spec_table[i].name[0])
                    && !RomString.str_prefix(name, spec_table[i].name))
                    return spec_table[i].function;
            }
            return null;
        }

        public static string spec_name(SpecFun function)
        {
            for (int i = 0; spec_table[i].function != null; i++)
            {
                if (function == spec_table[i].function)
                    return spec_table[i].name;
            }
            return null;
        }

        public static bool spec_troll_member(CharData ch)
        {
            CharData victim = null;
            int count = 0;
            string message;

            if (!Bit.IS_AWAKE(ch) || Bit.IS_AFFECTED(ch, AFF_CALM) || ch.in_room == null
                || Bit.IS_AFFECTED(ch, AFF_CHARM) || ch.fighting != null)
                return false;

            for (var vch = ch.in_room.people; vch != null; vch = vch.next_in_room)
            {
                if (!Bit.IS_NPC(vch) || ch == vch)
                    continue;

                if (vch.pIndexData.vnum == MOB_VNUM_PATROLMAN)
                    return false;

                if (vch.pIndexData.group == GROUP_VNUM_OGRES
                    && ch.level > vch.level - 2 && !Fight.is_safe(ch, vch))
                {
                    if (RomRandom.number_range(0, count) == 0)
                        victim = vch;

                    count++;
                }
            }

            if (victim == null)
                return false;

            switch (RomRandom.number_range(0, 6))
            {
                default:
                    message = null;
                    break;
                case 0:
                    message = "$n yells 'I've been looking for you, punk!'";
                    break;
                case 1:
                    message = "With a scream of rage, $n attacks $N.";
                    break;
                case 2:
                    message =
                        "$n says 'What's slimy Ogre trash like you doing around here?'";
                    break;
                case 3:
                    message = "$n cracks his knuckles and says 'Do ya feel lucky?'";
                    break;
                case 4:
                    message = "$n says 'There's no cops to save you this time!'";
                    break;
                case 5:
                    message = "$n says 'Time to join your brother, spud.'";
                    break;
                case 6:
                    message = "$n says 'Let's rock.'";
                    break;
            }

            if (message != null)
                Comm.act(message, ch, null, victim, TO_ALL);
            Fight.multi_hit(ch, victim, TYPE_UNDEFINED);
            return true;
        }

        public static bool spec_ogre_member(CharData ch)
        {
            CharData victim = null;
            int count = 0;
            string message;

            if (!Bit.IS_AWAKE(ch) || Bit.IS_AFFECTED(ch, AFF_CALM) || ch.in_room == null
                || Bit.IS_AFFECTED(ch, AFF_CHARM) || ch.fighting != null)
                return false;

            for (var vch = ch.in_room.people; vch != null; vch = vch.next_in_room)
            {
                if (!Bit.IS_NPC(vch) || ch == vch)
                    continue;

                if (vch.pIndexData.vnum == MOB_VNUM_PATROLMAN)
                    return false;

                if (vch.pIndexData.group == GROUP_VNUM_TROLLS
                    && ch.level > vch.level - 2 && !Fight.is_safe(ch, vch))
                {
                    if (RomRandom.number_range(0, count) == 0)
                        victim = vch;

                    count++;
                }
            }

            if (victim == null)
                return false;

            switch (RomRandom.number_range(0, 6))
            {
                default:
                    message = null;
                    break;
                case 0:
                    message = "$n yells 'I've been looking for you, punk!'";
                    break;
                case 1:
                    message = "With a scream of rage, $n attacks $N.'";
                    break;
                case 2:
                    message =
                        "$n says 'What's Troll filth like you doing around here?'";
                    break;
                case 3:
                    message = "$n cracks his knuckles and says 'Do ya feel lucky?'";
                    break;
                case 4:
                    message = "$n says 'There's no cops to save you this time!'";
                    break;
                case 5:
                    message = "$n says 'Time to join your brother, spud.'";
                    break;
                case 6:
                    message = "$n says 'Let's rock.'";
                    break;
            }

            if (message != null)
                Comm.act(message, ch, null, victim, TO_ALL);
            Fight.multi_hit(ch, victim, TYPE_UNDEFINED);
            return true;
        }

        public static bool spec_patrolman(CharData ch)
        {
            CharData victim = null;
            string message;
            int count = 0;

            if (!Bit.IS_AWAKE(ch) || Bit.IS_AFFECTED(ch, AFF_CALM) || ch.in_room == null
                || Bit.IS_AFFECTED(ch, AFF_CHARM) || ch.fighting != null)
                return false;

            for (var vch = ch.in_room.people; vch != null; vch = vch.next_in_room)
            {
                if (vch == ch)
                    continue;

                if (vch.fighting != null)
                {
                    if (RomRandom.number_range(0, count) == 0)
                        victim = (vch.level > vch.fighting.level)
                            ? vch : vch.fighting;
                    count++;
                }
            }

            if (victim == null
                || (Bit.IS_NPC(victim)
                    && victim.spec_fun == ch.spec_fun)) return false;

            ObjData obj;
            if (((obj = Handler.get_eq_char(ch, WEAR_NECK_1)) != null
                 && obj.pIndexData.vnum == OBJ_VNUM_WHISTLE)
                || ((obj = Handler.get_eq_char(ch, WEAR_NECK_2)) != null
                    && obj.pIndexData.vnum == OBJ_VNUM_WHISTLE))
            {
                Comm.act("You blow down hard on $p.", ch, obj, null, TO_CHAR);
                Comm.act("$n blows on $p, ***WHEEEEEEEEEEEET***", ch, obj, null, TO_ROOM);

                for (var vch = Game.char_list; vch != null; vch = vch.next)
                {
                    if (vch.in_room == null)
                        continue;

                    if (vch.in_room != ch.in_room
                        && vch.in_room.area == ch.in_room.area)
                        Comm.send_to_char("You hear a shrill whistling sound.\n\r", vch);
                }
            }

            switch (RomRandom.number_range(0, 6))
            {
                default:
                    message = null;
                    break;
                case 0:
                    message = "$n yells 'All roit! All roit! break it up!'";
                    break;
                case 1:
                    message =
                        "$n says 'Society's to blame, but what's a bloke to do?'";
                    break;
                case 2:
                    message = "$n mumbles 'bloody kids will be the death of us all.'";
                    break;
                case 3:
                    message = "$n shouts 'Stop that! Stop that!' and attacks.";
                    break;
                case 4:
                    message = "$n pulls out his billy and goes to work.";
                    break;
                case 5:
                    message =
                        "$n sighs in resignation and proceeds to break up the fight.";
                    break;
                case 6:
                    message = "$n says 'Settle down, you hooligans!'";
                    break;
            }

            if (message != null)
                Comm.act(message, ch, null, null, TO_ALL);

            Fight.multi_hit(ch, victim, TYPE_UNDEFINED);

            return true;
        }

        public static bool spec_nasty(CharData ch)
        {
            if (!Bit.IS_AWAKE(ch))
            {
                return false;
            }

            if (ch.position != POS_FIGHTING)
            {
                for (var victim = ch.in_room.people; victim != null; )
                {
                    var v_next = victim.next_in_room;
                    if (!Bit.IS_NPC(victim)
                        && (victim.level > ch.level)
                        && (victim.level < ch.level + 10))
                    {
                        Interp.do_function(ch, Fight.do_backstab, victim.name);
                        if (ch.position != POS_FIGHTING)
                        {
                            Interp.do_function(ch, Fight.do_murder, victim.name);
                        }

                        return true;
                    }
                    victim = v_next;
                }
                return false;
            }

            var fight = ch.fighting;
            if (fight == null)
                return false;

            switch (RomRandom.number_bits(2))
            {
                case 0:
                    Comm.act("$n rips apart your coin purse, spilling your gold!",
                        ch, null, fight, TO_VICT);
                    Comm.act("You slash apart $N's coin purse and gather his gold.",
                        ch, null, fight, TO_CHAR);
                    Comm.act("$N's coin purse is ripped apart!",
                        ch, null, fight, TO_NOTVICT);
                    long gold = fight.gold / 10;
                    fight.gold -= gold;
                    ch.gold += gold;
                    return true;

                case 1:
                    Interp.do_function(ch, Fight.do_flee, "");
                    return true;

                default:
                    return false;
            }
        }

        static bool dragon(CharData ch, string spell_name)
        {
            if (ch.position != POS_FIGHTING)
                return false;

            CharData victim = null;
            for (var vch = ch.in_room.people; vch != null; )
            {
                var v_next = vch.next_in_room;
                if (vch.fighting == ch && RomRandom.number_bits(3) == 0)
                {
                    victim = vch;
                    break;
                }
                vch = v_next;
            }

            if (victim == null)
                return false;

            int sn = Lookup.skill_lookup(spell_name);
            if (sn < 0)
                return false;
            Tables.skill_table[sn].spell_fun(sn, ch.level, ch, victim, TARGET_CHAR);
            return true;
        }

        public static bool spec_breath_any(CharData ch)
        {
            if (ch.position != POS_FIGHTING)
                return false;

            switch (RomRandom.number_bits(3))
            {
                case 0:
                    return spec_breath_fire(ch);
                case 1:
                case 2:
                    return spec_breath_lightning(ch);
                case 3:
                    return spec_breath_gas(ch);
                case 4:
                    return spec_breath_acid(ch);
                case 5:
                case 6:
                case 7:
                    return spec_breath_frost(ch);
            }

            return false;
        }

        public static bool spec_breath_acid(CharData ch)
        {
            return dragon(ch, "acid breath");
        }

        public static bool spec_breath_fire(CharData ch)
        {
            return dragon(ch, "fire breath");
        }

        public static bool spec_breath_frost(CharData ch)
        {
            return dragon(ch, "frost breath");
        }

        public static bool spec_breath_gas(CharData ch)
        {
            if (ch.position != POS_FIGHTING)
                return false;

            int sn = Lookup.skill_lookup("gas breath");
            if (sn < 0)
                return false;
            Tables.skill_table[sn].spell_fun(sn, ch.level, ch, null, TARGET_CHAR);
            return true;
        }

        public static bool spec_breath_lightning(CharData ch)
        {
            return dragon(ch, "lightning breath");
        }

        public static bool spec_cast_adept(CharData ch)
        {
            if (!Bit.IS_AWAKE(ch))
                return false;

            CharData victim = null;
            for (var vch = ch.in_room.people; vch != null; )
            {
                var v_next = vch.next_in_room;
                if (vch != ch && Handler.can_see(ch, vch) && RomRandom.number_bits(1) == 0
                    && !Bit.IS_NPC(vch) && vch.level < 11)
                {
                    victim = vch;
                    break;
                }
                vch = v_next;
            }

            if (victim == null)
                return false;

            switch (RomRandom.number_bits(4))
            {
                case 0:
                    Comm.act("$n utters the word 'abrazak'.", ch, null, null, TO_ROOM);
                    Magic.spell_armor(Lookup.skill_lookup("armor"), ch.level, ch, victim,
                        TARGET_CHAR);
                    return true;

                case 1:
                    Comm.act("$n utters the word 'fido'.", ch, null, null, TO_ROOM);
                    Magic.spell_bless(Lookup.skill_lookup("bless"), ch.level, ch, victim,
                        TARGET_CHAR);
                    return true;

                case 2:
                    Comm.act("$n utters the words 'judicandus noselacri'.", ch, null,
                        null, TO_ROOM);
                    Magic.spell_cure_blindness(Lookup.skill_lookup("cure blindness"), ch.level,
                        ch, victim, TARGET_CHAR);
                    return true;

                case 3:
                    Comm.act("$n utters the words 'judicandus dies'.", ch, null, null,
                        TO_ROOM);
                    Magic.spell_cure_light(Lookup.skill_lookup("cure light"), ch.level, ch,
                        victim, TARGET_CHAR);
                    return true;

                case 4:
                    Comm.act("$n utters the words 'judicandus sausabru'.", ch, null, null,
                        TO_ROOM);
                    Magic.spell_cure_poison(Lookup.skill_lookup("cure poison"), ch.level, ch,
                        victim, TARGET_CHAR);
                    return true;

                case 5:
                    Comm.act("$n utters the word 'candusima'.", ch, null, null, TO_ROOM);
                    Magic.spell_refresh(Lookup.skill_lookup("refresh"), ch.level, ch, victim,
                        TARGET_CHAR);
                    return true;

                case 6:
                    Comm.act("$n utters the words 'judicandus eugzagz'.", ch, null, null,
                        TO_ROOM);
                    Magic.spell_cure_disease(Lookup.skill_lookup("cure disease"), ch.level, ch,
                        victim, TARGET_CHAR);
                    break;
            }

            return false;
        }

        public static bool spec_cast_cleric(CharData ch)
        {
            if (ch.position != POS_FIGHTING)
                return false;

            CharData victim = null;
            for (var vch = ch.in_room.people; vch != null; )
            {
                var v_next = vch.next_in_room;
                if (vch.fighting == ch && RomRandom.number_bits(2) == 0)
                {
                    victim = vch;
                    break;
                }
                vch = v_next;
            }

            if (victim == null)
                return false;

            string spell;
            for (;;)
            {
                int min_level;

                switch (RomRandom.number_bits(4))
                {
                    case 0:
                        min_level = 0;
                        spell = "blindness";
                        break;
                    case 1:
                        min_level = 3;
                        spell = "cause serious";
                        break;
                    case 2:
                        min_level = 7;
                        spell = "earthquake";
                        break;
                    case 3:
                        min_level = 9;
                        spell = "cause critical";
                        break;
                    case 4:
                        min_level = 10;
                        spell = "dispel evil";
                        break;
                    case 5:
                        min_level = 12;
                        spell = "curse";
                        break;
                    case 6:
                        min_level = 12;
                        spell = "change sex";
                        break;
                    case 7:
                        min_level = 13;
                        spell = "flamestrike";
                        break;
                    case 8:
                    case 9:
                    case 10:
                        min_level = 15;
                        spell = "harm";
                        break;
                    case 11:
                        min_level = 15;
                        spell = "plague";
                        break;
                    default:
                        min_level = 16;
                        spell = "dispel magic";
                        break;
                }

                if (ch.level >= min_level)
                    break;
            }

            int sn = Lookup.skill_lookup(spell);
            if (sn < 0)
                return false;
            Tables.skill_table[sn].spell_fun(sn, ch.level, ch, victim, TARGET_CHAR);
            return true;
        }

        public static bool spec_cast_judge(CharData ch)
        {
            if (ch.position != POS_FIGHTING)
                return false;

            CharData victim = null;
            for (var vch = ch.in_room.people; vch != null; )
            {
                var v_next = vch.next_in_room;
                if (vch.fighting == ch && RomRandom.number_bits(2) == 0)
                {
                    victim = vch;
                    break;
                }
                vch = v_next;
            }

            if (victim == null)
                return false;

            string spell = "high explosive";
            int sn = Lookup.skill_lookup(spell);
            if (sn < 0)
                return false;
            Tables.skill_table[sn].spell_fun(sn, ch.level, ch, victim, TARGET_CHAR);
            return true;
        }

        public static bool spec_cast_mage(CharData ch)
        {
            if (ch.position != POS_FIGHTING)
                return false;

            CharData victim = null;
            for (var vch = ch.in_room.people; vch != null; )
            {
                var v_next = vch.next_in_room;
                if (vch.fighting == ch && RomRandom.number_bits(2) == 0)
                {
                    victim = vch;
                    break;
                }
                vch = v_next;
            }

            if (victim == null)
                return false;

            string spell;
            for (;;)
            {
                int min_level;

                switch (RomRandom.number_bits(4))
                {
                    case 0:
                        min_level = 0;
                        spell = "blindness";
                        break;
                    case 1:
                        min_level = 3;
                        spell = "chill touch";
                        break;
                    case 2:
                        min_level = 7;
                        spell = "weaken";
                        break;
                    case 3:
                        min_level = 8;
                        spell = "teleport";
                        break;
                    case 4:
                        min_level = 11;
                        spell = "colour spray";
                        break;
                    case 5:
                        min_level = 12;
                        spell = "change sex";
                        break;
                    case 6:
                        min_level = 13;
                        spell = "energy drain";
                        break;
                    case 7:
                    case 8:
                    case 9:
                        min_level = 15;
                        spell = "fireball";
                        break;
                    case 10:
                        min_level = 20;
                        spell = "plague";
                        break;
                    default:
                        min_level = 20;
                        spell = "acid blast";
                        break;
                }

                if (ch.level >= min_level)
                    break;
            }

            int sn = Lookup.skill_lookup(spell);
            if (sn < 0)
                return false;
            Tables.skill_table[sn].spell_fun(sn, ch.level, ch, victim, TARGET_CHAR);
            return true;
        }

        public static bool spec_cast_undead(CharData ch)
        {
            if (ch.position != POS_FIGHTING)
                return false;

            CharData victim = null;
            for (var vch = ch.in_room.people; vch != null; )
            {
                var v_next = vch.next_in_room;
                if (vch.fighting == ch && RomRandom.number_bits(2) == 0)
                {
                    victim = vch;
                    break;
                }
                vch = v_next;
            }

            if (victim == null)
                return false;

            string spell;
            for (;;)
            {
                int min_level;

                switch (RomRandom.number_bits(4))
                {
                    case 0:
                        min_level = 0;
                        spell = "curse";
                        break;
                    case 1:
                        min_level = 3;
                        spell = "weaken";
                        break;
                    case 2:
                        min_level = 6;
                        spell = "chill touch";
                        break;
                    case 3:
                        min_level = 9;
                        spell = "blindness";
                        break;
                    case 4:
                        min_level = 12;
                        spell = "poison";
                        break;
                    case 5:
                        min_level = 15;
                        spell = "energy drain";
                        break;
                    case 6:
                        min_level = 18;
                        spell = "harm";
                        break;
                    case 7:
                        min_level = 21;
                        spell = "teleport";
                        break;
                    case 8:
                        min_level = 20;
                        spell = "plague";
                        break;
                    default:
                        min_level = 18;
                        spell = "harm";
                        break;
                }

                if (ch.level >= min_level)
                    break;
            }

            int sn = Lookup.skill_lookup(spell);
            if (sn < 0)
                return false;
            Tables.skill_table[sn].spell_fun(sn, ch.level, ch, victim, TARGET_CHAR);
            return true;
        }

        public static bool spec_executioner(CharData ch)
        {
            if (!Bit.IS_AWAKE(ch) || ch.fighting != null)
                return false;

            string crime = "";
            CharData victim = null;
            for (var vch = ch.in_room.people; vch != null; )
            {
                var v_next = vch.next_in_room;

                if (!Bit.IS_NPC(vch) && Bit.IS_SET(vch.act, PLR_KILLER)
                    && Handler.can_see(ch, vch))
                {
                    crime = "KILLER";
                    victim = vch;
                    break;
                }

                if (!Bit.IS_NPC(vch) && Bit.IS_SET(vch.act, PLR_THIEF)
                    && Handler.can_see(ch, vch))
                {
                    crime = "THIEF";
                    victim = vch;
                    break;
                }
                vch = v_next;
            }

            if (victim == null)
                return false;

            string buf = RomString.sprintf("%s is a %s!  PROTECT THE INNOCENT!  MORE BLOOOOD!!!",
                victim.name, crime);
            Bit.REMOVE_BIT(ref ch.comm, COMM_NOSHOUT);
            Interp.do_function(ch, ActComm.do_yell, buf);
            Fight.multi_hit(ch, victim, TYPE_UNDEFINED);
            return true;
        }

        public static bool spec_fido(CharData ch)
        {
            if (!Bit.IS_AWAKE(ch))
                return false;

            for (var corpse = ch.in_room.contents; corpse != null; )
            {
                var c_next = corpse.next_content;
                if (corpse.item_type != ITEM_CORPSE_NPC)
                {
                    corpse = c_next;
                    continue;
                }

                Comm.act("$n savagely devours a corpse.", ch, null, null, TO_ROOM);
                for (var obj = corpse.contains; obj != null; )
                {
                    var obj_next = obj.next_content;
                    Handler.obj_from_obj(obj);
                    Handler.obj_to_room(obj, ch.in_room);
                    obj = obj_next;
                }
                Handler.extract_obj(corpse);
                return true;
            }

            return false;
        }

        public static bool spec_guard(CharData ch)
        {
            if (!Bit.IS_AWAKE(ch) || ch.fighting != null)
                return false;

            int max_evil = 300;
            CharData ech = null;
            string crime = "";
            CharData victim = null;

            for (var vch = ch.in_room.people; vch != null; )
            {
                var v_next = vch.next_in_room;

                if (!Bit.IS_NPC(vch) && Bit.IS_SET(vch.act, PLR_KILLER)
                    && Handler.can_see(ch, vch))
                {
                    crime = "KILLER";
                    victim = vch;
                    break;
                }

                if (!Bit.IS_NPC(vch) && Bit.IS_SET(vch.act, PLR_THIEF)
                    && Handler.can_see(ch, vch))
                {
                    crime = "THIEF";
                    victim = vch;
                    break;
                }

                if (vch.fighting != null
                    && vch.fighting != ch && vch.alignment < max_evil)
                {
                    max_evil = vch.alignment;
                    ech = vch;
                }
                vch = v_next;
            }

            if (victim != null)
            {
                string buf = RomString.sprintf("%s is a %s!  PROTECT THE INNOCENT!!  BANZAI!!",
                    victim.name, crime);
                Bit.REMOVE_BIT(ref ch.comm, COMM_NOSHOUT);
                Interp.do_function(ch, ActComm.do_yell, buf);
                Fight.multi_hit(ch, victim, TYPE_UNDEFINED);
                return true;
            }

            if (ech != null)
            {
                Comm.act("$n screams 'PROTECT THE INNOCENT!!  BANZAI!!",
                    ch, null, null, TO_ROOM);
                Fight.multi_hit(ch, ech, TYPE_UNDEFINED);
                return true;
            }

            return false;
        }

        public static bool spec_janitor(CharData ch)
        {
            if (!Bit.IS_AWAKE(ch))
                return false;

            for (var trash = ch.in_room.contents; trash != null; )
            {
                var trash_next = trash.next_content;
                if (!Bit.IS_SET(trash.wear_flags, ITEM_TAKE) || !ActObj.can_loot(ch, trash))
                {
                    trash = trash_next;
                    continue;
                }
                if (trash.item_type == ITEM_DRINK_CON
                    || trash.item_type == ITEM_TRASH || trash.cost < 10)
                {
                    Comm.act("$n picks up some trash.", ch, null, null, TO_ROOM);
                    Handler.obj_from_room(trash);
                    Handler.obj_to_char(trash, ch);
                    return true;
                }
                trash = trash_next;
            }

            return false;
        }

        static string mayor_path;
        static int mayor_pos;
        static bool mayor_move;

        public static bool spec_mayor(CharData ch)
        {
            const string open_path =
                "W3a3003b33000c111d0d111Oe333333Oe22c222112212111a1S.";

            const string close_path =
                "W3a3003b33000c111d0d111CE333333CE22c222112212111a1S.";

            if (!mayor_move)
            {
                if (Game.time_info.hour == 6)
                {
                    mayor_path = open_path;
                    mayor_move = true;
                    mayor_pos = 0;
                }

                if (Game.time_info.hour == 20)
                {
                    mayor_path = close_path;
                    mayor_move = true;
                    mayor_pos = 0;
                }
            }

            if (ch.fighting != null)
                return spec_cast_mage(ch);
            if (!mayor_move || ch.position < POS_SLEEPING)
                return false;

            switch (mayor_path[mayor_pos])
            {
                case '0':
                case '1':
                case '2':
                case '3':
                    ActMove.move_char(ch, mayor_path[mayor_pos] - '0', false);
                    break;

                case 'W':
                    ch.position = POS_STANDING;
                    Comm.act("$n awakens and groans loudly.", ch, null, null, TO_ROOM);
                    break;

                case 'S':
                    ch.position = POS_SLEEPING;
                    Comm.act("$n lies down and falls asleep.", ch, null, null, TO_ROOM);
                    break;

                case 'a':
                    Comm.act("$n says 'Hello Honey!'", ch, null, null, TO_ROOM);
                    break;

                case 'b':
                    Comm.act
                        ("$n says 'What a view!  I must do something about that dump!'",
                        ch, null, null, TO_ROOM);
                    break;

                case 'c':
                    Comm.act
                        ("$n says 'Vandals!  Youngsters have no respect for anything!'",
                        ch, null, null, TO_ROOM);
                    break;

                case 'd':
                    Comm.act("$n says 'Good day, citizens!'", ch, null, null, TO_ROOM);
                    break;

                case 'e':
                    Comm.act("$n says 'I hereby declare the city of Midgaard open!'",
                        ch, null, null, TO_ROOM);
                    break;

                case 'E':
                    Comm.act("$n says 'I hereby declare the city of Midgaard closed!'",
                        ch, null, null, TO_ROOM);
                    break;

                case 'O':
                    Interp.do_function(ch, ActMove.do_open, "gate");
                    break;

                case 'C':
                    Interp.do_function(ch, ActMove.do_close, "gate");
                    break;

                case '.':
                    mayor_move = false;
                    break;
            }

            mayor_pos++;
            return false;
        }

        public static bool spec_poison(CharData ch)
        {
            CharData victim;

            if (ch.position != POS_FIGHTING
                || (victim = ch.fighting) == null
                || RomRandom.number_percent() > 2 * ch.level) return false;

            Comm.act("You bite $N!", ch, null, victim, TO_CHAR);
            Comm.act("$n bites $N!", ch, null, victim, TO_NOTVICT);
            Comm.act("$n bites you!", ch, null, victim, TO_VICT);
            Magic.spell_poison(Gsn.poison, ch.level, ch, victim, TARGET_CHAR);
            return true;
        }

        public static bool spec_thief(CharData ch)
        {
            if (ch.position != POS_STANDING)
                return false;

            for (var victim = ch.in_room.people; victim != null; )
            {
                var v_next = victim.next_in_room;

                if (Bit.IS_NPC(victim)
                    || victim.level >= LEVEL_IMMORTAL
                    || RomRandom.number_bits(5) != 0 || !Handler.can_see(ch, victim))
                {
                    victim = v_next;
                    continue;
                }

                if (Bit.IS_AWAKE(victim) && RomRandom.number_range(0, ch.level) == 0)
                {
                    Comm.act("You discover $n's hands in your wallet!",
                        ch, null, victim, TO_VICT);
                    Comm.act("$N discovers $n's hands in $S wallet!",
                        ch, null, victim, TO_NOTVICT);
                    return true;
                }
                else
                {
                    long gold =
                        victim.gold * Bit.UMIN(RomRandom.number_range(1, 20),
                            ch.level / 2) / 100;
                    gold = Bit.UMIN(gold, (long)ch.level * ch.level * 10);
                    ch.gold += gold;
                    victim.gold -= gold;
                    long silver =
                        victim.silver * Bit.UMIN(RomRandom.number_range(1, 20),
                            ch.level / 2) / 100;
                    silver = Bit.UMIN(silver, (long)ch.level * ch.level * 25);
                    ch.silver += silver;
                    victim.silver -= silver;
                    return true;
                }
            }

            return false;
        }
    }
}
