using System.Collections.Generic;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const int NUM_TILESETS = (int)WorldTileset.NUM_TILESETS;

    public static Dictionary<TileTypeDefines, TILESET> gTilesets = new();
}
