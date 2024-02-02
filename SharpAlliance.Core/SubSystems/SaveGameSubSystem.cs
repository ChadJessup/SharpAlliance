using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SharpAlliance.Core.Screens;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class SaveGameSubSystem : IDisposable
{
    private readonly IFileManager files;

    public SaveGameSubSystem(IFileManager fileManager)
    {
        this.files = fileManager;
    }

    public void InitGameOptions()
    {
        gGameSettings = new GameSettings(this.files)
        {
            //Init the Game Settings
            bLastSavedGameSlot = -1,
            MusicVolume = 63,
            SoundEffectVolume = 63,
            SpeechVolume = 63
        };

        //Set the settings
        SetSoundEffectsVolume(gGameSettings.SoundEffectVolume);
        SetSpeechVolume(gGameSettings.SpeechVolume);
        //MusicSetVolume(gGameSettings.MusicVolume);
        gGameSettings.MusicVolume = 127;

        gGameSettings[TOPTION.SUBTITLES] = true;
        gGameSettings[TOPTION.SPEECH] = true;
        gGameSettings[TOPTION.KEY_ADVANCE_SPEECH] = false;
        gGameSettings[TOPTION.RTCONFIRM] = false;
        gGameSettings[TOPTION.HIDE_BULLETS] = false;
        gGameSettings[TOPTION.TRACKING_MODE] = true;
        gGameSettings[TOPTION.MUTE_CONFIRMATIONS] = false;
        gGameSettings[TOPTION.ANIMATE_SMOKE] = true;
        gGameSettings[TOPTION.BLOOD_N_GORE] = true;
        gGameSettings[TOPTION.DONT_MOVE_MOUSE] = false;
        gGameSettings[TOPTION.OLD_SELECTION_METHOD] = false;
        gGameSettings[TOPTION.ALWAYS_SHOW_MOVEMENT_PATH] = false;

        gGameSettings[TOPTION.SLEEPWAKE_NOTIFICATION] = true;

        gGameSettings[TOPTION.USE_METRIC_SYSTEM] = false;

        gGameSettings[TOPTION.BLOOD_N_GORE] = false;

        gGameSettings[TOPTION.MERC_ALWAYS_LIGHT_UP] = false;
        gGameSettings[TOPTION.SMART_CURSOR] = false;

        gGameSettings[TOPTION.SNAP_CURSOR_TO_DOOR] = true;
        gGameSettings[TOPTION.GLOW_ITEMS] = true;
        gGameSettings[TOPTION.TOGGLE_TREE_TOPS] = true;
        gGameSettings[TOPTION.TOGGLE_WIREFRAME] = true;
        gGameSettings[TOPTION.CURSOR_3D] = false;
        // JA2Gold
        gGameSettings[TOPTION.MERC_CASTS_LIGHT] = true;

        gGameSettings.ubSizeOfDisplayCover = 4;
        gGameSettings.ubSizeOfLOS = 4;

        //Since we just set the settings, save them
        SaveGameSettings();
    }

    private void SetSpeechVolume(int speechVolume)
    {
        gGameSettings.SpeechVolume = Math.Min(speechVolume, 127);
    }

    private void SetSoundEffectsVolume(int soundEffectVolume)
    {
        gGameSettings.SoundEffectVolume = Math.Min(soundEffectVolume, 127);
    }

    private bool SaveGameSettings()
    {
        //create the file
        var settingsFile = $@"C:\temp\Ja2.json";

        //Record the current settings into the game settins structure

        gGameSettings.VersionNumber = czVersionNumber;
        gGameSettings.uiSettingsVersionNumber = GameSettings.CURRENT_VERSION;

        //Write the game settings to disk

        var settingsJson = JsonSerializer.Serialize<GameSettings>(gGameSettings);
        File.WriteAllText(settingsFile, settingsJson);

        return true;
    }

    public bool LoadGameSettings()
    {
        Stream hFile;
        uint uiNumBytesRead;


        //if the game settings file does NOT exist, or if it is smaller then what it should be
        if (!files.FileExists(GameSettings.GAME_SETTINGS_FILE))
        {
            //Initialize the settings
            GameSettings.InitGameSettings();

            //delete the shade tables aswell
            //DeleteShadeTableDir();
        }
        else
        {
            hFile = files.FileOpen(GameSettings.GAME_SETTINGS_FILE, FileAccess.Read, FileMode.Open, false);
            if (hFile == Stream.Null)
            {
                files.FileClose(hFile);
                GameSettings.InitGameSettings();
                return (false);
            }

            Span<byte> buffer = new byte[76];
            files.FileRead(hFile, buffer, out uiNumBytesRead);
            if (uiNumBytesRead != 76)
            {
                files.FileClose(hFile);
                GameSettings.InitGameSettings();
                return (false);
            }

            files.FileClose(hFile);
        }


        //if the version in the game setting file is older then the we want, init the game settings
        if (gGameSettings.uiSettingsVersionNumber < GameSettings.CURRENT_VERSION)
        {
            //Initialize the settings
            GameSettings.InitGameSettings();

            //delete the shade tables aswell
            //DeleteShadeTableDir();

            return (true);
        }


        //
        //Do checking to make sure the settings are valid
        //
        if (gGameSettings.bLastSavedGameSlot < 0 || gGameSettings.bLastSavedGameSlot >= NUM_SAVE_GAMES)
        {
            gGameSettings.bLastSavedGameSlot = -1;
        }

//        if (gGameSettings.MusicVolume > HIGHVOLUME)
//        {
//            gGameSettings.MusicVolume = MIDVOLUME;
//        }
//
//        if (gGameSettings.SoundEffectVolume > HIGHVOLUME)
//        {
//            gGameSettings.SoundEffectVolume  = MIDVOLUME;
//        }
//
//        if (gGameSettings.SpeechVolume > HIGHVOLUME)
//        {
//            gGameSettings.SpeechVolume = MIDVOLUME;
//        }


        //make sure that at least subtitles or speech is enabled
        if (!GameSettings.fOptions[TOPTION.SUBTITLES] && !GameSettings.fOptions[TOPTION.SPEECH])
        {
            GameSettings.fOptions[TOPTION.SUBTITLES] = true;
            GameSettings.fOptions[TOPTION.SPEECH] = true;
        }


        //
        //	Set the settings
        //

//        SetSoundEffectsVolume(gGameSettings.ubSoundEffectsVolume);
//        SetSpeechVolume(gGameSettings.ubSpeechVolume);
//        MusicSetVolume(gGameSettings.ubMusicVolumeSetting);

        //if the user doesnt want the help screens present
        if (gGameSettings.fHideHelpInAllScreens)
        {
            HelpScreen.gHelpScreen.usHasPlayerSeenHelpScreenInCurrentScreen = true;
        }
        else
        {
            //Set it so that every screens help will come up the first time ( the 'x' will be set )
            // gHelpScreen.usHasPlayerSeenHelpScreenInCurrentScreen = 0xffff;
            HelpScreen.gHelpScreen.usHasPlayerSeenHelpScreenInCurrentScreen = false;
        }

        return true;
    }

    public void Dispose()
    {
    }
}
