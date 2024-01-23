using System.Collections.Generic;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems.LaptopSubSystem;

namespace SharpAlliance.Core;

// TODO: Move to built-in internationalization stuff.
public static class EnglishText
{
    //Aim Home Page

    public static string[] AimBottomMenuText =
    {
    	//Text for the links at the bottom of all AIM pages
    	"Home",
        "Members",
        "Alumni",
        "Policies",
        "History",
        "Links",
    };


    // bookmarks for different websites
    // IMPORTANT make sure you move down the Cancel string as bookmarks are being added
    public static string[] pBookMarkStrings =
    {
        "A.I.M.",
        "Bobby Ray's",
        "I.M.P",
        "M.E.R.C.",
        "Mortuary",
        "Florist",
        "Insurance",
        "Cancel",
    };

    public static Dictionary<LaptopText, string> gzLaptopHelpText = new()
    {
    	//Buttons:
    	{ LaptopText.LAPTOP_BN_HLP_TXT_VIEW_EMAIL, "View email" },
        { LaptopText.LAPTOP_BN_HLP_TXT_BROWSE_VARIOUS_WEB_SITES, "Browse various web sites" },
        { LaptopText.LAPTOP_BN_HLP_TXT_VIEW_FILES_AND_EMAIL_ATTACHMENTS, "View files and email attachments" },
        { LaptopText.LAPTOP_BN_HLP_TXT_READ_LOG_OF_EVENTS, "Read log of events" },
        { LaptopText.LAPTOP_BN_HLP_TXT_VIEW_TEAM_INFO, "View team info" },
        { LaptopText.LAPTOP_BN_HLP_TXT_VIEW_FINANCIAL_SUMMARY_AND_HISTORY, "View financial summary and history" },
        { LaptopText.LAPTOP_BN_HLP_TXT_CLOSE_LAPTOP, "Close laptop" },
    
    	//Bottom task bar icons (if they exist):
    	{ LaptopText.LAPTOP_BN_HLP_TXT_YOU_HAVE_NEW_MAIL, "You have new mail" },
        { LaptopText.LAPTOP_BN_HLP_TXT_YOU_HAVE_NEW_FILE, "You have new file(s)" },
    
    	//Bookmarks:
    	{ LaptopText.BOOKMARK_TEXT_ASSOCIATION_OF_INTERNATION_MERCENARIES, "Association of International Mercenaries" },
        { LaptopText.BOOKMARK_TEXT_BOBBY_RAY_ONLINE_WEAPON_MAIL_ORDER, "Bobby Ray's online weapon mail order" },
        { LaptopText.BOOKMARK_TEXT_INSTITUTE_OF_MERCENARY_PROFILING, "Institute of Mercenary Profiling" },
        { LaptopText.BOOKMARK_TEXT_MORE_ECONOMIC_RECRUITING_CENTER, "More Economic Recruiting Center" },
        { LaptopText.BOOKMARK_TEXT_MCGILLICUTTY_MORTUARY, "McGillicutty's Mortuary" },
        { LaptopText.BOOKMARK_TEXT_UNITED_FLORAL_SERVICE, "United Floral Service" },
        { LaptopText.BOOKMARK_TEXT_INSURANCE_BROKERS_FOR_AIM_CONTRACTS, "Insurance Brokers for A.I.M. contracts" },
    };

    public static string[] pLaptopTitles =
    {
        "Mail Box",
        "File Viewer",
        "Personnel",
        "Bookkeeper Plus",
        "History Log",
    };

    // Web error messages. Please use foreign language equivilant for these messages. 
    // DNS is the acronym for Domain Name Server
    // URL is the acronym for Uniform Resource Locator

    public static string[] pErrorStrings =
    {
        "Error",
        "Server does not have DNS entry.",
        "Check URL address and try again.",
        "OK",
        "Intermittent Connection to Host. Expect longer transfer times.",
    };


    public static string[] pWebTitle =
    {
        "sir-FER 4.0",		// our name for the version of the browser, play on company name
    };


    // The titles for the web program title bar, for each page loaded

    public static string[] pWebPagesTitles =
    {
        "A.I.M.",
        "A.I.M. Members",
        "A.I.M. Mug Shots",		// a mug shot is another name for a portrait
	    "A.I.M. Sort",
        "A.I.M.",
        "A.I.M. Alumni",
        "A.I.M. Policies",
        "A.I.M. History",
        "A.I.M. Links",
        "M.E.R.C.",
        "M.E.R.C. Accounts",
        "M.E.R.C. Registration",
        "M.E.R.C. Index",
        "Bobby Ray's",
        "Bobby Ray's - Guns",
        "Bobby Ray's - Ammo",
        "Bobby Ray's - Armor",
        "Bobby Ray's - Misc",							//misc is an abbreviation for miscellaneous				
	    "Bobby Ray's - Used",
        "Bobby Ray's - Mail Order",
        "I.M.P.",
        "I.M.P.",
        "United Floral Service",
        "United Floral Service - Gallery",
        "United Floral Service - Order Form",
        "United Floral Service - Card Gallery",
        "Malleus, Incus & Stapes Insurance Brokers",
        "Information",
        "Contract",
        "Comments",
        "McGillicutty's Mortuary",
        "",
        "URL not found.",
        "Bobby Ray's - Recent Shipments",
        "",
        "",
    };

    public static string[] pShowBookmarkString =
    {
        "Sir-Help",
        "Click Web Again for Bookmarks.",
    };


    public static string[] pPersonnelString =
    {
        "Mercs:", 			// mercs we have
    };


    // icon text strings that appear on the laptop

    public static string[] pLaptopIcons =
    {
        "E-mail",
        "Web",
        "Financial",
        "Personnel",
        "History",
        "Files",
        "Shut Down",
        "sir-FER 4.0",			// our play on the company name (Sirtech) and web surFER
    };


    public static string[] Message =
    [
        "",

        // In the following 8 strings, the %s is the merc's name, and the %d (if any) is a number.

        "%s is hit in the head and loses a point of wisdom!",
        "%s is hit in the shoulder and loses a point of dexterity!",
        "%s is hit in the chest and loses a point of strength!",
        "%s is hit in the legs and loses a point of agility!",
        "%s is hit in the head and loses %d points of wisdom!",
        "%s is hit in the shoulder and loses %d points of dexterity!",
        "%s is hit in the chest and loses %d points of strength!",
        "%s is hit in the legs and loses %d points of agility!",
        "Interrupt!",

        // The first %s is a merc's name, the second is a string from pNoiseVolStr,
        // the third is a string from pNoiseTypeStr, and the last is a string from pDirectionStr

        "", //OBSOLETE
        "Your reinforcements have arrived!",

        // In the following four lines, all %s's are merc names

        "%s reloads.",
        "%s doesn't have enough Action Points!",
        "%s is applying first aid. (Press any key to cancel.)",
        "%s and %s are applying first aid. (Press any key to cancel.)",
        // the following 17 strings are used to create lists of gun advantages and disadvantages
        // (separated by commas)
        "reliable",
        "unreliable",
        "easy to repair",
        "hard to repair",
        "high damage",
        "low damage",
        "quick firing",
        "slow firing",
        "long range",
        "short range",
        "light",
        "heavy",
        "small",
        "fast burst fire",
        "no burst fire",
        "large magazine",
        "small magazine",

        // In the following two lines, all %s's are merc names

        "%s's camouflage has worn off.",
        "%s's camouflage has washed off.",

        // The first %s is a merc name and the second %s is an item name

        "Second weapon is out of ammo!",
        "%s has stolen the %s.",

        // The %s is a merc name

        "%s's weapon can't burst fire.",

        "You've already got one of those attached.",
        "Merge items?",

        // Both %s's are item names

        "You can't attach a %s to a %s.",

        "None",
        "Eject ammo",
        "Attachments",

        //You cannot use "item(s)" and your "other item" at the same time.
        //Ex:  You cannot use sun goggles and you gas mask at the same time.
        "You cannot use %s and your %s at the same time.",
        "The item you have in your cursor can be attached to certain items by placing it in one of the four attachment slots.",
        "The item you have in your cursor can be attached to certain items by placing it in one of the four attachment slots. (However in this case, the item is not compatible.)",
        "The sector isn't cleared of enemies!",
        "You still need to give %s %s",
        "%s is hit in the head!",
        "Abandon the fight?",
        "This attachment will be permanent.  Go ahead with it?",
        "%s feels more energetic!",
        "%s slipped on some marbles!",
        "%s failed to grab the %s!",
        "%s has repaired the %s",
        "Interrupt for ",
        "Surrender?",
        "This person refuses your aid.",
        "I DON'T think so!",
        "To travel in Skyrider's chopper, you'll have to ASSIGN mercs to VEHICLE/HELICOPTER first.",
        "%s only had enough time to reload ONE gun",
        "Bloodcats' turn",
    ];

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

    // paused game strings
    public static string[] pPausedGameText =
    {
        "Game Paused",
        "Resume Game (|P|a|u|s|e)",
        "Pause Game (|P|a|u|s|e)",
    };

    // help text used when moving the merc arrival sector
    public static string[] pBullseyeStrings =
    {
        "Click on the sector where you would like the mercs to arrive instead.",
        "OK.  Arriving mercs will be dropped off in %s",
        "Mercs can't be flown there, the airspace isn't secured!",
        "Canceled.  Arrival sector unchanged",
        "Airspace over %s is no longer secure!  Arrival sector was moved to %s.",
    };

    // help text used during strategic route plotting
    public static string[] pMapPlotStrings =
    {
        "Click again on the destination to confirm your final route, or click on another sector to place more waypoints.",
        "Travel route confirmed.",
        "Destination unchanged.",
        "Travel route canceled.",
        "Travel route shortened.",
    };

    // helicopter pilot payment 

    public static string[] pSkyriderText =
    {
        "Skyrider was paid $%d", 			// skyrider was paid an amount of money
    	"Skyrider is still owed $%d", 		// skyrider is still owed an amount of money
    	"Skyrider has finished refueling",	// skyrider has finished refueling
    	"",//unused
    	"",//unused
    	"Skyrider is ready to fly once more.", // Skyrider was grounded but has been freed
    	"Skyrider has no passengers.  If it is your intention to transport mercs in this sector, assign them to Vehicle/Helicopter first.",
    };



    // Mine strings

    public static string[] pwMineStrings =
    {
        "Mine",						// 0
    	"Silver",
        "Gold",
        "Daily Production",
        "Possible Production",
        "Abandoned",				// 5
    	"Shut Down",
        "Running Out",
        "Producing",
        "Status",
        "Production Rate",
        "Ore Type",				// 10
    	"Town Control",
        "Town Loyalty",
    //	"Working Miners",
    };

    // These are the different terrain types. 

    public static Dictionary<Traversability, string> pLandTypeStrings = new()
    {
        { Traversability.TOWN , "Urban" },
        { Traversability.ROAD , "Road" },
        { Traversability.PLAINS , "Plains" },
        { Traversability.SAND, "Desert" },
        { Traversability.SPARSE, "Woods" },
        { Traversability.DENSE, "Forest" },
        { Traversability.SWAMP, "Swamp" },
        { Traversability.WATER, "Water" },
        { Traversability.HILLS, "Hills" },
        { Traversability.GROUNDBARRIER, "Impassable" },
        { Traversability.NS_RIVER, "River" },	//river from north to south
    	{ Traversability.EW_RIVER, "River" },	//river from east to west
    	{ Traversability.EDGEOFWORLD, "Foreign Country" },
    	//NONE of the following are used for directional travel, just for the sector description.
    	{ Traversability.TROPICS, "Tropical" },
        { Traversability.FARMLAND, "Farmland" },
        { Traversability.PLAINS_ROAD, "Plains, road" },
        { Traversability.SPARSE_ROAD, "Woods, road" },
        { Traversability.FARMLAND_ROAD, "Farm, road" },
        { Traversability.TROPICS_ROAD, "Tropical, road" },
        { Traversability.DENSE_ROAD, "Forest, road" },
        { Traversability.COASTAL, "Coastline" },
        { Traversability.HILLS_ROAD, "Mountain, road" },
        { Traversability.COASTAL_ROAD, "Coastal, road" },
        { Traversability.SAND_ROAD, "Desert, road" },
        { Traversability.SWAMP_ROAD, "Swamp, road" },
        { Traversability.SPARSE_SAM_SITE, "Woods, SAM site" },
        { Traversability.SAND_SAM_SITE, "Desert, SAM site" },
        { Traversability.TROPICS_SAM_SITE, "Tropical, SAM site" },
        { Traversability.MEDUNA_SAM_SITE, "Meduna, SAM site" },
    	
    	//These are descriptions for special sectors
    	{ Traversability.CAMBRIA_HOSPITAL_SITE, "Cambria Hospital" },
        { Traversability.DRASSEN_AIRPORT_SITE, "Drassen Airport" },
        { Traversability.MEDUNA_AIRPORT_SITE, "Meduna Airport" },
        { Traversability.SAM_SITE, "SAM site" },
        { Traversability.REBEL_HIDEOUT, "Rebel Hideout" }, //The rebel base underground in sector A10
    	{ Traversability.TIXA_DUNGEON, "Tixa Dungeon" },	//The basement of the Tixa Prison (J9)
    	{ Traversability.CREATURE_LAIR, "Creature Lair" },	//Any mine sector with creatures in it
    	{ Traversability.ORTA_BASEMENT, "Orta Basement" }, //The basement of Orta (K4)
    	{ Traversability.TUNNEL, "Tunnel" },				//The tunnel access from the maze garden in Meduna 
    									//leading to the secret shelter underneath the palace
    	{ Traversability.SHELTER, "Shelter" },				//The shelter underneath the queen's palace
    	{ Traversability.ABANDONED_MINE, "" },							//Unused
    };

    public static Dictionary<GIO_CFS, string> zGioDifConfirmText = new()
    {
        { GIO_CFS.NOVICE , "You have chosen NOVICE mode. This setting is appropriate for those new to Jagged Alliance, those new to strategy games in general, or those wishing shorter battles in the game. Your choice will affect things throughout the entire course of the game, so choose wisely. Are you sure you want to play in Novice mode?" },
        { GIO_CFS.EXPERIENCED , "You have chosen EXPERIENCED mode. This setting is suitable for those already familiar with Jagged Alliance or similar games. Your choice will affect things throughout the entire course of the game, so choose wisely. Are you sure you want to play in Experienced mode?"},
        { GIO_CFS.EXPERT , "You have chosen EXPERT mode. We warned you. Don't blame us if you get shipped back in a body bag. Your choice will affect things throughout the entire course of the game, so choose wisely. Are you sure you want to play in Expert mode?"},
    };


    // NOTE: combine prestatbuildstring with statgain to get a line like the example below.
    // "John has gained 3 points of marksmanship skill." 

    public static string[] sPreStatBuildString =
    {
        "lost", 			// the merc has lost a statistic
    	"gained", 		// the merc has gained a statistic
    	"point of",	// singular
    	"points of",	// plural
    	"level of",	// singular
    	"levels of",	// plural
    };

    public static string[] sStatGainStrings =
    {
        "health.",
        "agility.",
        "dexterity.",
        "wisdom.",
        "medical skill.",
        "explosives skill.",
        "mechanical skill.",
        "marksmanship skill.",
        "experience.",
        "strength.",
        "leadership.",
    };

    public static string[] pFinanceHeaders =
    {
        "Day", 				// the day column
    	"Credit", 			// the credits column (to ADD money to your account)
    	"Debit",				// the debits column (to SUBTRACT money from your account)
    	"Transaction", // transaction type - see TransactionText below
    	"Balance", 		// balance at this point in time
    	"Page", 				// page number
    	"Day(s)", 			// the day(s) of transactions this page displays 
    };


    public static string[] pMessageStrings2 =
    {
        "Exit Game?",
        "OK",
        "YES",
        "NO",
        "CANCEL",
        "REHIRE",
        "LIE",
        "No description", //Save slots that don't have a description.
    	"Game Saved.",
        "Game Saved.",
        "QuickSave", //The name of the quicksave file (filename, text reference)
    	"SaveGame",	//The name of the normal savegame file, such as SaveGame01, SaveGame02, etc.
    	"sav",				//The 3 character dos extension (represents sav)
    	"..\\SavedGames", //The name of the directory where games are saved.
    	"Day",
        "Mercs",
        "Empty Slot", //An empty save game slot
    	"Demo",				//Demo of JA2
    	"Debug",				//State of development of a project (JA2) that is a debug build
    	"Release",			//Release build for JA2
    	"rpm",					//Abbreviation for Rounds per minute -- the potential # of bullets fired in a minute.
    	"min",					//Abbreviation for minute.
    	"m",						//One character abbreviation for meter (metric distance measurement unit).
    	"rnds",				//Abbreviation for rounds (# of bullets)
    	"kg",					//Abbreviation for kilogram (metric weight measurement unit)
    	"lb",					//Abbreviation for pounds (Imperial weight measurement unit)
    	"Home",				//Home as in homepage on the internet.
    	"USD",					//Abbreviation to US dollars
    	"n/a",					//Lowercase acronym for not applicable.
    	"Meanwhile",		//Meanwhile
    	"%s has arrived in sector %s%s", //Name/Squad has arrived in sector A9.  Order must not change without notifying
    																		//SirTech
    	"Version",
        "Empty Quick Save Slot",
        "This slot is reserved for Quick Saves made from the tactical and map screens using ALT+S.",
        "Opened",
        "Closed",
        "You are running low on disk space.  You only have %sMB free and Jagged Alliance 2 requires %sMB.",
        "Hired %s from AIM",
        "%s has caught %s.",		//'Merc name' has caught 'item' -- let SirTech know if name comes after item.
    	"%s has taken the drug.", //'Merc name' has taken the drug
    	"%s has no medical skill",//'Merc name' has no medical skill.
    
    	//CDRom errors (such as ejecting CD while attempting to read the CD)
    	"The integrity of the game has been compromised.",
        "ERROR: Ejected CD-ROM",
    
    	//When firing heavier weapons in close quarters, you may not have enough room to do so.
    	"There is no room to fire from here.",
    	
    	//Can't change stance due to objects in the way...
    	"Cannot change stance at this time.",
    
    	//Simple text indications that appear in the game, when the merc can do one of these things.
    	"Drop",
        "Throw",
        "Pass",

        "%s passed to %s.", //"Item" passed to "merc".  Please try to keep the item %s before the merc %s, otherwise,
    											 //must notify SirTech.
    	"No room to pass %s to %s.", //pass "item" to "merc".  Same instructions as above.
    
    	//A list of attachments appear after the items.  Ex:  Kevlar vest ( Ceramic Plate 'Attached )'
    	" Attached )",
    
    	//Cheat modes
    	"Cheat level ONE reached",
        "Cheat level TWO reached",
    
    	//Toggling various stealth modes
    	"Squad on stealth mode.",
        "Squad off stealth mode.",
        "%s on stealth mode.",
        "%s off stealth mode.",
    
    	//Wireframes are shown through buildings to reveal doors and windows that can't otherwise be seen in 
    	//an isometric engine.  You can toggle this mode freely in the game.
    	"Extra Wireframes On",
        "Extra Wireframes Off",
    
    	//These are used in the cheat modes for changing levels in the game.  Going from a basement level to
    	//an upper level, etc.  
    	"Can't go up from this level...",
        "There are no lower levels...",
        "Entering basement level %d...",
        "Leaving basement...",
        "'s",		// used in the shop keeper inteface to mark the ownership of the item eg Red's gun
    	"Follow mode OFF.",
        "Follow mode ON.",
        "3D Cursor OFF.",
        "3D Cursor ON.",
        "Squad %d active.",
        "You cannot afford to pay for %s's daily salary of %s",	//first %s is the mercs name, the seconds is a string containing the salary
    	"Skip",
        "%s cannot leave alone.",
        "A save has been created called, SaveGame99.sav.  If needed, rename it to SaveGame01 - SaveGame10 and then you will have access to it in the Load screen.",
        "%s drank some %s",
        "A package has arrived in Drassen.",
        "%s should arrive at the designated drop-off point (sector %s) on day %d, at approximately %s.",		//first %s is mercs name, next is the sector location and name where they will be arriving in, lastely is the day an the time of arrival
    	"History log updated.",
    };

    //option Text
    public static string[] zOptionsToggleText =
    [
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
    ];

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

    public static Dictionary<Items, string> ShortItemNames = new()
    {
        { Items.NONE, "" },
    };


    public static string[] pTraverseStrings =
    {
        "Previous",
        "Next",
    };

    // Text for Bobby Ray's Home Page

    public static string[] BobbyRaysFrontText =
    {
    	//Details on the web site
    
    	"This is the place to be for the newest and hottest in weaponry and military supplies",
        "We can find the perfect solution for all your explosives needs",
        "Used and refitted items",
    
    	//Text for the various links to the sub pages
    
    	"Miscellaneous",
        "GUNS",
        "AMMUNITION",		//5
    	"ARMOR",
    
    	//Details on the web site
    
    	"If we don't sell it, you can't get it!",
        "Under Construction",
    };

    // new mail notify string 

    public static string[] pNewMailStrings =
    {
        "You have new mail...",
    };


    public static string[] pUpdatePanelButtons = new[]
    {
        "Continue",
        "Stop",
    };

    // various history events
    // THESE STRINGS ARE "HISTORY LOG" STRINGS AND THEIR LENGTH IS VERY LIMITED.
    // PLEASE BE MINDFUL OF THE LENGTH OF THESE STRINGS. ONE WAY TO "TEST" THIS
    // IS TO TURN "CHEAT MODE" ON AND USE CONTROL-R IN THE TACTICAL SCREEN, THEN
    // GO INTO THE LAPTOP/HISTORY LOG AND CHECK OUT THE STRINGS. CONTROL-R INSERTS
    // MANY (NOT ALL) OF THE STRINGS IN THE FOLLOWING LIST INTO THE GAME.
    public static Dictionary<HISTORY, string> pHistoryStrings = new()
    {
        { HISTORY.ENTERED_HISTORY_MODE,"" },																						// leave this line blank
    	{ HISTORY.HIRED_MERC_FROM_AIM,"%s was hired from A.I.M." }, 										// merc was hired from the aim site
    	{ HISTORY.HIRED_MERC_FROM_MERC,"%s was hired from M.E.R.C." }, 									// merc was hired from the aim site
    	{ HISTORY.MERC_KILLED,"%s died." }, 															// merc was killed
    	{ HISTORY.SETTLED_ACCOUNTS_AT_MERC,"Settled Accounts at M.E.R.C." },								// paid outstanding bills at MERC
    	{ HISTORY.ACCEPTED_ASSIGNMENT_FROM_ENRICO,"Accepted Assignment From Enrico Chivaldori" },
        { HISTORY.CHARACTER_GENERATED,"IMP Profile Generated" },
        { HISTORY.PURCHASED_INSURANCE,"Purchased Insurance Contract for %s." }, 				// insurance contract purchased
    	{ HISTORY.CANCELLED_INSURANCE,"Canceled Insurance Contract for %s." }, 				// insurance contract canceled
    	{ HISTORY.INSURANCE_CLAIM_PAYOUT,"Insurance Claim Payout for %s." }, 							// insurance claim payout for merc
    	{ HISTORY.EXTENDED_CONTRACT_1_DAY,"Extended %s's contract by a day." }, 						// Extented "mercs name"'s for a day
    	{ HISTORY.EXTENDED_CONTRACT_1_WEEK,"Extended %s's contract by 1 week." }, 					// Extented "mercs name"'s for a week
    	{ HISTORY.EXTENDED_CONTRACT_2_WEEK,"Extended %s's contract by 2 weeks." }, 					// Extented "mercs name"'s 2 weeks
    	{ HISTORY.MERC_FIRED,"%s was dismissed." }, 													// "merc's name" was dismissed.
    	{ HISTORY.MERC_QUIT,"%s quit." }, 																		// "merc's name" quit.
    	{ HISTORY.QUEST_STARTED,"quest started." }, 															// a particular quest started
    	{ HISTORY.QUEST_FINISHED,"quest completed." },
        { HISTORY.TALKED_TO_MINER,"Talked to head miner of %s" },									// talked to head miner of town
    	{ HISTORY.LIBERATED_TOWN,"Liberated %s" },
        { HISTORY.CHEAT_ENABLED,"Cheat Used" },
        { HISTORY.TALKED_TO_FATHER_WALKER,"Food should be in Omerta by tomorrow" },
        { HISTORY.MERC_MARRIED_OFF,"%s left team to become Daryl Hick's wife" },
        { HISTORY.MERC_CONTRACT_EXPIRED,"%s's contract expired." },
        { HISTORY.RPC_JOINED_TEAM,"%s was recruited." },
        { HISTORY.ENRICO_COMPLAINED,"Enrico complained about lack of progress" },
        { HISTORY.WONBATTLE,"Battle won" },
        { HISTORY.MINE_RUNNING_OUT,"%s mine started running out of ore" },
        { HISTORY.MINE_RAN_OUT,"%s mine ran out of ore" },
        { HISTORY.MINE_SHUTDOWN,"%s mine was shut down" },
        { HISTORY.MINE_REOPENED,"%s mine was reopened" },
        { HISTORY.DISCOVERED_TIXA,"Found out about a prison called Tixa." },
        { HISTORY.DISCOVERED_ORTA,"Heard about a secret weapons plant called Orta." },
        { HISTORY.GOT_ROCKET_RIFLES,"Scientist in Orta donated a slew of rocket rifles." },
        { HISTORY.DEIDRANNA_DEAD_BODIES,"Queen Deidranna has a use for dead bodies." },
        { HISTORY.BOXING_MATCHES,"Frank talked about fighting matches in San Mona." },
        { HISTORY.SOMETHING_IN_MINES,"A patient thinks he saw something in the mines." },
        { HISTORY.DEVIN,"Met someone named Devin - he sells explosives." },
        { HISTORY.MIKE,"Ran into the famous ex-AIM merc Mike!" },
        { HISTORY.TONY,"Met Tony - he deals in arms." },
        { HISTORY.KROTT,"Got a rocket rifle from Sergeant Krott." },
        { HISTORY.KYLE,"Gave Kyle the deed to Angel's leather shop." },
        { HISTORY.MADLAB,"Madlab offered to build a robot." },
        { HISTORY.GABBY,"Gabby can make stealth concoction for bugs." },
        { HISTORY.KEITH_OUT_OF_BUSINESS,"Keith is out of business." },
        { HISTORY.HOWARD_CYANIDE,"Howard provided cyanide to Queen Deidranna." },
        { HISTORY.KEITH,"Met Keith - all purpose dealer in Cambria." },
        { HISTORY.HOWARD,"Met Howard - deals pharmaceuticals in Balime" },
        { HISTORY.PERKO,"Met Perko - runs a small repair business." },
        { HISTORY.SAM,"Met Sam of Balime - runs a hardware shop." },
        { HISTORY.FRANZ,"Franz deals in electronics and other goods." },
        { HISTORY.ARNOLD,"Arnold runs a repair shop in Grumm." },
        { HISTORY.FREDO,"Fredo repairs electronics in Grumm." },
        { HISTORY.RICHGUY_BALIME,"Received donation from rich guy in Balime." },
        { HISTORY.JAKE,"Met a junkyard dealer named Jake." },
        { HISTORY.BUM_KEYCARD,"Some bum gave us an electronic keycard." },
        { HISTORY.WALTER,"Bribed Walter to unlock the door to the basement." },
        { HISTORY.DAVE,"If Dave has gas, he'll provide free fillups." },
        { HISTORY.PABLO,"Greased Pablo's palms." },
        { HISTORY.KINGPIN_MONEY,"Kingpin keeps money in San Mona mine." },
        { HISTORY.WON_BOXING,"%s won Extreme Fighting match" },
        { HISTORY.LOST_BOXING,"%s lost Extreme Fighting match" },
        { HISTORY.DISQUALIFIED_BOXING,"%s was disqualified in Extreme Fighting" },
        { HISTORY.FOUND_MONEY,"Found a lot of money stashed in the abandoned mine." },
        { HISTORY.ASSASSIN,"Encountered assassin sent by Kingpin." },
        { HISTORY.LOSTTOWNSECTOR,"Lost control of sector" },				//ENEMY_INVASION_CODE
    	{ HISTORY.DEFENDEDTOWNSECTOR,"Defended sector" },
        { HISTORY.LOSTBATTLE,"Lost battle" },							//ENEMY_ENCOUNTER_CODE
    	{ HISTORY.FATALAMBUSH,"Fatal ambush" },						//ENEMY_AMBUSH_CODE
    	{ HISTORY.WIPEDOUTENEMYAMBUSH,"Wiped out enemy ambush" },
        { HISTORY.UNSUCCESSFULATTACK,"Unsuccessful attack" },			//ENTERING_ENEMY_SECTOR_CODE
    	{ HISTORY.SUCCESSFULATTACK,"Successful attack!" },
        { HISTORY.CREATURESATTACKED,"Creatures attacked" },			//CREATURE_ATTACK_CODE
    	{ HISTORY.KILLEDBYBLOODCATS,"Killed by bloodcats" },			//BLOODCAT_AMBUSH_CODE
    	{ HISTORY.SLAUGHTEREDBLOODCATS,"Slaughtered bloodcats" },
        { HISTORY.NPC_KILLED,"%s was killed" },
        { HISTORY.GAVE_CARMEN_HEAD,"Gave Carmen a terrorist's head" },
        { HISTORY.SLAY_MYSTERIOUSLY_LEFT,"Slay left" },
        { HISTORY.MERC_KILLED_CHARACTER,"Killed %s" },
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
    	"In order to use the editor, please select a campaign other than the default.",		//@@new
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
        //is used for the final version.  Please don't modify the "#if JA2BETAVERSION" or the "#else" or the "#endif" as they are
        //used by the compiler and will cause program errors if modified/removed.  It's okay to translate the strings though.
        "Attempting to load an older version save.  Automatically update and load the save?",

        //Translators, the next two strings are for the same thing.  The first one is for beta version releases and the second one
        //is used for the final version.  Please don't modify the "#if JA2BETAVERSION" or the "#else" or the "#endif" as they are
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


    // Text having to do with the History Log

    public static string[] pHistoryTitle =
    {
        "History Log",
    };

    public static string[] pHistoryHeaders =
    {
        "Day", 			// the day the history event occurred
        "Page", 			// the current page in the history report we are in
        "Day", 			// the days the history report occurs over
        "Location", 			// location (in sector) the event occurred
        "Event", 			// the event label
    };

    // the names of the towns in the game

    public static Dictionary<TOWNS, string> pTownNames = new()
    {
        { TOWNS.BLANK_SECTOR , "" },
        { TOWNS.OMERTA , "Omerta" },
        { TOWNS.DRASSEN , "Drassen" },
        { TOWNS.ALMA , "Alma" },
        { TOWNS.GRUMM , "Grumm" },
        { TOWNS.TIXA , "Tixa" },
        { TOWNS.CAMBRIA , "Cambria" },
        { TOWNS.SAN_MONA , "San Mona" },
        { TOWNS.ESTONI , "Estoni" },
        { TOWNS.ORTA , "Orta" },
        { TOWNS.BALIME , "Balime" },
        { TOWNS.MEDUNA , "Meduna" },
        { TOWNS.CHITZENA , "Chitzena" },
    };

    // the financial screen strings
    public static string[] pFinanceTitle =
    {
        "Bookkeeper Plus",		//the name we made up for the financial program in the game
    };


    public static string[] pFinanceSummary =
    {
        "Credit:", 				// credit (subtract from) to player's account
    	"Debit:", 				// debit (add to) to player's account
    	"Yesterday's Actual Income:",
        "Yesterday's Other Deposits:",
        "Yesterday's Debits:",
        "Balance At Day's End:",
        "Today's Actual Income:",
        "Today's Other Deposits:",
        "Today's Debits:",
        "Current Balance:",
        "Forecasted Income:",
        "Projected Balance:", 		// projected balance for player for tommorow
    };


    public static string[] pTransactionAlternateText =
    {
        "Insurance for", 				// insurance for a merc
    	"Ext. %s's contract by one day.", 				// entend mercs contract by a day
    	"Ext. %s contract by 1 week.",
        "Ext. %s contract by 2 weeks.",
    };


    public static Dictionary<FinanceEvent, string> pTransactionText = new()
    {
        { FinanceEvent.ACCRUED_INTEREST , "Accrued Interest" },			// interest the player has accumulated so far
    	{ FinanceEvent.ANONYMOUS_DEPOSIT , "Anonymous Deposit" },
        { FinanceEvent.TRANSACTION_FEE , "Transaction Fee" },
        { FinanceEvent.HIRED_MERC , "Hired" }, 				// Merc was hired
    	{ FinanceEvent.BOBBYR_PURCHASE , "Bobby Ray Purchase" }, 		// Bobby Ray is the name of an arms dealer
    	{ FinanceEvent.PAY_SPECK_FOR_MERC, "Settled Accounts at M.E.R.C." },
        { FinanceEvent.MEDICAL_DEPOSIT , "Medical Deposit for %s" }, 		// medical deposit for merc
    	{ FinanceEvent.IMP_PROFILE , "IMP Profile Analysis" }, 		// IMP is the acronym for International Mercenary Profiling
    	{ FinanceEvent.PURCHASED_INSURANCE , "Purchased Insurance for %s" },
        { FinanceEvent.REDUCED_INSURANCE , "Reduced Insurance for %s" },
        { FinanceEvent.EXTENDED_INSURANCE , "Extended Insurance for %s" }, 				// johnny contract extended
    	{ FinanceEvent.CANCELLED_INSURANCE , "Canceled Insurance for %s" },
        { FinanceEvent.INSURANCE_PAYOUT , "Insurance Claim for %s" }, 		// insurance claim for merc
    	{ FinanceEvent.EXTENDED_CONTRACT_BY_1_DAY , "a day" }, 				// merc's contract extended for a day
    	{ FinanceEvent.EXTENDED_CONTRACT_BY_1_WEEK , "1 week" }, 				// merc's contract extended for a week
    	{ FinanceEvent.EXTENDED_CONTRACT_BY_2_WEEKS , "2 weeks" }, 				// ... for 2 weeks
    	{ FinanceEvent.DEPOSIT_FROM_GOLD_MINE , "Mine income" },
        { FinanceEvent.DEPOSIT_FROM_SILVER_MINE , "" }, //String nuked
    	{ FinanceEvent.PURCHASED_FLOWERS , "Purchased Flowers" },
        { FinanceEvent.FULL_MEDICAL_REFUND , "Full Medical Refund for %s" },
        { FinanceEvent.PARTIAL_MEDICAL_REFUND , "Partial Medical Refund for %s" },
        { FinanceEvent.NO_MEDICAL_REFUND , "No Medical Refund for %s" },
        { FinanceEvent.PAYMENT_TO_NPC , "Payment to %s" },		// %s is the name of the npc being paid
    	{ FinanceEvent.TRANSFER_FUNDS_TO_MERC , "Transfer Funds to %s" }, 			// transfer funds to a merc
    	{ FinanceEvent.TRANSFER_FUNDS_FROM_MERC , "Transfer Funds from %s" }, 		// transfer funds from a merc
    	{ FinanceEvent.TRAIN_TOWN_MILITIA , "Equip militia in %s" }, // initial cost to equip a town's militia
    	{ FinanceEvent.PURCHASED_ITEM_FROM_DEALER , "Purchased items from %s." },	//is used for the Shop keeper interface.  The dealers name will be appended to the end of the string.
    	{ FinanceEvent.MERC_DEPOSITED_MONEY_TO_PLAYER_ACCOUNT , "%s deposited money." },
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

    INTERFACE = 0,
    DIALOG = 1,
    CHAT = 2,
    DEBUG = 3,
    UI_FEEDBACK = 4,
    ERROR = 5,
    BETAVERSION = 6,
    TESTVERSION = 7,
    MAP_UI_POSITION_MIDDLE = 8,
    MAP_UI_POSITION_UPPER = 9,
    MAP_UI_POSITION_LOWER = 10,
    SKULL_UI_FEEDBACK = 11,

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
