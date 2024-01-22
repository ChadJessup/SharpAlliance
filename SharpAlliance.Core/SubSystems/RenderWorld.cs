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
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core.SubSystems;

public class RenderWorld : IDisposable
{
    public int fSelectMode;

    private const int NUM_ITEM_CYCLE_COLORS = 60;

    public static TILES_DYNAMIC uiLayerUsedFlags = (TILES_DYNAMIC)0xffffffff;
    private static Rgba32 gsLobOutline;
    private static Rgba32 gsThrowOutline;
    private static Rgba32 gsGiveOutline;
    private static Rgba32 gusNormalItemOutlineColor;
    private static Rgba32 gusYellowItemOutlineColor;
    private static Rgba32[] us16BPPItemCycleWhiteColors = new Rgba32[NUM_ITEM_CYCLE_COLORS];
    private static Rgba32[] us16BPPItemCycleRedColors = new Rgba32[NUM_ITEM_CYCLE_COLORS];
    private static Rgba32[] us16BPPItemCycleYellowColors = new Rgba32[NUM_ITEM_CYCLE_COLORS];

    private static byte[] ubRGBItemCycleWhiteColors =
{
    25,     25,     25,
    50,     50,     50,
    75,   75,   75,
    100,    100,    100,
    125,    125,    125,
    150,    150,    150,
    175,    175,    175,
    200,    200,    200,
    225,    225,    225,
    250,    250,    250,

    250,    250,    250,
    225,    225,    225,
    200,    200,    200,
    175,    175,    175,
    150,    150,    150,
    125,    125,    125,
    100,    100,    100,
    75,   75,   75,
    50,     50,     50,
    25,     25,     25,

    25,     25,     25,
    50,     50,     50,
    75,   75,   75,
    100,    100,    100,
    125,    125,    125,
    150,    150,    150,
    175,    175,    175,
    200,    200,    200,
    225,    225,    225,
    250,    250,    250,

    250,    250,    250,
    225,    225,    225,
    200,    200,    200,
    175,    175,    175,
    150,    150,    150,
    125,    125,    125,
    100,    100,    100,
    75,   75,   75,
    50,     50,     50,
    25,     25,     25,

    25,     25,     25,
    50,     50,     50,
    75,   75,   75,
    100,    100,    100,
    125,    125,    125,
    150,    150,    150,
    175,    175,    175,
    200,    200,    200,
    225,    225,    225,
    250,    250,    250,

    250,    250,    250,
    225,    225,    225,
    200,    200,    200,
    175,    175,    175,
    150,    150,    150,
    125,    125,    125,
    100,    100,    100,
    75,   75,   75,
    50,     50,     50,
    25,     25,     25

};


    private static byte[] ubRGBItemCycleRedColors =
    {
    25,     0,      0,
    50,     0,      0,
    75,   0,        0,
    100,    0,      0,
    125,    0,      0,
    150,    0,      0,
    175,    0,      0,
    200,    0,      0,
    225,    0,      0,
    250,    0,      0,

    250,    0,      0,
    225,    0,      0,
    200,    0,      0,
    175,    0,      0,
    150,    0,      0,
    125,    0,      0,
    100,    0,      0,
    75,   0,        0,
    50,     0,      0,
    25,     0,      0,

    25,     0,      0,
    50,     0,      0,
    75,   0,        0,
    100,    0,      0,
    125,    0,      0,
    150,    0,      0,
    175,    0,      0,
    200,    0,      0,
    225,    0,      0,
    250,    0,      0,

    250,    0,      0,
    225,    0,      0,
    200,    0,      0,
    175,    0,      0,
    150,    0,      0,
    125,    0,      0,
    100,    0,      0,
    75,   0,        0,
    50,     0,      0,
    25,     0,      0,

    25,     0,      0,
    50,     0,      0,
    75,   0,        0,
    100,    0,      0,
    125,    0,      0,
    150,    0,      0,
    175,    0,      0,
    200,    0,      0,
    225,    0,      0,
    250,    0,      0,

    250,    0,      0,
    225,    0,      0,
    200,    0,      0,
    175,    0,      0,
    150,    0,      0,
    125,    0,      0,
    100,    0,      0,
    75,   0,        0,
    50,     0,      0,
    25,     0,      0,

};

    private static byte[] ubRGBItemCycleYellowColors =
    {
    25,     25,     0,
    50,     50,     0,
    75,      75,    0,
    100,    100,    0,
    125,    125,    0,
    150,    150,    0,
    175,    175,    0,
    200,    200,    0,
    225,    225,    0,
    250,    250,    0,

    250,    250,    0,
    225,    225,    0,
    200,    200,    0,
    175,    175,    0,
    150,    150,    0,
    125,    125,    0,
    100,    100,    0,
    75,     75,     0,
    50,     50,     0,
    25,     25,     0,

    25,     25,     0,
    50,     50,     0,
    75,      75,    0,
    100,    100,    0,
    125,    125,    0,
    150,    150,    0,
    175,    175,    0,
    200,    200,    0,
    225,    225,    0,
    250,    250,    0,

    250,    250,    0,
    225,    225,    0,
    200,    200,    0,
    175,    175,    0,
    150,    150,    0,
    125,    125,    0,
    100,    100,    0,
    75,   75,   0,
    50,     50,     0,
    25,     25,     0,

    25,     25,     0,
    50,     50,     0,
    75,   75,   0,
    100,    100,    0,
    125,    125,    0,
    150,    150,    0,
    175,    175,    0,
    200,    200,    0,
    225,    225,    0,
    250,    250,    0,

    250,    250,    0,
    225,    225,    0,
    200,    200,    0,
    175,    175,    0,
    150,    150,    0,
    125,    125,    0,
    100,    100,    0,
    75,   75,   0,
    50,     50,     0,
    25,     25,     0,

};

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

        gfScrollInertia = 0;
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
        int sTempPosX_W = 0;
        int sTempPosY_W = 0;


        // For debug text for all 4 angles
        double at1, at2, at3, at4;

        int sX_S, sY_S;
        int sScreenCenterX = 0, sScreenCenterY = 0;
        int sDistToCenterY;
        int sDistToCenterX;
        int sNewScreenX, sNewScreenY;
        int sMult;


        //Makesure it's a multiple of 5
        sMult = sTempRenderCenterX / CELL_X_SIZE;
        sTempRenderCenterX = (sMult * CELL_X_SIZE) + (CELL_X_SIZE / 2);

        //Makesure it's a multiple of 5
        sMult = (int)sTempRenderCenterY / CELL_X_SIZE;
        sTempRenderCenterY = (sMult * CELL_Y_SIZE) + (CELL_Y_SIZE / 2);


        // Find the diustance from render center to true world center
        sDistToCenterX = sTempRenderCenterX - gCenterWorldX;
        sDistToCenterY = (sTempRenderCenterY - gCenterWorldY);

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
                sMult = (int)sTempRenderCenterY / CELL_X_SIZE;
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

    internal static void InitRenderParams(int ubRestrictionID)
    {
        int gsTilesX, gsTilesY;
        int  cnt, cnt2;
        double dWorldX, dWorldY;

        switch (ubRestrictionID)
        {
            case 0:     //Default!

                gTopLeftWorldLimitX = CELL_X_SIZE;
                gTopLeftWorldLimitY = ((WORLD_ROWS / 2) * CELL_X_SIZE);

                gTopRightWorldLimitX = (WORLD_COLS / 2) * CELL_Y_SIZE;
                gTopRightWorldLimitY = CELL_X_SIZE;

                gBottomLeftWorldLimitX = ((WORLD_COLS / 2) * CELL_Y_SIZE);
                gBottomLeftWorldLimitY = (int)(WORLD_ROWS * CELL_Y_SIZE);

                gBottomRightWorldLimitX = (WORLD_COLS * CELL_Y_SIZE);
                gBottomRightWorldLimitY = (int)((WORLD_ROWS / 2) * CELL_X_SIZE);
                break;

            case 1:     // BAEMENT LEVEL 1

                gTopLeftWorldLimitX = (3 * WORLD_ROWS / 10) * CELL_X_SIZE;
                gTopLeftWorldLimitY = (int)((WORLD_ROWS / 2) * CELL_X_SIZE);

                gTopRightWorldLimitX = (WORLD_ROWS / 2) * CELL_X_SIZE;
                gTopRightWorldLimitY = (int)((3 * WORLD_COLS / 10) * CELL_X_SIZE);

                gBottomLeftWorldLimitX = (WORLD_ROWS / 2) * CELL_X_SIZE;
                gBottomLeftWorldLimitY = (int)((7 * WORLD_COLS / 10) * CELL_X_SIZE);

                gBottomRightWorldLimitX = (7 * WORLD_ROWS / 10) * CELL_X_SIZE;
                gBottomRightWorldLimitY = (int)((WORLD_ROWS / 2) * CELL_X_SIZE);
                break;

        }

        gCenterWorldX = (WORLD_ROWS) / 2 * CELL_X_SIZE;
        gCenterWorldY = ((WORLD_COLS) / 2 * CELL_Y_SIZE);

        // Convert Bounding box into screen coords
        IsometricUtils.FromCellToScreenCoordinates(gTopLeftWorldLimitX, gTopLeftWorldLimitY, out gsTLX, out gsTLY);
        IsometricUtils.FromCellToScreenCoordinates(gTopRightWorldLimitX, gTopRightWorldLimitY, out gsTRX, out gsTRY);
        IsometricUtils.FromCellToScreenCoordinates(gBottomLeftWorldLimitX, gBottomLeftWorldLimitY, out gsBLX, out gsBLY);
        IsometricUtils.FromCellToScreenCoordinates(gBottomRightWorldLimitX, gBottomRightWorldLimitY, out gsBRX, out gsBRY);
        IsometricUtils.FromCellToScreenCoordinates(gCenterWorldX, gCenterWorldY, out gsCX, out gsCY);

        // Adjust for interface height tabbing!
        gsTLY += ROOF_LEVEL_HEIGHT;
        gsTRY += ROOF_LEVEL_HEIGHT;
        gsCY += (ROOF_LEVEL_HEIGHT / 2);

        // Take these spaning distances and determine # tiles spaning
        gsTilesX = (gsTRX - gsTLX) / WORLD_TILE_X;
        gsTilesY = (gsBRY - gsTRY) / WORLD_TILE_Y;

//        DebugMsg(TOPIC_JA2, DBG_LEVEL_0, String("World Screen Width %d Height %d", (gsTRX - gsTLX), (gsBRY - gsTRY)));


        // Determine scale factors
        // First scale world screen coords for VIEWPORT ratio
        dWorldX = (gsTRX - gsTLX);
        dWorldY = (gsBRY - gsTRY);

        gdScaleX = RadarScreen.RADAR_WINDOW_WIDTH / dWorldX;
        gdScaleY = RadarScreen.RADAR_WINDOW_HEIGHT / dWorldY;

        for (cnt = 0, cnt2 = 0; cnt2 < NUM_ITEM_CYCLE_COLORS; cnt += 3, cnt2++)
        {
            us16BPPItemCycleWhiteColors[cnt2] = (FROMRGB(ubRGBItemCycleWhiteColors[cnt], ubRGBItemCycleWhiteColors[cnt + 1], ubRGBItemCycleWhiteColors[cnt + 2]));
            us16BPPItemCycleRedColors[cnt2] = (FROMRGB(ubRGBItemCycleRedColors[cnt], ubRGBItemCycleRedColors[cnt + 1], ubRGBItemCycleRedColors[cnt + 2]));
            us16BPPItemCycleYellowColors[cnt2] = (FROMRGB(ubRGBItemCycleYellowColors[cnt], ubRGBItemCycleYellowColors[cnt + 1], ubRGBItemCycleYellowColors[cnt + 2]));
        }

        gsLobOutline = (FROMRGB(10, 200, 10));
        gsThrowOutline = (FROMRGB(253, 212, 10));
        gsGiveOutline = (FROMRGB(253, 0, 0));

        gusNormalItemOutlineColor = (FROMRGB(255, 255, 255));
        gusYellowItemOutlineColor = (FROMRGB(255, 255, 0));

        // NOW GET DISTANCE SPANNING WORLD LIMITS IN WORLD COORDS
        //FromScreenToCellCoordinates( ( gTopRightWorldLimitX - gTopLeftWorldLimitX ), ( gTopRightWorldLimitY - gTopLeftWorldLimitY ), &gsWorldSpanX, &gsWorldSpanY );

        // CALCULATE 16BPP COLORS FOR ITEMS
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
