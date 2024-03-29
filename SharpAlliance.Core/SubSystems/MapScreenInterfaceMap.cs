﻿using System;
using System.Collections.Generic;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.Screens;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class MapScreenInterfaceMap
{
    private FASTHELPREGION[] pFastHelpMapScreenList = new FASTHELPREGION[Globals.MAX_MAPSCREEN_FAST_HELP];

    // the leave item list
    private static List<MERC_LEAVE_ITEM?> gpLeaveList = new(Globals.NUM_LEAVE_LIST_SLOTS);

    // holds ids of mercs who left stuff behind
    private static int[] guiLeaveListOwnerProfileId = new int[Globals.NUM_LEAVE_LIST_SLOTS];

    // the palettes
    private Rgba32[] pMapLTRedPalette;
    private Rgba32[] pMapDKRedPalette;
    private Rgba32[] pMapLTGreenPalette;
    private Rgba32[] pMapDKGreenPalette;
    // destination plotting character
    public static int bSelectedDestChar = -1;

    // map region
    public static Rectangle MapScreenRect = new(
        (MAP_VIEW_START_X + MAP_GRID_X - 2),
        (MAP_VIEW_START_Y + MAP_GRID_Y - 1),
        MAP_VIEW_START_X + MAP_VIEW_WIDTH - 1 + MAP_GRID_X,
        MAP_VIEW_START_Y + MAP_VIEW_HEIGHT - 10 + MAP_GRID_Y);

    public MapScreenInterfaceMap(IVideoManager videoManager)
    {
        video = videoManager;
    }

    public object guiUpdatePanelTactical { get; internal set; }
    public object guiUpdatePanel { get; internal set; }

    public static Point[] pMapScreenFastHelpLocationList = new Point[]
    {
        new(25,200 ),
        new(150,200),
        new(450,430),
        new(400,200),
        new(250,100),
        new(100,100),
        new(100,100),
        new(100,100),
        new(100,100),
        new(150,200),
        new(100,100),
    };

    public static int[] pMapScreenFastHelpWidthList = new int[]
    {
        100,
        100,
        100,
        100,
        100,
        100,
        100,
        100,
        100,
        300,
    };

    // list of map sectors that player isn't allowed to even highlight
    private static bool[,] sBadSectorsList = new bool[Globals.WORLD_MAP_X, Globals.WORLD_MAP_X];
    private static IVideoManager video;
    internal static MAP_ROW sSelMapY = MAP_ROW.A;
    internal static int sSelMapX = 9;
    internal static int iCurrentMapSectorZ;
    internal static int bSelectedAssignChar;
    internal static int bSelectedContractChar;

    public void SetUpBadSectorsList()
    {
        // initalizes all sectors to highlighable and then the ones non highlightable are marked as such
        sbyte bY;

        //memset(&sBadSectorsList, 0, sizeof(sBadSectorsList));

        // the border regions
        for (bY = 0; bY < Globals.WORLD_MAP_X; bY++)
        {
            sBadSectorsList[0, bY]
                = sBadSectorsList[Globals.WORLD_MAP_X - 1, bY]
                = sBadSectorsList[bY, 0]
                = sBadSectorsList[bY, Globals.WORLD_MAP_X - 1]
                = true;
        }

        sBadSectorsList[4, 1] = true;
        sBadSectorsList[5, 1] = true;
        sBadSectorsList[16, 1] = true;
        sBadSectorsList[16, 5] = true;
        sBadSectorsList[16, 6] = true;

        sBadSectorsList[16, 10] = true;
        sBadSectorsList[16, 11] = true;
        sBadSectorsList[16, 12] = true;
        sBadSectorsList[16, 13] = true;
        sBadSectorsList[16, 14] = true;
        sBadSectorsList[16, 15] = true;
        sBadSectorsList[16, 16] = true;

        sBadSectorsList[15, 13] = true;
        sBadSectorsList[15, 14] = true;
        sBadSectorsList[15, 15] = true;
        sBadSectorsList[15, 16] = true;

        sBadSectorsList[14, 14] = true;
        sBadSectorsList[14, 15] = true;
        sBadSectorsList[14, 16] = true;

        sBadSectorsList[13, 14] = true;
    }

    public void InitializePalettesForMap()
    {
        // init palettes
        HVSURFACE hSrcVSurface;
        VSURFACE_DESC vs_desc;

        // load image
        HVOBJECT uiTempMap = video.GetVideoObject("INTERFACE\\b_map.pcx");

        // get video surface
        var surfType = video.Surfaces.CreateSurface(uiTempMap);
        var surf = video.GetVideoSurface(out hSrcVSurface, surfType);
        var palette = video.GetVSurfacePaletteEntries(uiTempMap);

        // set up various palettes
        this.pMapLTRedPalette = video.Create16BPPPaletteShaded(palette, redScale: 400, greenScale: 0, blueScale: 0, mono: true);
        this.pMapDKRedPalette = video.Create16BPPPaletteShaded(palette, redScale: 200, greenScale: 0, blueScale: 0, mono: true);
        this.pMapLTGreenPalette = video.Create16BPPPaletteShaded(palette, redScale: 0, greenScale: 400, blueScale: 0, mono: true);
        this.pMapDKGreenPalette = video.Create16BPPPaletteShaded(palette, redScale: 0, greenScale: 200, blueScale: 0, mono: true);

        // delete image
        // video.DeleteVideoSurfaceFromIndex(uiTempMap);
    }

    public static bool DrawMap()
    {
        HVSURFACE hSrcVSurface;
        int uiDestPitchBYTES = 0;
        int uiSrcPitchBYTES = 0;
        int pDestBuf = 0;
        int pSrcBuf = 0;
        Rectangle clip;
        int cnt = 0, cnt2 = 0;

        //if (!iCurrentMapSectorZ)
        //{
        //    // pDestBuf = LockVideoSurface(Globals.Surfaces.SAVE_BUFFER, out uiDestPitchBYTES);

        //    if (!video.GetVideoSurface(out hSrcVSurface, Globals.guiBIGMAP))
        //    {
        //        return false;
        //    }

        //    // pSrcBuf = LockVideoSurface(Globals.guiBIGMAP, out uiSrcPitchBYTES);

        //    // clip blits to mapscreen region
        //    //ClipBlitsToMapViewRegion( );

        //    if (fZoomFlag)
        //    {
        //        // set up bounds
        //        if (iZoomX < WEST_ZOOM_BOUND)
        //        {
        //            iZoomX = WEST_ZOOM_BOUND;
        //        }

        //        if (iZoomX > EAST_ZOOM_BOUND)
        //        {
        //            iZoomX = EAST_ZOOM_BOUND;
        //        }

        //        if (iZoomY < NORTH_ZOOM_BOUND + 1)
        //        {
        //            iZoomY = NORTH_ZOOM_BOUND;
        //        }

        //        if (iZoomY > SOUTH_ZOOM_BOUND)
        //        {
        //            iZoomY = SOUTH_ZOOM_BOUND;
        //        }

        //        clip = new()
        //        {
        //            X = iZoomX - 2,
        //            Width = iZoomX - 2 + Globals.MAP_VIEW_WIDTH + 2,
        //            Y = iZoomY - 3,
        //            Height = iZoomY - 3 + Globals.MAP_VIEW_HEIGHT - 1,
        //        };

        //        /*
        //        clip.iLeft=clip.iLeft - 1;
        //        clip.iRight=clip.iLeft + MapScreenRect.iRight - MapScreenRect.iLeft;
        //        clip.iTop=iZoomY - 1;
        //        clip.iBottom=clip.iTop + MapScreenRect.iBottom - MapScreenRect.iTop;
        //        */

        //        if (clip.Bottom > hSrcVSurface.usHeight)
        //        {
        //            clip.Height = hSrcVSurface.usHeight;
        //        }

        //        if (clip.Right > hSrcVSurface.usWidth)
        //        {
        //            clip.Width = hSrcVSurface.usWidth;
        //        }

        //        video.Blt8BPPDataSubTo16BPPBuffer(pDestBuf, uiDestPitchBYTES, hSrcVSurface, pSrcBuf, uiSrcPitchBYTES, Globals.MAP_VIEW_START_X + Globals.MAP_GRID_X, Globals.MAP_VIEW_START_Y + Globals.MAP_GRID_Y - 2, out clip);
        //    }
        //    else
        //    {
        //        video.Blt8BPPDataTo16BPPBufferHalf(pDestBuf, uiDestPitchBYTES, hSrcVSurface, pSrcBuf, uiSrcPitchBYTES, Globals.MAP_VIEW_START_X + 1, Globals.MAP_VIEW_START_Y);
        //    }

        //    //UnLockVideoSurface(Globals.guiBIGMAP);
        //    //UnLockVideoSurface(Globals.Surfaces.SAVE_BUFFER);


        //    // shade map sectors (must be done after Tixa/Orta/Mine icons have been blitted, but before icons!)		
        //    for (cnt = 1; cnt < Globals.MAP_WORLD_X - 1; cnt++)
        //    {
        //        for (cnt2 = 1; cnt2 < Globals.MAP_WORLD_Y - 1; cnt2++)
        //        {
        //            // LATE DESIGN CHANGE: darken sectors not yet visited, instead of those under known enemy control
        //            if (GetSectorFlagStatus(cnt, cnt2, iCurrentMapSectorZ, SF.ALREADY_VISITED) == false)
        //            //				if ( IsTheSectorPerceivedToBeUnderEnemyControl( cnt, cnt2, ( int )( iCurrentMapSectorZ ) ) )
        //            {
        //                if (fShowAircraftFlag && !iCurrentMapSectorZ)
        //                {
        //                    if (!Globals.strategicMap[cnt + cnt2 * Globals.WORLD_MAP_X].fEnemyAirControlled)
        //                    {
        //                        // sector not visited, not air controlled
        //                        ShadeMapElem(cnt, cnt2, MAP_SHADE.DK_GREEN);
        //                    }
        //                    else
        //                    {
        //                        // sector not visited, controlled and air not
        //                        ShadeMapElem(cnt, cnt2, MAP_SHADE.DK_RED);
        //                    }
        //                }
        //                else
        //                {
        //                    // not visited
        //                    ShadeMapElem(cnt, cnt2, MAP_SHADE.BLACK);
        //                }
        //            }
        //            else
        //            {
        //                if (fShowAircraftFlag && !iCurrentMapSectorZ)
        //                {
        //                    if (!Globals.strategicMap[cnt + cnt2 * Globals.WORLD_MAP_X].fEnemyAirControlled)
        //                    {
        //                        // sector visited and air controlled
        //                        ShadeMapElem(cnt, cnt2, MAP_SHADE.LT_GREEN);
        //                    }
        //                    else
        //                    {
        //                        // sector visited but not air controlled
        //                        ShadeMapElem(cnt, cnt2, MAP_SHADE.LT_RED);
        //                    }
        //                }
        //            }
        //        }
        //    }


        //    // UNFORTUNATELY, WE CAN'T SHADE THESE ICONS AS PART OF SHADING THE MAP, BECAUSE FOR AIRSPACE, THE SHADE FUNCTION
        //    // DOESN'T MERELY SHADE THE EXISTING MAP SURFACE, BUT INSTEAD GRABS THE ORIGINAL GRAPHICS FROM BIGMAP, AND CHANGES
        //    // THEIR PALETTE.  BLITTING ICONS PRIOR TO SHADING WOULD MEAN THEY DON'T SHOW UP IN AIRSPACE VIEW AT ALL.

        //    // if Orta found
        //    if (fFoundOrta)
        //    {
        //        DrawOrta();
        //    }

        //    // if Tixa found
        //    if (fFoundTixa)
        //    {
        //        DrawTixa();
        //    }

        //    // draw SAM sites
        //    ShowSAMSitesOnStrategicMap();

        //    // draw mine icons
        //    for (MINE iCounter = 0; iCounter < MINE.MAX_NUMBER_OF_MINES; iCounter++)
        //    {
        //        BlitMineIcon(Globals.gMineLocation[iCounter].sSectorX, Globals.gMineLocation[iCounter].sSectorY);
        //    }


        //    // if mine details filter is set
        //    if (fShowMineFlag)
        //    {
        //        // show mine name/production text
        //        for (MINE iCounter = 0; iCounter < MINE.MAX_NUMBER_OF_MINES; iCounter++)
        //        {
        //            BlitMineText(Globals.gMineLocation[iCounter].sSectorX, Globals.gMineLocation[iCounter].sSectorY);
        //        }
        //    }

        //    // draw towns names & loyalty ratings, and grey town limit borders
        //    if (fShowTownFlag)
        //    {
        //        BlitTownGridMarkers();
        //        ShowTownText();
        //    }

        //    // draw militia icons
        //    if (fShowMilitia)
        //    {
        //        DrawTownMilitiaForcesOnMap();
        //    }

        //    if (fShowAircraftFlag && !gfInChangeArrivalSectorMode)
        //    {
        //        DrawBullseye();
        //    }
        //}
        //else
        //{
        //    HandleLowerLevelMapBlit();
        //}


        //// show mine outlines even when viewing underground sublevels - they indicate where the mine entrances are
        //if (fShowMineFlag)
        //{
        //    // draw grey mine sector borders
        //    BlitMineGridMarkers();
        //}


        //// do not show mercs/vehicles when airspace is ON
        //// commented out on a trial basis!
        ////	if( !fShowAircraftFlag )
        //{
        //    if (fShowTeamFlag)
        //    {
        //        ShowTeamAndVehicles(Globals.SHOW_TEAMMATES | Globals.SHOW_VEHICLES);
        //    }
        //    else
        //    {
        //        HandleShowingOfEnemiesWithMilitiaOn();
        //    }

        //    /*
        //            if((fShowTeamFlag)&&(fShowVehicleFlag))
        //             ShowTeamAndVehicles(SHOW_TEAMMATES | SHOW_VEHICLES);
        //            else if(fShowTeamFlag)
        //                ShowTeamAndVehicles(SHOW_TEAMMATES);
        //            else if(fShowVehicleFlag)
        //                ShowTeamAndVehicles(SHOW_VEHICLES);
        //            else
        //            {
        //                HandleShowingOfEnemiesWithMilitiaOn( );
        //            }
        //    */
        //}

        //if (fShowItemsFlag)
        //{
        //    ShowItemsOnMap();
        //}

        //DisplayLevelString();

        //RestoreClipRegionToFullScreen( );

        return true;
    }

    public static void HandleMAPUILoseCursorFromOtherScreen()
    {
        // rerender map without cursors
        Globals.fMapPanelDirty = true;

        if (Globals.fInMapMode)
        {
            MapScreen.RenderMapRegionBackground();
        }

        return;
    }

    public void SetUpMapScreenFastHelpText()
    {
        // now run through and display all the fast help text for the mapscreen functional regions
        for (int iCounter = 0; iCounter < Globals.NUMBER_OF_MAPSCREEN_HELP_MESSAGES; iCounter++)
        {
            this.pFastHelpMapScreenList[iCounter].iX = pMapScreenFastHelpLocationList[iCounter].X;
            this.pFastHelpMapScreenList[iCounter].iY = pMapScreenFastHelpLocationList[iCounter].Y;
            this.pFastHelpMapScreenList[iCounter].iW = pMapScreenFastHelpWidthList[iCounter];
            this.pFastHelpMapScreenList[iCounter].FastHelpText = EnglishText.pMapScreenFastHelpTextList[iCounter];
        }
    }

    public static void InitLeaveList()
    {
        // init leave list with nullS/zeroes
        for (int iCounter = 0; iCounter < Globals.NUM_LEAVE_LIST_SLOTS; iCounter++)
        {
            gpLeaveList.Add(null);
            gpLeaveList[iCounter] = null;
            guiLeaveListOwnerProfileId[iCounter] = (int)NO_PROFILE;
        }
    }

    public static void RenderMapBorder()
    {
        // renders the actual border to the Surfaces.SAVE_BUFFER

        /*	
            if( fDisabledMapBorder )
            {
                return;
            }
        */

        if (fShowMapInventoryPool)
        {
            // render background, then leave
            //            BlitInventoryPoolGraphic();
            return;
        }

        // get and blt border
//        video.GetVideoObject(out HVOBJECT hHandle, Globals.guiMapBorder);
//        VideoObjectManager.BltVideoObject(
//            SurfaceType.SAVE_BUFFER,
//            hHandle,
//            0,
//            Globals.MAP_BORDER_X,
//            Globals.MAP_BORDER_Y,
//            VO_BLT.SRCTRANSPARENCY,
//            null);

        // show the level marker
        //        DisplayCurrentLevelMarker();


        return;
    }

    internal static void InitMapSecrets()
    {
        fFoundTixa = false;
        fFoundOrta = false;

        for (int ubSamIndex = 0; ubSamIndex < NUMBER_OF_SAMS; ubSamIndex++)
        {
            fSamSiteFound[(SAM_SITE)ubSamIndex] = false;
        }
    }

    internal static bool IsTheCursorAllowedToHighLightThisSector(int sSectorX, MAP_ROW sSectorY)
    {
        // check to see if this sector is a blocked out sector?

        if (sBadSectorsList[sSectorX, (int)sSectorY])
        {
            return (false);
        }
        else
        {
            // return cursor is allowed to highlight this sector
            return (true);
        }
    }
}

public struct MERC_LEAVE_ITEM
{
    public OBJECTTYPE o;
}

public struct FASTHELPREGION
{
    // the string
    public string FastHelpText;

    // the x and y position values
    public int iX;
    public int iY;
    public int iW;
}

public enum MAP_SHADE
{
    BLACK = 0,
    LT_GREEN,
    DK_GREEN,
    LT_RED,
    DK_RED,
}
