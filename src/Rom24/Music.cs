using static Rom24.Merc;

namespace Rom24
{
    public class SongData
    {
        public string group;
        public string name;
        public string[] lyrics = new string[Music.MAX_LINES];
        public int lines;
    }

    public static class Music
    {
        public const int MAX_SONGS = 20;
        public const int MAX_LINES = 100;
        public const int MAX_GLOBAL = 10;

        public static int[] channel_songs = new int[MAX_GLOBAL + 1];
        public static SongData[] song_table = new SongData[MAX_SONGS];

        static Music()
        {
            for (int i = 0; i < MAX_SONGS; i++)
                song_table[i] = new SongData();
            for (int i = 0; i <= MAX_GLOBAL; i++)
                channel_songs[i] = -1;
        }

        public static void song_update()
        {
            if (channel_songs[1] >= MAX_SONGS)
                channel_songs[1] = -1;

            if (channel_songs[1] > -1)
            {
                if (channel_songs[0] >= MAX_LINES
                    || channel_songs[0] >= song_table[channel_songs[1]].lines)
                {
                    channel_songs[0] = -1;

                    for (int i = 1; i < MAX_GLOBAL; i++)
                        channel_songs[i] = channel_songs[i + 1];
                    channel_songs[MAX_GLOBAL] = -1;
                }
                else
                {
                    string buf;
                    if (channel_songs[0] < 0)
                    {
                        buf = RomString.sprintf("Music: %s, %s",
                            song_table[channel_songs[1]].group,
                            song_table[channel_songs[1]].name);
                        channel_songs[0] = 0;
                    }
                    else
                    {
                        buf = RomString.sprintf("Music: '%s'",
                            song_table[channel_songs[1]].lyrics[channel_songs[0]]);
                        channel_songs[0]++;
                    }

                    for (var d = Game.descriptor_list; d != null; d = d.next)
                    {
                        var victim = d.original != null ? d.original : d.character;

                        if (d.connected == CON_PLAYING &&
                            !Bit.IS_SET(victim.comm, COMM_NOMUSIC) &&
                            !Bit.IS_SET(victim.comm, COMM_QUIET))
                            Comm.act_new("$t", d.character, buf, null, TO_CHAR,
                                POS_SLEEPING);
                    }
                }
            }

            for (var obj = Game.object_list; obj != null; obj = obj.next)
            {
                if (obj.item_type != ITEM_JUKEBOX || obj.value[1] < 0)
                    continue;

                if (obj.value[1] >= MAX_SONGS)
                {
                    obj.value[1] = -1;
                    continue;
                }

                RoomIndexData room = obj.in_room;
                if (room == null)
                {
                    if (obj.carried_by == null)
                        continue;
                    else if ((room = obj.carried_by.in_room) == null)
                        continue;
                }

                string line;
                if (obj.value[0] < 0)
                {
                    string start = RomString.sprintf("$p starts playing %s, %s.",
                        song_table[obj.value[1]].group,
                        song_table[obj.value[1]].name);
                    if (room.people != null)
                        Comm.act(start, room.people, obj, null, TO_ALL);
                    obj.value[0] = 0;
                    continue;
                }
                else
                {
                    if (obj.value[0] >= MAX_LINES
                        || obj.value[0] >= song_table[obj.value[1]].lines)
                    {
                        obj.value[0] = -1;
                        obj.value[1] = obj.value[2];
                        obj.value[2] = obj.value[3];
                        obj.value[3] = obj.value[4];
                        obj.value[4] = -1;
                        continue;
                    }

                    line = song_table[obj.value[1]].lyrics[obj.value[0]];
                    obj.value[0]++;
                }

                string bop = RomString.sprintf("$p bops: '%s'", line);
                if (room.people != null)
                    Comm.act(bop, room.people, obj, null, TO_ALL);
            }
        }

        public static void load_songs()
        {
            for (int i = 0; i <= MAX_GLOBAL; i++)
                channel_songs[i] = -1;

            var path = Path.Combine(Game.area_dir, MUSIC_FILE);
            if (!File.Exists(path))
            {
                Db.bug("Couldn't open music file, no songs available.", 0);
                return;
            }

            using var fp = new AreaReader(path);
            for (int count = 0; count < MAX_SONGS; count++)
            {
                char letter = fp.fread_letter();
                if (letter == '#')
                {
                    if (count < MAX_SONGS)
                        song_table[count].name = null;
                    return;
                }
                else
                    fp.Ungetc(letter);

                song_table[count].group = fp.fread_string();
                song_table[count].name = fp.fread_string();

                int lines = 0;
                for (;;)
                {
                    letter = fp.fread_letter();

                    if (letter == '~')
                    {
                        song_table[count].lines = lines;
                        break;
                    }
                    else
                        fp.Ungetc(letter);

                    if (lines >= MAX_LINES)
                    {
                        Db.bug("Too many lines in a song -- limit is  %d.", MAX_LINES);
                        break;
                    }

                    song_table[count].lyrics[lines] = fp.fread_string_eol();
                    lines++;
                }
            }
        }

        public static void do_play(CharData ch, string argument)
        {
            string str = RomString.one_argument(argument, out string arg);

            ObjData juke;
            for (juke = ch.in_room.contents; juke != null;
                 juke = juke.next_content)
                if (juke.item_type == ITEM_JUKEBOX
                    && Handler.can_see_obj(ch, juke))
                    break;

            if (argument.Length == 0)
            {
                Comm.send_to_char("Play what?\n\r", ch);
                return;
            }

            if (juke == null)
            {
                Comm.send_to_char("You see nothing to play.\n\r", ch);
                return;
            }

            if (!RomString.str_cmp(arg, "list"))
            {
                int col = 0;
                bool artist = false, match = false;

                var output = new System.Text.StringBuilder();
                argument = str;
                argument = RomString.one_argument(argument, out arg);

                if (!RomString.str_cmp(arg, "artist"))
                    artist = true;

                if (argument.Length != 0)
                    match = true;

                string buf = RomString.sprintf("%s has the following songs available:\n\r",
                    juke.short_descr);
                output.Append(RomString.capitalize(buf));

                for (int i = 0; i < MAX_SONGS; i++)
                {
                    if (song_table[i].name == null)
                        break;

                    if (artist && (!match
                                   || !RomString.str_prefix(argument, song_table[i].group)))
                        buf = RomString.sprintf("%-39s %-39s\n\r",
                            song_table[i].group, song_table[i].name);
                    else if (!artist && (!match
                                         || !RomString.str_prefix(argument,
                                             song_table[i].name)))
                        buf = RomString.sprintf("%-35s ", song_table[i].name);
                    else
                        continue;
                    output.Append(buf);
                    if (!artist && ++col % 2 == 0)
                        output.Append("\n\r");
                }
                if (!artist && col % 2 != 0)
                    output.Append("\n\r");

                Comm.page_to_char(output.ToString(), ch);
                return;
            }

            bool global = false;
            if (!RomString.str_cmp(arg, "loud"))
            {
                argument = str;
                global = true;
            }

            if (argument.Length == 0)
            {
                Comm.send_to_char("Play what?\n\r", ch);
                return;
            }

            if ((global && channel_songs[MAX_GLOBAL] > -1)
                || (!global && juke.value[4] > -1))
            {
                Comm.send_to_char("The jukebox is full up right now.\n\r", ch);
                return;
            }

            int song;
            for (song = 0; song < MAX_SONGS; song++)
            {
                if (song_table[song].name == null)
                {
                    Comm.send_to_char("That song isn't available.\n\r", ch);
                    return;
                }
                if (!RomString.str_prefix(argument, song_table[song].name))
                    break;
            }

            if (song >= MAX_SONGS)
            {
                Comm.send_to_char("That song isn't available.\n\r", ch);
                return;
            }

            Comm.send_to_char("Coming right up.\n\r", ch);

            if (global)
            {
                for (int i = 1; i <= MAX_GLOBAL; i++)
                    if (channel_songs[i] < 0)
                    {
                        if (i == 1)
                            channel_songs[0] = -1;
                        channel_songs[i] = song;
                        return;
                    }
            }
            else
            {
                for (int i = 1; i < 5; i++)
                    if (juke.value[i] < 0)
                    {
                        if (i == 1)
                            juke.value[0] = -1;
                        juke.value[i] = song;
                        return;
                    }
            }
        }
    }
}
