using static Rom24.Merc;

namespace Rom24
{
    public static partial class ActObj
    {
        public static bool remove_obj(CharData ch, int iWear, bool fReplace)
        {
            var obj = Handler.get_eq_char(ch, iWear);
            if (obj == null)
                return true;

            if (!fReplace)
                return false;

            if (Bit.IS_SET(obj.extra_flags, ITEM_NOREMOVE))
            {
                Comm.act("You can't remove $p.", ch, obj, null, TO_CHAR);
                return false;
            }

            Handler.unequip_char(ch, obj);
            Comm.act("$n stops using $p.", ch, obj, null, TO_ROOM);
            Comm.act("You stop using $p.", ch, obj, null, TO_CHAR);
            return true;
        }

        public static void do_put(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg1);
            argument = RomString.one_argument(argument, out string arg2);

            if (!RomString.str_cmp(arg2, "in") || !RomString.str_cmp(arg2, "on"))
                argument = RomString.one_argument(argument, out arg2);

            if (arg1.Length == 0 || arg2.Length == 0)
            {
                Comm.send_to_char("Put what in what?\n\r", ch);
                return;
            }

            if (!RomString.str_cmp(arg2, "all") || !RomString.str_prefix("all.", arg2))
            {
                Comm.send_to_char("You can't do that.\n\r", ch);
                return;
            }

            var container = Handler.get_obj_here(ch, arg2);
            if (container == null)
            {
                Comm.act("I see no $T here.", ch, null, arg2, TO_CHAR);
                return;
            }

            if (container.item_type != ITEM_CONTAINER)
            {
                Comm.send_to_char("That's not a container.\n\r", ch);
                return;
            }

            if (Bit.IS_SET(container.value[1], CONT_CLOSED))
            {
                Comm.act("The $d is closed.", ch, null, container.name, TO_CHAR);
                return;
            }

            if (RomString.str_cmp(arg1, "all") && RomString.str_prefix("all.", arg1))
            {
                var obj = Handler.get_obj_carry(ch, arg1, ch);
                if (obj == null)
                {
                    Comm.send_to_char("You do not have that item.\n\r", ch);
                    return;
                }

                if (obj == container)
                {
                    Comm.send_to_char("You can't fold it into itself.\n\r", ch);
                    return;
                }

                if (!Handler.can_drop_obj(ch, obj))
                {
                    Comm.send_to_char("You can't let go of it.\n\r", ch);
                    return;
                }

                if (Bit.WEIGHT_MULT(obj) != 100)
                {
                    Comm.send_to_char("You have a feeling that would be a bad idea.\n\r",
                        ch);
                    return;
                }

                if (Handler.get_obj_weight(obj) + Handler.get_true_weight(container)
                    > (container.value[0] * 10)
                    || Handler.get_obj_weight(obj) > (container.value[3] * 10))
                {
                    Comm.send_to_char("It won't fit.\n\r", ch);
                    return;
                }

                if (container.pIndexData.vnum == OBJ_VNUM_PIT
                    && !Bit.CAN_WEAR(container, ITEM_TAKE))
                {
                    if (obj.timer != 0)
                        Bit.SET_BIT(ref obj.extra_flags, ITEM_HAD_TIMER);
                    else
                        obj.timer = RomRandom.number_range(100, 200);
                }

                Handler.obj_from_char(obj);
                Handler.obj_to_obj(obj, container);

                if (Bit.IS_SET(container.value[1], CONT_PUT_ON))
                {
                    Comm.act("$n puts $p on $P.", ch, obj, container, TO_ROOM);
                    Comm.act("You put $p on $P.", ch, obj, container, TO_CHAR);
                }
                else
                {
                    Comm.act("$n puts $p in $P.", ch, obj, container, TO_ROOM);
                    Comm.act("You put $p in $P.", ch, obj, container, TO_CHAR);
                }
            }
            else
            {
                ObjData obj_next;
                for (var obj = ch.carrying; obj != null; obj = obj_next)
                {
                    obj_next = obj.next_content;

                    if ((arg1.Length <= 3 || Handler.is_name(arg1.Substring(4), obj.name))
                        && Handler.can_see_obj(ch, obj)
                        && Bit.WEIGHT_MULT(obj) == 100
                        && obj.wear_loc == WEAR_NONE
                        && obj != container && Handler.can_drop_obj(ch, obj)
                        && Handler.get_obj_weight(obj) + Handler.get_true_weight(container)
                        <= (container.value[0] * 10)
                        && Handler.get_obj_weight(obj) < (container.value[3] * 10))
                    {
                        if (container.pIndexData.vnum == OBJ_VNUM_PIT
                            && !Bit.CAN_WEAR(obj, ITEM_TAKE))
                        {
                            if (obj.timer != 0)
                                Bit.SET_BIT(ref obj.extra_flags, ITEM_HAD_TIMER);
                            else
                                obj.timer = RomRandom.number_range(100, 200);
                        }

                        Handler.obj_from_char(obj);
                        Handler.obj_to_obj(obj, container);

                        if (Bit.IS_SET(container.value[1], CONT_PUT_ON))
                        {
                            Comm.act("$n puts $p on $P.", ch, obj, container, TO_ROOM);
                            Comm.act("You put $p on $P.", ch, obj, container, TO_CHAR);
                        }
                        else
                        {
                            Comm.act("$n puts $p in $P.", ch, obj, container, TO_ROOM);
                            Comm.act("You put $p in $P.", ch, obj, container, TO_CHAR);
                        }
                    }
                }
            }
        }

        public static void do_give(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg1);
            argument = RomString.one_argument(argument, out string arg2);

            if (arg1.Length == 0 || arg2.Length == 0)
            {
                Comm.send_to_char("Give what to whom?\n\r", ch);
                return;
            }

            if (Interp.is_number(arg1))
            {
                int amount = Interp.atoi(arg1);
                if (amount <= 0
                    || (RomString.str_cmp(arg2, "coins") && RomString.str_cmp(arg2, "coin") &&
                        RomString.str_cmp(arg2, "gold") && RomString.str_cmp(arg2, "silver")))
                {
                    Comm.send_to_char("Sorry, you can't do that.\n\r", ch);
                    return;
                }

                bool silver = RomString.str_cmp(arg2, "gold");

                argument = RomString.one_argument(argument, out arg2);
                if (arg2.Length == 0)
                {
                    Comm.send_to_char("Give what to whom?\n\r", ch);
                    return;
                }

                var victim = Handler.get_char_room(ch, arg2);
                if (victim == null)
                {
                    Comm.send_to_char("They aren't here.\n\r", ch);
                    return;
                }

                if ((!silver && ch.gold < amount) || (silver && ch.silver < amount))
                {
                    Comm.send_to_char("You haven't got that much.\n\r", ch);
                    return;
                }

                if (silver)
                {
                    ch.silver -= amount;
                    victim.silver += amount;
                }
                else
                {
                    ch.gold -= amount;
                    victim.gold += amount;
                }

                string buf = RomString.sprintf("$n gives you %d %s.", amount,
                    silver ? "silver" : "gold");
                Comm.act(buf, ch, null, victim, TO_VICT);
                Comm.act("$n gives $N some coins.", ch, null, victim, TO_NOTVICT);
                buf = RomString.sprintf("You give $N %d %s.", amount,
                    silver ? "silver" : "gold");
                Comm.act(buf, ch, null, victim, TO_CHAR);

                /*
                 * Bribe trigger
                 */
                if (Bit.IS_NPC(victim) && Bit.HAS_TRIGGER(victim, TRIG_BRIBE))
                    MobProg.mp_bribe_trigger(victim, ch, silver ? amount : amount * 100);

                if (Bit.IS_NPC(victim) && Bit.IS_SET(victim.act, ACT_IS_CHANGER))
                {
                    int change = (silver ? 95 * amount / 100 / 100 : 95 * amount);

                    if (!silver && change > victim.silver)
                        victim.silver += change;

                    if (silver && change > victim.gold)
                        victim.gold += change;

                    if (change < 1 && Handler.can_see(victim, ch))
                    {
                        Comm.act(
                            "$n tells you 'I'm sorry, you did not give me enough to change.'",
                            victim, null, ch, TO_VICT);
                        ch.reply = victim;
                        buf = RomString.sprintf("%d %s %s",
                            amount, silver ? "silver" : "gold", ch.name);
                        Interp.do_function(victim, do_give, buf);
                    }
                    else if (Handler.can_see(victim, ch))
                    {
                        buf = RomString.sprintf("%d %s %s",
                            change, silver ? "gold" : "silver", ch.name);
                        Interp.do_function(victim, do_give, buf);
                        if (silver)
                        {
                            buf = RomString.sprintf("%d silver %s",
                                (95 * amount / 100 - change * 100), ch.name);
                            Interp.do_function(victim, do_give, buf);
                        }
                        Comm.act("$n tells you 'Thank you, come again.'",
                            victim, null, ch, TO_VICT);
                        ch.reply = victim;
                    }
                }
                return;
            }

            var obj = Handler.get_obj_carry(ch, arg1, ch);
            if (obj == null)
            {
                Comm.send_to_char("You do not have that item.\n\r", ch);
                return;
            }

            if (obj.wear_loc != WEAR_NONE)
            {
                Comm.send_to_char("You must remove it first.\n\r", ch);
                return;
            }

            var vict = Handler.get_char_room(ch, arg2);
            if (vict == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }

            if (Bit.IS_NPC(vict) && vict.pIndexData.pShop != null)
            {
                Comm.act("$N tells you 'Sorry, you'll have to sell that.'",
                    ch, null, vict, TO_CHAR);
                ch.reply = vict;
                return;
            }

            if (!Handler.can_drop_obj(ch, obj))
            {
                Comm.send_to_char("You can't let go of it.\n\r", ch);
                return;
            }

            if (vict.carry_number + Handler.get_obj_number(obj) > Handler.can_carry_n(vict))
            {
                Comm.act("$N has $S hands full.", ch, null, vict, TO_CHAR);
                return;
            }

            if (Handler.get_carry_weight(vict) + Handler.get_obj_weight(obj) >
                Handler.can_carry_w(vict))
            {
                Comm.act("$N can't carry that much weight.", ch, null, vict, TO_CHAR);
                return;
            }

            if (!Handler.can_see_obj(vict, obj))
            {
                Comm.act("$N can't see it.", ch, null, vict, TO_CHAR);
                return;
            }

            Handler.obj_from_char(obj);
            Handler.obj_to_char(obj, vict);
            Game.MOBtrigger = false;
            Comm.act("$n gives $p to $N.", ch, obj, vict, TO_NOTVICT);
            Comm.act("$n gives you $p.", ch, obj, vict, TO_VICT);
            Comm.act("You give $p to $N.", ch, obj, vict, TO_CHAR);
            Game.MOBtrigger = true;

            /*
             * Give trigger
             */
            if (Bit.IS_NPC(vict) && Bit.HAS_TRIGGER(vict, TRIG_GIVE))
                MobProg.mp_give_trigger(vict, ch, obj);
        }

        public static void do_fill(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Fill what?\n\r", ch);
                return;
            }

            var obj = Handler.get_obj_carry(ch, arg, ch);
            if (obj == null)
            {
                Comm.send_to_char("You do not have that item.\n\r", ch);
                return;
            }

            bool found = false;
            ObjData fountain;
            for (fountain = ch.in_room.contents; fountain != null;
                 fountain = fountain.next_content)
            {
                if (fountain.item_type == ITEM_FOUNTAIN)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Comm.send_to_char("There is no fountain here!\n\r", ch);
                return;
            }

            if (obj.item_type != ITEM_DRINK_CON)
            {
                Comm.send_to_char("You can't fill that.\n\r", ch);
                return;
            }

            if (obj.value[1] != 0 && obj.value[2] != fountain.value[2])
            {
                Comm.send_to_char("There is already another liquid in it.\n\r", ch);
                return;
            }

            if (obj.value[1] >= obj.value[0])
            {
                Comm.send_to_char("Your container is full.\n\r", ch);
                return;
            }

            string buf = RomString.sprintf("You fill $p with %s from $P.",
                Tables.liq_table[fountain.value[2]].liq_name);
            Comm.act(buf, ch, obj, fountain, TO_CHAR);
            buf = RomString.sprintf("$n fills $p with %s from $P.",
                Tables.liq_table[fountain.value[2]].liq_name);
            Comm.act(buf, ch, obj, fountain, TO_ROOM);
            obj.value[2] = fountain.value[2];
            obj.value[1] = obj.value[0];
        }

        public static void do_pour(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);

            if (arg.Length == 0 || argument.Length == 0)
            {
                Comm.send_to_char("Pour what into what?\n\r", ch);
                return;
            }

            var outObj = Handler.get_obj_carry(ch, arg, ch);
            if (outObj == null)
            {
                Comm.send_to_char("You don't have that item.\n\r", ch);
                return;
            }

            if (outObj.item_type != ITEM_DRINK_CON)
            {
                Comm.send_to_char("That's not a drink container.\n\r", ch);
                return;
            }

            if (!RomString.str_cmp(argument, "out"))
            {
                if (outObj.value[1] == 0)
                {
                    Comm.send_to_char("It's already empty.\n\r", ch);
                    return;
                }

                outObj.value[1] = 0;
                outObj.value[3] = 0;
                string buf = RomString.sprintf("You invert $p, spilling %s all over the ground.",
                    Tables.liq_table[outObj.value[2]].liq_name);
                Comm.act(buf, ch, outObj, null, TO_CHAR);

                buf = RomString.sprintf("$n inverts $p, spilling %s all over the ground.",
                    Tables.liq_table[outObj.value[2]].liq_name);
                Comm.act(buf, ch, outObj, null, TO_ROOM);
                return;
            }

            CharData vch = null;
            var inObj = Handler.get_obj_here(ch, argument);
            if (inObj == null)
            {
                vch = Handler.get_char_room(ch, argument);

                if (vch == null)
                {
                    Comm.send_to_char("Pour into what?\n\r", ch);
                    return;
                }

                inObj = Handler.get_eq_char(vch, WEAR_HOLD);

                if (inObj == null)
                {
                    Comm.send_to_char("They aren't holding anything.", ch);
                    return;
                }
            }

            if (inObj.item_type != ITEM_DRINK_CON)
            {
                Comm.send_to_char("You can only pour into other drink containers.\n\r",
                    ch);
                return;
            }

            if (inObj == outObj)
            {
                Comm.send_to_char("You cannot change the laws of physics!\n\r", ch);
                return;
            }

            if (inObj.value[1] != 0 && inObj.value[2] != outObj.value[2])
            {
                Comm.send_to_char("They don't hold the same liquid.\n\r", ch);
                return;
            }

            if (outObj.value[1] == 0)
            {
                Comm.act("There's nothing in $p to pour.", ch, outObj, null, TO_CHAR);
                return;
            }

            if (inObj.value[1] >= inObj.value[0])
            {
                Comm.act("$p is already filled to the top.", ch, inObj, null, TO_CHAR);
                return;
            }

            int amount = Bit.UMIN(outObj.value[1], inObj.value[0] - inObj.value[1]);

            inObj.value[1] += amount;
            outObj.value[1] -= amount;
            inObj.value[2] = outObj.value[2];

            if (vch == null)
            {
                string buf = RomString.sprintf("You pour %s from $p into $P.",
                    Tables.liq_table[outObj.value[2]].liq_name);
                Comm.act(buf, ch, outObj, inObj, TO_CHAR);
                buf = RomString.sprintf("$n pours %s from $p into $P.",
                    Tables.liq_table[outObj.value[2]].liq_name);
                Comm.act(buf, ch, outObj, inObj, TO_ROOM);
            }
            else
            {
                string buf = RomString.sprintf("You pour some %s for $N.",
                    Tables.liq_table[outObj.value[2]].liq_name);
                Comm.act(buf, ch, null, vch, TO_CHAR);
                buf = RomString.sprintf("$n pours you some %s.",
                    Tables.liq_table[outObj.value[2]].liq_name);
                Comm.act(buf, ch, null, vch, TO_VICT);
                buf = RomString.sprintf("$n pours some %s for $N.",
                    Tables.liq_table[outObj.value[2]].liq_name);
                Comm.act(buf, ch, null, vch, TO_NOTVICT);
            }
        }

        public static void do_drop(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Drop what?\n\r", ch);
                return;
            }

            if (Interp.is_number(arg))
            {
                int amount = Interp.atoi(arg);
                argument = RomString.one_argument(argument, out arg);
                if (amount <= 0
                    || (RomString.str_cmp(arg, "coins") && RomString.str_cmp(arg, "coin") &&
                        RomString.str_cmp(arg, "gold") && RomString.str_cmp(arg, "silver")))
                {
                    Comm.send_to_char("Sorry, you can't do that.\n\r", ch);
                    return;
                }

                int gold = 0, silver = 0;

                if (!RomString.str_cmp(arg, "coins") || !RomString.str_cmp(arg, "coin")
                    || !RomString.str_cmp(arg, "silver"))
                {
                    if (ch.silver < amount)
                    {
                        Comm.send_to_char("You don't have that much silver.\n\r", ch);
                        return;
                    }

                    ch.silver -= amount;
                    silver = amount;
                }

                else
                {
                    if (ch.gold < amount)
                    {
                        Comm.send_to_char("You don't have that much gold.\n\r", ch);
                        return;
                    }

                    ch.gold -= amount;
                    gold = amount;
                }

                ObjData obj_next;
                for (var obj = ch.in_room.contents; obj != null; obj = obj_next)
                {
                    obj_next = obj.next_content;

                    switch (obj.pIndexData.vnum)
                    {
                        case OBJ_VNUM_SILVER_ONE:
                            silver += 1;
                            Handler.extract_obj(obj);
                            break;

                        case OBJ_VNUM_GOLD_ONE:
                            gold += 1;
                            Handler.extract_obj(obj);
                            break;

                        case OBJ_VNUM_SILVER_SOME:
                            silver += obj.value[0];
                            Handler.extract_obj(obj);
                            break;

                        case OBJ_VNUM_GOLD_SOME:
                            gold += obj.value[1];
                            Handler.extract_obj(obj);
                            break;

                        case OBJ_VNUM_COINS:
                            silver += obj.value[0];
                            gold += obj.value[1];
                            Handler.extract_obj(obj);
                            break;
                    }
                }

                Handler.obj_to_room(Handler.create_money(gold, silver), ch.in_room);
                Comm.act("$n drops some coins.", ch, null, null, TO_ROOM);
                Comm.send_to_char("OK.\n\r", ch);
                return;
            }

            if (RomString.str_cmp(arg, "all") && RomString.str_prefix("all.", arg))
            {
                var obj = Handler.get_obj_carry(ch, arg, ch);
                if (obj == null)
                {
                    Comm.send_to_char("You do not have that item.\n\r", ch);
                    return;
                }

                if (!Handler.can_drop_obj(ch, obj))
                {
                    Comm.send_to_char("You can't let go of it.\n\r", ch);
                    return;
                }

                Handler.obj_from_char(obj);
                Handler.obj_to_room(obj, ch.in_room);
                Comm.act("$n drops $p.", ch, obj, null, TO_ROOM);
                Comm.act("You drop $p.", ch, obj, null, TO_CHAR);
                if (Bit.IS_OBJ_STAT(obj, ITEM_MELT_DROP))
                {
                    Comm.act("$p dissolves into smoke.", ch, obj, null, TO_ROOM);
                    Comm.act("$p dissolves into smoke.", ch, obj, null, TO_CHAR);
                    Handler.extract_obj(obj);
                }
            }
            else
            {
                bool found = false;
                ObjData obj_next;
                for (var obj = ch.carrying; obj != null; obj = obj_next)
                {
                    obj_next = obj.next_content;

                    if ((arg.Length <= 3 || Handler.is_name(arg.Substring(4), obj.name))
                        && Handler.can_see_obj(ch, obj)
                        && obj.wear_loc == WEAR_NONE && Handler.can_drop_obj(ch, obj))
                    {
                        found = true;
                        Handler.obj_from_char(obj);
                        Handler.obj_to_room(obj, ch.in_room);
                        Comm.act("$n drops $p.", ch, obj, null, TO_ROOM);
                        Comm.act("You drop $p.", ch, obj, null, TO_CHAR);
                        if (Bit.IS_OBJ_STAT(obj, ITEM_MELT_DROP))
                        {
                            Comm.act("$p dissolves into smoke.", ch, obj, null, TO_ROOM);
                            Comm.act("$p dissolves into smoke.", ch, obj, null, TO_CHAR);
                            Handler.extract_obj(obj);
                        }
                    }
                }

                if (!found)
                {
                    if (arg.Length <= 3)
                        Comm.act("You are not carrying anything.",
                            ch, null, arg, TO_CHAR);
                    else
                        Comm.act("You are not carrying any $T.",
                            ch, null, arg.Substring(4), TO_CHAR);
                }
            }
        }

        public static void do_drink(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);
            ObjData obj;

            if (arg.Length == 0)
            {
                for (obj = ch.in_room.contents; obj != null; obj = obj.next_content)
                {
                    if (obj.item_type == ITEM_FOUNTAIN)
                        break;
                }

                if (obj == null)
                {
                    Comm.send_to_char("Drink what?\n\r", ch);
                    return;
                }
            }
            else
            {
                if ((obj = Handler.get_obj_here(ch, arg)) == null)
                {
                    Comm.send_to_char("You can't find it.\n\r", ch);
                    return;
                }
            }

            if (!Bit.IS_NPC(ch) && ch.pcdata.condition[COND_DRUNK] > 10)
            {
                Comm.send_to_char("You fail to reach your mouth.  *Hic*\n\r", ch);
                return;
            }

            int amount;
            int liquid;
            switch (obj.item_type)
            {
                default:
                    Comm.send_to_char("You can't drink from that.\n\r", ch);
                    return;

                case ITEM_FOUNTAIN:
                    if ((liquid = obj.value[2]) < 0)
                    {
                        Db.bug("Do_drink: bad liquid number %d.", liquid);
                        liquid = obj.value[2] = 0;
                    }
                    amount = Tables.liq_table[liquid].liq_affect[4] * 3;
                    break;

                case ITEM_DRINK_CON:
                    if (obj.value[1] <= 0)
                    {
                        Comm.send_to_char("It is already empty.\n\r", ch);
                        return;
                    }

                    if ((liquid = obj.value[2]) < 0)
                    {
                        Db.bug("Do_drink: bad liquid number %d.", liquid);
                        liquid = obj.value[2] = 0;
                    }

                    amount = Tables.liq_table[liquid].liq_affect[4];
                    amount = Bit.UMIN(amount, obj.value[1]);
                    break;
            }
            if (!Bit.IS_NPC(ch) && !Bit.IS_IMMORTAL(ch)
                && ch.pcdata.condition[COND_FULL] > 45)
            {
                Comm.send_to_char("You're too full to drink more.\n\r", ch);
                return;
            }

            Comm.act("$n drinks $T from $p.",
                ch, obj, Tables.liq_table[liquid].liq_name, TO_ROOM);
            Comm.act("You drink $T from $p.",
                ch, obj, Tables.liq_table[liquid].liq_name, TO_CHAR);

            Update.gain_condition(ch, COND_DRUNK,
                amount * Tables.liq_table[liquid].liq_affect[COND_DRUNK] / 36);
            Update.gain_condition(ch, COND_FULL,
                amount * Tables.liq_table[liquid].liq_affect[COND_FULL] / 4);
            Update.gain_condition(ch, COND_THIRST,
                amount * Tables.liq_table[liquid].liq_affect[COND_THIRST] / 10);
            Update.gain_condition(ch, COND_HUNGER,
                amount * Tables.liq_table[liquid].liq_affect[COND_HUNGER] / 2);

            if (!Bit.IS_NPC(ch) && ch.pcdata.condition[COND_DRUNK] > 10)
                Comm.send_to_char("You feel drunk.\n\r", ch);
            if (!Bit.IS_NPC(ch) && ch.pcdata.condition[COND_FULL] > 40)
                Comm.send_to_char("You are full.\n\r", ch);
            if (!Bit.IS_NPC(ch) && ch.pcdata.condition[COND_THIRST] > 40)
                Comm.send_to_char("Your thirst is quenched.\n\r", ch);

            if (obj.value[3] != 0)
            {
                Comm.act("$n chokes and gags.", ch, null, null, TO_ROOM);
                Comm.send_to_char("You choke and gag.\n\r", ch);
                var af = new AffectData();
                af.where = TO_AFFECTS;
                af.type = Gsn.poison;
                af.level = RomRandom.number_fuzzy(amount);
                af.duration = 3 * amount;
                af.location = APPLY_NONE;
                af.modifier = 0;
                af.bitvector = AFF_POISON;
                Handler.affect_join(ch, af);
            }

            if (obj.value[0] > 0)
                obj.value[1] -= amount;
        }

        public static void do_eat(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                Comm.send_to_char("Eat what?\n\r", ch);
                return;
            }

            var obj = Handler.get_obj_carry(ch, arg, ch);
            if (obj == null)
            {
                Comm.send_to_char("You do not have that item.\n\r", ch);
                return;
            }

            if (!Bit.IS_IMMORTAL(ch))
            {
                if (obj.item_type != ITEM_FOOD && obj.item_type != ITEM_PILL)
                {
                    Comm.send_to_char("That's not edible.\n\r", ch);
                    return;
                }

                if (!Bit.IS_NPC(ch) && ch.pcdata.condition[COND_FULL] > 40)
                {
                    Comm.send_to_char("You are too full to eat more.\n\r", ch);
                    return;
                }
            }

            Comm.act("$n eats $p.", ch, obj, null, TO_ROOM);
            Comm.act("You eat $p.", ch, obj, null, TO_CHAR);

            switch (obj.item_type)
            {
                case ITEM_FOOD:
                    if (!Bit.IS_NPC(ch))
                    {
                        int condition = ch.pcdata.condition[COND_HUNGER];
                        Update.gain_condition(ch, COND_FULL, obj.value[0]);
                        Update.gain_condition(ch, COND_HUNGER, obj.value[1]);
                        if (condition == 0 && ch.pcdata.condition[COND_HUNGER] > 0)
                            Comm.send_to_char("You are no longer hungry.\n\r", ch);
                        else if (ch.pcdata.condition[COND_FULL] > 40)
                            Comm.send_to_char("You are full.\n\r", ch);
                    }

                    if (obj.value[3] != 0)
                    {
                        Comm.act("$n chokes and gags.", ch, null, null, TO_ROOM);
                        Comm.send_to_char("You choke and gag.\n\r", ch);

                        var af = new AffectData();
                        af.where = TO_AFFECTS;
                        af.type = Gsn.poison;
                        af.level = RomRandom.number_fuzzy(obj.value[0]);
                        af.duration = 2 * obj.value[0];
                        af.location = APPLY_NONE;
                        af.modifier = 0;
                        af.bitvector = AFF_POISON;
                        Handler.affect_join(ch, af);
                    }
                    break;

                case ITEM_PILL:
                    Magic.obj_cast_spell(obj.value[1], obj.value[0], ch, ch, null);
                    Magic.obj_cast_spell(obj.value[2], obj.value[0], ch, ch, null);
                    Magic.obj_cast_spell(obj.value[3], obj.value[0], ch, ch, null);
                    break;
            }

            Handler.extract_obj(obj);
        }

        public static void do_wear(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Wear, wield, or hold what?\n\r", ch);
                return;
            }

            if (!RomString.str_cmp(arg, "all"))
            {
                ObjData obj_next;
                for (var obj = ch.carrying; obj != null; obj = obj_next)
                {
                    obj_next = obj.next_content;
                    if (obj.wear_loc == WEAR_NONE && Handler.can_see_obj(ch, obj))
                        wear_obj(ch, obj, false);
                }
                return;
            }
            else
            {
                var obj = Handler.get_obj_carry(ch, arg, ch);
                if (obj == null)
                {
                    Comm.send_to_char("You do not have that item.\n\r", ch);
                    return;
                }

                wear_obj(ch, obj, true);
            }
        }

        public static void do_remove(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Remove what?\n\r", ch);
                return;
            }

            var obj = Handler.get_obj_wear(ch, arg);
            if (obj == null)
            {
                Comm.send_to_char("You do not have that item.\n\r", ch);
                return;
            }

            remove_obj(ch, obj.wear_loc, true);
        }

        public static bool can_loot(CharData ch, ObjData obj)
        {
            if (Bit.IS_IMMORTAL(ch))
                return true;

            if (obj.owner == null || obj.owner.Length == 0)
                return true;

            CharData owner = null;
            for (var wch = Game.char_list; wch != null; wch = wch.next)
                if (!RomString.str_cmp(wch.name, obj.owner))
                    owner = wch;

            if (owner == null)
                return true;

            if (!RomString.str_cmp(ch.name, owner.name))
                return true;

            if (!Bit.IS_NPC(owner) && Bit.IS_SET(owner.act, PLR_CANLOOT))
                return true;

            if (Handler.is_same_group(ch, owner))
                return true;

            return false;
        }

        public static void get_obj(CharData ch, ObjData obj, ObjData container)
        {
            if (!Bit.CAN_WEAR(obj, ITEM_TAKE))
            {
                Comm.send_to_char("You can't take that.\n\r", ch);
                return;
            }

            if (ch.carry_number + Handler.get_obj_number(obj) > Handler.can_carry_n(ch))
            {
                Comm.act("$d: you can't carry that many items.",
                    ch, null, obj.name, TO_CHAR);
                return;
            }

            if ((obj.in_obj == null || obj.in_obj.carried_by != ch)
                && (Handler.get_carry_weight(ch) + Handler.get_obj_weight(obj) > Handler.can_carry_w(ch)))
            {
                Comm.act("$d: you can't carry that much weight.",
                    ch, null, obj.name, TO_CHAR);
                return;
            }

            if (!can_loot(ch, obj))
            {
                Comm.act("Corpse looting is not permitted.", ch, null, null, TO_CHAR);
                return;
            }

            if (obj.in_room != null)
            {
                for (var gch = obj.in_room.people; gch != null; gch = gch.next_in_room)
                    if (gch.on == obj)
                    {
                        Comm.act("$N appears to be using $p.", ch, obj, gch, TO_CHAR);
                        return;
                    }
            }

            if (container != null)
            {
                if (container.pIndexData.vnum == OBJ_VNUM_PIT
                    && Handler.get_trust(ch) < obj.level)
                {
                    Comm.send_to_char("You are not powerful enough to use it.\n\r", ch);
                    return;
                }

                if (container.pIndexData.vnum == OBJ_VNUM_PIT
                    && !Bit.CAN_WEAR(container, ITEM_TAKE)
                    && !Bit.IS_OBJ_STAT(obj, ITEM_HAD_TIMER))
                    obj.timer = 0;
                Comm.act("You get $p from $P.", ch, obj, container, TO_CHAR);
                Comm.act("$n gets $p from $P.", ch, obj, container, TO_ROOM);
                Bit.REMOVE_BIT(ref obj.extra_flags, ITEM_HAD_TIMER);
                Handler.obj_from_obj(obj);
            }
            else
            {
                Comm.act("You get $p.", ch, obj, container, TO_CHAR);
                Comm.act("$n gets $p.", ch, obj, container, TO_ROOM);
                Handler.obj_from_room(obj);
            }

            if (obj.item_type == ITEM_MONEY)
            {
                ch.silver += obj.value[0];
                ch.gold += obj.value[1];
                if (Bit.IS_SET(ch.act, PLR_AUTOSPLIT))
                {
                    int members = 0;
                    for (var gch = ch.in_room.people; gch != null;
                         gch = gch.next_in_room)
                    {
                        if (!Bit.IS_AFFECTED(gch, AFF_CHARM) && Handler.is_same_group(gch, ch))
                            members++;
                    }

                    if (members > 1 && (obj.value[0] > 1 || obj.value[1] != 0))
                    {
                        string buffer = $"{obj.value[0]} {obj.value[1]}";
                        Interp.do_function(ch, ActComm.do_split, buffer);
                    }
                }

                Handler.extract_obj(obj);
            }
            else
            {
                Handler.obj_to_char(obj, ch);
            }
        }

        public static void do_get(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg1);
            argument = RomString.one_argument(argument, out string arg2);

            if (!RomString.str_cmp(arg2, "from"))
                argument = RomString.one_argument(argument, out arg2);

            if (arg1.Length == 0)
            {
                Comm.send_to_char("Get what?\n\r", ch);
                return;
            }

            if (arg2.Length == 0)
            {
                if (RomString.str_cmp(arg1, "all") && RomString.str_prefix("all.", arg1))
                {
                    var obj = Handler.get_obj_list(ch, arg1, ch.in_room.contents);
                    if (obj == null)
                    {
                        Comm.act("I see no $T here.", ch, null, arg1, TO_CHAR);
                        return;
                    }

                    get_obj(ch, obj, null);
                }
                else
                {
                    bool found = false;
                    ObjData obj_next;
                    for (var obj = ch.in_room.contents; obj != null; obj = obj_next)
                    {
                        obj_next = obj.next_content;
                        if ((arg1.Length <= 3 || Handler.is_name(arg1.Substring(4), obj.name))
                            && Handler.can_see_obj(ch, obj))
                        {
                            found = true;
                            get_obj(ch, obj, null);
                        }
                    }

                    if (!found)
                    {
                        if (arg1.Length <= 3)
                            Comm.send_to_char("I see nothing here.\n\r", ch);
                        else
                            Comm.act("I see no $T here.", ch, null, arg1.Substring(4), TO_CHAR);
                    }
                }
            }
            else
            {
                if (!RomString.str_cmp(arg2, "all") || !RomString.str_prefix("all.", arg2))
                {
                    Comm.send_to_char("You can't do that.\n\r", ch);
                    return;
                }

                var container = Handler.get_obj_here(ch, arg2);
                if (container == null)
                {
                    Comm.act("I see no $T here.", ch, null, arg2, TO_CHAR);
                    return;
                }

                switch (container.item_type)
                {
                    default:
                        Comm.send_to_char("That's not a container.\n\r", ch);
                        return;

                    case ITEM_CONTAINER:
                    case ITEM_CORPSE_NPC:
                        break;

                    case ITEM_CORPSE_PC:
                        {
                            if (!can_loot(ch, container))
                            {
                                Comm.send_to_char("You can't do that.\n\r", ch);
                                return;
                            }
                        }
                        break;
                }

                if (Bit.IS_SET(container.value[1], CONT_CLOSED))
                {
                    Comm.act("The $d is closed.", ch, null, container.name, TO_CHAR);
                    return;
                }

                if (RomString.str_cmp(arg1, "all") && RomString.str_prefix("all.", arg1))
                {
                    var obj = Handler.get_obj_list(ch, arg1, container.contains);
                    if (obj == null)
                    {
                        Comm.act("I see nothing like that in the $T.",
                            ch, null, arg2, TO_CHAR);
                        return;
                    }
                    get_obj(ch, obj, container);
                }
                else
                {
                    bool found = false;
                    ObjData obj_next;
                    for (var obj = container.contains; obj != null; obj = obj_next)
                    {
                        obj_next = obj.next_content;
                        if ((arg1.Length <= 3 || Handler.is_name(arg1.Substring(4), obj.name))
                            && Handler.can_see_obj(ch, obj))
                        {
                            found = true;
                            if (container.pIndexData.vnum == OBJ_VNUM_PIT
                                && !Bit.IS_IMMORTAL(ch))
                            {
                                Comm.send_to_char("Don't be so greedy!\n\r", ch);
                                return;
                            }
                            get_obj(ch, obj, container);
                        }
                    }

                    if (!found)
                    {
                        if (arg1.Length <= 3)
                            Comm.act("I see nothing in the $T.", ch, null, arg2, TO_CHAR);
                        else
                            Comm.act("I see nothing like that in the $T.",
                                ch, null, arg2, TO_CHAR);
                    }
                }
            }
        }

        public static void do_sacrifice(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0 || !RomString.str_cmp(arg, ch.name))
            {
                Comm.act("$n offers $mself to Mota, who graciously declines.",
                    ch, null, null, TO_ROOM);
                Comm.send_to_char(
                    "Mota appreciates your offer and may accept it later.\n\r", ch);
                return;
            }

            var obj = Handler.get_obj_list(ch, arg, ch.in_room.contents);
            if (obj == null)
            {
                Comm.send_to_char("You can't find it.\n\r", ch);
                return;
            }

            if (obj.item_type == ITEM_CORPSE_PC)
            {
                if (obj.contains != null)
                {
                    Comm.send_to_char("Mota wouldn't like that.\n\r", ch);
                    return;
                }
            }

            if (!Bit.CAN_WEAR(obj, ITEM_TAKE) || Bit.CAN_WEAR(obj, ITEM_NO_SAC))
            {
                Comm.act("$p is not an acceptable sacrifice.", ch, obj, null, TO_CHAR);
                return;
            }

            if (obj.in_room != null)
            {
                for (var gch = obj.in_room.people; gch != null; gch = gch.next_in_room)
                    if (gch.on == obj)
                    {
                        Comm.act("$N appears to be using $p.", ch, obj, gch, TO_CHAR);
                        return;
                    }
            }

            int silver = Bit.UMAX(1, obj.level * 3);

            if (obj.item_type != ITEM_CORPSE_NPC && obj.item_type != ITEM_CORPSE_PC)
                silver = Bit.UMIN(silver, obj.cost);

            if (silver == 1)
                Comm.send_to_char(
                    "Mota gives you one silver coin for your sacrifice.\n\r", ch);
            else
            {
                Comm.send_to_char(
                    $"Mota gives you {silver} silver coins for your sacrifice.\n\r",
                    ch);
            }

            ch.silver += silver;

            if (Bit.IS_SET(ch.act, PLR_AUTOSPLIT))
            {
                int members = 0;
                for (var gch = ch.in_room.people; gch != null; gch = gch.next_in_room)
                {
                    if (Handler.is_same_group(gch, ch))
                        members++;
                }

                if (members > 1 && silver > 1)
                {
                    Interp.do_function(ch, ActComm.do_split, silver.ToString());
                }
            }

            Comm.act("$n sacrifices $p to Mota.", ch, obj, null, TO_ROOM);
            Comm.wiznet("$N sends up $p as a burnt offering.",
                ch, obj, WIZ_SACCING, 0, 0);
            Handler.extract_obj(obj);
        }

        public static void do_envenom(CharData ch, string argument)
        {
            if (argument.Length == 0)
            {
                Comm.send_to_char("Envenom what item?\n\r", ch);
                return;
            }

            var obj = Handler.get_obj_list(ch, argument, ch.carrying);

            if (obj == null)
            {
                Comm.send_to_char("You don't have that item.\n\r", ch);
                return;
            }

            int skill = Handler.get_skill(ch, Gsn.envenom);
            if (skill < 1)
            {
                Comm.send_to_char("Are you crazy? You'd poison yourself!\n\r", ch);
                return;
            }

            if (obj.item_type == ITEM_FOOD || obj.item_type == ITEM_DRINK_CON)
            {
                if (Bit.IS_OBJ_STAT(obj, ITEM_BLESS)
                    || Bit.IS_OBJ_STAT(obj, ITEM_BURN_PROOF))
                {
                    Comm.act("You fail to poison $p.", ch, obj, null, TO_CHAR);
                    return;
                }

                if (RomRandom.number_percent() < skill)
                {
                    Comm.act("$n treats $p with deadly poison.", ch, obj, null, TO_ROOM);
                    Comm.act("You treat $p with deadly poison.", ch, obj, null, TO_CHAR);
                    if (obj.value[3] == 0)
                    {
                        obj.value[3] = 1;
                        Skills.check_improve(ch, Gsn.envenom, true, 4);
                    }
                    Bit.WAIT_STATE(ch, Tables.skill_table[Gsn.envenom].beats);
                    return;
                }

                Comm.act("You fail to poison $p.", ch, obj, null, TO_CHAR);
                if (obj.value[3] == 0)
                    Skills.check_improve(ch, Gsn.envenom, false, 4);
                Bit.WAIT_STATE(ch, Tables.skill_table[Gsn.envenom].beats);
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

                if (obj.value[3] < 0
                    || Tables.attack_table[obj.value[3]].damage == DAM_BASH)
                {
                    Comm.send_to_char("You can only envenom edged weapons.\n\r", ch);
                    return;
                }

                if (Bit.IS_WEAPON_STAT(obj, WEAPON_POISON))
                {
                    Comm.act("$p is already envenomed.", ch, obj, null, TO_CHAR);
                    return;
                }

                int percent = RomRandom.number_percent();
                if (percent < skill)
                {
                    var af = new AffectData();
                    af.where = TO_WEAPON;
                    af.type = Gsn.poison;
                    af.level = ch.level * percent / 100;
                    af.duration = ch.level / 2 * percent / 100;
                    af.location = 0;
                    af.modifier = 0;
                    af.bitvector = WEAPON_POISON;
                    Handler.affect_to_obj(obj, af);

                    Comm.act("$n coats $p with deadly venom.", ch, obj, null, TO_ROOM);
                    Comm.act("You coat $p with venom.", ch, obj, null, TO_CHAR);
                    Skills.check_improve(ch, Gsn.envenom, true, 3);
                    Bit.WAIT_STATE(ch, Tables.skill_table[Gsn.envenom].beats);
                    return;
                }
                else
                {
                    Comm.act("You fail to envenom $p.", ch, obj, null, TO_CHAR);
                    Skills.check_improve(ch, Gsn.envenom, false, 3);
                    Bit.WAIT_STATE(ch, Tables.skill_table[Gsn.envenom].beats);
                    return;
                }
            }

            Comm.act("You can't poison $p.", ch, obj, null, TO_CHAR);
        }

        public static void do_quaff(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Quaff what?\n\r", ch);
                return;
            }

            var obj = Handler.get_obj_carry(ch, arg, ch);
            if (obj == null)
            {
                Comm.send_to_char("You do not have that potion.\n\r", ch);
                return;
            }

            if (obj.item_type != ITEM_POTION)
            {
                Comm.send_to_char("You can quaff only potions.\n\r", ch);
                return;
            }

            if (ch.level < obj.level)
            {
                Comm.send_to_char("This liquid is too powerful for you to drink.\n\r",
                    ch);
                return;
            }

            Comm.act("$n quaffs $p.", ch, obj, null, TO_ROOM);
            Comm.act("You quaff $p.", ch, obj, null, TO_CHAR);

            Magic.obj_cast_spell(obj.value[1], obj.value[0], ch, ch, null);
            Magic.obj_cast_spell(obj.value[2], obj.value[0], ch, ch, null);
            Magic.obj_cast_spell(obj.value[3], obj.value[0], ch, ch, null);

            Handler.extract_obj(obj);
        }

        public static void do_recite(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg1);
            argument = RomString.one_argument(argument, out string arg2);

            var scroll = Handler.get_obj_carry(ch, arg1, ch);
            if (scroll == null)
            {
                Comm.send_to_char("You do not have that scroll.\n\r", ch);
                return;
            }

            if (scroll.item_type != ITEM_SCROLL)
            {
                Comm.send_to_char("You can recite only scrolls.\n\r", ch);
                return;
            }

            if (ch.level < scroll.level)
            {
                Comm.send_to_char("This scroll is too complex for you to comprehend.\n\r",
                    ch);
                return;
            }

            ObjData obj = null;
            CharData victim;
            if (arg2.Length == 0)
            {
                victim = ch;
            }
            else
            {
                if ((victim = Handler.get_char_room(ch, arg2)) == null
                    && (obj = Handler.get_obj_here(ch, arg2)) == null)
                {
                    Comm.send_to_char("You can't find it.\n\r", ch);
                    return;
                }
            }

            Comm.act("$n recites $p.", ch, scroll, null, TO_ROOM);
            Comm.act("You recite $p.", ch, scroll, null, TO_CHAR);

            if (RomRandom.number_percent() >= 20 + Handler.get_skill(ch, Gsn.scrolls) * 4 / 5)
            {
                Comm.send_to_char("You mispronounce a syllable.\n\r", ch);
                Skills.check_improve(ch, Gsn.scrolls, false, 2);
            }
            else
            {
                Magic.obj_cast_spell(scroll.value[1], scroll.value[0], ch, victim, obj);
                Magic.obj_cast_spell(scroll.value[2], scroll.value[0], ch, victim, obj);
                Magic.obj_cast_spell(scroll.value[3], scroll.value[0], ch, victim, obj);
                Skills.check_improve(ch, Gsn.scrolls, true, 2);
            }

            Handler.extract_obj(scroll);
        }

        public static void do_brandish(CharData ch, string argument)
        {
            var staff = Handler.get_eq_char(ch, WEAR_HOLD);
            if (staff == null)
            {
                Comm.send_to_char("You hold nothing in your hand.\n\r", ch);
                return;
            }

            if (staff.item_type != ITEM_STAFF)
            {
                Comm.send_to_char("You can brandish only with a staff.\n\r", ch);
                return;
            }

            int sn = staff.value[3];
            if (sn < 0
                || sn >= MAX_SKILL || Tables.skill_table[sn].spell_fun == null)
            {
                Db.bug("Do_brandish: bad sn %d.", sn);
                return;
            }

            Bit.WAIT_STATE(ch, 2 * PULSE_VIOLENCE);

            if (staff.value[2] > 0)
            {
                Comm.act("$n brandishes $p.", ch, staff, null, TO_ROOM);
                Comm.act("You brandish $p.", ch, staff, null, TO_CHAR);
                if (ch.level < staff.level
                    || RomRandom.number_percent() >= 20 + Handler.get_skill(ch, Gsn.staves) * 4 / 5)
                {
                    Comm.act("You fail to invoke $p.", ch, staff, null, TO_CHAR);
                    Comm.act("...and nothing happens.", ch, null, null, TO_ROOM);
                    Skills.check_improve(ch, Gsn.staves, false, 2);
                }
                else
                {
                    CharData vch_next;
                    for (var vch = ch.in_room.people; vch != null; vch = vch_next)
                    {
                        vch_next = vch.next_in_room;

                        switch (Tables.skill_table[sn].target)
                        {
                            default:
                                Db.bug("Do_brandish: bad target for sn %d.", sn);
                                return;

                            case TAR_IGNORE:
                                if (vch != ch)
                                    continue;
                                break;

                            case TAR_CHAR_OFFENSIVE:
                                if (Bit.IS_NPC(ch) ? Bit.IS_NPC(vch) : !Bit.IS_NPC(vch))
                                    continue;
                                break;

                            case TAR_CHAR_DEFENSIVE:
                                if (Bit.IS_NPC(ch) ? !Bit.IS_NPC(vch) : Bit.IS_NPC(vch))
                                    continue;
                                break;

                            case TAR_CHAR_SELF:
                                if (vch != ch)
                                    continue;
                                break;
                        }

                        Magic.obj_cast_spell(staff.value[3], staff.value[0], ch, vch,
                            null);
                        Skills.check_improve(ch, Gsn.staves, true, 2);
                    }
                }
            }

            if (--staff.value[2] <= 0)
            {
                Comm.act("$n's $p blazes bright and is gone.", ch, staff, null, TO_ROOM);
                Comm.act("Your $p blazes bright and is gone.", ch, staff, null, TO_CHAR);
                Handler.extract_obj(staff);
            }
        }

        public static void do_zap(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);
            if (arg.Length == 0 && ch.fighting == null)
            {
                Comm.send_to_char("Zap whom or what?\n\r", ch);
                return;
            }

            var wand = Handler.get_eq_char(ch, WEAR_HOLD);
            if (wand == null)
            {
                Comm.send_to_char("You hold nothing in your hand.\n\r", ch);
                return;
            }

            if (wand.item_type != ITEM_WAND)
            {
                Comm.send_to_char("You can zap only with a wand.\n\r", ch);
                return;
            }

            CharData victim = null;
            ObjData obj = null;
            if (arg.Length == 0)
            {
                if (ch.fighting != null)
                {
                    victim = ch.fighting;
                }
                else
                {
                    Comm.send_to_char("Zap whom or what?\n\r", ch);
                    return;
                }
            }
            else
            {
                if ((victim = Handler.get_char_room(ch, arg)) == null
                    && (obj = Handler.get_obj_here(ch, arg)) == null)
                {
                    Comm.send_to_char("You can't find it.\n\r", ch);
                    return;
                }
            }

            Bit.WAIT_STATE(ch, 2 * PULSE_VIOLENCE);

            if (wand.value[2] > 0)
            {
                if (victim != null)
                {
                    Comm.act("$n zaps $N with $p.", ch, wand, victim, TO_NOTVICT);
                    Comm.act("You zap $N with $p.", ch, wand, victim, TO_CHAR);
                    Comm.act("$n zaps you with $p.", ch, wand, victim, TO_VICT);
                }
                else
                {
                    Comm.act("$n zaps $P with $p.", ch, wand, obj, TO_ROOM);
                    Comm.act("You zap $P with $p.", ch, wand, obj, TO_CHAR);
                }

                if (ch.level < wand.level
                    || RomRandom.number_percent() >= 20 + Handler.get_skill(ch, Gsn.wands) * 4 / 5)
                {
                    Comm.act("Your efforts with $p produce only smoke and sparks.",
                        ch, wand, null, TO_CHAR);
                    Comm.act("$n's efforts with $p produce only smoke and sparks.",
                        ch, wand, null, TO_ROOM);
                    Skills.check_improve(ch, Gsn.wands, false, 2);
                }
                else
                {
                    Magic.obj_cast_spell(wand.value[3], wand.value[0], ch, victim, obj);
                    Skills.check_improve(ch, Gsn.wands, true, 2);
                }
            }

            if (--wand.value[2] <= 0)
            {
                Comm.act("$n's $p explodes into fragments.", ch, wand, null, TO_ROOM);
                Comm.act("Your $p explodes into fragments.", ch, wand, null, TO_CHAR);
                Handler.extract_obj(wand);
            }
        }

        public static void do_steal(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg1);
            argument = RomString.one_argument(argument, out string arg2);

            if (arg1.Length == 0 || arg2.Length == 0)
            {
                Comm.send_to_char("Steal what from whom?\n\r", ch);
                return;
            }

            var victim = Handler.get_char_room(ch, arg2);
            if (victim == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }

            if (victim == ch)
            {
                Comm.send_to_char("That's pointless.\n\r", ch);
                return;
            }

            if (Fight.is_safe(ch, victim))
                return;

            if (Bit.IS_NPC(victim) && victim.position == POS_FIGHTING)
            {
                Comm.send_to_char("Kill stealing is not permitted.\n\r"
                    + "You'd better not -- you might get hit.\n\r", ch);
                return;
            }

            Bit.WAIT_STATE(ch, Tables.skill_table[Gsn.steal].beats);
            int percent = RomRandom.number_percent();

            if (!Bit.IS_AWAKE(victim))
                percent -= 10;
            else if (!Handler.can_see(victim, ch))
                percent += 25;
            else
                percent += 50;

            if (((ch.level + 7 < victim.level || ch.level - 7 > victim.level)
                 && !Bit.IS_NPC(victim) && !Bit.IS_NPC(ch))
                || (!Bit.IS_NPC(ch) && percent > Handler.get_skill(ch, Gsn.steal))
                || (!Bit.IS_NPC(ch) && !Handler.is_clan(ch)))
            {
                Comm.send_to_char("Oops.\n\r", ch);
                Handler.affect_strip(ch, Gsn.sneak);
                Bit.REMOVE_BIT(ref ch.affected_by, AFF_SNEAK);

                Comm.act("$n tried to steal from you.\n\r", ch, null, victim, TO_VICT);
                Comm.act("$n tried to steal from $N.\n\r", ch, null, victim, TO_NOTVICT);
                string buf;
                switch (RomRandom.number_range(0, 3))
                {
                    case 0:
                        buf = RomString.sprintf("%s is a lousy thief!", ch.name);
                        break;
                    case 1:
                        buf = RomString.sprintf("%s couldn't rob %s way out of a paper bag!",
                            ch.name, (ch.sex == 2) ? "her" : "his");
                        break;
                    case 2:
                        buf = RomString.sprintf("%s tried to rob me!", ch.name);
                        break;
                    default:
                        buf = RomString.sprintf("Keep your hands out of there, %s!", ch.name);
                        break;
                }
                if (!Bit.IS_AWAKE(victim))
                    Interp.do_function(victim, ActMove.do_wake, "");
                if (Bit.IS_AWAKE(victim))
                    Interp.do_function(victim, ActComm.do_yell, buf);
                if (!Bit.IS_NPC(ch))
                {
                    if (Bit.IS_NPC(victim))
                    {
                        Skills.check_improve(ch, Gsn.steal, false, 2);
                        Fight.multi_hit(victim, ch, TYPE_UNDEFINED);
                    }
                    else
                    {
                        buf = RomString.sprintf("$N tried to steal from %s.", victim.name);
                        Comm.wiznet(buf, ch, null, WIZ_FLAGS, 0, 0);
                        if (!Bit.IS_SET(ch.act, PLR_THIEF))
                        {
                            Bit.SET_BIT(ref ch.act, PLR_THIEF);
                            Comm.send_to_char("*** You are now a THIEF!! ***\n\r", ch);
                            Save.save_char_obj(ch);
                        }
                    }
                }

                return;
            }

            if (!RomString.str_cmp(arg1, "coin")
                || !RomString.str_cmp(arg1, "coins")
                || !RomString.str_cmp(arg1, "gold") || !RomString.str_cmp(arg1, "silver"))
            {
                int gold = (int)(victim.gold * RomRandom.number_range(1, ch.level) / MAX_LEVEL);
                int silver = (int)(victim.silver * RomRandom.number_range(1, ch.level) / MAX_LEVEL);
                if (gold <= 0 && silver <= 0)
                {
                    Comm.send_to_char("You couldn't get any coins.\n\r", ch);
                    return;
                }

                ch.gold += gold;
                ch.silver += silver;
                victim.silver -= silver;
                victim.gold -= gold;
                string buf;
                if (silver <= 0)
                    buf = RomString.sprintf("Bingo!  You got %d gold coins.\n\r", gold);
                else if (gold <= 0)
                    buf = RomString.sprintf("Bingo!  You got %d silver coins.\n\r", silver);
                else
                    buf = RomString.sprintf("Bingo!  You got %d silver and %d gold coins.\n\r",
                        silver, gold);

                Comm.send_to_char(buf, ch);
                Skills.check_improve(ch, Gsn.steal, true, 2);
                return;
            }

            var obj = Handler.get_obj_carry(victim, arg1, ch);
            if (obj == null)
            {
                Comm.send_to_char("You can't find it.\n\r", ch);
                return;
            }

            if (!Handler.can_drop_obj(ch, obj)
                || Bit.IS_SET(obj.extra_flags, ITEM_INVENTORY)
                || obj.level > ch.level)
            {
                Comm.send_to_char("You can't pry it away.\n\r", ch);
                return;
            }

            if (ch.carry_number + Handler.get_obj_number(obj) > Handler.can_carry_n(ch))
            {
                Comm.send_to_char("You have your hands full.\n\r", ch);
                return;
            }

            if (ch.carry_weight + Handler.get_obj_weight(obj) > Handler.can_carry_w(ch))
            {
                Comm.send_to_char("You can't carry that much weight.\n\r", ch);
                return;
            }

            Handler.obj_from_char(obj);
            Handler.obj_to_char(obj, ch);
            Comm.act("You pocket $p.", ch, obj, null, TO_CHAR);
            Skills.check_improve(ch, Gsn.steal, true, 2);
            Comm.send_to_char("Got it!\n\r", ch);
        }

        public static CharData find_keeper(CharData ch)
        {
            ShopData pShop = null;
            CharData keeper;
            for (keeper = ch.in_room.people; keeper != null; keeper = keeper.next_in_room)
            {
                if (Bit.IS_NPC(keeper) && (pShop = keeper.pIndexData.pShop) != null)
                    break;
            }

            if (pShop == null)
            {
                Comm.send_to_char("You can't do that here.\n\r", ch);
                return null;
            }

            if (Game.time_info.hour < pShop.open_hour)
            {
                Interp.do_function(keeper, ActComm.do_say, "Sorry, I am closed. Come back later.");
                return null;
            }

            if (Game.time_info.hour > pShop.close_hour)
            {
                Interp.do_function(keeper, ActComm.do_say,
                    "Sorry, I am closed. Come back tomorrow.");
                return null;
            }

            if (!Handler.can_see(keeper, ch))
            {
                Interp.do_function(keeper, ActComm.do_say,
                    "I don't trade with folks I can't see.");
                return null;
            }

            return keeper;
        }

        public static void obj_to_keeper(ObjData obj, CharData ch)
        {
            ObjData t_obj, t_obj_next;

            t_obj = null;
            for (t_obj = ch.carrying; t_obj != null; t_obj = t_obj_next)
            {
                t_obj_next = t_obj.next_content;

                if (obj.pIndexData == t_obj.pIndexData
                    && !RomString.str_cmp(obj.short_descr, t_obj.short_descr))
                {
                    if (Bit.IS_OBJ_STAT(t_obj, ITEM_INVENTORY))
                    {
                        Handler.extract_obj(obj);
                        return;
                    }
                    obj.cost = t_obj.cost;
                    break;
                }
            }

            if (t_obj == null)
            {
                obj.next_content = ch.carrying;
                ch.carrying = obj;
            }
            else
            {
                obj.next_content = t_obj.next_content;
                t_obj.next_content = obj;
            }

            obj.carried_by = ch;
            obj.in_room = null;
            obj.in_obj = null;
            ch.carry_number += Handler.get_obj_number(obj);
            ch.carry_weight += Handler.get_obj_weight(obj);
        }

        public static ObjData get_obj_keeper(CharData ch, CharData keeper, string argument)
        {
            int number = RomString.number_argument(argument, out string arg);
            int count = 0;
            for (var obj = keeper.carrying; obj != null; obj = obj.next_content)
            {
                if (obj.wear_loc == WEAR_NONE && Handler.can_see_obj(keeper, obj)
                    && Handler.can_see_obj(ch, obj) && Handler.is_name(arg, obj.name))
                {
                    if (++count == number)
                        return obj;

                    while (obj.next_content != null
                           && obj.pIndexData == obj.next_content.pIndexData
                           && !RomString.str_cmp(obj.short_descr,
                               obj.next_content.short_descr))
                        obj = obj.next_content;
                }
            }

            return null;
        }

        public static int get_cost(CharData keeper, ObjData obj, bool fBuy)
        {
            if (obj == null || keeper.pIndexData.pShop == null)
                return 0;

            var pShop = keeper.pIndexData.pShop;
            int cost;

            if (fBuy)
            {
                cost = obj.cost * pShop.profit_buy / 100;
            }
            else
            {
                cost = 0;
                for (int itype = 0; itype < MAX_TRADE; itype++)
                {
                    if (obj.item_type == pShop.buy_type[itype])
                    {
                        cost = obj.cost * pShop.profit_sell / 100;
                        break;
                    }
                }

                if (!Bit.IS_OBJ_STAT(obj, ITEM_SELL_EXTRACT))
                    for (var obj2 = keeper.carrying; obj2 != null; obj2 = obj2.next_content)
                    {
                        if (obj.pIndexData == obj2.pIndexData
                            && !RomString.str_cmp(obj.short_descr, obj2.short_descr))
                        {
                            if (Bit.IS_OBJ_STAT(obj2, ITEM_INVENTORY))
                                cost /= 2;
                            else
                                cost = cost * 3 / 4;
                        }
                    }
            }

            if (obj.item_type == ITEM_STAFF || obj.item_type == ITEM_WAND)
            {
                if (obj.value[1] == 0)
                    cost /= 4;
                else
                    cost = cost * obj.value[2] / obj.value[1];
            }

            return cost;
        }

        public static void do_buy(CharData ch, string argument)
        {
            int cost, roll;

            if (argument.Length == 0)
            {
                Comm.send_to_char("Buy what?\n\r", ch);
                return;
            }

            if (Bit.IS_SET(ch.in_room.room_flags, ROOM_PET_SHOP))
            {
                argument = RomString.smash_tilde(argument);

                if (Bit.IS_NPC(ch))
                    return;

                argument = RomString.one_argument(argument, out string arg);

                RoomIndexData pRoomIndexNext;
                if (ch.in_room.vnum == 9621)
                    pRoomIndexNext = Handler.get_room_index(9706);
                else
                    pRoomIndexNext = Handler.get_room_index(ch.in_room.vnum + 1);
                if (pRoomIndexNext == null)
                {
                    Db.bug("Do_buy: bad pet shop at vnum %d.", ch.in_room.vnum);
                    Comm.send_to_char("Sorry, you can't buy that here.\n\r", ch);
                    return;
                }

                var in_room = ch.in_room;
                ch.in_room = pRoomIndexNext;
                var pet = Handler.get_char_room(ch, arg);
                ch.in_room = in_room;

                if (pet == null || !Bit.IS_SET(pet.act, ACT_PET))
                {
                    Comm.send_to_char("Sorry, you can't buy that here.\n\r", ch);
                    return;
                }

                if (ch.pet != null)
                {
                    Comm.send_to_char("You already own a pet.\n\r", ch);
                    return;
                }

                cost = 10 * pet.level * pet.level;

                if ((ch.silver + 100 * ch.gold) < cost)
                {
                    Comm.send_to_char("You can't afford it.\n\r", ch);
                    return;
                }

                if (ch.level < pet.level)
                {
                    Comm.send_to_char(
                        "You're not powerful enough to master this pet.\n\r", ch);
                    return;
                }

                roll = RomRandom.number_percent();
                if (roll < Handler.get_skill(ch, Gsn.haggle))
                {
                    cost -= cost / 2 * roll / 100;
                    Comm.send_to_char(RomString.sprintf("You haggle the price down to %d coins.\n\r", cost), ch);
                    Skills.check_improve(ch, Gsn.haggle, true, 4);
                }

                Handler.deduct_cost(ch, cost);
                pet = Db.create_mobile(pet.pIndexData);
                Bit.SET_BIT(ref pet.act, ACT_PET);
                Bit.SET_BIT(ref pet.affected_by, AFF_CHARM);
                pet.comm = COMM_NOTELL | COMM_NOSHOUT | COMM_NOCHANNELS;

                argument = RomString.one_argument(argument, out arg);
                if (arg.Length != 0)
                {
                    pet.name = RomString.sprintf("%s %s", pet.name, arg);
                }

                pet.description = RomString.sprintf("%sA neck tag says 'I belong to %s'.\n\r",
                    pet.description, ch.name);

                Handler.char_to_room(pet, ch.in_room);
                ActComm.add_follower(pet, ch);
                pet.leader = ch;
                ch.pet = pet;
                Comm.send_to_char("Enjoy your pet.\n\r", ch);
                Comm.act("$n bought $N as a pet.", ch, null, pet, TO_ROOM);
                return;
            }
            else
            {
                var keeper = find_keeper(ch);
                if (keeper == null)
                    return;

                int number = RomString.mult_argument(argument, out string arg);
                var obj = get_obj_keeper(ch, keeper, arg);
                cost = get_cost(keeper, obj, true);

                if (number < 1 || number > 99)
                {
                    Comm.act("$n tells you 'Get real!", keeper, null, ch, TO_VICT);
                    return;
                }

                if (cost <= 0 || !Handler.can_see_obj(ch, obj))
                {
                    Comm.act("$n tells you 'I don't sell that -- try 'list''.",
                        keeper, null, ch, TO_VICT);
                    ch.reply = keeper;
                    return;
                }

                if (!Bit.IS_OBJ_STAT(obj, ITEM_INVENTORY))
                {
                    int count = 1;
                    for (var t_obj = obj.next_content;
                         count < number && t_obj != null; t_obj = t_obj.next_content)
                    {
                        if (t_obj.pIndexData == obj.pIndexData
                            && !RomString.str_cmp(t_obj.short_descr, obj.short_descr))
                            count++;
                        else
                            break;
                    }

                    if (count < number)
                    {
                        Comm.act("$n tells you 'I don't have that many in stock.",
                            keeper, null, ch, TO_VICT);
                        ch.reply = keeper;
                        return;
                    }
                }

                if ((ch.silver + ch.gold * 100) < cost * number)
                {
                    if (number > 1)
                        Comm.act("$n tells you 'You can't afford to buy that many.",
                            keeper, obj, ch, TO_VICT);
                    else
                        Comm.act("$n tells you 'You can't afford to buy $p'.",
                            keeper, obj, ch, TO_VICT);
                    ch.reply = keeper;
                    return;
                }

                if (obj.level > ch.level)
                {
                    Comm.act("$n tells you 'You can't use $p yet'.",
                        keeper, obj, ch, TO_VICT);
                    ch.reply = keeper;
                    return;
                }

                if (ch.carry_number + number * Handler.get_obj_number(obj) >
                    Handler.can_carry_n(ch))
                {
                    Comm.send_to_char("You can't carry that many items.\n\r", ch);
                    return;
                }

                if (ch.carry_weight + number * Handler.get_obj_weight(obj) >
                    Handler.can_carry_w(ch))
                {
                    Comm.send_to_char("You can't carry that much weight.\n\r", ch);
                    return;
                }

                roll = RomRandom.number_percent();
                if (!Bit.IS_OBJ_STAT(obj, ITEM_SELL_EXTRACT)
                    && roll < Handler.get_skill(ch, Gsn.haggle))
                {
                    cost -= obj.cost / 2 * roll / 100;
                    Comm.act("You haggle with $N.", ch, null, keeper, TO_CHAR);
                    Skills.check_improve(ch, Gsn.haggle, true, 4);
                }

                if (number > 1)
                {
                    string buf = RomString.sprintf("$n buys $p[%d].", number);
                    Comm.act(buf, ch, obj, null, TO_ROOM);
                    buf = RomString.sprintf("You buy $p[%d] for %d silver.", number,
                        cost * number);
                    Comm.act(buf, ch, obj, null, TO_CHAR);
                }
                else
                {
                    Comm.act("$n buys $p.", ch, obj, null, TO_ROOM);
                    string buf = RomString.sprintf("You buy $p for %d silver.", cost);
                    Comm.act(buf, ch, obj, null, TO_CHAR);
                }
                Handler.deduct_cost(ch, cost * number);
                keeper.gold += cost * number / 100;
                keeper.silver += cost * number - (cost * number / 100) * 100;

                for (int count = 0; count < number; count++)
                {
                    ObjData t_obj;
                    if (Bit.IS_SET(obj.extra_flags, ITEM_INVENTORY))
                        t_obj = Db.create_object(obj.pIndexData, obj.level);
                    else
                    {
                        t_obj = obj;
                        obj = obj.next_content;
                        Handler.obj_from_char(t_obj);
                    }

                    if (t_obj.timer > 0 && !Bit.IS_OBJ_STAT(t_obj, ITEM_HAD_TIMER))
                        t_obj.timer = 0;
                    Bit.REMOVE_BIT(ref t_obj.extra_flags, ITEM_HAD_TIMER);
                    Handler.obj_to_char(t_obj, ch);
                    if (cost < t_obj.cost)
                        t_obj.cost = cost;
                }
            }
        }

        public static void do_list(CharData ch, string argument)
        {
            if (Bit.IS_SET(ch.in_room.room_flags, ROOM_PET_SHOP))
            {
                RoomIndexData pRoomIndexNext;
                if (ch.in_room.vnum == 9621)
                    pRoomIndexNext = Handler.get_room_index(9706);
                else
                    pRoomIndexNext = Handler.get_room_index(ch.in_room.vnum + 1);

                if (pRoomIndexNext == null)
                {
                    Db.bug("Do_list: bad pet shop at vnum %d.", ch.in_room.vnum);
                    Comm.send_to_char("You can't do that here.\n\r", ch);
                    return;
                }

                bool found = false;
                for (var pet = pRoomIndexNext.people; pet != null; pet = pet.next_in_room)
                {
                    if (Bit.IS_SET(pet.act, ACT_PET))
                    {
                        if (!found)
                        {
                            found = true;
                            Comm.send_to_char("Pets for sale:\n\r", ch);
                        }
                        Comm.send_to_char(RomString.sprintf("[%2d] %8d - %s\n\r",
                            pet.level,
                            10 * pet.level * pet.level, pet.short_descr), ch);
                    }
                }
                if (!found)
                    Comm.send_to_char("Sorry, we're out of pets right now.\n\r", ch);
                return;
            }
            else
            {
                var keeper = find_keeper(ch);
                if (keeper == null)
                    return;
                RomString.one_argument(argument, out string arg);

                bool found = false;
                for (var obj = keeper.carrying; obj != null; obj = obj.next_content)
                {
                    int cost;
                    if (obj.wear_loc == WEAR_NONE && Handler.can_see_obj(ch, obj)
                        && (cost = get_cost(keeper, obj, true)) > 0
                        && (arg.Length == 0 || Handler.is_name(arg, obj.name)))
                    {
                        if (!found)
                        {
                            found = true;
                            Comm.send_to_char("[Lv Price Qty] Item\n\r", ch);
                        }

                        string buf;
                        if (Bit.IS_OBJ_STAT(obj, ITEM_INVENTORY))
                            buf = RomString.sprintf("[%2d %5d -- ] %s\n\r",
                                obj.level, cost, obj.short_descr);
                        else
                        {
                            int count = 1;

                            while (obj.next_content != null
                                   && obj.pIndexData == obj.next_content.pIndexData
                                   && !RomString.str_cmp(obj.short_descr,
                                       obj.next_content.short_descr))
                            {
                                obj = obj.next_content;
                                count++;
                            }
                            buf = RomString.sprintf("[%2d %5d %2d ] %s\n\r",
                                obj.level, cost, count, obj.short_descr);
                        }
                        Comm.send_to_char(buf, ch);
                    }
                }

                if (!found)
                    Comm.send_to_char("You can't buy anything here.\n\r", ch);
            }
        }

        public static void do_sell(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Sell what?\n\r", ch);
                return;
            }

            var keeper = find_keeper(ch);
            if (keeper == null)
                return;

            var obj = Handler.get_obj_carry(ch, arg, ch);
            if (obj == null)
            {
                Comm.act("$n tells you 'You don't have that item'.",
                    keeper, null, ch, TO_VICT);
                ch.reply = keeper;
                return;
            }

            if (!Handler.can_drop_obj(ch, obj))
            {
                Comm.send_to_char("You can't let go of it.\n\r", ch);
                return;
            }

            if (!Handler.can_see_obj(keeper, obj))
            {
                Comm.act("$n doesn't see what you are offering.", keeper, null, ch,
                    TO_VICT);
                return;
            }

            int cost = get_cost(keeper, obj, false);
            if (cost <= 0)
            {
                Comm.act("$n looks uninterested in $p.", keeper, obj, ch, TO_VICT);
                return;
            }
            if (cost > (keeper.silver + 100 * keeper.gold))
            {
                Comm.act("$n tells you 'I'm afraid I don't have enough wealth to buy $p.",
                    keeper, obj, ch, TO_VICT);
                return;
            }

            Comm.act("$n sells $p.", ch, obj, null, TO_ROOM);
            int roll = RomRandom.number_percent();
            if (!Bit.IS_OBJ_STAT(obj, ITEM_SELL_EXTRACT)
                && roll < Handler.get_skill(ch, Gsn.haggle))
            {
                Comm.send_to_char("You haggle with the shopkeeper.\n\r", ch);
                cost += obj.cost / 2 * roll / 100;
                cost = Bit.UMIN(cost, 95 * get_cost(keeper, obj, true) / 100);
                cost = Bit.UMIN(cost, (int)(keeper.silver + 100 * keeper.gold));
                Skills.check_improve(ch, Gsn.haggle, true, 4);
            }
            string buf = RomString.sprintf("You sell $p for %d silver and %d gold piece%s.",
                cost - (cost / 100) * 100, cost / 100, cost == 1 ? "" : "s");
            Comm.act(buf, ch, obj, null, TO_CHAR);
            ch.gold += cost / 100;
            ch.silver += cost - (cost / 100) * 100;
            Handler.deduct_cost(keeper, cost);
            if (keeper.gold < 0)
                keeper.gold = 0;
            if (keeper.silver < 0)
                keeper.silver = 0;

            if (obj.item_type == ITEM_TRASH || Bit.IS_OBJ_STAT(obj, ITEM_SELL_EXTRACT))
            {
                Handler.extract_obj(obj);
            }
            else
            {
                Handler.obj_from_char(obj);
                if (obj.timer != 0)
                    Bit.SET_BIT(ref obj.extra_flags, ITEM_HAD_TIMER);
                else
                    obj.timer = RomRandom.number_range(50, 100);
                obj_to_keeper(obj, keeper);
            }
        }

        public static void do_value(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Value what?\n\r", ch);
                return;
            }

            var keeper = find_keeper(ch);
            if (keeper == null)
                return;

            var obj = Handler.get_obj_carry(ch, arg, ch);
            if (obj == null)
            {
                Comm.act("$n tells you 'You don't have that item'.",
                    keeper, null, ch, TO_VICT);
                ch.reply = keeper;
                return;
            }

            if (!Handler.can_see_obj(keeper, obj))
            {
                Comm.act("$n doesn't see what you are offering.", keeper, null, ch,
                    TO_VICT);
                return;
            }

            if (!Handler.can_drop_obj(ch, obj))
            {
                Comm.send_to_char("You can't let go of it.\n\r", ch);
                return;
            }

            int cost = get_cost(keeper, obj, false);
            if (cost <= 0)
            {
                Comm.act("$n looks uninterested in $p.", keeper, obj, ch, TO_VICT);
                return;
            }

            string buf = RomString.sprintf(
                "$n tells you 'I'll give you %d silver and %d gold coins for $p'.",
                cost - (cost / 100) * 100, cost / 100);
            Comm.act(buf, keeper, obj, ch, TO_VICT);
            ch.reply = keeper;
        }
    }
}
