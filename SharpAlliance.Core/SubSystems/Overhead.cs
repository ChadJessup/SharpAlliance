using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems
{
    public class Overhead
    {
        public void InitOverhead()
        {
        }

        public ValueTask<bool> InitTacticalEngine()
        {
            return ValueTask.FromResult(true);
        }

        public static bool InOverheadMap()
        {
            return false;
        }

        public static bool FlatRoofAboveGridNo(int iMapIndex)
        {
            LEVELNODE? pRoof;
            TileTypeDefines uiTileType;
            pRoof = gpWorldLevelData[iMapIndex].pRoofHead;
            while (pRoof is not null)
            {
                if (pRoof.usIndex != TileDefines.NO_TILE)
                {
                    TileDefine.GetTileType(pRoof.usIndex, out uiTileType);
                    if (uiTileType >= TileTypeDefines.FIRSTROOF && uiTileType <= LASTROOF)
                    {
                        return true;
                    }
                }
                pRoof = pRoof.pNext;
            }
         
            return false;
        }

        public static void CencelAllActionsForTimeCompression()
        {
            int cnt;

            foreach(var pSoldier in Menptr)
            {
                if (pSoldier.bActive)
                {
                    if (pSoldier.bInSector)
                    {
                        // Hault!
                        SoldierControl.EVENT_StopMerc(pSoldier, pSoldier.sGridNo, pSoldier.bDirection);

                        // END AI actions
                        AIMain.CancelAIAction(pSoldier, 1);
                    }
                }
            }
        }

        public static void LocateSoldier(int usID, int fSetLocator)
        {
            SOLDIERTYPE? pSoldier;
            int sNewCenterWorldX, sNewCenterWorldY;

            //if (!bCenter && SoldierOnScreen(usID))
            //return;

            // do we need to move the screen?
            //ATE: Force this baby to locate if told to
            if (!SoldierFind.SoldierOnScreen(usID) || fSetLocator == 10)
            {
                // Get pointer of soldier
                pSoldier = MercPtrs[usID];

                // Center on guy
                sNewCenterWorldX = (int)pSoldier.dXPos;
                sNewCenterWorldY = (int)pSoldier.dYPos;

                RenderWorld.SetRenderCenter(sNewCenterWorldX, sNewCenterWorldY);

                // Plot new path!
                gfPlotNewMovement = true;
            }


            // do we flash the name & health bars/health string above?
            if (fSetLocator > 0)
            {
                if (fSetLocator == SETLOCATOR || fSetLocator == 10)
                {
                    InterfacePanel.ShowRadioLocator(usID, SHOW_LOCATOR.NORMAL);
                }
                else
                {
                    InterfacePanel.ShowRadioLocator(usID, SHOW_LOCATOR.FAST);
                }
            }
        }

        public static void LocateGridNo(int sGridNo)
        {
            InternalLocateGridNo(sGridNo, false);
        }

        private static void InternalLocateGridNo(int sGridNo, bool fForce)
        {
            int sNewCenterWorldX, sNewCenterWorldY;

            IsometricUtils.ConvertGridNoToCenterCellXY(sGridNo, out sNewCenterWorldX, out sNewCenterWorldY);

            // FIRST CHECK IF WE ARE ON SCREEN
            if (SoldierFind.GridNoOnScreen(sGridNo) && !fForce)
            {
                return;
            }

            RenderWorld.SetRenderCenter(sNewCenterWorldX, sNewCenterWorldY);
        }

        public static bool GetSoldier(out SOLDIERTYPE? ppSoldier, int usSoldierIndex)
        {
            // Check range of index given
            ppSoldier = null;

            if (usSoldierIndex < 0 || usSoldierIndex > Globals.TOTAL_SOLDIERS - 1)
            {
                // Set debug message
                return (false);
            }

            // Check if a guy exists here
            // Does another soldier exist here?
            if (Globals.MercPtrs[usSoldierIndex].bActive)
            {
                // Set Existing guy
                ppSoldier = Globals.MercPtrs[usSoldierIndex];
                return (true);
            }
            else
            {
                return (false);
            }
        }
    }

    // civilian "sub teams":
    public enum CIV_GROUP
    {
        NON_CIV_GROUP = 0,
        REBEL_CIV_GROUP,
        KINGPIN_CIV_GROUP,
        SANMONA_ARMS_GROUP,
        ANGELS_GROUP,
        BEGGARS_CIV_GROUP,
        TOURISTS_CIV_GROUP,
        ALMA_MILITARY_CIV_GROUP,
        DOCTORS_CIV_GROUP,
        COUPLE1_CIV_GROUP,
        HICKS_CIV_GROUP,
        WARDEN_CIV_GROUP,
        JUNKYARD_CIV_GROUP,
        FACTORY_KIDS_GROUP,
        QUEENS_CIV_GROUP,
        UNNAMED_CIV_GROUP_15,
        UNNAMED_CIV_GROUP_16,
        UNNAMED_CIV_GROUP_17,
        UNNAMED_CIV_GROUP_18,
        UNNAMED_CIV_GROUP_19,

        NUM_CIV_GROUPS
    };

    public class TacticalStatusType
    {
        public TEAM ubCurrentTeam { get; set; }

        public Dictionary<TEAM, TacticalTeamType> Team = new();
        public bool fHasAGameBeenStarted { get; set; }
        public int ubAttackBusyCount { get; set; }
        public TacticalEngineStatus uiFlags { get; set; }
        public bool fAtLeastOneGuyOnMultiSelect { get; set; }
        public bool fUnLockUIAfterHiddenInterrupt { get; set; }
        public uint uiTactialTurnLimitClock { get; set; }

        public int sSlideTarget;
        public int sSlideReason;
        public uint uiTimeSinceMercAIStart;
        public PANIC fPanicFlags;
        public int sPanicTriggerGridnoUnused;
        public int sHandGrid;
        public int ubSpottersCalledForBy;
        public int ubTheChosenOne;
        public uint uiTimeOfLastInput;
        public uint uiTimeSinceDemoOn;
        public int uiCountdownToRestart;
        public bool fGoingToEnterDemo;
        public bool fNOTDOLASTDEMO;
        public bool fMultiplayer;
        public Dictionary<CIV_GROUP, int> fCivGroupHostile = new();
        public int ubLastBattleSectorX;
        public int ubLastBattleSectorY;
        public bool fLastBattleWon;
        public int bOriginalSizeOfEnemyForce;
        public int bPanicTriggerIsAlarmUnused;
        public bool fVirginSector;
        public bool fEnemyInSector;
        public bool fInterruptOccurred;
        public int bRealtimeSpeed;
        public int ubEnemyIntention;
        public int ubEnemyIntendedRetreatDirection;
        public int ubEnemySightingOnTheirTurnEnemyID;
        public int ubEnemySightingOnTheirTurnPlayerID;
        public bool fEnemySightingOnTheirTurn;
        public bool fAutoBandageMode;
        public int bNumEnemiesFoughtInBattleUnused;
        public int ubEngagedInConvFromActionMercID;
        public int usTactialTurnLimitCounter;
        public bool fInTopMessage;
        public int ubTopMessageType;
        public int[] zTopMessageString = new int[20];
        public int usTactialTurnLimitMax;
        public bool fTactialTurnLimitStartedBeep;
        public BoxingStates bBoxingState;
        public int bConsNumTurnsNotSeen;
        public int ubArmyGuysKilled;
        public int []sPanicTriggerGridNo = new int[Globals.NUM_PANIC_TRIGGERS];
        public int [] bPanicTriggerIsAlarm = new int[Globals.NUM_PANIC_TRIGGERS];
        public int [] ubPanicTolerance = new int[Globals.NUM_PANIC_TRIGGERS];
        public bool fSaidCreatureFlavourQuote;
        public bool fHaveSeenCreature;
        public bool fKilledEnemyOnAttack;
        public int ubEnemyKilledOnAttack;
        public int bEnemyKilledOnAttackLevel;
        public int ubEnemyKilledOnAttackLocation;
        public bool fItemsSeenOnAttack;
        public bool ubItemsSeenOnAttackSoldier;
        public bool fBeenInCombatOnce;
        public bool fSaidCreatureSmellQuote;
        public int usItemsSeenOnAttackGridNo;
        public bool fLockItemLocators;
        public int ubLastQuoteSaid;
        public int ubLastQuoteProfileNUm;
        public bool fCantGetThrough;
        public int sCantGetThroughGridNo;
        public int sCantGetThroughSoldierGridNo;
        public int ubCantGetThroughID;
        public bool fDidGameJustStart;
        public bool fStatChangeCheatOn;
        public NPCID ubLastRequesterTargetID;
        public bool fGoodToAllowCrows;
        public int ubNumCrowsPossible;
        public int uiTimeCounterForGiveItemSrc;
        public int[] bNumFoughtInBattle = new int[Globals.MAXTEAMS];
        public int uiDecayBloodLastUpdate;
        public int uiTimeSinceLastInTactical;
        public int bConsNumTurnsWeHaventSeenButEnemyDoes;
        public bool fSomeoneHit;
        public int ubPaddingSmall;
        public int uiTimeSinceLastOpplistDecay;
        public int bMercArrivingQuoteBeingUsed;
        public int ubEnemyKilledOnAttackKiller;
        public bool fCountingDownForGuideDescription;
        public int bGuideDescriptionCountDown;
        public int ubGuideDescriptionToUse;
        public int bGuideDescriptionSectorX;
        public int bGuideDescriptionSectorY;
        public int fEnemyFlags;
        public bool fAutoBandagePending;
        public bool fHasEnteredCombatModeSinceEntering;
        public bool fDontAddNewCrows;
        public int ubMorePadding;
        public int sCreatureTenseQuoteDelay;
    }
}

// TACTICAL ENGINE STATUS FLAGS
public class TacticalTeamType
{
    public int RadarColor;
    public int bFirstID;
    public int bLastID;
    public TEAM bSide;
    public int bMenInSector;
    public int ubLastMercToRadio;
    public int bTeamActive;
    public int bAwareOfOpposition;
    public int bHuman;

    public bool IsHuman => this.bHuman > 0;
}

[Flags]
public enum PANIC
{
    BOMBS_HERE = 0x01,
    TRIGGERS_HERE = 0x02,
    NUM_PANIC_TRIGGERS = 3,
}
