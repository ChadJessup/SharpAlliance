﻿using System;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.Screens;
using SharpAlliance.Platform;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core.SubSystems;

public delegate void MSGBOX_CALLBACK(MessageBoxReturnCode bExitValue);

public class MessageBoxSubSystem : ISharpAllianceManager
{
    public const int MSGBOX_DEFAULT_WIDTH = 300;
    public const int MSGBOX_BUTTON_WIDTH = 61;
    public const int MSGBOX_BUTTON_HEIGHT = 20;
    public const int MSGBOX_BUTTON_X_SEP = 15;
    public const int MSGBOX_SMALL_BUTTON_WIDTH = 31;
    public const int MSGBOX_SMALL_BUTTON_X_SEP = 8;

    private readonly GameContext context;
    private readonly IClockManager clock;
    private readonly MouseSubSystem mouse;
    private readonly IVideoManager video;
    private readonly ButtonSubSystem buttons;
    private static GameSettings gGameSettings;
    private static MercTextBox? mercTextBox;
    private static IScreenManager screens;
    private readonly Overhead overhead;
    private readonly RenderWorld renderWorld;
    private static CursorSubSystem cursor;
    private static IInputManager inputs;

    private static MapScreen mapScreen;
    private static FadeScreen fadeScreen;

    // if the cursor was locked to a region
    internal static bool fCursorLockedToArea = false;

    //extern bool fMapExitDueToMessageBox;
    public static bool fInMapMode { get; private set; }

    public MessageBoxSubSystem(
        GameContext context,
        MouseSubSystem mouseSubSystem,
        CursorSubSystem cursorSubSystem,
        MercTextBox mercTextBox,
        ButtonSubSystem buttonSubSystem,
        RenderWorld renderWorld,
        IInputManager inputManager,
        IVideoManager videoManager,
        IScreenManager screenManager,
        //        IClockManager clockManager,
        Overhead overhead,
        GameSettings gameSettings)
    {
        video = videoManager;
        this.buttons = buttonSubSystem;
        gGameSettings = gameSettings;
        MessageBoxSubSystem.mercTextBox = mercTextBox;
        this.renderWorld = renderWorld;
        cursor = cursorSubSystem;
        this.mouse = mouseSubSystem;
        screens = screenManager;
        inputs = inputManager;
        this.overhead = overhead;
        this.context = context;
    }

    public void Dispose()
    {
    }

    public async ValueTask<bool> Initialize()
    {
        mapScreen = await screens.GetScreen<MapScreen>(ScreenName.MAP_SCREEN, activate: false);
        fadeScreen = await screens.GetScreen<FadeScreen>(ScreenName.FADE_SCREEN, activate: false);

        return true;
    }

    public int DoMessageBox(MessageBoxStyle ubStyle, string zString, ScreenName uiExitScreen, MSG_BOX_FLAG usFlags, MSGBOX_CALLBACK? ReturnCallback, ref Rectangle? pCenteringRect)
    {
        VSURFACE_DESC vs_desc;
        Rectangle aRect = new();
        int sButtonX, sButtonY, sBlankSpace;
        MercTextBoxBackground ubMercBoxBackground = MercTextBoxBackground.BASIC_MERC_POPUP_BACKGROUND;
        MercTextBoxBorder ubMercBoxBorder = MercTextBoxBorder.BASIC_MERC_POPUP_BORDER;
        FontColor ubFontColor;
        FontShadow ubFontShadowColor;
        CURSOR usCursor;
        int iId = -1;

        inputs.GetCursorPosition(out pOldMousePosition);

        //this variable can be unset if ur in a non gamescreen and DONT want the msg box to use the save buffer
        gfDontOverRideSaveBuffer = true;

        CursorSubSystem.SetCurrentCursorFromDatabase(CURSOR.NORMAL);

        if (gMsgBox.BackRegion.uiFlags.HasFlag(MouseRegionFlags.REGION_EXISTS))
        {
            return 0;
        }

        // Based on style....
        switch (ubStyle)
        {
            //default
            case MessageBoxStyle.MSG_BOX_BASIC_STYLE:

                ubMercBoxBackground = MercTextBoxBackground.DIALOG_MERC_POPUP_BACKGROUND;
                ubMercBoxBorder = MercTextBoxBorder.DIALOG_MERC_POPUP_BORDER;

                // Add button images
                gMsgBox.iButtonImages = ButtonSubSystem.LoadButtonImage("INTERFACE\\popupbuttons.sti", -1, 0, -1, 1, -1);
                ubFontColor = FontColor.FONT_MCOLOR_WHITE;
                ubFontShadowColor = FontShadow.DEFAULT_SHADOW;
                usCursor = CURSOR.NORMAL;

                break;

            case MessageBoxStyle.MSG_BOX_RED_ON_WHITE:
                ubMercBoxBackground = MercTextBoxBackground.WHITE_MERC_POPUP_BACKGROUND;
                ubMercBoxBorder = MercTextBoxBorder.RED_MERC_POPUP_BORDER;

                // Add button images
                gMsgBox.iButtonImages = ButtonSubSystem.LoadButtonImage("INTERFACE\\msgboxRedButtons.sti", -1, 0, -1, 1, -1);

                ubFontColor = (FontColor)2;
                ubFontShadowColor = FontShadow.NO_SHADOW;
                usCursor = CURSOR.LAPTOP_SCREEN;
                break;

            case MessageBoxStyle.MSG_BOX_BLUE_ON_GREY:
                ubMercBoxBackground = MercTextBoxBackground.GREY_MERC_POPUP_BACKGROUND;
                ubMercBoxBorder = MercTextBoxBorder.BLUE_MERC_POPUP_BORDER;

                // Add button images
                gMsgBox.iButtonImages = ButtonSubSystem.LoadButtonImage("INTERFACE\\msgboxGreyButtons.sti", -1, 0, -1, 1, -1);

                ubFontColor = (FontColor)2;
                ubFontShadowColor = (FontShadow)FontColor.FONT_MCOLOR_WHITE;
                usCursor = CURSOR.LAPTOP_SCREEN;
                break;
            case MessageBoxStyle.MSG_BOX_IMP_STYLE:
                ubMercBoxBackground = MercTextBoxBackground.IMP_POPUP_BACKGROUND;
                ubMercBoxBorder = MercTextBoxBorder.DIALOG_MERC_POPUP_BORDER;

                // Add button images
                gMsgBox.iButtonImages = ButtonSubSystem.LoadButtonImage("INTERFACE\\msgboxGreyButtons.sti", -1, 0, -1, 1, -1);

                ubFontColor = (FontColor)2;
                ubFontShadowColor = (FontShadow)FontColor.FONT_MCOLOR_WHITE;
                usCursor = CURSOR.LAPTOP_SCREEN;
                break;
            case MessageBoxStyle.MSG_BOX_BASIC_SMALL_BUTTONS:

                ubMercBoxBackground = MercTextBoxBackground.DIALOG_MERC_POPUP_BACKGROUND;
                ubMercBoxBorder = MercTextBoxBorder.DIALOG_MERC_POPUP_BORDER;

                // Add button images
                gMsgBox.iButtonImages = ButtonSubSystem.LoadButtonImage("INTERFACE\\popupbuttons.sti", -1, 2, -1, 3, -1);
                ubFontColor = FontColor.FONT_MCOLOR_WHITE;
                ubFontShadowColor = FontShadow.DEFAULT_SHADOW;
                usCursor = CURSOR.NORMAL;

                break;

            case MessageBoxStyle.MSG_BOX_LAPTOP_DEFAULT:
                ubMercBoxBackground = MercTextBoxBackground.LAPTOP_POPUP_BACKGROUND;
                ubMercBoxBorder = MercTextBoxBorder.LAPTOP_POP_BORDER;

                // Add button images
                gMsgBox.iButtonImages = ButtonSubSystem.LoadButtonImage("INTERFACE\\popupbuttons.sti", -1, 0, -1, 1, -1);
                ubFontColor = FontColor.FONT_MCOLOR_WHITE;
                ubFontShadowColor = FontShadow.DEFAULT_SHADOW;
                usCursor = CURSOR.LAPTOP_SCREEN;
                break;

            default:
                ubMercBoxBackground = MercTextBoxBackground.BASIC_MERC_POPUP_BACKGROUND;
                ubMercBoxBorder = MercTextBoxBorder.BASIC_MERC_POPUP_BORDER;

                // Add button images
                gMsgBox.iButtonImages = ButtonSubSystem.LoadButtonImage("INTERFACE\\msgboxbuttons.sti", -1, 0, -1, 1, -1);
                ubFontColor = FontColor.FONT_MCOLOR_WHITE;
                ubFontShadowColor = FontShadow.DEFAULT_SHADOW;
                usCursor = CURSOR.NORMAL;
                break;
        }

        if (usFlags.HasFlag(MSG_BOX_FLAG.USE_CENTERING_RECT) && pCenteringRect is not null)
        {
            aRect.Y = pCenteringRect.Value.Y;
            aRect.X = pCenteringRect.Value.X;
            aRect.Height = pCenteringRect.Value.Height;
            aRect.Width = pCenteringRect.Value.Width;
        }
        else
        {
            // Use default!
            aRect.Y = 0;
            aRect.X = 0;
            aRect.Height = 480;
            aRect.Width = 640;
        }

        // Set some values!
        gMsgBox.usFlags = usFlags;
        gMsgBox.uiExitScreen = uiExitScreen;
        gMsgBox.ExitCallback = ReturnCallback;
        gMsgBox.fRenderBox = true;
        gMsgBox.bHandled = 0;

        // Init message box
        gMsgBox.iBoxId = mercTextBox.PrepareMercPopupBox(
            iId, 
            ubMercBoxBackground, 
            ubMercBoxBorder, 
            zString, 
            MSGBOX_DEFAULT_WIDTH, 
            40, 
            10, 
            30, 
            out int usTextBoxWidth, 
            out int usTextBoxHeight);

        if (gMsgBox.iBoxId == -1)
        {
            return 0;
        }

        // Save height,width
        gMsgBox.Size = new(usTextBoxWidth, usTextBoxHeight);

        // Determine position ( centered in rect )
        gMsgBox.Location.X = ((aRect.Width - aRect.X - usTextBoxWidth) / 2) + aRect.X;
        gMsgBox.Location.Y = ((aRect.Height - aRect.Y - usTextBoxHeight) / 2) + aRect.Y;

        if (screens.CurrentScreenName == ScreenName.GAME_SCREEN)
        {
            gfStartedFromGameScreen = true;
        }

        if (fInMapMode == true)
        {
            //		fMapExitDueToMessageBox = true;
            gfStartedFromMapScreen = true;
            fMapPanelDirty = true;
        }


        // Set pending screen
        screens.SetPendingNewScreen(ScreenName.MSG_BOX_SCREEN);

        // Init save buffer
        vs_desc = new()
        {
            fCreateFlags = VSurfaceCreateFlags.VSURFACE_CREATE_DEFAULT | VSurfaceCreateFlags.VSURFACE_SYSTEM_MEM_USAGE,
            usWidth = usTextBoxWidth,
            usHeight = usTextBoxHeight,
            ubBitDepth = 16
        };

        var texture = video.Surfaces.CreateSurface(vs_desc);

        gMsgBox.uiSaveBuffer = texture.SurfaceType;

        //Save what we have under here...
        var pDestBuf = video.Surfaces[gMsgBox.uiSaveBuffer];
        var pSrcBuf = video.Surfaces[SurfaceType.FRAME_BUFFER];

//        video.BlitSurfaceToSurface(pDestBuf, SurfaceType.FRAME_BUFFER, gMsgBox.Location);
          video.Blt16BPPTo16BPP(
              pDestBuf,
              pSrcBuf,
              new(0, 0),
              gMsgBox.Location,
              new(usTextBoxWidth, usTextBoxHeight),
              debug: false);

        // Create top-level mouse region
        MouseSubSystem.MSYS_DefineRegion(
            gMsgBox.BackRegion,
            new(0, 0, 640, 480),
            MSYS_PRIORITY.HIGHEST,
            usCursor,
            null,
            MsgBoxClickCallback);

        if (gGameSettings[TOPTION.DONT_MOVE_MOUSE] == false)
        {
            if (usFlags.HasFlag(MSG_BOX_FLAG.OK))
            {
                MouseSubSystem.SimulateMouseMovement(new(gMsgBox.Location.X + (usTextBoxWidth / 2) + 27, gMsgBox.Location.Y + (usTextBoxHeight - 10)));
            }
            else
            {
                MouseSubSystem.SimulateMouseMovement(new(gMsgBox.Location.X + usTextBoxWidth / 2, gMsgBox.Location.Y + usTextBoxHeight - 4));
            }
        }

        // Add region
        MouseSubSystem.AddRegionToList(gMsgBox.BackRegion);

        // findout if cursor locked, if so, store old params and store, restore when done
        if (cursor.IsCursorRestricted())
        {
            fCursorLockedToArea = true;
            CursorSubSystem.GetRestrictedClipCursor(MessageBoxRestrictedCursorRegion);
            CursorSubSystem.FreeMouseCursor();
        }

        // Create four numbered buttons
        if (usFlags.HasFlag(MSG_BOX_FLAG.FOUR_NUMBERED_BUTTONS))
        {
            // This is exclusive of any other buttons... no ok, no cancel, no nothing

            sBlankSpace = usTextBoxWidth - MSGBOX_SMALL_BUTTON_WIDTH * 4 - MSGBOX_SMALL_BUTTON_X_SEP * 3;
            sButtonX = sBlankSpace / 2;
            sButtonY = usTextBoxHeight - MSGBOX_BUTTON_HEIGHT - 10;

            gMsgBox.uiButton[0] = ButtonSubSystem.CreateIconAndTextButton(
                gMsgBox.iButtonImages,
                "1",
                FontStyle.FONT12ARIAL,
                ubFontColor, ubFontShadowColor,
                ubFontColor, ubFontShadowColor,
                ButtonTextJustifies.TEXT_CJUSTIFIED,
                new(gMsgBox.Location.X + sButtonX, gMsgBox.Location.Y + sButtonY),
                ButtonFlags.BUTTON_TOGGLE,
                MSYS_PRIORITY.HIGHEST,
                null, NumberedMsgBoxCallback);
            ButtonSubSystem.SetButtonUserData(gMsgBox.uiButton[0], 0, 1);
            ButtonSubSystem.SetButtonCursor(gMsgBox.uiButton[0], usCursor);

            sButtonX += MSGBOX_SMALL_BUTTON_WIDTH + MSGBOX_SMALL_BUTTON_X_SEP;
            gMsgBox.uiButton[1] = ButtonSubSystem.CreateIconAndTextButton(
                gMsgBox.iButtonImages,
                "2",
                FontStyle.FONT12ARIAL,
                ubFontColor, ubFontShadowColor,
                ubFontColor, ubFontShadowColor,
                ButtonTextJustifies.TEXT_CJUSTIFIED,
                new(gMsgBox.Location.X + sButtonX, gMsgBox.Location.Y + sButtonY),
                ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                null, NumberedMsgBoxCallback);

            ButtonSubSystem.SetButtonUserData(gMsgBox.uiButton[1], 0, 2);
            ButtonSubSystem.SetButtonCursor(gMsgBox.uiButton[1], usCursor);

            sButtonX += MSGBOX_SMALL_BUTTON_WIDTH + MSGBOX_SMALL_BUTTON_X_SEP;
            gMsgBox.uiButton[2] = ButtonSubSystem.CreateIconAndTextButton(
                gMsgBox.iButtonImages,
                "3",
                FontStyle.FONT12ARIAL,
                ubFontColor, ubFontShadowColor,
                ubFontColor, ubFontShadowColor,
                ButtonTextJustifies.TEXT_CJUSTIFIED,
                new(gMsgBox.Location.X + sButtonX, gMsgBox.Location.Y + sButtonY), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                null, NumberedMsgBoxCallback);

            ButtonSubSystem.SetButtonUserData(gMsgBox.uiButton[2], 0, 3);
            ButtonSubSystem.SetButtonCursor(gMsgBox.uiButton[2], usCursor);

            sButtonX += MSGBOX_SMALL_BUTTON_WIDTH + MSGBOX_SMALL_BUTTON_X_SEP;
            gMsgBox.uiButton[3] = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, "4", FontStyle.FONT12ARIAL,
                                                             ubFontColor, ubFontShadowColor,
                                                             ubFontColor, ubFontShadowColor,
                                                             ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                             new(gMsgBox.Location.X + sButtonX, gMsgBox.Location.Y + sButtonY), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                             null, NumberedMsgBoxCallback);
            ButtonSubSystem.SetButtonUserData(gMsgBox.uiButton[3], 0, 4);
            ButtonSubSystem.SetButtonCursor(gMsgBox.uiButton[3], usCursor);
            ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiButton[3]);
            ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiButton[2]);
            ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiButton[1]);
            ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiButton[0]);

        }
        else
        {

            // Create text button
            if (usFlags.HasFlag(MSG_BOX_FLAG.OK))
            {


                //			sButtonX = ( usTextBoxWidth - MSGBOX_BUTTON_WIDTH ) / 2;
                sButtonX = (usTextBoxWidth - GetMSgBoxButtonWidth(gMsgBox.iButtonImages)) / 2;

                sButtonY = usTextBoxHeight - MSGBOX_BUTTON_HEIGHT - 10;

                gMsgBox.uiOKButton = ButtonSubSystem.CreateIconAndTextButton(
                    gMsgBox.iButtonImages,
                    EnglishText.pMessageStrings[MSG.OK],
                    FontStyle.FONT12ARIAL,
                    ubFontColor,
                    ubFontShadowColor,
                    ubFontColor,
                    ubFontShadowColor,
                    ButtonTextJustifies.TEXT_CJUSTIFIED,
                    new(gMsgBox.Location.X + sButtonX, gMsgBox.Location.Y + sButtonY),
                    ButtonFlags.BUTTON_TOGGLE,
                    MSYS_PRIORITY.HIGHEST,
                    null,
                    OKMsgBoxCallback);

                ButtonSubSystem.SetButtonCursor(gMsgBox.uiOKButton, usCursor);
                ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiOKButton);
            }



            // Create text button
            if (usFlags.HasFlag(MSG_BOX_FLAG.CANCEL))
            {
                sButtonX = (usTextBoxWidth - GetMSgBoxButtonWidth(gMsgBox.iButtonImages)) / 2;
                sButtonY = usTextBoxHeight - MSGBOX_BUTTON_HEIGHT - 10;

                gMsgBox.uiOKButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, EnglishText.pMessageStrings[MSG.CANCEL], FontStyle.FONT12ARIAL,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                 new(gMsgBox.Location.X + sButtonX, gMsgBox.Location.Y + sButtonY), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                 null, OKMsgBoxCallback);
                ButtonSubSystem.SetButtonCursor(gMsgBox.uiOKButton, usCursor);
                ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiOKButton);

            }

            if (usFlags.HasFlag(MSG_BOX_FLAG.YESNO))
            {
                sButtonX = (usTextBoxWidth - (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP)) / 2;
                sButtonY = usTextBoxHeight - MSGBOX_BUTTON_HEIGHT - 10;

                gMsgBox.uiYESButton = ButtonSubSystem.CreateIconAndTextButton(
                    gMsgBox.iButtonImages,
                    pMessageStrings[MSG.YES],
                    FontStyle.FONT12ARIAL,
                    ubFontColor, ubFontShadowColor,
                    ubFontColor, ubFontShadowColor,
                    ButtonTextJustifies.TEXT_CJUSTIFIED,
                    new(gMsgBox.Location.X + sButtonX, gMsgBox.Location.Y + sButtonY),
                    ButtonFlags.BUTTON_TOGGLE,
                    MSYS_PRIORITY.HIGHEST,
                    null,
                    YESMsgBoxCallback);

                ButtonSubSystem.SetButtonCursor(gMsgBox.uiYESButton, usCursor);
                ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiYESButton);

                gMsgBox.uiNOButton = ButtonSubSystem.CreateIconAndTextButton(
                    gMsgBox.iButtonImages,
                    pMessageStrings[MSG.NO],
                    FontStyle.FONT12ARIAL,
                    ubFontColor,
                    ubFontShadowColor,
                    ubFontColor,
                    ubFontShadowColor,
                    ButtonTextJustifies.TEXT_CJUSTIFIED,
                    new(gMsgBox.Location.X + sButtonX + MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP, gMsgBox.Location.Y + sButtonY),
                    ButtonFlags.BUTTON_TOGGLE,
                    MSYS_PRIORITY.HIGHEST,
                    null, NOMsgBoxCallback);

                ButtonSubSystem.SetButtonCursor(gMsgBox.uiNOButton, usCursor);
                ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiNOButton);

            }

            if (usFlags.HasFlag(MSG_BOX_FLAG.CONTINUESTOP))
            {
                sButtonX = (usTextBoxWidth - (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP)) / 2;
                sButtonY = usTextBoxHeight - MSGBOX_BUTTON_HEIGHT - 10;

                gMsgBox.uiYESButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, pUpdatePanelButtons[0], FontStyle.FONT12ARIAL,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                 new(gMsgBox.Location.X + sButtonX, gMsgBox.Location.Y + sButtonY), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                 null, YESMsgBoxCallback);
                ButtonSubSystem.SetButtonCursor(gMsgBox.uiYESButton, usCursor);

                ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiYESButton);

                gMsgBox.uiNOButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, pUpdatePanelButtons[1], FontStyle.FONT12ARIAL,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                 new(gMsgBox.Location.X + sButtonX + MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP, gMsgBox.Location.Y + sButtonY), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                 null, NOMsgBoxCallback);
                ButtonSubSystem.SetButtonCursor(gMsgBox.uiNOButton, usCursor);
                ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiNOButton);

            }

            if (usFlags.HasFlag(MSG_BOX_FLAG.OKCONTRACT))
            {
                sButtonX = (usTextBoxWidth - (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP)) / 2;
                sButtonY = usTextBoxHeight - MSGBOX_BUTTON_HEIGHT - 10;

                gMsgBox.uiYESButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, pMessageStrings[MSG.OK], FontStyle.FONT12ARIAL,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                 new(gMsgBox.Location.X + sButtonX, gMsgBox.Location.Y + sButtonY), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                 null, OKMsgBoxCallback);
                ButtonSubSystem.SetButtonCursor(gMsgBox.uiYESButton, usCursor);

                ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiYESButton);

                gMsgBox.uiNOButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, pMessageStrings[MSG.REHIRE], FontStyle.FONT12ARIAL,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                 new(gMsgBox.Location.X + sButtonX + MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP, gMsgBox.Location.Y + sButtonY), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                 null, ContractMsgBoxCallback);
                ButtonSubSystem.SetButtonCursor(gMsgBox.uiNOButton, usCursor);
                ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiNOButton);

            }

            if (usFlags.HasFlag(MSG_BOX_FLAG.YESNOCONTRACT))
            {
                sButtonX = (usTextBoxWidth - (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP)) / 3;
                sButtonY = usTextBoxHeight - MSGBOX_BUTTON_HEIGHT - 10;

                gMsgBox.uiYESButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, pMessageStrings[MSG.YES], FontStyle.FONT12ARIAL,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                 new(gMsgBox.Location.X + sButtonX, gMsgBox.Location.Y + sButtonY), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                 null, YESMsgBoxCallback);
                ButtonSubSystem.SetButtonCursor(gMsgBox.uiYESButton, usCursor);
                ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiYESButton);


                gMsgBox.uiNOButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, pMessageStrings[MSG.NO], FontStyle.FONT12ARIAL,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                 new(gMsgBox.Location.X + sButtonX + MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP, gMsgBox.Location.Y + sButtonY), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                 null, NOMsgBoxCallback);
                ButtonSubSystem.SetButtonCursor(gMsgBox.uiNOButton, usCursor);
                ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiNOButton);

                gMsgBox.uiOKButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, pMessageStrings[MSG.REHIRE], FontStyle.FONT12ARIAL,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                 new(gMsgBox.Location.X + sButtonX + 2 * (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP), gMsgBox.Location.Y + sButtonY), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                 null, ContractMsgBoxCallback);
                ButtonSubSystem.SetButtonCursor(gMsgBox.uiOKButton, usCursor);
                ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiOKButton);

            }


            if (usFlags.HasFlag(MSG_BOX_FLAG.GENERICCONTRACT))
            {
                sButtonX = (usTextBoxWidth - (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP)) / 3;
                sButtonY = usTextBoxHeight - MSGBOX_BUTTON_HEIGHT - 10;

                gMsgBox.uiYESButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, gzUserDefinedButton1, FontStyle.FONT12ARIAL,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                 new(gMsgBox.Location.X + sButtonX, gMsgBox.Location.Y + sButtonY), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                 null, YESMsgBoxCallback);
                ButtonSubSystem.SetButtonCursor(gMsgBox.uiYESButton, usCursor);
                ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiYESButton);


                gMsgBox.uiNOButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, gzUserDefinedButton2, FontStyle.FONT12ARIAL,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                 new(gMsgBox.Location.X + sButtonX + MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP, gMsgBox.Location.Y + sButtonY), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                 null, NOMsgBoxCallback);
                ButtonSubSystem.SetButtonCursor(gMsgBox.uiNOButton, usCursor);
                ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiNOButton);

                gMsgBox.uiOKButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, pMessageStrings[MSG.REHIRE], FontStyle.FONT12ARIAL,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                 new(gMsgBox.Location.X + sButtonX + 2 * (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP), gMsgBox.Location.Y + sButtonY), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                 null, ContractMsgBoxCallback);
                ButtonSubSystem.SetButtonCursor(gMsgBox.uiOKButton, usCursor);
                ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiOKButton);

            }

            if (usFlags.HasFlag(MSG_BOX_FLAG.GENERIC))
            {
                sButtonX = (usTextBoxWidth - (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP)) / 2;
                sButtonY = usTextBoxHeight - MSGBOX_BUTTON_HEIGHT - 10;

                gMsgBox.uiYESButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, gzUserDefinedButton1, FontStyle.FONT12ARIAL,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                 new(gMsgBox.Location.X + sButtonX, gMsgBox.Location.Y + sButtonY), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                 null, YESMsgBoxCallback);
                ButtonSubSystem.SetButtonCursor(gMsgBox.uiYESButton, usCursor);
                ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiYESButton);


                gMsgBox.uiNOButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, gzUserDefinedButton2, FontStyle.FONT12ARIAL,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                 new(gMsgBox.Location.X + sButtonX + MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP, gMsgBox.Location.Y + sButtonY),
                                                                 ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                 null, NOMsgBoxCallback);
                ButtonSubSystem.SetButtonCursor(gMsgBox.uiNOButton, usCursor);
                ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiNOButton);
            }

            if (usFlags.HasFlag(MSG_BOX_FLAG.YESNOLIE))
            {
                sButtonX = (usTextBoxWidth - (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP)) / 3;
                sButtonY = usTextBoxHeight - MSGBOX_BUTTON_HEIGHT - 10;

                gMsgBox.uiYESButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, pMessageStrings[MSG.YES], FontStyle.FONT12ARIAL,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                 new(gMsgBox.Location.X + sButtonX, gMsgBox.Location.Y + sButtonY), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                 null, YESMsgBoxCallback);
                ButtonSubSystem.SetButtonCursor(gMsgBox.uiYESButton, usCursor);
                ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiYESButton);


                gMsgBox.uiNOButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, pMessageStrings[MSG.NO], FontStyle.FONT12ARIAL,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                 new(gMsgBox.Location.X + sButtonX + MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP, gMsgBox.Location.Y + sButtonY), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                 null, NOMsgBoxCallback);
                ButtonSubSystem.SetButtonCursor(gMsgBox.uiNOButton, usCursor);
                ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiNOButton);

                gMsgBox.uiOKButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, pMessageStrings[MSG.LIE], FontStyle.FONT12ARIAL,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                 new(gMsgBox.Location.X + sButtonX + 2 * (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP), gMsgBox.Location.Y + sButtonY), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                 null, LieMsgBoxCallback);
                ButtonSubSystem.SetButtonCursor(gMsgBox.uiOKButton, usCursor);
                ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiOKButton);

            }

            if (usFlags.HasFlag(MSG_BOX_FLAG.OKSKIP))
            {
                sButtonX = (usTextBoxWidth - (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP)) / 2;
                sButtonY = usTextBoxHeight - MSGBOX_BUTTON_HEIGHT - 10;

                gMsgBox.uiYESButton = ButtonSubSystem.CreateIconAndTextButton(
                    gMsgBox.iButtonImages,
                    pMessageStrings[MSG.OK],
                    FontStyle.FONT12ARIAL,
                    ubFontColor, ubFontShadowColor,
                    ubFontColor, ubFontShadowColor,
                    ButtonTextJustifies.TEXT_CJUSTIFIED,
                    new(gMsgBox.Location.X + sButtonX, gMsgBox.Location.Y + sButtonY), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                    null, YESMsgBoxCallback);

                ButtonSubSystem.SetButtonCursor(gMsgBox.uiYESButton, usCursor);

                ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiYESButton);

                gMsgBox.uiNOButton = ButtonSubSystem.CreateIconAndTextButton(
                    gMsgBox.iButtonImages,
                    EnglishText.pMessageStrings[MSG.SKIP],
                    FontStyle.FONT12ARIAL,
                    ubFontColor, ubFontShadowColor,
                    ubFontColor, ubFontShadowColor,
                    ButtonTextJustifies.TEXT_CJUSTIFIED,
                    new(
                        gMsgBox.Location.X + sButtonX + MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP,
                        gMsgBox.Location.Y + sButtonY),
                    ButtonFlags.BUTTON_TOGGLE,
                    MSYS_PRIORITY.HIGHEST,
                    null,
                    NOMsgBoxCallback);

                ButtonSubSystem.SetButtonCursor(gMsgBox.uiNOButton, usCursor);
                ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiNOButton);
            }

        }

        ClockManager.InterruptTime();
        ClockManager.PauseGame();
        ClockManager.LockPauseState(1);
        // Pause timers as well....
        TimerControl.PauseTime(true);

        // Save mouse restriction region...
        CursorSubSystem.GetRestrictedClipCursor(gOldCursorLimitRectangle);
        CursorSubSystem.FreeMouseCursor();

        gfNewMessageBox = true;

        gfInMsgBox = true;

        return iId;
    }

    private static int GetMSgBoxButtonWidth(ButtonPic iButtonImage)
    {
        return ButtonSubSystem.GetWidthOfButtonPic(iButtonImage, iButtonImage.OnNormal);
    }

    public static void MsgBoxClickCallback(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        // if (iReason & MouseCallbackReasons.RBUTTON_UP)
        // {
        //     gMsgBox.bHandled = MSG.BOX_RETURN_NO;
        // }
        //
    }


    private static bool OKMsgBoxCallbackfLButtonDown = false;
    public static void OKMsgBoxCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
            OKMsgBoxCallbackfLButtonDown = true;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) && OKMsgBoxCallbackfLButtonDown)
        {
            btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;

            // OK, exit
            gMsgBox.bHandled = MessageBoxReturnCode.MSG_BOX_RETURN_OK;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
        {
            OKMsgBoxCallbackfLButtonDown = false;
        }
    }

    private static bool YESMsgBoxCallbackfLButtonDown = false;
    public static void YESMsgBoxCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
            YESMsgBoxCallbackfLButtonDown = true;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) && YESMsgBoxCallbackfLButtonDown)
        {
            btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;

            // OK, exit
            gMsgBox.bHandled = MessageBoxReturnCode.MSG_BOX_RETURN_YES;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
        {
            YESMsgBoxCallbackfLButtonDown = false;
        }
    }

    private static bool NOMsgBoxCallbackfLButtonDown = false;
    public static void NOMsgBoxCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
            NOMsgBoxCallbackfLButtonDown = true;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) && NOMsgBoxCallbackfLButtonDown)
        {
            btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;

            // OK, exit
            gMsgBox.bHandled = MessageBoxReturnCode.MSG_BOX_RETURN_NO;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
        {
            NOMsgBoxCallbackfLButtonDown = false;
        }
    }


    private static bool ContractMsgBoxCallbackfLButtonDown = false;
    public static void ContractMsgBoxCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
            ContractMsgBoxCallbackfLButtonDown = true;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) && ContractMsgBoxCallbackfLButtonDown)
        {
            btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;

            // OK, exit
            gMsgBox.bHandled = MessageBoxReturnCode.MSG_BOX_RETURN_CONTRACT;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
        {
            ContractMsgBoxCallbackfLButtonDown = false;
        }
    }

    private static bool LieMsgBoxCallbackfLButtonDown = false;
    public static void LieMsgBoxCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
            LieMsgBoxCallbackfLButtonDown = true;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) && LieMsgBoxCallbackfLButtonDown)
        {
            btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;

            // OK, exit
            gMsgBox.bHandled = MessageBoxReturnCode.MSG_BOX_RETURN_LIE;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
        {
            LieMsgBoxCallbackfLButtonDown = false;
        }
    }


    public static void NumberedMsgBoxCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;

            // OK, exit
            gMsgBox.bHandled = (MessageBoxReturnCode)ButtonSubSystem.GetButtonnUserData(btn, 0);
        }

    }

    ScreenName ExitMsgBox(MessageBoxReturnCode ubExitCode)
    {
        int uiDestPitchBYTES, uiSrcPitchBYTES;
        Image<Rgba32> pDestBuf, pSrcBuf;

        // Delete popup!
        mercTextBox?.RemoveMercPopupBoxFromIndex(gMsgBox.iBoxId);
        gMsgBox.iBoxId = -1;

        //Delete buttons!
        if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.FOUR_NUMBERED_BUTTONS))
        {
            ButtonSubSystem.RemoveButton(gMsgBox.uiButton[0]);
            ButtonSubSystem.RemoveButton(gMsgBox.uiButton[1]);
            ButtonSubSystem.RemoveButton(gMsgBox.uiButton[2]);
            ButtonSubSystem.RemoveButton(gMsgBox.uiButton[3]);
        }
        else
        {
            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.OK))
            {
                ButtonSubSystem.RemoveButton(gMsgBox.uiOKButton);
            }

            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.YESNO))
            {
                ButtonSubSystem.RemoveButton(gMsgBox.uiYESButton);
                ButtonSubSystem.RemoveButton(gMsgBox.uiNOButton);
            }

            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.OKCONTRACT))
            {
                ButtonSubSystem.RemoveButton(gMsgBox.uiYESButton);
                ButtonSubSystem.RemoveButton(gMsgBox.uiNOButton);
            }

            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.YESNOCONTRACT))
            {
                ButtonSubSystem.RemoveButton(gMsgBox.uiYESButton);
                ButtonSubSystem.RemoveButton(gMsgBox.uiNOButton);
                ButtonSubSystem.RemoveButton(gMsgBox.uiOKButton);
            }

            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.GENERICCONTRACT))
            {
                ButtonSubSystem.RemoveButton(gMsgBox.uiYESButton);
                ButtonSubSystem.RemoveButton(gMsgBox.uiNOButton);
                ButtonSubSystem.RemoveButton(gMsgBox.uiOKButton);
            }

            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.GENERIC))
            {
                ButtonSubSystem.RemoveButton(gMsgBox.uiYESButton);
                ButtonSubSystem.RemoveButton(gMsgBox.uiNOButton);
            }

            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.YESNOLIE))
            {
                ButtonSubSystem.RemoveButton(gMsgBox.uiYESButton);
                ButtonSubSystem.RemoveButton(gMsgBox.uiNOButton);
                ButtonSubSystem.RemoveButton(gMsgBox.uiOKButton);
            }

            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.CONTINUESTOP))
            {
                ButtonSubSystem.RemoveButton(gMsgBox.uiYESButton);
                ButtonSubSystem.RemoveButton(gMsgBox.uiNOButton);
            }

            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.OKSKIP))
            {
                ButtonSubSystem.RemoveButton(gMsgBox.uiYESButton);
                ButtonSubSystem.RemoveButton(gMsgBox.uiNOButton);
            }

        }

        // Delete button images
        ButtonSubSystem.UnloadButtonImage(gMsgBox.iButtonImages);

        // Unpause game....
        ClockManager.UnLockPauseState();
        ClockManager.UnPauseGame();
        // UnPause timers as well....
        TimerControl.PauseTime(false);

        // Restore mouse restriction region...
        MouseSubSystem.RestrictMouseCursor(gOldCursorLimitRectangle);


        gfInMsgBox = false;

        // Call done callback!
        gMsgBox.ExitCallback?.Invoke(ubExitCode);

        //if ur in a non gamescreen and DONT want the msg box to use the save buffer, unset gfDontOverRideSaveBuffer in ur callback
        if (((gMsgBox.uiExitScreen != ScreenName.GAME_SCREEN) || (fRestoreBackgroundForMessageBox == true)) && gfDontOverRideSaveBuffer)
        {
            // restore what we have under here...
            pSrcBuf = video.Surfaces[gMsgBox.uiSaveBuffer];
            pDestBuf = video.Surfaces[SurfaceType.FRAME_BUFFER];
            
            video.Blt16BPPTo16BPP(
                pDestBuf,
                pSrcBuf, 
                gMsgBox.Location,
                new(0, 0),
                gMsgBox.Size);
            
            //UnLockVideoSurface(gMsgBox.uiSaveBuffer);
            //UnLockVideoSurface(FRAME_BUFFER);
            
            video.InvalidateRegion(gMsgBox.Location.X, gMsgBox.Location.Y, (gMsgBox.Location.X + gMsgBox.Size.Width), (gMsgBox.Location.Y + gMsgBox.Size.Height));
        }

        fRestoreBackgroundForMessageBox = false;
        gfDontOverRideSaveBuffer = true;

        if (fCursorLockedToArea == true)
        {
            inputs.GetCursorPosition(out Point pPosition);

            if ((pPosition.X > MessageBoxRestrictedCursorRegion.Width) || (pPosition.X > MessageBoxRestrictedCursorRegion.X) && (pPosition.Y < MessageBoxRestrictedCursorRegion.Y) && (pPosition.Y > MessageBoxRestrictedCursorRegion.Height))
            {
                MouseSubSystem.SimulateMouseMovement(pOldMousePosition);
            }

            fCursorLockedToArea = false;
            MouseSubSystem.RestrictMouseCursor(MessageBoxRestrictedCursorRegion);
        }

        // Remove region
        MouseSubSystem.MSYS_RemoveRegion(gMsgBox.BackRegion);

        // Remove save buffer!
        // DeleteVideoSurfaceFromIndex(gMsgBox.uiSaveBuffer);


        switch (gMsgBox.uiExitScreen)
        {
            case ScreenName.GAME_SCREEN:

                if (Overhead.InOverheadMap())
                {
                    gfOverheadMapDirty = true;
                }
                else
                {
                    RenderWorld.SetRenderFlags(RenderingFlags.FULL);
                }
                break;
            case ScreenName.MAP_SCREEN:
                fMapPanelDirty = true;
                break;
        }

        if (fadeScreen.gfFadeInitialized)
        {
            screens.SetPendingNewScreen(ScreenName.FADE_SCREEN);
            return ScreenName.FADE_SCREEN;
        }

        return gMsgBox.uiExitScreen;
    }
}

public class MessageBox
{
    public MSG_BOX_FLAG usFlags;
    public ScreenName uiExitScreen;
    public MSGBOX_CALLBACK? ExitCallback;
    public Point Location;
    public SurfaceType uiSaveBuffer;
    public MOUSE_REGION BackRegion { get; } = new(nameof(MessageBox));
    public Size Size;
    public ButtonPic iButtonImages;
    public GUI_BUTTON uiOKButton;
    public GUI_BUTTON uiYESButton;
    public GUI_BUTTON uiNOButton;
    public GUI_BUTTON uiUnusedButton;
    public GUI_BUTTON[] uiButton = new GUI_BUTTON[4];
    public bool fRenderBox;
    public MessageBoxReturnCode bHandled;
    public int iBoxId;
}

public enum MessageBoxReturnCode
{
    MSG_BOX_RETURN_OK = 1,			// ENTER or on OK button
    MSG_BOX_RETURN_YES = 2,		// ENTER or YES button
    MSG_BOX_RETURN_NO = 3,			// ESC, Right Click or NO button
    MSG_BOX_RETURN_CONTRACT = 4,	// contract button
    MSG_BOX_RETURN_LIE = 5,		// LIE BUTTON
}

[Flags]
public enum MSG_BOX_FLAG
{
    USE_CENTERING_RECT = 0x0001,// Pass in a rect to center in
    OK = 0x0002,	// Displays OK button
    YESNO = 0x0004,// Displays YES NO buttons
    CANCEL = 0x0008,// Displays YES NO buttons
    FOUR_NUMBERED_BUTTONS = 0x0010,// Displays four numbered buttons, 1-4
    YESNOCONTRACT = 0x0020,     // yes no and contract buttons
    OKCONTRACT = 0x0040,	// ok and contract buttons
    YESNOLIE = 0x0080,		// ok and contract buttons
    CONTINUESTOP = 0x0100,	// continue stop box
    OKSKIP = 0x0200,		// Displays ok or skip (meanwhile) buttons
    GENERICCONTRACT = 0x0400,// displays contract buttoin + 2 user-defined text buttons
    GENERIC = 0x0800,			// 2 user-defined text buttons
}

public enum MessageBoxStyle
{
    MSG_BOX_BASIC_STYLE = 0,    // We'll have other styles, like in laptop, etc
                                // Graphics are all that are required here...
    MSG_BOX_RED_ON_WHITE,
    MSG_BOX_BLUE_ON_GREY,
    MSG_BOX_BASIC_SMALL_BUTTONS,
    MSG_BOX_IMP_STYLE,
    MSG_BOX_LAPTOP_DEFAULT,
}
