using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class TimerControl
{
    public static void PauseTime(bool fPaused)
    {
        Globals.gfPauseClock = fPaused;
    }
}
