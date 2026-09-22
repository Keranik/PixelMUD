using static Rom24.Merc;

namespace Rom24
{
    public static partial class ActInfo
    {
        static readonly string[] where_name =
        {
            "<used as light>     ",
            "<worn on finger>    ",
            "<worn on finger>    ",
            "<worn around neck>  ",
            "<worn around neck>  ",
            "<worn on torso>     ",
            "<worn on head>      ",
            "<worn on legs>      ",
            "<worn on feet>      ",
            "<worn on hands>     ",
            "<worn on arms>      ",
            "<worn as shield>    ",
            "<worn about body>   ",
            "<worn about waist>  ",
            "<worn around wrist> ",
            "<worn around wrist> ",
            "<wielded>           ",
            "<held>              ",
            "<floating nearby>   ",
        };

        public static string format_obj_to_char(ObjData obj, CharData ch, bool fShort)
        {
            string buf = "";

            if ((fShort && (obj.short_descr == null || obj.short_descr.Length == 0))
                || (obj.description == null || obj.description.Length == 0))
                return buf;

            if (Bit.IS_OBJ_STAT(obj, ITEM_INVIS))
                buf += "(Invis) ";
            if (Bit.IS_AFFECTED(ch, AFF_DETECT_EVIL) && Bit.IS_OBJ_STAT(obj, ITEM_EVIL))
                buf += "(Red Aura) ";
            if (Bit.IS_AFFECTED(ch, AFF_DETECT_GOOD) && Bit.IS_OBJ_STAT(obj, ITEM_BLESS))
                buf += "(Blue Aura) ";
            if (Bit.IS_AFFECTED(ch, AFF_DETECT_MAGIC) && Bit.IS_OBJ_STAT(obj, ITEM_MAGIC))
                buf += "(Magical) ";
            if (Bit.IS_OBJ_STAT(obj, ITEM_GLOW))
                buf += "(Glowing) ";
            if (Bit.IS_OBJ_STAT(obj, ITEM_HUM))
                buf += "(Humming) ";

            if (fShort)
            {
                if (obj.short_descr != null)
                    buf += obj.short_descr;
            }
            else
            {
                if (obj.description != null)
                    buf += obj.description;
            }

            return buf;
        }

        public static void show_list_to_char(ObjData list, CharData ch, bool fShort,
            bool fShowNothing)
        {
            if (ch.desc == null)
                return;

            var prgpstrShow = new System.Collections.Generic.List<string>();
            var prgnShow = new System.Collections.Generic.List<int>();

            for (var obj = list; obj != null; obj = obj.next_content)
            {
                if (obj.wear_loc == WEAR_NONE && Handler.can_see_obj(ch, obj))
                {
                    string pstrShow = format_obj_to_char(obj, ch, fShort);
                    bool fCombine = false;

                    if (Bit.IS_NPC(ch) || Bit.IS_SET(ch.comm, COMM_COMBINE))
                    {
                        for (int iShow = prgpstrShow.Count - 1; iShow >= 0; iShow--)
                        {
                            if (prgpstrShow[iShow] == pstrShow)
                            {
                                prgnShow[iShow]++;
                                fCombine = true;
                                break;
                            }
                        }
                    }

                    if (!fCombine)
                    {
                        prgpstrShow.Add(pstrShow);
                        prgnShow.Add(1);
                    }
                }
            }

            var output = new System.Text.StringBuilder();
            for (int iShow = 0; iShow < prgpstrShow.Count; iShow++)
            {
                if (prgpstrShow[iShow].Length == 0)
                    continue;

                if (Bit.IS_NPC(ch) || Bit.IS_SET(ch.comm, COMM_COMBINE))
                {
                    if (prgnShow[iShow] != 1)
                        output.Append(RomString.sprintf("(%2d) ", prgnShow[iShow]));
                    else
                        output.Append("     ");
                }
                output.Append(prgpstrShow[iShow]);
                output.Append("\n\r");
            }

            if (fShowNothing && prgpstrShow.Count == 0)
            {
                if (Bit.IS_NPC(ch) || Bit.IS_SET(ch.comm, COMM_COMBINE))
                    Comm.send_to_char("     ", ch);
                Comm.send_to_char("Nothing.\n\r", ch);
            }
            Comm.page_to_char(output.ToString(), ch);
        }

        static void show_char_to_char_0(CharData victim, CharData ch)
        {
            string buf = "";

            if (Bit.IS_SET(victim.comm, COMM_AFK))
                buf += "[AFK] ";
            if (Bit.IS_AFFECTED(victim, AFF_INVISIBLE))
                buf += "(Invis) ";
            if (victim.invis_level >= LEVEL_HERO)
                buf += "(Wizi) ";
            if (Bit.IS_AFFECTED(victim, AFF_HIDE))
                buf += "(Hide) ";
            if (Bit.IS_AFFECTED(victim, AFF_CHARM))
                buf += "(Charmed) ";
            if (Bit.IS_AFFECTED(victim, AFF_PASS_DOOR))
                buf += "(Translucent) ";
            if (Bit.IS_AFFECTED(victim, AFF_FAERIE_FIRE))
                buf += "(Pink Aura) ";
            if (Bit.IS_EVIL(victim) && Bit.IS_AFFECTED(ch, AFF_DETECT_EVIL))
                buf += "(Red Aura) ";
            if (Bit.IS_GOOD(victim) && Bit.IS_AFFECTED(ch, AFF_DETECT_GOOD))
                buf += "(Golden Aura) ";
            if (Bit.IS_AFFECTED(victim, AFF_SANCTUARY))
                buf += "(White Aura) ";
            if (!Bit.IS_NPC(victim) && Bit.IS_SET(victim.act, PLR_KILLER))
                buf += "(KILLER) ";
            if (!Bit.IS_NPC(victim) && Bit.IS_SET(victim.act, PLR_THIEF))
                buf += "(THIEF) ";
            if (victim.position == victim.start_pos
                && victim.long_descr.Length != 0)
            {
                buf += victim.long_descr;
                Comm.send_to_char(buf, ch);
                return;
            }

            buf += Handler.PERS(victim, ch);
            if (!Bit.IS_NPC(victim) && !Bit.IS_SET(ch.comm, COMM_BRIEF)
                && victim.position == POS_STANDING && ch.on == null)
                buf += victim.pcdata.title;

            switch (victim.position)
            {
                case POS_DEAD:
                    buf += " is DEAD!!";
                    break;
                case POS_MORTAL:
                    buf += " is mortally wounded.";
                    break;
                case POS_INCAP:
                    buf += " is incapacitated.";
                    break;
                case POS_STUNNED:
                    buf += " is lying here stunned.";
                    break;
                case POS_SLEEPING:
                    if (victim.on != null)
                    {
                        if (Bit.IS_SET(victim.on.value[2], SLEEP_AT))
                            buf += RomString.sprintf(" is sleeping at %s.", victim.on.short_descr);
                        else if (Bit.IS_SET(victim.on.value[2], SLEEP_ON))
                            buf += RomString.sprintf(" is sleeping on %s.", victim.on.short_descr);
                        else
                            buf += RomString.sprintf(" is sleeping in %s.", victim.on.short_descr);
                    }
                    else
                        buf += " is sleeping here.";
                    break;
                case POS_RESTING:
                    if (victim.on != null)
                    {
                        if (Bit.IS_SET(victim.on.value[2], REST_AT))
                            buf += RomString.sprintf(" is resting at %s.", victim.on.short_descr);
                        else if (Bit.IS_SET(victim.on.value[2], REST_ON))
                            buf += RomString.sprintf(" is resting on %s.", victim.on.short_descr);
                        else
                            buf += RomString.sprintf(" is resting in %s.", victim.on.short_descr);
                    }
                    else
                        buf += " is resting here.";
                    break;
                case POS_SITTING:
                    if (victim.on != null)
                    {
                        if (Bit.IS_SET(victim.on.value[2], SIT_AT))
                            buf += RomString.sprintf(" is sitting at %s.", victim.on.short_descr);
                        else if (Bit.IS_SET(victim.on.value[2], SIT_ON))
                            buf += RomString.sprintf(" is sitting on %s.", victim.on.short_descr);
                        else
                            buf += RomString.sprintf(" is sitting in %s.", victim.on.short_descr);
                    }
                    else
                        buf += " is sitting here.";
                    break;
                case POS_STANDING:
                    if (victim.on != null)
                    {
                        if (Bit.IS_SET(victim.on.value[2], STAND_AT))
                            buf += RomString.sprintf(" is standing at %s.", victim.on.short_descr);
                        else if (Bit.IS_SET(victim.on.value[2], STAND_ON))
                            buf += RomString.sprintf(" is standing on %s.", victim.on.short_descr);
                        else
                            buf += RomString.sprintf(" is standing in %s.", victim.on.short_descr);
                    }
                    else
                        buf += " is here.";
                    break;
                case POS_FIGHTING:
                    buf += " is here, fighting ";
                    if (victim.fighting == null)
                        buf += "thin air??";
                    else if (victim.fighting == ch)
                        buf += "YOU!";
                    else if (victim.in_room == victim.fighting.in_room)
                    {
                        buf += Handler.PERS(victim.fighting, ch);
                        buf += ".";
                    }
                    else
                        buf += "someone who left??";
                    break;
            }

            buf += "\n\r";
            if (buf.Length > 0)
            {
                var chars = buf.ToCharArray();
                chars[0] = Bit.UPPER(chars[0]);
                buf = new string(chars);
            }
            Comm.send_to_char(buf, ch);
        }

        static void show_char_to_char_1(CharData victim, CharData ch)
        {
            if (Handler.can_see(victim, ch))
            {
                if (ch == victim)
                    Comm.act("$n looks at $mself.", ch, null, null, TO_ROOM);
                else
                {
                    Comm.act("$n looks at you.", ch, null, victim, TO_VICT);
                    Comm.act("$n looks at $N.", ch, null, victim, TO_NOTVICT);
                }
            }

            if (victim.description.Length != 0)
            {
                Comm.send_to_char(victim.description, ch);
            }
            else
            {
                Comm.act("You see nothing special about $M.", ch, null, victim, TO_CHAR);
            }

            int percent;
            if (victim.max_hit > 0)
                percent = (100 * victim.hit) / victim.max_hit;
            else
                percent = -1;

            string buf = Handler.PERS(victim, ch);

            if (percent >= 100)
                buf += " is in excellent condition.\n\r";
            else if (percent >= 90)
                buf += " has a few scratches.\n\r";
            else if (percent >= 75)
                buf += " has some small wounds and bruises.\n\r";
            else if (percent >= 50)
                buf += " has quite a few wounds.\n\r";
            else if (percent >= 30)
                buf += " has some big nasty wounds and scratches.\n\r";
            else if (percent >= 15)
                buf += " looks pretty hurt.\n\r";
            else if (percent >= 0)
                buf += " is in awful condition.\n\r";
            else
                buf += " is bleeding to death.\n\r";

            if (buf.Length > 0)
            {
                var chars = buf.ToCharArray();
                chars[0] = Bit.UPPER(chars[0]);
                buf = new string(chars);
            }
            Comm.send_to_char(buf, ch);

            bool found = false;
            for (int iWear = 0; iWear < MAX_WEAR; iWear++)
            {
                var obj = Handler.get_eq_char(victim, iWear);
                if (obj != null && Handler.can_see_obj(ch, obj))
                {
                    if (!found)
                    {
                        Comm.send_to_char("\n\r", ch);
                        Comm.act("$N is using:", ch, null, victim, TO_CHAR);
                        found = true;
                    }
                    Comm.send_to_char(where_name[iWear], ch);
                    Comm.send_to_char(format_obj_to_char(obj, ch, true), ch);
                    Comm.send_to_char("\n\r", ch);
                }
            }

            if (victim != ch && !Bit.IS_NPC(ch)
                && RomRandom.number_percent() < Handler.get_skill(ch, Gsn.peek))
            {
                Comm.send_to_char("\n\rYou peek at the inventory:\n\r", ch);
                Skills.check_improve(ch, Gsn.peek, true, 4);
                show_list_to_char(victim.carrying, ch, true, true);
            }
        }

        static void show_char_to_char(CharData list, CharData ch)
        {
            for (var rch = list; rch != null; rch = rch.next_in_room)
            {
                if (rch == ch)
                    continue;

                if (Handler.get_trust(ch) < rch.invis_level)
                    continue;

                if (Handler.can_see(ch, rch))
                {
                    show_char_to_char_0(rch, ch);
                }
                else if (Handler.room_is_dark(ch.in_room)
                         && Bit.IS_AFFECTED(rch, AFF_INFRARED))
                {
                    Comm.send_to_char("You see glowing red eyes watching YOU!\n\r", ch);
                }
            }
        }

        public static void do_look(CharData ch, string argument)
        {
            argument ??= "";
            if (ch.desc == null)
                return;

            if (ch.position < POS_SLEEPING)
            {
                Comm.send_to_char("You can't see anything but stars!\n\r", ch);
                return;
            }

            if (ch.position == POS_SLEEPING)
            {
                Comm.send_to_char("You can't see anything, you're sleeping!\n\r", ch);
                return;
            }

            if (!Handler.check_blind(ch))
                return;

            if (!Bit.IS_NPC(ch)
                && !Bit.IS_SET(ch.act, PLR_HOLYLIGHT) && Handler.room_is_dark(ch.in_room))
            {
                Comm.send_to_char("It is pitch black ... \n\r", ch);
                show_char_to_char(ch.in_room.people, ch);
                return;
            }

            argument = RomString.one_argument(argument, out string arg1);
            argument = RomString.one_argument(argument, out string arg2);
            int number = RomString.number_argument(arg1, out string arg3);
            int count = 0;

            if (arg1.Length == 0 || !RomString.str_cmp(arg1, "auto"))
            {
                Gmcp.RoomInfo(ch);
                Comm.send_to_char("{s", ch);
                Comm.send_to_char(ch.in_room.name, ch);
                Comm.send_to_char("{x", ch);

                if ((Bit.IS_IMMORTAL(ch)
                     && (Bit.IS_NPC(ch) || Bit.IS_SET(ch.act, PLR_HOLYLIGHT)))
                    || Bit.IS_BUILDER(ch, ch.in_room.area))
                {
                    Comm.send_to_char(RomString.sprintf("{r [{RRoom %d{r]{x", ch.in_room.vnum), ch);
                }

                Comm.send_to_char("\n\r", ch);

                if (arg1.Length == 0
                    || (!Bit.IS_NPC(ch) && !Bit.IS_SET(ch.comm, COMM_BRIEF)))
                {
                    Comm.send_to_char("  ", ch);
                    Comm.send_to_char("{S", ch);
                    Comm.send_to_char(ch.in_room.description, ch);
                    Comm.send_to_char("{x", ch);
                }

                if (!Bit.IS_NPC(ch) && Bit.IS_SET(ch.act, PLR_AUTOEXIT))
                {
                    Comm.send_to_char("\n\r", ch);
                    Interp.do_function(ch, do_exits, "auto");
                }

                show_list_to_char(ch.in_room.contents, ch, false, false);
                show_char_to_char(ch.in_room.people, ch);
                return;
            }

            if (!RomString.str_cmp(arg1, "i") || !RomString.str_cmp(arg1, "in")
                || !RomString.str_cmp(arg1, "on"))
            {
                if (arg2.Length == 0)
                {
                    Comm.send_to_char("Look in what?\n\r", ch);
                    return;
                }

                var obj = Handler.get_obj_here(ch, arg2);
                if (obj == null)
                {
                    Comm.send_to_char("You do not see that here.\n\r", ch);
                    return;
                }

                switch (obj.item_type)
                {
                    default:
                        Comm.send_to_char("That is not a container.\n\r", ch);
                        break;

                    case ITEM_DRINK_CON:
                        if (obj.value[1] <= 0)
                        {
                            Comm.send_to_char("It is empty.\n\r", ch);
                            break;
                        }

                        Comm.send_to_char(RomString.sprintf("It's %sfilled with  a %s liquid.\n\r",
                            obj.value[1] < obj.value[0] / 4
                            ? "less than half-" :
                            obj.value[1] < 3 * obj.value[0] / 4
                            ? "about half-" : "more than half-",
                            Tables.liq_table[obj.value[2]].liq_color), ch);
                        break;

                    case ITEM_CONTAINER:
                    case ITEM_CORPSE_NPC:
                    case ITEM_CORPSE_PC:
                        if (Bit.IS_SET(obj.value[1], CONT_CLOSED))
                        {
                            Comm.send_to_char("It is closed.\n\r", ch);
                            break;
                        }

                        Comm.act("$p holds:", ch, obj, null, TO_CHAR);
                        show_list_to_char(obj.contains, ch, true, true);
                        break;
                }
                return;
            }

            var victim = Handler.get_char_room(ch, arg1);
            if (victim != null)
            {
                show_char_to_char_1(victim, ch);
                return;
            }

            for (var obj = ch.carrying; obj != null; obj = obj.next_content)
            {
                if (Handler.can_see_obj(ch, obj))
                {
                    string pdesc = Db.get_extra_descr(arg3, obj.extra_descr);
                    if (pdesc != null)
                    {
                        if (++count == number)
                        {
                            Comm.send_to_char(pdesc, ch);
                            return;
                        }
                        else
                            continue;
                    }

                    pdesc = Db.get_extra_descr(arg3, obj.pIndexData.extra_descr);
                    if (pdesc != null)
                    {
                        if (++count == number)
                        {
                            Comm.send_to_char(pdesc, ch);
                            return;
                        }
                        else
                            continue;
                    }

                    if (Handler.is_name(arg3, obj.name))
                        if (++count == number)
                        {
                            Comm.send_to_char(obj.description, ch);
                            Comm.send_to_char("\n\r", ch);
                            return;
                        }
                }
            }

            for (var obj = ch.in_room.contents; obj != null; obj = obj.next_content)
            {
                if (Handler.can_see_obj(ch, obj))
                {
                    string pdesc = Db.get_extra_descr(arg3, obj.extra_descr);
                    if (pdesc != null)
                        if (++count == number)
                        {
                            Comm.send_to_char(pdesc, ch);
                            return;
                        }

                    pdesc = Db.get_extra_descr(arg3, obj.pIndexData.extra_descr);
                    if (pdesc != null)
                        if (++count == number)
                        {
                            Comm.send_to_char(pdesc, ch);
                            return;
                        }

                    if (Handler.is_name(arg3, obj.name))
                        if (++count == number)
                        {
                            Comm.send_to_char(obj.description, ch);
                            Comm.send_to_char("\n\r", ch);
                            return;
                        }
                }
            }

            {
                string pdesc = Db.get_extra_descr(arg3, ch.in_room.extra_descr);
                if (pdesc != null)
                {
                    if (++count == number)
                    {
                        Comm.send_to_char(pdesc, ch);
                        return;
                    }
                }
            }

            if (count > 0 && count != number)
            {
                if (count == 1)
                    Comm.send_to_char(RomString.sprintf("You only see one %s here.\n\r", arg3), ch);
                else
                    Comm.send_to_char(RomString.sprintf("You only see %d of those here.\n\r", count), ch);
                return;
            }

            int door;
            if (!RomString.str_cmp(arg1, "n") || !RomString.str_cmp(arg1, "north"))
                door = 0;
            else if (!RomString.str_cmp(arg1, "e") || !RomString.str_cmp(arg1, "east"))
                door = 1;
            else if (!RomString.str_cmp(arg1, "s") || !RomString.str_cmp(arg1, "south"))
                door = 2;
            else if (!RomString.str_cmp(arg1, "w") || !RomString.str_cmp(arg1, "west"))
                door = 3;
            else if (!RomString.str_cmp(arg1, "u") || !RomString.str_cmp(arg1, "up"))
                door = 4;
            else if (!RomString.str_cmp(arg1, "d") || !RomString.str_cmp(arg1, "down"))
                door = 5;
            else
            {
                Comm.send_to_char("You do not see that here.\n\r", ch);
                return;
            }

            var pexit = ch.in_room.exit[door];
            if (pexit == null)
            {
                Comm.send_to_char("Nothing special there.\n\r", ch);
                return;
            }

            if (pexit.description != null && pexit.description.Length != 0)
                Comm.send_to_char(pexit.description, ch);
            else
                Comm.send_to_char("Nothing special there.\n\r", ch);

            if (pexit.keyword != null
                && pexit.keyword.Length != 0 && pexit.keyword[0] != ' ')
            {
                if (Bit.IS_SET(pexit.exit_info, EX_CLOSED))
                    Comm.act("The $d is closed.", ch, null, pexit.keyword, TO_CHAR);
                else if (Bit.IS_SET(pexit.exit_info, EX_ISDOOR))
                    Comm.act("The $d is open.", ch, null, pexit.keyword, TO_CHAR);
            }
        }

        public static void do_inventory(CharData ch, string argument)
        {
            Comm.send_to_char("You are carrying:\n\r", ch);
            show_list_to_char(ch.carrying, ch, true, true);
        }

        public static void do_equipment(CharData ch, string argument)
        {
            Comm.send_to_char("You are using:\n\r", ch);
            bool found = false;
            for (int iWear = 0; iWear < MAX_WEAR; iWear++)
            {
                var obj = Handler.get_eq_char(ch, iWear);
                if (obj == null)
                    continue;

                Comm.send_to_char(where_name[iWear], ch);
                if (Handler.can_see_obj(ch, obj))
                {
                    Comm.send_to_char(format_obj_to_char(obj, ch, true), ch);
                    Comm.send_to_char("\n\r", ch);
                }
                else
                {
                    Comm.send_to_char("something.\n\r", ch);
                }
                found = true;
            }

            if (!found)
                Comm.send_to_char("Nothing.\n\r", ch);
        }
    }
}
