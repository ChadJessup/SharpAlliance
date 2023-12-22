using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Screens;

public class MercTextBox
{
    private readonly IVideoManager video;
    private readonly FontSubSystem fonts;
    private readonly ILogger<MercTextBox> logger;
    private readonly ITextureManager textures;
    private readonly IServiceProvider services;
    private readonly string[] zMercBorderPopupFilenames =
    {
        "INTERFACE\\TactPopUp.sti",
        "INTERFACE\\TactRedPopUp.sti",
        "INTERFACE\\TactBluePopUp.sti",
        "INTERFACE\\TactPopUpMain.sti",
        "INTERFACE\\LaptopPopup.sti",
    };

    // filenames for background popup .pcx's
    private readonly string[] zMercBackgroundPopupFilenames =
    {
        "INTERFACE\\TactPopupBackground.pcx",
        "INTERFACE\\TactPopupWhiteBackground.pcx",
        "INTERFACE\\TactPopupGreyBackground.pcx",
        "INTERFACE\\TactPopupBackgroundMain.pcx",
        "INTERFACE\\LaptopPopupBackground.pcx",
        "INTERFACE\\imp_popup_background.pcx",
    };

    // the pop up box structure
    private MercPopUpBox gBasicPopUpTextBox;

    // the current pop up box
    private MercPopUpBox gPopUpTextBox;

    // the old one
    private MercPopUpBox gOldPopUpTextBox;

    // the list of boxes
    private MercPopUpBox[] gpPopUpBoxList = new MercPopUpBox[MAX_NUMBER_OF_POPUP_BOXES];

    public MercTextBox(
        ILogger<MercTextBox> logger,
        IServiceProvider serviceProvider,
        FontSubSystem fontSubSystem,
        ITextureManager textureManager,
        IVideoManager videoManager)
    {
        this.textures = textureManager;
        this.video = videoManager;
        this.fonts = fontSubSystem;
        this.logger = logger;
        this.services = serviceProvider;
    }

    public static ValueTask<bool> InitMercPopupBox()
    {
        return ValueTask.FromResult(true);
    }

    public static bool RemoveMercPopupBoxFromIndex(int iBoxId)
    {
        return true;
    }

    public static bool SetPrepareMercPopupFlags(MERC_POPUP_PREPARE_FLAGS uiFlags)
    {
        guiFlags |= uiFlags;
        return (true);

    }

    public int PrepareMercPopupBox(
        int iBoxId, 
        MercTextBoxBackground ubBackgroundIndex, 
        MercTextBoxBorder ubBorderIndex, 
        string pString, 
        int usWidth, 
        int usMarginX, 
        int usMarginTopY, 
        int usMarginBottomY, 
        out int pActualWidth, 
        out int pActualHeight)
    {
        pActualHeight = 0;
        pActualWidth = 0;

        int usNumberVerticalPixels, usNumberOfLines;
        int usTextWidth, usHeight;
        ushort i;
        HVOBJECT? hImageHandle;
        ushort usPosY, usPosX;
        VSURFACE_DESC vs_desc;
        int usStringPixLength;
        Rectangle DestRect;
        int uiDestPitchBYTES;
        int uiSrcPitchBYTES;
        Image<Rgba32> pDestBuf;
        Image<Rgba32> pSrcBuf;
        Rgba32 usColorVal;
        int usLoopEnd;
        int sDispTextXPos;
        MercPopUpBox pPopUpTextBox;

        if (usWidth >= 640)
        {
            return (-1);
        }

        if (usWidth <= MERC_TEXT_MIN_WIDTH)
        {
            usWidth = MERC_TEXT_MIN_WIDTH;
        }

        // check id value, if -1, box has not been inited yet
        if (iBoxId == -1)
        {
            // no box yet

            // create box
            pPopUpTextBox = new();

            // copy over ptr
            gPopUpTextBox = pPopUpTextBox;

            // Load appropriate images
            if (LoadTextMercPopupImages(ubBackgroundIndex, ubBorderIndex) == false)
            {
                MemFree(pPopUpTextBox);
                return (-1);
            }

        }
        else
        {
            // has been created already, 
            // Check if these images are different

            // grab box
            pPopUpTextBox = GetPopUpBoxIndex(iBoxId);

            // copy over ptr
            gPopUpTextBox = pPopUpTextBox;

            if (ubBackgroundIndex != pPopUpTextBox.ubBackgroundIndex
                || ubBorderIndex != pPopUpTextBox.ubBorderIndex
                || !pPopUpTextBox.fMercTextPopupInitialized)
            {
                //Remove old, set new
                RemoveTextMercPopupImages();
                if (LoadTextMercPopupImages(ubBackgroundIndex, ubBorderIndex) == false)
                {
                    return (-1);
                }
            }
        }

        gPopUpTextBox.uiFlags = guiFlags;
        // reset flags
        guiFlags = 0;

        usStringPixLength = fonts.WFStringPixLength(pString, TEXT_POPUP_FONT);

        if (usStringPixLength < (usWidth - (MERC_TEXT_POPUP_WINDOW_TEXT_OFFSET_X) * 2))
        {
            usWidth = usStringPixLength + MERC_TEXT_POPUP_WINDOW_TEXT_OFFSET_X * 2;
            usTextWidth = usWidth - (MERC_TEXT_POPUP_WINDOW_TEXT_OFFSET_X) * 2 + 1;
        }
        else
        {
            usTextWidth = usWidth - (MERC_TEXT_POPUP_WINDOW_TEXT_OFFSET_X) * 2 + 1 - usMarginX;
        }

        usNumberVerticalPixels = WordWrap.IanWrappedStringHeight(
            0, 
            0, 
            usTextWidth, 
            2, 
            TEXT_POPUP_FONT, 
            MERC_TEXT_COLOR, 
            pString, 
            FontColor.FONT_MCOLOR_BLACK, 
            false, 
            TextJustifies.LEFT_JUSTIFIED);

        usNumberOfLines = usNumberVerticalPixels / TEXT_POPUP_GAP_BN_LINES;

        usHeight = usNumberVerticalPixels + MERC_TEXT_POPUP_WINDOW_TEXT_OFFSET_X * 2;

        // Add height for margins
        usHeight += usMarginTopY + usMarginBottomY;

        // Add width for margins
        usWidth += (usMarginX * 2);

        // Add width for iconic...
        if ((pPopUpTextBox.uiFlags.HasFlag(MERC_POPUP_PREPARE_FLAGS.STOPICON | MERC_POPUP_PREPARE_FLAGS.SKULLICON)))
        {
            // Make minimun height for box...
            if (usHeight < 45)
            {
                usHeight = 45;
            }
            usWidth += 35;
        }

        if (usWidth >= MERC_BACKGROUND_WIDTH)
        {
            usWidth = MERC_BACKGROUND_WIDTH - 1;
        }
        //make sure the area isnt bigger then the background texture
        if ((usWidth >= MERC_BACKGROUND_WIDTH) || usHeight >= MERC_BACKGROUND_HEIGHT)
        {
            if (iBoxId == -1)
            {
                MemFree(pPopUpTextBox);
            }

            return (-1);
        }
        // Create a background video surface to blt the face onto
        vs_desc.fCreateFlags = VSurfaceCreateFlags.VSURFACE_CREATE_DEFAULT | VSurfaceCreateFlags.VSURFACE_SYSTEM_MEM_USAGE;
        vs_desc.usWidth = usWidth;
        vs_desc.usHeight = usHeight;
        vs_desc.ubBitDepth = 16;
        // this.video.AddVideoObject(out vs_desc, out pPopUpTextBox.uiSourceBufferIndex);
        pPopUpTextBox.fMercTextPopupSurfaceInitialized = true;

        pPopUpTextBox.sWidth = usWidth;
        pPopUpTextBox.sHeight = usHeight;

        pActualWidth = usWidth;
        pActualHeight = usHeight;


        DestRect = new()
        {
            X = 0,
            Y = 0,
            Width = 0 + usWidth,
            Height = 0 + usHeight,
        };

        if (pPopUpTextBox.uiFlags.HasFlag(MERC_POPUP_PREPARE_FLAGS.TRANS_BACK))
        {
            // Zero with yellow,
            // Set source transparcenty
            video.SetVideoSurfaceTransparency(pPopUpTextBox.uiSourceBufferIndex, FROMRGB(255, 255, 0));

//            pDestBuf = video.LockVideoSurface(pPopUpTextBox.uiSourceBufferIndex, out uiDestPitchBYTES);

            usColorVal = new Rgba32(255, 255, 0);

            for (int x = 0; x < usWidth; x++)
            {
                for (int y = 0; y < usHeight; y++)
                {
//                    pDestBuf[x, y] = usColorVal;
                }
            }

//            video.UnLockVideoSurface(pPopUpTextBox.uiSourceBufferIndex);
        }
        else
        {
            if (!video.GetVideoSurface(out HVSURFACE hSrcVSurface, pPopUpTextBox.uiMercTextPopUpBackground))
            {
                //AssertMsg(0, "Failed to GetVideoSurface for PrepareMercPopupBox.  VSurfaceID:  %d",
                //    pPopUpTextBox.uiMercTextPopUpBackground);
            }

//            pDestBuf = video.LockVideoSurface(pPopUpTextBox.uiSourceBufferIndex, out uiDestPitchBYTES);
//            pSrcBuf =  video.LockVideoSurface(pPopUpTextBox.uiMercTextPopUpBackground, out uiSrcPitchBYTES);

//            video.Blt8BPPDataSubTo16BPPBuffer(pDestBuf, uiDestPitchBYTES, hSrcVSurface, pSrcBuf, uiSrcPitchBYTES, 0, 0, out DestRect);

//            video.UnLockVideoSurface(pPopUpTextBox.uiMercTextPopUpBackground);
//            video.UnLockVideoSurface(pPopUpTextBox.uiSourceBufferIndex);
        }

        hImageHandle = video.GetVideoObject(pPopUpTextBox.uiMercTextPopUpBorder);

        usPosX = usPosY = 0;
        //blit top row of images
        for (i = TEXT_POPUP_GAP_BN_LINES; i < usWidth - TEXT_POPUP_GAP_BN_LINES; i += TEXT_POPUP_GAP_BN_LINES)
        {
            //TOP ROW
            VideoObjectManager.BltVideoObject(pPopUpTextBox.uiSourceBufferIndex, hImageHandle, 1, i, usPosY, VO_BLT.SRCTRANSPARENCY, null);
            //BOTTOM ROW
            VideoObjectManager.BltVideoObject(pPopUpTextBox.uiSourceBufferIndex, hImageHandle, 6, i, usHeight - TEXT_POPUP_GAP_BN_LINES + 6, VO_BLT.SRCTRANSPARENCY, null);
        }

        //blit the left and right row of images
        usPosX = 0;
        for (i = TEXT_POPUP_GAP_BN_LINES; i < usHeight - TEXT_POPUP_GAP_BN_LINES; i += TEXT_POPUP_GAP_BN_LINES)
        {
            VideoObjectManager.BltVideoObject(pPopUpTextBox.uiSourceBufferIndex, hImageHandle, 3, usPosX, i, VO_BLT.SRCTRANSPARENCY, null);
            VideoObjectManager.BltVideoObject(pPopUpTextBox.uiSourceBufferIndex, hImageHandle, 4, usPosX + usWidth - 4, i, VO_BLT.SRCTRANSPARENCY, null);
        }

        //blt the corner images for the row
        //top left
        VideoObjectManager.BltVideoObject(pPopUpTextBox.uiSourceBufferIndex, hImageHandle, 0, 0, usPosY, VO_BLT.SRCTRANSPARENCY, null);
        //top right
        VideoObjectManager.BltVideoObject(pPopUpTextBox.uiSourceBufferIndex, hImageHandle, 2, usWidth - TEXT_POPUP_GAP_BN_LINES, usPosY, VO_BLT.SRCTRANSPARENCY, null);
        //bottom left
        VideoObjectManager.BltVideoObject(pPopUpTextBox.uiSourceBufferIndex, hImageHandle, 5, 0, usHeight - TEXT_POPUP_GAP_BN_LINES, VO_BLT.SRCTRANSPARENCY, null);
        //bottom right
        VideoObjectManager.BltVideoObject(pPopUpTextBox.uiSourceBufferIndex, hImageHandle, 7, usWidth - TEXT_POPUP_GAP_BN_LINES, usHeight - TEXT_POPUP_GAP_BN_LINES, VO_BLT.SRCTRANSPARENCY, null);

        // Icon if ness....
        if (pPopUpTextBox.uiFlags.HasFlag(MERC_POPUP_PREPARE_FLAGS.STOPICON))
        {
            video.BltVideoObjectFromIndex(pPopUpTextBox.uiSourceBufferIndex, guiBoxIcons, 0, 5, 4, VO_BLT.SRCTRANSPARENCY, null);
        }
        if (pPopUpTextBox.uiFlags.HasFlag(MERC_POPUP_PREPARE_FLAGS.SKULLICON))
        {
            video.BltVideoObjectFromIndex(pPopUpTextBox.uiSourceBufferIndex, guiSkullIcons, 0, 9, 4, VO_BLT.SRCTRANSPARENCY, null);
        }

        //Get the font and shadow colors
        GetMercPopupBoxFontColor(ubBackgroundIndex, out FontColor ubFontColor, out FontShadow ubFontShadowColor);

        FontSubSystem.SetFontShadow(ubFontShadowColor);
        this.fonts.SetFontDestBuffer(pPopUpTextBox.uiSourceBufferIndex, 0, 0, usWidth, usHeight, false);

        //Display the text
        sDispTextXPos = ((MERC_TEXT_POPUP_WINDOW_TEXT_OFFSET_X + usMarginX));

        if (pPopUpTextBox.uiFlags.HasFlag(MERC_POPUP_PREPARE_FLAGS.STOPICON | MERC_POPUP_PREPARE_FLAGS.SKULLICON))
        {
            sDispTextXPos += 30;
        }

        //Display the text
        FontSubSystem.DisplayWrappedString(
            new(sDispTextXPos, MERC_TEXT_POPUP_WINDOW_TEXT_OFFSET_Y + usMarginTopY),
            usTextWidth,
            2,
            MERC_TEXT_FONT,
            ubFontColor,
            pString,
            FontColor.FONT_MCOLOR_BLACK,
            TextJustifies.LEFT_JUSTIFIED);

        this.fonts.SetFontDestBuffer(SurfaceType.FRAME_BUFFER, 0, 0, 640, 480, false);
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

        if (iBoxId == -1)
        {
            // now return attemp to add to pop up box list, if successful will return index
            return (AddPopUpBoxToList(pPopUpTextBox));
        }
        else
        {
            // set as current box
            SetCurrentPopUpBox(iBoxId);

            return (iBoxId);
        }
    }

    public MercPopUpBox GetPopUpBoxIndex(int iId)
    {
        return gpPopUpBoxList[iId];
    }

    //Pass in the background index, and pointers to the font and shadow color
    private void GetMercPopupBoxFontColor(MercTextBoxBackground ubBackgroundIndex, out FontColor pubFontColor, out FontShadow pubFontShadowColor)
    {
        switch (ubBackgroundIndex)
        {
            case MercTextBoxBackground.BASIC_MERC_POPUP_BACKGROUND:
                pubFontColor = TEXT_POPUP_COLOR;
                pubFontShadowColor = FontShadow.DEFAULT_SHADOW;
                break;

            case MercTextBoxBackground.WHITE_MERC_POPUP_BACKGROUND:
                pubFontColor = (FontColor)2;
                pubFontShadowColor = (FontShadow)FontColor.FONT_MCOLOR_WHITE;
                break;

            case MercTextBoxBackground.GREY_MERC_POPUP_BACKGROUND:
                pubFontColor = (FontColor)2;
                pubFontShadowColor = FontShadow.NO_SHADOW;
                break;

            case MercTextBoxBackground.LAPTOP_POPUP_BACKGROUND:
                pubFontColor = TEXT_POPUP_COLOR;
                pubFontShadowColor = FontShadow.DEFAULT_SHADOW;
                break;

            default:
                pubFontColor = TEXT_POPUP_COLOR;
                pubFontShadowColor = FontShadow.DEFAULT_SHADOW;
                break;
        }
    }

    public void RemoveTextMercPopupImages()
    {
        //this procedure will remove the background and border video surface/object from the indecies
        if (true)//gPopUpTextBox )
        {
            if (gPopUpTextBox.fMercTextPopupInitialized)
            {
                // the background
                video.DeleteVideoObjectFromIndex(gPopUpTextBox.uiMercTextPopUpBackground);

                // the border
                video.DeleteVideoObjectFromIndex(gPopUpTextBox.uiMercTextPopUpBorder);

                gPopUpTextBox.fMercTextPopupInitialized = false;
            }
        }

        // done
        return;
    }

    private int AddPopUpBoxToList(MercPopUpBox pPopUpTextBox)
    {
        // attempt to add box to list
        for (int iCounter = 0; iCounter < MAX_NUMBER_OF_POPUP_BOXES; iCounter++)
        {
            if (gpPopUpBoxList[iCounter] == null || !gpPopUpBoxList[iCounter].IsInitialized)
            {
                // found a spot, inset
                gpPopUpBoxList[iCounter] = pPopUpTextBox;

                // set as current
                SetCurrentPopUpBox(iCounter);

                // return index value
                return (iCounter);
            }
        }

        // return failure
        return (-1);
    }

    // Tactical Popup
    private bool LoadTextMercPopupImages(MercTextBoxBackground ubBackgroundIndex, MercTextBoxBorder ubBorderIndex)
    {
        VSURFACE_DESC vs_desc;
        VOBJECT_DESC VObjectDesc = new();

        // this function will load the graphics associated with the background and border index values

        // the background
        vs_desc.fCreateFlags = VSurfaceCreateFlags.VSURFACE_CREATE_FROMFILE | VSurfaceCreateFlags.VSURFACE_SYSTEM_MEM_USAGE;


       // Image<Rgba32> backgroundImage = this.textures.LoadTexture(zMercBackgroundPopupFilenames[(int)ubBackgroundIndex]);
       // Image<Rgba32> backgroundImage = video.AddVideoSurface(zMercBackgroundPopupFilenames[(int)ubBackgroundIndex], out var popupboxSurface);
        //gPopUpTextBox.uiMercTextPopUpBackground = popupboxSurface;

        // border
        VObjectDesc.ImageFile = Utils.FilenameForBPP(zMercBorderPopupFilenames[(int)ubBorderIndex]);
        var borderImage = video.GetVideoObject(VObjectDesc.ImageFile, out var key);

        gPopUpTextBox.uiMercTextPopUpBorder = key;
        gPopUpTextBox.fMercTextPopupInitialized = true;

        // so far so good, return successful
        gPopUpTextBox.ubBackgroundIndex = ubBackgroundIndex;
        gPopUpTextBox.ubBorderIndex = ubBorderIndex;

        return true;
    }

    public bool SetCurrentPopUpBox(int uiId)
    {
        // given id of the box, find it in the list and set to current

        //make sure the box id is valid
        if (uiId == (int)-1)
        {
            //Messages.ScreenMsg( FONT_MCOLOR_WHITE, MSG.BETAVERSION, "Error: Trying to set Current Popup Box using -1 as an ID" );
            return (false);
        }

        // see if box inited
        if (gpPopUpBoxList[uiId].IsInitialized)
        {
            gPopUpTextBox = gpPopUpBoxList[uiId];
            return (true);
        }

        return (false);
    }

    public bool OverrideMercPopupBox(MercPopUpBox pMercBox)
    {

        // store old box and set current this passed one
        gOldPopUpTextBox = gPopUpTextBox;

        gPopUpTextBox = pMercBox;

        return (true);
    }

    public bool ResetOverrideMercPopupBox()
    {
        gPopUpTextBox = gOldPopUpTextBox;

        return (true);
    }

    public bool RenderMercPopUpBoxFromIndex(int iBoxId, int sDestX, int sDestY, SurfaceType uiBuffer)
    {

        // set the current box
        if (SetCurrentPopUpBox(iBoxId) == false)
        {
            return (false);
        }

        // now attempt to render the box
        return (RenderMercPopupBox(sDestX, sDestY, uiBuffer));
    }

    public bool RenderMercPopupBox(int sDestX, int sDestY, SurfaceType uiBuffer)
    {
        //	int  uiDestPitchBYTES;
        //	int  uiSrcPitchBYTES;
        //  int  *pDestBuf;
        //	int  *pSrcBuf;

        // chad, is me - might mess up flow, we'll see
        if (!gPopUpTextBox.IsInitialized)
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
        if (gPopUpTextBox.uiFlags.HasFlag(MERC_POPUP_PREPARE_FLAGS.TRANS_BACK))
        {
            VideoSurfaceManager.BltVideoSurface(uiBuffer, ((SurfaceType?)gPopUpTextBox.uiSourceBufferIndex) ?? SurfaceType.Unknown, 0, sDestX, sDestY, BlitTypes.FAST | BlitTypes.USECOLORKEY, null);
        }
        else
        {
            VideoSurfaceManager.BltVideoSurface(uiBuffer, ((SurfaceType?)gPopUpTextBox.uiSourceBufferIndex) ?? SurfaceType.Unknown , 0, sDestX, sDestY, BlitTypes.FAST, null);
        }


        // blt, and grab return value
        //	fReturnValue = Blt16BPPTo16BPP(pDestBuf, uiDestPitchBYTES, pSrcBuf, uiSrcPitchBYTES, sDestX, sDestY, 0, 0, gPopUpTextBox.sWidth, gPopUpTextBox.sHeight);	

        //Invalidate!
        if (uiBuffer == SurfaceType.FRAME_BUFFER)
        {
            video.InvalidateRegion(sDestX, sDestY, (int)(sDestX + gPopUpTextBox.sWidth), (int)(sDestY + gPopUpTextBox.sHeight));
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

public class MercPopUpBox
{
    public SurfaceType uiSourceBufferIndex { get; set; }
    public int sWidth { get; set; }
    public int sHeight { get; set; }
    public MercTextBoxBackground ubBackgroundIndex { get; set; }
    public MercTextBoxBorder ubBorderIndex { get; set; }
    public SurfaceType uiMercTextPopUpBackground { get; set; }
    public string uiMercTextPopUpBorder { get; set; }
    public bool fMercTextPopupInitialized { get; set; }
    public bool fMercTextPopupSurfaceInitialized { get; set; }
    public MERC_POPUP_PREPARE_FLAGS uiFlags { get; set; }

    public bool IsInitialized { get; internal set; }
}

[Flags]
public enum MERC_POPUP_PREPARE_FLAGS
{
    TRANS_BACK = 0x00000001,
    MARGINS = 0x00000002,
    STOPICON = 0x00000004,
    SKULLICON = 0x00000008,
}
