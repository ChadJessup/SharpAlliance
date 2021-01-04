using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core
{
    public class SharpAllianceGameLogic : IGameLogic
    {
        private readonly GameContext context;
        private readonly IStringLocalizer<string> strings;

        public bool IsInitialized { get; private set; }

        public SharpAllianceGameLogic(GameContext context, IStringLocalizer<string> strings)
        {
            this.context = context;
            this.strings = strings;
        }

        public async ValueTask<bool> Initialize()
        {
            this.RegisterGameScreens(this.context);

            await this.InitializeScreens(this.context.ScreenManager.Screens);

            this.IsInitialized = true;

            return true;
        }

        private async Task InitializeScreens(Dictionary<string, IScreen> screens)
        {
            foreach (var screen in screens.Values)
            {
                await screen.Initialize();
            }
        }

        public async Task<int> GameLoop(CancellationToken token = default)
        {
            var inputManager = this.context.InputManager as InputManager;

            if (inputManager is null)
            {
                return 0;
            }

            // Call some game specific start up and splash screen!
            var splashScreen = await this.context.ScreenManager.ActivateScreen(ScreenNames.SplashScreen);

            while (this.context.State == GameState.Running && !token.IsCancellationRequested)
            {
                while (inputManager.DequeSpecificEvent(
                    out var inputAtom,
                    MouseEvents.LEFT_BUTTON_REPEAT | MouseEvents.RIGHT_BUTTON_REPEAT | MouseEvents.LEFT_BUTTON_DOWN | MouseEvents.LEFT_BUTTON_UP | MouseEvents.RIGHT_BUTTON_DOWN | MouseEvents.RIGHT_BUTTON_UP))
                {
                    switch (inputAtom!.Value.MouseEvents)
                    {
                    
                    }
                }
            }

            return 0;
        }

        private void RegisterGameScreens(GameContext context)
        {
            var sm = context.ScreenManager;

            sm.AddScreen<SplashScreen>(ScreenNames.SplashScreen);
        }

        public void Dispose()
        {
        }
    }
}
