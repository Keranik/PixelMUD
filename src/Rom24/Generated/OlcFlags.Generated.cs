// Copied from rom24-master/src/tables.c flag tables used by OLC.
using static Rom24.Merc;

namespace Rom24
{
    public static partial class Tables
    {
        public static readonly FlagType[] area_flags =
        {
            new FlagType("none", AREA_NONE, false),
            new FlagType("changed", AREA_CHANGED, true),
            new FlagType("added", AREA_ADDED, true),
            new FlagType("loading", AREA_LOADING, false),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] sex_flags =
        {
            new FlagType("male", SEX_MALE, true),
            new FlagType("female", SEX_FEMALE, true),
            new FlagType("neutral", SEX_NEUTRAL, true),
            new FlagType("random", 3, true),
            new FlagType("none", SEX_NEUTRAL, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] exit_flags =
        {
            new FlagType("door", EX_ISDOOR, true),
            new FlagType("closed", EX_CLOSED, true),
            new FlagType("locked", EX_LOCKED, true),
            new FlagType("pickproof", EX_PICKPROOF, true),
            new FlagType("nopass", EX_NOPASS, true),
            new FlagType("easy", EX_EASY, true),
            new FlagType("hard", EX_HARD, true),
            new FlagType("infuriating", EX_INFURIATING, true),
            new FlagType("noclose", EX_NOCLOSE, true),
            new FlagType("nolock", EX_NOLOCK, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] room_flags =
        {
            new FlagType("dark", ROOM_DARK, true),
            new FlagType("no_mob", ROOM_NO_MOB, true),
            new FlagType("indoors", ROOM_INDOORS, true),
            new FlagType("private", ROOM_PRIVATE, true),
            new FlagType("safe", ROOM_SAFE, true),
            new FlagType("solitary", ROOM_SOLITARY, true),
            new FlagType("pet_shop", ROOM_PET_SHOP, true),
            new FlagType("no_recall", ROOM_NO_RECALL, true),
            new FlagType("imp_only", ROOM_IMP_ONLY, true),
            new FlagType("gods_only", ROOM_GODS_ONLY, true),
            new FlagType("heroes_only", ROOM_HEROES_ONLY, true),
            new FlagType("newbies_only", ROOM_NEWBIES_ONLY, true),
            new FlagType("law", ROOM_LAW, true),
            new FlagType("nowhere", ROOM_NOWHERE, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] sector_flags =
        {
            new FlagType("inside", SECT_INSIDE, true),
            new FlagType("city", SECT_CITY, true),
            new FlagType("field", SECT_FIELD, true),
            new FlagType("forest", SECT_FOREST, true),
            new FlagType("hills", SECT_HILLS, true),
            new FlagType("mountain", SECT_MOUNTAIN, true),
            new FlagType("swim", SECT_WATER_SWIM, true),
            new FlagType("noswim", SECT_WATER_NOSWIM, true),
            new FlagType("unused", SECT_UNUSED, true),
            new FlagType("air", SECT_AIR, true),
            new FlagType("desert", SECT_DESERT, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] type_flags =
        {
            new FlagType("light", ITEM_LIGHT, true),
            new FlagType("scroll", ITEM_SCROLL, true),
            new FlagType("wand", ITEM_WAND, true),
            new FlagType("staff", ITEM_STAFF, true),
            new FlagType("weapon", ITEM_WEAPON, true),
            new FlagType("treasure", ITEM_TREASURE, true),
            new FlagType("armor", ITEM_ARMOR, true),
            new FlagType("potion", ITEM_POTION, true),
            new FlagType("furniture", ITEM_FURNITURE, true),
            new FlagType("trash", ITEM_TRASH, true),
            new FlagType("container", ITEM_CONTAINER, true),
            new FlagType("drinkcontainer", ITEM_DRINK_CON, true),
            new FlagType("key", ITEM_KEY, true),
            new FlagType("food", ITEM_FOOD, true),
            new FlagType("money", ITEM_MONEY, true),
            new FlagType("boat", ITEM_BOAT, true),
            new FlagType("npccorpse", ITEM_CORPSE_NPC, true),
            new FlagType("pc corpse", ITEM_CORPSE_PC, false),
            new FlagType("fountain", ITEM_FOUNTAIN, true),
            new FlagType("pill", ITEM_PILL, true),
            new FlagType("protect", ITEM_PROTECT, true),
            new FlagType("map", ITEM_MAP, true),
            new FlagType("portal", ITEM_PORTAL, true),
            new FlagType("warpstone", ITEM_WARP_STONE, true),
            new FlagType("roomkey", ITEM_ROOM_KEY, true),
            new FlagType("gem", ITEM_GEM, true),
            new FlagType("jewelry", ITEM_JEWELRY, true),
            new FlagType("jukebox", ITEM_JUKEBOX, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] extra_flags =
        {
            new FlagType("glow", ITEM_GLOW, true),
            new FlagType("hum", ITEM_HUM, true),
            new FlagType("dark", ITEM_DARK, true),
            new FlagType("lock", ITEM_LOCK, true),
            new FlagType("evil", ITEM_EVIL, true),
            new FlagType("invis", ITEM_INVIS, true),
            new FlagType("magic", ITEM_MAGIC, true),
            new FlagType("nodrop", ITEM_NODROP, true),
            new FlagType("bless", ITEM_BLESS, true),
            new FlagType("antigood", ITEM_ANTI_GOOD, true),
            new FlagType("antievil", ITEM_ANTI_EVIL, true),
            new FlagType("antineutral", ITEM_ANTI_NEUTRAL, true),
            new FlagType("noremove", ITEM_NOREMOVE, true),
            new FlagType("inventory", ITEM_INVENTORY, true),
            new FlagType("nopurge", ITEM_NOPURGE, true),
            new FlagType("rotdeath", ITEM_ROT_DEATH, true),
            new FlagType("visdeath", ITEM_VIS_DEATH, true),
            new FlagType("nonmetal", ITEM_NONMETAL, true),
            new FlagType("meltdrop", ITEM_MELT_DROP, true),
            new FlagType("hadtimer", ITEM_HAD_TIMER, true),
            new FlagType("sellextract", ITEM_SELL_EXTRACT, true),
            new FlagType("burnproof", ITEM_BURN_PROOF, true),
            new FlagType("nouncurse", ITEM_NOUNCURSE, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] wear_flags =
        {
            new FlagType("take", ITEM_TAKE, true),
            new FlagType("finger", ITEM_WEAR_FINGER, true),
            new FlagType("neck", ITEM_WEAR_NECK, true),
            new FlagType("body", ITEM_WEAR_BODY, true),
            new FlagType("head", ITEM_WEAR_HEAD, true),
            new FlagType("legs", ITEM_WEAR_LEGS, true),
            new FlagType("feet", ITEM_WEAR_FEET, true),
            new FlagType("hands", ITEM_WEAR_HANDS, true),
            new FlagType("arms", ITEM_WEAR_ARMS, true),
            new FlagType("shield", ITEM_WEAR_SHIELD, true),
            new FlagType("about", ITEM_WEAR_ABOUT, true),
            new FlagType("waist", ITEM_WEAR_WAIST, true),
            new FlagType("wrist", ITEM_WEAR_WRIST, true),
            new FlagType("wield", ITEM_WIELD, true),
            new FlagType("hold", ITEM_HOLD, true),
            new FlagType("nosac", ITEM_NO_SAC, true),
            new FlagType("wearfloat", ITEM_WEAR_FLOAT, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] apply_flags =
        {
            new FlagType("none", APPLY_NONE, true),
            new FlagType("strength", APPLY_STR, true),
            new FlagType("dexterity", APPLY_DEX, true),
            new FlagType("intelligence", APPLY_INT, true),
            new FlagType("wisdom", APPLY_WIS, true),
            new FlagType("constitution", APPLY_CON, true),
            new FlagType("sex", APPLY_SEX, true),
            new FlagType("class", APPLY_CLASS, true),
            new FlagType("level", APPLY_LEVEL, true),
            new FlagType("age", APPLY_AGE, true),
            new FlagType("height", APPLY_HEIGHT, true),
            new FlagType("weight", APPLY_WEIGHT, true),
            new FlagType("mana", APPLY_MANA, true),
            new FlagType("hp", APPLY_HIT, true),
            new FlagType("move", APPLY_MOVE, true),
            new FlagType("gold", APPLY_GOLD, true),
            new FlagType("experience", APPLY_EXP, true),
            new FlagType("ac", APPLY_AC, true),
            new FlagType("hitroll", APPLY_HITROLL, true),
            new FlagType("damroll", APPLY_DAMROLL, true),
            new FlagType("saves", APPLY_SAVES, true),
            new FlagType("savingpara", APPLY_SAVING_PARA, true),
            new FlagType("savingrod", APPLY_SAVING_ROD, true),
            new FlagType("savingpetri", APPLY_SAVING_PETRI, true),
            new FlagType("savingbreath", APPLY_SAVING_BREATH, true),
            new FlagType("savingspell", APPLY_SAVING_SPELL, true),
            new FlagType("spellaffect", APPLY_SPELL_AFFECT, false),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] container_flags =
        {
            new FlagType("closeable", 1, true),
            new FlagType("pickproof", 2, true),
            new FlagType("closed", 4, true),
            new FlagType("locked", 8, true),
            new FlagType("puton", 16, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] ac_type =
        {
            new FlagType("pierce", AC_PIERCE, true),
            new FlagType("bash", AC_BASH, true),
            new FlagType("slash", AC_SLASH, true),
            new FlagType("exotic", AC_EXOTIC, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] size_flags =
        {
            new FlagType("tiny", SIZE_TINY, true),
            new FlagType("small", SIZE_SMALL, true),
            new FlagType("medium", SIZE_MEDIUM, true),
            new FlagType("large", SIZE_LARGE, true),
            new FlagType("huge", SIZE_HUGE, true),
            new FlagType("giant", SIZE_GIANT, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] weapon_class =
        {
            new FlagType("exotic", WEAPON_EXOTIC, true),
            new FlagType("sword", WEAPON_SWORD, true),
            new FlagType("dagger", WEAPON_DAGGER, true),
            new FlagType("spear", WEAPON_SPEAR, true),
            new FlagType("mace", WEAPON_MACE, true),
            new FlagType("axe", WEAPON_AXE, true),
            new FlagType("flail", WEAPON_FLAIL, true),
            new FlagType("whip", WEAPON_WHIP, true),
            new FlagType("polearm", WEAPON_POLEARM, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] weapon_type2 =
        {
            new FlagType("flaming", WEAPON_FLAMING, true),
            new FlagType("frost", WEAPON_FROST, true),
            new FlagType("vampiric", WEAPON_VAMPIRIC, true),
            new FlagType("sharp", WEAPON_SHARP, true),
            new FlagType("vorpal", WEAPON_VORPAL, true),
            new FlagType("twohands", WEAPON_TWO_HANDS, true),
            new FlagType("shocking", WEAPON_SHOCKING, true),
            new FlagType("poison", WEAPON_POISON, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] res_flags =
        {
            new FlagType("summon", RES_SUMMON, true),
            new FlagType("charm", RES_CHARM, true),
            new FlagType("magic", RES_MAGIC, true),
            new FlagType("weapon", RES_WEAPON, true),
            new FlagType("bash", RES_BASH, true),
            new FlagType("pierce", RES_PIERCE, true),
            new FlagType("slash", RES_SLASH, true),
            new FlagType("fire", RES_FIRE, true),
            new FlagType("cold", RES_COLD, true),
            new FlagType("lightning", RES_LIGHTNING, true),
            new FlagType("acid", RES_ACID, true),
            new FlagType("poison", RES_POISON, true),
            new FlagType("negative", RES_NEGATIVE, true),
            new FlagType("holy", RES_HOLY, true),
            new FlagType("energy", RES_ENERGY, true),
            new FlagType("mental", RES_MENTAL, true),
            new FlagType("disease", RES_DISEASE, true),
            new FlagType("drowning", RES_DROWNING, true),
            new FlagType("light", RES_LIGHT, true),
            new FlagType("sound", RES_SOUND, true),
            new FlagType("wood", RES_WOOD, true),
            new FlagType("silver", RES_SILVER, true),
            new FlagType("iron", RES_IRON, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] vuln_flags =
        {
            new FlagType("summon", VULN_SUMMON, true),
            new FlagType("charm", VULN_CHARM, true),
            new FlagType("magic", VULN_MAGIC, true),
            new FlagType("weapon", VULN_WEAPON, true),
            new FlagType("bash", VULN_BASH, true),
            new FlagType("pierce", VULN_PIERCE, true),
            new FlagType("slash", VULN_SLASH, true),
            new FlagType("fire", VULN_FIRE, true),
            new FlagType("cold", VULN_COLD, true),
            new FlagType("lightning", VULN_LIGHTNING, true),
            new FlagType("acid", VULN_ACID, true),
            new FlagType("poison", VULN_POISON, true),
            new FlagType("negative", VULN_NEGATIVE, true),
            new FlagType("holy", VULN_HOLY, true),
            new FlagType("energy", VULN_ENERGY, true),
            new FlagType("mental", VULN_MENTAL, true),
            new FlagType("disease", VULN_DISEASE, true),
            new FlagType("drowning", VULN_DROWNING, true),
            new FlagType("light", VULN_LIGHT, true),
            new FlagType("sound", VULN_SOUND, true),
            new FlagType("wood", VULN_WOOD, true),
            new FlagType("silver", VULN_SILVER, true),
            new FlagType("iron", VULN_IRON, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] position_flags =
        {
            new FlagType("dead", POS_DEAD, false),
            new FlagType("mortal", POS_MORTAL, false),
            new FlagType("incap", POS_INCAP, false),
            new FlagType("stunned", POS_STUNNED, false),
            new FlagType("sleeping", POS_SLEEPING, true),
            new FlagType("resting", POS_RESTING, true),
            new FlagType("sitting", POS_SITTING, true),
            new FlagType("fighting", POS_FIGHTING, false),
            new FlagType("standing", POS_STANDING, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] portal_flags =
        {
            new FlagType("normal_exit", GATE_NORMAL_EXIT, true),
            new FlagType("no_curse", GATE_NOCURSE, true),
            new FlagType("go_with", GATE_GOWITH, true),
            new FlagType("buggy", GATE_BUGGY, true),
            new FlagType("random", GATE_RANDOM, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] furniture_flags =
        {
            new FlagType("stand_at", STAND_AT, true),
            new FlagType("stand_on", STAND_ON, true),
            new FlagType("stand_in", STAND_IN, true),
            new FlagType("sit_at", SIT_AT, true),
            new FlagType("sit_on", SIT_ON, true),
            new FlagType("sit_in", SIT_IN, true),
            new FlagType("rest_at", REST_AT, true),
            new FlagType("rest_on", REST_ON, true),
            new FlagType("rest_in", REST_IN, true),
            new FlagType("sleep_at", SLEEP_AT, true),
            new FlagType("sleep_on", SLEEP_ON, true),
            new FlagType("sleep_in", SLEEP_IN, true),
            new FlagType("put_at", PUT_AT, true),
            new FlagType("put_on", PUT_ON, true),
            new FlagType("put_in", PUT_IN, true),
            new FlagType("put_inside", PUT_INSIDE, true),
            new FlagType(null, 0, false)
        };

        public static readonly FlagType[] apply_types =
        {
            new FlagType("affects", TO_AFFECTS, true),
            new FlagType("object", TO_OBJECT, true),
            new FlagType("immune", TO_IMMUNE, true),
            new FlagType("resist", TO_RESIST, true),
            new FlagType("vuln", TO_VULN, true),
            new FlagType("weapon", TO_WEAPON, true),
            new FlagType(null, 0, true)
        };

        public static readonly BitType[] bitvector_type =
        {
            new BitType(affect_flags, "affect"),
            new BitType(apply_flags, "apply"),
            new BitType(imm_flags, "imm"),
            new BitType(res_flags, "res"),
            new BitType(vuln_flags, "vuln"),
            new BitType(weapon_type2, "weapon")
        };
    }
}
