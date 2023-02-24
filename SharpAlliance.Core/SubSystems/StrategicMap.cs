using System;
using System.Threading.Tasks;
using SharpAlliance.Core.Screens;

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
        int iCounterA = 0, iCounterB = 0;

        for (iCounterA = 0; iCounterA < (Globals.MAP_WORLD_X - 1); iCounterA++)
        {
            for (iCounterB = 0; iCounterB < (Globals.MAP_WORLD_Y - 1); iCounterB++)
            {
                if (Globals.StrategicMap[CALCULATE_STRATEGIC_INDEX(iCounterA, iCounterB)].bNameId == bTownId)
                {
                    ubSectorSize++;
                }
            }
        }

        return (ubSectorSize);
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
            if (Soldier.OK_CONTROLLABLE_MERC(pSoldier) && pSoldier.bAssignment == CurrentSquad())
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
                    if (bExitDirection != -1)
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
            if (bExitDirection != -1)
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

    // get index into array
    public static int CALCULATE_STRATEGIC_INDEX(int x, MAP_ROW y) => (x + ((int)y * Globals.MAP_WORLD_X));
    public static int GET_X_FROM_STRATEGIC_INDEX(int i) => (i % Globals.MAP_WORLD_X);
    public static MAP_ROW GET_Y_FROM_STRATEGIC_INDEX(int i) => (MAP_ROW)(i / Globals.MAP_WORLD_X);

    // macros to convert between the 2 different sector numbering systems
    public static int SECTOR_INFO_TO_STRATEGIC_INDEX(int i) => (CALCULATE_STRATEGIC_INDEX(SECTORINFO.SECTORX(i), SECTORINFO.SECTORY(i)));
    public static SEC STRATEGIC_INDEX_TO_SECTOR_INFO(int i) => (SECTORINFO.SECTOR(GET_X_FROM_STRATEGIC_INDEX(i), GET_Y_FROM_STRATEGIC_INDEX(i)));


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
