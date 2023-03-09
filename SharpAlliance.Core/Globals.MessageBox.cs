using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;
using SixLabors.ImageSharp;

namespace SharpAlliance.Core;

public partial class Globals
{
    public static bool gfNewMessageBox { get; set; }
    public static bool gfInMsgBox { get; set; } = false;
    public static MessageBox gMsgBox { get; set; } = new MessageBox();
    public static string? gzUserDefinedButton2;
    public static string? gzUserDefinedButton1;
    public static Rectangle gOldCursorLimitRectangle;
    public static bool fRestoreBackgroundForMessageBox;
    public static bool gfDontOverRideSaveBuffer;
    public static bool gfStartedFromGameScreen { get; set; }
    public static bool gfStartedFromMapScreen { get; set; }

    public static bool gfFadeInitialized;
    public static bool gfFadeInVideo;
    public static int gbFadeType;
    public static Action? gFadeFunction;
    public static bool gfOverheadMapDirty { get; set; }
    public static Rectangle MessageBoxRestrictedCursorRegion;
    // old mouse x and y positions
    public static Point pOldMousePosition;

}
