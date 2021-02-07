using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.Managers
{
    public class MusicManager : IMusicManager
    {
        public bool IsInitialized { get; }
        public void Dispose()
        {
        }

        public ValueTask<bool> Initialize()
        {
            return ValueTask.FromResult(true);
        }

        public int MusicGetVolume()
        {
            throw new NotImplementedException();
        }

        public bool MusicPoll(bool force)
        {
            return true;
        }

        public void MusicSetVolume(byte volume)
        {
        }

        public void MusicSetVolume(int value)
        {
            throw new NotImplementedException();
        }

        public void SetMusicMode(MusicMode musicMode)
        {
        }
    }
}
