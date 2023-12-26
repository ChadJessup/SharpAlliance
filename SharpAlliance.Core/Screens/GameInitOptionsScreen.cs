using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform;
using SixLabors.ImageSharp;
using static SharpAlliance.Core.EnglishText;
using static SharpAlliance.Core.Globals;
namespace SharpAlliance.Core.Screens;

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
    private readonly IVideoManager video;
    private readonly FadeScreen fade;
    private readonly GameOptions gGameOptions;
    private readonly ButtonSubSystem buttons;
    private readonly CursorSubSystem cursor;
    private readonly GameContext context;
    private readonly FontSubSystem fonts;
    private readonly IScreenManager screens;
    private readonly IInputManager input;
    private readonly GuiManager gui;
    private bool gfGIOScreenEntry = true;
    private bool gfGIOScreenExit = false;
    private bool gfReRenderGIOScreen = true;
    private bool gfGIOButtonsAllocated = false;
    private GameMode gubGameOptionScreenHandler = GameMode.GIO_NOTHING;
    private ScreenName gubGIOExitScreen = ScreenName.GAME_INIT_OPTIONS_SCREEN;
    private IntroScreen introScreen;
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
        IInputManager inputManager,
        GuiManager guiManager,
        IVideoManager videoManager,
        ButtonSubSystem buttonSubSystem,
        FontSubSystem fontSubSystem,
        CursorSubSystem cursorSubSystem,
        GameOptions gameOptions,
        FadeScreen fadeScreen)
    {
        this.video = videoManager;
        this.fade = fadeScreen;
        this.fonts = fontSubSystem;
        this.gGameOptions = gameOptions;
        this.buttons = buttonSubSystem;
        this.cursor = cursorSubSystem;
        this.context = gameContext;
        this.input = inputManager;
        this.gui = guiManager;
        this.screens = screenManager;
    }

    public bool IsInitialized { get; set; }
    public ScreenState State { get; set; }

    public async ValueTask Activate()
    {
        int usPosY;

        CursorSubSystem.SetCurrentCursorFromDatabase(CURSOR.NORMAL);

        // load the Main trade screen backgroiund image
        this.video.GetVideoObject("InterFace\\OptionsScreenBackGround.sti", out this.guiGIOMainBackGroundImageKey);

        //Ok button
        this.giGIODoneBtnImage = this.buttons.LoadButtonImage("INTERFACE\\PreferencesButtons.sti", -1, 0, -1, 2, -1);
        this.guiGIODoneButton = ButtonSubSystem.CreateIconAndTextButton(
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

        ButtonSubSystem.SpecifyButtonSoundScheme(
            this.guiGIODoneButton,
            BUTTON_SOUND_SCHEME.BIGSWITCH3);

        ButtonSubSystem.SpecifyDisabledButtonStyle(this.guiGIODoneButton, DISABLED_STYLE.NONE);

        //Cancel button
        this.giGIOCancelBtnImage = ButtonSubSystem.UseLoadedButtonImage(this.giGIODoneBtnImage, -1, 1, -1, 3, -1);
        this.guiGIOCancelButton = ButtonSubSystem.CreateIconAndTextButton(
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

        ButtonSubSystem.SpecifyButtonSoundScheme(this.guiGIOCancelButton, BUTTON_SOUND_SCHEME.BIGSWITCH3);


        //
        //Check box to toggle Difficulty settings
        //
        usPosY = GIO_DIF_SETTINGS_Y - GIO_OFFSET_TO_TOGGLE_BOX_Y;

        for (GameDifficulty cnt = 0; cnt < GameDifficulty.NUM_DIFF_SETTINGS; cnt++)
        {
            this.guiDifficultySettingsToggles[cnt] = this.buttons.CreateCheckBoxButton(
                new(GIO_DIF_SETTINGS_X + GIO_OFFSET_TO_TOGGLE_BOX, usPosY),
                "INTERFACE\\OptionsCheck.sti",
                MSYS_PRIORITY.HIGH + 10,
                this.BtnDifficultyTogglesCallback);

            ButtonSubSystem.SetButtonUserData(this.guiDifficultySettingsToggles[cnt], 0, (int)cnt);

            usPosY += GIO_GAP_BN_SETTINGS;
        }
        if (this.gGameOptions.ubDifficultyLevel == DifficultyLevel.Easy)
        {
            this.guiDifficultySettingsToggles[GameDifficulty.GIO_DIFF_EASY].uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
        }
        else if (this.gGameOptions.ubDifficultyLevel == DifficultyLevel.Medium)
        {
            this.guiDifficultySettingsToggles[GameDifficulty.GIO_DIFF_MED].uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
        }
        else if (this.gGameOptions.ubDifficultyLevel == DifficultyLevel.Hard)
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
            this.guiGameStyleToggles[gameStyle] = this.buttons.CreateCheckBoxButton(
                new(GIO_GAME_SETTINGS_X + GIO_OFFSET_TO_TOGGLE_BOX, usPosY),
                "INTERFACE\\OptionsCheck.sti", MSYS_PRIORITY.HIGH + 10,
                this.BtnGameStyleTogglesCallback);

            ButtonSubSystem.SetButtonUserData(this.guiGameStyleToggles[gameStyle], 0, (int)gameStyle);

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
            this.guiGameSaveToggles[opt] = this.buttons.CreateCheckBoxButton(
                new(GIO_IRON_MAN_SETTING_X + GIO_OFFSET_TO_TOGGLE_BOX, usPosY),
                "INTERFACE\\OptionsCheck.sti", MSYS_PRIORITY.HIGH + 10,
                this.BtnGameSaveTogglesCallback);

            ButtonSubSystem.SetButtonUserData(this.guiGameSaveToggles[opt], 0, (int)opt);

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
            this.guiGunOptionToggles[cnt] = this.buttons.CreateCheckBoxButton(
                new(GIO_GUN_SETTINGS_X + GIO_OFFSET_TO_TOGGLE_BOX, usPosY),
                "INTERFACE\\OptionsCheck.sti", MSYS_PRIORITY.HIGH + 10,
                this.BtnGunOptionsTogglesCallback);

            ButtonSubSystem.SetButtonUserData(this.guiGunOptionToggles[cnt], 0, (int)cnt);

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

        this.introScreen = (await this.screens.GetScreen(ScreenName.INTRO_SCREEN, activate: false) as IntroScreen)!;

        //REnder the screen once so we can blt ot to ths save buffer
        this.RenderGIOScreen();

        this.video.BlitBufferToBuffer(SurfaceType.RENDER_BUFFER, SurfaceType.SAVE_BUFFER, new(0, 0, 639, 439));

        this.gfGIOButtonsAllocated = true;
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
        ButtonSubSystem.MarkButtonsDirty();
        ButtonSubSystem.RenderButtons();

        // render help
        //RenderFastHelp( );
        GuiManager.RenderButtonsFastHelp( );

        this.video.ExecuteBaseDirtyRectQueue();
        this.video.EndFrameBufferRender();

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
        // Check for key
        while (this.input.DequeueEvent(out var Event) == true)
        {
            foreach (var ke in Event.KeyEvents.Where(ke => ke.Down))
            {
                switch (ke.Key)
                {
                    case Key.Escape:
                        //Exit out of the screen
                        this.gubGameOptionScreenHandler = GameMode.GIO_CANCEL;
                        break;

                    case Key.Enter:
                        this.gubGameOptionScreenHandler = GameMode.GIO_EXIT;
                        break;
                }
            }
        }
    }

    private void HandleGIOScreen()
    {
        if (this.gubGameOptionScreenHandler != GameMode.GIO_NOTHING)
        {
            switch (this.gubGameOptionScreenHandler)
            {
                case GameMode.GIO_CANCEL:
                    this.gubGIOExitScreen = ScreenName.MAINMENU_SCREEN;
                    this.gfGIOScreenExit = true;
                    break;

                case GameMode.GIO_EXIT:
                    //if we are already fading out, get out of here
                    if (this.fade.gFadeOutDoneCallback != this.DoneFadeOutForExitGameInitOptionScreen)
                    {
                        //Disable the ok button
                        ButtonSubSystem.DisableButton(this.guiGIODoneButton);

                        this.fade.gFadeOutDoneCallback = this.DoneFadeOutForExitGameInitOptionScreen;

                        this.fade.FadeOutNextFrame();
                    }
                
                    break;
                case GameMode.GIO_IRON_MAN_MODE:
                    this.DisplayMessageToUserAboutGameDifficulty();
                    break;
            }

            this.gubGameOptionScreenHandler = GameMode.GIO_NOTHING;
        }


        if (this.gfReRenderGIOScreen)
        {
            this.RenderGIOScreen();
            this.gfReRenderGIOScreen = false;
        }

        this.RestoreGIOButtonBackGrounds();
    }

    private void RestoreGIOButtonBackGrounds()
    {
        ushort usPosY;

        usPosY = GIO_DIF_SETTINGS_Y - GIO_OFFSET_TO_TOGGLE_BOX_Y;
        //Check box to toggle Difficulty settings
        for (GameDifficulty cnt = 0; cnt < GameDifficulty.NUM_DIFF_SETTINGS; cnt++)
        {
            RenderDirty.RestoreExternBackgroundRect(GIO_DIF_SETTINGS_X + GIO_OFFSET_TO_TOGGLE_BOX, usPosY, 34, 29);
            usPosY += GIO_GAP_BN_SETTINGS;
        }

        usPosY = GIO_GAME_SETTINGS_Y - GIO_OFFSET_TO_TOGGLE_BOX_Y;
        //Check box to toggle Game settings ( realistic, sci fi )
        for (GameStyle cnt = 0; cnt < GameStyle.NUM_GAME_STYLES; cnt++)
        {
            RenderDirty.RestoreExternBackgroundRect(GIO_GAME_SETTINGS_X + GIO_OFFSET_TO_TOGGLE_BOX, usPosY, 34, 29);

            usPosY += GIO_GAP_BN_SETTINGS;
        }

        usPosY = GIO_GUN_SETTINGS_Y - GIO_OFFSET_TO_TOGGLE_BOX_Y;

        //Check box to toggle Gun options
        for (GunOption cnt = 0; cnt < GunOption.NUM_GUN_OPTIONS; cnt++)
        {
            RenderDirty.RestoreExternBackgroundRect(GIO_GUN_SETTINGS_X + GIO_OFFSET_TO_TOGGLE_BOX, usPosY, 34, 29);
            usPosY += GIO_GAP_BN_SETTINGS;
        }

        // JA2Gold: no more timed turns setting
        /*
        //Check box to toggle timed turns options
        usPosY = GIO_TIMED_TURN_SETTING_Y-GIO_OFFSET_TO_TOGGLE_BOX_Y;
        for( cnt=0; cnt<GIO_NUM_TIMED_TURN_OPTIONS; cnt++)
        {
            RestoreExternBackgroundRect( GIO_TIMED_TURN_SETTING_X+GIO_OFFSET_TO_TOGGLE_BOX, usPosY, 34, 29 ); 
            usPosY += GIO_GAP_BN_SETTINGS;
        }
        */
        //Check box to toggle iron man options
        usPosY = GIO_IRON_MAN_SETTING_Y - GIO_OFFSET_TO_TOGGLE_BOX_Y;
        for (IronManMode cnt = 0; cnt < IronManMode.NUM_SAVE_OPTIONS; cnt++)
        {
            RenderDirty.RestoreExternBackgroundRect(GIO_IRON_MAN_SETTING_X + GIO_OFFSET_TO_TOGGLE_BOX, usPosY, 34, 29);
            usPosY += GIO_GAP_BN_SETTINGS;
        }
    }

    private void DoneFadeOutForExitGameInitOptionScreen()
    {
        // loop through and get the status of all the buttons
        this.gGameOptions.GunNut = this.GetCurrentGunButtonSetting();
        this.gGameOptions.SciFi = this.GetCurrentGameStyleButtonSetting();
        this.gGameOptions.ubDifficultyLevel = this.GetCurrentDifficultyButtonSetting() + 1;
        // JA2Gold: no more timed turns setting
        //gGameOptions.fTurnTimeLimit = GetCurrentTimedTurnsButtonSetting();
        // JA2Gold: iron man
        this.gGameOptions.IronManMode = this.GetCurrentGameSaveButtonSetting();


        //	gubGIOExitScreen = INIT_SCREEN;
        this.gubGIOExitScreen = ScreenName.INTRO_SCREEN;

        //set the fact that we should do the intro videos
        //	gbIntroScreenMode = INTRO_BEGINING;
        this.introScreen.SetIntroType(IntroScreenType.INTRO_BEGINING);

        this.ExitGIOScreen();

        //	gFadeInDoneCallback = DoneFadeInForExitGameInitOptionScreen;
        //	FadeInNextFrame( );
        CursorSubSystem.SetCurrentCursorFromDatabase(CURSOR.VIDEO_NO_CURSOR);
    }

    private bool GetCurrentGameSaveButtonSetting()
    {
        for (IronManMode cnt = 0; cnt < IronManMode.NUM_SAVE_OPTIONS; cnt++)
        {
            if (this.guiGameSaveToggles[cnt].uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
                return true;
            }
        }
    
        return false;
    }

    private DifficultyLevel GetCurrentDifficultyButtonSetting()
    {
        for (GameDifficulty cnt = 0; cnt < GameDifficulty.NUM_DIFF_SETTINGS; cnt++)
        {
            if (this.guiDifficultySettingsToggles[cnt].uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
                return (DifficultyLevel)cnt;
            }
        }

        return 0;
    }

    public bool GetCurrentGameStyleButtonSetting()
    {
        for (GameStyle cnt = 0; cnt < GameStyle.NUM_GAME_STYLES; cnt++)
        {
            if (this.guiGameStyleToggles[cnt].uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
                return true;
            }
        }
        
        return false;
    }

    private bool GetCurrentGunButtonSetting()
    {
        for (GunOption cnt = 0; cnt < GunOption.NUM_GUN_OPTIONS; cnt++)
        {
            if (this.guiGunOptionToggles[cnt].uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
                return true;
            }
        }
        
        return false;
    }

    private bool RenderGIOScreen()
    {
        int usPosY;

        //Get the main background screen graphic and blt it
       HVOBJECT background = video.GetVideoObject(this.guiGIOMainBackGroundImageKey);
       this.video.BltVideoObject(SurfaceType.FRAME_BUFFER, background, 0, 0, 0, VO_BLT.SRCTRANSPARENCY);
        //video.BltVideoObject(background, 0, 0, 0, 0);
        //Shade the background
        this.video.ShadowVideoSurfaceRect(SurfaceType.FRAME_BUFFER, new Rectangle(48, 55, 592, 378)); //358

        //Display the title
        FontSubSystem.DrawTextToScreen(
            EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_INITIAL_GAME_SETTINGS],
            new(GIO_MAIN_TITLE_X, GIO_MAIN_TITLE_Y),
            GIO_MAIN_TITLE_WIDTH,
            GIO_TITLE_FONT,
            GIO_TITLE_COLOR,
            FontColor.FONT_MCOLOR_BLACK,
            TextJustifies.CENTER_JUSTIFIED);

        //Display the Dif Settings Title Text
        //DrawTextToScreen( gzGIOScreenText[ GIO_DIF_LEVEL_TEXT ], GIO_DIF_SETTINGS_X, (GIO_DIF_SETTINGS_Y-GIO_GAP_BN_SETTINGS), GIO_DIF_SETTINGS_WIDTH, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED );	
        FontSubSystem.DisplayWrappedString(new(GIO_DIF_SETTINGS_X, GIO_DIF_SETTINGS_Y - GIO_GAP_BN_SETTINGS), GIO_DIF_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_DIF_LEVEL_TEXT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);


        usPosY = GIO_DIF_SETTINGS_Y + 2;
        //DrawTextToScreen( gzGIOScreenText[ GIO_EASY_TEXT ], (GIO_DIF_SETTINGS_X+GIO_OFFSET_TO_TEXT), usPosY, GIO_MAIN_TITLE_WIDTH, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED );	
        FontSubSystem.DisplayWrappedString(new(GIO_DIF_SETTINGS_X + GIO_OFFSET_TO_TEXT, usPosY), GIO_DIF_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_EASY_TEXT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

        usPosY += GIO_GAP_BN_SETTINGS;
        //DrawTextToScreen( gzGIOScreenText[ GIO_MEDIUM_TEXT ], (GIO_DIF_SETTINGS_X+GIO_OFFSET_TO_TEXT), usPosY, GIO_MAIN_TITLE_WIDTH, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED );	
        FontSubSystem.DisplayWrappedString(new(GIO_DIF_SETTINGS_X + GIO_OFFSET_TO_TEXT, usPosY), GIO_DIF_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_MEDIUM_TEXT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

        usPosY += GIO_GAP_BN_SETTINGS;
        //DrawTextToScreen( gzGIOScreenText[ GIO_HARD_TEXT ], (GIO_DIF_SETTINGS_X+GIO_OFFSET_TO_TEXT), usPosY, GIO_MAIN_TITLE_WIDTH, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED );	
        FontSubSystem.DisplayWrappedString(new(GIO_DIF_SETTINGS_X + GIO_OFFSET_TO_TEXT, usPosY), GIO_DIF_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_HARD_TEXT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

        //Display the Game Settings Title Text
        //	DrawTextToScreen( gzGIOScreenText[ GIO_GAME_STYLE_TEXT ], GIO_GAME_SETTINGS_X, (GIO_GAME_SETTINGS_Y-GIO_GAP_BN_SETTINGS), GIO_GAME_SETTINGS_WIDTH, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED );	
        FontSubSystem.DisplayWrappedString(new(GIO_GAME_SETTINGS_X, GIO_GAME_SETTINGS_Y - GIO_GAP_BN_SETTINGS), GIO_GAME_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_GAME_STYLE_TEXT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

        usPosY = GIO_GAME_SETTINGS_Y + 2;
        //DrawTextToScreen( gzGIOScreenText[ GIO_REALISTIC_TEXT ], (GIO_GAME_SETTINGS_X+GIO_OFFSET_TO_TEXT), usPosY, GIO_MAIN_TITLE_WIDTH, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED );	
        FontSubSystem.DisplayWrappedString(new(GIO_GAME_SETTINGS_X + GIO_OFFSET_TO_TEXT, usPosY), GIO_GAME_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_REALISTIC_TEXT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

        usPosY += GIO_GAP_BN_SETTINGS;
        //DrawTextToScreen( gzGIOScreenText[ GIO_SCI_FI_TEXT ], (GIO_GAME_SETTINGS_X+GIO_OFFSET_TO_TEXT), usPosY, GIO_MAIN_TITLE_WIDTH, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED );	
        FontSubSystem.DisplayWrappedString(new(GIO_GAME_SETTINGS_X + GIO_OFFSET_TO_TEXT, usPosY), GIO_GAME_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_SCI_FI_TEXT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

        //Display the Gun Settings Title Text
        //	DrawTextToScreen( gzGIOScreenText[ GIO_GUN_OPTIONS_TEXT ], GIO_GUN_SETTINGS_X, (GIO_GUN_SETTINGS_Y-GIO_GAP_BN_SETTINGS), GIO_GUN_SETTINGS_WIDTH, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED );	
        FontSubSystem.DisplayWrappedString(new(GIO_GUN_SETTINGS_X, GIO_GUN_SETTINGS_Y - GIO_GAP_BN_SETTINGS), GIO_GUN_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_GUN_OPTIONS_TEXT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

        usPosY = GIO_GUN_SETTINGS_Y + 2;
        //DrawTextToScreen( gzGIOScreenText[ GIO_REDUCED_GUNS_TEXT ], (GIO_GUN_SETTINGS_X+GIO_OFFSET_TO_TEXT), usPosY, GIO_MAIN_TITLE_WIDTH, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED );	
        FontSubSystem.DisplayWrappedString(new(GIO_GUN_SETTINGS_X + GIO_OFFSET_TO_TEXT, usPosY), GIO_GUN_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_REDUCED_GUNS_TEXT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

        usPosY += GIO_GAP_BN_SETTINGS;
        //DrawTextToScreen( gzGIOScreenText[ GIO_GUN_NUT_TEXT ], (GIO_GUN_SETTINGS_X+GIO_OFFSET_TO_TEXT), usPosY, GIO_MAIN_TITLE_WIDTH, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED );	
        FontSubSystem.DisplayWrappedString(new(GIO_GUN_SETTINGS_X + GIO_OFFSET_TO_TEXT, usPosY), GIO_GUN_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_GUN_NUT_TEXT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

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
        FontSubSystem.DisplayWrappedString(new(GIO_IRON_MAN_SETTING_X, GIO_IRON_MAN_SETTING_Y - GIO_GAP_BN_SETTINGS), GIO_DIF_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_GAME_SAVE_STYLE_TEXT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
        usPosY = GIO_IRON_MAN_SETTING_Y + 2;

        FontSubSystem.DisplayWrappedString(new(GIO_IRON_MAN_SETTING_X + GIO_OFFSET_TO_TEXT, usPosY), GIO_DIF_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_SAVE_ANYWHERE_TEXT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
        usPosY += GIO_GAP_BN_SETTINGS;

        FontSubSystem.DisplayWrappedString(new(GIO_IRON_MAN_SETTING_X + GIO_OFFSET_TO_TEXT, usPosY), GIO_DIF_SETTINGS_WIDTH, 2, GIO_TOGGLE_TEXT_FONT, GIO_TOGGLE_TEXT_COLOR, EnglishText.gzGIOScreenText[GameInitOptionScreenText.GIO_IRON_MAN_TEXT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

        usPosY += 20;
        FontSubSystem.DisplayWrappedString(new(GIO_IRON_MAN_SETTING_X + GIO_OFFSET_TO_TEXT, usPosY), 220, 2, FontStyle.FONT12ARIAL, GIO_TOGGLE_TEXT_COLOR, EnglishText.zNewTacticalMessages[(int)TCTL_MSG__.CANNOT_SAVE_DURING_COMBAT], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

        return true;
    }

    public void Draw(IVideoManager videoManager)
    {
        this.RenderGIOScreen();
        //ButtonSubSystem.MarkButtonsDirty();
        //ButtonSubSystem.RenderButtons();
    }

    public ValueTask<bool> Initialize()
    {

        return ValueTask.FromResult(true);
    }

    public void Dispose()
    {
    }

    private void BtnDifficultyTogglesCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            var ubButton = ButtonSubSystem.MSYS_GetBtnUserData(btn, 0);

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

    private void BtnGameStyleTogglesCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            var ubButton = ButtonSubSystem.MSYS_GetBtnUserData(btn, 0);

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

    private void BtnGameSaveTogglesCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            //		Ubyte	ubButton = (Ubyte)MSYS_GetBtnUserData( btn, 0 );

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

    private void BtnGunOptionsTogglesCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            var ubButton = ButtonSubSystem.MSYS_GetBtnUserData(btn, 0);

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

    private void BtnGIODoneCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
            this.video.InvalidateRegion(btn.MouseRegion.Bounds);
        }
        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
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

    private void BtnGIOCancelCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
            this.video.InvalidateRegion(btn.MouseRegion.Bounds);
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;

            this.gubGameOptionScreenHandler = GameMode.GIO_CANCEL;

            this.video.InvalidateRegion(btn.MouseRegion.Bounds);
        }
    }

    public ValueTask Deactivate()
    {
        return ValueTask.CompletedTask;
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
