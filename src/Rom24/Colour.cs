using static Rom24.Merc;

namespace Rom24
{
    public static class Colour
    {
        public static string colour(char type, CharData ch)
        {
            if (ch != null && Bit.IS_NPC(ch))
                return "";

            var col = ch?.pcdata;

            switch (type)
            {
                case 'x':
                    return "\x1b[0m";
                case 'p':
                    return custom(col?.prompt);
                case 's':
                    return custom(col?.room_title);
                case 'S':
                    return custom(col?.room_text);
                case 'd':
                    return custom(col?.gossip);
                case '9':
                    return custom(col?.gossip_text);
                case 'Z':
                    return custom(col?.wiznet);
                case 'o':
                    return custom(col?.room_exits);
                case 'O':
                    return custom(col?.room_things);
                case 'i':
                    return custom(col?.immtalk_text);
                case 'I':
                    return custom(col?.immtalk_type);
                case '2':
                    return custom(col?.fight_yhit);
                case '3':
                    return custom(col?.fight_ohit);
                case '4':
                    return custom(col?.fight_thit);
                case '5':
                    return custom(col?.fight_skill);
                case '1':
                    return custom(col?.fight_death);
                case '6':
                    return custom(col?.say);
                case '7':
                    return custom(col?.say_text);
                case 'k':
                    return custom(col?.tell);
                case 'K':
                    return custom(col?.tell_text);
                case 'l':
                    return custom(col?.reply);
                case 'L':
                    return custom(col?.reply_text);
                case 'n':
                    return custom(col?.gtell_text);
                case 'N':
                    return custom(col?.gtell_type);
                case 'a':
                    return custom(col?.auction);
                case 'A':
                    return custom(col?.auction_text);
                case 'q':
                    return custom(col?.question);
                case 'Q':
                    return custom(col?.question_text);
                case 'f':
                    return custom(col?.answer);
                case 'F':
                    return custom(col?.answer_text);
                case 'e':
                    return custom(col?.music);
                case 'E':
                    return custom(col?.music_text);
                case 'h':
                    return custom(col?.quote);
                case 'H':
                    return custom(col?.quote_text);
                case 'j':
                    return custom(col?.info);
                case 'b':
                    return "\x1b[0;34m";
                case 'c':
                    return "\x1b[0;36m";
                case 'g':
                    return "\x1b[0;32m";
                case 'm':
                    return "\x1b[0;35m";
                case 'r':
                    return "\x1b[0;31m";
                case 'w':
                    return "\x1b[0;37m";
                case 'y':
                    return "\x1b[0;33m";
                case 'B':
                    return "\x1b[1;34m";
                case 'C':
                    return "\x1b[1;36m";
                case 'G':
                    return "\x1b[1;32m";
                case 'M':
                    return "\x1b[1;35m";
                case 'R':
                    return "\x1b[1;31m";
                case 'W':
                    return "\x1b[1;37m";
                case 'Y':
                    return "\x1b[1;33m";
                case 'D':
                    return "\x1b[1;30m";
                case '*':
                    return "\a";
                case '/':
                    return "\n\r";
                case '-':
                    return "~";
                case '{':
                    return "{";
                default:
                    return "\x1b[0m";
            }
        }

        static string custom(int[] trip)
        {
            if (trip == null)
                return "\x1b[0m";
            if (trip[2] != 0)
                return $"\x1b[{trip[0]};3{trip[1]}m\a";
            return $"\x1b[{trip[0]};3{trip[1]}m";
        }

        public static string colour_string(string txt, CharData ch, bool useColour)
            => colour_string(txt, ch, useColour, false);

        /* comm.c colourconv: skip '{' immediately before '\n' */
        public static string colourconv(string txt, CharData ch)
        {
            if (ch?.desc == null || txt == null)
                return "";
            bool useColour = Bit.IS_SET(ch.act, PLR_COLOUR);
            return colour_string(txt, ch, useColour, true);
        }

        public static string colour_string(string txt, CharData ch, bool useColour, bool convGuard)
        {
            if (string.IsNullOrEmpty(txt)) return "";
            var sb = new System.Text.StringBuilder(txt.Length * 2);
            for (int i = 0; i < txt.Length; i++)
            {
                if (txt[i] == '{')
                {
                    if (i + 1 >= txt.Length)
                    {
                        if (useColour)
                            sb.Append(colour('\0', ch));
                        continue;
                    }
                    i++;
                    if (useColour)
                    {
                        /* colourconv: '{' immediately before '\n' copies the newline */
                        if (convGuard && txt[i] == '\n')
                        {
                            sb.Append(txt[i]);
                            continue;
                        }
                        sb.Append(colour(txt[i], ch));
                    }
                    continue;
                }
                sb.Append(txt[i]);
            }
            return sb.ToString();
        }
    }
}
