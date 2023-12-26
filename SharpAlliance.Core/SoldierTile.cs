using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.SubSystems;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class SoldierTile
{
    void SetDelayedTileWaiting(SOLDIERTYPE? pSoldier, int sCauseGridNo, int bValue)
    {
        int ubPerson;

        // Cancel AI Action
        // CancelAIAction( pSoldier, true );

        pSoldier.fDelayedMovement = bValue;
        pSoldier.sDelayedMovementCauseGridNo = sCauseGridNo;

        RESETTIMECOUNTER(ref pSoldier.NextTileCounter, NEXT_TILE_CHECK_DELAY);

        // ATE: Now update realtime movement speed....
        // check if guy exists here...
        ubPerson = WorldManager.WhoIsThere2(sCauseGridNo, pSoldier.bLevel);

        // There may not be anybody there, but it's reserved by them!
        if (gpWorldLevelData[sCauseGridNo].uiFlags.HasFlag(MAPELEMENTFLAGS.MOVEMENT_RESERVED))
        {
            ubPerson = gpWorldLevelData[sCauseGridNo].ubReservedSoldierID;
        }

        if (ubPerson != NOBODY)
        {
            // if they are our own team members ( both )
            if (MercPtrs[ubPerson].bTeam == gbPlayerNum && pSoldier.bTeam == gbPlayerNum)
            {
                // Here we have another guy.... save his stats so we can use them for
                // speed determinations....
                pSoldier.bOverrideMoveSpeed = ubPerson;
                pSoldier.fUseMoverrideMoveSpeed = true;
            }
        }
    }


    void SetFinalTile(SOLDIERTYPE? pSoldier, int sGridNo, bool fGivenUp)
    {
        // OK, If we were waiting for stuff, do it here...

        // ATE: Disabled stuff below, made obsolete by timeout...
        //if ( pSoldier.ubWaitActionToDo  )
        //{
        //	pSoldier.ubWaitActionToDo = 0;
        //	gbNumMercsUntilWaitingOver--;
        //}
        pSoldier.sFinalDestination = pSoldier.sGridNo;

        if (pSoldier.bTeam == gbPlayerNum && fGivenUp)
        {
            Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.INTERFACE, TacticalStr[(int)STR.NO_PATH_FOR_MERC], pSoldier.name);
        }

        SoldierControl.EVENT_StopMerc(pSoldier, pSoldier.sGridNo, pSoldier.bDirection);
    }


    void MarkMovementReserved(SOLDIERTYPE? pSoldier, int sGridNo)
    {
        // Check if we have one reserrved already, and free it first!
        if (pSoldier.sReservedMovementGridNo != NOWHERE)
        {
            UnMarkMovementReserved(pSoldier);
        }

        // For single-tiled mercs, set this gridno
        gpWorldLevelData[sGridNo].uiFlags |= MAPELEMENTFLAGS.MOVEMENT_RESERVED;

        // Save soldier's reserved ID #
        gpWorldLevelData[sGridNo].ubReservedSoldierID = pSoldier.ubID;

        pSoldier.sReservedMovementGridNo = sGridNo;
    }

    public static void UnMarkMovementReserved(SOLDIERTYPE pSoldier)
    {
        int sNewGridNo = -1;

//        sNewGridNo = GETWORLDINDEXFROMWORLDCOORDS(pSoldier.dYPos, pSoldier.dXPos);

        // OK, if NOT in fence anim....
        if (pSoldier.usAnimState == AnimationStates.HOPFENCE && pSoldier.sReservedMovementGridNo != sNewGridNo)
        {
            return;
        }

        // For single-tiled mercs, unset this gridno
        // See if we have one reserved!
        if (pSoldier.sReservedMovementGridNo != NOWHERE)
        {
            gpWorldLevelData[pSoldier.sReservedMovementGridNo].uiFlags &= ~MAPELEMENTFLAGS.MOVEMENT_RESERVED;

            pSoldier.sReservedMovementGridNo = NOWHERE;
        }
    }

    int TileIsClear(SOLDIERTYPE pSoldier, WorldDirections bDirection, int sGridNo, int bLevel)
    {
        int ubPerson;
        int sTempDestGridNo;
        int sNewGridNo;
        bool fSwapInDoor = false;

        if (sGridNo == NOWHERE)
        {
            return MOVE_TILE_CLEAR;
        }

        ubPerson = WorldManager.WhoIsThere2(sGridNo, bLevel);


        if (ubPerson != NO_SOLDIER)
        {
            // If this us?
            if (ubPerson != pSoldier.ubID)
            {
                // OK, set flag indicating we are blocked by a merc....
                if (pSoldier.bTeam != gbPlayerNum) // CJC: shouldn't this be in all cases???
                                                   //if ( 0 )
                {
                    pSoldier.fBlockedByAnotherMerc = true;
                    // Set direction we were trying to goto
                    pSoldier.bBlockedByAnotherMercDirection = bDirection;

                    // Are we only temporarily blocked?
                    // Check if our final destination is = our gridno
                    if (MercPtrs[ubPerson].sFinalDestination == MercPtrs[ubPerson].sGridNo)
                    {
                        return MOVE_TILE_STATIONARY_BLOCKED;
                    }
                    else
                    {
                        // OK, if buddy who is blocking us is trying to move too...
                        // And we are in opposite directions...
                        if (MercPtrs[ubPerson].fBlockedByAnotherMerc && MercPtrs[ubPerson].bBlockedByAnotherMercDirection == gOppositeDirection[bDirection])
                        {
                            // OK, try and get a path around buddy....
                            // We have to temporarily make buddy stopped...
                            sTempDestGridNo = MercPtrs[ubPerson].sFinalDestination;
                            MercPtrs[ubPerson].sFinalDestination = MercPtrs[ubPerson].sGridNo;

                            if (PathAI.PlotPath(
                                pSoldier,
                                pSoldier.sFinalDestination,
                                PlotPathDefines.NO_COPYROUTE,
                                NO_PLOT,
                                PlotPathDefines.TEMPORARY,
                                pSoldier.usUIMovementMode,
                                PlotPathDefines.NOT_STEALTH,
                                PlotPathDefines.FORWARD,
                                pSoldier.bActionPoints) > 0)
                            {
                                pSoldier.bPathStored = false;
                                // OK, make guy go here...
                                SoldierControl.EVENT_GetNewSoldierPath(pSoldier, pSoldier.sFinalDestination, pSoldier.usUIMovementMode);
                                // Restore final dest....
                                MercPtrs[ubPerson].sFinalDestination = sTempDestGridNo;
                                pSoldier.fBlockedByAnotherMerc = false;

                                // Is the next tile blocked too?
                                sNewGridNo = IsometricUtils.NewGridNo(pSoldier.sGridNo, IsometricUtils.DirectionInc(guiPathingData[0]));

                                return this.TileIsClear(pSoldier, (WorldDirections)guiPathingData[0], sNewGridNo, pSoldier.bLevel);
                            }
                            else
                            {

                                // Not for multi-tiled things...
                                if (!pSoldier.uiStatusFlags.HasFlag(SOLDIER.MULTITILE))
                                {
                                    // Is the next movement cost for a door?
//                                    if (PathAI.DoorTravelCost(pSoldier, sGridNo, gubWorldMovementCosts[sGridNo, bDirection, pSoldier.bLevel], (bool)(pSoldier.bTeam == gbPlayerNum), out var _) == TRAVELCOST.DOOR)
                                    {
                                        fSwapInDoor = true;
                                    }

                                    // If we are to swap and we're near a door, open door first and then close it...?


                                    // Swap now!
                                    MercPtrs[ubPerson].fBlockedByAnotherMerc = false;

                                    // Restore final dest....
                                    MercPtrs[ubPerson].sFinalDestination = sTempDestGridNo;

                                    // Swap merc positions.....
                                    this.SwapMercPositions(pSoldier, MercPtrs[ubPerson]);

                                    // With these two guys swapped, they should try and continue on their way....
                                    // Start them both again along their way...
                                    SoldierControl.EVENT_GetNewSoldierPath(pSoldier, pSoldier.sFinalDestination, pSoldier.usUIMovementMode);
                                    SoldierControl.EVENT_GetNewSoldierPath(MercPtrs[ubPerson], MercPtrs[ubPerson].sFinalDestination, MercPtrs[ubPerson].usUIMovementMode);
                                }
                            }
                        }
                        return MOVE_TILE_TEMP_BLOCKED;
                    }
                }
                else
                {
                    //return( MOVE_TILE_STATIONARY_BLOCKED );
                    // ATE: OK, put some smartshere...
                    // If we are waiting for more than a few times, change to stationary...
                    if (MercPtrs[ubPerson].fDelayedMovement >= 105)
                    {
                        // Set to special 'I want to walk through people' value
                        pSoldier.fDelayedMovement = 150;

                        return MOVE_TILE_STATIONARY_BLOCKED;
                    }
                    if (MercPtrs[ubPerson].sGridNo == MercPtrs[ubPerson].sFinalDestination)
                    {
                        return MOVE_TILE_STATIONARY_BLOCKED;
                    }
                    return MOVE_TILE_TEMP_BLOCKED;
                }
            }
        }

        if (gpWorldLevelData[sGridNo].uiFlags.HasFlag(MAPELEMENTFLAGS.MOVEMENT_RESERVED))
        {
            if (gpWorldLevelData[sGridNo].ubReservedSoldierID != pSoldier.ubID)
            {
                return MOVE_TILE_TEMP_BLOCKED;
            }
        }

        // Are we clear of structs?
        if (!Overhead.NewOKDestination(pSoldier, sGridNo, false, pSoldier.bLevel))
        {
            // ATE: Fence cost is an exclusiuon here....
//            if (gubWorldMovementCosts[sGridNo, bDirection, pSoldier.bLevel] != TRAVELCOST.FENCE)
            {
                // ATE: HIdden structs - we do something here... reveal it!
//                if (gubWorldMovementCosts[sGridNo, bDirection, pSoldier.bLevel] == TRAVELCOST.HIDDENOBSTACLE)
                {
                    gpWorldLevelData[sGridNo].uiFlags |= MAPELEMENTFLAGS.REVEALED;
                    gpWorldLevelData[sGridNo].uiFlags |= MAPELEMENTFLAGS.REDRAW;
                    RenderWorld.SetRenderFlags(RenderingFlags.MARKED);
//                    RecompileLocalMovementCosts((int)sGridNo);
                }

                // Unset flag for blocked by soldier...
                pSoldier.fBlockedByAnotherMerc = false;
                return MOVE_TILE_STATIONARY_BLOCKED;
            }
//            else
            {
            }
        }

        // Unset flag for blocked by soldier...
        pSoldier.fBlockedByAnotherMerc = false;

        return MOVE_TILE_CLEAR;
    }

    bool HandleNextTile(SOLDIERTYPE? pSoldier, WorldDirections bDirection, int sGridNo, int sFinalDestTile)
    {
        int bBlocked;
        TerrainTypeDefines bOverTerrainType;

        // Check for blocking if in realtime 
        //if ( ( gTacticalStatus.uiFlags & REALTIME ) || !( gTacticalStatus.uiFlags & INCOMBAT ) )

        // ATE: If not on visible tile, return clear ( for path out of map )
        if (!IsometricUtils.GridNoOnVisibleWorldTile(sGridNo))
        {
            return true;
        }

        // If animation state is crow, iall is clear
        if (pSoldier.usAnimState == AnimationStates.CROW_FLY)
        {
            return true;
        }

        {
            bBlocked = this.TileIsClear(pSoldier, bDirection, sGridNo, pSoldier.bLevel);

            // Check if we are blocked...
            if (bBlocked != MOVE_TILE_CLEAR)
            {
                // Is the next gridno our destination?
                // OK: Let's check if we are NOT walking off screen
                if (sGridNo == sFinalDestTile && pSoldier.ubWaitActionToDo == 0 && (pSoldier.bTeam == gbPlayerNum || pSoldier.sAbsoluteFinalDestination == NOWHERE))
                {
                    // Yah, well too bad, stop here.
                    this.SetFinalTile(pSoldier, pSoldier.sGridNo, false);

                    return false;
                }
                // CHECK IF they are stationary
                else if (bBlocked == MOVE_TILE_STATIONARY_BLOCKED)
                {
                    // Stationary, 
                    {
                        int sOldFinalDest;

                        // Maintain sFinalDest....
                        sOldFinalDest = pSoldier.sFinalDestination;
                        SoldierControl.EVENT_StopMerc(pSoldier, pSoldier.sGridNo, pSoldier.bDirection);
                        // Restore...
                        pSoldier.sFinalDestination = sOldFinalDest;

                        this.SetDelayedTileWaiting(pSoldier, sGridNo, 1);

                        return false;
                    }
                }
                else
                {
                    {
                        int sOldFinalDest;

                        // Maintain sFinalDest....
                        sOldFinalDest = pSoldier.sFinalDestination;
                        SoldierControl.EVENT_StopMerc(pSoldier, pSoldier.sGridNo, pSoldier.bDirection);
                        // Restore...
                        pSoldier.sFinalDestination = sOldFinalDest;

                        // Setting to two means: try and wait until this tile becomes free....
                        this.SetDelayedTileWaiting(pSoldier, sGridNo, 100);
                    }

                    return false;
                }
            }
            else
            {
                // Mark this tile as reserverd ( until we get there! )
                if (!(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED) && gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
                {
                    this.MarkMovementReserved(pSoldier, sGridNo);
                }

                bOverTerrainType = WorldManager.GetTerrainType(sGridNo);

                // Check if we are going into water!
                if (bOverTerrainType == TerrainTypeDefines.LOW_WATER
                    || bOverTerrainType == TerrainTypeDefines.MED_WATER
                    || bOverTerrainType == TerrainTypeDefines.DEEP_WATER)
                {
                    // Check if we are of prone or crawl height and change stance accordingly....
                    switch (gAnimControl[pSoldier.usAnimState].ubHeight)
                    {
                        case AnimationHeights.ANIM_PRONE:
                        case AnimationHeights.ANIM_CROUCH:

                            // Change height to stand
                            pSoldier.fContinueMoveAfterStanceChange = true;
                            SoldierControl.SendChangeSoldierStanceEvent(pSoldier, AnimationHeights.ANIM_STAND);
                            break;
                    }

                    // Check animation
                    // Change to walking
                    if (pSoldier.usAnimState == AnimationStates.RUNNING)
                    {
                        SoldierControl.ChangeSoldierState(pSoldier, AnimationStates.WALKING, 0, false);
                    }
                }
            }
        }
        return true;
    }



    bool HandleNextTileWaiting(SOLDIERTYPE pSoldier)
    {
        // Buddy is waiting to continue his path
        int bBlocked, bPathBlocked = 0;
        int sCost = 0;
        int sNewGridNo, sCheckGridNo = 0;
        int ubDirection;
        WorldDirections bCauseDirection;
        int ubPerson;
        PATH fFlags = 0;


        if (pSoldier.fDelayedMovement > 0)
        {
            if (TIMECOUNTERDONE(pSoldier.NextTileCounter, NEXT_TILE_CHECK_DELAY))
            {
                RESETTIMECOUNTER(ref pSoldier.NextTileCounter, NEXT_TILE_CHECK_DELAY);

                // Get direction from gridno...
                bCauseDirection = SoldierControl.GetDirectionToGridNoFromGridNo(pSoldier.sGridNo, pSoldier.sDelayedMovementCauseGridNo);

                bBlocked = this.TileIsClear(pSoldier, bCauseDirection, pSoldier.sDelayedMovementCauseGridNo, pSoldier.bLevel);

                // If we are waiting for a temp blockage.... continue to wait
                if (pSoldier.fDelayedMovement >= 100 && bBlocked == MOVE_TILE_TEMP_BLOCKED)
                {
                    // ATE: Increment 1
                    pSoldier.fDelayedMovement++;

                    // Are we close enough to give up? ( and are a pc )
                    if (pSoldier.fDelayedMovement > 120)
                    {
                        // Quit...
                        this.SetFinalTile(pSoldier, pSoldier.sGridNo, true);
                        pSoldier.fDelayedMovement = 0;
                    }
                    return true;
                }

                // Try new path if anything but temp blockage!
                if (bBlocked != MOVE_TILE_TEMP_BLOCKED)
                {
                    // Set to normal delay
                    if (pSoldier.fDelayedMovement >= 100 && pSoldier.fDelayedMovement != 150)
                    {
                        pSoldier.fDelayedMovement = 1;
                    }

                    // Default to pathing through people
                    fFlags = PATH.THROUGH_PEOPLE;

                    // Now, if we are in the state where we are desparently trying to get out...
                    // Use other flag
                    // CJC: path-through-people includes ignoring person at dest
                    /*
                    if ( pSoldier.fDelayedMovement >= 150 )
                    {
                        fFlags = PATH_IGNORE_PERSON_AT_DEST;
                    }
                    */

                    // Check destination first!
                    if (pSoldier.sAbsoluteFinalDestination == pSoldier.sFinalDestination)
                    {
                        // on last lap of scripted move, make sure we get to final dest
                        sCheckGridNo = pSoldier.sAbsoluteFinalDestination;
                    }
                    else if (!Overhead.NewOKDestination(pSoldier, pSoldier.sFinalDestination, true, pSoldier.bLevel))
                    {
                        if (pSoldier.fDelayedMovement >= 150)
                        {
                            // OK, look around dest for the first one!
//                            sCheckGridNo = FindGridNoFromSweetSpot(pSoldier, pSoldier.sFinalDestination, 6, ubDirection);

                            if (sCheckGridNo == NOWHERE)
                            {
                                // If this is nowhere, try harder!
//                                sCheckGridNo = FindGridNoFromSweetSpot(pSoldier, pSoldier.sFinalDestination, 16, &ubDirection);
                            }
                        }
                        else
                        {
                            // OK, look around dest for the first one!
//                            sCheckGridNo = FindGridNoFromSweetSpotThroughPeople(pSoldier, pSoldier.sFinalDestination, 6, &ubDirection);

                            if (sCheckGridNo == NOWHERE)
                            {
                                // If this is nowhere, try harder!
//                                sCheckGridNo = FindGridNoFromSweetSpotThroughPeople(pSoldier, pSoldier.sFinalDestination, 16, &ubDirection);
                            }
                        }
                    }
                    else
                    {
                        sCheckGridNo = pSoldier.sFinalDestination;
                    }

                    // Try another path to destination
                    // ATE: Allow path to exit grid!
//                    if (pSoldier.ubWaitActionToDo == 1 && gubWaitingForAllMercsToExitCode == WAIT_FOR_MERCS_TO_WALK_TO_GRIDNO)
                    {
                        gfPlotPathToExitGrid = true;
                    }

//                    sCost = (int)FindBestPath(pSoldier, sCheckGridNo, pSoldier.bLevel, pSoldier.usUIMovementMode, NO_COPYROUTE, fFlags);
                    gfPlotPathToExitGrid = false;

                    // Can we get there
                    if (sCost > 0)
                    {
                        // Is the next tile blocked too?
                        sNewGridNo = IsometricUtils.NewGridNo((int)pSoldier.sGridNo, IsometricUtils.DirectionInc((int)guiPathingData[0]));

//                        bPathBlocked = TileIsClear(pSoldier, guiPathingData[0], sNewGridNo, pSoldier.bLevel);

                        if (bPathBlocked == MOVE_TILE_STATIONARY_BLOCKED)
                        {
                            // Try to path around everyone except dest person

//                            if (pSoldier.ubWaitActionToDo == 1 && gubWaitingForAllMercsToExitCode == WAIT_FOR_MERCS_TO_WALK_TO_GRIDNO)
                            {
                                gfPlotPathToExitGrid = true;
                            }

//                            sCost = (int)FindBestPath(pSoldier, sCheckGridNo, pSoldier.bLevel, pSoldier.usUIMovementMode, NO_COPYROUTE, PATH.IGNORE_PERSON_AT_DEST);

                            gfPlotPathToExitGrid = false;

                            // Is the next tile in this new path blocked too?
                            sNewGridNo = IsometricUtils.NewGridNo(pSoldier.sGridNo, IsometricUtils.DirectionInc(guiPathingData[0]));

//                            bPathBlocked = TileIsClear(pSoldier, (int)guiPathingData[0], sNewGridNo, pSoldier.bLevel);

                            // now working with a path which does not go through people				
//                            pSoldier.ubDelayedMovementFlags &= (~DELAYED_MOVEMENT_FLAG_PATH_THROUGH_PEOPLE);
                        }
                        else
                        {
                            // path through people worked fine
                            if (pSoldier.fDelayedMovement < 150)
                            {
//                                pSoldier.ubDelayedMovementFlags |= DELAYED_MOVEMENT_FLAG_PATH_THROUGH_PEOPLE;
                            }
                        }

                        // Are we clear?
                        if (bPathBlocked == MOVE_TILE_CLEAR)
                        {
                            // Go for it path!
//                            if (pSoldier.ubWaitActionToDo == 1 && gubWaitingForAllMercsToExitCode == WAIT_FOR_MERCS_TO_WALK_TO_GRIDNO)
                            {
                                gfPlotPathToExitGrid = true;
                            }

                            //pSoldier.fDelayedMovement = false;
                            // ATE: THis will get set in EENT_GetNewSoldierPath....
                            pSoldier.usActionData = sCheckGridNo;

                            pSoldier.bPathStored = false;

                            SoldierControl.EVENT_GetNewSoldierPath(pSoldier, sCheckGridNo, pSoldier.usUIMovementMode);
                            gfPlotPathToExitGrid = false;

                            return true;
                        }
                    }

                    pSoldier.fDelayedMovement++;

                    if (pSoldier.fDelayedMovement == 99)
                    {
                        // Cap at 99
                        pSoldier.fDelayedMovement = 99;
                    }

                    // Do we want to force a swap?
                    if (pSoldier.fDelayedMovement == 3 && (pSoldier.sAbsoluteFinalDestination != NOWHERE || gTacticalStatus.fAutoBandageMode))
                    {
                        // with person who is in the way?
                        ubPerson = WorldManager.WhoIsThere2(pSoldier.sDelayedMovementCauseGridNo, pSoldier.bLevel);

                        // if either on a mission from god, or two AI guys not on stationary...
                        if (ubPerson != NOBODY
                            && (pSoldier.ubQuoteRecord != 0
                                || (pSoldier.bTeam != gbPlayerNum
                                && pSoldier.bOrders != Orders.STATIONARY
                                && MercPtrs[ubPerson].bTeam != gbPlayerNum
                                && MercPtrs[ubPerson].bOrders != Orders.STATIONARY)
                            || (pSoldier.bTeam == gbPlayerNum && gTacticalStatus.fAutoBandageMode
                                && !(MercPtrs[ubPerson].bTeam == CIV_TEAM
                                && MercPtrs[ubPerson].bOrders == Orders.STATIONARY))))
                        {
                            // Swap now!
                            //MercPtrs[ ubPerson ].fBlockedByAnotherMerc = false;

                            // Restore final dest....
                            //MercPtrs[ ubPerson ].sFinalDestination = sTempDestGridNo;

                            // Swap merc positions.....
                            this.SwapMercPositions(pSoldier, MercPtrs[ubPerson]);

                            // With these two guys swapped, we should try to continue on our way....				
                            pSoldier.fDelayedMovement = 0;

                            // We must calculate the path here so that we can give it the "through people" parameter
//                            if (gTacticalStatus.fAutoBandageMode && pSoldier.sAbsoluteFinalDestination == NOWHERE)
                            {
//                                FindBestPath(pSoldier, pSoldier.sFinalDestination, pSoldier.bLevel, pSoldier.usUIMovementMode, COPYROUTE, PATH.THROUGH_PEOPLE);
                            }
//                            else if (pSoldier.sAbsoluteFinalDestination != NOWHERE && !FindBestPath(pSoldier, pSoldier.sAbsoluteFinalDestination, pSoldier.bLevel, pSoldier.usUIMovementMode, COPYROUTE, PATH.THROUGH_PEOPLE))
                            {
                                // check to see if we're there now!
                                if (pSoldier.sGridNo == pSoldier.sAbsoluteFinalDestination)
                                {
                                    NPC.NPCReachedDestination(pSoldier, false);
                                    pSoldier.bNextAction = AI_ACTION.WAIT;
                                    pSoldier.usNextActionData = 500;
                                    return true;
                                }
                            }

                            pSoldier.bPathStored = true;

                            SoldierControl.EVENT_GetNewSoldierPath(pSoldier, pSoldier.sAbsoluteFinalDestination, pSoldier.usUIMovementMode);
                            //EVENT_GetNewSoldierPath( MercPtrs[ ubPerson ], MercPtrs[ ubPerson ].sFinalDestination, MercPtrs[ ubPerson ].usUIMovementMode );					
                        }

                    }

                    // Are we close enough to give up? ( and are a pc )
                    if (pSoldier.fDelayedMovement > 20 && pSoldier.fDelayedMovement != 150)
                    {
                        if (IsometricUtils.PythSpacesAway(pSoldier.sGridNo, pSoldier.sFinalDestination) < 5 && pSoldier.bTeam == gbPlayerNum)
                        {
                            // Quit...
                            this.SetFinalTile(pSoldier, pSoldier.sGridNo, false);
                            pSoldier.fDelayedMovement = 0;
                        }
                    }

                    // Are we close enough to give up? ( and are a pc )
                    if (pSoldier.fDelayedMovement > 170)
                    {
                        if (IsometricUtils.PythSpacesAway(pSoldier.sGridNo, pSoldier.sFinalDestination) < 5 && pSoldier.bTeam == gbPlayerNum)
                        {
                            // Quit...
                            this.SetFinalTile(pSoldier, pSoldier.sGridNo, false);
                            pSoldier.fDelayedMovement = 0;
                        }
                    }

                }
            }
        }
        return true;
    }


    bool TeleportSoldier(SOLDIERTYPE? pSoldier, int sGridNo, bool fForce)
    {
        int sX, sY;

        // Check dest...
        if (Overhead.NewOKDestination(pSoldier, sGridNo, true, 0) || fForce)
        {
            // TELEPORT TO THIS LOCATION!
            sX = IsometricUtils.CenterX(sGridNo);
            sY = IsometricUtils.CenterY(sGridNo);
            SoldierControl.EVENT_SetSoldierPosition(pSoldier, sX, sY);

            pSoldier.sFinalDestination = sGridNo;

            // Make call to FOV to update items...
//            RevealRoofsAndItems(pSoldier, true, true, pSoldier.bLevel, true);

            // Handle sight!
//            HandleSight(pSoldier, SIGHT.LOOK | SIGHT.RADIO);

            // Cancel services...
//            GivingSoldierCancelServices(pSoldier);

            // Change light....
            if (pSoldier.bLevel == 0)
            {
                if (pSoldier.iLight != (-1))
                {
//                    LightSpriteRoofStatus(pSoldier.iLight, false);
                }
            }
            else
            {
                if (pSoldier.iLight != (-1))
                {
//                    LightSpriteRoofStatus(pSoldier.iLight, true);
                }
            }
            return true;
        }

        return false;
    }

    // Swaps 2 soldier positions...
    void SwapMercPositions(SOLDIERTYPE pSoldier1, SOLDIERTYPE pSoldier2)
    {
        int sGridNo1, sGridNo2;

        // OK, save positions...
        sGridNo1 = pSoldier1.sGridNo;
        sGridNo2 = pSoldier2.sGridNo;

        // OK, remove each.....
        SoldierControl.RemoveSoldierFromGridNo(pSoldier1);
        SoldierControl.RemoveSoldierFromGridNo(pSoldier2);

        // OK, test OK destination for each.......
        if (Overhead.NewOKDestination(pSoldier1, sGridNo2, true, 0) && Overhead.NewOKDestination(pSoldier2, sGridNo1, true, 0))
        {
            // OK, call teleport function for each.......
            this.TeleportSoldier(pSoldier1, sGridNo2, false);
            this.TeleportSoldier(pSoldier2, sGridNo1, false);
        }
        else
        {
            // Place back...
            this.TeleportSoldier(pSoldier1, sGridNo1, true);
            this.TeleportSoldier(pSoldier2, sGridNo2, true);
        }
    }


    bool CanExchangePlaces(SOLDIERTYPE pSoldier1, SOLDIERTYPE pSoldier2, bool fShow)
    {
        // NB checks outside of this function 
//        if (EnoughPoints(pSoldier1, AP.EXCHANGE_PLACES, 0, fShow))
        {
//            if (EnoughPoints(pSoldier2, AP.EXCHANGE_PLACES, 0, fShow))
            {
                if (gAnimControl[pSoldier2.usAnimState].uiFlags.HasFlag(ANIM.MOVING))
                {
                    return false;
                }

                if (gAnimControl[pSoldier1.usAnimState].uiFlags.HasFlag(ANIM.MOVING) && !gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
                {
                    return false;
                }

                if (pSoldier2.bSide == 0)
                {
                    return true;
                }

                // hehe - don't allow animals to exchange places
                if (pSoldier2.uiStatusFlags.HasFlag(SOLDIER.ANIMAL))
                {
                    return false;
                }

                // must NOT be hostile, must NOT have stationary orders OR militia team, must be >= OKLIFE
                if (pSoldier2.bNeutral > 0 && pSoldier2.bLife >= OKLIFE &&
                           pSoldier2.ubCivilianGroup != CIV_GROUP.HICKS_CIV_GROUP &&
                         (pSoldier2.bOrders != Orders.STATIONARY || pSoldier2.bTeam == MILITIA_TEAM ||
                         (pSoldier2.sAbsoluteFinalDestination != NOWHERE && pSoldier2.sAbsoluteFinalDestination != pSoldier2.sGridNo)))
                {
                    return true;
                }

                if (fShow)
                {
                    if (pSoldier2.ubProfile == NO_PROFILE)
                    {
                        Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, TacticalStr[(int)STR.REFUSE_EXCHANGE_PLACES]);
                    }
                    else
                    {
                        Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, gzLateLocalizedString[3], pSoldier2.name);
                    }
                }

                // ATE: OK, reduce this guy's next ai counter....
                pSoldier2.uiAIDelay = 100;

                return false;
            }
//            else
            {
                return false;
            }
        }
//        else
        {
            return false;
        }
        return true;
    }
}
