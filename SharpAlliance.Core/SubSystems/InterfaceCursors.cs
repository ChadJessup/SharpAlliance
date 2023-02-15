using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.SubSystems;

public class InterfaceCursors
{
    private readonly ILogger<InterfaceCursors> logger;
    private readonly GameSettings gGameSettings;
    private readonly IInputManager inputs;
    private readonly IClockManager clock;
    private readonly WorldManager world;
    private readonly Overhead overhead;
    private readonly Globals globals;

    public InterfaceCursors(
        ILogger<InterfaceCursors> logger,
        Globals globals,
        IClockManager clock,
        GameSettings gameSettings,
        WorldManager worldManager,
        Overhead overhead,
        IInputManager inputManager)
    {
        this.logger = logger;
        this.globals = globals;
        this.clock = clock;
        this.gGameSettings = gameSettings;
        this.world = worldManager;
        this.overhead = overhead;
        this.inputs = inputManager;
    }

    public const int DISPLAY_AP_INDEX = (int)TileDefines.MOCKFLOOR1;

    public const int SNAPCURSOR_AP_X_STARTVAL = 18;
    public const int SNAPCURSOR_AP_Y_STARTVAL = 9;

    public const int LOOSE_CURSOR_DELAY = 300;
    static bool gfLooseCursorOn = false;
    static short gsLooseCursorGridNo = IsometricUtils.NOWHERE;
    static int guiLooseCursorID = 0;
    static uint guiLooseCursorTimeOfLastUpdate = 0;

    Dictionary<UICursorDefines, UICursor> gUICursors = new()
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

    UICursorDefines guiCurUICursor = UICursorDefines.NO_UICURSOR;
    UICursorDefines guiOldUICursor = UICursorDefines.NO_UICURSOR;
    ushort gusCurMousePos;
    ushort gusTargetDropPos;
    bool gfTargetDropPos = false;

    bool SetUICursor(UICursorDefines uiNewCursor)
    {
        guiOldUICursor = guiCurUICursor;
        guiCurUICursor = uiNewCursor;

        return true;
    }

    static bool fHideCursor = false;
    bool DrawUICursor()
    {
        ushort usMapPos;
        LEVELNODE? pNode;
        TileDefines usTileCursor; // might be ushort, but testing this out.

        //RaiseMouseToLevel( (byte)gsInterfaceLevel );

        HandleLooseCursorDraw();


        // OK, WE OVERRIDE HERE CURSOR DRAWING FOR THINGS LIKE
        if (Interface.gpItemPointer != null)
        {
            this.inputs.Mouse.MSYS_ChangeRegionCursor(Interface.gViewportRegion, CURSOR.VIDEO_NO_CURSOR);

            // Check if we are in the viewport region...
            if (Interface.gViewportRegion.uiFlags.HasFlag(MouseRegionFlags.IN_AREA))
            {
                DrawItemTileCursor();
            }
            else
            {
                DrawItemFreeCursor();
            }
            return true;
        }

        if (GetMouseMapPos(out usMapPos))
        {
            gusCurMousePos = usMapPos;

            if (guiCurUICursor == UICursorDefines.NO_UICURSOR)
            {
                this.inputs.Mouse.MSYS_ChangeRegionCursor(Interface.gViewportRegion, CURSOR.VIDEO_NO_CURSOR);
                return true;
            }

            if (gUICursors[guiCurUICursor].uiFlags.HasFlag(UICURSOR.SHOWTILE))
            {
                if (Interface.gsInterfaceLevel == InterfaceLevel.I_ROOF_LEVEL)
                {
                    pNode = this.world.AddTopmostToTail(gusCurMousePos, GetSnapCursorIndex(TileDefines.FIRSTPOINTERS3));
                }
                else
                {
                    pNode = this.world.AddTopmostToTail(gusCurMousePos, GetSnapCursorIndex(gUICursors[guiCurUICursor].usAdditionalData));
                }
                pNode.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                pNode.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;

                if (Interface.gsInterfaceLevel == InterfaceLevel.I_ROOF_LEVEL)
                {
                    // Put one on the roof as well
                    this.world.AddOnRoofToHead(gusCurMousePos, GetSnapCursorIndex(gUICursors[guiCurUICursor].usAdditionalData));
                    this.globals.gpWorldLevelData[gusCurMousePos].pOnRoofHead.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                    this.globals.gpWorldLevelData[gusCurMousePos].pOnRoofHead.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                }
            }

            gfTargetDropPos = false;

            if (gUICursors[guiCurUICursor].uiFlags.HasFlag(UICURSOR.FREEFLOWING) && !(gUICursors[guiCurUICursor].uiFlags.HasFlag(UICURSOR.DONTSHOW2NDLEVEL)))
            {
                gfTargetDropPos = true;
                gusTargetDropPos = gusCurMousePos;

                if (Interface.gsInterfaceLevel == InterfaceLevel.I_ROOF_LEVEL)
                {
                    // If we are over a target, jump to that....
                    if (HandleUI.gfUIFullTargetFound)
                    {
                        gusTargetDropPos = MercPtrs[gusUIFullTargetID].sGridNo;
                    }

                    // Put tile on the floor
                    this.world.AddTopmostToTail(gusTargetDropPos, TileDefines.FIRSTPOINTERS14);
                    this.globals.gpWorldLevelData[gusTargetDropPos].pTopmostHead.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                    this.globals.gpWorldLevelData[gusTargetDropPos].pTopmostHead.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;

                }
            }

            if (gUICursors[guiCurUICursor].uiFlags.HasFlag(UICURSOR.SHOWTILEAPDEPENDENT))
            {
                // Add depending on AP status
                usTileCursor = gUICursors[guiCurUICursor].usAdditionalData;

                // ATE; Is the current guy in steath mode?
                if (this.overhead.gusSelectedSoldier != OverheadTypes.NOBODY)
                {
                    if (MercPtrs[this.overhead.gusSelectedSoldier].bStealthMode)
                    {
                        usTileCursor = TileDefines.FIRSTPOINTERS9;
                    }
                }

                if (HandleUI.gfUIDisplayActionPointsInvalid || this.globals.gsCurrentActionPoints == 0)
                {
                    usTileCursor = TileDefines.FIRSTPOINTERS6;

                    // ATE; Is the current guy in steath mode?
                    if (this.overhead.gusSelectedSoldier != OverheadTypes.NOBODY)
                    {
                        if (MercPtrs[this.overhead.gusSelectedSoldier].bStealthMode)
                        {
                            usTileCursor = TileDefines.FIRSTPOINTERS10;
                        }
                    }
                }

                if (Interface.gsInterfaceLevel == InterfaceLevel.I_ROOF_LEVEL)
                {
                    pNode = this.world.AddTopmostToTail(gusCurMousePos, GetSnapCursorIndex(TileDefines.FIRSTPOINTERS14));
                }
                else
                {
                    pNode = this.world.AddTopmostToTail(gusCurMousePos, GetSnapCursorIndex(usTileCursor));
                }

                pNode.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                pNode.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;

                if (Interface.gsInterfaceLevel == InterfaceLevel.I_ROOF_LEVEL)
                {
                    // Put one on the roof as well
                    this.world.AddOnRoofToHead(gusCurMousePos, GetSnapCursorIndex(usTileCursor));
                    this.globals.gpWorldLevelData[gusCurMousePos].pOnRoofHead.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                    this.globals.gpWorldLevelData[gusCurMousePos].pOnRoofHead.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                }
            }


            // If snapping - remove from main viewport
            if (gUICursors[guiCurUICursor].uiFlags.HasFlag(UICURSOR.SNAPPING))
            {
                // Hide mouse region cursor
                this.inputs.Mouse.MSYS_ChangeRegionCursor(Interface.gViewportRegion, CURSOR.VIDEO_NO_CURSOR);

                // Set Snapping Cursor
                DrawSnappingCursor();
            }


            if (gUICursors[guiCurUICursor].uiFlags.HasFlag(UICURSOR.FREEFLOWING))
            {
                switch (guiCurUICursor)
                {
                    case UICursorDefines.MOVE_VEHICLE_UICURSOR:

                        // Set position for APS
                        HandleUI.gfUIDisplayActionPointsCenter = false;
                        HandleUI.gUIDisplayActionPointsOffX = 16;
                        HandleUI.gUIDisplayActionPointsOffY = 14;
                        break;

                    case UICursorDefines.MOVE_WALK_UICURSOR:
                    case UICursorDefines.MOVE_RUN_UICURSOR:

                        // Set position for APS
                        HandleUI.gfUIDisplayActionPointsCenter = false;
                        HandleUI.gUIDisplayActionPointsOffX = 16;
                        HandleUI.gUIDisplayActionPointsOffY = 14;
                        break;

                    case UICursorDefines.MOVE_SWAT_UICURSOR:

                        // Set position for APS
                        HandleUI.gfUIDisplayActionPointsCenter = false;
                        HandleUI.gUIDisplayActionPointsOffX = 16;
                        HandleUI.gUIDisplayActionPointsOffY = 10;
                        break;

                    case UICursorDefines.MOVE_PRONE_UICURSOR:

                        // Set position for APS
                        HandleUI.gfUIDisplayActionPointsCenter = false;
                        HandleUI.gUIDisplayActionPointsOffX = 16;
                        HandleUI.gUIDisplayActionPointsOffY = 9;
                        break;
                }

                fHideCursor = false;

                if (!fHideCursor)
                {
                    this.inputs.Mouse.MSYS_ChangeRegionCursor(Interface.gViewportRegion, gUICursors[guiCurUICursor].usFreeCursorName);

                }
                else
                {
                    // Hide
                    this.inputs.Mouse.MSYS_ChangeRegionCursor(Interface.gViewportRegion, CURSOR.VIDEO_NO_CURSOR);
                }

            }

            if (gUICursors[guiCurUICursor].uiFlags.HasFlag(UICURSOR.CENTERAPS))
            {
                HandleUI.gfUIDisplayActionPointsCenter = true;
            }
        }
        return true;
    }

    bool HideUICursor()
    {
        HandleLooseCursorHide();

        // OK, WE OVERRIDE HERE CURSOR DRAWING FOR THINGS LIKE
        if (Interface.gpItemPointer != null)
        {
            // Check if we are in the viewport region...
            if (Interface.gViewportRegion.uiFlags.HasFlag(MouseRegionFlags.IN_AREA))
            {
                HideItemTileCursor();
                return true;
            }
        }

        if (guiCurUICursor == UICursorDefines.NO_UICURSOR)
        {
            //Do nothing here
            return true;
        }

        if (gUICursors[guiCurUICursor].uiFlags.HasFlag(UICURSOR.SHOWTILE | UICURSOR.SHOWTILEAPDEPENDENT))
        {
            this.world.RemoveAllTopmostsOfTypeRange(gusCurMousePos, TileTypeDefines.FIRSTPOINTERS, TileTypeDefines.FIRSTPOINTERS);
            this.world.RemoveAllOnRoofsOfTypeRange(gusCurMousePos, TileTypeDefines.FIRSTPOINTERS, TileTypeDefines.FIRSTPOINTERS);
        }


        if (gUICursors[guiCurUICursor].uiFlags.HasFlag(UICURSOR.FREEFLOWING) && !(gUICursors[guiCurUICursor].uiFlags.HasFlag(UICURSOR.DONTSHOW2NDLEVEL)))
        {
            if (Interface.gsInterfaceLevel == InterfaceLevel.I_ROOF_LEVEL)
            {
                this.world.RemoveTopmost(gusCurMousePos, TileDefines.FIRSTPOINTERS14);
                this.world.RemoveTopmost(gusCurMousePos, TileDefines.FIRSTPOINTERS9);

                if (gfTargetDropPos)
                {
                    this.world.RemoveTopmost(gusTargetDropPos, TileDefines.FIRSTPOINTERS14);
                    this.world.RemoveTopmost(gusTargetDropPos, TileDefines.FIRSTPOINTERS9);
                }
            }

        }


        // If snapping - remove from main viewport
        if (gUICursors[guiCurUICursor].uiFlags.HasFlag(UICURSOR.SNAPPING))
        {
            // hide Snapping Cursor
            EraseSnappingCursor();
        }

        if (gUICursors[guiCurUICursor].uiFlags.HasFlag(UICURSOR.FREEFLOWING))
        {
            // Nothing special here...
        }

        return true;
    }

    static bool fShowAP = true;
    void DrawSnappingCursor()
    {
        SOLDIERTYPE? pSoldier;

        if (this.overhead.gusSelectedSoldier != OverheadTypes.NO_SOLDIER)
        {
            this.overhead.GetSoldier(out pSoldier, this.overhead.gusSelectedSoldier);

        }

        LEVELNODE newUIElem;

        // If we are in draw item mode, do nothing here but call the fuctiuon
        switch (guiCurUICursor)
        {
            case UICursorDefines.NO_UICURSOR:
                break;

            case UICursorDefines.NORMAL_SNAPUICURSOR:

                this.world.AddTopmostToHead(gusCurMousePos, TileDefines.FIRSTPOINTERS1);
                this.globals.gpWorldLevelData[gusCurMousePos].pTopmostHead.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                this.globals.gpWorldLevelData[gusCurMousePos].pTopmostHead.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                break;

            case UICursorDefines.ALL_MOVE_RUN_UICURSOR:
            case UICursorDefines.CONFIRM_MOVE_RUN_UICURSOR:
                if (Interface.gsInterfaceLevel > 0)
                {
                    this.world.AddUIElem(gusCurMousePos, TileDefines.GOODRUN1, 0, -TileDefine.WALL_HEIGHT - 8, out newUIElem);
                }
                else
                {
                    this.world.AddUIElem(gusCurMousePos, TileDefines.GOODRUN1, 0, 0, out newUIElem);
                }

                newUIElem.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                newUIElem.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                break;

            case UICursorDefines.ALL_MOVE_WALK_UICURSOR:
            case UICursorDefines.CONFIRM_MOVE_WALK_UICURSOR:
                if (Interface.gsInterfaceLevel > 0)
                {
                    this.world.AddUIElem(gusCurMousePos, TileDefines.GOODWALK1, 0, -TileDefine.WALL_HEIGHT - 8, out newUIElem);
                }
                else
                {
                    this.world.AddUIElem(gusCurMousePos, TileDefines.GOODWALK1, 0, 0, out newUIElem);
                }

                newUIElem.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                newUIElem.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                break;

            case UICursorDefines.ALL_MOVE_SWAT_UICURSOR:
            case UICursorDefines.CONFIRM_MOVE_SWAT_UICURSOR:
                if (Interface.gsInterfaceLevel > 0)
                {
                    this.world.AddUIElem(gusCurMousePos, TileDefines.GOODSWAT1, 0, -TileDefine.WALL_HEIGHT - 8, out newUIElem);
                }
                else
                {
                    this.world.AddUIElem(gusCurMousePos, TileDefines.GOODSWAT1, 0, 0, out newUIElem);
                }

                newUIElem.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                newUIElem.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                break;

            case UICursorDefines.ALL_MOVE_PRONE_UICURSOR:
            case UICursorDefines.CONFIRM_MOVE_PRONE_UICURSOR:
                if (Interface.gsInterfaceLevel > 0)
                {
                    this.world.AddUIElem(gusCurMousePos, TileDefines.GOODPRONE1, 0, -TileDefine.WALL_HEIGHT - 8 - 6, out newUIElem);
                }
                else
                {
                    this.world.AddUIElem(gusCurMousePos, TileDefines.GOODPRONE1, 0, -6, out newUIElem);
                }

                newUIElem.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                newUIElem.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                break;

            case UICursorDefines.ALL_MOVE_VEHICLE_UICURSOR:
            case UICursorDefines.CONFIRM_MOVE_VEHICLE_UICURSOR:
                if (Interface.gsInterfaceLevel > 0)
                {
                    this.world.AddUIElem(gusCurMousePos, TileDefines.VEHICLEMOVE1, 0, -TileDefine.WALL_HEIGHT - 8, out newUIElem);
                }
                else
                {
                    this.world.AddUIElem(gusCurMousePos, TileDefines.VEHICLEMOVE1, 0, 0, out newUIElem);
                }

                newUIElem.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                newUIElem.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                break;

            case UICursorDefines.MOVE_REALTIME_UICURSOR:
                break;

            case UICursorDefines.CANNOT_MOVE_UICURSOR:

                if (Interface.gsInterfaceLevel > 0)
                {
                    this.world.AddUIElem(gusCurMousePos, TileDefines.BADMARKER1, 0, -TileDefine.WALL_HEIGHT - 8, out newUIElem);
                    newUIElem.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                    newUIElem.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;

                    if (gGameSettings.fOptions[TOPTION.CURSOR_3D])
                    {
                        this.world.AddTopmostToHead(gusCurMousePos, TileDefines.FIRSTPOINTERS13);
                        this.globals.gpWorldLevelData[gusCurMousePos].pTopmostHead.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                        this.globals.gpWorldLevelData[gusCurMousePos].pTopmostHead.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                    }

                    this.world.AddOnRoofToHead(gusCurMousePos, TileDefines.FIRSTPOINTERS14);
                    this.globals.gpWorldLevelData[gusCurMousePos].pOnRoofHead.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                    this.globals.gpWorldLevelData[gusCurMousePos].pOnRoofHead.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;

                }
                else
                {
                    this.world.AddTopmostToHead(gusCurMousePos, TileDefines.BADMARKER1);
                    this.globals.gpWorldLevelData[gusCurMousePos].pTopmostHead.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                    this.globals.gpWorldLevelData[gusCurMousePos].pTopmostHead.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;

                    if (gGameSettings.fOptions[TOPTION.CURSOR_3D])
                    {
                        this.world.AddTopmostToHead(gusCurMousePos, TileDefines.FIRSTPOINTERS13);
                        this.globals.gpWorldLevelData[gusCurMousePos].pTopmostHead.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                        this.globals.gpWorldLevelData[gusCurMousePos].pTopmostHead.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                    }
                }

                break;
        }

        // Add action points
        if (HandleUI.gfUIDisplayActionPoints)
        {
            if (HandleUI.gfUIDisplayActionPointsInvalid)
            {
                if (COUNTERDONE(CURSORFLASH))
                {
                    RESETCOUNTER(CURSORFLASH);

                    fShowAP = !fShowAP;
                }
            }
            else
            {
                fShowAP = true;
            }

            if (Interface.gsInterfaceLevel > 0)
            {
                this.world.AddUIElem(gusCurMousePos, TileDefines.DISPLAY_AP_INDEX, SNAPCURSOR_AP_X_STARTVAL, SNAPCURSOR_AP_Y_STARTVAL - TileDefine.WALL_HEIGHT - 10, out newUIElem);
            }
            else
            {
                this.world.AddUIElem(gusCurMousePos, TileDefines.DISPLAY_AP_INDEX, SNAPCURSOR_AP_X_STARTVAL, SNAPCURSOR_AP_Y_STARTVAL, out newUIElem);
            }

            newUIElem.uiFlags |= LEVELNODEFLAGS.DISPLAY_AP;
            newUIElem.uiAPCost = this.globals.gsCurrentActionPoints;
            newUIElem.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
            newUIElem.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;

            if (!fShowAP)
            {
                HandleUI.gfUIDisplayActionPointsBlack = true;
            }
        }
    }

    void EraseSnappingCursor()
    {
        this.world.RemoveAllTopmostsOfTypeRange(gusCurMousePos, TileTypeDefines.MOCKFLOOR, TileTypeDefines.MOCKFLOOR);
        this.world.RemoveAllTopmostsOfTypeRange(gusCurMousePos, TileTypeDefines.FIRSTPOINTERS, TileTypeDefines.LASTPOINTERS);
        this.world.RemoveAllObjectsOfTypeRange(gusCurMousePos, TileTypeDefines.FIRSTPOINTERS, TileTypeDefines.LASTPOINTERS);
        this.world.RemoveAllOnRoofsOfTypeRange(gusCurMousePos, TileTypeDefines.FIRSTPOINTERS, TileTypeDefines.LASTPOINTERS);
        this.world.RemoveAllOnRoofsOfTypeRange(gusCurMousePos, TileTypeDefines.MOCKFLOOR, TileTypeDefines.MOCKFLOOR);
    }

    void StartLooseCursor(short sGridNo, int uiCursorID)
    {
        gfLooseCursorOn = true;
        guiLooseCursorID = uiCursorID;
        guiLooseCursorTimeOfLastUpdate = this.clock.GetJA2Clock();
        gsLooseCursorGridNo = sGridNo;
    }


    void HandleLooseCursorDraw()
    {
        if ((this.clock.GetJA2Clock() - guiLooseCursorTimeOfLastUpdate) > LOOSE_CURSOR_DELAY)
        {
            gfLooseCursorOn = false;
        }

        if (gfLooseCursorOn)
        {
            this.world.AddUIElem(gsLooseCursorGridNo, TileDefines.FIRSTPOINTERS4, 0, 0, out LEVELNODE newUIElem);
            newUIElem.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
            newUIElem.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
        }
    }

    void HandleLooseCursorHide()
    {
        if (gfLooseCursorOn)
        {
            this.world.RemoveTopmost(gsLooseCursorGridNo, TileDefines.FIRSTPOINTERS4);
        }
    }


    TileDefines GetSnapCursorIndex(TileDefines usAdditionalData)
    {
        // OK, this function will get the 'true' index for drawing the cursor....
        if (gGameSettings.fOptions[TOPTION.CURSOR_3D])
        {
            return usAdditionalData switch
            {
                TileDefines.FIRSTPOINTERS2 => TileDefines.FIRSTPOINTERS13,
                TileDefines.FIRSTPOINTERS3 => TileDefines.FIRSTPOINTERS14,
                TileDefines.FIRSTPOINTERS4 => TileDefines.FIRSTPOINTERS15,
                TileDefines.FIRSTPOINTERS5 => TileDefines.FIRSTPOINTERS16,
                TileDefines.FIRSTPOINTERS6 => TileDefines.FIRSTPOINTERS17,
                TileDefines.FIRSTPOINTERS7 => TileDefines.FIRSTPOINTERS18,
                TileDefines.FIRSTPOINTERS9 => TileDefines.FIRSTPOINTERS19,
                TileDefines.FIRSTPOINTERS10 => TileDefines.FIRSTPOINTERS20,
                _ => usAdditionalData,
            };
        }
        else
        {
            return usAdditionalData;
        }
    }
}


public record UICursor(
    UICursorDefines uiCursorID,
    UICURSOR uiFlags,
    CURSOR usFreeCursorName,
    TileDefines usAdditionalData)
{
    public UICursor(
        UICursorDefines uiCursorID,
        UICURSOR uiFlags,
        CURSOR usFreeCursorName,
        int usAdditionalData)
        : this(uiCursorID, uiFlags, usFreeCursorName, (TileDefines)usAdditionalData)
    { }
}


[Flags]
public enum UICURSOR
{
    FREEFLOWING = 0x00000002,
    SNAPPING = 0x00000004,
    SHOWTILE = 0x00000008,
    FLASHING = 0x00000020,
    CENTERAPS = 0x00000040,
    SHOWTILEAPDEPENDENT = 0x00000080,
    DONTSHOW2NDLEVEL = 0x00000100,
}
