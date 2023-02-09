using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core;

public enum IsometricDefines
{
    MAXCOL = WorldDefines.WORLD_COLS,
    MAXROW = WorldDefines.WORLD_ROWS,
    GRIDSIZE = MAXCOL * MAXROW,
    RIGHTMOSTGRID = MAXCOL - 1,
    LASTROWSTART = GRIDSIZE - MAXCOL,
    NOWHERE = GRIDSIZE + 1,
    NO_MAP_POS = NOWHERE,
    MAPWIDTH = WorldDefines.WORLD_COLS,
    MAPHEIGHT = WorldDefines.WORLD_ROWS,
    MAPLENGTH = MAPHEIGHT * MAPWIDTH,
}


public class IsometricUtils
{
}
