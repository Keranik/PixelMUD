using System.Globalization;
using static Rom24.Merc;

namespace Rom24
{
    public static class Board
    {
        public const int DEF_NORMAL = 0;
        public const int DEF_INCLUDE = 1;
        public const int DEF_EXCLUDE = 2;
        public const int DEFAULT_BOARD = 0;
        public const int MAX_LINE_LENGTH = 80;
        public const int MAX_NOTE_TEXT = 4 * MAX_STRING_LENGTH - 1000;
        public const int BOARD_NOACCESS = -1;
        public const int BOARD_NOTFOUND = -1;

        public static readonly BoardData[] boards =
        {
            new BoardData("General", "General discussion", 0, 2, "all", DEF_INCLUDE, 21),
            new BoardData("Ideas", "Suggestion for improvement", 0, 2, "all", DEF_NORMAL, 60),
            new BoardData("Announce", "Announcements from Immortals", 0, L_IMM, "all", DEF_NORMAL, 60),
            new BoardData("Bugs", "Typos, bugs, errors", 0, 1, "imm", DEF_NORMAL, 60),
            new BoardData("Personal", "Personal messages", 0, 1, "all", DEF_EXCLUDE, 28)
        };

        static readonly string szFinishPrompt =
            "({WC{x)ontinue, ({WV{x)iew, ({WP{x)ost or ({WF{x)orget it?";

        static long last_note_stamp;

        static string NoteDir =>
            Path.GetFullPath(Path.Combine(Game.area_dir, "..", "notes"));

        public static void free_note(NoteData note)
        {
            if (note == null) return;
            note.next = null;
            note.sender = null;
            note.to_list = null;
            note.subject = null;
            note.date = null;
            note.text = null;
        }

        public static NoteData new_note() => Recycle.new_note();

        static void append_note(StreamWriter fp, NoteData note)
        {
            fp.Write("Sender  {0}~\n", note.sender);
            fp.Write("Date    {0}~\n", note.date);
            fp.Write("Stamp   {0}\n", note.date_stamp);
            fp.Write("Expire  {0}\n", note.expire);
            fp.Write("To      {0}~\n", note.to_list);
            fp.Write("Subject {0}~\n", note.subject);
            fp.Write("Text\n{0}~\n\n", note.text);
        }

        public static void finish_note(BoardData board, NoteData note)
        {
            if (last_note_stamp >= Game.current_time)
                note.date_stamp = ++last_note_stamp;
            else
            {
                note.date_stamp = Game.current_time;
                last_note_stamp = Game.current_time;
            }

            if (board.note_first != null)
            {
                var p = board.note_first;
                for (; p.next != null; p = p.next)
                    ;
                p.next = note;
            }
            else
                board.note_first = note;

            Directory.CreateDirectory(NoteDir);
            var filename = Path.Combine(NoteDir, board.short_name);
            try
            {
                using var fp = new StreamWriter(filename, true);
                append_note(fp, note);
            }
            catch
            {
                Db.bug("Could not open one of the note files in append mode", 0);
                board.changed = true;
            }
        }

        public static int board_number(BoardData board)
        {
            for (int i = 0; i < MAX_BOARD; i++)
                if (ReferenceEquals(board, boards[i]))
                    return i;
            return -1;
        }

        public static int board_lookup(string name)
        {
            for (int i = 0; i < MAX_BOARD; i++)
                if (!RomString.str_cmp(boards[i].short_name, name))
                    return i;
            return -1;
        }

        static void unlink_note(BoardData board, NoteData note)
        {
            if (board.note_first == note)
                board.note_first = note.next;
            else
            {
                NoteData p;
                for (p = board.note_first; p != null && p.next != note; p = p.next)
                    ;
                if (p == null)
                    Db.bug("unlink_note: could not find note.", 0);
                else
                    p.next = note.next;
            }
        }

        static NoteData find_note(CharData ch, BoardData board, int num)
        {
            int count = 0;
            NoteData p;
            for (p = board.note_first; p != null; p = p.next)
                if (++count == num)
                    break;

            if (count == num && is_note_to(ch, p))
                return p;
            return null;
        }

        static void save_board(BoardData board)
        {
            Directory.CreateDirectory(NoteDir);
            var filename = Path.Combine(NoteDir, board.short_name);
            try
            {
                using var fp = new StreamWriter(filename, false);
                for (var note = board.note_first; note != null; note = note.next)
                    append_note(fp, note);
            }
            catch
            {
                Db.bug("Error writing to: " + filename, 0);
            }
        }

        static void show_note_to_char(CharData ch, NoteData note, int num)
        {
            Comm.send_to_char(RomString.sprintf(
                "[{W%4d{x] {Y%s{x: {g%s{x\n\r"
                + "{YDate{x:  %s\n\r"
                + "{YTo{x:    %s\n\r"
                + "{g==========================================================================={x\n\r"
                + "%s\n\r",
                num, note.sender, note.subject,
                note.date,
                note.to_list,
                note.text), ch);
        }

        public static void save_notes()
        {
            for (int i = 0; i < MAX_BOARD; i++)
                if (boards[i].changed)
                    save_board(boards[i]);
        }

        static void load_board(BoardData board)
        {
            var filename = Path.Combine(NoteDir, board.short_name);
            if (!File.Exists(filename))
                return;

            using var fp = new AreaReader(filename);
            NoteData last = null;
            for (;;)
            {
                if (fp.Eof())
                    return;

                var word = fp.fread_word();
                if (word.Length == 0)
                    return;
                if (RomString.str_cmp(word, "sender"))
                    break;
                var pnote = Recycle.new_note();
                pnote.sender = fp.fread_string();

                if (RomString.str_cmp(fp.fread_word(), "date"))
                    break;
                pnote.date = fp.fread_string();

                if (RomString.str_cmp(fp.fread_word(), "stamp"))
                    break;
                pnote.date_stamp = fp.fread_number();

                if (RomString.str_cmp(fp.fread_word(), "expire"))
                    break;
                pnote.expire = fp.fread_number();

                if (RomString.str_cmp(fp.fread_word(), "to"))
                    break;
                pnote.to_list = fp.fread_string();

                if (RomString.str_cmp(fp.fread_word(), "subject"))
                    break;
                pnote.subject = fp.fread_string();

                if (RomString.str_cmp(fp.fread_word(), "text"))
                    break;
                pnote.text = fp.fread_string();

                pnote.next = null;

                if (pnote.expire < Game.current_time)
                {
                    var archive_name = Path.Combine(NoteDir, board.short_name + ".old");
                    try
                    {
                        Directory.CreateDirectory(NoteDir);
                        using var fp_archive = new StreamWriter(archive_name, true);
                        append_note(fp_archive, pnote);
                    }
                    catch
                    {
                        Db.bug("Could not open archive boards for writing", 0);
                    }
                    free_note(pnote);
                    board.changed = true;
                    continue;
                }

                if (board.note_first == null)
                    board.note_first = pnote;
                else
                    last.next = pnote;
                last = pnote;
            }

            Db.bug("Load_notes: bad key word.", 0);
        }

        public static void load_boards()
        {
            for (int i = 0; i < MAX_BOARD; i++)
                load_board(boards[i]);
        }

        public static bool is_note_to(CharData ch, NoteData note)
        {
            if (!RomString.str_cmp(ch.name, note.sender))
                return true;

            if (Handler.is_full_name("all", note.to_list))
                return true;

            if (Bit.IS_IMMORTAL(ch) && (
                Handler.is_full_name("imm", note.to_list) ||
                Handler.is_full_name("imms", note.to_list) ||
                Handler.is_full_name("immortal", note.to_list) ||
                Handler.is_full_name("god", note.to_list) ||
                Handler.is_full_name("gods", note.to_list) ||
                Handler.is_full_name("immortals", note.to_list)))
                return true;

            if (Handler.get_trust(ch) == MAX_LEVEL && (
                Handler.is_full_name("imp", note.to_list) ||
                Handler.is_full_name("imps", note.to_list) ||
                Handler.is_full_name("implementor", note.to_list) ||
                Handler.is_full_name("implementors", note.to_list)))
                return true;

            if (Handler.is_full_name(ch.name, note.to_list))
                return true;

            if (Interp.is_number(note.to_list)
                && Handler.get_trust(ch) >= Interp.atoi(note.to_list))
                return true;

            return false;
        }

        public static int unread_notes(CharData ch, BoardData board)
        {
            if (board.read_level > Handler.get_trust(ch))
                return BOARD_NOACCESS;

            long last_read = ch.pcdata.last_note[board_number(board)];
            int count = 0;
            for (var note = board.note_first; note != null; note = note.next)
                if (is_note_to(ch, note) && last_read < note.date_stamp)
                    count++;
            return count;
        }

        static void do_nwrite(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            if (Handler.get_trust(ch) < ch.pcdata.board.write_level)
            {
                Comm.send_to_char("You cannot post notes on this board.\n\r", ch);
                return;
            }

            if (ch.pcdata.in_progress != null && ch.pcdata.in_progress.text == null)
            {
                Comm.send_to_char("Note in progress cancelled because you did not manage to write any text \n\r"
                    + "before losing link.\n\r\n\r", ch);
                free_note(ch.pcdata.in_progress);
                ch.pcdata.in_progress = null;
            }

            if (ch.pcdata.in_progress == null)
            {
                ch.pcdata.in_progress = new_note();
                ch.pcdata.in_progress.sender = ch.name;
                ch.pcdata.in_progress.date = ctime(Game.current_time);
            }

            Comm.act("{G$n starts writing a note.{x", ch, null, null, TO_ROOM);

            Comm.send_to_char(RomString.sprintf("You are now %s a new note on the {W%s{x board.\n\r"
                + "If you are using tintin, type #verbose to turn off alias expansion!\n\r\n\r",
                ch.pcdata.in_progress.text != null ? "continuing" : "posting",
                ch.pcdata.board.short_name), ch);

            Comm.send_to_char(RomString.sprintf("{YFrom{x:    %s\n\r\n\r", ch.name), ch);

            if (ch.pcdata.in_progress.text == null)
            {
                string buf = "";
                switch (ch.pcdata.board.force_type)
                {
                    case DEF_NORMAL:
                        buf = RomString.sprintf("If you press Return, default recipient \"{W%s{x\" will be chosen.\n\r",
                            ch.pcdata.board.names);
                        break;
                    case DEF_INCLUDE:
                        buf = RomString.sprintf("The recipient list MUST include \"{W%s{x\". If not, it will be added automatically.\n\r",
                            ch.pcdata.board.names);
                        break;
                    case DEF_EXCLUDE:
                        buf = RomString.sprintf("The recipient of this note must NOT include: \"{W%s{x\".",
                            ch.pcdata.board.names);
                        break;
                }

                Comm.send_to_char(buf, ch);
                Comm.send_to_char("\n\r{YTo{x:      ", ch);
                ch.desc.connected = CON_NOTE_TO;
            }
            else
            {
                Comm.send_to_char(RomString.sprintf("{YTo{x:      %s\n\r"
                    + "{YExpires{x: %s\n\r"
                    + "{YSubject{x: %s\n\r",
                    ch.pcdata.in_progress.to_list,
                    ctime(ch.pcdata.in_progress.expire) + "\n",
                    ch.pcdata.in_progress.subject), ch);
                Comm.send_to_char("{GYour note so far:{x\n\r", ch);
                Comm.send_to_char(ch.pcdata.in_progress.text, ch);
                Comm.send_to_char("\n\rEnter text. Type {W~{x or {WEND{x on an empty line to end note.\n\r"
                    + "=======================================================\n\r", ch);
                ch.desc.connected = CON_NOTE_TEXT;
            }
        }

        static void do_nread(CharData ch, string argument)
        {
            int count = 0;
            int bn = board_number(ch.pcdata.board);

            if (!RomString.str_cmp(argument, "again"))
            {
            }
            else if (Interp.is_number(argument))
            {
                int number = Interp.atoi(argument);
                NoteData p;
                for (p = ch.pcdata.board.note_first; p != null; p = p.next)
                    if (++count == number)
                        break;

                if (p == null || !is_note_to(ch, p))
                    Comm.send_to_char("No such note.\n\r", ch);
                else
                {
                    show_note_to_char(ch, p, count);
                    if (ch.pcdata.last_note[bn] < p.date_stamp)
                        ch.pcdata.last_note[bn] = p.date_stamp;
                }
            }
            else
            {
                count = 1;
                for (var p = ch.pcdata.board.note_first; p != null; p = p.next, count++)
                    if (p.date_stamp > ch.pcdata.last_note[bn] && is_note_to(ch, p))
                    {
                        show_note_to_char(ch, p, count);
                        if (ch.pcdata.last_note[bn] < p.date_stamp)
                            ch.pcdata.last_note[bn] = p.date_stamp;
                        return;
                    }

                Comm.send_to_char("No new notes in this board.\n\r", ch);

                string buf;
                if (next_board(ch))
                    buf = RomString.sprintf("Changed to next board, %s.\n\r", ch.pcdata.board.short_name);
                else
                    buf = "There are no more boards.\n\r";
                Comm.send_to_char(buf, ch);
            }
        }

        static void do_nremove(CharData ch, string argument)
        {
            if (!Interp.is_number(argument))
            {
                Comm.send_to_char("Remove which note?\n\r", ch);
                return;
            }

            var p = find_note(ch, ch.pcdata.board, Interp.atoi(argument));
            if (p == null)
            {
                Comm.send_to_char("No such note.\n\r", ch);
                return;
            }

            if (RomString.str_cmp(ch.name, p.sender) && Handler.get_trust(ch) < MAX_LEVEL)
            {
                Comm.send_to_char("You are not authorized to remove this note.\n\r", ch);
                return;
            }

            unlink_note(ch.pcdata.board, p);
            free_note(p);
            Comm.send_to_char("Note removed!\n\r", ch);
            save_board(ch.pcdata.board);
        }

        static void do_nlist(CharData ch, string argument)
        {
            int count = 0, show = 0, num = 0, has_shown = 0;

            if (Interp.is_number(argument))
            {
                show = Interp.atoi(argument);
                for (var p = ch.pcdata.board.note_first; p != null; p = p.next)
                    if (is_note_to(ch, p))
                        count++;
            }

            Comm.send_to_char("{WNotes on this board:{x\n\r"
                + "{rNum> Author        Subject{x\n\r", ch);

            long last_note = ch.pcdata.last_note[board_number(ch.pcdata.board)];

            for (var p = ch.pcdata.board.note_first; p != null; p = p.next)
            {
                num++;
                if (is_note_to(ch, p))
                {
                    has_shown++;
                    if (show == 0 || (count - show) < has_shown)
                    {
                        Comm.send_to_char(RomString.sprintf("{W%3d{x>{B%c{x{Y%-13s{x{y%s{x\n\r",
                            num,
                            last_note < p.date_stamp ? '*' : ' ',
                            p.sender, p.subject), ch);
                    }
                }
            }
        }

        static void do_ncatchup(CharData ch, string argument)
        {
            NoteData p;
            for (p = ch.pcdata.board.note_first; p != null && p.next != null; p = p.next)
                ;

            if (p == null)
                Comm.send_to_char("Alas, there are no notes in that board.\n\r", ch);
            else
            {
                ch.pcdata.last_note[board_number(ch.pcdata.board)] = p.date_stamp;
                Comm.send_to_char("All mesages skipped.\n\r", ch);
            }
        }

        public static void do_note(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            argument = RomString.one_argument(argument, out string arg);

            if (arg.Length == 0 || !RomString.str_cmp(arg, "read"))
                do_nread(ch, argument);
            else if (!RomString.str_cmp(arg, "list"))
                do_nlist(ch, argument);
            else if (!RomString.str_cmp(arg, "write"))
                do_nwrite(ch, argument);
            else if (!RomString.str_cmp(arg, "remove"))
                do_nremove(ch, argument);
            else if (!RomString.str_cmp(arg, "purge"))
                Comm.send_to_char("Obsolete.\n\r", ch);
            else if (!RomString.str_cmp(arg, "archive"))
                Comm.send_to_char("Obsolete.\n\r", ch);
            else if (!RomString.str_cmp(arg, "catchup"))
                do_ncatchup(ch, argument);
            else
                Interp.do_help(ch, "note");
        }

        public static void do_board(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            if (argument.Length == 0)
            {
                int count = 1;
                Comm.send_to_char("{RNum          Name Unread Description{x\n\r"
                    + "{R==== ============ ====== ============================={x\n\r", ch);
                for (int i = 0; i < MAX_BOARD; i++)
                {
                    int unread = unread_notes(ch, boards[i]);
                    if (unread != BOARD_NOACCESS)
                    {
                        Comm.send_to_char(RomString.sprintf("({W%2d{x) {g%12s{x [%s%4d{x] {y%s{x\n\r",
                            count, boards[i].short_name, unread != 0 ? "{G" : "{g",
                            unread, boards[i].long_name), ch);
                        count++;
                    }
                }

                Comm.send_to_char(RomString.sprintf("\n\rYou current board is {W%s{x.\n\r",
                    ch.pcdata.board.short_name), ch);

                if (ch.pcdata.board.read_level > Handler.get_trust(ch))
                    Comm.send_to_char("You cannot read nor write notes on this board.\n\r", ch);
                else if (ch.pcdata.board.write_level > Handler.get_trust(ch))
                    Comm.send_to_char("You can only read notes from this board.\n\r", ch);
                else
                    Comm.send_to_char("You can both read and write on this board.\n\r", ch);
                return;
            }

            if (ch.pcdata.in_progress != null)
            {
                Comm.send_to_char("Please finish your interrupted note first.\n\r", ch);
                return;
            }

            if (Interp.is_number(argument))
            {
                int count = 0;
                int number = Interp.atoi(argument);
                int i;
                for (i = 0; i < MAX_BOARD; i++)
                    if (unread_notes(ch, boards[i]) != BOARD_NOACCESS)
                        if (++count == number)
                            break;

                if (count == number)
                {
                    ch.pcdata.board = boards[i];
                    Comm.send_to_char(RomString.sprintf("Current board changed to {W%s{x. %s.\n\r",
                        boards[i].short_name,
                        Handler.get_trust(ch) < boards[i].write_level
                            ? "You can only read here"
                            : "You can both read and write here"), ch);
                }
                else
                    Comm.send_to_char("No such board.\n\r", ch);
                return;
            }

            int j;
            for (j = 0; j < MAX_BOARD; j++)
                if (!RomString.str_cmp(boards[j].short_name, argument))
                    break;

            if (j == MAX_BOARD)
            {
                Comm.send_to_char("No such board.\n\r", ch);
                return;
            }

            if (unread_notes(ch, boards[j]) == BOARD_NOACCESS)
            {
                Comm.send_to_char("No such board.\n\r", ch);
                return;
            }

            ch.pcdata.board = boards[j];
            Comm.send_to_char(RomString.sprintf("Current board changed to {W%s{x. %s.\n\r",
                boards[j].short_name,
                Handler.get_trust(ch) < boards[j].write_level
                    ? "You can only read here"
                    : "You can both read and write here"), ch);
        }

        public static void personal_message(string sender, string to, string subject, int expire_days, string text)
        {
            make_note("Personal", sender, to, subject, expire_days, text);
        }

        public static void make_note(string board_name, string sender, string to, string subject, int expire_days, string text)
        {
            int board_index = board_lookup(board_name);
            if (board_index == BOARD_NOTFOUND)
            {
                Db.bug("make_note: board not found", 0);
                return;
            }

            if (text.Length > MAX_NOTE_TEXT)
            {
                Db.bug("make_note: text too long (%d bytes)", text.Length);
                return;
            }

            var board = boards[board_index];
            var note = new_note();
            note.sender = sender;
            note.to_list = to;
            note.subject = subject;
            note.expire = Game.current_time + expire_days * 60L * 60L * 24L;
            note.text = text;
            note.date = ctime(Game.current_time);
            finish_note(board, note);
        }

        static bool next_board(CharData ch)
        {
            int i = board_number(ch.pcdata.board) + 1;
            while (i < MAX_BOARD && unread_notes(ch, boards[i]) == BOARD_NOACCESS)
                i++;
            if (i == MAX_BOARD)
                return false;
            ch.pcdata.board = boards[i];
            return true;
        }

        public static void handle_con_note_to(DescriptorData d, string argument)
        {
            var ch = d.character;
            if (ch.pcdata.in_progress == null)
            {
                d.connected = CON_PLAYING;
                Db.bug("nanny: In CON_NOTE_TO, but no note in progress", 0);
                return;
            }

            string buf = RomString.smash_tilde(argument ?? "");

            switch (ch.pcdata.board.force_type)
            {
                case DEF_NORMAL:
                    if (buf.Length == 0)
                    {
                        ch.pcdata.in_progress.to_list = ch.pcdata.board.names;
                        Comm.send_to_desc(RomString.sprintf("Assumed default recipient: {W%s{x\n\r",
                            ch.pcdata.board.names), d);
                    }
                    else
                        ch.pcdata.in_progress.to_list = buf;
                    break;

                case DEF_INCLUDE:
                    if (!Handler.is_full_name(ch.pcdata.board.names, buf))
                    {
                        buf = buf + " " + ch.pcdata.board.names;
                        ch.pcdata.in_progress.to_list = buf;
                        Comm.send_to_desc(RomString.sprintf(
                            "\n\rYou did not specify %s as recipient, so it was automatically added.\n\r"
                            + "{YNew To{x :  %s\n\r",
                            ch.pcdata.board.names, ch.pcdata.in_progress.to_list), d);
                    }
                    else
                        ch.pcdata.in_progress.to_list = buf;
                    break;

                case DEF_EXCLUDE:
                    if (buf.Length == 0)
                    {
                        Comm.send_to_desc("You must specify a recipient.\n\r"
                            + "{YTo{x:      ", d);
                        return;
                    }

                    if (Handler.is_full_name(ch.pcdata.board.names, buf))
                    {
                        Comm.send_to_desc(RomString.sprintf(
                            "You are not allowed to send notes to %s on this board. Try again.\n\r"
                            + "{YTo{x:      ", ch.pcdata.board.names), d);
                        return;
                    }
                    else
                        ch.pcdata.in_progress.to_list = buf;
                    break;
            }

            Comm.send_to_desc("{Y\n\rSubject{x: ", d);
            d.connected = CON_NOTE_SUBJECT;
        }

        public static void handle_con_note_subject(DescriptorData d, string argument)
        {
            var ch = d.character;
            if (ch.pcdata.in_progress == null)
            {
                d.connected = CON_PLAYING;
                Db.bug("nanny: In CON_NOTE_SUBJECT, but no note in progress", 0);
                return;
            }

            string buf = RomString.smash_tilde(argument ?? "");

            if (buf.Length == 0)
            {
                Comm.write_to_buffer(d, "Please find a meaningful subject!\n\r", 0);
                Comm.send_to_desc("{YSubject{x: ", d);
            }
            else if (buf.Length > 60)
            {
                Comm.write_to_buffer(d,
                    "No, no. This is just the Subject. You're note writing the note yet. Twit.\n\r", 0);
            }
            else
            {
                ch.pcdata.in_progress.subject = buf;
                if (Bit.IS_IMMORTAL(ch))
                {
                    Comm.send_to_desc(RomString.sprintf(
                        "\n\rHow many days do you want this note to expire in?\n\r"
                        + "Press Enter for default value for this board, {W%d{x days.\n\r"
                        + "{YExpire{x:  ",
                        ch.pcdata.board.purge_days), d);
                    d.connected = CON_NOTE_EXPIRE;
                }
                else
                {
                    ch.pcdata.in_progress.expire =
                        Game.current_time + ch.pcdata.board.purge_days * 24L * 3600L;
                    Comm.send_to_desc(RomString.sprintf("This note will expire %s\r",
                        ctime(ch.pcdata.in_progress.expire) + "\n"), d);
                    Comm.send_to_desc("\n\rEnter text. Type {W~{x or {WEND{x on an empty line to end note.\n\r"
                        + "=======================================================\n\r", d);
                    d.connected = CON_NOTE_TEXT;
                }
            }
        }

        public static void handle_con_note_expire(DescriptorData d, string argument)
        {
            var ch = d.character;
            if (ch.pcdata.in_progress == null)
            {
                d.connected = CON_PLAYING;
                Db.bug("nanny: In CON_NOTE_EXPIRE, but no note in progress", 0);
                return;
            }

            string buf = argument ?? "";
            int days;
            if (buf.Length == 0)
                days = ch.pcdata.board.purge_days;
            else if (!Interp.is_number(buf))
            {
                Comm.write_to_buffer(d, "Write the number of days!\n\r", 0);
                Comm.send_to_desc("{YExpire{x:  ", d);
                return;
            }
            else
            {
                days = Interp.atoi(buf);
                if (days <= 0)
                {
                    Comm.write_to_buffer(d, "This is a positive MUD. Use positive numbers only! :)\n\r", 0);
                    Comm.send_to_desc("{YExpire{x:  ", d);
                    return;
                }
            }

            ch.pcdata.in_progress.expire = Game.current_time + days * 24L * 3600L;
            Comm.send_to_desc("\n\rEnter text. Type {W~{x or {WEND{x on an empty line to end note.\n\r"
                + "=======================================================\n\r", d);
            d.connected = CON_NOTE_TEXT;
        }

        public static void handle_con_note_text(DescriptorData d, string argument)
        {
            var ch = d.character;
            if (ch.pcdata.in_progress == null)
            {
                d.connected = CON_PLAYING;
                Db.bug("nanny: In CON_NOTE_TEXT, but no note in progress", 0);
                return;
            }

            string buf = argument ?? "";
            if (!RomString.str_cmp(buf, "~") || !RomString.str_cmp(buf, "END"))
            {
                Comm.write_to_buffer(d, "\n\r\n\r", 0);
                Comm.send_to_desc(szFinishPrompt, d);
                Comm.write_to_buffer(d, "\n\r", 0);
                d.connected = CON_NOTE_FINISH;
                return;
            }

            buf = RomString.smash_tilde(buf);

            if (buf.Length > MAX_LINE_LENGTH)
            {
                Comm.send_to_desc(RomString.sprintf(
                    "Too long line rejected. Do NOT go over %d characters!\n\r", MAX_LINE_LENGTH), d);
                return;
            }

            string letter = ch.pcdata.in_progress.text ?? "";

            if (letter.Length + buf.Length > MAX_NOTE_TEXT)
            {
                Comm.write_to_buffer(d, "Note too long!\n\r", 0);
                free_note(ch.pcdata.in_progress);
                ch.pcdata.in_progress = null;
                d.connected = CON_PLAYING;
                return;
            }

            ch.pcdata.in_progress.text = letter + buf + "\r\n";
        }

        public static void handle_con_note_finish(DescriptorData d, string argument)
        {
            var ch = d.character;
            if (ch.pcdata.in_progress == null)
            {
                d.connected = CON_PLAYING;
                Db.bug("nanny: In CON_NOTE_FINISH, but no note in progress", 0);
                return;
            }

            char c = argument.Length == 0 ? '\0' : char.ToLowerInvariant(argument[0]);
            switch (c)
            {
                case 'c':
                    Comm.write_to_buffer(d, "Continuing note...\n\r", 0);
                    d.connected = CON_NOTE_TEXT;
                    break;

                case 'v':
                    if (ch.pcdata.in_progress.text != null)
                    {
                        Comm.send_to_desc("{gText of your note so far:{x\n\r", d);
                        Comm.write_to_buffer(d, ch.pcdata.in_progress.text, 0);
                    }
                    else
                        Comm.write_to_buffer(d, "You haven't written a thing!\n\r\n\r", 0);
                    Comm.send_to_desc(szFinishPrompt, d);
                    Comm.write_to_buffer(d, "\n\r", 0);
                    break;

                case 'p':
                    finish_note(ch.pcdata.board, ch.pcdata.in_progress);
                    Comm.write_to_buffer(d, "Note posted.\n\r", 0);
                    d.connected = CON_PLAYING;
                    ch.pcdata.in_progress = null;
                    Comm.act("{G$n finishes $s note.{x", ch, null, null, TO_ROOM);
                    break;

                case 'f':
                    Comm.write_to_buffer(d, "Note cancelled!\n\r", 0);
                    free_note(ch.pcdata.in_progress);
                    ch.pcdata.in_progress = null;
                    d.connected = CON_PLAYING;
                    break;

                default:
                    Comm.write_to_buffer(d, "Huh? Valid answers are:\n\r\n\r", 0);
                    Comm.send_to_desc(szFinishPrompt, d);
                    Comm.write_to_buffer(d, "\n\r", 0);
                    break;
            }
        }

        public static string ctime(long unix)
        {
            var dt = DateTimeOffset.FromUnixTimeSeconds(unix).ToLocalTime();
            return dt.ToString("ddd MMM ", CultureInfo.InvariantCulture)
                + dt.Day.ToString().PadLeft(2)
                + dt.ToString(" HH:mm:ss yyyy", CultureInfo.InvariantCulture);
        }
    }
}
