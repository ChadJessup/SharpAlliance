using System.Collections.Generic;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;
using SharpAlliance.Platform.Interfaces;
using Veldrid;

namespace SharpAlliance.Core.Interfaces
{
    public interface IScreenManager : ISharpAllianceManager
    {
        ValueTask<IScreen> ActivateScreen(ScreenName screenName);
        ValueTask<IScreen> ActivateScreen(IScreen screen);
        bool ScreenExists(ScreenName screenName);
        Dictionary<ScreenName, IScreen> Screens { get; set; }
        ValueTask<TScreen> GetScreen<TScreen>(ScreenName screenName, bool activate) where TScreen : IScreen;
        ValueTask<IScreen> GetScreen(ScreenName screenName, bool activate);
        IScreenManager AddScreen<TScreen>(ScreenName screenName) where TScreen : IScreen;
        static abstract IScreen CurrentScreen { get; }
        IScreen guiPendingScreen { get; set; }
        ScreenName CurrentScreenName { get; }

        static abstract void Draw(SpriteRenderer sr, GraphicsDevice gd, CommandList cl);
        void EndMapScreen(bool v);
        void ExitLaptop();
        ValueTask SetPendingNewScreen(ScreenName pendingScreen);
    }
}
