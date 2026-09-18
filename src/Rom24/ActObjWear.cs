using static Rom24.Merc;

namespace Rom24
{
    public static partial class ActObj
    {public static void wear_obj(CharData ch, ObjData obj, bool fReplace)
{
    string buf;

    if (ch.level < obj.level)
    {
        buf = RomString.sprintf( "You must be level %d to use this object.\n\r",
                 obj.level);
        Comm.send_to_char(buf, ch);
        Comm.act("$n tries to use $p, but is too inexperienced.",
             ch, obj, null, TO_ROOM);
        return;
    }

    if (obj.item_type == ITEM_LIGHT)
    {
        if (!ActObj.remove_obj(ch, WEAR_LIGHT, fReplace))
            return;
        Comm.act("$n lights $p and holds it.", ch, obj, null, TO_ROOM);
        Comm.act("You light $p and hold it.", ch, obj, null, TO_CHAR);
        Handler.equip_char(ch, obj, WEAR_LIGHT);
        return;
    }

    if (Bit.CAN_WEAR(obj, ITEM_WEAR_FINGER))
    {
        if (Handler.get_eq_char(ch, WEAR_FINGER_L) != null
            && Handler.get_eq_char(ch, WEAR_FINGER_R) != null
            && !ActObj.remove_obj(ch, WEAR_FINGER_L, fReplace)
            && !ActObj.remove_obj(ch, WEAR_FINGER_R, fReplace))
            return;

        if (Handler.get_eq_char(ch, WEAR_FINGER_L) == null)
        {
            Comm.act("$n wears $p on $s left finger.", ch, obj, null, TO_ROOM);
            Comm.act("You wear $p on your left finger.", ch, obj, null, TO_CHAR);
            Handler.equip_char(ch, obj, WEAR_FINGER_L);
            return;
        }

        if (Handler.get_eq_char(ch, WEAR_FINGER_R) == null)
        {
            Comm.act("$n wears $p on $s right finger.", ch, obj, null, TO_ROOM);
            Comm.act("You wear $p on your right finger.", ch, obj, null, TO_CHAR);
            Handler.equip_char(ch, obj, WEAR_FINGER_R);
            return;
        }

        Db.bug("Wear_obj: no free finger.", 0);
        Comm.send_to_char("You already wear two rings.\n\r", ch);
        return;
    }

    if (Bit.CAN_WEAR(obj, ITEM_WEAR_NECK))
    {
        if (Handler.get_eq_char(ch, WEAR_NECK_1) != null
            && Handler.get_eq_char(ch, WEAR_NECK_2) != null
            && !ActObj.remove_obj(ch, WEAR_NECK_1, fReplace)
            && !ActObj.remove_obj(ch, WEAR_NECK_2, fReplace))
            return;

        if (Handler.get_eq_char(ch, WEAR_NECK_1) == null)
        {
            Comm.act("$n wears $p around $s neck.", ch, obj, null, TO_ROOM);
            Comm.act("You wear $p around your neck.", ch, obj, null, TO_CHAR);
            Handler.equip_char(ch, obj, WEAR_NECK_1);
            return;
        }

        if (Handler.get_eq_char(ch, WEAR_NECK_2) == null)
        {
            Comm.act("$n wears $p around $s neck.", ch, obj, null, TO_ROOM);
            Comm.act("You wear $p around your neck.", ch, obj, null, TO_CHAR);
            Handler.equip_char(ch, obj, WEAR_NECK_2);
            return;
        }

        Db.bug("Wear_obj: no free neck.", 0);
        Comm.send_to_char("You already wear two neck items.\n\r", ch);
        return;
    }

    if (Bit.CAN_WEAR(obj, ITEM_WEAR_BODY))
    {
        if (!ActObj.remove_obj(ch, WEAR_BODY, fReplace))
            return;
        Comm.act("$n wears $p on $s torso.", ch, obj, null, TO_ROOM);
        Comm.act("You wear $p on your torso.", ch, obj, null, TO_CHAR);
        Handler.equip_char(ch, obj, WEAR_BODY);
        return;
    }

    if (Bit.CAN_WEAR(obj, ITEM_WEAR_HEAD))
    {
        if (!ActObj.remove_obj(ch, WEAR_HEAD, fReplace))
            return;
        Comm.act("$n wears $p on $s head.", ch, obj, null, TO_ROOM);
        Comm.act("You wear $p on your head.", ch, obj, null, TO_CHAR);
        Handler.equip_char(ch, obj, WEAR_HEAD);
        return;
    }

    if (Bit.CAN_WEAR(obj, ITEM_WEAR_LEGS))
    {
        if (!ActObj.remove_obj(ch, WEAR_LEGS, fReplace))
            return;
        Comm.act("$n wears $p on $s legs.", ch, obj, null, TO_ROOM);
        Comm.act("You wear $p on your legs.", ch, obj, null, TO_CHAR);
        Handler.equip_char(ch, obj, WEAR_LEGS);
        return;
    }

    if (Bit.CAN_WEAR(obj, ITEM_WEAR_FEET))
    {
        if (!ActObj.remove_obj(ch, WEAR_FEET, fReplace))
            return;
        Comm.act("$n wears $p on $s feet.", ch, obj, null, TO_ROOM);
        Comm.act("You wear $p on your feet.", ch, obj, null, TO_CHAR);
        Handler.equip_char(ch, obj, WEAR_FEET);
        return;
    }

    if (Bit.CAN_WEAR(obj, ITEM_WEAR_HANDS))
    {
        if (!ActObj.remove_obj(ch, WEAR_HANDS, fReplace))
            return;
        Comm.act("$n wears $p on $s hands.", ch, obj, null, TO_ROOM);
        Comm.act("You wear $p on your hands.", ch, obj, null, TO_CHAR);
        Handler.equip_char(ch, obj, WEAR_HANDS);
        return;
    }

    if (Bit.CAN_WEAR(obj, ITEM_WEAR_ARMS))
    {
        if (!ActObj.remove_obj(ch, WEAR_ARMS, fReplace))
            return;
        Comm.act("$n wears $p on $s arms.", ch, obj, null, TO_ROOM);
        Comm.act("You wear $p on your arms.", ch, obj, null, TO_CHAR);
        Handler.equip_char(ch, obj, WEAR_ARMS);
        return;
    }

    if (Bit.CAN_WEAR(obj, ITEM_WEAR_ABOUT))
    {
        if (!ActObj.remove_obj(ch, WEAR_ABOUT, fReplace))
            return;
        Comm.act("$n wears $p about $s torso.", ch, obj, null, TO_ROOM);
        Comm.act("You wear $p about your torso.", ch, obj, null, TO_CHAR);
        Handler.equip_char(ch, obj, WEAR_ABOUT);
        return;
    }

    if (Bit.CAN_WEAR(obj, ITEM_WEAR_WAIST))
    {
        if (!ActObj.remove_obj(ch, WEAR_WAIST, fReplace))
            return;
        Comm.act("$n wears $p about $s waist.", ch, obj, null, TO_ROOM);
        Comm.act("You wear $p about your waist.", ch, obj, null, TO_CHAR);
        Handler.equip_char(ch, obj, WEAR_WAIST);
        return;
    }

    if (Bit.CAN_WEAR(obj, ITEM_WEAR_WRIST))
    {
        if (Handler.get_eq_char(ch, WEAR_WRIST_L) != null
            && Handler.get_eq_char(ch, WEAR_WRIST_R) != null
            && !ActObj.remove_obj(ch, WEAR_WRIST_L, fReplace)
            && !ActObj.remove_obj(ch, WEAR_WRIST_R, fReplace))
            return;

        if (Handler.get_eq_char(ch, WEAR_WRIST_L) == null)
        {
            Comm.act("$n wears $p around $s left wrist.", ch, obj, null, TO_ROOM);
            Comm.act("You wear $p around your left wrist.",
                 ch, obj, null, TO_CHAR);
            Handler.equip_char(ch, obj, WEAR_WRIST_L);
            return;
        }

        if (Handler.get_eq_char(ch, WEAR_WRIST_R) == null)
        {
            Comm.act("$n wears $p around $s right wrist.",
                 ch, obj, null, TO_ROOM);
            Comm.act("You wear $p around your right wrist.",
                 ch, obj, null, TO_CHAR);
            Handler.equip_char(ch, obj, WEAR_WRIST_R);
            return;
        }

        Db.bug("Wear_obj: no free wrist.", 0);
        Comm.send_to_char("You already wear two wrist items.\n\r", ch);
        return;
    }

    if (Bit.CAN_WEAR(obj, ITEM_WEAR_SHIELD))
    {
        ObjData weapon = null;

        if (!ActObj.remove_obj(ch, WEAR_SHIELD, fReplace))
            return;

        weapon = Handler.get_eq_char(ch, WEAR_WIELD);
        if (weapon != null && ch.size < SIZE_LARGE
            && Bit.IS_WEAPON_STAT(weapon, WEAPON_TWO_HANDS))
        {
            Comm.send_to_char("Your hands are tied up with your weapon!\n\r", ch);
            return;
        }

        Comm.act("$n wears $p as a shield.", ch, obj, null, TO_ROOM);
        Comm.act("You wear $p as a shield.", ch, obj, null, TO_CHAR);
        Handler.equip_char(ch, obj, WEAR_SHIELD);
        return;
    }

    if (Bit.CAN_WEAR(obj, ITEM_WIELD))
    {
        int sn, skill;

        if (!ActObj.remove_obj(ch, WEAR_WIELD, fReplace))
            return;

        if (!Bit.IS_NPC(ch)
            && Handler.get_obj_weight(obj) >
            (Tables.str_app[Handler.get_curr_stat(ch, STAT_STR)].wield * 10))
        {
            Comm.send_to_char("It is too heavy for you to wield.\n\r", ch);
            return;
        }

        if (!Bit.IS_NPC(ch) && ch.size < SIZE_LARGE
            && Bit.IS_WEAPON_STAT(obj, WEAPON_TWO_HANDS)
            && Handler.get_eq_char(ch, WEAR_SHIELD) != null)
        {
            Comm.send_to_char("You need two hands free for that weapon.\n\r", ch);
            return;
        }

        Comm.act("$n wields $p.", ch, obj, null, TO_ROOM);
        Comm.act("You wield $p.", ch, obj, null, TO_CHAR);
        Handler.equip_char(ch, obj, WEAR_WIELD);

        sn = Handler.get_weapon_sn (ch);

        if (sn == Gsn.hand_to_hand)
            return;

        skill = Handler.get_weapon_skill (ch, sn);

        if (skill >= 100)
            Comm.act("$p feels like a part of you!", ch, obj, null, TO_CHAR);
        else if (skill > 85)
            Comm.act("You feel quite confident with $p.", ch, obj, null, TO_CHAR);
        else if (skill > 70)
            Comm.act("You are skilled with $p.", ch, obj, null, TO_CHAR);
        else if (skill > 50)
            Comm.act("Your skill with $p is adequate.", ch, obj, null, TO_CHAR);
        else if (skill > 25)
            Comm.act("$p feels a little clumsy in your hands.", ch, obj, null,
                 TO_CHAR);
        else if (skill > 1)
            Comm.act("You fumble and almost drop $p.", ch, obj, null, TO_CHAR);
        else
            Comm.act("You don't even know which end is up on $p.",
                 ch, obj, null, TO_CHAR);

        return;
    }

    if (Bit.CAN_WEAR(obj, ITEM_HOLD))
    {
        if (!ActObj.remove_obj(ch, WEAR_HOLD, fReplace))
            return;
        Comm.act("$n holds $p in $s hand.", ch, obj, null, TO_ROOM);
        Comm.act("You hold $p in your hand.", ch, obj, null, TO_CHAR);
        Handler.equip_char(ch, obj, WEAR_HOLD);
        return;
    }

    if (Bit.CAN_WEAR(obj, ITEM_WEAR_FLOAT))
    {
        if (!ActObj.remove_obj(ch, WEAR_FLOAT, fReplace))
            return;
        Comm.act("$n releases $p to float next to $m.", ch, obj, null, TO_ROOM);
        Comm.act("You release $p and it floats next to you.", ch, obj, null,
             TO_CHAR);
        Handler.equip_char(ch, obj, WEAR_FLOAT);
        return;
    }

    if (fReplace)
        Comm.send_to_char("You can't wear, wield, or hold that.\n\r", ch);

    return;
}



    }
}
