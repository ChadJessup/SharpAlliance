using System;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Screens;

/// <summary>
/// The small pictures around the team list and the info panel: the sleep icon in the list and clicking it to put a
/// merc to sleep or wake them, the icons for contract / life insurance / medical deposit in the info panel, and the
/// health, breath and morale bars (DisplayIconsForMercsAsleep, TeamListSleepRegionBtnCallBack,
/// RenderIconsForUpperLeftCornerPiece and DrawCharBars in mapscreen.c).
/// TODO(port): the checks for whether a merc can sleep or be woken (CanCharacterSleep / CanCharacterBeAwakened), putting
/// the other selected mercs to sleep, the bandage part of the health bar and the brown backdrop and maximum-breath part
/// of the breath bar.
/// </summary>
public partial class MapScreen
{
    private const int SLEEP_X = 123;
    private const int SLEEP_WIDTH = 142 - SLEEP_X;
    private const int SLEEP_ICON_X = 125;

    private const int CHAR_ICON_CONTRACT_Y = 64;
    private const int CHAR_ICON_X = 187;
    private const int CHAR_ICON_SPACING = 13;

    private const int BAR_INFO_X = 66;
    private const int BAR_INFO_Y = 61;
    private const int BAR_WIDTH = 3;
    private const int BAR_HEIGHT = 42;

    /// <summary>Whether this merc's sleep status can be changed at all (CanChangeSleepStatusForSoldier).</summary>
    private static bool CanChangeSleepStatusForSoldier(SOLDIERTYPE pSoldier)
    {
        // a vehicle, robot, a merc in transit or a POW
        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE) || AM_A_ROBOT(pSoldier)
            || pSoldier.bAssignment == Assignment.IN_TRANSIT || pSoldier.bAssignment == Assignment.ASSIGNMENT_POW)
        {
            return false;
        }

        // dead
        if (pSoldier.bLife <= 0 || pSoldier.bAssignment == Assignment.ASSIGNMENT_DEAD)
        {
            return false;
        }

        // this merc may be able to sleep or wake up: the player is allowed to click and find out
        return true;
    }

    /// <summary>A moon icon in the sleep column for every merc who is asleep.</summary>
    private void DisplayIconsForMercsAsleep()
    {
        var icon = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\sleepicon.sti"));

        for (int iCounter = 0; iCounter < MAX_CHARACTER_COUNT; iCounter++)
        {
            if (!gCharactersList[iCounter].fValid)
            {
                continue;
            }

            var pSoldier = MercPtrs[gCharactersList[iCounter].usSolID];
            if (pSoldier.bActive && pSoldier.fMercAsleep && CanChangeSleepStatusForSoldier(pSoldier))
            {
                video.BltVideoObject(SurfaceType.FRAME_BUFFER, icon, 0, SLEEP_ICON_X, Y_START + iCounter * RowHeight, VO_BLT.SRCTRANSPARENCY);
            }
        }
    }

    /// <summary>Clicking in the sleep column puts a merc to sleep, or wakes them up.</summary>
    private static void ToggleSleep(int row)
    {
        var pSoldier = MercPtrs[gCharactersList[row].usSolID];
        if (!CanChangeSleepStatusForSoldier(pSoldier))
        {
            return;
        }

        // TODO(port): CanCharacterSleep / CanCharacterBeAwakened (assignments that can't sleep, enemies about, forced awake)
        pSoldier.fMercAsleep = !pSoldier.fMercAsleep;

        fCharacterInfoPanelDirty = true;
        fTeamPanelDirty = true;
    }

    /// <summary>Icons next to the contract, salary and deposit lines of the info panel (RenderIconsForUpperLeftCornerPiece).</summary>
    private void RenderIconsForUpperLeftCornerPiece(SOLDIERTYPE pSoldier)
    {
        var icons = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\top_left_corner_icons.sti"));

        // if merc is an AIM merc: finite contract length icon
        if (pSoldier.ubWhatKindOfMercAmI == MERC_TYPE.AIM_MERC)
        {
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, icons, 0, CHAR_ICON_X, CHAR_ICON_CONTRACT_Y, VO_BLT.SRCTRANSPARENCY);
        }

        // if merc has life insurance
        if ((int)pSoldier.usLifeInsurance > 0)
        {
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, icons, 2, CHAR_ICON_X, CHAR_ICON_CONTRACT_Y + CHAR_ICON_SPACING, VO_BLT.SRCTRANSPARENCY);
        }

        // if merc has a medical deposit
        if (pSoldier.usMedicalDeposit > 0)
        {
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, icons, 1, CHAR_ICON_X, CHAR_ICON_CONTRACT_Y + 2 * CHAR_ICON_SPACING, VO_BLT.SRCTRANSPARENCY);
        }
    }

    /// <summary>The health, breath and morale bars left of the stats (DrawCharBars).</summary>
    private void DrawCharBars(SOLDIERTYPE pSoldier)
    {
        // skip POWs and dead mercs
        if (pSoldier.bLife == 0 || pSoldier.bAssignment == Assignment.ASSIGNMENT_DEAD || pSoldier.bAssignment == Assignment.ASSIGNMENT_POW)
        {
            return;
        }

        // current health
        DrawBar(BAR_INFO_X, pSoldier.bLife, new Rgba32(108, 12, 12, 255), new Rgba32(200, 0, 0, 255));

        // current energy
        if (!AM_A_ROBOT(pSoldier))
        {
            DrawBar(BAR_INFO_X + 6, pSoldier.bBreath, new Rgba32(8, 12, 118, 255), new Rgba32(8, 12, 160, 255));
        }

        // vehicles and the robot have no morale
        if (!pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE) && !AM_A_ROBOT(pSoldier))
        {
            DrawBar(BAR_INFO_X + 12, pSoldier.bMorale, new Rgba32(8, 112, 12, 255), new Rgba32(8, 180, 12, 255));
        }
    }

    /// <summary>One bar, three pixels wide: a shadow line, the bar line and another shadow line, growing up from the bottom.</summary>
    private void DrawBar(int x, int percent, Rgba32 shadow, Rgba32 bar)
    {
        int height = Math.Clamp(percent, 0, 100) * BAR_HEIGHT / 100;
        if (height <= 0)
        {
            return;
        }

        int top = BAR_INFO_Y - height;
        video.ColorFillVideoSurfaceArea(SurfaceType.FRAME_BUFFER, new Rectangle(x, top, 1, height + 1), shadow);
        video.ColorFillVideoSurfaceArea(SurfaceType.FRAME_BUFFER, new Rectangle(x + 1, top, 1, height + 1), bar);
        video.ColorFillVideoSurfaceArea(SurfaceType.FRAME_BUFFER, new Rectangle(x + 2, top, 1, height + 1), shadow);
    }
}
