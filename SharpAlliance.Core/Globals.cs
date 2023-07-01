global using TIMECOUNTER = System.UInt32;
global using PaletteRepID = System.String;

global using SharpAlliance.Core.SubSystems;
global using SharpAlliance.Platform.Interfaces;

global using static SharpAlliance.Core.Globals;
global using static SharpAlliance.Core.EnglishText;

using static SharpAlliance.Core.InteractiveTiles;
using static SharpAlliance.Core.Screens.CreditsScreen;
using static SharpAlliance.Core.SubSystems.StrategicAI;

using System;
using System.Collections.Generic;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.Screens;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;

namespace SharpAlliance.Core;

public partial class Globals
{

    static Globals()
    {
        foreach (var fact in Enum.GetValues<FACT>())
        {
            gubFact[fact] = false;
        }
    }

    public const int NAME_LENGTH = 30;
    public const int NICKNAME_LENGTH = 10;

    public const int MAX_AP_CARRIED = 5;      // APs carried from turn-to-turn
                                              // monster AP bonuses; expressed in 10ths (12 = 120% normal) 

    public const string FILES_DAT_FILE = "TEMP\\files.dat";

    public const TileIndexes NUMBEROFTILES = TileIndexes.NUMBEROFTILES;
    public static bool gfTacticalTraversal = false;
    public static GROUP? gpTacticalTraversalGroup = null;
    public static SOLDIERTYPE? gpTacticalTraversalChosenSoldier = null;

    public const CURSOR MSYS_NO_CURSOR = CURSOR.MSYS_NO_CURSOR;


    public static bool gfAutomaticallyStartAutoResolve = false;
    public static bool gfAutoAmbush = false;
    public static bool gfHighPotentialForAmbush = false;
    public static bool gfGotoSectorTransition = false;
    public static bool gfEnterAutoResolveMode = false;
    public static bool gfEnteringMapScreenToEnterPreBattleInterface = false;
    public static bool gfIgnoreAllInput = false;

    public const int MAX_MOVEMENT_NOISE = 9;
    public const FontColor COLOR1 = (FontColor)((int)FontColor.FONT_MCOLOR_BLACK << 8) | FontColor.FONT_MCOLOR_LTGREEN;
    public const FontColor COLOR2 = (FontColor)((int)FontColor.FONT_MCOLOR_BLACK << 8) | FontColor.FONT_MCOLOR_LTGRAY2;
    public const int LINE_HEIGHT = 15;

    public const int FIRST_LEVEL = 0;
    public const int SECOND_LEVEL = 1;

    public static int[] gsBoxerGridNo = { 11393, 11233, 11073 };

    public const int NUM_CORPSE_SHADES = 17;
    public const int ROTTING_CORPSE_FIND_SWEETSPOT_FROM_GRIDNO = 0x01;		//Find the closest spot to the given gridno
    public const int ROTTING_CORPSE_USE_NORTH_ENTRY_POINT = 0x02;		//Find the spot closest to the north entry grid
    public const int ROTTING_CORPSE_USE_SOUTH_ENTRY_POINT = 0x04;		//Find the spot closest to the south entry grid
    public const int ROTTING_CORPSE_USE_EAST_ENTRY_POINT = 0x08;		//Find the spot closest to the east entry grid
    public const int ROTTING_CORPSE_USE_WEST_ENTRY_POINT = 0x10;		//Find the spot closest to the west entry grid
    public const int ROTTING_CORPSE_USE_CAMMO_PALETTE = 0x20;		//We use cammo palette here....
    public const int ROTTING_CORPSE_VEHICLE = 0x40;		//Vehicle Corpse


    public const int BOXING_SECTOR_X = 5;
    public const int BOXING_SECTOR_Y = 4;
    public const int BOXING_SECTOR_Z = 0;
    public const int ROOM_SURROUNDING_BOXING_RING = 3;
    public const int BOXING_RING = 29;
    public const int BOXING_AI_START_POSITION = 11235;
    public const int NUM_BOXERS = 3;


    public const double DEATH_RATE_SEVERITY = 1.0f;			// increase to make death rates higher for same # of deaths/time

    // progress threshold that control Enrico E-mail timing
    public const int SOME_PROGRESS_THRESHOLD = 20;
    public const int ABOUT_HALFWAY_THRESHOLD = 55;
    public const int NEARLY_DONE_THRESHOLD = 80;
    public const int MINOR_SETBACK_THRESHOLD = 5;
    public const int MAJOR_SETBACK_THRESHOLD = 15;

    // Maximum value that can be returned by the rand function:
    public const int RAND_MAX = 0x7fff;

    public static int PreRandom(int uiRange) => RNG.PreRandom(uiRange);

    public const int MAX_ROTTING_CORPSES = 100;

    public static int giHabitatedDistance = 0;
    public static int giPopulationModifier = 0;
    public static int giLairID = 0;
    public static int giDestroyedLairID = 0;

    //various information required for keeping track of the battle sector involved for
    //prebattle interface, autoresolve, etc.
    public static int gsCreatureInsertionCode = 0;
    public static int gsCreatureInsertionGridNo = 0;
    public static int gubNumCreaturesAttackingTown = 0;
    public static int gubYoungMalesAttackingTown = 0;
    public static int gubYoungFemalesAttackingTown = 0;
    public static int gubAdultMalesAttackingTown = 0;
    public static int gubAdultFemalesAttackingTown = 0;
    public static CREATURE_BATTLE gubCreatureBattleCode = CREATURE_BATTLE.CODE_NONE;
    public static SEC gubSectorIDOfCreatureAttack = 0;

    public static ROTTING_CORPSE[] gRottingCorpse = new ROTTING_CORPSE[MAX_ROTTING_CORPSES];
    public static int giNumRottingCorpse = 0;


    //IMPORTANT:
    // Changing this define will invalidate the JA2 save.  If this is necessary, please ifdef your own value.
    public const int MAX_PREGENERATED_NUMS = 256;

    public static int guiPreRandomIndex = 0;
    public static int[] guiPreRandomNums = new int[MAX_PREGENERATED_NUMS];

    public static ushort gusAlphaMask;// = 0;
    public static ushort gusRedMask;// = 0;
    public static ushort gusGreenMask;// = 0;
    public static ushort gusBlueMask;// = 0;
    public static short gusRedShift;// = 0;
    public static short gusBlueShift;// = 0;
    public static short gusGreenShift;// = 0;

    public const int DEFAULT_MORALE = 50;

    public const int MORALE_MOD_MAX = 50;       // morale *mod* range is -50 to 50, if you change this, check the decay formulas!
    public const int DRUG_EFFECT_MORALE_MOD = 150;
    public const int ALCOHOL_EFFECT_MORALE_MOD = 160;
    public const int HOURS_BETWEEN_STRATEGIC_DECAY = 3;
    public const int PHOBIC_LIMIT = -20;


    public const int SHOW_MIN_FPS = 0;
    public const int SHOW_FULL_FPS = 1;
    // ENUMERATION OF SOLDIER POSIITONS IN GLOBAL SOLDIER LIST
    public const int MAX_NUM_SOLDIERS = 148;
    public const int NUM_PLANNING_MERCS = 8;
    public const int TOTAL_SOLDIERS = (NUM_PLANNING_MERCS + MAX_NUM_SOLDIERS);

    public const int MAXCOL = WORLD_COLS;
    public const int MAXROW = WORLD_ROWS;
    public const int GRIDSIZE = MAXCOL * MAXROW;
    public const int RIGHTMOSTGRID = MAXCOL - 1;
    public const int LASTROWSTART = GRIDSIZE - MAXCOL;
    public const int NOWHERE = GRIDSIZE + 1;
    public const int NO_MAP_POS = NOWHERE;
    public const int MAPWIDTH = WORLD_COLS;
    public const int MAPHEIGHT = WORLD_ROWS;
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

    //
    // Specific Dealer Flags
    // NOTE: Each dealer has 8 flags, but different dealers can and SHOULD share the same flag #s!
    //

    // Alex Fredo
    public const int ARMS_DEALER_FLAG__FREDO_HAS_SAID_ROCKET_RIFLE_QUOTE = 0x00000001;	// Alex Fredo has already repaired the Rocket Rifle
    // Franz Hinkle
    public const int ARMS_DEALER_FLAG__FRANZ_HAS_SOLD_VIDEO_CAMERA_TO_PLAYER = 0x00000001;	// Franz Hinkle has sold the video camera to the player

    public static int gsRecompileAreaTop { get; set; } = 0;
    public static int gsRecompileAreaLeft { get; set; } = 0;
    public static int gsRecompileAreaRight { get; set; } = 0;
    public static int gsRecompileAreaBottom { get; set; } = 0;

    public const int MAX_OBJECTS_PER_SLOT = 8;
    public const int MAX_ATTACHMENTS = 4;
    public const int MAX_MONEY_PER_SLOT = 20000;

    public static bool gfHiddenInterrupt;
    public static bool gfHiddenTurnbased;

    public static bool INTERRUPT_QUEUED => (gubOutOfTurnPersons > 0);


    public static bool RPC_RECRUITED(SOLDIERTYPE p) => ((p.ubProfile == NO_PROFILE) ? false : (gMercProfiles[p.ubProfile].ubMiscFlags.HasFlag(PROFILE_MISC_FLAG.RECRUITED)));
    public static bool AM_AN_EPC(SOLDIERTYPE p) => ((p.ubProfile == NO_PROFILE) ? false : (gMercProfiles[p.ubProfile].ubMiscFlags.HasFlag(PROFILE_MISC_FLAG.EPCACTIVE)));
    public static bool AM_A_ROBOT(SOLDIERTYPE p) => ((p.ubProfile == NO_PROFILE) ? false : (gMercProfiles[p.ubProfile].ubBodyType == SoldierBodyTypes.ROBOTNOWEAPON));

    public static email? pEmailList;
    public static PagePtr? pPageList;
    public static int iLastPage = -1;
    public static int iCurrentPage = 0;

    public static bool OK_ENEMY_MERC(SOLDIERTYPE p) => (p.bNeutral == 0 && (p.bSide != gbPlayerNum) && p.bLife >= OKLIFE);
    // Checks if our guy can be controllable .... checks bInSector, team, on duty, etc...
    public static bool OK_CONTROLLABLE_MERC(SOLDIERTYPE p) => (p.bLife >= OKLIFE && p.bActive && p.bInSector && p.bTeam == gbPlayerNum && p.bAssignment < Assignments.ON_DUTY);
    // Checks if our guy can be controllable .... checks bInSector, team, on duty, etc...
    public static bool OK_INSECTOR_MERC(SOLDIERTYPE p) => (p.bLife >= OKLIFE && p.bActive && p.bInSector && p.bTeam == gbPlayerNum && p.bAssignment < Assignments.ON_DUTY);
    // Checkf if our guy can be selected and is not in a position where our team has an interupt and he does not have one...
    public static bool OK_INTERRUPT_MERC(SOLDIERTYPE p) => ((INTERRUPT_QUEUED) ? ((p.bMoved > 0) ? false : true) : true);
    public static bool CREATURE_OR_BLOODCAT(SOLDIERTYPE p) => ((p.uiStatusFlags.HasFlag(SOLDIER.MONSTER)) || p.ubBodyType == SoldierBodyTypes.BLOODCAT);
    public static bool TANK(SOLDIERTYPE p) => (p.ubBodyType == SoldierBodyTypes.TANK_NE || p.ubBodyType == SoldierBodyTypes.TANK_NW);
    public static bool OK_ENTERABLE_VEHICLE(SOLDIERTYPE p) => ((p.uiStatusFlags.HasFlag(SOLDIER.VEHICLE)) && !TANK(p) && p.bLife >= OKLIFE);


    public static bool EXPLOSIVE_GUN(Items x) => (x == Items.ROCKET_LAUNCHER || x == Items.TANK_CANNON);

    public static UNDERGROUND_SECTORINFO? gpUndergroundSectorInfoHead = null;
    public static UNDERGROUND_SECTORINFO? gpUndergroundSectorInfoTail = null;
    public static bool gfPendingEnemies = false;

    //Used by ubGarrisonID when a sector doesn't point to a garrison.  Used by strategic AI only.
    public const Garrisons NO_GARRISON = (Garrisons)255;


    // Room Information
    public static int[] gubWorldRoomInfo = new int[WORLD_MAX];
    public static int[] gubWorldRoomHidden = new int[MAX_ROOMS];

    public static StrategicMapElement[] strategicMap = new StrategicMapElement[Globals.MAP_WORLD_X * Globals.MAP_WORLD_Y];
    public static Dictionary<Garrisons, GARRISON_GROUP> gGarrisonGroup = new();

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
    public static int[,,] gubWorldMovementCosts = new int[WORLD_MAX, MAXDIR, 2];

    public static Dictionary<NPCID, MERCPROFILESTRUCT> gMercProfiles { get; } = new();

    public static Dictionary<TileIndexes, TILE_ELEMENT> gTileDatabase = new();


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

    public static Dictionary<SoldierBodyTypes, ANI_SPEED_DEF> gubAnimWalkSpeeds = new();// ANI_SPEED_DEF[(int)SoldierBodyTypes.TOTALBODYTYPES];
    public static Dictionary<SoldierBodyTypes, ANI_SPEED_DEF> gubAnimRunSpeeds = new();
    public static Dictionary<SoldierBodyTypes, ANI_SPEED_DEF> gubAnimSwatSpeeds = new();// ANI_SPEED_DEF[(int)SoldierBodyTypes.TOTALBODYTYPES];
    public static Dictionary<SoldierBodyTypes, ANI_SPEED_DEF> gubAnimCrawlSpeeds = new();// ANI_SPEED_DEF[(int)SoldierBodyTypes.TOTALBODYTYPES];
    public static int[] gubMaxActionPoints = new int[(int)SoldierBodyTypes.TOTALBODYTYPES];

    public static Dictionary<SEC, SECTORINFO> SectorInfo = new();

    public static bool gfSomeoneSaidMoraleQuote = false;

    // UTILITY FUNCTIONS CALLED BY OVERHEAD.H
    public static WorldDirections[] gDirectionFrom8to2 = { 0, 0, (WorldDirections)1, (WorldDirections)1, 0, (WorldDirections)1, (WorldDirections)1, 0 };

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

    public const int OKBREATH = 10;
    public const int OKLIFE = 15;
    public const int CONSCIOUSNESS = 10;

    public static int[] guiPlottedPath = new int[256];
    public static int[] guiPathingData = new int[256];
    public static int giPathDataSize;
    public static int giPlotCnt;
    public static int guiEndPlotGridNo;

    public const int NO_GUY_SELECTION = 0;
    public const int SELECTED_GUY_SELECTION = 1;
    public const int NONSELECTED_GUY_SELECTION = 2;
    public const int ENEMY_GUY_SELECTION = 3;

    public static bool fMapScreenBottomDirty;

    public const int QUESTNOTSTARTED = 0;
    public const int QUESTINPROGRESS = 1;
    public const int QUESTDONE = 2;

    //Kris:  November 10, 1997
    //Please don't change this value from 10.  It will invalidate all of the maps and soldiers.
    public const int MAXPATROLGRIDS = 10;  // *** THIS IS A DUPLICATION - MUST BE MOVED !
    public const int MAX_FULLTILE_DIRECTIONS = 3;


    public static int[] gzLocation = new int[20];
    public static bool gfLocation = false;

    public static bool gfUIBodyHitLocation = false;

    public static int[] gzIntTileLocation = new int[20];
    public static bool gfUIIntTileLocation;

    public static int[] gzIntTileLocation2 = new int[20];
    public static bool gfUIIntTileLocation2;


    public static MOUSE_REGION? gDisableRegion;
    public static bool gfDisableRegionActive = false;

    public static MOUSE_REGION? gUserTurnRegion;
    public static bool gfUserTurnRegionActive = false;

    public static int[] gbFirstApproachFlags = { 0x01, 0x02, 0x04, 0x08 };
    public static Dictionary<Meanwhiles, int> gubAlternateNPCFileNumsForQueenMeanwhiles = new()
    {
        { Meanwhiles.END_OF_PLAYERS_FIRST_BATTLE,   160 },
        { Meanwhiles.DRASSEN_LIBERATED,             161 },
        { Meanwhiles.CAMBRIA_LIBERATED,             162 },
        { Meanwhiles.ALMA_LIBERATED,                163 },
        { Meanwhiles.GRUMM_LIBERATED,               164 },
        { Meanwhiles.CHITZENA_LIBERATED,            165 },
        { Meanwhiles.NW_SAM,                        166 },
        { Meanwhiles.NE_SAM,                        167 },
        { Meanwhiles.CENTRAL_SAM,                   168 },
        { Meanwhiles.FLOWERS,                       169 },
        { Meanwhiles.LOST_TOWN,                     170 },
        { Meanwhiles.INTERROGATION,                 171 },
        { Meanwhiles.CREATURES,                     172 },
        { Meanwhiles.KILL_CHOPPER,                  173 },
        { Meanwhiles.AWOL_SCIENTIST,                174 },
        { Meanwhiles.OUTSKIRTS_MEDUNA,              175 },
        { Meanwhiles.BALIME_LIBERATED,              176 },
    };

    public static Dictionary<Meanwhiles, int> gubAlternateNPCFileNumsForElliotMeanwhiles = new()
    {
        { Meanwhiles.END_OF_PLAYERS_FIRST_BATTLE,   180 },
        { Meanwhiles.DRASSEN_LIBERATED,             181 },
        { Meanwhiles.CAMBRIA_LIBERATED,             182 },
        { Meanwhiles.ALMA_LIBERATED,                183 },
        { Meanwhiles.GRUMM_LIBERATED,               184 },
        { Meanwhiles.CHITZENA_LIBERATED,            185 },
        { Meanwhiles.NW_SAM,                        186 },
        { Meanwhiles.NE_SAM,                        187 },
        { Meanwhiles.CENTRAL_SAM,                   188 },
        { Meanwhiles.FLOWERS,                       189 },
        { Meanwhiles.LOST_TOWN,                     190 },
        { Meanwhiles.INTERROGATION,                 191 },
        { Meanwhiles.CREATURES,                     192 },
        { Meanwhiles.KILL_CHOPPER,                  193 },
        { Meanwhiles.AWOL_SCIENTIST,                194 },
        { Meanwhiles.OUTSKIRTS_MEDUNA,              195 },
        { Meanwhiles.BALIME_LIBERATED,              196 },
    };


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
    public static NPCID gubNiceNPCProfile = NO_PROFILE;
    public static NPCID gubNastyNPCProfile = NO_PROFILE;

    public static GameOptions gGameOptions = new();
    public static GameSettings gGameSettings = new();

    public const MouseCallback? MSYS_NO_CALLBACK = null;

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
    public static int gfUIMouseOnValidCatcher = 0;
    public static int gubUIValidCatcherID = 0;
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

    public static int guiLastPageInHistoryRecordsList = 0;    // last page in list
                                                              // the page flipping buttons
    public static GUI_BUTTON[] giHistoryButton = new GUI_BUTTON[2];
    public static ButtonPic[] giHistoryButtonImage = new ButtonPic[2];

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

    // this has to be updated when we figure out how many facts we have
    public static Dictionary<FACT, bool> gubFact { get; } = new();
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

    // MODIFIERS FOR AP COST FOR MOVEMENT 
    public const double RUNDIVISOR = 1.8;
    public const int WALKCOST = -1;
    public const int SWATCOST = 0;
    public const int CRAWLCOST = 1;

    public const TEAM MAXTEAMS = TEAM.MAX_TEAMS;
    public const int MAXMERCS = MAX_NUM_SOLDIERS;

    //Global dynamic array of all of the items in a loaded map.
    public static List<WORLDITEM> gWorldItems = new();
    public static List<int> guiNumWorldItems = new();

    public static List<WORLDBOMB> gWorldBombs = new();
    public static int guiNumWorldBombs = 0;

    public const int SOUND_MAX_CACHED = 128;                       // number of cache slots
    public const int SOUND_MAX_CHANNELS = 16;					// number of mixer channels

    public const int NUMBER_OF_SAMS = 4;
    public const int POSITION_SOUND_FROM_SOLDIER = 0x00000001;
    public const int NUM_POSITION_SOUND_EFFECT_SLOTS = 10;

    // GLOBAL FOR SMOKE LISTING
    public static POSITIONSND[] gPositionSndData = new POSITIONSND[NUM_POSITION_SOUND_EFFECT_SLOTS];
    public static int guiNumPositionSnds = 0;
    public static bool gfPositionSoundsActive = false;

    public static MAPCREATE_STRUCT gMapInformation;

    public const Items NOTHING = Items.NONE;
    public const Items ITEM_NOT_FOUND = Items.UNSET;

    public const InventorySlot ITEM_SLOT_NOT_FOUND = (InventorySlot)Items.UNSET;

    public const int USABLE = 10;      // minimum work% of items to still be usable

    // border and bottom buttons
    public static GUI_BUTTON[] giMapBorderButtons = new GUI_BUTTON[6];
    public static int[] guiMapButtonInventory = { -1, -1, -1, -1, -1, -1 };

    public static DISABLED_STYLE gbDisabledButtonStyle;
    public static GUI_BUTTON gpCurrentFastHelpButton;

    public const int MAX_GENERIC_PICS = 40;
    public const int MAX_BUTTON_ICONS = 40;
    public const int MAX_BUTTON_PICS = 256;
    public const int MAX_BUTTONS = 400;

    public const Surfaces BUTTON_USE_DEFAULT = Surfaces.Unknown;
    public static readonly int? BUTTON_NO_FILENAME = null;
    public static readonly GUI_CALLBACK BUTTON_NO_CALLBACK = (ref GUI_BUTTON o, MSYS_CALLBACK_REASON r) => { };
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

    public static int fInterfacePanelDirty;
    public static bool gfRenderHilights = true;

    public static int gusUIOldSelectedSoldier;
    public static int giUIMessageOverlay;
    public static uint guiUIMessageTime;

    public static GUI_BUTTON? gpAnchoredButton;
    public static GUI_BUTTON? gpPrevAnchoredButton;
    public static bool gfAnchoredState;

    public static InventorySlot NUM_INV_SLOTS = InventorySlot.NUM_INV_SLOTS;

    public static List<GROUP> gpGroupList = new();
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
    public const MAP_ROW TIXA_SECTOR_Y = (MAP_ROW)10;
    public const AnimationSurfaceTypes INVALID_ANIMATION_SURFACE = (AnimationSurfaceTypes)32000;

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
        { MINE.DRASSEN, new(13, (MAP_ROW)4, TOWNS.DRASSEN) },
        { MINE.ALMA, new(14, (MAP_ROW)9, TOWNS.ALMA) },
        { MINE.CAMBRIA, new(8,  (MAP_ROW)8, TOWNS.CAMBRIA) },
        { MINE.CHITZENA, new(2,  (MAP_ROW)2, TOWNS.CHITZENA) },
        { MINE.GRUMM, new(3, (MAP_ROW)8, TOWNS.GRUMM) },
    };

    public static Dictionary<UICursorDefines, UICursor> gUICursors = new()
    {
        { UICursorDefines.NO_UICURSOR, new (UICursorDefines.NO_UICURSOR, 0, 0, 0) },
        { UICursorDefines.NORMAL_FREEUICURSOR, new (UICursorDefines.NORMAL_FREEUICURSOR, UICURSOR.FREEFLOWING, CURSOR.NORMAL, 0) },
        { UICursorDefines.NORMAL_SNAPUICURSOR, new (UICursorDefines.NORMAL_SNAPUICURSOR, UICURSOR.SNAPPING, 0, 0) },
        { UICursorDefines.MOVE_RUN_UICURSOR, new (UICursorDefines.MOVE_RUN_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILEAPDEPENDENT | UICURSOR.DONTSHOW2NDLEVEL, CURSOR.RUN1, TileIndexes.FIRSTPOINTERS2) },
        { UICursorDefines.MOVE_WALK_UICURSOR, new(UICursorDefines.MOVE_WALK_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILEAPDEPENDENT | UICURSOR.DONTSHOW2NDLEVEL | UICURSOR.CENTERAPS, CURSOR.WALK1, TileIndexes.FIRSTPOINTERS2) } ,
        { UICursorDefines.MOVE_SWAT_UICURSOR, new (UICursorDefines.MOVE_SWAT_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILEAPDEPENDENT | UICURSOR.DONTSHOW2NDLEVEL, CURSOR.SWAT1, TileIndexes.FIRSTPOINTERS2) },
        { UICursorDefines.MOVE_PRONE_UICURSOR, new (UICursorDefines.MOVE_PRONE_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILEAPDEPENDENT | UICURSOR.DONTSHOW2NDLEVEL, CURSOR.PRONE1, TileIndexes.FIRSTPOINTERS2) },
        { UICursorDefines.MOVE_VEHICLE_UICURSOR, new (UICursorDefines.MOVE_VEHICLE_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILEAPDEPENDENT | UICURSOR.DONTSHOW2NDLEVEL, CURSOR.DRIVEV, TileIndexes.FIRSTPOINTERS2) },
        { UICursorDefines.CONFIRM_MOVE_RUN_UICURSOR, new (UICursorDefines.CONFIRM_MOVE_RUN_UICURSOR, UICURSOR.SNAPPING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, 0, TileIndexes.FIRSTPOINTERS4) },
        { UICursorDefines.CONFIRM_MOVE_WALK_UICURSOR, new (UICursorDefines.CONFIRM_MOVE_WALK_UICURSOR, UICURSOR.SNAPPING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, 0, TileIndexes.FIRSTPOINTERS4) },
        { UICursorDefines.CONFIRM_MOVE_SWAT_UICURSOR, new (UICursorDefines.CONFIRM_MOVE_SWAT_UICURSOR, UICURSOR.SNAPPING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, 0, TileIndexes.FIRSTPOINTERS4) },
        { UICursorDefines.CONFIRM_MOVE_PRONE_UICURSOR, new (UICursorDefines.CONFIRM_MOVE_PRONE_UICURSOR, UICURSOR.SNAPPING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, 0, TileIndexes.FIRSTPOINTERS4) },
        { UICursorDefines.CONFIRM_MOVE_VEHICLE_UICURSOR, new (UICursorDefines.CONFIRM_MOVE_VEHICLE_UICURSOR, UICURSOR.SNAPPING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, 0, TileIndexes.FIRSTPOINTERS4) },
        { UICursorDefines.ALL_MOVE_RUN_UICURSOR, new (UICursorDefines.ALL_MOVE_RUN_UICURSOR, UICURSOR.SNAPPING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, 0, TileIndexes.FIRSTPOINTERS5) },
        { UICursorDefines.ALL_MOVE_WALK_UICURSOR, new (UICursorDefines.ALL_MOVE_WALK_UICURSOR, UICURSOR.SNAPPING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, 0, TileIndexes.FIRSTPOINTERS5) },
        { UICursorDefines.ALL_MOVE_SWAT_UICURSOR, new (UICursorDefines.ALL_MOVE_SWAT_UICURSOR, UICURSOR.SNAPPING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, 0, TileIndexes.FIRSTPOINTERS5) },
        { UICursorDefines.ALL_MOVE_PRONE_UICURSOR, new (UICursorDefines.ALL_MOVE_PRONE_UICURSOR, UICURSOR.SNAPPING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, 0, TileIndexes.FIRSTPOINTERS5) },
        { UICursorDefines.ALL_MOVE_VEHICLE_UICURSOR, new (UICursorDefines.ALL_MOVE_VEHICLE_UICURSOR, UICURSOR.SNAPPING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, 0, TileIndexes.FIRSTPOINTERS5) },
        { UICursorDefines.MOVE_REALTIME_UICURSOR, new (UICursorDefines.MOVE_REALTIME_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILEAPDEPENDENT | UICURSOR.DONTSHOW2NDLEVEL, CURSOR.VIDEO_NO_CURSOR, TileIndexes.FIRSTPOINTERS2) },
        { UICursorDefines.MOVE_RUN_REALTIME_UICURSOR, new (UICursorDefines.MOVE_RUN_REALTIME_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, CURSOR.VIDEO_NO_CURSOR, TileIndexes.FIRSTPOINTERS7) },
        { UICursorDefines.CONFIRM_MOVE_REALTIME_UICURSOR, new (UICursorDefines.CONFIRM_MOVE_REALTIME_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, CURSOR.VIDEO_NO_CURSOR, TileIndexes.FIRSTPOINTERS4) },
        { UICursorDefines.ALL_MOVE_REALTIME_UICURSOR, new (UICursorDefines.ALL_MOVE_REALTIME_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, CURSOR.VIDEO_NO_CURSOR, TileIndexes.FIRSTPOINTERS5) },
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
        { UICursorDefines.ACTION_TARGETAIM9_UICURSOR, new (UICursorDefines.ACTION_TARGETAIM9_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.TARGETON9, 0) },
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
        { UICursorDefines.ACTION_THROWAIM9_UICURSOR, new (UICursorDefines.ACTION_THROWAIM9_UICURSOR, UICURSOR.FREEFLOWING, CURSOR.THROWKON9, 0) },
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
    public static InterfacePanelDefines gsCurInterfacePanel { get; internal set; }
    public static MOUSE_REGION? gViewportRegion { get; set; }
    public static bool gfUIStanceDifferent { get; set; }

    public const int ROOF_LEVEL_HEIGHT = 50;

    // the big map .pcx
    public static uint guiBIGMAP = 0;

    public static uint guiBOTTOMPANEL { get; set; }
    public static uint guiRIGHTPANEL { get; set; }
//    public static Surfaces guiRENDERBUFFER { get; set; }
    //public static Surfaces Surfaces.SAVE_BUFFER { get; set; }
    public static Surfaces guiEXTRABUFFER { get; set; }
    public static bool gfExtraBuffer { get; set; }

    public static OBJECTTYPE? gpItemPointer { get; set; } = null;

    // graphics
    public static int guiMapBorder;
    //int guiMapBorderCorner;


    // scroll direction
    public static int giScrollButtonState = -1;

    public static int gbPixelDepth { get; set; }
    public static bool gfInvalidTraversal { get; internal set; }
    public static bool gfLoneEPCAttemptingTraversal { get; internal set; }
    public static int gubLoneMercAttemptingToAbandonEPCs { get; internal set; }
    public static int gbPotentiallyAbandonedEPCSlotID { get; internal set; }
    public static bool gfRobotWithoutControllerAttemptingTraversal { get; internal set; }
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

//    public static Image<Rgba32> gpFrameBuffer;
//    public static Image<Rgba32> gpPrimarySurface;

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

    public static Image<Rgba32> gpMouseCursor { get; set; }
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
    public static bool gfFacePanelActive { get; internal set; }

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
    public const int MAX_DAILY_SALARY = 30000;// must fit into an int (32k)
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

    public static STRATEGIC_STATUS gStrategicStatus;

    // stat change causes
    public const int FROM_SUCCESS = 0;
    public const int FROM_TRAINING = 1;
    public const int FROM_FAILURE = 2;

    public const int BUDDY_OPINION = +25;
    public const int HATED_OPINION = -25;

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
    public const int TEXT_X = 140;
    public const int PAGE_SIZE = 22;
    public const int RECORD_Y = TOP_DIVLINE_Y;
    public const int RECORD_HISTORY_WIDTH = 200;
    public const int HISTORY_DATE_X = PAGE_NUMBER_X + 85;
    public const int HISTORY_DATE_Y = PAGE_NUMBER_Y;
    public const int RECORD_LOCATION_WIDTH = 142;//95
    public const FontStyle HISTORY_HEADER_FONT = FontStyle.FONT14ARIAL;
    public const FontStyle HISTORY_TEXT_FONT = FontStyle.FONT12ARIAL;
    public const int RECORD_DATE_X = TOP_X + 10;
    public const int RECORD_DATE_WIDTH = 31;//68
    public const int RECORD_HEADER_Y = 90;

    public const Items LAST_DEALER_ITEM = (Items)(-1);
    public const int NO_DEALER_ITEM = 0;
    // item suitability categories for dealer inventory initialization, virtual customer sales, and re-ordering
    public const int ITEM_SUITABILITY_NONE = 0;
    public const int ITEM_SUITABILITY_LOW = 1;
    public const int ITEM_SUITABILITY_MEDIUM = 2;
    public const int ITEM_SUITABILITY_HIGH = 3;
    public const int ITEM_SUITABILITY_ALWAYS = 4;
    public const int DEALER_BUYING = 0;
    public const int DEALER_SELLING = 1;


    public const int NUM_RECORDS_PER_PAGE = PAGE_SIZE;
    //static int SIZE_OF_HISTORY_FILE_RECORD( sizeof(int ) + sizeof(int ) + sizeof(int ) + sizeof(int ) + sizeof(int ) + sizeof(int ) + sizeof(int ) )

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

    public const string MAINMENU_TEXT_FILE = "LoadScreens\\MainMenu.edt";
    public const int MAINMENU_RECORD_SIZE = 80 * 2;
    public const int MAINMENU_X = (640 - 214) / 2;
    public const int MAINMENU_TITLE_Y = 75;
    public const int MAINMENU_Y_SPACE = 37;
    public const int MAINMENU_Y = 480 - 187;

    public static int[] gusMainMenuButtonWidths = new int[(int)MainMenuItems.NUM_MENU_ITEMS];
    public static MOUSE_REGION gBackRegion = new(nameof(gBackRegion));
    public static MainMenuItems gbHandledMainMenu = MainMenuItems.Unknown;

    public static bool gfMainMenuScreenEntry = false;
    public static bool gfMainMenuScreenExit = false;

    public static string guiCreditFacesKey;
    public static string guiCreditBackGroundImageKey;
    public static bool gfCreditsScreenEntry;
    public static bool gfCreditsScreenExit;
    public static ScreenName guiCreditsExitScreen;
    public static bool gfCrdtHaveRenderedFirstFrameToSaveBuffer;
    public static FontStyle guiCreditScreenActiveFont;
    public static FontColor gubCreditScreenActiveColor;
    public static FontStyle guiCreditScreenTitleFont;
    public static FontColor gubCreditScreenTitleColor;
    public static int guiCrdtNodeScrollSpeed;
    public static CreditRenderFlag gubCreditScreenRenderFlags;
    public static int giCurrentlySelectedFace;
    public static bool gfPauseCreditScreen;
    public static TextJustifies gubCrdtJustification;
    public static int guiCurrentCreditRecord;
    public static uint guiCrdtLastTimeUpdatingNode;
    public static int guiGapBetweenCreditSections;
    public static int guiGapBetweenCreditNodes;
    public static int guiGapTillReadNextCredit;
    public static MOUSE_REGION[] gCrdtMouseRegions = new MOUSE_REGION[(int)PeopleInCredits.NUM_PEOPLE_IN_CREDITS];

    public static List<CRDT_NODE> gCrdtNodes = new();

    public static readonly CDRT_FACE[] gCreditFaces = new CDRT_FACE[]
    {
        //  x	    y	    		w		h			
	    new(298, 137,           37, 49, 310, 157,       304, 170,   2500, 0, 0),											//Camfield
	    new(348, 137,           43, 47, 354, 153,       354, 153,   3700, 0, 0),											//Shawn
	    new(407, 132,           30, 50, 408, 151,       410, 164,   3000, 0, 0),											//Kris
	    new(443, 131,           30, 50, 447, 151,       446, 161,   4000, 0, 0),											//Ian
	    new(487, 136,           43, 50, 493, 155,       493, 155,   3500, 0, 0),											//Linda
	    new(529, 145,           43, 50, 536, 164,       536, 164,   4000, 0, 0),											//Eric
	    new(581, 132,           43, 48, 584, 150,       583, 161,   3500, 0, 0),											//Lynn
	    new(278, 211,           36, 51, 283, 232,       283, 241,   3700, 0, 0),											//Norm
	    new(319, 210,           34, 49, 323, 227,       320, 339,   4000, 0, 0),											//George
	    new(358, 211,           38, 49, 364, 226,       361, 239,   3600, 0, 0),											//Andrew Stacey
	    new(396, 200,           42, 50, 406, 220,       403, 230,   4600, 0, 0),											//Scott
	    new(444, 202,           43, 51, 452, 220,       452, 231,   2800, 0, 0),											//Emmons
	    new(493, 188,           36, 51, 501, 207,       499, 217,   4500, 0, 0),											//Dave
	    new(531, 199,           47, 56, 541, 221,       540, 232,   4000, 0, 0),											//Alex
	    new(585, 196,           39, 49, 593, 218,       593, 228,   3500, 0, 0),                                            //Joey
    };

    public static CRDT_NODE? gCrdtLastAddedNode;

    public static STRATEGICEVENT? gpEventList = null;
    public static bool gfPreventDeletionOfAnyEvent = false;
    public static bool gfEventDeletionPending = false;
    public static bool gfProcessingGameEvents = false;
    public static uint guiTimeStampOfCurrentlyExecutingEvent = 0;

    public static bool gfRenderPBInterface;
    public static bool gfPBButtonsHidden;
    public static bool fDisableMapInterfaceDueToBattle = false;

    public static int ubCurrentContractRenewal = 0;
    public static int ubCurrentContractRenewalInProgress = 0;
    public static bool gfContractRenewalSquenceOn = false;
    public static bool gfInContractMenuFromRenewSequence = false;

    // These defines correlate to defines in font.h
    public const FontColor MSG_FONT_RED = FontColor.FONT_MCOLOR_RED;
    public const FontColor MSG_FONT_YELLOW = FontColor.FONT_MCOLOR_LTYELLOW;
    public const FontColor MSG_FONT_WHITE = FontColor.FONT_MCOLOR_WHITE;


    // the dirty state of the mapscreen interface bottom
    public static bool fMapBottomDirtied = false;
    //Used to flag the transition animation from mapscreen to laptop.
    public static bool gfStartMapScreenToLaptopTransition = false;
    // leaving map screen
    public static bool fLeavingMapScreen = false;
    // don't start transition from laptop to tactical stuff
    public static bool gfDontStartTransitionFromLaptop = false;
    // exiting to laptop?
    public static bool fLapTop = false;
    public static bool gfOneFramePauseOnExit = false;

    // disable team info panels due to battle roster
    public static bool fDisableDueToBattleRoster = false;


    public const int HOSPITAL_PATIENT_DISTANCE = 9;

    public static int giHospitalTempBalance;
    public static int giHospitalRefund;
    public static int gbHospitalPriceModifier;

    // the flags
    public static MERC_POPUP_PREPARE_FLAGS guiFlags = 0;
    public static int guiBoxIcons;
    public static int guiSkullIcons;

    public const int TEXT_POPUP_WINDOW_TEXT_OFFSET_X = 8;
    public const int TEXT_POPUP_WINDOW_TEXT_OFFSET_Y = 8;
    public const int TEXT_POPUP_STRING_WIDTH = 296;
    public const int TEXT_POPUP_GAP_BN_LINES = 10;
    public const FontStyle TEXT_POPUP_FONT = FontStyle.FONT12ARIAL;
    public const FontColor TEXT_POPUP_COLOR = FontColor.FONT_MCOLOR_WHITE;
    public const FontStyle MERC_TEXT_FONT = FontStyle.FONT12ARIAL;
    public const FontColor MERC_TEXT_COLOR = FontColor.FONT_MCOLOR_WHITE;
    public const int MERC_TEXT_MIN_WIDTH = 10;
    public const int MERC_TEXT_POPUP_WINDOW_TEXT_OFFSET_X = 10;
    public const int MERC_TEXT_POPUP_WINDOW_TEXT_OFFSET_Y = 10;
    public const int MERC_BACKGROUND_WIDTH = 350;
    public const int MERC_BACKGROUND_HEIGHT = 200;

    // the max number of pop up boxes availiable to user
    public const int MAX_NUMBER_OF_POPUP_BOXES = 10;

    public const int DIRTY_QUEUES = 200;
    public const int BACKGROUND_BUFFERS = 500;
    public const int VIDEO_OVERLAYS = 100;


    public static BACKGROUND_SAVE[] gBackSaves = new BACKGROUND_SAVE[BACKGROUND_BUFFERS];
    public static int guiNumBackSaves = 0;

    public static VIDEO_OVERLAY[] gVideoOverlays = new VIDEO_OVERLAY[VIDEO_OVERLAYS];
    public static int guiNumVideoOverlays = 0;

    public static SixLabors.ImageSharp.Rectangle gDirtyClipRect = new(0, 0, 640, 480);

    public const int MAX_LINE_COUNT = 6;
    public const int MAX_AGE = 10000;
    public const int X_START = 2;
    public const int Y_START = 330;
    public const int MAP_LINE_WIDTH = 300;
    public const int WIDTH_BETWEEN_NEW_STRINGS = 5;

    public static ScrollStringStPtr?[] gpDisplayList = new ScrollStringStPtr?[MAX_LINE_COUNT];
    public static ScrollStringStPtr?[] gMapScreenMessageList = new ScrollStringStPtr?[256];
    public static int gubStartOfMapScreenMessageList = 0;
    public static int gubEndOfMapScreenMessageList = 0;
    public static int gubCurrentMapMessageString = 0;

    public static FontStyle MAP_SCREEN_MESSAGE_FONT = FontStyle.TINYFONT1;
    public const int MAX_MESSAGES_ON_MAP_BOTTOM = 9;

    // exit states
    public static int gbExitingMapScreenToWhere = -1;
    public static int gubFirstMapscreenMessageIndex = 0;
    public static int guiCompressionStringBaseTime = 0;

    // graphics
    public static int guiMAPBOTTOMPANEL;
    public static int guiSliderBar;

    // buttons
    public static int[] guiMapMessageScrollButtons = new int[2];
    public static int[] guiMapBottomExitButtons = new int[3];
    public static int[] guiMapBottomTimeButtons = new int[2];

    // buttons images
    public static int[] guiMapMessageScrollButtonsImage = new int[2];
    public static int[] guiMapBottomExitButtonsImage = new int[3];
    public static int[] guiMapBottomTimeButtonsImage = new int[2];

    // mouse regions
    public static MOUSE_REGION gMapMessageScrollBarRegion;
    public static MOUSE_REGION gMapPauseRegion;

    public static MOUSE_REGION[] gTimeCompressionMask = new MOUSE_REGION[3];

    public const FontColor BETAVERSION_COLOR = FontColor.FONT_ORANGE;
    public const FontColor TESTVERSION_COLOR = FontColor.FONT_GREEN;
    public const FontColor DEBUG_COLOR = FontColor.FONT_RED;
    public const FontColor DIALOGUE_COLOR = FontColor.FONT_WHITE;
    public const FontColor INTERFACE_COLOR = FontColor.FONT_YELLOW;


    // the next said quote will pause time
    public static bool fPausedTimeDuringQuote = false;
    public static bool fWasPausedDuringDialogue = false;

    public static int gubLogForMeTooBleeds = 0;

    // has the text region been created?
    public static bool fTextBoxMouseRegionCreated = false;
    public static bool fExternFaceBoxRegionCreated = false;

    // due to last quote system?
    public static bool fDialogueBoxDueToLastMessage = false;

    // last quote timers
    public static int guiDialogueLastQuoteTime = 0;
    public static int guiDialogueLastQuoteDelay = 0;

    public static MOUSE_REGION gTextBoxMouseRegion;
    public static MOUSE_REGION gFacePopupMouseRegion;
    public static bool gfUseAlternateDialogueFile = false;

    public static bool gfDialogueQueuePaused = false;
    public static int gusSubtitleBoxWidth;
    public static int gusSubtitleBoxHeight;
    public static int giTextBoxOverlay = -1;
    public static int guiScreenIDUsedWhenUICreated;
    public static int[] gzQuoteStr = new int[QUOTE_MESSAGE_SIZE];

    // set the top position value for merc dialogue pop up boxes
    public const int gsTopPosition = 20;

    public const int DIALOGUESIZE = 480;
    public const int QUOTE_MESSAGE_SIZE = 520;
    public const int TALK_PANEL_FACE_X = 6;
    public const int TALK_PANEL_FACE_Y = 9;
    public const int TALK_PANEL_NAME_X = 5;
    public const int TALK_PANEL_NAME_Y = 114;
    public const int TALK_PANEL_NAME_WIDTH = 92;
    public const int TALK_PANEL_NAME_HEIGHT = 15;
    public const int TALK_PANEL_MENU_STARTY = 8;
    public const int TALK_PANEL_MENU_HEIGHT = 24;
    public const int TALK_MENU_WIDTH = 96;
    public const int TALK_MENU_HEIGHT = 16;
    public const int DIALOGUE_DEFAULT_SUBTITLE_WIDTH = 200;
    public const int TEXT_DELAY_MODIFIER = 60;


    //When set, the fast help text will be instantaneous, if consecutive regions with help text are
    //hilighted.  It is set, whenever the timer for the first help button expires, and the mode is
    //cleared as soon as the cursor moves into no region or a region with no helptext.
    public static bool gfPersistantFastHelpMode;
    public static int gsFastHelpDelay = 600; // In timer ticks
    public static bool gfShowFastHelp = true;
    public static int gusClickedIDNumber;
    public static bool gfClickedModeOn = false;


    //Records and stores the last place the user clicked.  These values are compared to the current
    //click to determine if a double click event has been detected.
    public static MOUSE_REGION? gpRegionLastLButtonDown = null;
    public static MOUSE_REGION? gpRegionLastLButtonUp = null;
    public static long guiRegionLastLButtonDownTime = 0;
    public static bool gfRefreshUpdate = false;
    public static bool fDisableJustForIan;

    public static long guiStartupTime;
    public static long guiCurrentTime;
    public static bool fClockMouseRegionCreated;
    public static bool gfPauseClock = false;

    // clock mouse region
    public static MOUSE_REGION? gClockMouseRegion;
    public static MOUSE_REGION? gClockScreenMaskMouseRegion;

    public static uint GetJA2Clock() => ClockManager.GetJA2Clock();

    internal static int wcslen(string src) => src.Length;

    public static void MemFree(object? _) => _ = null;

    public static Random _random = new Random();
    public static Random Random
    {
        get
        {
            if (_random is null)
            {
                _random = new Random();
            }

            return _random;
        }
    }

    public static INTERACTIVE_TILE_STACK_TYPE? gCurIntTileStack;
    public static bool gfCycleIntTile = false;
    public static CUR_INTERACTIVE_TILE gCurIntTile;
    public static bool gfOverIntTile = false;
    // Values to determine if we should check or not
    public static int gsINTOldRenderCenterX = 0;
    public static int gsINTOldRenderCenterY = 0;
    public static int gusINTOldMousePosX = 0;
    public static int gusINTOldMousePosY = 0;

    public const int MAP_BORDER_X = 261;
    public const int MAP_BORDER_Y = 0;
    public const int MAP_BORDER_CORNER_X = 584;
    public const int MAP_BORDER_CORNER_Y = 279;

    public static List<int> ghVideoObjects = new();
    public static bool gfVideoObjectsInit = false;
    public static VOBJECT_NODE gpVObjectHead { get; set; } = new VOBJECT_NODE();
    public static VOBJECT_NODE gpVObjectTail { get; set; } = new VOBJECT_NODE();
    public static int guiVObjectIndex { get; set; } = 1;
    public static int guiVObjectSize { get; set; } = 0;
    public static int guiVObjectTotalAdded { get; set; } = 0;

    public static int[] gubOutOfTurnOrder = new int[Globals.MAXMERCS];
    //gubOutOfTurnOrder[0] = END_OF_INTERRUPTS;
    //    gubOutOfTurnOrder[1] = 0;

    public const int END_OF_INTERRUPTS = 255;
    public static int gubOutOfTurnPersons = 0;

    public static int gubLastInterruptedGuy = 0;

    public const int MIN_APS_TO_INTERRUPT = 4;

    public const int EVERYBODY = MAXMERCS;

    public static bool gfWaitingForTriggerTimer;

    public const int MAX_MISC_NOISE_DURATION = 12;      // max dur for VERY loud NOBODY noises

    public const int DOOR_NOISE_VOLUME = 2;
    public const int WINDOW_CRACK_VOLUME = 4;
    public const int WINDOW_SMASH_VOLUME = 8;
    public const int MACHETE_VOLUME = 9;
    public const int TRIMMER_VOLUME = 18;
    public const int CHAINSAW_VOLUME = 30;
    public const int SMASHING_DOOR_VOLUME = 6;
    public const int CROWBAR_DOOR_VOLUME = 4;
    public const int ITEM_THROWN_VOLUME = 2;

    public const int TIME_BETWEEN_RT_OPPLIST_DECAYS = 20;

    // this is a fake "level" value (0 on ground, 1 on roof) for
    // HearNoise to ignore the effects of lighting(?)
    public const int LIGHT_IRRELEVANT = 127;

    public const int AUTOMATIC_INTERRUPT = 100;
    public const int NO_INTERRUPT = 127;

    public const int NEXT_TILE_CHECK_DELAY = 700;

    public const int MOVE_TILE_CLEAR = 1;
    public const int MOVE_TILE_TEMP_BLOCKED = -1;
    public const int MOVE_TILE_STATIONARY_BLOCKED = -2;

    public static bool gfFadeIn = false;
    public static bool gfFadeOut = false;
    public static bool gfFadeOutDone = false;
    public static bool gfFadeInDone = false;

    public static int gubElementsOnExplosionQueue;
    public static int gubAICounter;

    public static STRUCTURE? gStruct;


    //
    // Commented out/ to fix:
    // lots of other stuff, I think
    //

    public const int DEADLOCK_DELAY = 15000;

    // Very representing if this computer is the host, therefore controlling the ai
    public static byte gfAmIHost;

    // Defines
    public const int NUM_BULLET_SLOTS = 50;

    // GLOBAL FOR FACES LISTING
    public static BULLET[] gBullets = new BULLET[NUM_BULLET_SLOTS];
    public static int guiNumBullets = 0;

    public const int NOSCORE = 99;

    // ANY NEW ACTIONS ADDED - UPDATE OVERHEAD.C ARRAY WITH ACTION'S STRING VALUE
    public const AI_ACTION FIRST_MOVEMENT_ACTION = AI_ACTION.RANDOM_PATROL;
    public const AI_ACTION LAST_MOVEMENT_ACTION = AI_ACTION.MOVE_TO_CLIMB;

    public const AnimationStates NO_PENDING_ANIMATION = (AnimationStates)32001;
    public const WorldDirections NO_PENDING_DIRECTION = (WorldDirections)253;
    public const int NO_PENDING_STANCE = 254;
    public const int NO_DESIRED_HEIGHT = 255;

    public const InventorySlot NO_SLOT = (InventorySlot)(-1);

    // MACRO FOR DEFINING OF ITEM IS VISIBLE
    public static bool ITEMPOOL_VISIBLE(ITEM_POOL pItemPool) => ((pItemPool.bVisible >= (ItemVisibility)1)
        || (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.SHOW_ALL_ITEMS)));

    // NB brothel rooms 88-90 removed because they are the antechamber
    public static bool IN_BROTHEL(int room) => (gWorldSectorX == 5 && gWorldSectorY == MAP_ROW.C && (room) >= 91 && (room) <= 119);
    public static bool IN_BROTHEL_GUARD_ROOM(int room) => (room == 110);
    public static bool IN_KINGPIN_HOUSE(int room) => (gWorldSectorX == 5 && gWorldSectorY == MAP_ROW.D && (room) >= 30 && (room) <= 39);

    public const int MOVEINTERRUPT = 0;
    public const int SIGHTINTERRUPT = 1;
    public const int NOISEINTERRUPT = 2;

    public const int NUM_WATCHED_LOCS = 3;

    public const NPCID NO_PROFILE = NPCID.NO_PROFILE;

    public static Dictionary<TEAM, int[]> gbPublicOpplist = new();
    //public static int[,] gbPublicOpplist = new int[MAXTEAMS, TOTAL_SOLDIERS];
    public static Dictionary<int, List<int>> gbSeenOpponents = new();// int[TOTAL_SOLDIERS, TOTAL_SOLDIERS];
    public static Dictionary<int, List<int>> gsLastKnownOppLoc = new();// int[TOTAL_SOLDIERS][];// [TOTAL_SOLDIERS];		// merc vs. merc
    public static Dictionary<int, List<int>> gbLastKnownOppLevel = new();// int[TOTAL_SOLDIERS][];// [TOTAL_SOLDIERS];
    public static Dictionary<TEAM, List<int>> gsPublicLastKnownOppLoc = new(); //int[MAXTEAMS, TOTAL_SOLDIERS];	// team vs. merc
    public static Dictionary<TEAM, List<int>> gbPublicLastKnownOppLevel = new();
    public static Dictionary<TEAM, int> gubPublicNoiseVolume = new();
    public static Dictionary<TEAM, int> gsPublicNoiseGridno = new();
    public static Dictionary<TEAM, int> gbPublicNoiseLevel = new();
    public static int gfKnowAboutOpponents;

    public static int gfPlayerTeamSawJoey;
    public static int gfMikeShouldSayHi;

    public static int[,] gsWatchedLoc = new int[TOTAL_SOLDIERS, NUM_WATCHED_LOCS];
    public static int[,] gbWatchedLocLevel = new int[TOTAL_SOLDIERS, NUM_WATCHED_LOCS];
    public static int[,] gubWatchedLocPoints = new int[TOTAL_SOLDIERS, NUM_WATCHED_LOCS];
    public static bool[,] gfWatchedLocReset = new bool[TOTAL_SOLDIERS, NUM_WATCHED_LOCS];

    public const int BEST_SIGHTING_ARRAY_SIZE = 6;
    public const int BEST_SIGHTING_ARRAY_SIZE_ALL_TEAMS_LOOK_FOR_ALL = 6;
    public const int BEST_SIGHTING_ARRAY_SIZE_NONCOMBAT = 3;
    public const int BEST_SIGHTING_ARRAY_SIZE_INCOMBAT = 0;

    public static List<DOOR_STATUS> gpDoorStatus = new();
    public static int gubNumDoorStatus = 0;
    public static int[] gbMercIsNewInThisSector = new int[MAX_NUM_SOLDIERS];
    public static bool gfPlotDirectPath { get; set; } = false;
    public static bool gusPathShown { get; set; } = false;
    public static int gusAPtsToMove { get; set; } = 0;
    public static bool fReDrawScreenFlag { get; set; }
    public static INTERFACE guiTacticalInterfaceFlags { get; set; }
    public static bool fTeamPanelDirty { get; internal set; }

    public const int MANLOOKSFORMAN = 0;
    public const int HEARNOISE = 1;
    public const int NOTICEUNSEENATTACKERS = 2;

    public const double MAJOR_MAP_VERSION = 5.00;

    public static double gdMajorMapVersion = MAJOR_MAP_VERSION;
    public static bool gfWorldLoaded;

    //Current minor map version updater.
    public const int MINOR_MAP_VERSION = 25;
    public static int gubMinorMapVersion = MINOR_MAP_VERSION;

    // for ManLooksForMan()
    public const int MANLOOKSFOROTHERTEAMS = 0;
    public const int OTHERTEAMSLOOKFORMAN = 1;
    public const int VERIFYANDDECAYOPPLIST = 2;
    public const int HANDLESTEPPEDLOOKAT = 3;
    public const int LOOKANDLISTEN = 4;
    public const int UPDATEPUBLIC = 5;
    public const int CALLER_UNKNOWN = 6;

    // this variable is a flag used in HandleSight to determine whether (while in non-combat RT)
    // someone has just been seen, EITHER THE MOVER OR SOMEONE THE MOVER SEES
    public static int gfPlayerTeamSawCreatures = 0;

    public static int[] gubBestToMakeSighting = new int[BEST_SIGHTING_ARRAY_SIZE];
    public static int gubBestToMakeSightingSize = 0;
    //bool		gfHumanSawSomeoneInRealtime;

    public static bool gfDelayResolvingBestSightingDueToDoor = false;

    public const int SHOULD_BECOME_HOSTILE_SIZE = 32;

    public static int[] gubShouldBecomeHostileOrSayQuote = new int[SHOULD_BECOME_HOSTILE_SIZE];
    public static int gubNumShouldBecomeHostileOrSayQuote;

    // NB this ID is set for someone opening a door
    public static int gubInterruptProvoker = NOBODY;
    public static int[,] gubKnowledgeValue = new int[10, 10]
    {
       //   P E R S O N A L   O P P L I S T  //
       // -4   -3   -2   -1   0   1   2   3   4   5   //
       {   0,   1,   2,   3,  0,  5,  4,  3,  2,  1 }, // -4
       {   0,   0,   1,   2,  0,  4,  3,  2,  1,  0 }, // -3    O
       {   0,   0,   0,   1,  0,  3,  2,  1,  0,  0 }, // -2  P P
       {   0,   0,   0,   0,  0,  2,  1,  0,  0,  0 }, // -1  U P
       {   0,   1,   2,   3,  0,  5,  4,  3,  2,  1 }, //  0  B L
       {   0,   0,   0,   0,  0,  0,  0,  0,  0,  0 }, //  1  L I
       {   0,   0,   0,   0,  0,  1,  0,  0,  0,  0 }, //  2  I S
       {   0,   0,   0,   1,  0,  2,  1,  0,  0,  0 }, //  3  C T
       {   0,   0,   1,   2,  0,  3,  2,  1,  0,  0 }, //  4
       {   0,   1,   2,   3,  0,  4,  3,  2,  1,  0 }  //  5
    
    /*
       //   P E R S O N A L   O P P L I S T  //
       // -3   -2   -1   0   1   2   3   4   //
       {   0,   1,   2,  0,  4,  3,  2,  1  ) }, // -3    O
       {   0,   0,   1,  0,  3,  2,  1,  0  ) }, // -2  P P
       {   0,   0,   0,  0,  2,  1,  0,  0  ) }, // -1  U P
       {   1,   2,   3,  0,  5,  4,  3,  2  ) }, //  0  B L
       {   0,   0,   0,  0,  0,  0,  0,  0  ) }, //  1  L I
       {   0,   0,   0,  0,  1,  0,  0,  0  ) }, //  2  I S
       {   0,   0,   1,  0,  2,  1,  0,  0  ) }, //  3  C T
       {   0,   1,   2,  0,  3,  2,  1,  0   }  //  4
    	 */
     };

    public const int MAX_WATCHED_LOC_POINTS = 4;
    public const int WATCHED_LOC_RADIUS = 1;

    public const int RTP_COMBAT_AGGRESSIVE = 1;
    public const int RTP_COMBAT_CONSERVE = 2;
    public const int RTP_COMBAT_REFRAIN = 3;

    public const int NOT_NEW_SITUATION = 0;
    public const int WAS_NEW_SITUATION = 1;
    public const int IS_NEW_SITUATION = 2;

    public const int DIFF_ENEMY_EQUIP_MOD = 0;
    public const int DIFF_ENEMY_TO_HIT_MOD = 1;
    public const int DIFF_ENEMY_INTERRUPT_MOD = 2;
    public const int DIFF_RADIO_RED_ALERT = 3;
    public const int DIFF_MAX_COVER_RANGE = 4;
    public const int MAX_DIFF_PARMS = 5;      // how many different difficulty variable=s?;

    public static bool[,] gfWatchedLocHasBeenIncremented = new bool[TOTAL_SOLDIERS, NUM_WATCHED_LOCS];

    public const int AI_LIMIT_PER_UPDATE = 1;
    public static bool gfTurnBasedAI;

    public static int[,] gbDiff =
    {
     //       AI DIFFICULTY SETTING
     // WIMPY  EASY  NORMAL  TOUGH  ELITE
      {  -20,  -10,     0,    10,     20 },     // DIFF_ENEMY_EQUIP_MOD
      {  -10,   -5,     0,     5,     10 },     // DIFF_ENEMY_TO_HIT_MOD
      {   -2,   -1,     0,     1,      2 },     // DIFF_ENEMY_INTERRUPT_MOD
      {   50,   65,    80,    90,     95 },     // DIFF_RADIO_RED_ALERT
      {    4,    6,     8,    10,     13 }      // DIFF_MAX_COVER_RANGE
    };

    public static uint BOMB_QUEUE_DELAY { get; } = (uint)(1000 + Random.Next(500));

    public const int MAX_BOMB_QUEUE = 40;
    public static ExplosionQueueElement[] gExplosionQueue = new ExplosionQueueElement[MAX_BOMB_QUEUE];
    public static bool gfExplosionQueueActive = false;

    public const int ERASE_SPREAD_EFFECT = 2;
    public const int BLOOD_SPREAD_EFFECT = 3;
    public const int REDO_SPREAD_EFFECT = 4;

    public static bool gfExplosionQueueMayHaveChangedSight = false;
    public static int gubPersonToSetOffExplosions = NOBODY;

    public static int gsTempActionGridNo = NOWHERE;

    public const int NUM_EXPLOSION_SLOTS = 100;

    public const int MAX_DISTANCE_EXPLOSIVE_CAN_DESTROY_STRUCTURES = 2;

    public const int STRUCTURE_DAMAGE_EXPLOSION = 1;
    public const int STRUCTURE_DAMAGE_GUNFIRE = 2;


    // GLOBAL FOR SMOKE LISTING
    public static EXPLOSIONTYPE[] gExplosionData = new EXPLOSIONTYPE[NUM_EXPLOSION_SLOTS];
    public static int guiNumExplosions = 0;


    public static bool gfBasement = false;
    public static bool gfCaves = false;

    public int[,] gbLookDistance =
    {
        //  LOOKER DIR       LOOKEE DIR
        //                   NORTH    | NORTHEAST  |   EAST   |  SOUTHEAST  |   SOUTH  |  SOUTHWEST  |   WEST    |  NORTHWEST
        {/* NORTH      */     STRAIGHT,     ANGLE,       SIDE,     SBEHIND,     BEHIND,     SBEHIND,       SIDE,       ANGLE },
        {/* NORTHEAST  */     ANGLE,     STRAIGHT,      ANGLE,        SIDE,    SBEHIND,      BEHIND,    SBEHIND,        SIDE },
        {/* EAST       */     SIDE,         ANGLE,   STRAIGHT,       ANGLE,       SIDE,     SBEHIND,     BEHIND,     SBEHIND },
        {/* SOUTHEAST  */     SBEHIND,       SIDE,      ANGLE,    STRAIGHT,      ANGLE,        SIDE,    SBEHIND,      BEHIND },
        {/* SOUTH      */     BEHIND,     SBEHIND,       SIDE,       ANGLE,   STRAIGHT,       ANGLE,       SIDE,     SBEHIND },
        {/* SOUTHWEST  */     SBEHIND,     BEHIND,    SBEHIND,        SIDE,      ANGLE,    STRAIGHT,      ANGLE,        SIDE },
        {/* WEST       */     SIDE,       SBEHIND,     BEHIND,     SBEHIND,       SIDE,       ANGLE,   STRAIGHT,       ANGLE },
        {/* NORTHWEST  */     ANGLE,         SIDE,     SBEHIND,     BEHIND,    SBEHIND,        SIDE,      ANGLE,    STRAIGHT },
    };

    public const int SMELL_STRENGTH_MAX = 63;
    public const int BLOOD_STRENGTH_MAX = 7;
    public const int BLOOD_DELAY_MAX = 3;

    public static int[] gbSmellStrength =
    {
        NORMAL_HUMAN_SMELL_STRENGTH, // normal 
    	NORMAL_HUMAN_SMELL_STRENGTH + 2, // slob
    	NORMAL_HUMAN_SMELL_STRENGTH - 1  // snob
    };

    public const int HUMAN = 0;
    public const int CREATURE_ON_FLOOR = 0x01;
    public const int CREATURE_ON_ROOF = 0x02;
    public const int NORMAL_HUMAN_SMELL_STRENGTH = 10;
    public const int COW_SMELL_STRENGTH = 15;
    public const int NORMAL_CREATURE_SMELL_STRENGTH = 20;
    public const int MAXBLOODQUANTITY = 7;
    public const int BLOODDIVISOR = 10;
    public const int SMELL_TYPE_NUM_BITS = 2;

    public static int gsTreeRevealXPos;
    public static int gsTreeRevealYPos;

    public const int MAX_HUMAN_CREATURE_SMELL = (NORMAL_HUMAN_SMELL_STRENGTH - 1);

    public static int gsWhoThrewRock = NOBODY;

    public const int NIGHTSIGHTGOGGLES_BONUS = 2;
    public const int UVGOGGLES_BONUS = 4;

    // % values of sighting distance at various light levels

    public static int[,] gbLightSighting = new int[1, 16]
    {
    { // human
    	 80, // brightest
    	 86,
         93,
        100, // normal daylight, 3
    	 94,
         88,
         82,
         76,
         70, // mid-dawn, 8
    	 64,
         58,
         51,
         43, // normal nighttime, 12 (11 tiles)
    	 30,
         17,
          9
    }
    };

    public static SIGHT gubSightFlags = 0;

    public const int CONDBUL = 0;
    public const int COND = 0;
    public const int SINGLE = 0;
    public const int LIQ = 0;
    public const int USAGE = 0;
    public const int BUCKS = 0;


    public static Dictionary<Items, INVTYPE> Item = new()
    {
        //  							CLASS								SOUND			GRPH	GRA-			PER			         
        //CLASS						INDEX		CURSOR			TYPE			TYPE	PHIC	WT	PCKT	PRICE COOL	DESCRIPTION							REL		REPAIR	FLAGS
        //---------				-----		-------     -------		----	--	  --  ----  -----	----	-----------							---		------		-----
        { 0, new(IC.PUNCH, 0, CURS.PUNCHCURS, 0, 0, 0, 0, 0, 0, 0,      /* nothing! */            0, 0, ItemAttributes.ITEM_NOT_BUYABLE) },
        //---WEAPONS---
        // NB For convenience, in accessing the Weapons table, the class index
        // of a weapon must be equal to its position in the Item table
        { (Items)01, new(IC.GUN,                1,          CURS.TARGETCURS,    CONDBUL,    0,      1,      6,  1,       350,   2,		/* Glock 17        */	+2,     +2,         ItemAttributes.IF_STANDARD_GUN) },
        { (Items)02, new(IC.GUN,                    2,          CURS.TARGETCURS,    CONDBUL,    0,      2,      6,  1,       480,   2,		/* Glock 18        */	+1,     +1,         ItemAttributes.IF_STANDARD_GUN | ItemAttributes.ITEM_BIGGUNLIST) },
        { (Items)03, new(IC.GUN,                    3,          CURS.TARGETCURS,    CONDBUL,    0,      3,      11, 1,       450,   2,		/* Beretta 92F     */	-1,     -1,         ItemAttributes.IF_STANDARD_GUN | ItemAttributes.ITEM_BIGGUNLIST) },
        { (Items)04, new(IC.GUN,                    4,          CURS.TARGETCURS,    CONDBUL,    0,      4,      11, 1,       470,   2,		/* Beretta 93R     */	-2,     -2,         ItemAttributes.IF_STANDARD_GUN) },
        { (Items)05, new(IC.GUN,                    5,          CURS.TARGETCURS,    CONDBUL,    0,      5,      11, 1,       250,   1,		/* .38 S&W Special */	+4,     +4,         ItemAttributes.IF_STANDARD_GUN) },
        { (Items)06, new(IC.GUN,                    6,          CURS.TARGETCURS,    CONDBUL,    0,      6,      10, 1,       300,   1,		/* .357 Barracuda  */	+3,     +3,         ItemAttributes.IF_STANDARD_GUN | ItemAttributes.ITEM_BIGGUNLIST) },
        { (Items)07, new(IC.GUN,                    7,          CURS.TARGETCURS,    CONDBUL,    0,      7,      17, 1,       300,   1,		/* .357 DesertEagle*/ -1,       -1,         ItemAttributes.IF_STANDARD_GUN) },
        { (Items)08, new(IC.GUN,                    8,          CURS.TARGETCURS,    CONDBUL,    0,      8,      11, 1,       400,   2,		/* .45 M1911			 */  0,     0,          ItemAttributes.IF_STANDARD_GUN | ItemAttributes.ITEM_BIGGUNLIST) },
        { (Items)09, new(IC.GUN,                    9,          CURS.TARGETCURS,    CONDBUL,    0,      9,      21, 0,       980,   3,		/* H&K MP5K      	 */	-1,     0,          ItemAttributes.IF_STANDARD_GUN) },
        { (Items)10, new(IC.GUN,                    10,         CURS.TARGETCURS,    CONDBUL,    0,      10,     28, 0,      1170,   4,		/* .45 MAC-10	     */	-2,     -1,         ItemAttributes.IF_STANDARD_GUN) },
        { (Items)11, new(IC.GUN,                    11,         CURS.TARGETCURS,    CONDBUL,    0,      11,     48, 0,       700,   3,		/* Thompson M1A1   */	+3,     -3,         ItemAttributes.IF_TWOHANDED_GUN | ItemAttributes.ITEM_BIGGUNLIST) },
        { (Items)12, new(IC.GUN,                    12,         CURS.TARGETCURS,    CONDBUL,    0,      12,     26, 0,      1330,   5,		/* Colt Commando   */	 0,     -1,         ItemAttributes.IF_TWOHANDED_GUN) },
        { (Items)13, new(IC.GUN,                    13,         CURS.TARGETCURS,    CONDBUL,    0,      13,     31, 0,       770,   3,		/* H&K MP53		 		 */	-1,     -1,         ItemAttributes.IF_TWOHANDED_GUN | ItemAttributes.ITEM_BIGGUNLIST) },
        { (Items)14, new(IC.GUN,                    14,         CURS.TARGETCURS,    CONDBUL,    0,      14,     39, 0,      1180,   4,		/* AKSU-74         */ -2,       -1,         ItemAttributes.IF_TWOHANDED_GUN | ItemAttributes.ITEM_BIGGUNLIST) },
        { (Items)15, new(IC.GUN,                    15,         CURS.TARGETCURS,    CONDBUL,    0,      15,     28, 0,      2750,   9,		/* 5.7mm FN P90    */ -2,       -4,         ItemAttributes.IF_STANDARD_GUN | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)16, new(IC.GUN,                    16,         CURS.TARGETCURS,    CONDBUL,    0,      16,     19, 0,       620,   3,		/* Type-85         */ -4,       +2,         ItemAttributes.IF_TWOHANDED_GUN | ItemAttributes.ITEM_BIGGUNLIST) },
        { (Items)17, new(IC.GUN,                    17,         CURS.TARGETCURS,    CONDBUL,    0,      17,     39, 0,      1350,   5,		/* SKS             */ -4,       -2,         ItemAttributes.IF_TWOHANDED_GUN | ItemAttributes.ITEM_BIGGUNLIST) },
        { (Items)18, new(IC.GUN,                    18,         CURS.TARGETCURS,    CONDBUL,    0,      18,     43, 0,      1930,   6,		/* Dragunov        */ +2,       +2,         ItemAttributes.IF_TWOHANDED_GUN | ItemAttributes.ITEM_BIGGUNLIST) },
        { (Items)19, new(IC.GUN,                    19,         CURS.TARGETCURS,    CONDBUL,    0,      19,     66, 0,      1950,   6,		/* M24             */ +4,       +4,         ItemAttributes.IF_TWOHANDED_GUN) },
        { (Items)20, new(IC.GUN,                    20,         CURS.TARGETCURS,    CONDBUL,    0,      20,     36, 0,      2380,   8,		/* Steyr AUG       */ +1,       -2,         ItemAttributes.IF_TWOHANDED_GUN | ItemAttributes.ITEM_BIGGUNLIST) },
        { (Items)21, new(IC.GUN,                    21,         CURS.TARGETCURS,    CONDBUL,    0,      21,     41, 0,      1620,   6,		/* H&K G41         */ +1,       -1,         ItemAttributes.IF_TWOHANDED_GUN) },
        { (Items)22, new(IC.GUN,                    22,         CURS.TARGETCURS,    CONDBUL,    0,      22,     29, 0,      1100,   4,		/* Ruger Mini-14   */  0,       -1,         ItemAttributes.IF_TWOHANDED_GUN) },
        { (Items)23, new(IC.GUN,                    23,         CURS.TARGETCURS,    CONDBUL,    0,      23,     36, 0,      2680,   8,		/* C-7             */ -1,       -1,         ItemAttributes.IF_TWOHANDED_GUN) },
        { (Items)24, new(IC.GUN,                    24,         CURS.TARGETCURS,    CONDBUL,    0,      24,     36, 0,      1970,   7,		/* FA-MAS          */ -2,       -2,         ItemAttributes.IF_TWOHANDED_GUN | ItemAttributes.ITEM_BIGGUNLIST) },
        { (Items)25, new(IC.GUN,                    25,         CURS.TARGETCURS,    CONDBUL,    0,      25,     36, 0,      1830,   6,		/* AK-74           */ -1,       -2,         ItemAttributes.IF_TWOHANDED_GUN | ItemAttributes.ITEM_BIGGUNLIST) },
        { (Items)26, new(IC.GUN,                    26,         CURS.TARGETCURS,    CONDBUL,    0,      26,     43, 0,      1450,   5,		/* AKM             */ +2,       +2,         ItemAttributes.IF_TWOHANDED_GUN | ItemAttributes.ITEM_BIGGUNLIST) },
        { (Items)27, new(IC.GUN,                    27,         CURS.TARGETCURS,    CONDBUL,    0,      27,     29, 0,      2120,   7,		/* M-14            */ +1,       -1,         ItemAttributes.IF_TWOHANDED_GUN) },
        { (Items)28, new(IC.GUN,                    28,         CURS.TARGETCURS,    CONDBUL,    0,      28,     43, 0,      2680,   8,		/* FN-FAL          */	 0,     -1,         ItemAttributes.IF_TWOHANDED_GUN) },
        { (Items)29, new(IC.GUN,                    29,         CURS.TARGETCURS,    CONDBUL,    0,      29,     44, 0,      1570,   5,		/* H&K G3A3        */ +1,       -1,         ItemAttributes.IF_TWOHANDED_GUN | ItemAttributes.ITEM_BIGGUNLIST) },
        { (Items)30, new(IC.GUN,                    30,         CURS.TARGETCURS,    CONDBUL,    0,      30,     38, 0,      2530,   8,		/* H&K G11         */	+3,     -4,         ItemAttributes.IF_TWOHANDED_GUN | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)31, new(IC.GUN,                    31,         CURS.TARGETCURS,    CONDBUL,    0,      31,     36, 0,       670,   3,		/* Remington M870  */	+3,     +3,         ItemAttributes.IF_TWOHANDED_GUN) },
        { (Items)32, new(IC.GUN,                    32,         CURS.TARGETCURS,    CONDBUL,    0,      32,     38, 0,       980,   4,		/* SPAS-15         */ -2,       -2,         ItemAttributes.IF_TWOHANDED_GUN | ItemAttributes.ITEM_BIGGUNLIST) },
        { (Items)33, new(IC.GUN,                    33,         CURS.TARGETCURS,    CONDBUL,    0,      33,     41, 0,      2900,   9,		/* CAWS            */	-3,     -3,         ItemAttributes.IF_TWOHANDED_GUN | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)34, new(IC.GUN,                    34,         CURS.TARGETCURS,    CONDBUL,    0,      34,     68, 0,      3100,   10,		/* FN Minimi       */	-1,     -2,         ItemAttributes.IF_TWOHANDED_GUN) },
        { (Items)35, new(IC.GUN,                    35,         CURS.TARGETCURS,    CONDBUL,    0,      35,     48, 0,      3180,   10,		/* RPK-74          */ -1,       -2,         ItemAttributes.IF_TWOHANDED_GUN | ItemAttributes.ITEM_BIGGUNLIST) },
        { (Items)36, new(IC.GUN,                    36,         CURS.TARGETCURS,    CONDBUL,    0,      36,     93, 0,      3420,   10,		/* H&K 21E         */	+2,     +1,         ItemAttributes.IF_TWOHANDED_GUN | ItemAttributes.ITEM_BIGGUNLIST) },
        { (Items)37, new(IC.BLADE,                  37,         CURS.KNIFECURS,  0,             1,      79,     9,  2,       100,   2,		/* combat knife    */	+2,     +2,         ItemAttributes.IF_STANDARD_BLADE) },
        { (Items)38, new(IC.THROWING_KNIFE,        38,          CURS.TARGETCURS, 0,             1,      53,     1,  4,        50,   3,		/* throwing knife  */	-1,     -1,         ItemAttributes.IF_STANDARD_BLADE) },
        { (Items)39, new(IC.THROWN,                 39,         CURS.TOSSCURS,      0,              1,      57,     5,  2,         0,   0,		/* rock            */ 0,        0,          ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)40, new(IC.LAUNCHER,               40,         CURS.TRAJECTORYCURS,    0,      0,      37,     26, 0,       900,   7,		/* grenade launcher*/	0,      -1,         ItemAttributes.IF_TWOHANDED_GUN) },
        { (Items)41, new(IC.LAUNCHER,               41,         CURS.TRAJECTORYCURS,    0,      0,      0,      77, 0,      1800,   10,		/* mortar */					0,      -2,         ItemAttributes.IF_TWOHANDED_GUN) },
        { (Items)42, new(IC.THROWN,                 42,         CURS.TOSSCURS,      0,              1,      60,     4,  3,           0, 0,		/* another rock    */	0,      0,          ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)43, new(IC.BLADE,                  43,         CURS.KNIFECURS, 0,              0,      0,      0,  1,         0,   0,		/* yng male claws */	0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_DEFAULT_UNDROPPABLE) },
        { (Items)44, new(IC.BLADE,                  44,         CURS.KNIFECURS, 0,              0,      0,      0,  1,         0,   0,		/* yng fem claws  */	0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_DEFAULT_UNDROPPABLE) },
        { (Items)45, new(IC.BLADE,                  45,         CURS.KNIFECURS, 0,              0,      0,      0,  1,         0,   0,		/* old male claws */	0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_DEFAULT_UNDROPPABLE) },
        { (Items)46, new(IC.BLADE,                  46,         CURS.KNIFECURS, 0,              0,      0,      0,  1,         0,   0,		/* old fem claws  */	0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_DEFAULT_UNDROPPABLE) },
        { (Items)47, new(IC.TENTACLES,              47,         CURS.KNIFECURS, 0,              0,      0,      0,  1,         0,   0,		/* queen tentacles*/	0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_DEFAULT_UNDROPPABLE) },
        { (Items)48, new(IC.GUN,                    48,         CURS.TARGETCURS,    0,              0,      0,      0,  1,         0,   0,		/* queen spit */			0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_DEFAULT_UNDROPPABLE) },
        { (Items)49, new(IC.PUNCH,                  49,         CURS.PUNCHCURS, 0,              1,      102,    1,  4,        20,   2,		/* brass knuckles */	0,      0,          ItemAttributes.IF_STANDARD_BLADE) },
        { (Items)50, new(IC.LAUNCHER,               50,         CURS.INVALIDCURS,0,             0,      39,     13, 0,       500,   8,		/* underslung g.l.*/	0,      0,          ItemAttributes.IF_STANDARD_GUN) },
        { (Items)51, new(IC.GUN,                    51,         CURS.TARGETCURS,    0,              0,      38,     21, 0,       500,   9,		/* rocket Launcher*/	0,      -3,         ItemAttributes.IF_TWOHANDED_GUN) },	// now repairable
        { (Items)52, new(IC.BLADE,                  52,         CURS.KNIFECURS, 0,              0,      41,     13, 0,       200,   3,		/* machete */					0,      +3,         ItemAttributes.IF_STANDARD_BLADE) },
        { (Items)53, new(IC.BLADE,                  53,         CURS.KNIFECURS, 0,              0,      0,      0,  1,         0,   0,		/* bloodcat claws*/		0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_DEFAULT_UNDROPPABLE) },
        { (Items)54, new(IC.BLADE,                  54,         CURS.KNIFECURS, 0,              0,      0,      0,  1,         0,   0,		/* bloodcat bite */		0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_DEFAULT_UNDROPPABLE) },
        { (Items)55, new(IC.GUN,                    55,         CURS.TARGETCURS,    0,              0,      45,     40, 0,      5000,   0,		/* rocket rifle */		0,      -5,         ItemAttributes.IF_TWOHANDED_GUN | ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_ELECTRONIC) },
        { (Items)56, new(IC.GUN,                    56,         CURS.TARGETCURS,    0,              0,      40,     12, 0,      1000,   0,		/* Automag III */			0,      -2,         ItemAttributes.IF_STANDARD_GUN | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)57, new(IC.GUN,                    57,         CURS.TARGETCURS,    0,              0,      0,      0,  0,         0,   0,		/* infant spit */			0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_DEFAULT_UNDROPPABLE) },
        { (Items)58, new(IC.GUN,                    58,         CURS.TARGETCURS,    0,              0,      0,      0,  0,         0,   0,		/* yng male spit */		0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_DEFAULT_UNDROPPABLE) },
        { (Items)59, new(IC.GUN,                    59,         CURS.TARGETCURS,    0,              0,      0,      0,  0,         0,   0,		/* old male spit */		0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_DEFAULT_UNDROPPABLE) },
        { (Items)60, new(IC.GUN,                    60,         CURS.TARGETCURS,    0,              0,      37,     26, 0,         0,   0,		/* tank cannon  */		0,      0,          ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)61, new(IC.GUN,                    61,         CURS.TARGETCURS,        0,              0,      46,  12,    1,       500,   5,		/* dart gun */				0,      +3,         ItemAttributes.IF_STANDARD_GUN) },
        { (Items)62, new(IC.THROWING_KNIFE,        62,          CURS.TARGETCURS,        0,              1,      95,     1,  4,        50,   0,		/*bloody throw.knife*/0,        +4,         ItemAttributes.IF_STANDARD_BLADE | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)63, new(IC.GUN,                    63,         CURS.TARGETCURS,        0,              0,      48,  18,    0,         0,   0,		/* flamethrower */		0,      0,          ItemAttributes.IF_STANDARD_GUN | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)64, new(IC.PUNCH,                  64,         CURS.PUNCHCURS,     0,              1,      85,  30,    0,        40,   1,		/* Crowbar       */		0,      -4,         ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_DAMAGEABLE) },
        { (Items)65, new(IC.GUN,                    65,         CURS.TARGETCURS,        0,              0,      45,     40, 0,   10000, 0,		/* rocket rifle */		0,      -5,         ItemAttributes.IF_TWOHANDED_GUN | ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_ELECTRONIC) },
        { (Items)66, new(IC.NONE,                   0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)67, new(IC.NONE,                   0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)68, new(IC.NONE,                   0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)69, new(IC.NONE,                   0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)70, new(IC.NONE,                   0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        //---AMMO---
        { (Items)71, new(IC.AMMO,                   0,          CURS.INVALIDCURS,   0,              1,      32,     2,  8,        15,   2,		/* CLIP9_15 */				0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)72, new(IC.AMMO,                   1,          CURS.INVALIDCURS,   0,              1,      35,     3,  4,        30,   4,		/* CLIP9_30 */				0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)73, new(IC.AMMO,                   2,          CURS.INVALIDCURS,   0,              1,      33,     2,  8,        45,   4,		/* CLIP9_15_AP */			0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)74, new(IC.AMMO,                   3,          CURS.INVALIDCURS,   0,              1,      36,     3,  4,        90,   6,		/* CLIP9_30_AP */			0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)75, new(IC.AMMO,                   4,          CURS.INVALIDCURS,   0,              1,      34,     2,  8,        30,   3,		/* CLIP9_15_HP */			0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)76, new(IC.AMMO,                   5,          CURS.INVALIDCURS,   0,              1,      37,     3,  4,        60,   5,		/* CLIP9_30_HP */			0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)77, new(IC.AMMO,                   6,          CURS.INVALIDCURS,   0,              1,      24,     1,  8,         5,   1,		/* CLIP38_6 */				0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)78, new(IC.AMMO,                   7,          CURS.INVALIDCURS,   0,              1,      25,     1,  8,        15,   3,		/* CLIP38_6_AP */			0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)79, new(IC.AMMO,                   8,          CURS.INVALIDCURS,   0,              1,      26,     1,  8,        10,   2,		/* CLIP38_6_HP */			0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)80, new(IC.AMMO,                   9,          CURS.INVALIDCURS,   0,              1,      14,     2,  8,        10,   2,		/* CLIP45_7 */				0,      0,          ItemAttributes.IF_STANDARD_CLIP) },

        { (Items)81, new(IC.AMMO,                   10,         CURS.INVALIDCURS,   0,              1,       4,  10,    4,        45,   3,		/* CLIP45_30 */				0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)82, new(IC.AMMO,                   11,         CURS.INVALIDCURS,   0,              1,      15,     2,  8,        45,   4,		/* CLIP45_7_AP */			0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)83, new(IC.AMMO,                   12,         CURS.INVALIDCURS,   0,              1,       5,  10,    4,       135,   5,		/* CLIP45_30_AP */		0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)84, new(IC.AMMO,                   13,         CURS.INVALIDCURS,   0,              1,      16,     2,  8,        30,   3,		/* CLIP45_7_HP */			0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)85, new(IC.AMMO,                   14,         CURS.INVALIDCURS,   0,              1,       6,  10,    4,        90,   4,		/* CLIP45_30_HP */		0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)86, new(IC.AMMO,                   15,         CURS.INVALIDCURS,   0,              1,      11,     1,  8,        10,   1,		/* CLIP357_6 */				0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)87, new(IC.AMMO,                   16,         CURS.INVALIDCURS,   0,              1,      17,     3,  8,          15, 1,		/* CLIP357_9 */				0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)88, new(IC.AMMO,                   17,         CURS.INVALIDCURS,   0,              1,      12,     1,  8,        30,   3,		/* CLIP357_6_AP */		0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)89, new(IC.AMMO,                   18,         CURS.INVALIDCURS,   0,              1,      18,     3,  8,        45,   3,		/* CLIP357_9_AP */		0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)90, new(IC.AMMO,                   19,         CURS.INVALIDCURS,   0,              1,      13,     1,  8,        20,   2,		/* CLIP357_6_HP */		0,      0,          ItemAttributes.IF_STANDARD_CLIP) },

        { (Items)91, new(IC.AMMO,                   20,         CURS.INVALIDCURS,   0,              1,      19,     3,  8,        30,   2,		/* CLIP357_9_HP */		0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)92, new(IC.AMMO,                   21,         CURS.INVALIDCURS,   0,              1,       9,     6,  4,       150,   5,		/* CLIP545_30_AP */		0,      0,          ItemAttributes.IF_STANDARD_CLIP | ItemAttributes.ITEM_BIGGUNLIST) },
        { (Items)93, new(IC.AMMO,                   22,         CURS.INVALIDCURS,   0,              1,      10,     6,  4,       100,   4,		/* CLIP545_30_HP */		0,      0,          ItemAttributes.IF_STANDARD_CLIP | ItemAttributes.ITEM_BIGGUNLIST) },
        { (Items)94, new(IC.AMMO,                   23,         CURS.INVALIDCURS,   0,              1,       7,     5,  4,       150,   4,		/* CLIP556_30_AP */		0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)95, new(IC.AMMO,                   24,         CURS.INVALIDCURS,   0,              1,       8,     5,  4,       100,   3,		/* CLIP556_30_HP */		0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)96, new(IC.AMMO,                   25,         CURS.INVALIDCURS,   0,              1,      22,     3,  6,        60,   6,		/* CLIP762W_10_AP */	0,      0,          ItemAttributes.IF_STANDARD_CLIP | ItemAttributes.ITEM_BIGGUNLIST) },
        { (Items)97, new(IC.AMMO,                   26,         CURS.INVALIDCURS,   0,              1,      29,     8,  4,       180,   4,		/* CLIP762W_30_AP */	0,      0,          ItemAttributes.IF_STANDARD_CLIP | ItemAttributes.ITEM_BIGGUNLIST) },
        { (Items)98, new(IC.AMMO,                   27,         CURS.INVALIDCURS,   0,              1,      23,     3,  6,        40,   5,		/* CLIP762W_10_HP */	0,      0,          ItemAttributes.IF_STANDARD_CLIP | ItemAttributes.ITEM_BIGGUNLIST) },
        { (Items)99, new(IC.AMMO,                   28,         CURS.INVALIDCURS,   0,              1,      30,     8,  4,       120,   3,		/* CLIP762W_30_HP */	0,      0,          ItemAttributes.IF_STANDARD_CLIP | ItemAttributes.ITEM_BIGGUNLIST) },
        { (Items)100, new(IC.AMMO,                  29,         CURS.INVALIDCURS,   0,              1,       3,     1,  6,        30,   7,		/* CLIP762N_5_AP */		0,      0,          ItemAttributes.IF_STANDARD_CLIP) },

        { (Items)101, new(IC.AMMO,                  30,         CURS.INVALIDCURS,   0,              1,      27,     8,  4,       120,   6,		/* CLIP762N_20_AP */	0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)102, new(IC.AMMO,                  31,         CURS.INVALIDCURS,   0,              1,       2,     1,  6,        20,   6,		/* CLIP762N_5_HP */		0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)103, new(IC.AMMO,                  32,         CURS.INVALIDCURS,   0,              1,      28,     8,  4,        80,   5,		/* CLIP762N_20_HP */	0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)104, new(IC.AMMO,                  33,         CURS.INVALIDCURS,   0,              1,      31,     5,  4,       700,   8,		/* CLIP47_50_SAP */		0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)105, new(IC.AMMO,                  34,         CURS.INVALIDCURS,   0,              1,      20,     9 , 4,       750,   9,		/* CLIP57_50_SAP */		0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)106, new(IC.AMMO,                  35,         CURS.INVALIDCURS,   0,              1,      21,     9,  4,       500,   9,		/* CLIP57_50_HP */		0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)107, new(IC.AMMO,                  37,         CURS.INVALIDCURS,   0,              2,      22,     5,  6,        20,   3,		/* CLIP12G_7 */				0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)108, new(IC.AMMO,                  36,         CURS.INVALIDCURS,   0,              2,       4,     5,  6,        20,   3,		/* CLIP12G_7_BUCKSHOT */	0,      0,  ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)109, new(IC.AMMO,                  39,         CURS.INVALIDCURS,   0,              1,       0,  10,    6,       300,   9,		/* CLIPCAWS_10_SAP */	0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)110, new(IC.AMMO,                  38,         CURS.INVALIDCURS,   0,              1,       1,  10,    6,       300,   9,		/* CLIPCAWS_10_FLECH */		0,      0,  ItemAttributes.IF_STANDARD_CLIP) },

        { (Items)111, new(IC.AMMO,                  40,         CURS.INVALIDCURS,   0,              1,      110, 10,    4,       500,   9,		/* CLIPROCKET_AP */		0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)112, new(IC.AMMO,                  41,         CURS.INVALIDCURS,   0,              1,      115, 10,    4,       500,   9,		/* CLIPROCKET_HE */		0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)113, new(IC.AMMO,                  42,         CURS.INVALIDCURS,   0,              1,      114, 10,    4,       500,   9,		/* CLIPROCKET_HEAT */	0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)114, new(IC.AMMO,                  43,         CURS.INVALIDCURS,   0,              1,      119,    1,  8,        10,   4,		/* sleep dart */			0,      0,          ItemAttributes.IF_STANDARD_CLIP) },
        { (Items)115, new(IC.AMMO,                  44,         CURS.INVALIDCURS,   0,              0,      49,     8,  4,         0,   0,		/* flameThrwr clip */	0,      0,          ItemAttributes.IF_STANDARD_CLIP | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)116, new(IC.NONE,                  0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)117, new(IC.NONE,                  0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)118, new(IC.NONE,                  0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)119, new(IC.NONE,                  0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)120, new(IC.NONE,                  0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },

        { (Items)121, new(IC.NONE,                  0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)122, new(IC.NONE,                  0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)123, new(IC.NONE,                  0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)124, new(IC.NONE,                  0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)125, new(IC.NONE,                  0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)126, new(IC.NONE,                  0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)127, new(IC.NONE,                  0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)128, new(IC.NONE,                  0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)129, new(IC.NONE,                  0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)130, new(IC.NONE,                  0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },

        //---EXPLOSIVES---

        { (Items)131, new(IC.GRENADE,               0,          CURS.TOSSCURS,          0,              1,      38,     6,  4,       100,   6,		/* stun grenade				*/	0,      -2,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_REPAIRABLE) },
        { (Items)132, new(IC.GRENADE,               1,          CURS.TOSSCURS,          0,              1,      48,     6,  4,       120,   5,		/* tear gas grenade   */	0,      -2,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_REPAIRABLE) },
        { (Items)133, new(IC.GRENADE,               2,          CURS.TOSSCURS,          0,              1,      41,     6,  4,       500,   8, 		/* mustard gas grenade*/	0,      -3,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_REPAIRABLE) },
        { (Items)134, new(IC.GRENADE,               3,          CURS.TOSSCURS,          0,              1,      50,     3,  6,       150,   6,		/* mini hand grenade  */	0,      -4,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_REPAIRABLE) },
        { (Items)135, new(IC.GRENADE,               4,          CURS.TOSSCURS,          0,              1,      49,     6,  4,       200,   7,		/* reg hand grenade   */	0,      -4,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_REPAIRABLE) },
        { (Items)136, new(IC.BOMB,                  5,          CURS.INVALIDCURS,   0,              2,       3,  11,    2,       400,   7,		/* RDX                */	0,      -4,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE) },
        { (Items)137, new(IC.BOMB,                  6,          CURS.INVALIDCURS,   0,              2,       0,  11,    1,       500,   6,		/* TNT (="explosives")*/	0,      -4,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE) },
        { (Items)138, new(IC.BOMB,                  7,          CURS.INVALIDCURS,   0,              2,      23,  11,    1,      1000,   8,		/* HMX (=RDX+TNT)     */	0,      -4,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE) },
        { (Items)139, new(IC.BOMB,                  8,          CURS.INVALIDCURS,   0,              1,      45,  11,    1,       750,   7,		/* C1  (=RDX+min oil) */	0,      -4,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE) },
        { (Items)140, new(IC.BOMB,                  9,          CURS.INVALIDCURS,   0,              1,      40,  41,    2,       400,   9,		/* mortar shell       */	0,      -4,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_REPAIRABLE) },

        { (Items)141, new(IC.BOMB,                  10,         CURS.BOMBCURS        ,  0,              1,      46,     8,  1,       300,   5,		/* mine               */	0,      -4,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_REPAIRABLE) },
        { (Items)142, new(IC.BOMB,                  11,         CURS.INVALIDCURS,   0,              1,      44,  11,    1,      1500,   9,		/* C4  ("plastique")  */	0,      -4,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE) },
        { (Items)143, new(IC.BOMB,                  12,         CURS.BOMBCURS,          0,              1,      42,     4,  2,         0,   0,		/* trip flare				  */	0,      -2,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_REPAIRABLE) },
        { (Items)144, new(IC.BOMB,                  13,         CURS.BOMBCURS,          0,              1,      43,     4,  2,         0,   0,		/* trip klaxon        */	0,      -2,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_REPAIRABLE) },
        { (Items)145, new(IC.BOMB,                  14,         CURS.INVALIDCURS,   0,              1,      107,    2,  4,       250,   6,		/* shaped charge			*/	0,      -4,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_REPAIRABLE) },
        { (Items)146, new(IC.GRENADE,               15,         CURS.TOSSCURS,          0,              2,      24,     1,  6,        50,   3,		/* break light (flare)*/	0,      0,          ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE) },
        { (Items)147, new(IC.GRENADE,               16,         CURS.INVALIDCURS,   0,              1,      97,  10,    4,       400,   8,		/* 40mm HE grenade		*/	0,      -4,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_REPAIRABLE) },
        { (Items)148, new(IC.GRENADE,               17,         CURS.INVALIDCURS,   0,              1,      111, 10,    4,       250,   6,		/* 40mm tear gas grnd */	0,      -2,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_REPAIRABLE) },
        { (Items)149, new(IC.GRENADE,               18,         CURS.INVALIDCURS,   0,              1,      113, 10,    4,       200,   5,		/* 40mm stun grenade  */	0,      -2,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_REPAIRABLE) },
        { (Items)150, new(IC.GRENADE,               19,         CURS.INVALIDCURS,   0,              1,      112, 10,    4,       100,   7,		/* 40mm smoke grenade */	0,      -2,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_REPAIRABLE) },

        { (Items)151, new(IC.GRENADE,               20,         CURS.TOSSCURS,          0,              1,      98,     6,  4,        50,   4,		/* smoke hand grenade */	0,      -2,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_REPAIRABLE) },
        { (Items)152, new(IC.BOMB,                  21,         CURS.INVALIDCURS,   0,              1,      40,  41,    8,       450,   0,		/* tank shell       */		0,      -4,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_NOT_EDITOR) },
        { (Items)153, new(IC.BOMB,                  22,         CURS.INVALIDCURS,   0,              1,      40,  41,    2,       450,   0,		/* fake struct ignite*/		0,      -4,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)154, new(IC.GRENADE,               23,         CURS.TOSSCURS,          0,              2,      37,     6,  4,        50,   0,		/* creature cocktail*/		0,      0,          ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL) },
        { (Items)155, new(IC.BOMB,                  24,         CURS.INVALIDCURS,   0,              1,      40,  41,    2,       450,   0,		/* fake struct xplod*/		0,      -4,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)156, new(IC.BOMB,                  25,         CURS.INVALIDCURS,   0,              1,      40,  41,    2,       450,   0,		/* fake vehicle xplod*/		0,      -4,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)157, new(IC.GRENADE,               26,         CURS.TOSSCURS,          0,              1,      48,     6,  4,         0,   0,		/* BIG tear gas grenade*/	0,      -2,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)158, new(IC.GRENADE,               27,         CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* small creature gas */	0,      0,          0) },
        { (Items)159, new(IC.GRENADE,               28,         CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* big creature gas */		0,      0,          0) },
        { (Items)160, new(IC.GRENADE,               29,         CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* very sm creat gas */	  0,        0,          0) },

        //---ARMOUR---

        { (Items)161, new(IC.ARMOUR,             0,         CURS.INVALIDCURS,   COND,           1,      66,     20, 0,       300, 2,		/* Flak jacket     */ 0,        +2,         ItemAttributes.IF_STANDARD_ARMOUR) },
        { (Items)162, new(IC.ARMOUR,             1,         CURS.INVALIDCURS,   COND,           2,      18,     22, 0,       350, 0,		/* Flak jacket w X */ 0,        +1,         ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)163, new(IC.ARMOUR,             2,         CURS.INVALIDCURS,   COND,           2,      11,     18, 0,       400, 0,		/* Flak jacket w Y */ 0,        +3,         ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)164, new(IC.ARMOUR,             3,         CURS.INVALIDCURS,   COND,           1,      64,     32, 0,       500, 4,		/* Kevlar jacket   */ 0,        0,          ItemAttributes.IF_STANDARD_ARMOUR) },
        { (Items)165, new(IC.ARMOUR,             4,         CURS.INVALIDCURS,   COND,           2,      16,     35, 0,       600, 0,		/* Kevlar jack w X */ 0,        -1,         ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)166, new(IC.ARMOUR,             5,         CURS.INVALIDCURS,   COND,           2,       9,     29, 0,       700, 0,		/* Kevlar jack w Y */ 0,        +1,         ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)167, new(IC.ARMOUR,             6,         CURS.INVALIDCURS,   COND,           1,      65,     32, 0,      1000, 8,		/* Spectra jacket  */ 0,        -2,         ItemAttributes.IF_STANDARD_ARMOUR) },
        { (Items)168, new(IC.ARMOUR,             7,         CURS.INVALIDCURS,   COND,           2,      17,     35, 0,      1100, 0,		/* Spectra jack w X*/ 0,        -3,         ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)169, new(IC.ARMOUR,             8,         CURS.INVALIDCURS,   COND,           2,      10,     29, 0,      1200, 0,		/* Spectra jack w Y*/ 0,        -1,         ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)170, new(IC.ARMOUR,             9,         CURS.INVALIDCURS,   COND,           1,      67,     39, 0,       650,   5,		/* Kevlar leggings */	0,      0,          ItemAttributes.IF_STANDARD_ARMOUR) },

        { (Items)171, new(IC.ARMOUR,            10,         CURS.INVALIDCURS,   COND,           2,      19,     43, 0,       800,   0,		/* Kevlar legs w X */	0,      -1,         ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)172, new(IC.ARMOUR,            11,         CURS.INVALIDCURS,   COND,           2,      12,     35, 0,       950,   0,		/* Kevlar legs w Y */	0,      +1,         ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)173, new(IC.ARMOUR,            12,         CURS.INVALIDCURS,   COND,           1,      68,     39, 0,       900,   8,		/* Spectra leggings*/	0,      -2,         ItemAttributes.IF_STANDARD_ARMOUR) },
        { (Items)174, new(IC.ARMOUR,            13,         CURS.INVALIDCURS,   COND,           2,      20,     43, 0,      1100,   0,		/* Spectra legs w X*/	0,      -3,         ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)175, new(IC.ARMOUR,            14,         CURS.INVALIDCURS,   COND,           2,      13,     35, 0,      1300,   0,		/* Spectra legs w Y*/	0,      -1,         ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)176, new(IC.ARMOUR,            15,         CURS.INVALIDCURS,   COND,           1,      61,     14, 0,        50,   2,		/* Steel helmet    */	0,      +2,         ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_METAL) },
        { (Items)177, new(IC.ARMOUR,            16,         CURS.INVALIDCURS,   COND,           1,      63,     14, 0,       200,   4,		/* Kevlar helmet   */	0,      0,          ItemAttributes.IF_STANDARD_ARMOUR) },
        { (Items)178, new(IC.ARMOUR,            17,         CURS.INVALIDCURS,   COND,           2,      15,     15, 0,       250,   0,		/* Kevlar helm w X */	0,      -1,         ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)179, new(IC.ARMOUR,            18,         CURS.INVALIDCURS,   COND,           2,       8,     13, 0,       300,   0,		/* Kevlar helm w Y */	0,      +1,         ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)180, new(IC.ARMOUR,            19,         CURS.INVALIDCURS,   COND,           1,      62,     14, 0,       450,   7,		/* Spectra helmet  */	0,      -2,         ItemAttributes.IF_STANDARD_ARMOUR) },

        { (Items)181, new(IC.ARMOUR,            20,         CURS.INVALIDCURS,   COND,           2,      14,     15, 0,       550,   0,		/* Spectra helm w X*/	0,      -3,         ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)182, new(IC.ARMOUR,            21,         CURS.INVALIDCURS,   COND,           2,       7,     13, 0,       650,   0,		/* Spectra helm w Y*/	0,      -1,         ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)183, new(IC.ARMOUR,            22,         CURS.INVALIDCURS,   COND,           1,      81,     12, 2,       250,   5,		/* Ceramic plates  */ 0,        -4,         (ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_ATTACHMENT) & (~ItemAttributes.ITEM_REPAIRABLE)) },
        { (Items)184, new(IC.ARMOUR,            23,         CURS.INVALIDCURS,   COND,           1,      0,      0,  0,           0, 0,		/* Infant crt hide */ 0,        0,          ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_DEFAULT_UNDROPPABLE) },
        { (Items)185, new(IC.ARMOUR,            24,         CURS.INVALIDCURS,   COND,           1,      0,      0,  0,           0, 0,		/* Yng male hide */		0,      0,          ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_DEFAULT_UNDROPPABLE) },
        { (Items)186, new(IC.ARMOUR,            25,         CURS.INVALIDCURS,   COND,           1,      0,      0,  0,           0, 0,		/* Old male hide */		0,      0,          ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_DEFAULT_UNDROPPABLE) },
        { (Items)187, new(IC.ARMOUR,            26,         CURS.INVALIDCURS,   COND,           1,      0,      0,  0,           0, 0,		/* Queen cret hide */ 0,        0,          ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_DEFAULT_UNDROPPABLE) },
        { (Items)188, new(IC.ARMOUR,            27,         CURS.INVALIDCURS,   COND,           1,      96,     20, 0,       200,   2,		/* Leather jacket  */ 0,        +4,         ItemAttributes.IF_STANDARD_ARMOUR) },
        //NOTE: THE FOLLOWING ITEM'S PRICE VALUE IS IN DIALOGUE AND SHOULD NOT BE CHANG) ED
        { (Items)189, new(IC.ARMOUR,            28,         CURS.INVALIDCURS,   COND,           1,      116,    20, 0,       950,   0,		/* L jacket w kev  */ 0,        +2,         ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)190, new(IC.ARMOUR,            29,         CURS.INVALIDCURS,   COND,           1,      117,    20, 0,      1200,   0,		/* L jacket w kev 18*/0,        +1,         ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_NOT_BUYABLE) },

        { (Items)191, new(IC.ARMOUR,            30,         CURS.INVALIDCURS,   COND,           1,      118,    20, 0,      1500,   0,		/* L jacket w kev c*/ 0,        +3,         ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)192, new(IC.ARMOUR,            31,         CURS.INVALIDCURS,   COND,           1,      0,      0,  0,         0,   0,		/* yng fem hide */		0,      0,          ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_DEFAULT_UNDROPPABLE) },
        { (Items)193, new(IC.ARMOUR,            32,         CURS.INVALIDCURS,   COND,           1,      0,      0,  0,         0,   0,		/* old fem hide */		0,      0,          ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_DEFAULT_UNDROPPABLE) },
        { (Items)194, new(IC.ARMOUR,            33,         CURS.INVALIDCURS,   COND,           2,      25,     3,  1,        10,   1,		/* t-shirt */					0,      0,          ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_SHOW_STATUS | ItemAttributes.ITEM_UNAERODYNAMIC) },
        { (Items)195, new(IC.ARMOUR,            33,         CURS.INVALIDCURS,   COND,           2,      34,     3,  1,        10,   1,		/* t-shirt D. rules*/	0,      0,          ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_SHOW_STATUS | ItemAttributes.ITEM_UNAERODYNAMIC) },
        { (Items)196, new(IC.ARMOUR,            34,         CURS.INVALIDCURS,   COND,           1,      137,        32, 0,       700, 6,		/* Kevlar2 jacket  */ 0,        -1,         ItemAttributes.IF_STANDARD_ARMOUR) },
        { (Items)197, new(IC.ARMOUR,            35,         CURS.INVALIDCURS,   COND,           2,      40,     35, 0,       800, 0,		/* Kevlar2 jack w X*/ 0,        -2,         ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)198, new(IC.ARMOUR,            36,         CURS.INVALIDCURS,   COND,           2,      41,     29, 0,       900, 0,		/* Kevlar2 jack w Y*/ 0,        0,          ItemAttributes.IF_STANDARD_ARMOUR | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)199, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)200, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },

        //---MISC---
        { (Items)201, new(IC.MEDKIT,                0,          CURS.AIDCURS,           USAGE,    1,        73,     5,  4,       100,   1,		/* First aid kit */		0,      0,          ItemAttributes.IF_STANDARD_KIT) },
        { (Items)202, new(IC.MEDKIT,                0,          CURS.AIDCURS,           USAGE,    1,        86,     18, 0,       300,   1,		/* Medical Kit   */		0,      0,          ItemAttributes.IF_STANDARD_KIT | ItemAttributes.ITEM_METAL) },
        { (Items)203, new(IC.KIT,                   0,          CURS.REPAIRCURS,        COND,           2,      21,     50, 0,       250,   1,		/* Tool Kit	     */		0,      0,          ItemAttributes.IF_STANDARD_KIT | ItemAttributes.ITEM_METAL) },
        { (Items)204, new(IC.KIT,                   0,          CURS.INVALIDCURS,   COND,           1,      78,     3,  1,       250,   3,	  /* Locksmith kit */		0,      -2,         ItemAttributes.IF_STANDARD_KIT | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_REPAIRABLE) },
        { (Items)205, new(IC.KIT,                   0,          CURS.INVALIDCURS,   COND,           1,      58,     1,  4,       250,   5,	  /* Camouflage kit*/		0,      0,          ItemAttributes.IF_STANDARD_KIT) },
        { (Items)206, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)207, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,      52,     5,  4,       300,   5,		/* Silencer      */		0,      0,          ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_ATTACHMENT) },
        { (Items)208, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,      76,     9,  4,       500,   6,		/* Sniper scope  */		0,      0,          ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_ATTACHMENT) },
        { (Items)209, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,      69,     5,  2,        50,   4,		/* Bipod         */		0,      +5,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_ATTACHMENT) },
        { (Items)210, new(IC.FACE,              0,          CURS.INVALIDCURS,   0,              1,      77,     9,  1,       400,   7,		/* Extended ear	 */		0,      -3,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_ELECTRONIC) },

        { (Items)211, new(IC.FACE,              0,          CURS.INVALIDCURS,   0,              1,      74,     9,  1,       800,   7,		/* Night goggles */		0,      -1,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_ELECTRONIC) },
        { (Items)212, new(IC.FACE,              0,          CURS.INVALIDCURS,   0,              1,      55,     2,  4,       150,   3,		/* Sun goggles	 */		0,      +3,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE) },
        { (Items)213, new(IC.FACE,              0,          CURS.INVALIDCURS,   0,              1,    75,       9,  1,       100,   4,		/* Gas mask   	 */		0,      +1,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE) },
        { (Items)214, new(IC.KIT,                   0,          CURS.INVALIDCURS,   0,              2,       5,  10,    4,        10,   1,		/* Canteen       */		0,      0,          ItemAttributes.ITEM_DAMAGEABLE) },
        { (Items)215, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,      80,  10,    1,       200,   4,		/* Metal detector*/		0,      -2,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_ELECTRONIC) },
        { (Items)216, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              2,       6,     1,  4,       900,   7,		/* Compound 18	 */		0,      0,          ItemAttributes.ITEM_DAMAGEABLE) },
        { (Items)217, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              2,      42,     1,  4,      2500,   0,		/* Jar/QueenBlood*/		0,      0,          ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)218, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              2,       2,     1,  4,       500,   1,		/* Jar/Elixir    */		0,      0,          ItemAttributes.ITEM_DAMAGEABLE) },
        { (Items)219, new(IC.MONEY,             0,          CURS.INVALIDCURS,   0,              2,       1,     1,  1,         0,   0,		/* Money         */		0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_UNAERODYNAMIC) },
        { (Items)220, new(IC.MISC,              0,          CURS.JARCURS,           COND,           1,      71,     1,  2,          10, 1,		/* Glass jar		 */		0,      0,          ItemAttributes.ITEM_DAMAGEABLE) },

        { (Items)221, new(IC.MISC,              0,          CURS.INVALIDCURS,   COND,           1,      72,     5,  2,          50, 1,		/* Jar/CreatureBlood*/0,        0,          ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)222, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,      70,     1,  8,       150,   4,		/* Adren Booster */		0,      0,          ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_UNAERODYNAMIC) },
        { (Items)223, new(IC.MISC,              0,          CURS.INVALIDCURS,   COND,           1,      47,     1,  4,       100, 3,		/* Detonator     */		0,      +1,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_ATTACHMENT | ItemAttributes.ITEM_METAL) },
        { (Items)224, new(IC.MISC,              0,          CURS.INVALIDCURS,   COND,           1,      47,     1,  4,       200, 6,		/* Rem Detonator */		0,      -1,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_ATTACHMENT | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_ELECTRONIC) },
        { (Items)225, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,      93,     1,  8,         0,   0,		/* Videotape     */		0,      0,          ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)226, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,      94,     1,  8,         0,   0,		/* Deed          */		0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_UNAERODYNAMIC) },
        { (Items)227, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,      56,     1,  1,         0,   0,		/* Letter				 */		0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_UNAERODYNAMIC) },
        { (Items)228, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,      59,     1,  1,         0,   0,		/* Diskette	  	 */		0,      0,          ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)229, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,  124,        0,  1,      3000,   0,		/* Chalice	     */		0,      0,          ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)230, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,  120,        1,  4,        50,   0,		/* Bloodcat claws*/		0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_DAMAGEABLE) },

        { (Items)231, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,  121,        1,  4,       100,   0,		/* Bloodcat teeth*/		0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_DAMAGEABLE) },
        { (Items)232, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              3,      9,   60,    0,       400, 0,		/* Bloodcat pelt */		0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_DAMAGEABLE) },
        { (Items)233, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,      54,     0,  99,        0,   0,		/* Switch        */		0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_METAL) },
        { (Items)234, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,      47,     0,  99,        0,   0,		/* Action item   */		0,      0,          ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)235, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,      70,     1,  6,       300,   6,		/* Regen Booster */		0,      0,          ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_UNAERODYNAMIC) },
        { (Items)236, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,      70,     0,  99,        0,   0,		/* syringe 3     */		0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_UNAERODYNAMIC) },
        { (Items)237, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,      70,     0,  99,        0,   0,		/* syringe 4     */		0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_UNAERODYNAMIC) },
        { (Items)238, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,      70,     0,  99,        0,   0,		/* syringe 5     */		0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_UNAERODYNAMIC) },
        { (Items)239, new(IC.MISC,              0,          CURS.INVALIDCURS,   COND,           1,      72,     5,  2,          10, 1,		/* Jar/Human Blood*/	0,      0,          ItemAttributes.ITEM_DAMAGEABLE) },
        { (Items)240, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,      0,      0,  0,         0,   0,		/* ownership     */		0,      0,          ItemAttributes.ITEM_NOT_BUYABLE) },

        { (Items)241, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,      51,     4,  4,       750,   8,		/* Laser scope   */		0,      -1,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_ATTACHMENT | ItemAttributes.ITEM_ELECTRONIC) },
        { (Items)242, new(IC.MISC,              0,          CURS.REMOTECURS,        0,              1,      54,     9,  4,       400,   6,		/* Remote bomb trig*/	0,      -2,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_ELECTRONIC) },
        { (Items)243, new(IC.MISC,              0,          CURS.WIRECUTCURS,   0,              1,      88,     4,  2,        20,   2,		/* Wirecutters   */		0,      -4,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_METAL) },
        { (Items)244, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,      90,     9,  4,        30,   2,		/* Duckbill      */		0,      +5,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_ATTACHMENT) },
        { (Items)245, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,      106,    20, 1,        30,   1,		/* Alcohol  */				0,      0,          ItemAttributes.ITEM_DAMAGEABLE) },
        { (Items)246, new(IC.FACE,              0,          CURS.INVALIDCURS,   0,              1,      74,     11, 1,      1500,   10,		/* UV goggles */			0,      -1,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_ELECTRONIC) },
        { (Items)247, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              0,      44,     21, 0,        30,   0,		/* discarded LAW*/		0,      0,          ItemAttributes.IF_TWOHANDED_GUN | ItemAttributes.ITEM_NOT_BUYABLE) },
        { (Items)248, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              3,      0,      40, 0,         0,   0,		/* head - generic */	0,      0,          ItemAttributes.ITEM_DAMAGEABLE) },
        { (Items)249, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              3,      1,      40, 0,         0,   0,		/* head - Imposter*/	0,      0,          ItemAttributes.ITEM_DAMAGEABLE) },
        { (Items)250, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              3,      2,      40, 0,         0,   0,		/* head - T-Rex */		0,      0,          ItemAttributes.ITEM_DAMAGEABLE) },

        { (Items)251, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              3,      3,      40, 0,         0,   0,		/* head - Slay */			0,      0,          ItemAttributes.ITEM_DAMAGEABLE) },
        { (Items)252, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              3,      4,      40, 0,         0,   0,		/* head - Druggist */	0,      0,          ItemAttributes.ITEM_DAMAGEABLE) },
        { (Items)253, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              3,      5,      40, 0,         0,   0,		/* head - Matron */		0,      0,          ItemAttributes.ITEM_DAMAGEABLE) },
        { (Items)254, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              3,      6,      40, 0,         0,   0,		/* head - Tiffany */	0,      0,          ItemAttributes.ITEM_DAMAGEABLE) },
        { (Items)255, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,      100,    12, 1,        20,   1,		/* wine     */				0,      0,          ItemAttributes.ITEM_DAMAGEABLE) },
        { (Items)256, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,      101,    4,  4,        10,   1,		/* beer		  */				0,      0,          ItemAttributes.ITEM_DAMAGEABLE) },
        { (Items)257, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,      99,     0,  2,        20,   3,		/* pornos   */				0,      0,          ItemAttributes.ITEM_DAMAGEABLE) },
        { (Items)258, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              0,      43,     20, 0,       900,   6,		/* video camera */		0,      -4,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_ELECTRONIC ) },
        { (Items)259, new(IC.FACE,              0,          CURS.INVALIDCURS,   0,              0,      42,     5,  1,      2500,   0,		/* robot remote */		0,      -5,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_ELECTRONIC ) },
        { (Items)260, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,      103, 20,    0,       500,   0,		/* creature claws */	0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_DAMAGEABLE) },

        { (Items)261, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              3,      7,   40,    0,       250,   0,		/* creature flesh */	0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_DAMAGEABLE) },
        { (Items)262, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,      104, 10,    0,      1000,   0,		/* creature organ */	0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_DAMAGEABLE) },
        { (Items)263, new(IC.MISC,              0,          CURS.REMOTECURS,        0,              1,      54,     9,  4,       400,   6,		/* Remote trigger*/		0,      -2,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_METAL) },
        { (Items)264, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              0,      47,     2,  8,       500,   2,		/* gold watch */			0,      -4,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_METAL) },
        { (Items)265, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,      136, 100,   0,       200,   2,		/* golf clubs */			0,      0,          ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_METAL) },
        { (Items)266, new(IC.FACE,              0,          CURS.INVALIDCURS,   0,              3,      11,     5,  1,       100,   1,		/* walkman */					0,      -4,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_ELECTRONIC) },
        { (Items)267, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              3,      8,   50,    0,       300,   2,		/* portable tv */			0,      -3,         ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_ELECTRONIC) },
        { (Items)268, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)269, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              3,      10,  10,    1,        30,   1,		/* cigars */					0,      0,          ItemAttributes.ITEM_DAMAGEABLE) },
        { (Items)270, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },

        { (Items)271, new(IC.KEY,                   0,          CURS.INVALIDCURS,   0,              1,      82,     1,  8,         0,   0,		/* dull gold key */		0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_METAL) },
        { (Items)272, new(IC.KEY,                   1,          CURS.INVALIDCURS,   0,              1,      83,     1,  8,         0,   0,		/* silver key */			0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_METAL) },
        { (Items)273, new(IC.KEY,                   2,          CURS.INVALIDCURS,   0,              1,      84,     1,  8,         0,   0,		/* diamond-shpd key */0,        0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_METAL) },
        { (Items)274, new(IC.KEY,                   3,          CURS.INVALIDCURS,   0,              1,      87,     1,  8,         0,   0,		/* bright gold key */	0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_METAL) },
        { (Items)275, new(IC.KEY,                   4,          CURS.INVALIDCURS,   0,              1,      91,     1,  8,         0,   0,		/* gold key */				0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_METAL) },
        { (Items)276, new(IC.KEY,                   5,          CURS.INVALIDCURS,   0,              1,      92,     1,  8,         0,   0,		/* small gold key */	0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_METAL) },
        { (Items)277, new(IC.KEY,                   6,          CURS.INVALIDCURS,   0,              1,      108,    1,  8,         0,   0,		/* electronic */			0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_ELECTRONIC) },
        { (Items)278, new(IC.KEY,                   7,          CURS.INVALIDCURS,   0,              1,      109,    1,  8,         0,   0,		/* passcard       */	0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_METAL) },
        { (Items)279, new(IC.KEY,                   8,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* key      */				0,      0,          ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_METAL) },
        { (Items)280, new(IC.KEY,                   9,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* key      */				0,      0,          ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_METAL) },

        { (Items)281, new(IC.KEY,                   10,         CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* key      */				0,      0,          ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_METAL) },
        { (Items)282, new(IC.KEY,                   11,         CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* key      */				0,      0,          ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_METAL) },
        { (Items)283, new(IC.KEY,                   12,         CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* key      */				0,      0,          ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_METAL) },
        { (Items)284, new(IC.KEY,                   13,         CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* key      */				0,      0,          ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_METAL) },
        { (Items)285, new(IC.KEY,                   14,         CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* key      */				0,      0,          ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_METAL) },
        { (Items)286, new(IC.KEY,                   15,         CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* key      */				0,      0,          ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_METAL) },
        { (Items)287, new(IC.KEY,                   16,         CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* key      */				0,      0,          ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_METAL) },
        { (Items)288, new(IC.KEY,                   17,         CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* key      */				0,      0,          ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_METAL) },
        { (Items)289, new(IC.KEY,                   18,         CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* key      */				0,      0,          ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_METAL) },
        { (Items)290, new(IC.KEY,                   19,         CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* key      */				0,      0,          ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_METAL) },

        { (Items)291, new(IC.KEY,                   20,         CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* key      */				0,      0,          ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_METAL) },
        { (Items)292, new(IC.KEY,                   21,         CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* key      */				0,      0,          ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_METAL) },
        { (Items)293, new(IC.KEY,                   22,         CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* key      */				0,      0,          ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_METAL) },
        { (Items)294, new(IC.KEY,                   23,         CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* key      */				0,      0,          ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_METAL) },
        { (Items)295, new(IC.KEY,                   24,         CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* key      */				0,      0,          ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_METAL) },
        { (Items)296, new(IC.KEY,                   25,         CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* key      */				0,      0,          ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_METAL) },
        { (Items)297, new(IC.KEY,                   26,         CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* key      */				0,      0,          ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_METAL) },
        { (Items)298, new(IC.KEY,                   27,         CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* key      */				0,      0,          ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_METAL) },
        { (Items)299, new(IC.KEY,                   28,         CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* key      */				0,      0,          ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_METAL) },
        { (Items)300, new(IC.KEY,                   29,         CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* key      */				0,      0,          ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_METAL) },

        { (Items)301, new(IC.KEY,                   30,         CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* key      */				0,      0,          ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_METAL) },
        { (Items)302, new(IC.KEY,                   31,         CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* key      */				0,      0,          ItemAttributes.ITEM_NOT_EDITOR | ItemAttributes.ITEM_METAL) },
        { (Items)303, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              2,      31,     4,  0,       150,   2,		/* silver platter */	0,      0,          ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL) },
        { (Items)304, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              2,      30,     1,  6,         5,   1,		/* duct tape */				0,      0,          ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_HIDDEN_ADDON) },
        { (Items)305, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              2,      28,     3,  1,        20,   0,		/* aluminum rod */		0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_HIDDEN_ADDON) },
        { (Items)306, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              2,      32,     1,  8,         0,   0,		/* spring */					0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_UNAERODYNAMIC | ItemAttributes.ITEM_HIDDEN_ADDON) },
        { (Items)307, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              2,      33,     4,  1,        25,   0,		/* a. rod & spring */	0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_INSEPARABLE) },
        { (Items)308, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,   133,       4,  1,        20,   0,		/* steel rod */				0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_HIDDEN_ADDON) },
        { (Items)309, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,   132,       2,  6,         5,   3,		/* quick glue */			0,      0,          ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_HIDDEN_ADDON) },
        { (Items)310, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,   134,       6,  1,       150,   0,		/* gun barrel xtndr */0,        0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_INSEPARABLE) },

        { (Items)311, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              2,      35,     1,  8,         0,   0,		/* string */					0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_UNAERODYNAMIC | ItemAttributes.ITEM_HIDDEN_ADDON) },
        { (Items)312, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              2,      27,     1,  1,         0,   0,		/* tin can */					0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_UNAERODYNAMIC | ItemAttributes.ITEM_HIDDEN_ADDON) },
        { (Items)313, new(IC.MISC,              0,          CURS.TINCANCURS,        0,              2,      36,     2,  4,         0,   0,		/* string & tin can */0,        0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_UNAERODYNAMIC) },
        { (Items)314, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,  122,        3,  6,         5,   0,		/* marbles */					0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_DAMAGEABLE) },
        { (Items)315, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,  127,        6,  1,       200,   6,		/* lame boy */				0,      0,          ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_ELECTRONIC | ItemAttributes.ITEM_HIDDEN_ADDON) },
        { (Items)316, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,  128,        1,  8,         5,   1,		/* copper wire */			0,      0,          ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_HIDDEN_ADDON) },
        { (Items)317, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,  126,        7,  1,        50,   0,		/* display unit */		0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_ELECTRONIC | ItemAttributes.ITEM_HIDDEN_ADDON) },
        { (Items)318, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,  130,        1,  1,       100,   5,		/* fumble pak */			0,      0,          ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_ELECTRONIC) },
        { (Items)319, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,  131,        1,  2,        10,   5,		/* xray bulb */				0,      0,          ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_HIDDEN_ADDON) },
        { (Items)320, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              2,   29,        1,  8,         1,   1,		/* chewing gum */			0,      0,          ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_HIDDEN_ADDON) },

        { (Items)321, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,  129,        3,  1,       100,   0,		/* flash device */		0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_ELECTRONIC | ItemAttributes.ITEM_HIDDEN_ADDON) },
        { (Items)322, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              2,   26,        1,  6,         5,   1,		/* batteries */				0,      0,          ItemAttributes.ITEM_DAMAGEABLE) },
        { (Items)323, new(IC.MISC,              0,          CURS.INVALIDCURS,   0,              1,  123,        1,  8,         0,   0,		/* elastic */					0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_UNAERODYNAMIC) },
        { (Items)324, new(IC.MISC,              0,          CURS.REMOTECURS,        0,              1,  125,     10,    1,      2500,   0,		/* xray device */			0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_DAMAGEABLE | ItemAttributes.ITEM_REPAIRABLE | ItemAttributes.ITEM_METAL | ItemAttributes.ITEM_ELECTRONIC) },
        { (Items)325, new(IC.MONEY,             0,          CURS.INVALIDCURS,   0,              2,   38,        2,  1,       100,   0,		/* silver   */				0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_DAMAGEABLE) },
        { (Items)326, new(IC.MONEY,             0,          CURS.INVALIDCURS,   0,              1,  135,        2,  1,       300,   0,		/* gold			*/				0,      0,          ItemAttributes.ITEM_NOT_BUYABLE | ItemAttributes.ITEM_DAMAGEABLE) },
        { (Items)327, new(IC.KIT,                   0,          CURS.REFUELCURS,      0,                2,   39,     20,    0,       250,   0,		/* gas can */					0,      0,          ItemAttributes.ITEM_DAMAGEABLE) },
        { (Items)328, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)329, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)330, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },

        { (Items)331, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)332, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)333, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)334, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)335, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)336, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)337, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)338, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)339, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)340, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },

        { (Items)341, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)342, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)343, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)344, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)345, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)346, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)347, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)348, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)349, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
        { (Items)350, new(IC.NONE,              0,          CURS.INVALIDCURS,   0,              0,      0,      0,  0,         0,   0,		/* nothing! */				0,      0,          0) },
    };
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
