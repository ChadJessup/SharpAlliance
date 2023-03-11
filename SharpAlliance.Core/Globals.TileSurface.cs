using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core;

public partial class Globals
{
    public static int NUMBEROFTILETYPES = (int)TileTypeDefines.NUMBEROFTILETYPES;

    public static TILE_IMAGERY[] gTileSurfaceArray = new TILE_IMAGERY[NUMBEROFTILETYPES];
    public static int[] gbDefaultSurfaceUsed = new int[NUMBEROFTILETYPES];
    public static int[] gbSameAsDefaultSurfaceUsed = new int[NUMBEROFTILETYPES];
}
