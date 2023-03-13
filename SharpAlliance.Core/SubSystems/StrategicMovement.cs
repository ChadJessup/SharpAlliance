using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class StrategicMovement
{
    //Player grouping functions
    //.........................
    //Creates a new player group, returning the unique ID of that group.  This is the first
    //step before adding waypoints and members to the player group.
    int CreateNewPlayerGroupDepartingFromSector(int ubSectorX, MAP_ROW ubSectorY)
    {
        GROUP? pNew;
        ////AssertMsg(ubSectorX >= 1 && ubSectorX <= 16, string.Format("CreateNewPlayerGroup with out of range sectorX value of %d", ubSectorX));
        ////AssertMsg(ubSectorY >= 1 && ubSectorY <= 16, string.Format("CreateNewPlayerGroup with out of range sectorY value of %d", ubSectorY));
        pNew = new GROUP
        {
            ////AssertMsg(pNew, "MemAlloc failure during CreateNewPlayerGroup.");
            //memset(pNew, 0, sizeof(GROUP));
            pPlayerList = null,
            pWaypoints = null,
            ubSectorX = ubSectorX,
            ubNextX = ubSectorX,
            ubSectorY = ubSectorY,
            ubNextY = ubSectorY,
            ubOriginalSector = SECTORINFO.SECTOR(ubSectorX, ubSectorY),
            fPlayer = true,
            ubMoveType = MOVE_TYPES.ONE_WAY,
            ubNextWaypointID = 0,
            ubFatigueLevel = 100,
            ubRestAtFatigueLevel = 0,
            ubTransportationMask = VehicleTypes.FOOT,
            fVehicle = false,
            ubCreatedSectorID = SECTORINFO.SECTOR(ubSectorX, ubSectorY),
            ubSectorIDOfLastReassignment = (SEC)255,
        };

        return AddGroupToList(pNew);
    }


    int CreateNewVehicleGroupDepartingFromSector(int ubSectorX, MAP_ROW ubSectorY, int uiUNISEDVehicleId)
    {
        GROUP? pNew;
        ////AssertMsg(ubSectorX >= 1 && ubSectorX <= 16, string.Format("CreateNewVehicleGroup with out of range sectorX value of %d", ubSectorX));
        ////AssertMsg(ubSectorY >= 1 && ubSectorY <= 16, string.Format("CreateNewVehicleGroup with out of range sectorY value of %d", ubSectorY));
        pNew = new GROUP();
        pNew.ubSectorX = pNew.ubNextX = ubSectorX;
        pNew.ubSectorY = pNew.ubNextY = ubSectorY;
        pNew.ubOriginalSector = SECTORINFO.SECTOR(ubSectorX, ubSectorY);
        pNew.ubMoveType = MOVE_TYPES.ONE_WAY;
        pNew.ubNextWaypointID = 0;
        pNew.ubFatigueLevel = 100;
        pNew.ubRestAtFatigueLevel = 0;
        pNew.fVehicle = true;
        pNew.fPlayer = true;
        pNew.ubCreatedSectorID = pNew.ubOriginalSector;
        pNew.ubSectorIDOfLastReassignment = (SEC)255;

        // get the type
        pNew.ubTransportationMask = VehicleTypes.CAR;

        return (AddGroupToList(pNew));
    }

    //Allows you to add players to the group.
    bool AddPlayerToGroup(int ubGroupID, SOLDIERTYPE? pSoldier)
    {
        GROUP? pGroup;
        PLAYERGROUP? pPlayer, curr;
        pGroup = GetGroup(ubGroupID);
        Debug.Assert(pGroup is not null);
        pPlayer = new PLAYERGROUP();
        Debug.Assert(pPlayer is not null);
        ////AssertMsg(pGroup.fPlayer, "Attempting AddPlayerToGroup() on an ENEMY group!");
        pPlayer.pSoldier = pSoldier;
        pPlayer.ubProfileID = pSoldier.ubProfile;
        pPlayer.ubID = pSoldier.ubID;
        pPlayer.bFlags = 0;
        pPlayer.next = null;


        if (!pGroup.pPlayerList)
        {
            pGroup.pPlayerList = pPlayer;
            pGroup.ubGroupSize = 1;
            pGroup.ubPrevX = (int)(((int)pSoldier.ubPrevSectorID % 16) + 1);
            pGroup.ubPrevY = (MAP_ROW)(((int)pSoldier.ubPrevSectorID / 16) + 1);
            pGroup.ubSectorX = (int)pSoldier.sSectorX;
            pGroup.ubSectorY = pSoldier.sSectorY;
            pGroup.ubSectorZ = (int)pSoldier.bSectorZ;

            // set group id
            pSoldier.ubGroupID = ubGroupID;

            return true;
        }
        else
        {
            curr = pGroup.pPlayerList;
            pSoldier.ubNumTraversalsAllowedToMerge = curr.pSoldier.ubNumTraversalsAllowedToMerge;
            pSoldier.ubDesiredSquadAssignment = curr.pSoldier.ubDesiredSquadAssignment;
            while (curr.next)
            {
                if (curr.ubProfileID == pSoldier.ubProfile)
                {
                    ////AssertMsg(0, String("Attempting to add an already existing merc to group (ubProfile=%d).", pSoldier.ubProfile));
                }

                curr = curr.next;
            }
            curr.next = pPlayer;

            // set group id
            pSoldier.ubGroupID = ubGroupID;

            pGroup.ubGroupSize++;
            return true;
        }
    }


    // remove all grunts from player mvt grp
    bool RemoveAllPlayersFromGroup(int ubGroupId)
    {
        GROUP? pGroup;

        // grab group id
        pGroup = GetGroup(ubGroupId);

        // init errors checks
        // //AssertMsg(pGroup, String("Attempting to RemovePlayerFromGroup( %d ) from non-existant group", ubGroupId));

        return RemoveAllPlayersFromPGroup(pGroup);
    }

    bool RemoveAllPlayersFromPGroup(GROUP? pGroup)
    {
        PLAYERGROUP? curr;

        //AssertMsg(pGroup.fPlayer, "Attempting RemovePlayerFromGroup() on an ENEMY group!");

        curr = pGroup.pPlayerList;
        while (curr)
        {
            pGroup.pPlayerList = pGroup.pPlayerList.next;

            curr.pSoldier.ubPrevSectorID = SECTORINFO.SECTOR(pGroup.ubPrevX, pGroup.ubPrevY);
            curr.pSoldier.ubGroupID = 0;

            MemFree(curr);

            curr = pGroup.pPlayerList;
        }
        pGroup.ubGroupSize = 0;

        if (!pGroup.fPersistant)
        {   //remove the empty group
            RemovePGroup(pGroup);
        }
        else
        {
            CancelEmptyPersistentGroupMovement(pGroup);
        }

        return true;
    }

    bool RemovePlayerFromPGroup(GROUP? pGroup, SOLDIERTYPE? pSoldier)
    {
        PLAYERGROUP? prev, curr;
        //AssertMsg(pGroup.fPlayer, "Attempting RemovePlayerFromGroup() on an ENEMY group!");

        curr = pGroup.pPlayerList;

        if (!curr)
        {
            return false;
        }

        if (curr.pSoldier == pSoldier)
        { //possibly the only node
            pGroup.pPlayerList = pGroup.pPlayerList.next;

            //delete the node
            MemFree(curr);

            //process info for soldier
            pGroup.ubGroupSize--;
            pSoldier.ubPrevSectorID = SECTORINFO.SECTOR(pGroup.ubPrevX, pGroup.ubPrevY);
            pSoldier.ubGroupID = 0;

            // if there's nobody left in the group
            if (pGroup.ubGroupSize == 0)
            {
                if (!pGroup.fPersistant)
                {   //remove the empty group
                    RemovePGroup(pGroup);
                }
                else
                {
                    CancelEmptyPersistentGroupMovement(pGroup);
                }
            }

            return true;
        }
        prev = null;

        while (curr)
        { //definately more than one node

            if (curr.pSoldier == pSoldier)
            {
                //detach and delete the node
                if (prev)
                {
                    prev.next = curr.next;
                }
                MemFree(curr);

                //process info for soldier
                pSoldier.ubGroupID = 0;
                pGroup.ubGroupSize--;
                pSoldier.ubPrevSectorID = SECTORINFO.SECTOR(pGroup.ubPrevX, pGroup.ubPrevY);

                return true;
            }

            prev = curr;
            curr = curr.next;

        }

        // !curr
        return false;
    }

    bool RemovePlayerFromGroup(int ubGroupID, SOLDIERTYPE? pSoldier)
    {
        GROUP? pGroup;
        pGroup = GetGroup(ubGroupID);

        //KM : August 6, 1999 Patch fix
        //     Because the release build has no assertions, it was still possible for the group to be null,
        //     causing a crash.  Instead of crashing, it'll simply return false.
        if (!pGroup)
        {
            return false;
        }
        //end

        //AssertMsg(pGroup, String("Attempting to RemovePlayerFromGroup( %d, %d ) from non-existant group", ubGroupID, pSoldier.ubProfile));

        return RemovePlayerFromPGroup(pGroup, pSoldier);
    }



    bool GroupReversingDirectionsBetweenSectors(GROUP? pGroup, int ubSectorX, MAP_ROW ubSectorY, bool fBuildingWaypoints)
    {
        // if we're not between sectors, or we are but we're continuing in the same direction as before
        if (!GroupBetweenSectorsAndSectorXYIsInDifferentDirection(pGroup, ubSectorX, ubSectorY))
        {
            // then there's no need to reverse directions
            return false;
        }

        //The new direction is reversed, so we have to go back to the sector we just left.

        //Search for the arrival event, and kill it!
        GameEvents.DeleteStrategicEvent(EVENT.GROUP_ARRIVAL, pGroup.ubGroupID);

        //Adjust the information in the group to reflect the new movement.
        pGroup.ubPrevX = pGroup.ubNextX;
        pGroup.ubPrevY = pGroup.ubNextY;
        pGroup.ubNextX = pGroup.ubSectorX;
        pGroup.ubNextY = pGroup.ubSectorY;
        pGroup.ubSectorX = pGroup.ubPrevX;
        pGroup.ubSectorY = pGroup.ubPrevY;

        if (pGroup.fPlayer)
        {
            // ARM: because we've changed the group's ubSectoryX and ubSectorY, we must now also go and change the sSectorX and
            // sSectorY of all the soldiers in this group so that they stay in synch.  Otherwise pathing and movement problems
            // will result since the group is in one place while the merc is in another...
            SetLocationOfAllPlayerSoldiersInGroup(pGroup, pGroup.ubSectorX, pGroup.ubSectorY, 0);
        }


        // IMPORTANT: The traverse time doesn't change just because we reverse directions!  It takes the same time no matter
        // which direction you're going in!  This becomes critical in case the player reverse directions again before moving!

        // The time it takes to arrive there will be exactly the amount of time we have been moving away from it.
        SetGroupArrivalTime(pGroup, pGroup.uiTraverseTime - pGroup.uiArrivalTime + GameClock.GetWorldTotalMin() * 2);

        // if they're not already there
        if (pGroup.uiArrivalTime > GameClock.GetWorldTotalMin())
        {
            //Post the replacement event to move back to the previous sector!
            GameEvents.AddStrategicEvent(EVENT.GROUP_ARRIVAL, pGroup.uiArrivalTime, pGroup.ubGroupID);

            if (pGroup.fPlayer)
            {
                if ((pGroup.uiArrivalTime - ABOUT_TO_ARRIVE_DELAY) > GameClock.GetWorldTotalMin())
                {
                    // Post the about to arrive event
                    GameEvents.AddStrategicEvent(EVENT.GROUP_ABOUT_TO_ARRIVE, pGroup.uiArrivalTime - ABOUT_TO_ARRIVE_DELAY, pGroup.ubGroupID);
                }
            }
        }
        else
        {
            // IMPORTANT: this can't be called during RebuildWayPointsForGroupPath(), since it will clear the mercpath
            // prematurely by assuming the mercs are now at their final destination when only the first waypoint is in place!!!
            // To handle this situation, RebuildWayPointsForGroupPath() will issue it's own call after it's ready for it.
            if (!fBuildingWaypoints)
            {
                // never really left.  Must set check for battle true in order for HandleNonCombatGroupArrival() to run!
                GroupArrivedAtSector(pGroup.ubGroupID, true, true);
            }
        }


        return true;
    }



    bool GroupBetweenSectorsAndSectorXYIsInDifferentDirection(GROUP? pGroup, int ubSectorX, MAP_ROW ubSectorY)
    {
        int currDX, currDY, newDX, newDY;
        int ubNumUnalignedAxes = 0;


        if (!pGroup.fBetweenSectors)
        {
            return (false);
        }


        // Determine the direction the group is currently traveling in
        currDX = pGroup.ubNextX - pGroup.ubSectorX;
        currDY = pGroup.ubNextY - pGroup.ubSectorY;

        //Determine the direction the group would need to travel in to reach the given sector
        newDX = ubSectorX - pGroup.ubSectorX;
        newDY = ubSectorY - pGroup.ubSectorY;

        // clip the new dx/dy values to +/- 1
        if (newDX)
        {
            ubNumUnalignedAxes++;
            newDX /= Math.Abs(newDX);
        }
        if (newDY)
        {
            ubNumUnalignedAxes++;
            newDY /= Math.Abs(newDY);
        }

        // error checking
        if (ubNumUnalignedAxes > 1)
        {
            //AssertMsg(false, String("Checking a diagonal move for direction change, groupID %d. AM-0", pGroup.ubGroupID));
            return false;
        }

        // Compare the dx/dy's.  If they're exactly the same, group is travelling in the same direction as before, so we're not
        // changing directions.
        // Note that 90-degree orthogonal changes are considered changing direction, as well as the full 180-degree reversal.
        // That's because the party must return to the previous sector in each of those cases, too.
        if (currDX == newDX && currDY == newDY)
        {
            return (false);
        }


        // yes, we're between sectors, and we'd be changing direction to go to the given sector
        return (true);
    }


    //Appends a waypoint to the end of the list.  Waypoint MUST be on the
    //same horizontal or vertical level as the last waypoint added.
    bool AddWaypointToPGroup(GROUP? pGroup, int ubSectorX, MAP_ROW ubSectorY) //Same, but overloaded
    {
        List<WAYPOINT> pWay;
        int ubNumAlignedAxes = 0;
        bool fReversingDirection = false;


        //AssertMsg(ubSectorX >= 1 && ubSectorX <= 16, String("AddWaypointToPGroup with out of range sectorX value of %d", ubSectorX));
        //AssertMsg(ubSectorY >= 1 && ubSectorY <= 16, String("AddWaypointToPGroup with out of range sectorY value of %d", ubSectorY));

        if (pGroup is null)
        {
            return false;
        }

        //At this point, we have the group, and a valid coordinate.  Now we must
        //determine that this waypoint will be aligned exclusively to either the x or y axis of
        //the last waypoint in the list.
        pWay = pGroup.pWaypoints;
        if (pWay is null)
        {
            if (GroupReversingDirectionsBetweenSectors(pGroup, ubSectorX, ubSectorY, true))
            {
                if (pGroup.fPlayer)
                {
                    // because we reversed, we must add the new current sector back at the head of everyone's mercpath
                    AddSectorToFrontOfMercPathForAllSoldiersInGroup(pGroup, pGroup.ubSectorX, pGroup.ubSectorY);
                }

                //Very special case that requiring specific coding.  Check out the comments
                //at the above function for more information.
                fReversingDirection = true;
                // ARM:  Kris - new rulez.  Must still fall through and add a waypoint anyway!!!
            }
            else
            { //No waypoints, so compare against the current location.
                if (pGroup.ubSectorX == ubSectorX)
                {
                    ubNumAlignedAxes++;
                }
                if (pGroup.ubSectorY == ubSectorY)
                {
                    ubNumAlignedAxes++;
                }
            }
        }
        else
        {   //we do have a waypoint list, so go to the last entry
            while (pWay.next)
            {
                pWay = pWay.next;
            }
            //now, we are pointing to the last waypoint in the list
            if (pWay.x == ubSectorX)
            {
                ubNumAlignedAxes++;
            }
            if (pWay.y == ubSectorY)
            {
                ubNumAlignedAxes++;
            }
        }

        if (!fReversingDirection)
        {
            if (ubNumAlignedAxes == 0)
            {
                //AssertMsg(false, String("Invalid DIAGONAL waypoint being added for groupID %d. AM-0", pGroup.ubGroupID));
                return false;
            }

            if (ubNumAlignedAxes >= 2)
            {
                //AssertMsg(false, String("Invalid IDENTICAL waypoint being added for groupID %d. AM-0", pGroup.ubGroupID));
                return false;
            }

            // has to be different in exactly 1 axis to be a valid new waypoint
            Debug.Assert(ubNumAlignedAxes == 1);
        }


        if (pWay is null)
        { //We are adding the first waypoint.
            pGroup.pWaypoints = new();
            pWay = pGroup.pWaypoints;
        }
        else
        { //Add the waypoint to the end of the list
            pWay.next = new WAYPOINT();
            pWay = pWay.next;
        }

        //AssertMsg(pWay, "Failed to allocate memory for waypoint.");

        //Fill in the information for the new waypoint.
        pWay.x = ubSectorX;
        pWay.y = ubSectorY;
        pWay.next = null;

        //IMPORTANT:
        //The first waypoint added actually initiates the group's movement to the next sector.
        if (pWay == pGroup.pWaypoints)
        {
            // don't do this if we have reversed directions!!!  In that case, the required work has already been done back there
            if (!fReversingDirection)
            {
                //We need to calculate the next sector the group is moving to and post an event for it.
                InitiateGroupMovementToNextSector(pGroup);
            }
        }

        if (pGroup.fPlayer)
        {
            PLAYERGROUP? curr;
            //Also, nuke any previous "tactical traversal" information.
            curr = pGroup.pPlayerList;
            while (curr)
            {
                curr.pSoldier.ubStrategicInsertionCode = 0;
                curr = curr.next;
            }
        }

        return true;
    }

    bool AddWaypointToGroup(int ubGroupID, int ubSectorX, int ubSectorY)
    {
        GROUP? pGroup;
        pGroup = GetGroup(ubGroupID);
        return AddWaypointToPGroup(pGroup, ubSectorX, ubSectorY);
    }

    // NOTE: This does NOT expect a strategic sector ID
    bool AddWaypointIDToGroup(int ubGroupID, int ubSectorID)
    {
        GROUP? pGroup;
        pGroup = GetGroup(ubGroupID);
        return AddWaypointIDToPGroup(pGroup, ubSectorID);
    }

    // NOTE: This does NOT expect a strategic sector ID
    bool AddWaypointIDToPGroup(GROUP? pGroup, SEC ubSectorID)
    {
        int ubSectorX = SECTORINFO.SECTORX(ubSectorID);
        MAP_ROW ubSectorY = SECTORINFO.SECTORY(ubSectorID);
        return AddWaypointToPGroup(pGroup, ubSectorX, ubSectorY);
    }

    bool AddWaypointStrategicIDToGroup(int ubGroupID, int uiSectorID)
    {
        GROUP? pGroup;
        pGroup = GetGroup(ubGroupID);
        return AddWaypointStrategicIDToPGroup(pGroup, uiSectorID);
    }

    bool AddWaypointStrategicIDToPGroup(GROUP? pGroup, int uiSectorID)
    {
        int ubSectorX, ubSectorY;
        ubSectorX = (int)GET_X_FROM_STRATEGIC_INDEX(uiSectorID);
        ubSectorY = (int)GET_Y_FROM_STRATEGIC_INDEX(uiSectorID);
        return AddWaypointToPGroup(pGroup, ubSectorX, ubSectorY);
    }


    //Enemy grouping functions -- private use by the strategic AI.
    //............................................................
    private static GROUP? CreateNewEnemyGroupDepartingFromSector(SEC uiSector, int ubNumAdmins, int ubNumTroops, int ubNumElites)
    {
        GROUP pNew = new GROUP
        {
            pEnemyGroup = new(),
            //AssertMsg(uiSector >= 0 && uiSector <= 255, String("CreateNewEnemyGroup with out of range value of %d", uiSector));
            //AssertMsg(pNew, "MemAlloc failure during CreateNewEnemyGroup.");
            //memset(pNew, 0, sizeof(GROUP));
            //AssertMsg(pNew.pEnemyGroup, "MemAlloc failure during enemy group creation.");

            pWaypoints = null,
            ubSectorX = SECTORINFO.SECTORX(uiSector),
            ubSectorY = SECTORINFO.SECTORY(uiSector),
            ubOriginalSector = (int)uiSector,
            fPlayer = false,
            ubMoveType = MOVE_TYPES.CIRCULAR,
            ubNextWaypointID = 0,
            ubFatigueLevel = 100,
            ubRestAtFatigueLevel = 0,

            pEnemyGroup = new List<ENEMYGROUP>
            {
                new()
                {
                    ubNumAdmins = ubNumAdmins,
                    ubNumTroops = ubNumTroops,
                    ubNumElites = ubNumElites,
                }
            },
            ubGroupSize = (int)(ubNumTroops + ubNumElites),
            ubTransportationMask = FOOT,
            fVehicle = false,
            ubCreatedSectorID = ubOriginalSector,
            ubSectorIDOfLastReassignment = 255,
        };



        if (AddGroupToList(pNew))
        {
            return pNew;
        }

        return null;
    }

    //INTERNAL LIST MANIPULATION FUNCTIONS

    //When adding any new group to the list, this is what must be done:
    //1)  Find the first unused ID (unique)
    //2)  Assign that ID to the new group
    //3)  Insert the group at the end of the list.
    public static int AddGroupToList(GROUP? pGroup)
    {
        GROUP? curr;
        int bit, index, mask;
        int ID = 0;
        //First, find a unique ID
        while (++ID)
        {
            index = ID / 32;
            bit = ID % 32;
            mask = 1 << bit;
            if (!(uniqueIDMask[index] & mask))
            { //found a free ID
                pGroup.ubGroupID = ID;
                uniqueIDMask[index] += mask;
                //add group to list now.
                curr = gpGroupList;
                if (curr)
                { //point to the last item in list.
                    while (curr.next)
                    {
                        curr = curr.next;
                    }

                    curr.next = pGroup;
                }
                else //new list
                {
                    gpGroupList = pGroup;
                }

                pGroup.next = null;
                return ID;
            }
        }
        return false;
    }

    void RemoveGroupIdFromList(int ubId)
    {
        GROUP? pGroup;

        if (ubId == 0)
        {
            // no group, leave
            return;
        }

        // get group
        pGroup = GetGroup(ubId);

        // is there in fact a group?
        Debug.Assert(pGroup);

        // now remove this group
        RemoveGroupFromList(pGroup);

    }
    //Destroys the waypoint list, detaches group from list, then deallocated the memory for the group
    void RemoveGroupFromList(GROUP? pGroup)
    {
        GROUP? curr, temp;
        curr = gpGroupList;
        if (!curr)
        {
            return;
        }

        if (curr == pGroup)
        { //Removing head
            gpGroupList = curr.next;
        }
        else
        {
            while (curr.next)
            { //traverse the list
                if (curr.next == pGroup)
                { //the next node is the one we want to remove
                    temp = curr;
                    //curr now points to the nod we want to remove
                    curr = curr.next;
                    //detach the node from the list
                    temp.next = curr.next;
                    break;
                }
                curr = curr.next;
            }
        }

        if (curr == pGroup)
        { //we found the group, so now remove it.
            int bit, index, mask;

            //clear the unique group ID
            index = pGroup.ubGroupID / 32;
            bit = pGroup.ubGroupID % 32;
            mask = 1 << bit;

            if (!(uniqueIDMask[index] & mask))
            {
                mask = mask;
            }

            uniqueIDMask[index] -= mask;

            MemFree(curr);
            curr = null;
        }
    }

    public static GROUP? GetGroup(int ubGroupID)
    {
        GROUP? curr;
        curr = gpGroupList;
        while (curr is not null)
        {
            if (curr.ubGroupID == ubGroupID)
            {
                return curr;
            }

            curr = curr.next;
        }

        return null;
    }

    void HandleImportantPBIQuote(SOLDIERTYPE? pSoldier, GROUP? pInitiatingBattleGroup)
    {
        // wake merc up for THIS quote
        if (pSoldier.fMercAsleep)
        {
            TacticalCharacterDialogueWithSpecialEvent(pSoldier, QUOTE_ENEMY_PRESENCE, DIALOGUE_SPECIAL_EVENT_SLEEP, 0, 0);
            TacticalCharacterDialogueWithSpecialEvent(pSoldier, QUOTE_ENEMY_PRESENCE, DIALOGUE_SPECIAL_EVENT_BEGINPREBATTLEINTERFACE, (int)pInitiatingBattleGroup, 0);
            TacticalCharacterDialogueWithSpecialEvent(pSoldier, QUOTE_ENEMY_PRESENCE, DIALOGUE_SPECIAL_EVENT_SLEEP, 1, 0);
        }
        else
        {
            TacticalCharacterDialogueWithSpecialEvent(pSoldier, QUOTE_ENEMY_PRESENCE, DIALOGUE_SPECIAL_EVENT_BEGINPREBATTLEINTERFACE, (int)pInitiatingBattleGroup, 0);
        }
    }

    //If this is called, we are setting the game up to bring up the prebattle interface.  Before doing so,
    //one of the involved mercs will pipe up.  When he is finished, we automatically go into the mapscreen,
    //regardless of the mode we are in.
    void PrepareForPreBattleInterface(GROUP? pPlayerDialogGroup, GROUP? pInitiatingBattleGroup)
    {
        // ATE; Changed alogrithm here...
        // We first loop through the group and save ubID's ov valid guys to talk....
        // ( Can't if sleeping, unconscious, and EPC, etc....
        int[] ubMercsInGroup = new int[20];
        int ubNumMercs = 0;
        int ubChosenMerc;
        SOLDIERTYPE? pSoldier;
        PLAYERGROUP? pPlayer;

        if (fDisableMapInterfaceDueToBattle)
        {
            //AssertMsg(0, "fDisableMapInterfaceDueToBattle is set before attempting to bring up PBI.  Please send PRIOR save if possible and details on anything that just happened before this battle.");
            return;
        }

        // Pipe up with quote...
        //AssertMsg(pPlayerDialogGroup, "Didn't get a player dialog group for prebattle interface.");

        pPlayer = pPlayerDialogGroup.pPlayerList;
        //AssertMsg(pPlayer, String("Player group %d doesn't have *any* players in it!  (Finding dialog group)", pPlayerDialogGroup.ubGroupID));


        while (pPlayer != null)
        {
            pSoldier = pPlayer.pSoldier;

            if (pSoldier.bLife >= OKLIFE && !(pSoldier.uiStatusFlags & SOLDIER_VEHICLE) &&
                        !AM_A_ROBOT(pSoldier) && !AM_AN_EPC(pSoldier))
            {
                ubMercsInGroup[ubNumMercs] = pSoldier.ubID;
                ubNumMercs++;
            }

            pPlayer = pPlayer.next;
        }

        //Set music
        SetMusicMode(MUSIC_TACTICAL_ENEMYPRESENT);

        if (gfTacticalTraversal && pInitiatingBattleGroup == gpTacticalTraversalGroup ||
                pInitiatingBattleGroup && !pInitiatingBattleGroup.fPlayer &&
                pInitiatingBattleGroup.ubSectorX == gWorldSectorX &&
              pInitiatingBattleGroup.ubSectorY == gWorldSectorY && !gbWorldSectorZ)
        {   // At least say quote....
            if (ubNumMercs > 0)
            {
                if (pPlayerDialogGroup.uiFlags & GROUPFLAG_JUST_RETREATED_FROM_BATTLE)
                {
                    gfCantRetreatInPBI = true;
                }

                ubChosenMerc = (int)Globals.Random.Next(ubNumMercs);

                pSoldier = MercPtrs[ubMercsInGroup[ubChosenMerc]];
                gpTacticalTraversalChosenSoldier = pSoldier;

                if (!gfTacticalTraversal)
                {
                    HandleImportantPBIQuote(pSoldier, pInitiatingBattleGroup);
                }

                GameClock.InterruptTime();
                GameClock.PauseGame();
                GameClock.LockPauseState(11);

                if (!gfTacticalTraversal)
                {
                    fDisableMapInterfaceDueToBattle = true;
                }
            }
            return;
        }


        // Randomly pick a valid merc from the list we have created!
        if (ubNumMercs > 0)
        {
            if (pPlayerDialogGroup.uiFlags & GROUPFLAG_JUST_RETREATED_FROM_BATTLE)
            {
                gfCantRetreatInPBI = true;
            }

            ubChosenMerc = (int)Globals.Random.Next(ubNumMercs);

            pSoldier = MercPtrs[ubMercsInGroup[ubChosenMerc]];

            HandleImportantPBIQuote(pSoldier, pInitiatingBattleGroup);
            GameClock.InterruptTime();
            GameClock.PauseGame();
            GameClock.LockPauseState(12);

            // disable exit from mapscreen and what not until face done talking
            fDisableMapInterfaceDueToBattle = true;
        }
        else
        {
            // ATE: What if we have unconscious guys, etc....
            // We MUST start combat, but donot play quote...
            InitPreBattleInterface(pInitiatingBattleGroup, true);
        }
    }





    bool CheckConditionsForBattle(GROUP? pGroup)
    {
        GROUP? curr;
        GROUP? pPlayerDialogGroup = null;
        PLAYERGROUP? pPlayer;
        SOLDIERTYPE? pSoldier;
        bool fBattlePending = false;
        bool fPossibleQueuedBattle = false;
        bool fAliveMerc = false;
        bool fMilitiaPresent = false;
        bool fCombatAbleMerc = false;
        bool fBloodCatAmbush = false;

        if (gfWorldLoaded)
        { //look for people arriving in the currently loaded sector.  This handles reinforcements.
            curr = FindMovementGroupInSector((int)gWorldSectorX, (int)gWorldSectorY, true);
            if (!gbWorldSectorZ && PlayerMercsInSector((int)gWorldSectorX, (int)gWorldSectorY, gbWorldSectorZ) &&
                    pGroup.ubSectorX == gWorldSectorX && pGroup.ubSectorY == gWorldSectorY &&
                    curr)
            { //Reinforcements have arrived!

                if (gTacticalStatus.fEnemyInSector)
                {
                    HandleArrivalOfReinforcements(pGroup);
                    return (true);
                }
            }
        }

        if (!DidGameJustStart())
        {
            gubEnemyEncounterCode = NO_ENCOUNTER_CODE;
        }

        HandleOtherGroupsArrivingSimultaneously(pGroup.ubSectorX, pGroup.ubSectorY, pGroup.ubSectorZ);

        curr = gpGroupList;
        while (curr)
        {
            if (curr.fPlayer && curr.ubGroupSize)
            {
                if (!curr.fBetweenSectors)
                {
                    if (curr.ubSectorX == pGroup.ubSectorX && curr.ubSectorY == pGroup.ubSectorY && !curr.ubSectorZ)
                    {
                        if (!GroupHasInTransitDeadOrPOWMercs(curr) &&
                                (!IsGroupTheHelicopterGroup(curr) || !fHelicopterIsAirBorne) &&
                                (!curr.fVehicle || NumberMercsInVehicleGroup(curr)))
                        {
                            //Now, a player group is in this sector.  Determine if the group contains any mercs that can fight.  
                            //Vehicles, EPCs and the robot doesn't count.  Mercs below OKLIFE do.
                            pPlayer = curr.pPlayerList;
                            while (pPlayer)
                            {
                                pSoldier = pPlayer.pSoldier;
                                if (!(pSoldier.uiStatusFlags & SOLDIER_VEHICLE))
                                {
                                    if (!AM_A_ROBOT(pSoldier) &&
                                            !AM_AN_EPC(pSoldier) &&
                                            pSoldier.bLife >= OKLIFE)
                                    {
                                        fCombatAbleMerc = true;
                                    }
                                    if (pSoldier.bLife > 0)
                                    {
                                        fAliveMerc = true;
                                    }
                                }
                                pPlayer = pPlayer.next;
                            }
                            if (!pPlayerDialogGroup && fCombatAbleMerc)
                            {
                                pPlayerDialogGroup = curr;
                            }
                            if (fCombatAbleMerc)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            curr = curr.next;
        }

        if (pGroup.fPlayer)
        {
            pPlayerDialogGroup = pGroup;

            if (NumEnemiesInSector(pGroup.ubSectorX, pGroup.ubSectorY))
            {
                fBattlePending = true;
            }

            if (pGroup.uiFlags & GROUPFLAG_HIGH_POTENTIAL_FOR_AMBUSH && fBattlePending)
            { //This group has just arrived in a new sector from an adjacent sector that he retreated from
              //If this battle is an encounter type battle, then there is a 90% chance that the battle will
              //become an ambush scenario.
                gfHighPotentialForAmbush = true;
            }

            //If there are bloodcats in this sector, then it internally checks and handles it
            if (TestForBloodcatAmbush(pGroup))
            {
                fBloodCatAmbush = true;
                fBattlePending = true;
            }

            if (fBattlePending && (!fBloodCatAmbush || gubEnemyEncounterCode == ENTERING_BLOODCAT_LAIR_CODE))
            {
                if (PossibleToCoordinateSimultaneousGroupArrivals(pGroup))
                {
                    return false;
                }
            }
        }
        else
        {
            if (CountAllMilitiaInSector(pGroup.ubSectorX, pGroup.ubSectorY))
            {
                fMilitiaPresent = true;
                fBattlePending = true;
            }
            if (fAliveMerc)
            {
                fBattlePending = true;
            }
        }

        if (!fAliveMerc && !fMilitiaPresent)
        { //empty vehicle, everyone dead, don't care.  Enemies don't care.
            return false;
        }

        if (fBattlePending)
        {   //A battle is pending, but the player's could be all unconcious or dead.
            //Go through every group until we find at least one concious merc.  The looping will determine
            //if there are any live mercs and/or concious ones.  If there are no concious mercs, but alive ones,
            //then we will go straight to autoresolve, where the enemy will likely annihilate them or capture them.
            //If there are no alive mercs, then there is nothing anybody can do.  The enemy will completely ignore
            //this, and continue on.


            if (gubNumGroupsArrivedSimultaneously)
            { //Because this is a battle case, clear all the group flags 
                curr = gpGroupList;
                while (curr && gubNumGroupsArrivedSimultaneously)
                {
                    if (curr.uiFlags & GROUPFLAG_GROUP_ARRIVED_SIMULTANEOUSLY)
                    {
                        curr.uiFlags &= ~GROUPFLAG_GROUP_ARRIVED_SIMULTANEOUSLY;
                        gubNumGroupsArrivedSimultaneously--;
                    }
                    curr = curr.next;
                }
            }

            gpInitPrebattleGroup = pGroup;

            if (gubEnemyEncounterCode == BLOODCAT_AMBUSH_CODE || gubEnemyEncounterCode == ENTERING_BLOODCAT_LAIR_CODE)
            {
                NotifyPlayerOfBloodcatBattle(pGroup.ubSectorX, pGroup.ubSectorY);
                return true;
            }

            if (!fCombatAbleMerc)
            { //Prepare for instant autoresolve.
                gfDelayAutoResolveStart = true;
                gfUsePersistantPBI = true;
                if (fMilitiaPresent)
                {
                    NotifyPlayerOfInvasionByEnemyForces(pGroup.ubSectorX, pGroup.ubSectorY, 0, TriggerPrebattleInterface);
                }
                else
                {
                    string str;
                    int[] pSectorStr = new int[128];
                    StrategicMap.GetSectorIDString(pGroup.ubSectorX, pGroup.ubSectorY, pGroup.ubSectorZ, pSectorStr, true);
                    wprintf(str, gpStrategicString[STR_DIALOG_ENEMIES_ATTACK_UNCONCIOUSMERCS], pSectorStr);
                    DoScreenIndependantMessageBox(str, MSG_BOX_FLAG_OK, TriggerPrebattleInterface);
                }
            }



            if (pPlayerDialogGroup)
            {
                PrepareForPreBattleInterface(pPlayerDialogGroup, pGroup);
            }
            return true;
        }
        return false;
    }

    void TriggerPrebattleInterface(int ubResult)
    {
        StopTimeCompression();
        SpecialCharacterDialogueEvent(DIALOGUE_SPECIAL_EVENT_TRIGGERPREBATTLEINTERFACE, (int)gpInitPrebattleGroup, 0, 0, 0, 0);
        gpInitPrebattleGroup = null;
    }


    void DeployGroupToSector(GROUP? pGroup)
    {
        Debug.Assert(pGroup);
        if (pGroup.fPlayer)
        {
            //Update the sector positions of the players...
            return;
        }
        //Assuming enemy code from here on...
    }

    //This will get called after a battle is auto-resolved or automatically after arriving
    //at the next sector during a move and the area is clear.
    void CalculateNextMoveIntention(GROUP? pGroup)
    {
        int i;
        List<WAYPOINT> wp = new();

        Debug.Assert(pGroup is not null);

        //TEMP:  Ignore resting...

        //Should be surely an enemy group that has just made a new decision to go elsewhere!
        if (pGroup.fBetweenSectors)
        {
            return;
        }

        if (!pGroup.pWaypoints)
        {
            return;
        }

        //If the waypoints have been cancelled, then stop moving.
        /*
        if( pGroup.fWaypointsCancelled )
        {
            DeployGroupToSector( pGroup );
            return;
        }
        */

        //Determine if we are at a waypoint.
        i = pGroup.ubNextWaypointID;
        wp = pGroup.pWaypoints;
        while (i-- > 0)
        { //Traverse through the waypoint list to the next waypoint ID
            Debug.Assert(wp);
            wp = wp.next;
        }
        Debug.Assert(wp);

        //We have the next waypoint, now check if we are actually there.
        if (pGroup.ubSectorX == wp.x && pGroup.ubSectorY == wp.y)
        { //We have reached the next waypoint, so now determine what the next waypoint is.
            switch (pGroup.ubMoveType)
            {
                case ONE_WAY:
                    if (!wp.next)
                    { //No more waypoints, so we've reached the destination.
                        DeployGroupToSector(pGroup);
                        return;
                    }
                    //Advance destination to next waypoint ID
                    pGroup.ubNextWaypointID++;
                    break;
                case CIRCULAR:
                    wp = wp.next;
                    if (!wp)
                    {   //reached the end of the patrol route.  Set to the first waypoint in list, indefinately.
                        //NOTE:  If the last waypoint isn't exclusively aligned to the x or y axis of the first 
                        //			 waypoint, there will be an assertion failure inside the waypoint movement code.
                        pGroup.ubNextWaypointID = 0;
                    }
                    else
                    {
                        pGroup.ubNextWaypointID++;
                    }

                    break;
                case ENDTOEND_FORWARDS:
                    wp = wp.next;
                    if (!wp)
                    {
                        //AssertMsg(pGroup.ubNextWaypointID, "EndToEnd patrol group needs more than one waypoint!");
                        pGroup.ubNextWaypointID--;
                        pGroup.ubMoveType = ENDTOEND_BACKWARDS;
                    }
                    else
                    {
                        pGroup.ubNextWaypointID++;
                    }

                    break;
                case ENDTOEND_BACKWARDS:
                    if (!pGroup.ubNextWaypointID)
                    {
                        pGroup.ubNextWaypointID++;
                        pGroup.ubMoveType = ENDTOEND_FORWARDS;
                    }
                    else
                    {
                        pGroup.ubNextWaypointID--;
                    }

                    break;
            }
        }
        InitiateGroupMovementToNextSector(pGroup);
    }

    bool AttemptToMergeSeparatedGroups(GROUP? pGroup, bool fDecrementTraversals)
    {
        GROUP? curr = null;
        SOLDIERTYPE? pSoldier = null, pCharacter = null;
        PLAYERGROUP? pPlayer = null;
        bool fSuccess = false;

        return false;
    }

    void AwardExperienceForTravelling(GROUP? pGroup)
    {
        // based on how long movement took, mercs gain a bit of life experience for travelling
        PLAYERGROUP? pPlayerGroup;
        SOLDIERTYPE? pSoldier;
        int uiPoints;
        int uiCarriedPercent;

        if (!pGroup || !pGroup.fPlayer)
        {
            return;
        }

        pPlayerGroup = pGroup.pPlayerList;
        while (pPlayerGroup)
        {
            pSoldier = pPlayerGroup.pSoldier;
            if (pSoldier && !AM_A_ROBOT(pSoldier) &&
                    !AM_AN_EPC(pSoldier) && !(pSoldier.uiStatusFlags & SOLDIER_VEHICLE))
            {
                if (pSoldier.bLifeMax < 100)
                {
                    // award exp...
                    // amount was originally based on getting 100-bLifeMax points for 12 hours of travel (720)
                    // but changed to flat rate since StatChange makes roll vs 100-lifemax as well!
                    uiPoints = pGroup.uiTraverseTime / (450 / 100 - pSoldier.bLifeMax);
                    if (uiPoints > 0)
                    {
                        Campaign.StatChange(pSoldier, Stat.HEALTHAMT, (int)uiPoints, 0);
                    }
                }

                if (pSoldier.bStrength < 100)
                {
                    uiCarriedPercent = CalculateCarriedWeight(pSoldier);
                    if (uiCarriedPercent > 50)
                    {
                        uiPoints = pGroup.uiTraverseTime / (450 / (100 - pSoldier.bStrength));
                        Campaign.StatChange(pSoldier, Stat.STRAMT, (int)(uiPoints * (uiCarriedPercent - 50) / 100), 0);
                    }
                }
            }
            pPlayerGroup = pPlayerGroup.next;
        }

    }

    void AddCorpsesToBloodcatLair(int sSectorX, MAP_ROW sSectorY)
    {
        int sXPos, sYPos;

        // Setup some values!
        ROTTING_CORPSE_DEFINITION Corpse = new()
        {
            ubBodyType = SoldierBodyTypes.REGMALE,
            sHeightAdjustment = 0,
            bVisible = 1
        };

        SET_PALETTEREP_ID(Corpse.HeadPal, "BROWNHEAD");
        SET_PALETTEREP_ID(Corpse.VestPal, "YELLOWVEST");
        SET_PALETTEREP_ID(Corpse.SkinPal, "PINKSKIN");
        SET_PALETTEREP_ID(Corpse.PantsPal, "GREENPANTS");


        Corpse.bDirection = (int)Globals.Random.Next(8);

        // Set time of death
        // Make sure they will be rotting!
        Corpse.uiTimeOfDeath = GameClock.GetWorldTotalMin() - (2 * NUM_SEC_IN_DAY / 60);
        // Set type
        Corpse.ubType = (int)SMERC_JFK;
        Corpse.usFlags = ROTTING_CORPSE_FIND_SWEETSPOT_FROM_GRIDNO;

        // 1st gridno
        Corpse.sGridNo = 14319;
        IsometricUtils.ConvertGridNoToXY(Corpse.sGridNo, out sXPos, out sYPos);
        Corpse.dXPos = (IsometricUtils.CenterX(sXPos));
        Corpse.dYPos = (IsometricUtils.CenterY(sYPos));

        //Add the rotting corpse info to the sectors unloaded rotting corpse file
        AddRottingCorpseToUnloadedSectorsRottingCorpseFile(sSectorX, sSectorY, 0, Corpse);


        // 2nd gridno
        Corpse.sGridNo = 9835;
        IsometricUtils.ConvertGridNoToXY(Corpse.sGridNo, out sXPos, out sYPos);
        Corpse.dXPos = (IsometricUtils.CenterX(sXPos));
        Corpse.dYPos = (IsometricUtils.CenterY(sYPos));

        //Add the rotting corpse info to the sectors unloaded rotting corpse file
        AddRottingCorpseToUnloadedSectorsRottingCorpseFile(sSectorX, sSectorY, 0, Corpse);


        // 3rd gridno
        Corpse.sGridNo = 11262;
        IsometricUtils.ConvertGridNoToXY(Corpse.sGridNo, out sXPos, out sYPos);
        Corpse.dXPos = (IsometricUtils.CenterX(sXPos));
        Corpse.dYPos = (IsometricUtils.CenterY(sYPos));

        //Add the rotting corpse info to the sectors unloaded rotting corpse file
        AddRottingCorpseToUnloadedSectorsRottingCorpseFile(sSectorX, sSectorY, 0, Corpse);

    }




    //ARRIVALCALLBACK
    //...............
    //This is called whenever any group arrives in the next sector (player or enemy)
    //This function will first check to see if a battle should start, or if they
    //aren't at the final destination, they will move to the next sector. 
    void GroupArrivedAtSector(int ubGroupID, bool fCheckForBattle, bool fNeverLeft)
    {
        GROUP? pGroup;
        int iVehId = -1;
        PLAYERGROUP? curr;
        WorldDirections ubInsertionDirection;
        INSERTION_CODE ubStrategicInsertionCode;
        SOLDIERTYPE? pSoldier = null;
        bool fExceptionQueue = false;
        bool fFirstTimeInSector = false;
        bool fGroupDestroyed = false;
        bool fVehicleStranded = false;

        // reset
        gfWaitingForInput = false;

        // grab the group and see if valid
        pGroup = GetGroup(ubGroupID);

        if (pGroup == null)
        {
            return;
        }

        if (pGroup.fPlayer)
        {
            //Set the fact we have visited the  sector
            curr = pGroup.pPlayerList;
            if (curr)
            {
                if (curr.pSoldier.bAssignment < Assignments.ON_DUTY)
                {
                    ResetDeadSquadMemberList(curr.pSoldier.bAssignment);
                }
            }



            while (curr)
            {
                curr.pSoldier.uiStatusFlags &= ~SOLDIER.SHOULD_BE_TACTICALLY_VALID;
                curr = curr.next;
            }

            if (pGroup.fVehicle)
            {
                if ((iVehId = (GivenMvtGroupIdFindVehicleId(ubGroupID))) != -1)
                {
                    if (iVehId != iHelicopterVehicleId)
                    {
                        if (pGroup.pPlayerList == null)
                        {
                            // nobody here, better just get out now
                            // with vehicles, arriving empty is probably ok, since passengers might have been killed but vehicle lived.
                            return;
                        }
                    }
                }
            }
            else
            {
                if (pGroup.pPlayerList == null)
                {
                    // nobody here, better just get out now
                    //AssertMsg(0, String("Player group %d arrived in sector empty.  KM 0", ubGroupID));
                    return;
                }
            }
        }
        //Check for exception cases which 
        if (gTacticalStatus.bBoxingState != BoxingStates.NOT_BOXING)
        {
            if (!pGroup.fPlayer && pGroup.ubNextX == 5 && pGroup.ubNextY == (MAP_ROW)4 && pGroup.ubSectorZ == 0)
            {
                fExceptionQueue = true;
            }
        }
        //First check if the group arriving is going to queue another battle.
        //NOTE:  We can't have more than one battle ongoing at a time.
        if (fExceptionQueue
            || fCheckForBattle
            && gTacticalStatus.fEnemyInSector
            && FindMovementGroupInSector(gWorldSectorX, gWorldSectorY, true) is not null
            && (pGroup.ubNextX != gWorldSectorX || pGroup.ubNextY != gWorldSectorY || gbWorldSectorZ > 0)
            || AreInMeanwhile()
            ||
                //KM : Aug 11, 1999 -- Patch fix:  Added additional checks to prevent a 2nd battle in the case
                //     where the player is involved in a potential battle with bloodcats/civilians
                fCheckForBattle && HostileCiviliansPresent()
                || fCheckForBattle && HostileBloodcatsPresent()
            )
        {
            //QUEUE BATTLE!
            //Delay arrival by a random value ranging from 3-5 minutes, so it doesn't get the player
            //too suspicious after it happens to him a few times, which, by the way, is a rare occurrence.
            if (AreInMeanwhile())
            {
                pGroup.uiArrivalTime++; //tack on only 1 minute if we are in a meanwhile scene.  This effectively
                                        //prevents any battle from occurring while inside a meanwhile scene.
            }
            else
            {
                pGroup.uiArrivalTime += (uint)Globals.Random.Next(3) + 3;
            }


            if (!GameEvents.AddStrategicEvent(EVENT.GROUP_ARRIVAL, pGroup.uiArrivalTime, pGroup.ubGroupID))
            {
                //AssertMsg(0, "Failed to add movement event.");

                if (pGroup.fPlayer)
                {
                    if (pGroup.uiArrivalTime - ABOUT_TO_ARRIVE_DELAY > GameClock.GetWorldTotalMin())
                    {
                        GameEvents.AddStrategicEvent(EVENT.GROUP_ABOUT_TO_ARRIVE, pGroup.uiArrivalTime - ABOUT_TO_ARRIVE_DELAY, pGroup.ubGroupID);
                    }
                }
            }

            return;
        }


        //Update the position of the group
        pGroup.ubPrevX = pGroup.ubSectorX;
        pGroup.ubPrevY = pGroup.ubSectorY;
        pGroup.ubSectorX = pGroup.ubNextX;
        pGroup.ubSectorY = pGroup.ubNextY;
        pGroup.ubNextX = 0;
        pGroup.ubNextY = 0;


        if (pGroup.fPlayer)
        {
            if (pGroup.ubSectorZ == 0)
            {
                SectorInfo[SECTORINFO.SECTOR(pGroup.ubSectorX, pGroup.ubSectorY)].bLastKnownEnemies = QueenCommand.NumEnemiesInSector(pGroup.ubSectorX, pGroup.ubSectorY);
            }

            // award life 'experience' for travelling, based on travel time!
            if (!pGroup.fVehicle)
            {
                // gotta be walking to get tougher
                AwardExperienceForTravelling(pGroup);
            }
            else if (!IsGroupTheHelicopterGroup(pGroup))
            {
                SOLDIERTYPE? pSoldier;
                int iVehicleID;
                iVehicleID = GivenMvtGroupIdFindVehicleId(pGroup.ubGroupID);
                //AssertMsg(iVehicleID != -1, "GroupArrival for vehicle group.  Invalid iVehicleID. ");

                pSoldier = GetSoldierStructureForVehicle(iVehicleID);
                //AssertMsg(pSoldier, "GroupArrival for vehicle group.  Invalid soldier pointer.");

                SpendVehicleFuel(pSoldier, (int)(pGroup.uiTraverseTime * 6));

                if (VehicleFuelRemaining(pSoldier) == 0)
                {
                    ReportVehicleOutOfGas(iVehicleID, pGroup.ubSectorX, pGroup.ubSectorY);
                    //Nuke the group's path, so they don't continue moving.
                    ClearMercPathsAndWaypointsForAllInGroup(pGroup);
                }
            }
        }

        pGroup.uiTraverseTime = 0;
        SetGroupArrivalTime(pGroup, 0);
        pGroup.fBetweenSectors = false;

        fMapPanelDirty = true;
        fMapScreenBottomDirty = true;


        // if a player group
        if (pGroup.fPlayer)
        {
            // if this is the last sector along player group's movement path (no more waypoints)
            if (GroupAtFinalDestination(pGroup))
            {
                // clear their strategic movement (mercpaths and waypoints)
                ClearMercPathsAndWaypointsForAllInGroup(pGroup);
            }

            // if on surface
            if (pGroup.ubSectorZ == 0)
            {
                // check for discovering secret locations
                TOWNS bTownId = StrategicMap.GetTownIdForSector(pGroup.ubSectorX, pGroup.ubSectorY);

                if (bTownId == TOWNS.TIXA)
                {
                    SetTixaAsFound();
                }
                else if (bTownId == TOWNS.ORTA)
                {
                    SetOrtaAsFound();
                }
                else if (IsThisSectorASAMSector(pGroup.ubSectorX, pGroup.ubSectorY, 0))
                {
                    SetSAMSiteAsFound(GetSAMIdFromSector(pGroup.ubSectorX, pGroup.ubSectorY, 0));
                }
            }


            if (pGroup.ubSectorX < pGroup.ubPrevX)
            {
                ubInsertionDirection = WorldDirections.SOUTHWEST;
                ubStrategicInsertionCode = INSERTION_CODE.EAST;
            }
            else if (pGroup.ubSectorX > pGroup.ubPrevX)
            {
                ubInsertionDirection = WorldDirections.NORTHEAST;
                ubStrategicInsertionCode = INSERTION_CODE.WEST;
            }
            else if (pGroup.ubSectorY < pGroup.ubPrevY)
            {
                ubInsertionDirection = WorldDirections.NORTHWEST;
                ubStrategicInsertionCode = INSERTION_CODE.SOUTH;
            }
            else if (pGroup.ubSectorY > pGroup.ubPrevY)
            {
                ubInsertionDirection = WorldDirections.SOUTHEAST;
                ubStrategicInsertionCode = INSERTION_CODE.NORTH;
            }
            else
            {
                Debug.Assert(false);
                return;
            }


            if (pGroup.fVehicle == false)
            {
                // non-vehicle player group

                curr = pGroup.pPlayerList;
                while (curr)
                {
                    curr.pSoldier.fBetweenSectors = false;
                    curr.pSoldier.sSectorX = pGroup.ubSectorX;
                    curr.pSoldier.sSectorY = pGroup.ubSectorY;
                    curr.pSoldier.bSectorZ = pGroup.ubSectorZ;
                    curr.pSoldier.ubPrevSectorID = SECTORINFO.SECTOR(pGroup.ubPrevX, pGroup.ubPrevY);
                    curr.pSoldier.ubInsertionDirection = ubInsertionDirection;

                    // don't override if a tactical traversal
                    if (curr.pSoldier.ubStrategicInsertionCode != INSERTION_CODE.PRIMARY_EDGEINDEX &&
                            curr.pSoldier.ubStrategicInsertionCode != INSERTION_CODE.SECONDARY_EDGEINDEX)
                    {
                        curr.pSoldier.ubStrategicInsertionCode = ubStrategicInsertionCode;
                    }

                    if (curr.pSoldier.pMercPath)
                    {
                        // remove head from their mapscreen path list
                        curr.pSoldier.pMercPath = RemoveHeadFromStrategicPath(curr.pSoldier.pMercPath);
                    }

                    // ATE: Alrighty, check if this sector is currently loaded, if so, 
                    // add them to the tactical engine!
                    if (pGroup.ubSectorX == gWorldSectorX && pGroup.ubSectorY == gWorldSectorY && pGroup.ubSectorZ == gbWorldSectorZ)
                    {
                        UpdateMercInSector(curr.pSoldier, gWorldSectorX, gWorldSectorY, gbWorldSectorZ);
                    }
                    curr = curr.next;
                }

                // if there's anybody in the group
                if (pGroup.pPlayerList)
                {
                    // don't print any messages when arriving underground (there's no delay involved) or if we never left (cancel)
                    if (GroupAtFinalDestination(pGroup) && (pGroup.ubSectorZ == 0) && !fNeverLeft)
                    {
                        // if assigned to a squad
                        if (pGroup.pPlayerList.pSoldier.bAssignment < Assignments.ON_DUTY)
                        {
                            // squad
                            Messages.ScreenMsg(FontColor.FONT_MCOLOR_DKRED, MSG_INTERFACE, pMessageStrings[MSG_ARRIVE], pAssignmentStrings[pGroup.pPlayerList.pSoldier.bAssignment], pMapVertIndex[pGroup.pPlayerList.pSoldier.sSectorY], pMapHortIndex[pGroup.pPlayerList.pSoldier.sSectorX]);
                        }
                        else
                        {
                            // a loner
                            Messages.ScreenMsg(FontColor.FONT_MCOLOR_DKRED, MSG_INTERFACE, pMessageStrings[MSG_ARRIVE], pGroup.pPlayerList.pSoldier.name, pMapVertIndex[pGroup.pPlayerList.pSoldier.sSectorY], pMapHortIndex[pGroup.pPlayerList.pSoldier.sSectorX]);
                        }
                    }
                }
            }
            else    // vehicle player group
            {
                iVehId = GivenMvtGroupIdFindVehicleId(ubGroupID);
                Debug.Assert(iVehId != -1);

                if (pVehicleList[iVehId].pMercPath)
                {
                    // remove head from vehicle's mapscreen path list
                    pVehicleList[iVehId].pMercPath = RemoveHeadFromStrategicPath(pVehicleList[iVehId].pMercPath);
                }

                // update vehicle position
                SetVehicleSectorValues(iVehId, pGroup.ubSectorX, pGroup.ubSectorY);
                pVehicleList[iVehId].fBetweenSectors = false;

                // update passengers position
                UpdatePositionOfMercsInVehicle(iVehId);


                if (iVehId != iHelicopterVehicleId)
                {
                    pSoldier = GetSoldierStructureForVehicle(iVehId);
                    Debug.Assert(pSoldier is not null);

                    pSoldier.fBetweenSectors = false;
                    pSoldier.sSectorX = pGroup.ubSectorX;
                    pSoldier.sSectorY = pGroup.ubSectorY;
                    pSoldier.bSectorZ = pGroup.ubSectorZ;
                    pSoldier.ubInsertionDirection = ubInsertionDirection;

                    // ATE: Removed, may 21 - sufficient to use insertion direction...
                    //pSoldier.bDesiredDirection = ubInsertionDirection;

                    pSoldier.ubStrategicInsertionCode = ubStrategicInsertionCode;

                    // if this sector is currently loaded
                    if (pGroup.ubSectorX == gWorldSectorX && pGroup.ubSectorY == gWorldSectorY && pGroup.ubSectorZ == gbWorldSectorZ)
                    {
                        // add vehicle to the tactical engine!
                        UpdateMercInSector(pSoldier, gWorldSectorX, gWorldSectorY, gbWorldSectorZ);
                    }



                    // set directions of insertion
                    curr = pGroup.pPlayerList;
                    while (curr)
                    {
                        curr.pSoldier.fBetweenSectors = false;
                        curr.pSoldier.sSectorX = pGroup.ubSectorX;
                        curr.pSoldier.sSectorY = pGroup.ubSectorY;
                        curr.pSoldier.bSectorZ = pGroup.ubSectorZ;
                        curr.pSoldier.ubInsertionDirection = ubInsertionDirection;

                        // ATE: Removed, may 21 - sufficient to use insertion direction...
                        // curr.pSoldier.bDesiredDirection = ubInsertionDirection;

                        curr.pSoldier.ubStrategicInsertionCode = ubStrategicInsertionCode;

                        // if this sector is currently loaded
                        if (pGroup.ubSectorX == gWorldSectorX && pGroup.ubSectorY == gWorldSectorY && pGroup.ubSectorZ == gbWorldSectorZ)
                        {
                            // add passenger to the tactical engine!
                            UpdateMercInSector(curr.pSoldier, gWorldSectorX, gWorldSectorY, gbWorldSectorZ);
                        }

                        curr = curr.next;
                    }
                }
                else
                {
                    if (HandleHeliEnteringSector(pVehicleList[iVehId].sSectorX, pVehicleList[iVehId].sSectorY) == true)
                    {
                        // helicopter destroyed
                        fGroupDestroyed = true;
                    }
                }


                if (!fGroupDestroyed)
                {
                    // don't print any messages when arriving underground, there's no delay involved
                    if (GroupAtFinalDestination(pGroup) && (pGroup.ubSectorZ == 0) && !fNeverLeft)
                    {
                        Messages.ScreenMsg(FontColor.FONT_MCOLOR_DKRED, MSG_INTERFACE, pMessageStrings[MSG.ARRIVE], pVehicleStrings[pVehicleList[iVehId].ubVehicleType], pMapVertIndex[pGroup.ubSectorY], pMapHortIndex[pGroup.ubSectorX]);
                    }
                }
            }


            if (!fGroupDestroyed)
            {
                // check if sector had been visited previously
                fFirstTimeInSector = !GetSectorFlagStatus(pGroup.ubSectorX, pGroup.ubSectorY, pGroup.ubSectorZ, SF.ALREADY_VISITED);

                // on foot, or in a vehicle other than the chopper
                if (!pGroup.fVehicle || !IsGroupTheHelicopterGroup(pGroup))
                {

                    // ATE: Add a few corpse to the bloodcat lair...
                    if (SECTORINFO.SECTOR(pGroup.ubSectorX, pGroup.ubSectorY) == SEC.I16 && fFirstTimeInSector)
                    {
                        AddCorpsesToBloodcatLair(pGroup.ubSectorX, pGroup.ubSectorY);
                    }

                    // mark the sector as visited already
                    SetSectorFlag(pGroup.ubSectorX, pGroup.ubSectorY, pGroup.ubSectorZ, SF.ALREADY_VISITED);
                }
            }

            // update character info
            fTeamPanelDirty = true;
            fCharacterInfoPanelDirty = true;
        }

        if (!fGroupDestroyed)
        {
            //Determine if a battle should start.
            //if a battle does start, or get's delayed, then we will keep the group in memory including
            //all waypoints, until after the battle is resolved.  At that point, we will continue the processing.
            if (fCheckForBattle && !CheckConditionsForBattle(pGroup) && !gfWaitingForInput)
            {
                GROUP? next;
                HandleNonCombatGroupArrival(pGroup, true, fNeverLeft);

                if (gubNumGroupsArrivedSimultaneously)
                {
                    pGroup = gpGroupList;
                    while (gubNumGroupsArrivedSimultaneously && pGroup)
                    {
                        next = pGroup.next;
                        if (pGroup.uiFlags & GROUPFLAG_GROUP_ARRIVED_SIMULTANEOUSLY)
                        {
                            gubNumGroupsArrivedSimultaneously--;
                            HandleNonCombatGroupArrival(pGroup, false, false);
                        }
                        pGroup = next;
                    }
                }
            }
            else
            { //Handle cases for pre battle conditions
                pGroup.uiFlags = 0;
                if (gubNumAwareBattles)
                { //When the AI is looking for the players, and a battle is initiated, then 
                  //decrement the value, otherwise the queen will continue searching to infinity.
                    gubNumAwareBattles--;
                }
            }
        }
        gfWaitingForInput = false;
    }




    void HandleNonCombatGroupArrival(GROUP? pGroup, bool fMainGroup, bool fNeverLeft)
    {
        // if any mercs are actually in the group

        if (StrategicAILookForAdjacentGroups(pGroup))
        { //The routine actually just deleted the enemy group (player's don't get deleted), so we are done!
            return;
        }

        if (pGroup.fPlayer)
        {
            //The group will always exist after the AI was processed.

            //Determine if the group should rest, change routes, or continue moving.
            // if on foot, or in a vehicle other than the helicopter
            if (!pGroup.fVehicle || !IsGroupTheHelicopterGroup(pGroup))
            {
                // take control of sector
                SetThisSectorAsPlayerControlled(pGroup.ubSectorX, pGroup.ubSectorY, pGroup.ubSectorZ, false);
            }

            // if this is the last sector along their movement path (no more waypoints)
            if (GroupAtFinalDestination(pGroup))
            {
                // if currently selected sector has nobody in it
                if (PlayerMercsInSector((int)sSelMapX, (int)sSelMapY, (int)iCurrentMapSectorZ) == 0)
                {
                    // make this sector strategically selected
                    ChangeSelectedMapSector(pGroup.ubSectorX, pGroup.ubSectorY, pGroup.ubSectorZ);
                }

                // if on foot or in a vehicle other than the helicopter (Skyrider speaks for heli movement)
                if (!pGroup.fVehicle || !IsGroupTheHelicopterGroup(pGroup))
                {
                    StopTimeCompression();

                    // if traversing tactically, or we never left (just canceling), don't do this
                    if (!gfTacticalTraversal && !fNeverLeft)
                    {
                        RandomMercInGroupSaysQuote(pGroup, QUOTE_MERC_REACHED_DESTINATION);
                    }
                }
            }
            // look for NPCs to stop for, anyone is too tired to keep going, if all OK rebuild waypoints & continue movement
            // NOTE: Only the main group (first group arriving) will stop for NPCs, it's just too much hassle to stop them all
            PlayerGroupArrivedSafelyInSector(pGroup, fMainGroup);
        }
        else
        {
            if (!pGroup.fDebugGroup)
            {
                CalculateNextMoveIntention(pGroup);
            }
            else
            {
                RemovePGroup(pGroup);
            }
        }
        //Clear the non-persistant flags.
        pGroup.uiFlags = 0;
    }



    //Because a battle is about to start, we need to go through the event list and look for other
    //groups that may arrive at the same time -- enemies or players, and blindly add them to the sector
    //without checking for battle conditions, as it has already determined that a new battle is about to 
    //start.
    void HandleOtherGroupsArrivingSimultaneously(int ubSectorX, MAP_ROW ubSectorY, int ubSectorZ)
    {
        STRATEGICEVENT? pEvent;
        uint uiCurrTimeStamp;
        GROUP? pGroup;
        uiCurrTimeStamp = GameClock.GetWorldTotalSeconds();
        pEvent = gpEventList;
        gubNumGroupsArrivedSimultaneously = 0;
        while (pEvent && pEvent.uiTimeStamp <= uiCurrTimeStamp)
        {
            if (pEvent.ubCallbackID == EVENT_GROUP_ARRIVAL && !(pEvent.ubFlags.HasFlag(SEF.DELETION_PENDING)))
            {
                pGroup = GetGroup((int)pEvent.uiParam);
                Debug.Assert(pGroup);
                if (pGroup.ubNextX == ubSectorX && pGroup.ubNextY == ubSectorY && pGroup.ubSectorZ == ubSectorZ)
                {
                    if (pGroup.fBetweenSectors)
                    {
                        GroupArrivedAtSector((int)pEvent.uiParam, false, false);
                        pGroup.uiFlags |= GROUPFLAG_GROUP_ARRIVED_SIMULTANEOUSLY;
                        gubNumGroupsArrivedSimultaneously++;
                        GameEvents.DeleteStrategicEvent(EVENT.GROUP_ARRIVAL, pGroup.ubGroupID);
                        pEvent = gpEventList;
                        continue;
                    }
                }
            }
            pEvent = pEvent.next;
        }
    }

    //The user has just approved to plan a simultaneous arrival.  So we will syncronize all of the involved
    //groups so that they arrive at the same time (which is the time the final group would arrive).  
    void PrepareGroupsForSimultaneousArrival()
    {
        GROUP? pGroup;
        uint uiLatestArrivalTime = 0;
        SOLDIERTYPE? pSoldier = null;
        int iVehId = 0;

        pGroup = gpGroupList;
        while (pGroup)
        { //For all of the groups that haven't arrived yet, determine which one is going to take the longest.
            if (pGroup != gpPendingSimultaneousGroup
                  && pGroup.fPlayer
                    && pGroup.fBetweenSectors
                    && pGroup.ubNextX == gpPendingSimultaneousGroup.ubSectorX
                    && pGroup.ubNextY == gpPendingSimultaneousGroup.ubSectorY &&
                    !IsGroupTheHelicopterGroup(pGroup))
            {
                uiLatestArrivalTime = Math.Max(pGroup.uiArrivalTime, uiLatestArrivalTime);
                pGroup.uiFlags |= GROUPFLAG_SIMULTANEOUSARRIVAL_APPROVED | GROUPFLAG_MARKER;
            }
            pGroup = pGroup.next;
        }
        //Now, go through the list again, and reset their arrival event to the latest arrival time.
        pGroup = gpGroupList;
        while (pGroup)
        {
            if (pGroup.uiFlags & GROUPFLAG_MARKER)
            {
                GameEvents.DeleteStrategicEvent(EVENT.GROUP_ARRIVAL, pGroup.ubGroupID);

                // NOTE: This can cause the arrival time to be > GetWorldTotalMin() + TraverseTime, so keep that in mind
                // if you have any code that uses these 3 values to figure out how far along its route a group is!
                SetGroupArrivalTime(pGroup, uiLatestArrivalTime);
                GameEvents.AddStrategicEvent(EVENT.GROUP_ARRIVAL, pGroup.uiArrivalTime, pGroup.ubGroupID);

                if (pGroup.fPlayer)
                {
                    if (pGroup.uiArrivalTime - ABOUT_TO_ARRIVE_DELAY > GameClock.GetWorldTotalMin())
                    {
                        GameEvents.AddStrategicEvent(EVENT.GROUP_ABOUT_TO_ARRIVE, pGroup.uiArrivalTime - ABOUT_TO_ARRIVE_DELAY, pGroup.ubGroupID);
                    }
                }

                DelayEnemyGroupsIfPathsCross(pGroup);

                pGroup.uiFlags &= ~GROUPFLAG_MARKER;
            }
            pGroup = pGroup.next;
        }
        //We still have the first group that has arrived.  Because they are set up to be in the destination
        //sector, we will "warp" them back to the last sector, and also setup a new arrival time for them.
        pGroup = gpPendingSimultaneousGroup;
        pGroup.ubNextX = pGroup.ubSectorX;
        pGroup.ubNextY = pGroup.ubSectorY;
        pGroup.ubSectorX = pGroup.ubPrevX;
        pGroup.ubSectorY = pGroup.ubPrevY;
        SetGroupArrivalTime(pGroup, uiLatestArrivalTime);
        pGroup.fBetweenSectors = true;

        if (pGroup.fVehicle)
        {
            if ((iVehId = (GivenMvtGroupIdFindVehicleId(pGroup.ubGroupID))) != -1)
            {
                pVehicleList[iVehId].fBetweenSectors = true;

                // set up vehicle soldier
                pSoldier = GetSoldierStructureForVehicle(iVehId);

                if (pSoldier)
                {
                    pSoldier.fBetweenSectors = true;
                }
            }
        }

        GameEvents.AddStrategicEvent(EVENT.GROUP_ARRIVAL, pGroup.uiArrivalTime, pGroup.ubGroupID);

        if (pGroup.fPlayer)
        {
            if (pGroup.uiArrivalTime - ABOUT_TO_ARRIVE_DELAY > GameClock.GetWorldTotalMin())
            {
                GameEvents.AddStrategicEvent(EVENT.GROUP_ABOUT_TO_ARRIVE, pGroup.uiArrivalTime - ABOUT_TO_ARRIVE_DELAY, pGroup.ubGroupID);
            }
        }
        DelayEnemyGroupsIfPathsCross(pGroup);
    }

    //See if there are other groups OTW.  If so, and if we haven't asked the user yet to plan
    //a simultaneous attack, do so now, and readjust the groups accordingly.  If it is possible
    //to do so, then we will set up the gui, and postpone the prebattle interface.
    bool PossibleToCoordinateSimultaneousGroupArrivals(GROUP? pFirstGroup)
    {
        GROUP? pGroup;
        int ubNumNearbyGroups = 0;

        //If the user has already been asked, then don't ask the question again!
        if (pFirstGroup.uiFlags & (GROUPFLAG_SIMULTANEOUSARRIVAL_APPROVED | GROUPFLAG_SIMULTANEOUSARRIVAL_CHECKED) ||
            IsGroupTheHelicopterGroup(pFirstGroup))
        {
            return false;
        }

        //We can't coordinate simultaneous attacks on a sector without any stationary forces!  Otherwise, it
        //is possible that they will be gone when you finally arrive.
        //if( !NumStationaryEnemiesInSector( pFirstGroup.ubSectorX, pFirstGroup.ubSectorY ) )
        //	return false;

        //Count the number of groups that are scheduled to arrive in the same sector and are currently
        //adjacent to the sector in question.
        pGroup = gpGroupList;
        while (pGroup)
        {
            if (pGroup != pFirstGroup && pGroup.fPlayer && pGroup.fBetweenSectors &&
                  pGroup.ubNextX == pFirstGroup.ubSectorX && pGroup.ubNextY == pFirstGroup.ubSectorY &&
                    !(pGroup.uiFlags & GROUPFLAG_SIMULTANEOUSARRIVAL_CHECKED) &&
                    !IsGroupTheHelicopterGroup(pGroup))
            {
                pGroup.uiFlags |= GROUPFLAG_SIMULTANEOUSARRIVAL_CHECKED;
                ubNumNearbyGroups++;
            }
            pGroup = pGroup.next;
        }

        if (ubNumNearbyGroups)
        { //postpone the battle until the user answers the dialog.
            string str;
            int? pStr, pEnemyType;
            GameClock.InterruptTime();
            GameClock.PauseGame();
            GameClock.LockPauseState(13);
            gpPendingSimultaneousGroup = pFirstGroup;
            //Build the string
            if (ubNumNearbyGroups == 1)
            {
                pStr = gpStrategicString[STR_DETECTED_SINGULAR];
            }
            else
            {
                pStr = gpStrategicString[STR_DETECTED_PLURAL];
            }
            if (gubEnemyEncounterCode == ENTERING_BLOODCAT_LAIR_CODE)
            {
                pEnemyType = gpStrategicString[STR_PB_BLOODCATS];
            }
            else
            {
                pEnemyType = gpStrategicString[STR_PB_ENEMIES];
            }
            //header, sector, singular/plural str, confirmation string.
            //Ex:  Enemies have been detected in sector J9 and another squad is 
            //     about to arrive.  Do you wish to coordinate a simultaneous arrival?
            wprintf(str, pStr,
                pEnemyType, //Enemy type (Enemies or bloodcats)
                'A' + gpPendingSimultaneousGroup.ubSectorY - 1, gpPendingSimultaneousGroup.ubSectorX); //Sector location
            wcscat(str, "  ");
            wcscat(str, gpStrategicString[STR_COORDINATE]);
            //Setup the dialog

            //Kris August 03, 1999 Bug fix:  Changed 1st line to 2nd line to fix game breaking if this dialog came up while in tactical.
            //                               It would kick you to mapscreen, where things would break...
            //DoMapMessageBox( MSG_BOX_BASIC_STYLE, str, MAP_SCREEN, MSG_BOX_FLAG_YESNO, PlanSimultaneousGroupArrivalCallback );
            DoMapMessageBox(MSG_BOX_BASIC_STYLE, str, guiCurrentScreen, MSG_BOX_FLAG_YESNO, PlanSimultaneousGroupArrivalCallback);

            gfWaitingForInput = true;
            return true;
        }
        return false;
    }

    void PlanSimultaneousGroupArrivalCallback(int bMessageValue)
    {
        if (bMessageValue == MSG_BOX_RETURN_YES)
        {
            PrepareGroupsForSimultaneousArrival();
        }
        else
        {
            PrepareForPreBattleInterface(gpPendingSimultaneousGroup, gpPendingSimultaneousGroup);
        }
        UnLockPauseState();
        UnPauseGame();
    }

    void DelayEnemyGroupsIfPathsCross(GROUP? pPlayerGroup)
    {
        GROUP? pGroup;
        pGroup = gpGroupList;
        while (pGroup)
        {
            if (!pGroup.fPlayer)
            { //then check to see if this group will arrive in next sector before the player group.
                if (pGroup.uiArrivalTime < pPlayerGroup.uiArrivalTime)
                { //check to see if enemy group will cross paths with player group.
                    if (pGroup.ubNextX == pPlayerGroup.ubSectorX &&
                            pGroup.ubNextY == pPlayerGroup.ubSectorY &&
                            pGroup.ubSectorX == pPlayerGroup.ubNextX &&
                            pGroup.ubSectorY == pPlayerGroup.ubNextY)
                    { //Okay, the enemy group will cross paths with the player, so find and delete the arrival event
                      //and repost it in the future (like a minute or so after the player arrives)
                        DeleteStrategicEvent(EVENT_GROUP_ARRIVAL, pGroup.ubGroupID);

                        // NOTE: This can cause the arrival time to be > GetWorldTotalMin() + TraverseTime, so keep that in mind
                        // if you have any code that uses these 3 values to figure out how far along its route a group is!
                        SetGroupArrivalTime(pGroup, pPlayerGroup.uiArrivalTime + 1 + Globals.Random.Next(10));
                        if (!GameEvents.AddStrategicEvent(EVENT.GROUP_ARRIVAL, pGroup.uiArrivalTime, pGroup.ubGroupID))
                        {
                            //AssertMsg(0, "Failed to add movement event.");
                        }
                    }
                }
            }
            pGroup = pGroup.next;
        }
    }


    void InitiateGroupMovementToNextSector(GROUP? pGroup)
    {
        int dx, dy;
        int i;
        STRATEGIC_MOVE ubDirection;
        SEC ubSector;
        WAYPOINT? wp;
        int iVehId = -1;
        SOLDIERTYPE? pSoldier = null;
        int uiSleepMinutes = 0;


        Debug.Assert(pGroup);
        i = pGroup.ubNextWaypointID;
        wp = pGroup.pWaypoints;
        while (i-- > 0)
        { //Traverse through the waypoint list to the next waypoint ID
            Debug.Assert(wp);
            wp = wp.next;
        }
        Debug.Assert(wp);
        //We now have the correct waypoint.
        //Analyse the group and determine which direction it will move from the current sector.
        dx = wp.x - pGroup.ubSectorX;
        dy = wp.y - pGroup.ubSectorY;
        if (dx && dy)
        { //Can't move diagonally!
          //AssertMsg(0, String("Attempting to move to waypoint in a diagonal direction from sector %d,%d to sector %d,%d",
          //   pGroup.ubSectorX, pGroup.ubSectorY, wp.x, wp.y));
        }
        if (!dx && !dy) //Can't move to position currently at!
        {
            //AssertMsg(0, String("Attempting to move to waypoint %d, %d that you are already at!", wp.x, wp.y));
        }

        //Clip dx/dy value so that the move is for only one sector.
        if (dx >= 1)
        {
            ubDirection = STRATEGIC_MOVE.EAST_STRATEGIC_MOVE;
            dx = 1;
        }
        else if (dy >= 1)
        {
            ubDirection = STRATEGIC_MOVE.SOUTH_STRATEGIC_MOVE;
            dy = 1;
        }
        else if (dx <= -1)
        {
            ubDirection = STRATEGIC_MOVE.WEST_STRATEGIC_MOVE;
            dx = -1;
        }
        else if (dy <= -1)
        {
            ubDirection = STRATEGIC_MOVE.NORTH_STRATEGIC_MOVE;
            dy = -1;
        }
        else
        {
            Debug.Assert(false);
            return;
        }
        //All conditions for moving to the next waypoint are now good.
        pGroup.ubNextX = (int)(dx + pGroup.ubSectorX);
        pGroup.ubNextY = (dy + pGroup.ubSectorY);
        //Calc time to get to next waypoint...
        ubSector = SECTORINFO.SECTOR(pGroup.ubSectorX, pGroup.ubSectorY);
        if (!pGroup.ubSectorZ)
        {
            bool fCalcRegularTime = true;
            if (!pGroup.fPlayer)
            { //Determine if the enemy group is "sleeping".  If so, then simply delay their arrival time by the amount of time
              //they are going to be sleeping for.
                if (GetWorldHour() >= 21 || GetWorldHour() <= 4)
                { //It is definitely night time.
                    if (Chance(67))
                    { //2 in 3 chance of going to sleep.
                        pGroup.uiTraverseTime = GetSectorMvtTimeForGroup(ubSector, ubDirection, pGroup);
                        uiSleepMinutes = 360 + Globals.Random.Next(121); //6-8 hours sleep 
                        fCalcRegularTime = false;
                    }
                }
            }
            if (fCalcRegularTime)
            {
                pGroup.uiTraverseTime = GetSectorMvtTimeForGroup(ubSector, ubDirection, pGroup);
            }
        }
        else
        {
            pGroup.uiTraverseTime = 1;
        }

        if (pGroup.uiTraverseTime == 0xffffffff)
        {
            //AssertMsg(0, String("Group %d (%s) attempting illegal move from %c%d to %c%d (%s).",
            //pGroup.ubGroupID, (pGroup.fPlayer) ? "Player" : "AI",
            //        pGroup.ubSectorY + 'A', pGroup.ubSectorX, pGroup.ubNextY + 'A', pGroup.ubNextX,
            //        gszTerrain[SectorInfo[ubSector].ubTraversability[ubDirection]]));
        }

        // add sleep, if any
        pGroup.uiTraverseTime += uiSleepMinutes;

        if (gfTacticalTraversal && gpTacticalTraversalGroup == pGroup)
        {
            if (gfUndergroundTacticalTraversal)
            {   //underground movement between sectors takes 1 minute.
                pGroup.uiTraverseTime = 1;
            }
            else
            { //strategic movement between town sectors takes 5 minutes.
                pGroup.uiTraverseTime = 5;
            }
        }

        // if group isn't already between sectors
        if (!pGroup.fBetweenSectors)
        {
            // put group between sectors
            pGroup.fBetweenSectors = true;
            // and set it's arrival time
            SetGroupArrivalTime(pGroup, GameClock.GetWorldTotalMin() + pGroup.uiTraverseTime);
        }
        // NOTE: if the group is already between sectors, DON'T MESS WITH ITS ARRIVAL TIME!  THAT'S NOT OUR JOB HERE!!!


        // special override for AI patrol initialization only
        if (gfRandomizingPatrolGroup)
        { //We're initializing the patrol group, so randomize the enemy groups to have extremely quick and varying
          //arrival times so that their initial positions aren't easily determined.
            pGroup.uiTraverseTime = 1 + Globals.Random.Next(pGroup.uiTraverseTime - 1);
            SetGroupArrivalTime(pGroup, GameClock.GetWorldTotalMin() + pGroup.uiTraverseTime);
        }


        if (pGroup.fVehicle == true)
        {
            // vehicle, set fact it is between sectors too
            if ((iVehId = (GivenMvtGroupIdFindVehicleId(pGroup.ubGroupID))) != -1)
            {
                pVehicleList[iVehId].fBetweenSectors = true;
                pSoldier = GetSoldierStructureForVehicle(iVehId);

                if (pSoldier)
                {
                    pSoldier.fBetweenSectors = true;

                    // OK, Remove the guy from tactical engine!
                    RemoveSoldierFromTacticalSector(pSoldier, true);

                }
            }
        }

        //Post the event!
        if (!GameEvents.AddStrategicEvent(EVENT.GROUP_ARRIVAL, pGroup.uiArrivalTime, pGroup.ubGroupID))
        {
            //AssertMsg(0, "Failed to add movement event.");

            //For the case of player groups, we need to update the information of the soldiers.
            if (pGroup.fPlayer)
            {
                PLAYERGROUP? curr;

                if (pGroup.uiArrivalTime - ABOUT_TO_ARRIVE_DELAY > GameClock.GetWorldTotalMin())
                {
                    GameEvents.AddStrategicEvent(EVENT.GROUP_ABOUT_TO_ARRIVE, pGroup.uiArrivalTime - ABOUT_TO_ARRIVE_DELAY, pGroup.ubGroupID);
                }

                curr = pGroup.pPlayerList;
                while (curr)
                {
                    curr.pSoldier.fBetweenSectors = true;

                    // OK, Remove the guy from tactical engine!
                    RemoveSoldierFromTacticalSector(curr.pSoldier, true);

                    curr = curr.next;
                }
                CheckAndHandleUnloadingOfCurrentWorld();

                //If an enemy group will be crossing paths with the player group, delay the enemy group's arrival time so that
                //the player will always encounter that group.
                if (!pGroup.ubSectorZ)
                {
                    DelayEnemyGroupsIfPathsCross(pGroup);
                }
            }
        }
    }

    void RemoveGroupWaypoints(int ubGroupID)
    {
        GROUP? pGroup;
        pGroup = GetGroup(ubGroupID);
        Debug.Assert(pGroup);
        RemovePGroupWaypoints(pGroup);
    }

    void RemovePGroupWaypoints(GROUP? pGroup)
    {
        WAYPOINT? wp;
        //if there aren't any waypoints to delete, then return.  This also avoids setting
        //the fWaypointsCancelled flag.
        if (!pGroup.pWaypoints)
        {
            return;
        }
        //remove all of the waypoints.
        while (pGroup.pWaypoints)
        {
            wp = pGroup.pWaypoints;
            pGroup.pWaypoints = pGroup.pWaypoints.next;
            MemFree(wp);
        }
        pGroup.ubNextWaypointID = 0;
        pGroup.pWaypoints = null;

        //By setting this flag, it acknowledges the possibility that the group is currently between sectors,
        //and will continue moving until it reaches the next sector.  If the user decides to change directions,
        //during this process, the arrival event must be modified to send the group back.
        //pGroup.fWaypointsCancelled = true;  
    }



    // set groups waypoints as cancelled
    void SetWayPointsAsCanceled(int ubGroupID)
    {
        GROUP? pGroup;
        pGroup = GetGroup(ubGroupID);
        Debug.Assert(pGroup);

        //pGroup . fWaypointsCancelled = true;

        return;
    }


    // set this groups previous sector values
    void SetGroupPrevSectors(int ubGroupID, int ubX, MAP_ROW ubY)
    {
        GROUP? pGroup;
        pGroup = GetGroup(ubGroupID);
        Debug.Assert(pGroup);

        // since we have a group, set prev sector's x and y
        pGroup.ubPrevX = ubX;
        pGroup.ubPrevY = ubY;

    }


    void RemoveGroup(int ubGroupID)
    {
        GROUP? pGroup;
        pGroup = GetGroup(ubGroupID);

        if (ubGroupID == 51)
        {
            int i = 0;
        }

        Debug.Assert(pGroup);
        RemovePGroup(pGroup);
    }

    bool gfRemovingAllGroups = false;

    void RemovePGroup(GROUP? pGroup)
    {
        int bit, index, mask;

        if (pGroup.fPersistant && !gfRemovingAllGroups)
        {
            CancelEmptyPersistentGroupMovement(pGroup);
            return;
            DoScreenIndependantMessageBox("Strategic Info Warning:  Attempting to delete a persistant group.", MSG_BOX_FLAG_OK, null);
        }
        //if removing head, then advance head first.
        if (pGroup == gpGroupList)
        {
            gpGroupList = gpGroupList.next;
        }
        else
        { //detach this node from the list.
            GROUP? curr;
            curr = gpGroupList;
            while (curr.next && curr.next != pGroup)
            {
                curr = curr.next;
            }
            //AssertMsg(curr.next == pGroup, "Trying to remove a strategic group that isn't in the list!");
            curr.next = pGroup.next;
        }


        //Remove the waypoints.
        RemovePGroupWaypoints(pGroup);

        //Remove the arrival event if applicable.
        DeleteStrategicEvent(EVENT_GROUP_ARRIVAL, pGroup.ubGroupID);

        //Determine what type of group we have (because it requires different methods)
        if (pGroup.fPlayer)
        { //Remove player group
            PLAYERGROUP? pPlayer;
            while (pGroup.pPlayerList)
            {
                pPlayer = pGroup.pPlayerList;
                pGroup.pPlayerList = pGroup.pPlayerList.next;
                MemFree(pPlayer);
            }
        }
        else
        {
            RemoveGroupFromStrategicAILists(pGroup.ubGroupID);
            MemFree(pGroup.pEnemyGroup);
        }

        //clear the unique group ID
        index = pGroup.ubGroupID / 32;
        bit = pGroup.ubGroupID % 32;
        mask = 1 << bit;

        if (!(uniqueIDMask[index] & mask))
        {
            mask = mask;
        }

        uniqueIDMask[index] -= mask;

        MemFree(pGroup);
        pGroup = null;
    }

    void RemoveAllGroups()
    {
        gfRemovingAllGroups = true;
        while (gpGroupList)
        {
            RemovePGroup(gpGroupList);
        }
        gfRemovingAllGroups = false;
    }

    void SetGroupSectorValue(int sSectorX, MAP_ROW sSectorY, int sSectorZ, int ubGroupID)
    {
        GROUP? pGroup;
        PLAYERGROUP? pPlayer;

        // get the group
        pGroup = GetGroup(ubGroupID);

        // make sure it is valid
        Debug.Assert(pGroup);

        //Remove waypoints
        RemovePGroupWaypoints(pGroup);

        // set sector x and y to passed values
        pGroup.ubSectorX = pGroup.ubNextX = (int)sSectorX;
        pGroup.ubSectorY = pGroup.ubNextY = sSectorY;
        pGroup.ubSectorZ = (int)sSectorZ;
        pGroup.fBetweenSectors = false;

        // set next sectors same as current
        pGroup.ubOriginalSector = SECTORINFO.SECTOR(pGroup.ubSectorX, pGroup.ubSectorY);
        GameEvents.DeleteStrategicEvent(EVENT.GROUP_ARRIVAL, pGroup.ubGroupID);

        // set all of the mercs in the group so that they are in the new sector too.
        pPlayer = pGroup.pPlayerList;
        while (pPlayer)
        {
            pPlayer.pSoldier.sSectorX = sSectorX;
            pPlayer.pSoldier.sSectorY = sSectorY;
            pPlayer.pSoldier.bSectorZ = (int)sSectorZ;
            pPlayer.pSoldier.fBetweenSectors = false;
            pPlayer.pSoldier.uiStatusFlags &= ~SOLDIER.SHOULD_BE_TACTICALLY_VALID;
            pPlayer = pPlayer.next;
        }

        CheckAndHandleUnloadingOfCurrentWorld();
    }

    void SetEnemyGroupSector(GROUP? pGroup, SEC ubSectorID)
    {
        // make sure it is valid
        Debug.Assert(pGroup);
        GameEvents.DeleteStrategicEvent(EVENT.GROUP_ARRIVAL, pGroup.ubGroupID);

        //Remove waypoints
        if (!gfRandomizingPatrolGroup)
        {
            RemovePGroupWaypoints(pGroup);
        }

        // set sector x and y to passed values
        pGroup.ubSectorX = pGroup.ubNextX = SECTORINFO.SECTORX(ubSectorID);
        pGroup.ubSectorY = pGroup.ubNextY = SECTORINFO.SECTORY(ubSectorID);
        pGroup.ubSectorZ = 0;
        pGroup.fBetweenSectors = false;
        //pGroup.fWaypointsCancelled = false;
    }


    void SetGroupNextSectorValue(int sSectorX, MAP_ROW sSectorY, int ubGroupID)
    {
        GROUP? pGroup;

        // get the group
        pGroup = GetGroup(ubGroupID);

        // make sure it is valid
        Debug.Assert(pGroup);

        //Remove waypoints
        RemovePGroupWaypoints(pGroup);

        // set sector x and y to passed values
        pGroup.ubNextX = sSectorX;
        pGroup.ubNextY = sSectorY;
        pGroup.fBetweenSectors = false;

        // set next sectors same as current
        pGroup.ubOriginalSector = SECTORINFO.SECTOR(pGroup.ubSectorX, pGroup.ubSectorY);
    }


    // get eta of the group with this id
    int CalculateTravelTimeOfGroupId(int ubId)
    {
        GROUP? pGroup;

        // get the group
        pGroup = GetGroup(ubId);

        if (pGroup == null)
        {
            return (0);
        }

        return (CalculateTravelTimeOfGroup(pGroup));
    }

    int CalculateTravelTimeOfGroup(GROUP? pGroup)
    {
        int iDelta;
        int uiEtaTime = 0;
        WAYPOINT? pNode = null;
        WAYPOINT pCurrent, pDest;
        int ubCurrentSector = 0;


        // check if valid group
        if (pGroup == null)
        {
            // return current time
            return (uiEtaTime);
        }

        // set up next node
        pNode = pGroup.pWaypoints;

        // now get the delta in current sector and next sector
        iDelta = (SECTORINFO.SECTOR(pGroup.ubSectorX, pGroup.ubSectorY) - SECTORINFO.SECTOR(pGroup.ubNextX, pGroup.ubNextY));

        if (iDelta == 0)
        {
            // not going anywhere...return current time
            return (uiEtaTime);
        }


        // if already on the road
        if (pGroup.fBetweenSectors)
        {
            // to get travel time to the first sector, use the arrival time, this way it accounts for delays due to simul. arrival
            if (pGroup.uiArrivalTime >= GameClock.GetWorldTotalMin())
            {
                uiEtaTime += (pGroup.uiArrivalTime - GameClock.GetWorldTotalMin());
            }

            // first waypoint is NEXT sector
            pCurrent.x = pGroup.ubNextX;
            pCurrent.y = pGroup.ubNextY;
        }
        else
        {
            // first waypoint is CURRENT sector
            pCurrent.x = pGroup.ubSectorX;
            pCurrent.y = pGroup.ubSectorY;
        }

        while (pNode)
        {
            pDest.x = pNode.x;
            pDest.y = pNode.y;

            // update eta time by the path between these 2 waypts
            uiEtaTime += FindTravelTimeBetweenWaypoints(pCurrent, pDest, pGroup);

            pCurrent.x = pNode.x;
            pCurrent.y = pNode.y;

            // next waypt
            pNode = pNode.next;
        }

        return (uiEtaTime);
    }

    uint FindTravelTimeBetweenWaypoints(WAYPOINT? pSource, WAYPOINT? pDest, GROUP? pGroup)
    {
        SEC ubStart = 0, ubEnd = 0;
        int iDelta = 0;
        uint iCurrentCostInTime = 0;
        SEC ubCurrentSector = 0;
        StrategicMove ubDirection;
        uint iThisCostInTime;


        // find travel time between waypoints
        if (!pSource || !pDest)
        {
            // no change
            return (iCurrentCostInTime);
        }

        // get start and end setor values
        ubStart = SECTORINFO.SECTOR(pSource.x, pSource.y);
        ubEnd = SECTORINFO.SECTOR(pDest.x, pDest.y);

        // are we in fact moving?
        if (ubStart == ubEnd)
        {
            // no
            return (iCurrentCostInTime);
        }

        iDelta = (int)(ubEnd - ubStart);

        // which direction are we moving?
        if (iDelta > 0)
        {
            if (iDelta % (SOUTH_MOVE - 2) == 0)
            {
                iDelta = (SOUTH_MOVE - 2);
                ubDirection = StrategicMove.SOUTH;
            }
            else
            {
                iDelta = EAST_MOVE;
                ubDirection = StrategicMove.EAST;
            }
        }
        else
        {
            if (iDelta % (NORTH_MOVE + 2) == 0)
            {
                iDelta = (NORTH_MOVE + 2);
                ubDirection = StrategicMove.NORTH;
            }
            else
            {
                iDelta = WEST_MOVE;
                ubDirection = StrategicMove.WEST;
            }
        }

        for (ubCurrentSector = ubStart; ubCurrentSector != ubEnd; ubCurrentSector += (int)iDelta)
        {
            // find diff between current and next
            iThisCostInTime = GetSectorMvtTimeForGroup(ubCurrentSector, ubDirection, pGroup);

            if (iThisCostInTime == 0xffffffff)
            {
                //AssertMsg(0, String("Group %d (%s) attempting illegal move from sector %d, dir %d (%s).",
                //pGroup.ubGroupID, (pGroup.fPlayer) ? "Player" : "AI",
                //        ubCurrentSector, ubDirection,
                //        gszTerrain[SectorInfo[ubCurrentSector].ubTraversability[ubDirection]]));
            }

            // accumulate it
            iCurrentCostInTime += iThisCostInTime;
        }

        return (iCurrentCostInTime);
    }

    //CHANGES:  ubDirection contains the strategic move value, not the delta value.  
    uint GetSectorMvtTimeForGroup(SEC ubSector, StrategicMove ubDirection, GROUP? pGroup)
    {
        uint iTraverseTime;
        uint iBestTraverseTime = 1000000;
        int iEncumbrance, iHighestEncumbrance = 0;
        SOLDIERTYPE? pSoldier;
        bool fFoot, fCar, fTruck, fTracked, fAir;
        Traversability ubTraverseType;
        uint ubTraverseMod;


        // THIS FUNCTION WAS WRITTEN TO HANDLE MOVEMENT TYPES WHERE MORE THAN ONE TRANSPORTAION TYPE IS AVAILABLE.

        //Determine the group's method(s) of tranportation.  If more than one,
        //we will always use the highest time.
        fFoot = pGroup.ubTransportationMask.HasFlag(VehicleTypes.FOOT);
        fCar = pGroup.ubTransportationMask.HasFlag(VehicleTypes.CAR);
        fTruck = pGroup.ubTransportationMask.HasFlag(VehicleTypes.TRUCK);
        fTracked = pGroup.ubTransportationMask.HasFlag(VehicleTypes.TRACKED);
        fAir = pGroup.ubTransportationMask.HasFlag(VehicleTypes.AIR);

        ubTraverseType = SectorInfo[ubSector].ubTraversability[ubDirection];

        if (ubTraverseType == Traversability.EDGEOFWORLD)
        {
            return 0xffffffff; //can't travel here!
        }

        // ARM: Made air-only travel take its normal time per sector even through towns.  Because Skyrider charges by the sector,
        // not by flying time, it's annoying when his default route detours through a town to save time, but costs extra money.
        // This isn't exactly unrealistic, since the chopper shouldn't be faster flying over a town anyway...  Not that other
        // kinds of travel should be either - but the towns represents a kind of warping of our space-time scale as it is...
        if ((ubTraverseType == Traversability.TOWN) && (pGroup.ubTransportationMask != VehicleTypes.AIR))
        {
            return 5; //very fast, and vehicle types don't matter.
        }

        if (fFoot)
        {
            switch (ubTraverseType)
            {
                case Traversability.ROAD:
                    ubTraverseMod = 100;
                    break;
                case Traversability.PLAINS:
                    ubTraverseMod = 85;
                    break;
                case Traversability.SAND:
                    ubTraverseMod = 50;
                    break;
                case Traversability.SPARSE:
                    ubTraverseMod = 70;
                    break;
                case Traversability.DENSE:
                    ubTraverseMod = 60;
                    break;
                case Traversability.SWAMP:
                    ubTraverseMod = 35;
                    break;
                case Traversability.WATER:
                    ubTraverseMod = 25;
                    break;
                case Traversability.HILLS:
                    ubTraverseMod = 50;
                    break;
                case Traversability.GROUNDBARRIER:
                    ubTraverseMod = 0;
                    break;
                case Traversability.NS_RIVER:
                    ubTraverseMod = 25;
                    break;
                case Traversability.EW_RIVER:
                    ubTraverseMod = 25;
                    break;
                default:
                    Debug.Assert(false);
                    return 0xffffffff;
            }

            if (ubTraverseMod == 0)
            {
                return 0xffffffff; //Group can't traverse here.
            }
            iTraverseTime = FOOT_TRAVEL_TIME * 100 / ubTraverseMod;
            if (iTraverseTime < iBestTraverseTime)
            {
                iBestTraverseTime = iTraverseTime;
            }

            if (pGroup.fPlayer)
            {
                foreach (var curr in pGroup.pPlayerList)
                {
                    pSoldier = curr.pSoldier;
                    if (pSoldier.bAssignment != Assignments.VEHICLE)
                    { //Soldier is on foot and travelling.  Factor encumbrance into movement rate.
                        iEncumbrance = ItemSubSystem.CalculateCarriedWeight(pSoldier);
                        if (iEncumbrance > iHighestEncumbrance)
                        {
                            iHighestEncumbrance = iEncumbrance;
                        }
                    }
                }

                if (iHighestEncumbrance > 100)
                {
                    iBestTraverseTime = (uint)(iBestTraverseTime * iHighestEncumbrance / 100);
                }
            }
        }

        if (fCar)
        {
            ubTraverseMod = ubTraverseType switch
            {
                Traversability.ROAD => 100,
                _ => 0,
            };

            if (ubTraverseMod == 0)
            {
                return 0xffffffff; //Group can't traverse here.
            }

            iTraverseTime = CAR_TRAVEL_TIME * 100 / ubTraverseMod;
            if (iTraverseTime < iBestTraverseTime)
            {
                iBestTraverseTime = iTraverseTime;
            }
        }
        if (fTruck)
        {
            ubTraverseMod = ubTraverseType switch
            {
                Traversability.ROAD => 100,
                Traversability.PLAINS => 75,
                Traversability.SPARSE => 60,
                Traversability.HILLS => 50,
                _ => 0,
            };
            if (ubTraverseMod == 0)
            {
                return 0xffffffff; //Group can't traverse here.
            }

            iTraverseTime = TRUCK_TRAVEL_TIME * 100 / ubTraverseMod;
            if (iTraverseTime < iBestTraverseTime)
            {
                iBestTraverseTime = iTraverseTime;
            }
        }
        if (fTracked)
        {
            ubTraverseMod = ubTraverseType switch
            {
                Traversability.ROAD => 100,
                Traversability.PLAINS => 100,
                Traversability.SAND => 70,
                Traversability.SPARSE => 60,
                Traversability.HILLS => 60,
                Traversability.NS_RIVER => 20,
                Traversability.EW_RIVER => 20,
                Traversability.WATER => 10,
                _ => 0,
            };
            if (ubTraverseMod == 0)
            {
                return 0xffffffff; //Group can't traverse here.
            }

            iTraverseTime = TRACKED_TRAVEL_TIME * 100 / ubTraverseMod;
            if (iTraverseTime < iBestTraverseTime)
            {
                iBestTraverseTime = iTraverseTime;
            }
        }
        if (fAir)
        {
            iTraverseTime = AIR_TRAVEL_TIME;
            if (iTraverseTime < iBestTraverseTime)
            {
                iBestTraverseTime = iTraverseTime;
            }
        }
        return iBestTraverseTime;
    }



    //Counts the number of live mercs in any given sector.
    public static int PlayerMercsInSector(int ubSectorX, MAP_ROW ubSectorY, int ubSectorZ)
    {
        List<PLAYERGROUP> pPlayer;
        int ubNumMercs = 0;

        foreach (var pGroup in gpGroupList)
        {
            if (pGroup.fPlayer && !pGroup.fBetweenSectors)
            {
                if (pGroup.ubSectorX == ubSectorX && pGroup.ubSectorY == ubSectorY && pGroup.ubSectorZ == ubSectorZ)
                {
                    //we have a group, make sure that it isn't a group containing only dead members.
                    pPlayer = pGroup.pPlayerList;
                    foreach (var pg in pPlayer)
                    {
                        // robots count as mercs here, because they can fight, but vehicles don't
                        if ((pg.pSoldier.bLife > 0) && !(pg.pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE)))
                        {
                            ubNumMercs++;
                        }
                    }
                }
            }
        }

        return ubNumMercs;
    }

    int PlayerGroupsInSector(int ubSectorX, MAP_ROW ubSectorY, int ubSectorZ)
    {
        int ubNumGroups = 0;
        foreach (var pGroup in gpGroupList)
        {
            if (pGroup.fPlayer && !pGroup.fBetweenSectors)
            {
                if (pGroup.ubSectorX == ubSectorX && pGroup.ubSectorY == ubSectorY && pGroup.ubSectorZ == ubSectorZ)
                {
                    //we have a group, make sure that it isn't a group containing only dead members.
                    foreach (var pPlayer in pGroup.pPlayerList)
                    {
                        if (pPlayer.pSoldier.bLife > 0)
                        {
                            ubNumGroups++;
                            break;
                        }
                    }
                }
            }
        }

        return ubNumGroups;
    }


    // is the player group with this id in motion?
    bool PlayerIDGroupInMotion(int ubID)
    {
        GROUP? pGroup;

        // get the group
        pGroup = GetGroup(ubID);

        // make sure it is valid

        // no group
        if (pGroup == null)
        {
            return (false);
        }


        return (PlayerGroupInMotion(pGroup));
    }

    // is the player group in motion?
    bool PlayerGroupInMotion(GROUP? pGroup)
    {
        return (pGroup.fBetweenSectors);
    }


    // get travel time for this group
    int GetTravelTimeForGroup(int ubSector, int ubDirection, int ubGroup)
    {
        GROUP? pGroup;

        // get the group
        pGroup = GetGroup(ubGroup);

        // make sure it is valid
        Debug.Assert(pGroup is not null);

        return (GetSectorMvtTimeForGroup(ubSector, ubDirection, pGroup));
    }

    int GetTravelTimeForFootTeam(int ubSector, int ubDirection)
    {
        GROUP Group = new();

        // group going on foot
        Group.ubTransportationMask = VehicleTypes.FOOT;

        return (GetSectorMvtTimeForGroup(ubSector, ubDirection, out (Group)));

    }

    //Add this group to the current battle fray!
    //NOTE:  For enemies, only MAX_STRATEGIC_TEAM_SIZE at a time can be in a battle, so
    //if it ever gets past that, god help the player, but we'll have to insert them
    //as those slots free up.
    void HandleArrivalOfReinforcements(GROUP? pGroup)
    {
        SOLDIERTYPE? pSoldier;
        SECTORINFO? pSector;
        int iNumEnemiesInSector;
        int cnt;

        if (pGroup.fPlayer)
        { //We don't have to worry about filling up the player slots, because it is impossible
          //to have more player's in the game then the number of slots available for the player.
            PLAYERGROUP? pPlayer;
            int ubStrategicInsertionCode;
            //First, determine which entrypoint to use, based on the travel direction of the group.
            if (pGroup.ubSectorX < pGroup.ubPrevX)
            {
                ubStrategicInsertionCode = INSERTION_CODE_EAST;
            }
            else if (pGroup.ubSectorX > pGroup.ubPrevX)
            {
                ubStrategicInsertionCode = INSERTION_CODE_WEST;
            }
            else if (pGroup.ubSectorY < pGroup.ubPrevY)
            {
                ubStrategicInsertionCode = INSERTION_CODE_SOUTH;
            }
            else if (pGroup.ubSectorY > pGroup.ubPrevY)
            {
                ubStrategicInsertionCode = INSERTION_CODE_NORTH;
            }
            else
            {
                Debug.Assert(false);
                return;
            }
            pPlayer = pGroup.pPlayerList;

            cnt = 0;

            while (pPlayer)
            {
                pSoldier = pPlayer.pSoldier;
                Debug.Assert(pSoldier);
                pSoldier.ubStrategicInsertionCode = ubStrategicInsertionCode;
                UpdateMercInSector(pSoldier, pGroup.ubSectorX, pGroup.ubSectorY, 0);
                pPlayer = pPlayer.next;

                // DO arrives quote....
                if (cnt == 0)
                {
                    TacticalCharacterDialogue(pSoldier, QUOTE_MERC_REACHED_DESTINATION);
                }
                cnt++;
            }

            Messages.ScreenMsg(FontColor.FONT_YELLOW, MSG_INTERFACE, Message[STR_PLAYER_REINFORCEMENTS]);

        }
        else
        {
            gfPendingEnemies = true;
            ResetMortarsOnTeamCount();
            AddPossiblePendingEnemiesToBattle();
        }
        //Update the known number of enemies in the sector.
        pSector = SectorInfo[SECTOR(pGroup.ubSectorX, pGroup.ubSectorY)];
        iNumEnemiesInSector = NumEnemiesInSector(pGroup.ubSectorX, pGroup.ubSectorY);
        if (iNumEnemiesInSector)
        {
            if (pSector.bLastKnownEnemies >= 0)
            {
                pSector.bLastKnownEnemies = (int)iNumEnemiesInSector;
            }
            //if we don't know how many enemies there are, then we can't update this value.
        }
        else
        {
            pSector.bLastKnownEnemies = 0;
        }
    }

    public static bool PlayersBetweenTheseSectors(int sSource, int sDest, out int iCountEnter, out int iCountExit, out bool fAboutToArriveEnter)
    {
        GROUP? curr = gpGroupList;
        int sBattleSector = -1;
        bool fMayRetreatFromBattle = false;
        bool fRetreatingFromBattle = false;
        bool fHandleRetreats = false;
        bool fHelicopterGroup = false;
        int ubMercsInGroup = 0;

        iCountEnter = 0;
        iCountExit = 0;
        fAboutToArriveEnter = false;

        if (gpBattleGroup is not null)
        {
            //Debug.Assert( gfPreBattleInterfaceActive );
            sBattleSector = (int)SECTOR(gpBattleGroup.ubSectorX, gpBattleGroup.ubSectorY);
        }

        // debug only
        if (gfDisplayPotentialRetreatPaths == true)
        {
            //Debug.Assert( gfPreBattleInterfaceActive );
        }


        // get number of characters entering/existing between these two sectors.  Special conditions during 
        // pre-battle interface to return where this function is used to show potential retreating directions instead!

        //	check all groups
        while (curr)
        {
            // if player group
            if (curr.fPlayer == true)
            {
                fHelicopterGroup = IsGroupTheHelicopterGroup(curr);

                // if this group is aboard the helicopter and we're showing the airspace layer, don't count any mercs aboard the
                // chopper, because the chopper icon itself serves the function of showing the location/size of this group
                if (!fHelicopterGroup || !fShowAircraftFlag)
                {
                    // if only showing retreat paths, ignore groups not in the battle sector
                    // if NOT showing retreat paths, ignore groups not between sectors
                    if ((gfDisplayPotentialRetreatPaths == true) && (sBattleSector == sSource) ||
                             (gfDisplayPotentialRetreatPaths == false) && (curr.fBetweenSectors == true))
                    {
                        fMayRetreatFromBattle = false;
                        fRetreatingFromBattle = false;

                        if ((sBattleSector == sSource) && (SECTOR(curr.ubSectorX, curr.ubSectorY) == sSource) && (SECTOR(curr.ubPrevX, curr.ubPrevY) == sDest))
                        {
                            fMayRetreatFromBattle = true;
                        }

                        if ((sBattleSector == sDest) && (SECTOR(curr.ubSectorX, curr.ubSectorY) == sDest) && (SECTOR(curr.ubPrevX, curr.ubPrevY) == sSource))
                        {
                            fRetreatingFromBattle = true;
                        }

                        ubMercsInGroup = curr.ubGroupSize;

                        if (((SECTOR(curr.ubSectorX, curr.ubSectorY) == sSource) && (SECTOR(curr.ubNextX, curr.ubNextY) == sDest)) || (fMayRetreatFromBattle == true))
                        {
                            // if it's a valid vehicle, but not the helicopter (which can fly empty)
                            if (curr.fVehicle && !fHelicopterGroup && (GivenMvtGroupIdFindVehicleId(curr.ubGroupID) != -1))
                            {
                                // make sure empty vehicles (besides helicopter) aren't in motion!
                                Debug.Assert(ubMercsInGroup > 0);
                                // subtract 1, we don't wanna count the vehicle itself for purposes of showing a number on the map
                                ubMercsInGroup--;
                            }

                            iCountEnter += ubMercsInGroup;

                            if ((curr.uiArrivalTime - GameClock.GetWorldTotalMin() <= ABOUT_TO_ARRIVE_DELAY) || (fMayRetreatFromBattle == true))
                            {
                                fAboutToArriveEnter = true;
                            }
                        }
                        else if ((SECTOR(curr.ubSectorX, curr.ubSectorY) == sDest) && (SECTOR(curr.ubNextX, curr.ubNextY) == sSource) || (fRetreatingFromBattle == true))
                        {
                            // if it's a valid vehicle, but not the helicopter (which can fly empty)
                            if (curr.fVehicle && !fHelicopterGroup && (GivenMvtGroupIdFindVehicleId(curr.ubGroupID) != -1))
                            {
                                // make sure empty vehicles (besides helicopter) aren't in motion!
                                Debug.Assert(ubMercsInGroup > 0);
                                // subtract 1, we don't wanna count the vehicle itself for purposes of showing a number on the map
                                ubMercsInGroup--;
                            }

                            iCountExit += ubMercsInGroup;
                        }
                    }
                }
            }

            // next group
            curr = curr.next;
        }

        // if there was actually anyone leaving this sector and entering next
        if (*iCountEnter > 0)
        {
            return (true);
        }
        else
        {
            return (false);
        }
    }

    void MoveAllGroupsInCurrentSectorToSector(int ubSectorX, MAP_ROW ubSectorY, int ubSectorZ)
    {
        GROUP? pGroup;
        PLAYERGROUP? pPlayer;
        pGroup = gpGroupList;
        while (pGroup is not null)
        {
            if (pGroup.fPlayer && pGroup.ubSectorX == gWorldSectorX && pGroup.ubSectorY == gWorldSectorY &&
                  pGroup.ubSectorZ == gbWorldSectorZ && !pGroup.fBetweenSectors)
            { //This player group is in the currently loaded sector...
                pGroup.ubSectorX = ubSectorX;
                pGroup.ubSectorY = ubSectorY;
                pGroup.ubSectorZ = ubSectorZ;
                pPlayer = pGroup.pPlayerList;
                while (pPlayer is not null)
                {
                    pPlayer.pSoldier.sSectorX = ubSectorX;
                    pPlayer.pSoldier.sSectorY = ubSectorY;
                    pPlayer.pSoldier.bSectorZ = ubSectorZ;
                    pPlayer.pSoldier.fBetweenSectors = false;
                    pPlayer = pPlayer.next;
                }
            }
            pGroup = pGroup.next;
        }
        CheckAndHandleUnloadingOfCurrentWorld();
    }


    public static void GetGroupPosition(out int ubNextX, out MAP_ROW ubNextY, out int ubPrevX, out int ubPrevY, out uint uiTraverseTime, out int uiArriveTime, int ubGroupId)
    {
        GROUP? pGroup;

        // get the group
        pGroup = GetGroup(ubGroupId);

        // make sure it is valid

        // no group
        if (pGroup == null)
        {
            ubNextX = 0;
            ubNextY = 0;
            ubPrevX = 0;
            ubPrevY = 0;
            uiTraverseTime = 0;
            uiArriveTime = 0;
            return;
        }

        // valid group, grab values
        ubNextX = pGroup.ubNextX;
        ubNextY = pGroup.ubNextY;
        ubPrevX = pGroup.ubPrevX;
        ubPrevY = pGroup.ubPrevY;
        uiTraverseTime = pGroup.uiTraverseTime;
        uiArriveTime = pGroup.uiArrivalTime;


        return;
    }


    // this is only for grunts who were in mvt groups between sectors and are set to a new squad...NOTHING ELSE!!!!!
    void SetGroupPosition(int ubNextX, int ubNextY, int ubPrevX, int ubPrevY, int uiTraverseTime, int uiArriveTime, int ubGroupId)
    {
        GROUP? pGroup;
        PLAYERGROUP? pPlayer;


        // get the group
        pGroup = GetGroup(ubGroupId);

        // no group
        if (pGroup == null)
        {

            return;
        }

        // valid group, grab values
        pGroup.ubNextX = ubNextX;
        pGroup.ubNextY = ubNextY;
        pGroup.ubPrevX = ubPrevX;
        pGroup.ubPrevY = ubPrevY;
        pGroup.uiTraverseTime = uiTraverseTime;
        SetGroupArrivalTime(pGroup, uiArriveTime);
        pGroup.fBetweenSectors = true;


        AddWaypointToPGroup(pGroup, pGroup.ubNextX, pGroup.ubNextY);
        //now, if player group set all grunts in the group to be between secotrs
        if (pGroup.fPlayer == true)
        {
            pPlayer = pGroup.pPlayerList;
            while (pPlayer)
            {
                pPlayer.pSoldier.fBetweenSectors = true;
                pPlayer = pPlayer.next;
            }
        }


        return;
    }

    bool SaveStrategicMovementGroupsToSaveGameFile(Stream hFile)
    {
        GROUP? pGroup = null;
        int uiNumberOfGroups = 0;
        int uiNumBytesWritten = 0;

        pGroup = gpGroupList;

        //Count the number of active groups
        while (pGroup)
        {
            uiNumberOfGroups++;
            pGroup = pGroup.next;
        }


        // Save the number of movement groups to the saved game file
        FileWrite(hFile, &uiNumberOfGroups, sizeof(int), &uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(int))
        {
            //Error Writing size of L.L. to disk
            return (false);
        }


        pGroup = gpGroupList;

        //Loop through the linked lists and add each node
        while (pGroup)
        {
            // Save each node in the LL
            FileWrite(hFile, pGroup, sizeof(GROUP), &uiNumBytesWritten);
            if (uiNumBytesWritten != sizeof(GROUP))
            {
                //Error Writing group node to disk
                return (false);
            }

            //
            // Save the linked list, for the current type of group
            //

            // If its a player group
            if (pGroup.fPlayer)
            {
                //if there is a player list, add it
                if (pGroup.ubGroupSize)
                {
                    //Save the player group list
                    SavePlayerGroupList(hFile, pGroup);
                }
            }
            else //else its an enemy group
            {
                //Make sure the pointer is valid
                Debug.Assert(pGroup.pEnemyGroup);

                // 
                SaveEnemyGroupStruct(hFile, pGroup);
            }

            //Save the waypoint list for the group, if they have one
            SaveWayPointList(hFile, pGroup);



            pGroup = pGroup.next;
        }

        // Save the unique id mask
        FileWrite(hFile, uniqueIDMask, sizeof(int) * 8, &uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(int) * 8)
        {
            //Error Writing size of L.L. to disk
            return (false);
        }


        return (true);
    }



    bool LoadStrategicMovementGroupsFromSavedGameFile(Stream hFile)
    {
        GROUP? pGroup = null;
        GROUP? pTemp = null;
        int uiNumberOfGroups = 0;
        //int	uiNumBytesWritten=0;
        int uiNumBytesRead = 0;
        int cnt;
        int bit, index, mask;
        int ubNumPlayerGroupsEmpty = 0;
        int ubNumEnemyGroupsEmpty = 0;
        int ubNumPlayerGroupsFull = 0;
        int ubNumEnemyGroupsFull = 0;



        //delete the existing group list
        while (gpGroupList)
        {
            RemoveGroupFromList(gpGroupList);
        }


        //load the number of nodes in the list
        FileRead(hFile, &uiNumberOfGroups, sizeof(int), &uiNumBytesRead);
        if (uiNumBytesRead != sizeof(int))
        {
            //Error Writing size of L.L. to disk
            return (false);
        }

        pGroup = gpGroupList;

        //loop through all the nodes and add them to the LL
        for (cnt = 0; cnt < uiNumberOfGroups; cnt++)
        {
            //allocate memory for the node
            pTemp = MemAlloc(sizeof(GROUP));
            if (pTemp == null)
            {
                return (false);
            }

            //memset(pTemp, 0, sizeof(GROUP));

            //Read in the node
            FileRead(hFile, pTemp, sizeof(GROUP), &uiNumBytesRead);
            if (uiNumBytesRead != sizeof(GROUP))
            {
                //Error Writing size of L.L. to disk
                return (false);
            }


            //
            // Add either the pointer or the linked list.
            //

            if (pTemp.fPlayer)
            {
                //if there is a player list, add it
                if (pTemp.ubGroupSize)
                {
                    //Save the player group list
                    LoadPlayerGroupList(hFile, &pTemp);
                }
            }
            else //else its an enemy group
            {
                LoadEnemyGroupStructFromSavedGame(hFile, pTemp);
            }


            //Save the waypoint list for the group, if they have one
            LoadWayPointList(hFile, pTemp);


            pTemp.next = null;

            //add the node to the list

            //if its the firs node
            if (cnt == 0)
            {
                gpGroupList = pTemp;
                pGroup = gpGroupList;
            }
            else
            {
                pGroup.next = pTemp;
                pGroup = pGroup.next;
            }
        }

        // Load the unique id mask
        FileRead(hFile, uniqueIDMask, sizeof(int) * 8, out uiNumBytesRead);

        //@@@ TEMP!
        //Rebuild the uniqueIDMask as a very old bug broke the uniqueID assignments in extremely rare cases.
        //memset(uniqueIDMask, 0, sizeof(int) * 8);
        pGroup = gpGroupList;
        while (pGroup)
        {
            if (pGroup.fPlayer)
            {
                if (pGroup.ubGroupSize)
                {
                    ubNumPlayerGroupsFull++;
                }
                else
                {
                    ubNumPlayerGroupsEmpty++;
                }
            }
            else
            {
                if (pGroup.ubGroupSize)
                {
                    ubNumEnemyGroupsFull++;
                }
                else
                {
                    ubNumEnemyGroupsEmpty++;
                }
            }
            if (ubNumPlayerGroupsEmpty || ubNumEnemyGroupsEmpty)
            {
                //report error?
            }
            index = pGroup.ubGroupID / 32;
            bit = pGroup.ubGroupID % 32;
            mask = 1 << bit;
            uniqueIDMask[index] += mask;
            pGroup = pGroup.next;
        }

        if (uiNumBytesRead != sizeof(int) * 8)
        {
            return (false);
        }

        return (true);
    }


    //Saves the Player's group list to the saved game file
    bool SavePlayerGroupList(Stream hFile, GROUP? pGroup)
    {
        int uiNumberOfNodesInList = 0;
        PLAYERGROUP? pTemp = null;
        int uiNumBytesWritten = 0;
        int uiProfileID;

        pTemp = pGroup.pPlayerList;

        while (pTemp)
        {
            uiNumberOfNodesInList++;
            pTemp = pTemp.next;
        }

        //Save the number of nodes in the list
        FileWrite(hFile, &uiNumberOfNodesInList, sizeof(int), &uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(int))
        {
            //Error Writing size of L.L. to disk
            return (false);
        }

        pTemp = pGroup.pPlayerList;

        //Loop trhough and save only the players profile id
        while (pTemp)
        {
            // Save the ubProfile ID for this node
            uiProfileID = pTemp.ubProfileID;
            FileWrite(hFile, &uiProfileID, sizeof(int), &uiNumBytesWritten);
            if (uiNumBytesWritten != sizeof(int))
            {
                //Error Writing size of L.L. to disk
                return (false);
            }

            pTemp = pTemp.next;
        }

        return (true);
    }



    bool LoadPlayerGroupList(Stream hFile, GROUP? pGroup)
    {
        int uiNumberOfNodesInList = 0;
        PLAYERGROUP? pTemp = null;
        PLAYERGROUP? pHead = null;
        int uiNumberOfNodes = 0;
        int uiProfileID = 0;
        int uiNumBytesRead;
        int cnt = 0;
        int sTempID;
        GROUP? pTempGroup = pGroup;

        //	pTemp = pGroup;

        //	pHead = *pGroup.pPlayerList;

        // Load the number of nodes in the player list
        FileRead(hFile, &uiNumberOfNodes, sizeof(int), &uiNumBytesRead);
        if (uiNumBytesRead != sizeof(int))
        {
            //Error Writing size of L.L. to disk
            return (false);
        }


        //loop through all the nodes and set them up
        for (cnt = 0; cnt < uiNumberOfNodes; cnt++)
        {
            //allcate space for the current node
            pTemp = MemAlloc(sizeof(PLAYERGROUP));
            if (pTemp == null)
            {
                return (false);
            }


            // Load the ubProfile ID for this node
            FileRead(hFile, &uiProfileID, sizeof(int), &uiNumBytesRead);
            if (uiNumBytesRead != sizeof(int))
            {
                //Error Writing size of L.L. to disk
                return (false);
            }

            //Set up the current node
            pTemp.ubProfileID = (int)uiProfileID;
            sTempID = GetSoldierIDFromMercID(pTemp.ubProfileID);

            //Should never happen
            //Debug.Assert( sTempID != -1 );
            pTemp.ubID = (int)sTempID;

            pTemp.pSoldier = Menptr[pTemp.ubID];

            pTemp.next = null;

            //if its the first time through
            if (cnt == 0)
            {
                pTempGroup.pPlayerList = pTemp;
                pHead = pTemp;
            }
            else
            {
                pHead.next = pTemp;

                //move to the next node
                pHead = pHead.next;
            }
        }

        return (true);
    }


    //Saves the enemy group struct to the saved game struct
    bool SaveEnemyGroupStruct(Stream hFile, GROUP? pGroup)
    {
        int uiNumBytesWritten = 0;

        //Save the enemy struct info to the saved game file
        FileWrite(hFile, pGroup.pEnemyGroup, sizeof(ENEMYGROUP), &uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(ENEMYGROUP))
        {
            //Error Writing size of L.L. to disk
            return (false);
        }

        return (true);
    }


    //Loads the enemy group struct from the saved game file
    bool LoadEnemyGroupStructFromSavedGame(Stream hFile, GROUP? pGroup)
    {
        int uiNumBytesRead = 0;

        ENEMYGROUP pEnemyGroup = new();

        //Load the enemy struct
        FileRead(hFile, pEnemyGroup, sizeof(ENEMYGROUP), &uiNumBytesRead);
        if (uiNumBytesRead != sizeof(ENEMYGROUP))
        {
            //Error Writing size of L.L. to disk
            return (false);
        }

        //Assign the struct to the group list
        pGroup.pEnemyGroup = pEnemyGroup;

        return (true);
    }


    void CheckMembersOfMvtGroupAndComplainAboutBleeding(SOLDIERTYPE? pSoldier)
    {
        // run through members of group
        int ubGroupId = pSoldier.ubGroupID;
        GROUP? pGroup;
        PLAYERGROUP? pPlayer = null;
        SOLDIERTYPE? pCurrentSoldier = null;

        pGroup = GetGroup(ubGroupId);

        // valid group?
        if (pGroup == null)
        {
            return;
        }

        // player controlled group?
        if (pGroup.fPlayer == false)
        {
            return;
        }

        // make sure there are members in the group..if so, then run through and make each bleeder compain
        pPlayer = pGroup.pPlayerList;

        // is there a player list?
        if (pPlayer == null)
        {
            return;
        }

        BeginLoggingForBleedMeToos(true);

        while (pPlayer)
        {
            pCurrentSoldier = pPlayer.pSoldier;

            if (pCurrentSoldier.bBleeding > 0)
            {
                // complain about bleeding
                TacticalCharacterDialogue(pCurrentSoldier, QUOTE.STARTING_TO_BLEED);
            }
            pPlayer = pPlayer.next;

        }

        BeginLoggingForBleedMeToos(false);

    }


    bool SaveWayPointList(Stream hFile, GROUP? pGroup)
    {
        int cnt = 0;
        int uiNumberOfWayPoints = 0;
        int uiNumBytesWritten = 0;
        List<WAYPOINT> pWayPoints = pGroup.pWaypoints;

        //loop trhough and count all the node in the waypoint list
        uiNumberOfWayPoints = pWayPoints.Count;

        //Save the number of waypoints
        FileWrite(hFile, &uiNumberOfWayPoints, sizeof(int), &uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(int))
        {
            //Error Writing size of L.L. to disk
            return (false);
        }


        if (uiNumberOfWayPoints > 0)
        {
            pWayPoints = pGroup.pWaypoints;
            for (cnt = 0; cnt < uiNumberOfWayPoints; cnt++)
            {
                //Save the waypoint node
                FileWrite(hFile, pWayPoints, sizeof(WAYPOINT), &uiNumBytesWritten);
                if (uiNumBytesWritten != sizeof(WAYPOINT))
                {
                    //Error Writing size of L.L. to disk
                    return (false);
                }

                //Advance to the next waypoint
                pWayPoints = pWayPoints.next;
            }
        }

        return (true);
    }



    bool LoadWayPointList(Stream hFile, GROUP? pGroup)
    {
        int cnt = 0;
        int uiNumberOfWayPoints = 0;
        int uiNumBytesRead = 0;
        List<WAYPOINT> pWayPoints = pGroup.pWaypoints;
        List<WAYPOINT> pTemp = new();


        //Load the number of waypoints
        FileRead(hFile, &uiNumberOfWayPoints, sizeof(int), &uiNumBytesRead);
        if (uiNumBytesRead != sizeof(int))
        {
            //Error Writing size of L.L. to disk
            return (false);
        }


        if (uiNumberOfWayPoints)
        {
            pWayPoints = pGroup.pWaypoints;
            for (cnt = 0; cnt < uiNumberOfWayPoints; cnt++)
            {
                //Allocate memory for the node
                pTemp = MemAlloc(sizeof(WAYPOINT));
                if (pTemp == null)
                {
                    return (false);
                }

                memset(pTemp, 0, sizeof(WAYPOINT));

                //Load the waypoint node
                FileRead(hFile, pTemp, sizeof(WAYPOINT), &uiNumBytesRead);
                if (uiNumBytesRead != sizeof(WAYPOINT))
                {
                    //Error Writing size of L.L. to disk
                    return (false);
                }


                pTemp.next = null;


                //if its the first node
                if (cnt == 0)
                {
                    pGroup.pWaypoints = pTemp;
                    pWayPoints = pTemp;
                }
                else
                {
                    pWayPoints.next = pTemp;

                    //Advance to the next waypoint
                    pWayPoints = pWayPoints.next;
                }
            }
        }
        else
        {
            pGroup.pWaypoints = null;
        }

        return (true);
    }

    void CalculateGroupRetreatSector(GROUP? pGroup)
    {
        SECTORINFO? pSector;
        int uiSectorID;

        uiSectorID = SECTOR(pGroup.ubSectorX, pGroup.ubSectorY);
        pSector = &SectorInfo[uiSectorID];

        if (pSector.ubTraversability[NORTH_STRATEGIC_MOVE] != GROUNDBARRIER &&
                pSector.ubTraversability[NORTH_STRATEGIC_MOVE] != EDGEOFWORLD)
        {
            pGroup.ubPrevX = pGroup.ubSectorX;
            pGroup.ubPrevY = pGroup.ubSectorY - 1;
        }
        else if (pSector.ubTraversability[EAST_STRATEGIC_MOVE] != GROUNDBARRIER &&
                pSector.ubTraversability[EAST_STRATEGIC_MOVE] != EDGEOFWORLD)
        {
            pGroup.ubPrevX = pGroup.ubSectorX + 1;
            pGroup.ubPrevY = pGroup.ubSectorY;
        }
        else if (pSector.ubTraversability[WEST_STRATEGIC_MOVE] != GROUNDBARRIER &&
                pSector.ubTraversability[WEST_STRATEGIC_MOVE] != EDGEOFWORLD)
        {
            pGroup.ubPrevX = pGroup.ubSectorX - 1;
            pGroup.ubPrevY = pGroup.ubSectorY;
        }
        else if (pSector.ubTraversability[SOUTH_STRATEGIC_MOVE] != GROUNDBARRIER &&
                pSector.ubTraversability[SOUTH_STRATEGIC_MOVE] != EDGEOFWORLD)
        {
            pGroup.ubPrevX = pGroup.ubSectorX;
            pGroup.ubPrevY = pGroup.ubSectorY + 1;
        }
        else
        {
            //AssertMsg(0, String("Player group cannot retreat from sector %c%d ", pGroup.ubSectorY + 'A' - 1, pGroup.ubSectorX));
            return;
        }
        if (pGroup.fPlayer)
        { //update the previous sector for the mercs
            PLAYERGROUP? pPlayer;
            pPlayer = pGroup.pPlayerList;
            while (pPlayer)
            {
                pPlayer.pSoldier.ubPrevSectorID = (int)SECTOR(pGroup.ubPrevX, pGroup.ubPrevY);
                pPlayer = pPlayer.next;
            }
        }
    }

    //Called when all checks have been made for the group (if possible to retreat, etc.)  This function
    //blindly determines where to move the group.
    void RetreatGroupToPreviousSector(GROUP? pGroup)
    {
        int ubSector, ubDirection = 255;
        int iVehId, dx, dy;
        Debug.Assert(pGroup);
        //AssertMsg(!pGroup.fBetweenSectors, "Can't retreat a group when between sectors!");

        if (pGroup.ubPrevX != 16 || pGroup.ubPrevY != 16)
        { //Group has a previous sector
            pGroup.ubNextX = pGroup.ubPrevX;
            pGroup.ubNextY = pGroup.ubPrevY;

            //Determine the correct direction.
            dx = pGroup.ubNextX - pGroup.ubSectorX;
            dy = pGroup.ubNextY - pGroup.ubSectorY;
            if (dy == -1 && !dx)
            {
                ubDirection = NORTH_STRATEGIC_MOVE;
            }
            else if (dx == 1 && !dy)
            {
                ubDirection = EAST_STRATEGIC_MOVE;
            }
            else if (dy == 1 && !dx)
            {
                ubDirection = SOUTH_STRATEGIC_MOVE;
            }
            else if (dx == -1 && !dy)
            {
                ubDirection = WEST_STRATEGIC_MOVE;
            }
            else
            {

                //AssertMsg(0, String("Player group attempting illegal retreat from %c%d to %c%d.",
                //pGroup.ubSectorY + 'A' - 1, pGroup.ubSectorX, pGroup.ubNextY + 'A' - 1, pGroup.ubNextX));
            }
        }
        else
        { //Group doesn't have a previous sector.  Create one, then recurse
            CalculateGroupRetreatSector(pGroup);
            RetreatGroupToPreviousSector(pGroup);
        }

        //Calc time to get to next waypoint...
        ubSector = (int)SECTOR(pGroup.ubSectorX, pGroup.ubSectorY);
        pGroup.uiTraverseTime = GetSectorMvtTimeForGroup(ubSector, ubDirection, pGroup);
        if (pGroup.uiTraverseTime == 0xffffffff)
        {
            //AssertMsg(0, String("Group %d (%s) attempting illegal move from %c%d to %c%d (%s).",
            // pGroup.ubGroupID, (pGroup.fPlayer) ? "Player" : "AI",
            //         pGroup.ubSectorY + 'A', pGroup.ubSectorX, pGroup.ubNextY + 'A', pGroup.ubNextX,
            //         gszTerrain[SectorInfo[ubSector].ubTraversability[ubDirection]]));
        }

        if (!pGroup.uiTraverseTime)
        { //Because we are in the strategic layer, don't make the arrival instantaneous (towns).
            pGroup.uiTraverseTime = 5;
        }

        SetGroupArrivalTime(pGroup, GameClock.GetWorldTotalMin() + pGroup.uiTraverseTime);
        pGroup.fBetweenSectors = true;
        pGroup.uiFlags |= GROUPFLAG_JUST_RETREATED_FROM_BATTLE;

        if (pGroup.fVehicle == true)
        {
            // vehicle, set fact it is between sectors too
            if ((iVehId = (GivenMvtGroupIdFindVehicleId(pGroup.ubGroupID))) != -1)
            {
                pVehicleList[iVehId].fBetweenSectors = true;
            }
        }

        //Post the event!
        if (!GameEvents.AddStrategicEvent(EVENT.GROUP_ARRIVAL, pGroup.uiArrivalTime, pGroup.ubGroupID))
        {
            //AssertMsg(0, "Failed to add movement event.");

            //For the case of player groups, we need to update the information of the soldiers.
            if (pGroup.fPlayer)
            {
                PLAYERGROUP? curr;
                curr = pGroup.pPlayerList;

                if (pGroup.uiArrivalTime - ABOUT_TO_ARRIVE_DELAY > GameClock.GetWorldTotalMin())
                {
                    GameEvents.AddStrategicEvent(EVENT.GROUP_ABOUT_TO_ARRIVE, pGroup.uiArrivalTime - ABOUT_TO_ARRIVE_DELAY, pGroup.ubGroupID);
                }


                while (curr)
                {
                    curr.pSoldier.fBetweenSectors = true;

                    // OK, Remove the guy from tactical engine!
                    RemoveSoldierFromTacticalSector(curr.pSoldier, true);

                    curr = curr.next;
                }
            }
        }
    }

    public static GROUP? FindMovementGroupInSector(int ubSectorX, MAP_ROW ubSectorY, bool fPlayer)
    {
        GROUP? pGroup;
        pGroup = gpGroupList;
        while (pGroup)
        {
            if (pGroup.fPlayer)
            {
                // NOTE: These checks must always match the INVOLVED group checks in PBI!!!
                if (fPlayer && pGroup.ubGroupSize && !pGroup.fBetweenSectors &&
                        pGroup.ubSectorX == ubSectorX && pGroup.ubSectorY == ubSectorY && !pGroup.ubSectorZ &&
                        !GroupHasInTransitDeadOrPOWMercs(pGroup) &&
                    (!IsGroupTheHelicopterGroup(pGroup) || !fHelicopterIsAirBorne))
                {
                    return pGroup;
                }
            }
            else if (!fPlayer && pGroup.ubSectorX == ubSectorX && pGroup.ubSectorY == ubSectorY && !pGroup.ubSectorZ)
            {
                return pGroup;
            }

            pGroup = pGroup.next;
        }
        return null;
    }

    bool GroupAtFinalDestination(GROUP? pGroup)
    {
        WAYPOINT? wp;

        if (pGroup.ubMoveType != ONE_WAY)
        {
            return false; //Group will continue to patrol, hence never stops.
        }

        //Determine if we are at the final waypoint.
        wp = GetFinalWaypoint(pGroup);

        if (!wp)
        { //no waypoints, so the group is at it's destination.  This happens when
          //an enemy group is created in the destination sector (which is legal for
          //staging groups which always stop adjacent to their real sector destination)
            return true;
        }

        // if we're there
        if ((pGroup.ubSectorX == wp.x) && (pGroup.ubSectorY == wp.y))
        {
            return true;
        }

        return false;
    }

    WAYPOINT? GetFinalWaypoint(GROUP? pGroup)
    {
        WAYPOINT? wp;

        Debug.Assert(pGroup);

        //Make sure they're on a one way route, otherwise this request is illegal
        Debug.Assert(pGroup.ubMoveType == ONE_WAY);

        wp = pGroup.pWaypoints;
        if (wp)
        {
            while (wp.next)
            {
                wp = wp.next;
            }
        }

        return (wp);
    }


    //The sector supplied resets ALL enemy groups in the sector specified.  See comments in 
    //ResetMovementForEnemyGroup() for more details on what the resetting does.
    void ResetMovementForEnemyGroupsInLocation(int ubSectorX, int ubSectorY)
    {
        GROUP? pGroup, next;
        int sSectorX, sSectorZ;
        MAP_ROW sSectorY;

        GetCurrentBattleSectorXYZ(out sSectorX, out sSectorY, out sSectorZ);
        pGroup = gpGroupList;
        while (pGroup)
        {
            next = pGroup.next;
            if (!pGroup.fPlayer)
            {
                if (pGroup.ubSectorX == sSectorX && pGroup.ubSectorY == sSectorY)
                {
                    ResetMovementForEnemyGroup(pGroup);
                }
            }
            pGroup = next;
        }
    }


    //This function is used to reset the location of the enemy group if they are
    //currently between sectors.  If they were 50% of the way from sector A10 to A11,
    //then after this function is called, then that group would be 0% of the way from
    //sector A10 to A11.  In no way does this function effect the strategic path for
    //the group.
    void ResetMovementForEnemyGroup(GROUP? pGroup)
    {
        //Validate that the group is an enemy group and that it is moving.
        if (pGroup.fPlayer)
        {
            return;
        }
        if (!pGroup.fBetweenSectors || !pGroup.ubNextX || !pGroup.ubNextY)
        { //Reset the group's assignment by moving it to the group's original sector as it's pending group.
            RepollSAIGroup(pGroup);
            return;
        }

        //Cancel the event that is posted.
        DeleteStrategicEvent(EVENT.GROUP_ARRIVAL, pGroup.ubGroupID);

        //Calculate the new arrival time (all data pertaining to movement should be valid)
        if (pGroup.uiTraverseTime > 400)
        { //The group was likely sleeping which makes for extremely long arrival times.  Shorten it
          //arbitrarily.  Doesn't really matter if this isn't accurate.
            pGroup.uiTraverseTime = 90;
        }
        SetGroupArrivalTime(pGroup, GameClock.GetWorldTotalMin() + pGroup.uiTraverseTime);

        //Add a new event 
        GameEvents.AddStrategicEvent(EVENT.GROUP_ARRIVAL, pGroup.uiArrivalTime, pGroup.ubGroupID);
    }


    void UpdatePersistantGroupsFromOldSave(int uiSavedGameVersion)
    {
        GROUP? pGroup = null;
        bool fDone = false;
        int cnt;
        bool fDoChange = false;

        // ATE: If saved game is < 61, we need to do something better!
        if (uiSavedGameVersion < 61)
        {
            for (cnt = 0; cnt < 55; cnt++)
            {
                // create mvt groups
                pGroup = GetGroup((int)cnt);

                if (pGroup != null && pGroup.fPlayer)
                {
                    pGroup.fPersistant = true;
                }
            }

            fDoChange = true;
        }
        else if (uiSavedGameVersion < 63)
        {
            for (cnt = 0; cnt < NUMBER_OF_SQUADS; cnt++)
            {
                // create mvt groups
                pGroup = GetGroup(SquadMovementGroups[cnt]);

                if (pGroup != null)
                {
                    pGroup.fPersistant = true;
                }
            }

            for (cnt = 0; cnt < MAX_VEHICLES; cnt++)
            {
                pGroup = GetGroup(gubVehicleMovementGroups[cnt]);

                if (pGroup != null)
                {
                    pGroup.fPersistant = true;
                }
            }

            fDoChange = true;
        }

        if (fDoChange)
        {
            //Remove all empty groups
            fDone = false;
            while (!fDone)
            {
                pGroup = gpGroupList;
                while (pGroup)
                {
                    if (!pGroup.ubGroupSize && !pGroup.fPersistant)
                    {
                        RemovePGroup(pGroup);
                        break;
                    }
                    pGroup = pGroup.next;
                    if (!pGroup)
                    {
                        fDone = true;
                    }
                }
            }
        }
    }

    //Determines if any particular group WILL be moving through a given sector given it's current
    //position in the route and the pGroup.ubMoveType must be ONE_WAY.  If the group is currently 
    //IN the sector, or just left the sector, it will return false.
    bool GroupWillMoveThroughSector(GROUP? pGroup, int ubSectorX, MAP_ROW ubSectorY)
    {
        WAYPOINT? wp;
        int i, dx, dy;
        int ubOrigX;
        MAP_ROW ubOrigY;

        Debug.Assert(pGroup);
        //AssertMsg(pGroup.ubMoveType == ONE_WAY, String("GroupWillMoveThroughSector() -- Attempting to test group with an invalid move type.  ubGroupID: %d, ubMoveType: %d, sector: %c%d -- KM:0",
        //pGroup.ubGroupID, pGroup.ubMoveType, pGroup.ubSectorY + 'A' - 1, pGroup.ubSectorX));

        //Preserve the original sector values, as we will be temporarily modifying the group's ubSectorX/Y values
        //as we traverse the waypoints.
        ubOrigX = pGroup.ubSectorX;
        ubOrigY = pGroup.ubSectorY;

        i = pGroup.ubNextWaypointID;
        wp = pGroup.pWaypoints;

        if (!wp)
        { //This is a floating group!?
            return false;
        }
        while (i--)
        { //Traverse through the waypoint list to the next waypoint ID
            Debug.Assert(wp);
            wp = wp.next;
        }
        Debug.Assert(wp);


        while (wp)
        {
            while (pGroup.ubSectorX != wp.x || pGroup.ubSectorY != wp.y)
            {
                //We now have the correct waypoint.
                //Analyse the group and determine which direction it will move from the current sector.
                dx = wp.x - pGroup.ubSectorX;
                dy = wp.y - pGroup.ubSectorY;
                if (dx && dy)
                { //Can't move diagonally!
                  //AssertMsg(0, String("GroupWillMoveThroughSector() -- Attempting to process waypoint in a diagonal direction from sector %c%d to sector %c%d for group at sector %c%d -- KM:0",
                  // pGroup.ubSectorY + 'A', pGroup.ubSectorX, wp.y + 'A' - 1, wp.x, ubOrigY + 'A' - 1, ubOrigX));
                  // pGroup.ubSectorX = ubOrigX;
                  // pGroup.ubSectorY = ubOrigY;
                    return true;
                }
                if (!dx && !dy) //Can't move to position currently at!
                {
                    //AssertMsg(0, String("GroupWillMoveThroughSector() -- Attempting to process same waypoint at %c%d for group at %c%d -- KM:0",
                    //wp.y + 'A' - 1, wp.x, ubOrigY + 'A' - 1, ubOrigX));
                    //pGroup.ubSectorX = ubOrigX;
                    //pGroup.ubSectorY = ubOrigY;
                    return true;
                }
                //Clip dx/dy value so that the move is for only one sector.
                if (dx >= 1)
                {
                    dx = 1;
                }
                else if (dy >= 1)
                {
                    dy = 1;
                }
                else if (dx <= -1)
                {
                    dx = -1;
                }
                else if (dy <= -1)
                {
                    dy = -1;
                }
                else
                {
                    Debug.Assert(false);
                    pGroup.ubSectorX = ubOrigX;
                    pGroup.ubSectorY = ubOrigY;
                    return true;
                }
                //Advance the sector value
                pGroup.ubSectorX = (int)(dx + pGroup.ubSectorX);
                pGroup.ubSectorY = (dy + pGroup.ubSectorY);
                //Check to see if it the sector we are checking to see if this group will be moving through.
                if (pGroup.ubSectorX == ubSectorX && pGroup.ubSectorY == ubSectorY)
                {
                    pGroup.ubSectorX = ubOrigX;
                    pGroup.ubSectorY = ubOrigY;
                    return true;
                }
            }
            //Advance to the next waypoint.
            wp = wp.next;
        }
        pGroup.ubSectorX = ubOrigX;
        pGroup.ubSectorY = ubOrigY;
        return false;
    }



    int CalculateFuelCostBetweenSectors(int ubSectorID1, int ubSectorID2)
    {
        return (0);
    }

    bool VehicleHasFuel(SOLDIERTYPE? pSoldier)
    {
        Debug.Assert(pSoldier.uiStatusFlags & SOLDIER.VEHICLE);
        if (pSoldier.sBreathRed)
        {
            return true;
        }
        return false;
    }

    int VehicleFuelRemaining(SOLDIERTYPE? pSoldier)
    {
        Debug.Assert(pSoldier.uiStatusFlags & SOLDIER.VEHICLE);
        return pSoldier.sBreathRed;
    }

    bool SpendVehicleFuel(SOLDIERTYPE? pSoldier, int sFuelSpent)
    {
        Debug.Assert(pSoldier.uiStatusFlags & SOLDIER.VEHICLE);
        pSoldier.sBreathRed -= sFuelSpent;
        pSoldier.sBreathRed = (int)Math.Max(0, pSoldier.sBreathRed);
        pSoldier.bBreath = (int)((pSoldier.sBreathRed + 99) / 100);
        return (false);
    }

    void AddFuelToVehicle(SOLDIERTYPE? pSoldier, SOLDIERTYPE? pVehicle)
    {
        OBJECTTYPE? pItem;
        int sFuelNeeded, sFuelAvailable, sFuelAdded;
        pItem = pSoldier.inv[InventorySlot.HANDPOS];
        if (pItem.usItem != Items.GAS_CAN)
        {

            return;
        }
        //Soldier has gas can, so now add gas to vehicle while removing gas from the gas can.
        //A gas can with 100 status translate to 50% of a fillup.
        if (pVehicle.sBreathRed == 10000)
        { //Message for vehicle full?
            return;
        }
        if (pItem.bStatus)
        { //Fill 'er up.
            sFuelNeeded = 10000 - pVehicle.sBreathRed;
            sFuelAvailable = pItem.bStatus[0] * 50;
            sFuelAdded = Math.Min(sFuelNeeded, sFuelAvailable);
            //Add to vehicle
            pVehicle.sBreathRed += sFuelAdded;
            pVehicle.bBreath = (int)(pVehicle.sBreathRed / 100);
            //Subtract from item
            pItem.bStatus[0] = (int)(pItem.bStatus[0] - sFuelAdded / 50);
            if (pItem.bStatus[0] == 0)
            { //Gas can is empty, so toast the item.
                ItemSubSystem.DeleteObj(pItem);
            }
        }
    }

    void ReportVehicleOutOfGas(int iVehicleID, int ubSectorX, MAP_ROW ubSectorY)
    {
        string str = string.Empty;
        //Report that the vehicle that just arrived is out of gas.
        wprintf(str, gzLateLocalizedString[5],
            pVehicleStrings[pVehicleList[iVehicleID].ubVehicleType],
            ubSectorY + 'A' - 1, ubSectorX);
        DoScreenIndependantMessageBox(str, MSG_BOX_FLAG_OK, null);
    }

    void SetLocationOfAllPlayerSoldiersInGroup(GROUP? pGroup, int sSectorX, MAP_ROW sSectorY, int bSectorZ)
    {
        PLAYERGROUP? pPlayer = null;
        SOLDIERTYPE? pSoldier = null;

        pPlayer = pGroup.pPlayerList;
        while (pPlayer)
        {
            pSoldier = pPlayer.pSoldier;

            if (pSoldier != null)
            {
                pSoldier.sSectorX = sSectorX;
                pSoldier.sSectorY = sSectorY;
                pSoldier.bSectorZ = bSectorZ;
            }

            pPlayer = pPlayer.next;
        }


        // if it's a vehicle
        if (pGroup.fVehicle)
        {
            int iVehicleId = -1;
            VEHICLETYPE? pVehicle = null;

            iVehicleId = GivenMvtGroupIdFindVehicleId(pGroup.ubGroupID);
            Debug.Assert(iVehicleId != -1);

            pVehicle = (pVehicleList[iVehicleId]);

            pVehicle.sSectorX = sSectorX;
            pVehicle.sSectorY = sSectorY;
            pVehicle.sSectorZ = bSectorZ;

            // if it ain't the chopper
            if (iVehicleId != iHelicopterVehicleId)
            {
                pSoldier = GetSoldierStructureForVehicle(iVehicleId);
                Debug.Assert(pSoldier);

                // these are apparently unnecessary, since vehicles are part of the pPlayerList in a vehicle group.  Oh well. 
                pSoldier.sSectorX = sSectorX;
                pSoldier.sSectorY = sSectorY;
                pSoldier.bSectorZ = bSectorZ;
            }
        }
    }


    void RandomizePatrolGroupLocation(GROUP? pGroup)
    {   //Make sure this is an enemy patrol group
        WAYPOINT? wp;
        int ubMaxWaypointID = 0;
        int ubTotalWaypoints;
        int ubChosen;
        SEC ubSectorID;

        //return; //disabled for now

        Debug.Assert(!pGroup.fPlayer);
        Debug.Assert(pGroup.ubMoveType == MOVE_TYPES.ENDTOEND_FORWARDS);
        Debug.Assert(pGroup.pEnemyGroup.ubIntention == PATROL);

        //Search for the event, and kill it (if it exists)!
        GameEvents.DeleteStrategicEvent(EVENT.GROUP_ARRIVAL, pGroup.ubGroupID);

        //count the group's waypoints
        wp = pGroup.pWaypoints;
        while (wp)
        {
            if (wp.next)
            {
                ubMaxWaypointID++;
            }
            wp = wp.next;
        }
        //double it (they go back and forth) -- it's using zero based indices, so you have to add one to get the number of actual
        //waypoints in one direction.
        ubTotalWaypoints = (int)((ubMaxWaypointID) * 2);

        //pick the waypoint they start at
        ubChosen = (int)Globals.Random.Next(ubTotalWaypoints);

        if (ubChosen >= ubMaxWaypointID)
        { //They chose a waypoint going in the reverse direction, so translate it
          //to an actual waypointID and switch directions.  
            pGroup.ubMoveType = MOVE_TYPES.ENDTOEND_BACKWARDS;
            pGroup.ubNextWaypointID = ubChosen - ubMaxWaypointID;
            ubChosen = pGroup.ubNextWaypointID + 1;
        }
        else
        {
            pGroup.ubMoveType = MOVE_TYPES.ENDTOEND_FORWARDS;
            pGroup.ubNextWaypointID = ubChosen + 1;
        }

        //Traverse through the waypoint list again, to extract the location they are at.
        wp = pGroup.pWaypoints;
        while (wp && ubChosen)
        {
            ubChosen--;
            wp = wp.next;
        }

        //logic error if this fails.  We should have a null value for ubChosen
        Debug.Assert(!ubChosen);
        Debug.Assert(wp);

        //Move the group to the location of this chosen waypoint.
        ubSectorID = SECTORINFO.SECTOR(wp.x, wp.y);

        //Set up this global var to randomize the arrival time of the group from
        //1 minute to actual traverse time between the sectors.
        gfRandomizingPatrolGroup = true;

        SetEnemyGroupSector(pGroup, ubSectorID);
        InitiateGroupMovementToNextSector(pGroup);

        //Immediately turn off the flag once finished.
        gfRandomizingPatrolGroup = false;

    }

    //Whenever a player group arrives in a sector, and if bloodcats exist in the sector,
    //roll the dice to see if this will become an ambush random encounter.
    bool TestForBloodcatAmbush(GROUP? pGroup)
    {
        SECTORINFO? pSector;
        uint iHoursElapsed;
        SEC ubSectorID;
        int ubChance;
        int bDifficultyMaxCats;
        int bProgressMaxCats;
        int bNumMercMaxCats;
        bool fAlreadyAmbushed = false;

        if (pGroup.ubSectorZ)
        { //no ambushes underground (no bloodcats either)
            return false;
        }

        ubSectorID = (int)SECTOR(pGroup.ubSectorX, pGroup.ubSectorY);
        pSector = &SectorInfo[ubSectorID];

        ubChance = 5 * gGameOptions.ubDifficultyLevel;

        iHoursElapsed = (GameClock.GetWorldTotalMin() - pSector.uiTimeCurrentSectorWasLastLoaded) / 60;
        if (ubSectorID == SEC.N5 || ubSectorID == SEC.I16)
        { //These are special maps -- we use all placements.
            if (pSector.bBloodCats == -1)
            {
                pSector.bBloodCats = pSector.bBloodCatPlacements;
            }
            else if (pSector.bBloodCats > 0 && pSector.bBloodCats < pSector.bBloodCatPlacements)
            { //Slowly have them recuperate if we haven't been here for a long time.  The population will 
              //come back up to the maximum if left long enough.
                int iBloodCatDiff;
                iBloodCatDiff = pSector.bBloodCatPlacements - pSector.bBloodCats;
                pSector.bBloodCats += (int)Math.Min(iHoursElapsed / 18, iBloodCatDiff);
            }
            //Once 0, the bloodcats will never recupe.
        }
        else if (pSector.bBloodCats == -1)
        { //If we haven't been ambushed by bloodcats yet...
            if (gfAutoAmbush || PreChance(ubChance))
            {
                //randomly choose from 5-8, 7-10, 9-12 bloodcats based on easy, normal, and hard, respectively
                bDifficultyMaxCats = (int)(Globals.Random.Next(4) + (int)gGameOptions.ubDifficultyLevel * 2 + 3);

                //maximum of 3 bloodcats or 1 for every 6%, 5%, 4% progress based on easy, normal, and hard, respectively
                bProgressMaxCats = (int)Math.Max(Campaign.CurrentPlayerProgressPercentage() / (7 - (int)gGameOptions.ubDifficultyLevel), 3);

                //make sure bloodcats don't outnumber mercs by a factor greater than 2
                bNumMercMaxCats = (int)(PlayerMercsInSector(pGroup.ubSectorX, pGroup.ubSectorY, pGroup.ubSectorZ) * 2);

                //choose the lowest number of cats calculated by difficulty and progress.
                pSector.bBloodCats = (int)Math.Min(bDifficultyMaxCats, bProgressMaxCats);

                if (gGameOptions.ubDifficultyLevel != DifficultyLevel.Hard)
                { //if not hard difficulty, ensure cats never outnumber mercs by a factor of 2 (Math.Min 3 bloodcats)
                    pSector.bBloodCats = (int)Math.Min(pSector.bBloodCats, bNumMercMaxCats);
                    pSector.bBloodCats = (int)Math.Max(pSector.bBloodCats, 3);
                }

                //ensure that there aren't more bloodcats than placements
                pSector.bBloodCats = (int)Math.Min(pSector.bBloodCats, pSector.bBloodCatPlacements);
            }
        }
        else if (ubSectorID != SEC.I16)
        {
            if (!gfAutoAmbush && PreChance(95))
            { //already ambushed here.  But 5% chance of getting ambushed again!
                fAlreadyAmbushed = true;
            }
        }

        if (!fAlreadyAmbushed && ubSectorID != SEC.N5 && pSector.bBloodCats > 0 &&
                !pGroup.fVehicle && !NumEnemiesInSector(pGroup.ubSectorX, pGroup.ubSectorY))
        {
            if (ubSectorID != SEC.I16 || gubFact[FACT.PLAYER_KNOWS_ABOUT_BLOODCAT_LAIR] == 0)
            {
                gubEnemyEncounterCode = BLOODCAT_AMBUSH_CODE;
            }
            else
            {
                gubEnemyEncounterCode = ENTERING_BLOODCAT_LAIR_CODE;
            }
            return true;
        }
        else
        {
            gubEnemyEncounterCode = NO_ENCOUNTER_CODE;
            return false;
        }
    }

    void NotifyPlayerOfBloodcatBattle(int ubSectorX, int ubSectorY)
    {
        string str = string.Empty;
        string zTempString = string.Empty;
        if (gubEnemyEncounterCode == BLOODCAT_AMBUSH_CODE)
        {
            GetSectorIDString(ubSectorX, ubSectorY, 0, zTempString, true);
            wprintf(str, pMapErrorString[12], zTempString);
        }
        else if (gubEnemyEncounterCode == ENTERING_BLOODCAT_LAIR_CODE)
        {
            wcscpy(str, pMapErrorString[13]);
        }

        if (guiCurrentScreen == ScreenName.MAP_SCREEN)
        {   //Force render mapscreen (need to update the position of the group before the dialog appears.
            fMapPanelDirty = true;
            MapScreenHandle();
            InvalidateScreen();
            RefreshScreen(null);
        }

        gfUsePersistantPBI = true;
        DoScreenIndependantMessageBox(str, MSG_BOX_FLAG_OK, TriggerPrebattleInterface);
    }



    void PlaceGroupInSector(int ubGroupID, int sPrevX, MAP_ROW sPrevY, int sNextX, MAP_ROW sNextY, int bZ, bool fCheckForBattle)
    {
        ClearMercPathsAndWaypointsForAllInGroup(GetGroup(ubGroupID));

        // change where they are and where they're going
        SetGroupPrevSectors(ubGroupID, (int)sPrevX, (int)sPrevY);
        SetGroupSectorValue(sPrevX, sPrevY, bZ, ubGroupID);
        SetGroupNextSectorValue(sNextX, sNextY, ubGroupID);

        // call arrive event
        GroupArrivedAtSector(ubGroupID, fCheckForBattle, false);
    }



    // ARM: centralized it so we can do a comprehensive Debug.Assert on it.  Causing problems with helicopter group!
    void SetGroupArrivalTime(GROUP? pGroup, uint uiArrivalTime)
    {
        // PLEASE CENTRALIZE ALL CHANGES TO THE ARRIVAL TIMES OF GROUPS THROUGH HERE, ESPECIALLY THE HELICOPTER GROUP!!!

        // if this group is the helicopter group, we have to make sure that its arrival time is never greater than the sum
        // of the current time and its traverse time, 'cause those 3 values are used to plot its map position!  Because of this
        // the chopper groups must NEVER be delayed for any reason - it gets excluded from simultaneous arrival logic

        // Also note that non-chopper groups can currently be delayed such that this assetion would fail - enemy groups by
        // DelayEnemyGroupsIfPathsCross(), and player groups via PrepareGroupsForSimultaneousArrival().  So we skip the assert.

        if (IsGroupTheHelicopterGroup(pGroup))
        {
            // make sure it's valid (NOTE: the correct traverse time must be set first!)
            if (uiArrivalTime > (GameClock.GetWorldTotalMin() + pGroup.uiTraverseTime))
            {
                //AssertMsg(false, String("SetGroupArrivalTime: Setting invalid arrival time %d for group %d, WorldTime = %d, TraverseTime = %d", uiArrivalTime, pGroup.ubGroupID, GetWorldTotalMin(), pGroup.uiTraverseTime));

                // fix it if assertions are disabled
                uiArrivalTime = GameClock.GetWorldTotalMin() + pGroup.uiTraverseTime;
            }
        }

        pGroup.uiArrivalTime = uiArrivalTime;
    }



    // non-persistent groups should be simply removed instead!
    void CancelEmptyPersistentGroupMovement(GROUP? pGroup)
    {
        Debug.Assert(pGroup is not null);
        Debug.Assert(pGroup.ubGroupSize == 0);
        Debug.Assert(pGroup.fPersistant);


        // don't do this for vehicle groups - the chopper can keep flying empty, 
        // while other vehicles still exist and teleport to nearest sector instead
        if (pGroup.fVehicle)
        {
            return;
        }

        // prevent it from arriving empty
        GameEvents.DeleteStrategicEvent(EVENT.GROUP_ARRIVAL, pGroup.ubGroupID);

        // release memory for its waypoints
        RemoveGroupWaypoints(pGroup.ubGroupID);

        pGroup.uiTraverseTime = 0;
        SetGroupArrivalTime(pGroup, 0);
        pGroup.fBetweenSectors = false;

        pGroup.ubPrevX = 0;
        pGroup.ubPrevY = 0;
        pGroup.ubSectorX = 0;
        pGroup.ubSectorY = 0;
        pGroup.ubNextX = 0;
        pGroup.ubNextY = 0;
    }



    // look for NPCs to stop for, anyone is too tired to keep going, if all OK rebuild waypoints & continue movement
    void PlayerGroupArrivedSafelyInSector(GROUP? pGroup, bool fCheckForNPCs)
    {
        bool fPlayerPrompted = false;


        Debug.Assert(pGroup);
        Debug.Assert(pGroup.fPlayer);


        // if we haven't already checked for NPCs, and the group isn't empty 
        if (fCheckForNPCs && (HandlePlayerGroupEnteringSectorToCheckForNPCsOfNote(pGroup) == true))
        {
            // wait for player to answer/confirm prompt before doing anything else
            fPlayerPrompted = true;
        }

        // if we're not prompting the player
        if (!fPlayerPrompted)
        {
            // and we're not at the end of our road
            if (!GroupAtFinalDestination(pGroup))
            {
                if (AnyMercInGroupCantContinueMoving(pGroup))
                {
                    // stop: clear their strategic movement (mercpaths and waypoints)
                    ClearMercPathsAndWaypointsForAllInGroup(pGroup);

                    // NOTE: Of course, it would be better if they continued onwards once everyone was ready to go again, in which
                    // case we'd want to preserve the plotted path, but since the player can mess with the squads, etc.
                    // in the mean-time, that just seemed to risky to try to support.  They could get into a fight and be too
                    // injured to move, etc.  Basically, we'd have run a complete CanCharacterMoveInStrategic(0 check on all of them.
                    // It's a wish list task for AM...

                    // stop time so player can react if group was already on the move and suddenly halts
                    StopTimeCompression();
                }
                else
                {
                    // continue onwards: rebuild way points, initiate movement
                    RebuildWayPointsForGroupPath(GetGroupMercPathPtr(pGroup), pGroup.ubGroupID);
                }
            }
        }
    }



    bool HandlePlayerGroupEnteringSectorToCheckForNPCsOfNote(GROUP? pGroup)
    {
        int sSectorX = 0, sSectorY = 0;
        int bSectorZ = 0;
        string sString;
        string wSectorName;
        int sStrategicSector;


        Debug.Assert(pGroup);
        Debug.Assert(pGroup.fPlayer);

        // nobody in the group (perfectly legal with the chopper)
        if (pGroup.pPlayerList == null)
        {
            return (false);
        }

        // chopper doesn't stop for NPCs
        if (IsGroupTheHelicopterGroup(pGroup))
        {
            return (false);
        }

        // if we're already in the middle of a prompt (possible with simultaneously group arrivals!), don't try to prompt again
        if (gpGroupPrompting != null)
        {
            return (false);
        }


        // get the sector values
        sSectorX = pGroup.ubSectorX;
        sSectorY = pGroup.ubSectorY;
        bSectorZ = pGroup.ubSectorZ;


        // don't do this for underground sectors
        if (bSectorZ != 0)
        {
            return (false);
        }

        // get the strategic sector value
        sStrategicSector = sSectorX + MAP_WORLD_X * sSectorY;

        // skip towns/pseudo-towns (anything that shows up on the map as being special)
        if (StrategicMap[sStrategicSector].bNameId != BLANK_SECTOR)
        {
            return (false);
        }

        // skip SAM-sites
        if (IsThisSectorASAMSector(sSectorX, sSectorY, bSectorZ))
        {
            return (false);
        }


        // check for profiled NPCs in sector
        if (WildernessSectorWithAllProfiledNPCsNotSpokenWith(sSectorX, sSectorY, bSectorZ) == false)
        {
            return (false);
        }


        // store the group ptr for use by the callback function
        gpGroupPrompting = pGroup;

        // build string for squad
        GetSectorIDString(sSectorX, sSectorY, bSectorZ, wSectorName, false);
        wprintf(sString, pLandMarkInSectorString[0], pGroup.pPlayerList.pSoldier.bAssignment + 1, wSectorName);

        if (GroupAtFinalDestination(pGroup))
        {
            // do an OK message box
            DoScreenIndependantMessageBox(sString, MSG_BOX_FLAG_OK, HandlePlayerGroupEnteringSectorToCheckForNPCsOfNoteCallback);
        }
        else
        {
            // do a CONTINUE/STOP message box
            DoScreenIndependantMessageBox(sString, MSG_BOX_FLAG_CONTINUESTOP, HandlePlayerGroupEnteringSectorToCheckForNPCsOfNoteCallback);
        }

        // wait, we're prompting the player
        return (true);
    }


    bool WildernessSectorWithAllProfiledNPCsNotSpokenWith(int sSectorX, MAP_ROW sSectorY, int bSectorZ)
    {
        NPCID ubProfile;
        MERCPROFILESTRUCT? pProfile;
        bool fFoundSomebody = false;


        for (ubProfile = FIRST_RPC; ubProfile < NUM_PROFILES; ubProfile++)
        {
            pProfile = gMercProfiles[ubProfile];

            // skip stiffs
            if ((pProfile.bMercStatus == MercStatus.MERC_IS_DEAD) || (pProfile.bLife <= 0))
            {
                continue;
            }

            // skip vehicles
            if (ubProfile >= NPCID.PROF_HUMMER && ubProfile <= NPCID.PROF_HELICOPTER)
            {
                continue;
            }

            // in this sector?
            if (pProfile.sSectorX == sSectorX && pProfile.sSectorY == sSectorY && pProfile.bSectorZ == bSectorZ)
            {
                // if we haven't talked to him yet, and he's not currently recruired/escorted by player (!)
                if ((pProfile.ubLastDateSpokenTo == 0) &&
                        !(pProfile.ubMiscFlags & (PROFILE_MISC_FLAG_RECRUITED | PROFILE_MISC_FLAG_EPCACTIVE)))
                {
                    // then this is a guy we need to stop for...
                    fFoundSomebody = true;
                }
                else
                {
                    // already spoke to this guy, don't prompt about this sector again, regardless of status of other NPCs here
                    // (although Hamous wanders around, he never shares the same wilderness sector as other important NPCs)
                    return (false);
                }
            }
        }


        return (fFoundSomebody);
    }



    void HandlePlayerGroupEnteringSectorToCheckForNPCsOfNoteCallback(MessageBoxReturnCode ubExitValue)
    {
        Debug.Assert(gpGroupPrompting is not null);

        if ((ubExitValue == MessageBoxReturnCode.MSG_BOX_RETURN_YES) ||
                 (ubExitValue == MessageBoxReturnCode.MSG_BOX_RETURN_OK))
        {
            // NPCs now checked, continue moving if appropriate
            PlayerGroupArrivedSafelyInSector(gpGroupPrompting, false);
        }
        else if (ubExitValue == MessageBoxReturnCode.MSG_BOX_RETURN_NO)
        {
            // stop here

            // clear their strategic movement (mercpaths and waypoints)
            ClearMercPathsAndWaypointsForAllInGroup(gpGroupPrompting);

            //		// if currently selected sector has nobody in it
            //		if ( PlayerMercsInSector( ( int ) sSelMapX, ( int ) sSelMapY, ( int ) iCurrentMapSectorZ ) == 0 )
            // New: ALWAYS make this sector strategically selected, even if there were mercs in the previously selected one
            {
                ChangeSelectedMapSector(gpGroupPrompting.ubSectorX, gpGroupPrompting.ubSectorY, gpGroupPrompting.ubSectorZ);
            }

            StopTimeCompression();
        }

        gpGroupPrompting = null;

        fMapPanelDirty = true;
        fMapScreenBottomDirty = true;

        return;
    }


    bool DoesPlayerExistInPGroup(int ubGroupID, SOLDIERTYPE? pSoldier)
    {
        GROUP? pGroup;
        PLAYERGROUP? curr;

        pGroup = GetGroup(ubGroupID);

        return pGroup.pPlayerList.Any(ps => ps.pSoldier == pSoldier);
    }


    public static bool GroupHasInTransitDeadOrPOWMercs(GROUP? pGroup)
    {
        PLAYERGROUP? pPlayer;

        pPlayer = pGroup.pPlayerList;
        while (pPlayer)
        {
            if (pPlayer.pSoldier)
            {
                if ((pPlayer.pSoldier.bAssignment == Assignments.IN_TRANSIT) ||
                        (pPlayer.pSoldier.bAssignment == Assignments.ASSIGNMENT_POW) ||
                        (pPlayer.pSoldier.bAssignment == Assignments.ASSIGNMENT_DEAD))
                {
                    // yup!
                    return (true);
                }
            }

            pPlayer = pPlayer.next;
        }

        // nope
        return (false);
    }

    int NumberMercsInVehicleGroup(GROUP? pGroup)
    {
        int iVehicleID;
        iVehicleID = GivenMvtGroupIdFindVehicleId(pGroup.ubGroupID);
        Debug.Assert(iVehicleID != -1);
        if (iVehicleID != -1)
        {
            return (int)GetNumberInVehicle(iVehicleID);
        }
        return 0;
    }
}

public enum StrategicMove
{
    NORTH,
    EAST,
    SOUTH,
    WEST,
    THROUGH,
};

public class PLAYERGROUP
{
    public NPCID ubProfileID;                      //SAVE THIS VALUE ONLY.  The others are temp (for quick access)
    public int ubID;                                     //index in the Menptr array
    public SOLDIERTYPE? pSoldier;              //direct access to the soldier pointer
    public int bFlags;                                   //flags referring to individual player soldiers
    public PLAYERGROUP? next;			//next player in list
}

//This structure contains all of the information about a group moving in the strategic
//layer.  This includes all troops, equipment, and waypoints, and location.
//NOTE:  This is used for groups that are initiating a movement to another sector.
public class WAYPOINT
{
    public int x;
    public MAP_ROW y;
    public WAYPOINT? next;
}

public record ENEMYGROUP(
    int ubNumTroops,                      //number of regular troops in the group
    int ubNumElites,                      //number of elite troops in the group
    int ubNumAdmins,                      //number of administrators in the group
    int ubLeaderProfileID,            //could be Mike, maybe the warden... someone new, but likely nobody.
    int ubPendingReinforcements,//This group is waiting for reinforcements before attacking or attempting to fortify newly aquired sector.
    int ubAdminsInBattle,            //number of administrators in currently in battle.
    ENEMY_INTENTIONS ubIntention,                      //the type of group this is:  patrol, assault, spies, etc.
    int ubTroopsInBattle,             //number of soldiers currently in battle.
    int ubElitesInBattle,             //number of elite soldiers currently in battle.
    int[] bPadding);


public enum ENEMY_INTENTIONS//enemy intentions,
{
    NO_INTENTIONS,          //enemy intentions are undefined.
    PURSUIT,                        //enemy group has spotted a player group and is pursuing them.  If they lose the player group, they
                                    //will get reassigned.
    STAGING,                        //enemy is prepare to assault a town sector, but doesn't have enough troops.
    PATROL,                         //enemy is moving around determining safe areas.
    REINFORCEMENTS,         //enemy group has intentions to fortify position at final destination.
    ASSAULT,                        //enemy is ready to fight anything they encounter.
    NUM_ENEMY_INTENTIONS
};

public enum MOVE_TYPES//move types
{
    ONE_WAY,                        //from first waypoint to last, deleting each waypoint as they are reached.
    CIRCULAR,                       //from first to last, recycling forever.
    ENDTOEND_FORWARDS,  //from first to last -- when reaching last, change to backwards.
    ENDTOEND_BACKWARDS	//from last to first -- when reaching first, change to forwards.
};
