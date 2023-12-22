using System;

namespace SharpAlliance.Platform.Interfaces;

public interface IClockManager : ISharpAllianceManager
{
    static long GetClock()  => throw new NotImplementedException();
    static long GetTickCount()  => throw new NotImplementedException();
    static void UpdateClock()  => throw new NotImplementedException();
    static uint GetJA2Clock()  => throw new NotImplementedException();
    static void UnPauseGame()  => throw new NotImplementedException();
    static void PauseGame()  => throw new NotImplementedException();
    static void RemoveMouseRegionForPauseOfClock()  => throw new NotImplementedException();
    static void InterruptTime()  => throw new NotImplementedException();
    static void LockPauseState(int v)  => throw new NotImplementedException();
    static void UnLockPauseState() => throw new NotImplementedException();
}
