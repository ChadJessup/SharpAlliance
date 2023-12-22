using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SixLabors.ImageSharp;
using Rectangle = SixLabors.ImageSharp.Rectangle;
using Point = SixLabors.ImageSharp.Point;
using SharpAlliance.Core.Managers.VideoSurfaces;

using static SharpAlliance.Core.Globals;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace SharpAlliance.Core.Screens;

public class MainMenuScreen : IScreen
{
    private readonly GameInit gameInit;
    private readonly RenderDirty renderDirty;
    private readonly IScreenManager screens;
    private readonly GameOptions options;
    private readonly FontSubSystem fonts;
    private readonly IInputManager input;
    private readonly IMusicManager music;
    private readonly ButtonSubSystem buttons;
    private readonly MouseSubSystem mouse;
    private readonly IVideoManager video;
    private readonly CursorSubSystem cursor;
    private readonly IClockManager clock;

    private Dictionary<MainMenuItems, ButtonPic> iMenuImages = new();
    private Dictionary<MainMenuItems, GUI_BUTTON> iMenuButtons = new();

    private ScreenName guiMainMenuExitScreen = ScreenName.MAINMENU_SCREEN;

    private string mainMenuBackGroundImageKey;
    private string ja2LogoImageKey;

    private bool fInitialRender = false;
    //bool gfDoHelpScreen = 0;

    private IntroScreen introScreen;

    public MainMenuScreen(
        MouseSubSystem mouseSubSystem,
        IScreenManager screenManager,
        GameInit gameInit,
        IClockManager clockManager,
        ButtonSubSystem buttonSubSystem,
        IMusicManager musicManager,
        GameOptions gameOptions,
        IVideoManager videoManager,
        CursorSubSystem cursorSubSystem,
        FontSubSystem fontSubSystem,
        IInputManager inputManager,
        RenderDirty renderDirtySubSystem)
    {
        this.cursor = cursorSubSystem;
        this.clock = clockManager;
        this.input = inputManager;
        this.music = musicManager;
        this.mouse = mouseSubSystem;
        this.options = gameOptions;
        this.buttons = buttonSubSystem;
        this.fonts = fontSubSystem;
        this.screens = screenManager;
        this.video = videoManager;
        this.gameInit = gameInit;
        this.renderDirty = renderDirtySubSystem;
    }

    public bool IsInitialized { get; set; }
    public ScreenState State { get; set; }

    public async ValueTask Activate()
    {
        this.introScreen = await this.screens.GetScreen<IntroScreen>(ScreenName.INTRO_SCREEN, activate: false);
    }

    public async ValueTask<ScreenName> Handle()
    {
        uint cnt;
        uint uiTime;

        if (Globals.guiSplashStartTime + 4000 > Globals.GetJA2Clock())
        {
            CursorSubSystem.SetCurrentCursorFromDatabase(CURSOR.VIDEO_NO_CURSOR);
            this.music.SetMusicMode(MusicMode.NONE);

            //The splash screen hasn't been up long enough yet.
            return ScreenName.MAINMENU_SCREEN;
        }

        if (Globals.guiSplashFrameFade != 0)
        {
            //Fade the splash screen.
            uiTime = Globals.GetJA2Clock();
            if (Globals.guiSplashFrameFade > 2)
            {
                video.ShadowVideoSurfaceRectUsingLowPercentTable(SurfaceType.FRAME_BUFFER, new Rectangle(0, 0, 640, 480));
            }
            else if (Globals.guiSplashFrameFade > 1)
            {
                video.ColorFillVideoSurfaceArea(SurfaceType.FRAME_BUFFER, new Rectangle(0, 0, 640, 480), Color.Black);
            }
            else
            {
                uiTime = Globals.GetJA2Clock();
                //while( GetJA2Clock() < uiTime + 375 );
                this.music.SetMusicMode(MusicMode.MAIN_MENU);
            }

            //while( uiTime + 100 > GetJA2Clock() );

            Globals.guiSplashFrameFade--;

            video.InvalidateScreen();
            video.EndFrameBufferRender();

            CursorSubSystem.SetCurrentCursorFromDatabase(CURSOR.VIDEO_NO_CURSOR);

            return ScreenName.MAINMENU_SCREEN;
        }

        CursorSubSystem.SetCurrentCursorFromDatabase(CURSOR.NORMAL);

        if (Globals.gfMainMenuScreenEntry)
        {
            await this.InitMainMenu();
            Globals.gfMainMenuScreenEntry = false;
            Globals.gfMainMenuScreenExit = false;
            guiMainMenuExitScreen = ScreenName.MAINMENU_SCREEN;
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
            ButtonSubSystem.MarkAButtonDirty(this.iMenuButtons[(MainMenuItems)cnt]);
        }

        ButtonSubSystem.RenderButtons(this.iMenuButtons.Values);

        video.EndFrameBufferRender();

        this.HandleMainMenuInput();

        this.HandleMainMenuScreen();

        if (Globals.gfMainMenuScreenExit)
        {
            this.ExitMainMenu();
            Globals.gfMainMenuScreenExit = false;
            Globals.gfMainMenuScreenEntry = true;
        }

        if (guiMainMenuExitScreen != ScreenName.MAINMENU_SCREEN)
        {
            Globals.gfMainMenuScreenEntry = true;
        }

        return guiMainMenuExitScreen;
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
        if (Globals.gbHandledMainMenu != MainMenuItems.Unknown)
        {
            // Exit according to handled value!
            switch (Globals.gbHandledMainMenu)
            {
                case MainMenuItems.QUIT:
                    Globals.gfMainMenuScreenExit = true;

                    Globals.gfProgramIsRunning = false;
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
                    Globals.gbHandledMainMenu = 0;
                    // gfSaveGame = false;
                    Globals.gfMainMenuScreenExit = true;

                    break;

                case MainMenuItems.PREFERENCES:
                    //this.optionsScreen.guiPreviousOptionScreen = guiCurrentScreen;
                    guiMainMenuExitScreen = ScreenName.OPTIONS_SCREEN;
                    Globals.gbHandledMainMenu = 0;
                    Globals.gfMainMenuScreenExit = true;
                    break;

                case MainMenuItems.CREDITS:
                    guiMainMenuExitScreen = ScreenName.CREDIT_SCREEN;
                    Globals.gbHandledMainMenu = 0;
                    Globals.gfMainMenuScreenExit = true;
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
        guiMainMenuExitScreen = screen;

        //Remove the background region
        this.CreateDestroyBackGroundMouseMask(false);

        Globals.gfMainMenuScreenExit = true;
    }

    private void RestoreButtonBackGrounds()
    {
        byte cnt;

        //RestoreExternBackgroundRect( (ushort)(320 - gusMainMenuButtonWidths[TITLE]/2), MAINMENU_TITLE_Y, gusMainMenuButtonWidths[TITLE], 23 );


        for (cnt = 0; cnt < (byte)MainMenuItems.NUM_MENU_ITEMS; cnt++)
        {
            RenderDirty.RestoreExternBackgroundRect(
                (ushort)(320 - Globals.gusMainMenuButtonWidths[cnt] / 2),
                (short)(Globals.MAINMENU_Y + (cnt * Globals.MAINMENU_Y_SPACE) - 1),
                (ushort)(Globals.gusMainMenuButtonWidths[cnt] + 1),
                23);
        }
    }

    private PointF copyrightLocation = new(0, 465);
    private void RenderMainMenu()
    {
        //Get and display the background image
        HVOBJECT hPixHandle = video.GetVideoObject(this.mainMenuBackGroundImageKey);
        video.BlitSurfaceToSurface(hPixHandle.Images[0], SurfaceType.SAVE_BUFFER, new(0, 0), VO_BLT.SRCTRANSPARENCY);
        video.BlitSurfaceToSurface(hPixHandle.Images[0], SurfaceType.FRAME_BUFFER, new(0, 0), VO_BLT.SRCTRANSPARENCY);

        hPixHandle = video.GetVideoObject(this.ja2LogoImageKey);
        video.BlitSurfaceToSurface(hPixHandle.Images[0], SurfaceType.FRAME_BUFFER, new(188, 15), VO_BLT.SRCTRANSPARENCY);
        video.BlitSurfaceToSurface(hPixHandle.Images[0], SurfaceType.SAVE_BUFFER, new(188, 15), VO_BLT.SRCTRANSPARENCY);

        FontSubSystem.DrawTextToScreen(
            EnglishText.gzCopyrightText[0],
            copyrightLocation,
            640,
            FontStyle.FONT10ARIAL,
            FontColor.FONT_MCOLOR_WHITE,
            FontColor.FONT_MCOLOR_BLACK,
            TextJustifies.CENTER_JUSTIFIED);

        video.InvalidateRegion(new Rectangle(0, 0, 640, 480));
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
        this.background = this.video.GetVideoObject("LOADSCREENS\\MainMenuBackGround.sti", out this.mainMenuBackGroundImageKey);

        // load ja2 logo graphic and add it
        this.logo = this.video.GetVideoObject("LOADSCREENS\\Ja2Logo.sti", out this.ja2LogoImageKey);

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
        ButtonSubSystem.DisableButton(this.iMenuButtons[MainMenuItems.LOAD_GAME]);
        //}

        //	DisableButton( iMenuButtons[ CREDITS ] );
        //	DisableButton( iMenuButtons[ TITLE ] );

        Globals.gbHandledMainMenu = 0;
        this.fInitialRender = true;

        await this.screens.SetPendingNewScreen(ScreenName.MAINMENU_SCREEN);
        guiMainMenuExitScreen = ScreenName.MAINMENU_SCREEN;

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
            this.iMenuImages[MainMenuItems.NEW_GAME] = this.buttons.LoadButtonImage("LOADSCREENS\\titletext.sti", 0, 0, 1, 2, -1);
            sSlot = 0;

            this.iMenuImages[MainMenuItems.LOAD_GAME] = ButtonSubSystem.UseLoadedButtonImage(this.iMenuImages[MainMenuItems.NEW_GAME], 6, 3, 4, 5, -1);
            this.iMenuImages[MainMenuItems.PREFERENCES] = ButtonSubSystem.UseLoadedButtonImage(this.iMenuImages[MainMenuItems.NEW_GAME], 7, 7, 8, 9, -1);
            this.iMenuImages[MainMenuItems.CREDITS] = ButtonSubSystem.UseLoadedButtonImage(this.iMenuImages[MainMenuItems.NEW_GAME], 13, 10, 11, 12, -1);
            this.iMenuImages[MainMenuItems.QUIT] = ButtonSubSystem.UseLoadedButtonImage(this.iMenuImages[MainMenuItems.NEW_GAME], 14, 14, 15, 16, -1);

            for (cnt = 0; cnt < (int)MainMenuItems.NUM_MENU_ITEMS; cnt++)
            {
                var menuItem = (MainMenuItems)cnt;
                switch (cnt)
                {
                    case (int)MainMenuItems.NEW_GAME:
                        Globals.gusMainMenuButtonWidths[cnt] = ButtonSubSystem.GetWidthOfButtonPic(this.iMenuImages[menuItem], sSlot);
                        break;
                    case (int)MainMenuItems.LOAD_GAME:
                        Globals.gusMainMenuButtonWidths[cnt] = ButtonSubSystem.GetWidthOfButtonPic(this.iMenuImages[menuItem], 3);
                        break;
                    case (int)MainMenuItems.PREFERENCES:
                        Globals.gusMainMenuButtonWidths[cnt] = ButtonSubSystem.GetWidthOfButtonPic(this.iMenuImages[menuItem], 7);
                        break;
                    case (int)MainMenuItems.CREDITS:
                        Globals.gusMainMenuButtonWidths[cnt] = ButtonSubSystem.GetWidthOfButtonPic(this.iMenuImages[menuItem], 10);
                        break;
                    case (int)MainMenuItems.QUIT:
                        Globals.gusMainMenuButtonWidths[cnt] = ButtonSubSystem.GetWidthOfButtonPic(this.iMenuImages[menuItem], 15);
                        break;
                }

                this.iMenuButtons[menuItem] = ButtonSubSystem.QuickCreateButton(
                    this.iMenuImages[menuItem],
                    new Point(
                        320 - Globals.gusMainMenuButtonWidths[cnt] / 2,
                        Globals.MAINMENU_Y + (cnt * Globals.MAINMENU_Y_SPACE)),
                    ButtonFlags.BUTTON_TOGGLE,
                    MSYS_PRIORITY.HIGHEST,
                    MouseSubSystem.DefaultMoveCallback,
                    this.MenuButtonCallback);

                if (this.iMenuButtons[menuItem] is null)
                {
                    return false;
                }

                this.iMenuButtons[menuItem].UserData[0] = cnt;
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
                ButtonSubSystem.RemoveButton(this.iMenuButtons[(MainMenuItems)cnt]);
                ButtonSubSystem.UnloadButtonImage(this.iMenuImages[(MainMenuItems)cnt]);
            }

            fButtonsCreated = false;
        }

        return true;
    }

    private void MenuButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reasonValue)
    {
        MSYS_CALLBACK_REASON reason = reasonValue;
        MainMenuItems bID;

        bID = (MainMenuItems)btn.UserData[0];

        if (!btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED))
        {
            return;
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            // handle menu
            Globals.gbHandledMainMenu = bID;
            this.RenderMainMenu();

            if (Globals.gbHandledMainMenu == MainMenuItems.NEW_GAME)
            {
                this.SetMainMenuExitScreen(ScreenName.GAME_INIT_OPTIONS_SCREEN);
            }
            else if (Globals.gbHandledMainMenu == MainMenuItems.LOAD_GAME)
            {
                // if (gfKeyState[ALT])
                // {
                //     gfLoadGameUponEntry = true;
                // }
            }

            btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            this.RenderMainMenu();
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
        }
    }

    private static bool fRegionCreated = false;
    private HVOBJECT background;
    private HVOBJECT logo;

    public void CreateDestroyBackGroundMouseMask(bool fCreate)
    {
        if (fCreate)
        {
            if (fRegionCreated)
            {
                return;
            }

            // Make a mouse region
            MouseSubSystem.MSYS_DefineRegion(
                ref Globals.gBackRegion,
                new(0, 0, 640, 480),
                MSYS_PRIORITY.HIGHEST,
                CURSOR.NORMAL,
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

            MouseSubSystem.MSYS_RemoveRegion(Globals.gBackRegion);
            fRegionCreated = false;
        }
    }

    private void SelectMainMenuBackGroundRegionCallBack(ref MOUSE_REGION region, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
        {
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            //		if( gfDoHelpScreen )
            //		{
            //			SetMainMenuExitScreen( INIT_SCREEN );
            //			gfDoHelpScreen = false;
            //		}
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
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
        this.video.Surfaces[SurfaceType.FRAME_BUFFER].Mutate(ctx => ctx.Clear(Color.AliceBlue));
        this.video.InvalidateScreen();
    }

    public void Draw(IVideoManager videoManager)
    {


    }

    public ValueTask Deactivate()
    {
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
    }
}

// MENU ITEMS
public enum MainMenuItems
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
