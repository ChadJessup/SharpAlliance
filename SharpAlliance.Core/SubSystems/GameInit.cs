using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.SubSystems
{
    public class GameInit
    {
        private readonly IClockManager clock;
        private readonly StrategicMap strategicMap;
        private readonly ISoundManager sound;
        private readonly TacticalSaveSubSystem tacticalSave;
        private readonly SoldierCreate soldierCreate;
        private readonly Overhead overhead;
        private readonly Emails emails;
        private readonly Laptop laptop;
        private readonly SoldierProfileSubSystem soldierProfile;
        private readonly Interface ui;
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
            Interface ui,
            TurnBasedInput tbi,
            Cheats cheats,
            GameEvents gameEvents,
            NPC npc,
            ShopKeeper shopKeeper,
            World world,
            HelpScreenSubSystem helpScreen,
            DialogControl dialogs,
            AirRaid airRaid)
        {
            this.clock = clock;
            this.strategicMap = strategicMap;
            this.sound = soundManager;
            this.tacticalSave = tacticalSaveSubSystem;
            this.soldierCreate = soldierCreate;
            this.overhead = overhead;
            this.emails = emails;
            this.laptop = laptop;
            this.soldierProfile = soldierProfile;
            this.ui = ui;
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
            this.clock.gfGamePaused = true;

            //Reset the sectors
            this.strategicMap.gWorldSectorX = this.strategicMap.gWorldSectorY = 0;
            this.strategicMap.gbWorldSectorZ = -1;

            this.sound.SoundStopAll();

            //we are going to restart a game so initialize the variable so we can initialize a new game
            this.InitNewGame(fReset: true);

            
            //Deletes all the Temp files in the Maps\Temp directory
            this.tacticalSave.InitTacticalSave(fCreateTempDir: true);

            //Loop through all the soldier and delete them all
            for (cnt = 0; cnt < OverheadTypes.TOTAL_SOLDIERS; cnt++)
            {
                this.soldierCreate.TacticalRemoveSoldier(usSoldierIndex: cnt);
            }

            // Re-init overhead...
            this.overhead.InitOverhead();

            //Reset the email list
            this.emails.ShutDownEmailList();

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
            this.ui.gsCurInterfacePanel = InterfacePanelDefines.TEAM_PANEL;

            //Delete all the strategic events
            this.gameEvents.DeleteAllStrategicEvents();

            //This function gets called when ur in a game a click the quit to main menu button, therefore no game is in progress
            this.overhead.gTacticalStatus.fHasAGameBeenStarted = false;

            // Reset timer callbacks
            // gpCustomizableTimerCallback = null;

            this.turnBasedInput.gubCheatLevel = Cheats.STARTING_CHEAT_LEVEL;
        }

        private bool InitNewGame(bool fReset)
        {
            return true;
        }
    }
}
