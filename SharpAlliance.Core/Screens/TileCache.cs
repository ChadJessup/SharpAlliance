using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core;
using SharpAlliance.Core.SubSystems;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Screens;

public class TileCache
{
    private readonly WorldStructures worldStructures;
    private readonly ILogger<TileCache> logger;

    public TileCache(
        ILogger<TileCache> logger,
        WorldStructures worldStructure)
    {
        this.logger = logger;
        this.worldStructures = worldStructure;
    }

    public ValueTask<bool> InitTileCache()
    {
        return ValueTask.FromResult(true);
    }

    public static void CheckForAndDeleteTileCacheStructInfo(LEVELNODE? pNode, int usIndex)
    {
        ArgumentNullException.ThrowIfNull(pNode);

        STRUCTURE_FILE_REF? pStructureFileRef;

        if (usIndex >= Globals.TILE_CACHE_START_INDEX)
        {
            pStructureFileRef = GetCachedTileStructureRef((usIndex - Globals.TILE_CACHE_START_INDEX));

            if (pStructureFileRef != null)
            {
                WorldStructures.DeleteStructureFromWorld(pNode.pStructureData);
            }
        }
    }

    public static STRUCTURE_FILE_REF? GetCachedTileStructureRef(int iIndex)
    {
        if (iIndex == -1)
        {
            return null;
        }

        if (Globals.gpTileCache[iIndex].sStructRefID == -1)
        {
            return null;
        }

        return (Globals.gpTileCacheStructInfo[Globals.gpTileCache[iIndex].sStructRefID].pStructureFileRef);
    }

    private STRUCTURE_FILE_REF? GetCachedTileStructureRefFromFilename(string cFilename)
    {
        int sStructDataIndex;

        // Given filename, look for index
        sStructDataIndex = FindCacheStructDataIndex(cFilename);

        if (sStructDataIndex == -1)
        {
            return null;
        }

        return (Globals.gpTileCacheStructInfo[sStructDataIndex].pStructureFileRef);
    }

    public static int FindCacheStructDataIndex(string cFilename)
    {
        int cnt;

        for (cnt = 0; cnt < Globals.guiNumTileCacheStructs; cnt++)
        {
            if (Globals.gpTileCacheStructInfo[cnt].zRootName.Equals(cFilename, StringComparison.OrdinalIgnoreCase))
            {
                return cnt;
            }
        }

        return -1;
    }

    public static int GetCachedTile(string cFilename)
    {
        int  cnt;
        int  ubLowestIndex = 0;
        int sMostHits = 15000;

        // Check to see if surface exists already
        for (cnt = 0; cnt < guiCurTileCacheSize; cnt++)
        {
            if (gpTileCache[cnt].pImagery != null)
            {
                if (_stricmp(gpTileCache[cnt].zName, cFilename) == 0)
                {
                    // Found surface, return
                    gpTileCache[cnt].sHits++;
                    return cnt;
                }
            }
        }

        // Check if max size has been reached
        if (guiCurTileCacheSize == guiMaxTileCacheSize)
        {
            // cache out least used file
            for (cnt = 0; cnt < guiCurTileCacheSize; cnt++)
            {
                if (gpTileCache[cnt].sHits < sMostHits)
                {
                    sMostHits = gpTileCache[cnt].sHits;
                    ubLowestIndex = cnt;
                }
            }

            // Bump off lowest index
            DeleteTileSurface(gpTileCache[ubLowestIndex].pImagery);

            // Decrement
            gpTileCache[ubLowestIndex].sHits = 0;
            gpTileCache[ubLowestIndex].pImagery = null;
            gpTileCache[ubLowestIndex].sStructRefID = -1;
        }

        // If here, Insert at an empty slot
        // Find an empty slot
        for (cnt = 0; cnt < guiMaxTileCacheSize; cnt++)
        {
            if (gpTileCache[cnt].pImagery == null)
            {
                // Insert here
                gpTileCache[cnt].pImagery = LoadTileSurface(cFilename);

                if (gpTileCache[cnt].pImagery == null)
                {
                    return (-1);
                }

                strcpy(gpTileCache[cnt].zName, cFilename);
                gpTileCache[cnt].sHits = 1;

                // Get root name
                GetRootName(gpTileCache[cnt].zRootName, cFilename);

                gpTileCache[cnt].sStructRefID = FindCacheStructDataIndex(gpTileCache[cnt].zRootName);

                // ATE: Add z-strip info
                if (gpTileCache[cnt].sStructRefID != -1)
                {
                    AddZStripInfoToVObject(gpTileCache[cnt].pImagery.vo, gpTileCacheStructInfo[gpTileCache[cnt].sStructRefID].pStructureFileRef, TRUE, 0);
                }

                if (gpTileCache[cnt].pImagery.pAuxData != null)
                {
                    gpTileCache[cnt].ubNumFrames = gpTileCache[cnt].pImagery.pAuxData.ubNumberOfFrames;
                }
                else
                {
                    gpTileCache[cnt].ubNumFrames = 1;
                }

                // Has our cache size increased?
                if (cnt >= guiCurTileCacheSize)
                {
                    guiCurTileCacheSize = cnt + 1;
                    ;
                }

                return (cnt);
            }
        }

        // Can't find one!
        return (-1);
    }
}

public class TILE_CACHE_ELEMENT
{
    public string zName;           // Name of tile ( filename and directory here )
    public string zRootName;    // Root name
    public TILE_IMAGERY? pImagery;             // Tile imagery
    public int sHits;
    public int ubNumFrames;
    public int sStructRefID;
}


public struct TILE_CACHE_STRUCT
{
    public string Filename;
    public string zRootName;    // Root name
    public STRUCTURE_FILE_REF? pStructureFileRef;
}
