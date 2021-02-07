using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Platform.Interfaces
{
    public interface IClockManager : ISharpAllianceManager
    {
        bool gfGamePaused { get; set; }

        long GetClock();
        long GetTickCount();
        void UpdateClock();
        uint GetJA2Clock();
        void UnPauseGame();
        void PauseGame();
        void RemoveMouseRegionForPauseOfClock();
    }
}
