using System;
using Veldrid;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class CursorSubSystem : IDisposable
{
    public static void SetCurrentCursorFromDatabase(CURSOR cursor)
    {
    }

    public void Dispose()
    {
    }

    public void Draw(SpriteRenderer sr, GraphicsDevice gd, CommandList cl)
    {

    }

    public void GetRestrictedClipCursor(SixLabors.ImageSharp.Rectangle messageBoxRestrictedCursorRegion)
    {
    }

    public void FreeMouseCursor()
    {
    }

    public bool IsCursorRestricted()
    {
        return false;
    }
}

public enum CURSOR
{
    NORMAL,
    TARGET,
    TARGETON1,
    TARGETON2,
    TARGETON3,
    TARGETON4,
    TARGETON5,
    TARGETON6,
    TARGETON7,
    TARGETON8,
    TARGETON9,
    TARGETW1,
    TARGETW2,
    TARGETW3,
    TARGETW4,
    TARGETW5,
    TARGETRED,
    TARGETBLACK,
    TARGETDKBLACK,
    TARGETBURSTCONFIRM,
    TARGETBURST,
    TARGETBURSTRED,
    TARGETBURSTDKBLACK,
    PUNCHGRAY,
    PUNCHRED,
    PUNCHRED_ON1,
    PUNCHRED_ON2,
    PUNCHYELLOW_ON1,
    PUNCHYELLOW_ON2,
    PUNCHNOGO_ON1,
    PUNCHNOGO_ON2,
    RUN1,
    WALK1,
    SWAT1,
    PRONE1,
    HANDGRAB,
    NORMGRAB,
    KNIFE_REG,
    KNIFE_HIT,
    KNIFE_HIT_ON1,
    KNIFE_HIT_ON2,
    KNIFE_YELLOW_ON1,
    KNIFE_YELLOW_ON2,
    KNIFE_NOGO_ON1,
    KNIFE_NOGO_ON2,
    CROSS_REG,
    CROSS_ACTIVE,
    WWW,
    LAPTOP_SCREEN,
    IBEAM,
    LOOK,
    TALK,
    BLACKTALK,
    REDTALK,
    EXIT_NORTH,
    EXIT_SOUTH,
    EXIT_EAST,
    EXIT_WEST,
    NOEXIT_NORTH,
    NOEXIT_SOUTH,
    NOEXIT_EAST,
    NOEXIT_WEST,
    CONEXIT_NORTH,
    CONEXIT_SOUTH,
    CONEXIT_EAST,
    CONEXIT_WEST,
    STRATEGIC_VEHICLE,
    STRATEGIC_FOOT,
    INVALID_ACTION,
    CHOPPER,
    FLASH_TARGET,
    FLASH_TARGETBURST,
    FLASH_TALK,
    FLASH_REDTALK,
    CHECKMARK,
    TARGETWR1,
    TARGETYELLOW1,
    TARGETYELLOW2,
    TARGETYELLOW3,
    TARGETYELLOW4,
    EXIT_GRID,
    NOEXIT_GRID,
    CONEXIT_GRID,
    GOOD_WIRECUT,
    BAD_WIRECUT,
    GOOD_RELOAD,
    BAD_RELOAD,
    CUROSR_IBEAM_WHITE,
    GOOD_THROW,
    BAD_THROW,
    RED_THROW,
    FLASH_THROW,

    THROWKON1,
    THROWKON2,
    THROWKON3,
    THROWKON4,
    THROWKON5,
    THROWKON6,
    THROWKON7,
    THROWKON8,
    THROWKON9,
    THROWKW1,
    THROWKW2,
    THROWKW3,
    THROWKW4,
    THROWKW5,
    THROWKWR1,
    THROWKYELLOW1,
    THROWKYELLOW2,
    THROWKYELLOW3,
    THROWKYELLOW4,

    ITEM_GOOD_THROW,
    ITEM_BAD_THROW,
    ITEM_RED_THROW,
    ITEM_FLASH_THROW,
    ITEM_GIVE,

    BOMB_GRAY,
    BOMB_RED,
    REMOTE_GRAY,
    REMOTE_RED,

    ENTERV,
    DRIVEV,
    WAIT,

    PLACEMERC,
    PLACEGROUP,
    DPLACEMERC,
    DPLACEGROUP,
    REPAIR,
    REPAIRRED,

    JAR,
    JARRED,

    CAN,
    CANRED,

    X,
    WAIT_NODELAY,
    EXCHANGE_PLACES,

    STRATEGIC_BULLSEYE,
    JUMP_OVER,
    FUEL,
    FUEL_RED,

    MSYS_NO_CURSOR = 65534,
    VIDEO_NO_CURSOR = 0xFFFF,

    None = 0x10000,
}
