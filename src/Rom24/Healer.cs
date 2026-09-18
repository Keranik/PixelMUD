using static Rom24.Merc;

namespace Rom24
{
    public static class Healer
    {
        public static void do_heal(CharData ch, string argument)
        {
            CharData mob;
            for (mob = ch.in_room.people; mob != null; mob = mob.next_in_room)
            {
                if (Bit.IS_NPC(mob) && Bit.IS_SET(mob.act, ACT_IS_HEALER))
                    break;
            }

            if (mob == null)
            {
                Comm.send_to_char("You can't do that here.\n\r", ch);
                return;
            }

            RomString.one_argument(argument, out string arg);

            if (arg.Length == 0)
            {
                Comm.act("$N says 'I offer the following spells:'", ch, null, mob,
                    TO_CHAR);
                Comm.send_to_char("  light: cure light wounds      10 gold\n\r", ch);
                Comm.send_to_char("  serious: cure serious wounds  15 gold\n\r", ch);
                Comm.send_to_char("  critic: cure critical wounds  25 gold\n\r", ch);
                Comm.send_to_char("  heal: healing spell          50 gold\n\r", ch);
                Comm.send_to_char("  blind: cure blindness         20 gold\n\r", ch);
                Comm.send_to_char("  disease: cure disease         15 gold\n\r", ch);
                Comm.send_to_char("  poison:  cure poison          25 gold\n\r", ch);
                Comm.send_to_char("  uncurse: remove curse          50 gold\n\r", ch);
                Comm.send_to_char("  refresh: restore movement      5 gold\n\r", ch);
                Comm.send_to_char("  mana:  restore mana          10 gold\n\r", ch);
                Comm.send_to_char(" Type heal <type> to be healed.\n\r", ch);
                return;
            }

            SpellFun spell;
            int sn;
            string words;
            int cost;

            if (!RomString.str_prefix(arg, "light"))
            {
                spell = Magic.spell_cure_light;
                sn = Lookup.skill_lookup("cure light");
                words = "judicandus dies";
                cost = 1000;
            }
            else if (!RomString.str_prefix(arg, "serious"))
            {
                spell = Magic.spell_cure_serious;
                sn = Lookup.skill_lookup("cure serious");
                words = "judicandus gzfuajg";
                cost = 1600;
            }
            else if (!RomString.str_prefix(arg, "critical"))
            {
                spell = Magic.spell_cure_critical;
                sn = Lookup.skill_lookup("cure critical");
                words = "judicandus qfuhuqar";
                cost = 2500;
            }
            else if (!RomString.str_prefix(arg, "heal"))
            {
                spell = Magic.spell_heal;
                sn = Lookup.skill_lookup("heal");
                words = "pzar";
                cost = 5000;
            }
            else if (!RomString.str_prefix(arg, "blindness"))
            {
                spell = Magic.spell_cure_blindness;
                sn = Lookup.skill_lookup("cure blindness");
                words = "judicandus noselacri";
                cost = 2000;
            }
            else if (!RomString.str_prefix(arg, "disease"))
            {
                spell = Magic.spell_cure_disease;
                sn = Lookup.skill_lookup("cure disease");
                words = "judicandus eugzagz";
                cost = 1500;
            }
            else if (!RomString.str_prefix(arg, "poison"))
            {
                spell = Magic.spell_cure_poison;
                sn = Lookup.skill_lookup("cure poison");
                words = "judicandus sausabru";
                cost = 2500;
            }
            else if (!RomString.str_prefix(arg, "uncurse") || !RomString.str_prefix(arg, "curse"))
            {
                spell = Magic.spell_remove_curse;
                sn = Lookup.skill_lookup("remove curse");
                words = "candussido judifgz";
                cost = 5000;
            }
            else if (!RomString.str_prefix(arg, "mana") || !RomString.str_prefix(arg, "energize"))
            {
                spell = null;
                sn = -1;
                words = "energizer";
                cost = 1000;
            }
            else if (!RomString.str_prefix(arg, "refresh") || !RomString.str_prefix(arg, "moves"))
            {
                spell = Magic.spell_refresh;
                sn = Lookup.skill_lookup("refresh");
                words = "candusima";
                cost = 500;
            }
            else
            {
                Comm.act("$N says 'Type 'heal' for a list of spells.'",
                    ch, null, mob, TO_CHAR);
                return;
            }

            if (cost > (ch.gold * 100 + ch.silver))
            {
                Comm.act("$N says 'You do not have enough gold for my services.'",
                    ch, null, mob, TO_CHAR);
                return;
            }

            Bit.WAIT_STATE(ch, PULSE_VIOLENCE);

            Handler.deduct_cost(ch, cost);
            mob.gold += cost / 100;
            mob.silver += cost % 100;
            Comm.act("$n utters the words '$T'.", mob, null, words, TO_ROOM);

            if (spell == null)
            {
                ch.mana += RomRandom.dice(2, 8) + mob.level / 3;
                ch.mana = Bit.UMIN(ch.mana, ch.max_mana);
                Comm.send_to_char("A warm glow passes through you.\n\r", ch);
                return;
            }

            if (sn == -1)
                return;

            spell(sn, mob.level, mob, ch, TARGET_CHAR);
        }
    }
}
