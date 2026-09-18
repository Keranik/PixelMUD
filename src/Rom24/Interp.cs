using static Rom24.Merc;

namespace Rom24
{
    public static class Interp
    {
        public const int LOG_NORMAL = 0;
        public const int LOG_ALWAYS = 1;
        public const int LOG_NEVER = 2;

        public static readonly CmdType[] cmd_table =
        {
            new("north", do_north, POS_STANDING, 0, LOG_NEVER, 0),
            new("east", do_east, POS_STANDING, 0, LOG_NEVER, 0),
            new("south", do_south, POS_STANDING, 0, LOG_NEVER, 0),
            new("west", do_west, POS_STANDING, 0, LOG_NEVER, 0),
            new("up", do_up, POS_STANDING, 0, LOG_NEVER, 0),
            new("down", do_down, POS_STANDING, 0, LOG_NEVER, 0),

            new("at", ActWiz.do_at, POS_DEAD, MAX_LEVEL - 6, LOG_NORMAL, 1),
            new("cast", Magic.do_cast, POS_FIGHTING, 0, LOG_NORMAL, 1),
            new("auction", ActComm.do_auction, POS_SLEEPING, 0, LOG_NORMAL, 1),
            new("buy", ActObj.do_buy, POS_RESTING, 0, LOG_NORMAL, 1),
            new("channels", ActComm.do_channels, POS_DEAD, 0, LOG_NORMAL, 1),
            new("exits", do_exits, POS_RESTING, 0, LOG_NORMAL, 1),
            new("get", ActObj.do_get, POS_RESTING, 0, LOG_NORMAL, 1),
            new("goto", ActWiz.do_goto, POS_DEAD, MAX_LEVEL - 8, LOG_NORMAL, 1),
            new("group", ActComm.do_group, POS_SLEEPING, 0, LOG_NORMAL, 1),
            new("guild", ActWiz.do_guild, POS_DEAD, MAX_LEVEL - 4, LOG_ALWAYS, 1),
            new("hit", Fight.do_kill, POS_FIGHTING, 0, LOG_NORMAL, 0),
            new("inventory", do_inventory, POS_DEAD, 0, LOG_NORMAL, 1),
            new("kill", Fight.do_kill, POS_FIGHTING, 0, LOG_NORMAL, 1),
            new("look", do_look, POS_RESTING, 0, LOG_NORMAL, 1),
            new("clan", ActComm.do_clantalk, POS_SLEEPING, 0, LOG_NORMAL, 1),
            new("music", ActComm.do_music, POS_SLEEPING, 0, LOG_NORMAL, 1),
            new("order", ActComm.do_order, POS_RESTING, 0, LOG_NORMAL, 1),
            new("practice", ActInfo.do_practice, POS_SLEEPING, 0, LOG_NORMAL, 1),
            new("rest", ActMove.do_rest, POS_SLEEPING, 0, LOG_NORMAL, 1),
            new("scan", Scan.do_scan, POS_SLEEPING, 0, LOG_NORMAL, 1),
            new("sit", ActMove.do_sit, POS_SLEEPING, 0, LOG_NORMAL, 1),
            new("sockets", ActWiz.do_sockets, POS_DEAD, MAX_LEVEL - 4, LOG_NORMAL, 1),
            new("stand", ActMove.do_stand, POS_SLEEPING, 0, LOG_NORMAL, 1),
            new("tell", ActComm.do_tell, POS_RESTING, 0, LOG_NORMAL, 1),
            new("unlock", ActMove.do_unlock, POS_RESTING, 0, LOG_NORMAL, 1),
            new("wield", ActObj.do_wear, POS_RESTING, 0, LOG_NORMAL, 1),
            new("wizhelp", do_wizhelp, POS_DEAD, LEVEL_IMMORTAL, LOG_NORMAL, 1),

            new("affects", ActInfo.do_affects, POS_DEAD, 0, LOG_NORMAL, 1),
            new("areas", do_areas, POS_DEAD, 0, LOG_NORMAL, 1),
            new("board", Board.do_board, POS_SLEEPING, 0, LOG_NORMAL, 1),
            new("commands", do_commands, POS_DEAD, 0, LOG_NORMAL, 1),
            new("compare", ActInfo.do_compare, POS_RESTING, 0, LOG_NORMAL, 1),
            new("consider", ActInfo.do_consider, POS_RESTING, 0, LOG_NORMAL, 1),
            new("count", ActInfo.do_count, POS_SLEEPING, 0, LOG_NORMAL, 1),
            new("credits", do_credits, POS_DEAD, 0, LOG_NORMAL, 1),
            new("equipment", do_equipment, POS_DEAD, 0, LOG_NORMAL, 1),
            new("examine", ActInfo.do_examine, POS_RESTING, 0, LOG_NORMAL, 1),
            new("help", do_help, POS_DEAD, 0, LOG_NORMAL, 1),
            new("info", Skills.do_groups, POS_SLEEPING, 0, LOG_NORMAL, 1),
            new("motd", ActInfo.do_motd, POS_DEAD, 0, LOG_NORMAL, 1),
            new("read", ActInfo.do_read, POS_RESTING, 0, LOG_NORMAL, 1),
            new("report", ActInfo.do_report, POS_RESTING, 0, LOG_NORMAL, 1),
            new("rules", ActInfo.do_rules, POS_DEAD, 0, LOG_NORMAL, 1),
            new("score", do_score, POS_DEAD, 0, LOG_NORMAL, 1),
            new("skills", Skills.do_skills, POS_DEAD, 0, LOG_NORMAL, 1),
            new("socials", ActInfo.do_socials, POS_DEAD, 0, LOG_NORMAL, 1),
            new("show", ActInfo.do_show, POS_DEAD, 0, LOG_NORMAL, 1),
            new("spells", Skills.do_spells, POS_DEAD, 0, LOG_NORMAL, 1),
            new("story", ActInfo.do_story, POS_DEAD, 0, LOG_NORMAL, 1),
            new("time", do_time, POS_DEAD, 0, LOG_NORMAL, 1),
            new("typo", ActComm.do_typo, POS_DEAD, 0, LOG_NORMAL, 1),
            new("weather", do_weather, POS_RESTING, 0, LOG_NORMAL, 1),
            new("who", do_who, POS_DEAD, 0, LOG_NORMAL, 1),
            new("whois", ActInfo.do_whois, POS_DEAD, 0, LOG_NORMAL, 1),
            new("wizlist", ActInfo.do_wizlist, POS_DEAD, 0, LOG_NORMAL, 1),
            new("worth", ActInfo.do_worth, POS_SLEEPING, 0, LOG_NORMAL, 1),

            new("alia", Alias.do_alia, POS_DEAD, 0, LOG_NORMAL, 0),
            new("alias", Alias.do_alias, POS_DEAD, 0, LOG_NORMAL, 1),
            new("autolist", ActInfo.do_autolist, POS_DEAD, 0, LOG_NORMAL, 1),
            new("autoall", ActInfo.do_autoall, POS_DEAD, 0, LOG_NORMAL, 1),
            new("autoassist", ActInfo.do_autoassist, POS_DEAD, 0, LOG_NORMAL, 1),
            new("autoexit", ActInfo.do_autoexit, POS_DEAD, 0, LOG_NORMAL, 1),
            new("autogold", ActInfo.do_autogold, POS_DEAD, 0, LOG_NORMAL, 1),
            new("autoloot", ActInfo.do_autoloot, POS_DEAD, 0, LOG_NORMAL, 1),
            new("autosac", ActInfo.do_autosac, POS_DEAD, 0, LOG_NORMAL, 1),
            new("autosplit", ActInfo.do_autosplit, POS_DEAD, 0, LOG_NORMAL, 1),
            new("brief", ActInfo.do_brief, POS_DEAD, 0, LOG_NORMAL, 1),
            new("colour", ActComm.do_colour, POS_DEAD, 0, LOG_NORMAL, 1),
            new("color", ActComm.do_colour, POS_DEAD, 0, LOG_NORMAL, 1),
            new("combine", ActInfo.do_combine, POS_DEAD, 0, LOG_NORMAL, 1),
            new("compact", ActInfo.do_compact, POS_DEAD, 0, LOG_NORMAL, 1),
            new("description", ActInfo.do_description, POS_DEAD, 0, LOG_NORMAL, 1),
            new("delet", ActComm.do_delet, POS_DEAD, 0, LOG_ALWAYS, 0),
            new("delete", ActComm.do_delete, POS_STANDING, 0, LOG_ALWAYS, 1),
            new("nofollow", ActInfo.do_nofollow, POS_DEAD, 0, LOG_NORMAL, 1),
            new("noloot", ActInfo.do_noloot, POS_DEAD, 0, LOG_NORMAL, 1),
            new("nosummon", ActInfo.do_nosummon, POS_DEAD, 0, LOG_NORMAL, 1),
            new("outfit", ActWiz.do_outfit, POS_RESTING, 0, LOG_NORMAL, 1),
            new("password", ActInfo.do_password, POS_DEAD, 0, LOG_NEVER, 1),
            new("prompt", ActInfo.do_prompt, POS_DEAD, 0, LOG_NORMAL, 1),
            new("scroll", ActInfo.do_scroll, POS_DEAD, 0, LOG_NORMAL, 1),
            new("telnetga", ActInfo.do_telnetga, POS_DEAD, 0, LOG_NORMAL, 1),
            new("title", ActInfo.do_title, POS_DEAD, 0, LOG_NORMAL, 1),
            new("unalias", Alias.do_unalias, POS_DEAD, 0, LOG_NORMAL, 1),
            new("wimpy", ActInfo.do_wimpy, POS_DEAD, 0, LOG_NORMAL, 1),

            new("afk", ActComm.do_afk, POS_SLEEPING, 0, LOG_NORMAL, 1),
            new("answer", ActComm.do_answer, POS_SLEEPING, 0, LOG_NORMAL, 1),
            new("deaf", ActComm.do_deaf, POS_DEAD, 0, LOG_NORMAL, 1),
            new("emote", ActComm.do_emote, POS_RESTING, 0, LOG_NORMAL, 1),
            new("pmote", ActComm.do_pmote, POS_RESTING, 0, LOG_NORMAL, 1),
            new(".", ActComm.do_gossip, POS_SLEEPING, 0, LOG_NORMAL, 0),
            new("gossip", ActComm.do_gossip, POS_SLEEPING, 0, LOG_NORMAL, 1),
            new(",", ActComm.do_emote, POS_RESTING, 0, LOG_NORMAL, 0),
            new("grats", ActComm.do_grats, POS_SLEEPING, 0, LOG_NORMAL, 1),
            new("gtell", ActComm.do_gtell, POS_DEAD, 0, LOG_NORMAL, 1),
            new(";", ActComm.do_gtell, POS_DEAD, 0, LOG_NORMAL, 0),
            new("note", Board.do_note, POS_DEAD, 0, LOG_NORMAL, 1),
            new("pose", ActComm.do_pose, POS_RESTING, 0, LOG_NORMAL, 1),
            new("question", ActComm.do_question, POS_SLEEPING, 0, LOG_NORMAL, 1),
            new("quote", ActComm.do_quote, POS_SLEEPING, 0, LOG_NORMAL, 1),
            new("quiet", ActComm.do_quiet, POS_SLEEPING, 0, LOG_NORMAL, 1),
            new("reply", ActComm.do_reply, POS_SLEEPING, 0, LOG_NORMAL, 1),
            new("replay", ActComm.do_replay, POS_SLEEPING, 0, LOG_NORMAL, 1),
            new("say", ActComm.do_say, POS_RESTING, 0, LOG_NORMAL, 1),
            new("'", ActComm.do_say, POS_RESTING, 0, LOG_NORMAL, 0),
            new("shout", ActComm.do_shout, POS_RESTING, 3, LOG_NORMAL, 1),
            new("yell", ActComm.do_yell, POS_RESTING, 0, LOG_NORMAL, 1),

            new("brandish", ActObj.do_brandish, POS_RESTING, 0, LOG_NORMAL, 1),
            new("close", ActMove.do_close, POS_RESTING, 0, LOG_NORMAL, 1),
            new("drink", ActObj.do_drink, POS_RESTING, 0, LOG_NORMAL, 1),
            new("drop", ActObj.do_drop, POS_RESTING, 0, LOG_NORMAL, 1),
            new("eat", ActObj.do_eat, POS_RESTING, 0, LOG_NORMAL, 1),
            new("envenom", ActObj.do_envenom, POS_RESTING, 0, LOG_NORMAL, 1),
            new("fill", ActObj.do_fill, POS_RESTING, 0, LOG_NORMAL, 1),
            new("give", ActObj.do_give, POS_RESTING, 0, LOG_NORMAL, 1),
            new("heal", Healer.do_heal, POS_RESTING, 0, LOG_NORMAL, 1),
            new("hold", ActObj.do_wear, POS_RESTING, 0, LOG_NORMAL, 1),
            new("list", ActObj.do_list, POS_RESTING, 0, LOG_NORMAL, 1),
            new("lock", ActMove.do_lock, POS_RESTING, 0, LOG_NORMAL, 1),
            new("open", ActMove.do_open, POS_RESTING, 0, LOG_NORMAL, 1),
            new("pick", ActMove.do_pick, POS_RESTING, 0, LOG_NORMAL, 1),
            new("pour", ActObj.do_pour, POS_RESTING, 0, LOG_NORMAL, 1),
            new("put", ActObj.do_put, POS_RESTING, 0, LOG_NORMAL, 1),
            new("quaff", ActObj.do_quaff, POS_RESTING, 0, LOG_NORMAL, 1),
            new("recite", ActObj.do_recite, POS_RESTING, 0, LOG_NORMAL, 1),
            new("remove", ActObj.do_remove, POS_RESTING, 0, LOG_NORMAL, 1),
            new("sell", ActObj.do_sell, POS_RESTING, 0, LOG_NORMAL, 1),
            new("take", ActObj.do_get, POS_RESTING, 0, LOG_NORMAL, 1),
            new("sacrifice", ActObj.do_sacrifice, POS_RESTING, 0, LOG_NORMAL, 1),
            new("junk", ActObj.do_sacrifice, POS_RESTING, 0, LOG_NORMAL, 0),
            new("tap", ActObj.do_sacrifice, POS_RESTING, 0, LOG_NORMAL, 0),
            new("value", ActObj.do_value, POS_RESTING, 0, LOG_NORMAL, 1),
            new("wear", ActObj.do_wear, POS_RESTING, 0, LOG_NORMAL, 1),
            new("zap", ActObj.do_zap, POS_RESTING, 0, LOG_NORMAL, 1),

            new("backstab", Fight.do_backstab, POS_FIGHTING, 0, LOG_NORMAL, 1),
            new("bash", Fight.do_bash, POS_FIGHTING, 0, LOG_NORMAL, 1),
            new("bs", Fight.do_backstab, POS_FIGHTING, 0, LOG_NORMAL, 0),
            new("berserk", Fight.do_berserk, POS_FIGHTING, 0, LOG_NORMAL, 1),
            new("dirt", Fight.do_dirt, POS_FIGHTING, 0, LOG_NORMAL, 1),
            new("disarm", Fight.do_disarm, POS_FIGHTING, 0, LOG_NORMAL, 1),
            new("flee", Fight.do_flee, POS_FIGHTING, 0, LOG_NORMAL, 1),
            new("kick", Fight.do_kick, POS_FIGHTING, 0, LOG_NORMAL, 1),
            new("murde", Fight.do_murde, POS_FIGHTING, 0, LOG_NORMAL, 0),
            new("murder", Fight.do_murder, POS_FIGHTING, 5, LOG_ALWAYS, 1),
            new("rescue", Fight.do_rescue, POS_FIGHTING, 0, LOG_NORMAL, 0),
            new("surrender", Fight.do_surrender, POS_FIGHTING, 0, LOG_NORMAL, 1),
            new("trip", Fight.do_trip, POS_FIGHTING, 0, LOG_NORMAL, 1),

            new("mob", MobCmds.do_mob, POS_DEAD, 0, LOG_NEVER, 0),

            new("enter", ActMove.do_enter, POS_STANDING, 0, LOG_NORMAL, 1),
            new("follow", ActComm.do_follow, POS_RESTING, 0, LOG_NORMAL, 1),
            new("gain", Skills.do_gain, POS_STANDING, 0, LOG_NORMAL, 1),
            new("go", ActMove.do_enter, POS_STANDING, 0, LOG_NORMAL, 0),
            new("groups", Skills.do_groups, POS_SLEEPING, 0, LOG_NORMAL, 1),
            new("hide", ActMove.do_hide, POS_RESTING, 0, LOG_NORMAL, 1),
            new("play", Music.do_play, POS_RESTING, 0, LOG_NORMAL, 1),
            new("qui", do_qui, POS_DEAD, 0, LOG_NORMAL, 0),
            new("quit", do_quit, POS_DEAD, 0, LOG_NORMAL, 1),
            new("recall", ActMove.do_recall, POS_FIGHTING, 0, LOG_NORMAL, 1),
            new("/", ActMove.do_recall, POS_FIGHTING, 0, LOG_NORMAL, 0),
            new("rent", ActComm.do_rent, POS_DEAD, 0, LOG_NORMAL, 0),
            new("save", do_save, POS_DEAD, 0, LOG_NORMAL, 1),
            new("sleep", ActMove.do_sleep, POS_SLEEPING, 0, LOG_NORMAL, 1),
            new("sneak", ActMove.do_sneak, POS_STANDING, 0, LOG_NORMAL, 1),
            new("split", ActComm.do_split, POS_RESTING, 0, LOG_NORMAL, 1),
            new("steal", ActObj.do_steal, POS_STANDING, 0, LOG_NORMAL, 1),
            new("train", ActMove.do_train, POS_RESTING, 0, LOG_NORMAL, 1),
            new("visible", ActMove.do_visible, POS_SLEEPING, 0, LOG_NORMAL, 1),
            new("wake", ActMove.do_wake, POS_SLEEPING, 0, LOG_NORMAL, 1),
            new("where", ActInfo.do_where, POS_RESTING, 0, LOG_NORMAL, 1),

            new("advance", ActWiz.do_advance, POS_DEAD, MAX_LEVEL, LOG_ALWAYS, 1),
            new("copyover", ActWiz.do_copyover, POS_DEAD, MAX_LEVEL, LOG_ALWAYS, 1),
            new("dump", Db.do_dump, POS_DEAD, MAX_LEVEL, LOG_ALWAYS, 0),
            new("trust", ActWiz.do_trust, POS_DEAD, MAX_LEVEL, LOG_ALWAYS, 1),
            new("violate", ActWiz.do_violate, POS_DEAD, MAX_LEVEL, LOG_ALWAYS, 1),

            new("allow", Ban.do_allow, POS_DEAD, MAX_LEVEL - 2, LOG_ALWAYS, 1),
            new("ban", Ban.do_ban, POS_DEAD, MAX_LEVEL - 2, LOG_ALWAYS, 1),
            new("deny", ActWiz.do_deny, POS_DEAD, MAX_LEVEL - 1, LOG_ALWAYS, 1),
            new("disconnect", ActWiz.do_disconnect, POS_DEAD, MAX_LEVEL - 3, LOG_ALWAYS, 1),
            new("flag", Flags.do_flag, POS_DEAD, MAX_LEVEL - 4, LOG_ALWAYS, 1),
            new("freeze", ActWiz.do_freeze, POS_DEAD, MAX_LEVEL - 4, LOG_ALWAYS, 1),
            new("permban", Ban.do_permban, POS_DEAD, MAX_LEVEL - 1, LOG_ALWAYS, 1),
            new("protect", ActWiz.do_protect, POS_DEAD, MAX_LEVEL - 1, LOG_ALWAYS, 1),
            new("reboo", ActWiz.do_reboo, POS_DEAD, MAX_LEVEL - 1, LOG_NORMAL, 0),
            new("reboot", ActWiz.do_reboot, POS_DEAD, MAX_LEVEL - 1, LOG_ALWAYS, 1),
            new("set", ActWiz.do_set, POS_DEAD, MAX_LEVEL - 2, LOG_ALWAYS, 1),
            new("shutdow", ActWiz.do_shutdow, POS_DEAD, MAX_LEVEL - 1, LOG_NORMAL, 0),
            new("shutdown", ActWiz.do_shutdown, POS_DEAD, MAX_LEVEL - 1, LOG_ALWAYS, 1),
            new("wizlock", ActWiz.do_wizlock, POS_DEAD, MAX_LEVEL - 2, LOG_ALWAYS, 1),

            new("force", ActWiz.do_force, POS_DEAD, MAX_LEVEL - 7, LOG_ALWAYS, 1),
            new("load", ActWiz.do_load, POS_DEAD, MAX_LEVEL - 4, LOG_ALWAYS, 1),
            new("newlock", ActWiz.do_newlock, POS_DEAD, MAX_LEVEL - 4, LOG_ALWAYS, 1),
            new("nochannels", ActWiz.do_nochannels, POS_DEAD, MAX_LEVEL - 5, LOG_ALWAYS, 1),
            new("noemote", ActWiz.do_noemote, POS_DEAD, MAX_LEVEL - 5, LOG_ALWAYS, 1),
            new("noshout", ActWiz.do_noshout, POS_DEAD, MAX_LEVEL - 5, LOG_ALWAYS, 1),
            new("notell", ActWiz.do_notell, POS_DEAD, MAX_LEVEL - 5, LOG_ALWAYS, 1),
            new("pecho", ActWiz.do_pecho, POS_DEAD, MAX_LEVEL - 4, LOG_ALWAYS, 1),
            new("pardon", ActWiz.do_pardon, POS_DEAD, MAX_LEVEL - 3, LOG_ALWAYS, 1),
            new("purge", ActWiz.do_purge, POS_DEAD, MAX_LEVEL - 4, LOG_ALWAYS, 1),
            new("qmconfig", ActWiz.do_qmconfig, POS_DEAD, MAX_LEVEL, LOG_ALWAYS, 1),
            new("restore", ActWiz.do_restore, POS_DEAD, MAX_LEVEL - 4, LOG_ALWAYS, 1),
            new("sla", Fight.do_sla, POS_DEAD, MAX_LEVEL - 3, LOG_NORMAL, 0),
            new("slay", Fight.do_slay, POS_DEAD, MAX_LEVEL - 3, LOG_ALWAYS, 1),
            new("teleport", ActWiz.do_transfer, POS_DEAD, MAX_LEVEL - 5, LOG_ALWAYS, 1),
            new("transfer", ActWiz.do_transfer, POS_DEAD, MAX_LEVEL - 5, LOG_ALWAYS, 1),

            new("poofin", ActWiz.do_bamfin, POS_DEAD, MAX_LEVEL - 8, LOG_NORMAL, 1),
            new("poofout", ActWiz.do_bamfout, POS_DEAD, MAX_LEVEL - 8, LOG_NORMAL, 1),
            new("gecho", ActWiz.do_echo, POS_DEAD, MAX_LEVEL - 4, LOG_ALWAYS, 1),
            new("holylight", ActWiz.do_holylight, POS_DEAD, LEVEL_IMMORTAL, LOG_NORMAL, 1),
            new("incognito", ActWiz.do_incognito, POS_DEAD, LEVEL_IMMORTAL, LOG_NORMAL, 1),
            new("invis", ActWiz.do_invis, POS_DEAD, LEVEL_IMMORTAL, LOG_NORMAL, 0),
            new("log", ActWiz.do_log, POS_DEAD, MAX_LEVEL - 1, LOG_ALWAYS, 1),
            new("memory", Db.do_memory, POS_DEAD, LEVEL_IMMORTAL, LOG_NORMAL, 1),
            new("mwhere", ActWiz.do_mwhere, POS_DEAD, LEVEL_IMMORTAL, LOG_NORMAL, 1),
            new("owhere", ActWiz.do_owhere, POS_DEAD, LEVEL_IMMORTAL, LOG_NORMAL, 1),
            new("peace", ActWiz.do_peace, POS_DEAD, MAX_LEVEL - 5, LOG_NORMAL, 1),
            new("echo", ActWiz.do_recho, POS_DEAD, MAX_LEVEL - 6, LOG_ALWAYS, 1),
            new("return", ActWiz.do_return, POS_DEAD, MAX_LEVEL - 6, LOG_NORMAL, 1),
            new("snoop", ActWiz.do_snoop, POS_DEAD, MAX_LEVEL - 5, LOG_ALWAYS, 1),
            new("stat", ActWiz.do_stat, POS_DEAD, LEVEL_IMMORTAL, LOG_NORMAL, 1),
            new("string", ActWiz.do_string, POS_DEAD, MAX_LEVEL - 5, LOG_ALWAYS, 1),
            new("switch", ActWiz.do_switch, POS_DEAD, MAX_LEVEL - 6, LOG_ALWAYS, 1),
            new("wizinvis", ActWiz.do_invis, POS_DEAD, LEVEL_IMMORTAL, LOG_NORMAL, 1),
            new("vnum", ActWiz.do_vnum, POS_DEAD, MAX_LEVEL - 4, LOG_NORMAL, 1),
            new("zecho", ActWiz.do_zecho, POS_DEAD, MAX_LEVEL - 4, LOG_ALWAYS, 1),

            new("clone", ActWiz.do_clone, POS_DEAD, MAX_LEVEL - 5, LOG_ALWAYS, 1),

            new("wiznet", ActWiz.do_wiznet, POS_DEAD, LEVEL_IMMORTAL, LOG_NORMAL, 1),
            new("immtalk", ActComm.do_immtalk, POS_DEAD, LEVEL_IMMORTAL, LOG_NORMAL, 1),
            new("imotd", ActInfo.do_imotd, POS_DEAD, LEVEL_IMMORTAL, LOG_NORMAL, 1),
            new(":", ActComm.do_immtalk, POS_DEAD, LEVEL_IMMORTAL, LOG_NORMAL, 0),
            new("smote", ActWiz.do_smote, POS_DEAD, LEVEL_IMMORTAL, LOG_NORMAL, 1),
            new("prefi", ActWiz.do_prefi, POS_DEAD, LEVEL_IMMORTAL, LOG_NORMAL, 0),
            new("prefix", ActWiz.do_prefix, POS_DEAD, LEVEL_IMMORTAL, LOG_NORMAL, 1),
            new("mpdump", MobCmds.do_mpdump, POS_DEAD, LEVEL_IMMORTAL, LOG_NEVER, 1),
            new("mpstat", MobCmds.do_mpstat, POS_DEAD, LEVEL_IMMORTAL, LOG_NEVER, 1),

            new("edit", Olc.do_olc, POS_DEAD, 0, LOG_NORMAL, 1),
            new("asave", Olc.do_asave, POS_DEAD, 0, LOG_NORMAL, 1),
            new("alist", Olc.do_alist, POS_DEAD, 0, LOG_NORMAL, 1),
            new("resets", Olc.do_resets, POS_DEAD, 0, LOG_NORMAL, 1),
            new("redit", Olc.do_redit, POS_DEAD, 0, LOG_NORMAL, 1),
            new("medit", Olc.do_medit, POS_DEAD, 0, LOG_NORMAL, 1),
            new("aedit", Olc.do_aedit, POS_DEAD, 0, LOG_NORMAL, 1),
            new("oedit", Olc.do_oedit, POS_DEAD, 0, LOG_NORMAL, 1),
            new("mpedit", Olc.do_mpedit, POS_DEAD, 0, LOG_NORMAL, 1),
            new("hedit", Olc.do_hedit, POS_DEAD, 0, LOG_NORMAL, 1),

            new("", null, POS_DEAD, 0, LOG_NORMAL, 0)
        };

        static string smash_dollar(string str)
        {
            if (string.IsNullOrEmpty(str)) return str ?? "";
            return str.Replace("$", "S");
        }

        public static void interpret(CharData ch, string argument)
        {
            while (argument.Length > 0 && Bit.isspace(argument[0]))
                argument = argument.Substring(1);
            if (argument.Length == 0)
                return;

            Bit.REMOVE_BIT(ref ch.affected_by, AFF_HIDE);

            if (!Bit.IS_NPC(ch) && Bit.IS_SET(ch.act, PLR_FREEZE))
            {
                Comm.send_to_char("You're totally frozen!\n\r", ch);
                return;
            }

            string logline = argument;
            string command;
            if (!char.IsLetter(argument[0]) && !char.IsDigit(argument[0]))
            {
                command = argument.Substring(0, 1);
                argument = argument.Substring(1);
                while (argument.Length > 0 && Bit.isspace(argument[0]))
                    argument = argument.Substring(1);
            }
            else
            {
                argument = RomString.one_argument(argument, out command);
            }

            bool found = false;
            int cmd = 0;
            int trust = Handler.get_trust(ch);
            for (cmd = 0; cmd_table[cmd].name.Length != 0; cmd++)
            {
                if (command[0] == cmd_table[cmd].name[0]
                    && !RomString.str_prefix(command, cmd_table[cmd].name)
                    && cmd_table[cmd].level <= trust)
                {
                    found = true;
                    break;
                }
            }

            logline = smash_dollar(logline);

            if (cmd_table[cmd].log == LOG_NEVER)
                logline = "";

            if ((!Bit.IS_NPC(ch) && Bit.IS_SET(ch.act, PLR_LOG))
                || Game.fLogAll
                || cmd_table[cmd].log == LOG_ALWAYS)
            {
                Game.log_buf = $"Log {ch.name}: {logline}";
                string s = Game.log_buf.Replace("$", "$$").Replace("{", "{{");
                Comm.wiznet(s, ch, null, WIZ_SECURE, 0, Handler.get_trust(ch));
                Db.log_string(Game.log_buf);
            }

            if (ch.desc != null && ch.desc.snoop_by != null)
            {
                Comm.write_to_buffer(ch.desc.snoop_by, "% ", 2);
                Comm.write_to_buffer(ch.desc.snoop_by, logline, 0);
                Comm.write_to_buffer(ch.desc.snoop_by, "\n\r", 2);
            }

            if (!found)
            {
                if (!check_social(ch, command, argument)
                    && !Imc.command_hook(ch, command, argument))
                    Comm.send_to_char("Huh?\n\r", ch);
                return;
            }

            if (ch.position < cmd_table[cmd].position)
            {
                switch (ch.position)
                {
                    case POS_DEAD:
                        Comm.send_to_char("Lie still; you are DEAD.\n\r", ch);
                        break;
                    case POS_MORTAL:
                    case POS_INCAP:
                        Comm.send_to_char("You are hurt far too bad for that.\n\r", ch);
                        break;
                    case POS_STUNNED:
                        Comm.send_to_char("You are too stunned to do that.\n\r", ch);
                        break;
                    case POS_SLEEPING:
                        Comm.send_to_char("In your dreams, or what?\n\r", ch);
                        break;
                    case POS_RESTING:
                        Comm.send_to_char("Nah... You feel too relaxed...\n\r", ch);
                        break;
                    case POS_SITTING:
                        Comm.send_to_char("Better stand up first.\n\r", ch);
                        break;
                    case POS_FIGHTING:
                        Comm.send_to_char("No way!  You are still fighting!\n\r", ch);
                        break;
                }
                return;
            }

            cmd_table[cmd].do_fun(ch, argument);
        }

        public static bool check_social(CharData ch, string command, string argument)
        {
            int cmd = 0;
            bool found = false;
            for (cmd = 0; Game.social_table[cmd].name.Length != 0; cmd++)
            {
                if (command[0] == Game.social_table[cmd].name[0]
                    && !RomString.str_prefix(command, Game.social_table[cmd].name))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                return false;

            if (!Bit.IS_NPC(ch) && Bit.IS_SET(ch.comm, COMM_NOEMOTE))
            {
                Comm.send_to_char("You are anti-social!\n\r", ch);
                return true;
            }

            switch (ch.position)
            {
                case POS_DEAD:
                    Comm.send_to_char("Lie still; you are DEAD.\n\r", ch);
                    return true;
                case POS_INCAP:
                case POS_MORTAL:
                    Comm.send_to_char("You are hurt far too bad for that.\n\r", ch);
                    return true;
                case POS_STUNNED:
                    Comm.send_to_char("You are too stunned to do that.\n\r", ch);
                    return true;
                case POS_SLEEPING:
                    if (!RomString.str_cmp(Game.social_table[cmd].name, "snore"))
                        break;
                    Comm.send_to_char("In your dreams, or what?\n\r", ch);
                    return true;
            }

            argument = RomString.one_argument(argument, out string arg);
            CharData victim = null;
            if (arg.Length == 0)
            {
                Comm.act(Game.social_table[cmd].others_no_arg, ch, null, victim, TO_ROOM);
                Comm.act(Game.social_table[cmd].char_no_arg, ch, null, victim, TO_CHAR);
            }
            else if ((victim = Handler.get_char_room(ch, arg)) == null)
            {
                Comm.send_to_char("They aren't here.\n\r", ch);
            }
            else if (victim == ch)
            {
                Comm.act(Game.social_table[cmd].others_auto, ch, null, victim, TO_ROOM);
                Comm.act(Game.social_table[cmd].char_auto, ch, null, victim, TO_CHAR);
            }
            else
            {
                Comm.act(Game.social_table[cmd].others_found, ch, null, victim, TO_NOTVICT);
                Comm.act(Game.social_table[cmd].char_found, ch, null, victim, TO_CHAR);
                Comm.act(Game.social_table[cmd].vict_found, ch, null, victim, TO_VICT);

                if (!Bit.IS_NPC(ch) && Bit.IS_NPC(victim)
                    && !Bit.IS_AFFECTED(victim, AFF_CHARM)
                    && Bit.IS_AWAKE(victim) && victim.desc == null)
                {
                    switch (RomRandom.number_bits(4))
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                            Comm.act(Game.social_table[cmd].others_found,
                                victim, null, ch, TO_NOTVICT);
                            Comm.act(Game.social_table[cmd].char_found, victim, null, ch,
                                TO_CHAR);
                            Comm.act(Game.social_table[cmd].vict_found, victim, null, ch,
                                TO_VICT);
                            break;
                        case 9:
                        case 10:
                        case 11:
                        case 12:
                            Comm.act("$n slaps $N.", victim, null, ch, TO_NOTVICT);
                            Comm.act("You slap $N.", victim, null, ch, TO_CHAR);
                            Comm.act("$n slaps you.", victim, null, ch, TO_VICT);
                            break;
                    }
                }
            }

            return true;
        }

        public static int atoi(string arg)
        {
            if (arg == null)
                return 0;
            int i = 0;
            while (i < arg.Length && Bit.isspace(arg[i]))
                i++;
            if (i >= arg.Length)
                return 0;
            int sign = 1;
            if (arg[i] == '+' || arg[i] == '-')
            {
                if (arg[i] == '-')
                    sign = -1;
                i++;
            }
            int n = 0;
            bool any = false;
            unchecked
            {
                while (i < arg.Length && arg[i] >= '0' && arg[i] <= '9')
                {
                    n = n * 10 + (arg[i] - '0');
                    any = true;
                    i++;
                }
            }
            return any ? sign * n : 0;
        }

        public static bool is_number(string arg)
        {
            if (string.IsNullOrEmpty(arg))
                return false;

            int i = 0;
            if (arg[0] == '+' || arg[0] == '-')
                i++;

            for (; i < arg.Length; i++)
            {
                if (!char.IsDigit(arg[i]))
                    return false;
            }

            return true;
        }

        public static void do_function(CharData ch, DoFun fun, string argument) => fun(ch, argument);

        public static void do_north(CharData ch, string argument) => ActMove.move_char(ch, DIR_NORTH, false);
        public static void do_east(CharData ch, string argument) => ActMove.move_char(ch, DIR_EAST, false);
        public static void do_south(CharData ch, string argument) => ActMove.move_char(ch, DIR_SOUTH, false);
        public static void do_west(CharData ch, string argument) => ActMove.move_char(ch, DIR_WEST, false);
        public static void do_up(CharData ch, string argument) => ActMove.move_char(ch, DIR_UP, false);
        public static void do_down(CharData ch, string argument) => ActMove.move_char(ch, DIR_DOWN, false);

        public static void do_look(CharData ch, string argument) => ActInfo.do_look(ch, argument);
        public static void do_exits(CharData ch, string argument) => ActInfo.do_exits(ch, argument);
        public static void do_inventory(CharData ch, string argument) => ActInfo.do_inventory(ch, argument);
        public static void do_equipment(CharData ch, string argument) => ActInfo.do_equipment(ch, argument);
        public static void do_score(CharData ch, string argument) => ActInfo.do_score(ch, argument);
        public static void do_who(CharData ch, string argument) => ActInfo.do_who(ch, argument);
        public static void do_help(CharData ch, string argument) => ActInfo.do_help(ch, argument);
        public static void do_credits(CharData ch, string argument) => do_help(ch, "diku");
        public static void do_time(CharData ch, string argument) => ActInfo.do_time(ch, argument);
        public static void do_weather(CharData ch, string argument) => ActInfo.do_weather(ch, argument);
        public static void do_areas(CharData ch, string argument) => ActInfo.do_areas(ch, argument);

        public static void do_wizhelp(CharData ch, string argument)
        {
            int col = 0;
            for (int cmd = 0; cmd_table[cmd].name.Length != 0; cmd++)
            {
                if (cmd_table[cmd].level >= LEVEL_HERO
                    && cmd_table[cmd].level <= Handler.get_trust(ch) && cmd_table[cmd].show != 0)
                {
                    Comm.send_to_char($"{cmd_table[cmd].name,-12}", ch);
                    if (++col % 6 == 0)
                        Comm.send_to_char("\n\r", ch);
                }
            }
            if (col % 6 != 0)
                Comm.send_to_char("\n\r", ch);
        }

        public static void do_commands(CharData ch, string argument)
        {
            int col = 0;
            for (int cmd = 0; cmd_table[cmd].name.Length != 0; cmd++)
            {
                if (cmd_table[cmd].level < LEVEL_HERO
                    && cmd_table[cmd].level <= Handler.get_trust(ch) && cmd_table[cmd].show != 0)
                {
                    Comm.send_to_char($"{cmd_table[cmd].name,-12}", ch);
                    if (++col % 6 == 0)
                        Comm.send_to_char("\n\r", ch);
                }
            }
            if (col % 6 != 0)
                Comm.send_to_char("\n\r", ch);
        }

        public static void do_qui(CharData ch, string argument)
            => Comm.send_to_char("If you want to QUIT, you have to spell it out.\n\r", ch);

        public static void do_quit(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            if (ch.position == POS_FIGHTING)
            {
                Comm.send_to_char("No way! You are fighting.\n\r", ch);
                return;
            }

            if (ch.position < POS_STUNNED)
            {
                Comm.send_to_char("You're not DEAD yet.\n\r", ch);
                return;
            }
            Comm.send_to_char("Alas, all good things must come to an end.\n\r", ch);
            Comm.act("$n has left the game.", ch, null, null, TO_ROOM);
            Game.log_buf = $"{ch.name} has quit.";
            Db.log_string(Game.log_buf);
            Comm.wiznet("$N rejoins the real world.", ch, null, WIZ_LOGINS, 0,
                Handler.get_trust(ch));

            Save.save_char_obj(ch);

            if (ch.pcdata.in_progress != null)
                Board.free_note(ch.pcdata.in_progress);

            long id = ch.id;
            var d = ch.desc;
            Handler.extract_char(ch, true);
            if (d != null)
                Comm.close_socket(d);

            DescriptorData d_next;
            for (d = Game.descriptor_list; d != null; d = d_next)
            {
                d_next = d.next;
                var tch = d.original ?? d.character;
                if (tch != null && tch.id == id)
                {
                    Handler.extract_char(tch, true);
                    Comm.close_socket(d);
                }
            }
        }

        public static void do_save(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch))
                return;

            Save.save_char_obj(ch);
            Comm.send_to_char("Saving. Remember that ROM has automatic saving now.\n\r",
                ch);
            Bit.WAIT_STATE(ch, 4 * PULSE_VIOLENCE);
        }
    }
}
