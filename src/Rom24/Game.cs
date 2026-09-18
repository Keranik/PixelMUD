using static Rom24.Merc;

namespace Rom24
{
    /// <summary>Global game state — the C file-scope externs.</summary>
    public static class Game
    {
        public static DescriptorData descriptor_list;
        public static CharData char_list;
        public static ObjData object_list;
        public static AreaData area_first;
        public static AreaData area_last;
        public static AreaData current_area;
        public static HelpData help_first, help_last;
        public static HelpArea had_list;
        public static ShopData shop_first, shop_last;
        public static MprogCode mprog_list;
        public static string help_greeting = "";

        public static readonly MobIndexData[] mob_index_hash = new MobIndexData[MAX_KEY_HASH];
        public static readonly ObjIndexData[] obj_index_hash = new ObjIndexData[MAX_KEY_HASH];
        public static readonly RoomIndexData[] room_index_hash = new RoomIndexData[MAX_KEY_HASH];

        public static TimeInfoData time_info = new TimeInfoData();
        public static WeatherData weather_info = new WeatherData();
        public static readonly KillData[] kill_table = InitKillTable();
        public static readonly SocialType[] social_table = InitSocialTable();
        public static int social_count;

        static KillData[] InitKillTable()
        {
            var t = new KillData[MAX_LEVEL];
            for (int i = 0; i < t.Length; i++) t[i] = new KillData();
            return t;
        }

        static SocialType[] InitSocialTable()
        {
            var t = new SocialType[MAX_SOCIALS];
            for (int i = 0; i < t.Length; i++) t[i] = new SocialType();
            return t;
        }

        public static long current_time;
        public static bool fBootDb;
        public static bool merc_down;
        public static bool fCopyOver;
        public static bool wizlock, newlock, fLogAll, MOBtrigger = true;
        public static BanData ban_list;
        public static int newmobs, newobjs;
        public static int top_mob_index, top_obj_index, top_affect, top_ed, top_exit, top_help, top_area, top_room, top_reset, top_shop;
        public static int nAllocString, sAllocString, nAllocPerm, sAllocPerm;
        public static int top_vnum_room, top_vnum_mob, top_vnum_obj;
        public static int mobile_count;
        public static long last_pc_id, last_mob_id;

        public static int port = 4000;
        public static int mud_ansiprompt = 1, mud_ansicolor = 1, mud_telnetga = 1;
        public static string mud_ipaddress = "0.0.0.0";

        public static string area_dir;
        public static string player_dir;
        public static string strArea = "";
        public static int fpAreaLine;

        public static string bug_buf = "";
        public static string log_buf = "";
        public static string str_boot_time = "";
        public static int max_on;
    }
}
