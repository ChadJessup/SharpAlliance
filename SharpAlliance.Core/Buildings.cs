using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.SubSystems;
using static SharpAlliance.Core.Globals;
namespace SharpAlliance.Core;

public class Buildings
{
    BUILDING? CreateNewBuilding(int? pubBuilding)
    {
        if (gubNumberOfBuildings + 1 >= MAX_BUILDINGS)
        {
            return (null);
        }
        // increment # of buildings
        gubNumberOfBuildings++;
        // clear entry
        gBuildings[gubNumberOfBuildings].ubNumClimbSpots = 0;
        pubBuilding = gubNumberOfBuildings;
        // return pointer (have to subtract 1 since we just added 1
        return ((gBuildings[gubNumberOfBuildings]));
    }

    BUILDING? GenerateBuilding(int sDesiredSpot)
    {
        int uiLoop;
        int sTempGridNo, sNextTempGridNo, sVeryTemporaryGridNo;
        int sStartGridNo, sCurrGridNo, sPrevGridNo = NOWHERE, sRightGridNo;
        WorldDirections bDirection, bTempDirection;
        bool fFoundDir, fFoundWall;
        int uiChanceIn = ROOF_LOCATION_CHANCE; // chance of a location being considered
        int sWallGridNo;
        WallOrientation bDesiredOrientation;
        int bSkipSpots = 0;
        SOLDIERTYPE FakeSoldier;
        BUILDING? pBuilding;
        int ubBuildingID = 0;

        pBuilding = CreateNewBuilding(ubBuildingID);
        if (pBuilding is null)
        {
            return (null);
        }

        // set up fake soldier for location testing
        FakeSoldier = new()
        {
            sGridNo = sDesiredSpot,
            bLevel = 1,
            bTeam = TEAM.ENEMY_TEAM,
        };

#if ROOF_DEBUG
        memset(gsCoverValue, 0x7F, sizeof(int) * WORLD_MAX);
#endif

        // Set reachable 
        PathAI.RoofReachableTest(sDesiredSpot, ubBuildingID);

        // From sGridNo, search until we find a spot that isn't part of the building
        bDirection = WorldDirections.NORTHWEST;
        sTempGridNo = sDesiredSpot;
        // using diagonal directions to hopefully prevent picking a 
        // spot that 
        while ((gpWorldLevelData[sTempGridNo].uiFlags & MAPELEMENT_REACHABLE))
        {
            sNextTempGridNo = NewGridNo(sTempGridNo, DirectionInc(bDirection));
            if (sTempGridNo == sNextTempGridNo)
            {
                // hit edge of map!??!
                return (null);
            }
            else
            {
                sTempGridNo = sNextTempGridNo;
            }
        }

        // we've got our spot
        sStartGridNo = sTempGridNo;

        sCurrGridNo = sStartGridNo;
        sVeryTemporaryGridNo = NewGridNo(sCurrGridNo, DirectionInc(EAST));
        if (gpWorldLevelData[sVeryTemporaryGridNo].uiFlags & MAPELEMENT_REACHABLE)
        {
            // go north first
            bDirection = NORTH;
        }
        else
        {
            // go that way (east)
            bDirection = EAST;
        }

        gpWorldLevelData[sStartGridNo].ubExtFlags[0] |= MAPELEMENT_EXT_ROOFCODE_VISITED;

        while (1)
        {

            // if point to (2 clockwise) is not part of building and is not visited,
            // or is starting point, turn!
            sRightGridNo = NewGridNo(sCurrGridNo, DirectionInc(gTwoCDirection[bDirection]));
            sTempGridNo = sRightGridNo;
            if (((!(gpWorldLevelData[sTempGridNo].uiFlags & MAPELEMENT_REACHABLE) && !(gpWorldLevelData[sTempGridNo].ubExtFlags[0] & MAPELEMENT_EXT_ROOFCODE_VISITED)) || (sTempGridNo == sStartGridNo)) && (sCurrGridNo != sStartGridNo))
            {
                bDirection = gTwoCDirection[bDirection];
                // try in that direction
                continue;
            }

            // if spot ahead is part of building, turn
            sTempGridNo = NewGridNo(sCurrGridNo, DirectionInc(bDirection));
            if (gpWorldLevelData[sTempGridNo].uiFlags & MAPELEMENT_REACHABLE)
            {
                // first search for a spot that is neither part of the building or visited

                // we KNOW that the spot in the original direction is blocked, so only loop 3 times
                bTempDirection = gTwoCDirection[bDirection];
                fFoundDir = false;
                for (uiLoop = 0; uiLoop < 3; uiLoop++)
                {
                    sTempGridNo = NewGridNo(sCurrGridNo, DirectionInc(bTempDirection));
                    if (!(gpWorldLevelData[sTempGridNo].uiFlags & MAPELEMENT_REACHABLE) && !(gpWorldLevelData[sTempGridNo].ubExtFlags[0] & MAPELEMENT_EXT_ROOFCODE_VISITED))
                    {
                        // this is the way to go!
                        fFoundDir = true;
                        break;
                    }
                    bTempDirection = gTwoCDirection[bTempDirection];
                }
                if (!fFoundDir)
                {
                    // now search for a spot that is just not part of the building
                    bTempDirection = gTwoCDirection[bDirection];
                    fFoundDir = false;
                    for (uiLoop = 0; uiLoop < 3; uiLoop++)
                    {
                        sTempGridNo = NewGridNo(sCurrGridNo, DirectionInc(bTempDirection));
                        if (!(gpWorldLevelData[sTempGridNo].uiFlags & MAPELEMENT_REACHABLE))
                        {
                            // this is the way to go!
                            fFoundDir = true;
                            break;
                        }
                        bTempDirection = gTwoCDirection[bTempDirection];
                    }
                    if (!fFoundDir)
                    {
                        // WTF is going on?
                        return (null);
                    }
                }
                bDirection = bTempDirection;
                // try in that direction
                continue;
            }

            // move ahead
            sPrevGridNo = sCurrGridNo;
            sCurrGridNo = sTempGridNo;
            sRightGridNo = NewGridNo(sCurrGridNo, DirectionInc(gTwoCDirection[bDirection]));

# if ROOF_DEBUG
            if (gsCoverValue[sCurrGridNo] == 0x7F7F)
            {
                gsCoverValue[sCurrGridNo] = 1;
            }
            else if (gsCoverValue[sCurrGridNo] >= 0)
            {
                gsCoverValue[sCurrGridNo]++;
            }

            DebugAI(String("Roof code visits %d", sCurrGridNo));
#endif

            if (sCurrGridNo == sStartGridNo)
            {
                // done
                break;
            }

            if (!(gpWorldLevelData[sCurrGridNo].ubExtFlags[0] & MAPELEMENT_EXT_ROOFCODE_VISITED))
            {
                gpWorldLevelData[sCurrGridNo].ubExtFlags[0] |= MAPELEMENT_EXT_ROOFCODE_VISITED;

                // consider this location as possible climb gridno		
                // there must be a regular wall adjacent to this for us to consider it a 
                // climb gridno

                // if the direction is east or north, the wall would be in our gridno;
                // if south or west, the wall would be in the gridno two clockwise
                fFoundWall = false;

                switch (bDirection)
                {
                    case NORTH:
                        sWallGridNo = sCurrGridNo;
                        bDesiredOrientation = OUTSIDE_TOP_RIGHT;
                        break;
                    case EAST:
                        sWallGridNo = sCurrGridNo;
                        bDesiredOrientation = OUTSIDE_TOP_LEFT;
                        break;
                    case SOUTH:
                        sWallGridNo = (int)(sCurrGridNo + DirectionInc(gTwoCDirection[bDirection]));
                        bDesiredOrientation = OUTSIDE_TOP_RIGHT;
                        break;
                    case WEST:
                        sWallGridNo = (int)(sCurrGridNo + DirectionInc(gTwoCDirection[bDirection]));
                        bDesiredOrientation = OUTSIDE_TOP_LEFT;
                        break;
                    default:
                        // what the heck?
                        return (null);
                }

                if (bDesiredOrientation == WallOrientation.OUTSIDE_TOP_LEFT)
                {
                    if (WallExistsOfTopLeftOrientation(sWallGridNo))
                    {
                        fFoundWall = true;
                    }
                }
                else
                {
                    if (WallExistsOfTopRightOrientation(sWallGridNo))
                    {
                        fFoundWall = true;
                    }
                }

                if (fFoundWall)
                {
                    if (bSkipSpots > 0)
                    {
                        bSkipSpots--;
                    }
                    else if (Globals.Random.Next(uiChanceIn) == 0)
                    {
                        // don't consider people as obstacles
                        if (NewOKDestination(&FakeSoldier, sCurrGridNo, false, 0))
                        {
                            pBuilding.sUpClimbSpots[pBuilding.ubNumClimbSpots] = sCurrGridNo;
                            pBuilding.sDownClimbSpots[pBuilding.ubNumClimbSpots] = sRightGridNo;
                            pBuilding.ubNumClimbSpots++;

                            if (pBuilding.ubNumClimbSpots == MAX_CLIMBSPOTS_PER_BUILDING)
                            {
                                // gotta stop!
                                return (pBuilding);
                            }

                            // if location is added as a spot, reset uiChanceIn
                            uiChanceIn = ROOF_LOCATION_CHANCE;
# if ROOF_DEBUG
                            gsCoverValue[sCurrGridNo] = 99;
#endif
                            // skip the next spot
                            bSkipSpots = 1;
                        }
                        else
                        {
                            // if location is not added, 100% chance of handling next location
                            // and the next until we can add one
                            uiChanceIn = 1;

                        }
                    }
                    else
                    {
                        // didn't pick this location, so increase chance that next location 
                        // will be considered
                        if (uiChanceIn > 2)
                        {
                            uiChanceIn--;
                        }
                    }

                }
                else
                {
                    // can't select this spot
                    if ((sPrevGridNo != NOWHERE) && (pBuilding.ubNumClimbSpots > 0))
                    {
                        if (pBuilding.sDownClimbSpots[pBuilding.ubNumClimbSpots - 1] == sCurrGridNo)
                        {
                            // unselect previous spot
                            pBuilding.ubNumClimbSpots--;
                            // overwrote a selected spot so go into automatic selection for later
                            uiChanceIn = 1;
# if ROOF_DEBUG
                            // reset marker
                            gsCoverValue[sPrevGridNo] = 1;
#endif
                        }
                    }

                    // skip the next gridno
                    bSkipSpots = 1;
                }

            }

        }

        // at end could prune # of locations if there are too many

        /*
        #if ROOF_DEBUG
            SetRenderFlags( RENDER_FLAG_FULL );
            RenderWorld();
            RenderCoverDebug( );
            InvalidateScreen( );
            EndFrameBufferRender();
            RefreshScreen( null );
        #endif
        */

        return (pBuilding);
    }

    private static BUILDING? FindBuilding(int sGridNo)
    {
        int ubBuildingID;
        //int					ubRoomNo;

        if (sGridNo <= 0 || sGridNo > WORLD_MAX)
        {
            return (null);
        }

        // id 0 indicates no building
        ubBuildingID = gubBuildingInfo[sGridNo];
        if (ubBuildingID == NO_BUILDING)
        {
            return (null);
            /*
            // need extra checks to see if is valid spot... 
            // must have valid room information and be a flat-roofed
            // building
            if ( InARoom( sGridNo, &ubRoomNo ) && (FindStructure( sGridNo, STRUCTURE_NORMAL_ROOF ) != null) )
            {
                return( GenerateBuilding( sGridNo ) );
            }
            else
            {
                return( null );
            }
            */
        }
        else if (ubBuildingID > gubNumberOfBuildings) // huh?
        {
            return (null);
        }

        return ((gBuildings[ubBuildingID]));
    }

    public static bool InBuilding(int sGridNo)
    {
        if (FindBuilding(sGridNo) == null)
        {
            return (false);
        }
        return (true);
    }


    void GenerateBuildings()
    {
        int uiLoop;

        gubNumberOfBuildings = 0;

        if ((gbWorldSectorZ > 0) || gfEditMode)
        {
            return;
        }

        // reset ALL reachable flags
        // do once before we start building generation for
        // whole map
        for (uiLoop = 0; uiLoop < WORLD_MAX; uiLoop++)
        {
            gpWorldLevelData[uiLoop].uiFlags &= ~(MAPELEMENTFLAGS.REACHABLE);
            gpWorldLevelData[uiLoop].ubExtFlags[0] &= ~(MAPELEMENT_EXT.ROOFCODE_VISITED);
        }

        // search through world
        // for each location in a room try to find building info

        for (uiLoop = 0; uiLoop < WORLD_MAX; uiLoop++)
        {
            if ((gubWorldRoomInfo[uiLoop] != NO_ROOM) && (gubBuildingInfo[uiLoop] == NO_BUILDING)
                && (WorldStructures.FindStructure(uiLoop, STRUCTUREFLAGS.NORMAL_ROOF) != null))
            {
                GenerateBuilding((int)uiLoop);
            }
        }
    }

    int FindClosestClimbPoint(int sStartGridNo, int sDesiredGridNo, bool fClimbUp)
    {
        BUILDING? pBuilding;
        int ubNumClimbSpots;
        int[] psClimbSpots;
        int ubLoop;
        int sDistance, sClosestDistance = 1000, sClosestSpot = NOWHERE;

        pBuilding = FindBuilding(sDesiredGridNo);
        if (pBuilding is null)
        {
            return (NOWHERE);
        }

        ubNumClimbSpots = pBuilding.ubNumClimbSpots;

        if (fClimbUp)
        {
            psClimbSpots = pBuilding.sUpClimbSpots;
        }
        else
        {
            psClimbSpots = pBuilding.sDownClimbSpots;
        }

        for (ubLoop = 0; ubLoop < ubNumClimbSpots; ubLoop++)
        {
            if ((WorldManager.WhoIsThere2(pBuilding.sUpClimbSpots[ubLoop], 0) == NOBODY)
                && (WorldManager.WhoIsThere2(pBuilding.sDownClimbSpots[ubLoop], 1) == NOBODY))
            {
                sDistance = IsometricUtils.PythSpacesAway(sStartGridNo, psClimbSpots[ubLoop]);
                if (sDistance < sClosestDistance)
                {
                    sClosestDistance = sDistance;
                    sClosestSpot = psClimbSpots[ubLoop];
                }
            }
        }

        return (sClosestSpot);
    }

    bool SameBuilding(int sGridNo1, int sGridNo2)
    {
        if (gubBuildingInfo[sGridNo1] == NO_BUILDING)
        {
            return (false);
        }
        if (gubBuildingInfo[sGridNo2] == NO_BUILDING)
        {
            return (false);
        }
        return ((bool)(gubBuildingInfo[sGridNo1] == gubBuildingInfo[sGridNo2]));
    }
}

public class BUILDING
{
    public int[] sUpClimbSpots = new int[MAX_CLIMBSPOTS_PER_BUILDING];
    public int[] sDownClimbSpots = new int[MAX_CLIMBSPOTS_PER_BUILDING];
    public int ubNumClimbSpots;
};

