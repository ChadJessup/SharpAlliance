using System;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SixLabors.ImageSharp;

namespace SharpAlliance.Core;

internal class RadarScreen
{
    // RADAR WINDOW DEFINES
    private const int RADAR_WINDOW_X = 543;
    private const int RADAR_WINDOW_TM_Y = INTERFACE_START_Y + 13;
    private const int RADAR_WINDOW_SM_Y = INV_INTERFACE_START_Y + 13;
    private const int RADAR_WINDOW_WIDTH = 88;
    private const int RADAR_WINDOW_HEIGHT = 44;
    private static int gsRadarX;
    private static int gsRadarY;
    private static bool fRenderRadarScreen;

    internal static bool InitRadarScreen()
    {
        // Add region for radar
        MouseSubSystem.MSYS_DefineRegion(
            gRadarRegion,
            new Rectangle(RADAR_WINDOW_X, RADAR_WINDOW_TM_Y, RADAR_WINDOW_X + RADAR_WINDOW_WIDTH, RADAR_WINDOW_TM_Y + RADAR_WINDOW_HEIGHT),
            MSYS_PRIORITY.HIGHEST, 0,
            RadarRegionMoveCallback,
            RadarRegionButtonCallback);

        // Add region
        MouseSubSystem.MSYS_AddRegion(ref gRadarRegion);

        //disable the radar map
        MouseSubSystem.MSYS_DisableRegion(ref gRadarRegion);

        gsRadarX = RADAR_WINDOW_X;
        gsRadarY = RADAR_WINDOW_TM_Y;

        return true;
    }

    private static void RadarRegionButtonCallback(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        int sRadarX, sRadarY;

        // check if we are allowed to do anything?
        if (fRenderRadarScreen == false)
        {
            return;
        }

        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            if (!Overhead.InOverheadMap())
            {
                // Use relative coordinates to set center of viewport
                sRadarX = pRegion.RelativeMousePos.X - (RADAR_WINDOW_WIDTH / 2);
                sRadarY = pRegion.RelativeMousePos.Y - (RADAR_WINDOW_HEIGHT / 2);

                AdjustWorldCenterFromRadarCoords(sRadarX, sRadarY);
            }
            else
            {
                Overhead.KillOverheadMap();
            }
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_DWN))
        {
            if (!Overhead.InOverheadMap())
            {
                Overhead.GoIntoOverheadMap();
            }
            else
            {
                Overhead.KillOverheadMap();
            }
        }
    }

    private static void RadarRegionMoveCallback(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        int sRadarX, sRadarY;

        // check if we are allowed to do anything?
        if (fRenderRadarScreen == false)
        {
            return;
        }

        if (iReason == MSYS_CALLBACK_REASON.MOVE)
        {
            if (pRegion.ButtonState.HasFlag(ButtonMasks.MSYS_LEFT_BUTTON))
            {
                // Use relative coordinates to set center of viewport
                sRadarX = pRegion.RelativeMousePos.X - (RADAR_WINDOW_WIDTH / 2);
                sRadarY = pRegion.RelativeMousePos.Y - (RADAR_WINDOW_HEIGHT / 2);

                AdjustWorldCenterFromRadarCoords(sRadarX, sRadarY);

                RenderWorld.SetRenderFlags(RenderingFlags.FULL);
            }
        }
    }

    private static void AdjustWorldCenterFromRadarCoords(int sRadarX, int sRadarY)
    {
        int sScreenX, sScreenY;
        int sTempX_W, sTempY_W;
        int sNewCenterWorldX, sNewCenterWorldY;
        int sNumXSteps, sNumYSteps;

        // Use radar scale values to get screen values, then convert ot map values, rounding to nearest middle tile
        sScreenX = (int)(sRadarX / gdScaleX);
        sScreenY = (int)(sRadarY / gdScaleY);

        // Adjust to viewport start!
        sScreenX -= ((gsVIEWPORT_END_X - gsVIEWPORT_START_X) / 2);
        sScreenY -= ((gsVIEWPORT_END_Y - gsVIEWPORT_START_Y) / 2);

        //Make sure these coordinates are multiples of scroll steps
        sNumXSteps = sScreenX / SCROLL_X_STEP;
        sNumYSteps = sScreenY / SCROLL_Y_STEP;

        sScreenX = (sNumXSteps * SCROLL_X_STEP);
        sScreenY = (sNumYSteps * SCROLL_Y_STEP);

        // Adjust back
        sScreenX += ((gsVIEWPORT_END_X - gsVIEWPORT_START_X) / 2);
        sScreenY += ((gsVIEWPORT_END_Y - gsVIEWPORT_START_Y) / 2);

        // Subtract world center
        //sScreenX += gsCX;
        //sScreenY += gsCY;

        // Convert these into world coordinates
        IsometricUtils.FromScreenToCellCoordinates(sScreenX, sScreenY, out sTempX_W, out sTempY_W);

        // Adjust these to world center
        sNewCenterWorldX = (gCenterWorldX + sTempX_W);
        sNewCenterWorldY = ((int)gCenterWorldY + sTempY_W);

        RenderWorld.SetRenderCenter(sNewCenterWorldX, (MAP_ROW)sNewCenterWorldY);
    }
}
