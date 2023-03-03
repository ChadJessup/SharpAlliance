using System.Collections.Generic;
using SharpAlliance.Core.Managers.Image;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems
{
    public class SOLDIERTYPE
    {
        // ID
        public int ubID;
        int bReserved1;

        // DESCRIPTION / STATS, ETC
        public SoldierBodyTypes ubBodyType;
        public int bActionPoints;
        int bInitialActionPoints;

        public SOLDIER uiStatusFlags { get; set; }

        public Dictionary<InventorySlot, OBJECTTYPE> inv = new();

        OBJECTTYPE pTempObject;
        // KEY_ON_RING pKeyRing;

        public int bOldLife;          // life at end of last turn, recorded for monster AI
                                // attributes
        public bool bInSector { get; set; }
        int bFlashPortraitFrame;
        int sFractLife;       // fraction of life pts (in hundreths)	
        public int bBleeding;     // blood loss control variable
        public int bBreath;           // current breath value
        public int bBreathMax;   // max breath, affected by fatigue/sleep
        public bool bStealthMode { get; set; }

        int sBreathRed;           // current breath value
        public int fDelayedMovement;

        public bool fReloading;
        public int ubWaitActionToDo;
        public bool fPauseAim;
        public int ubInsertionDirection;
        public int bGunType;
        // skills
        public int ubOppNum;
        public int bLastRenderVisibleValue;
        public bool fInMissionExitNode;
        public int ubAttackingHand;
        public int bScientific;
        // traits	
        public int sWeightCarriedAtTurnStart;
        public string name; // max 10 chars?

        public int bVisible;          // to render or not to render...


        public bool bActive;

        public TEAM bTeam;             // Team identifier

        //NEW MOVEMENT INFORMATION for Strategic Movement
        public int ubGroupID;        //the movement group the merc is currently part of.
        public bool fBetweenSectors;    //set when the group isn't actually in a sector.
                                 //sSectorX and sSectorY will reflect the sector the
                                 //merc was at last.

        public int ubMovementNoiseHeard;// 8 flags by direction

        // 23 bytes so far 	

        // WORLD POSITION STUFF
        public float dXPos;
        public float dYPos;
        public float dOldXPos;
        public float dOldYPos;
        int sInitialGridNo;
        public int sGridNo;
        public int bDirection;
        public int sHeightAdjustment;
        public int sDesiredHeight;
        public int sTempNewGridNo;                   // New grid no for advanced animations
        public int sRoomNo;
        public int bOverTerrainType;
        public int bOldOverTerrainType;

        public bool bCollapsed;                    // collapsed due to being out of APs
        int bBreathCollapsed;                  // collapsed due to being out of APs
                                               // 50 bytes so far

        public uint UpdateCounter;
        public uint DamageCounter;
        public uint ReloadCounter;
        public uint FlashSelCounter;
        public uint AICounter;
        public uint FadeCounter;

        int ubDesiredHeight;
        public AnimationStates usPendingAnimation;
        int ubPendingStanceChange;
        public AnimationStates usAnimState;
        public bool fNoAPToFinishMove;
        bool fPausedMove;
        bool fUIdeadMerc;                // UI Flags for removing a newly dead merc
        bool fUInewMerc;                 // UI Flags for adding newly created merc ( panels, etc )
        bool fUICloseMerc;               // UI Flags for closing panels
        bool fUIFirstTimeNOAP;       // UI Flag for diming guys when no APs ( dirty flags )
        bool fUIFirstTimeUNCON;  // UI FLAG For unconscious dirty		



        // TIMECOUNTER UpdateCounter;
        // TIMECOUNTER DamageCounter;
        // TIMECOUNTER ReloadCounter;
        // TIMECOUNTER FlashSelCounter;
        // TIMECOUNTER AICounter;
        // TIMECOUNTER FadeCounter;

        public int ubSkillTrait1;
        public int ubSkillTrait2;
        public int uiAIDelay;
        public int bDexterity;        // dexterity (hand coord) value
        public int bWisdom;
        public int sReloadDelay;
        public int ubAttackerID;
        public int ubPreviousAttackerID;
        public bool fTurnInProgress;

        public bool fIntendedTarget; // intentionally shot?
        public bool fPauseAllAnimation;

        public int bExpLevel;     // general experience level
        public int sInsertionGridNo;

        public bool fContinueMoveAfterStanceChange;

        // 60
        //AnimationSurfaceCacheType AnimCache; // will be 9 bytes once changed to pointers

        public int bLife;             // current life (hit points or health)
        public TEAM bSide;
        public int bViewRange;
        public int bNewOppCnt;
        public int bService;      // first aid, or other time consuming process

        public int usAniCode;
        public int usAniFrame;
        public int sAniDelay;

        // MOVEMENT TO NEXT TILE HANDLING STUFF
        public int bAgility;          // agility (speed) value
        public int ubDelayedMovementCauseMerc;
        public int sDelayedMovementCauseGridNo;
        public int sReservedMovementGridNo;

        public int bStrength;

        // Weapon Stuff
        public bool fHoldAttackerUntilDone;
        public int sTargetGridNo;
        public int bTargetLevel;
        public int bTargetCubeLevel;
        public int sLastTarget;
        public int bTilesMoved;
        public int bLeadership;
        public float dNextBleed;
        public bool fWarnedAboutBleeding;
        public bool fDyingComment;

        public int ubTilesMovedPerRTBreathUpdate;
        public int usLastMovementAnimPerRTBreathUpdate;

        public bool fTurningToShoot;
        public bool fTurningToFall;
        public bool fTurningUntilDone;
        public bool fGettingHit;
        public bool fInNonintAnim;
        public bool fFlashLocator;
        public int sLocatorFrame;
        public bool fShowLocator;
        public bool fFlashPortrait;
        public int bMechanical;
        public int bLifeMax;          // maximum life for this merc

        int iFaceIndex;


        // PALETTE MANAGEMENT STUFF
        PaletteRepID HeadPal;   // 30 
        PaletteRepID PantsPal;  // 30
        PaletteRepID VestPal;   // 30
        PaletteRepID SkinPal;   // 30
        PaletteRepID MiscPal;   // 30

        // FULL 3-d TILE STUFF ( keep records of three tiles infront )
        int[] usFrontArcFullTileList = new int[MAX_FULLTILE_DIRECTIONS];
        int[] usFrontArcFullTileGridNos = new int[MAX_FULLTILE_DIRECTIONS];


        SGPPaletteEntry p8BPPPalette; // 4
        int p16BPPPalette;
        public int[] pShades = new int[Globals.NUM_SOLDIER_SHADES]; // Shading tables
        int[] pGlowShades = new int[20]; // 
        public int pCurrentShade;
        public int bMedical;
        public bool fBeginFade;
        public int ubFadeLevel;
        public int ubServiceCount;
        public int ubServicePartner;
        public int bMarksmanship;
        public int bExplosive;
        // THROW_PARAMS pThrowParams;
        public bool fTurningFromPronePosition;
        public bool bReverse;
        //struct TAG_level_node               *pLevelNode;
        //struct TAG_level_node               *pExternShadowLevelNode;
        //struct TAG_level_node               *pRoofUILevelNode;

        // WALKING STUFF
        public WorldDirections bDesiredDirection;
        public int sDestXPos;
        public int sDestYPos;
        public int sDesiredDest;
        public int sDestination;
        public int sFinalDestination;
        public int bLevel;
        public int bStopped;
        public int bNeedToLook;


        // PATH STUFF
        public int[] usPathingData = new int[Globals.MAX_PATH_LIST_SIZE];
        public int usPathDataSize;
        public int usPathIndex;
        public int sBlackList;
        public int bAimTime;
        public int bShownAimTime;
        public int bPathStored;   // good for AI to reduct redundancy
        public int bHasKeys;          // allows AI controlled dudes to open locked doors


        // UNBLIT BACKGROUND
        int pBackGround;
        int pZBackground;
        int usUnblitX, usUnblitY;
        int usUnblitWidth, usUnblitHeight;

        public INSERTION_CODE ubStrategicInsertionCode;
        public int usStrategicInsertionData;

        public int iLight;
        public int iMuzFlash;
        public int bMuzFlashCount;

        public int sX;
        public int sY;

        public int usOldAniState;
        public int sOldAniCode;

        public int bBulletsLeft;
        public int ubSuppressionPoints;

        // STUFF FOR RANDOM ANIMATIONS
        public int uiTimeOfLastRandomAction;
        public int usLastRandomAnim;


        // AI STUFF
        public int[] bOppList = new int[Globals.MAX_NUM_SOLDIERS]; // AI knowledge database
        public AI_ACTION bLastAction;
        public AI_ACTION bAction;
        public int usActionData;
        public AI_ACTION bNextAction;
        public int usNextActionData;
        public int bActionInProgress;
        public STATUS bAlertStatus;
        public int bOppCnt;
        public int bNeutral;
        public int bNewSituation;
        public int bNextTargetLevel;
        public int bOrders;
        public Attitudes bAttitude;
        public int bUnderFire;
        public int bShock;
        public int bUnderEscort;
        public int bBypassToGreen;
        public int ubLastMercToRadio;
        public int bDominantDir;              // AI main direction to face...
        public int bPatrolCnt;                    // number of patrol gridnos
        public int bNextPatrolPnt;            // index to next patrol gridno
        public int[] usPatrolGrid = new int[MAXPATROLGRIDS];// AI list for ptr.orders==PATROL
        public int sNoiseGridno;
        public int ubNoiseVolume;
        public int bLastAttackHit;
        public int ubXRayedBy;
        public float dHeightAdjustment;
        public int bMorale;
        public int bTeamMoraleMod;
        public int bTacticalMoraleMod;
        public int bStrategicMoraleMod;
        public int bAIMorale;
        public MERC ubPendingAction;
        public int ubPendingActionAnimCount;
        public int uiPendingActionData1;
        public int sPendingActionData2;
        public int bPendingActionData3;
        public int ubDoorHandleCode;
        public int uiPendingActionData4;
        public int bInterruptDuelPts;
        public int bPassedLastInterrupt;
        public int bIntStartAPs;
        public int bMoved;
        public int bHunting;
        public int ubLastCall;
        public int ubCaller;
        public int sCallerGridNo;
        public int bCallPriority;
        public int bCallActedUpon;
        public int bFrenzied;
        public int bNormalSmell;
        public int bMonsterSmell;
        public int bMobility;
        public int bRTPCombat;
        public AIDEFINES fAIFlags;

        public bool fDontChargeReadyAPs;
        public int usAnimSurface;
        public int sZLevel;
        public bool fPrevInWater;
        public bool fGoBackToAimAfterHit;

        public int sWalkToAttackGridNo;
        public int sWalkToAttackWalkToCost;

        public bool fForceRenderColor;
        public bool fForceNoRenderPaletteCycle;
        public int sLocatorOffX;
        public int sLocatorOffY;
        public bool fStopPendingNextTile;
        public bool fForceShade;
        public int pForcedShade;
        public int bDisplayDamageCount;
        public int fDisplayDamage;
        public int sDamage;
        public int sDamageX;
        public int sDamageY;
        public int bDamageDir;
        public bool bDoBurst;
        public AnimationStates usUIMovementMode;
        public int bUIInterfaceLevel;
        public bool fUIMovementFast;

        // TIMECOUNTER BlinkSelCounter;
        // TIMECOUNTER PortraitFlashCounter;
        public bool fDeadSoundPlayed;
        public NPCID ubProfile { get; }
        public int ubQuoteRecord;
        public QUOTE_ACTION_ID ubQuoteActionID;
        public int ubBattleSoundID;
        public bool fClosePanel;
        public bool fClosePanelToDie;
        public int ubClosePanelFrame;
        public bool fDeadPanel;
        public int ubDeadPanelFrame;
        public bool fOpenPanel;
        public int bOpenPanelFrame;
        public int sPanelFaceX;
        public int sPanelFaceY;
        // QUOTE STUFF
        public int bNumHitsThisTurn;
        public SOLDIER_QUOTE usQuoteSaidFlags;
        public int fCloseCall;
        public SKILLCHECKS bLastSkillCheck;
        public int ubSkillCheckAttempts;
        public int bVocalVolume;  // verbal sounds need to differ in volume
        public int bStartFallDir;
        public int fTryingToFall;
        public int ubPendingDirection;
        public int uiAnimSubFlags;
        public int bAimShotLocation;
        public int ubHitLocation;
        public int[] pEffectShades = new int[Globals.NUM_SOLDIER_EFFECTSHADES]; // Shading tables for effects
        public int ubPlannedUIAPCost;
        public int sPlannedTargetX;
        public int sPlannedTargetY;
        public int[] sSpreadLocations = new int[6];
        public bool fDoSpread;
        public int sStartGridNo;
        public int sEndGridNo;
        public int sForcastGridno;
        public int sZLevelOverride;
        public int bMovedPriorToInterrupt;
        public int iEndofContractTime;               // time, in global time(resolution, minutes) that merc will leave, or if its a M.E.R.C. merc it will be set to -1.  -2 for NPC and player generated
        public int iStartContractTime;
        public int iTotalContractLength;         // total time of AIM mercs contract	or the time since last paid for a M.E.R.C. merc
        public int iNextActionSpecialData;       // AI special action data record for the next action
        public MERC_TYPE ubWhatKindOfMercAmI;          //Set to the type of character it is
        public Assignments bAssignment;                           // soldiers current assignment 
        int bOldAssignment;                        // old assignment, for autosleep purposes
        bool fForcedToStayAwake;             // forced by player to stay awake, reset to false, the moment they are set to rest or sleep
        int bTrainStat;                                // current stat soldier is training
        public int sSectorX { get; set; }       // X position on the Stategic Map
        public MAP_ROW sSectorY;                                 // Y position on the Stategic Map
        public int bSectorZ;                                  // Z sector location
        int iVehicleId;                               // the id of the vehicle the char is in
        //PathStPtr pMercPath;                                //Path Structure
        int fHitByGasFlags;                       // flags 
        int usMedicalDeposit;         // is there a medical deposit on merc 
        CIV_GROUP usLifeInsurance;          // is there life insurance taken out on merc  
        //DEF:  Used for the communications
        int uiStartMovementTime;             // the time since the merc first started moving 
        int uiOptimumMovementTime;           // everytime in ececute overhead the time for the current ani will be added to this total
        int usLastUpdateTime;                    // The last time the soldier was in ExecuteOverhead

        bool fIsSoldierMoving;                           // ie.  Record time is on
        bool fIsSoldierDelayed;                      //Is the soldier delayed Soldier 
        bool fSoldierUpdatedFromNetwork;
        int uiSoldierUpdateNumber;
        byte ubSoldierUpdateType;
        //END
        int iStartOfInsuranceContract;
        int uiLastAssignmentChangeMin;       // timestamp of last assignment change in minutes
        int iTotalLengthOfInsuranceContract;
        public SOLDIER_CLASS ubSoldierClass;                                   //admin, elite, troop (creature types?)
        int ubAPsLostToSuppression;
        bool fChangingStanceDueToSuppression;
        int ubSuppressorID;
        //Squad merging vars
        int ubDesiredSquadAssignment;
        int ubNumTraversalsAllowedToMerge;
        public AnimationStates usPendingAnimation2;
        public CIV_GROUP ubCivilianGroup;
        // time changes...when a stat was changed according to GetJA2Clock();
        public int uiChangeLevelTime;
        public int uiChangeHealthTime;
        public int uiChangeStrengthTime;
        public int uiChangeDexterityTime;
        public int uiChangeAgilityTime;
        public int uiChangeWisdomTime;
        public int uiChangeLeadershipTime;
        public int uiChangeMarksmanshipTime;
        public int uiChangeExplosivesTime;
        public int uiChangeMedicalTime;
        public int uiChangeMechanicalTime;
        public int uiUniqueSoldierIdValue; // the unique value every instance of a soldier gets - 1 is the first valid value
        public int bBeingAttackedCount;       // Being attacked counter
        public int[] bNewItemCount = new int[(int)InventorySlot.NUM_INV_SLOTS];
        public int[] bNewItemCycleCount = new int[(int)InventorySlot.NUM_INV_SLOTS];
        public bool fCheckForNewlyAddedItems;
        public int bEndDoorOpenCode;
        public int ubScheduleID;
        public int sEndDoorOpenCodeData;
        //TIMECOUNTER NextTileCounter;
        public bool fBlockedByAnotherMerc;
        public int bBlockedByAnotherMercDirection;
        public Items usAttackingWeapon;
        public WM bWeaponMode;
        public int ubTargetID;
        public int bAIScheduleProgress;
        public int sOffWorldGridNo;
        //TAG_anitile                 pAniTile;	
        public int bCamo;
        public int sAbsoluteFinalDestination;
        public int ubHiResDirection;
        public int ubHiResDesiredDirection;
        public int ubLastFootPrintSound;
        public int bVehicleID;
        public int fPastXDest;
        public int fPastYDest;
        public int bMovementDirection;
        public int sOldGridNo;
        public int usDontUpdateNewGridNoOnMoveAnimChange;
        public int sBoundingBoxWidth;
        public int sBoundingBoxHeight;
        public int sBoundingBoxOffsetX;
        public int sBoundingBoxOffsetY;
        int uiTimeSameBattleSndDone;
        int bOldBattleSnd;
        bool fReactingFromBeingShot;
        public bool fContractPriceHasIncreased;
        int iBurstSoundID;
        bool fFixingSAMSite;
        bool fFixingRobot;
        int bSlotItemTakenFrom;
        bool fSignedAnotherContract;
        int ubAutoBandagingMedic;
        public bool fDontChargeTurningAPs;
        public int ubRobotRemoteHolderID;
        public int uiTimeOfLastContractUpdate;
        public int bTypeOfLastContract;
        public int bTurnsCollapsed;
        public int bSleepDrugCounter;
        public int ubMilitiaKills;
        public int[] bFutureDrugEffect = new int[2];                      // value to represent effect of a needle
        public int[] bDrugEffectRate = new int[2];                            // represents rate of increase and decrease of effect  
        public int[] bDrugEffect = new int[2];                                    // value that affects AP & morale calc ( -ve is poorly )
        public int[] bDrugSideEffectRate = new int[2];                    // duration of negative AP and morale effect
        public int[] bDrugSideEffect = new int[2];                            // duration of negative AP and morale effect
        public int[] bTimesDrugUsedSinceSleep = new int[2];

        public int bBlindedCounter;
        public bool fMercCollapsedFlag;
        public bool fDoneAssignmentAndNothingToDoFlag;
        public bool fMercAsleep;
        bool fDontChargeAPsForStanceChange;

        int ubHoursOnAssignment;                      // used for assignments handled only every X hours

        int ubMercJustFired;   // the merc was just fired..there may be dialogue events occuring, this flag will prevent any interaction with contracts
                                 // until after the merc leaves	
        public int ubTurnsUntilCanSayHeardNoise;
        public SOLDIER_QUOTE usQuoteSaidExtFlags;

        public int sContPathLocation;
        public int bGoodContPath;
        public int ubPendingActionInterrupted;
        public int bNoiseLevel;
        public int bRegenerationCounter;
        public int bRegenBoostersUsedToday;
        public int bNumPelletsHitBy;
        public int sSkillCheckGridNo;
        public int ubLastEnemyCycledID;

        public int ubPrevSectorID;
        public int ubNumTilesMovesSinceLastForget;
        public int bTurningIncrement;
        public int uiBattleSoundID;

        bool fSoldierWasMoving;
        bool fSayAmmoQuotePending;
        public int usValueGoneUp;

        public int ubNumLocateCycles;
        public int ubDelayedMovementFlags;
        public bool fMuzzleFlash;
        public int ubCTGTTargetID;

        //TIMECOUNTER PanelAnimateCounter;
        int uiMercChecksum;

        int bCurrentCivQuote;
        int bCurrentCivQuoteDelta;
        public SOLDIER_MISC ubMiscSoldierFlags;
        public REASON_STOPPED ubReasonCantFinishMove;

        int sLocationOfFadeStart;
        int bUseExitGridForReentryDirection;

        public int uiTimeSinceLastSpoke;
        public int ubContractRenewalQuoteCode;
        public int sPreTraversalGridNo;
        public int uiXRayActivatedTime;
        public bool bTurningFromUI;
        public int bPendingActionData5;

        public int bDelayedStrategicMoraleMod;
        public int ubDoorOpeningNoise;

        public GROUP                                pGroup;
	    public int ubLeaveHistoryCode;
        public bool fDontUnsetLastTargetFromTurn;
        public int bOverrideMoveSpeed;
        public bool fUseMoverrideMoveSpeed;

        public int uiTimeSoldierWillArrive;
        public bool fDieSoundUsed;
        public bool fUseLandingZoneForArrival;
        public bool fFallClockwise;
        public int bVehicleUnderRepairID;
        public int iTimeCanSignElsewhere;
        public int bHospitalPriceModifier;
        public int[] bFillerBytes = new int[3];
        public int uiStartTimeOfInsuranceContract;
        public bool fRTInNonintAnim;
        public bool fDoingExternalDeath;
        public int bCorpseQuoteTolerance;
        public int bYetAnotherPaddingSpace;
        public int iPositionSndID;
        public int iTuringSoundID;
        public int ubLastDamageReason;
        public bool fComplainedThatTired;
        public int[] sLastTwoLocations = new int[2];
        public int bFillerDude;
        public int uiTimeSinceLastBleedGrunt;
        public int ubNextToPreviousAttackerID;

        int[] bFiller = new int[39];
    }

    public class GROUP
    {
        public bool fDebugGroup;                    //for testing purposes -- handled differently in certain cases.
        public bool fPlayer;                            //set if this is a player controlled group.
        public bool fVehicle;                           //vehicle controlled group?
        public bool fPersistant;                    //This flag when set prevents the group from being automatically deleted when it becomes empty.
        public int ubGroupID;                            //the unique ID of the group (used for hooking into events and SOLDIERTYPE)
        public int ubGroupSize;                      //total number of individuals in the group.
        public int ubSectorX;
        public MAP_ROW ubSectorY;     //last/curr sector occupied
        public int ubSectorZ;
        public int ubNextX, ubNextY;             //next sector destination
        public int ubPrevX, ubPrevY;             //prev sector occupied (could be same as ubSectorX/Y)
        public int ubOriginalSector;             //sector where group was created.
        public bool fBetweenSectors;            //set only if a group is between sector.
        public int ubMoveType;                           //determines the type of movement (ONE_WAY, CIRCULAR, ENDTOEND, etc.)
        public int ubNextWaypointID;             //the ID of the next waypoint
        public int ubFatigueLevel;                   //the fatigue level of the weakest member in group
        public int ubRestAtFatigueLevel;     //when the group's fatigue level <= this level, they will rest upon arrival at next sector.
        public int ubRestToFatigueLevel;     //when resting, the group will rest until the fatigue level reaches this level.
        public int uiArrivalTime;                   //the arrival time in world minutes that the group will arrive at the next sector.
        public int uiTraverseTime;              //the total traversal time from the previous sector to the next sector.
        public bool fRestAtNight;                   //set when the group is permitted to rest between 2200 and 0600 when moving
        public bool fWaypointsCancelled;    //set when groups waypoints have been removed.
        //WAYPOINT pWaypoints;                   //a list of all of the waypoints in the groups movement.
        public int ubTransportationMask;     //the mask combining all of the groups transportation methods.
        public int uiFlags;                             //various conditions that apply to the group
        public int ubCreatedSectorID;            //used for debugging strategic AI for keeping track of the sector ID a group was created in.
        public int ubSectorIDOfLastReassignment; //used for debuggin strategic AI.  Records location of any reassignments.
        public int[] bPadding = new int[29];                      //***********************************************//
        
        //PLAYERGROUP pPlayerList;       //list of players in the group
        //ENEMYGROUP pEnemyGroup;        //a structure containing general enemy info
        public GROUP next;						//next group
    }
}
