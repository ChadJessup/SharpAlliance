namespace SharpAlliance.Core;

public static class IsometricDefines
{
    public const int MAXCOL = (int)WorldDefines.WORLD_COLS;
    public const int MAXROW = (int)WorldDefines.WORLD_ROWS;
    public const int GRIDSIZE = MAXCOL * MAXROW;
    public const int RIGHTMOSTGRID = MAXCOL - 1;
    public const int LASTROWSTART = GRIDSIZE - MAXCOL;
    public const int NOWHERE = GRIDSIZE + 1;
    public const int NO_MAP_POS = NOWHERE;
    public const int MAPWIDTH = (int)WorldDefines.WORLD_COLS;
    public const int MAPHEIGHT = (int)WorldDefines.WORLD_ROWS;
    public const int MAPLENGTH = MAPHEIGHT * MAPWIDTH;
}
