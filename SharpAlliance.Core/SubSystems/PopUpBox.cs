using SixLabors.ImageSharp;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class PopUpBox
{
    // force an update of this box
    public static void ForceUpDateOfBox(int uiIndex)
    {
        if ((uiIndex < 0) || (uiIndex >= Globals.MAX_POPUP_BOX_COUNT))
        {
            return;
        }

        if (Globals.PopUpBoxList[uiIndex] != null)
        {
            Globals.PopUpBoxList[uiIndex].fUpdated = false;
        }
    }
}

public class POPUPBOX
{
    public Rectangle Dimensions;
    public Point Position;
    public int uiLeftMargin;
    public int uiRightMargin;
    public int uiBottomMargin;
    public int uiTopMargin;
    public int uiLineSpace;
    public int iBorderObjectIndex;
    public int iBackGroundSurface;
    public int uiFlags;
    public int uiBuffer;
    public int uiSecondColumnMinimunOffset;
    public int uiSecondColumnCurrentOffset;
    public int uiBoxMinWidth;
    public bool fUpdated;
    public bool fShowBox;

    public POPUPSTRINGPTR[] Text = new POPUPSTRINGPTR[Globals.MAX_POPUP_BOX_STRING_COUNT];
    public POPUPSTRINGPTR[] pSecondColumnString = new POPUPSTRINGPTR[Globals.MAX_POPUP_BOX_STRING_COUNT];
}

public struct POPUPSTRINGPTR
{
    public string pString;
    public int ubForegroundColor;
    public int ubBackgroundColor;
    public int ubHighLight;
    public int ubShade;
    public int ubSecondaryShade;
    public int uiFont;
    public int fColorFlag;
    public bool fHighLightFlag;
    public bool fShadeFlag;
    public bool fSecondaryShadeFlag;
}
