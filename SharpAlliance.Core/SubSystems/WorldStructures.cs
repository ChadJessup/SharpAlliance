using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

/// <summary>
/// There are a few 'structure' types in this codebase.
/// This one is specifically for structures that are places on a world.
/// 
/// Once porting is done, things will be renamed more appropriately.
/// </summary>
public class WorldStructures
{
    private readonly ILogger<WorldStructures> logger;
    private readonly World world;

    public WorldStructures(
        ILogger<WorldStructures> logger,
        World world)
    {
        this.logger = logger;
        this.world = world;
    }

    public static STRUCTURE? FindStructure(int sGridNo, STRUCTUREFLAGS fFlags)
    { // finds a structure that matches any of the given flags
        STRUCTURE? pCurrent;

        pCurrent = Globals.gpWorldLevelData[sGridNo].pStructureHead;
        while (pCurrent != null)
        {
            if ((pCurrent.fFlags & fFlags) != 0)
            {
                return (pCurrent);
            }

            pCurrent = pCurrent.pNext;
        }

        return (null);
    }

    public static LEVELNODE? FindLevelNodeBasedOnStructure(int sGridNo, STRUCTURE? pStructure)
    {
        LEVELNODE? pLevelNode;

        //ATE: First look on the struct layer.....
        pLevelNode = Globals.gpWorldLevelData[sGridNo].pStructHead;
        while (pLevelNode != null)
        {
            if (pLevelNode.pStructureData == pStructure)
            {
                return (pLevelNode);
            }
            pLevelNode = pLevelNode.pNext;
        }

        // Next the roof layer....
        pLevelNode = Globals.gpWorldLevelData[sGridNo].pRoofHead;
        while (pLevelNode != null)
        {
            if (pLevelNode.pStructureData == pStructure)
            {
                return (pLevelNode);
            }
            pLevelNode = pLevelNode.pNext;
        }

        // Then the object layer....
        pLevelNode = Globals.gpWorldLevelData[sGridNo].pObjectHead;
        while (pLevelNode != null)
        {
            if (pLevelNode.pStructureData == pStructure)
            {
                return (pLevelNode);
            }
            pLevelNode = pLevelNode.pNext;
        }

        // Finally the onroof layer....
        pLevelNode = Globals.gpWorldLevelData[sGridNo].pOnRoofHead;
        while (pLevelNode != null)
        {
            if (pLevelNode.pStructureData == pStructure)
            {
                return (pLevelNode);
            }
            pLevelNode = pLevelNode.pNext;
        }

        // Assert here if it cannot be found....
        //AssertMsg(0, "FindLevelNodeBasedOnStruct failed.");


        return (null);
    }

    public static bool AddStructureToWorld(int sBaseGridNo, int bLevel, DB_STRUCTURE_REF pDBStructureRef, LEVELNODE? pLevelN)
    {
        STRUCTURE? pStructure;

        pStructure = InternalAddStructureToWorld(sBaseGridNo, bLevel, pDBStructureRef, pLevelN);

        if (pStructure is null)
        {
            return false;
        }

        return true;
    }

    private static STRUCTURE? InternalAddStructureToWorld(int sBaseGridNo, int bLevel, DB_STRUCTURE_REF pDBStructureRef, LEVELNODE? pLevelNode)
    {
        // Adds a complete structure to the world at a location plus all other locations covered by the structure
        int sGridNo;
        List<STRUCTURE> ppStructure = new();
        STRUCTURE? pBaseStructure;
        DB_STRUCTURE pDBStructure;
        List<DB_STRUCTURE_TILE> ppTile;
        int ubLoop;
        int ubLoop2;
        int sBaseTileHeight = -1;
        int usStructureID = 0;

        if (pLevelNode is null)
        {
            return null;
        }

        if (pDBStructureRef is null)
        {
            return null;
        }

        pDBStructure = pDBStructureRef.pDBStructure;

        ppTile = pDBStructureRef.ppTile;
        if (ppTile is null)
        {
            return null;
        }

        if (pDBStructure.ubNumberOfTiles > 0)
        {
            return null;
        }

        // first check to see if the structure will be blocked
        if (!OkayToAddStructureToWorld(sBaseGridNo, bLevel, pDBStructureRef, Globals.INVALID_STRUCTURE_ID))
        {
            return (null);
        }

        for (ubLoop = (int)STRUCTUREFLAGS.BASE_TILE; ubLoop < pDBStructure.ubNumberOfTiles; ubLoop++)
        { // for each tile, create the appropriate STRUCTURE struct
            ppStructure[ubLoop] = CreateStructureFromDB(pDBStructureRef, ubLoop);
            if (ppStructure[ubLoop] == null)
            {
                // Free allocated memory and abort!
                for (ubLoop2 = 0; ubLoop2 < ubLoop; ubLoop2++)
                {
                    // MemFree(ppStructure[ubLoop2]);
                }
                // MemFree(ppStructure);
                return (null);
            }
            ppStructure[ubLoop].sGridNo = sBaseGridNo + ppTile[ubLoop].sPosRelToBase;
            if (ubLoop != (int)STRUCTUREFLAGS.BASE_TILE)
            {
                //# if JA2EDITOR
                //                //Kris: 
                //                //Added this undo code if in the editor.
                //                //It is important to save tiles effected by multitiles.  If the structure placement
                //                //fails below, it doesn't matter, because it won't hurt the undo code.
                //                if (gfEditMode)
                //                    AddToUndoList(ppStructure[ubLoop].sGridNo);
                //#endif

                ppStructure[ubLoop].sBaseGridNo = sBaseGridNo;
            }

            if (ppTile[ubLoop].fFlags.HasFlag(TILE.ON_ROOF))
            {
                //                ppStructure[ubLoop].sCubeOffset = (bLevel + 1) * PROFILE.Z_SIZE;
            }
            else
            {
                //                ppStructure[ubLoop].sCubeOffset = bLevel * PROFILE.Z_SIZE;
            }
            if (ppTile[ubLoop].fFlags.HasFlag(TILE.PASSABLE))
            {
                ppStructure[ubLoop].fFlags |= STRUCTUREFLAGS.PASSABLE;
            }
            if (pLevelNode.uiFlags.HasFlag(LEVELNODEFLAGS.SOLDIER))
            {
                // should now be unncessary
                ppStructure[ubLoop].fFlags |= STRUCTUREFLAGS.PERSON;
                ppStructure[ubLoop].fFlags &= ~(STRUCTUREFLAGS.BLOCKSMOVES);
            }
            else if (pLevelNode.uiFlags.HasFlag(LEVELNODEFLAGS.ROTTINGCORPSE) || ((STRUCTUREFLAGS)pDBStructure.fFlags).HasFlag(STRUCTUREFLAGS.CORPSE))
            {
                ppStructure[ubLoop].fFlags |= STRUCTUREFLAGS.CORPSE;
                // attempted check to screen this out for queen creature or vehicle
                if (pDBStructure.ubNumberOfTiles < 10)
                {
                    ppStructure[ubLoop].fFlags |= STRUCTUREFLAGS.PASSABLE;
                    ppStructure[ubLoop].fFlags &= ~(STRUCTUREFLAGS.BLOCKSMOVES);
                }
                else
                {
                    // make sure not transparent
                    ppStructure[ubLoop].fFlags &= ~(STRUCTUREFLAGS.TRANSPARENT);
                }
            }
        }

        if (pLevelNode.uiFlags.HasFlag(LEVELNODEFLAGS.SOLDIER))
        {
            // use the merc's ID as the structure ID for his/her structure
            usStructureID = pLevelNode.pSoldier.ubID;
        }
        else if (pLevelNode.uiFlags.HasFlag(LEVELNODEFLAGS.ROTTINGCORPSE))
        {
            // ATE: Offset IDs so they don't collide with soldiers
            //            usStructureID = (Globals.TOTAL_SOLDIERS + pLevelNode.pAniTile.uiUserData);
        }
        else
        {
            Globals.gusNextAvailableStructureID++;
            if (Globals.gusNextAvailableStructureID == 0)
            {
                // skip past the #s for soldiers' structures and the invalid structure #
                Globals.gusNextAvailableStructureID = Globals.FIRST_AVAILABLE_STRUCTURE_ID;
            }
            usStructureID = Globals.gusNextAvailableStructureID;
        }
        // now add all these to the world!
        for (ubLoop = (int)STRUCTUREFLAGS.BASE_TILE; ubLoop < pDBStructure.ubNumberOfTiles; ubLoop++)
        {
            sGridNo = ppStructure[ubLoop].sGridNo;
            if (ubLoop == (int)STRUCTUREFLAGS.BASE_TILE)
            {
                sBaseTileHeight = Globals.gpWorldLevelData[sGridNo].sHeight;
            }
            else
            {
                if (Globals.gpWorldLevelData[sGridNo].sHeight != sBaseTileHeight)
                {
                    // not level ground! abort!
                    for (ubLoop2 = (int)STRUCTUREFLAGS.BASE_TILE; ubLoop2 < ubLoop; ubLoop2++)
                    {
                        DeleteStructureFromTile((Globals.gpWorldLevelData[ppStructure[ubLoop2].sGridNo]), ppStructure[ubLoop2]);
                    }
                    //MemFree(ppStructure);
                    return (null);
                }
            }

            if (AddStructureToTile((Globals.gpWorldLevelData[sGridNo]), ppStructure[ubLoop], usStructureID) == false)
            {
                // error! abort!
                for (ubLoop2 = (int)STRUCTUREFLAGS.BASE_TILE; ubLoop2 < ubLoop; ubLoop2++)
                {
                    DeleteStructureFromTile((Globals.gpWorldLevelData[ppStructure[ubLoop2].sGridNo]), ppStructure[ubLoop2]);
                }

                //MemFree(ppStructure);
                return (null);
            }
        }

        pBaseStructure = ppStructure[(int)STRUCTUREFLAGS.BASE_TILE];
        pLevelNode.pStructureData = pBaseStructure;

        //MemFree(ppStructure);
        // And we're done! return a pointer to the base structure!

        return (pBaseStructure);
    }

    public static bool AddStructureToTile(MAP_ELEMENT? pMapElement, STRUCTURE? pStructure, int usStructureID)
    {
        // adds a STRUCTURE to a MAP_ELEMENT (adds part of a structure to a location on the map)
        STRUCTURE? pStructureTail;

        if (pMapElement is null || pStructure is null)
        {
            return false;
        }

        pStructureTail = pMapElement.pStructureTail;
        if (pStructureTail == null)
        { // set the head and tail to the new structure
            pMapElement.pStructureHead = pStructure;
        }
        else
        { // add to the end of the list
            pStructure.pPrev = pStructureTail;
            pStructureTail.pNext = pStructure;
        }

        pMapElement.pStructureTail = pStructure;
        pStructure.usStructureID = usStructureID;

        if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.OPENABLE))
        {
            pMapElement.uiFlags |= MAPELEMENTFLAGS.INTERACTIVETILE;
        }

        return (true);
    }

    public static STRUCTURE? CreateStructureFromDB(DB_STRUCTURE_REF pDBStructureRef, int ubTileNum)
    {
        // Creates a STRUCTURE struct for one tile of a structure
        STRUCTURE? pStructure;
        DB_STRUCTURE pDBStructure;
        DB_STRUCTURE_TILE? pTile;

        // set pointers to the DBStructure and Tile
        if (pDBStructureRef is null)
        {
            return null;
        }

        pDBStructure = pDBStructureRef.pDBStructure;

        if (pDBStructureRef.ppTile is null)
        {
            return null;
        }

        pTile = pDBStructureRef.ppTile[ubTileNum];

        if (pTile is null)
        {
            return null;
        }

        pStructure = new()
        {
            fFlags = pDBStructure.fFlags,
            pShape = pTile.Shape,
            pDBStructureRef = pDBStructureRef
        };

        if (pTile.sPosRelToBase == 0)
        {   // base tile
            pStructure.fFlags |= STRUCTUREFLAGS.BASE_TILE;
            pStructure.ubHitPoints = pDBStructure.ubHitPoints;
        }
        if (pDBStructure.ubWallOrientation != WallOrientation.NO_ORIENTATION)
        {
            if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.WALL))
            {
                // for multi-tile walls, which are only the special corner pieces,
                // the non-base tile gets no orientation value because this copy
                // will be skipped	
                if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.BASE_TILE))
                {
                    pStructure.ubWallOrientation = pDBStructure.ubWallOrientation;
                }
            }
            else
            {
                pStructure.ubWallOrientation = pDBStructure.ubWallOrientation;
            }
        }

        pStructure.ubVehicleHitLocation = pTile.ubVehicleHitLocation;
        return (pStructure);
    }

    public static bool OkayToAddStructureToWorld(
        int sBaseGridNo,
        int bLevel,
        DB_STRUCTURE_REF pDBStructureRef,
        int sExclusionID)
    {
        return (InternalOkayToAddStructureToWorld(
            sBaseGridNo,
            bLevel,
            pDBStructureRef,
            sExclusionID,
            sExclusionID == Globals.IGNORE_PEOPLE_STRUCTURE_ID));
    }

    public static bool OkayToAddStructureToTile(
        int sBaseGridNo,
        STRUCTURE_ON sCubeOffset,
        DB_STRUCTURE_REF pDBStructureRef,
        int ubTileIndex,
        int sExclusionID,
        bool fIgnorePeople)
    {
        ArgumentNullException.ThrowIfNull(pDBStructureRef.ppTile);

        // Verifies whether a structure is blocked from being added to the map at a particular point
        DB_STRUCTURE pDBStructure;
        List<DB_STRUCTURE_TILE> ppTile = new();
        STRUCTURE? pExistingStructure;
        STRUCTURE? pOtherExistingStructure;
        int bLoop, bLoop2;
        int sGridNo;
        int sOtherGridNo;

        ppTile = pDBStructureRef.ppTile;
        sGridNo = sBaseGridNo + ppTile[ubTileIndex].sPosRelToBase;
        if (sGridNo < 0 || sGridNo > WORLD_MAX)
        {
            return (false);
        }

        if (Globals.gpWorldLevelData[sBaseGridNo].sHeight != Globals.gpWorldLevelData[sGridNo].sHeight)
        {
            // uneven terrain, one portion on top of cliff and another not! can't add!
            return (false);
        }

        pDBStructure = pDBStructureRef.pDBStructure;
        pExistingStructure = Globals.gpWorldLevelData[sGridNo].pStructureHead;

        /*
            // If adding a mobile structure, always allow addition if the mobile structure tile is passable
            if ( (pDBStructure.fFlags & STRUCTUREFLAGS.MOBILE) && (ppTile[ubTileIndex].fFlags & TILE_PASSABLE) )
            {
                return( true );
            }
        */

        while (pExistingStructure != null)
        {
            if (sCubeOffset == pExistingStructure.sCubeOffset)
            {
                // CJC:
                // If adding a mobile structure, allow addition if existing structure is passable
                if ((((STRUCTUREFLAGS)pDBStructure.fFlags).HasFlag(STRUCTUREFLAGS.MOBILE)) && (pExistingStructure.fFlags.HasFlag(STRUCTUREFLAGS.PASSABLE)))
                {
                    // Skip!
                    pExistingStructure = pExistingStructure.pNext;
                    continue;
                }

                if (((STRUCTUREFLAGS)pDBStructure.fFlags).HasFlag(STRUCTUREFLAGS.OBSTACLE))
                {

                    // CJC: NB these next two if states are probably COMPLETELY OBSOLETE but I'm leaving
                    // them in there for now (no harm done)

                    // ATE:
                    // ignore this one if it has the same ID num as exclusion
                    if (sExclusionID != Globals.INVALID_STRUCTURE_ID)
                    {
                        if (pExistingStructure.usStructureID == sExclusionID)
                        {
                            // Skip!
                            pExistingStructure = pExistingStructure.pNext;
                            continue;
                        }
                    }

                    if (fIgnorePeople)
                    {
                        // If we are a person, skip!
                        if (pExistingStructure.usStructureID < Globals.TOTAL_SOLDIERS)
                        {
                            // Skip!
                            pExistingStructure = pExistingStructure.pNext;
                            continue;
                        }
                    }

                    // two obstacle structures aren't allowed in the same tile at the same height
                    // ATE: There is more sophisticated logic for mobiles, so postpone this check if mobile....
                    if ((pExistingStructure.fFlags.HasFlag(STRUCTUREFLAGS.OBSTACLE)) && !(((STRUCTUREFLAGS)pDBStructure.fFlags).HasFlag(STRUCTUREFLAGS.MOBILE)))
                    {
                        if (pExistingStructure.fFlags.HasFlag(STRUCTUREFLAGS.PASSABLE) && !(pExistingStructure.fFlags.HasFlag(STRUCTUREFLAGS.MOBILE)))
                        {
                            // no mobiles, existing structure is passable
                        }
                        else
                        {
                            return (false);
                        }
                    }
                    else if ((pDBStructure.ubNumberOfTiles > 1) && (pExistingStructure.fFlags.HasFlag(STRUCTUREFLAGS.WALLSTUFF)))
                    {
                        // if not an open door...
                        if (!((pExistingStructure.fFlags.HasFlag(STRUCTUREFLAGS.ANYDOOR)) && (pExistingStructure.fFlags.HasFlag(STRUCTUREFLAGS.OPEN))))
                        {

                            // we could be trying to place a multi-tile obstacle on top of a wall; we shouldn't
                            // allow this if the structure is going to be on both sides of the wall
                            for (bLoop = 1; bLoop < 4; bLoop++)
                            {
                                switch (pExistingStructure.ubWallOrientation)
                                {
                                    case WallOrientation.OUTSIDE_TOP_LEFT:
                                    case WallOrientation.INSIDE_TOP_LEFT:
                                        sOtherGridNo = IsometricUtils.NewGridNo(sGridNo, IsometricUtils.DirectionInc((bLoop + 2)));
                                        break;
                                    case WallOrientation.OUTSIDE_TOP_RIGHT:
                                    case WallOrientation.INSIDE_TOP_RIGHT:
                                        sOtherGridNo = IsometricUtils.NewGridNo(sGridNo, IsometricUtils.DirectionInc(bLoop));
                                        break;
                                    default:
                                        // @%?@#%?@%
                                        sOtherGridNo = IsometricUtils.NewGridNo(sGridNo, IsometricUtils.DirectionInc((int)WorldDirections.SOUTHEAST));
                                        break;
                                }
                                for (bLoop2 = 0; bLoop2 < pDBStructure.ubNumberOfTiles; bLoop2++)
                                {
                                    if (sBaseGridNo + ppTile[bLoop2].sPosRelToBase == sOtherGridNo)
                                    {
                                        // obstacle will straddle wall!
                                        return (false);
                                    }
                                }
                            }
                        }

                    }
                }
                else if (((STRUCTUREFLAGS)pDBStructure.fFlags).HasFlag(STRUCTUREFLAGS.WALLSTUFF))
                {
                    // two walls with the same alignment aren't allowed in the same tile
                    if ((pExistingStructure.fFlags.HasFlag(STRUCTUREFLAGS.WALLSTUFF)) && (pDBStructure.ubWallOrientation == pExistingStructure.ubWallOrientation))
                    {
                        return (false);
                    }
                    else if (!(pExistingStructure.fFlags.HasFlag(STRUCTUREFLAGS.CORPSE | STRUCTUREFLAGS.PERSON)))
                    {
                        // it's possible we're trying to insert this wall on top of a multitile obstacle
                        for (bLoop = 1; bLoop < 4; bLoop++)
                        {
                            switch (pDBStructure.ubWallOrientation)
                            {
                                case WallOrientation.OUTSIDE_TOP_LEFT:
                                case WallOrientation.INSIDE_TOP_LEFT:
                                    sOtherGridNo = IsometricUtils.NewGridNo(sGridNo, IsometricUtils.DirectionInc((bLoop + 2)));
                                    break;
                                case WallOrientation.OUTSIDE_TOP_RIGHT:
                                case WallOrientation.INSIDE_TOP_RIGHT:
                                    sOtherGridNo = IsometricUtils.NewGridNo(sGridNo, IsometricUtils.DirectionInc(bLoop));
                                    break;
                                default:
                                    // @%?@#%?@%
                                    sOtherGridNo = IsometricUtils.NewGridNo(sGridNo, IsometricUtils.DirectionInc((int)WorldDirections.SOUTHEAST));
                                    break;
                            }
                            for (ubTileIndex = 0; ubTileIndex < pDBStructure.ubNumberOfTiles; ubTileIndex++)
                            {
                                pOtherExistingStructure = FindStructureByID(sOtherGridNo, pExistingStructure.usStructureID);
                                if (pOtherExistingStructure is not null)
                                {
                                    return (false);
                                }
                            }
                        }
                    }
                }

                if (((STRUCTUREFLAGS)pDBStructure.fFlags).HasFlag(STRUCTUREFLAGS.MOBILE))
                {
                    // ATE:
                    // ignore this one if it has the same ID num as exclusion
                    if (sExclusionID != Globals.INVALID_STRUCTURE_ID)
                    {
                        if (pExistingStructure.usStructureID == sExclusionID)
                        {
                            // Skip!
                            pExistingStructure = pExistingStructure.pNext;
                            continue;
                        }
                    }

                    if (fIgnorePeople)
                    {
                        // If we are a person, skip!
                        if (pExistingStructure.usStructureID < Globals.TOTAL_SOLDIERS)
                        {
                            // Skip!
                            pExistingStructure = pExistingStructure.pNext;
                            continue;
                        }
                    }

                    // ATE: Added check here - UNLESS the part we are trying to add is PASSABLE!
                    if (pExistingStructure.fFlags.HasFlag(STRUCTUREFLAGS.MOBILE) && !(pExistingStructure.fFlags.HasFlag(STRUCTUREFLAGS.PASSABLE)) && !(ppTile[ubTileIndex].fFlags.HasFlag(TILE.PASSABLE)))
                    {
                        // don't allow 2 people in the same tile
                        return (false);
                    }

                    // ATE: Another rule: allow PASSABLE *IF* the PASSABLE is *NOT* MOBILE!
                    if (!(pExistingStructure.fFlags.HasFlag(STRUCTUREFLAGS.MOBILE)) && (pExistingStructure.fFlags.HasFlag(STRUCTUREFLAGS.PASSABLE)))
                    {
                        // Skip!
                        pExistingStructure = pExistingStructure.pNext;
                        continue;
                    }

                    // ATE: Added here - UNLESS this part is PASSABLE....
                    // two obstacle structures aren't allowed in the same tile at the same height
                    if ((pExistingStructure.fFlags.HasFlag(STRUCTUREFLAGS.OBSTACLE)) && !(ppTile[ubTileIndex].fFlags.HasFlag(TILE.PASSABLE)))
                    {
                        return (false);
                    }
                }

                if ((((STRUCTUREFLAGS)pDBStructure.fFlags).HasFlag(STRUCTUREFLAGS.OPENABLE)))
                {
                    if (pExistingStructure.fFlags.HasFlag(STRUCTUREFLAGS.OPENABLE))
                    {
                        // don't allow two openable structures in the same tile or things will screw
                        // up on an interface level
                        return (false);
                    }
                }
            }

            pExistingStructure = pExistingStructure.pNext;
        }


        return (true);
    }

    private static bool InternalOkayToAddStructureToWorld(int sBaseGridNo, int bLevel, DB_STRUCTURE_REF pDBStructureRef, int sExclusionID, bool fIgnorePeople)
    {
        int ubLoop;
        STRUCTURE_ON sCubeOffset;

        if (pDBStructureRef is null
            || pDBStructureRef.pDBStructure.ubNumberOfTiles > 0
            || pDBStructureRef.ppTile is null)
        {
            return false;
        }

        /*
            if (gpWorldLevelData[sGridNo].sHeight != sBaseTileHeight)
            {
                // not level ground!
                return( false );
            }
        */

        for (ubLoop = 0; ubLoop < pDBStructureRef.pDBStructure.ubNumberOfTiles; ubLoop++)
        {
            if (pDBStructureRef.ppTile[ubLoop].fFlags.HasFlag(TILE.ON_ROOF))
            {
                if (bLevel == 0)
                {
                    sCubeOffset = (STRUCTURE_ON)PROFILE.Z_SIZE;
                }
                else
                {
                    return (false);
                }
            }
            else
            {
                sCubeOffset = (STRUCTURE_ON)(bLevel * PROFILE.Z_SIZE);
            }
            if (!OkayToAddStructureToTile(sBaseGridNo, sCubeOffset, pDBStructureRef, ubLoop, sExclusionID, fIgnorePeople))
            {
                return (false);
            }
        }
        return (true);
    }

    public static bool DeleteStructureFromWorld(STRUCTURE? pStructure)
    {
        if (pStructure is null)
        {
            return false;
        }

        // removes all of the STRUCTURE elements for a structure from the world
        MAP_ELEMENT? pBaseMapElement;
        STRUCTURE? pBaseStructure;
        List<DB_STRUCTURE_TILE> ppTile;
        STRUCTURE? pCurrent;
        int ubLoop, ubLoop2;
        int ubNumberOfTiles;
        int sBaseGridNo, sGridNo;
        int usStructureID;
        bool fMultiStructure;
        bool fRecompileMPs;
        bool fRecompileExtraRadius; // for doors... yuck
        int sCheckGridNo;

        pBaseStructure = FindBaseStructure(pStructure);

        if (pBaseStructure is null)
        {
            return false;
        }

        usStructureID = pBaseStructure.usStructureID;
        fMultiStructure = ((pBaseStructure.fFlags & STRUCTUREFLAGS.MULTI) != 0);
        fRecompileMPs = ((Globals.gsRecompileAreaLeft != 0) && !(pBaseStructure.fFlags.HasFlag(STRUCTUREFLAGS.MOBILE) != false));

        if (fRecompileMPs)
        {
            fRecompileExtraRadius = pBaseStructure.fFlags.HasFlag(STRUCTUREFLAGS.WALLSTUFF) != false;
        }
        else
        {
            fRecompileExtraRadius = false;
        }

        pBaseMapElement = Globals.gpWorldLevelData[pBaseStructure.sGridNo];
        ppTile = pBaseStructure.pDBStructureRef.ppTile;
        sBaseGridNo = pBaseStructure.sGridNo;
        ubNumberOfTiles = pBaseStructure.pDBStructureRef.pDBStructure.ubNumberOfTiles;
        // Free all the tiles
        for (ubLoop = (int)STRUCTUREFLAGS.BASE_TILE; ubLoop < ubNumberOfTiles; ubLoop++)
        {
            sGridNo = sBaseGridNo + ppTile[ubLoop].sPosRelToBase;
            // there might be two structures in this tile, one on each level, but we just want to
            // delete one on each pass
            pCurrent = FindStructureByID(sGridNo, usStructureID);
            if (pCurrent is not null)
            {
                DeleteStructureFromTile(Globals.gpWorldLevelData[sGridNo], pCurrent);
            }

            if (!Globals.gfEditMode && (fRecompileMPs))
            {
                if (fRecompileMPs)
                {
                    World.AddTileToRecompileArea(sGridNo);
                    if (fRecompileExtraRadius)
                    {
                        // add adjacent tiles too
                        for (ubLoop2 = 0; ubLoop2 < (int)WorldDirections.NUM_WORLD_DIRECTIONS; ubLoop2++)
                        {
                            sCheckGridNo = IsometricUtils.NewGridNo(sGridNo, IsometricUtils.DirectionInc(ubLoop2));
                            if (sCheckGridNo != sGridNo)
                            {
                                World.AddTileToRecompileArea(sCheckGridNo);
                            }
                        }
                    }
                }
            }
        }

        return (true);
    }

    public static void DeleteStructureFromTile(MAP_ELEMENT pMapElement, STRUCTURE? pStructure)
    {
        // removes a STRUCTURE element at a particular location from the world
        // put location pointer in tile
        if (pMapElement.pStructureHead == pStructure)
        {
            if (pMapElement.pStructureTail == pStructure)
            {
                // only element in the list!
                pMapElement.pStructureHead = null;
                pMapElement.pStructureTail = null;
            }
            else
            {
                // first element in the list of 2+ members
                pMapElement.pStructureHead = pStructure.pNext;
            }
        }
        else if (pMapElement.pStructureTail == pStructure)
        {
            // last element in the list
            pStructure.pPrev.pNext = null;
            pMapElement.pStructureTail = pStructure.pPrev;
        }
        else
        {
            // second or later element in the list; it's guaranteed that there is a 
            // previous element but not necessary a next
            pStructure.pPrev.pNext = pStructure.pNext;
            if (pStructure.pNext != null)
            {
                pStructure.pNext.pPrev = pStructure.pPrev;
            }
        }
        if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.OPENABLE))
        { // only one allowed in a tile, so we are safe to do this...
            pMapElement.uiFlags &= (~MAPELEMENTFLAGS.INTERACTIVETILE);
        }

        pStructure = null;
    }

    public static STRUCTURE? FindBaseStructure(STRUCTURE? pStructure)
    {
        // finds the base structure for any structure
        if (pStructure is null)
        {
            return null;
        }

        if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.BASE_TILE))
        {
            return (pStructure);
        }

        return (FindStructureByID(pStructure.sBaseGridNo, pStructure.usStructureID));
    }

    public static STRUCTURE? FindStructureByID(int sGridNo, int usStructureID)
    {
        // finds a structure that matches any of the given flags
        STRUCTURE? pCurrent;

        pCurrent = Globals.gpWorldLevelData[sGridNo].pStructureHead;
        while (pCurrent is not null)
        {
            if (pCurrent.usStructureID == usStructureID)
            {
                return (pCurrent);
            }

            pCurrent = pCurrent.pNext;
        }

        return null;
    }
}

public enum TILE : byte
{
    ON_ROOF = 0x01,
    PASSABLE = 0x02,
}
