using static Rom24.Merc;

namespace Rom24
{
    public delegate bool OlcFun(CharData ch, string argument);

    public static partial class Olc
    {
        public const int ED_NONE = 0;
        public const int ED_AREA = 1;
        public const int ED_ROOM = 2;
        public const int ED_OBJECT = 3;
        public const int ED_MOBILE = 4;
        public const int ED_MPCODE = 5;
        public const int ED_HELP = 6;

        static readonly (string name, DoFun do_fun)[] editor_table =
        {
            ("area", do_aedit),
            ("room", do_redit),
            ("object", do_oedit),
            ("mobile", do_medit),
            ("mpcode", do_mpedit),
            ("hedit", do_hedit),
            (null, null)
        };

        public static AreaData get_area_data(int vnum)
        {
            for (var pArea = Game.area_first; pArea != null; pArea = pArea.next)
            {
                if (pArea.vnum == vnum)
                    return pArea;
            }
            return null;
        }

        public static AreaData get_vnum_area(int vnum)
        {
            for (var pArea = Game.area_first; pArea != null; pArea = pArea.next)
            {
                if (vnum >= pArea.min_vnum && vnum <= pArea.max_vnum)
                    return pArea;
            }
            return null;
        }

        public static bool edit_done(CharData ch)
        {
            ch.desc.pEdit = null;
            ch.desc.editor = 0;
            return false;
        }

        public static void do_olc(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            argument = RomString.one_argument(argument, out string command);

            if (command.Length == 0)
            {
                Interp.do_help(ch, "olc");
                return;
            }

            for (int cmd = 0; editor_table[cmd].name != null; cmd++)
            {
                if (!RomString.str_prefix(command, editor_table[cmd].name))
                {
                    editor_table[cmd].do_fun(ch, argument);
                    return;
                }
            }

            Interp.do_help(ch, "olc");
        }

        public static void do_aedit(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            var pArea = ch.in_room.area;
            argument = RomString.one_argument(argument, out string arg);

            if (Interp.is_number(arg))
            {
                int value = Interp.atoi(arg);
                pArea = get_area_data(value);
                if (pArea == null)
                {
                    Comm.send_to_char("That area vnum does not exist.\n\r", ch);
                    return;
                }
            }
            else if (!RomString.str_cmp(arg, "create"))
            {
                if (ch.pcdata.security < 9)
                {
                    Comm.send_to_char("AEdit : Insufficient security to create area.\n\r",
                        ch);
                    return;
                }

                aedit_create(ch, "");
                ch.desc.editor = ED_AREA;
                return;
            }

            if (!Bit.IS_BUILDER(ch, pArea))
            {
                Comm.send_to_char("Insufficient security to edit areas.\n\r", ch);
                return;
            }

            ch.desc.pEdit = pArea;
            ch.desc.editor = ED_AREA;
        }

        static bool aedit_create(CharData ch, string argument)
        {
            var pArea = Recycle.new_area();
            Game.area_last.next = pArea;
            Game.area_last = pArea;
            ch.desc.pEdit = pArea;

            Bit.SET_BIT(ref pArea.area_flags, AREA_ADDED);
            Comm.send_to_char("Area Created.\n\r", ch);
            return false;
        }

        public static void do_redit(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            argument = RomString.one_argument(argument, out string arg1);
            var pRoom = ch.in_room;

            if (!RomString.str_cmp(arg1, "reset"))
            {
                if (!Bit.IS_BUILDER(ch, pRoom.area))
                {
                    Comm.send_to_char("Insufficient security to modify room.\n\r", ch);
                    return;
                }

                Db.reset_room(pRoom);
                Comm.send_to_char("Room reset.\n\r", ch);
                return;
            }
            else if (!RomString.str_cmp(arg1, "create"))
            {
                if (argument.Length == 0 || Interp.atoi(argument) == 0)
                {
                    Comm.send_to_char("Syntax:  edit room create [vnum]\n\r", ch);
                    return;
                }

                if (redit_create(ch, argument))
                {
                    ch.desc.editor = ED_ROOM;
                    Handler.char_from_room(ch);
                    Handler.char_to_room(ch, (RoomIndexData)ch.desc.pEdit);
                    Bit.SET_BIT(ref ((RoomIndexData)ch.desc.pEdit).area.area_flags,
                        AREA_CHANGED);
                }
                return;
            }
            else if (!Bit.IS_NULLSTR(arg1))
            {
                int rv = Interp.atoi(arg1);
                pRoom = Handler.get_room_index(rv);

                if (pRoom == null)
                {
                    Comm.send_to_char("REdit : Nonexistant room.\n\r", ch);
                    return;
                }

                if (!Bit.IS_BUILDER(ch, pRoom.area))
                {
                    Comm.send_to_char("REdit : Insufficient security to modify room.\n\r",
                        ch);
                    return;
                }

                Handler.char_from_room(ch);
                Handler.char_to_room(ch, pRoom);
            }

            if (!Bit.IS_BUILDER(ch, pRoom.area))
            {
                Comm.send_to_char("REdit : Insufficient security to modify room.\n\r",
                    ch);
                return;
            }

            ch.desc.pEdit = pRoom;
            ch.desc.editor = ED_ROOM;
        }

        static bool redit_create(CharData ch, string argument)
        {
            int value = Interp.atoi(argument);
            if (argument.Length == 0 || value <= 0)
            {
                Comm.send_to_char("Syntax:  create [vnum > 0]\n\r", ch);
                return false;
            }

            var pArea = get_vnum_area(value);
            if (pArea == null)
            {
                Comm.send_to_char("REdit:  That vnum is not assigned an area.\n\r", ch);
                return false;
            }

            if (!Bit.IS_BUILDER(ch, pArea))
            {
                Comm.send_to_char("REdit:  Vnum in an area you cannot build in.\n\r", ch);
                return false;
            }

            if (Handler.get_room_index(value) != null)
            {
                Comm.send_to_char("REdit:  Room vnum already exists.\n\r", ch);
                return false;
            }

            var pRoom = Recycle.new_room_index();
            pRoom.area = pArea;
            pRoom.vnum = value;

            if (value > Game.top_vnum_room)
                Game.top_vnum_room = value;

            int iHash = value % MAX_KEY_HASH;
            pRoom.next = Game.room_index_hash[iHash];
            Game.room_index_hash[iHash] = pRoom;
            ch.desc.pEdit = pRoom;

            Comm.send_to_char("Room created.\n\r", ch);
            return true;
        }

        public static void do_oedit(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            argument = RomString.one_argument(argument, out string arg1);

            if (Interp.is_number(arg1))
            {
                int value = Interp.atoi(arg1);
                var pObj = Handler.get_obj_index(value);
                if (pObj == null)
                {
                    Comm.send_to_char("OEdit:  That vnum does not exist.\n\r", ch);
                    return;
                }

                if (!Bit.IS_BUILDER(ch, pObj.area))
                {
                    Comm.send_to_char("Insufficient security to modify objects.\n\r", ch);
                    return;
                }

                ch.desc.pEdit = pObj;
                ch.desc.editor = ED_OBJECT;
                return;
            }
            else
            {
                if (!RomString.str_cmp(arg1, "create"))
                {
                    int value = Interp.atoi(argument);
                    if (argument.Length == 0 || value == 0)
                    {
                        Comm.send_to_char("Syntax:  edit object create [vnum]\n\r", ch);
                        return;
                    }

                    var pArea = get_vnum_area(value);

                    if (pArea == null)
                    {
                        Comm.send_to_char
                            ("OEdit:  That vnum is not assigned an area.\n\r", ch);
                        return;
                    }

                    if (!Bit.IS_BUILDER(ch, pArea))
                    {
                        Comm.send_to_char("Insufficient security to modify objects.\n\r",
                            ch);
                        return;
                    }

                    if (oedit_create(ch, argument))
                    {
                        Bit.SET_BIT(ref pArea.area_flags, AREA_CHANGED);
                        ch.desc.editor = ED_OBJECT;
                    }
                    return;
                }
            }

            Comm.send_to_char("OEdit:  There is no default object to edit.\n\r", ch);
        }

        static bool oedit_create(CharData ch, string argument)
        {
            int value = Interp.atoi(argument);
            if (argument.Length == 0 || value == 0)
            {
                Comm.send_to_char("Syntax:  oedit create [vnum]\n\r", ch);
                return false;
            }

            var pArea = get_vnum_area(value);
            if (pArea == null)
            {
                Comm.send_to_char("OEdit:  That vnum is not assigned an area.\n\r", ch);
                return false;
            }

            if (!Bit.IS_BUILDER(ch, pArea))
            {
                Comm.send_to_char("OEdit:  Vnum in an area you cannot build in.\n\r", ch);
                return false;
            }

            if (Handler.get_obj_index(value) != null)
            {
                Comm.send_to_char("OEdit:  Object vnum already exists.\n\r", ch);
                return false;
            }

            var pObj = Recycle.new_obj_index();
            pObj.vnum = value;
            pObj.area = pArea;

            if (value > Game.top_vnum_obj)
                Game.top_vnum_obj = value;

            int iHash = value % MAX_KEY_HASH;
            pObj.next = Game.obj_index_hash[iHash];
            Game.obj_index_hash[iHash] = pObj;
            ch.desc.pEdit = pObj;

            Comm.send_to_char("Object Created.\n\r", ch);
            return true;
        }

        public static void do_medit(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg1);

            if (Bit.IS_NPC(ch))
                return;

            if (Interp.is_number(arg1))
            {
                int value = Interp.atoi(arg1);
                var pMob = Handler.get_mob_index(value);
                if (pMob == null)
                {
                    Comm.send_to_char("MEdit:  That vnum does not exist.\n\r", ch);
                    return;
                }

                if (!Bit.IS_BUILDER(ch, pMob.area))
                {
                    Comm.send_to_char("Insufficient security to modify mobs.\n\r", ch);
                    return;
                }

                ch.desc.pEdit = pMob;
                ch.desc.editor = ED_MOBILE;
                return;
            }
            else
            {
                if (!RomString.str_cmp(arg1, "create"))
                {
                    int value = Interp.atoi(argument);
                    if (arg1.Length == 0 || value == 0)
                    {
                        Comm.send_to_char("Syntax:  edit mobile create [vnum]\n\r", ch);
                        return;
                    }

                    var pArea = get_vnum_area(value);

                    if (pArea == null)
                    {
                        Comm.send_to_char
                            ("OEdit:  That vnum is not assigned an area.\n\r", ch);
                        return;
                    }

                    if (!Bit.IS_BUILDER(ch, pArea))
                    {
                        Comm.send_to_char("Insufficient security to modify mobs.\n\r",
                            ch);
                        return;
                    }

                    if (medit_create(ch, argument))
                    {
                        Bit.SET_BIT(ref pArea.area_flags, AREA_CHANGED);
                        ch.desc.editor = ED_MOBILE;
                    }
                    return;
                }
            }

            Comm.send_to_char("MEdit:  There is no default mobile to edit.\n\r", ch);
        }

        static bool medit_create(CharData ch, string argument)
        {
            int value = Interp.atoi(argument);
            if (argument.Length == 0 || value == 0)
            {
                Comm.send_to_char("Syntax:  medit create [vnum]\n\r", ch);
                return false;
            }

            var pArea = get_vnum_area(value);

            if (pArea == null)
            {
                Comm.send_to_char("MEdit:  That vnum is not assigned an area.\n\r", ch);
                return false;
            }

            if (!Bit.IS_BUILDER(ch, pArea))
            {
                Comm.send_to_char("MEdit:  Vnum in an area you cannot build in.\n\r", ch);
                return false;
            }

            if (Handler.get_mob_index(value) != null)
            {
                Comm.send_to_char("MEdit:  Mobile vnum already exists.\n\r", ch);
                return false;
            }

            var pMob = Recycle.new_mob_index();
            pMob.vnum = value;
            pMob.area = pArea;

            if (value > Game.top_vnum_mob)
                Game.top_vnum_mob = value;

            pMob.act = ACT_IS_NPC;
            int iHash = value % MAX_KEY_HASH;
            pMob.next = Game.mob_index_hash[iHash];
            Game.mob_index_hash[iHash] = pMob;
            ch.desc.pEdit = pMob;

            Comm.send_to_char("Mobile Created.\n\r", ch);
            return true;
        }

        public static void do_mpedit(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string command);

            if (Interp.is_number(command))
            {
                int vnum = Interp.atoi(command);
                var pMcode = Db.get_mprog_index(vnum);
                if (pMcode == null)
                {
                    Comm.send_to_char("MPEdit : That vnum does not exist.\n\r", ch);
                    return;
                }

                var ad = get_vnum_area(vnum);

                if (ad == null)
                {
                    Comm.send_to_char("MPEdit : VNUM no asignado a ningun area.\n\r", ch);
                    return;
                }

                if (!Bit.IS_BUILDER(ch, ad))
                {
                    Comm.send_to_char
                        ("MPEdit : Insuficiente seguridad para editar area.\n\r", ch);
                    return;
                }

                ch.desc.pEdit = pMcode;
                ch.desc.editor = ED_MPCODE;
                return;
            }

            if (!RomString.str_cmp(command, "create"))
            {
                if (argument.Length == 0)
                {
                    Comm.send_to_char("Sintaxis : mpedit create [vnum]\n\r", ch);
                    return;
                }

                mpedit_create(ch, argument);
                return;
            }

            Comm.send_to_char("Sintaxis : mpedit [vnum]\n\r", ch);
            Comm.send_to_char("           mpedit create [vnum]\n\r", ch);
        }

        static bool mpedit_create(CharData ch, string argument)
        {
            int value = Interp.atoi(argument);
            if (Bit.IS_NULLSTR(argument) || value < 1)
            {
                Comm.send_to_char("Sintaxis : mpedit create [vnum]\n\r", ch);
                return false;
            }

            var ad = get_vnum_area(value);

            if (ad == null)
            {
                Comm.send_to_char("MPEdit : VNUM no asignado a ningun area.\n\r", ch);
                return false;
            }

            if (!Bit.IS_BUILDER(ch, ad))
            {
                Comm.send_to_char
                    ("MPEdit : Insuficiente seguridad para crear MobProgs.\n\r", ch);
                return false;
            }

            if (Db.get_mprog_index(value) != null)
            {
                Comm.send_to_char("MPEdit: Code vnum already exists.\n\r", ch);
                return false;
            }

            var pMcode = Recycle.new_mpcode();
            pMcode.vnum = value;
            pMcode.next = Game.mprog_list;
            Game.mprog_list = pMcode;
            ch.desc.pEdit = pMcode;
            ch.desc.editor = ED_MPCODE;

            Comm.send_to_char("MobProgram Code Created.\n\r", ch);
            return true;
        }

        public static void do_hedit(CharData ch, string argument)
        {
            string arg1 = argument;
            bool found = false;

            if (argument.Length != 0)
            {
                string argall = "";
                while (argument.Length != 0)
                {
                    argument = RomString.one_argument(argument, out string argone);
                    if (argall.Length != 0)
                        argall += " ";
                    argall += argone;
                }
                for (var pHelp = Game.help_first; pHelp != null; pHelp = pHelp.next)
                {
                    if (Handler.is_name(argall, pHelp.keyword))
                    {
                        ch.desc.pEdit = pHelp;
                        ch.desc.editor = ED_HELP;
                        found = true;
                        return;
                    }
                }
            }
            if (!found)
            {
                argument = RomString.one_argument(arg1, out arg1);

                if (!RomString.str_cmp(arg1, "new"))
                {
                    if (argument.Length == 0)
                    {
                        Comm.send_to_char("Syntax: edit help new [topic]\n\r", ch);
                        return;
                    }
                    if (hedit_new(ch, argument))
                        ch.desc.editor = ED_HELP;
                    return;
                }
            }
            Comm.send_to_char("HEdit:  There is no default help to edit.\n\r", ch);
        }

        public static void do_resets(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg1);
            argument = RomString.one_argument(argument, out string arg2);
            argument = RomString.one_argument(argument, out string arg3);
            argument = RomString.one_argument(argument, out string arg4);
            argument = RomString.one_argument(argument, out string arg5);
            argument = RomString.one_argument(argument, out string arg6);
            argument = RomString.one_argument(argument, out string arg7);

            if (!Bit.IS_BUILDER(ch, ch.in_room.area))
            {
                Comm.send_to_char("Resets: Invalid security for editing this area.\n\r",
                    ch);
                return;
            }

            if (arg1.Length == 0)
            {
                if (ch.in_room.reset_first != null)
                {
                    Comm.send_to_char("Resets: M = mobile, R = room, O = object, "
                        + "P = pet, S = shopkeeper\n\r", ch);
                    display_resets(ch);
                }
                else
                    Comm.send_to_char("No resets in this room.\n\r", ch);
            }

            if (Interp.is_number(arg1))
            {
                var pRoom = ch.in_room;
                ResetData pReset = null;

                if (!RomString.str_cmp(arg2, "delete"))
                {
                    int insert_loc = Interp.atoi(arg1);

                    if (ch.in_room.reset_first == null)
                    {
                        Comm.send_to_char("No resets in this area.\n\r", ch);
                        return;
                    }

                    if (insert_loc - 1 <= 0)
                    {
                        pReset = pRoom.reset_first;
                        pRoom.reset_first = pRoom.reset_first.next;
                        if (pRoom.reset_first == null)
                            pRoom.reset_last = null;
                    }
                    else
                    {
                        int iReset = 0;
                        ResetData prev = null;

                        for (pReset = pRoom.reset_first;
                             pReset != null; pReset = pReset.next)
                        {
                            if (++iReset == insert_loc)
                                break;
                            prev = pReset;
                        }

                        if (pReset == null)
                        {
                            Comm.send_to_char("Reset not found.\n\r", ch);
                            return;
                        }

                        if (prev != null)
                            prev.next = prev.next.next;
                        else
                            pRoom.reset_first = pRoom.reset_first.next;

                        for (pRoom.reset_last = pRoom.reset_first;
                             pRoom.reset_last.next != null;
                             pRoom.reset_last = pRoom.reset_last.next) ;
                    }

                    Recycle.free_reset_data(pReset);
                    Comm.send_to_char("Reset deleted.\n\r", ch);
                }
                else if ((!RomString.str_cmp(arg2, "mob") && Interp.is_number(arg3))
                    || (!RomString.str_cmp(arg2, "obj") && Interp.is_number(arg3)))
                {
                    if (!RomString.str_cmp(arg2, "mob"))
                    {
                        int mv = Interp.atoi(arg3);
                        if (Handler.get_mob_index(Interp.is_number(arg3) ? mv : 1) == null)
                        {
                            Comm.send_to_char("Mob doesn't exist.\n\r", ch);
                            return;
                        }
                        pReset = Recycle.new_reset_data();
                        pReset.command = 'M';
                        pReset.arg1 = mv;
                        int a4 = Interp.atoi(arg4);
                        pReset.arg2 = Interp.is_number(arg4) ? a4 : 1;
                        pReset.arg3 = ch.in_room.vnum;
                        int a5 = Interp.atoi(arg5);
                        pReset.arg4 = Interp.is_number(arg5) ? a5 : 1;
                    }
                    else if (!RomString.str_cmp(arg2, "obj"))
                    {
                        pReset = Recycle.new_reset_data();
                        int ov = Interp.atoi(arg3);
                        pReset.arg1 = ov;
                        if (!RomString.str_prefix(arg4, "inside"))
                        {
                            int iv = Interp.atoi(arg5);
                            var temp = Handler.get_obj_index(Interp.is_number(arg5) ? iv : 1);
                            if ((temp.item_type != ITEM_CONTAINER) &&
                                (temp.item_type != ITEM_CORPSE_NPC))
                            {
                                Comm.send_to_char("Object 2 is not a container.\n\r", ch);
                                return;
                            }
                            pReset.command = 'P';
                            int a6 = Interp.atoi(arg6);
                            pReset.arg2 = Interp.is_number(arg6) ? a6 : 1;
                            pReset.arg3 = Interp.is_number(arg5) ? iv : 1;
                            int a7 = Interp.atoi(arg7);
                            pReset.arg4 = Interp.is_number(arg7) ? a7 : 1;
                        }
                        else if (!RomString.str_cmp(arg4, "room"))
                        {
                            if (Handler.get_obj_index(ov) == null)
                            {
                                Comm.send_to_char("Vnum doesn't exist.\n\r", ch);
                                return;
                            }
                            pReset.command = 'O';
                            pReset.arg2 = 0;
                            pReset.arg3 = ch.in_room.vnum;
                            pReset.arg4 = 0;
                        }
                        else
                        {
                            if (Lookup.flag_value(Tables.wear_loc_flags, arg4) == NO_FLAG)
                            {
                                Comm.send_to_char("Resets: '? wear-loc'\n\r", ch);
                                return;
                            }
                            if (Handler.get_obj_index(ov) == null)
                            {
                                Comm.send_to_char("Vnum doesn't exist.\n\r", ch);
                                return;
                            }
                            pReset.arg1 = ov;
                            pReset.arg3 = Lookup.flag_value(Tables.wear_loc_flags, arg4);
                            if (pReset.arg3 == WEAR_NONE)
                                pReset.command = 'G';
                            else
                                pReset.command = 'E';
                        }
                    }
                    int idx = Interp.atoi(arg1);
                    add_reset(ch.in_room, pReset, idx);
                    Bit.SET_BIT(ref ch.in_room.area.area_flags, AREA_CHANGED);
                    Comm.send_to_char("Reset added.\n\r", ch);
                }
                else if (!RomString.str_cmp(arg2, "random") && Interp.is_number(arg3))
                {
                    int r3 = Interp.atoi(arg3);
                    if (r3 < 1 || r3 > 6)
                    {
                        Comm.send_to_char("Invalid argument.\n\r", ch);
                        return;
                    }
                    pReset = Recycle.new_reset_data();
                    pReset.command = 'R';
                    pReset.arg1 = ch.in_room.vnum;
                    pReset.arg2 = r3;
                    int idx = Interp.atoi(arg1);
                    add_reset(ch.in_room, pReset, idx);
                    Bit.SET_BIT(ref ch.in_room.area.area_flags, AREA_CHANGED);
                    Comm.send_to_char("Random exits reset added.\n\r", ch);
                }
                else
                {
                    Comm.send_to_char("Syntax: RESET <number> OBJ <vnum> <wear_loc>\n\r",
                        ch);
                    Comm.send_to_char
                        ("        RESET <number> OBJ <vnum> inside <vnum> [limit] [count]\n\r",
                        ch);
                    Comm.send_to_char("        RESET <number> OBJ <vnum> room\n\r", ch);
                    Comm.send_to_char
                        ("        RESET <number> MOB <vnum> [max #x area] [max #x room]\n\r",
                        ch);
                    Comm.send_to_char("        RESET <number> DELETE\n\r", ch);
                    Comm.send_to_char("        RESET <number> RANDOM [#x exits]\n\r", ch);
                }
            }
        }

        public static void add_reset(RoomIndexData room, ResetData pReset, int index)
        {
            if (room.reset_first == null)
            {
                room.reset_first = pReset;
                room.reset_last = pReset;
                pReset.next = null;
                return;
            }

            index--;

            if (index == 0)
            {
                pReset.next = room.reset_first;
                room.reset_first = pReset;
                return;
            }

            int iReset = 0;
            ResetData reset;
            for (reset = room.reset_first; reset.next != null; reset = reset.next)
            {
                if (++iReset == index)
                    break;
            }

            pReset.next = reset.next;
            reset.next = pReset;
            if (pReset.next == null)
                room.reset_last = pReset;
        }

        static void display_resets(CharData ch)
        {
            var pRoom = ch.in_room;
            int iReset = 0;
            MobIndexData pMob = null;

            Comm.send_to_char
                (" No.  Loads    Description       Location         Vnum   Mx Mn Description"
                 + "\n\r"
                 + "==== ======== ============= =================== ======== ===== ==========="
                 + "\n\r", ch);

            for (var pReset = pRoom.reset_first; pReset != null; pReset = pReset.next)
            {
                string final = RomString.sprintf("[%2d] ", ++iReset);

                switch (pReset.command)
                {
                    default:
                        final += RomString.sprintf("Bad reset command: %c.", pReset.command);
                        break;
                    case 'M':
                    {
                        var pMobIndex = Handler.get_mob_index(pReset.arg1);
                        if (pMobIndex == null)
                            continue;
                        var pRoomIndex = Handler.get_room_index(pReset.arg3);
                        if (pRoomIndex == null)
                            continue;
                        pMob = pMobIndex;
                        final += RomString.sprintf(
                            "M[%5d] %-13.13s in room             R[%5d] %2d-%2d %-15.15s\n\r",
                            pReset.arg1, pMob.short_descr, pReset.arg3,
                            pReset.arg2, pReset.arg4, pRoomIndex.name);
                        var pRoomIndexPrev = Handler.get_room_index(pRoomIndex.vnum - 1);
                        if (pRoomIndexPrev != null
                            && Bit.IS_SET(pRoomIndexPrev.room_flags, ROOM_PET_SHOP)
                            && final.Length > 5)
                        {
                            var chars = final.ToCharArray();
                            chars[5] = 'P';
                            final = new string(chars);
                        }
                        break;
                    }
                    case 'P':
                    {
                        var pObjIndex = Handler.get_obj_index(pReset.arg1);
                        if (pObjIndex == null)
                        {
                            continue;
                        }
                        var pObjToIndex = Handler.get_obj_index(pReset.arg3);
                        if (pObjToIndex == null)
                        {
                            continue;
                        }
                        final += RomString.sprintf(
                            "O[%5d] %-13.13s inside              O[%5d] %2d-%2d %-15.15s\n\r",
                            pReset.arg1, pObjIndex.short_descr, pReset.arg3,
                            pReset.arg2, pReset.arg4, pObjToIndex.short_descr);
                        break;
                    }
                    case 'O':
                    {
                        var pObjIndex = Handler.get_obj_index(pReset.arg1);
                        if (pObjIndex == null)
                            continue;
                        var pRoomIndex = Handler.get_room_index(pReset.arg3);
                        if (pRoomIndex == null)
                            continue;
                        final += RomString.sprintf("O[%5d] %-13.13s in room             "
                            + "R[%5d]       %-15.15s\n\r",
                            pReset.arg1, pObjIndex.short_descr,
                            pReset.arg3, pRoomIndex.name);
                        break;
                    }
                    case 'G':
                    case 'E':
                    {
                        var pObjIndex = Handler.get_obj_index(pReset.arg1);
                        if (pObjIndex == null)
                            continue;
                        if (pMob == null)
                        {
                            final += "Give/Equip Object - No Previous Mobile\n\r";
                            break;
                        }
                        if (pMob.pShop != null)
                            final += RomString.sprintf(
                                "O[%5d] %-13.13s in the inventory of S[%5d]       %-15.15s\n\r",
                                pReset.arg1, pObjIndex.short_descr, pMob.vnum, pMob.short_descr);
                        else
                            final += RomString.sprintf(
                                "O[%5d] %-13.13s %-19.19s M[%5d]       %-15.15s\n\r",
                                pReset.arg1, pObjIndex.short_descr,
                                (pReset.command == 'G') ?
                                Lookup.flag_string(Tables.wear_loc_strings, WEAR_NONE)
                                : Lookup.flag_string(Tables.wear_loc_strings, pReset.arg3),
                                pMob.vnum, pMob.short_descr);
                        break;
                    }
                    /*
                     * Doors are set in rs_flags don't need to be displayed.
                     * If you want to display them then uncomment the new_reset
                     * line in the case 'D' in load_resets in db.c and here.
                     */
                    case 'D':
                    {
                        var pRoomIndex = Handler.get_room_index(pReset.arg1);
                        final += RomString.sprintf(
                            "R[%5d] %s door of %-19.19s reset to %s\n\r",
                            pReset.arg1,
                            RomString.capitalize(ActMove.dir_name[pReset.arg2]),
                            pRoomIndex.name,
                            Lookup.flag_string(Tables.door_resets, pReset.arg3));
                        break;
                    }
                    case 'R':
                    {
                        var pRoomIndex = Handler.get_room_index(pReset.arg1);
                        if (pRoomIndex == null)
                            continue;
                        final += RomString.sprintf("R[%5d] Exits are randomized in %s\n\r",
                            pReset.arg1, pRoomIndex.name);
                        break;
                    }
                }
                Comm.send_to_char(final, ch);
            }
        }

        public static void do_alist(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            string result = RomString.sprintf("[%3s] [%-27s] (%-5s-%5s) [%-10s] %3s [%-10s]\n\r",
                "Num", "Area Name", "lvnum", "uvnum", "Filename", "Sec",
                "Builders");

            for (var pArea = Game.area_first; pArea != null; pArea = pArea.next)
            {
                result += RomString.sprintf(
                    "[%3d] %-29.29s (%-5d-%5d) %-12.12s [%d] [%-10.10s]\n\r",
                    pArea.vnum, pArea.name, pArea.min_vnum, pArea.max_vnum,
                    pArea.file_name, pArea.security, pArea.builders);
            }

            Comm.send_to_char(result, ch);
        }
    }
}
