using System;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.SubSystems;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Screens;

/// <summary>
/// The part of mapscreen.c that runs the screen: MapScreenHandle and the entry/exit set-up it does.
/// TODO(port): what is here is the map view only. Still to come: the character (team) list and info panel,
/// the bottom panel (messages, time compression buttons), the border buttons, assignments, contracts, movement
/// plotting, the helicopter, militia, sector inventory and the popups.
/// </summary>
public partial class MapScreen
{
    private const int MAP_BOTTOM_Y = 359;
    private const int MAP_INDEX_COLOR_VALUE = 32 * 4 - 9;
    private const int MAP_HORT_INDEX_X = 292;
    private const int MAP_HORT_INDEX_Y = 10;
    private const int MAP_VERT_INDEX_X = 273;
    private const int MAP_VERT_INDEX_Y = 31;
    private const int MAP_HORT_HEIGHT = 18;
    private const int MAX_VIEW_SECTORS = 16;
    private const int MAP_LEVEL_STRING_X = 432;
    private const int MAP_LEVEL_STRING_Y = 305;
    private const int CLOCK_X = 554;
    private const int CLOCK_Y = 459;

    // position of the town names on the map: 10 * the sector position of the centre of the town
    private static readonly (int x, int y)[] pTownPoints =
    [
        (0, 0),
        (90, 10),   // Omerta
        (125, 40),  // Drassen
        (130, 90),  // Alma
        (15, 80),   // Grumm
        (85, 100),  // Tixa
        (95, 70),   // Cambria
        (45, 40),   // San Mona
        (55, 90),   // Estoni
        (35, 110),  // Orta
        (110, 120), // Balime
        (45, 150),  // Meduna
        (15, 20),   // Chitzena
    ];

    private bool fMapScreenEntered;
    private Image<Rgba32>? bigMapHalf;
    private HVOBJECT? mapBorder;
    private HVOBJECT? mapBottomPanel;
    private readonly MOUSE_REGION mapViewRegion = new("Map View");

    public ValueTask Activate()
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>The set-up MapScreenHandle does the first time through.</summary>
    private void EnterMapScreen()
    {
        fMapScreenEntered = true;
        fInMapMode = true;
        guiCurrentScreen = ScreenName.MAP_SCREEN;

        guiTacticalInterfaceFlags |= INTERFACE.MAPSCREEN;

        // if arrival sector is invalid, reset to A9
        if ((gsMercArriveSectorX < 1) || ((int)gsMercArriveSectorY < 1) || (gsMercArriveSectorX > 16) || ((int)gsMercArriveSectorY > 16))
        {
            gsMercArriveSectorX = 9;
            gsMercArriveSectorY = MAP_ROW.A;
        }

        gfInConfirmMapMoveMode = false;
        gfInChangeArrivalSectorMode = false;

        // stop time compression while entering the map, as in the original
        GameClock.StopTimeCompression();

        // load the graphics
        mapBorder = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\MBS.sti"));
        mapBottomPanel = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\map_screen_bottom.sti"));

        var bigMap = video.GetVideoObject("INTERFACE\\b_map.pcx");
        bigMapHalf?.Dispose();
        bigMapHalf = bigMap.Images[0].Clone(ctx => ctx.Resize(bigMap.Images[0].Width / 2, bigMap.Images[0].Height / 2));


        // change selected sector/level if necessary
        if (GameInit.AnyMercsHired() == false)
        {
            // select starting sector (A9 - Omerta)
            ChangeSelectedMapSector(9, MAP_ROW.A, 0);
        }
        else if ((gWorldSectorX > 0) && (gWorldSectorY > 0) && (gbWorldSectorZ != -1))
        {
            ChangeSelectedMapSector(gWorldSectorX, (MAP_ROW)gWorldSectorY, gbWorldSectorZ);
        }
        else if ((MapScreenInterfaceMap.sSelMapX == 0) || ((int)MapScreenInterfaceMap.sSelMapY == 0) || (MapScreenInterfaceMap.iCurrentMapSectorZ == -1))
        {
            ChangeSelectedMapSector(9, MAP_ROW.A, 0);
        }

        // the map view takes clicks to select a sector
        MouseSubSystem.MSYS_DefineRegion(
            mapViewRegion,
            new Rectangle(
                MAP_VIEW_START_X + MAP_GRID_X,
                MAP_VIEW_START_Y + MAP_GRID_Y,
                MAP_VIEW_WIDTH - 1,
                MAP_VIEW_HEIGHT - 1 + 8),
            MSYS_PRIORITY.HIGH - 3,
            CURSOR.NORMAL,
            MSYS_NO_CALLBACK,
            MapViewRegionCallback);

        LoadCharacters();
        CreateMouseRegionsForTeamList();
        CreateBottomInterface();
        CreateButtonsForMapBorder();
        AddTeamPanelSortButtonsForMapScreen();

        fMapPanelDirty = true;
        fTeamPanelDirty = true;
        fCharacterInfoPanelDirty = true;
        fMapScreenBottomDirty = true;
    }

    private void LeaveMapScreen()
    {
        if (!fMapScreenEntered)
        {
            return;
        }

        fMapScreenEntered = false;
        fInMapMode = false;
        MouseSubSystem.MSYS_RemoveRegion(mapViewRegion);
        RemoveMouseRegionsForTeamList();
        DestroyBottomInterface();
        DestroyBorderButtons();
        RemoveTeamPanelSortButtonsForMapScreen();
        guiTacticalInterfaceFlags &= ~INTERFACE.MAPSCREEN;
    }

    private static void MapViewRegionCallback(ref MOUSE_REGION region, MSYS_CALLBACK_REASON reason)
    {
        if (!reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            return;
        }

        // the sector under the mouse
        int sMapX = (region.MousePos.X - MAP_VIEW_START_X) / MAP_GRID_X;
        int sMapY = (region.MousePos.Y - MAP_VIEW_START_Y) / MAP_GRID_Y;

        if (sMapX < 1 || sMapX > MAX_VIEW_SECTORS || sMapY < 1 || sMapY > MAX_VIEW_SECTORS)
        {
            return;
        }

        ChangeSelectedMapSector(sMapX, (MAP_ROW)sMapY, MapScreenInterfaceMap.iCurrentMapSectorZ);
    }

    /// <summary>One frame of the map screen (MapScreenHandle).</summary>
    public async ValueTask<ScreenName> Handle()
    {
        if (!fMapScreenEntered)
        {
            EnterMapScreen();
        }

        DrawMapScreen();

        video.EndFrameBufferRender();

        // a button on the bottom panel asked to leave for another screen
        if (screenToLeaveFor is ScreenName leaveFor)
        {
            screenToLeaveFor = null;

            if (leaveFor == ScreenName.LAPTOP_SCREEN)
            {
                // the laptop opens with an animation out of the map screen picture, which it takes from the extra buffer
                video.BlitBufferToBuffer(SurfaceType.FRAME_BUFFER, guiEXTRABUFFER, new(0, 0, 640, 480));
                gfStartMapScreenToLaptopTransition = true;
            }

            await screens.SetPendingNewScreen(leaveFor);
        }

        return ScreenName.MAP_SCREEN;
    }

    private void DrawMapScreen()
    {
        video.ColorFillVideoSurfaceArea(SurfaceType.FRAME_BUFFER, new(0, 0, 640, 480), new Rgba32(8, 8, 8, 255));

        var frame = video.Surfaces[SurfaceType.FRAME_BUFFER];

        // the map: the big map is drawn at half size
        if (bigMapHalf is not null && MapScreenInterfaceMap.iCurrentMapSectorZ == 0)
        {
            video.Blt16BPPTo16BPP(
                frame,
                bigMapHalf,
                new(MAP_VIEW_START_X + 1, MAP_VIEW_START_Y),
                new(0, 0),
                new(Math.Min(bigMapHalf.Width, 640 - MAP_VIEW_START_X - 1), Math.Min(bigMapHalf.Height, 480 - MAP_VIEW_START_Y)));

            ShadeUnvisitedSectors(frame);
            if (MapScreenInterfaceBorder.fShowTownFlag)
            {
                ShowTownText();
            }

            ShowMinesAndSamSites();

            if (MapScreenInterfaceBorder.fShowMineFlag)
            {
                ShowMineText();
            }

            if (MapScreenInterfaceBorder.fShowTeamFlag)
            {
                ShowTeamOnMap();
            }
        }

        // the border goes over the edges of the map
        if (mapBorder is not null)
        {
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, mapBorder, 0, MAP_BORDER_X, MAP_BORDER_Y, VO_BLT.SRCTRANSPARENCY);
        }

        if (mapBottomPanel is not null)
        {
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, mapBottomPanel, 0, 0, MAP_BOTTOM_Y, VO_BLT.SRCTRANSPARENCY);
        }

        LoadCharacters();
        DisplayCharacterInfoPanel();
        DisplayCharacterList();

        DrawMapIndexBigMap();
        DrawHighlightedSector();
        DrawSelectedSector();
        DisplayLevelString();
        DrawClock();
        DisplayMapScreenBottom();
        // the whole frame is redrawn every time, so every button has to be drawn every time
        ButtonSubSystem.MarkButtonsDirty(buttonList);
        ButtonSubSystem.RenderButtons(buttonList);

        video.InvalidateRegion(0, 0, 640, 480);
    }

    private static (int x, int y) ScreenXYFromMapXY(int sMapX, int sMapY)
        => (sMapX * MAP_GRID_X + MAP_VIEW_START_X, sMapY * MAP_GRID_Y + MAP_VIEW_START_Y);

    /// <summary>Darkens the sectors the player has not visited (ShadeMapElem with MAP_SHADE_BLACK).</summary>
    private static void ShadeUnvisitedSectors(Image<Rgba32> frame)
    {
        for (int x = 1; x < MAP_WORLD_X - 1; x++)
        {
            for (int y = 1; y < MAP_WORLD_Y - 1; y++)
            {
                if (TacticalSaveSubSystem.GetSectorFlagStatus(x, (MAP_ROW)y, MapScreenInterfaceMap.iCurrentMapSectorZ, SF.ALREADY_VISITED))
                {
                    continue;
                }

                // the big map is blitted at MAP_VIEW_START_X + 1
                var (sx, sy) = ScreenXYFromMapXY(x, y);
                sx += 1;

                var area = Rectangle.FromLTRB(sx, sy, sx + MAP_GRID_X - 1, sy + MAP_GRID_Y - 1);
                Blitters.Blt16BPPBufferShadowRect(frame, area);
                Blitters.Blt16BPPBufferShadowRect(frame, area);
            }
        }
    }

    /// <summary>Town names on the map (ShowTownText).</summary>
    private static void ShowTownText()
    {
        FontSubSystem.SetFont(FontStyle.BLOCKFONT2);
        FontSubSystem.SetFontBackground(FontColor.FONT_MCOLOR_BLACK);

        for (TOWNS bTown = FIRST_TOWN; bTown < TOWNS.NUM_TOWNS; bTown++)
        {
            // skip Orta/Tixa until found
            if ((bTown == TOWNS.ORTA && !fFoundOrta) || (bTown == TOWNS.TIXA && !fFoundTixa))
            {
                continue;
            }

            if (!pTownNames.TryGetValue(bTown, out string? name) || (int)bTown >= pTownPoints.Length)
            {
                continue;
            }

            int usX = MAP_VIEW_START_X + MAP_GRID_X + (pTownPoints[(int)bTown].x * MAP_GRID_X) / 10;
            int usY = MAP_VIEW_START_Y + MAP_GRID_Y + ((pTownPoints[(int)bTown].y * MAP_GRID_Y) / 10) + 1;

            if (usX > MAP_VIEW_START_X + MAP_VIEW_WIDTH || usX < MAP_VIEW_START_X || usY < MAP_VIEW_START_Y || usY > MAP_VIEW_START_Y + MAP_VIEW_HEIGHT)
            {
                continue;
            }

            // don't show the loyalty until loyalty tracking for that town has started
            string loyalty = string.Empty;
            bool loyaltyTooLowToTrainMilitia = false;
            if (gTownLoyalty.TryGetValue(bTown, out var townLoyalty) && townLoyalty.fStarted
                && gfTownUsesLoyalty.TryGetValue(bTown, out bool usesLoyalty) && usesLoyalty)
            {
                loyalty = $"{townLoyalty.ubRating}% Loyal";
                loyaltyTooLowToTrainMilitia = townLoyalty.ubRating < MIN_RATING_TO_TRAIN_TOWN && MilitiaTrainingAllowedInTown(bTown);
            }

            // red for low loyalty, green otherwise
            FontSubSystem.SetFontForeground(loyaltyTooLowToTrainMilitia ? FontColor.FONT_MCOLOR_RED : FontColor.FONT_MCOLOR_LTGREEN);

            int nameX = usX - FontSubSystem.StringPixLength(name, FontStyle.BLOCKFONT2) / 2;
            mprintf(nameX, usY, name);

            if (loyalty.Length > 0)
            {
                // centred under the name, but not past the left edge of the map
                FontSubSystem.FindFontCenterCoordinates(nameX, usY, FontSubSystem.StringPixLength(name, FontStyle.BLOCKFONT2), 0, loyalty, FontStyle.BLOCKFONT2, out int secondX, out _);
                secondX = Math.Max(secondX, MAP_VIEW_START_X + 23);
                mprintf(secondX, usY + FontSubSystem.GetFontHeight(FontStyle.BLOCKFONT2), loyalty);
            }
        }
    }

    private static bool MilitiaTrainingAllowedInTown(TOWNS bTownId) => bTownId switch
    {
        TOWNS.DRASSEN or TOWNS.ALMA or TOWNS.GRUMM or TOWNS.CAMBRIA or TOWNS.BALIME or TOWNS.MEDUNA or TOWNS.CHITZENA => true,
        _ => false,
    };

    /// <summary>The sector letters and numbers around the map (DrawMapIndexBigMap).</summary>
    private static void DrawMapIndexBigMap()
    {
        FontSubSystem.SetFont(FontStyle.BLOCKFONT2);
        FontSubSystem.SetFontBackground(FontColor.FONT_MCOLOR_BLACK);

        for (int iCount = 1; iCount <= MAX_VIEW_SECTORS; iCount++)
        {
            string hort = iCount.ToString();
            string vert = ((char)('A' + iCount - 1)).ToString();

            FontSubSystem.SetFontForeground(iCount == MapScreenInterfaceMap.sSelMapX ? FontColor.FONT_YELLOW : (FontColor)MAP_INDEX_COLOR_VALUE);
            FontSubSystem.FindFontCenterCoordinates(MAP_HORT_INDEX_X + (iCount - 1) * MAP_GRID_X, MAP_HORT_INDEX_Y, MAP_GRID_X, MAP_HORT_HEIGHT, hort, FontStyle.BLOCKFONT2, out int usX, out int usY);
            mprintf(usX, usY, hort);

            FontSubSystem.SetFontForeground((int)MapScreenInterfaceMap.sSelMapY == iCount ? FontColor.FONT_YELLOW : (FontColor)MAP_INDEX_COLOR_VALUE);
            FontSubSystem.FindFontCenterCoordinates(MAP_VERT_INDEX_X, MAP_VERT_INDEX_Y + (iCount - 1) * MAP_GRID_Y, MAP_HORT_HEIGHT, MAP_GRID_Y, vert, FontStyle.BLOCKFONT2, out usX, out usY);
            mprintf(usX, usY, vert);
        }
    }

    /// <summary>A yellow box around the selected sector.</summary>
    private void DrawSelectedSector()
    {
        var (sx, sy) = ScreenXYFromMapXY(MapScreenInterfaceMap.sSelMapX, (int)MapScreenInterfaceMap.sSelMapY);
        sx += 1;
        var yellow = new Rgba32(255, 255, 0, 255);

        video.ColorFillVideoSurfaceArea(SurfaceType.FRAME_BUFFER, new(sx, sy, MAP_GRID_X, 1), yellow);
        video.ColorFillVideoSurfaceArea(SurfaceType.FRAME_BUFFER, new(sx, sy + MAP_GRID_Y - 1, MAP_GRID_X, 1), yellow);
        video.ColorFillVideoSurfaceArea(SurfaceType.FRAME_BUFFER, new(sx, sy, 1, MAP_GRID_Y), yellow);
        video.ColorFillVideoSurfaceArea(SurfaceType.FRAME_BUFFER, new(sx + MAP_GRID_X - 1, sy, 1, MAP_GRID_Y), yellow);
    }

    /// <summary>The "Level 1" text shown when looking at an underground level.</summary>
    private static void DisplayLevelString()
    {
        if (MapScreenInterfaceMap.iCurrentMapSectorZ == 0)
        {
            return;
        }

        FontSubSystem.SetFont(FontStyle.BLOCKFONT2);
        FontSubSystem.SetFontForeground((FontColor)MAP_INDEX_COLOR_VALUE);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
        mprintf(MAP_LEVEL_STRING_X, MAP_LEVEL_STRING_Y, "Level " + MapScreenInterfaceMap.iCurrentMapSectorZ);
    }

    private static void DrawClock()
    {
        FontSubSystem.SetFont(FontStyle.COMPFONT);
        FontSubSystem.SetFontBackground(FontColor.FONT_MCOLOR_BLACK);
        FontSubSystem.SetFontForeground(FontColor.FONT_LTGREEN);
        mprintf(CLOCK_X, CLOCK_Y + 1, gswzWorldTimeStr);
    }

    private const int SMALL_YELLOW_BOX = 0;
    private const int SMALL_DULL_YELLOW_BOX = 2;
    private const int MERC_ICONS_PER_LINE = 6;
    private const int ROWS_PER_SECTOR = 5;
    private const int MAP_X_ICON_OFFSET = 2;
    private const int MAP_Y_ICON_OFFSET = 1;

    /// <summary>
    /// A small box on the map for each of the player's mercs in a sector: bright yellow for those who can move,
    /// dull yellow for those busy with an assignment (ShowTeamAndVehicles / ShowOnDutyTeam / ShowAssignedTeam).
    /// TODO(port): vehicles, enemies, mercs in motion between sectors and the helicopter.
    /// </summary>
    private void ShowTeamOnMap()
    {
        var icons = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\boxes.sti"));
        var perSector = new System.Collections.Generic.Dictionary<(int x, int y), int>();

        // mobile mercs first, then those on an assignment, so the bright boxes come first in each sector
        for (int pass = 0; pass < 2; pass++)
        {
            for (int i = 0; i < MAX_CHARACTER_COUNT; i++)
            {
                if (!gCharactersList[i].fValid)
                {
                    continue;
                }

                var pSoldier = MercPtrs[gCharactersList[i].usSolID];
                if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE)
                    || pSoldier.bSectorZ != MapScreenInterfaceMap.iCurrentMapSectorZ
                    || pSoldier.bLife <= 0
                    || pSoldier.fBetweenSectors
                    || pSoldier.bAssignment == Assignment.IN_TRANSIT
                    || pSoldier.bAssignment == Assignment.ASSIGNMENT_POW)
                {
                    continue;
                }

                bool mobile = pSoldier.bAssignment < Assignment.ON_DUTY || pSoldier.bAssignment == Assignment.VEHICLE;
                if (mobile != (pass == 0))
                {
                    continue;
                }

                var key = (pSoldier.sSectorX, (int)pSoldier.sSectorY);
                perSector.TryGetValue(key, out int position);
                perSector[key] = position + 1;

                // don't show more icons than fit into one sector
                if (position >= MERC_ICONS_PER_LINE * ROWS_PER_SECTOR)
                {
                    continue;
                }

                var (sx, sy) = ScreenXYFromMapXY(key.Item1, key.Item2);
                video.BltVideoObject(
                    SurfaceType.FRAME_BUFFER,
                    icons,
                    mobile ? SMALL_YELLOW_BOX : SMALL_DULL_YELLOW_BOX,
                    sx + MAP_X_ICON_OFFSET + 3 * (position % MERC_ICONS_PER_LINE),
                    sy + MAP_Y_ICON_OFFSET + 3 * (position / MERC_ICONS_PER_LINE),
                    VO_BLT.SRCTRANSPARENCY);
            }
        }
    }

    private static readonly (int x, int y)[] samSites =
    [
        ((int)SAM.SAM_1_X, (int)SAM.SAM_1_Y),
        ((int)SAM.SAM_2_X, (int)SAM.SAM_2_Y),
        ((int)SAM.SAM_3_X, (int)SAM.SAM_3_Y),
        ((int)SAM.SAM_4_X, (int)SAM.SAM_4_Y),
    ];

    /// <summary>The mine icons, and the SAM sites that the player has found (BlitMineIcon / ShowSAMSitesOnStrategicMap).</summary>
    private void ShowMinesAndSamSites()
    {
        var mineIcon = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\mine.sti"));
        foreach (var mine in gMineLocation.Values)
        {
            var (sx, sy) = ScreenXYFromMapXY(mine.sSectorX, (int)mine.sSectorY);
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, mineIcon, 1, sx + MAP_GRID_X / 4, sy + MAP_GRID_Y / 4, VO_BLT.SRCTRANSPARENCY);
        }

        var samIcon = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\SAM.sti"));
        for (int i = 0; i < samSites.Length; i++)
        {
            // has the sam site here been found?
            if (!fSamSiteFound.TryGetValue((SAM_SITE)i, out bool found) || !found)
            {
                continue;
            }

            var (sx, sy) = ScreenXYFromMapXY(samSites[i].x, samSites[i].y);
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, samIcon, 1, sx + 5, sy + 3, VO_BLT.SRCTRANSPARENCY);
        }
    }

    /// <summary>A white box around the sector the mouse is over.</summary>
    private void DrawHighlightedSector()
    {
        if (!mapViewRegion.HasMouse)
        {
            return;
        }

        int sMapX = (mapViewRegion.MousePos.X - MAP_VIEW_START_X) / MAP_GRID_X;
        int sMapY = (mapViewRegion.MousePos.Y - MAP_VIEW_START_Y) / MAP_GRID_Y;
        if (sMapX < 1 || sMapX > MAX_VIEW_SECTORS || sMapY < 1 || sMapY > MAX_VIEW_SECTORS)
        {
            return;
        }

        var (sx, sy) = ScreenXYFromMapXY(sMapX, sMapY);
        sx += 1;
        var white = new Rgba32(255, 255, 255, 255);

        video.ColorFillVideoSurfaceArea(SurfaceType.FRAME_BUFFER, new(sx, sy, MAP_GRID_X, 1), white);
        video.ColorFillVideoSurfaceArea(SurfaceType.FRAME_BUFFER, new(sx, sy + MAP_GRID_Y - 1, MAP_GRID_X, 1), white);
        video.ColorFillVideoSurfaceArea(SurfaceType.FRAME_BUFFER, new(sx, sy, 1, MAP_GRID_Y), white);
        video.ColorFillVideoSurfaceArea(SurfaceType.FRAME_BUFFER, new(sx + MAP_GRID_X - 1, sy, 1, MAP_GRID_Y), white);
    }
}
