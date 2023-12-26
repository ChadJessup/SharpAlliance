using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;
using SharpAlliance.Platform.Interfaces;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class Squads
{
    public static bool fExitingVehicleToSquad = false;

    private static IFileManager files;
    public Squads(IFileManager fileManager) => files = fileManager;


    public static SquadEnum CurrentSquad()
    {
        // returns which squad is current squad

        return iCurrentTacticalSquad;
    }

    public static int NumberOfPeopleInSquad(SquadEnum bSquadValue)
    {
        int bCounter = 0;
        int bSquadCount = 0;

        if (bSquadValue == NO_CURRENT_SQUAD)
        {
            return 0;
        }

        // find number of characters in particular squad.
        for (bCounter = 0; bCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; bCounter++)
        {

            // valid slot?
            if (Squad[bSquadValue][bCounter] != null)
            {
                // yep
                bSquadCount++;
            }
        }

        // return number found
        return bSquadCount;
    }

    void InitSquads()
    {
        // init the squad lists to null ptrs.
        int iCounterB = 0;
        SquadEnum iCounter = 0;
        GROUP? pGroup = null;

        // null each list of ptrs.
        for (iCounter = 0; iCounter < SquadEnum.NUMBER_OF_SQUADS; iCounter++)
        {
            for (iCounterB = 0; iCounterB < NUMBER_OF_SOLDIERS_PER_SQUAD; iCounterB++)
            {

                // squad, soldier 
                Squad[iCounter][iCounterB] = null;

            }

            // create mvt groups
            SquadMovementGroups[iCounter] = StrategicMovement.CreateNewPlayerGroupDepartingFromSector(1, MAP_ROW.A);

            // Set persistent....
            pGroup = StrategicMovement.GetGroup(SquadMovementGroups[iCounter]);
            pGroup.fPersistant = true;

        }

        //memset(sDeadMercs, -1, sizeof(int) * NUMBER_OF_SQUADS * NUMBER_OF_SOLDIERS_PER_SQUAD);

        return;
    }

    public static bool IsThisSquadFull(SquadEnum bSquadValue)
    {
        int iCounter = 0;

        // run through entries in the squad list, make sure there is a free entry
        for (iCounter = 0; iCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; iCounter++)
        {
            // check this slot
            if (Squad[bSquadValue][iCounter] == null)
            {
                // a free slot found - not full
                return false;
            }
        }

        // no free slots - it's full
        return true;
    }

    public static SquadEnum GetFirstEmptySquad()
    {
        SquadEnum ubCounter = 0;

        for (ubCounter = 0; ubCounter < SquadEnum.NUMBER_OF_SQUADS; ubCounter++)
        {
            if (SquadIsEmpty(ubCounter) == true)
            {
                // empty squad, return value
                return ubCounter;
            }
        }

        // not found - none are completely empty (shouldn't ever happen!)
        Debug.Assert(false);
        return (SquadEnum)(-1);
    }

    public static bool AddCharacterToSquad(SOLDIERTYPE? pCharacter, SquadEnum bSquadValue)
    {
        int bCounter = 0;
        //	bool fBetweenSectors = false;
        GROUP? pGroup;
        bool fNewSquad;


        // add character to squad...return success or failure
        // run through list of people in squad, find first free slo

        if (fExitingVehicleToSquad)
        {
            return false;
        }


        // ATE: If any vehicle exists in this squad AND we're not set to 
        // a driver or or passenger, when return false
        if (DoesVehicleExistInSquad(bSquadValue))
        {
            // We're not allowing anybody to go on a vehicle if they are not passengers!
            // NB: We obviously need to make sure that REAL passengers have their
            // flags set before adding them to a squad!
            if (!pCharacter.uiStatusFlags.HasFlag(SOLDIER.PASSENGER | SOLDIER.DRIVER | SOLDIER.VEHICLE))
            {
                return false;
            }
        }



        // if squad is on the move, can't add someone
        if (IsThisSquadOnTheMove(bSquadValue) == true)
        {
            // nope, go away now
            return false;
        }



        for (bCounter = 0; bCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; bCounter++)
        {
            // check if on current squad and current slot?
            if (Squad[bSquadValue][bCounter] == pCharacter)
            {
                // 'successful of sorts, if there, then he's 'added'
                return true;
            }

            // free slot, add here
            if (Squad[bSquadValue][bCounter] == null)
            {
                // check if squad empty, if not check sector x,y,z are the same as this guys
                if (SquadIsEmpty(bSquadValue) == false)
                {
                    GetLocationOfSquad(out int sX, out MAP_ROW sY, out int bZ, bSquadValue);

                    // if not same, return false
                    if ((pCharacter.sSectorX != sX) || (pCharacter.sSectorY != sY) || (pCharacter.bSectorZ != bZ))
                    {
                        return false;
                    }
                    // remove them
                    RemoveCharacterFromSquads(pCharacter);

                    //				fBetweenSectors =  Squad[ bSquadValue ][ 0 ].fBetweenSectors;
                }
                else
                {
                    // remove them
                    RemoveCharacterFromSquads(pCharacter);
                }

                /*
                            if( fBetweenSectors == true )
                            {
                                pCharacter.fBetweenSectors = true;
                            }
                */

                // copy path of squad to this char
                CopyPathOfSquadToCharacter(pCharacter, bSquadValue);

                // check if old mvt group
                if (pCharacter.ubGroupID != 0)
                {
                    // in valid group, remove from that group
                    StrategicMovement.RemovePlayerFromGroup(pCharacter.ubGroupID, pCharacter);

                    // character not on a reserved group
                    if ((pCharacter.bAssignment >= Assignments.ON_DUTY) && (pCharacter.bAssignment != Assignments.VEHICLE))
                    {
                        // get the group from the character
                        pGroup = StrategicMovement.GetGroup(pCharacter.ubGroupID);

                        // if valid group, delete it
                        if (pGroup is not null)
                        {
                            StrategicMovement.RemoveGroupFromList(pGroup);
                        }
                    }

                }

                if ((pCharacter.bAssignment == Assignments.VEHICLE) && (pCharacter.iVehicleId == iHelicopterVehicleId) && (pCharacter.iVehicleId != -1))
                {
                    // if creating a new squad from guys exiting the chopper			
                    fNewSquad = SquadIsEmpty(bSquadValue);

//                    RemoveSoldierFromHelicopter(pCharacter);

                    StrategicMovement.AddPlayerToGroup(SquadMovementGroups[bSquadValue], pCharacter);
                    StrategicMovement.SetGroupSectorValue(pCharacter.sSectorX, pCharacter.sSectorY, pCharacter.bSectorZ, SquadMovementGroups[bSquadValue]);
                    pCharacter.ubGroupID = SquadMovementGroups[bSquadValue];

                    // if we've just started a new squad
                    if (fNewSquad)
                    {
                        // grab group
                        pGroup = StrategicMovement.GetGroup(pVehicleList[iHelicopterVehicleId].ubMovementGroup);
                        Debug.Assert(pGroup is not null);

                        if (pGroup is not null)
                        {
                            // set where it is and where it's going, then make it arrive there.  Don't check for battle
                            StrategicMovement.PlaceGroupInSector(SquadMovementGroups[bSquadValue], pGroup.ubPrevX, pGroup.ubPrevY, pGroup.ubSectorX, pGroup.ubSectorY, pGroup.ubSectorZ, false);
                        }
                    }
                }
                else if ((pCharacter.bAssignment == Assignments.VEHICLE) && (pCharacter.iVehicleId != -1))
                {
                    fExitingVehicleToSquad = true;
                    // remove from vehicle
//                    TakeSoldierOutOfVehicle(pCharacter);
                    fExitingVehicleToSquad = false;


                    StrategicMovement.AddPlayerToGroup(SquadMovementGroups[bSquadValue], pCharacter);
                    StrategicMovement.SetGroupSectorValue(pCharacter.sSectorX, pCharacter.sSectorY, pCharacter.bSectorZ, SquadMovementGroups[bSquadValue]);
                    pCharacter.ubGroupID = SquadMovementGroups[bSquadValue];
                }
                else
                {
                    StrategicMovement.AddPlayerToGroup(SquadMovementGroups[bSquadValue], pCharacter);
                    StrategicMovement.SetGroupSectorValue(pCharacter.sSectorX, pCharacter.sSectorY, pCharacter.bSectorZ, SquadMovementGroups[bSquadValue]);
                    pCharacter.ubGroupID = SquadMovementGroups[bSquadValue];
                }


                // assign here
                Squad[bSquadValue][bCounter] = pCharacter;

                if (pCharacter.bAssignment != (Assignments)bSquadValue)
                {
                    // check to see if we should wake them up
                    if (pCharacter.fMercAsleep)
                    {
                        // try to wake him up
//                        SetMercAwake(pCharacter, false, false);
                    }

//                    SetTimeOfAssignmentChangeForMerc(pCharacter);
                }

                // set squad value
//                ChangeSoldiersAssignment(pCharacter, bSquadValue);
                if (pCharacter.bOldAssignment < Assignments.ON_DUTY)
                {
                    pCharacter.bOldAssignment = (Assignments)bSquadValue;
                }

                // if current tactical sqaud...upadte panel
                if (NumberOfPeopleInSquad(iCurrentTacticalSquad) == 0)
                {
                    SetCurrentSquad(bSquadValue, true);
                }

                if (bSquadValue == iCurrentTacticalSquad)
                {
//                    CheckForAndAddMercToTeamPanel(Squad[iCurrentTacticalSquad][bCounter]);
                }

                if (pCharacter.ubID == gusSelectedSoldier)
                {
                    SetCurrentSquad(bSquadValue, true);
                }


                return true;
            }
        }

        return false;
    }


    // find the first slot we can fit the guy in
    public static bool AddCharacterToAnySquad(SOLDIERTYPE? pCharacter)
    {
        // add character to any squad, if character is assigned to a squad, returns true
        SquadEnum bCounter = 0;
        SquadEnum bFirstEmptySquad = SquadEnum.UNSET;


        // remove them from current squad
        RemoveCharacterFromSquads(pCharacter);

        // first look for a compatible NON-EMPTY squad (don't start new squad if we don't have to)
        for (bCounter = 0; bCounter < NUMBER_OF_SQUADS; bCounter++)
        {
            if (SquadIsEmpty(bCounter) == false)
            {
                if (AddCharacterToSquad(pCharacter, bCounter) == true)
                {
                    return true;
                }
            }
            else
            {
                if (bFirstEmptySquad == SquadEnum.UNSET)
                {
                    bFirstEmptySquad = bCounter;
                }
            }
        }

        // no non-empty compatible squads were found

        // try the first empty one (and there better be one)
        if (bFirstEmptySquad != SquadEnum.UNSET)
        {
            if (AddCharacterToSquad(pCharacter, bFirstEmptySquad) == true)
            {
                return true;
            }
        }

        // should never happen!
        Debug.Assert(false);
        return false;
    }

    // find the first slot we can fit the guy in
    public static SquadEnum AddCharacterToUniqueSquad(SOLDIERTYPE? pCharacter)
    {
        // add character to any squad, if character is assigned to a squad, returns true
        SquadEnum bCounter = 0;

        // check if character on a squad

        // remove them
        RemoveCharacterFromSquads(pCharacter);

        for (bCounter = 0; bCounter < NUMBER_OF_SQUADS; bCounter++)
        {
            if (SquadIsEmpty(bCounter) == true)
            {
                if (AddCharacterToSquad(pCharacter, bCounter) == true)
                {
                    return bCounter;
                }
            }
        }

        return SquadEnum.UNSET;
    }

    public static bool SquadIsEmpty(SquadEnum bSquadValue)
    {
        // run through this squad's slots and find if they ALL are empty
        int iCounter = 0;

        for (iCounter = 0; iCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; iCounter++)
        {
            if (Squad[bSquadValue][iCounter] != null)
            {
                return false;
            }
        }

        return true;
    }



    // find and remove characters from any squad
    public static bool RemoveCharacterFromSquads(SOLDIERTYPE? pCharacter)
    {
        SquadEnum iCounterA = 0;
        int iCounter = 0;
        int ubGroupId = 0;
        // find character and remove.. check characters in all squads


        // squad?
        for (iCounterA = 0; iCounterA < SquadEnum.NUMBER_OF_SQUADS; iCounterA++)
        {
            // slot?
            for (iCounter = 0; iCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; iCounter++)
            {

                // check if on current squad and current slot?
                if (Squad[iCounterA][iCounter] == pCharacter)
                {


                    // found and nulled
                    Squad[iCounterA][iCounter] = null;

                    // Release memory for his personal path, BUT DON'T CLEAR HIS GROUP'S PATH/WAYPOINTS (pass in groupID -1).
                    // Just because one guy leaves a group is no reason to cancel movement for the rest of the group.
//                    pCharacter.pMercPath = ClearStrategicPathList(pCharacter.pMercPath, -1);

                    // remove character from mvt group
                    StrategicMovement.RemovePlayerFromGroup(SquadMovementGroups[iCounterA], pCharacter);

                    // reset player mvt group id value
                    pCharacter.ubGroupID = 0;

                    if (pCharacter.fBetweenSectors && pCharacter.uiStatusFlags.HasFlag(SOLDIER.VEHICLE))
                    {
                        ubGroupId = StrategicMovement.CreateNewPlayerGroupDepartingFromSector(pCharacter.sSectorX, pCharacter.sSectorY);

                        // assign to a group
                        StrategicMovement.AddPlayerToGroup(ubGroupId, pCharacter);
                    }

                    RebuildSquad(iCounterA);

                    if (pCharacter.bLife == 0)
                    {
                        AddDeadCharacterToSquadDeadGuys(pCharacter, iCounterA);
                    }

                    //if we are not loading a saved game
                    if (!gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.LOADING_SAVED_GAME)
                        && guiCurrentScreen == ScreenName.GAME_SCREEN)
                    {
                        UpdateCurrentlySelectedMerc(pCharacter, iCounterA);
                    }

                    return true;
                }
            }
        }

        // not found
        return false;
    }

    public static bool RemoveCharacterFromASquad(SOLDIERTYPE? pCharacter, SquadEnum bSquadValue)
    {
        int iCounter = 0;
        SquadEnum iCounterA = 0;

        // remove character from particular squad..return if successful
        for (iCounter = 0; iCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; iCounter++)
        {
            // check if on current squad and current slot?
            if (Squad[bSquadValue][iCounter] == pCharacter)
            {

                UpdateCurrentlySelectedMerc(pCharacter, bSquadValue);

                // found and nulled
                Squad[bSquadValue][iCounter] = null;

                // remove character from mvt group
//                RemovePlayerFromGroup(SquadMovementGroups[bSquadValue], pCharacter);


                if (pCharacter.bLife == 0)
                {
                    AddDeadCharacterToSquadDeadGuys(pCharacter, iCounterA);
                }

                RebuildSquad(bSquadValue);


                // found
                return true;
            }
        }

        // not found
        return false;
    }

    public static bool IsCharacterInSquad(SOLDIERTYPE? pCharacter, SquadEnum bSquadValue)
    {
        int iCounter = 0;
        // find character in particular squad..return if successful
        for (iCounter = 0; iCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; iCounter++)
        {
            // check if on current squad and current slot?
            if (Squad[bSquadValue][iCounter] == pCharacter)
            {
                // found
                return true;
            }
        }

        // not found
        return false;
    }

    public static int SlotCharacterIsInSquad(SOLDIERTYPE? pCharacter, SquadEnum bSquadValue)
    {
        int bCounter = 0;

        // find character in particular squad..return slot if successful, else -1
        for (bCounter = 0; bCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; bCounter++)
        {
            // check if on current squad and current slot?
            if (Squad[bSquadValue][bCounter] == pCharacter)
            {
                // found
                return bCounter;
            }
        }

        // not found
        return -1;
    }

    public static SquadEnum SquadCharacterIsIn(SOLDIERTYPE? pCharacter)
    {
        // returns which squad character is in, -1 if none found
        SquadEnum iCounterA = 0;
        int iCounter = 0;

        // squad?
        for (iCounterA = 0; iCounterA < NUMBER_OF_SQUADS; iCounterA++)
        {
            // slot?
            for (iCounter = 0; iCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; iCounter++)
            {

                // check if on current squad and current slot?
                if (Squad[iCounterA][iCounter] == pCharacter)
                {
                    // return value
                    return iCounterA;
                }
            }
        }

        // return failure
        return SquadEnum.UNSET;
    }

    int NumberOfNonEPCsInSquad(SquadEnum bSquadValue)
    {
        int bCounter = 0;
        int bSquadCount = 0;

        if (bSquadValue == NO_CURRENT_SQUAD)
        {
            return 0;
        }

        // find number of characters in particular squad.
        for (bCounter = 0; bCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; bCounter++)
        {

            // valid slot?
            if (Squad[bSquadValue][bCounter] != null && !AM_AN_EPC(Squad[bSquadValue][bCounter]))
            {
                // yep
                bSquadCount++;
            }
        }

        // return number found
        return bSquadCount;
    }

    bool IsRobotControllerInSquad(SquadEnum bSquadValue)
    {
        int bCounter = 0;

        if (bSquadValue == NO_CURRENT_SQUAD)
        {
            return false;
        }

        // find number of characters in particular squad.
        for (bCounter = 0; bCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; bCounter++)
        {
            // valid slot?
            if ((Squad[bSquadValue][bCounter] != null) && SoldierControl.ControllingRobot(Squad[bSquadValue][bCounter]))
            {
                // yep
                return true;
            }
        }

        // return number found
        return false;
    }

    public static bool SectorSquadIsIn(SquadEnum bSquadValue, out int sMapX, out MAP_ROW sMapY, out int sMapZ)
    {
        // returns if there is anyone on the squad and what sector ( strategic ) they are in
        int bCounter = 0;
        sMapX = 0;
        sMapY = 0;
        sMapZ = 0;

        Debug.Assert(bSquadValue < SquadEnum.ON_DUTY);

        for (bCounter = 0; bCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; bCounter++)
        {
            // if valid soldier, get current sector and return
            if (Squad[bSquadValue][bCounter] != null)
            {
                sMapX = Squad[bSquadValue][bCounter].sSectorX;
                sMapY = Squad[bSquadValue][bCounter].sSectorY;
                sMapZ = Squad[bSquadValue][bCounter].bSectorZ;

                return true;
            }

        }

        // return there is no squad
        return false;
    }


    public static bool CopyPathOfSquadToCharacter(SOLDIERTYPE? pCharacter, SquadEnum bSquadValue)
    {
        // copy path from squad to character
        int bCounter = 0;

        for (bCounter = 0; bCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; bCounter++)
        {
            if ((Squad[bSquadValue][bCounter] != pCharacter) && (Squad[bSquadValue][bCounter] != null))
            {
                // valid character, copy paths
//                pCharacter.pMercPath = CopyPaths(Squad[bSquadValue][bCounter].pMercPath, pCharacter.pMercPath);

                // return success
                return true;
            }
        }

        // return failure
        return false;
    }


    bool CopyPathOfCharacterToSquad(SOLDIERTYPE? pCharacter, SquadEnum bSquadValue)
    {
        // copy path of this character to members of squad
        bool fSuccess = false;
        int bCounter = 0;

        // anyone else on squad?
        if (NumberOfPeopleInSquad(bSquadValue) < 2)
        {
            // nope

            // return failure
            return false;
        }

        // copy each person on squad, skip this character
        for (bCounter = 0; bCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; bCounter++)
        {
            if ((Squad[bSquadValue][bCounter] != pCharacter) && (Squad[bSquadValue][bCounter] != null))
            {
                // valid character, copy paths

                // first empty path
//                Squad[bSquadValue][bCounter].pMercPath = ClearStrategicPathList(Squad[bSquadValue][bCounter].pMercPath, -1);

                // then copy
//                Squad[bSquadValue][bCounter].pMercPath = CopyPaths(pCharacter.pMercPath, Squad[bSquadValue][bCounter].pMercPath);

                // successful at least once
                fSuccess = true;
            }
        }

        // return success?
        return fSuccess;
    }

    public static bool SetCurrentSquad(SquadEnum iCurrentSquad, bool fForce)
    {
        // set the current tactical squad
        int iCounter = 0;


        // ARM: can't call SetCurrentSquad() in mapscreen, it calls SelectSoldier(), that will initialize interface panels!!!
        // ATE: Adjusted conditions a bit ( sometimes were not getting selected )
        if (guiCurrentScreen == ScreenName.LAPTOP_SCREEN || guiCurrentScreen == ScreenName.MAP_SCREEN)
        {
            return false;
        }

        // ATE; Added to allow us to have NO current squad
        if (iCurrentSquad == NO_CURRENT_SQUAD)
        {
            // set current squad and return success
            iCurrentTacticalSquad = iCurrentSquad;

            // cleat list
            InterfacePanel.RemoveAllPlayersFromSlot();

            // set all auto faces inactive
            Faces.SetAllAutoFacesInactive();

            return false;
        }


        // check if valid value passed
        if ((iCurrentSquad >= NUMBER_OF_SQUADS) || (iCurrentSquad < 0))
        {
            // no
            return false;
        }

        // check if squad is current
        if (iCurrentSquad == iCurrentTacticalSquad && !fForce)
        {
            return true;
        }

        // set current squad and return success
        iCurrentTacticalSquad = iCurrentSquad;

        // cleat list
        InterfacePanel.RemoveAllPlayersFromSlot();

        // set all auto faces inactive
        Faces.SetAllAutoFacesInactive();

        if (iCurrentTacticalSquad != NO_CURRENT_SQUAD)
        {
            for (iCounter = 0; iCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; iCounter++)
            {
                if (Squad[iCurrentTacticalSquad][iCounter] != null)
                {
                    // squad set, now add soldiers in 
//                    CheckForAndAddMercToTeamPanel(Squad[iCurrentTacticalSquad][iCounter]);
                }
            }
        }

        // check if the currently selected guy is on this squad, if not, get the first one on the new squad
        if (gusSelectedSoldier != NO_SOLDIER)
        {
            if (Menptr[gusSelectedSoldier].bAssignment != (Assignments)iCurrentTacticalSquad)
            {
                // ATE: Changed this to false for ackoledgement sounds.. sounds bad if just starting/entering sector..
//                SelectSoldier(Squad[iCurrentTacticalSquad][0].ubID, false, true);
            }
        }
        else
        {
            // ATE: Changed this to false for ackoledgement sounds.. sounds bad if just starting/entering sector..
//            SelectSoldier(Squad[iCurrentTacticalSquad][0].ubID, false, true);
        }

        return true;
    }

    void RebuildCurrentSquad()
    {
        // rebuilds current squad to reset faces in tactical
        int iCounter = 0;
        int iCount = 0;
        SOLDIERTYPE? pDeadSoldier = null;

        // check if valid value passed
        if ((iCurrentTacticalSquad >= NUMBER_OF_SQUADS) || (iCurrentTacticalSquad < 0))
        {
            // no
            return;
        }

        // set default squad..just inc ase we no longer have a valid squad
        this.SetDefaultSquadOnSectorEntry(true);

        // cleat list
        InterfacePanel.RemoveAllPlayersFromSlot();

        // set all auto faces inactive
        Faces.SetAllAutoFacesInactive();

        gfPausedTacticalRenderInterfaceFlags = DIRTYLEVEL2;

        if (iCurrentTacticalSquad != NO_CURRENT_SQUAD)
        {
            for (iCounter = 0; iCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; iCounter++)
            {
                if (Squad[iCurrentTacticalSquad][iCounter] != null)
                {
                    // squad set, now add soldiers in 
//                    CheckForAndAddMercToTeamPanel(Squad[iCurrentTacticalSquad][iCounter]);
                }
            }

            for (iCounter = 0; iCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; iCounter++)
            {
                if (sDeadMercs[iCurrentTacticalSquad][iCounter] != NPCID.UNSET)
                {
                    pDeadSoldier = SoldierProfileSubSystem.FindSoldierByProfileID(sDeadMercs[iCurrentTacticalSquad][iCounter], true);

                    if (pDeadSoldier is not null)
                    {
                        // squad set, now add soldiers in 
//                        CheckForAndAddMercToTeamPanel(pDeadSoldier);
                    }
                }
            }
        }
    }


    void ExamineCurrentSquadLights()
    {
        // rebuilds current squad to reset faces in tactical
        int iCounter = 0;
        int ubLoop;

        // OK, we should add lights for any guy currently bInSector who is not bad OKLIFE...
        ubLoop = gTacticalStatus.Team[gbPlayerNum].bFirstID;
        for (; ubLoop <= gTacticalStatus.Team[gbPlayerNum].bLastID; ubLoop++)
        {
            if (MercPtrs[ubLoop].bInSector && MercPtrs[ubLoop].bLife >= OKLIFE)
            {
//                PositionSoldierLight(MercPtrs[ubLoop]);
            }
        }


        // check if valid value passed
        //if( ( iCurrentTacticalSquad >= NUMBER_OF_SQUADS ) || ( iCurrentTacticalSquad < 0 ) )
        //{
        // no
        //	return;
        //}

        //for( iCounter = 0; iCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; iCounter++ )
        //{
        //	if(  Squad[ iCurrentTacticalSquad ][ iCounter ] != null )
        //	{
        //		PositionSoldierLight( Squad[ iCurrentTacticalSquad ][ iCounter ] );
        //	}
        //}
    }



    bool GetSoldiersInSquad(SquadEnum iCurrentSquad, SOLDIERTYPE?[] pSoldierArray )
    {
        int iCounter = 0;
        // will get the soldiertype pts for every merc in this squad

        // check if valid value passed
        if ((iCurrentSquad >= NUMBER_OF_SQUADS) || (iCurrentSquad < 0))
        {
            // no
            return false;
        }

        // copy pts values over
        for (iCounter = 0; iCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; iCounter++)
        {
            pSoldierArray[iCounter] = Squad[iCurrentSquad][iCounter];
        }

        return true;
    }


    bool IsSquadOnCurrentTacticalMap(SquadEnum iCurrentSquad)
    {
        int iCounter = 0;
        // check to see if this squad is on the current map

        // check if valid value passed
        if ((iCurrentSquad >= NUMBER_OF_SQUADS) || (iCurrentSquad < 0))
        {
            // no
            return false;
        }

        // go through memebrs of squad...if anyone on this map, return true
        for (iCounter = 0; iCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; iCounter++)
        {
            if (Squad[iCurrentSquad][iCounter] != null)
            {
                // ATE; Added more checks here for being in sector ( fBetweenSectors and SectorZ )
                if ((Squad[iCurrentSquad][iCounter].sSectorX == gWorldSectorX) && (Squad[iCurrentSquad][iCounter].sSectorY == gWorldSectorY) && Squad[iCurrentSquad][iCounter].bSectorZ == gbWorldSectorZ && Squad[iCurrentSquad][iCounter].fBetweenSectors != true)
                {
                    return true;
                }
            }
        }

        return false;
    }


    void SetDefaultSquadOnSectorEntry(bool fForce)
    {
        SquadEnum iCounter = 0;
        // check if selected squad is in current sector, if so, do nothing, if not...first first case that they are

        if (this.IsSquadOnCurrentTacticalMap(iCurrentTacticalSquad) == true)
        {
            // is in sector, leave
            return;
        }

        //otherwise...

        // find first squad availiable
        for (iCounter = 0; iCounter < NUMBER_OF_SQUADS; iCounter++)
        {
            if (this.IsSquadOnCurrentTacticalMap(iCounter) == true)
            {
                // squad in sector...set as current
                SetCurrentSquad(iCounter, fForce);

                return;
            }
        }

        // If here, set to no current squad
        SetCurrentSquad(NO_CURRENT_SQUAD, false);

        return;
    }

    public static SquadEnum GetLastSquadActive()
    {
        // find id of last squad in the list with active mercs in it
        SquadEnum iCounter = 0;
        int iCounterB = 0;
        SquadEnum iLastSquad = 0;

        for (iCounter = 0; iCounter < NUMBER_OF_SQUADS; iCounter++)
        {
            for (iCounterB = 0; iCounterB < NUMBER_OF_SOLDIERS_PER_SQUAD; iCounterB++)
            {
                if (Squad[iCounter][iCounterB] != null)
                {
                    iLastSquad = iCounter;
                }
            }
        }

        return iLastSquad;
    }


    public static void GetSquadPosition(out int ubNextX, out MAP_ROW ubNextY, out int ubPrevX, out MAP_ROW ubPrevY, out uint uiTraverseTime, out uint uiArriveTime, SquadEnum ubSquadValue)
    {
        // grab the mvt group for this squad and find all this information

        if (SquadMovementGroups[ubSquadValue] == 0)
        {
            ubNextX = 0;
            ubNextY = 0;
            ubPrevX = 0;
            ubPrevY = 0;
            uiTraverseTime = 0;
            uiArriveTime = 0;
            return;
        }

        // grab this squads mvt position
        StrategicMovement.GetGroupPosition(
            out ubNextX,
            out ubNextY,
            out ubPrevX,
            out ubPrevY,
            out uiTraverseTime,
            out uiArriveTime,
            SquadMovementGroups[ubSquadValue]);

        return;
    }


    private static void SetSquadPositionBetweenSectors(
        int ubNextX,
        MAP_ROW ubNextY,
        int ubPrevX,
        MAP_ROW ubPrevY,
        int uiTraverseTime,
        int uiArriveTime,
        SquadEnum ubSquadValue)
    {
        // set mvt group position for squad for 

        if (SquadMovementGroups[ubSquadValue] == 0)
        {
            return;
        }

        StrategicMovement.SetGroupPosition(ubNextX, ubNextY, ubPrevX, ubPrevY, uiTraverseTime, uiArriveTime, SquadMovementGroups[ubSquadValue]);

        return;
    }



    public static unsafe bool SaveSquadInfoToSavedGameFile(Stream hFile)
    {
        Dictionary<SquadEnum, List<SAVE_SQUAD_INFO_STRUCT>> sSquadSaveStruct = new();// SAVE_SQUAD_INFO_STRUCT[(int)NUMBER_OF_SQUADS, NUMBER_OF_SOLDIERS_PER_SQUAD];
        int uiSaveSize = 0;
        //Reset the current squad info
        int iCounterB = 0;
        SquadEnum iCounter = 0;


        for (iCounter = 0; iCounter < NUMBER_OF_SQUADS; iCounter++)
        {
            for (iCounterB = 0; iCounterB < NUMBER_OF_SOLDIERS_PER_SQUAD; iCounterB++)
            {
                if (Squad[iCounter][iCounterB] is not null)
                {
                    sSquadSaveStruct[iCounter][iCounterB].uiID = Squad[iCounter][iCounterB].ubID;
                }
                else
                {
                    sSquadSaveStruct[iCounter][iCounterB].uiID = -1;
                }
            }
        }

        //Save the squad info to the Saved Game File
        uiSaveSize = sizeof(SAVE_SQUAD_INFO_STRUCT) * (int)NUMBER_OF_SQUADS * NUMBER_OF_SOLDIERS_PER_SQUAD;

        files.FileWrite(hFile, sSquadSaveStruct, uiSaveSize, out int uiNumBytesWritten);
        if (uiNumBytesWritten != uiSaveSize)
        {
            return false;
        }


        //Save all the squad movement id's
        files.FileWrite(hFile, SquadMovementGroups, sizeof(int) * (int)NUMBER_OF_SQUADS, out uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(int) * (int)NUMBER_OF_SQUADS)
        {
            return false;
        }

        return true;
    }

    public static unsafe bool LoadSquadInfoFromSavedGameFile(Stream hFile)
    {
        Dictionary<SquadEnum, List<SAVE_SQUAD_INFO_STRUCT>> sSquadSaveStruct = new();// SAVE_SQUAD_INFO_STRUCT[(int)NUMBER_OF_SQUADS, NUMBER_OF_SOLDIERS_PER_SQUAD];
        int uiNumBytesRead = 0;
        int uiSaveSize = 0;

        //Reset the current squad info
        int iCounterB = 0;
        SquadEnum iCounter = 0;

        // null each list of ptrs.
        for (iCounter = 0; iCounter < NUMBER_OF_SQUADS; iCounter++)
        {
            for (iCounterB = 0; iCounterB < NUMBER_OF_SOLDIERS_PER_SQUAD; iCounterB++)
            {
                // squad, soldier 
                Squad[iCounter][iCounterB] = null;
            }
        }


        // Load in the squad info
        uiSaveSize = sizeof(SAVE_SQUAD_INFO_STRUCT) * (int)NUMBER_OF_SQUADS * NUMBER_OF_SOLDIERS_PER_SQUAD;

        //files.FileRead<SAVE_SQUAD_INFO_STRUCT[]>(hFile, ref sSquadSaveStruct, uiSaveSize, out uiNumBytesRead);
        if (uiNumBytesRead != uiSaveSize)
        {
            return false;
        }



        // Loop through the array loaded in
        for (iCounter = 0; iCounter < NUMBER_OF_SQUADS; iCounter++)
        {
            for (iCounterB = 0; iCounterB < NUMBER_OF_SOLDIERS_PER_SQUAD; iCounterB++)
            {
                if (sSquadSaveStruct[iCounter][iCounterB].uiID != -1)
                {
                    Squad[iCounter][iCounterB] = Menptr[sSquadSaveStruct[iCounter][iCounterB].uiID];
                }
                else
                {
                    Squad[iCounter][iCounterB] = null;
                }
            }
        }


        //Load in the Squad movement id's
        //files.FileRead(hFile, ref SquadMovementGroups, sizeof(int) * (int)NUMBER_OF_SQUADS, out uiNumBytesRead);
        if (uiNumBytesRead != sizeof(int) * (int)NUMBER_OF_SQUADS)
        {
            return false;
        }

        return true;
    }


    public static void GetLocationOfSquad(out int sX, out MAP_ROW sY, out int bZ, SquadEnum bSquadValue)
    {
        // run through list of guys, once valid merc found, get his sector x and y and z
        int iCounter = 0;
        sX = 0;
        sY = 0;
        bZ = 0;

        for (iCounter = 0; iCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; iCounter++)
        {
            if (Squad[bSquadValue][iCounter] is not null)
            {
                // valid guy
                sX = Squad[bSquadValue][iCounter].sSectorX;
                sY = Squad[bSquadValue][iCounter].sSectorY;
                bZ = Squad[bSquadValue][iCounter].bSectorZ;
            }
        }

        return;
    }

    public static bool IsThisSquadOnTheMove(SquadEnum bSquadValue)
    {
        int iCounter = 0;

        for (iCounter = 0; iCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; iCounter++)
        {
            if (Squad[bSquadValue][iCounter] is not null)
            {
                return Squad[bSquadValue][iCounter].fBetweenSectors;
            }
        }

        return false;
    }

    // rebuild this squad after someone has been removed, to 'squeeze' together any empty spots
    public static void RebuildSquad(SquadEnum bSquadValue)
    {
        int iCounter = 0, iCounterB = 0;

        for (iCounterB = 0; iCounterB < NUMBER_OF_SOLDIERS_PER_SQUAD - 1; iCounterB++)
        {
            for (iCounter = 0; iCounter < NUMBER_OF_SOLDIERS_PER_SQUAD - 1; iCounter++)
            {
                if (Squad[bSquadValue][iCounter] == null)
                {
                    if (Squad[bSquadValue][iCounter + 1] != null)
                    {
                        Squad[bSquadValue][iCounter] = Squad[bSquadValue][iCounter + 1];
                        Squad[bSquadValue][iCounter + 1] = null;
                    }
                }
            }
        }

        return;
    }

    public static void UpdateCurrentlySelectedMerc(SOLDIERTYPE? pSoldier, SquadEnum bSquadValue)
    {
        int ubID = 0;

        // if this squad is the current one and and the psoldier is the currently selected soldier, get rid of 'em
        if (bSquadValue != iCurrentTacticalSquad)
        {
            return;
        }

        // Are we the selected guy?
        if (gusSelectedSoldier == pSoldier.ubID)
        {
//            ubID = FindNextActiveAndAliveMerc(pSoldier, false, false);

            if (ubID != NOBODY && ubID != gusSelectedSoldier)
            {
                Overhead.SelectSoldier(ubID, false, false);
            }
            else
            {
                gusSelectedSoldier = NOBODY;

                // ATE: Make sure we are in TEAM panel at this point!
//                SetCurrentInterfacePanel(TEAM_PANEL);
            }
        }

        return;
    }


    bool IsSquadInSector(SOLDIERTYPE? pSoldier, SquadEnum ubSquad)
    {

        if (pSoldier == null)
        {
            return false;
        }

        if (pSoldier.fBetweenSectors == true)
        {
            return false;
        }

        if (pSoldier.bAssignment == Assignments.IN_TRANSIT)
        {
            return false;
        }

        if (pSoldier.bAssignment == Assignments.ASSIGNMENT_POW)
        {
            return false;
        }

        if (SquadIsEmpty(ubSquad) == true)
        {
            return true;
        }

        if ((pSoldier.sSectorX != Squad[ubSquad][0].sSectorX) || (pSoldier.sSectorY != Squad[ubSquad][0].sSectorY) || (pSoldier.bSectorZ != Squad[ubSquad][0].bSectorZ))
        {
            return false;
        }

        if (Squad[ubSquad][0].fBetweenSectors == true)
        {
            return false;
        }


        return true;
    }


    bool IsAnyMercOnSquadAsleep(SquadEnum ubSquadValue)
    {
        int iCounter = 0;

        if (SquadIsEmpty(ubSquadValue) == true)
        {
            return false;
        }

        for (iCounter = 0; iCounter < NUMBER_OF_SOLDIERS_PER_SQUAD - 1; iCounter++)
        {
            if (Squad[ubSquadValue][iCounter] != null)
            {
                if (Squad[ubSquadValue][iCounter].fMercAsleep)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool AddDeadCharacterToSquadDeadGuys(SOLDIERTYPE? pSoldier, SquadEnum iSquadValue)
    {
        int iCounter = 0;
        SOLDIERTYPE? pTempSoldier = null;

        // is dead guy in any squad
        if (IsDeadGuyOnAnySquad(pSoldier) == true)
        {
            return true;
        }

        // first find out if the guy is in the list
        for (iCounter = 0; iCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; iCounter++)
        {
            // valid soldier?
            if (sDeadMercs[iSquadValue][iCounter] != NPCID.UNSET)
            {
                pTempSoldier = SoldierProfileSubSystem.FindSoldierByProfileID(sDeadMercs[iSquadValue][iCounter], true);

                if (pSoldier == pTempSoldier)
                {
                    return true;
                }
            }
        }


        // now insert the guy
        for (iCounter = 0; iCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; iCounter++)
        {
            // valid soldier?
            if (sDeadMercs[iSquadValue][iCounter] != NPCID.UNSET)
            {
                // yep
                pTempSoldier = SoldierProfileSubSystem.FindSoldierByProfileID(sDeadMercs[iSquadValue][iCounter], true);

                // valid soldier?
                if (pTempSoldier == null)
                {
                    // nope
                    sDeadMercs[iSquadValue][iCounter] = pSoldier.ubProfile;
                    return true;
                }
            }
            else
            {
                // nope
                sDeadMercs[iSquadValue][iCounter] = pSoldier.ubProfile;
                return true;
            }
        }

        // no go
        return false;
    }

    public static bool IsDeadGuyOnAnySquad(SOLDIERTYPE? pSoldier)
    {
        SquadEnum iCounterA = 0;
            int iCounter = 0;

        // squad?
        for (iCounterA = 0; iCounterA < SquadEnum.NUMBER_OF_SQUADS; iCounterA++)
        {
            // slot?
            for (iCounter = 0; iCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; iCounter++)
            {
                if (sDeadMercs[iCounterA][iCounter] == pSoldier.ubProfile)
                {
                    return true;
                }
            }
        }

        return false;
    }

    bool IsDeadGuyInThisSquadSlot(int bSlotId, SquadEnum bSquadValue, ref int bNumberOfDeadGuysSoFar)
    {
        int iCounter = 0, iCount = 0;

        // see if we have gone too far?
        if (bSlotId < bNumberOfDeadGuysSoFar)
        {
            // reset
            bNumberOfDeadGuysSoFar = 0;
        }

        for (iCounter = 0; iCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; iCounter++)
        {
            if (sDeadMercs[bSquadValue][iCounter] != NPCID.UNSET)
            {
                // not gone far enough yet
                if (bNumberOfDeadGuysSoFar > iCounter)
                {
                    iCount++;
                }
                else
                {
                    // far enough, start checking
                    bNumberOfDeadGuysSoFar++;

                    return true;
                }
            }
        }

        return false;
    }

    public static bool SoldierIsDeadAndWasOnSquad(SOLDIERTYPE? pSoldier, SquadEnum bSquadValue)
    {
        int iCounter = 0;

        if (bSquadValue == NO_CURRENT_SQUAD)
        {
            return false;
        }

        // check if guy is on squad
        for (iCounter = 0; iCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; iCounter++)
        {
            if (pSoldier.ubProfile == sDeadMercs[bSquadValue][iCounter])
            {
                return true;
            }
        }

        return false;
    }

    public static bool ResetDeadSquadMemberList(SquadEnum iSquadValue)
    {
        sDeadMercs[iSquadValue] = new();

        return true;
    }


    // this passed  soldier on the current squad int he tactical map
    public static bool IsMercOnCurrentSquad(SOLDIERTYPE? pSoldier)
    {
        int iCounter = 0;

        // valid soldier?
        if (pSoldier == null)
        {
            // no
            return false;
        }

        // active grunt?
        if (pSoldier.bActive == false)
        {
            // no
            return false;
        }

        // current squad valid?
        if (iCurrentTacticalSquad >= NUMBER_OF_SQUADS)
        {
            // no
            return false;
        }


        for (iCounter = 0; iCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; iCounter++)
        {
            if (Squad[iCurrentTacticalSquad][iCounter] == pSoldier)
            {
                // found him
                return true;
            }
        }

        return false;
    }

    int NumberOfPlayerControllableMercsInSquad(SquadEnum bSquadValue)
    {
        SOLDIERTYPE? pSoldier;
        int bCounter = 0;
        int bSquadCount = 0;

        if (bSquadValue == NO_CURRENT_SQUAD)
        {
            return 0;
        }

        // find number of characters in particular squad.
        for (bCounter = 0; bCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; bCounter++)
        {

            // valid slot?
            if (Squad[bSquadValue][bCounter] != null)
            {
                // yep
                pSoldier = Squad[bSquadValue][bCounter];

                //Kris:  This breaks the CLIENT of this function, tactical traversal.  Do NOT check for EPCS or ROBOT here.
                //if ( !AM_AN_EPC( pSoldier ) && !AM_A_ROBOT( pSoldier ) && 
                if (!pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE))
                {
                    bSquadCount++;
                }
            }
        }

        // return number found
        return bSquadCount;
    }


    public static bool DoesVehicleExistInSquad(SquadEnum bSquadValue)
    {
        SOLDIERTYPE? pSoldier;
        int bCounter = 0;
        int bSquadCount = 0;

        if (bSquadValue == NO_CURRENT_SQUAD)
        {
            return false;
        }

        // find number of characters in particular squad.
        for (bCounter = 0; bCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; bCounter++)
        {
            // valid slot?
            if (Squad[bSquadValue][bCounter] != null)
            {
                // yep
                pSoldier = Squad[bSquadValue][bCounter];

                // If we are an EPC or ROBOT, don't allow this
                if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE))
                {
                    return true;
                }
            }
        }

        return false;
    }

    void CheckSquadMovementGroups()
    {
        SquadEnum iSquad;
        GROUP? pGroup;

        for (iSquad = 0; iSquad < SquadEnum.NUMBER_OF_SQUADS; iSquad++)
        {
            pGroup = StrategicMovement.GetGroup(SquadMovementGroups[iSquad]);
            if (pGroup == null)
            {
                // recreate group
                SquadMovementGroups[iSquad] = StrategicMovement.CreateNewPlayerGroupDepartingFromSector(1, MAP_ROW.A);

                // Set persistent....
                pGroup = StrategicMovement.GetGroup(SquadMovementGroups[iSquad]);
                Debug.Assert(pGroup is not null);
                pGroup.fPersistant = true;
            }
        }
    }
}


// enums for squads
public enum SquadEnum
{
    FIRST_SQUAD = 0,
    SECOND_SQUAD,
    THIRD_SQUAD,
    FOURTH_SQUAD,
    FIFTH_SQUAD,
    SIXTH_SQUAD,
    SEVENTH_SQUAD,
    EIGTH_SQUAD,
    NINTH_SQUAD,
    TENTH_SQUAD,
    ELEVENTH_SQUAD,
    TWELTH_SQUAD,
    THIRTEENTH_SQUAD,
    FOURTEENTH_SQUAD,
    FIFTHTEEN_SQUAD,
    SIXTEENTH_SQUAD,
    SEVENTEENTH_SQUAD,
    EIGTHTEENTH_SQUAD,
    NINTEENTH_SQUAD,
    TWENTYTH_SQUAD,
    NUMBER_OF_SQUADS,

    ON_DUTY = 20,
    UNSET = -1,
}

public class SAVE_SQUAD_INFO_STRUCT
{
    public int uiID;                     // The soldiers ID
    public short[] sPadding = new short[5];
    //	int	bSquadValue;		// The squad id

}
