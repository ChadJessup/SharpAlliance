﻿using System;
using System.Collections.Generic;
using SharpAlliance.Core;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems;
using SixLabors.ImageSharp;
using static SharpAlliance.Core.EnglishText;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const int SHOW_MIN_FPS = 0;
    public const int SHOW_FULL_FPS = 1;
    // ENUMERATION OF SOLDIER POSIITONS IN GLOBAL SOLDIER LIST
    public const int MAX_NUM_SOLDIERS = 148;
    public const int NUM_PLANNING_MERCS = 8;
    public const int TOTAL_SOLDIERS = (NUM_PLANNING_MERCS + MAX_NUM_SOLDIERS);

    public const int MAXCOL = World.WORLD_COLS;
    public const int MAXROW = World.WORLD_ROWS;
    public const int GRIDSIZE = MAXCOL * MAXROW;
    public const int RIGHTMOSTGRID = MAXCOL - 1;
    public const int LASTROWSTART = GRIDSIZE - MAXCOL;
    public const int NOWHERE = GRIDSIZE + 1;
    public const int NO_MAP_POS = NOWHERE;
    public const int MAPWIDTH = World.WORLD_COLS;
    public const int MAPHEIGHT = World.WORLD_ROWS;
    public const int MAPLENGTH = MAPHEIGHT * MAPWIDTH;
    public const int NUM_PANIC_TRIGGERS = 3;
    public const int WORLD_TILE_X = 40;
    public const int WORLD_TILE_Y = 20;
    public const int WORLD_COLS = 160;
    public const int WORLD_ROWS = 160;
    public const int WORLD_COORD_COLS = 1600;
    public const int WORLD_COORD_ROWS = 1600;
    public const int WORLD_MAX = 25600;
    public const int CELL_X_SIZE = 10;
    public const int CELL_Y_SIZE = 10;

    // what are we showing?..teams/vehicles
    // Show values
    public const int SHOW_TEAMMATES = 1;
    public const int SHOW_VEHICLES = 2;

    public const int WORLD_BASE_HEIGHT = 0;
    public const int WORLD_CLIFF_HEIGHT = 80;

    public const int NO_ROOM = 0;
    public const int MAX_ROOMS = 250;

    public static int guiLevelNodes { get; set; } = 0;

    public static int gsRecompileAreaTop { get; set; } = 0;
    public static int gsRecompileAreaLeft { get; set; } = 0;
    public static int gsRecompileAreaRight { get; set; } = 0;
    public static int gsRecompileAreaBottom { get; set; } = 0;

    public const int MAX_OBJECTS_PER_SLOT = 8;
    public const int MAX_ATTACHMENTS = 4;
    public const int MAX_MONEY_PER_SLOT = 20000;

    public static Dictionary<Items, INVTYPE> Item = new();

    // Room Information
    public static int[] gubWorldRoomInfo = new int[WORLD_MAX];
    public static int[] gubWorldRoomHidden = new int[MAX_ROOMS];

    public static StrategicMapElement[] StrategicMap = new StrategicMapElement[Globals.MAP_WORLD_X * Globals.MAP_WORLD_Y];
    public static List<GARRISON_GROUP> gGarrisonGroup = new();

    public static bool gfApplyChangesToTempFile { get; set; } = false;


    public string? gubErrorText;
    public bool gfAniEditMode;
    public static bool gfEditMode;
    public bool fFirstTimeInGameScreen;
    public bool fDirtyRectangleMode;
    public string? gDebugStr;
    public string? gSystemDebugStr;

    public bool gfMode;
    public static int gsCurrentActionPoints;
    public int gbFPSDisplay;
    public bool gfResetInputCheck;
    public static bool gfGlobalError;

    public static int gubCurrentScene = 0;

    public static int guiGameCycleCounter;

    public const int MAX_POPUP_BOX_COUNT = 20;
    public const int MAX_POPUP_BOX_STRING_COUNT = 50;		// worst case = 45: move menu with 20 soldiers, each on different squad + overhead

    // VIDEO OVERLAYS 
    public int giFPSOverlay;
    public int giCounterPeriodOverlay;
    public static bool gfProgramIsRunning { get; set; } // Turn this to false to exit program

    // World Data
    public static List<MAP_ELEMENT> gpWorldLevelData { get; set; } = new();

    // World Movement Costs
    public static int[,,] gubWorldMovementCosts = new int[World.WORLD_MAX, World.MAXDIR, 2];
    public static int[,] gszTerrain = new int[(int)Traversability.NUM_TRAVTERRAIN_TYPES, 15];

    public static Dictionary<NPCID, MERCPROFILESTRUCT> gMercProfiles { get; } = new();

    public static TacticalStatusType gTacticalStatus { get; set; } = new TacticalStatusType();
    public static TILE_ELEMENT[] gTileDatabase = new TILE_ELEMENT[(int)TileDefines.NUMBEROFTILES];

    public static SOLDIERTYPE[] Menptr = new SOLDIERTYPE[TOTAL_SOLDIERS];
    public static SOLDIERTYPE[] MercPtrs = new SOLDIERTYPE[TOTAL_SOLDIERS];

    // MERC SLOTS - A LIST OF ALL ACTIVE MERCS
    public static SOLDIERTYPE[] MercSlots = new SOLDIERTYPE[TOTAL_SOLDIERS];
    public static int guiNumMercSlots { get; set; }

    // These HAVE to total 100% at all times!!!
    public const int PROGRESS_PORTION_KILLS = 25;
    public const int PROGRESS_PORTION_CONTROL = 25;
    public const int PROGRESS_PORTION_INCOME = 50;

    public static int gusSelectedSoldier { get; set; }
    public static int gusOldSelectedSoldier { get; set; }

    public static TEAM gbPlayerNum { get; set; }
    public static bool gbShowEnemies { get; set; }
    public static bool gUIActionModeChangeDueToMouseOver { get; internal set; }
    public static bool gfUIForceReExamineCursorData { get; internal set; }

    public static int[,] gusAnimInst = new int[AnimationControl.MAX_ANIMATIONS, AnimationControl.MAX_FRAMES_PER_ANIM];
    public static Dictionary<AnimationStates, ANIMCONTROLTYPE> gAnimControl = new();

    public static ANI_SPEED_DEF[] gubAnimWalkSpeeds = new ANI_SPEED_DEF[(int)SoldierBodyTypes.TOTALBODYTYPES];
    public static ANI_SPEED_DEF[] gubAnimRunSpeeds = new ANI_SPEED_DEF[(int)SoldierBodyTypes.TOTALBODYTYPES];
    public static ANI_SPEED_DEF[] gubAnimSwatSpeeds = new ANI_SPEED_DEF[(int)SoldierBodyTypes.TOTALBODYTYPES];
    public static ANI_SPEED_DEF[] gubAnimCrawlSpeeds = new ANI_SPEED_DEF[(int)SoldierBodyTypes.TOTALBODYTYPES];
    public static int[] gubMaxActionPoints = new int[(int)SoldierBodyTypes.TOTALBODYTYPES];

    public static Dictionary<SEC, SECTORINFO> SectorInfo = new();


    public static SOLDIERTYPE? gpRequesterMerc = null;
    public static SOLDIERTYPE? gpRequesterTargetMerc = null;
    public static int gsRequesterGridNo;
    public static int gsOverItemsGridNo = (int)Globals.NOWHERE;
    public static int gsOverItemsLevel = 0;
    public static bool gfUIInterfaceSetBusy = false;
    public static uint guiUIInterfaceBusyTime = 0;

    public static bool gfTacticalForceNoCursor = false;
    public static LEVELNODE? gpInvTileThatCausedMoveConfirm = null;
    public static bool gfResetUIMovementOptimization = false;
    public static bool gfResetUIItemCursorOptimization = false;
    public static bool gfBeginVehicleCursor = false;
    public static int gsOutOfRangeGridNo = Globals.NOWHERE;
    public static byte gubOutOfRangeMerc = Globals.NOBODY;
    public static bool gfOKForExchangeCursor = false;
    public static uint guiUIInterfaceSwapCursorsTime = 0;
    public static int gsJumpOverGridNo = 0;
    public static Dictionary<UI_EVENT_DEFINES, UI_EVENT> gEvents = new();

    public static UI_MODE gCurrentUIMode = UI_MODE.IDLE_MODE;
    public static UI_MODE gOldUIMode = UI_MODE.IDLE_MODE;
    public static UI_EVENT_DEFINES guiCurrentEvent = UI_EVENT_DEFINES.I_DO_NOTHING;
    public static UI_EVENT_DEFINES guiOldEvent = UI_EVENT_DEFINES.I_DO_NOTHING;
    public static UICursorDefines guiCurrentUICursor = UICursorDefines.NO_UICURSOR;
    public static UICursorDefines guiNewUICursor = UICursorDefines.NORMAL_SNAPUICURSOR;
    public static UI_EVENT_DEFINES guiPendingOverrideEvent = UI_EVENT_DEFINES.I_DO_NOTHING;
    public static int gusSavedMouseX;
    public static int gusSavedMouseY;
    // UIKEYBOARD_HOOK gUIKeyboardHook = null;

    public static bool gfDisplayTimerCursor = false;
    public static UICursorDefines guiTimerCursorID = 0;
    public static uint guiTimerLastUpdate = 0;
    public static uint guiTimerCursorDelay = 0;

    public static bool gfUISelectiveTargetFound;
    public static int gusUISelectiveTargetID;
    public static FIND_SOLDIER_RESPONSES guiUISelectiveTargetFlags;
    public static bool gfUIFullTargetFound;
    public static int gusUIFullTargetID;
    public static FIND_SOLDIER_RESPONSES guiUIFullTargetFlags;

    public const int MERC_HIRE_OVER_20_MERCS_HIRED = -1;
    public const int MERC_HIRE_FAILED = 0;
    public const int MERC_HIRE_OK = 1;
    public const int MERC_ARRIVE_TIME_SLOT_1 = (7 * 60 + 30);	// 7:30 a.m.
    public const int MERC_ARRIVE_TIME_SLOT_2 = (13 * 60 + 30);// 1:30 pm
    public const int MERC_ARRIVE_TIME_SLOT_3 = (19 * 60 + 30);// 7:30 pm


    public const int LOYALTY_LOW_THRESHOLD = 30;
    public const int LOYALTY_OK_THRESHOLD = 50;
    public const int LOYALTY_HIGH_THRESHOLD = 80;

    public const int MIN_RATING_TO_TRAIN_TOWN = 20;

    public const int NUM_SEC_IN_DAY = 86400;
    public const int NUM_SEC_IN_HOUR = 3600;
    public const int NUM_SEC_IN_MIN = 60;

    public const int OKBREATH = 10;
    public const int OKLIFE = 15;
    public const int CONSCIOUSNESS = 10;

    public const NPCID FIRST_RPC = (NPCID)57;

    public static int[] guiPlottedPath = new int[256];
    public static int[] guiPathingData = new int[256];
    public static int giPathDataSize;
    public static int giPlotCnt;
    public static int guiEndPlotGridNo;

    public const int NO_GUY_SELECTION = 0;
    public const int SELECTED_GUY_SELECTION = 1;
    public const int NONSELECTED_GUY_SELECTION = 2;
    public const int ENEMY_GUY_SELECTION = 3;


    public const int QUESTNOTSTARTED = 0;
    public const int QUESTINPROGRESS = 1;
    public const int QUESTDONE = 2;



    public static int[] gzLocation;// [20];
    public static bool gfLocation = false;

    public static bool gfUIBodyHitLocation = false;

    public static int[] gzIntTileLocation;// [20];
    public static bool gfUIIntTileLocation;

    public static int[] gzIntTileLocation2;// [20];
    public static bool gfUIIntTileLocation2;


    public static MouseRegion gDisableRegion;
    public static bool gfDisableRegionActive = false;

    public static MouseRegion gUserTurnRegion;
    public static bool gfUserTurnRegionActive = false;


    // For use with mouse button query routines
    public static bool fRightButtonDown = false;
    public static bool fLeftButtonDown = false;
    public static bool fIgnoreLeftUp = false;

    public static bool gUITargetReady = false;
    public static bool gUITargetShotWaiting = false;
    public static uint gsUITargetShotGridNo = Globals.NOWHERE;
    public static bool gUIUseReverse = false;

    public static SixLabors.ImageSharp.Rectangle gRubberBandRect = new(0, 0, 0, 0);
    public static bool gRubberBandActive = false;
    public static bool gfIgnoreOnSelectedGuy = false;
    public static bool gfViewPortAdjustedForSouth = false;

    public static int guiCreateGuyIndex = 0;
    // Temp values for placing bad guys
    public static int guiCreateBadGuyIndex = 8;

    // FLAGS
    // These flags are set for a single frame execution and then are reset for the next iteration. 
    public static bool gfUIDisplayActionPoints { get; set; } = false;
    public static bool gfUIDisplayActionPointsInvalid { get; set; } = false;
    public static bool gfUIDisplayActionPointsBlack { get; set; } = false;
    public static bool gfUIDisplayActionPointsCenter { get; set; } = false;

    public static bool gfInTalkPanel = false;
    public static SOLDIERTYPE? gpSrcSoldier = null;
    public static SOLDIERTYPE? gpDestSoldier = null;
    public static SOLDIERTYPE? gpPendingDestSoldier;
    public static SOLDIERTYPE? gpPendingSrcSoldier;

    public static NPCID gubSrcSoldierProfile;
    public static int gubNiceNPCProfile = SoldierControl.NO_PROFILE;
    public static int gubNastyNPCProfile = SoldierControl.NO_PROFILE;

    public static ScreenName guiCurrentScreen;

    public static int gUIDisplayActionPointsOffY = 0;
    public static int gUIDisplayActionPointsOffX = 0;
    public static bool gfUIDoNotHighlightSelMerc = false;
    public static int gfUIHandleSelection = 0;
    public static bool gfUIHandleSelectionAboveGuy = false;
    public static bool gfUIInDeadlock = false;
    public static byte gUIDeadlockedSoldier = Globals.NOBODY;
    public static int gfUIHandleShowMoveGrid { get; set; } = 0;
    public static int gusMouseXPos { get; internal set; }
    public static int gusMouseYPos { get; internal set; }
    public static int gsGlobalCursorYOffset { get; internal set; }
    public static bool gfScrollInertia { get; internal set; }
    public static bool gfScrollPending { get; internal set; }

    public static int gsUIHandleShowMoveGridLocation = Globals.NOWHERE;
    public static bool gfUIOverItemPool = false;
    public static int gfUIOverItemPoolGridNo = 0;
    public static bool gfUIHandlePhysicsTrajectory = false;
    public static bool gfUIMouseOnValidCatcher = false;
    public static byte gubUIValidCatcherID = 0;
    public static bool gfUIConfirmExitArrows = false;
    public static bool gfUIShowCurIntTile = false;
    public static bool gfUIWaitingForUserSpeechAdvance = false;        // Waiting for key input/mouse click to advance speech
    public static bool gfUIKeyCheatModeOn = false;     // Sets cool cheat keys on
    public static int gfUIAllMoveOn = 0;      // Sets to all move
    public static bool gfUICanBeginAllMoveCycle = false;       // GEts set so we know that the next right-click is a move-call inc\stead of a movement cycle through
    public static int gsSelectedGridNo = 0;
    public static int gsSelectedLevel = InterfaceLevel.I_GROUND_LEVEL;
    public static int gsSelectedGuy = Globals.NO_SOLDIER;
    public static bool gfUIDisplayDamage = false;
    public static byte gbDamage = 0;
    public static uint gsDamageGridNo = 0;
    public static bool gfUIRefreshArrows = false;
    // Thse flags are not re-set after each frame
    public static bool gfPlotNewMovement = false;
    public static bool gfPlotNewMovementNOCOST = false;
    public static ARROWS guiShowUPDownArrows = ARROWS.HIDE_UP | ARROWS.HIDE_DOWN;
    public static int gbAdjustStanceDiff = 0;
    public static int gbClimbID = 0;
    public static bool gfUIShowExitEast = false;
    public static bool gfUIShowExitWest = false;
    public static bool gfUIShowExitNorth = false;
    public static bool gfUIShowExitSouth = false;
    public static bool gfUIShowExitExitGrid = false;
    public static bool gfUINewStateForIntTile = false;

    public static int gWorldSectorX { get; set; }
    public static MAP_ROW gWorldSectorY { get; set; }
    public static int gbWorldSectorZ { get; set; }
    public static bool guiForceRefreshMousePositionCalculation { get; internal set; }

    public const bool NO_PLOT = false;
    public const bool PLOT = true;

    public static bool gfRenderScroll { get; set; }
    public static bool gfScrollStart { get; set; }
    public static int gsScrollXIncrement { get; set; }
    public static int gsScrollYIncrement { get; set; }
    public static ScrollDirection guiScrollDirection { get; set; }
    public static int gsRenderHeight { get; set; }
    public static bool gfGetNewPathThroughPeople { get; internal set; }
    public static bool gfIgnoreScrolling { get; internal set; }

    // GLOBAL VARIABLES
    public static int SCROLL_X_STEP;
    public static int SCROLL_Y_STEP;

    public static int gsVIEWPORT_START_X;
    public static int gsVIEWPORT_START_Y;
    public static int gsVIEWPORT_WINDOW_START_Y;
    public static int gsVIEWPORT_END_Y;
    public static int gsVIEWPORT_WINDOW_END_Y;
    public static int gsVIEWPORT_END_X;

    public static int gsRenderCenterX;
    public static int gsRenderCenterY;
    public static int gsRenderWorldOffsetX;
    public static int gsRenderWorldOffsetY;

    // CURRENT VIEWPORT IN WORLD COORDS
    public static int gsTopLeftWorldX, gsTopLeftWorldY;
    public static int gsTopRightWorldX, gsTopRightWorldY;
    public static int gsBottomLeftWorldX, gsBottomLeftWorldY;
    public static int gsBottomRightWorldX, gsBottomRightWorldY;

    public static SixLabors.ImageSharp.Rectangle gSelectRegion;
    public static SixLabors.ImageSharp.Point gSelectAnchor;

    // GLOBAL COORDINATES
    public static int gTopLeftWorldLimitX, gTopLeftWorldLimitY;
    public static int gTopRightWorldLimitX, gTopRightWorldLimitY;
    public static int gBottomLeftWorldLimitX, gBottomLeftWorldLimitY;
    public static int gBottomRightWorldLimitX, gBottomRightWorldLimitY;
    public static int gCenterWorldX, gCenterWorldY;
    public static int gsTLX, gsTLY, gsTRX, gsTRY;
    public static int gsBLX, gsBLY, gsBRX, gsBRY;
    public static int gsCX, gsCY;
    public static float gdScaleX, gdScaleY;
    public static bool fLandLayerDirty;
    public static bool gfIgnoreScrollDueToCenterAdjust;

    public static int gusAnchorMouseY;
    public static int usOldMouseY;
    public static bool ubNearHeigherLevel;
    public static bool ubNearLowerLevel;
    public static byte ubUpHeight, ubDownDepth;
    public static ARROWS uiOldShowUPDownArrows;

    public const int MAX_QUESTS = 30;
    public const int MAX_FACTS = 65536;
    public const int NUM_FACTS = 500;			//If you increase this number, add entries to the fact text list in QuestText.c

    public static Dictionary<QUEST, int> gubQuest { get; } = new();
    public static Dictionary<FACT, int> gubFact { get; } = new(); // this has to be updated when we figure out how many facts we have
    public static bool gfBoxersResting { get; internal set; }
    public static int guiHelicopterSkyriderTalkState { get; internal set; }
    public static int guiTimeOfLastSkyriderMonologue { get; internal set; }
    public static ScreenName guiPreviousOptionScreen { get; internal set; }
    public static bool gfExitDebugScreen { get; internal set; }
    public static bool gfInOpenDoorMenu { get; internal set; }
    public static bool gfNPCCircularDistLimit { get; internal set; }
    public static bool fMapPanelDirty { get; internal set; }
    public static bool fInMapMode { get; internal set; }
    public static bool gfMapPanelWasRedrawn { get; internal set; }
    public static bool fShowMapInventoryPool { get; internal set; }
    public static int ghTownMineBox { get; internal set; }
    public static int ghAttributeBox { get; internal set; }
    public static bool fSkyRiderAvailable { get; internal set; }
    public static int gubNPCDistLimit { get; internal set; }
    public static bool gfRecalculatingExistingPathCost { get; internal set; }

    public static int gsFoodQuestSectorX;
    public static int gsFoodQuestSectorY;

    public static int MAP_WORLD_X = 18;
    public static int MAP_WORLD_Y = 18;

    public static int guiUITargetSoldierId = NOBODY;

    public const int MAX_PATH_LIST_SIZE = 30;
    public const int NUM_SOLDIER_SHADES = 48;
    public const int NUM_SOLDIER_EFFECTSHADES = 2;
    //TACTICAL OVERHEAD STUFF
    public const int NO_SOLDIER = TOTAL_SOLDIERS; // SAME AS NOBODY
    public const int NOBODY = NO_SOLDIER;

    // MODIFIERS FOR AP COST FOR MOVEMENT 
    public const double RUNDIVISOR = 1.8;
    public const int WALKCOST = -1;
    public const int SWATCOST = 0;
    public const int CRAWLCOST = 1;

    public const int MAXTEAMS = 6;
    public const int MAXMERCS = MAX_NUM_SOLDIERS;

    //Global dynamic array of all of the items in a loaded map.
    public static List<WORLDITEM> gWorldItems = new();
    public static List<int> guiNumWorldItems = new();

    public static List<WORLDBOMB> gWorldBombs = new();
    public static int guiNumWorldBombs = 0;


    public static AnimationSurfaceType[] gAnimSurfaceDatabase = new AnimationSurfaceType[(int)AnimationSurfaceTypes.NUMANIMATIONSURFACETYPES];
    public static AnimationStructureType[,] gAnimStructureDatabase = new AnimationStructureType[(int)SoldierBodyTypes.TOTALBODYTYPES, (int)StructData.NUM_STRUCT_IDS];

    public static GameOptions gGameOptions = new();

    public static MAPCREATE_STRUCT gMapInformation;

    public const Items NOTHING = Items.NONE;
    public const Items ITEM_NOT_FOUND = (Items)(-1);

    public const int USABLE = 10;      // minimum work% of items to still be usable

    // border and bottom buttons
    public static GUI_BUTTON[] giMapBorderButtons = new GUI_BUTTON[6];
    public static int[] guiMapButtonInventory = { -1, -1, -1, -1, -1, -1 };

    public static List<TILE_CACHE_STRUCT> gpTileCacheStructInfo = new();
    public static List<TILE_CACHE_ELEMENT> gpTileCache = new();

    public const int TILE_CACHE_START_INDEX = 36000;
    public static int guiNumTileCacheStructs = 0;

    public static DISABLED_STYLE gbDisabledButtonStyle;
    public static GUI_BUTTON gpCurrentFastHelpButton;

    public const int MAX_GENERIC_PICS = 40;
    public const int MAX_BUTTON_ICONS = 40;
    public const int MAX_BUTTON_PICS = 256;
    public const int MAX_BUTTONS = 400;

    public const Surfaces BUTTON_USE_DEFAULT = Surfaces.Unknown;
    public static readonly int? BUTTON_NO_FILENAME = null;
    public static readonly GuiCallback BUTTON_NO_CALLBACK = (ref GUI_BUTTON o, MouseCallbackReasons r) => { };
    //public const int MAX_NUMBER_OF_MINES = (int)MINE.MAX_NUMBER_OF_MINES;
    public const int BUTTON_NO_IMAGE = -1;
    public const int BUTTON_NO_SLOT = -1;

    public const int BUTTON_INIT = 1;
    public const int BUTTON_WAS_CLICKED = 2;
    public static bool gfDelayButtonDeletion = false;
    public static bool gfPendingButtonDeletion = false;
    public const string DEFAULT_GENERIC_BUTTON_OFF = "GENBUTN.STI";
    public const string DEFAULT_GENERIC_BUTTON_ON = "GENBUTN2.STI";
    public const string DEFAULT_GENERIC_BUTTON_OFF_HI = "GENBUTN3.STI";
    public const string DEFAULT_GENERIC_BUTTON_ON_HI = "GENBUTN4.STI";

    public const int DIRTYLEVEL0 = 0;
    public const int DIRTYLEVEL1 = 1;
    public const int DIRTYLEVEL2 = 2;

    public static bool gfRenderHilights = true;

    public static GUI_BUTTON? gpAnchoredButton;
    public static GUI_BUTTON? gpPrevAnchoredButton;
    public static bool gfAnchoredState;

    public static GROUP? gpGroupList;
    public static GROUP? gpPendingSimultaneousGroup = null;


    public static UICursorDefines guiCurUICursor = UICursorDefines.NO_UICURSOR;
    public static UICursorDefines guiOldUICursor = UICursorDefines.NO_UICURSOR;
    public static int gusCurMousePos;
    public static int gusTargetDropPos;
    public static bool gfTargetDropPos = false;

    public static POPUPBOX[] PopUpBoxList = new POPUPBOX[MAX_POPUP_BOX_COUNT];

    // PopUpBox Flags
    public const int POPUP_BOX_FLAG_CLIP_TEXT = 1;
    public const int POPUP_BOX_FLAG_CENTER_TEXT = 2;
    public const int POPUP_BOX_FLAG_RESIZE = 4;
    public const int POPUP_BOX_FLAG_CAN_HIGHLIGHT_SHADED_LINES = 8;

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
    public const int INVALID_ANIMATION_SURFACE = 32000;

    public static int gubCheatLevel { get; internal set; }
    public static bool fShowAttributeMenu { get; internal set; }
    public static bool fPausedMarkButtonsDirtyFlag { get; internal set; }
    public static bool fDisableHelpTextRestoreFlag { get; internal set; }

    // wait time until temp path is drawn, from placing cursor on a map grid
    public const int MIN_WAIT_TIME_FOR_TEMP_PATH = 200;

    // number of LINKED LISTS for sets of leave items (each slot holds an unlimited # of items)
    public const int NUM_LEAVE_LIST_SLOTS = 20;
    // this table holds mine values that never change and don't need to be saved

    public static int guiBackgroundRect;
    public static bool gfExitPalEditScreen = false;
    public static bool gfInitRect = true;
    public static bool gfDoneWithSplashScreen = false;

    public static Dictionary<MINE, MINE_LOCATION_TYPE> gMineLocation = new()
    {
        { MINE.SAN_MONA, new(4, (MAP_ROW)4, TOWNS.SAN_MONA) },
        { MINE.DRASSEN, new(13, (MAP_ROW)4, TOWNS.DRASSEN ) },
        { MINE.ALMA, new(14, (MAP_ROW)9, TOWNS.ALMA) },
        { MINE.CAMBRIA, new(8,  (MAP_ROW)8, TOWNS.CAMBRIA ) },
        { MINE.CHITZENA, new(2,  (MAP_ROW)2, TOWNS.CHITZENA) },
        { MINE.GRUMM, new(3, (MAP_ROW)8, TOWNS.GRUMM) },
    };

    public static Dictionary<UICursorDefines, UICursor> gUICursors = new()
    {
        { UICursorDefines.NO_UICURSOR, new (UICursorDefines.NO_UICURSOR, 0, 0, 0) },
        { UICursorDefines.NORMAL_FREEUICURSOR, new (UICursorDefines.NORMAL_FREEUICURSOR, UICURSOR.FREEFLOWING, CURSOR.NORMAL, 0) },
        { UICursorDefines.NORMAL_SNAPUICURSOR, new (UICursorDefines.NORMAL_SNAPUICURSOR, UICURSOR.SNAPPING, 0, 0) },
        { UICursorDefines.MOVE_RUN_UICURSOR, new (UICursorDefines.MOVE_RUN_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILEAPDEPENDENT | UICURSOR.DONTSHOW2NDLEVEL, CURSOR.RUN1, TileDefines.FIRSTPOINTERS2) },
        { UICursorDefines.MOVE_WALK_UICURSOR, new(UICursorDefines.MOVE_WALK_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILEAPDEPENDENT | UICURSOR.DONTSHOW2NDLEVEL | UICURSOR.CENTERAPS, CURSOR.WALK1, TileDefines.FIRSTPOINTERS2) } ,
        { UICursorDefines.MOVE_SWAT_UICURSOR, new (UICursorDefines.MOVE_SWAT_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILEAPDEPENDENT | UICURSOR.DONTSHOW2NDLEVEL, CURSOR.SWAT1, TileDefines.FIRSTPOINTERS2) },
        { UICursorDefines.MOVE_PRONE_UICURSOR, new (UICursorDefines.MOVE_PRONE_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILEAPDEPENDENT | UICURSOR.DONTSHOW2NDLEVEL, CURSOR.PRONE1, TileDefines.FIRSTPOINTERS2) },
        { UICursorDefines.MOVE_VEHICLE_UICURSOR, new (UICursorDefines.MOVE_VEHICLE_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILEAPDEPENDENT | UICURSOR.DONTSHOW2NDLEVEL, CURSOR.DRIVEV, TileDefines.FIRSTPOINTERS2) },
        { UICursorDefines.CONFIRM_MOVE_RUN_UICURSOR, new (UICursorDefines.CONFIRM_MOVE_RUN_UICURSOR, UICURSOR.SNAPPING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, 0, TileDefines.FIRSTPOINTERS4) },
        { UICursorDefines.CONFIRM_MOVE_WALK_UICURSOR, new (UICursorDefines.CONFIRM_MOVE_WALK_UICURSOR, UICURSOR.SNAPPING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, 0, TileDefines.FIRSTPOINTERS4) },
        { UICursorDefines.CONFIRM_MOVE_SWAT_UICURSOR, new (UICursorDefines.CONFIRM_MOVE_SWAT_UICURSOR, UICURSOR.SNAPPING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, 0, TileDefines.FIRSTPOINTERS4) },
        { UICursorDefines.CONFIRM_MOVE_PRONE_UICURSOR, new (UICursorDefines.CONFIRM_MOVE_PRONE_UICURSOR, UICURSOR.SNAPPING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, 0, TileDefines.FIRSTPOINTERS4) },
        { UICursorDefines.CONFIRM_MOVE_VEHICLE_UICURSOR, new (UICursorDefines.CONFIRM_MOVE_VEHICLE_UICURSOR, UICURSOR.SNAPPING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, 0, TileDefines.FIRSTPOINTERS4) },
        { UICursorDefines.ALL_MOVE_RUN_UICURSOR, new (UICursorDefines.ALL_MOVE_RUN_UICURSOR, UICURSOR.SNAPPING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, 0, TileDefines.FIRSTPOINTERS5) },
        { UICursorDefines.ALL_MOVE_WALK_UICURSOR, new (UICursorDefines.ALL_MOVE_WALK_UICURSOR, UICURSOR.SNAPPING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, 0, TileDefines.FIRSTPOINTERS5) },
        { UICursorDefines.ALL_MOVE_SWAT_UICURSOR, new (UICursorDefines.ALL_MOVE_SWAT_UICURSOR, UICURSOR.SNAPPING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, 0, TileDefines.FIRSTPOINTERS5) },
        { UICursorDefines.ALL_MOVE_PRONE_UICURSOR, new (UICursorDefines.ALL_MOVE_PRONE_UICURSOR, UICURSOR.SNAPPING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, 0, TileDefines.FIRSTPOINTERS5) },
        { UICursorDefines.ALL_MOVE_VEHICLE_UICURSOR, new (UICursorDefines.ALL_MOVE_VEHICLE_UICURSOR, UICURSOR.SNAPPING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, 0, TileDefines.FIRSTPOINTERS5) },
        { UICursorDefines.MOVE_REALTIME_UICURSOR, new (UICursorDefines.MOVE_REALTIME_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILEAPDEPENDENT | UICURSOR.DONTSHOW2NDLEVEL, CURSOR.VIDEO_NO_CURSOR, TileDefines.FIRSTPOINTERS2) },
        { UICursorDefines.MOVE_RUN_REALTIME_UICURSOR, new (UICursorDefines.MOVE_RUN_REALTIME_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, CURSOR.VIDEO_NO_CURSOR, TileDefines.FIRSTPOINTERS7) },
        { UICursorDefines.CONFIRM_MOVE_REALTIME_UICURSOR, new (UICursorDefines.CONFIRM_MOVE_REALTIME_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, CURSOR.VIDEO_NO_CURSOR, TileDefines.FIRSTPOINTERS4) },
        { UICursorDefines.ALL_MOVE_REALTIME_UICURSOR, new (UICursorDefines.ALL_MOVE_REALTIME_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, CURSOR.VIDEO_NO_CURSOR, TileDefines.FIRSTPOINTERS5) },
        { UICursorDefines.ON_OWNED_MERC_UICURSOR, new (UICursorDefines.ON_OWNED_MERC_UICURSOR, UICURSOR.SNAPPING, 0, 0) },
        { UICursorDefines.ON_OWNED_SELMERC_UICURSOR, new (UICursorDefines.ON_OWNED_SELMERC_UICURSOR, UICURSOR.SNAPPING, 0, 0) },
        { UICursorDefines.ACTION_SHOOT_UICURSOR, new (UICursorDefines.ACTION_SHOOT_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGET, 0) },
        { UICursorDefines.ACTION_NOCHANCE_SHOOT_UICURSOR, new (UICursorDefines.ACTION_NOCHANCE_SHOOT_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETDKBLACK, 0) },
        { UICursorDefines.ACTION_NOCHANCE_BURST_UICURSOR, new (UICursorDefines.ACTION_NOCHANCE_BURST_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETBURSTDKBLACK, 0) },
        { UICursorDefines.ACTION_FLASH_TOSS_UICURSOR, new (UICursorDefines.ACTION_FLASH_TOSS_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGET, 0) },
        { UICursorDefines.ACTION_TOSS_UICURSOR, new (UICursorDefines.ACTION_TOSS_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGET, 0) },
        { UICursorDefines.ACTION_RED_TOSS_UICURSOR, new (UICursorDefines.ACTION_RED_TOSS_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETRED, 0) },
        { UICursorDefines.ACTION_FLASH_SHOOT_UICURSOR, new (UICursorDefines.ACTION_FLASH_SHOOT_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.FLASH_TARGET, 0) },
        { UICursorDefines.ACTION_FLASH_BURST_UICURSOR, new (UICursorDefines.ACTION_FLASH_BURST_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.FLASH_TARGETBURST, 0) },
        { UICursorDefines.ACTION_TARGETAIM1_UICURSOR, new (UICursorDefines.ACTION_TARGETAIM1_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETON1, 0) },
        { UICursorDefines.ACTION_TARGETAIM2_UICURSOR, new (UICursorDefines.ACTION_TARGETAIM2_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETON2, 0) },
        { UICursorDefines.ACTION_TARGETAIM3_UICURSOR, new (UICursorDefines.ACTION_TARGETAIM3_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETON3, 0) },
        { UICursorDefines.ACTION_TARGETAIM4_UICURSOR, new (UICursorDefines.ACTION_TARGETAIM4_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETON4, 0) },
        { UICursorDefines.ACTION_TARGETAIM5_UICURSOR, new (UICursorDefines.ACTION_TARGETAIM5_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETON5, 0) },
        { UICursorDefines.ACTION_TARGETAIM6_UICURSOR, new (UICursorDefines.ACTION_TARGETAIM6_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETON6, 0) },
        { UICursorDefines.ACTION_TARGETAIM7_UICURSOR, new (UICursorDefines.ACTION_TARGETAIM7_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETON7, 0) },
        { UICursorDefines.ACTION_TARGETAIM8_UICURSOR, new (UICursorDefines.ACTION_TARGETAIM8_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETON8, 0) },
        { UICursorDefines.ACTION_TARGETAIM8_UICURSOR, new (UICursorDefines.ACTION_TARGETAIM8_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETON9, 0) },
        { UICursorDefines.ACTION_TARGETAIMCANT1_UICURSOR, new (UICursorDefines.ACTION_TARGETAIMCANT1_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETW1, 0) },
        { UICursorDefines.ACTION_TARGETAIMCANT2_UICURSOR, new (UICursorDefines.ACTION_TARGETAIMCANT2_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETW2, 0) },
        { UICursorDefines.ACTION_TARGETAIMCANT3_UICURSOR, new (UICursorDefines.ACTION_TARGETAIMCANT3_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETW3, 0) },
        { UICursorDefines.ACTION_TARGETAIMCANT4_UICURSOR, new (UICursorDefines.ACTION_TARGETAIMCANT4_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETW4, 0) },
        { UICursorDefines.ACTION_TARGETAIMCANT5_UICURSOR, new (UICursorDefines.ACTION_TARGETAIMCANT5_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETW5, 0) },
        { UICursorDefines.ACTION_TARGETRED_UICURSOR, new (UICursorDefines.ACTION_TARGETRED_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETRED, 0) },
        { UICursorDefines.ACTION_TARGETBURST_UICURSOR, new (UICursorDefines.ACTION_TARGETBURST_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETBURST, 0) },
        { UICursorDefines.ACTION_TARGETREDBURST_UICURSOR, new (UICursorDefines.ACTION_TARGETREDBURST_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETBURSTRED, 0) },
        { UICursorDefines.ACTION_TARGETCONFIRMBURST_UICURSOR, new (UICursorDefines.ACTION_TARGETCONFIRMBURST_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETBURSTCONFIRM, 0) },
        { UICursorDefines.ACTION_TARGETAIMFULL_UICURSOR, new (UICursorDefines.ACTION_TARGETAIMFULL_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETWR1, 0) },
        { UICursorDefines.ACTION_TARGETAIMYELLOW1_UICURSOR, new (UICursorDefines.ACTION_TARGETAIMYELLOW1_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETYELLOW1, 0) },
        { UICursorDefines.ACTION_TARGETAIMYELLOW2_UICURSOR, new (UICursorDefines.ACTION_TARGETAIMYELLOW2_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETYELLOW2, 0) },
        { UICursorDefines.ACTION_TARGETAIMYELLOW3_UICURSOR, new (UICursorDefines.ACTION_TARGETAIMYELLOW3_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETYELLOW3, 0) },
        { UICursorDefines.ACTION_TARGETAIMYELLOW4_UICURSOR, new (UICursorDefines.ACTION_TARGETAIMYELLOW4_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETYELLOW4, 0) },
        { UICursorDefines.ACTION_TARGET_RELOADING, new (UICursorDefines.ACTION_TARGET_RELOADING, UICURSOR.FREEFLOWING, CURSOR.TARGETBLACK, 0) },
        { UICursorDefines.ACTION_PUNCH_GRAY, new (UICursorDefines.ACTION_PUNCH_GRAY, UICURSOR.FREEFLOWING, CURSOR.PUNCHGRAY, 0) },
        { UICursorDefines.ACTION_PUNCH_RED, new (UICursorDefines.ACTION_PUNCH_RED, UICURSOR.FREEFLOWING, CURSOR.PUNCHRED, 0) },
        { UICursorDefines.ACTION_PUNCH_RED_AIM1_UICURSOR, new (UICursorDefines.ACTION_PUNCH_RED_AIM1_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.PUNCHRED_ON1, 0) },
        { UICursorDefines.ACTION_PUNCH_RED_AIM2_UICURSOR, new (UICursorDefines.ACTION_PUNCH_RED_AIM2_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.PUNCHRED_ON2, 0) },
        { UICursorDefines.ACTION_PUNCH_YELLOW_AIM1_UICURSOR, new (UICursorDefines.ACTION_PUNCH_YELLOW_AIM1_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.PUNCHYELLOW_ON1, 0) },
        { UICursorDefines.ACTION_PUNCH_YELLOW_AIM2_UICURSOR, new (UICursorDefines.ACTION_PUNCH_YELLOW_AIM2_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.PUNCHYELLOW_ON2, 0) },
        { UICursorDefines.ACTION_PUNCH_NOGO_AIM1_UICURSOR, new (UICursorDefines.ACTION_PUNCH_NOGO_AIM1_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.PUNCHNOGO_ON1, 0) },
        { UICursorDefines.ACTION_PUNCH_NOGO_AIM2_UICURSOR, new (UICursorDefines.ACTION_PUNCH_NOGO_AIM2_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.PUNCHNOGO_ON2, 0) },
        { UICursorDefines.ACTION_FIRSTAID_GRAY, new (UICursorDefines.ACTION_FIRSTAID_GRAY, UICURSOR.FREEFLOWING, CURSOR.CROSS_REG, 0) },
        { UICursorDefines.ACTION_FIRSTAID_RED, new (UICursorDefines.ACTION_FIRSTAID_RED, UICURSOR.FREEFLOWING, CURSOR.CROSS_ACTIVE, 0) },
        { UICursorDefines.ACTION_OPEN, new (UICursorDefines.ACTION_OPEN, UICURSOR.FREEFLOWING, CURSOR.HANDGRAB, 0) },
        { UICursorDefines.CANNOT_MOVE_UICURSOR, new (UICursorDefines.CANNOT_MOVE_UICURSOR, UICURSOR.SNAPPING, 0, 0) },
        { UICursorDefines.NORMALHANDCURSOR_UICURSOR, new (UICursorDefines.NORMALHANDCURSOR_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.NORMGRAB, 0) },
        { UICursorDefines.OKHANDCURSOR_UICURSOR, new (UICursorDefines.OKHANDCURSOR_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.HANDGRAB, 0) },
        { UICursorDefines.KNIFE_REG_UICURSOR, new (UICursorDefines.KNIFE_REG_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.KNIFE_REG, 0) },
        { UICursorDefines.KNIFE_HIT_UICURSOR, new (UICursorDefines.KNIFE_HIT_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.KNIFE_HIT, 0) },
        { UICursorDefines.KNIFE_HIT_AIM1_UICURSOR, new (UICursorDefines.KNIFE_HIT_AIM1_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.KNIFE_HIT_ON1, 0) },
        { UICursorDefines.KNIFE_HIT_AIM2_UICURSOR, new (UICursorDefines.KNIFE_HIT_AIM2_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.KNIFE_HIT_ON2, 0) },
        { UICursorDefines.KNIFE_YELLOW_AIM1_UICURSOR, new (UICursorDefines.KNIFE_YELLOW_AIM1_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.KNIFE_YELLOW_ON1, 0) },
        { UICursorDefines.KNIFE_YELLOW_AIM2_UICURSOR, new (UICursorDefines.KNIFE_YELLOW_AIM2_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.KNIFE_YELLOW_ON2, 0) },
        { UICursorDefines.KNIFE_NOGO_AIM1_UICURSOR, new (UICursorDefines.KNIFE_NOGO_AIM1_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.KNIFE_NOGO_ON1, 0) },
        { UICursorDefines.KNIFE_NOGO_AIM2_UICURSOR, new (UICursorDefines.KNIFE_NOGO_AIM2_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.KNIFE_NOGO_ON2, 0) },
        { UICursorDefines.LOOK_UICURSOR, new (UICursorDefines.LOOK_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.LOOK, 0) },
        { UICursorDefines.TALK_NA_UICURSOR, new (UICursorDefines.TALK_NA_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TALK, 0) },
        { UICursorDefines.TALK_A_UICURSOR, new (UICursorDefines.TALK_A_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.REDTALK, 0) },
        { UICursorDefines.TALK_OUT_RANGE_NA_UICURSOR, new (UICursorDefines.TALK_OUT_RANGE_NA_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.FLASH_TALK, (ushort)CURSOR.BLACKTALK) },
        { UICursorDefines.TALK_OUT_RANGE_A_UICURSOR, new (UICursorDefines.TALK_OUT_RANGE_A_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.FLASH_REDTALK, (ushort)CURSOR.BLACKTALK) },
        { UICursorDefines.EXIT_NORTH_UICURSOR, new (UICursorDefines.EXIT_NORTH_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.EXIT_NORTH, 0) },
        { UICursorDefines.EXIT_SOUTH_UICURSOR, new (UICursorDefines.EXIT_SOUTH_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.EXIT_SOUTH, 0) },
        { UICursorDefines.EXIT_EAST_UICURSOR, new (UICursorDefines.EXIT_EAST_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.EXIT_EAST, 0) },
        { UICursorDefines.EXIT_WEST_UICURSOR, new (UICursorDefines.EXIT_WEST_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.EXIT_WEST, 0) },
        { UICursorDefines.EXIT_GRID_UICURSOR, new (UICursorDefines.EXIT_GRID_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.EXIT_GRID, 0) },
        { UICursorDefines.NOEXIT_NORTH_UICURSOR, new (UICursorDefines.NOEXIT_NORTH_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.NOEXIT_NORTH, 0) },
        { UICursorDefines.NOEXIT_SOUTH_UICURSOR, new (UICursorDefines.NOEXIT_SOUTH_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.NOEXIT_SOUTH, 0) },
        { UICursorDefines.NOEXIT_EAST_UICURSOR, new (UICursorDefines.NOEXIT_EAST_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.NOEXIT_EAST, 0) },
        { UICursorDefines.NOEXIT_WEST_UICURSOR, new (UICursorDefines.NOEXIT_WEST_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.NOEXIT_WEST, 0) },
        { UICursorDefines.NOEXIT_GRID_UICURSOR, new (UICursorDefines.NOEXIT_GRID_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.NOEXIT_GRID, 0) },
        { UICursorDefines.CONFIRM_EXIT_NORTH_UICURSOR, new (UICursorDefines.CONFIRM_EXIT_NORTH_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.CONEXIT_NORTH, 0) },
        { UICursorDefines.CONFIRM_EXIT_SOUTH_UICURSOR, new (UICursorDefines.CONFIRM_EXIT_SOUTH_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.CONEXIT_SOUTH, 0) },
        { UICursorDefines.CONFIRM_EXIT_EAST_UICURSOR, new (UICursorDefines.CONFIRM_EXIT_EAST_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.CONEXIT_EAST, 0) },
        { UICursorDefines.CONFIRM_EXIT_WEST_UICURSOR, new (UICursorDefines.CONFIRM_EXIT_WEST_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.CONEXIT_WEST, 0) },
        { UICursorDefines.CONFIRM_EXIT_GRID_UICURSOR, new (UICursorDefines.CONFIRM_EXIT_GRID_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.CONEXIT_GRID, 0) },
        { UICursorDefines.GOOD_WIRECUTTER_UICURSOR, new (UICursorDefines.GOOD_WIRECUTTER_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.GOOD_WIRECUT, 0) },
        { UICursorDefines.BAD_WIRECUTTER_UICURSOR, new (UICursorDefines.BAD_WIRECUTTER_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.BAD_WIRECUT, 0) },
        { UICursorDefines.GOOD_REPAIR_UICURSOR, new (UICursorDefines.GOOD_REPAIR_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.REPAIR, 0) },
        { UICursorDefines.BAD_REPAIR_UICURSOR, new (UICursorDefines.BAD_REPAIR_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.REPAIRRED, 0) },
        { UICursorDefines.GOOD_RELOAD_UICURSOR, new (UICursorDefines.GOOD_RELOAD_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.GOOD_RELOAD, 0) },
        { UICursorDefines.BAD_RELOAD_UICURSOR, new (UICursorDefines.BAD_RELOAD_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.BAD_RELOAD, 0) },
        { UICursorDefines.GOOD_JAR_UICURSOR, new (UICursorDefines.GOOD_JAR_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.JARRED, 0) },
        { UICursorDefines.BAD_JAR_UICURSOR, new (UICursorDefines.BAD_JAR_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.JAR, 0) },
        { UICursorDefines.GOOD_THROW_UICURSOR, new (UICursorDefines.GOOD_THROW_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.GOOD_THROW, 0) },
        { UICursorDefines.BAD_THROW_UICURSOR, new (UICursorDefines.BAD_THROW_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.BAD_THROW, 0) },
        { UICursorDefines.RED_THROW_UICURSOR, new (UICursorDefines.RED_THROW_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.RED_THROW, 0) },
        { UICursorDefines.FLASH_THROW_UICURSOR, new (UICursorDefines.FLASH_THROW_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.FLASH_THROW, 0) },
        { UICursorDefines.ACTION_THROWAIM1_UICURSOR, new (UICursorDefines.ACTION_THROWAIM1_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.THROWKON1, 0) },
        { UICursorDefines.ACTION_THROWAIM2_UICURSOR, new (UICursorDefines.ACTION_THROWAIM2_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.THROWKON2, 0) },
        { UICursorDefines.ACTION_THROWAIM3_UICURSOR, new (UICursorDefines.ACTION_THROWAIM3_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.THROWKON3, 0) },
        { UICursorDefines.ACTION_THROWAIM4_UICURSOR, new (UICursorDefines.ACTION_THROWAIM4_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.THROWKON4, 0) },
        { UICursorDefines.ACTION_THROWAIM5_UICURSOR, new (UICursorDefines.ACTION_THROWAIM5_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.THROWKON5, 0) },
        { UICursorDefines.ACTION_THROWAIM6_UICURSOR, new (UICursorDefines.ACTION_THROWAIM6_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.THROWKON6, 0) },
        { UICursorDefines.ACTION_THROWAIM7_UICURSOR, new (UICursorDefines.ACTION_THROWAIM7_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.THROWKON7, 0) },
        { UICursorDefines.ACTION_THROWAIM8_UICURSOR, new (UICursorDefines.ACTION_THROWAIM8_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.THROWKON8, 0) },
        { UICursorDefines.ACTION_THROWAIM8_UICURSOR, new (UICursorDefines.ACTION_THROWAIM8_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.THROWKON9, 0) },
        { UICursorDefines.ACTION_THROWAIMCANT1_UICURSOR, new (UICursorDefines.ACTION_THROWAIMCANT1_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.THROWKW1, 0) },
        { UICursorDefines.ACTION_THROWAIMCANT2_UICURSOR, new (UICursorDefines.ACTION_THROWAIMCANT2_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.THROWKW2, 0) },
        { UICursorDefines.ACTION_THROWAIMCANT3_UICURSOR, new (UICursorDefines.ACTION_THROWAIMCANT3_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.THROWKW3, 0) },
        { UICursorDefines.ACTION_THROWAIMCANT4_UICURSOR, new (UICursorDefines.ACTION_THROWAIMCANT4_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.THROWKW4, 0) },
        { UICursorDefines.ACTION_THROWAIMCANT5_UICURSOR, new (UICursorDefines.ACTION_THROWAIMCANT5_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.THROWKW5, 0) },
        { UICursorDefines.ACTION_THROWAIMFULL_UICURSOR, new (UICursorDefines.ACTION_THROWAIMFULL_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.THROWKWR1, 0) },
        { UICursorDefines.ACTION_THROWAIMYELLOW1_UICURSOR, new (UICursorDefines.ACTION_THROWAIMYELLOW1_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.THROWKYELLOW1, 0) },
        { UICursorDefines.ACTION_THROWAIMYELLOW2_UICURSOR, new (UICursorDefines.ACTION_THROWAIMYELLOW2_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.THROWKYELLOW2, 0) },
        { UICursorDefines.ACTION_THROWAIMYELLOW3_UICURSOR, new (UICursorDefines.ACTION_THROWAIMYELLOW3_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.THROWKYELLOW3, 0) },
        { UICursorDefines.ACTION_THROWAIMYELLOW4_UICURSOR, new (UICursorDefines.ACTION_THROWAIMYELLOW4_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.THROWKYELLOW4, 0) },
        { UICursorDefines.THROW_ITEM_GOOD_UICURSOR, new (UICursorDefines.THROW_ITEM_GOOD_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.ITEM_GOOD_THROW, 0) },
        { UICursorDefines.THROW_ITEM_BAD_UICURSOR, new (UICursorDefines.THROW_ITEM_BAD_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.ITEM_BAD_THROW, 0) },
        { UICursorDefines.THROW_ITEM_RED_UICURSOR, new (UICursorDefines.THROW_ITEM_RED_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.ITEM_RED_THROW, 0) },
        { UICursorDefines.THROW_ITEM_FLASH_UICURSOR, new (UICursorDefines.THROW_ITEM_FLASH_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.ITEM_FLASH_THROW, 0) },
        { UICursorDefines.PLACE_BOMB_GREY_UICURSOR, new (UICursorDefines.PLACE_BOMB_GREY_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.BOMB_GRAY, 0) },
        { UICursorDefines.PLACE_BOMB_RED_UICURSOR, new (UICursorDefines.PLACE_BOMB_RED_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.BOMB_RED, 0) },
        { UICursorDefines.PLACE_REMOTE_GREY_UICURSOR, new (UICursorDefines.PLACE_REMOTE_GREY_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.REMOTE_GRAY, 0) },
        { UICursorDefines.PLACE_REMOTE_RED_UICURSOR, new (UICursorDefines.PLACE_REMOTE_RED_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.REMOTE_RED, 0) },
        { UICursorDefines.PLACE_TINCAN_GREY_UICURSOR, new (UICursorDefines.PLACE_TINCAN_GREY_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.CAN, 0) },
        { UICursorDefines.PLACE_TINCAN_RED_UICURSOR, new (UICursorDefines.PLACE_TINCAN_RED_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.CANRED, 0) },
        { UICursorDefines.ENTER_VEHICLE_UICURSOR, new (UICursorDefines.ENTER_VEHICLE_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.ENTERV, 0) },
        { UICursorDefines.INVALID_ACTION_UICURSOR, new (UICursorDefines.INVALID_ACTION_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.INVALID_ACTION, 0) },
        { UICursorDefines.FLOATING_X_UICURSOR, new (UICursorDefines.FLOATING_X_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.X, 0) },
        { UICursorDefines.EXCHANGE_PLACES_UICURSOR, new (UICursorDefines.EXCHANGE_PLACES_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.EXCHANGE_PLACES, 0) },
        { UICursorDefines.JUMP_OVER_UICURSOR, new (UICursorDefines.JUMP_OVER_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.JUMP_OVER, 0) },
        { UICursorDefines.REFUEL_GREY_UICURSOR, new (UICursorDefines.REFUEL_GREY_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.FUEL, 0) },
        { UICursorDefines.REFUEL_RED_UICURSOR, new(UICursorDefines.REFUEL_RED_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.FUEL_RED, 0) },
};

    public static int gsInterfaceLevel { get; set; }
    public InterfacePanelDefines gsCurInterfacePanel { get; internal set; }
    public static MouseRegion? gViewportRegion { get; set; }
    public static bool gfUIStanceDifferent { get; set; }

    public const int ROOF_LEVEL_HEIGHT = 50;

    // the big map .pcx
    public static int guiBIGMAP = 0;

    public static uint guiBOTTOMPANEL { get; set; }
    public static uint guiRIGHTPANEL { get; set; }
    public static uint guiRENDERBUFFER { get; set; }
    public static uint guiSAVEBUFFER { get; set; }
    public static uint guiEXTRABUFFER { get; set; }
    public static bool gfExtraBuffer { get; set; }

    public static OBJECTTYPE? gpItemPointer { get; set; } = null;

    // graphics
    public static int guiMapBorder;
    //UINT32 guiMapBorderCorner;


    // scroll direction
    public static int giScrollButtonState = -1;

    public static int gbPixelDepth { get; set; }
    public static bool gfInvalidTraversal { get; internal set; }
    public static bool gfLoneEPCAttemptingTraversal { get; internal set; }
    public static int gubLoneMercAttemptingToAbandonEPCs { get; internal set; }
    public static int gbPotentiallyAbandonedEPCSlotID { get; internal set; }
    public static bool gfRobotWithoutControllerAttemptingTraversal { get; internal set; }

    public const int INVALID_STRUCTURE_ID = (Globals.TOTAL_SOLDIERS + 100);
    public const int IGNORE_PEOPLE_STRUCTURE_ID = (Globals.TOTAL_SOLDIERS + 101);
    public const int FIRST_AVAILABLE_STRUCTURE_ID = (INVALID_STRUCTURE_ID + 2);

    public static int gusNextAvailableStructureID = FIRST_AVAILABLE_STRUCTURE_ID;


    public static string[] gzLateLocalizedString = new[]
{
    "%S loadscreen data file not found...",

	//1-5
	"The robot cannot leave this sector when nobody is using the controller.",

	//This message comes up if you have pending bombs waiting to explode in tactical.
	"You can't compress time right now.  Wait for the fireworks!",  

	//'Name' refuses to move.
	"%s refuses to move.",

	//%s a merc name
	"%s does not have enough energy to change stance.",

	//A message that pops up when a vehicle runs out of gas.
	"The %s has run out of gas and is now stranded in %c%d.",

	//6-10

	// the following two strings are combined with the pNewNoise[] strings above to report noises
	// heard above or below the merc
	"above",
    "below",

	//The following strings are used in autoresolve for autobandaging related feedback.
	"None of your mercs have any medical ability.",
    "There are no medical supplies to perform bandaging.",
    "There weren't enough medical supplies to bandage everybody.",
    "None of your mercs need bandaging.",
    "Bandages mercs automatically.",
    "All your mercs are bandaged.",

	//14
	"Arulco",

  "(roof)",

    "Health: %d/%d",

	//In autoresolve if there were 5 mercs fighting 8 enemies the text would be "5 vs. 8"
	//"vs." is the abbreviation of versus.
	"%d vs. %d",

    "The %s is full!",  //(ex "The ice cream truck is full")

  "%s does not need immediate first aid or bandaging but rather more serious medical attention and/or rest.",

	//20
	//Happens when you get shot in the legs, and you fall down.
	"%s is hit in the leg and collapses!",
	//Name can't speak right now.
	"%s can't speak right now.",

	//22-24 plural versions 
	"%d green militia have been promoted to veteran militia.",
    "%d green militia have been promoted to regular militia.",
    "%d regular militia have been promoted to veteran militia.",

	//25
	"Switch",

	//26
	//Name has gone psycho -- when the game forces the player into burstmode (certain unstable characters)
	"%s goes psycho!",

	//27-28
	//Messages why a player can't time compress.
	"It is currently unsafe to compress time because you have mercs in sector %s.",
    "It is currently unsafe to compress time when mercs are in the creature infested mines.",

	//29-31 singular versions 
	"1 green militia has been promoted to a veteran militia.",
    "1 green militia has been promoted to a regular militia.",
    "1 regular militia has been promoted to a veteran militia.",

	//32-34
	"%s doesn't say anything.",
    "Travel to surface?",
    "(Squad %d)",

	//35
	//Ex: "Red has repaired Scope's MP5K".  Careful to maintain the proper order (Red before Scope, Scope before MP5K)
	"%s has repaired %s's %s",

	//36
	"BLOODCAT",

	//37-38 "Name trips and falls"
	"%s trips and falls",
    "This item can't be picked up from here.",

	//39
	"None of your remaining mercs are able to fight.  The militia will fight the creatures on their own.",

	//40-43
	//%s is the name of merc.
	"%s ran out of medical kits!",
    "%s lacks the necessary skill to doctor anyone!",
    "%s ran out of tool kits!",
    "%s lacks the necessary skill to repair anything!",

	//44-45
	"Repair Time",
    "%s cannot see this person.",

	//46-48
	"%s's gun barrel extender falls off!",
    "No more than %d militia trainers are permitted per sector.",
  "Are you sure?",

	//49-50
	"Time Compression",
    "The vehicle's gas tank is now full.",

	//51-52 Fast help text in mapscreen.
	"Continue Time Compression (|S|p|a|c|e)",
    "Stop Time Compression (|E|s|c)",

	//53-54 "Magic has unjammed the Glock 18" or "Magic has unjammed Raven's H&K G11" 
	"%s has unjammed the %s",
    "%s has unjammed %s's %s",

	//55 
	"Can't compress time while viewing sector inventory.",

    "The Jagged Alliance 2 PLAY DISK was not found. Program will now exit.",

    "Items successfully combined.",
	
	//58
	//Displayed with the version information when cheats are enabled.
	"Current/Max Progress: %d%%/%d%%",

    "Escort John and Mary?",
	
	// 60
  "Switch Activated.",

    "%s's ceramic plates have been smashed!",
};

    public static string[] TacticalStr = new string[]
{
    "Air Raid",
    "Apply first aid automatically?",
	
	// CAMFIELD NUKE THIS and add quote #66.
	
	"%s notices that items are missing from the shipment.",
	
	// The %s is a string from pDoorTrapStrings
	
	"The lock has %s.",
    "There's no lock.",
    "Success!",
    "Failure.",
    "Success!",
    "Failure.",
    "The lock isn't trapped.",
    "Success!",
	// The %s is a merc name
	"%s doesn't have the right key.",
    "The lock is untrapped.",
    "The lock isn't trapped.",
    "Locked.",
    "DOOR",
    "TRAPPED",
    "LOCKED",
    "UNLOCKED",
    "SMASHED",
    "There's a switch here.  Activate it?",
    "Disarm trap?",
    "Prev...",
    "Next...",
    "More...",

	// In the next 2 strings, %s is an item name

	"The %s has been placed on the ground.",
    "The %s has been given to %s.",

	// In the next 2 strings, %s is a name

	"%s has been paid in full.",
    "%s is still owed %d.",
    "Choose detonation frequency:",  	//in this case, frequency refers to a radio signal
	"How many turns 'til she blows:",	//how much time, in turns, until the bomb blows
	"Set remote detonator frequency:", 	//in this case, frequency refers to a radio signal
	"Disarm boobytrap?",
    "Remove blue flag?",
    "Put blue flag here?",
    "Ending Turn",

	// In the next string, %s is a name. Stance refers to way they are standing.

	"You sure you want to attack %s ?",
    "Ah, vehicles can't change stance.",
    "The robot can't change its stance.",

	// In the next 3 strings, %s is a name

	"%s can't change to that stance here.",
    "%s can't have first aid done here.",
    "%s doesn't need first aid.",
    "Can't move there.",
    "Your team's full. No room for a recruit.",	//there's no room for a recruit on the player's team

	// In the next string, %s is a name

	"%s has been recruited.",

	// Here %s is a name and %d is a number

	"%s is owed $%d.",

	// In the next string, %s is a name

	"Escort %s?",

	// In the next string, the first %s is a name and the second %s is an amount of money (including $ sign)

	"Hire %s for %s per day?",

	// This line is used repeatedly to ask player if they wish to participate in a boxing match. 

	"You want to fight?",

	// In the next string, the first %s is an item name and the 
	// second %s is an amount of money (including $ sign)

	"Buy %s for %s?",

	// In the next string, %s is a name

	"%s is being escorted on squad %d.",

	// These messages are displayed during play to alert the player to a particular situation

	"JAMMED",					//weapon is jammed.
	"Robot needs %s caliber ammo.",		//Robot is out of ammo
	"Throw there? Not gonna happen.",		//Merc can't throw to the destination he selected

	// These are different buttons that the player can turn on and off.

	"Stealth Mode (|Z)",
    "|Map Screen",
    "|Done (End Turn)",
    "Talk",
    "Mute",
    "Stance Up (|P|g|U|p)",
    "Cursor Level (|T|a|b)",
    "Climb / Jump",
    "Stance Down (|P|g|D|n)",
    "Examine (|C|t|r|l)",
    "Previous Merc",
    "Next Merc (|S|p|a|c|e)",
    "|Options",
    "|Burst Mode",
    "|Look/Turn",
    "Health: %d/%d\nEnergy: %d/%d\nMorale: %s",
    "Heh?",					//this means "what?" 
	"Cont",					//an abbrieviation for "Continued" 
	"Mute off for %s.",
    "Mute on for %s.",
    "Health: %d/%d\nFuel: %d/%d",
    "Exit Vehicle" ,
    "Change Squad ( |S|h|i|f|t |S|p|a|c|e )",
    "Drive",
    "N/A",						//this is an acronym for "Not Applicable." 
	"Use ( Hand To Hand )",
    "Use ( Firearm )",
    "Use ( Blade )",
    "Use ( Explosive )",
    "Use ( Medkit )",
    "(Catch)",
    "(Reload)",
    "(Give)",
    "%s has been set off.",
    "%s has arrived.",
    "%s ran out of Action Points.",
    "%s isn't available.",
    "%s is all bandaged.",
    "%s is out of bandages.",
    "Enemy in sector!",
    "No enemies in sight.",
    "Not enough Action Points.",
    "Nobody's using the remote.",
    "Burst fire emptied the clip!",
    "SOLDIER",
    "CREPITUS",
    "MILITIA",
    "CIVILIAN",
    "Exiting Sector",
    "OK",
    "Cancel",
    "Selected Merc",
    "All Mercs in Squad",
    "Go to Sector",
    "Go to Map",
    "You can't leave the sector from this side.",
    "%s is too far away.",
    "Removing Treetops",
    "Showing Treetops",
    "CROW",				//Crow, as in the large black bird
	"NECK",
    "HEAD",
    "TORSO",
    "LEGS",
    "Tell the Queen what she wants to know?",
    "Fingerprint ID aquired",
    "Invalid fingerprint ID. Weapon non-functional",
    "Target aquired",
    "Path Blocked",
    "Deposit/Withdraw Money",		//Help text over the $ button on the Single Merc Panel 
	"No one needs first aid.",
    "Jam.",											// Short form of JAMMED, for small inv slots
	"Can't get there.",					// used ( now ) for when we click on a cliff
	"Path is blocked. Do you want to switch places with this person?",
    "The person refuses to move.",
	// In the following message, '%s' would be replaced with a quantity of money (e.g. $200)
	"Do you agree to pay %s?",
    "Accept free medical treatment?",
    "Agree to marry Daryl?",
    "Key Ring Panel",
    "You cannot do that with an EPC.",
    "Spare Krott?",
    "Out of effective weapon range.",
    "Miner",
    "Vehicle can only travel between sectors",
    "Can't autobandage right now",
    "Path Blocked for %s",
    "Your mercs, who were captured by Deidranna's army are imprisoned here!",
    "Lock hit",
    "Lock destroyed",
    "Somebody else is trying to use this door.",
    "Health: %d/%d\nFuel: %d/%d",
  "%s cannot see %s.",  // Cannot see person trying to talk to
};

    public const int MAXLOCKDESCLENGTH = 40;

    public int KEY_USED = 0x01;
    public int LOCK_UNOPENABLE = 255;
    public int NO_KEY = 255;
    public int MAX_KEYS_PER_LOCK = 4;
    public int LOCK_REGULAR = 1;
    public int LOCK_PADLOCK = 2;
    public int LOCK_CARD = 3;
    public int LOCK_ELECTRONIC = 4;
    public int LOCK_SPECIAL = 5;

    public const int DONTSETDOORSTATUS = 2;

    public const int MAX_DIRTY_REGIONS = 128;

    public const int CURRENT_MOUSE_DATA = 0;
    public const int PREVIOUS_MOUSE_DATA = 1;
    public const int MAX_NUM_FRAMES = 25;

    // 8-bit palette stuff
    public static SGPPaletteEntry[] gSgpPalette = new SGPPaletteEntry[256];
    public static BufferState guiFrameBufferState;    // BUFFER_READY, BUFFER_DIRTY
    public static BufferState guiMouseBufferState;    // BUFFER_READY, BUFFER_DIRTY, BUFFER_DISABLED
    public static VideoManagerState guiVideoManagerState;   // VIDEO_ON, VIDEO_OFF, VIDEO_SUSPENDED, VIDEO_SHUTTING_DOWN
    public static ThreadState guiRefreshThreadState;  // THREAD_ON, THREAD_OFF, THREAD_SUSPENDED

    public static bool gfPrintFrameBuffer;
    public static int guiPrintFrameBufferIndex;

    public static Image<Rgba32> gpFrameBuffer;
    public static Image<Rgba32> gpPrimarySurface;

    public static int gusScreenWidth = 640;
    public static int gusScreenHeight = 480;
    public static int gubScreenPixelDepth;
    public static bool gfVideoCapture = false;
    public static int guiFramePeriod = 1000 / 15;
    public static long guiLastFrame;
    public static int[] gpFrameData = new int[MAX_NUM_FRAMES];
    public static int giNumFrames = 0;
    public static int gusMouseCursorWidth;
    public static int gusMouseCursorHeight;
    public static int gsMouseCursorXOffset;
    public static int gsMouseCursorYOffset;
    public static bool gfFatalError = false;
    public static string gFatalErrorString;

    public static SixLabors.ImageSharp.Rectangle gScrollRegion;
    public static Action? gpFrameBufferRefreshOverride;

    public static Texture gpMouseCursor { get; set; }
    public static Image<Rgba32> gpMouseCursorOriginal { get; set; }

    public static ScreenName guiIntroExitScreen { get; set; } = ScreenName.INTRO_SCREEN;
    public static SmackerFiles giCurrentIntroBeingPlayed = SmackerFiles.SMKINTRO_NO_VIDEO;

    public static SixLabors.ImageSharp.Rectangle[] gListOfDirtyRegions = new SixLabors.ImageSharp.Rectangle[MAX_DIRTY_REGIONS];
    public static int guiDirtyRegionCount;
    public static bool gfForceFullScreenRefresh;
    public static SixLabors.ImageSharp.Rectangle[] gDirtyRegionsEx = new SixLabors.ImageSharp.Rectangle[MAX_DIRTY_REGIONS];
    public static int[] gDirtyRegionsFlagsEx = new int[MAX_DIRTY_REGIONS];
    public static int guiDirtyRegionExCount;
    public static SixLabors.ImageSharp.Rectangle[] gBACKUPListOfDirtyRegions = new SixLabors.ImageSharp.Rectangle[MAX_DIRTY_REGIONS];
    public static int gBACKUPuiDirtyRegionCount;
    public static bool gBACKUPfForceFullScreenRefresh;

    public static IntroScreenType gbIntroScreenMode = IntroScreenType.Unknown;

    public const int LOCKED_NO_NEWGRIDNO = 2;

    //Current number of doors in world.
    public static int gubNumDoors = 0;

    //Dynamic array of Doors.  For general game purposes, the doors that are locked and/or trapped
    //are permanently saved within the map, and are loaded and allocated when the map is loaded.  Because
    //the editor allows more doors to be added, or removed, the actual size of the DoorTable may change.
    public static List<DOOR> DoorTable = new();

    //Current max number of doors.  This is only used by the editor.  When adding doors to the 
    //world, we may run out of space in the DoorTable, so we will allocate a new array with extra slots,
    //then copy everything over again.  gubMaxDoors holds the arrays actual number of slots, even though
    //the current number (gubNumDoors) will be <= to it.
    public static int gubMaxDoors = 0;


    public static bool gfIntroScreenEntry;
    public static bool gfIntroScreenExit;
    public static long guiSplashStartTime { get; set; } = 0;
    public static int guiSplashFrameFade { get; set; } = 10;

    public static SMKFLIC? gpSmackFlic = null;

    public const int REMOVAL_RATE_INCREMENT = 250;      // the smallest increment by which removal rate change during depletion (use round #s)

    public const int LOW_MINE_LOYALTY_THRESHOLD = 50;   // below this the head miner considers his town's population disloyal
                                                        // Mine production is being processed 4x daily: 9am ,noon, 3pm, and 6pm.
                                                        // This is loosely based on a 6am-6pm working day of 4 "shifts".
    public const int MINE_PRODUCTION_NUMBER_OF_PERIODS = 4;                     // how many times a day mine production is processed
    public const int MINE_PRODUCTION_START_TIME = (9 * 60);       // hour of first daily mine production event (in minutes)
    public const int MINE_PRODUCTION_PERIOD = (3 * 60);     // time seperating daily mine production events (in minutes)

    // this table holds mine values that change during the course of the game and must be saved
    public static Dictionary<MINE, MINE_STATUS_TYPE> gMineStatus = new();

    // the are not being randomized at all at this time
    public static Dictionary<MINE, MINE_TYPE> gubMineTypes = new()
    {
        { MINE.SAN_MONA,    MINE_TYPE.GOLD_MINE },			// SAN MONA
        { MINE.DRASSEN,     MINE_TYPE.SILVER_MINE },		// DRASSEN
        { MINE.ALMA,        MINE_TYPE.SILVER_MINE },		// ALMA
        { MINE.CAMBRIA,     MINE_TYPE.SILVER_MINE },		// CAMBRIA
        { MINE.CHITZENA,    MINE_TYPE.SILVER_MINE },		// CHITZENA
        { MINE.GRUMM,       MINE_TYPE.GOLD_MINE },            // GRUMM
    };

    // These values also determine the most likely ratios of mine sizes after random production increases are done
    public static Dictionary<MINE, int> guiMinimumMineProduction = new()
    {
        { MINE.SAN_MONA,       0 } ,		// SAN MONA
        { MINE.DRASSEN,     1000 } ,		// DRASSEN
        { MINE.ALMA,        1500 } ,		// ALMA
        { MINE.CAMBRIA,     1500 } ,		// CAMBRIA
        { MINE.CHITZENA,     500 } ,		// CHITZENA
        { MINE.GRUMM,       2000 } ,		// GRUMM
    };

    public static HEAD_MINER_TYPE[] gHeadMinerData = new HEAD_MINER_TYPE[(int)MINER.NUM_HEAD_MINERS]
    {
    	//	Profile #		running out		creatures!		all dead!		creatures again!		external face graphic
    	new(NPCID.FRED, new[] { 17, 18, 27,26 }, ExternalFaces.MINER_FRED_EXTERNAL_FACE),
        new(NPCID.MATT, new[] { -1, 18, 32, 31 }, ExternalFaces.MINER_MATT_EXTERNAL_FACE),
        new(NPCID.OSWALD, new[] { 14, 15, 24, 23 }, ExternalFaces.MINER_OSWALD_EXTERNAL_FACE),
        new(NPCID.CALVIN, new[] { 14, 15, 24, 23 }, ExternalFaces.MINER_CALVIN_EXTERNAL_FACE),
        new(NPCID.CARL, new[] { 14, 15, 24, 23 }, ExternalFaces.MINER_CARL_EXTERNAL_FACE),
    };


    public const int DEFAULT_EXTERN_PANEL_X_POS = 320;
    public const int DEFAULT_EXTERN_PANEL_Y_POS = 40;


    public const int DIALOGUE_TACTICAL_UI = 1;
    public const int DIALOGUE_CONTACTPAGE_UI = 2;
    public const int DIALOGUE_NPC_UI = 3;
    public const int DIALOGUE_SPECK_CONTACT_PAGE_UI = 4;
    public const int DIALOGUE_EXTERNAL_NPC_UI = 5;
    public const int DIALOGUE_SHOPKEEPER_UI = 6;

    public const double SALARY_CHANGE_PER_LEVEL = 1.25;        // Mercs salary is multiplied by this
    public const int MAX_DAILY_SALARY = 30000;// must fit into an INT16 (32k)
    public const int MAX_LARGE_SALARY = 500000; // no limit, really

    public const Stat FIRST_CHANGEABLE_STAT = Stat.HEALTHAMT;
    public const Stat LAST_CHANGEABLE_STAT = Stat.LDRAMT;
    public const Stat CHANGEABLE_STAT_COUNT = (Stat)(Stat.LDRAMT - Stat.HEALTHAMT + 1);
    public const int MAX_STAT_VALUE = 100;			// for stats and skills
    public const int MAXEXPLEVEL = 10;    // maximum merc experience level
    public const int SKILLS_SUBPOINTS_TO_IMPROVE = 25;
    public const int ATTRIBS_SUBPOINTS_TO_IMPROVE = 50;
    public const int LEVEL_SUBPOINTS_TO_IMPROVE = 350;    // per current level!	(Can't go over 6500, 10x must fit in USHORT!)
    public const int WORKIMPROVERATE = 2;      // increase to make working  mercs improve more
    public const int TRAINIMPROVERATE = 2;      // increase to make training mercs improve more

    public const int ARMY_GUN_LEVELS = 11;

    public static readonly STRATEGIC_STATUS gStrategicStatus = new();

    public const int MSG_INTERFACE = 0;
    public const int MSG_DIALOG = 1;
    public const int MSG_CHAT = 2;
    public const int MSG_DEBUG = 3;
    public const int MSG_UI_FEEDBACK = 4;
    public const int MSG_ERROR = 5;
    public const int MSG_BETAVERSION = 6;
    public const int MSG_TESTVERSION = 7;
    public const int MSG_MAP_UI_POSITION_MIDDLE = 8;
    public const int MSG_MAP_UI_POSITION_UPPER = 9;
    public const int MSG_MAP_UI_POSITION_LOWER = 10;
    public const int MSG_SKULL_UI_FEEDBACK = 11;

    // stat change causes
    public const int FROM_SUCCESS = 0;
    public const int FROM_TRAINING = 1;
    public const int FROM_FAILURE = 2;

    public int BUDDY_OPINION = +25;
    public int HATED_OPINION = -25;


    public static bool BUDDY_MERC(MERCPROFILESTRUCT prof, MERCPROFILESTRUCT bud)
        => prof.bBuddy[0] == bud
        || prof.bBuddy[1] == bud
        || prof.bBuddy[2] == bud;

    public static bool HATED_MERC(MERCPROFILESTRUCT prof, MERCPROFILESTRUCT hat)
        => prof.bHated[0] == hat
        || prof.bHated[1] == hat
        || prof.bHated[2] == hat;

    public const int TIME_BETWEEN_HATED_COMPLAINTS = 24;
    public const int SUSPICIOUS_DEATH = 1;
    public const int VERY_SUSPICIOUS_DEATH = 2;

    // training cap: you can't train any stat/skill beyond this value
    public const int TRAINING_RATING_CAP = 85;


    public const int HEALTH_INCREASE = 0x0001;
    public const int STRENGTH_INCREASE = 0x0002;
    public const int DEX_INCREASE = 0x0004;
    public const int AGIL_INCREASE = 0x0008;
    public const int WIS_INCREASE = 0x0010;
    public const int LDR_INCREASE = 0x0020;
    public const int MRK_INCREASE = 0x0040;
    public const int MED_INCREASE = 0x0080;
    public const int EXP_INCREASE = 0x0100;
    public const int MECH_INCREASE = 0x0200;
    public const int LVL_INCREASE = 0x0400;

    public const string HISTORY_DATA_FILE = "TEMP\\History.dat";

    public const int TOP_X = 0 + LAPTOP_SCREEN_UL_X;
    public const int TOP_Y = LAPTOP_SCREEN_UL_Y;
    public const int BLOCK_HIST_HEIGHT = 10;
    public const int BOX_HEIGHT = 14;
    public const int TOP_DIVLINE_Y = 101;
    public const int DIVLINE_X = 130;
    public const int MID_DIVLINE_Y = 155;
    public const int BOT_DIVLINE_Y = 204;
    public const int TITLE_X = 140;
    public const int TITLE_Y = 33;
    public const int TEXT_X = 140;
    public const int PAGE_SIZE = 22;
    public const int RECORD_Y = TOP_DIVLINE_Y;
    public const int RECORD_HISTORY_WIDTH = 200;
    public const int PAGE_NUMBER_X = TOP_X + 20;
    public const int PAGE_NUMBER_Y = TOP_Y + 33;
    public const int HISTORY_DATE_X = PAGE_NUMBER_X + 85;
    public const int HISTORY_DATE_Y = PAGE_NUMBER_Y;
    public const int RECORD_LOCATION_WIDTH = 142;//95
    public const FontStyle HISTORY_HEADER_FONT = FontStyle.FONT14ARIAL;
    public const FontStyle HISTORY_TEXT_FONT = FontStyle.FONT12ARIAL;
    public const int RECORD_DATE_X = TOP_X + 10;
    public const int RECORD_DATE_WIDTH = 31;//68
    public const int RECORD_HEADER_Y = 90;


    public const int NUM_RECORDS_PER_PAGE = PAGE_SIZE;
    //static int SIZE_OF_HISTORY_FILE_RECORD( sizeof(UINT8 ) + sizeof(UINT8 ) + sizeof(UINT32 ) + sizeof(UINT16 ) + sizeof(UINT16 ) + sizeof(UINT8 ) + sizeof(UINT8 ) )

    // button positions
    public const int NEXT_BTN_X = 577;
    public const int PREV_BTN_X = 553;
    public const int BTN_Y = 53;

    public static int guiTITLE;
    public static int guiTOP;
    public static int guiLONGLINE;
    public static int guiSHADELINE;

    public const int LAPTOP_SIDE_PANEL_X = 0;
    public const int LAPTOP_SIDE_PANEL_Y = 0;
    public const int LAPTOP_SIDE_PANEL_WIDTH = 640;
    public const int LAPTOP_SIDE_PANEL_HEIGHT = 480;
    public const int LAPTOP_X = 0;
    public const int LAPTOP_Y = 0;
    public const int LAPTOP_SCREEN_UL_X = 111;
    public const int LAPTOP_SCREEN_UL_Y = 27;
    public const int LAPTOP_SCREEN_LR_X = 613;
    public const int LAPTOP_SCREEN_LR_Y = 427;
    public const int LAPTOP_UL_X = 24;
    public const int LAPTOP_UL_Y = 27;
    public const int LAPTOP_SCREEN_WIDTH = LAPTOP_SCREEN_LR_X - LAPTOP_SCREEN_UL_X;
    public const int LAPTOP_SCREEN_HEIGHT = LAPTOP_SCREEN_LR_Y - LAPTOP_SCREEN_UL_Y;

    // new positions for web browser

    public const int LAPTOP_SCREEN_WEB_UL_Y = LAPTOP_SCREEN_UL_Y + 19;
    public const int LAPTOP_SCREEN_WEB_LR_Y = LAPTOP_SCREEN_WEB_UL_Y + LAPTOP_SCREEN_HEIGHT;
    public const int LAPTOP_SCREEN_WEB_DELTA_Y = LAPTOP_SCREEN_WEB_UL_Y - LAPTOP_SCREEN_UL_Y;

    // the laptop on/off button 
    public const int ON_X = 113;
    public const int ON_Y = 445;

    public const int PREV_PAGE_BUTTON = 0;
    public const int NEXT_PAGE_BUTTON = 1;
}

public enum Stat
{
    SALARYAMT = 0,
    HEALTHAMT = 1,
    AGILAMT = 2,
    DEXTAMT = 3,
    WISDOMAMT = 4,
    MEDICALAMT = 5,
    EXPLODEAMT = 6,
    MECHANAMT = 7,
    MARKAMT = 8,
    EXPERAMT = 9,
    STRAMT = 10,
    LDRAMT = 11, // leadership
    ASSIGNAMT = 12,
    NAMEAMT = 13,

    FIRST_CHANGEABLE_STAT = HEALTHAMT,
    LAST_CHANGEABLE_STAT = LDRAMT,
    CHANGEABLE_STAT_COUNT = (LDRAMT - HEALTHAMT + 1),
}
