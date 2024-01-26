using System;
using System.Collections.Generic;
using System.IO;

namespace SharpAlliance.Core;

public class GameSettings
{
    public const string GAME_SETTINGS_FILE = "..\\Ja2.set";
    public const string GAME_INI_FILE = "..\\Ja2.ini";

    //Change this number when we want any who gets the new build to reset the options
    public const int CURRENT_VERSION = 522;
    private static IFileManager files;

    public GameSettings(IFileManager fileManager)
    {
        files = fileManager;
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

    internal static bool SaveGameSettings()
    {
        Stream hFile;
        int uiNumBytesWritten;

        //create the file
        hFile = files.FileOpen(GAME_SETTINGS_FILE, FileAccess.Write, FileMode.OpenOrCreate, false);
        if (hFile == Stream.Null)
        {
            files.FileClose(hFile);
            return (false);
        }



        //Record the current settings into the game settins structure

//        gGameSettings.SoundEffectVolume = GetSoundEffectsVolume();
//        gGameSettings.SpeechVolume = GetSpeechVolume();
//        gGameSettings.MusicVolume = MusicGetVolume();

        gGameSettings.VersionNumber = czVersionNumber;

        gGameSettings.uiSettingsVersionNumber = CURRENT_VERSION;

        //Write the game settings to disk
        // sizeof( GAME_SETTINGS ) = 76
        files.FileWrite(hFile, gGameSettings, 76, out uiNumBytesWritten);
        if (uiNumBytesWritten != 76)
        {
            files.FileClose(hFile);
            return (false);
        }

        files.FileClose(hFile);

        return (true);
    }

    internal static void InitGameSettings()
    {
        //Init the Game Settings
        gGameSettings.bLastSavedGameSlot = -1;
        gGameSettings.MusicVolume = 63;
        gGameSettings.SoundEffectVolume = 63;
        gGameSettings.SpeechVolume = 63;

        //Set the settings
        // SetSoundEffectsVolume(gGameSettings.ubSoundEffectsVolume);
        // SetSpeechVolume(gGameSettings.ubSpeechVolume);
        // MusicSetVolume(gGameSettings.ubMusicVolumeSetting);

        fOptions[TOPTION.SUBTITLES] = true;
        fOptions[TOPTION.SPEECH] = true;
        fOptions[TOPTION.KEY_ADVANCE_SPEECH] = false;
        fOptions[TOPTION.RTCONFIRM] = false;
        fOptions[TOPTION.HIDE_BULLETS] = false;
        fOptions[TOPTION.TRACKING_MODE] = true;
        fOptions[TOPTION.MUTE_CONFIRMATIONS] = false;
        fOptions[TOPTION.ANIMATE_SMOKE] = true;
        fOptions[TOPTION.BLOOD_N_GORE] = true;
        fOptions[TOPTION.DONT_MOVE_MOUSE] = false;
        fOptions[TOPTION.OLD_SELECTION_METHOD] = false;
        fOptions[TOPTION.ALWAYS_SHOW_MOVEMENT_PATH] = false;
        fOptions[TOPTION.SLEEPWAKE_NOTIFICATION] = true;
        fOptions[TOPTION.USE_METRIC_SYSTEM] = false;
        fOptions[TOPTION.MERC_ALWAYS_LIGHT_UP] = false;
        fOptions[TOPTION.SMART_CURSOR] = false;
        fOptions[TOPTION.SNAP_CURSOR_TO_DOOR] = true;
        fOptions[TOPTION.GLOW_ITEMS] = true;
        fOptions[TOPTION.TOGGLE_TREE_TOPS] = true;
        fOptions[TOPTION.TOGGLE_WIREFRAME] = true;
        fOptions[TOPTION.CURSOR_3D] = false;
        // JA2Gold
        fOptions[TOPTION.MERC_CASTS_LIGHT] = true;

        gGameSettings.ubSizeOfDisplayCover = 4;
        gGameSettings.ubSizeOfLOS = 4;

        //Since we just set the settings, save them
        SaveGameSettings();
    }
}
