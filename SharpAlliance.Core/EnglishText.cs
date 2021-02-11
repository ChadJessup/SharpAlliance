using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Screens;

namespace SharpAlliance.Core
{
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
            "Speech",
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
}
