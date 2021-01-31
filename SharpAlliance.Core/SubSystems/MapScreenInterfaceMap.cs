using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Managers.VideoSurfaces;
using Veldrid;

namespace SharpAlliance.Core.SubSystems
{
    public class MapScreenInterfaceMap
    {
        private static class Constants
        {
            // size of squares on the map
            public const int MAP_GRID_X = 21;
            public const int MAP_GRID_Y = 18;

            // the number of help region messages
            public const int NUMBER_OF_MAPSCREEN_HELP_MESSAGES = 5;
            public const int MAX_MAPSCREEN_FAST_HELP = 100;

            // scroll bounds
            public const int EAST_ZOOM_BOUND = 378;
            public const int WEST_ZOOM_BOUND = 42;
            public const int SOUTH_ZOOM_BOUND = 324;
            public const int NORTH_ZOOM_BOUND = 36;

            // map view region
            public const int MAP_VIEW_START_X = 270;
            public const int MAP_VIEW_START_Y = 10;
            public const int MAP_VIEW_WIDTH = 336;
            public const int MAP_VIEW_HEIGHT = 298;

            // zoomed in grid sizes
            public const int MAP_GRID_ZOOM_X = MAP_GRID_X * 2;
            public const int MAP_GRID_ZOOM_Y = MAP_GRID_Y * 2;

            // number of units wide
            public const int WORLD_MAP_X = 18;

            // dirty regions for the map
            public const int DMAP_GRID_X = MAP_GRID_X + 1;
            public const int DMAP_GRID_Y = MAP_GRID_Y + 1;
            public const int DMAP_GRID_ZOOM_X = MAP_GRID_ZOOM_X + 1;
            public const int DMAP_GRID_ZOOM_Y = MAP_GRID_ZOOM_Y + 1;

            // Orta position on the map
            public const int ORTA_SECTOR_X = 4;
            public const int ORTA_SECTOR_Y = 11;

            public const int TIXA_SECTOR_X = 9;
            public const int TIXA_SECTOR_Y = 10;

            // what are we showing?..teams/vehicles
            // Show values
            public const int SHOW_TEAMMATES = 1;
            public const int SHOW_VEHICLES = 2;

            // wait time until temp path is drawn, from placing cursor on a map grid
            public const int MIN_WAIT_TIME_FOR_TEMP_PATH = 200;

            // number of LINKED LISTS for sets of leave items (each slot holds an unlimited # of items)
            public const int NUM_LEAVE_LIST_SLOTS = 20;
        }

        private FASTHELPREGION[] pFastHelpMapScreenList = new FASTHELPREGION[Constants.MAX_MAPSCREEN_FAST_HELP];

        // the leave item list
        private List<MERC_LEAVE_ITEM?> gpLeaveList = new(Constants.NUM_LEAVE_LIST_SLOTS);

        // holds ids of mercs who left stuff behind
        private int[] guiLeaveListOwnerProfileId = new int[Constants.NUM_LEAVE_LIST_SLOTS];

        // the palettes
        private ushort pMapLTRedPalette;
        private ushort pMapDKRedPalette;
        private ushort pMapLTGreenPalette;
        private ushort pMapDKGreenPalette;


        public MapScreenInterfaceMap(IVideoManager videoManager)
        {
            this.video = videoManager;
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
        private bool[,] sBadSectorsList = new bool[Constants.WORLD_MAP_X, Constants.WORLD_MAP_X];
        private readonly IVideoManager video;

        public void SetUpBadSectorsList()
        {
            // initalizes all sectors to highlighable and then the ones non highlightable are marked as such
            sbyte bY;

            //memset(&sBadSectorsList, 0, sizeof(sBadSectorsList));

            // the border regions
            for (bY = 0; bY < Constants.WORLD_MAP_X; bY++)
            {
                this.sBadSectorsList[0, bY]
                    = this.sBadSectorsList[Constants.WORLD_MAP_X - 1, bY]
                    = this.sBadSectorsList[bY, 0]
                    = this.sBadSectorsList[bY, Constants.WORLD_MAP_X - 1]
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

        public ValueTask<bool> InitializePalettesForMap()
        {
            // init palettes
            HVSURFACE hSrcVSurface;
            SGPPaletteEntry[] pPalette = new SGPPaletteEntry[256];
            VSURFACE_DESC vs_desc;
            uint uiTempMap;

            // load image
            vs_desc.fCreateFlags = VSurfaceCreateFlags.VSURFACE_CREATE_FROMFILE | VSurfaceCreateFlags.VSURFACE_SYSTEM_MEM_USAGE;
            vs_desc.ImageFile = "INTERFACE\\b_map.pcx";
            this.video.AddVideoSurface(out vs_desc, out uiTempMap);

            // get video surface
            this.video.GetVideoSurface(out hSrcVSurface, uiTempMap);
            this.video.GetVSurfacePaletteEntries(hSrcVSurface, pPalette);

            // set up various palettes
            pMapLTRedPalette = this.video.Create16BPPPaletteShaded(pPalette: ref pPalette, redScale: 400, greenScale: 0,   blueScale: 0, mono: true);
            pMapDKRedPalette = this.video.Create16BPPPaletteShaded(ref pPalette, redScale: 200, greenScale: 0,   blueScale: 0, mono: true);
            pMapLTGreenPalette = this.video.Create16BPPPaletteShaded(ref pPalette, redScale: 0, greenScale: 400, blueScale: 0, mono: true);
            pMapDKGreenPalette = this.video.Create16BPPPaletteShaded(ref pPalette, redScale: 0, greenScale: 200, blueScale: 0, mono: true);

            // delete image
            this.video.DeleteVideoSurfaceFromIndex(uiTempMap);

            return ValueTask.FromResult(true);
        }

        public void SetUpMapScreenFastHelpText()
        {
            // now run through and display all the fast help text for the mapscreen functional regions
            for (int iCounter = 0; iCounter < Constants.NUMBER_OF_MAPSCREEN_HELP_MESSAGES; iCounter++)
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
            for (int iCounter = 0; iCounter < Constants.NUM_LEAVE_LIST_SLOTS; iCounter++)
            {
                this.gpLeaveList[iCounter] = null;
                this.guiLeaveListOwnerProfileId[iCounter] = SoldierControl.NO_PROFILE;
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
}
