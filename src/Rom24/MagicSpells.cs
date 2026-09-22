using static Rom24.Merc;

namespace Rom24
{
    public static partial class Magic
    {public static void spell_acid_blast(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    int dam;

    dam = RomRandom.dice(level, 12);
    if (Magic.saves_spell(level, victim, DAM_ACID))
        dam /= 2;
    Fight.damage(ch, victim, dam, sn, DAM_ACID, true);
    return;
}



public static void spell_armor(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    var af = new AffectData();

    if (Handler.is_affected(victim, sn))
    {
        if (victim == ch)
            Comm.send_to_char("You are already armored.\n\r", ch);
        else
            Comm.act("$N is already armored.", ch, null, victim, TO_CHAR);
        return;
    }
    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.duration = 24;
    af.modifier = -20;
    af.location = APPLY_AC;
    af.bitvector = 0;
    Handler.affect_to_char(victim, af);
    Comm.send_to_char("You feel someone protecting you.\n\r", victim);
    if (ch != victim)
        Comm.act("$N is protected by your magic.", ch, null, victim, TO_CHAR);
    return;
}



public static void spell_bless(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = null;
    ObjData obj = null;
    var af = new AffectData();

    
    if (target == TARGET_OBJ)
    {
        obj = (vo as ObjData);
        if (Bit.IS_OBJ_STAT(obj, ITEM_BLESS))
        {
            Comm.act("$p is already blessed.", ch, obj, null, TO_CHAR);
            return;
        }

        if (Bit.IS_OBJ_STAT(obj, ITEM_EVIL))
        {
            AffectData paf = null;

            paf = Handler.affect_find(obj.affected, Gsn.curse);
            if (!saves_dispel(level, paf != null ? paf.level : obj.level, 0))
            {
                if (paf != null)
                    Handler.affect_remove_obj(obj, paf);
                Comm.act("$p glows a pale blue.", ch, obj, null, TO_ALL);
                Bit.REMOVE_BIT(ref obj.extra_flags, ITEM_EVIL);
                return;
            }
            else
            {
                Comm.act("The evil of $p is too powerful for you to overcome.",
                     ch, obj, null, TO_CHAR);
                return;
            }
        }

        af.where = TO_OBJECT;
        af.type = sn;
        af.level = level;
        af.duration = 6 + level;
        af.location = APPLY_SAVES;
        af.modifier = -1;
        af.bitvector = ITEM_BLESS;
        Handler.affect_to_obj(obj, af);

        Comm.act("$p glows with a holy aura.", ch, obj, null, TO_ALL);

        if (obj.wear_loc != WEAR_NONE)
            ch.saving_throw -= 1;
        return;
    }

    
    victim = (vo as CharData);


    if (victim.position == POS_FIGHTING || Handler.is_affected(victim, sn))
    {
        if (victim == ch)
            Comm.send_to_char("You are already blessed.\n\r", ch);
        else
            Comm.act("$N already has divine favor.", ch, null, victim, TO_CHAR);
        return;
    }

    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.duration = 6 + level;
    af.location = APPLY_HITROLL;
    af.modifier = level / 8;
    af.bitvector = 0;
    Handler.affect_to_char(victim, af);

    af.location = APPLY_SAVING_SPELL;
    af.modifier = 0 - level / 8;
    Handler.affect_to_char(victim, af);
    Comm.send_to_char("You feel righteous.\n\r", victim);
    if (ch != victim)
        Comm.act("You grant $N the favor of your god.", ch, null, victim,
             TO_CHAR);
    return;
}



public static void spell_blindness(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    var af = new AffectData();

    if (Bit.IS_AFFECTED(victim, AFF_BLIND)
        || Magic.saves_spell(level, victim, DAM_OTHER))
        return;


    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.location = APPLY_HITROLL;
    af.modifier = -4;
    af.duration = 1 + level;
    af.bitvector = AFF_BLIND;
    Handler.affect_to_char(victim, af);
    Comm.send_to_char("You are blinded!\n\r", victim);
    Comm.act("$n appears to be blinded.", victim, null, null, TO_ROOM);
    return;
}



public static void spell_burning_hands(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    int[] dam_each = {
        0,
        0, 0, 0, 0, 14, 17, 20, 23, 26, 29,
        29, 29, 30, 30, 31, 31, 32, 32, 33, 33,
        34, 34, 35, 35, 36, 36, 37, 37, 38, 38,
        39, 39, 40, 40, 41, 41, 42, 42, 43, 43,
        44, 44, 45, 45, 46, 46, 47, 47, 48, 48
    };
    int dam;

    level = Bit.UMIN(level, dam_each.Length - 1);
    level = Bit.UMAX(0, level);
    dam = RomRandom.number_range(dam_each[level] / 2, dam_each[level] * 2);
    if (Magic.saves_spell(level, victim, DAM_FIRE))
        dam /= 2;
    Fight.damage(ch, victim, dam, sn, DAM_FIRE, true);
    return;
}



public static void spell_call_lightning(int sn, int level, CharData ch, object vo, int target)
{
    CharData vch = null;
    CharData vch_next = null;
    int dam;

    if (!Bit.IS_OUTSIDE(ch))
    {
        Comm.send_to_char("You must be out of doors.\n\r", ch);
        return;
    }

    if (Game.weather_info.sky < SKY_RAINING)
    {
        Comm.send_to_char("You need bad weather.\n\r", ch);
        return;
    }

    dam = RomRandom.dice(level / 2, 8);

    Comm.send_to_char("Mota's lightning strikes your foes!\n\r", ch);
    Comm.act("$n calls Mota's lightning to strike $s foes!",
         ch, null, null, TO_ROOM);

    for (vch = Game.char_list; vch != null; vch = vch_next)
    {
        vch_next = vch.next;
        if (vch.in_room == null)
            continue;
        if (vch.in_room == ch.in_room)
        {
            if (vch != ch && (Bit.IS_NPC(ch) ? !Bit.IS_NPC(vch) : Bit.IS_NPC(vch)))
                Fight.damage(ch, vch, Magic.saves_spell(level, vch, DAM_LIGHTNING)
                        ? dam / 2 : dam, sn, DAM_LIGHTNING, true);
            continue;
        }

        if (vch.in_room.area == ch.in_room.area && Bit.IS_OUTSIDE(vch)
            && Bit.IS_AWAKE(vch))
            Comm.send_to_char("Lightning flashes in the sky.\n\r", vch);
    }

    return;
}



public static void spell_calm(int sn, int level, CharData ch, object vo, int target)
{
    CharData vch = null;
    int mlevel = 0;
    int count = 0;
    int high_level = 0;
    int chance;
    var af = new AffectData();

    
    for (vch = ch.in_room.people; vch != null; vch = vch.next_in_room)
    {
        if (vch.position == POS_FIGHTING)
        {
            count++;
            if (Bit.IS_NPC(vch))
                mlevel += vch.level;
            else
                mlevel += vch.level / 2;
            high_level = Bit.UMAX(high_level, vch.level);
        }
    }

    
    chance = 4 * level - high_level + 2 * count;

    if (Bit.IS_IMMORTAL(ch))        
        mlevel = 0;

    if (RomRandom.number_range(0, chance) >= mlevel)
    {                            
        for (vch = ch.in_room.people; vch != null; vch = vch.next_in_room)
        {
            if (Bit.IS_NPC(vch) && (Bit.IS_SET(vch.imm_flags, IMM_MAGIC) ||
                                 Bit.IS_SET(vch.act, ACT_UNDEAD)))
                return;

            if (Bit.IS_AFFECTED(vch, AFF_CALM) || Bit.IS_AFFECTED(vch, AFF_BERSERK)
                || Handler.is_affected(vch, Lookup.skill_lookup("frenzy")))
                return;

            Comm.send_to_char("A wave of calm passes over you.\n\r", vch);

            if (vch.fighting != null || vch.position == POS_FIGHTING)
                Fight.stop_fighting(vch, false);


            af.where = TO_AFFECTS;
            af.type = sn;
            af.level = level;
            af.duration = level / 4;
            af.location = APPLY_HITROLL;
            if (!Bit.IS_NPC(vch))
                af.modifier = -5;
            else
                af.modifier = -2;
            af.bitvector = AFF_CALM;
            Handler.affect_to_char(vch, af);

            af.location = APPLY_DAMROLL;
            Handler.affect_to_char(vch, af);
        }
    }
}

public static void spell_cancellation(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    bool found = false;

    level += 2;

    if ((!Bit.IS_NPC(ch) && Bit.IS_NPC(victim) &&
         !(Bit.IS_AFFECTED(ch, AFF_CHARM) && ch.master == victim)) ||
        (Bit.IS_NPC(ch) && !Bit.IS_NPC(victim)))
    {
        Comm.send_to_char("You failed, try dispel magic.\n\r", ch);
        return;
    }

    

    

    if (check_dispel(level, victim, Lookup.skill_lookup("armor")))
        found = true;

    if (check_dispel(level, victim, Lookup.skill_lookup("bless")))
        found = true;

    if (check_dispel(level, victim, Lookup.skill_lookup("blindness")))
    {
        found = true;
        Comm.act("$n is no longer blinded.", victim, null, null, TO_ROOM);
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("calm")))
    {
        found = true;
        Comm.act("$n no longer looks so peaceful...", victim, null, null,
             TO_ROOM);
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("change sex")))
    {
        found = true;
        Comm.act("$n looks more like $mself again.", victim, null, null, TO_ROOM);
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("charm person")))
    {
        found = true;
        Comm.act("$n regains $s free will.", victim, null, null, TO_ROOM);
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("chill touch")))
    {
        found = true;
        Comm.act("$n looks warmer.", victim, null, null, TO_ROOM);
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("curse")))
        found = true;

    if (check_dispel(level, victim, Lookup.skill_lookup("detect evil")))
        found = true;

    if (check_dispel(level, victim, Lookup.skill_lookup("detect good")))
        found = true;

    if (check_dispel(level, victim, Lookup.skill_lookup("detect hidden")))
        found = true;

    if (check_dispel(level, victim, Lookup.skill_lookup("detect invis")))
        found = true;

    if (check_dispel(level, victim, Lookup.skill_lookup("detect magic")))
        found = true;

    if (check_dispel(level, victim, Lookup.skill_lookup("faerie fire")))
    {
        Comm.act("$n's outline fades.", victim, null, null, TO_ROOM);
        found = true;
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("fly")))
    {
        Comm.act("$n falls to the ground!", victim, null, null, TO_ROOM);
        found = true;
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("frenzy")))
    {
        Comm.act("$n no longer looks so wild.", victim, null, null, TO_ROOM);;
        found = true;
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("giant strength")))
    {
        Comm.act("$n no longer looks so mighty.", victim, null, null, TO_ROOM);
        found = true;
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("haste")))
    {
        Comm.act("$n is no longer moving so quickly.", victim, null, null,
             TO_ROOM);
        found = true;
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("infravision")))
        found = true;

    if (check_dispel(level, victim, Lookup.skill_lookup("invis")))
    {
        Comm.act("$n fades into existance.", victim, null, null, TO_ROOM);
        found = true;
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("mass invis")))
    {
        Comm.act("$n fades into existance.", victim, null, null, TO_ROOM);
        found = true;
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("pass door")))
        found = true;

    if (check_dispel(level, victim, Lookup.skill_lookup("protection evil")))
        found = true;

    if (check_dispel(level, victim, Lookup.skill_lookup("protection good")))
        found = true;

    if (check_dispel(level, victim, Lookup.skill_lookup("sanctuary")))
    {
        Comm.act("The white aura around $n's body vanishes.",
             victim, null, null, TO_ROOM);
        found = true;
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("shield")))
    {
        Comm.act("The shield protecting $n vanishes.", victim, null, null,
             TO_ROOM);
        found = true;
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("sleep")))
        found = true;

    if (check_dispel(level, victim, Lookup.skill_lookup("slow")))
    {
        Comm.act("$n is no longer moving so slowly.", victim, null, null,
             TO_ROOM);
        found = true;
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("stone skin")))
    {
        Comm.act("$n's skin regains its normal texture.", victim, null, null,
             TO_ROOM);
        found = true;
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("weaken")))
    {
        Comm.act("$n looks stronger.", victim, null, null, TO_ROOM);
        found = true;
    }

    if (found)
        Comm.send_to_char("Ok.\n\r", ch);
    else
        Comm.send_to_char("Spell failed.\n\r", ch);
}

public static void spell_cause_light(int sn, int level, CharData ch, object vo, int target)
{
    Fight.damage(ch, (vo as CharData), RomRandom.dice(1, 8) + level / 3, sn, DAM_HARM,
            true);
    return;
}



public static void spell_cause_critical(int sn, int level, CharData ch, object vo, int target)
{
    Fight.damage(ch, (vo as CharData), RomRandom.dice(3, 8) + level - 6, sn, DAM_HARM,
            true);
    return;
}



public static void spell_cause_serious(int sn, int level, CharData ch, object vo, int target)
{
    Fight.damage(ch, (vo as CharData), RomRandom.dice(2, 8) + level / 2, sn, DAM_HARM,
            true);
    return;
}

public static void spell_chain_lightning(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    CharData tmp_vict = null, last_vict = null, next_vict = null;
    bool found;
    int dam;

    

    Comm.act("A lightning bolt leaps from $n's hand and arcs to $N.",
         ch, null, victim, TO_ROOM);
    Comm.act("A lightning bolt leaps from your hand and arcs to $N.",
         ch, null, victim, TO_CHAR);
    Comm.act("A lightning bolt leaps from $n's hand and hits you!",
         ch, null, victim, TO_VICT);

    dam = RomRandom.dice(level, 6);
    if (Magic.saves_spell(level, victim, DAM_LIGHTNING))
        dam /= 3;
    Fight.damage(ch, victim, dam, sn, DAM_LIGHTNING, true);
    last_vict = victim;
    level -= 4;                    

    
    while (level > 0)
    {
        found = false;
        for (tmp_vict = ch.in_room.people;
             tmp_vict != null; tmp_vict = next_vict)
        {
            next_vict = tmp_vict.next_in_room;
            if (!Fight.is_safe_spell(ch, tmp_vict, true) && tmp_vict != last_vict)
            {
                found = true;
                last_vict = tmp_vict;
                Comm.act("The bolt arcs to $n!", tmp_vict, null, null, TO_ROOM);
                Comm.act("The bolt hits you!", tmp_vict, null, null, TO_CHAR);
                dam = RomRandom.dice(level, 6);
                if (Magic.saves_spell(level, tmp_vict, DAM_LIGHTNING))
                    dam /= 3;
                Fight.damage(ch, tmp_vict, dam, sn, DAM_LIGHTNING, true);
                level -= 4;        
            }
        }                        

        if (!found)
        {                        
            if (ch == null)
                return;

            if (last_vict == ch)
            {                    
                Comm.act("The bolt seems to have fizzled out.", ch, null, null,
                     TO_ROOM);
                Comm.act("The bolt grounds out through your body.", ch, null,
                     null, TO_CHAR);
                return;
            }

            last_vict = ch;
            Comm.act("The bolt arcs to $n...whoops!", ch, null, null, TO_ROOM);
            Comm.send_to_char("You are struck by your own lightning!\n\r", ch);
            dam = RomRandom.dice(level, 6);
            if (Magic.saves_spell(level, ch, DAM_LIGHTNING))
                dam /= 3;
            Fight.damage(ch, ch, dam, sn, DAM_LIGHTNING, true);
            level -= 4;            
            if (ch == null)
                return;
        }
        
    }
}


public static void spell_change_sex(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    var af = new AffectData();

    if (Handler.is_affected(victim, sn))
    {
        if (victim == ch)
            Comm.send_to_char("You've already been changed.\n\r", ch);
        else
            Comm.act("$N has already had $s(?) sex changed.", ch, null, victim,
                 TO_CHAR);
        return;
    }
    if (Magic.saves_spell(level, victim, DAM_OTHER))
        return;
    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.duration = 2 * level;
    af.location = APPLY_SEX;
    do
    {
        af.modifier = RomRandom.number_range(0, 2) - victim.sex;
    }
    while (af.modifier == 0);
    af.bitvector = 0;
    Handler.affect_to_char(victim, af);
    Comm.send_to_char("You feel different.\n\r", victim);
    Comm.act("$n doesn't look like $mself anymore...", victim, null, null,
         TO_ROOM);
    return;
}



public static void spell_charm_person(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    var af = new AffectData();

    if (Fight.is_safe(ch, victim))
        return;

    if (victim == ch)
    {
        Comm.send_to_char("You like yourself even better!\n\r", ch);
        return;
    }

    if (Bit.IS_AFFECTED(victim, AFF_CHARM)
        || Bit.IS_AFFECTED(ch, AFF_CHARM)
        || level < victim.level || Bit.IS_SET(victim.imm_flags, IMM_CHARM)
        || Magic.saves_spell(level, victim, DAM_CHARM))
        return;


    if (Bit.IS_SET(victim.in_room.room_flags, ROOM_LAW))
    {
        Comm.send_to_char("The mayor does not allow charming in the city limits.\n\r", ch);
        return;
    }

    if (victim.master != null)
        ActComm.stop_follower(victim);
    ActComm.add_follower(victim, ch);
    victim.leader = ch;
    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.duration = RomRandom.number_fuzzy(level / 4);
    af.location = 0;
    af.modifier = 0;
    af.bitvector = AFF_CHARM;
    Handler.affect_to_char(victim, af);
    Comm.act("Isn't $n just so nice?", ch, null, victim, TO_VICT);
    if (ch != victim)
        Comm.act("$N looks at you with adoring eyes.", ch, null, victim, TO_CHAR);
    return;
}



public static void spell_chill_touch(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    int[] dam_each = {
        0,
        0, 0, 6, 7, 8, 9, 12, 13, 13, 13,
        14, 14, 14, 15, 15, 15, 16, 16, 16, 17,
        17, 17, 18, 18, 18, 19, 19, 19, 20, 20,
        20, 21, 21, 21, 22, 22, 22, 23, 23, 23,
        24, 24, 24, 25, 25, 25, 26, 26, 26, 27
    };
    var af = new AffectData();
    int dam;

    level = Bit.UMIN(level, dam_each.Length - 1);
    level = Bit.UMAX(0, level);
    dam = RomRandom.number_range(dam_each[level] / 2, dam_each[level] * 2);
    if (!Magic.saves_spell(level, victim, DAM_COLD))
    {
        Comm.act("$n turns blue and shivers.", victim, null, null, TO_ROOM);
        af.where = TO_AFFECTS;
        af.type = sn;
        af.level = level;
        af.duration = 6;
        af.location = APPLY_STR;
        af.modifier = -1;
        af.bitvector = 0;
        Handler.affect_join(victim, af);
    }
    else
    {
        dam /= 2;
    }

    Fight.damage(ch, victim, dam, sn, DAM_COLD, true);
    return;
}



public static void spell_colour_spray(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    int[] dam_each = {
        0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        30, 35, 40, 45, 50, 55, 55, 55, 56, 57,
        58, 58, 59, 60, 61, 61, 62, 63, 64, 64,
        65, 66, 67, 67, 68, 69, 70, 70, 71, 72,
        73, 73, 74, 75, 76, 76, 77, 78, 79, 79
    };
    int dam;

    level = Bit.UMIN(level, dam_each.Length - 1);
    level = Bit.UMAX(0, level);
    dam = RomRandom.number_range(dam_each[level] / 2, dam_each[level] * 2);
    if (Magic.saves_spell(level, victim, DAM_LIGHT))
        dam /= 2;
    else
        spell_blindness (Lookup.skill_lookup("blindness"),
                         level / 2, ch, victim, TARGET_CHAR);

    Fight.damage(ch, victim, dam, sn, DAM_LIGHT, true);
    return;
}



public static void spell_continual_light(int sn, int level, CharData ch, object vo, int target)
{
    ObjData light = null;

    if (target_name[0] != '\0')
    {                            
        light = Handler.get_obj_carry(ch, target_name, ch);

        if (light == null)
        {
            Comm.send_to_char("You don't see that here.\n\r", ch);
            return;
        }

        if (Bit.IS_OBJ_STAT(light, ITEM_GLOW))
        {
            Comm.act("$p is already glowing.", ch, light, null, TO_CHAR);
            return;
        }

        Bit.SET_BIT(ref light.extra_flags, ITEM_GLOW);
        Comm.act("$p glows with a white light.", ch, light, null, TO_ALL);
        return;
    }

    light = Db.create_object(Handler.get_obj_index(OBJ_VNUM_LIGHT_BALL), 0);
    Handler.obj_to_room(light, ch.in_room);
    Comm.act("$n twiddles $s thumbs and $p appears.", ch, light, null, TO_ROOM);
    Comm.act("You twiddle your thumbs and $p appears.", ch, light, null, TO_CHAR);
    return;
}



public static void spell_control_weather(int sn, int level, CharData ch, object vo, int target)
{
    if (!RomString.str_cmp(target_name, "better"))
        Game.weather_info.change += RomRandom.dice(level / 3, 4);
    else if (!RomString.str_cmp(target_name, "worse"))
        Game.weather_info.change -= RomRandom.dice(level / 3, 4);
    else
        Comm.send_to_char("Do you want it to get better or worse?\n\r", ch);

    Comm.send_to_char("Ok.\n\r", ch);
    return;
}



public static void spell_create_food(int sn, int level, CharData ch, object vo, int target)
{
    ObjData mushroom = null;

    mushroom = Db.create_object(Handler.get_obj_index(OBJ_VNUM_MUSHROOM), 0);
    mushroom.value[0] = level / 2;
    mushroom.value[1] = level;
    Handler.obj_to_room(mushroom, ch.in_room);
    Comm.act("$p suddenly appears.", ch, mushroom, null, TO_ROOM);
    Comm.act("$p suddenly appears.", ch, mushroom, null, TO_CHAR);
    return;
}

public static void spell_create_rose(int sn, int level, CharData ch, object vo, int target)
{
    ObjData rose = null;
    rose = Db.create_object(Handler.get_obj_index(OBJ_VNUM_ROSE), 0);
    Comm.act("$n has created a beautiful red rose.", ch, rose, null, TO_ROOM);
    Comm.send_to_char("You create a beautiful red rose.\n\r", ch);
    Handler.obj_to_char(rose, ch);
    return;
}

public static void spell_create_spring(int sn, int level, CharData ch, object vo, int target)
{
    ObjData spring = null;

    spring = Db.create_object(Handler.get_obj_index(OBJ_VNUM_SPRING), 0);
    spring.timer = level;
    Handler.obj_to_room(spring, ch.in_room);
    Comm.act("$p flows from the ground.", ch, spring, null, TO_ROOM);
    Comm.act("$p flows from the ground.", ch, spring, null, TO_CHAR);
    return;
}



public static void spell_create_water(int sn, int level, CharData ch, object vo, int target)
{
    ObjData obj = vo as ObjData;
    int water;

    if (obj.item_type != ITEM_DRINK_CON)
    {
        Comm.send_to_char("It is unable to hold water.\n\r", ch);
        return;
    }

    if (obj.value[2] != LIQ_WATER && obj.value[1] != 0)
    {
        Comm.send_to_char("It contains some other liquid.\n\r", ch);
        return;
    }

    water = Bit.UMIN(level * (Game.weather_info.sky >= SKY_RAINING ? 4 : 2),
                  obj.value[0] - obj.value[1]);

    if (water > 0)
    {
        obj.value[2] = LIQ_WATER;
        obj.value[1] += water;
        if (!Handler.is_name("water", obj.name))
        {
            string buf;

            buf = RomString.sprintf( "%s water", obj.name);
            
            obj.name = (buf);
        }
        Comm.act("$p is filled.", ch, obj, null, TO_CHAR);
    }

    return;
}



public static void spell_cure_blindness(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;

    if (!Handler.is_affected(victim, Gsn.blindness))
    {
        if (victim == ch)
            Comm.send_to_char("You aren't blind.\n\r", ch);
        else
            Comm.act("$N doesn't appear to be blinded.", ch, null, victim,
                 TO_CHAR);
        return;
    }

    if (check_dispel(level, victim, Gsn.blindness))
    {
        Comm.send_to_char("Your vision returns!\n\r", victim);
        Comm.act("$n is no longer blinded.", victim, null, null, TO_ROOM);
    }
    else
        Comm.send_to_char("Spell failed.\n\r", ch);
}



public static void spell_cure_critical(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    int heal;

    heal = RomRandom.dice(3, 8) + level - 6;
    victim.hit = Bit.UMIN(victim.hit + heal, victim.max_hit);
    Fight.update_pos(victim);
    Comm.send_to_char("You feel better!\n\r", victim);
    if (ch != victim)
        Comm.send_to_char("Ok.\n\r", ch);
    return;
}


public static void spell_cure_disease(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;

    if (!Handler.is_affected(victim, Gsn.plague))
    {
        if (victim == ch)
            Comm.send_to_char("You aren't ill.\n\r", ch);
        else
            Comm.act("$N doesn't appear to be diseased.", ch, null, victim,
                 TO_CHAR);
        return;
    }

    if (check_dispel(level, victim, Gsn.plague))
    {
        Comm.send_to_char("Your sores vanish.\n\r", victim);
        Comm.act("$n looks relieved as $s sores vanish.", victim, null, null,
             TO_ROOM);
    }
    else
        Comm.send_to_char("Spell failed.\n\r", ch);
}



public static void spell_cure_light(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    int heal;

    heal = RomRandom.dice(1, 8) + level / 3;
    victim.hit = Bit.UMIN(victim.hit + heal, victim.max_hit);
    Fight.update_pos(victim);
    Comm.send_to_char("You feel better!\n\r", victim);
    if (ch != victim)
        Comm.send_to_char("Ok.\n\r", ch);
    return;
}



public static void spell_cure_poison(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;

    if (!Handler.is_affected(victim, Gsn.poison))
    {
        if (victim == ch)
            Comm.send_to_char("You aren't poisoned.\n\r", ch);
        else
            Comm.act("$N doesn't appear to be poisoned.", ch, null, victim,
                 TO_CHAR);
        return;
    }

    if (check_dispel(level, victim, Gsn.poison))
    {
        Comm.send_to_char("A warm feeling runs through your body.\n\r", victim);
        Comm.act("$n looks much better.", victim, null, null, TO_ROOM);
    }
    else
        Comm.send_to_char("Spell failed.\n\r", ch);
}

public static void spell_cure_serious(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    int heal;

    heal = RomRandom.dice(2, 8) + level / 2;
    victim.hit = Bit.UMIN(victim.hit + heal, victim.max_hit);
    Fight.update_pos(victim);
    Comm.send_to_char("You feel better!\n\r", victim);
    if (ch != victim)
        Comm.send_to_char("Ok.\n\r", ch);
    return;
}



public static void spell_curse(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = null;
    ObjData obj = null;
    var af = new AffectData();

    
    if (target == TARGET_OBJ)
    {
        obj = (vo as ObjData);
        if (Bit.IS_OBJ_STAT(obj, ITEM_EVIL))
        {
            Comm.act("$p is already filled with evil.", ch, obj, null, TO_CHAR);
            return;
        }

        if (Bit.IS_OBJ_STAT(obj, ITEM_BLESS))
        {
            AffectData paf = null;

            paf = Handler.affect_find(obj.affected, Lookup.skill_lookup("bless"));
            if (!saves_dispel(level, paf != null ? paf.level : obj.level, 0))
            {
                if (paf != null)
                    Handler.affect_remove_obj(obj, paf);
                Comm.act("$p glows with a red aura.", ch, obj, null, TO_ALL);
                Bit.REMOVE_BIT(ref obj.extra_flags, ITEM_BLESS);
                return;
            }
            else
            {
                Comm.act("The holy aura of $p is too powerful for you to overcome.",
                     ch, obj, null, TO_CHAR);
                return;
            }
        }

        af.where = TO_OBJECT;
        af.type = sn;
        af.level = level;
        af.duration = 2 * level;
        af.location = APPLY_SAVES;
        af.modifier = +1;
        af.bitvector = ITEM_EVIL;
        Handler.affect_to_obj(obj, af);

        Comm.act("$p glows with a malevolent aura.", ch, obj, null, TO_ALL);

        if (obj.wear_loc != WEAR_NONE)
            ch.saving_throw += 1;
        return;
    }

    
    victim = (vo as CharData);

    if (Bit.IS_AFFECTED(victim, AFF_CURSE)
        || Magic.saves_spell(level, victim, DAM_NEGATIVE))
        return;
    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.duration = 2 * level;
    af.location = APPLY_HITROLL;
    af.modifier = -1 * (level / 8);
    af.bitvector = AFF_CURSE;
    Handler.affect_to_char(victim, af);

    af.location = APPLY_SAVING_SPELL;
    af.modifier = level / 8;
    Handler.affect_to_char(victim, af);

    Comm.send_to_char("You feel unclean.\n\r", victim);
    if (ch != victim)
        Comm.act("$N looks very uncomfortable.", ch, null, victim, TO_CHAR);
    return;
}



public static void spell_demonfire(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    int dam;

    if (!Bit.IS_NPC(ch) && !Bit.IS_EVIL(ch))
    {
        victim = ch;
        Comm.send_to_char("The demons turn upon you!\n\r", ch);
    }

    ch.alignment = Bit.UMAX(-1000, ch.alignment - 50);

    if (victim != ch)
    {
        Comm.act("$n calls forth the demons of Hell upon $N!",
             ch, null, victim, TO_ROOM);
        Comm.act("$n has assailed you with the demons of Hell!",
             ch, null, victim, TO_VICT);
        Comm.send_to_char("You conjure forth the demons of hell!\n\r", ch);
    }
    dam = RomRandom.dice(level, 10);
    if (Magic.saves_spell(level, victim, DAM_NEGATIVE))
        dam /= 2;
    Fight.damage(ch, victim, dam, sn, DAM_NEGATIVE, true);
    spell_curse (Gsn.curse, 3 * level / 4, ch, victim, TARGET_CHAR);
}

public static void spell_detect_evil(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    var af = new AffectData();

    if (Bit.IS_AFFECTED(victim, AFF_DETECT_EVIL))
    {
        if (victim == ch)
            Comm.send_to_char("You can already sense evil.\n\r", ch);
        else
            Comm.act("$N can already detect evil.", ch, null, victim, TO_CHAR);
        return;
    }
    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.duration = level;
    af.modifier = 0;
    af.location = APPLY_NONE;
    af.bitvector = AFF_DETECT_EVIL;
    Handler.affect_to_char(victim, af);
    Comm.send_to_char("Your eyes tingle.\n\r", victim);
    if (ch != victim)
        Comm.send_to_char("Ok.\n\r", ch);
    return;
}


public static void spell_detect_good(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    var af = new AffectData();

    if (Bit.IS_AFFECTED(victim, AFF_DETECT_GOOD))
    {
        if (victim == ch)
            Comm.send_to_char("You can already sense good.\n\r", ch);
        else
            Comm.act("$N can already detect good.", ch, null, victim, TO_CHAR);
        return;
    }
    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.duration = level;
    af.modifier = 0;
    af.location = APPLY_NONE;
    af.bitvector = AFF_DETECT_GOOD;
    Handler.affect_to_char(victim, af);
    Comm.send_to_char("Your eyes tingle.\n\r", victim);
    if (ch != victim)
        Comm.send_to_char("Ok.\n\r", ch);
    return;
}



public static void spell_detect_hidden(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    var af = new AffectData();

    if (Bit.IS_AFFECTED(victim, AFF_DETECT_HIDDEN))
    {
        if (victim == ch)
            Comm.send_to_char("You are already as alert as you can be. \n\r", ch);
        else
            Comm.act("$N can already sense hidden lifeforms.", ch, null, victim,
                 TO_CHAR);
        return;
    }
    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.duration = level;
    af.location = APPLY_NONE;
    af.modifier = 0;
    af.bitvector = AFF_DETECT_HIDDEN;
    Handler.affect_to_char(victim, af);
    Comm.send_to_char("Your awareness improves.\n\r", victim);
    if (ch != victim)
        Comm.send_to_char("Ok.\n\r", ch);
    return;
}



public static void spell_detect_invis(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    var af = new AffectData();

    if (Bit.IS_AFFECTED(victim, AFF_DETECT_INVIS))
    {
        if (victim == ch)
            Comm.send_to_char("You can already see invisible.\n\r", ch);
        else
            Comm.act("$N can already see invisible things.", ch, null, victim,
                 TO_CHAR);
        return;
    }

    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.duration = level;
    af.modifier = 0;
    af.location = APPLY_NONE;
    af.bitvector = AFF_DETECT_INVIS;
    Handler.affect_to_char(victim, af);
    Comm.send_to_char("Your eyes tingle.\n\r", victim);
    if (ch != victim)
        Comm.send_to_char("Ok.\n\r", ch);
    return;
}



public static void spell_detect_magic(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    var af = new AffectData();

    if (Bit.IS_AFFECTED(victim, AFF_DETECT_MAGIC))
    {
        if (victim == ch)
            Comm.send_to_char("You can already sense magical auras.\n\r", ch);
        else
            Comm.act("$N can already detect magic.", ch, null, victim, TO_CHAR);
        return;
    }

    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.duration = level;
    af.modifier = 0;
    af.location = APPLY_NONE;
    af.bitvector = AFF_DETECT_MAGIC;
    Handler.affect_to_char(victim, af);
    Comm.send_to_char("Your eyes tingle.\n\r", victim);
    if (ch != victim)
        Comm.send_to_char("Ok.\n\r", ch);
    return;
}



public static void spell_detect_poison(int sn, int level, CharData ch, object vo, int target)
{
    ObjData obj = vo as ObjData;

    if (obj.item_type == ITEM_DRINK_CON || obj.item_type == ITEM_FOOD)
    {
        if (obj.value[3] != 0)
            Comm.send_to_char("You smell poisonous fumes.\n\r", ch);
        else
            Comm.send_to_char("It looks delicious.\n\r", ch);
    }
    else
    {
        Comm.send_to_char("It doesn't look poisoned.\n\r", ch);
    }

    return;
}



public static void spell_dispel_evil(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    int dam;

    if (!Bit.IS_NPC(ch) && Bit.IS_EVIL(ch))
        victim = ch;

    if (Bit.IS_GOOD(victim))
    {
        Comm.act("Mota protects $N.", ch, null, victim, TO_ROOM);
        return;
    }

    if (Bit.IS_NEUTRAL(victim))
    {
        Comm.act("$N does not seem to be affected.", ch, null, victim, TO_CHAR);
        return;
    }

    if (victim.hit > (ch.level * 4))
        dam = RomRandom.dice(level, 4);
    else
        dam = Bit.UMAX(victim.hit, RomRandom.dice(level, 4));
    if (Magic.saves_spell(level, victim, DAM_HOLY))
        dam /= 2;
    Fight.damage(ch, victim, dam, sn, DAM_HOLY, true);
    return;
}


public static void spell_dispel_good(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    int dam;

    if (!Bit.IS_NPC(ch) && Bit.IS_GOOD(ch))
        victim = ch;

    if (Bit.IS_EVIL(victim))
    {
        Comm.act("$N is protected by $S evil.", ch, null, victim, TO_ROOM);
        return;
    }

    if (Bit.IS_NEUTRAL(victim))
    {
        Comm.act("$N does not seem to be affected.", ch, null, victim, TO_CHAR);
        return;
    }

    if (victim.hit > (ch.level * 4))
        dam = RomRandom.dice(level, 4);
    else
        dam = Bit.UMAX(victim.hit, RomRandom.dice(level, 4));
    if (Magic.saves_spell(level, victim, DAM_NEGATIVE))
        dam /= 2;
    Fight.damage(ch, victim, dam, sn, DAM_NEGATIVE, true);
    return;
}




public static void spell_dispel_magic(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    bool found = false;

    if (Magic.saves_spell(level, victim, DAM_OTHER))
    {
        Comm.send_to_char("You feel a brief tingling sensation.\n\r", victim);
        Comm.send_to_char("You failed.\n\r", ch);
        return;
    }

    

    if (check_dispel(level, victim, Lookup.skill_lookup("armor")))
        found = true;

    if (check_dispel(level, victim, Lookup.skill_lookup("bless")))
        found = true;

    if (check_dispel(level, victim, Lookup.skill_lookup("blindness")))
    {
        found = true;
        Comm.act("$n is no longer blinded.", victim, null, null, TO_ROOM);
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("calm")))
    {
        found = true;
        Comm.act("$n no longer looks so peaceful...", victim, null, null,
             TO_ROOM);
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("change sex")))
    {
        found = true;
        Comm.act("$n looks more like $mself again.", victim, null, null, TO_ROOM);
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("charm person")))
    {
        found = true;
        Comm.act("$n regains $s free will.", victim, null, null, TO_ROOM);
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("chill touch")))
    {
        found = true;
        Comm.act("$n looks warmer.", victim, null, null, TO_ROOM);
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("curse")))
        found = true;

    if (check_dispel(level, victim, Lookup.skill_lookup("detect evil")))
        found = true;

    if (check_dispel(level, victim, Lookup.skill_lookup("detect good")))
        found = true;

    if (check_dispel(level, victim, Lookup.skill_lookup("detect hidden")))
        found = true;

    if (check_dispel(level, victim, Lookup.skill_lookup("detect invis")))
        found = true;

    if (check_dispel(level, victim, Lookup.skill_lookup("detect magic")))
        found = true;

    if (check_dispel(level, victim, Lookup.skill_lookup("faerie fire")))
    {
        Comm.act("$n's outline fades.", victim, null, null, TO_ROOM);
        found = true;
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("fly")))
    {
        Comm.act("$n falls to the ground!", victim, null, null, TO_ROOM);
        found = true;
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("frenzy")))
    {
        Comm.act("$n no longer looks so wild.", victim, null, null, TO_ROOM);;
        found = true;
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("giant strength")))
    {
        Comm.act("$n no longer looks so mighty.", victim, null, null, TO_ROOM);
        found = true;
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("haste")))
    {
        Comm.act("$n is no longer moving so quickly.", victim, null, null,
             TO_ROOM);
        found = true;
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("infravision")))
        found = true;

    if (check_dispel(level, victim, Lookup.skill_lookup("invis")))
    {
        Comm.act("$n fades into existance.", victim, null, null, TO_ROOM);
        found = true;
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("mass invis")))
    {
        Comm.act("$n fades into existance.", victim, null, null, TO_ROOM);
        found = true;
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("pass door")))
        found = true;


    if (check_dispel(level, victim, Lookup.skill_lookup("protection evil")))
        found = true;

    if (check_dispel(level, victim, Lookup.skill_lookup("protection good")))
        found = true;

    if (check_dispel(level, victim, Lookup.skill_lookup("sanctuary")))
    {
        Comm.act("The white aura around $n's body vanishes.",
             victim, null, null, TO_ROOM);
        found = true;
    }

    if (Bit.IS_AFFECTED(victim, AFF_SANCTUARY)
        && !saves_dispel(level, victim.level, -1)
        && !Handler.is_affected(victim, Lookup.skill_lookup("sanctuary")))
    {
        Bit.REMOVE_BIT(ref victim.affected_by, AFF_SANCTUARY);
        Comm.act("The white aura around $n's body vanishes.",
             victim, null, null, TO_ROOM);
        found = true;
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("shield")))
    {
        Comm.act("The shield protecting $n vanishes.", victim, null, null,
             TO_ROOM);
        found = true;
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("sleep")))
        found = true;

    if (check_dispel(level, victim, Lookup.skill_lookup("slow")))
    {
        Comm.act("$n is no longer moving so slowly.", victim, null, null,
             TO_ROOM);
        found = true;
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("stone skin")))
    {
        Comm.act("$n's skin regains its normal texture.", victim, null, null,
             TO_ROOM);
        found = true;
    }

    if (check_dispel(level, victim, Lookup.skill_lookup("weaken")))
    {
        Comm.act("$n looks stronger.", victim, null, null, TO_ROOM);
        found = true;
    }

    if (found)
        Comm.send_to_char("Ok.\n\r", ch);
    else
        Comm.send_to_char("Spell failed.\n\r", ch);
    return;
}

public static void spell_earthquake(int sn, int level, CharData ch, object vo, int target)
{
    CharData vch = null;
    CharData vch_next = null;

    Comm.send_to_char("The earth trembles beneath your feet!\n\r", ch);
    Comm.act("$n makes the earth tremble and shiver.", ch, null, null, TO_ROOM);

    for (vch = Game.char_list; vch != null; vch = vch_next)
    {
        vch_next = vch.next;
        if (vch.in_room == null)
            continue;
        if (vch.in_room == ch.in_room)
        {
            if (vch != ch && !Fight.is_safe_spell(ch, vch, true))
            {
                if (Bit.IS_AFFECTED(vch, AFF_FLYING))
                    Fight.damage(ch, vch, 0, sn, DAM_BASH, true);
                else
                    Fight.damage(ch, vch, level + RomRandom.dice(2, 8), sn, DAM_BASH, true);
            }
            continue;
        }

        if (vch.in_room.area == ch.in_room.area)
            Comm.send_to_char("The earth trembles and shivers.\n\r", vch);
    }

    return;
}

public static void spell_enchant_armor(int sn, int level, CharData ch, object vo, int target)
{
    ObjData obj = vo as ObjData;
    AffectData paf = null;
    int result, fail;
    int ac_bonus, added;
    bool ac_found = false;

    if (obj.item_type != ITEM_ARMOR)
    {
        Comm.send_to_char("That isn't an armor.\n\r", ch);
        return;
    }

    if (obj.wear_loc != -1)
    {
        Comm.send_to_char("The item must be carried to be enchanted.\n\r", ch);
        return;
    }

    
    ac_bonus = 0;
    fail = 25;                    

    

    if (!obj.enchanted)
        for (paf = obj.pIndexData.affected; paf != null; paf = paf.next)
        {
            if (paf.location == APPLY_AC)
            {
                ac_bonus = paf.modifier;
                ac_found = true;
                fail += 5 * (ac_bonus * ac_bonus);
            }

            else                
                fail += 20;
        }

    for (paf = obj.affected; paf != null; paf = paf.next)
    {
        if (paf.location == APPLY_AC)
        {
            ac_bonus = paf.modifier;
            ac_found = true;
            fail += 5 * (ac_bonus * ac_bonus);
        }

        else                    
            fail += 20;
    }

    
    fail -= level;

    if (Bit.IS_OBJ_STAT(obj, ITEM_BLESS))
        fail -= 15;
    if (Bit.IS_OBJ_STAT(obj, ITEM_GLOW))
        fail -= 5;

    fail = Bit.URANGE(5, fail, 85);

    result = RomRandom.number_percent();

    
    if (result < (fail / 5))
    {                            
        Comm.act("$p flares blindingly... and evaporates!", ch, obj, null,
             TO_CHAR);
        Comm.act("$p flares blindingly... and evaporates!", ch, obj, null,
             TO_ROOM);
        Handler.extract_obj(obj);
        return;
    }

    if (result < (fail / 3))
    {                            
        AffectData paf_next = null;

        Comm.act("$p glows brightly, then fades...oops.", ch, obj, null, TO_CHAR);
        Comm.act("$p glows brightly, then fades.", ch, obj, null, TO_ROOM);
        obj.enchanted = true;

        
        for (paf = obj.affected; paf != null; paf = paf_next)
        {
            paf_next = paf.next;
            
        }
        obj.affected = null;

        
        obj.extra_flags = 0;
        return;
    }

    if (result <= fail)
    {                            
        Comm.send_to_char("Nothing seemed to happen.\n\r", ch);
        return;
    }

    
    if (!obj.enchanted)
    {
        AffectData af_new = null;
        obj.enchanted = true;

        for (paf = obj.pIndexData.affected; paf != null; paf = paf.next)
        {
            af_new = Recycle.new_affect();

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

    if (result <= (90 - level / 5))
    {                            
        Comm.act("$p shimmers with a gold aura.", ch, obj, null, TO_CHAR);
        Comm.act("$p shimmers with a gold aura.", ch, obj, null, TO_ROOM);
        Bit.SET_BIT(ref obj.extra_flags, ITEM_MAGIC);
        added = -1;
    }

    else
    {                            

        Comm.act("$p glows a brillant gold!", ch, obj, null, TO_CHAR);
        Comm.act("$p glows a brillant gold!", ch, obj, null, TO_ROOM);
        Bit.SET_BIT(ref obj.extra_flags, ITEM_MAGIC);
        Bit.SET_BIT(ref obj.extra_flags, ITEM_GLOW);
        added = -2;
    }

    

    if (obj.level < LEVEL_HERO)
        obj.level = Bit.UMIN(LEVEL_HERO - 1, obj.level + 1);

    if (ac_found)
    {
        for (paf = obj.affected; paf != null; paf = paf.next)
        {
            if (paf.location == APPLY_AC)
            {
                paf.type = sn;
                paf.modifier += added;
                paf.level = Bit.UMAX(paf.level, level);
            }
        }
    }
    else
    {                            

        paf = Recycle.new_affect();

        paf.where = TO_OBJECT;
        paf.type = sn;
        paf.level = level;
        paf.duration = -1;
        paf.location = APPLY_AC;
        paf.modifier = added;
        paf.bitvector = 0;
        paf.next = obj.affected;
        obj.affected = paf;
    }

}




public static void spell_enchant_weapon(int sn, int level, CharData ch, object vo, int target)
{
    ObjData obj = vo as ObjData;
    AffectData paf = null;
    int result, fail;
    int hit_bonus, dam_bonus, added;
    bool hit_found = false, dam_found = false;

    if (obj.item_type != ITEM_WEAPON)
    {
        Comm.send_to_char("That isn't a weapon.\n\r", ch);
        return;
    }

    if (obj.wear_loc != -1)
    {
        Comm.send_to_char("The item must be carried to be enchanted.\n\r", ch);
        return;
    }

    
    hit_bonus = 0;
    dam_bonus = 0;
    fail = 25;                    

    

    if (!obj.enchanted)
        for (paf = obj.pIndexData.affected; paf != null; paf = paf.next)
        {
            if (paf.location == APPLY_HITROLL)
            {
                hit_bonus = paf.modifier;
                hit_found = true;
                fail += 2 * (hit_bonus * hit_bonus);
            }

            else if (paf.location == APPLY_DAMROLL)
            {
                dam_bonus = paf.modifier;
                dam_found = true;
                fail += 2 * (dam_bonus * dam_bonus);
            }

            else                
                fail += 25;
        }

    for (paf = obj.affected; paf != null; paf = paf.next)
    {
        if (paf.location == APPLY_HITROLL)
        {
            hit_bonus = paf.modifier;
            hit_found = true;
            fail += 2 * (hit_bonus * hit_bonus);
        }

        else if (paf.location == APPLY_DAMROLL)
        {
            dam_bonus = paf.modifier;
            dam_found = true;
            fail += 2 * (dam_bonus * dam_bonus);
        }

        else                    
            fail += 25;
    }

    
    fail -= 3 * level / 2;

    if (Bit.IS_OBJ_STAT(obj, ITEM_BLESS))
        fail -= 15;
    if (Bit.IS_OBJ_STAT(obj, ITEM_GLOW))
        fail -= 5;

    fail = Bit.URANGE(5, fail, 95);

    result = RomRandom.number_percent();

    
    if (result < (fail / 5))
    {                            
        Comm.act("$p shivers violently and explodes!", ch, obj, null, TO_CHAR);
        Comm.act("$p shivers violently and explodeds!", ch, obj, null, TO_ROOM);
        Handler.extract_obj(obj);
        return;
    }

    if (result < (fail / 2))
    {                            
        AffectData paf_next = null;

        Comm.act("$p glows brightly, then fades...oops.", ch, obj, null, TO_CHAR);
        Comm.act("$p glows brightly, then fades.", ch, obj, null, TO_ROOM);
        obj.enchanted = true;

        
        for (paf = obj.affected; paf != null; paf = paf_next)
        {
            paf_next = paf.next;
            
        }
        obj.affected = null;

        
        obj.extra_flags = 0;
        return;
    }

    if (result <= fail)
    {                            
        Comm.send_to_char("Nothing seemed to happen.\n\r", ch);
        return;
    }

    
    if (!obj.enchanted)
    {
        AffectData af_new = null;
        obj.enchanted = true;

        for (paf = obj.pIndexData.affected; paf != null; paf = paf.next)
        {
            af_new = Recycle.new_affect();

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

    if (result <= (100 - level / 5))
    {                            
        Comm.act("$p glows blue.", ch, obj, null, TO_CHAR);
        Comm.act("$p glows blue.", ch, obj, null, TO_ROOM);
        Bit.SET_BIT(ref obj.extra_flags, ITEM_MAGIC);
        added = 1;
    }

    else
    {                            

        Comm.act("$p glows a brillant blue!", ch, obj, null, TO_CHAR);
        Comm.act("$p glows a brillant blue!", ch, obj, null, TO_ROOM);
        Bit.SET_BIT(ref obj.extra_flags, ITEM_MAGIC);
        Bit.SET_BIT(ref obj.extra_flags, ITEM_GLOW);
        added = 2;
    }

    

    if (obj.level < LEVEL_HERO - 1)
        obj.level = Bit.UMIN(LEVEL_HERO - 1, obj.level + 1);

    if (dam_found)
    {
        for (paf = obj.affected; paf != null; paf = paf.next)
        {
            if (paf.location == APPLY_DAMROLL)
            {
                paf.type = sn;
                paf.modifier += added;
                paf.level = Bit.UMAX(paf.level, level);
                if (paf.modifier > 4)
                    Bit.SET_BIT(ref obj.extra_flags, ITEM_HUM);
            }
        }
    }
    else
    {                            

        paf = Recycle.new_affect();

        paf.where = TO_OBJECT;
        paf.type = sn;
        paf.level = level;
        paf.duration = -1;
        paf.location = APPLY_DAMROLL;
        paf.modifier = added;
        paf.bitvector = 0;
        paf.next = obj.affected;
        obj.affected = paf;
    }

    if (hit_found)
    {
        for (paf = obj.affected; paf != null; paf = paf.next)
        {
            if (paf.location == APPLY_HITROLL)
            {
                paf.type = sn;
                paf.modifier += added;
                paf.level = Bit.UMAX(paf.level, level);
                if (paf.modifier > 4)
                    Bit.SET_BIT(ref obj.extra_flags, ITEM_HUM);
            }
        }
    }
    else
    {                            

        paf = Recycle.new_affect();

        paf.type = sn;
        paf.level = level;
        paf.duration = -1;
        paf.location = APPLY_HITROLL;
        paf.modifier = added;
        paf.bitvector = 0;
        paf.next = obj.affected;
        obj.affected = paf;
    }

}




public static void spell_energy_drain(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    int dam;

    if (victim != ch)
        ch.alignment = Bit.UMAX(-1000, ch.alignment - 50);

    if (Magic.saves_spell(level, victim, DAM_NEGATIVE))
    {
        Comm.send_to_char("You feel a momentary chill.\n\r", victim);
        return;
    }


    if (victim.level <= 2)
    {
        dam = ch.hit + 1;
    }
    else
    {
        Update.gain_exp(victim, 0 - RomRandom.number_range(level / 2, 3 * level / 2));
        victim.mana /= 2;
        victim.move /= 2;
        dam = RomRandom.dice(1, level);
        ch.hit += dam;
    }

    Comm.send_to_char("You feel your life slipping away!\n\r", victim);
    Comm.send_to_char("Wow....what a rush!\n\r", ch);
    Fight.damage(ch, victim, dam, sn, DAM_NEGATIVE, true);

    return;
}



public static void spell_fireball(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    int[] dam_each = {
        0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 30, 35, 40, 45, 50, 55,
        60, 65, 70, 75, 80, 82, 84, 86, 88, 90,
        92, 94, 96, 98, 100, 102, 104, 106, 108, 110,
        112, 114, 116, 118, 120, 122, 124, 126, 128, 130
    };
    int dam;

    level = Bit.UMIN(level, dam_each.Length - 1);
    level = Bit.UMAX(0, level);
    dam = RomRandom.number_range(dam_each[level] / 2, dam_each[level] * 2);
    if (Magic.saves_spell(level, victim, DAM_FIRE))
        dam /= 2;
    Fight.damage(ch, victim, dam, sn, DAM_FIRE, true);
    return;
}


public static void spell_fireproof(int sn, int level, CharData ch, object vo, int target)
{
    ObjData obj = vo as ObjData;
    var af = new AffectData();

    if (Bit.IS_OBJ_STAT(obj, ITEM_BURN_PROOF))
    {
        Comm.act("$p is already protected from burning.", ch, obj, null, TO_CHAR);
        return;
    }

    af.where = TO_OBJECT;
    af.type = sn;
    af.level = level;
    af.duration = RomRandom.number_fuzzy(level / 4);
    af.location = APPLY_NONE;
    af.modifier = 0;
    af.bitvector = ITEM_BURN_PROOF;

    Handler.affect_to_obj(obj, af);

    Comm.act("You protect $p from fire.", ch, obj, null, TO_CHAR);
    Comm.act("$p is surrounded by a protective aura.", ch, obj, null, TO_ROOM);
}



public static void spell_flamestrike(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    int dam;

    dam = RomRandom.dice(6 + level / 2, 8);
    if (Magic.saves_spell(level, victim, DAM_FIRE))
        dam /= 2;
    Fight.damage(ch, victim, dam, sn, DAM_FIRE, true);
    return;
}



public static void spell_faerie_fire(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    var af = new AffectData();

    if (Bit.IS_AFFECTED(victim, AFF_FAERIE_FIRE))
        return;
    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.duration = level;
    af.location = APPLY_AC;
    af.modifier = 2 * level;
    af.bitvector = AFF_FAERIE_FIRE;
    Handler.affect_to_char(victim, af);
    Comm.send_to_char("You are surrounded by a pink outline.\n\r", victim);
    Comm.act("$n is surrounded by a pink outline.", victim, null, null, TO_ROOM);
    return;
}



public static void spell_faerie_fog(int sn, int level, CharData ch, object vo, int target)
{
    CharData ich = null;

    Comm.act("$n conjures a cloud of purple smoke.", ch, null, null, TO_ROOM);
    Comm.send_to_char("You conjure a cloud of purple smoke.\n\r", ch);

    for (ich = ch.in_room.people; ich != null; ich = ich.next_in_room)
    {
        if (ich.invis_level > 0)
            continue;

        if (ich == ch || Magic.saves_spell(level, ich, DAM_OTHER))
            continue;

        Handler.affect_strip(ich, Gsn.invis);
        Handler.affect_strip(ich, Gsn.mass_invis);
        Handler.affect_strip(ich, Gsn.sneak);
        Bit.REMOVE_BIT(ref ich.affected_by, AFF_HIDE);
        Bit.REMOVE_BIT(ref ich.affected_by, AFF_INVISIBLE);
        Bit.REMOVE_BIT(ref ich.affected_by, AFF_SNEAK);
        Comm.act("$n is revealed!", ich, null, null, TO_ROOM);
        Comm.send_to_char("You are revealed!\n\r", ich);
    }

    return;
}

public static void spell_floating_disc(int sn, int level, CharData ch, object vo, int target)
{
    ObjData disc = null, floating = null;

    floating = Handler.get_eq_char(ch, WEAR_FLOAT);
    if (floating != null && Bit.IS_OBJ_STAT(floating, ITEM_NOREMOVE))
    {
        Comm.act("You can't remove $p.", ch, floating, null, TO_CHAR);
        return;
    }

    disc = Db.create_object(Handler.get_obj_index(OBJ_VNUM_DISC), 0);
    disc.value[0] = ch.level * 10;    
    disc.value[3] = ch.level * 5;    
    disc.timer = ch.level * 2 - RomRandom.number_range(0, level / 2);

    Comm.act("$n has created a floating black disc.", ch, null, null, TO_ROOM);
    Comm.send_to_char("You create a floating disc.\n\r", ch);
    Handler.obj_to_char(disc, ch);
    ActObj.wear_obj(ch, disc, true);
    return;
}


public static void spell_fly(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    var af = new AffectData();

    if (Bit.IS_AFFECTED(victim, AFF_FLYING))
    {
        if (victim == ch)
            Comm.send_to_char("You are already airborne.\n\r", ch);
        else
            Comm.act("$N doesn't need your help to fly.", ch, null, victim,
                 TO_CHAR);
        return;
    }
    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.duration = level + 3;
    af.location = 0;
    af.modifier = 0;
    af.bitvector = AFF_FLYING;
    Handler.affect_to_char(victim, af);
    Comm.send_to_char("Your feet rise off the ground.\n\r", victim);
    Comm.act("$n's feet rise off the ground.", victim, null, null, TO_ROOM);
    return;
}



public static void spell_frenzy(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    var af = new AffectData();

    if (Handler.is_affected(victim, sn) || Bit.IS_AFFECTED(victim, AFF_BERSERK))
    {
        if (victim == ch)
            Comm.send_to_char("You are already in a frenzy.\n\r", ch);
        else
            Comm.act("$N is already in a frenzy.", ch, null, victim, TO_CHAR);
        return;
    }

    if (Handler.is_affected(victim, Lookup.skill_lookup("calm")))
    {
        if (victim == ch)
            Comm.send_to_char("Why don't you just relax for a while?\n\r", ch);
        else
            Comm.act("$N doesn't look like $e wants to fight anymore.",
                 ch, null, victim, TO_CHAR);
        return;
    }

    if ((Bit.IS_GOOD(ch) && !Bit.IS_GOOD(victim)) ||
        (Bit.IS_NEUTRAL(ch) && !Bit.IS_NEUTRAL(victim)) ||
        (Bit.IS_EVIL(ch) && !Bit.IS_EVIL(victim)))
    {
        Comm.act("Your god doesn't seem to like $N", ch, null, victim, TO_CHAR);
        return;
    }

    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.duration = level / 3;
    af.modifier = level / 6;
    af.bitvector = 0;

    af.location = APPLY_HITROLL;
    Handler.affect_to_char(victim, af);

    af.location = APPLY_DAMROLL;
    Handler.affect_to_char(victim, af);

    af.modifier = 10 * (level / 12);
    af.location = APPLY_AC;
    Handler.affect_to_char(victim, af);

    Comm.send_to_char("You are filled with holy wrath!\n\r", victim);
    Comm.act("$n gets a wild look in $s eyes!", victim, null, null, TO_ROOM);
}



public static void spell_gate(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = null;
    bool gate_pet;

    if ((victim = Handler.get_char_world(ch, target_name)) == null
        || victim == ch
        || victim.in_room == null
        || !Handler.can_see_room(ch, victim.in_room)
        || Bit.IS_SET(victim.in_room.room_flags, ROOM_SAFE)
        || Bit.IS_SET(victim.in_room.room_flags, ROOM_PRIVATE)
        || Bit.IS_SET(victim.in_room.room_flags, ROOM_SOLITARY)
        || Bit.IS_SET(victim.in_room.room_flags, ROOM_NO_RECALL)
        || Bit.IS_SET(ch.in_room.room_flags, ROOM_NO_RECALL)
        || victim.level >= level + 3
        || (Handler.is_clan(victim) && !Handler.is_same_clan(ch, victim))
        || (!Bit.IS_NPC(victim) && victim.level >= LEVEL_HERO)    
        || (Bit.IS_NPC(victim) && Bit.IS_SET(victim.imm_flags, IMM_SUMMON))
        || (Bit.IS_NPC(victim) && Magic.saves_spell(level, victim, DAM_OTHER)))
    {
        Comm.send_to_char("You failed.\n\r", ch);
        return;
    }
    if (ch.pet != null && ch.in_room == ch.pet.in_room)
        gate_pet = true;
    else
        gate_pet = false;

    Comm.act("$n steps through a gate and vanishes.", ch, null, null, TO_ROOM);
    Comm.send_to_char("You step through a gate and vanish.\n\r", ch);
    Handler.char_from_room(ch);
    Handler.char_to_room(ch, victim.in_room);

    Comm.act("$n has arrived through a gate.", ch, null, null, TO_ROOM);
    Interp.do_function(ch, Interp.do_look, "auto");

    if (gate_pet)
    {
        Comm.act("$n steps through a gate and vanishes.", ch.pet, null, null,
             TO_ROOM);
        Comm.send_to_char("You step through a gate and vanish.\n\r", ch.pet);
        Handler.char_from_room(ch.pet);
        Handler.char_to_room(ch.pet, victim.in_room);
        Comm.act("$n has arrived through a gate.", ch.pet, null, null, TO_ROOM);
        Interp.do_function(ch.pet, Interp.do_look, "auto");
    }
}



public static void spell_giant_strength(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    var af = new AffectData();

    if (Handler.is_affected(victim, sn))
    {
        if (victim == ch)
            Comm.send_to_char("You are already as strong as you can get!\n\r",
                          ch);
        else
            Comm.act("$N can't get any stronger.", ch, null, victim, TO_CHAR);
        return;
    }

    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.duration = level;
    af.location = APPLY_STR;
    af.modifier = 1 + ((level >= 18) ? 1 : 0) + ((level >= 25) ? 1 : 0) + ((level >= 32) ? 1 : 0);
    af.bitvector = 0;
    Handler.affect_to_char(victim, af);
    Comm.send_to_char("Your muscles surge with heightened power!\n\r", victim);
    Comm.act("$n's muscles surge with heightened power.", victim, null, null,
         TO_ROOM);
    return;
}



public static void spell_harm(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    int dam;

    dam = Bit.UMAX(20, victim.hit - RomRandom.dice(1, 4));
    if (Magic.saves_spell(level, victim, DAM_HARM))
        dam = Bit.UMIN(50, dam / 2);
    dam = Bit.UMIN(100, dam);
    Fight.damage(ch, victim, dam, sn, DAM_HARM, true);
    return;
}



public static void spell_haste(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    var af = new AffectData();

    if (Handler.is_affected(victim, sn) || Bit.IS_AFFECTED(victim, AFF_HASTE)
        || Bit.IS_SET(victim.off_flags, OFF_FAST))
    {
        if (victim == ch)
            Comm.send_to_char("You can't move any faster!\n\r", ch);
        else
            Comm.act("$N is already moving as fast as $E can.",
                 ch, null, victim, TO_CHAR);
        return;
    }

    if (Bit.IS_AFFECTED(victim, AFF_SLOW))
    {
        if (!check_dispel(level, victim, Lookup.skill_lookup("slow")))
        {
            if (victim != ch)
                Comm.send_to_char("Spell failed.\n\r", ch);
            Comm.send_to_char("You feel momentarily faster.\n\r", victim);
            return;
        }
        Comm.act("$n is moving less slowly.", victim, null, null, TO_ROOM);
        return;
    }

    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    if (victim == ch)
        af.duration = level / 2;
    else
        af.duration = level / 4;
    af.location = APPLY_DEX;
    af.modifier = 1 + ((level >= 18) ? 1 : 0) + ((level >= 25) ? 1 : 0) + ((level >= 32) ? 1 : 0);
    af.bitvector = AFF_HASTE;
    Handler.affect_to_char(victim, af);
    Comm.send_to_char("You feel yourself moving more quickly.\n\r", victim);
    Comm.act("$n is moving more quickly.", victim, null, null, TO_ROOM);
    if (ch != victim)
        Comm.send_to_char("Ok.\n\r", ch);
    return;
}



public static void spell_heal(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    victim.hit = Bit.UMIN(victim.hit + 100, victim.max_hit);
    Fight.update_pos(victim);
    Comm.send_to_char("A warm feeling fills your body.\n\r", victim);
    if (ch != victim)
        Comm.send_to_char("Ok.\n\r", ch);
    return;
}

public static void spell_heat_metal(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    ObjData obj_lose = null, obj_next = null;
    int dam = 0;
    bool fail = true;

    if (!Magic.saves_spell(level + 2, victim, DAM_FIRE)
        && !Bit.IS_SET(victim.imm_flags, IMM_FIRE))
    {
        for (obj_lose = victim.carrying;
             obj_lose != null; obj_lose = obj_next)
        {
            obj_next = obj_lose.next_content;
            if (RomRandom.number_range(1, 2 * level) > obj_lose.level
                && !Magic.saves_spell(level, victim, DAM_FIRE)
                && !Bit.IS_OBJ_STAT(obj_lose, ITEM_NONMETAL)
                && !Bit.IS_OBJ_STAT(obj_lose, ITEM_BURN_PROOF))
            {
                switch (obj_lose.item_type)
                {
                    case ITEM_ARMOR:
                        if (obj_lose.wear_loc != -1)
                        {        
                            if (Handler.can_drop_obj(victim, obj_lose)
                                && (obj_lose.weight / 10) <
                                RomRandom.number_range(1,
                                              2 * Handler.get_curr_stat(victim,
                                                                 STAT_DEX))
                                && ActObj.remove_obj(victim, obj_lose.wear_loc,
                                               true))
                            {
                                Comm.act("$n yelps and throws $p to the ground!",
                                     victim, obj_lose, null, TO_ROOM);
                                Comm.act("You remove and drop $p before it burns you.",
                                     victim, obj_lose, null, TO_CHAR);
                                dam +=
                                    (RomRandom.number_range(1, obj_lose.level) / 3);
                                Handler.obj_from_char(obj_lose);
                                Handler.obj_to_room(obj_lose, victim.in_room);
                                fail = false;
                            }
                            else
                            {    

                                Comm.act("Your skin is seared by $p!",
                                     victim, obj_lose, null, TO_CHAR);
                                dam += (RomRandom.number_range(1, obj_lose.level));
                                fail = false;
                            }

                        }
                        else
                        {        

                            if (Handler.can_drop_obj(victim, obj_lose))
                            {
                                Comm.act("$n yelps and throws $p to the ground!",
                                     victim, obj_lose, null, TO_ROOM);
                                Comm.act("You and drop $p before it burns you.",
                                     victim, obj_lose, null, TO_CHAR);
                                dam +=
                                    (RomRandom.number_range(1, obj_lose.level) / 6);
                                Handler.obj_from_char(obj_lose);
                                Handler.obj_to_room(obj_lose, victim.in_room);
                                fail = false;
                            }
                            else
                            {    

                                Comm.act("Your skin is seared by $p!",
                                     victim, obj_lose, null, TO_CHAR);
                                dam +=
                                    (RomRandom.number_range(1, obj_lose.level) / 2);
                                fail = false;
                            }
                        }
                        break;
                    case ITEM_WEAPON:
                        if (obj_lose.wear_loc != -1)
                        {        
                            if (Bit.IS_WEAPON_STAT(obj_lose, WEAPON_FLAMING))
                                continue;

                            if (Handler.can_drop_obj(victim, obj_lose)
                                && ActObj.remove_obj(victim, obj_lose.wear_loc,
                                               true))
                            {
                                Comm.act("$n is burned by $p, and throws it to the ground.",
                                     victim, obj_lose, null, TO_ROOM);
                                Comm.send_to_char("You throw your red-hot weapon to the ground!\n\r",
                                     victim);
                                dam += 1;
                                Handler.obj_from_char(obj_lose);
                                Handler.obj_to_room(obj_lose, victim.in_room);
                                fail = false;
                            }
                            else
                            {    

                                Comm.send_to_char("Your weapon sears your flesh!\n\r",
                                     victim);
                                dam += RomRandom.number_range(1, obj_lose.level);
                                fail = false;
                            }
                        }
                        else
                        {        

                            if (Handler.can_drop_obj(victim, obj_lose))
                            {
                                Comm.act("$n throws a burning hot $p to the ground!",
                                     victim, obj_lose, null, TO_ROOM);
                                Comm.act("You and drop $p before it burns you.",
                                     victim, obj_lose, null, TO_CHAR);
                                dam +=
                                    (RomRandom.number_range(1, obj_lose.level) / 6);
                                Handler.obj_from_char(obj_lose);
                                Handler.obj_to_room(obj_lose, victim.in_room);
                                fail = false;
                            }
                            else
                            {    

                                Comm.act("Your skin is seared by $p!",
                                     victim, obj_lose, null, TO_CHAR);
                                dam +=
                                    (RomRandom.number_range(1, obj_lose.level) / 2);
                                fail = false;
                            }
                        }
                        break;
                }
            }
        }
    }
    if (fail)
    {
        Comm.send_to_char("Your spell had no effect.\n\r", ch);
        Comm.send_to_char("You feel momentarily warmer.\n\r", victim);
    }
    else
    {                            

        if (Magic.saves_spell(level, victim, DAM_FIRE))
            dam = 2 * dam / 3;
        Fight.damage(ch, victim, dam, sn, DAM_FIRE, true);
    }
}


public static void spell_holy_word(int sn, int level, CharData ch, object vo, int target)
{
    CharData vch = null;
    CharData vch_next = null;
    int dam;
    int bless_num, curse_num, frenzy_num;

    bless_num = Lookup.skill_lookup("bless");
    curse_num = Lookup.skill_lookup("curse");
    frenzy_num = Lookup.skill_lookup("frenzy");

    Comm.act("$n utters a word of divine power!", ch, null, null, TO_ROOM);
    Comm.send_to_char("You utter a word of divine power.\n\r", ch);

    for (vch = ch.in_room.people; vch != null; vch = vch_next)
    {
        vch_next = vch.next_in_room;

        if ((Bit.IS_GOOD(ch) && Bit.IS_GOOD(vch)) ||
            (Bit.IS_EVIL(ch) && Bit.IS_EVIL(vch)) ||
            (Bit.IS_NEUTRAL(ch) && Bit.IS_NEUTRAL(vch)))
        {
            Comm.send_to_char("You feel full more powerful.\n\r", vch);
            spell_frenzy (frenzy_num, level, ch, vch, TARGET_CHAR);
            spell_bless (bless_num, level, ch, vch, TARGET_CHAR);
        }

        else if ((Bit.IS_GOOD(ch) && Bit.IS_EVIL(vch)) ||
                 (Bit.IS_EVIL(ch) && Bit.IS_GOOD(vch)))
        {
            if (!Fight.is_safe_spell(ch, vch, true))
            {
                spell_curse (curse_num, level, ch, vch, TARGET_CHAR);
                Comm.send_to_char("You are struck down!\n\r", vch);
                dam = RomRandom.dice(level, 6);
                Fight.damage(ch, vch, dam, sn, DAM_ENERGY, true);
            }
        }

        else if (Bit.IS_NEUTRAL(ch))
        {
            if (!Fight.is_safe_spell(ch, vch, true))
            {
                spell_curse (curse_num, level / 2, ch, vch,
                             TARGET_CHAR);
                Comm.send_to_char("You are struck down!\n\r", vch);
                dam = RomRandom.dice(level, 4);
                Fight.damage(ch, vch, dam, sn, DAM_ENERGY, true);
            }
        }
    }

    Comm.send_to_char("You feel drained.\n\r", ch);
    ch.move = 0;
    ch.hit /= 2;
}

public static void spell_identify(int sn, int level, CharData ch, object vo, int target)
{
    ObjData obj = vo as ObjData;
    string buf;
    AffectData paf = null;

    buf = RomString.sprintf(
             "Object '%s' is type %s, extra flags %s.\n\rWeight is %d, value is %d, level is %d.\n\r",
             obj.name,
             Handler.item_name(obj.item_type),
             Handler.extra_bit_name(obj.extra_flags),
             obj.weight / 10, obj.cost, obj.level);
    Comm.send_to_char(buf, ch);

    switch (obj.item_type)
    {
        case ITEM_SCROLL:
        case ITEM_POTION:
        case ITEM_PILL:
            buf = RomString.sprintf( "Level %d spells of:", obj.value[0]);
            Comm.send_to_char(buf, ch);

            if (obj.value[1] >= 0 && obj.value[1] < MAX_SKILL)
            {
                Comm.send_to_char(" '", ch);
                Comm.send_to_char(Tables.skill_table[obj.value[1]].name, ch);
                Comm.send_to_char("'", ch);
            }

            if (obj.value[2] >= 0 && obj.value[2] < MAX_SKILL)
            {
                Comm.send_to_char(" '", ch);
                Comm.send_to_char(Tables.skill_table[obj.value[2]].name, ch);
                Comm.send_to_char("'", ch);
            }

            if (obj.value[3] >= 0 && obj.value[3] < MAX_SKILL)
            {
                Comm.send_to_char(" '", ch);
                Comm.send_to_char(Tables.skill_table[obj.value[3]].name, ch);
                Comm.send_to_char("'", ch);
            }

            if (obj.value[4] >= 0 && obj.value[4] < MAX_SKILL)
            {
                Comm.send_to_char(" '", ch);
                Comm.send_to_char(Tables.skill_table[obj.value[4]].name, ch);
                Comm.send_to_char("'", ch);
            }

            Comm.send_to_char(".\n\r", ch);
            break;

        case ITEM_WAND:
        case ITEM_STAFF:
            buf = RomString.sprintf( "Has %d charges of level %d",
                     obj.value[2], obj.value[0]);
            Comm.send_to_char(buf, ch);

            if (obj.value[3] >= 0 && obj.value[3] < MAX_SKILL)
            {
                Comm.send_to_char(" '", ch);
                Comm.send_to_char(Tables.skill_table[obj.value[3]].name, ch);
                Comm.send_to_char("'", ch);
            }

            Comm.send_to_char(".\n\r", ch);
            break;

        case ITEM_DRINK_CON:
            buf = RomString.sprintf( "It holds %s-colored %s.\n\r",
                     Tables.liq_table[obj.value[2]].liq_color,
                     Tables.liq_table[obj.value[2]].liq_name);
            Comm.send_to_char(buf, ch);
            break;

        case ITEM_CONTAINER:
            buf = RomString.sprintf( "Capacity: %d#  Maximum weight: %d#  flags: %s\n\r",
                     obj.value[0], obj.value[3],
                     Handler.cont_bit_name(obj.value[1]));
            Comm.send_to_char(buf, ch);
            if (obj.value[4] != 100)
            {
                buf = RomString.sprintf( "Weight multiplier: %d%%\n\r", obj.value[4]);
                Comm.send_to_char(buf, ch);
            }
            break;

        case ITEM_WEAPON:
            Comm.send_to_char("Weapon type is ", ch);
            switch (obj.value[0])
            {
                case (WEAPON_EXOTIC):
                    Comm.send_to_char("exotic.\n\r", ch);
                    break;
                case (WEAPON_SWORD):
                    Comm.send_to_char("sword.\n\r", ch);
                    break;
                case (WEAPON_DAGGER):
                    Comm.send_to_char("dagger.\n\r", ch);
                    break;
                case (WEAPON_SPEAR):
                    Comm.send_to_char("spear/staff.\n\r", ch);
                    break;
                case (WEAPON_MACE):
                    Comm.send_to_char("mace/club.\n\r", ch);
                    break;
                case (WEAPON_AXE):
                    Comm.send_to_char("axe.\n\r", ch);
                    break;
                case (WEAPON_FLAIL):
                    Comm.send_to_char("flail.\n\r", ch);
                    break;
                case (WEAPON_WHIP):
                    Comm.send_to_char("whip.\n\r", ch);
                    break;
                case (WEAPON_POLEARM):
                    Comm.send_to_char("polearm.\n\r", ch);
                    break;
                default:
                    Comm.send_to_char("unknown.\n\r", ch);
                    break;
            }
            if (obj.pIndexData.new_format)
                buf = RomString.sprintf( "Damage is %dd%d (average %d).\n\r",
                         obj.value[1], obj.value[2],
                         (1 + obj.value[2]) * obj.value[1] / 2);
            else
                buf = RomString.sprintf( "Damage is %d to %d (average %d).\n\r",
                         obj.value[1], obj.value[2],
                         (obj.value[1] + obj.value[2]) / 2);
            Comm.send_to_char(buf, ch);
            if (obj.value[4] != 0)
            {                    
                buf = RomString.sprintf( "Weapons flags: %s\n\r",
                         Handler.weapon_bit_name(obj.value[4]));
                Comm.send_to_char(buf, ch);
            }
            break;

        case ITEM_ARMOR:
            buf = RomString.sprintf(
                     "Armor class is %d pierce, %d bash, %d slash, and %d vs. magic.\n\r",
                     obj.value[0], obj.value[1], obj.value[2],
                     obj.value[3]);
            Comm.send_to_char(buf, ch);
            break;
    }

    if (!obj.enchanted)
        for (paf = obj.pIndexData.affected; paf != null; paf = paf.next)
        {
            if (paf.location != APPLY_NONE && paf.modifier != 0)
            {
                buf = RomString.sprintf( "Affects %s by %d.\n\r",
                         Handler.affect_loc_name(paf.location), paf.modifier);
                Comm.send_to_char(buf, ch);
                if (paf.bitvector != 0)
                {
                    switch (paf.where)
                    {
                        case TO_AFFECTS:
                            buf = RomString.sprintf( "Adds %s affect.\n",
                                     Handler.affect_bit_name(paf.bitvector));
                            break;
                        case TO_OBJECT:
                            buf = RomString.sprintf( "Adds %s object flag.\n",
                                     Handler.extra_bit_name(paf.bitvector));
                            break;
                        case TO_IMMUNE:
                            buf = RomString.sprintf( "Adds immunity to %s.\n",
                                     Handler.imm_bit_name(paf.bitvector));
                            break;
                        case TO_RESIST:
                            buf = RomString.sprintf( "Adds resistance to %s.\n\r",
                                     Handler.imm_bit_name(paf.bitvector));
                            break;
                        case TO_VULN:
                            buf = RomString.sprintf( "Adds vulnerability to %s.\n\r",
                                     Handler.imm_bit_name(paf.bitvector));
                            break;
                        default:
                            buf = RomString.sprintf( "Unknown bit %d: %d\n\r",
                                     paf.where, paf.bitvector);
                            break;
                    }
                    Comm.send_to_char(buf, ch);
                }
            }
        }

    for (paf = obj.affected; paf != null; paf = paf.next)
    {
        if (paf.location != APPLY_NONE && paf.modifier != 0)
        {
            buf = RomString.sprintf( "Affects %s by %d",
                     Handler.affect_loc_name(paf.location), paf.modifier);
            Comm.send_to_char(buf, ch);
            if (paf.duration > -1)
                buf = RomString.sprintf( ", %d hours.\n\r", paf.duration);
            else
                buf = RomString.sprintf( ".\n\r");
            Comm.send_to_char(buf, ch);
            if (paf.bitvector != 0)
            {
                switch (paf.where)
                {
                    case TO_AFFECTS:
                        buf = RomString.sprintf( "Adds %s affect.\n",
                                 Handler.affect_bit_name(paf.bitvector));
                        break;
                    case TO_OBJECT:
                        buf = RomString.sprintf( "Adds %s object flag.\n",
                                 Handler.extra_bit_name(paf.bitvector));
                        break;
                    case TO_WEAPON:
                        buf = RomString.sprintf( "Adds %s weapon flags.\n",
                                 Handler.weapon_bit_name(paf.bitvector));
                        break;
                    case TO_IMMUNE:
                        buf = RomString.sprintf( "Adds immunity to %s.\n",
                                 Handler.imm_bit_name(paf.bitvector));
                        break;
                    case TO_RESIST:
                        buf = RomString.sprintf( "Adds resistance to %s.\n\r",
                                 Handler.imm_bit_name(paf.bitvector));
                        break;
                    case TO_VULN:
                        buf = RomString.sprintf( "Adds vulnerability to %s.\n\r",
                                 Handler.imm_bit_name(paf.bitvector));
                        break;
                    default:
                        buf = RomString.sprintf( "Unknown bit %d: %d\n\r",
                                 paf.where, paf.bitvector);
                        break;
                }
                Comm.send_to_char(buf, ch);
            }
        }
    }

    return;
}



public static void spell_infravision(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    var af = new AffectData();

    if (Bit.IS_AFFECTED(victim, AFF_INFRARED))
    {
        if (victim == ch)
            Comm.send_to_char("You can already see in the dark.\n\r", ch);
        else
            Comm.act("$N already has infravision.\n\r", ch, null, victim,
                 TO_CHAR);
        return;
    }
    Comm.act("$n's eyes glow red.\n\r", ch, null, null, TO_ROOM);

    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.duration = 2 * level;
    af.location = APPLY_NONE;
    af.modifier = 0;
    af.bitvector = AFF_INFRARED;
    Handler.affect_to_char(victim, af);
    Comm.send_to_char("Your eyes glow red.\n\r", victim);
    return;
}



public static void spell_invis(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = null;
    ObjData obj = null;
    var af = new AffectData();

    
    if (target == TARGET_OBJ)
    {
        obj = (vo as ObjData);

        if (Bit.IS_OBJ_STAT(obj, ITEM_INVIS))
        {
            Comm.act("$p is already invisible.", ch, obj, null, TO_CHAR);
            return;
        }

        af.where = TO_OBJECT;
        af.type = sn;
        af.level = level;
        af.duration = level + 12;
        af.location = APPLY_NONE;
        af.modifier = 0;
        af.bitvector = ITEM_INVIS;
        Handler.affect_to_obj(obj, af);

        Comm.act("$p fades out of sight.", ch, obj, null, TO_ALL);
        return;
    }

    
    victim = (vo as CharData);

    if (Bit.IS_AFFECTED(victim, AFF_INVISIBLE))
        return;

    Comm.act("$n fades out of existence.", victim, null, null, TO_ROOM);

    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.duration = level + 12;
    af.location = APPLY_NONE;
    af.modifier = 0;
    af.bitvector = AFF_INVISIBLE;
    Handler.affect_to_char(victim, af);
    Comm.send_to_char("You fade out of existence.\n\r", victim);
    return;
}



public static void spell_know_alignment(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    string msg;
    int ap;

    ap = victim.alignment;

    if (ap > 700)
        msg = "$N has a pure and good aura.";
    else if (ap > 350)
        msg = "$N is of excellent moral character.";
    else if (ap > 100)
        msg = "$N is often kind and thoughtful.";
    else if (ap > -100)
        msg = "$N doesn't have a firm moral commitment.";
    else if (ap > -350)
        msg = "$N lies to $S friends.";
    else if (ap > -700)
        msg = "$N is a black-hearted murderer.";
    else
        msg = "$N is the embodiment of pure evil!.";

    Comm.act(msg, ch, null, victim, TO_CHAR);
    return;
}



public static void spell_lightning_bolt(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    int[] dam_each = {
        0,
        0, 0, 0, 0, 0, 0, 0, 0, 25, 28,
        31, 34, 37, 40, 40, 41, 42, 42, 43, 44,
        44, 45, 46, 46, 47, 48, 48, 49, 50, 50,
        51, 52, 52, 53, 54, 54, 55, 56, 56, 57,
        58, 58, 59, 60, 60, 61, 62, 62, 63, 64
    };
    int dam;

    level = Bit.UMIN(level, dam_each.Length - 1);
    level = Bit.UMAX(0, level);
    dam = RomRandom.number_range(dam_each[level] / 2, dam_each[level] * 2);
    if (Magic.saves_spell(level, victim, DAM_LIGHTNING))
        dam /= 2;
    Fight.damage(ch, victim, dam, sn, DAM_LIGHTNING, true);
    return;
}



public static void spell_locate_object(int sn, int level, CharData ch, object vo, int target)
{
    string buf;
    System.Text.StringBuilder buffer;
    ObjData obj = null;
    ObjData in_obj = null;
    bool found;
    int number = 0, max_found;

    found = false;
    number = 0;
    max_found = Bit.IS_IMMORTAL(ch) ? 200 : 2 * level;

    buffer = new System.Text.StringBuilder();

    for (obj = Game.object_list; obj != null; obj = obj.next)
    {
        if (!Handler.can_see_obj(ch, obj) || !Handler.is_name(target_name, obj.name)
            || Bit.IS_OBJ_STAT(obj, ITEM_NOLOCATE)
            || RomRandom.number_percent() > 2 * level || ch.level < obj.level)
            continue;

        found = true;
        number++;

        for (in_obj = obj; in_obj.in_obj != null; in_obj = in_obj.in_obj);

        if (in_obj.carried_by != null && Handler.can_see(ch, in_obj.carried_by))
        {
            buf = RomString.sprintf( "one is carried by %s\n\r",
                     Handler.PERS(in_obj.carried_by, ch));
        }
        else
        {
            if (Bit.IS_IMMORTAL(ch) && in_obj.in_room != null)
                buf = RomString.sprintf( "one is in %s [Room %d]\n\r",
                         in_obj.in_room.name, in_obj.in_room.vnum);
            else
                buf = RomString.sprintf( "one is in %s\n\r",
                         in_obj.in_room == null
                         ? "somewhere" : in_obj.in_room.name);
        }

        if (buf.Length > 0)
        {
            var chars = buf.ToCharArray();
            chars[0] = Bit.UPPER(chars[0]);
            buf = new string(chars);
        }
        buffer.Append( buf);

        if (number >= max_found)
            break;
    }

    if (!found)
        Comm.send_to_char("Nothing like that in heaven or earth.\n\r", ch);
    else
        Comm.page_to_char(buffer.ToString(), ch);

    

    return;
}



public static void spell_magic_missile(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    int[] dam_each = {
        0,
        3, 3, 4, 4, 5, 6, 6, 6, 6, 6,
        7, 7, 7, 7, 7, 8, 8, 8, 8, 8,
        9, 9, 9, 9, 9, 10, 10, 10, 10, 10,
        11, 11, 11, 11, 11, 12, 12, 12, 12, 12,
        13, 13, 13, 13, 13, 14, 14, 14, 14, 14
    };
    int dam;

    level = Bit.UMIN(level, dam_each.Length - 1);
    level = Bit.UMAX(0, level);
    dam = RomRandom.number_range(dam_each[level] / 2, dam_each[level] * 2);
    if (Magic.saves_spell(level, victim, DAM_ENERGY))
        dam /= 2;
    Fight.damage(ch, victim, dam, sn, DAM_ENERGY, true);
    return;
}

public static void spell_mass_healing(int sn, int level, CharData ch, object vo, int target)
{
    CharData gch = null;
    int heal_num, refresh_num;

    heal_num = Lookup.skill_lookup("heal");
    refresh_num = Lookup.skill_lookup("refresh");

    for (gch = ch.in_room.people; gch != null; gch = gch.next_in_room)
    {
        if ((Bit.IS_NPC(ch) && Bit.IS_NPC(gch)) || (!Bit.IS_NPC(ch) && !Bit.IS_NPC(gch)))
        {
            spell_heal (heal_num, level, ch, gch, TARGET_CHAR);
            spell_refresh (refresh_num, level, ch, gch, TARGET_CHAR);
        }
    }
}


public static void spell_mass_invis(int sn, int level, CharData ch, object vo, int target)
{
    var af = new AffectData();
    CharData gch = null;

    for (gch = ch.in_room.people; gch != null; gch = gch.next_in_room)
    {
        if (!Handler.is_same_group(gch, ch) || Bit.IS_AFFECTED(gch, AFF_INVISIBLE))
            continue;
        Comm.act("$n slowly fades out of existence.", gch, null, null, TO_ROOM);
        Comm.send_to_char("You slowly fade out of existence.\n\r", gch);

        af.where = TO_AFFECTS;
        af.type = sn;
        af.level = level / 2;
        af.duration = 24;
        af.location = APPLY_NONE;
        af.modifier = 0;
        af.bitvector = AFF_INVISIBLE;
        Handler.affect_to_char(gch, af);
    }
    Comm.send_to_char("Ok.\n\r", ch);

    return;
}



public static void spell_null(int sn, int level, CharData ch, object vo, int target)
{
    Comm.send_to_char("That's not a spell!\n\r", ch);
    return;
}



public static void spell_pass_door(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    var af = new AffectData();

    if (Bit.IS_AFFECTED(victim, AFF_PASS_DOOR))
    {
        if (victim == ch)
            Comm.send_to_char("You are already out of phase.\n\r", ch);
        else
            Comm.act("$N is already shifted out of phase.", ch, null, victim,
                 TO_CHAR);
        return;
    }

    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.duration = RomRandom.number_fuzzy(level / 4);
    af.location = APPLY_NONE;
    af.modifier = 0;
    af.bitvector = AFF_PASS_DOOR;
    Handler.affect_to_char(victim, af);
    Comm.act("$n turns translucent.", victim, null, null, TO_ROOM);
    Comm.send_to_char("You turn translucent.\n\r", victim);
    return;
}



public static void spell_plague(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    var af = new AffectData();

    if (Magic.saves_spell(level, victim, DAM_DISEASE) ||
        (Bit.IS_NPC(victim) && Bit.IS_SET(victim.act, ACT_UNDEAD)))
    {
        if (ch == victim)
            Comm.send_to_char("You feel momentarily ill, but it passes.\n\r", ch);
        else
            Comm.act("$N seems to be unaffected.", ch, null, victim, TO_CHAR);
        return;
    }

    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level * 3 / 4;
    af.duration = level;
    af.location = APPLY_STR;
    af.modifier = -5;
    af.bitvector = AFF_PLAGUE;
    Handler.affect_join(victim, af);

    Comm.send_to_char("You scream in agony as plague sores erupt from your skin.\n\r",
         victim);
    Comm.act("$n screams in agony as plague sores erupt from $s skin.", victim,
         null, null, TO_ROOM);
}

public static void spell_poison(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = null;
    ObjData obj = null;
    var af = new AffectData();


    if (target == TARGET_OBJ)
    {
        obj = (vo as ObjData);

        if (obj.item_type == ITEM_FOOD || obj.item_type == ITEM_DRINK_CON)
        {
            if (Bit.IS_OBJ_STAT(obj, ITEM_BLESS)
                || Bit.IS_OBJ_STAT(obj, ITEM_BURN_PROOF))
            {
                Comm.act("Your spell fails to corrupt $p.", ch, obj, null,
                     TO_CHAR);
                return;
            }
            obj.value[3] = 1;
            Comm.act("$p is infused with poisonous vapors.", ch, obj, null,
                 TO_ALL);
            return;
        }

        if (obj.item_type == ITEM_WEAPON)
        {
            if (Bit.IS_WEAPON_STAT(obj, WEAPON_FLAMING)
                || Bit.IS_WEAPON_STAT(obj, WEAPON_FROST)
                || Bit.IS_WEAPON_STAT(obj, WEAPON_VAMPIRIC)
                || Bit.IS_WEAPON_STAT(obj, WEAPON_SHARP)
                || Bit.IS_WEAPON_STAT(obj, WEAPON_VORPAL)
                || Bit.IS_WEAPON_STAT(obj, WEAPON_SHOCKING)
                || Bit.IS_OBJ_STAT(obj, ITEM_BLESS)
                || Bit.IS_OBJ_STAT(obj, ITEM_BURN_PROOF))
            {
                Comm.act("You can't seem to envenom $p.", ch, obj, null, TO_CHAR);
                return;
            }

            if (Bit.IS_WEAPON_STAT(obj, WEAPON_POISON))
            {
                Comm.act("$p is already envenomed.", ch, obj, null, TO_CHAR);
                return;
            }

            af.where = TO_WEAPON;
            af.type = sn;
            af.level = level / 2;
            af.duration = level / 8;
            af.location = 0;
            af.modifier = 0;
            af.bitvector = WEAPON_POISON;
            Handler.affect_to_obj(obj, af);

            Comm.act("$p is coated with deadly venom.", ch, obj, null, TO_ALL);
            return;
        }

        Comm.act("You can't poison $p.", ch, obj, null, TO_CHAR);
        return;
    }

    victim = (vo as CharData);

    if (Magic.saves_spell(level, victim, DAM_POISON))
    {
        Comm.act("$n turns slightly green, but it passes.", victim, null, null,
             TO_ROOM);
        Comm.send_to_char("You feel momentarily ill, but it passes.\n\r", victim);
        return;
    }

    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.duration = level;
    af.location = APPLY_STR;
    af.modifier = -2;
    af.bitvector = AFF_POISON;
    Handler.affect_join(victim, af);
    Comm.send_to_char("You feel very sick.\n\r", victim);
    Comm.act("$n looks very ill.", victim, null, null, TO_ROOM);
    return;
}



public static void spell_protection_evil(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    var af = new AffectData();

    if (Bit.IS_AFFECTED(victim, AFF_PROTECT_EVIL)
        || Bit.IS_AFFECTED(victim, AFF_PROTECT_GOOD))
    {
        if (victim == ch)
            Comm.send_to_char("You are already protected.\n\r", ch);
        else
            Comm.act("$N is already protected.", ch, null, victim, TO_CHAR);
        return;
    }

    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.duration = 24;
    af.location = APPLY_SAVING_SPELL;
    af.modifier = -1;
    af.bitvector = AFF_PROTECT_EVIL;
    Handler.affect_to_char(victim, af);
    Comm.send_to_char("You feel holy and pure.\n\r", victim);
    if (ch != victim)
        Comm.act("$N is protected from evil.", ch, null, victim, TO_CHAR);
    return;
}

public static void spell_protection_good(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    var af = new AffectData();

    if (Bit.IS_AFFECTED(victim, AFF_PROTECT_GOOD)
        || Bit.IS_AFFECTED(victim, AFF_PROTECT_EVIL))
    {
        if (victim == ch)
            Comm.send_to_char("You are already protected.\n\r", ch);
        else
            Comm.act("$N is already protected.", ch, null, victim, TO_CHAR);
        return;
    }

    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.duration = 24;
    af.location = APPLY_SAVING_SPELL;
    af.modifier = -1;
    af.bitvector = AFF_PROTECT_GOOD;
    Handler.affect_to_char(victim, af);
    Comm.send_to_char("You feel aligned with darkness.\n\r", victim);
    if (ch != victim)
        Comm.act("$N is protected from good.", ch, null, victim, TO_CHAR);
    return;
}


public static void spell_ray_of_truth(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    int dam, align;

    if (Bit.IS_EVIL(ch))
    {
        victim = ch;
        Comm.send_to_char("The energy explodes inside you!\n\r", ch);
    }

    if (victim != ch)
    {
        Comm.act("$n raises $s hand, and a blinding ray of light shoots forth!",
             ch, null, null, TO_ROOM);
        Comm.send_to_char("You raise your hand and a blinding ray of light shoots forth!\n\r",
             ch);
    }

    if (Bit.IS_GOOD(victim))
    {
        Comm.act("$n seems unharmed by the light.", victim, null, victim,
             TO_ROOM);
        Comm.send_to_char("The light seems powerless to affect you.\n\r", victim);
        return;
    }

    dam = RomRandom.dice(level, 10);
    if (Magic.saves_spell(level, victim, DAM_HOLY))
        dam /= 2;

    align = victim.alignment;
    align -= 350;

    if (align < -1000)
        align = -1000 + (align + 1000) / 3;

    dam = (dam * align * align) / 1000000;

    Fight.damage(ch, victim, dam, sn, DAM_HOLY, true);
    spell_blindness (Gsn.blindness,
                     3 * level / 4, ch, victim, TARGET_CHAR);
}


public static void spell_recharge(int sn, int level, CharData ch, object vo, int target)
{
    ObjData obj = vo as ObjData;
    int chance, percent;

    if (obj.item_type != ITEM_WAND && obj.item_type != ITEM_STAFF)
    {
        Comm.send_to_char("That item does not carry charges.\n\r", ch);
        return;
    }

    if (obj.value[3] >= 3 * level / 2)
    {
        Comm.send_to_char("Your skills are not great enough for that.\n\r", ch);
        return;
    }

    if (obj.value[1] == 0)
    {
        Comm.send_to_char("That item has already been recharged once.\n\r", ch);
        return;
    }

    chance = 40 + 2 * level;

    chance -= obj.value[3];    
    chance -= (obj.value[1] - obj.value[2]) *
        (obj.value[1] - obj.value[2]);

    chance = Bit.UMAX(level / 2, chance);

    percent = RomRandom.number_percent();

    if (percent < chance / 2)
    {
        Comm.act("$p glows softly.", ch, obj, null, TO_CHAR);
        Comm.act("$p glows softly.", ch, obj, null, TO_ROOM);
        obj.value[2] = Bit.UMAX(obj.value[1], obj.value[2]);
        obj.value[1] = 0;
        Gmcp.ItemUpdate(obj);
        return;
    }

    else if (percent <= chance)
    {
        int chargeback, chargemax;

        Comm.act("$p glows softly.", ch, obj, null, TO_CHAR);
        Comm.act("$p glows softly.", ch, obj, null, TO_CHAR);

        chargemax = obj.value[1] - obj.value[2];

        if (chargemax > 0)
            chargeback = Bit.UMAX(1, chargemax * percent / 100);
        else
            chargeback = 0;

        obj.value[2] += chargeback;
        obj.value[1] = 0;
        Gmcp.ItemUpdate(obj);
        return;
    }

    else if (percent <= Bit.UMIN(95, 3 * chance / 2))
    {
        Comm.send_to_char("Nothing seems to happen.\n\r", ch);
        if (obj.value[1] > 1)
        {
            obj.value[1]--;
            Gmcp.ItemUpdate(obj);
        }
        return;
    }

    else
    {                            

        Comm.act("$p glows brightly and explodes!", ch, obj, null, TO_CHAR);
        Comm.act("$p glows brightly and explodes!", ch, obj, null, TO_ROOM);
        Handler.extract_obj(obj);
    }
}

public static void spell_refresh(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    victim.move = Bit.UMIN(victim.move + level, victim.max_move);
    if (victim.max_move == victim.move)
        Comm.send_to_char("You feel fully refreshed!\n\r", victim);
    else
        Comm.send_to_char("You feel less tired.\n\r", victim);
    if (ch != victim)
        Comm.send_to_char("Ok.\n\r", ch);
    return;
}

public static void spell_remove_curse(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = null;
    ObjData obj = null;
    bool found = false;

    
    if (target == TARGET_OBJ)
    {
        obj = (vo as ObjData);

        if (Bit.IS_OBJ_STAT(obj, ITEM_NODROP)
            || Bit.IS_OBJ_STAT(obj, ITEM_NOREMOVE))
        {
            if (!Bit.IS_OBJ_STAT(obj, ITEM_NOUNCURSE)
                && !saves_dispel(level + 2, obj.level, 0))
            {
                Bit.REMOVE_BIT(ref obj.extra_flags, ITEM_NODROP);
                Bit.REMOVE_BIT(ref obj.extra_flags, ITEM_NOREMOVE);
                Comm.act("$p glows blue.", ch, obj, null, TO_ALL);
                return;
            }

            Comm.act("The curse on $p is beyond your power.", ch, obj, null,
                 TO_CHAR);
            return;
        }
        Comm.act("There doesn't seem to be a curse on $p.", ch, obj, null,
             TO_CHAR);
        return;
    }

    
    victim = (vo as CharData);

    if (check_dispel(level, victim, Gsn.curse))
    {
        Comm.send_to_char("You feel better.\n\r", victim);
        Comm.act("$n looks more relaxed.", victim, null, null, TO_ROOM);
    }

    for (obj = victim.carrying; (obj != null && !found);
         obj = obj.next_content)
    {
        if ((Bit.IS_OBJ_STAT(obj, ITEM_NODROP)
             || Bit.IS_OBJ_STAT(obj, ITEM_NOREMOVE))
            && !Bit.IS_OBJ_STAT(obj, ITEM_NOUNCURSE))
        {                        
            if (!saves_dispel(level, obj.level, 0))
            {
                found = true;
                Bit.REMOVE_BIT(ref obj.extra_flags, ITEM_NODROP);
                Bit.REMOVE_BIT(ref obj.extra_flags, ITEM_NOREMOVE);
                Comm.act("Your $p glows blue.", victim, obj, null, TO_CHAR);
                Comm.act("$n's $p glows blue.", victim, obj, null, TO_ROOM);
            }
        }
    }
}

public static void spell_sanctuary(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    var af = new AffectData();

    if (Bit.IS_AFFECTED(victim, AFF_SANCTUARY))
    {
        if (victim == ch)
            Comm.send_to_char("You are already in sanctuary.\n\r", ch);
        else
            Comm.act("$N is already in sanctuary.", ch, null, victim, TO_CHAR);
        return;
    }

    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.duration = level / 6;
    af.location = APPLY_NONE;
    af.modifier = 0;
    af.bitvector = AFF_SANCTUARY;
    Handler.affect_to_char(victim, af);
    Comm.act("$n is surrounded by a white aura.", victim, null, null, TO_ROOM);
    Comm.send_to_char("You are surrounded by a white aura.\n\r", victim);
    return;
}



public static void spell_shield(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    var af = new AffectData();

    if (Handler.is_affected(victim, sn))
    {
        if (victim == ch)
            Comm.send_to_char("You are already shielded from harm.\n\r", ch);
        else
            Comm.act("$N is already protected by a shield.", ch, null, victim,
                 TO_CHAR);
        return;
    }

    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.duration = 8 + level;
    af.location = APPLY_AC;
    af.modifier = -20;
    af.bitvector = 0;
    Handler.affect_to_char(victim, af);
    Comm.act("$n is surrounded by a force shield.", victim, null, null, TO_ROOM);
    Comm.send_to_char("You are surrounded by a force shield.\n\r", victim);
    return;
}



public static void spell_shocking_grasp(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    int[] dam_each = {
        0,
        0, 0, 0, 0, 0, 0, 20, 25, 29, 33,
        36, 39, 39, 39, 40, 40, 41, 41, 42, 42,
        43, 43, 44, 44, 45, 45, 46, 46, 47, 47,
        48, 48, 49, 49, 50, 50, 51, 51, 52, 52,
        53, 53, 54, 54, 55, 55, 56, 56, 57, 57
    };
    int dam;

    level = Bit.UMIN(level, dam_each.Length - 1);
    level = Bit.UMAX(0, level);
    dam = RomRandom.number_range(dam_each[level] / 2, dam_each[level] * 2);
    if (Magic.saves_spell(level, victim, DAM_LIGHTNING))
        dam /= 2;
    Fight.damage(ch, victim, dam, sn, DAM_LIGHTNING, true);
    return;
}



public static void spell_sleep(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    var af = new AffectData();

    if (Bit.IS_AFFECTED(victim, AFF_SLEEP)
        || (Bit.IS_NPC(victim) && Bit.IS_SET(victim.act, ACT_UNDEAD))
        || (level + 2) < victim.level
        || Magic.saves_spell(level - 4, victim, DAM_CHARM)) return;

    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.duration = 4 + level;
    af.location = APPLY_NONE;
    af.modifier = 0;
    af.bitvector = AFF_SLEEP;
    Handler.affect_join(victim, af);

    if (Bit.IS_AWAKE(victim))
    {
        Comm.send_to_char("You feel very sleepy ..... zzzzzz.\n\r", victim);
        Comm.act("$n goes to sleep.", victim, null, null, TO_ROOM);
        victim.position = POS_SLEEPING;
    }
    return;
}

public static void spell_slow(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    var af = new AffectData();

    if (Handler.is_affected(victim, sn) || Bit.IS_AFFECTED(victim, AFF_SLOW))
    {
        if (victim == ch)
            Comm.send_to_char("You can't move any slower!\n\r", ch);
        else
            Comm.act("$N can't get any slower than that.",
                 ch, null, victim, TO_CHAR);
        return;
    }

    if (Magic.saves_spell(level, victim, DAM_OTHER)
        || Bit.IS_SET(victim.imm_flags, IMM_MAGIC))
    {
        if (victim != ch)
            Comm.send_to_char("Nothing seemed to happen.\n\r", ch);
        Comm.send_to_char("You feel momentarily lethargic.\n\r", victim);
        return;
    }

    if (Bit.IS_AFFECTED(victim, AFF_HASTE))
    {
        if (!check_dispel(level, victim, Lookup.skill_lookup("haste")))
        {
            if (victim != ch)
                Comm.send_to_char("Spell failed.\n\r", ch);
            Comm.send_to_char("You feel momentarily slower.\n\r", victim);
            return;
        }

        Comm.act("$n is moving less quickly.", victim, null, null, TO_ROOM);
        return;
    }


    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.duration = level / 2;
    af.location = APPLY_DEX;
    af.modifier = -1 - ((level >= 18) ? 1 : 0) - ((level >= 25) ? 1 : 0) - ((level >= 32) ? 1 : 0);
    af.bitvector = AFF_SLOW;
    Handler.affect_to_char(victim, af);
    Comm.send_to_char("You feel yourself slowing d o w n...\n\r", victim);
    Comm.act("$n starts to move in slow motion.", victim, null, null, TO_ROOM);
    return;
}




public static void spell_stone_skin(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    var af = new AffectData();

    if (Handler.is_affected(ch, sn))
    {
        if (victim == ch)
            Comm.send_to_char("Your skin is already as hard as a rock.\n\r", ch);
        else
            Comm.act("$N is already as hard as can be.", ch, null, victim,
                 TO_CHAR);
        return;
    }

    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.duration = level;
    af.location = APPLY_AC;
    af.modifier = -40;
    af.bitvector = 0;
    Handler.affect_to_char(victim, af);
    Comm.act("$n's skin turns to stone.", victim, null, null, TO_ROOM);
    Comm.send_to_char("Your skin turns to stone.\n\r", victim);
    return;
}



public static void spell_summon(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = null;

    if ((victim = Handler.get_char_world(ch, target_name)) == null
        || victim == ch
        || victim.in_room == null
        || Bit.IS_SET(ch.in_room.room_flags, ROOM_SAFE)
        || Bit.IS_SET(victim.in_room.room_flags, ROOM_SAFE)
        || Bit.IS_SET(victim.in_room.room_flags, ROOM_PRIVATE)
        || Bit.IS_SET(victim.in_room.room_flags, ROOM_SOLITARY)
        || Bit.IS_SET(victim.in_room.room_flags, ROOM_NO_RECALL)
        || (Bit.IS_NPC(victim) && Bit.IS_SET(victim.act, ACT_AGGRESSIVE))
        || victim.level >= level + 3
        || (!Bit.IS_NPC(victim) && victim.level >= LEVEL_IMMORTAL)
        || victim.fighting != null
        || (Bit.IS_NPC(victim) && Bit.IS_SET(victim.imm_flags, IMM_SUMMON))
        || (Bit.IS_NPC(victim) && victim.pIndexData.pShop != null)
        || (!Bit.IS_NPC(victim) && Bit.IS_SET(victim.act, PLR_NOSUMMON))
        || (Bit.IS_NPC(victim) && Magic.saves_spell(level, victim, DAM_OTHER)))
    {
        Comm.send_to_char("You failed.\n\r", ch);
        return;
    }

    Comm.act("$n disappears suddenly.", victim, null, null, TO_ROOM);
    Handler.char_from_room(victim);
    Handler.char_to_room(victim, ch.in_room);
    Comm.act("$n arrives suddenly.", victim, null, null, TO_ROOM);
    Comm.act("$n has summoned you!", ch, null, victim, TO_VICT);
    Interp.do_function(victim, Interp.do_look, "auto");
    return;
}



public static void spell_teleport(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    RoomIndexData pRoomIndex = null;

    if (victim.in_room == null
        || Bit.IS_SET(victim.in_room.room_flags, ROOM_NO_RECALL)
        || (victim != ch && Bit.IS_SET(victim.imm_flags, IMM_SUMMON))
        || (!Bit.IS_NPC(ch) && victim.fighting != null)
        || (victim != ch && (Magic.saves_spell(level - 5, victim, DAM_OTHER))))
    {
        Comm.send_to_char("You failed.\n\r", ch);
        return;
    }

    pRoomIndex = Handler.get_random_room(victim);

    if (victim != ch)
        Comm.send_to_char("You have been teleported!\n\r", victim);

    Comm.act("$n vanishes!", victim, null, null, TO_ROOM);
    Handler.char_from_room(victim);
    Handler.char_to_room(victim, pRoomIndex);
    Comm.act("$n slowly fades into existence.", victim, null, null, TO_ROOM);
    Interp.do_function(victim, Interp.do_look, "auto");
    return;
}



public static void spell_ventriloquate(int sn, int level, CharData ch, object vo, int target)
{
    string buf1;
    string buf2;
    string speaker;
    CharData vch = null;

    target_name = RomString.one_argument(target_name, out speaker);

    buf1 = RomString.sprintf( "%s says '%s'.\n\r", speaker, target_name);
    buf2 = RomString.sprintf( "Someone makes %s say '%s'.\n\r", speaker, target_name);
    if (buf1.Length > 0) buf1 = char.ToUpperInvariant(buf1[0]) + buf1.Substring(1);

    for (vch = ch.in_room.people; vch != null; vch = vch.next_in_room)
    {
        if (!Handler.is_exact_name(speaker, vch.name) && Bit.IS_AWAKE(vch))
            Comm.send_to_char(Magic.saves_spell(level, vch, DAM_OTHER) ? buf2 : buf1,
                          vch);
    }

    return;
}



public static void spell_weaken(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    var af = new AffectData();

    if (Handler.is_affected(victim, sn) || Magic.saves_spell(level, victim, DAM_OTHER))
        return;

    af.where = TO_AFFECTS;
    af.type = sn;
    af.level = level;
    af.duration = level / 2;
    af.location = APPLY_STR;
    af.modifier = -1 * (level / 5);
    af.bitvector = AFF_WEAKEN;
    Handler.affect_to_char(victim, af);
    Comm.send_to_char("You feel your strength slip away.\n\r", victim);
    Comm.act("$n looks tired and weak.", victim, null, null, TO_ROOM);
    return;
}





public static void spell_word_of_recall(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    RoomIndexData location = null;

    if (Bit.IS_NPC(victim))
        return;

    if ((location = Handler.get_room_index(ROOM_VNUM_TEMPLE)) == null)
    {
        Comm.send_to_char("You are completely lost.\n\r", victim);
        return;
    }

    if (Bit.IS_SET(victim.in_room.room_flags, ROOM_NO_RECALL) ||
        Bit.IS_AFFECTED(victim, AFF_CURSE))
    {
        Comm.send_to_char("Spell failed.\n\r", victim);
        return;
    }

    if (victim.fighting != null)
        Fight.stop_fighting(victim, true);

    ch.move /= 2;
    Comm.act("$n disappears.", victim, null, null, TO_ROOM);
    Handler.char_from_room(victim);
    Handler.char_to_room(victim, location);
    Comm.act("$n appears in the room.", victim, null, null, TO_ROOM);
    Interp.do_function(victim, Interp.do_look, "auto");
}


public static void spell_acid_breath(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    int dam, hp_dam, dice_dam, hpch;

    Comm.act("$n spits acid at $N.", ch, null, victim, TO_NOTVICT);
    Comm.act("$n spits a stream of corrosive acid at you.", ch, null, victim,
         TO_VICT);
    Comm.act("You spit acid at $N.", ch, null, victim, TO_CHAR);

    hpch = Bit.UMAX(12, ch.hit);
    hp_dam = RomRandom.number_range(hpch / 11 + 1, hpch / 6);
    dice_dam = RomRandom.dice(level, 16);

    dam = Bit.UMAX(hp_dam + dice_dam / 10, dice_dam + hp_dam / 10);

    if (Magic.saves_spell(level, victim, DAM_ACID))
    {
        Effects.acid_effect(victim, level / 2, dam / 4, TARGET_CHAR);
        Fight.damage(ch, victim, dam / 2, sn, DAM_ACID, true);
    }
    else
    {
        Effects.acid_effect(victim, level, dam, TARGET_CHAR);
        Fight.damage(ch, victim, dam, sn, DAM_ACID, true);
    }
}



public static void spell_fire_breath(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    CharData vch = null, vch_next = null;
    int dam, hp_dam, dice_dam;
    int hpch;

    Comm.act("$n breathes forth a cone of fire.", ch, null, victim, TO_NOTVICT);
    Comm.act("$n breathes a cone of hot fire over you!", ch, null, victim,
         TO_VICT);
    Comm.act("You breath forth a cone of fire.", ch, null, null, TO_CHAR);

    hpch = Bit.UMAX(10, ch.hit);
    hp_dam = RomRandom.number_range(hpch / 9 + 1, hpch / 5);
    dice_dam = RomRandom.dice(level, 20);

    dam = Bit.UMAX(hp_dam + dice_dam / 10, dice_dam + hp_dam / 10);
    Effects.fire_effect(victim.in_room, level, dam / 2, TARGET_ROOM);

    for (vch = victim.in_room.people; vch != null; vch = vch_next)
    {
        vch_next = vch.next_in_room;

        if (Fight.is_safe_spell(ch, vch, true)
            || (Bit.IS_NPC(vch) && Bit.IS_NPC(ch)
                && (ch.fighting != vch || vch.fighting != ch)))
            continue;

        if (vch == victim)
        {                        
            if (Magic.saves_spell(level, vch, DAM_FIRE))
            {
                Effects.fire_effect(vch, level / 2, dam / 4, TARGET_CHAR);
                Fight.damage(ch, vch, dam / 2, sn, DAM_FIRE, true);
            }
            else
            {
                Effects.fire_effect(vch, level, dam, TARGET_CHAR);
                Fight.damage(ch, vch, dam, sn, DAM_FIRE, true);
            }
        }
        else
        {                        

            if (Magic.saves_spell(level - 2, vch, DAM_FIRE))
            {
                Effects.fire_effect(vch, level / 4, dam / 8, TARGET_CHAR);
                Fight.damage(ch, vch, dam / 4, sn, DAM_FIRE, true);
            }
            else
            {
                Effects.fire_effect(vch, level / 2, dam / 4, TARGET_CHAR);
                Fight.damage(ch, vch, dam / 2, sn, DAM_FIRE, true);
            }
        }
    }
}

public static void spell_frost_breath(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    CharData vch = null, vch_next = null;
    int dam, hp_dam, dice_dam, hpch;

    Comm.act("$n breathes out a freezing cone of frost!", ch, null, victim,
         TO_NOTVICT);
    Comm.act("$n breathes a freezing cone of frost over you!", ch, null, victim,
         TO_VICT);
    Comm.act("You breath out a cone of frost.", ch, null, null, TO_CHAR);

    hpch = Bit.UMAX(12, ch.hit);
    hp_dam = RomRandom.number_range(hpch / 11 + 1, hpch / 6);
    dice_dam = RomRandom.dice(level, 16);

    dam = Bit.UMAX(hp_dam + dice_dam / 10, dice_dam + hp_dam / 10);
    Effects.cold_effect(victim.in_room, level, dam / 2, TARGET_ROOM);

    for (vch = victim.in_room.people; vch != null; vch = vch_next)
    {
        vch_next = vch.next_in_room;

        if (Fight.is_safe_spell(ch, vch, true)
            || (Bit.IS_NPC(vch) && Bit.IS_NPC(ch)
                && (ch.fighting != vch || vch.fighting != ch)))
            continue;

        if (vch == victim)
        {                        
            if (Magic.saves_spell(level, vch, DAM_COLD))
            {
                Effects.cold_effect(vch, level / 2, dam / 4, TARGET_CHAR);
                Fight.damage(ch, vch, dam / 2, sn, DAM_COLD, true);
            }
            else
            {
                Effects.cold_effect(vch, level, dam, TARGET_CHAR);
                Fight.damage(ch, vch, dam, sn, DAM_COLD, true);
            }
        }
        else
        {
            if (Magic.saves_spell(level - 2, vch, DAM_COLD))
            {
                Effects.cold_effect(vch, level / 4, dam / 8, TARGET_CHAR);
                Fight.damage(ch, vch, dam / 4, sn, DAM_COLD, true);
            }
            else
            {
                Effects.cold_effect(vch, level / 2, dam / 4, TARGET_CHAR);
                Fight.damage(ch, vch, dam / 2, sn, DAM_COLD, true);
            }
        }
    }
}


public static void spell_gas_breath(int sn, int level, CharData ch, object vo, int target)
{
    CharData vch = null;
    CharData vch_next = null;
    int dam, hp_dam, dice_dam, hpch;

    Comm.act("$n breathes out a cloud of poisonous gas!", ch, null, null,
         TO_ROOM);
    Comm.act("You breath out a cloud of poisonous gas.", ch, null, null, TO_CHAR);

    hpch = Bit.UMAX(16, ch.hit);
    hp_dam = RomRandom.number_range(hpch / 15 + 1, 8);
    dice_dam = RomRandom.dice(level, 12);

    dam = Bit.UMAX(hp_dam + dice_dam / 10, dice_dam + hp_dam / 10);
    Effects.poison_effect(ch.in_room, level, dam, TARGET_ROOM);

    for (vch = ch.in_room.people; vch != null; vch = vch_next)
    {
        vch_next = vch.next_in_room;

        if (Fight.is_safe_spell(ch, vch, true)
            || (Bit.IS_NPC(ch) && Bit.IS_NPC(vch)
                && (ch.fighting == vch || vch.fighting == ch)))
            continue;

        if (Magic.saves_spell(level, vch, DAM_POISON))
        {
            Effects.poison_effect(vch, level / 2, dam / 4, TARGET_CHAR);
            Fight.damage(ch, vch, dam / 2, sn, DAM_POISON, true);
        }
        else
        {
            Effects.poison_effect(vch, level, dam, TARGET_CHAR);
            Fight.damage(ch, vch, dam, sn, DAM_POISON, true);
        }
    }
}

public static void spell_lightning_breath(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    int dam, hp_dam, dice_dam, hpch;

    Comm.act("$n breathes a bolt of lightning at $N.", ch, null, victim,
         TO_NOTVICT);
    Comm.act("$n breathes a bolt of lightning at you!", ch, null, victim,
         TO_VICT);
    Comm.act("You breathe a bolt of lightning at $N.", ch, null, victim, TO_CHAR);

    hpch = Bit.UMAX(10, ch.hit);
    hp_dam = RomRandom.number_range(hpch / 9 + 1, hpch / 5);
    dice_dam = RomRandom.dice(level, 20);

    dam = Bit.UMAX(hp_dam + dice_dam / 10, dice_dam + hp_dam / 10);

    if (Magic.saves_spell(level, victim, DAM_LIGHTNING))
    {
        Effects.shock_effect(victim, level / 2, dam / 4, TARGET_CHAR);
        Fight.damage(ch, victim, dam / 2, sn, DAM_LIGHTNING, true);
    }
    else
    {
        Effects.shock_effect(victim, level, dam, TARGET_CHAR);
        Fight.damage(ch, victim, dam, sn, DAM_LIGHTNING, true);
    }
}


public static void spell_general_purpose(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    int dam;

    dam = RomRandom.number_range(25, 100);
    if (Magic.saves_spell(level, victim, DAM_PIERCE))
        dam /= 2;
    Fight.damage(ch, victim, dam, sn, DAM_PIERCE, true);
    return;
}

public static void spell_high_explosive(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = vo as CharData;
    int dam;

    dam = RomRandom.number_range(30, 120);
    if (Magic.saves_spell(level, victim, DAM_PIERCE))
        dam /= 2;
    Fight.damage(ch, victim, dam, sn, DAM_PIERCE, true);
    return;
}

public static void spell_farsight(int sn, int level, CharData ch, object vo, int target)
{
    if (Bit.IS_AFFECTED(ch, AFF_BLIND))
    {
        Comm.send_to_char("Maybe it would help if you could see?\n\r", ch);
        return;
    }

    Interp.do_function(ch, Scan.do_scan, target_name);
}


public static void spell_portal(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = null;
    ObjData portal = null, stone = null;

    if ((victim = Handler.get_char_world(ch, target_name)) == null
        || victim == ch
        || victim.in_room == null
        || !Handler.can_see_room(ch, victim.in_room)
        || Bit.IS_SET(victim.in_room.room_flags, ROOM_SAFE)
        || Bit.IS_SET(victim.in_room.room_flags, ROOM_PRIVATE)
        || Bit.IS_SET(victim.in_room.room_flags, ROOM_SOLITARY)
        || Bit.IS_SET(victim.in_room.room_flags, ROOM_NO_RECALL)
        || Bit.IS_SET(ch.in_room.room_flags, ROOM_NO_RECALL)
        || victim.level >= level + 3 || (!Bit.IS_NPC(victim) && victim.level >= LEVEL_HERO)    
        || (Bit.IS_NPC(victim) && Bit.IS_SET(victim.imm_flags, IMM_SUMMON))
        || (Bit.IS_NPC(victim) && Magic.saves_spell(level, victim, DAM_NONE))
        || (Handler.is_clan(victim) && !Handler.is_same_clan(ch, victim)))
    {
        Comm.send_to_char("You failed.\n\r", ch);
        return;
    }

    stone = Handler.get_eq_char(ch, WEAR_HOLD);
    if (!Bit.IS_IMMORTAL(ch)
        && (stone == null || stone.item_type != ITEM_WARP_STONE))
    {
        Comm.send_to_char("You lack the proper component for this spell.\n\r",
                      ch);
        return;
    }

    if (stone != null && stone.item_type == ITEM_WARP_STONE)
    {
        Comm.act("You draw upon the power of $p.", ch, stone, null, TO_CHAR);
        Comm.act("It flares brightly and vanishes!", ch, stone, null, TO_CHAR);
        Handler.extract_obj(stone);
    }

    portal = Db.create_object(Handler.get_obj_index(OBJ_VNUM_PORTAL), 0);
    portal.timer = 2 + level / 25;
    portal.value[3] = victim.in_room.vnum;

    Handler.obj_to_room(portal, ch.in_room);

    Comm.act("$p rises up from the ground.", ch, portal, null, TO_ROOM);
    Comm.act("$p rises up before you.", ch, portal, null, TO_CHAR);
}

public static void spell_nexus(int sn, int level, CharData ch, object vo, int target)
{
    CharData victim = null;
    ObjData portal = null, stone = null;
    RoomIndexData to_room = null, from_room = null;

    from_room = ch.in_room;

    if ((victim = Handler.get_char_world(ch, target_name)) == null
        || victim == ch
        || (to_room = victim.in_room) == null
        || !Handler.can_see_room(ch, to_room) || !Handler.can_see_room(ch, from_room)
        || Bit.IS_SET(to_room.room_flags, ROOM_SAFE)
        || Bit.IS_SET(from_room.room_flags, ROOM_SAFE)
        || Bit.IS_SET(to_room.room_flags, ROOM_PRIVATE)
        || Bit.IS_SET(to_room.room_flags, ROOM_SOLITARY)
        || Bit.IS_SET(to_room.room_flags, ROOM_NO_RECALL)
        || Bit.IS_SET(from_room.room_flags, ROOM_NO_RECALL)
        || victim.level >= level + 3 || (!Bit.IS_NPC(victim) && victim.level >= LEVEL_HERO)    
        || (Bit.IS_NPC(victim) && Bit.IS_SET(victim.imm_flags, IMM_SUMMON))
        || (Bit.IS_NPC(victim) && Magic.saves_spell(level, victim, DAM_NONE))
        || (Handler.is_clan(victim) && !Handler.is_same_clan(ch, victim)))
    {
        Comm.send_to_char("You failed.\n\r", ch);
        return;
    }

    stone = Handler.get_eq_char(ch, WEAR_HOLD);
    if (!Bit.IS_IMMORTAL(ch)
        && (stone == null || stone.item_type != ITEM_WARP_STONE))
    {
        Comm.send_to_char("You lack the proper component for this spell.\n\r",
                      ch);
        return;
    }

    if (stone != null && stone.item_type == ITEM_WARP_STONE)
    {
        Comm.act("You draw upon the power of $p.", ch, stone, null, TO_CHAR);
        Comm.act("It flares brightly and vanishes!", ch, stone, null, TO_CHAR);
        Handler.extract_obj(stone);
    }

    
    portal = Db.create_object(Handler.get_obj_index(OBJ_VNUM_PORTAL), 0);
    portal.timer = 1 + level / 10;
    portal.value[3] = to_room.vnum;

    Handler.obj_to_room(portal, from_room);

    Comm.act("$p rises up from the ground.", ch, portal, null, TO_ROOM);
    Comm.act("$p rises up before you.", ch, portal, null, TO_CHAR);

    
    if (to_room == from_room)
        return;

    
    portal = Db.create_object(Handler.get_obj_index(OBJ_VNUM_PORTAL), 0);
    portal.timer = 1 + level / 10;
    portal.value[3] = from_room.vnum;

    Handler.obj_to_room(portal, to_room);

    if (to_room.people != null)
    {
        Comm.act("$p rises up from the ground.", to_room.people, portal, null,
             TO_ROOM);
        Comm.act("$p rises up from the ground.", to_room.people, portal, null,
             TO_CHAR);
    }
}
    }
}
