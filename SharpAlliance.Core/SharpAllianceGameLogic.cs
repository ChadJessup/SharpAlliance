using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;
using Veldrid;
using static SharpAlliance.Platform.NullManagers.NullScreenManager;

namespace SharpAlliance.Core
{
    public class SharpAllianceGameLogic : IGameLogic
    {
        private readonly GameContext context;
        private readonly IStringLocalizer<string> strings;
        private readonly HelpScreenSubSystem helpScreen;
        private readonly ButtonSubSystem buttons;
        private readonly CursorSubSystem cursors;
        private readonly SaveGameSubSystem saves;
        private readonly IVideoManager video;
        private readonly IOSManager os;
        private readonly MouseSubSystem mouse;
        private readonly FontSubSystem fonts;
        private readonly IInputManager inputs;
        private readonly IMusicManager music;
        private readonly MessageBoxSubSystem messageBox;
        private readonly Globals globals;
        private IScreen guiCurrentScreen;
        private MapScreen mapScreen;

        public bool IsInitialized { get; private set; }

        public SharpAllianceGameLogic(
            GameContext context,
            IStringLocalizer<string> strings,
            MouseSubSystem mouseSubSystem,
            ButtonSubSystem buttonSubSystem,
            CursorSubSystem cursorSubSystem,
            FontSubSystem fontSubSystem,
            HelpScreenSubSystem helpScreenSubSystem,
            IInputManager inputManager,
            SaveGameSubSystem saveGameSubSystem,
            MessageBoxSubSystem messageBoxSubSystem,
            IOSManager OSManager,
            IVideoManager videoManager,
            IMusicManager musicManager,
            Globals globals)
        {
            this.context = context;
            this.strings = strings;

            // These should be initialized already
            this.mouse = mouseSubSystem;
            this.buttons = buttonSubSystem;
            this.cursors = cursorSubSystem;
            this.fonts = fontSubSystem;
            this.helpScreen = helpScreenSubSystem;
            this.inputs = inputManager;
            this.saves = saveGameSubSystem;
            this.video = videoManager;
            this.music = musicManager;
            this.messageBox = messageBoxSubSystem;
            this.globals = globals;
            this.os = OSManager;
        }

        public async ValueTask<bool> Initialize()
        {
            this.RegisterGameScreens(this.context);
            await this.InitializeScreens(this.context.ScreenManager.Screens);
            this.saves.LoadGameSettings();
            this.saves.InitGameOptions();

            var splashScreen = await this.context.ScreenManager.ActivateScreen(ScreenNames.SplashScreen) as SplashScreen;
            splashScreen?.SetIntroType(IntroScreenType.SPLASH);

            this.mapScreen = (await this.context.ScreenManager.GetScreen(ScreenNames.MAP_SCREEN, activate: false) as MapScreen)!;

            this.mapScreen.HandlePreloadOfMapGraphics();
            this.IsInitialized = true;

            return this.IsInitialized;
        }

        public async Task<int> GameLoop(CancellationToken token = default)
        {
            IScreen uiOldScreen = this.context.ScreenManager.CurrentScreen;
            var sm = this.context.ScreenManager;

            while (this.context.State == GameState.Running && !token.IsCancellationRequested)
            {
                uiOldScreen = this.guiCurrentScreen;

                this.inputs.ProcessEvents();

                this.inputs.GetCursorPosition(out Point MousePos);

                // Hook into mouse stuff for MOVEMENT MESSAGES
                this.mouse.MouseHook(MouseEvents.MousePosition, MousePos.X, MousePos.Y, this.inputs.gfLeftButtonState, this.inputs.gfRightButtonState);

                this.music.MusicPoll(false);

                while (this.inputs.DequeSpecificEvent(out InputAtom? inputAtom, MouseEvents.LEFT_BUTTON_REPEAT | MouseEvents.RIGHT_BUTTON_REPEAT | MouseEvents.LEFT_BUTTON_DOWN | MouseEvents.LEFT_BUTTON_UP | MouseEvents.RIGHT_BUTTON_DOWN | MouseEvents.RIGHT_BUTTON_UP))
                {
                    switch (inputAtom!.Value.MouseEvents)
                    {
                        case MouseEvents.LEFT_BUTTON_DOWN:
                            this.mouse.MouseHook(MouseEvents.LEFT_BUTTON_DOWN, MousePos.X, MousePos.Y, this.inputs.gfLeftButtonState, this.inputs.gfRightButtonState);
                            break;
                        case MouseEvents.LEFT_BUTTON_UP:
                            this.mouse.MouseHook(MouseEvents.LEFT_BUTTON_UP, MousePos.X, MousePos.Y, this.inputs.gfLeftButtonState, this.inputs.gfRightButtonState);
                            break;
                        case MouseEvents.RIGHT_BUTTON_DOWN:
                            this.mouse.MouseHook(MouseEvents.RIGHT_BUTTON_DOWN, MousePos.X, MousePos.Y, this.inputs.gfLeftButtonState, this.inputs.gfRightButtonState);
                            break;
                        case MouseEvents.RIGHT_BUTTON_UP:
                            this.mouse.MouseHook(MouseEvents.RIGHT_BUTTON_UP, MousePos.X, MousePos.Y, this.inputs.gfLeftButtonState, this.inputs.gfRightButtonState);
                            break;
                        case MouseEvents.LEFT_BUTTON_REPEAT:
                            this.mouse.MouseHook(MouseEvents.LEFT_BUTTON_REPEAT, MousePos.X, MousePos.Y, this.inputs.gfLeftButtonState, this.inputs.gfRightButtonState);
                            break;
                        case MouseEvents.RIGHT_BUTTON_REPEAT:
                            this.mouse.MouseHook(MouseEvents.RIGHT_BUTTON_REPEAT, MousePos.X, MousePos.Y, this.inputs.gfLeftButtonState, this.inputs.gfRightButtonState);
                            break;
                    }
                }

                if (this.globals.gfGlobalError)
                {
                    this.guiCurrentScreen = await sm.GetScreen(ScreenNames.ERROR_SCREEN, activate: true);
                }

                // ATE: Force to be in message box screen!
                if (this.messageBox.gfInMsgBox)
                {
                    sm.guiPendingScreen = await sm.GetScreen(ScreenNames.MSG_BOX_SCREEN, activate: true);
                }

                if (sm.guiPendingScreen != NullScreen.Instance)
                {
                    // Based on active screen, deinit!
                    if (sm.guiPendingScreen != this.guiCurrentScreen)
                    {
                        switch (this.guiCurrentScreen)
                        {
                            case MapScreen ms when sm.guiPendingScreen is MSG_BOX_SCREEN:
                                sm.EndMapScreen(false);
                                break;
                            case LAPTOP_SCREEN:
                                sm.ExitLaptop();
                                break;
                        }
                    }

                    // if the screen has chnaged
                    if (uiOldScreen != sm.guiPendingScreen)
                    {
                        // Set the fact that the screen has changed
                        uiOldScreen = sm.guiPendingScreen;

                        this.HandleNewScreenChange(sm.guiPendingScreen, this.guiCurrentScreen);
                    }

                    this.guiCurrentScreen = sm.guiPendingScreen;
                    sm.guiPendingScreen = NullScreen.Instance;
                }

                uiOldScreen = await sm.CurrentScreen.Handle();

                // if the screen has chnaged
                if (uiOldScreen != this.guiCurrentScreen)
                {
                    this.HandleNewScreenChange(uiOldScreen, this.guiCurrentScreen);
                    this.guiCurrentScreen = uiOldScreen;
                }

                this.video.RefreshScreen();

                this.video.DrawFrame();

                this.globals.guiGameCycleCounter++;

                this.context.ClockManager.UpdateClock();

                if (this.context.State != GameState.Running)
                {
                    this.context.State = GameState.ExitRequested;
                }
            }

            return 0;
        }

        public void HandleNewScreenChange(IScreen newScreen, IScreen oldScreen)
        {
            //if we are not going into the message box screen, and we didnt just come from it
            if ((newScreen is not MSG_BOX_SCREEN && oldScreen is not MSG_BOX_SCREEN))
            {
                //reset the help screen
                //NewScreenSoResetHelpScreen();
            }
        }

        private async Task InitializeScreens(Dictionary<string, IScreen> screens)
        {
            foreach (var screen in screens.Values)
            {
                await screen.Initialize();
            }
        }

        private void RegisterGameScreens(GameContext context)
        {
            var sm = context.ScreenManager;

            sm.AddScreen<SplashScreen>(ScreenNames.SplashScreen);
            sm.AddScreen<InitScreen>(ScreenNames.InitScreen);
            sm.AddScreen<MapScreen>(ScreenNames.MAP_SCREEN);
            sm.AddScreen<LAPTOP_SCREEN>(ScreenNames.LAPTOP_SCREEN);
            sm.AddScreen<MSG_BOX_SCREEN>(ScreenNames.MSG_BOX_SCREEN);
            sm.AddScreen<FadeScreen>(ScreenNames.FADE_SCREEN);
        }

        private void CheckForSpace()
        {

            //if we are to check for free space on the hard drive
            //if (gubCheckForFreeSpaceOnHardDriveCount < DONT_CHECK_FOR_FREE_SPACE)
            //{
            //    //only if we are in a screen that can get this check
            //    if (guiCurrentScreen == MAP_SCREEN || guiCurrentScreen == GAME_SCREEN || guiCurrentScreen == SAVE_LOAD_SCREEN)
            //    {
            //        if (gubCheckForFreeSpaceOnHardDriveCount < 1)
            //        {
            //            gubCheckForFreeSpaceOnHardDriveCount++;
            //        }
            //        else
            //        {
            //            // Make sure the user has enough hard drive space
            //            //if (!DoesUserHaveEnoughHardDriveSpace())
            //            //{
            //            //    CHAR16 zText[512];
            //            //    CHAR16 zSpaceOnDrive[512];
            //            //    UINT32 uiSpaceOnDrive;
            //            //    CHAR16 zSizeNeeded[512];
            //            //
            //            //    wprintf(zSizeNeeded, L"%d", REQUIRED_FREE_SPACE / BYTESINMEGABYTE);
            //            //    InsertCommasForDollarFigure(zSizeNeeded);
            //            //
            //            //    uiSpaceOnDrive = GetFreeSpaceOnHardDriveWhereGameIsRunningFrom();
            //            //
            //            //    wprintf(zSpaceOnDrive, L"%.2f", uiSpaceOnDrive / (FLOAT)BYTESINMEGABYTE);
            //            //
            //            //    wprintf(zText, pMessageStrings[MSG_LOWDISKSPACE_WARNING], zSpaceOnDrive, zSizeNeeded);
            //            //
            //            //    if (guiPreviousOptionScreen == MAP_SCREEN)
            //            //        DoMapMessageBox(MSG_BOX_BASIC_STYLE, zText, MAP_SCREEN, MSG_BOX_FLAG_OK, NULL);
            //            //    else
            //            //        DoMessageBox(MSG_BOX_BASIC_STYLE, zText, GAME_SCREEN, MSG_BOX_FLAG_OK, NULL, NULL);
            //            //}
            //            //gubCheckForFreeSpaceOnHardDriveCount = DONT_CHECK_FOR_FREE_SPACE;
            //        }
            //    }
            //}
        }

        public void Dispose()
        {
        }
    }
}
