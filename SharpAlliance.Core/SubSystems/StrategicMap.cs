using System;
using System.Threading.Tasks;
using SharpAlliance.Core.Screens;
using static SharpAlliance.Core.Globals;
using static SharpAlliance.Core.EnglishText;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SharpAlliance.Core.SubSystems;

public class StrategicMap
{
    public ValueTask<bool> InitStrategicMovementCosts()
    {
        return ValueTask.FromResult(true);
    }

    public ValueTask<bool> InitStrategicEngine()
    {
        return ValueTask.FromResult(true);
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

        return (ubSectorSize);
    }

    public static TOWNS GetTownIdForSector(int sMapX, MAP_ROW sMapY)
    {
        // return the name value of the town in this sector

        return (Globals.strategicMap[CALCULATE_STRATEGIC_INDEX(sMapX, sMapY)].bNameId);
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
                    if (AM_A_ROBOT(pSoldier) && !CanRobotBeControlled(pSoldier))
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

                if (SoldierOKForSectorExit(pSoldier, bExitDirection, usAdditionalData))
                {
                    fAtLeastOneMercControllable++;

                    if (cnt == Globals.gusSelectedSoldier)
                    {
                        fOnlySelectedGuy = true;
                    }
                }
                else
                {
                    GROUP? pGroup;

                    // ATE: Dont's assume exit grids here...
                    if (bExitDirection != (StrategicMove)(-1))
                    {
                        //Now, determine if this is a valid path.
                        pGroup = GetGroup(pValidSoldier.ubGroupID);
                        //AssertMsg(pGroup, string.Format("%S is not in a valid group (pSoldier.ubGroupID is %d)", pValidSoldier.name, pValidSoldier.ubGroupID));
                        if (Globals.gbWorldSectorZ == 0)
                        {
                            puiTraverseTimeInMinutes = GetSectorMvtTimeForGroup(SECTORINFO.SECTOR(pGroup.ubSectorX, pGroup.ubSectorY), bExitDirection, pGroup);
                        }
                        else if (Globals.gbWorldSectorZ > 1)
                        { //We are attempting to traverse in an underground environment.  We need to use a complete different
                          //method.  When underground, all sectors are instantly adjacent.
                            puiTraverseTimeInMinutes = UndergroundTacticalTraversalTime(bExitDirection);
                        }
                        if (puiTraverseTimeInMinutes == 0xffffffff)
                        {
                            Globals.gfInvalidTraversal = true;
                            return 0;
                        }
                    }
                    else
                    {
                        puiTraverseTimeInMinutes = 0; //exit grid travel is instantaneous
                    }
                }
            }
        }

        // If we are here, at least one guy is controllable in this sector, at least he can go!
        if (fAtLeastOneMercControllable > 0)
        {
            ubPlayerControllableMercsInSquad = NumberOfPlayerControllableMercsInSquad(Globals.MercPtrs[Globals.gusSelectedSoldier].bAssignment);
            if (fAtLeastOneMercControllable <= ubPlayerControllableMercsInSquad)
            { //if the selected merc is an EPC and we can only leave with that merc, then prevent it
              //as EPCs aren't allowed to leave by themselves.  Instead of restricting this in the 
              //exiting sector gui, we restrict it by explaining it with a message box.
                if (AM_AN_EPC(Globals.MercPtrs[Globals.gusSelectedSoldier]))
                {
                    if (AM_A_ROBOT(pSoldier) && !CanRobotBeControlled(pSoldier))
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
                GROUP? pGroup;
                //Now, determine if this is a valid path.
                pGroup = GetGroup(pValidSoldier.ubGroupID);
                //AssertMsg(pGroup, string.Format("%S is not in a valid group (pSoldier.ubGroupID is %d)", pValidSoldier.name, pValidSoldier.ubGroupID));
                if (Globals.gbWorldSectorZ == 0)
                {
                    puiTraverseTimeInMinutes = GetSectorMvtTimeForGroup(SECTORINFO.SECTOR(pGroup.ubSectorX, pGroup.ubSectorY), bExitDirection, pGroup);
                }
                else if (Globals.gbWorldSectorZ > 0)
                { //We are attempting to traverse in an underground environment.  We need to use a complete different
                  //method.  When underground, all sectors are instantly adjacent.
                    puiTraverseTimeInMinutes = UndergroundTacticalTraversalTime(bExitDirection);
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

        return (ubReturnVal);
    }

    // Get sector ID string makes a string like 'A9 - OMERTA', or just J11 if no town....
    public static void GetSectorIDString(int sSectorX, MAP_ROW sSectorY, int bSectorZ, string zString, bool fDetailed)
    {
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
            pUnderground = FindUnderGroundSector(sSectorX, sSectorY, bSectorZ);
            if (pUnderground && (pUnderground.fVisited || gfGettingNameFromSaveLoadScreen))
            {
                bMineIndex = StrategicMines.GetIdOfMineForSector(sSectorX, sSectorY, bSectorZ);
                if (bMineIndex != (MINE)(-1))
                {
                    wprintf(zString, "%c%d: %s %s", 'A' + sSectorY - 1, sSectorX, pTownNames[StrategicMines.GetTownAssociatedWithMine(bMineIndex)], pwMineStrings[0]);
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
                            wprintf(ref zString, "P3: %s", pLandTypeStrings[Traversability.SHELTER]);
                            break;
                        default:
                            wprintf(ref zString, "%c%d: %s", 'A' + sSectorY - 1, sSectorX, pLandTypeStrings[Traversability.CREATURE_LAIR]);
                            break;
                    }
                }
            }
            else
            { //Display nothing
                wcscpy(zString, "");
            }
        }
        else
        {
            bTownNameID = strategicMap[CALCULATE_STRATEGIC_INDEX(sSectorX, sSectorY)].bNameId;
            ubSectorID = SECTORINFO.SECTOR(sSectorX, sSectorY);
            pSector = SectorInfo[ubSectorID];
            ubLandType = pSector.ubTraversability[(StrategicMove)4];
            wprintf(zString, "%c%d: ", 'A' + sSectorY - 1, sSectorX);

            if (bTownNameID == TOWNS.BLANK_SECTOR)
            {
                // OK, build string id like J11
                // are we dealing with the unfound towns?
                switch (ubSectorID)
                {
                    case SEC.D2: //Chitzena SAM
                        if (!fSamSiteFound[SAM_SITE.ONE])
                        {
                            zString = wcscat(pLandTypeStrings[Traversability.TROPICS]);
                        }
                        else if (fDetailed)
                        {
                            zString = wcscat(pLandTypeStrings[Traversability.TROPICS_SAM_SITE]);
                        }
                        else
                        {
                            zString = wcscat(pLandTypeStrings[Traversability.SAM_SITE]);
                        }

                        break;
                    case SEC.D15: //Drassen SAM
                        if (!fSamSiteFound[SAM_SITE.TWO])
                        {
                            zString = wcscat(pLandTypeStrings[Traversability.SPARSE]);
                        }
                        else if (fDetailed)
                        {
                            zString = wcscat(pLandTypeStrings[Traversability.SPARSE_SAM_SITE]);
                        }
                        else
                        {
                            zString = wcscat(pLandTypeStrings[Traversability.SAM_SITE]);
                        }

                        break;
                    case SEC.I8: //Cambria SAM
                        if (!fSamSiteFound[SAM_SITE.THREE])
                        {
                            zString = wcscat(pLandTypeStrings[Traversability.SAND]);
                        }
                        else if (fDetailed)
                        {
                            zString = wcscat(pLandTypeStrings[Traversability.SAND_SAM_SITE]);
                        }
                        else
                        {
                            zString = wcscat(pLandTypeStrings[Traversability.SAM_SITE]);
                        }

                        break;
                    default:
                        zString = wcscat(pLandTypeStrings[ubLandType]);
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
                            wcscat(zString, pLandTypeStrings[DRASSEN_AIRPORT_SITE]);
                        }
                        else
                        {
                            wcscat(zString, pTownNames[TOWNS.DRASSEN]);
                        }

                        break;
                    case SEC.F8:
                        if (fDetailed)
                        {
                            wcscat(zString, pLandTypeStrings[CAMBRIA_HOSPITAL_SITE]);
                        }
                        else
                        {
                            wcscat(zString, pTownNames[TOWNS.CAMBRIA]);
                        }

                        break;
                    case SEC.J9: //Tixa
                        if (!fFoundTixa)
                        {
                            wcscat(zString, pLandTypeStrings[SAND]);
                        }
                        else
                        {
                            wcscat(zString, pTownNames[TOWNS.TIXA]);
                        }

                        break;
                    case SEC.K4: //Orta
                        if (!fFoundOrta)
                        {
                            wcscat(zString, pLandTypeStrings[SWAMP]);
                        }
                        else
                        {
                            wcscat(zString, pTownNames[TOWNS.ORTA]);
                        }

                        break;
                    case SEC.N3:
                        if (fDetailed)
                        {
                            wcscat(zString, pLandTypeStrings[MEDUNA_AIRPORT_SITE]);
                        }
                        else
                        {
                            wcscat(zString, pTownNames[TOWNS.MEDUNA]);
                        }

                        break;
                    default:
                        if (ubSectorID == SEC.N4 && fSamSiteFound[SAM_SITE_FOUR])
                        {   //Meduna's SAM site
                            if (fDetailed)
                            {
                                wcscat(zString, pLandTypeStrings[MEDUNA_SAM_SITE]);
                            }
                            else
                            {
                                wcscat(zString, pLandTypeStrings[SAM_SITE]);
                            }
                        }
                        else
                        {   //All other towns that are known since beginning of the game.
                            zString = wcscat(pTownNames[bTownNameID]);
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
                                        zString = wcscat(" "); //space
                                        zString += wcscat(pwMineStrings[0]); //then "Mine"
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
    public static int CALCULATE_STRATEGIC_INDEX(int x, MAP_ROW y) => (x + ((int)y * Globals.MAP_WORLD_X));
    public static int GET_X_FROM_STRATEGIC_INDEX(int i) => (i % Globals.MAP_WORLD_X);
    public static MAP_ROW GET_Y_FROM_STRATEGIC_INDEX(int i) => (MAP_ROW)(i / Globals.MAP_WORLD_X);

    // macros to convert between the 2 different sector numbering systems
    public static int SECTOR_INFO_TO_STRATEGIC_INDEX(SEC i) => (CALCULATE_STRATEGIC_INDEX(SECTORINFO.SECTORX(i), SECTORINFO.SECTORY(i)));
    public static SEC STRATEGIC_INDEX_TO_SECTOR_INFO(int i) => (SECTORINFO.SECTOR(GET_X_FROM_STRATEGIC_INDEX(i), GET_Y_FROM_STRATEGIC_INDEX(i)));

    public static bool IsThisSectorASAMSector(SEC sSectorX, MAP_ROW sSectorY, int bSectorZ)
    {

        // is the sector above ground?
        if (bSectorZ != 0)
        {
            return (false);
        }

        if ((SAM.SAM_1_X == (SAM)sSectorX) && (SAM.SAM_1_Y == (SAM)sSectorY))
        {
            return (true);
        }
        else if ((SAM.SAM_2_X == (SAM)sSectorX) && (SAM.SAM_2_Y == (SAM)sSectorY))
        {
            return (true);
        }
        else if ((SAM.SAM_3_X == (SAM)sSectorX) && (SAM.SAM_3_Y == (SAM)sSectorY))
        {
            return (true);
        }
        else if ((SAM.SAM_4_X == (SAM)sSectorX) && (SAM.SAM_4_Y == (SAM)sSectorY))
        {
            return (true);
        }

        return (false);
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
