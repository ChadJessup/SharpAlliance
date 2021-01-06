namespace SharpAlliance.Platform.Interfaces
{
    public interface IMusicManager : ISharpAllianceManager
    {
        bool MusicPoll(bool force);
    }
}
