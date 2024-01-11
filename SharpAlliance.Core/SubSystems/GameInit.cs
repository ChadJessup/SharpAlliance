using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;

namespace SharpAlliance.Core.SubSystems;

public class GameInit
{
    private readonly BobbyR bobby;
    private readonly IClockManager clock;
    private readonly StrategicMap strategicMap;
    private readonly ISoundManager sound;
    private readonly TacticalSaveSubSystem tacticalSave;
    private readonly SoldierCreate soldierCreate;
    private readonly Overhead overhead;
    private readonly Finances finances;
    private readonly IScreenManager screens;
    private readonly Messages messages;
    private readonly Emails emails;
    private readonly Laptop laptop;
    private readonly SoldierProfileSubSystem soldierProfile;
    private readonly World world;
    private readonly NPC npc;
    private readonly HelpScreenSubSystem helpScreen;
    private readonly ShopKeeper shopKeeper;
    private readonly TurnBasedInput turnBasedInput;
    private readonly Cheats cheats;
    private readonly GameEvents gameEvents;
    private readonly DialogControl dialogs;
    private readonly AirRaid airRaid;

    public GameInit(
        IClockManager clock,
        StrategicMap strategicMap,
        ISoundManager soundManager,
        TacticalSaveSubSystem tacticalSaveSubSystem,
        SoldierCreate soldierCreate,
        Overhead overhead,
        Emails emails,
        Laptop laptop,
        SoldierProfileSubSystem soldierProfile,
        Messages messages,
        TurnBasedInput tbi,
        Finances finances,
        Cheats cheats,
        GameEvents gameEvents,
        NPC npc,
        IScreenManager screenManager,
        ShopKeeper shopKeeper,
        World world,
        HelpScreenSubSystem helpScreen,
        BobbyR bobbyR,
        DialogControl dialogs,
        AirRaid airRaid)
    {
        this.screens = screenManager;
        this.messages = messages;
        this.bobby = bobbyR;
        this.clock = clock;
        this.strategicMap = strategicMap;
        this.finances = finances;
        this.sound = soundManager;
        this.tacticalSave = tacticalSaveSubSystem;
        this.soldierCreate = soldierCreate;
        this.overhead = overhead;
        this.emails = emails;
        this.laptop = laptop;
        this.soldierProfile = soldierProfile;
        this.world = world;
        this.npc = npc;
        this.helpScreen = helpScreen;
        this.shopKeeper = shopKeeper;
        this.turnBasedInput = tbi;
        this.cheats = cheats;
        this.gameEvents = gameEvents;
        this.dialogs = dialogs;
        this.airRaid = airRaid;
    }

    //This function is called when the game is REstarted.  Things that need to be reinited are placed in here
    public void ReStartingGame()
    {
        int cnt;

        //Pause the game
        Globals.gfGamePaused = true;

        //Reset the sectors
        Globals.gWorldSectorY = 0;
        Globals.gWorldSectorX = 0;
        Globals.gbWorldSectorZ = -1;

        this.sound.SoundStopAll();

        //we are going to restart a game so initialize the variable so we can initialize a new game
        this.InitNewGame(fReset: true);

        
        //Deletes all the Temp files in the Maps\Temp directory
        this.tacticalSave.InitTacticalSave(fCreateTempDir: true);

        //Loop through all the soldier and delete them all
        for (cnt = 0; cnt < Globals.TOTAL_SOLDIERS; cnt++)
        {
            this.soldierCreate.TacticalRemoveSoldier(usSoldierIndex: cnt);
        }

        // Re-init overhead...
        Overhead.InitOverhead();

        //Reset the email list
        Emails.ShutDownEmailList();

        //Reinit the laptopn screen variables
        this.laptop.InitLaptopAndLaptopScreens();
        this.laptop.LaptopScreenInit();

        //Reload the Merc profiles
        this.soldierProfile.LoadMercProfiles();

        // Reload quote files
        this.npc.ReloadAllQuoteFiles();

        //Initialize the ShopKeeper Interface ( arms dealer inventory, etc. )
        this.shopKeeper.ShopKeeperScreenInit();

        //Delete the world info
        this.world.TrashWorld();

        //Init the help screen system
        this.helpScreen.InitHelpScreenSystem();

        this.dialogs.EmptyDialogueQueue();

        if (this.airRaid.InAirRaid())
        {
            this.airRaid.EndAirRaid();
        }

        //Reset so we can use the 'cheat key' to start with mercs
        //TempHiringOfMercs(0, true);

        //Make sure the game starts in the TEAM panel ( it wasnt being reset )
        Globals.gsCurInterfacePanel = InterfacePanelDefines.TEAM_PANEL;

        //Delete all the strategic events
        GameEvents.DeleteAllStrategicEvents();

        //This function gets called when ur in a game a click the quit to main menu button, therefore no game is in progress
        Globals.gTacticalStatus.fHasAGameBeenStarted = false;

        // Reset timer callbacks
        // gpCustomizableTimerCallback = null;

        Globals.gubCheatLevel = Cheats.STARTING_CHEAT_LEVEL;
    }

    public async ValueTask<bool> InitNewGame(bool fReset)
    {
        int iStartingCash;

        //	static fScreenCount = 0;

        if (fReset)
        {
            gubScreenCount = 0;
            return true;
        }

        // reset meanwhile flags
        uiMeanWhileFlags = 0;

        // Reset the selected soldier
        gusSelectedSoldier = NOBODY;

        if (gubScreenCount == 0)
        {
            if (!this.soldierProfile.LoadMercProfiles())
            {
                return false;
            }
        }

        //Initialize the Arms Dealers and Bobby Rays inventory
        if (gubScreenCount == 0)
        {
            //Init all the arms dealers inventory
            ArmsDealerInit.InitAllArmsDealers();
            this.bobby.InitBobbyRayInventory();
        }

        // clear tactical 
        this.messages.ClearTacticalMessageQueue();

        // clear mapscreen messages
        this.messages.FreeGlobalMessageList();

        // IF our first time, go into laptop!
        if (gubScreenCount == 0)
        {
            //Init the laptop here
            this.laptop.InitLaptopAndLaptopScreens();

            InitStrategicLayer();

            // Set new game flag
            Laptop.SetLaptopNewGameFlag();

            // this is for the "mercs climbing down from a rope" animation, NOT Skyrider!!
            MercEntering.ResetHeliSeats();

            // Setup two new messages!
            Emails.AddPreReadEmail(OLD_ENRICO_1, OLD_ENRICO_1_LENGTH, EmailAddresses.MAIL_ENRICO, GameClock.GetWorldTotalMin());
            Emails.AddPreReadEmail(OLD_ENRICO_2, OLD_ENRICO_2_LENGTH, EmailAddresses.MAIL_ENRICO, GameClock.GetWorldTotalMin());
            Emails.AddPreReadEmail(RIS_REPORT, RIS_REPORT_LENGTH, EmailAddresses.RIS_EMAIL, GameClock.GetWorldTotalMin());
            Emails.AddPreReadEmail(OLD_ENRICO_3, OLD_ENRICO_3_LENGTH, EmailAddresses.MAIL_ENRICO, GameClock.GetWorldTotalMin());
            Emails.AddEmail(IMP_EMAIL_INTRO, IMP_EMAIL_INTRO_LENGTH, EmailAddresses.CHAR_PROFILE_SITE, GameClock.GetWorldTotalMin());
            //AddEmail(ENRICO_CONGRATS,ENRICO_CONGRATS_LENGTH,MAIL_ENRICO, GetWorldTotalMin() );

            // ATE: Set starting cash....
            switch (gGameOptions.ubDifficultyLevel)
            {
                case DifficultyLevel.Easy:

                    iStartingCash = 45000;
                    break;

                case DifficultyLevel.Medium:

                    iStartingCash = 35000;
                    break;

                case DifficultyLevel.Hard:

                    iStartingCash = 30000;
                    break;

                default:
                    Debug.Assert(false);
                    return false;
            }

            // Setup initial money
            this.finances.AddTransactionToPlayersBook(FinanceEvent.ANONYMOUS_DEPOSIT, 0, GameClock.GetWorldTotalMin(), iStartingCash);


            {
                uint uiDaysTimeMercSiteAvailable = (uint)Globals.Random.GetRandom(2) + 1;

                // schedule email for message from spec at 7am 3 days in the future
                GameEvents.AddFutureDayStrategicEvent(EVENT.DAY3_ADD_EMAIL_FROM_SPECK, 60 * 7, 0, uiDaysTimeMercSiteAvailable);
            }

            Laptop.SetLaptopExitScreen(ScreenName.InitScreen);
            await this.screens.SetPendingNewScreen(ScreenName.LAPTOP_SCREEN);
            gubScreenCount = 1;

            //Set the fact the game is in progress
            gTacticalStatus.fHasAGameBeenStarted = true;

            return true;
        }

        /*
        if( ( guiExitScreen == MAP_SCREEN ) && ( LaptopSaveInfo.gfNewGameLaptop ) )
        {
            SetLaptopExitScreen( GAME_SCREEN );
            return( true );
        }
    */
        if (gubScreenCount == 1)
        {
            // OK , FADE HERE
            //BeginFade( INIT_SCREEN, 35, FADE_OUT_REALFADE, 5 );
            //BeginFade( INIT_SCREEN, 35, FADE_OUT_VERSION_FASTER, 25 );
            //BeginFade( INIT_SCREEN, 35, FADE_OUT_VERSION_SIDE, 0 );


            gubScreenCount = 2;
            return true;
        }

        /*
            if ( gubScreenCount == 2 )
            {

                if ( !SetCurrentWorldSector( 9, 1, 0 ) )
                {

                }

                SetLaptopExitScreen( MAP_SCREEN );

                FadeInGameScreen( );

                EnterTacticalScreen( );

                if( gfAtLeastOneMercWasHired == true )
                {  
                    gubScreenCount = 3;
                }
                else
                {

                }

                return( true );
            }

            */

        return true;
    }

    private void InitStrategicLayer()
    {
        // Clear starategic layer!
        StrategicMap.SetupNewStrategicGame();
        Quests.InitQuestEngine();

        //Setup a new campaign via the enemy perspective.
        CampaignInit.InitNewCampaign();
        // Init Squad Lists
        Squads.InitSquads();
        // Init vehicles
        Vehicles.InitVehicles();
        // init town loyalty
        StrategicTownLoyalty.InitTownLoyalty();
        // init the mine management system
        StrategicMines.InitializeMines();
        // initialize map screen flags
        MapScreenInterfaceBorder.InitMapScreenFlags();
        // initialize NPCs, select alternate maps, etc
        InitNPCs();
        // init Skyrider and his helicopter
        MapScreenHelicopter.InitializeHelicopter();
        //Clear out the vehicle list
        Vehicles.ClearOutVehicleList();

        InitBloodCatSectors();

        StrategicMap.InitializeSAMSites();

        // make Orta, Tixa, SAM sites not found
        MapScreenInterfaceMap.InitMapSecrets();


        // free up any leave list arrays that were left allocated
        MapScreen.ShutDownLeaveList();
        // re-set up leave list arrays for dismissed mercs
        MapScreenInterfaceMap.InitLeaveList();

        // reset time compression mode to X0 (this will also pause it)
        GameClock.SetGameTimeCompressionLevel(TIME_COMPRESS.TIME_COMPRESS_X0);

        // select A9 Omerta as the initial selected sector
        MapScreen.ChangeSelectedMapSector(9, MAP_ROW.A, 0);

        // Reset these flags or mapscreen could be disabled and cause major headache.
        fDisableDueToBattleRoster = false;
        fDisableMapInterfaceDueToBattle = false;
    }

    private void InitBloodCatSectors()
    {
        throw new NotImplementedException();
    }

    private void InitNPCs()
    {
        throw new NotImplementedException();
    }
}
