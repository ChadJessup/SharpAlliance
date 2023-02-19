﻿using System;
using System.Collections.Generic;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems;
using SixLabors.ImageSharp;

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

    public const int WORLD_BASE_HEIGHT = 0;
    public const int WORLD_CLIFF_HEIGHT = 80;

    public const int NO_ROOM = 0;
    public const int MAX_ROOMS = 250;

    // Room Information
    public static int[] gubWorldRoomInfo = new int[WORLD_MAX];
    public static int[] gubWorldRoomHidden = new int[MAX_ROOMS];

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

    // VIDEO OVERLAYS 
    public int giFPSOverlay;
    public int giCounterPeriodOverlay;
    public static bool gfProgramIsRunning { get; set; } // Turn this to FALSE to exit program

    // World Data
    public static List<MAP_ELEMENT> gpWorldLevelData { get; set; } = new();

    // World Movement Costs
    public static int[,,] gubWorldMovementCosts = new int[World.WORLD_MAX, World.MAXDIR, 2];
    public static int[,] gszTerrain = new int[(int)Traversability.NUM_TRAVTERRAIN_TYPES, 15];

    public static Dictionary<NPCID, MERCPROFILE> gMercProfiles { get; } = new();

    public static TacticalStatusType gTacticalStatus { get; set; } = new TacticalStatusType();
    public static TILE_ELEMENT[] gTileDatabase = new TILE_ELEMENT[(int)TileDefines.NUMBEROFTILES];

    public static SOLDIERTYPE[] Menptr = new SOLDIERTYPE[TOTAL_SOLDIERS];
    public static SOLDIERTYPE[] MercPtrs = new SOLDIERTYPE[TOTAL_SOLDIERS];

    // MERC SLOTS - A LIST OF ALL ACTIVE MERCS
    public static SOLDIERTYPE[] MercSlots = new SOLDIERTYPE[TOTAL_SOLDIERS];
    public static int guiNumMercSlots { get; set; }

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

    public static Rectangle gRubberBandRect = new(0, 0, 0, 0);
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

    public static Rectangle gSelectRegion;
    public static Point gSelectAnchor;

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

    public const int DIRTYLEVEL0 = 0;
    public const int DIRTYLEVEL1 = 1;
    public const int DIRTYLEVEL2 = 2;

    public static UICursorDefines guiCurUICursor = UICursorDefines.NO_UICURSOR;
    public static UICursorDefines guiOldUICursor = UICursorDefines.NO_UICURSOR;
    public static int gusCurMousePos;
    public static int gusTargetDropPos;
    public static bool gfTargetDropPos = false;


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
}
