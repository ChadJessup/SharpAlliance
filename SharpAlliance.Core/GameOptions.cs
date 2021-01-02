namespace SharpAlliance.Core
{
    public class GameOptions
    {
        public bool GunNut { get; set; } = false;
        public bool SciFi { get; set; } = true;
        public DifficultyLevel DifficultyLevel { get; set; } = DifficultyLevel.Easy;
        public bool TurnTimeLimit { get; set; } = false;
        public bool IronManMode { get; set; } = false;
    }

    public class GameSettings
    {
        public int LastSavedGameSlot { get; set; }
        public int MusicVolume { get; set; }
        public int SoundEffectVolume { get; set; }
        public int SpeechVolume { get; set; }
        public
    }
}
