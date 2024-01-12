namespace SharpAlliance.Core.SubSystems;

public static class StrategicTurns
{
    private static uint guiLastStrategicTime = 0;
    private static uint guiLastTacticalRealTime = 0;

    public static void SyncStrategicTurnTimes()
    {
        guiLastStrategicTime = GameClock.GetWorldTotalSeconds();
        guiLastTacticalRealTime = GetJA2Clock();
    }

    public static void StrategicTurnsNewGame()
    {
        // Sync game start time
        SyncStrategicTurnTimes();
    }
}
