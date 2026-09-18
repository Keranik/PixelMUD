// Hand-filled from const.c / tables.c until extract_tables.py can run (no Python on this machine).
using static Rom24.Merc;

namespace Rom24
{
    public static partial class Tables
    {
        public static readonly ItemType[] item_table =
        {
            new ItemType(ITEM_LIGHT, "light"), new ItemType(ITEM_SCROLL, "scroll"),
            new ItemType(ITEM_WAND, "wand"), new ItemType(ITEM_STAFF, "staff"),
            new ItemType(ITEM_WEAPON, "weapon"), new ItemType(ITEM_TREASURE, "treasure"),
            new ItemType(ITEM_ARMOR, "armor"), new ItemType(ITEM_POTION, "potion"),
            new ItemType(ITEM_CLOTHING, "clothing"), new ItemType(ITEM_FURNITURE, "furniture"),
            new ItemType(ITEM_TRASH, "trash"), new ItemType(ITEM_CONTAINER, "container"),
            new ItemType(ITEM_DRINK_CON, "drink"), new ItemType(ITEM_KEY, "key"),
            new ItemType(ITEM_FOOD, "food"), new ItemType(ITEM_MONEY, "money"),
            new ItemType(ITEM_BOAT, "boat"), new ItemType(ITEM_CORPSE_NPC, "npc_corpse"),
            new ItemType(ITEM_CORPSE_PC, "pc_corpse"), new ItemType(ITEM_FOUNTAIN, "fountain"),
            new ItemType(ITEM_PILL, "pill"), new ItemType(ITEM_PROTECT, "protect"),
            new ItemType(ITEM_MAP, "map"), new ItemType(ITEM_PORTAL, "portal"),
            new ItemType(ITEM_WARP_STONE, "warp_stone"), new ItemType(ITEM_ROOM_KEY, "room_key"),
            new ItemType(ITEM_GEM, "gem"), new ItemType(ITEM_JEWELRY, "jewelry"),
            new ItemType(ITEM_JUKEBOX, "jukebox"), new ItemType(0, null)
        };

        public static readonly AttackType[] attack_table =
        {
            new AttackType("none", "hit", -1),
            new AttackType("slice", "slice", DAM_SLASH),
            new AttackType("stab", "stab", DAM_PIERCE),
            new AttackType("slash", "slash", DAM_SLASH),
            new AttackType("whip", "whip", DAM_SLASH),
            new AttackType("claw", "claw", DAM_SLASH),
            new AttackType("blast", "blast", DAM_BASH),
            new AttackType("pound", "pound", DAM_BASH),
            new AttackType("crush", "crush", DAM_BASH),
            new AttackType("grep", "grep", DAM_SLASH),
            new AttackType("bite", "bite", DAM_PIERCE),
            new AttackType("pierce", "pierce", DAM_PIERCE),
            new AttackType("suction", "suction", DAM_BASH),
            new AttackType("beating", "beating", DAM_BASH),
            new AttackType("digestion", "digestion", DAM_ACID),
            new AttackType("charge", "charge", DAM_BASH),
            new AttackType("slap", "slap", DAM_BASH),
            new AttackType("punch", "punch", DAM_BASH),
            new AttackType("wrath", "wrath", DAM_ENERGY),
            new AttackType("magic", "magic", DAM_ENERGY),
            new AttackType("divine", "divine power", DAM_HOLY),
            new AttackType("cleave", "cleave", DAM_SLASH),
            new AttackType("scratch", "scratch", DAM_PIERCE),
            new AttackType("peck", "peck", DAM_PIERCE),
            new AttackType("peckb", "peck", DAM_BASH),
            new AttackType("chop", "chop", DAM_SLASH),
            new AttackType("sting", "sting", DAM_PIERCE),
            new AttackType("smash", "smash", DAM_BASH),
            new AttackType("shbite", "shocking bite", DAM_LIGHTNING),
            new AttackType("flbite", "flaming bite", DAM_FIRE),
            new AttackType("frbite", "freezing bite", DAM_COLD),
            new AttackType("acbite", "acidic bite", DAM_ACID),
            new AttackType("chomp", "chomp", DAM_PIERCE),
            new AttackType("drain", "life drain", DAM_NEGATIVE),
            new AttackType("thrust", "thrust", DAM_PIERCE),
            new AttackType("slime", "slime", DAM_ACID),
            new AttackType("shock", "shock", DAM_LIGHTNING),
            new AttackType("thwack", "thwack", DAM_BASH),
            new AttackType("flame", "flame", DAM_FIRE),
            new AttackType("chill", "chill", DAM_COLD),
            new AttackType(null, null, 0)
        };

        public static readonly WeaponType[] weapon_table =
        {
            new WeaponType("sword", OBJ_VNUM_SCHOOL_SWORD, WEAPON_SWORD, "sword"),
            new WeaponType("mace", OBJ_VNUM_SCHOOL_MACE, WEAPON_MACE, "mace"),
            new WeaponType("dagger", OBJ_VNUM_SCHOOL_DAGGER, WEAPON_DAGGER, "dagger"),
            new WeaponType("axe", OBJ_VNUM_SCHOOL_AXE, WEAPON_AXE, "axe"),
            new WeaponType("staff", OBJ_VNUM_SCHOOL_STAFF, WEAPON_SPEAR, "spear"),
            new WeaponType("flail", OBJ_VNUM_SCHOOL_FLAIL, WEAPON_FLAIL, "flail"),
            new WeaponType("whip", OBJ_VNUM_SCHOOL_WHIP, WEAPON_WHIP, "whip"),
            new WeaponType("polearm", OBJ_VNUM_SCHOOL_POLEARM, WEAPON_POLEARM, "polearm"),
            new WeaponType(null, 0, 0, null)
        };

        public static readonly LiqType[] liq_table =
        {
            new LiqType("water", "clear", new short[] {0,1,10,0,16}),
            new LiqType("beer", "amber", new short[] {12,1,8,1,12}),
            new LiqType("red wine", "burgundy", new short[] {30,1,8,1,5}),
            new LiqType("ale", "brown", new short[] {15,1,8,1,12}),
            new LiqType("dark ale", "dark", new short[] {16,1,8,1,12}),
            new LiqType("whisky", "golden", new short[] {120,1,5,0,2}),
            new LiqType("lemonade", "pink", new short[] {0,1,9,2,12}),
            new LiqType("firebreather", "boiling", new short[] {190,0,4,0,2}),
            new LiqType("local specialty", "clear", new short[] {151,1,3,0,2}),
            new LiqType("slime mold juice", "green", new short[] {0,2,-8,1,2}),
            new LiqType("milk", "white", new short[] {0,2,9,3,12}),
            new LiqType("tea", "tan", new short[] {0,1,8,0,6}),
            new LiqType("coffee", "black", new short[] {0,1,8,0,6}),
            new LiqType("blood", "red", new short[] {0,2,-1,2,6}),
            new LiqType("salt water", "clear", new short[] {0,1,-2,0,1}),
            new LiqType("coke", "brown", new short[] {0,2,9,2,12}),
            new LiqType("root beer", "brown", new short[] {0,2,9,2,12}),
            new LiqType("elvish wine", "green", new short[] {35,2,8,1,5}),
            new LiqType("white wine", "golden", new short[] {28,1,8,1,5}),
            new LiqType("champagne", "golden", new short[] {32,1,8,1,5}),
            new LiqType("mead", "honey-colored", new short[] {34,2,8,2,12}),
            new LiqType("rose wine", "pink", new short[] {26,1,8,1,5}),
            new LiqType("benedictine wine", "burgundy", new short[] {40,1,8,1,5}),
            new LiqType("vodka", "clear", new short[] {130,1,5,0,2}),
            new LiqType("cranberry juice", "red", new short[] {0,1,9,2,12}),
            new LiqType("orange juice", "orange", new short[] {0,2,9,3,12}),
            new LiqType("absinthe", "green", new short[] {200,1,4,0,2}),
            new LiqType("brandy", "golden", new short[] {80,1,5,0,4}),
            new LiqType("aquavit", "clear", new short[] {140,1,5,0,2}),
            new LiqType("schnapps", "clear", new short[] {90,1,5,0,2}),
            new LiqType("icewine", "purple", new short[] {50,2,6,1,5}),
            new LiqType("amontillado", "burgundy", new short[] {35,2,8,1,5}),
            new LiqType("sherry", "red", new short[] {38,2,7,1,5}),
            new LiqType("framboise", "red", new short[] {50,1,7,1,5}),
            new LiqType("rum", "amber", new short[] {151,1,4,0,2}),
            new LiqType("cordial", "clear", new short[] {100,1,5,0,2}),
            new LiqType(null, null, new short[] {0,0,0,0,0})
        };

        public static readonly RaceType[] race_table =
        {
            new RaceType("unique", false, 0,0,0,0,0,0,0,0),
            new RaceType("human", true, 0,0,0,0,0,0, A|H|M|V, A|B|C|D|E|F|G|H|I|J|K),
            new RaceType("elf", true, 0, AFF_INFRARED, 0, 0, RES_CHARM, VULN_IRON, A|H|M|V, A|B|C|D|E|F|G|H|I|J|K),
            new RaceType("dwarf", true, 0, AFF_INFRARED, 0, 0, RES_POISON|RES_DISEASE, VULN_DROWNING, A|H|M|V, A|B|C|D|E|F|G|H|I|J|K),
            new RaceType("giant", true, 0,0,0, 0, RES_FIRE|RES_COLD, VULN_MENTAL|VULN_LIGHTNING, A|H|M|V, A|B|C|D|E|F|G|H|I|J|K),
            new RaceType("bat", false, 0, AFF_FLYING|AFF_DARK_VISION, OFF_DODGE|OFF_FAST, 0,0, VULN_LIGHT, A|G|V, A|C|D|E|F|H|J|K|P),
            new RaceType("bear", false, 0,0, OFF_CRUSH|OFF_DISARM|OFF_BERSERK, 0, RES_BASH|RES_COLD, 0, A|G|V, A|B|C|D|E|F|H|J|K|U|V),
            new RaceType("cat", false, 0, AFF_DARK_VISION, OFF_FAST|OFF_DODGE, 0,0,0, A|G|V, A|C|D|E|F|H|J|K|Q|U|V),
            new RaceType("centipede", false, 0, AFF_DARK_VISION, 0, 0, RES_PIERCE|RES_COLD, VULN_BASH, A|B|G|O, A|C|K),
            new RaceType("dog", false, 0,0, OFF_FAST, 0,0,0, A|G|V, A|C|D|E|F|H|J|K|U|V),
            new RaceType("doll", false, 0,0,0, IMM_COLD|IMM_POISON|IMM_HOLY|IMM_NEGATIVE|IMM_MENTAL|IMM_DISEASE|IMM_DROWNING, RES_BASH|RES_LIGHT, VULN_SLASH|VULN_FIRE|VULN_ACID|VULN_LIGHTNING|VULN_ENERGY, E|J|M|cc, A|B|C|G|H|K),
            new RaceType("dragon", false, 0, AFF_INFRARED|AFF_FLYING, 0, 0, RES_FIRE|RES_BASH|RES_CHARM, VULN_PIERCE|VULN_COLD, A|H|Z, A|C|D|E|F|G|H|I|J|K|P|Q|U|V|X),
            new RaceType("fido", false, 0,0, OFF_DODGE|ASSIST_RACE, 0,0, VULN_MAGIC, A|B|G|V, A|C|D|E|F|H|J|K|Q|V),
            new RaceType("fox", false, 0, AFF_DARK_VISION, OFF_FAST|OFF_DODGE, 0,0,0, A|G|V, A|C|D|E|F|H|J|K|Q|V),
            new RaceType("goblin", false, 0, AFF_INFRARED, 0, 0, RES_DISEASE, VULN_MAGIC, A|H|M|V, A|B|C|D|E|F|G|H|I|J|K),
            new RaceType("hobgoblin", false, 0, AFF_INFRARED, 0, 0, RES_DISEASE|RES_POISON, 0, A|H|M|V, A|B|C|D|E|F|G|H|I|J|K|Y),
            new RaceType("kobold", false, 0, AFF_INFRARED, 0, 0, RES_POISON, VULN_MAGIC, A|B|H|M|V, A|B|C|D|E|F|G|H|I|J|K|Q),
            new RaceType("lizard", false, 0,0,0, 0, RES_POISON, VULN_COLD, A|G|X|cc, A|C|D|E|F|H|K|Q|V),
            new RaceType("modron", false, 0, AFF_INFRARED, ASSIST_RACE|ASSIST_ALIGN, IMM_CHARM|IMM_DISEASE|IMM_MENTAL|IMM_HOLY|IMM_NEGATIVE, RES_FIRE|RES_COLD|RES_ACID, 0, H, A|B|C|G|H|J|K),
            new RaceType("orc", false, 0, AFF_INFRARED, 0, 0, RES_DISEASE, VULN_LIGHT, A|H|M|V, A|B|C|D|E|F|G|H|I|J|K),
            new RaceType("pig", false, 0,0,0,0,0,0, A|G|V, A|C|D|E|F|H|J|K),
            new RaceType("rabbit", false, 0,0, OFF_DODGE|OFF_FAST, 0,0,0, A|G|V, A|C|D|E|F|H|J|K),
            new RaceType("school monster", false, ACT_NOALIGN, 0,0, IMM_CHARM|IMM_SUMMON, 0, VULN_MAGIC, A|M|V, A|B|C|D|E|F|H|J|K|Q|U),
            new RaceType("snake", false, 0,0,0, 0, RES_POISON, VULN_COLD, A|G|X|Y|cc, A|D|E|F|K|L|Q|V|X),
            new RaceType("song bird", false, 0, AFF_FLYING, OFF_FAST|OFF_DODGE, 0,0,0, A|G|W, A|C|D|E|F|H|K|P),
            new RaceType("troll", false, 0, AFF_REGENERATION|AFF_INFRARED|AFF_DETECT_HIDDEN, OFF_BERSERK, 0, RES_CHARM|RES_BASH, VULN_FIRE|VULN_ACID, A|B|H|M|V, A|B|C|D|E|F|G|H|I|J|K|U|V),
            new RaceType("water fowl", false, 0, AFF_SWIM|AFF_FLYING, 0,0, RES_DROWNING, 0, A|G|W, A|C|D|E|F|H|K|P),
            new RaceType("wolf", false, 0, AFF_DARK_VISION, OFF_FAST|OFF_DODGE, 0,0,0, A|G|V, A|C|D|E|F|J|K|Q|V),
            new RaceType("wyvern", false, 0, AFF_FLYING|AFF_DETECT_INVIS|AFF_DETECT_HIDDEN, OFF_BASH|OFF_FAST|OFF_DODGE, IMM_POISON, 0, VULN_LIGHT, A|B|G|Z, A|C|D|E|F|H|J|K|Q|V|X),
            new RaceType("unique", false, 0,0,0, 0,0,0, 0, 0),
            null
        };

        public static readonly string[] skill_names =
        {
            "reserved","acid blast","armor","bless","blindness","burning hands","call lightning",
            "calm","cancellation","cause critical","cause light","cause serious","chain lightning",
            "change sex","charm person","chill touch","colour spray","continual light","control weather",
            "create food","create rose","create spring","create water","cure blindness","cure critical",
            "cure disease","cure light","cure poison","cure serious","curse","demonfire","detect evil",
            "detect good","detect hidden","detect invis","detect magic","detect poison","dispel evil",
            "dispel good","dispel magic","earthquake","enchant armor","enchant weapon","energy drain",
            "faerie fire","faerie fog","farsight","fireball","fireproof","flamestrike","fly",
            "floating disc","frenzy","gate","giant strength","harm","haste","heal","heat metal",
            "holy word","identify","infravision","invisibility","know alignment","lightning bolt",
            "locate object","magic missile","mass healing","mass invis","nexus","pass door","plague",
            "poison","portal","protection evil","protection good","ray of truth","recharge","refresh",
            "remove curse","sanctuary","shield","shocking grasp","sleep","slow","stone skin","summon",
            "teleport","ventriloquate","weaken","word of recall","acid breath","fire breath",
            "frost breath","gas breath","lightning breath","general purpose","high explosive",
            "axe","dagger","flail","mace","polearm","shield block","spear","sword","whip",
            "backstab","bash","berserk","dirt kicking","disarm","dodge","enhanced damage","envenom",
            "hand to hand","kick","parry","rescue","trip","second attack","third attack",
            "fast healing","haggle","hide","lore","meditation","peek","pick lock","sneak","steal",
            "scrolls","staves","wands","recall", null
        };

        public static readonly ClanType[] clan_table =
        {
            new ClanType("", "", ROOM_VNUM_ALTAR, true),
            new ClanType("loner", "[ Loner ] ", ROOM_VNUM_ALTAR, true),
            new ClanType("rom", "[  ROM  ] ", ROOM_VNUM_ALTAR, false)
        };

        public static readonly WiznetType[] wiznet_table =
        {
            new WiznetType("on", WIZ_ON, LEVEL_IMMORTAL),
            new WiznetType("prefix", WIZ_PREFIX, LEVEL_IMMORTAL),
            new WiznetType("ticks", WIZ_TICKS, LEVEL_IMMORTAL),
            new WiznetType("logins", WIZ_LOGINS, LEVEL_IMMORTAL),
            new WiznetType("sites", WIZ_SITES, MAX_LEVEL - 4),
            new WiznetType("links", WIZ_LINKS, MAX_LEVEL - 7),
            new WiznetType("newbies", WIZ_NEWBIE, LEVEL_IMMORTAL),
            new WiznetType("spam", WIZ_SPAM, MAX_LEVEL - 5),
            new WiznetType("deaths", WIZ_DEATHS, LEVEL_IMMORTAL),
            new WiznetType("resets", WIZ_RESETS, MAX_LEVEL - 4),
            new WiznetType("mobdeaths", WIZ_MOBDEATHS, MAX_LEVEL - 4),
            new WiznetType("flags", WIZ_FLAGS, MAX_LEVEL - 5),
            new WiznetType("penalties", WIZ_PENALTIES, MAX_LEVEL - 5),
            new WiznetType("saccing", WIZ_SACCING, MAX_LEVEL - 5),
            new WiznetType("levels", WIZ_LEVELS, LEVEL_IMMORTAL),
            new WiznetType("load", WIZ_LOAD, MAX_LEVEL - 2),
            new WiznetType("restore", WIZ_RESTORE, MAX_LEVEL - 2),
            new WiznetType("snoops", WIZ_SNOOPS, MAX_LEVEL - 2),
            new WiznetType("switches", WIZ_SWITCHES, MAX_LEVEL - 2),
            new WiznetType("secure", WIZ_SECURE, MAX_LEVEL - 1),
            new WiznetType(null, 0, 0)
        };

        public static readonly PositionType[] position_table =
        {
            new PositionType("dead", "dead"), new PositionType("mortally wounded", "mort"),
            new PositionType("incapacitated", "incap"), new PositionType("stunned", "stun"),
            new PositionType("sleeping", "sleep"), new PositionType("resting", "rest"),
            new PositionType("sitting", "sit"), new PositionType("fighting", "fight"),
            new PositionType("standing", "stand"), new PositionType(null, null)
        };

        public static readonly SexType[] sex_table =
        {
            new SexType("none"), new SexType("male"), new SexType("female"), new SexType("either"), new SexType(null)
        };

        public static readonly SizeType[] size_table =
        {
            new SizeType("tiny"), new SizeType("small"), new SizeType("medium"),
            new SizeType("large"), new SizeType("huge"), new SizeType("giant"), new SizeType(null)
        };

        public static readonly ClassType[] class_table =
        {
            new ClassType("mage", "Mag", STAT_INT, OBJ_VNUM_SCHOOL_DAGGER, new[] {3018, 9618}, 75, 20, 6, 6, 8, true, "mage basics", "mage default"),
            new ClassType("cleric", "Cle", STAT_WIS, OBJ_VNUM_SCHOOL_MACE, new[] {3003, 9619}, 75, 20, 2, 7, 10, true, "cleric basics", "cleric default"),
            new ClassType("thief", "Thi", STAT_DEX, OBJ_VNUM_SCHOOL_DAGGER, new[] {3028, 9639}, 75, 20, -4, 8, 13, false, "thief basics", "thief default"),
            new ClassType("warrior", "War", STAT_STR, OBJ_VNUM_SCHOOL_SWORD, new[] {3022, 9633}, 75, 20, -10, 11, 15, false, "warrior basics", "warrior default")
        };

        public static readonly PcRaceType[] pc_race_table =
        {
            new PcRaceType("null race", "", 0, new[] {100,100,100,100}, new[] {""}, new[] {13,13,13,13,13}, new[] {18,18,18,18,18}, 0),
            new PcRaceType("human", "Human", 0, new[] {100,100,100,100}, new[] {""}, new[] {13,13,13,13,13}, new[] {18,18,18,18,18}, SIZE_MEDIUM),
            new PcRaceType("elf", " Elf ", 5, new[] {100,125,100,120}, new[] {"sneak","hide"}, new[] {12,14,13,15,11}, new[] {16,20,18,21,15}, SIZE_SMALL),
            new PcRaceType("dwarf", "Dwarf", 8, new[] {150,100,125,100}, new[] {"berserk"}, new[] {14,12,14,10,15}, new[] {20,16,19,14,21}, SIZE_MEDIUM),
            new PcRaceType("giant", "Giant", 6, new[] {200,150,150,105}, new[] {"bash","fast healing"}, new[] {16,11,13,11,14}, new[] {22,15,18,15,20}, SIZE_LARGE)
        };

        public static readonly StrAppType[] str_app =
        {
            new StrAppType(-5,-4,0,0), new StrAppType(-5,-4,3,1), new StrAppType(-3,-2,3,2),
            new StrAppType(-3,-1,10,3), new StrAppType(-2,-1,25,4), new StrAppType(-2,-1,55,5),
            new StrAppType(-1,0,80,6), new StrAppType(-1,0,90,7), new StrAppType(0,0,100,8),
            new StrAppType(0,0,100,9), new StrAppType(0,0,115,10), new StrAppType(0,0,115,11),
            new StrAppType(0,0,130,12), new StrAppType(0,0,130,13), new StrAppType(0,1,140,14),
            new StrAppType(1,1,150,15), new StrAppType(1,2,165,16), new StrAppType(2,3,180,22),
            new StrAppType(2,3,200,25), new StrAppType(3,4,225,30), new StrAppType(3,5,250,35),
            new StrAppType(4,6,300,40), new StrAppType(4,6,350,45), new StrAppType(5,7,400,50),
            new StrAppType(5,8,450,55), new StrAppType(6,9,500,60)
        };

        public static readonly int[] int_app =
        {
            3,5,7,8,9,10,11,12,13,15,17,19,22,25,28,31,34,37,40,44,49,55,60,70,80,85
        };

        public static readonly WisAppType[] wis_app =
        {
            new WisAppType(0), new WisAppType(0), new WisAppType(0), new WisAppType(0),
            new WisAppType(0), new WisAppType(1), new WisAppType(1), new WisAppType(1),
            new WisAppType(1), new WisAppType(1), new WisAppType(1), new WisAppType(1),
            new WisAppType(1), new WisAppType(1), new WisAppType(1), new WisAppType(2),
            new WisAppType(2), new WisAppType(2), new WisAppType(3), new WisAppType(3),
            new WisAppType(3), new WisAppType(3), new WisAppType(4), new WisAppType(4),
            new WisAppType(4), new WisAppType(5)
        };

        public static readonly ConAppType[] con_app =
        {
            new ConAppType(-4,20), new ConAppType(-3,25), new ConAppType(-2,30), new ConAppType(-2,35),
            new ConAppType(-1,40), new ConAppType(-1,45), new ConAppType(-1,50), new ConAppType(0,55),
            new ConAppType(0,60), new ConAppType(0,65), new ConAppType(0,70), new ConAppType(0,75),
            new ConAppType(0,80), new ConAppType(0,85), new ConAppType(0,88), new ConAppType(1,90),
            new ConAppType(2,95), new ConAppType(2,97), new ConAppType(3,99), new ConAppType(3,99),
            new ConAppType(4,99), new ConAppType(4,99), new ConAppType(5,99), new ConAppType(6,99),
            new ConAppType(7,99), new ConAppType(8,99)
        };

        public static readonly DexAppType[] dex_app =
        {
            new DexAppType(60), new DexAppType(50), new DexAppType(50), new DexAppType(40),
            new DexAppType(30), new DexAppType(20), new DexAppType(10), new DexAppType(0),
            new DexAppType(0), new DexAppType(0), new DexAppType(0), new DexAppType(0),
            new DexAppType(0), new DexAppType(0), new DexAppType(0), new DexAppType(-10),
            new DexAppType(-15), new DexAppType(-20), new DexAppType(-30), new DexAppType(-40),
            new DexAppType(-50), new DexAppType(-60), new DexAppType(-75), new DexAppType(-90),
            new DexAppType(-105), new DexAppType(-120)
        };

        public static readonly FlagType[] door_resets =
        {
            new FlagType("open and unlocked", 0, true),
            new FlagType("closed and unlocked", 1, true),
            new FlagType("closed and locked", 2, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] wear_loc_strings =
        {
            new FlagType("in the inventory", WEAR_NONE, true),
            new FlagType("as a light", WEAR_LIGHT, true),
            new FlagType("on the left finger", WEAR_FINGER_L, true),
            new FlagType("on the right finger", WEAR_FINGER_R, true),
            new FlagType("around the neck (1)", WEAR_NECK_1, true),
            new FlagType("around the neck (2)", WEAR_NECK_2, true),
            new FlagType("on the body", WEAR_BODY, true),
            new FlagType("over the head", WEAR_HEAD, true),
            new FlagType("on the legs", WEAR_LEGS, true),
            new FlagType("on the feet", WEAR_FEET, true),
            new FlagType("on the hands", WEAR_HANDS, true),
            new FlagType("on the arms", WEAR_ARMS, true),
            new FlagType("as a shield", WEAR_SHIELD, true),
            new FlagType("about the shoulders", WEAR_ABOUT, true),
            new FlagType("around the waist", WEAR_WAIST, true),
            new FlagType("on the left wrist", WEAR_WRIST_L, true),
            new FlagType("on the right wrist", WEAR_WRIST_R, true),
            new FlagType("wielded", WEAR_WIELD, true),
            new FlagType("held in the hands", WEAR_HOLD, true),
            new FlagType("floating nearby", WEAR_FLOAT, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] wear_loc_flags =
        {
            new FlagType("none", WEAR_NONE, true),
            new FlagType("light", WEAR_LIGHT, true),
            new FlagType("lfinger", WEAR_FINGER_L, true),
            new FlagType("rfinger", WEAR_FINGER_R, true),
            new FlagType("neck1", WEAR_NECK_1, true),
            new FlagType("neck2", WEAR_NECK_2, true),
            new FlagType("body", WEAR_BODY, true),
            new FlagType("head", WEAR_HEAD, true),
            new FlagType("legs", WEAR_LEGS, true),
            new FlagType("feet", WEAR_FEET, true),
            new FlagType("hands", WEAR_HANDS, true),
            new FlagType("arms", WEAR_ARMS, true),
            new FlagType("shield", WEAR_SHIELD, true),
            new FlagType("about", WEAR_ABOUT, true),
            new FlagType("waist", WEAR_WAIST, true),
            new FlagType("lwrist", WEAR_WRIST_L, true),
            new FlagType("rwrist", WEAR_WRIST_R, true),
            new FlagType("wielded", WEAR_WIELD, true),
            new FlagType("hold", WEAR_HOLD, true),
            new FlagType("floating", WEAR_FLOAT, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] act_flags =
        {
            new FlagType("npc", A, false),
            new FlagType("sentinel", B, true),
            new FlagType("scavenger", C, true),
            new FlagType("aggressive", F, true),
            new FlagType("stay_area", G, true),
            new FlagType("wimpy", H, true),
            new FlagType("pet", I, true),
            new FlagType("train", J, true),
            new FlagType("practice", K, true),
            new FlagType("undead", O, true),
            new FlagType("cleric", Q, true),
            new FlagType("mage", R, true),
            new FlagType("thief", S, true),
            new FlagType("warrior", T, true),
            new FlagType("noalign", U, true),
            new FlagType("nopurge", V, true),
            new FlagType("outdoors", W, true),
            new FlagType("indoors", Y, true),
            new FlagType("healer", aa, true),
            new FlagType("gain", bb, true),
            new FlagType("update_always", cc, true),
            new FlagType("changer", dd, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] plr_flags =
        {
            new FlagType("npc", A, false),
            new FlagType("autoassist", C, false),
            new FlagType("autoexit", D, false),
            new FlagType("autoloot", E, false),
            new FlagType("autosac", F, false),
            new FlagType("autogold", G, false),
            new FlagType("autosplit", H, false),
            new FlagType("holylight", N, false),
            new FlagType("can_loot", P, false),
            new FlagType("nosummon", Q, false),
            new FlagType("nofollow", R, false),
            new FlagType("colour", T, false),
            new FlagType("permit", U, true),
            new FlagType("log", W, false),
            new FlagType("deny", X, false),
            new FlagType("freeze", Y, false),
            new FlagType("thief", Z, false),
            new FlagType("killer", aa, false),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] affect_flags =
        {
            new FlagType("blind", A, true),
            new FlagType("invisible", B, true),
            new FlagType("detect_evil", C, true),
            new FlagType("detect_invis", D, true),
            new FlagType("detect_magic", E, true),
            new FlagType("detect_hidden", F, true),
            new FlagType("detect_good", G, true),
            new FlagType("sanctuary", H, true),
            new FlagType("faerie_fire", I, true),
            new FlagType("infrared", J, true),
            new FlagType("curse", K, true),
            new FlagType("poison", M, true),
            new FlagType("protect_evil", N, true),
            new FlagType("protect_good", O, true),
            new FlagType("sneak", P, true),
            new FlagType("hide", Q, true),
            new FlagType("sleep", R, true),
            new FlagType("charm", S, true),
            new FlagType("flying", T, true),
            new FlagType("pass_door", U, true),
            new FlagType("haste", V, true),
            new FlagType("calm", W, true),
            new FlagType("plague", X, true),
            new FlagType("weaken", Y, true),
            new FlagType("dark_vision", Z, true),
            new FlagType("berserk", aa, true),
            new FlagType("swim", bb, true),
            new FlagType("regeneration", cc, true),
            new FlagType("slow", dd, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] off_flags =
        {
            new FlagType("area_attack", A, true),
            new FlagType("backstab", B, true),
            new FlagType("bash", C, true),
            new FlagType("berserk", D, true),
            new FlagType("disarm", E, true),
            new FlagType("dodge", F, true),
            new FlagType("fade", G, true),
            new FlagType("fast", H, true),
            new FlagType("kick", I, true),
            new FlagType("dirt_kick", J, true),
            new FlagType("parry", K, true),
            new FlagType("rescue", L, true),
            new FlagType("tail", M, true),
            new FlagType("trip", N, true),
            new FlagType("crush", O, true),
            new FlagType("assist_all", P, true),
            new FlagType("assist_align", Q, true),
            new FlagType("assist_race", R, true),
            new FlagType("assist_players", S, true),
            new FlagType("assist_guard", T, true),
            new FlagType("assist_vnum", U, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] imm_flags =
        {
            new FlagType("summon", A, true),
            new FlagType("charm", B, true),
            new FlagType("magic", C, true),
            new FlagType("weapon", D, true),
            new FlagType("bash", E, true),
            new FlagType("pierce", F, true),
            new FlagType("slash", G, true),
            new FlagType("fire", H, true),
            new FlagType("cold", I, true),
            new FlagType("lightning", J, true),
            new FlagType("acid", K, true),
            new FlagType("poison", L, true),
            new FlagType("negative", M, true),
            new FlagType("holy", N, true),
            new FlagType("energy", O, true),
            new FlagType("mental", P, true),
            new FlagType("disease", Q, true),
            new FlagType("drowning", R, true),
            new FlagType("light", S, true),
            new FlagType("sound", T, true),
            new FlagType("wood", X, true),
            new FlagType("silver", Y, true),
            new FlagType("iron", Z, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] form_flags =
        {
            new FlagType("edible", FORM_EDIBLE, true),
            new FlagType("poison", FORM_POISON, true),
            new FlagType("magical", FORM_MAGICAL, true),
            new FlagType("instant_decay", FORM_INSTANT_DECAY, true),
            new FlagType("other", FORM_OTHER, true),
            new FlagType("animal", FORM_ANIMAL, true),
            new FlagType("sentient", FORM_SENTIENT, true),
            new FlagType("undead", FORM_UNDEAD, true),
            new FlagType("construct", FORM_CONSTRUCT, true),
            new FlagType("mist", FORM_MIST, true),
            new FlagType("intangible", FORM_INTANGIBLE, true),
            new FlagType("biped", FORM_BIPED, true),
            new FlagType("centaur", FORM_CENTAUR, true),
            new FlagType("insect", FORM_INSECT, true),
            new FlagType("spider", FORM_SPIDER, true),
            new FlagType("crustacean", FORM_CRUSTACEAN, true),
            new FlagType("worm", FORM_WORM, true),
            new FlagType("blob", FORM_BLOB, true),
            new FlagType("mammal", FORM_MAMMAL, true),
            new FlagType("bird", FORM_BIRD, true),
            new FlagType("reptile", FORM_REPTILE, true),
            new FlagType("snake", FORM_SNAKE, true),
            new FlagType("dragon", FORM_DRAGON, true),
            new FlagType("amphibian", FORM_AMPHIBIAN, true),
            new FlagType("fish", FORM_FISH, true),
            new FlagType("cold_blood", FORM_COLD_BLOOD, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] part_flags =
        {
            new FlagType("head", PART_HEAD, true),
            new FlagType("arms", PART_ARMS, true),
            new FlagType("legs", PART_LEGS, true),
            new FlagType("heart", PART_HEART, true),
            new FlagType("brains", PART_BRAINS, true),
            new FlagType("guts", PART_GUTS, true),
            new FlagType("hands", PART_HANDS, true),
            new FlagType("feet", PART_FEET, true),
            new FlagType("fingers", PART_FINGERS, true),
            new FlagType("ear", PART_EAR, true),
            new FlagType("eye", PART_EYE, true),
            new FlagType("long_tongue", PART_LONG_TONGUE, true),
            new FlagType("eyestalks", PART_EYESTALKS, true),
            new FlagType("tentacles", PART_TENTACLES, true),
            new FlagType("fins", PART_FINS, true),
            new FlagType("wings", PART_WINGS, true),
            new FlagType("tail", PART_TAIL, true),
            new FlagType("claws", PART_CLAWS, true),
            new FlagType("fangs", PART_FANGS, true),
            new FlagType("horns", PART_HORNS, true),
            new FlagType("scales", PART_SCALES, true),
            new FlagType("tusks", PART_TUSKS, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] comm_flags =
        {
            new FlagType("quiet", COMM_QUIET, true),
            new FlagType("deaf", COMM_DEAF, true),
            new FlagType("nowiz", COMM_NOWIZ, true),
            new FlagType("noclangossip", COMM_NOAUCTION, true),
            new FlagType("nogossip", COMM_NOGOSSIP, true),
            new FlagType("noquestion", COMM_NOQUESTION, true),
            new FlagType("nomusic", COMM_NOMUSIC, true),
            new FlagType("noclan", COMM_NOCLAN, true),
            new FlagType("noquote", COMM_NOQUOTE, true),
            new FlagType("shoutsoff", COMM_SHOUTSOFF, true),
            new FlagType("compact", COMM_COMPACT, true),
            new FlagType("brief", COMM_BRIEF, true),
            new FlagType("prompt", COMM_PROMPT, true),
            new FlagType("combine", COMM_COMBINE, true),
            new FlagType("telnet_ga", COMM_TELNET_GA, true),
            new FlagType("show_affects", COMM_SHOW_AFFECTS, true),
            new FlagType("nograts", COMM_NOGRATS, true),
            new FlagType("noemote", COMM_NOEMOTE, false),
            new FlagType("noshout", COMM_NOSHOUT, false),
            new FlagType("notell", COMM_NOTELL, false),
            new FlagType("nochannels", COMM_NOCHANNELS, false),
            new FlagType("snoop_proof", COMM_SNOOP_PROOF, false),
            new FlagType("afk", COMM_AFK, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] mprog_flags =
        {
            new FlagType("act", TRIG_ACT, true), new FlagType("bribe", TRIG_BRIBE, true),
            new FlagType("death", TRIG_DEATH, true), new FlagType("entry", TRIG_ENTRY, true),
            new FlagType("fight", TRIG_FIGHT, true), new FlagType("give", TRIG_GIVE, true),
            new FlagType("greet", TRIG_GREET, true), new FlagType("grall", TRIG_GRALL, true),
            new FlagType("kill", TRIG_KILL, true), new FlagType("hpcnt", TRIG_HPCNT, true),
            new FlagType("random", TRIG_RANDOM, true), new FlagType("speech", TRIG_SPEECH, true),
            new FlagType("exit", TRIG_EXIT, true), new FlagType("exall", TRIG_EXALL, true),
            new FlagType("delay", TRIG_DELAY, true), new FlagType("surr", TRIG_SURR, true),
            new FlagType(null, 0, true)
        };
    }
}
