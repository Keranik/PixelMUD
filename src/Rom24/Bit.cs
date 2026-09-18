using static Rom24.Merc;

namespace Rom24
{
    /// <summary>
    /// Faithful ports of merc.h utility / character macros.
    /// SET_BIT/REMOVE_BIT take ref because the C macros assign through the argument.
    /// </summary>
    public static class Bit
    {
        public static bool IS_SET(long flag, long bit) => (flag & bit) != 0;
        public static void SET_BIT(ref long var, long bit) => var |= bit;
        public static void SET_BIT(ref int var, int bit) => var |= bit;
        public static void SET_BIT(ref int var, long bit) => var |= (int)bit;
        public static void REMOVE_BIT(ref long var, long bit) => var &= ~bit;
        public static void REMOVE_BIT(ref int var, int bit) => var &= ~bit;
        public static void REMOVE_BIT(ref int var, long bit) => var &= ~(int)bit;
        public static void TOGGLE_BIT(ref long var, long bit) => var ^= bit;
        public static void TOGGLE_BIT(ref int var, int bit) => var ^= bit;
        public static void TOGGLE_BIT(ref int var, long bit) => var ^= (int)bit;

        public static int UMIN(int a, int b) => a < b ? a : b;
        public static long UMIN(long a, long b) => a < b ? a : b;
        public static int UMAX(int a, int b) => a > b ? a : b;
        public static long UMAX(long a, long b) => a > b ? a : b;
        public static int URANGE(int a, int b, int c) => b < a ? a : (b > c ? c : b);

        public static char LOWER(char c) => (c >= 'A' && c <= 'Z') ? (char)(c + 'a' - 'A') : c;
        public static char UPPER(char c) => (c >= 'a' && c <= 'z') ? (char)(c + 'A' - 'a') : c;
        public static bool isspace(char c)
            => c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\f' || c == '\v';

        public static bool IS_NULLSTR(string str) => string.IsNullOrEmpty(str);
        public static bool IS_VALID(object data)
        {
            if (data == null) return false;
            if (data is CharData c) return c.valid;
            if (data is ObjData o) return o.valid;
            if (data is DescriptorData d) return d.valid;
            if (data is MemData m) return m.valid;
            if (data is PcData p) return p.valid;
            return true;
        }

        public static bool IS_NPC(CharData ch) => IS_SET(ch.act, ACT_IS_NPC);
        public static bool HAS_TRIGGER(CharData ch, long trig) =>
            IS_SET(ch.pIndexData.mprog_flags, trig);
        public static bool IS_IMMORTAL(CharData ch) => Handler.get_trust(ch) >= LEVEL_IMMORTAL;
        public static bool IS_HERO(CharData ch) => Handler.get_trust(ch) >= LEVEL_HERO;
        public static bool IS_TRUSTED(CharData ch, int level) => Handler.get_trust(ch) >= level;
        public static bool IS_AFFECTED(CharData ch, long sn) => IS_SET(ch.affected_by, sn);

        public static bool IS_GOOD(CharData ch) => ch.alignment >= 350;
        public static bool IS_EVIL(CharData ch) => ch.alignment <= -350;
        public static bool IS_NEUTRAL(CharData ch) => !IS_GOOD(ch) && !IS_EVIL(ch);
        public static bool IS_AWAKE(CharData ch) => ch.position > POS_SLEEPING;
        public static bool IS_SWITCHED(CharData ch) => ch.desc != null && ch.desc.original != null;
        public static bool IS_BUILDER(CharData ch, AreaData Area) =>
            !IS_NPC(ch) && !IS_SWITCHED(ch) &&
            (ch.pcdata.security >= Area.security
             || (Area.builders != null && Area.builders.IndexOf(ch.name, StringComparison.Ordinal) >= 0)
             || (Area.builders != null && Area.builders.IndexOf("All", StringComparison.Ordinal) >= 0));

        public static bool IS_OBJ_STAT(ObjData obj, long stat) => IS_SET(obj.extra_flags, stat);
        public static bool IS_WEAPON_STAT(ObjData obj, long stat) => IS_SET(obj.value[4], stat);
        public static bool CAN_WEAR(ObjData obj, long part) => IS_SET(obj.wear_flags, part);
        public static bool IS_OUTSIDE(CharData ch) => !IS_SET(ch.in_room.room_flags, ROOM_INDOORS);
        public static int WEIGHT_MULT(ObjData obj) => obj.item_type == ITEM_CONTAINER ? obj.value[4] : 100;

        public static void WAIT_STATE(CharData ch, int npulse) => ch.wait = UMAX(ch.wait, npulse);
        public static void DAZE_STATE(CharData ch, int npulse) => ch.daze = UMAX(ch.daze, npulse);
    }
}
