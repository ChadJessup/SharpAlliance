using System;
using SharpAlliance.Core.SubSystems;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Veldrid.MetalBindings;
using SixLabors.ImageSharp;

namespace SharpAlliance.Core;

public class IsometricUtils
{
    int[] DirIncrementer = new int[8]
    {
        -Globals.MAPWIDTH,        //N
	    1-Globals.MAPWIDTH,       //NE
	    1,                //E
	    1+Globals.MAPWIDTH,       //SE
	    Globals.MAPWIDTH,         //S
	    Globals.MAPWIDTH-1,       //SW
	    -1,               //W
	    -Globals.MAPWIDTH-1       //NW
    };

    public bool OutOfBounds(int sGridno, int sProposedGridno)
    {
        int sMod, sPropMod;

        // get modulas of our origin
        sMod = sGridno % Globals.MAXCOL;

        if (sMod != 0)          // if we're not on leftmost grid
        {
            if (sMod != Globals.RIGHTMOSTGRID)  // if we're not on rightmost grid
            {
                if (sGridno < Globals.LASTROWSTART) // if we're above bottom row
                {
                    if (sGridno > Globals.MAXCOL)   // if we're below top row
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

        sPropMod = sProposedGridno % Globals.MAXCOL;

        if (sMod == 0 && sPropMod == Globals.RIGHTMOSTGRID)
        {
            return (true);
        }
        else if (sMod == Globals.RIGHTMOSTGRID && sPropMod == 0)
        {
            return (true);
        }
        else if (sGridno >= Globals.LASTROWSTART && sProposedGridno >= Globals.GRIDSIZE)
        {
            return (true);
        }
        else
        {
            return (false);
        }
    }

    public static void ConvertGridNoToCellXY(int sGridNo, out int sXPos, out int sYPos)
    {
        sYPos = (sGridNo / Globals.WORLD_COLS);
        sXPos = sGridNo - (sYPos * Globals.WORLD_COLS);
        sYPos *= Globals.CELL_Y_SIZE;
        sXPos *= Globals.CELL_X_SIZE;
    }

    public static bool IsPointInScreenRect(int sXPos, int sYPos, Rectangle pRect)
    {
        if ((sXPos >= pRect.Left) && (sXPos <= pRect.Right) && (sYPos >= pRect.Top) && (sYPos <= pRect.Bottom))
        {
            return (true);
        }
        else
        {
            return (false);
        }
    }


    public static void FromCellToScreenCoordinates(int sCellX, int sCellY, out int psScreenX, out int psScreenY)
    {
        psScreenX = (2 * sCellX) - (2 * sCellY);
        psScreenY = sCellX + sCellY;
    }

    public static void FromScreenToCellCoordinates(int sScreenX, int sScreenY, out int psCellX, out int psCellY)
    {
        psCellX = ((sScreenX + (2 * sScreenY)) / 4);
        psCellY = ((2 * sScreenY) - sScreenX) / 4;
    }

    // These two functions take into account that our world is projected and attached
    // to the screen (0,0) in a specific way, and we MUSt take that into account then
    // determining screen coords

    public void FloatFromCellToScreenCoordinates(float dCellX, float dCellY, out float pdScreenX, out float pdScreenY)
    {
        float dScreenX, dScreenY;

        dScreenX = (2 * dCellX) - (2 * dCellY);
        dScreenY = dCellX + dCellY;

        pdScreenX = dScreenX;
        pdScreenY = dScreenY;
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
