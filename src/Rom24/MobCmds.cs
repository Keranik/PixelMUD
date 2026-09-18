using static Rom24.Merc;

namespace Rom24
{
    public static class MobCmds
    {
        static readonly (string name, DoFun do_fun)[] mob_cmd_table =
        {
            ("asound", do_mpasound),
            ("gecho", do_mpgecho),
            ("zecho", do_mpzecho),
            ("kill", do_mpkill),
            ("assist", do_mpassist),
            ("junk", do_mpjunk),
            ("echo", do_mpecho),
            ("echoaround", do_mpechoaround),
            ("echoat", do_mpechoat),
            ("mload", do_mpmload),
            ("oload", do_mpoload),
            ("purge", do_mppurge),
            ("goto", do_mpgoto),
            ("at", do_mpat),
            ("transfer", do_mptransfer),
            ("gtransfer", do_mpgtransfer),
            ("otransfer", do_mpotransfer),
            ("force", do_mpforce),
            ("gforce", do_mpgforce),
            ("vforce", do_mpvforce),
            ("cast", do_mpcast),
            ("damage", do_mpdamage),
            ("remember", do_mpremember),
            ("forget", do_mpforget),
            ("delay", do_mpdelay),
            ("cancel", do_mpcancel),
            ("call", do_mpcall),
            ("flee", do_mpflee),
            ("remove", do_mpremove),
            ("", null)
        };

        public static void do_mob(CharData ch, string argument)
        {
            if (ch.desc != null && Handler.get_trust(ch) < MAX_LEVEL)
                return;
            mob_interpret(ch, argument);
        }

        public static void mob_interpret(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string command);

            for (int cmd = 0; mob_cmd_table[cmd].name.Length != 0; cmd++)
            {
                if (command.Length != 0 && command[0] == mob_cmd_table[cmd].name[0]
                    && !RomString.str_prefix(command, mob_cmd_table[cmd].name))
                {
                    mob_cmd_table[cmd].do_fun(ch, argument);
                    return;
                }
            }
            Db.bug(RomString.sprintf("Mob_interpret: invalid cmd from mob %d: '%s'",
                Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0, command), 0);
        }

        public static string mprog_type_to_name(int type)
        {
            switch (type)
            {
                case (int)TRIG_ACT:
                    return "ACT";
                case (int)TRIG_SPEECH:
                    return "SPEECH";
                case (int)TRIG_RANDOM:
                    return "RANDOM";
                case (int)TRIG_FIGHT:
                    return "FIGHT";
                case (int)TRIG_HPCNT:
                    return "HPCNT";
                case (int)TRIG_DEATH:
                    return "DEATH";
                case (int)TRIG_ENTRY:
                    return "ENTRY";
                case (int)TRIG_GREET:
                    return "GREET";
                case (int)TRIG_GRALL:
                    return "GRALL";
                case (int)TRIG_GIVE:
                    return "GIVE";
                case (int)TRIG_BRIBE:
                    return "BRIBE";
                case (int)TRIG_KILL:
                    return "KILL";
                case (int)TRIG_DELAY:
                    return "DELAY";
                case (int)TRIG_SURR:
                    return "SURRENDER";
                case (int)TRIG_EXIT:
                    return "EXIT";
                case (int)TRIG_EXALL:
                    return "EXALL";
                default:
                    return "ERROR";
            }
        }

        public static void do_mpstat(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Mpstat whom?\n\r", ch);
                return;
            }

            var victim = Handler.get_char_world(ch, arg);
            if (victim == null)
            {
                Comm.send_to_char("No such creature.\n\r", ch);
                return;
            }

            if (!Bit.IS_NPC(victim))
            {
                Comm.send_to_char("That is not a mobile.\n\r", ch);
                return;
            }

            if ((victim = Handler.get_char_world(ch, arg)) == null)
            {
                Comm.send_to_char("No such creature visible.\n\r", ch);
                return;
            }

            Comm.send_to_char(RomString.sprintf("Mobile #%-6d [%s]\n\r",
                victim.pIndexData.vnum, victim.short_descr), ch);

            Comm.send_to_char(RomString.sprintf("Delay   %-6d [%s]\n\r",
                victim.mprog_delay,
                victim.mprog_target == null
                    ? "No target" : victim.mprog_target.name), ch);

            if (victim.pIndexData.mprog_flags == 0)
            {
                Comm.send_to_char("[No programs set]\n\r", ch);
                return;
            }

            int i = 0;
            for (var mprg = victim.pIndexData.mprogs; mprg != null; mprg = mprg.next)
            {
                Comm.send_to_char(RomString.sprintf("[%2d] Trigger [%-8s] Program [%4d] Phrase [%s]\n\r",
                    ++i,
                    mprog_type_to_name(mprg.trig_type),
                    mprg.vnum, mprg.trig_phrase), ch);
            }
        }

        public static void do_mpdump(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string buf);
            int vnum = Interp.atoi(buf);
            var mprg = Db.get_mprog_index(vnum);
            if (mprg == null)
            {
                Comm.send_to_char("No such MOBprogram.\n\r", ch);
                return;
            }
            Comm.page_to_char(mprg.code, ch);
        }

        static void do_mpgecho(CharData ch, string argument)
        {
            if (argument.Length == 0)
            {
                Db.bug("MpGEcho: missing argument from vnum %d",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            for (var d = Game.descriptor_list; d != null; d = d.next)
            {
                if (d.connected == CON_PLAYING)
                {
                    if (Bit.IS_IMMORTAL(d.character))
                        Comm.send_to_char("Mob echo> ", d.character);
                    Comm.send_to_char(argument, d.character);
                    Comm.send_to_char("\n\r", d.character);
                }
            }
        }

        static void do_mpzecho(CharData ch, string argument)
        {
            if (argument.Length == 0)
            {
                Db.bug("MpZEcho: missing argument from vnum %d",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            if (ch.in_room == null)
                return;
            for (var d = Game.descriptor_list; d != null; d = d.next)
            {
                if (d.connected == CON_PLAYING
                    && d.character.in_room != null
                    && d.character.in_room.area == ch.in_room.area)
                {
                    if (Bit.IS_IMMORTAL(d.character))
                        Comm.send_to_char("Mob echo> ", d.character);
                    Comm.send_to_char(argument, d.character);
                    Comm.send_to_char("\n\r", d.character);
                }
            }
        }

        static void do_mpasound(CharData ch, string argument)
        {
            if (argument.Length == 0)
                return;
            var was_in_room = ch.in_room;
            for (int door = 0; door < 6; door++)
            {
                var pexit = was_in_room.exit[door];
                if (pexit != null && pexit.to_room != null && pexit.to_room != was_in_room)
                {
                    ch.in_room = pexit.to_room;
                    Game.MOBtrigger = false;
                    Comm.act(argument, ch, null, null, TO_ROOM);
                    Game.MOBtrigger = true;
                }
            }
            ch.in_room = was_in_room;
        }

        static void do_mpkill(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
                return;
            var victim = Handler.get_char_room(ch, arg);
            if (victim == null)
                return;
            if (victim == ch || Bit.IS_NPC(victim) || ch.position == POS_FIGHTING)
                return;
            if (Bit.IS_AFFECTED(ch, AFF_CHARM) && ch.master == victim)
            {
                Db.bug("MpKill - Charmed mob attacking master from vnum %d.",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            Fight.multi_hit(ch, victim, TYPE_UNDEFINED);
        }

        static void do_mpassist(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
                return;
            var victim = Handler.get_char_room(ch, arg);
            if (victim == null)
                return;
            if (victim == ch || ch.fighting != null || victim.fighting == null)
                return;
            Fight.multi_hit(ch, victim.fighting, TYPE_UNDEFINED);
        }

        static void do_mpjunk(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
                return;
            if (RomString.str_cmp(arg, "all") && RomString.str_prefix("all.", arg))
            {
                var obj = Handler.get_obj_wear(ch, arg);
                if (obj != null)
                {
                    Handler.unequip_char(ch, obj);
                    Handler.extract_obj(obj);
                    return;
                }
                obj = Handler.get_obj_carry(ch, arg, ch);
                if (obj == null)
                    return;
                Handler.extract_obj(obj);
            }
            else
            {
                for (var obj = ch.carrying; obj != null; )
                {
                    var obj_next = obj.next_content;
                    if (arg.Length <= 3 || Handler.is_name(arg.Substring(4), obj.name))
                    {
                        if (obj.wear_loc != WEAR_NONE)
                            Handler.unequip_char(ch, obj);
                        Handler.extract_obj(obj);
                    }
                    obj = obj_next;
                }
            }
        }

        static void do_mpechoaround(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
                return;
            var victim = Handler.get_char_room(ch, arg);
            if (victim == null)
                return;
            Comm.act(argument, ch, null, victim, TO_NOTVICT);
        }

        static void do_mpechoat(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);
            if (arg.Length == 0 || argument.Length == 0)
                return;
            var victim = Handler.get_char_room(ch, arg);
            if (victim == null)
                return;
            Comm.act(argument, ch, null, victim, TO_VICT);
        }

        static void do_mpecho(CharData ch, string argument)
        {
            if (argument.Length == 0)
                return;
            Comm.act(argument, ch, null, null, TO_ROOM);
        }

        static void do_mpmload(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);
            if (ch.in_room == null || arg.Length == 0 || !Interp.is_number(arg))
                return;
            int vnum = Interp.atoi(arg);
            var pMobIndex = Handler.get_mob_index(vnum);
            if (pMobIndex == null)
            {
                Db.bug(RomString.sprintf("Mpmload: bad mob index (%d) from mob %d",
                    vnum, Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0), 0);
                return;
            }
            var victim = Db.create_mobile(pMobIndex);
            Handler.char_to_room(victim, ch.in_room);
        }

        static void do_mpoload(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg1);
            argument = RomString.one_argument(argument, out string arg2);
            RomString.one_argument(argument, out string arg3);
            if (arg1.Length == 0 || !Interp.is_number(arg1))
            {
                Db.bug("Mpoload - Bad syntax from vnum %d.",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            int level;
            if (arg2.Length == 0)
                level = Handler.get_trust(ch);
            else
            {
                if (!Interp.is_number(arg2))
                {
                    Db.bug("Mpoload - Bad syntax from vnum %d.",
                        Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                    return;
                }
                level = Interp.atoi(arg2);
                if (level < 0 || level > Handler.get_trust(ch))
                {
                    Db.bug("Mpoload - Bad level from vnum %d.",
                        Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                    return;
                }
            }
            bool fToroom = false, fWear = false;
            if (arg3.Length > 0 && (arg3[0] == 'R' || arg3[0] == 'r'))
                fToroom = true;
            else if (arg3.Length > 0 && (arg3[0] == 'W' || arg3[0] == 'w'))
                fWear = true;
            int ov = Interp.atoi(arg1);
            var pObjIndex = Handler.get_obj_index(ov);
            if (pObjIndex == null)
            {
                Db.bug("Mpoload - Bad vnum arg from vnum %d.",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            var obj = Db.create_object(pObjIndex, level);
            if ((fWear || !fToroom) && Bit.CAN_WEAR(obj, ITEM_TAKE))
            {
                Handler.obj_to_char(obj, ch);
                if (fWear)
                    ActObj.wear_obj(ch, obj, true);
            }
            else
                Handler.obj_to_room(obj, ch.in_room);
        }

        static void do_mppurge(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                for (var victim = ch.in_room.people; victim != null; )
                {
                    var vnext = victim.next_in_room;
                    if (Bit.IS_NPC(victim) && victim != ch
                        && !Bit.IS_SET(victim.act, ACT_NOPURGE))
                        Handler.extract_char(victim, true);
                    victim = vnext;
                }
                for (var obj = ch.in_room.contents; obj != null; )
                {
                    var obj_next = obj.next_content;
                    if (!Bit.IS_SET(obj.extra_flags, ITEM_NOPURGE))
                        Handler.extract_obj(obj);
                    obj = obj_next;
                }
                return;
            }
            var vch = Handler.get_char_room(ch, arg);
            if (vch == null)
            {
                var obj = Handler.get_obj_here(ch, arg);
                if (obj != null)
                    Handler.extract_obj(obj);
                else
                    Db.bug("Mppurge - Bad argument from vnum %d.",
                        Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            if (!Bit.IS_NPC(vch))
            {
                Db.bug("Mppurge - Purging a PC from vnum %d.",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            Handler.extract_char(vch, true);
        }

        static void do_mpgoto(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                Db.bug("Mpgoto - No argument from vnum %d.",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            var location = ActWiz.find_location(ch, arg);
            if (location == null)
            {
                Db.bug("Mpgoto - No such location from vnum %d.",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            if (ch.fighting != null)
                Fight.stop_fighting(ch, true);
            Handler.char_from_room(ch);
            Handler.char_to_room(ch, location);
        }

        static void do_mpat(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);
            if (arg.Length == 0 || argument.Length == 0)
            {
                Db.bug("Mpat - Bad argument from vnum %d.",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            var location = ActWiz.find_location(ch, arg);
            if (location == null)
            {
                Db.bug("Mpat - No such location from vnum %d.",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            var original = ch.in_room;
            var on = ch.on;
            Handler.char_from_room(ch);
            Handler.char_to_room(ch, location);
            Interp.interpret(ch, argument);
            for (var wch = Game.char_list; wch != null; wch = wch.next)
            {
                if (wch == ch)
                {
                    Handler.char_from_room(ch);
                    Handler.char_to_room(ch, original);
                    ch.on = on;
                    break;
                }
            }
        }

        static void do_mptransfer(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg1);
            argument = RomString.one_argument(argument, out string arg2);
            if (arg1.Length == 0)
            {
                Db.bug("Mptransfer - Bad syntax from vnum %d.",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            if (!RomString.str_cmp(arg1, "all"))
            {
                for (var victim = ch.in_room.people; victim != null; )
                {
                    var victim_next = victim.next_in_room;
                    if (!Bit.IS_NPC(victim))
                        do_mptransfer(ch, RomString.sprintf("%s %s", victim.name, arg2));
                    victim = victim_next;
                }
                return;
            }
            RoomIndexData location;
            if (arg2.Length == 0)
                location = ch.in_room;
            else
            {
                location = ActWiz.find_location(ch, arg2);
                if (location == null)
                {
                    Db.bug("Mptransfer - No such location from vnum %d.",
                        Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                    return;
                }
                if (Handler.room_is_private(location))
                    return;
            }
            var vch = Handler.get_char_world(ch, arg1);
            if (vch == null || vch.in_room == null)
                return;
            if (vch.fighting != null)
                Fight.stop_fighting(vch, true);
            Handler.char_from_room(vch);
            Handler.char_to_room(vch, location);
            Interp.do_look(vch, "auto");
        }

        static void do_mpgtransfer(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg1);
            argument = RomString.one_argument(argument, out string arg2);
            if (arg1.Length == 0)
            {
                Db.bug("Mpgtransfer - Bad syntax from vnum %d.",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            var who = Handler.get_char_room(ch, arg1);
            if (who == null)
                return;
            for (var victim = ch.in_room.people; victim != null; )
            {
                var victim_next = victim.next_in_room;
                if (Handler.is_same_group(who, victim))
                    do_mptransfer(ch, RomString.sprintf("%s %s", victim.name, arg2));
                victim = victim_next;
            }
        }

        static void do_mpforce(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);
            if (arg.Length == 0 || argument.Length == 0)
            {
                Db.bug("Mpforce - Bad syntax from vnum %d.",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            if (!RomString.str_cmp(arg, "all"))
            {
                for (var vch = Game.char_list; vch != null; )
                {
                    var vch_next = vch.next;
                    if (vch.in_room == ch.in_room
                        && Handler.get_trust(vch) < Handler.get_trust(ch) && Handler.can_see(ch, vch))
                        Interp.interpret(vch, argument);
                    vch = vch_next;
                }
            }
            else
            {
                var victim = Handler.get_char_room(ch, arg);
                if (victim == null || victim == ch)
                    return;
                Interp.interpret(victim, argument);
            }
        }

        static void do_mpgforce(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);
            if (arg.Length == 0 || argument.Length == 0)
            {
                Db.bug("MpGforce - Bad syntax from vnum %d.",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            var victim = Handler.get_char_room(ch, arg);
            if (victim == null || victim == ch)
                return;
            for (var vch = victim.in_room.people; vch != null; )
            {
                var vch_next = vch.next_in_room;
                if (Handler.is_same_group(victim, vch))
                    Interp.interpret(vch, argument);
                vch = vch_next;
            }
        }

        static void do_mpvforce(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);
            if (arg.Length == 0 || argument.Length == 0)
            {
                Db.bug("MpVforce - Bad syntax from vnum %d.",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            if (!Interp.is_number(arg))
            {
                Db.bug("MpVforce - Non-number argument vnum %d.",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            int vnum = Interp.atoi(arg);
            for (var victim = Game.char_list; victim != null; )
            {
                var victim_next = victim.next;
                if (Bit.IS_NPC(victim) && victim.pIndexData.vnum == vnum
                    && ch != victim && victim.fighting == null)
                    Interp.interpret(victim, argument);
                victim = victim_next;
            }
        }

        static void do_mpcast(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string spell);
            RomString.one_argument(argument, out string target);
            if (spell.Length == 0)
            {
                Db.bug("MpCast - Bad syntax from vnum %d.",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            int sn = Lookup.skill_lookup(spell);
            if (sn < 0)
            {
                Db.bug("MpCast - No such spell from vnum %d.",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            var vch = Handler.get_char_room(ch, target);
            var obj = Handler.get_obj_here(ch, target);
            object victim = null;
            switch (Tables.skill_table[sn].target)
            {
                default:
                    return;
                case TAR_IGNORE:
                    break;
                case TAR_CHAR_OFFENSIVE:
                    if (vch == null || vch == ch)
                        return;
                    victim = vch;
                    break;
                case TAR_CHAR_DEFENSIVE:
                    victim = vch ?? (object)ch;
                    break;
                case TAR_CHAR_SELF:
                    victim = ch;
                    break;
                case TAR_OBJ_CHAR_DEF:
                case TAR_OBJ_CHAR_OFF:
                case TAR_OBJ_INV:
                    if (obj == null)
                        return;
                    victim = obj;
                    break;
            }
            Tables.skill_table[sn].spell_fun(sn, ch.level, ch, victim,
                Tables.skill_table[sn].target);
        }

        static void do_mpdamage(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string target);
            argument = RomString.one_argument(argument, out string min);
            argument = RomString.one_argument(argument, out string max);
            if (target.Length == 0)
            {
                Db.bug("MpDamage - Bad syntax from vnum %d.",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            bool fAll = !RomString.str_cmp(target, "all");
            CharData victim = null;
            if (!fAll && (victim = Handler.get_char_room(ch, target)) == null)
                return;
            if (!Interp.is_number(min))
            {
                Db.bug("MpDamage - Bad damage min vnum %d.",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            if (!Interp.is_number(max))
            {
                Db.bug("MpDamage - Bad damage max vnum %d.",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            int low = Interp.atoi(min);
            int high = Interp.atoi(max);
            RomString.one_argument(argument, out target);
            bool fKill = target.Length != 0;
            if (fAll)
            {
                for (victim = ch.in_room.people; victim != null; )
                {
                    var victim_next = victim.next_in_room;
                    if (victim != ch)
                        Fight.damage(victim, victim,
                            fKill ? RomRandom.number_range(low, high)
                                : Bit.UMIN(victim.hit, RomRandom.number_range(low, high)),
                            TYPE_UNDEFINED, DAM_NONE, false);
                    victim = victim_next;
                }
            }
            else
                Fight.damage(victim, victim,
                    fKill ? RomRandom.number_range(low, high)
                        : Bit.UMIN(victim.hit, RomRandom.number_range(low, high)),
                    TYPE_UNDEFINED, DAM_NONE, false);
        }

        static void do_mpremember(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);
            if (arg.Length != 0)
                ch.mprog_target = Handler.get_char_world(ch, arg);
            else
                Db.bug("MpRemember: missing argument from vnum %d.",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
        }

        static void do_mpforget(CharData ch, string argument)
        {
            ch.mprog_target = null;
        }

        static void do_mpdelay(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);
            if (!Interp.is_number(arg))
            {
                Db.bug("MpDelay: invalid arg from vnum %d.",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            ch.mprog_delay = Interp.atoi(arg);
        }

        static void do_mpcancel(CharData ch, string argument)
        {
            ch.mprog_delay = -1;
        }

        static void do_mpcall(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                Db.bug("MpCall: missing arguments from vnum %d.",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            var prg = Db.get_mprog_index(Interp.atoi(arg));
            if (prg == null)
            {
                Db.bug("MpCall: invalid prog from vnum %d.",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            CharData vch = null;
            ObjData obj1 = null, obj2 = null;
            argument = RomString.one_argument(argument, out arg);
            if (arg.Length != 0) vch = Handler.get_char_room(ch, arg);
            argument = RomString.one_argument(argument, out arg);
            if (arg.Length != 0) obj1 = Handler.get_obj_here(ch, arg);
            argument = RomString.one_argument(argument, out arg);
            if (arg.Length != 0) obj2 = Handler.get_obj_here(ch, arg);
            MobProg.program_flow(prg.vnum, prg.code, ch, vch, obj1, obj2);
        }

        static void do_mpflee(CharData ch, string argument)
        {
            if (ch.fighting != null)
                return;
            var was_in = ch.in_room;
            if (was_in == null)
                return;
            for (int attempt = 0; attempt < 6; attempt++)
            {
                int door = RomRandom.number_door();
                var pexit = was_in.exit[door];
                if (pexit == null || pexit.to_room == null
                    || Bit.IS_SET(pexit.exit_info, EX_CLOSED)
                    || (Bit.IS_NPC(ch) && Bit.IS_SET(pexit.to_room.room_flags, ROOM_NO_MOB)))
                    continue;
                ActMove.move_char(ch, door, false);
                if (ch.in_room != was_in)
                    return;
            }
        }

        static void do_mpotransfer(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                Db.bug("MpOTransfer - Missing argument from vnum %d.",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            RomString.one_argument(argument, out string buf);
            var location = ActWiz.find_location(ch, buf);
            if (location == null)
            {
                Db.bug("MpOTransfer - No such location from vnum %d.",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            var obj = Handler.get_obj_here(ch, arg);
            if (obj == null)
                return;
            if (obj.carried_by == null)
                Handler.obj_from_room(obj);
            else
            {
                if (obj.wear_loc != WEAR_NONE)
                    Handler.unequip_char(ch, obj);
                Handler.obj_from_char(obj);
            }
            Handler.obj_to_room(obj, location);
        }

        static void do_mpremove(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);
            var victim = Handler.get_char_room(ch, arg);
            if (victim == null)
                return;
            RomString.one_argument(argument, out arg);
            bool fAll = false;
            int vnum = 0;
            if (!RomString.str_cmp(arg, "all"))
                fAll = true;
            else if (!Interp.is_number(arg))
            {
                Db.bug("MpRemove: Invalid object from vnum %d.",
                    Bit.IS_NPC(ch) ? ch.pIndexData.vnum : 0);
                return;
            }
            else
                vnum = Interp.atoi(arg);
            for (var obj = victim.carrying; obj != null; )
            {
                var obj_next = obj.next_content;
                if (fAll || obj.pIndexData.vnum == vnum)
                {
                    Handler.unequip_char(ch, obj);
                    Handler.obj_from_char(obj);
                    Handler.extract_obj(obj);
                }
                obj = obj_next;
            }
        }
    }
}
