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
using SixLabors.ImageSharp;
using Rectangle = SixLabors.ImageSharp.Rectangle;
using Point = SixLabors.ImageSharp.Point;
using SharpAlliance.Core.Managers.VideoSurfaces;

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
        private readonly CursorSubSystem cursor;
        private readonly IClockManager clock;

        public const string MAINMENU_TEXT_FILE = "LoadScreens\\MainMenu.edt";
        public const int MAINMENU_RECORD_SIZE = 80 * 2;
        public const int MAINMENU_X = (640 - 214) / 2;
        public const int MAINMENU_TITLE_Y = 75;
        public const int MAINMENU_Y_SPACE = 37;
        public const int MAINMENU_Y = 480 - 187;

        // MENU ITEMS
        private enum MainMenuItems
        {
            //	TITLE,
            NEW_GAME,
            LOAD_GAME,
            PREFERENCES,
            CREDITS,
            QUIT,
            NUM_MENU_ITEMS,
            Unknown = 99,
        };

        private Dictionary<MainMenuItems, int> iMenuImages = new();
        private Dictionary<MainMenuItems, int> iMenuButtons = new();

        ushort[] gusMainMenuButtonWidths = new ushort[(int)MainMenuItems.NUM_MENU_ITEMS];

        string mainMenuBackGroundImageKey;
        string ja2LogoImageKey;

        private MouseRegion gBackRegion = new(nameof(gBackRegion));
        private MainMenuItems gbHandledMainMenu = MainMenuItems.Unknown;
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
            ButtonSubSystem buttonSubSystem,
            CursorSubSystem cursorSubSystem,
            IInputManager inputManager,
            Globals globals,
            RenderDirtySubSystem renderDirtySubSystem)
        {
            this.globals = globals;
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

        public async ValueTask<ScreenName> Handle()
        {
            uint cnt;
            uint uiTime;

            if (this.introScreen.guiSplashStartTime + 4000 > this.clock.GetJA2Clock())
            {
                this.cursor.SetCurrentCursorFromDatabase(Cursor.VIDEO_NO_CURSOR);
                this.music.SetMusicMode(MusicMode.NONE);

                //The splash screen hasn't been up long enough yet.
                return ScreenName.MAINMENU_SCREEN;
            }

            if (this.introScreen.guiSplashFrameFade != 0)
            { //Fade the splash screen.
                uiTime = this.clock.GetJA2Clock();
                if (this.introScreen.guiSplashFrameFade > 2)
                {
                    this.video.ShadowVideoSurfaceRectUsingLowPercentTable(new Rectangle(0, 0, 640, 480));
                }
                else if (this.introScreen.guiSplashFrameFade > 1)
                {
                    this.video.ColorFillVideoSurfaceArea(Surfaces.FRAME_BUFFER, new Rectangle(0, 0, 640, 480), Color.Black);
                }
                else
                {
                    uiTime = this.clock.GetJA2Clock();
                    //while( GetJA2Clock() < uiTime + 375 );
                    this.music.SetMusicMode(MusicMode.MAIN_MENU);
                }

                //while( uiTime + 100 > GetJA2Clock() );

                this.introScreen.guiSplashFrameFade--;

                // this.video.InvalidateScreen();
                // this.video.EndFrameBufferRender();

                this.cursor.SetCurrentCursorFromDatabase(Cursor.VIDEO_NO_CURSOR);

                return ScreenName.MAINMENU_SCREEN;
            }

            this.cursor.SetCurrentCursorFromDatabase(Cursor.NORMAL);

            if (this.gfMainMenuScreenEntry)
            {
                await this.InitMainMenu();
                this.gfMainMenuScreenEntry = false;
                this.gfMainMenuScreenExit = false;
                this.guiMainMenuExitScreen = ScreenName.MAINMENU_SCREEN;
                this.music.SetMusicMode(MusicMode.MAIN_MENU);
            }

            if (this.fInitialRender)
            {
                this.ClearMainMenu();
                this.RenderMainMenu();

                this.fInitialRender = false;
            }

            this.RestoreButtonBackGrounds();

            // Render buttons
            for (cnt = 0; cnt < (int)MainMenuItems.NUM_MENU_ITEMS; cnt++)
            {
                this.buttons.MarkAButtonDirty(this.iMenuButtons[(MainMenuItems)cnt]);
            }

//            this.video.EndFrameBufferRender();

            this.HandleMainMenuInput();

            this.HandleMainMenuScreen();

            if (this.gfMainMenuScreenExit)
            {
                this.ExitMainMenu();
                this.gfMainMenuScreenExit = false;
                this.gfMainMenuScreenEntry = true;
            }

            if (this.guiMainMenuExitScreen != ScreenName.MAINMENU_SCREEN)
            {
                this.gfMainMenuScreenEntry = true;
            }

            return this.guiMainMenuExitScreen;
        }

        private void ExitMainMenu()
        {
            this.CreateDestroyBackGroundMouseMask(false);

            this.CreateDestroyMainMenuButtons(false);

            this.video.DeleteVideoObjectFromIndex(this.mainMenuBackGroundImageKey);
            this.video.DeleteVideoObjectFromIndex(this.ja2LogoImageKey);

            //gMsgBox.uiExitScreen = ScreenName.MAINMENU_SCREEN;
        }

        private void HandleMainMenuScreen()
        {
            if (this.gbHandledMainMenu != MainMenuItems.Unknown)
            {
                // Exit according to handled value!
                switch (this.gbHandledMainMenu)
                {
                    case MainMenuItems.QUIT:
                        this.gfMainMenuScreenExit = true;

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
                        this.guiMainMenuExitScreen = ScreenName.SAVE_LOAD_SCREEN;
                        this.gbHandledMainMenu = 0;
                        // gfSaveGame = false;
                        this.gfMainMenuScreenExit = true;

                        break;

                    case MainMenuItems.PREFERENCES:
                        //this.optionsScreen.guiPreviousOptionScreen = guiCurrentScreen;
                        this.guiMainMenuExitScreen = ScreenName.OPTIONS_SCREEN;
                        this.gbHandledMainMenu = 0;
                        this.gfMainMenuScreenExit = true;
                        break;

                    case MainMenuItems.CREDITS:
                        this.guiMainMenuExitScreen = ScreenName.CREDIT_SCREEN;
                        this.gbHandledMainMenu = 0;
                        this.gfMainMenuScreenExit = true;
                        break;
                }
            }

        }

        private void HandleMainMenuInput()
        {
            // Check for key
            while (this.input.DequeueEvent(out var InputEvent) == true)
            {
                if (InputEvent.KeyEvents.Any(ke => !ke.Down))
                {
                    this.SetMainMenuExitScreen(ScreenName.InitScreen);

                }
            }
        }

        private void SetMainMenuExitScreen(ScreenName screen)
        {
            this.guiMainMenuExitScreen = screen;

            //Remove the background region
            this.CreateDestroyBackGroundMouseMask(false);

            this.gfMainMenuScreenExit = true;
        }

        private void RestoreButtonBackGrounds()
        {
            byte cnt;

            //	RestoreExternBackgroundRect( (ushort)(320 - gusMainMenuButtonWidths[TITLE]/2), MAINMENU_TITLE_Y, gusMainMenuButtonWidths[TITLE], 23 );


            for (cnt = 0; cnt < (byte)MainMenuItems.NUM_MENU_ITEMS; cnt++)
            {
                this.renderDirty.RestoreExternBackgroundRect(
                    (ushort)(320 - this.gusMainMenuButtonWidths[cnt] / 2),
                    (short)(MAINMENU_Y + (cnt * MAINMENU_Y_SPACE) - 1),
                    (ushort)(this.gusMainMenuButtonWidths[cnt] + 1),
                    23);
            }
        }

        private void RenderMainMenu()
        {
            HVOBJECT hPixHandle;

            //Get and display the background image
            hPixHandle = this.video.GetVideoObject(this.mainMenuBackGroundImageKey);
            this.video.BltVideoObject(hPixHandle, 0, 0, 0, 0);

            hPixHandle = this.video.GetVideoObject(this.ja2LogoImageKey);
            this.video.BltVideoObject(hPixHandle, 0, 188, 480 - (15 + (int)hPixHandle.Textures[0].Height), 0);

            this.video.DrawTextToScreen(EnglishText.gzCopyrightText[0], 0, 465, 640, FontStyle.FONT10ARIAL, FontColor.FONT_MCOLOR_WHITE, FontColor.FONT_MCOLOR_BLACK, false, TextJustifies.CENTER_JUSTIFIED);

//            this.video.InvalidateRegion(new Rectangle(0, 0, 640, 480));
        }

        public ValueTask<bool> Initialize()
        {
            return ValueTask.FromResult(true);
        }

        public async ValueTask<bool> InitMainMenu()
        {
            //	gfDoHelpScreen = 0;

            //Check to see whatr saved game files exist
            // TODO: re-add when saveloadscreen is added.
            // this.screens.GetScreen(ScreenName.SAVE_LOAD_SCREEN, activate: false)
            // InitSaveGameArray();

            //Create the background mouse mask
            this.CreateDestroyBackGroundMouseMask(true);

            this.CreateDestroyMainMenuButtons(fCreate: true);

            // load background graphic and add it
            this.video.AddVideoObject("LOADSCREENS\\MainMenuBackGround.sti", out this.mainMenuBackGroundImageKey);

            // load ja2 logo graphic and add it
            this.video.AddVideoObject("LOADSCREENS\\Ja2Logo.sti", out this.ja2LogoImageKey);

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
            this.buttons.DisableButton(this.iMenuButtons[MainMenuItems.LOAD_GAME]);
            //}

            //	DisableButton( iMenuButtons[ CREDITS ] );
            //	DisableButton( iMenuButtons[ TITLE ] );

            this.gbHandledMainMenu = 0;
            this.fInitialRender = true;

            await this.screens.SetPendingNewScreen(ScreenName.MAINMENU_SCREEN);
            this.guiMainMenuExitScreen = ScreenName.MAINMENU_SCREEN;

            this.options.InitGameOptions();

            this.input.DequeueAllKeyBoardEvents();

            return true;
        }

        private static bool fButtonsCreated = false;
        private bool CreateDestroyMainMenuButtons(bool fCreate)
        {
            int cnt;
            string filename;
            short sSlot;
            int iStartLoc = 0;
            string zText;

            if (fCreate)
            {
                if (fButtonsCreated)
                {
                    return true;
                }

                //reset the variable that allows the user to ALT click on the continue save btn to load the save instantly
                // TODO: Enable when SaveLoadScreen is ported.
                //gfLoadGameUponEntry = false;

                // Load button images
                filename = "LOADSCREENS\\titletext.sti";// MLG_TITLETEXT);

                this.iMenuImages[MainMenuItems.NEW_GAME] = this.buttons.LoadButtonImage(filename, 0, 0, 1, 2, -1);
                sSlot = 0;

                this.iMenuImages[MainMenuItems.LOAD_GAME] = this.buttons.UseLoadedButtonImage(this.iMenuImages[MainMenuItems.NEW_GAME], 6, 3, 4, 5, -1);
                this.iMenuImages[MainMenuItems.PREFERENCES] = this.buttons.UseLoadedButtonImage(this.iMenuImages[MainMenuItems.NEW_GAME], 7, 7, 8, 9, -1);
                this.iMenuImages[MainMenuItems.CREDITS] = this.buttons.UseLoadedButtonImage(this.iMenuImages[MainMenuItems.NEW_GAME], 13, 10, 11, 12, -1);
                this.iMenuImages[MainMenuItems.QUIT] = this.buttons.UseLoadedButtonImage(this.iMenuImages[MainMenuItems.NEW_GAME], 14, 14, 15, 16, -1);

                for (cnt = 0; cnt < (int)MainMenuItems.NUM_MENU_ITEMS; cnt++)
                {
                    var menuItem = (MainMenuItems)cnt;
                    switch (cnt)
                    {
                        case (int)MainMenuItems.NEW_GAME:
                            this.gusMainMenuButtonWidths[cnt] = this.buttons.GetWidthOfButtonPic((ushort)this.iMenuImages[menuItem], sSlot);
                            break;
                        case (int)MainMenuItems.LOAD_GAME:
                            this.gusMainMenuButtonWidths[cnt] = this.buttons.GetWidthOfButtonPic((ushort)this.iMenuImages[menuItem], 3);
                            break;
                        case (int)MainMenuItems.PREFERENCES:
                            this.gusMainMenuButtonWidths[cnt] = this.buttons.GetWidthOfButtonPic((ushort)this.iMenuImages[menuItem], 7);
                            break;
                        case (int)MainMenuItems.CREDITS:
                            this.gusMainMenuButtonWidths[cnt] = this.buttons.GetWidthOfButtonPic((ushort)this.iMenuImages[menuItem], 10);
                            break;
                        case (int)MainMenuItems.QUIT:
                            this.gusMainMenuButtonWidths[cnt] = this.buttons.GetWidthOfButtonPic((ushort)this.iMenuImages[menuItem], 15);
                            break;
                    }

                    this.iMenuButtons[menuItem] = this.buttons.QuickCreateButton(
                        this.iMenuImages[menuItem],
                        new Point(320 - this.gusMainMenuButtonWidths[cnt] / 2, MAINMENU_Y + (cnt * MAINMENU_Y_SPACE)),
                        ButtonFlags.BUTTON_TOGGLE,
                        MSYS_PRIORITY.HIGHEST,
                        MouseSubSystem.DefaultMoveCallback,
                        this.MenuButtonCallback);

                    if (this.iMenuButtons[menuItem] == -1)
                    {
                        return false;
                    }

                    this.buttons.ButtonList[this.iMenuButtons[menuItem]].UserData[0] = cnt;
                }

                fButtonsCreated = true;
            }
            else
            {
                if (!fButtonsCreated)
                {
                    return true;
                }

                // Delete images/buttons
                for (cnt = 0; cnt < (int)MainMenuItems.NUM_MENU_ITEMS; cnt++)
                {
                    this.buttons.RemoveButton(this.iMenuButtons[(MainMenuItems)cnt]);
                    this.buttons.UnloadButtonImage(this.iMenuImages[(MainMenuItems)cnt]);
                }

                fButtonsCreated = false;
            }

            return true;
        }

        private void MenuButtonCallback(ref GUI_BUTTON btn, MouseCallbackReasons reasonValue)
        {
            MouseCallbackReasons reason = reasonValue;
            MainMenuItems bID;

            bID = (MainMenuItems)btn.UserData[0];

            if (!btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED))
            {
                return;
            }

            if (reason.HasFlag(MouseCallbackReasons.LBUTTON_UP))
            {
                // handle menu
                this.gbHandledMainMenu = bID;
                this.RenderMainMenu();

                if (this.gbHandledMainMenu == MainMenuItems.NEW_GAME)
                {
                    this.SetMainMenuExitScreen(ScreenName.GAME_INIT_OPTIONS_SCREEN);
                }
                else if (this.gbHandledMainMenu == MainMenuItems.LOAD_GAME)
                {
                    // if (gfKeyState[ALT])
                    // {
                    //     gfLoadGameUponEntry = true;
                    // }
                }

                btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
            }

            if (reason.HasFlag(MouseCallbackReasons.LBUTTON_DWN))
            {
                this.RenderMainMenu();
                btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
            }
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
                    ref this.gBackRegion,
                    new(0, 0, 640, 480),
                    MSYS_PRIORITY.HIGHEST,
                    Cursor.NORMAL,
                    null,
                    this.SelectMainMenuBackGroundRegionCallBack);

                fRegionCreated = true;
            }
            else
            {
                if (!fRegionCreated)
                {
                    return;
                }

                this.mouse.MSYS_RemoveRegion(ref this.gBackRegion);
                fRegionCreated = false;
            }
        }

        private void SelectMainMenuBackGroundRegionCallBack(ref MouseRegion region, MouseCallbackReasons iReason)
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

        public void Draw(SpriteRenderer sr, GraphicsDevice gd, CommandList cl)
        {
            //Get and display the background image
            //            this.video.GetVideoObject(this.mainMenuBackGroundImageKey, out var background);
            //            this.video.GetVideoObject(this.ja2LogoImageKey, out var logo);

            var background = this.video.AddVideoObject("LOADSCREENS\\MainMenuBackGround.sti", out this.mainMenuBackGroundImageKey);

            // load ja2 logo graphic and add it
            var logo = this.video.AddVideoObject("LOADSCREENS\\Ja2Logo.sti", out this.ja2LogoImageKey);

            sr.AddSprite(rectangle: new (0, 0, 640, 480), background.Textures[0], this.mainMenuBackGroundImageKey);
            sr.AddSprite(loc: new(188, 480 - (15 + (int)logo.Textures[0].Height)), logo.Textures[0], this.ja2LogoImageKey);

            this.buttons.RenderButtons();

            this.video.DrawTextToScreen(EnglishText.gzCopyrightText[0], 0, 465, 640, FontStyle.FONT10ARIAL, FontColor.FONT_MCOLOR_WHITE, FontColor.FONT_MCOLOR_BLACK, false, TextJustifies.CENTER_JUSTIFIED);
        }

        public ValueTask Deactivate()
        {
            return ValueTask.CompletedTask;
        }
    }
}
