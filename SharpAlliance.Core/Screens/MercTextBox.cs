using System;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Screens;

public class MercTextBox
{

    public static ValueTask<bool> InitMercPopupBox()
    {
        return ValueTask.FromResult(true);
    }

    public static void RemoveMercPopupBoxFromIndex(int iBoxId)
    {
    }

    public static bool SetPrepareMercPopupFlags(MERC_POPUP_PREPARE_FLAGS uiFlags)
    {
        guiFlags |= uiFlags;
        return (true);

    }

    public static int PrepareMercPopupBox(int iId, MercTextBoxBackground ubMercBoxBackground, MercTextBoxBorder ubMercBoxBorder, string zString, int mSGBOX_DEFAULT_WIDTH, int v1, int v2, int v3, out int usTextBoxWidth, out int usTextBoxHeight)
    {
        usTextBoxHeight = 11;
        usTextBoxWidth = 56;
        return 0;
    }

    public static bool SetCurrentPopUpBox(int uiId)
    {
        // given id of the box, find it in the list and set to current

        //make sure the box id is valid
        if (uiId == (int)-1)
        {
            //Messages.ScreenMsg( FONT_MCOLOR_WHITE, MSG_BETAVERSION, "Error: Trying to set Current Popup Box using -1 as an ID" );
            return (false);
        }

        // see if box inited
        if (Globals.gpPopUpBoxList[uiId] != null)
        {
            Globals.gPopUpTextBox = Globals.gpPopUpBoxList[uiId];
            return (true);
        }
        return (false);
    }

    public static bool OverrideMercPopupBox(MercPopUpBox? pMercBox)
    {

        // store old box and set current this passed one
        Globals.gOldPopUpTextBox = Globals.gPopUpTextBox;

        Globals.gPopUpTextBox = pMercBox;

        return (true);
    }

    public static bool ResetOverrideMercPopupBox()
    {
        Globals.gPopUpTextBox = Globals.gOldPopUpTextBox;

        return (true);
    }

    public static bool RenderMercPopUpBoxFromIndex(int iBoxId, int sDestX, int sDestY, Surfaces uiBuffer)
    {

        // set the current box
        if (SetCurrentPopUpBox(iBoxId) == false)
        {
            return (false);
        }

        // now attempt to render the box
        return (RenderMercPopupBox(sDestX, sDestY, uiBuffer));
    }

    public static bool RenderMercPopupBox(int sDestX, int sDestY, Surfaces uiBuffer)
    {
        //	int  uiDestPitchBYTES;
        //	int  uiSrcPitchBYTES;
        //  int  *pDestBuf;
        //	int  *pSrcBuf;

        // chad, is me - might mess up flow, we'll see
        if (Globals.gPopUpTextBox is null)
        {
            return false;
        }

        // will render/transfer the image from the buffer in the data structure to the buffer specified by user
        bool fReturnValue = true;

        // grab the destination buffer
        //	pDestBuf = ( int* )LockVideoSurface( uiBuffer, &uiDestPitchBYTES );

        // now lock it
        //	pSrcBuf = ( int* )LockVideoSurface( gPopUpTextBox.uiSourceBufferIndex, &uiSrcPitchBYTES);


        //check to see if we are wanting to blit a transparent background
        if (Globals.gPopUpTextBox?.uiFlags.HasFlag(MERC_POPUP_PREPARE_FLAGS.TRANS_BACK) ?? false)
        {
            VideoSurfaceManager.BltVideoSurface(uiBuffer, ((Surfaces?)Globals.gPopUpTextBox?.uiSourceBufferIndex) ?? Surfaces.Unknown, 0, sDestX, sDestY, BlitTypes.FAST | BlitTypes.USECOLORKEY, null);
        }
        else
        {
            VideoSurfaceManager.BltVideoSurface(uiBuffer, ((Surfaces?)Globals.gPopUpTextBox?.uiSourceBufferIndex) ?? Surfaces.Unknown , 0, sDestX, sDestY, BlitTypes.FAST, null);
        }


        // blt, and grab return value
        //	fReturnValue = Blt16BPPTo16BPP(pDestBuf, uiDestPitchBYTES, pSrcBuf, uiSrcPitchBYTES, sDestX, sDestY, 0, 0, gPopUpTextBox.sWidth, gPopUpTextBox.sHeight);	

        //Invalidate!
        if (uiBuffer == Surfaces.FRAME_BUFFER)
        {
            VeldridVideoManager.InvalidateRegion(sDestX, sDestY, (int)(sDestX + Globals.gPopUpTextBox?.sWidth ?? 0), (int)(sDestY + Globals.gPopUpTextBox?.sHeight ?? 0));
        }

        // unlock the video surfaces

        // source
        //	UnLockVideoSurface( gPopUpTextBox.uiSourceBufferIndex );

        // destination
        //	UnLockVideoSurface( uiBuffer );

        // return success or failure
        return fReturnValue;
    }
}


// background enumeration
public enum MercTextBoxBackground
{
    BASIC_MERC_POPUP_BACKGROUND = 0,
    WHITE_MERC_POPUP_BACKGROUND,
    GREY_MERC_POPUP_BACKGROUND,
    DIALOG_MERC_POPUP_BACKGROUND,
    LAPTOP_POPUP_BACKGROUND,
    IMP_POPUP_BACKGROUND,
};

// border enumeration
public enum MercTextBoxBorder
{
    BASIC_MERC_POPUP_BORDER = 0,
    RED_MERC_POPUP_BORDER,
    BLUE_MERC_POPUP_BORDER,
    DIALOG_MERC_POPUP_BORDER,
    LAPTOP_POP_BORDER
};

public struct MercPopUpBox
{
    public int uiSourceBufferIndex;
    public int sWidth;
    public int sHeight;
    public int ubBackgroundIndex;
    public int ubBorderIndex;
    public int uiMercTextPopUpBackground;
    public int uiMercTextPopUpBorder;
    public bool fMercTextPopupInitialized;
    public bool fMercTextPopupSurfaceInitialized;
    public MERC_POPUP_PREPARE_FLAGS uiFlags;
}

[Flags]
public enum MERC_POPUP_PREPARE_FLAGS
{
    TRANS_BACK = 0x00000001,
    MARGINS = 0x00000002,
    STOPICON = 0x00000004,
    SKULLICON = 0x00000008,
}
