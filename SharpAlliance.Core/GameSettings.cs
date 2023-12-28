using System;
using System.Collections.Generic;

namespace SharpAlliance.Core;

public class GameSettings
{
    //Change this number when we want any who gets the new build to reset the options
    public const int CURRENT_VERSION = 522;

    public GameSettings()
    {
        foreach (var option in Enum.GetValues<TOPTION>())
        {
            GameSettings.fOptions.TryAdd(option, false);
        }
    }

    public int bLastSavedGameSlot { get; set; }
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

    private static Dictionary<TOPTION, bool> options = new();

    public static Dictionary<TOPTION, bool> fOptions => options;

    public bool this[TOPTION option]
    {
        get => options[option];
        set => options[option] = value;
    }

    internal void SaveGameSettings()
    {
    }
}
