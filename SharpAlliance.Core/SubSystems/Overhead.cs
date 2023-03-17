using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

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

    public static int FindAdjacentGridEx(SOLDIERTYPE pSoldier, int sGridNo, WorldDirections pubDirection, out int psAdjustedGridNo, bool fForceToPerson, bool fDoor)
    {
        // psAdjustedGridNo gets the original gridno or the new one if updated
        // It will ONLY be updated IF we were over a merc, ( it's updated to their gridno )
        // pubDirection gets the direction to the final gridno
        // fForceToPerson: forces the grid under consideration to be the one occupiedby any target
        // in that location, because we could be passed a gridno based on the overlap of soldier's graphic
        // fDoor determines whether special door-handling code should be used (for interacting with doors)

        int[] sFourGrids = new int[4];
        int sDistance = 0;
        WorldDirections[] sDirs = { WorldDirections.NORTH, WorldDirections.EAST, WorldDirections.SOUTH, WorldDirections.WEST };
        int cnt;
        int sClosest = NOWHERE, sSpot, sOkTest;
        int sCloseGridNo = NOWHERE;
        FIND_SOLDIER_RESPONSES uiMercFlags;
        int usSoldierIndex;
        int ubDir;
        STRUCTURE? pDoor;
        //STRUCTURE                            *pWall;
        WallOrientation ubWallOrientation;
        bool fCheckGivenGridNo = true;
        WorldDirections ubTestDirection;
        EXITGRID ExitGrid;

        // Set default direction
        if (pubDirection > 0)
        {
            pubDirection = pSoldier.bDirection;
        }

        // CHECK IF WE WANT TO FORCE GRIDNO TO PERSON
        psAdjustedGridNo = sGridNo;

        // CHECK IF IT'S THE SAME ONE AS WE'RE ON, IF SO, RETURN THAT!
        if (pSoldier.sGridNo == sGridNo && WorldStructures.FindStructure(sGridNo, (STRUCTUREFLAGS.SWITCH)) is null)
        {
            // OK, if we are looking for a door, it may be in the same tile as us, so find the direction we
            // have to face to get to the door, not just our initial direction...
            // If we are in the same tile as a switch, we can NEVER pull it....
            if (fDoor)
            {
                // This can only happen if a door was to the south to east of us!

                // Do south!
                //sSpot = NewGridNo( sGridNo, DirectionInc( SOUTH ) );

                // ATE: Added: Switch behave EXACTLY like doors
                pDoor = WorldStructures.FindStructure(sGridNo, (STRUCTUREFLAGS.ANYDOOR));

                if (pDoor != null)
                {
                    // Get orinetation
                    ubWallOrientation = pDoor.ubWallOrientation;

                    if (ubWallOrientation == WallOrientation.OUTSIDE_TOP_LEFT || ubWallOrientation == WallOrientation.INSIDE_TOP_LEFT)
                    {
                        // To the south!
                        sSpot = IsometricUtils.NewGridNo(sGridNo, IsometricUtils.DirectionInc(WorldDirections.SOUTH));
                        if (pubDirection > 0)
                        {
                            (pubDirection) = SoldierControl.GetDirectionFromGridNo(sSpot, pSoldier);
                        }
                    }

                    if (ubWallOrientation == WallOrientation.OUTSIDE_TOP_RIGHT || ubWallOrientation == WallOrientation.INSIDE_TOP_RIGHT)
                    {
                        // TO the east!
                        sSpot = IsometricUtils.NewGridNo(sGridNo, IsometricUtils.DirectionInc(WorldDirections.EAST));
                        if (pubDirection > 0)
                        {
                            (pubDirection) = SoldierControl.GetDirectionFromGridNo(sSpot, pSoldier);
                        }
                    }
                }
            }

            // Use soldier's direction
            return (sGridNo);
        }

        // Look for a door!
        if (fDoor)
        {
            pDoor = WorldStructures.FindStructure(sGridNo, (STRUCTUREFLAGS.ANYDOOR | STRUCTUREFLAGS.SWITCH));
        }
        else
        {
            pDoor = null;
        }

        if (fForceToPerson)
        {
            if (SoldierFind.FindSoldier(sGridNo, out usSoldierIndex, out uiMercFlags, FIND_SOLDIER.GRIDNO))
            {
                sGridNo = MercPtrs[usSoldierIndex].sGridNo;
                if (psAdjustedGridNo != null)
                {
                    psAdjustedGridNo = sGridNo;

                    // Use direction to this guy!
                    if (pubDirection > 0)
                    {
                        (pubDirection) = SoldierControl.GetDirectionFromGridNo(sGridNo, pSoldier);
                    }
                }
            }
        }


        if ((sOkTest = NewOKDestination(pSoldier, sGridNo, true, pSoldier.bLevel)) > 0)    // no problem going there! nobody on it!
        {
            // OK, if we are looking to goto a switch, ignore this....
            if (pDoor is not null)
            {
                if (pDoor.fFlags.HasFlag(STRUCTUREFLAGS.SWITCH))
                {
                    // Don't continuel
                    fCheckGivenGridNo = false;
                }
            }

            // If there is an exit grid....
            if (GetExitGrid(sGridNo, out ExitGrid))
            {
                // Don't continuel
                fCheckGivenGridNo = false;
            }


            if (fCheckGivenGridNo)
            {
                sDistance = PlotPath(pSoldier, sGridNo, NO_COPYROUTE, NO_PLOT, TEMPORARY, pSoldier.usUIMovementMode, NOT_STEALTH, FORWARD, pSoldier.bActionPoints);

                if (sDistance > 0)
                {

                    if (sDistance < sClosest)
                    {
                        sClosest = sDistance;
                        sCloseGridNo = sGridNo;
                    }
                }
            }
        }


        for (cnt = 0; cnt < 4; cnt++)
        {
            // MOVE OUT TWO DIRECTIONS
            sFourGrids[cnt] = sSpot = NewGridNo(sGridNo, DirectionInc(sDirs[cnt]));

            ubTestDirection = sDirs[cnt];

            // For switches, ALLOW them to walk through walls to reach it....
            if (pDoor && pDoor.fFlags & STRUCTURE_SWITCH)
            {
                ubTestDirection = gOppositeDirection[ubTestDirection];
            }

            if (fDoor)
            {
                if (gubWorldMovementCosts[sSpot][ubTestDirection][pSoldier.bLevel] >= TRAVELCOST_BLOCKED)
                {
                    // obstacle or wall there!
                    continue;
                }
            }
            else
            {
                // this function returns original MP cost if not a door cost
                if (DoorTravelCost(pSoldier, sSpot, gubWorldMovementCosts[sSpot][ubTestDirection][pSoldier.bLevel], FALSE, null) >= TRAVELCOST_BLOCKED)
                {
                    // obstacle or wall there!
                    continue;
                }
            }

            // Eliminate some directions if we are looking at doors!
            if (pDoor != null)
            {
                // Get orinetation
                ubWallOrientation = pDoor.ubWallOrientation;

                // Refuse the south and north and west  directions if our orientation is top-right
                if (ubWallOrientation == OUTSIDE_TOP_RIGHT || ubWallOrientation == INSIDE_TOP_RIGHT)
                {
                    if (sDirs[cnt] == NORTH || sDirs[cnt] == WEST || sDirs[cnt] == SOUTH)
                        continue;
                }

                // Refuse the north and west and east directions if our orientation is top-right
                if (ubWallOrientation == OUTSIDE_TOP_LEFT || ubWallOrientation == INSIDE_TOP_LEFT)
                {
                    if (sDirs[cnt] == NORTH || sDirs[cnt] == WEST || sDirs[cnt] == EAST)
                        continue;
                }
            }

            // If this spot is our soldier's gridno use that!
            if (sSpot == pSoldier.sGridNo)
            {
                // Use default diurection ) soldier's direction )

                // OK, at least get direction to face......
                // Defaults to soldier's facing dir unless we change it!
                //if ( pDoor != null )
                {
                    // Use direction to the door!
                    if (pubDirection)
                    {
                        (*pubDirection) = (UINT8)GetDirectionFromGridNo(sGridNo, pSoldier);
                    }
                }
                return (sSpot);
            }

            // don't store path, just measure it
            ubDir = (UINT8)GetDirectionToGridNoFromGridNo(sSpot, sGridNo);

            if ((NewOKDestinationAndDirection(pSoldier, sSpot, ubDir, TRUE, pSoldier.bLevel) > 0) &&
                ((sDistance = PlotPath(pSoldier, sSpot, NO_COPYROUTE, NO_PLOT, TEMPORARY, (INT16)pSoldier.usUIMovementMode, NOT_STEALTH, FORWARD, pSoldier.bActionPoints)) > 0))
            {
                if (sDistance < sClosest)
                {
                    sClosest = sDistance;
                    sCloseGridNo = (INT16)sSpot;
                }
            }
        }

        if (sClosest != NOWHERE)
        {
            // Take last direction and use opposite!
            // This will be usefull for ours and AI mercs

            // If our gridno is the same ( which can be if we are look at doors )
            if (sGridNo == sCloseGridNo)
            {
                if (pubDirection)
                {
                    // ATE: Only if we have a valid door!
                    if (pDoor)
                    {
                        switch (pDoor.pDBStructureRef.pDBStructure.ubWallOrientation)
                        {
                            case OUTSIDE_TOP_LEFT:
                            case INSIDE_TOP_LEFT:

                                *pubDirection = SOUTH;
                                break;

                            case OUTSIDE_TOP_RIGHT:
                            case INSIDE_TOP_RIGHT:

                                *pubDirection = EAST;
                                break;
                        }
                    }
                }
            }
            else
            {
                // Calculate direction if our gridno is different....
                ubDir = (UINT8)GetDirectionToGridNoFromGridNo(sCloseGridNo, sGridNo);
                if (pubDirection)
                {
                    *pubDirection = ubDir;
                }
            }
            //if ( psAdjustedGridNo != null )
            //{
            //		(*psAdjustedGridNo) = sCloseGridNo;
            //}
            if (sCloseGridNo == NOWHERE)
            {
                return (-1);
            }
            return (sCloseGridNo);
        }
        else
        {
            return (-1);
        }
    }

    // NB if making changes don't forget to update NewOKDestinationAndDirection
    public static bool NewOKDestination(SOLDIERTYPE pCurrSoldier, int sGridNo, bool fPeopleToo, int bLevel)
    {
        int bPerson;
        STRUCTURE? pStructure;
        STRUCTURE_ON sDesiredLevel;
        bool fOKCheckStruct;

        if (!GridNoOnVisibleWorldTile(sGridNo))
        {
            return (true);
        }

        if (fPeopleToo && (bPerson = WhoIsThere2(sGridNo, bLevel)) != NO_SOLDIER)
        {
            // we could be multitiled... if the person there is us, and the gridno is not
            // our base gridno, skip past these checks
            if (!(bPerson == pCurrSoldier.ubID && sGridNo != pCurrSoldier.sGridNo))
            {
                if (pCurrSoldier.bTeam == gbPlayerNum)
                {
                    if ((Menptr[bPerson].bVisible >= 0) || (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.SHOW_ALL_MERCS)))
                    {
                        return (false);                 // if someone there it's NOT OK
                    }
                }
                else
                {
                    return (false);                 // if someone there it's NOT OK
                }
            }
        }

        // Check structure database
        if ((pCurrSoldier.uiStatusFlags.HasFlag(SOLDIER.MULTITILE)) && !(gfEstimatePath))
        {
            AnimationSurfaceTypes usAnimSurface;
            STRUCTURE_FILE_REF? pStructureFileRef;
            bool fOk;
            int bLoop;
            int usStructureID = INVALID_STRUCTURE_ID;

            // this could be kinda slow...

            // Get animation surface...
            usAnimSurface = AnimationControl.DetermineSoldierAnimationSurface(pCurrSoldier, pCurrSoldier.usUIMovementMode);
            // Get structure ref...
            pStructureFileRef = GetAnimationStructureRef(pCurrSoldier.ubID, usAnimSurface, pCurrSoldier.usUIMovementMode);

            // opposite directions should be mirrors, so only check 4
            if (pStructureFileRef)
            {
                // if ANY direction is valid, consider moving here valid
                for (bLoop = 0; bLoop < NUM_WORLD_DIRECTIONS; bLoop++)
                {
                    // ATE: Only if we have a levelnode...
                    if (pCurrSoldier.pLevelNode != null && pCurrSoldier.pLevelNode.pStructureData != null)
                    {
                        usStructureID = pCurrSoldier.pLevelNode.pStructureData.usStructureID;
                    }
                    else
                    {
                        usStructureID = INVALID_STRUCTURE_ID;
                    }

                    fOk = InternalOkayToAddStructureToWorld(sGridNo, bLevel, (pStructureFileRef.pDBStructureRef[bLoop]), usStructureID, (BOOLEAN)!fPeopleToo);
                    if (fOk)
                    {
                        return (true);
                    }
                }
            }

            return (false);
        }
        else
        {
            // quick test
            if (gpWorldLevelData[sGridNo].pStructureHead != null)
            {
                // Something is here, check obstruction in future
                if (bLevel == 0)
                {
                    sDesiredLevel = STRUCTURE_ON.GROUND;
                }
                else
                {
                    sDesiredLevel = STRUCTURE_ON.ROOF;
                }

                pStructure = WorldStructures.FindStructure(sGridNo, STRUCTUREFLAGS.BLOCKSMOVES);

                // ATE: If we are trying to get a path to an exit grid AND
                // we are a cave....still allow this..
                //if ( pStructure && gfPlotPathToExitGrid && pStructure.fFlags & STRUCTURE_CAVEWALL )
                if (pStructure is not null && gfPlotPathToExitGrid)
                {
                    pStructure = null;
                }

                while (pStructure != null)
                {
                    if (!(pStructure.fFlags.HasFlag(STRUCTUREFLAGS.PASSABLE)))
                    {
                        fOKCheckStruct = true;

                        // Check if this is a multi-tile
                        if ((pStructure.fFlags.HasFlag(STRUCTUREFLAGS.MOBILE)) && (pCurrSoldier.uiStatusFlags.HasFlag(SOLDIER.MULTITILE)))
                        {
                            // Check IDs with soldier's ID
                            if (pCurrSoldier.pLevelNode != null && pCurrSoldier.pLevelNode.pStructureData != null && pCurrSoldier.pLevelNode.pStructureData.usStructureID == pStructure.usStructureID)
                            {
                                fOKCheckStruct = false;
                            }
                        }

                        if (fOKCheckStruct)
                        {
                            if (pStructure.sCubeOffset == sDesiredLevel)
                            {
                                return (false);
                            }
                        }
                    }

                    pStructure = FindNextStructure(pStructure, STRUCTUREFLAGS.BLOCKSMOVES);
                }
            }
        }

        return (true);
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

        foreach (var pSoldier in Menptr)
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
    public int[] sPanicTriggerGridNo = new int[Globals.NUM_PANIC_TRIGGERS];
    public int[] bPanicTriggerIsAlarm = new int[Globals.NUM_PANIC_TRIGGERS];
    public int[] ubPanicTolerance = new int[Globals.NUM_PANIC_TRIGGERS];
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
