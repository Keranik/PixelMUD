using static Rom24.Merc;

namespace Rom24
{
    public static class Recycle
    {
        public static CharData new_char()
        {
            var ch = new CharData();
            ch.valid = true;
            ch.name = "";
            ch.short_descr = "";
            ch.long_descr = "";
            ch.description = "";
            ch.prompt = "";
            ch.prefix = "";
            ch.logon = Game.current_time;
            ch.lines = PAGELEN;
            for (int i = 0; i < 4; i++)
                ch.armor[i] = 100;
            ch.position = POS_STANDING;
            ch.hit = 20;
            ch.max_hit = 20;
            ch.mana = 100;
            ch.max_mana = 100;
            ch.move = 100;
            ch.max_move = 100;
            for (int i = 0; i < MAX_STATS; i++)
            {
                ch.perm_stat[i] = 13;
                ch.mod_stat[i] = 0;
            }
            return ch;
        }

        public static MemData new_mem_data()
        {
            var memory = new MemData();
            memory.next = null;
            memory.id = 0;
            memory.reaction = 0;
            memory.when = 0;
            memory.valid = true;
            return memory;
        }

        public static void free_mem_data(MemData memory)
        {
            if (memory == null || !Bit.IS_VALID(memory))
                return;
            memory.valid = false;
        }

        public static PcData new_pcdata()
        {
            var pc = new PcData();
            pc.board = Board.boards[Board.DEFAULT_BOARD];
            return pc;
        }

        public static NoteData new_note() => new NoteData();

        /* recycle.c new_obj: *obj = obj_zero (wear_loc 0), not WEAR_NONE */
        public static ObjData new_obj() => new ObjData();

        public static void free_obj(ObjData obj)
        {
            if (!Bit.IS_VALID(obj))
                return;
            obj.affected = null;
            obj.extra_descr = null;
            obj.valid = false;
        }

        public static AffectData new_affect() => new AffectData();

        public static ExtraDescrData new_extra_descr() => new ExtraDescrData();

        public static ExitData new_exit() => new ExitData();

        public static DescriptorData new_descriptor() => new DescriptorData { connected = CON_GET_NAME };

        public static ResetData new_reset_data()
        {
            Game.top_reset++;
            return new ResetData();
        }

        public static void free_reset_data(ResetData pReset)
        {
        }

        public static void free_affect(AffectData paf)
        {
        }

        public static void free_extra_descr(ExtraDescrData ed)
        {
        }

        public static void free_exit(ExitData pexit)
        {
        }

        public static AreaData new_area()
        {
            Game.top_area++;
            var pArea = new AreaData();
            pArea.name = "New area";
            pArea.area_flags = AREA_ADDED;
            pArea.security = 1;
            pArea.builders = "None";
            pArea.min_vnum = 0;
            pArea.max_vnum = 0;
            pArea.age = 0;
            pArea.nplayer = 0;
            pArea.empty = true;
            pArea.file_name = RomString.sprintf("area%d.are", pArea.vnum);
            pArea.vnum = Game.top_area - 1;
            return pArea;
        }

        public static RoomIndexData new_room_index()
        {
            Game.top_room++;
            return new RoomIndexData { heal_rate = 100, mana_rate = 100 };
        }

        public static ObjIndexData new_obj_index()
        {
            Game.top_obj_index++;
            var pObj = new ObjIndexData();
            pObj.name = "no name";
            pObj.short_descr = "(no short description)";
            pObj.description = "(no description)";
            pObj.item_type = ITEM_TRASH;
            pObj.material = "unknown";
            pObj.condition = 100;
            pObj.new_format = true;
            return pObj;
        }

        public static MobIndexData new_mob_index()
        {
            Game.top_mob_index++;
            var pMob = new MobIndexData();
            pMob.player_name = "no name";
            pMob.short_descr = "(no short description)";
            pMob.long_descr = "(no long description)\n\r";
            pMob.description = "";
            pMob.act = ACT_IS_NPC;
            pMob.race = Lookup.race_lookup("human");
            pMob.material = "unknown";
            pMob.size = SIZE_MEDIUM;
            pMob.start_pos = POS_STANDING;
            pMob.default_pos = POS_STANDING;
            pMob.new_format = true;
            return pMob;
        }

        public static MprogCode new_mpcode() => new MprogCode();

        public static HelpData new_help() => new HelpData();

        public static HelpArea new_had() => new HelpArea();

        public static long get_pc_id()
        {
            var now = Game.current_time;
            if (now <= Game.last_pc_id)
                Game.last_pc_id++;
            else
                Game.last_pc_id = now;
            return Game.last_pc_id;
        }

        public static GenData new_gen_data() => new GenData { valid = true };

        public static void free_gen_data(GenData gen)
        {
            if (gen == null || !Bit.IS_VALID(gen))
                return;
            gen.valid = false;
        }

        public static long get_mob_id() => ++Game.last_mob_id;
    }
}
