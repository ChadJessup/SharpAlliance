using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform.Interfaces;
using SixLabors.ImageSharp;
using Veldrid;
using Point = SixLabors.ImageSharp.Point;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace SharpAlliance.Core.Screens
{
    public class PreferenceScreen : IScreen
    {
        public const FontStyle OPT_BUTTON_FONT = FontStyle.FONT14ARIAL;
        public const FontColor OPT_BUTTON_ON_COLOR = FontColor.FONT_MCOLOR_WHITE;
        public const FontColor OPT_BUTTON_OFF_COLOR = FontColor.FONT_MCOLOR_WHITE;

        public const FontStyle OPTIONS_TITLE_FONT = FontStyle.FONT14ARIAL;
        public const FontColor OPTIONS_TITLE_COLOR = FontColor.FONT_MCOLOR_WHITE;

        public const FontStyle OPT_MAIN_FONT = FontStyle.FONT12ARIAL;
        public const FontColor OPT_MAIN_COLOR = OPT_BUTTON_ON_COLOR;//FONT_MCOLOR_WHITE
        public const FontColor OPT_HIGHLIGHT_COLOR = FontColor.FONT_MCOLOR_WHITE;//FONT_MCOLOR_LTYELLOW

        public const int OPTIONS_SCREEN_WIDTH = 440;
        public const int OPTIONS_SCREEN_HEIGHT = 400;

        public const int OPTIONS__TOP_LEFT_X = 100;
        public const int OPTIONS__TOP_LEFT_Y = 40;
        public const int OPTIONS__BOTTOM_RIGHT_X = OPTIONS__TOP_LEFT_X + OPTIONS_SCREEN_WIDTH;
        public const int OPTIONS__BOTTOM_RIGHT_Y = OPTIONS__TOP_LEFT_Y + OPTIONS_SCREEN_HEIGHT;

        public const int OPT_SAVE_BTN_X = 51;
        public const int OPT_SAVE_BTN_Y = 438;

        public const int OPT_LOAD_BTN_X = 190;
        public const int OPT_LOAD_BTN_Y = OPT_SAVE_BTN_Y;

        public const int OPT_QUIT_BTN_X = 329;
        public const int OPT_QUIT_BTN_Y = OPT_SAVE_BTN_Y;

        public const int OPT_DONE_BTN_X = 469;
        public const int OPT_DONE_BTN_Y = OPT_SAVE_BTN_Y;

        public const int OPT_GAP_BETWEEN_TOGGLE_BOXES = 31;//40

        public TOPTION gubFirstColOfOptions = (TOPTION)OPT_FIRST_COLUMN_TOGGLE_CUT_OFF;


        //Text
        public const int OPT_TOGGLE_BOX_FIRST_COL_TEXT_X = OPT_TOGGLE_BOX_FIRST_COLUMN_X + OPT_SPACE_BETWEEN_TEXT_AND_TOGGLE_BOX;//350
        public const int OPT_TOGGLE_BOX_FIRST_COL_TEXT_Y = OPT_TOGGLE_BOX_FIRST_COLUMN_START_Y;//100

        public const int OPT_TOGGLE_BOX_SECOND_TEXT_X = OPT_TOGGLE_BOX_SECOND_COLUMN_X + OPT_SPACE_BETWEEN_TEXT_AND_TOGGLE_BOX;//350
        public const int OPT_TOGGLE_BOX_SECOND_TEXT_Y = OPT_TOGGLE_BOX_SECOND_COLUMN_START_Y;//100

        //toggle boxes
        public const int OPT_SPACE_BETWEEN_TEXT_AND_TOGGLE_BOX = 30;//220
        public const int OPT_TOGGLE_TEXT_OFFSET_Y = 2;//3

        public const int OPT_TOGGLE_BOX_FIRST_COLUMN_X = 265; //257 //OPT_TOGGLE_BOX_TEXT_X + OPT_SPACE_BETWEEN_TEXT_AND_TOGGLE_BOX
        public const int OPT_TOGGLE_BOX_FIRST_COLUMN_START_Y = 110;//OPT_TOGGLE_BOX_TEXT_Y

        public const int OPT_TOGGLE_BOX_SECOND_COLUMN_X = 428; //OPT_TOGGLE_BOX_TEXT_X + OPT_SPACE_BETWEEN_TEXT_AND_TOGGLE_BOX
        public const int OPT_TOGGLE_BOX_SECOND_COLUMN_START_Y = OPT_TOGGLE_BOX_FIRST_COLUMN_START_Y;

        public const int OPT_TOGGLE_BOX_TEXT_WIDTH = OPT_TOGGLE_BOX_SECOND_COLUMN_X - OPT_TOGGLE_BOX_FIRST_COLUMN_X - 20;

        // Slider bar defines
        public const int OPT_GAP_BETWEEN_SLIDER_BARS = 60;
        //#define		OPT_SLIDER_BAR_WIDTH								200
        public const int OPT_SLIDER_BAR_SIZE = 258;

        public const int OPT_SLIDER_TEXT_WIDTH = 45;

        public const int OPT_SOUND_FX_TEXT_X = 38;
        public const int OPT_SOUND_FX_TEXT_Y = 87;//116//110

        public const int OPT_SPEECH_TEXT_X = 85;//OPT_SOUND_FX_TEXT_X + OPT_SLIDER_TEXT_WIDTH
        public const int OPT_SPEECH_TEXT_Y = OPT_SOUND_FX_TEXT_Y;//OPT_SOUND_FX_TEXT_Y + OPT_GAP_BETWEEN_SLIDER_BARS

        public const int OPT_MUSIC_TEXT_X = 137;
        public const int OPT_MUSIC_TEXT_Y = OPT_SOUND_FX_TEXT_Y;//OPT_SPEECH_TEXT_Y + OPT_GAP_BETWEEN_SLIDER_BARS

        public const int OPT_TEXT_TO_SLIDER_OFFSET_Y = 25;

        public const int OPT_SOUND_EFFECTS_SLIDER_X = 56;
        public const int OPT_SOUND_EFFECTS_SLIDER_Y = 126;//110 + OPT_TEXT_TO_SLIDER_OFFSET_Y

        public const int OPT_SPEECH_SLIDER_X = 107;
        public const int OPT_SPEECH_SLIDER_Y = OPT_SOUND_EFFECTS_SLIDER_Y;

        public const int OPT_MUSIC_SLIDER_X = 158;
        public const int OPT_MUSIC_SLIDER_Y = OPT_SOUND_EFFECTS_SLIDER_Y;

        public const int OPT_MUSIC_SLIDER_PLAY_SOUND_DELAY = 75;

        public const int OPT_FIRST_COLUMN_TOGGLE_CUT_OFF = 10;//8

        private readonly IClockManager clock;
        private readonly MessageBoxSubSystem messageBox;
        private readonly GameSettings settings;
        private readonly FontSubSystem fonts;
        private readonly GameOptions options;
        private readonly ISoundManager sound;
        private readonly IInputManager inputs;
        private readonly IMusicManager music;
        private readonly GameInit gameInit;
        private readonly Messages messages;

        private readonly List<GUI_BUTTON> buttonList = new();

        private bool gfOptionsScreenEntry;
        private bool gfOptionsScreenExit;
        private bool gfRedrawOptionsScreen;
        private bool gfEnteredFromMapScreen;
        private bool gfExitOptionsAfterMessageBox;
        private bool gfExitOptionsDueToMessageBox;
        private int giOptionsMessageBox;
        private ScreenName guiOptionsScreen = ScreenName.OPTIONS_SCREEN;
        private ScreenName guiPreviousOptionScreen = ScreenName.OPTIONS_SCREEN;

        private string guiOptionBackGroundImageKey;
        private string guiOptionsAddOnImagesKey;
        private Dictionary<TOPTION, GUI_BUTTON> guiOptionsToggles = new((int)TOPTION.NUM_GAME_OPTIONS);
        //Mouse regions for the name of the option
        private Dictionary<TOPTION, MOUSE_REGION> gSelectedOptionTextRegion = new((int)TOPTION.NUM_GAME_OPTIONS);

        private ButtonPic giOptionsButtonImages;
        private GUI_BUTTON guiOptGotoSaveGameBtn;
        private ButtonPic giGotoLoadBtnImage;
        private GUI_BUTTON guiOptGotoLoadGameBtn;
        private ButtonPic giQuitBtnImage;
        private GUI_BUTTON guiQuitButton;
        private ButtonPic giDoneBtnImage;
        private GUI_BUTTON guiDoneButton;

        private Slider guiSoundEffectsSlider;
        private Slider guiSpeechSlider;
        private Slider guiMusicSlider;
        private int gbHighLightedOptionText;
        private bool gfSettingOfTreeTopStatusOnEnterOfOptionScreen;
        private bool gfSettingOfItemGlowStatusOnEnterOfOptionScreen;
        private bool gfSettingOfDontAnimateSmoke;
        private uint guiSoundFxSliderMoving;

        public PreferenceScreen(
            IVideoManager videoManager,
            IClockManager clockManager,
            ISoundManager soundManager,
            IInputManager inputManager,
            IMusicManager musicManager,
            GameInit gameInit,
            GameSettings gameSettings,
            GameOptions gameOptions,
            FontSubSystem fontSubSystem,
            Messages messages,
            MessageBoxSubSystem messageBoxSubSystem)
        {
            this.messages = messages;
            this.gameInit = gameInit;
            this.music = musicManager;
            this.fonts = fontSubSystem;
            this.options = gameOptions;
            this.settings = gameSettings;
            this.sound = soundManager;
            this.inputs = inputManager;
            this.clock = clockManager;
            this.messageBox = messageBoxSubSystem;
        }

        public bool IsInitialized { get; set; }
        public ScreenState State { get; set; }

        public ValueTask Activate()
        {
            foreach (var option in Enum.GetValues<TOPTION>())
            {
                try
                {
                    this.gSelectedOptionTextRegion.Add(option, new MOUSE_REGION(option.ToString()));
                }
                catch (Exception e)
                {

                }
            }

            return ValueTask.CompletedTask;
        }

        public void Draw(SpriteRenderer sr, GraphicsDevice gd, CommandList cl)
        {
            var background = VeldridVideoManager.AddVideoObject("INTERFACE\\OptionScreenBase.sti", out this.guiOptionBackGroundImageKey);

            // load button, title graphic and add it
            var options = VeldridVideoManager.AddVideoObject("INTERFACE\\optionscreenaddons.sti", out this.guiOptionsAddOnImagesKey);

            this.RenderOptionsScreen();

            sr.AddSprite(new Point(0, 0), background.Textures[0], this.guiOptionBackGroundImageKey);
            sr.AddSprite(new Point(0, 434), options.Textures[0], this.guiOptionsAddOnImagesKey);

            // render buttons marked dirty	
            ButtonSubSystem.MarkButtonsDirty(this.buttonList);
            ButtonSubSystem.RenderButtons(this.buttonList);

            GuiManager.RenderSliderBars();
        }

        public ValueTask<ScreenName> Handle()
        {
            if (this.gfOptionsScreenEntry)
            {
                ClockManager.PauseGame();
                this.EnterOptionsScreen();
                this.gfOptionsScreenEntry = false;
                this.gfOptionsScreenExit = false;
                this.gfRedrawOptionsScreen = true;
                this.RenderOptionsScreen();

                //Blit the background to the save buffer
                //VeldridVideoManager.BlitBufferToBuffer(guiRENDERBUFFER, guiSAVEBUFFER, 0, 0, 640, 480);
                VeldridVideoManager.InvalidateRegion(new Rectangle(0, 0, 640, 480));
            }

            VeldridVideoManager.RestoreBackgroundRects();

            this.GetOptionsScreenUserInput();

            this.HandleOptionsScreen();

            if (this.gfRedrawOptionsScreen)
            {
                GuiManager.RenderButtons(this.buttonList);
                this.RenderOptionsScreen();

                this.gfRedrawOptionsScreen = false;
            }

            //Render the active slider bars
            GuiManager.RenderSliderBars();

            // render buttons marked dirty	
            ButtonSubSystem.MarkButtonsDirty(this.buttonList);
            GuiManager.RenderButtons(this.buttonList);

            // ATE: Put here to save RECTS before any fast help being drawn...
            VeldridVideoManager.SaveBackgroundRects();
            GuiManager.RenderButtonsFastHelp();

            VeldridVideoManager.ExecuteBaseDirtyRectQueue();
            // EndFrameBufferRender();

            if (this.gfOptionsScreenExit)
            {
                this.ExitOptionsScreen();
                this.gfOptionsScreenExit = false;
                this.gfOptionsScreenEntry = true;

                ClockManager.UnPauseGame();
            }

            return ValueTask.FromResult(this.guiOptionsScreen);
        }

        private void ExitOptionsScreen()
        {
        }

        private void HandleOptionsScreen()
        {
            this.HandleSliderBarMovementSounds();
            this.HandleHighLightedText(true);
        }

        static uint uiLastSoundFxTime = 0;
        static uint uiLastSpeechTime = 0;
        static uint uiLastPlayingSpeechID = SoundManager.NO_SAMPLE;
        static uint uiLastPlayingSoundID = SoundManager.NO_SAMPLE;
        private void HandleSliderBarMovementSounds()
        {
            long uiCurTime = Globals.GetJA2Clock();

            if ((uiLastSoundFxTime - OPT_MUSIC_SLIDER_PLAY_SOUND_DELAY) > this.guiSoundFxSliderMoving)
            {
                this.guiSoundFxSliderMoving = 0xffffffff;

                //The slider has stopped moving, reset the ambient sector sounds ( so it will change the volume )
                //if (!DidGameJustStart())
                {
                    //HandleNewSectorAmbience(gTilesets[giCurrentTilesetID].ubAmbientID);
                }

                if (!this.sound.SoundIsPlaying(uiLastPlayingSoundID))
                {
                    //uiLastPlayingSoundID = PlayJA2SampleFromFile("Sounds\\Weapons\\LMG Reload.wav", RATE_11025, HIGHVOLUME, 1, MIDDLEPAN);
                }
            }
            else
            {
                uiLastSoundFxTime = Globals.GetJA2Clock();
            }

            if ((uiLastSpeechTime - OPT_MUSIC_SLIDER_PLAY_SOUND_DELAY) > this.guiSpeechSliderMoving)
            {
                this.guiSpeechSliderMoving = 0xffffffff;

                if (!this.sound.SoundIsPlaying(uiLastPlayingSpeechID))
                {
                    //uiLastPlayingSpeechID = PlayJA2GapSample("BattleSnds\\m_cool.wav", RATE_11025, HIGHVOLUME, 1, MIDDLEPAN, null);
                }
            }
            else
            {
                uiLastSpeechTime = Globals.GetJA2Clock();
            }
        }

        private void GetOptionsScreenUserInput()
        {
        }

        private void RenderOptionsScreen()
        {
            HVOBJECT hPixHandle;
            int usPosY;
            TOPTION cnt;
            int usWidth = 0;

            //Get and display the background image
            hPixHandle = VeldridVideoManager.GetVideoObject(this.guiOptionBackGroundImageKey);
            VeldridVideoManager.BltVideoObject(hPixHandle, 0, 0, 0, 0);

            //Get and display the titla image
            hPixHandle = VeldridVideoManager.GetVideoObject(this.guiOptionsAddOnImagesKey);
            VeldridVideoManager.BltVideoObject(hPixHandle, 0, 0, 0, 0);
            VeldridVideoManager.BltVideoObject(hPixHandle, 1, 0, 434, 0);

            //
            // Text for the toggle boxes
            //

            usPosY = OPT_TOGGLE_BOX_FIRST_COLUMN_START_Y - 15;

            // Display the First column of toggles
            for (cnt = 0; cnt < this.gubFirstColOfOptions; cnt++)
            {
                usWidth = FontSubSystem.StringPixLength(EnglishText.zOptionsToggleText[(int)cnt], OPT_MAIN_FONT);

                //if the string is going to wrap, move the string up a bit
                if (usWidth > OPT_TOGGLE_BOX_TEXT_WIDTH)
                {
                    FontSubSystem.DisplayWrappedString(
                        new(OPT_TOGGLE_BOX_FIRST_COL_TEXT_X, usPosY - 5),
                        OPT_TOGGLE_BOX_TEXT_WIDTH,
                        2,
                        OPT_MAIN_FONT,
                        OPT_MAIN_COLOR,
                        EnglishText.zOptionsToggleText[(int)cnt],
                        FontColor.FONT_MCOLOR_BLACK,
                        TextJustifies.LEFT_JUSTIFIED);
                }
                else
                {
                    FontSubSystem.DrawTextToScreen(
                        EnglishText.zOptionsToggleText[(int)cnt],
                        new(OPT_TOGGLE_BOX_FIRST_COL_TEXT_X, usPosY),
                        0,
                        OPT_MAIN_FONT,
                        OPT_MAIN_COLOR,
                        FontColor.FONT_MCOLOR_BLACK,
                        TextJustifies.LEFT_JUSTIFIED);
                }

                usPosY += OPT_GAP_BETWEEN_TOGGLE_BOXES;
            }

            usPosY = OPT_TOGGLE_BOX_SECOND_COLUMN_START_Y - 15;
            //Display the 2nd column of toggles
            for (cnt = this.gubFirstColOfOptions; cnt < TOPTION.NUM_GAME_OPTIONS; cnt++)
            {
                usWidth = FontSubSystem.StringPixLength(EnglishText.zOptionsToggleText[(int)cnt], OPT_MAIN_FONT);

                //if the string is going to wrap, move the string up a bit
                if (usWidth > OPT_TOGGLE_BOX_TEXT_WIDTH)
                {
                    FontSubSystem.DisplayWrappedString(
                        new(OPT_TOGGLE_BOX_SECOND_TEXT_X, usPosY - 5),
                        OPT_TOGGLE_BOX_TEXT_WIDTH,
                        2,
                        OPT_MAIN_FONT,
                        OPT_MAIN_COLOR,
                        EnglishText.zOptionsToggleText[(int)cnt],
                        FontColor.FONT_MCOLOR_BLACK,
                        TextJustifies.LEFT_JUSTIFIED);
                }
                else
                {
                    FontSubSystem.DrawTextToScreen(EnglishText.zOptionsToggleText[(int)cnt], new(OPT_TOGGLE_BOX_SECOND_TEXT_X, usPosY), 0, OPT_MAIN_FONT, OPT_MAIN_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
                }

                usPosY += OPT_GAP_BETWEEN_TOGGLE_BOXES;
            }

            //
            // Text for the Slider Bars
            //

            //Display the Sound Fx text
            FontSubSystem.DisplayWrappedString(new(OPT_SOUND_FX_TEXT_X, OPT_SOUND_FX_TEXT_Y), OPT_SLIDER_TEXT_WIDTH, 2, OPT_MAIN_FONT, OPT_MAIN_COLOR, EnglishText.zOptionsText[OptionsText.OPT_SOUND_FX], FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);

            //Display the Speech text
            FontSubSystem.DisplayWrappedString(new(OPT_SPEECH_TEXT_X, OPT_SPEECH_TEXT_Y), OPT_SLIDER_TEXT_WIDTH, 2, OPT_MAIN_FONT, OPT_MAIN_COLOR, EnglishText.zOptionsText[OptionsText.OPT_SPEECH], FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);

            //Display the Music text
            FontSubSystem.DisplayWrappedString(new(OPT_MUSIC_TEXT_X, OPT_MUSIC_TEXT_Y), OPT_SLIDER_TEXT_WIDTH, 2, OPT_MAIN_FONT, OPT_MAIN_COLOR, EnglishText.zOptionsText[OptionsText.OPT_MUSIC], FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);

            VeldridVideoManager.InvalidateRegion(new(OPTIONS__TOP_LEFT_X, OPTIONS__TOP_LEFT_Y, OPTIONS__BOTTOM_RIGHT_X, OPTIONS__BOTTOM_RIGHT_Y));
        }

        private void EnterOptionsScreen()
        {
            int usPosY;
            Size TextSize = new();

            this.guiOptionsScreen = ScreenName.OPTIONS_SCREEN;

            //Init the slider bar;
            GuiManager.Sliders.InitSliderSystem();

            if (this.gfExitOptionsDueToMessageBox)
            {
                this.gfRedrawOptionsScreen = true;
                this.gfExitOptionsDueToMessageBox = false;
                return;
            }

            this.gfExitOptionsDueToMessageBox = false;

            // load the options screen background graphic and add it
            VeldridVideoManager.AddVideoObject("INTERFACE\\OptionScreenBase.sti", out this.guiOptionBackGroundImageKey);

            // load button, title graphic and add it
            VeldridVideoManager.AddVideoObject("INTERFACE\\optionscreenaddons.sti", out this.guiOptionsAddOnImagesKey);

            //Save game button
            this.giOptionsButtonImages = ButtonSubSystem.LoadButtonImage("INTERFACE\\OptionScreenAddons.sti", -1, 2, -1, 3, -1);

            this.guiOptGotoSaveGameBtn = ButtonSubSystem.CreateIconAndTextButton(
                this.giOptionsButtonImages,
                EnglishText.zOptionsText[OptionsText.OPT_SAVE_GAME],
                OPT_BUTTON_FONT,
                OPT_BUTTON_ON_COLOR,
                FontShadow.DEFAULT_SHADOW,
                OPT_BUTTON_OFF_COLOR,
                FontShadow.DEFAULT_SHADOW,
                ButtonTextJustifies.BUTTON_TEXT_CENTER,
                new(OPT_SAVE_BTN_X, OPT_SAVE_BTN_Y),
                ButtonFlags.BUTTON_TOGGLE,
                MSYS_PRIORITY.HIGH,
                MouseSubSystem.DefaultMoveCallback,
                this.BtnOptGotoSaveGameCallback);

            this.buttonList.Add(guiOptGotoSaveGameBtn);

            ButtonSubSystem.SpecifyDisabledButtonStyle(this.guiOptGotoSaveGameBtn, DISABLED_STYLE.HATCHED);
            if (this.guiPreviousOptionScreen == ScreenName.MAINMENU_SCREEN)// || !CanGameBeSaved())
            {
                ButtonSubSystem.DisableButton(this.guiOptGotoSaveGameBtn);
            }

            //Load game button
            this.giGotoLoadBtnImage = ButtonSubSystem.UseLoadedButtonImage(this.giOptionsButtonImages, -1, 2, -1, 3, -1);
            this.guiOptGotoLoadGameBtn = ButtonSubSystem.CreateIconAndTextButton(
                this.giGotoLoadBtnImage,
                EnglishText.zOptionsText[OptionsText.OPT_LOAD_GAME],
                OPT_BUTTON_FONT,
                OPT_BUTTON_ON_COLOR,
                FontShadow.DEFAULT_SHADOW,
                OPT_BUTTON_OFF_COLOR,
                FontShadow.DEFAULT_SHADOW,
                ButtonTextJustifies.TEXT_CJUSTIFIED,
                new(OPT_LOAD_BTN_X, OPT_LOAD_BTN_Y),
                ButtonFlags.BUTTON_TOGGLE,
                MSYS_PRIORITY.HIGH,
                MouseSubSystem.DefaultMoveCallback,
                this.BtnOptGotoLoadGameCallback);

            this.buttonList.Add(this.guiOptGotoLoadGameBtn);
            
            //	SpecifyDisabledButtonStyle( guiBobbyRAcceptOrder, DISABLED_STYLE_SHADED );

            //Quit to main menu button
            this.giQuitBtnImage = ButtonSubSystem.UseLoadedButtonImage(this.giOptionsButtonImages, -1, 2, -1, 3, -1);
            this.guiQuitButton = ButtonSubSystem.CreateIconAndTextButton(
                this.giQuitBtnImage,
                EnglishText.zOptionsText[OptionsText.OPT_MAIN_MENU],
                OPT_BUTTON_FONT,
                OPT_BUTTON_ON_COLOR,
                FontShadow.DEFAULT_SHADOW,
                OPT_BUTTON_OFF_COLOR,
                FontShadow.DEFAULT_SHADOW,
                ButtonTextJustifies.TEXT_CJUSTIFIED,
                new(OPT_QUIT_BTN_X, OPT_QUIT_BTN_Y),
                ButtonFlags.BUTTON_TOGGLE,
                MSYS_PRIORITY.HIGH,
                MouseSubSystem.DefaultMoveCallback,
                this.BtnOptQuitCallback);

            this.buttonList.Add(this.guiQuitButton);

            ButtonSubSystem.SpecifyDisabledButtonStyle(this.guiQuitButton, DISABLED_STYLE.HATCHED);
            //	DisableButton( guiQuitButton );

            //Done button

            this.giDoneBtnImage = ButtonSubSystem.UseLoadedButtonImage(this.giOptionsButtonImages, -1, 2, -1, 3, -1);
            this.guiDoneButton = ButtonSubSystem.CreateIconAndTextButton(
                this.giDoneBtnImage,
                EnglishText.zOptionsText[OptionsText.OPT_DONE],
                OPT_BUTTON_FONT,
                OPT_BUTTON_ON_COLOR,
                FontShadow.DEFAULT_SHADOW,
                OPT_BUTTON_OFF_COLOR,
                FontShadow.DEFAULT_SHADOW,
                ButtonTextJustifies.BUTTON_TEXT_CENTER,
                new(OPT_DONE_BTN_X, OPT_DONE_BTN_Y),
                ButtonFlags.BUTTON_TOGGLE,
                MSYS_PRIORITY.HIGH,
                MouseSubSystem.DefaultMoveCallback,
                this.BtnDoneCallback);

            this.buttonList.Add(this.guiDoneButton);

            //	SpecifyDisabledButtonStyle( guiBobbyRAcceptOrder, DISABLED_STYLE_SHADED );

            //
            // Toggle Boxes
            //
            TextSize.Height = FontSubSystem.GetFontHeight(OPT_MAIN_FONT);

            //Create the first column of check boxes
            usPosY = OPT_TOGGLE_BOX_FIRST_COLUMN_START_Y;
            this.gubFirstColOfOptions = (TOPTION)OPT_FIRST_COLUMN_TOGGLE_CUT_OFF;
            for (TOPTION cnt = 0; cnt < this.gubFirstColOfOptions; cnt++)
            {
                var option = cnt;

                //Check box to toggle tracking mode
                this.guiOptionsToggles[option] = ButtonSubSystem.CreateCheckBoxButton(
                    new(OPT_TOGGLE_BOX_FIRST_COLUMN_X, usPosY),
                    "INTERFACE\\OptionsCheckBoxes.sti",
                    MSYS_PRIORITY.HIGH + 10,
                    this.BtnOptionsTogglesCallback);

                this.buttonList.Add(this.guiOptionsToggles[option]);

                ButtonSubSystem.SetButtonUserData(this.guiOptionsToggles[option], 0, option);

                TextSize.Width = FontSubSystem.StringPixLength(EnglishText.zOptionsToggleText[(int)cnt], OPT_MAIN_FONT);

                if (TextSize.Width > OPT_TOGGLE_BOX_TEXT_WIDTH)
                {
                    //Get how many lines will be used to display the string, without displaying the string
                    int ubNumLines = FontSubSystem.DisplayWrappedString(
                        new(0, 0),
                        OPT_TOGGLE_BOX_TEXT_WIDTH,
                        2,
                        OPT_MAIN_FONT,
                        OPT_HIGHLIGHT_COLOR,
                        EnglishText.zOptionsToggleText[(int)cnt],
                        FontColor.FONT_MCOLOR_BLACK,
                        (TextJustifies)((int)((int)ButtonTextJustifies.BUTTON_TEXT_LEFT | FontSubSystem.DONT_DISPLAY_TEXT) / FontSubSystem.GetFontHeight(OPT_MAIN_FONT)));

                    TextSize.Width = OPT_TOGGLE_BOX_TEXT_WIDTH;

                    //Create mouse regions for the option toggle text
                    MouseSubSystem.MSYS_DefineRegion(
                        this.gSelectedOptionTextRegion[cnt],
                        new Rectangle(
                            OPT_TOGGLE_BOX_FIRST_COLUMN_X,
                            480 - usPosY,
                            OPT_TOGGLE_BOX_FIRST_COL_TEXT_X,
                            TextSize.Height),
                        MSYS_PRIORITY.HIGH,
                        CURSOR.NORMAL,
                        this.SelectedOptionTextRegionMovementCallBack,
                        this.SelectedOptionTextRegionCallBack);

                    MouseSubSystem.SetRegionUserData(
                        this.gSelectedOptionTextRegion[cnt],
                        0,
                        this.guiOptionsToggles[option]);
                }
                else
                {
                    //Create mouse regions for the option toggle text
                    MouseSubSystem.MSYS_DefineRegion(
                        this.gSelectedOptionTextRegion[cnt],
                        new Rectangle(
                            OPT_TOGGLE_BOX_FIRST_COLUMN_X,
                            480 - usPosY,
                            OPT_TOGGLE_BOX_SECOND_TEXT_X,
                            TextSize.Height),
                        MSYS_PRIORITY.HIGH,
                        CURSOR.NORMAL,
                        this.SelectedOptionTextRegionMovementCallBack,
                        this.SelectedOptionTextRegionCallBack);

                    var textRegion = this.gSelectedOptionTextRegion[cnt];
                    MouseSubSystem.SetRegionUserData(ref textRegion, 0, this.guiOptionsToggles[option]);
                }

                MouseSubSystem.SetRegionFastHelpText(this.gSelectedOptionTextRegion[option], EnglishText.zOptionsScreenHelpText[cnt]);
                ButtonSubSystem.SetButtonFastHelpText(this.guiOptionsToggles[option], EnglishText.zOptionsScreenHelpText[cnt]);

                usPosY += OPT_GAP_BETWEEN_TOGGLE_BOXES;
            }

            //Create the 2nd column of check boxes
            usPosY = OPT_TOGGLE_BOX_FIRST_COLUMN_START_Y;
            for (TOPTION cnt = this.gubFirstColOfOptions; cnt < TOPTION.NUM_GAME_OPTIONS; cnt++)
            {
                var option = (TOPTION)cnt;

                //Check box to toggle tracking mode
                this.guiOptionsToggles[option] = ButtonSubSystem.CreateCheckBoxButton(
                    new(OPT_TOGGLE_BOX_SECOND_COLUMN_X, usPosY),
                    "INTERFACE\\OptionsCheckBoxes.sti",
                    MSYS_PRIORITY.HIGH + 10,
                    this.BtnOptionsTogglesCallback);

                this.buttonList.Add(this.guiOptionsToggles[option]);

                ButtonSubSystem.SetButtonUserData(this.guiOptionsToggles[option], 0, option);


                //
                // Create mouse regions for the option toggle text
                //


                TextSize.Width = FontSubSystem.StringPixLength(EnglishText.zOptionsToggleText[(int)cnt], OPT_MAIN_FONT);

                if (TextSize.Width > OPT_TOGGLE_BOX_TEXT_WIDTH)
                {
                    //Get how many lines will be used to display the string, without displaying the string
                    // int ubNumLines = FontSubSystem.DisplayWrappedString(
                    //     new(0, 0),
                    //     OPT_TOGGLE_BOX_TEXT_WIDTH,
                    //     2,
                    //     OPT_MAIN_FONT,
                    //     OPT_HIGHLIGHT_COLOR,
                    //     EnglishText.zOptionsToggleText[(int)cnt],
                    //     FontColor.FONT_MCOLOR_BLACK,
                    //     (TextJustifies)((int)((int)ButtonTextJustifies.BUTTON_TEXT_LEFT | FontSubSystem.DONT_DISPLAY_TEXT) / FontSubSystem.GetFontHeight(OPT_MAIN_FONT)));

                    FontSubSystem.DrawTextToScreen(
                        EnglishText.zOptionsToggleText[(int)cnt],
                        new(0, 0),
                        OPT_TOGGLE_BOX_TEXT_WIDTH,
                        OPT_MAIN_FONT,
                        OPT_HIGHLIGHT_COLOR,
                        FontColor.FONT_MCOLOR_BLACK,
                        TextJustifies.LEFT_JUSTIFIED);

                    TextSize.Width = OPT_TOGGLE_BOX_TEXT_WIDTH;

                    MouseSubSystem.MSYS_DefineRegion(
                        this.gSelectedOptionTextRegion[cnt],
                        new(
                            OPT_TOGGLE_BOX_SECOND_COLUMN_X + 13,
                            480 - usPosY,
                            OPT_TOGGLE_BOX_SECOND_TEXT_X + TextSize.Width,
                            TextSize.Height),
                        MSYS_PRIORITY.HIGH,
                        CURSOR.NORMAL,
                        this.SelectedOptionTextRegionMovementCallBack,
                        this.SelectedOptionTextRegionCallBack);

                    MouseSubSystem.SetRegionUserData(this.gSelectedOptionTextRegion[cnt], 0, this.guiOptionsToggles[option]);
                }
                else
                {
                    MouseSubSystem.MSYS_DefineRegion(
                        this.gSelectedOptionTextRegion[option],
                        new(
                            OPT_TOGGLE_BOX_SECOND_COLUMN_X + 13,
                            480 - usPosY,
                            OPT_TOGGLE_BOX_SECOND_TEXT_X + TextSize.Width,
                            TextSize.Height),
                        MSYS_PRIORITY.HIGH,
                        CURSOR.NORMAL,
                        this.SelectedOptionTextRegionMovementCallBack,
                        this.SelectedOptionTextRegionCallBack);

                    MouseSubSystem.SetRegionUserData(this.gSelectedOptionTextRegion[option], 0, this.guiOptionsToggles[option]);
                }

                MouseSubSystem.SetRegionFastHelpText(this.gSelectedOptionTextRegion[option], EnglishText.zOptionsScreenHelpText[cnt]);
                ButtonSubSystem.SetButtonFastHelpText(this.guiOptionsToggles[option], EnglishText.zOptionsScreenHelpText[cnt]);

                usPosY += OPT_GAP_BETWEEN_TOGGLE_BOXES;
            }

            //Create a mouse region so when the user leaves a togglebox text region we can detect it then unselect the region
            MouseSubSystem.MSYS_DefineRegion(
                ref this.gSelectedToggleBoxAreaRegion,
                new Rectangle(0, 0, 640, 480),
                MSYS_PRIORITY.NORMAL,
                CURSOR.NORMAL,
                this.SelectedToggleBoxAreaRegionMovementCallBack,
                null);

            //Render the scene before adding the slider boxes
            this.RenderOptionsScreen();

            //Add a slider bar for the Sound Effects 
            this.guiSoundEffectsSlider = GuiManager.Sliders.AddSlider(
                SliderStyle.SLIDER_VERTICAL_STEEL,
                CURSOR.NORMAL,
                new(OPT_SOUND_EFFECTS_SLIDER_X, OPT_SOUND_EFFECTS_SLIDER_Y),
                OPT_SLIDER_BAR_SIZE,
                127,
                MSYS_PRIORITY.HIGH,
                this.SoundFXSliderChangeCallBack,
                0);

            // AssertMsg(guiSoundEffectsSliderID, "Failed to AddSlider");
            GuiManager.Sliders.SetSliderValue(ref this.guiSoundEffectsSlider, this.sound.GetSoundEffectsVolume());

            //Add a slider bar for the Speech
            this.guiSpeechSlider = GuiManager.Sliders.AddSlider(
                SliderStyle.SLIDER_VERTICAL_STEEL,
                CURSOR.NORMAL,
                new(OPT_SPEECH_SLIDER_X, OPT_SPEECH_SLIDER_Y),
                OPT_SLIDER_BAR_SIZE,
                127,
                MSYS_PRIORITY.HIGH,
                this.SpeechSliderChangeCallBack,
                0);

            // AssertMsg(guiSpeechSliderID, "Failed to AddSlider");
            GuiManager.Sliders.SetSliderValue(ref this.guiSpeechSlider, this.sound.GetSpeechVolume());

            //Add a slider bar for the Music
            this.guiMusicSlider = GuiManager.Sliders.AddSlider(
                SliderStyle.SLIDER_VERTICAL_STEEL,
                CURSOR.NORMAL,
                new(OPT_MUSIC_SLIDER_X, OPT_MUSIC_SLIDER_Y),
                OPT_SLIDER_BAR_SIZE,
                127,
                MSYS_PRIORITY.HIGH,
                this.MusicSliderChangeCallBack,
                0);

            // AssertMsg(guiMusicSliderID, "Failed to AddSlider");
            GuiManager.Sliders.SetSliderValue(ref this.guiMusicSlider, this.music.MusicGetVolume());

            //Remove the mouse region over the clock
            ClockManager.RemoveMouseRegionForPauseOfClock();

            //Draw the screen
            this.gfRedrawOptionsScreen = true;

            //Set the option screen toggle boxes
            this.SetOptionsScreenToggleBoxes();

            Messages.DisableScrollMessages();

            //reset
            this.gbHighLightedOptionText = -1;

            //get the status of the tree top option

            this.gfSettingOfTreeTopStatusOnEnterOfOptionScreen = this.settings[TOPTION.TOGGLE_TREE_TOPS];

            //Get the status of the item glow option
            this.gfSettingOfItemGlowStatusOnEnterOfOptionScreen = this.settings[TOPTION.GLOW_ITEMS];

            this.gfSettingOfDontAnimateSmoke = this.settings[TOPTION.ANIMATE_SMOKE];
        }

        private void SetOptionsScreenToggleBoxes()
        {
            TOPTION cnt;

            for (cnt = 0; cnt < TOPTION.NUM_GAME_OPTIONS; cnt++)
            {
                if (this.settings[cnt])
                {
                    this.guiOptionsToggles[cnt].uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
                }
                else
                {
                    this.guiOptionsToggles[cnt].uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);
                }
            }
        }

        private void SelectedToggleBoxAreaRegionMovementCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON reason)
        {
            if (reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
            {
            }
            else if (reason.HasFlag(MSYS_CALLBACK_REASON.GAIN_MOUSE))
            {
                TOPTION ubCnt;

                //loop through all the toggle box's and remove the in area flag
                for (ubCnt = 0; ubCnt < TOPTION.NUM_GAME_OPTIONS; ubCnt++)
                {
                    this.guiOptionsToggles[ubCnt].MouseRegion.HasMouse = false;
                }

                this.gbHighLightedOptionText = -1;

                VeldridVideoManager.InvalidateRegion(pRegion.Bounds);
            }
        }

        private void SpeechSliderChangeCallBack(int iNewValue)
        {
            this.sound.SetSpeechVolume(iNewValue);

            this.guiSpeechSliderMoving = Globals.GetJA2Clock();
        }

        private void MusicSliderChangeCallBack(int iNewValue)
        {
            this.music.MusicSetVolume(iNewValue);
        }

        private void SelectedOptionTextRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
        {
            TOPTION ubButton = (TOPTION)MouseSubSystem.GetRegionUserData(ref pRegion, 0);

            if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
            {
                this.HandleOptionToggle(ubButton, !this.settings[ubButton], false, true);

                VeldridVideoManager.InvalidateRegion(pRegion.Bounds);
            }
            else if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))//iReason & MSYS_CALLBACK_REASON_LBUTTON_REPEAT || 
            {
                if (this.settings[ubButton])
                {
                    this.HandleOptionToggle(ubButton, true, true, true);
                }
                else
                {
                    this.HandleOptionToggle(ubButton, false, true, true);
                }
            }
        }

        private void SelectedOptionTextRegionMovementCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON reason)
        {
            var bButton = (GUI_BUTTON)MouseSubSystem.GetRegionUserData(ref pRegion, 0);

            if (reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
            {
                this.HandleHighLightedText(false);

                this.gbHighLightedOptionText = -1;

                VeldridVideoManager.InvalidateRegion(pRegion.Bounds);
            }
            else if (reason.HasFlag(MSYS_CALLBACK_REASON.GAIN_MOUSE))
            {
                //                this.gbHighLightedOptionText = bButton;

                VeldridVideoManager.InvalidateRegion(pRegion.Bounds);
            }
        }

        static int bLastRegion = -1;
        private void HandleHighLightedText(bool fHighLight)
        {
            Point Pos = new(0, 0);

            TOPTION ubCnt;
            int bHighLight = -1;
            int usWidth;

            if (this.gbHighLightedOptionText == -1)
            {
                fHighLight = false;
            }

            //if the user has the mouse in one of the checkboxes
            for (ubCnt = 0; ubCnt < TOPTION.NUM_GAME_OPTIONS; ubCnt++)
            {
                if (this.guiOptionsToggles[ubCnt].MouseRegion.HasMouse)
                {
                    this.gbHighLightedOptionText = (int)ubCnt;
                    fHighLight = true;
                }
            }

            // If there is a valid section being highlighted
            if (this.gbHighLightedOptionText != -1)
            {
                bLastRegion = this.gbHighLightedOptionText;
            }

            bHighLight = this.gbHighLightedOptionText;

            if (bLastRegion != -1 && this.gbHighLightedOptionText == -1)
            {
                fHighLight = false;
                bHighLight = bLastRegion;
                bLastRegion = -1;
            }

            if (bHighLight != -1)
            {
                if (bHighLight < OPT_FIRST_COLUMN_TOGGLE_CUT_OFF)
                {
                    Pos = new(
                        OPT_TOGGLE_BOX_FIRST_COL_TEXT_X,
                        OPT_TOGGLE_BOX_FIRST_COLUMN_START_Y - 15 + (bHighLight * OPT_GAP_BETWEEN_TOGGLE_BOXES));
                }
                else
                {
                    Pos = new(
                        OPT_TOGGLE_BOX_SECOND_TEXT_X,
                        OPT_TOGGLE_BOX_SECOND_COLUMN_START_Y - 15 + ((bHighLight - OPT_FIRST_COLUMN_TOGGLE_CUT_OFF) * OPT_GAP_BETWEEN_TOGGLE_BOXES));
                }


                usWidth = FontSubSystem.StringPixLength(EnglishText.zOptionsToggleText[bHighLight], OPT_MAIN_FONT);

                //if the string is going to wrap, move the string up a bit
                if (usWidth > OPT_TOGGLE_BOX_TEXT_WIDTH)
                {
                    if (fHighLight)
                    {
                        FontSubSystem.DisplayWrappedString(Pos, OPT_TOGGLE_BOX_TEXT_WIDTH, 2, OPT_MAIN_FONT, OPT_HIGHLIGHT_COLOR, EnglishText.zOptionsToggleText[bHighLight], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
                    }
                    else
                    {
                        FontSubSystem.DisplayWrappedString(Pos, OPT_TOGGLE_BOX_TEXT_WIDTH, 2, OPT_MAIN_FONT, OPT_MAIN_COLOR, EnglishText.zOptionsToggleText[bHighLight], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
                    }
                }
                else
                {
                    if (fHighLight)
                    {
                        FontSubSystem.DrawTextToScreen(EnglishText.zOptionsToggleText[bHighLight], Pos, 0, OPT_MAIN_FONT, OPT_HIGHLIGHT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
                    }
                    else
                    {
                        FontSubSystem.DrawTextToScreen(EnglishText.zOptionsToggleText[bHighLight], Pos, 0, OPT_MAIN_FONT, OPT_MAIN_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
                    }
                }
            }
        }

        private void BtnOptGotoLoadGameCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
        {
            if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
            {
                btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
                VeldridVideoManager.InvalidateRegion(btn.MouseRegion.Bounds);
            }

            if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
            {
                btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);

                this.SetOptionsExitScreen(ScreenName.SAVE_LOAD_SCREEN);
                // gfSaveGame = false;

                VeldridVideoManager.InvalidateRegion(btn.MouseRegion.Bounds);
            }

            if (reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
            {
                btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);
                VeldridVideoManager.InvalidateRegion(btn.MouseRegion.Bounds);
            }
        }

        private void BtnOptQuitCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
        {
            if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
            {
                btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
                VeldridVideoManager.InvalidateRegion(btn.MouseRegion.Bounds);
            }

            if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
            {
                btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);

                //Confirm the Exit to the main menu screen
                DoOptionsMessageBox(MessageBoxStyle.MSG_BOX_BASIC_STYLE, EnglishText.zOptionsText[OptionsText.OPT_RETURN_TO_MAIN], ScreenName.OPTIONS_SCREEN, MessageBoxFlags.MSG_BOX_FLAG_YESNO, ConfirmQuitToMainMenuMessageBoxCallBack);

                ///		SetOptionsExitScreen( MAINMENU_SCREEN );

                VeldridVideoManager.InvalidateRegion(btn.MouseRegion.Bounds);
            }

            if (reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
            {
                btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);
                VeldridVideoManager.InvalidateRegion(btn.MouseRegion.Bounds);
            }
        }

        private void ConfirmQuitToMainMenuMessageBoxCallBack(MessageBoxReturnCode bExitValue)
        {
            // yes, Quit to main menu
            if (bExitValue == MessageBoxReturnCode.MSG_BOX_RETURN_YES)
            {
                this.gfEnteredFromMapScreen = false;
                this.gfExitOptionsAfterMessageBox = true;
                this.SetOptionsExitScreen(ScreenName.MAINMENU_SCREEN);

                //We want to reinitialize the game
                this.gameInit.ReStartingGame();
            }
            else
            {
                this.gfExitOptionsAfterMessageBox = false;
                this.gfExitOptionsDueToMessageBox = false;
            }
        }

        private void BtnDoneCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
        {
            if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
            {
                btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
                VeldridVideoManager.InvalidateRegion(btn.MouseRegion.Bounds);
            }

            if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
            {
                btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);

                this.SetOptionsExitScreen(this.guiPreviousOptionScreen);

                VeldridVideoManager.InvalidateRegion(btn.MouseRegion.Bounds);
            }

            if (reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
            {
                btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);
                VeldridVideoManager.InvalidateRegion(btn.MouseRegion.Bounds);
            }
        }

        private void SetOptionsExitScreen(ScreenName uiExitScreen)
        {
            this.guiOptionsScreen = uiExitScreen;
            this.gfOptionsScreenExit = true;
        }

        private void BtnOptGotoSaveGameCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
        {
            if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
            {
                btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
                VeldridVideoManager.InvalidateRegion(btn.MouseRegion.Bounds);
            }

            if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
            {
                btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);

                this.SetOptionsExitScreen(ScreenName.SAVE_LOAD_SCREEN);
                //gfSaveGame = true;

                VeldridVideoManager.InvalidateRegion(btn.MouseRegion.Bounds);
            }

            if (reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
            {
                btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);
                VeldridVideoManager.InvalidateRegion(btn.MouseRegion.Bounds);
            }
        }

        private static uint uiOptionToggleSound = SoundManager.NO_SAMPLE;
        private MOUSE_REGION gSelectedToggleBoxAreaRegion = new(nameof(gSelectedToggleBoxAreaRegion));
        private uint guiSpeechSliderMoving;

        private void HandleOptionToggle(TOPTION ubButton, bool fState, bool fDown, bool fPlaySound)
        {
            SoundPans uiSideToPlaySoundOn = SoundPans.MIDDLEPAN;
            //	static	bool	fCheckBoxDrawnDownLastTime = false;

            if (fState)
            {
                this.settings[ubButton] = true;

                this.guiOptionsToggles[ubButton].uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;

                if (fDown)
                {
                    ButtonSubSystem.DrawCheckBoxButtonOn(this.guiOptionsToggles[ubButton]);
                }
            }
            else
            {
                this.settings[ubButton] = false;

                this.guiOptionsToggles[ubButton].uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;

                if (fDown)
                {
                    ButtonSubSystem.DrawCheckBoxButtonOff(this.guiOptionsToggles[ubButton]);
                }

                //check to see if the user is unselecting either the spech or subtitles toggle
                if (ubButton == TOPTION.SPEECH || ubButton == TOPTION.SUBTITLES)
                {
                    //make sure that at least of of the toggles is still enabled
                    if (!(this.guiOptionsToggles[TOPTION.SPEECH].uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON)))
                    {
                        if (!(this.guiOptionsToggles[TOPTION.SUBTITLES].uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON)))
                        {
                            this.settings[ubButton] = true;
                            this.guiOptionsToggles[ubButton].uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;

                            //Confirm the Exit to the main menu screen
                            this.DoOptionsMessageBox(MessageBoxStyle.MSG_BOX_BASIC_STYLE, EnglishText.zOptionsText[OptionsText.OPT_NEED_AT_LEAST_SPEECH_OR_SUBTITLE_OPTION_ON], ScreenName.OPTIONS_SCREEN, MessageBoxFlags.MSG_BOX_FLAG_OK, null);
                            this.gfExitOptionsDueToMessageBox = false;
                        }
                    }
                }
            }

            //stop the sound if
            //	if( SoundIsPlaying( uiOptionToggleSound ) && !fDown )
            {
                this.sound.SoundStop(uiOptionToggleSound);
            }


            if (fPlaySound)
            {
                if (fDown)
                {
                    //				case BTN_SND_CLICK_OFF:
                    // PlayJA2Sample(BIG_SWITCH3_IN, RATE_11025, BTNVOLUME, 1, MIDDLEPAN);
                }
                else
                {
                    //		case BTN_SND_CLICK_ON:
                    // PlayJA2Sample(BIG_SWITCH3_OUT, RATE_11025, BTNVOLUME, 1, MIDDLEPAN);
                }
            }
        }

        private bool DoOptionsMessageBox(
            MessageBoxStyle ubStyle, 
            string zString, 
            ScreenName uiExitScreen, 
            MessageBoxFlags usFlags, 
            MSGBOX_CALLBACK ReturnCallback)
        {
            Rectangle? CenteringRect = new(0, 0, 639, 479);

            // reset exit mode
            gfExitOptionsDueToMessageBox = true;

            // do message box and return
            this.giOptionsMessageBox = this.messageBox.DoMessageBox(ubStyle, zString, uiExitScreen, (usFlags | MessageBoxFlags.MSG_BOX_FLAG_USE_CENTERING_RECT), ReturnCallback, ref CenteringRect);

            // send back return state
            return ((giOptionsMessageBox != -1));
        }

        private void BtnOptionsTogglesCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
        {
            var ubButton = (TOPTION)ButtonSubSystem.MSYS_GetBtnUserData(btn, 0);

            if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
            {
                if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
                {
                    this.HandleOptionToggle(ubButton, true, false, false);

                    //			gGameSettings.fOptions[ ubButton ] = true;
                    btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
                }
                else
                {
                    btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;

                    this.HandleOptionToggle(ubButton, false, false, false);
                }
            }
            else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
            {
                if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
                {
                    this.HandleOptionToggle(ubButton, true, true, false);

                    btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
                }
                else
                {
                    btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;

                    this.HandleOptionToggle(ubButton, false, true, false);
                }
            }
        }

        private void SoundFXSliderChangeCallBack(int iNewValue)
        {
            this.sound.SetSoundEffectsVolume(iNewValue);

            this.guiSoundFxSliderMoving = Globals.GetJA2Clock();
        }

        public ValueTask<bool> Initialize()
        {
            //Set so next time we come in, we can set up
            this.gfOptionsScreenEntry = true;

            return ValueTask.FromResult(true);
        }

        public void Dispose()
        {
        }

        public ValueTask Deactivate()
        {
            return ValueTask.CompletedTask;
        }
    }

    // defines used for the zOptionsText
    public enum OptionsText
    {
        OPT_SAVE_GAME,
        OPT_LOAD_GAME,
        OPT_MAIN_MENU,
        OPT_DONE,
        OPT_SOUND_FX,
        OPT_SPEECH,
        OPT_MUSIC,
        OPT_RETURN_TO_MAIN,
        OPT_NEED_AT_LEAST_SPEECH_OR_SUBTITLE_OPTION_ON,
    };
}
