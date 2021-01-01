using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Platform;

namespace SharpAlliance.Core.Screens
{
    public class SplashScreen : IScreen
    {
        private readonly GameContext context;
        private readonly int guiSplashFrameFade = 10;
        private readonly int guiSplashStartTime = 0;

        public SplashScreen(GameContext context)
        {
            this.context = context;
        }

        public bool IsInitialized { get; set; }

        public ValueTask Activate()
        {
            return ValueTask.CompletedTask;
        }

        public bool Initialize()
        {
            return true;
        }

        public void Dispose()
        {
        }

        public ValueTask<int> Handle()
        {
            return ValueTask.FromResult(0);
        }
    }
}
