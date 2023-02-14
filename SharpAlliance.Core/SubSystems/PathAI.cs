using System;
using Microsoft.Extensions.Logging;

namespace SharpAlliance.Core.SubSystems;

public class PathAI
{
    private readonly ILogger<PathAI> logger;
    private readonly GameSettings gGameSettings;
    private readonly Overhead overhead;
    private readonly Globals globals;

    public static bool gfPlotDirectPath { get; set; } = false;

    public PathAI(
        ILogger<PathAI> logger,
        GameSettings gameSettings,
        Overhead overhead,
        Globals globals)
    {
        this.logger = logger;
        this.gGameSettings = gameSettings;
        this.overhead = overhead;
        this.globals = globals;
    }

    public short UIPlotPath(
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
        short sRet;

        if (_KeyDown(SHIFT))
        {
            gfPlotDirectPath = true;
        }

        // If we are on the same level as the interface level, continue, else return
        if (pSold.bLevel != Interface.gsInterfaceLevel)
        {
            return (0);
        }

        if (gGameSettings.fOptions[TOPTION.ALWAYS_SHOW_MOVEMENT_PATH])
        {
            bPlot = true;
        }

        sRet = PlotPath(pSold, sDestGridno, bCopyRoute, bPlot, bStayOn, usMovementMode, bStealth, bReverse, sAPBudget);
        gfPlotDirectPath = false;
        return (sRet);
    }

    public short PlotPath(
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
        int sTileCost, sPoints = 0, sTempGrid, sAnimCost = 0;
        int sPointsWalk = 0, sPointsCrawl = 0, sPointsRun = 0, sPointsSwat = 0;
        AP sExtraCostStand, sExtraCostSwat, sExtraCostCrawl;
        int iLastGrid;
        int iCnt;
        int sOldGrid = 0;
        int sFootOrderIndex;
        TRAVELCOST sSwitchValue;
        STEPSTART[] sFootOrder = new STEPSTART[] { STEPSTART.GREEN, STEPSTART.PURPLE, STEPSTART.BLUE,
                                                    STEPSTART.ORANGE, STEPSTART.RED };
        int usTileIndex;
        ushort usTileNum;
        LEVELNODE? pNode;
        AnimationStates usMovementModeToUseForAPs;
        bool bIgnoreNextCost = false;
        short sTestGridno;

        if (bPlot && gusPathShown)
        {
            ErasePath(false);
        }

        gusAPtsToMove = 0;
        sTempGrid = (short)pSold.sGridNo;

        sFootOrderIndex = 0;


        //gubNPCMovementMode = (Ubyte) usMovementMode;
        // distance limit to reduce the cost of plotting a path to a location we can't reach

        // For now, use known hight adjustment
        if (gfRecalculatingExistingPathCost || FindBestPath(pSold, sDestGridno, (byte)pSold.bLevel, usMovementMode, bCopyRoute, 0))
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
            sTestGridno = NewGridNo(pSold.sGridNo, (short)DirectionInc((ushort)guiPathingData[0]));
            if (this.globals.gubWorldMovementCosts[sTestGridno, (byte)guiPathingData[0], pSold.bLevel] == TRAVELCOST.FENCE)
            {
                if (usMovementMode == AnimationStates.RUNNING && pSold.usAnimState != AnimationStates.RUNNING)
                {
                    sPoints -= AP.START_RUN_COST;
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
            gusAPtsToMove += sAnimCost;





            if (bStayOn)
            {
                iLastGrid = giPathDataSize + 1;
            }
            else
            {
                iLastGrid = giPathDataSize;
            }


            for (iCnt = 0; iCnt < iLastGrid; iCnt++)
            {
                sExtraCostStand = 0;
                sExtraCostSwat = 0;
                sExtraCostCrawl = 0;
                // what is the next gridno in the path?
                sOldGrid = sTempGrid;

                sTempGrid = NewGridNo(sTempGrid, (short)DirectionInc((ushort)guiPathingData[iCnt]));

                // Get switch value...
                sSwitchValue = this.globals.gubWorldMovementCosts[sTempGrid, (byte)guiPathingData[iCnt], pSold.bLevel];

                // get the tile cost for that tile based on WALKING
                sTileCost = TerrainActionPoints(pSold, sTempGrid, (byte)guiPathingData[iCnt], pSold.bLevel);

                usMovementModeToUseForAPs = usMovementMode;

                // ATE - MAKE MOVEMENT ALWAYS WALK IF IN WATER
                if (gpWorldLevelData[sTempGrid].ubTerrainID == DEEP_WATER || gpWorldLevelData[sTempGrid].ubTerrainID == MED_WATER || gpWorldLevelData[sTempGrid].ubTerrainID == LOW_WATER)
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

                                    sPoints += sExtraCostStand;
                                }
                                break;

                            case AnimationStates.SWATTING:

                                // Add cost to stand once there BEFORE....
                                sExtraCostSwat += AP.CROUCH;
                                sPoints += sExtraCostSwat;
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
                                    sExtraCostStand += AP.CROUCH;
                                    break;

                                default:
                                    break;
                            }
                        }

                        // so, then we must modify it for other movement styles and accumulate
                        switch (usMovementModeToUseForAPs)
                        {
                            case AnimationStates.RUNNING:
                                sPoints += (short)(double)((sTileCost / OverheadTypes.RUNDIVISOR)) + sExtraCostStand;
                                break;
                            case AnimationStates.WALKING:
                                sPoints += (sTileCost + OverheadTypes.WALKCOST) + sExtraCostStand;
                                break;
                            case AnimationStates.SWATTING:
                                sPoints += (sTileCost + OverheadTypes.SWATCOST) + sExtraCostSwat;
                                break;
                            case AnimationStates.CRAWLING:
                                sPoints += (sTileCost + OverheadTypes.CRAWLCOST) + sExtraCostCrawl;
                                break;
                            default:
                                sPoints += sTileCost;
                                break;
                        }
                    }
                }

                // THIS NEXT SECTION ONLY NEEDS TO HAPPEN FOR CURSOR UI FEEDBACK, NOT ACTUAL COSTING

                if (bPlot && ((this.overhead.gTacticalStatus.uiFlags & TURNBASED) && (this.overhead.gTacticalStatus.uiFlags & INCOMBAT))) // OR USER OPTION ON... ***)
                {
                    // ATE; TODO: Put stuff in here to allow for fact of costs other than movement ( jump fence, open door )

                    // store WALK cost
                    sPointsWalk += (sTileCost + OverheadTypes.WALKCOST) + (int)sExtraCostStand;

                    // now get cost as if CRAWLING
                    sPointsCrawl += (sTileCost + OverheadTypes.CRAWLCOST) + sExtraCostCrawl;

                    // now get cost as if SWATTING
                    sPointsSwat += (sTileCost + OverheadTypes.SWATCOST) + sExtraCostSwat;

                    // now get cost as if RUNNING
                    sPointsRun += (short)(double)((sTileCost / OverheadTypes.RUNDIVISOR)) + sExtraCostStand;
                }

                if (iCnt == 0 && bPlot)
                {
                    gusAPtsToMove = sPoints;

                    giPlotCnt = 0;

                }


                //if ( gTacticalStatus.uiFlags & TURNBASED && (gTacticalStatus.uiFlags & INCOMBAT) ) // OR USER OPTION "show paths" ON... ***
                {
                    if (bPlot && ((iCnt < (iLastGrid - 1)) || (iCnt < iLastGrid && bStayOn)))
                    {
                        guiPlottedPath[giPlotCnt++] = sTempGrid;

                        // we need a footstep graphic ENTERING the next tile

                        // get the direction
                        usTileNum = (ushort)guiPathingData[iCnt] + 2;
                        if (usTileNum > 8)
                        {
                            usTileNum = 1;
                        }

                        // Are we a vehicle?
                        if (pSold.uiStatusFlags & SOLDIER.VEHICLE)
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

                        GetTileIndexFromTypeSubIndex(FOOTPRINTS, (ushort)usTileNum, usTileIndex);

                        // Adjust based on what mode we are in...
                        if ((this.overhead.gTacticalStatus.uiFlags & TacticalEngineStatus.REALTIME) || !(this.overhead.gTacticalStatus.uiFlags & INCOMBAT))
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
                            pNode = AddObjectToTail(sTempGrid, usTileIndex);
                            pNode.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                            pNode.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                        }
                        else
                        {
                            pNode = AddOnRoofToTail(sTempGrid, usTileIndex);
                            pNode.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                            pNode.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                        }



                        // we need a footstep graphic LEAVING this tile

                        // get the direction using the NEXT tile (thus iCnt+1 as index)
                        usTileNum = (ushort)guiPathingData[iCnt + 1] + 2;
                        if (usTileNum > 8)
                        {
                            usTileNum = 1;
                        }


                        // this is a LEAVING footstep which is always the second set of 8
                        usTileNum += 8;

                        GetTileIndexFromTypeSubIndex(FOOTPRINTS, (ushort)usTileNum, usTileIndex);

                        // Adjust based on what mode we are in...
                        if ((this.overhead.gTacticalStatus.uiFlags & TacticalEngineStatus.REALTIME) || !(this.overhead.gTacticalStatus.uiFlags & INCOMBAT))
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
                            pNode = AddObjectToTail(sTempGrid, usTileIndex);
                            pNode.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                            pNode.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                        }
                        else
                        {
                            pNode = AddOnRoofToTail(sTempGrid, usTileIndex);
                            pNode.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                            pNode.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                        }
                    }

                }       // end of if turn based or real-time user option "show paths" on...
            }

            if (bPlot)
            {
                gusPathShown = true;
            }

        }   // end of found a path

        // reset distance limit 
        gubNPCDistLimit = 0;

        return (sPoints);
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

public enum TRAVELCOST
{
    NONE = 0,
    FLAT = 10,
    BUMPY = 12,
    GRASS = 12,
    THICK = 16,
    DEBRIS = 20,
    SHORE = 30,
    KNEEDEEP = 36,
    DEEPWATER = 50,
    FENCE = 40,

    // these values are used to indicate "this is an obstacle
    // if there is a door (perceived) open/closed in this tile
    DOOR_CLOSED_HERE = 220,
    DOOR_CLOSED_N = 221,
    DOOR_CLOSED_W = 222,
    DOOR_OPEN_HERE = 223,
    DOOR_OPEN_N = 224,
    DOOR_OPEN_NE = 225,
    DOOR_OPEN_E = 226,
    DOOR_OPEN_SE = 227,
    DOOR_OPEN_S = 228,
    DOOR_OPEN_SW = 229,
    DOOR_OPEN_W = 230,
    DOOR_OPEN_NW = 231,
    DOOR_OPEN_N_N = 232,
    DOOR_OPEN_NW_N = 233,
    DOOR_OPEN_NE_N = 234,
    DOOR_OPEN_W_W = 235,
    DOOR_OPEN_SW_W = 236,
    DOOR_OPEN_NW_W = 237,
    NOT_STANDING = 248,
    OFF_MAP = 249,
    CAVEWALL = 250,
    HIDDENOBSTACLE = 251,
    DOOR = 252,
    OBSTACLE = 253,
    WALL = 254,
    EXITGRID = 255,
    TRAINTRACKS = 30,
    DIRTROAD = 9,
    PAVEDROAD = 9,
    FLATFLOOR = 10,
    BLOCKED = OFF_MAP,
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

public static class TRAVELCOSTExtensions
{
    public static bool IS_TRAVELCOST_DOOR(this TRAVELCOST x)
    {
        return (x >= TRAVELCOST.DOOR_CLOSED_HERE && x <= TRAVELCOST.DOOR_OPEN_NW_W);
    }

    public static bool IS_TRAVELCOST_CLOSED_DOOR(this TRAVELCOST x)
    {
        return (x >= TRAVELCOST.DOOR_CLOSED_HERE && ((int)x) << (int)TRAVELCOST.DOOR_CLOSED_W > 0);
    }
}
