using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpAlliance.Core.Screens;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core
{
    public class SharpAllianceGameLogic : IGameLogic
    {
        private readonly GameContext context;

        public SharpAllianceGameLogic(GameContext context)
        => this.context = context;

        public async ValueTask<bool> Initialize()
        {
            this.RegisterGameScreens(this.context);

            await this.InitializeScreens(this.context.ScreenManager.Screens);

            // Call some game specific start up and splash screen!
            var splashScreen = await context.ScreenManager.ActivateScreen(ScreenNames.SplashScreen);

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
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(100);
                Console.WriteLine(nameof(GameLoop));
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
