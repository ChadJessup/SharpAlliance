using System;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core.Screens
{
    public class TileCache
    {
        public ValueTask<bool> InitTileCache()
        {
            return ValueTask.FromResult(true);
        }

        public static void CheckForAndDeleteTileCacheStructInfo(LEVELNODE? pNode, ushort usIndex)
        {
            STRUCTURE_FILE_REF? pStructureFileRef;

            if (usIndex >= TILE_CACHE_START_INDEX)
            {
                pStructureFileRef = GetCachedTileStructureRef((usIndex - TILE_CACHE_START_INDEX));

                if (pStructureFileRef != null)
                {
                    DeleteStructureFromWorld(pNode.pStructureData);
                }
            }
        }
    }
}
