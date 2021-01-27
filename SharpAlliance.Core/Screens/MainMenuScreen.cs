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

namespace SharpAlliance.Core.Screens
{
    public class MainMenuScreen : IScreen
    {
        private readonly ButtonSubSystem buttons;
        private readonly GameInit gameInit;
        private readonly IScreenManager screens;
        private readonly GameOptions options;
        private readonly IInputManager input;
        private readonly MouseSubSystem mouse;
        private readonly IVideoManager video;

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

        int guiMainMenuBackGroundImage;
        int guiJa2LogoImage;

        private MouseRegion gBackRegion = new();
        private sbyte gbHandledMainMenu = 0;
        private bool fInitialRender = false;
        //bool						gfDoHelpScreen = 0;

        private bool gfMainMenuScreenEntry = false;
        private bool gfMainMenuScreenExit = false;

        ScreenName guiMainMenuExitScreen = ScreenName.MAINMENU_SCREEN;

        public MainMenuScreen(
            MouseSubSystem mouseSubSystem,
            IScreenManager screenManager,
            IVideoManager videoManager,
            GameInit gameInit,
            GameOptions gameOptions,
            ButtonSubSystem buttonSubSystem,
            IInputManager inputManager)
        {
            this.buttons = buttonSubSystem;
            this.input = inputManager;
            this.mouse = mouseSubSystem;
            this.video = videoManager;
            this.options = gameOptions;
            this.screens = screenManager;
            this.gameInit = gameInit;
        }

        public bool IsInitialized { get; set; }
        public ScreenState State { get; set; }
        public bool gfFadeInitialized;
        public bool gfFadeIn;
        public bool gfFadeInVideo;
        public int gbFadeType;
        public Action gFadeFunction;

        public ValueTask Activate()
        {
            return ValueTask.CompletedTask;
        }

        public void Dispose()
        {
        }

        public ValueTask<ScreenName> Handle()
        {
            return ValueTask.FromResult(ScreenName.MAINMENU_SCREEN);
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
            //	FilenameForBPP("INTERFACE\\Ja2_2.sti", VObjectDesc.ImageFile);
            VObjectDesc.ImageFile = Utils.FilenameForBPP("LOADSCREENS\\Ja2Logo.sti");
            this.video.AddVideoObject(ref VObjectDesc, out guiJa2LogoImage);

            /*
                // Gray out some buttons based on status of game!
                if( gGameSettings.bLastSavedGameSlot < 0 || gGameSettings.bLastSavedGameSlot >= NUM_SAVE_GAMES )
                {
                    DisableButton( iMenuButtons[ LOAD_GAME ] );
                }
                //The ini file said we have a saved game, but there is no saved game
                else if( gbSaveGameArray[ gGameSettings.bLastSavedGameSlot ] == FALSE )
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
                //			gfDoHelpScreen = FALSE;
                //		}
            }
            else if (iReason.HasFlag(MouseCallbackReasons.RBUTTON_UP))
            {
                /*
                        if( gfDoHelpScreen )
                        {
                            SetMainMenuExitScreen( INIT_SCREEN );
                            gfDoHelpScreen = FALSE;
                        }
                */
            }
        }

        public void ClearMainMenu()
        {
        }
    }
}
