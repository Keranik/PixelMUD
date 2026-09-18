using static Rom24.Merc;

namespace Rom24
{
    public static class Scan
    {
        static readonly string[] distance =
        {
            "right here.", "nearby to the %s.", "not far %s.",
            "off in the distance %s."
        };

        public static void do_scan(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg1);
            int door;

            if (arg1.Length == 0)
            {
                Comm.act("$n looks all around.", ch, null, null, TO_ROOM);
                Comm.send_to_char("Looking around you see:\n\r", ch);
                scan_list(ch.in_room, ch, 0, -1);

                for (door = 0; door < 6; door++)
                {
                    var pExit = ch.in_room.exit[door];
                    if (pExit != null)
                        scan_list(pExit.to_room, ch, 1, door);
                }
                return;
            }
            else if (!RomString.str_cmp(arg1, "n") || !RomString.str_cmp(arg1, "north"))
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
                Comm.send_to_char("Which way do you want to scan?\n\r", ch);
                return;
            }

            Comm.act("You peer intently $T.", ch, null, ActMove.dir_name[door], TO_CHAR);
            Comm.act("$n peers intently $T.", ch, null, ActMove.dir_name[door], TO_ROOM);
            /* scan.c sprintfs "Looking %s you see:\n\r" into buf and never sends it */

            var scan_room = ch.in_room;

            for (int depth = 1; depth < 4; depth++)
            {
                var pExit = scan_room.exit[door];
                if (pExit != null)
                {
                    scan_room = pExit.to_room;
                    scan_list(pExit.to_room, ch, depth, door);
                }
            }
        }

        static void scan_list(RoomIndexData scan_room, CharData ch, int depth, int door)
        {
            if (scan_room == null)
                return;
            for (var rch = scan_room.people; rch != null; rch = rch.next_in_room)
            {
                if (rch == ch)
                    continue;
                if (!Bit.IS_NPC(rch) && rch.invis_level > Handler.get_trust(ch))
                    continue;
                if (Handler.can_see(ch, rch))
                    scan_char(rch, ch, depth, door);
            }
        }

        static void scan_char(CharData victim, CharData ch, int depth, int door)
        {
            string buf = Handler.PERS(victim, ch);
            buf += ", ";
            buf += RomString.sprintf(distance[depth], ActMove.dir_name[door < 0 ? 0 : door]);
            buf += "\n\r";
            Comm.send_to_char(buf, ch);
        }
    }
}
