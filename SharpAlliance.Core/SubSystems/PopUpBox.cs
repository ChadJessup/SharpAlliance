using System;
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

    internal static FontStyle GetBoxFont(int ghVehicleBox)
    {
        throw new NotImplementedException();
    }

    internal static void GetBoxPosition(int hBoxHandle, out Point Position)
    {
        if ((hBoxHandle < 0) || (hBoxHandle >= MAX_POPUP_BOX_COUNT))
        {
            Position = new Point();
            return;
        }

        //        Assert(PopUpBoxList[hBoxHandle]);

        Position = new Point
        {
            X = PopUpBoxList[hBoxHandle].Position.X,
            Y = PopUpBoxList[hBoxHandle].Position.Y
        };
    }

    internal static bool GetBoxShadeFlag(int ghVehicleBox, int iValue)
    {
        throw new NotImplementedException();
    }

    internal static void GetBoxSize(int hBoxHandle, out Rectangle Dimensions)
    {
        if ((hBoxHandle < 0) || (hBoxHandle >= MAX_POPUP_BOX_COUNT))
        {
            Dimensions = new Rectangle();
            return;
        }

        Dimensions = PopUpBoxList[hBoxHandle].Dimensions;
    }

    internal static int GetLineSpace(int ghVehicleBox)
    {
        throw new NotImplementedException();
    }

    internal static int GetNumberOfLinesOfTextInBox(int ghVehicleBox)
    {
        throw new NotImplementedException();
    }

    internal static int GetTopMarginSize(int ghAssignmentBox)
    {
        throw new NotImplementedException();
    }

    internal static void HideBox(int ghEpcBox)
    {
        throw new NotImplementedException();
    }

    internal static void HighLightBoxLine(int ghVehicleBox, int iValue)
    {
        throw new NotImplementedException();
    }

    internal static bool IsBoxShown(int uiHandle)
    {
        if ((uiHandle < 0) || (uiHandle >= MAX_POPUP_BOX_COUNT))
        {
            return (false);
        }

        if (PopUpBoxList[uiHandle] == null)
        {
            return (false);
        }

        return (PopUpBoxList[uiHandle].fShowBox);
    }

    internal static void RestorePopUpBoxes()
    {
        throw new NotImplementedException();
    }

    internal static void SecondaryShadeStringInBox(int ghVehicleBox, int uiMenuLine)
    {
        throw new NotImplementedException();
    }

    internal static void SetBoxPosition(int ghVehicleBox, Point vehiclePosition)
    {
        throw new NotImplementedException();
    }

    internal static void SetBoxSecondaryShade(int ghVehicleBox, FontColor fONT_YELLOW)
    {
        throw new NotImplementedException();
    }

    internal static void SetCurrentBox(int ghVehicleBox)
    {
        throw new NotImplementedException();
    }

    internal static void ShowBox(int ghVehicleBox)
    {
        throw new NotImplementedException();
    }

    internal static void UnHighLightBox(int ghVehicleBox)
    {
        throw new NotImplementedException();
    }

    internal static void UnSecondaryShadeStringInBox(int ghVehicleBox, int uiMenuLine)
    {
        throw new NotImplementedException();
    }

    internal static void UnShadeStringInBox(int ghVehicleBox, int uiMenuLine)
    {
        throw new NotImplementedException();
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
