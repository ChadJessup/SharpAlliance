using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Screens;

namespace SharpAlliance.Core;

public partial class Globals
{
    public static int guiBrokenSaveGameVersion = 0;

    public static bool gfSaveLoadScreenEntry = true;
    public static bool gfSaveLoadScreenExit = false;
    public static bool gfExitAfterMessageBox = false;
    public static int giSaveLoadMessageBox = -1;                    // SaveLoad pop up messages index value
    public static ScreenName guiSaveLoadExitScreen = ScreenName.SAVE_LOAD_SCREEN;
    //Contains the array of valid save game locations
    public static bool[] gbSaveGameArray = new bool[NUM_SAVE_GAMES];
    public static bool gfDoingQuickLoad = false;
    public static bool gfFailedToSaveGameWhenInsideAMessageBox = false;
    //This flag is used to diferentiate between loading a game and saveing a game.
    // gfSaveGame=true		For saving a game
    // gfSaveGame=false		For loading a game
    public static bool gfSaveGame = true;
    public static bool gfSaveLoadScreenButtonsCreated = false;
    public static int[] gbSaveGameSelectedLocation = new int[NUM_SAVE_GAMES];
    public static int gbSelectedSaveLocation = -1;
    public static int gbHighLightedLocation = -1;
    public static int gbLastHighLightedLocation = -1;
    public static int gbSetSlotToBeSelected = -1;
    public static int guiSlgBackGroundImage;
    public static int guiBackGroundAddOns;
    // The string that will contain the game desc text
    public static string gzGameDescTextField;// [SIZE_OF_SAVE_GAME_DESC] = { 0 };
    public static bool gfUserInTextInputMode = false;
    public static int gubSaveGameNextPass = 0;
    public static bool gfStartedFadingOut = false;
    public static bool gfCameDirectlyFromGame = false;
    public static bool gfLoadedGame = false;   //Used to know when a game has been loaded, the flag in gtacticalstatus might have been reset already
    public static bool gfLoadGameUponEntry = false;
    public static bool gfHadToMakeBasementLevels = false;
}
