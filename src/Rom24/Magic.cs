using static Rom24.Merc;

namespace Rom24
{
    public static partial class Magic
    {
        public static string target_name = "";

        public static int slot_lookup(int slot)
        {
            if (slot <= 0)
                return -1;

            for (int sn = 0; sn < MAX_SKILL; sn++)
            {
                if (sn >= Tables.skill_table.Length || Tables.skill_table[sn].name == null)
                    break;
                if (slot == Tables.skill_table[sn].slot)
                    return sn;
            }

            if (Game.fBootDb)
            {
                Db.bug("Slot_lookup: bad slot %d.", slot);
                Environment.Exit(1);
            }

            return -1;
        }

        public static void assign_spells()
        {
            var t = typeof(Magic);
            for (int sn = 0; sn < Tables.skill_table.Length; sn++)
            {
                var sk = Tables.skill_table[sn];
                if (sk.name == null)
                    break;
                /* const.c "reserved" spell_fun is 0, not spell_null */
                if (sk.name == "reserved")
                {
                    sk.spell_fun = null;
                    continue;
                }
                if (!sk.is_spell)
                {
                    sk.spell_fun = spell_null;
                    continue;
                }
                /* skill name "invisibility" is spell_invis in const.c */
                string funName = sk.name == "invisibility"
                    ? "spell_invis"
                    : "spell_" + sk.name.Replace(' ', '_');
                var method = t.GetMethod(funName);
                if (method != null)
                    sk.spell_fun = (SpellFun)Delegate.CreateDelegate(typeof(SpellFun), method);
                else
                    sk.spell_fun = spell_null;
            }
        }

        static readonly (string old, string neu)[] syl_table =
        {
            (" ", " "), ("ar", "abra"), ("au", "kada"), ("bless", "fido"),
            ("blind", "nose"), ("bur", "mosa"), ("cu", "judi"), ("de", "oculo"),
            ("en", "unso"), ("light", "dies"), ("lo", "hi"), ("mor", "zak"),
            ("move", "sido"), ("ness", "lacri"), ("ning", "illa"), ("per", "duda"),
            ("ra", "gru"), ("fresh", "ima"), ("re", "candus"), ("son", "sabru"),
            ("tect", "infra"), ("tri", "cula"), ("ven", "nofo"),
            ("a", "a"), ("b", "b"), ("c", "q"), ("d", "e"),
            ("e", "z"), ("f", "y"), ("g", "o"), ("h", "p"),
            ("i", "u"), ("j", "y"), ("k", "t"), ("l", "r"),
            ("m", "w"), ("n", "i"), ("o", "a"), ("p", "s"),
            ("q", "d"), ("r", "f"), ("s", "g"), ("t", "h"),
            ("u", "j"), ("v", "z"), ("w", "x"), ("x", "n"),
            ("y", "l"), ("z", "k"),
            ("", "")
        };

        public static void say_spell(CharData ch, int sn)
        {
            string buf = "";
            var name = Tables.skill_table[sn].name;
            int p = 0;
            while (p < name.Length)
            {
                int length = 1;
                for (int iSyl = 0; syl_table[iSyl].old.Length != 0; iSyl++)
                {
                    length = syl_table[iSyl].old.Length;
                    if (name.Length - p >= length
                        && string.Compare(name, p, syl_table[iSyl].old, 0, length,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        buf += syl_table[iSyl].neu;
                        break;
                    }
                    length = 0;
                }
                if (length == 0)
                    length = 1;
                p += length;
            }

            string buf2 = $"$n utters the words, '{buf}'.";
            string plain = $"$n utters the words, '{Tables.skill_table[sn].name}'.";

            for (var rch = ch.in_room.people; rch != null; rch = rch.next_in_room)
            {
                if (rch != ch)
                    Comm.act((!Bit.IS_NPC(rch) && ch.klass == rch.klass) ? plain : buf2,
                        ch, null, rch, TO_VICT);
            }
        }

        public static void do_cast(CharData ch, string argument)
        {
            if (Bit.IS_NPC(ch) && ch.desc == null)
                return;

            target_name = RomString.one_argument(argument, out string arg1);
            RomString.one_argument(target_name, out string arg2);

            if (arg1.Length == 0)
            {
                Comm.send_to_char("Cast which what where?\n\r", ch);
                return;
            }

            int sn = Lookup.find_spell(ch, arg1);
            if (sn < 1
                || Tables.skill_table[sn].spell_fun == spell_null || (!Bit.IS_NPC(ch)
                                                               && (ch.level <
                                                                   Tables.skill_table[sn].skill_level[ch.klass]
                                                                   || ch.pcdata.learned[sn]
                                                                   == 0)))
            {
                Comm.send_to_char("You don't know any spells of that name.\n\r", ch);
                return;
            }

            if (ch.position < Tables.skill_table[sn].minimum_position)
            {
                Comm.send_to_char("You can't concentrate enough.\n\r", ch);
                return;
            }

            int mana;
            if (ch.level + 2 == Tables.skill_table[sn].skill_level[ch.klass])
                mana = 50;
            else
                mana = Bit.UMAX(Tables.skill_table[sn].min_mana,
                    100 / (2 + ch.level -
                           Tables.skill_table[sn].skill_level[ch.klass]));

            CharData victim = null;
            ObjData obj = null;
            object vo = null;
            int target = TARGET_NONE;

            switch (Tables.skill_table[sn].target)
            {
                default:
                    Db.bug("Do_cast: bad target for sn %d.", sn);
                    return;

                case TAR_IGNORE:
                    break;

                case TAR_CHAR_OFFENSIVE:
                    if (arg2.Length == 0)
                    {
                        if ((victim = ch.fighting) == null)
                        {
                            Comm.send_to_char("Cast the spell on whom?\n\r", ch);
                            return;
                        }
                    }
                    else
                    {
                        if ((victim = Handler.get_char_room(ch, target_name)) == null)
                        {
                            Comm.send_to_char("They aren't here.\n\r", ch);
                            return;
                        }
                    }

                    if (!Bit.IS_NPC(ch))
                    {
                        if (Fight.is_safe(ch, victim) && victim != ch)
                        {
                            Comm.send_to_char("Not on that target.\n\r", ch);
                            return;
                        }
                        Fight.check_killer(ch, victim);
                    }

                    if (Bit.IS_AFFECTED(ch, AFF_CHARM) && ch.master == victim)
                    {
                        Comm.send_to_char("You can't do that on your own follower.\n\r",
                            ch);
                        return;
                    }

                    vo = victim;
                    target = TARGET_CHAR;
                    break;

                case TAR_CHAR_DEFENSIVE:
                    if (arg2.Length == 0)
                    {
                        victim = ch;
                    }
                    else
                    {
                        if ((victim = Handler.get_char_room(ch, target_name)) == null)
                        {
                            Comm.send_to_char("They aren't here.\n\r", ch);
                            return;
                        }
                    }

                    vo = victim;
                    target = TARGET_CHAR;
                    break;

                case TAR_CHAR_SELF:
                    if (arg2.Length != 0 && !Handler.is_name(target_name, ch.name))
                    {
                        Comm.send_to_char("You cannot cast this spell on another.\n\r",
                            ch);
                        return;
                    }

                    vo = ch;
                    target = TARGET_CHAR;
                    break;

                case TAR_OBJ_INV:
                    if (arg2.Length == 0)
                    {
                        Comm.send_to_char("What should the spell be cast upon?\n\r", ch);
                        return;
                    }

                    if ((obj = Handler.get_obj_carry(ch, target_name, ch)) == null)
                    {
                        Comm.send_to_char("You are not carrying that.\n\r", ch);
                        return;
                    }

                    vo = obj;
                    target = TARGET_OBJ;
                    break;

                case TAR_OBJ_CHAR_OFF:
                    if (arg2.Length == 0)
                    {
                        if ((victim = ch.fighting) == null)
                        {
                            Comm.send_to_char("Cast the spell on whom or what?\n\r", ch);
                            return;
                        }

                        target = TARGET_CHAR;
                    }
                    else if ((victim = Handler.get_char_room(ch, target_name)) != null)
                    {
                        target = TARGET_CHAR;
                    }

                    if (target == TARGET_CHAR)
                    {
                        if (Fight.is_safe_spell(ch, victim, false) && victim != ch)
                        {
                            Comm.send_to_char("Not on that target.\n\r", ch);
                            return;
                        }

                        if (Bit.IS_AFFECTED(ch, AFF_CHARM) && ch.master == victim)
                        {
                            Comm.send_to_char(
                                "You can't do that on your own follower.\n\r", ch);
                            return;
                        }

                        if (!Bit.IS_NPC(ch))
                            Fight.check_killer(ch, victim);

                        vo = victim;
                    }
                    else if ((obj = Handler.get_obj_here(ch, target_name)) != null)
                    {
                        vo = obj;
                        target = TARGET_OBJ;
                    }
                    else
                    {
                        Comm.send_to_char("You don't see that here.\n\r", ch);
                        return;
                    }
                    break;

                case TAR_OBJ_CHAR_DEF:
                    if (arg2.Length == 0)
                    {
                        vo = ch;
                        target = TARGET_CHAR;
                    }
                    else if ((victim = Handler.get_char_room(ch, target_name)) != null)
                    {
                        vo = victim;
                        target = TARGET_CHAR;
                    }
                    else if ((obj = Handler.get_obj_carry(ch, target_name, ch)) != null)
                    {
                        vo = obj;
                        target = TARGET_OBJ;
                    }
                    else
                    {
                        Comm.send_to_char("You don't see that here.\n\r", ch);
                        return;
                    }
                    break;
            }

            if (!Bit.IS_NPC(ch) && ch.mana < mana)
            {
                Comm.send_to_char("You don't have enough mana.\n\r", ch);
                return;
            }

            if (RomString.str_cmp(Tables.skill_table[sn].name, "ventriloquate"))
                say_spell(ch, sn);

            Bit.WAIT_STATE(ch, Tables.skill_table[sn].beats);

            if (RomRandom.number_percent() > Handler.get_skill(ch, sn))
            {
                Comm.send_to_char("You lost your concentration.\n\r", ch);
                Skills.check_improve(ch, sn, false, 1);
                ch.mana -= mana / 2;
            }
            else
            {
                ch.mana -= mana;
                if (Bit.IS_NPC(ch) || Tables.class_table[ch.klass].fMana)
                    Tables.skill_table[sn].spell_fun(sn, ch.level, ch, vo, target);
                else
                    Tables.skill_table[sn].spell_fun(sn, 3 * ch.level / 4, ch, vo, target);
                Skills.check_improve(ch, sn, true, 1);
            }

            if ((Tables.skill_table[sn].target == TAR_CHAR_OFFENSIVE
                 || (Tables.skill_table[sn].target == TAR_OBJ_CHAR_OFF
                     && target == TARGET_CHAR)) && victim != ch
                && victim != null && victim.master != ch)
            {
                for (var vch = ch.in_room.people; vch != null; vch = vch.next_in_room)
                {
                    if (victim == vch && victim.fighting == null)
                    {
                        Fight.check_killer(victim, ch);
                        Fight.multi_hit(victim, ch, TYPE_UNDEFINED);
                        break;
                    }
                }
            }
        }

        public static bool saves_spell(int level, CharData victim, int dam_type)
        {
            int save;

            save = 50 + (victim.level - level) * 5 - victim.saving_throw * 2;
            if (Bit.IS_AFFECTED(victim, AFF_BERSERK))
                save += victim.level / 2;

            switch (Handler.check_immune(victim, dam_type))
            {
                case IS_IMMUNE:
                    return true;
                case IS_RESISTANT:
                    save += 2;
                    break;
                case IS_VULNERABLE:
                    save -= 2;
                    break;
            }

            if (!Bit.IS_NPC(victim) && Tables.class_table[victim.klass].fMana)
                save = 9 * save / 10;
            save = Bit.URANGE(5, save, 95);
            return RomRandom.number_percent() < save;
        }

        public static bool saves_dispel(int dis_level, int spell_level, int duration)
        {
            if (duration == -1)
                spell_level += 5;

            int save = 50 + (spell_level - dis_level) * 5;
            save = Bit.URANGE(5, save, 95);
            return RomRandom.number_percent() < save;
        }

        public static bool check_dispel(int dis_level, CharData victim, int sn)
        {
            if (Handler.is_affected(victim, sn))
            {
                for (var af = victim.affected; af != null; af = af.next)
                {
                    if (af.type == sn)
                    {
                        if (!saves_dispel(dis_level, af.level, af.duration))
                        {
                            Handler.affect_strip(victim, sn);
                            if (Tables.skill_table[sn].msg_off != null)
                            {
                                Comm.send_to_char(Tables.skill_table[sn].msg_off, victim);
                                Comm.send_to_char("\n\r", victim);
                            }
                            return true;
                        }
                        else
                            af.level--;
                    }
                }
            }
            return false;
        }

        public static int mana_cost(CharData ch, int min_mana, int level)
        {
            if (ch.level + 2 == level)
                return 1000;
            return Bit.UMAX(min_mana, (100 / (2 + ch.level - level)));
        }

        public static void obj_cast_spell(int sn, int level, CharData ch, CharData victim,
            ObjData obj)
        {
            object vo;
            int target = TARGET_NONE;

            if (sn <= 0)
                return;

            if (sn >= MAX_SKILL || Tables.skill_table[sn].spell_fun == null)
            {
                Db.bug("Obj_cast_spell: bad sn %d.", sn);
                return;
            }

            switch (Tables.skill_table[sn].target)
            {
                default:
                    Db.bug("Obj_cast_spell: bad target for sn %d.", sn);
                    return;

                case TAR_IGNORE:
                    vo = null;
                    break;

                case TAR_CHAR_OFFENSIVE:
                    if (victim == null)
                        victim = ch.fighting;
                    if (victim == null)
                    {
                        Comm.send_to_char("You can't do that.\n\r", ch);
                        return;
                    }
                    if (Fight.is_safe(ch, victim) && ch != victim)
                    {
                        Comm.send_to_char("Something isn't right...\n\r", ch);
                        return;
                    }
                    vo = victim;
                    target = TARGET_CHAR;
                    break;

                case TAR_CHAR_DEFENSIVE:
                case TAR_CHAR_SELF:
                    if (victim == null)
                        victim = ch;
                    vo = victim;
                    target = TARGET_CHAR;
                    break;

                case TAR_OBJ_INV:
                    if (obj == null)
                    {
                        Comm.send_to_char("You can't do that.\n\r", ch);
                        return;
                    }
                    vo = obj;
                    target = TARGET_OBJ;
                    break;

                case TAR_OBJ_CHAR_OFF:
                    if (victim == null && obj == null)
                    {
                        if (ch.fighting != null)
                            victim = ch.fighting;
                        else
                        {
                            Comm.send_to_char("You can't do that.\n\r", ch);
                            return;
                        }
                    }

                    if (victim != null)
                    {
                        if (Fight.is_safe_spell(ch, victim, false) && ch != victim)
                        {
                            Comm.send_to_char("Somehting isn't right...\n\r", ch);
                            return;
                        }

                        vo = victim;
                        target = TARGET_CHAR;
                    }
                    else
                    {
                        vo = obj;
                        target = TARGET_OBJ;
                    }
                    break;

                case TAR_OBJ_CHAR_DEF:
                    if (victim == null && obj == null)
                    {
                        vo = ch;
                        target = TARGET_CHAR;
                    }
                    else if (victim != null)
                    {
                        vo = victim;
                        target = TARGET_CHAR;
                    }
                    else
                    {
                        vo = obj;
                        target = TARGET_OBJ;
                    }

                    break;
            }

            target_name = "";
            Tables.skill_table[sn].spell_fun(sn, level, ch, vo, target);

            if ((Tables.skill_table[sn].target == TAR_CHAR_OFFENSIVE
                 || (Tables.skill_table[sn].target == TAR_OBJ_CHAR_OFF
                     && target == TARGET_CHAR)) && victim != ch
                && victim != null && victim.master != ch)
            {
                for (var vch = ch.in_room.people; vch != null; vch = vch.next_in_room)
                {
                    if (victim == vch && victim.fighting == null)
                    {
                        Fight.check_killer(victim, ch);
                        Fight.multi_hit(victim, ch, TYPE_UNDEFINED);
                        break;
                    }
                }
            }
        }
    }
}
