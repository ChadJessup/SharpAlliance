using System.Collections.Generic;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.Interfaces
{
    public interface IScreenManager : ISharpAllianceManager
    {
        ValueTask<IScreen> ActivateScreen(ScreenName screenName);
        ValueTask<IScreen> ActivateScreen(IScreen screen);
        bool ScreenExists(ScreenName screenName);
        Dictionary<ScreenName, IScreen> Screens { get; set; }
        ValueTask<IScreen> GetScreen(ScreenName screenName, bool activate);
        IScreenManager AddScreen<TScreen>(ScreenName screenName) where TScreen : IScreen;
        IScreen CurrentScreen { get; }
        IScreen guiPendingScreen { get; set; }

        void EndMapScreen(bool v);
        void ExitLaptop();
    }
}
