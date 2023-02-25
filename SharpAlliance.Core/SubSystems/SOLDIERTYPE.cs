using SharpAlliance.Core.Managers.Image;

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

        public OBJECTTYPE[] inv = new OBJECTTYPE[(int)InventorySlot.NUM_INV_SLOTS];
        OBJECTTYPE pTempObject;
        // KEY_ON_RING pKeyRing;

        public int bOldLife;          // life at end of last turn, recorded for monster AI
                                // attributes
        public bool bInSector { get; set; }
        int bFlashPortraitFrame;
        int sFractLife;       // fraction of life pts (in hundreths)	
        public int bBleeding;     // blood loss control variable
        public int bBreath;           // current breath value
        int bBreathMax;   // max breath, affected by fatigue/sleep
        public bool bStealthMode { get; set; }

        int sBreathRed;           // current breath value
        public bool fDelayedMovement;

        bool fReloading;
        int ubWaitActionToDo;
        bool fPauseAim;
        int ubInsertionDirection;
        int bGunType;
        // skills
        int ubOppNum;
        int bLastRenderVisibleValue;
        bool fInMissionExitNode;
        int ubAttackingHand;
        int bScientific;
        // traits	
        int sWeightCarriedAtTurnStart;
        public string name; // max 10 chars?

        public int bVisible;          // to render or not to render...


        public bool bActive;

        public TEAM bTeam;             // Team identifier

        //NEW MOVEMENT INFORMATION for Strategic Movement
        public int ubGroupID;        //the movement group the merc is currently part of.
        bool fBetweenSectors;    //set when the group isn't actually in a sector.
                                    //sSectorX and sSectorY will reflect the sector the
                                    //merc was at last.

        int ubMovementNoiseHeard;// 8 flags by direction

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
        int sDesiredHeight;
        int sTempNewGridNo;                   // New grid no for advanced animations
        int sRoomNo;
        int bOverTerrainType;
        int bOldOverTerrainType;

        public bool bCollapsed;                    // collapsed due to being out of APs
        int bBreathCollapsed;                  // collapsed due to being out of APs
                                                // 50 bytes so far


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

        bool fContinueMoveAfterStanceChange;

        // 60
        //AnimationSurfaceCacheType AnimCache; // will be 9 bytes once changed to pointers

        public int bLife;             // current life (hit points or health)
        public TEAM bSide;
        public int bViewRange;
        public int bNewOppCnt;
        public int bService;      // first aid, or other time consuming process

        int usAniCode;
        public int usAniFrame;
        int sAniDelay;

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

        int ubTilesMovedPerRTBreathUpdate;
        int usLastMovementAnimPerRTBreathUpdate;

        bool fTurningToShoot;
        bool fTurningToFall;
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
        int[] usFrontArcFullTileList = new int[SoldierControl.MAX_FULLTILE_DIRECTIONS];
        int[] usFrontArcFullTileGridNos = new int[SoldierControl.MAX_FULLTILE_DIRECTIONS];


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
        public int bDesiredDirection;
        public int sDestXPos;
        public int sDestYPos;
        public int sDesiredDest;
        public int sDestination;
        public int sFinalDestination;
        public int bLevel;
        public int bStopped;
        public int bNeedToLook;


        // PATH STUFF
        int[] usPathingData = new int[Globals.MAX_PATH_LIST_SIZE];
        public int usPathDataSize;
        int usPathIndex;
        int sBlackList;
        public int bAimTime;
        public int bShownAimTime;
        public int bPathStored;   // good for AI to reduct redundancy
        int bHasKeys;          // allows AI controlled dudes to open locked doors


        // UNBLIT BACKGROUND
        int pBackGround;
        int pZBackground;
        int usUnblitX, usUnblitY;
        int usUnblitWidth, usUnblitHeight;

        public INSERTION_CODE ubStrategicInsertionCode;
        public int usStrategicInsertionData;

        int iLight;
        int iMuzFlash;
        int bMuzFlashCount;

        public int sX;
        public int sY;

        int usOldAniState;
        int sOldAniCode;

        int bBulletsLeft;
        int ubSuppressionPoints;

        // STUFF FOR RANDOM ANIMATIONS
        int uiTimeOfLastRandomAction;
        int usLastRandomAnim;


        // AI STUFF
        public int[] bOppList = new int[Globals.MAX_NUM_SOLDIERS]; // AI knowledge database
        int bLastAction;
        int bAction;
        int usActionData;
        int bNextAction;
        int usNextActionData;
        int bActionInProgress;
        int bAlertStatus;
        int bOppCnt;
        public int bNeutral;
        int bNewSituation;
        int bNextTargetLevel;
        int bOrders;
        int bAttitude;
        public int bUnderFire;
        int bShock;
        int bUnderEscort;
        int bBypassToGreen;
        int ubLastMercToRadio;
        int bDominantDir;              // AI main direction to face...
        int bPatrolCnt;                    // number of patrol gridnos
        int bNextPatrolPnt;            // index to next patrol gridno
        int[] usPatrolGrid = new int[SoldierControl.MAXPATROLGRIDS];// AI list for ptr.orders==PATROL
        int sNoiseGridno;
        int ubNoiseVolume;
        int bLastAttackHit;
        int ubXRayedBy;
        float dHeightAdjustment;
        int bMorale;
        int bTeamMoraleMod;
        int bTacticalMoraleMod;
        int bStrategicMoraleMod;
        int bAIMorale;
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
        public int fAIFlags;

        public bool fDontChargeReadyAPs;
        public int usAnimSurface;
        public int sZLevel;
        public bool fPrevInWater;
        public bool fGoBackToAimAfterHit;

        public int sWalkToAttackGridNo;
        public int sWalkToAttackWalkToCost;

        bool fForceRenderColor;
        bool fForceNoRenderPaletteCycle;
        int sLocatorOffX;
        int sLocatorOffY;
        bool fStopPendingNextTile;
        bool fForceShade;
        int pForcedShade;
        int bDisplayDamageCount;
        int fDisplayDamage;
        int sDamage;
        int sDamageX;
        int sDamageY;
        int bDamageDir;
        public bool bDoBurst;
        public AnimationStates usUIMovementMode;
        public int bUIInterfaceLevel;
        public bool fUIMovementFast;

        // TIMECOUNTER BlinkSelCounter;
        // TIMECOUNTER PortraitFlashCounter;
        public bool fDeadSoundPlayed;
        public NPCID ubProfile { get; }
        public int ubQuoteRecord;
        public int ubQuoteActionID;
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
        public int usQuoteSaidFlags;
        public int fCloseCall;
        public int bLastSkillCheck;
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
        int usLifeInsurance;          // is there life insurance taken out on merc  
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
        int usPendingAnimation2;
        public int ubCivilianGroup;
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
        bool fBlockedByAnotherMerc;
        int bBlockedByAnotherMercDirection;
        public Items usAttackingWeapon;
        public WM bWeaponMode;
        int ubTargetID;
        int bAIScheduleProgress;
        int sOffWorldGridNo;
        //TAG_anitile                 pAniTile;	
        int bCamo;
        int sAbsoluteFinalDestination;
        int ubHiResDirection;
        int ubHiResDesiredDirection;
        int ubLastFootPrintSound;
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
        int ubRobotRemoteHolderID;
        int uiTimeOfLastContractUpdate;
        int bTypeOfLastContract;
        int bTurnsCollapsed;
        int bSleepDrugCounter;
        int ubMilitiaKills;

        int[] bFutureDrugEffect = new int[2];                      // value to represent effect of a needle
        int[] bDrugEffectRate = new int[2];                            // represents rate of increase and decrease of effect  
        int[] bDrugEffect = new int[2];                                    // value that affects AP & morale calc ( -ve is poorly )
        int[] bDrugSideEffectRate = new int[2];                    // duration of negative AP and morale effect
        int[] bDrugSideEffect = new int[2];                            // duration of negative AP and morale effect
        int[] bTimesDrugUsedSinceSleep = new int[2];

        int bBlindedCounter;
        bool fMercCollapsedFlag;
        bool fDoneAssignmentAndNothingToDoFlag;
        public bool fMercAsleep;
        bool fDontChargeAPsForStanceChange;

        int ubHoursOnAssignment;                      // used for assignments handled only every X hours

        int ubMercJustFired;   // the merc was just fired..there may be dialogue events occuring, this flag will prevent any interaction with contracts
                                 // until after the merc leaves	
        int ubTurnsUntilCanSayHeardNoise;
        int usQuoteSaidExtFlags;

        public int sContPathLocation;
        public int bGoodContPath;
        int ubPendingActionInterrupted;
        int bNoiseLevel;
        int bRegenerationCounter;
        int bRegenBoostersUsedToday;
        int bNumPelletsHitBy;
        int sSkillCheckGridNo;
        int ubLastEnemyCycledID;

        int ubPrevSectorID;
        int ubNumTilesMovesSinceLastForget;
        int bTurningIncrement;
        int uiBattleSoundID;

        bool fSoldierWasMoving;
        bool fSayAmmoQuotePending;
        public int usValueGoneUp;

        int ubNumLocateCycles;
        int ubDelayedMovementFlags;
        bool fMuzzleFlash;
        int ubCTGTTargetID;

        //TIMECOUNTER PanelAnimateCounter;
        int uiMercChecksum;

        int bCurrentCivQuote;
        int bCurrentCivQuoteDelta;
        public SOLDIER_MISC ubMiscSoldierFlags;
        int ubReasonCantFinishMove;

        int sLocationOfFadeStart;
        int bUseExitGridForReentryDirection;

        int uiTimeSinceLastSpoke;
        int ubContractRenewalQuoteCode;
        int sPreTraversalGridNo;
        int uiXRayActivatedTime;
        public bool bTurningFromUI;
        int bPendingActionData5;

        int bDelayedStrategicMoraleMod;
        int ubDoorOpeningNoise;

        GROUP                                pGroup;
	int ubLeaveHistoryCode;
        bool fDontUnsetLastTargetFromTurn;
        int bOverrideMoveSpeed;
        bool fUseMoverrideMoveSpeed;

        int uiTimeSoldierWillArrive;
        bool fDieSoundUsed;
        bool fUseLandingZoneForArrival;
        bool fFallClockwise;
        int bVehicleUnderRepairID;
        int iTimeCanSignElsewhere;
        int bHospitalPriceModifier;
        int[] bFillerBytes = new int[3];
        int uiStartTimeOfInsuranceContract;
        bool fRTInNonintAnim;
        bool fDoingExternalDeath;
        int bCorpseQuoteTolerance;
        int bYetAnotherPaddingSpace;
        int iPositionSndID;
        int iTuringSoundID;
        int ubLastDamageReason;
        bool fComplainedThatTired;
        int[] sLastTwoLocations = new int[2];
        int bFillerDude;
        int uiTimeSinceLastBleedGrunt;
        int ubNextToPreviousAttackerID;

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
