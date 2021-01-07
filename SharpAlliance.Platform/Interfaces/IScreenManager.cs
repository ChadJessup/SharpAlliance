using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharpAlliance.Platform.Interfaces
{
    public interface IScreenManager : ISharpAllianceManager
    {
        ValueTask<IScreen> ActivateScreen(string screenName);
        ValueTask<IScreen> ActivateScreen(IScreen screen);
        bool ScreenExists(string screenName);
        Dictionary<string, IScreen> Screens { get; set; }
        ValueTask<IScreen> GetScreen(string screenName, bool activate);
        IScreenManager AddScreen<TScreen>(string screenName) where TScreen : IScreen;
        IScreen CurrentScreen { get; }
        IScreen guiPendingScreen { get; set; }

        void EndMapScreen(bool v);
        void ExitLaptop();
    }
}
