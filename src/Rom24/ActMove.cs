using static Rom24.Merc;

namespace Rom24
{
    public static class ActMove
    {
        public static readonly string[] dir_name = { "north", "east", "south", "west", "up", "down" };
        public static readonly int[] rev_dir = { 2, 3, 0, 1, 5, 4 };
        public static readonly int[] movement_loss = { 1, 2, 2, 3, 4, 6, 4, 1, 6, 10, 6 };

        public static void move_char(CharData ch, int door, bool follow)
        {
            if (door < 0 || door > 5)
            {
                Db.bug("Do_move: bad door %d.", door);
                return;
            }
            if (!Bit.IS_NPC(ch) && MobProg.mp_exit_trigger(ch, door))
                return;
            var in_room = ch.in_room;
            var pexit = in_room.exit[door];
            if (pexit == null || pexit.to_room == null || !Handler.can_see_room(ch, pexit.to_room))
            {
                Comm.send_to_char("Alas, you cannot go that way.\n\r", ch);
                Gmcp.WrongDir(ch, door);
                return;
            }
            var to_room = pexit.to_room;
            if (Bit.IS_SET(pexit.exit_info, EX_CLOSED)
                && (!Bit.IS_AFFECTED(ch, AFF_PASS_DOOR) || Bit.IS_SET(pexit.exit_info, EX_NOPASS))
                && !Bit.IS_TRUSTED(ch, ANGEL))
            {
                Comm.act("The $d is closed.", ch, null, pexit.keyword, TO_CHAR);
                return;
            }

            if (Bit.IS_AFFECTED(ch, AFF_CHARM)
                && ch.master != null && in_room == ch.master.in_room)
            {
                Comm.send_to_char("What?  And leave your beloved master?\n\r", ch);
                return;
            }

            if (!Handler.is_room_owner(ch, to_room) && Handler.room_is_private(to_room))
            {
                Comm.send_to_char("That room is private right now.\n\r", ch);
                return;
            }
            if (!Bit.IS_NPC(ch))
            {
                for (int iClass = 0; iClass < MAX_CLASS; iClass++)
                {
                    for (int iGuild = 0; iGuild < MAX_GUILD; iGuild++)
                    {
                        if (iClass != ch.klass
                            && to_room.vnum == Tables.class_table[iClass].guild[iGuild])
                        {
                            Comm.send_to_char("You aren't allowed in there.\n\r", ch);
                            return;
                        }
                    }
                }

                if (in_room.sector_type == SECT_AIR
                    || to_room.sector_type == SECT_AIR)
                {
                    if (!Bit.IS_AFFECTED(ch, AFF_FLYING) && !Bit.IS_IMMORTAL(ch))
                    {
                        Comm.send_to_char("You can't fly.\n\r", ch);
                        return;
                    }
                }

                if ((in_room.sector_type == SECT_WATER_NOSWIM
                     || to_room.sector_type == SECT_WATER_NOSWIM)
                    && !Bit.IS_AFFECTED(ch, AFF_FLYING))
                {
                    bool found = false;

                    if (Bit.IS_IMMORTAL(ch))
                        found = true;

                    for (var obj = ch.carrying; obj != null; obj = obj.next_content)
                    {
                        if (obj.item_type == ITEM_BOAT)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        Comm.send_to_char("You need a boat to go there.\n\r", ch);
                        return;
                    }
                }

                /* UMIN only caps the high end. Stock rooms use sector -1. */
                int move = movement_loss[Bit.URANGE(0, in_room.sector_type, SECT_MAX - 1)]
                    + movement_loss[Bit.URANGE(0, to_room.sector_type, SECT_MAX - 1)];

                move /= 2;

                if (Bit.IS_AFFECTED(ch, AFF_FLYING) || Bit.IS_AFFECTED(ch, AFF_HASTE))
                    move /= 2;

                if (Bit.IS_AFFECTED(ch, AFF_SLOW))
                    move *= 2;

                if (ch.move < move)
                {
                    Comm.send_to_char("You are too exhausted.\n\r", ch);
                    return;
                }

                Bit.WAIT_STATE(ch, 1);
                ch.move -= move;
            }
            if (!Bit.IS_AFFECTED(ch, AFF_SNEAK) && ch.invis_level < LEVEL_HERO)
                Comm.act("$n leaves $T.", ch, null, dir_name[door], TO_ROOM);
            Handler.char_from_room(ch);
            Handler.char_to_room(ch, to_room);
            if (!Bit.IS_AFFECTED(ch, AFF_SNEAK) && ch.invis_level < LEVEL_HERO)
                Comm.act("$n has arrived.", ch, null, null, TO_ROOM);
            Interp.do_look(ch, "auto");

            if (in_room == to_room)        /* no circular follows */
                return;

            for (var fch = in_room.people; fch != null; )
            {
                var fch_next = fch.next_in_room;

                if (fch.master == ch && Bit.IS_AFFECTED(fch, AFF_CHARM)
                    && fch.position < POS_STANDING)
                    Interp.do_function(fch, do_stand, "");

                if (fch.master == ch && fch.position == POS_STANDING
                    && Handler.can_see_room(fch, to_room))
                {
                    if (Bit.IS_SET(ch.in_room.room_flags, ROOM_LAW)
                        && (Bit.IS_NPC(fch) && Bit.IS_SET(fch.act, ACT_AGGRESSIVE)))
                    {
                        Comm.act("You can't bring $N into the city.",
                            ch, null, fch, TO_CHAR);
                        Comm.act("You aren't allowed in the city.",
                            fch, null, null, TO_CHAR);
                        fch = fch_next;
                        continue;
                    }

                    Comm.act("You follow $N.", fch, null, ch, TO_CHAR);
                    move_char(fch, door, true);
                }
                fch = fch_next;
            }

            /*
             * If someone is following the char, these triggers get activated
             * for the followers before the char, but it's safer this way...
             */
            if (Bit.IS_NPC(ch) && Bit.HAS_TRIGGER(ch, TRIG_ENTRY))
                MobProg.mp_percent_trigger(ch, null, null, null, (int)TRIG_ENTRY);
            if (!Bit.IS_NPC(ch))
                MobProg.mp_greet_trigger(ch);
        }

        public static void do_recall(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch) && !Bit.IS_SET(ch.act, ACT_PET))
            {
                Comm.send_to_char("Only players can recall.\n\r", ch);
                return;
            }

            Comm.act("$n prays for transportation!", ch, null, null, TO_ROOM);

            var location = Handler.get_room_index(ROOM_VNUM_TEMPLE);
            if (location == null)
            {
                Comm.send_to_char("You are completely lost.\n\r", ch);
                return;
            }

            if (ch.in_room == location)
                return;

            if (Bit.IS_SET(ch.in_room.room_flags, ROOM_NO_RECALL)
                || Bit.IS_AFFECTED(ch, AFF_CURSE))
            {
                Comm.send_to_char("Mota has forsaken you.\n\r", ch);
                return;
            }

            var victim = ch.fighting;
            if (victim != null)
            {
                int skill = Handler.get_skill(ch, Gsn.recall);

                if (RomRandom.number_percent() < 80 * skill / 100)
                {
                    Skills.check_improve(ch, Gsn.recall, false, 6);
                    Bit.WAIT_STATE(ch, 4);
                    Comm.send_to_char("You failed!.\n\r", ch);
                    return;
                }

                int lose = (ch.desc != null) ? 25 : 50;
                Update.gain_exp(ch, 0 - lose);
                Skills.check_improve(ch, Gsn.recall, true, 4);
                Comm.send_to_char($"You recall from combat!  You lose {lose} exps.\n\r", ch);
                Fight.stop_fighting(ch, true);
            }

            ch.move /= 2;
            Comm.act("$n disappears.", ch, null, null, TO_ROOM);
            Handler.char_from_room(ch);
            Handler.char_to_room(ch, location);
            Comm.act("$n appears in the room.", ch, null, null, TO_ROOM);
            Interp.do_function(ch, Interp.do_look, "auto");

            if (ch.pet != null)
                Interp.do_function(ch.pet, do_recall, "");
        }

        public static int find_door(CharData ch, string arg)
        {
            ExitData pexit;
            int door;

            if (!RomString.str_cmp(arg, "n") || !RomString.str_cmp(arg, "north"))
                door = 0;
            else if (!RomString.str_cmp(arg, "e") || !RomString.str_cmp(arg, "east"))
                door = 1;
            else if (!RomString.str_cmp(arg, "s") || !RomString.str_cmp(arg, "south"))
                door = 2;
            else if (!RomString.str_cmp(arg, "w") || !RomString.str_cmp(arg, "west"))
                door = 3;
            else if (!RomString.str_cmp(arg, "u") || !RomString.str_cmp(arg, "up"))
                door = 4;
            else if (!RomString.str_cmp(arg, "d") || !RomString.str_cmp(arg, "down"))
                door = 5;
            else
            {
                for (door = 0; door <= 5; door++)
                {
                    if ((pexit = ch.in_room.exit[door]) != null
                        && Bit.IS_SET(pexit.exit_info, EX_ISDOOR)
                        && pexit.keyword != null && Handler.is_name(arg, pexit.keyword))
                        return door;
                }
                Comm.act("I see no $T here.", ch, null, arg, TO_CHAR);
                return -1;
            }

            if ((pexit = ch.in_room.exit[door]) == null)
            {
                Comm.act("I see no door $T here.", ch, null, arg, TO_CHAR);
                return -1;
            }

            if (!Bit.IS_SET(pexit.exit_info, EX_ISDOOR))
            {
                Comm.send_to_char("You can't do that.\n\r", ch);
                return -1;
            }

            return door;
        }

        public static void do_open(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Open what?\n\r", ch);
                return;
            }

            var obj = Handler.get_obj_here(ch, arg);
            if (obj != null)
            {
                if (obj.item_type == ITEM_PORTAL)
                {
                    if (!Bit.IS_SET(obj.value[1], EX_ISDOOR))
                    {
                        Comm.send_to_char("You can't do that.\n\r", ch);
                        return;
                    }

                    if (!Bit.IS_SET(obj.value[1], EX_CLOSED))
                    {
                        Comm.send_to_char("It's already open.\n\r", ch);
                        return;
                    }

                    if (Bit.IS_SET(obj.value[1], EX_LOCKED))
                    {
                        Comm.send_to_char("It's locked.\n\r", ch);
                        return;
                    }

                    Bit.REMOVE_BIT(ref obj.value[1], EX_CLOSED);
                    Comm.act("You open $p.", ch, obj, null, TO_CHAR);
                    Comm.act("$n opens $p.", ch, obj, null, TO_ROOM);
                    return;
                }

                if (obj.item_type != ITEM_CONTAINER)
                {
                    Comm.send_to_char("That's not a container.\n\r", ch);
                    return;
                }
                if (!Bit.IS_SET(obj.value[1], CONT_CLOSED))
                {
                    Comm.send_to_char("It's already open.\n\r", ch);
                    return;
                }
                if (!Bit.IS_SET(obj.value[1], CONT_CLOSEABLE))
                {
                    Comm.send_to_char("You can't do that.\n\r", ch);
                    return;
                }
                if (Bit.IS_SET(obj.value[1], CONT_LOCKED))
                {
                    Comm.send_to_char("It's locked.\n\r", ch);
                    return;
                }

                Bit.REMOVE_BIT(ref obj.value[1], CONT_CLOSED);
                Comm.act("You open $p.", ch, obj, null, TO_CHAR);
                Comm.act("$n opens $p.", ch, obj, null, TO_ROOM);
                return;
            }

            int door = find_door(ch, arg);
            if (door >= 0)
            {
                var pexit = ch.in_room.exit[door];
                if (!Bit.IS_SET(pexit.exit_info, EX_CLOSED))
                {
                    Comm.send_to_char("It's already open.\n\r", ch);
                    return;
                }
                if (Bit.IS_SET(pexit.exit_info, EX_LOCKED))
                {
                    Comm.send_to_char("It's locked.\n\r", ch);
                    return;
                }

                Bit.REMOVE_BIT(ref pexit.exit_info, EX_CLOSED);
                Comm.act("$n opens the $d.", ch, null, pexit.keyword, TO_ROOM);
                Comm.send_to_char("Ok.\n\r", ch);

                var to_room = pexit.to_room;
                ExitData pexit_rev;
                if (to_room != null
                    && (pexit_rev = to_room.exit[rev_dir[door]]) != null
                    && pexit_rev.to_room == ch.in_room)
                {
                    Bit.REMOVE_BIT(ref pexit_rev.exit_info, EX_CLOSED);
                    for (var rch = to_room.people; rch != null; rch = rch.next_in_room)
                        Comm.act("The $d opens.", rch, null, pexit_rev.keyword, TO_CHAR);
                }
            }
        }

        public static void do_close(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Close what?\n\r", ch);
                return;
            }

            var obj = Handler.get_obj_here(ch, arg);
            if (obj != null)
            {
                if (obj.item_type == ITEM_PORTAL)
                {
                    if (!Bit.IS_SET(obj.value[1], EX_ISDOOR)
                        || Bit.IS_SET(obj.value[1], EX_NOCLOSE))
                    {
                        Comm.send_to_char("You can't do that.\n\r", ch);
                        return;
                    }

                    if (Bit.IS_SET(obj.value[1], EX_CLOSED))
                    {
                        Comm.send_to_char("It's already closed.\n\r", ch);
                        return;
                    }

                    Bit.SET_BIT(ref obj.value[1], EX_CLOSED);
                    Comm.act("You close $p.", ch, obj, null, TO_CHAR);
                    Comm.act("$n closes $p.", ch, obj, null, TO_ROOM);
                    return;
                }

                if (obj.item_type != ITEM_CONTAINER)
                {
                    Comm.send_to_char("That's not a container.\n\r", ch);
                    return;
                }
                if (Bit.IS_SET(obj.value[1], CONT_CLOSED))
                {
                    Comm.send_to_char("It's already closed.\n\r", ch);
                    return;
                }
                if (!Bit.IS_SET(obj.value[1], CONT_CLOSEABLE))
                {
                    Comm.send_to_char("You can't do that.\n\r", ch);
                    return;
                }

                Bit.SET_BIT(ref obj.value[1], CONT_CLOSED);
                Comm.act("You close $p.", ch, obj, null, TO_CHAR);
                Comm.act("$n closes $p.", ch, obj, null, TO_ROOM);
                return;
            }

            int door = find_door(ch, arg);
            if (door >= 0)
            {
                var pexit = ch.in_room.exit[door];
                if (Bit.IS_SET(pexit.exit_info, EX_CLOSED))
                {
                    Comm.send_to_char("It's already closed.\n\r", ch);
                    return;
                }

                Bit.SET_BIT(ref pexit.exit_info, EX_CLOSED);
                Comm.act("$n closes the $d.", ch, null, pexit.keyword, TO_ROOM);
                Comm.send_to_char("Ok.\n\r", ch);

                var to_room = pexit.to_room;
                ExitData pexit_rev;
                if (to_room != null
                    && (pexit_rev = to_room.exit[rev_dir[door]]) != null
                    && pexit_rev.to_room == ch.in_room)
                {
                    Bit.SET_BIT(ref pexit_rev.exit_info, EX_CLOSED);
                    for (var rch = to_room.people; rch != null; rch = rch.next_in_room)
                        Comm.act("The $d closes.", rch, null, pexit_rev.keyword,
                            TO_CHAR);
                }
            }
        }

        public static bool has_key(CharData ch, int key)
        {
            for (var obj = ch.carrying; obj != null; obj = obj.next_content)
            {
                if (obj.pIndexData.vnum == key)
                    return true;
            }

            return false;
        }

        public static void do_lock(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Lock what?\n\r", ch);
                return;
            }

            var obj = Handler.get_obj_here(ch, arg);
            if (obj != null)
            {
                if (obj.item_type == ITEM_PORTAL)
                {
                    if (!Bit.IS_SET(obj.value[1], EX_ISDOOR)
                        || Bit.IS_SET(obj.value[1], EX_NOCLOSE))
                    {
                        Comm.send_to_char("You can't do that.\n\r", ch);
                        return;
                    }
                    if (!Bit.IS_SET(obj.value[1], EX_CLOSED))
                    {
                        Comm.send_to_char("It's not closed.\n\r", ch);
                        return;
                    }

                    if (obj.value[4] < 0 || Bit.IS_SET(obj.value[1], EX_NOLOCK))
                    {
                        Comm.send_to_char("It can't be locked.\n\r", ch);
                        return;
                    }

                    if (!has_key(ch, obj.value[4]))
                    {
                        Comm.send_to_char("You lack the key.\n\r", ch);
                        return;
                    }

                    if (Bit.IS_SET(obj.value[1], EX_LOCKED))
                    {
                        Comm.send_to_char("It's already locked.\n\r", ch);
                        return;
                    }

                    Bit.SET_BIT(ref obj.value[1], EX_LOCKED);
                    Comm.act("You lock $p.", ch, obj, null, TO_CHAR);
                    Comm.act("$n locks $p.", ch, obj, null, TO_ROOM);
                    return;
                }

                if (obj.item_type != ITEM_CONTAINER)
                {
                    Comm.send_to_char("That's not a container.\n\r", ch);
                    return;
                }
                if (!Bit.IS_SET(obj.value[1], CONT_CLOSED))
                {
                    Comm.send_to_char("It's not closed.\n\r", ch);
                    return;
                }
                if (obj.value[2] < 0)
                {
                    Comm.send_to_char("It can't be locked.\n\r", ch);
                    return;
                }
                if (!has_key(ch, obj.value[2]))
                {
                    Comm.send_to_char("You lack the key.\n\r", ch);
                    return;
                }
                if (Bit.IS_SET(obj.value[1], CONT_LOCKED))
                {
                    Comm.send_to_char("It's already locked.\n\r", ch);
                    return;
                }

                Bit.SET_BIT(ref obj.value[1], CONT_LOCKED);
                Comm.act("You lock $p.", ch, obj, null, TO_CHAR);
                Comm.act("$n locks $p.", ch, obj, null, TO_ROOM);
                return;
            }

            int door = find_door(ch, arg);
            if (door >= 0)
            {
                var pexit = ch.in_room.exit[door];
                if (!Bit.IS_SET(pexit.exit_info, EX_CLOSED))
                {
                    Comm.send_to_char("It's not closed.\n\r", ch);
                    return;
                }
                if (pexit.key < 0)
                {
                    Comm.send_to_char("It can't be locked.\n\r", ch);
                    return;
                }
                if (!has_key(ch, pexit.key))
                {
                    Comm.send_to_char("You lack the key.\n\r", ch);
                    return;
                }
                if (Bit.IS_SET(pexit.exit_info, EX_LOCKED))
                {
                    Comm.send_to_char("It's already locked.\n\r", ch);
                    return;
                }

                Bit.SET_BIT(ref pexit.exit_info, EX_LOCKED);
                Comm.send_to_char("*Click*\n\r", ch);
                Comm.act("$n locks the $d.", ch, null, pexit.keyword, TO_ROOM);

                var to_room = pexit.to_room;
                ExitData pexit_rev;
                if (to_room != null
                    && (pexit_rev = to_room.exit[rev_dir[door]]) != null
                    && pexit_rev.to_room == ch.in_room)
                {
                    Bit.SET_BIT(ref pexit_rev.exit_info, EX_LOCKED);
                }
            }
        }

        public static void do_unlock(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Unlock what?\n\r", ch);
                return;
            }

            var obj = Handler.get_obj_here(ch, arg);
            if (obj != null)
            {
                if (obj.item_type == ITEM_PORTAL)
                {
                    if (!Bit.IS_SET(obj.value[1], EX_ISDOOR))
                    {
                        Comm.send_to_char("You can't do that.\n\r", ch);
                        return;
                    }

                    if (!Bit.IS_SET(obj.value[1], EX_CLOSED))
                    {
                        Comm.send_to_char("It's not closed.\n\r", ch);
                        return;
                    }

                    if (obj.value[4] < 0)
                    {
                        Comm.send_to_char("It can't be unlocked.\n\r", ch);
                        return;
                    }

                    if (!has_key(ch, obj.value[4]))
                    {
                        Comm.send_to_char("You lack the key.\n\r", ch);
                        return;
                    }

                    if (!Bit.IS_SET(obj.value[1], EX_LOCKED))
                    {
                        Comm.send_to_char("It's already unlocked.\n\r", ch);
                        return;
                    }

                    Bit.REMOVE_BIT(ref obj.value[1], EX_LOCKED);
                    Comm.act("You unlock $p.", ch, obj, null, TO_CHAR);
                    Comm.act("$n unlocks $p.", ch, obj, null, TO_ROOM);
                    return;
                }

                if (obj.item_type != ITEM_CONTAINER)
                {
                    Comm.send_to_char("That's not a container.\n\r", ch);
                    return;
                }
                if (!Bit.IS_SET(obj.value[1], CONT_CLOSED))
                {
                    Comm.send_to_char("It's not closed.\n\r", ch);
                    return;
                }
                if (obj.value[2] < 0)
                {
                    Comm.send_to_char("It can't be unlocked.\n\r", ch);
                    return;
                }
                if (!has_key(ch, obj.value[2]))
                {
                    Comm.send_to_char("You lack the key.\n\r", ch);
                    return;
                }
                if (!Bit.IS_SET(obj.value[1], CONT_LOCKED))
                {
                    Comm.send_to_char("It's already unlocked.\n\r", ch);
                    return;
                }

                Bit.REMOVE_BIT(ref obj.value[1], CONT_LOCKED);
                Comm.act("You unlock $p.", ch, obj, null, TO_CHAR);
                Comm.act("$n unlocks $p.", ch, obj, null, TO_ROOM);
                return;
            }

            int door = find_door(ch, arg);
            if (door >= 0)
            {
                var pexit = ch.in_room.exit[door];
                if (!Bit.IS_SET(pexit.exit_info, EX_CLOSED))
                {
                    Comm.send_to_char("It's not closed.\n\r", ch);
                    return;
                }
                if (pexit.key < 0)
                {
                    Comm.send_to_char("It can't be unlocked.\n\r", ch);
                    return;
                }
                if (!has_key(ch, pexit.key))
                {
                    Comm.send_to_char("You lack the key.\n\r", ch);
                    return;
                }
                if (!Bit.IS_SET(pexit.exit_info, EX_LOCKED))
                {
                    Comm.send_to_char("It's already unlocked.\n\r", ch);
                    return;
                }

                Bit.REMOVE_BIT(ref pexit.exit_info, EX_LOCKED);
                Comm.send_to_char("*Click*\n\r", ch);
                Comm.act("$n unlocks the $d.", ch, null, pexit.keyword, TO_ROOM);

                var to_room = pexit.to_room;
                ExitData pexit_rev;
                if (to_room != null
                    && (pexit_rev = to_room.exit[rev_dir[door]]) != null
                    && pexit_rev.to_room == ch.in_room)
                {
                    Bit.REMOVE_BIT(ref pexit_rev.exit_info, EX_LOCKED);
                }
            }
        }

        public static void do_pick(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Pick what?\n\r", ch);
                return;
            }

            Bit.WAIT_STATE(ch, Tables.skill_table[Gsn.pick_lock].beats);

            for (var gch = ch.in_room.people; gch != null; gch = gch.next_in_room)
            {
                if (Bit.IS_NPC(gch) && Bit.IS_AWAKE(gch) && ch.level + 5 < gch.level)
                {
                    Comm.act("$N is standing too close to the lock.",
                        ch, null, gch, TO_CHAR);
                    return;
                }
            }

            if (!Bit.IS_NPC(ch) && RomRandom.number_percent() > Handler.get_skill(ch, Gsn.pick_lock))
            {
                Comm.send_to_char("You failed.\n\r", ch);
                Skills.check_improve(ch, Gsn.pick_lock, false, 2);
                return;
            }

            var obj = Handler.get_obj_here(ch, arg);
            if (obj != null)
            {
                if (obj.item_type == ITEM_PORTAL)
                {
                    if (!Bit.IS_SET(obj.value[1], EX_ISDOOR))
                    {
                        Comm.send_to_char("You can't do that.\n\r", ch);
                        return;
                    }

                    if (!Bit.IS_SET(obj.value[1], EX_CLOSED))
                    {
                        Comm.send_to_char("It's not closed.\n\r", ch);
                        return;
                    }

                    if (obj.value[4] < 0)
                    {
                        Comm.send_to_char("It can't be unlocked.\n\r", ch);
                        return;
                    }

                    if (Bit.IS_SET(obj.value[1], EX_PICKPROOF))
                    {
                        Comm.send_to_char("You failed.\n\r", ch);
                        return;
                    }

                    Bit.REMOVE_BIT(ref obj.value[1], EX_LOCKED);
                    Comm.act("You pick the lock on $p.", ch, obj, null, TO_CHAR);
                    Comm.act("$n picks the lock on $p.", ch, obj, null, TO_ROOM);
                    Skills.check_improve(ch, Gsn.pick_lock, true, 2);
                    return;
                }

                if (obj.item_type != ITEM_CONTAINER)
                {
                    Comm.send_to_char("That's not a container.\n\r", ch);
                    return;
                }
                if (!Bit.IS_SET(obj.value[1], CONT_CLOSED))
                {
                    Comm.send_to_char("It's not closed.\n\r", ch);
                    return;
                }
                if (obj.value[2] < 0)
                {
                    Comm.send_to_char("It can't be unlocked.\n\r", ch);
                    return;
                }
                if (!Bit.IS_SET(obj.value[1], CONT_LOCKED))
                {
                    Comm.send_to_char("It's already unlocked.\n\r", ch);
                    return;
                }
                if (Bit.IS_SET(obj.value[1], CONT_PICKPROOF))
                {
                    Comm.send_to_char("You failed.\n\r", ch);
                    return;
                }

                Bit.REMOVE_BIT(ref obj.value[1], CONT_LOCKED);
                Comm.act("You pick the lock on $p.", ch, obj, null, TO_CHAR);
                Comm.act("$n picks the lock on $p.", ch, obj, null, TO_ROOM);
                Skills.check_improve(ch, Gsn.pick_lock, true, 2);
                return;
            }

            int door = find_door(ch, arg);
            if (door >= 0)
            {
                var pexit = ch.in_room.exit[door];
                if (!Bit.IS_SET(pexit.exit_info, EX_CLOSED) && !Bit.IS_IMMORTAL(ch))
                {
                    Comm.send_to_char("It's not closed.\n\r", ch);
                    return;
                }
                if (pexit.key < 0 && !Bit.IS_IMMORTAL(ch))
                {
                    Comm.send_to_char("It can't be picked.\n\r", ch);
                    return;
                }
                if (!Bit.IS_SET(pexit.exit_info, EX_LOCKED))
                {
                    Comm.send_to_char("It's already unlocked.\n\r", ch);
                    return;
                }
                if (Bit.IS_SET(pexit.exit_info, EX_PICKPROOF) && !Bit.IS_IMMORTAL(ch))
                {
                    Comm.send_to_char("You failed.\n\r", ch);
                    return;
                }

                Bit.REMOVE_BIT(ref pexit.exit_info, EX_LOCKED);
                Comm.send_to_char("*Click*\n\r", ch);
                Comm.act("$n picks the $d.", ch, null, pexit.keyword, TO_ROOM);
                Skills.check_improve(ch, Gsn.pick_lock, true, 2);

                var to_room = pexit.to_room;
                ExitData pexit_rev;
                if (to_room != null
                    && (pexit_rev = to_room.exit[rev_dir[door]]) != null
                    && pexit_rev.to_room == ch.in_room)
                {
                    Bit.REMOVE_BIT(ref pexit_rev.exit_info, EX_LOCKED);
                }
            }
        }

        public static void do_stand(CharData ch, string argument)
        {
            ObjData obj = null;

            if (argument.Length != 0)
            {
                if (ch.position == POS_FIGHTING)
                {
                    Comm.send_to_char("Maybe you should finish fighting first?\n\r", ch);
                    return;
                }
                obj = Handler.get_obj_list(ch, argument, ch.in_room.contents);
                if (obj == null)
                {
                    Comm.send_to_char("You don't see that here.\n\r", ch);
                    return;
                }
                if (obj.item_type != ITEM_FURNITURE
                    || (!Bit.IS_SET(obj.value[2], STAND_AT)
                        && !Bit.IS_SET(obj.value[2], STAND_ON)
                        && !Bit.IS_SET(obj.value[2], STAND_IN)))
                {
                    Comm.send_to_char("You can't seem to find a place to stand.\n\r", ch);
                    return;
                }
                if (ch.on != obj && Handler.count_users(obj) >= obj.value[0])
                {
                    Comm.act_new("There's no room to stand on $p.",
                        ch, obj, null, TO_CHAR, POS_DEAD);
                    return;
                }
                ch.on = obj;
            }

            switch (ch.position)
            {
                case POS_SLEEPING:
                    if (Bit.IS_AFFECTED(ch, AFF_SLEEP))
                    {
                        Comm.send_to_char("You can't wake up!\n\r", ch);
                        return;
                    }

                    if (obj == null)
                    {
                        Comm.send_to_char("You wake and stand up.\n\r", ch);
                        Comm.act("$n wakes and stands up.", ch, null, null, TO_ROOM);
                        ch.on = null;
                    }
                    else if (Bit.IS_SET(obj.value[2], STAND_AT))
                    {
                        Comm.act_new("You wake and stand at $p.", ch, obj, null, TO_CHAR,
                            POS_DEAD);
                        Comm.act("$n wakes and stands at $p.", ch, obj, null, TO_ROOM);
                    }
                    else if (Bit.IS_SET(obj.value[2], STAND_ON))
                    {
                        Comm.act_new("You wake and stand on $p.", ch, obj, null, TO_CHAR,
                            POS_DEAD);
                        Comm.act("$n wakes and stands on $p.", ch, obj, null, TO_ROOM);
                    }
                    else
                    {
                        Comm.act_new("You wake and stand in $p.", ch, obj, null, TO_CHAR,
                            POS_DEAD);
                        Comm.act("$n wakes and stands in $p.", ch, obj, null, TO_ROOM);
                    }
                    ch.position = POS_STANDING;
                    Interp.do_function(ch, Interp.do_look, "auto");
                    break;

                case POS_RESTING:
                case POS_SITTING:
                    if (obj == null)
                    {
                        Comm.send_to_char("You stand up.\n\r", ch);
                        Comm.act("$n stands up.", ch, null, null, TO_ROOM);
                        ch.on = null;
                    }
                    else if (Bit.IS_SET(obj.value[2], STAND_AT))
                    {
                        Comm.act("You stand at $p.", ch, obj, null, TO_CHAR);
                        Comm.act("$n stands at $p.", ch, obj, null, TO_ROOM);
                    }
                    else if (Bit.IS_SET(obj.value[2], STAND_ON))
                    {
                        Comm.act("You stand on $p.", ch, obj, null, TO_CHAR);
                        Comm.act("$n stands on $p.", ch, obj, null, TO_ROOM);
                    }
                    else
                    {
                        Comm.act("You stand in $p.", ch, obj, null, TO_CHAR);
                        Comm.act("$n stands on $p.", ch, obj, null, TO_ROOM);
                    }
                    ch.position = POS_STANDING;
                    break;

                case POS_STANDING:
                    Comm.send_to_char("You are already standing.\n\r", ch);
                    break;

                case POS_FIGHTING:
                    Comm.send_to_char("You are already fighting!\n\r", ch);
                    break;
            }
        }

        public static void do_rest(CharData ch, string argument)
        {
            ObjData obj = null;

            if (ch.position == POS_FIGHTING)
            {
                Comm.send_to_char("You are already fighting!\n\r", ch);
                return;
            }

            if (argument.Length != 0)
            {
                obj = Handler.get_obj_list(ch, argument, ch.in_room.contents);
                if (obj == null)
                {
                    Comm.send_to_char("You don't see that here.\n\r", ch);
                    return;
                }
            }
            else
                obj = ch.on;

            if (obj != null)
            {
                if (obj.item_type != ITEM_FURNITURE
                    || (!Bit.IS_SET(obj.value[2], REST_ON)
                        && !Bit.IS_SET(obj.value[2], REST_IN)
                        && !Bit.IS_SET(obj.value[2], REST_AT)))
                {
                    Comm.send_to_char("You can't rest on that.\n\r", ch);
                    return;
                }

                if (obj != null && ch.on != obj
                    && Handler.count_users(obj) >= obj.value[0])
                {
                    Comm.act_new("There's no more room on $p.", ch, obj, null, TO_CHAR,
                        POS_DEAD);
                    return;
                }

                ch.on = obj;
            }

            switch (ch.position)
            {
                case POS_SLEEPING:
                    if (Bit.IS_AFFECTED(ch, AFF_SLEEP))
                    {
                        Comm.send_to_char("You can't wake up!\n\r", ch);
                        return;
                    }

                    if (obj == null)
                    {
                        Comm.send_to_char("You wake up and start resting.\n\r", ch);
                        Comm.act("$n wakes up and starts resting.", ch, null, null,
                            TO_ROOM);
                    }
                    else if (Bit.IS_SET(obj.value[2], REST_AT))
                    {
                        Comm.act_new("You wake up and rest at $p.",
                            ch, obj, null, TO_CHAR, POS_SLEEPING);
                        Comm.act("$n wakes up and rests at $p.", ch, obj, null, TO_ROOM);
                    }
                    else if (Bit.IS_SET(obj.value[2], REST_ON))
                    {
                        Comm.act_new("You wake up and rest on $p.",
                            ch, obj, null, TO_CHAR, POS_SLEEPING);
                        Comm.act("$n wakes up and rests on $p.", ch, obj, null, TO_ROOM);
                    }
                    else
                    {
                        Comm.act_new("You wake up and rest in $p.",
                            ch, obj, null, TO_CHAR, POS_SLEEPING);
                        Comm.act("$n wakes up and rests in $p.", ch, obj, null, TO_ROOM);
                    }
                    ch.position = POS_RESTING;
                    break;

                case POS_RESTING:
                    Comm.send_to_char("You are already resting.\n\r", ch);
                    break;

                case POS_STANDING:
                    if (obj == null)
                    {
                        Comm.send_to_char("You rest.\n\r", ch);
                        Comm.act("$n sits down and rests.", ch, null, null, TO_ROOM);
                    }
                    else if (Bit.IS_SET(obj.value[2], REST_AT))
                    {
                        Comm.act("You sit down at $p and rest.", ch, obj, null, TO_CHAR);
                        Comm.act("$n sits down at $p and rests.", ch, obj, null, TO_ROOM);
                    }
                    else if (Bit.IS_SET(obj.value[2], REST_ON))
                    {
                        Comm.act("You sit on $p and rest.", ch, obj, null, TO_CHAR);
                        Comm.act("$n sits on $p and rests.", ch, obj, null, TO_ROOM);
                    }
                    else
                    {
                        Comm.act("You rest in $p.", ch, obj, null, TO_CHAR);
                        Comm.act("$n rests in $p.", ch, obj, null, TO_ROOM);
                    }
                    ch.position = POS_RESTING;
                    break;

                case POS_SITTING:
                    if (obj == null)
                    {
                        Comm.send_to_char("You rest.\n\r", ch);
                        Comm.act("$n rests.", ch, null, null, TO_ROOM);
                    }
                    else if (Bit.IS_SET(obj.value[2], REST_AT))
                    {
                        Comm.act("You rest at $p.", ch, obj, null, TO_CHAR);
                        Comm.act("$n rests at $p.", ch, obj, null, TO_ROOM);
                    }
                    else if (Bit.IS_SET(obj.value[2], REST_ON))
                    {
                        Comm.act("You rest on $p.", ch, obj, null, TO_CHAR);
                        Comm.act("$n rests on $p.", ch, obj, null, TO_ROOM);
                    }
                    else
                    {
                        Comm.act("You rest in $p.", ch, obj, null, TO_CHAR);
                        Comm.act("$n rests in $p.", ch, obj, null, TO_ROOM);
                    }
                    ch.position = POS_RESTING;
                    break;
            }
        }

        public static void do_sit(CharData ch, string argument)
        {
            ObjData obj = null;

            if (ch.position == POS_FIGHTING)
            {
                Comm.send_to_char("Maybe you should finish this fight first?\n\r", ch);
                return;
            }

            if (argument.Length != 0)
            {
                obj = Handler.get_obj_list(ch, argument, ch.in_room.contents);
                if (obj == null)
                {
                    Comm.send_to_char("You don't see that here.\n\r", ch);
                    return;
                }
            }
            else
                obj = ch.on;

            if (obj != null)
            {
                if (obj.item_type != ITEM_FURNITURE
                    || (!Bit.IS_SET(obj.value[2], SIT_ON)
                        && !Bit.IS_SET(obj.value[2], SIT_IN)
                        && !Bit.IS_SET(obj.value[2], SIT_AT)))
                {
                    Comm.send_to_char("You can't sit on that.\n\r", ch);
                    return;
                }

                if (obj != null && ch.on != obj
                    && Handler.count_users(obj) >= obj.value[0])
                {
                    Comm.act_new("There's no more room on $p.", ch, obj, null, TO_CHAR,
                        POS_DEAD);
                    return;
                }

                ch.on = obj;
            }
            switch (ch.position)
            {
                case POS_SLEEPING:
                    if (Bit.IS_AFFECTED(ch, AFF_SLEEP))
                    {
                        Comm.send_to_char("You can't wake up!\n\r", ch);
                        return;
                    }

                    if (obj == null)
                    {
                        Comm.send_to_char("You wake and sit up.\n\r", ch);
                        Comm.act("$n wakes and sits up.", ch, null, null, TO_ROOM);
                    }
                    else if (Bit.IS_SET(obj.value[2], SIT_AT))
                    {
                        Comm.act_new("You wake and sit at $p.", ch, obj, null, TO_CHAR,
                            POS_DEAD);
                        Comm.act("$n wakes and sits at $p.", ch, obj, null, TO_ROOM);
                    }
                    else if (Bit.IS_SET(obj.value[2], SIT_ON))
                    {
                        Comm.act_new("You wake and sit on $p.", ch, obj, null, TO_CHAR,
                            POS_DEAD);
                        Comm.act("$n wakes and sits at $p.", ch, obj, null, TO_ROOM);
                    }
                    else
                    {
                        Comm.act_new("You wake and sit in $p.", ch, obj, null, TO_CHAR,
                            POS_DEAD);
                        Comm.act("$n wakes and sits in $p.", ch, obj, null, TO_ROOM);
                    }

                    ch.position = POS_SITTING;
                    break;
                case POS_RESTING:
                    if (obj == null)
                        Comm.send_to_char("You stop resting.\n\r", ch);
                    else if (Bit.IS_SET(obj.value[2], SIT_AT))
                    {
                        Comm.act("You sit at $p.", ch, obj, null, TO_CHAR);
                        Comm.act("$n sits at $p.", ch, obj, null, TO_ROOM);
                    }

                    else if (Bit.IS_SET(obj.value[2], SIT_ON))
                    {
                        Comm.act("You sit on $p.", ch, obj, null, TO_CHAR);
                        Comm.act("$n sits on $p.", ch, obj, null, TO_ROOM);
                    }
                    ch.position = POS_SITTING;
                    break;
                case POS_SITTING:
                    Comm.send_to_char("You are already sitting down.\n\r", ch);
                    break;
                case POS_STANDING:
                    if (obj == null)
                    {
                        Comm.send_to_char("You sit down.\n\r", ch);
                        Comm.act("$n sits down on the ground.", ch, null, null, TO_ROOM);
                    }
                    else if (Bit.IS_SET(obj.value[2], SIT_AT))
                    {
                        Comm.act("You sit down at $p.", ch, obj, null, TO_CHAR);
                        Comm.act("$n sits down at $p.", ch, obj, null, TO_ROOM);
                    }
                    else if (Bit.IS_SET(obj.value[2], SIT_ON))
                    {
                        Comm.act("You sit on $p.", ch, obj, null, TO_CHAR);
                        Comm.act("$n sits on $p.", ch, obj, null, TO_ROOM);
                    }
                    else
                    {
                        Comm.act("You sit down in $p.", ch, obj, null, TO_CHAR);
                        Comm.act("$n sits down in $p.", ch, obj, null, TO_ROOM);
                    }
                    ch.position = POS_SITTING;
                    break;
            }
        }

        public static void do_sleep(CharData ch, string argument)
        {
            ObjData obj = null;

            switch (ch.position)
            {
                case POS_SLEEPING:
                    Comm.send_to_char("You are already sleeping.\n\r", ch);
                    break;

                case POS_RESTING:
                case POS_SITTING:
                case POS_STANDING:
                    if (argument.Length == 0 && ch.on == null)
                    {
                        Comm.send_to_char("You go to sleep.\n\r", ch);
                        Comm.act("$n goes to sleep.", ch, null, null, TO_ROOM);
                        ch.position = POS_SLEEPING;
                    }
                    else
                    {
                        if (argument.Length == 0)
                            obj = ch.on;
                        else
                            obj = Handler.get_obj_list(ch, argument, ch.in_room.contents);

                        if (obj == null)
                        {
                            Comm.send_to_char("You don't see that here.\n\r", ch);
                            return;
                        }
                        if (obj.item_type != ITEM_FURNITURE
                            || (!Bit.IS_SET(obj.value[2], SLEEP_ON)
                                && !Bit.IS_SET(obj.value[2], SLEEP_IN)
                                && !Bit.IS_SET(obj.value[2], SLEEP_AT)))
                        {
                            Comm.send_to_char("You can't sleep on that!\n\r", ch);
                            return;
                        }

                        if (ch.on != obj && Handler.count_users(obj) >= obj.value[0])
                        {
                            Comm.act_new("There is no room on $p for you.",
                                ch, obj, null, TO_CHAR, POS_DEAD);
                            return;
                        }

                        ch.on = obj;
                        if (Bit.IS_SET(obj.value[2], SLEEP_AT))
                        {
                            Comm.act("You go to sleep at $p.", ch, obj, null, TO_CHAR);
                            Comm.act("$n goes to sleep at $p.", ch, obj, null, TO_ROOM);
                        }
                        else if (Bit.IS_SET(obj.value[2], SLEEP_ON))
                        {
                            Comm.act("You go to sleep on $p.", ch, obj, null, TO_CHAR);
                            Comm.act("$n goes to sleep on $p.", ch, obj, null, TO_ROOM);
                        }
                        else
                        {
                            Comm.act("You go to sleep in $p.", ch, obj, null, TO_CHAR);
                            Comm.act("$n goes to sleep in $p.", ch, obj, null, TO_ROOM);
                        }
                        ch.position = POS_SLEEPING;
                    }
                    break;

                case POS_FIGHTING:
                    Comm.send_to_char("You are already fighting!\n\r", ch);
                    break;
            }
        }

        public static void do_wake(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                Interp.do_function(ch, do_stand, "");
                return;
            }

            if (!Bit.IS_AWAKE(ch))
            {
                Comm.send_to_char("You are asleep yourself!\n\r", ch);
                return;
            }

            var victim = Handler.get_char_room(ch, arg);
            if (victim == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }

            if (Bit.IS_AWAKE(victim))
            {
                Comm.act("$N is already awake.", ch, null, victim, TO_CHAR);
                return;
            }

            if (Bit.IS_AFFECTED(victim, AFF_SLEEP))
            {
                Comm.act("You can't wake $M!", ch, null, victim, TO_CHAR);
                return;
            }

            Comm.act_new("$n wakes you.", ch, null, victim, TO_VICT, POS_SLEEPING);
            Interp.do_function(ch, do_stand, "");
        }

        public static void do_sneak(CharData ch, string argument)
        {
            Comm.send_to_char("You attempt to move silently.\n\r", ch);
            Handler.affect_strip(ch, Gsn.sneak);

            if (Bit.IS_AFFECTED(ch, AFF_SNEAK))
                return;

            if (RomRandom.number_percent() < Handler.get_skill(ch, Gsn.sneak))
            {
                Skills.check_improve(ch, Gsn.sneak, true, 3);
                var af = new AffectData();
                af.where = TO_AFFECTS;
                af.type = Gsn.sneak;
                af.level = ch.level;
                af.duration = ch.level;
                af.location = APPLY_NONE;
                af.modifier = 0;
                af.bitvector = AFF_SNEAK;
                Handler.affect_to_char(ch, af);
            }
            else
                Skills.check_improve(ch, Gsn.sneak, false, 3);
        }

        public static void do_hide(CharData ch, string argument)
        {
            Comm.send_to_char("You attempt to hide.\n\r", ch);

            if (Bit.IS_AFFECTED(ch, AFF_HIDE))
                Bit.REMOVE_BIT(ref ch.affected_by, AFF_HIDE);

            if (RomRandom.number_percent() < Handler.get_skill(ch, Gsn.hide))
            {
                Bit.SET_BIT(ref ch.affected_by, AFF_HIDE);
                Skills.check_improve(ch, Gsn.hide, true, 3);
            }
            else
                Skills.check_improve(ch, Gsn.hide, false, 3);
        }

        public static void do_visible(CharData ch, string argument)
        {
            Handler.affect_strip(ch, Gsn.invis);
            Handler.affect_strip(ch, Gsn.mass_invis);
            Handler.affect_strip(ch, Gsn.sneak);
            Bit.REMOVE_BIT(ref ch.affected_by, AFF_HIDE);
            Bit.REMOVE_BIT(ref ch.affected_by, AFF_INVISIBLE);
            Bit.REMOVE_BIT(ref ch.affected_by, AFF_SNEAK);
            Comm.send_to_char("Ok.\n\r", ch);
        }

        public static void do_train(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            CharData mob;
            for (mob = ch.in_room.people; mob != null; mob = mob.next_in_room)
            {
                if (Bit.IS_NPC(mob) && Bit.IS_SET(mob.act, ACT_TRAIN))
                    break;
            }

            if (mob == null)
            {
                Comm.send_to_char("You can't do that here.\n\r", ch);
                return;
            }

            if (argument.Length == 0)
            {
                Comm.send_to_char(RomString.sprintf("You have %d training sessions.\n\r", ch.train), ch);
                argument = "foo";
            }

            int cost = 1;
            int stat = -1;
            string pOutput = null;

            if (!RomString.str_cmp(argument, "str"))
            {
                if (Tables.class_table[ch.klass].attr_prime == STAT_STR)
                    cost = 1;
                stat = STAT_STR;
                pOutput = "strength";
            }
            else if (!RomString.str_cmp(argument, "int"))
            {
                if (Tables.class_table[ch.klass].attr_prime == STAT_INT)
                    cost = 1;
                stat = STAT_INT;
                pOutput = "intelligence";
            }
            else if (!RomString.str_cmp(argument, "wis"))
            {
                if (Tables.class_table[ch.klass].attr_prime == STAT_WIS)
                    cost = 1;
                stat = STAT_WIS;
                pOutput = "wisdom";
            }
            else if (!RomString.str_cmp(argument, "dex"))
            {
                if (Tables.class_table[ch.klass].attr_prime == STAT_DEX)
                    cost = 1;
                stat = STAT_DEX;
                pOutput = "dexterity";
            }
            else if (!RomString.str_cmp(argument, "con"))
            {
                if (Tables.class_table[ch.klass].attr_prime == STAT_CON)
                    cost = 1;
                stat = STAT_CON;
                pOutput = "constitution";
            }
            else if (!RomString.str_cmp(argument, "hp"))
                cost = 1;
            else if (!RomString.str_cmp(argument, "mana"))
                cost = 1;
            else
            {
                string buf = "You can train:";
                if (ch.perm_stat[STAT_STR] < Handler.get_max_train(ch, STAT_STR))
                    buf += " str";
                if (ch.perm_stat[STAT_INT] < Handler.get_max_train(ch, STAT_INT))
                    buf += " int";
                if (ch.perm_stat[STAT_WIS] < Handler.get_max_train(ch, STAT_WIS))
                    buf += " wis";
                if (ch.perm_stat[STAT_DEX] < Handler.get_max_train(ch, STAT_DEX))
                    buf += " dex";
                if (ch.perm_stat[STAT_CON] < Handler.get_max_train(ch, STAT_CON))
                    buf += " con";
                buf += " hp mana";

                if (buf[buf.Length - 1] != ':')
                {
                    buf += ".\n\r";
                    Comm.send_to_char(buf, ch);
                }
                else
                {
                    Comm.act("You have nothing left to train, you $T!",
                        ch, null,
                        ch.sex == SEX_MALE ? "big stud" :
                        ch.sex == SEX_FEMALE ? "hot babe" : "wild thing", TO_CHAR);
                }

                return;
            }

            if (!RomString.str_cmp("hp", argument))
            {
                if (cost > ch.train)
                {
                    Comm.send_to_char("You don't have enough training sessions.\n\r", ch);
                    return;
                }

                ch.train -= cost;
                Gmcp.Worth(ch);
                ch.pcdata.perm_hit += 10;
                ch.max_hit += 10;
                ch.hit += 10;
                Comm.act("Your durability increases!", ch, null, null, TO_CHAR);
                Comm.act("$n's durability increases!", ch, null, null, TO_ROOM);
                return;
            }

            if (!RomString.str_cmp("mana", argument))
            {
                if (cost > ch.train)
                {
                    Comm.send_to_char("You don't have enough training sessions.\n\r", ch);
                    return;
                }

                ch.train -= cost;
                Gmcp.Worth(ch);
                ch.pcdata.perm_mana += 10;
                ch.max_mana += 10;
                ch.mana += 10;
                Comm.act("Your power increases!", ch, null, null, TO_CHAR);
                Comm.act("$n's power increases!", ch, null, null, TO_ROOM);
                return;
            }

            if (ch.perm_stat[stat] >= Handler.get_max_train(ch, stat))
            {
                Comm.act("Your $T is already at maximum.", ch, null, pOutput, TO_CHAR);
                return;
            }

            if (cost > ch.train)
            {
                Comm.send_to_char("You don't have enough training sessions.\n\r", ch);
                return;
            }

            ch.train -= cost;
            ch.perm_stat[stat] += 1;
            Gmcp.Worth(ch);
            Gmcp.Stats(ch);
            Comm.act("Your $T increases!", ch, null, pOutput, TO_CHAR);
            Comm.act("$n's $T increases!", ch, null, pOutput, TO_ROOM);
        }

        public static void do_enter(CharData ch, string argument)
        {
            if (ch.fighting != null)
                return;

            if (argument.Length != 0)
            {
                var old_room = ch.in_room;

                var portal = Handler.get_obj_list(ch, argument, ch.in_room.contents);

                if (portal == null)
                {
                    Comm.send_to_char("You don't see that here.\n\r", ch);
                    return;
                }

                if (portal.item_type != ITEM_PORTAL
                    || (Bit.IS_SET(portal.value[1], EX_CLOSED)
                        && !Bit.IS_TRUSTED(ch, ANGEL)))
                {
                    Comm.send_to_char("You can't seem to find a way in.\n\r", ch);
                    return;
                }

                if (!Bit.IS_TRUSTED(ch, ANGEL)
                    && !Bit.IS_SET(portal.value[2], GATE_NOCURSE)
                    && (Bit.IS_AFFECTED(ch, AFF_CURSE)
                        || Bit.IS_SET(old_room.room_flags, ROOM_NO_RECALL)))
                {
                    Comm.send_to_char("Something prevents you from leaving...\n\r", ch);
                    return;
                }

                RoomIndexData location;
                if (Bit.IS_SET(portal.value[2], GATE_RANDOM) || portal.value[3] == -1)
                {
                    location = Handler.get_random_room(ch);
                    portal.value[3] = location.vnum;
                }
                else if (Bit.IS_SET(portal.value[2], GATE_BUGGY)
                         && (RomRandom.number_percent() < 5)) location = Handler.get_random_room(ch);
                else
                    location = Handler.get_room_index(portal.value[3]);

                if (location == null
                    || location == old_room
                    || !Handler.can_see_room(ch, location)
                    || (Handler.room_is_private(location) && !Bit.IS_TRUSTED(ch, IMPLEMENTOR)))
                {
                    Comm.act("$p doesn't seem to go anywhere.", ch, portal, null,
                        TO_CHAR);
                    return;
                }

                if (Bit.IS_NPC(ch) && Bit.IS_SET(ch.act, ACT_AGGRESSIVE)
                    && Bit.IS_SET(location.room_flags, ROOM_LAW))
                {
                    Comm.send_to_char("Something prevents you from leaving...\n\r", ch);
                    return;
                }

                Comm.act("$n steps into $p.", ch, portal, null, TO_ROOM);

                if (Bit.IS_SET(portal.value[2], GATE_NORMAL_EXIT))
                    Comm.act("You enter $p.", ch, portal, null, TO_CHAR);
                else
                    Comm.act("You walk through $p and find yourself somewhere else...",
                        ch, portal, null, TO_CHAR);

                Handler.char_from_room(ch);
                Handler.char_to_room(ch, location);

                if (Bit.IS_SET(portal.value[2], GATE_GOWITH))
                {
                    Handler.obj_from_room(portal);
                    Handler.obj_to_room(portal, location);
                }

                if (Bit.IS_SET(portal.value[2], GATE_NORMAL_EXIT))
                    Comm.act("$n has arrived.", ch, portal, null, TO_ROOM);
                else
                    Comm.act("$n has arrived through $p.", ch, portal, null, TO_ROOM);

                Interp.do_function(ch, Interp.do_look, "auto");

                if (portal.value[0] > 0)
                {
                    portal.value[0]--;
                    if (portal.value[0] == 0)
                        portal.value[0] = -1;
                }

                if (old_room == location)
                    return;

                for (var fch = old_room.people; fch != null; )
                {
                    var fch_next = fch.next_in_room;

                    if (portal == null || portal.value[0] == -1)
                    {
                        fch = fch_next;
                        continue;
                    }

                    if (fch.master == ch && Bit.IS_AFFECTED(fch, AFF_CHARM)
                        && fch.position < POS_STANDING)
                        Interp.do_function(fch, do_stand, "");

                    if (fch.master == ch && fch.position == POS_STANDING)
                    {
                        if (Bit.IS_SET(ch.in_room.room_flags, ROOM_LAW)
                            && (Bit.IS_NPC(fch) && Bit.IS_SET(fch.act, ACT_AGGRESSIVE)))
                        {
                            Comm.act("You can't bring $N into the city.",
                                ch, null, fch, TO_CHAR);
                            Comm.act("You aren't allowed in the city.",
                                fch, null, null, TO_CHAR);
                            fch = fch_next;
                            continue;
                        }

                        Comm.act("You follow $N.", fch, null, ch, TO_CHAR);
                        Interp.do_function(fch, do_enter, argument);
                    }
                    fch = fch_next;
                }

                if (portal != null && portal.value[0] == -1)
                {
                    Comm.act("$p fades out of existence.", ch, portal, null, TO_CHAR);
                    if (ch.in_room == old_room)
                        Comm.act("$p fades out of existence.", ch, portal, null, TO_ROOM);
                    else if (old_room.people != null)
                    {
                        Comm.act("$p fades out of existence.",
                            old_room.people, portal, null, TO_CHAR);
                        Comm.act("$p fades out of existence.",
                            old_room.people, portal, null, TO_ROOM);
                    }
                    Handler.extract_obj(portal);
                }

                /*
                 * If someone is following the char, these triggers get activated
                 * for the followers before the char, but it's safer this way...
                 */
                if (Bit.IS_NPC(ch) && Bit.HAS_TRIGGER(ch, TRIG_ENTRY))
                    MobProg.mp_percent_trigger(ch, null, null, null, (int)TRIG_ENTRY);
                if (!Bit.IS_NPC(ch))
                    MobProg.mp_greet_trigger(ch);

                return;
            }

            Comm.send_to_char("Nope, can't do it.\n\r", ch);
        }
    }
}
