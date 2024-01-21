using System.Diagnostics;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems.LaptopSubSystem;
using SharpAlliance.Core.SubSystems.LaptopSubSystem.BobbyRSubSystem;

namespace SharpAlliance.Core.SubSystems;

public class GameInit
{
    private static BobbyR bobby;
    private readonly IClockManager clock;
    private readonly StrategicMap strategicMap;
    private readonly ISoundManager sound;
    private readonly TacticalSaveSubSystem tacticalSave;
    private readonly SoldierCreate soldierCreate;
    private readonly Overhead overhead;
    private static Finances finances;
    private static IScreenManager screens;
    private static Messages messages;
    private readonly Emails emails;
    private static SoldierProfileSubSystem soldierProfile;
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
        screens = screenManager;
        GameInit.messages = messages;
        bobby = bobbyR;
        this.clock = clock;
        this.strategicMap = strategicMap;
        GameInit.finances = finances;
        this.sound = soundManager;
        this.tacticalSave = tacticalSaveSubSystem;
        this.soldierCreate = soldierCreate;
        this.overhead = overhead;
        this.emails = emails;
        GameInit.soldierProfile = soldierProfile;
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
        InitNewGame(fReset: true);

        
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
        Laptop.InitLaptopAndLaptopScreens();
        Laptop.LaptopScreenInit();

        //Reload the Merc profiles
        soldierProfile.LoadMercProfiles();

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

    public static async ValueTask<bool> InitNewGame(bool fReset)
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
            if (!soldierProfile.LoadMercProfiles())
            {
                return false;
            }
        }

        //Initialize the Arms Dealers and Bobby Rays inventory
        if (gubScreenCount == 0)
        {
            //Init all the arms dealers inventory
            ArmsDealerInit.InitAllArmsDealers();
            bobby.InitBobbyRayInventory();
        }

        // clear tactical 
        messages.ClearTacticalMessageQueue();

        // clear mapscreen messages
        messages.FreeGlobalMessageList();

        // IF our first time, go into laptop!
        if (gubScreenCount == 0)
        {
            //Init the laptop here
            Laptop.InitLaptopAndLaptopScreens();

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
            finances.AddTransactionToPlayersBook(FinanceEvent.ANONYMOUS_DEPOSIT, 0, GameClock.GetWorldTotalMin(), iStartingCash);


            {
                uint uiDaysTimeMercSiteAvailable = (uint)Globals.Random.GetRandom(2) + 1;

                // schedule email for message from spec at 7am 3 days in the future
                GameEvents.AddFutureDayStrategicEvent(EVENT.DAY3_ADD_EMAIL_FROM_SPECK, 60 * 7, 0, uiDaysTimeMercSiteAvailable);
            }

            Laptop.SetLaptopExitScreen(ScreenName.InitScreen);
            await screens.SetPendingNewScreen(ScreenName.LAPTOP_SCREEN);
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

    private static void InitStrategicLayer()
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

    private static void InitNPCs()
    {
        MERCPROFILESTRUCT? pProfile;

        // add the pilot at a random location!
        pProfile = (gMercProfiles[NPCID.SKYRIDER]);
        switch (Globals.Random.GetRandom(4))
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

        // use alternate map, with Skyrider's shack, in this sector
        SectorInfo[SECTORINFO.SECTOR(pProfile.sSectorX, pProfile.sSectorY)].uiFlags |= SF.USE_ALTERNATE_MAP;


        // set up Madlab's secret lab (he'll be added when the meanwhile scene occurs)

        switch (Globals.Random.GetRandom(4))
        {
            case 0:
                // use alternate map in this sector
                SectorInfo[SECTORINFO.SECTOR(7, MAP_ROW.H)].uiFlags |= SF.USE_ALTERNATE_MAP;
                break;
            case 1:
                SectorInfo[SECTORINFO.SECTOR(16, MAP_ROW.H)].uiFlags |= SF.USE_ALTERNATE_MAP;
                break;
            case 2:
                SectorInfo[SECTORINFO.SECTOR(11, MAP_ROW.I)].uiFlags |= SF.USE_ALTERNATE_MAP;
                break;
            case 3:
                SectorInfo[SECTORINFO.SECTOR(4, MAP_ROW.E)].uiFlags |= SF.USE_ALTERNATE_MAP;
                break;
        }

        // add Micky in random location

        pProfile = (gMercProfiles[NPCID.MICKY]);
        switch (Globals.Random.GetRandom(5))
        {
            case 0:
                pProfile.sSectorX = 9;
                pProfile.sSectorY = MAP_ROW.G;
                pProfile.bSectorZ = 0;
                break;
            case 1:
                pProfile.sSectorX = 13;
                pProfile.sSectorY = MAP_ROW.D;
                pProfile.bSectorZ = 0;
                break;
            case 2:
                pProfile.sSectorX = 5;
                pProfile.sSectorY = MAP_ROW.C;
                pProfile.bSectorZ = 0;
                break;
            case 3:
                pProfile.sSectorX = 2;
                pProfile.sSectorY = MAP_ROW.H;
                pProfile.bSectorZ = 0;
                break;
            case 4:
                pProfile.sSectorX = 6;
                pProfile.sSectorY = MAP_ROW.C;
                pProfile.bSectorZ = 0;
                break;
        }

        // use alternate map in this sector
        //SectorInfo[ SECTOR( pProfile.sSectorX, pProfile.sSectorY ) ].uiFlags |= SF_USE_ALTERNATE_MAP;

        gfPlayerTeamSawJoey = false;


        if (gGameOptions.fSciFi)
        {
            // add Bob
            pProfile = (gMercProfiles[NPCID.BOB]);
            pProfile.sSectorX = 8;
            pProfile.sSectorY = MAP_ROW.F;
            pProfile.bSectorZ = 0;

            // add Gabby in random location
            pProfile = (gMercProfiles[NPCID.GABBY]);
            switch (Globals.Random.GetRandom(2))
            {
                case 0:
                    pProfile.sSectorX = 11;
                    pProfile.sSectorY = MAP_ROW.H;
                    pProfile.bSectorZ = 0;
                    break;
                case 1:
                    pProfile.sSectorX = 4;
                    pProfile.sSectorY = MAP_ROW.I;
                    pProfile.bSectorZ = 0;
                    break;
            }

            // use alternate map in this sector
            SectorInfo[SECTORINFO.SECTOR(pProfile.sSectorX, pProfile.sSectorY)].uiFlags |= SF.USE_ALTERNATE_MAP;
        }
        else
        { //not scifi, so use alternate map in Tixa's b1 level that doesn't have the stairs going down to the caves.
            UNDERGROUND_SECTORINFO? pSector;
            pSector = QueenCommand.FindUnderGroundSector(9, MAP_ROW.J, 1); //j9_b1
            if (pSector is not null)
            {
                pSector.uiFlags |= SF.USE_ALTERNATE_MAP;
            }
        }

        // init hospital variables
        giHospitalTempBalance = 0;
        giHospitalRefund = 0;
        gbHospitalPriceModifier = 0;

        // set up Devin so he will be placed ASAP
        gMercProfiles[NPCID.DEVIN].bNPCData = 3;
    }

    private static void InitBloodCatSectors()
    {
        //Hard coded table of bloodcat populations.  We don't have
        //access to the real population (if different) until we physically 
        //load the map.  If the real population is different, then an error
        //will be reported.
        for (SEC i = 0; i < (SEC)255; i++)
        {
            SectorInfo[i].bBloodCats = -1;
        }

        SectorInfo[SEC.A15].bBloodCatPlacements = 9;
        SectorInfo[SEC.B4].bBloodCatPlacements = 9;
        SectorInfo[SEC.B16].bBloodCatPlacements = 8;
        SectorInfo[SEC.C3].bBloodCatPlacements = 12;
        SectorInfo[SEC.C8].bBloodCatPlacements = 13;
        SectorInfo[SEC.C11].bBloodCatPlacements = 7;
        SectorInfo[SEC.D4].bBloodCatPlacements = 8;
        SectorInfo[SEC.D9].bBloodCatPlacements = 12;
        SectorInfo[SEC.E11].bBloodCatPlacements = 10;
        SectorInfo[SEC.E13].bBloodCatPlacements = 14;
        SectorInfo[SEC.F3].bBloodCatPlacements = 13;
        SectorInfo[SEC.F5].bBloodCatPlacements = 7;
        SectorInfo[SEC.F7].bBloodCatPlacements = 12;
        SectorInfo[SEC.F12].bBloodCatPlacements = 9;
        SectorInfo[SEC.F14].bBloodCatPlacements = 14;
        SectorInfo[SEC.F15].bBloodCatPlacements = 8;
        SectorInfo[SEC.G6].bBloodCatPlacements = 7;
        SectorInfo[SEC.G10].bBloodCatPlacements = 12;
        SectorInfo[SEC.G12].bBloodCatPlacements = 11;
        SectorInfo[SEC.H5].bBloodCatPlacements = 9;
        SectorInfo[SEC.I4].bBloodCatPlacements = 8;
        SectorInfo[SEC.I15].bBloodCatPlacements = 8;
        SectorInfo[SEC.J6].bBloodCatPlacements = 11;
        SectorInfo[SEC.K3].bBloodCatPlacements = 12;
        SectorInfo[SEC.K6].bBloodCatPlacements = 14;
        SectorInfo[SEC.K10].bBloodCatPlacements = 12;
        SectorInfo[SEC.K14].bBloodCatPlacements = 14;

        switch (gGameOptions.ubDifficultyLevel)
        {
            case DifficultyLevel.Easy: //50%
                SectorInfo[SEC.I16].bBloodCatPlacements = 14;
                SectorInfo[SEC.I16].bBloodCats = 14;
                SectorInfo[SEC.N5].bBloodCatPlacements = 8;
                SectorInfo[SEC.N5].bBloodCats = 8;
                break;
            case DifficultyLevel.Medium: //75%
                SectorInfo[SEC.I16].bBloodCatPlacements = 19;
                SectorInfo[SEC.I16].bBloodCats = 19;
                SectorInfo[SEC.N5].bBloodCatPlacements = 10;
                SectorInfo[SEC.N5].bBloodCats = 10;
                break;
            case DifficultyLevel.Hard: //100%
                SectorInfo[SEC.I16].bBloodCatPlacements = 26;
                SectorInfo[SEC.I16].bBloodCats = 26;
                SectorInfo[SEC.N5].bBloodCatPlacements = 12;
                SectorInfo[SEC.N5].bBloodCats = 12;
                break;
        }
    }

    internal static bool AnyMercsHired()
    {
//        INT32 cnt;
//        SOLDIERTYPE? pTeamSoldier;
//        INT16 bLastTeamID;

        // Find first guy availible in team
//        cnt = gTacticalStatus.Team[gbPlayerNum].bFirstID;

//        bLastTeamID = gTacticalStatus.Team[gbPlayerNum].bLastID;

        // look for all mercs on the same team, 
        foreach (var pTeamSoldier in MercPtrs)//; cnt <= bLastTeamID; cnt++, pTeamSoldier++)
        {
            if (pTeamSoldier.bActive)
            {
                return (true);
            }
        }

        return (false);
    }
}
