namespace SharpAlliance.Platform
{
    public enum GameState
    {
        Unknown = 0,
        StartingUp,
        Initializing,
        Running,
        Paused,
        Exiting,
        Disposing,
        ExitRequested,
    }
}
