using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;
using SharpAlliance.Platform.Interfaces;
using Veldrid;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Interfaces;

public interface IScreenManager : ISharpAllianceManager
{
    ValueTask<IScreen> ActivateScreen(ScreenName screenName);
    ValueTask<IScreen> ActivateScreen(IScreen screen);
    bool ScreenExists(ScreenName screenName);
    Dictionary<ScreenName, IScreen> Screens { get; set; }
    ValueTask<TScreen> GetScreen<TScreen>(ScreenName screenName, bool activate) where TScreen : IScreen;
    ValueTask<IScreen> GetScreen(ScreenName screenName, bool activate);
    IScreenManager AddScreen<TScreen>(ScreenName screenName) where TScreen : IScreen;
    static IScreen CurrentScreen { get; }
    IScreen guiPendingScreen { get; set; }
    ScreenName CurrentScreenName { get; }

    static void Draw(ITextureManager textureManager) => throw new NotImplementedException();
    void EndMapScreen(bool v);
    void ExitLaptop();
    ValueTask SetPendingNewScreen(ScreenName pendingScreen);
}
