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
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ValueTask<ScreenName> Handle()
        {
            throw new NotImplementedException();
        }

        public ValueTask<bool> Initialize()
        {
            throw new NotImplementedException();
        }

        internal void InitMainMenu()
        {
            throw new NotImplementedException();
        }
    }
}
