namespace SharpAlliance.Platform.Interfaces;

public interface IClockManager : ISharpAllianceManager
{
    static abstract long GetClock();
    static abstract long GetTickCount();
    static abstract void UpdateClock();
    static abstract uint GetJA2Clock();
    static abstract void UnPauseGame();
    static abstract void PauseGame();
    static abstract void RemoveMouseRegionForPauseOfClock();
    static abstract void InterruptTime();
    static abstract void LockPauseState(int v);
    static abstract void UnLockPauseState();
}
