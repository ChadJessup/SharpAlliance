using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Managers;

using static SharpAlliance.Core.Globals;

using TRAILCELLTYPE = System.Int32;
namespace SharpAlliance.Core.SubSystems;

public class PathAI
{
    private readonly ILogger<PathAI> logger;
    private readonly GameSettings gGameSettings;
    private readonly Overhead overhead;
    private readonly WorldManager worldManager;

    private static Dictionary<WorldDirections, int> dirDelta = new()
    {
        { WorldDirections.NORTH , -Globals.MAPWIDTH } ,     //N
    	{ WorldDirections.NORTHEAST , 1-Globals.MAPWIDTH },      //NE
    	{ WorldDirections.EAST , 1                  },      //E
    	{ WorldDirections.SOUTHEAST , 1+Globals.MAPWIDTH },      //SE
    	{ WorldDirections.SOUTH , Globals.MAPWIDTH   },      //S
    	{ WorldDirections.SOUTHWEST , Globals.MAPWIDTH-1 },      //SW
    	{ WorldDirections.WEST , -1                 },      //W
    	{ WorldDirections.NORTHWEST , -Globals.MAPWIDTH-1 },      //NW
    };


    public PathAI(
        ILogger<PathAI> logger,
        GameSettings gameSettings,
        Overhead overhead,
        WorldManager worldManager)
    {
        this.logger = logger;
        this.gGameSettings = gameSettings;
        this.overhead = overhead;
        this.worldManager = worldManager;
    }

    public static int DoorTravelCost(SOLDIERTYPE? pSoldier, int iGridNo, int ubMovementCost, bool fReturnPerceivedValue, out int? piDoorGridNo)
    {
        return (InternalDoorTravelCost(pSoldier, iGridNo, ubMovementCost, fReturnPerceivedValue, out piDoorGridNo, false));
    }

    public static void RoofReachableTest(int sStartGridNo, int ubBuildingID)
    {
        SOLDIERTYPE s = new()
        {
            sGridNo = sStartGridNo,
            bLevel = 1,
            bTeam = TEAM.ENEMY_TEAM,
        };

        gubBuildingInfoToSet = ubBuildingID;

        ReconfigurePathAI(ABSMAX_SKIPLIST_LEVEL, ABSMAX_TRAIL_TREE, ABSMAX_PATHQ);
        FindBestPath(s, NOWHERE, 1, WALKING, COPYREACHABLE, 0);
        RestorePathAIToDefaults();

        // set start position to reachable since path code sets it unreachable
        gpWorldLevelData[sStartGridNo].uiFlags |= MAPELEMENTFLAGS.REACHABLE;

        // reset building variable
        gubBuildingInfoToSet = 0;
    }

    void ReconfigurePathAI(int iNewMaxSkipListLevel, int iNewMaxTrailTree, int iNewMaxPathQ)
    {
        // make sure the specified parameters are reasonable
        iNewMaxSkipListLevel = Math.Max(iNewMaxSkipListLevel, ABSMAX_SKIPLIST_LEVEL);
        iNewMaxTrailTree = Math.Max(iNewMaxTrailTree, ABSMAX_TRAIL_TREE);
        iNewMaxPathQ = Math.Max(iNewMaxPathQ, ABSMAX_PATHQ);
        // assign them
        iMaxSkipListLevel = iNewMaxSkipListLevel;
        iMaxTrailTree = iNewMaxTrailTree;
        iMaxPathQ = iNewMaxPathQ;
        // relocate the head of the closed list to the end of the array portion being used
        pClosedHead = (pathQ[QPOOLNDX]);
    }


    public int UIPlotPath(
        SOLDIERTYPE? pSold,
        int sDestGridno,
        PlotPathDefines bCopyRoute,
        bool bPlot,
        PlotPathDefines bStayOn,
        AnimationStates usMovementMode,
        PlotPathDefines bStealth,
        PlotPathDefines bReverse,
        int sAPBudget)
    {
        // This function is specifically for UI calls to the pathing routine, to 
        // check whether the shift key is pressed, etc.
        int sRet;

        if (_KeyDown(SHIFT))
        {
            Globals.gfPlotDirectPath = true;
        }

        // If we are on the same level as the interface level, continue, else return
        if (pSold.bLevel != Globals.gsInterfaceLevel)
        {
            return (0);
        }

        if (GameSettings.fOptions[TOPTION.ALWAYS_SHOW_MOVEMENT_PATH])
        {
            bPlot = true;
        }

        sRet = PlotPath(
            pSold,
            sDestGridno,
            bCopyRoute,
            bPlot,
            bStayOn,
            usMovementMode,
            bStealth,
            bReverse,
            sAPBudget);

        Globals.gfPlotDirectPath = false;
        return (sRet);
    }

    public static int PlotPath(
        SOLDIERTYPE? pSold,
        int sDestGridno,
        PlotPathDefines? bCopyRoute,
        bool bPlot,
        PlotPathDefines? bStayOn,
        AnimationStates usMovementMode,
        PlotPathDefines? bStealth,
        PlotPathDefines? bReverse,
        int sAPBudget)
    {
        int sTileCost, sPoints = 0, sTempGrid, sAnimCost = 0;
        int sPointsWalk = 0, sPointsCrawl = 0, sPointsRun = 0, sPointsSwat = 0;
        int sExtraCostStand, sExtraCostSwat, sExtraCostCrawl;
        int iLastGrid;
        int iCnt;
        int sOldGrid = 0;
        int sFootOrderIndex;
        int sSwitchValue;
        STEPSTART[] sFootOrder = new STEPSTART[] { STEPSTART.GREEN, STEPSTART.PURPLE, STEPSTART.BLUE,
                                                    STEPSTART.ORANGE, STEPSTART.RED };
        int usTileIndex;
        int usTileNum;
        LEVELNODE? pNode;
        AnimationStates usMovementModeToUseForAPs;
        bool bIgnoreNextCost = false;
        int sTestGridno;

        if (bPlot && Globals.gusPathShown)
        {
            PathAI.ErasePath(false);
        }

        Globals.gusAPtsToMove = 0;
        sTempGrid = pSold.sGridNo;

        sFootOrderIndex = 0;


        //gubNPCMovementMode = (Ubyte) usMovementMode;
        // distance limit to reduce the cost of plotting a path to a location we can't reach

        // For now, use known hight adjustment
        if (Globals.gfRecalculatingExistingPathCost || FindBestPath(pSold, sDestGridno, (byte)pSold.bLevel, usMovementMode, bCopyRoute, 0))
        {
            // if soldier would be STARTING to run then he pays a penalty since it takes time to 
            // run full speed
            if (pSold.usAnimState != AnimationStates.RUNNING)
            {
                // for estimation purposes, always pay penalty
                sPointsRun = AP.START_RUN_COST;
            }

            // Add to points, those needed to start from different stance!
            sPoints += MinAPsToStartMovement(pSold, usMovementMode);


            // We should reduce points for starting to run if first tile is a fence...
            sTestGridno = IsometricUtils.NewGridNo(pSold.sGridNo, IsometricUtils.DirectionInc(Globals.guiPathingData[0]));
            if (Globals.gubWorldMovementCosts[sTestGridno, Globals.guiPathingData[0], pSold.bLevel] == TRAVELCOST.FENCE)
            {
                if (usMovementMode == AnimationStates.RUNNING && pSold.usAnimState != AnimationStates.RUNNING)
                {
                    sPoints -= (int)AP.START_RUN_COST;
                }
            }

            // FIRST, add up "startup" additional costs - such as intermediate animations, etc.
            switch (pSold.usAnimState)
            {
                //case START_AID   :
                //case GIVING_AID  :	sAnimCost = AP.STOP_FIRST_AID;
                //										break;
                //case TWISTOMACH  :
                //case COLLAPSED   :	sAnimCost = AP.GET_UP;
                //										break;
                //case TWISTBACK   :
                //case UNCONSCIOUS :	sAnimCost = (AP.ROLL_OVER+AP.GET_UP);
                //										break;

                //	case CROUCHING	 :  if (usMovementMode == WALKING || usMovementMode == RUNNING)
                //													sAnimCost = AP.CROUCH;
                //											break;
            }


            sPoints += sAnimCost;
            Globals.gusAPtsToMove += sAnimCost;

            if (bStayOn != 0)
            {
                iLastGrid = Globals.giPathDataSize + 1;
            }
            else
            {
                iLastGrid = Globals.giPathDataSize;
            }


            for (iCnt = 0; iCnt < iLastGrid; iCnt++)
            {
                sExtraCostStand = 0;
                sExtraCostSwat = 0;
                sExtraCostCrawl = 0;
                // what is the next gridno in the path?
                sOldGrid = sTempGrid;

                sTempGrid = IsometricUtils.NewGridNo(sTempGrid, IsometricUtils.DirectionInc(Globals.guiPathingData[iCnt]));

                // Get switch value...
                sSwitchValue = Globals.gubWorldMovementCosts[sTempGrid, Globals.guiPathingData[iCnt], pSold.bLevel];

                // get the tile cost for that tile based on WALKING
                sTileCost = TerrainActionPoints(pSold, sTempGrid, Globals.guiPathingData[iCnt], pSold.bLevel);

                usMovementModeToUseForAPs = usMovementMode;

                // ATE - MAKE MOVEMENT ALWAYS WALK IF IN WATER
                if (Globals.gpWorldLevelData[sTempGrid].ubTerrainID == TerrainTypeDefines.DEEP_WATER
                    || Globals.gpWorldLevelData[sTempGrid].ubTerrainID == TerrainTypeDefines.MED_WATER
                    || Globals.gpWorldLevelData[sTempGrid].ubTerrainID == TerrainTypeDefines.LOW_WATER)
                {
                    usMovementModeToUseForAPs = AnimationStates.WALKING;
                }

                if (bIgnoreNextCost)
                {
                    bIgnoreNextCost = false;
                }
                else
                {
                    // ATE: If we have a 'special cost, like jump fence... 
                    if (sSwitchValue == TRAVELCOST.FENCE)
                    {
                        sPoints += sTileCost;

                        bIgnoreNextCost = true;

                        // If we are changeing stance ( either before or after getting there....
                        // We need to reflect that...
                        switch (usMovementModeToUseForAPs)
                        {
                            case AnimationStates.RUNNING:
                            case AnimationStates.WALKING:

                                // Add here cost to go from crouch to stand AFTER fence hop....
                                // Since it's AFTER.. make sure we will be moving after jump...
                                if ((iCnt + 2) < iLastGrid)
                                {
                                    sExtraCostStand += (int)AP.CROUCH;

                                    // ATE: if running, charge extra point to srart again
                                    if (usMovementModeToUseForAPs == AnimationStates.RUNNING)
                                    {
                                        sExtraCostStand++;
                                    }

                                    sPoints += (int)sExtraCostStand;
                                }
                                break;

                            case AnimationStates.SWATTING:

                                // Add cost to stand once there BEFORE....
                                sExtraCostSwat += (int)AP.CROUCH;
                                sPoints += (int)sExtraCostSwat;
                                break;

                            case AnimationStates.CRAWLING:

                                // Can't do it here.....
                                break;

                        }
                    }
                    else if (sTileCost > 0)
                    {
                        // else, movement is adjusted based on mode...

                        if (sSwitchValue == TRAVELCOST.NOT_STANDING)
                        {
                            switch (usMovementModeToUseForAPs)
                            {
                                case AnimationStates.RUNNING:
                                case AnimationStates.WALKING:
                                    // charge crouch APs for ducking head!
                                    sExtraCostStand += (int)AP.CROUCH;
                                    break;

                                default:
                                    break;
                            }
                        }

                        // so, then we must modify it for other movement styles and accumulate
                        sPoints += usMovementModeToUseForAPs switch
                        {
                            AnimationStates.RUNNING => (short)(double)((sTileCost / Globals.RUNDIVISOR)) + (int)sExtraCostStand,
                            AnimationStates.WALKING => (sTileCost + Globals.WALKCOST) + (int)sExtraCostStand,
                            AnimationStates.SWATTING => (sTileCost + Globals.SWATCOST) + (int)sExtraCostSwat,
                            AnimationStates.CRAWLING => (sTileCost + Globals.CRAWLCOST) + (int)sExtraCostCrawl,
                            _ => sTileCost,
                        };
                    }
                }

                // THIS NEXT SECTION ONLY NEEDS TO HAPPEN FOR CURSOR UI FEEDBACK, NOT ACTUAL COSTING

                if (bPlot && ((Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED)) && (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))) // OR USER OPTION ON... ***)
                {
                    // ATE; TODO: Put stuff in here to allow for fact of costs other than movement ( jump fence, open door )

                    // store WALK cost
                    sPointsWalk += (sTileCost + Globals.WALKCOST) + sExtraCostStand;

                    // now get cost as if CRAWLING
                    sPointsCrawl += (sTileCost + Globals.CRAWLCOST) + sExtraCostCrawl;

                    // now get cost as if SWATTING
                    sPointsSwat += (sTileCost + Globals.SWATCOST) + sExtraCostSwat;

                    // now get cost as if RUNNING
                    sPointsRun += (short)(double)((sTileCost / Globals.RUNDIVISOR)) + sExtraCostStand;
                }

                if (iCnt == 0 && bPlot)
                {
                    Globals.gusAPtsToMove = sPoints;

                    Globals.giPlotCnt = 0;

                }


                //if ( gTacticalStatus.uiFlags.HasFlag(TURNBASED && (gTacticalStatus.uiFlags & INCOMBAT)) ) // OR USER OPTION "show paths" ON... ***
                {
                    if (bPlot && ((iCnt < (iLastGrid - 1)) || (iCnt < iLastGrid && bStayOn != 0)))
                    {
                        Globals.guiPlottedPath[Globals.giPlotCnt++] = sTempGrid;

                        // we need a footstep graphic ENTERING the next tile

                        // get the direction
                        usTileNum = Globals.guiPathingData[iCnt] + 2;
                        if (usTileNum > 8)
                        {
                            usTileNum = 1;
                        }

                        // Are we a vehicle?
                        if (pSold.uiStatusFlags.HasFlag(SOLDIER.VEHICLE))
                        {
                            // did we exceed WALK cost?
                            if (sPointsSwat > sAPBudget)
                            {
                                sFootOrderIndex = 4;
                            }
                            else
                            {
                                sFootOrderIndex = 3;
                            }
                        }
                        else
                        {
                            // did we exceed CRAWL cost?
                            if (sFootOrderIndex == 0 && sPointsCrawl > sAPBudget)
                            {
                                sFootOrderIndex++;
                            }

                            // did we exceed WALK cost?
                            if (sFootOrderIndex == 1 && sPointsSwat > sAPBudget)
                            {
                                sFootOrderIndex++;
                            }

                            // did we exceed SWAT cost?
                            if (sFootOrderIndex == 2 && sPointsWalk > sAPBudget)
                            {
                                sFootOrderIndex++;
                            }

                            // did we exceed RUN cost?
                            if (sFootOrderIndex == 3 && sPointsRun > sAPBudget)
                            {
                                sFootOrderIndex++;
                            }
                        }
                        TileDefine.GetTileIndexFromTypeSubIndex(TileTypeDefines.FOOTPRINTS, usTileNum, out usTileIndex);

                        // Adjust based on what mode we are in...
                        if ((Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.REALTIME)) || !(Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
                        {
                            // find out which color we're using
                            usTileIndex += (int)sFootOrder[4];
                        }
                        else // turn based
                        {
                            // find out which color we're using
                            usTileIndex += (int)sFootOrder[sFootOrderIndex];
                        }


                        /*
                        if ( sPoints <= sAPBudget)
                        {
                            // find out which color we're using
                            usTileIndex += sFootOrder[sFootOrderIndex];
                        }
                        else
                        {
                            // use red footprints ( offset by 16 )
                            usTileIndex += REDSTEPSTART;
                        }
                        */

                        if (pSold.bLevel == 0)
                        {
                            pNode = WorldManager.AddObjectToTail(sTempGrid, usTileIndex);
                            pNode.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                            pNode.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                        }
                        else
                        {
                            pNode = WorldManager.AddOnRoofToTail(sTempGrid, usTileIndex);
                            pNode.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                            pNode.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                        }



                        // we need a footstep graphic LEAVING this tile

                        // get the direction using the NEXT tile (thus iCnt+1 as index)
                        usTileNum = Globals.guiPathingData[iCnt + 1] + 2;
                        if (usTileNum > 8)
                        {
                            usTileNum = 1;
                        }


                        // this is a LEAVING footstep which is always the second set of 8
                        usTileNum += 8;

                        TileDefine.GetTileIndexFromTypeSubIndex(TileTypeDefines.FOOTPRINTS, usTileNum, out usTileIndex);

                        // Adjust based on what mode we are in...
                        if ((Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.REALTIME)) || !(Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
                        {
                            // find out which color we're using
                            usTileIndex += (int)sFootOrder[4];
                        }
                        else // turnbased
                        {
                            // find out which color we're using
                            usTileIndex += (int)sFootOrder[sFootOrderIndex];
                        }



                        if (pSold.bLevel == 0)
                        {
                            pNode = WorldManager.AddObjectToTail(sTempGrid, usTileIndex);
                            pNode.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                            pNode.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                        }
                        else
                        {
                            pNode = WorldManager.AddOnRoofToTail(sTempGrid, usTileIndex);
                            pNode.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                            pNode.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                        }
                    }

                }       // end of if turn based or real-time user option "show paths" on...
            }

            if (bPlot)
            {
                Globals.gusPathShown = true;
            }

        }   // end of found a path

        // reset distance limit 
        Globals.gubNPCDistLimit = 0;

        return (sPoints);
    }

    public static void ErasePath(bool bEraseOldOne)
    {
        int iCnt;

        // NOTE: This routine must be called BEFORE anything happens that changes
        //       a merc's gridno, else the....

        //EraseAPCursor();

        if (Globals.gfUIHandleShowMoveGrid > 0)
        {
            Globals.gfUIHandleShowMoveGrid = 0;

            WorldManager.RemoveTopmost(Globals.gsUIHandleShowMoveGridLocation, TileDefines.FIRSTPOINTERS4);
            WorldManager.RemoveTopmost(Globals.gsUIHandleShowMoveGridLocation, TileDefines.FIRSTPOINTERS9);
            WorldManager.RemoveTopmost(Globals.gsUIHandleShowMoveGridLocation, TileDefines.FIRSTPOINTERS2);
            WorldManager.RemoveTopmost(Globals.gsUIHandleShowMoveGridLocation, TileDefines.FIRSTPOINTERS13);
            WorldManager.RemoveTopmost(Globals.gsUIHandleShowMoveGridLocation, TileDefines.FIRSTPOINTERS15);
            WorldManager.RemoveTopmost(Globals.gsUIHandleShowMoveGridLocation, TileDefines.FIRSTPOINTERS19);
            WorldManager.RemoveTopmost(Globals.gsUIHandleShowMoveGridLocation, TileDefines.FIRSTPOINTERS20);
        }

        if (!Globals.gusPathShown)
        {
            //OldPath = false;
            return;
        }

        //if (OldPath > 0 && !eraseOldOne)
        //   return;

        //OldPath = false;

        Globals.gusPathShown = false;

        for (iCnt = 0; iCnt < Globals.giPlotCnt; iCnt++)
        {
            //Grid[PlottedPath[cnt]].fstep = 0;

            WorldManager.RemoveAllObjectsOfTypeRange(Globals.guiPlottedPath[iCnt], TileTypeDefines.FOOTPRINTS, TileTypeDefines.FOOTPRINTS);
            WorldManager.RemoveAllOnRoofsOfTypeRange(Globals.guiPlottedPath[iCnt], TileTypeDefines.FOOTPRINTS, TileTypeDefines.FOOTPRINTS);

            //RemoveAllObjectsOfTypeRange( guiPlottedPath[iCnt], FIRSTPOINTERS, FIRSTPOINTERS );
        }

        //for (cnt=0; cnt < GRIDSIZE; cnt++)
        //    Grid[cnt].fstep = 0;
        //RemoveAllStructsOfTypeRange( gusEndPlotGridNo, GOODRING, GOODRING );

        Globals.giPlotCnt = 0;
        // memset(guiPlottedPath, 0, 256 * sizeof(int));
    }

    private static int InternalDoorTravelCost(SOLDIERTYPE? pSoldier, int iGridNo, int ubMovementCost, bool fReturnPerceivedValue, out int? piDoorGridNo, bool fReturnDoorCost)
    {
        piDoorGridNo = -1;

        // This function will return either TRAVELCOST.DOOR (in place of closed door cost),
        // TRAVELCOST.OBSTACLE, or the base ground terrain
        // travel cost, depending on whether or not the door is open or closed etc.
        bool fDoorIsObstacleIfClosed = false;
        int iDoorGridNo = -1;
        DOOR_STATUS? pDoorStatus;
        DOOR? pDoor;
        STRUCTURE? pDoorStructure;
        bool fDoorIsOpen;
        int ubReplacementCost;

        if (TRAVELCOST.IS_TRAVELCOST.DOOR(ubMovementCost))
        {
            ubReplacementCost = TRAVELCOST.OBSTACLE;

            switch (ubMovementCost)
            {
                case TRAVELCOST.DOOR_CLOSED_HERE:
                    fDoorIsObstacleIfClosed = true;
                    iDoorGridNo = iGridNo;
                    ubReplacementCost = TRAVELCOST.DOOR;
                    break;
                case TRAVELCOST.DOOR_CLOSED_N:
                    fDoorIsObstacleIfClosed = true;
                    iDoorGridNo = iGridNo + dirDelta[WorldDirections.NORTH];
                    ubReplacementCost = TRAVELCOST.DOOR;
                    break;
                case TRAVELCOST.DOOR_CLOSED_W:
                    fDoorIsObstacleIfClosed = true;
                    iDoorGridNo = iGridNo + dirDelta[WorldDirections.WEST];
                    ubReplacementCost = TRAVELCOST.DOOR;
                    break;
                case TRAVELCOST.DOOR_OPEN_HERE:
                    fDoorIsObstacleIfClosed = false;
                    iDoorGridNo = iGridNo;
                    break;
                case TRAVELCOST.DOOR_OPEN_N:
                    fDoorIsObstacleIfClosed = false;
                    iDoorGridNo = iGridNo + dirDelta[WorldDirections.NORTH];
                    break;
                case TRAVELCOST.DOOR_OPEN_NE:
                    fDoorIsObstacleIfClosed = false;
                    iDoorGridNo = iGridNo + dirDelta[WorldDirections.NORTHEAST];
                    break;
                case TRAVELCOST.DOOR_OPEN_E:
                    fDoorIsObstacleIfClosed = false;
                    iDoorGridNo = iGridNo + dirDelta[WorldDirections.EAST];
                    break;
                case TRAVELCOST.DOOR_OPEN_SE:
                    fDoorIsObstacleIfClosed = false;
                    iDoorGridNo = iGridNo + dirDelta[WorldDirections.SOUTHEAST];
                    break;
                case TRAVELCOST.DOOR_OPEN_S:
                    fDoorIsObstacleIfClosed = false;
                    iDoorGridNo = iGridNo + dirDelta[WorldDirections.SOUTH];
                    break;
                case TRAVELCOST.DOOR_OPEN_SW:
                    fDoorIsObstacleIfClosed = false;
                    iDoorGridNo = iGridNo + dirDelta[WorldDirections.SOUTHWEST];
                    break;
                case TRAVELCOST.DOOR_OPEN_W:
                    fDoorIsObstacleIfClosed = false;
                    iDoorGridNo = iGridNo + dirDelta[WorldDirections.WEST];
                    break;
                case TRAVELCOST.DOOR_OPEN_NW:
                    fDoorIsObstacleIfClosed = false;
                    iDoorGridNo = iGridNo + dirDelta[WorldDirections.NORTHWEST];
                    break;
                case TRAVELCOST.DOOR_OPEN_N_N:
                    fDoorIsObstacleIfClosed = false;
                    iDoorGridNo = iGridNo + dirDelta[WorldDirections.NORTH] + dirDelta[WorldDirections.NORTH];
                    break;
                case TRAVELCOST.DOOR_OPEN_NW_N:
                    fDoorIsObstacleIfClosed = false;
                    iDoorGridNo = iGridNo + dirDelta[WorldDirections.NORTHWEST] + dirDelta[WorldDirections.NORTH];
                    break;
                case TRAVELCOST.DOOR_OPEN_NE_N:
                    fDoorIsObstacleIfClosed = false;
                    iDoorGridNo = iGridNo + dirDelta[WorldDirections.NORTHEAST] + dirDelta[WorldDirections.NORTH];
                    break;
                case TRAVELCOST.DOOR_OPEN_W_W:
                    fDoorIsObstacleIfClosed = false;
                    iDoorGridNo = iGridNo + dirDelta[WorldDirections.WEST] + dirDelta[WorldDirections.WEST];
                    break;
                case TRAVELCOST.DOOR_OPEN_SW_W:
                    fDoorIsObstacleIfClosed = false;
                    iDoorGridNo = iGridNo + dirDelta[WorldDirections.SOUTHWEST] + dirDelta[WorldDirections.WEST];
                    break;
                case TRAVELCOST.DOOR_OPEN_NW_W:
                    fDoorIsObstacleIfClosed = false;
                    iDoorGridNo = iGridNo + dirDelta[WorldDirections.NORTHWEST] + dirDelta[WorldDirections.WEST];
                    break;
                default:
                    break;
            }

            if (pSoldier is not null
                && (pSoldier.uiStatusFlags.HasFlag(SOLDIER.MONSTER)
                || pSoldier.uiStatusFlags.HasFlag(SOLDIER.ANIMAL)))
            {
                // can't open doors!
                ubReplacementCost = TRAVELCOST.OBSTACLE;
            }

            if (piDoorGridNo is not null)
            {
                // return gridno of door through pointer
                piDoorGridNo = iDoorGridNo;
            }

            if (fReturnPerceivedValue && Globals.gpWorldLevelData[iDoorGridNo].ubExtFlags[0].HasFlag(MAPELEMENT_EXT.DOOR_STATUS_PRESENT))
            {
                // check door status
                pDoorStatus = Keys.GetDoorStatus((int)iDoorGridNo);
                if (pDoorStatus is not null)
                {
                    fDoorIsOpen = (pDoorStatus.ubFlags.HasFlag(DOOR_STATUS_FLAGS.PERCEIVED_OPEN));
                }
                else
                {
                    // abort!
                    return (ubMovementCost);
                }
            }
            else
            {
                // check door structure
                pDoorStructure = WorldStructures.FindStructure(iDoorGridNo, STRUCTUREFLAGS.ANYDOOR);
                if (pDoorStructure is not null)
                {
                    fDoorIsOpen = (pDoorStructure.fFlags.HasFlag(STRUCTUREFLAGS.OPEN));
                }
                else
                {
                    // abort!
                    return (ubMovementCost);
                }
            }
            // now determine movement cost
            if (fDoorIsOpen)
            {
                if (fDoorIsObstacleIfClosed)
                {
                    ubMovementCost = Globals.gTileTypeMovementCost[(int)Globals.gpWorldLevelData[iGridNo].ubTerrainID];
                }
                else
                {
                    ubMovementCost = ubReplacementCost;
                }
            }
            else
            {
                if (fDoorIsObstacleIfClosed)
                {
                    // door is closed and this should be an obstacle, EXCEPT if we are calculating
                    // a path for an enemy or NPC with keys

                    // creatures and animals can't open doors!
                    if (fReturnPerceivedValue
                        || (pSoldier is not null && (pSoldier.uiStatusFlags.HasFlag(SOLDIER.MONSTER)
                        || pSoldier.uiStatusFlags.HasFlag(SOLDIER.ANIMAL))))
                    {
                        ubMovementCost = ubReplacementCost;
                    }
                    else
                    {
                        // have to check if door is locked and NPC does not have keys!
                        pDoor = Keys.FindDoorInfoAtGridNo(iDoorGridNo);
                        if (pDoor is not null)
                        {
                            if ((!pDoor.fLocked
                                || (pSoldier is not null && pSoldier.bHasKeys > 0)) && !fReturnDoorCost)
                            {
                                ubMovementCost = Globals.gTileTypeMovementCost[(int)Globals.gpWorldLevelData[iGridNo].ubTerrainID];
                            }
                            else
                            {
                                ubMovementCost = ubReplacementCost;
                            }
                        }
                        else
                        {
                            ubMovementCost = ubReplacementCost;
                        }
                    }
                }
                else
                {
                    ubMovementCost = Globals.gTileTypeMovementCost[(int)Globals.gpWorldLevelData[iGridNo].ubTerrainID];
                }
            }

        }
        return (ubMovementCost);

    }
}

// PLOT PATH defines
public enum PlotPathDefines
{
    NOT_STEALTH = 0,
    STEALTH = 1,
    NO_PLOT = 0,
    PLOT = 1,
    TEMPORARY = 0,
    PERMANENT = 1,
    FORWARD = 0,
    REVERSE = 1,
    NO_COPYROUTE = 0,
    COPYROUTE = 1,
    COPYREACHABLE = 2,
    COPYREACHABLE_AND_APS = 3,
    PATH_THROUGH_PEOPLE = 0x01,
    PATH_IGNORE_PERSON_AT_DEST = 0x02,
    PATH_CLOSE_GOOD_ENOUGH = 0x04,
    PATH_CLOSE_RADIUS = 5,
}

public class TRAVELCOST
{
    public const int NONE = 0;
    public const int FLAT = 10;
    public const int BUMPY = 12;
    public const int GRASS = 12;
    public const int THICK = 16;
    public const int DEBRIS = 20;
    public const int SHORE = 30;
    public const int KNEEDEEP = 36;
    public const int DEEPWATER = 50;
    public const int FENCE = 40;

    // these values are used to indicate "this is an obstacle
    // if there is a door (perceived) open/closed in this tile
    public const int DOOR_CLOSED_HERE = 220;
    public const int DOOR_CLOSED_N = 221;
    public const int DOOR_CLOSED_W = 222;
    public const int DOOR_OPEN_HERE = 223;
    public const int DOOR_OPEN_N = 224;
    public const int DOOR_OPEN_NE = 225;
    public const int DOOR_OPEN_E = 226;
    public const int DOOR_OPEN_SE = 227;
    public const int DOOR_OPEN_S = 228;
    public const int DOOR_OPEN_SW = 229;
    public const int DOOR_OPEN_W = 230;
    public const int DOOR_OPEN_NW = 231;
    public const int DOOR_OPEN_N_N = 232;
    public const int DOOR_OPEN_NW_N = 233;
    public const int DOOR_OPEN_NE_N = 234;
    public const int DOOR_OPEN_W_W = 235;
    public const int DOOR_OPEN_SW_W = 236;
    public const int DOOR_OPEN_NW_W = 237;
    public const int NOT_STANDING = 248;
    public const int OFF_MAP = 249;
    public const int CAVEWALL = 250;
    public const int HIDDENOBSTACLE = 251;
    public const int DOOR = 252;
    public const int OBSTACLE = 253;
    public const int WALL = 254;
    public const int EXITGRID = 255;
    public const int TRAINTRACKS = 30;
    public const int DIRTROAD = 9;
    public const int PAVEDROAD = 9;
    public const int FLATFLOOR = 10;
    public const int BLOCKED = OFF_MAP;

    public static bool IS_TRAVELCOST.DOOR(int x)
    {
        return (x >= TRAVELCOST.DOOR_CLOSED_HERE && x <= TRAVELCOST.DOOR_OPEN_NW_W);
    }

    public static bool IS_TRAVELCOST.CLOSED_DOOR(int x)
    {
        return (x >= TRAVELCOST.DOOR_CLOSED_HERE && ((int)x) << (int)TRAVELCOST.DOOR_CLOSED_W > 0);
    }
}

[Flags]
public enum STEPSTART
{
    GREEN = 0,
    RED = 16,
    PURPLE = 32,
    BLUE = 48,
    ORANGE = 64,
}

// OLD PATHAI STUFF
/////////////////////////////////////////////////
public class path_s
{
    public int iLocation;                        //4
    public path_s[] pNext = new path_s[ABSMAX_SKIPLIST_LEVEL];   //4 * MAX_SKIPLIST_LEVEL (5) = 20
    public int sPathNdx;                         //2
    TRAILCELLTYPE usCostSoFar;          //2
    TRAILCELLTYPE usCostToGo;           //2
    TRAILCELLTYPE usTotalCost;                  //2
    public int bLevel;                                //1
    public int ubTotalAPCost;                //1
    public int ubLegDistance;				//1
};
