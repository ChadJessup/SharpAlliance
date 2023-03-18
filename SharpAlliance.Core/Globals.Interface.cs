using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const int ARROWS_X_OFFSET = 10;
    public const int ARROWS_HEIGHT = 20;
    public const int ARROWS_WIDTH = 20;
    public const int UPARROW_Y_OFFSET = -30;
    public const int DOWNARROW_Y_OFFSET = -10;
    public const int BUTTON_PANEL_WIDTH = 78;
    public const int BUTTON_PANEL_HEIGHT = 76;

    public static bool gfInMovementMenu = false;
    public static int giMenuAnchorX, giMenuAnchorY;

    public const int PROG_BAR_START_X = 5;
    public const int PROG_BAR_START_Y = 2;
    public const int PROG_BAR_LENGTH = 627;

    public static bool gfProgBarActive = false;
    public static int gubProgNumEnemies = 0;
    public static int gubProgCurEnemy = 0;
    public static TOP_MESSAGE gTopMessage;
    public static bool gfTopMessageDirty = false;
    public static MOUSE_REGION gMenuOverlayRegion;
    public static int giPopupSlideMessageOverlay = -1;
    public static int gusOverlayPopupBoxWidth, gusOverlayPopupBoxHeight;
    public static MercPopUpBox gpOverrideMercBox;
    public static int gusUIMessageWidth, gusUIMessageHeight;
    public static MercPopUpBox gpUIMessageOverrideMercBox;
    public static int guiUIMessageTimeDelay = 0;
    public static bool gfUseSkullIconMessage = false;

    public const int MAX_UICOMPOSITES = 4;
    public const int INTERFACE_START_Y = 360;
    public const int INV_INTERFACE_START_Y = 340;
    public const int INTERFACE_START_X = 0;

    // GLOBAL DEFINES FOR SOME UI FLAGS
    public const int ARROWS_HIDE_UP = 0x00000002;
    public const int ARROWS_HIDE_DOWN = 0x00000004;
    public const int ARROWS_SHOW_UP_BESIDE = 0x00000008;
    public const int ARROWS_SHOW_DOWN_BESIDE = 0x00000020;
    public const int ARROWS_SHOW_UP_ABOVE_Y = 0x00000040;
    public const int ARROWS_SHOW_DOWN_BELOW_Y = 0x00000080;
    public const int ARROWS_SHOW_DOWN_BELOW_G = 0x00000200;
    public const int ARROWS_SHOW_DOWN_BELOW_YG = 0x00000400;
    public const int ARROWS_SHOW_DOWN_BELOW_GG = 0x00000800;
    public const int ARROWS_SHOW_UP_ABOVE_G = 0x00002000;
    public const int ARROWS_SHOW_UP_ABOVE_YG = 0x00004000;
    public const int ARROWS_SHOW_UP_ABOVE_GG = 0x00008000;
    public const int ARROWS_SHOW_UP_ABOVE_YY = 0x00020000;
    public const int ARROWS_SHOW_DOWN_BELOW_YY = 0x00040000;
    public const int ARROWS_SHOW_UP_ABOVE_CLIMB = 0x00080000;
    public const int ARROWS_SHOW_UP_ABOVE_CLIMB2 = 0x00400000;
    public const int ARROWS_SHOW_UP_ABOVE_CLIMB3 = 0x00800000;
    public const int ARROWS_SHOW_DOWN_CLIMB = 0x02000000;
    public const int LOCATEANDSELECT_MERC = 1;
    public const int LOCATE_MERC_ONCE = 2;
}

public struct TOP_MESSAGE
{
    public int uiSurface;
    public int bCurrentMessage;
    public int uiTimeOfLastUpdate;
    public int uiTimeSinceLastBeep;
    public int bAnimate;
    public int bYPos;
    public int sWorldRenderX;
    public int sWorldRenderY;
    public bool fCreated;
}
