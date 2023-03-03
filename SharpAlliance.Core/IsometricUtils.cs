using System;
using SharpAlliance.Core.SubSystems;
using SixLabors.ImageSharp;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class IsometricUtils
{
    static int[] DirIncrementer = new int[8]
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

    public static bool OutOfBounds(int sGridno, int sProposedGridno)
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

    static int sSameCursorPos;
    static int uiOldFrameNumber = 99999;

    public static void GetWorldXYAbsoluteScreenXY(int sWorldCellX, int sWorldCellY, out int psWorldScreenX, out int psWorldScreenY)
    {
        int sScreenCenterX, sScreenCenterY;
        int sDistToCenterY, sDistToCenterX;

        // Find the diustance from render center to true world center
        sDistToCenterX = (sWorldCellX * Globals.CELL_X_SIZE) - Globals.gCenterWorldX;
        sDistToCenterY = (sWorldCellY * Globals.CELL_Y_SIZE) - Globals.gCenterWorldY;


        // From render center in world coords, convert to render center in "screen" coords

        // ATE: We should call the fowllowing function but I'm putting it here verbatim for speed
        //FromCellToScreenCoordinates( sDistToCenterX , sDistToCenterY, &sScreenCenterX, &sScreenCenterY );
        sScreenCenterX = (2 * sDistToCenterX) - (2 * sDistToCenterY);
        sScreenCenterY = sDistToCenterX + sDistToCenterY;

        // Subtract screen center
        psWorldScreenX = sScreenCenterX + Globals.gsCX - Globals.gsTLX;
        psWorldScreenY = sScreenCenterY + Globals.gsCY - Globals.gsTLY;
    }

    public static int SpacesAway(int sOrigin, int sDest)
    {
        int sRows, sCols;

        sRows = Math.Abs((sOrigin / MAXCOL) - (sDest / MAXCOL));
        sCols = Math.Abs((sOrigin % MAXROW) - (sDest % MAXROW));

        return (Math.Max(sRows, sCols));
    }

    public static int PythSpacesAway(int sOrigin, int sDest)
    {
        int sRows, sCols, sResult;

        sRows = Math.Abs((sOrigin / MAXCOL) - (sDest / MAXCOL));
        sCols = Math.Abs((sOrigin % MAXROW) - (sDest % MAXROW));

        // apply Pythagoras's theorem for right-handed triangle:
        // dist^2 = rows^2 + cols^2, so use the square root to get the distance
        sResult = (int)Math.Sqrt((sRows * sRows) + (sCols * sCols));

        return (sResult);
    }


    public static bool GridNoOnVisibleWorldTile(int sGridNo)
    {

        // Check for valid gridno...
        ConvertGridNoToXY(sGridNo, out int sXMapPos, out int sYMapPos);

        // Get screen coordinates for current position of soldier
        GetWorldXYAbsoluteScreenXY(sXMapPos, sYMapPos, out int sWorldX, out int sWorldY);

        if (sWorldX > 0 && sWorldX < (Globals.gsTRX - Globals.gsTLX - 20) &&
                 sWorldY > 20 && sWorldY < (Globals.gsBLY - Globals.gsTLY - 20))
        {
            return (true);
        }

        return (false);
    }

    public static void ConvertGridNoToXY(int sGridNo, out int sXPos, out int sYPos)
    {
        sYPos = sGridNo / Globals.WORLD_COLS;
        sXPos = (sGridNo - (sYPos * Globals.WORLD_COLS));
    }

    public static void ConvertGridNoToCenterCellXY(int sGridNo, out int sXPos, out int sYPos)
    {
        sYPos = (sGridNo / Globals.WORLD_COLS);
        sXPos = (sGridNo - (sYPos * Globals.WORLD_COLS));

        sYPos = (sYPos * Globals.CELL_Y_SIZE) + (Globals.CELL_Y_SIZE / 2);
        sXPos = (sXPos * Globals.CELL_X_SIZE) + (Globals.CELL_X_SIZE / 2);
    }

    public static bool GetMouseMapPos(out int psMapPos)
    {
        int sWorldX, sWorldY;

        // Check if this is the same frame as before, return already calculated value if so!
        if (uiOldFrameNumber == Globals.guiGameCycleCounter && !Globals.guiForceRefreshMousePositionCalculation)
        {
            psMapPos = sSameCursorPos;

            if (sSameCursorPos == 0)
            {
                return (false);
            }

            return (true);
        }

        uiOldFrameNumber = Globals.guiGameCycleCounter;
        Globals.guiForceRefreshMousePositionCalculation = false;


        if (GetMouseXY(out sWorldX, out sWorldY))
        {
            psMapPos = MAPROWCOLTOPOS(sWorldY, sWorldX);
            sSameCursorPos = psMapPos;
            return (true);
        }
        else
        {
            psMapPos = 0;
            sSameCursorPos = (psMapPos);
            return false;
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

    public static int MAPROWCOLTOPOS(int r, int c)
    {
        return (((r < 0) || (r >= Globals.WORLD_ROWS) || (c < 0) || (c >= Globals.WORLD_COLS))
            ? (0xffff)
            : ((r) * Globals.WORLD_COLS + (c)));
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

    public static void FloatFromCellToScreenCoordinates(float dCellX, float dCellY, out float pdScreenX, out float pdScreenY)
    {
        float dScreenX, dScreenY;

        dScreenX = (2 * dCellX) - (2 * dCellY);
        dScreenY = dCellX + dCellY;

        pdScreenX = dScreenX;
        pdScreenY = dScreenY;
    }

    // Returns the (center ) cell coordinates in X
    public static int CenterX(int sGridNo)
    {
        int sYPos, sXPos;

        sYPos = sGridNo / Globals.WORLD_COLS;
        sXPos = (sGridNo - (sYPos * Globals.WORLD_COLS));

        return ((sXPos * Globals.CELL_X_SIZE) + (Globals.CELL_X_SIZE / 2));
    }


    // Returns the (center ) cell coordinates in Y
    public static int CenterY(int sGridNo)
    {
        int sYPos, sXPos;

        sYPos = sGridNo / Globals.WORLD_COLS;
        sXPos = (sGridNo - (sYPos * Globals.WORLD_COLS));

        return ((sYPos * Globals.CELL_Y_SIZE) + (Globals.CELL_Y_SIZE / 2));
    }


    public static int MapX(int sGridNo)
    {
        int sYPos, sXPos;

        sYPos = sGridNo / Globals.WORLD_COLS;
        sXPos = (sGridNo - (sYPos * Globals.WORLD_COLS));

        return (sXPos);
    }


    public static int MapY(int sGridNo)
    {
        int sYPos, sXPos;

        sYPos = sGridNo / Globals.WORLD_COLS;
        sXPos = (sGridNo - (sYPos * Globals.WORLD_COLS));

        return (sYPos);
    }

    public static int NewGridNo(int sGridno, int sDirInc)
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


    public static int DirectionInc(WorldDirections sDirection) => DirectionInc((int)sDirection);

    public static int DirectionInc(int sDirection)
    {
        if ((sDirection < 0) || (sDirection > 7))
        {
            //#if BETAVERSION
            //   NumMessage("DirectionInc: Invalid direction received, = ",direction);
            //#endif

            //direction = random(8);	// replace garbage with random direction
            sDirection = 1;
        }

        return (DirIncrementer[sDirection]);
    }

    public static bool GetMouseXY(out int psMouseX, out int psMouseY)
    {
        int sWorldX, sWorldY;

        if (!GetMouseWorldCoords(out sWorldX, out sWorldY))
        {
            (psMouseX) = 0;
            (psMouseY) = 0;
            return (false);
        }

    // Find start block
    (psMouseX) = (sWorldX / Globals.CELL_X_SIZE);
        (psMouseY) = (sWorldY / Globals.CELL_Y_SIZE);

        return (true);
    }


    public static bool GetMouseXYWithRemainder(out int psMouseX, out int psMouseY, out int psCellX, out int psCellY)
    {
        int sWorldX, sWorldY;

        if (!GetMouseWorldCoords(out sWorldX, out sWorldY))
        {
            psMouseX = 0;
            psMouseY = 0;
            psCellX = 0;
            psCellY = 0;

            return (false);
        }

        // Find start block
        (psMouseX) = (sWorldX / Globals.CELL_X_SIZE);
        (psMouseY) = (sWorldY / Globals.CELL_Y_SIZE);

        (psCellX) = sWorldX - ((psMouseX) * Globals.CELL_X_SIZE);
        (psCellY) = sWorldY - ((psMouseY) * Globals.CELL_Y_SIZE);

        return (true);
    }

    public static bool GetMouseWorldCoords(out int psMouseX, out int psMouseY)
    {
        int sOffsetX, sOffsetY;
        int sTempPosX_W, sTempPosY_W;
        int sStartPointX_W, sStartPointY_W;

        // Convert mouse screen coords into offset from center
        if (!(Globals.gViewportRegion.uiFlags.HasFlag(MouseRegionFlags.IN_AREA)))
        {
            psMouseX = 0;
            psMouseY = 0;
            return (false);
        }

        sOffsetX = Globals.gViewportRegion.MousePos.X - ((Globals.gsVIEWPORT_END_X - Globals.gsVIEWPORT_START_X) / 2); // + gsRenderWorldOffsetX;
        sOffsetY = Globals.gViewportRegion.MousePos.Y - ((Globals.gsVIEWPORT_END_Y - Globals.gsVIEWPORT_START_Y) / 2) + 10;// + gsRenderWorldOffsetY;

        // OK, Let's offset by a value if our interfac level is changed!
        if (Globals.gsInterfaceLevel != 0)
        {
            //sOffsetY -= 50;
        }


        FromScreenToCellCoordinates(sOffsetX, sOffsetY, out sTempPosX_W, out sTempPosY_W);

        // World start point is Render center plus this distance
        sStartPointX_W = Globals.gsRenderCenterX + sTempPosX_W;
        sStartPointY_W = Globals.gsRenderCenterY + sTempPosY_W;


        // check if we are out of bounds..
        if (sStartPointX_W < 0
            || sStartPointX_W >= Globals.WORLD_COORD_ROWS
            || sStartPointY_W < 0
            || sStartPointY_W >= Globals.WORLD_COORD_COLS)
        {
            psMouseX = 0;
            psMouseY = 0;
            return (false);
        }

        // Determine Start block and render offsets
        // Find start block
        // Add adjustment for render origin as well
        (psMouseX) = sStartPointX_W;
        (psMouseY) = sStartPointY_W;

        return (true);
    }


    public static bool GetMouseWorldCoordsInCenter(out int psMouseX, out int psMouseY)
    {
        int sMouseX, sMouseY;

        // Get grid position
        if (!GetMouseXY(out sMouseX, out sMouseY))
        {
            psMouseX = 0;
            psMouseY = 0;

            return (false);
        }

        // Now adjust these cell coords into world coords
        psMouseX = ((sMouseX) * Globals.CELL_X_SIZE) + (Globals.CELL_X_SIZE / 2);
        psMouseY = ((sMouseY) * Globals.CELL_Y_SIZE) + (Globals.CELL_Y_SIZE / 2);

        return (true);
    }
}
