using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems;
using Veldrid.OpenGLBinding;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SharpAlliance.Core.Managers;

public class WorldManager
{
    private readonly ILogger<WorldManager> logger;
    private readonly Globals globals;
    private readonly SaveLoadMap saveLoadMap;
    private readonly TileCache tileCache;
    private readonly RenderWorld renderWorld;

    public WorldManager(
        ILogger<WorldManager> logger,
        Globals globals,
        SaveLoadMap saveLoadMap,
        TileCache tileCache,
        RenderWorld renderWorld)
    {
        this.logger = logger;
        this.globals = globals;
        this.saveLoadMap = saveLoadMap;
        this.tileCache = tileCache;
        this.renderWorld = renderWorld;
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
                    RemoveTopmost(iMapIndex, (TileDefines)pOldTopmost.usIndex);
                    fRetVal = true;
                }
            }
        }

        return fRetVal;
    }

    public bool RemoveOnRoof(int iMapIndex, int usIndex)
    {
        LEVELNODE? pOnRoof = null;
        LEVELNODE? pOldOnRoof = null;

        pOnRoof = this.globals.gpWorldLevelData[iMapIndex].pOnRoofHead;

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
                    this.globals.gpWorldLevelData[iMapIndex].pOnRoofHead = pOnRoof.pNext;
                }
                else
                {
                    pOldOnRoof.pNext = pOnRoof.pNext;
                }

                // REMOVE ONROOF!
                pOnRoof = null;
                guiLevelNodes--;

                return (true);
            }

            pOldOnRoof = pOnRoof;
            pOnRoof = pOnRoof.pNext;

        }

        // Could not find it, return false

        return (false);
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

    public bool RemoveObject(int iMapIndex, int usIndex)
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

                this.tileCache.CheckForAndDeleteTileCacheStructInfo(pObject, usIndex);

                // Delete memory assosiated with item
                // MemFree(pObject);
                guiLevelNodes--;

                //Add the index to the maps temp file so we can remove it after reloading the map
                this.saveLoadMap.AddRemoveObjectToMapTempFile(iMapIndex, usIndex);

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
        => AddTopmostToHead(iMapIndex, (ushort)usIndex);

    public bool AddTopmostToHead(int iMapIndex, ushort usIndex)
    {
        LEVELNODE? pTopmost = this.globals.gpWorldLevelData[iMapIndex].pTopmostHead;

        // Allocate head
        //CHECKF(
        if (CreateLevelNode(out LEVELNODE? pNextTopmost) == false)
        {

        }

        pNextTopmost.pNext = pTopmost;
        pNextTopmost.usIndex = usIndex;

        // Set head
        this.globals.gpWorldLevelData[iMapIndex].pTopmostHead = pNextTopmost;

        this.renderWorld.ResetSpecificLayerOptimizing(TILES_DYNAMIC.TOPMOST);

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

        this.renderWorld.ResetSpecificLayerOptimizing(TILES_DYNAMIC.TOPMOST);

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

        LEVELNODE? pTopmost = this.globals.gpWorldLevelData[iMapIndex].pTopmostHead;

        // If we're at the head, set here
        if (pTopmost == null)
        {
            if (!CreateLevelNode(out pNextTopmost))
            {
                return null;
            }

            pNextTopmost.usIndex = usIndex;

            this.globals.gpWorldLevelData[iMapIndex].pTopmostHead = pNextTopmost;
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

        this.renderWorld.ResetSpecificLayerOptimizing(TILES_DYNAMIC.TOPMOST);
        return (pNextTopmost);
    }

    public bool AddOnRoofToHead(int iMapIndex, TileDefines usIndex)
    {
        LEVELNODE? pOnRoof = null;
        LEVELNODE? pNextOnRoof = null;

        pOnRoof = this.globals.gpWorldLevelData[iMapIndex].pOnRoofHead;

        if (CreateLevelNode(out pNextOnRoof) == false)
        {

        }

        if (usIndex < TileDefines.NUMBEROFTILES)
        {
            if (TileDefine.gTileDatabase[(int)usIndex].pDBStructureRef != null)
            {
                if (AddStructureToWorld((short)iMapIndex, 1, TileDefine.gTileDatabase[(int)usIndex].pDBStructureRef, pNextOnRoof) == false)
                {
                    // MemFree(pNextOnRoof);
                    guiLevelNodes--;
                    return (false);
                }
            }
        }

        pNextOnRoof.pNext = pOnRoof;
        pNextOnRoof.usIndex = (int)usIndex;


        // Set head
        this.globals.gpWorldLevelData[iMapIndex].pOnRoofHead = pNextOnRoof;

        this.renderWorld.ResetSpecificLayerOptimizing(TILES_DYNAMIC.ONROOF);
        return true;
    }

    public bool CreateLevelNode(out LEVELNODE ppNode)
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

        guiLevelNodes++;

        return (true);
    }
}
