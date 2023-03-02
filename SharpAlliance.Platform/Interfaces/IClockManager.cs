namespace SharpAlliance.Platform.Interfaces;

public interface IClockManager : ISharpAllianceManager
{
    long GetClock();
    long GetTickCount();
    void UpdateClock();
    static abstract uint GetJA2Clock();
    void UnPauseGame();
    void PauseGame();
    void RemoveMouseRegionForPauseOfClock();
    void InterruptTime();
    void LockPauseState(int v);
    void UnLockPauseState();
}
