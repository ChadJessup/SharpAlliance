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
        return true;

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
        Image<Rgba32> pDestBuf;
        Image<Rgba32> pSrcBuf;
        Rgba32 usColorVal;
        int usLoopEnd;
        int sDispTextXPos;
        MercPopUpBox pPopUpTextBox = new();

        if (usWidth >= 640)
        {
            return -1;
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
            this.gPopUpTextBox = pPopUpTextBox;

            // Load appropriate images
            if (this.LoadTextMercPopupImages(ubBackgroundIndex, ubBorderIndex) == false)
            {
                MemFree(pPopUpTextBox);
                return -1;
            }

        }
        else
        {
            // has been created already, 
            // Check if these images are different

            // grab box
            pPopUpTextBox = this.GetPopUpBoxIndex(iBoxId);

            // copy over ptr
            this.gPopUpTextBox = pPopUpTextBox;

            if (ubBackgroundIndex != pPopUpTextBox.ubBackgroundIndex
                || ubBorderIndex != pPopUpTextBox.ubBorderIndex
                || !pPopUpTextBox.fMercTextPopupInitialized)
            {
                //Remove old, set new
                this.RemoveTextMercPopupImages();
                if (this.LoadTextMercPopupImages(ubBackgroundIndex, ubBorderIndex) == false)
                {
                    return -1;
                }
            }
        }

        this.gPopUpTextBox.uiFlags = guiFlags;
        // reset flags
        guiFlags = 0;

        usStringPixLength = FontSubSystem.WFStringPixLength(pString, TEXT_POPUP_FONT);

        if (usStringPixLength < (usWidth - MERC_TEXT_POPUP_WINDOW_TEXT_OFFSET_X * 2))
        {
            usWidth = usStringPixLength + MERC_TEXT_POPUP_WINDOW_TEXT_OFFSET_X * 2;
            usTextWidth = usWidth - MERC_TEXT_POPUP_WINDOW_TEXT_OFFSET_X * 2 + 1;
        }
        else
        {
            usTextWidth = usWidth - MERC_TEXT_POPUP_WINDOW_TEXT_OFFSET_X * 2 + 1 - usMarginX;
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
        usWidth += usMarginX * 2;

        // Add width for iconic...
        if (pPopUpTextBox.uiFlags.HasFlag(MERC_POPUP_PREPARE_FLAGS.STOPICON | MERC_POPUP_PREPARE_FLAGS.SKULLICON))
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

            return -1;
        }
        // Create a background video surface to blt the face onto
        vs_desc = new()
        {
            fCreateFlags = VSurfaceCreateFlags.VSURFACE_CREATE_DEFAULT | VSurfaceCreateFlags.VSURFACE_SYSTEM_MEM_USAGE,
            usWidth = usWidth,
            usHeight = usHeight,
            ubBitDepth = 16
        };

        Texture texture = this.video.Surfaces.CreateSurface(vs_desc);
        pPopUpTextBox.uiSourceBufferIndex = texture.SurfaceType;

        pPopUpTextBox.fMercTextPopupSurfaceInitialized = true;
        pPopUpTextBox.Size = new(usWidth, usHeight);

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
            this.video.SetVideoSurfaceTransparency(pPopUpTextBox.uiSourceBufferIndex, FROMRGB(255, 255, 0));

            usColorVal = new Rgba32(255, 255, 0);

            for (int x = 0; x < usWidth; x++)
            {
                for (int y = 0; y < usHeight; y++)
                {
                    texture.Image[x, y] = usColorVal;
                }
            }
        }
        else
        {
            pDestBuf = this.video.Surfaces[pPopUpTextBox.uiMercTextPopUpBackground];
            pSrcBuf = this.video.Surfaces[pPopUpTextBox.uiMercTextPopUpBackground];

            video.Blt8BPPDataSubTo16BPPBuffer(pDestBuf, new(pSrcBuf.Width, pSrcBuf.Height), pSrcBuf, 0, 0, out DestRect);
        }

        hImageHandle = this.video.GetVideoObject(pPopUpTextBox.uiMercTextPopUpBorder);

        usPosX = usPosY = 0;
        //blit top row of images
        for (i = TEXT_POPUP_GAP_BN_LINES; i < usWidth - TEXT_POPUP_GAP_BN_LINES; i += TEXT_POPUP_GAP_BN_LINES)
        {
            //TOP ROW

            this.video.BltVideoObject(pPopUpTextBox.uiSourceBufferIndex, hImageHandle, 1, i, usPosY, VO_BLT.SRCTRANSPARENCY, null);
            //BOTTOM ROW
            this.video.BltVideoObject(pPopUpTextBox.uiSourceBufferIndex, hImageHandle, 6, i, usHeight - TEXT_POPUP_GAP_BN_LINES + 6, VO_BLT.SRCTRANSPARENCY, null);
        }

        //blit the left and right row of images
        usPosX = 0;
        for (i = TEXT_POPUP_GAP_BN_LINES; i < usHeight - TEXT_POPUP_GAP_BN_LINES; i += TEXT_POPUP_GAP_BN_LINES)
        {
            this.video.BltVideoObject(pPopUpTextBox.uiSourceBufferIndex, hImageHandle, 3, usPosX, i, VO_BLT.SRCTRANSPARENCY, null);
            this.video.BltVideoObject(pPopUpTextBox.uiSourceBufferIndex, hImageHandle, 4, usPosX + usWidth - 4, i, VO_BLT.SRCTRANSPARENCY, null);
        }

        //blt the corner images for the row
        //top left
        this.video.BltVideoObject(pPopUpTextBox.uiSourceBufferIndex, hImageHandle, 0, 0, usPosY, VO_BLT.SRCTRANSPARENCY, null);
        //top right
        this.video.BltVideoObject(pPopUpTextBox.uiSourceBufferIndex, hImageHandle, 2, usWidth - TEXT_POPUP_GAP_BN_LINES, usPosY, VO_BLT.SRCTRANSPARENCY, null);
        //bottom left
        this.video.BltVideoObject(pPopUpTextBox.uiSourceBufferIndex, hImageHandle, 5, 0, usHeight - TEXT_POPUP_GAP_BN_LINES, VO_BLT.SRCTRANSPARENCY, null);
        //bottom right
        this.video.BltVideoObject(pPopUpTextBox.uiSourceBufferIndex, hImageHandle, 7, usWidth - TEXT_POPUP_GAP_BN_LINES, usHeight - TEXT_POPUP_GAP_BN_LINES, VO_BLT.SRCTRANSPARENCY, null);

        // Icon if ness....
        if (pPopUpTextBox.uiFlags.HasFlag(MERC_POPUP_PREPARE_FLAGS.STOPICON))
        {
            this.video.BltVideoObjectFromIndex(pPopUpTextBox.uiSourceBufferIndex, guiBoxIcons, 0, 5, 4, VO_BLT.SRCTRANSPARENCY, null);
        }
        if (pPopUpTextBox.uiFlags.HasFlag(MERC_POPUP_PREPARE_FLAGS.SKULLICON))
        {
            this.video.BltVideoObjectFromIndex(pPopUpTextBox.uiSourceBufferIndex, guiSkullIcons, 0, 9, 4, VO_BLT.SRCTRANSPARENCY, null);
        }

        //Get the font and shadow colors
        this.GetMercPopupBoxFontColor(ubBackgroundIndex, out FontColor ubFontColor, out FontShadow ubFontShadowColor);

        FontSubSystem.SetFontShadow(ubFontShadowColor);
        FontSubSystem.SetFontDestBuffer(pPopUpTextBox.uiSourceBufferIndex, 0, 0, usWidth, usHeight, false);

        //Display the text
        sDispTextXPos = MERC_TEXT_POPUP_WINDOW_TEXT_OFFSET_X + usMarginX;

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

        FontSubSystem.SetFontDestBuffer(SurfaceType.FRAME_BUFFER, 0, 0, 640, 480, false);
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

        if (iBoxId == -1)
        {
            // now return attemp to add to pop up box list, if successful will return index
            return this.AddPopUpBoxToList(pPopUpTextBox);
        }
        else
        {
            // set as current box
            this.SetCurrentPopUpBox(iBoxId);

            return iBoxId;
        }
    }

    public MercPopUpBox GetPopUpBoxIndex(int iId)
    {
        return this.gpPopUpBoxList[iId];
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
            if (this.gPopUpTextBox.fMercTextPopupInitialized)
            {
                // the background
                this.video.DeleteVideoObjectFromIndex(this.gPopUpTextBox.uiMercTextPopUpBackground);

                // the border
                this.video.DeleteVideoObjectFromIndex(this.gPopUpTextBox.uiMercTextPopUpBorder);

                this.gPopUpTextBox.fMercTextPopupInitialized = false;
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
            if (this.gpPopUpBoxList[iCounter] == null)
            {
                // found a spot, inset
                this.gpPopUpBoxList[iCounter] = pPopUpTextBox;

                // set as current
                this.SetCurrentPopUpBox(iCounter);

                // return index value
                return iCounter;
            }
        }

        // return failure
        return -1;
    }

    // Tactical Popup
    private bool LoadTextMercPopupImages(MercTextBoxBackground ubBackgroundIndex, MercTextBoxBorder ubBorderIndex)
    {
        // this function will load the graphics associated with the background and border index values

        var VObjectDesc = video.GetVideoObject(zMercBackgroundPopupFilenames[(int)ubBackgroundIndex], out var popupboxSurface);
        SurfaceType surf = this.video.Surfaces.CreateSurface(VObjectDesc);
        gPopUpTextBox.uiMercTextPopUpBackground = surf;

        // border
        var borderFilePath = Utils.FilenameForBPP(this.zMercBorderPopupFilenames[(int)ubBorderIndex]);
        var borderImage = this.video.GetVideoObject(borderFilePath, out var key);

        this.gPopUpTextBox.uiMercTextPopUpBorder = key;
        this.gPopUpTextBox.fMercTextPopupInitialized = true;

        // so far so good, return successful
        this.gPopUpTextBox.ubBackgroundIndex = ubBackgroundIndex;
        this.gPopUpTextBox.ubBorderIndex = ubBorderIndex;

        return true;
    }

    public bool SetCurrentPopUpBox(int uiId)
    {
        // given id of the box, find it in the list and set to current

        //make sure the box id is valid
        if (uiId == (int)-1)
        {
            //Messages.ScreenMsg( FONT_MCOLOR_WHITE, MSG.BETAVERSION, "Error: Trying to set Current Popup Box using -1 as an ID" );
            return false;
        }

        // see if box inited
        if (this.gpPopUpBoxList[uiId] is not null)
        {
            this.gPopUpTextBox = this.gpPopUpBoxList[uiId];
            return true;
        }

        return false;
    }

    public bool OverrideMercPopupBox(MercPopUpBox pMercBox)
    {

        // store old box and set current this passed one
        this.gOldPopUpTextBox = this.gPopUpTextBox;

        this.gPopUpTextBox = pMercBox;

        return true;
    }

    public bool ResetOverrideMercPopupBox()
    {
        this.gPopUpTextBox = this.gOldPopUpTextBox;

        return true;
    }

    public bool RenderMercPopUpBoxFromIndex(int iBoxId, Point sDest, SurfaceType uiBuffer)
    {

        // set the current box
        if (this.SetCurrentPopUpBox(iBoxId) == false)
        {
            return false;
        }

        // now attempt to render the box
        return this.RenderMercPopupBox(sDest, uiBuffer);
    }

    public bool RenderMercPopupBox(Point sDest, SurfaceType uiBuffer)
    {
        // will render/transfer the image from the buffer in the data structure to the buffer specified by user
        bool fReturnValue = true;

        // grab the destination buffer
        //	pDestBuf = ( int* )LockVideoSurface( uiBuffer, &uiDestPitchBYTES );

        // now lock it
        //	pSrcBuf = ( int* )LockVideoSurface( gPopUpTextBox.uiSourceBufferIndex, &uiSrcPitchBYTES);


        //check to see if we are wanting to blit a transparent background
        if (this.gPopUpTextBox.uiFlags.HasFlag(MERC_POPUP_PREPARE_FLAGS.TRANS_BACK))
        {
            this.video.BltVideoSurface(
                uiBuffer,
                ((SurfaceType?)this.gPopUpTextBox.uiSourceBufferIndex) ?? SurfaceType.Unknown,
                0,
                sDest,
                BlitTypes.FAST | BlitTypes.USECOLORKEY,
                null);
        }
        else
        {
            this.video.BltVideoSurface(
                uiBuffer,
                ((SurfaceType?)this.gPopUpTextBox.uiSourceBufferIndex) ?? SurfaceType.Unknown,
                0,
                sDest,
                BlitTypes.FAST,
                null);
        }


        // blt, and grab return value
        //	fReturnValue = Blt16BPPTo16BPP(pDestBuf, uiDestPitchBYTES, pSrcBuf, uiSrcPitchBYTES, sDestX, sDestY, 0, 0, gPopUpTextBox.sWidth, gPopUpTextBox.sHeight);	

        //Invalidate!
        if (uiBuffer == SurfaceType.FRAME_BUFFER)
        {
            this.video.InvalidateRegion(new(sDest, this.gPopUpTextBox.Size));
        }

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
    public Size Size { get; set; }
    public MercTextBoxBackground ubBackgroundIndex { get; set; }
    public MercTextBoxBorder ubBorderIndex { get; set; }
    public SurfaceType uiMercTextPopUpBackground { get; set; }
    public string uiMercTextPopUpBorder { get; set; }
    public bool fMercTextPopupInitialized { get; set; }
    public bool fMercTextPopupSurfaceInitialized { get; set; }
    public MERC_POPUP_PREPARE_FLAGS uiFlags { get; set; }
}

[Flags]
public enum MERC_POPUP_PREPARE_FLAGS
{
    TRANS_BACK = 0x00000001,
    MARGINS = 0x00000002,
    STOPICON = 0x00000004,
    SKULLICON = 0x00000008,
}
