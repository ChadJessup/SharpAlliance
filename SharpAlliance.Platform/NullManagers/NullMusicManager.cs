using System.Threading.Tasks;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Platform.NullManagers
{
    public class NullMusicManager : IMusicManager
    {
        public bool IsInitialized => true;

        public ValueTask<bool> Initialize()
        {
            return ValueTask.FromResult(true);
        }

        public void Dispose()
        {
        }

        public bool MusicPoll(bool force)
        {
            return true;
        }

        public void SetMusicMode(MusicMode mode)
        {
        }

        public void MusicSetVolume(byte v)
        {
        }
    }
}
