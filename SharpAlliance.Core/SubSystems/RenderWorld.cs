using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using SixLabors.ImageSharp;
using Rectangle = SixLabors.ImageSharp.Rectangle;
using Point = SixLabors.ImageSharp.Point;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class RenderWorld : IDisposable
{
    public int fSelectMode;
    
    public static TILES_DYNAMIC uiLayerUsedFlags = (TILES_DYNAMIC)0xffffffff;

    public static RenderingFlags gRenderFlags { get; private set; }

    public void Dispose()
    {
    }

    public static void RenderStaticWorldRect(Rectangle rect, bool fDynamicsToo)
    {
    }

    public static void InvalidateWorldRedundency()
    {
        int uiCount;

        SetRenderFlags(RenderingFlags.CHECKZ);

        for (uiCount = 0; uiCount < WORLD_MAX; uiCount++)
        {
            gpWorldLevelData[uiCount].uiFlags |= MAPELEMENTFLAGS.REEVALUATE_REDUNDENCY;
        }
    }

    public static void ResetSpecificLayerOptimizing(TILES_DYNAMIC uiRowFlag)
    {
        uiLayerUsedFlags |= uiRowFlag;
    }


    public static void SetRenderCenter(int sNewX, int sNewY)
    {
        if (gfIgnoreScrolling)
        {
            return;
        }

        // Apply these new coordinates to the renderer!
        ApplyScrolling(sNewX, sNewY, true, false);

        // Set flag to ignore scrolling this frame
        gfIgnoreScrollDueToCenterAdjust = true;

        // Set full render flag!
        // DIRTY THE WORLD!
        SetRenderFlags(RenderingFlags.FULL);

        gfPlotNewMovement = true;

        if (gfScrollPending == true)
        {
            // Do a complete rebuild!
            gfScrollPending = false;

            // Restore Interface!
//            RestoreInterface();

            // Delete Topmost blitters saved areas
//            DeleteVideoOverlaysArea();

        }

        gfScrollInertia = false;
    }

    // Appy? HEahehahehahehae.....
    public static bool ApplyScrolling(int sTempRenderCenterX, int sTempRenderCenterY, bool fForceAdjust, bool fCheckOnly)
    {
        bool fScrollGood = false;
        bool fOutLeft = false;
        bool fOutRight = false;
        bool fOutTop = false;
        bool fOutBottom = false;


        double dOpp, dAdj, dAngle = 0;

        int sTopLeftWorldX, sTopLeftWorldY;
        int sTopRightWorldX, sTopRightWorldY;
        int sBottomLeftWorldX, sBottomLeftWorldY;
        int sBottomRightWorldX, sBottomRightWorldY;
        int sTempPosX_W = 0, sTempPosY_W = 0;


        // For debug text for all 4 angles
        double at1, at2, at3, at4;

        int sX_S, sY_S;
        int sScreenCenterX = 0, sScreenCenterY = 0;
        int sDistToCenterY, sDistToCenterX;
        int sNewScreenX, sNewScreenY;
        int sMult;


        //Makesure it's a multiple of 5
        sMult = sTempRenderCenterX / CELL_X_SIZE;
        sTempRenderCenterX = (sMult * CELL_X_SIZE) + (CELL_X_SIZE / 2);

        //Makesure it's a multiple of 5
        sMult = sTempRenderCenterY / CELL_X_SIZE;
        sTempRenderCenterY = (sMult * CELL_Y_SIZE) + (CELL_Y_SIZE / 2);


        // Find the diustance from render center to true world center
        sDistToCenterX = sTempRenderCenterX - gCenterWorldX;
        sDistToCenterY = sTempRenderCenterY - gCenterWorldY;

        // From render center in world coords, convert to render center in "screen" coords
//        FromCellToScreenCoordinates(sDistToCenterX, sDistToCenterY, out sScreenCenterX, out sScreenCenterY);

        // Subtract screen center
        sScreenCenterX += gsCX;
        sScreenCenterY += gsCY;

        // Adjust for offset position on screen
        sScreenCenterX -= 0;
        sScreenCenterY -= 10;


        // Get corners in screen coords
        // TOP LEFT
        sX_S = (gsVIEWPORT_END_X - gsVIEWPORT_START_X) / 2;
        sY_S = (gsVIEWPORT_END_Y - gsVIEWPORT_START_Y) / 2;

        sTopLeftWorldX = sScreenCenterX - sX_S;
        sTopLeftWorldY = sScreenCenterY - sY_S;

        sTopRightWorldX = sScreenCenterX + sX_S;
        sTopRightWorldY = sScreenCenterY - sY_S;

        sBottomLeftWorldX = sScreenCenterX - sX_S;
        sBottomLeftWorldY = sScreenCenterY + sY_S;

        sBottomRightWorldX = sScreenCenterX + sX_S;
        sBottomRightWorldY = sScreenCenterY + sY_S;

        // Get angles
        // TOP LEFT CORNER FIRST
        dOpp = sTopLeftWorldY - gsTLY;
        dAdj = sTopLeftWorldX - gsTLX;

//        dAngle = (double)atan2(dAdj, dOpp);
        at1 = dAngle * 180 / Math.PI;

        if (dAngle < 0)
        {
            fOutLeft = true;
        }
        else if (dAngle > Math.PI / 2)
        {
            fOutTop = true;
        }

        // TOP RIGHT CORNER
        dOpp = sTopRightWorldY - gsTRY;
        dAdj = gsTRX - sTopRightWorldX;

//        dAngle = (double)atan2(dAdj, dOpp);
        at2 = dAngle * 180 / Math.PI;

        if (dAngle < 0)
        {
            fOutRight = true;
        }
        else if (dAngle > Math.PI / 2)
        {
            fOutTop = true;
        }


        // BOTTOM LEFT CORNER
        dOpp = gsBLY - sBottomLeftWorldY;
        dAdj = sBottomLeftWorldX - gsBLX;

//        dAngle = (double)atan2(dAdj, dOpp);
        at3 = dAngle * 180 / Math.PI;

        if (dAngle < 0)
        {
            fOutLeft = true;
        }
        else if (dAngle > Math.PI / 2)
        {
            fOutBottom = true;
        }

        // BOTTOM RIGHT CORNER
        dOpp = gsBRY - sBottomRightWorldY;
        dAdj = gsBRX - sBottomRightWorldX;

//        dAngle = (double)atan2(dAdj, dOpp);
        at4 = dAngle * 180 / Math.PI;

        if (dAngle < 0)
        {
            fOutRight = true;
        }
        else if (dAngle > Math.PI / 2)
        {
            fOutBottom = true;
        }

        //sprintf(gDebugStr, "Angles: %d %d %d %d", (int)at1, (int)at2, (int)at3, (int)at4);

        if (!fOutRight && !fOutLeft && !fOutTop && !fOutBottom)
        {
            fScrollGood = true;
        }

        // If in editor, anything goes
        // if (gfEditMode && _KeyDown(SHIFT))
        // {
        //     fScrollGood = true;
        // }

        // Reset some UI flags
        gfUIShowExitEast = false;
        gfUIShowExitWest = false;
        gfUIShowExitNorth = false;
        gfUIShowExitSouth = false;


        if (!fScrollGood)
        {
            // Force adjustment, if true
            if (fForceAdjust)
            {
                if (fOutTop)
                {
                    // Adjust screen coordinates on the Y!
//                    CorrectRenderCenter(sScreenCenterX, (int)(gsTLY + sY_S), out sNewScreenX, out sNewScreenY);
//                    FromScreenToCellCoordinates(sNewScreenX, sNewScreenY, out sTempPosX_W, out sTempPosY_W);

                    sTempRenderCenterX = sTempPosX_W;
                    sTempRenderCenterY = sTempPosY_W;
                    fScrollGood = true;
                }

                if (fOutBottom)
                {
                    // OK, Ajust this since we get rounding errors in our two different calculations.
//                    CorrectRenderCenter(sScreenCenterX, (int)(gsBLY - sY_S - 50), out sNewScreenX, out sNewScreenY);
//                    FromScreenToCellCoordinates(sNewScreenX, sNewScreenY, out sTempPosX_W, out sTempPosY_W);

                    sTempRenderCenterX = sTempPosX_W;
                    sTempRenderCenterY = sTempPosY_W;
                    fScrollGood = true;
                }

                if (fOutLeft)
                {
//                    CorrectRenderCenter((int)(gsTLX + sX_S), sScreenCenterY, out sNewScreenX, out sNewScreenY);
//                    FromScreenToCellCoordinates(sNewScreenX, sNewScreenY, out sTempPosX_W, out sTempPosY_W);

                    sTempRenderCenterX = sTempPosX_W;
                    sTempRenderCenterY = sTempPosY_W;
                    fScrollGood = true;
                }

                if (fOutRight)
                {
//                    CorrectRenderCenter((int)(gsTRX - sX_S), sScreenCenterY, out sNewScreenX, out sNewScreenY);
//                    FromScreenToCellCoordinates(sNewScreenX, sNewScreenY, out sTempPosX_W, out sTempPosY_W);

                    sTempRenderCenterX = sTempPosX_W;
                    sTempRenderCenterY = sTempPosY_W;
                    fScrollGood = true;
                }

            }
            else
            {
                if (fOutRight)
                {
                    // Check where our cursor is!
                    if (gusMouseXPos >= 639)
                    {
                        gfUIShowExitEast = true;
                    }
                }

                if (fOutLeft)
                {
                    // Check where our cursor is!
                    if (gusMouseXPos == 0)
                    {
                        gfUIShowExitWest = true;
                    }
                }

                if (fOutTop)
                {
                    // Check where our cursor is!
                    if (gusMouseYPos == 0)
                    {
                        gfUIShowExitNorth = true;
                    }
                }

                if (fOutBottom)
                {
                    // Check where our cursor is!
                    if (gusMouseYPos >= 479)
                    {
                        gfUIShowExitSouth = true;
                    }
                }

            }
        }


        if (fScrollGood)
        {
            if (!fCheckOnly)
            {
                //sprintf(gDebugStr, "Center: %d %d ", (int)gsRenderCenterX, (int)gsRenderCenterY);

                //Makesure it's a multiple of 5
                sMult = sTempRenderCenterX / CELL_X_SIZE;
                gsRenderCenterX = (sMult * CELL_X_SIZE) + (CELL_X_SIZE / 2);

                //Makesure it's a multiple of 5
                sMult = sTempRenderCenterY / CELL_X_SIZE;
                gsRenderCenterY = (sMult * CELL_Y_SIZE) + (CELL_Y_SIZE / 2);

                //gsRenderCenterX = sTempRenderCenterX;
                //gsRenderCenterY = sTempRenderCenterY;

                gsTopLeftWorldX = sTopLeftWorldX - gsTLX;
                gsTopLeftWorldY = sTopLeftWorldY - gsTLY;

                gsTopRightWorldX = sTopRightWorldX - gsTLX;
                gsTopRightWorldY = sTopRightWorldY - gsTLY;

                gsBottomLeftWorldX = sBottomLeftWorldX - gsTLX;
                gsBottomLeftWorldY = sBottomLeftWorldY - gsTLY;

                gsBottomRightWorldX = sBottomRightWorldX - gsTLX;
                gsBottomRightWorldY = sBottomRightWorldY - gsTLY;

//                SetPositionSndsVolumeAndPanning();
            }

            return true;
        }

        return false;
    }

    public static void SetRenderFlags(RenderingFlags full)
    {
        gRenderFlags |= full;
    }
}

[Flags]
public enum RenderingFlags
{
    FULL = 0x00000001,
    SHADOWS = 0x00000002,
    MARKED = 0x00000004,
    SAVEOFF = 0x00000008,
    NOZ = 0x00000010,
    ROOMIDS = 0x00000020,
    CHECKZ = 0x00000040,
    ONLYLAND = 0x00000080,
    ONLYSTRUCT = 0x00000100,
    FOVDEBUG = 0x00000200,
}

[Flags]
public enum ScrollDirection
{
    SCROLL_UP = 0x00000001,
    SCROLL_DOWN = 0x00000002,
    SCROLL_RIGHT = 0x00000004,
    SCROLL_LEFT = 0x00000008,
    SCROLL_UPLEFT = 0x00000020,
    SCROLL_UPRIGHT = 0x00000040,
    SCROLL_DOWNLEFT = 0x00000080,
    SCROLL_DOWNRIGHT = 0x00000200,
}

public enum TILES_DYNAMIC : uint
{
    // highest bit value is rendered first!
    TILES_ALL_DYNAMICS = 0x00000fff,
    CHECKFOR_INT_TILE = 0x00000400,
    LAND = 0x00000200,
    OBJECTS = 0x00000100,
    SHADOWS = 0x00000080,
    STRUCT_MERCS = 0x00000040,
    MERCS = 0x00000020,
    STRUCTURES = 0x00000010,
    ROOF = 0x00000008,
    HIGHMERCS = 0x00000004,
    ONROOF = 0x00000002,
    TOPMOST = 0x00000001,
}
