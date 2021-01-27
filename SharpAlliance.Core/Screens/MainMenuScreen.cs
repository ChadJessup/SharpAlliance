using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Platform;

namespace SharpAlliance.Core.Screens
{
    public class MainMenuScreen : IScreen
    {
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
            return ValueTask.FromResult(true);
        }

        public void ClearMainMenu()
        {
        }
    }
}
