using System;
using System.Threading.Tasks;
using System.Timers;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Platform
{
    public class ClockManager : IClockManager
    {
        private long guiStartupTime;
        private long guiCurrentTime;
        private TimeSpan interval = TimeSpan.FromMilliseconds(10.0);

        private Timer? globalTimer;

        public bool IsInitialized { get; private set; }

        public ValueTask<bool> Initialize()
        {
            this.guiStartupTime = this.guiCurrentTime = DateTime.Now.Ticks;
            this.globalTimer = new Timer
            {
                Interval = this.interval.TotalMilliseconds,
                AutoReset = false,
            };

            this.globalTimer.Elapsed += this.ClockCallback;
            this.StartTimer();

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
        }

        public long GetTickCount()
        {
            return this.guiCurrentTime;
        }
    }
}
