using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;
using SixLabors.ImageSharp;
using Veldrid;
using static SharpAlliance.Core.Screens.IntroScreen;

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
        private readonly IScreenManager screen;
        private readonly MouseSubSystem mouse;
        private readonly FontSubSystem fonts;
        private readonly IInputManager inputs;
        private readonly IMusicManager music;
        private readonly MessageBoxSubSystem messageBox;
        private readonly Globals globals;
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
            IScreenManager screenManager,
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
            Globals = globals;
            this.os = OSManager;
            this.screen = screenManager;
        }

        public async ValueTask<bool> Initialize()
        {
            this.RegisterGameScreens(this.context, this.screen);
            await this.InitializeScreens(this.screen.Screens);
            this.saves.LoadGameSettings();
            this.saves.InitGameOptions();

            var initScreen = await this.screen.ActivateScreen(ScreenName.InitScreen) as InitScreen;

            this.mapScreen = (await this.screen.GetScreen(ScreenName.MAP_SCREEN, activate: false) as MapScreen)!;
            var introScreen = (await this.screen.GetScreen(ScreenName.INTRO_SCREEN, activate: false) as IntroScreen)!;
            introScreen.SetIntroType(IntroScreenType.INTRO_SPLASH);

            this.mapScreen.HandlePreloadOfMapGraphics();
            this.IsInitialized = true;

            return this.IsInitialized;
        }

        public async Task<int> GameLoop(CancellationToken token = default)
        {
            IScreen nextScreen = this.screen.CurrentScreen;
            var sm = this.screen;

            while (this.context.State == GameState.Running && !token.IsCancellationRequested)
            {
                nextScreen = sm.CurrentScreen;

                this.inputs.ProcessEvents();

                this.inputs.GetCursorPosition(out SixLabors.ImageSharp.Point MousePos);

                // Hook into mouse stuff for MOVEMENT MESSAGES

                this.mouse.MouseHook(MouseEvents.MousePosition, MousePos, this.inputs.gfLeftButtonState, this.inputs.gfRightButtonState);

                this.music.MusicPoll(false);

                while (this.inputs.DequeSpecificEvent(out InputSnapshot inputSnapshot))
                {
                    MouseEvents mouseEvent = this.inputs.ConvertToMouseEvents(ref inputSnapshot);

                    switch (mouseEvent)
                    {
                        case MouseEvents.LEFT_BUTTON_DOWN:
                            this.mouse.MouseHook(MouseEvents.LEFT_BUTTON_DOWN, MousePos, this.inputs.gfLeftButtonState, this.inputs.gfRightButtonState);
                            break;
                        case MouseEvents.LEFT_BUTTON_UP:
                            this.mouse.MouseHook(MouseEvents.LEFT_BUTTON_UP, MousePos, this.inputs.gfLeftButtonState, this.inputs.gfRightButtonState);
                            break;
                        case MouseEvents.RIGHT_BUTTON_DOWN:
                            this.mouse.MouseHook(MouseEvents.RIGHT_BUTTON_DOWN, MousePos, this.inputs.gfLeftButtonState, this.inputs.gfRightButtonState);
                            break;
                        case MouseEvents.RIGHT_BUTTON_UP:
                            this.mouse.MouseHook(MouseEvents.RIGHT_BUTTON_UP, MousePos, this.inputs.gfLeftButtonState, this.inputs.gfRightButtonState);
                            break;
                        case MouseEvents.LEFT_BUTTON_REPEAT:
                            this.mouse.MouseHook(MouseEvents.LEFT_BUTTON_REPEAT, MousePos, this.inputs.gfLeftButtonState, this.inputs.gfRightButtonState);
                            break;
                        case MouseEvents.RIGHT_BUTTON_REPEAT:
                            this.mouse.MouseHook(MouseEvents.RIGHT_BUTTON_REPEAT, MousePos, this.inputs.gfLeftButtonState, this.inputs.gfRightButtonState);
                            break;
                    }
                }

                if (Globals.gfGlobalError)
                {
                    await sm.ActivateScreen(ScreenName.ERROR_SCREEN);
                }

                // ATE: Force to be in message box screen!
                if (this.messageBox.gfInMsgBox)
                {
                    sm.guiPendingScreen = await sm.GetScreen(ScreenName.MSG_BOX_SCREEN, activate: true);
                }

                if (sm.guiPendingScreen != NullScreen.Instance)
                {
                    // Based on active screen, deinit!
                    if (sm.guiPendingScreen != sm.CurrentScreen)
                    {
                        switch (sm.CurrentScreen)
                        {
                            case MapScreen ms when sm.guiPendingScreen is MessageBoxScreen:
                                sm.EndMapScreen(false);
                                break;
                            case LaptopScreen:
                                sm.ExitLaptop();
                                break;
                        }
                    }

                    // if the screen has chnaged
                    if (nextScreen != sm.guiPendingScreen)
                    {
                        // Set the fact that the screen has changed
                        nextScreen = sm.guiPendingScreen;

                        this.HandleNewScreenChange(sm.guiPendingScreen, sm.CurrentScreen);
                    }

                    await sm.ActivateScreen(sm.guiPendingScreen);
                    sm.guiPendingScreen = NullScreen.Instance;
                }

                this.video.ClearElements();
                var nextScreenName = await sm.CurrentScreen.Handle();
                nextScreen = await sm.GetScreen(nextScreenName, activate: false);

                // if the screen has chnaged
                if (nextScreen != sm.CurrentScreen)
                {
                    this.HandleNewScreenChange(nextScreen, sm.CurrentScreen);
                    await sm.ActivateScreen(nextScreen);
                }

                this.video.RefreshScreen();

                this.video.DrawFrame();

                Globals.guiGameCycleCounter++;

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
            if (newScreen is not MessageBoxScreen && oldScreen is not MessageBoxScreen)
            {
                //reset the help screen
                //NewScreenSoResetHelpScreen();
            }
        }

        private async Task InitializeScreens(Dictionary<ScreenName, IScreen> screens)
        {
            foreach (var screen in screens.Values)
            {
                await screen.Initialize();
            }
        }

        private void RegisterGameScreens(GameContext context, IScreenManager screen)
        {
            var sm = screen;

            sm.AddScreen<MapScreen>(ScreenName.MAP_SCREEN);
            sm.AddScreen<InitScreen>(ScreenName.InitScreen);
            sm.AddScreen<FadeScreen>(ScreenName.FADE_SCREEN);
            sm.AddScreen<IntroScreen>(ScreenName.INTRO_SCREEN);
            sm.AddScreen<LaptopScreen>(ScreenName.LAPTOP_SCREEN);
            sm.AddScreen<CreditsScreen>(ScreenName.CREDIT_SCREEN);
            sm.AddScreen<MessageBoxScreen>(ScreenName.MSG_BOX_SCREEN);
            sm.AddScreen<MainMenuScreen>(ScreenName.MAINMENU_SCREEN);
            sm.AddScreen<PreferenceScreen>(ScreenName.OPTIONS_SCREEN);
            sm.AddScreen<GameInitOptionsScreen>(ScreenName.GAME_INIT_OPTIONS_SCREEN);
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
