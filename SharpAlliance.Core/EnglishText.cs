using System.Collections.Generic;
using SharpAlliance.Core.Screens;

namespace SharpAlliance.Core;

// TODO: Move to built-in internationalization stuff.
public static class EnglishText
{
    public static string[] gzCopyrightText = new string[]
    {
        "Copyright (C) 1999 Sir-tech Canada Ltd.  All rights reserved.",
    };

    public static string[] pMapScreenFastHelpTextList = new string[]
    {
        "To change a merc's assignment to such things as another squad, doctor or repair, click within the 'Assign' column",
        "To give a merc a destination in another sector, click within the 'Dest' column",
        "Once a merc has been given a movement order, time compression allows them to get going.",
        "Left click selects the sector. Left click again to give a merc movement orders, or Right click to get sector summary information.",
        "Press 'h' at any time in this screen to get this help dialogue up.",
        "Test Text",
        "Test Text",
        "Test Text",
        "Test Text",
        "There isn't much you can do on this screen until you arrive in Arulco. When you've finalized your team, click on the Time Compression button at the lower right. This will advance time until your team arrives in Arulco.",
    };

    //option Text
    public static string[] zOptionsToggleText = new string[]
    {
        "Hi Twitter!",
        "Mute Confirmations",
        "SubTitles",
        "Pause Text Dialogue",
        "Animate Smoke",
        "Blood n Gore",
        "Never Move My Mouse!",
        "Old Selection Method",
        "Show Movement Path",
        "Show Misses",
        "Real Time Confirmation",
        "Display sleep/wake notifications",
        "Use Metric System",
        "Merc Lights during Movement",
        "Snap Cursor to Mercs",
        "Snap Cursor to Doors",
        "Make Items Glow",
        "Show Tree Tops",
        "Show Wireframes",
        "Show 3D Cursor",
    };

    public static Dictionary<GameInitOptionScreenText, string> gzGIOScreenText = new()
    {
        { GameInitOptionScreenText.GIO_INITIAL_GAME_SETTINGS, "INITIAL GAME SETTINGS" },
        { GameInitOptionScreenText.GIO_GAME_STYLE_TEXT, "Game Style" },
        { GameInitOptionScreenText.GIO_REALISTIC_TEXT, "Realistic" },
        { GameInitOptionScreenText.GIO_SCI_FI_TEXT, "Sci Fi" },
        { GameInitOptionScreenText.GIO_GUN_OPTIONS_TEXT, "Gun Options" },
        { GameInitOptionScreenText.GIO_GUN_NUT_TEXT, "Tons of Guns" },
        { GameInitOptionScreenText.GIO_REDUCED_GUNS_TEXT, "Normal" },
        { GameInitOptionScreenText.GIO_DIF_LEVEL_TEXT, "Difficulty Level" },
        { GameInitOptionScreenText.GIO_EASY_TEXT, "Novice" },
        { GameInitOptionScreenText.GIO_MEDIUM_TEXT, "Experienced" },
        { GameInitOptionScreenText.GIO_HARD_TEXT, "Expert" },
        { GameInitOptionScreenText.GIO_OK_TEXT, "Ok" },
        { GameInitOptionScreenText.GIO_CANCEL_TEXT, "Cancel" },
        { GameInitOptionScreenText.GIO_GAME_SAVE_STYLE_TEXT, "Extra Difficulty" },
        { GameInitOptionScreenText.GIO_SAVE_ANYWHERE_TEXT, "Save Anytime" },
        { GameInitOptionScreenText.GIO_IRON_MAN_TEXT, "Iron Man" },
        { GameInitOptionScreenText.GIO_DISABLED_FOR_THE_DEMO_TEXT, "Disabled for Demo" },
    };

    public static string[] pUpdatePanelButtons = new[]
    {
        "Continue",
        "Stop",
    };


    public static Dictionary<MSG, string> pMessageStrings = new()
    {
        { MSG.EXITGAME, "Exit Game?" },
        { MSG.OK, "OK" },
        { MSG.YES, "YES" },
        { MSG.NO, "NO" },
        { MSG.CANCEL, "CANCEL" },
        { MSG.REHIRE, "REHIRE" },
        { MSG.LIE, "LIE" },
        { MSG.NODESC, "No description" }, //Save slots that don't have a description.
        { MSG.SAVESUCCESS, "Game Saved." },
        { MSG.SAVESLOTSUCCESS, "Game Saved." },
        { MSG.QUICKSAVE_NAME, "QuickSave" }, //The name of the quicksave file (filename, text reference)
        { MSG.SAVE_NAME, "SaveGame" },    //The name of the normal savegame file, such as SaveGame01, SaveGame02, etc.
        { MSG.SAVEEXTENSION, "sav" },             //The 3 character dos extension (represents sav)
        { MSG.SAVEDIRECTORY, "..\\SavedGames" }, //The name of the directory where games are saved.
        { MSG.DAY, "Day" },
        { MSG.MERCS, "Mercs" },
        { MSG.EMPTYSLOT, "Empty Slot" }, //An empty save game slot
        { MSG.DEMOWORD, "Demo" },                //Demo of JA2
        { MSG.DEBUGWORD, "Debug" },               //State of development of a project (JA2) that is a debug build
        { MSG.RELEASEWORD, "Release" },         //Release build for JA2
        { MSG.RPM, "rpm" },                 //Abbreviation for Rounds per minute -- the potential # of bullets fired in a minute.
        { MSG.MINUTE_ABBREVIATION, "min" },                 //Abbreviation for minute.
        { MSG.METER_ABBREVIATION, "m" },                       //One character abbreviation for meter (metric distance measurement unit).
        { MSG.ROUNDS_ABBREVIATION, "rnds" },                //Abbreviation for rounds (# of bullets)
        { MSG.KILOGRAM_ABBREVIATION, "kg" },                  //Abbreviation for kilogram (metric weight measurement unit)
        { MSG.POUND_ABBREVIATION, "lb" },                  //Abbreviation for pounds (Imperial weight measurement unit)
        { MSG.HOMEPAGE, "Home" },                //Home as in homepage on the internet.
        { MSG.USDOLLAR_ABBREVIATION, "USD" },                 //Abbreviation to US dollars
        { MSG.LOWERCASE_NA, "n/a" },                 //Lowercase acronym for not applicable.
        { MSG.MEANWHILE, "Meanwhile" },       //Meanwhile
        { MSG.ARRIVE, "%s has arrived in sector %s%s" }, //Name/Squad has arrived in sector A9.  Order must not change without notifying
                                                         //SirTech
        { MSG.VERSION, "Version" },
        { MSG.EMPTY_QUICK_SAVE_SLOT, "Empty Quick Save Slot" },
        { MSG.QUICK_SAVE_RESERVED_FOR_TACTICAL, "This slot is reserved for Quick Saves made from the tactical and map screens using ALT+S." },
        { MSG.OPENED, "Opened" },
        { MSG.CLOSED, "Closed" },
        { MSG.LOWDISKSPACE_WARNING, "You are running low on disk space.  You only have %sMB free and Jagged Alliance 2 requires %sMB." },
        { MSG.HIRED_MERC, "Hired %s from AIM" },
        { MSG.MERC_CAUGHT_ITEM, "%s has caught %s." },       //'Merc name' has caught 'item' -- let SirTech know if name comes after item.
        { MSG.MERC_TOOK_DRUG, "%s has taken the drug." }, //'Merc name' has taken the drug
        { MSG.MERC_HAS_NO_MEDSKILL, "%s has no medical skill" },//'Merc name' has no medical skill.

        //CDRom errors (such as ejecting CD while attempting to read the CD)
        { MSG.INTEGRITY_WARNING, "The integrity of the game has been compromised." },
        { MSG.CDROM_SAVE, "ERROR: Ejected CD-ROM" },

        //When firing heavier weapons in close quarters, you may not have enough room to do so.
        { MSG.CANT_FIRE_HERE, "There is no room to fire from here." },

        //Can't change stance due to objects in the way...
        { MSG.CANT_CHANGE_STANCE, "Cannot change stance at this time." },

        //Simple text indications that appear in the game, when the merc can do one of these things.
        { MSG.DROP, "Drop" },
        { MSG.THROW, "Throw" },
        { MSG.PASS, "Pass" },

        { MSG.ITEM_PASSED_TO_MERC, "%s passed to %s." }, //"Item" passed to "merc".  Please try to keep the item %s before the merc %s, otherwise,
                                                         //must notify SirTech.
        { MSG.NO_ROOM_TO_PASS_ITEM, "No room to pass %s to %s." }, //pass "item" to "merc".  Same instructions as above.

        //A list of attachments appear after the items.  Ex:  Kevlar vest ( Ceramic Plate 'Attached )'
        { MSG.END_ATTACHMENT_LIST, " Attached )" },

        //Cheat modes
        { MSG.CHEAT_LEVEL_ONE, "Cheat level ONE reached" },
        { MSG.CHEAT_LEVEL_TWO, "Cheat level TWO reached" },

        //Toggling various stealth modes
        { MSG.SQUAD_ON_STEALTHMODE, "Squad on stealth mode." },
        { MSG.SQUAD_OFF_STEALTHMODE, "Squad off stealth mode." },
        { MSG.MERC_ON_STEALTHMODE, "%s on stealth mode." },
        { MSG.MERC_OFF_STEALTHMODE, "%s off stealth mode." },

        //Wireframes are shown through buildings to reveal doors and windows that can't otherwise be seen in 
        //an isometric engine.  You can toggle this mode freely in the game.
        { MSG.WIREFRAMES_ADDED, "Extra Wireframes On" },
        { MSG.WIREFRAMES_REMOVED, "Extra Wireframes Off" },

        //These are used in the cheat modes for changing levels in the game.  Going from a basement level to
        //an upper level, etc.  
        { MSG.CANT_GO_UP, "Can't go up from this level..." },
        { MSG.CANT_GO_DOWN, "There are no lower levels..." },
        { MSG.ENTERING_LEVEL, "Entering basement level %d..." },
        { MSG.LEAVING_BASEMENT, "Leaving basement..." },
        { MSG.DASH_S, "'s" },      // used in the shop keeper inteface to mark the ownership of the item eg Red's gun
        { MSG.TACKING_MODE_OFF, "Follow mode OFF." },
        { MSG.TACKING_MODE_ON, "Follow mode ON." },
        { MSG.ThreeDCURSOR_OFF, "3D Cursor OFF." },
        { MSG.ThreeDCURSOR_ON, "3D Cursor ON." },
        { MSG.SQUAD_ACTIVE, "Squad %d active." },
        { MSG.CANT_AFFORD_TO_PAY_NPC_DAILY_SALARY_MSG, "You cannot afford to pay for %s's daily salary of %s" },    //first %s is the mercs name, the seconds is a string containing the salary
        { MSG.SKIP, "Skip" },
        { MSG.EPC_CANT_TRAVERSE, "%s cannot leave alone." },
        { MSG.CDROM_SAVE_GAME, "A save has been created called, SaveGame99.sav.  If needed, rename it to SaveGame01 - SaveGame10 and then you will have access to it in the Load screen." },
        { MSG.DRANK_SOME, "%s drank some %s" },
        { MSG.PACKAGE_ARRIVES, "A package has arrived in Drassen." },
        { MSG.JUST_HIRED_MERC_ARRIVAL_LOCATION_POPUP, "%s should arrive at the designated drop-off point (sector %s) on day %d, at approximately %s." },       //first %s is mercs name, next is the sector location and name where they will be arriving in, lastely is the day an the time of arrival
        { MSG.HISTORY_UPDATED, "History log updated." },
    };


    public static string[] gzCreditNames = new string[]
    {
        "Chris Camfield",
        "Shaun Lyng",
        "Kris Märnes",
        "Ian Currie",
        "Linda Currie",
        "Eric \"WTF\" Cheng",
        "Lynn Holowka",
        "Norman \"NRG\" Olsen",
        "George Brooks",
        "Andrew Stacey",
        "Scot Loving",
        "Andrew \"Big Cheese\" Emmons",
        "Dave \"The Feral\" French",
        "Alex Meduna",
        "Joey \"Joeker\" Whelan",
    };


    public static string[] gzCreditNameTitle = new string[]
    {
        "Game Internals Programmer", 			// Chris Camfield
    	"Co-designer/Writer",							// Shaun Lyng
    	"Strategic Systems & Editor Programmer",					//Kris \"The Cow Rape Man\" Marnes
    	"Producer/Co-designer",						// Ian Currie
    	"Co-designer/Map Designer",				// Linda Currie
    	"Artist",													// Eric \"WTF\" Cheng
    	"Beta Coordinator, Support",				// Lynn Holowka
    	"Artist Extraordinaire",						// Norman \"NRG\" Olsen
    	"Sound Guru",											// George Brooks
    	"Screen Designer/Artist",					// Andrew Stacey
    	"Lead Artist/Animator",						// Scot Loving
    	"Lead Programmer",									// Andrew \"Big Cheese Doddle\" Emmons
    	"Programmer",											// Dave French
    	"Strategic Systems & Game Balance Programmer",					// Alex Meduna
    	"Portraits Artist",								// Joey \"Joeker\" Whelan",
    };

    public static string[] gzCreditNameFunny = new string[]
    {
        "", 																			// Chris Camfield
    	"(still learning punctuation)",					// Shaun Lyng
    	"(\"It's done. I'm just fixing it\")",	//Kris \"The Cow Rape Man\" Marnes
    	"(getting much too old for this)",				// Ian Currie
    	"(and working on Wizardry 8)",						// Linda Currie
    	"(forced at gunpoint to also do QA)",			// Eric \"WTF\" Cheng
    	"(Left us for the CFSA - go figure...)",	// Lynn Holowka
    	"",																			// Norman \"NRG\" Olsen
    	"",																			// George Brooks
    	"(Dead Head and jazz lover)",						// Andrew Stacey
    	"(his real name is Robert)",							// Scot Loving
    	"(the only responsible person)",					// Andrew \"Big Cheese Doddle\" Emmons
    	"(can now get back to motocrossing)",	// Dave French
    	"(stolen from Wizardry 8)",							// Alex Meduna
    	"(did items and loading screens too!)",	// Joey \"Joeker\" Whelan",
    };

    public static string[] zNewTacticalMessages = new string[]
    {
        "Range to target: %d tiles",
        "Attaching the transmitter to your laptop computer.",
        "You cannot afford to hire %s",
        "For a limited time, the above fee covers the cost of the entire mission and includes the equipment listed below.",
        "Hire %s now and take advantage of our unprecedented 'one fee covers all' pricing.  Also included in this unbelievable offer is the mercenary's personal equipment at no charge.",
        "Fee",
        "There is someone else in the sector...",
        "Gun Range: %d tiles, Range to target: %d tiles",
        "Display Cover",
        "Line of Sight",
        "New Recruits cannot arrive there.",
        "Since your laptop has no transmitter, you won't be able to hire new team members.  Perhaps this would be a good time to load a saved game or start over!",
        "%s hears the sound of crumpling metal coming from underneath Jerry's body.  It sounds disturbingly like your laptop antenna being crushed.",  //the %s is the name of a merc.  @@@  Modified
    	"After scanning the note left behind by Deputy Commander Morris, %s senses an oppurtinity.  The note contains the coordinates for launching missiles against different towns in Arulco.  It also gives the coodinates of the origin - the missile facility.",
        "Noticing the control panel, %s figures the numbers can be reveresed, so that the missile might destroy this very facility.  %s needs to find an escape route.  The elevator appears to offer the fastest solution...",
        "This is an IRON MAN game and you cannot save when enemies are around.",	//	@@@  new text
    	"(Cannot save during combat)", //@@@@ new text
    	"The current campaign name is greater than 30 characters.",							// @@@ new text
    	"The current campaign cannot be found.",																	// @@@ new text
    	"Campaign: Default ( %S )",																							// @@@ new text
    	"Campaign: %S",																													// @@@ new text
    	"You have selected the campaign %S. This campaign is a player-modified version of the original Unfinished Business campaign. Are you sure you wish to play the %S campaign?",			// @@@ new text
    	"In order to use the editor, please select a campaign other than the default.",		///@@new
    };


    //This is the help text associated with the above toggles.
    public static Dictionary<TOPTION, string> zOptionsScreenHelpText = new()
    {
        //speech
        { TOPTION.SPEECH, "Keep this option ON if you want to hear character dialogue." },

        //Mute Confirmation
        { TOPTION.MUTE_CONFIRMATIONS, "Turns characters' verbal confirmations on or off." },

        //Subtitles
        { TOPTION.SUBTITLES, "Controls whether on-screen text is displayed for dialogue." },

        //Key to advance speech
        { TOPTION.KEY_ADVANCE_SPEECH, "If Subtitles are ON, turn this on also to be able to take your time reading NPC dialogue." },

        //Toggle smoke animation
        { TOPTION.ANIMATE_SMOKE, "Turn off this option if animating smoke slows down your game's framerate." },

        //Blood n Gore
        { TOPTION.BLOOD_N_GORE, "Turn this option OFF if blood offends you." },

        //Never move my mouse
        { TOPTION.DONT_MOVE_MOUSE, "Turn this option OFF to have your mouse automatically move over pop-up confirmation boxes when they appear." },

        //Old selection method
        { TOPTION.OLD_SELECTION_METHOD, "Turn this ON for character selection to work as in previous JAGGED ALLIANCE games (which is the opposite of how it works otherwise)." },

        //Show movement path
        { TOPTION.ALWAYS_SHOW_MOVEMENT_PATH, "Turn this ON to display movement paths in Real-time (or leave it off and use the SHIFT key when you do want them displayed)." },

        //show misses
        { TOPTION.SHOW_MISSES, "Turn ON to have the game show you where your bullets ended up when you \"miss\"." },

        //Real Time Confirmation
        { TOPTION.RTCONFIRM, "When ON, an additional \"safety\" click will be required for movement in Real-time." },

        //Sleep/Wake notification
        { TOPTION.SLEEPWAKE_NOTIFICATION, "When ON, you will be notified when mercs on \"assignment\" go to sleep and resume work." },

        //Use the metric system
        { TOPTION.USE_METRIC_SYSTEM, "When ON, uses the metric system for measurements; otherwise it uses the Imperial system." },

        //Merc Lighted movement
        { TOPTION.MERC_ALWAYS_LIGHT_UP, "When ON, the merc will light the ground while walking.  Turn OFF for faster frame rate." },

        //Smart cursor
        { TOPTION.SMART_CURSOR, "When ON, moving the cursor near your mercs will automatically highlight them." },

        //snap cursor to the door
        { TOPTION.SNAP_CURSOR_TO_DOOR, "When ON, moving the cursor near a door will automatically position the cursor over the door." },
        //glow items 
        { TOPTION.GLOW_ITEMS, "When ON, |Items continuously glow" },
        //toggle tree tops
        { TOPTION.TOGGLE_TREE_TOPS, "When ON, shows the |Tree tops." },
        //toggle wireframe
        { TOPTION.TOGGLE_WIREFRAME, "When ON, displays |Wireframes for obscured walls." },
        { TOPTION.CURSOR_3D, "When ON, the movement cursor is shown in 3D. ( |Home )" },
    };

    public static Dictionary<OptionsText, string> zOptionsText = new()
    {
        //button Text
        { OptionsText.OPT_SAVE_GAME, "Save Game" },
        { OptionsText.OPT_LOAD_GAME, "Load Game" },
        { OptionsText.OPT_MAIN_MENU, "Quit" },
        { OptionsText.OPT_DONE, "Done" },

        //Text above the slider bars
        { OptionsText.OPT_SOUND_FX, "Effects" },
        { OptionsText.OPT_SPEECH, "Speech" },
        { OptionsText.OPT_MUSIC, "Music" },

        //Confirmation pop when the user selects..
        { OptionsText.OPT_RETURN_TO_MAIN, "Quit game and return to the main menu?" },
        { OptionsText.OPT_NEED_AT_LEAST_SPEECH_OR_SUBTITLE_OPTION_ON, "You need either the Speech option, or the Subtitle option to be enabled." },
    };


    //SaveLoadScreen 
    public static string[] zSaveLoadText = new string[]
    {
        "Save Game",
        "Load Game",
        "Cancel",
        "Save Selected",
        "Load Selected",

        "Saved the game successfully",
        "ERROR saving the game!",
        "Loaded the game successfully",
        "ERROR loading the game!",

        "The game version in the saved game file is different then the current version.  It is most likely safe to continue.  Continue?",
        "The saved game files may be invalidated.  Do you want them all deleted?",

        //Translators, the next two strings are for the same thing.  The first one is for beta version releases and the second one
        //is used for the final version.  Please don't modify the "#ifdef JA2BETAVERSION" or the "#else" or the "#endif" as they are
        //used by the compiler and will cause program errors if modified/removed.  It's okay to translate the strings though.
        "Attempting to load an older version save.  Automatically update and load the save?",

        //Translators, the next two strings are for the same thing.  The first one is for beta version releases and the second one
        //is used for the final version.  Please don't modify the "#ifdef JA2BETAVERSION" or the "#else" or the "#endif" as they are
        //used by the compiler and will cause program errors if modified/removed.  It's okay to translate the strings though.
        "Attempting to load an older version save.  Automatically update and load the save?",

        "Are you sure you want to overwrite the saved game in slot #%d?",
        "Do you want to load the game from slot #",

        //The first %d is a number that contains the amount of free space on the users hard drive,
        //the second is the recommended amount of free space.
        "You are running low on disk space.  You only have %d Megs free and Jagged should have at least %d Megs free.",

        //When saving a game, a message box with this string appears on the screen
        "Saving...",

        "Normal Guns",
        "Tons of Guns",
        "Realistic style",
        "Sci Fi style",

        "Difficulty",
    };
}
public enum MSG
{
    EXITGAME,
    OK,
    YES,
    NO,
    CANCEL,
    REHIRE,
    LIE,
    NODESC,
    SAVESUCCESS,
    SAVESLOTSUCCESS,
    QUICKSAVE_NAME,
    SAVE_NAME,
    SAVEEXTENSION,
    SAVEDIRECTORY,
    DAY,
    MERCS,
    EMPTYSLOT,
    DEMOWORD,
    DEBUGWORD,
    RELEASEWORD,
    RPM,
    MINUTE_ABBREVIATION,
    METER_ABBREVIATION,
    ROUNDS_ABBREVIATION,
    KILOGRAM_ABBREVIATION,
    POUND_ABBREVIATION,
    HOMEPAGE,
    USDOLLAR_ABBREVIATION,
    LOWERCASE_NA,
    MEANWHILE,
    ARRIVE,
    VERSION,
    EMPTY_QUICK_SAVE_SLOT,
    QUICK_SAVE_RESERVED_FOR_TACTICAL,
    OPENED,
    CLOSED,
    LOWDISKSPACE_WARNING,
    HIRED_MERC,
    MERC_CAUGHT_ITEM,
    MERC_TOOK_DRUG,
    MERC_HAS_NO_MEDSKILL,
    INTEGRITY_WARNING,
    CDROM_SAVE,
    CANT_FIRE_HERE,
    CANT_CHANGE_STANCE,
    DROP,
    THROW,
    PASS,
    ITEM_PASSED_TO_MERC,
    NO_ROOM_TO_PASS_ITEM,
    END_ATTACHMENT_LIST,
    CHEAT_LEVEL_ONE,
    CHEAT_LEVEL_TWO,
    SQUAD_ON_STEALTHMODE,
    SQUAD_OFF_STEALTHMODE,
    MERC_ON_STEALTHMODE,
    MERC_OFF_STEALTHMODE,
    WIREFRAMES_ADDED,
    WIREFRAMES_REMOVED,
    CANT_GO_UP,
    CANT_GO_DOWN,
    ENTERING_LEVEL,
    LEAVING_BASEMENT,
    DASH_S,             // the old 's
    TACKING_MODE_OFF,
    TACKING_MODE_ON,
    ThreeDCURSOR_OFF,
    ThreeDCURSOR_ON,
    SQUAD_ACTIVE,
    CANT_AFFORD_TO_PAY_NPC_DAILY_SALARY_MSG,
    SKIP,
    EPC_CANT_TRAVERSE,
    CDROM_SAVE_GAME,
    DRANK_SOME,
    PACKAGE_ARRIVES,
    JUST_HIRED_MERC_ARRIVAL_LOCATION_POPUP,
    HISTORY_UPDATED,
};

public enum TCTL_MSG__
{
    RANGE_TO_TARGET,
    ATTACH_TRANSMITTER_TO_LAPTOP,
    CANNOT_AFFORD_MERC,
    AIMMEMBER_FEE_TEXT,
    AIMMEMBER_ONE_TIME_FEE,
    FEE,
    SOMEONE_ELSE_IN_SECTOR,
    RANGE_TO_TARGET_AND_GUN_RANGE,
    DISPLAY_COVER,
    LOS,
    INVALID_DROPOFF_SECTOR,
    PLAYER_LOST_SHOULD_RESTART,
    JERRY_BREAKIN_LAPTOP_ANTENA,
    END_GAME_POPUP_TXT_1,
    END_GAME_POPUP_TXT_2,
    IRON_MAN_CANT_SAVE_NOW,
    CANNOT_SAVE_DURING_COMBAT,
    CAMPAIGN_NAME_TOO_LARGE,
    CAMPAIGN_DOESN_T_EXIST,
    DEFAULT_CAMPAIGN_LABEL,
    CAMPAIGN_LABEL,
    NEW_CAMPAIGN_CONFIRM,
    CANT_EDIT_DEFAULT,
};

public enum STR
{
    AIR_RAID_TURN_STR,
    BEGIN_AUTOBANDAGE_PROMPT_STR,
    NOTICING_MISSING_ITEMS_FROM_SHIPMENT_STR,
    DOOR_LOCK_DESCRIPTION_STR,
    DOOR_THERE_IS_NO_LOCK_STR,
    DOOR_LOCK_DESTROYED_STR,
    DOOR_LOCK_NOT_DESTROYED_STR,
    DOOR_LOCK_HAS_BEEN_PICKED_STR,
    DOOR_LOCK_HAS_NOT_BEEN_PICKED_STR,
    DOOR_LOCK_UNTRAPPED_STR,
    DOOR_LOCK_HAS_BEEN_UNLOCKED_STR,
    DOOR_NOT_PROPER_KEY_STR,
    DOOR_LOCK_HAS_BEEN_UNTRAPPED_STR,
    DOOR_LOCK_IS_NOT_TRAPPED_STR,
    DOOR_LOCK_HAS_BEEN_LOCKED_STR,
    DOOR_DOOR_MOUSE_DESCRIPTION,
    DOOR_TRAPPED_MOUSE_DESCRIPTION,
    DOOR_LOCKED_MOUSE_DESCRIPTION,
    DOOR_UNLOCKED_MOUSE_DESCRIPTION,
    DOOR_BROKEN_MOUSE_DESCRIPTION,
    ACTIVATE_SWITCH_PROMPT,
    DISARM_TRAP_PROMPT,
    ITEMPOOL_POPUP_PREV_STR,
    ITEMPOOL_POPUP_NEXT_STR,
    ITEMPOOL_POPUP_MORE_STR,
    ITEM_HAS_BEEN_PLACED_ON_GROUND_STR,
    ITEM_HAS_BEEN_GIVEN_TO_STR,
    GUY_HAS_BEEN_PAID_IN_FULL_STR,
    GUY_STILL_OWED_STR,
    CHOOSE_BOMB_FREQUENCY_STR,
    CHOOSE_TIMER_STR,
    CHOOSE_REMOTE_FREQUENCY_STR,
    DISARM_BOOBYTRAP_PROMPT,
    REMOVE_BLUE_FLAG_PROMPT,
    PLACE_BLUE_FLAG_PROMPT,
    ENDING_TURN,
    ATTACK_OWN_GUY_PROMPT,
    VEHICLES_NO_STANCE_CHANGE_STR,
    ROBOT_NO_STANCE_CHANGE_STR,
    CANNOT_STANCE_CHANGE_STR,
    CANNOT_DO_FIRST_AID_STR,
    CANNOT_NO_NEED_FIRST_AID_STR,
    CANT_MOVE_THERE_STR,
    CANNOT_RECRUIT_TEAM_FULL,
    HAS_BEEN_RECRUITED_STR,
    BALANCE_OWED_STR,
    ESCORT_PROMPT,
    HIRE_PROMPT,
    BOXING_PROMPT,
    BUY_VEST_PROMPT,
    NOW_BING_ESCORTED_STR,
    JAMMED_ITEM_STR,
    ROBOT_NEEDS_GIVEN_CALIBER_STR,
    CANNOT_THROW_TO_DEST_STR,
    TOGGLE_STEALTH_MODE_POPUPTEXT,
    MAPSCREEN_POPUPTEXT,
    END_TURN_POPUPTEXT,
    TALK_CURSOR_POPUPTEXT,
    TOGGLE_MUTE_POPUPTEXT,
    CHANGE_STANCE_UP_POPUPTEXT,
    CURSOR_LEVEL_POPUPTEXT,
    JUMPCLIMB_POPUPTEXT,
    CHANGE_STANCE_DOWN_POPUPTEXT,
    EXAMINE_CURSOR_POPUPTEXT,
    PREV_MERC_POPUPTEXT,
    NEXT_MERC_POPUPTEXT,
    CHANGE_OPTIONS_POPUPTEXT,
    TOGGLE_BURSTMODE_POPUPTEXT,
    LOOK_CURSOR_POPUPTEXT,
    MERC_VITAL_STATS_POPUPTEXT,
    CANNOT_DO_INV_STUFF_STR,
    CONTINUE_OVER_FACE_STR,
    MUTE_OFF_STR,
    MUTE_ON_STR,
    DRIVER_POPUPTEXT,
    EXIT_VEHICLE_POPUPTEXT,
    CHANGE_SQUAD_POPUPTEXT,
    DRIVE_POPUPTEXT,
    NOT_APPLICABLE_POPUPTEXT,
    USE_HANDTOHAND_POPUPTEXT,
    USE_FIREARM_POPUPTEXT,
    USE_BLADE_POPUPTEXT,
    USE_EXPLOSIVE_POPUPTEXT,
    USE_MEDKIT_POPUPTEXT,
    CATCH_STR,
    RELOAD_STR,
    GIVE_STR,
    LOCK_TRAP_HAS_GONE_OFF_STR,
    MERC_HAS_ARRIVED_STR,
    GUY_HAS_RUN_OUT_OF_APS_STR,
    MERC_IS_UNAVAILABLE_STR,
    MERC_IS_ALL_BANDAGED_STR,
    MERC_IS_OUT_OF_BANDAGES_STR,
    ENEMY_IN_SECTOR_STR,
    NO_ENEMIES_IN_SIGHT_STR,
    NOT_ENOUGH_APS_STR,
    NOBODY_USING_REMOTE_STR,
    BURST_FIRE_DEPLETED_CLIP_STR,
    ENEMY_TEAM_MERC_NAME,
    CREATURE_TEAM_MERC_NAME,
    MILITIA_TEAM_MERC_NAME,
    CIV_TEAM_MERC_NAME,

    //The text for the 'exiting sector' gui
    EXIT_GUI_TITLE_STR,
    OK_BUTTON_TEXT_STR,
    CANCEL_BUTTON_TEXT_STR,
    EXIT_GUI_SELECTED_MERC_STR,
    EXIT_GUI_ALL_MERCS_IN_SQUAD_STR,
    EXIT_GUI_GOTO_SECTOR_STR,
    EXIT_GUI_GOTO_MAP_STR,
    CANNOT_LEAVE_SECTOR_FROM_SIDE_STR,
    MERC_IS_TOO_FAR_AWAY_STR,
    REMOVING_TREETOPS_STR,
    SHOWING_TREETOPS_STR,
    CROW_HIT_LOCATION_STR,
    NECK_HIT_LOCATION_STR,
    HEAD_HIT_LOCATION_STR,
    TORSO_HIT_LOCATION_STR,
    LEGS_HIT_LOCATION_STR,
    YESNOLIE_STR,
    GUN_GOT_FINGERPRINT,
    GUN_NOGOOD_FINGERPRINT,
    GUN_GOT_TARGET,
    NO_PATH,
    MONEY_BUTTON_HELP_TEXT,
    AUTOBANDAGE_NOT_NEEDED,
    SHORT_JAMMED_GUN,
    CANT_GET_THERE,
    EXCHANGE_PLACES_REQUESTER,
    REFUSE_EXCHANGE_PLACES,
    PAY_MONEY_PROMPT,
    FREE_MEDICAL_PROMPT,
    MARRY_DARYL_PROMPT,
    KEYRING_HELP_TEXT,
    EPC_CANNOT_DO_THAT,
    SPARE_KROTT_PROMPT,
    OUT_OF_RANGE_STRING,
    CIV_TEAM_MINER_NAME,
    VEHICLE_CANT_MOVE_IN_TACTICAL,
    CANT_AUTOBANDAGE_PROMPT,
    NO_PATH_FOR_MERC,
    POW_MERCS_ARE_HERE,
    LOCK_HAS_BEEN_HIT,
    LOCK_HAS_BEEN_DESTROYED,
    DOOR_IS_BUSY,
    VEHICLE_VITAL_STATS_POPUPTEXT,
    NO_LOS_TO_TALK_TARGET,
};
