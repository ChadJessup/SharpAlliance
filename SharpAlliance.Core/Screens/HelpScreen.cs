using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.SubSystems.LaptopSubSystem;
using SharpAlliance.Platform;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core.Screens;

public class HelpScreen : IScreen
{
    private const string HELPSCREEN_FILE = "BINARYDATA\\Help.edt";
    private const int HELPSCREEN_RECORD_SIZE = 80 * 8 * 2;

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
    private const FontColor HELP_SCREEN_TEXT_BACKGROUND = 0;//NO_SHADOW//FONT_MCOLOR_WHITE;
    private const int HELP_SCREEN_TITLE_OFFSET_Y = 7;
    private const int HELP_SCREEN_HELP_REMINDER_Y = HELP_SCREEN_TITLE_OFFSET_Y + 15;
    public const int HELP_SCREEN_NUM_BTNS = 8;
    private const int HELP_SCREEN_SHOW_HELP_AGAIN_REGION_OFFSET_X = 4;
    private const int HELP_SCREEN_SHOW_HELP_AGAIN_REGION_OFFSET_Y = 18;
    private const int HELP_SCREEN_SHOW_HELP_AGAIN_REGION_TEXT_OFFSET_X = 25 + HELP_SCREEN_SHOW_HELP_AGAIN_REGION_OFFSET_X;
    private const int HELP_SCREEN_SHOW_HELP_AGAIN_REGION_TEXT_OFFSET_Y = (HELP_SCREEN_SHOW_HELP_AGAIN_REGION_OFFSET_Y);
    private const int HELP_SCREEN_EXIT_BTN_OFFSET_X = 291;
    private const int HELP_SCREEN_EXIT_BTN_LOC_Y = 9;

    //the type of help screen
    private const int HLP_SCRN_DEFAULT_TYPE = 9;
    private const int HLP_SCRN_BUTTON_BORDER = 8;

    public static HELP_SCREEN_STRUCT gHelpScreen = new();

    private static bool gfHelpScreenEntry;
    private static bool gfHelpScreenExit;


    public void InitHelpScreenSystem()
    {
        //set some values
        gHelpScreen = new();

        //set it up so we can enter the screen
        gfHelpScreenEntry = true;
        gfHelpScreenExit = false;

        gHelpScreen.bCurrentHelpScreenActiveSubPage = HLP_SCRN_LPTP.UNSET;

        gHelpScreen.fHaveAlreadyBeenInHelpScreenSinceEnteringCurrenScreen = false;
    }

    //An array of record nums for the text on the help buttons
    private static Dictionary<HELP_SCREEN, List<HLP_TXT>> gHelpScreenBtnTextRecordNum = new()
    {
        //new screen:

        //Laptop button record nums
        {
            HELP_SCREEN.LAPTOP, new()
            {
                 HLP_TXT.LAPTOP_BUTTON_1,
                 HLP_TXT.LAPTOP_BUTTON_2,
                 HLP_TXT.LAPTOP_BUTTON_3,
                 HLP_TXT.LAPTOP_BUTTON_4,
                 HLP_TXT.LAPTOP_BUTTON_5,
                 HLP_TXT.LAPTOP_BUTTON_6,
                 HLP_TXT.LAPTOP_BUTTON_7,
                 HLP_TXT.LAPTOP_BUTTON_8,
            }
        },
        {
            HELP_SCREEN.MAPSCREEN, new()
            {
                 HLP_TXT.WELCOM_TO_ARULCO_BUTTON_1,
                 HLP_TXT.WELCOM_TO_ARULCO_BUTTON_2,
                 HLP_TXT.WELCOM_TO_ARULCO_BUTTON_3,
                 HLP_TXT.WELCOM_TO_ARULCO_BUTTON_4,
                 HLP_TXT.WELCOM_TO_ARULCO_BUTTON_5,
                 HLP_TXT.WELCOM_TO_ARULCO_BUTTON_6,
                 HLP_TXT.WELCOM_TO_ARULCO_BUTTON_7,
                 HLP_TXT.WELCOM_TO_ARULCO_BUTTON_8,
            }
        },
        {
            HELP_SCREEN.MAPSCREEN_NO_ONE_HIRED, new()
            { HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, }
        },
        {
            HELP_SCREEN.MAPSCREEN_NOT_IN_ARULCO, new()
            { HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, }
        },
        {
            HELP_SCREEN.MAPSCREEN_SECTOR_INVENTORY, new()
            { HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, }
        },
        {
            HELP_SCREEN.TACTICAL, new()
             {
                HLP_TXT.TACTICAL_BUTTON_1,
                HLP_TXT.TACTICAL_BUTTON_2,
                HLP_TXT.TACTICAL_BUTTON_3,
                HLP_TXT.TACTICAL_BUTTON_4,
                HLP_TXT.TACTICAL_BUTTON_5,
                HLP_TXT.TACTICAL_BUTTON_6,
                HLP_TXT.UNSET, HLP_TXT.UNSET,
            }
        },
        {
            HELP_SCREEN.OPTIONS, new()
            { HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, }
        },
        {
            HELP_SCREEN.LOAD_GAME, new()
            { HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, HLP_TXT.UNSET, }
        },
    };


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
    private static int HLP_SCRN__SCROLL_POSY => (gHelpScreen.usScreenLoc.Y + 63);
    private const int HLP_SCRN__SCROLL_UP_ARROW_X = 292;
    private const int HLP_SCRN__SCROLL_UP_ARROW_Y = 43;
    private const int HLP_SCRN__SCROLL_DWN_ARROW_X = HLP_SCRN__SCROLL_UP_ARROW_X;
    private const int HLP_SCRN__SCROLL_DWN_ARROW_Y = HLP_SCRN__SCROLL_UP_ARROW_Y + 202;

    private static IFileManager files;
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
    private static ButtonPic[] guiHelpScreenScrollArrowImage = new ButtonPic[2];

    private static HVOBJECT guiHelpScreenBackGround;
    private static SurfaceType guiHelpScreenTextBufferSurface;
    private static int gubRenderHelpScreenTwiceInaRow;
    private static bool gfScrollBoxIsScrolling;
    private static bool gfHaveRenderedFirstFrameToSaveBuffer;
    private static MOUSE_REGION gHelpScreenScrollArea = new(nameof(gHelpScreenScrollArea));

    public bool IsInitialized { get; set; }
    public ScreenState State { get; set; }

    public HelpScreen(
        ILogger<HelpScreen> logger,
        IVideoManager videoManager,
        ILibraryManager libraryManager,
        IFileManager fileManager,
        IInputManager inputManager)
    {
        files = fileManager;
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
        if (!gHelpScreen.usHasPlayerSeenHelpScreenInCurrentScreen)
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

            ButtonSubSystem.UnmarkButtonsDirty(buttonList);

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
        if (HelpScreen.gfHelpScreenEntry)
        {
            //setup the help screen
            EnterHelpScreen();

            HelpScreen.gfHelpScreenEntry = false;
            HelpScreen.gfHelpScreenExit = false;
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
        ButtonSubSystem.RenderButtons(buttonList);

        video.SaveBackgroundRects();
        GuiManager.RenderButtonsFastHelp();

        video.ExecuteBaseDirtyRectQueue();
        video.EndFrameBufferRender();

        //if we are leaving the help screen
        if (HelpScreen.gfHelpScreenExit)
        {
            HelpScreen.gfHelpScreenExit = false;

            HelpScreen.gfHelpScreenEntry = true;

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
                gHelpScreen.usHasPlayerSeenHelpScreenInCurrentScreen = false;
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

    private static void DeleteScrollArrowButtons()
    {
        //remove the mouse region that blankets
        MouseSubSystem.MSYS_RemoveRegion(gHelpScreenScrollArea);

        for (int i = 0; i < 2; i++)
        {
            ButtonSubSystem.RemoveButton(giHelpScreenScrollArrows[i]);
            ButtonSubSystem.UnloadButtonImage(guiHelpScreenScrollArrowImage[i]);
        }
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
                new Rectangle(gHelpScreen.usScreenLoc, gHelpScreen.usScreenSize));

            ButtonSubSystem.UnmarkButtonsDirty(buttonList);
        }


        //render the text buffer to the screen
        if (gHelpScreen.ubHelpScreenDirty >= HLP_SCRN_DRTY_LVL.REFRESH_TEXT)
        {
            RenderTextBufferToScreen();
        }
    }

    private static void DisplayCurrentScreenTitleAndFooter()
    {
        int iStartLoc = -1;
        string zText;
        int usPosX = 0, usPosY = 0, usWidth = 0;

        //new screen:

        //switch on the current screen
        switch (gHelpScreen.bCurrentHelpScreen)
        {
            case HELP_SCREEN.LAPTOP:
                iStartLoc = HELPSCREEN_RECORD_SIZE * (int)HLP_TXT.LAPTOP_TITLE;
                break;
            case HELP_SCREEN.MAPSCREEN:
                iStartLoc = HELPSCREEN_RECORD_SIZE * (int)HLP_TXT.WELCOM_TO_ARULCO_TITLE;
                break;
            case HELP_SCREEN.TACTICAL:
                iStartLoc = HELPSCREEN_RECORD_SIZE * (int)HLP_TXT.TACTICAL_TITLE;
                break;
            case HELP_SCREEN.MAPSCREEN_NO_ONE_HIRED:
                iStartLoc = HELPSCREEN_RECORD_SIZE * (int)HLP_TXT.MPSCRN_NO_1_HIRED_YET_TITLE;
                break;
            case HELP_SCREEN.MAPSCREEN_NOT_IN_ARULCO:
                iStartLoc = HELPSCREEN_RECORD_SIZE * (int)HLP_TXT.MPSCRN_NOT_IN_ARULCO_TITLE;
                break;
            case HELP_SCREEN.MAPSCREEN_SECTOR_INVENTORY:
                iStartLoc = HELPSCREEN_RECORD_SIZE * (int)HLP_TXT.SECTOR_INVTRY_TITLE;
                break;
            case HELP_SCREEN.OPTIONS:
                break;
            case HELP_SCREEN.LOAD_GAME:
                break;

            default:
                break;
        }

        //	GetHelpScreenTextPositions( NULL, NULL, &usWidth );

        if (gHelpScreen.bNumberOfButtons != 0)
        {
            usWidth = gHelpScreen.usScreenSize.Width - HELP_SCREEN_TEXT_LEFT_MARGIN_WITH_BTN - HELP_SCREEN_TEXT_RIGHT_MARGIN_SPACE;
        }
        else
        {
            usWidth = gHelpScreen.usScreenSize.Width - HELP_SCREEN_TEXT_LEFT_MARGIN - HELP_SCREEN_TEXT_RIGHT_MARGIN_SPACE;
        }

        //if this screen has a valid title
        if (iStartLoc != -1)
        {
            files.LoadEncryptedDataFromFile(HELPSCREEN_FILE, out zText, iStartLoc, HELPSCREEN_RECORD_SIZE);

            FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);

            usPosX = gHelpScreen.usLeftMarginPosX;

            //		DrawTextToScreen( zText, usPosX, (UINT16)(gHelpScreen.usScreenLocY+HELP_SCREEN_TITLE_OFFSET_Y), usWidth, 
            //									 HELP_SCREEN_TITLE_BODY_FONT, HELP_SCREEN_TITLE_BODY_COLOR, HELP_SCREEN_TEXT_BACKGROUND, FALSE, CENTER_JUSTIFIED );

            //Display the Title
            //            IanDisplayWrappedString(usPosX, (gHelpScreen.usScreenLoc.Y + HELP_SCREEN_TITLE_OFFSET_Y), usWidth, HELP_SCREEN_GAP_BTN_LINES,
            //                                                             HELP_SCREEN_TITLE_BODY_FONT, HELP_SCREEN_TITLE_BODY_COLOR, zText,
            //                                                             HELP_SCREEN_TEXT_BACKGROUND, false, 0);

        }

        //Display the '( press H to get help... )'
        iStartLoc = HELPSCREEN_RECORD_SIZE * (int)HLP_TXT.CONSTANT_SUBTITLE;
        files.LoadEncryptedDataFromFile(HELPSCREEN_FILE, out zText, iStartLoc, HELPSCREEN_RECORD_SIZE);

        usPosX = gHelpScreen.usLeftMarginPosX;

        usPosY = gHelpScreen.usScreenLoc.Y + HELP_SCREEN_HELP_REMINDER_Y;
        //	DrawTextToScreen( zText, usPosX, usPosY, usWidth, 
        //								 HELP_SCREEN_TEXT_BODY_FONT, HELP_SCREEN_TITLE_BODY_COLOR, HELP_SCREEN_TEXT_BACKGROUND, FALSE, CENTER_JUSTIFIED );

        WordWrap.IanDisplayWrappedString(
            new(usPosX, usPosY),
            usWidth,
            HELP_SCREEN_GAP_BTN_LINES,
            HELP_SCREEN_TITLE_BODY_FONT,
            HELP_SCREEN_TITLE_BODY_COLOR,
            zText,
            HELP_SCREEN_TEXT_BACKGROUND,
            false,
            0);


        if (!gHelpScreen.fForceHelpScreenToComeUp)
        {
            //calc location for the ' [ x ] Dont display again...'
            iStartLoc = HELPSCREEN_RECORD_SIZE * (int)HLP_TXT.CONSTANT_FOOTER;
            files.LoadEncryptedDataFromFile(HELPSCREEN_FILE, out zText, iStartLoc, HELPSCREEN_RECORD_SIZE);

            usPosX = gHelpScreen.usLeftMarginPosX + HELP_SCREEN_SHOW_HELP_AGAIN_REGION_TEXT_OFFSET_X;

            usPosY = gHelpScreen.usScreenLoc.Y + gHelpScreen.usScreenSize.Height - HELP_SCREEN_SHOW_HELP_AGAIN_REGION_TEXT_OFFSET_Y + 2;


            //Display the ' [ x ] Dont display again...'
            //            IanDisplayWrappedString(usPosX, usPosY, usWidth, HELP_SCREEN_GAP_BTN_LINES,
            //                                                             HELP_SCREEN_TEXT_BODY_FONT, HELP_SCREEN_TITLE_BODY_COLOR, zText,
            //                                                             HELP_SCREEN_TEXT_BACKGROUND, false, 0);
        }

        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
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
        Rectangle SrcRect;

        //var renderBuffer = video.Surfaces[SurfaceType.RENDER_BUFFER];
        //var textBuffer = video.Surfaces[guiHelpScreenTextBufferSurface];

        SrcRect = new()
        {
            X = 0,
            Y = gHelpScreen.iLineAtTopOfTextBuffer * HLP_SCRN__HEIGHT_OF_1_LINE_IN_BUFFER,
            Width = HLP_SCRN__WIDTH_OF_TEXT_BUFFER,
            Height = 0 + HLP_SCRN__HEIGHT_OF_TEXT_AREA - (2 * 8),
        };

        Point dstPoint = new(
            gHelpScreen.usLeftMarginPosX,
            gHelpScreen.usScreenLoc.Y + HELP_SCREEN_TEXT_OFFSET_Y);

        video.BlitBufferToBuffer(
            guiHelpScreenTextBufferSurface,
            SurfaceType.FRAME_BUFFER,
            SrcRect,
            dstPoint);

        //        video.Surfaces[SurfaceType.RENDER_BUFFER].SaveAsPng($@"C:\temp\text.png");

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
            pDestBuf = video.Surfaces[SurfaceType.FRAME_BUFFER];
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
            iTopPosScrollBox = (gHelpScreen.usScreenLoc.Y + 63);//HLP_SCRN__SCROLL_POSY;
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
                if (keyEvent.Repeat)
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

    private static void ChangeToHelpScreenSubPage(HLP_SCRN_LPTP bNewPage)
    {
        //if for some reason, we are assigning a lower number
        if (bNewPage < 0)
        {
            gHelpScreen.bCurrentHelpScreenActiveSubPage = 0;
        }

        //for some reason if the we are passing in a # that is greater then the max, set it to the max
        else if ((int)bNewPage >= gHelpScreen.bNumberOfButtons)
        {
            gHelpScreen.bCurrentHelpScreenActiveSubPage = (gHelpScreen.bNumberOfButtons == 0)
                ? (HLP_SCRN_LPTP)0
                : (HLP_SCRN_LPTP)gHelpScreen.bNumberOfButtons - 1;
        }

        //if we are selecting the current su page, exit
        else if (bNewPage == gHelpScreen.bCurrentHelpScreenActiveSubPage)
        {
            return;
        }

        //else assign the new subpage
        else
        {
            gHelpScreen.bCurrentHelpScreenActiveSubPage = bNewPage;
        }

        //refresh the screen
        gHelpScreen.ubHelpScreenDirty = HLP_SCRN_DRTY_LVL.REFRESH_TEXT;

        //'undepress' all the buttons
        for (int i = 0; i < gHelpScreen.bNumberOfButtons; i++)
        {
            guiHelpScreenBtns[i].uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);
        }

        //depress the proper button
        guiHelpScreenBtns[(int)gHelpScreen.bCurrentHelpScreenActiveSubPage].uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;

        //change the current sub page, and render it to the buffer
        ChangeHelpScreenSubPage();
    }

    private static void PrepareToExitHelpScreen()
    {
        gfHelpScreenExit = true;
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
            HELP_SCREEN_BTN_FONT_ON_COLOR, FontShadow.DEFAULT_SHADOW,
            HELP_SCREEN_BTN_FONT_OFF_COLOR, FontShadow.DEFAULT_SHADOW,
            ButtonTextJustifies.TEXT_CJUSTIFIED,
            new Point(usPosX, usPosY),
            ButtonFlags.BUTTON_TOGGLE,
            MSYS_PRIORITY.HIGHEST,
            MouseSubSystem.DefaultMoveCallback,
            BtnHelpScreenExitCallback);
        ButtonSubSystem.SetButtonFastHelpText(guiHelpScreenExitBtn, gzHelpScreenText[0]);
        ButtonSubSystem.SetButtonCursor(guiHelpScreenExitBtn, gHelpScreen.usCursor);

        buttonList.Add(guiHelpScreenExitBtn);

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
                (usPosY - 3),
                "INTERFACE\\OptionsCheckBoxes.sti",
                MSYS_PRIORITY.HIGHEST,
                BtnHelpScreenDontShowHelpAgainCallback);

            ButtonSubSystem.SetButtonCursor(gHelpScreenDontShowHelpAgainToggle, gHelpScreen.usCursor);

            buttonList.Add(gHelpScreenDontShowHelpAgainToggle);

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
            iStartLoc = HELPSCREEN_RECORD_SIZE * CONSTANT_FOOTER;
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
        gHelpScreen.usHasPlayerSeenHelpScreenInCurrentScreen = true;

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

    private static void CreateScrollAreaButtons()
    {
        int usPosX, usWidth, usPosY;
        int iPosY, iHeight;

        if (gHelpScreen.bNumberOfButtons != 0)
        {
            usPosX = gHelpScreen.usScreenLoc.X + HLP_SCRN__SCROLL_POSX + HELP_SCREEN_BUTTON_BORDER_WIDTH;
        }
        else
        {
            usPosX = gHelpScreen.usScreenLoc.X + HLP_SCRN__SCROLL_POSX;
        }

        usWidth = HLP_SCRN__WIDTH_OF_SCROLL_AREA;

        //Get the height and position of the scroll box
        CalculateHeightAndPositionForHelpScreenScrollBox(out iHeight, out iPosY);

        //Create a mouse region 'mask' the entrire screen
        MouseSubSystem.MSYS_DefineRegion(
            gHelpScreenScrollArea,
            new Rectangle(usPosX, iPosY, usPosX + usWidth, iPosY + HLP_SCRN__HEIGHT_OF_SCROLL_AREA),
            MSYS_PRIORITY.HIGHEST,
            gHelpScreen.usCursor,
            SelectHelpScrollAreaMovementCallBack,
            SelectHelpScrollAreaCallBack);

        MouseSubSystem.MSYS_AddRegion(ref gHelpScreenScrollArea);

        guiHelpScreenScrollArrowImage[0] = ButtonSubSystem.LoadButtonImage("INTERFACE\\HelpScreen.sti", 14, 10, 11, 12, 13);
        guiHelpScreenScrollArrowImage[1] = ButtonSubSystem.UseLoadedButtonImage(guiHelpScreenScrollArrowImage[0], 19, 15, 16, 17, 18);

        if (gHelpScreen.bNumberOfButtons != 0)
        {
            usPosX = gHelpScreen.usScreenLoc.X + HLP_SCRN__SCROLL_UP_ARROW_X + HELP_SCREEN_BUTTON_BORDER_WIDTH;
        }
        else
        {
            usPosX = gHelpScreen.usScreenLoc.X + HLP_SCRN__SCROLL_UP_ARROW_X;
        }

        usPosY = gHelpScreen.usScreenLoc.Y + HLP_SCRN__SCROLL_UP_ARROW_Y;

        //Create the scroll arrows
        giHelpScreenScrollArrows[0] = ButtonSubSystem.QuickCreateButton(
            guiHelpScreenScrollArrowImage[0],
            new Point(usPosX, usPosY),
            ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
            MouseSubSystem.DefaultMoveCallback,
            BtnHelpScreenScrollArrowsCallback);

        ButtonSubSystem.MSYS_SetBtnUserData(giHelpScreenScrollArrows[0], 0, 0);
        ButtonSubSystem.SetButtonCursor(giHelpScreenScrollArrows[0], gHelpScreen.usCursor);

        usPosY = gHelpScreen.usScreenLoc.Y + HLP_SCRN__SCROLL_DWN_ARROW_Y;

        //Create the scroll arrows
        giHelpScreenScrollArrows[1] = ButtonSubSystem.QuickCreateButton(
            guiHelpScreenScrollArrowImage[1],
            new Point(usPosX, usPosY),
            ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
            MouseSubSystem.DefaultMoveCallback,
            BtnHelpScreenScrollArrowsCallback);

        ButtonSubSystem.MSYS_SetBtnUserData(giHelpScreenScrollArrows[1], 0, 1);
        ButtonSubSystem.SetButtonCursor(giHelpScreenScrollArrows[1], gHelpScreen.usCursor);

        buttonList.AddRange(giHelpScreenScrollArrows);
    }

    private static void SelectHelpScrollAreaCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
        {
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            gfScrollBoxIsScrolling = false;
            gHelpScreen.iLastMouseClickY = -1;
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            gfScrollBoxIsScrolling = true;
            HelpScreenMouseMoveScrollBox(pRegion.MousePos.Y);
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
        }
    }

    private static void BtnHelpScreenScrollArrowsCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);
            video.InvalidateRegion(btn.MouseRegion.Bounds);
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            int iButtonID = (int)ButtonSubSystem.MSYS_GetBtnUserData(btn, 0);

            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;

            //if up
            if (iButtonID == 0)
            {
                ChangeTopLineInTextBufferByAmount(-1);
            }
            else
            {
                ChangeTopLineInTextBufferByAmount(1);
            }

            video.InvalidateRegion(btn.MouseRegion.Bounds);
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_REPEAT))
        {
            int iButtonID = (int)ButtonSubSystem.MSYS_GetBtnUserData(btn, 0);

            //if up
            if (iButtonID == 0)
            {
                ChangeTopLineInTextBufferByAmount(-1);
            }
            else
            {
                ChangeTopLineInTextBufferByAmount(1);
            }

            video.InvalidateRegion(btn.MouseRegion.Bounds);
        }
    }

    private static void SelectHelpScrollAreaMovementCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
        {
            //		InvalidateRegion(pRegion.RegionTopLeftX, pRegion.RegionTopLeftY, pRegion.RegionBottomRightX, pRegion.RegionBottomRightY);
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.GAIN_MOUSE))
        {
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.MOVE))
        {
            if (inputs.gfLeftButtonState)
            {
                HelpScreenMouseMoveScrollBox(pRegion.MousePos.Y);
            }
        }
    }

    private static void ChangeHelpScreenSubPage()
    {
        //reset
        gHelpScreen.iLineAtTopOfTextBuffer = 0;

        RenderCurrentHelpScreenTextToBuffer();

        //enable or disable the help screen arrow buttons
        if (gHelpScreen.usTotalNumberOfLinesInBuffer <= HLP_SCRN__MAX_NUMBER_DISPLAYED_LINES_IN_BUFFER)
        {
            ButtonSubSystem.DisableButton(giHelpScreenScrollArrows[0]);
            ButtonSubSystem.DisableButton(giHelpScreenScrollArrows[1]);
        }
        else
        {
            ButtonSubSystem.EnableButton(giHelpScreenScrollArrows[0]);
            ButtonSubSystem.EnableButton(giHelpScreenScrollArrows[1]);
        }
    }

    private static void RenderCurrentHelpScreenTextToBuffer()
    {
        //clear the buffer ( use 0, black as a transparent color
        ClearHelpScreenTextBuffer();

        //Render the current screen, and get the number of pixels it used to display
        gHelpScreen.usTotalNumberOfPixelsInBuffer = RenderSpecificHelpScreen();

        //calc the number of lines in the buffer
        gHelpScreen.usTotalNumberOfLinesInBuffer = gHelpScreen.usTotalNumberOfPixelsInBuffer / (HLP_SCRN__HEIGHT_OF_1_LINE_IN_BUFFER);
    }

    private static int RenderSpecificHelpScreen()
    {
        int usNumVerticalPixelsDisplayed = 0;
        //new screen:

        //set the buffer for the text to go to
        //	SetFontDestBuffer( guiHelpScreenTextBufferSurface, gHelpScreen.usLeftMarginPosX, gHelpScreen.usScreenLocY + HELP_SCREEN_TEXT_OFFSET_Y,
        //										 HLP_SCRN__WIDTH_OF_TEXT_BUFFER, HLP_SCRN__NUMBER_BYTES_IN_TEXT_BUFFER, FALSE );
        FontSubSystem.SetFontDestBuffer(
            guiHelpScreenTextBufferSurface, 0, 0,
            HLP_SCRN__WIDTH_OF_TEXT_BUFFER, HLP_SCRN__HEIGHT_OF_TEXT_BUFFER, false);

        //switch on the current screen
        switch (gHelpScreen.bCurrentHelpScreen)
        {
            case HELP_SCREEN.LAPTOP:
                usNumVerticalPixelsDisplayed = RenderLaptopHelpScreen();
                break;
            case HELP_SCREEN.MAPSCREEN:
                usNumVerticalPixelsDisplayed = RenderMapScreenHelpScreen();
                break;
            case HELP_SCREEN.TACTICAL:
                usNumVerticalPixelsDisplayed = RenderTacticalHelpScreen();
                break;
            case HELP_SCREEN.MAPSCREEN_NO_ONE_HIRED:
                usNumVerticalPixelsDisplayed = RenderMapScreenNoOneHiredYetHelpScreen();
                break;
            case HELP_SCREEN.MAPSCREEN_NOT_IN_ARULCO:
                usNumVerticalPixelsDisplayed = RenderMapScreenNotYetInArulcoHelpScreen();
                break;
            case HELP_SCREEN.MAPSCREEN_SECTOR_INVENTORY:
                usNumVerticalPixelsDisplayed = RenderMapScreenSectorInventoryHelpScreen();
                break;
            case HELP_SCREEN.OPTIONS:
                break;
            case HELP_SCREEN.LOAD_GAME:
                break;

            default:
                break;
        }

        FontSubSystem.SetFontDestBuffer(SurfaceType.FRAME_BUFFER, 0, 0, 640, 480, false);

        //add 1 line to the bottom of the buffer
        usNumVerticalPixelsDisplayed += 10;

        return (usNumVerticalPixelsDisplayed);
    }

    private static int RenderMapScreenSectorInventoryHelpScreen()
    {
        throw new NotImplementedException();
    }

    private static int RenderMapScreenNotYetInArulcoHelpScreen()
    {
        throw new NotImplementedException();
    }

    private static int RenderMapScreenNoOneHiredYetHelpScreen()
    {
        throw new NotImplementedException();
    }

    private static int RenderTacticalHelpScreen()
    {
        throw new NotImplementedException();
    }

    private static int RenderMapScreenHelpScreen()
    {
        throw new NotImplementedException();
    }

    private static int RenderLaptopHelpScreen()
    {
        int usPosX, usPosY, usWidth, usNumVertPixels = 100;
        int ubCnt;
        int usTotalNumberOfVerticalPixels = 0;
        int usFontHeight = FontSubSystem.GetFontHeight(HELP_SCREEN_TEXT_BODY_FONT);


        if (gHelpScreen.bCurrentHelpScreenActiveSubPage == HLP_SCRN_LPTP.UNSET)
        {
            return (0);
        }

        //Get the position for the text
        GetHelpScreenTextPositions(out usPosX, out usPosY, out usWidth);

        //switch on the current screen
        switch (gHelpScreen.bCurrentHelpScreenActiveSubPage)
        {
            case HLP_SCRN_LPTP.OVERVIEW:
                //Display all the paragraphs
                for (ubCnt = 0; ubCnt < 2; ubCnt++)
                {
                    //Display the text, and get the number of pixels it used to display it
                    usNumVertPixels = GetAndDisplayHelpScreenText(HLP_TXT.LAPTOP_OVERVIEW_P1 + ubCnt, usPosX, usPosY, usWidth);

                    //move the next text down by the right amount
                    usPosY = usPosY + usNumVertPixels + usFontHeight;

                    //add the total amount of pixels used
                    usTotalNumberOfVerticalPixels += usNumVertPixels + usFontHeight;
                }

                /*
                            //Display the first paragraph
                            usTotalNumberOfVerticalPixels = GetAndDisplayHelpScreenText( HLP_TXT.LAPTOP_OVERVIEW_P1, usPosX, usPosY, usWidth );

                            usPosY = usPosY+ usNumVertPixels + GetFontHeight( HELP_SCREEN_TEXT_BODY_FONT );

                            //Display the second paragraph
                            usTotalNumberOfVerticalPixels += GetAndDisplayHelpScreenText( HLP_TXT.LAPTOP_OVERVIEW_P2, usPosX, usPosY, usWidth );
                */
                break;

            case HLP_SCRN_LPTP.EMAIL:

                //Display the first paragraph
                usTotalNumberOfVerticalPixels = GetAndDisplayHelpScreenText(
                    HLP_TXT.LAPTOP_EMAIL_P1, usPosX, usPosY, usWidth);
                break;


            case HLP_SCRN_LPTP.WEB:

                //Display the first paragraph
                usTotalNumberOfVerticalPixels = GetAndDisplayHelpScreenText(HLP_TXT.LAPTOP_WEB_P1, usPosX, usPosY, usWidth);

                break;


            case HLP_SCRN_LPTP.FILES:

                //Display the first paragraph
                usTotalNumberOfVerticalPixels = GetAndDisplayHelpScreenText(HLP_TXT.LAPTOP_FILES_P1, usPosX, usPosY, usWidth);
                break;


            case HLP_SCRN_LPTP.HISTORY:
                //Display the first paragraph
                usTotalNumberOfVerticalPixels = GetAndDisplayHelpScreenText(HLP_TXT.LAPTOP_HISTORY_P1, usPosX, usPosY, usWidth);

                break;


            case HLP_SCRN_LPTP.PERSONNEL:

                //Display the first paragraph
                usTotalNumberOfVerticalPixels = GetAndDisplayHelpScreenText(HLP_TXT.LAPTOP_PERSONNEL_P1, usPosX, usPosY, usWidth);
                break;

            case HLP_SCRN_LPTP.FINANCIAL:
                //Display all the paragraphs
                for (ubCnt = 0; ubCnt < 2; ubCnt++)
                {
                    usNumVertPixels = GetAndDisplayHelpScreenText(HLP_TXT.FINANCES_P1 + ubCnt, usPosX, usPosY, usWidth);

                    //move the next text down by the right amount
                    usPosY = usPosY + usNumVertPixels + usFontHeight;

                    //add the total amount of pixels used
                    usTotalNumberOfVerticalPixels += usNumVertPixels + usFontHeight;
                }

                break;

            case HLP_SCRN_LPTP.MERC_STATS:
                //Display all the paragraphs
                for (ubCnt = 0; ubCnt < 15; ubCnt++)
                {
                    usNumVertPixels = GetAndDisplayHelpScreenText(HLP_TXT.MERC_STATS_P1 + ubCnt, usPosX, usPosY, usWidth);

                    //move the next text down by the right amount
                    usPosY = usPosY + usNumVertPixels + usFontHeight;

                    //add the total amount of pixels used
                    usTotalNumberOfVerticalPixels += usNumVertPixels + usFontHeight;
                }

                break;
        }

        return (usTotalNumberOfVerticalPixels);
    }

    private static int GetAndDisplayHelpScreenText(
        HLP_TXT record,
        int usPosX,
        int usPosY,
        int usWidth)
    {
        int uiRecord = (int)record;
        string zText;
        int usNumVertPixels = 0;
        int uiStartLoc;

        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);

        zText = GetHelpScreenText(uiRecord);

        //Get the record
        uiStartLoc = HELPSCREEN_RECORD_SIZE * uiRecord;
        files.LoadEncryptedDataFromFile(HELPSCREEN_FILE, out zText, uiStartLoc, HELPSCREEN_RECORD_SIZE);

        //Display the text
        usNumVertPixels = WordWrap.IanDisplayWrappedString(
            new Point(usPosX, usPosY),
            usWidth,
            ubGap: HELP_SCREEN_GAP_BTN_LINES,
            uiFont: HELP_SCREEN_TEXT_BODY_FONT,
            ubColor: HELP_SCREEN_TEXT_BODY_COLOR,
            pString: zText,
            ubBackGroundColor: HELP_SCREEN_TEXT_BACKGROUND,
            fDirty: false,
            uiFlags: 0);

        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

        return (usNumVertPixels);
    }

    private static void GetHelpScreenTextPositions(out int pusPosX, out int pusPosY, out int pusWidth)
    {
        //if there are buttons
        pusPosX = 0;
        pusWidth = HLP_SCRN__WIDTH_OF_TEXT_BUFFER - 1 * HELP_SCREEN_MARGIN_SIZE;       //DEF was 2
        pusPosY = 0;
    }

    private static void ClearHelpScreenTextBuffer()
    {
        // CLEAR THE FRAME BUFFER
        Image<Rgba32> pDestBuf = video.Surfaces[guiHelpScreenTextBufferSurface];

        pDestBuf = new(pDestBuf.Width, pDestBuf.Height);

        video.InvalidateScreen();
    }

    private static void BtnHelpScreenDontShowHelpAgainCallback(ref GUI_BUTTON button, MSYS_CALLBACK_REASON reason)
    {
        //	UINT8	ubButton = (UINT8)MSYS_GetBtnUserData( btn, 0 );

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            /*
                    btn.uiFlags &= ~BUTTON_CLICKED_ON;

                    if( gHelpScreen.usHasPlayerSeenHelpScreenInCurrentScreen & ( 1 << gHelpScreen.bCurrentHelpScreen ) )
                    {
            //
                        gHelpScreen.usHasPlayerSeenHelpScreenInCurrentScreen &= ~( 1 << gHelpScreen.bCurrentHelpScreen );
                    }
                    else
                    {
            //			gHelpScreen.usHasPlayerSeenHelpScreenInCurrentScreen |= ( 1 << gHelpScreen.bCurrentHelpScreen );

                    }
            //		btn.uiFlags |= BUTTON_CLICKED_ON;
            */
        }
    }

    private static bool CreateHelpScreenTextBuffer()
    {
        // Create a background video surface to blt the face onto
        Image<Rgba32> textBuffer = new(HLP_SCRN__WIDTH_OF_TEXT_BUFFER, HLP_SCRN__HEIGHT_OF_TEXT_BUFFER);

        var tex = video.CreateSurface(textBuffer);

        guiHelpScreenTextBufferSurface = tex.SurfaceType;

        return true;
    }

    private static void BtnHelpScreenExitCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
            video.InvalidateRegion(btn.MouseRegion.Bounds);
        }
        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            video.InvalidateRegion(btn.MouseRegion.Bounds);

            PrepareToExitHelpScreen();

            btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);
        }
        if (reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
        {
            btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);
            video.InvalidateRegion(btn.MouseRegion.Bounds);
        }
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
                sText = GetHelpScreenText((int)gHelpScreenBtnTextRecordNum[gHelpScreen.bCurrentHelpScreen][i]);

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

                buttonList.Add(guiHelpScreenBtns[i]);

                ButtonSubSystem.SetButtonCursor(guiHelpScreenBtns[i], gHelpScreen.usCursor);
                ButtonSubSystem.MSYS_SetBtnUserData(guiHelpScreenBtns[i], 0, i);

                //	SpecifyButtonTextOffsets( guiHelpScreenBtns[i], 19, 9, TRUE );

                usPosY += HELP_SCREEN_BTN_HEIGHT + HELP_SCREEN_GAP_BN_BTNS;
            }

            guiHelpScreenBtns[0].uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
        }
    }

    private static string GetHelpScreenText(int uiRecordToGet)
    {
        int iStartLoc = -1;

        iStartLoc = HELPSCREEN_RECORD_SIZE * uiRecordToGet;
        files.LoadEncryptedDataFromFile(HELPSCREEN_FILE, out string pText, iStartLoc, HELPSCREEN_RECORD_SIZE);

        return pText;
    }

    private static void BtnHelpScreenBtnsCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            //		btn.uiFlags |= BUTTON_CLICKED_ON;
            video.InvalidateRegion(btn.MouseRegion.Bounds);
        }
        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            //Get the btn id
            HLP_SCRN_LPTP bRetValue = (HLP_SCRN_LPTP)ButtonSubSystem.MSYS_GetBtnUserData(btn, 0);

            ChangeToHelpScreenSubPage(bRetValue);
            /*
                    //change the current page to the new one
                    gHelpScreen.bCurrentHelpScreenActiveSubPage = ( bRetValue > gHelpScreen.bNumberOfButtons ) ? gHelpScreen.bNumberOfButtons-1 : bRetValue; 

                    gHelpScreen.ubHelpScreenDirty = HLP_SCRN_DRTY_LVL_REFRESH_TEXT;

                    for( i=0; i< gHelpScreen.bNumberOfButtons; i++ )
                    {
                        ButtonList[ guiHelpScreenBtns[i] ].uiFlags &= (~BUTTON_CLICKED_ON );
                    }

                    //change the current sub page, and render it to the buffer
                    ChangeHelpScreenSubPage();
            */
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;

            video.InvalidateRegion(btn.MouseRegion.Bounds);
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
        {
            //		btn.uiFlags &= (~BUTTON_CLICKED_ON );
            video.InvalidateRegion(btn.MouseRegion.Bounds);
        }
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
                gHelpScreen.bNumberOfButtons = (int)HLP_SCRN_LPTP.HLP_SCRN_LPTP_NUM_PAGES;
                gHelpScreen.usCursor = CURSOR.LAPTOP_SCREEN;

                //center the screen inside the laptop screen
                gHelpScreen.usScreenLoc.X = LAPTOP_SCREEN_UL_X + (LAPTOP_SCREEN_WIDTH - gHelpScreen.usScreenSize.Width) / 2;
                gHelpScreen.usScreenLoc.Y = LAPTOP_SCREEN_UL_Y + (LAPTOP_SCREEN_HEIGHT - gHelpScreen.usScreenSize.Height) / 2;

                break;
            case HELP_SCREEN.MAPSCREEN:
                gHelpScreen.bNumberOfButtons = (int)HLP_SCRN_MPSCRN.HLP_SCRN_NUM_MPSCRN_BTNS;

                //calc the center position based on the current panel thats being displayed
                gHelpScreen.usScreenLoc.Y = (gsVIEWPORT_END_Y - gHelpScreen.usScreenSize.Height) / 2;
                break;
            case HELP_SCREEN.TACTICAL:
                gHelpScreen.bNumberOfButtons = (int)HLP_SCRN_TACTICAL.NUM_TACTICAL_PAGES;

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


            case HELP_SCREEN.OPTIONS:
            case HELP_SCREEN.LOAD_GAME:
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
    public bool usHasPlayerSeenHelpScreenInCurrentScreen;
    public HLP_SCRN_DRTY_LVL ubHelpScreenDirty;
    public Point usScreenLoc;
    public Size usScreenSize;
    public int iLastMouseClickY;         //last position the mouse was clicked ( if != -1 )
    public HLP_SCRN_LPTP bCurrentHelpScreenActiveSubPage;  //used to keep track of the current page being displayed
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

//Tactical
public enum HLP_SCRN_TACTICAL
{
    OVERVIEW,
    MOVEMENT,
    SIGHT,
    ATTACKING,
    ITEMS,
    KEYBOARD,

    NUM_TACTICAL_PAGES,
};

//mapscreen, welcome to arulco
public enum HLP_SCRN_MPSCRN
{
    OVERVIEW,
    ASSIGNMENTS,
    DESTINATIONS,
    MAP,
    MILITIA,
    AIRSPACE,
    ITEMS,
    KEYBOARD,

    HLP_SCRN_NUM_MPSCRN_BTNS,

};
//laptop sub pages
public enum HLP_SCRN_LPTP
{
    OVERVIEW,
    EMAIL,
    WEB,
    FILES,
    HISTORY,
    PERSONNEL,
    FINANCIAL,
    MERC_STATS,

    HLP_SCRN_LPTP_NUM_PAGES,

    UNSET = -1,
};

//enum for the help text paragrphs
public enum HLP_TXT
{
    CONSTANT_SUBTITLE,          //0
    CONSTANT_FOOTER,
    LAPTOP_TITLE,
    LAPTOP_BUTTON_1,
    LAPTOP_OVERVIEW_P1,
    LAPTOP_OVERVIEW_P2,
    LAPTOP_BUTTON_2,
    LAPTOP_EMAIL_P1,
    LAPTOP_BUTTON_3,
    LAPTOP_WEB_P1,

    LAPTOP_BUTTON_4,        //10
    LAPTOP_FILES_P1,
    LAPTOP_BUTTON_5,
    LAPTOP_HISTORY_P1,
    LAPTOP_BUTTON_6,
    LAPTOP_PERSONNEL_P1,

    LAPTOP_BUTTON_7,
    FINANCES_P1,
    FINANCES_P2,

    LAPTOP_BUTTON_8,
    MERC_STATS_P1,
    MERC_STATS_P2,
    MERC_STATS_P3,
    MERC_STATS_P4,
    MERC_STATS_P5,
    MERC_STATS_P6,
    MERC_STATS_P7,
    MERC_STATS_P8,
    MERC_STATS_P9,
    MERC_STATS_P10,
    MERC_STATS_P11,
    MERC_STATS_P12,
    MERC_STATS_P13,
    MERC_STATS_P14,
    MERC_STATS_P15,

    //mapscreen no one hired yet
    MPSCRN_NO_1_HIRED_YET_TITLE,
    MPSCRN_NO_1_HIRED_YET_P1,                                   //20
    MPSCRN_NO_1_HIRED_YET_P2,

    //mapscreen not in arulco yet
    MPSCRN_NOT_IN_ARULCO_TITLE,

    MPSCRN_NOT_IN_ARULCO_P1,
    MPSCRN_NOT_IN_ARULCO_P2,
    MPSCRN_NOT_IN_ARULCO_P3,

    WELCOM_TO_ARULCO_TITLE,
    WELCOM_TO_ARULCO_BUTTON_1,
    WELCOM_TO_ARULCO_OVERVIEW_P1,
    WELCOM_TO_ARULCO_OVERVIEW_P2,





    WELCOM_TO_ARULCO_OVERVIEW_P3,       //30
    WELCOM_TO_ARULCO_OVERVIEW_P4,
    WELCOM_TO_ARULCO_OVERVIEW_P5,
    WELCOM_TO_ARULCO_BUTTON_2,
    WELCOM_TO_ARULCO_ASSNMNT_P1,
    WELCOM_TO_ARULCO_ASSNMNT_P2,
    WELCOM_TO_ARULCO_ASSNMNT_P3,
    WELCOM_TO_ARULCO_ASSNMNT_P4,
    WELCOM_TO_ARULCO_ASSNMNT_P5,
    WELCOM_TO_ARULCO_ASSNMNT_P6,


    WELCOM_TO_ARULCO_ASSNMNT_P7,        //40
    WELCOM_TO_ARULCO_ASSNMNT_P8,
    WELCOM_TO_ARULCO_BUTTON_3,
    WELCOM_TO_ARULCO_DSTINATION_P1,
    WELCOM_TO_ARULCO_DSTINATION_P2,
    WELCOM_TO_ARULCO_DSTINATION_P3,
    WELCOM_TO_ARULCO_DSTINATION_P4,
    WELCOM_TO_ARULCO_DSTINATION_P5,
    WELCOM_TO_ARULCO_BUTTON_4,
    WELCOM_TO_ARULCO_MAP_P1,

    WELCOM_TO_ARULCO_MAP_P2,        //50
    WELCOM_TO_ARULCO_MAP_P3,
    WELCOM_TO_ARULCO_MAP_P4,
    WELCOM_TO_ARULCO_MAP_P5,
    WELCOM_TO_ARULCO_MAP_P6,
    WELCOM_TO_ARULCO_MAP_P7,
    WELCOM_TO_ARULCO_BUTTON_5,
    WELCOM_TO_ARULCO_MILITIA_P1,
    WELCOM_TO_ARULCO_MILITIA_P2,


    WELCOM_TO_ARULCO_MILITIA_P3,            //60

    WELCOM_TO_ARULCO_BUTTON_6,
    WELCOM_TO_ARULCO_AIRSPACE_P1,
    WELCOM_TO_ARULCO_AIRSPACE_P2,


    WELCOM_TO_ARULCO_BUTTON_7,
    WELCOM_TO_ARULCO_ITEMS_P1,

    WELCOM_TO_ARULCO_BUTTON_8,
    WELCOM_TO_ARULCO_KEYBOARD_P1,
    WELCOM_TO_ARULCO_KEYBOARD_P2,
    WELCOM_TO_ARULCO_KEYBOARD_P3,
    WELCOM_TO_ARULCO_KEYBOARD_P4,


    TACTICAL_TITLE,
    TACTICAL_BUTTON_1,
    TACTICAL_OVERVIEW_P1,
    TACTICAL_OVERVIEW_P2,
    TACTICAL_OVERVIEW_P3,
    TACTICAL_OVERVIEW_P4,

    TACTICAL_BUTTON_2,
    TACTICAL_MOVEMENT_P1,
    TACTICAL_MOVEMENT_P2,
    TACTICAL_MOVEMENT_P3,
    TACTICAL_MOVEMENT_P4,


    TACTICAL_BUTTON_3,
    TACTICAL_SIGHT_P1,
    TACTICAL_SIGHT_P2,
    TACTICAL_SIGHT_P3,
    TACTICAL_SIGHT_P4,

    TACTICAL_BUTTON_4,
    TACTICAL_ATTACKING_P1,
    TACTICAL_ATTACKING_P2,
    TACTICAL_ATTACKING_P3,

    TACTICAL_BUTTON_5,

    TACTICAL_ITEMS_P1,
    TACTICAL_ITEMS_P2,
    TACTICAL_ITEMS_P3,
    TACTICAL_ITEMS_P4,
    TACTICAL_BUTTON_6,


    TACTICAL_KEYBOARD_P1,
    TACTICAL_KEYBOARD_P2,
    TACTICAL_KEYBOARD_P3,
    TACTICAL_KEYBOARD_P4,
    TACTICAL_KEYBOARD_P5,
    TACTICAL_KEYBOARD_P6,
    TACTICAL_KEYBOARD_P7,
    TACTICAL_KEYBOARD_P8,


    SECTOR_INVTRY_TITLE,
    //	SECTOR_INVTRY_BUTTON_1,
    SECTOR_INVTRY_OVERVIEW_P1,
    SECTOR_INVTRY_OVERVIEW_P2,
    //	,

    UNSET = -1,
};


public class HELP_SCREEN_BTN_TEXT_RECORD
{
    public int[] iButtonTextNum = new int[HelpScreen.HELP_SCREEN_NUM_BTNS];
}
