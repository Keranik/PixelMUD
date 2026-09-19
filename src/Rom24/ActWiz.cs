using static Rom24.Merc;

namespace Rom24
{
    public static class ActWiz
    {
        public static void do_outfit(CharData ch, string argument)
        {
            if (ch.level > 5 || Bit.IS_NPC(ch))
            {
                Comm.send_to_char("Find it yourself!\n\r", ch);
                return;
            }

            var obj = Handler.get_eq_char(ch, WEAR_LIGHT);
            if (obj == null)
            {
                obj = Db.create_object(Handler.get_obj_index(OBJ_VNUM_SCHOOL_BANNER), 0);
                obj.cost = 0;
                Handler.obj_to_char(obj, ch);
                Handler.equip_char(ch, obj, WEAR_LIGHT);
            }

            obj = Handler.get_eq_char(ch, WEAR_BODY);
            if (obj == null)
            {
                obj = Db.create_object(Handler.get_obj_index(OBJ_VNUM_SCHOOL_VEST), 0);
                obj.cost = 0;
                Handler.obj_to_char(obj, ch);
                Handler.equip_char(ch, obj, WEAR_BODY);
            }

            if ((obj = Handler.get_eq_char(ch, WEAR_WIELD)) == null)
            {
                int sn = 0;
                int vnum = OBJ_VNUM_SCHOOL_SWORD;

                for (int i = 0; Tables.weapon_table[i].name != null; i++)
                {
                    int wsn = Tables.weapon_table[i].gsnName == null
                        ? -1 : Lookup.skill_lookup(Tables.weapon_table[i].gsnName);
                    if (wsn >= 0 && ch.pcdata.learned[sn] < ch.pcdata.learned[wsn])
                    {
                        sn = wsn;
                        vnum = Tables.weapon_table[i].vnum;
                    }
                }

                obj = Db.create_object(Handler.get_obj_index(vnum), 0);
                Handler.obj_to_char(obj, ch);
                Handler.equip_char(ch, obj, WEAR_WIELD);
            }

            if (((obj = Handler.get_eq_char(ch, WEAR_WIELD)) == null
                 || !Bit.IS_WEAPON_STAT(obj, WEAPON_TWO_HANDS))
                && (obj = Handler.get_eq_char(ch, WEAR_SHIELD)) == null)
            {
                obj = Db.create_object(Handler.get_obj_index(OBJ_VNUM_SCHOOL_SHIELD), 0);
                obj.cost = 0;
                Handler.obj_to_char(obj, ch);
                Handler.equip_char(ch, obj, WEAR_SHIELD);
            }

            Comm.send_to_char("You have been equipped by Mota.\n\r", ch);
        }

        public static void do_wiznet(CharData ch, string argument)
        {
            if (argument.Length == 0)
            {
                if (Bit.IS_SET(ch.wiznet, WIZ_ON))
                {
                    Comm.send_to_char("Signing off of Wiznet.\n\r", ch);
                    Bit.REMOVE_BIT(ref ch.wiznet, WIZ_ON);
                }
                else
                {
                    Comm.send_to_char("Welcome to Wiznet!\n\r", ch);
                    Bit.SET_BIT(ref ch.wiznet, WIZ_ON);
                }
                return;
            }

            if (!RomString.str_prefix(argument, "on"))
            {
                Comm.send_to_char("Welcome to Wiznet!\n\r", ch);
                Bit.SET_BIT(ref ch.wiznet, WIZ_ON);
                return;
            }

            if (!RomString.str_prefix(argument, "off"))
            {
                Comm.send_to_char("Signing off of Wiznet.\n\r", ch);
                Bit.REMOVE_BIT(ref ch.wiznet, WIZ_ON);
                return;
            }

            if (!RomString.str_prefix(argument, "status"))
            {
                string buf = "";
                if (!Bit.IS_SET(ch.wiznet, WIZ_ON))
                    buf += "off ";
                for (int flag = 0; Tables.wiznet_table[flag].name != null; flag++)
                    if (Bit.IS_SET(ch.wiznet, Tables.wiznet_table[flag].flag))
                    {
                        buf += Tables.wiznet_table[flag].name;
                        buf += " ";
                    }
                buf += "\n\r";
                Comm.send_to_char("Wiznet status:\n\r", ch);
                Comm.send_to_char(buf, ch);
                return;
            }

            if (!RomString.str_prefix(argument, "show"))
            {
                string buf = "";
                for (int flag = 0; Tables.wiznet_table[flag].name != null; flag++)
                {
                    if (Tables.wiznet_table[flag].level <= Handler.get_trust(ch))
                    {
                        buf += Tables.wiznet_table[flag].name;
                        buf += " ";
                    }
                }
                buf += "\n\r";
                Comm.send_to_char("Wiznet options available to you are:\n\r", ch);
                Comm.send_to_char(buf, ch);
                return;
            }

            int f = Lookup.wiznet_lookup(argument);
            if (f == -1 || Handler.get_trust(ch) < Tables.wiznet_table[f].level)
            {
                Comm.send_to_char("No such option.\n\r", ch);
                return;
            }

            if (Bit.IS_SET(ch.wiznet, Tables.wiznet_table[f].flag))
            {
                Comm.send_to_char(RomString.sprintf("You will no longer see %s on wiznet.\n\r",
                    Tables.wiznet_table[f].name), ch);
                Bit.REMOVE_BIT(ref ch.wiznet, Tables.wiznet_table[f].flag);
            }
            else
            {
                Comm.send_to_char(RomString.sprintf("You will now see %s on wiznet.\n\r",
                    Tables.wiznet_table[f].name), ch);
                Bit.SET_BIT(ref ch.wiznet, Tables.wiznet_table[f].flag);
            }
        }

        public static void do_guild(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg1);
            argument = RomString.one_argument(argument, out string arg2);

            if (arg1.Length == 0 || arg2.Length == 0)
            {
                Comm.send_to_char("Syntax: guild <char> <cln name>\n\r", ch);
                return;
            }
            var victim = Handler.get_char_world(ch, arg1);
            if (victim == null)
            {
                Comm.send_to_char("They aren't playing.\n\r", ch);
                return;
            }

            if (!RomString.str_prefix(arg2, "none"))
            {
                Comm.send_to_char("They are now clanless.\n\r", ch);
                Comm.send_to_char("You are now a member of no clan!\n\r", victim);
                victim.clan = 0;
                return;
            }

            int clan = Lookup.clan_lookup(arg2);
            if (clan == 0)
            {
                Comm.send_to_char("No such clan exists.\n\r", ch);
                return;
            }

            if (Tables.clan_table[clan].independent)
            {
                Comm.send_to_char(RomString.sprintf("They are now a %s.\n\r",
                    Tables.clan_table[clan].name), ch);
                Comm.send_to_char(RomString.sprintf("You are now a %s.\n\r",
                    Tables.clan_table[clan].name), victim);
            }
            else
            {
                Comm.send_to_char(RomString.sprintf("They are now a member of clan %s.\n\r",
                    RomString.capitalize(Tables.clan_table[clan].name)), ch);
            }

            victim.clan = clan;
        }

        public static void do_smote(CharData ch, string argument)
        {
            if (!Bit.IS_NPC(ch) && Bit.IS_SET(ch.comm, COMM_NOEMOTE))
            {
                Comm.send_to_char("You can't show your emotions.\n\r", ch);
                return;
            }

            if (argument.Length == 0)
            {
                Comm.send_to_char("Emote what?\n\r", ch);
                return;
            }

            if (argument.IndexOf(ch.name, StringComparison.Ordinal) < 0)
            {
                Comm.send_to_char("You must include your name in an smote.\n\r", ch);
                return;
            }

            Comm.send_to_char(argument, ch);
            Comm.send_to_char("\n\r", ch);

            if (ch.in_room == null) return;
            int matches = 0;
            for (var vch = ch.in_room.people; vch != null; vch = vch.next_in_room)
            {
                if (vch.desc == null || vch == ch)
                    continue;

                int letterIdx = argument.IndexOf(vch.name ?? "", StringComparison.Ordinal);
                if (letterIdx < 0)
                {
                    Comm.send_to_char(argument, vch);
                    Comm.send_to_char("\n\r", vch);
                    continue;
                }

                string temp = argument.Substring(0, letterIdx);
                string last = "";
                int nameIdx = 0;
                for (int li = letterIdx; li < argument.Length; li++)
                {
                    char letter = argument[li];
                    if (letter == '\'' && matches == vch.name.Length)
                    {
                        temp += "r";
                        continue;
                    }
                    if (letter == 's' && matches == vch.name.Length)
                    {
                        matches = 0;
                        continue;
                    }
                    if (matches == vch.name.Length)
                        matches = 0;
                    if (nameIdx < vch.name.Length && letter == vch.name[nameIdx])
                    {
                        matches++;
                        nameIdx++;
                        if (matches == vch.name.Length)
                        {
                            temp += "you";
                            last = "";
                            nameIdx = 0;
                            continue;
                        }
                        last += letter;
                        continue;
                    }
                    matches = 0;
                    temp += last;
                    temp += letter;
                    last = "";
                    nameIdx = 0;
                }
                Comm.send_to_char(temp, vch);
                Comm.send_to_char("\n\r", vch);
            }
        }

        public static void do_bamfin(CharData ch, string argument)
        {
            if (!Bit.IS_NPC(ch))
            {
                argument = RomString.smash_tilde(argument);
                if (argument.Length == 0)
                {
                    Comm.send_to_char(RomString.sprintf("Your poofin is %s\n\r",
                        ch.pcdata.bamfin), ch);
                    return;
                }
                if (argument.IndexOf(ch.name, StringComparison.Ordinal) < 0)
                {
                    Comm.send_to_char("You must include your name.\n\r", ch);
                    return;
                }
                ch.pcdata.bamfin = argument;
                Comm.send_to_char(RomString.sprintf("Your poofin is now %s\n\r",
                    ch.pcdata.bamfin), ch);
            }
        }

        public static void do_bamfout(CharData ch, string argument)
        {
            if (!Bit.IS_NPC(ch))
            {
                argument = RomString.smash_tilde(argument);
                if (argument.Length == 0)
                {
                    Comm.send_to_char(RomString.sprintf("Your poofout is %s\n\r",
                        ch.pcdata.bamfout), ch);
                    return;
                }
                if (argument.IndexOf(ch.name, StringComparison.Ordinal) < 0)
                {
                    Comm.send_to_char("You must include your name.\n\r", ch);
                    return;
                }
                ch.pcdata.bamfout = argument;
                Comm.send_to_char(RomString.sprintf("Your poofout is now %s\n\r",
                    ch.pcdata.bamfout), ch);
            }
        }

        public static void do_peace(CharData ch, string argument)
        {
            if (ch.in_room != null)
            {
                for (var rch = ch.in_room.people; rch != null; rch = rch.next_in_room)
                {
                    if (rch.fighting != null)
                        Fight.stop_fighting(rch, true);
                    if (Bit.IS_NPC(rch) && Bit.IS_SET(rch.act, ACT_AGGRESSIVE))
                        Bit.REMOVE_BIT(ref rch.act, ACT_AGGRESSIVE);
                }
            }
            Comm.send_to_char("Ok.\n\r", ch);
        }

        public static void do_wizlock(CharData ch, string argument)
        {
            Game.wizlock = !Game.wizlock;
            if (Game.wizlock)
            {
                Comm.wiznet("$N has wizlocked the game.", ch, null, 0, 0, 0);
                Comm.send_to_char("Game wizlocked.\n\r", ch);
            }
            else
            {
                Comm.wiznet("$N removes wizlock.", ch, null, 0, 0, 0);
                Comm.send_to_char("Game un-wizlocked.\n\r", ch);
            }
        }

        public static void do_newlock(CharData ch, string argument)
        {
            Game.newlock = !Game.newlock;
            if (Game.newlock)
            {
                Comm.wiznet("$N locks out new characters.", ch, null, 0, 0, 0);
                Comm.send_to_char("New characters have been locked out.\n\r", ch);
            }
            else
            {
                Comm.wiznet("$N allows new characters back in.", ch, null, 0, 0, 0);
                Comm.send_to_char("Newlock removed.\n\r", ch);
            }
        }

        public static void do_invis(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                if (ch.invis_level != 0)
                {
                    ch.invis_level = 0;
                    Comm.act("$n slowly fades into existence.", ch, null, null, TO_ROOM);
                    Comm.send_to_char("You slowly fade back into existence.\n\r", ch);
                }
                else
                {
                    ch.invis_level = Handler.get_trust(ch);
                    Comm.act("$n slowly fades into thin air.", ch, null, null, TO_ROOM);
                    Comm.send_to_char("You slowly vanish into thin air.\n\r", ch);
                }
            }
            else
            {
                int level = atoi(arg);
                if (level < 2 || level > Handler.get_trust(ch))
                {
                    Comm.send_to_char("Invis level must be between 2 and your level.\n\r",
                        ch);
                    return;
                }
                ch.reply = null;
                ch.invis_level = level;
                Comm.act("$n slowly fades into thin air.", ch, null, null, TO_ROOM);
                Comm.send_to_char("You slowly vanish into thin air.\n\r", ch);
            }
        }

        public static void do_incognito(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                if (ch.incog_level != 0)
                {
                    ch.incog_level = 0;
                    Comm.act("$n is no longer cloaked.", ch, null, null, TO_ROOM);
                    Comm.send_to_char("You are no longer cloaked.\n\r", ch);
                }
                else
                {
                    ch.incog_level = Handler.get_trust(ch);
                    Comm.act("$n cloaks $s presence.", ch, null, null, TO_ROOM);
                    Comm.send_to_char("You cloak your presence.\n\r", ch);
                }
            }
            else
            {
                int level = atoi(arg);
                if (level < 2 || level > Handler.get_trust(ch))
                {
                    Comm.send_to_char("Incog level must be between 2 and your level.\n\r",
                        ch);
                    return;
                }
                ch.reply = null;
                ch.incog_level = level;
                Comm.act("$n cloaks $s presence.", ch, null, null, TO_ROOM);
                Comm.send_to_char("You cloak your presence.\n\r", ch);
            }
        }

        public static void do_holylight(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            if (Bit.IS_SET(ch.act, PLR_HOLYLIGHT))
            {
                Bit.REMOVE_BIT(ref ch.act, PLR_HOLYLIGHT);
                Comm.send_to_char("Holy light mode off.\n\r", ch);
            }
            else
            {
                Bit.SET_BIT(ref ch.act, PLR_HOLYLIGHT);
                Comm.send_to_char("Holy light mode on.\n\r", ch);
            }
        }

        public static void do_prefi(CharData ch, string argument)
        {
            Comm.send_to_char("You cannot abbreviate the prefix command.\r\n", ch);
        }

        public static void do_prefix(CharData ch, string argument)
        {
            if (argument.Length == 0)
            {
                if (ch.prefix.Length == 0)
                {
                    Comm.send_to_char("You have no prefix to clear.\r\n", ch);
                    return;
                }

                Comm.send_to_char("Prefix removed.\r\n", ch);
                ch.prefix = "";
                return;
            }

            if (ch.prefix.Length != 0)
            {
                RomString.sprintf("Prefix changed to %s.\r\n", argument);
            }
            else
            {
                RomString.sprintf("Prefix set to %s.\r\n", argument);
            }

            ch.prefix = argument;
        }

        public static RoomIndexData find_location(CharData ch, string arg)
        {
            if (Interp.is_number(arg))
            {
                int vnum = atoi(arg);
                return Handler.get_room_index(vnum);
            }
            var victim = Handler.get_char_world(ch, arg);
            if (victim != null)
                return victim.in_room;
            var obj = Handler.get_obj_world(ch, arg);
            if (obj != null)
                return obj.in_room;
            return null;
        }

        public static void do_deny(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                Comm.send_to_char("Deny whom?\n\r", ch);
                return;
            }
            var victim = Handler.get_char_world(ch, arg);
            if (victim == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }
            if (Bit.IS_NPC(victim))
            {
                Comm.send_to_char("Not on NPC's.\n\r", ch);
                return;
            }
            if (Handler.get_trust(victim) >= Handler.get_trust(ch))
            {
                Comm.send_to_char("You failed.\n\r", ch);
                return;
            }
            Bit.SET_BIT(ref victim.act, PLR_DENY);
            Comm.send_to_char("You are denied access!\n\r", victim);
            Comm.wiznet(RomString.sprintf("$N denies access to %s", victim.name),
                ch, null, WIZ_PENALTIES, WIZ_SECURE, 0);
            Comm.send_to_char("OK.\n\r", ch);
            Save.save_char_obj(victim);
            Fight.stop_fighting(victim, true);
            Interp.do_function(victim, Interp.do_quit, "");
        }

        public static void do_disconnect(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                Comm.send_to_char("Disconnect whom?\n\r", ch);
                return;
            }
            if (Interp.is_number(arg))
            {
                int desc = atoi(arg);
                for (var d = Game.descriptor_list; d != null; d = d.next)
                {
                    if (d.descriptor == desc)
                    {
                        Comm.close_socket(d);
                        Comm.send_to_char("Ok.\n\r", ch);
                        return;
                    }
                }
            }
            var victim = Handler.get_char_world(ch, arg);
            if (victim == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }
            if (victim.desc == null)
            {
                Comm.act("$N doesn't have a descriptor.", ch, null, victim, TO_CHAR);
                return;
            }
            for (var d = Game.descriptor_list; d != null; d = d.next)
            {
                if (d == victim.desc)
                {
                    Comm.close_socket(d);
                    Comm.send_to_char("Ok.\n\r", ch);
                    return;
                }
            }
            Db.bug("Do_disconnect: desc not found.", 0);
            Comm.send_to_char("Descriptor not found!\n\r", ch);
        }

        public static void do_pardon(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg1);
            argument = RomString.one_argument(argument, out string arg2);
            if (arg1.Length == 0 || arg2.Length == 0)
            {
                Comm.send_to_char("Syntax: pardon <character> <killer|thief>.\n\r", ch);
                return;
            }
            var victim = Handler.get_char_world(ch, arg1);
            if (victim == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }
            if (Bit.IS_NPC(victim))
            {
                Comm.send_to_char("Not on NPC's.\n\r", ch);
                return;
            }
            if (!RomString.str_cmp(arg2, "killer"))
            {
                if (Bit.IS_SET(victim.act, PLR_KILLER))
                {
                    Bit.REMOVE_BIT(ref victim.act, PLR_KILLER);
                    Comm.send_to_char("Killer flag removed.\n\r", ch);
                    Comm.send_to_char("You are no longer a KILLER.\n\r", victim);
                }
                return;
            }
            if (!RomString.str_cmp(arg2, "thief"))
            {
                if (Bit.IS_SET(victim.act, PLR_THIEF))
                {
                    Bit.REMOVE_BIT(ref victim.act, PLR_THIEF);
                    Comm.send_to_char("Thief flag removed.\n\r", ch);
                    Comm.send_to_char("You are no longer a THIEF.\n\r", victim);
                }
                return;
            }
            Comm.send_to_char("Syntax: pardon <character> <killer|thief>.\n\r", ch);
        }

        public static void do_echo(CharData ch, string argument)
        {
            if (argument.Length == 0)
            {
                Comm.send_to_char("Global echo what?\n\r", ch);
                return;
            }
            for (var d = Game.descriptor_list; d != null; d = d.next)
            {
                if (d.connected == CON_PLAYING)
                {
                    if (Handler.get_trust(d.character) >= Handler.get_trust(ch))
                        Comm.send_to_char("global> ", d.character);
                    Comm.send_to_char(argument, d.character);
                    Comm.send_to_char("\n\r", d.character);
                }
            }
        }

        public static void do_recho(CharData ch, string argument)
        {
            if (argument.Length == 0)
            {
                Comm.send_to_char("Local echo what?\n\r", ch);
                return;
            }
            for (var d = Game.descriptor_list; d != null; d = d.next)
            {
                if (d.connected == CON_PLAYING
                    && d.character.in_room == ch.in_room)
                {
                    if (Handler.get_trust(d.character) >= Handler.get_trust(ch))
                        Comm.send_to_char("local> ", d.character);
                    Comm.send_to_char(argument, d.character);
                    Comm.send_to_char("\n\r", d.character);
                }
            }
        }

        public static void do_zecho(CharData ch, string argument)
        {
            if (argument.Length == 0)
            {
                Comm.send_to_char("Zone echo what?\n\r", ch);
                return;
            }
            for (var d = Game.descriptor_list; d != null; d = d.next)
            {
                if (d.connected == CON_PLAYING
                    && d.character.in_room != null && ch.in_room != null
                    && d.character.in_room.area == ch.in_room.area)
                {
                    if (Handler.get_trust(d.character) >= Handler.get_trust(ch))
                        Comm.send_to_char("zone> ", d.character);
                    Comm.send_to_char(argument, d.character);
                    Comm.send_to_char("\n\r", d.character);
                }
            }
        }

        public static void do_pecho(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);
            if (argument.Length == 0 || arg.Length == 0)
            {
                Comm.send_to_char("Personal echo what?\n\r", ch);
                return;
            }
            var victim = Handler.get_char_world(ch, arg);
            if (victim == null)
            {
                Comm.send_to_char("Target not found.\n\r", ch);
                return;
            }
            if (Handler.get_trust(victim) >= Handler.get_trust(ch) && Handler.get_trust(ch) != MAX_LEVEL)
                Comm.send_to_char("personal> ", victim);
            Comm.send_to_char(argument, victim);
            Comm.send_to_char("\n\r", victim);
            Comm.send_to_char("personal> ", ch);
            Comm.send_to_char(argument, ch);
            Comm.send_to_char("\n\r", ch);
        }

        public static void do_transfer(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg1);
            argument = RomString.one_argument(argument, out string arg2);
            if (arg1.Length == 0)
            {
                Comm.send_to_char("Transfer whom (and where)?\n\r", ch);
                return;
            }
            if (!RomString.str_cmp(arg1, "all"))
            {
                for (var d = Game.descriptor_list; d != null; d = d.next)
                {
                    if (d.connected == CON_PLAYING
                        && d.character != ch
                        && d.character.in_room != null
                        && Handler.can_see(ch, d.character))
                    {
                        Interp.do_function(ch, do_transfer,
                            RomString.sprintf("%s %s", d.character.name, arg2));
                    }
                }
                return;
            }
            RoomIndexData location;
            if (arg2.Length == 0)
                location = ch.in_room;
            else
            {
                location = find_location(ch, arg2);
                if (location == null)
                {
                    Comm.send_to_char("No such location.\n\r", ch);
                    return;
                }
                if (!Handler.is_room_owner(ch, location) && Handler.room_is_private(location)
                    && Handler.get_trust(ch) < MAX_LEVEL)
                {
                    Comm.send_to_char("That room is private right now.\n\r", ch);
                    return;
                }
            }
            var victim = Handler.get_char_world(ch, arg1);
            if (victim == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }
            if (victim.in_room == null)
            {
                Comm.send_to_char("They are in limbo.\n\r", ch);
                return;
            }
            if (victim.fighting != null)
                Fight.stop_fighting(victim, true);
            Comm.act("$n disappears in a mushroom cloud.", victim, null, null, TO_ROOM);
            Handler.char_from_room(victim);
            Handler.char_to_room(victim, location);
            Comm.act("$n arrives from a puff of smoke.", victim, null, null, TO_ROOM);
            if (ch != victim)
                Comm.act("$n has transferred you.", ch, null, victim, TO_VICT);
            Interp.do_function(victim, Interp.do_look, "auto");
            Comm.send_to_char("Ok.\n\r", ch);
        }

        public static void do_at(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);
            if (arg.Length == 0 || argument.Length == 0)
            {
                Comm.send_to_char("At where what?\n\r", ch);
                return;
            }
            var location = find_location(ch, arg);
            if (location == null)
            {
                Comm.send_to_char("No such location.\n\r", ch);
                return;
            }
            if (!Handler.is_room_owner(ch, location) && Handler.room_is_private(location)
                && Handler.get_trust(ch) < MAX_LEVEL)
            {
                Comm.send_to_char("That room is private right now.\n\r", ch);
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

        public static void do_goto(CharData ch, string argument)
        {
            if (argument.Length == 0)
            {
                Comm.send_to_char("Goto where?\n\r", ch);
                return;
            }
            var location = find_location(ch, argument);
            if (location == null)
            {
                Comm.send_to_char("No such location.\n\r", ch);
                return;
            }
            int count = 0;
            for (var rch = location.people; rch != null; rch = rch.next_in_room)
                count++;
            if (!Handler.is_room_owner(ch, location) && Handler.room_is_private(location)
                && (count > 1 || Handler.get_trust(ch) < MAX_LEVEL))
            {
                Comm.send_to_char("That room is private right now.\n\r", ch);
                return;
            }
            if (ch.fighting != null)
                Fight.stop_fighting(ch, true);
            if (ch.in_room != null)
            {
                for (var rch = ch.in_room.people; rch != null; rch = rch.next_in_room)
                {
                    if (Handler.get_trust(rch) >= ch.invis_level)
                    {
                        if (ch.pcdata != null && ch.pcdata.bamfout.Length != 0)
                            Comm.act("$t", ch, ch.pcdata.bamfout, rch, TO_VICT);
                        else
                            Comm.act("$n leaves in a swirling mist.", ch, null, rch, TO_VICT);
                    }
                }
                Handler.char_from_room(ch);
            }
            Handler.char_to_room(ch, location);
            for (var rch = ch.in_room.people; rch != null; rch = rch.next_in_room)
            {
                if (Handler.get_trust(rch) >= ch.invis_level)
                {
                    if (ch.pcdata != null && ch.pcdata.bamfin.Length != 0)
                        Comm.act("$t", ch, ch.pcdata.bamfin, rch, TO_VICT);
                    else
                        Comm.act("$n appears in a swirling mist.", ch, null, rch, TO_VICT);
                }
            }
            Interp.do_function(ch, Interp.do_look, "auto");
        }

        public static void do_violate(CharData ch, string argument)
        {
            if (argument.Length == 0)
            {
                Comm.send_to_char("Goto where?\n\r", ch);
                return;
            }
            var location = find_location(ch, argument);
            if (location == null)
            {
                Comm.send_to_char("No such location.\n\r", ch);
                return;
            }
            if (!Handler.room_is_private(location))
            {
                Comm.send_to_char("That room isn't private, use goto.\n\r", ch);
                return;
            }
            if (ch.fighting != null)
                Fight.stop_fighting(ch, true);
            if (ch.in_room != null)
            {
                for (var rch = ch.in_room.people; rch != null; rch = rch.next_in_room)
                {
                    if (Handler.get_trust(rch) >= ch.invis_level)
                    {
                        if (ch.pcdata != null && ch.pcdata.bamfout.Length != 0)
                            Comm.act("$t", ch, ch.pcdata.bamfout, rch, TO_VICT);
                        else
                            Comm.act("$n leaves in a swirling mist.", ch, null, rch, TO_VICT);
                    }
                }
                Handler.char_from_room(ch);
            }
            Handler.char_to_room(ch, location);
            for (var rch = ch.in_room.people; rch != null; rch = rch.next_in_room)
            {
                if (Handler.get_trust(rch) >= ch.invis_level)
                {
                    if (ch.pcdata != null && ch.pcdata.bamfin.Length != 0)
                        Comm.act("$t", ch, ch.pcdata.bamfin, rch, TO_VICT);
                    else
                        Comm.act("$n appears in a swirling mist.", ch, null, rch, TO_VICT);
                }
            }
            Interp.do_function(ch, Interp.do_look, "auto");
        }

        public static void do_stat(CharData ch, string argument)
        {
            string rest = RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                Comm.send_to_char("Syntax:\n\r", ch);
                Comm.send_to_char("  stat <name>\n\r", ch);
                Comm.send_to_char("  stat obj <name>\n\r", ch);
                Comm.send_to_char("  stat mob <name>\n\r", ch);
                Comm.send_to_char("  stat room <number>\n\r", ch);
                return;
            }
            if (!RomString.str_cmp(arg, "room"))
            {
                Interp.do_function(ch, do_rstat, rest);
                return;
            }
            if (!RomString.str_cmp(arg, "obj"))
            {
                Interp.do_function(ch, do_ostat, rest);
                return;
            }
            if (!RomString.str_cmp(arg, "char") || !RomString.str_cmp(arg, "mob"))
            {
                Interp.do_function(ch, do_mstat, rest);
                return;
            }
            if (Handler.get_obj_world(ch, argument) != null)
            {
                Interp.do_function(ch, do_ostat, argument);
                return;
            }
            if (Handler.get_char_world(ch, argument) != null)
            {
                Interp.do_function(ch, do_mstat, argument);
                return;
            }
            if (find_location(ch, argument) != null)
            {
                Interp.do_function(ch, do_rstat, argument);
                return;
            }
            Comm.send_to_char("Nothing by that name found anywhere.\n\r", ch);
        }

        public static void do_rstat(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);
            var location = arg.Length == 0 ? ch.in_room : find_location(ch, arg);
            if (location == null)
            {
                Comm.send_to_char("No such location.\n\r", ch);
                return;
            }
            if (!Handler.is_room_owner(ch, location) && ch.in_room != location
                && Handler.room_is_private(location) && !Bit.IS_TRUSTED(ch, IMPLEMENTOR))
            {
                Comm.send_to_char("That room is private right now.\n\r", ch);
                return;
            }
            Comm.send_to_char(RomString.sprintf("Name: '%s'\n\rArea: '%s'\n\r",
                location.name, location.area.name), ch);
            Comm.send_to_char(RomString.sprintf(
                "Vnum: %d  Sector: %d  Light: %d  Healing: %d  Mana: %d\n\r",
                location.vnum, location.sector_type, location.light,
                location.heal_rate, location.mana_rate), ch);
            Comm.send_to_char(RomString.sprintf(
                "Room flags: %d.\n\rDescription:\n\r%s",
                location.room_flags, location.description), ch);
            if (location.extra_descr != null)
            {
                Comm.send_to_char("Extra description keywords: '", ch);
                for (var ed = location.extra_descr; ed != null; ed = ed.next)
                {
                    Comm.send_to_char(ed.keyword, ch);
                    if (ed.next != null)
                        Comm.send_to_char(" ", ch);
                }
                Comm.send_to_char("'.\n\r", ch);
            }
            Comm.send_to_char("Characters:", ch);
            for (var rch = location.people; rch != null; rch = rch.next_in_room)
            {
                if (Handler.can_see(ch, rch))
                {
                    Comm.send_to_char(" ", ch);
                    RomString.one_argument(rch.name, out string nm);
                    Comm.send_to_char(nm, ch);
                }
            }
            Comm.send_to_char(".\n\rObjects:   ", ch);
            for (var obj = location.contents; obj != null; obj = obj.next_content)
            {
                Comm.send_to_char(" ", ch);
                RomString.one_argument(obj.name, out string on);
                Comm.send_to_char(on, ch);
            }
            Comm.send_to_char(".\n\r", ch);
            for (int door = 0; door <= 5; door++)
            {
                var pexit = location.exit[door];
                if (pexit != null)
                {
                    Comm.send_to_char(RomString.sprintf(
                        "Door: %d.  To: %d.  Key: %d.  Exit flags: %d.\n\rKeyword: '%s'.  Description: %s",
                        door,
                        pexit.to_room == null ? -1 : pexit.to_room.vnum, pexit.key,
                        pexit.exit_info, pexit.keyword,
                        pexit.description.Length != 0 ? pexit.description : "(none).\n\r"), ch);
                }
            }
        }

        public static void do_ostat(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                Comm.send_to_char("Stat what?\n\r", ch);
                return;
            }
            var obj = Handler.get_obj_world(ch, argument);
            if (obj == null)
            {
                Comm.send_to_char("Nothing like that in hell, earth, or heaven.\n\r", ch);
                return;
            }
            Comm.send_to_char(RomString.sprintf("Name(s): %s\n\r", obj.name), ch);
            Comm.send_to_char(RomString.sprintf("Vnum: %d  Format: %s  Type: %s  Resets: %d\n\r",
                obj.pIndexData.vnum,
                obj.pIndexData.new_format ? "new" : "old",
                Handler.item_name(obj.item_type), obj.pIndexData.reset_num), ch);
            Comm.send_to_char(RomString.sprintf("Short description: %s\n\rLong description: %s\n\r",
                obj.short_descr, obj.description), ch);
            Comm.send_to_char(RomString.sprintf("Wear bits: %s\n\rExtra bits: %s\n\r",
                Handler.wear_bit_name(obj.wear_flags),
                Handler.extra_bit_name(obj.extra_flags)), ch);
            Comm.send_to_char(RomString.sprintf("Number: %d/%d  Weight: %d/%d/%d (10th pounds)\n\r",
                1, Handler.get_obj_number(obj),
                obj.weight, Handler.get_obj_weight(obj), Handler.get_true_weight(obj)), ch);
            Comm.send_to_char(RomString.sprintf("Level: %d  Cost: %d  Condition: %d  Timer: %d\n\r",
                obj.level, obj.cost, obj.condition, obj.timer), ch);
            Comm.send_to_char(RomString.sprintf(
                "In room: %d  In object: %s  Carried by: %s  Wear_loc: %d\n\r",
                obj.in_room == null ? 0 : obj.in_room.vnum,
                obj.in_obj == null ? "(none)" : obj.in_obj.short_descr,
                obj.carried_by == null ? "(none)" :
                Handler.can_see(ch, obj.carried_by) ? obj.carried_by.name : "someone",
                obj.wear_loc), ch);
            Comm.send_to_char(RomString.sprintf("Values: %d %d %d %d %d\n\r",
                obj.value[0], obj.value[1], obj.value[2], obj.value[3], obj.value[4]), ch);

            switch (obj.item_type)
            {
                case ITEM_SCROLL:
                case ITEM_POTION:
                case ITEM_PILL:
                    Comm.send_to_char(RomString.sprintf("Level %d spells of:", obj.value[0]), ch);

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
                    Comm.send_to_char(RomString.sprintf("Has %d(%d) charges of level %d",
                        obj.value[1], obj.value[2], obj.value[0]), ch);

                    if (obj.value[3] >= 0 && obj.value[3] < MAX_SKILL)
                    {
                        Comm.send_to_char(" '", ch);
                        Comm.send_to_char(Tables.skill_table[obj.value[3]].name, ch);
                        Comm.send_to_char("'", ch);
                    }

                    Comm.send_to_char(".\n\r", ch);
                    break;

                case ITEM_DRINK_CON:
                    Comm.send_to_char(RomString.sprintf("It holds %s-colored %s.\n\r",
                        Tables.liq_table[obj.value[2]].liq_color,
                        Tables.liq_table[obj.value[2]].liq_name), ch);
                    break;

                case ITEM_WEAPON:
                    Comm.send_to_char("Weapon type is ", ch);
                    switch (obj.value[0])
                    {
                        case WEAPON_EXOTIC:
                            Comm.send_to_char("exotic\n\r", ch);
                            break;
                        case WEAPON_SWORD:
                            Comm.send_to_char("sword\n\r", ch);
                            break;
                        case WEAPON_DAGGER:
                            Comm.send_to_char("dagger\n\r", ch);
                            break;
                        case WEAPON_SPEAR:
                            Comm.send_to_char("spear/staff\n\r", ch);
                            break;
                        case WEAPON_MACE:
                            Comm.send_to_char("mace/club\n\r", ch);
                            break;
                        case WEAPON_AXE:
                            Comm.send_to_char("axe\n\r", ch);
                            break;
                        case WEAPON_FLAIL:
                            Comm.send_to_char("flail\n\r", ch);
                            break;
                        case WEAPON_WHIP:
                            Comm.send_to_char("whip\n\r", ch);
                            break;
                        case WEAPON_POLEARM:
                            Comm.send_to_char("polearm\n\r", ch);
                            break;
                        default:
                            Comm.send_to_char("unknown\n\r", ch);
                            break;
                    }
                    if (obj.pIndexData.new_format)
                        Comm.send_to_char(RomString.sprintf("Damage is %dd%d (average %d)\n\r",
                            obj.value[1], obj.value[2],
                            (1 + obj.value[2]) * obj.value[1] / 2), ch);
                    else
                        Comm.send_to_char(RomString.sprintf("Damage is %d to %d (average %d)\n\r",
                            obj.value[1], obj.value[2],
                            (obj.value[1] + obj.value[2]) / 2), ch);

                    Comm.send_to_char(RomString.sprintf("Damage noun is %s.\n\r",
                        (obj.value[3] > 0 && obj.value[3] < MAX_DAMAGE_MESSAGE)
                            ? Tables.attack_table[obj.value[3]].noun
                            : "undefined"), ch);

                    if (obj.value[4] != 0)
                    {
                        Comm.send_to_char(RomString.sprintf("Weapons flags: %s\n\r",
                            Handler.weapon_bit_name(obj.value[4])), ch);
                    }
                    break;

                case ITEM_ARMOR:
                    Comm.send_to_char(RomString.sprintf(
                        "Armor class is %d pierce, %d bash, %d slash, and %d vs. magic\n\r",
                        obj.value[0], obj.value[1], obj.value[2],
                        obj.value[3]), ch);
                    break;

                case ITEM_CONTAINER:
                    Comm.send_to_char(RomString.sprintf(
                        "Capacity: %d#  Maximum weight: %d#  flags: %s\n\r",
                        obj.value[0], obj.value[3],
                        Handler.cont_bit_name(obj.value[1])), ch);
                    if (obj.value[4] != 100)
                    {
                        Comm.send_to_char(RomString.sprintf("Weight multiplier: %d%%\n\r",
                            obj.value[4]), ch);
                    }
                    break;
            }

            if (obj.extra_descr != null || obj.pIndexData.extra_descr != null)
            {
                Comm.send_to_char("Extra description keywords: '", ch);

                for (var ed = obj.extra_descr; ed != null; ed = ed.next)
                {
                    Comm.send_to_char(ed.keyword, ch);
                    if (ed.next != null)
                        Comm.send_to_char(" ", ch);
                }

                for (var ed = obj.pIndexData.extra_descr; ed != null; ed = ed.next)
                {
                    Comm.send_to_char(ed.keyword, ch);
                    if (ed.next != null)
                        Comm.send_to_char(" ", ch);
                }

                Comm.send_to_char("'\n\r", ch);
            }

            for (var paf = obj.affected; paf != null; paf = paf.next)
            {
                Comm.send_to_char(RomString.sprintf("Affects %s by %d, level %d",
                    Handler.affect_loc_name(paf.location), paf.modifier, paf.level), ch);
                if (paf.duration > -1)
                    Comm.send_to_char(RomString.sprintf(", %d hours.\n\r", paf.duration), ch);
                else
                    Comm.send_to_char(".\n\r", ch);
                if (paf.bitvector != 0)
                {
                    string buf;
                    switch (paf.where)
                    {
                        case TO_AFFECTS:
                            buf = RomString.sprintf("Adds %s affect.\n",
                                Handler.affect_bit_name(paf.bitvector));
                            break;
                        case TO_WEAPON:
                            buf = RomString.sprintf("Adds %s weapon flags.\n",
                                Handler.weapon_bit_name(paf.bitvector));
                            break;
                        case TO_OBJECT:
                            buf = RomString.sprintf("Adds %s object flag.\n",
                                Handler.extra_bit_name(paf.bitvector));
                            break;
                        case TO_IMMUNE:
                            buf = RomString.sprintf("Adds immunity to %s.\n",
                                Handler.imm_bit_name(paf.bitvector));
                            break;
                        case TO_RESIST:
                            buf = RomString.sprintf("Adds resistance to %s.\n\r",
                                Handler.imm_bit_name(paf.bitvector));
                            break;
                        case TO_VULN:
                            buf = RomString.sprintf("Adds vulnerability to %s.\n\r",
                                Handler.imm_bit_name(paf.bitvector));
                            break;
                        default:
                            buf = RomString.sprintf("Unknown bit %d: %d\n\r",
                                paf.where, paf.bitvector);
                            break;
                    }
                    Comm.send_to_char(buf, ch);
                }
            }

            if (!obj.enchanted)
                for (var paf = obj.pIndexData.affected; paf != null; paf = paf.next)
                {
                    Comm.send_to_char(RomString.sprintf("Affects %s by %d, level %d.\n\r",
                        Handler.affect_loc_name(paf.location), paf.modifier,
                        paf.level), ch);
                    if (paf.bitvector != 0)
                    {
                        string buf;
                        switch (paf.where)
                        {
                            case TO_AFFECTS:
                                buf = RomString.sprintf("Adds %s affect.\n",
                                    Handler.affect_bit_name(paf.bitvector));
                                break;
                            case TO_OBJECT:
                                buf = RomString.sprintf("Adds %s object flag.\n",
                                    Handler.extra_bit_name(paf.bitvector));
                                break;
                            case TO_IMMUNE:
                                buf = RomString.sprintf("Adds immunity to %s.\n",
                                    Handler.imm_bit_name(paf.bitvector));
                                break;
                            case TO_RESIST:
                                buf = RomString.sprintf("Adds resistance to %s.\n\r",
                                    Handler.imm_bit_name(paf.bitvector));
                                break;
                            case TO_VULN:
                                buf = RomString.sprintf("Adds vulnerability to %s.\n\r",
                                    Handler.imm_bit_name(paf.bitvector));
                                break;
                            default:
                                buf = RomString.sprintf("Unknown bit %d: %d\n\r",
                                    paf.where, paf.bitvector);
                                break;
                        }
                        Comm.send_to_char(buf, ch);
                    }
                }
        }

        public static void do_mstat(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                Comm.send_to_char("Stat whom?\n\r", ch);
                return;
            }
            var victim = Handler.get_char_world(ch, argument);
            if (victim == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }
            Comm.send_to_char(RomString.sprintf("Name: %s\n\r", victim.name), ch);
            Comm.send_to_char(RomString.sprintf(
                "Vnum: %d  Format: %s  Race: %s  Group: %d  Sex: %s  Room: %d\n\r",
                Bit.IS_NPC(victim) ? victim.pIndexData.vnum : 0,
                Bit.IS_NPC(victim) ? (victim.pIndexData.new_format ? "new" : "old") : "pc",
                Tables.race_table[victim.race].name,
                Bit.IS_NPC(victim) ? victim.group : 0, Tables.sex_table[victim.sex].name,
                victim.in_room == null ? 0 : victim.in_room.vnum), ch);

            if (Bit.IS_NPC(victim))
            {
                Comm.send_to_char(RomString.sprintf("Count: %d  Killed: %d\n\r",
                    victim.pIndexData.count, victim.pIndexData.killed), ch);
            }

            Comm.send_to_char(RomString.sprintf(
                "Str: %d(%d)  Int: %d(%d)  Wis: %d(%d)  Dex: %d(%d)  Con: %d(%d)\n\r",
                victim.perm_stat[STAT_STR], Handler.get_curr_stat(victim, STAT_STR),
                victim.perm_stat[STAT_INT], Handler.get_curr_stat(victim, STAT_INT),
                victim.perm_stat[STAT_WIS], Handler.get_curr_stat(victim, STAT_WIS),
                victim.perm_stat[STAT_DEX], Handler.get_curr_stat(victim, STAT_DEX),
                victim.perm_stat[STAT_CON], Handler.get_curr_stat(victim, STAT_CON)), ch);
            Comm.send_to_char(RomString.sprintf(
                "Hp: %d/%d  Mana: %d/%d  Move: %d/%d  Practices: %d\n\r",
                victim.hit, victim.max_hit, victim.mana, victim.max_mana,
                victim.move, victim.max_move, Bit.IS_NPC(ch) ? 0 : victim.practice), ch);
            Comm.send_to_char(RomString.sprintf(
                "Lv: %d  Class: %s  Align: %d  Gold: %ld  Silver: %ld  Exp: %d\n\r",
                victim.level,
                Bit.IS_NPC(victim) ? "mobile" : Tables.class_table[victim.klass].name,
                victim.alignment, victim.gold, victim.silver, victim.exp), ch);

            Comm.send_to_char(RomString.sprintf(
                "Armor: pierce: %d  bash: %d  slash: %d  magic: %d\n\r",
                Handler.GET_AC(victim, AC_PIERCE), Handler.GET_AC(victim, AC_BASH),
                Handler.GET_AC(victim, AC_SLASH), Handler.GET_AC(victim, AC_EXOTIC)), ch);

            Comm.send_to_char(RomString.sprintf(
                "Hit: %d  Dam: %d  Saves: %d  Size: %s  Position: %s  Wimpy: %d\n\r",
                Handler.GET_HITROLL(victim), Handler.GET_DAMROLL(victim), victim.saving_throw,
                Tables.size_table[victim.size].name,
                Tables.position_table[victim.position].name, victim.wimpy), ch);

            if (Bit.IS_NPC(victim) && victim.pIndexData.new_format)
            {
                Comm.send_to_char(RomString.sprintf("Damage: %dd%d  Message:  %s\n\r",
                    victim.damage[DICE_NUMBER], victim.damage[DICE_TYPE],
                    Tables.attack_table[victim.dam_type].noun), ch);
            }
            Comm.send_to_char(RomString.sprintf("Fighting: %s\n\r",
                victim.fighting != null ? victim.fighting.name : "(none)"), ch);

            if (!Bit.IS_NPC(victim))
            {
                Comm.send_to_char(RomString.sprintf(
                    "Thirst: %d  Hunger: %d  Full: %d  Drunk: %d\n\r",
                    victim.pcdata.condition[COND_THIRST],
                    victim.pcdata.condition[COND_HUNGER],
                    victim.pcdata.condition[COND_FULL],
                    victim.pcdata.condition[COND_DRUNK]), ch);
            }

            Comm.send_to_char(RomString.sprintf("Carry number: %d  Carry weight: %ld\n\r",
                victim.carry_number, Handler.get_carry_weight(victim) / 10), ch);

            if (!Bit.IS_NPC(victim))
            {
                Comm.send_to_char(RomString.sprintf(
                    "Age: %d  Played: %d  Last Level: %d  Timer: %d\n\r",
                    Handler.get_age(victim),
                    (int)(victim.played + Game.current_time - victim.logon) / 3600,
                    victim.pcdata.last_level, victim.timer), ch);
            }

            Comm.send_to_char(RomString.sprintf("Act: %s\n\r", Handler.act_bit_name(victim.act)), ch);

            if (victim.comm != 0)
            {
                Comm.send_to_char(RomString.sprintf("Comm: %s\n\r",
                    Handler.comm_bit_name(victim.comm)), ch);
            }

            if (Bit.IS_NPC(victim) && victim.off_flags != 0)
            {
                Comm.send_to_char(RomString.sprintf("Offense: %s\n\r",
                    Handler.off_bit_name(victim.off_flags)), ch);
            }

            if (victim.imm_flags != 0)
            {
                Comm.send_to_char(RomString.sprintf("Immune: %s\n\r",
                    Handler.imm_bit_name(victim.imm_flags)), ch);
            }

            if (victim.res_flags != 0)
            {
                Comm.send_to_char(RomString.sprintf("Resist: %s\n\r",
                    Handler.imm_bit_name(victim.res_flags)), ch);
            }

            if (victim.vuln_flags != 0)
            {
                Comm.send_to_char(RomString.sprintf("Vulnerable: %s\n\r",
                    Handler.imm_bit_name(victim.vuln_flags)), ch);
            }

            Comm.send_to_char(RomString.sprintf("Form: %s\n\rParts: %s\n\r",
                Handler.form_bit_name(victim.form), Handler.part_bit_name(victim.parts)), ch);

            if (victim.affected_by != 0)
            {
                Comm.send_to_char(RomString.sprintf("Affected by %s\n\r",
                    Handler.affect_bit_name(victim.affected_by)), ch);
            }

            Comm.send_to_char(RomString.sprintf("Master: %s  Leader: %s  Pet: %s\n\r",
                victim.master != null ? victim.master.name : "(none)",
                victim.leader != null ? victim.leader.name : "(none)",
                victim.pet != null ? victim.pet.name : "(none)"), ch);

            if (!Bit.IS_NPC(victim))
            {
                Comm.send_to_char(RomString.sprintf("Security: %d.\n\r", victim.pcdata.security), ch);
            }

            Comm.send_to_char(RomString.sprintf("Short description: %s\n\rLong  description: %s",
                victim.short_descr,
                victim.long_descr.Length != 0 ? victim.long_descr : "(none)\n\r"), ch);

            if (Bit.IS_NPC(victim) && victim.spec_fun != null)
            {
                Comm.send_to_char(RomString.sprintf("Mobile has special procedure %s.\n\r",
                    Lookup.spec_name(victim.spec_fun)), ch);
            }

            for (var paf = victim.affected; paf != null; paf = paf.next)
            {
                Comm.send_to_char(RomString.sprintf(
                    "Spell: '%s' modifies %s by %d for %d hours with bits %s, level %d.\n\r",
                    Tables.skill_table[paf.type].name,
                    Handler.affect_loc_name(paf.location),
                    paf.modifier,
                    paf.duration, Handler.affect_bit_name(paf.bitvector), paf.level), ch);
            }
        }

        public static void do_vnum(CharData ch, string argument)
        {
            string rest = RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                Comm.send_to_char("Syntax:\n\r", ch);
                Comm.send_to_char("  vnum obj <name>\n\r", ch);
                Comm.send_to_char("  vnum mob <name>\n\r", ch);
                Comm.send_to_char("  vnum skill <skill or spell>\n\r", ch);
                return;
            }
            if (!RomString.str_cmp(arg, "obj"))
            {
                Interp.do_function(ch, do_ofind, rest);
                return;
            }
            if (!RomString.str_cmp(arg, "mob") || !RomString.str_cmp(arg, "char"))
            {
                Interp.do_function(ch, do_mfind, rest);
                return;
            }
            if (!RomString.str_cmp(arg, "skill") || !RomString.str_cmp(arg, "spell"))
            {
                Interp.do_function(ch, do_slookup, rest);
                return;
            }
            Interp.do_function(ch, do_mfind, argument);
            Interp.do_function(ch, do_ofind, argument);
        }

        public static void do_mfind(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                Comm.send_to_char("Find whom?\n\r", ch);
                return;
            }
            bool found = false;
            int nMatch = 0;
            for (int vnum = 0; nMatch < Game.top_mob_index; vnum++)
            {
                var pMobIndex = Handler.get_mob_index(vnum);
                if (pMobIndex != null)
                {
                    nMatch++;
                    if (Handler.is_name(argument, pMobIndex.player_name))
                    {
                        found = true;
                        Comm.send_to_char(RomString.sprintf("[%5d] %s\n\r",
                            pMobIndex.vnum, pMobIndex.short_descr), ch);
                    }
                }
            }
            if (!found)
                Comm.send_to_char("No mobiles by that name.\n\r", ch);
        }

        public static void do_ofind(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                Comm.send_to_char("Find what?\n\r", ch);
                return;
            }
            bool found = false;
            int nMatch = 0;
            for (int vnum = 0; nMatch < Game.top_obj_index; vnum++)
            {
                var pObjIndex = Handler.get_obj_index(vnum);
                if (pObjIndex != null)
                {
                    nMatch++;
                    if (Handler.is_name(argument, pObjIndex.name))
                    {
                        found = true;
                        Comm.send_to_char(RomString.sprintf("[%5d] %s\n\r",
                            pObjIndex.vnum, pObjIndex.short_descr), ch);
                    }
                }
            }
            if (!found)
                Comm.send_to_char("No objects by that name.\n\r", ch);
        }

        public static void do_slookup(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                Comm.send_to_char("Lookup which skill or spell?\n\r", ch);
                return;
            }
            if (!RomString.str_cmp(arg, "all"))
            {
                for (int sn = 0; sn < Tables.skill_table.Length; sn++)
                {
                    if (Tables.skill_table[sn].name == null)
                        break;
                    Comm.send_to_char(RomString.sprintf("Sn: %3d  Slot: %3d  Skill/spell: '%s'\n\r",
                        sn, Tables.skill_table[sn].slot, Tables.skill_table[sn].name), ch);
                }
            }
            else
            {
                int sn = Lookup.skill_lookup(arg);
                if (sn < 0)
                {
                    Comm.send_to_char("No such skill or spell.\n\r", ch);
                    return;
                }
                Comm.send_to_char(RomString.sprintf("Sn: %3d  Slot: %3d  Skill/spell: '%s'\n\r",
                    sn, Tables.skill_table[sn].slot, Tables.skill_table[sn].name), ch);
            }
        }

        public static void do_load(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                Comm.send_to_char("Syntax:\n\r", ch);
                Comm.send_to_char("  load mob <vnum>\n\r", ch);
                Comm.send_to_char("  load obj <vnum> <level>\n\r", ch);
                return;
            }
            if (!RomString.str_cmp(arg, "mob") || !RomString.str_cmp(arg, "char"))
            {
                Interp.do_function(ch, do_mload, argument);
                return;
            }
            if (!RomString.str_cmp(arg, "obj"))
            {
                Interp.do_function(ch, do_oload, argument);
                return;
            }
            Interp.do_function(ch, do_load, "");
        }

        public static void do_mload(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);
            if (arg.Length == 0 || !Interp.is_number(arg))
            {
                Comm.send_to_char("Syntax: load mob <vnum>.\n\r", ch);
                return;
            }
            int vnum = atoi(arg);
            var pMobIndex = Handler.get_mob_index(vnum);
            if (pMobIndex == null)
            {
                Comm.send_to_char("No mob has that vnum.\n\r", ch);
                return;
            }
            var victim = Db.create_mobile(pMobIndex);
            Handler.char_to_room(victim, ch.in_room);
            Comm.act("$n has created $N!", ch, null, victim, TO_ROOM);
            Comm.wiznet(RomString.sprintf("$N loads %s.", victim.short_descr),
                ch, null, WIZ_LOAD, WIZ_SECURE, Handler.get_trust(ch));
            Comm.send_to_char("Ok.\n\r", ch);
        }

        public static void do_oload(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg1);
            RomString.one_argument(argument, out string arg2);
            if (arg1.Length == 0 || !Interp.is_number(arg1))
            {
                Comm.send_to_char("Syntax: load obj <vnum> <level>.\n\r", ch);
                return;
            }
            int level = Handler.get_trust(ch);
            if (arg2.Length != 0)
            {
                if (!Interp.is_number(arg2))
                {
                    Comm.send_to_char("Syntax: oload <vnum> <level>.\n\r", ch);
                    return;
                }
                level = atoi(arg2);
                if (level < 0 || level > Handler.get_trust(ch))
                {
                    Comm.send_to_char("Level must be be between 0 and your level.\n\r",
                        ch);
                    return;
                }
            }
            int vnum = atoi(arg1);
            var pObjIndex = Handler.get_obj_index(vnum);
            if (pObjIndex == null)
            {
                Comm.send_to_char("No object has that vnum.\n\r", ch);
                return;
            }
            var obj = Db.create_object(pObjIndex, level);
            if (Bit.CAN_WEAR(obj, ITEM_TAKE))
                Handler.obj_to_char(obj, ch);
            else
                Handler.obj_to_room(obj, ch.in_room);
            Comm.act("$n has created $p!", ch, obj, null, TO_ROOM);
            Comm.wiznet("$N loads $p.", ch, obj, WIZ_LOAD, WIZ_SECURE, Handler.get_trust(ch));
            Comm.send_to_char("Ok.\n\r", ch);
        }

        public static void do_purge(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                if (ch.in_room != null)
                {
                    for (var victim = ch.in_room.people; victim != null; )
                    {
                        var vnext = victim.next_in_room;
                        if (Bit.IS_NPC(victim) && !Bit.IS_SET(victim.act, ACT_NOPURGE)
                            && victim != ch)
                            Handler.extract_char(victim, true);
                        victim = vnext;
                    }
                    for (var obj = ch.in_room.contents; obj != null; )
                    {
                        var obj_next = obj.next_content;
                        if (!Bit.IS_OBJ_STAT(obj, ITEM_NOPURGE))
                            Handler.extract_obj(obj);
                        obj = obj_next;
                    }
                    Comm.act("$n purges the room!", ch, null, null, TO_ROOM);
                }
                Comm.send_to_char("Ok.\n\r", ch);
                return;
            }
            var vch = Handler.get_char_world(ch, arg);
            if (vch == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }
            if (!Bit.IS_NPC(vch))
            {
                if (ch == vch)
                {
                    Comm.send_to_char("Ho ho ho.\n\r", ch);
                    return;
                }
                if (Handler.get_trust(ch) <= Handler.get_trust(vch))
                {
                    Comm.send_to_char("Maybe that wasn't a good idea...\n\r", ch);
                    Comm.send_to_char(RomString.sprintf("%s tried to purge you!\n\r", ch.name), vch);
                    return;
                }
                Comm.act("$n disintegrates $N.", ch, null, vch, TO_NOTVICT);
                if (vch.level > 1)
                    Save.save_char_obj(vch);
                var d = vch.desc;
                Handler.extract_char(vch, true);
                if (d != null)
                    Comm.close_socket(d);
                return;
            }
            Comm.act("$n purges $N.", ch, null, vch, TO_NOTVICT);
            Handler.extract_char(vch, true);
        }

        public static void do_reboo(CharData ch, string argument)
        {
            Comm.send_to_char("If you want to REBOOT, spell it out.\n\r", ch);
        }

        public static void do_reboot(CharData ch, string argument)
        {
            if (ch.invis_level < LEVEL_HERO)
                Interp.do_function(ch, do_echo, RomString.sprintf("Reboot by %s.", ch.name));
            Game.merc_down = true;
            for (var d = Game.descriptor_list; d != null; )
            {
                var d_next = d.next;
                var vch = d.original ?? d.character;
                if (vch != null)
                    Save.save_char_obj(vch);
                Comm.close_socket(d);
                d = d_next;
            }
        }

        public static void do_shutdow(CharData ch, string argument)
        {
            Comm.send_to_char("If you want to SHUTDOWN, spell it out.\n\r", ch);
        }

        public static void do_shutdown(CharData ch, string argument)
        {
            string buf = "";
            if (ch.invis_level < LEVEL_HERO)
                buf = RomString.sprintf("Shutdown by %s.", ch.name);
            Db.append_file(ch, SHUTDOWN_FILE, buf);
            buf += "\n\r";
            if (ch.invis_level < LEVEL_HERO)
                Interp.do_function(ch, do_echo, buf);
            Game.merc_down = true;
            for (var d = Game.descriptor_list; d != null; )
            {
                var d_next = d.next;
                var vch = d.original ?? d.character;
                if (vch != null)
                    Save.save_char_obj(vch);
                Comm.close_socket(d);
                d = d_next;
            }
        }

        public static void do_protect(CharData ch, string argument)
        {
            if (argument.Length == 0)
            {
                Comm.send_to_char("Protect whom from snooping?\n\r", ch);
                return;
            }
            var victim = Handler.get_char_world(ch, argument);
            if (victim == null)
            {
                Comm.send_to_char("You can't find them.\n\r", ch);
                return;
            }
            if (Bit.IS_SET(victim.comm, COMM_SNOOP_PROOF))
            {
                Comm.act_new("$N is no longer snoop-proof.", ch, null, victim, TO_CHAR, POS_DEAD);
                Comm.send_to_char("Your snoop-proofing was just removed.\n\r", victim);
                Bit.REMOVE_BIT(ref victim.comm, COMM_SNOOP_PROOF);
            }
            else
            {
                Comm.act_new("$N is now snoop-proof.", ch, null, victim, TO_CHAR, POS_DEAD);
                Comm.send_to_char("You are now immune to snooping.\n\r", victim);
                Bit.SET_BIT(ref victim.comm, COMM_SNOOP_PROOF);
            }
        }

        public static void do_freeze(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                Comm.send_to_char("Freeze whom?\n\r", ch);
                return;
            }
            var victim = Handler.get_char_world(ch, arg);
            if (victim == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }
            if (Bit.IS_NPC(victim))
            {
                Comm.send_to_char("Not on NPC's.\n\r", ch);
                return;
            }
            if (Handler.get_trust(victim) >= Handler.get_trust(ch))
            {
                Comm.send_to_char("You failed.\n\r", ch);
                return;
            }
            if (Bit.IS_SET(victim.act, PLR_FREEZE))
            {
                Bit.REMOVE_BIT(ref victim.act, PLR_FREEZE);
                Comm.send_to_char("You can play again.\n\r", victim);
                Comm.send_to_char("FREEZE removed.\n\r", ch);
                Comm.wiznet(RomString.sprintf("$N thaws %s.", victim.name),
                    ch, null, WIZ_PENALTIES, WIZ_SECURE, 0);
            }
            else
            {
                Bit.SET_BIT(ref victim.act, PLR_FREEZE);
                Comm.send_to_char("You can't do ANYthing!\n\r", victim);
                Comm.send_to_char("FREEZE set.\n\r", ch);
                Comm.wiznet(RomString.sprintf("$N puts %s in the deep freeze.", victim.name),
                    ch, null, WIZ_PENALTIES, WIZ_SECURE, 0);
            }
            Save.save_char_obj(victim);
        }

        public static void do_log(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                Comm.send_to_char("Log whom?\n\r", ch);
                return;
            }
            if (!RomString.str_cmp(arg, "all"))
            {
                if (Game.fLogAll)
                {
                    Game.fLogAll = false;
                    Comm.send_to_char("Log ALL off.\n\r", ch);
                }
                else
                {
                    Game.fLogAll = true;
                    Comm.send_to_char("Log ALL on.\n\r", ch);
                }
                return;
            }
            var victim = Handler.get_char_world(ch, arg);
            if (victim == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }
            if (Bit.IS_NPC(victim))
            {
                Comm.send_to_char("Not on NPC's.\n\r", ch);
                return;
            }
            if (Bit.IS_SET(victim.act, PLR_LOG))
            {
                Bit.REMOVE_BIT(ref victim.act, PLR_LOG);
                Comm.send_to_char("LOG removed.\n\r", ch);
            }
            else
            {
                Bit.SET_BIT(ref victim.act, PLR_LOG);
                Comm.send_to_char("LOG set.\n\r", ch);
            }
        }

        public static void do_set(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);
            if (arg.Length == 0)
            {
                Comm.send_to_char("Syntax:\n\r", ch);
                Comm.send_to_char("  set mob       <name> <field> <value>\n\r", ch);
                Comm.send_to_char("  set character <name> <field> <value>\n\r", ch);
                Comm.send_to_char("  set obj       <name> <field> <value>\n\r", ch);
                Comm.send_to_char("  set room      <room> <field> <value>\n\r", ch);
                Comm.send_to_char("  set skill     <name> <spell or skill> <value>\n\r", ch);
                return;
            }
            if (!RomString.str_prefix(arg, "mobile") || !RomString.str_prefix(arg, "character"))
            {
                Interp.do_function(ch, do_mset, argument);
                return;
            }
            if (!RomString.str_prefix(arg, "skill") || !RomString.str_prefix(arg, "spell"))
            {
                Interp.do_function(ch, do_sset, argument);
                return;
            }
            if (!RomString.str_prefix(arg, "object"))
            {
                Interp.do_function(ch, do_oset, argument);
                return;
            }
            if (!RomString.str_prefix(arg, "room"))
            {
                Interp.do_function(ch, do_rset, argument);
                return;
            }
            Interp.do_function(ch, do_set, "");
        }

        public static void do_sset(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg1);
            argument = RomString.one_argument(argument, out string arg2);
            argument = RomString.one_argument(argument, out string arg3);
            if (arg1.Length == 0 || arg2.Length == 0 || arg3.Length == 0)
            {
                Comm.send_to_char("Syntax:\n\r", ch);
                Comm.send_to_char("  set skill <name> <spell or skill> <value>\n\r", ch);
                Comm.send_to_char("  set skill <name> all <value>\n\r", ch);
                Comm.send_to_char("   (use the name of the skill, not the number)\n\r", ch);
                return;
            }
            var victim = Handler.get_char_world(ch, arg1);
            if (victim == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }
            if (Bit.IS_NPC(victim))
            {
                Comm.send_to_char("Not on NPC's.\n\r", ch);
                return;
            }
            bool fAll = !RomString.str_cmp(arg2, "all");
            int sn = 0;
            if (!fAll && (sn = Lookup.skill_lookup(arg2)) < 0)
            {
                Comm.send_to_char("No such skill or spell.\n\r", ch);
                return;
            }
            if (!Interp.is_number(arg3))
            {
                Comm.send_to_char("Value must be numeric.\n\r", ch);
                return;
            }
            int value = atoi(arg3);
            if (value < 0 || value > 100)
            {
                Comm.send_to_char("Value range is 0 to 100.\n\r", ch);
                return;
            }
            if (fAll)
            {
                for (sn = 0; sn < Tables.skill_table.Length; sn++)
                {
                    if (Tables.skill_table[sn].name != null)
                        victim.pcdata.learned[sn] = value;
                }
            }
            else
                victim.pcdata.learned[sn] = value;
        }

        public static void do_mset(CharData ch, string argument)
        {
            argument = RomString.smash_tilde(argument);
            argument = RomString.one_argument(argument, out string arg1);
            argument = RomString.one_argument(argument, out string arg2);
            string arg3 = argument;
            if (arg1.Length == 0 || arg2.Length == 0 || arg3.Length == 0)
            {
                Comm.send_to_char("Syntax:\n\r", ch);
                Comm.send_to_char("  set char <name> <field> <value>\n\r", ch);
                Comm.send_to_char("  Field being one of:\n\r", ch);
                Comm.send_to_char("    str int wis dex con sex class level\n\r", ch);
                Comm.send_to_char("    race group gold silver hp mana move prac\n\r", ch);
                Comm.send_to_char("    align train thirst hunger drunk full\n\r", ch);
                Comm.send_to_char("    security hours\n\r", ch);
                return;
            }
            var victim = Handler.get_char_world(ch, arg1);
            if (victim == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }
            victim.zone = null;
            int value = Interp.is_number(arg3) ? atoi(arg3) : -1;
            if (!RomString.str_cmp(arg2, "str"))
            {
                if (value < 3 || value > Handler.get_max_train(victim, STAT_STR))
                {
                    Comm.send_to_char(RomString.sprintf("Strength range is 3 to %d\n\r.",
                        Handler.get_max_train(victim, STAT_STR)), ch);
                    return;
                }
                victim.perm_stat[STAT_STR] = value;
                return;
            }
            if (!RomString.str_cmp(arg2, "security"))
            {
                if (Bit.IS_NPC(ch))
                {
                    Comm.send_to_char("NPC's can't set this value.\n\r", ch);
                    return;
                }

                if (Bit.IS_NPC(victim))
                {
                    Comm.send_to_char("Not on NPC's.\n\r", ch);
                    return;
                }

                if (value > ch.pcdata.security || value < 0)
                {
                    if (ch.pcdata.security != 0)
                    {
                        Comm.send_to_char(RomString.sprintf("Valid security is 0-%d.\n\r",
                            ch.pcdata.security), ch);
                    }
                    else
                    {
                        Comm.send_to_char("Valid security is 0 only.\n\r", ch);
                    }
                    return;
                }
                victim.pcdata.security = value;
                return;
            }
            if (!RomString.str_cmp(arg2, "int"))
            {
                if (value < 3 || value > Handler.get_max_train(victim, STAT_INT))
                {
                    Comm.send_to_char(RomString.sprintf("Intelligence range is 3 to %d.\n\r",
                        Handler.get_max_train(victim, STAT_INT)), ch);
                    return;
                }
                victim.perm_stat[STAT_INT] = value;
                return;
            }
            if (!RomString.str_cmp(arg2, "wis"))
            {
                if (value < 3 || value > Handler.get_max_train(victim, STAT_WIS))
                {
                    Comm.send_to_char(RomString.sprintf("Wisdom range is 3 to %d.\n\r",
                        Handler.get_max_train(victim, STAT_WIS)), ch);
                    return;
                }
                victim.perm_stat[STAT_WIS] = value;
                return;
            }
            if (!RomString.str_cmp(arg2, "dex"))
            {
                if (value < 3 || value > Handler.get_max_train(victim, STAT_DEX))
                {
                    Comm.send_to_char(RomString.sprintf("Dexterity range is 3 to %d.\n\r",
                        Handler.get_max_train(victim, STAT_DEX)), ch);
                    return;
                }
                victim.perm_stat[STAT_DEX] = value;
                return;
            }
            if (!RomString.str_cmp(arg2, "con"))
            {
                if (value < 3 || value > Handler.get_max_train(victim, STAT_CON))
                {
                    Comm.send_to_char(RomString.sprintf("Constitution range is 3 to %d.\n\r",
                        Handler.get_max_train(victim, STAT_CON)), ch);
                    return;
                }
                victim.perm_stat[STAT_CON] = value;
                return;
            }
            if (!RomString.str_prefix(arg2, "sex"))
            {
                if (value < 0 || value > 2)
                {
                    Comm.send_to_char("Sex range is 0 to 2.\n\r", ch);
                    return;
                }
                victim.sex = value;
                if (!Bit.IS_NPC(victim))
                    victim.pcdata.true_sex = value;
                return;
            }
            if (!RomString.str_prefix(arg2, "class"))
            {
                if (Bit.IS_NPC(victim))
                {
                    Comm.send_to_char("Mobiles have no class.\n\r", ch);
                    return;
                }

                int klass = Lookup.class_lookup(arg3);
                if (klass == -1)
                {
                    string buf = "Possible classes are: ";
                    for (klass = 0; klass < MAX_CLASS; klass++)
                    {
                        if (klass > 0)
                            buf += " ";
                        buf += Tables.class_table[klass].name;
                    }
                    buf += ".\n\r";
                    Comm.send_to_char(buf, ch);
                    return;
                }

                victim.klass = klass;
                return;
            }
            if (!RomString.str_prefix(arg2, "level"))
            {
                if (!Bit.IS_NPC(victim))
                {
                    Comm.send_to_char("Not on PC's.\n\r", ch);
                    return;
                }

                if (value < 0 || value > MAX_LEVEL)
                {
                    Comm.send_to_char(RomString.sprintf("Level range is 0 to %d.\n\r", MAX_LEVEL), ch);
                    return;
                }
                victim.level = value;
                return;
            }
            if (!RomString.str_prefix(arg2, "gold"))
            {
                victim.gold = value;
                return;
            }
            if (!RomString.str_prefix(arg2, "silver"))
            {
                victim.silver = value;
                return;
            }
            if (!RomString.str_prefix(arg2, "hp"))
            {
                if (value < -10 || value > 30000)
                {
                    Comm.send_to_char("Hp range is -10 to 30,000 hit points.\n\r", ch);
                    return;
                }
                victim.max_hit = value;
                if (!Bit.IS_NPC(victim))
                    victim.pcdata.perm_hit = value;
                return;
            }
            if (!RomString.str_prefix(arg2, "mana"))
            {
                if (value < 0 || value > 30000)
                {
                    Comm.send_to_char("Mana range is 0 to 30,000 mana points.\n\r", ch);
                    return;
                }
                victim.max_mana = value;
                if (!Bit.IS_NPC(victim))
                    victim.pcdata.perm_mana = value;
                return;
            }
            if (!RomString.str_prefix(arg2, "move"))
            {
                if (value < 0 || value > 30000)
                {
                    Comm.send_to_char("Move range is 0 to 30,000 move points.\n\r", ch);
                    return;
                }
                victim.max_move = value;
                if (!Bit.IS_NPC(victim))
                    victim.pcdata.perm_move = value;
                return;
            }
            if (!RomString.str_prefix(arg2, "practice"))
            {
                if (value < 0 || value > 250)
                {
                    Comm.send_to_char("Practice range is 0 to 250 sessions.\n\r", ch);
                    return;
                }
                victim.practice = value;
                return;
            }
            if (!RomString.str_prefix(arg2, "train"))
            {
                if (value < 0 || value > 50)
                {
                    Comm.send_to_char("Training session range is 0 to 50 sessions.\n\r", ch);
                    return;
                }
                victim.train = value;
                return;
            }
            if (!RomString.str_prefix(arg2, "align"))
            {
                if (value < -1000 || value > 1000)
                {
                    Comm.send_to_char("Alignment range is -1000 to 1000.\n\r", ch);
                    return;
                }
                victim.alignment = value;
                return;
            }
            if (!RomString.str_prefix(arg2, "thirst"))
            {
                if (Bit.IS_NPC(victim))
                {
                    Comm.send_to_char("Not on NPC's.\n\r", ch);
                    return;
                }

                if (value < -1 || value > 100)
                {
                    Comm.send_to_char("Thirst range is -1 to 100.\n\r", ch);
                    return;
                }

                victim.pcdata.condition[COND_THIRST] = value;
                return;
            }
            if (!RomString.str_prefix(arg2, "drunk"))
            {
                if (Bit.IS_NPC(victim))
                {
                    Comm.send_to_char("Not on NPC's.\n\r", ch);
                    return;
                }

                if (value < -1 || value > 100)
                {
                    Comm.send_to_char("Drunk range is -1 to 100.\n\r", ch);
                    return;
                }

                victim.pcdata.condition[COND_DRUNK] = value;
                return;
            }
            if (!RomString.str_prefix(arg2, "full"))
            {
                if (Bit.IS_NPC(victim))
                {
                    Comm.send_to_char("Not on NPC's.\n\r", ch);
                    return;
                }

                if (value < -1 || value > 100)
                {
                    Comm.send_to_char("Full range is -1 to 100.\n\r", ch);
                    return;
                }

                victim.pcdata.condition[COND_FULL] = value;
                return;
            }
            if (!RomString.str_prefix(arg2, "hunger"))
            {
                if (Bit.IS_NPC(victim))
                {
                    Comm.send_to_char("Not on NPC's.\n\r", ch);
                    return;
                }

                if (value < -1 || value > 100)
                {
                    Comm.send_to_char("Full range is -1 to 100.\n\r", ch);
                    return;
                }

                victim.pcdata.condition[COND_HUNGER] = value;
                return;
            }
            if (!RomString.str_prefix(arg2, "race"))
            {
                int race = Lookup.race_lookup(arg3);

                if (race == 0)
                {
                    Comm.send_to_char("That is not a valid race.\n\r", ch);
                    return;
                }

                if (!Bit.IS_NPC(victim) && !Tables.race_table[race].pc_race)
                {
                    Comm.send_to_char("That is not a valid player race.\n\r", ch);
                    return;
                }

                victim.race = race;
                return;
            }
            if (!RomString.str_prefix(arg2, "group"))
            {
                if (!Bit.IS_NPC(victim))
                {
                    Comm.send_to_char("Only on NPCs.\n\r", ch);
                    return;
                }
                victim.group = value;
                return;
            }
            if (!RomString.str_prefix(arg2, "hours"))
            {
                if (Bit.IS_NPC(victim))
                {
                    Comm.send_to_char("Not on NPC's.\n\r", ch);
                    return;
                }

                if (!Interp.is_number(arg3))
                {
                    Comm.send_to_char("Value must be numeric.\n\r", ch);
                    return;
                }

                value = atoi(arg3);

                if (value < 0 || value > 999)
                {
                    Comm.send_to_char("Value must be between 0 and 999.\n\r", ch);
                    return;
                }

                victim.played = value * 3600;
                Comm.send_to_char(RomString.sprintf("%s's hours set to %d.", victim.name, value), ch);

                return;
            }
            Interp.do_function(ch, do_mset, "");
        }

        public static void do_oset(CharData ch, string argument)
        {
            argument = RomString.smash_tilde(argument);
            argument = RomString.one_argument(argument, out string arg1);
            argument = RomString.one_argument(argument, out string arg2);
            string arg3 = argument;
            if (arg1.Length == 0 || arg2.Length == 0 || arg3.Length == 0)
            {
                Comm.send_to_char("Syntax:\n\r", ch);
                Comm.send_to_char("  set obj <object> <field> <value>\n\r", ch);
                Comm.send_to_char("  Field being one of:\n\r", ch);
                Comm.send_to_char("    value0 value1 value2 value3 value4 (v1-v4)\n\r", ch);
                Comm.send_to_char("    extra wear level weight cost timer\n\r", ch);
                return;
            }

            var obj = Handler.get_obj_world(ch, arg1);
            if (obj == null)
            {
                Comm.send_to_char("Nothing like that in heaven or earth.\n\r", ch);
                return;
            }

            int value = atoi(arg3);

            if (!RomString.str_cmp(arg2, "value0") || !RomString.str_cmp(arg2, "v0"))
            {
                obj.value[0] = Bit.UMIN(50, value);
                return;
            }

            if (!RomString.str_cmp(arg2, "value1") || !RomString.str_cmp(arg2, "v1"))
            {
                obj.value[1] = value;
                return;
            }

            if (!RomString.str_cmp(arg2, "value2") || !RomString.str_cmp(arg2, "v2"))
            {
                obj.value[2] = value;
                return;
            }

            if (!RomString.str_cmp(arg2, "value3") || !RomString.str_cmp(arg2, "v3"))
            {
                obj.value[3] = value;
                return;
            }

            if (!RomString.str_cmp(arg2, "value4") || !RomString.str_cmp(arg2, "v4"))
            {
                obj.value[4] = value;
                return;
            }

            if (!RomString.str_prefix(arg2, "extra"))
            {
                obj.extra_flags = value;
                return;
            }

            if (!RomString.str_prefix(arg2, "wear"))
            {
                obj.wear_flags = value;
                return;
            }

            if (!RomString.str_prefix(arg2, "level"))
            {
                obj.level = value;
                return;
            }

            if (!RomString.str_prefix(arg2, "weight"))
            {
                obj.weight = value;
                return;
            }

            if (!RomString.str_prefix(arg2, "cost"))
            {
                obj.cost = value;
                return;
            }

            if (!RomString.str_prefix(arg2, "timer"))
            {
                obj.timer = value;
                return;
            }

            Interp.do_function(ch, do_oset, "");
        }

        public static void do_rset(CharData ch, string argument)
        {
            argument = RomString.smash_tilde(argument);
            argument = RomString.one_argument(argument, out string arg1);
            argument = RomString.one_argument(argument, out string arg2);
            string arg3 = argument;
            if (arg1.Length == 0 || arg2.Length == 0 || arg3.Length == 0)
            {
                Comm.send_to_char("Syntax:\n\r", ch);
                Comm.send_to_char("  set room <location> <field> <value>\n\r", ch);
                Comm.send_to_char("  Field being one of:\n\r", ch);
                Comm.send_to_char("    flags sector\n\r", ch);
                return;
            }

            var location = find_location(ch, arg1);
            if (location == null)
            {
                Comm.send_to_char("No such location.\n\r", ch);
                return;
            }

            if (!Handler.is_room_owner(ch, location) && ch.in_room != location
                && Handler.room_is_private(location) && !Bit.IS_TRUSTED(ch, IMPLEMENTOR))
            {
                Comm.send_to_char("That room is private right now.\n\r", ch);
                return;
            }

            if (!Interp.is_number(arg3))
            {
                Comm.send_to_char("Value must be numeric.\n\r", ch);
                return;
            }
            int value = atoi(arg3);

            if (!RomString.str_prefix(arg2, "flags"))
            {
                location.room_flags = value;
                return;
            }

            if (!RomString.str_prefix(arg2, "sector"))
            {
                location.sector_type = value;
                return;
            }

            Interp.do_function(ch, do_rset, "");
        }

        public static void do_string(CharData ch, string argument)
        {
            argument = RomString.smash_tilde(argument);
            argument = RomString.one_argument(argument, out string type);
            argument = RomString.one_argument(argument, out string arg1);
            argument = RomString.one_argument(argument, out string arg2);
            string arg3 = argument;

            if (type.Length == 0 || arg1.Length == 0 || arg2.Length == 0
                || arg3.Length == 0)
            {
                Comm.send_to_char("Syntax:\n\r", ch);
                Comm.send_to_char("  string char <name> <field> <string>\n\r", ch);
                Comm.send_to_char("    fields: name short long desc title spec\n\r", ch);
                Comm.send_to_char("  string obj  <name> <field> <string>\n\r", ch);
                Comm.send_to_char("    fields: name short long extended\n\r", ch);
                return;
            }

            if (!RomString.str_prefix(type, "character") || !RomString.str_prefix(type, "mobile"))
            {
                var victim = Handler.get_char_world(ch, arg1);
                if (victim == null)
                {
                    Comm.send_to_char("They aren't here.\n\r", ch);
                    return;
                }

                victim.zone = null;

                if (!RomString.str_prefix(arg2, "name"))
                {
                    if (!Bit.IS_NPC(victim))
                    {
                        Comm.send_to_char("Not on PC's.\n\r", ch);
                        return;
                    }
                    victim.name = arg3;
                    return;
                }

                if (!RomString.str_prefix(arg2, "description"))
                {
                    victim.description = arg3;
                    return;
                }

                if (!RomString.str_prefix(arg2, "short"))
                {
                    victim.short_descr = arg3;
                    return;
                }

                if (!RomString.str_prefix(arg2, "long"))
                {
                    arg3 += "\n\r";
                    victim.long_descr = arg3;
                    return;
                }

                if (!RomString.str_prefix(arg2, "title"))
                {
                    if (Bit.IS_NPC(victim))
                    {
                        Comm.send_to_char("Not on NPC's.\n\r", ch);
                        return;
                    }

                    ActInfo.set_title(victim, arg3);
                    return;
                }

                if (!RomString.str_prefix(arg2, "spec"))
                {
                    if (!Bit.IS_NPC(victim))
                    {
                        Comm.send_to_char("Not on PC's.\n\r", ch);
                        return;
                    }

                    if ((victim.spec_fun = Lookup.spec_lookup(arg3)) == null)
                    {
                        Comm.send_to_char("No such spec fun.\n\r", ch);
                        return;
                    }

                    return;
                }
            }

            if (!RomString.str_prefix(type, "object"))
            {
                var obj = Handler.get_obj_world(ch, arg1);
                if (obj == null)
                {
                    Comm.send_to_char("Nothing like that in heaven or earth.\n\r", ch);
                    return;
                }

                if (!RomString.str_prefix(arg2, "name"))
                {
                    obj.name = arg3;
                    return;
                }

                if (!RomString.str_prefix(arg2, "short"))
                {
                    obj.short_descr = arg3;
                    return;
                }

                if (!RomString.str_prefix(arg2, "long"))
                {
                    obj.description = arg3;
                    return;
                }

                if (!RomString.str_prefix(arg2, "ed") || !RomString.str_prefix(arg2, "extended"))
                {
                    argument = RomString.one_argument(argument, out arg3);
                    if (argument == null)
                    {
                        Comm.send_to_char(
                            "Syntax: oset <object> ed <keyword> <string>\n\r", ch);
                        return;
                    }

                    argument += "\n\r";

                    var ed = Recycle.new_extra_descr();

                    ed.keyword = arg3;
                    ed.description = argument;
                    ed.next = obj.extra_descr;
                    obj.extra_descr = ed;
                    return;
                }
            }

            Interp.do_function(ch, do_string, "");
        }

        public static void do_nochannels(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Nochannel whom?", ch);
                return;
            }

            var victim = Handler.get_char_world(ch, arg);
            if (victim == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }

            if (Handler.get_trust(victim) >= Handler.get_trust(ch))
            {
                Comm.send_to_char("You failed.\n\r", ch);
                return;
            }

            if (Bit.IS_SET(victim.comm, COMM_NOCHANNELS))
            {
                Bit.REMOVE_BIT(ref victim.comm, COMM_NOCHANNELS);
                Comm.send_to_char("The gods have restored your channel priviliges.\n\r",
                    victim);
                Comm.send_to_char("NOCHANNELS removed.\n\r", ch);
                Comm.wiznet(RomString.sprintf("$N restores channels to %s", victim.name),
                    ch, null, WIZ_PENALTIES, WIZ_SECURE, 0);
            }
            else
            {
                Bit.SET_BIT(ref victim.comm, COMM_NOCHANNELS);
                Comm.send_to_char("The gods have revoked your channel priviliges.\n\r",
                    victim);
                Comm.send_to_char("NOCHANNELS set.\n\r", ch);
                Comm.wiznet(RomString.sprintf("$N revokes %s's channels.", victim.name),
                    ch, null, WIZ_PENALTIES, WIZ_SECURE, 0);
            }
        }

        public static void do_noemote(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Noemote whom?\n\r", ch);
                return;
            }

            var victim = Handler.get_char_world(ch, arg);
            if (victim == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }

            if (Handler.get_trust(victim) >= Handler.get_trust(ch))
            {
                Comm.send_to_char("You failed.\n\r", ch);
                return;
            }

            if (Bit.IS_SET(victim.comm, COMM_NOEMOTE))
            {
                Bit.REMOVE_BIT(ref victim.comm, COMM_NOEMOTE);
                Comm.send_to_char("You can emote again.\n\r", victim);
                Comm.send_to_char("NOEMOTE removed.\n\r", ch);
                Comm.wiznet(RomString.sprintf("$N restores emotes to %s.", victim.name),
                    ch, null, WIZ_PENALTIES, WIZ_SECURE, 0);
            }
            else
            {
                Bit.SET_BIT(ref victim.comm, COMM_NOEMOTE);
                Comm.send_to_char("You can't emote!\n\r", victim);
                Comm.send_to_char("NOEMOTE set.\n\r", ch);
                Comm.wiznet(RomString.sprintf("$N revokes %s's emotes.", victim.name),
                    ch, null, WIZ_PENALTIES, WIZ_SECURE, 0);
            }
        }

        public static void do_noshout(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Noshout whom?\n\r", ch);
                return;
            }

            var victim = Handler.get_char_world(ch, arg);
            if (victim == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }

            if (Bit.IS_NPC(victim))
            {
                Comm.send_to_char("Not on NPC's.\n\r", ch);
                return;
            }

            if (Handler.get_trust(victim) >= Handler.get_trust(ch))
            {
                Comm.send_to_char("You failed.\n\r", ch);
                return;
            }

            if (Bit.IS_SET(victim.comm, COMM_NOSHOUT))
            {
                Bit.REMOVE_BIT(ref victim.comm, COMM_NOSHOUT);
                Comm.send_to_char("You can shout again.\n\r", victim);
                Comm.send_to_char("NOSHOUT removed.\n\r", ch);
                Comm.wiznet(RomString.sprintf("$N restores shouts to %s.", victim.name),
                    ch, null, WIZ_PENALTIES, WIZ_SECURE, 0);
            }
            else
            {
                Bit.SET_BIT(ref victim.comm, COMM_NOSHOUT);
                Comm.send_to_char("You can't shout!\n\r", victim);
                Comm.send_to_char("NOSHOUT set.\n\r", ch);
                Comm.wiznet(RomString.sprintf("$N revokes %s's shouts.", victim.name),
                    ch, null, WIZ_PENALTIES, WIZ_SECURE, 0);
            }
        }

        public static void do_notell(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Notell whom?", ch);
                return;
            }

            var victim = Handler.get_char_world(ch, arg);
            if (victim == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }

            if (Handler.get_trust(victim) >= Handler.get_trust(ch))
            {
                Comm.send_to_char("You failed.\n\r", ch);
                return;
            }

            if (Bit.IS_SET(victim.comm, COMM_NOTELL))
            {
                Bit.REMOVE_BIT(ref victim.comm, COMM_NOTELL);
                Comm.send_to_char("You can tell again.\n\r", victim);
                Comm.send_to_char("NOTELL removed.\n\r", ch);
                Comm.wiznet(RomString.sprintf("$N restores tells to %s.", victim.name),
                    ch, null, WIZ_PENALTIES, WIZ_SECURE, 0);
            }
            else
            {
                Bit.SET_BIT(ref victim.comm, COMM_NOTELL);
                Comm.send_to_char("You can't tell!\n\r", victim);
                Comm.send_to_char("NOTELL set.\n\r", ch);
                Comm.wiznet(RomString.sprintf("$N revokes %s's tells.", victim.name),
                    ch, null, WIZ_PENALTIES, WIZ_SECURE, 0);
            }
        }

        public static void do_snoop(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Snoop whom?\n\r", ch);
                return;
            }

            var victim = Handler.get_char_world(ch, arg);
            if (victim == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }

            if (victim.desc == null)
            {
                Comm.send_to_char("No descriptor to snoop.\n\r", ch);
                return;
            }

            if (victim == ch)
            {
                Comm.send_to_char("Cancelling all snoops.\n\r", ch);
                Comm.wiznet("$N stops being such a snoop.",
                    ch, null, WIZ_SNOOPS, WIZ_SECURE, Handler.get_trust(ch));
                for (var d = Game.descriptor_list; d != null; d = d.next)
                {
                    if (d.snoop_by == ch.desc)
                        d.snoop_by = null;
                }
                return;
            }

            if (victim.desc.snoop_by != null)
            {
                Comm.send_to_char("Busy already.\n\r", ch);
                return;
            }

            if (!Handler.is_room_owner(ch, victim.in_room) && ch.in_room != victim.in_room
                && Handler.room_is_private(victim.in_room) && !Bit.IS_TRUSTED(ch, IMPLEMENTOR))
            {
                Comm.send_to_char("That character is in a private room.\n\r", ch);
                return;
            }

            if (Handler.get_trust(victim) >= Handler.get_trust(ch)
                || Bit.IS_SET(victim.comm, COMM_SNOOP_PROOF))
            {
                Comm.send_to_char("You failed.\n\r", ch);
                return;
            }

            if (ch.desc != null)
            {
                for (var d = ch.desc.snoop_by; d != null; d = d.snoop_by)
                {
                    if (d.character == victim || d.original == victim)
                    {
                        Comm.send_to_char("No snoop loops.\n\r", ch);
                        return;
                    }
                }
            }

            victim.desc.snoop_by = ch.desc;
            Comm.wiznet(RomString.sprintf("$N starts snooping on %s",
                Bit.IS_NPC(ch) ? victim.short_descr : victim.name),
                ch, null, WIZ_SNOOPS, WIZ_SECURE, Handler.get_trust(ch));
            Comm.send_to_char("Ok.\n\r", ch);
        }

        public static void do_switch(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Switch into whom?\n\r", ch);
                return;
            }

            if (ch.desc == null)
                return;

            if (ch.desc.original != null)
            {
                Comm.send_to_char("You are already switched.\n\r", ch);
                return;
            }

            var victim = Handler.get_char_world(ch, arg);
            if (victim == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }

            if (victim == ch)
            {
                Comm.send_to_char("Ok.\n\r", ch);
                return;
            }

            if (!Bit.IS_NPC(victim))
            {
                Comm.send_to_char("You can only switch into mobiles.\n\r", ch);
                return;
            }

            if (!Handler.is_room_owner(ch, victim.in_room) && ch.in_room != victim.in_room
                && Handler.room_is_private(victim.in_room) && !Bit.IS_TRUSTED(ch, IMPLEMENTOR))
            {
                Comm.send_to_char("That character is in a private room.\n\r", ch);
                return;
            }

            if (victim.desc != null)
            {
                Comm.send_to_char("Character in use.\n\r", ch);
                return;
            }

            Comm.wiznet(RomString.sprintf("$N switches into %s", victim.short_descr),
                ch, null, WIZ_SWITCHES, WIZ_SECURE, Handler.get_trust(ch));

            ch.desc.character = victim;
            ch.desc.original = ch;
            victim.desc = ch.desc;
            ch.desc = null;
            if (ch.prompt != null)
                victim.prompt = ch.prompt;
            victim.comm = ch.comm;
            victim.lines = ch.lines;
            Comm.send_to_char("Ok.\n\r", victim);
        }

        public static void do_return(CharData ch, string argument)
        {
            if (ch.desc == null)
                return;

            if (ch.desc.original == null)
            {
                Comm.send_to_char("You aren't switched.\n\r", ch);
                return;
            }

            Comm.send_to_char(
                "You return to your original body. Type replay to see any missed tells.\n\r",
                ch);
            if (ch.prompt != null)
            {
                ch.prompt = null;
            }

            Comm.wiznet(RomString.sprintf("$N returns from %s.", ch.short_descr),
                ch.desc.original, null, WIZ_SWITCHES, WIZ_SECURE,
                Handler.get_trust(ch));
            ch.desc.character = ch.desc.original;
            ch.desc.original = null;
            ch.desc.character.desc = ch.desc;
            ch.desc = null;
        }

        public static void do_advance(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg1);
            argument = RomString.one_argument(argument, out string arg2);

            if (arg1.Length == 0 || arg2.Length == 0 || !Interp.is_number(arg2))
            {
                Comm.send_to_char("Syntax: advance <char> <level>.\n\r", ch);
                return;
            }

            var victim = Handler.get_char_world(ch, arg1);
            if (victim == null)
            {
                Comm.send_to_char("That player is not here.\n\r", ch);
                return;
            }

            if (Bit.IS_NPC(victim))
            {
                Comm.send_to_char("Not on NPC's.\n\r", ch);
                return;
            }

            int level = atoi(arg2);
            if (level < 1 || level > MAX_LEVEL)
            {
                Comm.send_to_char(RomString.sprintf("Level must be 1 to %d.\n\r", MAX_LEVEL), ch);
                return;
            }

            if (level > Handler.get_trust(ch))
            {
                Comm.send_to_char("Limited to your trust level.\n\r", ch);
                return;
            }

            if (level <= victim.level)
            {
                Comm.send_to_char("Lowering a player's level!\n\r", ch);
                Comm.send_to_char("**** OOOOHHHHHHHHHH  NNNNOOOO ****\n\r", victim);
                int temp_prac = victim.practice;
                victim.level = 1;
                victim.exp = Handler.exp_per_level(victim, victim.pcdata.points);
                victim.max_hit = 10;
                victim.max_mana = 100;
                victim.max_move = 100;
                victim.practice = 0;
                victim.hit = victim.max_hit;
                victim.mana = victim.max_mana;
                victim.move = victim.max_move;
                Update.advance_level(victim, true);
                victim.practice = temp_prac;
            }
            else
            {
                Comm.send_to_char("Raising a player's level!\n\r", ch);
                Comm.send_to_char("**** OOOOHHHHHHHHHH  YYYYEEEESSS ****\n\r", victim);
            }

            for (int iLevel = victim.level; iLevel < level; iLevel++)
            {
                victim.level += 1;
                Update.advance_level(victim, true);
            }
            Comm.send_to_char(RomString.sprintf("You are now level %d.\n\r", victim.level), victim);
            victim.exp = Handler.exp_per_level(victim, victim.pcdata.points)
                * Bit.UMAX(1, victim.level);
            victim.trust = 0;
            Save.save_char_obj(victim);
        }

        public static void do_trust(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg1);
            argument = RomString.one_argument(argument, out string arg2);

            if (arg1.Length == 0 || arg2.Length == 0 || !Interp.is_number(arg2))
            {
                Comm.send_to_char("Syntax: trust <char> <level>.\n\r", ch);
                return;
            }

            var victim = Handler.get_char_world(ch, arg1);
            if (victim == null)
            {
                Comm.send_to_char("That player is not here.\n\r", ch);
                return;
            }

            int level = atoi(arg2);
            if (level < 0 || level > MAX_LEVEL)
            {
                Comm.send_to_char(RomString.sprintf("Level must be 0 (reset) or 1 to %d.\n\r", MAX_LEVEL), ch);
                return;
            }

            if (level > Handler.get_trust(ch))
            {
                Comm.send_to_char("Limited to your trust.\n\r", ch);
                return;
            }

            victim.trust = level;
        }

        public static void do_restore(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);
            if (arg.Length == 0 || !RomString.str_cmp(arg, "room"))
            {
                for (var vch = ch.in_room.people; vch != null; vch = vch.next_in_room)
                {
                    Handler.affect_strip(vch, Gsn.plague);
                    Handler.affect_strip(vch, Gsn.poison);
                    Handler.affect_strip(vch, Gsn.blindness);
                    Handler.affect_strip(vch, Gsn.sleep);
                    Handler.affect_strip(vch, Gsn.curse);

                    vch.hit = vch.max_hit;
                    vch.mana = vch.max_mana;
                    vch.move = vch.max_move;
                    Fight.update_pos(vch);
                    Comm.act("$n has restored you.", ch, null, vch, TO_VICT);
                }

                Comm.wiznet(RomString.sprintf("$N restored room %d.", ch.in_room.vnum),
                    ch, null, WIZ_RESTORE, WIZ_SECURE, Handler.get_trust(ch));

                Comm.send_to_char("Room restored.\n\r", ch);
                return;
            }

            if (Handler.get_trust(ch) >= MAX_LEVEL - 1 && !RomString.str_cmp(arg, "all"))
            {
                for (var d = Game.descriptor_list; d != null; d = d.next)
                {
                    var victim = d.character;

                    if (victim == null || Bit.IS_NPC(victim))
                        continue;

                    Handler.affect_strip(victim, Gsn.plague);
                    Handler.affect_strip(victim, Gsn.poison);
                    Handler.affect_strip(victim, Gsn.blindness);
                    Handler.affect_strip(victim, Gsn.sleep);
                    Handler.affect_strip(victim, Gsn.curse);

                    victim.hit = victim.max_hit;
                    victim.mana = victim.max_mana;
                    victim.move = victim.max_move;
                    Fight.update_pos(victim);
                    if (victim.in_room != null)
                        Comm.act("$n has restored you.", ch, null, victim, TO_VICT);
                }
                Comm.send_to_char("All active players restored.\n\r", ch);
                return;
            }

            var vch2 = Handler.get_char_world(ch, arg);
            if (vch2 == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }

            Handler.affect_strip(vch2, Gsn.plague);
            Handler.affect_strip(vch2, Gsn.poison);
            Handler.affect_strip(vch2, Gsn.blindness);
            Handler.affect_strip(vch2, Gsn.sleep);
            Handler.affect_strip(vch2, Gsn.curse);
            vch2.hit = vch2.max_hit;
            vch2.mana = vch2.max_mana;
            vch2.move = vch2.max_move;
            Fight.update_pos(vch2);
            Comm.act("$n has restored you.", ch, null, vch2, TO_VICT);
            Comm.wiznet(RomString.sprintf("$N restored %s",
                Bit.IS_NPC(vch2) ? vch2.short_descr : vch2.name),
                ch, null, WIZ_RESTORE, WIZ_SECURE, Handler.get_trust(ch));
            Comm.send_to_char("Ok.\n\r", ch);
        }

        public static void do_force(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);

            if (arg.Length == 0 || argument.Length == 0)
            {
                Comm.send_to_char("Force whom to do what?\n\r", ch);
                return;
            }

            RomString.one_argument(argument, out string arg2);

            if (!RomString.str_cmp(arg2, "delete") || !RomString.str_prefix(arg2, "mob"))
            {
                Comm.send_to_char("That will NOT be done.\n\r", ch);
                return;
            }

            string buf = RomString.sprintf("$n forces you to '%s'.", argument);

            if (!RomString.str_cmp(arg, "all"))
            {
                if (Handler.get_trust(ch) < MAX_LEVEL - 3)
                {
                    Comm.send_to_char("Not at your level!\n\r", ch);
                    return;
                }

                for (var desc = Game.descriptor_list; desc != null; )
                {
                    var desc_next = desc.next;

                    if (desc.connected == CON_PLAYING &&
                        Handler.get_trust(desc.character) < Handler.get_trust(ch))
                    {
                        Comm.act(buf, ch, null, desc.character, TO_VICT);
                        Interp.interpret(desc.character, argument);
                    }
                    desc = desc_next;
                }
            }
            else if (!RomString.str_cmp(arg, "players"))
            {
                if (Handler.get_trust(ch) < MAX_LEVEL - 2)
                {
                    Comm.send_to_char("Not at your level!\n\r", ch);
                    return;
                }

                for (var vch = Game.char_list; vch != null; )
                {
                    var vch_next = vch.next;

                    if (!Bit.IS_NPC(vch) && Handler.get_trust(vch) < Handler.get_trust(ch)
                        && vch.level < LEVEL_HERO)
                    {
                        Comm.act(buf, ch, null, vch, TO_VICT);
                        Interp.interpret(vch, argument);
                    }
                    vch = vch_next;
                }
            }
            else if (!RomString.str_cmp(arg, "gods"))
            {
                if (Handler.get_trust(ch) < MAX_LEVEL - 2)
                {
                    Comm.send_to_char("Not at your level!\n\r", ch);
                    return;
                }

                for (var vch = Game.char_list; vch != null; )
                {
                    var vch_next = vch.next;

                    if (!Bit.IS_NPC(vch) && Handler.get_trust(vch) < Handler.get_trust(ch)
                        && vch.level >= LEVEL_HERO)
                    {
                        Comm.act(buf, ch, null, vch, TO_VICT);
                        Interp.interpret(vch, argument);
                    }
                    vch = vch_next;
                }
            }
            else
            {
                var victim = Handler.get_char_world(ch, arg);
                if (victim == null)
                {
                    Comm.send_to_char("They aren't here.\n\r", ch);
                    return;
                }

                if (victim == ch)
                {
                    Comm.send_to_char("Aye aye, right away!\n\r", ch);
                    return;
                }

                if (!Handler.is_room_owner(ch, victim.in_room)
                    && ch.in_room != victim.in_room
                    && Handler.room_is_private(victim.in_room)
                    && !Bit.IS_TRUSTED(ch, IMPLEMENTOR))
                {
                    Comm.send_to_char("That character is in a private room.\n\r", ch);
                    return;
                }

                if (Handler.get_trust(victim) >= Handler.get_trust(ch))
                {
                    Comm.send_to_char("Do it yourself!\n\r", ch);
                    return;
                }

                if (!Bit.IS_NPC(victim) && Handler.get_trust(ch) < MAX_LEVEL - 3)
                {
                    Comm.send_to_char("Not at your level!\n\r", ch);
                    return;
                }

                Comm.act(buf, ch, null, victim, TO_VICT);
                Interp.interpret(victim, argument);
            }

            Comm.send_to_char("Ok.\n\r", ch);
        }

        static bool obj_check(CharData ch, ObjData obj)
        {
            if (Bit.IS_TRUSTED(ch, GOD)
                || (Bit.IS_TRUSTED(ch, IMMORTAL) && obj.level <= 20
                    && obj.cost <= 1000) || (Bit.IS_TRUSTED(ch, DEMI)
                    && obj.level <= 10 && obj.cost <= 500)
                || (Bit.IS_TRUSTED(ch, ANGEL) && obj.level <= 5 && obj.cost <= 250)
                || (Bit.IS_TRUSTED(ch, AVATAR) && obj.level == 0 && obj.cost <= 100))
                return true;
            else
                return false;
        }

        static void recursive_clone(CharData ch, ObjData obj, ObjData clone)
        {
            for (var c_obj = obj.contains; c_obj != null; c_obj = c_obj.next_content)
            {
                if (obj_check(ch, c_obj))
                {
                    var t_obj = Db.create_object(c_obj.pIndexData, 0);
                    Db.clone_object(c_obj, t_obj);
                    Handler.obj_to_obj(t_obj, clone);
                    recursive_clone(ch, c_obj, t_obj);
                }
            }
        }

        public static void do_clone(CharData ch, string argument)
        {
            string rest = RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Clone what?\n\r", ch);
                return;
            }

            CharData mob;
            ObjData obj;
            if (!RomString.str_prefix(arg, "object"))
            {
                mob = null;
                obj = Handler.get_obj_here(ch, rest);
                if (obj == null)
                {
                    Comm.send_to_char("You don't see that here.\n\r", ch);
                    return;
                }
            }
            else if (!RomString.str_prefix(arg, "mobile") || !RomString.str_prefix(arg, "character"))
            {
                obj = null;
                mob = Handler.get_char_room(ch, rest);
                if (mob == null)
                {
                    Comm.send_to_char("You don't see that here.\n\r", ch);
                    return;
                }
            }
            else
            {
                mob = Handler.get_char_room(ch, argument);
                obj = Handler.get_obj_here(ch, argument);
                if (mob == null && obj == null)
                {
                    Comm.send_to_char("You don't see that here.\n\r", ch);
                    return;
                }
            }

            if (obj != null)
            {
                if (!obj_check(ch, obj))
                {
                    Comm.send_to_char
                        ("Your powers are not great enough for such a task.\n\r", ch);
                    return;
                }

                var clone = Db.create_object(obj.pIndexData, 0);
                Db.clone_object(obj, clone);
                if (obj.carried_by != null)
                    Handler.obj_to_char(clone, ch);
                else
                    Handler.obj_to_room(clone, ch.in_room);
                recursive_clone(ch, obj, clone);

                Comm.act("$n has created $p.", ch, clone, null, TO_ROOM);
                Comm.act("You clone $p.", ch, clone, null, TO_CHAR);
                Comm.wiznet("$N clones $p.", ch, clone, WIZ_LOAD, WIZ_SECURE,
                    Handler.get_trust(ch));
                return;
            }
            else if (mob != null)
            {
                if (!Bit.IS_NPC(mob))
                {
                    Comm.send_to_char("You can only clone mobiles.\n\r", ch);
                    return;
                }

                if ((mob.level > 20 && !Bit.IS_TRUSTED(ch, GOD))
                    || (mob.level > 10 && !Bit.IS_TRUSTED(ch, IMMORTAL))
                    || (mob.level > 5 && !Bit.IS_TRUSTED(ch, DEMI))
                    || (mob.level > 0 && !Bit.IS_TRUSTED(ch, ANGEL))
                    || !Bit.IS_TRUSTED(ch, AVATAR))
                {
                    Comm.send_to_char
                        ("Your powers are not great enough for such a task.\n\r", ch);
                    return;
                }

                var clone = Db.create_mobile(mob.pIndexData);
                Db.clone_mobile(mob, clone);

                for (obj = mob.carrying; obj != null; obj = obj.next_content)
                {
                    if (obj_check(ch, obj))
                    {
                        var new_obj = Db.create_object(obj.pIndexData, 0);
                        Db.clone_object(obj, new_obj);
                        recursive_clone(ch, obj, new_obj);
                        Handler.obj_to_char(new_obj, clone);
                        new_obj.wear_loc = obj.wear_loc;
                    }
                }
                Handler.char_to_room(clone, ch.in_room);
                Comm.act("$n has created $N.", ch, null, clone, TO_ROOM);
                Comm.act("You clone $N.", ch, null, clone, TO_CHAR);
                Comm.wiznet(RomString.sprintf("$N clones %s.", clone.short_descr),
                    ch, null, WIZ_LOAD, WIZ_SECURE, Handler.get_trust(ch));
                return;
            }
        }

        public static void do_owhere(CharData ch, string argument)
        {
            bool found = false;
            int number = 0;
            int max_found = 200;
            var buffer = new System.Text.StringBuilder();

            if (argument.Length == 0)
            {
                Comm.send_to_char("Find what?\n\r", ch);
                return;
            }

            for (var obj = Game.object_list; obj != null; obj = obj.next)
            {
                if (!Handler.can_see_obj(ch, obj) || !Handler.is_name(argument, obj.name)
                    || ch.level < obj.level)
                    continue;

                found = true;
                number++;

                var in_obj = obj;
                for (; in_obj.in_obj != null; in_obj = in_obj.in_obj) ;

                string buf;
                if (in_obj.carried_by != null && Handler.can_see(ch, in_obj.carried_by)
                    && in_obj.carried_by.in_room != null)
                    buf = RomString.sprintf("%3d) %s is carried by %s [Room %d]\n\r",
                        number, obj.short_descr, Handler.PERS(in_obj.carried_by, ch),
                        in_obj.carried_by.in_room.vnum);
                else if (in_obj.in_room != null
                    && Handler.can_see_room(ch, in_obj.in_room))
                    buf = RomString.sprintf(
                        "%3d) %s is in %s [Room %d]\n\r",
                        number,
                        obj.short_descr,
                        in_obj.in_room.name,
                        in_obj.in_room.vnum);
                else
                    buf = RomString.sprintf("%3d) %s is somewhere\n\r", number,
                        obj.short_descr);

                if (buf.Length > 0)
                    buf = Bit.UPPER(buf[0]) + buf.Substring(1);
                buffer.Append(buf);

                if (number >= max_found)
                    break;
            }

            if (!found)
                Comm.send_to_char("Nothing like that in heaven or earth.\n\r", ch);
            else
                Comm.page_to_char(buffer.ToString(), ch);
        }

        public static void do_mwhere(CharData ch, string argument)
        {
            int count = 0;

            if (argument.Length == 0)
            {
                var buffer = new System.Text.StringBuilder();
                for (var d = Game.descriptor_list; d != null; d = d.next)
                {
                    if (d.character != null && d.connected == CON_PLAYING
                        && d.character.in_room != null && Handler.can_see(ch, d.character)
                        && Handler.can_see_room(ch, d.character.in_room))
                    {
                        var victim = d.character;
                        count++;
                        string buf;
                        if (d.original != null)
                            buf = RomString.sprintf(
                                "%3d) %s (in the body of %s) is in %s [%d]\n\r",
                                count, d.original.name, victim.short_descr,
                                victim.in_room.name, victim.in_room.vnum);
                        else
                            buf = RomString.sprintf("%3d) %s is in %s [%d]\n\r", count,
                                victim.name, victim.in_room.name,
                                victim.in_room.vnum);
                        buffer.Append(buf);
                    }
                }

                Comm.page_to_char(buffer.ToString(), ch);
                return;
            }

            bool found = false;
            var foundBuf = new System.Text.StringBuilder();
            for (var victim = Game.char_list; victim != null; victim = victim.next)
            {
                if (victim.in_room != null && Handler.is_name(argument, victim.name))
                {
                    found = true;
                    count++;
                    foundBuf.Append(RomString.sprintf("%3d) [%5d] %-28s [%5d] %s\n\r", count,
                        Bit.IS_NPC(victim) ? victim.pIndexData.vnum : 0,
                        Bit.IS_NPC(victim) ? victim.short_descr : victim.name,
                        victim.in_room.vnum, victim.in_room.name));
                }
            }

            if (!found)
                Comm.act("You didn't find any $T.", ch, null, argument, TO_CHAR);
            else
                Comm.page_to_char(foundBuf.ToString(), ch);
        }

        public static void do_sockets(CharData ch, string argument)
        {
            var buf = new System.Text.StringBuilder();
            int count = 0;

            RomString.one_argument(argument, out string arg);
            for (var d = Game.descriptor_list; d != null; d = d.next)
            {
                if (d.character != null && Handler.can_see(ch, d.character)
                    && (arg.Length == 0 || Handler.is_name(arg, d.character.name)
                        || (d.original != null && Handler.is_name(arg, d.original.name))))
                {
                    count++;
                    buf.Append(RomString.sprintf("[%3d %2d] %s@%s\n\r",
                        d.descriptor,
                        d.connected,
                        d.original != null ? d.original.name :
                        d.character != null ? d.character.name : "(none)", d.host));
                }
            }
            if (count == 0)
            {
                Comm.send_to_char("No one by that name is connected.\n\r", ch);
                return;
            }

            buf.Append(RomString.sprintf("%d user%s\n\r", count, count == 1 ? "" : "s"));
            Comm.page_to_char(buf.ToString(), ch);
        }

        public static void do_copyover(CharData ch, string argument)
        {
            CopyOver.DoCopyover(ch);
        }

        public static void copyover_recover()
        {
            CopyOver.Recover();
        }

        public static void do_qmconfig(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            if (argument.Length == 0)
            {
                Comm.send_to_char("Valid qmconfig options are:\n\r", ch);
                Comm.send_to_char("    show       (shows current status of toggles)\n\r", ch);
                Comm.send_to_char("    ansiprompt [on|off]\n\r", ch);
                Comm.send_to_char("    ansicolor  [on|off]\n\r", ch);
                Comm.send_to_char("    telnetga   [on|off]\n\r", ch);
                Comm.send_to_char("    read\n\r", ch);
                return;
            }

            argument = RomString.one_argument(argument, out string arg1);
            argument = RomString.one_argument(argument, out string arg2);

            if (!RomString.str_prefix(arg1, "read"))
            {
                qmconfig_read();
                return;
            }

            if (!RomString.str_prefix(arg1, "show"))
            {
                Comm.send_to_char(RomString.sprintf("ANSI prompt: %s", Game.mud_ansiprompt != 0 ? "{GON{x\n\r" : "{ROFF{x\n\r"), ch);
                Comm.send_to_char(RomString.sprintf("ANSI color : %s", Game.mud_ansicolor != 0 ? "{GON{x\n\r" : "{ROFF{x\n\r"), ch);
                Comm.send_to_char(RomString.sprintf("IP Address : %s\n\r", Game.mud_ipaddress), ch);
                Comm.send_to_char(RomString.sprintf("Telnet GA  : %s", Game.mud_telnetga != 0 ? "{GON{x\n\r" : "{ROFF{x\n\r"), ch);
                return;
            }

            if (!RomString.str_prefix(arg1, "ansiprompt"))
            {
                if (!RomString.str_prefix(arg2, "on"))
                {
                    Game.mud_ansiprompt = 1;
                    Comm.send_to_char("New logins will now get an ANSI color prompt.\n\r", ch);
                    return;
                }
                else if (!RomString.str_prefix(arg2, "off"))
                {
                    Game.mud_ansiprompt = 0;
                    Comm.send_to_char("New logins will not get an ANSI color prompt.\n\r", ch);
                    return;
                }

                Comm.send_to_char("Valid arguments are \"on\" and \"off\".\n\r", ch);
                return;
            }

            if (!RomString.str_prefix(arg1, "ansicolor"))
            {
                if (!RomString.str_prefix(arg2, "on"))
                {
                    Game.mud_ansicolor = 1;
                    Comm.send_to_char("New players will have color enabled.\n\r", ch);
                    return;
                }
                else if (!RomString.str_prefix(arg2, "off"))
                {
                    Game.mud_ansicolor = 0;
                    Comm.send_to_char("New players will not have color enabled.\n\r", ch);
                    return;
                }

                Comm.send_to_char("Valid arguments are \"on\" and \"off\".\n\r", ch);
                return;
            }

            if (!RomString.str_prefix(arg1, "telnetga"))
            {
                if (!RomString.str_prefix(arg2, "on"))
                {
                    Game.mud_telnetga = 1;
                    Comm.send_to_char("Telnet GA will be enabled for new players.\n\r", ch);
                    return;
                }
                else if (!RomString.str_prefix(arg2, "off"))
                {
                    Game.mud_telnetga = 0;
                    Comm.send_to_char("Telnet GA will be disabled for new players.\n\r", ch);
                    return;
                }

                Comm.send_to_char("Valid arguments are \"on\" and \"off\".\n\r", ch);
                return;
            }

            Comm.send_to_char("I have no clue what you are trying to do...\n\r", ch);
        }

        static void qmconfig_read()
        {
            Db.log_f("Loading configuration settings from ../area/qmconfig.rc.");

            var path = Path.Combine(Game.area_dir, "qmconfig.rc");
            if (!File.Exists(path))
            {
                Db.log_f("qmconfig.rc not found. Using compiled-in defaults.");
                return;
            }

            using var fp = new AreaReader(path);
            for (;;)
            {
                string word = fp.Eof() ? "END" : fp.fread_word();
                bool fMatch = false;

                switch (word.Length == 0 ? '\0' : Bit.UPPER(word[0]))
                {
                    case '#':
                        fMatch = true;
                        fp.fread_to_eol();
                        break;
                    case '*':
                        fMatch = true;
                        fp.fread_to_eol();
                        break;
                    case 'A':
                        if (!RomString.str_cmp(word, "Ansicolor"))
                        {
                            Game.mud_ansicolor = fp.fread_number();
                            fMatch = true;
                        }
                        if (!RomString.str_cmp(word, "Ansiprompt"))
                        {
                            Game.mud_ansiprompt = fp.fread_number();
                            fMatch = true;
                        }
                        break;
                    case 'E':
                        if (!RomString.str_cmp(word, "END"))
                        {
                            Db.log_f("Settings have been read from ../area/qmconfig.rc");
                            return;
                        }
                        break;
                    case 'T':
                        if (!RomString.str_cmp(word, "Telnetga"))
                        {
                            Game.mud_telnetga = fp.fread_number();
                            fMatch = true;
                        }
                        break;
                }
                if (!fMatch)
                {
                    Db.log_f("qmconfig_read: no match for %s!", word);
                    fp.fread_to_eol();
                }
            }
        }

        static int atoi(string s) => Interp.atoi(s);
    }
}
