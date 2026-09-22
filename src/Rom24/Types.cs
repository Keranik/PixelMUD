using static Rom24.Merc;

namespace Rom24
{
    public delegate void DoFun(CharData ch, string argument);
    public delegate void SpellFun(int sn, int level, CharData ch, object vo, int target);
    public delegate bool SpecFun(CharData ch);

    public class FlagType
    {
        public string name;
        public long bit;
        public bool settable;
        public FlagType(string name, long bit, bool settable)
        {
            this.name = name;
            this.bit = bit;
            this.settable = settable;
        }
    }

    public class BitType
    {
        public FlagType[] table;
        public string help;
        public BitType(FlagType[] table, string help)
        {
            this.table = table;
            this.help = help;
        }
    }

    /* C char** for OLC string_edit / string_append. */
    public class StringPtr
    {
        readonly Func<string> _get;
        readonly Action<string> _set;
        public StringPtr(Func<string> get, Action<string> set)
        {
            _get = get;
            _set = set;
        }
        public string Value
        {
            get => _get() ?? "";
            set => _set(value ?? "");
        }
    }

    public class SpecType
    {
        public string name;
        public SpecFun function;
        public SpecType(string name, SpecFun function)
        {
            this.name = name;
            this.function = function;
        }
    }

    public class WiznetType
    {
        public string name;
        public long flag;
        public int level;
        public WiznetType(string name, long flag, int level)
        {
            this.name = name;
            this.flag = flag;
            this.level = level;
        }
    }

    public class ItemType
    {
        public int type;
        public string name;
        public ItemType(int type, string name) { this.type = type; this.name = name; }
    }

    public class AttackType
    {
        public string name;
        public string noun;
        public int damage;
        public AttackType(string name, string noun, int damage)
        {
            this.name = name; this.noun = noun; this.damage = damage;
        }
    }

    public class WeaponType
    {
        public string name;
        public int vnum;
        public int type;
        public string gsnName;
        public WeaponType(string name, int vnum, int type, string gsnName)
        {
            this.name = name; this.vnum = vnum; this.type = type; this.gsnName = gsnName;
        }
    }

    public class LiqType
    {
        public string liq_name;
        public string liq_color;
        public short[] liq_affect;
        public LiqType(string name, string color, short[] affect)
        {
            liq_name = name; liq_color = color; liq_affect = affect;
        }
    }

    public class RaceType
    {
        public string name;
        public bool pc_race;
        public long act, aff, off, imm, res, vuln, form, parts;
        public RaceType(string name, bool pc_race, long act, long aff, long off,
            long imm, long res, long vuln, long form, long parts)
        {
            this.name = name; this.pc_race = pc_race;
            this.act = act; this.aff = aff; this.off = off;
            this.imm = imm; this.res = res; this.vuln = vuln;
            this.form = form; this.parts = parts;
        }
    }

    public class ClanType
    {
        public string name, who_name;
        public int hall;
        public bool independent;
        public ClanType(string name, string who_name, int hall, bool independent)
        {
            this.name = name; this.who_name = who_name; this.hall = hall; this.independent = independent;
        }
    }

    public class PositionType
    {
        public string name, short_name;
        public PositionType(string name, string short_name) { this.name = name; this.short_name = short_name; }
    }

    public class SexType
    {
        public string name;
        public SexType(string name) { this.name = name; }
        public SexType(string name, string ignored) { this.name = name; }
    }

    public class SizeType
    {
        public string name;
        public SizeType(string name) { this.name = name; }
        public SizeType(string name, string ignored) { this.name = name; }
    }

    public class ClassType
    {
        public string name;
        public string who_name;
        public int attr_prime;
        public int weapon;
        public int[] guild;
        public int skill_adept;
        public int thac0_00, thac0_32, hp_min, hp_max;
        public bool fMana;
        public string base_group, default_group;
        public ClassType(string name, string who_name, int attr_prime, int weapon,
            int[] guild, int skill_adept, int thac0_00, int thac0_32, int hp_min, int hp_max,
            bool fMana, string base_group, string default_group)
        {
            this.name = name; this.who_name = who_name; this.attr_prime = attr_prime;
            this.weapon = weapon; this.guild = guild; this.skill_adept = skill_adept;
            this.thac0_00 = thac0_00; this.thac0_32 = thac0_32;
            this.hp_min = hp_min; this.hp_max = hp_max; this.fMana = fMana;
            this.base_group = base_group; this.default_group = default_group;
        }
    }

    public class PcRaceType
    {
        public string name, who_name;
        public int points;
        public int[] class_mult;
        public string[] skills;
        public int[] stats, max_stats;
        public int size;
        public PcRaceType(string name, string who_name, int points, int[] class_mult,
            string[] skills, int[] stats, int[] max_stats, int size)
        {
            this.name = name; this.who_name = who_name; this.points = points;
            this.class_mult = class_mult; this.skills = skills;
            this.stats = stats; this.max_stats = max_stats; this.size = size;
        }
    }

    public class StrAppType { public int tohit, todam, carry, wield; public StrAppType(int tohit, int todam, int carry, int wield) { this.tohit = tohit; this.todam = todam; this.carry = carry; this.wield = wield; } }
    public class DexAppType { public int defensive; public DexAppType(int defensive) { this.defensive = defensive; } }
    public class ConAppType { public int hitp, shock; public ConAppType(int hitp, int shock) { this.hitp = hitp; this.shock = shock; } }
    public class WisAppType { public int practice; public WisAppType(int practice) { this.practice = practice; } }
    public class KillData { public int number, killed; }

    /* memory for mobs — merc.h struct mem_data */
    public class MemData
    {
        public MemData next;
        public bool valid = true;
        public int id;
        public int reaction;
        public long when;
    }
    public class GenData
    {
        public GenData next;
        public bool valid = true;
        public bool[] skill_chosen = new bool[MAX_SKILL];
        public bool[] group_chosen = new bool[MAX_GROUP];
        public int points_chosen;
    }
    public class SocialType
    {
        public string name = "";
        public string char_no_arg, others_no_arg, char_found, others_found, vict_found, char_not_found, char_auto, others_auto;
    }

    public class DescriptorData
    {
        public DescriptorData next;
        public DescriptorData snoop_by;
        public CharData character;
        public CharData original;
        public bool valid = true;
        public bool ansi;
        public string host = "";
        public System.Net.Sockets.Socket socket;
        public int descriptor;
        public int connected;
        public bool fcommand;
        public string inbuf = "";
        public string incomm = "";
        public string inlast = "";
        public int repeat;
        public System.Text.StringBuilder outbuf = new System.Text.StringBuilder();
        public string showstr_head;
        public string showstr_point;
        public int outsize;
        public int outtop;
        public int editor;
        public object pEdit;
        public StringPtr pString;
        public TelnetParser Telnet = new TelnetParser();
        public bool Gmcp;
        public bool GmcpOffered;
        public bool GmcpDoSent;
        public readonly bool[] TelnetAnsweredWill = new bool[256];
        public readonly bool[] TelnetAnsweredDo = new bool[256];
        public GmcpModules GmcpMods = new GmcpModules();
        public string GmcpClient = "";
        public string GmcpVersion = "";
        public int GmcpLastLevel = int.MinValue;
        public int GmcpLastPos = int.MinValue;
        public int GmcpLastAlign = int.MinValue;
        public int GmcpLastHunger = int.MinValue;
        public int GmcpLastThirst = int.MinValue;
        public string GmcpLastEnemy = "";
        public int GmcpLastEnemyPct = int.MinValue;
    }

    public class AffectData
    {
        public AffectData next;
        public bool valid = true;
        public int where;
        public int type;
        public int level;
        public int duration;
        public int location;
        public int modifier;
        public long bitvector;
    }

    public class ExtraDescrData
    {
        public ExtraDescrData next;
        public bool valid = true;
        public string keyword = "";
        public string description = "";
    }

    public class ExitData
    {
        public RoomIndexData to_room;
        public int vnum;
        public int exit_info;
        public int key;
        public string keyword = "";
        public string description = "";
        public ExitData next;
        public int rs_flags;
        public int orig_door;
    }

    public class ResetData
    {
        public ResetData next;
        public char command;
        public int arg1, arg2, arg3, arg4;
    }

    public class AreaData
    {
        public AreaData next;
        public HelpArea helps;
        public string file_name = "";
        public string name = "";
        public string credits = "";
        public int age;
        public int nplayer;
        public int low_range, high_range;
        public int min_vnum, max_vnum;
        public bool empty;
        public string builders = "";
        public int vnum;
        public int area_flags;
        public int security;
    }

    public class RoomIndexData
    {
        public RoomIndexData next;
        public CharData people;
        public ObjData contents;
        public ExtraDescrData extra_descr;
        public AreaData area;
        public ExitData[] exit = new ExitData[6];
        public ResetData reset_first, reset_last;
        public string name = "";
        public string description = "";
        public string owner = "";
        public int vnum;
        public long room_flags;
        public int light;
        public int sector_type;
        public int heal_rate = 100;
        public int mana_rate = 100;
        public int clan;
    }

    public class ShopData
    {
        public ShopData next;
        public int keeper;
        public int[] buy_type = new int[MAX_TRADE];
        public int profit_buy, profit_sell;
        public int open_hour, close_hour;
    }

    public class MprogList
    {
        public int trig_type;
        public string trig_phrase = "";
        public int vnum;
        public string code = "";
        public MprogList next;
        public bool valid = true;
    }

    public class MprogCode
    {
        public int vnum;
        public string code = "";
        public MprogCode next;
    }

    public class MobIndexData
    {
        public MobIndexData next;
        public SpecFun spec_fun;
        public ShopData pShop;
        public MprogList mprogs;
        public AreaData area;
        public int vnum;
        public int group;
        public bool new_format;
        public int count;
        public int killed;
        public string player_name = "";
        public string short_descr = "";
        public string long_descr = "";
        public string description = "";
        public long act;
        public long affected_by;
        public int alignment;
        public int level;
        public int hitroll;
        public int[] hit = new int[3];
        public int[] mana = new int[3];
        public int[] damage = new int[3];
        public int[] ac = new int[4];
        public int dam_type;
        public long off_flags, imm_flags, res_flags, vuln_flags;
        public int start_pos, default_pos;
        public int sex, race;
        public long wealth;
        public long form, parts;
        public int size;
        public string material = "";
        public long mprog_flags;
    }

    public class ObjIndexData
    {
        public ObjIndexData next;
        public ExtraDescrData extra_descr;
        public AffectData affected;
        public AreaData area;
        public bool new_format;
        public string name = "";
        public string short_descr = "";
        public string description = "";
        public int vnum;
        public int reset_num;
        public string material = "";
        public int item_type;
        public long extra_flags;
        public long wear_flags;
        public int level, condition, count, weight, cost;
        public int[] value = new int[5];
    }

    public class HelpData
    {
        public HelpData next;
        public HelpData next_area;
        public int level;
        public string keyword = "";
        public string text = "";
    }

    public class HelpArea
    {
        public HelpArea next;
        public HelpData first, last;
        public AreaData area;
        public string filename = "";
        public bool changed;
    }

    public class PcData
    {
        public PcData next;
        public bool valid = true;
        public string pwd = "";
        public string bamfin = "";
        public string bamfout = "";
        public string title = "";
        public int perm_hit, perm_mana, perm_move;
        public int true_sex;
        public int last_level;
        public int[] condition = new int[4];
        public int[] learned = new int[MAX_SKILL];
        public bool[] group_known = new bool[MAX_GROUP];
        public System.Text.StringBuilder buffer = new System.Text.StringBuilder();
        public int points;
        public bool confirm_delete;
        public string[] alias = new string[MAX_ALIAS];
        public string[] alias_sub = new string[MAX_ALIAS];
        public int security;
        public int[] text = new int[3];
        public int[] auction = new int[3];
        public int[] auction_text = new int[3];
        public int[] gossip = new int[3];
        public int[] gossip_text = new int[3];
        public int[] music = new int[3];
        public int[] music_text = new int[3];
        public int[] question = new int[3];
        public int[] question_text = new int[3];
        public int[] answer = new int[3];
        public int[] answer_text = new int[3];
        public int[] quote = new int[3];
        public int[] quote_text = new int[3];
        public int[] immtalk_text = new int[3];
        public int[] immtalk_type = new int[3];
        public int[] info = new int[3];
        public int[] say = new int[3];
        public int[] say_text = new int[3];
        public int[] tell = new int[3];
        public int[] tell_text = new int[3];
        public int[] reply = new int[3];
        public int[] reply_text = new int[3];
        public int[] gtell_text = new int[3];
        public int[] gtell_type = new int[3];
        public int[] wiznet = new int[3];
        public int[] room_title = new int[3];
        public int[] room_text = new int[3];
        public int[] room_exits = new int[3];
        public int[] room_things = new int[3];
        public int[] prompt = new int[3];
        public int[] fight_death = new int[3];
        public int[] fight_yhit = new int[3];
        public int[] fight_ohit = new int[3];
        public int[] fight_thit = new int[3];
        public int[] fight_skill = new int[3];
        public long[] last_note = new long[MAX_BOARD];
        public BoardData board;
        public NoteData in_progress;
        public ImcCharData imc;
    }

    public class BanData
    {
        public BanData next;
        public bool valid = true;
        public bool permanent;
        public int level;
        public long ban_flags;
        public string name = "";
    }

    public class NoteData
    {
        public NoteData next;
        public string sender, date, to_list, subject, text;
        public long date_stamp, expire;
    }

    public class BoardData
    {
        public string short_name, long_name;
        public int read_level, write_level;
        public string names;
        public int force_type, purge_days;
        public NoteData note_first;
        public bool changed;

        public BoardData(string short_name, string long_name, int read_level, int write_level,
            string names, int force_type, int purge_days)
        {
            this.short_name = short_name;
            this.long_name = long_name;
            this.read_level = read_level;
            this.write_level = write_level;
            this.names = names;
            this.force_type = force_type;
            this.purge_days = purge_days;
        }
    }

    public class CharData
    {
        public CharData next;
        public CharData next_in_room;
        public CharData master, leader, fighting, reply, pet, mprog_target;
        public MemData memory;
        public SpecFun spec_fun;
        public MobIndexData pIndexData;
        public DescriptorData desc;
        public AffectData affected;
        public ObjData carrying;
        public ObjData on;
        public RoomIndexData in_room;
        public RoomIndexData was_in_room;
        public AreaData zone;
        public PcData pcdata;
        public GenData gen_data;
        public bool valid = true;
        public string name = "";
        public long id;
        public int version;
        public string short_descr = "";
        public string long_descr = "";
        public string description = "";
        public string prompt = "";
        public string prefix = "";
        public int group, clan, sex, klass, race, level, trust;
        public int played, lines;
        public long logon;
        public int timer, wait, daze;
        public int hit, max_hit, mana, max_mana, move, max_move;
        public long gold, silver;
        public int exp;
        public long act, comm, wiznet;
        public long imm_flags, res_flags, vuln_flags;
        public int invis_level, incog_level;
        public long affected_by;
        public int position;
        public int practice, train, carry_weight, carry_number, saving_throw;
        public int alignment, hitroll, damroll;
        public int[] armor = new int[4];
        public int wimpy;
        public int[] perm_stat = new int[MAX_STATS];
        public int[] mod_stat = new int[MAX_STATS];
        public long form, parts;
        public int size;
        public string material = "";
        public long off_flags;
        public int[] damage = new int[3];
        public int dam_type, start_pos, default_pos;
        public int mprog_delay;
    }

    public class ObjData
    {
        public ObjData next;
        public ObjData next_content;
        public ObjData contains;
        public ObjData in_obj;
        public ObjData on;
        public CharData carried_by;
        public ExtraDescrData extra_descr;
        public AffectData affected;
        public ObjIndexData pIndexData;
        public RoomIndexData in_room;
        public bool valid = true;
        public bool enchanted;
        public string owner = "";
        public string name = "";
        public string short_descr = "";
        public string description = "";
        public int item_type;
        public long extra_flags;
        public long wear_flags;
        public int wear_loc;
        public int weight, cost, level, condition;
        public string material = "";
        public int timer;
        public int[] value = new int[5];
        public long GmcpId;
    }

    public class TimeInfoData
    {
        public int hour, day, month, year;
    }

    public class WeatherData
    {
        public int mmhg, change, sky, sunlight;
    }

    public class SkillType
    {
        public string name;
        public short[] skill_level;
        public short[] rating;
        public bool is_spell;
        public int slot, min_mana, beats;
        public string noun_damage;
        public string msg_off, msg_obj;
        public SpellFun spell_fun;
        public int target, minimum_position;
        public SkillType(string name, short[] skill_level, short[] rating, bool is_spell,
            int slot, int min_mana, int beats, string noun_damage,
            int target = 0, int minimum_position = 0, string msg_off = null, string msg_obj = null)
        {
            this.name = name; this.skill_level = skill_level; this.rating = rating;
            this.is_spell = is_spell; this.slot = slot; this.min_mana = min_mana;
            this.beats = beats; this.noun_damage = noun_damage;
            this.target = target; this.minimum_position = minimum_position;
            this.msg_off = msg_off; this.msg_obj = msg_obj;
        }
    }

    public class CmdType
    {
        public string name;
        public DoFun do_fun;
        public int position;
        public int level;
        public int log;
        public int show;
        public CmdType(string name, DoFun do_fun, int position, int level, int log, int show)
        {
            this.name = name; this.do_fun = do_fun; this.position = position;
            this.level = level; this.log = log; this.show = show;
        }
    }
}
