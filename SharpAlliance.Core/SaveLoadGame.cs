﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform.Interfaces;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const string czVersionNumber = "Build 04.12.02";
    public const string zTrackingNumber = "Z";

    public static int guiSaveGameVersion = 0;
    public static bool gMusicModeToPlay = false;
    public static bool gfUseConsecutiveQuickSaveSlots = false;
    public static int guiCurrentQuickSaveNumber = 0;
    public static int guiLastSaveGameNum;
    public static int guiJA2EncryptionSet = 0;
    public static int gubSaveGameLoc = 0;
    public static int guiScreenToGotoAfterLoadingSavedGame = 0;

    public const int NUM_SAVE_GAMES = 11;
    public const int BYTESINMEGABYTE = 1048576; //1024*1024;
    public const int REQUIRED_FREE_SPACE = (20 * BYTESINMEGABYTE);
    public const int SIZE_OF_SAVE_GAME_DESC = 128;
    public const int GAME_VERSION_LENGTH = 16;
    public const int SAVE__ERROR_NUM = 99;
    public const int SAVE__END_TURN_NUM = 98;

    public static bool gfRedrawSaveLoadScreen = true;

    //  Keeps track of the saved game version.  Increment the saved game version whenever 
    //  you will invalidate the saved game file
    public const int SAVE_GAME_VERSION = 99;
    public const int guiSavedGameVersion = SAVE_GAME_VERSION;

}

public class SaveLoadGame
{
    private readonly ILogger<SaveLoadGame> logger;
    private readonly IFileManager files;
    private readonly IVideoManager video;

    public SaveLoadGame(
        ILogger<SaveLoadGame> logger,
        IFileManager fileManager,
        IVideoManager videoManager)
    {
        this.logger = logger;
        this.files = fileManager;
        this.video = videoManager;
    }

    bool SaveGame(int ubSaveGameID, string pGameDesc)
    {
        int uiNumBytesWritten = 0;
        Stream hFile = Stream.Null;
        SAVED_GAME_HEADER SaveGameHeader;
        string zSaveGameName = string.Empty;// [512];
        int uiSizeOfGeneralInfo = Marshal.SizeOf<GENERAL_SAVE_INFO>();
        string saveDir = string.Empty;
        bool fPausedStateBeforeSaving = gfGamePaused;
        bool fLockPauseStateBeforeSaving = gfLockPauseState;
        int iSaveLoadGameMessageBoxID = -1;
        int usPosX, usActualWidth, usActualHeight;
        bool fWePausedIt = false;


        //        sprintf(saveDir, "%S", pMessageStrings[MGS.SAVEDIRECTORY]);

        if (ubSaveGameID >= NUM_SAVE_GAMES
            && ubSaveGameID != SAVE__ERROR_NUM
            && ubSaveGameID != SAVE__END_TURN_NUM)
        {
            return (false);     //ddd
        }

        //clear out the save game header
        SaveGameHeader = new();

        if (!GameClock.GamePaused())
        {
            PauseBeforeSaveGame();
            fWePausedIt = true;
        }

        //Place a message on the screen telling the user that we are saving the game
        iSaveLoadGameMessageBoxID = MercTextBox.PrepareMercPopupBox(iSaveLoadGameMessageBoxID, MercTextBoxBackground.BASIC_MERC_POPUP_BACKGROUND, MercTextBoxBorder.BASIC_MERC_POPUP_BORDER,
            EnglishText.zSaveLoadText[(int)SLG.SAVING_GAME_MESSAGE], 300, 0, 0, 0, out usActualWidth, out usActualHeight);
        usPosX = (640 - usActualWidth) / 2;

        MercTextBox.RenderMercPopUpBoxFromIndex(iSaveLoadGameMessageBoxID, usPosX, 160, Surfaces.FRAME_BUFFER);

        this.video.InvalidateRegion(new(0, 0, 640, 480));

        this.video.ExecuteBaseDirtyRectQueue();
        this.video.EndFrameBufferRender();
        this.video.RefreshScreen();

        if (MercTextBox.RemoveMercPopupBoxFromIndex(iSaveLoadGameMessageBoxID))
        {
            iSaveLoadGameMessageBoxID = -1;
        }

        //
        // make sure we redraw the screen when we are done
        //

        //if we are in the game screen
        if (guiCurrentScreen == ScreenName.GAME_SCREEN)
        {
            RenderWorld.SetRenderFlags(RenderingFlags.FULL);
        }
        else if (guiCurrentScreen == ScreenName.MAP_SCREEN)
        {
            fMapPanelDirty = true;
            fTeamPanelDirty = true;
            fCharacterInfoPanelDirty = true;
        }
        else if (guiCurrentScreen == ScreenName.SAVE_LOAD_SCREEN)
        {
            gfRedrawSaveLoadScreen = true;
        }

        gubSaveGameLoc = ubSaveGameID;


        //Set the fact that we are saving a game
        gTacticalStatus.uiFlags |= TacticalEngineStatus.LOADING_SAVED_GAME;


        //Save the current sectors open temp files to the disk
        if (!TacticalSaveSubSystem.SaveCurrentSectorsInformationToTempItemFile())
        {
            Messages.ScreenMsg(FontColor.FONT_MCOLOR_WHITE, MSG.TESTVERSION, "ERROR in SaveCurrentSectorsInformationToTempItemFile()");
            goto FAILED_TO_SAVE;
        }

        //if we are saving the quick save,
        if (ubSaveGameID == 0)
        {
            pGameDesc = wprintf(EnglishText.pMessageStrings[MSG.QUICKSAVE_NAME]);
        }

        //If there was no string, add one
        if (pGameDesc[0] == '\0')
        {
            pGameDesc = wcscpy(EnglishText.pMessageStrings[MSG.NODESC]);
        }

        //Check to see if the save directory exists
        if (!this.files.FileExists(saveDir))
        {
            //ok the direcotry doesnt exist, create it
            if (!this.files.MakeFileManDirectory(saveDir))
            {
                goto FAILED_TO_SAVE;
            }
        }

        //Create the name of the file
        CreateSavedGameFileNameFromNumber(ubSaveGameID, zSaveGameName);

        //if the file already exists, delete it
        if (this.files.FileExists(zSaveGameName))
        {
            if (!this.files.FileDelete(zSaveGameName))
            {
                goto FAILED_TO_SAVE;
            }
        }

        // create the save game file
        hFile = this.files.FileOpen(zSaveGameName, FileAccess.ReadWrite, false);
        if (!hFile.CanWrite)
        {
            goto FAILED_TO_SAVE;
        }

        //
        // If there are no enemy or civilians to save, we have to check BEFORE savinf the sector info struct because
        // the NewWayOfSavingEnemyAndCivliansToTempFile will RESET the civ or enemy flag AFTER they have been saved. 
        //
        EnemySoldierSave.NewWayOfSavingEnemyAndCivliansToTempFile(gWorldSectorX, gWorldSectorY, gbWorldSectorZ, true, true);
        EnemySoldierSave.NewWayOfSavingEnemyAndCivliansToTempFile(gWorldSectorX, gWorldSectorY, gbWorldSectorZ, false, true);

        //
        // Setup the save game header
        //

        SaveGameHeader.uiSavedGameVersion = guiSavedGameVersion;
        SaveGameHeader.sSavedGameDesc = wcscpy(pGameDesc);
        SaveGameHeader.zGameVersionNumber = czVersionNumber;

        //SaveGameHeader.uiFlags;

        //The following will be used to quickly access info to display in the save/load screen
        SaveGameHeader.uiDay = GameClock.GetWorldDay();
        SaveGameHeader.ubHour = GameClock.GetWorldHour();
        SaveGameHeader.ubMin = guiMin;

        //copy over the initial game options
        // memcpy(&SaveGameHeader.sInitialGameOptions, &gGameOptions, Marshal.SizeOf<GAME_OPTIONS>());

        //Get the sector value to save.
        GetBestPossibleSectorXYZValues(out SaveGameHeader.sSectorX, out SaveGameHeader.sSectorY, out SaveGameHeader.bSectorZ);

        /*

            //if the current sector is valid
            if( gfWorldLoaded )
            {
                SaveGameHeader.sSectorX = gWorldSectorX;
                SaveGameHeader.sSectorY = gWorldSectorY;
                SaveGameHeader.bSectorZ = gbWorldSectorZ;
            }
            else if( Squad[ iCurrentTacticalSquad ][ 0 ] && iCurrentTacticalSquad != NO_CURRENT_SQUAD )
            {
        //		if( Squad[ iCurrentTacticalSquad ][ 0 ].bAssignment != IN_TRANSIT )
                {
                    SaveGameHeader.sSectorX = Squad[ iCurrentTacticalSquad ][ 0 ].sSectorX;
                    SaveGameHeader.sSectorY = Squad[ iCurrentTacticalSquad ][ 0 ].sSectorY;
                    SaveGameHeader.bSectorZ = Squad[ iCurrentTacticalSquad ][ 0 ].bSectorZ;
                }
            }
            else
            {
                int					sSoldierCnt;
                SOLDIERTYPE		*pSoldier;
                int					bLastTeamID;
                int					bCount=0;
                bool				fFoundAMerc=false;

                // Set locator to first merc
                sSoldierCnt = gTacticalStatus.Team[ gbPlayerNum ].bFirstID;
                bLastTeamID = gTacticalStatus.Team[ gbPlayerNum ].bLastID;

                for ( pSoldier = MercPtrs[ sSoldierCnt ]; sSoldierCnt <= bLastTeamID; sSoldierCnt++,pSoldier++)
                {
                    if( pSoldier.bActive )
                    {
                        if ( pSoldier.bAssignment != IN_TRANSIT && !pSoldier.fBetweenSectors)
                        {
                            SaveGameHeader.sSectorX = pSoldier.sSectorX;
                            SaveGameHeader.sSectorY = pSoldier.sSectorY;
                            SaveGameHeader.bSectorZ = pSoldier.bSectorZ;
                            fFoundAMerc = true;
                            break;
                        }
                    }
                }

                if( !fFoundAMerc )
                {
                    SaveGameHeader.sSectorX = gWorldSectorX;
                    SaveGameHeader.sSectorY = gWorldSectorY;
                    SaveGameHeader.bSectorZ = gbWorldSectorZ;
                }
            }
        */

        SaveGameHeader.ubNumOfMercsOnPlayersTeam = NumberOfMercsOnPlayerTeam();
        SaveGameHeader.iCurrentBalance = LaptopSaveInfo.iCurrentBalance;


        SaveGameHeader.uiCurrentScreen = guiPreviousOptionScreen;

        SaveGameHeader.fAlternateSector = TacticalSaveSubSystem.GetSectorFlagStatus(gWorldSectorX, gWorldSectorY, gbWorldSectorZ, SF.USE_ALTERNATE_MAP);

        if (gfWorldLoaded)
        {
            SaveGameHeader.fWorldLoaded = true;
            SaveGameHeader.ubLoadScreenID = GetLoadScreenID(gWorldSectorX, gWorldSectorY, gbWorldSectorZ);
        }
        else
        {
            SaveGameHeader.fWorldLoaded = false;
            SaveGameHeader.ubLoadScreenID = 0;
        }

        SaveGameHeader.uiRandom = Globals.Random.Next(RAND_MAX);

        //
        // Save the Save Game header file
        //


        this.files.FileWrite(hFile, SaveGameHeader, Marshal.SizeOf<SAVED_GAME_HEADER>(), out uiNumBytesWritten);
        if (uiNumBytesWritten != Marshal.SizeOf<SAVED_GAME_HEADER>())
        {
            goto FAILED_TO_SAVE;
        }

        guiJA2EncryptionSet = CalcJA2EncryptionSet(SaveGameHeader);

        //
        //Save the gTactical Status array, plus the curent secotr location
        //
        if (!SaveTacticalStatusToSavedGame(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        // save the game clock info
        if (!SaveGameClock(hFile, fPausedStateBeforeSaving, fLockPauseStateBeforeSaving))
        {
            goto FAILED_TO_SAVE;
        }

        // save the strategic events
        if (!SaveStrategicEventsToSavedGame(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveLaptopInfoToSavedGame(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        //
        // Save the merc profiles
        //
        if (!SaveMercProfiles(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        //
        // Save the soldier structure
        //
        if (!SaveSoldierStructure(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        //Save the Finaces Data file 
        if (!SaveFilesToSavedGame(FINANCES_DATA_FILE, hFile))
        {
            goto FAILED_TO_SAVE;
        }

        //Save the history file
        if (!SaveFilesToSavedGame(HISTORY_DATA_FILE, hFile))
        {
            goto FAILED_TO_SAVE;
        }

        //Save the Laptop File file
        if (!SaveFilesToSavedGame(FILES_DAT_FILE, hFile))
        {
            goto FAILED_TO_SAVE;
        }

        //Save email stuff to save file
        if (!SaveEmailToSavedGame(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        //Save the strategic information
        if (!SaveStrategicInfoToSavedFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        //save the underground information
        if (!SaveUnderGroundSectorInfoToSaveGame(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        //save the squad info
        if (!SaveSquadInfoToSavedGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveStrategicMovementGroupsToSaveGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        //Save all the map temp files from the maps\temp directory into the saved game file
        if (!SaveMapTempFilesToSavedGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveQuestInfoToSavedGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveOppListInfoToSavedGame(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveMapScreenMessagesToSaveGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveNPCInfoToSaveGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveKeyTableToSaveGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveTempNpcQuoteArrayToSaveGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SavePreRandomNumbersToSaveGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveSmokeEffectsToSaveGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveArmsDealerInventoryToSaveGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveGeneralInfo(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveMineStatusToSaveGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveStrategicTownLoyaltyToSaveGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveVehicleInformationToSaveGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveBulletStructureToSaveGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SavePhysicsTableToSaveGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveAirRaidInfoToSaveGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveTeamTurnsToTheSaveGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveExplosionTableToSaveGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveCreatureDirectives(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveStrategicStatusToSaveGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveStrategicAI(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveLightEffectsToSaveGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveWatchedLocsToSavedGame(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveItemCursorToSavedGame(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveCivQuotesToSaveGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveBackupNPCInfoToSaveGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        if (!SaveMeanwhileDefsFromSaveGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        // save meanwhiledefs

        if (!SaveSchedules(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        // Save extra vehicle info
        if (!NewSaveVehicleMovementInfoToSavedGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        // Save contract renewal sequence stuff
        if (!MercContract.SaveContractRenewalDataToSaveGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        // Save leave list stuff
        if (!SaveLeaveItemList(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        //do the new way of saving bobbyr mail order items
        if (!NewWayOfSavingBobbyRMailOrdersToSaveGameFile(hFile))
        {
            goto FAILED_TO_SAVE;
        }

        //sss

        //Close the saved game file
        files.FileClose(hFile);


        //if we succesfully saved the game, mark this entry as the last saved game file
        if (ubSaveGameID != SAVE__ERROR_NUM && ubSaveGameID != SAVE__END_TURN_NUM)
        {
            gGameSettings.bLastSavedGameSlot = ubSaveGameID;
        }

        //Save the save game settings
        SaveGameSettings();

        //
        // Display a screen message that the save was succesful
        //

        //if its the quick save slot
        if (ubSaveGameID == 0)
        {
            Messages.ScreenMsg(FontColor.FONT_MCOLOR_WHITE, MSG.INTERFACE, EnglishText.pMessageStrings[MSG.SAVESUCCESS]);
        }
        //#if JA2BETAVERSION
        else if (ubSaveGameID == SAVE__END_TURN_NUM)
        {
            //		ScreenMsg( FONT_MCOLOR_WHITE, MGS.INTERFACE, pMessageStrings[ MGS.END_TURN_AUTO_SAVE ] );
        }
        //#endif
        else
        {
            Messages.ScreenMsg(FontColor.FONT_MCOLOR_WHITE, MSG.INTERFACE, EnglishText.pMessageStrings[MSG.SAVESLOTSUCCESS]);
        }

        //restore the music mode
        SetMusicMode(gubMusicMode);

        //Unset the fact that we are saving a game
        gTacticalStatus.uiFlags &= ~TacticalEngineStatus.LOADING_SAVED_GAME;

        UnPauseAfterSaveGame();

#if JA2BETAVERSION
    InitShutDownMapTempFileTest(false, "SaveMapTempFile", ubSaveGameID);
#endif

#if JA2BETAVERSION
    ValidateSoldierInitLinks(2);
#endif

        //Check for enough free hard drive space
        NextLoopCheckForEnoughFreeHardDriveSpace();

        return (true);

    //if there is an error saving the game
    FAILED_TO_SAVE:

#if JA2BETAVERSION
    SaveGameFilePosition(FileGetPos(hFile), "Failed to Save!!!");
#endif

        this.files.FileClose(hFile);

        if (fWePausedIt)
        {
            UnPauseAfterSaveGame();
        }

        //Delete the failed attempt at saving
        DeleteSaveGameNumber(ubSaveGameID);

        //Put out an error message
        Messages.ScreenMsg(FontColor.FONT_MCOLOR_WHITE, MSG.INTERFACE, zSaveLoadText[SLG.SAVE_GAME_ERROR]);

#if JA2BETAVERSION
    InitShutDownMapTempFileTest(false, "SaveMapTempFile", ubSaveGameID);
#endif

        //Check for enough free hard drive space
        NextLoopCheckForEnoughFreeHardDriveSpace();

#if JA2BETAVERSION
    if (fDisableDueToBattleRoster || fDisableMapInterfaceDueToBattle)
    {
        gubReportMapscreenLock = 2;
    }
#endif

        return (false);
    }



    int guiBrokenSaveGameVersion = 0;

    bool LoadSavedGame(int ubSavedGameID)
    {
        Stream hFile;
        SAVED_GAME_HEADER SaveGameHeader;
        int uiNumBytesRead = 0;

        int sLoadSectorX;
        MAP_ROW sLoadSectorY;
        int bLoadSectorZ;
        string zSaveGameName;// [512];
        int uiSizeOfGeneralInfo = Marshal.SizeOf<GENERAL_SAVE_INFO>();

        int uiRelStartPerc;
        int uiRelEndPerc;

#if JA2BETAVERSION
    gfDisplaySaveGamesNowInvalidatedMsg = false;
#endif

        uiRelStartPerc = uiRelEndPerc = 0;

        TrashAllSoldiers();

        //Empty the dialogue Queue cause someone could still have a quote in waiting
        EmptyDialogueQueue();

        //If there is someone talking, stop them
        StopAnyCurrentlyTalkingSpeech();

        ZeroAnimSurfaceCounts();

        ShutdownNPCQuotes();

        //is it a valid save number
        if (ubSavedGameID >= NUM_SAVE_GAMES)
        {
            if (ubSavedGameID != SAVE__END_TURN_NUM)
            {
                return (false);
            }
        }
        else if (!gbSaveGameArray[ubSavedGameID])
        {
            return (false);
        }

        //Used in mapescreen to disable the annoying 'swoosh' transitions
        gfDontStartTransitionFromLaptop = true;

        // Reset timer callbacks
        gpCustomizableTimerCallback = null;

        gubSaveGameLoc = ubSavedGameID;

        //Set the fact that we are loading a saved game
        gTacticalStatus.uiFlags |= LOADING_SAVED_GAME;

        //Trash the existing world.  This is done to ensure that if we are loading a game that doesn't have 
        //a world loaded, that we trash it beforehand -- else the player could theoretically enter that sector
        //where it would be in a pre-load state.  
        TrashWorld();


        //Deletes all the Temp files in the Maps\Temp directory
        InitTacticalSave(true);

        // ATE; Added to empry dialogue q
        EmptyDialogueQueue();

        //Create the name of the file
        CreateSavedGameFileNameFromNumber(ubSavedGameID, zSaveGameName);

        // open the save game file
        hFile = this.files.FileOpen(zSaveGameName, FileAccess.Read, false);
        if (!hFile.CanRead)
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }

        //Load the Save Game header file
        this.files.FileRead(hFile, ref SaveGameHeader, Marshal.SizeOf<SAVED_GAME_HEADER>(), out uiNumBytesRead);
        if (uiNumBytesRead != Marshal.SizeOf<SAVED_GAME_HEADER>())
        {
            this.files.FileClose(hFile);
            return (false);
        }

        guiJA2EncryptionSet = CalcJA2EncryptionSet(SaveGameHeader);

        guiBrokenSaveGameVersion = SaveGameHeader.uiSavedGameVersion;

        //if the player is loading up an older version of the game, and the person DOESNT have the cheats on, 
        if (SaveGameHeader.uiSavedGameVersion < 65 && !CHEATER_CHEAT_LEVEL())
        {
            //Fail loading the save
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }

        //Store the loading screenID that was saved
        gubLastLoadingScreenID = SaveGameHeader.ubLoadScreenID;

        //HACK
        guiSaveGameVersion = SaveGameHeader.uiSavedGameVersion;

        /*
            if( !LoadGeneralInfo( hFile ) )
            {
                FileClose( hFile );
                return(false);
            }
            #if JA2BETAVERSION
                LoadGameFilePosition( FileGetPos( hFile ), "Misc info" );
            #endif
        */



        //Load the gtactical status structure plus the current sector x,y,z
        if (!LoadTacticalStatusFromSavedGame(hFile))
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }
#if JA2BETAVERSION
    LoadGameFilePosition(FileGetPos(hFile), "Tactical Status");
#endif


        //This gets reset by the above function
        gTacticalStatus.uiFlags |= TacticalEngineStatus.LOADING_SAVED_GAME;


        //Load the game clock ingo
        if (!LoadGameClock(hFile))
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }
#if JA2BETAVERSION
    LoadGameFilePosition(FileGetPos(hFile), "Game Clock");
#endif


        //if we are suppose to use the alternate sector
        if (SaveGameHeader.fAlternateSector)
        {
            TacticalSaveSubSystem.SetSectorFlag(gWorldSectorX, gWorldSectorY, gbWorldSectorZ, SF.USE_ALTERNATE_MAP);
            gfUseAlternateMap = true;
        }


        //if the world was loaded when saved, reload it, otherwise dont
        if (SaveGameHeader.fWorldLoaded || SaveGameHeader.uiSavedGameVersion < 50)
        {
            //Get the current world sector coordinates
            sLoadSectorX = gWorldSectorX;
            sLoadSectorY = gWorldSectorY;
            bLoadSectorZ = gbWorldSectorZ;

            // This will guarantee that the sector will be loaded
            gbWorldSectorZ = -1;


            //if we should load a sector ( if the person didnt just start the game game )
            if ((gWorldSectorX != 0) && (gWorldSectorY != 0))
            {
                //Load the sector
                StrategicMap.SetCurrentWorldSector(sLoadSectorX, sLoadSectorY, bLoadSectorZ);
            }
        }
        else
        { //By clearing these values, we can avoid "in sector" checks -- at least, that's the theory.
            gWorldSectorX = 0;
            gWorldSectorY = 0;

            //Since there is no 
            if (SaveGameHeader.sSectorX == -1 || SaveGameHeader.sSectorY == MAP_ROW.UNSET || SaveGameHeader.bSectorZ == -1)
                gubLastLoadingScreenID = LOADINGSCREEN_HELI;
            else
                gubLastLoadingScreenID = GetLoadScreenID(SaveGameHeader.sSectorX, SaveGameHeader.sSectorY, SaveGameHeader.bSectorZ);

            BeginLoadScreen();
        }

        CreateLoadingScreenProgressBar();
        SetProgressBarColor(0, 0, 0, 150);

        uiRelStartPerc = 0;

        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Strategic Events...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;

        //load the game events
        if (!LoadStrategicEventsFromSavedGame(hFile))
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }

        uiRelEndPerc += 0;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Laptop Info");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;



        if (!LoadLaptopInfoFromSavedGame(hFile))
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }

        uiRelEndPerc += 0;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Merc Profiles...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;

        //
        // Load all the saved Merc profiles
        //
        if (!LoadSavedMercProfiles(hFile))
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }

        uiRelEndPerc += 30;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Soldier Structure...");
        uiRelStartPerc = uiRelEndPerc;


        //
        // Load the soldier structure info
        // 
        if (!LoadSoldierStructure(hFile))
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }

        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Finances Data File...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;


        //
        // Load the Finances Data and write it to a new file
        //
        if (!LoadFilesFromSavedGame(FINANCES_DATA_FILE, hFile))
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }
#if JA2BETAVERSION
    LoadGameFilePosition(FileGetPos(hFile), "Finances Data File");
#endif




        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "History File...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;


        //
        // Load the History Data and write it to a new file
        //
        if (!LoadFilesFromSavedGame(HISTORY_DATA_FILE, hFile))
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }
#if JA2BETAVERSION
    LoadGameFilePosition(FileGetPos(hFile), "History File");
#endif




        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "The Laptop FILES file...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;


        //
        // Load the Files Data and write it to a new file
        //
        if (!LoadFilesFromSavedGame(FILES_DAT_FILE, hFile))
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }
#if JA2BETAVERSION
    LoadGameFilePosition(FileGetPos(hFile), "The Laptop FILES file");
#endif



        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Email...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;


        // Load the data for the emails
        if (!LoadEmailFromSavedGame(hFile))
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }
#if JA2BETAVERSION
    LoadGameFilePosition(FileGetPos(hFile), "Email");
#endif



        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Strategic Information...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;



        //Load the strategic Information
        if (!LoadStrategicInfoFromSavedFile(hFile))
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }
#if JA2BETAVERSION
    LoadGameFilePosition(FileGetPos(hFile), "Strategic Information");
#endif



        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "UnderGround Information...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;


        //Load the underground information
        if (!LoadUnderGroundSectorInfoFromSavedGame(hFile))
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }
#if JA2BETAVERSION
    LoadGameFilePosition(FileGetPos(hFile), "UnderGround Information");
#endif


        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Squad Info...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;


        // Load all the squad info from the saved game file 
        if (!LoadSquadInfoFromSavedGameFile(hFile))
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }
#if JA2BETAVERSION
    LoadGameFilePosition(FileGetPos(hFile), "Squad Info");
#endif


        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Strategic Movement Groups...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;


        //Load the group linked list
        if (!LoadStrategicMovementGroupsFromSavedGameFile(hFile))
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }
#if JA2BETAVERSION
    LoadGameFilePosition(FileGetPos(hFile), "Strategic Movement Groups");
#endif


        uiRelEndPerc += 30;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "All the Map Temp files...");
        uiRelStartPerc = uiRelEndPerc;


        // Load all the map temp files from the saved game file into the maps\temp directory
        if (!LoadMapTempFilesFromSavedGameFile(hFile))
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }
#if JA2BETAVERSION
    LoadGameFilePosition(FileGetPos(hFile), "All the Map Temp files");
#endif



        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Quest Info...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;



        if (!LoadQuestInfoFromSavedGameFile(hFile))
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }
#if JA2BETAVERSION
    LoadGameFilePosition(FileGetPos(hFile), "Quest Info");
#endif


        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "OppList Info...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;


        if (!LoadOppListInfoFromSavedGame(hFile))
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }
#if JA2BETAVERSION
    LoadGameFilePosition(FileGetPos(hFile), "OppList Info");
#endif




        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "MapScreen Messages...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;



        if (!LoadMapScreenMessagesFromSaveGameFile(hFile))
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }
#if JA2BETAVERSION
    LoadGameFilePosition(FileGetPos(hFile), "MapScreen Messages");
#endif



        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "NPC Info...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;



        if (!LoadNPCInfoFromSavedGameFile(hFile, SaveGameHeader.uiSavedGameVersion))
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }
#if JA2BETAVERSION
    LoadGameFilePosition(FileGetPos(hFile), "NPC Info");
#endif



        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "KeyTable...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;



        if (!LoadKeyTableFromSaveedGameFile(hFile))
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }
#if JA2BETAVERSION
    LoadGameFilePosition(FileGetPos(hFile), "KeyTable");
#endif



        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Npc Temp Quote File...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;


        if (!LoadTempNpcQuoteArrayToSaveGameFile(hFile))
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }
#if JA2BETAVERSION
    LoadGameFilePosition(FileGetPos(hFile), "Npc Temp Quote File");
#endif




        uiRelEndPerc += 0;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "PreGenerated Random Files...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;


        if (!LoadPreRandomNumbersFromSaveGameFile(hFile))
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }
#if JA2BETAVERSION
    LoadGameFilePosition(FileGetPos(hFile), "PreGenerated Random Files");
#endif




        uiRelEndPerc += 0;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Smoke Effect Structures...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;



        if (!LoadSmokeEffectsFromLoadGameFile(hFile))
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }
#if JA2BETAVERSION
    LoadGameFilePosition(FileGetPos(hFile), "Smoke Effect Structures");
#endif



        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Arms Dealers Inventory...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;



        if (!LoadArmsDealerInventoryFromSavedGameFile(hFile, (bool)(SaveGameHeader.uiSavedGameVersion >= 54), (bool)(SaveGameHeader.uiSavedGameVersion >= 55)))
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }
#if JA2BETAVERSION
    LoadGameFilePosition(FileGetPos(hFile), "Arms Dealers Inventory");
#endif




        uiRelEndPerc += 0;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Misc info...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;




        if (!LoadGeneralInfo(hFile))
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }
#if JA2BETAVERSION
    LoadGameFilePosition(FileGetPos(hFile), "Misc info");
#endif




        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Mine Status...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;



        if (!LoadMineStatusFromSavedGameFile(hFile))
        {
            this.files.FileClose(hFile);
            guiSaveGameVersion = 0;
            return (false);
        }
#if JA2BETAVERSION
    LoadGameFilePosition(FileGetPos(hFile), "Mine Status");
#endif




        uiRelEndPerc += 0;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Town Loyalty...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;




        if (SaveGameHeader.uiSavedGameVersion >= 21)
        {
            if (!LoadStrategicTownLoyaltyFromSavedGameFile(hFile))
            {
                this.files.FileClose(hFile);
                guiSaveGameVersion = 0;
                return (false);
            }
#if JA2BETAVERSION
        LoadGameFilePosition(FileGetPos(hFile), "Town Loyalty");
#endif
        }





        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Vehicle Information...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;



        if (SaveGameHeader.uiSavedGameVersion >= 22)
        {
            if (!LoadVehicleInformationFromSavedGameFile(hFile, SaveGameHeader.uiSavedGameVersion))
            {
                this.files.FileClose(hFile);
                guiSaveGameVersion = 0;
                return (false);
            }
#if JA2BETAVERSION
        LoadGameFilePosition(FileGetPos(hFile), "Vehicle Information");
#endif
        }



        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Bullet Information...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;



        if (SaveGameHeader.uiSavedGameVersion >= 24)
        {
            if (!LoadBulletStructureFromSavedGameFile(hFile))
            {
                this.files.FileClose(hFile);
                guiSaveGameVersion = 0;
                return (false);
            }
#if JA2BETAVERSION
        LoadGameFilePosition(FileGetPos(hFile), "Bullet Information");
#endif
        }




        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Physics table...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;




        if (SaveGameHeader.uiSavedGameVersion >= 24)
        {
            if (!LoadPhysicsTableFromSavedGameFile(hFile))
            {
                this.files.FileClose(hFile);
                guiSaveGameVersion = 0;
                return (false);
            }
#if JA2BETAVERSION
        LoadGameFilePosition(FileGetPos(hFile), "Physics table");
#endif
        }




        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Air Raid Info...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;



        if (SaveGameHeader.uiSavedGameVersion >= 24)
        {
            if (!LoadAirRaidInfoFromSaveGameFile(hFile))
            {
                this.files.FileClose(hFile);
                guiSaveGameVersion = 0;
                return (false);
            }
#if JA2BETAVERSION
        LoadGameFilePosition(FileGetPos(hFile), "Air Raid Info");
#endif
        }



        uiRelEndPerc += 0;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Team Turn Info...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;



        if (SaveGameHeader.uiSavedGameVersion >= 24)
        {
            if (!LoadTeamTurnsFromTheSavedGameFile(hFile))
            {
                this.files.FileClose(hFile);
                guiSaveGameVersion = 0;
                return (false);
            }
#if JA2BETAVERSION
        LoadGameFilePosition(FileGetPos(hFile), "Team Turn Info");
#endif
        }




        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Explosion Table...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;



        if (SaveGameHeader.uiSavedGameVersion >= 25)
        {
            if (!LoadExplosionTableFromSavedGameFile(hFile))
            {
                this.files.FileClose(hFile);
                guiSaveGameVersion = 0;
                return (false);
            }
#if JA2BETAVERSION
        LoadGameFilePosition(FileGetPos(hFile), "Explosion Table");
#endif
        }




        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Creature Spreading...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;




        if (SaveGameHeader.uiSavedGameVersion >= 27)
        {
            if (!LoadCreatureDirectives(hFile, SaveGameHeader.uiSavedGameVersion))
            {
                this.files.FileClose(hFile);
                guiSaveGameVersion = 0;
                return (false);
            }
#if JA2BETAVERSION
        LoadGameFilePosition(FileGetPos(hFile), "Creature Spreading");
#endif
        }




        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Strategic Status...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;




        if (SaveGameHeader.uiSavedGameVersion >= 28)
        {
            if (!LoadStrategicStatusFromSaveGameFile(hFile))
            {
                this.files.FileClose(hFile);
                guiSaveGameVersion = 0;
                return (false);
            }
#if JA2BETAVERSION
        LoadGameFilePosition(FileGetPos(hFile), "Strategic Status");
#endif
        }



        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Strategic AI...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;



        if (SaveGameHeader.uiSavedGameVersion >= 31)
        {
            if (!LoadStrategicAI(hFile))
            {
                this.files.FileClose(hFile);
                guiSaveGameVersion = 0;
                return (false);
            }
#if JA2BETAVERSION
        LoadGameFilePosition(FileGetPos(hFile), "Strategic AI");
#endif
        }



        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Lighting Effects...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;



        if (SaveGameHeader.uiSavedGameVersion >= 37)
        {
            if (!LoadLightEffectsFromLoadGameFile(hFile))
            {
                this.files.FileClose(hFile);
                guiSaveGameVersion = 0;
                return (false);
            }
        }

        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Watched Locs Info...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;



        if (SaveGameHeader.uiSavedGameVersion >= 38)
        {
            if (!LoadWatchedLocsFromSavedGame(hFile))
            {
                this.files.FileClose(hFile);
                guiSaveGameVersion = 0;
                return (false);
            }
        }
#if JA2BETAVERSION
    LoadGameFilePosition(FileGetPos(hFile), "Watched Locs Info");
#endif




        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Item cursor Info...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;



        if (SaveGameHeader.uiSavedGameVersion >= 39)
        {
            if (!LoadItemCursorFromSavedGame(hFile))
            {
                this.files.FileClose(hFile);
                guiSaveGameVersion = 0;
                return (false);
            }
        }
#if JA2BETAVERSION
    LoadGameFilePosition(FileGetPos(hFile), "Item cursor Info");
#endif



        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Civ Quote System...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;



        if (SaveGameHeader.uiSavedGameVersion >= 51)
        {
            if (!LoadCivQuotesFromLoadGameFile(hFile))
            {
                this.files.FileClose(hFile);
                guiSaveGameVersion = 0;
                return false;
            }
        }
#if JA2BETAVERSION
    LoadGameFilePosition(FileGetPos(hFile), "Civ Quote System");
#endif




        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Backed up NPC Info...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;



        if (SaveGameHeader.uiSavedGameVersion >= 53)
        {
            if (!LoadBackupNPCInfoFromSavedGameFile(hFile, SaveGameHeader.uiSavedGameVersion))
            {
                this.files.FileClose(hFile);
                guiSaveGameVersion = 0;
                return (false);
            }
        }

        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Meanwhile definitions...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;



        if (SaveGameHeader.uiSavedGameVersion >= 58)
        {
            if (!LoadMeanwhileDefsFromSaveGameFile(hFile))
            {
                this.files.FileClose(hFile);
                guiSaveGameVersion = 0;
                return (false);
            }
        }
        else
        {
//            memcpy(gMeanwhileDef[gCurrentMeanwhileDef.ubMeanwhileID], &gCurrentMeanwhileDef, Marshal.SizeOf<MEANWHILE_DEFINITION>());
        }




        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Schedules...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;



        if (SaveGameHeader.uiSavedGameVersion >= 59)
        {
            // trash schedules loaded from map
            DestroyAllSchedulesWithoutDestroyingEvents();
            if (!LoadSchedulesFromSave(hFile))
            {
                this.files.FileClose(hFile);
                guiSaveGameVersion = 0;
                return (false);
            }
        }

        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Extra Vehicle Info...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;



        if (SaveGameHeader.uiSavedGameVersion >= 61)
        {
            if (SaveGameHeader.uiSavedGameVersion < 84)
            {
                if (!LoadVehicleMovementInfoFromSavedGameFile(hFile))
                {
                    this.files.FileClose(hFile);
                    guiSaveGameVersion = 0;
                    return (false);
                }
            }
            else
            {
                if (!NewLoadVehicleMovementInfoFromSavedGameFile(hFile))
                {
                    this.files.FileClose(hFile);
                    guiSaveGameVersion = 0;
                    return (false);
                }
            }
        }

        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Contract renweal sequence stuff...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;



        if (SaveGameHeader.uiSavedGameVersion < 62)
        {
            // the older games had a bug where this flag was never being set
            gfResetAllPlayerKnowsEnemiesFlags = true;
        }

        if (SaveGameHeader.uiSavedGameVersion >= 67)
        {
            if (!LoadContractRenewalDataFromSaveGameFile(hFile))
            {
                this.files.FileClose(hFile);
                guiSaveGameVersion = 0;
                return (false);
            }
        }


        if (SaveGameHeader.uiSavedGameVersion >= 70)
        {
            if (!LoadLeaveItemList(hFile))
            {
                this.files.FileClose(hFile);
                guiSaveGameVersion = 0;
                return (false);
            }
#if JA2BETAVERSION
        LoadGameFilePosition(FileGetPos(hFile), "Leave List");
#endif
        }

        if (SaveGameHeader.uiSavedGameVersion <= 73)
        {
            // Patch vehicle fuel
            AddVehicleFuelToSave();
        }


        if (SaveGameHeader.uiSavedGameVersion >= 85)
        {
            if (!NewWayOfLoadingBobbyRMailOrdersToSaveGameFile(hFile))
            {
                this.files.FileClose(hFile);
                guiSaveGameVersion = 0;
                return (false);
            }
#if JA2BETAVERSION
        LoadGameFilePosition(FileGetPos(hFile), "New way of loading Bobby R mailorders");
#endif
        }

        //If there are any old Bobby R Mail orders, tranfer them to the new system
        if (SaveGameHeader.uiSavedGameVersion < 85)
        {
            HandleOldBobbyRMailOrders();
        }


        ///lll




        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Final Checks...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;

        //
        //Close the saved game file
        //
        this.files.FileClose(hFile);

        // ATE: Patch? Patch up groups.....( will only do for old saves.. )
        UpdatePersistantGroupsFromOldSave(SaveGameHeader.uiSavedGameVersion);


        if (SaveGameHeader.uiSavedGameVersion <= 40)
        {
            // Cancel all pending purchase orders for BobbyRay's.  Starting with version 41, the BR orders events are 
            // posted with the usItemIndex itself as the parameter, rather than the inventory slot index.  This was
            // done to make it easier to modify BR's traded inventory lists later on without breaking saves.
            CancelAllPendingBRPurchaseOrders();
        }


        //if the world is loaded, apply the temp files to the loaded map
        if (SaveGameHeader.fWorldLoaded || SaveGameHeader.uiSavedGameVersion < 50)
        {
            // Load the current sectors Information From the temporary files
            if (!LoadCurrentSectorsInformationFromTempItemsFile())
            {
                InitExitGameDialogBecauseFileHackDetected();
                guiSaveGameVersion = 0;
                return (true);
            }
        }

        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Final Checks...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;

        InitAI();

        //Update the mercs in the sector with the new soldier info
        UpdateMercsInSector(gWorldSectorX, gWorldSectorY, gbWorldSectorZ);

        //ReconnectSchedules();
        PostSchedules();


        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Final Checks...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;


        //Reset the lighting level if we are outside
        if (gbWorldSectorZ == 0)
            LightSetBaseLevel(GetTimeOfDayAmbientLightLevel());

        //if we have been to this sector before
        //	if( SectorInfo[ SECTOR( gWorldSectorX,gWorldSectorY) ].uiFlags & SF_ALREADY_VISITED )
        {
            //Reset the fact that we are loading a saved game
            gTacticalStatus.uiFlags &= ~LOADING_SAVED_GAME;
        }

        // CJC January 13: we can't do this because (a) it resets militia IN THE MIDDLE OF 
        // COMBAT, and (b) if we add militia to the teams while LOADING_SAVED_GAME is set,
        // the team counters will not be updated properly!!!
        //	ResetMilitia();


        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Final Checks...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;

        //if the UI was locked in the saved game file
        if (gTacticalStatus.ubAttackBusyCount > 1)
        {
            //Lock the ui
            SetUIBusy((int)gusSelectedSoldier);
        }

        //Reset the shadow 
        SetFontShadow(DEFAULT_SHADOW);

#if JA2BETAVERSION
    AssertMsg(uiSizeOfGeneralInfo == 1024, String("Saved General info is NOT 1024, it is %d.  DF 1.", uiSizeOfGeneralInfo));
#endif

        //if we succesfully LOADED! the game, mark this entry as the last saved game file
        gGameSettings.bLastSavedGameSlot = ubSavedGameID;

        //Save the save game settings
        SaveGameSettings();


        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Final Checks...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;


        //Reset the Ai Timer clock
        giRTAILastUpdateTime = 0;

        //if we are in tactical
        if (guiScreenToGotoAfterLoadingSavedGame == ScreenName.GAME_SCREEN)
        {
            //Initialize the current panel
            InitializeCurrentPanel();

            SelectSoldier(gusSelectedSoldier, false, true);
        }

        uiRelEndPerc += 1;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Final Checks...");
        RenderProgressBar(0, 100);
        uiRelStartPerc = uiRelEndPerc;


        // init extern faces
        InitalizeStaticExternalNPCFaces();

        // load portraits
        LoadCarPortraitValues();

        // OK, turn OFF show all enemies....
        gTacticalStatus.uiFlags &= (~SHOW_ALL_MERCS);
        gTacticalStatus.uiFlags &= ~SHOW_ALL_ITEMS;

        if ((gTacticalStatus.uiFlags & INCOMBAT))
        {
//            DebugMsg(TOPIC_JA2, DBG_LEVEL_3, "Setting attack busy count to 0 from load");
            gTacticalStatus.ubAttackBusyCount = 0;
        }

        if (SaveGameHeader.uiSavedGameVersion < 64)
        { //Militia/enemies/creature team sizes have been changed from 32 to 20.  This function
          //will simply kill off the excess.  This will allow the ability to load previous saves, though
          //there will still be problems, though a LOT less than there would be without this call.
            TruncateStrategicGroupSizes();
        }

        // ATE: if we are within this window where skyridder was foobared, fix!
        if (SaveGameHeader.uiSavedGameVersion >= 61 && SaveGameHeader.uiSavedGameVersion <= 65)
        {
            SOLDIERTYPE? pSoldier;
            MERCPROFILESTRUCT? pProfile;

            if (!fSkyRiderSetUp)
            {
                // see if we can find him and remove him if so....
                pSoldier = FindSoldierByProfileID(NPCID.SKYRIDER, false);

                if (pSoldier != null)
                {
                    TacticalRemoveSoldier(pSoldier.ubID);
                }

                // add the pilot at a random location!
                pProfile = (gMercProfiles[NPCID.SKYRIDER]);
                switch (Globals.Random.Next(4))
                {
                    case 0:
                        pProfile.sSectorX = 15;
                        pProfile.sSectorY = MAP_ROW.B;
                        pProfile.bSectorZ = 0;
                        break;
                    case 1:
                        pProfile.sSectorX = 14;
                        pProfile.sSectorY = MAP_ROW.E;
                        pProfile.bSectorZ = 0;
                        break;
                    case 2:
                        pProfile.sSectorX = 12;
                        pProfile.sSectorY = MAP_ROW.D;
                        pProfile.bSectorZ = 0;
                        break;
                    case 3:
                        pProfile.sSectorX = 16;
                        pProfile.sSectorY = MAP_ROW.C;
                        pProfile.bSectorZ = 0;
                        break;
                }
            }
        }

        if (SaveGameHeader.uiSavedGameVersion < 68)
        {
            // correct bVehicleUnderRepairID for all mercs
            int ubID;
            for (ubID = 0; ubID < MAXMERCS; ubID++)
            {
                Menptr[ubID].bVehicleUnderRepairID = -1;
            }
        }

        if (SaveGameHeader.uiSavedGameVersion < 73)
        {
            if (LaptopSaveInfo.fMercSiteHasGoneDownYet)
                LaptopSaveInfo.fFirstVisitSinceServerWentDown = 2;
        }


        //Update the MERC merc contract lenght.  Before save version 77 the data was stored in the SOLDIERTYPE, 
        //after 77 the data is stored in the profile
        if (SaveGameHeader.uiSavedGameVersion < 77)
        {
            UpdateMercMercContractInfo();
        }


        if (SaveGameHeader.uiSavedGameVersion <= 89)
        {
            // ARM: A change was made in version 89 where refuel site availability now also depends on whether the player has
            // airspace control over that sector.  To update the settings immediately, must call it here.
            UpdateRefuelSiteAvailability();
        }

        if (SaveGameHeader.uiSavedGameVersion < 91)
        {
            //update the amount of money that has been paid to speck
            CalcAproximateAmountPaidToSpeck();
        }

        gfLoadedGame = true;

        uiRelEndPerc = 100;
        SetRelativeStartAndEndPercentage(0, uiRelStartPerc, uiRelEndPerc, "Done!");
        RenderProgressBar(0, 100);

        RemoveLoadingScreenProgressBar();

        SetMusicMode(gMusicModeToPlay);

        // reset to 0
        guiSaveGameVersion = 0;

        // reset once-per-convo records for everyone in the loaded sector
        ResetOncePerConvoRecordsForAllNPCsInLoadedSector();

        if (!(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
        {
            // fix lingering attack busy count problem on loading saved game by resetting a.b.c
            // if we're not in combat.
            gTacticalStatus.ubAttackBusyCount = 0;
        }

        // fix squads
        CheckSquadMovementGroups();

        //The above function LightSetBaseLevel adjusts ALL the level node light values including the merc node,
        //we must reset the values
        HandlePlayerTogglingLightEffects(false);


        return (true);
    }

    bool SaveMercProfiles(Stream hFile)
    {
        NPCID cnt;
        int uiNumBytesWritten = 0;
        int uiSaveSize = Marshal.SizeOf<MERCPROFILESTRUCT>();

        //Lopp through all the profiles to save
        for (cnt = 0; cnt < NUM_PROFILES; cnt++)
        {
            gMercProfiles[cnt].uiProfileChecksum = ProfileChecksum((gMercProfiles[cnt]));
            if (guiSavedGameVersion < 87)
            {
                JA2EncryptedFileWrite(hFile, gMercProfiles[cnt], uiSaveSize, out uiNumBytesWritten);
            }
            else
            {
                NewJA2EncryptedFileWrite(hFile, gMercProfiles[cnt], uiSaveSize, out uiNumBytesWritten);
            }
            if (uiNumBytesWritten != uiSaveSize)
            {
                return (false);
            }
        }

        return (true);
    }



    bool LoadSavedMercProfiles(Stream hFile)
    {
        NPCID cnt;
        int uiNumBytesRead = 0;

        //Lopp through all the profiles to Load
        for (cnt = 0; cnt < NUM_PROFILES; cnt++)
        {
            if (guiSaveGameVersion < 87)
            {
                JA2EncryptedFileRead(hFile, gMercProfiles[cnt], Marshal.SizeOf<MERCPROFILESTRUCT>(), out uiNumBytesRead);
            }
            else
            {
                NewJA2EncryptedFileRead(hFile, gMercProfiles[cnt], Marshal.SizeOf<MERCPROFILESTRUCT>(), out uiNumBytesRead);
            }
            if (uiNumBytesRead != Marshal.SizeOf<MERCPROFILESTRUCT>())
            {
                return (false);
            }
            if (gMercProfiles[cnt].uiProfileChecksum != ProfileChecksum((gMercProfiles[cnt])))
            {
                return (false);
            }
        }

        return (true);
    }

    //Not saving any of these in the soldier struct

    //	struct TAG_level_node				*pLevelNode;
    //	struct TAG_level_node				*pExternShadowLevelNode;
    //	struct TAG_level_node				*pRoofUILevelNode;
    //	int											*pBackGround;
    //	int											*pZBackground;
    //	int											*pForcedShade;
    //
    // 	int											*pEffectShades[ NUM_SOLDIER_EFFECTSHADES ]; // Shading tables for effects
    //  THROW_PARAMS								*pThrowParams;
    //  int											*pCurrentShade;
    //	int											*pGlowShades[ 20 ]; // 
    //	int											*pShades[ NUM_SOLDIER_SHADES ]; // Shading tables
    //	int											*p16BPPPalette;
    //	SGPPaletteEntry							*p8BPPPalette
    //	OBJECTTYPE									*pTempObject;


    bool SaveSoldierStructure(Stream hFile)
    {
        int cnt;
        int uiNumBytesWritten = 0;
        int ubOne = 1;
        int ubZero = 0;

        int uiSaveSize = Marshal.SizeOf<SOLDIERTYPE>();



        //Loop through all the soldier structs to save
        for (cnt = 0; cnt < TOTAL_SOLDIERS; cnt++)
        {

            //if the soldier isnt active, dont add them to the saved game file.
            if (!Menptr[cnt].bActive)
            {
                // Save the byte specifing to NOT load the soldiers 
                FileWrite(hFile, ubZero, 1, out uiNumBytesWritten);
                if (uiNumBytesWritten != 1)
                {
                    return (false);
                }
            }

            else
            {
                // Save the byte specifing to load the soldiers 
                FileWrite(hFile, ubOne, 1, out uiNumBytesWritten);
                if (uiNumBytesWritten != 1)
                {
                    return (false);
                }

                // calculate checksum for soldier
                Menptr[cnt].uiMercChecksum = MercChecksum((Menptr[cnt]));
                // Save the soldier structure
                if (guiSavedGameVersion < 87)
                {
                    JA2EncryptedFileWrite(hFile, Menptr[cnt], uiSaveSize, out uiNumBytesWritten);
                }
                else
                {
                    NewJA2EncryptedFileWrite(hFile, Menptr[cnt], uiSaveSize, out uiNumBytesWritten);
                }
                if (uiNumBytesWritten != uiSaveSize)
                {
                    return (false);
                }



                //
                // Save all the pointer info from the structure
                //


                //Save the pMercPath
                if (!SaveMercPathFromSoldierStruct(hFile, (int)cnt))
                    return (false);



                //
                //do we have a 	KEY_ON_RING									*pKeyRing;
                //

                if (Menptr[cnt].pKeyRing != null)
                {
                    // write to the file saying we have the ....
                    FileWrite(hFile, &ubOne, 1, out uiNumBytesWritten);
                    if (uiNumBytesWritten != 1)
                    {
                        return (false);
                    }

                    // Now save the ....
                    FileWrite(hFile, Menptr[cnt].pKeyRing, NUM_KEYS * Marshal.SizeOf<KEY_ON_RING>(), out uiNumBytesWritten);
                    if (uiNumBytesWritten != NUM_KEYS * Marshal.SizeOf<KEY_ON_RING>())
                    {
                        return (false);
                    }
                }
                else
                {
                    // write to the file saying we DO NOT have the Key ring
                    FileWrite(hFile, &ubZero, 1, out uiNumBytesWritten);
                    if (uiNumBytesWritten != 1)
                    {
                        return (false);
                    }
                }
            }
        }

        return (true);
    }



    bool LoadSoldierStructure(Stream hFile)
    {
        int cnt;
        int uiNumBytesRead = 0;
        SOLDIERTYPE SavedSoldierInfo;
        int uiSaveSize = Marshal.SizeOf<SOLDIERTYPE>();
        int ubId;
        int ubOne = 1;
        int ubActive = 1;
        int uiPercentage;

        SOLDIERCREATE_STRUCT CreateStruct;

        //Loop through all the soldier and delete them all
        for (cnt = 0; cnt < TOTAL_SOLDIERS; cnt++)
        {
            TacticalRemoveSoldier(cnt);
        }



        //Loop through all the soldier structs to load
        for (cnt = 0; cnt < TOTAL_SOLDIERS; cnt++)
        {

            //update the progress bar
            uiPercentage = (cnt * 100) / (TOTAL_SOLDIERS - 1);

            RenderProgressBar(0, uiPercentage);


            //Read in a byte to tell us whether or not there is a soldier loaded here.
            FileRead(hFile, &ubActive, 1, out uiNumBytesRead);
            if (uiNumBytesRead != 1)
            {
                return (false);
            }

            // if the soldier is not active, continue 
            if (!ubActive)
            {
                continue;
            }

            // else if there is a soldier 
            else
            {
                //Read in the saved soldier info into a Temp structure
                if (guiSaveGameVersion < 87)
                {
                    JA2EncryptedFileRead(hFile, &SavedSoldierInfo, uiSaveSize, out uiNumBytesRead);
                }
                else
                {
                    NewJA2EncryptedFileRead(hFile, &SavedSoldierInfo, uiSaveSize, out uiNumBytesRead);
                }
                if (uiNumBytesRead != uiSaveSize)
                {
                    return (false);
                }
                // check checksum
                if (MercChecksum(&SavedSoldierInfo) != SavedSoldierInfo.uiMercChecksum)
                {
                    return (false);
                }

                //Make sure all the pointer references are null'ed out.  
                SavedSoldierInfo.pTempObject = null;
                SavedSoldierInfo.pKeyRing = null;
                SavedSoldierInfo.p8BPPPalette = null;
                SavedSoldierInfo.p16BPPPalette = null;
                memset(SavedSoldierInfo.pShades, 0, Marshal.SizeOf<int*>() * NUM_SOLDIER_SHADES);
                memset(SavedSoldierInfo.pGlowShades, 0, Marshal.SizeOf<int*>() * 20);
                SavedSoldierInfo.pCurrentShade = null;
                SavedSoldierInfo.pThrowParams = null;
                SavedSoldierInfo.pLevelNode = null;
                SavedSoldierInfo.pExternShadowLevelNode = null;
                SavedSoldierInfo.pRoofUILevelNode = null;
                SavedSoldierInfo.pBackGround = null;
                SavedSoldierInfo.pZBackground = null;
                SavedSoldierInfo.pForcedShade = null;
                SavedSoldierInfo.pMercPath = null;
                memset(SavedSoldierInfo.pEffectShades, 0, Marshal.SizeOf<int*>() * NUM_SOLDIER_EFFECTSHADES);


                //if the soldier wasnt active, dont add them now.  Advance to the next merc
                //if( !SavedSoldierInfo.bActive )
                //	continue;


                //Create the new merc
                memset(&CreateStruct, 0, Marshal.SizeOf<SOLDIERCREATE_STRUCT>());
                CreateStruct.bTeam = SavedSoldierInfo.bTeam;
                CreateStruct.ubProfile = SavedSoldierInfo.ubProfile;
                CreateStruct.fUseExistingSoldier = true;
                CreateStruct.pExistingSoldier = &SavedSoldierInfo;

                if (!TacticalCreateSoldier(&CreateStruct, &ubId))
                    return (false);


                // Load the pMercPath
                if (!LoadMercPathToSoldierStruct(hFile, ubId))
                    return (false);


                //
                //do we have a 	KEY_ON_RING									*pKeyRing;
                //

                // Read the file to see if we have to load the keys
                FileRead(hFile, &ubOne, 1, out uiNumBytesRead);
                if (uiNumBytesRead != 1)
                {
                    return (false);
                }

                if (ubOne)
                {
                    // Now Load the ....
                    FileRead(hFile, Menptr[cnt].pKeyRing, NUM_KEYS * Marshal.SizeOf<KEY_ON_RING>(), out uiNumBytesRead);
                    if (uiNumBytesRead != NUM_KEYS * Marshal.SizeOf<KEY_ON_RING>())
                    {
                        return (false);
                    }

                }
                else
                {
                    Assert(Menptr[cnt].pKeyRing == null);
                }

                //if the soldier is an IMP character
                if (Menptr[cnt].ubWhatKindOfMercAmI == MERC_TYPE__PLAYER_CHARACTER && Menptr[cnt].bTeam == gbPlayerNum)
                {
                    ResetIMPCharactersEyesAndMouthOffsets(Menptr[cnt].ubProfile);
                }

                //if the saved game version is before x, calculate the amount of money paid to mercs
                if (guiSaveGameVersion < 83)
                {
                    //if the soldier is someone
                    if (Menptr[cnt].ubProfile != NO_PROFILE)
                    {
                        if (Menptr[cnt].ubWhatKindOfMercAmI == MERC_TYPE__MERC)
                        {
                            gMercProfiles[Menptr[cnt].ubProfile].uiTotalCostToDate = gMercProfiles[Menptr[cnt].ubProfile].sSalary * gMercProfiles[Menptr[cnt].ubProfile].iMercMercContractLength;
                        }
                        else
                        {
                            gMercProfiles[Menptr[cnt].ubProfile].uiTotalCostToDate = gMercProfiles[Menptr[cnt].ubProfile].sSalary * Menptr[cnt].iTotalContractLength;
                        }
                    }
                }

#if GERMAN
            // Fix neutral flags
            if (guiSaveGameVersion < 94)
            {
                if (Menptr[cnt].bTeam == OUR_TEAM && Menptr[cnt].bNeutral && Menptr[cnt].bAssignment != ASSIGNMENT_POW)
                {
                    // turn off neutral flag
                    Menptr[cnt].bNeutral = false;
                }
            }
#endif
                // JA2Gold: fix next-to-previous attacker value
                if (guiSaveGameVersion < 99)
                {
                    Menptr[cnt].ubNextToPreviousAttackerID = NOBODY;
                }

            }
        }

        // Fix robot
        if (guiSaveGameVersion <= 87)
        {
            SOLDIERTYPE* pSoldier;

            if (gMercProfiles[ROBOT].inv[VESTPOS] == SPECTRA_VEST)
            {
                // update this
                gMercProfiles[ROBOT].inv[VESTPOS] = SPECTRA_VEST_18;
                gMercProfiles[ROBOT].inv[HELMETPOS] = SPECTRA_HELMET_18;
                gMercProfiles[ROBOT].inv[LEGPOS] = SPECTRA_LEGGINGS_18;
                gMercProfiles[ROBOT].bAgility = 50;
                pSoldier = FindSoldierByProfileID(ROBOT, false);
                if (pSoldier)
                {
                    pSoldier.inv[VESTPOS].usItem = SPECTRA_VEST_18;
                    pSoldier.inv[HELMETPOS].usItem = SPECTRA_HELMET_18;
                    pSoldier.inv[LEGPOS].usItem = SPECTRA_LEGGINGS_18;
                    pSoldier.bAgility = 50;
                }
            }
        }

        return (true);
    }


    /*
    bool SavePtrInfo( PTR *pData, int uiSizeOfObject, Stream hFile )
    {
        int		ubOne = 1;
        int		ubZero = 0;
        int	uiNumBytesWritten;

        if( pData != null )
        {
            // write to the file saying we have the ....
            FileWrite( hFile, &ubOne, 1, out uiNumBytesWritten );
            if( uiNumBytesWritten != 1 )
            {
                DebugMsg( TOPIC_JA2, DBG_LEVEL_3, String("FAILED to Write Soldier Structure to File" ) );
                return(false);
            }

            // Now save the ....
            FileWrite( hFile, pData, uiSizeOfObject, out uiNumBytesWritten );
            if( uiNumBytesWritten != uiSizeOfObject )
            {
                DebugMsg( TOPIC_JA2, DBG_LEVEL_3, String("FAILED to Write Soldier Structure to File" ) );
                return(false);
            }
        }
        else
        {
            // write to the file saying we DO NOT have the ...
            FileWrite( hFile, &ubZero, 1, out uiNumBytesWritten );
            if( uiNumBytesWritten != 1 )
            {
                DebugMsg( TOPIC_JA2, DBG_LEVEL_3, String("FAILED to Write Soldier Structure to File" ) );
                return(false);
            }
        }

        return( true );
    }



    bool LoadPtrInfo( PTR *pData, int uiSizeOfObject, Stream hFile )
    {
        int		ubOne = 1;
        int		ubZero = 0;
        int	uiNumBytesRead;

        // Read the file to see if we have to load the ....
        FileRead( hFile, &ubOne, 1, out uiNumBytesRead );
        if( uiNumBytesRead != 1 )
        {
            DebugMsg( TOPIC_JA2, DBG_LEVEL_3, String("FAILED to Read Soldier Structure from File" ) );
            return(false);
        }

        if( ubOne )
        {
            // if there is memory already allocated, free it
            MemFree( pData );

            //Allocate space for the structure data
            *pData = MemAlloc( uiSizeOfObject );
            if( pData == null )
                return( false );

            // Now Load the ....
            FileRead( hFile, pData, uiSizeOfObject, out uiNumBytesRead );
            if( uiNumBytesRead != uiSizeOfObject )
            {
                DebugMsg( TOPIC_JA2, DBG_LEVEL_3, String("FAILED to Write Soldier Structure to File" ) );
                return(false);
            }
        }
        else
        {
            // if there is memory already allocated, free it
            if( pData )
            {
                MemFree( pData );
                pData = null;
            }
        }


        return( true );
    }
    */

    bool SaveFilesToSavedGame(string pSrcFileName, Stream hFile)
    {
        int uiFileSize;
        int uiNumBytesWritten = 0;
        Stream hSrcFile;
        int? pData;
        int uiNumBytesRead;


        //open the file
        hSrcFile = this.files.FileOpen(pSrcFileName, FILE_ACCESS_READ | FILE_OPEN_EXISTING, false);
        if (!hSrcFile)
        {
            return (false);
        }

        //Get the file size of the source data file
        uiFileSize = this.files.FileGetSize(hSrcFile);
        if (uiFileSize == 0)
        {
            return (false);
        }

        // Write the the size of the file to the saved game file
        FileWrite(hFile, uiFileSize, Marshal.SizeOf<int>(), out uiNumBytesWritten);
        if (uiNumBytesWritten != Marshal.SizeOf<int>())
        {
            return (false);
        }



        //Allocate a buffer to read the data into
        pData = MemAlloc(uiFileSize);
        if (pData == null)
        {
            return (false);
        }

//        memset(pData, 0, uiFileSize);

        // Read the saource file into the buffer
        FileRead(hSrcFile, pData, uiFileSize, out uiNumBytesRead);
        if (uiNumBytesRead != uiFileSize)
        {
            //Free the buffer
//            MemFree(pData);

            return (false);
        }



        // Write the buffer to the saved game file
        FileWrite(hFile, pData, uiFileSize, out uiNumBytesWritten);
        if (uiNumBytesWritten != uiFileSize)
        {
            //Free the buffer
//            MemFree(pData);

            return (false);
        }

        //Free the buffer
        //        MemFree(pData);

        //Clsoe the source data file
        this.files.FileClose(hSrcFile);

        return (true);
    }

    bool LoadFilesFromSavedGame(string pSrcFileName, Stream hFile)
    {
        int uiFileSize = 0;
        int uiNumBytesWritten = 0;
        Stream hSrcFile;
        int? pData;
        int uiNumBytesRead;



        //If the source file exists, delete it
        if (this.files.FileExists(pSrcFileName))
        {
            if (!this.files.FileDelete(pSrcFileName))
            {
                //unable to delete the original file
                return (false);
            }
        }

        //open the destination file to write to
        hSrcFile = this.files.FileOpen(pSrcFileName, FILE_ACCESS_WRITE | FILE_CREATE_ALWAYS, false);
        if (!hSrcFile)
        {
            //error, we cant open the saved game file
            return (false);
        }


        // Read the size of the data 
        this.files.FileRead(hFile, ref uiFileSize, Marshal.SizeOf<int>(), out uiNumBytesRead);
        if (uiNumBytesRead != Marshal.SizeOf<int>())
        {
            this.files.FileClose(hSrcFile);

            return (false);
        }


        //if there is nothing in the file, return;
        if (uiFileSize == 0)
        {
            this.files.FileClose(hSrcFile);
            return (true);
        }

        //Allocate a buffer to read the data into
        pData = MemAlloc(uiFileSize);
        if (pData == null)
        {
            this.files.FileClose(hSrcFile);
            return (false);
        }


        // Read into the buffer
        this.files.FileRead(hFile, ref pData, uiFileSize, out uiNumBytesRead);
        if (uiNumBytesRead != uiFileSize)
        {
            this.files.FileClose(hSrcFile);

            //Free the buffer
            MemFree(pData);

            return (false);
        }



        // Write the buffer to the new file
        this.files.FileWrite(hSrcFile, pData, uiFileSize, out uiNumBytesWritten);
        if (uiNumBytesWritten != uiFileSize)
        {
            this.files.FileClose(hSrcFile);
//            DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("FAILED to Write to the %s File", pSrcFileName));
            //Free the buffer
//            MemFree(pData);

            return (false);
        }

        //Free the buffer
        //        MemFree(pData);

        //Close the source data file
        this.files.FileClose(hSrcFile);

        return (true);
    }


    bool SaveEmailToSavedGame(Stream hFile)
    {
        int uiNumOfEmails = 0;
        int uiSizeOfEmails = 0;
        email pEmail = pEmailList;
        email pTempEmail = null;
        int cnt;
        int uiStringLength = 0;
        int uiNumBytesWritten = 0;

        SavedEmailStruct SavedEmail;

        //loop through all the email to find out the total number
        while (pEmail)
        {
            pEmail = pEmail.Next;
            uiNumOfEmails++;
        }

        uiSizeOfEmails = Marshal.SizeOf<email>() * uiNumOfEmails;

        //write the number of email messages
        FileWrite(hFile, uiNumOfEmails, Marshal.SizeOf<int>(), out uiNumBytesWritten);
        if (uiNumBytesWritten != Marshal.SizeOf<int>())
        {
            return (false);
        }


        //loop trhough all the emails, add each one individually
        pEmail = pEmailList;
        for (cnt = 0; cnt < uiNumOfEmails; cnt++)
        {
            //Get the strng length of the subject
            uiStringLength = (wcslen(pEmail.pSubject) + 1) * 2;

            //write the length of the current emails subject to the saved game file
            FileWrite(hFile, uiStringLength, Marshal.SizeOf<int>(), out uiNumBytesWritten);
            if (uiNumBytesWritten != Marshal.SizeOf<int>())
            {
                return (false);
            }

            //write the subject of the current email to the saved game file
            FileWrite(hFile, pEmail.pSubject, uiStringLength, out uiNumBytesWritten);
            if (uiNumBytesWritten != uiStringLength)
            {
                return (false);
            }


            //Get the current emails data and asign it to the 'Saved email' struct
            SavedEmail.usOffset = pEmail.usOffset;
            SavedEmail.usLength = pEmail.usLength;
            SavedEmail.ubSender = pEmail.ubSender;
            SavedEmail.iDate = pEmail.iDate;
            SavedEmail.iId = pEmail.iId;
            SavedEmail.iFirstData = pEmail.iFirstData;
            SavedEmail.uiSecondData = pEmail.uiSecondData;
            SavedEmail.fRead = pEmail.fRead;
            SavedEmail.fNew = pEmail.fNew;
            SavedEmail.iThirdData = pEmail.iThirdData;
            SavedEmail.iFourthData = pEmail.iFourthData;
            SavedEmail.uiFifthData = pEmail.uiFifthData;
            SavedEmail.uiSixData = pEmail.uiSixData;


            // write the email header to the saved game file
            FileWrite(hFile, &SavedEmail, Marshal.SizeOf<SavedEmailStruct>(), out uiNumBytesWritten);
            if (uiNumBytesWritten != Marshal.SizeOf<SavedEmailStruct>())
            {
                return (false);
            }

            //advance to the next email
            pEmail = pEmail.Next;
        }

        return (true);
    }


    bool LoadEmailFromSavedGame(Stream hFile)
    {
        int uiNumOfEmails = 0;
        int uiSizeOfSubject = 0;
        email pEmail = pEmailList;
        email pTempEmail = null;
        int? pData = null;
        int cnt;
        SavedEmailStruct SavedEmail;
        int uiNumBytesRead = 0;

        //Delete the existing list of emails
        ShutDownEmailList();

        pEmailList = null;
        //Allocate memory for the header node
        pEmailList = MemAlloc(Marshal.SizeOf<email>());
        if (pEmailList == null)
        {
            return (false);
        }

//        memset(pEmailList, 0, Marshal.SizeOf<email>());

        //read in the number of email messages
        FileRead(hFile, uiNumOfEmails, Marshal.SizeOf<int>(), out uiNumBytesRead);
        if (uiNumBytesRead != Marshal.SizeOf<int>())
        {
            return (false);
        }

        //loop through all the emails, add each one individually
        pEmail = pEmailList;
        for (cnt = 0; cnt < uiNumOfEmails; cnt++)
        {
            //get the length of the email subject
            FileRead(hFile, uiSizeOfSubject, Marshal.SizeOf<int>(), out uiNumBytesRead);
            if (uiNumBytesRead != Marshal.SizeOf<int>())
            {
                return (false);
            }

            //allocate space for the subject
            pData = MemAlloc(EMAIL_SUBJECT_LENGTH * Marshal.SizeOf<wchar_t>());
            if (pData == null)
            {
                return (false);
            }

//            memset(pData, 0, EMAIL_SUBJECT_LENGTH * Marshal.SizeOf<wchar_t>());

            //Get the subject
            FileRead(hFile, pData, uiSizeOfSubject, out uiNumBytesRead);
            if (uiNumBytesRead != uiSizeOfSubject)
            {
                return (false);
            }

            //get the rest of the data from the email
            FileRead(hFile, &SavedEmail, Marshal.SizeOf<SavedEmailStruct>(), out uiNumBytesRead);
            if (uiNumBytesRead != Marshal.SizeOf<SavedEmailStruct>())
            {
                return (false);
            }

            //
            //add the email
            //

            //if we havent allocated space yet
            pTempEmail = MemAlloc(Marshal.SizeOf<Email>());
            if (pTempEmail == null)
            {
                return (false);
            }

//            memset(pTempEmail, 0, Marshal.SizeOf<Email>());

            pTempEmail.usOffset = SavedEmail.usOffset;
            pTempEmail.usLength = SavedEmail.usLength;
            pTempEmail.ubSender = SavedEmail.ubSender;
            pTempEmail.iDate = SavedEmail.iDate;
            pTempEmail.iId = SavedEmail.iId;
            pTempEmail.fRead = SavedEmail.fRead;
            pTempEmail.fNew = SavedEmail.fNew;
            pTempEmail.pSubject = pData;
            pTempEmail.iFirstData = SavedEmail.iFirstData;
            pTempEmail.uiSecondData = SavedEmail.uiSecondData;
            pTempEmail.iThirdData = SavedEmail.iThirdData;
            pTempEmail.iFourthData = SavedEmail.iFourthData;
            pTempEmail.uiFifthData = SavedEmail.uiFifthData;
            pTempEmail.uiSixData = SavedEmail.uiSixData;


            //add the current email in
            pEmail.Next = pTempEmail;
            pTempEmail.Prev = pEmail;

            //moved to the next email
            pEmail = pEmail.Next;

            AddMessageToPages(pTempEmail.iId);

        }

        //if there are emails
        if (cnt)
        {
            //the first node of the LL was a dummy, node,get rid  of it
            pTempEmail = pEmailList;
            pEmailList = pEmailList.Next;
            pEmailList.Prev = null;
            MemFree(pTempEmail);
        }
        else
        {
            MemFree(pEmailList);
            pEmailList = null;
        }

        return (true);
    }


    bool SaveTacticalStatusToSavedGame(Stream hFile)
    {
        int uiNumBytesWritten;

        //write the gTacticalStatus to the saved game file
        FileWrite(hFile, gTacticalStatus, Marshal.SizeOf<TacticalStatusType>(), out uiNumBytesWritten);
        if (uiNumBytesWritten != Marshal.SizeOf<TacticalStatusType>())
        {
            return (false);
        }

        //
        //Save the current sector location to the saved game file
        //

        // save gWorldSectorX
        FileWrite(hFile, gWorldSectorX, Marshal.SizeOf<gWorldSectorX>(), out uiNumBytesWritten);
        if (uiNumBytesWritten != Marshal.SizeOf<gWorldSectorX>())
        {
            return (false);
        }


        // save gWorldSectorY
        FileWrite(hFile, gWorldSectorY, Marshal.SizeOf<gWorldSectorY>(), out uiNumBytesWritten);
        if (uiNumBytesWritten != Marshal.SizeOf<gWorldSectorY>())
        {
            return (false);
        }


        // save gbWorldSectorZ
        FileWrite(hFile, gbWorldSectorZ, Marshal.SizeOf<gbWorldSectorZ>(), out uiNumBytesWritten);
        if (uiNumBytesWritten != Marshal.SizeOf<gbWorldSectorZ>())
        {
            return (false);
        }

        return (true);
    }



    bool LoadTacticalStatusFromSavedGame(Stream hFile)
    {
        int uiNumBytesRead;

        //Read the gTacticalStatus to the saved game file
        FileRead(hFile, gTacticalStatus, Marshal.SizeOf<TacticalStatusType>(), out uiNumBytesRead);
        if (uiNumBytesRead != Marshal.SizeOf<TacticalStatusType>())
        {
            return (false);
        }

        //
        //Load the current sector location to the saved game file
        //

        // Load gWorldSectorX
        FileRead(hFile, gWorldSectorX, Marshal.SizeOf<gWorldSectorX>(), out uiNumBytesRead);
        if (uiNumBytesRead != Marshal.SizeOf<gWorldSectorX>())
        {
            return (false);
        }


        // Load gWorldSectorY
        FileRead(hFile, gWorldSectorY, Marshal.SizeOf<gWorldSectorY>(), out uiNumBytesRead);
        if (uiNumBytesRead != Marshal.SizeOf<gWorldSectorY>())
        {
            return (false);
        }


        // Load gbWorldSectorZ
        FileRead(hFile, gbWorldSectorZ, Marshal.SizeOf<gbWorldSectorZ>(), out uiNumBytesRead);
        if (uiNumBytesRead != Marshal.SizeOf<gbWorldSectorZ>())
        {
            return (false);
        }

        return (true);
    }



    bool CopySavedSoldierInfoToNewSoldier(SOLDIERTYPE pDestSourceInfo, SOLDIERTYPE pSourceInfo)
    {
        //Copy the old soldier information over to the new structure
        //memcpy(pDestSourceInfo, pSourceInfo, Marshal.SizeOf<SOLDIERTYPE>());

        return (true);
    }

    bool SetMercsInsertionGridNo()
    {
        int cnt = 0;

        // loop through all the mercs
        for (cnt = 0; cnt < TOTAL_SOLDIERS; cnt++)
        {
            //if the soldier is active
            if (Menptr[cnt].bActive)
            {

                if (Menptr[cnt].sGridNo != NOWHERE)
                {
                    //set the insertion type to gridno
                    Menptr[cnt].ubStrategicInsertionCode = INSERTION_CODE.GRIDNO;

                    //set the insertion gridno
                    Menptr[cnt].usStrategicInsertionData = Menptr[cnt].sGridNo;

                    //set the gridno
                    Menptr[cnt].sGridNo = NOWHERE;
                }
            }
        }

        return (true);
    }


    bool SaveOppListInfoToSavedGame(Stream hFile)
    {
        int uiSaveSize = 0;
        int uiNumBytesWritten = 0;


        // Save the Public Opplist
        uiSaveSize = MAXTEAMS * TOTAL_SOLDIERS;
        FileWrite(hFile, gbPublicOpplist, uiSaveSize, out uiNumBytesWritten);
        if (uiNumBytesWritten != uiSaveSize)
        {
            return (false);
        }

        // Save the Seen Oppenents
        uiSaveSize = TOTAL_SOLDIERS * TOTAL_SOLDIERS;
        FileWrite(hFile, gbSeenOpponents, uiSaveSize, out uiNumBytesWritten);
        if (uiNumBytesWritten != uiSaveSize)
        {
            return (false);
        }



        // Save the Last Known Opp Locations
        uiSaveSize = TOTAL_SOLDIERS * TOTAL_SOLDIERS;
        FileWrite(hFile, gsLastKnownOppLoc, uiSaveSize, out uiNumBytesWritten);
        if (uiNumBytesWritten != uiSaveSize)
        {
            return (false);
        }

        // Save the Last Known Opp Level
        uiSaveSize = TOTAL_SOLDIERS * TOTAL_SOLDIERS;
        FileWrite(hFile, gbLastKnownOppLevel, uiSaveSize, out uiNumBytesWritten);
        if (uiNumBytesWritten != uiSaveSize)
        {
            return (false);
        }


        // Save the Public Last Known Opp Locations
        uiSaveSize = MAXTEAMS * TOTAL_SOLDIERS;
        FileWrite(hFile, gsPublicLastKnownOppLoc, uiSaveSize, out uiNumBytesWritten);
        if (uiNumBytesWritten != uiSaveSize)
        {
            return (false);
        }

        // Save the Public Last Known Opp Level
        uiSaveSize = MAXTEAMS * TOTAL_SOLDIERS;
        FileWrite(hFile, gbPublicLastKnownOppLevel, uiSaveSize, out uiNumBytesWritten);
        if (uiNumBytesWritten != uiSaveSize)
        {
            return (false);
        }


        // Save the Public Noise Volume
        uiSaveSize = MAXTEAMS;
        FileWrite(hFile, gubPublicNoiseVolume, uiSaveSize, out uiNumBytesWritten);
        if (uiNumBytesWritten != uiSaveSize)
        {
            return (false);
        }

        // Save the Public Last Noise Gridno
        uiSaveSize = MAXTEAMS;
        FileWrite(hFile, gsPublicNoiseGridno, uiSaveSize, out uiNumBytesWritten);
        if (uiNumBytesWritten != uiSaveSize)
        {
            return (false);
        }



        return (true);
    }


    bool LoadOppListInfoFromSavedGame(Stream hFile)
    {
        int uiLoadSize = 0;
        int uiNumBytesRead = 0;

        // Load the Public Opplist
        uiLoadSize = MAXTEAMS * TOTAL_SOLDIERS;
        FileRead(hFile, gbPublicOpplist, uiLoadSize, out uiNumBytesRead);
        if (uiNumBytesRead != uiLoadSize)
        {
            return (false);
        }

        // Load the Seen Oppenents
        uiLoadSize = TOTAL_SOLDIERS * TOTAL_SOLDIERS;
        FileRead(hFile, gbSeenOpponents, uiLoadSize, out uiNumBytesRead);
        if (uiNumBytesRead != uiLoadSize)
        {
            return (false);
        }



        // Load the Last Known Opp Locations
        uiLoadSize = TOTAL_SOLDIERS * TOTAL_SOLDIERS;
        FileRead(hFile, gsLastKnownOppLoc, uiLoadSize, out uiNumBytesRead);
        if (uiNumBytesRead != uiLoadSize)
        {
            return (false);
        }

        // Load the Last Known Opp Level
        uiLoadSize = TOTAL_SOLDIERS * TOTAL_SOLDIERS;
        FileRead(hFile, gbLastKnownOppLevel, uiLoadSize, out uiNumBytesRead);
        if (uiNumBytesRead != uiLoadSize)
        {
            return (false);
        }


        // Load the Public Last Known Opp Locations
        uiLoadSize = MAXTEAMS * TOTAL_SOLDIERS;
        FileRead(hFile, gsPublicLastKnownOppLoc, uiLoadSize, out uiNumBytesRead);
        if (uiNumBytesRead != uiLoadSize)
        {
            return (false);
        }

        // Load the Public Last Known Opp Level
        uiLoadSize = MAXTEAMS * TOTAL_SOLDIERS;
        FileRead(hFile, gbPublicLastKnownOppLevel, uiLoadSize, out uiNumBytesRead);
        if (uiNumBytesRead != uiLoadSize)
        {
            return (false);
        }


        // Load the Public Noise Volume
        uiLoadSize = MAXTEAMS;
        FileRead(hFile, gubPublicNoiseVolume, uiLoadSize, out uiNumBytesRead);
        if (uiNumBytesRead != uiLoadSize)
        {
            return (false);
        }

        // Load the Public Last Noise Gridno
        uiLoadSize = MAXTEAMS;
        FileRead(hFile, gsPublicNoiseGridno, uiLoadSize, out uiNumBytesRead);
        if (uiNumBytesRead != uiLoadSize)
        {
            return (false);
        }

        return (true);
    }

    bool SaveWatchedLocsToSavedGame(Stream hFile)
    {
        int uiArraySize;
        int uiSaveSize = 0;
        int uiNumBytesWritten = 0;

        uiArraySize = TOTAL_SOLDIERS * NUM_WATCHED_LOCS;

        // save locations of watched points
        uiSaveSize = uiArraySize * Marshal.SizeOf<int>();
        FileWrite(hFile, gsWatchedLoc, uiSaveSize, out uiNumBytesWritten);
        if (uiNumBytesWritten != uiSaveSize)
        {
            return (false);
        }

        uiSaveSize = uiArraySize * Marshal.SizeOf<int>();

        FileWrite(hFile, gbWatchedLocLevel, uiSaveSize, out uiNumBytesWritten);
        if (uiNumBytesWritten != uiSaveSize)
        {
            return (false);
        }

        FileWrite(hFile, gubWatchedLocPoints, uiSaveSize, out uiNumBytesWritten);
        if (uiNumBytesWritten != uiSaveSize)
        {
            return (false);
        }

        FileWrite(hFile, gfWatchedLocReset, uiSaveSize, out uiNumBytesWritten);
        if (uiNumBytesWritten != uiSaveSize)
        {
            return (false);
        }

        return (true);
    }


    bool LoadWatchedLocsFromSavedGame(Stream hFile)
    {
        int uiArraySize;
        int uiLoadSize = 0;
        int uiNumBytesRead = 0;

        uiArraySize = TOTAL_SOLDIERS * NUM_WATCHED_LOCS;

        uiLoadSize = uiArraySize * Marshal.SizeOf<int>();
        FileRead(hFile, gsWatchedLoc, uiLoadSize, out uiNumBytesRead);
        if (uiNumBytesRead != uiLoadSize)
        {
            return (false);
        }

        uiLoadSize = uiArraySize * Marshal.SizeOf<int>();
        FileRead(hFile, gbWatchedLocLevel, uiLoadSize, out uiNumBytesRead);
        if (uiNumBytesRead != uiLoadSize)
        {
            return (false);
        }

        FileRead(hFile, gubWatchedLocPoints, uiLoadSize, out uiNumBytesRead);
        if (uiNumBytesRead != uiLoadSize)
        {
            return (false);
        }

        FileRead(hFile, gfWatchedLocReset, uiLoadSize, out uiNumBytesRead);
        if (uiNumBytesRead != uiLoadSize)
        {
            return (false);
        }


        return (true);
    }

    void CreateSavedGameFileNameFromNumber(int ubSaveGameID, string pzNewFileName)
    {
        //if we are creating the QuickSave file
        if (ubSaveGameID == 0)
        {
            sprintf(pzNewFileName, "%S\\%S.%S", EnglishText.pMessageStrings[MSG.SAVEDIRECTORY], EnglishText.pMessageStrings[MSG.QUICKSAVE_NAME], EnglishText.pMessageStrings[MSG.SAVEEXTENSION]);
        }
        else if (ubSaveGameID == SAVE__END_TURN_NUM)
        {
            //The name of the file
            sprintf(pzNewFileName, "%S\\Auto%02d.%S", EnglishText.pMessageStrings[MSG.SAVEDIRECTORY], guiLastSaveGameNum, EnglishText.pMessageStrings[MSG.SAVEEXTENSION]);

            //increment end turn number
            guiLastSaveGameNum++;

            //just have 2 saves
            if (guiLastSaveGameNum == 2)
            {
                guiLastSaveGameNum = 0;
            }
        }
        else
        {
            sprintf(pzNewFileName, "%S\\%S%02d.%S", EnglishText.pMessageStrings[MSG.SAVEDIRECTORY], EnglishText.pMessageStrings[MSG.SAVE_NAME], ubSaveGameID, EnglishText.pMessageStrings[MSG.SAVEEXTENSION]);
        }
    }

    bool SaveMercPathFromSoldierStruct(Stream hFile, int ubID)
    {
        int uiNumOfNodes = 0;
        Path pTempPath = Menptr[ubID].pMercPath;
        int uiNumBytesWritten = 0;


        //loop through to get all the nodes
        while (pTempPath)
        {
            uiNumOfNodes++;
            pTempPath = pTempPath.pNext;
        }


        //Save the number of the nodes
        files.FileWrite(hFile, uiNumOfNodes, Marshal.SizeOf<int>(), out uiNumBytesWritten);
        if (uiNumBytesWritten != Marshal.SizeOf<int>())
        {
            return (false);
        }

        //loop through all the nodes and add them
        pTempPath = Menptr[ubID].pMercPath;


        //loop through nodes and save all the nodes
        while (pTempPath)
        {
            //Save the number of the nodes
            files.FileWrite(hFile, pTempPath, Marshal.SizeOf<Path>(), out uiNumBytesWritten);
            if (uiNumBytesWritten != Marshal.SizeOf<Path>())
            {
                return (false);
            }

            pTempPath = pTempPath.pNext;
        }

        return (true);
    }

    bool LoadMercPathToSoldierStruct(Stream hFile, int ubID)
    {
        int uiNumOfNodes = 0;
        Path? pTempPath = null;
        Path? pTemp = null;
        int uiNumBytesRead = 0;
        int cnt;



        //The list SHOULD be empty at this point
        /*
            //if there is nodes, loop through and delete them
            if( Menptr[ ubID ].pMercPath )
            {
                pTempPath = Menptr[ ubID ].pMercPath;
                while( pTempPath )
                {
                    pTemp = pTempPath;
                    pTempPath = pTempPath.pNext;

                    MemFree( pTemp );
                    pTemp=null;
                }

                Menptr[ ubID ].pMercPath = null;
            }
        */

        //Load the number of the nodes
        files.FileRead(hFile, ref uiNumOfNodes, Marshal.SizeOf<int>(), out uiNumBytesRead);
        if (uiNumBytesRead != Marshal.SizeOf<int>())
        {
            return (false);
        }

        //load all the nodes
        for (cnt = 0; cnt < uiNumOfNodes; cnt++)
        {
            //Allocate memory for the new node
            pTemp = new(); // MemAlloc(Marshal.SizeOf<PathSt>());
            if (pTemp == null)
            {
                return (false);
            }

            //Load the node
            files.FileRead(hFile, ref pTemp, 17/*Marshal.SizeOf<Path>()*/, out uiNumBytesRead);
            if (uiNumBytesRead != Marshal.SizeOf<Path>())
            {
                return (false);
            }

            //Put the node into the list 
            if (cnt == 0)
            {
                pTempPath = pTemp;
                pTemp.pPrev = null;
            }
            else
            {
                pTempPath.pNext = pTemp;
                pTemp.pPrev = pTempPath;

                pTempPath = pTempPath.pNext;
            }

            pTemp.pNext = null;
        }

        //move to beginning of list
        pTempPath = MoveToBeginningOfPathList(pTempPath);

        Menptr[ubID].pMercPath = pTempPath;
        if (Menptr[ubID].pMercPath)
            Menptr[ubID].pMercPath.pPrev = null;

        return (true);
    }


#if JA2BETAVERSION
void InitSaveGameFilePosition()
{
    CHAR8 zFileName[128];

    sprintf(zFileName, "%S\\SaveGameFilePos%2d.txt", pMessageStrings[MGS.SAVEDIRECTORY], gubSaveGameLoc);

    FileDelete(zFileName);
}

void SaveGameFilePosition(int iPos, STR pMsg)
{
    Stream hFile;
    CHAR8 zTempString[512];
    int uiNumBytesWritten;
    int uiStrLen = 0;
    CHAR8 zFileName[128];

    sprintf(zFileName, "%S\\SaveGameFilePos%2d.txt", pMessageStrings[MGS.SAVEDIRECTORY], gubSaveGameLoc);

    // create the save game file
    hFile = FileOpen(zFileName, FILE_ACCESS_WRITE | FILE_OPEN_ALWAYS, false);
    if (!hFile)
    {
        FileClose(hFile);
        return;
    }

    FileSeek(hFile, 0, FILE_SEEK_FROM_END);

    sprintf(zTempString, "%8d     %s\n", iPos, pMsg);
    uiStrLen = strlen(zTempString);

    FileWrite(hFile, zTempString, uiStrLen, out uiNumBytesWritten);
    if (uiNumBytesWritten != uiStrLen)
    {
        FileClose(hFile);
        return;
    }

    FileClose(hFile);
}



void InitLoadGameFilePosition()
{
    CHAR8 zFileName[128];

    sprintf(zFileName, "%S\\LoadGameFilePos%2d.txt", pMessageStrings[MGS.SAVEDIRECTORY], gubSaveGameLoc);

    FileDelete(zFileName);
}
void LoadGameFilePosition(int iPos, STR pMsg)
{
    Stream hFile;
    CHAR8 zTempString[512];
    int uiNumBytesWritten;
    int uiStrLen = 0;

    CHAR8 zFileName[128];

    sprintf(zFileName, "%S\\LoadGameFilePos%2d.txt", pMessageStrings[MGS.SAVEDIRECTORY], gubSaveGameLoc);

    // create the save game file
    hFile = FileOpen(zFileName, FILE_ACCESS_WRITE | FILE_OPEN_ALWAYS, false);
    if (!hFile)
    {
        FileClose(hFile);
        return;
    }

    FileSeek(hFile, 0, FILE_SEEK_FROM_END);

    sprintf(zTempString, "%8d     %s\n", iPos, pMsg);
    uiStrLen = strlen(zTempString);

    FileWrite(hFile, zTempString, uiStrLen, out uiNumBytesWritten);
    if (uiNumBytesWritten != uiStrLen)
    {
        FileClose(hFile);
        return;
    }

    FileClose(hFile);


}
#endif



    bool SaveGeneralInfo(Stream hFile)
    {
        int uiNumBytesWritten;

        GENERAL_SAVE_INFO sGeneralInfo;
        memset(&sGeneralInfo, 0, Marshal.SizeOf<GENERAL_SAVE_INFO>());

        sGeneralInfo.ubMusicMode = gubMusicMode;
        sGeneralInfo.uiCurrentUniqueSoldierId = guiCurrentUniqueSoldierId;
        sGeneralInfo.uiCurrentScreen = guiPreviousOptionScreen;

        sGeneralInfo.usSelectedSoldier = gusSelectedSoldier;
        sGeneralInfo.sRenderCenterX = gsRenderCenterX;
        sGeneralInfo.sRenderCenterY = gsRenderCenterY;
        sGeneralInfo.fAtLeastOneMercWasHired = gfAtLeastOneMercWasHired;
        sGeneralInfo.fHavePurchasedItemsFromTony = gfHavePurchasedItemsFromTony;

        sGeneralInfo.fShowItemsFlag = fShowItemsFlag;
        sGeneralInfo.fShowTownFlag = fShowTownFlag;
        sGeneralInfo.fShowMineFlag = fShowMineFlag;
        sGeneralInfo.fShowAircraftFlag = fShowAircraftFlag;
        sGeneralInfo.fShowTeamFlag = fShowTeamFlag;

        sGeneralInfo.fHelicopterAvailable = fHelicopterAvailable;

        // helicopter vehicle id
        sGeneralInfo.iHelicopterVehicleId = iHelicopterVehicleId;

        // total distance travelled
        //	sGeneralInfo.iTotalHeliDistanceSinceRefuel = iTotalHeliDistanceSinceRefuel;

        // total owed by player
        sGeneralInfo.iTotalAccumulatedCostByPlayer = iTotalAccumulatedCostByPlayer;

        // whether or not skyrider is alive and well? and on our side yet?
        sGeneralInfo.fSkyRiderAvailable = fSkyRiderAvailable;

        // is the heli in the air?
        sGeneralInfo.fHelicopterIsAirBorne = fHelicopterIsAirBorne;

        // is the pilot returning straight to base?
        sGeneralInfo.fHeliReturnStraightToBase = fHeliReturnStraightToBase;

        // heli hovering
        sGeneralInfo.fHoveringHelicopter = fHoveringHelicopter;

        // time started hovering
        sGeneralInfo.uiStartHoverTime = uiStartHoverTime;

        // what state is skyrider's dialogue in in?
        sGeneralInfo.uiHelicopterSkyriderTalkState = guiHelicopterSkyriderTalkState;

        // the flags for skyrider events
        sGeneralInfo.fShowEstoniRefuelHighLight = fShowEstoniRefuelHighLight;
        sGeneralInfo.fShowOtherSAMHighLight = fShowOtherSAMHighLight;
        sGeneralInfo.fShowDrassenSAMHighLight = fShowDrassenSAMHighLight;
        sGeneralInfo.fShowCambriaHospitalHighLight = fShowCambriaHospitalHighLight;

        //The current state of the weather
        sGeneralInfo.uiEnvWeather = guiEnvWeather;

        sGeneralInfo.ubDefaultButton = gubDefaultButton;

        sGeneralInfo.fSkyriderEmptyHelpGiven = gfSkyriderEmptyHelpGiven;
        sGeneralInfo.ubHelicopterHitsTaken = gubHelicopterHitsTaken;
        sGeneralInfo.fSkyriderSaidCongratsOnTakingSAM = gfSkyriderSaidCongratsOnTakingSAM;
        sGeneralInfo.ubPlayerProgressSkyriderLastCommentedOn = gubPlayerProgressSkyriderLastCommentedOn;

        sGeneralInfo.fEnterMapDueToContract = fEnterMapDueToContract;
        sGeneralInfo.ubQuitType = ubQuitType;

        if (pContractReHireSoldier != null)
            sGeneralInfo.sContractRehireSoldierID = pContractReHireSoldier.ubID;
        else
            sGeneralInfo.sContractRehireSoldierID = -1;

        memcpy(sGeneralInfo.GameOptions, gGameOptions, Marshal.SizeOf<GAME_OPTIONS>());

        //Save the Ja2Clock()
        sGeneralInfo.uiBaseJA2Clock = guiBaseJA2Clock;

        // Save the current tactical panel mode
        sGeneralInfo.sCurInterfacePanel = gsCurInterfacePanel;

        // Save the selected merc
        if (gpSMCurrentMerc)
            sGeneralInfo.ubSMCurrentMercID = gpSMCurrentMerc.ubID;
        else
            sGeneralInfo.ubSMCurrentMercID = 255;

        //Save the fact that it is the first time in mapscreen
        sGeneralInfo.fFirstTimeInMapScreen = fFirstTimeInMapScreen;

        //save map screen disabling buttons
        sGeneralInfo.fDisableDueToBattleRoster = fDisableDueToBattleRoster;
        sGeneralInfo.fDisableMapInterfaceDueToBattle = fDisableMapInterfaceDueToBattle;

        // Save boxing info
        memcpy(sGeneralInfo.sBoxerGridNo, gsBoxerGridNo, NUM_BOXERS * Marshal.SizeOf<int>());
        memcpy(sGeneralInfo.ubBoxerID, gubBoxerID, NUM_BOXERS * Marshal.SizeOf<int>());
        memcpy(sGeneralInfo.fBoxerFought, gfBoxerFought, NUM_BOXERS * Marshal.SizeOf<bool>());

        //Save the helicopter status
        sGeneralInfo.fHelicopterDestroyed = fHelicopterDestroyed;
        sGeneralInfo.fShowMapScreenHelpText = fShowMapScreenHelpText;

        sGeneralInfo.iSortStateForMapScreenList = giSortStateForMapScreenList;
        sGeneralInfo.fFoundTixa = fFoundTixa;

        sGeneralInfo.uiTimeOfLastSkyriderMonologue = guiTimeOfLastSkyriderMonologue;
        sGeneralInfo.fSkyRiderSetUp = fSkyRiderSetUp;

        memcpy(sGeneralInfo.fRefuelingSiteAvailable, fRefuelingSiteAvailable, NUMBER_OF_REFUEL_SITES * Marshal.SizeOf<bool>());


        //Meanwhile stuff
        memcpy(sGeneralInfo.gCurrentMeanwhileDef, gCurrentMeanwhileDef, Marshal.SizeOf<MEANWHILE_DEFINITION>());
        //sGeneralInfo.gfMeanwhileScheduled = gfMeanwhileScheduled;
        sGeneralInfo.gfMeanwhileTryingToStart = gfMeanwhileTryingToStart;
        sGeneralInfo.gfInMeanwhile = gfInMeanwhile;


        // list of dead guys for squads...in id values . -1 means no one home 
        memcpy(sGeneralInfo.sDeadMercs, sDeadMercs, Marshal.SizeOf<int>() * (int)NUMBER_OF_SQUADS * NUMBER_OF_SOLDIERS_PER_SQUAD);

        // level of public noises
        memcpy(sGeneralInfo.gbPublicNoiseLevel, gbPublicNoiseLevel, Marshal.SizeOf<int>() * (int)MAXTEAMS);

        //The screen count for the initscreen
        sGeneralInfo.gubScreenCount = gubScreenCount;


        //used for the mean while screen
        sGeneralInfo.uiMeanWhileFlags = uiMeanWhileFlags;

        //Imp portrait number
        sGeneralInfo.iPortraitNumber = iPortraitNumber;

        // location of first enocunter with enemy
        sGeneralInfo.sWorldSectorLocationOfFirstBattle = sWorldSectorLocationOfFirstBattle;


        //State of email flags
        sGeneralInfo.fUnReadMailFlag = fUnReadMailFlag;
        sGeneralInfo.fNewMailFlag = fNewMailFlag;
        sGeneralInfo.fOldUnReadFlag = fOldUnreadFlag;
        sGeneralInfo.fOldNewMailFlag = fOldNewMailFlag;

        sGeneralInfo.fShowMilitia = fShowMilitia;

        sGeneralInfo.fNewFilesInFileViewer = fNewFilesInFileViewer;

        sGeneralInfo.fLastBoxingMatchWonByPlayer = gfLastBoxingMatchWonByPlayer;

        memcpy(sGeneralInfo.fSamSiteFound, fSamSiteFound, NUMBER_OF_SAMS * Marshal.SizeOf<bool>());

        sGeneralInfo.ubNumTerrorists = gubNumTerrorists;
        sGeneralInfo.ubCambriaMedicalObjects = gubCambriaMedicalObjects;

        sGeneralInfo.fDisableTacticalPanelButtons = gfDisableTacticalPanelButtons;

        sGeneralInfo.sSelMapX = sSelMapX;
        sGeneralInfo.sSelMapY = sSelMapY;
        sGeneralInfo.iCurrentMapSectorZ = iCurrentMapSectorZ;

        //Save the current status of the help screens flag that say wether or not the user has been there before
        sGeneralInfo.usHasPlayerSeenHelpScreenInCurrentScreen = gHelpScreen.usHasPlayerSeenHelpScreenInCurrentScreen;

        sGeneralInfo.ubBoxingMatchesWon = gubBoxingMatchesWon;
        sGeneralInfo.ubBoxersRests = gubBoxersRests;
        sGeneralInfo.fBoxersResting = gfBoxersResting;

        sGeneralInfo.ubDesertTemperature = gubDesertTemperature;
        sGeneralInfo.ubGlobalTemperature = gubGlobalTemperature;

        sGeneralInfo.sMercArriveSectorX = gsMercArriveSectorX;
        sGeneralInfo.sMercArriveSectorY = gsMercArriveSectorY;

        sGeneralInfo.fCreatureMeanwhileScenePlayed = gfCreatureMeanwhileScenePlayed;

        //save the global player num
        sGeneralInfo.ubPlayerNum = gbPlayerNum;

        //New stuff for the Prebattle interface / autoresolve
        sGeneralInfo.fPersistantPBI = gfPersistantPBI;
        sGeneralInfo.ubEnemyEncounterCode = gubEnemyEncounterCode;
        sGeneralInfo.ubExplicitEnemyEncounterCode = gubExplicitEnemyEncounterCode;
        sGeneralInfo.fBlitBattleSectorLocator = gfBlitBattleSectorLocator;
        sGeneralInfo.ubPBSectorX = gubPBSectorX;
        sGeneralInfo.ubPBSectorY = gubPBSectorY;
        sGeneralInfo.ubPBSectorZ = gubPBSectorZ;
        sGeneralInfo.fCantRetreatInPBI = gfCantRetreatInPBI;
        sGeneralInfo.fExplosionQueueActive = gfExplosionQueueActive;

        sGeneralInfo.bSelectedInfoChar = bSelectedInfoChar;

        sGeneralInfo.iHospitalTempBalance = giHospitalTempBalance;
        sGeneralInfo.iHospitalRefund = giHospitalRefund;
        sGeneralInfo.bHospitalPriceModifier = gbHospitalPriceModifier;
        sGeneralInfo.fPlayerTeamSawJoey = gfPlayerTeamSawJoey;
        sGeneralInfo.fMikeShouldSayHi = gfMikeShouldSayHi;

        //Setup the 
        //Save the current music mode
        this.files.FileWrite(hFile, sGeneralInfo, Marshal.SizeOf<GENERAL_SAVE_INFO>(), out uiNumBytesWritten);
        if (uiNumBytesWritten != Marshal.SizeOf<GENERAL_SAVE_INFO>())
        {
            this.files.FileClose(hFile);
            return (false);
        }

        return (true);
    }


    bool LoadGeneralInfo(Stream hFile)
    {
        int uiNumBytesRead;

        GENERAL_SAVE_INFO sGeneralInfo;
        memset(sGeneralInfo, 0, Marshal.SizeOf<GENERAL_SAVE_INFO>());


        //Load the current music mode
        this.files.FileRead(hFile, sGeneralInfo, Marshal.SizeOf<GENERAL_SAVE_INFO>(), out uiNumBytesRead);
        if (uiNumBytesRead != Marshal.SizeOf<GENERAL_SAVE_INFO>())
        {
            this.files.FileClose(hFile);
            return (false);
        }

        gMusicModeToPlay = sGeneralInfo.ubMusicMode;

        guiCurrentUniqueSoldierId = sGeneralInfo.uiCurrentUniqueSoldierId;

        guiScreenToGotoAfterLoadingSavedGame = sGeneralInfo.uiCurrentScreen;

        //	gusSelectedSoldier = NOBODY;
        gusSelectedSoldier = sGeneralInfo.usSelectedSoldier;

        gsRenderCenterX = sGeneralInfo.sRenderCenterX;
        gsRenderCenterY = sGeneralInfo.sRenderCenterY;

        gfAtLeastOneMercWasHired = sGeneralInfo.fAtLeastOneMercWasHired;



        gfHavePurchasedItemsFromTony = sGeneralInfo.fHavePurchasedItemsFromTony;

        fShowItemsFlag = sGeneralInfo.fShowItemsFlag;
        fShowTownFlag = sGeneralInfo.fShowTownFlag;
        fShowMineFlag = sGeneralInfo.fShowMineFlag;
        fShowAircraftFlag = sGeneralInfo.fShowAircraftFlag;
        fShowTeamFlag = sGeneralInfo.fShowTeamFlag;

        fHelicopterAvailable = sGeneralInfo.fHelicopterAvailable;

        // helicopter vehicle id
        iHelicopterVehicleId = sGeneralInfo.iHelicopterVehicleId;

        // total distance travelled
        //	iTotalHeliDistanceSinceRefuel = sGeneralInfo.iTotalHeliDistanceSinceRefuel;

        // total owed to player
        iTotalAccumulatedCostByPlayer = sGeneralInfo.iTotalAccumulatedCostByPlayer;

        // whether or not skyrider is alive and well? and on our side yet?
        fSkyRiderAvailable = sGeneralInfo.fSkyRiderAvailable;

        // is the heli in the air?
        fHelicopterIsAirBorne = sGeneralInfo.fHelicopterIsAirBorne;

        // is the pilot returning straight to base?
        fHeliReturnStraightToBase = sGeneralInfo.fHeliReturnStraightToBase;

        // heli hovering
        fHoveringHelicopter = sGeneralInfo.fHoveringHelicopter;

        // time started hovering
        uiStartHoverTime = sGeneralInfo.uiStartHoverTime;

        // what state is skyrider's dialogue in in?
        guiHelicopterSkyriderTalkState = sGeneralInfo.uiHelicopterSkyriderTalkState;

        // the flags for skyrider events
        fShowEstoniRefuelHighLight = sGeneralInfo.fShowEstoniRefuelHighLight;
        fShowOtherSAMHighLight = sGeneralInfo.fShowOtherSAMHighLight;
        fShowDrassenSAMHighLight = sGeneralInfo.fShowDrassenSAMHighLight;
        fShowCambriaHospitalHighLight = sGeneralInfo.fShowCambriaHospitalHighLight;

        //The current state of the weather
        guiEnvWeather = sGeneralInfo.uiEnvWeather;

        gubDefaultButton = sGeneralInfo.ubDefaultButton;

        gfSkyriderEmptyHelpGiven = sGeneralInfo.fSkyriderEmptyHelpGiven;
        gubHelicopterHitsTaken = sGeneralInfo.ubHelicopterHitsTaken;
        gfSkyriderSaidCongratsOnTakingSAM = sGeneralInfo.fSkyriderSaidCongratsOnTakingSAM;
        gubPlayerProgressSkyriderLastCommentedOn = sGeneralInfo.ubPlayerProgressSkyriderLastCommentedOn;

        fEnterMapDueToContract = sGeneralInfo.fEnterMapDueToContract;
        ubQuitType = sGeneralInfo.ubQuitType;

        //if the soldier id is valid
        if (sGeneralInfo.sContractRehireSoldierID == -1)
            pContractReHireSoldier = null;
        else
            pContractReHireSoldier = Menptr[sGeneralInfo.sContractRehireSoldierID];

        memcpy(gGameOptions, sGeneralInfo.GameOptions, Marshal.SizeOf<GAME_OPTIONS>());

        //Restore the JA2 Clock
        guiBaseJA2Clock = sGeneralInfo.uiBaseJA2Clock;

        // whenever guiBaseJA2Clock changes, we must reset all the timer variables that use it as a reference
        ResetJA2ClockGlobalTimers();


        // Restore the selected merc
        if (sGeneralInfo.ubSMCurrentMercID == 255)
            gpSMCurrentMerc = null;
        else
            gpSMCurrentMerc = Menptr[sGeneralInfo.ubSMCurrentMercID];

        //Set the interface panel to the team panel
        ShutdownCurrentPanel();

        //Restore the current tactical panel mode
        gsCurInterfacePanel = sGeneralInfo.sCurInterfacePanel;

        /*
        //moved to last stage in the LoadSaveGame() function
        //if we are in tactical
        if( guiScreenToGotoAfterLoadingSavedGame == GAME_SCREEN )
        {
            //Initialize the current panel
            InitializeCurrentPanel( );

            SelectSoldier( gusSelectedSoldier, false, true );
        }
        */

        //Restore the fact that it is the first time in mapscreen
        fFirstTimeInMapScreen = sGeneralInfo.fFirstTimeInMapScreen;

        //Load map screen disabling buttons
        fDisableDueToBattleRoster = sGeneralInfo.fDisableDueToBattleRoster;
        fDisableMapInterfaceDueToBattle = sGeneralInfo.fDisableMapInterfaceDueToBattle;

        memcpy(gsBoxerGridNo, sGeneralInfo.sBoxerGridNo, NUM_BOXERS * Marshal.SizeOf<int>());
        memcpy(gubBoxerID, sGeneralInfo.ubBoxerID, NUM_BOXERS * Marshal.SizeOf<int>());
        memcpy(gfBoxerFought, sGeneralInfo.fBoxerFought, NUM_BOXERS * Marshal.SizeOf<bool>());

        //Load the helicopter status
        fHelicopterDestroyed = sGeneralInfo.fHelicopterDestroyed;
        fShowMapScreenHelpText = sGeneralInfo.fShowMapScreenHelpText;


        giSortStateForMapScreenList = sGeneralInfo.iSortStateForMapScreenList;
        fFoundTixa = sGeneralInfo.fFoundTixa;

        guiTimeOfLastSkyriderMonologue = sGeneralInfo.uiTimeOfLastSkyriderMonologue;
        fSkyRiderSetUp = sGeneralInfo.fSkyRiderSetUp;

        memcpy(&fRefuelingSiteAvailable, &sGeneralInfo.fRefuelingSiteAvailable, NUMBER_OF_REFUEL_SITES * Marshal.SizeOf<bool>());


        //Meanwhile stuff
        memcpy(gCurrentMeanwhileDef, &sGeneralInfo.gCurrentMeanwhileDef, Marshal.SizeOf<MEANWHILE_DEFINITION>());
        //	gfMeanwhileScheduled = sGeneralInfo.gfMeanwhileScheduled;
        gfMeanwhileTryingToStart = sGeneralInfo.gfMeanwhileTryingToStart;
        gfInMeanwhile = sGeneralInfo.gfInMeanwhile;

        // list of dead guys for squads...in id values . -1 means no one home 
        memcpy(sDeadMercs, sGeneralInfo.sDeadMercs, Marshal.SizeOf<int>() * NUMBER_OF_SQUADS * NUMBER_OF_SOLDIERS_PER_SQUAD);

        // level of public noises
        for (int i = 0; i < sGeneralInfo.gbPublicNoiseLevel.Length; i++)
        {
            gbPublicNoiseLevel.Values[i] = sGeneralInfo.gbPublicNoiseLevel[i];
        }

        //the screen count for the init screen
        gubScreenCount = sGeneralInfo.gubScreenCount;

        //used for the mean while screen
        if (guiSaveGameVersion < 71)
        {
            uiMeanWhileFlags = sGeneralInfo.usOldMeanWhileFlags;
        }
        else
        {
            uiMeanWhileFlags = sGeneralInfo.uiMeanWhileFlags;
        }

        //Imp portrait number
        iPortraitNumber = sGeneralInfo.iPortraitNumber;

        // location of first enocunter with enemy
        sWorldSectorLocationOfFirstBattle = sGeneralInfo.sWorldSectorLocationOfFirstBattle;

        fShowMilitia = sGeneralInfo.fShowMilitia;

        fNewFilesInFileViewer = sGeneralInfo.fNewFilesInFileViewer;

        gfLastBoxingMatchWonByPlayer = sGeneralInfo.fLastBoxingMatchWonByPlayer;

        memcpy(fSamSiteFound, &sGeneralInfo.fSamSiteFound, NUMBER_OF_SAMS * Marshal.SizeOf<bool>());

        gubNumTerrorists = sGeneralInfo.ubNumTerrorists;
        gubCambriaMedicalObjects = sGeneralInfo.ubCambriaMedicalObjects;

        gfDisableTacticalPanelButtons = sGeneralInfo.fDisableTacticalPanelButtons;

        sSelMapX = sGeneralInfo.sSelMapX;
        sSelMapY = sGeneralInfo.sSelMapY;
        iCurrentMapSectorZ = sGeneralInfo.iCurrentMapSectorZ;

        //State of email flags
        fUnReadMailFlag = sGeneralInfo.fUnReadMailFlag;
        fNewMailFlag = sGeneralInfo.fNewMailFlag;
        fOldUnreadFlag = sGeneralInfo.fOldUnReadFlag;
        fOldNewMailFlag = sGeneralInfo.fOldNewMailFlag;

        //Save the current status of the help screens flag that say wether or not the user has been there before
        gHelpScreen.usHasPlayerSeenHelpScreenInCurrentScreen = sGeneralInfo.usHasPlayerSeenHelpScreenInCurrentScreen;

        gubBoxingMatchesWon = sGeneralInfo.ubBoxingMatchesWon;
        gubBoxersRests = sGeneralInfo.ubBoxersRests;
        gfBoxersResting = sGeneralInfo.fBoxersResting;

        gubDesertTemperature = sGeneralInfo.ubDesertTemperature;
        gubGlobalTemperature = sGeneralInfo.ubGlobalTemperature;

        gsMercArriveSectorX = sGeneralInfo.sMercArriveSectorX;
        gsMercArriveSectorY = sGeneralInfo.sMercArriveSectorY;

        gfCreatureMeanwhileScenePlayed = sGeneralInfo.fCreatureMeanwhileScenePlayed;

        //load the global player num
        gbPlayerNum = sGeneralInfo.ubPlayerNum;

        //New stuff for the Prebattle interface / autoresolve
        gfPersistantPBI = sGeneralInfo.fPersistantPBI;
        gubEnemyEncounterCode = sGeneralInfo.ubEnemyEncounterCode;
        gubExplicitEnemyEncounterCode = sGeneralInfo.ubExplicitEnemyEncounterCode;
        gfBlitBattleSectorLocator = sGeneralInfo.fBlitBattleSectorLocator;
        gubPBSectorX = sGeneralInfo.ubPBSectorX;
        gubPBSectorY = sGeneralInfo.ubPBSectorY;
        gubPBSectorZ = sGeneralInfo.ubPBSectorZ;
        gfCantRetreatInPBI = sGeneralInfo.fCantRetreatInPBI;
        gfExplosionQueueActive = sGeneralInfo.fExplosionQueueActive;

        bSelectedInfoChar = sGeneralInfo.bSelectedInfoChar;

        giHospitalTempBalance = sGeneralInfo.iHospitalTempBalance;
        giHospitalRefund = sGeneralInfo.iHospitalRefund;
        gbHospitalPriceModifier = sGeneralInfo.bHospitalPriceModifier;
        gfPlayerTeamSawJoey = sGeneralInfo.fPlayerTeamSawJoey;
        gfMikeShouldSayHi = sGeneralInfo.fMikeShouldSayHi;

        return (true);
    }

    bool SavePreRandomNumbersToSaveGameFile(Stream hFile)
    {
        int uiNumBytesWritten;

        //Save the Prerandom number index
        FileWrite(hFile, guiPreRandomIndex, Marshal.SizeOf<int>(), out uiNumBytesWritten);
        if (uiNumBytesWritten != Marshal.SizeOf<int>())
        {
            return (false);
        }

        //Save the Prerandom number index
        FileWrite(hFile, guiPreRandomNums, Marshal.SizeOf<int>() * MAX_PREGENERATED_NUMS, out uiNumBytesWritten);
        if (uiNumBytesWritten != Marshal.SizeOf<int>() * MAX_PREGENERATED_NUMS)
        {
            return (false);
        }

        return (true);
    }

    bool LoadPreRandomNumbersFromSaveGameFile(Stream hFile)
    {
        int uiNumBytesRead;

        //Load the Prerandom number index
        this.files.FileRead(hFile, ref guiPreRandomIndex, Marshal.SizeOf<int>(), out uiNumBytesRead);
        if (uiNumBytesRead != Marshal.SizeOf<int>())
        {
            return (false);
        }

        //Load the Prerandom number index
        this.files.FileRead(hFile, ref guiPreRandomNums, Marshal.SizeOf<int>() * MAX_PREGENERATED_NUMS, out uiNumBytesRead);
        if (uiNumBytesRead != Marshal.SizeOf<int>() * MAX_PREGENERATED_NUMS)
        {
            return (false);
        }

        return (true);
    }

    bool LoadMeanwhileDefsFromSaveGameFile(Stream hFile)
    {
        int uiNumBytesRead;

        if (guiSaveGameVersion < 72)
        {
            //Load the array of meanwhile defs
            FileRead(hFile, gMeanwhileDef, Marshal.SizeOf<MEANWHILE_DEFINITION>() * (NUM_MEANWHILES - 1), out uiNumBytesRead);
            if (uiNumBytesRead != Marshal.SizeOf<MEANWHILE_DEFINITION>() * (NUM_MEANWHILES - 1))
            {
                return (false);
            }
            // and set the last one
            memset(&(gMeanwhileDef[NUM_MEANWHILES - 1]), 0, Marshal.SizeOf<MEANWHILE_DEFINITION>());

        }
        else
        {
            //Load the array of meanwhile defs
            FileRead(hFile, gMeanwhileDef, Marshal.SizeOf<MEANWHILE_DEFINITION>() * NUM_MEANWHILES, out uiNumBytesRead);
            if (uiNumBytesRead != Marshal.SizeOf<MEANWHILE_DEFINITION>() * NUM_MEANWHILES)
            {
                return (false);
            }
        }

        return (true);
    }

    bool SaveMeanwhileDefsFromSaveGameFile(Stream hFile)
    {
        int uiNumBytesWritten;

        //Save the array of meanwhile defs
        FileWrite(hFile, gMeanwhileDef, Marshal.SizeOf<MEANWHILE_DEFINITION>() * NUM_MEANWHILES, out uiNumBytesWritten);
        if (uiNumBytesWritten != Marshal.SizeOf<MEANWHILE_DEFINITION>() * NUM_MEANWHILES)
        {
            return (false);
        }

        return (true);
    }

    bool DoesUserHaveEnoughHardDriveSpace()
    {
        int uiBytesFree = 0;

        uiBytesFree = GetFreeSpaceOnHardDriveWhereGameIsRunningFrom();

        //check to see if there is enough hard drive space
        if (uiBytesFree < REQUIRED_FREE_SPACE)
        {
            return (false);
        }

        return (true);
    }

#if JA2BETAVERSION

void InitShutDownMapTempFileTest(bool fInit, STR pNameOfFile, int ubSaveGameID)
{
    CHAR8 zFileName[128];
    Stream hFile;
    CHAR8 zTempString[512];
    int uiStrLen;
    int uiNumBytesWritten;

    //strcpy( gzNameOfMapTempFile, pNameOfFile);
    sprintf(gzNameOfMapTempFile, "%s%d", pNameOfFile, ubSaveGameID);

    sprintf(zFileName, "%S\\%s.txt", pMessageStrings[MGS.SAVEDIRECTORY], gzNameOfMapTempFile);

    if (fInit)
    {
        guiNumberOfMapTempFiles = 0;        //Test:  To determine where the temp files are crashing
        guiSizeOfTempFiles = 0;

        if (FileExists(zFileName))
        {
            FileDelete(zFileName);
        }
    }
    else
    {
        // create the save game file
        hFile = FileOpen(zFileName, FILE_ACCESS_WRITE | FILE_OPEN_ALWAYS, false);
        if (!hFile)
        {
            FileClose(hFile);
            return;
        }

        FileSeek(hFile, 0, FILE_SEEK_FROM_END);

        sprintf(zTempString, "Number Of Files: %6d.  Size of all files: %6d.\n", guiNumberOfMapTempFiles, guiSizeOfTempFiles);
        uiStrLen = strlen(zTempString);

        FileWrite(hFile, zTempString, uiStrLen, out uiNumBytesWritten);
        if (uiNumBytesWritten != uiStrLen)
        {
            FileClose(hFile);
            return;
        }

        FileClose(hFile);

    }
}

void WriteTempFileNameToFile(STR pFileName, int uiSizeOfFile, HFILE hSaveFile)
{
    Stream hFile;
    CHAR8 zTempString[512];
    int uiNumBytesWritten;
    int uiStrLen = 0;

    CHAR8 zFileName[128];

    guiSizeOfTempFiles += uiSizeOfFile;

    sprintf(zFileName, "%S\\%s.txt", pMessageStrings[MGS.SAVEDIRECTORY], gzNameOfMapTempFile);

    // create the save game file
    hFile = FileOpen(zFileName, FILE_ACCESS_WRITE | FILE_OPEN_ALWAYS, false);
    if (!hFile)
    {
        FileClose(hFile);
        return;
    }

    FileSeek(hFile, 0, FILE_SEEK_FROM_END);

    sprintf(zTempString, "%8d   %6d   %s\n", FileGetPos(hSaveFile), uiSizeOfFile, pFileName);
    uiStrLen = strlen(zTempString);

    FileWrite(hFile, zTempString, uiStrLen, out uiNumBytesWritten);
    if (uiNumBytesWritten != uiStrLen)
    {
        FileClose(hFile);
        return;
    }

    FileClose(hFile);
}

#endif

    void GetBestPossibleSectorXYZValues(out int psSectorX, out MAP_ROW psSectorY, out int pbSectorZ)
    {
        //if the current sector is valid
        if (gfWorldLoaded)
        {
            psSectorX = gWorldSectorX;
            psSectorY = gWorldSectorY;
            pbSectorZ = gbWorldSectorZ;
        }
        else if (iCurrentTacticalSquad != NO_CURRENT_SQUAD && Squad[iCurrentTacticalSquad][0])
        {
            if (Squad[iCurrentTacticalSquad][0].bAssignment != Assignments.IN_TRANSIT)
            {
                psSectorX = Squad[iCurrentTacticalSquad][0].sSectorX;
                psSectorY = Squad[iCurrentTacticalSquad][0].sSectorY;
                pbSectorZ = Squad[iCurrentTacticalSquad][0].bSectorZ;
            }
        }
        else
        {
            int sSoldierCnt;
            int bLastTeamID;
            int bCount = 0;
            bool fFoundAMerc = false;

            // Set locator to first merc
            sSoldierCnt = gTacticalStatus.Team[gbPlayerNum].bFirstID;
            bLastTeamID = gTacticalStatus.Team[gbPlayerNum].bLastID;

            //loop through all the mercs on the players team to find the one that is not moving
            //            for (pSoldier = MercPtrs[sSoldierCnt]; sSoldierCnt <= bLastTeamID; sSoldierCnt++, pSoldier++)
            foreach (var pSoldier in MercPtrs)
            {
                if (pSoldier.IsActive)
                {
                    if (pSoldier.bAssignment != Assignments.IN_TRANSIT && !pSoldier.fBetweenSectors)
                    {
                        //we found an alive, merc that is not moving
                        psSectorX = pSoldier.sSectorX;
                        psSectorY = pSoldier.sSectorY;
                        pbSectorZ = pSoldier.bSectorZ;
                        fFoundAMerc = true;
                        break;
                    }
                }
            }

            //if we didnt find a merc
            if (!fFoundAMerc)
            {
                // Set locator to first merc
                sSoldierCnt = gTacticalStatus.Team[gbPlayerNum].bFirstID;
                bLastTeamID = gTacticalStatus.Team[gbPlayerNum].bLastID;

                //loop through all the mercs and find one that is moving
                //                for (pSoldier = MercPtrs[sSoldierCnt]; sSoldierCnt <= bLastTeamID; sSoldierCnt++, pSoldier++)
                foreach (var pSoldier in MercPtrs)
                {
                    if (pSoldier.IsActive)
                    {
                        //we found an alive, merc that is not moving
                        psSectorX = pSoldier.sSectorX;
                        psSectorY = pSoldier.sSectorY;
                        pbSectorZ = pSoldier.bSectorZ;
                        fFoundAMerc = true;
                        break;
                    }
                }

                //if we STILL havent found a merc, give up and use the -1, -1, -1
                if (!fFoundAMerc)
                {
                    psSectorX = gWorldSectorX;
                    psSectorY = gWorldSectorY;
                    pbSectorZ = gbWorldSectorZ;
                }
            }
        }
    }


    void PauseBeforeSaveGame()
    {
        //if we are not in the save load screen
        if (guiCurrentScreen != ScreenName.SAVE_LOAD_SCREEN)
        {
            //Pause the game
            PauseGame();
        }
    }

    void UnPauseAfterSaveGame()
    {
        //if we are not in the save load screen
        if (guiCurrentScreen != ScreenName.SAVE_LOAD_SCREEN)
        {
            //UnPause time compression
            UnPauseGame();
        }
    }

    void TruncateStrategicGroupSizes()
    {
        GROUP* pGroup;
        SECTORINFO* pSector;
        int i;
        for (i = SEC_A1; i < SEC_P16; i++)
        {
            pSector = &SectorInfo[i];
            if (pSector.ubNumAdmins + pSector.ubNumTroops + pSector.ubNumElites > MAX_STRATEGIC_TEAM_SIZE)
            {
                if (pSector.ubNumAdmins > pSector.ubNumTroops)
                {
                    if (pSector.ubNumAdmins > pSector.ubNumElites)
                    {
                        pSector.ubNumAdmins = 20;
                        pSector.ubNumTroops = 0;
                        pSector.ubNumElites = 0;
                    }
                    else
                    {
                        pSector.ubNumAdmins = 0;
                        pSector.ubNumTroops = 0;
                        pSector.ubNumElites = 20;
                    }
                }
                else if (pSector.ubNumTroops > pSector.ubNumElites)
                {
                    if (pSector.ubNumTroops > pSector.ubNumAdmins)
                    {
                        pSector.ubNumAdmins = 0;
                        pSector.ubNumTroops = 20;
                        pSector.ubNumElites = 0;
                    }
                    else
                    {
                        pSector.ubNumAdmins = 20;
                        pSector.ubNumTroops = 0;
                        pSector.ubNumElites = 0;
                    }
                }
                else
                {
                    if (pSector.ubNumElites > pSector.ubNumTroops)
                    {
                        pSector.ubNumAdmins = 0;
                        pSector.ubNumTroops = 0;
                        pSector.ubNumElites = 20;
                    }
                    else
                    {
                        pSector.ubNumAdmins = 0;
                        pSector.ubNumTroops = 20;
                        pSector.ubNumElites = 0;
                    }
                }
            }
            //militia
            if (pSector.ubNumberOfCivsAtLevel[0] + pSector.ubNumberOfCivsAtLevel[1] + pSector.ubNumberOfCivsAtLevel[2] > MAX_STRATEGIC_TEAM_SIZE)
            {
                if (pSector.ubNumberOfCivsAtLevel[0] > pSector.ubNumberOfCivsAtLevel[1])
                {
                    if (pSector.ubNumberOfCivsAtLevel[0] > pSector.ubNumberOfCivsAtLevel[2])
                    {
                        pSector.ubNumberOfCivsAtLevel[0] = 20;
                        pSector.ubNumberOfCivsAtLevel[1] = 0;
                        pSector.ubNumberOfCivsAtLevel[2] = 0;
                    }
                    else
                    {
                        pSector.ubNumberOfCivsAtLevel[0] = 0;
                        pSector.ubNumberOfCivsAtLevel[1] = 0;
                        pSector.ubNumberOfCivsAtLevel[2] = 20;
                    }
                }
                else if (pSector.ubNumberOfCivsAtLevel[1] > pSector.ubNumberOfCivsAtLevel[2])
                {
                    if (pSector.ubNumberOfCivsAtLevel[1] > pSector.ubNumberOfCivsAtLevel[0])
                    {
                        pSector.ubNumberOfCivsAtLevel[0] = 0;
                        pSector.ubNumberOfCivsAtLevel[1] = 20;
                        pSector.ubNumberOfCivsAtLevel[2] = 0;
                    }
                    else
                    {
                        pSector.ubNumberOfCivsAtLevel[0] = 20;
                        pSector.ubNumberOfCivsAtLevel[1] = 0;
                        pSector.ubNumberOfCivsAtLevel[2] = 0;
                    }
                }
                else
                {
                    if (pSector.ubNumberOfCivsAtLevel[2] > pSector.ubNumberOfCivsAtLevel[1])
                    {
                        pSector.ubNumberOfCivsAtLevel[0] = 0;
                        pSector.ubNumberOfCivsAtLevel[1] = 0;
                        pSector.ubNumberOfCivsAtLevel[2] = 20;
                    }
                    else
                    {
                        pSector.ubNumberOfCivsAtLevel[0] = 0;
                        pSector.ubNumberOfCivsAtLevel[1] = 20;
                        pSector.ubNumberOfCivsAtLevel[2] = 0;
                    }
                }
            }
        }
        //Enemy groups
        pGroup = gpGroupList;
        while (pGroup)
        {
            if (!pGroup.fPlayer)
            {
                if (pGroup.pEnemyGroup.ubNumAdmins + pGroup.pEnemyGroup.ubNumTroops + pGroup.pEnemyGroup.ubNumElites > MAX_STRATEGIC_TEAM_SIZE)
                {
                    pGroup.ubGroupSize = 20;
                    if (pGroup.pEnemyGroup.ubNumAdmins > pGroup.pEnemyGroup.ubNumTroops)
                    {
                        if (pGroup.pEnemyGroup.ubNumAdmins > pGroup.pEnemyGroup.ubNumElites)
                        {
                            pGroup.pEnemyGroup.ubNumAdmins = 20;
                            pGroup.pEnemyGroup.ubNumTroops = 0;
                            pGroup.pEnemyGroup.ubNumElites = 0;
                        }
                        else
                        {
                            pGroup.pEnemyGroup.ubNumAdmins = 0;
                            pGroup.pEnemyGroup.ubNumTroops = 0;
                            pGroup.pEnemyGroup.ubNumElites = 20;
                        }
                    }
                    else if (pGroup.pEnemyGroup.ubNumTroops > pGroup.pEnemyGroup.ubNumElites)
                    {
                        if (pGroup.pEnemyGroup.ubNumTroops > pGroup.pEnemyGroup.ubNumAdmins)
                        {
                            pGroup.pEnemyGroup.ubNumAdmins = 0;
                            pGroup.pEnemyGroup.ubNumTroops = 20;
                            pGroup.pEnemyGroup.ubNumElites = 0;
                        }
                        else
                        {
                            pGroup.pEnemyGroup.ubNumAdmins = 20;
                            pGroup.pEnemyGroup.ubNumTroops = 0;
                            pGroup.pEnemyGroup.ubNumElites = 0;
                        }
                    }
                    else
                    {
                        if (pGroup.pEnemyGroup.ubNumElites > pGroup.pEnemyGroup.ubNumTroops)
                        {
                            pGroup.pEnemyGroup.ubNumAdmins = 0;
                            pGroup.pEnemyGroup.ubNumTroops = 0;
                            pGroup.pEnemyGroup.ubNumElites = 20;
                        }
                        else
                        {
                            pGroup.pEnemyGroup.ubNumAdmins = 0;
                            pGroup.pEnemyGroup.ubNumTroops = 20;
                            pGroup.pEnemyGroup.ubNumElites = 0;
                        }
                    }
                }
            }
            pGroup = pGroup.next;
        }
    }


    void UpdateMercMercContractInfo()
    {
        int ubCnt;
        SOLDIERTYPE* pSoldier;

        for (ubCnt = BIFF; ubCnt <= BUBBA; ubCnt++)
        {
            pSoldier = FindSoldierByProfileID(ubCnt, true);

            //if the merc is on the team
            if (pSoldier == null)
                continue;

            gMercProfiles[ubCnt].iMercMercContractLength = pSoldier.iTotalContractLength;

            pSoldier.iTotalContractLength = 0;
        }
    }

    int GetNumberForAutoSave(bool fLatestAutoSave)
    {
        string zFileName1 = string.Empty;//[256];
        string zFileName2 = string.Empty;// [256];
        Stream hFile;
        bool fFile1Exist, fFile2Exist;
        SGP_FILETIME CreationTime1, LastAccessedTime1, LastWriteTime1;
        SGP_FILETIME CreationTime2, LastAccessedTime2, LastWriteTime2;

        fFile1Exist = false;
        fFile2Exist = false;


        //The name of the file
        sprintf(zFileName1, "%S\\Auto%02d.%S", EnglishText.pMessageStrings[MSG.SAVEDIRECTORY], 0, EnglishText.pMessageStrings[MSG.SAVEEXTENSION]);
        sprintf(zFileName2, "%S\\Auto%02d.%S", EnglishText.pMessageStrings[MSG.SAVEDIRECTORY], 1, EnglishText.pMessageStrings[MSG.SAVEEXTENSION]);

        if (this.files.FileExists(zFileName1))
        {
            hFile = this.files.FileOpen(zFileName1, FILE_ACCESS_READ | FILE_OPEN_EXISTING, false);

            GetFileManFileTime(hFile, &CreationTime1, &LastAccessedTime1, &LastWriteTime1);

            this.files.FileClose(hFile);

            fFile1Exist = true;
        }

        if (this.files.FileExists(zFileName2))
        {
            hFile = this.files.FileOpen(zFileName2, FILE_ACCESS_READ | FILE_OPEN_EXISTING, false);

            GetFileManFileTime(hFile, &CreationTime2, &LastAccessedTime2, &LastWriteTime2);

            this.files.FileClose(hFile);

            fFile2Exist = true;
        }

        if (!fFile1Exist && !fFile2Exist)
            return (-1);
        else if (fFile1Exist && !fFile2Exist)
        {
            if (fLatestAutoSave)
                return (0);
            else
                return (-1);
        }
        else if (!fFile1Exist && fFile2Exist)
        {
            if (fLatestAutoSave)
                return (1);
            else
                return (-1);
        }
        else
        {
            if (CompareSGPFileTimes(&LastWriteTime1, &LastWriteTime2) > 0)
                return (0);
            else
                return (1);
        }
    }

    void HandleOldBobbyRMailOrders()
    {
        int iCnt;
        int iNewListCnt = 0;

        if (LaptopSaveInfo.usNumberOfBobbyRayOrderUsed != 0)
        {
            //Allocate memory for the list
            //            gpNewBobbyrShipments = MemAlloc(Marshal.SizeOf<NewBobbyRayOrderStruct>() * LaptopSaveInfo.usNumberOfBobbyRayOrderUsed);
            if (gpNewBobbyrShipments == null)
            {
                Debug.Assert(0);
                return;
            }

            giNumberOfNewBobbyRShipment = LaptopSaveInfo.usNumberOfBobbyRayOrderUsed;

            //loop through and add the old items to the new list
            for (iCnt = 0; iCnt < LaptopSaveInfo.usNumberOfBobbyRayOrderItems; iCnt++)
            {
                //if this slot is used
                if (LaptopSaveInfo.BobbyRayOrdersOnDeliveryArray[iCnt].fActive)
                {
                    //copy over the purchase info
                    memcpy(gpNewBobbyrShipments[iNewListCnt].BobbyRayPurchase,
                                    LaptopSaveInfo.BobbyRayOrdersOnDeliveryArray[iCnt].BobbyRayPurchase,
                                    Marshal.SizeOf<BobbyRayPurchaseStruct>() * MAX_PURCHASE_AMOUNT);

                    gpNewBobbyrShipments[iNewListCnt].fActive = true;
                    gpNewBobbyrShipments[iNewListCnt].ubDeliveryLoc = BR_DRASSEN;
                    gpNewBobbyrShipments[iNewListCnt].ubDeliveryMethod = 0;
                    gpNewBobbyrShipments[iNewListCnt].ubNumberPurchases = LaptopSaveInfo.BobbyRayOrdersOnDeliveryArray[iCnt].ubNumberPurchases;
                    gpNewBobbyrShipments[iNewListCnt].uiPackageWeight = 1;
                    gpNewBobbyrShipments[iNewListCnt].uiOrderedOnDayNum = GetWorldDay();
                    gpNewBobbyrShipments[iNewListCnt].fDisplayedInShipmentPage = true;

                    iNewListCnt++;
                }
            }

            //Clear out the old list
            LaptopSaveInfo.usNumberOfBobbyRayOrderUsed = 0;
            MemFree(LaptopSaveInfo.BobbyRayOrdersOnDeliveryArray);
            LaptopSaveInfo.BobbyRayOrdersOnDeliveryArray = null;
        }
    }


    int CalcJA2EncryptionSet(SAVED_GAME_HEADER pSaveGameHeader)
    {
        int uiEncryptionSet = 0;

        uiEncryptionSet = pSaveGameHeader.uiSavedGameVersion;
        uiEncryptionSet *= pSaveGameHeader.uiFlags;
        uiEncryptionSet += pSaveGameHeader.iCurrentBalance;
        uiEncryptionSet *= (pSaveGameHeader.ubNumOfMercsOnPlayersTeam + 1);
        uiEncryptionSet += pSaveGameHeader.bSectorZ * 3;
        uiEncryptionSet += pSaveGameHeader.ubLoadScreenID;

        if (pSaveGameHeader.fAlternateSector)
        {
            uiEncryptionSet += 7;
        }

        if (pSaveGameHeader.uiRandom % 2 == 0)
        {
            uiEncryptionSet++;

            if (pSaveGameHeader.uiRandom % 7 == 0)
            {
                uiEncryptionSet++;
                if (pSaveGameHeader.uiRandom % 23 == 0)
                {
                    uiEncryptionSet++;
                }
                if (pSaveGameHeader.uiRandom % 79 == 0)
                {
                    uiEncryptionSet += 2;
                }
            }
        }

#if GERMAN
    uiEncryptionSet *= 11;
#endif

        uiEncryptionSet = uiEncryptionSet % 10;

        uiEncryptionSet += pSaveGameHeader.uiDay / 10;

        uiEncryptionSet = uiEncryptionSet % 19;

        // now pick a different set of #s depending on what game options we've chosen
        if (pSaveGameHeader.sInitialGameOptions.fGunNut)
        {
            uiEncryptionSet += BASE_NUMBER_OF_ROTATION_ARRAYS * 6;
        }

        if (pSaveGameHeader.sInitialGameOptions.fSciFi)
        {
            uiEncryptionSet += BASE_NUMBER_OF_ROTATION_ARRAYS * 3;
        }

        switch (pSaveGameHeader.sInitialGameOptions.ubDifficultyLevel)
        {
            case DIF_LEVEL_EASY:
                uiEncryptionSet += 0;
                break;
            case DIF_LEVEL_MEDIUM:
                uiEncryptionSet += BASE_NUMBER_OF_ROTATION_ARRAYS;
                break;
            case DIF_LEVEL_HARD:
                uiEncryptionSet += BASE_NUMBER_OF_ROTATION_ARRAYS * 2;
                break;
        }

        return (uiEncryptionSet);
    }
}

public struct GENERAL_SAVE_INFO
{
    //The screen that the gaem was saved from
    public int uiCurrentScreen;
    public int uiCurrentUniqueSoldierId;
    //The music that was playing when the game was saved
    public int ubMusicMode;
    //Flag indicating that we have purchased something from Tony
    public bool fHavePurchasedItemsFromTony;
    //The selected soldier in tactical
    public int usSelectedSoldier;
    // The x and y scroll position
    public int sRenderCenterX;
    public int sRenderCenterY;
    public bool fAtLeastOneMercWasHired;
    //General Map screen state flags
    public bool fShowItemsFlag;
    public bool fShowTownFlag;
    public bool fShowTeamFlag;
    public bool fShowMineFlag;
    public bool fShowAircraftFlag;
    // is the helicopter available to player?
    public bool fHelicopterAvailable;
    // helicopter vehicle id
    public int iHelicopterVehicleId;
    // total distance travelled
    public int UNUSEDiTotalHeliDistanceSinceRefuel;
    // total owed to player
    public int iTotalAccumulatedCostByPlayer;
    // whether or not skyrider is alive and well? and on our side yet?
    public bool fSkyRiderAvailable;
    // skyrider engaging in a monologue
    public bool UNUSEDfSkyriderMonologue;
    // list of sector locations
    public int[][] UNUSED;// [2][ 2 ];
    // is the heli in the air?
    public bool fHelicopterIsAirBorne;
    // is the pilot returning straight to base?
    public bool fHeliReturnStraightToBase;
    // heli hovering
    public bool fHoveringHelicopter;
    // time started hovering
    public int uiStartHoverTime;
    // what state is skyrider's dialogue in in?
    public int uiHelicopterSkyriderTalkState;
    // the flags for skyrider events
    public bool fShowEstoniRefuelHighLight;
    public bool fShowOtherSAMHighLight;
    public bool fShowDrassenSAMHighLight;
    public int uiEnvWeather;
    public int ubDefaultButton;
    public bool fSkyriderEmptyHelpGiven;
    public bool fEnterMapDueToContract;
    public int ubHelicopterHitsTaken;
    public int ubQuitType;
    public bool fSkyriderSaidCongratsOnTakingSAM;
    public int sContractRehireSoldierID;
    //  public   GAME_OPTIONS GameOptions;
    public int uiSeedNumber;
    public int uiBaseJA2Clock;
    public int sCurInterfacePanel;
    public int ubSMCurrentMercID;
    public bool fFirstTimeInMapScreen;
    public bool fDisableDueToBattleRoster;
    public bool fDisableMapInterfaceDueToBattle;
    public int[] sBoxerGridNo;// [NUM_BOXERS];
    public int[] ubBoxerID;// [NUM_BOXERS];
    public bool[] fBoxerFought;// [NUM_BOXERS];
    public bool fHelicopterDestroyed;                               //if the chopper is destroyed
    public bool fShowMapScreenHelpText;                         //If true, displays help in mapscreen
    public int iSortStateForMapScreenList;
    public bool fFoundTixa;
    public int uiTimeOfLastSkyriderMonologue;
    public bool fShowCambriaHospitalHighLight;
    public bool fSkyRiderSetUp;
    public bool[] fRefuelingSiteAvailable;// [NUMBER_OF_REFUEL_SITES];
    //Meanwhile stuff
    public MEANWHILE_DEFINITION gCurrentMeanwhileDef;
    public bool ubPlayerProgressSkyriderLastCommentedOn;
    public bool gfMeanwhileTryingToStart;
    public bool gfInMeanwhile;
    // list of dead guys for squads...in id values . -1 means no one home 
    public int[][] sDeadMercs;// [NUMBER_OF_SQUADS][NUMBER_OF_SOLDIERS_PER_SQUAD];
                              // levels of publicly known noises
    public int[] gbPublicNoiseLevel;// [MAXTEAMS];
    public int gubScreenCount;
    public MEANWHILEFLAGS usOldMeanWhileFlags;
    public int iPortraitNumber;
    public int sWorldSectorLocationOfFirstBattle;
    public bool fUnReadMailFlag;
    public bool fNewMailFlag;
    public bool fOldUnReadFlag;
    public bool fOldNewMailFlag;
    public bool fShowMilitia;
    public bool fNewFilesInFileViewer;
    public bool fLastBoxingMatchWonByPlayer;
    public int uiUNUSED;
    public bool[] fSamSiteFound;// [NUMBER_OF_SAMS];
    public int ubNumTerrorists;
    public int ubCambriaMedicalObjects;
    public bool fDisableTacticalPanelButtons;
    public int sSelMapX;
    public int sSelMapY;
    public int iCurrentMapSectorZ;
    public int usHasPlayerSeenHelpScreenInCurrentScreen;
    public bool fHideHelpInAllScreens;
    public int ubBoxingMatchesWon;
    public int ubBoxersRests;
    public bool fBoxersResting;
    public int ubDesertTemperature;
    public int ubGlobalTemperature;
    public int sMercArriveSectorX;
    public int sMercArriveSectorY;
    public bool fCreatureMeanwhileScenePlayed;
    public int ubPlayerNum;
    //New stuff for the Prebattle interface / autoresolve
    public bool fPersistantPBI;
    public int ubEnemyEncounterCode;
    public bool ubExplicitEnemyEncounterCode;
    public bool fBlitBattleSectorLocator;
    public int ubPBSectorX;
    public MAP_ROW ubPBSectorY;
    public int ubPBSectorZ;
    public bool fCantRetreatInPBI;
    public bool fExplosionQueueActive;
    public int[] ubUnused;// [1];
    public MEANWHILEFLAGS uiMeanWhileFlags;
    public int bSelectedInfoChar;
    public int bHospitalPriceModifier;
    public int[] bUnused2;// [2];
    public int iHospitalTempBalance;
    public int iHospitalRefund;
    public int fPlayerTeamSawJoey;
    public int fMikeShouldSayHi;
    public int[] ubFiller;// [550];		//This structure should be 1024 bytes
}

public class SAVED_GAME_HEADER
{
    public int uiSavedGameVersion;
    public int[] zGameVersionNumber;// [GAME_VERSION_LENGTH];
    public string sSavedGameDesc;
    public int uiFlags;
    //The following will be used to quickly access info to display in the save/load screen
    public uint uiDay;
    public uint ubHour;
    public uint ubMin;
    public int sSectorX;
    public MAP_ROW sSectorY;
    public int bSectorZ;
    public int ubNumOfMercsOnPlayersTeam;
    public int iCurrentBalance;
    public ScreenName uiCurrentScreen;
    public bool fAlternateSector;
    public bool fWorldLoaded;
    public int ubLoadScreenID;       //The load screen that should be used when loading the saved game
                                     //    public GAME_OPTIONS sInitialGameOptions;   //need these in the header so we can get the info from it on the save load screen.
    public int uiRandom;
    public int[] ubFiller;// [110];

}
