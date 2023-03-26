using System;
using System.Collections.Generic;

namespace SharpAlliance.Core;

public class GameOptions
{
    public bool GunNut { get; set; } = false;
    public bool SciFi { get; set; } = true;
    public DifficultyLevel ubDifficultyLevel { get; set; } = DifficultyLevel.Easy;
    public bool TurnTimeLimit { get; set; } = false;
    public bool IronManMode { get; set; } = false;

    public void InitGameOptions()
    {
    }
}

//If you add any options, MAKE sure you add the corresponding string to the Options Screen string array
public enum TOPTION
{
    SPEECH,
    MUTE_CONFIRMATIONS,
    SUBTITLES,
    KEY_ADVANCE_SPEECH,
    ANIMATE_SMOKE,
    //	HIDE_BULLETS,
    //	CONFIRM_MOVE,
    BLOOD_N_GORE,
    DONT_MOVE_MOUSE,
    OLD_SELECTION_METHOD,
    ALWAYS_SHOW_MOVEMENT_PATH,


    //	TIME_LIMIT_TURNS,			//moved to the game init screen

    SHOW_MISSES,

    RTCONFIRM,

    // DISPLAY_ENEMY_INDICATOR,
    // Displays the number of enemies seen by the merc, ontop of their portrait
    SLEEPWAKE_NOTIFICATION,

    USE_METRIC_SYSTEM,      //If set, uses the metric system

    MERC_ALWAYS_LIGHT_UP,

    SMART_CURSOR,

    SNAP_CURSOR_TO_DOOR,

    GLOW_ITEMS,
    TOGGLE_TREE_TOPS,
    TOGGLE_WIREFRAME,
    CURSOR_3D,

    NUM_GAME_OPTIONS,               //Toggle up this will be able to be Toggled by the player

    //These options will NOT be toggable by the Player
    MERC_CASTS_LIGHT = NUM_GAME_OPTIONS,
    HIDE_BULLETS,
    TRACKING_MODE,

    NUM_ALL_GAME_OPTIONS,
}

public class GameSettings
{
    public GameSettings()
    {
        foreach (var option in Enum.GetValues<TOPTION>())
        {
            GameSettings.fOptions.TryAdd(option, false);
        }
    }

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

    private static Dictionary<TOPTION, bool> options = new();

    public static Dictionary<TOPTION, bool> fOptions => options;

    public bool this[TOPTION option]
    {
        get => options[option];
        set => options[option] = value;
    }
}
