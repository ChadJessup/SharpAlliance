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
        this.worldStructures = worldStructure;
    }

    public const int TILE_CACHE_START_INDEX = 36000;
    public static int guiNumTileCacheStructs = 0;

    public List<TILE_CACHE_STRUCT> gpTileCacheStructInfo = new();
    public List<TILE_CACHE_ELEMENT> gpTileCache = new();

    public ValueTask<bool> InitTileCache()
    {
        return ValueTask.FromResult(true);
    }

    public void CheckForAndDeleteTileCacheStructInfo(LEVELNODE? pNode, int usIndex)
    {
        ArgumentNullException.ThrowIfNull(pNode);

        STRUCTURE_FILE_REF? pStructureFileRef;

        if (usIndex >= TILE_CACHE_START_INDEX)
        {
            pStructureFileRef = GetCachedTileStructureRef((usIndex - TILE_CACHE_START_INDEX));

            if (pStructureFileRef != null)
            {
                this.worldStructures.DeleteStructureFromWorld(pNode.pStructureData);
            }
        }
    }

    private STRUCTURE_FILE_REF? GetCachedTileStructureRef(int iIndex)
    {
        if (iIndex == -1)
        {
            return null;
        }

        if (gpTileCache[iIndex].sStructRefID == -1)
        {
            return null;
        }

        return (gpTileCacheStructInfo[gpTileCache[iIndex].sStructRefID].pStructureFileRef);
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

        return (gpTileCacheStructInfo[sStructDataIndex].pStructureFileRef);
    }

    public int FindCacheStructDataIndex(string cFilename)
    {
        int cnt;

        for (cnt = 0; cnt < guiNumTileCacheStructs; cnt++)
        {
            if (gpTileCacheStructInfo[cnt].zRootName.Equals(cFilename, StringComparison.OrdinalIgnoreCase))
            {
                return cnt;
            }
        }

        return -1;
    }

}

public struct TILE_CACHE_ELEMENT
{
    string zName;           // Name of tile ( filename and directory here )
    string zRootName;    // Root name
    TILE_IMAGERY? pImagery;             // Tile imagery
    int sHits;
    int ubNumFrames;
    public int sStructRefID;
}


public struct TILE_CACHE_STRUCT
{
    public string Filename;
    public string zRootName;    // Root name
    public STRUCTURE_FILE_REF? pStructureFileRef;
}
