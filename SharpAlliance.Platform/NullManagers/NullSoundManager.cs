using System;
using System.Threading.Tasks;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Platform.NullManagers
{
    public class NullSoundManager : ISoundManager, ISound2dManager, ISound3dManager
    {
        public bool IsInitialized { get; } = true;

        public ValueTask<bool> Initialize()
        {
            return ValueTask.FromResult(true);
        }

        public void Dispose()
        {
        }

        public void SoundStopAll()
        {
            throw new NotImplementedException();
        }

        public ValueTask<bool> InitSound()
        {
            throw new NotImplementedException();
        }

    }
}
