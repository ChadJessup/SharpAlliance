using System;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.Managers
{
    public class ClockManager : IClockManager
    {
        private DateTimeOffset DateTimeOffset;
        private long guiStartupTime;
        private long guiCurrentTime;
        private TimeSpan interval = TimeSpan.FromMilliseconds(10.0);

        private Timer? globalTimer;
        private bool gfLockPauseState = false;
        private int guiLockPauseStateLastReasonId = 0;
        private bool fClockMouseRegionCreated;

        private bool gfPauseClock = false;

        public bool IsInitialized { get; private set; }
        public bool gfGamePaused { get; set; }
        public bool gfTimeInterrupt { get; set; } = false;

        // clock mouse region
        private MouseRegion gClockMouseRegion;
        private MouseRegion gClockScreenMaskMouseRegion;

        private readonly GameContext context;
        private IInputManager inputs;

        public ClockManager(GameContext context)
        {
            this.context = context;
        }

        public ValueTask<bool> Initialize()
        {
            this.DateTimeOffset = new DateTimeOffset();
            this.guiStartupTime = this.guiCurrentTime = this.DateTimeOffset.ToUnixTimeMilliseconds();
            this.globalTimer = new Timer
            {
                Interval = this.interval.TotalMilliseconds,
                AutoReset = false,
            };

            this.globalTimer.Elapsed += this.ClockCallback;
            this.StartTimer();

            this.inputs = this.context.Services.GetRequiredService<IInputManager>();

            this.IsInitialized = true;

            return ValueTask.FromResult(true);
        }

        public long GetClock()
        {
            return this.guiCurrentTime;
        }

        private void ClockCallback(object sender, ElapsedEventArgs e)
        {
            this.guiCurrentTime = e.SignalTime.Ticks;

            this.StartTimer();
        }

        private void StartTimer() => this.globalTimer?.Start();
        private void StopTimer() => this.globalTimer?.Stop();

        public void Dispose()
        {
            this.StopTimer();

            this.globalTimer?.Dispose();
        }

        public void UpdateClock()
        {
            this.guiCurrentTime = this.DateTimeOffset.ToUnixTimeMilliseconds();
        }

        public long GetTickCount()
        {
            return this.guiCurrentTime;
        }

        public uint GetJA2Clock()
        {
            return (uint)this.guiCurrentTime;
        }

        public void UnPauseGame()
        {
            // if we're paused
            if (this.gfGamePaused)
            {
                // ignore request if locked
                if (this.gfLockPauseState)
                {
                    //ScreenMsg(FONT_ORANGE, MSG_TESTVERSION, L"Call to UnPauseGame() while Pause State is LOCKED! AM-4");
                    return;
                }

                this.gfGamePaused = false;
                // fMapScreenBottomDirty = TRUE;
            }
        }

        public void PauseGame()
        {
            // always allow pausing, even if "locked".  Locking applies only to trying to compress time, not to pausing it
            if (!this.gfGamePaused)
            {
                this.gfGamePaused = true;
                //fMapScreenBottomDirty = true;
            }
        }

        public void RemoveMouseRegionForPauseOfClock()
        {
            // remove pause region
            if (this.fClockMouseRegionCreated == true)
            {
                this.inputs.Mouse.MSYS_RemoveRegion(this.gClockMouseRegion);
                this.fClockMouseRegionCreated = false;

            }
        }

        public void InterruptTime()
        {
            gfTimeInterrupt = true;
        }

        // call this to prevent player from changing the time compression state via the interface
        public void LockPauseState(int uiUniqueReasonId)
        {
            gfLockPauseState = true;

            // if adding a new call, please choose a new uiUniqueReasonId, this helps track down the cause when it's left locked
            // Highest # used was 21 on Feb 15 '99.
            guiLockPauseStateLastReasonId = uiUniqueReasonId;
        }

        public void PauseTime(bool fPaused)
        {
            gfPauseClock = fPaused;
        }

        public void UnLockPauseState()
        {
            throw new NotImplementedException();
        }
    }
}
