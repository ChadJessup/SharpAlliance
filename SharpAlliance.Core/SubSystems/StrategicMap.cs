using System;
using System.Threading.Tasks;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.Managers;
using SharpAlliance.Platform.Interfaces;
using SharpAlliance.Core.Interfaces;

namespace SharpAlliance.Core.SubSystems;

public class StrategicMap
{
    private static IFileManager files;
    private IScreenManager screens;
    private Keys keys;
    private IMusicManager music;
    private AIMain aiMain;

    public StrategicMap(
        IFileManager fileManager,
        IScreenManager screenManager,
        IMusicManager musicManager,
        Keys keys,
        AIMain aiMain)
    {
        this.music = this.music;
        this.keys = keys;
        files = fileManager;
        this.screens = screenManager;
        this.aiMain = aiMain;
    }

    public ValueTask<bool> InitStrategicEngine()
    {
        return ValueTask.FromResult(true);
    }

    public bool SetCurrentWorldSector(int sMapX, MAP_ROW sMapY, int bMapZ)
    {
        UNDERGROUND_SECTORINFO? pUnderWorld = null;
        bool fChangeMusic = true;

        // ATE: Zero out accounting functions
        gbMercIsNewInThisSector = new int[MAX_NUM_SOLDIERS];

//        SyncStrategicTurnTimes();

        // is the sector already loaded?
        if ((gWorldSectorX == sMapX) && (sMapY == gWorldSectorY) && (bMapZ == gbWorldSectorZ))
        {
            //Inserts the enemies into the newly loaded map based on the strategic information.
            //Note, the flag will return true only if enemies were added.  The game may wish to
            //do something else in a case where no enemies are present.

            this.screens.SetPendingNewScreen(ScreenName.GAME_SCREEN);
//            if (!NumEnemyInSector())
//            {
//                PrepareEnemyForSectorBattle();
//            }

            if (gubNumCreaturesAttackingTown > 0
                && gbWorldSectorZ == 0
                && gubSectorIDOfCreatureAttack == SECTORINFO.SECTOR(gWorldSectorX, gWorldSectorY))
            {
//                PrepareCreaturesForBattle();
            }

            if (gfGotoSectorTransition)
            {
//                BeginLoadScreen();
                gfGotoSectorTransition = false;
            }

            // Check for helicopter being on the ground in this sector...
//            HandleHelicopterOnGroundGraphic();

//            ResetMilitia();
            OppList.AllTeamsLookForAll(1);
            return true;
        }

        if (gWorldSectorX > 0 && gWorldSectorY > 0 && gbWorldSectorZ != -1)
        {
//            HandleDefiniteUnloadingOfWorld(ABOUT_TO_LOAD_NEW_MAP);
        }

        // make this the currently loaded sector
        gWorldSectorX = sMapX;
        gWorldSectorY = sMapY;
        gbWorldSectorZ = bMapZ;

        // update currently selected map sector to match
//        ChangeSelectedMapSector(sMapX, sMapY, bMapZ);


        //Check to see if the sector we are loading is the cave sector under Tixa.  If so
        //then we will set up the meanwhile scene to start the creature quest.
        if (!gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.LOADING_SAVED_GAME))
        {
//            StopAnyCurrentlyTalkingSpeech();

            if (gWorldSectorX == 9 && gWorldSectorY == MAP_ROW.J /*10*/ && gbWorldSectorZ == 2)
            {
//                InitCreatureQuest(); //Ignored if already active.
            }
        }

        //Stop playing any music -- will fade out.
        // SetMusicMode( MUSIC_NONE );

        // ATE: Determine if we should set the default music...

        // Are we already in 'tense' music...

        // ATE: Change music only if not loading....
        /*-
        if ( gubMusicMode == MUSIC_TACTICAL_ENEMYPRESENT  )
        {
            fChangeMusic = false;
        }

        // Did we 'tactically traverse' over....
        if ( gfTacticalTraversal )
        {
            fChangeMusic = false;
        }

        // If we have no music playing at all....
        if ( gubMusicMode == MUSIC_NONE  )
        {
            fChangeMusic = true;
        }
        -*/

        if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.LOADING_SAVED_GAME))
        {
            fChangeMusic = true;
        }
        else
        {
            fChangeMusic = false;
        }


        if (fChangeMusic)
        {
            this.music.SetMusicMode(MusicMode.MAIN_MENU);
        }

        // ATE: Do this stuff earlier!
        if (!gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.LOADING_SAVED_GAME))
        {
            // Update the last time we were in tactical...
            gTacticalStatus.uiTimeSinceLastInTactical = GameClock.GetWorldTotalMin();

            // init some AI stuff
//            InitializeTacticalStatusAtBattleStart();

            // CJC: delay this until after entering the sector!
            //InitAI();

            // Check for helicopter being on the ground in this sector...
//            HandleHelicopterOnGroundSkyriderProfile();
        }

        //Load and enter the new sector
//        if (EnterSector(gWorldSectorX, gWorldSectorY, bMapZ))
//        {
//            // CJC: moved this here Feb 17
//            if (!(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.LOADING_SAVED_GAME)))
//            {
//                aiMain.InitAI();
//            }
//
//            //If there are any people with schedules, now is the time to process them.
//            //CJC: doesn't work here if we're going through the tactical placement GUI; moving
//            // this call to PrepareLoadedSector()
//            //PostSchedules();
//
//            // ATE: OK, add code here to update the states of doors if they should 
//            // be closed......
//            if (!(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.LOADING_SAVED_GAME)))
//            {
//                keys.ExamineDoorsOnEnteringSector();
//            }
//
//            // Update all the doors in the sector according to the temp file previously
//            // loaded, and any changes made by the schedules
//            keys.UpdateDoorGraphicsFromStatus(true, false);
//
//            //Set the fact we have visited the  sector
//            TacticalSaveSubSystem.SetSectorFlag(gWorldSectorX, gWorldSectorY, gbWorldSectorZ, SF.ALREADY_LOADED);
//
//            // Check for helicopter being on the ground in this sector...
//            HandleHelicopterOnGroundGraphic();
//        }
//        else
//            return (false);

        if (!gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.LOADING_SAVED_GAME))
        {
            if (gubMusicMode != MusicMode.TACTICAL_ENEMYPRESENT
                && gubMusicMode != MusicMode.TACTICAL_BATTLE
//                || (!NumHostilesInSector(sMapX, sMapY, bMapZ) && gubMusicMode == MusicMode.TACTICAL_ENEMYPRESENT)
                )
            {
                // ATE; Fade FA.T....
                //music.SetMusicFadeSpeed(5);

                this.music.SetMusicMode(MusicMode.TACTICAL_NOTHING);
            }

            // ATE: Check what sector we are in, to show description if we have an RPC.....
//            HandleRPCDescriptionOfSector(sMapX, sMapY, bMapZ);



            // ATE: Set Flag for being visited...
            TacticalSaveSubSystem.SetSectorFlag(sMapX, sMapY, bMapZ, SF.HAS_ENTERED_TACTICAL);

            // ATE; Reset some flags for creature sayings....
            gTacticalStatus.fSaidCreatureFlavourQuote = false;
            gTacticalStatus.fHaveSeenCreature = false;
            gTacticalStatus.fBeenInCombatOnce = false;
            gTacticalStatus.fSaidCreatureSmellQuote = false;
            HandleUI.ResetMultiSelection();

            // ATE: Decide if we can have crows here....
            gTacticalStatus.fGoodToAllowCrows = false;
            gTacticalStatus.fHasEnteredCombatModeSinceEntering = false;
            gTacticalStatus.fDontAddNewCrows = false;

            // Adjust delay for tense quote
            gTacticalStatus.sCreatureTenseQuoteDelay = (int)(10 + Globals.Random.Next(20));

            {

                if (CreatureSpreading.GetWarpOutOfMineCodes(out int sWarpWorldX, out MAP_ROW sWarpWorldY, out int bWarpWorldZ, out int sWarpGridNo) && gbWorldSectorZ >= 2)
                {
                    gTacticalStatus.uiFlags |= TacticalEngineStatus.IN_CREATURE_LAIR;
                }
                else
                {
                    gTacticalStatus.uiFlags &= ~TacticalEngineStatus.IN_CREATURE_LAIR;
                }
            }

            // Every third turn
            //if ( Random( 3 ) == 0  )
            {
                gTacticalStatus.fGoodToAllowCrows = true;
                gTacticalStatus.ubNumCrowsPossible = (int)(5 + Globals.Random.Next(5));
            }

        }

        return true;
    }

    public static void GetMapFileName(int sMapX, MAP_ROW sMapY, int bSectorZ, out string bString, bool fUsePlaceholder, bool fAddAlternateMapLetter)
    {
        string bTestString;
        string bExtensionString;

        if (bSectorZ != 0)
        {
            bExtensionString = sprintf("_b%d", bSectorZ);
        }
        else
        {
            bExtensionString = "";
        }

        // the gfUseAlternateMap flag is set in the loading saved games.  When starting a new game the underground sector
        //info has not been initialized, so we need the flag to load an alternate sector.
//        if (gfUseAlternateMap | GetSectorFlagStatus(sMapX, sMapY, bSectorZ, SF.USE_ALTERNATE_MAP))
        {
            gfUseAlternateMap = false;

            //if we ARE to use the a map, or if we are saving AND the save game version is before 80, add the a
            if (fAddAlternateMapLetter)
            {
                bExtensionString += "_a";
            }
        }

        // If we are in a meanwhile...
        if (Meanwhile.AreInMeanwhile() && sMapX == 3 && sMapY == (MAP_ROW)16 && bSectorZ == 0)//GetMeanwhileID() != INTERROGATION )
        {
            if (fAddAlternateMapLetter)
            {
                bExtensionString += "_m";
            }
        }

        // This is the string to return, but...
        bString = sprintf("%s%s%s.DAT", pVertStrings[sMapY], pHortStrings[sMapX], bExtensionString);

        // We will test against this string
        bTestString = sprintf("MAPS\\%s", bString);

        if (fUsePlaceholder && !files.FileExists(bTestString))
        {
            // Debug str
            // DebugMsg(TOPIC_JA2, DBG_LEVEL_3, string.Format("Map does not exist for %s, using default.", bTestString));
            // Set to a string we know!
            bString = sprintf("H10.DAT", pVertStrings[sMapY], pHortStrings[sMapX]);
            Messages.ScreenMsg(FontColor.FONT_YELLOW, MSG.DEBUG, "Using PLACEHOLDER map!");
        }
        return;
    }

    // return number of sectors this town takes up
    public static int GetTownSectorSize(TOWNS bTownId)
    {
        int ubSectorSize = 0;
        int iCounterA = 0;
        MAP_ROW iCounterB = 0;

        for (iCounterA = 0; iCounterA < (Globals.MAP_WORLD_X - 1); iCounterA++)
        {
            for (iCounterB = 0; (int)iCounterB < (Globals.MAP_WORLD_Y - 1); iCounterB++)
            {
                if (Globals.strategicMap[CALCULATE_STRATEGIC_INDEX(iCounterA, iCounterB)].bNameId == bTownId)
                {
                    ubSectorSize++;
                }
            }
        }

        return ubSectorSize;
    }

    public static TOWNS GetTownIdForSector(int sMapX, MAP_ROW sMapY)
    {
        // return the name value of the town in this sector

        return Globals.strategicMap[CALCULATE_STRATEGIC_INDEX(sMapX, sMapY)].bNameId;
    }


    //ATE: Returns false if NOBODY is close enough, 1 if ONLY selected guy is and 2 if all on squad are...
    public static int OKForSectorExit(StrategicMove bExitDirection, int usAdditionalData, out uint puiTraverseTimeInMinutes)
    {
        puiTraverseTimeInMinutes = 0;

        int cnt;
        SOLDIERTYPE? pSoldier;
        int fAtLeastOneMercControllable = 0;
        bool fOnlySelectedGuy = false;
        SOLDIERTYPE? pValidSoldier = null;
        int ubReturnVal = 0;
        int ubNumControllableMercs = 0;
        int ubNumMercs = 0;
        int ubNumEPCs = 0;
        int ubPlayerControllableMercsInSquad = 0;

        if (Globals.gusSelectedSoldier == Globals.NOBODY)
        { //must have a selected soldier to be allowed to tactically traverse.
            return 0;
        }

        /*
        //Exception code for the two sectors in San Mona that are separated by a cliff.  We want to allow strategic
        //traversal, but NOT tactical traversal.  The only way to tactically go from D4 to D5 (or viceversa) is to enter
        //the cave entrance.
        if( gWorldSectorX == 4 && gWorldSectorY == 4 && !gbWorldSectorZ && bExitDirection == EAST_STRATEGIC_MOVE )
        {
            gfInvalidTraversal = true;
            return false;
        }
        if( gWorldSectorX == 5 && gWorldSectorY == 4 && !gbWorldSectorZ && bExitDirection == WEST_STRATEGIC_MOVE )
        {
            gfInvalidTraversal = true;
            return false;
        }
        */

        Globals.gfInvalidTraversal = false;
        Globals.gfLoneEPCAttemptingTraversal = false;
        Globals.gubLoneMercAttemptingToAbandonEPCs = 0;
        Globals.gbPotentiallyAbandonedEPCSlotID = -1;

        // Look through all mercs and check if they are within range of east end....
        cnt = Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bFirstID;

        // look for all mercs on the same team, 
        for (pSoldier = Globals.MercPtrs[cnt]; cnt <= Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bLastID; cnt++)//, pSoldier++)
        {
            // If we are controllable 
            if (Soldier.OK_CONTROLLABLE_MERC(pSoldier) && pSoldier.bAssignment == (Assignments)Squads.CurrentSquad())
            {
                //Need to keep a copy of a good soldier, so we can access it later, and
                //not more than once.
                pValidSoldier = pSoldier;

                ubNumControllableMercs++;

                //We need to keep track of the number of EPCs and mercs in this squad.  If we have
                //only one merc and one or more EPCs, then we can't allow the merc to tactically traverse,
                //if he is the only merc near enough to traverse.
                if (AM_AN_EPC(pSoldier))
                {
                    ubNumEPCs++;
                    //Also record the EPC's slot ID incase we later build a string using the EPC's name.
                    Globals.gbPotentiallyAbandonedEPCSlotID = (int)cnt;
                    if (AM_A_ROBOT(pSoldier) && !SoldierControl.CanRobotBeControlled(pSoldier))
                    {
                        Globals.gfRobotWithoutControllerAttemptingTraversal = true;
                        ubNumControllableMercs--;
                        continue;
                    }
                }
                else
                {
                    ubNumMercs++;
                }

//                if (SoldierOKForSectorExit(pSoldier, bExitDirection, usAdditionalData))
//                {
//                    fAtLeastOneMercControllable++;
//
//                    if (cnt == Globals.gusSelectedSoldier)
//                    {
//                        fOnlySelectedGuy = true;
//                    }
//                }
//                else
                {
                    GROUP? pGroup;

                    // ATE: Dont's assume exit grids here...
                    if (bExitDirection != (StrategicMove)(-1))
                    {
                        //Now, determine if this is a valid path.
//                        pGroup = GetGroup(pValidSoldier.ubGroupID);
                        //AssertMsg(pGroup, string.Format("%S is not in a valid group (pSoldier.ubGroupID is %d)", pValidSoldier.name, pValidSoldier.ubGroupID));
                        if (Globals.gbWorldSectorZ == 0)
                        {
//                            puiTraverseTimeInMinutes = GetSectorMvtTimeForGroup(SECTORINFO.SECTOR(pGroup.ubSectorX, pGroup.ubSectorY), bExitDirection, pGroup);
                        }
                        else if (Globals.gbWorldSectorZ > 1)
                        { //We are attempting to traverse in an underground environment.  We need to use a complete different
                          //method.  When underground, all sectors are instantly adjacent.
//                            puiTraverseTimeInMinutes = UndergroundTacticalTraversalTime(bExitDirection);
                        }
                        if (puiTraverseTimeInMinutes == 0xffffffff)
                        {
                            Globals.gfInvalidTraversal = true;
                            return 0;
                        }
                    }
//                    else
                    {
                        puiTraverseTimeInMinutes = 0; //exit grid travel is instantaneous
                    }
                }
            }
        }

        // If we are here, at least one guy is controllable in this sector, at least he can go!
        if (fAtLeastOneMercControllable > 0)
        {
//            ubPlayerControllableMercsInSquad = NumberOfPlayerControllableMercsInSquad(Globals.MercPtrs[Globals.gusSelectedSoldier].bAssignment);
            if (fAtLeastOneMercControllable <= ubPlayerControllableMercsInSquad)
            { //if the selected merc is an EPC and we can only leave with that merc, then prevent it
              //as EPCs aren't allowed to leave by themselves.  Instead of restricting this in the 
              //exiting sector gui, we restrict it by explaining it with a message box.
                if (AM_AN_EPC(Globals.MercPtrs[Globals.gusSelectedSoldier]))
                {
                    if (AM_A_ROBOT(pSoldier) && !SoldierControl.CanRobotBeControlled(pSoldier))
                    {
                        //gfRobotWithoutControllerAttemptingTraversal = true;
                        return 0;
                    }
                    else if (fAtLeastOneMercControllable < ubPlayerControllableMercsInSquad || fAtLeastOneMercControllable == 1)
                    {
                        Globals.gfLoneEPCAttemptingTraversal = true;
                        return 0;
                    }
                }
                else
                {   //We previously counted the number of EPCs and mercs, and if the selected merc is not an EPC and there are no
                    //other mercs in the squad able to escort the EPCs, we will prohibit this merc from tactically traversing.
                    if (ubNumEPCs > 0 && ubNumMercs == 1 && fAtLeastOneMercControllable < ubPlayerControllableMercsInSquad)
                    {
                        Globals.gubLoneMercAttemptingToAbandonEPCs = ubNumEPCs;
                        return 0;
                    }
                }
            }
            if (bExitDirection != (StrategicMove)(-1))
            {
                GROUP? pGroup = null;
                //Now, determine if this is a valid path.
//                pGroup = GetGroup(pValidSoldier.ubGroupID);
                //AssertMsg(pGroup, string.Format("%S is not in a valid group (pSoldier.ubGroupID is %d)", pValidSoldier.name, pValidSoldier.ubGroupID));
                if (Globals.gbWorldSectorZ == 0)
                {
//                    puiTraverseTimeInMinutes = GetSectorMvtTimeForGroup(SECTORINFO.SECTOR(pGroup.ubSectorX, pGroup.ubSectorY), bExitDirection, pGroup);
                }
                else if (Globals.gbWorldSectorZ > 0)
                { //We are attempting to traverse in an underground environment.  We need to use a complete different
                  //method.  When underground, all sectors are instantly adjacent.
//                    puiTraverseTimeInMinutes = UndergroundTacticalTraversalTime(bExitDirection);
                }
                if (puiTraverseTimeInMinutes == 0xffffffff)
                {
                    Globals.gfInvalidTraversal = true;
                    ubReturnVal = 0;
                }
                else
                {
                    ubReturnVal = 1;
                }
            }
            else
            {
                ubReturnVal = 1;
                puiTraverseTimeInMinutes = 0; //exit grid travel is instantaneous
            }
        }

        if (ubReturnVal != 0)
        {
            // Default to false again, until we see that we have
            ubReturnVal = 0;

            if (fAtLeastOneMercControllable > 0)
            {
                // Do we contain the selected guy?
                if (fOnlySelectedGuy)
                {
                    ubReturnVal = 1;
                }
                // Is the whole squad able to go here?
                if (fAtLeastOneMercControllable == ubPlayerControllableMercsInSquad)
                {
                    ubReturnVal = 2;
                }
            }
        }

        return ubReturnVal;
    }

    // Get sector ID string makes a string like 'A9 - OMERTA', or just J11 if no town....
    public static void GetSectorIDString(int sSectorX, MAP_ROW sSectorY, int bSectorZ, out string zString, bool fDetailed)
    {
        zString = "";

        SECTORINFO? pSector = null;
        UNDERGROUND_SECTORINFO? pUnderground;
        TOWNS bTownNameID;
        MINE bMineIndex;
        SEC ubSectorID = 0;
        Traversability ubLandType = 0;

        if (sSectorX <= 0 || sSectorY <= 0 || bSectorZ < 0)
        {
            //wprintf( zString, "%s", pErrorStrings[0] );
        }
        else if (bSectorZ != 0)
        {
            pUnderground = QueenCommand.FindUnderGroundSector(sSectorX, sSectorY, bSectorZ);
            if (pUnderground is not null && (pUnderground.fVisited > 0 || gfGettingNameFromSaveLoadScreen))
            {
                bMineIndex = StrategicMines.GetIdOfMineForSector(sSectorX, sSectorY, bSectorZ);
                if (bMineIndex != (MINE)(-1))
                {
                    wprintf(zString, "%c%d: %s %s", 'A' + (int)sSectorY - 1, sSectorX, pTownNames[StrategicMines.GetTownAssociatedWithMine(bMineIndex)], pwMineStrings[0]);
                }
                else
                {
                    switch (SECTORINFO.SECTOR(sSectorX, sSectorY))
                    {
                        case SEC.A10:
                            wprintf(zString, "A10: %s", pLandTypeStrings[Traversability.REBEL_HIDEOUT]);
                            break;
                        case SEC.J9:
                            wprintf(zString, "J9: %s", pLandTypeStrings[Traversability.TIXA_DUNGEON]);
                            break;
                        case SEC.K4:
                            wprintf(zString, "K4: %s", pLandTypeStrings[Traversability.ORTA_BASEMENT]);
                            break;
                        case SEC.O3:
                            wprintf(zString, "O3: %s", pLandTypeStrings[Traversability.TUNNEL]);
                            break;
                        case SEC.P3:
                            wprintf(zString, "P3: %s", pLandTypeStrings[Traversability.SHELTER]);
                            break;
                        default:
                            wprintf(zString, "%c%d: %s", 'A' + (int)sSectorY - 1, sSectorX, pLandTypeStrings[Traversability.CREATURE_LAIR]);
                            break;
                    }
                }
            }
            else
            { //Display nothing
                zString = "";
            }
        }
        else
        {
            bTownNameID = strategicMap[CALCULATE_STRATEGIC_INDEX(sSectorX, sSectorY)].bNameId;
            ubSectorID = SECTORINFO.SECTOR(sSectorX, sSectorY);
            pSector = SectorInfo[ubSectorID];
            ubLandType = pSector.ubTraversability[(StrategicMove)4];
            zString = wprintf("%c%d: ", 'A' + (int)sSectorY - 1, sSectorX);

            if (bTownNameID == TOWNS.BLANK_SECTOR)
            {
                // OK, build string id like J11
                // are we dealing with the unfound towns?
                switch (ubSectorID)
                {
                    case SEC.D2: //Chitzena SAM
                        if (!fSamSiteFound[SAM_SITE.ONE])
                        {
                            zString = wcscat(zString, pLandTypeStrings[Traversability.TROPICS]);
                        }
                        else if (fDetailed)
                        {
                            zString = wcscat(zString, pLandTypeStrings[Traversability.TROPICS_SAM_SITE]);
                        }
                        else
                        {
                            zString = wcscat(zString, pLandTypeStrings[Traversability.SAM_SITE]);
                        }

                        break;
                    case SEC.D15: //Drassen SAM
                        if (!fSamSiteFound[SAM_SITE.TWO])
                        {
                            zString = wcscat(zString, pLandTypeStrings[Traversability.SPARSE]);
                        }
                        else if (fDetailed)
                        {
                            zString = wcscat(zString, pLandTypeStrings[Traversability.SPARSE_SAM_SITE]);
                        }
                        else
                        {
                            zString = wcscat(zString, pLandTypeStrings[Traversability.SAM_SITE]);
                        }

                        break;
                    case SEC.I8: //Cambria SAM
                        if (!fSamSiteFound[SAM_SITE.THREE])
                        {
                            zString = wcscat(zString, pLandTypeStrings[Traversability.SAND]);
                        }
                        else if (fDetailed)
                        {
                            zString = wcscat(zString, pLandTypeStrings[Traversability.SAND_SAM_SITE]);
                        }
                        else
                        {
                            zString = wcscat(zString, pLandTypeStrings[Traversability.SAM_SITE]);
                        }

                        break;
                    default:
                        zString = wcscat(zString, pLandTypeStrings[ubLandType]);
                        break;
                }
            }
            else
            {
                switch (ubSectorID)
                {
                    case SEC.B13:
                        if (fDetailed)
                        {
                            wcscat(zString, pLandTypeStrings[Traversability.DRASSEN_AIRPORT_SITE]);
                        }
                        else
                        {
                            wcscat(zString, pTownNames[TOWNS.DRASSEN]);
                        }

                        break;
                    case SEC.F8:
                        if (fDetailed)
                        {
                            wcscat(zString, pLandTypeStrings[Traversability.CAMBRIA_HOSPITAL_SITE]);
                        }
                        else
                        {
                            wcscat(zString, pTownNames[TOWNS.CAMBRIA]);
                        }

                        break;
                    case SEC.J9: //Tixa
//                        if (!fFoundTixa)
//                        {
//                            wcscat(zString, pLandTypeStrings[Traversability.SAND]);
//                        }
//                        else
//                        {
//                            wcscat(zString, pTownNames[TOWNS.TIXA]);
//                        }

                        break;
                    case SEC.K4: //Orta
                        if (!fFoundOrta)
                        {
                            wcscat(zString, pLandTypeStrings[Traversability.SWAMP]);
                        }
                        else
                        {
                            wcscat(zString, pTownNames[TOWNS.ORTA]);
                        }

                        break;
                    case SEC.N3:
                        if (fDetailed)
                        {
                            wcscat(zString, pLandTypeStrings[Traversability.MEDUNA_AIRPORT_SITE]);
                        }
                        else
                        {
                            wcscat(zString, pTownNames[TOWNS.MEDUNA]);
                        }

                        break;
                    default:
                        if (ubSectorID == SEC.N4 && fSamSiteFound[SAM_SITE.FOUR])
                        {   //Meduna's SAM site
                            if (fDetailed)
                            {
                                wcscat(zString, pLandTypeStrings[Traversability.MEDUNA_SAM_SITE]);
                            }
                            else
                            {
                                wcscat(zString, pLandTypeStrings[Traversability.SAM_SITE]);
                            }
                        }
                        else
                        {   //All other towns that are known since beginning of the game.
                            zString = wcscat(zString, pTownNames[bTownNameID]);
                            if (fDetailed)
                            {
                                switch (ubSectorID)
                                { //Append the word, "mine" for town sectors containing a mine.
                                    case SEC.B2:
                                    case SEC.D4:
                                    case SEC.D13:
                                    case SEC.H3:
                                    case SEC.H8:
                                    case SEC.I14:
                                        zString = wcscat(zString, " "); //space
                                        zString += wcscat(zString, pwMineStrings[0]); //then "Mine"
                                        break;
                                }
                            }
                        }
                        break;
                }
            }
        }
    }

    // get index into array
    public static int CALCULATE_STRATEGIC_INDEX(int x, MAP_ROW y) => x + ((int)y * Globals.MAP_WORLD_X);
    public static int GET_X_FROM_STRATEGIC_INDEX(int i) => i % Globals.MAP_WORLD_X;
    public static MAP_ROW GET_Y_FROM_STRATEGIC_INDEX(int i) => (MAP_ROW)(i / Globals.MAP_WORLD_X);

    // macros to convert between the 2 different sector numbering systems
    public static int SECTOR_INFO_TO_STRATEGIC_INDEX(SEC i) => CALCULATE_STRATEGIC_INDEX(SECTORINFO.SECTORX(i), SECTORINFO.SECTORY(i));
    public static SEC STRATEGIC_INDEX_TO_SECTOR_INFO(int i) => SECTORINFO.SECTOR(GET_X_FROM_STRATEGIC_INDEX(i), GET_Y_FROM_STRATEGIC_INDEX(i));

    public static bool IsThisSectorASAMSector(SEC sSectorX, MAP_ROW sSectorY, int bSectorZ)
    {

        // is the sector above ground?
        if (bSectorZ != 0)
        {
            return false;
        }

        if ((SAM.SAM_1_X == (SAM)sSectorX) && (SAM.SAM_1_Y == (SAM)sSectorY))
        {
            return true;
        }
        else if ((SAM.SAM_2_X == (SAM)sSectorX) && (SAM.SAM_2_Y == (SAM)sSectorY))
        {
            return true;
        }
        else if ((SAM.SAM_3_X == (SAM)sSectorX) && (SAM.SAM_3_Y == (SAM)sSectorY))
        {
            return true;
        }
        else if ((SAM.SAM_4_X == (SAM)sSectorX) && (SAM.SAM_4_Y == (SAM)sSectorY))
        {
            return true;
        }

        return false;
    }

    internal static void SetupNewStrategicGame()
    {
        throw new NotImplementedException();
    }

    internal static void UpdateMercInSector(SOLDIERTYPE sOLDIERTYPE, int v1, int v2, int v3)
    {
        throw new NotImplementedException();
    }

    internal static void InitializeSAMSites()
    {
        throw new NotImplementedException();
    }
}

public class StrategicMapElement
{
    public int[] UNUSEDuiFootEta = new int[4];          // eta/mvt costs for feet 
    public int[] UNUSEDuiVehicleEta = new int[4];       // eta/mvt costs for vehicles 
    public int[] uiBadFootSector = new int[4];    // blocking mvt for foot
    public int[] uiBadVehicleSector = new int[4]; // blocking mvt from vehicles
    public TOWNS bNameId;
    public bool fEnemyControlled;   // enemy controlled or not
    public bool fEnemyAirControlled;
    public bool UNUSEDfLostControlAtSomeTime;
    public int bSAMCondition; // SAM Condition .. 0 - 100, just like an item's status
    public int[] bPadding = new int[20];
}
