using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;
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

    // NB if making changes don't forget to update NewOKDestination
    public static bool NewOKDestinationAndDirection(SOLDIERTYPE pCurrSoldier, int sGridNo, WorldDirections bDirection, bool fPeopleToo, int bLevel)
    {
        int bPerson;
        STRUCTURE? pStructure;
        STRUCTURE_ON sDesiredLevel;
        bool fOKCheckStruct;

        if (fPeopleToo && (bPerson = WorldManager.WhoIsThere2(sGridNo, bLevel)) != NO_SOLDIER)
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
            WorldDirections bLoop;
            int usStructureID = INVALID_STRUCTURE_ID;

            // this could be kinda slow...

            // Get animation surface...
            usAnimSurface = AnimationControl.DetermineSoldierAnimationSurface(pCurrSoldier, pCurrSoldier.usUIMovementMode);
            // Get structure ref...
            pStructureFileRef = AnimationData.GetAnimationStructureRef(pCurrSoldier.ubID, usAnimSurface, pCurrSoldier.usUIMovementMode);

            if (pStructureFileRef is not null)
            {

                // use the specified direction for checks
                bLoop = bDirection;

                {
                    // ATE: Only if we have a levelnode...
                    if (pCurrSoldier.pLevelNode != null && pCurrSoldier.pLevelNode.pStructureData != null)
                    {
                        usStructureID = pCurrSoldier.pLevelNode.pStructureData.usStructureID;
                    }

                    fOk = InternalOkayToAddStructureToWorld(sGridNo, pCurrSoldier.bLevel, (pStructureFileRef.pDBStructureRef[gOneCDirection[bLoop]]), usStructureID, (bool)!fPeopleToo);
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

                    pStructure = StructureInternals.FindNextStructure(pStructure, STRUCTUREFLAGS.BLOCKSMOVES);
                }
            }
        }
        return (true);
    }

    public static void SelectSoldier(int usSoldierID, bool fAcknowledge, bool fForceReselect)
    {
        InternalSelectSoldier(usSoldierID, fAcknowledge, fForceReselect, false);
    }

    private static void InternalSelectSoldier(int usSoldierID, bool fAcknowledge, bool fForceReselect, bool fFromUI)
    {
        SOLDIERTYPE? pSoldier, pOldSoldier;


        // ARM: can't call SelectSoldier() in mapscreen, that will initialize interface panels!!!
        // ATE: Adjusted conditions a bit ( sometimes were not getting selected )
        if (guiCurrentScreen == ScreenName.LAPTOP_SCREEN || guiCurrentScreen == ScreenName.MAP_SCREEN)
        {
            return;
        }

        if (usSoldierID == NOBODY)
        {
            return;
        }

        //if we are in the shop keeper interface
        if (guiTacticalInterfaceFlags.HasFlag(INTERFACE.SHOPKEEP_INTERFACE))
        {
            //dont allow the player to change the selected merc
            return;
        }


        // Get guy
        pSoldier = MercPtrs[usSoldierID];


        // If we are dead, ignore
        if (!OK_CONTROLLABLE_MERC(pSoldier))
        {
            return;
        }

        // Don't do it if we don't have an interrupt
        if (!OK_INTERRUPT_MERC(pSoldier))
        {
            // OK, we want to display message that we can't....
            if (fFromUI)
            {
                Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG_UI_FEEDBACK, TacticalStr[MERC_IS_UNAVAILABLE_STR], pSoldier.name);
            }
            return;
        }

        if (pSoldier.ubID == gusSelectedSoldier)
        {
            if (!fForceReselect)
            {
                return;
            }
        }

        // CANCEL FROM PLANNING MODE!
        if (InUIPlanMode())
        {
            EndUIPlan();
        }

        // Unselect old selected guy
        if (gusSelectedSoldier != NO_SOLDIER)
        {
            // Get guy
            pOldSoldier = MercPtrs[gusSelectedSoldier];
            pOldSoldier.fShowLocator = false;
            pOldSoldier.fFlashLocator = false;

            // DB This used to say pSoldier... I fixed it
            if (pOldSoldier.bLevel == 0)
            {
                //ConcealWalls((int)(pSoldier.dXPos/CELL_X_SIZE), (int)(pSoldier.dYPos/CELL_Y_SIZE), REVEAL_WALLS_RADIUS);
                //	ApplyTranslucencyToWalls((int)(pOldSoldier.dXPos/CELL_X_SIZE), (int)(pOldSoldier.dYPos/CELL_Y_SIZE));
                //LightHideTrees((int)(pOldSoldier.dXPos/CELL_X_SIZE), (int)(pOldSoldier.dYPos/CELL_Y_SIZE));
            }
            //DeleteSoldierLight( pOldSoldier );

            if (pOldSoldier.uiStatusFlags.HasFlag(SOLDIER.GREEN_RAY))
            {
                LightHideRays((pOldSoldier.dXPos / CELL_X_SIZE), (pOldSoldier.dYPos / CELL_Y_SIZE));
                pOldSoldier.uiStatusFlags &= (~SOLDIER.GREEN_RAY);
            }

            UpdateForContOverPortrait(pOldSoldier, false);
        }

        gusSelectedSoldier = usSoldierID;

        // find which squad this guy is, then set selected squad to this guy
        Squads.SetCurrentSquad((SquadEnum)pSoldier.bAssignment, false);

        if (pSoldier.bLevel == 0)
        {
            //RevealWalls((int)(pSoldier.dXPos/CELL_X_SIZE), (int)(pSoldier.dYPos/CELL_Y_SIZE), REVEAL_WALLS_RADIUS);
            //	CalcTranslucentWalls((int)(pSoldier.dXPos/CELL_X_SIZE), (int)(pSoldier.dYPos/CELL_Y_SIZE));
            //LightTranslucentTrees((int)(pSoldier.dXPos/CELL_X_SIZE), (int)(pSoldier.dYPos/CELL_Y_SIZE));
        }

        //SetCheckSoldierLightFlag( pSoldier );

        // Set interface to reflect new selection!
        SetCurrentTacticalPanelCurrentMerc(usSoldierID);

        // PLay ATTN SOUND
        if (fAcknowledge)
        {
            if (!GameSettings.fOptions[TOPTION.MUTE_CONFIRMATIONS])
            {
                DoMercBattleSound(pSoldier, BATTLE_SOUND_ATTN1);
            }
        }

        // Change UI mode to reflact that we are selected
        // NOT if we are locked inthe UI
        if (gTacticalStatus.ubCurrentTeam == OUR_TEAM
            && gCurrentUIMode != UI_MODE.LOCKUI_MODE
            && gCurrentUIMode != UI_MODE.LOCKOURTURN_UI_MODE)
        {
            guiPendingOverrideEvent = UI_EVENT_DEFINES.M_ON_TERRAIN;
        }

        ChangeInterfaceLevel(pSoldier.bLevel);

        if (pSoldier.fMercAsleep)
        {
            PutMercInAwakeState(pSoldier);
        }

        // possibly say personality quote
        if ((pSoldier.bTeam == gbPlayerNum) && (pSoldier.ubProfile != NO_PROFILE && pSoldier.ubWhatKindOfMercAmI != MERC_TYPE__PLAYER_CHARACTER) && !(pSoldier.usQuoteSaidFlags & SOLDIER_QUOTE_SAID_PERSONALITY))
        {
            switch (gMercProfiles[pSoldier.ubProfile].bPersonalityTrait)
            {
                case PersonalityTrait.PSYCHO:
                    if (Globals.Random.Next(50) == 0)
                    {
                        DialogControl.TacticalCharacterDialogue(pSoldier, QUOTE.PERSONALITY_TRAIT);
                        pSoldier.usQuoteSaidFlags |= SOLDIER_QUOTE.SAID_PERSONALITY;
                    }
                    break;
                default:
                    break;
            }
        }

        UpdateForContOverPortrait(pSoldier, true);

        // Remove any interactive tiles we could be over!
        BeginCurInteractiveTileCheck(INTILE_CHECK_SELECTIVE);
    }

    public static int FindAdjacentGridEx(SOLDIERTYPE pSoldier, int sGridNo, ref WorldDirections pubDirection, out int psAdjustedGridNo, bool fForceToPerson, bool fDoor)
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
        int sClosest = NOWHERE, sSpot;
        bool sOkTest;
        int sCloseGridNo = NOWHERE;
        FIND_SOLDIER_RESPONSES uiMercFlags;
        int usSoldierIndex;
        WorldDirections ubDir;
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


        if ((sOkTest = NewOKDestination(pSoldier, sGridNo, true, pSoldier.bLevel)))    // no problem going there! nobody on it!
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
                sDistance = PathAI.PlotPath(pSoldier, sGridNo, PlotPathDefines.NO_COPYROUTE, NO_PLOT, PlotPathDefines.TEMPORARY, pSoldier.usUIMovementMode, PlotPathDefines.NOT_STEALTH, PlotPathDefines.FORWARD, pSoldier.bActionPoints);

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
            sFourGrids[cnt] = sSpot = IsometricUtils.NewGridNo(sGridNo, IsometricUtils.DirectionInc(sDirs[cnt]));

            ubTestDirection = sDirs[cnt];

            // For switches, ALLOW them to walk through walls to reach it....
            if (pDoor is not null && pDoor.fFlags.HasFlag(STRUCTUREFLAGS.SWITCH))
            {
                ubTestDirection = gOppositeDirection[ubTestDirection];
            }

            if (fDoor)
            {
                if (gubWorldMovementCosts[sSpot, (int)ubTestDirection, pSoldier.bLevel] >= TRAVELCOST.BLOCKED)
                {
                    // obstacle or wall there!
                    continue;
                }
            }
            else
            {
                // this function returns original MP cost if not a door cost
                if (PathAI.DoorTravelCost(pSoldier, sSpot, gubWorldMovementCosts[sSpot, (int)ubTestDirection, pSoldier.bLevel], false, out var _) >= TRAVELCOST.BLOCKED)
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
                if (ubWallOrientation == WallOrientation.OUTSIDE_TOP_RIGHT || ubWallOrientation == WallOrientation.INSIDE_TOP_RIGHT)
                {
                    if (sDirs[cnt] == WorldDirections.NORTH || sDirs[cnt] == WorldDirections.WEST || sDirs[cnt] == WorldDirections.SOUTH)
                    {
                        continue;
                    }
                }

                // Refuse the north and west and east directions if our orientation is top-right
                if (ubWallOrientation == WallOrientation.OUTSIDE_TOP_LEFT || ubWallOrientation == WallOrientation.INSIDE_TOP_LEFT)
                {
                    if (sDirs[cnt] == WorldDirections.NORTH || sDirs[cnt] == WorldDirections.WEST || sDirs[cnt] == WorldDirections.EAST)
                    {
                        continue;
                    }
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
                    if (pubDirection > 0)
                    {
                        (pubDirection) = SoldierControl.GetDirectionFromGridNo(sGridNo, pSoldier);
                    }
                }
                return (sSpot);
            }

            // don't store path, just measure it
            ubDir = SoldierControl.GetDirectionToGridNoFromGridNo(sSpot, sGridNo);

            if ((NewOKDestinationAndDirection(pSoldier, sSpot, ubDir, true, pSoldier.bLevel))
                && ((sDistance = PathAI.PlotPath(pSoldier, sSpot, PlotPathDefines.NO_COPYROUTE, NO_PLOT, PlotPathDefines.TEMPORARY, pSoldier.usUIMovementMode, PlotPathDefines.NOT_STEALTH, PlotPathDefines.FORWARD, pSoldier.bActionPoints)) > 0))
            {
                if (sDistance < sClosest)
                {
                    sClosest = sDistance;
                    sCloseGridNo = (int)sSpot;
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
                if (pubDirection > 0)
                {
                    // ATE: Only if we have a valid door!
                    if (pDoor is not null)
                    {
                        switch (pDoor.pDBStructureRef.pDBStructure.ubWallOrientation)
                        {
                            case WallOrientation.OUTSIDE_TOP_LEFT:
                            case WallOrientation.INSIDE_TOP_LEFT:

                                pubDirection = WorldDirections.SOUTH;
                                break;

                            case WallOrientation.OUTSIDE_TOP_RIGHT:
                            case WallOrientation.INSIDE_TOP_RIGHT:

                                pubDirection = WorldDirections.EAST;
                                break;
                        }
                    }
                }
            }
            else
            {
                // Calculate direction if our gridno is different....
                ubDir = SoldierControl.GetDirectionToGridNoFromGridNo(sCloseGridNo, sGridNo);
                if (pubDirection > 0)
                {
                    pubDirection = ubDir;
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

    public static SOLDIERTYPE? InternalReduceAttackBusyCount(int ubID, bool fCalledByAttacker, int ubTargetID)
    {
        // Strange as this may seem, this function returns a pointer to
        // the *target* in case the target has changed sides as a result
        // of being attacked
        SOLDIERTYPE? pSoldier;
        SOLDIERTYPE? pTarget;
        bool fEnterCombat = false;

        if (ubID == NOBODY)
        {
            pSoldier = null;
            pTarget = null;
        }
        else
        {
            pSoldier = MercPtrs[ubID];
            if (ubTargetID != NOBODY)
            {
                pTarget = MercPtrs[ubTargetID];
            }
            else
            {
                pTarget = null;
                // DebugMsg(TOPIC_JA2, DBG_LEVEL_3, string(">>Target ptr is null!"));
            }
        }

        if (fCalledByAttacker)
        {
            if (pSoldier is not null && Item[pSoldier.inv[InventorySlot.HANDPOS].usItem].usItemClass.HasFlag(IC.GUN))
            {
                if (pSoldier.bBulletsLeft > 0)
                {
                    return (pTarget);
                }
            }
        }

        //	if ((gTacticalStatus.uiFlags & TURNBASED) && (gTacticalStatus.uiFlags & INCOMBAT))
        //	{

        if (gTacticalStatus.ubAttackBusyCount == 0)
        {
            // ATE: We have a problem here... if testversion, report error......
            // But for all means.... DON'T wrap!
            if ((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
            {
                // DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("!!!!!!! &&&&&&& Problem with attacker busy count decrementing past 0.... preventing wrap-around."));
# if JA2BETAVERSION
                ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_BETAVERSION, L"Attack busy problem. Save, exit and send debug.txt + save file to Sir-Tech.");
#endif
            }
        }
        else
        {
            gTacticalStatus.ubAttackBusyCount--;
        }

       // DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("!!!!!!! Ending attack, attack count now %d", gTacticalStatus.ubAttackBusyCount));
        //	}

        if (gTacticalStatus.ubAttackBusyCount > 0)
        {
            return (pTarget);
        }

        if ((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED))
            && (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
        {

            // Check to see if anyone was suppressed
            if (pSoldier is not null)
            {
                HandleSuppressionFire(pSoldier.ubTargetID, ubID);
            }
            else
            {
                HandleSuppressionFire(NOBODY, ubID);
            }


            //HandleAfterShootingGuy( pSoldier, pTarget );

            // suppression fire might cause the count to be increased, so check it again
            if (gTacticalStatus.ubAttackBusyCount > 0)
            {
                //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("!!!!!!! Starting suppression, attack count now %d", gTacticalStatus.ubAttackBusyCount));
                return (pTarget);
            }
        }

        // ATE: IN MEANWHILES, we have 'combat' in realtime....
        // this is so we DON'T call freeupattacker() which will cancel
        // the AI guy's meanwhile NPC stuff.
        // OK< let's NOT do this if it was the queen attacking....
        if (Meanwhile.AreInMeanwhile() && pSoldier != null && pSoldier.ubProfile != NPCID.QUEEN)
        {
            return (pTarget);
        }

        if (pTarget is not null)
        {
            // reset # of shotgun pellets hit by
            pTarget.bNumPelletsHitBy = 0;
            // reset flag for making "ow" sound on being shot
        }

        if (pSoldier is not null)
        {
            // reset attacking hand
            pSoldier.ubAttackingHand = InventorySlot.HANDPOS;

            // if there is a valid target, and our attack was noticed
            if (pTarget is not null && (pSoldier.uiStatusFlags.HasFlag(SOLDIER.ATTACK_NOTICED)))
            {
                // stuff that only applies to when we attack
                if (pTarget.ubBodyType != SoldierBodyTypes.CROW)
                {
                    if (pSoldier.bTeam == gbPlayerNum)
                    {
                        fEnterCombat = ProcessImplicationsOfPCAttack(pSoldier, out pTarget, REASON_NORMAL_ATTACK);
                        if (!fEnterCombat)
                        {
                            // DebugMsg(TOPIC_JA2, DBG_LEVEL_3, ">>Not entering combat as a result of PC attack");
                        }
                    }
                }

                // global

                // ATE: If we are an animal, etc, don't change to hostile... 
                // ( and don't go into combat )
                if (pTarget.ubBodyType == SoldierBodyTypes.CROW)
                {
                    // Loop through our team, make guys who can see this fly away....
                    {
                        int cnt;
                        SOLDIERTYPE pTeamSoldier;
                        TEAM ubTeam;

                        ubTeam = pTarget.bTeam;

                        for (cnt = gTacticalStatus.Team[ubTeam].bFirstID, pTeamSoldier = MercPtrs[cnt]; cnt <= gTacticalStatus.Team[ubTeam].bLastID; cnt++, pTeamSoldier++)
                        {
                            if (pTeamSoldier.bActive && pTeamSoldier.bInSector)
                            {
                                if (pTeamSoldier.ubBodyType == SoldierBodyTypes.CROW)
                                {
                                    if (pTeamSoldier.bOppList[pSoldier.ubID] == SEEN_CURRENTLY)
                                    {
                                        //ZEROTIMECOUNTER( pTeamSoldier.AICounter );		

                                        //MakeCivHostile( pTeamSoldier, 2 );

                                        HandleCrowFlyAway(pTeamSoldier);

                                    }
                                }
                            }
                        }
                    }

                    // Don't enter combat...
                    fEnterCombat = false;
                }

                if (gTacticalStatus.bBoxingState == BoxingStates.BOXING)
                {
                    if (pTarget is not null && pTarget.bLife <= 0)
                    {
                        // someone has won!
                        EndBoxingMatch(pTarget);
                    }
                }


                // if soldier and target were not both players and target was not under fire before...
                if ((pSoldier.bTeam != gbPlayerNum || pTarget.bTeam != gbPlayerNum))
                {
                    if (pTarget.bOppList[pSoldier.ubID] != SEEN_CURRENTLY)
                    {
                        OppList.NoticeUnseenAttacker(pSoldier, pTarget, 0);
                    }
                    // "under fire" lasts for 2 turns
                    pTarget.bUnderFire = 2;
                }

            }
            else if (pTarget is not null)
            {
                // something is wrong here!
                if (!pTarget.bActive || !pTarget.bInSector)
                {
                    //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, ">>Invalid target attacked!");
                }
                else if (!(pSoldier.uiStatusFlags.HasFlag(SOLDIER.ATTACK_NOTICED)))
                {
                    // DebugMsg(TOPIC_JA2, DBG_LEVEL_3, ">>Attack not noticed");
                }
            }
            else
            {
                // no target, don't enter combat
                fEnterCombat = false;
            }

            if (pSoldier.fSayAmmoQuotePending)
            {
                pSoldier.fSayAmmoQuotePending = false;
                DialogControl.TacticalCharacterDialogue(pSoldier, QUOTE.OUT_OF_AMMO);
            }

            if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.PC))
            {
                HandleUI.UnSetUIBusy(ubID);
            }
            else
            {
                AIMain.FreeUpNPCFromAttacking(ubID);
            }

            if (!fEnterCombat)
            {
                //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, ">>Not to enter combat from this attack");
            }


            if (fEnterCombat && !(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
            {
                // Go into combat!

                // If we are in a meanwhile... don't enter combat here...
                if (!AreInMeanwhile())
                {
                    EnterCombatMode(pSoldier.bTeam);
                }
            }

            pSoldier.uiStatusFlags &= (~SOLDIER.ATTACK_NOTICED);
        }

        if (gTacticalStatus.fKilledEnemyOnAttack)
        {
            // Check for death quote...
            HandleKilledQuote(MercPtrs[gTacticalStatus.ubEnemyKilledOnAttack], MercPtrs[gTacticalStatus.ubEnemyKilledOnAttackKiller], gTacticalStatus.ubEnemyKilledOnAttackLocation, gTacticalStatus.bEnemyKilledOnAttackLevel);
            gTacticalStatus.fKilledEnemyOnAttack = false;
        }

        // ATE: Check for stat changes....
        Campaign.HandleAnyStatChangesAfterAttack();


        if (gTacticalStatus.fItemsSeenOnAttack && gTacticalStatus.ubCurrentTeam == gbPlayerNum)
        {
            gTacticalStatus.fItemsSeenOnAttack = false;

            // Display quote!
            if (!AM_AN_EPC(MercPtrs[gTacticalStatus.ubItemsSeenOnAttackSoldier]))
            {
                DialogControl.TacticalCharacterDialogueWithSpecialEvent(MercPtrs[gTacticalStatus.ubItemsSeenOnAttackSoldier], (QUOTE.SPOTTED_SOMETHING_ONE + Globals.Random.Next(2)), DIALOGUE_SPECIAL_EVENT.SIGNAL_ITEM_LOCATOR_START, gTacticalStatus.usItemsSeenOnAttackGridNo, 0);
            }
            else
            {
                // Turn off item lock for locators...
                gTacticalStatus.fLockItemLocators = false;
                // Slide to location!
                SlideToLocation(0, gTacticalStatus.usItemsSeenOnAttackGridNo);
            }
        }

        if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.CHECK_SIGHT_AT_END_OF_ATTACK))
        {
            int ubLoop;
            SOLDIERTYPE? pSightSoldier;

            AllTeamsLookForAll(false);

            // call fov code
            ubLoop = gTacticalStatus.Team[gbPlayerNum].bFirstID;
            for (pSightSoldier = MercPtrs[ubLoop]; ubLoop <= gTacticalStatus.Team[gbPlayerNum].bLastID; ubLoop++, pSightSoldier++)
            {
                if (pSightSoldier.bActive && pSightSoldier.bInSector)
                {
                    RevealRoofsAndItems(pSightSoldier, true, false, pSightSoldier.bLevel, false);
                }
            }

            gTacticalStatus.uiFlags &= ~TacticalEngineStatus.CHECK_SIGHT_AT_END_OF_ATTACK;
        }

        DequeueAllDemandGameEvents(true);

        CheckForEndOfBattle(false);

        // if we're in realtime, turn off the attacker's muzzle flash at this point
        if (!(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)) && pSoldier is not null)
        {
            OppList.EndMuzzleFlash(pSoldier);
        }

        if (pSoldier is not null && pSoldier.bWeaponMode == WM.ATTACHED)
        {
            // change back to single shot
            pSoldier.bWeaponMode = WM.NORMAL;
        }

        // record last target
        // Check for valid target!
        if (pSoldier is not null)
        {
            pSoldier.sLastTarget = pSoldier.sTargetGridNo;
        }

        return (pTarget);
    }

    // internal function for turning neutral to FALSE
    public static void SetSoldierNonNeutral(SOLDIERTYPE pSoldier)
    {
        pSoldier.bNeutral = 0;

        if (gTacticalStatus.bBoxingState == BoxingStates.NOT_BOXING)
        {
            // Special code for strategic implications
            CalculateNonPersistantPBIInfo();
        }
    }


    public static void SlideTo(int sGridno, int usSoldierID, int usReasonID, int fSetLocator)
    {
        int cnt;


        if (usSoldierID == NOBODY)
        {
            return;
        }

        if (fSetLocator == SETANDREMOVEPREVIOUSLOCATOR)
        {
            for (cnt = 0; cnt < TOTAL_SOLDIERS; cnt++)
            {
                if (MercPtrs[cnt].bActive && MercPtrs[cnt].bInSector)
                {
                    // Remove all existing locators...
                    MercPtrs[cnt].fFlashLocator = false;
                }
            }
        }

        // Locate even if on screen
        if (fSetLocator > 0)
        {
            InterfacePanel.ShowRadioLocator(usSoldierID, SHOW_LOCATOR.NORMAL);
        }

        // FIRST CHECK IF WE ARE ON SCREEN
        if (SoldierFind.GridNoOnScreen(MercPtrs[usSoldierID].sGridNo))
        {
            return;
        }

        // sGridNo here for DG compatibility
        gTacticalStatus.sSlideTarget = MercPtrs[usSoldierID].sGridNo;
        gTacticalStatus.sSlideReason = usReasonID;

        // Plot new path!
        gfPlotNewMovement = true;
    }


    public static void SlideToLocation(int usReasonID, int sDestGridNo)
    {
        if (sDestGridNo == NOWHERE)
        {
            return;
        }

        // FIRST CHECK IF WE ARE ON SCREEN
        if (SoldierFind.GridNoOnScreen(sDestGridNo))
        {
            return;
        }

        // sGridNo here for DG compatibility
        gTacticalStatus.sSlideTarget = sDestGridNo;
        gTacticalStatus.sSlideReason = usReasonID;

        // Plot new path!
        gfPlotNewMovement = true;
    }


    void RebuildAllSoldierShadeTables()
    {
        int cnt;

        // Loop through all mercs and make go
        foreach (var pSoldier in Menptr)//, cnt = 0; cnt < TOTAL_SOLDIERS; pSoldier++, cnt++)
        {
            if (pSoldier.bActive)
            {
                SoldierControl.CreateSoldierPalettes(pSoldier);
            }
        }

    }

    public static SOLDIERTYPE? ReduceAttackBusyCount(int ubID, bool fCalledByAttacker)
    {
        if (ubID == NOBODY)
        {
            return (InternalReduceAttackBusyCount(ubID, fCalledByAttacker, NOBODY));
        }
        else
        {
            return (InternalReduceAttackBusyCount(ubID, fCalledByAttacker, MercPtrs[ubID].ubTargetID));
        }
    }

    // NB if making changes don't forget to update NewOKDestinationAndDirection
    public static bool NewOKDestination(SOLDIERTYPE pCurrSoldier, int sGridNo, bool fPeopleToo, int bLevel)
    {
        int bPerson;
        STRUCTURE? pStructure;
        STRUCTURE_ON sDesiredLevel;
        bool fOKCheckStruct;

        if (!IsometricUtils.GridNoOnVisibleWorldTile(sGridNo))
        {
            return (true);
        }

        if (fPeopleToo && (bPerson = WorldManager.WhoIsThere2(sGridNo, bLevel)) != NO_SOLDIER)
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
            pStructureFileRef = AnimationData.GetAnimationStructureRef(pCurrSoldier.ubID, usAnimSurface, pCurrSoldier.usUIMovementMode);

            // opposite directions should be mirrors, so only check 4
            if (pStructureFileRef is not null)
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

                    fOk = StructureInternals.InternalOkayToAddStructureToWorld(sGridNo, bLevel, (pStructureFileRef.pDBStructureRef[bLoop]), usStructureID, (bool)!fPeopleToo);
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

                    pStructure = StructureInternals.FindNextStructure(pStructure, STRUCTUREFLAGS.BLOCKSMOVES);
                }
            }
        }

        return (true);
    }

    public static void HandleSuppressionFire(int ubTargetedMerc, int ubCausedAttacker)
    {
        int bTolerance;
        int sClosestOpponent, sClosestOppLoc;
        int ubPointsLost, ubTotalPointsLost;
        AnimationHeights ubNewStance;
        int uiLoop;
        int ubLoop2;
        SOLDIERTYPE pSoldier;

        for (uiLoop = 0; uiLoop < guiNumMercSlots; uiLoop++)
        {
            pSoldier = MercSlots[uiLoop];
            if (IS_MERC_BODY_TYPE(pSoldier) && pSoldier.bLife >= OKLIFE && pSoldier.ubSuppressionPoints > 0)
            {
                bTolerance = CalcSuppressionTolerance(pSoldier);

                // multiply by 2, add 1 and divide by 2 to round off to nearest whole number
                ubPointsLost = (((pSoldier.ubSuppressionPoints * 6) / (bTolerance + 6)) * 2 + 1) / 2;

                // reduce loss of APs based on stance
                // ATE: Taken out because we can possibly supress ourselves...
                //switch (gAnimControl[ pSoldier.usAnimState ].ubEndHeight)
                //{
                //	case ANIM_PRONE:
                //		ubPointsLost = ubPointsLost * 2 / 4;
                //		break;
                //	case ANIM_CROUCH:
                //		ubPointsLost = ubPointsLost * 3 / 4;
                //		break;
                //	default:
                //		break;
                //}

                // cap the # of APs we can lose
                if (ubPointsLost > MAX_APS_SUPPRESSED)
                {
                    ubPointsLost = MAX_APS_SUPPRESSED;
                }

                ubTotalPointsLost = ubPointsLost;

                // Subtract off the APs lost before this point to find out how many points are lost now
                if (pSoldier.ubAPsLostToSuppression >= ubPointsLost)
                {
                    continue;
                }

                // morale modifier
                if (ubTotalPointsLost / 2 > pSoldier.ubAPsLostToSuppression / 2)
                {
                    for (ubLoop2 = 0; ubLoop2 < (ubTotalPointsLost / 2) - (pSoldier.ubAPsLostToSuppression / 2); ubLoop2++)
                    {
                        Morale.HandleMoraleEvent(pSoldier, MoraleEventNames.MORALE_SUPPRESSED, pSoldier.sSectorX, pSoldier.sSectorY, pSoldier.bSectorZ);
                    }
                }

                ubPointsLost -= pSoldier.ubAPsLostToSuppression;
                ubNewStance = 0;

                // merc may get to react
                if (pSoldier.ubSuppressionPoints >= (130 / (6 + bTolerance)))
                {
                    // merc gets to use APs to react!
                    switch (gAnimControl[pSoldier.usAnimState].ubEndHeight)
                    {
                        case AnimationHeights.ANIM_PRONE:
                            // can't change stance below prone!
                            break;
                        case AnimationHeights.ANIM_CROUCH:
                            if (ubTotalPointsLost >= AP.PRONE && SoldierControl.IsValidStance(pSoldier, AnimationHeights.ANIM_PRONE))
                            {
                                sClosestOpponent = AIUtils.ClosestKnownOpponent(pSoldier, out sClosestOppLoc, out var _);
                                if (sClosestOpponent == NOWHERE || IsometricUtils.SpacesAway(pSoldier.sGridNo, sClosestOppLoc) > 8)
                                {
                                    if (ubPointsLost < AP.PRONE)
                                    {
                                        // Have to give APs back so that we can change stance without
                                        // losing more APs
                                        pSoldier.bActionPoints += (AP.PRONE - ubPointsLost);
                                        ubPointsLost = 0;
                                    }
                                    else
                                    {
                                        ubPointsLost -= AP.PRONE;
                                    }
                                    ubNewStance = AnimationHeights.ANIM_PRONE;
                                }
                            }
                            break;
                        default: // standing!
                            if (pSoldier.bOverTerrainType == TerrainTypeDefines.LOW_WATER || pSoldier.bOverTerrainType == TerrainTypeDefines.DEEP_WATER)
                            {
                                // can't change stance here!
                                break;
                            }
                            else if (ubTotalPointsLost >= (AP.CROUCH + AP.PRONE) && (gAnimControl[pSoldier.usAnimState].ubEndHeight != AnimationHeights.ANIM_PRONE) && SoldierControl.IsValidStance(pSoldier, AnimationHeights.ANIM_PRONE))
                            {
                                sClosestOpponent = AIUtils.ClosestKnownOpponent(pSoldier, out sClosestOppLoc, out var _);
                                if (sClosestOpponent == NOWHERE || IsometricUtils.SpacesAway(pSoldier.sGridNo, sClosestOppLoc) > 8)
                                {
                                    if (gAnimControl[pSoldier.usAnimState].ubEndHeight == AnimationHeights.ANIM_STAND)
                                    {
                                        // can only crouch for now
                                        ubNewStance = AnimationHeights.ANIM_CROUCH;
                                    }
                                    else
                                    {
                                        // lie prone!
                                        ubNewStance = AnimationHeights.ANIM_PRONE;
                                    }
                                }
                                else if (gAnimControl[pSoldier.usAnimState].ubEndHeight == AnimationHeights.ANIM_STAND && SoldierControl.IsValidStance(pSoldier, AnimationHeights.ANIM_CROUCH))
                                {
                                    // crouch, at least!
                                    ubNewStance = AnimationHeights.ANIM_CROUCH;
                                }
                            }
                            else if (ubTotalPointsLost >= AP.CROUCH && (gAnimControl[pSoldier.usAnimState].ubEndHeight != AnimationHeights.ANIM_CROUCH) && SoldierControl.IsValidStance(pSoldier, AnimationHeights.ANIM_CROUCH))
                            {
                                // crouch!
                                ubNewStance = AnimationHeights.ANIM_CROUCH;
                            }
                            break;
                    }
                }

                // Reduce action points!
                pSoldier.bActionPoints -= ubPointsLost;
                pSoldier.ubAPsLostToSuppression = ubTotalPointsLost;

                if ((pSoldier.uiStatusFlags.HasFlag(SOLDIER.PC)) && (pSoldier.ubSuppressionPoints > 8) && (pSoldier.ubID == ubTargetedMerc))
                {
                    if (!(pSoldier.usQuoteSaidFlags.HasFlag(SOLDIER_QUOTE.SAID_BEING_PUMMELED)))
                    {
                        pSoldier.usQuoteSaidFlags |= SOLDIER_QUOTE.SAID_BEING_PUMMELED;
                        // say we're under heavy fire!

                        // ATE: For some reason, we forgot #53!
                        if (pSoldier.ubProfile != (NPCID)53)
                        {
                            DialogControl.TacticalCharacterDialogue(pSoldier, QUOTE.UNDER_HEAVY_FIRE);
                        }
                    }
                }

                if (ubNewStance != 0)
                {
                    // This person is going to change stance

                    // This person will be busy while they crouch or go prone
                    if ((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED)) && (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
                    {
                       // DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("!!!!!!! Starting suppression, on %d", pSoldier.ubID));

                        gTacticalStatus.ubAttackBusyCount++;

                        // make sure supressor ID is the same!
                        pSoldier.ubSuppressorID = ubCausedAttacker;
                    }
                    pSoldier.fChangingStanceDueToSuppression = true;
                    pSoldier.fDontChargeAPsForStanceChange = true;

                    // AI people will have to have their actions cancelled
                    if (!(pSoldier.uiStatusFlags.HasFlag(SOLDIER.PC)))
                    {
                        AIMain.CancelAIAction(pSoldier, 1);
                        pSoldier.bAction = AI_ACTION.CHANGE_STANCE;
                        pSoldier.usActionData = ubNewStance;
                        pSoldier.bActionInProgress = 1;
                    }

                    // go for it!
                    // ATE: Cancel any PENDING ANIMATIONS...
                    pSoldier.usPendingAnimation = NO_PENDING_ANIMATION;
                    // ATE: Turn off non-interrupt flag ( this NEEDS to be done! )
                    pSoldier.fInNonintAnim = false;
                    pSoldier.fRTInNonintAnim = false;

                    SoldierControl.ChangeSoldierStance(pSoldier, ubNewStance);
                }

            } // end of examining one soldier
        } // end of loop

    }

    public static int CalcSuppressionTolerance(SOLDIERTYPE pSoldier)
    {
        int bTolerance;

        // Calculate basic tolerance value
        bTolerance = pSoldier.bExpLevel * 2;
        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.PC))
        {
            // give +1 for every 10% morale from 50, for a maximum bonus/penalty of 5.
            bTolerance += (pSoldier.bMorale - 50) / 10;
        }
        else
        {
            // give +2 for every morale category from normal, for a max change of 4 
            bTolerance += (pSoldier.bAIMorale - MORALE.NORMAL) * 2;
        }

        if (pSoldier.ubProfile != NO_PROFILE)
        {
            // change tolerance based on attitude
            switch (gMercProfiles[pSoldier.ubProfile].bAttitude)
            {
                case ATT.AGGRESSIVE:
                    bTolerance += 2;
                    break;
                case ATT.COWARD:
                    bTolerance += -2;
                    break;
                default:
                    break;
            }
        }
        else
        {
            // generic NPC/civvie; change tolerance based on attitude
            switch (pSoldier.bAttitude)
            {
                case Attitudes.BRAVESOLO:
                case Attitudes.BRAVEAID:
                    bTolerance += 2;
                    break;
                case Attitudes.AGGRESSIVE:
                    bTolerance += 1;
                    break;
                case Attitudes.DEFENSIVE:
                    bTolerance += -1;
                    break;
                default:
                    break;
            }
        }

        if (bTolerance < 0)
        {
            bTolerance = 0;
        }

        return (bTolerance);
    }

    public static bool FlatRoofAboveGridNo(int iMapIndex)
    {
        LEVELNODE? pRoof;
        TileTypeDefines uiTileType;
        pRoof = gpWorldLevelData[iMapIndex].pRoofHead;
        while (pRoof is not null)
        {
            if (pRoof.usIndex != TileIndexes.NO_TILE)
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
    public int ubItemsSeenOnAttackSoldier;
    public bool fBeenInCombatOnce;
    public bool fSaidCreatureSmellQuote;
    public int usItemsSeenOnAttackGridNo;
    public bool fLockItemLocators;
    public QUOTE ubLastQuoteSaid;
    public NPCID ubLastQuoteProfileNUm;
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
    public int[] bNumFoughtInBattle = new int[(int)Globals.MAXTEAMS];
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
    public bool IsTeamActive => bTeamActive > 0;
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
