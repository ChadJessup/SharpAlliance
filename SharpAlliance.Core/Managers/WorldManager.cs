using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Veldrid.OpenGLBinding;

namespace SharpAlliance.Core.Managers;

public class WorldManager
{
    private readonly ILogger<WorldManager> logger;
    private readonly Globals globals;

    public WorldManager(
        ILogger<WorldManager> logger,
        Globals globals)
    {
        this.logger = logger;
        this.globals = globals;
    }

    public int guiLevelNodes { get; set; } = 0;

    public bool RemoveAllOnRoofsOfTypeRange(int iMapIndex, TileTypeDefines fStartType, TileTypeDefines fEndType)
    {
        LEVELNODE? pOnRoof = null;
        LEVELNODE? pOldOnRoof = null;
        TileTypeDefines fTileType;
        bool fRetVal = false;

        pOnRoof = this.globals.gpWorldLevelData[iMapIndex].pOnRoofHead;

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

    public bool RemoveAllTopmostsOfTypeRange(int iMapIndex, TileTypeDefines fStartType, TileTypeDefines fEndType)
    {
        LEVELNODE? pTopmost = null;
        LEVELNODE? pOldTopmost = null;
        TileTypeDefines fTileType;
        bool fRetVal = false;

        pTopmost = this.globals.gpWorldLevelData[iMapIndex].pTopmostHead;

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
                    RemoveTopmost(iMapIndex, pOldTopmost.usIndex);
                    fRetVal = true;
                }
            }
        }

        return fRetVal;
    }

    public bool RemoveAllObjectsOfTypeRange(int iMapIndex, TileTypeDefines fStartType, TileTypeDefines fEndType)
    {
        LEVELNODE? pObject = null;
        LEVELNODE? pOldObject = null;
        TileTypeDefines fTileType;
        bool fRetVal = false;

        pObject = this.globals.gpWorldLevelData[iMapIndex].pObjectHead;

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

    public bool RemoveObject(int iMapIndex, ushort usIndex)
    {
        LEVELNODE? pObject = null;
        LEVELNODE? pOldObject = null;

        pObject = this.globals.gpWorldLevelData[iMapIndex].pObjectHead;

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
                    this.globals.gpWorldLevelData[iMapIndex].pObjectHead = pObject.pNext;
                }
                else
                {
                    pOldObject.pNext = pObject.pNext;
                }

                CheckForAndDeleteTileCacheStructInfo(pObject, usIndex);

                // Delete memory assosiated with item
                // MemFree(pObject);
                guiLevelNodes--;

                //Add the index to the maps temp file so we can remove it after reloading the map
                AddRemoveObjectToMapTempFile(iMapIndex, usIndex);

                return true;
            }

            pOldObject = pObject;
            pObject = pObject.pNext;
        }

        // Could not find it, return false
        return (false);
    }

    public bool RemoveTopmost(int iMapIndex, TileDefines usIndex)
        => RemoveTopmost(iMapIndex, (ushort)usIndex);

    public bool RemoveTopmost(int iMapIndex, ushort usIndex)
    {
        LEVELNODE? pTopmost = null;
        LEVELNODE? pOldTopmost = null;

        pTopmost = this.globals.gpWorldLevelData[iMapIndex].pTopmostHead;

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
                    this.globals.gpWorldLevelData[iMapIndex].pTopmostHead = pTopmost.pNext;
                }
                else
                {
                    pOldTopmost.pNext = pTopmost.pNext;
                }

                // Delete memory assosiated with item
                //MemFree(pTopmost);
                guiLevelNodes--;

                return (true);
            }

            pOldTopmost = pTopmost;
            pTopmost = pTopmost.pNext;
        }

        // Could not find it, return false
        return (false);
    }

    public bool AddTopmostToHead(int iMapIndex, TileDefines usIndex)
        => AddTopmostToHead(iMapIndex, usIndex);

    public bool AddTopmostToHead(int iMapIndex, ushort usIndex)
    {
        LEVELNODE? pTopmost = null;
        LEVELNODE? pNextTopmost = null;

        pTopmost = gpWorldLevelData[iMapIndex].pTopmostHead;

        // Allocate head
        CHECKF(CreateLevelNode(out pNextTopmost) != false);
        pNextTopmost.pNext = pTopmost;
        pNextTopmost.usIndex = usIndex;

        // Set head
        gpWorldLevelData[iMapIndex].pTopmostHead = pNextTopmost;

        ResetSpecificLayerOptimizing(TILES_DYNAMIC_TOPMOST);
        return (true);
    }

    public bool AddUIElem(int iMapIndex, ushort usIndex, sbyte sRelativeX, sbyte sRelativeY, List<LEVELNODE> ppNewNode)
    {
        LEVELNODE? pTopmost = null;

        pTopmost = AddTopmostToTail(iMapIndex, usIndex);

        CHECKF(pTopmost != null);

        // Set flags
        pTopmost.uiFlags |= LEVELNODE_USERELPOS;
        pTopmost.sRelativeX = sRelativeX;
        pTopmost.sRelativeY = sRelativeY;

        if (ppNewNode != null)
        {
            ppNewNode = pTopmost;
        }

        ResetSpecificLayerOptimizing(TILES_DYNAMIC_TOPMOST);
        return (true);
    }

    public void RemoveUIElem(int iMapIndex, ushort usIndex)
    {
        RemoveTopmost(iMapIndex, usIndex);
    }

    public LEVELNODE? AddTopmostToTail(int iMapIndex, ushort usIndex)
    {
        LEVELNODE? pTopmost = null;
        LEVELNODE? pNextTopmost = null;

        pTopmost = gpWorldLevelData[iMapIndex].pTopmostHead;

        // If we're at the head, set here
        if (pTopmost == null)
        {
            CHECKN(CreateLevelNode(out pNextTopmost) != false);
            pNextTopmost.usIndex = usIndex;

            gpWorldLevelData[iMapIndex].pTopmostHead = pNextTopmost;
        }
        else
        {
            while (pTopmost != null)
            {
                if (pTopmost.pNext == null)
                {
                    CHECKN(CreateLevelNode(out pNextTopmost) != false);
                    pTopmost.pNext = pNextTopmost;
                    pNextTopmost.pNext = null;
                    pNextTopmost.usIndex = usIndex;

                    break;
                }

                pTopmost = pTopmost.pNext;

            }

        }

        ResetSpecificLayerOptimizing(TILES_DYNAMIC_TOPMOST);
        return (pNextTopmost);
    }

    public bool AddOnRoofToHead(int iMapIndex, ushort usIndex)
    {
        LEVELNODE? pOnRoof = null;
        LEVELNODE? pNextOnRoof = null;

        pOnRoof = gpWorldLevelData[iMapIndex].pOnRoofHead;

        CHECKF(CreateLevelNode(&pNextOnRoof) != false);
        if (usIndex < NUMBEROFTILES)
        {
            if (gTileDatabase[usIndex].pDBStructureRef != null)
            {
                if (AddStructureToWorld((INT16)iMapIndex, 1, gTileDatabase[usIndex].pDBStructureRef, pNextOnRoof) == FALSE)
                {
                    // MemFree(pNextOnRoof);
                    guiLevelNodes--;
                    return (false);
                }
            }
        }

        pNextOnRoof.pNext = pOnRoof;
        pNextOnRoof.usIndex = usIndex;


        // Set head
        gpWorldLevelData[iMapIndex].pOnRoofHead = pNextOnRoof;

        ResetSpecificLayerOptimizing(TILES_DYNAMIC_ONROOF);
        return true;
    }
}
