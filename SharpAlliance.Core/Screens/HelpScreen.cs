﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Core.SubSystems.LaptopSubSystem;
using SharpAlliance.Platform;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core.Screens;

public class HelpScreen : IScreen
{
    //The defualt size and placement of the screen
    private const int HELP_SCREEN_DEFUALT_LOC_X = 155;
    private const int HELP_SCREEN_DEFUALT_LOC_Y = 105;
    private const int HELP_SCREEN_DEFUALT_LOC_WIDTH = HELP_SCREEN_SMALL_LOC_WIDTH + HELP_SCREEN_BUTTON_BORDER_WIDTH;
    private const int HELP_SCREEN_DEFUALT_LOC_HEIGHT = 292;//300;
    private const int HELP_SCREEN_BUTTON_BORDER_WIDTH = 92;
    private const int HELP_SCREEN_SMALL_LOC_WIDTH = 320;
    private const int HELP_SCREEN_SMALL_LOC_HEIGHT = HELP_SCREEN_DEFUALT_LOC_HEIGHT; //224;
    private const int HELP_SCREEN_BTN_OFFSET_X = 11;
    private const int HELP_SCREEN_BTN_OFFSET_Y = 12;//50;
    private const FontColor HELP_SCREEN_BTN_FONT_ON_COLOR = (FontColor)73;
    private const FontColor HELP_SCREEN_BTN_FONT_OFF_COLOR = FontColor.FONT_MCOLOR_WHITE;
    private const int HELP_SCREEN_BTN_FONT_BACK_COLOR = 50;
    private const FontStyle HELP_SCREEN_BTN_FONT = FontStyle.FONT10ARIAL;
    private const int HELP_SCREEN_BTN_WIDTH = 77;
    private const int HELP_SCREEN_BTN_HEIGHT = 22;
    private const int HELP_SCREEN_GAP_BN_BTNS = 8;
    private const int HELP_SCREEN_MARGIN_SIZE = 10;
    private const int HELP_SCREEN_TEXT_RIGHT_MARGIN_SPACE = 36;
    private const int HELP_SCREEN_TEXT_LEFT_MARGIN_WITH_BTN = (HELP_SCREEN_BUTTON_BORDER_WIDTH + 5 + HELP_SCREEN_MARGIN_SIZE);
    private const int HELP_SCREEN_TEXT_LEFT_MARGIN = (5 + HELP_SCREEN_MARGIN_SIZE);
    private const int HELP_SCREEN_TEXT_OFFSET_Y = 48;
    private const int HELP_SCREEN_GAP_BTN_LINES = 2;
    private const FontStyle HELP_SCREEN_TITLE_BODY_FONT = FontStyle.FONT12ARIAL;
    private const FontColor HELP_SCREEN_TITLE_BODY_COLOR = FontColor.FONT_MCOLOR_WHITE;//FONT_NEARBLACK;
    private const FontStyle HELP_SCREEN_TEXT_BODY_FONT = FontStyle.FONT10ARIAL;
    private const FontColor HELP_SCREEN_TEXT_BODY_COLOR = FontColor.FONT_MCOLOR_WHITE;//FONT_NEARBLACK;
    private const int HELP_SCREEN_TEXT_BACKGROUND = 0;//NO_SHADOW//FONT_MCOLOR_WHITE;
    private const int HELP_SCREEN_TITLE_OFFSET_Y = 7;
    private const int HELP_SCREEN_HELP_REMINDER_Y = HELP_SCREEN_TITLE_OFFSET_Y + 15;
    private const int HELP_SCREEN_NUM_BTNS = 8;
    private const int HELP_SCREEN_SHOW_HELP_AGAIN_REGION_OFFSET_X = 4;
    private const int HELP_SCREEN_SHOW_HELP_AGAIN_REGION_OFFSET_Y = 18;
    private const int HELP_SCREEN_SHOW_HELP_AGAIN_REGION_TEXT_OFFSET_X = 25 + HELP_SCREEN_SHOW_HELP_AGAIN_REGION_OFFSET_X;
    private const int HELP_SCREEN_SHOW_HELP_AGAIN_REGION_TEXT_OFFSET_Y = (HELP_SCREEN_SHOW_HELP_AGAIN_REGION_OFFSET_Y);
    private const int HELP_SCREEN_EXIT_BTN_OFFSET_X = 291;
    private const int HELP_SCREEN_EXIT_BTN_LOC_Y = 9;

    //the type of help screen
    private const int HLP_SCRN_DEFAULT_TYPE = 9;
    private const int HLP_SCRN_BUTTON_BORDER = 8;



    //this is the size of the text buffer where everything will be blitted.
    // 2 ( bytest for char ) * width of buffer * height of 1 line * # of text lines
    //#define	HLP_SCRN__NUMBER_BYTES_IN_TEXT_BUFFER						( 2 * HLP_SCRN__WIDTH_OF_TEXT_BUFFER * HLP_SCRN__HEIGHT_OF_1_LINE_IN_BUFFER * HLP_SCRN__MAX_NUMBER_OF_LINES_IN_BUFFER )
    private const int HLP_SCRN__WIDTH_OF_TEXT_BUFFER = 280;
    private const int HLP_SCRN__MAX_NUMBER_OF_LINES_IN_BUFFER = 170;//100
    private static int HLP_SCRN__HEIGHT_OF_1_LINE_IN_BUFFER = (FontSubSystem.GetFontHeight(HELP_SCREEN_TEXT_BODY_FONT) + HELP_SCREEN_GAP_BTN_LINES);
    private const int HLP_SCRN__MAX_NUMBER_PIXELS_DISPLAYED_IN_TEXT_BUFFER = HELP_SCREEN_DEFUALT_LOC_HEIGHT;
    private static int HLP_SCRN__HEIGHT_OF_TEXT_BUFFER = (HLP_SCRN__HEIGHT_OF_1_LINE_IN_BUFFER * HLP_SCRN__MAX_NUMBER_OF_LINES_IN_BUFFER);
    private static int HLP_SCRN__MAX_NUMBER_DISPLAYED_LINES_IN_BUFFER = (HLP_SCRN__HEIGHT_OF_TEXT_AREA / HLP_SCRN__HEIGHT_OF_1_LINE_IN_BUFFER);
    private const int HLP_SCRN__HEIGHT_OF_TEXT_AREA = 228;
    private const int HLP_SCRN__HEIGHT_OF_SCROLL_AREA = 182;
    private const int HLP_SCRN__WIDTH_OF_SCROLL_AREA = 20;
    private const int HLP_SCRN__SCROLL_POSX = 292;
    private static int HLP_SCRN__SCROLL_POSY = (gHelpScreen.usScreenLoc.Y + 63);
    private const int HLP_SCRN__SCROLL_UP_ARROW_X = 292;
    private const int HLP_SCRN__SCROLL_UP_ARROW_Y = 43;
    private const int HLP_SCRN__SCROLL_DWN_ARROW_X = HLP_SCRN__SCROLL_UP_ARROW_X;
    private const int HLP_SCRN__SCROLL_DWN_ARROW_Y = HLP_SCRN__SCROLL_UP_ARROW_Y + 202;
    private static IInputManager inputs;
    private static IVideoManager video;
    private static MOUSE_REGION gHelpScreenFullScreenMask = new(nameof(gHelpScreenFullScreenMask));
    private static GUI_BUTTON gHelpScreenDontShowHelpAgainToggle;
    private static List<GUI_BUTTON> buttonList = [];
    private static ButtonPic giExitBtnImage;
    private static GUI_BUTTON guiHelpScreenExitBtn;
    private static GUI_BUTTON[] guiHelpScreenBtns = new GUI_BUTTON[HELP_SCREEN_NUM_BTNS];
    private static ButtonPic[] giHelpScreenButtonsImage = new ButtonPic[HELP_SCREEN_NUM_BTNS];
    private static GUI_BUTTON[] giHelpScreenScrollArrows = new GUI_BUTTON[2];
    private static HVOBJECT guiHelpScreenBackGround;

    public bool IsInitialized { get; set; }
    public ScreenState State { get; set; }

    public HelpScreen(
        ILogger<HelpScreen> logger,
        IVideoManager videoManager,
        IInputManager inputManager)
    {
        inputs = inputManager;
        video = videoManager;
    }

    public static bool ShouldTheHelpScreenComeUp(HELP_SCREEN ubScreenID, bool fForceHelpScreenToComeUp)
    {

        //if the screen is being forsced to come up ( user pressed 'h' )
        if (fForceHelpScreenToComeUp)
        {
            //Set thefact that the user broughtthe help screen up
            gHelpScreen.fForceHelpScreenToComeUp = true;

            goto HELP_SCREEN_SHOULD_COME_UP;
        }

        //if we are already in the help system, return true
        if (gHelpScreen.uiFlags.HasFlag(HELP_SCREEN_ACTIVE.Yes))
        {
            return (true);
        }

        //has the player been in the screen before
        if (((gHelpScreen.usHasPlayerSeenHelpScreenInCurrentScreen >> (byte)ubScreenID) & 0x01) != 0)
        {
            goto HELP_SCREEN_WAIT_1_FRAME;
        }

        //if we have already been in the screen, and the user DIDNT press 'h', leave
        if (gHelpScreen.fHaveAlreadyBeenInHelpScreenSinceEnteringCurrenScreen)
        {
            return (false);
        }

        //should the screen come up, based on the users choice for it automatically coming up
        //	if( !( gHelpScreen.fHideHelpInAllScreens ) )
        {
            //		goto HELP_SCREEN.WAIT_1_FRAME;
        }

        //the help screen shouldnt come up
        return (false);

    HELP_SCREEN_WAIT_1_FRAME:

        // we have to wait 1 frame while the screen renders
        if (gHelpScreen.bDelayEnteringHelpScreenBy1FrameCount < 2)
        {
            gHelpScreen.bDelayEnteringHelpScreenBy1FrameCount += 1;

            ButtonSubSystem.UnmarkButtonsDirty(Laptop.buttonList);

            return (false);
        }

    HELP_SCREEN_SHOULD_COME_UP:

        //Record which screen it is

        //if its mapscreen
        if (ubScreenID == HELP_SCREEN.MAPSCREEN)
        {
            //determine which screen it is ( is any mercs hired, did game just start )
            gHelpScreen.bCurrentHelpScreen = HelpScreenDetermineWhichMapScreenHelpToShow();
        }
        else
        {
            gHelpScreen.bCurrentHelpScreen = ubScreenID;
        }

        //mark it that the help screnn is enabled
        gHelpScreen.uiFlags |= HELP_SCREEN_ACTIVE.Yes;

        // reset
        gHelpScreen.bDelayEnteringHelpScreenBy1FrameCount = 0;

        return (true);
    }

    internal static void HelpScreenHandler()
    {
        //if we are just entering the help screen
        if (HelpScreenSubSystem.gfHelpScreenEntry)
        {
            //setup the help screen
            EnterHelpScreen();

            HelpScreenSubSystem.gfHelpScreenEntry = false;
            HelpScreenSubSystem.gfHelpScreenExit = false;
        }

        video.RestoreBackgroundRects();


        //get the mouse and keyboard inputs
        GetHelpScreenUserInput();

        //handle the help screen
        HandleHelpScreen();

        //if the help screen is dirty, re-render it
        if (gHelpScreen.ubHelpScreenDirty != HLP_SCRN_DRTY_LVL.NOT_DIRTY)
        {
            //temp
            //		gHelpScreen.ubHelpScreenDirty = HLP_SCRN_DRTY_LVL.REFRESH_ALL;


            RenderHelpScreen();
            gHelpScreen.ubHelpScreenDirty = HLP_SCRN_DRTY_LVL.NOT_DIRTY;
        }

        // render buttons marked dirty	
        //  MarkButtonsDirty( );
        ButtonSubSystem.RenderButtons(Laptop.buttonList);

        video.SaveBackgroundRects();
        GuiManager.RenderButtonsFastHelp();

        video.ExecuteBaseDirtyRectQueue();
        video.EndFrameBufferRender();

        //if we are leaving the help screen
        if (HelpScreenSubSystem.gfHelpScreenExit)
        {
            HelpScreenSubSystem.gfHelpScreenExit = false;

            HelpScreenSubSystem.gfHelpScreenEntry = true;

            //exit mouse regions etc..
            ExitHelpScreen();

            //reset the helpscreen id
            gHelpScreen.bCurrentHelpScreen = HELP_SCREEN.UNSET;
        }
    }

    private static void ExitHelpScreen()
    {
        int i;

        if (!gHelpScreen.fForceHelpScreenToComeUp)
        {
            //Get the current value of the checkbox
            if (gHelpScreenDontShowHelpAgainToggle.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
                gGameSettings.fHideHelpInAllScreens = true;
                gHelpScreen.usHasPlayerSeenHelpScreenInCurrentScreen = 0;
            }
            else
            {
                gGameSettings.fHideHelpInAllScreens = false;
            }

            //remove the mouse region for the '[ ] dont show help...'
            ButtonSubSystem.RemoveButton(gHelpScreenDontShowHelpAgainToggle);
        }


        //mark it that the help screen is not active
        gHelpScreen.uiFlags &= ~HELP_SCREEN_ACTIVE.Yes;

        //remove the mouse region that blankets
        MouseSubSystem.MSYS_RemoveRegion(gHelpScreenFullScreenMask);

        //checkbox to toggle show help again toggle
        //	MSYS_RemoveRegion( &HelpScreenDontShowHelpAgainToggleTextRegion );


        //remove the hepl graphic
        // video.DeleteVideoObjectFromIndex(guiHelpScreenBackGround);


        //remove the exit button
        ButtonSubSystem.RemoveButton(guiHelpScreenExitBtn);

        //if there are any buttons, remove them
        if (gHelpScreen.bNumberOfButtons != 0)
        {
            for (i = 0; i < gHelpScreen.bNumberOfButtons; i++)
            {
                ButtonSubSystem.UnloadButtonImage(giHelpScreenButtonsImage[i]);
                ButtonSubSystem.RemoveButton(guiHelpScreenBtns[i]);
            }
        }

        //destroy the text buffer for the help screen
        DestroyHelpScreenTextBuffer();


        //Handles the dirtying of any special screen we are about to reenter
        HelpScreenSpecialExitCode();

        //if the game was NOT paused
        if (gHelpScreen.fWasTheGamePausedPriorToEnteringHelpScreen == false)
        {
            //un pause the game
            GameClock.UnPauseGame();
        }

        //Delete the scroll box, and scroll arrow regions/buttons
        DeleteScrollArrowButtons();

        //reset
        gHelpScreen.fForceHelpScreenToComeUp = false;

        GameSettings.SaveGameSettings();
    }

    private static void DestroyHelpScreenTextBuffer()
    {
        //        DeleteVideoSurfaceFromIndex(guiHelpScreenTextBufferSurface);
    }

    private static void HelpScreenSpecialExitCode()
    {
        //switch on the current screen
        switch (gHelpScreen.bCurrentHelpScreen)
        {
            case HELP_SCREEN.LAPTOP:
                fReDrawScreenFlag = true;
                break;

            case HELP_SCREEN.MAPSCREEN_NO_ONE_HIRED:
            case HELP_SCREEN.MAPSCREEN_NOT_IN_ARULCO:
            case HELP_SCREEN.MAPSCREEN_SECTOR_INVENTORY:
            case HELP_SCREEN.MAPSCREEN:
                fCharacterInfoPanelDirty = true;
                fTeamPanelDirty = true;
                fMapScreenBottomDirty = true;
                fMapPanelDirty = true;
                break;

            case HELP_SCREEN.TACTICAL:
                fInterfacePanelDirty = DIRTYLEVEL2;
                RenderWorld.SetRenderFlags(RenderingFlags.FULL);
                break;

            case HELP_SCREEN.OPTIONS:
                break;
            case HELP_SCREEN.LOAD_GAME:
                break;

            default:
                break;
        }
    }

    private static void RenderHelpScreen()
    {
        //rrr

        if (gfHaveRenderedFirstFrameToSaveBuffer)
        {
            //Restore the background before blitting the text back on
            RenderDirty.RestoreExternBackgroundRect(gHelpScreen.usScreenLoc.X, gHelpScreen.usScreenLoc.Y, gHelpScreen.usScreenSize.Width, gHelpScreen.usScreenSize.Height);
        }


        if (gHelpScreen.ubHelpScreenDirty == HLP_SCRN_DRTY_LVL.REFRESH_ALL)
        {
            //Display the helpscreen background
            DrawHelpScreenBackGround();

            //Display the current screens title, and footer info
            DisplayCurrentScreenTitleAndFooter();
        }


        if (!gfHaveRenderedFirstFrameToSaveBuffer)
        {
            gfHaveRenderedFirstFrameToSaveBuffer = true;

            //blit everything to the save buffer ( cause the save buffer can bleed through )
            video.BlitBufferToBuffer(
                SurfaceType.RENDER_BUFFER, 
                SurfaceType.SAVE_BUFFER,
                new(gHelpScreen.usScreenLoc.X, gHelpScreen.usScreenLoc.Y, (int)(gHelpScreen.usScreenLoc.X + gHelpScreen.usScreenSize.Width), (int)(gHelpScreen.usScreenLoc.Y + gHelpScreen.usScreenSize.Height)));

            ButtonSubSystem.UnmarkButtonsDirty(buttonList);
        }


        //render the text buffer to the screen
        if (gHelpScreen.ubHelpScreenDirty >= HLP_SCRN_DRTY_LVL.REFRESH_TEXT)
        {
            RenderTextBufferToScreen();
        }
    }

    private static bool DrawHelpScreenBackGround()
    {
        int usPosX;

        //Get and display the background image

        usPosX = gHelpScreen.usScreenLoc.X;

        //if there are buttons, blit the button border
        if (gHelpScreen.bNumberOfButtons != 0)
        {
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiHelpScreenBackGround, HLP_SCRN_BUTTON_BORDER, usPosX, gHelpScreen.usScreenLoc.Y, VO_BLT.SRCTRANSPARENCY);
            usPosX += HELP_SCREEN_BUTTON_BORDER_WIDTH;
        }

        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiHelpScreenBackGround, HLP_SCRN_DEFAULT_TYPE, usPosX, gHelpScreen.usScreenLoc.Y, VO_BLT.SRCTRANSPARENCY);

        video.InvalidateRegion(gHelpScreen.usScreenLoc.X, gHelpScreen.usScreenLoc.Y, gHelpScreen.usScreenLoc.X + gHelpScreen.usScreenSize.Width, gHelpScreen.usScreenLoc.Y + gHelpScreen.usScreenSize.Height);

        return true;
    }

    private static void RenderTextBufferToScreen()
    {
        HVSURFACE hDestVSurface, hSrcVSurface;
        Rectangle SrcRect;


        video.GetVideoSurface(out hDestVSurface, guiRENDERBUFFER);
        video.GetVideoSurface(out hSrcVSurface, guiHelpScreenTextBufferSurface);

        SrcRect = new()
        {
            X = 0,
            Y = gHelpScreen.iLineAtTopOfTextBuffer * HLP_SCRN__HEIGHT_OF_1_LINE_IN_BUFFER,
            Width = HLP_SCRN__WIDTH_OF_TEXT_BUFFER,
            Height = 0 + HLP_SCRN__HEIGHT_OF_TEXT_AREA - (2 * 8),
        };

        video.BltVSurfaceUsingDD(hDestVSurface, hSrcVSurface, VO_BLT.SRCTRANSPARENCY, gHelpScreen.usLeftMarginPosX, (gHelpScreen.usScreenLoc.Y + HELP_SCREEN_TEXT_OFFSET_Y), out SrcRect);

        DisplayHelpScreenTextBufferScrollBox();
    }

    private static void DisplayHelpScreenTextBufferScrollBox()
    {
        int iSizeOfBox;
        int iTopPosScrollBox = 0;
        Image<Rgba32> pDestBuf;
        int uiDestPitchBYTES = 0;
        int usPosX;

        if (gHelpScreen.bNumberOfButtons != 0)
        {
            usPosX = gHelpScreen.usScreenLoc.X + HLP_SCRN__SCROLL_POSX + HELP_SCREEN_BUTTON_BORDER_WIDTH;
        }
        else
        {
            usPosX = gHelpScreen.usScreenLoc.X + HLP_SCRN__SCROLL_POSX;
        }

        //
        //first calculate the height of the scroll box
        //

        CalculateHeightAndPositionForHelpScreenScrollBox(out iSizeOfBox, out iTopPosScrollBox);

        //
        // next draw the box
        //

        //if there ARE scroll bars, draw the 
        if (!(gHelpScreen.usTotalNumberOfLinesInBuffer <= HLP_SCRN__MAX_NUMBER_DISPLAYED_LINES_IN_BUFFER))
        {
            video.ColorFillVideoSurfaceArea(
                SurfaceType.FRAME_BUFFER,
                new(usPosX, iTopPosScrollBox, usPosX + HLP_SCRN__WIDTH_OF_SCROLL_AREA, iTopPosScrollBox + iSizeOfBox - 1),
                FROMRGB(227, 198, 88));

            //display the line
            //pDestBuf = LockVideoSurface(FRAME_BUFFER, &uiDestPitchBYTES);
            video.SetClippingRegionAndImageWidth(uiDestPitchBYTES, new(0, 0, 640, 480));

            // draw the gold highlite line on the top and left
            video.LineDraw(false, new(usPosX, iTopPosScrollBox), new(usPosX + HLP_SCRN__WIDTH_OF_SCROLL_AREA, iTopPosScrollBox), FROMRGB(235, 222, 171), pDestBuf);
            video.LineDraw(false, new(usPosX, iTopPosScrollBox), new(usPosX, iTopPosScrollBox + iSizeOfBox - 1), FROMRGB(235, 222, 171), pDestBuf);

            // draw the shadow line on the bottom and right
            video.LineDraw(false,
                new(usPosX, iTopPosScrollBox + iSizeOfBox - 1),
                new(usPosX + HLP_SCRN__WIDTH_OF_SCROLL_AREA, iTopPosScrollBox + iSizeOfBox - 1),
                FROMRGB(65, 49, 6),
                pDestBuf);

            video.LineDraw(false,
                new(usPosX + HLP_SCRN__WIDTH_OF_SCROLL_AREA, iTopPosScrollBox), new(usPosX + HLP_SCRN__WIDTH_OF_SCROLL_AREA, iTopPosScrollBox + iSizeOfBox - 1),
                FROMRGB(65, 49, 6),
                pDestBuf);

            // unlock frame buffer
            // video.UnLockVideoSurface(SurfaceType.FRAME_BUFFER);
        }
    }

    private static void CalculateHeightAndPositionForHelpScreenScrollBox(out int piHeightOfScrollBox, out int piTopOfScrollBox)
    {
        int iSizeOfBox, iTopPosScrollBox;
        float dPercentSizeOfBox = 0;
        float dTemp = 0;

        dPercentSizeOfBox = HLP_SCRN__MAX_NUMBER_DISPLAYED_LINES_IN_BUFFER / (float)gHelpScreen.usTotalNumberOfLinesInBuffer;

        //if the # is >= 1 then the box is the full size of the scroll area
        if (dPercentSizeOfBox >= 1.0)
        {
            iSizeOfBox = HLP_SCRN__HEIGHT_OF_SCROLL_AREA;

            //no need to calc the top spot for the box
            iTopPosScrollBox = HLP_SCRN__SCROLL_POSY;
        }
        else
        {
            iSizeOfBox = (int)(dPercentSizeOfBox * HLP_SCRN__HEIGHT_OF_SCROLL_AREA + 0.5);

            //
            //next, calculate the top position of the box
            //
            dTemp = (HLP_SCRN__HEIGHT_OF_SCROLL_AREA / (float)gHelpScreen.usTotalNumberOfLinesInBuffer) * gHelpScreen.iLineAtTopOfTextBuffer;

            iTopPosScrollBox = (int)(dTemp + .5) + HLP_SCRN__SCROLL_POSY;
        }

        piHeightOfScrollBox = iSizeOfBox;
        piTopOfScrollBox = iTopPosScrollBox;
    }

    private static void HandleHelpScreen()
    {
        //if any of the possible screens need to have a some code done every loop..  its done in here
        SpecialHandlerCode();

        if (gfScrollBoxIsScrolling)
        {
            if (inputs.gfLeftButtonState)
            {
                HelpScreenMouseMoveScrollBox(gusMouseYPos);
            }
            else
            {
                gfScrollBoxIsScrolling = false;
                gHelpScreen.iLastMouseClickY = -1;
            }
        }

        if (gubRenderHelpScreenTwiceInaRow < 3)
        {
            //test
            //		gHelpScreen.ubHelpScreenDirty = HLP_SCRN_DRTY_LVL.REFRESH_ALL; 

            gubRenderHelpScreenTwiceInaRow++;

            ButtonSubSystem.UnmarkButtonsDirty(buttonList);
        }

        // refresh all of help screens buttons
        RefreshAllHelpScreenButtons();
    }

    private static void RefreshAllHelpScreenButtons()
    {
        //loop through all the buttons, and refresh them
        for (int i = 0; i < gHelpScreen.bNumberOfButtons; i++)
        {
            guiHelpScreenBtns[i].uiFlags |= ButtonFlags.BUTTON_DIRTY;
        }

        guiHelpScreenExitBtn.uiFlags |= ButtonFlags.BUTTON_DIRTY;

        if (!gHelpScreen.fForceHelpScreenToComeUp)
        {
            gHelpScreenDontShowHelpAgainToggle.uiFlags |= ButtonFlags.BUTTON_DIRTY;
        }

        giHelpScreenScrollArrows[0].uiFlags |= ButtonFlags.BUTTON_DIRTY;
        giHelpScreenScrollArrows[1].uiFlags |= ButtonFlags.BUTTON_DIRTY;
    }

    private static void HelpScreenMouseMoveScrollBox(int usMousePosY)
    {
        int iPosY, iHeight;
        int iNumberOfIncrements = 0;
        float dSizeOfIncrement = (HLP_SCRN__HEIGHT_OF_SCROLL_AREA / (float)gHelpScreen.usTotalNumberOfLinesInBuffer);
        float dTemp;
        int iNewPosition;

        CalculateHeightAndPositionForHelpScreenScrollBox(out iHeight, out iPosY);

        if (AreWeClickingOnScrollBar(usMousePosY) || gHelpScreen.iLastMouseClickY != -1)
        {
            if (gHelpScreen.iLastMouseClickY == -1)
            {
                gHelpScreen.iLastMouseClickY = usMousePosY;
            }

            if (usMousePosY < gHelpScreen.iLastMouseClickY)
            {
                //			iNewPosition = iPosY - ( int)( dSizeOfIncrement + .5);
                iNewPosition = iPosY - (gHelpScreen.iLastMouseClickY - usMousePosY);

            }
            else if (usMousePosY > gHelpScreen.iLastMouseClickY)
            {
                //			iNewPosition = iPosY + ( int)( dSizeOfIncrement + .5);
                iNewPosition = iPosY + usMousePosY - gHelpScreen.iLastMouseClickY;
            }
            else
            {
                return;
            }

            dTemp = (iNewPosition - iPosY) / dSizeOfIncrement;

            if (dTemp < 0)
            {
                iNumberOfIncrements = (int)(dTemp - 0.5);
            }
            else
            {
                iNumberOfIncrements = (int)(dTemp + 0.5);
            }

            gHelpScreen.iLastMouseClickY = usMousePosY;

            //		return;
        }
        else
        {
            //if the mouse is higher then the top of the scroll area, set it to the top of the scroll area
            if (usMousePosY < HLP_SCRN__SCROLL_POSY)
            {
                usMousePosY = HLP_SCRN__SCROLL_POSY;
            }

            dTemp = (usMousePosY - iPosY) / dSizeOfIncrement;

            if (dTemp < 0)
            {
                iNumberOfIncrements = (int)(dTemp - 0.5);
            }
            else
            {
                iNumberOfIncrements = (int)(dTemp + 0.5);
            }
        }

        //if there has been a change
        if (iNumberOfIncrements != 0)
        {
            ChangeTopLineInTextBufferByAmount(iNumberOfIncrements);
        }
    }

    // - is up, + is down
    private static void ChangeTopLineInTextBufferByAmount(int iAmouontToMove)
    {
        //if we are moving up
        if (iAmouontToMove < 0)
        {
            if (gHelpScreen.iLineAtTopOfTextBuffer + iAmouontToMove >= 0)
            {
                //if we can move up by the requested amount
                if ((gHelpScreen.usTotalNumberOfLinesInBuffer - gHelpScreen.iLineAtTopOfTextBuffer) > iAmouontToMove)
                {
                    gHelpScreen.iLineAtTopOfTextBuffer += iAmouontToMove;
                }

                //else, trying to move past the top
                else
                {
                    gHelpScreen.iLineAtTopOfTextBuffer = 0;
                }
            }
            else
            {
                gHelpScreen.iLineAtTopOfTextBuffer = 0;
            }
        }

        //else we are moving down
        else
        {
            //if we dont have to scroll cause there is not enough text
            if (gHelpScreen.usTotalNumberOfLinesInBuffer <= HLP_SCRN__MAX_NUMBER_DISPLAYED_LINES_IN_BUFFER)
            {
                gHelpScreen.iLineAtTopOfTextBuffer = 0;
            }
            else
            {
                if ((gHelpScreen.iLineAtTopOfTextBuffer + HLP_SCRN__MAX_NUMBER_DISPLAYED_LINES_IN_BUFFER + iAmouontToMove) <= gHelpScreen.usTotalNumberOfLinesInBuffer)
                {
                    gHelpScreen.iLineAtTopOfTextBuffer += iAmouontToMove;
                }
                else
                {
                    gHelpScreen.iLineAtTopOfTextBuffer = gHelpScreen.usTotalNumberOfLinesInBuffer - HLP_SCRN__MAX_NUMBER_DISPLAYED_LINES_IN_BUFFER;
                }
            }
        }

        //	RenderCurrentHelpScreenTextToBuffer();

        gHelpScreen.ubHelpScreenDirty = HLP_SCRN_DRTY_LVL.REFRESH_TEXT;
    }

    private static bool AreWeClickingOnScrollBar(int usMousePosY)
    {
        int iPosY, iHeight;

        CalculateHeightAndPositionForHelpScreenScrollBox(out iHeight, out iPosY);

        if (usMousePosY >= iPosY && usMousePosY < (iPosY + iHeight))
        {
            return (true);
        }
        else
        {
            return (false);
        }
    }

    private static void SpecialHandlerCode()
    {
        //switch on the current screen
        switch (gHelpScreen.bCurrentHelpScreen)
        {
            case HELP_SCREEN.LAPTOP:
                Laptop.PrintDate();
                Laptop.PrintBalance();
                Laptop.PrintNumberOnTeam();
                break;
            case HELP_SCREEN.MAPSCREEN:
                break;
            case HELP_SCREEN.TACTICAL:
                break;

            case HELP_SCREEN.MAPSCREEN_NO_ONE_HIRED:
                break;
            case HELP_SCREEN.MAPSCREEN_NOT_IN_ARULCO:
                break;
            case HELP_SCREEN.MAPSCREEN_SECTOR_INVENTORY:
                break;
            case HELP_SCREEN.OPTIONS:
                break;
            case HELP_SCREEN.LOAD_GAME:
                break;

            default:
                break;
        }
    }

    private static void GetHelpScreenUserInput()
    {
        Point MousePos;

        inputs.GetCursorPosition(out MousePos);

        while (inputs.DequeueEvent(out var Event))
        {
            MouseEvent me = Event.MouseEvents.LastOrDefault();

            // HOOK INTO MOUSE HOOKS
            switch (me.MouseButton)
            {
                case MouseButton.Left:
                    MouseSubSystem.MouseHook(MouseEvents.LEFT_BUTTON_DOWN, MousePos, inputs.gfLeftButtonState, inputs.gfRightButtonState);
                    break;
                case MouseButton.Right:
                    MouseSubSystem.MouseHook(MouseEvents.RIGHT_BUTTON_DOWN, MousePos, inputs.gfLeftButtonState, inputs.gfRightButtonState);
                    break;
//                case MouseButton.Left:// LEFT_BUTTON_UP:
//                    MouseSubSystem.MouseHook(MouseEvents.LEFT_BUTTON_UP, MousePos.X, MousePos.Y, _LeftButtonDown, _RightButtonDown);
//                    break;
//                case MouseButton.Right:// RIGHT_BUTTON_UP:
//                    MouseSubSystem.MouseHook(MouseEvents.RIGHT_BUTTON_UP, MousePos.X, MousePos.Y, _LeftButtonDown, _RightButtonDown);
//                    break;
//                case MouseButton.Right://_BUTTON_REPEAT:
//                    MouseSubSystem.MouseHook(MouseEvents.RIGHT_BUTTON_REPEAT, MousePos.X, MousePos.Y, _LeftButtonDown, _RightButtonDown);
//                    break;
//                case MouseButton.Left://LEFT_BUTTON_REPEAT:
//                    MouseSubSystem.MouseHook(MouseEvents.LEFT_BUTTON_REPEAT, MousePos.X, MousePos.Y, _LeftButtonDown, _RightButtonDown);
//                    break;
            }


            var keyEvent = Event!.KeyEvents.LastOrDefault();

            if (keyEvent.Down)
            {
                if (!inputs.HandleTextInput(Event))
                {
                    switch (keyEvent.Key)
                    {
                        case Key.Escape:
                            PrepareToExitHelpScreen();
                            break;

                        case Key.Down:
                            {
                                ChangeTopLineInTextBufferByAmount(1);
                            }
                            break;

                        case Key.Up:
                            {
                                ChangeTopLineInTextBufferByAmount(-1);
                            }
                            break;

                        case Key.PageUp:
                            {
                                ChangeTopLineInTextBufferByAmount(-(HLP_SCRN__MAX_NUMBER_DISPLAYED_LINES_IN_BUFFER - 1));
                            }
                            break;
                        case Key.PageDown:
                            {
                                ChangeTopLineInTextBufferByAmount((HLP_SCRN__MAX_NUMBER_DISPLAYED_LINES_IN_BUFFER - 1));
                            }
                            break;

                        case Key.Left:
                            ChangeToHelpScreenSubPage((gHelpScreen.bCurrentHelpScreenActiveSubPage - 1));
                            break;

                        case Key.Right:
                            ChangeToHelpScreenSubPage((gHelpScreen.bCurrentHelpScreenActiveSubPage + 1));
                            break;

                            /*

                                            case LEFTARROW:
                                            { 
                                            }
                                                break;

                                            case RIGHTARROW:
                                            { 
                                            }
                                                break;
                            */
                    }
                }

                //if (!HandleTextInput(Event) && Event.usEvent == KEY_REPEAT)
                if(keyEvent.Repeat)
                {
                    switch (keyEvent.Key)
                    {
                        case Key.Down:
                            {
                                ChangeTopLineInTextBufferByAmount(1);
                            }
                            break;

                        case Key.Up:
                            {
                                ChangeTopLineInTextBufferByAmount(-1);
                            }
                            break;

                        case Key.PageUp:
                            {
                                ChangeTopLineInTextBufferByAmount(-(HLP_SCRN__MAX_NUMBER_DISPLAYED_LINES_IN_BUFFER - 1));
                            }
                            break;
                        case Key.PageDown:
                            {
                                ChangeTopLineInTextBufferByAmount((HLP_SCRN__MAX_NUMBER_DISPLAYED_LINES_IN_BUFFER - 1));
                            }
                            break;
                    }
                }
            }
        }
    }

    private static bool EnterHelpScreen()
    {
        VOBJECT_DESC VObjectDesc;
        int usPosX, usPosY;//, usWidth, usHeight;
                           //	int	iStartLoc;
                           //	CHAR16 zText[1024];

        //Clear out all the save background rects
        RenderDirty.EmptyBackgroundRects();


        ButtonSubSystem.UnmarkButtonsDirty(buttonList);

        // remeber if the game was paused or not ( so when we exit we know what to do )
        gHelpScreen.fWasTheGamePausedPriorToEnteringHelpScreen = gfGamePaused;

        //pause the game
        GameClock.PauseGame();


        //Determine the help screen size, based off the help screen
        SetSizeAndPropertiesOfHelpScreen();

        //Create a mouse region 'mask' the entrire screen
        MouseSubSystem.MSYS_DefineRegion(
            gHelpScreenFullScreenMask,
            new(0, 0, 640, 480),
            MSYS_PRIORITY.HIGHEST,
            gHelpScreen.usCursor,
            MSYS_NO_CALLBACK,
            MSYS_NO_CALLBACK);

        MouseSubSystem.MSYS_AddRegion(ref gHelpScreenFullScreenMask);


        //Create the exit button
        if (gHelpScreen.bNumberOfButtons != 0)
        {
            usPosX = gHelpScreen.usScreenLoc.X + HELP_SCREEN_EXIT_BTN_OFFSET_X + HELP_SCREEN_BUTTON_BORDER_WIDTH;
        }
        else
        {
            usPosX = gHelpScreen.usScreenLoc.X + HELP_SCREEN_EXIT_BTN_OFFSET_X;
        }

        usPosY = gHelpScreen.usScreenLoc.Y + HELP_SCREEN_EXIT_BTN_LOC_Y;

        //Create the exit buttons
        giExitBtnImage = ButtonSubSystem.LoadButtonImage("INTERFACE\\HelpScreen.sti", -1, 0, 4, 2, 6);

        guiHelpScreenExitBtn = ButtonSubSystem.CreateIconAndTextButton(
            giExitBtnImage,
            "",
            HELP_SCREEN_BTN_FONT,
            HELP_SCREEN_BTN_FONT_ON_COLOR,  FontShadow.DEFAULT_SHADOW,
            HELP_SCREEN_BTN_FONT_OFF_COLOR, FontShadow.DEFAULT_SHADOW,
            ButtonTextJustifies.TEXT_CJUSTIFIED,
            new Point(usPosX, usPosY),
            ButtonFlags.BUTTON_TOGGLE,
            MSYS_PRIORITY.HIGHEST,
            MouseSubSystem.DefaultMoveCallback,
            BtnHelpScreenExitCallback);
        ButtonSubSystem.SetButtonFastHelpText(guiHelpScreenExitBtn, gzHelpScreenText[HLP_SCRN_TXT__EXIT_SCREEN]);
        ButtonSubSystem.SetButtonCursor(guiHelpScreenExitBtn, gHelpScreen.usCursor);



        //Create the buttons needed for the screen
        CreateHelpScreenButtons();


        //if there are buttons
        if (gHelpScreen.bNumberOfButtons != 0)
        {
            usPosX = gHelpScreen.usScreenLoc.X + HELP_SCREEN_SHOW_HELP_AGAIN_REGION_OFFSET_X + HELP_SCREEN_BUTTON_BORDER_WIDTH;
        }
        else
        {
            usPosX = gHelpScreen.usScreenLoc.X + HELP_SCREEN_SHOW_HELP_AGAIN_REGION_OFFSET_X;
        }

        usPosY = gHelpScreen.usScreenLoc.Y + gHelpScreen.usScreenSize.Height - HELP_SCREEN_SHOW_HELP_AGAIN_REGION_OFFSET_Y;

        if (!gHelpScreen.fForceHelpScreenToComeUp)
        {
            gHelpScreenDontShowHelpAgainToggle = ButtonSubSystem.CreateCheckBoxButton(
                usPosX,
                (int)(usPosY - 3),
                "INTERFACE\\OptionsCheckBoxes.sti",
                MSYS_PRIORITY.HIGHEST,
                BtnHelpScreenDontShowHelpAgainCallback);

            ButtonSubSystem.SetButtonCursor(gHelpScreenDontShowHelpAgainToggle, gHelpScreen.usCursor);

            // Set the state of the chec box
            if (gGameSettings.fHideHelpInAllScreens)
            {
                gHelpScreenDontShowHelpAgainToggle.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
            }
            else
            {
                gHelpScreenDontShowHelpAgainToggle.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
            }
        }

        /*
            ///creatre a region for the text that says ' [ x ] click to continue seeing ....'
            iStartLoc = HELPSCREEN_RECORD_SIZE * HLP_TXT_CONSTANT_FOOTER;
            LoadEncryptedDataFromFile(HELPSCREEN_FILE, zText, iStartLoc, HELPSCREEN_RECORD_SIZE );

            usWidth = StringPixLength( zText, HELP_SCREEN.TEXT_BODY_FONT );
            usHeight = GetFontHeight( HELP_SCREEN.TEXT_BODY_FONT );

        /*
            MSYS_DefineRegion( &HelpScreenDontShowHelpAgainToggleTextRegion, usPosX, usPosY, (int)(usPosX+usWidth), (int)(usPosY+usHeight), MSYS_PRIORITY_HIGHEST-1,
                                     gHelpScreen.usCursor, MSYS_NO_CALLBACK, HelpScreenDontShowHelpAgainToggleTextRegionCallBack ); 
          MSYS_AddRegion( &HelpScreenDontShowHelpAgainToggleTextRegion ); 
        */

        // load the help screen background graphic and add it
        guiHelpScreenBackGround = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\HelpScreen.sti"));

        //create the text buffer
        CreateHelpScreenTextBuffer();


        //make sure we redraw everything
        gHelpScreen.ubHelpScreenDirty = HLP_SCRN_DRTY_LVL.REFRESH_ALL;

        //mark it that we have been in since we enter the current screen
        gHelpScreen.fHaveAlreadyBeenInHelpScreenSinceEnteringCurrenScreen = true;

        //set the fact that we have been to the screen
        gHelpScreen.usHasPlayerSeenHelpScreenInCurrentScreen &= (ushort)~(1 << (ushort)gHelpScreen.bCurrentHelpScreen);

        //always start at the top
        gHelpScreen.iLineAtTopOfTextBuffer = 0;

        //set it so there was no previous click
        gHelpScreen.iLastMouseClickY = -1;

        //Create the scroll box, and scroll arrow regions/buttons
        CreateScrollAreaButtons();

        //render the active page to the text buffer
        ChangeHelpScreenSubPage();

        //reset scroll box flag
        gfScrollBoxIsScrolling = false;

        //reset first frame buffer
        gfHaveRenderedFirstFrameToSaveBuffer = false;

        gubRenderHelpScreenTwiceInaRow = 0;

        return (true);
    }

    private static void CreateHelpScreenButtons()
    {
        int usPosX, usPosY;
        string sText = string.Empty;
        int i;

        //if there are buttons to create
        if (gHelpScreen.bNumberOfButtons != 0)
        {

            usPosX = gHelpScreen.usScreenLoc.X + HELP_SCREEN_BTN_OFFSET_X;
            usPosY = HELP_SCREEN_BTN_OFFSET_Y + gHelpScreen.usScreenLoc.Y;


            //loop through all the buttons, and create them
            for (i = 0; i < gHelpScreen.bNumberOfButtons; i++)
            {
                //get the text for the button
                GetHelpScreenText(gHelpScreenBtnTextRecordNum[gHelpScreen.bCurrentHelpScreen].iButtonTextNum[i], sText);

                /*
                            guiHelpScreenBtns[i] = CreateTextButton( sText, HELP_SCREEN_BTN_FONT, HELP_SCREEN_BTN_FONT_COLOR, HELP_SCREEN_BTN_FONT_BACK_COLOR, 
                                    BUTTON_USE_DEFAULT, usPosX, usPosY, HELP_SCREEN_BTN_WIDTH, HELP_SCREEN_BTN_HEIGHT, 
                                    BUTTON_TOGGLE, MSYS_PRIORITY_HIGHEST, BUTTON_NO_CALLBACK, BtnHelpScreenBtnsCallback );
                */


                giHelpScreenButtonsImage[i] = ButtonSubSystem.UseLoadedButtonImage(giExitBtnImage, -1, 1, 5, 3, 7);

                guiHelpScreenBtns[i] = ButtonSubSystem.CreateIconAndTextButton(
                    giHelpScreenButtonsImage[i],
                    sText,
                    HELP_SCREEN_BTN_FONT,
                    HELP_SCREEN_BTN_FONT_ON_COLOR,  
                    FontShadow.DEFAULT_SHADOW,
                    HELP_SCREEN_BTN_FONT_OFF_COLOR, 
                    FontShadow.DEFAULT_SHADOW,
                    ButtonTextJustifies.TEXT_CJUSTIFIED,
                    new Point(usPosX, usPosY),
                    ButtonFlags.BUTTON_TOGGLE,
                    MSYS_PRIORITY.HIGHEST,
                    MouseSubSystem.DefaultMoveCallback,
                    BtnHelpScreenBtnsCallback);

                ButtonSubSystem.SetButtonCursor(guiHelpScreenBtns[i], gHelpScreen.usCursor);
                ButtonSubSystem.MSYS_SetBtnUserData(guiHelpScreenBtns[i], 0, i);

                //	SpecifyButtonTextOffsets( guiHelpScreenBtns[i], 19, 9, TRUE );

                usPosY += HELP_SCREEN_BTN_HEIGHT + HELP_SCREEN_GAP_BN_BTNS;
            }

            guiHelpScreenBtns[0].uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
        }
    }

    private static void BtnHelpScreenBtnsCallback(ref GUI_BUTTON button, MSYS_CALLBACK_REASON reason)
    {
        throw new NotImplementedException();
    }

    private static void SetSizeAndPropertiesOfHelpScreen()
    {

        //new screen:
        gHelpScreen.bNumberOfButtons = 0;

        //
        //these are the default settings, so if the screen uses different then defualt, set them in the switch
        //
        {
            gHelpScreen.usScreenSize.Width = HELP_SCREEN_DEFUALT_LOC_WIDTH;
            gHelpScreen.usScreenSize.Height = HELP_SCREEN_DEFUALT_LOC_HEIGHT;

            gHelpScreen.usScreenLoc.X = (640 - gHelpScreen.usScreenSize.Width) / 2;
            gHelpScreen.usScreenLoc.Y = (480 - gHelpScreen.usScreenSize.Height) / 2;

            gHelpScreen.bCurrentHelpScreenActiveSubPage = 0;

            gHelpScreen.usCursor = CURSOR.NORMAL;
        }


        switch (gHelpScreen.bCurrentHelpScreen)
        {
            case HELP_SCREEN.LAPTOP:
                gHelpScreen.bNumberOfButtons = HLP_SCRN_LPTP_NUM_PAGES;
                gHelpScreen.usCursor = CURSOR.LAPTOP_SCREEN;

                //center the screen inside the laptop screen
                gHelpScreen.usScreenLoc.X = LAPTOP_SCREEN_UL_X + (LAPTOP_SCREEN_WIDTH - gHelpScreen.usScreenSize.Width) / 2;
                gHelpScreen.usScreenLoc.Y = LAPTOP_SCREEN_UL_Y + (LAPTOP_SCREEN_HEIGHT - gHelpScreen.usScreenSize.Height) / 2;

                break;
            case HELP_SCREEN.MAPSCREEN:
                gHelpScreen.bNumberOfButtons = HLP_SCRN_NUM_MPSCRN_BTNS;

                //calc the center position based on the current panel thats being displayed
                gHelpScreen.usScreenLoc.Y = (gsVIEWPORT_END_Y - gHelpScreen.usScreenSize.Height) / 2;
                break;
            case HELP_SCREEN.TACTICAL:
                gHelpScreen.bNumberOfButtons = HLP_SCRN_NUM_TACTICAL_PAGES;

                //calc the center position based on the current panel thats being displayed
                gHelpScreen.usScreenLoc.Y = (gsVIEWPORT_END_Y - gHelpScreen.usScreenSize.Height) / 2;
                break;

            case HELP_SCREEN.MAPSCREEN_NO_ONE_HIRED:
            case HELP_SCREEN.MAPSCREEN_NOT_IN_ARULCO:
            case HELP_SCREEN.MAPSCREEN_SECTOR_INVENTORY:
                gHelpScreen.usScreenSize.Width = HELP_SCREEN_SMALL_LOC_WIDTH;
                gHelpScreen.usScreenSize.Height = HELP_SCREEN_SMALL_LOC_HEIGHT;

                //calc screen position since we just set the width and height
                gHelpScreen.usScreenLoc.X = (640 - gHelpScreen.usScreenSize.Width) / 2;

                //calc the center position based on the current panel thats being displayed
                gHelpScreen.usScreenLoc.Y = (gsVIEWPORT_END_Y - gHelpScreen.usScreenSize.Height) / 2;

                gHelpScreen.bNumberOfButtons = 0;
                gHelpScreen.bCurrentHelpScreenActiveSubPage = 0;
                break;


            case HELP_SCREEN_OPTIONS:
            case HELP_SCREEN_LOAD_GAME:
                break;

            default:
                break;
        }

        //if there are buttons
        if (gHelpScreen.bNumberOfButtons != 0)
        {
            gHelpScreen.usLeftMarginPosX = gHelpScreen.usScreenLoc.X + HELP_SCREEN_TEXT_LEFT_MARGIN_WITH_BTN;
        }
        else
        {
            gHelpScreen.usLeftMarginPosX = gHelpScreen.usScreenLoc.X + HELP_SCREEN_TEXT_LEFT_MARGIN;
        }
    }

    private static HELP_SCREEN HelpScreenDetermineWhichMapScreenHelpToShow()
    {
        if (fShowMapInventoryPool)
        {
            return (HELP_SCREEN.MAPSCREEN_SECTOR_INVENTORY);
        }

        if (GameInit.AnyMercsHired() == false)
        {
            return (HELP_SCREEN.MAPSCREEN_NO_ONE_HIRED);
        }

        if (gTacticalStatus.fDidGameJustStart)
        {
            return (HELP_SCREEN.MAPSCREEN_NOT_IN_ARULCO);
        }

        return (HELP_SCREEN.MAPSCREEN);
    }

    public ValueTask Activate()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask Deactivate()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
    }

    public void Draw(IVideoManager videoManager)
    {
        throw new NotImplementedException();
    }

    public ValueTask<ScreenName> Handle()
    {
        return ValueTask.FromResult(ScreenName.HelpScreen);
    }

    public ValueTask<bool> Initialize()
    {
        return ValueTask.FromResult(true);
    }
}

//enum used for the different help screens that can come up
public enum HELP_SCREEN
{
    LAPTOP,
    MAPSCREEN,
    MAPSCREEN_NO_ONE_HIRED,
    MAPSCREEN_NOT_IN_ARULCO,
    MAPSCREEN_SECTOR_INVENTORY,
    TACTICAL,
    OPTIONS,
    LOAD_GAME,

    NUMBER_OF_HELP_SCREENS,

    UNSET = -1,
};

public enum HELP_SCREEN_ACTIVE
{
    No = 0,
    Yes = 1,
}


public class HELP_SCREEN_STRUCT
{
    public HELP_SCREEN bCurrentHelpScreen;
    public HELP_SCREEN_ACTIVE uiFlags;
    public ushort usHasPlayerSeenHelpScreenInCurrentScreen;
    public HLP_SCRN_DRTY_LVL ubHelpScreenDirty;
    public Point usScreenLoc;
    public Size usScreenSize;
    public int iLastMouseClickY;         //last position the mouse was clicked ( if != -1 )
    public int bCurrentHelpScreenActiveSubPage;  //used to keep track of the current page being displayed
    public int bNumberOfButtons;

    //used so if the user checked the box to show the help, it doesnt automatically come up every frame
    public bool fHaveAlreadyBeenInHelpScreenSinceEnteringCurrenScreen;
    public int bDelayEnteringHelpScreenBy1FrameCount;
    public int usLeftMarginPosX;
    public CURSOR usCursor;
    public bool fWasTheGamePausedPriorToEnteringHelpScreen;

    //scroll variables
    public int usTotalNumberOfPixelsInBuffer;
    public int iLineAtTopOfTextBuffer;
    public int usTotalNumberOfLinesInBuffer;
    public bool fForceHelpScreenToComeUp;
}

//enums for the different dirty levels
public enum HLP_SCRN_DRTY_LVL
{
    NOT_DIRTY,
    REFRESH_TEXT,
    REFRESH_ALL,
};
