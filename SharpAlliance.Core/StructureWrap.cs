using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class StructureWrap
{
    bool IsFencePresentAtGridno(int sGridNo)
    {
        if (FindStructure(sGridNo, STRUCTURE_ANYFENCE) != null)
        {
            return (true);
        }

        return (false);
    }

    bool IsRoofPresentAtGridno(int sGridNo)
    {
        if (FindStructure(sGridNo, STRUCTURE_ROOF) != null)
        {
            return (true);
        }

        return (false);
    }


    bool IsJumpableFencePresentAtGridno(int sGridNo)
    {
        STRUCTURE? pStructure;

        pStructure = FindStructure(sGridNo, STRUCTUREFLAGS.OBSTACLE);

        if (pStructure is not null)
        {
            if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.FENCE) && !(pStructure.fFlags.HasFlag(STRUCTUREFLAGS.SPECIAL)))
            {
                return (true);
            }
            if (pStructure.pDBStructureRef.pDBStructure.ubArmour == MATERIAL.SANDBAG && StructureHeight(pStructure) < 2)
            {
                return (true);
            }
        }

        return (false);
    }


    bool IsDoorPresentAtGridno(int sGridNo)
    {
        if (FindStructure(sGridNo, STRUCTUREFLAGS.ANYDOOR) != null)
        {
            return (true);
        }

        return (false);
    }


    bool IsTreePresentAtGridno(int sGridNo)
    {
        if (FindStructure(sGridNo, STRUCTUREFLAGS.TREE) != null)
        {
            return (true);
        }

        return (false);
    }


    LEVELNODE? IsWallPresentAtGridno(int sGridNo)
    {
        LEVELNODE? pNode = null;
        STRUCTURE? pStructure;

        pStructure = FindStructure(sGridNo, STRUCTUREFLAGS.WALLSTUFF);

        if (pStructure != null)
        {
            pNode = FindLevelNodeBasedOnStructure(sGridNo, pStructure);
        }

        return (pNode);
    }

    LEVELNODE? GetWallLevelNodeOfSameOrientationAtGridno(int sGridNo, WallOrientation ubOrientation)
    {
        LEVELNODE? pNode = null;
        STRUCTURE? pStructure;

        pStructure = FindStructure(sGridNo, STRUCTUREFLAGS.WALLSTUFF);

        while (pStructure != null)
        {
            // Check orientation
            if (pStructure.ubWallOrientation == ubOrientation)
            {
                pNode = FindLevelNodeBasedOnStructure(sGridNo, pStructure);
                return (pNode);
            }
            pStructure = FindNextStructure(pStructure, STRUCTUREFLAGS.WALLSTUFF);
        }

        return (null);
    }


    LEVELNODE? GetWallLevelNodeAndStructOfSameOrientationAtGridno(int sGridNo, int ubOrientation, STRUCTURE? ppStructure)
    {
        LEVELNODE? pNode = null;
        STRUCTURE? pStructure, pBaseStructure;

        (ppStructure) = null;

        pStructure = FindStructure(sGridNo, STRUCTUREFLAGS.WALLSTUFF);

        while (pStructure != null)
        {
            // Check orientation
            if (pStructure.ubWallOrientation == ubOrientation)
            {
                pBaseStructure = FindBaseStructure(pStructure);
                if (pBaseStructure)
                {
                    pNode = FindLevelNodeBasedOnStructure(pBaseStructure.sGridNo, pBaseStructure);
                    (ppStructure) = pBaseStructure;
                    return (pNode);
                }
            }
            pStructure = FindNextStructure(pStructure, STRUCTUREFLAGS.WALLSTUFF);
        }

        return (null);
    }


    bool IsDoorVisibleAtGridNo(int sGridNo)
    {
        STRUCTURE? pStructure;
        int sNewGridNo;

        pStructure = FindStructure(sGridNo, STRUCTUREFLAGS.ANYDOOR);

        if (pStructure != null)
        {
            // Check around based on orientation
            switch (pStructure.ubWallOrientation)
            {
                case WallOrientation.INSIDE_TOP_LEFT:
                case WallOrientation.OUTSIDE_TOP_LEFT:

                    // Here, check north direction
                    sNewGridNo = IsometricUtils.NewGridNo(sGridNo, IsometricUtils.DirectionInc(WorldDirections.NORTH));

                    if (IsRoofVisible2(sNewGridNo))
                    {
                        // OK, now check south, if true, she's not visible
                        sNewGridNo = IsometricUtils.NewGridNo(sGridNo, IsometricUtils.DirectionInc(WorldDirections.SOUTH));

                        if (IsRoofVisible2(sNewGridNo))
                        {
                            return (false);
                        }
                    }
                    break;

                case WallOrientation.INSIDE_TOP_RIGHT:
                case WallOrientation.OUTSIDE_TOP_RIGHT:

                    // Here, check west direction
                    sNewGridNo = IsometricUtils.NewGridNo(sGridNo, IsometricUtils.DirectionInc(WorldDirections.WEST));

                    if (IsRoofVisible2(sNewGridNo))
                    {
                        // OK, now check south, if true, she's not visible
                        sNewGridNo = IsometricUtils.NewGridNo(sGridNo, IsometricUtils.DirectionInc(WorldDirections.EAST));

                        if (IsRoofVisible2(sNewGridNo))
                        {
                            return (false);
                        }
                    }
                    break;

            }

        }

        // Return true here, even if she does not exist
        return (true);
    }


    bool DoesGridnoContainHiddenStruct(int sGridNo, bool pfVisible)
    {
        // ATE: These are ignored now - always return false

        //STRUCTURE *pStructure;

        //pStructure = FindStructure( sGridNo, STRUCTURE_HIDDEN );

        //if ( pStructure != null )
        //{
        //	if ( !(gpWorldLevelData[ sGridNo ].uiFlags & MAPELEMENT_REVEALED ) && !(gTacticalStatus.uiFlags&SHOW_ALL_MERCS)  )
        //	{
        //		*pfVisible = false;
        //	}
        //	else
        //	{
        //		*pfVisible = true;
        //	}//
        //
        //	return( true );
        //}

        return (false);
    }


    bool IsHiddenStructureVisible(int sGridNo, int usIndex)
    {
        // Check if it's a hidden struct and we have not revealed anything!
        if (gTileDatabase[usIndex].uiFlags & HIDDEN_TILE)
        {
            if (!(gpWorldLevelData[sGridNo].uiFlags.HasFlag(MAPELEMENTFLAGS.REVEALED))
                && !(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.SHOW_ALL_MERCS)))
            {
                // Return false
                return (false);
            }
        }

        return (true);
    }


    bool WallExistsOfTopLeftOrientation(int sGridNo)
    {
        // CJC: changing to search only for normal walls, July 16, 1998
        STRUCTURE? pStructure;

        pStructure = FindStructure(sGridNo, STRUCTUREFLAGS.WALL);

        while (pStructure != null)
        {
            // Check orientation
            if (pStructure.ubWallOrientation == WallOrientation.INSIDE_TOP_LEFT
                || pStructure.ubWallOrientation == WorldDirections.OUTSIDE_TOP_LEFT)
            {
                return (true);
            }

            pStructure = FindNextStructure(pStructure, STRUCTUREFLAGS.WALL);

        }

        return (false);
    }

    bool WallExistsOfTopRightOrientation(int sGridNo)
    {
        // CJC: changing to search only for normal walls, July 16, 1998
        STRUCTURE? pStructure;

        pStructure = FindStructure(sGridNo, STRUCTUREFLAGS.WALL);

        while (pStructure != null)
        {
            // Check orientation
            if (pStructure.ubWallOrientation == WallOrientation.INSIDE_TOP_RIGHT
                || pStructure.ubWallOrientation == WallOrientation.OUTSIDE_TOP_RIGHT)
            {
                return (true);
            }

            pStructure = FindNextStructure(pStructure, STRUCTUREFLAGS.WALL);

        }

        return (false);
    }

    bool WallOrClosedDoorExistsOfTopLeftOrientation(int sGridNo)
    {
        STRUCTURE? pStructure;

        pStructure = FindStructure(sGridNo, STRUCTUREFLAGS.WALLSTUFF);

        while (pStructure != null)
        {
            // skip it if it's an open door
            if (!((pStructure.fFlags.HasFlag(STRUCTUREFLAGS.ANYDOOR))
                && (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.OPEN))))
            {
                // Check orientation
                if (pStructure.ubWallOrientation == WallOrientation.INSIDE_TOP_LEFT
                    || pStructure.ubWallOrientation == WallOrientation.OUTSIDE_TOP_LEFT)
                {
                    return (true);
                }
            }

            pStructure = FindNextStructure(pStructure, STRUCTUREFLAGS.WALLSTUFF);

        }

        return (false);
    }

    bool WallOrClosedDoorExistsOfTopRightOrientation(int sGridNo)
    {
        STRUCTURE? pStructure;

        pStructure = FindStructure(sGridNo, STRUCTUREFLAGS.WALLSTUFF);

        while (pStructure != null)
        {
            // skip it if it's an open door
            if (!((pStructure.fFlags.HasFlag(STRUCTUREFLAGS.ANYDOOR)) && (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.OPEN))))
            {
                // Check orientation
                if (pStructure.ubWallOrientation == WallOrientation.INSIDE_TOP_RIGHT
                    || pStructure.ubWallOrientation == WallOrientation.OUTSIDE_TOP_RIGHT)
                {
                    return (true);
                }
            }

            pStructure = FindNextStructure(pStructure, STRUCTUREFLAGS.WALLSTUFF);

        }

        return (false);
    }

    bool OpenRightOrientedDoorWithDoorOnRightOfEdgeExists(int sGridNo)
    {
        STRUCTURE? pStructure;

        pStructure = FindStructure(sGridNo, STRUCTUREFLAGS.ANYDOOR);

        while (pStructure != null && (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.OPEN)))
        {
            // Check orientation
            if (pStructure.ubWallOrientation == WallOrientation.INSIDE_TOP_RIGHT
                || pStructure.ubWallOrientation == WallOrientation.OUTSIDE_TOP_RIGHT)
            {
                if ((pStructure.fFlags & STRUCTURE_DOOR) || (pStructure.fFlags & STRUCTURE_DDOOR_RIGHT))
                {
                    return (true);
                }
            }

            pStructure = FindNextStructure(pStructure, STRUCTURE_ANYDOOR);

        }

        return (false);
    }

    bool OpenLeftOrientedDoorWithDoorOnLeftOfEdgeExists(int sGridNo)
    {
        STRUCTURE? pStructure;

        pStructure = FindStructure(sGridNo, STRUCTURE_ANYDOOR);

        while (pStructure != null && (pStructure.fFlags & STRUCTURE_OPEN))
        {
            // Check orientation
            if (pStructure.ubWallOrientation == INSIDE_TOP_LEFT || pStructure.ubWallOrientation == OUTSIDE_TOP_LEFT)
            {
                if ((pStructure.fFlags & STRUCTURE_DOOR) || (pStructure.fFlags & STRUCTURE_DDOOR_LEFT))
                {
                    return (true);
                }
            }

            pStructure = FindNextStructure(pStructure, STRUCTURE_ANYDOOR);

        }

        return (false);
    }

    STRUCTURE? FindCuttableWireFenceAtGridNo(int sGridNo)
    {
        STRUCTURE? pStructure;

        pStructure = FindStructure(sGridNo, STRUCTURE_WIREFENCE);
        if (pStructure != null && pStructure.ubWallOrientation != NO_ORIENTATION && !(pStructure.fFlags & STRUCTURE_OPEN))
        {
            return (pStructure);
        }
        return (null);
    }

    bool CutWireFence(int sGridNo)
    {
        STRUCTURE? pStructure;

        pStructure = FindCuttableWireFenceAtGridNo(sGridNo);
        if (pStructure)
        {
            pStructure = SwapStructureForPartnerAndStoreChangeInMap(sGridNo, pStructure);
            if (pStructure)
            {
                RecompileLocalMovementCosts(sGridNo);
                SetRenderFlags(RENDER_FLAG_FULL);
                return (true);
            }
        }
        return (false);
    }

    bool IsCuttableWireFenceAtGridNo(int sGridNo)
    {
        return (FindCuttableWireFenceAtGridNo(sGridNo) != null);
    }


    int IsRepairableStructAtGridNo(int sGridNo, int? pubID)
    {
        int ubMerc;

        // OK, first look for a vehicle....
        ubMerc = WhoIsThere2(sGridNo, 0);

        if (pubID != null)
        {
            (pubID) = ubMerc;
        }

        if (ubMerc != NOBODY)
        {
            if (MercPtrs[ubMerc].uiStatusFlags.HasFlag(SOLDIER.VEHICLE))
            {
                return (2);
            }
        }
        // Then for over a robot....

        // then for SAM site....
        if (DoesSAMExistHere(gWorldSectorX, gWorldSectorY, gbWorldSectorZ, sGridNo))
        {
            return (3);
        }


        return (false);
    }


    bool IsRefuelableStructAtGridNo(int sGridNo, int? pubID)
    {
        int ubMerc;

        // OK, first look for a vehicle....
        ubMerc = WhoIsThere2(sGridNo, 0);

        if (pubID != null)
        {
            (pubID) = ubMerc;
        }

        if (ubMerc != NOBODY)
        {
            if (MercPtrs[ubMerc].uiStatusFlags.HasFlag(SOLDIER.VEHICLE))
            {
                return (true);
            }
        }
        return (false);
    }

    bool IsCutWireFenceAtGridNo(int sGridNo)
    {
        STRUCTURE? pStructure;

        pStructure = WorldStructures.FindStructure(sGridNo, STRUCTUREFLAGS.WIREFENCE);
        if (pStructure != null && (pStructure.ubWallOrientation != WallOrientation.NO_ORIENTATION)
            && (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.OPEN)))
        {
            return (true);
        }
        return (false);
    }



    int FindDoorAtGridNoOrAdjacent(int sGridNo)
    {
        STRUCTURE? pStructure;
        STRUCTURE? pBaseStructure;
        int sTestGridNo;

        sTestGridNo = sGridNo;
        pStructure = WorldStructures.FindStructure(sTestGridNo, STRUCTUREFLAGS.ANYDOOR);
        if (pStructure is not null)
        {
            pBaseStructure = WorldStructures.FindBaseStructure(pStructure);
            return (pBaseStructure.sGridNo);
        }

        sTestGridNo = sGridNo + IsometricUtils.DirectionInc(WorldDirections.NORTH);
        pStructure = WorldStructures.FindStructure(sTestGridNo, STRUCTUREFLAGS.ANYDOOR);
        if (pStructure is not null)
        {
            pBaseStructure = WorldStructures.FindBaseStructure(pStructure);
            return (pBaseStructure.sGridNo);
        }

        sTestGridNo = sGridNo + IsometricUtils.DirectionInc(WorldDirections.WEST);
        pStructure = WorldStructures.FindStructure(sTestGridNo, STRUCTUREFLAGS.ANYDOOR);
        if (pStructure is not null)
        {
            pBaseStructure = WorldStructures.FindBaseStructure(pStructure);
            return (pBaseStructure.sGridNo);
        }

        return (NOWHERE);
    }



    bool IsCorpseAtGridNo(int sGridNo, int ubLevel)
    {
        if (RottingCorpses.GetCorpseAtGridNo(sGridNo, ubLevel) != null)
        {
            return (true);
        }
        else
        {
            return (false);
        }
    }


    bool SetOpenableStructureToClosed(int sGridNo, int ubLevel)
    {
        STRUCTURE? pStructure;
        STRUCTURE? pNewStructure;

        pStructure = FindStructure(sGridNo, STRUCTURE_OPENABLE);
        if (!pStructure)
        {
            return (false);
        }

        if (pStructure.fFlags & STRUCTURE_OPEN)
        {
            pNewStructure = SwapStructureForPartner(sGridNo, pStructure);
            if (pNewStructure != null)
            {
                RecompileLocalMovementCosts(sGridNo);
                SetRenderFlags(RENDER_FLAG_FULL);
            }
        }
        // else leave it as is!
        return (true);
    }
}


public partial class Globals
{

}
