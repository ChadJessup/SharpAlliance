using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;
using Veldrid;

namespace SharpAlliance.Core.Screens
{
    public class MainMenuScreen : IScreen
    {
        private readonly ButtonSubSystem buttons;
        private readonly GameInit gameInit;
        private readonly RenderDirtySubSystem renderDirty;
        private readonly IScreenManager screens;
        private readonly GameOptions options;
        private readonly IInputManager input;
        private readonly IMusicManager music;
        private readonly MouseSubSystem mouse;
        private readonly IVideoManager video;
        private readonly Globals globals;
        private readonly IVideoSurfaceManager vsurface;
        private readonly CursorSubSystem cursor;
        private readonly IClockManager clock;

        public const string MAINMENU_TEXT_FILE = "LoadScreens\\MainMenu.edt";
        public const int MAINMENU_RECORD_SIZE = 80 * 2;
        public const int MAINMENU_X = ((640 - 214) / 2);
        public const int MAINMENU_TITLE_Y = 75;
        public const int MAINMENU_Y = 277;  //200
        public const int MAINMENU_Y_SPACE = 37;

        // MENU ITEMS
        private enum MainMenuItems
        {
            //	TITLE,
            NEW_GAME,
            LOAD_GAME,
            PREFERENCES,
            CREDITS,
            QUIT,
            NUM_MENU_ITEMS
        };

        private int[] iMenuImages = new int[(int)MainMenuItems.NUM_MENU_ITEMS];
        private int[] iMenuButtons = new int[(int)MainMenuItems.NUM_MENU_ITEMS];

        ushort[] gusMainMenuButtonWidths = new ushort[(int)MainMenuItems.NUM_MENU_ITEMS];

        string guiMainMenuBackGroundImage;
        string guiJa2LogoImage;

        private MouseRegion gBackRegion = new();
        private MainMenuItems gbHandledMainMenu = 0;
        private bool fInitialRender = false;
        //bool						gfDoHelpScreen = 0;

        private bool gfMainMenuScreenEntry = false;
        private bool gfMainMenuScreenExit = false;
        private IntroScreen introScreen;

        ScreenName guiMainMenuExitScreen = ScreenName.MAINMENU_SCREEN;

        public MainMenuScreen(
            MouseSubSystem mouseSubSystem,
            IScreenManager screenManager,
            IVideoManager videoManager,
            GameInit gameInit,
            IClockManager clockManager,
            IMusicManager musicManager,
            GameOptions gameOptions,
            IVideoSurfaceManager videoSurfaceManager,
            ButtonSubSystem buttonSubSystem,
            CursorSubSystem cursorSubSystem,
            IInputManager inputManager,
            Globals globals,
            RenderDirtySubSystem renderDirtySubSystem)
        {
            this.globals = globals;
            this.vsurface = videoSurfaceManager;
            this.cursor = cursorSubSystem;
            this.clock = clockManager;
            this.buttons = buttonSubSystem;
            this.input = inputManager;
            this.music = musicManager;
            this.mouse = mouseSubSystem;
            this.video = videoManager;
            this.options = gameOptions;
            this.screens = screenManager;
            this.gameInit = gameInit;
            this.renderDirty = renderDirtySubSystem;
        }

        public bool IsInitialized { get; set; }
        public ScreenState State { get; set; }
        public bool gfFadeInitialized;
        public bool gfFadeIn;
        public bool gfFadeInVideo;
        public int gbFadeType;
        public Action gFadeFunction;

        public async ValueTask Activate()
        {
            this.introScreen = await this.screens.GetScreen<IntroScreen>(ScreenName.INTRO_SCREEN, activate: false);
        }

        public void Dispose()
        {
        }

        public ValueTask<ScreenName> Handle()
        {
            uint cnt;
            uint uiTime;

            if (this.introScreen.guiSplashStartTime + 4000 > this.clock.GetJA2Clock())
            {
                this.cursor.SetCurrentCursorFromDatabase(Cursor.VIDEO_NO_CURSOR);
                this.music.SetMusicMode(MusicMode.NONE);

                return ValueTask.FromResult(ScreenName.MAINMENU_SCREEN);  //The splash screen hasn't been up long enough yet.
            }

            if (this.introScreen.guiSplashFrameFade != 0)
            { //Fade the splash screen.
                uiTime = this.clock.GetJA2Clock();
                if (this.introScreen.guiSplashFrameFade > 2)
                {
                    this.vsurface.ShadowVideoSurfaceRectUsingLowPercentTable(VideoSurfaceManager.FRAME_BUFFER, 0, 0, 640, 480);
                }
                else if (this.introScreen.guiSplashFrameFade > 1)
                {
                    this.vsurface.ColorFillVideoSurfaceArea(VideoSurfaceManager.FRAME_BUFFER, 0, 0, 640, 480, 0);
                }
                else
                {
                    uiTime = this.clock.GetJA2Clock();
                    //while( GetJA2Clock() < uiTime + 375 );
                    this.music.SetMusicMode(MusicMode.MAIN_MENU);
                }

                //while( uiTime + 100 > GetJA2Clock() );

                this.introScreen.guiSplashFrameFade--;

                this.video.InvalidateScreen();
                this.video.EndFrameBufferRender();

                this.cursor.SetCurrentCursorFromDatabase(Cursor.VIDEO_NO_CURSOR);

                return ValueTask.FromResult(ScreenName.MAINMENU_SCREEN);
            }

            this.cursor.SetCurrentCursorFromDatabase(Cursor.NORMAL);

            if (gfMainMenuScreenEntry)
            {
                this.InitMainMenu();
                gfMainMenuScreenEntry = false;
                gfMainMenuScreenExit = false;
                guiMainMenuExitScreen = ScreenName.MAINMENU_SCREEN;
                this.music.SetMusicMode(MusicMode.MAIN_MENU);
            }

            if (fInitialRender)
            {
                ClearMainMenu();
                this.RenderMainMenu();

                fInitialRender = false;
            }

            this.RestoreButtonBackGrounds();

            // Render buttons
            for (cnt = 0; cnt < (int)MainMenuItems.NUM_MENU_ITEMS; cnt++)
            {
                this.buttons.MarkAButtonDirty(iMenuButtons[cnt]);
            }

            this.buttons.RenderButtons();

            this.video.EndFrameBufferRender();


            //	if ( gfDoHelpScreen )
            //		HandleHelpScreenInput();
            //	else
            this.HandleMainMenuInput();

            this.HandleMainMenuScreen();

            if (gfMainMenuScreenExit)
            {
                this.ExitMainMenu();
                gfMainMenuScreenExit = false;
                gfMainMenuScreenEntry = true;
            }

            if (guiMainMenuExitScreen != ScreenName.MAINMENU_SCREEN)
            {
                gfMainMenuScreenEntry = true;
            }

            return ValueTask.FromResult(guiMainMenuExitScreen);
        }

        private void ExitMainMenu()
        {
        }

        private void HandleMainMenuScreen()
        {
                if (gbHandledMainMenu != 0)
                {
                    // Exit according to handled value!
                    switch (gbHandledMainMenu)
                    {
                        case MainMenuItems.QUIT:
                            gfMainMenuScreenExit = true;

                            this.globals.gfProgramIsRunning = false;
                            break;

                        case MainMenuItems.NEW_GAME:

                            //					gfDoHelpScreen = 1;
                            //				gfMainMenuScreenExit = true;
                            //				if( !gfDoHelpScreen )
                            //					SetMainMenuExitScreen( INIT_SCREEN );
                            break;

                        case MainMenuItems.LOAD_GAME:
                            // Select the game which is to be restored
                            // guiPreviousOptionScreen = guiCurrentScreen;
                            guiMainMenuExitScreen = ScreenName.SAVE_LOAD_SCREEN;
                            gbHandledMainMenu = 0;
                            // gfSaveGame = false;
                            gfMainMenuScreenExit = true;

                            break;

                        case MainMenuItems.PREFERENCES:
                            //this.optionsScreen.guiPreviousOptionScreen = guiCurrentScreen;
                            guiMainMenuExitScreen = ScreenName.OPTIONS_SCREEN;
                            gbHandledMainMenu = 0;
                            gfMainMenuScreenExit = true;
                            break;

                        case MainMenuItems.CREDITS:
                            guiMainMenuExitScreen = ScreenName.CREDIT_SCREEN;
                            gbHandledMainMenu = 0;
                            gfMainMenuScreenExit = true;
                            break;
                    }
                }
            
        }

        private void HandleMainMenuInput()
        {
            // Check for key
            while (this.input.DequeueEvent(out var InputEvent) == true)
            {
                switch (InputEvent!.Value.KeyboardEvents)
                {
                    case KeyboardEvents.KEY_UP:
                        this.SetMainMenuExitScreen(ScreenName.InitScreen);
                        break;
                }
            }

        }

        private void SetMainMenuExitScreen(ScreenName screen)
        {
            guiMainMenuExitScreen = screen;

            //Remove the background region
            CreateDestroyBackGroundMouseMask(false);

            gfMainMenuScreenExit = true;
        }

        private void RestoreButtonBackGrounds()
        {
            byte cnt;

            //	RestoreExternBackgroundRect( (UINT16)(320 - gusMainMenuButtonWidths[TITLE]/2), MAINMENU_TITLE_Y, gusMainMenuButtonWidths[TITLE], 23 );


            for (cnt = 0; cnt < (byte)MainMenuItems.NUM_MENU_ITEMS; cnt++)
            {
                this.renderDirty.RestoreExternBackgroundRect(
                    (ushort)(320 - gusMainMenuButtonWidths[cnt] / 2),
                    (short)(MAINMENU_Y + (cnt * MAINMENU_Y_SPACE) - 1),
                    (ushort)(gusMainMenuButtonWidths[cnt] + 1),
                    23);
            }
        }

        private void RenderMainMenu()
        {
            (Texture, HVOBJECT) hPixHandle;

            //Get and display the background image
            this.video.GetVideoObject(guiMainMenuBackGroundImage, out hPixHandle);
            this.video.BltVideoObject(this.video.guiSAVEBUFFER, hPixHandle, 0, 0, 0, VideoObjectManager.VO_BLT_SRCTRANSPARENCY, null);
            this.video.BltVideoObject(VideoSurfaceManager.FRAME_BUFFER, hPixHandle, 0, 0, 0, VideoObjectManager.VO_BLT_SRCTRANSPARENCY, null);

            this.video.GetVideoObject(guiJa2LogoImage, out hPixHandle);
            this.video.BltVideoObject(VideoSurfaceManager.FRAME_BUFFER, hPixHandle, 0, 188, 15, VideoObjectManager.VO_BLT_SRCTRANSPARENCY, null);
            this.video.BltVideoObject(this.video.guiSAVEBUFFER, hPixHandle, 0, 188, 15, VideoObjectManager.VO_BLT_SRCTRANSPARENCY, null);

            this.video.DrawTextToScreen(EnglishText.gzCopyrightText[0], 0, 465, 640, FontStyle.FONT10ARIAL, FontColor.FONT_MCOLOR_WHITE, FontColor.FONT_MCOLOR_BLACK, false, TextJustifies.CENTER_JUSTIFIED);

            this.video.InvalidateRegion(0, 0, 640, 480);
        }

        public ValueTask<bool> Initialize()
        {
            return ValueTask.FromResult(true);
        }

        public ValueTask<bool> InitMainMenu()
        {
            VOBJECT_DESC VObjectDesc = new();

            //	gfDoHelpScreen = 0;

            //Check to see whatr saved game files exist
            // TODO: re-add when saveloadscreen is added.
            // this.screens.GetScreen(ScreenName.SAVE_LOAD_SCREEN, activate: false)
            // InitSaveGameArray();

            //Create the background mouse mask
            this.CreateDestroyBackGroundMouseMask(true);

            this.CreateDestroyMainMenuButtons(fCreate: true);

            // load background graphic and add it
            VObjectDesc.fCreateFlags = VideoObjectCreateFlags.VOBJECT_CREATE_FROMFILE;
            VObjectDesc.ImageFile = Utils.FilenameForBPP("LOADSCREENS\\MainMenuBackGround.sti");
            this.video.AddVideoObject(ref VObjectDesc, out guiMainMenuBackGroundImage);

            // load ja2 logo graphic and add it
            VObjectDesc.fCreateFlags = VideoObjectCreateFlags.VOBJECT_CREATE_FROMFILE;
            VObjectDesc.ImageFile = Utils.FilenameForBPP("LOADSCREENS\\Ja2Logo.sti");
            this.video.AddVideoObject(ref VObjectDesc, out guiJa2LogoImage);

            /*
                // Gray out some buttons based on status of game!
                if( gGameSettings.bLastSavedGameSlot < 0 || gGameSettings.bLastSavedGameSlot >= NUM_SAVE_GAMES )
                {
                    DisableButton( iMenuButtons[ LOAD_GAME ] );
                }
                //The ini file said we have a saved game, but there is no saved game
                else if( gbSaveGameArray[ gGameSettings.bLastSavedGameSlot ] == false )
                    DisableButton( iMenuButtons[ LOAD_GAME ] );
            */

            //if there are no saved games, disable the button
            // TODO: re-add when saveloadscreen is added.
            //if (!IsThereAnySavedGameFiles())
            //{
            this.buttons.DisableButton(iMenuButtons[(int)MainMenuItems.LOAD_GAME]);
            //}

            //	DisableButton( iMenuButtons[ CREDITS ] );
            //	DisableButton( iMenuButtons[ TITLE ] );

            gbHandledMainMenu = 0;
            fInitialRender = true;

            this.screens.SetPendingNewScreen(ScreenName.MAINMENU_SCREEN);
            guiMainMenuExitScreen = ScreenName.MAINMENU_SCREEN;

            this.options.InitGameOptions();

            this.input.DequeueAllKeyBoardEvents();

            return ValueTask.FromResult(true);
        }

        private void CreateDestroyMainMenuButtons(bool fCreate)
        {
        }

        private static bool fRegionCreated = false;
        public void CreateDestroyBackGroundMouseMask(bool fCreate)
        {
            if (fCreate)
            {
                if (fRegionCreated)
                {
                    return;
                }

                // Make a mouse region
                this.mouse.MSYS_DefineRegion(
                    ref gBackRegion,
                    0,
                    0,
                    640,
                    480,
                    MSYS_PRIORITY.HIGHEST,
                    Cursor.NORMAL,
                    null,
                    SelectMainMenuBackGroundRegionCallBack);

                // Add region
                this.mouse.MSYS_AddRegion(ref gBackRegion);

                fRegionCreated = true;
            }
            else
            {
                if (!fRegionCreated)
                {
                    return;
                }

                this.mouse.MSYS_RemoveRegion(ref gBackRegion);
                fRegionCreated = false;
            }
        }

        private void SelectMainMenuBackGroundRegionCallBack(MouseRegion region, MouseCallbackReasons iReason)
        {
            if (iReason.HasFlag(MouseCallbackReasons.INIT))
            {
            }
            else if (iReason.HasFlag(MouseCallbackReasons.LBUTTON_UP))
            {
                //		if( gfDoHelpScreen )
                //		{
                //			SetMainMenuExitScreen( INIT_SCREEN );
                //			gfDoHelpScreen = false;
                //		}
            }
            else if (iReason.HasFlag(MouseCallbackReasons.RBUTTON_UP))
            {
                /*
                        if( gfDoHelpScreen )
                        {
                            SetMainMenuExitScreen( INIT_SCREEN );
                            gfDoHelpScreen = false;
                        }
                */
            }
        }

        public void ClearMainMenu()
        {
            this.video.InvalidateScreen();
        }
    }
}
