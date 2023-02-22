using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems;

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
            if (pOnRoof.usIndex != (uint)TileCategory.NO_TILE)
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

    public static LEVELNODE? AddObjectToTail(int iMapIndex, int usIndex)
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

        //CheckForAndAddTileCacheStructInfo( pNextObject, (INT16)iMapIndex, usIndex );

        RenderWorld.ResetSpecificLayerOptimizing(TILES_DYNAMIC.OBJECTS);
        return (pNextObject);

    }

    // OnRoof layer
    // #################################################################

    public static LEVELNODE? AddOnRoofToTail(int iMapIndex, int usIndex)
    {
        LEVELNODE? pOnRoof = null;
        LEVELNODE? pNextOnRoof = null;

        pOnRoof = Globals.gpWorldLevelData[iMapIndex].pOnRoofHead;

        // If we're at the head, set here
        if (pOnRoof == null)
        {
            if(!CreateLevelNode(out pOnRoof))
            {
                return null;
            }

            if (usIndex < (int)TileDefines.NUMBEROFTILES)
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
                    if(!CreateLevelNode(out pNextOnRoof))
                    {
                        return null;
                    }

                    if (usIndex < (int)TileDefines.NUMBEROFTILES)
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

            if (pOldTopmost.usIndex != (ushort)TileCategory.NO_TILE && pOldTopmost.usIndex < (ushort)TileDefines.NUMBEROFTILES)
            {
                TileDefine.GetTileType(pOldTopmost.usIndex, out fTileType);

                if (fTileType >= fStartType && fTileType <= fEndType)
                {
                    // Remove Item
                    RemoveTopmost(iMapIndex, (TileDefines)pOldTopmost.usIndex);
                    fRetVal = true;
                }
            }
        }

        return fRetVal;
    }

    public static bool RemoveOnRoof(int iMapIndex, int usIndex)
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

            if (pOldObject.usIndex != (ushort)TileCategory.NO_TILE && pOldObject.usIndex < (ushort)TileDefines.NUMBEROFTILES)
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

    public static bool RemoveObject(int iMapIndex, int usIndex)
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

        if (!GridNoOnVisibleWorldTile(sGridNo))
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
        => RemoveTopmost(iMapIndex, (ushort)usIndex);

    public static bool RemoveTopmost(int iMapIndex, ushort usIndex)
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
        => AddTopmostToHead(iMapIndex, (ushort)usIndex);

    public bool AddTopmostToHead(int iMapIndex, ushort usIndex)
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

    public bool AddUIElem(int iMapIndex, ushort usIndex, int sRelativeX, int sRelativeY, out LEVELNODE ppNewNode)
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

    public void RemoveUIElem(int iMapIndex, ushort usIndex)
    {
        RemoveTopmost(iMapIndex, usIndex);
    }


    public LEVELNODE? AddTopmostToTail(int iMapIndex, TileDefines usIndex)
        => AddTopmostToTail(iMapIndex, (ushort)usIndex);

    public LEVELNODE? AddTopmostToTail(int iMapIndex, ushort usIndex)
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
            if (Globals.gTileDatabase[(int)usIndex].pDBStructureRef != null)
            {
                if (WorldStructures.AddStructureToWorld((short)iMapIndex, 1, Globals.gTileDatabase[(int)usIndex].pDBStructureRef, pNextOnRoof) == false)
                {
                    // MemFree(pNextOnRoof);
                    Globals.guiLevelNodes--;
                    return (false);
                }
            }
        }

        pNextOnRoof.pNext = pOnRoof;
        pNextOnRoof.usIndex = (int)usIndex;


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

        if (!SubSystems.Environment.gfBasement)
        {
            pStructure = this.worldStructures.FindStructure(sMapPos, STRUCTUREFLAGS.ROOF);

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
                //if ( !( gpWorldLevelData[ sMapPos ].uiFlags & MAPELEMENT_REVEALED ) )
                {
                    return (true);
                }
            }
        }

        return (false);
    }
}
