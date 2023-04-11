using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.Image;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class StructureInternals
{

    // Function operating on a structure tile
    private static int FilledTilePositions(DB_STRUCTURE_TILE? pTile)
    {
        int ubFilled = 0, ubShapeValue;
        int bLoopX, bLoopY, bLoopZ;

        // Loop through all parts of a structure and add up the number of
        // filled spots
        for (bLoopX = 0; bLoopX < PROFILE_X_SIZE; bLoopX++)
        {
            for (bLoopY = 0; bLoopY < PROFILE_Y_SIZE; bLoopY++)
            {
                ubShapeValue = pTile.Shape[bLoopX, bLoopY];
                for (bLoopZ = 0; bLoopZ < PROFILE_Z_SIZE; bLoopZ++)
                {
                    if ((ubShapeValue & AtHeight[bLoopZ]) == 0)
                    {
                        ubFilled++;
                    }
                }
            }
        }
        return (ubFilled);
    }

    //
    // Structure database functions
    //

    private static void FreeStructureFileRef(STRUCTURE_FILE_REF pFileRef)
    { // Frees all of the memory associated with a file reference, including
      // the file reference structure itself

        int usLoop;

        if (pFileRef.pDBStructureRef != null)
        {
            for (usLoop = 0; usLoop < pFileRef.usNumberOfStructures; usLoop++)
            {
                if (pFileRef.pDBStructureRef[usLoop].ppTile.Any())
                {
                    pFileRef.pDBStructureRef[usLoop].ppTile.Clear();
                }
            }

            pFileRef.pDBStructureRef = null;
        }
        // if (pFileRef.pubStructureData != null)
        // {
        //     pFileRef.pubStructureData = null;
        // }

        // if (pFileRef.pAuxData != null)
        // {
        //     MemFree(pFileRef.pAuxData);
        // }

        MemFree(pFileRef);
    }

    void FreeAllStructureFiles()
    {
        // Frees all of the structure database!
        STRUCTURE_FILE_REF pFileRef;
        STRUCTURE_FILE_REF pNextRef;

        pFileRef = gpStructureFileRefs;
        while (IntPtr.Zero != pFileRef.pNext)
        {
            pNextRef = Marshal.PtrToStructure<STRUCTURE_FILE_REF>(pFileRef.pNext);
            FreeStructureFileRef(pFileRef);
            pFileRef = pNextRef;
        }
    }

    bool FreeStructureFile(STRUCTURE_FILE_REF pStructureFile)
    {
        // unlink the file ref
        // if (pStructureFile.pPrev != null)
        // {
        //     pStructureFile.pPrev.pNext = pStructureFile.pNext;
        // }
        // else
        // {
        //     // freeing the head of the list!
        //     gpStructureFileRefs = pStructureFile.pNext;
        // }
        // if (pStructureFile.pNext != null)
        // {
        //     pStructureFile.pNext.pPrev = pStructureFile.pPrev;
        // }
        // if (pStructureFile.pPrev == null && pStructureFile.pNext == null)
        // {
        //     // toasting the list!
        //     gpStructureFileRefs = null;
        // }
        // and free all the structures used!
        FreeStructureFileRef(pStructureFile);
        return (true);
    }

    private unsafe static bool LoadStructureData(string szFileName, STRUCTURE_FILE_REF pFileRef, out int puiStructureDataSize)
    {
        // Loads a structure file's data as a honking chunk o' memory 
        //int **ppubStructureData, int * puiDataSize, STRUCTURE_FILE_HEADER * pHeader )
        puiStructureDataSize = 0;
        Stream hInput;
        STRUCTURE_FILE_HEADER Header = new();
        int uiBytesRead;
        int uiDataSize;
        bool fOk;

        CHECKF(szFileName);
        CHECKF(pFileRef);
        hInput = FileManager.FileOpen(szFileName, FileAccess.Read /*FILE_OPEN_EXISTING*/, false);
        if (hInput.Length == -1)
        {
            return (false);
        }

        uint STRUCTURE_FILE_HEADER_SIZE = 16;
        fOk = FileManager.FileRead<STRUCTURE_FILE_HEADER>(hInput, ref Header, sizeof(STRUCTURE_FILE_HEADER), out uiBytesRead);
        var szId = new string(Header.szId);
        if (!fOk || uiBytesRead != STRUCTURE_FILE_HEADER_SIZE
            || !szId.Equals(STRUCTURE_FILE_ID)
            || Header.usNumberOfStructures == 0)
        {
            FileManager.FileClose(hInput);
            return (false);
        }
        pFileRef.usNumberOfStructures = Header.usNumberOfStructures;
        if (Header.fFlags.HasFlag(STRUCTURE_FILE_CONTAINS.AUXIMAGEDATA))
        {
            uiDataSize = sizeof(AuxObjectData) * Header.usNumberOfImages;

            fOk = FileManager.FileRead<AuxObjectData>(hInput, ref pFileRef.pAuxData, uiDataSize, out uiBytesRead);
            if (!fOk || uiBytesRead != uiDataSize)
            {
                MemFree(pFileRef.pAuxData);
                FileManager.FileClose(hInput);
                return (false);
            }
            if (Header.usNumberOfImageTileLocsStored > 0)
            {
                uiDataSize = sizeof(RelTileLoc) * Header.usNumberOfImageTileLocsStored;

                fOk = FileManager.FileRead(hInput, ref pFileRef.pTileLocData, uiDataSize, out uiBytesRead);
                if (!fOk || uiBytesRead != uiDataSize)
                {
                    MemFree(pFileRef.pAuxData);
                    FileManager.FileClose(hInput);
                    return (false);
                }
            }
        }
        if (Header.fFlags.HasFlag(STRUCTURE_FILE_CONTAINS.STRUCTUREDATA))
        {
            pFileRef.usNumberOfStructuresStored = Header.usNumberOfStructuresStored;
            uiDataSize = Header.usStructureDataSize;
            // Determine the size of the data, from the header just read,
            // allocate enough memory and read it in
            //pFileRef.pubStructureData = MemAlloc(uiDataSize);
            if (pFileRef.pubStructureData == null)
            {
                FileManager.FileClose(hInput);

                return (false);
            }

            fOk = FileManager.FileRead(hInput, ref pFileRef.pubStructureData, uiDataSize, out uiBytesRead);
            if (!fOk || uiBytesRead != uiDataSize)
            {
                MemFree(pFileRef.pubStructureData);
                // if (pFileRef.pAuxData != null)
                // {
                //     MemFree(pFileRef.pAuxData);
                //     if (pFileRef.pTileLocData != null)
                //     {
                //         MemFree(pFileRef.pTileLocData);
                //     }
                // }
                FileManager.FileClose(hInput);
                return (false);
            }

            puiStructureDataSize = uiDataSize;
        }

        FileManager.FileClose(hInput);
        return (true);
    }

    private static bool CreateFileStructureArrays(STRUCTURE_FILE_REF? pFileRef, int uiDataSize)
    { // Based on a file chunk, creates all the dynamic arrays for the 
      // structure definitions contained within

        int? pCurrent;
        List<DB_STRUCTURE_REF> pDBStructureRef = new();
        List<DB_STRUCTURE_TILE> ppTileArray = new();
        int usLoop;
        int usIndex;
        int usTileLoop;
        int uiHitPoints;

        pCurrent = pFileRef.pubStructureData;
        pFileRef.pDBStructureRef.Add(pDBStructureRef);
        for (usLoop = 0; usLoop < pFileRef.usNumberOfStructuresStored; usLoop++)
        {
            if (pCurrent + sizeof(DB_STRUCTURE) > pFileRef.pubStructureData + uiDataSize)
            {   // gone past end of file block?!
                // freeing of memory will occur outside of the function
                return (false);
            }
            usIndex = ((DB_STRUCTURE?)pCurrent).usStructureNumber;
            pDBStructureRef[usIndex].pDBStructure = (DB_STRUCTURE?)pCurrent;
            pDBStructureRef[usIndex].ppTile = ppTileArray;
            pCurrent += sizeof(DB_STRUCTURE);
            // Set things up to calculate hit points
            uiHitPoints = 0;
            for (usTileLoop = 0; usTileLoop < pDBStructureRef[usIndex].pDBStructure.ubNumberOfTiles; usTileLoop++)
            {
                if (pCurrent + sizeof(DB_STRUCTURE) > pFileRef.pubStructureData + uiDataSize)
                {   // gone past end of file block?!
                    // freeing of memory will occur outside of the function
                    return (false);
                }
                ppTileArray[usTileLoop] = (DB_STRUCTURE_TILE)pCurrent;
                // set the single-value relative position between this tile and the base tile
                ppTileArray[usTileLoop].sPosRelToBase = ppTileArray[usTileLoop].bXPosRelToBase + ppTileArray[usTileLoop].bYPosRelToBase * WORLD_COLS;
                uiHitPoints += FilledTilePositions(ppTileArray[usTileLoop]);
                pCurrent += sizeof(DB_STRUCTURE_TILE);
            }
            // scale hit points down to something reasonable...
            uiHitPoints = uiHitPoints * 100 / 255;
            /*
            if (uiHitPoints > 255)
            {
                uiHitPoints = 255;
            }
            */
            pDBStructureRef[usIndex].pDBStructure.ubHitPoints = uiHitPoints;
            /*
            if (pDBStructureRef[usIndex].pDBStructure.usStructureNumber + 1 == pFileRef.usNumberOfStructures)
            {
                break;
            }
            */
        }
        return (true);
    }

    public static STRUCTURE_FILE_REF? LoadStructureFile(string szFileName)
    { // NB should be passed in expected number of structures so we can check equality
        int uiDataSize = 0;
        bool fOk;
        STRUCTURE_FILE_REF pFileRef = new();

        fOk = LoadStructureData(szFileName, pFileRef, out uiDataSize);
        if (!fOk)
        {
            MemFree(pFileRef);
            return (null);
        }
        if (pFileRef.pubStructureData != null)
        {
            fOk = CreateFileStructureArrays(pFileRef, uiDataSize);
            if (fOk == false)
            {
                FreeStructureFileRef(pFileRef);
                return (null);
            }
        }
        // Add the file reference to the master list, at the head for convenience	
        if (gpStructureFileRefs != null)
        {
            gpStructureFileRefs.pPrev = pFileRef;
        }
        pFileRef.pNext = gpStructureFileRefs;
        gpStructureFileRefs = pFileRef;
        return (pFileRef);
    }


    //
    // Structure creation functions
    //


    private static STRUCTURE? CreateStructureFromDB(DB_STRUCTURE_REF? pDBStructureRef, int ubTileNum)
    { // Creates a STRUCTURE struct for one tile of a structure
        STRUCTURE? pStructure;
        DB_STRUCTURE? pDBStructure;
        DB_STRUCTURE_TILE? pTile;

        // set pointers to the DBStructure and Tile
        CHECKN(pDBStructureRef);
        CHECKN(pDBStructureRef.pDBStructure);
        pDBStructure = pDBStructureRef.pDBStructure;
        CHECKN(pDBStructureRef.ppTile);
        pTile = pDBStructureRef.ppTile[ubTileNum];
        CHECKN(pTile);

        // setup
        pStructure = new()
        {
            fFlags = pDBStructure.fFlags,
            pShape = (pTile.Shape),
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

    private static bool OkayToAddStructureToTile(int sBaseGridNo, STRUCTURE_ON sCubeOffset, DB_STRUCTURE_REF pDBStructureRef, int ubTileIndex, int sExclusionID, bool fIgnorePeople)
    {
        // Verifies whether a structure is blocked from being added to the map at a particular point
        DB_STRUCTURE? pDBStructure = null;
        List<DB_STRUCTURE_TILE> ppTile = new();
        STRUCTURE? pExistingStructure = null;
        STRUCTURE? pOtherExistingStructure = null;
        int bLoop = 0, bLoop2 = 0;
        int sGridNo = 0;
        int sOtherGridNo = 0;

        ppTile = pDBStructureRef.ppTile;
        sGridNo = sBaseGridNo + ppTile[ubTileIndex].sPosRelToBase;
        if (sGridNo < 0 || sGridNo > WORLD_MAX)
        {
            return (false);
        }

        if (gpWorldLevelData[sBaseGridNo].sHeight != gpWorldLevelData[sGridNo].sHeight)
        {
            // uneven terrain, one portion on top of cliff and another not! can't add!
            return (false);
        }

        pDBStructure = pDBStructureRef.pDBStructure;
        pExistingStructure = gpWorldLevelData[sGridNo].pStructureHead;

        /*
            // If adding a mobile structure, always allow addition if the mobile structure tile is passable
            if ( (pDBStructure.fFlags.HasFlag(STRUCTUREFLAGS.MOBILE)) && (ppTile[ubTileIndex].fFlags & TILE_PASSABLE) )
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
                if ((pDBStructure.fFlags.HasFlag(STRUCTUREFLAGS.MOBILE) && (pExistingStructure.fFlags.HasFlag(STRUCTUREFLAGS.PASSABLE))))
                {
                    // Skip!
                    pExistingStructure = pExistingStructure.pNext;
                    continue;
                }


                if (pDBStructure.fFlags.HasFlag(STRUCTUREFLAGS.OBSTACLE))
                {

                    // CJC: NB these next two if states are probably COMPLETELY OBSOLETE but I'm leaving
                    // them in there for now (no harm done)

                    // ATE:
                    // ignore this one if it has the same ID num as exclusion
                    if (sExclusionID != INVALID_STRUCTURE_ID)
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
                        if (pExistingStructure.usStructureID < TOTAL_SOLDIERS)
                        {
                            // Skip!
                            pExistingStructure = pExistingStructure.pNext;
                            continue;
                        }
                    }

                    // two obstacle structures aren't allowed in the same tile at the same height
                    // ATE: There is more sophisticated logic for mobiles, so postpone this check if mobile....
                    if ((pExistingStructure.fFlags.HasFlag(STRUCTUREFLAGS.OBSTACLE)) && !(pDBStructure.fFlags.HasFlag(STRUCTUREFLAGS.MOBILE)))
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
                                    case WallOrientation.NO_ORIENTATION:
                                        break;
                                    case WallOrientation.INSIDE_BOTTOM_CORNER:
                                        break;
                                    case WallOrientation.OUTSIDE_BOTTOM_CORNER:
                                        break;
                                    default:
                                        // @%?@#%?@%
                                        sOtherGridNo = IsometricUtils.NewGridNo(sGridNo, IsometricUtils.DirectionInc(WorldDirections.SOUTHEAST));
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
                else if (pDBStructure.fFlags.HasFlag(STRUCTUREFLAGS.WALLSTUFF))
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
                                    sOtherGridNo = IsometricUtils.NewGridNo(sGridNo, IsometricUtils.DirectionInc((int)(bLoop + 2)));
                                    break;
                                case WallOrientation.OUTSIDE_TOP_RIGHT:
                                case WallOrientation.INSIDE_TOP_RIGHT:
                                    sOtherGridNo = IsometricUtils.NewGridNo(sGridNo, IsometricUtils.DirectionInc(bLoop));
                                    break;
                                default:
                                    // @%?@#%?@%
                                    sOtherGridNo = IsometricUtils.NewGridNo(sGridNo, IsometricUtils.DirectionInc(WorldDirections.SOUTHEAST));
                                    break;
                            }
                            for (ubTileIndex = 0; ubTileIndex < pDBStructure.ubNumberOfTiles; ubTileIndex++)
                            {
                                pOtherExistingStructure = FindStructureByID(sOtherGridNo, pExistingStructure.usStructureID);
                                if (pOtherExistingStructure)
                                {
                                    return (false);
                                }
                            }
                        }
                    }
                }

                if (pDBStructure.fFlags.HasFlag(STRUCTUREFLAGS.MOBILE))
                {
                    // ATE:
                    // ignore this one if it has the same ID num as exclusion
                    if (sExclusionID != INVALID_STRUCTURE_ID)
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
                        if (pExistingStructure.usStructureID < TOTAL_SOLDIERS)
                        {
                            // Skip!
                            pExistingStructure = pExistingStructure.pNext;
                            continue;
                        }
                    }

                    // ATE: Added check here - UNLESS the part we are trying to add is PASSABLE!
                    if (pExistingStructure.fFlags.HasFlag(STRUCTUREFLAGS.MOBILE)
                        && !(pExistingStructure.fFlags.HasFlag(STRUCTUREFLAGS.PASSABLE))
                        && !(ppTile[ubTileIndex].fFlags.HasFlag(TILE.PASSABLE)))
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

                if ((pDBStructure.fFlags.HasFlag(STRUCTUREFLAGS.OPENABLE)))
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

    public static bool InternalOkayToAddStructureToWorld(int sBaseGridNo, int bLevel, DB_STRUCTURE_REF pDBStructureRef, int sExclusionID, bool fIgnorePeople)
    {
        int ubLoop;
        STRUCTURE_ON sCubeOffset;

        CHECKF(pDBStructureRef);
        CHECKF(pDBStructureRef.pDBStructure);
        CHECKF(pDBStructureRef.pDBStructure.ubNumberOfTiles > 0);
        CHECKF(pDBStructureRef.ppTile);

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
                    sCubeOffset = (STRUCTURE_ON)PROFILE_Z_SIZE;
                }
                else
                {
                    return (false);
                }
            }
            else
            {
                sCubeOffset = (STRUCTURE_ON)(bLevel * PROFILE_Z_SIZE);
            }
            if (!OkayToAddStructureToTile(sBaseGridNo, sCubeOffset, pDBStructureRef, ubLoop, sExclusionID, fIgnorePeople))
            {
                return (false);
            }
        }
        return (true);
    }

    public static bool OkayToAddStructureToWorld(int sBaseGridNo, int bLevel, DB_STRUCTURE_REF pDBStructureRef, int sExclusionID)
    {
        return (InternalOkayToAddStructureToWorld(sBaseGridNo, bLevel, pDBStructureRef, sExclusionID, (bool)(sExclusionID == IGNORE_PEOPLE_STRUCTURE_ID)));
    }

    private static bool AddStructureToTile(MAP_ELEMENT? pMapElement, STRUCTURE? pStructure, int usStructureID)
    { // adds a STRUCTURE to a MAP_ELEMENT (adds part of a structure to a location on the map)
        STRUCTURE? pStructureTail;

        CHECKF(pMapElement);
        CHECKF(pStructure);
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


    private static STRUCTURE? InternalAddStructureToWorld(int sBaseGridNo, int bLevel, DB_STRUCTURE_REF pDBStructureRef, LEVELNODE? pLevelNode)
    { // Adds a complete structure to the world at a location plus all other locations covered by the structure
        int sGridNo;
        List<STRUCTURE?> ppStructure = new();
        STRUCTURE? pBaseStructure;
        DB_STRUCTURE? pDBStructure;
        List<DB_STRUCTURE_TILE> ppTile;
        int ubLoop;
        int ubLoop2;
        int sBaseTileHeight = -1;
        int usStructureID;

        CHECKF(pDBStructureRef);
        CHECKF(pLevelNode);

        pDBStructure = pDBStructureRef.pDBStructure;
        CHECKF(pDBStructure);

        ppTile = pDBStructureRef.ppTile;
        CHECKF(ppTile);

        CHECKF(pDBStructure.ubNumberOfTiles > 0);

        // first check to see if the structure will be blocked
        if (!OkayToAddStructureToWorld(sBaseGridNo, bLevel, pDBStructureRef, INVALID_STRUCTURE_ID))
        {
            return (null);
        }

        // We go through a definition stage here and a later stage of
        // adding everything to the world so that we don't have to untangle
        // things if we run out of memory.  First we create an array of
        // pointers to point to all of the STRUCTURE elements created in
        // the first stage.  This array gets given to the base tile so
        // there is an easy way to remove an entire object from the world quickly

        for (ubLoop = BASE_TILE; ubLoop < pDBStructure.ubNumberOfTiles; ubLoop++)
        { // for each tile, create the appropriate STRUCTURE struct
            ppStructure[ubLoop] = CreateStructureFromDB(pDBStructureRef, ubLoop);
            if (ppStructure[ubLoop] == null)
            {
                // Free allocated memory and abort!
                for (ubLoop2 = 0; ubLoop2 < ubLoop; ubLoop2++)
                {
                    MemFree(ppStructure[ubLoop2]);
                }
                MemFree(ppStructure);
                return (null);
            }
            ppStructure[ubLoop].sGridNo = sBaseGridNo + ppTile[ubLoop].sPosRelToBase;
            if (ubLoop != BASE_TILE)
            {
                ppStructure[ubLoop].sBaseGridNo = sBaseGridNo;
            }
            if (ppTile[ubLoop].fFlags.HasFlag(TILE.ON_ROOF))
            {
                ppStructure[ubLoop].sCubeOffset = (STRUCTURE_ON)((bLevel + 1) * PROFILE_Z_SIZE);
            }
            else
            {
                ppStructure[ubLoop].sCubeOffset = (STRUCTURE_ON)(bLevel * PROFILE_Z_SIZE);
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
            else if (pLevelNode.uiFlags.HasFlag(LEVELNODEFLAGS.ROTTINGCORPSE) || pDBStructure.fFlags.HasFlag(STRUCTUREFLAGS.CORPSE))
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
            usStructureID = (int)(TOTAL_SOLDIERS + (int)pLevelNode.pAniTile.uiUserData);
        }
        else
        {
            gusNextAvailableStructureID++;
            if (gusNextAvailableStructureID == 0)
            {
                // skip past the #s for soldiers' structures and the invalid structure #
                gusNextAvailableStructureID = FIRST_AVAILABLE_STRUCTURE_ID;
            }
            usStructureID = gusNextAvailableStructureID;
        }
        // now add all these to the world!
        for (ubLoop = BASE_TILE; ubLoop < pDBStructure.ubNumberOfTiles; ubLoop++)
        {
            sGridNo = ppStructure[ubLoop].sGridNo;
            if (ubLoop == BASE_TILE)
            {
                sBaseTileHeight = gpWorldLevelData[sGridNo].sHeight;
            }
            else
            {
                if (gpWorldLevelData[sGridNo].sHeight != sBaseTileHeight)
                {
                    // not level ground! abort!
                    for (ubLoop2 = BASE_TILE; ubLoop2 < ubLoop; ubLoop2++)
                    {
                        DeleteStructureFromTile((gpWorldLevelData[ppStructure[ubLoop2].sGridNo]), ppStructure[ubLoop2]);
                    }
                    MemFree(ppStructure);
                    return (null);
                }
            }
            if (AddStructureToTile((gpWorldLevelData[sGridNo]), ppStructure[ubLoop], usStructureID) == false)
            {
                // error! abort!
                for (ubLoop2 = BASE_TILE; ubLoop2 < ubLoop; ubLoop2++)
                {
                    DeleteStructureFromTile((gpWorldLevelData[ppStructure[ubLoop2].sGridNo]), ppStructure[ubLoop2]);
                }
                MemFree(ppStructure);
                return (null);
            }
        }

        pBaseStructure = ppStructure[BASE_TILE];
        pLevelNode.pStructureData = pBaseStructure;

        MemFree(ppStructure);
        // And we're done! return a pointer to the base structure!

        return (pBaseStructure);
    }

    bool AddStructureToWorld(int sBaseGridNo, int bLevel, DB_STRUCTURE_REF pDBStructureRef, LEVELNODE? pLevelN)
    {
        STRUCTURE? pStructure;

        pStructure = InternalAddStructureToWorld(sBaseGridNo, bLevel, pDBStructureRef, (LEVELNODE?)pLevelN);
        if (pStructure == null)
        {
            return (false);
        }
        return (true);
    }

    //
    // Structure deletion functions
    //

    private static void DeleteStructureFromTile(MAP_ELEMENT pMapElement, STRUCTURE? pStructure)
    { // removes a STRUCTURE element at a particular location from the world
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
        MemFree(pStructure);
    }

    private static STRUCTURE? InternalSwapStructureForPartner(int sGridNo, STRUCTURE? pStructure, bool fFlipSwitches, bool fStoreInMap)
    { // switch structure 
        LEVELNODE? pLevelNode;
        LEVELNODE? pShadowNode;
        STRUCTURE? pBaseStructure;
        STRUCTURE? pNewBaseStructure;
        DB_STRUCTURE_REF? pPartnerDBStructure;
        bool fDoor;

        int bDelta;
        int ubHitPoints;
        STRUCTURE_ON sCubeOffset;

        if (pStructure == null)
        {
            return (null);
        }
        pBaseStructure = FindBaseStructure(pStructure);
        CHECKF(pBaseStructure);
        if ((pBaseStructure.pDBStructureRef.pDBStructure).bPartnerDelta == NO_PARTNER_STRUCTURE)
        {
            return (null);
        }
        fDoor = ((pBaseStructure.fFlags.HasFlag(STRUCTUREFLAGS.ANYDOOR)));
        pLevelNode = WorldStructures.FindLevelNodeBasedOnStructure(pBaseStructure.sGridNo, pBaseStructure);
        if (pLevelNode == null)
        {
            return (null);
        }
        pShadowNode = WorldManager.FindShadow(pBaseStructure.sGridNo, pLevelNode.usIndex);

        // record values
        bDelta = pBaseStructure.pDBStructureRef.pDBStructure.bPartnerDelta;
        pPartnerDBStructure = pBaseStructure.pDBStructureRef + bDelta;
        sGridNo = pBaseStructure.sGridNo;
        ubHitPoints = pBaseStructure.ubHitPoints;
        sCubeOffset = pBaseStructure.sCubeOffset;
        // delete the old structure and add the new one
        if (DeleteStructureFromWorld(pBaseStructure) == false)
        {
            return (null);
        }
        pNewBaseStructure = InternalAddStructureToWorld(sGridNo, ((int)sCubeOffset / PROFILE_Z_SIZE), pPartnerDBStructure, pLevelNode);
        if (pNewBaseStructure == null)
        {
            return (null);
        }
        // set values in the new structure
        pNewBaseStructure.ubHitPoints = ubHitPoints;
        if (!fDoor)
        { // swap the graphics

            // store removal of previous if necessary
            if (fStoreInMap)
            {
                SaveLoadMap.ApplyMapChangesToMapTempFile(true);
                RemoveStructFromMapTempFile(sGridNo, pLevelNode.usIndex);
            }

            pLevelNode.usIndex += bDelta;

            // store removal of new one if necessary
            if (fStoreInMap)
            {
                AddStructToMapTempFile(sGridNo, pLevelNode.usIndex);
                SaveLoadMap.ApplyMapChangesToMapTempFile(false);
            }

            if (pShadowNode != null)
            {
                pShadowNode.usIndex += bDelta;
            }
        }

        //if ( (pNewBaseStructure.fFlags.HasFlag(STRUCTUREFLAGS.SWITCH) && (pNewBaseStructure.fFlags.HasFlag(STRUCTUREFLAGS.OPEN) )
        if (false /*fFlipSwitches*/ )
        {
            if (pNewBaseStructure.fFlags.HasFlag(STRUCTUREFLAGS.SWITCH))
            {
                // just turned a switch on!
                ActivateSwitchInGridNo(NOBODY, sGridNo);
            }
        }
        return (pNewBaseStructure);
    }

    public static STRUCTURE? SwapStructureForPartner(int sGridNo, STRUCTURE? pStructure)
    {
        return (InternalSwapStructureForPartner(sGridNo, pStructure, true, false));
    }

    public static STRUCTURE? SwapStructureForPartnerWithoutTriggeringSwitches(int sGridNo, STRUCTURE? pStructure)
    {
        return (InternalSwapStructureForPartner(sGridNo, pStructure, false, false));
    }

    public static STRUCTURE? SwapStructureForPartnerAndStoreChangeInMap(int sGridNo, STRUCTURE? pStructure)
    {
        return (InternalSwapStructureForPartner(sGridNo, pStructure, true, true));
    }

    public static STRUCTURE? FindStructure(int sGridNo, STRUCTUREFLAGS fFlags)
    { // finds a structure that matches any of the given flags
        STRUCTURE? pCurrent;

        pCurrent = gpWorldLevelData[sGridNo].pStructureHead;
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

    public static STRUCTURE? FindNextStructure(STRUCTURE? pStructure, STRUCTUREFLAGS fFlags)
    {
        STRUCTURE? pCurrent;

        CHECKF(pStructure);
        pCurrent = pStructure.pNext;
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

    public static STRUCTURE? FindStructureByID(int sGridNo, int usStructureID)
    { // finds a structure that matches any of the given flags
        STRUCTURE? pCurrent;

        pCurrent = gpWorldLevelData[sGridNo].pStructureHead;
        while (pCurrent != null)
        {
            if (pCurrent.usStructureID == usStructureID)
            {
                return (pCurrent);
            }
            pCurrent = pCurrent.pNext;
        }
        return (null);
    }

    public static STRUCTURE? FindBaseStructure(STRUCTURE? pStructure)
    { // finds the base structure for any structure
        CHECKF(pStructure);
        if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.BASE_TILE))
        {
            return (pStructure);
        }
        return (FindStructureByID(pStructure.sBaseGridNo, pStructure.usStructureID));
    }

    STRUCTURE? FindNonBaseStructure(int sGridNo, STRUCTURE? pStructure)
    { // finds a non-base structure in a location
        CHECKF(pStructure);
        if (!(pStructure.fFlags.HasFlag(STRUCTUREFLAGS.BASE_TILE)))
        {   // error!
            return (null);
        }

        return (FindStructureByID(sGridNo, pStructure.usStructureID));
    }

    int GetBaseTile(STRUCTURE? pStructure)
    {
        if (pStructure == null)
        {
            return (-1);
        }
        if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.BASE_TILE))
        {
            return (pStructure.sGridNo);
        }
        else
        {
            return (pStructure.sBaseGridNo);
        }
    }

    public static int StructureHeight(STRUCTURE? pStructure)
    { // return the height of an object from 1-4
        int ubLoopX, ubLoopY;
        PROFILE? pShape;
        int ubShapeValue;
        int bLoopZ;
        int bGreatestHeight = -1;

        if (pStructure == null || pStructure.pShape == null)
        {
            return (0);
        }

        if (pStructure.ubStructureHeight != 0)
        {
            return (pStructure.ubStructureHeight);
        }

        pShape = pStructure.pShape;

        // loop horizontally on the X and Y planes
        for (ubLoopX = 0; ubLoopX < PROFILE_X_SIZE; ubLoopX++)
        {
            for (ubLoopY = 0; ubLoopY < PROFILE_Y_SIZE; ubLoopY++)
            {
                ubShapeValue = (pShape)[ubLoopX, ubLoopY];
                // loop DOWN vertically so that we find the tallest point first
                // and don't need to check any below it
                for (bLoopZ = PROFILE_Z_SIZE - 1; bLoopZ > bGreatestHeight; bLoopZ--)
                {
                    if ((ubShapeValue & AtHeight[bLoopZ]) != 0)
                    {
                        bGreatestHeight = bLoopZ;
                        if (bGreatestHeight == PROFILE_Z_SIZE - 1)
                        {
                            // store height
                            pStructure.ubStructureHeight = bGreatestHeight + 1;
                            return (bGreatestHeight + 1);
                        }
                        break;
                    }
                }
            }
        }
        // store height
        pStructure.ubStructureHeight = bGreatestHeight + 1;
        return (bGreatestHeight + 1);
    }

    int GetTallestStructureHeight(int sGridNo, bool fOnRoof)
    {
        STRUCTURE? pCurrent;
        int iHeight;
        int iTallest = 0;
        STRUCTURE_ON sDesiredHeight;

        if (fOnRoof)
        {
            sDesiredHeight = STRUCTURE_ON.ROOF;
        }
        else
        {
            sDesiredHeight = STRUCTURE_ON.GROUND;
        }
        pCurrent = gpWorldLevelData[sGridNo].pStructureHead;
        while (pCurrent != null)
        {
            if (pCurrent.sCubeOffset == sDesiredHeight)
            {
                iHeight = StructureHeight(pCurrent);
                if (iHeight > iTallest)
                {
                    iTallest = iHeight;
                }
            }
            pCurrent = pCurrent.pNext;
        }
        return (iTallest);
    }


    int GetStructureTargetHeight(int sGridNo, bool fOnRoof)
    {
        STRUCTURE? pCurrent;
        int iHeight;
        int iTallest = 0;
        STRUCTURE_ON sDesiredHeight;

        if (fOnRoof)
        {
            sDesiredHeight = STRUCTURE_ON.ROOF;
        }
        else
        {
            sDesiredHeight = STRUCTURE_ON.GROUND;
        }

        // prioritize openable structures and doors
        pCurrent = FindStructure(sGridNo, (STRUCTUREFLAGS.DOOR | STRUCTUREFLAGS.OPENABLE));
        if (pCurrent is not null)
        {
            // use this structure
            if (pCurrent.fFlags.HasFlag(STRUCTUREFLAGS.DOOR))
            {
                iTallest = 3; // don't aim at the very top of the door
            }
            else
            {
                iTallest = StructureHeight(pCurrent);
            }
        }
        else
        {
            pCurrent = gpWorldLevelData[sGridNo].pStructureHead;
            while (pCurrent != null)
            {
                if (pCurrent.sCubeOffset == sDesiredHeight)
                {
                    iHeight = StructureHeight(pCurrent);

                    if (iHeight > iTallest)
                    {
                        iTallest = iHeight;
                    }
                }
                pCurrent = pCurrent.pNext;
            }
        }
        return (iTallest);
    }


    public static int StructureBottomLevel(STRUCTURE? pStructure)
    { // return the bottom level of an object, from 1-4
        int ubLoopX, ubLoopY;
        PROFILE? pShape;
        int ubShapeValue;
        int bLoopZ;
        int bLowestHeight = PROFILE_Z_SIZE;

        if (pStructure == null || pStructure.pShape == null)
        {
            return (0);
        }
        pShape = pStructure.pShape;

        // loop horizontally on the X and Y planes
        for (ubLoopX = 0; ubLoopX < PROFILE_X_SIZE; ubLoopX++)
        {
            for (ubLoopY = 0; ubLoopY < PROFILE_Y_SIZE; ubLoopY++)
            {
                ubShapeValue = (pShape)[ubLoopX, ubLoopY];
                // loop DOWN vertically so that we find the tallest point first
                // and don't need to check any below it
                for (bLoopZ = 0; bLoopZ < bLowestHeight; bLoopZ++)
                {
                    if (ubShapeValue & AtHeight[bLoopZ])
                    {
                        bLowestHeight = bLoopZ;
                        if (bLowestHeight == 0)
                        {
                            return (1);
                        }
                        break;
                    }
                }
            }
        }
        return (bLowestHeight + 1);
    }


    public static bool StructureDensity(STRUCTURE? pStructure, out int pubLevel0, out int pubLevel1, out int pubLevel2, out int pubLevel3)
    {
        int ubLoopX, ubLoopY;
        int ubShapeValue;
        PROFILE? pShape;

        CHECKF(pStructure);
        pubLevel0 = 0;
        pubLevel1 = 0;
        pubLevel2 = 0;
        pubLevel3 = 0;

        pShape = pStructure.pShape;

        for (ubLoopX = 0; ubLoopX < PROFILE_X_SIZE; ubLoopX++)
        {
            for (ubLoopY = 0; ubLoopY < PROFILE_Y_SIZE; ubLoopY++)
            {
                ubShapeValue = (pShape)[ubLoopX, ubLoopY];
                if (ubShapeValue & AtHeight[0])
                {
                    (pubLevel0)++;
                }
                if (ubShapeValue & AtHeight[1])
                {
                    (pubLevel1)++;
                }
                if (ubShapeValue & AtHeight[2])
                {
                    (pubLevel2)++;
                }
                if (ubShapeValue & AtHeight[3])
                {
                    (pubLevel3)++;
                }

            }
        }
        // convert values to percentages!
        pubLevel0 *= 4;
        pubLevel1 *= 4;
        pubLevel2 *= 4;
        pubLevel3 *= 4;
        return (true);
    }

    public static int DamageStructure(STRUCTURE? pStructure, int ubDamage, int ubReason, int sGridNo, int sX, int sY, int ubOwner)
    {   // do damage to a structure; returns true if the structure should be removed

        STRUCTURE? pBase;
        int ubArmour;
        //LEVELNODE			*pNode;

        CHECKF(pStructure);
        if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.PERSON) || pStructure.fFlags.HasFlag(STRUCTUREFLAGS.CORPSE))
        {
            // don't hurt this structure, it's used for hit detection only!
            return (0);
        }

        if ((pStructure.pDBStructureRef.pDBStructure.ubArmour == MATERIAL.INDESTRUCTABLE_METAL)
            || (pStructure.pDBStructureRef.pDBStructure.ubArmour == MATERIAL.INDESTRUCTABLE_STONE))
        {
            return (0);
        }

        // Account for armour!
        if (ubReason == STRUCTURE_DAMAGE_EXPLOSION)
        {
            if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.EXPLOSIVE))
            {
                ubArmour = gubMaterialArmour[pStructure.pDBStructureRef.pDBStructure.ubArmour] / 3;
            }
            else
            {
                ubArmour = gubMaterialArmour[pStructure.pDBStructureRef.pDBStructure.ubArmour] / 2;
            }

            if (ubArmour > ubDamage)
            {
                // didn't even scratch the paint
                return (0);
            }
            else
            {
                // did some damage to the structure
                ubDamage -= ubArmour;
            }
        }
        else
        {
            ubDamage = 0;
        }

        // OK, Let's check our reason
        if (ubReason == STRUCTURE_DAMAGE_GUNFIRE)
        {
            // If here, we have penetrated, check flags
            // Are we an explodable structure?
            if ((pStructure.fFlags.HasFlag(STRUCTUREFLAGS.EXPLOSIVE)) && Globals.Random.Next(2) > 0)
            {
                // Remove struct!
                pBase = FindBaseStructure(pStructure);

                // ATE: Set hit points to zero....
                pBase.ubHitPoints = 0;

                // Get LEVELNODE for struct and remove!
                // pNode = FindLevelNodeBasedOnStructure( pBase.sGridNo, pBase );


                //Set a flag indicating that the following changes are to go the the maps temp file
                // ApplyMapChangesToMapTempFile( true );
                // Remove!
                // RemoveStructFromLevelNode( pBase.sGridNo, pNode );
                // ApplyMapChangesToMapTempFile( false );

                // Generate an explosion here!
                ExplosionControl.IgniteExplosion(ubOwner, sX, sY, 0, sGridNo, Items.STRUCTURE_IGNITE, 0);

                // ATE: Return false here, as we are dealing with deleting the graphic here...
                return (0);
            }

            // Make hit sound....
            if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.CAVEWALL))
            {
                //PlayJA2Sample(S_VEG_IMPACT1, RATE_11025, SoundVolume(HIGHVOLUME, sGridNo), 1, SoundDir(sGridNo));
            }
            else
            {
                if (guiMaterialHitSound[pStructure.pDBStructureRef.pDBStructure.ubArmour] != -1)
                {
                    //PlayJA2Sample(guiMaterialHitSound[pStructure.pDBStructureRef.pDBStructure.ubArmour], RATE_11025, SoundVolume(HIGHVOLUME, sGridNo), 1, SoundDir(sGridNo));
                }
            }
            // Don't update damage HPs....
            return (1);
        }

        // OK, LOOK FOR A SAM SITE, UPDATE....
//        UpdateAndDamageSAMIfFound(gWorldSectorX, gWorldSectorY, gbWorldSectorZ, sGridNo, ubDamage);

        // find the base so we can reduce the hit points!
        pBase = FindBaseStructure(pStructure);
        CHECKF(pBase);
        if (pBase.ubHitPoints <= ubDamage)
        {
            // boom! structure destroyed!
            return (1);
        }
        else
        {
            pBase.ubHitPoints -= ubDamage;

            //Since the structure is being damaged, set the map element that a structure is damaged
            gpWorldLevelData[sGridNo].uiFlags |= MAPELEMENTFLAGS.STRUCTUREFLAGS_DAMAGED;

            // We are a little damaged....
            return (2);
        }
    }

    static string[] WallOrientationString =
    {
        "None",
        "Inside left",
        "Inside right",
        "Outside left",
        "Outside right"
    };

    void DebugStructurePage1()
    {
        STRUCTURE? pStructure;
        STRUCTURE? pBase;
        //LEVELNODE *		pLand;
        int sGridNo;
        STRUCTURE_ON sDesiredLevel;
        int bHeight, bDens0, bDens1, bDens2, bDens3;
        int bStructures;

        FontSubSystem.SetFont(FontStyle.LARGEFONT1);

        gprintf(0, 0, "DEBUG STRUCTURES PAGE 1 OF 1");
        if (IsometricUtils.GetMouseMapPos(out sGridNo) == false)
        {
            return;
            //gprintf( 0, LINE_HEIGHT * 1, "No structure selected" );
        }

        if (gsInterfaceLevel == InterfaceLevel.I_GROUND_LEVEL)
        {
            sDesiredLevel = STRUCTURE_ON.GROUND;
        }
        else
        {
            sDesiredLevel = STRUCTURE_ON.ROOF;
        }

        gprintf(320, 0, "Building %d", gubBuildingInfo[sGridNo]);
        /*
        pLand = gpWorldLevelData[sGridNo].pLandHead;
        gprintf( 320, 0, "Fake light %d", pLand.ubFakeShadeLevel );
        gprintf( 320, LINE_HEIGHT, "Real light: ground %d roof %d", LightTrueLevel( sGridNo, 0 ), LightTrueLevel( sGridNo, 1 ) );
        */

        pStructure = gpWorldLevelData[sGridNo].pStructureHead;
        while (pStructure != null)
        {
            if (pStructure.sCubeOffset == sDesiredLevel)
            {
                break;
            }
            pStructure = pStructure.pNext;
        }

        if (pStructure != null)
        {
            if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.GENERIC))
            {
                gprintf(0, LINE_HEIGHT * 1, "Generic structure %x #%d", pStructure.fFlags, pStructure.pDBStructureRef.pDBStructure.usStructureNumber);
            }
            else if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.TREE))
            {
                gprintf(0, LINE_HEIGHT * 1, "Tree");
            }
            else if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.WALL))
            {
                gprintf(0, LINE_HEIGHT * 1, "Wall with orientation %s", WallOrientationString[pStructure.ubWallOrientation]);
            }
            else if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.WALLNWINDOW))
            {
                gprintf(0, LINE_HEIGHT * 1, "Wall with window");
            }
            else if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.VEHICLE))
            {
                gprintf(0, LINE_HEIGHT * 1, "Vehicle %d", pStructure.pDBStructureRef.pDBStructure.usStructureNumber);
            }
            else if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.NORMAL_ROOF))
            {
                gprintf(0, LINE_HEIGHT * 1, "Roof");
            }
            else if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.SLANTED_ROOF))
            {
                gprintf(0, LINE_HEIGHT * 1, "Slanted roof");
            }
            else if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.DOOR))
            {
                gprintf(0, LINE_HEIGHT * 1, "Door with orientation %s", WallOrientationString[pStructure.ubWallOrientation]);
            }
            else if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.SLIDINGDOOR))
            {
                gprintf(0, LINE_HEIGHT * 1, "%s sliding door with orientation %s",
                    (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.OPEN) ? "Open" : "Closed"),
                    WallOrientationString[pStructure.ubWallOrientation]);
            }
            else if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.DDOOR_LEFT))
            {
                gprintf(0, LINE_HEIGHT * 1, "DDoorLft with orientation %s", WallOrientationString[pStructure.ubWallOrientation]);
            }
            else if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.DDOOR_RIGHT))
            {
                gprintf(0, LINE_HEIGHT * 1, "DDoorRt with orientation %s", WallOrientationString[pStructure.ubWallOrientation]);
            }
            else
            {
                gprintf(0, LINE_HEIGHT * 1, "UNKNOWN STRUCTURE! (%x)", pStructure.fFlags);
            }
            bHeight = StructureHeight(pStructure);
            pBase = FindBaseStructure(pStructure);
            gprintf(0, LINE_HEIGHT * 2, "Structure height %d, cube offset %d, armour %d, HP %d", bHeight, pStructure.sCubeOffset, gubMaterialArmour[pStructure.pDBStructureRef.pDBStructure.ubArmour], pBase.ubHitPoints);
            if (StructureDensity(pStructure, out bDens0, out bDens1, out bDens2, out bDens3) == true)
            {
                gprintf(0, LINE_HEIGHT * 3, "Structure fill %d%%/%d%%/%d%%/%d%% density %d", bDens0, bDens1, bDens2, bDens3,
                    pStructure.pDBStructureRef.pDBStructure.ubDensity);
            }

# if !LOS_DEBUG
            gprintf(0, LINE_HEIGHT * 4, "Structure ID %d", pStructure.usStructureID);
#endif

            pStructure = gpWorldLevelData[sGridNo].pStructureHead;
            for (bStructures = 0; pStructure != null; pStructure = pStructure.pNext)
            {
                bStructures++;
            }
            gprintf(0, LINE_HEIGHT * 12, "Number of structures = %d", bStructures);
        }
# if LOS_DEBUG
        if (gLOSTestResults.fLOSTestPerformed)
        {
            gprintf(0, LINE_HEIGHT * 4, "LOS from (%7d,%7d,%7d)", gLOSTestResults.iStartX, gLOSTestResults.iStartY, gLOSTestResults.iStartZ);
            gprintf(0, LINE_HEIGHT * 5, "to (%7d,%7d,%7d)", gLOSTestResults.iEndX, gLOSTestResults.iEndY, gLOSTestResults.iEndZ);
            if (gLOSTestResults.fOutOfRange)
            {
                gprintf(0, LINE_HEIGHT * 6, "is out of range");
            }
            else if (gLOSTestResults.fLOSClear)
            {
                gprintf(0, LINE_HEIGHT * 6, "is clear!");
            }
            else
            {
                gprintf(0, LINE_HEIGHT * 6, "is blocked at (%7d,%7d,%7d)!", gLOSTestResults.iStoppedX, gLOSTestResults.iStoppedY, gLOSTestResults.iStoppedZ);
                gprintf(0, LINE_HEIGHT * 10, "Blocked at cube level %d", gLOSTestResults.iCurrCubesZ);
            }
            gprintf(0, LINE_HEIGHT * 7, "Passed through %d tree bits!", gLOSTestResults.ubTreeSpotsHit);
            gprintf(0, LINE_HEIGHT * 8, "Maximum range was %7d", gLOSTestResults.iMaxDistance);
            gprintf(0, LINE_HEIGHT * 9, "actual range was %7d", gLOSTestResults.iDistance);
            if (gLOSTestResults.ubChanceToGetThrough <= 100)
            {
                gprintf(0, LINE_HEIGHT * 11, "Chance to get through was %d", gLOSTestResults.ubChanceToGetThrough);
            }
        }
#endif
        gprintf(0, LINE_HEIGHT * 13, "N %d NE %d E %d SE %d",
            gubWorldMovementCosts[sGridNo, WorldDirections.NORTH, gsInterfaceLevel],
            gubWorldMovementCosts[sGridNo, WorldDirections.NORTHEAST, gsInterfaceLevel],
            gubWorldMovementCosts[sGridNo, WorldDirections.EAST, gsInterfaceLevel],
            gubWorldMovementCosts[sGridNo, WorldDirections.SOUTHEAST, gsInterfaceLevel]);
        gprintf(0, LINE_HEIGHT * 14, "S %d SW %d W %d NW %d",
            gubWorldMovementCosts[sGridNo, WorldDirections.SOUTH, gsInterfaceLevel],
            gubWorldMovementCosts[sGridNo, WorldDirections.SOUTHWEST, gsInterfaceLevel],
            gubWorldMovementCosts[sGridNo, WorldDirections.WEST, gsInterfaceLevel],
            gubWorldMovementCosts[sGridNo, WorldDirections.NORTHWEST, gsInterfaceLevel]);
        gprintf(0, LINE_HEIGHT * 15, "Ground smell %d strength %d",
            Smell.SMELL_TYPE(gpWorldLevelData[sGridNo].ubSmellInfo),
            Smell.SMELL_STRENGTH(gpWorldLevelData[sGridNo].ubSmellInfo));

# if COUNT_PATHS
        if (guiTotalPathChecks > 0)
        {
            gprintf(0, LINE_HEIGHT * 16,
                "Total %ld, %%succ %3ld | %%failed %3ld | %%unsucc %3ld",
                guiTotalPathChecks,
                100 * guiSuccessfulPathChecks / guiTotalPathChecks,
                100 * guiFailedPathChecks / guiTotalPathChecks,
                100 * guiUnsuccessfulPathChecks / guiTotalPathChecks);

        }
#else
        gprintf(0, LINE_HEIGHT * 16,
            "Adj soldiers %d", gpWorldLevelData[sGridNo].ubAdjacentSoldierCnt);
#endif
    }

    public static bool AddZStripInfoToVObject(HVOBJECT hVObject, STRUCTURE_FILE_REF? pStructureFileRef, bool fFromAnimation, int sSTIStartIndex)
    {
        int uiLoop;
        int ubLoop2;
        int ubNumIncreasing = 0;
        int ubNumStable = 0;
        int ubNumDecreasing = 0;
        bool fFound = false;
        ZStripInfo? pCurr;
        int sLeftHalfWidth;
        int sRightHalfWidth;
        int sOffsetX;
        int sOffsetY;
        int usWidth;
        int usHeight;
        DB_STRUCTURE_REF? pDBStructureRef;
        DB_STRUCTURE? pDBStructure = null;
        int sSTIStep = 0;
        int sStructIndex = 0;
        int sNext;
        int uiDestVoIndex;
        bool fCopyIntoVo;
        bool fFirstTime;


        if (pStructureFileRef.usNumberOfStructuresStored == 0)
        {
            return (true);
        }
        for (uiLoop = 0; uiLoop < pStructureFileRef.usNumberOfStructures; uiLoop++)
        {
            pDBStructureRef = (pStructureFileRef.pDBStructureRef[uiLoop]);
            pDBStructure = pDBStructureRef.pDBStructure;
            //if (pDBStructure != null && pDBStructure.ubNumberOfTiles > 1 && !(pDBStructure.fFlags.HasFlag(STRUCTUREFLAGS.WALLSTUFF)) )
            if (pDBStructure != null && pDBStructure.ubNumberOfTiles > 1)
            {
                for (ubLoop2 = 1; ubLoop2 < pDBStructure.ubNumberOfTiles; ubLoop2++)
                {
                    if (pDBStructureRef.ppTile[ubLoop2].sPosRelToBase != 0)
                    {
                        // spans multiple tiles! (could be two levels high in one tile)
                        fFound = true;
                        break;
                    }
                }
            }
        }

        // ATE: Make all corpses use z-strip info..
        if (pDBStructure != null && pDBStructure.fFlags.HasFlag(STRUCTUREFLAGS.CORPSE))
        {
            fFound = true;
        }

        if (!fFound)
        {
            // no multi-tile images in this vobject; that's okay... return!
            return (true);
        }

//        hVObject.ppZStripInfo = MemAlloc(sizeof(ZStripInfo?) * hVObject.usNumberOfObjects);
        if (hVObject.ppZStripInfo == null)
        {
            return (false);
        }
        // memset(hVObject.ppZStripInfo, 0, sizeof(ZStripInfo?) * hVObject.usNumberOfObjects);


        if (fFromAnimation)
        {
            // Determine step index for STI
            if (sSTIStartIndex == -1)
            {
                // one-direction only for this anim structure
                sSTIStep = hVObject.usNumberOfObjects;
                sSTIStartIndex = 0;
            }
            else
            {
                sSTIStep = (hVObject.usNumberOfObjects / pStructureFileRef.usNumberOfStructures);
            }
        }
        else
        {
            sSTIStep = 1;
        }

        sStructIndex = 0;
        sNext = sSTIStartIndex + sSTIStep;
        fFirstTime = true;

        for (uiLoop = (int)sSTIStartIndex; uiLoop < hVObject.usNumberOfObjects; uiLoop++)
        {
            // Defualt to true
            fCopyIntoVo = true;

            // Increment struct index....
            if (uiLoop == (int)sNext)
            {
                sNext = (int)(uiLoop + sSTIStep);
                sStructIndex++;
            }
            else
            {
                if (fFirstTime)
                {
                    fFirstTime = false;
                }
                else
                {
                    fCopyIntoVo = false;
                }
            }

            if (fFromAnimation)
            {
                uiDestVoIndex = sStructIndex;
            }
            else
            {
                uiDestVoIndex = uiLoop;
            }


            if (fCopyIntoVo && sStructIndex < pStructureFileRef.usNumberOfStructures)
            {
                pDBStructure = pStructureFileRef.pDBStructureRef[sStructIndex].pDBStructure;
                if (pDBStructure != null && (pDBStructure.ubNumberOfTiles > 1 || (pDBStructure.fFlags.HasFlag(STRUCTUREFLAGS.CORPSE))))
                //if (pDBStructure != null && pDBStructure.ubNumberOfTiles > 1 )
                {
                    // ATE: We allow SLIDING DOORS of 2 tile sizes...
                    if (!(pDBStructure.fFlags.HasFlag(STRUCTUREFLAGS.ANYDOOR)
                        || ((pDBStructure.fFlags.HasFlag(STRUCTUREFLAGS.ANYDOOR)))
                        && (pDBStructure.fFlags.HasFlag(STRUCTUREFLAGS.SLIDINGDOOR))))
                    {
//                        hVObject.ppZStripInfo[uiDestVoIndex] = MemAlloc(sizeof(ZStripInfo));
                        if (hVObject.ppZStripInfo[uiDestVoIndex] == null)
                        {
                            // augh!! out of memory!  free everything allocated and abort
                            for (ubLoop2 = 0; ubLoop2 < uiLoop; ubLoop2++)
                            {
                                if (hVObject.ppZStripInfo[ubLoop2] != null)
                                {
                                    MemFree(hVObject.ppZStripInfo[uiLoop]);
                                }
                            }
                            MemFree(hVObject.ppZStripInfo);
                            hVObject.ppZStripInfo = null;
                            return (false);
                        }
                        else
                        {
                            pCurr = hVObject.ppZStripInfo[uiDestVoIndex];

                            ubNumIncreasing = 0;
                            ubNumStable = 0;
                            ubNumDecreasing = 0;

                            // time to do our calculations!
                            sOffsetX = hVObject.pETRLEObject[uiLoop].sOffsetX;
                            sOffsetY = hVObject.pETRLEObject[uiLoop].sOffsetY;
                            usWidth = hVObject.pETRLEObject[uiLoop].usWidth;
                            usHeight = hVObject.pETRLEObject[uiLoop].usHeight;
                            if (pDBStructure.fFlags.HasFlag(STRUCTUREFLAGS.MOBILE | STRUCTUREFLAGS.CORPSE))
                            {
                                int i = 0;
                                // adjust for the difference between the animation and structure base tile				

                                //if (pDBStructure.fFlags.HasFlag(STRUCTUREFLAGS.MOBILE ) )
                                {
                                    sOffsetX = sOffsetX + (WORLD_TILE_X / 2);
                                    sOffsetY = sOffsetY + (WORLD_TILE_Y / 2);
                                }
                                // adjust for the tile offset 
                                sOffsetX = sOffsetX - pDBStructure.bZTileOffsetX * (WORLD_TILE_X / 2) + pDBStructure.bZTileOffsetY * (WORLD_TILE_X / 2);
                                sOffsetY = sOffsetY - pDBStructure.bZTileOffsetY * (WORLD_TILE_Y / 2);
                            }

                            // figure out how much of the image is on each side of
                            // the bottom corner of the base tile
                            if (sOffsetX <= 0)
                            {
                                // note that the adjustments here by (WORLD_TILE_X / 2) are to account for the X difference
                                // between the blit position and the bottom corner of the base tile
                                sRightHalfWidth = usWidth + sOffsetX - (WORLD_TILE_X / 2);

                                if (sRightHalfWidth >= 0)
                                {
                                    // Case 1: negative image offset, image straddles bottom corner

                                    // negative of a negative is positive
                                    sLeftHalfWidth = -sOffsetX + (WORLD_TILE_X / 2);
                                }
                                else
                                {
                                    // Case 2: negative image offset, image all on left side

                                    // bump up the LeftHalfWidth to the right edge of the last tile-half,
                                    // so we can calculate the size of the leftmost portion accurately
                                    // NB subtracting a negative to add the absolute value
                                    sLeftHalfWidth = usWidth - (sRightHalfWidth % (WORLD_TILE_X / 2));
                                    sRightHalfWidth = 0;
                                }
                            }
                            else if (sOffsetX < (WORLD_TILE_X / 2))
                            {
                                sLeftHalfWidth = (WORLD_TILE_X / 2) - sOffsetX;
                                sRightHalfWidth = usWidth - sLeftHalfWidth;
                                if (sRightHalfWidth <= 0)
                                {
                                    // Case 3: positive offset < 20, image all on left side
                                    // should never happen because these images are multi-tile!
                                    sRightHalfWidth = 0;
                                    // fake the left width to one half-tile
                                    sLeftHalfWidth = (WORLD_TILE_X / 2);
                                }
                                else
                                {
                                    // Case 4: positive offset < 20, image straddles bottom corner

                                    // all okay?
                                }
                            }
                            else
                            {
                                // Case 5: positive offset, image all on right side
                                // should never happen either 
                                sLeftHalfWidth = 0;
                                sRightHalfWidth = usWidth;
                            }

                            if (sLeftHalfWidth > 0)
                            {
                                ubNumIncreasing = sLeftHalfWidth / (WORLD_TILE_X / 2);
                            }
                            if (sRightHalfWidth > 0)
                            {
                                ubNumStable = 1;
                                if (sRightHalfWidth > (WORLD_TILE_X / 2))
                                {
                                    ubNumDecreasing = sRightHalfWidth / (WORLD_TILE_X / 2);
                                }
                            }
                            if (sLeftHalfWidth > 0)
                            {
                                pCurr.ubFirstZStripWidth = sLeftHalfWidth % (WORLD_TILE_X / 2);
                                if (pCurr.ubFirstZStripWidth == 0)
                                {
                                    ubNumIncreasing--;
                                    pCurr.ubFirstZStripWidth = (WORLD_TILE_X / 2);
                                }
                            }
                            else // right side only; offset is at least 20 (= WORLD_TILE_X / 2)
                            {
                                if (sOffsetX > WORLD_TILE_X)
                                {
                                    pCurr.ubFirstZStripWidth = (WORLD_TILE_X / 2) - (sOffsetX - WORLD_TILE_X) % (WORLD_TILE_X / 2);
                                }
                                else
                                {
                                    pCurr.ubFirstZStripWidth = WORLD_TILE_X - sOffsetX;
                                }
                                if (pCurr.ubFirstZStripWidth == 0)
                                {
                                    ubNumDecreasing--;
                                    pCurr.ubFirstZStripWidth = (WORLD_TILE_X / 2);
                                }

                            }

                            // now create the array!
                            pCurr.ubNumberOfZChanges = ubNumIncreasing + ubNumStable + ubNumDecreasing;
//                            pCurr.pbZChange = MemAlloc(pCurr.ubNumberOfZChanges);
                            if (pCurr.pbZChange == null)
                            {
                                // augh!
                                for (ubLoop2 = 0; ubLoop2 < uiLoop; ubLoop2++)
                                {
                                    if (hVObject.ppZStripInfo[ubLoop2] != null)
                                    {
                                        MemFree(hVObject.ppZStripInfo[uiLoop]);
                                    }
                                }
                                MemFree(hVObject.ppZStripInfo);
                                hVObject.ppZStripInfo = null;
                                return (false);
                            }
                            for (ubLoop2 = 0; ubLoop2 < ubNumIncreasing; ubLoop2++)
                            {
                                pCurr.pbZChange[ubLoop2] = 1;
                            }
                            for (; ubLoop2 < ubNumIncreasing + ubNumStable; ubLoop2++)
                            {
                                pCurr.pbZChange[ubLoop2] = 0;
                            }
                            for (; ubLoop2 < pCurr.ubNumberOfZChanges; ubLoop2++)
                            {
                                pCurr.pbZChange[ubLoop2] = -1;
                            }
                            if (ubNumIncreasing > 0)
                            {
                                pCurr.bInitialZChange = -(ubNumIncreasing);
                            }
                            else if (ubNumStable > 0)
                            {
                                pCurr.bInitialZChange = 0;
                            }
                            else
                            {
                                pCurr.bInitialZChange = -(ubNumDecreasing);
                            }
                        }
                    }
                }
            }
        }
        return (true);
    }

    bool InitStructureDB()
    {
        gusNextAvailableStructureID = FIRST_AVAILABLE_STRUCTURE_ID;
        return (true);
    }

    bool FiniStructureDB()
    {
        gusNextAvailableStructureID = FIRST_AVAILABLE_STRUCTURE_ID;
        return (true);
    }


    public static BLOCKING GetBlockingStructureInfo(int sGridNo, WorldDirections bDir, int bNextDir, int bLevel, out int pStructHeight, out STRUCTURE? ppTallestStructure, bool fWallsBlock)
    {
        STRUCTURE? pCurrent, pStructure = null;
        STRUCTURE_ON sDesiredLevel;
        bool fOKStructOnLevel = false;
        bool fMinimumBlockingFound = false;

        if (bLevel == 0)
        {
            sDesiredLevel = STRUCTURE_ON.GROUND;
        }
        else
        {
            sDesiredLevel = STRUCTURE_ON.ROOF;
        }

        pCurrent = gpWorldLevelData[sGridNo].pStructureHead;

        // If no struct, return
        if (pCurrent == null)
        {
            (pStructHeight) = StructureHeight(pCurrent);
            (ppTallestStructure) = null;
            return (BLOCKING.NOTHING_BLOCKING);
        }

        while (pCurrent != null)
        {
            // Check level!
            if (pCurrent.sCubeOffset == sDesiredLevel)
            {
                fOKStructOnLevel = true;
                pStructure = pCurrent;

                // Turn off if we are on upper level!
                if (pCurrent.fFlags.HasFlag(STRUCTUREFLAGS.ROOF) && bLevel == 1)
                {
                    fOKStructOnLevel = false;
                }

                // Don't stop FOV for people
                if (pCurrent.fFlags.HasFlag((STRUCTUREFLAGS.CORPSE | STRUCTUREFLAGS.PERSON)))
                {
                    fOKStructOnLevel = false;
                }


                if (pCurrent.fFlags.HasFlag(STRUCTUREFLAGS.TREE | STRUCTUREFLAGS.ANYFENCE))
                {
                    fMinimumBlockingFound = true;
                }

                // Default, if we are a wall, set full blocking
                if ((pCurrent.fFlags.HasFlag(STRUCTUREFLAGS.WALL)) && !fWallsBlock)
                {
                    // Return full blocking!
                    // OK! This will be handled by movement costs......!
                    fOKStructOnLevel = false;
                }

                // CHECK FOR WINDOW
                if (pCurrent.fFlags.HasFlag(STRUCTUREFLAGS.WALLNWINDOW))
                {
                    switch (pCurrent.ubWallOrientation)
                    {
                        case WallOrientation.OUTSIDE_TOP_LEFT:
                        case WallOrientation.INSIDE_TOP_LEFT:

                            (pStructHeight) = StructureHeight(pCurrent);
                            (ppTallestStructure) = pCurrent;

                            if (pCurrent.fFlags.HasFlag(STRUCTUREFLAGS.OPEN))
                            {
                                return (BLOCKING.TOPLEFT_OPEN_WINDOW);
                            }
                            else
                            {
                                return (BLOCKING.TOPLEFT_WINDOW);
                            }

                        case WallOrientation.OUTSIDE_TOP_RIGHT:
                        case WallOrientation.INSIDE_TOP_RIGHT:

                            (pStructHeight) = StructureHeight(pCurrent);
                            (ppTallestStructure) = pCurrent;

                            if (pCurrent.fFlags.HasFlag(STRUCTUREFLAGS.OPEN))
                            {
                                return (BLOCKING.TOPRIGHT_OPEN_WINDOW);
                            }
                            else
                            {
                                return (BLOCKING.TOPRIGHT_WINDOW);
                            }
                    }
                }

                // Check for door
                if (pCurrent.fFlags.HasFlag(STRUCTUREFLAGS.ANYDOOR))
                {
                    // If we are not opem, we are full blocking!
                    if (!(pCurrent.fFlags.HasFlag(STRUCTUREFLAGS.OPEN)))
                    {
                        (pStructHeight) = StructureHeight(pCurrent);
                        (ppTallestStructure) = pCurrent;
                        return (BLOCKING.FULL_BLOCKING);
                    }
                    else
                    {
                        switch (pCurrent.ubWallOrientation)
                        {
                            case WallOrientation.OUTSIDE_TOP_LEFT:
                            case WallOrientation.INSIDE_TOP_LEFT:

                                (pStructHeight) = StructureHeight(pCurrent);
                                (ppTallestStructure) = pCurrent;
                                return (BLOCKING.TOPLEFT_DOOR);

                            case WallOrientation.OUTSIDE_TOP_RIGHT:
                            case WallOrientation.INSIDE_TOP_RIGHT:

                                (pStructHeight) = StructureHeight(pCurrent);
                                (ppTallestStructure) = pCurrent;
                                return (BLOCKING.TOPRIGHT_DOOR);
                        }
                    }
                }
            }

            pCurrent = pCurrent.pNext;
        }

        // OK, here, we default to we've seen a struct, reveal just this one
        if (fOKStructOnLevel)
        {
            if (fMinimumBlockingFound)
            {
                (pStructHeight) = StructureHeight(pStructure);
                (ppTallestStructure) = pStructure;
                return (BLOCKING.REDUCE_RANGE);
            }
            else
            {
                (pStructHeight) = StructureHeight(pStructure);
                (ppTallestStructure) = pStructure;
                return (BLOCKING.NEXT_TILE);
            }
        }
        else
        {
            (pStructHeight) = 0;
            (ppTallestStructure) = null;
            return (BLOCKING.NOTHING_BLOCKING);
        }
    }




    int StructureFlagToType(STRUCTUREFLAGS uiFlag)
    {
        int ubLoop;
        STRUCTUREFLAGS uiBit = STRUCTUREFLAGS.GENERIC;

        for (ubLoop = 8; ubLoop < 32; ubLoop++)
        {
            if ((uiFlag & uiBit) != 0)
            {
                return (ubLoop);
            }

            uiBit <<= (STRUCTUREFLAGS)1;
        }
        return (0);
    }

    STRUCTUREFLAGS StructureTypeToFlag(int ubType)
    {
        STRUCTUREFLAGS uiFlag = (STRUCTUREFLAGS)0x1;

        uiFlag <<= ubType;
        return (uiFlag);
    }

    STRUCTURE? FindStructureBySavedInfo(int sGridNo, int ubType, WallOrientation ubWallOrientation, int bLevel)
    {
        STRUCTURE? pCurrent;
        STRUCTUREFLAGS uiTypeFlag;

        uiTypeFlag = StructureTypeToFlag(ubType);

        pCurrent = gpWorldLevelData[sGridNo].pStructureHead;
        while (pCurrent != null)
        {
            if (pCurrent.fFlags.HasFlag(uiTypeFlag)
                && pCurrent.ubWallOrientation == ubWallOrientation
                && ((bLevel == 0 && pCurrent.sCubeOffset == 0) || (bLevel > 0 && pCurrent.sCubeOffset > 0)))
            {
                return (pCurrent);
            }
            pCurrent = pCurrent.pNext;
        }
        return (null);
    }


    SoundDefine GetStructureOpenSound(STRUCTURE? pStructure, bool fClose)
    {
        var uiSoundID = pStructure.pDBStructureRef.pDBStructure.ubArmour switch
        {
            MATERIAL.LIGHT_METAL or MATERIAL.THICKER_METAL => SoundDefine.OPEN_LOCKER,
            MATERIAL.WOOD_WALL or MATERIAL.PLYWOOD_WALL or MATERIAL.FURNITURE => SoundDefine.OPEN_WOODEN_BOX,
            _ => SoundDefine.OPEN_DEFAULT_OPENABLE,
        };
        if (fClose)
        {
            uiSoundID++;
        }

        return (uiSoundID);
    }
}

[Flags]
public enum STRUCTUREFLAGS : uint
{
    // NOT used in DB structures!
    BASE_TILE = 0x00000001,
    OPEN = 0x00000002,
    OPENABLE = 0x00000004,
    // synonyms for OPENABLE
    CLOSEABLE = 0x00000004,
    SEARCHABLE = 0x00000004,
    HIDDEN = 0x00000008,
    MOBILE = 0x00000010,
    // PASSABLE is set for each structure instance where
    // the tile flag TILE_PASSABLE is set
    PASSABLE = 0x00000020,
    EXPLOSIVE = 0x00000040,
    TRANSPARENT = 0x00000080,
    GENERIC = 0x00000100,
    TREE = 0x00000200,
    FENCE = 0x00000400,
    WIREFENCE = 0x00000800,
    HASITEMONTOP = 0x00001000,            // ATE: HASITEM: struct has item on top of it
    SPECIAL = 0x00002000,
    LIGHTSOURCE = 0x00004000,
    VEHICLE = 0x00008000,
    WALL = 0x00010000,
    WALLNWINDOW = 0x00020000,
    SLIDINGDOOR = 0x00040000,
    DOOR = 0x00080000,

    // a "multi" structure (as opposed to multitiled) is composed of multiple graphics & structures
    MULTI = 0x00100000,
    CAVEWALL = 0x00200000,
    DDOOR_LEFT = 0x00400000,
    DDOOR_RIGHT = 0x00800000,
    NORMAL_ROOF = 0x01000000,
    SLANTED_ROOF = 0x02000000,
    TALL_ROOF = 0x04000000,
    SWITCH = 0x08000000,
    ON_LEFT_WALL = 0x10000000,
    ON_RIGHT_WALL = 0x20000000,
    CORPSE = 0x40000000,
    PERSON = 0x80000000,

    // COMBINATION FLAGS
    ANYFENCE = 0x00000C00,
    ANYDOOR = 0x00CC0000,
    OBSTACLE = 0x00008F00,
    WALLSTUFF = 0x00CF0000,
    BLOCKSMOVES = 0x00208F00,
    TYPE_DEFINED = 0x8FEF8F00,
    ROOF = 0x07000000,
}


[StructLayout(LayoutKind.Explicit, Size = 24)]
public unsafe struct STRUCTURE_FILE_REF
{
    [FieldOffset(00)] public IntPtr pPrev;
    [FieldOffset(02)] public IntPtr pNext;
    [FieldOffset(04)] public AuxObjectData pAuxData;
    [FieldOffset(20)] public RelTileLoc pTileLocData;
    [FieldOffset(00)] public IntPtr pubStructureData;
    [FieldOffset(00)] public DB_STRUCTURE_REF[] pDBStructureRef; // dynamic array
    [FieldOffset(00)] public ushort usNumberOfStructures;
    [FieldOffset(00)] public ushort usNumberOfStructuresStored;
} // 24 bytes

// IMPORTANT THING TO REMEMBER
//
// Although the number of structures and images about which information
// may be stored in a file, the two are stored very differently.
//
// The structure data stored amounts to a sparse array, with no data
// saved for any structures that are not defined.
//
// For image information, however, an array is stored with every entry
// filled regardless of whether there is non-zero data defined for
// that graphic!

// chad: read this exactly off disk thusly, then manually parse the rest using the numbers stored
// below. Trying to avoid unsafe here.

[StructLayout(LayoutKind.Explicit, Size = 16)]
public unsafe struct STRUCTURE_FILE_HEADER
{
    [FieldOffset(00)] public fixed char szId[4];
    [FieldOffset(04)] public ushort usNumberOfStructures;
    [FieldOffset(04)] public ushort usNumberOfImages;
    [FieldOffset(06)] public ushort usNumberOfStructuresStored;
    [FieldOffset(08)] public ushort usStructureDataSize;
    [FieldOffset(09)] public STRUCTURE_FILE_CONTAINS fFlags;
    [FieldOffset(12)] private fixed byte bUnused[3];
    [FieldOffset(15)] public ushort usNumberOfImageTileLocsStored;
} // 16 bytes
