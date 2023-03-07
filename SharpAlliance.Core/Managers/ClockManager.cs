using System;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.Managers;

public class ClockManager : IClockManager
{
    private static DateTimeOffset DateTimeOffset;
    private static TimeSpan interval = TimeSpan.FromMilliseconds(10.0);

    private Timer? globalTimer;

    public bool IsInitialized { get; private set; }

    private readonly GameContext context;
    private IInputManager inputs;

    public ClockManager(GameContext context)
    {
        context = context;
    }

    public ValueTask<bool> Initialize()
    {
        DateTimeOffset = new DateTimeOffset();
        Globals.guiStartupTime = Globals.guiCurrentTime = DateTimeOffset.ToUnixTimeMilliseconds();
        globalTimer = new Timer
        {
            Interval = interval.TotalMilliseconds,
            AutoReset = false,
        };

        globalTimer.Elapsed += ClockCallback;
        StartTimer();

        inputs = context.Services.GetRequiredService<IInputManager>();

        IsInitialized = true;

        return ValueTask.FromResult(true);
    }

    public static long GetClock()
    {
        return Globals.guiCurrentTime;
    }

    private void ClockCallback(object sender, ElapsedEventArgs e)
    {
        Globals.guiCurrentTime = e.SignalTime.Ticks;

        StartTimer();
    }

    private void StartTimer() => globalTimer?.Start();
    private void StopTimer() => globalTimer?.Stop();

    public void Dispose()
    {
        StopTimer();

        globalTimer?.Dispose();
    }

    public static void UpdateClock()
    {
        Globals.guiCurrentTime = DateTimeOffset.ToUnixTimeMilliseconds();
    }

    public static long GetTickCount()
    {
        return Globals.guiCurrentTime;
    }

    public static uint GetJA2Clock()
    {
        return (uint)Globals.guiCurrentTime;
    }

    public static void UnPauseGame()
    {
        // if we're paused
        if (Globals.gfGamePaused)
        {
            // ignore request if locked
            if (Globals.gfLockPauseState)
            {
                //Messages.ScreenMsg(FONT_ORANGE, MSG_TESTVERSION, "Call to UnPauseGame() while Pause State is LOCKED! AM-4");
                return;
            }

            Globals.gfGamePaused = false;
            // fMapScreenBottomDirty = true;
        }
    }

    public static void PauseGame()
    {
        // always allow pausing, even if "locked".  Locking applies only to trying to compress time, not to pausing it
        if (!Globals.gfGamePaused)
        {
            Globals.gfGamePaused = true;
            //fMapScreenBottomDirty = true;
        }
    }

    public static void RemoveMouseRegionForPauseOfClock()
    {
        // remove pause region
        if (Globals.fClockMouseRegionCreated == true)
        {
            MouseSubSystem.MSYS_RemoveRegion(Globals.gClockMouseRegion);
            Globals.fClockMouseRegionCreated = false;

        }
    }

    public static void InterruptTime()
    {
        Globals.gfTimeInterrupt = true;
    }

    // call this to prevent player from changing the time compression state via the interface
    public static void LockPauseState(int uiUniqueReasonId)
    {
        Globals.gfLockPauseState = true;

        // if adding a new call, please choose a new uiUniqueReasonId, this helps track down the cause when it's left locked
        // Highest # used was 21 on Feb 15 '99.
        Globals.guiLockPauseStateLastReasonId = uiUniqueReasonId;
    }

    public static void UnLockPauseState()
    {
        throw new NotImplementedException();
    }
}
