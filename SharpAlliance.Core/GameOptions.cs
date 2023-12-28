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
        Globals.gGameOptions.GunNut = false;
        Globals.gGameOptions.SciFi = true;
        Globals.gGameOptions.ubDifficultyLevel = DifficultyLevel.Easy;
        Globals.gGameOptions.IronManMode = false;
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
