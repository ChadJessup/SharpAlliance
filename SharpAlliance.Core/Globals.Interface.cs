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
