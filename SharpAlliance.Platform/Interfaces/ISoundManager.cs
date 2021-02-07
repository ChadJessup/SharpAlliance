using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Platform.Interfaces
{
    public interface ISoundManager : ISharpAllianceManager
    {
        void SoundStopAll();
        ValueTask<bool> InitSound();
        void SetSoundEffectsVolume(int iNewValue);
        void SoundStop(uint uiOptionToggleSound);
        int GetSoundEffectsVolume();
        int GetSpeechVolume();
        void SetSpeechVolume(int iNewValue);
    }
}
