namespace Rom24
{
    public static class Gsn
    {
        public static int backstab, dodge, hide, sneak, steal, pick_lock, peek;
        public static int disarm, enhanced_damage, kick, parry, rescue;
        public static int second_attack, third_attack;
        public static int blindness, charm_person, curse, invis, mass_invis, plague, poison, sleep, fly, sanctuary;
        public static int axe, dagger, flail, mace, polearm, shield_block, spear, sword, whip;
        public static int bash, berserk, dirt, hand_to_hand, trip;
        public static int fast_healing, haggle, lore, meditation;
        public static int scrolls, staves, wands, recall, envenom;

        public static void assign()
        {
            backstab = Lookup.skill_lookup("backstab");
            dodge = Lookup.skill_lookup("dodge");
            hide = Lookup.skill_lookup("hide");
            sneak = Lookup.skill_lookup("sneak");
            steal = Lookup.skill_lookup("steal");
            pick_lock = Lookup.skill_lookup("pick lock");
            peek = Lookup.skill_lookup("peek");
            disarm = Lookup.skill_lookup("disarm");
            enhanced_damage = Lookup.skill_lookup("enhanced damage");
            kick = Lookup.skill_lookup("kick");
            parry = Lookup.skill_lookup("parry");
            rescue = Lookup.skill_lookup("rescue");
            second_attack = Lookup.skill_lookup("second attack");
            third_attack = Lookup.skill_lookup("third attack");
            blindness = Lookup.skill_lookup("blindness");
            charm_person = Lookup.skill_lookup("charm person");
            curse = Lookup.skill_lookup("curse");
            invis = Lookup.skill_lookup("invisibility");
            mass_invis = Lookup.skill_lookup("mass invis");
            plague = Lookup.skill_lookup("plague");
            poison = Lookup.skill_lookup("poison");
            sleep = Lookup.skill_lookup("sleep");
            /* const.c "fly" pgsn is NULL; gsn_fly stays 0 */
            sanctuary = Lookup.skill_lookup("sanctuary");
            axe = Lookup.skill_lookup("axe");
            dagger = Lookup.skill_lookup("dagger");
            flail = Lookup.skill_lookup("flail");
            mace = Lookup.skill_lookup("mace");
            polearm = Lookup.skill_lookup("polearm");
            shield_block = Lookup.skill_lookup("shield block");
            spear = Lookup.skill_lookup("spear");
            sword = Lookup.skill_lookup("sword");
            whip = Lookup.skill_lookup("whip");
            bash = Lookup.skill_lookup("bash");
            berserk = Lookup.skill_lookup("berserk");
            dirt = Lookup.skill_lookup("dirt kicking");
            hand_to_hand = Lookup.skill_lookup("hand to hand");
            trip = Lookup.skill_lookup("trip");
            fast_healing = Lookup.skill_lookup("fast healing");
            haggle = Lookup.skill_lookup("haggle");
            lore = Lookup.skill_lookup("lore");
            meditation = Lookup.skill_lookup("meditation");
            scrolls = Lookup.skill_lookup("scrolls");
            staves = Lookup.skill_lookup("staves");
            wands = Lookup.skill_lookup("wands");
            recall = Lookup.skill_lookup("recall");
            envenom = Lookup.skill_lookup("envenom");
        }
    }
}
