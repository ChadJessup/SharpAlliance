using System;
using System.Diagnostics;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class Meanwhile
{
    // set flag for this event
    void SetMeanWhileFlag(Meanwhiles ubMeanwhileID)
    {
        switch (ubMeanwhileID)
        {
            case Meanwhiles.END_OF_PLAYERS_FIRST_BATTLE:
                uiMeanWhileFlags |= MEANWHILEFLAGS.END_OF_PLAYERS_FIRST_BATTLE_FLAG;
                break;
            case Meanwhiles.DRASSEN_LIBERATED:
                uiMeanWhileFlags |= MEANWHILEFLAGS.DRASSEN_LIBERATED_FLAG;
                break;
            case Meanwhiles.CAMBRIA_LIBERATED:
                uiMeanWhileFlags |= MEANWHILEFLAGS.CAMBRIA_LIBERATED_FLAG;
                break;
            case Meanwhiles.ALMA_LIBERATED:
                uiMeanWhileFlags |= MEANWHILEFLAGS.ALMA_LIBERATED_FLAG;
                break;
            case Meanwhiles.GRUMM_LIBERATED:
                uiMeanWhileFlags |= MEANWHILEFLAGS.GRUMM_LIBERATED_FLAG;
                break;
            case Meanwhiles.CHITZENA_LIBERATED:
                uiMeanWhileFlags |= MEANWHILEFLAGS.CHITZENA_LIBERATED_FLAG;
                break;
            case Meanwhiles.BALIME_LIBERATED:
                uiMeanWhileFlags |= MEANWHILEFLAGS.BALIME_LIBERATED_FLAG;
                break;
            case Meanwhiles.NW_SAM:
                uiMeanWhileFlags |= MEANWHILEFLAGS.NW_SAM_FLAG;
                break;
            case Meanwhiles.NE_SAM:
                uiMeanWhileFlags |= MEANWHILEFLAGS.NE_SAM_FLAG;
                break;
            case Meanwhiles.CENTRAL_SAM:
                uiMeanWhileFlags |= MEANWHILEFLAGS.CENTRAL_SAM_FLAG;
                break;
            case Meanwhiles.FLOWERS:
                uiMeanWhileFlags |= MEANWHILEFLAGS.FLOWERS_FLAG;
                break;
            case Meanwhiles.LOST_TOWN:
                uiMeanWhileFlags |= MEANWHILEFLAGS.LOST_TOWN_FLAG;
                break;
            case Meanwhiles.CREATURES:
                uiMeanWhileFlags |= MEANWHILEFLAGS.CREATURES_FLAG;
                break;
            case Meanwhiles.KILL_CHOPPER:
                uiMeanWhileFlags |= MEANWHILEFLAGS.KILL_CHOPPER_FLAG;
                break;
            case Meanwhiles.AWOL_SCIENTIST:
                uiMeanWhileFlags |= MEANWHILEFLAGS.AWOL_SCIENTIST_FLAG;
                break;
            case Meanwhiles.OUTSKIRTS_MEDUNA:
                uiMeanWhileFlags |= MEANWHILEFLAGS.OUTSKIRTS_MEDUNA_FLAG;
                break;
            case Meanwhiles.INTERROGATION:
                uiMeanWhileFlags |= MEANWHILEFLAGS.INTERROGATION_FLAG;
                break;
        }
    }

    // is this flag set?
    bool GetMeanWhileFlag(Meanwhiles ubMeanwhileID)
    {
        MEANWHILEFLAGS uiTrue = 0;
        switch (ubMeanwhileID)
        {
            case Meanwhiles.END_OF_PLAYERS_FIRST_BATTLE:
                uiTrue = (uiMeanWhileFlags & MEANWHILEFLAGS.END_OF_PLAYERS_FIRST_BATTLE_FLAG);
                break;
            case Meanwhiles.DRASSEN_LIBERATED:
                uiTrue = (uiMeanWhileFlags & MEANWHILEFLAGS.DRASSEN_LIBERATED_FLAG);
                break;
            case Meanwhiles.CAMBRIA_LIBERATED:
                uiTrue = (uiMeanWhileFlags & MEANWHILEFLAGS.CAMBRIA_LIBERATED_FLAG);
                break;
            case Meanwhiles.ALMA_LIBERATED:
                uiTrue = (uiMeanWhileFlags & MEANWHILEFLAGS.ALMA_LIBERATED_FLAG);
                break;
            case Meanwhiles.GRUMM_LIBERATED:
                uiTrue = (uiMeanWhileFlags & MEANWHILEFLAGS.GRUMM_LIBERATED_FLAG);
                break;
            case Meanwhiles.CHITZENA_LIBERATED:
                uiTrue = (uiMeanWhileFlags & MEANWHILEFLAGS.CHITZENA_LIBERATED_FLAG);
                break;
            case Meanwhiles.BALIME_LIBERATED:
                uiTrue = (uiMeanWhileFlags & MEANWHILEFLAGS.BALIME_LIBERATED_FLAG);
                break;
            case Meanwhiles.NW_SAM:
                uiTrue = (uiMeanWhileFlags & MEANWHILEFLAGS.NW_SAM_FLAG);
                break;
            case Meanwhiles.NE_SAM:
                uiTrue = (uiMeanWhileFlags & MEANWHILEFLAGS.NE_SAM_FLAG);
                break;
            case Meanwhiles.CENTRAL_SAM:
                uiTrue = (uiMeanWhileFlags & MEANWHILEFLAGS.CENTRAL_SAM_FLAG);
                break;
            case Meanwhiles.FLOWERS:
                uiTrue = (uiMeanWhileFlags & MEANWHILEFLAGS.FLOWERS_FLAG);
                break;
            case Meanwhiles.LOST_TOWN:
                uiTrue = (uiMeanWhileFlags & MEANWHILEFLAGS.LOST_TOWN_FLAG);
                break;
            case Meanwhiles.CREATURES:
                uiTrue = (uiMeanWhileFlags & MEANWHILEFLAGS.CREATURES_FLAG);
                break;
            case Meanwhiles.KILL_CHOPPER:
                uiTrue = (uiMeanWhileFlags & MEANWHILEFLAGS.KILL_CHOPPER_FLAG);
                break;
            case Meanwhiles.AWOL_SCIENTIST:
                uiTrue = (uiMeanWhileFlags & MEANWHILEFLAGS.AWOL_SCIENTIST_FLAG);
                break;
            case Meanwhiles.OUTSKIRTS_MEDUNA:
                uiTrue = (uiMeanWhileFlags & MEANWHILEFLAGS.OUTSKIRTS_MEDUNA_FLAG);
                break;
            case Meanwhiles.INTERROGATION:
                uiTrue = (uiMeanWhileFlags & MEANWHILEFLAGS.INTERROGATION_FLAG);
                break;
        }

        if (uiTrue > 0)
        {
            return (true);
        }
        else
        {
            return (false);
        }
    }


    int GetFreeNPCSave()
    {
        int uiCount;

        for (uiCount = 0; uiCount < guiNumNPCSaves; uiCount++)
        {
            if ((gNPCSaveData[uiCount].ubProfile == NO_PROFILE))
                return ((int)uiCount);
        }

        if (guiNumNPCSaves < MAX_MEANWHILE_PROFILES)
            return ((int)guiNumNPCSaves++);

        return (-1);
    }

    void RecountNPCSaves()
    {
        int uiCount;

        for (uiCount = guiNumNPCSaves - 1; (uiCount >= 0); uiCount--)
        {
            if ((gNPCSaveData[uiCount].ubProfile != NO_PROFILE))
            {
                guiNumNPCSaves = (int)(uiCount + 1);
                break;
            }
        }
    }



    void ScheduleMeanwhileEvent(MEANWHILE_DEFINITION? pMeanwhileDef, uint uiTime)
    {
        // event scheduled to happen before, ignore
        if (GetMeanWhileFlag(pMeanwhileDef.ubMeanwhileID) == true)
        {
            return;
        }

        // set the meanwhile flag for this event
        SetMeanWhileFlag(pMeanwhileDef.ubMeanwhileID);

        // set the id value
        ubCurrentMeanWhileId = pMeanwhileDef.ubMeanwhileID;

        // Copy definiaiotn structure into position in global array....
        gMeanwhileDef[(int)pMeanwhileDef.ubMeanwhileID] = pMeanwhileDef;

        // A meanwhile.. poor elliot!
        // increment his slapped count...

        // We need to do it here 'cause they may skip it...
        if (gMercProfiles[NPCID.ELLIOT].bNPCData != 17)
        {
            gMercProfiles[NPCID.ELLIOT].bNPCData++;
        }

        GameEvents.AddStrategicEvent(EVENT.MEANWHILE, uiTime, pMeanwhileDef.ubMeanwhileID);
    }


    bool BeginMeanwhile(int ubMeanwhileID)
    {
        int cnt;

        // copy meanwhile data from array to structure for current
        gCurrentMeanwhileDef = gMeanwhileDef[ubMeanwhileID];

        gfMeanwhileTryingToStart = true;
        GameClock.PauseGame();
        // prevent anyone from messing with the pause!
        GameClock.LockPauseState(6);

        // Set NO_PROFILE info....
        for (cnt = 0; cnt < MAX_MEANWHILE_PROFILES; cnt++)
        {
            gNPCSaveData[cnt].ubProfile = NO_PROFILE;
        }

        return (true);
    }


    void BringupMeanwhileBox()
    {
        int[] zStr = new int[256];

# if JA2TESTVERSION
        wprintf(zStr, "Meanwhile..... ( %S : Remember to make sure towns are controlled if required by script )", gzMeanwhileStr[gCurrentMeanwhileDef.ubMeanwhileID]);
#else
        wprintf(zStr, "%s.....", pMessageStrings[MSG_MEANWHILE]);
#endif

# if JA2TESTVERSION
        if (gCurrentMeanwhileDef.ubMeanwhileID != INTERROGATION)
#else
            if (gCurrentMeanwhileDef.ubMeanwhileID != INTERROGATION && MeanwhileSceneSeen(gCurrentMeanwhileDef.ubMeanwhileID))
#endif
            {
                DoMessageBox(MSG_BOX_BASIC_STYLE, zStr, guiCurrentScreen, MSG_BOX_FLAG_OKSKIP, BeginMeanwhileCallBack, null);
            }
            else
            {
                DoMessageBox(MSG_BOX_BASIC_STYLE, zStr, guiCurrentScreen, (int)MSG_BOX_FLAG_OK, BeginMeanwhileCallBack, null);
            }
    }

    void CheckForMeanwhileOKStart()
    {
        if (gfMeanwhileTryingToStart)
        {
            // Are we in prebattle interface?
            if (gfPreBattleInterfaceActive)
            {
                return;
            }

            if (!InterfaceOKForMeanwhilePopup())
            {
                return;
            }

            if (!DialogueQueueIsEmptyOrSomebodyTalkingNow())
            {
                return;
            }

            gfMeanwhileTryingToStart = false;

            guiOldScreen = guiCurrentScreen;

            if (guiCurrentScreen == ScreenName.GAME_SCREEN)
            {
                LeaveTacticalScreen(ScreenName.GAME_SCREEN);
            }



            // We need to make sure we have no item - at least in tactical
            // In mapscreen, time is paused when manipulating items...
            CancelItemPointer();

            BringupMeanwhileBox();
        }
    }

    void StartMeanwhile()
    {
        int iIndex;
        int bNumDone = 0;

        // OK, save old position...
        if (gfWorldLoaded)
        {
            gsOldSectorX = gWorldSectorX;
            gsOldSectorY = gWorldSectorY;
            gsOldSectorZ = gbWorldSectorZ;
        }

        gsOldSelectedSectorX = sSelMapX;
        gsOldSelectedSectorY = sSelMapY;
        gsOldSelectedSectorZ = (int)iCurrentMapSectorZ;

        gfInMeanwhile = true;

        // ATE: Change music before load
        SetMusicMode(MUSIC_MAIN_MENU);


        gfWorldWasLoaded = gfWorldLoaded;

        // OK, we have been told to start.....
        SetCurrentInterfacePanel((int)TEAM_PANEL);

        // Setup NPC locations, depending on meanwhile type...
        switch (gCurrentMeanwhileDef.ubMeanwhileID)
        {
            case Meanwhiles.END_OF_PLAYERS_FIRST_BATTLE:
            case Meanwhiles.DRASSEN_LIBERATED:
            case Meanwhiles.CAMBRIA_LIBERATED:
            case Meanwhiles.ALMA_LIBERATED:
            case Meanwhiles.GRUMM_LIBERATED:
            case Meanwhiles.CHITZENA_LIBERATED:
            case Meanwhiles.BALIME_LIBERATED:
            case Meanwhiles.NW_SAM:
            case Meanwhiles.NE_SAM:
            case Meanwhiles.CENTRAL_SAM:
            case Meanwhiles.FLOWERS:
            case Meanwhiles.LOST_TOWN:
            case Meanwhiles.CREATURES:
            case Meanwhiles.KILL_CHOPPER:
            case Meanwhiles.AWOL_SCIENTIST:
            case Meanwhiles.OUTSKIRTS_MEDUNA:

                // SAVE QUEEN!
                iIndex = GetFreeNPCSave();
                if (iIndex != -1)
                {
                    gNPCSaveData[iIndex].ubProfile = QUEEN;
                    gNPCSaveData[iIndex].sX = gMercProfiles[QUEEN].sSectorX;
                    gNPCSaveData[iIndex].sY = gMercProfiles[QUEEN].sSectorY;
                    gNPCSaveData[iIndex].sZ = gMercProfiles[QUEEN].bSectorZ;
                    gNPCSaveData[iIndex].sGridNo = gMercProfiles[QUEEN].sGridNo;

                    // Force reload of NPC files...
                    ReloadQuoteFile(QUEEN);

                    ChangeNpcToDifferentSector(QUEEN, 3, 16, 0);
                }

                // SAVE MESSANGER!
                iIndex = GetFreeNPCSave();
                if (iIndex != -1)
                {
                    gNPCSaveData[iIndex].ubProfile = NPCID.ELLIOT;
                    gNPCSaveData[iIndex].sX = gMercProfiles[NPCID.ELLIOT].sSectorX;
                    gNPCSaveData[iIndex].sY = gMercProfiles[NPCID.ELLIOT].sSectorY;
                    gNPCSaveData[iIndex].sZ = gMercProfiles[NPCID.ELLIOT].bSectorZ;
                    gNPCSaveData[iIndex].sGridNo = gMercProfiles[NPCID.ELLIOT].sGridNo;

                    // Force reload of NPC files...
                    ReloadQuoteFile(NPCID.ELLIOT);

                    ChangeNpcToDifferentSector(NPCID.ELLIOT, 3, 16, 0);
                }

                if (gCurrentMeanwhileDef.ubMeanwhileID == OUTSKIRTS_MEDUNA)
                {
                    // SAVE JOE!
                    iIndex = GetFreeNPCSave();
                    if (iIndex != -1)
                    {
                        gNPCSaveData[iIndex].ubProfile = NPCID.JOE;
                        gNPCSaveData[iIndex].sX = gMercProfiles[NPCID.JOE].sSectorX;
                        gNPCSaveData[iIndex].sY = gMercProfiles[NPCID.JOE].sSectorY;
                        gNPCSaveData[iIndex].sZ = gMercProfiles[NPCID.JOE].bSectorZ;
                        gNPCSaveData[iIndex].sGridNo = gMercProfiles[NPCID.JOE].sGridNo;

                        // Force reload of NPC files...
                        ReloadQuoteFile(NPCID.JOE);

                        ChangeNpcToDifferentSector(NPCID.JOE, 3, 16, 0);
                    }
                }

                break;


            case Meanwhiles.INTERROGATION:

                // SAVE QUEEN!
                iIndex = GetFreeNPCSave();
                if (iIndex != -1)
                {
                    gNPCSaveData[iIndex].ubProfile = NPCID.QUEEN;
                    gNPCSaveData[iIndex].sX = gMercProfiles[NPCID.QUEEN].sSectorX;
                    gNPCSaveData[iIndex].sY = gMercProfiles[NPCID.QUEEN].sSectorY;
                    gNPCSaveData[iIndex].sZ = gMercProfiles[NPCID.QUEEN].bSectorZ;
                    gNPCSaveData[iIndex].sGridNo = gMercProfiles[NPCID.QUEEN].sGridNo;

                    // Force reload of NPC files...
                    ReloadQuoteFile(NPCID.QUEEN);

                    ChangeNpcToDifferentSector(NPCID.QUEEN, 7, 14, 0);
                }

                // SAVE MESSANGER!
                iIndex = GetFreeNPCSave();
                if (iIndex != -1)
                {
                    gNPCSaveData[iIndex].ubProfile = NPCID.ELLIOT;
                    gNPCSaveData[iIndex].sX = gMercProfiles[NPCID.ELLIOT].sSectorX;
                    gNPCSaveData[iIndex].sY = gMercProfiles[NPCID.ELLIOT].sSectorY;
                    gNPCSaveData[iIndex].sZ = gMercProfiles[NPCID.ELLIOT].bSectorZ;
                    gNPCSaveData[iIndex].sGridNo = gMercProfiles[NPCID.ELLIOT].sGridNo;

                    // Force reload of NPC files...
                    ReloadQuoteFile(NPCID.ELLIOT);

                    ChangeNpcToDifferentSector(NPCID.ELLIOT, 7, 14, 0);
                }

                // SAVE JOE!
                iIndex = GetFreeNPCSave();
                if (iIndex != -1)
                {
                    gNPCSaveData[iIndex].ubProfile = NPCID.JOE;
                    gNPCSaveData[iIndex].sX = gMercProfiles[NPCID.JOE].sSectorX;
                    gNPCSaveData[iIndex].sY = gMercProfiles[NPCID.JOE].sSectorY;
                    gNPCSaveData[iIndex].sZ = gMercProfiles[NPCID.JOE].bSectorZ;
                    gNPCSaveData[iIndex].sGridNo = gMercProfiles[NPCID.JOE].sGridNo;

                    // Force reload of NPC files...
                    ReloadQuoteFile(NPCID.JOE);

                    ChangeNpcToDifferentSector(NPCID.JOE, 7, 14, 0);
                }

                break;


        }

        // fade out old screen....
        FadeOutNextFrame();

        // Load new map....
        gFadeOutDoneCallback = DoneFadeOutMeanwhile;


    }


    void DoneFadeOutMeanwhile()
    {
        // OK, insertion data found, enter sector!

        SetCurrentWorldSector(gCurrentMeanwhileDef.sSectorX, gCurrentMeanwhileDef.sSectorY, 0);

        //LocateToMeanwhileCharacter( );
        LocateMeanWhileGrid();

        gFadeInDoneCallback = DoneFadeInMeanwhile;

        FadeInNextFrame();
    }


    void DoneFadeInMeanwhile()
    {
        // ATE: double check that we are in meanwhile
        // this is if we cancel right away.....
        if (gfInMeanwhile)
        {
            giNPCReferenceCount = 1;

            if (gCurrentMeanwhileDef.ubMeanwhileID != INTERROGATION)
            {
                gTacticalStatus.uiFlags |= SHOW_ALL_MERCS;
            }

            TriggerNPCRecordImmediately(gCurrentMeanwhileDef.ubNPCNumber, (int)gCurrentMeanwhileDef.usTriggerEvent);
        }
    }




    void BeginMeanwhileCallBack(int bExitValue)
    {
        if (bExitValue == MSG_BOX_RETURN_OK || bExitValue == MSG_BOX_RETURN_YES)
        {
            gTacticalStatus.uiFlags |= ENGAGED_IN_CONV;
            // Increment reference count...
            giNPCReferenceCount = 1;

            StartMeanwhile();
        }
        else
        {
            // skipped scene!
            ProcessImplicationsOfMeanwhile();
            UnLockPauseState();
            UnPauseGame();
        }
    }


    bool AreInMeanwhile()
    {
        STRATEGICEVENT? curr;

        //KM:  April 6, 1999
        //Tactical traversal needs to take precedence over meanwhile events.  When tactically traversing, we
        //expect to make it to the other side without interruption.
        if (gfTacticalTraversal)
        {
            return false;
        }

        if (gfInMeanwhile)
        {
            return true;
        }
        //Check to make sure a meanwhile scene isn't in the event list occurring at the exact same time as this call.  Meanwhile
        //scenes have precedence over a new battle if they occur in the same second.
        curr = gpEventList;
        while (curr is not null)
        {
            if (curr.uiTimeStamp == GameClock.GetWorldTotalSeconds())
            {
                if (curr.ubCallbackID == EVENT.MEANWHILE)
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
            curr = curr.next;
        }

        return (false);
    }

    void ProcessImplicationsOfMeanwhile()
    {
        switch (gCurrentMeanwhileDef.ubMeanwhileID)
        {
            case Meanwhiles.END_OF_PLAYERS_FIRST_BATTLE:
                if (gGameOptions.ubDifficultyLevel == DIF_LEVEL_HARD)
                { //Wake up the queen earlier to punish the good players!
                    ExecuteStrategicAIAction(STRATEGIC_AI_ACTION_WAKE_QUEEN, 0, 0);
                }
                HandleNPCDoAction(NPCID.QUEEN, NPC_ACTION_SEND_SOLDIERS_TO_BATTLE_LOCATION, 0);
                break;
            case Meanwhiles.CAMBRIA_LIBERATED:
            case Meanwhiles.ALMA_LIBERATED:
            case Meanwhiles.GRUMM_LIBERATED:
            case Meanwhiles.CHITZENA_LIBERATED:
            case Meanwhiles.BALIME_LIBERATED:
                ExecuteStrategicAIAction(STRATEGIC_AI_ACTION_WAKE_QUEEN, 0, 0);
                break;
            case Meanwhiles.DRASSEN_LIBERATED:
                ExecuteStrategicAIAction(STRATEGIC_AI_ACTION_WAKE_QUEEN, 0, 0);
                HandleNPCDoAction(NPCID.QUEEN, NPC_ACTION_SEND_SOLDIERS_TO_DRASSEN, 0);
                break;
            case Meanwhiles.CREATURES:
                // add Rat
                HandleNPCDoAction(NPCID.QUEEN, NPC_ACTION_ADD_RAT, 0);
                break;
            case Meanwhiles.AWOL_SCIENTIST:
                {
                    int sSectorX;
                    MAP_ROW sSectorY;

                    StartQuest(QUEST_FIND_SCIENTIST, -1, -1);
                    // place Madlab and robot!
                    if (SectorInfo[SECTOR(7, MAP_ROW.H)].uiFlags & SF.USE_ALTERNATE_MAP)
                    {
                        sSectorX = 7;
                        sSectorY = MAP_ROW.H;
                    }
                    else if (SectorInfo[SECTOR(16, MAP_ROW.H)].uiFlags & SF.USE_ALTERNATE_MAP)
                    {
                        sSectorX = 16;
                        sSectorY = MAP_ROW.H;
                    }
                    else if (SectorInfo[SECTOR(11, MAP_ROW.I)].uiFlags & SF.USE_ALTERNATE_MAP)
                    {
                        sSectorX = 11;
                        sSectorY = MAP_ROW_I;
                    }
                    else if (SectorInfo[SECTOR(4, MAP_ROW.E)].uiFlags & SF.USE_ALTERNATE_MAP)
                    {
                        sSectorX = 4;
                        sSectorY = MAP_ROW.E;
                    }
                    else
                    {
                        Debug.Assert(0);
                    }
                    gMercProfiles[NPCID.MADLAB].sSectorX = sSectorX;
                    gMercProfiles[NPCID.MADLAB].sSectorY = sSectorY;
                    gMercProfiles[NPCID.MADLAB].bSectorZ = 0;

                    gMercProfiles[NPCID.ROBOT].sSectorX = sSectorX;
                    gMercProfiles[NPCID.ROBOT].sSectorY = sSectorY;
                    gMercProfiles[NPCID.ROBOT].bSectorZ = 0;
                }
                break;
            case Meanwhiles.NW_SAM:
                ExecuteStrategicAIAction(NPC_ACTION_SEND_TROOPS_TO_SAM, SAM_1_X, SAM_1_Y);
                break;
            case Meanwhiles.NE_SAM:
                ExecuteStrategicAIAction(NPC_ACTION_SEND_TROOPS_TO_SAM, SAM_2_X, SAM_2_Y);
                break;
            case Meanwhiles.CENTRAL_SAM:
                ExecuteStrategicAIAction(NPC_ACTION_SEND_TROOPS_TO_SAM, SAM_3_X, SAM_3_X);
                break;

            default:
                break;
        }
    }

    void EndMeanwhile()
    {
        int cnt;
        NPCID ubProfile;

        EmptyDialogueQueue();
        ProcessImplicationsOfMeanwhile();
        SetMeanwhileSceneSeen(gCurrentMeanwhileDef.ubMeanwhileID);

        gfInMeanwhile = false;
        giNPCReferenceCount = 0;

        gTacticalStatus.uiFlags &= (~TacticalEngineStatus.ENGAGED_IN_CONV);

        GameClock.UnLockPauseState();
        GameClock.UnPauseGame();

        // ATE: Make sure!
        TurnOffSectorLocator();

        if (gCurrentMeanwhileDef.ubMeanwhileID != Meanwhiles.INTERROGATION)
        {
            gTacticalStatus.uiFlags &= (~TacticalEngineStatus.SHOW_ALL_MERCS);

            // OK, load old sector again.....
            FadeOutNextFrame();

            // Load new map....
            gFadeOutDoneCallback = DoneFadeOutMeanwhileOnceDone;
        }
        else
        {
            // We leave this sector open for our POWs to escape!
            // Set music mode to enemy present!
            SetMusicMode(MUSIC_TACTICAL_ENEMYPRESENT);

            // ATE: Restore people to saved positions...
            // OK, restore NPC save info...
            for (cnt = 0; cnt < guiNumNPCSaves; cnt++)
            {
                ubProfile = gNPCSaveData[cnt].ubProfile;

                if (ubProfile != NO_PROFILE)
                {
                    gMercProfiles[ubProfile].sSectorX = gNPCSaveData[cnt].sX;
                    gMercProfiles[ubProfile].sSectorY = gNPCSaveData[cnt].sY;
                    gMercProfiles[ubProfile].bSectorZ = (int)gNPCSaveData[cnt].sZ;
                    gMercProfiles[ubProfile].sGridNo = (int)gNPCSaveData[cnt].sGridNo;

                    // Ensure NPC files loaded...
                    ReloadQuoteFile(ubProfile);
                }
            }

        }

    }

    void DoneFadeOutMeanwhileOnceDone()
    {
        int cnt;
        NPCID ubProfile;

        // OK, insertion data found, enter sector!
        gfReloadingScreenFromMeanwhile = true;


        if (gfWorldWasLoaded)
        {
            SetCurrentWorldSector(gsOldSectorX, gsOldSectorY, (int)gsOldSectorZ);

            ExamineCurrentSquadLights();
        }
        else
        {
            TrashWorld();
            // NB no world is loaded!
            gWorldSectorX = 0;
            gWorldSectorY = 0;
            gbWorldSectorZ = -1;
        }

        ChangeSelectedMapSector(gsOldSelectedSectorX, gsOldSelectedSectorY, (int)gsOldSelectedSectorZ);

        gfReloadingScreenFromMeanwhile = false;

        // OK, restore NPC save info...
        for (cnt = 0; cnt < guiNumNPCSaves; cnt++)
        {
            ubProfile = gNPCSaveData[cnt].ubProfile;

            if (ubProfile != NO_PROFILE)
            {
                gMercProfiles[ubProfile].sSectorX = gNPCSaveData[cnt].sX;
                gMercProfiles[ubProfile].sSectorY = gNPCSaveData[cnt].sY;
                gMercProfiles[ubProfile].bSectorZ = (int)gNPCSaveData[cnt].sZ;
                gMercProfiles[ubProfile].sGridNo = (int)gNPCSaveData[cnt].sGridNo;

                // Ensure NPC files loaded...
                ReloadQuoteFile(ubProfile);
            }
        }

        gFadeInDoneCallback = DoneFadeInMeanwhileOnceDone;

        // OK, based on screen we were in....
        switch (guiOldScreen)
        {
            case MAP_SCREEN:
                InternalLeaveTacticalScreen(MAP_SCREEN);
                //gfEnteringMapScreen = true;
                break;

            case GAME_SCREEN:
                // restore old interface panel flag
                SetCurrentInterfacePanel((int)TEAM_PANEL);
                break;
        }

        FadeInNextFrame();

    }

    void DoneFadeInMeanwhileOnceDone()
    {

    }

    void LocateMeanWhileGrid()
    {
        int sGridNo = 0;

        // go to the approp. gridno
        sGridNo = gusMeanWhileGridNo[ubCurrentMeanWhileId];

        InternalLocateGridNo(sGridNo, true);

        return;
    }

    void LocateToMeanwhileCharacter()
    {
        SOLDIERTYPE? pSoldier;

        if (gfInMeanwhile)
        {
            pSoldier = FindSoldierByProfileID(gCurrentMeanwhileDef.ubNPCNumber, false);

            if (pSoldier != null)
            {
                LocateSoldier(pSoldier.ubID, false);
            }
        }
    }


    bool AreReloadingFromMeanwhile()
    {
        return (gfReloadingScreenFromMeanwhile);
    }

    int GetMeanwhileID()
    {
        return (gCurrentMeanwhileDef.ubMeanwhileID);
    }


    void HandleCreatureRelease()
    {
        int uiTime = 0;
        MEANWHILE_DEFINITION MeanwhileDef;

        MeanwhileDef.sSectorX = 3;
        MeanwhileDef.sSectorY = 16;
        MeanwhileDef.ubNPCNumber = QUEEN;
        MeanwhileDef.usTriggerEvent = 0;

        uiTime = GetWorldTotalMin() + 5;

        MeanwhileDef.ubMeanwhileID = CREATURES;

        // schedule the event
        ScheduleMeanwhileEvent(&MeanwhileDef, uiTime);
    }


    void HandleMeanWhileEventPostingForTownLiberation(int bTownId)
    {
        // post event for meanwhile whithin the next 6 hours if it still will be daylight, otherwise the next morning
        int uiTime = 0;
        MEANWHILE_DEFINITION MeanwhileDef;
        int ubId = 0;
        bool fHandled = false;

        MeanwhileDef.sSectorX = 3;
        MeanwhileDef.sSectorY = 16;
        MeanwhileDef.ubNPCNumber = QUEEN;
        MeanwhileDef.usTriggerEvent = 0;

        uiTime = GetWorldTotalMin() + 5;

        // which town iberated?
        switch (bTownId)
        {
            case DRASSEN:
                ubId = DRASSEN_LIBERATED;
                fHandled = true;
                break;
            case CAMBRIA:
                ubId = CAMBRIA_LIBERATED;
                fHandled = true;
                break;
            case ALMA:
                ubId = ALMA_LIBERATED;
                fHandled = true;
                break;
            case GRUMM:
                ubId = GRUMM_LIBERATED;
                fHandled = true;
                break;
            case CHITZENA:
                ubId = CHITZENA_LIBERATED;
                fHandled = true;
                break;
            case BALIME:
                ubId = BALIME_LIBERATED;
                fHandled = true;
                break;
        }

        if (fHandled)
        {
            MeanwhileDef.ubMeanwhileID = ubId;

            // schedule the event
            ScheduleMeanwhileEvent(&MeanwhileDef, uiTime);
        }
    }

    void HandleMeanWhileEventPostingForTownLoss(int bTownId)
    {
        int uiTime = 0;
        MEANWHILE_DEFINITION MeanwhileDef;

        // make sure scene hasn't been used before
        if (GetMeanWhileFlag(LOST_TOWN))
        {
            return;
        }

        MeanwhileDef.sSectorX = 3;
        MeanwhileDef.sSectorY = 16;
        MeanwhileDef.ubNPCNumber = QUEEN;
        MeanwhileDef.usTriggerEvent = 0;

        uiTime = GetWorldTotalMin() + 5;

        MeanwhileDef.ubMeanwhileID = LOST_TOWN;

        // schedule the event
        ScheduleMeanwhileEvent(&MeanwhileDef, uiTime);
    }

    void HandleMeanWhileEventPostingForSAMLiberation(int bSamId)
    {
        int uiTime = 0;
        MEANWHILE_DEFINITION MeanwhileDef;
        int ubId = 0;
        bool fHandled = false;

        if (bSamId == -1)
        {
            // invalid parameter!
            return;
        }
        else if (bSamId == 3)
        {
            // no meanwhile scene for this SAM site
            return;
        }

        MeanwhileDef.sSectorX = 3;
        MeanwhileDef.sSectorY = 16;
        MeanwhileDef.ubNPCNumber = QUEEN;
        MeanwhileDef.usTriggerEvent = 0;

        uiTime = GetWorldTotalMin() + 5;

        // which SAM iberated?
        switch (bSamId)
        {
            case 0:
                ubId = NW_SAM;
                fHandled = true;
                break;
            case 1:
                ubId = NE_SAM;
                fHandled = true;
                break;
            case 2:
                ubId = CENTRAL_SAM;
                fHandled = true;
                break;
            default:
                // wtf?
                break;
        }

        if (fHandled)
        {
            MeanwhileDef.ubMeanwhileID = ubId;

            // schedule the event
            ScheduleMeanwhileEvent(&MeanwhileDef, uiTime);
        }


    }

    void HandleFlowersMeanwhileScene(int bTimeCode)
    {
        int uiTime = 0;
        MEANWHILE_DEFINITION MeanwhileDef;
        int ubId = 0;

        // make sure scene hasn't been used before
        if (GetMeanWhileFlag(FLOWERS))
        {
            return;
        }

        MeanwhileDef.sSectorX = 3;
        MeanwhileDef.sSectorY = 16;
        MeanwhileDef.ubNPCNumber = QUEEN;
        MeanwhileDef.usTriggerEvent = 0;

        // time delay should be based on time code, 0 next day, 1 seeral days (random)
        if (bTimeCode == 0)
        {
            // 20-24 hours later
            uiTime = GetWorldTotalMin() + 60 * (20 + Random(5));
        }
        else
        {
            // 2-4 days later
            uiTime = GetWorldTotalMin() + 60 * (24 + Random(48));
        }

        MeanwhileDef.ubMeanwhileID = FLOWERS;

        // schedule the event
        ScheduleMeanwhileEvent(&MeanwhileDef, uiTime);
    }

    void HandleOutskirtsOfMedunaMeanwhileScene()
    {
        int uiTime = 0;
        MEANWHILE_DEFINITION MeanwhileDef;
        int ubId = 0;

        // make sure scene hasn't been used before
        if (GetMeanWhileFlag(OUTSKIRTS_MEDUNA))
        {
            return;
        }

        MeanwhileDef.sSectorX = 3;
        MeanwhileDef.sSectorY = 16;
        MeanwhileDef.ubNPCNumber = QUEEN;
        MeanwhileDef.usTriggerEvent = 0;

        uiTime = GetWorldTotalMin() + 5;

        MeanwhileDef.ubMeanwhileID = OUTSKIRTS_MEDUNA;

        // schedule the event
        ScheduleMeanwhileEvent(&MeanwhileDef, uiTime);
    }

    void HandleKillChopperMeanwhileScene()
    {
        int uiTime = 0;
        MEANWHILE_DEFINITION MeanwhileDef;
        int ubId = 0;

        // make sure scene hasn't been used before
        if (GetMeanWhileFlag(KILL_CHOPPER))
        {
            return;
        }

        MeanwhileDef.sSectorX = 3;
        MeanwhileDef.sSectorY = 16;
        MeanwhileDef.ubNPCNumber = QUEEN;
        MeanwhileDef.usTriggerEvent = 0;

        uiTime = GetWorldTotalMin() + 55 + Random(10);

        MeanwhileDef.ubMeanwhileID = KILL_CHOPPER;

        // schedule the event
        ScheduleMeanwhileEvent(&MeanwhileDef, uiTime);
    }

    void HandleScientistAWOLMeanwhileScene()
    {
        int uiTime = 0;
        MEANWHILE_DEFINITION MeanwhileDef;
        int ubId = 0;

        // make sure scene hasn't been used before
        if (GetMeanWhileFlag(AWOL_SCIENTIST))
        {
            return;
        }

        MeanwhileDef.sSectorX = 3;
        MeanwhileDef.sSectorY = 16;
        MeanwhileDef.ubNPCNumber = QUEEN;
        MeanwhileDef.usTriggerEvent = 0;

        uiTime = GetWorldTotalMin() + 5;

        MeanwhileDef.ubMeanwhileID = AWOL_SCIENTIST;

        // schedule the event
        ScheduleMeanwhileEvent(&MeanwhileDef, uiTime);
    }

    void HandleInterrogationMeanwhileScene()
    {
        int uiTime = 0;
        MEANWHILE_DEFINITION MeanwhileDef;
        int ubId = 0;

        // make sure scene hasn't been used before
        if (GetMeanWhileFlag(INTERROGATION))
        {
            return;
        }

        MeanwhileDef.sSectorX = 7; // what sector?
        MeanwhileDef.sSectorY = MAP_ROW_N;
        MeanwhileDef.ubNPCNumber = QUEEN;
        MeanwhileDef.usTriggerEvent = 0;

        uiTime = GetWorldTotalMin() + 60;

        MeanwhileDef.ubMeanwhileID = INTERROGATION;

        // schedule the event
        ScheduleMeanwhileEvent(&MeanwhileDef, uiTime);
    }

    void HandleFirstBattleVictory()
    {
        int uiTime = 0;
        MEANWHILE_DEFINITION MeanwhileDef;
        int ubId = 0;

        if (GetMeanWhileFlag(END_OF_PLAYERS_FIRST_BATTLE))
        {
            return;
        }

        MeanwhileDef.sSectorX = 3;
        MeanwhileDef.sSectorY = 16;
        MeanwhileDef.ubNPCNumber = QUEEN;
        MeanwhileDef.usTriggerEvent = 0;

        uiTime = GetWorldTotalMin() + 5;

        ubId = END_OF_PLAYERS_FIRST_BATTLE;

        MeanwhileDef.ubMeanwhileID = ubId;

        // schedule the event
        ScheduleMeanwhileEvent(&MeanwhileDef, uiTime);

    }


    void HandleDelayedFirstBattleVictory()
    {
        int uiTime = 0;
        MEANWHILE_DEFINITION MeanwhileDef;
        int ubId = 0;

        if (GetMeanWhileFlag(END_OF_PLAYERS_FIRST_BATTLE))
        {
            return;
        }

        MeanwhileDef.sSectorX = 3;
        MeanwhileDef.sSectorY = 16;
        MeanwhileDef.ubNPCNumber = QUEEN;
        MeanwhileDef.usTriggerEvent = 0;

        /*
        //It is theoretically impossible to liberate a town within 60 minutes of the first battle (which is supposed to
        //occur outside of a town in this scenario).  The delay is attributed to the info taking longer to reach the queen.
        uiTime = GetWorldTotalMin() + 60;
        */
        uiTime = GetWorldTotalMin() + 5;

        ubId = END_OF_PLAYERS_FIRST_BATTLE;

        MeanwhileDef.ubMeanwhileID = ubId;

        // schedule the event
        ScheduleMeanwhileEvent(&MeanwhileDef, uiTime);

    }


    void HandleFirstBattleEndingWhileInTown(int sSectorX, int sSectorY, int bSectorZ, bool fFromAutoResolve)
    {
        int bTownId = 0;
        int sSector = 0;

        if (GetMeanWhileFlag(END_OF_PLAYERS_FIRST_BATTLE))
        {
            return;
        }

        // if this is in fact a town and it is the first battle, then set gfFirstBattleMeanwhileScenePending true
        // if  is true then this is the end of the second battle, post the first meanwhile OR, on call to trash world, that
        // means player is leaving sector

        // grab sector value
        sSector = sSectorX + sSectorY * MAP_WORLD_X;

        // get town name id
        bTownId = StrategicMap[sSector].bNameId;

        if (bTownId == BLANK_SECTOR)
        {
            // invalid town
            HandleDelayedFirstBattleVictory();
            gfFirstBattleMeanwhileScenePending = false;
        }
        else if (gfFirstBattleMeanwhileScenePending || fFromAutoResolve)
        {
            HandleFirstBattleVictory();
            gfFirstBattleMeanwhileScenePending = false;
        }
        else
        {
            gfFirstBattleMeanwhileScenePending = true;
        }

        return;
    }


    void HandleFirstMeanWhileSetUpWithTrashWorld()
    {

        // exiting sector after first battle fought
        if (gfFirstBattleMeanwhileScenePending)
        {
            HandleFirstBattleVictory();
            gfFirstBattleMeanwhileScenePending = false;
        }

    }
}

public struct NPC_SAVE_INFO
{
    public NPCID ubProfile;
    public int sX;
    public MAP_ROW sY;
    public int sZ;
    public int sGridNo;
}

public enum Meanwhiles
{
    END_OF_PLAYERS_FIRST_BATTLE,
    DRASSEN_LIBERATED,
    CAMBRIA_LIBERATED,
    ALMA_LIBERATED,
    GRUMM_LIBERATED,
    CHITZENA_LIBERATED,
    NW_SAM,
    NE_SAM,
    CENTRAL_SAM,
    FLOWERS,
    LOST_TOWN,
    INTERROGATION,
    CREATURES,
    KILL_CHOPPER,
    AWOL_SCIENTIST,
    OUTSKIRTS_MEDUNA,
    BALIME_LIBERATED,
    NUM_MEANWHILES
};


public class MEANWHILE_DEFINITION
{
    public int sSectorX;
    public int sSectorY;
    public int usTriggerEvent;
    public Meanwhiles ubMeanwhileID;
    public int ubNPCNumber;
}

[Flags]
public enum MEANWHILEFLAGS
{
    END_OF_PLAYERS_FIRST_BATTLE_FLAG = 0x00000001,
    DRASSEN_LIBERATED_FLAG = 0x00000002,
    CAMBRIA_LIBERATED_FLAG = 0x00000004,
    ALMA_LIBERATED_FLAG = 0x00000008,
    GRUMM_LIBERATED_FLAG = 0x00000010,
    CHITZENA_LIBERATED_FLAG = 0x00000020,
    NW_SAM_FLAG = 0x00000040,
    NE_SAM_FLAG = 0x00000080,
    CENTRAL_SAM_FLAG = 0x00000100,
    FLOWERS_FLAG = 0x00000200,
    LOST_TOWN_FLAG = 0x00000400,
    CREATURES_FLAG = 0x00000800,
    KILL_CHOPPER_FLAG = 0x00001000,
    AWOL_SCIENTIST_FLAG = 0x00002000,
    OUTSKIRTS_MEDUNA_FLAG = 0x00004000,
    INTERROGATION_FLAG = 0x00008000,
    BALIME_LIBERATED_FLAG = 0x00010000,
}
