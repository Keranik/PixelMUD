using static Rom24.Merc;

namespace Rom24
{
    public static class Nanny
    {
        public static void nanny(DescriptorData d, string argument)
        {
            /* Delete leading spaces UNLESS character is writing a note */
            if (d.connected != CON_NOTE_TEXT)
            {
                while (argument.Length > 0 && Bit.isspace(argument[0]))
                    argument = argument.Substring(1);
            }
            var ch = d.character;
            switch (d.connected)
            {
                default:
                    Db.bug("Nanny: bad d->connected %d.", d.connected);
                    Comm.close_socket(d);
                    return;

                case CON_ANSI:
                    if (argument.Length == 0 || Bit.UPPER(argument[0]) == 'Y')
                    {
                        d.ansi = true;
                        Comm.send_to_desc("{RAnsi enabled!{x\n\r", d);
                        d.connected = CON_GET_NAME;
                        {
                            var greet = Game.help_greeting ?? "";
                            if (greet.Length > 0 && greet[0] == '.')
                                Comm.send_to_desc(greet.Substring(1), d);
                            else
                                Comm.send_to_desc(greet, d);
                        }
                        break;
                    }

                    if (Bit.UPPER(argument[0]) == 'N')
                    {
                        d.ansi = false;
                        Comm.send_to_desc("Ansi disabled!\n\r", d);
                        d.connected = CON_GET_NAME;
                        {
                            var greet = Game.help_greeting ?? "";
                            if (greet.Length > 0 && greet[0] == '.')
                                Comm.send_to_desc(greet.Substring(1), d);
                            else
                                Comm.send_to_desc(greet, d);
                        }
                        break;
                    }
                    else
                    {
                        Comm.send_to_desc("Do you want ANSI? (Y/n) ", d);
                        return;
                    }

                case CON_GET_NAME:
                    if (argument.Length == 0) { Comm.close_socket(d); return; }
                    {
                        var chars = argument.ToCharArray();
                        chars[0] = Bit.UPPER(chars[0]);
                        argument = new string(chars);
                    }
                    if (!Comm.check_parse_name(argument))
                    {
                        Comm.send_to_desc("Illegal name, try another.\n\rName: ", d);
                        return;
                    }

                    bool fOld = Save.load_char_obj(d, argument);
                    ch = d.character;

                    if (Bit.IS_SET(ch.act, PLR_DENY))
                    {
                        Db.log_f("Denying access to %s@%s.", argument, d.host);
                        Comm.send_to_desc("You are denied access.\n\r", d);
                        Comm.close_socket(d);
                        return;
                    }

                    if (Ban.check_ban(d.host, BAN_PERMIT)
                        && !Bit.IS_SET(ch.act, PLR_PERMIT))
                    {
                        Comm.send_to_desc("Your site has been banned from this mud.\n\r",
                            d);
                        Comm.close_socket(d);
                        return;
                    }

                    if (Comm.check_reconnect(d, argument, false))
                    {
                        fOld = true;
                    }
                    else
                    {
                        if (Game.wizlock && !Bit.IS_IMMORTAL(ch))
                        {
                            Comm.send_to_desc("The game is wizlocked.\n\r", d);
                            Comm.close_socket(d);
                            return;
                        }
                    }

                    if (fOld)
                    {
                        /* Old player */
                        Comm.send_to_desc("Password: ", d);
                        Comm.write_to_buffer(d, Comm.echo_off_str, 0);
                        d.connected = CON_GET_OLD_PASSWORD;
                        return;
                    }
                    else
                    {
                        /* New player */
                        if (Game.newlock)
                        {
                            Comm.send_to_desc("The game is newlocked.\n\r", d);
                            Comm.close_socket(d);
                            return;
                        }

                        if (Ban.check_ban(d.host, BAN_NEWBIES))
                        {
                            Comm.send_to_desc
                                ("New players are not allowed from your site.\n\r",
                                d);
                            Comm.close_socket(d);
                            return;
                        }

                        Comm.send_to_desc(RomString.sprintf("Did I get that right, %s (Y/N)? ", argument), d);
                        d.connected = CON_CONFIRM_NEW_NAME;
                        return;
                    }

                case CON_GET_OLD_PASSWORD:
                    Comm.write_to_buffer(d, "\n\r");
                    if (UnixCrypt.crypt(argument, ch.pcdata.pwd) != ch.pcdata.pwd)
                    {
                        Comm.send_to_desc("Wrong password.\n\r", d);
                        Comm.close_socket(d);
                        return;
                    }

                    Comm.write_to_buffer(d, Comm.echo_on_str, 0);

                    if (Comm.check_playing(d, ch.name))
                        return;

                    if (Comm.check_reconnect(d, ch.name, true))
                        return;

                    Db.log_f("%s@%s has connected.", ch.name, d.host);
                    Game.log_buf = RomString.sprintf("%s@%s has connected.", ch.name, d.host);
                    Comm.wiznet(Game.log_buf, null, null, WIZ_SITES, 0, Handler.get_trust(ch));
                    if (d.ansi) Bit.SET_BIT(ref ch.act, PLR_COLOUR);
                    else Bit.REMOVE_BIT(ref ch.act, PLR_COLOUR);
                    if (Bit.IS_IMMORTAL(ch))
                    {
                        Interp.do_function(ch, Interp.do_help, "imotd");
                        d.connected = CON_READ_IMOTD;
                    }
                    else
                    {
                        Interp.do_function(ch, Interp.do_help, "motd");
                        d.connected = CON_READ_MOTD;
                    }
                    break;

                case CON_CONFIRM_NEW_NAME:
                    switch (argument.FirstOrDefault())
                    {
                        case 'y':
                        case 'Y':
                            Comm.send_to_desc(RomString.sprintf(
                                "New character.\n\rGive me a password for %s: %s",
                                ch.name, Comm.echo_off_str), d);
                            d.connected = CON_GET_NEW_PASSWORD;
                            if (ch.desc.ansi) Bit.SET_BIT(ref ch.act, PLR_COLOUR);
                            break;
                        case 'n':
                        case 'N':
                            Comm.send_to_desc("Ok, what IS it, then? ", d);
                            Save.free_char(d.character);
                            d.character = null;
                            d.connected = CON_GET_NAME;
                            break;
                        default:
                            Comm.send_to_desc("Please type Yes or No? ", d);
                            break;
                    }
                    break;

                case CON_GET_NEW_PASSWORD:
                    Comm.write_to_buffer(d, "\n\r");
                    if (argument.Length < 5)
                    {
                        Comm.send_to_desc("Password must be at least five characters long.\n\rPassword: ", d);
                        return;
                    }
                    var pwdnew = UnixCrypt.crypt(argument, ch.name);
                    if (pwdnew.Contains('~'))
                    {
                        Comm.send_to_desc("New password not acceptable, try again.\n\rPassword: ", d);
                        return;
                    }
                    ch.pcdata.pwd = pwdnew;
                    Comm.send_to_desc("Please retype password: ", d);
                    d.connected = CON_CONFIRM_NEW_PASSWORD;
                    break;

                case CON_CONFIRM_NEW_PASSWORD:
                    Comm.write_to_buffer(d, "\n\r");
                    if (UnixCrypt.crypt(argument, ch.pcdata.pwd) != ch.pcdata.pwd)
                    {
                        Comm.send_to_desc("Passwords don't match.\n\rRetype password: ", d);
                        d.connected = CON_GET_NEW_PASSWORD;
                        return;
                    }

                    Comm.write_to_buffer(d, Comm.echo_on_str, 0);
                    Comm.send_to_desc("The following races are available:\n\r  ", d);
                    for (int race = 1; Tables.race_table[race] != null && Tables.race_table[race].name != null; race++)
                    {
                        if (!Tables.race_table[race].pc_race) break;
                        Comm.write_to_buffer(d, Tables.race_table[race].name + " ");
                    }
                    Comm.write_to_buffer(d, "\n\r");
                    Comm.send_to_desc("What is your race (help for more information)? ", d);
                    d.connected = CON_GET_NEW_RACE;
                    break;

                case CON_GET_NEW_RACE:
                {
                    RomString.one_argument(argument, out string arg);
                    if (arg == "help")
                    {
                        argument = RomString.one_argument(argument, out _);
                        Interp.do_function(ch, Interp.do_help, argument.Length == 0 ? "race help" : argument);
                        Comm.send_to_desc("What is your race (help for more information)? ", d);
                        break;
                    }
                    int race = Lookup.race_lookup(argument);
                    if (race == 0 || !Tables.race_table[race].pc_race)
                    {
                        Comm.send_to_desc("That is not a valid race.\n\r", d);
                        Comm.send_to_desc("The following races are available:\n\r  ", d);
                        for (int r = 1; Tables.race_table[r] != null && Tables.race_table[r].name != null; r++)
                        {
                            if (!Tables.race_table[r].pc_race) break;
                            Comm.write_to_buffer(d, Tables.race_table[r].name + " ");
                        }
                        Comm.write_to_buffer(d, "\n\r");
                        Comm.send_to_desc("What is your race? (help for more information) ", d);
                        break;
                    }
                    ch.race = race;
                    for (int i = 0; i < MAX_STATS; i++)
                        ch.perm_stat[i] = Tables.pc_race_table[race].stats[i];
                    ch.affected_by = ch.affected_by | Tables.race_table[race].aff;
                    ch.imm_flags = ch.imm_flags | Tables.race_table[race].imm;
                    ch.res_flags = ch.res_flags | Tables.race_table[race].res;
                    ch.vuln_flags = ch.vuln_flags | Tables.race_table[race].vuln;
                    ch.form = Tables.race_table[race].form;
                    ch.parts = Tables.race_table[race].parts;

                    var skills = Tables.pc_race_table[race].skills;
                    for (int i = 0; i < 5; i++)
                    {
                        if (skills == null || i >= skills.Length || skills[i] == null)
                            break;
                        Skills.group_add(ch, skills[i], false);
                    }
                    ch.pcdata.points = Tables.pc_race_table[race].points;
                    ch.size = Tables.pc_race_table[race].size;
                    Comm.send_to_desc("What is your sex (M/F)? ", d);
                    d.connected = CON_GET_NEW_SEX;
                    break;
                }

                case CON_GET_NEW_SEX:
                    switch (argument.FirstOrDefault())
                    {
                        case 'm':
                        case 'M':
                            ch.sex = ch.pcdata.true_sex = SEX_MALE;
                            break;
                        case 'f':
                        case 'F':
                            ch.sex = ch.pcdata.true_sex = SEX_FEMALE;
                            break;
                        default:
                            Comm.send_to_desc("That's not a sex.\n\rWhat IS your sex? ", d);
                            return;
                    }
                    {
                        var buf = "Select a class [";
                        for (int iClass = 0; iClass < MAX_CLASS; iClass++)
                        {
                            if (iClass > 0) buf += " ";
                            buf += Tables.class_table[iClass].name;
                        }
                        buf += "]: ";
                        Comm.write_to_buffer(d, buf);
                    }
                    d.connected = CON_GET_NEW_CLASS;
                    break;

                case CON_GET_NEW_CLASS:
                {
                    int iClass = Lookup.class_lookup(argument);
                    if (iClass == -1)
                    {
                        Comm.send_to_desc("That's not a class.\n\rWhat IS your class? ", d);
                        return;
                    }
                    ch.klass = iClass;

                    Game.log_buf = RomString.sprintf("%s@%s new player.", ch.name, d.host);
                    Db.log_string(Game.log_buf);
                    Comm.wiznet("Newbie alert!  $N sighted.", ch, null, WIZ_NEWBIE, 0, 0);
                    Comm.wiznet(Game.log_buf, null, null, WIZ_SITES, 0, Handler.get_trust(ch));

                    Comm.write_to_buffer(d, "\n\r");
                    Comm.send_to_desc("You may be good, neutral, or evil.\n\r", d);
                    Comm.send_to_desc("Which alignment (G/N/E)? ", d);
                    d.connected = CON_GET_ALIGNMENT;
                    break;
                }

                case CON_GET_ALIGNMENT:
                    switch (argument.FirstOrDefault())
                    {
                        case 'g':
                        case 'G':
                            ch.alignment = 750;
                            break;
                        case 'n':
                        case 'N':
                            ch.alignment = 0;
                            break;
                        case 'e':
                        case 'E':
                            ch.alignment = -750;
                            break;
                        default:
                            Comm.send_to_desc("That's not a valid alignment.\n\r", d);
                            Comm.send_to_desc("Which alignment (G/N/E)? ", d);
                            return;
                    }
                    Comm.write_to_buffer(d, "\n\r");
                    Skills.group_add(ch, "rom basics", false);
                    Skills.group_add(ch, Tables.class_table[ch.klass].base_group, false);
                    ch.pcdata.learned[Gsn.recall] = 50;
                    Comm.send_to_desc("Do you wish to customize this character?\n\r", d);
                    Comm.send_to_desc("Customization takes time, but allows a wider range of skills and abilities.\n\r", d);
                    Comm.send_to_desc("Customize (Y/N)? ", d);
                    d.connected = CON_DEFAULT_CHOICE;
                    break;

                case CON_DEFAULT_CHOICE:
                    Comm.write_to_buffer(d, "\n\r");
                    switch (argument.FirstOrDefault())
                    {
                        case 'y':
                        case 'Y':
                            ch.gen_data = Recycle.new_gen_data();
                            ch.gen_data.points_chosen = ch.pcdata.points;
                            Interp.do_function(ch, Interp.do_help, "group header");
                            Skills.list_group_costs(ch);
                            Comm.write_to_buffer(d, "You already have the following skills:\n\r");
                            Interp.do_function(ch, Skills.do_skills, "");
                            Interp.do_function(ch, Interp.do_help, "menu choice");
                            d.connected = CON_GEN_GROUPS;
                            break;
                        case 'n':
                        case 'N':
                            Skills.group_add(ch, Tables.class_table[ch.klass].default_group, true);
                            Comm.write_to_buffer(d, "\n\r");
                            Comm.write_to_buffer(d, "Please pick a weapon from the following choices:\n\r");
                            Comm.write_to_buffer(d, weapon_choices(ch) + "\n\rYour choice? ");
                            d.connected = CON_PICK_WEAPON;
                            break;
                        default:
                            Comm.write_to_buffer(d, "Please answer (Y/N)? ");
                            return;
                    }
                    break;

                case CON_PICK_WEAPON:
                    Comm.write_to_buffer(d, "\n\r");
                    {
                        int weapon = Lookup.weapon_lookup(argument);
                        int gsn = -1;
                        if (weapon != -1 && Tables.weapon_table[weapon].gsnName != null)
                            gsn = Lookup.skill_lookup(Tables.weapon_table[weapon].gsnName);
                        if (weapon == -1 || gsn < 0 || ch.pcdata.learned[gsn] <= 0)
                        {
                            Comm.write_to_buffer(d, "That's not a valid selection. Choices are:\n\r");
                            Comm.write_to_buffer(d, weapon_choices(ch) + "\n\rYour choice? ");
                            return;
                        }
                        ch.pcdata.learned[gsn] = 40;
                    }
                    Comm.write_to_buffer(d, "\n\r");
                    Interp.do_function(ch, Interp.do_help, "motd");
                    d.connected = CON_READ_MOTD;
                    break;

                case CON_GEN_GROUPS:
                    Comm.send_to_char("\n\r", ch);
                    if (!RomString.str_cmp(argument, "done"))
                    {
                        if (ch.pcdata.points == Tables.pc_race_table[ch.race].points)
                        {
                            Comm.send_to_char("You didn't pick anything.\n\r", ch);
                            break;
                        }
                        if (ch.pcdata.points < 40 + Tables.pc_race_table[ch.race].points)
                        {
                            Comm.send_to_char(RomString.sprintf(
                                "You must take at least %d points of skills and groups",
                                40 + Tables.pc_race_table[ch.race].points), ch);
                            break;
                        }
                        Comm.send_to_char(RomString.sprintf("Creation points: %d\n\r", ch.pcdata.points), ch);
                        string buf = RomString.sprintf("Experience per level: %d\n\r",
                            Handler.exp_per_level(ch, ch.gen_data.points_chosen));
                        if (ch.pcdata.points < 40)
                            ch.train = (40 - ch.pcdata.points + 1) / 2;
                        Recycle.free_gen_data(ch.gen_data);
                        ch.gen_data = null;
                        Comm.send_to_char(buf, ch);
                        Comm.write_to_buffer(d, "\n\r");
                        Comm.write_to_buffer(d, "Please pick a weapon from the following choices:\n\r");
                        Comm.write_to_buffer(d, weapon_choices(ch) + "\n\rYour choice? ");
                        d.connected = CON_PICK_WEAPON;
                        break;
                    }

                    if (!Skills.parse_gen_groups(ch, argument))
                        Comm.send_to_char(
                            "Choices are: list,learned,premise,add,drop,info,help, and done.\n\r",
                            ch);

                    Interp.do_function(ch, Interp.do_help, "menu choice");
                    break;

                case CON_BREAK_CONNECT:
                    switch (argument.FirstOrDefault())
                    {
                        case 'y':
                        case 'Y':
                            DescriptorData d_next;
                            for (var d_old = Game.descriptor_list; d_old != null;
                                 d_old = d_next)
                            {
                                d_next = d_old.next;
                                if (d_old == d || d_old.character == null)
                                    continue;

                                if (RomString.str_cmp(ch.name, d_old.original != null
                                    ? d_old.original.name : d_old.character.name))
                                    continue;

                                Comm.close_socket(d_old);
                            }
                            if (Comm.check_reconnect(d, ch.name, true))
                                return;
                            Comm.send_to_desc("Reconnect attempt failed.\n\rName: ", d);
                            if (d.character != null)
                            {
                                Save.free_char(d.character);
                                d.character = null;
                            }
                            d.connected = CON_GET_NAME;
                            break;

                        case 'n':
                        case 'N':
                            Comm.send_to_desc("Name: ", d);
                            if (d.character != null)
                            {
                                Save.free_char(d.character);
                                d.character = null;
                            }
                            d.connected = CON_GET_NAME;
                            break;

                        default:
                            Comm.send_to_desc("Please type Y or N? ", d);
                            break;
                    }
                    break;

                case CON_READ_IMOTD:
                    Comm.write_to_buffer(d, "\n\r");
                    Interp.do_function(ch, Interp.do_help, "motd");
                    d.connected = CON_READ_MOTD;
                    break;

                case CON_READ_MOTD:
                    if (ch.pcdata == null || string.IsNullOrEmpty(ch.pcdata.pwd))
                    {
                        Comm.write_to_buffer(d, "Warning! Null password!\n\r");
                        Comm.write_to_buffer(d, "Please report old password with bug.\n\r");
                        Comm.write_to_buffer(d, "Type 'password null <new password>' to fix.\n\r");
                    }
                    Comm.write_to_buffer(d, "\n\rWelcome to ROM 2.4.  Please don't feed the mobiles!\n\r");
                    ch.next = Game.char_list;
                    Game.char_list = ch;
                    d.connected = CON_PLAYING;
                    Handler.reset_char(ch);

                    if (ch.level == 0)
                    {
                        if (Game.mud_ansicolor != 0) Bit.SET_BIT(ref ch.act, PLR_COLOUR);
                        if (Game.mud_telnetga != 0) Bit.SET_BIT(ref ch.comm, COMM_TELNET_GA);
                        ch.perm_stat[Tables.class_table[ch.klass].attr_prime] += 3;
                        ch.level = 1;
                        ch.exp = Handler.exp_per_level(ch, ch.pcdata.points);
                        ch.hit = ch.max_hit;
                        ch.mana = ch.max_mana;
                        ch.move = ch.max_move;
                        ch.train = 3;
                        ch.practice = 5;
                        string buf = RomString.sprintf("the %s", Tables.title_table[ch.klass][ch.level]
                            [ch.sex == SEX_FEMALE ? 1 : 0]);
                        ActInfo.set_title(ch, buf);

                        Interp.do_function(ch, ActWiz.do_outfit, "");
                        Handler.obj_to_char(Db.create_object(Handler.get_obj_index(OBJ_VNUM_MAP), 0),
                            ch);

                        Handler.char_to_room(ch, Handler.get_room_index(ROOM_VNUM_SCHOOL));
                        Comm.send_to_char("\n\r", ch);
                        Interp.do_function(ch, Interp.do_help, "newbie info");
                        Comm.send_to_char("\n\r", ch);
                    }
                    else if (ch.in_room != null)
                        Handler.char_to_room(ch, ch.in_room);
                    else if (Bit.IS_IMMORTAL(ch))
                        Handler.char_to_room(ch, Handler.get_room_index(ROOM_VNUM_CHAT));
                    else
                        Handler.char_to_room(ch, Handler.get_room_index(ROOM_VNUM_TEMPLE));
                    Comm.act("$n has entered the game.", ch, null, null, TO_ROOM);
                    Interp.do_function(ch, Interp.do_look, "auto");

                    Comm.wiznet("$N has left real life behind.", ch, null,
                        WIZ_LOGINS, WIZ_SITES, Handler.get_trust(ch));

                    if (ch.pet != null)
                    {
                        Handler.char_to_room(ch.pet, ch.in_room);
                        Comm.act("$n has entered the game.", ch.pet, null, null,
                            TO_ROOM);
                    }

                    Comm.send_to_char("\n", ch);
                    Interp.do_function(ch, Board.do_board, "");
                    break;

                case CON_NOTE_TO:
                    Board.handle_con_note_to(d, argument);
                    break;
                case CON_NOTE_SUBJECT:
                    Board.handle_con_note_subject(d, argument);
                    break;
                case CON_NOTE_EXPIRE:
                    Board.handle_con_note_expire(d, argument);
                    break;
                case CON_NOTE_TEXT:
                    Board.handle_con_note_text(d, argument);
                    break;
                case CON_NOTE_FINISH:
                    Board.handle_con_note_finish(d, argument);
                    break;
            }
        }

        static string weapon_choices(CharData ch)
        {
            var buf = "";
            for (int i = 0; Tables.weapon_table[i].name != null; i++)
            {
                var gsnName = Tables.weapon_table[i].gsnName;
                int gsn = gsnName == null ? -1 : Lookup.skill_lookup(gsnName);
                if (gsn >= 0 && ch.pcdata.learned[gsn] > 0)
                    buf += Tables.weapon_table[i].name + " ";
            }
            return buf;
        }
    }
}
