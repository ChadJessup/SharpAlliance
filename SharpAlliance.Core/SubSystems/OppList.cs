using System;
using System.Diagnostics;
using System.Linq;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.SubSystems;
using static SharpAlliance.Core.Globals;
using static SharpAlliance.Core.IsometricUtils;

namespace SharpAlliance.Core.SubSystems;

public class OppList
{
    public static int DECAY_OPPLIST_VALUE(int value)
    {
        if ((value) >= SEEN_THIS_TURN)
        {
            (value)++;
            if ((value) > OLDEST_SEEN_VALUE)
            {
                (value) = NOT_HEARD_OR_SEEN;
            }
        }
        else
        {
            if ((value) <= HEARD_THIS_TURN)
            {
                (value)--;
                if ((value) < OLDEST_HEARD_VALUE)
                {
                    (value) = NOT_HEARD_OR_SEEN;
                }
            }
        }

        return value;
    }


    public static int AdjustMaxSightRangeForEnvEffects(SOLDIERTYPE? pSoldier, int bLightLevel, int sDistVisible)
    {
        int sNewDist = 0;

        sNewDist = sDistVisible * gbLightSighting[0, bLightLevel] / 100;

        // Adjust it based on weather...
        if (guiEnvWeather.HasFlag(WEATHER_FORECAST.SHOWERS | WEATHER_FORECAST.THUNDERSHOWERS))
        {
            sNewDist = sNewDist * 70 / 100;
        }

        return (sNewDist);
    }

    void SwapBestSightingPositions(int bPos1, int bPos2)
    {
        int ubTemp;

        ubTemp = Globals.gubBestToMakeSighting[bPos1];
        Globals.gubBestToMakeSighting[bPos1] = Globals.gubBestToMakeSighting[bPos2];
        Globals.gubBestToMakeSighting[bPos2] = ubTemp;
    }

    void ReevaluateBestSightingPosition(SOLDIERTYPE? pSoldier, int bInterruptDuelPts)
    {
        int ubLoop, ubLoop2;
        bool fFound = false;
        bool fPointsGotLower = false;

        if (bInterruptDuelPts == Globals.NO_INTERRUPT)
        {
            return;
        }

        if (!(pSoldier.uiStatusFlags.HasFlag(SOLDIER.MONSTER)))
        {
            //gfHumanSawSomeoneInRealtime = true;
        }

        if ((pSoldier.bInterruptDuelPts != Globals.NO_INTERRUPT) && (bInterruptDuelPts < pSoldier.bInterruptDuelPts))
        {
            fPointsGotLower = true;
        }

        if (fPointsGotLower)
        {
            // loop to end of array less 1 entry since we can't swap the last entry out of the array
            for (ubLoop = 0; ubLoop < gubBestToMakeSightingSize - 1; ubLoop++)
            {
                if (pSoldier.ubID == gubBestToMakeSighting[ubLoop])
                {
                    fFound = true;
                    break;
                }
            }

            // this guy has fewer interrupt pts vs another enemy!  reduce position unless in last place
            if (fFound)
            {
                // set new points
                //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("RBSP: reducing points for %d to %d", pSoldier.ubID, bInterruptDuelPts));
                pSoldier.bInterruptDuelPts = bInterruptDuelPts;

                // must percolate him down 
                for (ubLoop2 = ubLoop + 1; ubLoop2 < Globals.gubBestToMakeSightingSize; ubLoop2++)
                {
                    if (gubBestToMakeSighting[ubLoop2] != Globals.NOBODY && Globals.MercPtrs[Globals.gubBestToMakeSighting[ubLoop2 - 1]].bInterruptDuelPts < Globals.MercPtrs[gubBestToMakeSighting[ubLoop2]].bInterruptDuelPts)
                    {
                        SwapBestSightingPositions((int)(ubLoop2 - 1), ubLoop2);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else if (pSoldier.ubID == gubBestToMakeSighting[gubBestToMakeSightingSize - 1])
            {
                // in list but can't be bumped down... set his new points
                // DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("RBSP: reduced points for last individual %d to %d", pSoldier.ubID, bInterruptDuelPts));
                pSoldier.bInterruptDuelPts = bInterruptDuelPts;
            }
        }
        else
        {
            // loop through whole array
            for (ubLoop = 0; ubLoop < gubBestToMakeSightingSize; ubLoop++)
            {
                if (pSoldier.ubID == gubBestToMakeSighting[ubLoop])
                {
                    fFound = true;
                    break;
                }
            }

            if (!fFound)
            {
                for (ubLoop = 0; ubLoop < gubBestToMakeSightingSize; ubLoop++)
                {
                    if ((gubBestToMakeSighting[ubLoop] == Globals.NOBODY) || (bInterruptDuelPts > Globals.MercPtrs[gubBestToMakeSighting[ubLoop]].bInterruptDuelPts))
                    {
                        if (gubBestToMakeSighting[gubBestToMakeSightingSize - 1] != Globals.NOBODY)
                        {
                            Globals.MercPtrs[gubBestToMakeSighting[gubBestToMakeSightingSize - 1]].bInterruptDuelPts = Globals.NO_INTERRUPT;
                            //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("RBSP: resetting points for %d to zilch", pSoldier.ubID));
                        }

                        // set new points
                        //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("RBSP: setting points for %d to %d", pSoldier.ubID, bInterruptDuelPts));
                        pSoldier.bInterruptDuelPts = bInterruptDuelPts;

                        // insert here!
                        for (ubLoop2 = gubBestToMakeSightingSize - 1; ubLoop2 > ubLoop; ubLoop2--)
                        {
                            gubBestToMakeSighting[ubLoop2] = gubBestToMakeSighting[ubLoop2 - 1];
                        }
                        gubBestToMakeSighting[ubLoop] = pSoldier.ubID;
                        break;
                    }
                }
            }
            // else points didn't get lower, so do nothing (because we want to leave each merc with as low int points as possible)
        }

        for (ubLoop = 0; ubLoop < BEST_SIGHTING_ARRAY_SIZE; ubLoop++)
        {
            if ((gubBestToMakeSighting[ubLoop] != Globals.NOBODY))
            {
                //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("RBSP entry %d: %d (%d pts)", ubLoop, gubBestToMakeSighting[ubLoop], Globals.MercPtrs[gubBestToMakeSighting[ubLoop]].bInterruptDuelPts));
            }
        }

    }

    void HandleBestSightingPositionInRealtime()
    {
        // This function is called for handling interrupts when opening a door in non-combat or
        // just sighting in non-combat, deciding who gets the first turn

        int ubLoop;

        if (gfDelayResolvingBestSightingDueToDoor)
        {
            //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, "HBSPIR: skipping due to door flag");
            return;
        }

        if (gubBestToMakeSighting[0] != Globals.NOBODY)
        {
            //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, "HBSPIR called and there is someone in the list");

            //if (gfHumanSawSomeoneInRealtime)
            {
                if (gubBestToMakeSighting[1] == Globals.NOBODY)
                {
                    // award turn
                    EnterCombatMode(Globals.MercPtrs[gubBestToMakeSighting[0]].bTeam);
                }
                else
                {
                    // if 1st and 2nd on same team, or 1st and 3rd on same team, or there IS no 3rd, award turn to 1st
                    if ((Globals.MercPtrs[gubBestToMakeSighting[0]].bTeam == Globals.MercPtrs[gubBestToMakeSighting[1]].bTeam) ||
                                ((gubBestToMakeSighting[2] == Globals.NOBODY) || (Globals.MercPtrs[gubBestToMakeSighting[0]].bTeam == Globals.MercPtrs[gubBestToMakeSighting[2]].bTeam))
                         )
                    {
                        EnterCombatMode(Globals.MercPtrs[gubBestToMakeSighting[0]].bTeam);
                    }
                    else // give turn to 2nd best but interrupt to 1st
                    {
                        //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, "Entering combat mode: turn for 2nd best, int for best");

                        EnterCombatMode(Globals.MercPtrs[gubBestToMakeSighting[1]].bTeam);
                        // 2nd guy loses control
                        AddToIntList(gubBestToMakeSighting[1], false, true);
                        // 1st guy gains control
                        AddToIntList(gubBestToMakeSighting[0], true, true);
                        // done
                        DoneAddingToIntList(Globals.MercPtrs[gubBestToMakeSighting[0]], true, SIGHTINTERRUPT);
                    }
                }
            }

            for (ubLoop = 0; ubLoop < BEST_SIGHTING_ARRAY_SIZE; ubLoop++)
            {
                if (gubBestToMakeSighting[ubLoop] != Globals.NOBODY)
                {
                    Globals.MercPtrs[gubBestToMakeSighting[ubLoop]].bInterruptDuelPts = Globals.NO_INTERRUPT;
                    //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("RBSP: done, resetting points for %d to zilch", Globals.MercPtrs[gubBestToMakeSighting[ubLoop]].ubID));
                }
            }

            for (ubLoop = 0; ubLoop < guiNumMercSlots; ubLoop++)
            {
                if (MercSlots[ubLoop] is not null)
                {
                    //AssertMsg(MercSlots[ubLoop].bInterruptDuelPts == Globals.NO_INTERRUPT, String("%S (%d) still has interrupt pts!", MercSlots[ubLoop].name, MercSlots[ubLoop].ubID));
                }
            }
        }
    }

    void HandleBestSightingPositionInTurnbased()
    {
        // This function is called for handling interrupts when opening a door in turnbased

        int ubLoop, ubLoop2;
        bool fOk = false;

        if (gubBestToMakeSighting[0] != Globals.NOBODY)
        {
            if (Globals.MercPtrs[gubBestToMakeSighting[0]].bTeam != gTacticalStatus.ubCurrentTeam)
            {

                // interrupt!
                for (ubLoop = 0; ubLoop < gubBestToMakeSightingSize; ubLoop++)
                {
                    if (gubBestToMakeSighting[ubLoop] == Globals.NOBODY)
                    {
                        if (gubInterruptProvoker == Globals.NOBODY)
                        {
                            // do nothing (for now) - abort!
                            return;
                        }
                        else
                        {
                            // use this guy as the "interrupted" fellow
                            Globals.gubBestToMakeSighting[ubLoop] = Globals.gubInterruptProvoker;
                            fOk = true;
                            break;
                        }

                    }
                    else if (Globals.MercPtrs[Globals.gubBestToMakeSighting[ubLoop]].bTeam == Globals.gTacticalStatus.ubCurrentTeam)
                    {
                        fOk = true;
                        break;
                    }
                }

                if (fOk)
                {
                    // this is the guy who gets "interrupted"; all else before him interrupted him
                    AddToIntList(Globals.gubBestToMakeSighting[ubLoop], false, true);
                    for (ubLoop2 = 0; ubLoop2 < ubLoop; ubLoop2++)
                    {
                        AddToIntList(Globals.gubBestToMakeSighting[ubLoop2], true, true);
                    }
                    // done
                    DoneAddingToIntList(Globals.MercPtrs[Globals.gubBestToMakeSighting[ubLoop]], true, SIGHTINTERRUPT);
                }

            }
            for (ubLoop = 0; ubLoop < Globals.BEST_SIGHTING_ARRAY_SIZE; ubLoop++)
            {
                if (Globals.gubBestToMakeSighting[ubLoop] != Globals.NOBODY)
                {
                    Globals.MercPtrs[Globals.gubBestToMakeSighting[ubLoop]].bInterruptDuelPts = Globals.NO_INTERRUPT;
                    //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("RBSP (TB): done, resetting points for %d to zilch", Globals.MercPtrs[gubBestToMakeSighting[ubLoop]].ubID));
                }
            }

            for (ubLoop = 0; ubLoop < Globals.guiNumMercSlots; ubLoop++)
            {
                if (Globals.MercSlots[ubLoop] is not null)
                {
                    // AssertMsg(Globals.MercSlots[ubLoop].bInterruptDuelPts == Globals.NO_INTERRUPT, String("%S (%d) still has interrupt pts!", MercSlots[ubLoop].name, MercSlots[ubLoop].ubID));
                }
            }


        }

    }

    void InitSightArrays()
    {
        int uiLoop;

        for (uiLoop = 0; uiLoop < Globals.BEST_SIGHTING_ARRAY_SIZE; uiLoop++)
        {
            Globals.gubBestToMakeSighting[uiLoop] = Globals.NOBODY;
        }
        //gfHumanSawSomeoneInRealtime = false;
    }

    public static void AddToShouldBecomeHostileOrSayQuoteList(int ubID)
    {
        int ubLoop;

        Debug.Assert(Globals.gubNumShouldBecomeHostileOrSayQuote < Globals.SHOULD_BECOME_HOSTILE_SIZE);

        if (Globals.MercPtrs[ubID].bLife < Globals.OKLIFE)
        {
            return;
        }

        // make sure not already in list
        for (ubLoop = 0; ubLoop < Globals.gubNumShouldBecomeHostileOrSayQuote; ubLoop++)
        {
            if (Globals.gubShouldBecomeHostileOrSayQuote[ubLoop] == ubID)
            {
                return;
            }
        }

        Globals.gubShouldBecomeHostileOrSayQuote[Globals.gubNumShouldBecomeHostileOrSayQuote] = ubID;
        Globals.gubNumShouldBecomeHostileOrSayQuote++;
    }

    int SelectSpeakerFromHostileOrSayQuoteList()
    {
        int[] ubProfileList = new int[Globals.SHOULD_BECOME_HOSTILE_SIZE]; // NB list of merc IDs, not profiles!
        int ubLoop, ubNumProfiles = 0;
        SOLDIERTYPE? pSoldier;

        for (ubLoop = 0; ubLoop < Globals.gubNumShouldBecomeHostileOrSayQuote; ubLoop++)
        {
            pSoldier = Globals.MercPtrs[Globals.gubShouldBecomeHostileOrSayQuote[ubLoop]];
            if (pSoldier.ubProfile != NO_PROFILE)
            {

                // make sure person can say quote!!!!
                gMercProfiles[pSoldier.ubProfile].ubMiscFlags2 |= ProfileMiscFlags2.PROFILE_MISC_FLAG2_NEEDS_TO_SAY_HOSTILE_QUOTE;

                if (NPCHasUnusedHostileRecord(pSoldier.ubProfile, APPROACH_DECLARATION_OF_HOSTILITY))
                {
                    ubProfileList[ubNumProfiles] = gubShouldBecomeHostileOrSayQuote[ubLoop];
                    ubNumProfiles++;
                }
                else
                {
                    // turn flag off again
                    gMercProfiles[pSoldier.ubProfile].ubMiscFlags2 &= ~ProfileMiscFlags2.PROFILE_MISC_FLAG2_NEEDS_TO_SAY_HOSTILE_QUOTE;
                }

            }
        }

        if (ubNumProfiles == 0)
        {
            return (Globals.NOBODY);
        }
        else
        {
            return (ubProfileList[Globals.Random.Next(ubNumProfiles)]);
        }
    }

    void CheckHostileOrSayQuoteList()
    {
        if (gubNumShouldBecomeHostileOrSayQuote == 0 || !DialogueQueueIsEmpty() || gfInTalkPanel || gfWaitingForTriggerTimer)
        {
            return;
        }
        else
        {
            int ubSpeaker, ubLoop;
            SOLDIERTYPE? pSoldier;

            ubSpeaker = SelectSpeakerFromHostileOrSayQuoteList();
            if (ubSpeaker == Globals.NOBODY)
            {
                // make sure everyone on this list is hostile
                for (ubLoop = 0; ubLoop < gubNumShouldBecomeHostileOrSayQuote; ubLoop++)
                {
                    pSoldier = Globals.MercPtrs[gubShouldBecomeHostileOrSayQuote[ubLoop]];
                    if (pSoldier.bNeutral > 0)
                    {
                        MakeCivHostile(pSoldier, 2);
                        // make civ group, if any, hostile
                        if (pSoldier.bTeam == TEAM.CIV_TEAM
                            && pSoldier.ubCivilianGroup != CIV_GROUP.NON_CIV_GROUP
                            && gTacticalStatus.fCivGroupHostile[pSoldier.ubCivilianGroup] == CIV_GROUP_WILL_BECOME_HOSTILE)
                        {
                            gTacticalStatus.fCivGroupHostile[pSoldier.ubCivilianGroup] = CIV_GROUP_HOSTILE;
                        }
                    }
                }

                // unpause all AI
                UnPauseAI();
                // reset the list 
                gubShouldBecomeHostileOrSayQuote = new int[SHOULD_BECOME_HOSTILE_SIZE];
                gubNumShouldBecomeHostileOrSayQuote = 0;
                //and return/go into combat
                if (!(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
                {
                    EnterCombatMode(CIV_TEAM);
                }
            }
            else
            {
                // pause all AI
                PauseAIUntilManuallyUnpaused();
                // stop everyone?

                // We want to make this guy visible to the player.
                if (Globals.MercPtrs[ubSpeaker].bVisible == 0)
                {
                    gbPublicOpplist[gbPlayerNum][ubSpeaker] = HEARD_THIS_TURN;
                    HandleSight(Globals.MercPtrs[ubSpeaker], SIGHT.LOOK | SIGHT.RADIO);
                }

                // trigger hater
                TriggerNPCWithIHateYouQuote(Globals.MercPtrs[ubSpeaker].ubProfile);
            }
        }
    }

    void HandleSight(SOLDIERTYPE? pSoldier, SIGHT ubSightFlags)
    {
        int uiLoop;
        SOLDIERTYPE? pThem;
        int bTempNewSituation;

        if (!pSoldier.bActive || !pSoldier.bInSector || pSoldier.uiStatusFlags.HasFlag(SOLDIER.DEAD))
        {
            // I DON'T THINK SO!
            return;
        }

        gubSightFlags = ubSightFlags;

        if (gubBestToMakeSightingSize != BEST_SIGHTING_ARRAY_SIZE_ALL_TEAMS_LOOK_FOR_ALL)
        {
            // if this is not being called as a result of all teams look for all, reset array size
            if ((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
            {
                // NB the incombat size is 0
                gubBestToMakeSightingSize = BEST_SIGHTING_ARRAY_SIZE_INCOMBAT;
            }
            else
            {
                gubBestToMakeSightingSize = BEST_SIGHTING_ARRAY_SIZE_NONCOMBAT;
            }

            InitSightArrays();
        }

        for (uiLoop = 0; uiLoop < NUM_WATCHED_LOCS; uiLoop++)
        {
            gfWatchedLocHasBeenIncremented[pSoldier.ubID, uiLoop] = false;
        }

        gfPlayerTeamSawCreatures = false;

        // store new situation value
        bTempNewSituation = pSoldier.bNewSituation;
        pSoldier.bNewSituation = 0;

        // if we've been told to make this soldier look (& others look back at him)
        if (ubSightFlags.HasFlag(SIGHT.LOOK))
        {

            // if this soldier's under our control and well enough to look
            if (pSoldier.bLife >= OKLIFE)
            {
                /*
       #if RECORDOPPLIST
            fprintf(OpplistFile,"ManLooksForOtherTeams (HandleSight/Look) for %d\n",pSoldier.guynum);
       #endif
               */
                // he looks for all other soldiers not on his own team
                ManLooksForOtherTeams(pSoldier);

                // if "Show only enemies seen" option is ON and it's this guy looking
                //if (pSoldier.ubID == ShowOnlySeenPerson)
                //NewShowOnlySeenPerson(pSoldier);                  // update the string
            }


            /*
       #if RECORDOPPLIST
          fprintf(OpplistFile,"OtherTeamsLookForMan (HandleSight/Look) for %d\n",ptr.guynum);
       #endif
            */

            // all soldiers under our control but not on ptr's team look for him
            OtherTeamsLookForMan(pSoldier);
        } // end of SIGHT_LOOK

        // if we've been told that interrupts are possible as a result of sighting
        if ((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED))
           && (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
           && (ubSightFlags.HasFlag(SIGHT.INTERRUPT)))
        {
            ResolveInterruptsVs(pSoldier, SIGHTINTERRUPT);
        }

        if (gubBestToMakeSightingSize == BEST_SIGHTING_ARRAY_SIZE_NONCOMBAT)
        {
            HandleBestSightingPositionInRealtime();
        }

        if (pSoldier.bNewSituation > 0 && !(pSoldier.uiStatusFlags.HasFlag(SOLDIER.PC)))
        {
            HaultSoldierFromSighting(pSoldier, true);
        }
        pSoldier.bNewSituation = Math.Max(pSoldier.bNewSituation, bTempNewSituation);

        // if we've been told to radio the results
        if (ubSightFlags.HasFlag(SIGHT.RADIO))
        {
            if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.PC))
            {
                // update our team's public knowledge
                RadioSightings(pSoldier, EVERYBODY, pSoldier.bTeam);

                // if it's our local player's merc
                if (PTR_OURTEAM(pSoldier))
                {
                    // revealing roofs and looking for items handled here, too
                    RevealRoofsAndItems(pSoldier, true, true, pSoldier.bLevel, false);
                }
            }
            // unless in easy mode allow alerted enemies to radio
            else if (gGameOptions.ubDifficultyLevel >= DifficultyLevel.Medium)
            {
                // don't allow admins to radio
                if (pSoldier.bTeam == TEAM.ENEMY_TEAM && gTacticalStatus.Team[TEAM.ENEMY_TEAM].bAwareOfOpposition > 0 && pSoldier.ubSoldierClass != SOLDIER_CLASS.ADMINISTRATOR)
                {
                    RadioSightings(pSoldier, EVERYBODY, pSoldier.bTeam);
                }
            }

            pSoldier.bNewOppCnt = 0;
            pSoldier.bNeedToLook = 0;


            // Temporary for opplist synching - disable random order radioing

            // all non-humans under our control would now radio, if they were allowed
            // to radio automatically (but they're not).  So just nuke new opp cnt
            // NEW: under LOCALOPPLIST, humans on other teams now also radio in here
            for (uiLoop = 0; uiLoop < guiNumMercSlots; uiLoop++)
            {
                pThem = MercSlots[uiLoop];

                if (pThem != null && pThem.bLife >= OKLIFE)
                {
                    // if this merc is on the same team as the target soldier
                    if (pThem.bTeam == pSoldier.bTeam)
                    {
                        continue;        // he doesn't look (he ALWAYS knows about him)
                    }

                    // other human team's merc report sightings to their teams now
                    if (pThem.uiStatusFlags.HasFlag(SOLDIER.PC))
                    {
                        // Temporary for opplist synching - disable random order radioing
                        // exclude our own team, we've already done them, randomly
                        if (pThem.bTeam != gbPlayerNum)
                        {
                            RadioSightings(pThem, pSoldier.ubID, pThem.bTeam);
                        }
                    }
                    // unless in easy mode allow alerted enemies to radio
                    else if (gGameOptions.ubDifficultyLevel >= DifficultyLevel.Medium)
                    {
                        // don't allow admins to radio
                        if (pThem.bTeam == TEAM.ENEMY_TEAM
                            && gTacticalStatus.Team[ENEMY_TEAM].bAwareOfOpposition > 0
                            && pThem.ubSoldierClass.HasFlag(SOLDIER_CLASS.ADMINISTRATOR))
                        {
                            RadioSightings(pThem, EVERYBODY, pThem.bTeam);
                        }
                    }


                    pThem.bNewOppCnt = 0;
                    pThem.bNeedToLook = 0;
                }
            }
        }

        // CJC August 13 2002: at the end of handling sight, reset sight flags to allow interrupts in case an audio cue should
        // cause someone to see an enemy
        gubSightFlags |= SIGHT.INTERRUPT;
    }


    void OurTeamRadiosRandomlyAbout(int ubAbout)
    {
        int iLoop;
        int radioCnt = 0;
        int[] radioMan = new int[20];
        SOLDIERTYPE? pSoldier;


        // Temporary for opplist synching - disable random order radioing
        // # if RECORDOPPLIST
        //         for (iLoop = Status.team[Net.pnum].guystart, ourPtr = Globals.MercPtrs[iLoop]; iLoop < Status.team[Net.pnum].guyend; iLoop++, ourPtr++)
        //         {
        //             // if this merc is active, in this sector, and well enough to look
        //             if (pSoldier.active && pSoldier.bInSector && (pSoldier.bLife >= OKLIFE))
        //             {
        //                 RadioSightings(pSoldier, ubAbout, pSoldier.bTeam);
        //                 pSoldier.bNewOppCnt = 0;
        //             }
        //         }
        // 
        //         return;
        // #endif


        // All mercs on our local team check if they should radio about him
        iLoop = gTacticalStatus.Team[gbPlayerNum].bFirstID;

        // make a list of all of our team's mercs
        for (pSoldier = Globals.MercPtrs[iLoop]; iLoop <= gTacticalStatus.Team[gbPlayerNum].bLastID; iLoop++, pSoldier++)
        {
            // if this merc is active, in this sector, and well enough to look
            if (pSoldier.bActive && pSoldier.bInSector && (pSoldier.bLife >= OKLIFE))
            {
                // put him on our list, and increment the counter
                radioMan[radioCnt++] = (int)iLoop;
            }
        }


        // now RANDOMLY handle each of the mercs on our list, until none remain
        // (this is all being done ONLY so that the mercs in the earliest merc
        //  slots do not arbitrarily get the bulk of the sighting speech quote
        //  action, while the later ones almost never pipe up, and is NOT
        //  strictly necessary, but a nice improvement over original JA)
        while (radioCnt > 0)
        {
            // pick a merc from one of the remaining slots at random
            iLoop = Globals.Random.Next(radioCnt);

            // handle radioing for that merc
            RadioSightings(Globals.MercPtrs[radioMan[iLoop]], ubAbout, Globals.MercPtrs[radioMan[iLoop]].bTeam);
            Menptr[radioMan[iLoop]].bNewOppCnt = 0;

            // unless it WAS the last used slot that we happened to pick
            if (iLoop != (radioCnt - 1))
            {
                // move the contents of the last slot into the one just handled
                radioMan[iLoop] = radioMan[radioCnt - 1];
            }

            radioCnt--;
        }
    }

    public static bool TeamNoLongerSeesMan(TEAM ubTeam, SOLDIERTYPE? pOpponent, int ubExcludeID, int bIteration)
    {
        int bLoop;
        SOLDIERTYPE? pMate;


        bLoop = gTacticalStatus.Team[ubTeam].bFirstID;

        // look for all mercs on the same team, check opplists for this soldier
        for (pMate = Globals.MercPtrs[bLoop]; bLoop <= gTacticalStatus.Team[ubTeam].bLastID; bLoop++, pMate++)
        {
            // if this "teammate" is me, myself, or I (whom we want to exclude)
            if (bLoop == ubExcludeID)
            {
                continue;          // skip to next teammate, I KNOW I don't see him...
            }

            // if this merc is not on the same team
            if (pMate.bTeam != ubTeam)
            {
                continue;  // skip him, he's no teammate at all!
            }

            // if this merc is not active, at base, on assignment, dead, unconscious
            if (!pMate.bActive || !pMate.bInSector || (pMate.bLife < OKLIFE))
            {
                continue;  // next merc
            }

            // if this teammate currently sees this opponent
            if (pMate.bOppList[pOpponent.ubID] == SEEN_CURRENTLY)
            {
                return (false);     // that's all I need to know, get out of here
            }
        }

        // # if WE_SEE_WHAT_MILITIA_SEES_AND_VICE_VERSA
        //         if (bIteration == 0)
        //         {
        //             if (ubTeam == gbPlayerNum && gTacticalStatus.Team[MILITIA_TEAM].bTeamActive)
        //             {
        //                 // check militia team as well
        //                 return (TeamNoLongerSeesMan(MILITIA_TEAM, pOpponent, ubExcludeID, 1));
        //             }
        //             else if (ubTeam == MILITIA_TEAM && gTacticalStatus.Team[gbPlayerNum].bTeamActive)
        //             {
        //                 // check player team as well
        //                 return (TeamNoLongerSeesMan(gbPlayerNum, pOpponent, ubExcludeID, 1));
        //             }
        //         }
        // #endif

        // none of my friends is currently seeing the guy, so return success
        return (true);
    }

    public static int DistanceSmellable(SOLDIERTYPE? pSoldier, SOLDIERTYPE? pSubject)
    {
        int sDistVisible = Globals.STRAIGHT; // as a base

        //if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED))
        //{
        sDistVisible *= 2;
        //}
        //else
        //{

        //	sDistVisible += 3;
        //}

        if (pSubject is not null)
        {
            if (pSubject.uiStatusFlags.HasFlag(SOLDIER.MONSTER))
            {
                // trying to smell a friend; change nothing
            }
            else
            {
                // smelling a human or animal; if they are coated with monster smell, distance shrinks
                sDistVisible = sDistVisible * (pSubject.bNormalSmell - pSubject.bMonsterSmell) / NORMAL_HUMAN_SMELL_STRENGTH;
                if (sDistVisible < 0)
                {
                    sDistVisible = 0;
                }
            }
        }
        return (sDistVisible);
    }

    public static int MaxDistanceVisible()
    {
        return (STRAIGHT * 2);
    }

    public static int DistanceVisible(SOLDIERTYPE? pSoldier, WorldDirections bFacingDir, WorldDirections bSubjectDir, int sSubjectGridNo, int bLevel)
    {
        int sDistVisible;
        int bLightLevel;
        SOLDIERTYPE? pSubject;

        pSubject = SimpleFindSoldier(sSubjectGridNo, bLevel);

        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.MONSTER))
        {
            if (!pSubject)
            {
                return (0);
            }

            return (DistanceSmellable(pSoldier, pSubject));
        }

        if (pSoldier.bBlindedCounter > 0)
        {
            // we're bliiiiiiiiind!!!
            return (0);
        }

        if (bFacingDir == WorldDirections.DIRECTION_IRRELEVANT && TANK(pSoldier))
        {
            // always calculate direction for tanks so we have something to work with
            bFacingDir = pSoldier.bDesiredDirection;
            bSubjectDir = (int)GetDirectionToGridNoFromGridNo(pSoldier.sGridNo, sSubjectGridNo);
            //bSubjectDir = atan8(pSoldier.sX,pSoldier.sY,pOpponent.sX,pOpponent.sY);
        }

        if (!TANK(pSoldier) && (bFacingDir == WorldDirections.DIRECTION_IRRELEVANT || (pSoldier.uiStatusFlags.HasFlag(SOLDIER.ROBOT)) || (pSubject && pSubject.fMuzzleFlash)))
        {
            sDistVisible = MaxDistanceVisible();
        }
        else
        {

            if (pSoldier.sGridNo == sSubjectGridNo)
            {
                // looking up or down or two people accidentally in same tile... don't want it to be 0!
                sDistVisible = MaxDistanceVisible();
            }
            else
            {
                sDistVisible = gbLookDistance[bFacingDir, bSubjectDir];

                if (sDistVisible == ANGLE && (pSoldier.bTeam == OUR_TEAM || pSoldier.bAlertStatus >= STATUS.RED))
                {
                    sDistVisible = STRAIGHT;
                }

                sDistVisible *= 2;

                if (pSoldier.usAnimState == AnimationStates.RUNNING)
                {
                    if (gbLookDistance[bFacingDir, bSubjectDir] != STRAIGHT)
                    {
                        // reduce sight when we're not looking in that direction...
                        // (20%?)
                        sDistVisible = (sDistVisible * 8) / 10;
                    }
                }
            }
        }

        if (pSoldier.bLevel != bLevel)
        {
            // add two tiles distance to visibility to/from roofs
            sDistVisible += 2;
        }

        // now reduce based on light level; SHADE_MIN is the define for the
        // highest number the light can be
        bLightLevel = LightTrueLevel(sSubjectGridNo, bLevel);


        if (pSubject && !(pSubject.fMuzzleFlash && (bLightLevel > NORMAL_LIGHTLEVEL_DAY)))
        {
            // ATE: Made function to adjust light distence...
            sDistVisible = AdjustMaxSightRangeForEnvEffects(pSoldier, bLightLevel, sDistVisible);
        }

        // if we wanted to simulate desert-blindness, we'd bump up the light level 
        // under certain conditions (daytime in the desert, for instance)
        if (bLightLevel < NORMAL_LIGHTLEVEL_DAY)
        {
            // greater than normal daylight level; check for sun goggles
            if (pSoldier.inv[HEAD1POS].usItem == Items.SUNGOGGLES || pSoldier.inv[HEAD2POS].usItem == Items.SUNGOGGLES)
            {
                // increase sighting distance by up to 2 tiles
                sDistVisible++;
                if (bLightLevel < NORMAL_LIGHTLEVEL_DAY - 1)
                {
                    sDistVisible++;
                    ;
                }
            }
        }
        else if (bLightLevel > NORMAL_LIGHTLEVEL_DAY + 5)
        {
            if ((pSoldier.inv[HEAD1POS].usItem == Items.NIGHTGOGGLES
                || pSoldier.inv[HEAD2POS].usItem == Items.NIGHTGOGGLES
                || pSoldier.inv[HEAD1POS].usItem == Items.UVGOGGLES
                || pSoldier.inv[HEAD2POS].usItem == Items.UVGOGGLES)
                || (pSoldier.ubBodyType == SoldierBodyTypes.BLOODCAT
                || AM_A_ROBOT(pSoldier)))
            {
                if (pSoldier.inv[HEAD1POS].usItem == Items.NIGHTGOGGLES || pSoldier.inv[HEAD2POS].usItem == Items.NIGHTGOGGLES || AM_A_ROBOT(pSoldier))
                {
                    if (bLightLevel > NORMAL_LIGHTLEVEL_NIGHT)
                    {
                        // when it gets really dark, light-intensification goggles become less effective
                        if (bLightLevel < NORMAL_LIGHTLEVEL_NIGHT + 3)
                        {
                            sDistVisible += (NIGHTSIGHTGOGGLES_BONUS / 2);
                        }
                        // else no help at all!
                    }
                    else
                    {
                        sDistVisible += NIGHTSIGHTGOGGLES_BONUS;
                    }

                }
                // UV goggles only function above ground... ditto for bloodcats
                else if (gbWorldSectorZ == 0)
                {
                    sDistVisible += UVGOGGLES_BONUS;
                }

            }

            // give one step better vision for people with nightops
            if (HAS_SKILL_TRAIT(pSoldier, NIGHTOPS))
            {
                sDistVisible += 1 * NUM_SKILL_TRAITS(pSoldier, NIGHTOPS);
            }

        }


        // let tanks see and be seen further (at night)
        if ((TANK(pSoldier) && sDistVisible > 0) || (pSubject is not null && TANK(pSubject)))
        {
            sDistVisible = Math.Max(sDistVisible + 5, MaxDistanceVisible());
        }

        if (gpWorldLevelData[pSoldier.sGridNo].ubExtFlags[bLevel].HasFlag((MAPELEMENT_EXT.TEARGAS | MAPELEMENT_EXT.MUSTARDGAS)))
        {
            if (pSoldier.inv[InventorySlot.HEAD1POS].usItem != Items.GASMASK && pSoldier.inv[InventorySlot.HEAD2POS].usItem != Items.GASMASK)
            {
                // in gas without a gas mask; reduce max distance visible to 2 tiles at most
                sDistVisible = Math.Min(sDistVisible, 2);
            }
        }

        return (sDistVisible);
    }

    public static void EndMuzzleFlash(SOLDIERTYPE? pSoldier)
    {
        int uiLoop;
        SOLDIERTYPE? pOtherSoldier;

        pSoldier.fMuzzleFlash = false;

        //# if WE_SEE_WHAT_MILITIA_SEES_AND_VICE_VERSA
        //        if (pSoldier.bTeam != gbPlayerNum && pSoldier.bTeam != MILITIA_TEAM)
        //#else
        if (pSoldier.bTeam != gbPlayerNum)
        {
            pSoldier.bVisible = 0; // indeterminate state
        }

        for (uiLoop = 0; uiLoop < guiNumMercSlots; uiLoop++)
        {
            pOtherSoldier = MercSlots[uiLoop];

            if (pOtherSoldier != null)
            {
                if (pOtherSoldier.bOppList[pSoldier.ubID] == SEEN_CURRENTLY)
                {
                    if (pOtherSoldier.sGridNo != NOWHERE)
                    {
                        if (PythSpacesAway(pOtherSoldier.sGridNo, pSoldier.sGridNo) > DistanceVisible(pOtherSoldier, WorldDirections.DIRECTION_IRRELEVANT, WorldDirections.DIRECTION_IRRELEVANT, pSoldier.sGridNo, pSoldier.bLevel))
                        {
                            // if this guy can no longer see us, change to seen this turn
                            HandleManNoLongerSeen(pOtherSoldier, pSoldier, (pOtherSoldier.bOppList[pSoldier.ubID]), (gbPublicOpplist[pOtherSoldier.bTeam][pSoldier.ubID]));
                        }
                        // else this person is still seen, if the looker is on our side or the militia the person should stay visible
                        //# if WE_SEE_WHAT_MILITIA_SEES_AND_VICE_VERSA
                        //                        else if (pOtherSoldier.bTeam == gbPlayerNum || pOtherSoldier.bTeam == MILITIA_TEAM)
                        //			  #else
                        else if (pOtherSoldier.bTeam == gbPlayerNum)
                        {
                            pSoldier.bVisible = true; // yes, still seen
                        }
                    }
                }
            }
        }
        DecideTrueVisibility(pSoldier, false);

    }

    void TurnOffEveryonesMuzzleFlashes()
    {
        int uiLoop;
        SOLDIERTYPE? pSoldier;

        for (uiLoop = 0; uiLoop < guiNumMercSlots; uiLoop++)
        {
            pSoldier = MercSlots[uiLoop];

            if (pSoldier != null && pSoldier.fMuzzleFlash)
            {
                EndMuzzleFlash(pSoldier);
            }
        }
    }

    void TurnOffTeamsMuzzleFlashes(int ubTeam)
    {
        int ubLoop;
        SOLDIERTYPE? pSoldier;

        for (ubLoop = gTacticalStatus.Team[ubTeam].bFirstID; ubLoop <= gTacticalStatus.Team[ubTeam].bLastID; ubLoop++)
        {
            pSoldier = Globals.MercPtrs[ubLoop];

            if (pSoldier.fMuzzleFlash)
            {
                EndMuzzleFlash(pSoldier);
            }
        }
    }

    int DecideHearing(SOLDIERTYPE? pSoldier)
    {
        // calculate the hearing value for the merc...

        InventorySlot bSlot;
        int bHearing;

        if (TANK(pSoldier))
        {
            return (-5);
        }
        else if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.MONSTER))
        {
            return (-10);
        }

        bHearing = 0;

        if (pSoldier.bExpLevel > 3)
        {
            bHearing++;
        }

        if (HAS_SKILL_TRAIT(pSoldier, NIGHTOPS))
        {
            // sharper hearing generally
            bHearing += 1 * NUM_SKILL_TRAITS(pSoldier, NIGHTOPS);
        }

        bSlot = FindObj(pSoldier, EXTENDEDEAR);
        if (bSlot == HEAD1POS || bSlot == HEAD2POS)
        {
            // at 81-100% adds +5, at 61-80% adds +4, at 41-60% adds +3, etc.
            bHearing += pSoldier.inv[bSlot].bStatus[0] / 20 + 1;
        }

        // adjust for dark conditions	
        switch (ubAmbientLightLevel)
        {
            case 8:
            case 9:
                bHearing += 1;
                break;
            case 10:
                bHearing += 2;
                break;
            case 11:
            case 12:
            case 13:
            case 14:
            case 15:
                bHearing += 3;
                if (HAS_SKILL_TRAIT(pSoldier, NIGHTOPS))
                {
                    // yet another bonus for nighttime
                    bHearing += 1 * NUM_SKILL_TRAITS(pSoldier, NIGHTOPS);
                }
                break;
            default:
                break;
        }

        return (bHearing);
    }

    void InitOpplistForDoorOpening()
    {
        // this is called before generating a noise for opening a door so that
        // the results of hearing the noise are lumped in with the results from AllTeamsLookForAll
        gubBestToMakeSightingSize = BEST_SIGHTING_ARRAY_SIZE_ALL_TEAMS_LOOK_FOR_ALL;
        gfDelayResolvingBestSightingDueToDoor = true; // will be turned off in allteamslookforall
        // DebugMsg(TOPIC_JA2, DBG_LEVEL_3, "HBSPIR: setting door flag on");
        // must init sight arrays here
        InitSightArrays();
    }

    void AllTeamsLookForAll(int ubAllowInterrupts)
    {
        int uiLoop;
        SOLDIERTYPE? pSoldier;

        if ((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.LOADING_SAVED_GAME)))
        {
            return;
        }

        if (ubAllowInterrupts > 0 || !(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
        {
            gubBestToMakeSightingSize = BEST_SIGHTING_ARRAY_SIZE_ALL_TEAMS_LOOK_FOR_ALL;
            if (gfDelayResolvingBestSightingDueToDoor)
            {
                // turn off flag now, and skip init of sight arrays
                //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, "HBSPIR: turning door flag off");
                gfDelayResolvingBestSightingDueToDoor = false;
            }
            else
            {
                InitSightArrays();
            }
        }

        for (uiLoop = 0; uiLoop < guiNumMercSlots; uiLoop++)
        {
            pSoldier = MercSlots[uiLoop];

            if (pSoldier != null && pSoldier.bLife >= OKLIFE)
            {
                HandleSight(pSoldier, SIGHT.LOOK);  // no radio or interrupts yet
            }
        }

        // the player team now radios about all sightings
        for (uiLoop = gTacticalStatus.Team[gbPlayerNum].bFirstID; uiLoop <= gTacticalStatus.Team[gbPlayerNum].bLastID; uiLoop++)
        {
            HandleSight(Globals.MercPtrs[uiLoop], SIGHT_RADIO);      // looking was done above
        }

        if (!(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
        {
            // decide who should get first turn
            HandleBestSightingPositionInRealtime();
            // this could have made us switch to combat mode
            if ((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
            {
                gubBestToMakeSightingSize = BEST_SIGHTING_ARRAY_SIZE_INCOMBAT;
            }
            else
            {
                gubBestToMakeSightingSize = BEST_SIGHTING_ARRAY_SIZE_NONCOMBAT;
            }
        }
        else if (ubAllowInterrupts > 0)
        {
            HandleBestSightingPositionInTurnbased();
            // reset sighting size to 0
            gubBestToMakeSightingSize = BEST_SIGHTING_ARRAY_SIZE_INCOMBAT;
        }

        /*

        // do this here as well as in overhead so the looks/interrupts are combined!

        // if a door was recently opened/closed (doesn't matter if we could see it)
        // this is done here so we can first handle everyone looking through the
        // door, and deal with the resulting opplist changes, interrupts, etc.
        if (Status.doorCreakedGridno != NOWHERE)
         {
          // opening/closing a door makes a bit of noise (constant volume)
          MakeNoise(Status.doorCreakedGuynum,Status.doorCreakedGridno,TTypeList[Grid[Status.doorCreakedGridno].land],DOOR_NOISE.VOLUME,NOISE.CREAKING,EXPECTED_NOSEND);

          Status.doorCreakedGridno = NOWHERE;
          Status.doorCreakedGuynum = Globals.NOBODY;
         }


        // all soldiers now radio their findings (NO interrupts permitted this early!)
        // NEW: our entire team must radio first, so that they radio about EVERYBODY
        // rather radioing about individuals one a a time (repeats see 1 enemy quote)
        for (cnt = Status.team[Net.pnum].guystart,ptr = Globals.MercPtrs[cnt]; cnt < Status.team[Net.pnum].guyend; cnt++,ptr++)
         {
          if (ptr.active && ptr.in_sector && (ptr.life >= OKLIFE))
            HandleSight(ptr,SIGHT_RADIO);      // looking was done above
         }

        for (cnt = 0,ptr = Menptr; cnt < MAXMERCS; cnt++,ptr++)
         {
          if (ptr.active && ptr.in_sector && (ptr.life >= OKLIFE) && !PTR_OURTEAM)
            HandleSight(ptr,SIGHT_RADIO);      // looking was done above
         }


        // if interrupts were allowed
        if (allowInterrupts)
          // resolve interrupts against the selected character (others disallowed)
          HandleSight(Globals.MercPtrs[Status.allLookCharacter],SIGHT_INTERRUPT);


        // revert to normal interrupt operation
        InterruptOnlyGuynum = Globals.NOBODY;
        InterruptsAllowed = true;
        */

        // reset interrupt only guynum which may have been used
        gubInterruptProvoker = Globals.NOBODY;
    }

    void ManLooksForOtherTeams(SOLDIERTYPE? pSoldier)
    {
        int uiLoop;
        SOLDIERTYPE? pOpponent;


        //    DebugMsg(TOPIC_JA2OPPLIST, DBG_LEVEL_3,
        //          String("MANLOOKSFOROTHERTEAMS ID %d(%S) team %d side %d", pSoldier.ubID, pSoldier.name, pSoldier.bTeam, pSoldier.bSide));

        // one soldier (pSoldier) looks for every soldier on another team (pOpponent)

        for (uiLoop = 0; uiLoop < guiNumMercSlots; uiLoop++)
        {
            pOpponent = MercSlots[uiLoop];

            // if this soldier is around and alive
            if (pOpponent && pOpponent.bLife)
            {
                // and if he's on another team...
                if (pSoldier.bTeam != pOpponent.bTeam)
                {

                    // use both sides actual x,y co-ordinates (neither side's moving)
                    // if he sees this opponent...
                    ManLooksForMan(pSoldier, pOpponent, MANLOOKSFOROTHERTEAMS);

                    // OK, We now want to , if in non-combat, set visiblity to 0 if not visible still....
                    // This allows us to walk away from buddy and have them disappear instantly
                    if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED) && !(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
                    {
                        if (pOpponent.bVisible == 0)
                        {
                            pOpponent.bVisible = -1;
                        }
                    }
                }
            }
        }
    }

    public static void HandleManNoLongerSeen(SOLDIERTYPE? pSoldier, SOLDIERTYPE? pOpponent, int pPersOL, int pbPublOL)
    {
        // if neither side is neutral AND
        // if this soldier is an opponent (fights for different side)
        if (!CONSIDERED_NEUTRAL(pOpponent, pSoldier) && !CONSIDERED_NEUTRAL(pSoldier, pOpponent) && (pSoldier.bSide != pOpponent.bSide))
        {
            RemoveOneOpponent(pSoldier);
        }

        // change personal opplist to indicate "seen this turn"
        // don't use UpdatePersonal() here, because we're changing to a *lower*
        // opplist value (which UpdatePersonal ignores) and we're not updating
        // the lastKnown gridno at all, we're keeping it at its previous value
        /*
    #if RECORDOPPLIST
      fprintf(OpplistFile,"ManLooksForMan: changing personalOpplist to %d for guynum %d, opp %d\n",SEEN_THIS_TURN,ptr.guynum,oppPtr.guynum);
    #endif
        */

        pPersOL = SEEN_THIS_TURN;

        if ((pSoldier.ubCivilianGroup == CIV_GROUP.KINGPIN_CIV_GROUP) && (pOpponent.bTeam == gbPlayerNum))
        {
            if (RenderFun.InARoom(pOpponent.sGridNo, out var ubRoom) && IN_BROTHEL(ubRoom) && (IN_BROTHEL_GUARD_ROOM(ubRoom)))
            {
                // unauthorized!
                // make guard run to block guard room
                AIMain.CancelAIAction(pSoldier, 1);
                RESETTIMECOUNTER(ref pSoldier.AICounter, 0);
                pSoldier.bNextAction = AI_ACTION.RUN;
                pSoldier.usNextActionData = 13250;
            }
        }

        // if opponent was seen publicly last time
        if (pbPublOL == SEEN_CURRENTLY)
        {
            // check if I was the only one who was seeing this guy (exlude ourselves)
            // THIS MUST HAPPEN EVEN FOR ENEMIES, TO MAKE THEIR PUBLIC opplist DECAY!
            if (TeamNoLongerSeesMan(pSoldier.bTeam, pOpponent, pSoldier.ubID, 0) > 0)
            {
                // DebugMsg(TOPIC_JA2OPPLIST, DBG_LEVEL_3, String("TeamNoLongerSeesMan: ID %d(%S) to ID %d", pSoldier.ubID, pSoldier.name, pOpponent.ubID));

                //fprintf(OpplistFile, "TeamNoLongerSeesMan returns true for team %d, opp %d\n", ptr.team, oppPtr.guynum);
                //fprintf(OpplistFile, "ManLooksForMan: changing publicOpplist to %d for team %d, opp %d\n", SEEN_THIS_TURN, ptr.team, oppPtr.guynum);

                // don't use UpdatePublic() here, because we're changing to a *lower*
                // opplist value (which UpdatePublic ignores) and we're not updating
                // the lastKnown gridno at all, we're keeping it at its previous value
                pbPublOL = SEEN_THIS_TURN;

                // ATE: Set visiblity to 0
                if (pSoldier.bTeam == gbPlayerNum && pOpponent.bTeam != gbPlayerNum)
                {
                    pOpponent.bVisible = 0;
                }
            }
        }

        // if we had only seen the guy for an instant and now lost sight of him
        if (gbSeenOpponents[pSoldier.ubID, pOpponent.ubID] == -1)
        {    // we can't leave it -1, because InterruptDuel() uses the special -1
             // value to know if we're only JUST seen the guy and screw up otherwise
             // it's enough to know we have seen him before
            gbSeenOpponents[pSoldier.ubID, pOpponent.ubID] = 1;
        }
    }

    static int ManLooksForMan(SOLDIERTYPE? pSoldier, SOLDIERTYPE? pOpponent, int ubCaller)
    {
        WorldDirections bDir;
        bool bAware = false;
        bool bSuccess = false;
        int sDistVisible, sDistAway;
        int? pPersOL, pbPublOL;


        /*
        if (ptr.guynum >= Globals.NOBODY)
         {
       #if BETAVERSION
          NumMessage("ManLooksForMan: ERROR - ptr.guynum = ",ptr.guynum);
       #endif
          return(success);
         }

        if (oppPtr.guynum >= Globals.NOBODY)
         {
       #if BETAVERSION
          NumMessage("ManLooksForMan: ERROR - oppPtr.guynum = ",oppPtr.guynum);
       #endif
          return(success);
         }

       */

        // if we're somehow looking while inactive, at base, dead or dying
        if (!pSoldier.bActive || !pSoldier.bInSector || (pSoldier.bLife < Globals.OKLIFE))
        {
            /*
            #if BETAVERSION
               sprintf(tempstr,"ManLooksForMan: ERROR - %s is looking while inactive/at base/dead/dying.  Caller %s",
                        ExtMen[ptr.guynum].name,LastCaller2Text[caller]);

            #if RECORDNET
               fprintf(NetDebugFile,"\n\t%s\n\n",tempstr);
            #endif

               PopMessage(tempstr);
            #endif
            */

            // # if TESTOPPLIST
            //                 DebugMsg(TOPIC_JA2OPPLIST, DBG_LEVEL_3,
            //                         String("ERROR: ManLooksForMan - WE are inactive/dead etc ID %d(%S)to ID %d", pSoldier.ubID, pSoldier.name, pOpponent.ubID));
            // #endif

            return (0);
        }



        // if we're somehow looking for a guy who is inactive, at base, or already dead
        if (!pOpponent.bActive || !pOpponent.bInSector || pOpponent.bLife <= 0 || pOpponent.sGridNo == NOWHERE)
        {
            /*
            #if BETAVERSION
               sprintf(tempstr,"ManLooksForMan: ERROR - %s looks for %s, who is inactive/at base/dead.  Caller %s",
                ExtMen[ptr.guynum].name,ExtMen[oppPtr.guynum].name,LastCaller2Text[caller]);

            #if RECORDNET
               fprintf(NetDebugFile,"\n\t%s\n\n",tempstr);
            #endif

               PopMessage(tempstr);
            #endif
            */

            // # if TESTOPPLIST
            //                 DebugMsg(TOPIC_JA2OPPLIST, DBG_LEVEL_3,
            //                        String("ERROR: ManLooksForMan - TARGET is inactive etc ID %d(%S)to ID %d", pSoldier.ubID, pSoldier.name, pOpponent.ubID));
            // #endif

            return (0);
        }


        // if he's looking for a guy who is on the same team
        if (pSoldier.bTeam == pOpponent.bTeam)
        {
            /*
            #if BETAVERSION
               sprintf(tempstr,"ManLooksFormMan: ERROR - on SAME TEAM.  ptr.guynum = %d, oppPtr.guynum = %d",
                                ptr.guynum,oppPtr.guynum);
            #if RECORDNET
               fprintf(NetDebugFile,"\n\t%s\n\n",tempstr);
            #endif

               PopMessage(tempstr);
            #endif
            */

            // # if TESTOPPLIST
            //                 DebugMsg(TOPIC_JA2OPPLIST, DBG_LEVEL_3,
            //                         String("ERROR: ManLooksForMan - SAME TEAM ID %d(%S)to ID %d", pSoldier.ubID, pSoldier.name, pOpponent.ubID));
            // #endif

            return (0);
        }

        if (pSoldier.bLife < OKLIFE || pSoldier.fMercAsleep == true)
        {
            return (0);
        }

        // NEED TO CHANGE THIS
        /*
        // don't allow unconscious persons to look, but COLLAPSED, etc. is OK
        if (ptr.anitype[ptr.anim] == UNCONSCIOUS)
          return(success);
       */

        if (pSoldier.ubBodyType == SoldierBodyTypes.LARVAE_MONSTER || (pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE && pSoldier.bTeam == OUR_TEAM)))
        {
            // don't do sight for these
            return (0);
        }


        /*
        if (ptrProjected)
         {
          // use looker's PROJECTED x,y co-ordinates (those of his next gridno)
          fromX = ptr.destx;
          fromY = ptr.desty;
          fromGridno = ExtMen[ptr.guynum].nextGridno;
         }
        else
         {
          // use looker's ACTUAL x,y co-ordinates (those of gridno he's in now)
          fromX = ptr.x;
          fromY = ptr.y;
          fromGridno = ptr.sGridNo;
         }


        if (oppPtrProjected)
         {
          // use target's PROJECTED x,y co-ordinates (those of his next gridno)
          toX = oppPtr.destx;
          toY = oppPtr.desty;
          toGridno = ExtMen[oppPtr.guynum].nextGridno;
         }
        else
         {
          // use target's ACTUAL x,y co-ordinates (those of gridno he's in now)
          toX = oppPtr.x;
          toY = oppPtr.y;
          toGridno = oppPtr.gridno;
         }
       */

        pPersOL = (pSoldier.bOppList[pOpponent.ubID]);
        pbPublOL = (gbPublicOpplist[pSoldier.bTeam][pOpponent.ubID]);

        // if soldier is known about (SEEN or HEARD within last few turns)
        if (pPersOL is not null || pbPublOL is not null)
        {
            bAware = true;

            // then we look for him full viewing distance in EVERY direction
            sDistVisible = DistanceVisible(pSoldier, WorldDirections.DIRECTION_IRRELEVANT, 0, pOpponent.sGridNo, pOpponent.bLevel);

            //if (pSoldier.ubID == 0)
            //sprintf(gDebugStr,"ALREADY KNOW: ME %d him %d val %d",pSoldier.ubID,pOpponent.ubID,pSoldier.bOppList[pOpponent.ubID]);
        }
        else   // soldier is not currently known about
        {
            // distance we "see" then depends on the direction he is located from us
            bDir = SoldierControl.atan8(pSoldier.sX, pSoldier.sY, pOpponent.sX, pOpponent.sY);
            // BIG NOTE: must use desdir instead of direction, since in a projected
            // situation, the direction may still be changing if it's one of the first
            // few animation steps when this guy's turn to do his stepped look comes up
            sDistVisible = DistanceVisible(pSoldier, pSoldier.bDesiredDirection, bDir, pOpponent.sGridNo, pOpponent.bLevel);

            //if (pSoldier.ubID == 0)
            //sprintf(gDebugStr,"dist visible %d: my dir %d to him %d",sDistVisible,pSoldier.bDesiredDirection,bDir);
        }

        // calculate how many spaces away soldier is (using Pythagoras' theorem)
        sDistAway = PythSpacesAway(pSoldier.sGridNo, pOpponent.sGridNo);

        // # if TESTOPPLIST
        //             DebugMsg(TOPIC_JA2OPPLIST, DBG_LEVEL_3, String("MANLOOKSFORMAN: ID %d(%S) to ID %d: sDistAway %d sDistVisible %d", pSoldier.ubID, pSoldier.name, pOpponent.ubID, sDistAway, sDistVisible));
        // #endif


        // if we see close enough to see the soldier
        if (sDistAway <= sDistVisible)
        {
            // and we can trace a line of sight to his x,y coordinates
            // must use the REAL opplist value here since we may or may not know of him
            if (SoldierToSoldierLineOfSightTest(pSoldier, pOpponent, (int)sDistVisible, bAware))
            {
                ManSeesMan(pSoldier, pOpponent, pOpponent.sGridNo, pOpponent.bLevel, MANLOOKSFORMAN, ubCaller);
                bSuccess = true;
            }
            // # if TESTOPPLIST
            //                 else
            //                     DebugMsg(TOPIC_JA2OPPLIST, DBG_LEVEL_3, String("FAILED LINEOFSIGHT: ID %d (%S)to ID %d Personally %d, public %d", pSoldier.ubID, pSoldier.name, pOpponent.ubID, *pPersOL, *pbPublOL));
            // #endif

            /*
               // if we're looking for a local merc, and changed doors were in the way
               if (PTR_OURTEAM && (NextFreeDoorIndex > 0))
                 // make or fail, if we passed through any "changed" doors along the way,
                 // reveal their true status (change the structure to its real value)
                 // (do this even if we don't have LOS, to close doors that *BREAK* LOS)
                 RevealDoorsAlongLOS();
            */

        }



        /*
        #if RECORDOPPLIST
         fprintf(OpplistFile,"MLFM: %s by %2d(g%4d,x%3d,y%3d,%s) at %2d(g%4d,x%3d,y%3d,%s), aware %d, dA=%d,dV=%d, desDir=%d, %s\n",
                (success) ? "SCS" : "FLR",
                ptr.guynum,fromGridno,fromX,fromY,(ptrProjected)?"PROJ":"REG.",
                oppPtr.guynum,toGridno,toX,toY,(oppPtrProjected)?"PROJ":"REG.",
                aware,distAway,distVisible,ptr.desdir,
                LastCaller2Text[caller]);
        #endif
        */

        // if soldier seen personally LAST time could not be seen THIS time
        if (!bSuccess && (pPersOL == SEEN_CURRENTLY))
        {
            HandleManNoLongerSeen(pSoldier, pOpponent, pPersOL, pbPublOL);
        }
        else
        {
            if (!bSuccess)
            {
                // # if TESTOPPLIST
                //                     DebugMsg(TOPIC_JA2OPPLIST, DBG_LEVEL_3, String("NO LONGER VISIBLE ID %d (%S)to ID %d Personally %d, public %d success: %d", pSoldier.ubID, pSoldier.name, pOpponent.ubID, *pPersOL, *pbPublOL, bSuccess));
                // #endif


                // we didn't see the opponent, but since we didn't last time, we should be
                //if (*pbPublOL)
                //pOpponent.bVisible = true;
            }
            // # if TESTOPPLIST
            //                 else
            //                     DebugMsg(TOPIC_JA2OPPLIST, DBG_LEVEL_3, String("COOL. STILL VISIBLE ID %d (%S)to ID %d Personally %d, public %d success: %d", pSoldier.ubID, pSoldier.name, pOpponent.ubID, *pPersOL, *pbPublOL, bSuccess));
            // #endif
        }
        return (bSuccess);
    }

    static void ManSeesMan(SOLDIERTYPE? pSoldier, SOLDIERTYPE? pOpponent, int sOppGridno, int bOppLevel, int ubCaller, int ubCaller2)
    {
        int bDoLocate = 0;
        bool fNewOpponent = false;
        bool fNotAddedToList = true;
        int bOldOppList = pSoldier.bOppList[pOpponent.ubID];

        if (pSoldier.ubID >= Globals.NOBODY)
        {
            /*
       #if BETAVERSION
          NumMessage("ManSeesMan: ERROR - ptr.guynum = ",ptr.guynum);
       #endif
            */
            return;
        }

        if (pOpponent.ubID >= Globals.NOBODY)
        {
            /*
       #if BETAVERSION
          NumMessage("ManSeesMan: ERROR - oppPtr.guynum = ",oppPtr.guynum);
       #endif
            */
            return;
        }

        // if we're somehow looking while inactive, at base, dying or already dead
        if (!pSoldier.bActive || !pSoldier.bInSector || (pSoldier.bLife < OKLIFE))
        {
            /*
       #if BETAVERSION
          sprintf(tempstr,"ManSeesMan: ERROR - %s is SEEING ManSeesMan while inactive/at base/dead/dying",ExtMen[ptr.guynum].name);
          PopMessage(tempstr);
       #endif
           */
            return;
        }

        // if we're somehow seeing a guy who is inactive, at base, or already dead
        if (!pOpponent.bActive || !pOpponent.bInSector || pOpponent.bLife <= 0)
        {
            /*
       #if BETAVERSION
          sprintf(tempstr,"ManSeesMan: ERROR - %s sees %s, ManSeesMan, who is inactive/at base/dead",ExtMen[ptr.guynum].name,ExtMen[oppPtr.guynum].name);
          PopMessage(tempstr);
       #endif
            */
            return;
        }


        // if we're somehow seeing a guy who is on the same team
        if (pSoldier.bTeam == pOpponent.bTeam)
        {
            /*
       #if BETAVERSION
          sprintf(tempstr,"ManSeesMan: ERROR - on SAME TEAM.  ptr.guynum = %d, oppPtr.guynum = %d",
                           ptr.guynum,oppPtr.guynum);
          PopMessage(tempstr);
       #endif
            */
            return;
        }

        // if we're seeing a guy we didn't see on our last chance to look for him
        if (pSoldier.bOppList[pOpponent.ubID] != SEEN_CURRENTLY)
        {
            if (pOpponent.bTeam == gbPlayerNum)
            {
                if (pSoldier.ubProfile != NO_PROFILE)
                {
                    if (pSoldier.bTeam == TEAM.CIV_TEAM)
                    {
                        // if this person doing the sighting is a member of a civ group that hates us but 
                        // this fact hasn't been revealed, change the side of these people now. This will
                        // make them non-neutral so AddOneOpponent will be called, and the guy will say his
                        // "I hate you" quote
                        if (pSoldier.IsNeutral)
                        {
                            if (pSoldier.ubCivilianGroup != CIV_GROUP.NON_CIV_GROUP && gTacticalStatus.fCivGroupHostile[pSoldier.ubCivilianGroup] >= CIV_GROUP_WILL_BECOME_HOSTILE)
                            {
                                AddToShouldBecomeHostileOrSayQuoteList(pSoldier.ubID);
                                fNotAddedToList = false;
                            }
                        }
                        else if (NPCHasUnusedRecordWithGivenApproach(pSoldier.ubProfile, APPROACH_DECLARATION_OF_HOSTILITY))
                        {
                            // only add if have something to say
                            AddToShouldBecomeHostileOrSayQuoteList(pSoldier.ubID);
                            fNotAddedToList = false;
                        }

                        if (fNotAddedToList)
                        {
                            switch (pSoldier.ubProfile)
                            {
                                case NPCID.CARMEN:
                                    if (pOpponent.ubProfile == NPCID.SLAY) // 64
                                    {
                                        // Carmen goes to war (against Slay)
                                        if (pSoldier.IsNeutral)
                                        {
                                            //SetSoldierNonNeutral( pSoldier );
                                            pSoldier.bAttitude = Attitudes.ATTACKSLAYONLY;
                                            NPC.TriggerNPCRecord(pSoldier.ubProfile, 28);
                                        }
                                        /*
                                        if ( ! gTacticalStatus.uiFlags.HasFlag(INCOMBAT ))
                                        {
                                            EnterCombatMode( pSoldier.bTeam );
                                        }
                                        */
                                    }
                                    break;
                                case NPCID.ELDIN:
                                    if (pSoldier.IsNeutral)
                                    {
                                        int ubRoom = 0;
                                        // if player is in behind the ropes of the museum display
                                        // or if alarm has gone off (status red)
                                        RenderFun.InARoom(pOpponent.sGridNo, out ubRoom);

                                        if ((Facts.CheckFact(FACT.MUSEUM_OPEN, 0) == false && ubRoom >= 22 && ubRoom <= 41) || Facts.CheckFact(FACT.MUSEUM_ALARM_WENT_OFF, 0) || (ubRoom == 39 || ubRoom == 40) || (FindObj(pOpponent, CHALICE) != NO_SLOT))
                                        {
                                            Facts.SetFactTrue(FACT.MUSEUM_ALARM_WENT_OFF);
                                            AddToShouldBecomeHostileOrSayQuoteList(pSoldier.ubID);
                                        }
                                    }
                                    break;
                                case NPCID.JIM:
                                case NPCID.JACK:
                                case NPCID.OLAF:
                                case NPCID.RAY:
                                case NPCID.OLGA:
                                case NPCID.TYRONE:
                                    // change orders, reset action!
                                    if (pSoldier.bOrders != Orders.SEEKENEMY)
                                    {
                                        pSoldier.bOrders = Orders.SEEKENEMY;
                                        if (pSoldier.bOppCnt == 0)
                                        {
                                            // didn't see anyone before!
                                            AIMain.CancelAIAction(pSoldier, 1);
                                            SetNewSituation(pSoldier);
                                        }
                                    }
                                    break;
                                case NPCID.ANGEL:
                                    if (pOpponent.ubProfile == NPCID.MARIA)
                                    {
                                        if (Facts.CheckFact(FACT.MARIA_ESCORTED_AT_LEATHER_SHOP, NPCID.MARIA) == true)
                                        {
                                            // she was rescued! yay!
                                            NPC.TriggerNPCRecord(NPCID.ANGEL, 12);
                                        }
                                    }
                                    else if ((Facts.CheckFact(FACT.ANGEL_LEFT_DEED, NPCID.ANGEL) == true) && (Facts.CheckFact(FACT.ANGEL_MENTIONED_DEED, ANGEL) == false))
                                    {
                                        AIMain.CancelAIAction(pSoldier, 1);
                                        pSoldier.sAbsoluteFinalDestination = NOWHERE;
                                        SoldierControl.EVENT_StopMerc(pSoldier, pSoldier.sGridNo, pSoldier.bDirection);
                                        NPC.TriggerNPCRecord(NPCID.ANGEL, 20);
                                        // trigger Angel to walk off afterwards
                                        //TriggerNPCRecord( ANGEL, 24 );
                                    }
                                    break;
                                //case QUEEN:
                                case NPCID.JOE:
                                case NPCID.ELLIOT:
                                    if (!(gMercProfiles[pSoldier.ubProfile].ubMiscFlags2.HasFlag(ProfileMiscFlags2.PROFILE_MISC_FLAG2_SAID_FIRSTSEEN_QUOTE)))
                                    {
                                        if (!Meanwhile.AreInMeanwhile())
                                        {
                                            NPC.TriggerNPCRecord(pSoldier.ubProfile, 4);
                                            gMercProfiles[pSoldier.ubProfile].ubMiscFlags2 |= ProfileMiscFlags2.PROFILE_MISC_FLAG2_SAID_FIRSTSEEN_QUOTE;
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    else
                    {
                        switch (pSoldier.ubProfile)
                        {
                            /*
                            case MIKE:
                                if ( gfPlayerTeamSawMike && !( gMercProfiles[ pSoldier.ubProfile ].ubMiscFlags2 & PROFILE_MISC_FLAG2_SAID_FIRSTSEEN_QUOTE ) )
                                {
                                    InitiateConversation( pSoldier, pOpponent, NPC_INITIAL_QUOTE, 0 );
                                    gMercProfiles[ pSoldier.ubProfile ].ubMiscFlags2 |= PROFILE_MISC_FLAG2_SAID_FIRSTSEEN_QUOTE;
                                }
                                break;
                                */
                            case NPCID.IGGY:
                                if (!(gMercProfiles[pSoldier.ubProfile].ubMiscFlags2.HasFlag(ProfileMiscFlags2.PROFILE_MISC_FLAG2_SAID_FIRSTSEEN_QUOTE)))
                                {
                                    NPC.TriggerNPCRecord(pSoldier.ubProfile, 9);
                                    gMercProfiles[pSoldier.ubProfile].ubMiscFlags2 |= ProfileMiscFlags2.PROFILE_MISC_FLAG2_SAID_FIRSTSEEN_QUOTE;
                                    gbPublicOpplist[gbPlayerNum][pSoldier.ubID] = HEARD_THIS_TURN;
                                }
                                break;
                        }
                    }
                }
                else
                {
                    if (pSoldier.bTeam == CIV_TEAM)
                    {
                        if (pSoldier.ubCivilianGroup != CIV_GROUP.NON_CIV_GROUP
                            && gTacticalStatus.fCivGroupHostile[pSoldier.ubCivilianGroup] >= CIV_GROUP_WILL_BECOME_HOSTILE
                            && pSoldier.IsNeutral)
                        {
                            AddToShouldBecomeHostileOrSayQuoteList(pSoldier.ubID);
                        }
                        else if (pSoldier.ubCivilianGroup == CIV_GROUP.KINGPIN_CIV_GROUP)
                        {
                            // generic kingpin goon...

                            // check to see if we are looking at Maria or unauthorized personnel in the brothel
                            if (pOpponent.ubProfile == NPCID.MARIA)
                            {
                                MakeCivHostile(pSoldier, 2);
                                if (!(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
                                {
                                    EnterCombatMode(pSoldier.bTeam);
                                }

                                Facts.SetFactTrue(FACT.MARIA_ESCAPE_NOTICED);
                            }
                            else
                            {
                                int ubRoom;

                                // JA2 Gold: only go hostile if see player IN guard room
                                //if ( InARoom( pOpponent.sGridNo, &ubRoom ) && IN_BROTHEL( ubRoom ) && ( gMercProfiles[ MADAME ].bNPCData == 0 || IN_BROTHEL_GUARD_ROOM( ubRoom ) ) )
                                if (RenderFun.InARoom(pOpponent.sGridNo, out ubRoom) && IN_BROTHEL_GUARD_ROOM(ubRoom))
                                {
                                    // unauthorized!
                                    MakeCivHostile(pSoldier, 2);
                                    if (!(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
                                    {
                                        EnterCombatMode(pSoldier.bTeam);
                                    }
                                }
                            }
                        }
                        else if (pSoldier.ubCivilianGroup == CIV_GROUP.HICKS_CIV_GROUP
                            && Facts.CheckFact(FACT.HICKS_MARRIED_PLAYER_MERC, 0) == false)
                        {
                            uint uiTime;
                            int sX, sY;

                            // if before 6:05 or after 22:00, make hostile and enter combat
                            uiTime = GameClock.GetWorldMinutesInDay();
                            if (uiTime < 365 || uiTime > 1320)
                            {
                                // get off our farm!
                                MakeCivHostile(pSoldier, 2);
                                if (!(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
                                {
                                    EnterCombatMode(pSoldier.bTeam);

                                    Overhead.LocateSoldier(pSoldier.ubID, true);
                                    GetSoldierScreenPos(pSoldier, out sX, out sY);
                                    // begin quote
                                    BeginCivQuote(pSoldier, CIV_QUOTE_HICKS_SEE_US_AT_NIGHT, 0, sX, sY);
                                }
                            }
                        }
                    }
                }
            }
            else if (pSoldier.bTeam == gbPlayerNum)
            {
                if ((pOpponent.ubProfile == NPCID.MIKE) && (pSoldier.ubWhatKindOfMercAmI == MERC_TYPE.AIM_MERC) && !(pSoldier.usQuoteSaidExtFlags.HasFlag(SOLDIER_QUOTE.SAID_EXT_MIKE)))
                {
                    if (gfMikeShouldSayHi == false)
                    {
                        gfMikeShouldSayHi = true;
                    }

                    TacticalCharacterDialogue(pSoldier, QUOTE.AIM_SEEN_MIKE);
                    pSoldier.usQuoteSaidExtFlags |= SOLDIER_QUOTE.SAID_EXT_MIKE;
                }
                else if (pOpponent.ubProfile == NPCID.JOEY && gfPlayerTeamSawJoey == false)
                {
                    TacticalCharacterDialogue(pSoldier, QUOTE.SPOTTED_JOEY);
                    gfPlayerTeamSawJoey = true;
                }
            }

            // as soon as a bloodcat sees someone, it becomes hostile
            // this is safe to do here because we haven't made this new person someone we've seen yet
            // (so we are assured we won't count 'em twice for oppcnt purposes)
            if (pSoldier.ubBodyType == SoldierBodyTypes.BLOODCAT)
            {
                if (pSoldier.IsNeutral)
                {
                    MakeBloodcatsHostile();
                    /*
                  SetSoldierNonNeutral( pSoldier );
                  RecalculateOppCntsDueToNoLongerNeutral( pSoldier );
                  if ( ( gTacticalStatus.uiFlags.HasFlag(INCOMBAT )) )
                  {
                      CheckForPotentialAddToBattleIncrement( pSoldier );
                  }
                    */

                    //PlayJA2Sample(BLOODCAT_ROAR, RATE_11025, HIGHVOLUME, 1, MIDDLEPAN);
                }
                else
                {
                    if (pSoldier.bOppCnt == 0)
                    {
                        if (Globals.Random.Next(2) == 0)
                        {
                            //PlayJA2Sample(BLOODCAT_ROAR, RATE_11025, HIGHVOLUME, 1, MIDDLEPAN);
                        }
                    }
                }
            }
            else if (pOpponent.ubBodyType == SoldierBodyTypes.BLOODCAT && pOpponent.bNeutral > 0)
            {
                MakeBloodcatsHostile();
                /*
                SetSoldierNonNeutral( pOpponent );
                RecalculateOppCntsDueToNoLongerNeutral( pOpponent );
                if ( ( gTacticalStatus.uiFlags.HasFlag(INCOMBAT )) )
                {
                    CheckForPotentialAddToBattleIncrement( pOpponent );
                }
                */
            }

            // if both of us are not neutral, AND
            // if this man is actually a true opponent (we're not on the same side)
            if (!CONSIDERED_NEUTRAL(pOpponent, pSoldier) && !CONSIDERED_NEUTRAL(pSoldier, pOpponent) && (pSoldier.bSide != pOpponent.bSide))
            {
                AddOneOpponent(pSoldier);

                // # if TESTOPPLIST
                //                     DebugMsg(TOPIC_JA2OPPLIST, DBG_LEVEL_3, String("ManSeesMan: ID %d(%S) to ID %d NEW TO ME", pSoldier.ubID, pSoldier.name, pOpponent.ubID));
                // #endif

                // if we also haven't seen him earlier this turn
                if (pSoldier.bOppList[pOpponent.ubID] != SEEN_THIS_TURN)
                {
                    fNewOpponent = true;
                    pSoldier.bNewOppCnt++;        // increment looker's NEW opponent count
                                                  //ScreenMsg( FONT_MCOLOR_LTYELLOW, MSG_INTERFACE, "Soldier %d sees soldier %d!", pSoldier.ubID, pOpponent.ubID );

                    //ExtMen[ptr.guynum].lastCaller = caller;
                    //ExtMen[ptr.guynum].lastCaller2 = caller2;

                    IncrementWatchedLoc(pSoldier.ubID, pOpponent.sGridNo, pOpponent.bLevel);

                    if (pSoldier.bTeam == OUR_TEAM && pOpponent.bTeam == TEAM.ENEMY_TEAM)
                    {
                        if (Facts.CheckFact(FACT.FIRST_BATTLE_FOUGHT, 0) == false)
                        {
                            Facts.SetFactTrue(FACT.FIRST_BATTLE_BEING_FOUGHT);
                        }
                    }


                }
                else
                {
                    SetWatchedLocAsUsed(pSoldier.ubID, pOpponent.sGridNo, pOpponent.bLevel);
                }

                // we already know the soldier isn't SEEN_CURRENTLY,
                // now check if he is really "NEW" ie. not expected to be there

                // if the looker hasn't seen this opponent at all earlier this turn, OR
                // if the opponent is not where the looker last thought him to be
                if ((pSoldier.bOppList[pOpponent.ubID] != SEEN_THIS_TURN) ||
                    (gsLastKnownOppLoc[pSoldier.ubID, pOpponent.ubID] != sOppGridno))
                {
                    SetNewSituation(pSoldier);  // force the looker to re-evaluate
                }
                else
                {
                    // if we in a non-combat movement decision, presumably this is not
                    // something we were quite expecting, so make a new decision.  For
                    // other (combat) movement decisions, we took his position into account
                    // when we made it, so don't make us think again & slow things down.
                    switch (pSoldier.bAction)
                    {
                        case AI_ACTION.RANDOM_PATROL:
                        case AI_ACTION.SEEK_OPPONENT:
                        case AI_ACTION.SEEK_FRIEND:
                        case AI_ACTION.POINT_PATROL:
                        case AI_ACTION.LEAVE_WATER_GAS:
                        case AI_ACTION.SEEK_NOISE:
                            SetNewSituation(pSoldier);  // force the looker to re-evaluate
                            break;
                    }
                }
            }

        }
        // # if TESTOPPLIST
        //             else
        //                 DebugMsg(TOPIC_JA2OPPLIST, DBG_LEVEL_3, String("ManSeesMan: ID %d(%S) to ID %d ALREADYSEENCURRENTLY", pSoldier.ubID, pSoldier.name, pOpponent.ubID));
        // #endif
        //bOldOppValue = pSoldier.bOppList[ pOpponent.ubID ];
        // remember that the soldier is currently seen and his new location
        UpdatePersonal(pSoldier, pOpponent.ubID, SEEN_CURRENTLY, sOppGridno, bOppLevel);

        if (ubCaller2 == MANLOOKSFOROTHERTEAMS || ubCaller2 == OTHERTEAMSLOOKFORMAN || ubCaller2 == CALLER_UNKNOWN) // unknown.hearing
        {

            if (gubBestToMakeSightingSize != BEST_SIGHTING_ARRAY_SIZE_INCOMBAT && gTacticalStatus.bBoxingState == NOT_BOXING)
            {
                if (fNewOpponent)
                {
                    if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
                    {
                        // presumably a door opening... we do require standard interrupt conditions				
                        if (StandardInterruptConditionsMet(pSoldier, pOpponent.ubID, bOldOppList))
                        {
                            ReevaluateBestSightingPosition(pSoldier, CalcInterruptDuelPts(pSoldier, pOpponent.ubID, true));
                        }
                    }
                    // require the enemy not to be dying if we are the sighter; in other words,
                    // always add for AI guys, and always add for people with life >= OKLIFE
                    else if (!(pSoldier.bTeam == gbPlayerNum && pOpponent.bLife < OKLIFE))
                    {
                        ReevaluateBestSightingPosition(pSoldier, CalcInterruptDuelPts(pSoldier, pOpponent.ubID, true));
                    }
                }
            }
        }

        // if this man has never seen this opponent before in this sector
        if (gbSeenOpponents[pSoldier.ubID, pOpponent.ubID] == 0)
        {
            // remember that he is just seeing him now for the first time (-1)
            gbSeenOpponents[pSoldier.ubID, pOpponent.ubID] = -1;
        }
        else
        {
            // man is seeing an opponent AGAIN whom he has seen at least once before
            gbSeenOpponents[pSoldier.ubID, pOpponent.ubID] = 1;
        }



        // if looker is on local team, and the enemy was invisible or "maybe"
        // visible just prior to this
        //# if WE_SEE_WHAT_MILITIA_SEES_AND_VICE_VERSA
        //            if ((PTR_OURTEAM || (pSoldier.bTeam == MILITIA_TEAM)) && (pOpponent.bVisible <= 0))
        //#else
        if (PTR_OURTEAM(pSoldier) && (pOpponent.bVisible <= 0))
        {
            // if opponent was truly invisible, not just turned off temporarily (false)
            if (pOpponent.bVisible == -1)
            {
                // then locate to him and set his locator flag
                bDoLocate = 1;

            }

            // make opponent visible (to us)
            // must do this BEFORE the locate since it checks for visibility
            pOpponent.bVisible = 1;

            //ATE: Cancel any fading going on!
            // ATE: Added for fade in.....
            if (pOpponent.fBeginFade == 1 || pOpponent.fBeginFade == 2)
            {
                pOpponent.fBeginFade = 0;

                if (pOpponent.bLevel > 0 && gpWorldLevelData[pOpponent.sGridNo].pRoofHead != null)
                {
                    pOpponent.ubFadeLevel = gpWorldLevelData[pOpponent.sGridNo].pRoofHead.ubShadeLevel;
                }
                else
                {
                    pOpponent.ubFadeLevel = gpWorldLevelData[pOpponent.sGridNo].pLandHead.ubShadeLevel;
                }

                // Set levelnode shade level....
                if (pOpponent.pLevelNode)
                {
                    pOpponent.pLevelNode.ubShadeLevel = pOpponent.ubFadeLevel;
                }
            }


            //# if TESTOPPLIST
            //                    DebugMsg(TOPIC_JA2OPPLIST, DBG_LEVEL_3, String("!!! ID %d (%S) MAKING %d VISIBLE", pSoldier.ubID, pSoldier.name, pOpponent.ubID));
            //#endif

            // update variable for STATUS screen
            //pOpponent.bLastKnownLife = pOpponent.life;

            if (bDoLocate > 0)
            {

                // Change his anim speed!
                SetSoldierAniSpeed(pOpponent);

                // if show enemies is ON, then we must have already revealed these roofs
                // and we're also following his movements, so don't bother sliding
                if (!gbShowEnemies)
                {
                    //DoSoldierRoofs(pOpponent);

                    // slide to the newly seen opponent, and if appropriate, start his locator
                    //SlideToMe = oppPtr.guynum;
                }

                //LastOpponentLocatedTo = oppPtr.guynum;

                /*
       #if RECORDNET
            fprintf(NetDebugFile,"\tManSeesMan - LOCATE\n");
       #endif
                */


                if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED) && ((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)) | gTacticalStatus.fVirginSector))
                {
                    if (!pOpponent.IsNeutral && (pSoldier.bSide != pOpponent.bSide))
                    {
                        SlideTo(0, pOpponent.ubID, pSoldier.ubID, SETLOCATOR);
                    }
                }
            }


        }
        else if (!PTR_OURTEAM(pSoldier))
        {
            // ATE: Check stance, change to threatending
            ReevaluateEnemyStance(pSoldier, pSoldier.usAnimState);
        }

    }


    void DecideTrueVisibility(SOLDIERTYPE? pSoldier, int ubLocate)
    {
        // if his visibility is still in the special "limbo" state (false)
        if (pSoldier.bVisible == 0)
        {
            // then none of our team's merc turned him visible,
            // therefore he now becomes truly invisible
            pSoldier.bVisible = -1;

            // Don;t adjust anim speed here, it's done once fade is over!
        }


        // If soldier is not visible, make sure his red "locator" is turned off
        //if ((pSoldier.bVisible < 0) && !gbShowEnemies)
        //	pSoldier.bLocator = false;


        if (ubLocate)
        {
            // if he remains visible (or ShowEnemies ON)
            if ((pSoldier.bVisible >= 0) || gbShowEnemies)
            {
                /*
       #if RECORDNET
            fprintf(NetDebugFile,"\tDecideTrueVisibility - LOCATE\n");
       #endif
           */

                if (PTR_OURTEAM(pSoldier))
                {
                    //if (ConfigOptions[FOLLOWMODE] && Status.stopSlidingAt == Globals.NOBODY)
                    //  LocateMember(ptr.guynum,DONTSETLOCATOR);
                }
                else // not our team - if we're NOT allied then locate...
                     //if (pSoldier.side != gTacticalStatus.Team[gbPlayerNum].side && ConfigOptions[FOLLOWMODE])
                     //if (Status.stopSlidingAt == Globals.NOBODY)
                if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED)
                    && (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
                {
                    //LocateSoldier(pSoldier.ubID,DONTSETLOCATOR);
                    SlideTo(0, pSoldier.ubID, Globals.NOBODY, DONTSETLOCATOR);
                }

                // follow his movement on our screen as he moves around...
                //LocateMember(ptr.guynum,DONTSETLOCATOR);
            }
        }
    }

    void OtherTeamsLookForMan(SOLDIERTYPE? pOpponent)
    {
        int uiLoop;
        int bOldOppList;
        SOLDIERTYPE? pSoldier;


        //NumMessage("OtherTeamsLookForMan, guy#",oppPtr.guynum);

        // if the guy we're looking for is NOT on our team AND is currently visible
        // # if WE_SEE_WHAT_MILITIA_SEES_AND_VICE_VERSA
        //             if ((pOpponent.bTeam != gbPlayerNum && pOpponent.bTeam != MILITIA_TEAM) && (pOpponent.bVisible >= 0 && pOpponent.bVisible < 2) && pOpponent.bLife)
        // #else
        if ((pOpponent.bTeam != gbPlayerNum) && (pOpponent.bVisible >= 0 && pOpponent.bVisible < 2) && pOpponent.IsAlive)
        // #endif
        {
            // assume he's no longer visible, until one of our mercs sees him again
            pOpponent.bVisible = 0;
        }

        // # if TESTOPPLIST
        //             DebugMsg(TOPIC_JA2OPPLIST, DBG_LEVEL_3,
        //                     String("OTHERTEAMSLOOKFORMAN ID %d(%S) team %d side %d", pOpponent.ubID, pOpponent.name, pOpponent.bTeam, pOpponent.bSide));
        // #endif


        // all soldiers not on oppPtr's team now look for him
        for (uiLoop = 0; uiLoop < guiNumMercSlots; uiLoop++)
        {
            pSoldier = MercSlots[uiLoop];

            // if this merc is active, in this sector, and well enough to look
            if (pSoldier != null && pSoldier.bLife >= OKLIFE && (pSoldier.ubBodyType != LARVAE_MONSTER))
            {
                // if this merc is on the same team as the target soldier
                if (pSoldier.bTeam == pOpponent.bTeam)
                {
                    continue;        // he doesn't look (he ALWAYS knows about him)
                }

                bOldOppList = pSoldier.bOppList[pOpponent.ubID];

                // this merc looks for the soldier in question
                // use both sides actual x,y co-ordinates (neither side's moving)
                if (ManLooksForMan(pSoldier, pOpponent, OTHERTEAMSLOOKFORMAN) > 0)
                {
                    // if a new opponent is seen (which must be oppPtr himself)
                    //if ((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED)) && (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)) && pSoldier.bNewOppCnt)
                    // Calc interrupt points in non-combat because we might get an interrupt or be interrupted
                    // on our first turn

                    // if doing regular in-combat sighting (not on opening doors!)
                    if (gubBestToMakeSightingSize == BEST_SIGHTING_ARRAY_SIZE_INCOMBAT)
                    {
                        if ((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED))
                            && (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
                            && pSoldier.bNewOppCnt > 0)
                        {
                            // as long as viewer meets minimum interrupt conditions
                            if (gubSightFlags.HasFlag(SIGHT.INTERRUPT) && StandardInterruptConditionsMet(pSoldier, pOpponent.ubID, bOldOppList))
                            {
                                // calculate the interrupt duel points						
                                pSoldier.bInterruptDuelPts = CalcInterruptDuelPts(pSoldier, pOpponent.ubID, true);
                                //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("Calculating int duel pts in OtherTeamsLookForMan, %d has %d points", pSoldier.ubID, pSoldier.bInterruptDuelPts));
                            }
                            else
                            {
                                pSoldier.bInterruptDuelPts = Globals.NO_INTERRUPT;
                            }
                        }
                    }
                }

                // if "Show only enemies seen" option is ON and it's this guy looking
                //if (ptr.guynum == ShowOnlySeenPerson)
                //NewShowOnlySeenPerson(ptr);                  // update the string
            }
        }


        // if he's not on our team
        if (pOpponent.bTeam != gbPlayerNum)
        {
            // don't do a locate here, it's already done by Man Sees Man for new opps.
            DecideTrueVisibility(pOpponent, NOLOCATE);
        }
    }

    public static void AddOneOpponent(SOLDIERTYPE? pSoldier)
    {
        int bOldOppCnt = pSoldier.bOppCnt;

        pSoldier.bOppCnt++;

        if (bOldOppCnt == 0)
        {
            // if we hadn't known about opponents being here for sure prior to this
            if (pSoldier.ubBodyType == SoldierBodyTypes.LARVAE_MONSTER)
            {
                // never become aware of you!
                return;
            }

            if (pSoldier.bAlertStatus < STATUS.RED)
            {
                CheckForChangingOrders(pSoldier);
            }

            pSoldier.bAlertStatus = STATUS.BLACK;   // force black AI status right away

            if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.MONSTER))
            {
                pSoldier.ubCaller = Globals.NOBODY;
                pSoldier.bCallPriority = 0;
            }
        }

        if (pSoldier.bTeam == gbPlayerNum)
        {
            // adding an opponent for player; reset # of turns that we haven't seen an enemy
            gTacticalStatus.bConsNumTurnsNotSeen = 0;
        }

    }



    public static void RemoveOneOpponent(SOLDIERTYPE? pSoldier)
    {
        pSoldier.bOppCnt--;

        if (pSoldier.bOppCnt < 0)
        {
            //             DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("Oppcnt for %d (%s) tried to go below 0", pSoldier.ubID, pSoldier.name));
            // # if JA2BETAVERSION
            //                 ScreenMsg(MSG_FONT_YELLOW, MSG_UI_FEEDBACK, "Opponent counter dropped below 0 for person %d (%s).  Please inform Sir-tech of this, and what has just been happening in the game.", pSoldier.ubID, pSoldier.name);
            // #endif
            pSoldier.bOppCnt = 0;
        }

        // if no opponents remain in sight, drop status to RED (but NOT newSit.!)
        if (pSoldier.bOppCnt == 0)
        {
            pSoldier.bAlertStatus = STATUS.RED;
        }
    }




    void RemoveManAsTarget(SOLDIERTYPE? pSoldier)
    {

        SOLDIERTYPE? pOpponent;
        int ubTarget;


        ubTarget = pSoldier.ubID;

        // clean up the public opponent lists and locations
        for (TEAM ubLoop = 0; ubLoop < (TEAM)MAXTEAMS; ubLoop++)
        {
            // never causes any additional looks
            UpdatePublic(ubLoop, ubTarget, NOT_HEARD_OR_SEEN, NOWHERE, 0);
        }

        /*


        IAN COMMENTED THIS OUT MAY 1997 - DO WE NEED THIS?

         // make sure this guy is no longer a possible target for anyone
         for (cnt = 0, pOpponent = Menptr; cnt < MAXMERCS; cnt++,pOpponent++)
          {
           if (pOpponent.bOppNum == ubTarget)
               pOpponent.bOppNum = Globals.NOBODY;
          }

            */


        // clean up all opponent's opplists
        for (int ubLoop = 0; ubLoop < guiNumMercSlots; ubLoop++)
        {
            pOpponent = MercSlots[ubLoop];

            // if the target is active, a true opponent and currently seen by this merc
            if (pOpponent)
            {
                // check to see if OPPONENT considers US neutral
                if ((pOpponent.bOppList[ubTarget] == SEEN_CURRENTLY) && !pOpponent.bNeutral && !CONSIDERED_NEUTRAL(pOpponent, pSoldier) && (pSoldier.bSide != pOpponent.bSide))
                {
                    RemoveOneOpponent(pOpponent);
                }
                UpdatePersonal(pOpponent, ubTarget, NOT_HEARD_OR_SEEN, NOWHERE, 0);
                gbSeenOpponents[ubLoop, ubTarget] = 0;
            }
        }

        /*

         for (ubLoop = 0,pOpponent = Menptr; ubLoop < MAXMERCS; ubLoop++,pOpponent++)
          {
           // if the target is a true opponent and currently seen by this merc
           if (!pSoldier.bNeutral && !pSoldier.bNeutral &&
               (pOpponent.bOppList[ubTarget] == SEEN_CURRENTLY)

                     )
                     ///*** UNTIL ANDREW GETS THE SIDE PARAMETERS WORKING
               // && (pSoldier.side != pOpponent.side))
            {
             RemoveOneOpponent(pOpponent);
            }

           UpdatePersonal(pOpponent,ubTarget,NOT_HEARD_OR_SEEN,NOWHERE,0);

           gbSeenOpponents[ubLoop, ubTarget] = false;
          }
        */

        ResetLastKnownLocs(pSoldier);

        if (gTacticalStatus.Team[pSoldier.bTeam].ubLastMercToRadio == ubTarget)
        {
            gTacticalStatus.Team[pSoldier.bTeam].ubLastMercToRadio = Globals.NOBODY;
        }
    }

    public static void UpdatePublic(TEAM ubTeam, int ubID, int bNewOpplist, int sGridno, int bLevel)
    {
        int cnt;
        int pbPublOL;
        bool ubTeamMustLookAgain = false;
        bool ubMadeDifference = false;
        // SOLDIERTYPE? pSoldier;


        pbPublOL = (gbPublicOpplist[ubTeam][ubID]);

        // if new opplist is more up-to-date, or we are just wiping it for some reason
        if ((gubKnowledgeValue[pbPublOL - OLDEST_HEARD_VALUE, bNewOpplist - OLDEST_HEARD_VALUE] > 0) ||
            (bNewOpplist == NOT_HEARD_OR_SEEN))
        {
            // if this team is becoming aware of a soldier it wasn't previously aware of
            if ((bNewOpplist != NOT_HEARD_OR_SEEN) && (pbPublOL == NOT_HEARD_OR_SEEN))
            {
                ubTeamMustLookAgain = true;
            }

            // change the public opplist *BEFORE* anyone looks again or we'll recurse!
            pbPublOL = bNewOpplist;
        }


        // always update the gridno, no matter what
        gsPublicLastKnownOppLoc[ubTeam][ubID] = sGridno;
        gbPublicLastKnownOppLevel[ubTeam][ubID] = bLevel;

        // if team has been told about a guy the team was completely unaware of
        if (ubTeamMustLookAgain)
        {
            // then everyone on team who's not aware of guynum must look for him
            cnt = gTacticalStatus.Team[ubTeam].bFirstID;

            // for (pSoldier = Globals.MercPtrs[cnt]; cnt <= gTacticalStatus.Team[ubTeam].bLastID; cnt++, pSoldier++)
            foreach (var pSoldier in MercPtrs.Skip(cnt))
            {
                // if this soldier is active, in this sector, and well enough to look
                if (pSoldier.bActive && pSoldier.bInSector && (pSoldier.bLife >= OKLIFE) && !(pSoldier.uiStatusFlags.HasFlag(SOLDIER.GASSED)))
                {
                    // if soldier isn't aware of guynum, give him another chance to see
                    if (pSoldier.bOppList[ubID] == NOT_HEARD_OR_SEEN)
                    {
                        if (ManLooksForMan(pSoldier, MercPtrs[ubID], UPDATEPUBLIC) > 0)
                        {
                            // then he actually saw guynum because of our new public knowledge
                            ubMadeDifference = true;
                        }

                        // whether successful or not, whack newOppCnt.  Since this is a
                        // delayed reaction to a radio call, there's no chance of interrupt!
                        pSoldier.bNewOppCnt = 0;

                        // if "Show only enemies seen" option is ON and it's this guy looking
                        //if (pSoldier.ubID == ShowOnlySeenPerson)
                        // NewShowOnlySeenPerson(pSoldier);                  // update the string
                    }
                }
            }
        }
    }




    public static void UpdatePersonal(SOLDIERTYPE? pSoldier, int ubID, int bNewOpplist, int sGridno, int bLevel)
    {
        /*
    #if RECORDOPPLIST
     fprintf(OpplistFile,"UpdatePersonal - for %d about %d to %d (was %d) at g%d\n",
            ptr.guynum,guynum,newOpplist,ptr.opplist[guynum],gridno);
    #endif

        */



        // if new opplist is more up-to-date, or we are just wiping it for some reason
        if ((gubKnowledgeValue[pSoldier.bOppList[ubID] - OLDEST_HEARD_VALUE, bNewOpplist - OLDEST_HEARD_VALUE] > 0) ||
            (bNewOpplist == NOT_HEARD_OR_SEEN))
        {
            pSoldier.bOppList[ubID] = bNewOpplist;
        }

        // always update the gridno, no matter what
        gsLastKnownOppLoc[pSoldier.ubID, ubID] = sGridno;
        gbLastKnownOppLevel[pSoldier.ubID, ubID] = bLevel;
    }

    int OurMaxPublicOpplist()
    {
        int uiLoop;
        int bHighestOpplist = 0;
        int ubOppValue, ubHighestValue = 0;
        SOLDIERTYPE? pSoldier;

        for (uiLoop = 0; uiLoop < guiNumMercSlots; uiLoop++)
        {
            pSoldier = MercSlots[uiLoop];

            // if this merc is inactive, at base, on assignment, or dead
            if (!pSoldier || !pSoldier.bLife)
            {
                continue;       // next merc
            }

            // if this man is NEUTRAL / on our side, he's not an opponent
            if (pSoldier.bNeutral || (gTacticalStatus.Team[gbPlayerNum].bSide == Menptr[pSoldier.ubID].bSide))
            {
                continue;       // next merc
            }

            // opponent, check our public opplist value for him
            ubOppValue = gubKnowledgeValue[0 - OLDEST_HEARD_VALUE, gbPublicOpplist[gbPlayerNum][pSoldier.ubID] - OLDEST_HEARD_VALUE];

            if (ubOppValue > ubHighestValue)
            {
                ubHighestValue = ubOppValue;
                bHighestOpplist = gbPublicOpplist[gbPlayerNum][pSoldier.ubID];
            }
        }

        return (bHighestOpplist);
    }




    /*
    bool VisibleAnywhere(SOLDIERTYPE *pSoldier)
    {
     int team,cnt;
     SOLDIERTYPE *pOpponent;


     // this takes care of any mercs on our own team
     if (pSoldier.bVisible >= 0)
       return(true);

     // if playing alone, "anywhere" is just over here!
     //if (!Net.multiType || Net.activePlayers < 2)
       //return(false);


     for (bTeam = 0; bTeam < MAXTEAMS; bTeam++)
      {
       // skip our team (local visible flag will do for them)
       if (bTeam == gbPlayerNum)
         continue;

       // skip any inactive teams
       if (!gTacticalStatus.team[bTeam].teamActive)
         continue;

       // skip non-human teams (they don't communicate for their machines!)
       if (!gTacticalStatus.Team[bTeam].human)
         continue;

       // so we're left with another human player's team of mercs...

       // check if soldier is currently visible to any human mercs on other teams
       for (cnt = Status.team[team].guystart,oppPtr = Menptr + cnt; cnt < Status.team[team].guyend; cnt++,oppPtr++)
        {
         // if this merc is inactive, or in no condition to care
         if (!oppPtr.active || !oppPtr.in_sector || oppPtr.deadAndRemoved || (oppPtr.life < OKLIFE))
           continue;          // skip him!

         if (oppPtr.opplist[ptr.guynum] == SEEN_CURRENTLY)
           return(true);
        }
      }


     // nobody anywhere sees him
     return(false);
    }

    */


    void ResetLastKnownLocs(SOLDIERTYPE? pSoldier)
    {
        int uiLoop;

        for (uiLoop = 0; uiLoop < guiNumMercSlots; uiLoop++)
        {
            if (MercSlots[uiLoop] is not null)
            {
                gsLastKnownOppLoc[pSoldier.ubID, MercSlots[uiLoop].ubID] = NOWHERE;

                // IAN added this June 14/97
                gsPublicLastKnownOppLoc[pSoldier.bTeam, MercSlots[uiLoop].ubID] = NOWHERE;
            }
        }
    }



    /*
    // INITIALIZATION STUFF
    -------------------------
    // Upon loading a scenario, call these:
    InitOpponentKnowledgeSystem();

    // loop through all soldiers and for each soldier call
    InitSoldierOpplist(pSoldier);

    // call this once
    AllTeamsLookForAll(NO_INTERRUPTS);	// no interrupts permitted this early


    // for each additional soldier created, call
    InitSoldierOpplist(pSoldier);
    HandleSight(pSoldier,SIGHT_LOOK);



    MOVEMENT STUFF
    -----------------
    // whenever new tile is reached, call
    HandleSight(pSoldier,SIGHT_LOOK);

    */

    void InitOpponentKnowledgeSystem()
    {
        int iTeam, cnt, cnt2;

        // memset(gbSeenOpponents, 0, sizeof(gbSeenOpponents));
        // memset(gbPublicOpplist, NOT_HEARD_OR_SEEN, sizeof(gbPublicOpplist));

        for (iTeam = 0; iTeam < MAXTEAMS; iTeam++)
        {
            gubPublicNoiseVolume[iTeam] = 0;
            gsPublicNoiseGridno[iTeam] = NOWHERE;
            gbPublicNoiseLevel[iTeam] = 0;
            for (cnt = 0; cnt < MAX_NUM_SOLDIERS; cnt++)
            {
                gsPublicLastKnownOppLoc[iTeam, cnt] = NOWHERE;
            }
        }

        // initialize public last known locations for all teams
        for (cnt = 0; cnt < MAX_NUM_SOLDIERS; cnt++)
        {
            for (cnt2 = 0; cnt2 < NUM_WATCHED_LOCS; cnt2++)
            {
                gsWatchedLoc[cnt, cnt2] = NOWHERE;
                gubWatchedLocPoints[cnt, cnt2] = 0;
                gfWatchedLocReset[cnt, cnt2] = false;
            }
        }

        for (cnt = 0; cnt < SHOULD_BECOME_HOSTILE_SIZE; cnt++)
        {
            gubShouldBecomeHostileOrSayQuote[cnt] = Globals.NOBODY;
        }

        gubNumShouldBecomeHostileOrSayQuote = 0;
    }



    void InitSoldierOppList(SOLDIERTYPE? pSoldier)
    {
        //memset(pSoldier.bOppList, NOT_HEARD_OR_SEEN, sizeof(pSoldier.bOppList));
        pSoldier.bOppCnt = 0;
        ResetLastKnownLocs(pSoldier);
        //memset(gbSeenOpponents[pSoldier.ubID], 0, MAXMERCS);
    }


    void BetweenTurnsVisibilityAdjustments()
    {
        // make all soldiers on other teams that are no longer seen not visible
        foreach (var pSoldier in Menptr)
        {
            if (pSoldier.bActive && pSoldier.bInSector && pSoldier.IsAlive)
            {
                if (!PTR_OURTEAM(pSoldier))
                {
                    // check if anyone on our team currently sees him (exclude Globals.NOBODY)
                    if (TeamNoLongerSeesMan(gbPlayerNum, pSoldier, Globals.NOBODY, 0))
                    {
                        // then our team has lost sight of him
                        pSoldier.bVisible = -1;        // make him fully invisible

                        // Allow fade to adjust anim speed
                    }
                }
            }
        }
    }


    public static void SaySeenQuote(SOLDIERTYPE? pSoldier, int fSeenCreature, bool fVirginSector, bool fSeenJoey)
    {
        SOLDIERTYPE? pTeamSoldier;
        int ubNumEnemies = 0;
        int ubNumAllies = 0;
        int cnt;

        if (Meanwhile.AreInMeanwhile())
        {
            return;
        }

        // Check out for our under large fire quote
        if (!(pSoldier.usQuoteSaidFlags.HasFlag(SOLDIER_QUOTE.SAID_IN_SHIT)))
        {
            // Get total enemies.
            // Loop through all mercs in sector and count # of enemies
            for (cnt = 0; cnt < guiNumMercSlots; cnt++)
            {
                pTeamSoldier = MercSlots[cnt];

                if (pTeamSoldier != null)
                {
                    if (OK_ENEMY_MERC(pTeamSoldier))
                    {
                        ubNumEnemies++;
                    }
                }
            }

            // OK, after this, check our guys
            for (cnt = 0; cnt < guiNumMercSlots; cnt++)
            {
                pTeamSoldier = MercSlots[cnt];

                if (pTeamSoldier != null)
                {
                    if (!OK_ENEMY_MERC(pTeamSoldier))
                    {
                        if (pTeamSoldier.bOppCnt >= (ubNumEnemies / 2))
                        {
                            ubNumAllies++;
                        }
                    }
                }
            }

            // now check!
            if ((pSoldier.bOppCnt - ubNumAllies) > 2)
            {
                // Say quote!
                TacticalCharacterDialogue(pSoldier, QUOTE.IN_TROUBLE_SLASH_IN_BATTLE);

                pSoldier.usQuoteSaidFlags |= SOLDIER_QUOTE.SAID_IN_SHIT;

                return;
            }

        }


        if (fSeenCreature == true)
        {
            // Is this our first time seeing them?
            if (gMercProfiles[pSoldier.ubProfile].ubMiscFlags.HasFlag(ProfileMiscFlags1.PROFILE_MISC_FLAG_HAVESEENCREATURE))
            {
                // Are there multiplaes and we have not said this quote during this battle?
                if (!(pSoldier.usQuoteSaidFlags.HasFlag(SOLDIER_QUOTE.SAID_MULTIPLE_CREATURES)))
                {
                    // Check for multiples!
                    ubNumEnemies = 0;

                    // Get total enemies.
                    // Loop through all mercs in sector and count # of enemies
                    for (cnt = 0; cnt < guiNumMercSlots; cnt++)
                    {
                        pTeamSoldier = MercSlots[cnt];

                        if (pTeamSoldier != null)
                        {
                            if (OK_ENEMY_MERC(pTeamSoldier))
                            {
                                if (pTeamSoldier.uiStatusFlags.HasFlag(SOLDIER.MONSTER && pSoldier.bOppList[pTeamSoldier.ubID] == SEEN_CURRENTLY))
                                {
                                    ubNumEnemies++;
                                }
                            }
                        }
                    }

                    if (ubNumEnemies > 2)
                    {
                        // Yes, set flag
                        pSoldier.usQuoteSaidFlags |= SOLDIER_QUOTE.SAID_MULTIPLE_CREATURES;

                        // Say quote
                        TacticalCharacterDialogue(pSoldier, QUOTE.ATTACKED_BY_MULTIPLE_CREATURES);
                    }
                    else
                    {
                        TacticalCharacterDialogue(pSoldier, QUOTE.SEE_CREATURE);
                    }
                }
                else
                {
                    TacticalCharacterDialogue(pSoldier, QUOTE.SEE_CREATURE);
                }
            }
            else
            {
                // Yes, set flag
                gMercProfiles[pSoldier.ubProfile].ubMiscFlags |= ProfileMiscFlags1.PROFILE_MISC_FLAG_HAVESEENCREATURE;

                TacticalCharacterDialogue(pSoldier, QUOTE.FIRSTTIME_GAME_SEE_CREATURE);
            }
        }
        // 2 is for bloodcat...
        else if (fSeenCreature == 2)
        {
            TacticalCharacterDialogue(pSoldier, QUOTE.SPOTTED_BLOODCAT);
        }
        else
        {
            if (fVirginSector)
            {
                // First time we've seen a guy this sector
                TacticalCharacterDialogue(pSoldier, QUOTE.SEE_ENEMY_VARIATION);
            }
            else
            {
                if (Globals.Random.Next(100) < 30)
                {
                    DoMercBattleSound(pSoldier, BATTLE_SOUND_ENEMY);
                }
                else
                {
                    TacticalCharacterDialogue(pSoldier, QUOTE.SEE_ENEMY);
                }
            }
        }
    }

    public static void OurTeamSeesSomeone(SOLDIERTYPE? pSoldier, int bNumReRevealed, int bNumNewEnemies)
    {
        if (gTacticalStatus.fVirginSector)
        {
            // If we are in NPC dialogue now... stop!
            DeleteTalkingMenu();

            // Say quote!
            SaySeenQuote(pSoldier, gfPlayerTeamSawCreatures, true, gfPlayerTeamSawJoey);

            HaultSoldierFromSighting(pSoldier, true);

            // Set virgin sector to false....
            gTacticalStatus.fVirginSector = false;
        }
        else
        {
            // if this merc is selected and he's actually moving
            //if ((pSoldier.ubID == gusSelectedSoldier) && !pSoldier.bStopped)
            // ATE: Change this to if the guy is ours....
            // How will this feel?
            if (pSoldier.bTeam == gbPlayerNum)
            {
                // STOP IF WE WERE MOVING....
                /// Speek up!
                if (bNumReRevealed > 0 && bNumNewEnemies == 0)
                {
                    DoMercBattleSound(pSoldier, BATTLE_SOUND_CURSE1);
                }
                else
                {
                    SaySeenQuote(pSoldier, gfPlayerTeamSawCreatures, false, gfPlayerTeamSawJoey);
                }

                HaultSoldierFromSighting(pSoldier, true);

                if (gTacticalStatus.fEnemySightingOnTheirTurn)
                {
                    // Locate to our guy, then slide to enemy
                    Overhead.LocateSoldier(pSoldier.ubID, SETLOCATOR);

                    // Now slide to other guy....
                    SlideTo(NOWHERE, gTacticalStatus.ubEnemySightingOnTheirTurnEnemyID, Globals.NOBODY, SETLOCATOR);

                }

                // Unset User's turn UI
                UnSetUIBusy(pSoldier.ubID);
            }
        }

        // OK, check what music mode we are in, change to battle if we're in battle
        // If we are in combat....
        if ((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
        {
            // If we are NOT in any music mode...
            if (gubMusicMode == MUSIC_NONE)
            {
                SetMusicMode(MUSIC_TACTICAL_BATTLE);
            }
        }


    }

    public static void RadioSightings(SOLDIERTYPE? pSoldier, int ubAbout, TEAM ubTeamToRadioTo)
    {
        SOLDIERTYPE? pOpponent;
        int iLoop;
        int start, end, revealedEnemies = 0, unknownEnemies = 0, stillUnseen = 1;
        int scrollToGuynum = Globals.NOBODY, sightedHatedOpponent = 0;
        //int 	oppIsCivilian;
        int pPersOL, pbPublOL; //,dayQuote;
        bool fContactSeen;
        bool fSawCreatureForFirstTime = false;


        //# if TESTOPPLIST
        //            DebugMsg(TOPIC_JA2OPPLIST, DBG_LEVEL_3,
        //                         String("RADIO SIGHTINGS: for %d about %d", pSoldier.ubID, ubAbout));
        //#endif



        //# if RECORDNET
        //            if (!ptr.human)
        //                fprintf(NetDebugFile, "\tNPC %d(%s) radios his sightings to his team\n", ptr.guynum, ExtMen[ptr.guynum].name);
        //#endif

        gTacticalStatus.Team[pSoldier.bTeam].ubLastMercToRadio = pSoldier.ubID;

        // who are we radioing about?
        if (ubAbout == EVERYBODY)
        {
            start = 0;
            end = MAXMERCS;
        }
        else
        {
            start = ubAbout;
            end = ubAbout + 1;
        }


        // hang a pointer to the start of our this guy's personal opplist
        pPersOL = (pSoldier.bOppList[start]);

        // hang a pointer to the start of this guy's opponents in the public opplist
        pbPublOL = (gbPublicOpplist[ubTeamToRadioTo][start]);

        pOpponent = Globals.MercPtrs[start];

        // loop through every one of this guy's opponents
        for (iLoop = start; iLoop < end; iLoop++)//, pOpponent++, pPersOL++, pbPublOL++)
        {
            fContactSeen = false;

            //# if TESTOPPLIST
            //                DebugMsg(TOPIC_JA2OPPLIST, DBG_LEVEL_3,
            //                         String("RS: checking %d", pOpponent.ubID));
            //#endif


            // make sure this merc is active, here & still alive (unconscious OK)
            if (!pOpponent.bActive || !pOpponent.bInSector || pOpponent.bLife == 0)
            {
                //# if TESTOPPLIST
                //                    DebugMsg(TOPIC_JA2OPPLIST, DBG_LEVEL_3,
                //                        String("RS: inactive/notInSector/life %d", pOpponent.ubID));
                //#endif


                continue;                          // skip to the next merc
            }

            // if these two mercs are on the same SIDE, then they're NOT opponents
            // NEW: Apr. 21 '96: must allow ALL non-humans to get radioed about
            if ((pSoldier.bSide == pOpponent.bSide) && (pOpponent.uiStatusFlags.HasFlag(SOLDIER.PC)))
            {
                //# if TESTOPPLIST
                //                    DebugMsg(TOPIC_JA2OPPLIST, DBG_LEVEL_3,
                //                     String("RS: same side %d", pSoldier.bSide));
                //#endif

                continue;                          // skip to the next merc
            }

            // determine whether we think we're still unseen or if "our cover's blown"
            // if we know about this opponent's location for any reason
            if ((pOpponent.bVisible >= 0) || gbShowEnemies)
            {
                // and he can see us, then gotta figure we KNOW that he can see us
                if (pOpponent.bOppList[pSoldier.ubID] == SEEN_CURRENTLY)
                {
                    stillUnseen = 0;
                }
            }


            // if we personally don't know a thing about this opponent
            if (pPersOL == Globals.NOT_HEARD_OR_SEEN)
            {
                //# if RECORDOPPLIST
                //                    //fprintf(OpplistFile,"not heard or seen\n");
                //#endif

                //# if TESTOPPLIST
                //                    DebugMsg(TOPIC_JA2OPPLIST, DBG_LEVEL_3,
                //                     String("RS: not heard or seen"));
                //#endif

                continue;                          // skip to the next opponent
            }

            // if personal knowledge is NOT more up to date and NOT the same as public
            if ((gubKnowledgeValue[pbPublOL - OLDEST_HEARD_VALUE, pPersOL - OLDEST_HEARD_VALUE]) == 0
                && (pbPublOL != pPersOL))
            {
                //# if RECORDOPPLIST
                //                    //fprintf(OpplistFile,"no new knowledge (per %d, pub %d)\n",*pPersOL,*pbPublOL);
                //#endif

                // # if TESTOPPLIST
                //                     DebugMsg(TOPIC_JA2OPPLIST, DBG_LEVEL_3,
                //                       String("RS: no new knowledge per %d pub %d", *pPersOL, *pbPublOL));
                // #endif



                continue;                          // skip to the next opponent
            }

            //# if RECORDOPPLIST
            //                //fprintf(OpplistFile,"made it!\n");
            //#endif

            // # if TESTOPPLIST
            //                 DebugMsg(TOPIC_JA2OPPLIST, DBG_LEVEL_3,
            //                      String("RS: made it!"));
            // #endif



            // if it's our merc, and he currently sees this opponent
            if (PTR_OURTEAM(pSoldier) && (pPersOL == SEEN_CURRENTLY) && !((pOpponent.bSide == pSoldier.bSide) || pOpponent.bNeutral > 0))
            {
                // used by QueueDayMessage() to scroll to one of the new enemies
                // scroll to the last guy seen, unless we see a hated guy, then use him!
                if (sightedHatedOpponent == 0)
                {
                    scrollToGuynum = pOpponent.ubID;
                }

                // don't care whether and how many new enemies are seen if everyone visible
                // and he's healthy enough to be a threat (so is worth talking about)

                // do the following if we're radioing to our own team; if radioing to militia
                // then alert them instead
                if (ubTeamToRadioTo != MILITIA_TEAM)
                {
                    if (!gbShowEnemies && (pOpponent.bLife >= OKLIFE))
                    {
                        // if this enemy has not been publicly seen or heard recently
                        if (pbPublOL == NOT_HEARD_OR_SEEN)
                        {
                            // chalk up another "unknown" enemy
                            unknownEnemies++;

                            fContactSeen = true;
                            // if this enemy is hated by the merc doing the sighting
                            //if (MercHated(Proptr[ptr.characternum].p_bias,oppPtr.characternum))
                            //sightedHatedOpponent = true;

                            // now the important part: does this enemy see him/her back?
                            if (pOpponent.bOppList[pSoldier.ubID] != SEEN_CURRENTLY)
                            {
                                // EXPERIENCE GAIN (10): Discovered a new enemy without being seen
                                Campaign.StatChange(pSoldier, Stat.EXPERAMT, 10, 0);
                            }
                        }
                        else
                        {

                            // if he has publicly not been seen now, or anytime during this turn
                            if ((pbPublOL != SEEN_CURRENTLY) && (pbPublOL != SEEN_THIS_TURN))
                            {
                                // chalk up another "revealed" enemy
                                revealedEnemies++;
                                fContactSeen = true;
                            }
                            else
                            {
                                if (Globals.MercPtrs[0].bLife < 10)
                                {
                                    int i = 0;
                                }
                            }
                        }

                        if (fContactSeen)
                        {
                            if (pSoldier.bTeam == gbPlayerNum)
                            {
                                if (gTacticalStatus.ubCurrentTeam != gbPlayerNum)
                                {
                                    // Save some stuff!
                                    if (gTacticalStatus.fEnemySightingOnTheirTurn)
                                    {
                                        // this has already come up so turn OFF the pause-all-anims flag for the previous
                                        // person and set it for this next person
                                        Globals.MercPtrs[gTacticalStatus.ubEnemySightingOnTheirTurnEnemyID].fPauseAllAnimation = false;
                                    }
                                    else
                                    {
                                        gTacticalStatus.fEnemySightingOnTheirTurn = true;
                                    }
                                    gTacticalStatus.ubEnemySightingOnTheirTurnEnemyID = pOpponent.ubID;
                                    gTacticalStatus.ubEnemySightingOnTheirTurnPlayerID = pSoldier.ubID;
                                    gTacticalStatus.uiTimeSinceDemoOn = GetJA2Clock();

                                    pOpponent.fPauseAllAnimation = true;

                                }
                            }

                            if (pOpponent.uiStatusFlags.HasFlag(SOLDIER.MONSTER))
                            {
                                gfPlayerTeamSawCreatures = true;
                            }

                            // ATE: Added for bloodcat...
                            if (pOpponent.ubBodyType == BLOODCAT)
                            {
                                // 2 is for bloodcat
                                gfPlayerTeamSawCreatures = 2;
                            }

                        }

                        if (pOpponent.uiStatusFlags.HasFlag(SOLDIER.MONSTER))
                        {
                            if (!(gMercProfiles[pSoldier.ubProfile].ubMiscFlags & ProfileMiscFlags1.PROFILE_MISC_FLAG_HAVESEENCREATURE))
                            {
                                fSawCreatureForFirstTime = true;
                            }
                        }

                    }
                }
                else
                {
                    // radioing to militia that we saw someone! alert them!
                    if (gTacticalStatus.Team[MILITIA_TEAM].bTeamActive > 0
                        && gTacticalStatus.Team[MILITIA_TEAM].bAwareOfOpposition == 0)
                    {
                        HandleInitialRedAlert(MILITIA_TEAM, false);
                    }
                }
            }   // end of our team's merc sees new opponent

            // IF WE'RE HERE, OUR PERSONAL INFORMATION IS AT LEAST AS UP-TO-DATE
            // AS THE PUBLIC KNOWLEDGE, SO WE WILL REPLACE THE PUBLIC KNOWLEDGE
            //# if RECORDOPPLIST
            //                fprintf(OpplistFile, "UpdatePublic (RadioSightings) for team %d about %d\n", ptr.team, oppPtr.guynum);
            //#endif


            // # if TESTOPPLIST
            //                 DebugMsg(TOPIC_JA2OPPLIST, DBG_LEVEL_3,
            //                          String("...............UPDATE PUBLIC: soldier %d SEEING soldier %d", pSoldier.ubID, pOpponent.ubID));
            // #endif

            UpdatePublic(ubTeamToRadioTo, pOpponent.ubID, pPersOL, gsLastKnownOppLoc[pSoldier.ubID, pOpponent.ubID], gbLastKnownOppLevel[pSoldier.ubID, pOpponent.ubID]);
        }


        // if soldier heard a misc noise more important that his team's public one
        if (pSoldier.ubNoiseVolume > gubPublicNoiseVolume[ubTeamToRadioTo])
        {
            // replace the soldier's team's public noise with his
            gsPublicNoiseGridno[ubTeamToRadioTo] = pSoldier.sNoiseGridno;
            gbPublicNoiseLevel[ubTeamToRadioTo] = pSoldier.bNoiseLevel;
            gubPublicNoiseVolume[ubTeamToRadioTo] = pSoldier.ubNoiseVolume;
        }


        // if this soldier is on the local team
        if (PTR_OURTEAM)
        {
            // don't trigger sighting quotes or stop merc's movement if everyone visible
            //if (!(gTacticalStatus.uiFlags.HasFlag(SHOW_ALL_MERCS)))
            {
                // if we've revealed any enemies, or seen any previously unknown enemies
                if (revealedEnemies || unknownEnemies)
                {
                    // First check for a virgin map and set to false if we see our first guy....
                    // Only if this guy is an ememy!

                    OurTeamSeesSomeone(pSoldier, revealedEnemies, unknownEnemies);
                }
                else if (fSawCreatureForFirstTime)
                {
                    gMercProfiles[pSoldier.ubProfile].ubMiscFlags |= ProfileMiscFlags1.PROFILE_MISC_FLAG_HAVESEENCREATURE;
                    TacticalCharacterDialogue(pSoldier, QUOTE_FIRSTTIME_GAME_SEE_CREATURE);
                }

            }
        }
    }

    void DebugSoldierPage1()
    {
        SOLDIERTYPE? pSoldier;
        int usSoldierIndex;
        int uiMercFlags;
        int usMapPos;
        int ubLine = 0;

        if (FindSoldierFromMouse(out usSoldierIndex, out uiMercFlags))
        {
            // Get Soldier
            Overhead.GetSoldier(out pSoldier, usSoldierIndex);

            FontSubSystem.SetFont(FontStyle.LARGEFONT1);
            gprintf(0, 0, "DEBUG SOLDIER PAGE ONE, GRIDNO %d", pSoldier.sGridNo);
            FontSubSystem.SetFont(FontStyle.LARGEFONT1);

            ubLine = 2;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "ID:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.ubID);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "TEAM:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.bTeam);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "SIDE:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.bSide);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "STATUS FLAGS:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%x", pSoldier.uiStatusFlags);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "HUMAN:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", gTacticalStatus.Team[pSoldier.bTeam].bHuman);
            ubLine++;
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "APs:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.bActionPoints);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Breath:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.bBreath);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Life:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.bLife);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "LifeMax:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.bLifeMax);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Bleeding:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.bBleeding);

            ubLine = 2;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(200, LINE_HEIGHT * ubLine, "Agility:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(350, LINE_HEIGHT * ubLine, "%d ( %d )", pSoldier.bAgility, EffectiveAgility(pSoldier));
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(200, LINE_HEIGHT * ubLine, "Dexterity:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(350, LINE_HEIGHT * ubLine, "%d( %d )", pSoldier.bDexterity, EffectiveDexterity(pSoldier));
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(200, LINE_HEIGHT * ubLine, "Strength:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(350, LINE_HEIGHT * ubLine, "%d", pSoldier.bStrength);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(200, LINE_HEIGHT * ubLine, "Wisdom:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(350, LINE_HEIGHT * ubLine, "%d ( %d )", pSoldier.bWisdom, EffectiveWisdom(pSoldier));
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(200, LINE_HEIGHT * ubLine, "Exp Lvl:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(350, LINE_HEIGHT * ubLine, "%d ( %d )", pSoldier.bExpLevel, EffectiveExpLevel(pSoldier));
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(200, LINE_HEIGHT * ubLine, "Mrksmnship:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(350, LINE_HEIGHT * ubLine, "%d ( %d )", pSoldier.bMarksmanship, EffectiveMarksmanship(pSoldier));
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(200, LINE_HEIGHT * ubLine, "Mechanical:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(350, LINE_HEIGHT * ubLine, "%d", pSoldier.bMechanical);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(200, LINE_HEIGHT * ubLine, "Explosive:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(350, LINE_HEIGHT * ubLine, "%d", pSoldier.bExplosive);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(200, LINE_HEIGHT * ubLine, "Medical:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(350, LINE_HEIGHT * ubLine, "%d", pSoldier.bMedical);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(200, LINE_HEIGHT * ubLine, "Drug Effects:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(400, LINE_HEIGHT * ubLine, "%d", pSoldier.bDrugEffect[0]);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(200, LINE_HEIGHT * ubLine, "Drug Side Effects:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(400, LINE_HEIGHT * ubLine, "%d", pSoldier.bDrugSideEffect[0]);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(200, LINE_HEIGHT * ubLine, "Booze Effects:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(400, LINE_HEIGHT * ubLine, "%d", pSoldier.bDrugEffect[1]);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(200, LINE_HEIGHT * ubLine, "Hangover Side Effects:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(400, LINE_HEIGHT * ubLine, "%d", pSoldier.bDrugSideEffect[1]);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(200, LINE_HEIGHT * ubLine, "AI has Keys:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(400, LINE_HEIGHT * ubLine, "%d", pSoldier.bHasKeys);
            ubLine++;
        }
        else if (GetMouseMapPos(out usMapPos))
        {
            FontSubSystem.SetFont(FontStyle.LARGEFONT1);
            gprintf(0, 0, "DEBUG LAND PAGE ONE");
            FontSubSystem.SetFont(FontStyle.LARGEFONT1);

            ubLine++;
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Num dirty rects:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(200, LINE_HEIGHT * ubLine, "%d", guiNumBackSaves);
            ubLine++;


        }

    }

    void DebugSoldierPage2()
    {
        SOLDIERTYPE? pSoldier;
        int usSoldierIndex;
        int uiMercFlags;
        int usMapPos;
        TILE_ELEMENT TileElem;
        LEVELNODE? pNode;
        int ubLine;

        if (FindSoldierFromMouse(out usSoldierIndex, out uiMercFlags))
        {
            // Get Soldier
            Overhead.GetSoldier(out pSoldier, usSoldierIndex);

            FontSubSystem.SetFont(FontStyle.LARGEFONT1);
            gprintf(0, 0, "DEBUG SOLDIER PAGE TWO, GRIDNO %d", pSoldier.sGridNo);
            FontSubSystem.SetFont(FontStyle.LARGEFONT1);

            ubLine = 2;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "ID:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.ubID);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Body Type:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.ubBodyType);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Opp Cnt:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.bOppCnt);
            ubLine++;

            if (pSoldier.bTeam == OUR_TEAM || pSoldier.bTeam == MILITIA_TEAM) // look at 8 to 15 opplist entries
            {
                FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
                gprintf(0, LINE_HEIGHT * ubLine, "Opplist B:");
                FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
                gprintf(150, LINE_HEIGHT * ubLine, "%d %d %d %d %d %d %d %d", pSoldier.bOppList[20], pSoldier.bOppList[21], pSoldier.bOppList[22],
                                pSoldier.bOppList[23], pSoldier.bOppList[24], pSoldier.bOppList[25], pSoldier.bOppList[26], pSoldier.bOppList[27]);
                ubLine++;
            }
            else    // team 1 - enemies so look at first 8 (0-7) opplist entries
            {
                FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
                gprintf(0, LINE_HEIGHT * ubLine, "OppList A:");
                FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
                gprintf(150, LINE_HEIGHT * ubLine, "%d %d %d %d %d %d %d %d", pSoldier.bOppList[0], pSoldier.bOppList[1], pSoldier.bOppList[2],
                                pSoldier.bOppList[3], pSoldier.bOppList[4], pSoldier.bOppList[5], pSoldier.bOppList[6],
                                pSoldier.bOppList[7]);
                ubLine++;
            }

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Visible:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.bVisible);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Direction:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%S", gzDirectionStr[pSoldier.bDirection]);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "DesDirection:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%S", gzDirectionStr[pSoldier.bDesiredDirection]);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "GridNo:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.sGridNo);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Dest:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.sFinalDestination);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Path Size:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.usPathDataSize);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Path Index:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.usPathIndex);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "First 3 Steps:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d %d %d", pSoldier.usPathingData[0],
            pSoldier.usPathingData[1],
            pSoldier.usPathingData[2]);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Next 3 Steps:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d %d %d", pSoldier.usPathingData[pSoldier.usPathIndex],
            pSoldier.usPathingData[pSoldier.usPathIndex + 1],
            pSoldier.usPathingData[pSoldier.usPathIndex + 2]);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "FlashInd:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.fFlashLocator);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "ShowInd:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.fShowLocator);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Main hand:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%s", ShortItemNames[pSoldier.inv[InventorySlot.HANDPOS].usItem]);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Second hand:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%s", ShortItemNames[pSoldier.inv[InventorySlot.SECONDHANDPOS].usItem]);
            ubLine++;

            if (GetMouseMapPos(out usMapPos))
            {
                FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
                gprintf(0, LINE_HEIGHT * ubLine, "CurrGridNo:");
                FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
                gprintf(150, LINE_HEIGHT * ubLine, "%d", usMapPos);
                ubLine++;
            }

        }
        else if (GetMouseMapPos(out usMapPos))
        {
            FontSubSystem.SetFont(FontStyle.LARGEFONT1);
            gprintf(0, 0, "DEBUG LAND PAGE TWO");
            FontSubSystem.SetFont(FontStyle.LARGEFONT1);

            ubLine = 1;

            FontSubSystem.SetFontColors(COLOR1);
            mprintf(0, LINE_HEIGHT * ubLine, "Land Raised:");
            FontSubSystem.SetFontColors(COLOR2);
            mprintf(150, LINE_HEIGHT * ubLine, "%d", gpWorldLevelData[usMapPos].sHeight);
            ubLine++;

            FontSubSystem.SetFontColors(COLOR1);
            mprintf(0, LINE_HEIGHT * ubLine, "Land Node:");
            FontSubSystem.SetFontColors(COLOR2);
            mprintf(150, LINE_HEIGHT * ubLine, "%x", gpWorldLevelData[usMapPos].pLandHead);
            ubLine++;

            if (gpWorldLevelData[usMapPos].pLandHead != null)
            {
                FontSubSystem.SetFontColors(COLOR1);
                mprintf(0, LINE_HEIGHT * ubLine, "Land Node:");
                FontSubSystem.SetFontColors(COLOR2);
                mprintf(150, LINE_HEIGHT * ubLine, "%d", gpWorldLevelData[usMapPos].pLandHead.usIndex);
                ubLine++;

                TileElem = gTileDatabase[gpWorldLevelData[usMapPos].pLandHead.usIndex];

                // Check for full tile
                FontSubSystem.SetFontColors(COLOR1);
                mprintf(0, LINE_HEIGHT * ubLine, "Full Land:");
                FontSubSystem.SetFontColors(COLOR2);
                mprintf(150, LINE_HEIGHT * ubLine, "%d", TileElem.ubFullTile);
                ubLine++;
            }

            FontSubSystem.SetFontColors(COLOR1);
            mprintf(0, LINE_HEIGHT * ubLine, "Land St Node:");
            FontSubSystem.SetFontColors(COLOR2);
            mprintf(150, LINE_HEIGHT * ubLine, "%x", gpWorldLevelData[usMapPos].pLandStart);
            ubLine++;

            FontSubSystem.SetFontColors(COLOR1);
            mprintf(0, LINE_HEIGHT * ubLine, "GRIDNO:");
            FontSubSystem.SetFontColors(COLOR2);
            mprintf(150, LINE_HEIGHT * ubLine, "%d", usMapPos);
            ubLine++;

            if (gpWorldLevelData[usMapPos].uiFlags.HasFlag(MAPELEMENTFLAGS.MOVEMENT_RESERVED))
            {
                FontSubSystem.SetFontColors(COLOR2);
                mprintf(0, LINE_HEIGHT * ubLine, "Merc: %d", gpWorldLevelData[usMapPos].ubReservedSoldierID);
                FontSubSystem.SetFontColors(COLOR2);
                mprintf(150, LINE_HEIGHT * ubLine, "RESERVED MOVEMENT FLAG ON:");
                ubLine++;
            }


            pNode = GetCurInteractiveTile();

            if (pNode != null)
            {
                FontSubSystem.SetFontColors(COLOR2);
                mprintf(0, LINE_HEIGHT * ubLine, "Tile: %d", pNode.usIndex);
                FontSubSystem.SetFontColors(COLOR2);
                mprintf(150, LINE_HEIGHT * ubLine, "ON INT TILE");
                ubLine++;
            }


            if (gpWorldLevelData[usMapPos].uiFlags.HasFlag(MAPELEMENTFLAGS.REVEALED))
            {
                FontSubSystem.SetFontColors(COLOR2);
                //mprintf( 0, LINE_HEIGHT * 9, "Merc: %d",  gpWorldLevelData[ usMapPos ].ubReservedSoldierID );			
                FontSubSystem.SetFontColors(COLOR2);
                mprintf(150, LINE_HEIGHT * ubLine, "REVEALED");
                ubLine++;
            }

            if (gpWorldLevelData[usMapPos].uiFlags.HasFlag(MAPELEMENTFLAGS.RAISE_LAND_START))
            {
                FontSubSystem.SetFontColors(COLOR2);
                //mprintf( 0, LINE_HEIGHT * 9, "Merc: %d",  gpWorldLevelData[ usMapPos ].ubReservedSoldierID );			
                FontSubSystem.SetFontColors(COLOR2);
                mprintf(150, LINE_HEIGHT * ubLine, "Land Raise Start");
                ubLine++;
            }

            if (gpWorldLevelData[usMapPos].uiFlags.HasFlag(MAPELEMENTFLAGS.RAISE_LAND_END))
            {
                FontSubSystem.SetFontColors(COLOR2);
                //mprintf( 0, LINE_HEIGHT * 9, "Merc: %d",  gpWorldLevelData[ usMapPos ].ubReservedSoldierID );			
                FontSubSystem.SetFontColors(COLOR2);
                mprintf(150, LINE_HEIGHT * ubLine, "Raise Land End");
                ubLine++;
            }

            if (gubWorldRoomInfo[usMapPos] != NO_ROOM)
            {
                FontSubSystem.SetFontColors(COLOR2);
                mprintf(0, LINE_HEIGHT * ubLine, "Room Number");
                FontSubSystem.SetFontColors(COLOR2);
                mprintf(150, LINE_HEIGHT * ubLine, "%d", gubWorldRoomInfo[usMapPos]);
                ubLine++;
            }

            if (gpWorldLevelData[usMapPos].ubExtFlags[0].HasFlag(MAPELEMENTFLAGS.EXT_NOBURN_STRUCT))
            {
                FontSubSystem.SetFontColors(COLOR2);
                mprintf(0, LINE_HEIGHT * ubLine, "Don't Use Burn Through For Soldier");
                ubLine++;
            }

        }

    }


    void DebugSoldierPage3()
    {
        SOLDIERTYPE? pSoldier;
        int usSoldierIndex;
        int uiMercFlags;
        int usMapPos;
        int ubLine;

        if (FindSoldierFromMouse(out usSoldierIndex, out uiMercFlags))
        {
            // Get Soldier
            Overhead.GetSoldier(out pSoldier, usSoldierIndex);

            FontSubSystem.SetFont(FontStyle.LARGEFONT1);
            gprintf(0, 0, "DEBUG SOLDIER PAGE THREE, GRIDNO %d", pSoldier.sGridNo);
            FontSubSystem.SetFont(FontStyle.LARGEFONT1);

            ubLine = 2;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "ID:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.ubID);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Action:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%S", gzActionStr[pSoldier.bAction]);
            if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.ENEMY))
            {
                gprintf(350, LINE_HEIGHT * ubLine, "Alert %S", gzAlertStr[pSoldier.bAlertStatus]);
            }
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Action Data:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.usActionData);

            if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.ENEMY))
            {
                gprintf(350, LINE_HEIGHT * ubLine, "AIMorale %d", pSoldier.bAIMorale);
            }
            else
            {
                gprintf(350, LINE_HEIGHT * ubLine, "Morale %d", pSoldier.bMorale);
            }
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Delayed Movement:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.fDelayedMovement);
            if (gubWatchedLocPoints[pSoldier.ubID, 0] > 0)
            {
                gprintf(350, LINE_HEIGHT * ubLine, "Watch %d/%d for %d pts",
                    gsWatchedLoc[pSoldier.ubID, 0],
                    gbWatchedLocLevel[pSoldier.ubID, 0],
                    gubWatchedLocPoints[pSoldier.ubID, 0]
                    );
            }

            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "ActionInProg:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.bActionInProgress);
            ubLine++;
            if (gubWatchedLocPoints[pSoldier.ubID, 1] > 0)
            {
                gprintf(350, LINE_HEIGHT * ubLine, "Watch %d/%d for %d pts",
                    gsWatchedLoc[pSoldier.ubID, 1],
                    gbWatchedLocLevel[pSoldier.ubID, 1],
                    gubWatchedLocPoints[pSoldier.ubID, 1]
                    );
            }

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Last Action:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%S", gzActionStr[pSoldier.bLastAction]);
            ubLine++;

            if (gubWatchedLocPoints[pSoldier.ubID, 2] > 0)
            {
                gprintf(350, LINE_HEIGHT * ubLine, "Watch %d/%d for %d pts",
                    gsWatchedLoc[pSoldier.ubID, 2],
                    gbWatchedLocLevel[pSoldier.ubID, 2],
                    gubWatchedLocPoints[pSoldier.ubID, 2]
                    );
            }

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Animation:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%S", gAnimControl[pSoldier.usAnimState].zAnimStr);
            ubLine++;

            /*
                    if ( gubWatchedLocPoints[ pSoldier.ubID ,  3 ] > 0 )
                    {
                        gprintf( 350, LINE_HEIGHT * ubLine, "Watch %d/%d for %d pts", 
                            gsWatchedLoc[ pSoldier.ubID ,  3 ], 
                            gbWatchedLocLevel[ pSoldier.ubID ,  3 ], 
                            gubWatchedLocPoints[ pSoldier.ubID ,  3 ]
                            );
                    }
            */

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Getting Hit:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.fGettingHit);

            if (pSoldier.ubCivilianGroup != 0)
            {
                gprintf(350, LINE_HEIGHT * ubLine, "Civ group %d", pSoldier.ubCivilianGroup);
            }
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Suppress pts:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.ubSuppressionPoints);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Attacker ID:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.ubAttackerID);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "EndAINotCalled:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.fTurnInProgress);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "PrevAnimation:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%S", gAnimControl[pSoldier.usOldAniState].zAnimStr);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "PrevAniCode:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", gusAnimInst[pSoldier.usOldAniState, pSoldier.sOldAniCode]);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "GridNo:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.sGridNo);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "AniCode:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", gusAnimInst[pSoldier.usAnimState, pSoldier.usAniCode]);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "No APS To fin Move:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.fNoAPToFinishMove);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Reload Delay:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.sReloadDelay);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Reloading:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.fReloading);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Bullets out:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.bBulletsLeft);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Anim non-int:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.fInNonintAnim);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "RT Anim non-int:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.fRTInNonintAnim);
            ubLine++;

            // OPIONION OF SELECTED MERC
            if (gusSelectedSoldier != Globals.NOBODY && (Globals.MercPtrs[gusSelectedSoldier].ubProfile < FIRST_NPC) && pSoldier.ubProfile != NO_PROFILE)
            {
                FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
                gprintf(0, LINE_HEIGHT * ubLine, "NPC Opinion:");
                FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
                gprintf(150, LINE_HEIGHT * ubLine, "%d", gMercProfiles[pSoldier.ubProfile].bMercOpinion[Globals.MercPtrs[gusSelectedSoldier].ubProfile]);
                ubLine++;
            }

        }
        else if (GetMouseMapPos(out usMapPos))
        {
            DOOR_STATUS? pDoorStatus;
            STRUCTURE? pStructure;

            FontSubSystem.SetFont(FontStyle.LARGEFONT1);
            gprintf(0, 0, "DEBUG LAND PAGE THREE");
            FontSubSystem.SetFont(FontStyle.LARGEFONT1);

            // OK, display door information here.....
            pDoorStatus = GetDoorStatus(usMapPos);

            ubLine = 1;

            if (pDoorStatus == null)
            {
                FontSubSystem.SetFontColors(COLOR1);
                mprintf(0, LINE_HEIGHT * ubLine, "No Door Status");
                ubLine++;
                ubLine++;
                ubLine++;
            }
            else
            {
                FontSubSystem.SetFontColors(COLOR1);
                mprintf(0, LINE_HEIGHT * ubLine, "Door Status Found:");
                FontSubSystem.SetFontColors(COLOR2);
                mprintf(150, LINE_HEIGHT * ubLine, " %d", usMapPos);
                ubLine++;

                FontSubSystem.SetFontColors(COLOR1);
                mprintf(0, LINE_HEIGHT * ubLine, "Actual Status:");
                FontSubSystem.SetFontColors(COLOR2);

                if (pDoorStatus.ubFlags & DOOR_OPEN)
                {
                    mprintf(200, LINE_HEIGHT * ubLine, "OPEN");
                }
                else
                {
                    mprintf(200, LINE_HEIGHT * ubLine, "CLOSED");
                }
                ubLine++;


                FontSubSystem.SetFontColors(COLOR1);
                mprintf(0, LINE_HEIGHT * ubLine, "Perceived Status:");
                FontSubSystem.SetFontColors(COLOR2);

                if (pDoorStatus.ubFlags & DOOR_PERCEIVED_NOTSET)
                {
                    mprintf(200, LINE_HEIGHT * ubLine, "NOT SET");
                }
                else
                {
                    if (pDoorStatus.ubFlags & DOOR_PERCEIVED_OPEN)
                    {
                        mprintf(200, LINE_HEIGHT * ubLine, "OPEN");
                    }
                    else
                    {
                        mprintf(200, LINE_HEIGHT * ubLine, "CLOSED");
                    }
                }
                ubLine++;
            }

            //Find struct data and se what it says......
            pStructure = StructureInternals.FindStructure(usMapPos, STRUCTUREFLAGS.ANYDOOR);

            if (pStructure == null)
            {
                FontSubSystem.SetFontColors(COLOR1);
                mprintf(0, LINE_HEIGHT * ubLine, "No Door Struct Data");
                ubLine++;
            }
            else
            {

                FontSubSystem.SetFontColors(COLOR1);
                mprintf(0, LINE_HEIGHT * ubLine, "State:");
                FontSubSystem.SetFontColors(COLOR2);
                if (!(pStructure.fFlags.HasFlag(STRUCTUREFLAGS.OPEN)))
                {
                    mprintf(200, LINE_HEIGHT * ubLine, "CLOSED");
                }
                else
                {
                    mprintf(200, LINE_HEIGHT * ubLine, "OPEN");
                }
                ubLine++;
            }
        }

    }

    void AppendAttachmentCode(Items usItem, ref string str)
    {
        str = usItem switch
        {
            Items.SILENCER => wcscat(str, " Sil"),
            Items.SNIPERSCOPE => wcscat(str, " Scp"),
            Items.BIPOD => wcscat(str, " Bip"),
            Items.LASERSCOPE => wcscat(str, " Las"),
            _ => string.Empty,
        };
    }

    void WriteQuantityAndAttachments(OBJECTTYPE? pObject, int yp)
    {
        int[] szAttach = new int[30];
        bool fAttachments;
        //100%  Qty: 2  Attach:
        //100%  Qty: 2  
        //100%  Attach:
        //100%
        if (!pObject.usItem)
        {
            return;
        }
        //Build attachment string
        fAttachments = false;
        if (pObject.usAttachItem[0] || pObject.usAttachItem[1] ||
              pObject.usAttachItem[2] || pObject.usAttachItem[3])
        {
            fAttachments = true;
            szAttach = wprintf("(");
            AppendAttachmentCode(pObject.usAttachItem[0], szAttach);
            AppendAttachmentCode(pObject.usAttachItem[1], szAttach);
            AppendAttachmentCode(pObject.usAttachItem[2], szAttach);
            AppendAttachmentCode(pObject.usAttachItem[3], szAttach);
            wcscat(szAttach, " )");
        }

        if (Item[pObject.usItem].usItemClass == IC.AMMO)
        { //ammo
            if (pObject.ubNumberOfObjects > 1)
            {
                string str = string.Empty;
                string temp = string.Empty;
                int i;
                str = wprintf("Clips:  %d  (%d", pObject.ubNumberOfObjects, pObject.bStatus[0]);
                for (i = 1; i < pObject.ubNumberOfObjects; i++)
                {
                    temp = wprintf(", %d", pObject.bStatus[0]);
                    wcscat(str, temp);
                }

                wcscat(str, ")");
                gprintf(320, yp, str);
            }
            else
            {
                gprintf(320, yp, "%d rounds", pObject.bStatus[0]);
            }

            return;
        }
        if (pObject.ubNumberOfObjects > 1 && fAttachments)
        { //everything
            gprintf(320, yp, "%d%%  Qty:  %d  %s",
                pObject.bStatus[0], pObject.ubNumberOfObjects, szAttach);
        }
        else if (pObject.ubNumberOfObjects > 1)
        { //condition and quantity
            gprintf(320, yp, "%d%%  Qty:  %d  ",
                pObject.bStatus[0], pObject.ubNumberOfObjects);
        }
        else if (fAttachments)
        { //condition and attachments
            gprintf(320, yp, "%d%%  %s", pObject.bStatus[0], szAttach);
        }
        else
        { //condition
            gprintf(320, yp, "%d%%", pObject.bStatus[0]);
        }
    }

    void DebugSoldierPage4()
    {
        SOLDIERTYPE? pSoldier;
        int uiMercFlags;
        string szOrders = string.Empty;
        string szAttitude = string.Empty;
        int usSoldierIndex;
        int ubLine;

        if (FindSoldierFromMouse(out usSoldierIndex, out uiMercFlags))
        {
            // Get Soldier
            Overhead.GetSoldier(out pSoldier, usSoldierIndex);

            FontSubSystem.SetFont(FontStyle.LARGEFONT1);
            gprintf(0, 0, "DEBUG SOLDIER PAGE FOUR, GRIDNO %d", pSoldier.sGridNo);
            FontSubSystem.SetFont(FontStyle.LARGEFONT1);
            ubLine = 2;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "Exp. Level:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.bExpLevel);
            switch (pSoldier.ubSoldierClass)
            {
                case SOLDIER_CLASS.ADMINISTRATOR:
                    gprintf(320, LINE_HEIGHT * ubLine, "(Administrator)");
                    break;
                case SOLDIER_CLASS.ELITE:
                    gprintf(320, LINE_HEIGHT * ubLine, "(Army Elite)");
                    break;
                case SOLDIER_CLASS.ARMY:
                    gprintf(320, LINE_HEIGHT * ubLine, "(Army Troop)");
                    break;
                case SOLDIER_CLASS.CREATURE:
                    gprintf(320, LINE_HEIGHT * ubLine, "(Creature)");
                    break;
                case SOLDIER_CLASS.GREEN_MILITIA:
                    gprintf(320, LINE_HEIGHT * ubLine, "(Green Militia)");
                    break;
                case SOLDIER_CLASS.REG_MILITIA:
                    gprintf(320, LINE_HEIGHT * ubLine, "(Reg Militia)");
                    break;
                case SOLDIER_CLASS.ELITE_MILITIA:
                    gprintf(320, LINE_HEIGHT * ubLine, "(Elite Militia)");
                    break;
                case SOLDIER_CLASS.MINER:
                    gprintf(320, LINE_HEIGHT * ubLine, "(Miner)");
                    break;
                default:
                    break; //don't care (don't write anything)
            }
            ubLine++;

            if (pSoldier.bTeam != OUR_TEAM)
            {
                SOLDIERINITNODE? pNode;
                szOrders = pSoldier.bOrders switch
                {
                    Orders.STATIONARY => wprintf("STATIONARY"),
                    Orders.ONGUARD => wprintf("ON GUARD"),
                    Orders.ONCALL => wprintf("ON CALL"),
                    Orders.SEEKENEMY => wprintf("SEEK ENEMY"),
                    Orders.CLOSEPATROL => wprintf("CLOSE PATROL"),
                    Orders.FARPATROL => wprintf("FAR PATROL"),
                    Orders.POINTPATROL => wprintf("POINT PATROL"),
                    Orders.RNDPTPATROL => wprintf("RND PT PATROL"),
                    _ => wprintf("UNKNOWN"),
                };

                szAttitude = pSoldier.bAttitude switch
                {
                    Attitudes.DEFENSIVE => wprintf("DEFENSIVE"),
                    Attitudes.BRAVESOLO => wprintf("BRAVE SOLO"),
                    Attitudes.BRAVEAID => wprintf("BRAVE AID"),
                    Attitudes.AGGRESSIVE => wprintf("AGGRESSIVE"),
                    Attitudes.CUNNINGSOLO => wprintf("CUNNING SOLO"),
                    Attitudes.CUNNINGAID => wprintf("CUNNING AID"),
                    _ => wprintf("UNKNOWN"),
                };

                pNode = gSoldierInitHead;
                while (pNode)
                {
                    if (pNode.pSoldier == pSoldier)
                    {
                        break;
                    }

                    pNode = pNode.next;
                }
                FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
                if (pNode)
                {
                    gprintf(0, LINE_HEIGHT * ubLine, "%s, %s, REL EQUIP: %d, REL ATTR: %d",
                        szOrders, szAttitude, pNode.pBasicPlacement.bRelativeEquipmentLevel,
                        pNode.pBasicPlacement.bRelativeAttributeLevel);
                }
                else
                {
                    gprintf(0, LINE_HEIGHT * ubLine, "%s, %s", szOrders, szAttitude);
                }
                ubLine++;
            }

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "ID:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            gprintf(150, LINE_HEIGHT * ubLine, "%d", pSoldier.ubID);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "HELMETPOS:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            if (pSoldier.inv[HELMETPOS].usItem)
            {
                gprintf(150, LINE_HEIGHT * ubLine, "%s", ShortItemNames[pSoldier.inv[HELMETPOS].usItem]);
            }

            WriteQuantityAndAttachments(&pSoldier.inv[HELMETPOS], LINE_HEIGHT * ubLine);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "VESTPOS:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            if (pSoldier.inv[VESTPOS].usItem)
            {
                gprintf(150, LINE_HEIGHT * ubLine, "%s", ShortItemNames[pSoldier.inv[VESTPOS].usItem]);
            }

            WriteQuantityAndAttachments(&pSoldier.inv[VESTPOS], LINE_HEIGHT * ubLine);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "LEGPOS:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            if (pSoldier.inv[LEGPOS].usItem)
            {
                gprintf(150, LINE_HEIGHT * ubLine, "%s", ShortItemNames[pSoldier.inv[LEGPOS].usItem]);
            }

            WriteQuantityAndAttachments(&pSoldier.inv[LEGPOS], LINE_HEIGHT * ubLine);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "HEAD1POS:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            if (pSoldier.inv[HEAD1POS].usItem)
            {
                gprintf(150, LINE_HEIGHT * ubLine, "%s", ShortItemNames[pSoldier.inv[HEAD1POS].usItem]);
            }

            WriteQuantityAndAttachments(&pSoldier.inv[HEAD1POS], LINE_HEIGHT * ubLine);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "HEAD2POS:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            if (pSoldier.inv[HEAD2POS].usItem)
            {
                gprintf(150, LINE_HEIGHT * ubLine, "%s", ShortItemNames[pSoldier.inv[HEAD2POS].usItem]);
            }

            WriteQuantityAndAttachments(&pSoldier.inv[HEAD2POS], LINE_HEIGHT * ubLine);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "HANDPOS:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            if (pSoldier.inv[InventorySlot.HANDPOS].usItem)
            {
                gprintf(150, LINE_HEIGHT * ubLine, "%s", ShortItemNames[pSoldier.inv[InventorySlot.HANDPOS].usItem]);
            }

            WriteQuantityAndAttachments(&pSoldier.inv[InventorySlot.HANDPOS], LINE_HEIGHT * ubLine);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "SECONDHANDPOS:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            if (pSoldier.inv[InventorySlot.SECONDHANDPOS].usItem)
            {
                gprintf(150, LINE_HEIGHT * ubLine, "%s", ShortItemNames[pSoldier.inv[InventorySlot.SECONDHANDPOS].usItem]);
            }

            WriteQuantityAndAttachments(&pSoldier.inv[InventorySlot.SECONDHANDPOS], LINE_HEIGHT * ubLine);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "BIGPOCK1POS:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            if (pSoldier.inv[BIGPOCK1POS].usItem)
            {
                gprintf(150, LINE_HEIGHT * ubLine, "%s", ShortItemNames[pSoldier.inv[BIGPOCK1POS].usItem]);
            }

            WriteQuantityAndAttachments(&pSoldier.inv[BIGPOCK1POS], LINE_HEIGHT * ubLine);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "BIGPOCK2POS:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            if (pSoldier.inv[BIGPOCK2POS].usItem)
            {
                gprintf(150, LINE_HEIGHT * ubLine, "%s", ShortItemNames[pSoldier.inv[BIGPOCK2POS].usItem]);
            }

            WriteQuantityAndAttachments(&pSoldier.inv[BIGPOCK2POS], LINE_HEIGHT * ubLine);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "BIGPOCK3POS:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            if (pSoldier.inv[BIGPOCK3POS].usItem)
            {
                gprintf(150, LINE_HEIGHT * ubLine, "%s", ShortItemNames[pSoldier.inv[BIGPOCK3POS].usItem]);
            }

            WriteQuantityAndAttachments(&pSoldier.inv[BIGPOCK3POS], LINE_HEIGHT * ubLine);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "BIGPOCK4POS:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            if (pSoldier.inv[BIGPOCK4POS].usItem)
            {
                gprintf(150, LINE_HEIGHT * ubLine, "%s", ShortItemNames[pSoldier.inv[BIGPOCK4POS].usItem]);
            }

            WriteQuantityAndAttachments(&pSoldier.inv[BIGPOCK4POS], LINE_HEIGHT * ubLine);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "SMALLPOCK1POS:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            if (pSoldier.inv[SMALLPOCK1POS].usItem)
            {
                gprintf(150, LINE_HEIGHT * ubLine, "%s", ShortItemNames[pSoldier.inv[SMALLPOCK1POS].usItem]);
            }

            WriteQuantityAndAttachments(&pSoldier.inv[SMALLPOCK1POS], LINE_HEIGHT * ubLine);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "SMALLPOCK2POS:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            if (pSoldier.inv[SMALLPOCK2POS].usItem)
            {
                gprintf(150, LINE_HEIGHT * ubLine, "%s", ShortItemNames[pSoldier.inv[SMALLPOCK2POS].usItem]);
            }

            WriteQuantityAndAttachments(&pSoldier.inv[SMALLPOCK2POS], LINE_HEIGHT * ubLine);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "SMALLPOCK3POS:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            if (pSoldier.inv[SMALLPOCK3POS].usItem)
            {
                gprintf(150, LINE_HEIGHT * ubLine, "%s", ShortItemNames[pSoldier.inv[SMALLPOCK3POS].usItem]);
            }

            WriteQuantityAndAttachments(&pSoldier.inv[SMALLPOCK3POS], LINE_HEIGHT * ubLine);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "SMALLPOCK4POS:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            if (pSoldier.inv[SMALLPOCK4POS].usItem)
            {
                gprintf(150, LINE_HEIGHT * ubLine, "%s", ShortItemNames[pSoldier.inv[SMALLPOCK4POS].usItem]);
            }

            WriteQuantityAndAttachments(&pSoldier.inv[SMALLPOCK4POS], LINE_HEIGHT * ubLine);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "SMALLPOCK5POS:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            if (pSoldier.inv[SMALLPOCK5POS].usItem)
            {
                gprintf(150, LINE_HEIGHT * ubLine, "%s", ShortItemNames[pSoldier.inv[SMALLPOCK5POS].usItem]);
            }

            WriteQuantityAndAttachments(&pSoldier.inv[SMALLPOCK5POS], LINE_HEIGHT * ubLine);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "SMALLPOCK6POS:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            if (pSoldier.inv[SMALLPOCK6POS].usItem)
            {
                gprintf(150, LINE_HEIGHT * ubLine, "%s", ShortItemNames[pSoldier.inv[SMALLPOCK6POS].usItem]);
            }

            WriteQuantityAndAttachments(&pSoldier.inv[SMALLPOCK6POS], LINE_HEIGHT * ubLine);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "SMALLPOCK7POS:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            if (pSoldier.inv[SMALLPOCK7POS].usItem)
            {
                gprintf(150, LINE_HEIGHT * ubLine, "%s", ShortItemNames[pSoldier.inv[SMALLPOCK7POS].usItem]);
            }

            WriteQuantityAndAttachments(&pSoldier.inv[SMALLPOCK7POS], LINE_HEIGHT * ubLine);
            ubLine++;

            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.GREEN);
            gprintf(0, LINE_HEIGHT * ubLine, "SMALLPOCK8POS:");
            FontSubSystem.SetFontShade(FontStyle.LARGEFONT1, FONT_SHADE.NEUTRAL);
            if (pSoldier.inv[SMALLPOCK8POS].usItem)
            {
                gprintf(150, LINE_HEIGHT * ubLine, "%s", ShortItemNames[pSoldier.inv[SMALLPOCK8POS].usItem]);
            }

            WriteQuantityAndAttachments(&pSoldier.inv[SMALLPOCK8POS], LINE_HEIGHT * ubLine);
            ubLine++;
        }
        else
        {
            FontSubSystem.SetFont(FontStyle.LARGEFONT1);
            gprintf(0, 0, "DEBUG LAND PAGE FOUR");
            FontSubSystem.SetFont(FontStyle.LARGEFONT1);
        }
    }

    // 
    // Noise stuff
    //

    int MovementNoise(SOLDIERTYPE? pSoldier)
    {
        int iStealthSkill, iRoll;
        int ubMaxVolume, ubVolume, ubBandaged, ubEffLife;
        int bInWater = 0;

        if (pSoldier.bTeam == ENEMY_TEAM)
        {
            return ((int)(MAX_MOVEMENT_NOISE - PreRandom(2)));
        }

        iStealthSkill = 20 + 4 * EffectiveExpLevel(pSoldier) + ((EffectiveDexterity(pSoldier) * 4) / 10); // 24-100

        // big bonus for those "extra stealthy" mercs
        if (pSoldier.ubBodyType == BLOODCAT)
        {
            iStealthSkill += 50;
        }
        else if (HAS_SKILL_TRAIT(pSoldier, STEALTHY))
        {
            iStealthSkill += 25 * NUM_SKILL_TRAITS(pSoldier, STEALTHY);
        }


        //NumMessage("Base Stealth = ",stealthSkill);


        ubBandaged = pSoldier.bLifeMax - pSoldier.bLife - pSoldier.bBleeding;
        ubEffLife = pSoldier.bLife + (ubBandaged / 2);

        // IF "SNEAKER'S" "EFFECTIVE LIFE" IS AT LESS THAN 50
        if (ubEffLife < 50)
        {
            // reduce effective stealth skill by up to 50% for low life
            iStealthSkill -= (iStealthSkill * (50 - ubEffLife)) / 100;
        }

        // if breath is below 50%
        if (pSoldier.bBreath < 50)
        {
            // reduce effective stealth skill by up to 50%
            iStealthSkill -= (iStealthSkill * (50 - pSoldier.bBreath)) / 100;
        }

        // if sneaker is moving through water
        if (Water(pSoldier.sGridNo))
        {
            iStealthSkill -= 10; // 10% penalty
        }
        else if (DeepWater(pSoldier.sGridNo))
        {
            iStealthSkill -= 20; // 20% penalty
        }

        if (pSoldier.bDrugEffect[DRUG_TYPE_ADRENALINE])
        {
            // minus 3 percent per bonus AP from adrenaline
            iStealthSkill -= 3 * pSoldier.bDrugEffect[DRUG_TYPE_ADRENALINE];
        }

        /*
            // if sneaker is too eager and impatient to "do it right"
            if ((pSoldier.bTrait == OVER_ENTHUS) || (pSoldier.bAttitude == AGGRESSIVE))
            {
                ubStealthSkill -= 10;	// 10% penalty
            }
        */
        //NumMessage("Modified Stealth = ",stealthSkill);

        iStealthSkill = Math.Max(iStealthSkill, 0);

        if (!pSoldier.bStealthMode)    // REGULAR movement
        {
            ubMaxVolume = MAX_MOVEMENT_NOISE - (iStealthSkill / 16);    // 9 - (0 to 6) => 3 to 9

            if (bInWater > 0)
            {
                ubMaxVolume++;      // in water, can be even louder
            }
            switch (pSoldier.usAnimState)
            {
                case AnimationStates.CRAWLING:
                    ubMaxVolume -= 2;
                    break;
                case AnimationStates.SWATTING:
                    ubMaxVolume -= 1;
                    break;
                case AnimationStates.RUNNING:
                    ubMaxVolume += 3;
                    break;
            }

            if (ubMaxVolume < 2)
            {
                ubVolume = ubMaxVolume;
            }
            else
            {
                ubVolume = 1 + (int)PreRandom(ubMaxVolume);   // actual volume is 1 to max volume
            }
        }
        else            // in STEALTH mode
        {
            iRoll = (int)PreRandom(100);  // roll them bones!

            if (iRoll >= iStealthSkill)   // v1.13 modification: give a second chance!
            {
                iRoll = (int)PreRandom(100);
            }

            if (iRoll < iStealthSkill)
            {
                ubVolume = 0;   // made it, stayed quiet moving through this tile
            }
            else    // OOPS!
            {
                ubVolume = 1 + ((iRoll - iStealthSkill + 1) / 16);  // volume is 1 - 7 ... 
                switch (pSoldier.usAnimState)
                {
                    case AnimationStates.CRAWLING:
                        ubVolume -= 2;
                        break;
                    case AnimationStates.SWATTING:
                        ubVolume -= 1;
                        break;
                    case AnimationStates.RUNNING:
                        ubVolume += 3;
                        break;
                }
                if (ubVolume < 1)
                {
                    ubVolume = 0;
                }

                // randomize at which movement step the sneaking failure will happen
                //			Status.stepsTilNoise = Globals.Random.Next(MAXMOVESTEPS);	// 0 - 6
            }
        }

        //NumMessage("Volume = ",volume);

        // save noise volume where stepped HandleSteppedLook can back get at it later
        //	Status.moveNoiseVolume = ubVolume;
        return (ubVolume);
    }

    int DoorOpeningNoise(SOLDIERTYPE? pSoldier)
    {
        int sGridNo;
        DOOR_STATUS? pDoorStatus;
        int ubDoorNoise;

        // door being opened gridno is always the pending-action-data2 value
        sGridNo = pSoldier.sPendingActionData2;
        pDoorStatus = GetDoorStatus(sGridNo);

        if (pDoorStatus && pDoorStatus.ubFlags & DOOR_HAS_TIN_CAN)
        {
            // double noise possible!
            ubDoorNoise = DOOR_NOISE.VOLUME * 3;
        }
        else
        {
            ubDoorNoise = DOOR_NOISE.VOLUME;
        }
        if (MovementNoise(pSoldier) > 0)
        {
            // failed any stealth checks
            return (ubDoorNoise);
        }
        else
        {
            // succeeded in being stealthy!
            return (0);
        }
    }

    public static void MakeNoise(int ubNoiseMaker, int sGridNo, int bLevel, int ubTerrType, int ubVolume, int ubNoiseType)
    {
        EV_S_NOISE SNoise;

        SNoise.ubNoiseMaker = ubNoiseMaker;
        SNoise.sGridNo = sGridNo;
        SNoise.bLevel = bLevel;
        SNoise.ubTerrType = ubTerrType;
        SNoise.ubVolume = ubVolume;
        SNoise.ubNoiseType = ubNoiseType;

        if (gTacticalStatus.ubAttackBusyCount > 0)
        {
            // delay these events until the attack is over!
            AddGameEvent(S_NOISE, DEMAND_EVENT_DELAY, &SNoise);
        }
        else
        {
            // AddGameEvent( S_NOISE, 0, &SNoise );

            // now call directly
            OurNoise(SNoise.ubNoiseMaker, SNoise.sGridNo, SNoise.bLevel, SNoise.ubTerrType, SNoise.ubVolume, SNoise.ubNoiseType);

        }

        /*
            int bWeControlNoise = false;

            if (ubNoiseMode == UNEXPECTED)
            {
                bWeControlNoise = true;
            }
            else	// EXPECTED noise
            {
                if (ubNoiseMaker < Globals.NOBODY)
                {
                    if (Menptr[ubNoiseMaker].controller == Net.pnum)
                    {
                        bWeControlNoise = true;
                    }
                }
                else
                {
                    // expected noise by Globals.NOBODY is sent by LEADER, received by others
                    if (Net.pnum == LEADER)
                    {
                        bWeControlNoise = true;
                    }
                }
            }

            if (bWeControlNoise)
            {
                OurNoise(ubNoiseMaker,sGridNo,ubTerrType,ubVolume,ubNoiseType,ubNoiseMode);
            }
            else
            {
                // can't be UNEXPECTED, check if it's a SEND or NO_SEND
                if (ubNoiseMode == EXPECTED_NOSEND)
                {
                    // no NET_NOISE message is required, trigger TheirNoise() right here
                    TheirNoise(ubNoiseMaker,sGridNo,ubTerrType,ubVolume,ubNoiseType,ubNoiseMode);
                }
                else
                {

                    // EXPECTED_SEND, TheirNoise() will be triggered by the arrival of the
                    // NET_NOISE message, not by us.  Wait here until that's all done...

                    // wait for the NET_NOISE to arrive (it will set noiseReceived flag)
                    //stopAction = true;		// prevent real-time events from passing us by
                    MarkTime(&LoopTime);
                    while (Status.noiseReceived != ubNoiseType)
                    {
                        LoopTimePast = Elapsed(&LoopTime);
                        if (LoopTimePast > 50 && LoopTimePast < 2000)
                        {
                            KeepInterfaceGoing(19); // xxx yyy zzz experimental Aug 16/96 9:15 pm
                        }
                        else
                        {
                            KeyHitReport("MakeNoise: Waiting for NET_NOISE, need ubNoiseType ",ubNoiseType);
                        }
                        CheckForNetIncoming();
                    };
                    //stopAction = false;	// re-enable real-time events

                    // turn off the oppChk flag again
                    Status.noiseReceived = -1;

                }
            }
        */
    }


    void OurNoise(int ubNoiseMaker, int sGridNo, int bLevel, int ubTerrType, int ubVolume, NOISE ubNoiseType)
    {
        int bSendNoise = 0;
        SOLDIERTYPE? pSoldier;


        // # if BYPASSNOISE
        //             return;
        // #endif


        // # if BETAVERSION
        //             sprintf(tempstr, "OurNoise: ubNoiseType = %s, ubNoiseMaker = %d, ubNoiseMode = %d, sGridNo = %d, ubVolume = %d",
        //                      NoiseTypeStr[ubNoiseType], ubNoiseMaker, ubNoiseMode, sGridNo, ubVolume);
        // # if RECORDNET
        //             fprintf(NetDebugFile, "\t%s\n", tempstr);
        // #endif
        // # if TESTNOISE
        //             PopMessage(tempstr);
        // #endif
        // #endif

        // see if anyone actually hears this noise, sees ubNoiseMaker, etc.
        ProcessNoise(ubNoiseMaker, sGridNo, bLevel, ubTerrType, ubVolume, ubNoiseType);

        if ((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED)) && (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)) && (ubNoiseMaker < Globals.NOBODY) && !gfDelayResolvingBestSightingDueToDoor)
        {
            pSoldier = Globals.MercPtrs[ubNoiseMaker];

            // interrupts are possible, resolve them now (we're in control here)
            // (you can't interrupt Globals.NOBODY, even if you hear the noise)

            ResolveInterruptsVs(pSoldier, NOISEINTERRUPT);
        }

    }



    void TheirNoise(int ubNoiseMaker, int sGridNo, int bLevel, int ubTerrType, int ubVolume, NOISE ubNoiseType)
    {
        //	SOLDIERTYPE *pSoldier;


        //# if BYPASSNOISE
        //            return;
        //#endif


        // # if BETAVERSION
        //             sprintf(tempstr, "TheirNoise: ubNoiseType = %s, ubNoiseMaker = %d, ubNoiseMode = %d, sGridNo = %d, ubVolume = %d",
        //                      NoiseTypeStr[ubNoiseType], ubNoiseMaker, ubNoiseMode, sGridNo, ubVolume);
        // # if RECORDNET
        //             fprintf(NetDebugFile, "\t%s\n", tempstr);
        // #endif
        // 
        // # if TESTNOISE
        //             PopMessage(tempstr);
        // #endif
        // #endif

        // see if anyone actually hears this noise, sees noiseMaker, etc.
        ProcessNoise(ubNoiseMaker, sGridNo, bLevel, ubTerrType, ubVolume, ubNoiseType);

        // if noiseMaker is SOMEBODY
        if (ubNoiseMaker < Globals.NOBODY)
        {
            /*
            pSoldier = Globals.MercPtrs[ubNoiseMaker];

            //stopAction = true;		// prevent real-time events from passing us by
            MarkTime(&LoopTime);
            do
            {
                LoopTimePast = Elapsed(&LoopTime);
                if (LoopTimePast > 50 && LoopTimePast < 2000)
                {
                    KeepInterfaceGoing(20); // xxx yyy zzz experimental Aug 16/96 9:15 pm
                }
                else
                {
                    // the gridno is added to end of the string by KeyHitReport itself...
                    sprintf(tempstr,"TheirNoise: Waiting for NOISE.INT_DONE for guynum %d, ubNoiseType %d(%s), sGridNo ",
                        pSoldier.guynum,ubNoiseType,NoiseTypeStr[ubNoiseType]);
                    KeyHitReport(tempstr,sGridNo);
                }

                CheckForNetIncoming();
            } while ((ExtMen[pSoldier.guynum].noiseRcvdGridno[ubNoiseType] != sGridNo) && pSoldier.in_sector);
            //stopAction = false;	// re-enable real-time events

            // reset the gridno flag for next time
            ExtMen[pSoldier.guynum].noiseRcvdGridno[ubNoiseType] = NOWHERE;
            */
        }
        // else if noiseMaker's Globals.NOBODY, no opplist changes or interrupts are possible
    }



    void ProcessNoise(int ubNoiseMaker, int sGridNo, int bLevel, int ubTerrType, int ubBaseVolume, NOISE ubNoiseType)
    {
        SOLDIERTYPE? pSoldier;
        int bLoop;
        TEAM bTeam;
        int ubLoudestEffVolume, ubEffVolume;
        //	int ubPlayVolume;
        int bCheckTerrain = 0;
        TerrainTypeDefines ubSourceTerrType;
        int ubSource;
        bool bTellPlayer = false, bHeard;
        bool bSeen;
        int ubHeardLoudestBy;
        WorldDirections ubLoudestNoiseDir;
        WorldDirections ubNoiseDir;


        // # if RECORDOPPLIST
        //             fprintf(OpplistFile, "PN: nType=%s, nMaker=%d, g=%d, tType=%d, bVol=%d\n",
        //                 NoiseTypeStr[noiseType], ubNoiseMaker, sGridNo, ubTerrType, baseVolume);
        // #endif

        // if the base volume itself was negligible
        if (ubBaseVolume == 0)
        {
            return;
        }


        // EXPLOSIONs are special, because they COULD be caused by a merc who is
        // no longer alive (but he placed the bomb or flaky grenade in the past).
        // Later noiseMaker gets whacked to Globals.NOBODY anyway, so that's OK.  So a
        // dead noiseMaker is only used here to decide WHICH soldiers HearNoise().

        // if noise is made by a person, AND it's not noise from an explosion
        if ((ubNoiseMaker < Globals.NOBODY) && (ubNoiseType != NOISE.EXPLOSION))
        {
            // inactive/not in sector/dead soldiers, shouldn't be making noise!
            if (!Menptr[ubNoiseMaker].bActive || !Menptr[ubNoiseMaker].bInSector ||
            Menptr[ubNoiseMaker].uiStatusFlags.HasFlag(SOLDIER.DEAD))
            {
                // # if BETAVERSION
                //                     NumMessage("ProcessNoise: ERROR - Noisemaker is inactive/not in sector/dead, Guy #", ubNoiseMaker);
                // #endif
                return;
            }

            // if he's out of life, and this isn't just his "dying scream" which is OK
            if (Menptr[ubNoiseMaker].bLife == 0 && (ubNoiseType != NOISE.SCREAM))
            {
                // # if BETAVERSION
                //                     NumMessage("ProcessNoise: ERROR - Noisemaker is lifeless, Guy #", ubNoiseMaker);
                // #endif
                return;
            }
        }


        // DETERMINE THE TERRAIN TYPE OF THE GRIDNO WHERE NOISE IS COMING FROM

        ubSourceTerrType = gpWorldLevelData[sGridNo].ubTerrainID;
        /*
            // start with the terrain type passed in to us
            ubSourceTerrType = ubTerrType;

            // if this isn't enough to get a valid terrain type
            if ((ubSourceTerrType < GROUNDTYPE) || (ubSourceTerrType > OCEANTYPE))
            {
                // use the source gridno of the noise itself
                ubSourceTerrType = TTypeList[Terrain(sGridNo)];
            }
            */

        // if we have now somehow obtained a valid terrain type
        if ((ubSourceTerrType >= TerrainTypeDefines.FLAT_GROUND) || (ubSourceTerrType <= TerrainTypeDefines.DEEP_WATER))
        {
            //NumMessage("Source Terrain Type = ",ubSourceTerrType);
            bCheckTerrain = 1;
        }
        // else give up trying to get terrain type, just assume sound isn't muffled


        // DETERMINE THE *PERCEIVED* SOURCE OF THE NOISE
        switch (ubNoiseType)
        {
            // for noise generated by an OBJECT shot/thrown/dropped by the noiseMaker
            case NOISE.ROCK_IMPACT:
                gsWhoThrewRock = ubNoiseMaker;
                // the source of the noise is not at all obvious, so hide it from
                // the listener and maintain noiseMaker's cover by making source Globals.NOBODY
                ubSource = Globals.NOBODY;
                break;
            case NOISE.BULLET_IMPACT:
            case NOISE.GRENADE_IMPACT:
            case NOISE.EXPLOSION:
                // the source of the noise is not at all obvious, so hide it from
                // the listener and maintain noiseMaker's cover by making source Globals.NOBODY
                ubSource = Globals.NOBODY;
                break;

            default:
                // normal situation: the noiseMaker is obviously the source of the noise
                ubSource = ubNoiseMaker;
                break;
        }

        // LOOP THROUGH EACH TEAM
        for (bTeam = 0; bTeam < (TEAM)MAXTEAMS; bTeam++)
        {
            // skip any inactive teams
            if (gTacticalStatus.Team[bTeam].bTeamActive == 0)
            {
                continue;
            }

            // if a the noise maker is a person, not just Globals.NOBODY
            if (ubNoiseMaker < Globals.NOBODY)
            {
                // if this team is the same TEAM as the noise maker's
                // (for now, assume we will report noises by unknown source on same SIDE)
                // OR, if the noise maker is currently in sight to this HUMAN team

                // CJC: changed to if the side is the same side as the noise maker's!
                // CJC: changed back!

                if (bTeam == Menptr[ubNoiseMaker].bTeam)
                {
                    continue;
                }

                if (gTacticalStatus.Team[bTeam].bHuman > 0)
                {
                    if (gbPublicOpplist[bTeam][ubNoiseMaker] == SEEN_CURRENTLY)
                    {
                        continue;
                    }
                }
            }

            // # if REPORTTHEIRNOISE
            //                 // if this is any team
            //                 if (true)
            // #else
            // if this is our team
            if (true)
            //if (bTeam == Net.pnum)
            // #endif
            {
                // tell player about noise if enemies are present
                bTellPlayer = gTacticalStatus.fEnemyInSector
                    && ((!gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
                    || (gTacticalStatus.ubCurrentTeam > 0));

                // # ifndef TESTNOISE
                //                         switch (ubNoiseType)
                //                         {
                //                             case NOISE.GUNFIRE:
                //                             case NOISE.BULLET_IMPACT:
                //                             case NOISE.ROCK_IMPACT:
                //                             case NOISE.GRENADE_IMPACT:
                //                                 // It's noise caused by a projectile.  If the projectile was seen by
                //                                 // the local player while in flight (PublicBullet), then don't bother
                //                                 // giving him a message about the noise it made, he's obviously aware.
                //                                 if (1 /*PublicBullet*/)
                //                                 {
                //                                     bTellPlayer = false;
                //                                 }
                // 
                //                                 break;
                // 
                //                             case NOISE.EXPLOSION:
                //                                 // if center of explosion is in visual range of team, don't report
                //                                 // noise, because the player is already watching the thing go BOOM!
                //                                 if (TeamMemberNear(bTeam, sGridNo, STRAIGHT))
                //                                 {
                //                                     bTellPlayer = false;
                //                                 }
                //                                 break;
                // 
                //                             case NOISE.SILENT_ALARM:
                //                                 bTellPlayer = false;
                //                                 break;
                //                         }
                // 
                //                         // if noise was made by a person
                //                         if (ubNoiseMaker < Globals.NOBODY)
                //                         {
                //                             // if noisemaker has been *PUBLICLY* SEEN OR HEARD during THIS TURN
                //                             if ((gbPublicOpplist[bTeam, ubNoiseMaker] == SEEN_CURRENTLY) || // seen now
                //                                 (gbPublicOpplist[bTeam, ubNoiseMaker] == SEEN_THIS_TURN) || // seen this turn
                //                                 (gbPublicOpplist[bTeam, ubNoiseMaker] == HEARD_THIS_TURN))  // heard this turn
                //                             {
                //                                 // then don't bother reporting any noise made by him to the player
                //                                 bTellPlayer = false;
                //                             }
                //                             /*
                //                             else if ( (Menptr[ubNoiseMaker].bVisible == true) && (bTeam == gbPlayerNum) )
                //                             {
                //                                 ScreenMsg( MSG_FONT_YELLOW, MSG_TESTVERSION, "Handling noise from person not currently seen in player's public opplist" );
                //                             }
                //                             */
                // 
                //                             if (Globals.MercPtrs[ubNoiseMaker].bLife == 0)
                //                             {
                //                                 // this guy is dead (just dying) so don't report to player
                //                                 bTellPlayer = false;
                //                             }
                // 
                //                         }
                //                     }
                // #endif

                // refresh flags for this new team
                bHeard = false;
                bSeen = false;
                ubLoudestEffVolume = 0;
                ubHeardLoudestBy = Globals.NOBODY;

                // All mercs on this team check if they are eligible to hear this noise
                for (bLoop = gTacticalStatus.Team[bTeam].bFirstID, pSoldier = Menptr + bLoop; bLoop <= gTacticalStatus.Team[bTeam].bLastID; bLoop++, pSoldier++)
                {
                    // if this "listener" is inactive, or in no condition to care
                    if (!pSoldier.bActive || !pSoldier.bInSector || pSoldier.uiStatusFlags.HasFlag(SOLDIER.DEAD) || (pSoldier.bLife < OKLIFE) || pSoldier.ubBodyType == LARVAE_MONSTER)
                    {
                        continue;          // skip him!
                    }

                    if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE) && pSoldier.bTeam == OUR_TEAM)
                    {
                        continue; // skip
                    }

                    if (bTeam == gbPlayerNum && pSoldier.bAssignment == Assignments.ASSIGNMENT_POW)
                    {
                        // POWs should not be processed for noise
                        continue;
                    }

                    // if a the noise maker is a person, not just Globals.NOBODY
                    if (ubNoiseMaker < Globals.NOBODY)
                    {
                        // if this listener can see this noise maker
                        if (pSoldier.bOppList[ubNoiseMaker] == SEEN_CURRENTLY)
                        {
                            // civilians care about gunshots even if they come from someone they can see
                            if (!(pSoldier.IsNeutral && ubNoiseType == NOISE.GUNFIRE))
                            {
                                continue;        // then who cares whether he can also hear the guy?
                            }
                        }

                        // screen out allied militia from hearing us
                        switch (Globals.MercPtrs[ubNoiseMaker].bTeam)
                        {
                            case OUR_TEAM:
                                // if the listener is militia and still on our side, ignore noise from us
                                if (pSoldier.bTeam == MILITIA_TEAM && pSoldier.bSide == 0)
                                {
                                    continue;
                                }
                                break;
                            case ENEMY_TEAM:
                                switch (pSoldier.ubProfile)
                                {
                                    case NPCID.WARDEN:
                                    case NPCID.GENERAL:
                                    case NPCID.SERGEANT:
                                    case NPCID.CONRAD:
                                        // ignore soldier team
                                        continue;
                                    default:
                                        break;
                                }
                                break;
                            case MILITIA_TEAM:
                                // if the noisemaker is militia and still on our side, ignore noise if we're listening
                                if (pSoldier.bTeam == OUR_TEAM && Globals.MercPtrs[ubNoiseMaker].bSide == 0)
                                {
                                    continue;
                                }
                                break;
                        }

                        if (gWorldSectorX == 5 && gWorldSectorY == MAP_ROW.N)
                        {
                            // in the bloodcat arena sector, skip noises between army & bloodcats
                            if (pSoldier.bTeam == TEAM.ENEMY_TEAM && Globals.MercPtrs[ubNoiseMaker].bTeam == TEAM.CREATURE_TEAM)
                            {
                                continue;
                            }
                            if (pSoldier.bTeam == TEAM.CREATURE_TEAM && Globals.MercPtrs[ubNoiseMaker].bTeam == ENEMY_TEAM)
                            {
                                continue;
                            }
                        }


                    }
                    else
                    {
                        // screen out allied militia from hearing us
                        if ((ubNoiseMaker == Globals.NOBODY) && pSoldier.bTeam == MILITIA_TEAM && pSoldier.bSide == 0)
                        {
                            continue;
                        }
                    }

                    if ((pSoldier.bTeam == CIV_TEAM) && (ubNoiseType == NOISE.GUNFIRE || ubNoiseType == NOISE.EXPLOSION))
                    {
                        pSoldier.ubMiscSoldierFlags |= SOLDIER_MISC.HEARD_GUNSHOT;
                    }

                    // Can the listener hear noise of that volume given his circumstances?
                    ubEffVolume = CalcEffVolume(pSoldier, sGridNo, bLevel, ubNoiseType, ubBaseVolume, bCheckTerrain, pSoldier.bOverTerrainType, ubSourceTerrType);

                    // # if RECORDOPPLIST
                    //                     fprintf(OpplistFile, "PN: guy %d - effVol=%d,chkTer=%d,pSoldier.tType=%d,srcTType=%d\n",
                    //                      bLoop, effVolume, bCheckTerrain, pSoldier.terrtype, ubSourceTerrType);
                    // #endif


                    if (ubEffVolume > 0)
                    {
                        // ALL RIGHT!  Passed all the tests, this listener hears this noise!!!
                        HearNoise(pSoldier, ubSource, sGridNo, bLevel, ubEffVolume, ubNoiseType, out bSeen);

                        bHeard = true;

                        ubNoiseDir = SoldierControl.atan8(CenterX(pSoldier.sGridNo), CenterY(pSoldier.sGridNo), CenterX(sGridNo), CenterY(sGridNo));

                        // check the 'noise heard & reported' bit for that soldier & direction
                        if (ubNoiseType != NOISE.MOVEMENT || bTeam != OUR_TEAM || (pSoldier.bInterruptDuelPts != Globals.NO_INTERRUPT) || !(pSoldier.ubMovementNoiseHeard & (1 << ubNoiseDir)))
                        {
                            if (ubEffVolume > ubLoudestEffVolume)
                            {
                                ubLoudestEffVolume = ubEffVolume;
                                ubHeardLoudestBy = pSoldier.ubID;
                                ubLoudestNoiseDir = ubNoiseDir;
                            }
                        }

                    }
                    else
                    {
                        //NameMessage(pSoldier," can't hear this noise",2500);
                        ubEffVolume = 0;
                    }
                }


                // if the noise was heard at all
                if (bHeard)
                {
                    // and we're doing our team
                    if (bTeam == OUR_TEAM)
                    /*
                    if (team == Net.pnum)
                    */
                    {
                        // if we are to tell the player about this type of noise
                        if (bTellPlayer && ubHeardLoudestBy != Globals.NOBODY)
                        {
                            // the merc that heard it the LOUDEST is the one to comment
                            // should add level to this function call
                            TellPlayerAboutNoise(Globals.MercPtrs[ubHeardLoudestBy], ubNoiseMaker, sGridNo, bLevel, ubLoudestEffVolume, ubNoiseType, ubLoudestNoiseDir);

                            if (ubNoiseType == NOISE.MOVEMENT)
                            {
                                Globals.MercPtrs[ubHeardLoudestBy].ubMovementNoiseHeard |= (1 << ubNoiseDir);
                            }

                        }
                        //if ( !(pSoldier.ubMovementNoiseHeard & (1 << ubNoiseDir) ) )
                    }
                    // # if REPORTTHEIRNOISE
                    //                     else   // debugging: report noise heard by other team's soldiers
                    //                     {
                    //                         if (bTellPlayer)
                    //                         {
                    //                             TellPlayerAboutNoise(Globals.MercPtrs[ubHeardLoudestBy], ubNoiseMaker, sGridNo, bLevel, ubLoudestEffVolume, ubNoiseType, ubLoudestNoiseDir);
                    //                         }
                    //                     }
                    // #endif
                }

                // if the listening team is human-controlled AND
                // the noise's source is another soldier
                // (computer-controlled teams don't radio or automatically report NOISE)
                if (gTacticalStatus.Team[bTeam].IsHuman && (ubSource < NOBODY))
                {
                    // if ubNoiseMaker was seen by at least one member of this team
                    if (bSeen)
                    {
                        // Temporary for opplist synching - disable random order radioing
                        //# if RECORDOPPLIST
                        //                        // insure all machines radio in synch to keep logs the same
                        //                        for (bLoop = Status.team[team].guystart, pSoldier = Menptr + bLoop; bLoop < Status.team[team].guyend; bLoop++, pSoldier++)
                        //                        {
                        //                            // if this merc is active, in this sector, and well enough to look
                        //                            if (pSoldier.active && pSoldier.in_sector && (pSoldier.life >= OKLIFE))
                        //                            {
                        //                                RadioSightings(pSoldier, ubSource);
                        //                                pSoldier.newOppCnt = 0;
                        //                            }
                        //                        }
                        //#else
                        // if this human team is OURS
                        if (true /* bTeam == Net.pnum */)
                        {
                            // this team is now allowed to report sightings and set Public flags
                            OurTeamRadiosRandomlyAbout(ubSource);
                        }
                        else    // noise was heard by another human-controlled team (not ours)
                        {
                            // mark noise maker as being seen currently
                            //UpdatePublic(bTeam,ubSource,SEEN_CURRENTLY,sGridNo,NOUPDATE,ACTUAL);
                            UpdatePublic(bTeam, ubSource, SEEN_CURRENTLY, sGridNo, bLevel);
                        }
                        //#endif
                    }
                    else // not seen
                    {
                        if (bHeard)
                        {
                            // # if RECORDOPPLIST
                            //                             fprintf(OpplistFile, "UpdatePublic (ProcessNoise/heard) for team %d about %d\n", team, ubSource);
                            // #endif

                            // mark noise maker as having been PUBLICLY heard THIS TURN
                            //UpdatePublic(team,ubSource,HEARD_THIS_TURN,sGridNo,NOUPDATE,ACTUAL);
                            UpdatePublic(bTeam, ubSource, HEARD_THIS_TURN, sGridNo, bLevel);
                        }
                    }
                }
            }

            gsWhoThrewRock = Globals.NOBODY;
        }



        int CalcEffVolume(SOLDIERTYPE? pSoldier, int sGridNo, int bLevel, NOISE ubNoiseType, int ubBaseVolume,
                    int bCheckTerrain, TerrainTypeDefines ubTerrType1, TerrainTypeDefines ubTerrType2)
        {
            int iEffVolume, iDistance;

            if (pSoldier.inv[InventorySlot.HEAD1POS].usItem == Items.WALKMAN
                || pSoldier.inv[InventorySlot.HEAD2POS].usItem == Items.WALKMAN)
            {
                return (0);
            }

            if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
            {
                // ATE: Funny things happen to ABC stuff if bNewSituation set....
                if (gTacticalStatus.ubCurrentTeam == pSoldier.bTeam)
                {
                    return (0);
                }
            }

            //sprintf(tempstr,"CalcEffVolume BY %s for gridno %d, baseVolume = %d",pSoldier.name,gridno,baseVolume);
            //PopMessage(tempstr);

            // adjust default noise volume by listener's hearing capability
            iEffVolume = ubBaseVolume + DecideHearing(pSoldier);


            // effective volume reduced by listener's number of opponents in sight
            iEffVolume -= pSoldier.bOppCnt;


            // calculate the distance (in adjusted pixels) between the source of the
            // noise (gridno) and the location of the would-be listener (pSoldier.gridno)
            iDistance = PythSpacesAway(pSoldier.sGridNo, sGridNo);
            /*
            distance = AdjPixelsAway(pSoldier.x,pSoldier.y,CenterX(sGridNo),CenterY(sGridNo));

               distance /= 15;      // divide by 15 to convert from adj. pixels to tiles
               */
            //NumMessage("Distance = ",distance);

            // effective volume fades over distance beyond 1 tile away
            iEffVolume -= (iDistance - 1);

            /*
            if (pSoldier.bTeam == CIV_TEAM && pSoldier.ubBodyType != CROW )
            {
                if (pSoldier.ubCivilianGroup == 0 && pSoldier.ubProfile == NO_PROFILE)
                {
                    // nameless civs reduce effective volume by 2 for gunshots etc
                    // (double the reduction due to distance)
                    // so that they don't cower from attacks that are really far away
                    switch (ubNoiseType)
                    {
                        case NOISE.GUNFIRE:
                        case NOISE.BULLET_IMPACT:
                        case NOISE.GRENADE_IMPACT:
                        case NOISE.EXPLOSION:
                            iEffVolume -= iDistance;
                            break;
                        default:
                            break;
                    }
                }
                else if (pSoldier.IsNeutral)
                {
                    // NPCs and people in groups ignore attack noises unless they are no longer neutral
                    switch (ubNoiseType)
                    {
                        case NOISE.GUNFIRE:
                        case NOISE.BULLET_IMPACT:
                        case NOISE.GRENADE_IMPACT:
                        case NOISE.EXPLOSION:
                            iEffVolume = 0;
                            break;
                        default:
                            break;
                    }
                }
            }
            */

            if (pSoldier.usAnimState == AnimationStates.RUNNING)
            {
                iEffVolume -= 5;
            }

            // chad: they cheated here. No Assignment for SLEEPING, 
            if (pSoldier.bAssignment == Assignments.SLEEPING)
            {
                // decrease effective volume since we're asleep!
                iEffVolume -= 5;
            }

            // check for floor/roof difference
            if (bLevel > pSoldier.bLevel)
            {
                // sound is amplified by roof
                iEffVolume += 5;
            }
            else if (bLevel < pSoldier.bLevel)
            {
                // sound is muffled
                iEffVolume -= 5;
            }

            // if we still have a chance of hearing this, and the terrain types are known
            if (iEffVolume > 0)
            {
                if (bCheckTerrain > 0)
                {
                    // if, between noise and listener, one is outside and one is inside

                    // NOTE: This is a pretty dumb way of doing things, since it won't detect
                    // the presence of walls between 2 spots both inside or both outside, but
                    // given our current system it's the best that we can do

                    if (((ubTerrType1 == TerrainTypeDefines.FLAT_FLOOR) && (ubTerrType2 != TerrainTypeDefines.FLAT_FLOOR)) ||
                        ((ubTerrType1 != TerrainTypeDefines.FLAT_FLOOR) && (ubTerrType2 == TerrainTypeDefines.FLAT_FLOOR)))
                    {
                        //PopMessage("Sound is muffled by wall(s)");

                        // sound is muffled, reduce the effective volume of the noise
                        iEffVolume -= 5;
                    }
                }

            }

            //NumMessage("effVolume = ",ubEffVolume);
            if (iEffVolume > 0)
            {
                return ((int)iEffVolume);
            }
            else
            {
                return (0);
            }
        }




        void HearNoise(SOLDIERTYPE? pSoldier, int ubNoiseMaker, int sGridNo, int bLevel, int ubVolume, NOISE ubNoiseType, out int? ubSeen)
        {
            ubSeen = null;

            int sNoiseX, sNoiseY;
            int bHadToTurn = 0, bSourceSeen = 0;
            int bOldOpplist;
            int sDistVisible;
            WorldDirections bDirection;
            bool fMuzzleFlash = false;

            //	DebugMsg( TOPIC_JA2, DBG_LEVEL_3, String( "%d hears noise from %d (%d/%d) volume %d", pSoldier.ubID, ubNoiseMaker, sGridNo, bLevel, ubVolume ) );


            if (pSoldier.ubBodyType == SoldierBodyTypes.CROW)
            {
                CrowsFlyAway(pSoldier.bTeam);
                return;
            }

            // "Turn head" towards the source of the noise and try to see what's there

            // don't use DistanceVisible here, but use maximum visibility distance
            // in as straight line instead.  Represents guy "turning just his head"

            // CJC 97/10: CHANGE!  Since STRAIGHT can not reliably be used as a 
            // max sighting distance (varies based on realtime/turnbased), call
            // the function with the new DIRECTION_IRRELEVANT define

            // is he close enough to see that gridno if he turns his head?

            // ignore muzzle flashes when turning head to see noise
            if (ubNoiseType == NOISE.GUNFIRE && ubNoiseMaker != Globals.NOBODY && Globals.MercPtrs[ubNoiseMaker].fMuzzleFlash)
            {
                sNoiseX = CenterX(sGridNo);
                sNoiseY = CenterY(sGridNo);
                bDirection = SoldierControl.atan8(pSoldier.sX, pSoldier.sY, sNoiseX, sNoiseY);
                if (pSoldier.bDirection != bDirection && pSoldier.bDirection != gOneCDirection[bDirection] && pSoldier.bDirection != gOneCCDirection[bDirection])
                {
                    // temporarily turn off muzzle flash so DistanceVisible can be calculated without it
                    Globals.MercPtrs[ubNoiseMaker].fMuzzleFlash = false;
                    fMuzzleFlash = true;
                }
            }

            sDistVisible = DistanceVisible(pSoldier, WorldDirections.DIRECTION_IRRELEVANT, WorldDirections.DIRECTION_IRRELEVANT, sGridNo, bLevel);

            if (fMuzzleFlash)
            {
                // turn flash on again
                Globals.MercPtrs[ubNoiseMaker].fMuzzleFlash = true;
            }

            if (PythSpacesAway(pSoldier.sGridNo, sGridNo) <= sDistVisible)
            {
                // just use the XXadjustedXX center of the gridno
                sNoiseX = CenterX(sGridNo);
                sNoiseY = CenterY(sGridNo);

                if (pSoldier.bDirection != SoldierControl.atan8(pSoldier.sX, pSoldier.sY, sNoiseX, sNoiseY))
                {
                    bHadToTurn = 1;
                }
                else
                {
                    bHadToTurn = 0;
                }

                // and we can trace a line of sight to his x,y coordinates?
                // (taking into account we are definitely aware of this guy now)

                // skip LOS check if we had to turn and we're a tank.  sorry Mr Tank, no looking out of the sides for you!
                if (!(bHadToTurn > 0 && TANK(pSoldier)))
                {
                    if (SoldierTo3DLocationLineOfSightTest(pSoldier, sGridNo, bLevel, 0, (int)sDistVisible, true))
                    {
                        // he can actually see the spot where the noise came from!
                        bSourceSeen = 1;

                        // if this sounds like a door opening/closing (could also be a crate)
                        if (ubNoiseType == NOISE.CREAKING)
                        {
                            // then look around and update ALL doors that have secretly changed
                            //LookForDoors(pSoldier,AWARE);
                        }
                    }
                }

                // # if RECORDOPPLIST
                //                 fprintf(OpplistFile, "HN: %s by %2d(g%4d,x%3d,y%3d) at %2d(g%4d,x%3d,y%3d), hTT=%d\n",
                //                     (bSourceSeen) ? "SCS" : "FLR",
                //                     pSoldier.guynum, pSoldier.sGridNo, pSoldier.sX, pSoldier.sY,
                //                     ubNoiseMaker, sGridNo, sNoiseX, sNoiseY,
                //                     bHadToTurn);
                // #endif
            }

            // if noise is made by a person
            if (ubNoiseMaker < Globals.NOBODY)
            {
                bOldOpplist = pSoldier.bOppList[ubNoiseMaker];

                // WE ALREADY KNOW THAT HE'S ON ANOTHER TEAM, AND HE'S NOT BEING SEEN
                // ProcessNoise() ALREADY DID THAT WORK FOR US

                if (bSourceSeen > 0)
                {
                    ManSeesMan(pSoldier, Globals.MercPtrs[ubNoiseMaker], Menptr[ubNoiseMaker].sGridNo, Menptr[ubNoiseMaker].bLevel, HEARNOISE, CALLER_UNKNOWN);

                    // if it's an AI soldier, he is not allowed to automatically radio any
                    // noise heard, but manSeesMan has set his newOppCnt, so clear it here
                    if (!(pSoldier.uiStatusFlags.HasFlag(SOLDIER.PC)))
                    {
                        pSoldier.bNewOppCnt = 0;
                    }

                    ubSeen = 1;
                    // RadioSightings() must only be called later on by ProcessNoise() itself
                    // because we want the soldier who heard noise the LOUDEST to report it

                    if (pSoldier.bNeutral > 0)
                    {
                        // could be a civilian watching us shoot at an enemy
                        if (((ubNoiseType == NOISE.GUNFIRE) || (ubNoiseType == NOISE.BULLET_IMPACT)) && (ubVolume >= 3))
                        {
                            // if status is only GREEN or YELLOW
                            if (pSoldier.bAlertStatus < STATUS.RED)
                            {
                                // then this soldier goes to status RED, has proof of enemy presence
                                pSoldier.bAlertStatus = STATUS.RED;
                                CheckForChangingOrders(pSoldier);
                            }
                        }
                    }

                }
                else         // noise maker still can't be seen
                {
                    SetNewSituation(pSoldier); // re-evaluate situation

                    // if noise type was unmistakably that of gunfire
                    if (((ubNoiseType == NOISE.GUNFIRE) || (ubNoiseType == NOISE.BULLET_IMPACT)) && (ubVolume >= 3))
                    {
                        // if status is only GREEN or YELLOW
                        if (pSoldier.bAlertStatus < STATUS.RED)
                        {
                            // then this soldier goes to status RED, has proof of enemy presence
                            pSoldier.bAlertStatus = STATUS.RED;
                            CheckForChangingOrders(pSoldier);
                        }
                    }

                    // remember that the soldier has been heard and his new location
                    UpdatePersonal(pSoldier, ubNoiseMaker, HEARD_THIS_TURN, sGridNo, bLevel);

                    // Public info is not set unless EVERYONE on the team fails to see the
                    // ubnoisemaker, leaving the 'seen' flag false.  See ProcessNoise().

                    // CJC: set the noise gridno for the soldier, if appropriate - this is what is looked at by the AI!
                    if (ubVolume >= pSoldier.ubNoiseVolume)
                    {
                        // yes it is, so remember this noise INSTEAD (old noise is forgotten)
                        pSoldier.sNoiseGridno = sGridNo;
                        pSoldier.bNoiseLevel = bLevel;

                        // no matter how loud noise was, don't remember it for than 12 turns!
                        if (ubVolume < MAX_MISC_NOISE.DURATION)
                        {
                            pSoldier.ubNoiseVolume = ubVolume;
                        }
                        else
                        {
                            pSoldier.ubNoiseVolume = MAX_MISC_NOISE.DURATION;
                        }

                        SetNewSituation(pSoldier);  // force a fresh AI decision to be made
                    }

                }

                if (pSoldier.fAIFlags.HasFlag(AIDEFINES.AI_ASLEEP))
                {
                    switch (ubNoiseType)
                    {
                        case NOISE.BULLET_IMPACT:
                        case NOISE.GUNFIRE:
                        case NOISE.EXPLOSION:
                        case NOISE.SCREAM:
                        case NOISE.WINDOW_SMASHING:
                        case NOISE.DOOR_SMASHING:
                            // WAKE UP!
                            pSoldier.fAIFlags &= (~AIDEFINES.AI_ASLEEP);
                            break;
                        default:
                            break;
                    }
                }

                // FIRST REQUIRE MUTUAL HOSTILES!
                if (!CONSIDERED_NEUTRAL(Globals.MercPtrs[ubNoiseMaker], pSoldier) && !CONSIDERED_NEUTRAL(pSoldier, Globals.MercPtrs[ubNoiseMaker]) && (pSoldier.bSide != Globals.MercPtrs[ubNoiseMaker].bSide))
                {
                    // regardless of whether the noisemaker (who's not Globals.NOBODY) was seen or not,
                    // as long as listener meets minimum interrupt conditions
                    if (gfDelayResolvingBestSightingDueToDoor)
                    {
                        if (bSourceSeen && (!((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED)) && (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))) || (gubSightFlags.HasFlag(SIGHT.INTERRUPT) && StandardInterruptConditionsMet(pSoldier, ubNoiseMaker, bOldOpplist))))
                        {
                            // we should be adding this to the array for the AllTeamLookForAll to handle				
                            // since this is a door opening noise, add a bonus equal to half the door volume
                            int ubPoints;

                            ubPoints = CalcInterruptDuelPts(pSoldier, ubNoiseMaker, true);
                            if (ubPoints != Globals.NO_INTERRUPT)
                            {
                                // require the enemy not to be dying if we are the sighter; in other words,
                                // always add for AI guys, and always add for people with life >= OKLIFE
                                if (pSoldier.bTeam != gbPlayerNum || Globals.MercPtrs[ubNoiseMaker].bLife >= OKLIFE)
                                {
                                    ReevaluateBestSightingPosition(pSoldier, (int)(ubPoints + (ubVolume / 2)));
                                }
                            }
                        }
                    }
                    else
                    {
                        if ((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED)) && (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
                        {
                            if (StandardInterruptConditionsMet(pSoldier, ubNoiseMaker, bOldOpplist))
                            {
                                // he gets a chance to interrupt the noisemaker
                                pSoldier.bInterruptDuelPts = CalcInterruptDuelPts(pSoldier, ubNoiseMaker, true);
                                //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("Calculating int duel pts in noise code, %d has %d points", pSoldier.ubID, pSoldier.bInterruptDuelPts));
                            }
                            else
                            {
                                pSoldier.bInterruptDuelPts = Globals.NO_INTERRUPT;
                            }
                        }
                        else if (bSourceSeen > 0)
                        {
                            // seen source, in realtime, so check for sighting stuff
                            HandleBestSightingPositionInRealtime();
                        }
                    }

                }
            }
            else   // noise made by Globals.NOBODY
            {
                // if noise type was unmistakably that of an explosion (seen or not) or alarm
                if (!(pSoldier.uiStatusFlags.HasFlag(SOLDIER.PC)))
                {
                    if ((ubNoiseType == NOISE.EXPLOSION || ubNoiseType == NOISE.SILENT_ALARM) && (ubVolume >= 3))
                    {
                        if (ubNoiseType == NOISE.SILENT_ALARM)
                        {
                            WearGasMaskIfAvailable(pSoldier);
                        }
                        // if status is only GREEN or YELLOW
                        if (pSoldier.bAlertStatus < STATUS.RED)
                        {
                            // then this soldier goes to status RED, has proof of enemy presence
                            pSoldier.bAlertStatus = STATUS.RED;
                            CheckForChangingOrders(pSoldier);
                        }
                    }
                }
                // if the source of the noise can't be seen,
                // OR if it's a rock and the listener had to turn so that by the time he
                // looked all his saw was a bunch of rocks lying still
                if (bSourceSeen == 0 || ((ubNoiseType == NOISE.ROCK_IMPACT) && (bHadToTurn > 0)) || ubNoiseType == NOISE.SILENT_ALARM)
                {
                    // check if the effective volume of this new noise is greater than or at
                    // least equal to the volume of the currently noticed noise stored
                    if (ubVolume >= pSoldier.ubNoiseVolume)
                    {
                        // yes it is, so remember this noise INSTEAD (old noise is forgotten)
                        pSoldier.sNoiseGridno = sGridNo;
                        pSoldier.bNoiseLevel = bLevel;

                        // no matter how loud noise was, don't remember it for than 12 turns!
                        if (ubVolume < MAX_MISC_NOISE.DURATION)
                        {
                            pSoldier.ubNoiseVolume = ubVolume;
                        }
                        else
                        {
                            pSoldier.ubNoiseVolume = MAX_MISC_NOISE.DURATION;
                        }

                        SetNewSituation(pSoldier);  // force a fresh AI decision to be made
                    }
                }
                else
                // if listener sees the source of the noise, AND it's either a grenade,
                //  or it's a rock that he watched land (didn't need to turn)
                {
                    SetNewSituation(pSoldier);  // re-evaluate situation

                    // if status is only GREEN or YELLOW
                    if (pSoldier.bAlertStatus < STATUS.RED)
                    {
                        // then this soldier goes to status RED, has proof of enemy presence
                        pSoldier.bAlertStatus = STATUS.RED;
                        CheckForChangingOrders(pSoldier);
                    }
                }

                if (gubBestToMakeSightingSize == BEST_SIGHTING_ARRAY_SIZE_INCOMBAT)
                {
                    // if the noise heard was the fall of a rock
                    if ((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED)) && (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)) && ubNoiseType == NOISE.ROCK_IMPACT)
                    {
                        // give every ELIGIBLE listener an automatic interrupt, since it's
                        // reasonable to assume the guy throwing wants to wait for their reaction!
                        if (StandardInterruptConditionsMet(pSoldier, Globals.NOBODY, false))
                        {
                            pSoldier.bInterruptDuelPts = AUTOMATIC_INTERRUPT;          // force automatic interrupt
                                                                                       //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("Calculating int duel pts in noise code, %d has %d points", pSoldier.ubID, pSoldier.bInterruptDuelPts));
                        }
                        else
                        {
                            pSoldier.bInterruptDuelPts = Globals.NO_INTERRUPT;
                        }
                    }
                }
            }
        }

        void TellPlayerAboutNoise(SOLDIERTYPE? pSoldier, int ubNoiseMaker, int sGridNo, int bLevel, int ubVolume, NOISE ubNoiseType, int ubNoiseDir)
        {
            int ubVolumeIndex;

            // CJC: tweaked the noise categories upwards a bit because our movement noises can be louder now.
            if (ubVolume < 4)
            {
                ubVolumeIndex = 0;      // 1-3: faint noise
            }
            else if (ubVolume < 8)  // 4-7: definite noise
            {
                ubVolumeIndex = 1;
            }
            else if (ubVolume < 12) // 8-11: loud noise
            {
                ubVolumeIndex = 2;
            }
            else                                        // 12+: very loud noise
            {
                ubVolumeIndex = 3;
            }

            // display a message about a noise...
            // e.g. Sidney hears a loud splash from/to? the north.

            if (ubNoiseMaker != Globals.NOBODY && pSoldier.bTeam == gbPlayerNum && pSoldier.bTeam == Menptr[ubNoiseMaker].bTeam)
            {
                // # if JA2BETAVERSION
                //                 ScreenMsg(MSG_FONT_RED, MSG_ERROR, "ERROR! TAKE SCREEN CAPTURE AND TELL CAMFIELD NOW!");
                //                 ScreenMsg(MSG_FONT_RED, MSG_ERROR, "%s (%d) heard noise from %s (%d), noise at %dL%d, type %d", pSoldier.name, pSoldier.ubID, Menptr[ubNoiseMaker].name, ubNoiseMaker, sGridNo, bLevel, ubNoiseType);
                // #endif
            }

            if (bLevel == pSoldier.bLevel || ubNoiseType == NOISE.EXPLOSION || ubNoiseType == NOISE.SCREAM || ubNoiseType == NOISE.ROCK_IMPACT || ubNoiseType == NOISE.GRENADE_IMPACT)
            {
                Messages.ScreenMsg(MSG_FONT_YELLOW, MSG_INTERFACE, pNewNoiseStr[ubNoiseType], pSoldier.name, pNoiseVolStr[ubVolumeIndex], pDirectionStr[ubNoiseDir]);
            }
            else if (bLevel > pSoldier.bLevel)
            {
                // from above!
                Messages.ScreenMsg(MSG_FONT_YELLOW, MSG_INTERFACE, pNewNoiseStr[ubNoiseType], pSoldier.name, pNoiseVolStr[ubVolumeIndex], gzLateLocalizedString[6]);
            }
            else
            {
                // from below!
                Messages.ScreenMsg(MSG_FONT_YELLOW, MSG_INTERFACE, pNewNoiseStr[ubNoiseType], pSoldier.name, pNoiseVolStr[ubVolumeIndex], gzLateLocalizedString[7]);
            }

            // if the quote was faint, say something
            if (ubVolumeIndex == 0)
            {
                if (!Meanwhile.AreInMeanwhile() && !(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.ENGAGED_IN_CONV)) && pSoldier.ubTurnsUntilCanSayHeardNoise == 0)
                {
                    TacticalCharacterDialogue(pSoldier, QUOTE.HEARD_SOMETHING);
                    if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
                    {
                        pSoldier.ubTurnsUntilCanSayHeardNoise = 2;
                    }
                    else
                    {
                        pSoldier.ubTurnsUntilCanSayHeardNoise = 5;
                    }
                }
            }

            // flag soldier as having reported noise in a particular direction

        }

        void VerifyAndDecayOpplist(SOLDIERTYPE? pSoldier)
        {
            int uiLoop;
            int pPersOL;           // pointer into soldier's opponent list
            SOLDIERTYPE? pOpponent;

            // reduce all seen/known opponent's turn counters by 1 (towards 0)
            // 1) verify accuracy of the opplist by testing sight vs known opponents
            // 2) increment opplist value if opponent is known but not currenly seen
            // 3) forget about known opponents who haven't been noticed in some time

            // if soldier is unconscious, make sure his opplist is wiped out & bail out
            if (pSoldier.bLife < OKLIFE)
            {
                //memset(pSoldier.bOppList, NOT_HEARD_OR_SEEN, sizeof(pSoldier.bOppList));
                pSoldier.bOppCnt = 0;
                return;
            }

            // if any new opponents were seen earlier and not yet radioed
            if (pSoldier.bNewOppCnt > 0)
            {
                // # if BETAVERSION
                //                 sprintf(tempstr, "VerifyAndDecayOpplist: WARNING - %d(%s) still has %d NEW OPPONENTS - lastCaller %s/%s",
                //                     pSoldier.guynum, ExtMen[pSoldier.guynum].name, pSoldier.newOppCnt,
                //                     LastCallerText[ExtMen[pSoldier.guynum].lastCaller],
                //                     LastCaller2Text[ExtMen[pSoldier.guynum].lastCaller2]);
                // 
                // # if TESTVERSION	// make this ERROR/BETA again when it's fixed!
                //                 PopMessage(tempstr);
                // #endif
                // 
                // # if RECORDNET
                //                 fprintf(NetDebugFile, "\n\t%s\n\n", tempstr);
                // #endif
                // 
                // #endif

                if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.PC))
                {
                    RadioSightings(pSoldier, EVERYBODY, pSoldier.bTeam);
                }

                pSoldier.bNewOppCnt = 0;
            }

            // man looks for each of his opponents WHO ARE ALREADY KNOWN TO HIM
            for (uiLoop = 0; uiLoop < guiNumMercSlots; uiLoop++)
            {
                pOpponent = MercSlots[uiLoop];

                // if this merc is active, here, and alive
                if (pOpponent != null && pOpponent.IsAlive)
                {
                    // if this merc is on the same team, he's no opponent, so skip him
                    if (pSoldier.bTeam == pOpponent.bTeam)
                    {
                        continue;
                    }

                    pPersOL = pSoldier.bOppList[pOpponent.ubID];

                    // if this opponent is "known" in any way (seen or heard recently)
                    if (pPersOL != NOT_HEARD_OR_SEEN)
                    {
                        // use both sides actual x,y co-ordinates (neither side's moving)
                        ManLooksForMan(pSoldier, pOpponent, VERIFYANDDECAYOPPLIST);

                        // decay opplist value if necessary
                        DECAY_OPPLIST_VALUE(pPersOL);
                        /*
                  // if opponent was SEEN recently but is NOT visible right now
                  if (*pPersOL >= SEEN_THIS_TURN)
                   {
                    (*pPersOL)++;          // increment #turns it's been since last seen

                    // if it's now been longer than the maximum we care to remember
                    if (*pPersOL > SEEN_2_TURNS_AGO)
                      *pPersOL = 0;        // forget that we knew this guy
                   }
                  else
                   {
                    // if opponent was merely HEARD recently, not actually seen
                    if (*pPersOL <= HEARD_THIS_TURN)
                     {
                      (*pPersOL)--;        // increment #turns it's been since last heard

                  // if it's now been longer than the maximum we care to remember
                  if (*pPersOL < HEARD_2_TURNS_AGO)
                    *pPersOL = 0;      // forget that we knew this guy
                         }
                           }
                       */
                    }

                }
            }


            // if any new opponents were seen
            if (pSoldier.bNewOppCnt > 0)
            {
                // turns out this is NOT an error!  If this guy was gassed last time he
                // looked, his sight limit was 2 tiles, and now he may no longer be gassed
                // and thus he sees opponents much further away for the first time!
                // - Always happens if you STUNGRENADE an opponent by surprise...
                // # if RECORDNET
                //                 fprintf(NetDebugFile, "\tVerifyAndDecayOpplist: d(%s) saw %d new opponents\n",
                //                         pSoldier.guynum, ExtMen[pSoldier.guynum].name, pSoldier.newOppCnt);
                // #endif

                if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.PC))
                {
                    RadioSightings(pSoldier, EVERYBODY, pSoldier.bTeam);
                }

                pSoldier.bNewOppCnt = 0;
            }
        }

        void DecayIndividualOpplist(SOLDIERTYPE? pSoldier)
        {
            int uiLoop;
            int? pPersOL;           // pointer into soldier's opponent list
            SOLDIERTYPE? pOpponent;

            // reduce all currently seen opponent's turn counters by 1 (towards 0)

            // if soldier is unconscious, make sure his opplist is wiped out & bail out
            if (pSoldier.bLife < OKLIFE)
            {
                // must make sure that public opplist is kept to match...
                for (uiLoop = 0; uiLoop < TOTAL_SOLDIERS; uiLoop++)
                {
                    if (pSoldier.bOppList[uiLoop] == SEEN_CURRENTLY)
                    {
                        HandleManNoLongerSeen(pSoldier, Globals.MercPtrs[uiLoop], (pSoldier.bOppList[uiLoop]), (gbPublicOpplist[pSoldier.bTeam][uiLoop]));
                    }
                }
                //void HandleManNoLongerSeen( SOLDIERTYPE * pSoldier, SOLDIERTYPE * pOpponent, int * pPersOL, int * pbPublOL )

                //memset(pSoldier.bOppList, NOT_HEARD_OR_SEEN, sizeof(pSoldier.bOppList));
                pSoldier.bOppCnt = 0;
                return;
            }

            // man looks for each of his opponents WHO IS CURRENTLY SEEN
            for (uiLoop = 0; uiLoop < guiNumMercSlots; uiLoop++)
            {
                pOpponent = MercSlots[uiLoop];

                // if this merc is active, here, and alive
                if (pOpponent != null && pOpponent.IsAlive)
                {
                    // if this merc is on the same team, he's no opponent, so skip him
                    if (pSoldier.bTeam == pOpponent.bTeam)
                    {
                        continue;
                    }

                    pPersOL = pSoldier.bOppList[pOpponent.ubID];

                    // if this opponent is seen currently
                    if (pPersOL == SEEN_CURRENTLY)
                    {
                        // they are NOT visible now!
                        (pPersOL)++;
                        if (!CONSIDERED_NEUTRAL(pOpponent, pSoldier) && !CONSIDERED_NEUTRAL(pSoldier, pOpponent) && (pSoldier.bSide != pOpponent.bSide))
                        {
                            RemoveOneOpponent(pSoldier);
                        }

                    }
                }
            }
        }



        void VerifyPublicOpplistDueToDeath(SOLDIERTYPE? pSoldier)
        {
            int uiLoop, uiTeamMateLoop;
            int pPersOL, pMatePersOL;    // pointers into soldier's opponent list
            SOLDIERTYPE? pOpponent, pTeamMate;
            bool bOpponentStillSeen;


            // OK, someone died. Anyone that the deceased ALONE saw has to decay
            // immediately in the Public Opplist.


            // If deceased didn't see ANYONE, don't bother
            if (pSoldier.bOppCnt == 0)
            {
                return;
            }


            // Deceased looks for each of his opponents who is "seen currently"
            for (uiLoop = 0; uiLoop < guiNumMercSlots; uiLoop++)
            {
                // first, initialize flag since this will be a "new" opponent
                bOpponentStillSeen = false;

                // grab a pointer to the "opponent"
                pOpponent = MercSlots[uiLoop];

                // if this opponent is active, here, and alive
                if (pOpponent != null && pOpponent.IsAlive)
                {
                    // if this opponent is on the same team, he's no opponent, so skip him
                    if (pSoldier.bTeam == pOpponent.bTeam)
                    {
                        continue;
                    }

                    // point to what the deceased's personal opplist value is
                    pPersOL = pSoldier.bOppList[pOpponent.ubID];

                    // if this opponent was CURRENTLY SEEN by the deceased (before his
                    // untimely demise)
                    if (pPersOL == SEEN_CURRENTLY)
                    {
                        // then we need to know if any teammates ALSO see this opponent, so loop through
                        // trying to find ONE witness to the death...
                        for (uiTeamMateLoop = 0; uiTeamMateLoop < guiNumMercSlots; uiTeamMateLoop++)
                        {
                            // grab a pointer to the potential teammate
                            pTeamMate = MercSlots[uiTeamMateLoop];

                            // if this teammate is active, here, and alive
                            if (pTeamMate != null && pTeamMate.IsAlive)
                            {
                                // if this opponent is NOT on the same team, then skip him
                                if (pTeamMate.bTeam != pSoldier.bTeam)
                                {
                                    continue;
                                }

                                // point to what the teammate's personal opplist value is
                                pMatePersOL = pTeamMate.bOppList[pOpponent.ubID];

                                // test to see if this value is "seen currently"
                                if (pMatePersOL == SEEN_CURRENTLY)
                                {
                                    // this opponent HAS been verified! 
                                    bOpponentStillSeen = true;

                                    // we can stop looking for other witnesses now
                                    break;
                                }
                            }
                        }
                    }

                    // if no witnesses for this opponent, then decay the Public Opplist
                    if (!bOpponentStillSeen)
                    {
                        DECAY_OPPLIST_VALUE(gbPublicOpplist[pSoldier.bTeam][pOpponent.ubID]);
                    }
                }
            }
        }


        void DecayPublicOpplist(TEAM bTeam)
        {
            int uiLoop;
            int bNoPubliclyKnownOpponents = 1;
            SOLDIERTYPE? pSoldier;
            int? pbPublOL;


            //NumMessage("Decay for team #",team);

            // decay the team's public noise volume, forget public noise gridno if <= 0
            // used to be -1 per turn but that's not fast enough!
            if (gubPublicNoiseVolume[bTeam] > 0)
            {
                if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
                {
                    gubPublicNoiseVolume[bTeam] = (int)((int)(gubPublicNoiseVolume[bTeam] * 7) / 10);
                }
                else
                {
                    gubPublicNoiseVolume[bTeam] = gubPublicNoiseVolume[bTeam] / 2;
                }

                if (gubPublicNoiseVolume[bTeam] <= 0)
                {
                    gsPublicNoiseGridno[bTeam] = NOWHERE;
                }
            }

            // decay the team's Public Opplist
            for (uiLoop = 0; uiLoop < guiNumMercSlots; uiLoop++)
            {
                pSoldier = MercSlots[uiLoop];

                // for every active, living soldier on ANOTHER team
                if (pSoldier is not null && pSoldier.IsAlive && (pSoldier.bTeam != bTeam))
                {
                    // hang a pointer to the byte holding team's public opplist for this merc
                    pbPublOL = gbPublicOpplist[bTeam][pSoldier.ubID];

                    if (pbPublOL == NOT_HEARD_OR_SEEN)
                    {
                        continue;
                    }

                    // well, that make this a "publicly known opponent", so nuke that flag
                    bNoPubliclyKnownOpponents = 0;

                    // if this person has been SEEN recently, but is not currently visible
                    if (pbPublOL >= SEEN_THIS_TURN)
                    {
                        (pbPublOL)++;      // increment how long it's been
                    }
                    else
                    {
                        // if this person has been only HEARD recently
                        if (pbPublOL <= HEARD_THIS_TURN)
                        {
                            (pbPublOL)--;    // increment how long it's been
                        }
                    }

                    // if it's been longer than the maximum we care to remember
                    if ((pbPublOL > OLDEST_SEEN_VALUE) || (pbPublOL < OLDEST_HEARD_VALUE))
                    {
                        //fprintf(OpplistFile, "UpdatePublic (DecayPublicOpplist) for team %d about %d\n", team, pSoldier.guynum);

                        // forget about him,
                        // and also forget where he was last seen (it's been too long)
                        // this is mainly so POINT_PATROL guys don't SEEK_OPPONENTs forever
                        UpdatePublic(bTeam, pSoldier.ubID, NOT_HEARD_OR_SEEN, NOWHERE, 0);
                    }
                }
            }

            // if all opponents are publicly unknown (NOT_HEARD_OR_SEEN)
            if (bNoPubliclyKnownOpponents)
            {
                // forget about the last radio alert (ie. throw away who made the call)
                // this is mainly so POINT_PATROL guys don't SEEK_FRIEND forever after
                gTacticalStatus.Team[bTeam].ubLastMercToRadio = Globals.NOBODY;
            }

            // decay watched locs as well
            DecayWatchedLocs(bTeam);
        }

        // bit of a misnomer; this is now decay all opplists
        void NonCombatDecayPublicOpplist(int uiTime)
        {
            int cnt;

            if (uiTime - gTacticalStatus.uiTimeSinceLastOpplistDecay >= TIME_BETWEEN_RT_OPPLIST_DECAYS)
            {
                // decay!
                for (cnt = 0; cnt < guiNumMercSlots; cnt++)
                {
                    if (MercSlots[cnt] is not null)
                    {
                        VerifyAndDecayOpplist(MercSlots[cnt]);
                    }
                }


                for (TEAM t = 0; t < (TEAM)MAXTEAMS; t++)
                {
                    if (gTacticalStatus.Team[t].bMenInSector > 0)
                    {
                        // decay team's public opplist
                        DecayPublicOpplist(t);
                    }
                }
                // update time
                gTacticalStatus.uiTimeSinceLastOpplistDecay = uiTime;
            }
        }

        void RecalculateOppCntsDueToNoLongerNeutral(SOLDIERTYPE? pSoldier)
        {
            int uiLoop;
            SOLDIERTYPE? pOpponent;

            pSoldier.bOppCnt = 0;

            if (!pSoldier.IsNeutral)
            {
                for (uiLoop = 0; uiLoop < guiNumMercSlots; uiLoop++)
                {
                    pOpponent = MercSlots[uiLoop];

                    // for every active, living soldier on ANOTHER team
                    if (pOpponent && pOpponent.IsAlive && !pOpponent.bNeutral && (pOpponent.bTeam != pSoldier.bTeam) && (!CONSIDERED_NEUTRAL(pOpponent, pSoldier) && !CONSIDERED_NEUTRAL(pSoldier, pOpponent) && (pSoldier.bSide != pOpponent.bSide)))
                    {
                        if (pSoldier.bOppList[pOpponent.ubID] == SEEN_CURRENTLY)
                        {
                            AddOneOpponent(pSoldier);
                        }
                        if (pOpponent.bOppList[pSoldier.ubID] == SEEN_CURRENTLY)
                        {
                            // have to add to opponent's oppcount as well since we just became non-neutral
                            AddOneOpponent(pOpponent);
                        }
                    }
                }
            }
        }

        void RecalculateOppCntsDueToBecomingNeutral(SOLDIERTYPE? pSoldier)
        {
            int uiLoop;
            SOLDIERTYPE? pOpponent;

            if (pSoldier.IsNeutral)
            {
                pSoldier.bOppCnt = 0;

                for (uiLoop = 0; uiLoop < guiNumMercSlots; uiLoop++)
                {
                    pOpponent = MercSlots[uiLoop];

                    // for every active, living soldier on ANOTHER team
                    if (pOpponent && pOpponent.IsAlive && !pOpponent.bNeutral && (pOpponent.bTeam != pSoldier.bTeam) && !CONSIDERED_NEUTRAL(pSoldier, pOpponent) && (pSoldier.bSide != pOpponent.bSide))
                    {
                        if (pOpponent.bOppList[pSoldier.ubID] == SEEN_CURRENTLY)
                        {
                            // have to rem from opponent's oppcount as well since we just became neutral
                            RemoveOneOpponent(pOpponent);
                        }
                    }
                }
            }
        }

        void NoticeUnseenAttacker(SOLDIERTYPE? pAttacker, SOLDIERTYPE? pDefender, int bReason)
        {
            int bOldOppList;
            int ubTileSightLimit;
            bool fSeesAttacker = false;
            WorldDirections bDirection;
            bool fMuzzleFlash = false;

            if (!(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
            {
                return;
            }

            if (pAttacker.usAttackingWeapon == Items.DART_GUN)
            {
                // rarely noticed
                if (SkillChecks.SkillCheck(pDefender, SKILLCHECKS.NOTICE_DART_CHECK, 0) < 0)
                {
                    return;
                }
            }

            // do we need to do checks for life/breath here?

            if (pDefender.ubBodyType == SoldierBodyTypes.LARVAE_MONSTER || (pDefender.uiStatusFlags.HasFlag(SOLDIER.VEHICLE) && pDefender.bTeam == OUR_TEAM))
            {
                return;
            }

            bOldOppList = pDefender.bOppList[pAttacker.ubID];
            if (PythSpacesAway(pAttacker.sGridNo, pDefender.sGridNo) <= MaxDistanceVisible())
            {
                // check LOS, considering we are now aware of the attacker
                // ignore muzzle flashes when must turning head 
                if (pAttacker.fMuzzleFlash)
                {
                    bDirection = SoldierControl.atan8(pDefender.sX, pDefender.sY, pAttacker.sX, pAttacker.sY);
                    if (pDefender.bDirection != bDirection && pDefender.bDirection != gOneCDirection[bDirection] && pDefender.bDirection != gOneCCDirection[bDirection])
                    {
                        // temporarily turn off muzzle flash so DistanceVisible can be calculated without it
                        pAttacker.fMuzzleFlash = false;
                        fMuzzleFlash = true;
                    }
                }

                ubTileSightLimit = DistanceVisible(pDefender, WorldDirections.DIRECTION_IRRELEVANT, 0, pAttacker.sGridNo, pAttacker.bLevel);
                if (SoldierToSoldierLineOfSightTest(pDefender, pAttacker, ubTileSightLimit, true) != 0)
                {
                    fSeesAttacker = true;
                }
                if (fMuzzleFlash)
                {
                    pAttacker.fMuzzleFlash = true;
                }
            }

            if (fSeesAttacker)
            {
                ManSeesMan(pDefender, pAttacker, pAttacker.sGridNo, pAttacker.bLevel, NOTICEUNSEENATTACKER, CALLER_UNKNOWN);

                // newOppCnt not needed here (no radioing), must get reset right away
                // CJC: Huh? well, leave it in for now
                pDefender.bNewOppCnt = 0;


                if (pDefender.bTeam == gbPlayerNum)
                {
                    // EXPERIENCE GAIN (5): Victim notices/sees a previously UNSEEN attacker
                    Campaign.StatChange(pDefender, Stat.EXPERAMT, 5, false);

                    // mark attacker as being SEEN right now
                    RadioSightings(pDefender, pAttacker.ubID, pDefender.bTeam);

                }
                // NOTE: ENEMIES DON'T REPORT A SIGHTING PUBLICLY UNTIL THEY RADIO IT IN!
                else
                {
                    // go to threatening stance
                    ReevaluateEnemyStance(pDefender, pDefender.usAnimState);
                }
            }
            else  // victim NOTICED the attack, but CAN'T SEE the actual attacker
            {
                SetNewSituation(pDefender);          // re-evaluate situation

                // if victim's alert status is only GREEN or YELLOW
                if (pDefender.bAlertStatus < STATUS_RED)
                {
                    // then this soldier goes to status RED, has proof of enemy presence
                    pDefender.bAlertStatus = STATUS_RED;
                    CheckForChangingOrders(pDefender);
                }

                UpdatePersonal(pDefender, pAttacker.ubID, HEARD_THIS_TURN, pAttacker.sGridNo, pAttacker.bLevel);

                // if the victim is a human-controlled soldier, instantly report publicly
                if (pDefender.uiStatusFlags.HasFlag(SOLDIER.PC))
                {
                    // mark attacker as having been PUBLICLY heard THIS TURN & remember where
                    UpdatePublic(pDefender.bTeam, pAttacker.ubID, HEARD_THIS_TURN, pAttacker.sGridNo, pAttacker.bLevel);
                }
            }

            if (StandardInterruptConditionsMet(pDefender, pAttacker.ubID, bOldOppList))
            {
                //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("INTERRUPT: NoticeUnseenAttacker, standard conditions are met; defender %d, attacker %d", pDefender.ubID, pAttacker.ubID));

                // calculate the interrupt duel points
                //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, "Calculating int duel pts for defender in NUA");
                pDefender.bInterruptDuelPts = CalcInterruptDuelPts(pDefender, pAttacker.ubID, false);
            }
            else
            {
                pDefender.bInterruptDuelPts = Globals.NO_INTERRUPT;
            }

            // say quote

            if (pDefender.bInterruptDuelPts != Globals.NO_INTERRUPT)
            {
                // check for possible interrupt and handle control change if it happens
                // this code is basically ResolveInterruptsVs for 1 man only...

                // calculate active soldier's dueling pts for the upcoming interrupt duel
                //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, "Calculating int duel pts for attacker in NUA");
                pAttacker.bInterruptDuelPts = CalcInterruptDuelPts(pAttacker, pDefender.ubID, false);
                if (InterruptDuel(pDefender, pAttacker))
                {
                    // DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("INTERRUPT: NoticeUnseenAttacker, defender pts %d, attacker pts %d, defender gets interrupt", pDefender.bInterruptDuelPts, pAttacker.bInterruptDuelPts));
                    AddToIntList(pAttacker.ubID, false, true);
                    AddToIntList(pDefender.ubID, true, true);
                    DoneAddingToIntList(pDefender, true, SIGHTINTERRUPT);
                }
                // either way, clear out both sides' duelPts fields to prepare next duel
                pDefender.bInterruptDuelPts = Globals.NO_INTERRUPT;
                //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("Resetting int pts for %d in NUA", pDefender.ubID));
                pAttacker.bInterruptDuelPts = Globals.NO_INTERRUPT;
                // DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("Resetting int pts for %d in NUA", pAttacker.ubID));
            }
        }

        void CheckForAlertWhenEnemyDies(SOLDIERTYPE? pDyingSoldier)
        {
            int ubID;
            SOLDIERTYPE? pSoldier;
            WorldDirections bDir;
            int sDistAway, sDistVisible;

            for (ubID = gTacticalStatus.Team[pDyingSoldier.bTeam].bFirstID; ubID <= gTacticalStatus.Team[pDyingSoldier.bTeam].bLastID; ubID++)
            {

                pSoldier = Globals.MercPtrs[ubID];

                if (pSoldier.bActive && pSoldier.bInSector && (pSoldier != pDyingSoldier) && (pSoldier.bLife >= OKLIFE) && (pSoldier.bAlertStatus < STATUS_RED))
                {
                    // this guy might have seen the man die

                    // distance we "see" then depends on the direction he is located from us
                    bDir = SoldierControl.atan8(pSoldier.sX, pSoldier.sY, pDyingSoldier.sX, pDyingSoldier.sY);
                    sDistVisible = DistanceVisible(pSoldier, pSoldier.bDesiredDirection, bDir, pDyingSoldier.sGridNo, pDyingSoldier.bLevel);
                    sDistAway = PythSpacesAway(pSoldier.sGridNo, pDyingSoldier.sGridNo);

                    // if we see close enough to see the soldier
                    if (sDistAway <= sDistVisible)
                    {
                        // and we can trace a line of sight to his x,y coordinates
                        // assume enemies are always aware of their buddies...
                        if (SoldierTo3DLocationLineOfSightTest(pSoldier, pDyingSoldier.sGridNo, pDyingSoldier.bLevel, 0, (int)sDistVisible, true))
                        {
                            pSoldier.bAlertStatus = STATUS_RED;
                            CheckForChangingOrders(pSoldier);
                        }
                    }
                }

            }

        }

        bool ArmyKnowsOfPlayersPresence()
        {
            int ubID;
            SOLDIERTYPE? pSoldier;

            // if anyone is still left...
            if (gTacticalStatus.Team[ENEMY_TEAM].bTeamActive > 0 && gTacticalStatus.Team[ENEMY_TEAM].bMenInSector > 0)
            {
                for (ubID = gTacticalStatus.Team[ENEMY_TEAM].bFirstID; ubID <= gTacticalStatus.Team[ENEMY_TEAM].bLastID; ubID++)
                {
                    pSoldier = Globals.MercPtrs[ubID];

                    if (pSoldier.bActive && pSoldier.bInSector && (pSoldier.bLife >= OKLIFE) && (pSoldier.bAlertStatus >= STATUS_RED))
                    {
                        return (true);
                    }
                }
            }
            return (false);
        }

        bool MercSeesCreature(SOLDIERTYPE? pSoldier)
        {
            bool fSeesCreature = false;
            int ubID;

            if (pSoldier.bOppCnt > 0)
            {
                for (ubID = gTacticalStatus.Team[CREATURE_TEAM].bFirstID; ubID <= gTacticalStatus.Team[CREATURE_TEAM].bLastID; ubID++)
                {
                    if ((pSoldier.bOppList[ubID] == SEEN_CURRENTLY) && (Globals.MercPtrs[ubID].uiStatusFlags.HasFlag(SOLDIER.MONSTER)))
                    {
                        return (true);
                    }
                }
            }
            return (false);
        }


        int FindUnusedWatchedLoc(int ubID)
        {
            int bLoop;

            for (bLoop = 0; bLoop < NUM_WATCHED_LOCS; bLoop++)
            {
                if (gsWatchedLoc[ubID, bLoop] == NOWHERE)
                {
                    return (bLoop);
                }
            }
            return (-1);
        }

        int FindWatchedLocWithLessThanXPointsLeft(int ubID, int ubPointLimit)
        {
            int bLoop;

            for (bLoop = 0; bLoop < NUM_WATCHED_LOCS; bLoop++)
            {
                if (gsWatchedLoc[ubID, bLoop] != NOWHERE && gubWatchedLocPoints[ubID, bLoop] <= ubPointLimit)
                {
                    return (bLoop);
                }
            }
            return (-1);
        }

        int FindWatchedLoc(int ubID, int sGridNo, int bLevel)
        {
            int bLoop;

            for (bLoop = 0; bLoop < NUM_WATCHED_LOCS; bLoop++)
            {
                if (gsWatchedLoc[ubID, bLoop] != NOWHERE && gbWatchedLocLevel[ubID, bLoop] == bLevel)
                {
                    if (SpacesAway(gsWatchedLoc[ubID, bLoop], sGridNo) <= WATCHED_LOC_RADIUS)
                    {
                        return (bLoop);
                    }
                }
            }
            return (-1);
        }

        int GetWatchedLocPoints(int ubID, int sGridNo, int bLevel)
        {
            int bLoc;

            bLoc = FindWatchedLoc(ubID, sGridNo, bLevel);
            if (bLoc != -1)
            {
                /*
                if (gubWatchedLocPoints[ ubID ,  bLoc ] > 1)
                {
                    ScreenMsg( FONT_MCOLOR_LTYELLOW, MSG_BETAVERSION, "Soldier %d getting %d points for interrupt in watched location", ubID, gubWatchedLocPoints[ ubID ,  bLoc ] - 1 );
                }
                */
                // one loc point is worth nothing, so return number minus 1

                // experiment with 1 loc point being worth 1 point
                return (gubWatchedLocPoints[ubID, bLoc]);
            }

            return (0);
        }


        int GetHighestVisibleWatchedLoc(int ubID)
        {
            int bLoop;
            int bHighestLoc = -1;
            int bHighestPoints = 0;
            int sDistVisible;

            for (bLoop = 0; bLoop < NUM_WATCHED_LOCS; bLoop++)
            {
                if (gsWatchedLoc[ubID, bLoop] != NOWHERE && gubWatchedLocPoints[ubID, bLoop] > bHighestPoints)
                {
                    sDistVisible = DistanceVisible(MercPtrs[ubID], WorldDirections.DIRECTION_IRRELEVANT, WorldDirections.DIRECTION_IRRELEVANT, gsWatchedLoc[ubID, bLoop], gbWatchedLocLevel[ubID, bLoop]);
                    // look at standing height
                    if (SoldierTo3DLocationLineOfSightTest(MercPtrs[ubID], gsWatchedLoc[ubID, bLoop], gbWatchedLocLevel[ubID, bLoop], 3, (int)sDistVisible, true))
                    {
                        bHighestLoc = bLoop;
                        bHighestPoints = gubWatchedLocPoints[ubID, bLoop];
                    }
                }
            }
            return (bHighestLoc);
        }

        int GetHighestWatchedLocPoints(int ubID)
        {
            int bLoop;
            int bHighestPoints = 0;

            for (bLoop = 0; bLoop < NUM_WATCHED_LOCS; bLoop++)
            {
                if (gsWatchedLoc[ubID, bLoop] != NOWHERE && gubWatchedLocPoints[ubID, bLoop] > bHighestPoints)
                {
                    bHighestPoints = gubWatchedLocPoints[ubID, bLoop];
                }
            }
            return (bHighestPoints);
        }


        void CommunicateWatchedLoc(int ubID, int sGridNo, int bLevel, int ubPoints)
        {
            int ubLoop;
            TEAM bTeam;
            int bLoopPoint, bPoint;

            bTeam = Globals.MercPtrs[ubID].bTeam;

            for (ubLoop = gTacticalStatus.Team[bTeam].bFirstID; ubLoop < gTacticalStatus.Team[bTeam].bLastID; ubLoop++)
            {
                if (ubLoop == ubID || Globals.MercPtrs[ubLoop].bActive == false || Globals.MercPtrs[ubLoop].bInSector == false || Globals.MercPtrs[ubLoop].bLife < OKLIFE)
                {
                    continue;
                }
                bLoopPoint = FindWatchedLoc(ubLoop, sGridNo, bLevel);
                if (bLoopPoint == -1)
                {
                    // add this as a watched point
                    bPoint = FindUnusedWatchedLoc(ubLoop);
                    if (bPoint == -1)
                    {
                        // if we have a point with only 1 point left, replace it
                        bPoint = FindWatchedLocWithLessThanXPointsLeft(ubLoop, ubPoints);
                    }
                    if (bPoint != -1)
                    {
                        gsWatchedLoc[ubLoop, bPoint] = sGridNo;
                        gbWatchedLocLevel[ubLoop, bPoint] = bLevel;
                        gubWatchedLocPoints[ubLoop, bPoint] = ubPoints;
                        gfWatchedLocReset[ubLoop, bPoint] = false;
                        gfWatchedLocHasBeenIncremented[ubLoop, bPoint] = true;
                    }
                    // else no points available!
                }
                else
                {
                    // increment to max
                    gubWatchedLocPoints[ubLoop, bLoopPoint] = Math.Max(gubWatchedLocPoints[ubLoop, bLoopPoint], ubPoints);

                    gfWatchedLocReset[ubLoop, bLoopPoint] = false;
                    gfWatchedLocHasBeenIncremented[ubLoop, bLoopPoint] = true;
                }
            }
        }


        void IncrementWatchedLoc(int ubID, int sGridNo, int bLevel)
        {
            int bPoint;

            bPoint = FindWatchedLoc(ubID, sGridNo, bLevel);
            if (bPoint == -1)
            {
                // try adding point
                bPoint = FindUnusedWatchedLoc(ubID);
                if (bPoint == -1)
                {
                    // if we have a point with only 1 point left, replace it
                    bPoint = FindWatchedLocWithLessThanXPointsLeft(ubID, 1);
                }

                if (bPoint != -1)
                {
                    gsWatchedLoc[ubID, bPoint] = sGridNo;
                    gbWatchedLocLevel[ubID, bPoint] = bLevel;
                    gubWatchedLocPoints[ubID, bPoint] = 1;
                    gfWatchedLocReset[ubID, bPoint] = false;
                    gfWatchedLocHasBeenIncremented[ubID, bPoint] = true;

                    CommunicateWatchedLoc(ubID, sGridNo, bLevel, 1);
                }
                // otherwise abort; no points available
            }
            else
            {
                if (!gfWatchedLocHasBeenIncremented[ubID, bPoint] && gubWatchedLocPoints[ubID, bPoint] < MAX_WATCHED_LOC_POINTS)
                {
                    gubWatchedLocPoints[ubID, bPoint]++;
                    CommunicateWatchedLoc(ubID, sGridNo, bLevel, gubWatchedLocPoints[ubID, bPoint]);
                }
                gfWatchedLocReset[ubID, bPoint] = false;
                gfWatchedLocHasBeenIncremented[ubID, bPoint] = true;
            }
        }

        void SetWatchedLocAsUsed(int ubID, int sGridNo, int bLevel)
        {
            int bPoint;

            bPoint = FindWatchedLoc(ubID, sGridNo, bLevel);
            if (bPoint != -1)
            {
                gfWatchedLocReset[ubID, bPoint] = false;
            }
        }

        bool WatchedLocLocationIsEmpty(int sGridNo, int bLevel, TEAM bTeam)
        {
            // look to see if there is anyone near the watched loc who is not on this team
            int ubID;
            int sTempGridNo, sX, sY;

            for (sY = -WATCHED_LOC_RADIUS; sY <= WATCHED_LOC_RADIUS; sY++)
            {
                for (sX = -WATCHED_LOC_RADIUS; sX <= WATCHED_LOC_RADIUS; sX++)
                {
                    sTempGridNo = sGridNo + sX + sY * WORLD_ROWS;
                    if (sTempGridNo < 0 || sTempGridNo >= WORLD_MAX)
                    {
                        continue;
                    }
                    ubID = WorldManager.WhoIsThere2(sTempGridNo, bLevel);
                    if (ubID != Globals.NOBODY && Globals.MercPtrs[ubID].bTeam != bTeam)
                    {
                        return (false);
                    }
                }
            }
            return (true);
        }

        void DecayWatchedLocs(TEAM bTeam)
        {
            int cnt, cnt2;

            // loop through all soldiers
            for (cnt = gTacticalStatus.Team[bTeam].bFirstID; cnt <= gTacticalStatus.Team[bTeam].bLastID; cnt++)
            {
                // for each watched location
                for (cnt2 = 0; cnt2 < NUM_WATCHED_LOCS; cnt2++)
                {
                    if (gsWatchedLoc[cnt, cnt2] != NOWHERE && WatchedLocLocationIsEmpty(gsWatchedLoc[cnt, cnt2], gbWatchedLocLevel[cnt, cnt2], bTeam))
                    {
                        // if the reset flag is still set, then we should decay this point
                        if (gfWatchedLocReset[cnt, cnt2])
                        {
                            // turn flag off again			
                            gfWatchedLocReset[cnt, cnt2] = false;

                            // halve points
                            gubWatchedLocPoints[cnt, cnt2] /= 2;
                            // if points have reached 0, then reset the location
                            if (gubWatchedLocPoints[cnt, cnt2] == 0)
                            {
                                gsWatchedLoc[cnt, cnt2] = NOWHERE;
                            }
                        }
                        else
                        {
                            // flag was false so set to true (will be reset if new people seen there next turn)
                            gfWatchedLocReset[cnt, cnt2] = true;
                        }
                    }
                }
            }
        }

        void MakeBloodcatsHostile()
        {
            int iLoop = gTacticalStatus.Team[TEAM.CREATURE_TEAM].bFirstID;

            //            for (pSoldier = Globals.MercPtrs[iLoop]; iLoop <= gTacticalStatus.Team[TEAM.CREATURE_TEAM].bLastID; iLoop++, pSoldier++)
            foreach (var pSoldier in MercPtrs.Skip(iLoop))
            {
                if (pSoldier.ubBodyType == SoldierBodyTypes.BLOODCAT
                    && pSoldier.bActive && pSoldier.bInSector && pSoldier.IsAlive)
                {
                    SetSoldierNonNeutral(pSoldier);
                    RecalculateOppCntsDueToNoLongerNeutral(pSoldier);
                    if ((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
                    {
                        CheckForPotentialAddToBattleIncrement(pSoldier);
                    }
                }
            }
        }
    }
}

// noise type constants
public enum NOISE
{
    UNKNOWN = 0,
    MOVEMENT,
    CREAKING,
    SPLASHING,
    BULLET_IMPACT,
    GUNFIRE,
    EXPLOSION,
    SCREAM,
    ROCK_IMPACT,
    GRENADE_IMPACT,
    WINDOW_SMASHING,
    DOOR_SMASHING,
    SILENT_ALARM, // only heard by enemies
    MAX_NOISES,
};
