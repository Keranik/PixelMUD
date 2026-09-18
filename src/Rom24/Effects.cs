using static Rom24.Merc;

namespace Rom24
{
    public static class Effects
    {public static void acid_effect(object vo, int level, int dam, int target)
{
    if (target == TARGET_ROOM)
    {                            
        RoomIndexData room = vo as RoomIndexData;
        ObjData obj = null, obj_next = null;

        for (obj = room.contents; obj != null; obj = obj_next)
        {
            obj_next = obj.next_content;
            acid_effect (obj, level, dam, TARGET_OBJ);
        }
        return;
    }

    if (target == TARGET_CHAR)
    {                            
        CharData victim = vo as CharData;
        ObjData obj = null, obj_next = null;

        
        for (obj = victim.carrying; obj != null; obj = obj_next)
        {
            obj_next = obj.next_content;
            acid_effect (obj, level, dam, TARGET_OBJ);
        }
        return;
    }

    if (target == TARGET_OBJ)
    {                            
        ObjData obj = vo as ObjData;
        ObjData t_obj = null, n_obj = null;
        int chance;
        string msg;

        if (Bit.IS_OBJ_STAT(obj, ITEM_BURN_PROOF)
            || Bit.IS_OBJ_STAT(obj, ITEM_NOPURGE) || RomRandom.number_range(0, 4) == 0)
            return;

        chance = level / 4 + dam / 10;

        if (chance > 25)
            chance = (chance - 25) / 2 + 25;
        if (chance > 50)
            chance = (chance - 50) / 2 + 50;

        if (Bit.IS_OBJ_STAT(obj, ITEM_BLESS))
            chance -= 5;

        chance -= obj.level * 2;

        switch (obj.item_type)
        {
            default:
                return;
            case ITEM_CONTAINER:
            case ITEM_CORPSE_PC:
            case ITEM_CORPSE_NPC:
                msg = "$p fumes and dissolves.";
                break;
            case ITEM_ARMOR:
                msg = "$p is pitted and etched.";
                break;
            case ITEM_CLOTHING:
                msg = "$p is corroded into scrap.";
                break;
            case ITEM_STAFF:
            case ITEM_WAND:
                chance -= 10;
                msg = "$p corrodes and breaks.";
                break;
            case ITEM_SCROLL:
                chance += 10;
                msg = "$p is burned into waste.";
                break;
        }

        chance = Bit.URANGE(5, chance, 95);

        if (RomRandom.number_percent() > chance)
            return;

        if (obj.carried_by != null)
            Comm.act(msg, obj.carried_by, obj, null, TO_ALL);
        else if (obj.in_room != null && obj.in_room.people != null)
            Comm.act(msg, obj.in_room.people, obj, null, TO_ALL);

        if (obj.item_type == ITEM_ARMOR)
        {                        
            AffectData paf = null;
            bool af_found = false;
            int i;

            Handler.affect_enchant(obj);

            for (paf = obj.affected; paf != null; paf = paf.next)
            {
                if (paf.location == APPLY_AC)
                {
                    af_found = true;
                    paf.type = -1;
                    paf.modifier += 1;
                    paf.level = Bit.UMAX(paf.level, level);
                    break;
                }
            }

            if (!af_found)
                
            {
                paf = Recycle.new_affect();

                paf.type = -1;
                paf.level = level;
                paf.duration = -1;
                paf.location = APPLY_AC;
                paf.modifier = 1;
                paf.bitvector = 0;
                paf.next = obj.affected;
                obj.affected = paf;
            }

            if (obj.carried_by != null && obj.wear_loc != WEAR_NONE)
                for (i = 0; i < 4; i++)
                    obj.carried_by.armor[i] += 1;
            return;
        }

        
        if (obj.contains != null)
        {                        
            for (t_obj = obj.contains; t_obj != null; t_obj = n_obj)
            {
                n_obj = t_obj.next_content;
                Handler.obj_from_obj(t_obj);
                if (obj.in_room != null)
                    Handler.obj_to_room(t_obj, obj.in_room);
                else if (obj.carried_by != null)
                    Handler.obj_to_room(t_obj, obj.carried_by.in_room);
                else
                {
                    Handler.extract_obj(t_obj);
                    continue;
                }

                acid_effect (t_obj, level / 2, dam / 2, TARGET_OBJ);
            }
        }

        Handler.extract_obj(obj);
        return;
    }
}


public static void cold_effect(object vo, int level, int dam, int target)
{
    if (target == TARGET_ROOM)
    {                            
        RoomIndexData room = vo as RoomIndexData;
        ObjData obj = null, obj_next = null;

        for (obj = room.contents; obj != null; obj = obj_next)
        {
            obj_next = obj.next_content;
            cold_effect (obj, level, dam, TARGET_OBJ);
        }
        return;
    }

    if (target == TARGET_CHAR)
    {                            
        CharData victim = vo as CharData;
        ObjData obj = null, obj_next = null;

        
        if (!Magic.saves_spell(level / 4 + dam / 20, victim, DAM_COLD))
        {
            var af = new AffectData();

            Comm.act("$n turns blue and shivers.", victim, null, null, TO_ROOM);
            Comm.act("A chill sinks deep into your bones.", victim, null, null,
                 TO_CHAR);
            af.where = TO_AFFECTS;
            af.type = Lookup.skill_lookup("chill touch");
            af.level = level;
            af.duration = 6;
            af.location = APPLY_STR;
            af.modifier = -1;
            af.bitvector = 0;
            Handler.affect_join(victim, af);
        }

        
        if (!Bit.IS_NPC(victim))
            Update.gain_condition(victim, COND_HUNGER, dam / 20);

        
        for (obj = victim.carrying; obj != null; obj = obj_next)
        {
            obj_next = obj.next_content;
            cold_effect (obj, level, dam, TARGET_OBJ);
        }
        return;
    }

    if (target == TARGET_OBJ)
    {                            
        ObjData obj = vo as ObjData;
        int chance;
        string msg;

        if (Bit.IS_OBJ_STAT(obj, ITEM_BURN_PROOF)
            || Bit.IS_OBJ_STAT(obj, ITEM_NOPURGE) || RomRandom.number_range(0, 4) == 0)
            return;

        chance = level / 4 + dam / 10;

        if (chance > 25)
            chance = (chance - 25) / 2 + 25;
        if (chance > 50)
            chance = (chance - 50) / 2 + 50;

        if (Bit.IS_OBJ_STAT(obj, ITEM_BLESS))
            chance -= 5;

        chance -= obj.level * 2;

        switch (obj.item_type)
        {
            default:
                return;
            case ITEM_POTION:
                msg = "$p freezes and shatters!";
                chance += 25;
                break;
            case ITEM_DRINK_CON:
                msg = "$p freezes and shatters!";
                chance += 5;
                break;
        }

        chance = Bit.URANGE(5, chance, 95);

        if (RomRandom.number_percent() > chance)
            return;

        if (obj.carried_by != null)
            Comm.act(msg, obj.carried_by, obj, null, TO_ALL);
        else if (obj.in_room != null && obj.in_room.people != null)
            Comm.act(msg, obj.in_room.people, obj, null, TO_ALL);

        Handler.extract_obj(obj);
        return;
    }
}



public static void fire_effect(object vo, int level, int dam, int target)
{
    if (target == TARGET_ROOM)
    {                            
        RoomIndexData room = vo as RoomIndexData;
        ObjData obj = null, obj_next = null;

        for (obj = room.contents; obj != null; obj = obj_next)
        {
            obj_next = obj.next_content;
            fire_effect (obj, level, dam, TARGET_OBJ);
        }
        return;
    }

    if (target == TARGET_CHAR)
    {                            
        CharData victim = vo as CharData;
        ObjData obj = null, obj_next = null;

        
        if (!Bit.IS_AFFECTED(victim, AFF_BLIND)
            && !Magic.saves_spell(level / 4 + dam / 20, victim, DAM_FIRE))
        {
            var af = new AffectData();
            Comm.act("$n is blinded by smoke!", victim, null, null, TO_ROOM);
            Comm.act("Your eyes tear up from smoke...you can't see a thing!",
                 victim, null, null, TO_CHAR);

            af.where = TO_AFFECTS;
            af.type = Lookup.skill_lookup("fire breath");
            af.level = level;
            af.duration = RomRandom.number_range(0, level / 10);
            af.location = APPLY_HITROLL;
            af.modifier = -4;
            af.bitvector = AFF_BLIND;

            Handler.affect_to_char(victim, af);
        }

        
        if (!Bit.IS_NPC(victim))
            Update.gain_condition(victim, COND_THIRST, dam / 20);

        
        for (obj = victim.carrying; obj != null; obj = obj_next)
        {
            obj_next = obj.next_content;

            fire_effect (obj, level, dam, TARGET_OBJ);
        }
        return;
    }

    if (target == TARGET_OBJ)
    {                            
        ObjData obj = vo as ObjData;
        ObjData t_obj = null, n_obj = null;
        int chance;
        string msg;

        if (Bit.IS_OBJ_STAT(obj, ITEM_BURN_PROOF)
            || Bit.IS_OBJ_STAT(obj, ITEM_NOPURGE) || RomRandom.number_range(0, 4) == 0)
            return;

        chance = level / 4 + dam / 10;

        if (chance > 25)
            chance = (chance - 25) / 2 + 25;
        if (chance > 50)
            chance = (chance - 50) / 2 + 50;

        if (Bit.IS_OBJ_STAT(obj, ITEM_BLESS))
            chance -= 5;
        chance -= obj.level * 2;

        switch (obj.item_type)
        {
            default:
                return;
            case ITEM_CONTAINER:
                msg = "$p ignites and burns!";
                break;
            case ITEM_POTION:
                chance += 25;
                msg = "$p bubbles and boils!";
                break;
            case ITEM_SCROLL:
                chance += 50;
                msg = "$p crackles and burns!";
                break;
            case ITEM_STAFF:
                chance += 10;
                msg = "$p smokes and chars!";
                break;
            case ITEM_WAND:
                msg = "$p sparks and sputters!";
                break;
            case ITEM_FOOD:
                msg = "$p blackens and crisps!";
                break;
            case ITEM_PILL:
                msg = "$p melts and drips!";
                break;
        }

        chance = Bit.URANGE(5, chance, 95);

        if (RomRandom.number_percent() > chance)
            return;

        if (obj.carried_by != null)
            Comm.act(msg, obj.carried_by, obj, null, TO_ALL);
        else if (obj.in_room != null && obj.in_room.people != null)
            Comm.act(msg, obj.in_room.people, obj, null, TO_ALL);

        if (obj.contains != null)
        {
            

            for (t_obj = obj.contains; t_obj != null; t_obj = n_obj)
            {
                n_obj = t_obj.next_content;
                Handler.obj_from_obj(t_obj);
                if (obj.in_room != null)
                    Handler.obj_to_room(t_obj, obj.in_room);
                else if (obj.carried_by != null)
                    Handler.obj_to_room(t_obj, obj.carried_by.in_room);
                else
                {
                    Handler.extract_obj(t_obj);
                    continue;
                }
                fire_effect (t_obj, level / 2, dam / 2, TARGET_OBJ);
            }
        }

        Handler.extract_obj(obj);
        return;
    }
}

public static void poison_effect(object vo, int level, int dam, int target)
{
    if (target == TARGET_ROOM)
    {                            
        RoomIndexData room = vo as RoomIndexData;
        ObjData obj = null, obj_next = null;

        for (obj = room.contents; obj != null; obj = obj_next)
        {
            obj_next = obj.next_content;
            poison_effect (obj, level, dam, TARGET_OBJ);
        }
        return;
    }

    if (target == TARGET_CHAR)
    {                            
        CharData victim = vo as CharData;
        ObjData obj = null, obj_next = null;

        
        if (!Magic.saves_spell(level / 4 + dam / 20, victim, DAM_POISON))
        {
            var af = new AffectData();

            Comm.send_to_char("You feel poison coursing through your veins.\n\r",
                          victim);
            Comm.act("$n looks very ill.", victim, null, null, TO_ROOM);

            af.where = TO_AFFECTS;
            af.type = Gsn.poison;
            af.level = level;
            af.duration = level / 2;
            af.location = APPLY_STR;
            af.modifier = -1;
            af.bitvector = AFF_POISON;
            Handler.affect_join(victim, af);
        }

        
        for (obj = victim.carrying; obj != null; obj = obj_next)
        {
            obj_next = obj.next_content;
            poison_effect (obj, level, dam, TARGET_OBJ);
        }
        return;
    }

    if (target == TARGET_OBJ)
    {                            
        ObjData obj = vo as ObjData;
        int chance;


        if (Bit.IS_OBJ_STAT(obj, ITEM_BURN_PROOF)
            || Bit.IS_OBJ_STAT(obj, ITEM_BLESS) || RomRandom.number_range(0, 4) == 0)
            return;

        chance = level / 4 + dam / 10;
        if (chance > 25)
            chance = (chance - 25) / 2 + 25;
        if (chance > 50)
            chance = (chance - 50) / 2 + 50;

        chance -= obj.level * 2;

        switch (obj.item_type)
        {
            default:
                return;
            case ITEM_FOOD:
                break;
            case ITEM_DRINK_CON:
                if (obj.value[0] == obj.value[1])
                    return;
                break;
        }

        chance = Bit.URANGE(5, chance, 95);

        if (RomRandom.number_percent() > chance)
            return;

        obj.value[3] = 1;
        return;
    }
}


public static void shock_effect(object vo, int level, int dam, int target)
{
    if (target == TARGET_ROOM)
    {
        RoomIndexData room = vo as RoomIndexData;
        ObjData obj = null, obj_next = null;

        for (obj = room.contents; obj != null; obj = obj_next)
        {
            obj_next = obj.next_content;
            shock_effect (obj, level, dam, TARGET_OBJ);
        }
        return;
    }

    if (target == TARGET_CHAR)
    {
        CharData victim = vo as CharData;
        ObjData obj = null, obj_next = null;

        
        if (!Magic.saves_spell(level / 4 + dam / 20, victim, DAM_LIGHTNING))
        {
            Comm.send_to_char("Your muscles stop responding.\n\r", victim);
            Bit.DAZE_STATE(victim, Bit.UMAX(12, level / 4 + dam / 20));
        }

        
        for (obj = victim.carrying; obj != null; obj = obj_next)
        {
            obj_next = obj.next_content;
            shock_effect (obj, level, dam, TARGET_OBJ);
        }
        return;
    }

    if (target == TARGET_OBJ)
    {
        ObjData obj = vo as ObjData;
        int chance;
        string msg;

        if (Bit.IS_OBJ_STAT(obj, ITEM_BURN_PROOF)
            || Bit.IS_OBJ_STAT(obj, ITEM_NOPURGE) || RomRandom.number_range(0, 4) == 0)
            return;

        chance = level / 4 + dam / 10;

        if (chance > 25)
            chance = (chance - 25) / 2 + 25;
        if (chance > 50)
            chance = (chance - 50) / 2 + 50;

        if (Bit.IS_OBJ_STAT(obj, ITEM_BLESS))
            chance -= 5;

        chance -= obj.level * 2;

        switch (obj.item_type)
        {
            default:
                return;
            case ITEM_WAND:
            case ITEM_STAFF:
                chance += 10;
                msg = "$p overloads and explodes!";
                break;
            case ITEM_JEWELRY:
                chance -= 10;
                msg = "$p is fused into a worthless lump.";
                break;
        }

        chance = Bit.URANGE(5, chance, 95);

        if (RomRandom.number_percent() > chance)
            return;

        if (obj.carried_by != null)
            Comm.act(msg, obj.carried_by, obj, null, TO_ALL);
        else if (obj.in_room != null && obj.in_room.people != null)
            Comm.act(msg, obj.in_room.people, obj, null, TO_ALL);

        Handler.extract_obj(obj);
        return;
    }
}
    }
}
