using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Platform;

namespace SharpAlliance.Core.Screens
{
    public class FadeScreen : IScreen
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

        public ValueTask<IScreen> Handle()
        {
            throw new NotImplementedException();
        }

        public ValueTask<bool> Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
