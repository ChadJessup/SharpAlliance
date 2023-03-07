using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.SubSystems;

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
        WorldStructures = worldStructure;
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

    public int FindCacheStructDataIndex(string cFilename)
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

}

public struct TILE_CACHE_ELEMENT
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
