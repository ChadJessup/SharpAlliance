using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Platform.Interfaces;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class HelpScreenSubSystem : IDisposable
{
    public static bool gfHelpScreenEntry;
    public static bool gfHelpScreenExit;

    public bool IsInitialized { get; }

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

        gHelpScreen.bCurrentHelpScreenActiveSubPage = -1;

        gHelpScreen.fHaveAlreadyBeenInHelpScreenSinceEnteringCurrenScreen = false;
    }
}
