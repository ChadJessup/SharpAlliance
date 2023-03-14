﻿using System;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.Screens;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;
using SixLabors.ImageSharp;
using static SharpAlliance.Core.EnglishText;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems
{
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
        private static GameSettings gGameSettings;
        private readonly MercTextBox mercTextBox;
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
            RenderWorld renderWorld,
            IInputManager inputManager,
            IScreenManager screenManager,
            IClockManager clockManager,
            Overhead overhead,
            GameSettings gameSettings)
        {
            gGameSettings = gameSettings;
            mercTextBox = mercTextBox;
            renderWorld = renderWorld;
            cursor = cursorSubSystem;
            screens = screenManager;
            mouse = mouseSubSystem;
            inputs = inputManager;
            clock = clockManager;
            overhead = overhead;
            context = context;
        }

        public bool IsInitialized { get; }

        public void Dispose()
        {
        }

        public async ValueTask<bool> Initialize()
        {
            mapScreen = await screens.GetScreen<MapScreen>(ScreenName.MAP_SCREEN, activate: false);
            fadeScreen = await screens.GetScreen<FadeScreen>(ScreenName.FADE_SCREEN, activate: false);

            return true;
        }

        public static int DoMessageBox(MessageBoxStyle ubStyle, string zString, ScreenName uiExitScreen, MessageBoxFlags usFlags, MSGBOX_CALLBACK ReturnCallback, ref Rectangle? pCenteringRect)
        {
            VSURFACE_DESC vs_desc;
            int usTextBoxWidth;
            int usTextBoxHeight;
            Rectangle aRect = new();
            int uiDestPitchBYTES, uiSrcPitchBYTES;
            int pDestBuf, pSrcBuf;
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

            cursor.SetCurrentCursorFromDatabase(CURSOR.NORMAL);

            if (gMsgBox.BackRegion.uiFlags.HasFlag(MouseRegionFlags.REGION_EXISTS))
            {
                return (0);
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

            if (usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_USE_CENTERING_RECT) && pCenteringRect is not null)
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
            gMsgBox.iBoxId = MercTextBox.PrepareMercPopupBox(iId, ubMercBoxBackground, ubMercBoxBorder, zString, MSGBOX_DEFAULT_WIDTH, 40, 10, 30, out usTextBoxWidth, out usTextBoxHeight);

            if (gMsgBox.iBoxId == -1)
            {
                return 0;
            }

            // Save height,width
            gMsgBox.usWidth = usTextBoxWidth;
            gMsgBox.usHeight = usTextBoxHeight;

            // Determine position ( centered in rect )
            gMsgBox.sX = (int)((((aRect.Width - aRect.X) - usTextBoxWidth) / 2) + aRect.X);
            gMsgBox.sY = (int)((((aRect.Height - aRect.Y) - usTextBoxHeight) / 2) + aRect.Y);

            if (screens.CurrentScreenName == ScreenName.GAME_SCREEN)
            {
                gfStartedFromGameScreen = true;
            }

            if ((fInMapMode == true))
            {
                //		fMapExitDueToMessageBox = true;
                gfStartedFromMapScreen = true;
                mapScreen.fMapPanelDirty = true;
            }


            // Set pending screen
            screens.SetPendingNewScreen(ScreenName.MSG_BOX_SCREEN);

            // Init save buffer
            vs_desc.fCreateFlags = VSurfaceCreateFlags.VSURFACE_CREATE_DEFAULT | VSurfaceCreateFlags.VSURFACE_SYSTEM_MEM_USAGE;
            vs_desc.usWidth = usTextBoxWidth;
            vs_desc.usHeight = usTextBoxHeight;
            vs_desc.ubBitDepth = 16;

            //if (AddVideoSurface(vs_desc, gMsgBox.uiSaveBuffer) == false)
            //{
            //    return (-1);
            //}

            //Save what we have under here...
            //pDestBuf = LockVideoSurface(gMsgBox.uiSaveBuffer, out uiDestPitchBYTES);
            //pSrcBuf = LockVideoSurface(FRAME_BUFFER, out uiSrcPitchBYTES);

            // Blt16BPPTo16BPP(pDestBuf, uiDestPitchBYTES,
            //             pSrcBuf, uiSrcPitchBYTES,
            //             0, 0,
            //             gMsgBox.sX, gMsgBox.sY,
            //             usTextBoxWidth, usTextBoxHeight);
            // 
            // UnLockVideoSurface(gMsgBox.uiSaveBuffer);
            // UnLockVideoSurface(FRAME_BUFFER);

            // Create top-level mouse region
            MouseSubSystem.MSYS_DefineRegion(gMsgBox.BackRegion, new(0, 0, 640, 480), MSYS_PRIORITY.HIGHEST,
                                 usCursor, null, MsgBoxClickCallback);

            if (gGameSettings[TOPTION.DONT_MOVE_MOUSE] == false)
            {
                if (usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_OK))
                {
                    MouseSubSystem.SimulateMouseMovement((gMsgBox.sX + (usTextBoxWidth / 2) + 27), (gMsgBox.sY + (usTextBoxHeight - 10)));
                }
                else
                {
                    MouseSubSystem.SimulateMouseMovement(gMsgBox.sX + usTextBoxWidth / 2, gMsgBox.sY + usTextBoxHeight - 4);
                }
            }

            // Add region
            MouseSubSystem.AddRegionToList(gMsgBox.BackRegion);

            // findout if cursor locked, if so, store old params and store, restore when done
            if (cursor.IsCursorRestricted())
            {
                fCursorLockedToArea = true;
                cursor.GetRestrictedClipCursor(MessageBoxRestrictedCursorRegion);
                cursor.FreeMouseCursor();
            }

            // Create four numbered buttons
            if (usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_FOUR_NUMBERED_BUTTONS))
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
                    new((gMsgBox.sX + sButtonX), (gMsgBox.sY + sButtonY)),
                    ButtonFlags.BUTTON_TOGGLE,
                    MSYS_PRIORITY.HIGHEST,
                    null, NumberedMsgBoxCallback);
                ButtonSubSystem.SetButtonUserData(gMsgBox.uiButton[0], 0, 1);
                ButtonSubSystem.SetButtonCursor(gMsgBox.uiButton[0], usCursor);

                sButtonX += MSGBOX_SMALL_BUTTON_WIDTH + MSGBOX_SMALL_BUTTON_X_SEP;
                gMsgBox.uiButton[1] = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, "2", FontStyle.FONT12ARIAL,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                 new((gMsgBox.sX + sButtonX), (gMsgBox.sY + sButtonY)),
                                                                 ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                 null, NumberedMsgBoxCallback);
                ButtonSubSystem.SetButtonUserData(gMsgBox.uiButton[1], 0, 2);
                ButtonSubSystem.SetButtonCursor(gMsgBox.uiButton[1], usCursor);

                sButtonX += MSGBOX_SMALL_BUTTON_WIDTH + MSGBOX_SMALL_BUTTON_X_SEP;
                gMsgBox.uiButton[2] = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, "3", FontStyle.FONT12ARIAL,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                 new((gMsgBox.sX + sButtonX), (gMsgBox.sY + sButtonY)), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                 null, NumberedMsgBoxCallback);
                ButtonSubSystem.SetButtonUserData(gMsgBox.uiButton[2], 0, 3);
                ButtonSubSystem.SetButtonCursor(gMsgBox.uiButton[2], usCursor);

                sButtonX += MSGBOX_SMALL_BUTTON_WIDTH + MSGBOX_SMALL_BUTTON_X_SEP;
                gMsgBox.uiButton[3] = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, "4", FontStyle.FONT12ARIAL,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ubFontColor, ubFontShadowColor,
                                                                 ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                 new((gMsgBox.sX + sButtonX), (gMsgBox.sY + sButtonY)), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
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
                if (usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_OK))
                {


                    //			sButtonX = ( usTextBoxWidth - MSGBOX_BUTTON_WIDTH ) / 2;
                    sButtonX = (usTextBoxWidth - GetMSgBoxButtonWidth(gMsgBox.iButtonImages)) / 2;

                    sButtonY = usTextBoxHeight - MSGBOX_BUTTON_HEIGHT - 10;

                    gMsgBox.uiOKButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, EnglishText.pMessageStrings[MSG.OK], FontStyle.FONT12ARIAL,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                     new((gMsgBox.sX + sButtonX), (gMsgBox.sY + sButtonY)), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                     null, OKMsgBoxCallback);
                    ButtonSubSystem.SetButtonCursor(gMsgBox.uiOKButton, usCursor);
                    ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiOKButton);
                }



                // Create text button
                if (usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_CANCEL))
                {
                    sButtonX = (usTextBoxWidth - GetMSgBoxButtonWidth(gMsgBox.iButtonImages)) / 2;
                    sButtonY = usTextBoxHeight - MSGBOX_BUTTON_HEIGHT - 10;

                    gMsgBox.uiOKButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, EnglishText.pMessageStrings[MSG.CANCEL], FontStyle.FONT12ARIAL,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                     new((gMsgBox.sX + sButtonX), (gMsgBox.sY + sButtonY)), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                     null, OKMsgBoxCallback);
                    ButtonSubSystem.SetButtonCursor(gMsgBox.uiOKButton, usCursor);
                    ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiOKButton);

                }

                if (usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_YESNO))
                {
                    sButtonX = (usTextBoxWidth - (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP)) / 2;
                    sButtonY = usTextBoxHeight - MSGBOX_BUTTON_HEIGHT - 10;

                    gMsgBox.uiYESButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, pMessageStrings[MSG.YES], FontStyle.FONT12ARIAL,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                     new((gMsgBox.sX + sButtonX), (gMsgBox.sY + sButtonY)), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                     null, YESMsgBoxCallback);
                    ButtonSubSystem.SetButtonCursor(gMsgBox.uiYESButton, usCursor);

                    ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiYESButton);

                    gMsgBox.uiNOButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, pMessageStrings[MSG.NO], FontStyle.FONT12ARIAL,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                     new((gMsgBox.sX + sButtonX + (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP)), (gMsgBox.sY + sButtonY)), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                     null, NOMsgBoxCallback);
                    ButtonSubSystem.SetButtonCursor(gMsgBox.uiNOButton, usCursor);
                    ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiNOButton);

                }

                if (usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_CONTINUESTOP))
                {
                    sButtonX = (usTextBoxWidth - (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP)) / 2;
                    sButtonY = usTextBoxHeight - MSGBOX_BUTTON_HEIGHT - 10;

                    gMsgBox.uiYESButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, pUpdatePanelButtons[0], FontStyle.FONT12ARIAL,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                     new((gMsgBox.sX + sButtonX), (gMsgBox.sY + sButtonY)), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                     null, YESMsgBoxCallback);
                    ButtonSubSystem.SetButtonCursor(gMsgBox.uiYESButton, usCursor);

                    ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiYESButton);

                    gMsgBox.uiNOButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, pUpdatePanelButtons[1], FontStyle.FONT12ARIAL,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                     new((gMsgBox.sX + sButtonX + (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP)), (gMsgBox.sY + sButtonY)), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                     null, NOMsgBoxCallback);
                    ButtonSubSystem.SetButtonCursor(gMsgBox.uiNOButton, usCursor);
                    ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiNOButton);

                }

                if (usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_OKCONTRACT))
                {
                    sButtonX = (usTextBoxWidth - (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP)) / 2;
                    sButtonY = usTextBoxHeight - MSGBOX_BUTTON_HEIGHT - 10;

                    gMsgBox.uiYESButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, pMessageStrings[MSG.OK], FontStyle.FONT12ARIAL,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                     new((gMsgBox.sX + sButtonX), (gMsgBox.sY + sButtonY)), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                     null, OKMsgBoxCallback);
                    ButtonSubSystem.SetButtonCursor(gMsgBox.uiYESButton, usCursor);

                    ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiYESButton);

                    gMsgBox.uiNOButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, pMessageStrings[MSG.REHIRE], FontStyle.FONT12ARIAL,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                     new((gMsgBox.sX + sButtonX + (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP)), (gMsgBox.sY + sButtonY)), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                     null, ContractMsgBoxCallback);
                    ButtonSubSystem.SetButtonCursor(gMsgBox.uiNOButton, usCursor);
                    ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiNOButton);

                }

                if (usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_YESNOCONTRACT))
                {
                    sButtonX = (usTextBoxWidth - (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP)) / 3;
                    sButtonY = usTextBoxHeight - MSGBOX_BUTTON_HEIGHT - 10;

                    gMsgBox.uiYESButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, pMessageStrings[MSG.YES], FontStyle.FONT12ARIAL,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                     new((gMsgBox.sX + sButtonX), (gMsgBox.sY + sButtonY)), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                     null, YESMsgBoxCallback);
                    ButtonSubSystem.SetButtonCursor(gMsgBox.uiYESButton, usCursor);
                    ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiYESButton);


                    gMsgBox.uiNOButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, pMessageStrings[MSG.NO], FontStyle.FONT12ARIAL,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                     new((gMsgBox.sX + sButtonX + (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP)), (gMsgBox.sY + sButtonY)), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                     null, NOMsgBoxCallback);
                    ButtonSubSystem.SetButtonCursor(gMsgBox.uiNOButton, usCursor);
                    ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiNOButton);

                    gMsgBox.uiOKButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, pMessageStrings[MSG.REHIRE], FontStyle.FONT12ARIAL,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                     new((gMsgBox.sX + sButtonX + 2 * (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP)), (gMsgBox.sY + sButtonY)), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                     null, ContractMsgBoxCallback);
                    ButtonSubSystem.SetButtonCursor(gMsgBox.uiOKButton, usCursor);
                    ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiOKButton);

                }


                if (usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_GENERICCONTRACT))
                {
                    sButtonX = (usTextBoxWidth - (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP)) / 3;
                    sButtonY = usTextBoxHeight - MSGBOX_BUTTON_HEIGHT - 10;

                    gMsgBox.uiYESButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, gzUserDefinedButton1, FontStyle.FONT12ARIAL,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                     new((gMsgBox.sX + sButtonX), (gMsgBox.sY + sButtonY)), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                     null, YESMsgBoxCallback);
                    ButtonSubSystem.SetButtonCursor(gMsgBox.uiYESButton, usCursor);
                    ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiYESButton);


                    gMsgBox.uiNOButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, gzUserDefinedButton2, FontStyle.FONT12ARIAL,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                     new((gMsgBox.sX + sButtonX + (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP)), (gMsgBox.sY + sButtonY)), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                     null, NOMsgBoxCallback);
                    ButtonSubSystem.SetButtonCursor(gMsgBox.uiNOButton, usCursor);
                    ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiNOButton);

                    gMsgBox.uiOKButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, pMessageStrings[MSG.REHIRE], FontStyle.FONT12ARIAL,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                     new((gMsgBox.sX + sButtonX + 2 * (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP)), (gMsgBox.sY + sButtonY)), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                     null, ContractMsgBoxCallback);
                    ButtonSubSystem.SetButtonCursor(gMsgBox.uiOKButton, usCursor);
                    ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiOKButton);

                }

                if (usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_GENERIC))
                {
                    sButtonX = (usTextBoxWidth - (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP)) / 2;
                    sButtonY = usTextBoxHeight - MSGBOX_BUTTON_HEIGHT - 10;

                    gMsgBox.uiYESButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, gzUserDefinedButton1, FontStyle.FONT12ARIAL,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                     new((gMsgBox.sX + sButtonX), (gMsgBox.sY + sButtonY)), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                     null, YESMsgBoxCallback);
                    ButtonSubSystem.SetButtonCursor(gMsgBox.uiYESButton, usCursor);
                    ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiYESButton);


                    gMsgBox.uiNOButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, gzUserDefinedButton2, FontStyle.FONT12ARIAL,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                     new((gMsgBox.sX + sButtonX + (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP)), (gMsgBox.sY + sButtonY)),
                                                                     ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                     null, NOMsgBoxCallback);
                    ButtonSubSystem.SetButtonCursor(gMsgBox.uiNOButton, usCursor);
                    ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiNOButton);
                }

                if (usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_YESNOLIE))
                {
                    sButtonX = (usTextBoxWidth - (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP)) / 3;
                    sButtonY = usTextBoxHeight - MSGBOX_BUTTON_HEIGHT - 10;

                    gMsgBox.uiYESButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, pMessageStrings[MSG.YES], FontStyle.FONT12ARIAL,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                     new((gMsgBox.sX + sButtonX), (gMsgBox.sY + sButtonY)), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                     null, YESMsgBoxCallback);
                    ButtonSubSystem.SetButtonCursor(gMsgBox.uiYESButton, usCursor);
                    ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiYESButton);


                    gMsgBox.uiNOButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, pMessageStrings[MSG.NO], FontStyle.FONT12ARIAL,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                     new((gMsgBox.sX + sButtonX + (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP)), (gMsgBox.sY + sButtonY)), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                     null, NOMsgBoxCallback);
                    ButtonSubSystem.SetButtonCursor(gMsgBox.uiNOButton, usCursor);
                    ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiNOButton);

                    gMsgBox.uiOKButton = ButtonSubSystem.CreateIconAndTextButton(gMsgBox.iButtonImages, pMessageStrings[MSG.LIE], FontStyle.FONT12ARIAL,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ubFontColor, ubFontShadowColor,
                                                                     ButtonTextJustifies.TEXT_CJUSTIFIED,
                                                                     new((gMsgBox.sX + sButtonX + 2 * (MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP)), (gMsgBox.sY + sButtonY)), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                                                                     null, LieMsgBoxCallback);
                    ButtonSubSystem.SetButtonCursor(gMsgBox.uiOKButton, usCursor);
                    ButtonSubSystem.ForceButtonUnDirty(gMsgBox.uiOKButton);

                }

                if (usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_OKSKIP))
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
                        new((gMsgBox.sX + sButtonX), (gMsgBox.sY + sButtonY)), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
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
                            gMsgBox.sX + sButtonX + MSGBOX_BUTTON_WIDTH + MSGBOX_BUTTON_X_SEP,
                            gMsgBox.sY + sButtonY),
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
            //CursorSubSystem.GetRestrictedClipCursor(gOldCursorLimitRectangle);
            //CursorSubSystem.FreeMouseCursor();

            gfNewMessageBox = true;

            gfInMsgBox = true;

            return (iId);
        }

        private static int GetMSgBoxButtonWidth(ButtonPic iButtonImage)
        {
            return ButtonSubSystem.GetWidthOfButtonPic(iButtonImage, iButtonImage.OnNormal);
        }

        public static void MsgBoxClickCallback(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
        {
            // if (iReason & MouseCallbackReasons.RBUTTON_UP)
            // {
            //     gMsgBox.bHandled = MSG_BOX_RETURN_NO;
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
            else if ((reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP)) && OKMsgBoxCallbackfLButtonDown)
            {
                btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);

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
            else if ((reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP)) && YESMsgBoxCallbackfLButtonDown)
            {
                btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);

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
            else if ((reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP)) && NOMsgBoxCallbackfLButtonDown)
            {
                btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);

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
            else if ((reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP)) && ContractMsgBoxCallbackfLButtonDown)
            {
                btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);

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
            else if ((reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP)) && LieMsgBoxCallbackfLButtonDown)
            {
                btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);

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
                btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);

                // OK, exit
                gMsgBox.bHandled = (MessageBoxReturnCode)ButtonSubSystem.GetButtonnUserData(btn, 0);
            }

        }

        ScreenName ExitMsgBox(MessageBoxReturnCode ubExitCode)
        {
            int uiDestPitchBYTES, uiSrcPitchBYTES;
            int pDestBuf, pSrcBuf;
            Point pPosition;

            // Delete popup!
            MercTextBox.RemoveMercPopupBoxFromIndex(gMsgBox.iBoxId);
            gMsgBox.iBoxId = -1;

            //Delete buttons!
            if (gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_FOUR_NUMBERED_BUTTONS))
            {
                ButtonSubSystem.RemoveButton(gMsgBox.uiButton[0]);
                ButtonSubSystem.RemoveButton(gMsgBox.uiButton[1]);
                ButtonSubSystem.RemoveButton(gMsgBox.uiButton[2]);
                ButtonSubSystem.RemoveButton(gMsgBox.uiButton[3]);
            }
            else
            {
                if (gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_OK))
                {
                    ButtonSubSystem.RemoveButton(gMsgBox.uiOKButton);
                }

                if (gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_YESNO))
                {
                    ButtonSubSystem.RemoveButton(gMsgBox.uiYESButton);
                    ButtonSubSystem.RemoveButton(gMsgBox.uiNOButton);
                }

                if (gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_OKCONTRACT))
                {
                    ButtonSubSystem.RemoveButton(gMsgBox.uiYESButton);
                    ButtonSubSystem.RemoveButton(gMsgBox.uiNOButton);
                }

                if (gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_YESNOCONTRACT))
                {
                    ButtonSubSystem.RemoveButton(gMsgBox.uiYESButton);
                    ButtonSubSystem.RemoveButton(gMsgBox.uiNOButton);
                    ButtonSubSystem.RemoveButton(gMsgBox.uiOKButton);
                }

                if (gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_GENERICCONTRACT))
                {
                    ButtonSubSystem.RemoveButton(gMsgBox.uiYESButton);
                    ButtonSubSystem.RemoveButton(gMsgBox.uiNOButton);
                    ButtonSubSystem.RemoveButton(gMsgBox.uiOKButton);
                }

                if (gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_GENERIC))
                {
                    ButtonSubSystem.RemoveButton(gMsgBox.uiYESButton);
                    ButtonSubSystem.RemoveButton(gMsgBox.uiNOButton);
                }

                if (gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_YESNOLIE))
                {
                    ButtonSubSystem.RemoveButton(gMsgBox.uiYESButton);
                    ButtonSubSystem.RemoveButton(gMsgBox.uiNOButton);
                    ButtonSubSystem.RemoveButton(gMsgBox.uiOKButton);
                }

                if (gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_CONTINUESTOP))
                {
                    ButtonSubSystem.RemoveButton(gMsgBox.uiYESButton);
                    ButtonSubSystem.RemoveButton(gMsgBox.uiNOButton);
                }

                if (gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_OKSKIP))
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
                // pSrcBuf = LockVideoSurface(gMsgBox.uiSaveBuffer, out uiSrcPitchBYTES);
                // pDestBuf = LockVideoSurface(FRAME_BUFFER, out uiDestPitchBYTES);
                // 
                // Blt16BPPTo16BPP(pDestBuf, uiDestPitchBYTES,
                //             pSrcBuf, uiSrcPitchBYTES,
                //             gMsgBox.sX, gMsgBox.sY,
                //             0, 0,
                //             gMsgBox.usWidth, gMsgBox.usHeight);
                // 
                // UnLockVideoSurface(gMsgBox.uiSaveBuffer);
                // UnLockVideoSurface(FRAME_BUFFER);
                // 
                // InvalidateRegion(gMsgBox.sX, gMsgBox.sY, (gMsgBox.sX + gMsgBox.usWidth), (gMsgBox.sY + gMsgBox.usHeight));
            }

            fRestoreBackgroundForMessageBox = false;
            gfDontOverRideSaveBuffer = true;

            if (fCursorLockedToArea == true)
            {
                inputs.GetCursorPosition(out pPosition);

                if ((pPosition.X > MessageBoxRestrictedCursorRegion.Width) || (pPosition.X > MessageBoxRestrictedCursorRegion.X) && (pPosition.Y < MessageBoxRestrictedCursorRegion.Y) && (pPosition.Y > MessageBoxRestrictedCursorRegion.Height))
                {
                    MouseSubSystem.SimulateMouseMovement(pOldMousePosition.X, pOldMousePosition.Y);
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
                    mapScreen.fMapPanelDirty = true;
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
        public MessageBoxFlags usFlags;
        public ScreenName uiExitScreen;
        public MSGBOX_CALLBACK? ExitCallback;
        public int sX;
        public int sY;
        public Surfaces uiSaveBuffer;
        public MOUSE_REGION BackRegion { get; } = new(nameof(MessageBox));
        public int usWidth;
        public int usHeight;
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
    public enum MessageBoxFlags
    {
        MSG_BOX_FLAG_USE_CENTERING_RECT = 0x0001,// Pass in a rect to center in
        MSG_BOX_FLAG_OK = 0x0002,	// Displays OK button
        MSG_BOX_FLAG_YESNO = 0x0004,// Displays YES NO buttons
        MSG_BOX_FLAG_CANCEL = 0x0008,// Displays YES NO buttons
        MSG_BOX_FLAG_FOUR_NUMBERED_BUTTONS = 0x0010,// Displays four numbered buttons, 1-4
        MSG_BOX_FLAG_YESNOCONTRACT = 0x0020,     // yes no and contract buttons
        MSG_BOX_FLAG_OKCONTRACT = 0x0040,	// ok and contract buttons
        MSG_BOX_FLAG_YESNOLIE = 0x0080,		// ok and contract buttons
        MSG_BOX_FLAG_CONTINUESTOP = 0x0100,	// continue stop box
        MSG_BOX_FLAG_OKSKIP = 0x0200,		// Displays ok or skip (meanwhile) buttons
        MSG_BOX_FLAG_GENERICCONTRACT = 0x0400,// displays contract buttoin + 2 user-defined text buttons
        MSG_BOX_FLAG_GENERIC = 0x0800,			// 2 user-defined text buttons
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
}
