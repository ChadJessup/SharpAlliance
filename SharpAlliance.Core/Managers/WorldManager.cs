using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Managers;

public class WorldManager
{
    private readonly ILogger<WorldManager> logger;
    private readonly SaveLoadMap saveLoadMap;
    private readonly TileCache tileCache;
    private readonly RenderWorld renderWorld;
    private readonly WorldStructures worldStructures;

    public WorldManager(
        ILogger<WorldManager> logger,
        SaveLoadMap saveLoadMap,
        TileCache tileCache,
        RenderWorld renderWorld,
        WorldStructures worldStructures)
    {
        this.logger = logger;
        this.saveLoadMap = saveLoadMap;
        this.tileCache = tileCache;
        this.renderWorld = renderWorld;
        this.worldStructures = worldStructures;
    }

    public static LEVELNODE? FindShadow(int sGridNo, TileIndexes usStructIndex)
    {
        LEVELNODE? pLevelNode;
        TileIndexes usShadowIndex;

        if (usStructIndex < TileIndexes.FIRSTOSTRUCT1 || usStructIndex >= TileIndexes.FIRSTSHADOW1)
        {
            return null;
        }

        usShadowIndex = usStructIndex - TileIndexes.FIRSTOSTRUCT1 + TileIndexes.FIRSTSHADOW1;

        pLevelNode = gpWorldLevelData[sGridNo].pShadowHead;
        while (pLevelNode != null)
        {
            if (pLevelNode.usIndex == usShadowIndex)
            {
                break;
            }
            pLevelNode = pLevelNode.pNext;
        }
        return pLevelNode;

    }

    public static bool DeepWater(int sGridNo)
    {
        MAP_ELEMENT pMapElement;

        pMapElement = gpWorldLevelData[sGridNo];
        if (pMapElement.ubTerrainID == TerrainTypeDefines.DEEP_WATER)
        {
            // check for a bridge!  otherwise...
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool WaterTooDeepForAttacks(int sGridNo)
    {
        return DeepWater(sGridNo);
    }


    public static bool RemoveAllStructsOfTypeRange(int iMapIndex, TileTypeDefines fStartType, TileTypeDefines fEndType)
    {
        LEVELNODE? pStruct = null;
        LEVELNODE? pOldStruct = null;
        TileIndexes usIndex;
        bool fRetVal = false;

        pStruct = gpWorldLevelData[iMapIndex].pStructHead;

        // Look through all structs and Search for type

        while (pStruct != null)
        {

            if (pStruct.usIndex != TileIndexes.NO_TILE)
            {

                TileDefine.GetTileType(pStruct.usIndex, out TileTypeDefines fTileType);

                // Advance to next
                pOldStruct = pStruct;
                pStruct = pStruct.pNext;

                if (fTileType >= fStartType && fTileType <= fEndType)
                {
                    usIndex = pOldStruct.usIndex;

                    // Remove Item
                    if (usIndex < TileIndexes.NUMBEROFTILES)
                    {
                        RemoveStruct(iMapIndex, pOldStruct.usIndex);
                        fRetVal = true;
                        if (!GridNoIndoors(iMapIndex) && gTileDatabase[usIndex].uiFlags.HasFlag(TileCategory.HAS_SHADOW_BUDDY)
                            && gTileDatabase[usIndex].sBuddyNum != (TileIndexes)(-1))
                        {
                            RemoveShadow(iMapIndex, gTileDatabase[usIndex].sBuddyNum);
                        }
                    }
                }
            }
        }
        return fRetVal;
    }

    public static bool AddObjectToHead(int iMapIndex, TileIndexes usIndex)
    {
        LEVELNODE? pObject = null;

        pObject = gpWorldLevelData[iMapIndex].pObjectHead;

        CHECKF(CreateLevelNode(out LEVELNODE? pNextObject) != false);

        pNextObject.pNext = pObject;
        pNextObject.usIndex = usIndex;

        // Set head
        gpWorldLevelData[iMapIndex].pObjectHead = pNextObject;

        //CheckForAndAddTileCacheStructInfo( pNextObject, (INT16)iMapIndex, usIndex );

        // If it's NOT the first head
        RenderWorld.ResetSpecificLayerOptimizing(TILES_DYNAMIC.OBJECTS);

        //Add the object to the map temp file, if we have to
        SaveLoadMap.AddObjectToMapTempFile(iMapIndex, usIndex);

        return true;
    }

    public static bool TypeRangeExistsInObjectLayer(int iMapIndex, TileTypeDefines fStartType, TileTypeDefines fEndType, out TileIndexes pusObjectIndex)
    {
        LEVELNODE? pObject = null;
        LEVELNODE? pOldObject = null;

        pObject = gpWorldLevelData[iMapIndex].pObjectHead;

        // Look through all objects and Search for type

        while (pObject != null)
        {
            // Advance to next
            pOldObject = pObject;
            pObject = pObject.pNext;

            if (pOldObject.usIndex != TileIndexes.NO_TILE && pOldObject.usIndex < NUMBEROFTILES)
            {
                TileDefine.GetTileType(pOldObject.usIndex, out TileTypeDefines fTileType);

                if (fTileType >= fStartType && fTileType <= fEndType)
                {
                    pusObjectIndex = pOldObject.usIndex;
                    return true;
                }

            }

        }

        // Could not find it, return false

        pusObjectIndex = TileIndexes.NO_TILE;
        return false;
    }

    public static bool AddStructToHead(int iMapIndex, TileIndexes usIndex)
    {
        LEVELNODE? pStruct = null;
        DB_STRUCTURE? pDBStructure;

        pStruct = gpWorldLevelData[iMapIndex].pStructHead;

        CHECKF(CreateLevelNode(out LEVELNODE? pNextStruct) != false);

        if (usIndex < NUMBEROFTILES)
        {
            //            if (gTileDatabase[usIndex].pDBStructureRef != null)
            {
                //                if (WorldStructures.AddStructureToWorld(iMapIndex, 0, gTileDatabase[usIndex].pDBStructureRef, pNextStruct) == false)
                //                {
                //                    MemFree(pNextStruct);
                //                    guiLevelNodes--;
                //                    return (false);
                //                }
            }
        }

        pNextStruct.pNext = pStruct;
        pNextStruct.usIndex = usIndex;

        // Set head
        gpWorldLevelData[iMapIndex].pStructHead = pNextStruct;

        WorldManager.SetWorldFlagsFromNewNode(iMapIndex, pNextStruct.usIndex);

        if (usIndex < NUMBEROFTILES)
        {
            // Check flags for tiledat and set a shadow if we have a buddy
            if (!GridNoIndoors(iMapIndex) && gTileDatabase[usIndex].uiFlags.HasFlag(TileCategory.HAS_SHADOW_BUDDY) && gTileDatabase[usIndex].sBuddyNum != TileIndexes.UNSET)
            {
                AddShadowToHead(iMapIndex, gTileDatabase[usIndex].sBuddyNum);
                gpWorldLevelData[iMapIndex].pShadowHead.uiFlags |= LEVELNODEFLAGS.BUDDYSHADOW;
            }

            //Check for special flag to stop burn-through on same-tile structs...
            //            if (gTileDatabase[usIndex].pDBStructureRef != null)
            //            {
            //                pDBStructure = gTileDatabase[usIndex].pDBStructureRef.pDBStructure;
            //
            //                // Default to off....
            //                gpWorldLevelData[iMapIndex].ubExtFlags[0] &= (~MAPELEMENTFLAGS_EXT.NOBURN_STRUCT);
            //
            //                // If we are NOT a wall and NOT multi-tiles, set mapelement flag...
            //                if (WorldStructures.FindStructure(iMapIndex, STRUCTUREFLAGS.WALLSTUFF) is null && pDBStructure.ubNumberOfTiles == 1)
            //                {
            //                    // Set flag...
            //                    gpWorldLevelData[iMapIndex].ubExtFlags[0] |= MAPELEMENTFLAGS_EXT.NOBURN_STRUCT;
            //                }
            //            }
        }

        //Add the structure the maps temp file
        //        AddStructToMapTempFile(iMapIndex, usIndex);

        //CheckForAndAddTileCacheStructInfo( pNextStruct, (INT16)iMapIndex, usIndex );

        RenderWorld.ResetSpecificLayerOptimizing(TILES_DYNAMIC.STRUCTURES);
        return true;
    }

    public static bool AddShadowToHead(int iMapIndex, TileIndexes usIndex)
    {
        LEVELNODE? pShadow;

        pShadow = gpWorldLevelData[iMapIndex].pShadowHead;

        // Allocate head
        CHECKF(CreateLevelNode(out LEVELNODE? pNextShadow) != false);
        pNextShadow.pNext = pShadow;
        pNextShadow.usIndex = usIndex;

        // Set head
        gpWorldLevelData[iMapIndex].pShadowHead = pNextShadow;

        RenderWorld.ResetSpecificLayerOptimizing(TILES_DYNAMIC.SHADOWS);
        return true;
    }


    public static bool RemoveShadow(int iMapIndex, TileIndexes usIndex)
    {
        LEVELNODE? pShadow = null;
        LEVELNODE? pOldShadow = null;

        pShadow = gpWorldLevelData[iMapIndex].pShadowHead;

        // Look through all shadows and remove index if found

        while (pShadow != null)
        {
            if (pShadow.usIndex == usIndex)
            {
                // OK, set links
                // Check for head or tail
                if (pOldShadow == null)
                {
                    // It's the head
                    gpWorldLevelData[iMapIndex].pShadowHead = pShadow.pNext;
                }
                else
                {
                    pOldShadow.pNext = pShadow.pNext;
                }

                // Delete memory assosiated with item
                MemFree(pShadow);
                guiLevelNodes--;

                return true;
            }

            pOldShadow = pShadow;
            pShadow = pShadow.pNext;

        }

        // Could not find it, return false

        return false;
    }

    private static void SetWorldFlagsFromNewNode(int iMapIndex, TileIndexes usIndex)
    {
    }

    // When adding, put in order such that it's drawn before any walls of a
    // lesser orientation value
    public static bool AddWallToStructLayer(int iMapIndex, TileIndexes usIndex, bool fReplace)
    {
        LEVELNODE? pStruct = null;
        bool fInsertFound = false;
        bool fRoofFound = false;
        int ubRoofLevel = 0;
        int ubLevel = 0;

        pStruct = gpWorldLevelData[iMapIndex].pStructHead;


        // Get orientation of peice we want to add
        TileDefine.GetWallOrientation(usIndex, out WallOrientation usWallOrientation);

        // Look through all objects and Search for orientation
        while (pStruct != null)
        {

            TileDefine.GetWallOrientation(pStruct.usIndex, out WallOrientation usCheckWallOrient);
            //OLD CASE 
            //if ( usCheckWallOrient > usWallOrientation )
            //Kris:
            //New case -- If placing a new wall which is at right angles to the current wall, then
            //we insert it.
            if (usCheckWallOrient > usWallOrientation)
            {
                if ((usWallOrientation == WallOrientation.INSIDE_TOP_RIGHT || usWallOrientation == WallOrientation.OUTSIDE_TOP_RIGHT)
                    && (usCheckWallOrient == WallOrientation.INSIDE_TOP_LEFT || usCheckWallOrient == WallOrientation.OUTSIDE_TOP_LEFT)
                    || (usWallOrientation == WallOrientation.INSIDE_TOP_LEFT || usWallOrientation == WallOrientation.OUTSIDE_TOP_LEFT)
                    && (usCheckWallOrient == WallOrientation.INSIDE_TOP_RIGHT || usCheckWallOrient == WallOrientation.OUTSIDE_TOP_RIGHT))
                {
                    fInsertFound = true;
                }
            }

            TileDefine.GetTileType(pStruct.usIndex, out TileTypeDefines uiCheckType);

            //		if ( uiCheckType >= FIRSTFLOOR && uiCheckType <= LASTFLOOR )
            if (uiCheckType >= TileTypeDefines.FIRSTROOF && uiCheckType <= LASTROOF)
            {
                fRoofFound = true;
                ubRoofLevel = ubLevel;
            }

            //OLD CHECK
            // Check if it's the same orientation
            //if ( usCheckWallOrient == usWallOrientation )
            //Kris:
            //New check -- we want to check for walls being parallel to each other.  If so, then
            //we we want to replace it.  This is because of an existing problem with say, INSIDE_TOP_LEFT
            //and OUTSIDE_TOP_LEFT walls coexisting.
            if ((usWallOrientation == WallOrientation.INSIDE_TOP_RIGHT || usWallOrientation == WallOrientation.OUTSIDE_TOP_RIGHT)
                && (usCheckWallOrient == WallOrientation.INSIDE_TOP_RIGHT || usCheckWallOrient == WallOrientation.OUTSIDE_TOP_RIGHT)
                || (usWallOrientation == WallOrientation.INSIDE_TOP_LEFT || usWallOrientation == WallOrientation.OUTSIDE_TOP_LEFT)
                && (usCheckWallOrient == WallOrientation.INSIDE_TOP_LEFT || usCheckWallOrient == WallOrientation.OUTSIDE_TOP_LEFT))
            {
                // Same, if replace, replace here
                if (fReplace)
                {
                    return false;//ReplaceStructIndex(iMapIndex, pStruct.usIndex, usIndex));
                }
                else
                {
                    return false;
                }
            }

            // Advance to next
            pStruct = pStruct.pNext;

            ubLevel++;

        }

        // Check if we found an insert position, otherwise set to head
        if (fInsertFound)
        {
            // Insert struct at head
            AddStructToHead(iMapIndex, usIndex);
        }
        else
        {
            // Make sure it's ALWAYS after the roof ( if any )
            if (fRoofFound)
            {
                WorldManager.InsertStructIndex(iMapIndex, usIndex, ubRoofLevel);
            }
            else
            {
                //                AddStructToTail(iMapIndex, usIndex);
            }
        }

        RenderWorld.ResetSpecificLayerOptimizing(TILES_DYNAMIC.STRUCTURES);
        // Could not find it, return false
        return true;
    }

    public static bool InsertStructIndex(int iMapIndex, TileIndexes usIndex, int ubLevel)
    {
        LEVELNODE? pStruct = null;
        int level = 0;
        bool CanInsert = false;

        pStruct = gpWorldLevelData[iMapIndex].pStructHead;

        // If we want to insert at head;
        if (ubLevel == 0)
        {
            return AddStructToHead(iMapIndex, usIndex);
        }


        // Allocate memory for new item
        CHECKF(CreateLevelNode(out LEVELNODE? pNextStruct) != false);

        pNextStruct.usIndex = usIndex;

        // Move to index before insertion
        while (pStruct != null)
        {
            if (level == (ubLevel - 1))
            {
                CanInsert = true;
                break;
            }

            pStruct = pStruct.pNext;
            level++;

        }

        // Check if level has been macthed
        if (!CanInsert)
        {
            MemFree(pNextStruct);
            guiLevelNodes--;
            return false;
        }

        if (usIndex < NUMBEROFTILES)
        {
            {
                if (WorldStructures.AddStructureToWorld(iMapIndex, 0, gTileDatabase[usIndex].pDBStructureRef, pNextStruct) == false)
                {
                    MemFree(pNextStruct);
                    guiLevelNodes--;
                    return false;
                }
            }
        }

        // Set links, according to position!
        pNextStruct.pNext = pStruct.pNext;
        pStruct.pNext = pNextStruct;

        //CheckForAndAddTileCacheStructInfo( pNextStruct, (INT16)iMapIndex, usIndex );

        RenderWorld.ResetSpecificLayerOptimizing(TILES_DYNAMIC.STRUCTURES);
        return true;
    }

    public static bool RemoveStructFromLevelNode(int iMapIndex, LEVELNODE? pNode)
    {
        LEVELNODE? pStruct = null;
        LEVELNODE? pOldStruct = null;
        TileIndexes usIndex;

        usIndex = pNode.usIndex;

        pStruct = gpWorldLevelData[iMapIndex].pStructHead;

        // Look through all structs and remove index if found

        while (pStruct != null)
        {
            if (pStruct == pNode)
            {
                // OK, set links
                // Check for head or tail
                if (pOldStruct == null)
                {
                    // It's the head
                    gpWorldLevelData[iMapIndex].pStructHead = pStruct.pNext;
                }
                else
                {
                    pOldStruct.pNext = pStruct.pNext;
                }

                // Delete memory assosiated with item
                WorldStructures.DeleteStructureFromWorld(pStruct.pStructureData);

                //If we have to, make sure to remove this node when we reload the map from a saved game
                //                RemoveStructFromMapTempFile(iMapIndex, usIndex);

                if (pNode.usIndex < TileIndexes.NUMBEROFTILES)
                {
                    // Check flags for tiledat and set a shadow if we have a buddy
                    if (!GridNoIndoors(iMapIndex) && gTileDatabase[usIndex].uiFlags.HasFlag(TileCategory.HAS_SHADOW_BUDDY)
                        && gTileDatabase[usIndex].sBuddyNum != TileIndexes.UNSET)
                    {
                        RemoveShadow(iMapIndex, gTileDatabase[usIndex].sBuddyNum);
                    }
                }
                MemFree(pStruct);
                guiLevelNodes--;

                return true;
            }

            pOldStruct = pStruct;
            pStruct = pStruct.pNext;

        }

        // Could not find it, return false
        //        RemoveWorldFlagsFromNewNode(iMapIndex, usIndex);

        return false;

    }

    public static bool FloorAtGridNo(int iMapIndex)
    {
        LEVELNODE? pLand;
        pLand = gpWorldLevelData[iMapIndex].pLandHead;
        // Look through all objects and Search for type
        while (pLand is not null)
        {
            if (pLand.usIndex != TileIndexes.NO_TILE)
            {
                TileDefine.GetTileType(pLand.usIndex, out TileTypeDefines uiTileType);
                if (uiTileType >= TileTypeDefines.FIRSTFLOOR && uiTileType <= LASTFLOOR)
                {
                    return true;
                }

                pLand = pLand.pNext;
            }
        }

        return false;
    }

    public static bool GridNoIndoors(int iMapIndex)
    {
        if (gfBasement || gfCaves)
        {
            return true;
        }

        if (FloorAtGridNo(iMapIndex))
        {
            return true;
        }

        return false;
    }

    public static bool RemoveStruct(int iMapIndex, TileIndexes usIndex)
    {
        LEVELNODE? pStruct = null;
        LEVELNODE? pOldStruct = null;

        pStruct = gpWorldLevelData[iMapIndex].pStructHead;

        // Look through all structs and remove index if found

        while (pStruct != null)
        {
            if (pStruct.usIndex == usIndex)
            {
                // OK, set links
                // Check for head or tail
                if (pOldStruct == null)
                {
                    // It's the head
                    gpWorldLevelData[iMapIndex].pStructHead = pStruct.pNext;
                }
                else
                {
                    pOldStruct.pNext = pStruct.pNext;
                }

                //Check for special flag to stop burn-through on same-tile structs...
                if (pStruct.pStructureData != null)
                {
                    // If we are NOT a wall and NOT multi-tiles, set mapelement flag...
                    //if ( !( pStruct.pStructureData.fFlags & STRUCTURE_WALLSTUFF ) && pStruct.pStructureData.pDBStructureRef.pDBStructure.ubNumberOfTiles == 1 )
                    //{
                    // UNSet flag...
                    //	gpWorldLevelData[ iMapIndex ].ubExtFlags[0] &= ( ~MAPELEMENTFLAGS.EXT_NOBURN_STRUCT );
                    //}
                }

                // Delete memory assosiated with item
                WorldStructures.DeleteStructureFromWorld(pStruct.pStructureData);

                //If we have to, make sure to remove this node when we reload the map from a saved game
                //                RemoveStructFromMapTempFile(iMapIndex, usIndex);

                if (usIndex < TileIndexes.NUMBEROFTILES)
                {
                    // Check flags for tiledat and set a shadow if we have a buddy
                    if (!GridNoIndoors(iMapIndex) && gTileDatabase[usIndex].uiFlags.HasFlag(TileCategory.HAS_SHADOW_BUDDY)
                        && gTileDatabase[usIndex].sBuddyNum != TileIndexes.UNSET)
                    {
                        RemoveShadow(iMapIndex, gTileDatabase[usIndex].sBuddyNum);
                    }
                }
                MemFree(pStruct);
                guiLevelNodes--;

                return true;
            }

            pOldStruct = pStruct;
            pStruct = pStruct.pNext;

        }

        // Could not find it, return false
        //        RemoveWorldFlagsFromNewNode(iMapIndex, usIndex);

        return false;
    }

    public static TerrainTypeDefines GetTerrainType(int sGridNo)
    {
        return gpWorldLevelData[sGridNo].ubTerrainID;
        /*
            LEVELNODE	*pNode;


            // Check if we have anything in object layer which has a terrain modifier
            pNode = gpWorldLevelData[ sGridNo ].pObjectHead;

            if ( pNode != null )
            {
                if ( gTileDatabase[ pNode.usIndex ].ubTerrainID != NO_TERRAIN )
                {
                    return( gTileDatabase[ pNode.usIndex ].ubTerrainID );
                }
            }

            // Now try terrain!
            pNode = gpWorldLevelData[ sGridNo ].pLandHead;

            return( gTileDatabase[ pNode.usIndex ].ubTerrainID );
        */
    }

    public static bool RemoveAllOnRoofsOfTypeRange(int iMapIndex, TileTypeDefines fStartType, TileTypeDefines fEndType)
    {
        LEVELNODE? pOnRoof = null;
        LEVELNODE? pOldOnRoof = null;
        bool fRetVal = false;

        pOnRoof = Globals.gpWorldLevelData[iMapIndex].pOnRoofHead;

        // Look through all OnRoofs and Search for type

        while (pOnRoof != null)
        {
            if (pOnRoof.usIndex != TileIndexes.NO_TILE)
            {
                TileDefine.GetTileType(pOnRoof.usIndex, out TileTypeDefines fTileType);

                // Advance to next
                pOldOnRoof = pOnRoof;
                pOnRoof = pOnRoof.pNext;

                if (fTileType >= fStartType && fTileType <= fEndType)
                {
                    // Remove Item
                    RemoveOnRoof(iMapIndex, pOldOnRoof.usIndex);
                    fRetVal = true;
                }
            }
        }

        return fRetVal;
    }

    // First for object layer
    // #################################################################

    public static LEVELNODE? AddObjectToTail(int iMapIndex, TileIndexes usIndex)
    {
        LEVELNODE? pObject = null;
        LEVELNODE? pNextObject = null;

        pObject = Globals.gpWorldLevelData[iMapIndex].pObjectHead;

        // If we're at the head, set here
        if (pObject == null)
        {
            if (!CreateLevelNode(out pNextObject))
            {
                return null;
            }

            pNextObject.usIndex = usIndex;

            Globals.gpWorldLevelData[iMapIndex].pObjectHead = pNextObject;
        }
        else
        {
            while (pObject != null)
            {

                if (pObject.pNext == null)
                {
                    if (!CreateLevelNode(out pNextObject))
                    {
                        return null;
                    }

                    pObject.pNext = pNextObject;

                    pNextObject.pNext = null;
                    pNextObject.usIndex = usIndex;

                    break;
                }

                pObject = pObject.pNext;

            }

        }

        //CheckForAndAddTileCacheStructInfo( pNextObject, (int)iMapIndex, usIndex );

        RenderWorld.ResetSpecificLayerOptimizing(TILES_DYNAMIC.OBJECTS);
        return pNextObject;

    }

    // OnRoof layer
    // #################################################################

    public static LEVELNODE? AddOnRoofToTail(int iMapIndex, TileIndexes usIndex)
    {
        LEVELNODE? pOnRoof = null;
        LEVELNODE? pNextOnRoof = null;

        pOnRoof = Globals.gpWorldLevelData[iMapIndex].pOnRoofHead;

        // If we're at the head, set here
        if (pOnRoof == null)
        {
            if (!CreateLevelNode(out pOnRoof))
            {
                return null;
            }

            if (usIndex < TileIndexes.NUMBEROFTILES)
            {
                if (WorldStructures.AddStructureToWorld(iMapIndex, 1, Globals.gTileDatabase[usIndex].pDBStructureRef, pOnRoof) == false)
                {
                    // MemFree(pOnRoof);
                    Globals.guiLevelNodes--;
                    return null;
                }
            }
            pOnRoof.usIndex = usIndex;

            Globals.gpWorldLevelData[iMapIndex].pOnRoofHead = pOnRoof;

            RenderWorld.ResetSpecificLayerOptimizing(TILES_DYNAMIC.ONROOF);
            return pOnRoof;

        }
        else
        {
            while (pOnRoof != null)
            {

                if (pOnRoof.pNext == null)
                {
                    if (!CreateLevelNode(out pNextOnRoof))
                    {
                        return null;
                    }

                    if (usIndex < TileIndexes.NUMBEROFTILES)
                    {
                        if (WorldStructures.AddStructureToWorld(iMapIndex, 1, Globals.gTileDatabase[usIndex].pDBStructureRef, pNextOnRoof) == false)
                        {
                            // MemFree(pNextOnRoof);
                            Globals.guiLevelNodes--;
                            return null;
                        }
                    }

                    pOnRoof.pNext = pNextOnRoof;

                    pNextOnRoof.pNext = null;
                    pNextOnRoof.usIndex = usIndex;
                    break;
                }

                pOnRoof = pOnRoof.pNext;

            }

        }

        RenderWorld.ResetSpecificLayerOptimizing(TILES_DYNAMIC.ONROOF);
        return pNextOnRoof;
    }

    public static bool RemoveAllTopmostsOfTypeRange(int iMapIndex, TileTypeDefines fStartType, TileTypeDefines fEndType)
    {
        LEVELNODE? pTopmost = null;
        LEVELNODE? pOldTopmost = null;
        bool fRetVal = false;

        pTopmost = Globals.gpWorldLevelData[iMapIndex].pTopmostHead;

        // Look through all topmosts and Search for type

        while (pTopmost != null)
        {
            // Advance to next
            pOldTopmost = pTopmost;
            pTopmost = pTopmost.pNext;

            if (pOldTopmost.usIndex != TileIndexes.NO_TILE && pOldTopmost.usIndex < TileIndexes.NUMBEROFTILES)
            {
                TileDefine.GetTileType(pOldTopmost.usIndex, out TileTypeDefines fTileType);

                if (fTileType >= fStartType && fTileType <= fEndType)
                {
                    // Remove Item
                    RemoveTopmost(iMapIndex, pOldTopmost.usIndex);
                    fRetVal = true;
                }
            }
        }

        return fRetVal;
    }

    public static bool RemoveOnRoof(int iMapIndex, TileIndexes usIndex)
    {
        LEVELNODE? pOnRoof = null;
        LEVELNODE? pOldOnRoof = null;

        pOnRoof = Globals.gpWorldLevelData[iMapIndex].pOnRoofHead;

        // Look through all OnRoofs and remove index if found

        while (pOnRoof is not null)
        {
            if (pOnRoof.usIndex == usIndex)
            {
                // OK, set links
                // Check for head or tail
                if (pOldOnRoof == null)
                {
                    // It's the head
                    Globals.gpWorldLevelData[iMapIndex].pOnRoofHead = pOnRoof.pNext;
                }
                else
                {
                    pOldOnRoof.pNext = pOnRoof.pNext;
                }

                // REMOVE ONROOF!
                pOnRoof = null;
                Globals.guiLevelNodes--;

                return true;
            }

            pOldOnRoof = pOnRoof;
            pOnRoof = pOnRoof.pNext;

        }

        // Could not find it, return false

        return false;
    }

    public static bool RemoveAllObjectsOfTypeRange(int iMapIndex, TileTypeDefines fStartType, TileTypeDefines fEndType)
    {
        LEVELNODE? pObject = null;
        LEVELNODE? pOldObject = null;
        bool fRetVal = false;

        pObject = Globals.gpWorldLevelData[iMapIndex].pObjectHead;

        // Look through all objects and Search for type

        while (pObject != null)
        {
            // Advance to next
            pOldObject = pObject;
            pObject = pObject.pNext;

            if (pOldObject.usIndex != TileIndexes.NO_TILE && pOldObject.usIndex < TileIndexes.NUMBEROFTILES)
            {

                TileDefine.GetTileType(pOldObject.usIndex, out TileTypeDefines fTileType);

                if (fTileType >= fStartType && fTileType <= fEndType)
                {
                    // Remove Item
                    RemoveObject(iMapIndex, pOldObject.usIndex);
                    fRetVal = true;
                }

            }

        }
        return fRetVal;
    }

    public static bool RemoveObject(int iMapIndex, TileIndexes usIndex)
    {
        LEVELNODE? pObject = null;
        LEVELNODE? pOldObject = null;

        pObject = Globals.gpWorldLevelData[iMapIndex].pObjectHead;

        // Look through all objects and remove index if found

        while (pObject is not null)
        {
            if (pObject.usIndex == usIndex)
            {
                // OK, set links
                // Check for head or tail
                if (pOldObject == null)
                {
                    // It's the head
                    Globals.gpWorldLevelData[iMapIndex].pObjectHead = pObject.pNext;
                }
                else
                {
                    pOldObject.pNext = pObject.pNext;
                }

                TileCache.CheckForAndDeleteTileCacheStructInfo(pObject, usIndex);

                // Delete memory assosiated with item
                // MemFree(pObject);
                Globals.guiLevelNodes--;

                //Add the index to the maps temp file so we can remove it after reloading the map
                SaveLoadMap.AddRemoveObjectToMapTempFile(iMapIndex, usIndex);

                return true;
            }

            pOldObject = pObject;
            pObject = pObject.pNext;
        }

        // Could not find it, return false
        return false;
    }

    public static int WhoIsThere2(int sGridNo, int bLevel)
    {
        STRUCTURE? pStructure;

        if (!IsometricUtils.GridNoOnVisibleWorldTile(sGridNo))
        {
            return Globals.NOBODY;
        }

        if (Globals.gpWorldLevelData[sGridNo].pStructureHead != null)
        {

            pStructure = Globals.gpWorldLevelData[sGridNo].pStructureHead;

            while (pStructure is not null)
            {
                // person must either have their pSoldier.sGridNo here or be non-passable
                if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.PERSON)
                    && (!pStructure.fFlags.HasFlag(STRUCTUREFLAGS.PASSABLE)
                    || Globals.MercPtrs[pStructure.usStructureID].sGridNo == sGridNo))
                {
                    if ((bLevel == 0 && pStructure.sCubeOffset == 0) || (bLevel > 0 && pStructure.sCubeOffset > 0))
                    {
                        // found a person, on the right level!
                        // structure ID and merc ID are identical for merc structures
                        return pStructure.usStructureID;
                    }
                }

                pStructure = pStructure.pNext;
            }
        }

        return Globals.NOBODY;
    }

    public static bool RemoveTopmost(int iMapIndex, TileIndexes usIndex)
    {
        LEVELNODE? pTopmost = null;
        LEVELNODE? pOldTopmost = null;

        pTopmost = Globals.gpWorldLevelData[iMapIndex].pTopmostHead;

        // Look through all topmosts and remove index if found

        while (pTopmost is not null)
        {
            if (pTopmost.usIndex == usIndex)
            {
                // OK, set links
                // Check for head or tail
                if (pOldTopmost == null)
                {
                    // It's the head
                    Globals.gpWorldLevelData[iMapIndex].pTopmostHead = pTopmost.pNext;
                }
                else
                {
                    pOldTopmost.pNext = pTopmost.pNext;
                }

                // Delete memory assosiated with item
                //MemFree(pTopmost);
                Globals.guiLevelNodes--;

                return true;
            }

            pOldTopmost = pTopmost;
            pTopmost = pTopmost.pNext;
        }

        // Could not find it, return false
        return false;
    }

    public bool AddTopmostToHead(int iMapIndex, TileIndexes usIndex)
    {
        LEVELNODE? pTopmost = Globals.gpWorldLevelData[iMapIndex].pTopmostHead;

        // Allocate head
        //CHECKF(
        if (CreateLevelNode(out LEVELNODE? pNextTopmost) == false)
        {

        }

        pNextTopmost.pNext = pTopmost;
        pNextTopmost.usIndex = usIndex;

        // Set head
        Globals.gpWorldLevelData[iMapIndex].pTopmostHead = pNextTopmost;

        RenderWorld.ResetSpecificLayerOptimizing(TILES_DYNAMIC.TOPMOST);

        return true;
    }

    public void AddUIElem(int iMapIndex, TileIndexes usIndex, int sRelativeX, int sRelativeY, out LEVELNODE ppNewNode)
        => this.AddUIElem(iMapIndex, usIndex, sRelativeX, sRelativeY, out ppNewNode);

    public bool AddUIElem(int iMapIndex, TileIndexes usIndex, int sRelativeX, MAP_ROW sRelativeY, out LEVELNODE ppNewNode)
    {
        LEVELNODE? pTopmost = this.AddTopmostToTail(iMapIndex, usIndex);

        Debug.Assert(pTopmost != null);

        // Set flags
        pTopmost.uiFlags |= LEVELNODEFLAGS.USERELPOS;
        pTopmost.sRelativeX = sRelativeX;
        pTopmost.sRelativeY = sRelativeY;

        RenderWorld.ResetSpecificLayerOptimizing(TILES_DYNAMIC.TOPMOST);

        ppNewNode = pTopmost;

        return true;
    }

    public void RemoveUIElem(int iMapIndex, TileIndexes usIndex)
    {
        RemoveTopmost(iMapIndex, usIndex);
    }

    public LEVELNODE? AddTopmostToTail(int iMapIndex, TileIndexes usIndex)
    {
        LEVELNODE? pNextTopmost = null;

        LEVELNODE? pTopmost = Globals.gpWorldLevelData[iMapIndex].pTopmostHead;

        // If we're at the head, set here
        if (pTopmost == null)
        {
            if (!CreateLevelNode(out pNextTopmost))
            {
                return null;
            }

            pNextTopmost.usIndex = usIndex;

            Globals.gpWorldLevelData[iMapIndex].pTopmostHead = pNextTopmost;
        }
        else
        {
            while (pTopmost is not null)
            {
                if (pTopmost.pNext is null)
                {
                    if (!CreateLevelNode(out pNextTopmost))
                    {
                        return null;
                    }

                    pTopmost.pNext = pNextTopmost;
                    pNextTopmost.pNext = null;
                    pNextTopmost.usIndex = usIndex;

                    break;
                }

                pTopmost = pTopmost.pNext;
            }
        }

        RenderWorld.ResetSpecificLayerOptimizing(TILES_DYNAMIC.TOPMOST);
        return pNextTopmost;
    }

    public bool AddOnRoofToHead(int iMapIndex, TileIndexes usIndex)
    {
        LEVELNODE? pOnRoof = null;

        pOnRoof = Globals.gpWorldLevelData[iMapIndex].pOnRoofHead;

        if (CreateLevelNode(out LEVELNODE? pNextOnRoof) == false)
        {

        }

        if (usIndex < TileIndexes.NUMBEROFTILES)
        {
            {
                if (WorldStructures.AddStructureToWorld((short)iMapIndex, 1, Globals.gTileDatabase[usIndex].pDBStructureRef, pNextOnRoof) == false)
                {
                    // MemFree(pNextOnRoof);
                    Globals.guiLevelNodes--;
                    return false;
                }
            }
        }

        pNextOnRoof.pNext = pOnRoof;
        pNextOnRoof.usIndex = usIndex;


        // Set head
        Globals.gpWorldLevelData[iMapIndex].pOnRoofHead = pNextOnRoof;

        RenderWorld.ResetSpecificLayerOptimizing(TILES_DYNAMIC.ONROOF);
        return true;
    }

    public static bool CreateLevelNode(out LEVELNODE ppNode)
    {
        ppNode = new LEVELNODE
        {
            ubShadeLevel = LightingSystem.LightGetAmbient(),
            ubNaturalShadeLevel = LightingSystem.LightGetAmbient(),
            pSoldier = null,
            pNext = null,
            sRelativeX = 0,
            sRelativeY = 0,
        };

        Globals.guiLevelNodes++;

        return true;
    }

    public bool IsRoofVisible(int sMapPos)
    {
        STRUCTURE? pStructure;

        if (!gfBasement)
        {
            pStructure = WorldStructures.FindStructure(sMapPos, STRUCTUREFLAGS.ROOF);

            if (pStructure != null)
            {
                if (!Globals.gpWorldLevelData[sMapPos].uiFlags.HasFlag(MAPELEMENTFLAGS.REVEALED))
                {
                    return true;
                }
            }
        }
        else
        {
            //if ( InARoom( sMapPos, &ubRoom ) )
            {
                //if ( !( gpWorldLevelData[ sMapPos ].uiFlags.HasFlag(MAPELEMENTFLAGS.REVEALED )) )
                {
                    return true;
                }
            }
        }

        return false;
    }

    internal void SetTreeTopStateForMap()
    {
        if (!gGameSettings[TOPTION.TOGGLE_TREE_TOPS])
        {
            WorldHideTrees();
            gTacticalStatus.uiFlags |= TacticalEngineStatus.NOHIDE_REDUNDENCY;
        }
        else
        {
            WorldShowTrees();
            gTacticalStatus.uiFlags &= ~TacticalEngineStatus.NOHIDE_REDUNDENCY;
        }

        // FOR THE NEXT RENDER LOOP, RE-EVALUATE REDUNDENT TILES
        RenderWorld.InvalidateWorldRedundency();
    }

    private void WorldShowTrees()
    {
        throw new NotImplementedException();
    }

    private void WorldHideTrees()
    {
        throw new NotImplementedException();
    }
}
