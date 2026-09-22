using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Rom24
{
    /// <summary>
    /// Which GMCP packages this connection wants. A name with no dot enables
    /// itself and every package under it. A longer name enables only itself.
    /// Core is always on. Until the client sends Supports, Char, Room, and
    /// Comm are on so a client that only answered DO still receives data.
    /// </summary>
    public sealed class GmcpModules
    {
        readonly Dictionary<string, int> _mods = new(StringComparer.OrdinalIgnoreCase);
        public bool Declared { get; private set; }

        public void Set(IEnumerable<string> entries)
        {
            _mods.Clear();
            Declared = true;
            Add(entries);
            Declared = true;
        }

        public void Add(IEnumerable<string> entries)
        {
            if (entries == null)
                return;
            Declared = true;
            foreach (var raw in entries)
            {
                if (string.IsNullOrWhiteSpace(raw))
                    continue;
                var parts = raw.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                var name = parts[0];
                int ver = 1;
                if (parts.Length > 1)
                    int.TryParse(parts[1], out ver);
                if (ver < 1)
                    ver = 1;
                _mods[name] = ver;
            }
        }

        public void Remove(IEnumerable<string> entries)
        {
            if (entries == null)
                return;
            Declared = true;
            foreach (var raw in entries)
            {
                if (string.IsNullOrWhiteSpace(raw))
                    continue;
                _mods.Remove(raw.Trim().Split(' ')[0]);
            }
        }

        public void Clear()
        {
            _mods.Clear();
            Declared = false;
        }

        public bool Allows(string package)
        {
            if (string.IsNullOrEmpty(package))
                return false;
            if (package.Equals("Core", StringComparison.OrdinalIgnoreCase)
                || package.StartsWith("Core.", StringComparison.OrdinalIgnoreCase))
                return true;
            if (!Declared)
                return Family(package, "Char") || Family(package, "Room") || Family(package, "Comm");
            if (_mods.ContainsKey(package))
                return true;
            var top = package.Split('.')[0];
            return _mods.ContainsKey(top);
        }

        static bool Family(string package, string name)
            => package.Equals(name, StringComparison.OrdinalIgnoreCase)
               || package.StartsWith(name + ".", StringComparison.OrdinalIgnoreCase);
    }

    public static class Gmcp
    {
        static readonly string[] DirKeys = { "n", "e", "s", "w", "u", "d" };
        static readonly string[] Sectors =
        {
            "Inside", "City", "Field", "Forest", "Hills", "Mountain",
            "Swim", "Noswim", "Unused", "Air", "Desert"
        };
        static readonly string[] WearNames =
        {
            "light", "finger", "finger", "neck", "neck", "body", "head", "legs",
            "feet", "hands", "arms", "shield", "about", "waist", "wrist", "wrist",
            "wield", "hold", "float"
        };

        static long _nextId = 1;
        static readonly HashSet<string> _parseLogged = new(StringComparer.OrdinalIgnoreCase);

        public static long AllocId()
        {
            var id = _nextId++;
            if (_nextId <= 0)
                _nextId = 1;
            return id;
        }

        public static void NoteId(long id)
        {
            if (id >= _nextId)
                _nextId = id + 1;
        }

        public static void Offer(DescriptorData d)
        {
            if (d == null || d.GmcpOffered)
                return;
            d.GmcpOffered = true;
            SendBytes(d, new byte[] { TelnetParser.IAC, TelnetParser.WILL, TelnetParser.GMCP });
        }

        public static void OnNegotiate(DescriptorData d, byte command, byte option)
        {
            if (d == null)
                return;
            if (option == 1)
                return;

            if (option == TelnetParser.GMCP)
            {
                if (command == TelnetParser.DO)
                    d.Gmcp = true;
                else if (command == TelnetParser.DONT)
                {
                    d.Gmcp = false;
                    d.GmcpMods.Clear();
                }
                else if (command == TelnetParser.WILL)
                {
                    if (!d.GmcpDoSent)
                    {
                        d.GmcpDoSent = true;
                        SendBytes(d, new byte[] { TelnetParser.IAC, TelnetParser.DO, TelnetParser.GMCP });
                    }
                }
                return;
            }

            if (command == TelnetParser.WILL || command == TelnetParser.WONT)
            {
                if (d.TelnetAnsweredWill[option])
                    return;
                d.TelnetAnsweredWill[option] = true;
                if (command == TelnetParser.WILL)
                    SendBytes(d, new byte[] { TelnetParser.IAC, TelnetParser.DONT, option });
            }
            else if (command == TelnetParser.DO || command == TelnetParser.DONT)
            {
                if (d.TelnetAnsweredDo[option])
                    return;
                d.TelnetAnsweredDo[option] = true;
                if (command == TelnetParser.DO)
                    SendBytes(d, new byte[] { TelnetParser.IAC, TelnetParser.WONT, option });
            }
        }

        public static void OnFrame(DescriptorData d, byte[] payload)
        {
            if (d == null || payload == null || payload.Length == 0)
                return;
            string text;
            try { text = Encoding.UTF8.GetString(payload); }
            catch { return; }
            text = text.Trim();
            if (text.Length == 0)
                return;

            string package;
            string body;
            int sp = text.IndexOf(' ');
            if (sp < 0)
            {
                package = text;
                body = "";
            }
            else
            {
                package = text.Substring(0, sp);
                body = text.Substring(sp + 1).Trim();
            }

            try
            {
                Dispatch(d, package, body);
            }
            catch (JsonException)
            {
                if (_parseLogged.Add(package))
                    Db.log_f("GMCP: bad JSON in %s", package);
            }
            catch (Exception ex)
            {
                if (_parseLogged.Add(package))
                    Db.log_f("GMCP: %s failed: %s", package, ex.Message);
            }
        }

        static void Dispatch(DescriptorData d, string package, string body)
        {
            if (Eq(package, "Core.Hello"))
            {
                Hello(d, body);
                return;
            }
            if (Eq(package, "Core.Supports.Set"))
            {
                d.GmcpMods.Set(ReadStringArray(body));
                if (Playing(d))
                    SendLogin(d);
                return;
            }
            if (Eq(package, "Core.Supports.Add"))
            {
                d.GmcpMods.Add(ReadStringArray(body));
                if (Playing(d))
                    SendLogin(d);
                return;
            }
            if (Eq(package, "Core.Supports.Remove"))
            {
                d.GmcpMods.Remove(ReadStringArray(body));
                return;
            }
            if (Eq(package, "Core.Ping"))
            {
                Send(d, "Core.Ping", null);
                return;
            }
            if (Eq(package, "Core.KeepAlive"))
            {
                if (d.character != null)
                    d.character.timer = 0;
                return;
            }
            if (!Playing(d))
                return;
            var ch = d.character;
            if (Eq(package, "Char.Items.Inv"))
                SendItems(d, "inv", InvItems(ch));
            else if (Eq(package, "Char.Items.Room"))
                SendItems(d, "room", RoomItems(ch));
            else if (Eq(package, "Char.Items.Contents"))
                SendContents(d, ch, JsonString(body));
            else if (Eq(package, "Char.Skills.Groups") || Eq(package, "Char.Skills.Get"))
                Send(d, "Char.Skills.Groups", JsonSerializer.Serialize(SkillsDoc(ch, null)));
            else if (Eq(package, "Char.Skills.List"))
                Send(d, "Char.Skills.List", JsonSerializer.Serialize(SkillsDoc(ch, JsonString(body))));
            else if (Eq(package, "Char.Skills.Info"))
                Send(d, "Char.Skills.Info", JsonSerializer.Serialize(SkillInfo(ch, JsonString(body))));
        }

        static void Hello(DescriptorData d, string body)
        {
            if (body.Length == 0)
                return;
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                return;
            foreach (var p in doc.RootElement.EnumerateObject())
            {
                if (p.Name.Equals("client", StringComparison.OrdinalIgnoreCase))
                    d.GmcpClient = p.Value.ToString();
                else if (p.Name.Equals("version", StringComparison.OrdinalIgnoreCase))
                    d.GmcpVersion = p.Value.ToString();
            }
        }

        public static void Goodbye(DescriptorData d)
        {
            if (d == null || !d.Gmcp)
                return;
            try { Send(d, "Core.Goodbye", "\"Goodbye\""); }
            catch { }
        }

        public static void SendLogin(DescriptorData d)
        {
            if (!Playing(d))
                return;
            var ch = d.character;
            Send(d, "Char.StatusVars", StatusVars());
            Name(ch);
            Status(ch, true);
            Vitals(ch);
            Worth(ch);
            Stats(ch);
            Affects(ch);
            RoomInfo(ch);
            Players(ch);
            SendItems(d, "inv", InvItems(ch));
            SendItems(d, "eq", EqItems(ch));
            SendItems(d, "room", RoomItems(ch));
            Send(d, "Char.Skills.Groups", JsonSerializer.Serialize(SkillsDoc(ch, null)));
        }

        public static void OnPrompt(CharData ch)
        {
            if (!Playing(ch))
                return;
            Vitals(ch);
            Status(ch, false);
        }

        public static void Vitals(CharData ch)
        {
            if (!Playing(ch))
                return;
            Send(ch.desc, "Char.Vitals", JsonSerializer.Serialize(new Dictionary<string, int>
            {
                ["hp"] = ch.hit,
                ["maxhp"] = ch.max_hit,
                ["mana"] = ch.mana,
                ["maxmana"] = ch.max_mana,
                ["move"] = ch.move,
                ["maxmove"] = ch.max_move
            }));
        }

        public static void Name(CharData ch)
        {
            if (!Playing(ch))
                return;
            string title = ch.pcdata?.title ?? "";
            Send(ch.desc, "Char.Name", JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["name"] = ch.name ?? "",
                ["fullname"] = ((ch.name ?? "") + title).Trim()
            }));
        }

        public static void Worth(CharData ch)
        {
            if (!Playing(ch) || Bit.IS_NPC(ch))
                return;
            Send(ch.desc, "Char.Worth", JsonSerializer.Serialize(new Dictionary<string, long>
            {
                ["gold"] = ch.gold,
                ["silver"] = ch.silver,
                ["exp"] = ch.exp,
                ["trains"] = ch.train,
                ["pracs"] = ch.practice
            }));
        }

        public static void Stats(CharData ch)
        {
            if (!Playing(ch) || Bit.IS_NPC(ch))
                return;
            Send(ch.desc, "Char.Stats", JsonSerializer.Serialize(new Dictionary<string, int>
            {
                ["str"] = Handler.get_curr_stat(ch, Merc.STAT_STR),
                ["int"] = Handler.get_curr_stat(ch, Merc.STAT_INT),
                ["wis"] = Handler.get_curr_stat(ch, Merc.STAT_WIS),
                ["dex"] = Handler.get_curr_stat(ch, Merc.STAT_DEX),
                ["con"] = Handler.get_curr_stat(ch, Merc.STAT_CON)
            }));
        }

        public static void Status(CharData ch, bool force)
        {
            if (!Playing(ch) || Bit.IS_NPC(ch))
                return;
            var d = ch.desc;
            int hunger = ch.pcdata != null ? ch.pcdata.condition[Merc.COND_HUNGER] : 0;
            int thirst = ch.pcdata != null ? ch.pcdata.condition[Merc.COND_THIRST] : 0;
            string enemy = null;
            int epct = 0;
            if (ch.fighting != null)
            {
                enemy = Bit.IS_NPC(ch.fighting) ? ch.fighting.short_descr : ch.fighting.name;
                epct = ch.fighting.max_hit > 0 ? ch.fighting.hit * 100 / ch.fighting.max_hit : 0;
            }
            if (!force
                && d.GmcpLastLevel == ch.level
                && d.GmcpLastPos == ch.position
                && d.GmcpLastAlign == ch.alignment
                && d.GmcpLastHunger == hunger
                && d.GmcpLastThirst == thirst
                && d.GmcpLastEnemy == (enemy ?? "")
                && d.GmcpLastEnemyPct == epct)
                return;
            d.GmcpLastLevel = ch.level;
            d.GmcpLastPos = ch.position;
            d.GmcpLastAlign = ch.alignment;
            d.GmcpLastHunger = hunger;
            d.GmcpLastThirst = thirst;
            d.GmcpLastEnemy = enemy ?? "";
            d.GmcpLastEnemyPct = epct;

            var doc = new Dictionary<string, object>
            {
                ["level"] = ch.level,
                ["race"] = RaceName(ch),
                ["class"] = ClassName(ch),
                ["position"] = PositionName(ch.position),
                ["align"] = ch.alignment,
                ["hunger"] = hunger,
                ["thirst"] = thirst
            };
            if (enemy != null)
            {
                doc["enemy"] = enemy;
                doc["enemypct"] = epct;
            }
            Send(d, "Char.Status", JsonSerializer.Serialize(doc));
        }

        public static void Affects(CharData ch)
        {
            if (!Playing(ch))
                return;
            var list = new List<Dictionary<string, object>>();
            for (var af = ch.affected; af != null; af = af.next)
            {
                string name = "unknown";
                if (af.type >= 0 && af.type < Tables.skill_table.Length
                    && Tables.skill_table[af.type].name != null)
                    name = Tables.skill_table[af.type].name;
                list.Add(new Dictionary<string, object>
                {
                    ["name"] = name,
                    ["duration"] = af.duration
                });
            }
            Send(ch.desc, "Char.Affects", JsonSerializer.Serialize(new Dictionary<string, object>
            {
                ["affects"] = list
            }));
        }

        public static void RoomInfo(CharData ch)
        {
            if (!Playing(ch) || ch.in_room == null)
                return;
            var room = ch.in_room;
            bool hidden = CantSeeRoom(ch, room);
            string name = hidden ? "It is pitch black ..." : OneLine(room.name);
            var exits = hidden ? new Dictionary<string, int>() : ExitMap(ch);
            int sector = Bit.URANGE(0, room.sector_type, Merc.SECT_MAX - 1);
            string env = sector >= 0 && sector < Sectors.Length ? Sectors[sector] : "Inside";
            string area = room.area?.name ?? "";
            Send(ch.desc, "Room.Info", JsonSerializer.Serialize(new Dictionary<string, object>
            {
                ["num"] = room.vnum,
                ["name"] = name,
                ["area"] = OneLine(area),
                ["environment"] = env,
                ["exits"] = exits
            }));
        }

        public static void WrongDir(CharData ch, int door)
        {
            if (!Playing(ch) || door < 0 || door >= DirKeys.Length)
                return;
            Send(ch.desc, "Room.WrongDir", JsonSerializer.Serialize(DirKeys[door]));
        }

        public static void Players(CharData ch)
        {
            if (!Playing(ch) || ch.in_room == null)
                return;
            Send(ch.desc, "Room.Players", JsonSerializer.Serialize(PlayerList(ch, ch.in_room)));
        }

        public static void AddPlayer(CharData ch)
        {
            if (ch?.in_room == null || Bit.IS_NPC(ch))
                return;
            var one = new List<Dictionary<string, string>> { PlayerObj(ch) };
            string json = JsonSerializer.Serialize(one);
            for (var p = ch.in_room.people; p != null; p = p.next_in_room)
            {
                if (p == ch || !Playing(p) || !Handler.can_see(p, ch))
                    continue;
                Send(p.desc, "Room.AddPlayer", json);
            }
        }

        public static void RemovePlayer(CharData ch)
        {
            if (ch?.in_room == null || Bit.IS_NPC(ch))
                return;
            string json = JsonSerializer.Serialize(ch.name ?? "");
            for (var p = ch.in_room.people; p != null; p = p.next_in_room)
            {
                if (p == ch || !Playing(p))
                    continue;
                Send(p.desc, "Room.RemovePlayer", json);
            }
        }

        public static void Channel(DescriptorData d, string chan, string player, string msg)
        {
            if (d == null)
                return;
            Send(d, "Comm.Channel", JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["chan"] = chan ?? "",
                ["player"] = player ?? "",
                ["msg"] = Plain(msg)
            }));
        }

        public static void ItemAdd(CharData ch, ObjData obj, string location)
            => ItemDelta(ch, "Add", location, obj);

        public static void ItemRemove(CharData ch, ObjData obj, string location)
            => ItemDelta(ch, "Remove", location, obj);

        public static void ItemRoomAdd(RoomIndexData room, ObjData obj)
            => ItemRoomDelta(room, "Add", obj);

        public static void ItemRoomRemove(RoomIndexData room, ObjData obj)
            => ItemRoomDelta(room, "Remove", obj);

        /// <summary>
        /// Same id and location, new fields. Used when charges or hours change
        /// and the object is not moved.
        /// </summary>
        public static void ItemUpdate(ObjData obj)
        {
            if (obj == null)
                return;
            if (obj.carried_by != null)
            {
                string location = obj.wear_loc != Merc.WEAR_NONE ? "eq" : "inv";
                ItemDelta(obj.carried_by, "Update", location, obj);
                return;
            }
            if (obj.in_room != null)
                ItemRoomDelta(obj.in_room, "Update", obj);
        }

        static void ItemDelta(CharData ch, string op, string location, ObjData obj)
        {
            if (!Playing(ch) || obj == null)
                return;
            string loc = location == "eq" ? WearName(obj.wear_loc) : "none";
            if (op == "Remove" && location == "eq")
                loc = WearName(obj.wear_loc);
            Send(ch.desc, "Char.Items." + op, JsonSerializer.Serialize(new Dictionary<string, object>
            {
                ["location"] = location,
                ["item"] = ItemObj(obj, loc)
            }));
        }

        static void ItemRoomDelta(RoomIndexData room, string op, ObjData obj)
        {
            if (room == null || obj == null)
                return;
            string json = JsonSerializer.Serialize(new Dictionary<string, object>
            {
                ["location"] = "room",
                ["item"] = ItemObj(obj, "none")
            });
            for (var p = room.people; p != null; p = p.next_in_room)
            {
                if (!Playing(p) || !Handler.can_see_obj(p, obj))
                    continue;
                Send(p.desc, "Char.Items." + op, json);
            }
        }

        public static Dictionary<string, int> ExitMap(CharData ch)
        {
            var exits = new Dictionary<string, int>();
            if (ch?.in_room == null || CantSeeRoom(ch, ch.in_room))
                return exits;
            for (int door = 0; door < 6; door++)
            {
                var pexit = ch.in_room.exit[door];
                if (pexit == null || pexit.to_room == null)
                    continue;
                if (!Handler.can_see_room(ch, pexit.to_room))
                    continue;
                if (Bit.IS_SET(pexit.exit_info, Merc.EX_CLOSED))
                    continue;
                exits[DirKeys[door]] = pexit.to_room.vnum;
            }
            return exits;
        }

        public static byte[] EscapeIac(byte[] raw)
        {
            var escaped = new List<byte>(raw.Length);
            foreach (byte b in raw)
            {
                escaped.Add(b);
                if (b == 255)
                    escaped.Add(255);
            }
            return escaped.ToArray();
        }

        public static byte[] Frame(string package, string json)
        {
            string body = json == null ? package : package + " " + json;
            var raw = Encoding.UTF8.GetBytes(body);
            var frame = new List<byte>(raw.Length + 8);
            frame.Add(TelnetParser.IAC);
            frame.Add(TelnetParser.SB);
            frame.Add(TelnetParser.GMCP);
            frame.AddRange(EscapeIac(raw));
            frame.Add(TelnetParser.IAC);
            frame.Add(TelnetParser.SE);
            return frame.ToArray();
        }

        public static void Send(DescriptorData d, string package, string json)
        {
            if (d == null || !d.Gmcp || d.socket == null)
                return;
            if (!d.GmcpMods.Allows(package))
                return;
            try
            {
                SendBytes(d, Frame(package, json));
            }
            catch { }
        }

        static void SendItems(DescriptorData d, string location, List<Dictionary<string, object>> items)
        {
            Send(d, "Char.Items.List", JsonSerializer.Serialize(new Dictionary<string, object>
            {
                ["location"] = location,
                ["items"] = items
            }));
        }

        static void SendContents(DescriptorData d, CharData ch, string id)
        {
            var obj = FindId(ch.carrying, id);
            var items = new List<Dictionary<string, object>>();
            if (obj != null)
            {
                for (var c = obj.contains; c != null; c = c.next_content)
                {
                    if (Handler.can_see_obj(ch, c))
                        items.Add(ItemObj(c, "none"));
                }
            }
            SendItems(d, id ?? "", items);
        }

        static List<Dictionary<string, object>> InvItems(CharData ch)
        {
            var list = new List<Dictionary<string, object>>();
            for (var obj = ch.carrying; obj != null; obj = obj.next_content)
            {
                if (obj.wear_loc != Merc.WEAR_NONE)
                    continue;
                if (!Handler.can_see_obj(ch, obj))
                    continue;
                list.Add(ItemObj(obj, "none"));
            }
            return list;
        }

        static List<Dictionary<string, object>> EqItems(CharData ch)
        {
            var list = new List<Dictionary<string, object>>();
            for (var obj = ch.carrying; obj != null; obj = obj.next_content)
            {
                if (obj.wear_loc == Merc.WEAR_NONE)
                    continue;
                if (!Handler.can_see_obj(ch, obj))
                    continue;
                list.Add(ItemObj(obj, WearName(obj.wear_loc)));
            }
            return list;
        }

        static List<Dictionary<string, object>> RoomItems(CharData ch)
        {
            var list = new List<Dictionary<string, object>>();
            if (ch.in_room == null || CantSeeRoom(ch, ch.in_room))
                return list;
            for (var obj = ch.in_room.contents; obj != null; obj = obj.next_content)
            {
                if (!Handler.can_see_obj(ch, obj))
                    continue;
                list.Add(ItemObj(obj, "none"));
            }
            return list;
        }

        static Dictionary<string, object> ItemObj(ObjData obj, string loc)
        {
            if (obj.GmcpId == 0)
            {
                obj.GmcpId = AllocId();
            }
            var item = new Dictionary<string, object>
            {
                ["id"] = obj.GmcpId.ToString(),
                ["name"] = OneLine(obj.short_descr),
                ["loc"] = loc
            };
            if (obj.item_type == Merc.ITEM_WAND || obj.item_type == Merc.ITEM_STAFF)
            {
                item["charges"] = obj.value[2];
                item["maxcharges"] = obj.value[1];
            }
            else if (obj.item_type == Merc.ITEM_LIGHT)
            {
                item["hours"] = obj.value[2];
            }
            else if (obj.item_type == Merc.ITEM_DRINK_CON)
            {
                item["sips"] = obj.value[1];
                item["maxsips"] = obj.value[0];
            }
            return item;
        }

        static ObjData FindId(ObjData list, string id)
        {
            for (var obj = list; obj != null; obj = obj.next_content)
            {
                if (obj.GmcpId.ToString() == id)
                    return obj;
                var inner = FindId(obj.contains, id);
                if (inner != null)
                    return inner;
            }
            return null;
        }

        static object SkillsDoc(CharData ch, string onlyGroup)
        {
            var groups = new List<Dictionary<string, object>>();
            if (Bit.IS_NPC(ch) || ch.pcdata == null)
                return new Dictionary<string, object> { ["groups"] = groups };
            for (int gn = 0; gn < Tables.group_table.Length; gn++)
            {
                var g = Tables.group_table[gn];
                if (g?.name == null)
                    continue;
                if (!string.IsNullOrEmpty(onlyGroup)
                    && !g.name.Equals(onlyGroup, StringComparison.OrdinalIgnoreCase))
                    continue;
                var skills = new List<Dictionary<string, object>>();
                if (g.spells != null)
                {
                    foreach (var spell in g.spells)
                    {
                        int sn = Lookup.skill_lookup(spell);
                        if (sn < 0 || sn >= ch.pcdata.learned.Length)
                            continue;
                        int pct = ch.pcdata.learned[sn];
                        if (pct < 1)
                            continue;
                        skills.Add(new Dictionary<string, object>
                        {
                            ["name"] = Tables.skill_table[sn].name,
                            ["percent"] = pct
                        });
                    }
                }
                bool known = gn < ch.pcdata.group_known.Length && ch.pcdata.group_known[gn];
                if (!known && skills.Count == 0)
                    continue;
                groups.Add(new Dictionary<string, object>
                {
                    ["name"] = g.name,
                    ["skills"] = skills
                });
            }
            return new Dictionary<string, object> { ["groups"] = groups };
        }

        static object SkillInfo(CharData ch, string name)
        {
            int sn = string.IsNullOrEmpty(name) ? -1 : Lookup.skill_lookup(name);
            int pct = 0;
            string found = name ?? "";
            if (sn >= 0 && ch.pcdata != null && sn < ch.pcdata.learned.Length)
            {
                pct = ch.pcdata.learned[sn];
                found = Tables.skill_table[sn].name ?? found;
            }
            return new Dictionary<string, object>
            {
                ["name"] = found,
                ["percent"] = pct
            };
        }

        static List<Dictionary<string, string>> PlayerList(CharData looker, RoomIndexData room)
        {
            var list = new List<Dictionary<string, string>>();
            for (var p = room.people; p != null; p = p.next_in_room)
            {
                if (Bit.IS_NPC(p) || p == looker)
                    continue;
                if (!Handler.can_see(looker, p))
                    continue;
                list.Add(PlayerObj(p));
            }
            return list;
        }

        static Dictionary<string, string> PlayerObj(CharData ch)
        {
            string title = ch.pcdata?.title ?? "";
            return new Dictionary<string, string>
            {
                ["name"] = ch.name ?? "",
                ["fullname"] = ((ch.name ?? "") + title).Trim()
            };
        }

        static string StatusVars()
            => JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["level"] = "Level",
                ["race"] = "Race",
                ["class"] = "Class",
                ["position"] = "Position",
                ["align"] = "Alignment",
                ["hunger"] = "Hunger",
                ["thirst"] = "Thirst",
                ["enemy"] = "Enemy",
                ["enemypct"] = "Enemy%"
            });

        static string RaceName(CharData ch)
        {
            if (ch.race < 0 || ch.race >= Tables.race_table.Length)
                return "";
            return Tables.race_table[ch.race].name ?? "";
        }

        static string ClassName(CharData ch)
        {
            if (Bit.IS_NPC(ch) || ch.klass < 0 || ch.klass >= Tables.class_table.Length)
                return "mobile";
            return Tables.class_table[ch.klass].name ?? "";
        }

        static string PositionName(int pos)
        {
            if (pos < 0 || pos >= Tables.position_table.Length || Tables.position_table[pos].name == null)
                return "standing";
            return Tables.position_table[pos].name;
        }

        static string WearName(int loc)
        {
            if (loc < 0 || loc >= WearNames.Length)
                return "none";
            return WearNames[loc];
        }

        static bool CantSeeRoom(CharData ch, RoomIndexData room)
        {
            if (ch == null || room == null)
                return true;
            if (Bit.IS_AFFECTED(ch, Merc.AFF_BLIND) && !Bit.IS_SET(ch.act, Merc.PLR_HOLYLIGHT))
                return true;
            if (!Bit.IS_NPC(ch) && !Bit.IS_SET(ch.act, Merc.PLR_HOLYLIGHT) && Handler.room_is_dark(room))
                return true;
            return false;
        }

        static bool Playing(DescriptorData d)
            => d != null && d.Gmcp && d.character != null && d.connected == Merc.CON_PLAYING;

        static bool Playing(CharData ch)
            => ch?.desc != null && Playing(ch.desc) && ch.desc.character == ch;

        static bool Eq(string a, string b) => a.Equals(b, StringComparison.OrdinalIgnoreCase);

        static string JsonString(string body)
        {
            if (string.IsNullOrEmpty(body))
                return "";
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.ValueKind == JsonValueKind.String)
                    return doc.RootElement.GetString() ?? "";
            }
            catch (JsonException) { }
            return body.Trim().Trim('"');
        }

        static List<string> ReadStringArray(string body)
        {
            var list = new List<string>();
            if (string.IsNullOrWhiteSpace(body))
                return list;
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
                return list;
            foreach (var el in doc.RootElement.EnumerateArray())
                list.Add(el.ToString());
            return list;
        }

        static string OneLine(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";
            return s.Replace("\n", "").Replace("\r", "").Trim();
        }

        static string Plain(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";
            var sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '{' && i + 1 < s.Length)
                {
                    i++;
                    continue;
                }
                if (s[i] == '\n' || s[i] == '\r')
                    continue;
                sb.Append(s[i]);
            }
            return sb.ToString();
        }

        static void SendBytes(DescriptorData d, byte[] bytes)
        {
            if (d?.socket == null || bytes == null || bytes.Length == 0)
                return;
            try
            {
                int off = 0;
                while (off < bytes.Length)
                {
                    int n = d.socket.Send(bytes, off, bytes.Length - off, SocketFlags.None);
                    if (n <= 0)
                        return;
                    off += n;
                }
            }
            catch { }
        }
    }
}
