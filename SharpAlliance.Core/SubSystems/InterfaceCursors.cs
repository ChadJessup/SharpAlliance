using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core.SubSystems;

public class InterfaceCursors
{
    public const int DISPLAY_AP_INDEX = (int)TileDefines.MOCKFLOOR1;

    public const int SNAPCURSOR_AP_X_STARTVAL = 18;
    public const int SNAPCURSOR_AP_Y_STARTVAL = 9;

    public const int LOOSE_CURSOR_DELAY = 300;
    static bool gfLooseCursorOn = false;
    static short gsLooseCursorGridNo = IsometricDefines.NOWHERE;
    static int guiLooseCursorID = 0;
    static int guiLooseCursorTimeOfLastUpdate = 0;

    Dictionary<UICursorDefines, UICursor> gUICursors = new()
    {
        { UICursorDefines.NO_UICURSOR, new (UICursorDefines.NO_UICURSOR, 0, 0, 0) },
        { UICursorDefines.NORMAL_FREEUICURSOR, new (UICursorDefines.NORMAL_FREEUICURSOR, UICURSOR.FREEFLOWING, Cursor.NORMAL, 0) },
        { UICursorDefines.NORMAL_SNAPUICURSOR, new (UICursorDefines.NORMAL_SNAPUICURSOR, UICURSOR.SNAPPING, 0, 0) },
        { UICursorDefines.MOVE_RUN_UICURSOR, new (UICursorDefines.MOVE_RUN_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILEAPDEPENDENT | UICURSOR.DONTSHOW2NDLEVEL, Cursor.RUN1, TileDefines.FIRSTPOINTERS2) },
        { UICursorDefines.MOVE_WALK_UICURSOR, new(UICursorDefines.MOVE_WALK_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILEAPDEPENDENT | UICURSOR.DONTSHOW2NDLEVEL | UICURSOR.CENTERAPS, Cursor.WALK1, TileDefines.FIRSTPOINTERS2) } ,
        { UICursorDefines.MOVE_SWAT_UICURSOR, new (UICursorDefines.MOVE_SWAT_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILEAPDEPENDENT | UICURSOR.DONTSHOW2NDLEVEL, Cursor.SWAT1, TileDefines.FIRSTPOINTERS2) },
        { UICursorDefines.MOVE_PRONE_UICURSOR, new (UICursorDefines.MOVE_PRONE_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILEAPDEPENDENT | UICURSOR.DONTSHOW2NDLEVEL, Cursor.PRONE1, TileDefines.FIRSTPOINTERS2) },
        { UICursorDefines.MOVE_VEHICLE_UICURSOR, new (UICursorDefines.MOVE_VEHICLE_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILEAPDEPENDENT | UICURSOR.DONTSHOW2NDLEVEL, Cursor.DRIVEV, TileDefines.FIRSTPOINTERS2) },
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
        { UICursorDefines.MOVE_REALTIME_UICURSOR, new (UICursorDefines.MOVE_REALTIME_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILEAPDEPENDENT | UICURSOR.DONTSHOW2NDLEVEL, TileDefines.VIDEO_NO_CURSOR, TileDefines.FIRSTPOINTERS2) },
        { UICursorDefines.MOVE_RUN_REALTIME_UICURSOR, new (UICursorDefines.MOVE_RUN_REALTIME_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, TileDefines.VIDEO_NO_CURSOR, TileDefines.FIRSTPOINTERS7) },
        { UICursorDefines.CONFIRM_MOVE_REALTIME_UICURSOR, new (UICursorDefines.CONFIRM_MOVE_REALTIME_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, TileDefines.VIDEO_NO_CURSOR, TileDefines.FIRSTPOINTERS4) },
        { UICursorDefines.ALL_MOVE_REALTIME_UICURSOR, new (UICursorDefines.ALL_MOVE_REALTIME_UICURSOR, UICURSOR.FREEFLOWING | UICURSOR.SHOWTILE | UICURSOR.DONTSHOW2NDLEVEL, Cursor.VIDEO_NO_CURSOR, TileDefines.FIRSTPOINTERS5) },
        { UICursorDefines.ON_OWNED_MERC_UICURSOR, new (UICursorDefines.ON_OWNED_MERC_UICURSOR, UICURSOR.SNAPPING, 0, 0) },
        { UICursorDefines.ON_OWNED_SELMERC_UICURSOR, new (UICursorDefines.ON_OWNED_SELMERC_UICURSOR, UICURSOR.SNAPPING, 0, 0) },
        { UICursorDefines.ACTION_SHOOT_UICURSOR, new (UICursorDefines.ACTION_SHOOT_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGET, 0) },
        { UICursorDefines.ACTION_NOCHANCE_SHOOT_UICURSOR, new (UICursorDefines.ACTION_NOCHANCE_SHOOT_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGETDKBLACK, 0) },
        { UICursorDefines.ACTION_NOCHANCE_BURST_UICURSOR, new (UICursorDefines.ACTION_NOCHANCE_BURST_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGETBURSTDKBLACK, 0) },
        { UICursorDefines.ACTION_FLASH_TOSS_UICURSOR, new (UICursorDefines.ACTION_FLASH_TOSS_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGET, 0) },
        { UICursorDefines.ACTION_TOSS_UICURSOR, new (UICursorDefines.ACTION_TOSS_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGET, 0) },
        { UICursorDefines.ACTION_RED_TOSS_UICURSOR, new (UICursorDefines.ACTION_RED_TOSS_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGETRED, 0) },
        { UICursorDefines.ACTION_FLASH_SHOOT_UICURSOR, new (UICursorDefines.ACTION_FLASH_SHOOT_UICURSOR, UICURSOR.FREEFLOWING, Cursor.FLASH_TARGET, 0) },
        { UICursorDefines.ACTION_FLASH_BURST_UICURSOR, new (UICursorDefines.ACTION_FLASH_BURST_UICURSOR, UICURSOR.FREEFLOWING, Cursor.FLASH_TARGETBURST, 0) },
        { UICursorDefines.ACTION_TARGETAIM1_UICURSOR, new (UICursorDefines.ACTION_TARGETAIM1_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGETON1, 0) },
        { UICursorDefines.ACTION_TARGETAIM2_UICURSOR, new (UICursorDefines.ACTION_TARGETAIM2_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGETON2, 0) },
        { UICursorDefines.ACTION_TARGETAIM3_UICURSOR, new (UICursorDefines.ACTION_TARGETAIM3_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGETON3, 0) },
        { UICursorDefines.ACTION_TARGETAIM4_UICURSOR, new (UICursorDefines.ACTION_TARGETAIM4_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGETON4, 0) },
        { UICursorDefines.ACTION_TARGETAIM5_UICURSOR, new (UICursorDefines.ACTION_TARGETAIM5_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGETON5, 0) },
        { UICursorDefines.ACTION_TARGETAIM6_UICURSOR, new (UICursorDefines.ACTION_TARGETAIM6_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGETON6, 0) },
        { UICursorDefines.ACTION_TARGETAIM7_UICURSOR, new (UICursorDefines.ACTION_TARGETAIM7_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGETON7, 0) },
        { UICursorDefines.ACTION_TARGETAIM8_UICURSOR, new (UICursorDefines.ACTION_TARGETAIM8_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGETON8, 0) },
        { UICursorDefines.ACTION_TARGETAIM8_UICURSOR, new (UICursorDefines.ACTION_TARGETAIM8_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGETON9, 0) },
        { UICursorDefines.ACTION_TARGETAIMCANT1_UICURSOR, new (UICursorDefines.ACTION_TARGETAIMCANT1_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGETW1, 0) },
        { UICursorDefines.ACTION_TARGETAIMCANT2_UICURSOR, new (UICursorDefines.ACTION_TARGETAIMCANT2_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGETW2, 0) },
        { UICursorDefines.ACTION_TARGETAIMCANT3_UICURSOR, new (UICursorDefines.ACTION_TARGETAIMCANT3_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGETW3, 0) },
        { UICursorDefines.ACTION_TARGETAIMCANT4_UICURSOR, new (UICursorDefines.ACTION_TARGETAIMCANT4_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGETW4, 0) },
        { UICursorDefines.ACTION_TARGETAIMCANT5_UICURSOR, new (UICursorDefines.ACTION_TARGETAIMCANT5_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGETW5, 0) },
        { UICursorDefines.ACTION_TARGETRED_UICURSOR, new (UICursorDefines.ACTION_TARGETRED_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGETRED, 0) },
        { UICursorDefines.ACTION_TARGETBURST_UICURSOR, new (UICursorDefines.ACTION_TARGETBURST_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGETBURST, 0) },
        { UICursorDefines.ACTION_TARGETREDBURST_UICURSOR, new (UICursorDefines.ACTION_TARGETREDBURST_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGETBURSTRED, 0) },
        { UICursorDefines.ACTION_TARGETCONFIRMBURST_UICURSOR, new (UICursorDefines.ACTION_TARGETCONFIRMBURST_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGETBURSTCONFIRM, 0) },
        { UICursorDefines.ACTION_TARGETAIMFULL_UICURSOR, new (UICursorDefines.ACTION_TARGETAIMFULL_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGETWR1, 0) },
        { UICursorDefines.ACTION_TARGETAIMYELLOW1_UICURSOR, new (UICursorDefines.ACTION_TARGETAIMYELLOW1_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGETYELLOW1, 0) },
        { UICursorDefines.ACTION_TARGETAIMYELLOW2_UICURSOR, new (UICursorDefines.ACTION_TARGETAIMYELLOW2_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGETYELLOW2, 0) },
        { UICursorDefines.ACTION_TARGETAIMYELLOW3_UICURSOR, new (UICursorDefines.ACTION_TARGETAIMYELLOW3_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGETYELLOW3, 0) },
        { UICursorDefines.ACTION_TARGETAIMYELLOW4_UICURSOR, new (UICursorDefines.ACTION_TARGETAIMYELLOW4_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TARGETYELLOW4, 0) },
        { UICursorDefines.ACTION_TARGET_RELOADING, new (UICursorDefines.ACTION_TARGET_RELOADING, UICURSOR.FREEFLOWING, Cursor.TARGETBLACK, 0) },
        { UICursorDefines.ACTION_PUNCH_GRAY, new (UICursorDefines.ACTION_PUNCH_GRAY, UICURSOR.FREEFLOWING, Cursor.PUNCHGRAY, 0) },
        { UICursorDefines.ACTION_PUNCH_RED, new (UICursorDefines.ACTION_PUNCH_RED, UICURSOR.FREEFLOWING, Cursor.PUNCHRED, 0) },
        { UICursorDefines.ACTION_PUNCH_RED_AIM1_UICURSOR, new (UICursorDefines.ACTION_PUNCH_RED_AIM1_UICURSOR, UICURSOR.FREEFLOWING, Cursor.PUNCHRED_ON1, 0) },
        { UICursorDefines.ACTION_PUNCH_RED_AIM2_UICURSOR, new (UICursorDefines.ACTION_PUNCH_RED_AIM2_UICURSOR, UICURSOR.FREEFLOWING, Cursor.PUNCHRED_ON2, 0) },
        { UICursorDefines.ACTION_PUNCH_YELLOW_AIM1_UICURSOR, new (UICursorDefines.ACTION_PUNCH_YELLOW_AIM1_UICURSOR, UICURSOR.FREEFLOWING, Cursor.PUNCHYELLOW_ON1, 0) },
        { UICursorDefines.ACTION_PUNCH_YELLOW_AIM2_UICURSOR, new (UICursorDefines.ACTION_PUNCH_YELLOW_AIM2_UICURSOR, UICURSOR.FREEFLOWING, Cursor.PUNCHYELLOW_ON2, 0) },
        { UICursorDefines.ACTION_PUNCH_NOGO_AIM1_UICURSOR, new (UICursorDefines.ACTION_PUNCH_NOGO_AIM1_UICURSOR, UICURSOR.FREEFLOWING, Cursor.PUNCHNOGO_ON1, 0) },
        { UICursorDefines.ACTION_PUNCH_NOGO_AIM2_UICURSOR, new (UICursorDefines.ACTION_PUNCH_NOGO_AIM2_UICURSOR, UICURSOR.FREEFLOWING, Cursor.PUNCHNOGO_ON2, 0) },
        { UICursorDefines.ACTION_FIRSTAID_GRAY, new (UICursorDefines.ACTION_FIRSTAID_GRAY, UICURSOR.FREEFLOWING, Cursor.CROSS_REG, 0) },
        { UICursorDefines.ACTION_FIRSTAID_RED, new (UICursorDefines.ACTION_FIRSTAID_RED, UICURSOR.FREEFLOWING, Cursor.CROSS_ACTIVE, 0) },
        { UICursorDefines.ACTION_OPEN, new (UICursorDefines.ACTION_OPEN, UICURSOR.FREEFLOWING, Cursor.HANDGRAB, 0) },
        { UICursorDefines.CANNOT_MOVE_UICURSOR, new (UICursorDefines.CANNOT_MOVE_UICURSOR, UICURSOR.SNAPPING, 0, 0) },
        { UICursorDefines.NORMALHANDCURSOR_UICURSOR, new (UICursorDefines.NORMALHANDCURSOR_UICURSOR, UICURSOR.FREEFLOWING, Cursor.NORMGRAB, 0) },
        { UICursorDefines.OKHANDCURSOR_UICURSOR, new (UICursorDefines.OKHANDCURSOR_UICURSOR, UICURSOR.FREEFLOWING, Cursor.HANDGRAB, 0) },
        { UICursorDefines.KNIFE_REG_UICURSOR, new (UICursorDefines.KNIFE_REG_UICURSOR, UICURSOR.FREEFLOWING, Cursor.KNIFE_REG, 0) },
        { UICursorDefines.KNIFE_HIT_UICURSOR, new (UICursorDefines.KNIFE_HIT_UICURSOR, UICURSOR.FREEFLOWING, Cursor.KNIFE_HIT, 0) },
        { UICursorDefines.KNIFE_HIT_AIM1_UICURSOR, new (UICursorDefines.KNIFE_HIT_AIM1_UICURSOR, UICURSOR.FREEFLOWING, Cursor.KNIFE_HIT_ON1, 0) },
        { UICursorDefines.KNIFE_HIT_AIM2_UICURSOR, new (UICursorDefines.KNIFE_HIT_AIM2_UICURSOR, UICURSOR.FREEFLOWING, Cursor.KNIFE_HIT_ON2, 0) },
        { UICursorDefines.KNIFE_YELLOW_AIM1_UICURSOR, new (UICursorDefines.KNIFE_YELLOW_AIM1_UICURSOR, UICURSOR.FREEFLOWING, Cursor.KNIFE_YELLOW_ON1, 0) },
        { UICursorDefines.KNIFE_YELLOW_AIM2_UICURSOR, new (UICursorDefines.KNIFE_YELLOW_AIM2_UICURSOR, UICURSOR.FREEFLOWING, Cursor.KNIFE_YELLOW_ON2, 0) },
        { UICursorDefines.KNIFE_NOGO_AIM1_UICURSOR, new (UICursorDefines.KNIFE_NOGO_AIM1_UICURSOR, UICURSOR.FREEFLOWING, Cursor.KNIFE_NOGO_ON1, 0) },
        { UICursorDefines.KNIFE_NOGO_AIM2_UICURSOR, new (UICursorDefines.KNIFE_NOGO_AIM2_UICURSOR, UICURSOR.FREEFLOWING, Cursor.KNIFE_NOGO_ON2, 0) },
        { UICursorDefines.LOOK_UICURSOR, new (UICursorDefines.LOOK_UICURSOR, UICURSOR.FREEFLOWING, Cursor.LOOK, 0) },
        { UICursorDefines.TALK_NA_UICURSOR, new (UICursorDefines.TALK_NA_UICURSOR, UICURSOR.FREEFLOWING, Cursor.TALK, 0) },
        { UICursorDefines.TALK_A_UICURSOR, new (UICursorDefines.TALK_A_UICURSOR, UICURSOR.FREEFLOWING, Cursor.REDTALK, 0) },
        { UICursorDefines.TALK_OUT_RANGE_NA_UICURSOR, new (UICursorDefines.TALK_OUT_RANGE_NA_UICURSOR, UICURSOR.FREEFLOWING, Cursor.FLASH_TALK, (ushort)Cursor.BLACKTALK) },
        { UICursorDefines.TALK_OUT_RANGE_A_UICURSOR, new (UICursorDefines.TALK_OUT_RANGE_A_UICURSOR, UICURSOR.FREEFLOWING, Cursor.FLASH_REDTALK, (ushort)Cursor.BLACKTALK) },
        { UICursorDefines.EXIT_NORTH_UICURSOR, new (UICursorDefines.EXIT_NORTH_UICURSOR, UICURSOR.FREEFLOWING, Cursor.EXIT_NORTH, 0) },
        { UICursorDefines.EXIT_SOUTH_UICURSOR, new (UICursorDefines.EXIT_SOUTH_UICURSOR, UICURSOR.FREEFLOWING, Cursor.EXIT_SOUTH, 0) },
        { UICursorDefines.EXIT_EAST_UICURSOR, new (UICursorDefines.EXIT_EAST_UICURSOR, UICURSOR.FREEFLOWING, Cursor.EXIT_EAST, 0) },
        { UICursorDefines.EXIT_WEST_UICURSOR, new (UICursorDefines.EXIT_WEST_UICURSOR, UICURSOR.FREEFLOWING, Cursor.EXIT_WEST, 0) },
        { UICursorDefines.EXIT_GRID_UICURSOR, new (UICursorDefines.EXIT_GRID_UICURSOR, UICURSOR.FREEFLOWING, Cursor.EXIT_GRID, 0) },
        { UICursorDefines.NOEXIT_NORTH_UICURSOR, new (UICursorDefines.NOEXIT_NORTH_UICURSOR, UICURSOR.FREEFLOWING, Cursor.NOEXIT_NORTH, 0) },
        { UICursorDefines.NOEXIT_SOUTH_UICURSOR, new (UICursorDefines.NOEXIT_SOUTH_UICURSOR, UICURSOR.FREEFLOWING, Cursor.NOEXIT_SOUTH, 0) },
        { UICursorDefines.NOEXIT_EAST_UICURSOR, new (UICursorDefines.NOEXIT_EAST_UICURSOR, UICURSOR.FREEFLOWING, Cursor.NOEXIT_EAST, 0) },
        { UICursorDefines.NOEXIT_WEST_UICURSOR, new (UICursorDefines.NOEXIT_WEST_UICURSOR, UICURSOR.FREEFLOWING, Cursor.NOEXIT_WEST, 0) },
        { UICursorDefines.NOEXIT_GRID_UICURSOR, new (UICursorDefines.NOEXIT_GRID_UICURSOR, UICURSOR.FREEFLOWING, Cursor.NOEXIT_GRID, 0) },
        { UICursorDefines.CONFIRM_EXIT_NORTH_UICURSOR, new (UICursorDefines.CONFIRM_EXIT_NORTH_UICURSOR, UICURSOR.FREEFLOWING, Cursor.CONEXIT_NORTH, 0) },
        { UICursorDefines.CONFIRM_EXIT_SOUTH_UICURSOR, new (UICursorDefines.CONFIRM_EXIT_SOUTH_UICURSOR, UICURSOR.FREEFLOWING, Cursor.CONEXIT_SOUTH, 0) },
        { UICursorDefines.CONFIRM_EXIT_EAST_UICURSOR, new (UICursorDefines.CONFIRM_EXIT_EAST_UICURSOR, UICURSOR.FREEFLOWING, Cursor.CONEXIT_EAST, 0) },
        { UICursorDefines.CONFIRM_EXIT_WEST_UICURSOR, new (UICursorDefines.CONFIRM_EXIT_WEST_UICURSOR, UICURSOR.FREEFLOWING, Cursor.CONEXIT_WEST, 0) },
        { UICursorDefines.CONFIRM_EXIT_GRID_UICURSOR, new (UICursorDefines.CONFIRM_EXIT_GRID_UICURSOR, UICURSOR.FREEFLOWING, Cursor.CONEXIT_GRID, 0) },
        { UICursorDefines.GOOD_WIRECUTTER_UICURSOR, new (UICursorDefines.GOOD_WIRECUTTER_UICURSOR, UICURSOR.FREEFLOWING, Cursor.GOOD_WIRECUT, 0) },
        { UICursorDefines.BAD_WIRECUTTER_UICURSOR, new (UICursorDefines.BAD_WIRECUTTER_UICURSOR, UICURSOR.FREEFLOWING, Cursor.BAD_WIRECUT, 0) },
        { UICursorDefines.GOOD_REPAIR_UICURSOR, new (UICursorDefines.GOOD_REPAIR_UICURSOR, UICURSOR.FREEFLOWING, Cursor.REPAIR, 0) },
        { UICursorDefines.BAD_REPAIR_UICURSOR, new (UICursorDefines.BAD_REPAIR_UICURSOR, UICURSOR.FREEFLOWING, Cursor.REPAIRRED, 0) },
        { UICursorDefines.GOOD_RELOAD_UICURSOR, new (UICursorDefines.GOOD_RELOAD_UICURSOR, UICURSOR.FREEFLOWING, Cursor.GOOD_RELOAD, 0) },
        { UICursorDefines.BAD_RELOAD_UICURSOR, new (UICursorDefines.BAD_RELOAD_UICURSOR, UICURSOR.FREEFLOWING, Cursor.BAD_RELOAD, 0) },
        { UICursorDefines.GOOD_JAR_UICURSOR, new (UICursorDefines.GOOD_JAR_UICURSOR, UICURSOR.FREEFLOWING, Cursor.JARRED, 0) },
        { UICursorDefines.BAD_JAR_UICURSOR, new (UICursorDefines.BAD_JAR_UICURSOR, UICURSOR.FREEFLOWING, Cursor.JAR, 0) },
        { UICursorDefines.GOOD_THROW_UICURSOR, new (UICursorDefines.GOOD_THROW_UICURSOR, UICURSOR.FREEFLOWING, Cursor.GOOD_THROW, 0) },
        { UICursorDefines.BAD_THROW_UICURSOR, new (UICursorDefines.BAD_THROW_UICURSOR, UICURSOR.FREEFLOWING, Cursor.BAD_THROW, 0) },
        { UICursorDefines.RED_THROW_UICURSOR, new (UICursorDefines.RED_THROW_UICURSOR, UICURSOR.FREEFLOWING, Cursor.RED_THROW, 0) },
        { UICursorDefines.FLASH_THROW_UICURSOR, new (UICursorDefines.FLASH_THROW_UICURSOR, UICURSOR.FREEFLOWING, Cursor.FLASH_THROW, 0) },
        { UICursorDefines.ACTION_THROWAIM1_UICURSOR, new (UICursorDefines.ACTION_THROWAIM1_UICURSOR, UICURSOR.FREEFLOWING, Cursor.THROWKON1, 0) },
        { UICursorDefines.ACTION_THROWAIM2_UICURSOR, new (UICursorDefines.ACTION_THROWAIM2_UICURSOR, UICURSOR.FREEFLOWING, Cursor.THROWKON2, 0) },
        { UICursorDefines.ACTION_THROWAIM3_UICURSOR, new (UICursorDefines.ACTION_THROWAIM3_UICURSOR, UICURSOR.FREEFLOWING, Cursor.THROWKON3, 0) },
        { UICursorDefines.ACTION_THROWAIM4_UICURSOR, new (UICursorDefines.ACTION_THROWAIM4_UICURSOR, UICURSOR.FREEFLOWING, Cursor.THROWKON4, 0) },
        { UICursorDefines.ACTION_THROWAIM5_UICURSOR, new (UICursorDefines.ACTION_THROWAIM5_UICURSOR, UICURSOR.FREEFLOWING, Cursor.THROWKON5, 0) },
        { UICursorDefines.ACTION_THROWAIM6_UICURSOR, new (UICursorDefines.ACTION_THROWAIM6_UICURSOR, UICURSOR.FREEFLOWING, Cursor.THROWKON6, 0) },
        { UICursorDefines.ACTION_THROWAIM7_UICURSOR, new (UICursorDefines.ACTION_THROWAIM7_UICURSOR, UICURSOR.FREEFLOWING, Cursor.THROWKON7, 0) },
        { UICursorDefines.ACTION_THROWAIM8_UICURSOR, new (UICursorDefines.ACTION_THROWAIM8_UICURSOR, UICURSOR.FREEFLOWING, Cursor.THROWKON8, 0) },
        { UICursorDefines.ACTION_THROWAIM8_UICURSOR, new (UICursorDefines.ACTION_THROWAIM8_UICURSOR, UICURSOR.FREEFLOWING, Cursor.THROWKON9, 0) },
        { UICursorDefines.ACTION_THROWAIMCANT1_UICURSOR, new (UICursorDefines.ACTION_THROWAIMCANT1_UICURSOR, UICURSOR.FREEFLOWING, Cursor.THROWKW1, 0) },
        { UICursorDefines.ACTION_THROWAIMCANT2_UICURSOR, new (UICursorDefines.ACTION_THROWAIMCANT2_UICURSOR, UICURSOR.FREEFLOWING, Cursor.THROWKW2, 0) },
        { UICursorDefines.ACTION_THROWAIMCANT3_UICURSOR, new (UICursorDefines.ACTION_THROWAIMCANT3_UICURSOR, UICURSOR.FREEFLOWING, Cursor.THROWKW3, 0) },
        { UICursorDefines.ACTION_THROWAIMCANT4_UICURSOR, new (UICursorDefines.ACTION_THROWAIMCANT4_UICURSOR, UICURSOR.FREEFLOWING, Cursor.THROWKW4, 0) },
        { UICursorDefines.ACTION_THROWAIMCANT5_UICURSOR, new (UICursorDefines.ACTION_THROWAIMCANT5_UICURSOR, UICURSOR.FREEFLOWING, Cursor.THROWKW5, 0) },
        { UICursorDefines.ACTION_THROWAIMFULL_UICURSOR, new (UICursorDefines.ACTION_THROWAIMFULL_UICURSOR, UICURSOR.FREEFLOWING, Cursor.THROWKWR1, 0) },
        { UICursorDefines.ACTION_THROWAIMYELLOW1_UICURSOR, new (UICursorDefines.ACTION_THROWAIMYELLOW1_UICURSOR, UICURSOR.FREEFLOWING, Cursor.THROWKYELLOW1, 0) },
        { UICursorDefines.ACTION_THROWAIMYELLOW2_UICURSOR, new (UICursorDefines.ACTION_THROWAIMYELLOW2_UICURSOR, UICURSOR.FREEFLOWING, Cursor.THROWKYELLOW2, 0) },
        { UICursorDefines.ACTION_THROWAIMYELLOW3_UICURSOR, new (UICursorDefines.ACTION_THROWAIMYELLOW3_UICURSOR, UICURSOR.FREEFLOWING, Cursor.THROWKYELLOW3, 0) },
        { UICursorDefines.ACTION_THROWAIMYELLOW4_UICURSOR, new (UICursorDefines.ACTION_THROWAIMYELLOW4_UICURSOR, UICURSOR.FREEFLOWING, Cursor.THROWKYELLOW4, 0) },
        { UICursorDefines.THROW_ITEM_GOOD_UICURSOR, new (UICursorDefines.THROW_ITEM_GOOD_UICURSOR, UICURSOR.FREEFLOWING, Cursor.ITEM_GOOD_THROW, 0) },
        { UICursorDefines.THROW_ITEM_BAD_UICURSOR, new (UICursorDefines.THROW_ITEM_BAD_UICURSOR, UICURSOR.FREEFLOWING, Cursor.ITEM_BAD_THROW, 0) },
        { UICursorDefines.THROW_ITEM_RED_UICURSOR, new (UICursorDefines.THROW_ITEM_RED_UICURSOR, UICURSOR.FREEFLOWING, Cursor.ITEM_RED_THROW, 0) },
        { UICursorDefines.THROW_ITEM_FLASH_UICURSOR, new (UICursorDefines.THROW_ITEM_FLASH_UICURSOR, UICURSOR.FREEFLOWING, Cursor.ITEM_FLASH_THROW, 0) },
        { UICursorDefines.PLACE_BOMB_GREY_UICURSOR, new (UICursorDefines.PLACE_BOMB_GREY_UICURSOR, UICURSOR.FREEFLOWING, Cursor.BOMB_GRAY, 0) },
        { UICursorDefines.PLACE_BOMB_RED_UICURSOR, new (UICursorDefines.PLACE_BOMB_RED_UICURSOR, UICURSOR.FREEFLOWING, Cursor.BOMB_RED, 0) },
        { UICursorDefines.PLACE_REMOTE_GREY_UICURSOR, new (UICursorDefines.PLACE_REMOTE_GREY_UICURSOR, UICURSOR.FREEFLOWING, Cursor.REMOTE_GRAY, 0) },
        { UICursorDefines.PLACE_REMOTE_RED_UICURSOR, new (UICursorDefines.PLACE_REMOTE_RED_UICURSOR, UICURSOR.FREEFLOWING, Cursor.REMOTE_RED, 0) },
        { UICursorDefines.PLACE_TINCAN_GREY_UICURSOR, new (UICursorDefines.PLACE_TINCAN_GREY_UICURSOR, UICURSOR.FREEFLOWING, Cursor.CAN, 0) },
        { UICursorDefines.PLACE_TINCAN_RED_UICURSOR, new (UICursorDefines.PLACE_TINCAN_RED_UICURSOR, UICURSOR.FREEFLOWING, Cursor.CANRED, 0) },
        { UICursorDefines.ENTER_VEHICLE_UICURSOR, new (UICursorDefines.ENTER_VEHICLE_UICURSOR, UICURSOR.FREEFLOWING, Cursor.ENTERV, 0) },
        { UICursorDefines.INVALID_ACTION_UICURSOR, new (UICursorDefines.INVALID_ACTION_UICURSOR, UICURSOR.FREEFLOWING, Cursor.INVALID_ACTION, 0) },
        { UICursorDefines.FLOATING_X_UICURSOR, new (UICursorDefines.FLOATING_X_UICURSOR, UICURSOR.FREEFLOWING, Cursor.X, 0) },
        { UICursorDefines.EXCHANGE_PLACES_UICURSOR, new (UICursorDefines.EXCHANGE_PLACES_UICURSOR, UICURSOR.FREEFLOWING, Cursor.EXCHANGE_PLACES, 0) },
        { UICursorDefines.JUMP_OVER_UICURSOR, new (UICursorDefines.JUMP_OVER_UICURSOR, UICURSOR.FREEFLOWING, Cursor.JUMP_OVER, 0) },
        { UICursorDefines.REFUEL_GREY_UICURSOR, new (UICursorDefines.REFUEL_GREY_UICURSOR, UICURSOR.FREEFLOWING, Cursor.FUEL, 0) },
        { UICursorDefines.REFUEL_RED_UICURSOR, new(UICursorDefines.REFUEL_RED_UICURSOR, UICURSOR.FREEFLOWING, Cursor.FUEL_RED, 0) },
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

        //RaiseMouseToLevel( (INT8)gsInterfaceLevel );

        HandleLooseCursorDraw();


        // OK, WE OVERRIDE HERE CURSOR DRAWING FOR THINGS LIKE
        if (gpItemPointer != null)
        {
            MSYS_ChangeRegionCursor(&gViewportRegion, VIDEO_NO_CURSOR);

            // Check if we are in the viewport region...
            if (gViewportRegion.uiFlags & MSYS_MOUSE_IN_AREA)
            {
                DrawItemTileCursor();
            }
            else
            {
                DrawItemFreeCursor();
            }
            return true;
        }

        if (GetMouseMapPos(ref usMapPos))
        {
            gusCurMousePos = usMapPos;

            if (guiCurUICursor == UICursorDefines.NO_UICURSOR)
            {
                MSYS_ChangeRegionCursor(&gViewportRegion, VIDEO_NO_CURSOR);
                return true;
            }

            if (gUICursors[guiCurUICursor].uiFlags.HasFlag(UICURSOR.SHOWTILE))
            {
                if (gsInterfaceLevel == InterfaceLevel.I_ROOF_LEVEL)
                {
                    pNode = AddTopmostToTail(gusCurMousePos, GetSnapCursorIndex(TileDefines.FIRSTPOINTERS3));
                }
                else
                {
                    pNode = AddTopmostToTail(gusCurMousePos, GetSnapCursorIndex(gUICursors[guiCurUICursor].usAdditionalData));
                }
                pNode.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                pNode.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;

                if (gsInterfaceLevel == InterfaceLevel.I_ROOF_LEVEL)
                {
                    // Put one on the roof as well
                    AddOnRoofToHead(gusCurMousePos, GetSnapCursorIndex(gUICursors[guiCurUICursor].usAdditionalData));
                    gpWorldLevelData[gusCurMousePos].pOnRoofHead.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                    gpWorldLevelData[gusCurMousePos].pOnRoofHead.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                }
            }

            gfTargetDropPos = false;

            if (gUICursors[guiCurUICursor].uiFlags.HasFlag(UICURSOR.FREEFLOWING) && !(gUICursors[guiCurUICursor].uiFlags.HasFlag(UICURSOR.DONTSHOW2NDLEVEL)))
            {
                gfTargetDropPos = true;
                gusTargetDropPos = gusCurMousePos;

                if (gsInterfaceLevel == InterfaceLevel.I_ROOF_LEVEL)
                {
                    // If we are over a target, jump to that....
                    if (gfUIFullTargetFound)
                    {
                        gusTargetDropPos = MercPtrs[gusUIFullTargetID].sGridNo;
                    }

                    // Put tile on the floor
                    AddTopmostToTail(gusTargetDropPos, TileDefines.FIRSTPOINTERS14);
                    gpWorldLevelData[gusTargetDropPos].pTopmostHead.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                    gpWorldLevelData[gusTargetDropPos].pTopmostHead.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;

                }
            }

            if (gUICursors[guiCurUICursor].uiFlags.HasFlag(UICURSOR.SHOWTILEAPDEPENDENT))
            {
                // Add depending on AP status
                usTileCursor = gUICursors[guiCurUICursor].usAdditionalData;

                // ATE; Is the current guy in steath mode?
                if (gusSelectedSoldier != NOBODY)
                {
                    if (MercPtrs[gusSelectedSoldier].bStealthMode)
                    {
                        usTileCursor = TileDefines.FIRSTPOINTERS9;
                    }
                }

                if (gfUIDisplayActionPointsInvalid || gsCurrentActionPoints == 0)
                {
                    usTileCursor = TileDefines.FIRSTPOINTERS6;

                    // ATE; Is the current guy in steath mode?
                    if (gusSelectedSoldier != NOBODY)
                    {
                        if (MercPtrs[gusSelectedSoldier].bStealthMode)
                        {
                            usTileCursor = TileDefines.FIRSTPOINTERS10;
                        }
                    }
                }

                if (gsInterfaceLevel == InterfaceLevel.I_ROOF_LEVEL)
                {
                    pNode = AddTopmostToTail(gusCurMousePos, GetSnapCursorIndex(TileDefines.FIRSTPOINTERS14));
                }
                else
                {
                    pNode = AddTopmostToTail(gusCurMousePos, GetSnapCursorIndex(usTileCursor));
                }

                pNode.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                pNode.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;

                if (gsInterfaceLevel == InterfaceLevel.I_ROOF_LEVEL)
                {
                    // Put one on the roof as well
                    AddOnRoofToHead(gusCurMousePos, GetSnapCursorIndex(usTileCursor));
                    gpWorldLevelData[gusCurMousePos].pOnRoofHead.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                    gpWorldLevelData[gusCurMousePos].pOnRoofHead.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                }
            }


            // If snapping - remove from main viewport
            if (gUICursors[guiCurUICursor].uiFlags.HasFlag(UICURSOR.SNAPPING))
            {
                // Hide mouse region cursor
                MSYS_ChangeRegionCursor(out gViewportRegion, Cursor.VIDEO_NO_CURSOR);

                // Set Snapping Cursor
                DrawSnappingCursor();
            }


            if (gUICursors[guiCurUICursor].uiFlags.HasFlag(UICURSOR.FREEFLOWING))
            {
                switch (guiCurUICursor)
                {
                    case UICursorDefines.MOVE_VEHICLE_UICURSOR:

                        // Set position for APS
                        gfUIDisplayActionPointsCenter = false;
                        gUIDisplayActionPointsOffX = 16;
                        gUIDisplayActionPointsOffY = 14;
                        break;

                    case UICursorDefines.MOVE_WALK_UICURSOR:
                    case UICursorDefines.MOVE_RUN_UICURSOR:

                        // Set position for APS
                        gfUIDisplayActionPointsCenter = false;
                        gUIDisplayActionPointsOffX = 16;
                        gUIDisplayActionPointsOffY = 14;
                        break;

                    case UICursorDefines.MOVE_SWAT_UICURSOR:

                        // Set position for APS
                        gfUIDisplayActionPointsCenter = false;
                        gUIDisplayActionPointsOffX = 16;
                        gUIDisplayActionPointsOffY = 10;
                        break;

                    case UICursorDefines.MOVE_PRONE_UICURSOR:

                        // Set position for APS
                        gfUIDisplayActionPointsCenter = false;
                        gUIDisplayActionPointsOffX = 16;
                        gUIDisplayActionPointsOffY = 9;
                        break;
                }

                fHideCursor = false;

                if (!fHideCursor)
                {
                    MSYS_ChangeRegionCursor(out gViewportRegion, gUICursors[guiCurUICursor].usFreeCursorName);

                }
                else
                {
                    // Hide
                    MSYS_ChangeRegionCursor(out gViewportRegion, VIDEO_NO_CURSOR);
                }

            }

            if (gUICursors[guiCurUICursor].uiFlags.HasFlag(UICURSOR.CENTERAPS))
            {
                gfUIDisplayActionPointsCenter = true;
            }
        }
        return true;
    }

    bool HideUICursor()
    {
        HandleLooseCursorHide();

        // OK, WE OVERRIDE HERE CURSOR DRAWING FOR THINGS LIKE
        if (gpItemPointer != null)
        {
            // Check if we are in the viewport region...
            if (gViewportRegion.uiFlags.HasFlag(MSYS_MOUSE_IN_AREA))
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
            RemoveAllTopmostsOfTypeRange(gusCurMousePos, TileTypeDefines.FIRSTPOINTERS, TileTypeDefines.FIRSTPOINTERS);
            RemoveAllOnRoofsOfTypeRange(gusCurMousePos, TileTypeDefines.FIRSTPOINTERS, TileTypeDefines.FIRSTPOINTERS);
        }


        if (gUICursors[guiCurUICursor].uiFlags.HasFlag(UICURSOR.FREEFLOWING) && !(gUICursors[guiCurUICursor].uiFlags.HasFlag(UICURSOR.DONTSHOW2NDLEVEL)))
        {
            if (gsInterfaceLevel == I_ROOF_LEVEL)
            {
                RemoveTopmost(gusCurMousePos, TileDefines.FIRSTPOINTERS14);
                RemoveTopmost(gusCurMousePos, TileDefines.FIRSTPOINTERS9);

                if (gfTargetDropPos)
                {
                    RemoveTopmost(gusTargetDropPos, TileDefines.FIRSTPOINTERS14);
                    RemoveTopmost(gusTargetDropPos, TileDefines.FIRSTPOINTERS9);
                }
            }

        }


        // If snapping - remove from main viewport
        if (gUICursors[guiCurUICursor].uiFlags & UICURSOR.SNAPPING)
        {
            // hide Snapping Cursor
            EraseSnappingCursor();
        }

        if (gUICursors[guiCurUICursor].uiFlags & UICURSOR.FREEFLOWING)
        {
            // Nothing special here...
        }

        return true;
    }

    static bool fShowAP = true;
    void DrawSnappingCursor()
    {
        LEVELNODE? pNewUIElem;
        SOLDIERTYPE? pSoldier;

        if (gusSelectedSoldier != NO_SOLDIER)
        {
            GetSoldier(out pSoldier, gusSelectedSoldier);

        }

        // If we are in draw item mode, do nothing here but call the fuctiuon
        switch (guiCurUICursor)
        {
            case UICursorDefines.NO_UICURSOR:
                break;

            case UICursorDefines.NORMAL_SNAPUICURSOR:

                AddTopmostToHead(gusCurMousePos, TileDefines.FIRSTPOINTERS1);
                gpWorldLevelData[gusCurMousePos].pTopmostHead.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                gpWorldLevelData[gusCurMousePos].pTopmostHead.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                break;

            case UICursorDefines.ALL_MOVE_RUN_UICURSOR:
            case UICursorDefines.CONFIRM_MOVE_RUN_UICURSOR:
                if (gsInterfaceLevel > 0)
                {
                    AddUIElem(gusCurMousePos, TileDefines.GOODRUN1, 0, -WALL_HEIGHT - 8, out pNewUIElem);
                }
                else
                {
                    AddUIElem(gusCurMousePos, TileDefines.GOODRUN1, 0, 0, out pNewUIElem);
                }
                pNewUIElem.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                pNewUIElem.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                break;

            case UICursorDefines.ALL_MOVE_WALK_UICURSOR:
            case UICursorDefines.CONFIRM_MOVE_WALK_UICURSOR:
                if (gsInterfaceLevel > 0)
                {
                    AddUIElem(gusCurMousePos, TileDefines.GOODWALK1, 0, -WALL_HEIGHT - 8, out pNewUIElem);
                }
                else
                {
                    AddUIElem(gusCurMousePos, TileDefines.GOODWALK1, 0, 0, out pNewUIElem);
                }
                pNewUIElem.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                pNewUIElem.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                break;

            case UICursorDefines.ALL_MOVE_SWAT_UICURSOR:
            case UICursorDefines.CONFIRM_MOVE_SWAT_UICURSOR:
                if (gsInterfaceLevel > 0)
                {
                    AddUIElem(gusCurMousePos, TileDefines.GOODSWAT1, 0, -WALL_HEIGHT - 8, out pNewUIElem);
                }
                else
                {
                    AddUIElem(gusCurMousePos, TileDefines.GOODSWAT1, 0, 0, out pNewUIElem);
                }
                pNewUIElem.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                pNewUIElem.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                break;

            case UICursorDefines.ALL_MOVE_PRONE_UICURSOR:
            case UICursorDefines.CONFIRM_MOVE_PRONE_UICURSOR:
                if (gsInterfaceLevel > 0)
                {
                    AddUIElem(gusCurMousePos, TileDefines.GOODPRONE1, 0, -WALL_HEIGHT - 8 - 6, out pNewUIElem);
                }
                else
                {
                    AddUIElem(gusCurMousePos, TileDefines.GOODPRONE1, 0, -6, out pNewUIElem);
                }
                pNewUIElem.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                pNewUIElem.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                break;

            case UICursorDefines.ALL_MOVE_VEHICLE_UICURSOR:
            case UICursorDefines.CONFIRM_MOVE_VEHICLE_UICURSOR:
                if (gsInterfaceLevel > 0)
                {
                    AddUIElem(gusCurMousePos, TileDefines.VEHICLEMOVE1, 0, -WALL_HEIGHT - 8, out pNewUIElem);
                }
                else
                {
                    AddUIElem(gusCurMousePos, TileDefines.VEHICLEMOVE1, 0, 0, out pNewUIElem);
                }

                pNewUIElem.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                pNewUIElem.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                break;

            case UICursorDefines.MOVE_REALTIME_UICURSOR:
                break;

            case UICursorDefines.CANNOT_MOVE_UICURSOR:

                if (gsInterfaceLevel > 0)
                {
                    AddUIElem(gusCurMousePos, TileDefines.BADMARKER1, 0, -WALL_HEIGHT - 8, out pNewUIElem);
                    pNewUIElem.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                    pNewUIElem.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;

                    if (gGameSettings.fOptions[TOPTION_3D_CURSOR])
                    {
                        AddTopmostToHead(gusCurMousePos, TileDefines.FIRSTPOINTERS13);
                        gpWorldLevelData[gusCurMousePos].pTopmostHead.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                        gpWorldLevelData[gusCurMousePos].pTopmostHead.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                    }

                    AddOnRoofToHead(gusCurMousePos, TileDefines.FIRSTPOINTERS14);
                    gpWorldLevelData[gusCurMousePos].pOnRoofHead.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                    gpWorldLevelData[gusCurMousePos].pOnRoofHead.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;

                }
                else
                {
                    AddTopmostToHead(gusCurMousePos, TileDefines.BADMARKER1);
                    gpWorldLevelData[gusCurMousePos].pTopmostHead.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                    gpWorldLevelData[gusCurMousePos].pTopmostHead.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;

                    if (gGameSettings.fOptions[TOPTION_3D_CURSOR])
                    {
                        AddTopmostToHead(gusCurMousePos, TileDefines.FIRSTPOINTERS13);
                        gpWorldLevelData[gusCurMousePos].pTopmostHead.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                        gpWorldLevelData[gusCurMousePos].pTopmostHead.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                    }
                }

                break;
        }

        // Add action points
        if (gfUIDisplayActionPoints)
        {

            if (gfUIDisplayActionPointsInvalid)
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

            if (gsInterfaceLevel > 0)
            {
                AddUIElem(gusCurMousePos, DISPLAY_AP_INDEX, SNAPCURSOR_AP_X_STARTVAL, SNAPCURSOR_AP_Y_STARTVAL - WALL_HEIGHT - 10, out pNewUIElem);
            }
            else
            {
                AddUIElem(gusCurMousePos, DISPLAY_AP_INDEX, SNAPCURSOR_AP_X_STARTVAL, SNAPCURSOR_AP_Y_STARTVAL, out pNewUIElem);
            }
            pNewUIElem.uiFlags |= LEVELNODE_DISPLAY_AP;
            pNewUIElem.uiAPCost = gsCurrentActionPoints;
            pNewUIElem.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
            pNewUIElem.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;

            if (!fShowAP)
            {
                gfUIDisplayActionPointsBlack = true;
            }
        }
    }

    void EraseSnappingCursor()
    {
        RemoveAllTopmostsOfTypeRange(gusCurMousePos, TileTypeDefines.MOCKFLOOR, TileTypeDefines.MOCKFLOOR);
        RemoveAllTopmostsOfTypeRange(gusCurMousePos, TileTypeDefines.FIRSTPOINTERS, TileTypeDefines.LASTPOINTERS);
        RemoveAllObjectsOfTypeRange(gusCurMousePos, TileTypeDefines.FIRSTPOINTERS, TileTypeDefines.LASTPOINTERS);
        RemoveAllOnRoofsOfTypeRange(gusCurMousePos, TileTypeDefines.FIRSTPOINTERS, TileTypeDefines.LASTPOINTERS);
        RemoveAllOnRoofsOfTypeRange(gusCurMousePos, TileTypeDefines.MOCKFLOOR, TileTypeDefines.MOCKFLOOR);
    }

    void StartLooseCursor(short sGridNo, int uiCursorID)
    {
        gfLooseCursorOn = true;

        guiLooseCursorID = uiCursorID;

        guiLooseCursorTimeOfLastUpdate = GetJA2Clock();

        gsLooseCursorGridNo = sGridNo;
    }


    void HandleLooseCursorDraw()
    {
        LEVELNODE? pNewUIElem;

        if ((GetJA2Clock() - guiLooseCursorTimeOfLastUpdate) > LOOSE_CURSOR_DELAY)
        {
            gfLooseCursorOn = false;
        }

        if (gfLooseCursorOn)
        {
            AddUIElem(gsLooseCursorGridNo, TileDefines.FIRSTPOINTERS4, 0, 0, out pNewUIElem);
            pNewUIElem.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
            pNewUIElem.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
        }
    }


    void HandleLooseCursorHide()
    {
        if (gfLooseCursorOn)
        {
            RemoveTopmost(gsLooseCursorGridNo, TileDefines.FIRSTPOINTERS4);
        }
    }


    TileDefines GetSnapCursorIndex(TileDefines usAdditionalData)
    {
        // OK, this function will get the 'true' index for drawing the cursor....
        if (gGameSettings.fOptions[TOPTION_3D_CURSOR])
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
    Cursor usFreeCursorName,
    TileDefines usAdditionalData)
{
    public UICursor(
        UICursorDefines uiCursorID,
        UICURSOR uiFlags,
        Cursor usFreeCursorName,
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
