using System.Collections.Generic;
using System.Threading.Tasks;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Platform.NullManagers
{
    public class NullScreenManager : IScreenManager
    {
        public Dictionary<string, IScreen> Screens { get; set; } = new();

        public IScreenManager AddScreen<TScreen>(string screenName)
            where TScreen : IScreen
        {
            return this;
        }

        public ValueTask<IScreen> GetScreen(string screenName, bool activate)
        {
            var ns = new NullScreen();
            return ValueTask.FromResult<IScreen>(ns);
        }

        public bool Initialize()
        {
            return true;
        }

        public void Dispose()
        {
        }

        public bool ScreenExists(string screenName)
        {
            return false;
        }

        public ValueTask<IScreen> ActivateScreen(string screenName)
        {
            return ValueTask.FromResult<IScreen>(new NullScreen());
        }

        public ValueTask<IScreen> ActivateScreen(IScreen screen)
        {
            return ValueTask.FromResult<IScreen>(new NullScreen());
        }

        private class NullScreen : IScreen
        {
            public bool IsInitialized { get; set; } = true;

            public ValueTask Activate()
            {
                return ValueTask.CompletedTask;
            }

            public void Dispose()
            {
            }

            public ValueTask<int> Handle()
            {
                return ValueTask.FromResult(0);
            }

            public bool Initialize()
            {
                return true;
            }
        }
    }
}
