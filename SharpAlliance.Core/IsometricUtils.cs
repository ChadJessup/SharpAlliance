using System;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public class IsometricUtils
{
    public const int MAXCOL = World.WORLD_COLS;
    public const int MAXROW = World.WORLD_ROWS;
    public const int GRIDSIZE = MAXCOL * MAXROW;
    public const int RIGHTMOSTGRID = MAXCOL - 1;
    public const int LASTROWSTART = GRIDSIZE - MAXCOL;
    public const int NOWHERE = GRIDSIZE + 1;
    public const int NO_MAP_POS = NOWHERE;
    public const int MAPWIDTH = World.WORLD_COLS;
    public const int MAPHEIGHT = World.WORLD_ROWS;
    public const int MAPLENGTH = MAPHEIGHT * MAPWIDTH;

    int[] DirIncrementer = new int[8]
    {
        -MAPWIDTH,        //N
	    1-MAPWIDTH,       //NE
	    1,                //E
	    1+MAPWIDTH,       //SE
	    MAPWIDTH,         //S
	    MAPWIDTH-1,       //SW
	    -1,               //W
	    -MAPWIDTH-1       //NW
    };

    public bool OutOfBounds(int sGridno, int sProposedGridno)
    {
        int sMod, sPropMod;

        // get modulas of our origin
        sMod = sGridno % MAXCOL;

        if (sMod != 0)          // if we're not on leftmost grid
        {
            if (sMod != RIGHTMOSTGRID)  // if we're not on rightmost grid
            {
                if (sGridno < LASTROWSTART) // if we're above bottom row
                {
                    if (sGridno > MAXCOL)   // if we're below top row
                    {
                        // Everything's OK - we're not on the edge of the map
                        return (false);
                    }
                }
            }
        }

        // if we've got this far, there's a potential problem - check it out!

        if (sProposedGridno < 0)
        {
            return (true);
        }

        sPropMod = sProposedGridno % MAXCOL;

        if (sMod == 0 && sPropMod == RIGHTMOSTGRID)
        {
            return (true);
        }
        else if (sMod == RIGHTMOSTGRID && sPropMod == 0)
        {
            return (true);
        }
        else if (sGridno >= LASTROWSTART && sProposedGridno >= GRIDSIZE)
        {
            return (true);
        }
        else
        {
            return (false);
        }
    }

    public int NewGridNo(int sGridno, int sDirInc)
    {
        int sProposedGridno = sGridno + sDirInc;

        // now check for out-of-bounds 
        if (OutOfBounds(sGridno, sProposedGridno))
        {
            // return ORIGINAL gridno to user
            sProposedGridno = sGridno;
        }

        return (sProposedGridno);
    }


    public int DirectionInc(WorldDirections sDirection) => DirectionInc((int)sDirection);

    public int DirectionInc(int sDirection)
    {
        if ((sDirection < 0) || (sDirection > 7))
        {
            //#ifdef BETAVERSION
            //   NumMessage("DirectionInc: Invalid direction received, = ",direction);
            //#endif

            //direction = random(8);	// replace garbage with random direction
            sDirection = 1;
        }

        return (DirIncrementer[sDirection]);
    }
}
