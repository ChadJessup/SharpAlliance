using System.Collections.Generic;
using System.Threading.Tasks;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Platform.NullManagers
{
    public class NullScreenManager : IScreenManager
    {
        public Dictionary<string, IScreen> Screens { get; set; } = new();
        public bool IsInitialized { get; } = true;
        public IScreen CurrentScreen { get; }
        public IScreen guiPendingScreen { get; set; }

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

        public ValueTask<bool> Initialize()
        {
            return ValueTask.FromResult(true);
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

        public void RefreshScreen(object p)
        {
            throw new System.NotImplementedException();
        }

        public void HandleNewScreenChange(IScreen uiOldScreen, IScreen guiCurrentScreen)
        {
            throw new System.NotImplementedException();
        }

        public void EndMapScreen(bool v)
        {
            throw new System.NotImplementedException();
        }

        public void ExitLaptop()
        {
            throw new System.NotImplementedException();
        }

        public class NullScreen : IScreen
        {
            public static IScreen Instance { get; } = new NullScreen();
            public bool IsInitialized { get; set; } = true;
            public ScreenState State { get; set; } = ScreenState.Unknown;

            public ValueTask Activate()
            {
                return ValueTask.CompletedTask;
            }

            public void Dispose()
            {
            }

            public ValueTask<IScreen> Handle()
            {
                return ValueTask.FromResult<IScreen>(this);
            }

            public ValueTask<bool> Initialize()
            {
                return ValueTask.FromResult(true);
            }
        }
    }
}
