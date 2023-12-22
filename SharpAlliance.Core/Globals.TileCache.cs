using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Screens;

namespace SharpAlliance.Core;

public partial class Globals
{
    public static int  guiNumTileCacheStructs = 0;
    public static int  guiMaxTileCacheSize = 50;
    public static int guiCurTileCacheSize { get; internal set; }

    public static int giDefaultStructIndex = -1;

    public static List<TILE_CACHE_STRUCT> gpTileCacheStructInfo = new();
    public static List<TILE_CACHE_ELEMENT> gpTileCache = new();

    public const int TILE_CACHE_START_INDEX = 36000;
}
