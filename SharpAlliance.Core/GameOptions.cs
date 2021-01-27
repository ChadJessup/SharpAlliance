using System;

namespace SharpAlliance.Core
{
    public class GameOptions
    {
        public bool GunNut { get; set; } = false;
        public bool SciFi { get; set; } = true;
        public DifficultyLevel DifficultyLevel { get; set; } = DifficultyLevel.Easy;
        public bool TurnTimeLimit { get; set; } = false;
        public bool IronManMode { get; set; } = false;

        public void InitGameOptions()
        {
        }
    }

    public class GameSettings
    {
        public int LastSavedGameSlot { get; set; }
        public int MusicVolume { get; set; }
        public int SoundEffectVolume { get; set; }
        public int SpeechVolume { get; set; }
        // public Options
        public string VersionNumber { get; set; }
        public int uiSettingsVersionNumber { get; set; }
        public int uiMeanwhileScenesSeenFlags { get; set; }

        public bool fHideHelpInAllScreens { get; set; }
        public bool fUNUSEDPlayerFinishedTheGame { get; set; } // JA2Gold: for UB compatibility
        public int ubSizeOfDisplayCover { get; set; }
        public int ubSizeOfLOS { get; set; }
    }
}
