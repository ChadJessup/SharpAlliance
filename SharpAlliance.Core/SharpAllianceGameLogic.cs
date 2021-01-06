using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;

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
        private readonly InputManager inputs;
        private readonly IMusicManager music;

        private IScreen guiCurrentScreen;

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
            IOSManager OSManager,
            IVideoManager videoManager,
            IMusicManager musicManager)
        {
            this.context = context;
            this.strings = strings;

            // These should be initialized already
            this.mouse = mouseSubSystem;
            this.buttons = buttonSubSystem;
            this.cursors = cursorSubSystem;
            this.fonts = fontSubSystem;
            this.helpScreen = helpScreenSubSystem;
            this.inputs = (inputManager as InputManager)!;
            this.saves = saveGameSubSystem;
            this.video = videoManager;
            this.music = musicManager;

            this.os = OSManager;
        }

        public async ValueTask<bool> Initialize()
        {
            this.RegisterGameScreens(this.context);
            await this.InitializeScreens(this.context.ScreenManager.Screens);
            this.saves.LoadGameSettings();
            this.saves.InitGameOptions();

            var splashScreen = await this.context.ScreenManager.ActivateScreen(ScreenNames.SplashScreen) as SplashScreen;
            splashScreen?.SetIntroType(IntroScreen.SPLASH);

            this.IsInitialized = true;

            return this.IsInitialized;
        }

        public async Task<int> GameLoop(CancellationToken token = default)
        {
            while (this.context.State == GameState.Running && !token.IsCancellationRequested)
            {
                var shouldContinue = await this.os.Pump(() =>
                {
                    IScreen uiOldScreen = guiCurrentScreen;

                    this.inputs.GetCursorPosition(out Point MousePos);

                    // Hook into mouse stuff for MOVEMENT MESSAGES
                    this.mouse.MouseHook(MouseEvents.MOUSE_POS, MousePos.X, MousePos.Y, this.inputs.gfLeftButtonState, this.inputs.gfRightButtonState);

                    this.music.MusicPoll(false);

                    while (this.inputs.DequeSpecificEvent(out var inputAtom, MouseEvents.LEFT_BUTTON_REPEAT | MouseEvents.RIGHT_BUTTON_REPEAT | MouseEvents.LEFT_BUTTON_DOWN | MouseEvents.LEFT_BUTTON_UP | MouseEvents.RIGHT_BUTTON_DOWN | MouseEvents.RIGHT_BUTTON_UP))
                    {
                        switch (inputAtom!.Value.MouseEvents)
                        {

                        }
                    }
                });

                if (!shouldContinue)
                {
                    this.context.State = GameState.ExitRequested;
                }
            }

            return 0;
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
        }

        public void Dispose()
        {
        }
    }
}
