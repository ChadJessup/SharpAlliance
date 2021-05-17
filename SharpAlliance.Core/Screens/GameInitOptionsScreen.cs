using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform;
using Veldrid;
using static SharpAlliance.Core.EnglishText;

namespace SharpAlliance.Core.Screens
{
    public class GameInitOptionsScreen : IScreen
    {
        public const FontStyle GIO_TITLE_FONT = FontStyle.FONT16ARIAL;//FONT14ARIAL
        public const FontColor GIO_TITLE_COLOR = FontColor.FONT_MCOLOR_WHITE;
        public const FontStyle GIO_TOGGLE_TEXT_FONT = FontStyle.FONT16ARIAL;//FONT14ARIAL
        public const FontColor GIO_TOGGLE_TEXT_COLOR = FontColor.FONT_MCOLOR_WHITE;
        public const FontStyle OPT_BUTTON_FONT = FontStyle.FONT14ARIAL;
        public const FontColor OPT_BUTTON_ON_COLOR = FontColor.FONT_MCOLOR_WHITE;
        public const FontColor OPT_BUTTON_OFF_COLOR = FontColor.FONT_MCOLOR_WHITE;

        //buttons
        public const int GIO_BTN_OK_X = 141;
        public const int GIO_BTN_OK_Y = 418;
        public const int GIO_CANCEL_X = 379;
        //main title
        public const int GIO_MAIN_TITLE_X = 0;
        public const int GIO_MAIN_TITLE_Y = 68;
        public const int GIO_MAIN_TITLE_WIDTH = 640;
        //radio box locations
        public const int GIO_GAP_BN_SETTINGS = 35;
        public const int GIO_OFFSET_TO_TEXT = 20;//30
        public const int GIO_OFFSET_TO_TOGGLE_BOX = 155;//200
        public const int GIO_OFFSET_TO_TOGGLE_BOX_Y = 9;
        public const int GIO_DIF_SETTINGS_X = 80;
        public const int GIO_DIF_SETTINGS_Y = 150;
        public const int GIO_DIF_SETTINGS_WIDTH = GIO_OFFSET_TO_TOGGLE_BOX - GIO_OFFSET_TO_TEXT; //230
        public const int GIO_GAME_SETTINGS_X = 350;
        public const int GIO_GAME_SETTINGS_Y = 300;//280//150
        public const int GIO_GAME_SETTINGS_WIDTH = GIO_DIF_SETTINGS_WIDTH;
        public const int GIO_GUN_SETTINGS_X = GIO_GAME_SETTINGS_X;
        public const int GIO_GUN_SETTINGS_Y = GIO_DIF_SETTINGS_Y;//150//280
        public const int GIO_GUN_SETTINGS_WIDTH = GIO_DIF_SETTINGS_WIDTH;
        public const int GIO_IRON_MAN_SETTING_X = GIO_DIF_SETTINGS_X;
        public const int GIO_IRON_MAN_SETTING_Y = GIO_GAME_SETTINGS_Y;
        public const int GIO_IRON_MAN_SETTING_WIDTH = GIO_DIF_SETTINGS_WIDTH;

        private readonly FadeScreen fade;
        private readonly GameOptions gGameOptions;
        private readonly CursorSubSystem cursor;
        private readonly GameContext context;
        private readonly FontSubSystem fonts;
        private readonly IScreenManager screens;
        private readonly IVideoManager video;
        private readonly IInputManager inputs;
        private readonly GuiManager gui;
        private bool gfGIOScreenEntry = true;
        private bool gfGIOScreenExit = false;
        private bool gfReRenderGIOScreen = true;
        private bool gfGIOButtonsAllocated = false;
        private GameMode gubGameOptionScreenHandler = GameMode.GIO_NOTHING;
        private ScreenName gubGIOExitScreen = ScreenName.GAME_INIT_OPTIONS_SCREEN;
        private string guiGIOMainBackGroundImageKey;
        private int giGioMessageBox = -1;
        private ButtonPic giGIODoneBtnImage;
        private GUI_BUTTON guiGIODoneButton;
        private ButtonPic giGIOCancelBtnImage;
        private GUI_BUTTON guiGIOCancelButton;

        private Dictionary<GunOption, GUI_BUTTON> guiGunOptionToggles = new();
        private Dictionary<IronManMode, GUI_BUTTON> guiGameSaveToggles = new();
        private Dictionary<GameDifficulty, GUI_BUTTON> guiDifficultySettingsToggles = new();
        private Dictionary<GameStyle, GUI_BUTTON> guiGameStyleToggles = new();

        public GameInitOptionsScreen(
            GameContext gameContext,
            IScreenManager screenManager,
            IVideoManager videoManager,
            IInputManager inputManager,
            GuiManager guiManager,
            FontSubSystem fontSubSystem,
            CursorSubSystem cursorSubSystem,
            GameOptions gameOptions,
            FadeScreen fadeScreen)
        {
            this.fade = fadeScreen;
            this.fonts = fontSubSystem;
            this.gGameOptions = gameOptions;
            this.cursor = cursorSubSystem;
            this.context = gameContext;
            this.inputs = inputManager;
            this.video = videoManager;
            this.gui = guiManager;
            this.screens = screenManager;
        }

        public bool IsInitialized { get; set; }
        public ScreenState State { get; set; }

        public ValueTask Activate()
        {
            int usPosY;

            this.cursor.SetCurrentCursorFromDatabase(Cursor.NORMAL);

            // load the Main trade screen backgroiund image
            this.video.AddVideoObject("InterFace\\OptionsScreenBackGround.sti", out this.guiGIOMainBackGroundImageKey);

            //Ok button
            this.giGIODoneBtnImage = this.gui.Buttons.LoadButtonImage("INTERFACE\\PreferencesButtons.sti", -1, 0, -1, 2, -1);
            this.guiGIODoneButton = this.gui.Buttons.CreateIconAndTextButton(
                this.giGIODoneBtnImage,
                EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_OK_TEXT],
                OPT_BUTTON_FONT,
                OPT_BUTTON_ON_COLOR,
                FontShadow.DEFAULT_SHADOW,
                OPT_BUTTON_OFF_COLOR,
                FontShadow.DEFAULT_SHADOW,
                ButtonTextJustifies.TEXT_CJUSTIFIED,
                new SixLabors.ImageSharp.Point(GIO_BTN_OK_X, GIO_BTN_OK_Y),
                ButtonFlags.BUTTON_TOGGLE,
                MSYS_PRIORITY.HIGH,
                MouseSubSystem.DefaultMoveCallback,
                this.BtnGIODoneCallback);

            this.gui.Buttons.SpecifyButtonSoundScheme(
                this.guiGIODoneButton,
                BUTTON_SOUND_SCHEME.BIGSWITCH3);

            this.gui.Buttons.SpecifyDisabledButtonStyle(this.guiGIODoneButton, DISABLED_STYLE.NONE);

            //Cancel button
            this.giGIOCancelBtnImage = this.gui.Buttons.UseLoadedButtonImage(this.giGIODoneBtnImage, -1, 1, -1, 3, -1);
            this.guiGIOCancelButton = this.gui.Buttons.CreateIconAndTextButton(
                this.giGIOCancelBtnImage,
                EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_CANCEL_TEXT],
                OPT_BUTTON_FONT,
                OPT_BUTTON_ON_COLOR,
                FontShadow.DEFAULT_SHADOW,
                OPT_BUTTON_OFF_COLOR,
                FontShadow.DEFAULT_SHADOW,
                ButtonTextJustifies.TEXT_CJUSTIFIED,
                new SixLabors.ImageSharp.Point(GIO_CANCEL_X, GIO_BTN_OK_Y),
                ButtonFlags.BUTTON_TOGGLE,
                MSYS_PRIORITY.HIGH,
                MouseSubSystem.DefaultMoveCallback,
                this.BtnGIOCancelCallback);

            this.gui.Buttons.SpecifyButtonSoundScheme(this.guiGIOCancelButton, BUTTON_SOUND_SCHEME.BIGSWITCH3);


            //
            //Check box to toggle Difficulty settings
            //
            usPosY = GIO_DIF_SETTINGS_Y - GIO_OFFSET_TO_TOGGLE_BOX_Y;

            for (GameDifficulty cnt = 0; cnt < GameDifficulty.NUM_DIFF_SETTINGS; cnt++)
            {
                this.guiDifficultySettingsToggles[cnt] = this.gui.Buttons.CreateCheckBoxButton(
                    new(GIO_DIF_SETTINGS_X + GIO_OFFSET_TO_TOGGLE_BOX, usPosY),
                    "INTERFACE\\OptionsCheck.sti",
                    MSYS_PRIORITY.HIGH + 10,
                    this.BtnDifficultyTogglesCallback);

                this.gui.Buttons.SetButtonUserData(this.guiDifficultySettingsToggles[cnt], 0, (int)cnt);

                usPosY += GIO_GAP_BN_SETTINGS;
            }
            if (this.gGameOptions.DifficultyLevel == DifficultyLevel.Easy)
            {
                this.guiDifficultySettingsToggles[GameDifficulty.GIO_DIFF_EASY].uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
            }
            else if (this.gGameOptions.DifficultyLevel == DifficultyLevel.Medium)
            {
                this.guiDifficultySettingsToggles[GameDifficulty.GIO_DIFF_MED].uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
            }
            else if (this.gGameOptions.DifficultyLevel == DifficultyLevel.Hard)
            {
                this.guiDifficultySettingsToggles[GameDifficulty.GIO_DIFF_HARD].uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
            }
            else
            {
                this.guiDifficultySettingsToggles[GameDifficulty.GIO_DIFF_MED].uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
            }


            //
            //Check box to toggle Game settings ( realistic, sci fi )
            //

            usPosY = GIO_GAME_SETTINGS_Y - GIO_OFFSET_TO_TOGGLE_BOX_Y;
            for (GameStyle gameStyle = 0; gameStyle < GameStyle.NUM_GAME_STYLES; gameStyle++)
            {
                this.guiGameStyleToggles[gameStyle] = this.gui.Buttons.CreateCheckBoxButton(
                    new(GIO_GAME_SETTINGS_X + GIO_OFFSET_TO_TOGGLE_BOX, usPosY),
                    "INTERFACE\\OptionsCheck.sti", MSYS_PRIORITY.HIGH + 10,
                    this.BtnGameStyleTogglesCallback);

                this.gui.Buttons.SetButtonUserData(this.guiGameStyleToggles[gameStyle], 0, (int)gameStyle);

                usPosY += GIO_GAP_BN_SETTINGS;
            }
            if (this.gGameOptions.SciFi)
            {
                this.guiGameStyleToggles[GameStyle.GIO_SCI_FI].uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
            }
            else
            {
                this.guiGameStyleToggles[GameStyle.GIO_REALISTIC].uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
            }

            // JA2Gold: iron man buttons
            usPosY = GIO_IRON_MAN_SETTING_Y - GIO_OFFSET_TO_TOGGLE_BOX_Y;
            for (IronManMode opt = 0; opt < IronManMode.NUM_SAVE_OPTIONS; opt++)
            {
                this.guiGameSaveToggles[opt] = this.gui.Buttons.CreateCheckBoxButton(
                    new(GIO_IRON_MAN_SETTING_X + GIO_OFFSET_TO_TOGGLE_BOX, usPosY),
                    "INTERFACE\\OptionsCheck.sti", MSYS_PRIORITY.HIGH + 10,
                    this.BtnGameSaveTogglesCallback);

                this.gui.Buttons.SetButtonUserData(this.guiGameSaveToggles[opt], 0, (int)opt);

                usPosY += GIO_GAP_BN_SETTINGS;
            }

            if (this.gGameOptions.IronManMode)
            {
                this.guiGameSaveToggles[IronManMode.GIO_IRON_MAN].uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
            }
            else
            {
                this.guiGameSaveToggles[IronManMode.GIO_CAN_SAVE].uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
            }

            //
            // Check box to toggle Gun options
            //

            usPosY = GIO_GUN_SETTINGS_Y - GIO_OFFSET_TO_TOGGLE_BOX_Y;
            for (GunOption cnt = 0; cnt < GunOption.NUM_GUN_OPTIONS; cnt++)
            {
                this.guiGunOptionToggles[cnt] = this.gui.Buttons.CreateCheckBoxButton(
                    new(GIO_GUN_SETTINGS_X + GIO_OFFSET_TO_TOGGLE_BOX, usPosY),
                    "INTERFACE\\OptionsCheck.sti", MSYS_PRIORITY.HIGH + 10,
                    this.BtnGunOptionsTogglesCallback);

                this.gui.Buttons.SetButtonUserData(this.guiGunOptionToggles[cnt], 0, (int)cnt);

                usPosY += GIO_GAP_BN_SETTINGS;
            }

            if (this.gGameOptions.GunNut)
            {
                this.guiGunOptionToggles[GunOption.GIO_GUN_NUT].uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
            }
            else
            {
                this.guiGunOptionToggles[GunOption.GIO_REDUCED_GUNS].uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
            }

            //Reset the exit screen
            this.gubGIOExitScreen = ScreenName.GAME_INIT_OPTIONS_SCREEN;

            //REnder the screen once so we can blt ot to ths save buffer
            this.RenderGIOScreen();

            this.video.BlitBufferToBuffer(0, 0, 639, 439);

            //this.video.BlitBufferToBuffer(guiRENDERBUFFER, guiSAVEBUFFER, 0, 0, 639, 439);

            this.gfGIOButtonsAllocated = true;

            return ValueTask.CompletedTask;
        }

        public async ValueTask<ScreenName> Handle()
        {
            if (this.gfGIOScreenEntry)
            {
                this.gfGIOScreenEntry = false;
                this.gfGIOScreenExit = false;
                this.video.InvalidateRegion(new(0, 0, 640, 480));
            }

            this.GetGIOScreenUserInput();
            this.HandleGIOScreen();

            // render buttons marked dirty	
            // this.gui.Buttons.MarkButtonsDirty();
            //this.gui.Buttons.RenderButtons();

            // render help
            //	RenderFastHelp( );
            //	RenderButtonsFastHelp( );


            // ExecuteBaseDirtyRectQueue();
            // EndFrameBufferRender();

            if (this.fade.HandleFadeOutCallback())
            {
                var mm = await this.screens.GetScreen<MainMenuScreen>(ScreenName.MAINMENU_SCREEN, activate: false);
                mm.ClearMainMenu();
                return this.gubGIOExitScreen;
            }

            if (this.fade.HandleBeginFadeOut(this.gubGIOExitScreen))
            {
                return this.gubGIOExitScreen;
            }

            if (this.gfGIOScreenExit)
            {
                this.ExitGIOScreen();
            }

            if (this.fade.HandleFadeInCallback())
            {
                // Re-render the scene!
                this.RenderGIOScreen();
            }

            if (this.fade.HandleBeginFadeIn(this.gubGIOExitScreen))
            {
            }

            return this.gubGIOExitScreen;
        }

        private void ExitGIOScreen()
        {
        }

        private void GetGIOScreenUserInput()
        {
        }

        private void HandleGIOScreen()
        {
        }

        private bool RenderGIOScreen()
        {
            int usPosY;

            //Get the main background screen graphic and blt it
            HVOBJECT background = this.video.GetVideoObject(this.guiGIOMainBackGroundImageKey);
            //BltVideoObject(FRAME_BUFFER, hPixHandle, 0, 0, 0, VO_BLT_SRCTRANSPARENCY, NULL);
            this.video.BltVideoObject(background, 0, 0, 0, 0);
            //Shade the background
            // this.video.ShadowVideoSurfaceRect(FRAME_BUFFER, 48, 55, 592, 378); //358


            //Display the title
            this.fonts.DrawTextToScreen(
                EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_INITIAL_GAME_SETTINGS],
                GIO_MAIN_TITLE_X,
                GIO_MAIN_TITLE_Y,
                GIO_MAIN_TITLE_WIDTH,
                GIO_TITLE_FONT,
                GIO_TITLE_COLOR,
                FontColor.FONT_MCOLOR_BLACK,
                TextJustifies.CENTER_JUSTIFIED);

            //Display the Dif Settings Title Text
            //DrawTextToScreen( gzGIOScreenText[ GIO_DIF_LEVEL_TEXT ], GIO_DIF_SETTINGS_X, (GIO_DIF_SETTINGS_Y-GIO_GAP_BN_SETTINGS), GIO_DIF_SETTINGS_WIDTH, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED );	
            this.fonts.DisplayWrappedString(new(GIO_DIF_SETTINGS_X, GIO_DIF_SETTINGS_Y - GIO_GAP_BN_SETTINGS), GIO_DIF_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_DIF_LEVEL_TEXT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);


            usPosY = GIO_DIF_SETTINGS_Y + 2;
            //DrawTextToScreen( gzGIOScreenText[ GIO_EASY_TEXT ], (GIO_DIF_SETTINGS_X+GIO_OFFSET_TO_TEXT), usPosY, GIO_MAIN_TITLE_WIDTH, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED );	
            this.fonts.DisplayWrappedString(new(GIO_DIF_SETTINGS_X + GIO_OFFSET_TO_TEXT, usPosY), GIO_DIF_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_EASY_TEXT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

            usPosY += GIO_GAP_BN_SETTINGS;
            //DrawTextToScreen( gzGIOScreenText[ GIO_MEDIUM_TEXT ], (GIO_DIF_SETTINGS_X+GIO_OFFSET_TO_TEXT), usPosY, GIO_MAIN_TITLE_WIDTH, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED );	
            this.fonts.DisplayWrappedString(new(GIO_DIF_SETTINGS_X + GIO_OFFSET_TO_TEXT, usPosY), GIO_DIF_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_MEDIUM_TEXT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

            usPosY += GIO_GAP_BN_SETTINGS;
            //DrawTextToScreen( gzGIOScreenText[ GIO_HARD_TEXT ], (GIO_DIF_SETTINGS_X+GIO_OFFSET_TO_TEXT), usPosY, GIO_MAIN_TITLE_WIDTH, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED );	
            this.fonts.DisplayWrappedString(new(GIO_DIF_SETTINGS_X + GIO_OFFSET_TO_TEXT, usPosY), GIO_DIF_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_HARD_TEXT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

            //Display the Game Settings Title Text
            //	DrawTextToScreen( gzGIOScreenText[ GIO_GAME_STYLE_TEXT ], GIO_GAME_SETTINGS_X, (GIO_GAME_SETTINGS_Y-GIO_GAP_BN_SETTINGS), GIO_GAME_SETTINGS_WIDTH, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED );	
            this.fonts.DisplayWrappedString(new(GIO_GAME_SETTINGS_X, GIO_GAME_SETTINGS_Y - GIO_GAP_BN_SETTINGS), GIO_GAME_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_GAME_STYLE_TEXT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

            usPosY = GIO_GAME_SETTINGS_Y + 2;
            //DrawTextToScreen( gzGIOScreenText[ GIO_REALISTIC_TEXT ], (GIO_GAME_SETTINGS_X+GIO_OFFSET_TO_TEXT), usPosY, GIO_MAIN_TITLE_WIDTH, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED );	
            this.fonts.DisplayWrappedString(new(GIO_GAME_SETTINGS_X + GIO_OFFSET_TO_TEXT, usPosY), GIO_GAME_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_REALISTIC_TEXT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

            usPosY += GIO_GAP_BN_SETTINGS;
            //DrawTextToScreen( gzGIOScreenText[ GIO_SCI_FI_TEXT ], (GIO_GAME_SETTINGS_X+GIO_OFFSET_TO_TEXT), usPosY, GIO_MAIN_TITLE_WIDTH, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED );	
            this.fonts.DisplayWrappedString(new(GIO_GAME_SETTINGS_X + GIO_OFFSET_TO_TEXT, usPosY), GIO_GAME_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_SCI_FI_TEXT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

            //Display the Gun Settings Title Text
            //	DrawTextToScreen( gzGIOScreenText[ GIO_GUN_OPTIONS_TEXT ], GIO_GUN_SETTINGS_X, (GIO_GUN_SETTINGS_Y-GIO_GAP_BN_SETTINGS), GIO_GUN_SETTINGS_WIDTH, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED );	
            this.fonts.DisplayWrappedString(new(GIO_GUN_SETTINGS_X, GIO_GUN_SETTINGS_Y - GIO_GAP_BN_SETTINGS), GIO_GUN_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_GUN_OPTIONS_TEXT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

            usPosY = GIO_GUN_SETTINGS_Y + 2;
            //DrawTextToScreen( gzGIOScreenText[ GIO_REDUCED_GUNS_TEXT ], (GIO_GUN_SETTINGS_X+GIO_OFFSET_TO_TEXT), usPosY, GIO_MAIN_TITLE_WIDTH, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED );	
            this.fonts.DisplayWrappedString(new(GIO_GUN_SETTINGS_X + GIO_OFFSET_TO_TEXT, usPosY), GIO_GUN_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_REDUCED_GUNS_TEXT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

            usPosY += GIO_GAP_BN_SETTINGS;
            //DrawTextToScreen( gzGIOScreenText[ GIO_GUN_NUT_TEXT ], (GIO_GUN_SETTINGS_X+GIO_OFFSET_TO_TEXT), usPosY, GIO_MAIN_TITLE_WIDTH, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED );	
            this.fonts.DisplayWrappedString(new(GIO_GUN_SETTINGS_X + GIO_OFFSET_TO_TEXT, usPosY), GIO_GUN_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_GUN_NUT_TEXT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

            // JA2Gold: no more timed turns setting
            /*
            //Display the Timed turns Settings Title Text
            DisplayWrappedString( GIO_TIMED_TURN_SETTING_X, (GIO_TIMED_TURN_SETTING_Y-GIO_GAP_BN_SETTINGS), GIO_DIF_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, gzGIOScreenText[ GIO_TIMED_TURN_TITLE_TEXT ], FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED );
            usPosY = GIO_TIMED_TURN_SETTING_Y+2;

            DisplayWrappedString( (GIO_TIMED_TURN_SETTING_X+GIO_OFFSET_TO_TEXT), usPosY, GIO_DIF_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, gzGIOScreenText[ GIO_NO_TIMED_TURNS_TEXT ], FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED );
            usPosY += GIO_GAP_BN_SETTINGS;

            DisplayWrappedString( (GIO_TIMED_TURN_SETTING_X+GIO_OFFSET_TO_TEXT), usPosY, GIO_DIF_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, gzGIOScreenText[ GIO_TIMED_TURNS_TEXT ], FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED );
            */

            // JA2Gold: Display the iron man Settings Title Text
            this.fonts.DisplayWrappedString(new(GIO_IRON_MAN_SETTING_X, GIO_IRON_MAN_SETTING_Y - GIO_GAP_BN_SETTINGS), GIO_DIF_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_GAME_SAVE_STYLE_TEXT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
            usPosY = GIO_IRON_MAN_SETTING_Y + 2;

            this.fonts.DisplayWrappedString(new(GIO_IRON_MAN_SETTING_X + GIO_OFFSET_TO_TEXT, usPosY), GIO_DIF_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_SAVE_ANYWHERE_TEXT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
            usPosY += GIO_GAP_BN_SETTINGS;

            this.fonts.DisplayWrappedString(new(GIO_IRON_MAN_SETTING_X + GIO_OFFSET_TO_TEXT, usPosY), GIO_DIF_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_IRON_MAN_TEXT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

            usPosY += 20;
            this.fonts.DisplayWrappedString(new(GIO_IRON_MAN_SETTING_X + GIO_OFFSET_TO_TEXT, usPosY), 220, 2, FontStyle.FONT12ARIAL, GIO_TOGGLE_TEXT_COLOR, EnglishText.zNewTacticalMessages[(int)TCTL_MSG__.CANNOT_SAVE_DURING_COMBAT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

            return true;
        }

        public void Draw(SpriteRenderer sr, GraphicsDevice gd, CommandList cl)
        {
            this.RenderGIOScreen();
            //this.gui.Buttons.MarkButtonsDirty();
            //this.gui.Buttons.RenderButtons();
        }

        public ValueTask<bool> Initialize()
        {

            return ValueTask.FromResult(true);
        }

        public void Dispose()
        {
        }

        private void BtnDifficultyTogglesCallback(ref GUI_BUTTON btn, MouseCallbackReasons reason)
        {
            if (reason.HasFlag(MouseCallbackReasons.LBUTTON_UP))
            {
                var ubButton = this.gui.Buttons.MSYS_GetBtnUserData(btn, 0);

                if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
                {
                    for (GameDifficulty cnt = 0; cnt < GameDifficulty.NUM_DIFF_SETTINGS; cnt++)
                    {
                        this.guiDifficultySettingsToggles[cnt].uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
                    }

                    //enable the current button
                    btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
                }
                else
                {
                    bool fAnyChecked = false;

                    //if none of the other boxes are checked, do not uncheck this box
                    for (GunOption cnt = 0; cnt < GunOption.NUM_GUN_OPTIONS; cnt++)
                    {
                        if (this.guiDifficultySettingsToggles[(GameDifficulty)cnt].uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
                        {
                            fAnyChecked = true;
                        }
                    }

                    //if none are checked, re check this one
                    if (!fAnyChecked)
                    {
                        btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
                    }
                }
            }
        }

        private void BtnGameStyleTogglesCallback(ref GUI_BUTTON btn, MouseCallbackReasons reason)
        {
            if (reason.HasFlag(MouseCallbackReasons.LBUTTON_UP))
            {
                var ubButton = this.gui.Buttons.MSYS_GetBtnUserData(btn, 0);

                if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
                {
                    for (GameStyle cnt = 0; cnt < GameStyle.NUM_GAME_STYLES; cnt++)
                    {
                        this.guiGameStyleToggles[cnt].uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
                    }

                    //enable the current button
                    btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
                }
                else
                {
                    bool fAnyChecked = false;

                    //if none of the other boxes are checked, do not uncheck this box
                    for (GunOption cnt = 0; cnt < GunOption.NUM_GUN_OPTIONS; cnt++)
                    {
                        if (this.guiGameStyleToggles[(GameStyle)cnt].uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
                        {
                            fAnyChecked = true;
                        }
                    }
                    //if none are checked, re check this one
                    if (!fAnyChecked)
                    {
                        btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
                    }
                }
            }
        }

        private void BtnGameSaveTogglesCallback(ref GUI_BUTTON btn, MouseCallbackReasons reason)
        {
            if (reason.HasFlag(MouseCallbackReasons.LBUTTON_UP))
            {
                //		UINT8	ubButton = (UINT8)MSYS_GetBtnUserData( btn, 0 );

                if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
                {
                    for (IronManMode cnt = 0; cnt < IronManMode.NUM_SAVE_OPTIONS; cnt++)
                    {
                        this.guiGameSaveToggles[cnt].uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
                    }

                    //enable the current button
                    btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
                }
                else
                {
                    bool fAnyChecked = false;

                    //if none of the other boxes are checked, do not uncheck this box
                    for (IronManMode cnt = 0; cnt < IronManMode.NUM_SAVE_OPTIONS; cnt++)
                    {
                        if (this.guiGameSaveToggles[cnt].uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
                        {
                            fAnyChecked = true;
                        }
                    }

                    //if none are checked, re check this one
                    if (!fAnyChecked)
                    {
                        btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
                    }
                }
            }
        }

        private void BtnGunOptionsTogglesCallback(ref GUI_BUTTON btn, MouseCallbackReasons reason)
        {
            if (reason.HasFlag(MouseCallbackReasons.LBUTTON_UP))
            {
                var ubButton = this.gui.Buttons.MSYS_GetBtnUserData(btn, 0);

                if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
                {
                    for (GunOption cnt = 0; cnt < GunOption.NUM_GUN_OPTIONS; cnt++)
                    {
                        this.guiGunOptionToggles[cnt].uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
                    }

                    //enable the current button
                    btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
                }
                else
                {
                    bool fAnyChecked = false;

                    //if none of the other boxes are checked, do not uncheck this box
                    for (GunOption cnt = 0; cnt < GunOption.NUM_GUN_OPTIONS; cnt++)
                    {
                        if (this.guiGunOptionToggles[cnt].uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
                        {
                            fAnyChecked = true;
                        }
                    }

                    //if none are checked, re check this one
                    if (!fAnyChecked)
                    {
                        btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
                    }
                }
            }
        }

        private void BtnGIODoneCallback(ref GUI_BUTTON btn, MouseCallbackReasons reason)
        {
            if (reason.HasFlag(MouseCallbackReasons.LBUTTON_DWN))
            {
                btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
                this.video.InvalidateRegion(btn.MouseRegion.Bounds);
            }
            if (reason.HasFlag(MouseCallbackReasons.LBUTTON_UP))
            {
                btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;

                //if the user doesnt have IRON MAN mode selected
                if (!this.DisplayMessageToUserAboutIronManMode())
                {
                    //Confirm the difficulty setting
                    this.DisplayMessageToUserAboutGameDifficulty();
                }

                this.video.InvalidateRegion(btn.MouseRegion.Bounds);
            }
        }

        private bool DisplayMessageToUserAboutIronManMode()
        {
            return true;
        }

        private void DisplayMessageToUserAboutGameDifficulty()
        {
        }

        private void BtnGIOCancelCallback(ref GUI_BUTTON btn, MouseCallbackReasons reason)
        {
            if (reason.HasFlag(MouseCallbackReasons.LBUTTON_DWN))
            {
                btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
                this.video.InvalidateRegion(btn.MouseRegion.Bounds);
            }

            if (reason.HasFlag(MouseCallbackReasons.LBUTTON_UP))
            {
                btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;

                this.gubGameOptionScreenHandler = GameMode.GIO_CANCEL;

                this.video.InvalidateRegion(btn.MouseRegion.Bounds);
            }
        }

        public ValueTask Deactivate()
        {
            throw new NotImplementedException();
        }
    }

    //Difficulty settings
    public enum GameDifficulty
    {
        GIO_DIFF_EASY,
        GIO_DIFF_MED,
        GIO_DIFF_HARD,

        NUM_DIFF_SETTINGS,
    };

    // Game Settings options
    public enum GameStyle
    {
        GIO_REALISTIC,
        GIO_SCI_FI,

        NUM_GAME_STYLES,
    };

    public enum GameMode
    {
        GIO_NOTHING,
        GIO_CANCEL,
        GIO_EXIT,
        GIO_IRON_MAN_MODE
    };

    // Gun options
    public enum GunOption
    {
        GIO_REDUCED_GUNS,
        GIO_GUN_NUT,

        NUM_GUN_OPTIONS,
    };

    public enum IronManMode
    {
        GIO_CAN_SAVE,
        GIO_IRON_MAN,

        NUM_SAVE_OPTIONS,
    };

    // Game init option screen
    public enum GameInitOptionScreenText
    {
        GIO_INITIAL_GAME_SETTINGS,

        GIO_GAME_STYLE_TEXT,
        GIO_REALISTIC_TEXT,
        GIO_SCI_FI_TEXT,

        GIO_GUN_OPTIONS_TEXT,
        GIO_GUN_NUT_TEXT,
        GIO_REDUCED_GUNS_TEXT,

        GIO_DIF_LEVEL_TEXT,
        GIO_EASY_TEXT,
        GIO_MEDIUM_TEXT,
        GIO_HARD_TEXT,

        GIO_OK_TEXT,
        GIO_CANCEL_TEXT,

        GIO_GAME_SAVE_STYLE_TEXT,
        GIO_SAVE_ANYWHERE_TEXT,
        GIO_IRON_MAN_TEXT,
        GIO_DISABLED_FOR_THE_DEMO_TEXT,
    };
}
