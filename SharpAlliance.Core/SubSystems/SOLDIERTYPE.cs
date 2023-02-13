using SharpAlliance.Core.Managers.Image;

namespace SharpAlliance.Core.SubSystems
{
    public class SOLDIERTYPE
    {
        // ID
        public int ubID;
        int bReserved1;

        // DESCRIPTION / STATS, ETC
        public int ubBodyType;
        public int bActionPoints;
        int bInitialActionPoints;

        public SOLDIER uiStatusFlags { get; set; }

        public OBJECTTYPE[] inv = new OBJECTTYPE[(int)InventorySlot.NUM_INV_SLOTS];
        OBJECTTYPE pTempObject;
        // KEY_ON_RING pKeyRing;

        int bOldLife;          // life at end of last turn, recorded for monster AI
                                // attributes
        public int bInSector;
        int bFlashPortraitFrame;
        int sFractLife;       // fraction of life pts (in hundreths)	
        public int bBleeding;     // blood loss control variable
        public int bBreath;           // current breath value
        int bBreathMax;   // max breath, affected by fatigue/sleep
        int bStealthMode;

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
        public int[] name = new int[10];

        public int bVisible;          // to render or not to render...


        public bool bActive;

        public int bTeam;             // Team identifier

        //NEW MOVEMENT INFORMATION for Strategic Movement
        int ubGroupID;        //the movement group the merc is currently part of.
        bool fBetweenSectors;    //set when the group isn't actually in a sector.
                                    //sSectorX and sSectorY will reflect the sector the
                                    //merc was at last.

        int ubMovementNoiseHeard;// 8 flags by direction

        // 23 bytes so far 	

        // WORLD POSITION STUFF
        float dXPos;
        float dYPos;
        float dOldXPos;
        float dOldYPos;
        int sInitialGridNo;
        public int sGridNo;
        public int bDirection;
        int sHeightAdjustment;
        int sDesiredHeight;
        int sTempNewGridNo;                   // New grid no for advanced animations
        int sRoomNo;
        int bOverTerrainType;
        int bOldOverTerrainType;

        public bool bCollapsed;                    // collapsed due to being out of APs
        int bBreathCollapsed;                  // collapsed due to being out of APs
                                                // 50 bytes so far


        int ubDesiredHeight;
        public int usPendingAnimation;
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

        int ubSkillTrait1;
        int ubSkillTrait2;

        int uiAIDelay;
        int bDexterity;        // dexterity (hand coord) value
        int bWisdom;
        int sReloadDelay;
        int ubAttackerID;
        int ubPreviousAttackerID;
        bool fTurnInProgress;

        bool fIntendedTarget; // intentionally shot?
        bool fPauseAllAnimation;

        int bExpLevel;     // general experience level
        int sInsertionGridNo;

        bool fContinueMoveAfterStanceChange;

        // 60
        //AnimationSurfaceCacheType AnimCache; // will be 9 bytes once changed to pointers

        public int bLife;             // current life (hit points or health)
        public int bSide;
        int bViewRange;
        int bNewOppCnt;
        int bService;      // first aid, or other time consuming process

        int usAniCode;
        int usAniFrame;
        int sAniDelay;

        // MOVEMENT TO NEXT TILE HANDLING STUFF
        int bAgility;          // agility (speed) value
        int ubDelayedMovementCauseMerc;
        int sDelayedMovementCauseGridNo;
        int sReservedMovementGridNo;

        int bStrength;

        // Weapon Stuff
        bool fHoldAttackerUntilDone;
        int sTargetGridNo;
        int bTargetLevel;
        public int bTargetCubeLevel;
        int sLastTarget;
        int bTilesMoved;
        int bLeadership;
        float dNextBleed;
        bool fWarnedAboutBleeding;
        bool fDyingComment;

        int ubTilesMovedPerRTBreathUpdate;
        int usLastMovementAnimPerRTBreathUpdate;

        bool fTurningToShoot;
        bool fTurningToFall;
        public bool fTurningUntilDone;
        bool fGettingHit;
        bool fInNonintAnim;
        bool fFlashLocator;
        int sLocatorFrame;
        bool fShowLocator;
        bool fFlashPortrait;
        int bMechanical;
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
        public int[] pShades = new int[OverheadTypes.NUM_SOLDIER_SHADES]; // Shading tables
        int[] pGlowShades = new int[20]; // 
        public int pCurrentShade;
        public int bMedical;
        bool fBeginFade;
        int ubFadeLevel;
        int ubServiceCount;
        int ubServicePartner;
        int bMarksmanship;
        int bExplosive;
        // THROW_PARAMS pThrowParams;
        public bool fTurningFromPronePosition;
        public bool bReverse;
        //struct TAG_level_node               *pLevelNode;
        //struct TAG_level_node               *pExternShadowLevelNode;
        //struct TAG_level_node               *pRoofUILevelNode;

        // WALKING STUFF
        int bDesiredDirection;
        int sDestXPos;
        int sDestYPos;
        int sDesiredDest;
        int sDestination;
        public int sFinalDestination;
        public int bLevel;
        int bStopped;
        int bNeedToLook;


        // PATH STUFF
        int[] usPathingData = new int[OverheadTypes.MAX_PATH_LIST_SIZE];
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

        public int ubStrategicInsertionCode;
        public int usStrategicInsertionData;

        int iLight;
        int iMuzFlash;
        int bMuzFlashCount;

        int sX;
        int sY;

        int usOldAniState;
        int sOldAniCode;

        int bBulletsLeft;
        int ubSuppressionPoints;

        // STUFF FOR RANDOM ANIMATIONS
        int uiTimeOfLastRandomAction;
        int usLastRandomAnim;


        // AI STUFF
        int[] bOppList = new int[OverheadTypes.MAX_NUM_SOLDIERS]; // AI knowledge database
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
        int bUnderFire;
        int bShock;
        int bUnderEscort;
        int bBypassToGreen;
        int ubLastMercToRadio;
        int bDominantDir;              // AI main direction to face...
        int bPatrolCnt;                    // number of patrol gridnos
        int bNextPatrolPnt;            // index to next patrol gridno
        int[] usPatrolGrid = new int[SoldierControl.MAXPATROLGRIDS];// AI list for ptr->orders==PATROL
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
        public int ubPendingAction;
        public int ubPendingActionAnimCount;
        public int uiPendingActionData1;
        int sPendingActionData2;
        int bPendingActionData3;
        int ubDoorHandleCode;
        int uiPendingActionData4;
        int bInterruptDuelPts;
        int bPassedLastInterrupt;
        int bIntStartAPs;
        int bMoved;
        int bHunting;
        int ubLastCall;
        int ubCaller;
        int sCallerGridNo;
        int bCallPriority;
        int bCallActedUpon;
        int bFrenzied;
        int bNormalSmell;
        int bMonsterSmell;
        int bMobility;
        int bRTPCombat;
        int fAIFlags;

        bool fDontChargeReadyAPs;
        int usAnimSurface;
        int sZLevel;
        bool fPrevInWater;
        bool fGoBackToAimAfterHit;

        int sWalkToAttackGridNo;
        int sWalkToAttackWalkToCost;

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
        int bDoBurst;
        public AnimationStates usUIMovementMode;
        int bUIInterfaceLevel;
        public bool fUIMovementFast;

        // TIMECOUNTER BlinkSelCounter;
        // TIMECOUNTER PortraitFlashCounter;
        bool fDeadSoundPlayed;
        public int ubProfile { get; }
        int ubQuoteRecord;
        int ubQuoteActionID;
        int ubBattleSoundID;

        bool fClosePanel;
        bool fClosePanelToDie;
        int ubClosePanelFrame;
        bool fDeadPanel;
        int ubDeadPanelFrame;
        bool fOpenPanel;
        int bOpenPanelFrame;

        int sPanelFaceX;
        int sPanelFaceY;

        // QUOTE STUFF
        int bNumHitsThisTurn;
        int usQuoteSaidFlags;
        int fCloseCall;
        int bLastSkillCheck;
        int ubSkillCheckAttempts;

        int bVocalVolume;  // verbal sounds need to differ in volume

        int bStartFallDir;
        int fTryingToFall;

        int ubPendingDirection;
        int uiAnimSubFlags;

        public int bAimShotLocation;
        int ubHitLocation;

        int[] pEffectShades = new int[OverheadTypes.NUM_SOLDIER_EFFECTSHADES]; // Shading tables for effects

        int ubPlannedUIAPCost;
        int sPlannedTargetX;
        int sPlannedTargetY;

        int[] sSpreadLocations = new int[6];
        bool fDoSpread;
        int sStartGridNo;
        int sEndGridNo;
        int sForcastGridno;
        int sZLevelOverride;
        int bMovedPriorToInterrupt;
        int iEndofContractTime;               // time, in global time(resolution, minutes) that merc will leave, or if its a M.E.R.C. merc it will be set to -1.  -2 for NPC and player generated
        int iStartContractTime;
        int iTotalContractLength;         // total time of AIM mercs contract	or the time since last paid for a M.E.R.C. merc
        int iNextActionSpecialData;       // AI special action data record for the next action
        public int ubWhatKindOfMercAmI;          //Set to the type of character it is
        public int bAssignment;                           // soldiers current assignment 
        int bOldAssignment;                        // old assignment, for autosleep purposes
        bool fForcedToStayAwake;             // forced by player to stay awake, reset to false, the moment they are set to rest or sleep
        int bTrainStat;                                // current stat soldier is training
        int sSectorX;                                 // X position on the Stategic Map
        int sSectorY;                                 // Y position on the Stategic Map
        int bSectorZ;                                  // Z sector location
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
        int ubCivilianGroup;

        // time changes...when a stat was changed according to GetJA2Clock();
        int uiChangeLevelTime;
        int uiChangeHealthTime;
        int uiChangeStrengthTime;
        int uiChangeDexterityTime;
        int uiChangeAgilityTime;
        int uiChangeWisdomTime;
        int uiChangeLeadershipTime;
        int uiChangeMarksmanshipTime;
        int uiChangeExplosivesTime;
        int uiChangeMedicalTime;
        int uiChangeMechanicalTime;

        int uiUniqueSoldierIdValue; // the unique value every instance of a soldier gets - 1 is the first valid value
        int bBeingAttackedCount;       // Being attacked counter

        int[] bNewItemCount = new int[(int)InventorySlot.NUM_INV_SLOTS];
        int[] bNewItemCycleCount = new int[(int)InventorySlot.NUM_INV_SLOTS];
        bool fCheckForNewlyAddedItems;
        int bEndDoorOpenCode;

        int ubScheduleID;
        int sEndDoorOpenCodeData;
        //TIMECOUNTER NextTileCounter;
        bool fBlockedByAnotherMerc;
        int bBlockedByAnotherMercDirection;
        int usAttackingWeapon;
        public int bWeaponMode;
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
        int fPastXDest;
        int fPastYDest;
        int bMovementDirection;
        int sOldGridNo;
        public int usDontUpdateNewGridNoOnMoveAnimChange;
        int sBoundingBoxWidth;
        int sBoundingBoxHeight;
        int sBoundingBoxOffsetX;
        int sBoundingBoxOffsetY;
        int uiTimeSameBattleSndDone;
        int bOldBattleSnd;
        bool fReactingFromBeingShot;
        bool fContractPriceHasIncreased;
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
        int usValueGoneUp;

        int ubNumLocateCycles;
        int ubDelayedMovementFlags;
        bool fMuzzleFlash;
        int ubCTGTTargetID;

        //TIMECOUNTER PanelAnimateCounter;
        int uiMercChecksum;

        int bCurrentCivQuote;
        int bCurrentCivQuoteDelta;
        int ubMiscSoldierFlags;
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
        bool fDebugGroup;                    //for testing purposes -- handled differently in certain cases.
        bool fPlayer;                            //set if this is a player controlled group.
        bool fVehicle;                           //vehicle controlled group?
        bool fPersistant;                    //This flag when set prevents the group from being automatically deleted when it becomes empty.
        int ubGroupID;                            //the unique ID of the group (used for hooking into events and SOLDIERTYPE)
        int ubGroupSize;                      //total number of individuals in the group.
        int ubSectorX, ubSectorY;     //last/curr sector occupied
        int ubSectorZ;
        int ubNextX, ubNextY;             //next sector destination
        int ubPrevX, ubPrevY;             //prev sector occupied (could be same as ubSectorX/Y)
        int ubOriginalSector;             //sector where group was created.
        bool fBetweenSectors;            //set only if a group is between sector.
        int ubMoveType;                           //determines the type of movement (ONE_WAY, CIRCULAR, ENDTOEND, etc.)
        int ubNextWaypointID;             //the ID of the next waypoint
        int ubFatigueLevel;                   //the fatigue level of the weakest member in group
        int ubRestAtFatigueLevel;     //when the group's fatigue level <= this level, they will rest upon arrival at next sector.
        int ubRestToFatigueLevel;     //when resting, the group will rest until the fatigue level reaches this level.
        int uiArrivalTime;                   //the arrival time in world minutes that the group will arrive at the next sector.
        int uiTraverseTime;              //the total traversal time from the previous sector to the next sector.
        bool fRestAtNight;                   //set when the group is permitted to rest between 2200 and 0600 when moving
        bool fWaypointsCancelled;    //set when groups waypoints have been removed.
        //WAYPOINT pWaypoints;                   //a list of all of the waypoints in the groups movement.
        int ubTransportationMask;     //the mask combining all of the groups transportation methods.
        int uiFlags;                             //various conditions that apply to the group
        int ubCreatedSectorID;            //used for debugging strategic AI for keeping track of the sector ID a group was created in.
        int ubSectorIDOfLastReassignment; //used for debuggin strategic AI.  Records location of any reassignments.
        int[] bPadding = new int[29];                      //***********************************************//

        //PLAYERGROUP pPlayerList;       //list of players in the group
        //ENEMYGROUP pEnemyGroup;        //a structure containing general enemy info
        GROUP next;						//next group
    }
}
