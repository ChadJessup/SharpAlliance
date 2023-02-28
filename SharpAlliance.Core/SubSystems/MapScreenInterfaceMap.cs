using System.Collections.Generic;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.Screens;
using SixLabors.ImageSharp;

namespace SharpAlliance.Core.SubSystems;

public class MapScreenInterfaceMap
{
    private FASTHELPREGION[] pFastHelpMapScreenList = new FASTHELPREGION[Globals.MAX_MAPSCREEN_FAST_HELP];

    // the leave item list
    private List<MERC_LEAVE_ITEM?> gpLeaveList = new(Globals.NUM_LEAVE_LIST_SLOTS);

    // holds ids of mercs who left stuff behind
    private int[] guiLeaveListOwnerProfileId = new int[Globals.NUM_LEAVE_LIST_SLOTS];

    // the palettes
    private ushort pMapLTRedPalette;
    private ushort pMapDKRedPalette;
    private ushort pMapLTGreenPalette;
    private ushort pMapDKGreenPalette;


    public MapScreenInterfaceMap(IVideoManager videoManager)
    {
        //VeldridVideoManager = videoManager;
    }

    public object guiUpdatePanelTactical { get; internal set; }
    public object guiUpdatePanel { get; internal set; }

    private Point[] pMapScreenFastHelpLocationList = new Point[]
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

    private int[] pMapScreenFastHelpWidthList = new int[]
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
    private bool[,] sBadSectorsList = new bool[Globals.WORLD_MAP_X, Globals.WORLD_MAP_X];
    private readonly IVideoManager video;

    public void SetUpBadSectorsList()
    {
        // initalizes all sectors to highlighable and then the ones non highlightable are marked as such
        sbyte bY;

        //memset(&sBadSectorsList, 0, sizeof(sBadSectorsList));

        // the border regions
        for (bY = 0; bY < Globals.WORLD_MAP_X; bY++)
        {
            this.sBadSectorsList[0, bY]
                = this.sBadSectorsList[Globals.WORLD_MAP_X - 1, bY]
                = this.sBadSectorsList[bY, 0]
                = this.sBadSectorsList[bY, Globals.WORLD_MAP_X - 1]
                = true;
        }

        this.sBadSectorsList[4, 1] = true;
        this.sBadSectorsList[5, 1] = true;
        this.sBadSectorsList[16, 1] = true;
        this.sBadSectorsList[16, 5] = true;
        this.sBadSectorsList[16, 6] = true;

        this.sBadSectorsList[16, 10] = true;
        this.sBadSectorsList[16, 11] = true;
        this.sBadSectorsList[16, 12] = true;
        this.sBadSectorsList[16, 13] = true;
        this.sBadSectorsList[16, 14] = true;
        this.sBadSectorsList[16, 15] = true;
        this.sBadSectorsList[16, 16] = true;

        this.sBadSectorsList[15, 13] = true;
        this.sBadSectorsList[15, 14] = true;
        this.sBadSectorsList[15, 15] = true;
        this.sBadSectorsList[15, 16] = true;

        this.sBadSectorsList[14, 14] = true;
        this.sBadSectorsList[14, 15] = true;
        this.sBadSectorsList[14, 16] = true;

        this.sBadSectorsList[13, 14] = true;
    }

    public void InitializePalettesForMap()
    {
        // init palettes
        HVSURFACE hSrcVSurface;
        SGPPaletteEntry[] pPalette = new SGPPaletteEntry[256];
        VSURFACE_DESC vs_desc;
        Surfaces uiTempMap;

        // load image
        vs_desc.fCreateFlags = VSurfaceCreateFlags.VSURFACE_CREATE_FROMFILE | VSurfaceCreateFlags.VSURFACE_SYSTEM_MEM_USAGE;
        vs_desc.ImageFile = "INTERFACE\\b_map.pcx";
        VeldridVideoManager.AddVideoSurface(out vs_desc, out uiTempMap);

        // get video surface
        VeldridVideoManager.GetVideoSurface(out hSrcVSurface, uiTempMap);
        VeldridVideoManager.GetVSurfacePaletteEntries(hSrcVSurface, pPalette);

        // set up various palettes
        this.pMapLTRedPalette = VeldridVideoManager.Create16BPPPaletteShaded(pPalette: ref pPalette, redScale: 400, greenScale: 0, blueScale: 0, mono: true);
        this.pMapDKRedPalette = VeldridVideoManager.Create16BPPPaletteShaded(ref pPalette, redScale: 200, greenScale: 0, blueScale: 0, mono: true);
        this.pMapLTGreenPalette = VeldridVideoManager.Create16BPPPaletteShaded(ref pPalette, redScale: 0, greenScale: 400, blueScale: 0, mono: true);
        this.pMapDKGreenPalette = VeldridVideoManager.Create16BPPPaletteShaded(ref pPalette, redScale: 0, greenScale: 200, blueScale: 0, mono: true);

        // delete image
        VeldridVideoManager.DeleteVideoSurfaceFromIndex(uiTempMap);
    }

    public static bool DrawMap()
    {
        HVSURFACE hSrcVSurface;
        int uiDestPitchBYTES;
        int uiSrcPitchBYTES;
        int pDestBuf;
        int pSrcBuf;
        Rectangle clip;
        int cnt, cnt2;

        if (!iCurrentMapSectorZ)
        {
            // pDestBuf = LockVideoSurface(Globals.guiSAVEBUFFER, out uiDestPitchBYTES);

            if (!GetVideoSurface(hSrcVSurface, Globals.guiBIGMAP))
            {
                return false;
            }

            // pSrcBuf = LockVideoSurface(Globals.guiBIGMAP, out uiSrcPitchBYTES);

            // clip blits to mapscreen region
            //ClipBlitsToMapViewRegion( );

            if (fZoomFlag)
            {
                // set up bounds
                if (iZoomX < WEST_ZOOM_BOUND)
                {
                    iZoomX = WEST_ZOOM_BOUND;
                }

                if (iZoomX > EAST_ZOOM_BOUND)
                {
                    iZoomX = EAST_ZOOM_BOUND;
                }

                if (iZoomY < NORTH_ZOOM_BOUND + 1)
                {
                    iZoomY = NORTH_ZOOM_BOUND;
                }

                if (iZoomY > SOUTH_ZOOM_BOUND)
                {
                    iZoomY = SOUTH_ZOOM_BOUND;
                }

                clip = new()
                {
                    X = iZoomX - 2,
                    Width = iZoomX - 2 + Globals.MAP_VIEW_WIDTH + 2,
                    Y = iZoomY - 3,
                    Height = iZoomY - 3 + Globals.MAP_VIEW_HEIGHT - 1,
                };

                /*
                clip.iLeft=clip.iLeft - 1;
                clip.iRight=clip.iLeft + MapScreenRect.iRight - MapScreenRect.iLeft;
                clip.iTop=iZoomY - 1;
                clip.iBottom=clip.iTop + MapScreenRect.iBottom - MapScreenRect.iTop;
                */

                if (clip.Bottom > hSrcVSurface.usHeight)
                {
                    clip.Bottom = hSrcVSurface.usHeight;
                }

                if (clip.Right > hSrcVSurface.usWidth)
                {
                    clip.Right = hSrcVSurface.usWidth;
                }

                Blt8BPPDataSubTo16BPPBuffer(pDestBuf, uiDestPitchBYTES, hSrcVSurface, pSrcBuf, uiSrcPitchBYTES, Globals.MAP_VIEW_START_X + Globals.MAP_GRID_X, Globals.MAP_VIEW_START_Y + Globals.MAP_GRID_Y - 2, out clip);
            }
            else
            {
                Blt8BPPDataTo16BPPBufferHalf(pDestBuf, uiDestPitchBYTES, hSrcVSurface, pSrcBuf, uiSrcPitchBYTES, Globals.MAP_VIEW_START_X + 1, Globals.MAP_VIEW_START_Y);
            }

            //UnLockVideoSurface(Globals.guiBIGMAP);
            //UnLockVideoSurface(Globals.guiSAVEBUFFER);


            // shade map sectors (must be done after Tixa/Orta/Mine icons have been blitted, but before icons!)		
            for (cnt = 1; cnt < Globals.MAP_WORLD_X - 1; cnt++)
            {
                for (cnt2 = 1; cnt2 < Globals.MAP_WORLD_Y - 1; cnt2++)
                {
                    // LATE DESIGN CHANGE: darken sectors not yet visited, instead of those under known enemy control
                    if (GetSectorFlagStatus(cnt, cnt2, iCurrentMapSectorZ, SF.ALREADY_VISITED) == false)
                    //				if ( IsTheSectorPerceivedToBeUnderEnemyControl( cnt, cnt2, ( int )( iCurrentMapSectorZ ) ) )
                    {
                        if (fShowAircraftFlag && !iCurrentMapSectorZ)
                        {
                            if (!Globals.StrategicMap[cnt + cnt2 * Globals.WORLD_MAP_X].fEnemyAirControlled)
                            {
                                // sector not visited, not air controlled
                                ShadeMapElem(cnt, cnt2, MAP_SHADE.DK_GREEN);
                            }
                            else
                            {
                                // sector not visited, controlled and air not
                                ShadeMapElem(cnt, cnt2, MAP_SHADE.DK_RED);
                            }
                        }
                        else
                        {
                            // not visited
                            ShadeMapElem(cnt, cnt2, MAP_SHADE.BLACK);
                        }
                    }
                    else
                    {
                        if (fShowAircraftFlag && !iCurrentMapSectorZ)
                        {
                            if (!Globals.StrategicMap[cnt + cnt2 * Globals.WORLD_MAP_X].fEnemyAirControlled)
                            {
                                // sector visited and air controlled
                                ShadeMapElem(cnt, cnt2, MAP_SHADE.LT_GREEN);
                            }
                            else
                            {
                                // sector visited but not air controlled
                                ShadeMapElem(cnt, cnt2, MAP_SHADE.LT_RED);
                            }
                        }
                    }
                }
            }


            // UNFORTUNATELY, WE CAN'T SHADE THESE ICONS AS PART OF SHADING THE MAP, BECAUSE FOR AIRSPACE, THE SHADE FUNCTION
            // DOESN'T MERELY SHADE THE EXISTING MAP SURFACE, BUT INSTEAD GRABS THE ORIGINAL GRAPHICS FROM BIGMAP, AND CHANGES
            // THEIR PALETTE.  BLITTING ICONS PRIOR TO SHADING WOULD MEAN THEY DON'T SHOW UP IN AIRSPACE VIEW AT ALL.

            // if Orta found
            if (fFoundOrta)
            {
                DrawOrta();
            }

            // if Tixa found
            if (fFoundTixa)
            {
                DrawTixa();
            }

            // draw SAM sites
            ShowSAMSitesOnStrategicMap();

            // draw mine icons
            for (MINE iCounter = 0; iCounter < MINE.MAX_NUMBER_OF_MINES; iCounter++)
            {
                BlitMineIcon(Globals.gMineLocation[iCounter].sSectorX, Globals.gMineLocation[iCounter].sSectorY);
            }


            // if mine details filter is set
            if (fShowMineFlag)
            {
                // show mine name/production text
                for (MINE iCounter = 0; iCounter < MINE.MAX_NUMBER_OF_MINES; iCounter++)
                {
                    BlitMineText(Globals.gMineLocation[iCounter].sSectorX, Globals.gMineLocation[iCounter].sSectorY);
                }
            }

            // draw towns names & loyalty ratings, and grey town limit borders
            if (fShowTownFlag)
            {
                BlitTownGridMarkers();
                ShowTownText();
            }

            // draw militia icons
            if (fShowMilitia)
            {
                DrawTownMilitiaForcesOnMap();
            }

            if (fShowAircraftFlag && !Globals.gfInChangeArrivalSectorMode)
            {
                DrawBullseye();
            }
        }
        else
        {
            HandleLowerLevelMapBlit();
        }


        // show mine outlines even when viewing underground sublevels - they indicate where the mine entrances are
        if (fShowMineFlag)
        {
            // draw grey mine sector borders
            BlitMineGridMarkers();
        }


        // do not show mercs/vehicles when airspace is ON
        // commented out on a trial basis!
        //	if( !fShowAircraftFlag )
        {
            if (fShowTeamFlag)
            {
                ShowTeamAndVehicles(Globals.SHOW_TEAMMATES | Globals.SHOW_VEHICLES);
            }
            else
            {
                HandleShowingOfEnemiesWithMilitiaOn();
            }

            /*
                    if((fShowTeamFlag)&&(fShowVehicleFlag))
                     ShowTeamAndVehicles(SHOW_TEAMMATES | SHOW_VEHICLES);
                    else if(fShowTeamFlag)
                        ShowTeamAndVehicles(SHOW_TEAMMATES);
                    else if(fShowVehicleFlag)
                        ShowTeamAndVehicles(SHOW_VEHICLES);
                    else
                    {
                        HandleShowingOfEnemiesWithMilitiaOn( );
                    }
            */
        }

        if (fShowItemsFlag)
        {
            ShowItemsOnMap();
        }

        DisplayLevelString();

        //RestoreClipRegionToFullScreen( );

        return (true);
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
            this.pFastHelpMapScreenList[iCounter].iX = this.pMapScreenFastHelpLocationList[iCounter].X;
            this.pFastHelpMapScreenList[iCounter].iY = this.pMapScreenFastHelpLocationList[iCounter].Y;
            this.pFastHelpMapScreenList[iCounter].iW = this.pMapScreenFastHelpWidthList[iCounter];
            this.pFastHelpMapScreenList[iCounter].FastHelpText = EnglishText.pMapScreenFastHelpTextList[iCounter];
        }
    }

    public void InitLeaveList()
    {
        // init leave list with nullS/zeroes
        for (int iCounter = 0; iCounter < Globals.NUM_LEAVE_LIST_SLOTS; iCounter++)
        {
            this.gpLeaveList[iCounter] = null;
            this.guiLeaveListOwnerProfileId[iCounter] = SoldierControl.NO_PROFILE;
        }
    }

    public static void RenderMapBorder()
    {
        // renders the actual border to the guiSAVEBUFFER
        HVOBJECT hHandle;

        /*	
            if( fDisabledMapBorder )
            {
                return;
            }
        */

        if (fShowMapInventoryPool)
        {
            // render background, then leave
            BlitInventoryPoolGraphic();
            return;
        }

        // get and blt border
        VeldridVideoManager.GetVideoObject(out hHandle, Globals.guiMapBorder);
        VeldridVideoManager.BltVideoObject(Globals.guiSAVEBUFFER, hHandle, 0, Globals.MAP_BORDER_X, Globals.MAP_BORDER_Y, VO_BLT_SRCTRANSPARENCY, null);

        // show the level marker
        DisplayCurrentLevelMarker();


        return;
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
