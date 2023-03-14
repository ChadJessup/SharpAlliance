using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems;
using static System.Runtime.InteropServices.JavaScript.JSType;
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

    public static bool RemoveAllStructsOfTypeRange(int iMapIndex, TileTypeDefines fStartType, TileTypeDefines fEndType)
    {
        LEVELNODE? pStruct = null;
        LEVELNODE? pOldStruct = null;
        TileTypeDefines fTileType;
        TileDefines usIndex;
        bool fRetVal = false;

        pStruct = gpWorldLevelData[iMapIndex].pStructHead;

        // Look through all structs and Search for type

        while (pStruct != null)
        {

            if (pStruct.usIndex != TileDefines.NO_TILE)
            {

                TileDefine.GetTileType(pStruct.usIndex, out fTileType);

                // Advance to next
                pOldStruct = pStruct;
                pStruct = pStruct.pNext;

                if (fTileType >= fStartType && fTileType <= fEndType)
                {
                    usIndex = pOldStruct.usIndex;

                    // Remove Item
                    if (usIndex < TileDefines.NUMBEROFTILES)
                    {
                        RemoveStruct(iMapIndex, pOldStruct.usIndex);
                        fRetVal = true;
                        if (!GridNoIndoors(iMapIndex) && gTileDatabase[usIndex].uiFlags.HasFlag(TileCategory.HAS_SHADOW_BUDDY)
                            && gTileDatabase[usIndex].sBuddyNum != -1)
                        {
                            RemoveShadow(iMapIndex, gTileDatabase[usIndex].sBuddyNum);
                        }
                    }
                }
            }
        }

        return fRetVal;
    }

    public static bool RemoveStructFromLevelNode(int iMapIndex, LEVELNODE? pNode)
    {
        LEVELNODE? pStruct = null;
        LEVELNODE? pOldStruct = null;
        TileDefines usIndex;

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
                DeleteStructureFromWorld(pStruct.pStructureData);

                //If we have to, make sure to remove this node when we reload the map from a saved game
                RemoveStructFromMapTempFile(iMapIndex, usIndex);

                if (pNode.usIndex < TileDefines.NUMBEROFTILES)
                {
                    // Check flags for tiledat and set a shadow if we have a buddy
                    if (!GridNoIndoors(iMapIndex) && gTileDatabase[usIndex].uiFlags.HasFlag(TileCategory.HAS_SHADOW_BUDDY)
                        && gTileDatabase[usIndex].sBuddyNum != -1)
                    {
                        RemoveShadow(iMapIndex, gTileDatabase[usIndex].sBuddyNum);
                    }
                }
                MemFree(pStruct);
                guiLevelNodes--;

                return (true);
            }

            pOldStruct = pStruct;
            pStruct = pStruct.pNext;

        }

        // Could not find it, return FALSE
        RemoveWorldFlagsFromNewNode(iMapIndex, usIndex);

        return (false);

    }

    public static bool FloorAtGridNo(int iMapIndex)
    {
        LEVELNODE? pLand;
        TileTypeDefines uiTileType;
        pLand = gpWorldLevelData[iMapIndex].pLandHead;
        // Look through all objects and Search for type
        while (pLand is not null)
        {
            if (pLand.usIndex != TileDefines.NO_TILE)
            {
                TileDefine.GetTileType(pLand.usIndex, out uiTileType);
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

    public static bool RemoveStruct(int iMapIndex, TileDefines usIndex)
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
                    //if ( !( pStruct->pStructureData->fFlags & STRUCTURE_WALLSTUFF ) && pStruct->pStructureData->pDBStructureRef->pDBStructure->ubNumberOfTiles == 1 )
                    //{
                    // UNSet flag...
                    //	gpWorldLevelData[ iMapIndex ].ubExtFlags[0] &= ( ~MAPELEMENT_EXT_NOBURN_STRUCT );
                    //}
                }

                // Delete memory assosiated with item
                DeleteStructureFromWorld(pStruct.pStructureData);

                //If we have to, make sure to remove this node when we reload the map from a saved game
                RemoveStructFromMapTempFile(iMapIndex, usIndex);

                if (usIndex < TileDefines.NUMBEROFTILES)
                {
                    // Check flags for tiledat and set a shadow if we have a buddy
                    if (!GridNoIndoors(iMapIndex) && gTileDatabase[usIndex].uiFlags.HasFlag(TileCategory.HAS_SHADOW_BUDDY)
                        && gTileDatabase[usIndex].sBuddyNum != -1)
                    {
                        RemoveShadow(iMapIndex, gTileDatabase[usIndex].sBuddyNum);
                    }
                }
                MemFree(pStruct);
                guiLevelNodes--;

                return (true);
            }

            pOldStruct = pStruct;
            pStruct = pStruct.pNext;

        }

        // Could not find it, return FALSE
        RemoveWorldFlagsFromNewNode(iMapIndex, usIndex);

        return (false);
    }

    public static TerrainTypeDefines GetTerrainType(int sGridNo)
    {
        return (gpWorldLevelData[sGridNo].ubTerrainID);
        /*
            LEVELNODE	*pNode;


            // Check if we have anything in object layer which has a terrain modifier
            pNode = gpWorldLevelData[ sGridNo ].pObjectHead;

            if ( pNode != NULL )
            {
                if ( gTileDatabase[ pNode->usIndex ].ubTerrainID != NO_TERRAIN )
                {
                    return( gTileDatabase[ pNode->usIndex ].ubTerrainID );
                }
            }

            // Now try terrain!
            pNode = gpWorldLevelData[ sGridNo ].pLandHead;

            return( gTileDatabase[ pNode->usIndex ].ubTerrainID );
        */
    }

    public static bool RemoveAllOnRoofsOfTypeRange(int iMapIndex, TileTypeDefines fStartType, TileTypeDefines fEndType)
    {
        LEVELNODE? pOnRoof = null;
        LEVELNODE? pOldOnRoof = null;
        TileTypeDefines fTileType;
        bool fRetVal = false;

        pOnRoof = Globals.gpWorldLevelData[iMapIndex].pOnRoofHead;

        // Look through all OnRoofs and Search for type

        while (pOnRoof != null)
        {
            if (pOnRoof.usIndex != TileDefines.NO_TILE)
            {
                TileDefine.GetTileType(pOnRoof.usIndex, out fTileType);

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

    public static LEVELNODE? AddObjectToTail(int iMapIndex, TileDefines usIndex)
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
        return (pNextObject);

    }

    // OnRoof layer
    // #################################################################

    public static LEVELNODE? AddOnRoofToTail(int iMapIndex, TileDefines usIndex)
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

            if (usIndex < TileDefines.NUMBEROFTILES)
            {
                if (Globals.gTileDatabase[usIndex].pDBStructureRef != null)
                {
                    if (WorldStructures.AddStructureToWorld(iMapIndex, 1, Globals.gTileDatabase[usIndex].pDBStructureRef, pOnRoof) == false)
                    {
                        // MemFree(pOnRoof);
                        Globals.guiLevelNodes--;
                        return (null);
                    }
                }
            }
            pOnRoof.usIndex = usIndex;

            Globals.gpWorldLevelData[iMapIndex].pOnRoofHead = pOnRoof;

            RenderWorld.ResetSpecificLayerOptimizing(TILES_DYNAMIC.ONROOF);
            return (pOnRoof);

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

                    if (usIndex < TileDefines.NUMBEROFTILES)
                    {
                        if (Globals.gTileDatabase[usIndex].pDBStructureRef != null)
                        {
                            if (WorldStructures.AddStructureToWorld(iMapIndex, 1, Globals.gTileDatabase[usIndex].pDBStructureRef, pNextOnRoof) == false)
                            {
                                // MemFree(pNextOnRoof);
                                Globals.guiLevelNodes--;
                                return (null);
                            }
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
        return (pNextOnRoof);
    }

    public static bool RemoveAllTopmostsOfTypeRange(int iMapIndex, TileTypeDefines fStartType, TileTypeDefines fEndType)
    {
        LEVELNODE? pTopmost = null;
        LEVELNODE? pOldTopmost = null;
        TileTypeDefines fTileType;
        bool fRetVal = false;

        pTopmost = Globals.gpWorldLevelData[iMapIndex].pTopmostHead;

        // Look through all topmosts and Search for type

        while (pTopmost != null)
        {
            // Advance to next
            pOldTopmost = pTopmost;
            pTopmost = pTopmost.pNext;

            if (pOldTopmost.usIndex != TileDefines.NO_TILE && pOldTopmost.usIndex < TileDefines.NUMBEROFTILES)
            {
                TileDefine.GetTileType(pOldTopmost.usIndex, out fTileType);

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

    public static bool RemoveOnRoof(int iMapIndex, TileDefines usIndex)
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

                return (true);
            }

            pOldOnRoof = pOnRoof;
            pOnRoof = pOnRoof.pNext;

        }

        // Could not find it, return false

        return (false);
    }

    public static bool RemoveAllObjectsOfTypeRange(int iMapIndex, TileTypeDefines fStartType, TileTypeDefines fEndType)
    {
        LEVELNODE? pObject = null;
        LEVELNODE? pOldObject = null;
        TileTypeDefines fTileType;
        bool fRetVal = false;

        pObject = Globals.gpWorldLevelData[iMapIndex].pObjectHead;

        // Look through all objects and Search for type

        while (pObject != null)
        {
            // Advance to next
            pOldObject = pObject;
            pObject = pObject.pNext;

            if (pOldObject.usIndex != TileDefines.NO_TILE && pOldObject.usIndex < TileDefines.NUMBEROFTILES)
            {

                TileDefine.GetTileType(pOldObject.usIndex, out fTileType);

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

    public static bool RemoveObject(int iMapIndex, TileDefines usIndex)
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
        return (false);
    }

    public static int WhoIsThere2(int sGridNo, int bLevel)
    {
        STRUCTURE? pStructure;

        if (!IsometricUtils.GridNoOnVisibleWorldTile(sGridNo))
        {
            return (Globals.NOBODY);
        }

        if (Globals.gpWorldLevelData[sGridNo].pStructureHead != null)
        {

            pStructure = Globals.gpWorldLevelData[sGridNo].pStructureHead;

            while (pStructure is not null)
            {
                // person must either have their pSoldier.sGridNo here or be non-passable
                if ((pStructure.fFlags.HasFlag(STRUCTUREFLAGS.PERSON))
                    && (!(pStructure.fFlags.HasFlag(STRUCTUREFLAGS.PASSABLE))
                    || Globals.MercPtrs[pStructure.usStructureID].sGridNo == sGridNo))
                {
                    if ((bLevel == 0 && pStructure.sCubeOffset == 0) || (bLevel > 0 && pStructure.sCubeOffset > 0))
                    {
                        // found a person, on the right level!
                        // structure ID and merc ID are identical for merc structures
                        return (pStructure.usStructureID);
                    }
                }

                pStructure = pStructure.pNext;
            }
        }

        return (Globals.NOBODY);
    }

    public static bool RemoveTopmost(int iMapIndex, TileDefines usIndex)
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

                return (true);
            }

            pOldTopmost = pTopmost;
            pTopmost = pTopmost.pNext;
        }

        // Could not find it, return false
        return (false);
    }

    public bool AddTopmostToHead(int iMapIndex, TileDefines usIndex)
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

        return (true);
    }

    public void AddUIElem(int iMapIndex, TileDefines usIndex, int sRelativeX, int sRelativeY, out LEVELNODE ppNewNode)
        => AddUIElem(iMapIndex, usIndex, sRelativeX, sRelativeY, out ppNewNode);

    public bool AddUIElem(int iMapIndex, TileDefines usIndex, int sRelativeX, MAP_ROW sRelativeY, out LEVELNODE ppNewNode)
    {
        LEVELNODE? pTopmost = AddTopmostToTail(iMapIndex, usIndex);

        Debug.Assert(pTopmost != null);

        // Set flags
        pTopmost.uiFlags |= LEVELNODEFLAGS.USERELPOS;
        pTopmost.sRelativeX = sRelativeX;
        pTopmost.sRelativeY = sRelativeY;

        RenderWorld.ResetSpecificLayerOptimizing(TILES_DYNAMIC.TOPMOST);

        ppNewNode = pTopmost;

        return (true);
    }

    public void RemoveUIElem(int iMapIndex, TileDefines usIndex)
    {
        RemoveTopmost(iMapIndex, usIndex);
    }

    public LEVELNODE? AddTopmostToTail(int iMapIndex, TileDefines usIndex)
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
        return (pNextTopmost);
    }

    public bool AddOnRoofToHead(int iMapIndex, TileDefines usIndex)
    {
        LEVELNODE? pOnRoof = null;
        LEVELNODE? pNextOnRoof = null;

        pOnRoof = Globals.gpWorldLevelData[iMapIndex].pOnRoofHead;

        if (CreateLevelNode(out pNextOnRoof) == false)
        {

        }

        if (usIndex < TileDefines.NUMBEROFTILES)
        {
            if (gTileDatabase[usIndex].pDBStructureRef != null)
            {
                if (WorldStructures.AddStructureToWorld((short)iMapIndex, 1, Globals.gTileDatabase[usIndex].pDBStructureRef, pNextOnRoof) == false)
                {
                    // MemFree(pNextOnRoof);
                    Globals.guiLevelNodes--;
                    return (false);
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

        return (true);
    }

    public bool IsRoofVisible(int sMapPos)
    {
        STRUCTURE? pStructure;

        if (!gfBasement)
        {
            pStructure = WorldStructures.FindStructure(sMapPos, STRUCTUREFLAGS.ROOF);

            if (pStructure != null)
            {
                if (!(Globals.gpWorldLevelData[sMapPos].uiFlags.HasFlag(MAPELEMENTFLAGS.REVEALED)))
                {
                    return (true);
                }
            }
        }
        else
        {
            //if ( InARoom( sMapPos, &ubRoom ) )
            {
                //if ( !( gpWorldLevelData[ sMapPos ].uiFlags.HasFlag(MAPELEMENT_REVEALED )) )
                {
                    return (true);
                }
            }
        }

        return (false);
    }
}
