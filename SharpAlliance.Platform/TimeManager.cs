using System;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Platform
{
    public class TimeManager : ITimeManager
    {
        public TimeSpan BaseTimeSlice { get; set; } = TimeSpan.FromSeconds(10.0);

        public bool Initialize()
        {
            return true;
        }


        public void PauseTime(bool shouldPause)
        {
        }

        public DateTime GetClock()
        {
            return DateTime.Now;
        }

        public void Dispose()
        {
        }
    }
}
