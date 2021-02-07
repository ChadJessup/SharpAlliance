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
