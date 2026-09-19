/**************************************************************************
 *  File: olc_save.c                                                       *
 *                                                                         *
 *  Much time and thought has gone into this software and you are          *
 *  benefitting.  We hope that you share your changes too.  What goes      *
 *  around, comes around.                                                  *
 *                                                                         *
 *  This code was freely distributed with the The Isles 1.1 source code,   *
 *  and has been used here for OLC - OLC would not be what it is without   *
 *  all the previous coders who released their source code.                *
 *                                                                         *
 ***************************************************************************/
/* OLC_SAVE.C
 * This takes care of saving all the .are information.
 * Notes:
 * -If a good syntax checker is used for setting vnum ranges of areas
 *  then it would become possible to just cycle through vnums instead
 *  of using the iHash stuff and checking that the room or reset or
 *  mob etc is part of that area.
 */

using static Rom24.Merc;

namespace Rom24
{
    public static partial class Olc
    {
        static long DIF(long a, long b) => ~((~a) | b);

        /*
         *  Verbose writes reset data in plain english into the comments
         *  section of the resets.  It makes areas considerably larger but
         *  may aid in debugging.
         */

        /*****************************************************************************
         Name:        fix_string
         Purpose:    Returns a string without \r and ~.
         ****************************************************************************/
        static string fix_string(string str)
        {
            if (str == null)
                return "";

            var strfix = new char[str.Length];
            int i;
            int o;

            for (o = i = 0; i + o < str.Length; i++)
            {
                if (str[i + o] == '\r' || str[i + o] == '~')
                    o++;
                if (i + o >= str.Length)
                    break;
                strfix[i] = str[i + o];
            }
            return new string(strfix, 0, i);
        }

        /*****************************************************************************
         Name:        save_area_list
         Purpose:    Saves the listing of files to be loaded at startup.
         Called by:    do_asave(olc_save.c).
         ****************************************************************************/
        static void save_area_list()
        {
            var path = Path.Combine(Game.area_dir, "area.lst");
            try
            {
                using var fp = new StreamWriter(path);
                /*
                 * Add any help files that need to be loaded at
                 * startup to this section.
                 */
                fp.Write(RomString.sprintf("social.are\n"));    /* ROM OLC */

                for (var ha = Game.had_list; ha != null; ha = ha.next)
                    if (ha.area == null)
                        fp.Write(RomString.sprintf("%s\n", ha.filename));

                for (var pArea = Game.area_first; pArea != null; pArea = pArea.next)
                {
                    fp.Write(RomString.sprintf("%s\n", pArea.file_name));
                }

                fp.Write(RomString.sprintf("$\n"));
            }
            catch
            {
                Db.bug("Save_area_list: fopen", 0);
            }
        }

        /*
         * ROM OLC
         * Used in save_mobile and save_object below.  Writes
         * flags on the form fread_flag reads.
         *
         * buf[] must hold at least 32+1 characters.
         *
         * -- Hugin
         */
        static string fwrite_flag(long flags)
        {
            if (flags == 0)
            {
                return "0";
            }

            /* 32 -- number of bits in a long */

            var buf = new char[32];
            int cp = 0;
            for (int offset = 0; offset < 32; offset++)
                if ((flags & (1L << offset)) != 0)
                {
                    if (offset <= 'Z' - 'A')
                        buf[cp++] = (char)('A' + offset);
                    else
                        buf[cp++] = (char)('a' + offset - ('Z' - 'A' + 1));
                }

            return new string(buf, 0, cp);
        }

        static void save_mobprogs(StreamWriter fp, AreaData pArea)
        {
            fp.Write(RomString.sprintf("#MOBPROGS\n"));

            for (int i = pArea.min_vnum; i <= pArea.max_vnum; i++)
            {
                var pMprog = Db.get_mprog_index(i);
                if (pMprog != null)
                {
                    fp.Write(RomString.sprintf("#%d\n", i));
                    fp.Write(RomString.sprintf("%s~\n", fix_string(pMprog.code)));
                }
            }

            fp.Write(RomString.sprintf("#0\n\n"));
        }

        /*****************************************************************************
         Name:        save_mobile
         Purpose:    Save one mobile to file, new format -- Hugin
         Called by:    save_mobiles (below).
         ****************************************************************************/
        static void save_mobile(StreamWriter fp, MobIndexData pMobIndex)
        {
            int race = pMobIndex.race;
            long temp;

            fp.Write(RomString.sprintf("#%d\n", pMobIndex.vnum));
            fp.Write(RomString.sprintf("%s~\n", pMobIndex.player_name));
            fp.Write(RomString.sprintf("%s~\n", pMobIndex.short_descr));
            fp.Write(RomString.sprintf("%s~\n", fix_string(pMobIndex.long_descr)));
            fp.Write(RomString.sprintf("%s~\n", fix_string(pMobIndex.description)));
            fp.Write(RomString.sprintf("%s~\n", Tables.race_table[race].name));
            fp.Write(RomString.sprintf("%s ", fwrite_flag(pMobIndex.act)));
            fp.Write(RomString.sprintf("%s ", fwrite_flag(pMobIndex.affected_by)));
            fp.Write(RomString.sprintf("%d %d\n", pMobIndex.alignment, pMobIndex.group));
            fp.Write(RomString.sprintf("%d ", pMobIndex.level));
            fp.Write(RomString.sprintf("%d ", pMobIndex.hitroll));
            fp.Write(RomString.sprintf("%dd%d+%d ", pMobIndex.hit[DICE_NUMBER],
                pMobIndex.hit[DICE_TYPE], pMobIndex.hit[DICE_BONUS]));
            fp.Write(RomString.sprintf("%dd%d+%d ", pMobIndex.mana[DICE_NUMBER],
                pMobIndex.mana[DICE_TYPE], pMobIndex.mana[DICE_BONUS]));
            fp.Write(RomString.sprintf("%dd%d+%d ", pMobIndex.damage[DICE_NUMBER],
                pMobIndex.damage[DICE_TYPE], pMobIndex.damage[DICE_BONUS]));
            fp.Write(RomString.sprintf("%s\n", Tables.attack_table[pMobIndex.dam_type].name));
            fp.Write(RomString.sprintf("%d %d %d %d\n",
                pMobIndex.ac[AC_PIERCE] / 10,
                pMobIndex.ac[AC_BASH] / 10,
                pMobIndex.ac[AC_SLASH] / 10, pMobIndex.ac[AC_EXOTIC] / 10));
            fp.Write(RomString.sprintf("%s ", fwrite_flag(pMobIndex.off_flags)));
            fp.Write(RomString.sprintf("%s ", fwrite_flag(pMobIndex.imm_flags)));
            fp.Write(RomString.sprintf("%s ", fwrite_flag(pMobIndex.res_flags)));
            fp.Write(RomString.sprintf("%s\n", fwrite_flag(pMobIndex.vuln_flags)));
            fp.Write(RomString.sprintf("%s %s %s %ld\n",
                Tables.position_table[pMobIndex.start_pos].short_name,
                Tables.position_table[pMobIndex.default_pos].short_name,
                Tables.sex_table[pMobIndex.sex].name, pMobIndex.wealth));
            fp.Write(RomString.sprintf("%s ", fwrite_flag(pMobIndex.form)));
            fp.Write(RomString.sprintf("%s ", fwrite_flag(pMobIndex.parts)));

            fp.Write(RomString.sprintf("%s ", Tables.size_table[pMobIndex.size].name));
            fp.Write(RomString.sprintf("%s\n",
                Bit.IS_NULLSTR(pMobIndex.material) ? pMobIndex.material : "unknown"));

            if ((temp = DIF(Tables.race_table[race].act, pMobIndex.act)) != 0)
                fp.Write(RomString.sprintf("F act %s\n", fwrite_flag(temp)));

            if ((temp = DIF(Tables.race_table[race].aff, pMobIndex.affected_by)) != 0)
                fp.Write(RomString.sprintf("F aff %s\n", fwrite_flag(temp)));

            if ((temp = DIF(Tables.race_table[race].off, pMobIndex.off_flags)) != 0)
                fp.Write(RomString.sprintf("F off %s\n", fwrite_flag(temp)));

            if ((temp = DIF(Tables.race_table[race].imm, pMobIndex.imm_flags)) != 0)
                fp.Write(RomString.sprintf("F imm %s\n", fwrite_flag(temp)));

            if ((temp = DIF(Tables.race_table[race].res, pMobIndex.res_flags)) != 0)
                fp.Write(RomString.sprintf("F res %s\n", fwrite_flag(temp)));

            if ((temp = DIF(Tables.race_table[race].vuln, pMobIndex.vuln_flags)) != 0)
                fp.Write(RomString.sprintf("F vul %s\n", fwrite_flag(temp)));

            if ((temp = DIF(Tables.race_table[race].form, pMobIndex.form)) != 0)
                fp.Write(RomString.sprintf("F for %s\n", fwrite_flag(temp)));

            if ((temp = DIF(Tables.race_table[race].parts, pMobIndex.parts)) != 0)
                fp.Write(RomString.sprintf("F par %s\n", fwrite_flag(temp)));

            for (var pMprog = pMobIndex.mprogs; pMprog != null; pMprog = pMprog.next)
            {
                fp.Write(RomString.sprintf("M %s %d %s~\n",
                    MobCmds.mprog_type_to_name(pMprog.trig_type), pMprog.vnum,
                    pMprog.trig_phrase));
            }
        }

        /*****************************************************************************
         Name:        save_mobiles
         Purpose:    Save #MOBILES secion of an area file.
         Called by:    save_area(olc_save.c).
         Notes:         Changed for ROM OLC.
         ****************************************************************************/
        static void save_mobiles(StreamWriter fp, AreaData pArea)
        {
            fp.Write(RomString.sprintf("#MOBILES\n"));

            for (int i = pArea.min_vnum; i <= pArea.max_vnum; i++)
            {
                var pMob = Handler.get_mob_index(i);
                if (pMob != null)
                    save_mobile(fp, pMob);
            }

            fp.Write(RomString.sprintf("#0\n\n\n\n"));
        }

        /*****************************************************************************
         Name:        save_object
         Purpose:    Save one object to file.
                        new ROM format saving -- Hugin
         Called by:    save_objects (below).
         ****************************************************************************/
        static void save_object(StreamWriter fp, ObjIndexData pObjIndex)
        {
            char letter;

            fp.Write(RomString.sprintf("#%d\n", pObjIndex.vnum));
            fp.Write(RomString.sprintf("%s~\n", pObjIndex.name));
            fp.Write(RomString.sprintf("%s~\n", pObjIndex.short_descr));
            fp.Write(RomString.sprintf("%s~\n", fix_string(pObjIndex.description)));
            fp.Write(RomString.sprintf("%s~\n", pObjIndex.material));
            fp.Write(RomString.sprintf("%s ", Handler.item_name(pObjIndex.item_type)));
            fp.Write(RomString.sprintf("%s ", fwrite_flag(pObjIndex.extra_flags)));
            fp.Write(RomString.sprintf("%s\n", fwrite_flag(pObjIndex.wear_flags)));

            /*
             *  Using fwrite_flag to write most values gives a strange
             *  looking area file, consider making a case for each
             *  item type later.
             */

            switch (pObjIndex.item_type)
            {
                default:
                    fp.Write(RomString.sprintf("%s ", fwrite_flag(pObjIndex.value[0])));
                    fp.Write(RomString.sprintf("%s ", fwrite_flag(pObjIndex.value[1])));
                    fp.Write(RomString.sprintf("%s ", fwrite_flag(pObjIndex.value[2])));
                    fp.Write(RomString.sprintf("%s ", fwrite_flag(pObjIndex.value[3])));
                    fp.Write(RomString.sprintf("%s\n", fwrite_flag(pObjIndex.value[4])));
                    break;

                case ITEM_DRINK_CON:
                case ITEM_FOUNTAIN:
                    fp.Write(RomString.sprintf("%d %d '%s' %d %d\n",
                        pObjIndex.value[0],
                        pObjIndex.value[1],
                        Tables.liq_table[pObjIndex.value[2]].liq_name,
                        pObjIndex.value[3], pObjIndex.value[4]));
                    break;

                case ITEM_CONTAINER:
                    fp.Write(RomString.sprintf("%d %s %d %d %d\n",
                        pObjIndex.value[0],
                        fwrite_flag(pObjIndex.value[1]),
                        pObjIndex.value[2],
                        pObjIndex.value[3], pObjIndex.value[4]));
                    break;

                case ITEM_WEAPON:
                    fp.Write(RomString.sprintf("%s %d %d %s %s\n",
                        Handler.weapon_name(pObjIndex.value[0]),
                        pObjIndex.value[1],
                        pObjIndex.value[2],
                        Tables.attack_table[pObjIndex.value[3]].name,
                        fwrite_flag(pObjIndex.value[4])));
                    break;

                case ITEM_PILL:
                case ITEM_POTION:
                case ITEM_SCROLL:
                    fp.Write(RomString.sprintf("%d '%s' '%s' '%s' '%s'\n", pObjIndex.value[0] > 0 ?    /* no negative numbers */
                        pObjIndex.value[0]
                        : 0,
                        pObjIndex.value[1] != -1 ?
                        Tables.skill_table[pObjIndex.value[1]].name
                        : "",
                        pObjIndex.value[2] != -1 ?
                        Tables.skill_table[pObjIndex.value[2]].name
                        : "",
                        pObjIndex.value[3] != -1 ?
                        Tables.skill_table[pObjIndex.value[3]].name
                        : "",
                        pObjIndex.value[4] != -1 ?
                        Tables.skill_table[pObjIndex.value[4]].name : ""));
                    break;

                case ITEM_STAFF:
                case ITEM_WAND:
                    fp.Write(RomString.sprintf("%d %d %d '%s' %d\n",
                        pObjIndex.value[0],
                        pObjIndex.value[1],
                        pObjIndex.value[2],
                        pObjIndex.value[3] != -1 ?
                        Tables.skill_table[pObjIndex.value[3]].name :
                        "", pObjIndex.value[4]));
                    break;
            }

            fp.Write(RomString.sprintf("%d ", pObjIndex.level));
            fp.Write(RomString.sprintf("%d ", pObjIndex.weight));
            fp.Write(RomString.sprintf("%d ", pObjIndex.cost));

            if (pObjIndex.condition > 90)
                letter = 'P';
            else if (pObjIndex.condition > 75)
                letter = 'G';
            else if (pObjIndex.condition > 50)
                letter = 'A';
            else if (pObjIndex.condition > 25)
                letter = 'W';
            else if (pObjIndex.condition > 10)
                letter = 'D';
            else if (pObjIndex.condition > 0)
                letter = 'B';
            else
                letter = 'R';

            fp.Write(RomString.sprintf("%c\n", letter));

            for (var pAf = pObjIndex.affected; pAf != null; pAf = pAf.next)
            {
                if (pAf.where == TO_OBJECT || pAf.bitvector == 0)
                    fp.Write(RomString.sprintf("A\n%d %d\n", pAf.location, pAf.modifier));
                else
                {
                    fp.Write(RomString.sprintf("F\n"));

                    switch (pAf.where)
                    {
                        case TO_AFFECTS:
                            fp.Write(RomString.sprintf("A "));
                            break;
                        case TO_IMMUNE:
                            fp.Write(RomString.sprintf("I "));
                            break;
                        case TO_RESIST:
                            fp.Write(RomString.sprintf("R "));
                            break;
                        case TO_VULN:
                            fp.Write(RomString.sprintf("V "));
                            break;
                        default:
                            Db.bug("olc_save: Invalid Affect->where", 0);
                            break;
                    }

                    fp.Write(RomString.sprintf("%d %d %s\n", pAf.location, pAf.modifier,
                        fwrite_flag(pAf.bitvector)));
                }
            }

            for (var pEd = pObjIndex.extra_descr; pEd != null; pEd = pEd.next)
            {
                fp.Write(RomString.sprintf("E\n%s~\n%s~\n", pEd.keyword,
                    fix_string(pEd.description)));
            }
        }

        /*****************************************************************************
         Name:        save_objects
         Purpose:    Save #OBJECTS section of an area file.
         Called by:    save_area(olc_save.c).
         Notes:         Changed for ROM OLC.
         ****************************************************************************/
        static void save_objects(StreamWriter fp, AreaData pArea)
        {
            fp.Write(RomString.sprintf("#OBJECTS\n"));

            for (int i = pArea.min_vnum; i <= pArea.max_vnum; i++)
            {
                var pObj = Handler.get_obj_index(i);
                if (pObj != null)
                    save_object(fp, pObj);
            }

            fp.Write(RomString.sprintf("#0\n\n\n\n"));
        }

        /*****************************************************************************
         Name:        save_rooms
         Purpose:    Save #ROOMS section of an area file.
         Called by:    save_area(olc_save.c).
         ****************************************************************************/
        static void save_rooms(StreamWriter fp, AreaData pArea)
        {
            fp.Write(RomString.sprintf("#ROOMS\n"));
            for (int iHash = 0; iHash < MAX_KEY_HASH; iHash++)
            {
                for (var pRoomIndex = Game.room_index_hash[iHash]; pRoomIndex != null;
                     pRoomIndex = pRoomIndex.next)
                {
                    if (pRoomIndex.area == pArea)
                    {
                        fp.Write(RomString.sprintf("#%d\n", pRoomIndex.vnum));
                        fp.Write(RomString.sprintf("%s~\n", pRoomIndex.name));
                        fp.Write(RomString.sprintf("%s~\n", fix_string(pRoomIndex.description)));
                        fp.Write(RomString.sprintf("0 "));
                        fp.Write(RomString.sprintf("%d ", pRoomIndex.room_flags));
                        fp.Write(RomString.sprintf("%d\n", pRoomIndex.sector_type));

                        for (var pEd = pRoomIndex.extra_descr; pEd != null; pEd = pEd.next)
                        {
                            fp.Write(RomString.sprintf("E\n%s~\n%s~\n", pEd.keyword,
                                fix_string(pEd.description)));
                        }
                        for (int door = 0; door < MAX_DIR; door++)
                        {                /* I hate this! */
                            var pExit = pRoomIndex.exit[door];
                            if (pExit != null && pExit.to_room != null)
                            {
                                int locks = 0;

                                /* HACK : TO PREVENT EX_LOCKED etc without EX_ISDOOR
                                   to stop booting the mud */
                                if (Bit.IS_SET(pExit.rs_flags, EX_CLOSED)
                                    || Bit.IS_SET(pExit.rs_flags, EX_LOCKED)
                                    || Bit.IS_SET(pExit.rs_flags, EX_PICKPROOF)
                                    || Bit.IS_SET(pExit.rs_flags, EX_NOPASS)
                                    || Bit.IS_SET(pExit.rs_flags, EX_EASY)
                                    || Bit.IS_SET(pExit.rs_flags, EX_HARD)
                                    || Bit.IS_SET(pExit.rs_flags, EX_INFURIATING)
                                    || Bit.IS_SET(pExit.rs_flags, EX_NOCLOSE)
                                    || Bit.IS_SET(pExit.rs_flags, EX_NOLOCK))
                                    Bit.SET_BIT(ref pExit.rs_flags, EX_ISDOOR);
                                else
                                    Bit.REMOVE_BIT(ref pExit.rs_flags, EX_ISDOOR);

                                /* THIS SUCKS but it's backwards compatible */
                                /* NOTE THAT EX_NOCLOSE NOLOCK etc aren't being saved */
                                if (Bit.IS_SET(pExit.rs_flags, EX_ISDOOR)
                                    && (!Bit.IS_SET(pExit.rs_flags, EX_PICKPROOF))
                                    && (!Bit.IS_SET(pExit.rs_flags, EX_NOPASS)))
                                    locks = 1;
                                if (Bit.IS_SET(pExit.rs_flags, EX_ISDOOR)
                                    && (Bit.IS_SET(pExit.rs_flags, EX_PICKPROOF))
                                    && (!Bit.IS_SET(pExit.rs_flags, EX_NOPASS)))
                                    locks = 2;
                                if (Bit.IS_SET(pExit.rs_flags, EX_ISDOOR)
                                    && (!Bit.IS_SET(pExit.rs_flags, EX_PICKPROOF))
                                    && (Bit.IS_SET(pExit.rs_flags, EX_NOPASS)))
                                    locks = 3;
                                if (Bit.IS_SET(pExit.rs_flags, EX_ISDOOR)
                                    && (Bit.IS_SET(pExit.rs_flags, EX_PICKPROOF))
                                    && (Bit.IS_SET(pExit.rs_flags, EX_NOPASS)))
                                    locks = 4;

                                fp.Write(RomString.sprintf("D%d\n", pExit.orig_door));
                                fp.Write(RomString.sprintf("%s~\n",
                                    fix_string(pExit.description)));
                                fp.Write(RomString.sprintf("%s~\n", pExit.keyword));
                                fp.Write(RomString.sprintf("%d %d %d\n", locks,
                                    pExit.key, pExit.to_room.vnum));
                            }
                        }
                        if (pRoomIndex.mana_rate != 100
                            || pRoomIndex.heal_rate != 100)
                            fp.Write(RomString.sprintf("M %d H %d\n",
                                pRoomIndex.mana_rate,
                                pRoomIndex.heal_rate));
                        if (pRoomIndex.clan > 0)
                            fp.Write(RomString.sprintf("C %s~\n",
                                Tables.clan_table[pRoomIndex.clan].name));

                        if (!Bit.IS_NULLSTR(pRoomIndex.owner))
                            fp.Write(RomString.sprintf("O %s~\n", pRoomIndex.owner));

                        fp.Write(RomString.sprintf("S\n"));
                    }
                }
            }
            fp.Write(RomString.sprintf("#0\n\n\n\n"));
        }

        /*****************************************************************************
         Name:        save_specials
         Purpose:    Save #SPECIALS section of area file.
         Called by:    save_area(olc_save.c).
         ****************************************************************************/
        static void save_specials(StreamWriter fp, AreaData pArea)
        {
            fp.Write(RomString.sprintf("#SPECIALS\n"));

            for (int iHash = 0; iHash < MAX_KEY_HASH; iHash++)
            {
                for (var pMobIndex = Game.mob_index_hash[iHash]; pMobIndex != null;
                     pMobIndex = pMobIndex.next)
                {
                    if (pMobIndex != null && pMobIndex.area == pArea && pMobIndex.spec_fun != null)
                    {
                        fp.Write(RomString.sprintf("M %d %s\n", pMobIndex.vnum,
                            Special.spec_name(pMobIndex.spec_fun)));
                    }
                }
            }

            fp.Write(RomString.sprintf("S\n\n\n\n"));
        }

        /*
         * This function is obsolete.  It it not needed but has been left here
         * for historical reasons.  It is used currently for the same reason.
         *
         * I don't think it's obsolete in ROM -- Hugin.
         */
        static void save_door_resets(StreamWriter fp, AreaData pArea)
        {
            for (int iHash = 0; iHash < MAX_KEY_HASH; iHash++)
            {
                for (var pRoomIndex = Game.room_index_hash[iHash]; pRoomIndex != null;
                     pRoomIndex = pRoomIndex.next)
                {
                    if (pRoomIndex.area == pArea)
                    {
                        for (int door = 0; door < MAX_DIR; door++)
                        {
                            var pExit = pRoomIndex.exit[door];
                            if (pExit != null
                                && pExit.to_room != null
                                && (Bit.IS_SET(pExit.rs_flags, EX_CLOSED)
                                    || Bit.IS_SET(pExit.rs_flags, EX_LOCKED)))
                                fp.Write(RomString.sprintf("D 0 %d %d %d\n",
                                    pRoomIndex.vnum,
                                    pExit.orig_door,
                                    Bit.IS_SET(pExit.rs_flags, EX_LOCKED) ? 2 : 1));
                        }
                    }
                }
            }
        }

        /*****************************************************************************
         Name:        save_resets
         Purpose:    Saves the #RESETS section of an area file.
         Called by:    save_area(olc_save.c)
         ****************************************************************************/
        static void save_resets(StreamWriter fp, AreaData pArea)
        {
            MobIndexData pLastMob = null;
            ObjIndexData pLastObj;
            string buf;

            fp.Write(RomString.sprintf("#RESETS\n"));

            save_door_resets(fp, pArea);

            for (int iHash = 0; iHash < MAX_KEY_HASH; iHash++)
            {
                for (var pRoom = Game.room_index_hash[iHash]; pRoom != null; pRoom = pRoom.next)
                {
                    if (pRoom.area == pArea)
                    {
                        for (var pReset = pRoom.reset_first; pReset != null;
                             pReset = pReset.next)
                        {
                            switch (pReset.command)
                            {
                                default:
                                    Db.bug("Save_resets: bad command %c.",
                                        pReset.command);
                                    break;

                                case 'M':
                                    pLastMob = Handler.get_mob_index(pReset.arg1);
                                    fp.Write(RomString.sprintf("M 0 %d %d %d %d Load %s\n",
                                        pReset.arg1,
                                        pReset.arg2,
                                        pReset.arg3,
                                        pReset.arg4, pLastMob.short_descr));
                                    break;

                                case 'O':
                                    pLastObj = Handler.get_obj_index(pReset.arg1);
                                    var oRoom = Handler.get_room_index(pReset.arg3);
                                    fp.Write(RomString.sprintf("O 0 %d 0 %d %s loaded to %s\n",
                                        pReset.arg1,
                                        pReset.arg3,
                                        RomString.capitalize(pLastObj.short_descr),
                                        oRoom.name));
                                    break;

                                case 'P':
                                    pLastObj = Handler.get_obj_index(pReset.arg1);
                                    fp.Write(RomString.sprintf("P 0 %d %d %d %d %s put inside %s\n",
                                        pReset.arg1,
                                        pReset.arg2,
                                        pReset.arg3,
                                        pReset.arg4,
                                        RomString.capitalize(Handler.get_obj_index
                                            (pReset.arg1).short_descr),
                                        pLastObj.short_descr));
                                    break;

                                case 'G':
                                    fp.Write(RomString.sprintf("G 0 %d 0 %s is given to %s\n",
                                        pReset.arg1,
                                        RomString.capitalize(Handler.get_obj_index
                                            (pReset.arg1).short_descr),
                                        pLastMob != null ? pLastMob.short_descr :
                                        "!NO_MOB!"));
                                    if (pLastMob == null)
                                    {
                                        buf = RomString.sprintf("Save_resets: !NO_MOB! in [%s]",
                                            pArea.file_name);
                                        Db.bug(buf, 0);
                                    }
                                    break;

                                case 'E':
                                    fp.Write(RomString.sprintf(
                                        "E 0 %d 0 %d %s is loaded %s of %s\n",
                                        pReset.arg1, pReset.arg3,
                                        RomString.capitalize(Handler.get_obj_index
                                            (pReset.arg1).short_descr),
                                        Lookup.flag_string(Tables.wear_loc_strings,
                                            pReset.arg3),
                                        pLastMob != null ? pLastMob.short_descr :
                                        "!NO_MOB!"));
                                    if (pLastMob == null)
                                    {
                                        buf = RomString.sprintf("Save_resets: !NO_MOB! in [%s]",
                                            pArea.file_name);
                                        Db.bug(buf, 0);
                                    }
                                    break;

                                case 'D':
                                    break;

                                case 'R':
                                    var rRoom = Handler.get_room_index(pReset.arg1);
                                    fp.Write(RomString.sprintf("R 0 %d %d Randomize %s\n",
                                        pReset.arg1, pReset.arg2, rRoom.name));
                                    break;
                            }
                        }
                    }                    /* End if correct area */
                }                        /* End for pRoom */
            }                            /* End for iHash */

            fp.Write(RomString.sprintf("S\n\n\n\n"));
        }

        /*****************************************************************************
         Name:        save_shops
         Purpose:    Saves the #SHOPS section of an area file.
         Called by:    save_area(olc_save.c)
         ****************************************************************************/
        static void save_shops(StreamWriter fp, AreaData pArea)
        {
            fp.Write(RomString.sprintf("#SHOPS\n"));

            for (int iHash = 0; iHash < MAX_KEY_HASH; iHash++)
            {
                for (var pMobIndex = Game.mob_index_hash[iHash]; pMobIndex != null;
                     pMobIndex = pMobIndex.next)
                {
                    if (pMobIndex != null && pMobIndex.area == pArea && pMobIndex.pShop != null)
                    {
                        var pShopIndex = pMobIndex.pShop;

                        fp.Write(RomString.sprintf("%d ", pShopIndex.keeper));
                        for (int iTrade = 0; iTrade < MAX_TRADE; iTrade++)
                        {
                            if (pShopIndex.buy_type[iTrade] != 0)
                            {
                                fp.Write(RomString.sprintf("%d ", pShopIndex.buy_type[iTrade]));
                            }
                            else
                                fp.Write(RomString.sprintf("0 "));
                        }
                        fp.Write(RomString.sprintf("%d %d ", pShopIndex.profit_buy,
                            pShopIndex.profit_sell));
                        fp.Write(RomString.sprintf("%d %d\n", pShopIndex.open_hour,
                            pShopIndex.close_hour));
                    }
                }
            }

            fp.Write(RomString.sprintf("0\n\n\n\n"));
        }

        static void save_helps(StreamWriter fp, HelpArea ha)
        {
            var help = ha.first;

            fp.Write(RomString.sprintf("#HELPS\n"));

            for (; help != null; help = help.next_area)
            {
                fp.Write(RomString.sprintf("%d %s~\n", help.level, help.keyword));
                fp.Write(RomString.sprintf("%s~\n\n", fix_string(help.text)));
            }

            fp.Write(RomString.sprintf("-1 $~\n\n"));

            ha.changed = false;
        }

        static void save_other_helps(CharData ch)
        {
            for (var ha = Game.had_list; ha != null; ha = ha.next)
                if (ha.changed == true)
                {
                    var path = Path.Combine(Game.area_dir, ha.filename);
                    StreamWriter fp;
                    try
                    {
                        fp = new StreamWriter(path);
                    }
                    catch
                    {
                        return;
                    }

                    using (fp)
                    {
                        save_helps(fp, ha);

                        if (ch != null)
                            Comm.printf_to_char(ch, "%s\n\r", ha.filename);

                        fp.Write(RomString.sprintf("#$\n"));
                    }
                }
        }

        /*****************************************************************************
         Name:        save_area
         Purpose:    Save an area, note that this format is new.
         Called by:    do_asave(olc_save.c).
         ****************************************************************************/
        public static void save_area(AreaData pArea)
        {
            var path = Path.Combine(Game.area_dir, pArea.file_name);
            StreamWriter fp;
            try
            {
                fp = new StreamWriter(path);
            }
            catch
            {
                Db.bug("Open_area: fopen", 0);
                return;
            }

            using (fp)
            {
                fp.Write(RomString.sprintf("#AREADATA\n"));
                fp.Write(RomString.sprintf("Name %s~\n", pArea.name));
                fp.Write(RomString.sprintf("Builders %s~\n", fix_string(pArea.builders)));
                fp.Write(RomString.sprintf("VNUMs %d %d\n", pArea.min_vnum, pArea.max_vnum));
                fp.Write(RomString.sprintf("Credits %s~\n", pArea.credits));
                fp.Write(RomString.sprintf("Security %d\n", pArea.security));
                fp.Write(RomString.sprintf("End\n\n\n\n"));

                save_mobiles(fp, pArea);
                save_objects(fp, pArea);
                save_rooms(fp, pArea);
                save_specials(fp, pArea);
                save_resets(fp, pArea);
                save_shops(fp, pArea);
                save_mobprogs(fp, pArea);

                if (pArea.helps != null && pArea.helps.first != null)
                    save_helps(fp, pArea.helps);

                fp.Write(RomString.sprintf("#$\n"));
            }
        }

        /*****************************************************************************
         Name:        do_asave
         Purpose:    Entry point for saving area data.
         Called by:    interpreter(interp.c)
         ****************************************************************************/
        public static void do_asave(CharData ch, string argument)
        {
            AreaData pArea;
            int value, sec;

            if (ch == null)                    /* Do an autosave */
                sec = 9;
            else if (!Bit.IS_NPC(ch))
                sec = ch.pcdata.security;
            else
                sec = 0;
            _ = sec;

            /*    {
                save_area_list();
                for( pArea = area_first; pArea; pArea = pArea->next )
                {
                    save_area( pArea );
                    REMOVE_BIT( pArea->area_flags, AREA_CHANGED );
                }
                return;
                } */

            argument = RomString.smash_tilde(argument);
            string arg1 = argument;

            if (arg1.Length == 0)
            {
                if (ch != null)
                {
                    Comm.send_to_char("Syntax:\n\r", ch);
                    Comm.send_to_char("  asave <vnum>   - saves a particular area\n\r",
                        ch);
                    Comm.send_to_char("  asave list     - saves the area.lst file\n\r",
                        ch);
                    Comm.send_to_char
                        ("  asave area     - saves the area being edited\n\r", ch);
                    Comm.send_to_char("  asave changed  - saves all changed zones\n\r",
                        ch);
                    Comm.send_to_char("  asave world    - saves the world! (db dump)\n\r",
                        ch);
                    Comm.send_to_char("\n\r", ch);
                }

                return;
            }

            /* Snarf the value (which need not be numeric). */
            value = Interp.atoi(arg1);
            if ((pArea = get_area_data(value)) == null && Interp.is_number(arg1))
            {
                if (ch != null)
                    Comm.send_to_char("That area does not exist.\n\r", ch);
                return;
            }

            /* Save area of given vnum. */
            /* ------------------------ */
            if (Interp.is_number(arg1))
            {
                if (ch != null && !Bit.IS_BUILDER(ch, pArea))
                {
                    Comm.send_to_char("You are not a builder for this area.\n\r", ch);
                    return;
                }

                save_area_list();
                save_area(pArea);

                return;
            }

            /* Save the world, only authorized areas. */
            /* -------------------------------------- */
            if (!RomString.str_cmp("world", arg1))
            {
                save_area_list();
                for (pArea = Game.area_first; pArea != null; pArea = pArea.next)
                {
                    /* Builder must be assigned this area. */
                    if (ch != null && !Bit.IS_BUILDER(ch, pArea))
                        continue;

                    save_area(pArea);
                    Bit.REMOVE_BIT(ref pArea.area_flags, AREA_CHANGED);
                }

                if (ch != null)
                    Comm.send_to_char("You saved the world.\n\r", ch);

                save_other_helps(null);

                return;
            }

            /* Save changed areas, only authorized areas. */
            /* ------------------------------------------ */
            if (!RomString.str_cmp("changed", arg1))
            {
                string buf;

                save_area_list();

                if (ch != null)
                    Comm.send_to_char("Saved zones:\n\r", ch);
                else
                    Db.log_string("Saved zones:");

                buf = RomString.sprintf("None.\n\r");

                for (pArea = Game.area_first; pArea != null; pArea = pArea.next)
                {
                    /* Builder must be assigned this area. */
                    if (ch != null && !Bit.IS_BUILDER(ch, pArea))
                        continue;

                    /* Save changed areas. */
                    if (Bit.IS_SET(pArea.area_flags, AREA_CHANGED))
                    {
                        save_area(pArea);
                        buf = RomString.sprintf("%24s - '%s'", pArea.name, pArea.file_name);
                        if (ch != null)
                        {
                            Comm.send_to_char(buf, ch);
                            Comm.send_to_char("\n\r", ch);
                        }
                        else
                            Db.log_string(buf);
                        Bit.REMOVE_BIT(ref pArea.area_flags, AREA_CHANGED);
                    }
                }

                save_other_helps(ch);

                if (!RomString.str_cmp(buf, "None.\n\r"))
                {
                    if (ch != null)
                        Comm.send_to_char(buf, ch);
                    else
                        Db.log_string("None.");
                }

                return;
            }

            /* Save the area.lst file. */
            /* ----------------------- */
            if (!RomString.str_cmp(arg1, "list"))
            {
                save_area_list();
                return;
            }

            /* Save area being edited, if authorized. */
            /* -------------------------------------- */
            if (!RomString.str_cmp(arg1, "area"))
            {
                if (ch == null || ch.desc == null)
                    return;

                /* Is character currently editing. */
                if (ch.desc.editor == ED_NONE)
                {
                    Comm.send_to_char("You are not editing an area, "
                        + "therefore an area vnum is required.\n\r", ch);
                    return;
                }

                /* Find the area to save. */
                switch (ch.desc.editor)
                {
                    case ED_AREA:
                        pArea = (AreaData)ch.desc.pEdit;
                        break;
                    case ED_ROOM:
                        pArea = ch.in_room.area;
                        break;
                    case ED_OBJECT:
                        pArea = ((ObjIndexData)ch.desc.pEdit).area;
                        break;
                    case ED_MOBILE:
                        pArea = ((MobIndexData)ch.desc.pEdit).area;
                        break;
                    case ED_HELP:
                        Comm.send_to_char("Grabando area : ", ch);
                        save_other_helps(ch);
                        return;
                    default:
                        pArea = ch.in_room.area;
                        break;
                }

                if (!Bit.IS_BUILDER(ch, pArea))
                {
                    Comm.send_to_char("You are not a builder for this area.\n\r", ch);
                    return;
                }

                save_area_list();
                save_area(pArea);
                Bit.REMOVE_BIT(ref pArea.area_flags, AREA_CHANGED);
                Comm.send_to_char("Area saved.\n\r", ch);
                return;
            }

            /* Show correct syntax. */
            /* -------------------- */
            if (ch != null)
                do_asave(ch, "");
        }
    }
}
