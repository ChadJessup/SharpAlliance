using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Screens;
using SharpAlliance.Platform;

namespace SharpAlliance.Core.SubSystems;

public class HelpScreenSubSystem : IDisposable
{
    public static bool gfHelpScreenEntry;
    public static bool gfHelpScreenExit;
    private readonly ILogger<HelpScreenSubSystem> logger;

    public bool IsInitialized { get; }

    public HelpScreenSubSystem(ILogger<HelpScreenSubSystem> logger, GameContext gameContext)
    {
        this.logger = logger;
    }

    public void Dispose()
    {
    }

    public void InitHelpScreenSystem()
    {
        //set some values
        gHelpScreen = new();

        //set it up so we can enter the screen
        gfHelpScreenEntry = true;
        gfHelpScreenExit = false;

        gHelpScreen.bCurrentHelpScreenActiveSubPage = HLP_SCRN_LPTP.UNSET;

        gHelpScreen.fHaveAlreadyBeenInHelpScreenSinceEnteringCurrenScreen = false;
    }
}
