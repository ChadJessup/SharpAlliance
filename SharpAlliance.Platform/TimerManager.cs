﻿using System;
using System.Threading.Tasks;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Platform
{
    public class TimerManager : ITimerManager
    {
        public TimeSpan BaseTimeSlice { get; set; } = TimeSpan.FromSeconds(10.0);

        public ValueTask<bool> Initialize()
        {
            return ValueTask.FromResult(true);
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
