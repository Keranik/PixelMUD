using static Rom24.Merc;

namespace Rom24
{
    public static class ActComm
    {
        public static void do_say(CharData ch, string argument)
        {
            if (argument.Length == 0)
            {
                Comm.send_to_char("Say what?\n\r", ch);
                return;
            }

            Comm.act("{6$n says '{7$T{6'{x", ch, null, argument, TO_ROOM, "say", argument);
            Comm.act("{6You say '{7$T{6'{x", ch, null, argument, TO_CHAR, "say", argument);

            if (!Bit.IS_NPC(ch))
            {
                for (var mob = ch.in_room.people; mob != null; )
                {
                    var mob_next = mob.next_in_room;
                    if (Bit.IS_NPC(mob) && Bit.HAS_TRIGGER(mob, TRIG_SPEECH)
                        && mob.position == mob.pIndexData.default_pos)
                        MobProg.mp_act_trigger(argument, mob, ch, null, null, (int)TRIG_SPEECH);
                    mob = mob_next;
                }
            }
        }

        public static void do_channels(CharData ch, string argument)
        {
            Comm.send_to_char("   channel     status\n\r", ch);
            Comm.send_to_char("---------------------\n\r", ch);

            Comm.send_to_char("{dgossip{x         ", ch);
            if (!Bit.IS_SET(ch.comm, COMM_NOGOSSIP))
                Comm.send_to_char("ON\n\r", ch);
            else
                Comm.send_to_char("OFF\n\r", ch);

            Comm.send_to_char("{aauction{x        ", ch);
            if (!Bit.IS_SET(ch.comm, COMM_NOAUCTION))
                Comm.send_to_char("ON\n\r", ch);
            else
                Comm.send_to_char("OFF\n\r", ch);

            Comm.send_to_char("{emusic{x          ", ch);
            if (!Bit.IS_SET(ch.comm, COMM_NOMUSIC))
                Comm.send_to_char("ON\n\r", ch);
            else
                Comm.send_to_char("OFF\n\r", ch);

            Comm.send_to_char("{qQ{x/{fA{x            ", ch);
            if (!Bit.IS_SET(ch.comm, COMM_NOQUESTION))
                Comm.send_to_char("ON\n\r", ch);
            else
                Comm.send_to_char("OFF\n\r", ch);

            Comm.send_to_char("{hQuote{x          ", ch);
            if (!Bit.IS_SET(ch.comm, COMM_NOQUOTE))
                Comm.send_to_char("ON\n\r", ch);
            else
                Comm.send_to_char("OFF\n\r", ch);

            Comm.send_to_char("{tgrats{x          ", ch);
            if (!Bit.IS_SET(ch.comm, COMM_NOGRATS))
                Comm.send_to_char("ON\n\r", ch);
            else
                Comm.send_to_char("OFF\n\r", ch);

            if (Bit.IS_IMMORTAL(ch))
            {
                Comm.send_to_char("{igod channel{x    ", ch);
                if (!Bit.IS_SET(ch.comm, COMM_NOWIZ))
                    Comm.send_to_char("ON\n\r", ch);
                else
                    Comm.send_to_char("OFF\n\r", ch);
            }

            Comm.send_to_char("{tshouts{x         ", ch);
            if (!Bit.IS_SET(ch.comm, COMM_SHOUTSOFF))
                Comm.send_to_char("ON\n\r", ch);
            else
                Comm.send_to_char("OFF\n\r", ch);

            Comm.send_to_char("{ktells{x          ", ch);
            if (!Bit.IS_SET(ch.comm, COMM_DEAF))
                Comm.send_to_char("ON\n\r", ch);
            else
                Comm.send_to_char("OFF\n\r", ch);

            Comm.send_to_char("{tquiet mode{x     ", ch);
            if (Bit.IS_SET(ch.comm, COMM_QUIET))
                Comm.send_to_char("ON\n\r", ch);
            else
                Comm.send_to_char("OFF\n\r", ch);

            if (Bit.IS_SET(ch.comm, COMM_AFK))
                Comm.send_to_char("You are AFK.\n\r", ch);

            if (Bit.IS_SET(ch.comm, COMM_SNOOP_PROOF))
                Comm.send_to_char("You are immune to snooping.\n\r", ch);

            if (ch.lines != PAGELEN)
            {
                if (ch.lines != 0)
                {
                    Comm.send_to_char(RomString.sprintf("You display %d lines of scroll.\n\r",
                        ch.lines + 2), ch);
                }
                else
                    Comm.send_to_char("Scroll buffering is off.\n\r", ch);
            }

            if (ch.prompt != null)
            {
                Comm.send_to_char(RomString.sprintf("Your current prompt is: %s\n\r", ch.prompt), ch);
            }

            if (Bit.IS_SET(ch.comm, COMM_NOSHOUT))
                Comm.send_to_char("You cannot shout.\n\r", ch);

            if (Bit.IS_SET(ch.comm, COMM_NOTELL))
                Comm.send_to_char("You cannot use tell.\n\r", ch);

            if (Bit.IS_SET(ch.comm, COMM_NOCHANNELS))
                Comm.send_to_char("You cannot use channels.\n\r", ch);

            if (Bit.IS_SET(ch.comm, COMM_NOEMOTE))
                Comm.send_to_char("You cannot show emotions.\n\r", ch);
        }

        public static void do_deaf(CharData ch, string argument)
        {
            if (Bit.IS_SET(ch.comm, COMM_DEAF))
            {
                Comm.send_to_char("You can now hear tells again.\n\r", ch);
                Bit.REMOVE_BIT(ref ch.comm, COMM_DEAF);
            }
            else
            {
                Comm.send_to_char("From now on, you won't hear tells.\n\r", ch);
                Bit.SET_BIT(ref ch.comm, COMM_DEAF);
            }
        }

        public static void do_quiet(CharData ch, string argument)
        {
            if (Bit.IS_SET(ch.comm, COMM_QUIET))
            {
                Comm.send_to_char("Quiet mode removed.\n\r", ch);
                Bit.REMOVE_BIT(ref ch.comm, COMM_QUIET);
            }
            else
            {
                Comm.send_to_char("From now on, you will only hear says and emotes.\n\r",
                    ch);
                Bit.SET_BIT(ref ch.comm, COMM_QUIET);
            }
        }

        public static void do_afk(CharData ch, string argument)
        {
            if (Bit.IS_SET(ch.comm, COMM_AFK))
            {
                Comm.send_to_char("AFK mode removed. Type 'replay' to see tells.\n\r",
                    ch);
                Bit.REMOVE_BIT(ref ch.comm, COMM_AFK);
            }
            else
            {
                Comm.send_to_char("You are now in AFK mode.\n\r", ch);
                Bit.SET_BIT(ref ch.comm, COMM_AFK);
            }
        }

        public static void do_replay(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
            {
                Comm.send_to_char("You can't replay.\n\r", ch);
                return;
            }

            if (ch.pcdata.buffer.Length == 0)
            {
                Comm.send_to_char("You have no tells to replay.\n\r", ch);
                return;
            }

            Comm.page_to_char(ch.pcdata.buffer.ToString(), ch);
            ch.pcdata.buffer.Clear();
        }

        public static void do_grats(CharData ch, string argument)
        {
            if (argument.Length == 0)
            {
                if (Bit.IS_SET(ch.comm, COMM_NOGRATS))
                {
                    Comm.send_to_char("Grats channel is now ON.\n\r", ch);
                    Bit.REMOVE_BIT(ref ch.comm, COMM_NOGRATS);
                }
                else
                {
                    Comm.send_to_char("Grats channel is now OFF.\n\r", ch);
                    Bit.SET_BIT(ref ch.comm, COMM_NOGRATS);
                }
            }
            else
            {
                if (Bit.IS_SET(ch.comm, COMM_QUIET))
                {
                    Comm.send_to_char("You must turn off quiet mode first.\n\r", ch);
                    return;
                }

                if (Bit.IS_SET(ch.comm, COMM_NOCHANNELS))
                {
                    Comm.send_to_char(
                        "The gods have revoked your channel priviliges.\n\r", ch);
                    return;
                }

                Bit.REMOVE_BIT(ref ch.comm, COMM_NOGRATS);
                Comm.send_to_char(RomString.sprintf("{tYou grats '%s'{x\n\r", argument), ch);
                Gmcp.Channel(ch.desc, "grats", ch.name, argument);
                for (var d = Game.descriptor_list; d != null; d = d.next)
                {
                    var victim = d.original != null ? d.original : d.character;
                    if (d.connected == CON_PLAYING &&
                        d.character != ch &&
                        !Bit.IS_SET(victim.comm, COMM_NOGRATS) &&
                        !Bit.IS_SET(victim.comm, COMM_QUIET))
                    {
                        Comm.act_new("{t$n grats '$t'{x",
                            ch, argument, d.character, TO_VICT, POS_SLEEPING, "grats", argument);
                    }
                }
            }
        }

        public static void do_quote(CharData ch, string argument)
        {
            if (argument.Length == 0)
            {
                if (Bit.IS_SET(ch.comm, COMM_NOQUOTE))
                {
                    Comm.send_to_char("{hQuote channel is now ON.{x\n\r", ch);
                    Bit.REMOVE_BIT(ref ch.comm, COMM_NOQUOTE);
                }
                else
                {
                    Comm.send_to_char("{hQuote channel is now OFF.{x\n\r", ch);
                    Bit.SET_BIT(ref ch.comm, COMM_NOQUOTE);
                }
            }
            else
            {
                if (Bit.IS_SET(ch.comm, COMM_QUIET))
                {
                    Comm.send_to_char("You must turn off quiet mode first.\n\r", ch);
                    return;
                }

                if (Bit.IS_SET(ch.comm, COMM_NOCHANNELS))
                {
                    Comm.send_to_char(
                        "The gods have revoked your channel priviliges.\n\r", ch);
                    return;
                }

                Bit.REMOVE_BIT(ref ch.comm, COMM_NOQUOTE);
                Comm.send_to_char(RomString.sprintf("{hYou quote '{H%s{h'{x\n\r", argument), ch);
                Gmcp.Channel(ch.desc, "quote", ch.name, argument);
                for (var d = Game.descriptor_list; d != null; d = d.next)
                {
                    var victim = d.original != null ? d.original : d.character;
                    if (d.connected == CON_PLAYING &&
                        d.character != ch &&
                        !Bit.IS_SET(victim.comm, COMM_NOQUOTE) &&
                        !Bit.IS_SET(victim.comm, COMM_QUIET))
                    {
                        Comm.act_new("{h$n quotes '{H$t{h'{x",
                            ch, argument, d.character, TO_VICT, POS_SLEEPING, "quote", argument);
                    }
                }
            }
        }

        public static void do_answer(CharData ch, string argument)
        {
            if (argument.Length == 0)
            {
                if (Bit.IS_SET(ch.comm, COMM_NOQUESTION))
                {
                    Comm.send_to_char("Q/A channel is now ON.\n\r", ch);
                    Bit.REMOVE_BIT(ref ch.comm, COMM_NOQUESTION);
                }
                else
                {
                    Comm.send_to_char("Q/A channel is now OFF.\n\r", ch);
                    Bit.SET_BIT(ref ch.comm, COMM_NOQUESTION);
                }
            }
            else
            {
                if (Bit.IS_SET(ch.comm, COMM_QUIET))
                {
                    Comm.send_to_char("You must turn off quiet mode first.\n\r", ch);
                    return;
                }

                if (Bit.IS_SET(ch.comm, COMM_NOCHANNELS))
                {
                    Comm.send_to_char(
                        "The gods have revoked your channel priviliges.\n\r", ch);
                    return;
                }

                Bit.REMOVE_BIT(ref ch.comm, COMM_NOQUESTION);
                Comm.send_to_char(RomString.sprintf("{fYou answer '{F%s{f'{x\n\r", argument), ch);
                Gmcp.Channel(ch.desc, "answer", ch.name, argument);
                for (var d = Game.descriptor_list; d != null; d = d.next)
                {
                    var victim = d.original != null ? d.original : d.character;
                    if (d.connected == CON_PLAYING &&
                        d.character != ch &&
                        !Bit.IS_SET(victim.comm, COMM_NOQUESTION) &&
                        !Bit.IS_SET(victim.comm, COMM_QUIET))
                    {
                        Comm.act_new("{f$n answers '{F$t{f'{x",
                            ch, argument, d.character, TO_VICT, POS_SLEEPING, "answer", argument);
                    }
                }
            }
        }

        public static void do_question(CharData ch, string argument)
        {
            if (argument.Length == 0)
            {
                if (Bit.IS_SET(ch.comm, COMM_NOQUESTION))
                {
                    Comm.send_to_char("Q/A channel is now ON.\n\r", ch);
                    Bit.REMOVE_BIT(ref ch.comm, COMM_NOQUESTION);
                }
                else
                {
                    Comm.send_to_char("Q/A channel is now OFF.\n\r", ch);
                    Bit.SET_BIT(ref ch.comm, COMM_NOQUESTION);
                }
            }
            else
            {
                if (Bit.IS_SET(ch.comm, COMM_QUIET))
                {
                    Comm.send_to_char("You must turn off quiet mode first.\n\r", ch);
                    return;
                }

                if (Bit.IS_SET(ch.comm, COMM_NOCHANNELS))
                {
                    Comm.send_to_char(
                        "The gods have revoked your channel priviliges.\n\r", ch);
                    return;
                }

                Bit.REMOVE_BIT(ref ch.comm, COMM_NOQUESTION);
                Comm.send_to_char(RomString.sprintf("{qYou question '{Q%s{q'{x\n\r", argument), ch);
                Gmcp.Channel(ch.desc, "question", ch.name, argument);
                for (var d = Game.descriptor_list; d != null; d = d.next)
                {
                    var victim = d.original != null ? d.original : d.character;
                    if (d.connected == CON_PLAYING &&
                        d.character != ch &&
                        !Bit.IS_SET(victim.comm, COMM_NOQUESTION) &&
                        !Bit.IS_SET(victim.comm, COMM_QUIET))
                    {
                        Comm.act_new("{q$n questions '{Q$t{q'{x",
                            ch, argument, d.character, TO_VICT, POS_SLEEPING, "question", argument);
                    }
                }
            }
        }

        public static void do_shout(CharData ch, string argument)
        {
            if (argument.Length == 0)
            {
                if (Bit.IS_SET(ch.comm, COMM_SHOUTSOFF))
                {
                    Comm.send_to_char("You can hear shouts again.\n\r", ch);
                    Bit.REMOVE_BIT(ref ch.comm, COMM_SHOUTSOFF);
                }
                else
                {
                    Comm.send_to_char("You will no longer hear shouts.\n\r", ch);
                    Bit.SET_BIT(ref ch.comm, COMM_SHOUTSOFF);
                }
                return;
            }

            if (Bit.IS_SET(ch.comm, COMM_NOSHOUT))
            {
                Comm.send_to_char("You can't shout.\n\r", ch);
                return;
            }

            Bit.REMOVE_BIT(ref ch.comm, COMM_SHOUTSOFF);
            Bit.WAIT_STATE(ch, 12);
            Comm.act("You shout '$T'", ch, null, argument, TO_CHAR, "shout", argument);
            for (var d = Game.descriptor_list; d != null; d = d.next)
            {
                var victim = d.original != null ? d.original : d.character;
                if (d.connected == CON_PLAYING &&
                    d.character != ch &&
                    !Bit.IS_SET(victim.comm, COMM_SHOUTSOFF) &&
                    !Bit.IS_SET(victim.comm, COMM_QUIET))
                {
                    Comm.act("$n shouts '$t'", ch, argument, d.character, TO_VICT, "shout", argument);
                }
            }
        }

        public static void do_colour(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
            {
                Comm.send_to_char_bw("ColoUr is not ON, Way Moron!\n\r", ch);
                return;
            }

            argument = RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                if (!Bit.IS_SET(ch.act, PLR_COLOUR))
                {
                    Bit.SET_BIT(ref ch.act, PLR_COLOUR);
                    Comm.send_to_char("ColoUr is now ON, Way Cool!\n\r"
                        + "Further syntax:\n\r   colour {c<{xfield{c> <{xcolour{c>{x\n\r"
                        + "   colour {c<{xfield{c>{x {cbeep{x|{cnobeep{x\n\r"
                        + "Type help {ccolour{x and {ccolour2{x for details.\n\r"
                        + "ColoUr is brought to you by Lope, ant@solace.mh.se.\n\r",
                        ch);
                }
                else
                {
                    Comm.send_to_char_bw("ColoUr is now OFF, <sigh>\n\r", ch);
                    Bit.REMOVE_BIT(ref ch.act, PLR_COLOUR);
                }
                return;
            }

            if (!RomString.str_cmp(arg, "default"))
            {
                Handler.default_colour(ch);
                Comm.send_to_char_bw("ColoUr setting set to default values.\n\r", ch);
                return;
            }

            if (!RomString.str_cmp(arg, "all"))
            {
                Handler.all_colour(ch, argument);
                return;
            }

            int[] field = null;
            if (!RomString.str_cmp(arg, "text")) field = ch.pcdata.text;
            else if (!RomString.str_cmp(arg, "auction")) field = ch.pcdata.auction;
            else if (!RomString.str_cmp(arg, "auction_text")) field = ch.pcdata.auction_text;
            else if (!RomString.str_cmp(arg, "gossip")) field = ch.pcdata.gossip;
            else if (!RomString.str_cmp(arg, "gossip_text")) field = ch.pcdata.gossip_text;
            else if (!RomString.str_cmp(arg, "music")) field = ch.pcdata.music;
            else if (!RomString.str_cmp(arg, "music_text")) field = ch.pcdata.music_text;
            else if (!RomString.str_cmp(arg, "question")) field = ch.pcdata.question;
            else if (!RomString.str_cmp(arg, "question_text")) field = ch.pcdata.question_text;
            else if (!RomString.str_cmp(arg, "answer")) field = ch.pcdata.answer;
            else if (!RomString.str_cmp(arg, "answer_text")) field = ch.pcdata.answer_text;
            else if (!RomString.str_cmp(arg, "quote")) field = ch.pcdata.quote;
            else if (!RomString.str_cmp(arg, "quote_text")) field = ch.pcdata.quote_text;
            else if (!RomString.str_cmp(arg, "immtalk_text")) field = ch.pcdata.immtalk_text;
            else if (!RomString.str_cmp(arg, "immtalk_type")) field = ch.pcdata.immtalk_type;
            else if (!RomString.str_cmp(arg, "info")) field = ch.pcdata.info;
            else if (!RomString.str_cmp(arg, "say")) field = ch.pcdata.say;
            else if (!RomString.str_cmp(arg, "say_text")) field = ch.pcdata.say_text;
            else if (!RomString.str_cmp(arg, "tell")) field = ch.pcdata.tell;
            else if (!RomString.str_cmp(arg, "tell_text")) field = ch.pcdata.tell_text;
            else if (!RomString.str_cmp(arg, "reply")) field = ch.pcdata.reply;
            else if (!RomString.str_cmp(arg, "reply_text")) field = ch.pcdata.reply_text;
            else if (!RomString.str_cmp(arg, "gtell_text")) field = ch.pcdata.gtell_text;
            else if (!RomString.str_cmp(arg, "gtell_type")) field = ch.pcdata.gtell_type;
            else if (!RomString.str_cmp(arg, "wiznet")) field = ch.pcdata.wiznet;
            else if (!RomString.str_cmp(arg, "room_title")) field = ch.pcdata.room_title;
            else if (!RomString.str_cmp(arg, "room_text")) field = ch.pcdata.room_text;
            else if (!RomString.str_cmp(arg, "room_exits")) field = ch.pcdata.room_exits;
            else if (!RomString.str_cmp(arg, "room_things")) field = ch.pcdata.room_things;
            else if (!RomString.str_cmp(arg, "prompt")) field = ch.pcdata.prompt;
            else if (!RomString.str_cmp(arg, "fight_death")) field = ch.pcdata.fight_death;
            else if (!RomString.str_cmp(arg, "fight_yhit")) field = ch.pcdata.fight_yhit;
            else if (!RomString.str_cmp(arg, "fight_ohit")) field = ch.pcdata.fight_ohit;
            else if (!RomString.str_cmp(arg, "fight_thit")) field = ch.pcdata.fight_thit;
            else if (!RomString.str_cmp(arg, "fight_skill")) field = ch.pcdata.fight_skill;
            else
            {
                Comm.send_to_char_bw("Unrecognised Colour Parameter Not Set.\n\r", ch);
                return;
            }

            if (!Handler.alter_colour(ch, field, argument))
                return;
            Comm.send_to_char_bw("New Colour Parameter Set.\n\r", ch);
        }

        public static void do_auction(CharData ch, string argument)
        {
            if (argument.Length == 0)
            {
                if (Bit.IS_SET(ch.comm, COMM_NOAUCTION))
                {
                    Comm.send_to_char("{aAuction channel is now ON.{x\n\r", ch);
                    Bit.REMOVE_BIT(ref ch.comm, COMM_NOAUCTION);
                }
                else
                {
                    Comm.send_to_char("{aAuction channel is now OFF.{x\n\r", ch);
                    Bit.SET_BIT(ref ch.comm, COMM_NOAUCTION);
                }
            }
            else
            {
                if (Bit.IS_SET(ch.comm, COMM_QUIET))
                {
                    Comm.send_to_char("You must turn off quiet mode first.\n\r", ch);
                    return;
                }

                if (Bit.IS_SET(ch.comm, COMM_NOCHANNELS))
                {
                    Comm.send_to_char(
                        "The gods have revoked your channel priviliges.\n\r", ch);
                    return;
                }

                Bit.REMOVE_BIT(ref ch.comm, COMM_NOAUCTION);
            }

            Comm.send_to_char(RomString.sprintf("{aYou auction '{A%s{a'{x\n\r", argument), ch);
            Gmcp.Channel(ch.desc, "auction", ch.name, argument);
            for (var d = Game.descriptor_list; d != null; d = d.next)
            {
                var victim = d.original != null ? d.original : d.character;

                if (d.connected == CON_PLAYING &&
                    d.character != ch &&
                    !Bit.IS_SET(victim.comm, COMM_NOAUCTION) &&
                    !Bit.IS_SET(victim.comm, COMM_QUIET))
                {
                    Comm.act_new("{a$n auctions '{A$t{a'{x",
                        ch, argument, d.character, TO_VICT, POS_DEAD, "auction", argument);
                }
            }
        }

        public static void do_gossip(CharData ch, string argument)
        {
            if (argument.Length == 0)
            {
                if (Bit.IS_SET(ch.comm, COMM_NOGOSSIP))
                {
                    Comm.send_to_char("Gossip channel is now ON.\n\r", ch);
                    Bit.REMOVE_BIT(ref ch.comm, COMM_NOGOSSIP);
                }
                else
                {
                    Comm.send_to_char("Gossip channel is now OFF.\n\r", ch);
                    Bit.SET_BIT(ref ch.comm, COMM_NOGOSSIP);
                }
            }
            else
            {
                if (Bit.IS_SET(ch.comm, COMM_QUIET))
                {
                    Comm.send_to_char("You must turn off quiet mode first.\n\r", ch);
                    return;
                }

                if (Bit.IS_SET(ch.comm, COMM_NOCHANNELS))
                {
                    Comm.send_to_char(
                        "The gods have revoked your channel priviliges.\n\r", ch);
                    return;
                }

                Bit.REMOVE_BIT(ref ch.comm, COMM_NOGOSSIP);

                Comm.send_to_char(RomString.sprintf("{dYou gossip '{9%s{d'{x\n\r", argument), ch);
                Gmcp.Channel(ch.desc, "gossip", ch.name, argument);
                for (var d = Game.descriptor_list; d != null; d = d.next)
                {
                    var victim = d.original != null ? d.original : d.character;

                    if (d.connected == CON_PLAYING &&
                        d.character != ch &&
                        !Bit.IS_SET(victim.comm, COMM_NOGOSSIP) &&
                        !Bit.IS_SET(victim.comm, COMM_QUIET))
                    {
                        Comm.act_new("{d$n gossips '{9$t{d'{x",
                            ch, argument, d.character, TO_VICT, POS_SLEEPING, "gossip", argument);
                    }
                }
            }
        }

        public static void do_immtalk(CharData ch, string argument)
        {
            if (argument.Length == 0)
            {
                if (Bit.IS_SET(ch.comm, COMM_NOWIZ))
                {
                    Comm.send_to_char("Immortal channel is now ON\n\r", ch);
                    Bit.REMOVE_BIT(ref ch.comm, COMM_NOWIZ);
                }
                else
                {
                    Comm.send_to_char("Immortal channel is now OFF\n\r", ch);
                    Bit.SET_BIT(ref ch.comm, COMM_NOWIZ);
                }
                return;
            }

            Bit.REMOVE_BIT(ref ch.comm, COMM_NOWIZ);

            Comm.act_new("{i[{I$n{i]: $t{x", ch, argument, null, TO_CHAR, POS_DEAD, "immtalk", argument);
            for (var d = Game.descriptor_list; d != null; d = d.next)
            {
                if (d.connected == CON_PLAYING &&
                    Bit.IS_IMMORTAL(d.character) &&
                    !Bit.IS_SET(d.character.comm, COMM_NOWIZ))
                {
                    Comm.act_new("{i[{I$n{i]: $t{x", ch, argument, d.character, TO_VICT,
                        POS_DEAD, "immtalk", argument);
                }
            }
        }

        public static void do_gtell(CharData ch, string argument)
        {
            if (argument.Length == 0)
            {
                Comm.send_to_char("Tell your group what?\n\r", ch);
                return;
            }

            if (Bit.IS_SET(ch.comm, COMM_NOTELL))
            {
                Comm.send_to_char("Your message didn't get through!\n\r", ch);
                return;
            }

            for (var gch = Game.char_list; gch != null; gch = gch.next)
            {
                if (Handler.is_same_group(gch, ch))
                    Comm.act_new("$n tells the group '$t'",
                        ch, argument, gch, TO_VICT, POS_SLEEPING, "gtell", argument);
            }
        }

        public static void do_follow(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.send_to_char("Follow whom?\n\r", ch);
                return;
            }

            var victim = Handler.get_char_room(ch, arg);
            if (victim == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }

            if (Bit.IS_AFFECTED(ch, AFF_CHARM) && ch.master != null)
            {
                Comm.act("But you'd rather follow $N!", ch, null, ch.master, TO_CHAR);
                return;
            }

            if (victim == ch)
            {
                if (ch.master == null)
                {
                    Comm.send_to_char("You already follow yourself.\n\r", ch);
                    return;
                }
                stop_follower(ch);
                return;
            }

            if (!Bit.IS_NPC(victim) && Bit.IS_SET(victim.act, PLR_NOFOLLOW)
                && !Bit.IS_IMMORTAL(ch))
            {
                Comm.act("$N doesn't seem to want any followers.\n\r", ch, null, victim,
                    TO_CHAR);
                return;
            }

            Bit.REMOVE_BIT(ref ch.act, PLR_NOFOLLOW);

            if (ch.master != null)
                stop_follower(ch);

            add_follower(ch, victim);
        }

        public static void do_tell(CharData ch, string argument)
        {
            if (Bit.IS_SET(ch.comm, COMM_NOTELL) || Bit.IS_SET(ch.comm, COMM_DEAF))
            {
                Comm.send_to_char("Your message didn't get through.\n\r", ch);
                return;
            }

            if (Bit.IS_SET(ch.comm, COMM_QUIET))
            {
                Comm.send_to_char("You must turn off quiet mode first.\n\r", ch);
                return;
            }

            if (Bit.IS_SET(ch.comm, COMM_DEAF))
            {
                Comm.send_to_char("You must turn off deaf mode first.\n\r", ch);
                return;
            }

            argument = RomString.one_argument(argument, out string arg);

            if (arg.Length == 0 || argument.Length == 0)
            {
                Comm.send_to_char("Tell whom what?\n\r", ch);
                return;
            }

            var victim = Handler.get_char_world(ch, arg);
            if (victim == null
                || (Bit.IS_NPC(victim) && victim.in_room != ch.in_room))
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }

            if (victim.desc == null && !Bit.IS_NPC(victim))
            {
                Comm.act("$N seems to have misplaced $S link...try again later.",
                    ch, null, victim, TO_CHAR);
                string buf = RomString.sprintf("{k%s tells you '{K%s{k'{x\n\r", Handler.PERS(ch, victim),
                    argument);
                if (buf.Length > 0)
                    buf = Bit.UPPER(buf[0]) + buf.Substring(1);
                victim.pcdata.buffer.Append(buf);
                return;
            }

            if (!(Bit.IS_IMMORTAL(ch) && ch.level > LEVEL_IMMORTAL)
                && !Bit.IS_AWAKE(victim))
            {
                Comm.act("$E can't hear you.", ch, null, victim, TO_CHAR);
                return;
            }

            if (
                (Bit.IS_SET(victim.comm, COMM_QUIET)
                 || Bit.IS_SET(victim.comm, COMM_DEAF)) && !Bit.IS_IMMORTAL(ch))
            {
                Comm.act("$E is not receiving tells.", ch, null, victim, TO_CHAR);
                return;
            }

            if (Bit.IS_SET(victim.comm, COMM_AFK))
            {
                if (Bit.IS_NPC(victim))
                {
                    Comm.act("$E is AFK, and not receiving tells.",
                        ch, null, victim, TO_CHAR);
                    return;
                }

                Comm.act("$E is AFK, but your tell will go through when $E returns.",
                    ch, null, victim, TO_CHAR);
                string buf = RomString.sprintf("{k%s tells you '{K%s{k'{x\n\r", Handler.PERS(ch, victim),
                    argument);
                if (buf.Length > 0)
                    buf = Bit.UPPER(buf[0]) + buf.Substring(1);
                victim.pcdata.buffer.Append(buf);
                return;
            }

            if (victim.desc != null
                && victim.desc.connected >= CON_NOTE_TO && victim.desc.connected <= CON_NOTE_FINISH)
            {
                Comm.act("$E is writing a note, but your tell will go through when $E returns.",
                    ch, null, victim, TO_CHAR);
                string buf = RomString.sprintf("{k%s tells you '{K%s{k'{x\n\r", Handler.PERS(ch, victim), argument);
                if (buf.Length > 0)
                    buf = Bit.UPPER(buf[0]) + buf.Substring(1);
                victim.pcdata.buffer.Append(buf);
                return;
            }

            Comm.act("{kYou tell $N '{K$t{k'{x", ch, argument, victim, TO_CHAR, "tell", argument);
            Comm.act_new("{k$n tells you '{K$t{k'{x", ch, argument, victim, TO_VICT,
                POS_DEAD, "tell", argument);
            victim.reply = ch;

            if (!Bit.IS_NPC(ch) && Bit.IS_NPC(victim) && Bit.HAS_TRIGGER(victim, TRIG_SPEECH))
                MobProg.mp_act_trigger(argument, victim, ch, null, null, (int)TRIG_SPEECH);
        }

        public static void do_reply(CharData ch, string argument)
        {
            if (Bit.IS_SET(ch.comm, COMM_NOTELL))
            {
                Comm.send_to_char("Your message didn't get through.\n\r", ch);
                return;
            }

            var victim = ch.reply;
            if (victim == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }

            if (victim.desc == null && !Bit.IS_NPC(victim))
            {
                Comm.act("$N seems to have misplaced $S link...try again later.",
                    ch, null, victim, TO_CHAR);
                string buf = RomString.sprintf("{k%s tells you '{K%s{k'{x\n\r", Handler.PERS(ch, victim),
                    argument);
                if (buf.Length > 0)
                    buf = Bit.UPPER(buf[0]) + buf.Substring(1);
                victim.pcdata.buffer.Append(buf);
                return;
            }

            if (!Bit.IS_IMMORTAL(ch) && !Bit.IS_AWAKE(victim))
            {
                Comm.act("$E can't hear you.", ch, null, victim, TO_CHAR);
                return;
            }

            if (
                (Bit.IS_SET(victim.comm, COMM_QUIET)
                 || Bit.IS_SET(victim.comm, COMM_DEAF)) && !Bit.IS_IMMORTAL(ch)
                && !Bit.IS_IMMORTAL(victim))
            {
                Comm.act_new("$E is not receiving tells.", ch, null, victim, TO_CHAR,
                    POS_DEAD);
                return;
            }

            if (!Bit.IS_IMMORTAL(victim) && !Bit.IS_AWAKE(ch))
            {
                Comm.send_to_char("In your dreams, or what?\n\r", ch);
                return;
            }

            if (Bit.IS_SET(victim.comm, COMM_AFK))
            {
                if (Bit.IS_NPC(victim))
                {
                    Comm.act_new("$E is AFK, and not receiving tells.",
                        ch, null, victim, TO_CHAR, POS_DEAD);
                    return;
                }

                Comm.act_new("$E is AFK, but your tell will go through when $E returns.",
                    ch, null, victim, TO_CHAR, POS_DEAD);
                string buf = RomString.sprintf("{k%s tells you '{K%s{k'{x\n\r", Handler.PERS(ch, victim),
                    argument);
                if (buf.Length > 0)
                    buf = Bit.UPPER(buf[0]) + buf.Substring(1);
                victim.pcdata.buffer.Append(buf);
                return;
            }

            Comm.act_new("{kYou tell $N '{K$t{k'{x", ch, argument, victim, TO_CHAR,
                POS_DEAD, "tell", argument);
            Comm.act_new("{k$n tells you '{K$t{k'{x", ch, argument, victim, TO_VICT,
                POS_DEAD, "tell", argument);
            victim.reply = ch;
        }

        public static void do_yell(CharData ch, string argument)
        {
            if (Bit.IS_SET(ch.comm, COMM_NOSHOUT))
            {
                Comm.send_to_char("You can't yell.\n\r", ch);
                return;
            }

            if (argument.Length == 0)
            {
                Comm.send_to_char("Yell what?\n\r", ch);
                return;
            }

            Comm.act("You yell '$t'", ch, argument, null, TO_CHAR, "yell", argument);
            for (var d = Game.descriptor_list; d != null; d = d.next)
            {
                if (d.connected == CON_PLAYING
                    && d.character != ch
                    && d.character.in_room != null
                    && d.character.in_room.area == ch.in_room.area
                    && !Bit.IS_SET(d.character.comm, COMM_QUIET))
                {
                    Comm.act("$n yells '$t'", ch, argument, d.character, TO_VICT, "yell", argument);
                }
            }
        }

        public static void do_emote(CharData ch, string argument)
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

            if (!((argument[0] >= 'A' && argument[0] <= 'Z')
                  || (argument[0] >= 'a' && argument[0] <= 'z'))
                || Bit.isspace(argument[0]))
            {
                Comm.send_to_char("Moron!\n\r", ch);
                return;
            }

            Game.MOBtrigger = false;
            Comm.act("$n $T", ch, null, argument, TO_ROOM);
            Comm.act("$n $T", ch, null, argument, TO_CHAR);
            Game.MOBtrigger = true;
        }

        public static void add_follower(CharData ch, CharData master)
        {
            if (ch.master != null)
            {
                Db.bug("Add_follower: non-null master.", 0);
                return;
            }

            ch.master = master;
            ch.leader = null;

            if (Handler.can_see(master, ch))
                Comm.act("$n now follows you.", ch, null, master, TO_VICT);

            Comm.act("You now follow $N.", ch, null, master, TO_CHAR);
        }

        public static void stop_follower(CharData ch)
        {
            if (ch.master == null)
            {
                Db.bug("Stop_follower: null master.", 0);
                return;
            }

            if (Bit.IS_AFFECTED(ch, AFF_CHARM))
            {
                Bit.REMOVE_BIT(ref ch.affected_by, AFF_CHARM);
                Handler.affect_strip(ch, Gsn.charm_person);
            }

            if (Handler.can_see(ch.master, ch) && ch.in_room != null)
            {
                Comm.act("$n stops following you.", ch, null, ch.master, TO_VICT);
                Comm.act("You stop following $N.", ch, null, ch.master, TO_CHAR);
            }
            if (ch.master.pet == ch)
                ch.master.pet = null;

            ch.master = null;
            ch.leader = null;
        }

        public static void nuke_pets(CharData ch)
        {
            var pet = ch.pet;
            if (pet != null)
            {
                stop_follower(pet);
                if (pet.in_room != null)
                    Comm.act("$N slowly fades away.", ch, null, pet, TO_NOTVICT);
                Handler.extract_char(pet, true);
            }
            ch.pet = null;
        }

        public static void die_follower(CharData ch)
        {
            if (ch.master != null)
            {
                if (ch.master.pet == ch)
                    ch.master.pet = null;
                stop_follower(ch);
            }

            ch.leader = null;

            for (var fch = Game.char_list; fch != null; fch = fch.next)
            {
                if (fch.master == ch)
                    stop_follower(fch);
                if (fch.leader == ch)
                    fch.leader = fch;
            }
        }

        public static void do_order(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg);
            RomString.one_argument(argument, out string arg2);

            if (!RomString.str_cmp(arg2, "delete") || !RomString.str_cmp(arg2, "mob"))
            {
                Comm.send_to_char("That will NOT be done.\n\r", ch);
                return;
            }

            if (arg.Length == 0 || argument.Length == 0)
            {
                Comm.send_to_char("Order whom to do what?\n\r", ch);
                return;
            }

            if (Bit.IS_AFFECTED(ch, AFF_CHARM))
            {
                Comm.send_to_char("You feel like taking, not giving, orders.\n\r", ch);
                return;
            }

            bool fAll;
            CharData victim;
            if (!RomString.str_cmp(arg, "all"))
            {
                fAll = true;
                victim = null;
            }
            else
            {
                fAll = false;
                victim = Handler.get_char_room(ch, arg);
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

                if (!Bit.IS_AFFECTED(victim, AFF_CHARM) || victim.master != ch
                    || (Bit.IS_IMMORTAL(victim) && victim.trust >= ch.trust))
                {
                    Comm.send_to_char("Do it yourself!\n\r", ch);
                    return;
                }
            }

            bool found = false;
            CharData och_next;
            for (var och = ch.in_room.people; och != null; och = och_next)
            {
                och_next = och.next_in_room;

                if (Bit.IS_AFFECTED(och, AFF_CHARM)
                    && och.master == ch && (fAll || och == victim))
                {
                    found = true;
                    string buf = RomString.sprintf("$n orders you to '%s'.", argument);
                    Comm.act(buf, ch, null, och, TO_VICT);
                    Interp.interpret(och, argument);
                }
            }

            if (found)
            {
                Bit.WAIT_STATE(ch, PULSE_VIOLENCE);
                Comm.send_to_char("Ok.\n\r", ch);
            }
            else
                Comm.send_to_char("You have no followers here.\n\r", ch);
        }

        public static void do_group(CharData ch, string argument)
        {
            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                var leader = (ch.leader != null) ? ch.leader : ch;
                Comm.send_to_char(RomString.sprintf("%s's group:\n\r", Handler.PERS(leader, ch)), ch);

                for (var gch = Game.char_list; gch != null; gch = gch.next)
                {
                    if (Handler.is_same_group(gch, ch))
                    {
                        Comm.send_to_char(RomString.sprintf(
                            "[%2d %s] %-16s %4d/%4d hp %4d/%4d mana %4d/%4d mv %5d xp\n\r",
                            gch.level,
                            Bit.IS_NPC(gch) ? "Mob" : Tables.class_table[gch.klass].who_name,
                            RomString.capitalize(Handler.PERS(gch, ch)), gch.hit, gch.max_hit,
                            gch.mana, gch.max_mana, gch.move, gch.max_move,
                            gch.exp), ch);
                    }
                }
                return;
            }

            var victim = Handler.get_char_room(ch, arg);
            if (victim == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
                return;
            }

            if (ch.master != null || (ch.leader != null && ch.leader != ch))
            {
                Comm.send_to_char("But you are following someone else!\n\r", ch);
                return;
            }

            if (victim.master != ch && ch != victim)
            {
                Comm.act_new("$N isn't following you.", ch, null, victim, TO_CHAR,
                    POS_SLEEPING);
                return;
            }

            if (Bit.IS_AFFECTED(victim, AFF_CHARM))
            {
                Comm.send_to_char("You can't remove charmed mobs from your group.\n\r",
                    ch);
                return;
            }

            if (Bit.IS_AFFECTED(ch, AFF_CHARM))
            {
                Comm.act_new("You like your master too much to leave $m!",
                    ch, null, victim, TO_VICT, POS_SLEEPING);
                return;
            }

            if (Handler.is_same_group(victim, ch) && ch != victim)
            {
                victim.leader = null;
                Comm.act_new("$n removes $N from $s group.",
                    ch, null, victim, TO_NOTVICT, POS_RESTING);
                Comm.act_new("$n removes you from $s group.",
                    ch, null, victim, TO_VICT, POS_SLEEPING);
                Comm.act_new("You remove $N from your group.",
                    ch, null, victim, TO_CHAR, POS_SLEEPING);
                return;
            }

            victim.leader = ch;
            Comm.act_new("$N joins $n's group.", ch, null, victim, TO_NOTVICT,
                POS_RESTING);
            Comm.act_new("You join $n's group.", ch, null, victim, TO_VICT, POS_SLEEPING);
            Comm.act_new("$N joins your group.", ch, null, victim, TO_CHAR, POS_SLEEPING);
        }

        public static void do_split(CharData ch, string argument)
        {
            argument = RomString.one_argument(argument, out string arg1);
            RomString.one_argument(argument, out string arg2);

            if (arg1.Length == 0)
            {
                Comm.send_to_char("Split how much?\n\r", ch);
                return;
            }

            int amount_silver = Interp.atoi(arg1);
            int amount_gold = 0;
            if (arg2.Length != 0)
                amount_gold = Interp.atoi(arg2);

            if (amount_gold < 0 || amount_silver < 0)
            {
                Comm.send_to_char("Your group wouldn't like that.\n\r", ch);
                return;
            }

            if (amount_gold == 0 && amount_silver == 0)
            {
                Comm.send_to_char("You hand out zero coins, but no one notices.\n\r", ch);
                return;
            }

            if (ch.gold < amount_gold || ch.silver < amount_silver)
            {
                Comm.send_to_char("You don't have that much to split.\n\r", ch);
                return;
            }

            int members = 0;
            for (var gch = ch.in_room.people; gch != null; gch = gch.next_in_room)
            {
                if (Handler.is_same_group(gch, ch) && !Bit.IS_AFFECTED(gch, AFF_CHARM))
                    members++;
            }

            if (members < 2)
            {
                Comm.send_to_char("Just keep it all.\n\r", ch);
                return;
            }

            int share_silver = amount_silver / members;
            int extra_silver = amount_silver % members;
            int share_gold = amount_gold / members;
            int extra_gold = amount_gold % members;

            if (share_gold == 0 && share_silver == 0)
            {
                Comm.send_to_char("Don't even bother, cheapskate.\n\r", ch);
                return;
            }

            ch.silver -= amount_silver;
            ch.silver += share_silver + extra_silver;
            ch.gold -= amount_gold;
            ch.gold += share_gold + extra_gold;

            if (share_silver > 0)
            {
                Comm.send_to_char(
                    $"You split {amount_silver} silver coins. Your share is {share_silver + extra_silver} silver.\n\r",
                    ch);
            }

            if (share_gold > 0)
            {
                Comm.send_to_char(
                    $"You split {amount_gold} gold coins. Your share is {share_gold + extra_gold} gold.\n\r",
                    ch);
            }

            string buf;
            if (share_gold == 0)
            {
                buf = $"$n splits {amount_silver} silver coins. Your share is {share_silver} silver.";
            }
            else if (share_silver == 0)
            {
                buf = $"$n splits {amount_gold} gold coins. Your share is {share_gold} gold.";
            }
            else
            {
                buf =
                    $"$n splits {amount_silver} silver and {amount_gold} gold coins, giving you {share_silver} silver and {share_gold} gold.\n\r";
            }

            for (var gch = ch.in_room.people; gch != null; gch = gch.next_in_room)
            {
                if (gch != ch && Handler.is_same_group(gch, ch)
                    && !Bit.IS_AFFECTED(gch, AFF_CHARM))
                {
                    Comm.act(buf, ch, null, gch, TO_VICT);
                    gch.gold += share_gold;
                    gch.silver += share_silver;
                }
            }
        }

        public static void do_delet(CharData ch, string argument)
        {
            Comm.send_to_char("You must type the full command to delete yourself.\n\r",
                ch);
        }

        public static void do_delete(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            if (ch.pcdata.confirm_delete)
            {
                if (argument.Length != 0)
                {
                    Comm.send_to_char("Delete status removed.\n\r", ch);
                    ch.pcdata.confirm_delete = false;
                    return;
                }
                else
                {
                    string strsave = Path.Combine(Game.player_dir, RomString.capitalize(ch.name));
                    Comm.wiznet("$N turns $Mself into line noise.", ch, null, 0, 0, 0);
                    Fight.stop_fighting(ch, true);
                    Interp.do_function(ch, Interp.do_quit, "");
                    File.Delete(strsave);
                    return;
                }
            }

            if (argument.Length != 0)
            {
                Comm.send_to_char("Just type delete. No argument.\n\r", ch);
                return;
            }

            Comm.send_to_char("Type delete again to confirm this command.\n\r", ch);
            Comm.send_to_char("WARNING: this command is irreversible.\n\r", ch);
            Comm.send_to_char(
                "Typing delete with an argument will undo delete status.\n\r", ch);
            ch.pcdata.confirm_delete = true;
            Comm.wiznet("$N is contemplating deletion.", ch, null, 0, 0, Handler.get_trust(ch));
        }

        public static void do_music(CharData ch, string argument)
        {
            if (argument.Length == 0)
            {
                if (Bit.IS_SET(ch.comm, COMM_NOMUSIC))
                {
                    Comm.send_to_char("Music channel is now ON.\n\r", ch);
                    Bit.REMOVE_BIT(ref ch.comm, COMM_NOMUSIC);
                }
                else
                {
                    Comm.send_to_char("Music channel is now OFF.\n\r", ch);
                    Bit.SET_BIT(ref ch.comm, COMM_NOMUSIC);
                }
            }
            else
            {
                if (Bit.IS_SET(ch.comm, COMM_QUIET))
                {
                    Comm.send_to_char("You must turn off quiet mode first.\n\r", ch);
                    return;
                }

                if (Bit.IS_SET(ch.comm, COMM_NOCHANNELS))
                {
                    Comm.send_to_char(
                        "The gods have revoked your channel priviliges.\n\r", ch);
                    return;
                }

                Bit.REMOVE_BIT(ref ch.comm, COMM_NOMUSIC);

                Comm.send_to_char(RomString.sprintf("{eYou MUSIC: '{E%s{e'{x\n\r", argument), ch);
                Gmcp.Channel(ch.desc, "music", ch.name, argument);
                for (var d = Game.descriptor_list; d != null; d = d.next)
                {
                    var victim = d.original != null ? d.original : d.character;

                    if (d.connected == CON_PLAYING &&
                        d.character != ch &&
                        !Bit.IS_SET(victim.comm, COMM_NOMUSIC) &&
                        !Bit.IS_SET(victim.comm, COMM_QUIET))
                    {
                        Comm.act_new("{e$n MUSIC: '{E$t{e'{x",
                            ch, argument, d.character, TO_VICT, POS_SLEEPING, "music", argument);
                    }
                }
            }
        }

        public static void do_clantalk(CharData ch, string argument)
        {
            if (!Handler.is_clan(ch) || Tables.clan_table[ch.clan].independent)
            {
                Comm.send_to_char("You aren't in a clan.\n\r", ch);
                return;
            }
            if (argument.Length == 0)
            {
                if (Bit.IS_SET(ch.comm, COMM_NOCLAN))
                {
                    Comm.send_to_char("Clan channel is now ON\n\r", ch);
                    Bit.REMOVE_BIT(ref ch.comm, COMM_NOCLAN);
                }
                else
                {
                    Comm.send_to_char("Clan channel is now OFF\n\r", ch);
                    Bit.SET_BIT(ref ch.comm, COMM_NOCLAN);
                }
                return;
            }

            if (Bit.IS_SET(ch.comm, COMM_NOCHANNELS))
            {
                Comm.send_to_char("The gods have revoked your channel priviliges.\n\r",
                    ch);
                return;
            }

            Bit.REMOVE_BIT(ref ch.comm, COMM_NOCLAN);

            Comm.send_to_char(RomString.sprintf("You clan '%s'{x\n\r", argument), ch);
            Gmcp.Channel(ch.desc, "clan", ch.name, argument);
            for (var d = Game.descriptor_list; d != null; d = d.next)
            {
                if (d.connected == CON_PLAYING &&
                    d.character != ch &&
                    Handler.is_same_clan(ch, d.character) &&
                    !Bit.IS_SET(d.character.comm, COMM_NOCLAN) &&
                    !Bit.IS_SET(d.character.comm, COMM_QUIET))
                {
                    Comm.act_new("$n clans '$t'{x", ch, argument, d.character, TO_VICT,
                        POS_DEAD, "clan", argument);
                }
            }
        }

        public static void do_pmote(CharData ch, string argument)
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

            if (!((argument[0] >= 'A' && argument[0] <= 'Z')
                  || (argument[0] >= 'a' && argument[0] <= 'z'))
                || Bit.isspace(argument[0]))
            {
                Comm.send_to_char("Moron!\n\r", ch);
                return;
            }

            Comm.act("$n $t", ch, argument, null, TO_CHAR);

            int matches = 0;
            if (ch.in_room == null) return;
            for (var vch = ch.in_room.people; vch != null; vch = vch.next_in_room)
            {
                if (vch.desc == null || vch == ch)
                    continue;

                int letterIdx = argument.IndexOf(vch.name ?? "", StringComparison.Ordinal);
                if (letterIdx < 0)
                {
                    Game.MOBtrigger = false;
                    Comm.act("$N $t", vch, argument, ch, TO_CHAR);
                    Game.MOBtrigger = true;
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
                    {
                        matches = 0;
                    }

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

                Game.MOBtrigger = false;
                Comm.act("$N $t", vch, temp, ch, TO_CHAR);
                Game.MOBtrigger = true;
            }
        }

        static readonly string[][] pose_table =
        {
            new[]
            {
                "You sizzle with energy.",
                "$n sizzles with energy.",
                "You feel very holy.",
                "$n looks very holy.",
                "You perform a small card trick.",
                "$n performs a small card trick.",
                "You show your bulging muscles.",
                "$n shows $s bulging muscles."
            },
            new[]
            {
                "You turn into a butterfly, then return to your normal shape.",
                "$n turns into a butterfly, then returns to $s normal shape.",
                "You nonchalantly turn wine into water.",
                "$n nonchalantly turns wine into water.",
                "You wiggle your ears alternately.",
                "$n wiggles $s ears alternately.",
                "You crack nuts between your fingers.",
                "$n cracks nuts between $s fingers."
            },
            new[]
            {
                "Blue sparks fly from your fingers.",
                "Blue sparks fly from $n's fingers.",
                "A halo appears over your head.",
                "A halo appears over $n's head.",
                "You nimbly tie yourself into a knot.",
                "$n nimbly ties $mself into a knot.",
                "You grizzle your teeth and look mean.",
                "$n grizzles $s teeth and looks mean."
            },
            new[]
            {
                "Little red lights dance in your eyes.",
                "Little red lights dance in $n's eyes.",
                "You recite words of wisdom.",
                "$n recites words of wisdom.",
                "You juggle with daggers, apples, and eyeballs.",
                "$n juggles with daggers, apples, and eyeballs.",
                "You hit your head, and your eyes roll.",
                "$n hits $s head, and $s eyes roll."
            },
            new[]
            {
                "A slimy green monster appears before you and bows.",
                "A slimy green monster appears before $n and bows.",
                "Deep in prayer, you levitate.",
                "Deep in prayer, $n levitates.",
                "You steal the underwear off every person in the room.",
                "Your underwear is gone!  $n stole it!",
                "Crunch, crunch -- you munch a bottle.",
                "Crunch, crunch -- $n munches a bottle."
            },
            new[]
            {
                "You turn everybody into a little pink elephant.",
                "You are turned into a little pink elephant by $n.",
                "An angel consults you.",
                "An angel consults $n.",
                "The dice roll ... and you win again.",
                "The dice roll ... and $n wins again.",
                "... 98, 99, 100 ... you do pushups.",
                "... 98, 99, 100 ... $n does pushups."
            },
            new[]
            {
                "A small ball of light dances on your fingertips.",
                "A small ball of light dances on $n's fingertips.",
                "Your body glows with an unearthly light.",
                "$n's body glows with an unearthly light.",
                "You count the money in everyone's pockets.",
                "Check your money, $n is counting it.",
                "Arnold Schwarzenegger admires your physique.",
                "Arnold Schwarzenegger admires $n's physique."
            },
            new[]
            {
                "Smoke and fumes leak from your nostrils.",
                "Smoke and fumes leak from $n's nostrils.",
                "A spot light hits you.",
                "A spot light hits $n.",
                "You balance a pocket knife on your tongue.",
                "$n balances a pocket knife on your tongue.",
                "Watch your feet, you are juggling granite boulders.",
                "Watch your feet, $n is juggling granite boulders."
            },
            new[]
            {
                "The light flickers as you rap in magical languages.",
                "The light flickers as $n raps in magical languages.",
                "Everyone levitates as you pray.",
                "You levitate as $n prays.",
                "You produce a coin from everyone's ear.",
                "$n produces a coin from your ear.",
                "Oomph!  You squeeze water out of a granite boulder.",
                "Oomph!  $n squeezes water out of a granite boulder."
            },
            new[]
            {
                "Your head disappears.",
                "$n's head disappears.",
                "A cool breeze refreshes you.",
                "A cool breeze refreshes $n.",
                "You step behind your shadow.",
                "$n steps behind $s shadow.",
                "You pick your teeth with a spear.",
                "$n picks $s teeth with a spear."
            },
            new[]
            {
                "A fire elemental singes your hair.",
                "A fire elemental singes $n's hair.",
                "The sun pierces through the clouds to illuminate you.",
                "The sun pierces through the clouds to illuminate $n.",
                "Your eyes dance with greed.",
                "$n's eyes dance with greed.",
                "Everyone is swept off their foot by your hug.",
                "You are swept off your feet by $n's hug."
            },
            new[]
            {
                "The sky changes color to match your eyes.",
                "The sky changes color to match $n's eyes.",
                "The ocean parts before you.",
                "The ocean parts before $n.",
                "You deftly steal everyone's weapon.",
                "$n deftly steals your weapon.",
                "Your karate chop splits a tree.",
                "$n's karate chop splits a tree."
            },
            new[]
            {
                "The stones dance to your command.",
                "The stones dance to $n's command.",
                "A thunder cloud kneels to you.",
                "A thunder cloud kneels to $n.",
                "The Grey Mouser buys you a beer.",
                "The Grey Mouser buys $n a beer.",
                "A strap of your armor breaks over your mighty thews.",
                "A strap of $n's armor breaks over $s mighty thews."
            },
            new[]
            {
                "The heavens and grass change colour as you smile.",
                "The heavens and grass change colour as $n smiles.",
                "The Burning Man speaks to you.",
                "The Burning Man speaks to $n.",
                "Everyone's pocket explodes with your fireworks.",
                "Your pocket explodes with $n's fireworks.",
                "A boulder cracks at your frown.",
                "A boulder cracks at $n's frown."
            },
            new[]
            {
                "Everyone's clothes are transparent, and you are laughing.",
                "Your clothes are transparent, and $n is laughing.",
                "An eye in a pyramid winks at you.",
                "An eye in a pyramid winks at $n.",
                "Everyone discovers your dagger a centimeter from their eye.",
                "You discover $n's dagger a centimeter from your eye.",
                "Mercenaries arrive to do your bidding.",
                "Mercenaries arrive to do $n's bidding."
            },
            new[]
            {
                "A black hole swallows you.",
                "A black hole swallows $n.",
                "Valentine Michael Smith offers you a glass of water.",
                "Valentine Michael Smith offers $n a glass of water.",
                "Where did you go?",
                "Where did $n go?",
                "Four matched Percherons bring in your chariot.",
                "Four matched Percherons bring in $n's chariot."
            },
            new[]
            {
                "The world shimmers in time with your whistling.",
                "The world shimmers in time with $n's whistling.",
                "The great god Mota gives you a staff.",
                "The great god Mota gives $n a staff.",
                "Click.",
                "Click.",
                "Atlas asks you to relieve him.",
                "Atlas asks $n to relieve him."
            }
        };

        public static void do_pose(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            int level = Bit.UMIN(ch.level, pose_table.Length - 1);
            int pose = RomRandom.number_range(0, level);

            Comm.act(pose_table[pose][2 * ch.klass + 0], ch, null, null, TO_CHAR);
            Comm.act(pose_table[pose][2 * ch.klass + 1], ch, null, null, TO_ROOM);
        }

        public static void do_bug(CharData ch, string argument)
        {
            Db.append_file(ch, BUG_FILE, argument);
            Comm.send_to_char("Bug logged.\n\r", ch);
        }

        public static void do_typo(CharData ch, string argument)
        {
            Db.append_file(ch, TYPO_FILE, argument);
            Comm.send_to_char("Typo logged.\n\r", ch);
        }

        public static void do_rent(CharData ch, string argument)
        {
            Comm.send_to_char("There is no rent here.  Just save and quit.\n\r", ch);
        }
    }
}
