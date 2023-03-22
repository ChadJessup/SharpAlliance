using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.SubSystems;
using static SharpAlliance.Core.Globals;

using static System.Math;
using SharpAlliance.Core.Screens;
using System.IO;

namespace SharpAlliance.Core;

public class SoldierControl
{
    // HUALT EVENT IS USED TO STOP A MERC - NETWORKING SHOULD CHECK / ADJUST TO GRIDNO?
    public static void EVENT_StopMerc(SOLDIERTYPE? pSoldier, int sGridNo, WorldDirections bDirection)
    {
        int sX, sY;

        // MOVE GUY TO GRIDNO--- SHOULD BE THE SAME UNLESS IN MULTIPLAYER
        // Makesure center of tile
        sX = IsometricUtils.CenterX(sGridNo);
        sY = IsometricUtils.CenterY(sGridNo);


        //Cancel pending events
        if (pSoldier.fDelayedMovement == 0)
        {
            pSoldier.usPendingAnimation = Globals.NO_PENDING_ANIMATION;
            pSoldier.usPendingAnimation2 = Globals.NO_PENDING_ANIMATION;
            pSoldier.ubPendingDirection = NO_PENDING_DIRECTION;
            pSoldier.ubPendingAction = MERC.NO_PENDING_ACTION;
        }

        pSoldier.bEndDoorOpenCode = 0;
        pSoldier.fTurningFromPronePosition = false;

        // Cancel path data!
        pSoldier.usPathIndex = pSoldier.usPathDataSize = 0;

        // Set ext tile waiting flag off!
        pSoldier.fDelayedMovement = 0;

        // Turn off reverse...
        pSoldier.bReverse = false;

        EVENT_SetSoldierPosition(pSoldier, (float)sX, (float)sY);
        pSoldier.sDestXPos = (int)pSoldier.dXPos;
        pSoldier.sDestYPos = (int)pSoldier.dYPos;
        EVENT_SetSoldierDirection(pSoldier, bDirection);

        if (gAnimControl[pSoldier.usAnimState].uiFlags.HasFlag(ANIM.MOVING))
        {
            SoldierGotoStationaryStance(pSoldier);
        }

        // ATE; IF turning to shoot, stop!
        if (pSoldier.fTurningToShoot)
        {
            pSoldier.fTurningToShoot = false;
            // Release attacker
            // DebugMsg(TOPIC_JA2, DBG_LEVEL_3, string.Format("@@@@@@@ Reducing attacker busy count..., ending fire because saw something"));
            ReduceAttackBusyCount(pSoldier.ubID, false);
        }

        // Turn off multi-move speed override....
        if (pSoldier.sGridNo == pSoldier.sFinalDestination)
        {
            pSoldier.fUseMoverrideMoveSpeed = false;
        }

        // Unset UI!
        UnSetUIBusy(pSoldier.ubID);

        UnMarkMovementReserved(pSoldier);
    }

    // FUNCTIONS CALLED BY EVENT PUMP
    /////////////////////////////////

    public static bool ChangeSoldierState(SOLDIERTYPE? pSoldier, AnimationStates usNewState, int usStartingAniCode, bool fForce)
    {
        EV_S_CHANGESTATE SChangeState;

        // Send message that we have changed states
        SChangeState.usNewState = usNewState;
        SChangeState.usSoldierID = pSoldier.ubID;
        SChangeState.uiUniqueId = pSoldier.uiUniqueSoldierIdValue;
        SChangeState.usStartingAniCode = usStartingAniCode;
        SChangeState.sXPos = pSoldier.sX;
        SChangeState.sYPos = pSoldier.sY;
        SChangeState.fForce = fForce;
        SChangeState.uiUniqueId = pSoldier.uiUniqueSoldierIdValue;

        //AddGameEvent( S_CHANGESTATE, 0, &SChangeState );
        EVENT_InitNewSoldierAnim(pSoldier, SChangeState.usNewState, SChangeState.usStartingAniCode, SChangeState.fForce);


        return (true);
    }


    // This function reevaluates the stance if the guy sees us!
    public static bool ReevaluateEnemyStance(SOLDIERTYPE? pSoldier, AnimationStates usAnimState)
    {
        int cnt, iClosestEnemy = NOBODY;
        int sTargetXPos, sTargetYPos;
        bool fReturnVal = false;
        int sDist, sClosestDist = 10000;

        // make the chosen one not turn to face us
        if (OK_ENEMY_MERC(pSoldier) && pSoldier.ubID != gTacticalStatus.ubTheChosenOne && gAnimControl[usAnimState].ubEndHeight == AnimationHeights.ANIM_STAND && !(pSoldier.uiStatusFlags.HasFlag(SOLDIER.UNDERAICONTROL)))
        {
            if (pSoldier.fTurningFromPronePosition == TURNING_FROM_PRONE_OFF)
            {
                // If we are a queen and see enemies, goto ready
                if (pSoldier.ubBodyType == SoldierBodyTypes.QUEENMONSTER)
                {
                    if (gAnimControl[usAnimState].uiFlags.HasFlag(ANIM.BREATH))
                    {
                        if (pSoldier.bOppCnt > 0)
                        {
                            EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.QUEEN_INTO_READY, 0, true);
                            return (true);
                        }
                    }
                }

                // ATE: Don't do this if we're not a merc.....
                if (!IS_MERC_BODY_TYPE(pSoldier))
                {
                    return (false);
                }

                if (gAnimControl[usAnimState].uiFlags.HasFlag(ANIM.MERCIDLE | ANIM.BREATH))
                {
                    if (pSoldier.bOppCnt > 0)
                    {
                        // Pick a guy this buddy sees and turn towards them!
                        for (cnt = gTacticalStatus.Team[OUR_TEAM].bFirstID; cnt <= gTacticalStatus.Team[OUR_TEAM].bLastID; cnt++)
                        {
                            if (pSoldier.bOppList[cnt] == SEEN_CURRENTLY)
                            {
                                sDist = IsometricUtils.PythSpacesAway(pSoldier.sGridNo, MercPtrs[cnt].sGridNo);
                                if (sDist < sClosestDist)
                                {
                                    sClosestDist = sDist;
                                    iClosestEnemy = cnt;
                                }
                            }
                        }

                        if (iClosestEnemy != NOBODY)
                        {
                            // Change to fire ready animation
                            IsometricUtils.ConvertGridNoToXY(MercPtrs[iClosestEnemy].sGridNo, out sTargetXPos, out sTargetYPos);

                            pSoldier.fDontChargeReadyAPs = true;

                            // Ready weapon
                            fReturnVal = SoldierReadyWeapon(pSoldier, sTargetXPos, sTargetYPos, false);

                            return (fReturnVal);
                        }

                    }
                }
            }
        }
        return (false);

    }


    public static void CheckForFreeupFromHit(SOLDIERTYPE? pSoldier, int uiOldAnimFlags, int uiNewAnimFlags, int usOldAniState, int usNewState)
    {
        // THIS COULD POTENTIALLY CALL EVENT_INITNEWAnim() if the GUY was SUPPRESSED
        // CHECK IF THE OLD ANIMATION WAS A HIT START THAT WAS NOT FOLLOWED BY A HIT FINISH
        // IF SO, RELEASE ATTACKER FROM ATTACKING

        // If old and new animations are the same, do nothing!
        if (usOldAniState == QUEEN_HIT && usNewState == QUEEN_HIT)
        {
            return;
        }

        if (usOldAniState != usNewState && (uiOldAnimFlags & ANIM.HITSTART) && !(uiNewAnimFlags & ANIM.HITFINISH) && !(uiNewAnimFlags & ANIM.IGNOREHITFINISH) && !(pSoldier.uiStatusFlags.HasFlag(SOLDIER.TURNINGFROMHIT)))
        {
            // Release attacker
            //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("@@@@@@@ Releasesoldierattacker, normal hit animation ended NEW: %s ( %d ) OLD: %s ( %d )", gAnimControl[usNewState].zAnimStr, usNewState, gAnimControl[usOldAniState].zAnimStr, pSoldier.usOldAniState));
            ReleaseSoldiersAttacker(pSoldier);

            //FREEUP GETTING HIT FLAG
            pSoldier.fGettingHit = false;

            // ATE: if our guy, have 10% change of say damn, if still conscious...
            if (pSoldier.bTeam == gbPlayerNum && pSoldier.bLife >= OKLIFE)
            {
                if (Globals.Random.Next(10) == 0)
                {
                    DoMercBattleSound(pSoldier, (BATTLE_SOUND_CURSE1));
                }
            }
        }

        // CHECK IF WE HAVE FINSIHED A HIT WHILE DOWN
        // OBLY DO THIS IF 1 ) We are dead already or 2 ) We are alive still
        if ((uiOldAnimFlags & ANIM.HITWHENDOWN) && ((pSoldier.uiStatusFlags.HasFlag(SOLDIER.DEAD)) || pSoldier.IsAlive))
        {
            // Release attacker
            // DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("@@@@@@@ Releasesoldierattacker, animation of kill on the ground ended"));
            ReleaseSoldiersAttacker(pSoldier);

            //FREEUP GETTING HIT FLAG
            pSoldier.fGettingHit = false;

            if (pSoldier.bLife == 0)
            {
                //ATE: Set previous attacker's value!
                // This is so that the killer can say their killed quote....
                pSoldier.ubAttackerID = pSoldier.ubPreviousAttackerID;
            }
        }
    }


    // THIS IS CALLED FROM AN EVENT ( S_CHANGESTATE )!
    public static bool EVENT_InitNewSoldierAnim(SOLDIERTYPE? pSoldier, AnimationStates usNewState, int usStartingAniCode, bool fForce)
    {
        int usNewGridNo = 0;
        int sAPCost = 0;
        int sBPCost = 0;
        int uiOldAnimFlags;
        int uiNewAnimFlags;
        int usSubState;
        Items usItem;
        bool fTryingToRestart = false;

        CHECKF(usNewState < AnimationStates.NUMANIMATIONSTATES);


        ///////////////////////////////////////////////////////////////////////
        //			DO SOME CHECKS ON OUR NEW ANIMATION!
        /////////////////////////////////////////////////////////////////////

        // If we are NOT loading a game, continue normally
        if (!(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.LOADING_SAVED_GAME)))
        {
            // CHECK IF WE ARE TRYING TO INTURRUPT A SCRIPT WHICH WE DO NOT WANT INTERRUPTED!
            if (pSoldier.fInNonintAnim)
            {
                return (false);
            }

            if (pSoldier.fRTInNonintAnim)
            {
                if (!(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
                {
                    return (false);
                }
                else
                {
                    pSoldier.fRTInNonintAnim = false;
                }
            }


            // Check if we can restart this animation if it's the same as our current!
            if (usNewState == pSoldier.usAnimState)
            {
                if ((gAnimControl[pSoldier.usAnimState].uiFlags.HasFlag(ANIM.NORESTART))
                    && !fForce)
                {
                    fTryingToRestart = true;
                }
            }

            // Check state, if we are not at the same height, set this ani as the pending one and
            // change stance accordingly
            // ATE: ONLY IF WE ARE STARTING AT START OF ANIMATION!
            if (usStartingAniCode == 0)
            {
                if (gAnimControl[usNewState].ubHeight != gAnimControl[pSoldier.usAnimState].ubEndHeight &&
                         !(gAnimControl[usNewState].uiFlags.HasFlag(ANIM.STANCECHANGEANIM | ANIM.IGNORE_AUTOSTANCE)))
                {

                    // Check if we are going from crouched height to prone height, and adjust fast turning accordingly
                    // Make guy turn while crouched THEN go into prone
                    if ((gAnimControl[usNewState].ubEndHeight == AnimationHeights.ANIM_PRONE
                        && gAnimControl[pSoldier.usAnimState].ubEndHeight == AnimationHeights.ANIM_CROUCH)
                        && !(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
                    {
                        pSoldier.fTurningUntilDone = true;
                        pSoldier.ubPendingStanceChange = gAnimControl[usNewState].ubEndHeight;
                        pSoldier.usPendingAnimation = usNewState;
                        return (true);
                    }
                    // Check if we are in realtime and we are going from stand to crouch
                    else if (gAnimControl[usNewState].ubEndHeight == AnimationHeights.ANIM_CROUCH
                        && gAnimControl[pSoldier.usAnimState].ubEndHeight == AnimationHeights.ANIM_STAND
                        && (gAnimControl[pSoldier.usAnimState].uiFlags.HasFlag(ANIM.MOVING))
                        && ((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.REALTIME))
                        || !(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))))
                    {
                        pSoldier.ubDesiredHeight = gAnimControl[usNewState].ubEndHeight;
                        // Continue with this course of action IE: Do animation and skip from stand to crouch
                    }
                    // Check if we are in realtime and we are going from crouch to stand
                    else if (gAnimControl[usNewState].ubEndHeight == AnimationHeights.ANIM_STAND
                        && gAnimControl[pSoldier.usAnimState].ubEndHeight == AnimationHeights.ANIM_CROUCH
                        && (gAnimControl[pSoldier.usAnimState].uiFlags.HasFlag(ANIM.MOVING))
                        && ((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.REALTIME))
                        || !(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
                        && pSoldier.usAnimState != AnimationStates.HELIDROP)
                    {
                        pSoldier.ubDesiredHeight = gAnimControl[usNewState].ubEndHeight;
                        // Continue with this course of action IE: Do animation and skip from stand to crouch
                    }
                    else
                    {
                        // ONLY DO FOR EVERYONE BUT PLANNING GUYS
                        if (pSoldier.ubID < MAX_NUM_SOLDIERS)
                        {
                            // Set our next moving animation to be pending, after
                            pSoldier.usPendingAnimation = usNewState;
                            // Set new state to be animation to move to new stance
                            SendChangeSoldierStanceEvent(pSoldier, gAnimControl[usNewState].ubHeight);
                            return (true);
                        }
                    }
                }
            }

            if (usNewState == AnimationStates.ADJACENT_GET_ITEM)
            {
                if (pSoldier.ubPendingDirection != NO_PENDING_DIRECTION)
                {
                    EVENT_InternalSetSoldierDesiredDirection(pSoldier, pSoldier.ubPendingDirection, false, pSoldier.usAnimState);
                    pSoldier.ubPendingDirection = NO_PENDING_DIRECTION;
                    pSoldier.usPendingAnimation = AnimationStates.ADJACENT_GET_ITEM;
                    pSoldier.fTurningUntilDone = true;
                    SoldierGotoStationaryStance(pSoldier);
                    return (true);
                }
            }


            if (usNewState == AnimationStates.CLIMBUPROOF)
            {
                if (pSoldier.ubPendingDirection != NO_PENDING_DIRECTION)
                {
                    EVENT_SetSoldierDesiredDirection(pSoldier, pSoldier.ubPendingDirection);
                    pSoldier.ubPendingDirection = NO_PENDING_DIRECTION;
                    pSoldier.usPendingAnimation = AnimationStates.CLIMBUPROOF;
                    pSoldier.fTurningUntilDone = true;
                    SoldierGotoStationaryStance(pSoldier);
                    return (true);
                }
            }

            if (usNewState == AnimationStates.CLIMBDOWNROOF)
            {
                if (pSoldier.ubPendingDirection != NO_PENDING_DIRECTION)
                {
                    EVENT_SetSoldierDesiredDirection(pSoldier, pSoldier.ubPendingDirection);
                    pSoldier.ubPendingDirection = NO_PENDING_DIRECTION;
                    pSoldier.usPendingAnimation = AnimationStates.CLIMBDOWNROOF;
                    pSoldier.fTurningFromPronePosition = false;
                    pSoldier.fTurningUntilDone = true;
                    SoldierGotoStationaryStance(pSoldier);
                    return (true);
                }
            }

            // ATE: Don't raise/lower automatically if we are low on health,
            // as our gun looks lowered anyway....
            //if ( pSoldier.bLife > INJURED_CHANGE_THREASHOLD )
            {
                // Don't do some of this if we are a monster!
                // ATE: LOWER AIMATION IS GOOD, RAISE ONE HOWEVER MAY CAUSE PROBLEMS FOR AI....
                if (!(pSoldier.uiStatusFlags.HasFlag(SOLDIER.MONSTER))
                    && pSoldier.ubBodyType != SoldierBodyTypes.ROBOTNOWEAPON
                    && pSoldier.bTeam == gbPlayerNum)
                {
                    // If this animation is a raise_weapon animation
                    if ((gAnimControl[usNewState].uiFlags.HasFlag(ANIM.RAISE_WEAPON))
                        && !(gAnimControl[pSoldier.usAnimState].uiFlags.HasFlag(ANIM.RAISE_WEAPON | ANIM.NOCHANGE_WEAPON)))
                    {
                        // We are told that we need to rasie weapon
                        // Do so only if
                        // 1) We have a rifle in hand...
                        usItem = pSoldier.inv[InventorySlot.HANDPOS].usItem;

                        if (usItem != NOTHING && (Item[usItem].fFlags & ITEM_TWO_HANDED) && usItem != ROCKET_LAUNCHER)
                        {
                            // Switch on height!
                            switch (gAnimControl[pSoldier.usAnimState].ubEndHeight)
                            {
                                case AnimationHeights.ANIM_STAND:

                                    // 2) OK, all's fine... lower weapon first....
                                    pSoldier.usPendingAnimation = usNewState;
                                    // Set new state to be animation to move to new stance
                                    usNewState = AnimationStates.RAISE_RIFLE;
                                    break;
                            }
                        }
                    }

                    // If this animation is a lower_weapon animation
                    if ((gAnimControl[usNewState].uiFlags.HasFlag(ANIM.LOWER_WEAPON)) && !(gAnimControl[pSoldier.usAnimState].uiFlags.HasFlag(ANIM.LOWER_WEAPON | ANIM.NOCHANGE_WEAPON)))
                    {
                        // We are told that we need to rasie weapon
                        // Do so only if
                        // 1) We have a rifle in hand...
                        usItem = pSoldier.inv[InventorySlot.HANDPOS].usItem;

                        if (usItem != NOTHING && (Item[usItem].fFlags & ITEM_TWO_HANDED) && usItem != ROCKET_LAUNCHER)
                        {
                            // Switch on height!
                            switch (gAnimControl[pSoldier.usAnimState].ubEndHeight)
                            {
                                case AnimationHeights.ANIM_STAND:

                                    // 2) OK, all's fine... lower weapon first....
                                    pSoldier.usPendingAnimation = usNewState;
                                    // Set new state to be animation to move to new stance
                                    usNewState = AnimationStates.LOWER_RIFLE;
                                    break;
                            }
                        }
                    }
                }
            }

            // Are we cowering and are tyring to move, getup first...
            if (gAnimControl[usNewState].uiFlags & ANIM.MOVING && pSoldier.usAnimState == AnimationStates.COWERING && gAnimControl[usNewState].ubEndHeight == AnimationHeights.ANIM_STAND)
            {
                pSoldier.usPendingAnimation = usNewState;
                // Set new state to be animation to move to new stance
                usNewState = AnimationStates.END_COWER;
            }

            // If we want to start swatting, put a pending animation
            if (pSoldier.usAnimState != AnimationStates.START_SWAT && usNewState == AnimationStates.SWATTING)
            {
                // Set new state to be animation to move to new stance
                usNewState = AnimationStates.START_SWAT;
            }

            if (pSoldier.usAnimState == AnimationStates.SWATTING && usNewState == AnimationStates.CROUCHING)
            {
                // Set new state to be animation to move to new stance
                usNewState = AnimationStates.END_SWAT;
            }

            if (pSoldier.usAnimState == AnimationStates.WALKING
                && usNewState == AnimationStates.STANDING
                && pSoldier.bLife < INJURED_CHANGE_THREASHOLD
                && pSoldier.ubBodyType <= SoldierBodyTypes.REGFEMALE
                && !MercInWater(pSoldier))
            {
                // Set new state to be animation to move to new stance
                usNewState = AnimationStates.END_HURT_WALKING;
            }

            // Check if we are an enemy, and we are in an animation what should be overriden
            // by if he sees us or not.
            if (ReevaluateEnemyStance(pSoldier, usNewState))
            {
                return (true);
            }

            // OK.......
            if (pSoldier.ubBodyType > REGFEMALE)
            {
                if (pSoldier.bLife < INJURED_CHANGE_THREASHOLD)
                {
                    if (usNewState == AnimationStates.READY_RIFLE_STAND)
                    {
                        //	pSoldier.usPendingAnimation2 = usNewState;
                        //	usNewState = FROM_INJURED_TRANSITION;
                    }
                }
            }

            // Alrighty, check if we should free buddy up!
            if (usNewState == AnimationStates.GIVING_AID)
            {
                UnSetUIBusy(pSoldier.ubID);
            }


            // SUBSTITUDE VARIOUS REG ANIMATIONS WITH ODD BODY TYPES
            if (SubstituteBodyTypeAnimation(pSoldier, usNewState, usSubState))
            {
                usNewState = usSubState;
            }

            // CHECK IF WE CAN DO THIS ANIMATION!
            if (IsAnimationValidForBodyType(pSoldier, usNewState) == false)
            {
                return (false);
            }

            // OK, make guy transition if a big merc...
            if (pSoldier.uiAnimSubFlags & SUB_ANIM_BIGGUYTHREATENSTANCE)
            {
                if (usNewState == AnimationStates.KNEEL_DOWN && pSoldier.usAnimState != AnimationStates.BIGMERC_CROUCH_TRANS_INTO)
                {
                    Items usItem;

                    // Do we have a rifle?
                    usItem = pSoldier.inv[InventorySlot.HANDPOS].usItem;

                    if (usItem != NOTHING)
                    {
                        if (Item[usItem].usItemClass == IC.GUN && usItem != ROCKET_LAUNCHER)
                        {
                            if ((Item[usItem].fFlags & ITEM_TWO_HANDED))
                            {
                                usNewState = AnimationStates.BIGMERC_CROUCH_TRANS_INTO;
                            }
                        }
                    }
                }

                if (usNewState == AnimationStates.KNEEL_UP && pSoldier.usAnimState != AnimationStates.BIGMERC_CROUCH_TRANS_OUTOF)
                {
                    Items usItem;

                    // Do we have a rifle?
                    usItem = pSoldier.inv[InventorySlot.HANDPOS].usItem;

                    if (usItem != NOTHING)
                    {
                        if (Item[usItem].usItemClass == IC.GUN && usItem != ROCKET_LAUNCHER)
                        {
                            if ((Item[usItem].fFlags & ITEM_TWO_HANDED))
                            {
                                usNewState = AnimationStates.BIGMERC_CROUCH_TRANS_OUTOF;
                            }
                        }
                    }
                }
            }

            // OK, if we have reverse set, do the side step!
            if (pSoldier.bReverse)
            {
                if (usNewState == AnimationStates.WALKING || usNewState == AnimationStates.RUNNING || usNewState == AnimationStates.SWATTING)
                {
                    // CHECK FOR SIDEWAYS!
                    if (pSoldier.bDirection == gPurpendicularDirection[pSoldier.bDirection, pSoldier.usPathingData[pSoldier.usPathIndex]])
                    {
                        // We are perpendicular!
                        usNewState = AnimationStates.SIDE_STEP;
                    }
                    else
                    {
                        if (gAnimControl[pSoldier.usAnimState].ubEndHeight == AnimationHeights.ANIM_CROUCH)
                        {
                            usNewState = AnimationStates.SWAT_BACKWARDS;
                        }
                        else
                        {
                            // Here, change to  opposite direction
                            usNewState = AnimationStates.WALK_BACKWARDS;
                        }
                    }
                }
            }

            // ATE: Patch hole for breath collapse for roofs, fences
            if (usNewState == AnimationStates.CLIMBUPROOF || usNewState == AnimationStates.CLIMBDOWNROOF || usNewState == AnimationStates.HOPFENCE)
            {
                // Check for breath collapse if a given animation like
                if (CheckForBreathCollapse(pSoldier) || pSoldier.bCollapsed)
                {
                    // UNset UI
                    UnSetUIBusy(pSoldier.ubID);

                    SoldierCollapse(pSoldier);

                    pSoldier.bBreathCollapsed = false;

                    return (false);

                }
            }

            // If we are in water.....and trying to run, change to run
            if (pSoldier.bOverTerrainType == LOW_WATER || pSoldier.bOverTerrainType == MED_WATER)
            {
                // Check animation
                // Change to walking
                if (usNewState == RUNNING)
                {
                    usNewState = WALKING;
                }
            }

            // Turn off anipause flag for any anim!
            pSoldier.uiStatusFlags &= (~SOLDIER.PAUSEANIMOVE);

            // Unset paused for no APs.....
            AdjustNoAPToFinishMove(pSoldier, false);

            if (usNewState == CRAWLING && pSoldier.usDontUpdateNewGridNoOnMoveAnimChange == 1)
            {
                if (pSoldier.fTurningFromPronePosition != TURNING_FROM_PRONE_ENDING_UP_FROM_MOVE)
                {
                    pSoldier.fTurningFromPronePosition = TURNING_FROM_PRONE_START_UP_FROM_MOVE;
                }

                // ATE: IF we are starting to crawl, but have to getup to turn first......
                if (pSoldier.fTurningFromPronePosition == TURNING_FROM_PRONE_START_UP_FROM_MOVE)
                {
                    usNewState = PRONE_UP;
                    pSoldier.fTurningFromPronePosition = TURNING_FROM_PRONE_ENDING_UP_FROM_MOVE;
                }
            }

            // We are about to start moving
            // Handle buddy beginning to move...
            // check new gridno, etc
            // ATE: Added: Make check that old anim is not a moving one as well
            if (gAnimControl[usNewState].uiFlags & ANIM_MOVING && !(gAnimControl[pSoldier.usAnimState].uiFlags & ANIM_MOVING) || (gAnimControl[usNewState].uiFlags & ANIM_MOVING && fForce))
            {
                bool fKeepMoving;

                if (usNewState == CRAWLING && pSoldier.usDontUpdateNewGridNoOnMoveAnimChange == LOCKED_NO_NEWGRIDNO)
                {
                    // Turn off lock once we are crawling once...
                    pSoldier.usDontUpdateNewGridNoOnMoveAnimChange = 1;
                }

                // ATE: Additional check here if we have just been told to update animation ONLY, not goto gridno stuff...
                if (!pSoldier.usDontUpdateNewGridNoOnMoveAnimChange)
                {
                    if (usNewState != SWATTING)
                    {
                        //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("Handling New gridNo for %d: Old %s, New %s", pSoldier.ubID, gAnimControl[pSoldier.usAnimState].zAnimStr, gAnimControl[usNewState].zAnimStr));

                        if (!(gAnimControl[usNewState].uiFlags & ANIM_SPECIALMOVE))
                        {
                            // Handle goto new tile...
                            if (HandleGotoNewGridNo(pSoldier, &fKeepMoving, true, usNewState))
                            {
                                if (!fKeepMoving)
                                {
                                    return (false);
                                }

                                // Make sure desy = zeroed out...
                                // pSoldier.fPastXDest = pSoldier.fPastYDest = false;
                            }
                            else
                            {
                                if (pSoldier.bBreathCollapsed)
                                {
                                    // UNset UI
                                    UnSetUIBusy(pSoldier.ubID);

                                    SoldierCollapse(pSoldier);

                                    pSoldier.bBreathCollapsed = false;
                                }
                                return (false);
                            }
                        }
                        else
                        {
                            // Change desired direction
                            // Just change direction
                            EVENT_InternalSetSoldierDestination(pSoldier, pSoldier.usPathingData[pSoldier.usPathIndex], false, pSoldier.usAnimState);
                        }

                        //check for services
                        ReceivingSoldierCancelServices(pSoldier);
                        GivingSoldierCancelServices(pSoldier);


                        // Check if we are a vehicle, and start playing noise sound....
                        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE))
                        {
                            HandleVehicleMovementSound(pSoldier, true);
                        }
                    }
                }
            }
            else
            {
                // Check for stopping movement noise...
                if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE))
                {
                    HandleVehicleMovementSound(pSoldier, false);

                    // If a vehicle, set hewight to 0
                    SetSoldierHeight(pSoldier, (double)(0));
                }

            }

            // Reset to false always.....
            // ( Unless locked ) 
            if (gAnimControl[usNewState].uiFlags & ANIM_MOVING)
            {
                if (pSoldier.usDontUpdateNewGridNoOnMoveAnimChange != LOCKED_NO_NEWGRIDNO)
                {
                    pSoldier.usDontUpdateNewGridNoOnMoveAnimChange = 0;
                }
            }

            if (fTryingToRestart)
            {
                return (false);
            }

        }


        // ATE: If this is an AI guy.. unlock him!
        if (gTacticalStatus.fEnemySightingOnTheirTurn)
        {
            if (gTacticalStatus.ubEnemySightingOnTheirTurnEnemyID == pSoldier.ubID)
            {
                pSoldier.fPauseAllAnimation = false;
                gTacticalStatus.fEnemySightingOnTheirTurn = false;
            }
        }

        ///////////////////////////////////////////////////////////////////////
        //			HERE DOWN - WE HAVE MADE A DESCISION!
        /////////////////////////////////////////////////////////////////////

        uiOldAnimFlags = gAnimControl[pSoldier.usAnimState].uiFlags;
        uiNewAnimFlags = gAnimControl[usNewState].uiFlags;

        usNewGridNo = IsometricUtils.NewGridNo(pSoldier.sGridNo, IsometricUtils.DirectionInc(pSoldier.usPathingData[pSoldier.usPathIndex]));


        // CHECKING IF WE HAVE A HIT FINISH BUT NO DEATH IS DONE WITH A SPECIAL ANI CODE
        // IN THE HIT FINSIH ANI SCRIPTS

        // CHECKING IF WE HAVE FINISHED A DEATH ANIMATION IS DONE WITH A SPECIAL ANI CODE
        // IN THE DEATH SCRIPTS


        // CHECK IF THIS NEW STATE IS NON-INTERRUPTABLE
        // IF SO - SET NON-INT FLAG
        if (uiNewAnimFlags & ANIM_NONINTERRUPT)
        {
            pSoldier.fInNonintAnim = true;
        }

        if (uiNewAnimFlags & ANIM_RT_NONINTERRUPT)
        {
            pSoldier.fRTInNonintAnim = true;
        }

        // CHECK IF WE ARE NOT AIMING, IF NOT, RESET LAST TAGRET!
        if (!(gAnimControl[pSoldier.usAnimState].uiFlags & ANIM_FIREREADY) && !(gAnimControl[usNewState].uiFlags & ANIM_FIREREADY))
        {
            // ATE: Also check for the transition anims to not reset this
            // this should have used a flag but we're out of them....
            if (usNewState != READY_RIFLE_STAND && usNewState != READY_RIFLE_PRONE && usNewState != READY_RIFLE_CROUCH && usNewState != ROBOT_SHOOT)
            {
                pSoldier.sLastTarget = NOWHERE;
            }
        }

        // If a special move state, release np aps
        if ((gAnimControl[usNewState].uiFlags & ANIM_SPECIALMOVE))
        {
            AdjustNoAPToFinishMove(pSoldier, false);
        }

        if (gAnimControl[usNewState].uiFlags & ANIM_UPDATEMOVEMENTMODE)
        {
            if (pSoldier.bTeam == gbPlayerNum)
            {
                // pSoldier.usUIMovementMode =  GetMoveStateBasedOnStance( pSoldier, gAnimControl[ usNewState ].ubEndHeight );	
            }
        }

        // ATE: If not a moving animation - turn off reverse....
        if (!(gAnimControl[usNewState].uiFlags.HasFlag(ANIM.MOVING)))
        {
            pSoldier.bReverse = false;
        }

        // ONLY DO FOR EVERYONE BUT PLANNING GUYS
        if (pSoldier.ubID < MAX_NUM_SOLDIERS)
        {

            // Do special things based on new state
            switch (usNewState)
            {
                case AnimationStates.STANDING:

                    // Update desired height
                    pSoldier.ubDesiredHeight = AnimationHeights.ANIM_STAND;
                    break;

                case AnimationStates.CROUCHING:

                    // Update desired height
                    pSoldier.ubDesiredHeight = AnimationHeights.ANIM_CROUCH;
                    break;

                case AnimationStates.PRONE:

                    // Update desired height
                    pSoldier.ubDesiredHeight = AnimationHeights.ANIM_PRONE;
                    break;

                case AnimationStates.READY_RIFLE_STAND:
                case AnimationStates.READY_RIFLE_PRONE:
                case AnimationStates.READY_RIFLE_CROUCH:
                case AnimationStates.READY_DUAL_STAND:
                case AnimationStates.READY_DUAL_CROUCH:
                case AnimationStates.READY_DUAL_PRONE:

                    // OK, get points to ready weapon....
                    if (!pSoldier.fDontChargeReadyAPs)
                    {
                        sAPCost = GetAPsToReadyWeapon(pSoldier, usNewState);
                        DeductPoints(pSoldier, sAPCost, sBPCost);
                    }
                    else
                    {
                        pSoldier.fDontChargeReadyAPs = false;
                    }
                    break;

                case AnimationStates.WALKING:

                    pSoldier.usPendingAnimation = NO_PENDING_ANIMATION;
                    pSoldier.ubPendingActionAnimCount = 0;
                    break;

                case AnimationStates.SWATTING:

                    pSoldier.usPendingAnimation = NO_PENDING_ANIMATION;
                    pSoldier.ubPendingActionAnimCount = 0;
                    break;

                case AnimationStates.CRAWLING:

                    // Turn off flag...
                    pSoldier.fTurningFromPronePosition = TURNING_FROM_PRONE_OFF;
                    pSoldier.ubPendingActionAnimCount = 0;
                    pSoldier.usPendingAnimation = NO_PENDING_ANIMATION;
                    break;

                case AnimationStates.RUNNING:

                    // Only if our previous is not running
                    if (pSoldier.usAnimState != AnimationStates.RUNNING)
                    {
                        sAPCost = AP.START_RUN_COST;
                        Points.DeductPoints(pSoldier, sAPCost, sBPCost);
                    }
                    // Set pending action count to 0
                    pSoldier.ubPendingActionAnimCount = 0;
                    pSoldier.usPendingAnimation = NO_PENDING_ANIMATION;
                    break;

                case AnimationStates.ADULTMONSTER_WALKING:
                    pSoldier.ubPendingActionAnimCount = 0;
                    break;

                case AnimationStates.ROBOT_WALK:
                    pSoldier.ubPendingActionAnimCount = 0;
                    break;

                case AnimationStates.KNEEL_UP:
                case AnimationStates.KNEEL_DOWN:
                case AnimationStates.BIGMERC_CROUCH_TRANS_INTO:
                case AnimationStates.BIGMERC_CROUCH_TRANS_OUTOF:

                    if (!pSoldier.fDontChargeAPsForStanceChange)
                    {
                        Points.DeductPoints(pSoldier, AP.CROUCH, BP.CROUCH);
                    }
                    pSoldier.fDontChargeAPsForStanceChange = false;
                    break;

                case AnimationStates.PRONE_UP:
                case AnimationStates.PRONE_DOWN:

                    // ATE: If we are NOT waiting for prone down...
                    if (pSoldier.fTurningFromPronePosition < TURNING_FROM_PRONE_START_UP_FROM_MOVE && !pSoldier.fDontChargeAPsForStanceChange)
                    {
                        // ATE: Don't do this if we are still 'moving'....
                        if (pSoldier.sGridNo == pSoldier.sFinalDestination || pSoldier.usPathIndex == 0)
                        {
                            Points.DeductPoints(pSoldier, AP.PRONE, BP.PRONE);
                        }
                    }
                    pSoldier.fDontChargeAPsForStanceChange = false;
                    break;

                //Deduct points for stance change
                //sAPCost = GetAPsToChangeStance( pSoldier, gAnimControl[ usNewState ].ubEndHeight );
                //DeductPoints( pSoldier, sAPCost, 0 );
                //break;

                case AnimationStates.START_AID:

                    Points.DeductPoints(pSoldier, AP.START_FIRST_AID, BP.START_FIRST_AID);
                    break;

                case AnimationStates.CUTTING_FENCE:
                    Points.DeductPoints(pSoldier, AP.USEWIRECUTTERS, BP.USEWIRECUTTERS);
                    break;

                case AnimationStates.PLANT_BOMB:

                    Points.DeductPoints(pSoldier, AP.DROP_BOMB, 0);
                    break;

                case AnimationStates.STEAL_ITEM:

                    Points.DeductPoints(pSoldier, AP.STEAL_ITEM, 0);
                    break;

                case AnimationStates.CROW_DIE:

                    // Delete shadow of crow....
                    if (pSoldier.pAniTile != null)
                    {
                        DeleteAniTile(pSoldier.pAniTile);
                        pSoldier.pAniTile = null;
                    }
                    break;

                case AnimationStates.CROW_FLY:

                    // Ate: startup a shadow ( if gridno is set )
                    HandleCrowShadowNewGridNo(pSoldier);
                    break;

                case AnimationStates.CROW_EAT:

                    // ATE: Make sure height level is 0....
                    SetSoldierHeight(pSoldier, (double)(0));
                    HandleCrowShadowRemoveGridNo(pSoldier);
                    break;

                case AnimationStates.USE_REMOTE:

                    Points.DeductPoints(pSoldier, AP.USE_REMOTE, 0);
                    break;

                //case PUNCH:

                //Deduct points for punching
                //sAPCost = MinAPsToAttack( pSoldier, pSoldier.sGridNo, false );
                //DeductPoints( pSoldier, sAPCost, 0 );
                //break;

                case AnimationStates.HOPFENCE:

                    Points.DeductPoints(pSoldier, AP.JUMPFENCE, BP.JUMPFENCE);
                    break;

                // Deduct aps for falling down....
                case AnimationStates.FALLBACK_HIT_STAND:
                case AnimationStates.FALLFORWARD_FROMHIT_STAND:

                    Points.DeductPoints(pSoldier, AP.FALL_DOWN, BP.FALL_DOWN);
                    break;

                case AnimationStates.FALLFORWARD_FROMHIT_CROUCH:

                    Points.DeductPoints(pSoldier, (AP.FALL_DOWN / 2), (BP.FALL_DOWN / 2));
                    break;

                case AnimationStates.QUEEN_SWIPE:

                    // ATE: set damage counter...
                    pSoldier.uiPendingActionData1 = 0;
                    break;

                case AnimationStates.CLIMBDOWNROOF:

                    // disable sight
                    gTacticalStatus.uiFlags |= TacticalEngineStatus.DISALLOW_SIGHT;

                    Points.DeductPoints(pSoldier, AP.CLIMBOFFROOF, BP.CLIMBOFFROOF);
                    break;

                case AnimationStates.CLIMBUPROOF:

                    // disable sight
                    gTacticalStatus.uiFlags |= TacticalEngineStatus.DISALLOW_SIGHT;

                    Points.DeductPoints(pSoldier, AP.CLIMBROOF, BP.CLIMBROOF);
                    break;

                case AnimationStates.JUMP_OVER_BLOCKING_PERSON:

                    // Set path....
                    {
                        int usNewGridNo;

                        Points.DeductPoints(pSoldier, AP.JUMP_OVER, BP.JUMP_OVER);

                        usNewGridNo = IsometricUtils.NewGridNo(pSoldier.sGridNo, IsometricUtils.DirectionInc(pSoldier.bDirection));
                        usNewGridNo = IsometricUtils.NewGridNo(usNewGridNo, IsometricUtils.DirectionInc(pSoldier.bDirection));

                        pSoldier.usPathDataSize = 0;
                        pSoldier.usPathIndex = 0;
                        pSoldier.usPathingData[pSoldier.usPathDataSize] = pSoldier.bDirection;
                        pSoldier.usPathDataSize++;
                        pSoldier.usPathingData[pSoldier.usPathDataSize] = pSoldier.bDirection;
                        pSoldier.usPathDataSize++;
                        pSoldier.sFinalDestination = usNewGridNo;
                        // Set direction
                        EVENT_InternalSetSoldierDestination(pSoldier, pSoldier.usPathingData[pSoldier.usPathIndex], false, JUMP_OVER_BLOCKING.PERSON);
                    }
                    break;


                case AnimationStates.GENERIC_HIT_STAND:
                case AnimationStates.GENERIC_HIT_CROUCH:
                case AnimationStates.STANDING_BURST_HIT:
                case AnimationStates.ADULTMONSTER_HIT:
                case AnimationStates.ADULTMONSTER_DYING:
                case AnimationStates.COW_HIT:
                case AnimationStates.COW_DYING:
                case AnimationStates.BLOODCAT_HIT:
                case AnimationStates.BLOODCAT_DYING:
                case AnimationStates.WATER_HIT:
                case AnimationStates.WATER_DIE:
                case AnimationStates.DEEP_WATER_HIT:
                case AnimationStates.DEEP_WATER_DIE:
                case AnimationStates.RIFLE_STAND_HIT:
                case AnimationStates.LARVAE_HIT:
                case AnimationStates.LARVAE_DIE:
                case AnimationStates.QUEEN_HIT:
                case AnimationStates.QUEEN_DIE:
                case AnimationStates.INFANT_HIT:
                case AnimationStates.INFANT_DIE:
                case AnimationStates.CRIPPLE_HIT:
                case AnimationStates.CRIPPLE_DIE:
                case AnimationStates.CRIPPLE_DIE_FLYBACK:
                case AnimationStates.ROBOTNW_HIT:
                case AnimationStates.ROBOTNW_DIE:

                    // Set getting hit flag to true
                    pSoldier.fGettingHit = true;
                    break;

                case AnimationStates.CHARIOTS_OF_FIRE:
                case AnimationStates.BODYEXPLODING:

                    // Merc on fire!
                    // pSoldier.uiPendingActionData1 = PlaySoldierJA2Sample(pSoldier.ubID, (FIRE_ON_MERC), RATE_11025, SoundVolume(HIGHVOLUME, pSoldier.sGridNo), 5, SoundDir(pSoldier.sGridNo), true);
                    break;
            }
        }

        // Remove old animation profile
        HandleAnimationProfile(pSoldier, pSoldier.usAnimState, true);


        // From animation control, set surface
        if (SetSoldierAnimationSurface(pSoldier, usNewState) == false)
        {
            return (false);
        }


        // Set state
        pSoldier.usOldAniState = pSoldier.usAnimState;
        pSoldier.sOldAniCode = pSoldier.usAniCode;

        // Change state value!
        pSoldier.usAnimState = usNewState;

        pSoldier.sZLevelOverride = -1;

        if (!(pSoldier.uiStatusFlags.HasFlag(SOLDIER.LOCKPENDINGACTIONCOUNTER)))
        {
            //ATE Cancel ANY pending action...
            if (pSoldier.ubPendingActionAnimCount > 0 && (gAnimControl[pSoldier.usOldAniState].uiFlags.HasFlag(ANIM.MOVING)))
            {
                // Do some special things for some actions
                switch (pSoldier.ubPendingAction)
                {
                    case MERC.GIVEITEM:

                        // Unset target as enaged
                        MercPtrs[pSoldier.uiPendingActionData4].uiStatusFlags &= (~SOLDIER.ENGAGEDINACTION);
                        break;
                }
                pSoldier.ubPendingAction = MERC.NO_PENDING_ACTION;
            }
            else
            {
                // Increment this for almost all animations except some movement ones...
                // That's because this represents ANY animation other than the one we began when the pending action was started
                // ATE: Added to ignore this count if we are waiting for someone to move out of our way...
                if (usNewState != AnimationStates.START_SWAT
                    && usNewState != AnimationStates.END_SWAT
                    && !(gAnimControl[usNewState].uiFlags.HasFlag(ANIM.NOCHANGE_PENDINGCOUNT))
                    && pSoldier.fDelayedMovement == 0
                    && !(pSoldier.uiStatusFlags.HasFlag(SOLDIER.ENGAGEDINACTION)))
                {
                    pSoldier.ubPendingActionAnimCount++;
                }
            }
        }

        // Set new animation profile
        HandleAnimationProfile(pSoldier, usNewState, false);

        // Reset some animation values
        pSoldier.fForceShade = false;

        CheckForFreeupFromHit(pSoldier, uiOldAnimFlags, uiNewAnimFlags, pSoldier.usOldAniState, usNewState);

        // Set current frame
        pSoldier.usAniCode = usStartingAniCode;

        // ATE; For some animations that could use some variations, do so....
        if (usNewState == CHARIOTS_OF_FIRE || usNewState == BODYEXPLODING)
        {
            pSoldier.usAniCode = (int)(Globals.Random.Next(10));
        }

        // ATE: Default to first frame....
        // Will get changed ( probably ) by AdjustToNextAnimationFrame()
        ConvertAniCodeToAniFrame(pSoldier, (int)(0));

        // Set delay speed
        SetSoldierAniSpeed(pSoldier);

        // Reset counters
        RESETTIMECOUNTER(ref pSoldier.UpdateCounter, pSoldier.sAniDelay);

        // Adjust to new animation frame ( the first one )
        AdjustToNextAnimationFrame(pSoldier);

        // Setup offset information for UI above guy
        SetSoldierLocatorOffsets(pSoldier);

        // If our own guy...
        if (pSoldier.bTeam == gbPlayerNum)
        {
            // Are we stationary?
            if (gAnimControl[usNewState].uiFlags & ANIM_STATIONARY)
            {
                // Position light....
                // SetCheckSoldierLightFlag( pSoldier );
            }
            else
            {
                // Hide light.....
                // DeleteSoldierLight( pSoldier );
            }
        }

        // If we are certain animations, reload palette
        if (usNewState == VEHICLE_DIE || usNewState == CHARIOTS_OF_FIRE || usNewState == BODYEXPLODING)
        {
            CreateSoldierPalettes(pSoldier);
        }

        // ATE: if the old animation was a movement, and new is not, play sound...
        // OK, play final footstep sound...
        if (!(gTacticalStatus.uiFlags & LOADING_SAVED_GAME))
        {
            if ((gAnimControl[pSoldier.usAnimState].uiFlags & ANIM_STATIONARY) &&
                     (gAnimControl[pSoldier.usOldAniState].uiFlags & ANIM_MOVING))
            {
                PlaySoldierFootstepSound(pSoldier);
            }
        }

        // Free up from stance change
        FreeUpNPCFromStanceChange(pSoldier);

        return (true);
    }


    void InternalRemoveSoldierFromGridNo(SOLDIERTYPE? pSoldier, bool fForce)
    {
        int bDir;
        int iGridNo;

        if ((pSoldier.sGridNo != NO_MAP.POS))
        {
            if (pSoldier.bInSector || fForce)
            {
                // Remove from world ( old pos )
                RemoveMerc(pSoldier.sGridNo, pSoldier, false);
                HandleAnimationProfile(pSoldier, pSoldier.usAnimState, true);

                // Remove records of this guy being adjacent
                for (bDir = 0; bDir < NUM_WORLD_DIRECTIONS; bDir++)
                {
                    iGridNo = pSoldier.sGridNo + DirIncrementer[bDir];
                    if (iGridNo >= 0 && iGridNo < WORLD_MAX)
                    {
                        gpWorldLevelData[iGridNo].ubAdjacentSoldierCnt--;
                    }
                }

                HandlePlacingRoofMarker(pSoldier, pSoldier.sGridNo, false, false);

                // Remove reseved movement value
                UnMarkMovementReserved(pSoldier);

                HandleCrowShadowRemoveGridNo(pSoldier);

                // Reset gridno...
                pSoldier.sGridNo = NO_MAP_POS;
            }
        }
    }

    void RemoveSoldierFromGridNo(SOLDIERTYPE? pSoldier)
    {
        InternalRemoveSoldierFromGridNo(pSoldier, false);
    }


    public static void EVENT_InternalSetSoldierPosition(SOLDIERTYPE? pSoldier, float dNewXPos, float dNewYPos, bool fUpdateDest, bool fUpdateFinalDest, bool fForceRemove)
    {
        int sNewGridNo;

        // Not if we're dead!
        if ((pSoldier.uiStatusFlags.HasFlag(SOLDIER.DEAD)))
        {
            return;
        }

        // Set new map index
        sNewGridNo = GETWORLDINDEXFROMWORLDCOORDS(dNewYPos, dNewXPos);

        if (fUpdateDest)
        {
            pSoldier.sDestination = sNewGridNo;
        }

        if (fUpdateFinalDest)
        {
            pSoldier.sFinalDestination = sNewGridNo;
        }

        // Copy old values
        pSoldier.dOldXPos = pSoldier.dXPos;
        pSoldier.dOldYPos = pSoldier.dYPos;

        // Set New pos
        pSoldier.dXPos = dNewXPos;
        pSoldier.dYPos = dNewYPos;

        pSoldier.sX = (int)dNewXPos;
        pSoldier.sY = (int)dNewYPos;

        HandleCrowShadowNewPosition(pSoldier);

        SetSoldierGridNo(pSoldier, sNewGridNo, fForceRemove);

        if (!(pSoldier.uiStatusFlags.HasFlag(SOLDIER.DRIVER | SOLDIER.PASSENGER)))
        {
            if (GameSettings.fOptions[TOPTION.MERC_ALWAYS_LIGHT_UP])
            {
                SetCheckSoldierLightFlag(pSoldier);
            }
        }

        // ATE: Mirror calls if we are a vehicle ( for all our passengers )
        UpdateAllVehiclePassengersGridNo(pSoldier);

    }

    public static void EVENT_SetSoldierPosition(SOLDIERTYPE? pSoldier, float dNewXPos, float dNewYPos)
    {
        EVENT_InternalSetSoldierPosition(pSoldier, dNewXPos, dNewYPos, true, true, false);
    }

    public static void EVENT_SetSoldierPositionForceDelete(SOLDIERTYPE? pSoldier, float dNewXPos, float dNewYPos)
    {
        EVENT_InternalSetSoldierPosition(pSoldier, dNewXPos, dNewYPos, true, true, true);
    }

    public static void EVENT_SetSoldierPositionAndMaybeFinalDest(SOLDIERTYPE? pSoldier, float dNewXPos, float dNewYPos, bool fUpdateFinalDest)
    {
        EVENT_InternalSetSoldierPosition(pSoldier, dNewXPos, dNewYPos, true, fUpdateFinalDest, false);
    }

    public static void EVENT_SetSoldierPositionAndMaybeFinalDestAndMaybeNotDestination(SOLDIERTYPE? pSoldier, float dNewXPos, float dNewYPos, bool fUpdateDest, bool fUpdateFinalDest)
    {
        EVENT_InternalSetSoldierPosition(pSoldier, dNewXPos, dNewYPos, fUpdateDest, fUpdateFinalDest, false);
    }


    public static void InternalSetSoldierHeight(SOLDIERTYPE? pSoldier, double dNewHeight, bool fUpdateLevel)
    {

        int bOldLevel = pSoldier.bLevel;

        pSoldier.dHeightAdjustment = dNewHeight;
        pSoldier.sHeightAdjustment = (int)pSoldier.dHeightAdjustment;

        if (!fUpdateLevel)
        {
            return;
        }

        if (pSoldier.sHeightAdjustment > 0)
        {
            pSoldier.bLevel = SECOND_LEVEL;

            ApplyTranslucencyToWalls((int)(pSoldier.dXPos / CELL_X_SIZE), (int)(pSoldier.dYPos / CELL_Y_SIZE));
            //LightHideTrees((int)(pSoldier.dXPos/CELL_X_SIZE), (int)(pSoldier.dYPos/CELL_Y_SIZE));
            //ConcealAllWalls();

            //pSoldier.pLevelNode.ubShadeLevel=gpWorldLevelData[pSoldier.sGridNo].pRoofHead.ubShadeLevel;
            //pSoldier.pLevelNode.ubSumLights=gpWorldLevelData[pSoldier.sGridNo].pRoofHead.ubSumLights;
            //pSoldier.pLevelNode.ubMaxLights=gpWorldLevelData[pSoldier.sGridNo].pRoofHead.ubMaxLights;
            //pSoldier.pLevelNode.ubNaturalShadeLevel=gpWorldLevelData[pSoldier.sGridNo].pRoofHead.ubNaturalShadeLevel;
        }
        else
        {
            pSoldier.bLevel = FIRST_LEVEL;

            //pSoldier.pLevelNode.ubShadeLevel=gpWorldLevelData[pSoldier.sGridNo].pLandHead.ubShadeLevel;
            //pSoldier.pLevelNode.ubSumLights=gpWorldLevelData[pSoldier.sGridNo].pLandHead.ubSumLights;
            //pSoldier.pLevelNode.ubMaxLights=gpWorldLevelData[pSoldier.sGridNo].pLandHead.ubMaxLights;
            //pSoldier.pLevelNode.ubNaturalShadeLevel=gpWorldLevelData[pSoldier.sGridNo].pLandHead.ubNaturalShadeLevel;


        }

        if (bOldLevel == 0 && pSoldier.bLevel == 0)
        {

        }
        else
        {
            // Show room at new level
            //HideRoom( pSoldier.sGridNo, pSoldier );
        }
    }

    public static void SetSoldierHeight(SOLDIERTYPE? pSoldier, double dNewHeight)
    {
        InternalSetSoldierHeight(pSoldier, dNewHeight, true);
    }


    public static void SetSoldierGridNo(SOLDIERTYPE? pSoldier, int sNewGridNo, bool fForceRemove)
    {
        bool fInWaterValue;
        int bDir;
        int cnt;
        SOLDIERTYPE? pEnemy;

        //int	sX, sY, sWorldX, sZLevel;

        // Not if we're dead!
        if ((pSoldier.uiStatusFlags.HasFlag(SOLDIER.DEAD)))
        {
            return;
        }

        if (sNewGridNo != pSoldier.sGridNo || pSoldier.pLevelNode == null)
        {
            // Check if we are moving AND this is our next dest gridno....
            if (gAnimControl[pSoldier.usAnimState].uiFlags & (ANIM_MOVING | ANIM_SPECIALMOVE))
            {
                if (!(gTacticalStatus.uiFlags & LOADING_SAVED_GAME))
                {
                    if (sNewGridNo != pSoldier.sDestination)
                    {
                        // THIS MUST be our new one......MAKE IT SO
                        sNewGridNo = pSoldier.sDestination;
                    }

                    // Now check this baby....
                    if (sNewGridNo == pSoldier.sGridNo)
                    {
                        return;
                    }
                }
            }

            pSoldier.sOldGridNo = pSoldier.sGridNo;

            if (pSoldier.ubBodyType == QUEENMONSTER)
            {
                SetPositionSndGridNo(pSoldier.iPositionSndID, sNewGridNo);
            }

            if (!(pSoldier.uiStatusFlags & (SOLDIER.DRIVER | SOLDIER.PASSENGER)))
            {
                InternalRemoveSoldierFromGridNo(pSoldier, fForceRemove);
            }

            // CHECK IF OUR NEW GIRDNO IS VALID,IF NOT DONOT SET!
            if (!IsometricUtils.GridNoOnVisibleWorldTile(sNewGridNo))
            {
                pSoldier.sGridNo = sNewGridNo;
                return;
            }

            // Alrighty, update UI for this guy, if he's the selected guy...
            if (gusSelectedSoldier == pSoldier.ubID)
            {
                if (guiCurrentEvent == C_WAIT_FOR_CONFIRM)
                {
                    // Update path!
                    gfPlotNewMovement = true;
                }
            }


            // Reset some flags for optimizations..
            pSoldier.sWalkToAttackGridNo = NOWHERE;

            // ATE: Make sure!
            // RemoveMerc( pSoldier.sGridNo, pSoldier, false );

            pSoldier.sGridNo = sNewGridNo;

            // OK, check for special code to close door...
            if (pSoldier.bEndDoorOpenCode == 2)
            {
                pSoldier.bEndDoorOpenCode = 0;

                HandleDoorChangeFromGridNo(pSoldier, pSoldier.sEndDoorOpenCodeData, false);
            }

            // OK, Update buddy's strategic insertion code....
            pSoldier.ubStrategicInsertionCode = INSERTION_CODE_GRIDNO;
            pSoldier.usStrategicInsertionData = sNewGridNo;


            // Remove this gridno as a reserved place!
            if (!(pSoldier.uiStatusFlags & (SOLDIER.DRIVER | SOLDIER.PASSENGER)))
            {
                UnMarkMovementReserved(pSoldier);
            }

            if (pSoldier.sInitialGridNo == 0)
            {
                pSoldier.sInitialGridNo = sNewGridNo;
                pSoldier.usPatrolGrid[0] = sNewGridNo;
            }

            // Add records of this guy being adjacent
            for (bDir = 0; bDir < NUM_WORLD_DIRECTIONS; bDir++)
            {
                gpWorldLevelData[pSoldier.sGridNo + DirIncrementer[bDir]].ubAdjacentSoldierCnt++;
            }

            if (!(pSoldier.uiStatusFlags & (SOLDIER.DRIVER | SOLDIER.PASSENGER)))
            {
                DropSmell(pSoldier);
            }

            // HANDLE ANY SPECIAL RENDERING SITUATIONS
            pSoldier.sZLevelOverride = -1;
            // If we are over a fence ( hopping ), make us higher!

            if (IsJumpableFencePresentAtGridno(sNewGridNo))
            {
                //sX = MapX( sNewGridNo );
                //sY = MapY( sNewGridNo );
                //GetWorldXYAbsoluteScreenXY( sX, sY, &sWorldX, &sZLevel);
                //pSoldier.sZLevelOverride = (sZLevel*Z_SUBLAYERS)+ROOF_Z_LEVEL;
                pSoldier.sZLevelOverride = TOPMOST_Z_LEVEL;
            }


            // Add/ remove tree if we are near it
            // CheckForFullStructures( pSoldier );

            // Add merc at new pos
            if (!(pSoldier.uiStatusFlags & (SOLDIER.DRIVER | SOLDIER.PASSENGER)))
            {
                AddMercToHead(pSoldier.sGridNo, pSoldier, true);

                // If we are in the middle of climbing the roof!
                if (pSoldier.usAnimState == CLIMBUPROOF)
                {
                    if (pSoldier.iLight != (-1))
                    {
                        LightSpriteRoofStatus(pSoldier.iLight, true);
                    }
                }
                else if (pSoldier.usAnimState == CLIMBDOWNROOF)
                {
                    if (pSoldier.iLight != (-1))
                    {
                        LightSpriteRoofStatus(pSoldier.iLight, false);
                    }
                }

                //JA2Gold:
                //if the player wants the merc to cast the fake light AND it is night
                if (pSoldier.bTeam != OUR_TEAM || GameSettings.fOptions[TOPTION.MERC_CASTS_LIGHT] && NightTime())
                {
                    if (pSoldier.bLevel > 0 && gpWorldLevelData[pSoldier.sGridNo].pRoofHead != null)
                    {
                        gpWorldLevelData[pSoldier.sGridNo].pMercHead.ubShadeLevel = gpWorldLevelData[pSoldier.sGridNo].pRoofHead.ubShadeLevel;
                        gpWorldLevelData[pSoldier.sGridNo].pMercHead.ubSumLights = gpWorldLevelData[pSoldier.sGridNo].pRoofHead.ubSumLights;
                        gpWorldLevelData[pSoldier.sGridNo].pMercHead.ubMaxLights = gpWorldLevelData[pSoldier.sGridNo].pRoofHead.ubMaxLights;
                        gpWorldLevelData[pSoldier.sGridNo].pMercHead.ubNaturalShadeLevel = gpWorldLevelData[pSoldier.sGridNo].pRoofHead.ubNaturalShadeLevel;
                    }
                    else
                    {
                        gpWorldLevelData[pSoldier.sGridNo].pMercHead.ubShadeLevel = gpWorldLevelData[pSoldier.sGridNo].pLandHead.ubShadeLevel;
                        gpWorldLevelData[pSoldier.sGridNo].pMercHead.ubSumLights = gpWorldLevelData[pSoldier.sGridNo].pLandHead.ubSumLights;
                        gpWorldLevelData[pSoldier.sGridNo].pMercHead.ubMaxLights = gpWorldLevelData[pSoldier.sGridNo].pLandHead.ubMaxLights;
                        gpWorldLevelData[pSoldier.sGridNo].pMercHead.ubNaturalShadeLevel = gpWorldLevelData[pSoldier.sGridNo].pLandHead.ubNaturalShadeLevel;
                    }
                }

                ///HandlePlacingRoofMarker( pSoldier, pSoldier.sGridNo, true, false );

                HandleAnimationProfile(pSoldier, pSoldier.usAnimState, false);

                HandleCrowShadowNewGridNo(pSoldier);
            }

            pSoldier.bOldOverTerrainType = pSoldier.bOverTerrainType;
            pSoldier.bOverTerrainType = WorldManager.GetTerrainType(pSoldier.sGridNo);

            // OK, check that our animation is up to date!
            // Check our water value

            if (!(pSoldier.uiStatusFlags & (SOLDIER.DRIVER | SOLDIER.PASSENGER)))
            {
                fInWaterValue = MercInWater(pSoldier);

                // ATE: If ever in water MAKE SURE WE WALK AFTERWOODS!
                if (fInWaterValue)
                {
                    pSoldier.usUIMovementMode = WALKING;
                }

                if (fInWaterValue != pSoldier.fPrevInWater)
                {
                    //Update Animation data
                    SetSoldierAnimationSurface(pSoldier, pSoldier.usAnimState);

                    // Update flag
                    pSoldier.fPrevInWater = fInWaterValue;

                    // Update sound...
                    if (fInWaterValue)
                    {
                        //PlaySoldierJA2Sample(pSoldier.ubID, ENTER_WATER_1, RATE_11025, SoundVolume(MIDVOLUME, pSoldier.sGridNo), 1, SoundDir(pSoldier.sGridNo), true);
                    }
                    else
                    {
                        // ATE: Check if we are going from water to land - if so, resume
                        // with regular movement mode...
                        EVENT_InitNewSoldierAnim(pSoldier, pSoldier.usUIMovementMode, 0, false);
                    }

                }


                // OK, If we were not in deep water but we are now, handle deep animations!
                if (pSoldier.bOverTerrainType == DEEP_WATER && pSoldier.bOldOverTerrainType != DEEP_WATER)
                {
                    // Based on our current animation, change!
                    switch (pSoldier.usAnimState)
                    {
                        case WALKING:
                        case RUNNING:

                            // IN deep water, swim!

                            // Make transition from low to deep
                            EVENT_InitNewSoldierAnim(pSoldier, LOW_TO_DEEP_WATER, 0, false);
                            pSoldier.usPendingAnimation = DEEP_WATER_SWIM;

                            //PlayJA2Sample(ENTER_DEEP_WATER_1, RATE_11025, SoundVolume(MIDVOLUME, pSoldier.sGridNo), 1, SoundDir(pSoldier.sGridNo));

                    }
                }

                // Damage water if in deep water....
                if (pSoldier.bOverTerrainType == MED_WATER || pSoldier.bOverTerrainType == DEEP_WATER)
                {
                    WaterDamage(pSoldier);
                }

                // OK, If we were in deep water but we are NOT now, handle mid animations!
                if (pSoldier.bOverTerrainType != DEEP_WATER && pSoldier.bOldOverTerrainType == DEEP_WATER)
                {
                    // Make transition from low to deep
                    EVENT_InitNewSoldierAnim(pSoldier, DEEP_TO_LOW_WATER, 0, false);
                    pSoldier.usPendingAnimation = pSoldier.usUIMovementMode;
                }
            }

            // are we now standing in tear gas without a decently working gas mask?
            if (GetSmokeEffectOnTile(sNewGridNo, pSoldier.bLevel))
            {
                bool fSetGassed = true;

                // If we have a functioning gas mask...
                if (pSoldier.inv[HEAD1POS].usItem == GASMASK && pSoldier.inv[HEAD1POS].bStatus[0] >= GASMASK_MIN_STATUS)
                {
                    fSetGassed = false;
                }
                if (pSoldier.inv[HEAD2POS].usItem == GASMASK && pSoldier.inv[HEAD2POS].bStatus[0] >= GASMASK_MIN_STATUS)
                {
                    fSetGassed = false;
                }

                if (fSetGassed)
                {
                    pSoldier.uiStatusFlags |= SOLDIER.GASSED;
                }
            }

            if (pSoldier.bTeam == gbPlayerNum && pSoldier.bStealthMode)
            {
                // Merc got to a new tile by "sneaking". Did we theoretically sneak
                // past an enemy?

                if (pSoldier.bOppCnt > 0)      // opponents in sight
                {
                    // check each possible enemy
                    for (cnt = 0; cnt < MAX_NUM_SOLDIERS; cnt++)
                    {
                        pEnemy = MercPtrs[cnt];
                        // if this guy is here and alive enough to be looking for us
                        if (pEnemy.bActive && pEnemy.bInSector && (pEnemy.bLife >= OKLIFE))
                        {
                            // no points for sneaking by the neutrals & friendlies!!!
                            if (pEnemy.bNeutral == 0 && (pSoldier.bSide != pEnemy.bSide) && (pEnemy.ubBodyType != SoldierBodyTypes.COW && pEnemy.ubBodyType != SoldierBodyTypes.CROW))
                            {
                                // if we SEE this particular oppponent, and he DOESN'T see us... and he COULD see us...
                                if ((pSoldier.bOppList[cnt] == SEEN_CURRENTLY) &&
                                     pEnemy.bOppList[pSoldier.ubID] != SEEN_CURRENTLY &&
                                     IsometricUtils.PythSpacesAway(pSoldier.sGridNo, pEnemy.sGridNo) < OppList.DistanceVisible(pEnemy, WorldDirections.DIRECTION_IRRELEVANT, WorldDirections.DIRECTION_IRRELEVANT, pSoldier.sGridNo, pSoldier.bLevel))
                                {
                                    // AGILITY (5):  Soldier snuck 1 square past unaware enemy
                                    Campaign.StatChange(pSoldier, Stat.AGILAMT, 5, 0);
                                    // Keep looping, we'll give'em 1 point for EACH such enemy!
                                }
                            }
                        }
                    }
                }
            }

            // Adjust speed based on terrain, etc
            SetSoldierAniSpeed(pSoldier);

        }
        else
        {
            int i = 0;
        }
    }


    public static void EVENT_FireSoldierWeapon(SOLDIERTYPE? pSoldier, int sTargetGridNo)
    {
        int sTargetXPos, sTargetYPos;
        bool fDoFireRightAway = false;

        // CANNOT BE SAME GRIDNO!
        if (pSoldier.sGridNo == sTargetGridNo)
        {
            return;
        }

        if (pSoldier.ubID == 33)
        {
            int i = 0;
        }


        // Increment the number of people busy doing stuff because of an attack
        //if ( (gTacticalStatus.uiFlags & TURNBASED) && (gTacticalStatus.uiFlags & INCOMBAT) )
        //{
        gTacticalStatus.ubAttackBusyCount++;
        //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, string.Format("!!!!!!! Starting attack, attack count now %d", gTacticalStatus.ubAttackBusyCount));
        //}

        // Set soldier's target gridno
        // This assignment was redundent because it's already set in 
        // the actual event call
        pSoldier.sTargetGridNo = sTargetGridNo;
        //pSoldier.sLastTarget = sTargetGridNo;
        pSoldier.ubTargetID = WorldManager.WhoIsThere2(sTargetGridNo, pSoldier.bTargetLevel);

        if (Item[pSoldier.inv[InventorySlot.HANDPOS].usItem].usItemClass.HasFlag(IC.GUN))
        {
            if (pSoldier.bDoBurst)
            {
                // Set the TOTAL number of bullets to be fired
                // Can't shoot more bullets than we have in our magazine!
                pSoldier.bBulletsLeft = Math.Min(Weapon[pSoldier.inv[pSoldier.ubAttackingHand].usItem].ubShotsPerBurst, pSoldier.inv[pSoldier.ubAttackingHand].ubGunShotsLeft);
            }
            else if (IsValidSecondHandShot(pSoldier))
            {
                // two-pistol attack - two bullets!
                pSoldier.bBulletsLeft = 2;
            }
            else
            {
                pSoldier.bBulletsLeft = 1;
            }
            if (pSoldier.inv[pSoldier.ubAttackingHand].ubGunAmmoType == AMMO_BUCKSHOT)
            {
                pSoldier.bBulletsLeft *= NUM_BUCKSHOT_PELLETS;
            }
        }
        //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("!!!!!!! Starting attack, bullets left %d", pSoldier.bBulletsLeft));

        // Convert our grid-not into an XY
        IsometricUtils.ConvertGridNoToXY(sTargetGridNo, out sTargetXPos, out sTargetYPos);


        // Change to fire animation
        // Ready weapon
        SoldierReadyWeapon(pSoldier, sTargetXPos, sTargetYPos, false);

        // IF WE ARE AN NPC, SLIDE VIEW TO SHOW WHO IS SHOOTING
        {
            //if ( pSoldier.fDoSpread )
            //{
            // If we are spreading burst, goto right away!
            //EVENT_InitNewSoldierAnim( pSoldier, SelectFireAnimation( pSoldier, gAnimControl[ pSoldier.usAnimState ].ubEndHeight ), 0, false );

            //}

            // else
            {
                if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.MONSTER))
                {
                    // Force our direction!
                    EVENT_SetSoldierDirection(pSoldier, pSoldier.bDesiredDirection);
                    EVENT_InitNewSoldierAnim(pSoldier, SelectFireAnimation(pSoldier, gAnimControl[pSoldier.usAnimState].ubEndHeight), 0, false);
                }
                else
                {
                    // IF WE ARE IN REAl-TIME, FIRE IMMEDIATELY!
                    if (((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.REALTIME))
                        || !(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))))
                    {
                        //fDoFireRightAway = true;
                    }

                    // Check if our weapon has no intermediate anim...
                    switch (pSoldier.inv[InventorySlot.HANDPOS].usItem)
                    {
                        case Items.ROCKET_LAUNCHER:
                        case Items.MORTAR:
                        case Items.GLAUNCHER:

                            fDoFireRightAway = true;
                            break;
                    }

                    if (fDoFireRightAway)
                    {
                        // Set to true so we don't get toasted twice for APs..
                        pSoldier.fDontUnsetLastTargetFromTurn = true;

                        // Make sure we don't try and do fancy prone turning.....
                        pSoldier.fTurningFromPronePosition = false;

                        // Force our direction!
                        EVENT_SetSoldierDirection(pSoldier, pSoldier.bDesiredDirection);

                        EVENT_InitNewSoldierAnim(pSoldier, SelectFireAnimation(pSoldier, gAnimControl[pSoldier.usAnimState].ubEndHeight), 0, false);
                    }
                    else
                    {
                        // Set flag indicating we are about to shoot once destination direction is hit
                        pSoldier.fTurningToShoot = true;

                        if (pSoldier.bTeam != gbPlayerNum && pSoldier.bVisible != -1)
                        {
                            Overhead.LocateSoldier(pSoldier.ubID, DONTSETLOCATOR);
                        }
                    }
                }
            }
        }
    }

    //gAnimControl[ pSoldier.usAnimState ].ubEndHeight
    //					ChangeSoldierState( pSoldier, SHOOT_RIFLE_STAND, 0 , false );

    public static AnimationStates SelectFireAnimation(SOLDIERTYPE? pSoldier, AnimationHeights ubHeight)
    {
        int sDist;
        Items usItem;
        double dTargetX;
        double dTargetY;
        double dTargetZ;
        bool fDoLowShot = false;


        //Do different things if we are a monster
        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.MONSTER))
        {
            switch (pSoldier.ubBodyType)
            {
                case SoldierBodyTypes.ADULTFEMALEMONSTER:
                case SoldierBodyTypes.AM_MONSTER:
                case SoldierBodyTypes.YAF_MONSTER:
                case SoldierBodyTypes.YAM_MONSTER:
                    return (AnimationStates.MONSTER_SPIT_ATTACK);
                case SoldierBodyTypes.LARVAE_MONSTER:
                    break;
                case SoldierBodyTypes.INFANT_MONSTER:
                    return (AnimationStates.INFANT_ATTACK);
                case SoldierBodyTypes.QUEENMONSTER:
                    return (AnimationStates.QUEEN_SPIT);
            }

            return (AnimationStates.STANDING);
        }

        if (pSoldier.ubBodyType == SoldierBodyTypes.ROBOTNOWEAPON)
        {
            if (pSoldier.bDoBurst)
            {
                return (AnimationStates.ROBOT_BURST_SHOOT);
            }
            else
            {
                return (AnimationStates.ROBOT_SHOOT);
            }
        }

        // Check for rocket laucncher....
        if (pSoldier.inv[InventorySlot.HANDPOS].usItem == Items.ROCKET_LAUNCHER)
        {
            return (AnimationStates.SHOOT_ROCKET);
        }

        // Check for rocket laucncher....
        if (pSoldier.inv[InventorySlot.HANDPOS].usItem == Items.MORTAR)
        {
            return (AnimationStates.SHOOT_MORTAR);
        }

        // Check for tank cannon
        if (pSoldier.inv[InventorySlot.HANDPOS].usItem == Items.TANK_CANNON)
        {
            return (AnimationStates.TANK_SHOOT);
        }

        if (pSoldier.ubBodyType == TANK_NW || pSoldier.ubBodyType == TANK_NE)
        {
            return (AnimationStates.TANK_BURST);
        }

        // Determine which animation to do...depending on stance and gun in hand...
        switch (ubHeight)
        {
            case AnimationHeights.ANIM_STAND:

                usItem = pSoldier.inv[InventorySlot.HANDPOS].usItem;

                // CHECK 2ND HAND!
                if (IsValidSecondHandShot(pSoldier))
                {
                    // Increment the number of people busy doing stuff because of an attack
                    //gTacticalStatus.ubAttackBusyCount++;
                    //DebugMsg( TOPIC_JA2, DBG_LEVEL_3, String("!!!!!!! Starting attack with 2 guns, attack count now %d", gTacticalStatus.ubAttackBusyCount) );

                    return (AnimationStates.SHOOT_DUAL_STAND);
                }
                else
                {
                    // OK, while standing check distance away from target, and shoot low if we should!
                    sDist = IsometricUtils.PythSpacesAway(pSoldier.sGridNo, pSoldier.sTargetGridNo);

                    //ATE: OK, SEE WERE WE ARE TARGETING....
                    GetTargetWorldPositions(pSoldier, pSoldier.sTargetGridNo, out dTargetX, out dTargetY, out dTargetZ);

                    //CalculateSoldierZPos( pSoldier, FIRING_POS, &dFirerZ );

                    if (sDist <= 2 && dTargetZ <= 100)
                    {
                        fDoLowShot = true;
                    }

                    // ATE: Made distence away long for psitols such that they never use this....
                    //if ( !(Item[ usItem ].fFlags & ITEM_TWO_HANDED) )
                    //{
                    //	fDoLowShot = false;
                    //}

                    // Don't do any low shots if in water
                    if (MercInWater(pSoldier))
                    {
                        fDoLowShot = false;
                    }


                    if (pSoldier.bDoBurst)
                    {
                        if (fDoLowShot)
                        {
                            return (AnimationStates.FIRE_BURST_LOW_STAND);
                        }
                        else
                        {
                            return (AnimationStates.STANDING_BURST);
                        }
                    }
                    else
                    {
                        if (fDoLowShot)
                        {
                            return (AnimationStates.FIRE_LOW_STAND);
                        }
                        else
                        {
                            return (AnimationStates.SHOOT_RIFLE_STAND);
                        }
                    }
                }
                break;

            case AnimationHeights.ANIM_PRONE:

                if (pSoldier.bDoBurst)
                {
                    //				pSoldier.fBurstCompleted = false;
                    return (AnimationStates.PRONE_BURST);
                }
                else
                {
                    if (IsValidSecondHandShot(pSoldier))
                    {
                        return (AnimationStates.SHOOT_DUAL_PRONE);
                    }
                    else
                    {
                        return (AnimationStates.SHOOT_RIFLE_PRONE);
                    }
                }
                break;

            case AnimationHeights.ANIM_CROUCH:

                if (IsValidSecondHandShot(pSoldier))
                {
                    // Increment the number of people busy doing stuff because of an attack
                    //gTacticalStatus.ubAttackBusyCount++;
                    //DebugMsg( TOPIC_JA2, DBG_LEVEL_3, String("!!!!!!! Starting attack with 2 guns, attack count now %d", gTacticalStatus.ubAttackBusyCount) );

                    return (AnimationStates.SHOOT_DUAL_CROUCH);
                }
                else
                {
                    if (pSoldier.bDoBurst)
                    {
                        //				pSoldier.fBurstCompleted = false;
                        return (AnimationStates.CROUCHED_BURST);
                    }
                    else
                    {
                        return (AnimationStates.SHOOT_RIFLE_CROUCH);
                    }
                }
                break;

            default:
                //AssertMsg(false, string.Format("SelectFireAnimation: ERROR - Invalid height %d", ubHeight));
                break;
        }


        // If here, an internal error has occured!
        Debug.Assert(false);
        return (0);
    }


    AnimationStates GetMoveStateBasedOnStance(SOLDIERTYPE? pSoldier, AnimationHeights ubStanceHeight)
    {
        // Determine which animation to do...depending on stance and gun in hand...
        switch (ubStanceHeight)
        {
            case AnimationHeights.ANIM_STAND:
                if (pSoldier.fUIMovementFast && !(pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE)))
                {
                    return (AnimationStates.RUNNING);
                }
                else
                {
                    return (AnimationStates.WALKING);
                }
                break;

            case AnimationHeights.ANIM_PRONE:
                if (pSoldier.fUIMovementFast)
                {
                    return (AnimationStates.CRAWLING);
                }
                else
                {
                    return (AnimationStates.CRAWLING);
                }
                break;

            case AnimationHeights.ANIM_CROUCH:
                if (pSoldier.fUIMovementFast)
                {
                    return (AnimationStates.SWATTING);
                }
                else
                {
                    return (AnimationStates.SWATTING);
                }
                break;


            default:
                //AssertMsg(false, String("GetMoveStateBasedOnStance: ERROR - Invalid height %d", ubStanceHeight));
                break;
        }


        // If here, an internal error has occured!
        Debug.Assert(false);
        return (0);
    }


    void SelectFallAnimation(SOLDIERTYPE? pSoldier)
    {
        // Determine which animation to do...depending on stance and gun in hand...
        switch (gAnimControl[pSoldier.usAnimState].ubEndHeight)
        {
            case AnimationHeights.ANIM_STAND:
                EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.FLYBACK_HIT, 0, false);
                break;

            case AnimationHeights.ANIM_PRONE:
                EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.FLYBACK_HIT, 0, false);
                break;
        }

    }

    public static bool SoldierReadyWeapon(SOLDIERTYPE? pSoldier, int sTargetXPos, int sTargetYPos, bool fEndReady)
    {
        WorldDirections sFacingDir;

        sFacingDir = GetDirectionFromXY(sTargetXPos, sTargetYPos, pSoldier);

        return (InternalSoldierReadyWeapon(pSoldier, sFacingDir, fEndReady));
    }


    private static bool InternalSoldierReadyWeapon(SOLDIERTYPE? pSoldier, WorldDirections sFacingDir, bool fEndReady)
    {
        AnimationStates usAnimState;
        bool fReturnVal = false;

        // Handle monsters differently
        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.MONSTER))
        {
            if (!fEndReady)
            {
                EVENT_SetSoldierDesiredDirection(pSoldier, sFacingDir);
            }
            return (false);
        }

        usAnimState = PickSoldierReadyAnimation(pSoldier, fEndReady);

        if (usAnimState != AnimationStates.INVALID_ANIMATION)
        {
            EVENT_InitNewSoldierAnim(pSoldier, usAnimState, 0, false);
            fReturnVal = true;
        }

        if (!fEndReady)
        {
            // Ready direction for new facing direction
            if (usAnimState == AnimationStates.INVALID_ANIMATION)
            {
                usAnimState = pSoldier.usAnimState;
            }

            EVENT_InternalSetSoldierDesiredDirection(pSoldier, sFacingDir, false, usAnimState);

            // Check if facing dir is different from ours and change direction if so!
            //if ( sFacingDir != pSoldier.bDirection )
            //{
            //	DeductPoints( pSoldier, AP.CHANGE_FACING, 0 );
            //}//

        }

        return (fReturnVal);
    }

    public static AnimationStates PickSoldierReadyAnimation(SOLDIERTYPE? pSoldier, bool fEndReady)
    {

        // Invalid animation if nothing in our hands
        if (pSoldier.inv[InventorySlot.HANDPOS].usItem == NOTHING)
        {
            return (AnimationStates.INVALID_ANIMATION);
        }

        if (pSoldier.bOverTerrainType == TerrainTypeDefines.DEEP_WATER)
        {
            return (AnimationStates.INVALID_ANIMATION);
        }

        if (pSoldier.ubBodyType == SoldierBodyTypes.ROBOTNOWEAPON)
        {
            return (AnimationStates.INVALID_ANIMATION);
        }

        // Check if we have a gun.....
        if (Item[pSoldier.inv[InventorySlot.HANDPOS].usItem].usItemClass != IC.GUN && pSoldier.inv[InventorySlot.HANDPOS].usItem != Items.GLAUNCHER)
        {
            return (AnimationStates.INVALID_ANIMATION);
        }

        if (pSoldier.inv[InventorySlot.HANDPOS].usItem == Items.ROCKET_LAUNCHER)
        {
            return (AnimationStates.INVALID_ANIMATION);
        }

        if (pSoldier.ubBodyType == SoldierBodyTypes.TANK_NW || pSoldier.ubBodyType == SoldierBodyTypes.TANK_NE)
        {
            return (AnimationStates.INVALID_ANIMATION);
        }

        if (fEndReady)
        {
            // IF our gun is already drawn, do not change animation, just direction
            if (gAnimControl[pSoldier.usAnimState].uiFlags.HasFlag(ANIM.FIREREADY | ANIM.FIRE))
            {

                switch (gAnimControl[pSoldier.usAnimState].ubEndHeight)
                {
                    case AnimationHeights.ANIM_STAND:

                        // CHECK 2ND HAND!
                        if (IsValidSecondHandShot(pSoldier))
                        {
                            return (AnimationStates.END_DUAL_STAND);
                        }
                        else
                        {
                            return (AnimationStates.END_RIFLE_STAND);
                        }
                        break;

                    case AnimationHeights.ANIM_PRONE:

                        if (IsValidSecondHandShot(pSoldier))
                        {
                            return (AnimationStates.END_DUAL_PRONE);
                        }
                        else
                        {
                            return (AnimationStates.END_RIFLE_PRONE);
                        }
                        break;

                    case AnimationHeights.ANIM_CROUCH:

                        // CHECK 2ND HAND!
                        if (IsValidSecondHandShot(pSoldier))
                        {
                            return (AnimationStates.END_DUAL_CROUCH);
                        }
                        else
                        {
                            return (AnimationStates.END_RIFLE_CROUCH);
                        }
                }
            }
        }
        else
        {
            // IF our gun is already drawn, do not change animation, just direction
            if (!(gAnimControl[pSoldier.usAnimState].uiFlags.HasFlag(ANIM.FIREREADY | ANIM.FIRE)))
            {

                {
                    switch (gAnimControl[pSoldier.usAnimState].ubEndHeight)
                    {
                        case AnimationHeights.ANIM_STAND:

                            // CHECK 2ND HAND!
                            if (IsValidSecondHandShot(pSoldier))
                            {
                                return (AnimationStates.READY_DUAL_STAND);
                            }
                            else
                            {
                                return (AnimationStates.READY_RIFLE_STAND);
                            }
                            break;

                        case AnimationHeights.ANIM_PRONE:
                            // Go into crouch, turn, then go into prone again
                            //ChangeSoldierStance( pSoldier, ANIM_CROUCH );
                            //pSoldier.ubDesiredHeight = ANIM_PRONE;
                            //ChangeSoldierState( pSoldier, PRONE_UP );
                            if (IsValidSecondHandShot(pSoldier))
                            {
                                return (AnimationStates.READY_DUAL_PRONE);
                            }
                            else
                            {
                                return (AnimationStates.READY_RIFLE_PRONE);
                            }
                            break;

                        case AnimationHeights.ANIM_CROUCH:

                            // CHECK 2ND HAND!
                            if (IsValidSecondHandShot(pSoldier))
                            {
                                return (AnimationStates.READY_DUAL_CROUCH);
                            }
                            else
                            {
                                return (AnimationStates.READY_RIFLE_CROUCH);
                            }
                    }
                }

            }
        }

        return (AnimationStates.INVALID_ANIMATION);
    }

    // ATE: THIS FUNCTION IS USED FOR ALL SOLDIER TAKE DAMAGE FUNCTIONS!
    public static void EVENT_SoldierGotHit(SOLDIERTYPE? pSoldier, Items usWeaponIndex, int sDamage, int sBreathLoss, WorldDirections bDirection, int sRange, int ubAttackerID, int ubSpecial, int ubHitLocation, int sSubsequent, int sLocationGrid)
    {
        int ubCombinedLoss, ubVolume;
        TAKE_DAMAGE ubReason;
        SOLDIERTYPE? pNewSoldier;

        ubReason = 0;

        // ATE: If we have gotten hit, but are still in our attack animation, reduce count!
        switch (pSoldier.usAnimState)
        {
            case AnimationStates.SHOOT_ROCKET:
            case AnimationStates.SHOOT_MORTAR:
            case AnimationStates.THROW_ITEM:
            case AnimationStates.LOB_ITEM:

                // DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("@@@@@@@ Freeing up attacker - ATTACK ANIMATION %s ENDED BY HIT ANIMATION, Now %d", gAnimControl[pSoldier.usAnimState].zAnimStr, gTacticalStatus.ubAttackBusyCount));
                ReduceAttackBusyCount(pSoldier.ubID, false);
                break;
        }

        // DO STUFF COMMON FOR ALL TYPES
        if (ubAttackerID != NOBODY)
        {
            MercPtrs[ubAttackerID].bLastAttackHit = 1;
        }

        // Set attacker's ID
        pSoldier.ubAttackerID = ubAttackerID;

        if (!(pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE)))
        {
            // Increment  being attacked count
            pSoldier.bBeingAttackedCount++;
        }

        // if defender is a vehicle, there will be no hit animation played!
        if (!(pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE)))
        {
            // Increment the number of people busy doing stuff because of an attack (busy doing hit anim!)
            gTacticalStatus.ubAttackBusyCount++;
            //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("!!!!!!! Person got hit, attack count now %d", gTacticalStatus.ubAttackBusyCount));
        }

        // ATE; Save hit location info...( for later anim determination stuff )
        pSoldier.ubHitLocation = ubHitLocation;

        // handle morale for heavy damage attacks
        if (sDamage > 25)
        {
            if (pSoldier.ubAttackerID != NOBODY && MercPtrs[pSoldier.ubAttackerID].bTeam == gbPlayerNum)
            {
                Morale.HandleMoraleEvent(MercPtrs[pSoldier.ubAttackerID], MoraleEventNames.MORALE_DID_LOTS_OF_DAMAGE, MercPtrs[pSoldier.ubAttackerID].sSectorX, MercPtrs[pSoldier.ubAttackerID].sSectorY, MercPtrs[pSoldier.ubAttackerID].bSectorZ);
            }
            if (pSoldier.bTeam == gbPlayerNum)
            {
                Morale.HandleMoraleEvent(pSoldier, MoraleEventNames.MORALE_TOOK_LOTS_OF_DAMAGE, pSoldier.sSectorX, pSoldier.sSectorY, pSoldier.bSectorZ);
            }
        }

        // SWITCH IN TYPE OF WEAPON
        if (ubSpecial == FIRE_WEAPON_TOSSED_OBJECT_SPECIAL)
        {
            ubReason = TAKE_DAMAGE.OBJECT;
        }
        else if (Item[usWeaponIndex].usItemClass.HasFlag(IC.TENTACLES))
        {
            ubReason = TAKE_DAMAGE.TENTACLES;
        }
        else if (Item[usWeaponIndex].usItemClass.HasFlag((IC.GUN | IC.THROWING_KNIFE)))
        {
            if (ubSpecial == FIRE_WEAPON_SLEEP_DART_SPECIAL)
            {
                int uiChance;

                // put the drug in!
                pSoldier.bSleepDrugCounter = 10;

                uiChance = SleepDartSuccumbChance(pSoldier);

                if (PreRandom(100) < uiChance)
                {
                    // succumb to the drug!
                    sBreathLoss = (int)(pSoldier.bBreathMax * 100);
                }

            }
            else if (ubSpecial == FIRE_WEAPON_BLINDED_BY_SPIT_SPECIAL)
            {
                // blinded!!
                if ((pSoldier.bBlindedCounter == 0))
                {
                    // say quote
                    if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.PC))
                    {
                        DialogControl.TacticalCharacterDialogue(pSoldier, QUOTE.BLINDED);
                    }

                    DecayIndividualOpplist(pSoldier);
                }
                // will always increase counter by at least 1
                pSoldier.bBlindedCounter += (sDamage / 8) + 1;

                // Dirty panel
                fInterfacePanelDirty = DIRTYLEVEL2;
            }
            sBreathLoss += BP.GET_HIT;
            ubReason = TAKE_DAMAGE.GUNFIRE;
        }
        else if (Item[usWeaponIndex].usItemClass.HasFlag(IC.BLADE))
        {
            sBreathLoss = BP.GET_HIT;
            ubReason = TAKE_DAMAGE.BLADE;
        }
        else if (Item[usWeaponIndex].usItemClass.HasFlag(IC.PUNCH))
        {
            // damage from hand-to-hand is 1/4 normal, 3/4 breath.. the sDamage value
            // is actually how much breath we'll take away
            sBreathLoss = sDamage * 100;
            sDamage = sDamage / PUNCH_REAL_DAMAGE_PORTION;
            if (Meanwhile.AreInMeanwhile() && gCurrentMeanwhileDef.ubMeanwhileID == Meanwhiles.INTERROGATION)
            {
                sBreathLoss = 0;
                sDamage /= 2;
            }
            ubReason = TAKE_DAMAGE.HANDTOHAND;
        }
        else if (Item[usWeaponIndex].usItemClass.HasFlag(IC.EXPLOSV))
        {
            if (usWeaponIndex == Items.STRUCTURE_EXPLOSION)
            {
                ubReason = TAKE_DAMAGE.STRUCTURE_EXPLOSION;
            }
            else
            {
                ubReason = TAKE_DAMAGE.EXPLOSION;
            }
        }
        else
        {
            //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("Soldier Control: Weapon class not handled in SoldierGotHit( ) %d", usWeaponIndex));
        }


        // CJC: moved to after SoldierTakeDamage so that any quotes from the defender
        // will not be said if they are knocked out or killed
        if (ubReason != TAKE_DAMAGE.TENTACLES && ubReason != TAKE_DAMAGE.OBJECT)
        {
            // OK, OK: THis is hairy, however, it's ness. because the normal freeup call uses the
            // attckers intended target, and here we want to use thier actual target....

            // ATE: If it's from GUNFIRE damage, keep in mind bullets...
            if (Item[usWeaponIndex].usItemClass.HasFlag(IC.GUN))
            {
                pNewSoldier = FreeUpAttackerGivenTarget(pSoldier.ubAttackerID, pSoldier.ubID);
            }
            else
            {
                pNewSoldier = ReduceAttackBusyGivenTarget(pSoldier.ubAttackerID, pSoldier.ubID);
            }

            if (pNewSoldier != null)
            {
                pSoldier = pNewSoldier;
            }
            //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("!!!!!!! Tried to free up attacker, attack count now %d", gTacticalStatus.ubAttackBusyCount));
        }


        // OK, If we are a vehicle.... damage vehicle...( people inside... )
        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE))
        {
            SoldierTakeDamage(pSoldier, AnimationHeights.ANIM_CROUCH, sDamage, sBreathLoss, ubReason, pSoldier.ubAttackerID, NOWHERE, false, true);
            return;
        }

        // DEDUCT LIFE
        ubCombinedLoss = SoldierTakeDamage(pSoldier, AnimationHeights.ANIM_CROUCH, sDamage, sBreathLoss, ubReason, pSoldier.ubAttackerID, NOWHERE, false, true);

        // ATE: OK, Let's check our ASSIGNMENT state,
        // If anything other than on a squad or guard, make them guard....
        if (pSoldier.bTeam == gbPlayerNum)
        {
            if (pSoldier.bAssignment >= Assignments.ON_DUTY && pSoldier.bAssignment != Assignments.ASSIGNMENT_POW)
            {
                if (pSoldier.fMercAsleep)
                {
                    pSoldier.fMercAsleep = false;
                    pSoldier.fForcedToStayAwake = false;

                    // refresh map screen
                    fCharacterInfoPanelDirty = true;
                    fTeamPanelDirty = true;
                }

                AddCharacterToAnySquad(pSoldier);
            }
        }


        // SCREAM!!!!
        ubVolume = CalcScreamVolume(pSoldier, ubCombinedLoss);

        // IF WE ARE AT A HIT_STOP ANIMATION
        // DO APPROPRIATE HITWHILE DOWN ANIMATION
        if (!(gAnimControl[pSoldier.usAnimState].uiFlags.HasFlag(ANIM.HITSTOP)) || pSoldier.usAnimState != AnimationStates.JFK_HITDEATH_STOP)
        {
            MakeNoise(pSoldier.ubID, pSoldier.sGridNo, pSoldier.bLevel, pSoldier.bOverTerrainType, ubVolume, NOISE.SCREAM);
        }

        // IAN ADDED THIS SAT JUNE 14th : HAVE TO SHOW VICTIM!
        if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED)
            && (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
            && pSoldier.bVisible != -1
            && pSoldier.bTeam == gbPlayerNum)
        {
            Overhead.LocateSoldier(pSoldier.ubID, DONTSETLOCATOR);
        }

        if (Item[usWeaponIndex].usItemClass.HasFlag(IC.BLADE))
        {
            //PlayJA2Sample((int)(KNIFE_IMPACT), RATE_11025, SoundVolume(MIDVOLUME, pSoldier.sGridNo), 1, SoundDir(pSoldier.sGridNo));
        }
        else
        {
            //PlayJA2Sample((int)(BULLET_IMPACT_1 + Globals.Random.Next(3)), RATE_11025, SoundVolume(MIDVOLUME, pSoldier.sGridNo), 1, SoundDir(pSoldier.sGridNo));
        }

        // PLAY RANDOM GETTING HIT SOUND
        // ONLY IF WE ARE CONSCIOUS!
        if (pSoldier.bLife >= CONSCIOUSNESS)
        {
            if (pSoldier.ubBodyType == SoldierBodyTypes.CROW)
            {
                // Exploding crow...
                //PlayJA2Sample(CROW_EXPLODE_1, RATE_11025, SoundVolume(HIGHVOLUME, pSoldier.sGridNo), 1, SoundDir(pSoldier.sGridNo));
            }
            else
            {
                // ATE: This is to disallow large amounts of smaples being played which is load!
                if (pSoldier.fGettingHit && pSoldier.usAniCode != STANDING_BURST_HIT)
                {

                }
                else
                {
                    DoMercBattleSound(pSoldier, (int)(BATTLE_SOUND_HIT1 + Globals.Random.Next(2)));
                }
            }
        }

        // CHECK FOR DOING HIT WHILE DOWN
        if ((gAnimControl[pSoldier.usAnimState].uiFlags.HasFlag(ANIM.HITSTOP)))
        {
            switch (pSoldier.usAnimState)
            {
                case AnimationStates.FLYBACKHIT_STOP:
                    ChangeSoldierState(pSoldier, AnimationStates.FALLBACK_DEATHTWICH, 0, false);
                    break;

                case AnimationStates.STAND_FALLFORWARD_STOP:
                    ChangeSoldierState(pSoldier, AnimationStates.GENERIC_HIT_DEATHTWITCHNB, 0, false);
                    break;

                case AnimationStates.JFK_HITDEATH_STOP:
                    ChangeSoldierState(pSoldier, AnimationStates.JFK_HITDEATH_TWITCHB, 0, false);
                    break;

                case AnimationStates.FALLBACKHIT_STOP:
                    ChangeSoldierState(pSoldier, AnimationStates.FALLBACK_HIT_DEATHTWITCHNB, 0, false);
                    break;

                case AnimationStates.PRONE_LAYFROMHIT_STOP:
                    ChangeSoldierState(pSoldier, AnimationStates.PRONE_HIT_DEATHTWITCHNB, 0, false);
                    break;

                case AnimationStates.PRONE_HITDEATH_STOP:
                    ChangeSoldierState(pSoldier, AnimationStates.PRONE_HIT_DEATHTWITCHB, 0, false);
                    break;

                case AnimationStates.FALLFORWARD_HITDEATH_STOP:
                    ChangeSoldierState(pSoldier, AnimationStates.GENERIC_HIT_DEATHTWITCHB, 0, false);
                    break;

                case AnimationStates.FALLBACK_HITDEATH_STOP:
                    ChangeSoldierState(pSoldier, AnimationStates.FALLBACK_HIT_DEATHTWITCHB, 0, false);
                    break;

                case AnimationStates.FALLOFF_DEATH_STOP:
                    ChangeSoldierState(pSoldier, AnimationStates.FALLOFF_TWITCHB, 0, false);
                    break;

                case AnimationStates.FALLOFF_STOP:
                    ChangeSoldierState(pSoldier, AnimationStates.FALLOFF_TWITCHNB, 0, false);
                    break;

                case AnimationStates.FALLOFF_FORWARD_DEATH_STOP:
                    ChangeSoldierState(pSoldier, AnimationStates.FALLOFF_FORWARD_TWITCHB, 0, false);
                    break;

                case AnimationStates.FALLOFF_FORWARD_STOP:
                    ChangeSoldierState(pSoldier, AnimationStates.FALLOFF_FORWARD_TWITCHNB, 0, false);
                    break;

                default:
                    //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("Soldier Control: Death state %d has no death hit", pSoldier.usAnimState));
                    break;
            }
            return;
        }

        // Set goback to aim after hit flag!
        // Only if we were aiming!
        if (gAnimControl[pSoldier.usAnimState].uiFlags.HasFlag(ANIM.FIREREADY))
        {
            pSoldier.fGoBackToAimAfterHit = true;
        }

        // IF COWERING, PLAY SPECIFIC GENERIC HIT STAND...
        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.COWERING))
        {
            if (pSoldier.bLife == 0 || IS_MERC_BODY_TYPE(pSoldier))
            {
                EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.GENERIC_HIT_STAND, 0, false);
            }
            else
            {
                EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.CIV_COWER_HIT, 0, false);
            }
            return;
        }

        // Change based on body type
        switch (pSoldier.ubBodyType)
        {
            case SoldierBodyTypes.COW:
                EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.COW_HIT, 0, false);
                return;
                break;

            case SoldierBodyTypes.BLOODCAT:
                EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.BLOODCAT_HIT, 0, false);
                return;
                break;

            case SoldierBodyTypes.ADULTFEMALEMONSTER:
            case SoldierBodyTypes.AM_MONSTER:
            case SoldierBodyTypes.YAF_MONSTER:
            case SoldierBodyTypes.YAM_MONSTER:

                EVENT_InitNewSoldierAnim(pSoldier, ADULTMONSTER_HIT, 0, false);
                return;
                break;

            case SoldierBodyTypes.LARVAE_MONSTER:
                EVENT_InitNewSoldierAnim(pSoldier, LARVAE_HIT, 0, false);
                return;
                break;

            case SoldierBodyTypes.QUEENMONSTER:
                EVENT_InitNewSoldierAnim(pSoldier, QUEEN_HIT, 0, false);
                return;
                break;

            case SoldierBodyTypes.CRIPPLECIV:

                {
                    // OK, do some code here to allow the fact that poor buddy can be thrown back if it's a big enough hit...
                    EVENT_InitNewSoldierAnim(pSoldier, CRIPPLE_HIT, 0, false);

                    //pSoldier.bLife = 0;
                    //EVENT_InitNewSoldierAnim( pSoldier, CRIPPLE_DIE_FLYBACK, 0 , false );


                }
                return;
                break;

            case SoldierBodyTypes.ROBOTNOWEAPON:
                EVENT_InitNewSoldierAnim(pSoldier, ROBOTNW_HIT, 0, false);
                return;
                break;


            case SoldierBodyTypes.INFANT_MONSTER:
                EVENT_InitNewSoldierAnim(pSoldier, INFANT_HIT, 0, false);
                return;

            case SoldierBodyTypes.CROW:

                EVENT_InitNewSoldierAnim(pSoldier, CROW_DIE, 0, false);
                return;

            //case FATCIV:
            case SoldierBodyTypes.MANCIV:
            case SoldierBodyTypes.MINICIV:
            case SoldierBodyTypes.DRESSCIV:
            case SoldierBodyTypes.HATKIDCIV:
            case SoldierBodyTypes.KIDCIV:

                // OK, if life is 0 and not set as dead ( this is a death hit... )
                if (!(pSoldier.uiStatusFlags.HasFlag(SOLDIER.DEAD)) && pSoldier.bLife == 0)
                {
                    // Randomize death!
                    if (Globals.Random.Next(2) > 0)
                    {
                        EVENT_InitNewSoldierAnim(pSoldier, CIV_DIE2, 0, false);
                        return;
                    }
                }

                // IF here, go generic hit ALWAYS.....
                EVENT_InitNewSoldierAnim(pSoldier, GENERIC_HIT_STAND, 0, false);
                return;
                break;
        }

        // If here, we are a merc, check if we are in water
        if (pSoldier.bOverTerrainType == LOW_WATER)
        {
            EVENT_InitNewSoldierAnim(pSoldier, WATER_HIT, 0, false);
            return;
        }
        if (pSoldier.bOverTerrainType == DEEP_WATER)
        {
            EVENT_InitNewSoldierAnim(pSoldier, DEEP_WATER_HIT, 0, false);
            return;
        }


        // SWITCH IN TYPE OF WEAPON
        if (Item[usWeaponIndex].usItemClass & (IC.GUN | IC.THROWING_KNIFE))
        {
            SoldierGotHitGunFire(pSoldier, usWeaponIndex, sDamage, bDirection, sRange, ubAttackerID, ubSpecial, ubHitLocation);
        }
        if (Item[usWeaponIndex].usItemClass.HasFlag(IC.BLADE))
        {
            SoldierGotHitBlade(pSoldier, usWeaponIndex, sDamage, bDirection, sRange, ubAttackerID, ubSpecial, ubHitLocation);
        }
        if (Item[usWeaponIndex].usItemClass.HasFlag(IC.EXPLOSV || Item[usWeaponIndex].usItemClass & IC_TENTACLES))
        {
            SoldierGotHitExplosion(pSoldier, usWeaponIndex, sDamage, bDirection, sRange, ubAttackerID, ubSpecial, ubHitLocation);
        }
        if (Item[usWeaponIndex].usItemClass.HasFlag(IC.PUNCH))
        {
            SoldierGotHitPunch(pSoldier, usWeaponIndex, sDamage, bDirection, sRange, ubAttackerID, ubSpecial, ubHitLocation);
        }

    }

    int CalcScreamVolume(SOLDIERTYPE? pSoldier, int ubCombinedLoss)
    {
        // NB explosions are so loud they should drown out screams
        int ubVolume;

        if (ubCombinedLoss < 1)
        {
            ubVolume = 1;
        }
        else
        {
            ubVolume = ubCombinedLoss;
        }

        // Victim yells out in pain, making noise.  Yelps are louder from greater
        // wounds, but softer for more experienced soldiers.

        if (ubVolume > (10 - SkillChecks.EffectiveExpLevel(pSoldier)))
        {
            ubVolume = 10 - SkillChecks.EffectiveExpLevel(pSoldier);
        }

        /*
                // the "Speck factor"...  He's a whiner, and extra-sensitive to pain!
                if (ptr.trait == NERVOUS)
                    ubVolume += 2;
        */

        if (ubVolume < 0)
        {
            ubVolume = 0;
        }

        return (ubVolume);
    }


    void DoGenericHit(SOLDIERTYPE? pSoldier, int ubSpecial, WorldDirections bDirection)
    {
        // Based on stance, select generic hit animation
        switch (gAnimControl[pSoldier.usAnimState].ubEndHeight)
        {
            case AnimationHeights.ANIM_STAND:
                // For now, check if we are affected by a burst
                // For now, if the weapon was a gun, special 1 == burst
                // ATE: Only do this for mercs!
                if (ubSpecial == FIRE_WEAPON_BURST_SPECIAL && pSoldier.ubBodyType <= REGFEMALE)
                {
                    //SetSoldierDesiredDirection( pSoldier, bDirection );
                    EVENT_SetSoldierDirection(pSoldier, bDirection);
                    EVENT_SetSoldierDesiredDirection(pSoldier, pSoldier.bDirection);

                    EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.STANDING_BURST_HIT, 0, false);
                }
                else
                {
                    // Check in hand for rifle
                    if (SoldierCarriesTwoHandedWeapon(pSoldier))
                    {
                        EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.RIFLE_STAND_HIT, 0, false);
                    }
                    else
                    {
                        EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.GENERIC_HIT_STAND, 0, false);
                    }
                }
                break;

            case AnimationHeights.ANIM_PRONE:

                EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.GENERIC_HIT_PRONE, 0, false);
                break;

            case AnimationHeights.ANIM_CROUCH:
                EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.GENERIC_HIT_CROUCH, 0, false);
                break;

        }
    }


    void SoldierGotHitGunFire(SOLDIERTYPE? pSoldier, Items usWeaponIndex, int sDamage, WorldDirections bDirection, int sRange, int ubAttackerID, int ubSpecial, int ubHitLocation)
    {
        int usNewGridNo;
        bool fBlownAway = false;
        bool fHeadHit = false;
        bool fFallenOver = false;

        // MAYBE CHANGE TO SPECIAL ANIMATION BASED ON VALUE SET BY DAMAGE CALCULATION CODE
        // ALL THESE ONLY WORK ON STANDING PEOPLE
        if (!(pSoldier.uiStatusFlags.HasFlag(SOLDIER.MONSTER)) && gAnimControl[pSoldier.usAnimState].ubEndHeight == AnimationHeights.ANIM_STAND)
        {
            if (gAnimControl[pSoldier.usAnimState].ubEndHeight == AnimationHeights.ANIM_STAND)
            {
                if (ubSpecial == FIRE_WEAPON_HEAD_EXPLODE_SPECIAL)
                {
                    if (GameSettings.fOptions[TOPTION.BLOOD_N_GORE])
                    {
                        if (IsometricUtils.SpacesAway(pSoldier.sGridNo, Menptr[ubAttackerID].sGridNo) <= MAX_DISTANCE_FOR_MESSY_DEATH)
                        {
                            usNewGridNo = IsometricUtils.NewGridNo((int)pSoldier.sGridNo, (int)(IsometricUtils.DirectionInc(pSoldier.bDirection)));

                            // CHECK OK DESTINATION!
                            if (OKFallDirection(pSoldier, usNewGridNo, pSoldier.bLevel, pSoldier.bDirection, AnimationStates.JFK_HITDEATH))
                            {
                                usNewGridNo = IsometricUtils.NewGridNo((int)usNewGridNo, (int)(IsometricUtils.DirectionInc(pSoldier.bDirection)));

                                if (OKFallDirection(pSoldier, usNewGridNo, pSoldier.bLevel, pSoldier.bDirection, pSoldier.usAnimState))
                                {
                                    fHeadHit = true;
                                }
                            }
                        }
                    }
                }
                else if (ubSpecial == FIRE_WEAPON_CHEST_EXPLODE_SPECIAL)
                {
                    if (GameSettings.fOptions[TOPTION.BLOOD_N_GORE])
                    {
                        if (IsometricUtils.SpacesAway(pSoldier.sGridNo, Menptr[ubAttackerID].sGridNo) <= MAX_DISTANCE_FOR_MESSY_DEATH)
                        {

                            // possibly play torso explosion anim!
                            if (pSoldier.bDirection == bDirection)
                            {
                                usNewGridNo = IsometricUtils.NewGridNo(pSoldier.sGridNo, IsometricUtils.DirectionInc(gOppositeDirection[pSoldier.bDirection]));

                                if (OKFallDirection(pSoldier, usNewGridNo, pSoldier.bLevel, gOppositeDirection[bDirection], FLYBACK_HIT))
                                {
                                    usNewGridNo = IsometricUtils.NewGridNo(usNewGridNo, IsometricUtils.DirectionInc(gOppositeDirection[bDirection]));

                                    if (OKFallDirection(pSoldier, usNewGridNo, pSoldier.bLevel, gOppositeDirection[bDirection], pSoldier.usAnimState))
                                    {
                                        fBlownAway = true;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (ubSpecial == FIRE_WEAPON_LEG_FALLDOWN_SPECIAL)
                {
                    // possibly play fall over anim!
                    // this one is NOT restricted by distance
                    if (IsValidStance(pSoldier, AnimationHeights.ANIM_PRONE))
                    {
                        // Can't be in water, or not standing
                        if (gAnimControl[pSoldier.usAnimState].ubEndHeight == AnimationHeights.ANIM_STAND && !MercInWater(pSoldier))
                        {
                            fFallenOver = true;
                            Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG_INTERFACE, gzLateLocalizedString[20], pSoldier.name);
                        }
                    }
                }
            }
        }

        // IF HERE AND GUY IS DEAD, RETURN!
        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.DEAD))
        {
            //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("@@@@@@@ Releasesoldierattacker,Dead soldier hit"));
            ReleaseSoldiersAttacker(pSoldier);
            return;
        }

        if (fFallenOver)
        {
            SoldierCollapse(pSoldier);
            return;
        }

        if (fBlownAway)
        {
            // Only for mercs...
            if (pSoldier.ubBodyType < (SoldierBodyTypes)4)
            {
                ChangeToFlybackAnimation(pSoldier, (int)bDirection);
                return;
            }
        }

        if (fHeadHit)
        {
            // Only for mercs ( or KIDS! )
            if (pSoldier.ubBodyType < (SoldierBodyTypes)4 || pSoldier.ubBodyType == SoldierBodyTypes.HATKIDCIV || pSoldier.ubBodyType == SoldierBodyTypes.KIDCIV)
            {
                EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.JFK_HITDEATH, 0, false);
                return;
            }
        }

        DoGenericHit(pSoldier, ubSpecial, bDirection);

    }

    void SoldierGotHitExplosion(SOLDIERTYPE? pSoldier, Items usWeaponIndex, int sDamage, WorldDirections bDirection, int sRange, int ubAttackerID, int ubSpecial, int ubHitLocation)
    {
        int sNewGridNo;

        // IF HERE AND GUY IS DEAD, RETURN!
        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.DEAD))
        {
            return;
        }

        //check for services
        ReceivingSoldierCancelServices(pSoldier);
        GivingSoldierCancelServices(pSoldier);


        if (GameSettings.fOptions[TOPTION.BLOOD_N_GORE])
        {
            if (Explosive[Item[usWeaponIndex].ubClassIndex].ubRadius >= 3 && pSoldier.bLife == 0 && gAnimControl[pSoldier.usAnimState].ubEndHeight != ANIM_PRONE)
            {
                if (sRange >= 2 && sRange <= 4)
                {
                    DoMercBattleSound(pSoldier, (int)(BATTLE_SOUND_HIT1 + Globals.Random.Next(2)));

                    EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.CHARIOTS_OF_FIRE, 0, false);
                    return;
                }
                else if (sRange <= 1)
                {
                    DoMercBattleSound(pSoldier, (int)(BATTLE_SOUND_HIT1 + Globals.Random.Next(2)));

                    EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.BODYEXPLODING, 0, false);
                    return;
                }
            }
        }

        // If we can't fal back or such, so generic hit...
        if (pSoldier.ubBodyType >= (SoldierBodyTypes)4)
        {
            DoGenericHit(pSoldier, 0, bDirection);
            return;
        }

        // Based on stance, select generic hit animation
        switch (gAnimControl[pSoldier.usAnimState].ubEndHeight)
        {
            case AnimationHeights.ANIM_STAND:
            case AnimationHeights.ANIM_CROUCH:

                EVENT_SetSoldierDirection(pSoldier, bDirection);
                EVENT_SetSoldierDesiredDirection(pSoldier, pSoldier.bDirection);

                // Check behind us!
                sNewGridNo = IsometricUtils.NewGridNo((int)pSoldier.sGridNo, IsometricUtils.DirectionInc(gOppositeDirection[bDirection]));

                if (OKFallDirection(pSoldier, sNewGridNo, pSoldier.bLevel, gOppositeDirection[bDirection], FLYBACK_HIT))
                {
                    ChangeToFallbackAnimation(pSoldier, (int)bDirection);
                }
                else
                {
                    if (gAnimControl[pSoldier.usAnimState].ubEndHeight == AnimationHeights.ANIM_STAND)
                    {
                        BeginTyingToFall(pSoldier);
                        EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.FALLFORWARD_FROMHIT_STAND, 0, false);
                    }
                    else
                    {
                        SoldierCollapse(pSoldier);
                    }
                }
                break;

            case AnimationHeights.ANIM_PRONE:

                SoldierCollapse(pSoldier);
                break;
        }

    }


    void SoldierGotHitBlade(SOLDIERTYPE? pSoldier, int usWeaponIndex, int sDamage, int bDirection, int sRange, int ubAttackerID, int ubSpecial, int ubHitLocation)
    {

        // IF HERE AND GUY IS DEAD, RETURN!
        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.DEAD))
        {
            return;
        }


        // Based on stance, select generic hit animation
        switch (gAnimControl[pSoldier.usAnimState].ubEndHeight)
        {
            case AnimationHeights.ANIM_STAND:

                // Check in hand for rifle
                if (SoldierCarriesTwoHandedWeapon(pSoldier))
                {
                    EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.RIFLE_STAND_HIT, 0, false);
                }
                else
                {
                    EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.GENERIC_HIT_STAND, 0, false);
                }
                break;

            case AnimationHeights.ANIM_CROUCH:
                EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.GENERIC_HIT_CROUCH, 0, false);
                break;

            case AnimationHeights.ANIM_PRONE:
                EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.GENERIC_HIT_PRONE, 0, false);
                break;
        }

    }


    void SoldierGotHitPunch(SOLDIERTYPE? pSoldier, int usWeaponIndex, int sDamage, int bDirection, int sRange, int ubAttackerID, int ubSpecial, int ubHitLocation)
    {

        // IF HERE AND GUY IS DEAD, RETURN!
        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.DEAD))
        {
            return;
        }

        // Based on stance, select generic hit animation
        switch (gAnimControl[pSoldier.usAnimState].ubEndHeight)
        {
            case AnimationHeights.ANIM_STAND:
                // Check in hand for rifle
                if (SoldierCarriesTwoHandedWeapon(pSoldier))
                {
                    EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.RIFLE_STAND_HIT, 0, false);
                }
                else
                {
                    EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.GENERIC_HIT_STAND, 0, false);
                }
                break;

            case AnimationHeights.ANIM_CROUCH:
                EVENT_InitNewSoldierAnim(pSoldier, GENERIC_HIT_CROUCH, 0, false);
                break;

            case AnimationHeights.ANIM_PRONE:
                EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.GENERIC_HIT_PRONE, 0, false);
                break;

        }

    }

    public static bool EVENT_InternalGetNewSoldierPath(SOLDIERTYPE? pSoldier, int sDestGridNo, AnimationStates usMovementAnim, int fFromUI, bool fForceRestartAnim)
    {
        int iDest;
        int sNewGridNo;
        bool fContinue;
        int uiDist;
        AnimationStates usAnimState;
        AnimationStates usMoveAnimState = usMovementAnim;
        int sMercGridNo;
        int[] usPathingData = new int[MAX_PATH_LIST_SIZE];
        int ubPathingMaxDirection;
        bool fAdvancePath = true;
        PATH fFlags = 0;

        // Ifd this code, make true if a player
        if (fFromUI == 3)
        {
            if (pSoldier.bTeam == gbPlayerNum)
            {
                fFromUI = 1;
            }
            else
            {
                fFromUI = 0;
            }
        }

        // ATE: if a civ, and from UI, and were cowering, remove from cowering
        if (AM_AN_EPC(pSoldier) && fFromUI > 0)
        {
            if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.COWERING))
            {
                SetSoldierCowerState(pSoldier, false);
                usMoveAnimState = AnimationStates.WALKING;
            }
        }


        pSoldier.bGoodContPath = 0;

        if (pSoldier.fDelayedMovement > 0)
        {
            if (pSoldier.ubDelayedMovementFlags.HasFlag(DELAYED_MOVEMENT_FLAG.PATH_THROUGH_PEOPLE))
            {
                fFlags = PATH.THROUGH_PEOPLE;
            }
            else
            {
                fFlags = PATH.IGNORE_PERSON_AT_DEST;
            }

            pSoldier.fDelayedMovement = 0;
        }

        if (gfGetNewPathThroughPeople)
        {
            fFlags = PATH.THROUGH_PEOPLE;
        }

        // ATE: Some stuff here for realtime, going through interface....
        if ((!(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
            && (gAnimControl[pSoldier.usAnimState].uiFlags.HasFlag(ANIM.MOVING)) && fFromUI == 1) || fFromUI == 2)
        {
            if (pSoldier.bCollapsed)
            {
                return (false);
            }

            sMercGridNo = pSoldier.sGridNo;
            pSoldier.sGridNo = pSoldier.sDestination;

            // Check if path is good before copying it into guy's path...
            if (FindBestPath(pSoldier, sDestGridNo, pSoldier.bLevel, pSoldier.usUIMovementMode, NO_COPYROUTE, fFlags) == 0)
            {
                // Set to old....
                pSoldier.sGridNo = sMercGridNo;

                return (false);
            }

            uiDist = FindBestPath(pSoldier, sDestGridNo, pSoldier.bLevel, pSoldier.usUIMovementMode, COPYROUTE, fFlags);

            pSoldier.sGridNo = sMercGridNo;
            pSoldier.sFinalDestination = sDestGridNo;

            if (uiDist > 0)
            {
                // Add one to path data size....
                if (fAdvancePath)
                {
                    memcpy(usPathingData, pSoldier.usPathingData, sizeof(usPathingData));
                    ubPathingMaxDirection = (int)usPathingData[MAX_PATH_LIST_SIZE - 1];
                    memcpy((pSoldier.usPathingData[1]), usPathingData, sizeof(usPathingData) - sizeof(int));

                    // If we have reach the max, go back one sFinalDest....
                    if (pSoldier.usPathDataSize == MAX_PATH_LIST_SIZE)
                    {
                        //pSoldier.sFinalDestination = NewGridNo( (int)pSoldier.sFinalDestination, DirectionInc( gOppositeDirection[ ubPathingMaxDirection ] ) );
                    }
                    else
                    {
                        pSoldier.usPathDataSize++;
                    }
                }

                usMoveAnimState = pSoldier.usUIMovementMode;

                if (pSoldier.bOverTerrainType == TerrainTypeDefines.DEEP_WATER)
                {
                    usMoveAnimState = AnimationStates.DEEP_WATER_SWIM;
                }

                // Change animation only.... set value to NOT call any goto new gridno stuff.....
                if (usMoveAnimState != pSoldier.usAnimState)
                {
                    //
                    pSoldier.usDontUpdateNewGridNoOnMoveAnimChange = 1;

                    EVENT_InitNewSoldierAnim(pSoldier, usMoveAnimState, 0, false);
                }

                return (true);
            }

            return (false);
        }

        // we can use the soldier's level here because we don't have pathing across levels right now...
        if (pSoldier.bPathStored)
        {
            fContinue = true;
        }
        else
        {
            iDest = FindBestPath(pSoldier, sDestGridNo, pSoldier.bLevel, usMovementAnim, COPYROUTE, fFlags);
            fContinue = (iDest != 0);
        }

        // Only if we can get a path here
        if (fContinue)
        {
            // Debug messages
            //DebugMsg(TOPIC_JA2, DBG_LEVEL_0, String("Soldier %d: Get new path", pSoldier.ubID));

            // Set final destination
            pSoldier.sFinalDestination = sDestGridNo;
            pSoldier.fPastXDest = 0;
            pSoldier.fPastYDest = 0;


            // CHECK IF FIRST TILE IS FREE
            sNewGridNo = IsometricUtils.NewGridNo(pSoldier.sGridNo, IsometricUtils.DirectionInc((int)pSoldier.usPathingData[pSoldier.usPathIndex]));

            // If true, we're OK, if not, WAIT for a guy to pass!
            // If we are in deep water, we can only swim!
            if (pSoldier.bOverTerrainType == TerrainTypeDefines.DEEP_WATER)
            {
                usMoveAnimState = AnimationStates.DEEP_WATER_SWIM;
            }

            // If we were aiming, end aim!
            usAnimState = PickSoldierReadyAnimation(pSoldier, true);

            // Add a pending animation first!
            // Only if we were standing!
            if (usAnimState != AnimationStates.INVALID_ANIMATION && gAnimControl[pSoldier.usAnimState].ubEndHeight == AnimationHeights.ANIM_STAND)
            {
                EVENT_InitNewSoldierAnim(pSoldier, usAnimState, 0, false);
                pSoldier.usPendingAnimation = usMoveAnimState;
            }
            else
            {
                // Call local copy for change soldier state!
                EVENT_InitNewSoldierAnim(pSoldier, usMoveAnimState, 0, fForceRestartAnim);

            }

            // Change desired direction
            // ATE: Here we have a situation where in RT, we may have
            // gotten a new path, but we are alreayd moving.. so
            // at leasty change new dest. This will be redundent if the ANI is a totaly new one

            return (true);
        }

        return (false);
    }

    public static void EVENT_GetNewSoldierPath(SOLDIERTYPE? pSoldier, int sDestGridNo, AnimationStates usMovementAnim)
    {
        // ATE: Default restart of animation to true
        EVENT_InternalGetNewSoldierPath(pSoldier, sDestGridNo, usMovementAnim, 0, true);
    }

    // Change our state based on stance, to stop!
    void StopSoldier(SOLDIERTYPE? pSoldier)
    {
        ReceivingSoldierCancelServices(pSoldier);
        GivingSoldierCancelServices(pSoldier);

        if (!(gAnimControl[pSoldier.usAnimState].uiFlags & ANIM_STATIONARY))
        {
            //SoldierGotoStationaryStance( pSoldier );
            EVENT_StopMerc(pSoldier, pSoldier.sGridNo, pSoldier.bDirection);
        }

        // Set desination
        pSoldier.sFinalDestination = pSoldier.sGridNo;

    }

    public static void SoldierGotoStationaryStance(SOLDIERTYPE? pSoldier)
    {
        // ATE: This is to turn off fast movement, that us used to change movement mode
        // for ui display on stance changes....
        if (pSoldier.bTeam == gbPlayerNum)
        {
            //pSoldier.fUIMovementFast = false;
        }

        // The queen, if she sees anybody, goes to ready, not normal breath....
        if (pSoldier.ubBodyType == SoldierBodyTypes.QUEENMONSTER)
        {
            if (pSoldier.bOppCnt > 0 || pSoldier.bTeam == gbPlayerNum)
            {
                EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.QUEEN_READY, 0, true);
                return;
            }
        }

        // Check if we are in deep water!
        if (pSoldier.bOverTerrainType == DEEP_WATER)
        {
            // IN deep water, tred!
            EVENT_InitNewSoldierAnim(pSoldier, DEEP_WATER_TRED, 0, false);
        }
        else if (pSoldier.ubServicePartner != NOBODY && pSoldier.bLife >= OKLIFE && pSoldier.bBreath > 0)
        {
            EVENT_InitNewSoldierAnim(pSoldier, GIVING_AID, 0, false);
        }
        else
        {
            // Change state back to stationary state for given height
            switch (gAnimControl[pSoldier.usAnimState].ubEndHeight)
            {
                case AnimationHeights.ANIM_STAND:

                    // If we are cowering....goto cower state
                    if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.COWERING))
                    {
                        EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.START_COWER, 0, false);
                    }
                    else
                    {
                        EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.STANDING, 0, false);
                    }
                    break;

                case AnimationHeights.ANIM_CROUCH:

                    // If we are cowering....goto cower state
                    if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.COWERING))
                    {
                        EVENT_InitNewSoldierAnim(pSoldier, COWERING, 0, false);
                    }
                    else
                    {
                        EVENT_InitNewSoldierAnim(pSoldier, CROUCHING, 0, false);
                    }
                    break;

                case ANIM_PRONE:
                    EVENT_InitNewSoldierAnim(pSoldier, PRONE, 0, false);
                    break;
            }

        }

    }


    public static void ChangeSoldierStance(SOLDIERTYPE? pSoldier, AnimationHeights ubDesiredStance)
    {
        AnimationStates usNewState;

        // Check if they are the same!
        if (ubDesiredStance == gAnimControl[pSoldier.usAnimState].ubEndHeight)
        {
            // Free up from stance change
            FreeUpNPCFromStanceChange(pSoldier);
            return;
        }

        // Set UI Busy
        SetUIBusy(pSoldier.ubID);

        // ATE: If we are an NPC, cower....
        if (pSoldier.ubBodyType >= SoldierBodyTypes.FATCIV && pSoldier.ubBodyType <= SoldierBodyTypes.KIDCIV)
        {
            if (ubDesiredStance == AnimationHeights.ANIM_STAND)
            {
                SetSoldierCowerState(pSoldier, false);
            }
            else
            {
                SetSoldierCowerState(pSoldier, true);
            }
        }
        else
        {
            usNewState = GetNewSoldierStateFromNewStance(pSoldier, ubDesiredStance);

            // Set desired stance
            pSoldier.ubDesiredHeight = ubDesiredStance;

            // Now change to appropriate animation
            EVENT_InitNewSoldierAnim(pSoldier, usNewState, 0, false);
        }
    }

    void EVENT_InternalSetSoldierDestination(SOLDIERTYPE? pSoldier, int usNewDirection, bool fFromMove, AnimationStates usAnimState)
    {
        int usNewGridNo;
        int sXPos, sYPos;

        // Get dest gridno, convert to center coords
        usNewGridNo = IsometricUtils.NewGridNo((int)pSoldier.sGridNo, IsometricUtils.DirectionInc(usNewDirection));

        ConvertMapPosToWorldTileCenter(usNewGridNo, out sXPos, out sYPos);

        // Save new dest gridno, x, y
        pSoldier.sDestination = usNewGridNo;
        pSoldier.sDestXPos = sXPos;
        pSoldier.sDestYPos = sYPos;

        pSoldier.bMovementDirection = (int)usNewDirection;


        // OK, ATE: If we are side_stepping, calculate a NEW desired direction....
        if (pSoldier.bReverse && usAnimState == SIDE_STEP)
        {
            int ubPerpDirection;

            // Get a new desired direction, 
            ubPerpDirection = gPurpendicularDirection[pSoldier.bDirection][usNewDirection];

            // CHange actual and desired direction....
            EVENT_SetSoldierDirection(pSoldier, ubPerpDirection);
            pSoldier.bDesiredDirection = pSoldier.bDirection;
        }
        else
        {
            if (!(gAnimControl[usAnimState].uiFlags & ANIM_SPECIALMOVE))
            {
                EVENT_InternalSetSoldierDesiredDirection(pSoldier, usNewDirection, fFromMove, usAnimState);
            }
        }
    }

    void EVENT_SetSoldierDestination(SOLDIERTYPE? pSoldier, int usNewDirection)
    {
        EVENT_InternalSetSoldierDestination(pSoldier, usNewDirection, false, pSoldier.usAnimState);
    }


    // function to determine which direction a creature can turn in
    int MultiTiledTurnDirection(SOLDIERTYPE pSoldier, WorldDirections bStartDirection, WorldDirections bDesiredDirection)
    {
        int bTurningIncrement;
        WorldDirections bCurrentDirection;
        int bLoop;
        int usStructureID;
        AnimationSurfaceTypes usAnimSurface;
        STRUCTURE_FILE_REF? pStructureFileRef;
        bool fOk = false;

        // start by trying to turn in quickest direction
        bTurningIncrement = QuickestDirection(bStartDirection, bDesiredDirection);

        usAnimSurface = AnimationControl.DetermineSoldierAnimationSurface(pSoldier, pSoldier.usUIMovementMode);

        pStructureFileRef = GetAnimationStructureRef(pSoldier.ubID, usAnimSurface, pSoldier.usUIMovementMode);
        if (pStructureFileRef is null)
        {
            // without structure data, well, assume quickest direction
            return (bTurningIncrement);
        }

        // ATE: Only if we have a levelnode...
        if (pSoldier.pLevelNode != null && pSoldier.pLevelNode.pStructureData != null)
        {
            usStructureID = pSoldier.pLevelNode.pStructureData.usStructureID;
        }
        else
        {
            usStructureID = INVALID_STRUCTURE_ID;
        }

        bLoop = 0;
        bCurrentDirection = bStartDirection;

        while (bLoop < 2)
        {
            while (bCurrentDirection != bDesiredDirection)
            {
                bCurrentDirection += bTurningIncrement;

                // did we wrap directions?
                if (bCurrentDirection < 0)
                {
                    bCurrentDirection = (MAXDIR - 1);
                }
                else if (bCurrentDirection >= World.MAXDIR)
                {
                    bCurrentDirection = 0;
                }

                // check to see if we can add creature in that direction
                fOk = StructureInternals.OkayToAddStructureToWorld(pSoldier.sGridNo, pSoldier.bLevel, &(pStructureFileRef.pDBStructureRef[gOneCDirection[bCurrentDirection]]), usStructureID);
                if (!fOk)
                {
                    break;
                }
            }

            if ((bCurrentDirection == bDesiredDirection) && fOk)
            {
                // success!!
                return (bTurningIncrement);
            }

            bLoop++;
            if (bLoop < 2)
            {
                // change direction of loop etc
                bCurrentDirection = bStartDirection;
                bTurningIncrement *= -1;
            }
        }
        // nothing found... doesn't matter much what we return
        return (bTurningIncrement);
    }

    public static void EVENT_InternalSetSoldierDesiredDirection(SOLDIERTYPE? pSoldier, WorldDirections usNewDirection, bool fInitalMove, AnimationStates usAnimState)
    {
        //if ( usAnimState == WALK_BACKWARDS )
        if (pSoldier.bReverse && usAnimState != AnimationStates.SIDE_STEP)
        {
            // OK, check if we are going to go in the exact opposite than our facing....
            usNewDirection = gOppositeDirection[usNewDirection];
        }


        pSoldier.bDesiredDirection = usNewDirection;

        // If we are prone, goto crouched first!
        // ONly if we are stationary, and only if directions are differnet!

        // ATE: If we are fNoAPsToFinnishMove, stop what we were doing and
        // reset flag.....
        if (pSoldier.fNoAPToFinishMove && (gAnimControl[usAnimState].uiFlags.HasFlag(ANIM.MOVING)))
        {
            // ATE; Commented this out: NEVER, EVER, start a new anim from this function, as an eternal loop will result....
            //SoldierGotoStationaryStance( pSoldier );
            // Reset flag!
            AdjustNoAPToFinishMove(pSoldier, false);
        }

        if (pSoldier.bDesiredDirection != pSoldier.bDirection)
        {
            if (gAnimControl[usAnimState].uiFlags.HasFlag(ANIM.BREATH | ANIM.OK_CHARGE_AP_FOR_TURN | ANIM.FIREREADY)
                && !fInitalMove && !pSoldier.fDontChargeTurningAPs)
            {
                // Deduct points for initial turn!
                switch (gAnimControl[usAnimState].ubEndHeight)
                {
                    // Now change to appropriate animation
                    case AnimationHeights.ANIM_STAND:
                        Points.DeductPoints(pSoldier, AP.LOOK_STANDING, 0);
                        break;

                    case AnimationHeights.ANIM_CROUCH:
                        Points.DeductPoints(pSoldier, AP.LOOK_CROUCHED, 0);
                        break;

                    case AnimationHeights.ANIM_PRONE:
                        Points.DeductPoints(pSoldier, AP.LOOK_PRONE, 0);
                        break;
                }

            }

            pSoldier.fDontChargeTurningAPs = false;

            if (fInitalMove)
            {
                if (gAnimControl[usAnimState].ubHeight == AnimationHeights.ANIM_PRONE)
                {
                    if (pSoldier.fTurningFromPronePosition != TURNING_FROM_PRONE_ENDING_UP_FROM_MOVE)
                    {
                        pSoldier.fTurningFromPronePosition = TURNING_FROM_PRONE_START_UP_FROM_MOVE;
                    }
                }
            }

            if (gAnimControl[usAnimState].uiFlags.HasFlag(ANIM.STATIONARY)
                || pSoldier.fNoAPToFinishMove || fInitalMove)
            {
                if (gAnimControl[usAnimState].ubHeight == AnimationHeights.ANIM_PRONE)
                {
                    // Set this beasty of a flag to allow us to go back down to prone if we choose!
                    // ATE: Alrighty, set flag to go back down only if we are not moving anywhere
                    //if ( pSoldier.sDestination == pSoldier.sGridNo )	
                    if (!fInitalMove)
                    {
                        pSoldier.fTurningFromPronePosition = TURNING_FROM_PRONE_ON;

                        // Set a pending animation to change stance first...
                        SendChangeSoldierStanceEvent(pSoldier, AnimationHeights.ANIM_CROUCH);

                    }
                }
            }
        }

        // Set desired direction for the extended directions...
        pSoldier.ubHiResDesiredDirection = ubExtDirection[pSoldier.bDesiredDirection];

        if (pSoldier.bDesiredDirection != pSoldier.bDirection)
        {
            if (pSoldier.uiStatusFlags.HasFlag((SOLDIER.VEHICLE))
                || CREATURE_OR_BLOODCAT(pSoldier))
            {
                pSoldier.uiStatusFlags |= SOLDIER.PAUSEANIMOVE;
            }
        }


        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE))
        {
            pSoldier.bTurningIncrement = (int)ExtQuickestDirection(pSoldier.ubHiResDirection, pSoldier.ubHiResDesiredDirection);
        }
        else
        {
            if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.MULTITILE))
            {
                pSoldier.bTurningIncrement = MultiTiledTurnDirection(pSoldier, pSoldier.bDirection, pSoldier.bDesiredDirection);
            }
            else
            {
                pSoldier.bTurningIncrement = (int)QuickestDirection(pSoldier.bDirection, pSoldier.bDesiredDirection);
            }
        }

    }


    private static void EVENT_SetSoldierDesiredDirection(SOLDIERTYPE? pSoldier, WorldDirections usNewDirection)
    {
        EVENT_InternalSetSoldierDesiredDirection(pSoldier, usNewDirection, false, pSoldier.usAnimState);
    }

    public static void EVENT_SetSoldierDirection(SOLDIERTYPE? pSoldier, WorldDirections usNewDirection)
    {
        // Remove old location data
        HandleAnimationProfile(pSoldier, pSoldier.usAnimState, true);

        pSoldier.bDirection = usNewDirection;

        // Updated extended direction.....
        pSoldier.ubHiResDirection = ubExtDirection[pSoldier.bDirection];

        // Add new stuff
        HandleAnimationProfile(pSoldier, pSoldier.usAnimState, false);

        // If we are turning, we have chaanged our aim!
        if (!pSoldier.fDontUnsetLastTargetFromTurn)
        {
            pSoldier.sLastTarget = NOWHERE;
        }

        AdjustForFastTurnAnimation(pSoldier);

        // Update structure info!
        //	 if ( pSoldier.uiStatusFlags.HasFlag(SOLDIER.MULTITILE ))
        {
            UpdateMercStructureInfo(pSoldier);
        }

        // Handle Profile data for hit locations
        HandleAnimationProfile(pSoldier, pSoldier.usAnimState, true);

        HandleCrowShadowNewDirection(pSoldier);

        // Change values!
        SetSoldierLocatorOffsets(pSoldier);

    }


    public static void EVENT_BeginMercTurn(SOLDIERTYPE? pSoldier, bool fFromRealTime, int iRealTimeCounter)
    {
        // NB realtimecounter is not used, always passed in as 0 now!

        int iBlood;

        if (pSoldier.bUnderFire > 0)
        {
            // UnderFire now starts at 2 for "under fire this turn",
            // down to 1 for "under fire last turn", to 0.
            pSoldier.bUnderFire--;
        }

        // ATE: Add decay effect sfor drugs...
        if (fFromRealTime) //&& iRealTimeCounter % 300 )
        {
            HandleEndTurnDrugAdjustments(pSoldier);
        }
        else
        {
            HandleEndTurnDrugAdjustments(pSoldier);
        }

        // ATE: Don't bleed if in AUTO BANDAGE!
        if (!gTacticalStatus.fAutoBandageMode)
        {
            // Blood is not for the weak of heart, or mechanical
            if (!(pSoldier.uiStatusFlags & (SOLDIER.VEHICLE | SOLDIER.ROBOT)))
            {
                if (pSoldier.bBleeding || pSoldier.bLife < OKLIFE) // is he bleeding or dying?
                {
                    iBlood = CheckBleeding(pSoldier);   // check if he might lose another life point

                    // ATE: Only if in sector!
                    if (pSoldier.bInSector)
                    {
                        if (iBlood != NOBLOOD)
                        {
                            DropBlood(pSoldier, (int)iBlood, pSoldier.bVisible);
                        }
                    }
                }
            }
        }

        // survived bleeding, but is he out of breath?
        if (pSoldier.bLife && !pSoldier.bBreath && MercInWater(pSoldier))
        {
            // Drowning...
        }

        // if he is still alive (didn't bleed to death)
        if (pSoldier.bLife)
        {
            // reduce the effects of any residual shock from past injuries by half
            pSoldier.bShock /= 2;

            // if this person has heard a noise that hasn't been investigated
            if (pSoldier.sNoiseGridno != NOWHERE)
            {
                if (pSoldier.ubNoiseVolume)    // and the noise volume is still positive
                {
                    pSoldier.ubNoiseVolume--;  // the volume of the noise "decays" by 1 point

                    if (!pSoldier.ubNoiseVolume)   // if the volume has reached zero
                    {
                        pSoldier.sNoiseGridno = NOWHERE;       // forget about the noise!
                    }
                }
            }

            // save unused action points up to a maximum
            /*
        if ((savedPts = pSoldier.bActionPts) > MAX_AP.CARRIED)
          savedPts = MAX_AP.CARRIED;
               */
            if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.GASSED))
            {
                // then must get a gas mask or leave the gassed area to get over it
                if ((pSoldier.inv[HEAD1POS].usItem == GASMASK || pSoldier.inv[HEAD2POS].usItem == GASMASK) || !(GetSmokeEffectOnTile(pSoldier.sGridNo, pSoldier.bLevel)))
                {
                    // Turn off gassed flag....
                    pSoldier.uiStatusFlags &= (~SOLDIER.GASSED);
                }
            }

            if (pSoldier.bBlindedCounter > 0)
            {
                pSoldier.bBlindedCounter--;
                if (pSoldier.bBlindedCounter == 0)
                {
                    // we can SEE!!!!!
                    HandleSight(pSoldier, SIGHT.LOOK);
                    // Dirty panel
                    fInterfacePanelDirty = DIRTYLEVEL2;
                }
            }

            // ATE: To get around a problem...
            // If an AI guy, and we have 0 life, and are still at higher hieght,
            // Kill them.....


            pSoldier.sWeightCarriedAtTurnStart = (int)CalculateCarriedWeight(pSoldier);

            UnusedAPsToBreath(pSoldier);

            // Set flag back to normal, after reaching a certain statge
            if (pSoldier.bBreath > 80)
            {
                pSoldier.usQuoteSaidFlags &= (~SOLDIER_QUOTE.SAID_LOW_BREATH);
            }
            if (pSoldier.bBreath > 50)
            {
                pSoldier.usQuoteSaidFlags &= (~SOLDIER_QUOTE.SAID_DROWNING);
            }


            if (pSoldier.ubTurnsUntilCanSayHeardNoise > 0)
            {
                pSoldier.ubTurnsUntilCanSayHeardNoise--;
            }

            if (pSoldier.bInSector)
            {
                CheckForBreathCollapse(pSoldier);
            }

            CalcNewActionPoints(pSoldier);

            pSoldier.bTilesMoved = 0;

            if (pSoldier.bInSector)
            {
                BeginSoldierGetup(pSoldier);

                // CJC Nov 30: handle RT opplist decaying in another function which operates less often
                if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
                {
                    VerifyAndDecayOpplist(pSoldier);

                    // turn off xray
                    if (pSoldier.uiXRayActivatedTime)
                    {
                        TurnOffXRayEffects(pSoldier);
                    }
                }

                if ((pSoldier.bTeam == gbPlayerNum) && (pSoldier.ubProfile != NO_PROFILE))
                {
                    switch (gMercProfiles[pSoldier.ubProfile].bPersonalityTrait)
                    {
                        case PersonalityTrait.FEAR_OF_INSECTS:
                            if (MercSeesCreature(pSoldier))
                            {
                                Morale.HandleMoraleEvent(pSoldier, MoraleEventNames.MORALE_INSECT_PHOBIC_SEES_CREATURE, pSoldier.sSectorX, pSoldier.sSectorY, pSoldier.bSectorZ);
                                if (!(pSoldier.usQuoteSaidFlags.HasFlag(SOLDIER_QUOTE.SAID_PERSONALITY)))
                                {
                                    DialogControl.TacticalCharacterDialogue(pSoldier, QUOTE.PERSONALITY_TRAIT);
                                    pSoldier.usQuoteSaidFlags |= SOLDIER_QUOTE.SAID_PERSONALITY;
                                }
                            }
                            break;
                        case PersonalityTrait.CLAUSTROPHOBIC:
                            if (gbWorldSectorZ > 0 && Globals.Random.Next(6 - gbWorldSectorZ) == 0)
                            {
                                // underground!
                                Morale.HandleMoraleEvent(pSoldier, MoraleEventNames.MORALE_CLAUSTROPHOBE_UNDERGROUND, pSoldier.sSectorX, pSoldier.sSectorY, pSoldier.bSectorZ);
                                if (!(pSoldier.usQuoteSaidFlags.HasFlag(SOLDIER_QUOTE.SAID_PERSONALITY)))
                                {
                                    DialogControl.TacticalCharacterDialogue(pSoldier, QUOTE.PERSONALITY_TRAIT);
                                    pSoldier.usQuoteSaidFlags |= SOLDIER_QUOTE.SAID_PERSONALITY;
                                }

                            }
                            break;
                        case PersonalityTrait.NERVOUS:
                            if (DistanceToClosestFriend(pSoldier) > NERVOUS_RADIUS)
                            {
                                // augh!! 
                                if (pSoldier.bMorale < 50)
                                {
                                    Morale.HandleMoraleEvent(pSoldier, MoraleEventNames.MORALE_NERVOUS_ALONE, pSoldier.sSectorX, pSoldier.sSectorY, pSoldier.bSectorZ);
                                    if (!(pSoldier.usQuoteSaidFlags.HasFlag(SOLDIER_QUOTE.SAID_PERSONALITY)))
                                    {
                                        DialogControl.TacticalCharacterDialogue(pSoldier, QUOTE.PERSONALITY_TRAIT);
                                        pSoldier.usQuoteSaidFlags |= SOLDIER_QUOTE.SAID_PERSONALITY;
                                    }
                                }
                            }
                            else
                            {
                                if (pSoldier.bMorale > 45)
                                {
                                    // turn flag off, so that we say it every two turns
                                    pSoldier.usQuoteSaidFlags &= ~SOLDIER_QUOTE.SAID_PERSONALITY;
                                }
                            }
                            break;
                    }
                }
            }

            // Reset quote flags for under heavy fire and close call!
            pSoldier.usQuoteSaidFlags &= (~SOLDIER_QUOTE.SAID_BEING_PUMMELED);
            pSoldier.usQuoteSaidExtFlags &= (~SOLDIER_QUOTE.SAID_EXT_CLOSE_CALL);
            pSoldier.bNumHitsThisTurn = 0;
            pSoldier.ubSuppressionPoints = 0;
            pSoldier.fCloseCall = 0;

            pSoldier.ubMovementNoiseHeard = 0;

            // If soldier has new APs, reset flags!
            if (pSoldier.bActionPoints > 0)
            {
                pSoldier.fUIFirstTimeNOAP = false;
                pSoldier.bMoved = 0;
                pSoldier.bPassedLastInterrupt = 0;
            }
        }
    }

    public static bool ConvertAniCodeToAniFrame(SOLDIERTYPE pSoldier, int usAniFrame)
    {
        AnimationSurfaceTypes usAnimSurface;
        int ubTempDir;
        // Given ani code, adjust for facing direction

        // get anim surface and determine # of frames
        usAnimSurface = GetSoldierAnimationSurface(pSoldier, pSoldier.usAnimState);

        CHECKF(usAnimSurface != INVALID_ANIMATION_SURFACE);

        // COnvert world direction into sprite direction
        ubTempDir = gOneCDirection[pSoldier.bDirection];

        //If we are only one frame, ignore what the script is telling us!
        if (gAnimSurfaceDatabase[usAnimSurface].ubFlags & ANIM_DATA_FLAG_NOFRAMES)
        {
            usAniFrame = 0;
        }

        if (gAnimSurfaceDatabase[usAnimSurface].uiNumDirections == 32)
        {
            ubTempDir = gExtOneCDirection[pSoldier.ubHiResDirection];
        }
        // Check # of directions /surface, adjust if ness.
        else if (gAnimSurfaceDatabase[usAnimSurface].uiNumDirections == 4)
        {
            ubTempDir = ubTempDir / 2;
        }
        // Check # of directions /surface, adjust if ness.
        else if (gAnimSurfaceDatabase[usAnimSurface].uiNumDirections == 1)
        {
            ubTempDir = 0;
        }
        // Check # of directions /surface, adjust if ness.
        else if (gAnimSurfaceDatabase[usAnimSurface].uiNumDirections == 3)
        {
            if (pSoldier.bDirection == WorldDirections.NORTHWEST)
            {
                ubTempDir = 1;
            }
            if (pSoldier.bDirection == WorldDirections.WEST)
            {
                ubTempDir = 0;
            }
            if (pSoldier.bDirection == WorldDirections.EAST)
            {
                ubTempDir = 2;
            }
        }
        else if (gAnimSurfaceDatabase[usAnimSurface].uiNumDirections == 2)
        {
            ubTempDir = gDirectionFrom8to2[pSoldier.bDirection];
        }

        pSoldier.usAniFrame = usAniFrame + (int)((gAnimSurfaceDatabase[usAnimSurface].uiNumFramesPerDir * ubTempDir));

        if (gAnimSurfaceDatabase[usAnimSurface].hVideoObject == null)
        {
            pSoldier.usAniFrame = 0;
            return (true);
        }

        if (pSoldier.usAniFrame >= gAnimSurfaceDatabase[usAnimSurface].hVideoObject.usNumberOfObjects)
        {
            // Debug msg here....
            //		ScreenMsg( FONT_MCOLOR_LTYELLOW, MSG_BETAVERSION, "Soldier Animation: Wrong Number of frames per number of objects: %d vs %d, %S",  gAnimSurfaceDatabase[ usAnimSurface ].uiNumFramesPerDir, gAnimSurfaceDatabase[ usAnimSurface ].hVideoObject.usNumberOfObjects, gAnimControl[ pSoldier.usAnimState ].zAnimStr );	

            pSoldier.usAniFrame = 0;
        }

        return (true);
    }


    void TurnSoldier(SOLDIERTYPE? pSoldier)
    {
        int sDirection;
        bool fDoDirectionChange = true;
        int cnt;

        // If we are a vehicle... DON'T TURN!
        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE))
        {
            if (pSoldier.ubBodyType != TANK_NW && pSoldier.ubBodyType != TANK_NE)
            {
                return;
            }
        }

        // We handle sight now....
        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.LOOK_NEXT_TURNSOLDIER))
        {
            if ((gAnimControl[pSoldier.usAnimState].uiFlags.HasFlag(ANIM.STATIONARY)
                && pSoldier.usAnimState != AnimationStates.CLIMBUPROOF
                && pSoldier.usAnimState != AnimationStates.CLIMBDOWNROOF))
            {
                // HANDLE SIGHT!
                HandleSight(pSoldier, SIGHT.LOOK | SIGHT.RADIO);
            }
            // Turn off!
            pSoldier.uiStatusFlags &= (~SOLDIER.LOOK_NEXT_TURNSOLDIER);

            HandleSystemNewAISituation(pSoldier, false);
        }


        if (pSoldier.fTurningToShoot)
        {
            if (pSoldier.bDirection == pSoldier.bDesiredDirection)
            {
                if (((gAnimControl[pSoldier.usAnimState].uiFlags & ANIM_FIREREADY) && !pSoldier.fTurningFromPronePosition) || pSoldier.ubBodyType == ROBOTNOWEAPON || pSoldier.ubBodyType == TANK_NW || pSoldier.ubBodyType == TANK_NE)
                {
                    EVENT_InitNewSoldierAnim(pSoldier, SelectFireAnimation(pSoldier, gAnimControl[pSoldier.usAnimState].ubEndHeight), 0, false);
                    pSoldier.fTurningToShoot = false;

                    // Save last target gridno!
                    //pSoldier.sLastTarget = pSoldier.sTargetGridNo;

                }
                // Else check if we are trying to shoot and once was prone, but am now crouched because we needed to turn...
                else if (pSoldier.fTurningFromPronePosition)
                {
                    if (IsValidStance(pSoldier, AnimationHeights.ANIM_PRONE))
                    {
                        SendChangeSoldierStanceEvent(pSoldier, AnimationHeights.ANIM_PRONE);
                        pSoldier.usPendingAnimation = SelectFireAnimation(pSoldier, AnimationHeights.ANIM_PRONE);
                    }
                    else
                    {
                        EVENT_InitNewSoldierAnim(pSoldier, SelectFireAnimation(pSoldier, AnimationHeights.ANIM_CROUCH), 0, false);
                    }
                    pSoldier.fTurningToShoot = false;
                    pSoldier.fTurningFromPronePosition = TURNING_FROM_PRONE_OFF;
                }
            }
        }

        if (pSoldier.fTurningToFall)
        {
            if (pSoldier.bDirection == pSoldier.bDesiredDirection)
            {
                SelectFallAnimation(pSoldier);
                pSoldier.fTurningToFall = false;
            }
        }

        if (pSoldier.fTurningUntilDone && (pSoldier.ubPendingStanceChange != NO_PENDING_STANCE))
        {
            if (pSoldier.bDirection == pSoldier.bDesiredDirection)
            {
                SendChangeSoldierStanceEvent(pSoldier, pSoldier.ubPendingStanceChange);
                pSoldier.ubPendingStanceChange = NO_PENDING_STANCE;
                pSoldier.fTurningUntilDone = false;
            }
        }

        if (pSoldier.fTurningUntilDone && (pSoldier.usPendingAnimation != NO_PENDING_ANIMATION))
        {
            if (pSoldier.bDirection == pSoldier.bDesiredDirection)
            {
                int usPendingAnimation;

                usPendingAnimation = pSoldier.usPendingAnimation;
                pSoldier.usPendingAnimation = NO_PENDING_ANIMATION;

                EVENT_InitNewSoldierAnim(pSoldier, usPendingAnimation, 0, false);
                pSoldier.fTurningUntilDone = false;
            }
        }

        // Don't do anything if we are at dest direction!
        if (pSoldier.bDirection == pSoldier.bDesiredDirection)
        {
            if (pSoldier.ubBodyType == TANK_NW || pSoldier.ubBodyType == TANK_NE)
            {
                if (pSoldier.iTuringSoundID != NO_SAMPLE)
                {
                    SoundStop(pSoldier.iTuringSoundID);
                    pSoldier.iTuringSoundID = NO_SAMPLE;

                    //PlaySoldierJA2Sample(pSoldier.ubID, TURRET_STOP, RATE_11025, SoundVolume(HIGHVOLUME, pSoldier.sGridNo), 1, SoundDir(pSoldier.sGridNo), true);
                }
            }

            // Turn off!
            pSoldier.uiStatusFlags &= (~SOLDIER.LOOK_NEXT_TURNSOLDIER);
            pSoldier.fDontUnsetLastTargetFromTurn = false;

            // Unset ui busy if from ui
            if (pSoldier.bTurningFromUI && (pSoldier.fTurningFromPronePosition != 3) && (pSoldier.fTurningFromPronePosition != 1))
            {
                UnSetUIBusy(pSoldier.ubID);
                pSoldier.bTurningFromUI = false;
            }

            if (pSoldier.uiStatusFlags & (SOLDIER.VEHICLE) || CREATURE_OR_BLOODCAT(pSoldier))
            {
                pSoldier.uiStatusFlags &= (~SOLDIER_PAUSEANIMOVE);
            }

            FreeUpNPCFromTurning(pSoldier, LOOK);

            // Undo our flag for prone turning...
            // Else check if we are trying to shoot and once was prone, but am now crouched because we needed to turn...
            if (pSoldier.fTurningFromPronePosition == TURNING_FROM_PRONE_ON)
            {
                // ATE: Don't do this if we have something in our hands we are going to throw!
                if (IsValidStance(pSoldier, ANIM_PRONE) && pSoldier.pTempObject == null)
                {
                    SendChangeSoldierStanceEvent(pSoldier, ANIM_PRONE);
                }
                pSoldier.fTurningFromPronePosition = TURNING_FROM_PRONE_OFF;
            }

            // If a special code, make guy crawl after stance change!
            if (pSoldier.fTurningFromPronePosition == TURNING_FROM_PRONE_ENDING_UP_FROM_MOVE && pSoldier.usAnimState != PRONE_UP && pSoldier.usAnimState != PRONE_DOWN)
            {
                if (IsValidStance(pSoldier, ANIM_PRONE))
                {
                    EVENT_InitNewSoldierAnim(pSoldier, CRAWLING, 0, false);
                }
            }

            if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.TURNINGFROMHIT))
            {
                if (pSoldier.fGettingHit == 1)
                {
                    if (pSoldier.usPendingAnimation != FALLFORWARD_ROOF && pSoldier.usPendingAnimation != FALLOFF && pSoldier.usAnimState != FALLFORWARD_ROOF && pSoldier.usAnimState != FALLOFF)
                    {
                        // Go back to original direction
                        EVENT_SetSoldierDesiredDirection(pSoldier, (int)pSoldier.uiPendingActionData1);

                        //SETUP GETTING HIT FLAG TO 2
                        pSoldier.fGettingHit = 2;
                    }
                    else
                    {
                        pSoldier.uiStatusFlags &= (~SOLDIER_TURNINGFROMHIT);
                    }
                }
                else if (pSoldier.fGettingHit == 2)
                {
                    // Turn off
                    pSoldier.uiStatusFlags &= (~SOLDIER.TURNINGFROMHIT);

                    // Release attacker
                    //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("@@@@@@@ Releasesoldierattacker, turning from hit animation ended"));
                    ReleaseSoldiersAttacker(pSoldier);

                    //FREEUP GETTING HIT FLAG
                    pSoldier.fGettingHit = false;
                }
            }

            return;
        }

        // IF WE ARE HERE, WE ARE IN THE PROCESS OF TURNING

        // double CHECK TO UNSET fNOAPs...
        if (pSoldier.fNoAPToFinishMove)
        {
            AdjustNoAPToFinishMove(pSoldier, false);
        }

        // Do something different for vehicles....
        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE))
        {
            fDoDirectionChange = false;

            // Get new direction
            /*
            sDirection = pSoldier.ubHiResDirection + ExtQuickestDirection( pSoldier.ubHiResDirection, pSoldier.ubHiResDesiredDirection );
            */
            sDirection = pSoldier.ubHiResDirection + pSoldier.bTurningIncrement;
            if (sDirection > 31)
            {
                sDirection = 0;
            }
            else
            {
                if (sDirection < 0)
                {
                    sDirection = 31;
                }
            }
            pSoldier.ubHiResDirection = (int)sDirection;

            // Are we at a multiple of a 'cardnal' direction?
            for (cnt = 0; cnt < 8; cnt++)
            {
                if (sDirection == ubExtDirection[cnt])
                {
                    fDoDirectionChange = true;

                    sDirection = (int)cnt;

                    break;
                }
            }

            if (pSoldier.ubBodyType == TANK_NW || pSoldier.ubBodyType == TANK_NE)
            {
                if (pSoldier.iTuringSoundID == NO_SAMPLE)
                {
                    // pSoldier.iTuringSoundID = //PlaySoldierJA2Sample(pSoldier.ubID, TURRET_MOVE, RATE_11025, SoundVolume(HIGHVOLUME, pSoldier.sGridNo), 100, SoundDir(pSoldier.sGridNo), true);
                }
            }
        }
        else
        {
            // Get new direction
            //sDirection = pSoldier.bDirection + QuickestDirection( pSoldier.bDirection, pSoldier.bDesiredDirection );
            sDirection = pSoldier.bDirection + pSoldier.bTurningIncrement;
            if (sDirection > 7)
            {
                sDirection = 0;
            }
            else
            {
                if (sDirection < 0)
                {
                    sDirection = 7;
                }
            }
        }


        // CHECK FOR A VALID TURN DIRECTION
        // This is needed for prone animations as well as any multi-tiled structs
        if (fDoDirectionChange)
        {
            if (OKToAddMercToWorld(pSoldier, (int)sDirection))
            {
                // Don't do this if we are walkoing off screen...
                if (gubWaitingForAllMercsToExitCode == WAIT_FOR_MERCS_TO_WALKOFF_SCREEN || gubWaitingForAllMercsToExitCode == WAIT_FOR_MERCS_TO_WALK_TO_GRIDNO)
                {

                }
                else
                {
                    // ATE: We should only do this if we are STATIONARY!
                    if ((gAnimControl[pSoldier.usAnimState].uiFlags & ANIM_STATIONARY))
                    {
                        pSoldier.uiStatusFlags |= SOLDIER_LOOK_NEXT_TURNSOLDIER;
                    }
                    // otherwise, it's handled next tile...
                }

                EVENT_SetSoldierDirection(pSoldier, sDirection);

                if (pSoldier.ubBodyType != LARVAE_MONSTER && !MercInWater(pSoldier) && pSoldier.bOverTerrainType != DIRT_ROAD && pSoldier.bOverTerrainType != PAVED_ROAD)
                {
                    PlaySoldierFootstepSound(pSoldier);
                }
            }
            else
            {
                // Are we prone crawling?
                if (pSoldier.usAnimState == AnimationStates.CRAWLING)
                {
                    // OK, we want to getup, turn and go prone again....
                    SendChangeSoldierStanceEvent(pSoldier, AnimationHeights.ANIM_CROUCH);
                    pSoldier.fTurningFromPronePosition = TURNING_FROM_PRONE_ENDING_UP_FROM_MOVE;
                }
                // If we are a creature, or multi-tiled, cancel AI action.....?
                else if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.MULTITILE))
                {
                    pSoldier.bDesiredDirection = pSoldier.bDirection;
                }

            }
        }
    }


    int[] gRedGlowR =
    {
    0,			// Normal shades
	25,
    50,
    75,
    100,
    125,
    150,
    175,
    200,
    225,

    0,		// For gray palettes
	25,
    50,
    75,
    100,
    125,
    150,
    175,
    200,
    225,

};

    int[] gOrangeGlowR =
    {
    0,			// Normal shades
	25,
    50,
    75,
    100,
    125,
    150,
    175,
    200,
    225,

    0,		// For gray palettes
	25,
    50,
    75,
    100,
    125,
    150,
    175,
    200,
    225,

};


#if false
int	gOrangeGlowG[]=
{
	0,			// Normal shades
	5,
	10,
	25,
	30,
	35,
	40,
	45,
	50,
	55,

	0,		// For gray palettes
	5,
	10,
	25,
	30,
	35,
	40,
	45,
	50,
	55,
};
#endif

    int[] gOrangeGlowG =
    {
    0,			// Normal shades
	20,
    40,
    60,
    80,
    100,
    120,
    140,
    160,
    180,

    0,		// For gray palettes
	20,
    40,
    60,
    80,
    100,
    120,
    140,
    160,
    180,

};


    public static bool CreateSoldierPalettes(SOLDIERTYPE? pSoldier)
    {
        int usAnimSurface, usPaletteAnimSurface;
        string zColFilename;// [100];
        int iWhich;
        int cnt;
        int bBodyTypePalette;
        SGPPaletteEntry[] Temp8BPPPalette;// [256];

        //NT32 uiCount;
        //PPaletteEntry Pal[256];

        if (pSoldier.p8BPPPalette != null)
        {
            MemFree(pSoldier.p8BPPPalette);
            pSoldier.p8BPPPalette = null;
        }


        // Allocate mem for new palette
        pSoldier.p8BPPPalette = new();//MemAlloc(sizeof(SGPPaletteEntry) * 256);
        //memset(pSoldier.p8BPPPalette, 0, sizeof(SGPPaletteEntry) * 256);

        //CHECKF(pSoldier.p8BPPPalette != null);

        // --- TAKE FROM CURRENT ANIMATION HVOBJECT!
        usAnimSurface = AnimationControl.GetSoldierAnimationSurface(pSoldier, pSoldier.usAnimState);

        CHECKF(usAnimSurface != INVALID_ANIMATION_SURFACE);

        if ((bBodyTypePalette = GetBodyTypePaletteSubstitutionCode(pSoldier, pSoldier.ubBodyType, zColFilename)) == -1)
        {
            // ATE: here we want to use the breath cycle for the palette.....
            usPaletteAnimSurface = LoadSoldierAnimationSurface(pSoldier, STANDING);

            if (usPaletteAnimSurface != INVALID_ANIMATION_SURFACE)
            {
                // Use palette from HVOBJECT, then use substitution for pants, etc
                memcpy(pSoldier.p8BPPPalette, gAnimSurfaceDatabase[usPaletteAnimSurface].hVideoObject.pPaletteEntry, sizeof(pSoldier.p8BPPPalette) * 256);

                // Substitute based on head, etc
                SetPaletteReplacement(pSoldier.p8BPPPalette, pSoldier.HeadPal);
                SetPaletteReplacement(pSoldier.p8BPPPalette, pSoldier.VestPal);
                SetPaletteReplacement(pSoldier.p8BPPPalette, pSoldier.PantsPal);
                SetPaletteReplacement(pSoldier.p8BPPPalette, pSoldier.SkinPal);
            }
        }
        else if (bBodyTypePalette == 0)
        {
            // Use palette from hvobject
            memcpy(pSoldier.p8BPPPalette, gAnimSurfaceDatabase[usAnimSurface].hVideoObject.pPaletteEntry, sizeof(pSoldier.p8BPPPalette) * 256);
        }
        else
        {
            // Use col file
            if (CreateSGPPaletteFromCOLFile(Temp8BPPPalette, zColFilename))
            {
                // Copy into palette
                memcpy(pSoldier.p8BPPPalette, Temp8BPPPalette, sizeof(pSoldier.p8BPPPalette) * 256);
            }
            else
            {
                // Use palette from hvobject
                memcpy(pSoldier.p8BPPPalette, gAnimSurfaceDatabase[usAnimSurface].hVideoObject.pPaletteEntry, sizeof(pSoldier.p8BPPPalette) * 256);
            }
        }


        if (pSoldier.p16BPPPalette != null)
        {
            MemFree(pSoldier.p16BPPPalette);
            pSoldier.p16BPPPalette = null;
        }

        // -- BUILD 16BPP Palette from this
        pSoldier.p16BPPPalette = Create16BPPPalette(pSoldier.p8BPPPalette);

        for (iWhich = 0; iWhich < NUM_SOLDIER_SHADES; iWhich++)
        {
            if (pSoldier.pShades[iWhich] != null)
            {
                MemFree(pSoldier.pShades[iWhich]);
                pSoldier.pShades[iWhich] = null;
            }
        }

        for (iWhich = 0; iWhich < NUM_SOLDIER_EFFECTSHADES; iWhich++)
        {
            if (pSoldier.pEffectShades[iWhich] != null)
            {
                MemFree(pSoldier.pEffectShades[iWhich]);
                pSoldier.pEffectShades[iWhich] = null;
            }
        }

        for (iWhich = 0; iWhich < 20; iWhich++)
        {
            if (pSoldier.pGlowShades[iWhich] != null)
            {
                MemFree(pSoldier.pGlowShades[iWhich]);
                pSoldier.pGlowShades[iWhich] = null;
            }
        }


        CreateSoldierPaletteTables(pSoldier, HVOBJECT_GLOW_GREEN);


        // Build a grayscale palette for testing grayout of mercs
        //for(uiCount=0; uiCount < 256; uiCount++)
        //{
        //	Pal[uiCount].peRed=(int)(uiCount%128)+128;
        //	Pal[uiCount].peGreen=(int)(uiCount%128)+128;
        //	Pal[uiCount].peBlue=(int)(uiCount%128)+128;
        //}
        pSoldier.pEffectShades[0] = Create16BPPPaletteShaded(pSoldier.p8BPPPalette, 100, 100, 100, true);
        pSoldier.pEffectShades[1] = Create16BPPPaletteShaded(pSoldier.p8BPPPalette, 100, 150, 100, true);

        // Build shades for glowing visible bad guy

        // First do visible guy
        pSoldier.pGlowShades[0] = Create16BPPPaletteShaded(pSoldier.p8BPPPalette, 255, 255, 255, false);
        for (cnt = 1; cnt < 10; cnt++)
        {
            pSoldier.pGlowShades[cnt] = CreateEnemyGlow16BPPPalette(pSoldier.p8BPPPalette, gRedGlowR[cnt], 255, false);
        }

        // Now for gray guy...
        pSoldier.pGlowShades[10] = Create16BPPPaletteShaded(pSoldier.p8BPPPalette, 100, 100, 100, true);
        for (cnt = 11; cnt < 19; cnt++)
        {
            pSoldier.pGlowShades[cnt] = CreateEnemyGreyGlow16BPPPalette(pSoldier.p8BPPPalette, gRedGlowR[cnt], 0, false);
        }
        pSoldier.pGlowShades[19] = CreateEnemyGreyGlow16BPPPalette(pSoldier.p8BPPPalette, gRedGlowR[18], 0, false);


        // ATE: OK, piggyback on the shades we are not using for 2 colored lighting....
        // ORANGE, VISIBLE GUY
        pSoldier.pShades[20] = Create16BPPPaletteShaded(pSoldier.p8BPPPalette, 255, 255, 255, false);
        for (cnt = 21; cnt < 30; cnt++)
        {
            pSoldier.pShades[cnt] = CreateEnemyGlow16BPPPalette(pSoldier.p8BPPPalette, gOrangeGlowR[(cnt - 20)], gOrangeGlowG[(cnt - 20)], true);
        }

        // ORANGE, GREY GUY
        pSoldier.pShades[30] = Create16BPPPaletteShaded(pSoldier.p8BPPPalette, 100, 100, 100, true);
        for (cnt = 31; cnt < 39; cnt++)
        {
            pSoldier.pShades[cnt] = CreateEnemyGreyGlow16BPPPalette(pSoldier.p8BPPPalette, gOrangeGlowR[(cnt - 20)], gOrangeGlowG[(cnt - 20)], true);
        }
        pSoldier.pShades[39] = CreateEnemyGreyGlow16BPPPalette(pSoldier.p8BPPPalette, gOrangeGlowR[18], gOrangeGlowG[18], true);

        return (true);
    }



    public static void AdjustAniSpeed(SOLDIERTYPE? pSoldier)
    {
        if ((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.SLOW_ANIMATION)))
        {
            if (gTacticalStatus.bRealtimeSpeed == -1)
            {
                pSoldier.sAniDelay = 10000;
            }
            else
            {
                pSoldier.sAniDelay = pSoldier.sAniDelay * (1 * gTacticalStatus.bRealtimeSpeed / 2);
            }
        }


        RESETTIMECOUNTER(ref pSoldier.UpdateCounter, pSoldier.sAniDelay);
    }


    public static void CalculateSoldierAniSpeed(SOLDIERTYPE? pSoldier, SOLDIERTYPE? pStatsSoldier)
    {
        int uiTerrainDelay;
        int uiSpeed = 0;

        int bBreathDef, bLifeDef, bAgilDef;
        int bAdditional = 0;

        // for those animations which have a speed of zero, we have to calculate it
        // here. Some animation, such as water-movement, have an ADDITIONAL speed
        switch (pSoldier.usAnimState)
        {
            case AnimationStates.PRONE:
            case AnimationStates.STANDING:

                pSoldier.sAniDelay = (pStatsSoldier.bBreath * 2) + (100 - pStatsSoldier.bLife);

                // Limit it!
                if (pSoldier.sAniDelay < 40)
                {
                    pSoldier.sAniDelay = 40;
                }
                AdjustAniSpeed(pSoldier);
                return;

            case AnimationStates.CROUCHING:

                pSoldier.sAniDelay = (pStatsSoldier.bBreath * 2) + ((100 - pStatsSoldier.bLife));

                // Limit it!
                if (pSoldier.sAniDelay < 40)
                {
                    pSoldier.sAniDelay = 40;
                }
                AdjustAniSpeed(pSoldier);
                return;

            case AnimationStates.WALKING:

                // Adjust based on body type
                bAdditional = (int)(gubAnimWalkSpeeds[pStatsSoldier.ubBodyType].sSpeed);
                if (bAdditional < 0)
                {
                    bAdditional = 0;
                }

                break;

            case AnimationStates.RUNNING:

                // Adjust based on body type
                bAdditional = gubAnimRunSpeeds[pStatsSoldier.ubBodyType].sSpeed;
                if (bAdditional < 0)
                {
                    bAdditional = 0;
                }

                break;

            case AnimationStates.SWATTING:

                // Adjust based on body type
                if (pStatsSoldier.ubBodyType <= SoldierBodyTypes.REGFEMALE)
                {
                    bAdditional = (int)gubAnimSwatSpeeds[pStatsSoldier.ubBodyType].sSpeed;
                    if (bAdditional < 0)
                    {
                        bAdditional = 0;
                    }
                }
                break;

            case CRAWLING:

                // Adjust based on body type
                if (pStatsSoldier.ubBodyType <= SoldierBodyTypes.REGFEMALE)
                {
                    bAdditional = (int)gubAnimCrawlSpeeds[pStatsSoldier.ubBodyType].sSpeed;
                    if (bAdditional < 0)
                    {
                        bAdditional = 0;
                    }
                }
                break;

            case AnimationStates.READY_RIFLE_STAND:

                // Raise rifle based on aim vs non-aim.
                if (pSoldier.bAimTime == 0)
                {
                    // Quick shot
                    pSoldier.sAniDelay = 70;
                }
                else
                {
                    pSoldier.sAniDelay = 150;
                }
                AdjustAniSpeed(pSoldier);
                return;
        }


        // figure out movement speed (terrspeed)
        if (gAnimControl[pSoldier.usAnimState].uiFlags.HasFlag(ANIM.MOVING))
        {
            uiSpeed = gsTerrainTypeSpeedModifiers[pStatsSoldier.bOverTerrainType];

            uiTerrainDelay = uiSpeed;
        }
        else
        {
            uiTerrainDelay = 40;            // standing still
        }

        bBreathDef = 50 - (pStatsSoldier.bBreath / 2);

        if (bBreathDef > 30)
        {
            bBreathDef = 30;
        }

        bAgilDef = 50 - (SkillChecks.EffectiveAgility(pStatsSoldier) / 4);
        bLifeDef = 50 - (pStatsSoldier.bLife / 2);

        uiTerrainDelay += (bLifeDef + bBreathDef + bAgilDef + bAdditional);

        pSoldier.sAniDelay = uiTerrainDelay;

        // If a moving animation and w/re on drugs, increase speed....
        if (gAnimControl[pSoldier.usAnimState].uiFlags.HasFlag(ANIM.MOVING))
        {
            if (GetDrugEffect(pSoldier, DRUG_TYPE_ADRENALINE))
            {
                pSoldier.sAniDelay = pSoldier.sAniDelay / 2;
            }
        }

        // MODIFTY NOW BASED ON REAL-TIME, ETC
        // Adjust speed, make twice as fast if in turn-based!
        if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED) && (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
        {
            pSoldier.sAniDelay = pSoldier.sAniDelay / 2;
        }

        // MODIFY IF REALTIME COMBAT
        if (!(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
        {
            // ATE: If realtime, and stealth mode...
            if (pStatsSoldier.bStealthMode)
            {
                pSoldier.sAniDelay = (pSoldier.sAniDelay * 2);
            }

            //pSoldier.sAniDelay = pSoldier.sAniDelay * ( 1 * gTacticalStatus.bRealtimeSpeed / 2 );
        }

    }


    public static void SetSoldierAniSpeed(SOLDIERTYPE? pSoldier)
    {
        SOLDIERTYPE? pStatsSoldier;


        // ATE: If we are an enemy and are not visible......
        // Set speed to 0
        if ((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED) && (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
            || gTacticalStatus.fAutoBandageMode)
        {
            if (((pSoldier.bVisible == -1 && pSoldier.bVisible == pSoldier.bLastRenderVisibleValue) || gTacticalStatus.fAutoBandageMode) && pSoldier.usAnimState != MONSTER_UP)
            {
                pSoldier.sAniDelay = 0;
                RESETTIMECOUNTER(ref pSoldier.UpdateCounter, pSoldier.sAniDelay);
                return;
            }
        }

        // Default stats soldier to same as normal soldier.....
        pStatsSoldier = pSoldier;

        if (pSoldier.fUseMoverrideMoveSpeed)
        {
            pStatsSoldier = MercPtrs[pSoldier.bOverrideMoveSpeed];
        }

        // Only calculate if set to zero
        if ((pSoldier.sAniDelay = gAnimControl[pSoldier.usAnimState].sSpeed) == 0)
        {
            CalculateSoldierAniSpeed(pSoldier, pStatsSoldier);
        }

        AdjustAniSpeed(pSoldier);

        if (_KeyDown(SPACE))
        {
            //pSoldier.sAniDelay = 1000;
        }

    }


    ///////////////////////////////////////////////////////
    //PALETTE REPLACEMENT FUNCTIONS
    ///////////////////////////////////////////////////////
    bool LoadPaletteData()
    {
        Stream hFile;
        int cnt, cnt2;

        hFile = FileManager.FileOpen(PALETTEFILENAME, FILE_ACCESS_READ, FALSE);

        // Read # of types
        if (!FileManager.FileRead(hFile, ref guiNumPaletteSubRanges, sizeof(guiNumPaletteSubRanges), null))
        {
            return (false);
        }

        // Malloc!
        gpPaletteSubRanges = MemAlloc(sizeof(PaletteSubRangeType) * guiNumPaletteSubRanges);
        gubpNumReplacementsPerRange = MemAlloc(sizeof(int) * guiNumPaletteSubRanges);

        // Read # of types for each!
        for (cnt = 0; cnt < guiNumPaletteSubRanges; cnt++)
        {
            if (!FileManager.FileRead(hFile, ref gubpNumReplacementsPerRange[cnt], sizeof(int), null))
            {
                return (false);
            }
        }

        // Loop for each one, read in data
        for (cnt = 0; cnt < guiNumPaletteSubRanges; cnt++)
        {
            if (!FileManager.FileRead(hFile, &gpPaletteSubRanges[cnt].ubStart, sizeof(int), null))
            {
                return (false);
            }
            if (!FileManager.FileRead(hFile, &gpPaletteSubRanges[cnt].ubEnd, sizeof(int), null))
            {
                return (false);
            }
        }


        // Read # of palettes
        if (!FileManager.FileRead(hFile, &guiNumReplacements, sizeof(guiNumReplacements), null))
        {
            return (false);
        }

        // Malloc!
        gpPalRep = MemAlloc(sizeof(PaletteReplacementType) * guiNumReplacements);

        // Read!
        for (cnt = 0; cnt < guiNumReplacements; cnt++)
        {
            // sizeof(gpPalRep[cnt].ubType)
            if (!FileManager.FileRead(hFile, gpPalRep[cnt].ubType, 100, null))
            {
                return (false);
            }

            // sizeof(gpPalRep[cnt].ID)
            if (!FileManager.FileRead(hFile, &gpPalRep[cnt].ID, 100, null))
            {
                return (false);
            }

            // # entries // sizeof(gpPalRep[cnt].ubPaletteSize)
            if (!FileManager.FileRead(hFile, ref gpPalRep[cnt].ubPaletteSize, 100, null))
            {
                return (false);
            }

            // Malloc
            gpPalRep[cnt].r = MemAlloc(gpPalRep[cnt].ubPaletteSize);
            CHECKF(gpPalRep[cnt].r != null);
            gpPalRep[cnt].g = MemAlloc(gpPalRep[cnt].ubPaletteSize);
            CHECKF(gpPalRep[cnt].g != null);
            gpPalRep[cnt].b = MemAlloc(gpPalRep[cnt].ubPaletteSize);
            CHECKF(gpPalRep[cnt].b != null);

            for (cnt2 = 0; cnt2 < gpPalRep[cnt].ubPaletteSize; cnt2++)
            {
                if (!FileManager.FileRead(hFile, ref gpPalRep[cnt].r[cnt2], sizeof(int), null))
                {
                    return (false);
                }
                if (!FileManager.FileRead(hFile, ref gpPalRep[cnt].g[cnt2], sizeof(int), null))
                {
                    return (false);
                }
                if (!FileManager.FileRead(hFile, ref gpPalRep[cnt].b[cnt2], sizeof(int), null))
                {
                    return (false);
                }
            }

            FileManager.FileClose(hFile);

            return (true);
        }
    }

    bool SetPaletteReplacement(SGPPaletteEntry? p8BPPPalette, PaletteRepID aPalRep)
    {
        int cnt2;
        int ubType;
        int ubPalIndex;

        CHECKF(GetPaletteRepIndexFromID(aPalRep, ubPalIndex));

        // Get range type
        ubType = gpPalRep[ubPalIndex].ubType;

        for (cnt2 = gpPaletteSubRanges[ubType].ubStart; cnt2 <= gpPaletteSubRanges[ubType].ubEnd; cnt2++)
        {
            p8BPPPalette[cnt2].peRed = gpPalRep[ubPalIndex].r[cnt2 - gpPaletteSubRanges[ubType].ubStart];
            p8BPPPalette[cnt2].peGreen = gpPalRep[ubPalIndex].g[cnt2 - gpPaletteSubRanges[ubType].ubStart];
            p8BPPPalette[cnt2].peBlue = gpPalRep[ubPalIndex].b[cnt2 - gpPaletteSubRanges[ubType].ubStart];
        }

        return (true);
    }


    bool DeletePaletteData()
    {
        int cnt;

        // Free!
        if (gpPaletteSubRanges != null)
        {
            MemFree(gpPaletteSubRanges);
            gpPaletteSubRanges = null;
        }

        if (gubpNumReplacementsPerRange != null)
        {
            MemFree(gubpNumReplacementsPerRange);
            gubpNumReplacementsPerRange = null;
        }


        for (cnt = 0; cnt < guiNumReplacements; cnt++)
        {
            // Free
            if (gpPalRep[cnt].r != null)
            {
                MemFree(gpPalRep[cnt].r);
                gpPalRep[cnt].r = null;
            }
            if (gpPalRep[cnt].g != null)
            {
                MemFree(gpPalRep[cnt].g);
                gpPalRep[cnt].g = null;
            }
            if (gpPalRep[cnt].b != null)
            {
                MemFree(gpPalRep[cnt].b);
                gpPalRep[cnt].b = null;
            }
        }

        // Free
        if (gpPalRep != null)
        {
            MemFree(gpPalRep);
            gpPalRep = null;
        }

        return (true);
    }


    bool GetPaletteRepIndexFromID(PaletteRepID aPalRep, int? pubPalIndex)
    {
        int cnt;

        // Check if type exists
        for (cnt = 0; cnt < guiNumReplacements; cnt++)
        {
            if (COMPARE_PALETTEREP_ID(aPalRep, gpPalRep[cnt].ID))
            {
                pubPalIndex = (int)cnt;
                return (true);
            }
        }

        //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, "Invalid Palette Replacement ID given");
        return (false);
    }

    private static AnimationStates GetNewSoldierStateFromNewStance(SOLDIERTYPE? pSoldier, AnimationHeights ubDesiredStance)
    {
        AnimationStates usNewState;
        int bCurrentHeight;

        bCurrentHeight = (ubDesiredStance - gAnimControl[pSoldier.usAnimState].ubEndHeight);

        // Now change to appropriate animation

        switch (bCurrentHeight)
        {
            case AnimationHeights.ANIM_STAND - AnimationHeights.ANIM_CROUCH:
                usNewState = AnimationStates.KNEEL_UP;
                break;
            case AnimationHeights.ANIM_CROUCH - AnimationHeights.ANIM_STAND:
                usNewState = AnimationStates.KNEEL_DOWN;
                break;

            case AnimationHeights.ANIM_STAND - AnimationHeights.ANIM_PRONE:
                usNewState = AnimationStates.PRONE_UP;
                break;
            case AnimationHeights.ANIM_PRONE - AnimationHeights.ANIM_STAND:
                usNewState = AnimationStates.KNEEL_DOWN;
                break;

            case AnimationHeights.ANIM_CROUCH - AnimationHeights.ANIM_PRONE:
                usNewState = AnimationStates.PRONE_UP;
                break;
            case AnimationHeights.ANIM_PRONE - AnimationHeights.ANIM_CROUCH:
                usNewState = AnimationStates.PRONE_DOWN;
                break;

            default:

                // Cannot get here unless ub desired stance is bogus
                // DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("GetNewSoldierStateFromNewStance bogus ubDesiredStance value %d", ubDesiredStance));
                usNewState = pSoldier.usAnimState;
                break;
        }

        return (usNewState);
    }


    void MoveMercFacingDirection(SOLDIERTYPE? pSoldier, bool fReverse, double dMovementDist)
    {
        double dAngle = (double)0;

        // Determine which direction we are in 
        switch (pSoldier.bDirection)
        {
            case WorldDirections.NORTH:
                dAngle = (double)(-1 * Math.PI);
                break;

            case WorldDirections.NORTHEAST:
                dAngle = (double)(Math.PI * .75);
                break;

            case WorldDirections.EAST:
                dAngle = (double)(Math.PI / 2);
                break;

            case WorldDirections.SOUTHEAST:
                dAngle = (double)(Math.PI / 4);
                break;

            case WorldDirections.SOUTH:
                dAngle = (double)0;
                break;

            case WorldDirections.SOUTHWEST:
                //dAngle = (double)(  PI * -.25 );
                dAngle = (double)-0.786;
                break;

            case WorldDirections.WEST:
                dAngle = (double)(Math.PI * -.5);
                break;

            case WorldDirections.NORTHWEST:
                dAngle = (double)(Math.PI * -.75);
                break;

        }

        if (fReverse)
        {
            dMovementDist = dMovementDist * -1;
        }

        MoveMerc(pSoldier, dMovementDist, dAngle, false);

    }

    void BeginSoldierClimbUpRoof(SOLDIERTYPE? pSoldier)
    {
        WorldDirections bNewDirection;

        if (FindHeigherLevel(pSoldier, pSoldier.sGridNo, pSoldier.bDirection, out bNewDirection) && (pSoldier.bLevel == 0))
        {
            if (EnoughPoints(pSoldier, GetAPsToClimbRoof(pSoldier, false), 0, true))
            {
                if (pSoldier.bTeam == gbPlayerNum)
                {
                    // OK, SET INTERFACE FIRST
                    SetUIBusy(pSoldier.ubID);
                }

                pSoldier.sTempNewGridNo = IsometricUtils.NewGridNo(pSoldier.sGridNo, IsometricUtils.DirectionInc(bNewDirection));

                pSoldier.ubPendingDirection = bNewDirection;
                //pSoldier.usPendingAnimation = CLIMBUPROOF;
                EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.CLIMBUPROOF, 0, false);

                InternalReceivingSoldierCancelServices(pSoldier, false);
                InternalGivingSoldierCancelServices(pSoldier, false);

            }
        }

    }

    void BeginSoldierClimbFence(SOLDIERTYPE? pSoldier)
    {
        WorldDirections bDirection;

        if (FindFenceJumpDirection(pSoldier, pSoldier.sGridNo, pSoldier.bDirection, out bDirection))
        {
            pSoldier.sTempNewGridNo = IsometricUtils.NewGridNo(pSoldier.sGridNo, IsometricUtils.DirectionInc(bDirection));
            pSoldier.fDontChargeTurningAPs = true;
            EVENT_InternalSetSoldierDesiredDirection(pSoldier, bDirection, false, pSoldier.usAnimState);
            pSoldier.fTurningUntilDone = true;
            // ATE: Reset flag to go back to prone...
            pSoldier.fTurningFromPronePosition = TURNING_FROM_PRONE_OFF;
            pSoldier.usPendingAnimation = AnimationStates.HOPFENCE;
        }

    }

    int SleepDartSuccumbChance(SOLDIERTYPE? pSoldier)
    {
        int uiChance;
        int bEffectiveStrength;

        // figure out base chance of succumbing, 
        bEffectiveStrength = SkillChecks.EffectiveStrength(pSoldier);

        if (bEffectiveStrength > 90)
        {
            uiChance = 110 - bEffectiveStrength;
        }
        else if (bEffectiveStrength > 80)
        {
            uiChance = 120 - bEffectiveStrength;
        }
        else if (bEffectiveStrength > 70)
        {
            uiChance = 130 - bEffectiveStrength;
        }
        else
        {
            uiChance = 140 - bEffectiveStrength;
        }

        // add in a bonus based on how long it's been since shot... highest chance at the beginning
        uiChance += (10 - pSoldier.bSleepDrugCounter);

        return (uiChance);
    }

    void BeginSoldierGetup(SOLDIERTYPE? pSoldier)
    {
        // RETURN IF WE ARE BEING SERVICED
        if (pSoldier.ubServiceCount > 0)
        {
            return;
        }

        // ATE: Don't getup if we are in a meanwhile
        if (Meanwhile.AreInMeanwhile())
        {
            return;
        }

        if (pSoldier.bCollapsed)
        {
            if (pSoldier.bLife >= OKLIFE && pSoldier.bBreath >= OKBREATH && (pSoldier.bSleepDrugCounter == 0))
            {
                // get up you hoser!

                pSoldier.bCollapsed = false;
                pSoldier.bTurnsCollapsed = 0;

                if (IS_MERC_BODY_TYPE(pSoldier))
                {
                    switch (pSoldier.usAnimState)
                    {
                        case AnimationStates.FALLOFF_FORWARD_STOP:
                        case AnimationStates.PRONE_LAYFROMHIT_STOP:
                        case AnimationStates.STAND_FALLFORWARD_STOP:
                            ChangeSoldierStance(pSoldier, AnimationHeights.ANIM_CROUCH);
                            break;

                        case AnimationStates.FALLBACKHIT_STOP:
                        case AnimationStates.FALLOFF_STOP:
                        case AnimationStates.FLYBACKHIT_STOP:
                        case AnimationStates.FALLBACK_HIT_STAND:
                        case AnimationStates.FALLOFF:
                        case AnimationStates.FLYBACK_HIT:

                            // ROLL OVER
                            EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.ROLLOVER, 0, false);
                            break;

                        default:

                            ChangeSoldierStance(pSoldier, AnimationHeights.ANIM_CROUCH);
                            break;
                    }
                }
                else
                {
                    EVENT_InitNewSoldierAnim(pSoldier, END_COWER, 0, false);
                }
            }
            else
            {
                pSoldier.bTurnsCollapsed++;
                if ((gTacticalStatus.bBoxingState == BOXING) && (pSoldier.uiStatusFlags.HasFlag(SOLDIER.BOXER)))
                {
                    if (pSoldier.bTurnsCollapsed > 1)
                    {
                        // We have a winnah!  But it isn't this boxer!
                        EndBoxingMatch(pSoldier);
                    }
                }
            }
        }
        else if (pSoldier.bSleepDrugCounter > 0)
        {
            int uiChance;

            uiChance = SleepDartSuccumbChance(pSoldier);

            if (PreRandom(100) < uiChance)
            {
                // succumb to the drug!
                Points.DeductPoints(pSoldier, 0, (int)(pSoldier.bBreathMax * 100));
                SoldierCollapse(pSoldier);
            }
        }

        if (pSoldier.bSleepDrugCounter > 0)
        {
            pSoldier.bSleepDrugCounter--;
        }
    }


    public static void HandleTakeDamageDeath(SOLDIERTYPE? pSoldier, int bOldLife, TAKE_DAMAGE ubReason)
    {
        switch (ubReason)
        {
            case TAKE_DAMAGE.BLOODLOSS:
            case TAKE_DAMAGE.ELECTRICITY:
            case TAKE_DAMAGE.GAS:

                if (pSoldier.bInSector)
                {
                    if (pSoldier.bVisible != -1)
                    {
                        if (ubReason != TAKE_DAMAGE.BLOODLOSS)
                        {
                            DoMercBattleSound(pSoldier, BATTLE_SOUND_DIE1);
                            pSoldier.fDeadSoundPlayed = true;
                        }
                    }

                    if ((ubReason == TAKE_DAMAGE.ELECTRICITY) && pSoldier.bLife < OKLIFE)
                    {
                        pSoldier.fInNonintAnim = false;
                    }

                    // Check for < OKLIFE
                    if (pSoldier.bLife < OKLIFE && pSoldier.bLife != 0 && !pSoldier.bCollapsed)
                    {
                        SoldierCollapse(pSoldier);
                    }

                    // THis is for the die animation that will be happening....
                    if (pSoldier.bLife == 0)
                    {
                        pSoldier.fDoingExternalDeath = true;
                    }

                    // Check if he is dead....
                    CheckForAndHandleSoldierDyingNotFromHit(pSoldier);

                }

                //if( !( guiTacticalInterfaceFlags.HasFlag(INTERFACE.MAPSCREEN )) )
                {
                    HandleSoldierTakeDamageFeedback(pSoldier);
                }

                if ((guiTacticalInterfaceFlags.HasFlag(INTERFACE.MAPSCREEN)) || !pSoldier.bInSector)
                {
                    if (pSoldier.bLife == 0 && !(pSoldier.uiStatusFlags.HasFlag(SOLDIER.DEAD)))
                    {
                        StrategicHandlePlayerTeamMercDeath(pSoldier);

                        // ATE: Here, force always to use die sound...
                        pSoldier.fDieSoundUsed = false;
                        DoMercBattleSound(pSoldier, BATTLE_SOUND_DIE1);
                        pSoldier.fDeadSoundPlayed = true;

                        // ATE: DO death sound
                        //PlayJA2Sample((int)DOORCR_1, RATE_11025, HIGHVOLUME, 1, MIDDLEPAN);
                        //PlayJA2Sample((int)HEADCR_1, RATE_11025, HIGHVOLUME, 1, MIDDLEPAN);
                    }
                }
                break;
        }

        if (ubReason == TAKE_DAMAGE.ELECTRICITY)
        {
            if (pSoldier.bLife >= OKLIFE)
            {
                //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("Freeing up attacker from electricity damage"));
                ReleaseSoldiersAttacker(pSoldier);
            }
        }
    }


    public static int SoldierTakeDamage(SOLDIERTYPE? pSoldier, AnimationHeights bHeight, int sLifeDeduct, int sBreathLoss, TAKE_DAMAGE ubReason, int ubAttacker, int sSourceGrid, int sSubsequent, bool fShowDamage)
    {
        int bOldLife;
        int ubCombinedLoss;
        int bBandage;
        int sAPCost;
        int ubBlood;


        pSoldier.ubLastDamageReason = ubReason;


        // CJC Jan 21 99: add check to see if we are hurting an enemy in an enemy-controlled
        // sector; if so, this is a sign of player activity
        switch (pSoldier.bTeam)
        {
            case TEAM.ENEMY_TEAM:
                // if we're in the wilderness this always counts
                if (strategicMap[CALCULATE_STRATEGIC_INDEX(gWorldSectorX, gWorldSectorY)].fEnemyControlled || SectorInfo[SECTORINFO.SECTOR(gWorldSectorX, gWorldSectorY)].ubTraversability[(StrategicMove)STRATEGIC_MOVE.THROUGH] != Traversability.TOWN)
                {
                    // update current day of activity!
                    StrategicStatus.UpdateLastDayOfPlayerActivity(GameClock.GetWorldDay());
                }
                break;
            case TEAM.CREATURE_TEAM:
                // always a sign of activity?
                StrategicStatus.UpdateLastDayOfPlayerActivity(GameClock.GetWorldDay());
                break;
            case TEAM.CIV_TEAM:
                if (pSoldier.ubCivilianGroup == CIV_GROUP.KINGPIN_CIV_GROUP && gubQuest[QUEST.RESCUE_MARIA] == QUESTINPROGRESS && gTacticalStatus.bBoxingState == BoxingStates.NOT_BOXING)
                {
                    SOLDIERTYPE? pMaria = SoldierProfileSubSystem.FindSoldierByProfileID(NPCID.MARIA, false);
                    if (pMaria is not null && pMaria.bActive && pMaria.bInSector)
                    {
                        Facts.SetFactTrue(FACT.MARIA_ESCAPE_NOTICED);
                    }
                }
                break;
            default:
                break;
        }

        // Deduct life!, Show damage if we want!
        bOldLife = pSoldier.bLife;

        // OK, If we are a vehicle.... damage vehicle...( people inside... )
        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE))
        {
            if (TANK(pSoldier))
            {
                //sLifeDeduct = (sLifeDeduct * 2) / 3;
            }
            else
            {
                if (ubReason == TAKE_DAMAGE.GUNFIRE)
                {
                    sLifeDeduct /= 3;
                }
                else if (ubReason == TAKE_DAMAGE.EXPLOSION && sLifeDeduct > 50)
                {
                    // boom!
                    sLifeDeduct *= 2;
                }
            }

            VehicleTakeDamage(pSoldier.bVehicleID, ubReason, sLifeDeduct, pSoldier.sGridNo, ubAttacker);
            HandleTakeDamageDeath(pSoldier, bOldLife, ubReason);
            return (0);
        }

        // ATE: If we are elloit being attacked in a meanwhile...
        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.NPC_SHOOTING))
        {
            // Almost kill but not quite.....
            sLifeDeduct = (pSoldier.bLife - 1);
            // Turn off
            pSoldier.uiStatusFlags &= (~SOLDIER.NPC_SHOOTING);
        }

        // CJC: make sure Elliot doesn't bleed to death!
        if (ubReason == TAKE_DAMAGE.BLOODLOSS && Meanwhile.AreInMeanwhile())
        {
            return (0);
        }


        // Calculate bandage
        bBandage = pSoldier.bLifeMax - pSoldier.bLife - pSoldier.bBleeding;

        if (guiCurrentScreen == ScreenName.MAP_SCREEN)
        {
            fReDrawFace = true;
        }

        if (CREATURE_OR_BLOODCAT(pSoldier))
        {
            int sReductionFactor = 0;

            if (pSoldier.ubBodyType == SoldierBodyTypes.BLOODCAT)
            {
                sReductionFactor = 2;
            }
            else if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.MONSTER))
            {
                switch (pSoldier.ubBodyType)
                {
                    case LARVAE_MONSTER:
                    case INFANT_MONSTER:
                        sReductionFactor = 1;
                        break;
                    case YAF_MONSTER:
                    case YAM_MONSTER:
                        sReductionFactor = 4;
                        break;
                    case ADULTFEMALEMONSTER:
                    case AM_MONSTER:
                        sReductionFactor = 6;
                        break;
                    case QUEENMONSTER:
                        // increase with range!
                        if (ubAttacker == NOBODY)
                        {
                            sReductionFactor = 8;
                        }
                        else
                        {
                            sReductionFactor = 4 + IsometricUtils.PythSpacesAway(MercPtrs[ubAttacker].sGridNo, pSoldier.sGridNo) / 2;
                        }
                        break;
                }
            }

            if (ubReason == TAKE_DAMAGE.EXPLOSION)
            {
                sReductionFactor /= 4;
            }
            if (sReductionFactor > 1)
            {
                sLifeDeduct = (sLifeDeduct + (sReductionFactor / 2)) / sReductionFactor;
            }
            else if (ubReason == TAKE_DAMAGE.EXPLOSION)
            {
                // take at most 2/3rds
                sLifeDeduct = (sLifeDeduct * 2) / 3;
            }

            // reduce breath loss to a smaller degree, except for the queen...
            if (pSoldier.ubBodyType == SoldierBodyTypes.QUEENMONSTER)
            {
                // in fact, reduce breath loss by MORE!
                sReductionFactor = Math.Min(sReductionFactor, 8);
                sReductionFactor *= 2;
            }
            else
            {
                sReductionFactor /= 2;
            }
            if (sReductionFactor > 1)
            {
                sBreathLoss = (sBreathLoss + (sReductionFactor / 2)) / sReductionFactor;
            }
        }

        if (sLifeDeduct > pSoldier.bLife)
        {
            pSoldier.bLife = 0;
        }
        else
        {
            // Decrease Health
            pSoldier.bLife -= sLifeDeduct;
        }

        // ATE: Put some logic in here to allow enemies to die quicker.....
        // Are we an enemy?
        if (pSoldier.bSide != gbPlayerNum && pSoldier.bNeutral == 0 && pSoldier.ubProfile == NO_PROFILE)
        {
            // ATE: Give them a chance to fall down...
            if (pSoldier.IsAlive && pSoldier.bLife < (OKLIFE - 1))
            {
                // Are we taking damage from bleeding?
                if (ubReason == TAKE_DAMAGE.BLOODLOSS)
                {
                    // Fifty-fifty chance to die now!
                    if (Globals.Random.Next(2) == 0 || gTacticalStatus.Team[pSoldier.bTeam].bMenInSector == 1)
                    {
                        // Kill!
                        pSoldier.bLife = 0;
                    }
                }
                else
                {
                    // OK, see how far we are..
                    if (pSoldier.bLife < (OKLIFE - 3))
                    {
                        // Kill!
                        pSoldier.bLife = 0;
                    }
                }
            }
        }

        if (fShowDamage)
        {
            pSoldier.sDamage += sLifeDeduct;
        }

        // Truncate life
        if (pSoldier.bLife < 0)
        {
            pSoldier.bLife = 0;
        }


        // Calculate damage to our items if from an explosion!
        if (ubReason == TAKE_DAMAGE.EXPLOSION || ubReason == TAKE_DAMAGE.STRUCTURE_EXPLOSION)
        {
            CheckEquipmentForDamage(pSoldier, sLifeDeduct);
        }



        // Calculate bleeding
        if (ubReason != TAKE_DAMAGE.GAS && !AM_A_ROBOT(pSoldier))
        {
            if (ubReason == TAKE_DAMAGE.HANDTOHAND)
            {
                if (sLifeDeduct > 0)
                {
                    // HTH does 1 pt bleeding per hit
                    pSoldier.bBleeding = pSoldier.bBleeding + 1;
                }
            }
            else
            {
                pSoldier.bBleeding = pSoldier.bLifeMax - (pSoldier.bLife + bBandage);
            }

        }

        // Deduct breath AND APs!
        sAPCost = (sLifeDeduct / AP.GET_WOUNDED_DIVISOR); // + fallCost;

        // ATE: if the robot, do not deduct
        if (!AM_A_ROBOT(pSoldier))
        {
            DeductPoints(pSoldier, sAPCost, sBreathLoss);
        }

        ubCombinedLoss = (int)sLifeDeduct / 10 + sBreathLoss / 2000;

        // Add shock
        if (!AM_A_ROBOT(pSoldier))
        {
            pSoldier.bShock += ubCombinedLoss;
        }

        // start the stopwatch - the blood is gushing!
        pSoldier.dNextBleed = CalcSoldierNextBleed(pSoldier);

        if (pSoldier.bInSector && pSoldier.bVisible != -1)
        {
            // If we are already dead, don't show damage!
            if (bOldLife != 0 && fShowDamage && sLifeDeduct != 0 && sLifeDeduct < 1000)
            {
                // Display damage
                int sOffsetX, sOffsetY;

                // Set Damage display counter
                pSoldier.fDisplayDamage = 1;
                pSoldier.bDisplayDamageCount = 0;

                if (pSoldier.ubBodyType == SoldierBodyTypes.QUEENMONSTER)
                {
                    pSoldier.sDamageX = 0;
                    pSoldier.sDamageY = 0;
                }
                else
                {
                    GetSoldierAnimOffsets(pSoldier, out sOffsetX, out sOffsetY);
                    pSoldier.sDamageX = sOffsetX;
                    pSoldier.sDamageY = sOffsetY;
                }
            }
        }

        // OK, if here, let's see if we should drop our weapon....
        if (ubReason != TAKE_DAMAGE.BLOODLOSS && !(AM_A_ROBOT(pSoldier)))
        {
            int sTestOne, sTestTwo, sChanceToDrop;
            int bVisible = -1;

            sTestOne = SkillChecks.EffectiveStrength(pSoldier);
            sTestTwo = (2 * (Math.Max(sLifeDeduct, (sBreathLoss / 100))));


            if (pSoldier.ubAttackerID != NOBODY && MercPtrs[pSoldier.ubAttackerID].ubBodyType == SoldierBodyTypes.BLOODCAT)
            {
                // bloodcat boost, let them make people drop items more
                sTestTwo += 20;
            }

            // If damage > effective strength....
            sChanceToDrop = (Math.Max(0, (sTestTwo - sTestOne)));

            // ATE: Increase odds of NOT dropping an UNDROPPABLE OBJECT
            if ((pSoldier.inv[InventorySlot.HANDPOS].fFlags.HasFlag(OBJECT.UNDROPPABLE)))
            {
                sChanceToDrop -= 30;
            }

            if (Globals.Random.Next(100) < (int)sChanceToDrop)
            {
                // OK, drop item in main hand...
                if (pSoldier.inv[InventorySlot.HANDPOS].usItem != NOTHING)
                {
                    if (!(pSoldier.inv[InventorySlot.HANDPOS].fFlags.HasFlag(OBJECT.UNDROPPABLE)))
                    {
                        // ATE: if our guy, make visible....
                        if (pSoldier.bTeam == gbPlayerNum)
                        {
                            bVisible = 1;
                        }

                        AddItemToPool(pSoldier.sGridNo, (pSoldier.inv[InventorySlot.HANDPOS]), bVisible, pSoldier.bLevel, 0, -1);
                        ItemSubSystem.DeleteObj((pSoldier.inv[InventorySlot.HANDPOS]));
                    }
                }
            }
        }

        // Drop some blood!
        // decide blood amt, if any
        ubBlood = (sLifeDeduct / BLOODDIVISOR);
        if (ubBlood > MAXBLOODQUANTITY)
        {
            ubBlood = MAXBLOODQUANTITY;
        }

        if (!(pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE | SOLDIER.ROBOT)))
        {
            if (ubBlood != 0)
            {
                if (pSoldier.bInSector)
                {
                    DropBlood(pSoldier, ubBlood, pSoldier.bVisible);
                }
            }
        }

        //Set UI Flag for unconscious, if it's our own guy!
        if (pSoldier.bTeam == gbPlayerNum)
        {
            if (pSoldier.bLife < OKLIFE && pSoldier.IsAlive && bOldLife >= OKLIFE)
            {
                pSoldier.fUIFirstTimeUNCON = true;
                fInterfacePanelDirty = DIRTYLEVEL2;
            }
        }

        if (pSoldier.bInSector)
        {
            CheckForBreathCollapse(pSoldier);
        }

        // EXPERIENCE CLASS GAIN (combLoss): Getting wounded in battle

        Interface.DirtyMercPanelInterface(pSoldier, DIRTYLEVEL1);


        if (ubAttacker != NOBODY)
        {
            // don't give exp for hitting friends!
            if ((MercPtrs[ubAttacker].bTeam == gbPlayerNum) && (pSoldier.bTeam != gbPlayerNum))
            {
                if (ubReason == TAKE_DAMAGE.EXPLOSION)
                {
                    // EXPLOSIVES GAIN (combLoss):  Causing wounds in battle
                    Campaign.StatChange(MercPtrs[ubAttacker], Stat.EXPLODEAMT, (int)(10 * ubCombinedLoss), FROM_FAILURE);
                }
                /*
                else if ( ubReason == TAKE_DAMAGE.GUNFIRE )
                {
                    // MARKSMANSHIP GAIN (combLoss):  Causing wounds in battle
                    StatChange( MercPtrs[ ubAttacker ], MARKAMT, (int)( 5 * ubCombinedLoss ), false );
                }
                */
            }
        }


        if (PTR_OURTEAM(pSoldier))
        {
            // EXPERIENCE GAIN: Took some damage
            Campaign.StatChange(pSoldier, Stat.EXPERAMT, (int)(5 * ubCombinedLoss), FROM_FAILURE);

            // Check for quote
            if (!(pSoldier.usQuoteSaidFlags.HasFlag(SOLDIER_QUOTE.SAID_BEING_PUMMELED)))
            {
                // Check attacker!
                if (ubAttacker != NOBODY && ubAttacker != pSoldier.ubID)
                {
                    pSoldier.bNumHitsThisTurn++;

                    if ((pSoldier.bNumHitsThisTurn >= 3) && (pSoldier.bLife - pSoldier.bOldLife > 20))
                    {
                        if (Globals.Random.Next(100) < (int)((40 * (pSoldier.bNumHitsThisTurn - 2))))
                        {
                            DelayedTacticalCharacterDialogue(pSoldier, QUOTE.TAKEN_A_BREATING);
                            pSoldier.usQuoteSaidFlags |= SOLDIER_QUOTE.SAID_BEING_PUMMELED;
                            pSoldier.bNumHitsThisTurn = 0;
                        }
                    }
                }
            }
        }

        if ((ubAttacker != NOBODY) && (Menptr[ubAttacker].bTeam == OUR_TEAM) && (pSoldier.ubProfile != NO_PROFILE) && (pSoldier.ubProfile >= FIRST_RPC))
        {
            gMercProfiles[pSoldier.ubProfile].ubMiscFlags |= ProfileMiscFlags1.PROFILE_MISC_FLAG_WOUNDEDBYPLAYER;
            if (pSoldier.ubProfile == (NPCID)114)
            {
                Facts.SetFactTrue(FACT.PACOS_KILLED);
            }
        }

        HandleTakeDamageDeath(pSoldier, bOldLife, ubReason);

        // Check if we are < unconscious, and shutup if so! also wipe sight
        if (pSoldier.bLife < CONSCIOUSNESS)
        {
            Faces.ShutupaYoFace(pSoldier.iFaceIndex);
        }

        if (pSoldier.bLife < OKLIFE)
        {
            DecayIndividualOpplist(pSoldier);
        }


        return (ubCombinedLoss);
    }

    bool InternalDoMercBattleSound(SOLDIERTYPE? pSoldier, BATTLE_SOUND ubBattleSoundID, int bSpecialCode)
    {
        SGPFILENAME zFilename;
        SOUNDPARMS spParms;
        int ubSoundID;
        int uiSoundID;
        int iFaceIndex;
        bool fDoSub = false;
        SoundDefine uiSubSoundID = 0;
        bool fSpeechSound = false;

        // doubleCHECK RANGE
        CHECKF(ubBattleSoundID < NUM_MERC_BATTLE_SOUNDS);

        if ((pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE)))
        {
            // Pick a passenger from vehicle....
            pSoldier = PickRandomPassengerFromVehicle(pSoldier);

            if (pSoldier == null)
            {
                return (false);
            }

        }

        // If a death sound, and we have already done ours...
        if (ubBattleSoundID == BATTLE_SOUND_DIE1)
        {
            if (pSoldier.fDieSoundUsed)
            {
                return (true);
            }
        }


        // Are we mute?
        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.MUTE))
        {
            return (false);
        }


        //	uiTimeSameBattleSndDone

        // If we are a creature, etc, pick a better sound...
        if (ubBattleSoundID == BATTLE_SOUND_HIT1 || ubBattleSoundID == BATTLE_SOUND_HIT2)
        {
            switch (pSoldier.ubBodyType)
            {
                case SoldierBodyTypes.COW:

                    fDoSub = true;
                    uiSubSoundID = SoundDefine.COW_HIT_SND;
                    break;

                case SoldierBodyTypes.YAF_MONSTER:
                case SoldierBodyTypes.YAM_MONSTER:
                case SoldierBodyTypes.ADULTFEMALEMONSTER:
                case SoldierBodyTypes.AM_MONSTER:

                    fDoSub = true;

                    if (Globals.Random.Next(2) == 0)
                    {
                        uiSubSoundID = SoundDefine.ACR_DIE_PART1;
                    }
                    else
                    {
                        uiSubSoundID = SoundDefine.ACR_LUNGE;
                    }
                    break;

                case SoldierBodyTypes.INFANT_MONSTER:

                    fDoSub = true;
                    uiSubSoundID = SoundDefine.BCR_SHRIEK;
                    break;

                case SoldierBodyTypes.QUEENMONSTER:

                    fDoSub = true;
                    uiSubSoundID = SoundDefine.LQ_SHRIEK;
                    break;

                case SoldierBodyTypes.LARVAE_MONSTER:

                    fDoSub = true;
                    uiSubSoundID = SoundDefine.BCR_SHRIEK;
                    break;

                case SoldierBodyTypes.BLOODCAT:

                    fDoSub = true;
                    uiSubSoundID = SoundDefine.BLOODCAT_HIT_1;
                    break;

                case SoldierBodyTypes.ROBOTNOWEAPON:

                    fDoSub = true;
                    uiSubSoundID = (SoundDefine.S_METAL_IMPACT1 + Globals.Random.Next(2));
                    break;
            }
        }

        if (ubBattleSoundID == BATTLE_SOUND_DIE1)
        {
            switch (pSoldier.ubBodyType)
            {
                case SoldierBodyTypes.COW:

                    fDoSub = true;
                    uiSubSoundID = SoundDefine.COW_DIE_SND;
                    break;

                case SoldierBodyTypes.YAF_MONSTER:
                case SoldierBodyTypes.YAM_MONSTER:
                case SoldierBodyTypes.ADULTFEMALEMONSTER:
                case SoldierBodyTypes.AM_MONSTER:

                    fDoSub = true;
                    uiSubSoundID = SoundDefine.CREATURE_FALL_PART_2;
                    break;

                case SoldierBodyTypes.INFANT_MONSTER:

                    fDoSub = true;
                    uiSubSoundID = SoundDefine.BCR_DYING;
                    break;

                case SoldierBodyTypes.LARVAE_MONSTER:

                    fDoSub = true;
                    uiSubSoundID = SoundDefine.LCR_RUPTURE;
                    break;

                case SoldierBodyTypes.QUEENMONSTER:

                    fDoSub = true;
                    uiSubSoundID = SoundDefine.LQ_DYING;
                    break;

                case SoldierBodyTypes.BLOODCAT:

                    fDoSub = true;
                    uiSubSoundID = SoundDefine.BLOODCAT_DIE_1;
                    break;

                case SoldierBodyTypes.ROBOTNOWEAPON:

                    fDoSub = true;
                    uiSubSoundID = (SoundDefine.EXPLOSION_1);
                    //PlayJA2Sample(ROBOT_DEATH, RATE_11025, HIGHVOLUME, 1, MIDDLEPAN);
                    break;

            }
        }

        // OK. any other sound, not hits, robot makes a beep
        if (pSoldier.ubBodyType == SoldierBodyTypes.ROBOTNOWEAPON && !fDoSub)
        {
            fDoSub = true;
            if (ubBattleSoundID == BATTLE_SOUND.ATTN1)
            {
                uiSubSoundID = SoundDefine.ROBOT_GREETING;
            }
            else
            {
                uiSubSoundID = SoundDefine.ROBOT_BEEP;
            }
        }

        if (fDoSub)
        {
            if (guiCurrentScreen != ScreenName.GAME_SCREEN)
            {
                //PlayJA2Sample(uiSubSoundID, RATE_11025, HIGHVOLUME, 1, MIDDLEPAN);
            }
            else
            {
                //PlayJA2Sample(uiSubSoundID, RATE_11025, SoundVolume((int)CalculateSpeechVolume(HIGHVOLUME), pSoldier.sGridNo), 1, SoundDir(pSoldier.sGridNo));
            }
            return (true);
        }

        // Check if this is the same one we just played...
        if (pSoldier.bOldBattleSnd == ubBattleSoundID
            && gBattleSndsData[ubBattleSoundID].fDontAllowTwoInRow)
        {
            // Are we below the min delay?
            if ((GetJA2Clock() - pSoldier.uiTimeSameBattleSndDone) < MIN_SUBSEQUENT_SNDS_DELAY)
            {
                return (true);
            }
        }

        // If a battle snd is STILL playing....
        if (SoundIsPlaying(pSoldier.uiBattleSoundID))
        {
            // We can do a few things here....
            // Is this a crutial one...?
            if (gBattleSndsData[ubBattleSoundID].fStopDialogue == 1)
            {
                // Stop playing origonal
                SoundStop(pSoldier.uiBattleSoundID);
            }
            else
            {
                // Skip this one...
                return (true);
            }
        }

        // If we are talking now....
        if (IsMercSayingDialogue(pSoldier.ubProfile))
        {
            // We can do a couple of things now...
            if (gBattleSndsData[ubBattleSoundID].fStopDialogue == 1)
            {
                // Stop dialigue...
                DialogueAdvanceSpeech();
            }
            else if (gBattleSndsData[ubBattleSoundID].fStopDialogue == 2)
            {
                // Skip battle snd...
                return (true);
            }
        }


        // Save this one we're doing...
        pSoldier.bOldBattleSnd = ubBattleSoundID;
        pSoldier.uiTimeSameBattleSndDone = GetJA2Clock();


        // Adjust based on morale...
        if (ubBattleSoundID == BATTLE_SOUND_OK1 && pSoldier.bMorale < LOW_MORALE_BATTLE_SND_THREASHOLD)
        {
            ubBattleSoundID = BATTLE_SOUND_LOWMARALE_OK1;
        }
        if (ubBattleSoundID == BATTLE_SOUND_ATTN1 && pSoldier.bMorale < LOW_MORALE_BATTLE_SND_THREASHOLD)
        {
            ubBattleSoundID = BATTLE_SOUND_LOWMARALE_ATTN1;
        }

        ubSoundID = ubBattleSoundID;

        //if the sound to be played is a confirmation, check to see if we are to play it
        if (ubSoundID == BATTLE_SOUND_OK1)
        {
            if (GameSettings.fOptions[TOPTION.MUTE_CONFIRMATIONS])
            {
                return (true);
            }
            //else a speech sound is to be played
            else
            {
                fSpeechSound = true;
            }
        }

        // Randomize between sounds, if appropriate
        if (gBattleSndsData[ubSoundID].ubRandomVal != 0)
        {
            ubSoundID = ubSoundID + (int)Globals.Random.Next(gBattleSndsData[ubSoundID].ubRandomVal);

        }


        // OK, build file and play!
        if (pSoldier.ubProfile != NO_PROFILE)
        {
            sprintf(zFilename, "BATTLESNDS\\%03d_%s.wav", pSoldier.ubProfile, gBattleSndsData[ubSoundID].zName);

            if (!FileManager.FileExists(zFilename))
            {
                // OK, temp build file...
                if (pSoldier.ubBodyType == REGFEMALE)
                {
                    sprintf(zFilename, "BATTLESNDS\\f_%s.wav", gBattleSndsData[ubSoundID].zName);
                }
                else
                {
                    sprintf(zFilename, "BATTLESNDS\\m_%s.wav", gBattleSndsData[ubSoundID].zName);
                }
            }
        }
        else
        {
            // Check if we can play this!
            if (!gBattleSndsData[ubSoundID].fBadGuy)
            {
                return (false);
            }

            if (pSoldier.ubBodyType == HATKIDCIV || pSoldier.ubBodyType == KIDCIV)
            {
                if (ubSoundID == BATTLE_SOUND_DIE1)
                {
                    sprintf(zFilename, "BATTLESNDS\\kid%d_dying.wav", pSoldier.ubBattleSoundID);
                }
                else
                {
                    sprintf(zFilename, "BATTLESNDS\\kid%d_%s.wav", pSoldier.ubBattleSoundID, gBattleSndsData[ubSoundID].zName);
                }
            }
            else
            {
                if (ubSoundID == BATTLE_SOUND_DIE1)
                {
                    sprintf(zFilename, "BATTLESNDS\\bad%d_die.wav", pSoldier.ubBattleSoundID);
                }
                else
                {
                    sprintf(zFilename, "BATTLESNDS\\bad%d_%s.wav", pSoldier.ubBattleSoundID, gBattleSndsData[ubSoundID].zName);
                }
            }
        }

        // Play sound!
        //memset(&spParms, 0xff, sizeof(SOUNDPARMS));

        spParms.uiSpeed = RATE_11025;
        //spParms.uiVolume = CalculateSpeechVolume( pSoldier.bVocalVolume );

        spParms.uiVolume = (int)CalculateSpeechVolume(HIGHVOLUME);

        // ATE: Reduce volume for OK sounds...
        // ( Only for all-moves or multi-selection cases... )
        if (bSpecialCode == BATTLE_SND_LOWER_VOLUME)
        {
            spParms.uiVolume = (int)CalculateSpeechVolume(MIDVOLUME);
        }

        // If we are an enemy.....reduce due to volume
        if (pSoldier.bTeam != gbPlayerNum)
        {
            spParms.uiVolume = SoundVolume((int)spParms.uiVolume, pSoldier.sGridNo);
        }

        spParms.uiLoop = 1;
        spParms.uiPan = SoundDir(pSoldier.sGridNo);
        spParms.uiPriority = GROUP_PLAYER;

        if ((uiSoundID = SoundPlay(zFilename, &spParms)) == SOUND_ERROR)
        {
            return (false);
        }
        else
        {
            pSoldier.uiBattleSoundID = uiSoundID;

            if (pSoldier.ubProfile != NO_PROFILE)
            {
                // Get soldier's face ID
                iFaceIndex = pSoldier.iFaceIndex;

                // Check face index
                if (iFaceIndex != -1)
                {
                    ExternSetFaceTalking(iFaceIndex, uiSoundID);
                }
            }

            return (true);
        }
    }

    bool DoMercBattleSound(SOLDIERTYPE? pSoldier, int ubBattleSoundID)
    {
        // We WANT to play some RIGHT AWAY.....
        if (gBattleSndsData[ubBattleSoundID].fStopDialogue == 1 || (pSoldier.ubProfile == NO_PROFILE) || InOverheadMap())
        {
            return (InternalDoMercBattleSound(pSoldier, ubBattleSoundID, 0));
        }

        // So here, only if we were currently saying dialogue.....
        if (!IsMercSayingDialogue(pSoldier.ubProfile))
        {
            return (InternalDoMercBattleSound(pSoldier, ubBattleSoundID, 0));
        }

        // OK, queue it up otherwise!
        TacticalCharacterDialogueWithSpecialEvent(pSoldier, 0, DIALOGUE_SPECIAL_EVENT_DO_BATTLE_SND, ubBattleSoundID, 0);

        return (true);
    }


    bool PreloadSoldierBattleSounds(SOLDIERTYPE? pSoldier, bool fRemove)
    {
        int cnt;

        CHECKF(pSoldier.bActive != false);

        for (cnt = 0; cnt < NUM_MERC_BATTLE_SOUNDS; cnt++)
        {
            // OK, build file and play!
            if (pSoldier.ubProfile != NO_PROFILE)
            {
                if (gBattleSndsData[cnt].fPreload)
                {
                    if (fRemove)
                    {
                        SoundUnlockSample(gBattleSndsData[cnt].zName);
                    }
                    else
                    {
                        SoundLockSample(gBattleSndsData[cnt].zName);
                    }
                }
            }
            else
            {
                if (gBattleSndsData[cnt].fPreload && gBattleSndsData[cnt].fBadGuy)
                {
                    if (fRemove)
                    {
                        SoundUnlockSample(gBattleSndsData[cnt].zName);
                    }
                    else
                    {
                        SoundLockSample(gBattleSndsData[cnt].zName);
                    }
                }
            }
        }

        return (true);
    }



    bool CheckSoldierHitRoof(SOLDIERTYPE? pSoldier)
    {
        // Check if we are near a lower level
        WorldDirections bNewDirection;
        bool fReturnVal = false;
        int sNewGridNo;
        // Default to true
        bool fDoForwards = true;

        if (pSoldier.bLife >= OKLIFE)
        {
            return (false);
        }

        if (FindLowerLevel(pSoldier, pSoldier.sGridNo, pSoldier.bDirection, out bNewDirection) && (pSoldier.bLevel > 0))
        {
            // ONly if standing!
            if (gAnimControl[pSoldier.usAnimState].ubHeight == AnimationHeights.ANIM_STAND)
            {
                // We are near a lower level.
                // Use opposite direction
                bNewDirection = gOppositeDirection[bNewDirection];

                // Alrighty, let's not blindly change here, look at whether the dest gridno is good!
                sNewGridNo = IsometricUtils.NewGridNo(pSoldier.sGridNo, IsometricUtils.DirectionInc(gOppositeDirection[bNewDirection]));
                if (!NewOKDestination(pSoldier, sNewGridNo, true, 0))
                {
                    return (false);
                }
                sNewGridNo = IsometricUtils.NewGridNo(sNewGridNo, IsometricUtils.DirectionInc(gOppositeDirection[bNewDirection]));
                if (!NewOKDestination(pSoldier, sNewGridNo, true, 0))
                {
                    return (false);
                }

                // Are wee near enough to fall forwards....
                if (pSoldier.bDirection == gOneCDirection[bNewDirection] ||
                         pSoldier.bDirection == gTwoCDirection[bNewDirection] ||
                         pSoldier.bDirection == bNewDirection ||
                         pSoldier.bDirection == gOneCCDirection[bNewDirection] ||
                         pSoldier.bDirection == gTwoCCDirection[bNewDirection])
                {
                    // Do backwards...
                    fDoForwards = false;
                }

                // If we are facing the opposite direction, fall backwards
                // ATE: Make this more usefull...
                if (fDoForwards)
                {
                    pSoldier.sTempNewGridNo = IsometricUtils.NewGridNo(pSoldier.sGridNo, (int)(-1 * IsometricUtils.DirectionInc(bNewDirection)));
                    pSoldier.sTempNewGridNo = IsometricUtils.NewGridNo(pSoldier.sTempNewGridNo, (int)(-1 * IsometricUtils.DirectionInc(bNewDirection)));
                    EVENT_SetSoldierDesiredDirection(pSoldier, gOppositeDirection[bNewDirection]);
                    pSoldier.fTurningUntilDone = true;
                    pSoldier.usPendingAnimation = AnimationStates.FALLFORWARD_ROOF;
                    //EVENT_InitNewSoldierAnim( pSoldier, FALLFORWARD_ROOF, 0 , false );

                    // Deduct hitpoints/breath for falling!
                    SoldierTakeDamage(pSoldier, AnimationHeights.ANIM_CROUCH, 100, 5000, TAKE_DAMAGE.FALLROOF, NOBODY, NOWHERE, 0, true);

                    fReturnVal = true;

                }
                else
                {

                    pSoldier.sTempNewGridNo = IsometricUtils.NewGridNo((int)pSoldier.sGridNo, (int)(-1 * IsometricUtils.DirectionInc(bNewDirection)));
                    pSoldier.sTempNewGridNo = IsometricUtils.NewGridNo((int)pSoldier.sTempNewGridNo, (int)(-1 * IsometricUtils.DirectionInc(bNewDirection)));
                    EVENT_SetSoldierDesiredDirection(pSoldier, bNewDirection);
                    pSoldier.fTurningUntilDone = true;
                    pSoldier.usPendingAnimation = AnimationStates.FALLOFF;

                    // Deduct hitpoints/breath for falling!
                    SoldierTakeDamage(pSoldier, AnimationHeights.ANIM_CROUCH, 100, 5000, TAKE_DAMAGE.FALLROOF, NOBODY, NOWHERE, 0, true);

                    fReturnVal = true;
                }
            }
        }

        return (fReturnVal);
    }

    void BeginSoldierClimbDownRoof(SOLDIERTYPE? pSoldier)
    {
        WorldDirections bNewDirection;

        if (FindLowerLevel(pSoldier, pSoldier.sGridNo, pSoldier.bDirection, out bNewDirection) && (pSoldier.bLevel > 0))
        {
            if (EnoughPoints(pSoldier, GetAPsToClimbRoof(pSoldier, true), 0, true))
            {
                if (pSoldier.bTeam == gbPlayerNum)
                {
                    // OK, SET INTERFACE FIRST
                    SetUIBusy(pSoldier.ubID);
                }

                pSoldier.sTempNewGridNo = IsometricUtils.NewGridNo(pSoldier.sGridNo, IsometricUtils.DirectionInc(bNewDirection));

                bNewDirection = gTwoCDirection[bNewDirection];

                pSoldier.ubPendingDirection = bNewDirection;
                EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.CLIMBDOWNROOF, 0, false);

                InternalReceivingSoldierCancelServices(pSoldier, false);
                InternalGivingSoldierCancelServices(pSoldier, false);

            }
        }

    }

    void MoveMerc(SOLDIERTYPE pSoldier, float dMovementChange, double dAngle, bool fCheckRange)
    {
        int dDegAngle;
        float dDeltaPos;
        float dXPos, dYPos;
        bool fStop = false;

        dDegAngle = (int)(dAngle * 180 / PI);
        //sprintf( gDebugStr, "Move Angle: %d", (int)dDegAngle );

        // Find delta Movement for X pos
        dDeltaPos = (float)(dMovementChange * Math.Sin(dAngle));

        // Find new position
        dXPos = pSoldier.dXPos + dDeltaPos;

        if (fCheckRange)
        {
            fStop = false;

            switch (pSoldier.bMovementDirection)
            {
                case WorldDirections.NORTHEAST:
                case WorldDirections.EAST:
                case WorldDirections.SOUTHEAST:

                    if (dXPos >= pSoldier.sDestXPos)
                    {
                        fStop = true;
                    }
                    break;

                case WorldDirections.NORTHWEST:
                case WorldDirections.WEST:
                case WorldDirections.SOUTHWEST:

                    if (dXPos <= pSoldier.sDestXPos)
                    {
                        fStop = true;
                    }
                    break;

                case WorldDirections.NORTH:
                case WorldDirections.SOUTH:

                    fStop = true;
                    break;

            }

            if (fStop)
            {
                //dXPos = pSoldier.sDestXPos;
                pSoldier.fPastXDest = 1;

                if (pSoldier.sGridNo == pSoldier.sFinalDestination)
                {
                    dXPos = pSoldier.sDestXPos;
                }
            }
        }

        // Find delta Movement for Y pos
        dDeltaPos = (float)(dMovementChange * Math.Cos(dAngle));

        // Find new pos
        dYPos = pSoldier.dYPos + dDeltaPos;

        if (fCheckRange)
        {
            fStop = false;

            switch (pSoldier.bMovementDirection)
            {
                case WorldDirections.NORTH:
                case WorldDirections.NORTHEAST:
                case WorldDirections.NORTHWEST:

                    if (dYPos <= pSoldier.sDestYPos)
                    {
                        fStop = true;
                    }
                    break;

                case WorldDirections.SOUTH:
                case WorldDirections.SOUTHWEST:
                case WorldDirections.SOUTHEAST:

                    if (dYPos >= pSoldier.sDestYPos)
                    {
                        fStop = true;
                    }
                    break;

                case WorldDirections.EAST:
                case WorldDirections.WEST:

                    fStop = true;
                    break;

            }

            if (fStop)
            {
                //dYPos = pSoldier.sDestYPos;
                pSoldier.fPastYDest = 1;

                if (pSoldier.sGridNo == pSoldier.sFinalDestination)
                {
                    dYPos = pSoldier.sDestYPos;
                }
            }
        }

        // OK, set new position
        EVENT_InternalSetSoldierPosition(pSoldier, dXPos, dYPos, false, false, false);

        //	DebugMsg( TOPIC_JA2, DBG_LEVEL_3, String("X: %f Y: %f", dXPos, dYPos ) );

    }

    public static WorldDirections GetDirectionFromGridNo(int sGridNo, SOLDIERTYPE? pSoldier)
    {
        int sXPos, sYPos;

        IsometricUtils.ConvertGridNoToXY(sGridNo, out sXPos, out sYPos);

        return (GetDirectionFromXY(sXPos, sYPos, pSoldier));
    }

    public static WorldDirections GetDirectionToGridNoFromGridNo(int sGridNoDest, int sGridNoSrc)
    {
        int sXPos2, sYPos2;
        int sXPos, sYPos;

        IsometricUtils.ConvertGridNoToXY(sGridNoSrc, out sXPos, out sYPos);
        IsometricUtils.ConvertGridNoToXY(sGridNoDest, out sXPos2, out sYPos2);

        return (atan8(sXPos2, sYPos2, sXPos, sYPos));

    }

    public static WorldDirections GetDirectionFromXY(int sXPos, int sYPos, SOLDIERTYPE? pSoldier)
    {
        int sXPos2, sYPos2;

        IsometricUtils.ConvertGridNoToXY(pSoldier.sGridNo, out sXPos2, out sYPos2);

        return (atan8(sXPos2, sYPos2, sXPos, sYPos));
    }

#if false
int  atan8( int x1, int y1, int x2, int y2 )
{
static int trig[8] = { 2, 3, 4, 5, 6, 7, 8, 1 };
// returned values are N=1, NE=2, E=3, SE=4, S=5, SW=6, W=7, NW=8
	double dx=(x2-x1);
	double dy=(y2-y1);
	double a;
	int i,k;
	if (dx==0)
		dx=0.00390625; // 1/256th
//#define PISLICES (8)
	a=(atan2(dy,dx) + PI/PISLICES)/(PI/(PISLICES/2));
	i=(int)a;
	if (a>0)
		k=i; else
	if (a<0)
		k=i+(PISLICES-1); else
		k=0;
	return(trig[k]);
}
#endif

    //#if 0
    public static WorldDirections atan8(int sXPos, int sYPos, int sXPos2, int sYPos2)
    {
        double test_x = sXPos2 - sXPos;
        double test_y = sYPos2 - sYPos;
        WorldDirections mFacing = WorldDirections.WEST;
        int dDegAngle;
        double angle;

        if (test_x == 0)
        {
            test_x = 0.04;
        }

        angle = atan2(test_x, test_y);


        dDegAngle = (int)(angle * 180 / PI);
        //sprintf( gDebugStr, "Move Angle: %d", (int)dDegAngle );

        do
        {
            if (angle >= -PI * .375 && angle <= -PI * .125)
            {
                mFacing = WorldDirections.SOUTHWEST;
                break;
            }

            if (angle <= PI * .375 && angle >= PI * .125)
            {
                mFacing = WorldDirections.SOUTHEAST;
                break;
            }

            if (angle >= PI * .623 && angle <= PI * .875)
            {
                mFacing = WorldDirections.NORTHEAST;
                break;
            }

            if (angle <= -PI * .623 && angle >= -PI * .875)
            {
                mFacing = WorldDirections.NORTHWEST;
                break;
            }

            if (angle > -PI * 0.125 && angle < PI * 0.125)
            {
                mFacing = WorldDirections.SOUTH;
            }
            if (angle > PI * 0.375 && angle < PI * 0.623)
            {
                mFacing = WorldDirections.EAST;
            }
            if ((angle > PI * 0.875 && angle <= PI) || (angle > -PI && angle < -PI * 0.875))
            {
                mFacing = WorldDirections.NORTH;
            }
            if (angle > -PI * 0.623 && angle < -PI * 0.375)
            {
                mFacing = WorldDirections.WEST;
            }

        } while (false);

        return (mFacing);
    }


    WorldDirections atan8FromAngle(double angle)
    {
        WorldDirections mFacing = WorldDirections.WEST;

        if (angle > Math.PI)
        {
            angle = (angle - Math.PI) - Math.PI;
        }
        if (angle < -Math.PI)
        {
            angle = (Math.PI - (Math.Abs(angle) - Math.PI));
        }

        do
        {
            if (angle >= -Math.PI * .375 && angle <= -Math.PI * .125)
            {
                mFacing = WorldDirections.SOUTHWEST;
                break;
            }

            if (angle <= Math.PI * .375 && angle >= Math.PI * .125)
            {
                mFacing = WorldDirections.SOUTHEAST;
                break;
            }

            if (angle >= Math.PI * .623 && angle <= Math.PI * .875)
            {
                mFacing = WorldDirections.NORTHEAST;
                break;
            }

            if (angle <= -Math.PI * .623 && angle >= -Math.PI * .875)
            {
                mFacing = WorldDirections.NORTHWEST;
                break;
            }

            if (angle > -Math.PI * 0.125 && angle < Math.PI * 0.125)
            {
                mFacing = WorldDirections.SOUTH;
            }
            if (angle > Math.PI * 0.375 && angle < Math.PI * 0.623)
            {
                mFacing = WorldDirections.EAST;
            }
            if ((angle > Math.PI * 0.875 && angle <= Math.PI) || (angle > -Math.PI && angle < -Math.PI * 0.875))
            {
                mFacing = WorldDirections.NORTH;
            }
            if (angle > -Math.PI * 0.623 && angle < -Math.PI * 0.375)
            {
                mFacing = WorldDirections.WEST;
            }

        } while (false);

        return (mFacing);
    }


    void CheckForFullStructures(SOLDIERTYPE? pSoldier)
    {
        // This function checks to see if we are near a specific structure type which requires us to blit a
        // small obscuring peice
        int sGridNo;
        int? usFullTileIndex;
        int cnt;


        // Check in all 'Above' directions
        for (cnt = 0; cnt < MAX_FULLTILE_DIRECTIONS; cnt++)
        {
            sGridNo = pSoldier.sGridNo + gsFullTileDirections[cnt];

            if (CheckForFullStruct(sGridNo, out usFullTileIndex))
            {
                // Add one for the item's obsuring part
                pSoldier.usFrontArcFullTileList[cnt] = usFullTileIndex + 1;
                pSoldier.usFrontArcFullTileGridNos[cnt] = sGridNo;
                AddTopmostToHead(sGridNo, pSoldier.usFrontArcFullTileList[cnt]);
            }
            else
            {
                if (pSoldier.usFrontArcFullTileList[cnt] != 0)
                {
                    RemoveTopmost(pSoldier.usFrontArcFullTileGridNos[cnt], pSoldier.usFrontArcFullTileList[cnt]);
                }
                pSoldier.usFrontArcFullTileList[cnt] = 0;
                pSoldier.usFrontArcFullTileGridNos[cnt] = 0;
            }
        }

    }


    bool CheckForFullStruct(int sGridNo, out int? pusIndex)
    {
        LEVELNODE? pStruct = null;
        LEVELNODE? pOldStruct = null;
        int fTileFlags;

        pStruct = gpWorldLevelData[sGridNo].pStructHead;

        // Look through all structs and Search for type

        while (pStruct != null)
        {

            if (pStruct.usIndex != NO_TILE && pStruct.usIndex < (int)TileDefines.NUMBEROFTILES)
            {

                GetTileFlags(pStruct.usIndex, fTileFlags);

                // Advance to next
                pOldStruct = pStruct;
                pStruct = pStruct.pNext;

                //if( (pOldStruct.pStructureData!=null) && ( pOldStruct.pStructureData.fFlags&STRUCTURE_TREE ) )
                if (fTileFlags & FULL3D_TILE)
                {
                    // CHECK IF THIS TREE IS FAIRLY ALONE!
                    if (FullStructAlone(sGridNo, 2))
                    {
                        // Return true and return index
                        pusIndex = pOldStruct.usIndex;
                        return (true);
                    }
                    else
                    {
                        pusIndex = null;
                        return (false);
                    }

                }

            }
            else
            {
                // Advance to next
                pOldStruct = pStruct;
                pStruct = pStruct.pNext;
            }

        }

        // Could not find it, return false
        pusIndex = null;
        return (false);
    }


    bool FullStructAlone(int sGridNo, int ubRadius)
    {
        int sTop, sBottom;
        int sLeft, sRight;
        int cnt1, cnt2;
        int iNewIndex;
        int leftmost;


        // Determine start end end indicies and num rows
        sTop = ubRadius;
        sBottom = -ubRadius;
        sLeft = -ubRadius;
        sRight = ubRadius;

        for (cnt1 = sBottom; cnt1 <= sTop; cnt1++)
        {

            leftmost = ((sGridNo + (WORLD_COLS * cnt1)) / WORLD_COLS) * WORLD_COLS;

            for (cnt2 = sLeft; cnt2 <= sRight; cnt2++)
            {
                iNewIndex = sGridNo + (WORLD_COLS * cnt1) + cnt2;


                if (iNewIndex >= 0 && iNewIndex < WORLD_MAX &&
                       iNewIndex >= leftmost && iNewIndex < (leftmost + WORLD_COLS))
                {
                    if (iNewIndex != sGridNo)
                    {
                        if (StructureInternals.FindStructure(iNewIndex, STRUCTUREFLAGS.TREE) != null)
                        {
                            return (false);
                        }
                    }
                }

            }
        }

        return (true);
    }


    public static void AdjustForFastTurnAnimation(SOLDIERTYPE? pSoldier)
    {

        // CHECK FOR FASTTURN ANIMATIONS
        // ATE: Mod: Only fastturn for OUR guys!
        if (gAnimControl[pSoldier.usAnimState].uiFlags & ANIM_FASTTURN && pSoldier.bTeam == gbPlayerNum && !(pSoldier.uiStatusFlags.HasFlag(SOLDIER.TURNINGFROMHIT)))
        {
            if (pSoldier.bDirection != pSoldier.bDesiredDirection)
            {
                pSoldier.sAniDelay = FAST_TURN_ANIM_SPEED;
            }
            else
            {
                SetSoldierAniSpeed(pSoldier);
                //	FreeUpNPCFromTurning( pSoldier, LOOK);
            }
        }

    }

    bool IsActionInterruptable(SOLDIERTYPE? pSoldier)
    {
        if (gAnimControl[pSoldier.usAnimState].uiFlags & ANIM_NONINTERRUPT)
        {
            return (false);
        }
        return (true);
    }

    // WRAPPER FUNCTIONS FOR SOLDIER EVENTS
    void SendSoldierPositionEvent(SOLDIERTYPE? pSoldier, double dNewXPos, double dNewYPos)
    {
        // Sent event for position update
        EV_S_SETPOSITION SSetPosition;

        SSetPosition.usSoldierID = pSoldier.ubID;
        SSetPosition.uiUniqueId = pSoldier.uiUniqueSoldierIdValue;

        SSetPosition.dNewXPos = dNewXPos;
        SSetPosition.dNewYPos = dNewYPos;

        AddGameEvent(S_SETPOSITION, 0, &SSetPosition);

    }

    void SendSoldierDestinationEvent(SOLDIERTYPE? pSoldier, int usNewDestination)
    {
        // Sent event for position update
        EV_S_CHANGEDEST SChangeDest;

        SChangeDest.usSoldierID = pSoldier.ubID;
        SChangeDest.usNewDestination = usNewDestination;
        SChangeDest.uiUniqueId = pSoldier.uiUniqueSoldierIdValue;

        AddGameEvent(S_CHANGEDEST, 0, &SChangeDest);

    }

    void SendSoldierSetDirectionEvent(SOLDIERTYPE? pSoldier, int usNewDirection)
    {
        // Sent event for position update
        EV_S_SETDIRECTION SSetDirection;

        SSetDirection.usSoldierID = pSoldier.ubID;
        SSetDirection.usNewDirection = usNewDirection;
        SSetDirection.uiUniqueId = pSoldier.uiUniqueSoldierIdValue;

        AddGameEvent(S_SETDIRECTION, 0, &SSetDirection);

    }

    public static void SendSoldierSetDesiredDirectionEvent(SOLDIERTYPE? pSoldier, WorldDirections usDesiredDirection)
    {
        // Sent event for position update
        EV_S_SETDESIREDDIRECTION SSetDesiredDirection;

        SSetDesiredDirection.usSoldierID = pSoldier.ubID;
        SSetDesiredDirection.usDesiredDirection = usDesiredDirection;
        SSetDesiredDirection.uiUniqueId = pSoldier.uiUniqueSoldierIdValue;

        AddGameEvent(S_SETDESIREDDIRECTION, 0, &SSetDesiredDirection);

    }

    void SendGetNewSoldierPathEvent(SOLDIERTYPE? pSoldier, int sDestGridNo, int usMovementAnim)
    {
        EV_S_GETNEWPATH SGetNewPath;

        SGetNewPath.usSoldierID = pSoldier.ubID;
        SGetNewPath.sDestGridNo = sDestGridNo;
        SGetNewPath.usMovementAnim = usMovementAnim;
        SGetNewPath.uiUniqueId = pSoldier.uiUniqueSoldierIdValue;

        AddGameEvent(S_GETNEWPATH, 0, &SGetNewPath);
    }


    public static void SendChangeSoldierStanceEvent(SOLDIERTYPE? pSoldier, AnimationHeights ubNewStance)
    {
        ChangeSoldierStance(pSoldier, ubNewStance);
    }


    public static void SendBeginFireWeaponEvent(SOLDIERTYPE? pSoldier, int sTargetGridNo)
    {
        EV_S_BEGINFIREWEAPON SBeginFireWeapon;

        SBeginFireWeapon.usSoldierID = pSoldier.ubID;
        SBeginFireWeapon.sTargetGridNo = sTargetGridNo;
        SBeginFireWeapon.bTargetLevel = pSoldier.bTargetLevel;
        SBeginFireWeapon.bTargetCubeLevel = pSoldier.bTargetCubeLevel;
        SBeginFireWeapon.uiUniqueId = pSoldier.uiUniqueSoldierIdValue;

        AddGameEvent(S_BEGINFIREWEAPON, 0, &SBeginFireWeapon);

    }

    // This function just encapolates the check for turnbased and having an attacker in the first place
    public static void ReleaseSoldiersAttacker(SOLDIERTYPE? pSoldier)
    {
        int cnt;
        int ubNumToFree;

        //if ( gTacticalStatus.uiFlags & TURNBASED && (gTacticalStatus.uiFlags & INCOMBAT) )
        {
            // ATE: Removed...
            //if ( pSoldier.ubAttackerID != NOBODY )
            {
                // JA2 Gold
                // set next-to-previous attacker, so long as this isn't a repeat attack
                if (pSoldier.ubPreviousAttackerID != pSoldier.ubAttackerID)
                {
                    pSoldier.ubNextToPreviousAttackerID = pSoldier.ubPreviousAttackerID;
                }

                // get previous attacker id
                pSoldier.ubPreviousAttackerID = pSoldier.ubAttackerID;

                // Copy BeingAttackedCount here....
                ubNumToFree = pSoldier.bBeingAttackedCount;
                // Zero it out BEFORE, as supression may increase it again...
                pSoldier.bBeingAttackedCount = 0;

                for (cnt = 0; cnt < ubNumToFree; cnt++)
                {
                    //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("@@@@@@@ Freeing up attacker of %d (attacker is %d) - releasesoldierattacker num to free is %d", pSoldier.ubID, pSoldier.ubAttackerID, ubNumToFree));
                    ReduceAttackBusyCount(pSoldier.ubAttackerID, false);
                }

                // ATE: Set to NOBODY if this person is NOT dead
                // otherise, we keep it so the kill can be awarded!
                if (pSoldier.bLife != 0 && pSoldier.ubBodyType != SoldierBodyTypes.QUEENMONSTER)
                {
                    pSoldier.ubAttackerID = NOBODY;
                }
            }
        }
    }

    public static bool MercInWater(SOLDIERTYPE? pSoldier)
    {
        // Our water texture , for now is of a given type
        if (pSoldier.bOverTerrainType == TerrainTypeDefines.LOW_WATER
            || pSoldier.bOverTerrainType == TerrainTypeDefines.MED_WATER
            || pSoldier.bOverTerrainType == TerrainTypeDefines.DEEP_WATER)
        {
            return (true);
        }
        else
        {
            return (false);
        }
    }


    void RevivePlayerTeam()
    {
        int cnt;

        // End the turn of player charactors
        cnt = gTacticalStatus.Team[gbPlayerNum].bFirstID;

        // look for all mercs on the same team, 
        //for (pSoldier = MercPtrs[cnt]; cnt <= gTacticalStatus.Team[gbPlayerNum].bLastID; cnt++, pSoldier++)
        foreach (var pSoldier in MercPtrs)
        {
            ReviveSoldier(pSoldier);
        }

    }


    void ReviveSoldier(SOLDIERTYPE? pSoldier)
    {
        int sX, sY;

        if (pSoldier.bLife < OKLIFE && pSoldier.bActive)
        {
            // If dead or unconscious, revive!
            pSoldier.uiStatusFlags &= (~SOLDIER.DEAD);

            pSoldier.bLife = pSoldier.bLifeMax;
            pSoldier.bBleeding = 0;
            pSoldier.ubDesiredHeight = AnimationHeights.ANIM_STAND;

            AddManToTeam(pSoldier.bTeam);

            // Set to standing
            pSoldier.fInNonintAnim = false;
            pSoldier.fRTInNonintAnim = false;

            // Change to standing,unless we can getup with an animation
            EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.STANDING, 0, true);
            BeginSoldierGetup(pSoldier);

            // Makesure center of tile
            sX = IsometricUtils.CenterX(pSoldier.sGridNo);
            sY = IsometricUtils.CenterY(pSoldier.sGridNo);

            EVENT_SetSoldierPosition(pSoldier, (float)sX, (float)sY);

            // Dirty INterface
            fInterfacePanelDirty = DIRTYLEVEL2;

        }

    }

    public static void HandleAnimationProfile(SOLDIERTYPE? pSoldier, AnimationStates usAnimState, bool fRemove)
    {
        //#if 0
        ANIM_PROF pProfile;
        ANIM_PROF_DIR pProfileDir;
        ANIM_PROF_TILE pProfileTile;
        NPCID bProfileID;
        int iTileCount;
        int sGridNo;
        AnimationSurfaceTypes usAnimSurface;

        // ATE

        // Get Surface Index
        usAnimSurface = AnimationControl.DetermineSoldierAnimationSurface(pSoldier, usAnimState);

        CHECKV(usAnimSurface != INVALID_ANIMATION_SURFACE);

        bProfileID = gAnimSurfaceDatabase[usAnimSurface].bProfile;

        // Determine if this animation has a profile
        if (bProfileID != NPCID.UNSET)
        {
            // Getprofile
            pProfile = (gpAnimProfiles[bProfileID]);

            // Get direction
            pProfileDir = (pProfile.Dirs[pSoldier.bDirection]);

            // Loop tiles and set accordingly into world
            for (iTileCount = 0; iTileCount < pProfileDir.ubNumTiles; iTileCount++)
            {
                pProfileTile = (pProfileDir.pTiles[iTileCount]);

                sGridNo = pSoldier.sGridNo + ((WORLD_COLS * pProfileTile.bTileY) + pProfileTile.bTileX);

                // Check if in bounds
                if (!OutOfBounds(pSoldier.sGridNo, sGridNo))
                {
                    if (fRemove)
                    {
                        // Remove from world
                        RemoveMerc(sGridNo, pSoldier, true);
                    }
                    else
                    {
                        // PLace into world
                        AddMercToHead(sGridNo, pSoldier, false);
                        //if ( pProfileTile.bTileY != 0 || pProfileTile.bTileX != 0 )
                        {
                            gpWorldLevelData[sGridNo].pMercHead.uiFlags |= LEVELNODEFLAGS.MERCPLACEHOLDER;
                            gpWorldLevelData[sGridNo].pMercHead.uiAnimHitLocationFlags = pProfileTile.usTileFlags;
                        }
                    }
                }

            }
        }

        //#endif

    }


    LEVELNODE? GetAnimProfileFlags(int sGridNo, int usFlags, SOLDIERTYPE? ppTargSoldier, LEVELNODE? pGivenNode)
    {
        LEVELNODE? pNode;

        (ppTargSoldier) = null;
        (usFlags) = 0;

        if (pGivenNode == null)
        {
            pNode = gpWorldLevelData[sGridNo].pMercHead;
        }
        else
        {
            pNode = pGivenNode.pNext;
        }

        //#if 0

        if (pNode != null)
        {
            if (pNode.uiFlags.HasFlag(LEVELNODEFLAGS.MERCPLACEHOLDER))
            {
                (usFlags) = (int)pNode.uiAnimHitLocationFlags;
                (ppTargSoldier) = pNode.pSoldier;
            }
        }

        //#endif

        return (pNode);

    }


    bool GetProfileFlagsFromGridno(SOLDIERTYPE pSoldier, AnimationStates usAnimState, int sTestGridNo, out uint usFlags)
    {
        ANIM_PROF pProfile;
        ANIM_PROF_DIR? pProfileDir;
        ANIM_PROF_TILE? pProfileTile;
        NPCID bProfileID;
        int iTileCount;
        int sGridNo;
        AnimationSurfaceTypes usAnimSurface;

        // Get Surface Index
        usAnimSurface = AnimationControl.DetermineSoldierAnimationSurface(pSoldier, usAnimState);

        CHECKF(usAnimSurface != INVALID_ANIMATION_SURFACE);

        bProfileID = gAnimSurfaceDatabase[usAnimSurface].bProfile;

        usFlags = 0;

        // Determine if this animation has a profile
        if (bProfileID != NPCID.UNSET)
        {
            // Getprofile
            pProfile = (gpAnimProfiles[bProfileID]);

            // Get direction
            pProfileDir = (pProfile.Dirs[pSoldier.bDirection]);

            // Loop tiles and set accordingly into world
            for (iTileCount = 0; iTileCount < pProfileDir.ubNumTiles; iTileCount++)
            {
                pProfileTile = (pProfileDir.pTiles[iTileCount]);

                sGridNo = pSoldier.sGridNo + ((WORLD_COLS * pProfileTile.bTileY) + pProfileTile.bTileX);

                // Check if in bounds
                if (!OutOfBounds(pSoldier.sGridNo, sGridNo))
                {
                    if (sGridNo == sTestGridNo)
                    {
                        usFlags = pProfileTile.usTileFlags;
                        return (true);
                    }
                }

            }
        }

        return (false);
    }


    void EVENT_SoldierBeginGiveItem(SOLDIERTYPE pSoldier)
    {
        SOLDIERTYPE pTSoldier;

        if (VerifyGiveItem(pSoldier, out pTSoldier))
        {
            // CHANGE DIRECTION AND GOTO ANIMATION NOW
            pSoldier.bDesiredDirection = (WorldDirections)pSoldier.bPendingActionData3;
            pSoldier.bDirection = (WorldDirections)pSoldier.bPendingActionData3;

            // begin animation
            EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.GIVE_ITEM, 0, false);

        }
        else
        {
            UnSetEngagedInConvFromPCAction(pSoldier);

            MemFree(pSoldier.pTempObject);
        }
    }


    void EVENT_SoldierBeginBladeAttack(SOLDIERTYPE pSoldier, int sGridNo, WorldDirections ubDirection)
    {
        SOLDIERTYPE pTSoldier;
        //int uiMercFlags;
        int usSoldierIndex;
        WorldDirections ubTDirection;
        bool fChangeDirection = false;
        ROTTING_CORPSE? pCorpse;

        // Increment the number of people busy doing stuff because of an attack
        //if ( (gTacticalStatus.uiFlags & TURNBASED) && (gTacticalStatus.uiFlags & INCOMBAT) )
        //{
        gTacticalStatus.ubAttackBusyCount++;
        //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("Begin blade attack: ATB  %d", gTacticalStatus.ubAttackBusyCount));

        //}

        // CHANGE DIRECTION AND GOTO ANIMATION NOW
        EVENT_SetSoldierDesiredDirection(pSoldier, ubDirection);
        EVENT_SetSoldierDirection(pSoldier, ubDirection);
        // CHANGE TO ANIMATION

        // DETERMINE ANIMATION TO PLAY 
        // LATER BASED ON IF TAREGT KNOWS OF US, STANCE, ETC
        // GET POINTER TO TAREGT
        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.MONSTER))
        {
            int ubTargetID;

            // Is there an unconscious guy at gridno......
            ubTargetID = WorldManager.WhoIsThere2(sGridNo, pSoldier.bTargetLevel);

            if (ubTargetID != NOBODY && ((MercPtrs[ubTargetID].bLife < OKLIFE && MercPtrs[ubTargetID].IsAlive) || (MercPtrs[ubTargetID].bBreath < OKBREATH && MercPtrs[ubTargetID].bCollapsed)))
            {
                pSoldier.uiPendingActionData4 = ubTargetID;
                // add regen bonus
                pSoldier.bRegenerationCounter++;
                EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.MONSTER_BEGIN_EATTING_FLESH, 0, false);
            }
            else
            {
                if (IsometricUtils.PythSpacesAway(pSoldier.sGridNo, sGridNo) <= 1)
                {
                    EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.MONSTER_CLOSE_ATTACK, 0, false);
                }
                else
                {
                    EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.ADULTMONSTER_ATTACKING, 0, false);
                }
            }
        }
        else if (pSoldier.ubBodyType == SoldierBodyTypes.BLOODCAT)
        {
            // Check if it's a claws or teeth...
            if (pSoldier.inv[InventorySlot.HANDPOS].usItem == Items.BLOODCAT_CLAW_ATTACK)
            {
                EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.BLOODCAT_SWIPE, 0, false);
            }
            else
            {
                EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.BLOODCAT_BITE_ANIM, 0, false);
            }
        }
        else
        {
            usSoldierIndex = WorldManager.WhoIsThere2(sGridNo, pSoldier.bTargetLevel);
            if (usSoldierIndex != NOBODY)
            {
                Overhead.GetSoldier(out pTSoldier, usSoldierIndex);

                // Look at stance of target
                switch (gAnimControl[pTSoldier.usAnimState].ubEndHeight)
                {
                    case AnimationHeights.ANIM_STAND:
                    case AnimationHeights.ANIM_CROUCH:

                        // CHECK IF HE CAN SEE US, IF SO RANDOMIZE
                        if (pTSoldier.bOppList[pSoldier.ubID] == 0 && pTSoldier.bTeam != pSoldier.bTeam)
                        {
                            // WE ARE NOT SEEN
                            EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.STAB, 0, false);
                        }
                        else
                        {
                            // WE ARE SEEN
                            if (Globals.Random.Next(50) > 25)
                            {
                                EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.STAB, 0, false);
                            }
                            else
                            {
                                EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.SLICE, 0, false);
                            }

                            // IF WE ARE SEEN, MAKE SURE GUY TURNS!
                            // Get direction to target
                            // IF WE ARE AN ANIMAL, CAR, MONSTER, DONT'T TURN
                            if (!(pTSoldier.uiStatusFlags.HasFlag(SOLDIER.MONSTER | SOLDIER.ANIMAL | SOLDIER.VEHICLE)))
                            {
                                // OK, stop merc....
                                EVENT_StopMerc(pTSoldier, pTSoldier.sGridNo, pTSoldier.bDirection);

                                if (pTSoldier.bTeam != gbPlayerNum)
                                {
                                    AIMain.CancelAIAction(pTSoldier, 1);
                                }

                                ubTDirection = GetDirectionFromGridNo(pSoldier.sGridNo, pTSoldier);
                                SendSoldierSetDesiredDirectionEvent(pTSoldier, ubTDirection);
                            }
                        }

                        break;

                    case AnimationHeights.ANIM_PRONE:

                        // CHECK OUR STANCE
                        if (gAnimControl[pSoldier.usAnimState].ubEndHeight != AnimationHeights.ANIM_CROUCH)
                        {
                            // SET DESIRED STANCE AND SET PENDING ANIMATION
                            SendChangeSoldierStanceEvent(pSoldier, AnimationHeights.ANIM_CROUCH);
                            pSoldier.usPendingAnimation = CROUCH_STAB;
                        }
                        else
                        {
                            // USE crouched one
                            // NEED TO CHANGE STANCE IF NOT CROUCHD!
                            EVENT_InitNewSoldierAnim(pSoldier, CROUCH_STAB, 0, false);
                        }
                        break;
                }
            }
            else
            {
                // OK, SEE IF THERE IS AN OBSTACLE HERE...
                if (!NewOKDestination(pSoldier, sGridNo, false, pSoldier.bLevel))
                {
                    EVENT_InitNewSoldierAnim(pSoldier, STAB, 0, false);
                }
                else
                {
                    // Check for corpse!
                    pCorpse = GetCorpseAtGridNo(sGridNo, pSoldier.bLevel);

                    if (pCorpse == null)
                    {
                        EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.CROUCH_STAB, 0, false);
                    }
                    else
                    {
                        if (IsValidDecapitationCorpse(pCorpse))
                        {
                            EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.DECAPITATE, 0, false);
                        }
                        else
                        {
                            EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.CROUCH_STAB, 0, false);
                        }
                    }
                }
            }
        }

        // SET TARGET GRIDNO
        pSoldier.sTargetGridNo = sGridNo;
        pSoldier.bTargetLevel = pSoldier.bLevel;
        pSoldier.ubTargetID = WorldManager.WhoIsThere2(sGridNo, pSoldier.bTargetLevel);
    }


    public static void EVENT_SoldierBeginPunchAttack(SOLDIERTYPE? pSoldier, int sGridNo, WorldDirections ubDirection)
    {
        bool fMartialArtist = false;
        SOLDIERTYPE? pTSoldier;
        //int uiMercFlags;
        int usSoldierIndex;
        int ubTDirection;
        bool fChangeDirection = false;
        Items usItem;

        // Get item in hand...
        usItem = pSoldier.inv[InventorySlot.HANDPOS].usItem;


        // Increment the number of people busy doing stuff because of an attack
        //if ( (gTacticalStatus.uiFlags & TURNBASED) && (gTacticalStatus.uiFlags & INCOMBAT) )
        //{
        gTacticalStatus.ubAttackBusyCount++;
        //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, string.Format("Begin HTH attack: ATB  %d", gTacticalStatus.ubAttackBusyCount));

        //}

        // get target.....
        usSoldierIndex = WorldManager.WhoIsThere2(pSoldier.sTargetGridNo, pSoldier.bLevel);
        if (usSoldierIndex != NOBODY)
        {
            Overhead.GetSoldier(out pTSoldier, usSoldierIndex);

            fChangeDirection = true;
        }
        else
        {
            return;
        }


        if (fChangeDirection)
        {
            // CHANGE DIRECTION AND GOTO ANIMATION NOW
            EVENT_SetSoldierDesiredDirection(pSoldier, ubDirection);
            EVENT_SetSoldierDirection(pSoldier, ubDirection);
        }


        // Are we a martial artist?
        if (HAS_SKILL_TRAIT(pSoldier, SkillTrait.MARTIALARTS))
        {
            fMartialArtist = true;
        }


        if (fMartialArtist && !Meanwhile.AreInMeanwhile() && usItem != Items.CROWBAR)
        {
            // Are we in attack mode yet?
            if (pSoldier.usAnimState != AnimationStates.NINJA_BREATH
                && gAnimControl[pSoldier.usAnimState].ubHeight == AnimationHeights.ANIM_STAND
                && gAnimControl[pTSoldier.usAnimState].ubHeight != AnimationHeights.ANIM_PRONE)
            {
                EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.NINJA_GOTOBREATH, 0, false);
            }
            else
            {
                DoNinjaAttack(pSoldier);
            }
        }
        else
        {
            // Look at stance of target
            switch (gAnimControl[pTSoldier.usAnimState].ubEndHeight)
            {
                case AnimationHeights.ANIM_STAND:
                case AnimationHeights.ANIM_CROUCH:

                    if (usItem != Items.CROWBAR)
                    {
                        EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.PUNCH, 0, false);
                    }
                    else
                    {
                        EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.CROWBAR_ATTACK, 0, false);
                    }

                    // CHECK IF HE CAN SEE US, IF SO CHANGE DIR
                    if (pTSoldier.bOppList[pSoldier.ubID] == 0 && pTSoldier.bTeam != pSoldier.bTeam)
                    {
                        // Get direction to target
                        // IF WE ARE AN ANIMAL, CAR, MONSTER, DONT'T TURN
                        if (!(pTSoldier.uiStatusFlags.HasFlag(SOLDIER.MONSTER | SOLDIER.ANIMAL | SOLDIER.VEHICLE)))
                        {
                            // OK, stop merc....
                            EVENT_StopMerc(pTSoldier, pTSoldier.sGridNo, pTSoldier.bDirection);

                            if (pTSoldier.bTeam != gbPlayerNum)
                            {
                                AIMain.CancelAIAction(pTSoldier, 1);
                            }

                            ubTDirection = (int)GetDirectionFromGridNo(pSoldier.sGridNo, pTSoldier);
                            SendSoldierSetDesiredDirectionEvent(pTSoldier, ubTDirection);
                        }
                    }
                    break;

                case AnimationHeights.ANIM_PRONE:

                    // CHECK OUR STANCE
                    // ATE: Added this for CIV body types 'cause of elliot
                    if (!IS_MERC_BODY_TYPE(pSoldier))
                    {
                        EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.PUNCH, 0, false);
                    }
                    else
                    {
                        if (gAnimControl[pSoldier.usAnimState].ubEndHeight != AnimationHeights.ANIM_CROUCH)
                        {
                            // SET DESIRED STANCE AND SET PENDING ANIMATION
                            SendChangeSoldierStanceEvent(pSoldier, AnimationHeights.ANIM_CROUCH);
                            pSoldier.usPendingAnimation = AnimationStates.PUNCH_LOW;
                        }
                        else
                        {
                            // USE crouched one
                            // NEED TO CHANGE STANCE IF NOT CROUCHD!
                            EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.PUNCH_LOW, 0, false);
                        }
                    }
                    break;
            }
        }

        // SET TARGET GRIDNO
        pSoldier.sTargetGridNo = sGridNo;
        pSoldier.bTargetLevel = pSoldier.bLevel;
        pSoldier.sLastTarget = sGridNo;
        pSoldier.ubTargetID = WorldManager.WhoIsThere2(sGridNo, pSoldier.bTargetLevel);
    }


    void EVENT_SoldierBeginKnifeThrowAttack(SOLDIERTYPE? pSoldier, int sGridNo, WorldDirections ubDirection)
    {
        // Increment the number of people busy doing stuff because of an attack
        //if ( (gTacticalStatus.uiFlags & TURNBASED) && (gTacticalStatus.uiFlags & INCOMBAT) )
        //{
        gTacticalStatus.ubAttackBusyCount++;
        //}
        pSoldier.bBulletsLeft = 1;
        //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, string.Format("!!!!!!! Starting knifethrow attack, bullets left %d", pSoldier.bBulletsLeft));

        EVENT_InitNewSoldierAnim(pSoldier, THROW_KNIFE, 0, false);

        // CHANGE DIRECTION AND GOTO ANIMATION NOW
        EVENT_SetSoldierDesiredDirection(pSoldier, ubDirection);
        EVENT_SetSoldierDirection(pSoldier, ubDirection);


        // SET TARGET GRIDNO
        pSoldier.sTargetGridNo = sGridNo;
        pSoldier.sLastTarget = sGridNo;
        pSoldier.fTurningFromPronePosition = false;
        // NB target level must be set by functions outside of here... but I think it
        // is already set in HandleItem or in the AI code - CJC
        pSoldier.ubTargetID = WorldManager.WhoIsThere2(sGridNo, pSoldier.bTargetLevel);
    }


    void EVENT_SoldierBeginDropBomb(SOLDIERTYPE? pSoldier)
    {
        // Increment the number of people busy doing stuff because of an attack
        switch (gAnimControl[pSoldier.usAnimState].ubHeight)
        {
            case AnimationHeights.ANIM_STAND:

                EVENT_InitNewSoldierAnim(pSoldier, PLANT_BOMB, 0, false);
                break;

            default:

                // Call hander for planting bomb...
                HandleSoldierDropBomb(pSoldier, pSoldier.sPendingActionData2);
                SoldierGotoStationaryStance(pSoldier);
                break;
        }

    }


    void EVENT_SoldierBeginUseDetonator(SOLDIERTYPE? pSoldier)
    {
        // Increment the number of people busy doing stuff because of an attack
        switch (gAnimControl[pSoldier.usAnimState].ubHeight)
        {
            case AnimationHeights.ANIM_STAND:

                EVENT_InitNewSoldierAnim(pSoldier, USE_REMOTE, 0, false);
                break;

            default:

                // Call hander for planting bomb...
                HandleSoldierUseRemote(pSoldier, pSoldier.sPendingActionData2);
                break;
        }
    }

    void EVENT_SoldierBeginFirstAid(SOLDIERTYPE? pSoldier, int sGridNo, int ubDirection)
    {
        SOLDIERTYPE? pTSoldier;
        //int uiMercFlags;
        int usSoldierIndex;
        bool fRefused = false;

        usSoldierIndex = WorldManager.WhoIsThere2(sGridNo, pSoldier.bLevel);
        if (usSoldierIndex != NOBODY)
        {
            pTSoldier = MercPtrs[usSoldierIndex];

            // OK, check if we should play quote...
            if (pTSoldier.bTeam != gbPlayerNum)
            {
                if (pTSoldier.ubProfile != NO_PROFILE && pTSoldier.ubProfile >= FIRST_RPC && !RPC_RECRUITED(pTSoldier))
                {
                    fRefused = PCDoesFirstAidOnNPC(pTSoldier.ubProfile);
                }

                if (!fRefused)
                {
                    if (CREATURE_OR_BLOODCAT(pTSoldier))
                    {
                        // nope!!
                        fRefused = true;
                        Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG_UI_FEEDBACK, Message[STR_REFUSE_FIRSTAID_FOR_CREATURE]);
                    }
                    else if (!pTSoldier.bNeutral && pTSoldier.bLife >= OKLIFE && pTSoldier.bSide != pSoldier.bSide)
                    {
                        fRefused = true;
                        Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG_UI_FEEDBACK, Message[STR_REFUSE_FIRSTAID]);
                    }

                }
            }

            if (fRefused)
            {
                UnSetUIBusy(pSoldier.ubID);
                return;
            }

            // ATE: We can only give firsty aid to one perosn at a time... cancel
            // any now...
            InternalGivingSoldierCancelServices(pSoldier, false);

            // CHANGE DIRECTION AND GOTO ANIMATION NOW
            EVENT_SetSoldierDesiredDirection(pSoldier, ubDirection);
            EVENT_SetSoldierDirection(pSoldier, ubDirection);

            // CHECK OUR STANCE AND GOTO CROUCH IF NEEDED
            //if ( gAnimControl[ pSoldier.usAnimState ].ubEndHeight != ANIM_CROUCH )
            //{
            // SET DESIRED STANCE AND SET PENDING ANIMATION
            //	SendChangeSoldierStanceEvent( pSoldier, ANIM_CROUCH );
            //	pSoldier.usPendingAnimation = START_AID;
            //}
            //else
            {
                // CHANGE TO ANIMATION
                EVENT_InitNewSoldierAnim(pSoldier, START_AID, 0, false);
            }

            // SET TARGET GRIDNO
            pSoldier.sTargetGridNo = sGridNo;

            // SET PARTNER ID
            pSoldier.ubServicePartner = (int)usSoldierIndex;

            // SET PARTNER'S COUNT REFERENCE
            pTSoldier.ubServiceCount++;

            // If target and doer are no the same guy...
            if (pTSoldier.ubID != pSoldier.ubID && !pTSoldier.bCollapsed)
            {
                SoldierGotoStationaryStance(pTSoldier);
            }
        }
    }


    void EVENT_SoldierEnterVehicle(SOLDIERTYPE? pSoldier, int sGridNo, int ubDirection)
    {
        SOLDIERTYPE? pTSoldier;
        int uiMercFlags;
        int usSoldierIndex;

        if (FindSoldier(sGridNo, &usSoldierIndex, &uiMercFlags, FIND_SOLDIER_GRIDNO))
        {
            pTSoldier = MercPtrs[usSoldierIndex];

            // Enter vehicle...
            EnterVehicle(pTSoldier, pSoldier);
        }

        UnSetUIBusy(pSoldier.ubID);
    }


    int SoldierDressWound(SOLDIERTYPE? pSoldier, SOLDIERTYPE? pVictim, int sKitPts, int sStatus)
    {
        int uiDressSkill, uiPossible, uiActual, uiMedcost, uiDeficiency, uiAvailAPs, uiUsedAPs;
        int ubBelowOKlife, ubPtsLeft;
        bool fRanOut = false;

        if (pVictim.bBleeding < 1 && pVictim.bLife >= OKLIFE)
        {
            return (0);     // nothing to do, shouldn't have even been called!
        }

        if (pVictim.bLife == 0)
        {
            return (0);
        }

        // in case he has multiple kits in hand, limit influence of kit status to 100%!
        if (sStatus >= 100)
        {
            sStatus = 100;
        }

        // calculate wound-dressing skill (3x medical, 2x equip, 1x level, 1x dex)
        uiDressSkill = ((3 * SkillChecks.EffectiveMedical(pSoldier)) +                  // medical knowledge
                                           (2 * sStatus) +                                                              // state of medical kit
                                           (10 * SkillChecks.EffectiveExpLevel(pSoldier)) +                 // battle injury experience
                                                       SkillChecks.EffectiveDexterity(pSoldier)) / 7;       // general "handiness"

        // try to use every AP that the merc has left
        uiAvailAPs = pSoldier.bActionPoints;

        // OK, If we are in real-time, use another value...
        if (!(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED)) || !(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
        {
            // Set to a value which looks good based on our tactical turns duration
            uiAvailAPs = RT_FIRST_AID_GAIN_MODIFIER;
        }

        // calculate how much bandaging CAN be done this turn
        uiPossible = (uiAvailAPs * uiDressSkill) / 50;  // max rate is 2 * fullAPs

        // if no healing is possible (insufficient APs or insufficient dressSkill)
        if (uiPossible == 0)
        {
            return (0);
        }

        if (pSoldier.inv[InventorySlot.HANDPOS].usItem == MEDICKIT)      // using the GOOD medic stuff
        {
            uiPossible += (uiPossible / 2);         // add extra 50 %
        }

        uiActual = uiPossible;      // start by assuming maximum possible


        // figure out how far below OKLIFE the victim is
        if (pVictim.bLife >= OKLIFE)
        {
            ubBelowOKlife = 0;
        }
        else
        {
            ubBelowOKlife = OKLIFE - pVictim.bLife;
        }

        // figure out how many healing pts we need to stop dying (2x cost)
        uiDeficiency = (2 * ubBelowOKlife);

        // if, after that, the patient will still be bleeding
        if ((pVictim.bBleeding - ubBelowOKlife) > 0)
        {
            // then add how many healing pts we need to stop bleeding (1x cost)
            uiDeficiency += (pVictim.bBleeding - ubBelowOKlife);
        }

        // now, make sure we weren't going to give too much
        if (uiActual > uiDeficiency)    // if we were about to apply too much
        {
            uiActual = uiDeficiency;    // reduce actual not to waste anything
        }


        // now make sure we HAVE that much
        if (pSoldier.inv[InventorySlot.HANDPOS].usItem == MEDICKIT)
        {
            uiMedcost = (uiActual + 1) / 2;     // cost is only half, rounded up

            if (uiMedcost > (int)sKitPts)            // if we can't afford this
            {
                fRanOut = true;
                uiMedcost = sKitPts;        // what CAN we afford?
                uiActual = uiMedcost * 2;       // give double this as aid
            }
        }
        else
        {
            uiMedcost = uiActual;

            if (uiMedcost > (int)sKitPts)        // can't afford it
            {
                fRanOut = true;
                uiMedcost = uiActual = sKitPts;     // recalc cost AND aid
            }
        }

        ubPtsLeft = (int)uiActual;


        // heal real life points first (if below OKLIFE) because we don't want the
        // patient still DYING if bandages run out, or medic is disabled/distracted!
        // NOTE: Dressing wounds for life below OKLIFE now costs 2 pts/life point!
        if (ubPtsLeft && pVictim.bLife < OKLIFE)
        {
            // if we have enough points to bring him all the way to OKLIFE this turn
            if (ubPtsLeft >= (2 * ubBelowOKlife))
            {
                // raise life to OKLIFE
                pVictim.bLife = OKLIFE;

                // reduce bleeding by the same number of life points healed up
                pVictim.bBleeding -= ubBelowOKlife;

                // use up appropriate # of actual healing points
                ubPtsLeft -= (2 * ubBelowOKlife);
            }
            else
            {
                pVictim.bLife += (ubPtsLeft / 2);
                pVictim.bBleeding -= (ubPtsLeft / 2);

                ubPtsLeft = ubPtsLeft % 2;  // if ptsLeft was odd, ptsLeft = 1
            }

            // this should never happen any more, but make sure bleeding not negative
            if (pVictim.bBleeding < 0)
            {
                pVictim.bBleeding = 0;
            }

            // if this healing brought the patient out of the worst of it, cancel dying
            if (pVictim.bLife >= OKLIFE)
            {
                //pVictim.dying = pVictim.dyingComment = false;
                //pVictim.shootOn = true;

                // turn off merc QUOTE flags
                pVictim.fDyingComment = false;

            }

            // update patient's entire panel (could have regained consciousness, etc.)
        }


        // if any healing points remain, apply that to any remaining bleeding (1/1)
        // DON'T spend any APs/kit pts to cure bleeding until merc is no longer dying
        //if ( ubPtsLeft && pVictim.bBleeding && !pVictim.dying)
        if (ubPtsLeft && pVictim.bBleeding)
        {
            // if we have enough points to bandage all remaining bleeding this turn
            if (ubPtsLeft >= pVictim.bBleeding)
            {
                ubPtsLeft -= pVictim.bBleeding;
                pVictim.bBleeding = 0;
            }
            else        // bandage what we can
            {
                pVictim.bBleeding -= ubPtsLeft;
                ubPtsLeft = 0;
            }

            // update patient's life bar only
        }


        // if wound has been dressed enough so that bleeding won't occur, turn off
        // the "warned about bleeding" flag so merc tells us about the next bleeding
        if (pVictim.bBleeding <= MIN_BLEEDING_THRESHOLD)
        {
            pVictim.fWarnedAboutBleeding = false;
        }


        // if there are any ptsLeft now, then we didn't actually get to use them
        uiActual -= ubPtsLeft;

        // usedAPs equals (actionPts) * (%of possible points actually used)
        uiUsedAPs = (uiActual * uiAvailAPs) / uiPossible;

        if (pSoldier.inv[InventorySlot.HANDPOS].usItem == Items.MEDICKIT)  // using the GOOD medic stuff
        {
            uiUsedAPs = (uiUsedAPs * 2) / 3;    // reverse 50% bonus by taking 2/3rds
        }

        DeductPoints(pSoldier, (int)uiUsedAPs, (int)((uiUsedAPs * BP.PER_AP_LT_EFFORT)));


        if (PTR_OURTEAM(pSoldier))
        {
            // MEDICAL GAIN   (actual / 2):  Helped someone by giving first aid
            Campaign.StatChange(pSoldier, Stat.MEDICALAMT, (int)(uiActual / 2), 0);

            // DEXTERITY GAIN (actual / 6):  Helped someone by giving first aid
            Campaign.StatChange(pSoldier, Stat.DEXTAMT, (int)(uiActual / 6), 0);
        }

        return (uiMedcost);
    }


    private static void InternalReceivingSoldierCancelServices(SOLDIERTYPE? pSoldier, bool fPlayEndAnim)
    {
        SOLDIERTYPE? pTSoldier;
        int cnt;

        if (pSoldier.ubServiceCount > 0)
        {
            // Loop through guys who have us as servicing
            for (pTSoldier = Menptr, cnt = 0; cnt < MAX_NUM_SOLDIERS; pTSoldier++, cnt++)
            {
                if (pTSoldier.bActive)
                {
                    if (pTSoldier.ubServicePartner == pSoldier.ubID)
                    {
                        // END SERVICE!			
                        pSoldier.ubServiceCount--;

                        pTSoldier.ubServicePartner = NOBODY;

                        if (gTacticalStatus.fAutoBandageMode)
                        {
                            pSoldier.ubAutoBandagingMedic = NOBODY;

                            ActionDone(pTSoldier);
                        }
                        else
                        {
                            // don't use end aid animation in autobandage
                            if (pTSoldier.bLife >= OKLIFE && pTSoldier.bBreath > 0 && fPlayEndAnim)
                            {
                                EVENT_InitNewSoldierAnim(pTSoldier, AnimationStates.END_AID, 0, false);
                            }
                        }


                    }
                }
            }

        }

    }


    public static void ReceivingSoldierCancelServices(SOLDIERTYPE? pSoldier)
    {
        InternalReceivingSoldierCancelServices(pSoldier, true);
    }


    private static void InternalGivingSoldierCancelServices(SOLDIERTYPE? pSoldier, bool fPlayEndAnim)
    {
        SOLDIERTYPE? pTSoldier;

        // GET TARGET SOLDIER
        if (pSoldier.ubServicePartner != NOBODY)
        {
            pTSoldier = MercPtrs[pSoldier.ubServicePartner];

            // END SERVICE!
            pTSoldier.ubServiceCount--;

            pSoldier.ubServicePartner = NOBODY;

            if (gTacticalStatus.fAutoBandageMode)
            {
                pTSoldier.ubAutoBandagingMedic = NOBODY;

                ActionDone(pSoldier);
            }
            else
            {
                if (pSoldier.bLife >= OKLIFE && pSoldier.bBreath > 0 && fPlayEndAnim)
                {
                    // don't use end aid animation in autobandage
                    EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.END_AID, 0, false);
                }
            }
        }

    }

    public static void GivingSoldierCancelServices(SOLDIERTYPE? pSoldier)
    {
        InternalGivingSoldierCancelServices(pSoldier, true);
    }


    void HaultSoldierFromSighting(SOLDIERTYPE? pSoldier, bool fFromSightingEnemy)
    {
        // SEND HUALT EVENT!
        //EV_S_STOP_MERC				SStopMerc;

        //SStopMerc.sGridNo					= pSoldier.sGridNo;
        //SStopMerc.bDirection			= pSoldier.bDirection;
        //SStopMerc.usSoldierID			= pSoldier.ubID;
        //AddGameEvent( S_STOP_MERC, 0, &SStopMerc );

        // If we are a 'specialmove... ignore...
        if ((gAnimControl[pSoldier.usAnimState].uiFlags & ANIM_SPECIALMOVE))
        {
            return;
        }

        // OK, check if we were going to throw something, and give it back if so!
        if (pSoldier.pTempObject != null && fFromSightingEnemy)
        {
            // Place it back into inv....
            AutoPlaceObject(pSoldier, pSoldier.pTempObject, false);
            MemFree(pSoldier.pTempObject);
            pSoldier.pTempObject = null;
            pSoldier.usPendingAnimation = NO_PENDING_ANIMATION;
            pSoldier.usPendingAnimation2 = NO_PENDING_ANIMATION;

            // Decrement attack counter...
            //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("@@@@@@@ Reducing attacker busy count..., ending throw because saw something"));
            ReduceAttackBusyCount(pSoldier.ubID, false);

            // ATE: Goto stationary stance......
            SoldierGotoStationaryStance(pSoldier);

            Interface.DirtyMercPanelInterface(pSoldier, DIRTYLEVEL2);
        }

        if (!(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
        {
            EVENT_StopMerc(pSoldier, pSoldier.sGridNo, pSoldier.bDirection);
        }
        else
        {
            // Pause this guy from no APS
            AdjustNoAPToFinishMove(pSoldier, true);

            pSoldier.ubReasonCantFinishMove = REASON_STOPPED.SIGHT;

            // ATE; IF turning to shoot, stop!
            // ATE: We want to do this only for enemies, not items....
            if (pSoldier.fTurningToShoot && fFromSightingEnemy)
            {
                pSoldier.fTurningToShoot = false;
                // Release attacker			

                // OK - this is hightly annoying , but due to the huge combinations of
                // things that can happen - 1 of them is that sLastTarget will get unset
                // after turn is done - so set flag here to tell it not to...
                pSoldier.fDontUnsetLastTargetFromTurn = true;

                //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("@@@@@@@ Reducing attacker busy count..., ending fire because saw something"));
                ReduceAttackBusyCount(pSoldier.ubID, false);
            }

            // OK, if we are stopped at our destination, cancel pending action...
            if (fFromSightingEnemy)
            {
                if (pSoldier.ubPendingAction != NO_PENDING_ACTION && pSoldier.sGridNo == pSoldier.sFinalDestination)
                {
                    pSoldier.ubPendingAction = NO_PENDING_ACTION;
                }

                // Stop pending animation....
                pSoldier.usPendingAnimation = NO_PENDING_ANIMATION;
                pSoldier.usPendingAnimation2 = NO_PENDING_ANIMATION;
            }

            if (!pSoldier.fTurningToShoot)
            {
                pSoldier.fTurningFromPronePosition = false;
            }
        }

        // Unset UI!
        if (fFromSightingEnemy || (pSoldier.pTempObject == null && !pSoldier.fTurningToShoot))
        {
            UnSetUIBusy(pSoldier.ubID);
        }

        pSoldier.bTurningFromUI = false;

        UnSetEngagedInConvFromPCAction(pSoldier);
    }


    void ReLoadSoldierAnimationDueToHandItemChange(SOLDIERTYPE? pSoldier, Items usOldItem, Items usNewItem)
    {
        // DON'T continue aiming!
        // GOTO STANCE
        // CHECK FOR AIMING ANIMATIONS
        bool fOldRifle = false;
        bool fNewRifle = false;

        // Shutoff burst....
        // ( we could be on, then change gun that does not have burst )
        if (Weapon[usNewItem].ubShotsPerBurst == 0)
        {
            pSoldier.bDoBurst = false;
            pSoldier.bWeaponMode = WM.NORMAL;
        }

        if (gAnimControl[pSoldier.usAnimState].uiFlags.HasFlag(ANIM.FIREREADY))
        {
            // Stop aiming!
            SoldierGotoStationaryStance(pSoldier);
        }

        // Cancel services...
        GivingSoldierCancelServices(pSoldier);

        // Did we have a rifle and do we now not have one?
        if (usOldItem != NOTHING)
        {
            if (Item[usOldItem].usItemClass == IC.GUN)
            {
                if ((Item[usOldItem].fFlags.HasFlag(ItemAttributes.ITEM_TWO_HANDED)) && usOldItem != Items.ROCKET_LAUNCHER)
                {
                    fOldRifle = true;
                }
            }
        }

        if (usNewItem != NOTHING)
        {
            if (Item[usNewItem].usItemClass == IC.GUN)
            {
                if ((Item[usNewItem].fFlags.HasFlag(ItemAttributes.ITEM_TWO_HANDED)) && usNewItem != Items.ROCKET_LAUNCHER)
                {
                    fNewRifle = true;
                }
            }
        }

        // Switch on stance!
        switch (gAnimControl[pSoldier.usAnimState].ubEndHeight)
        {
            case AnimationHeights.ANIM_STAND:

                if (fOldRifle && !fNewRifle)
                {
                    // Put it away!
                    EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.LOWER_RIFLE, 0, false);
                }
                else if (!fOldRifle && fNewRifle)
                {
                    // Bring it up!
                    EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.RAISE_RIFLE, 0, false);
                }
                else
                {
                    SetSoldierAnimationSurface(pSoldier, pSoldier.usAnimState);
                }
                break;

            case AnimationHeights.ANIM_CROUCH:
            case AnimationHeights.ANIM_PRONE:

                SetSoldierAnimationSurface(pSoldier, pSoldier.usAnimState);
                break;
        }

    }



    int CreateEnemyGlow16BPPPalette(List<SGPPaletteEntry> pPalette, int rscale, int gscale, bool fAdjustGreen)
    {
        int? r16, g16, b16, usColor;
        int[] p16BPPPalette = new int[sizeof(int) * 256];
        int cnt;
        int rmod, gmod, bmod;
        int r, g, b;

        Debug.Assert(pPalette != null);

        for (cnt = 0; cnt < 256; cnt++)
        {
            gmod = (pPalette[cnt].peGreen);
            bmod = (pPalette[cnt].peBlue);

            rmod = Math.Max(rscale, (pPalette[cnt].peRed));

            if (fAdjustGreen)
            {
                gmod = Math.Max(gscale, (pPalette[cnt].peGreen));
            }

            r = (int)Math.Min(rmod, 255);
            g = (int)Math.Min(gmod, 255);
            b = (int)Math.Min(bmod, 255);

            if (gusRedShift < 0)
            {
                r16 = ((int)r >> (-gusRedShift));
            }
            else
            {
                r16 = ((int)r << gusRedShift);
            }

            if (gusGreenShift < 0)
            {
                g16 = ((int)g >> (-gusGreenShift));
            }
            else
            {
                g16 = ((int)g << gusGreenShift);
            }

            if (gusBlueShift < 0)
            {
                b16 = ((int)b >> (-gusBlueShift));
            }
            else
            {
                b16 = ((int)b << gusBlueShift);
            }

            // Prevent creation of pure black color
            usColor = (r16 & gusRedMask) | (g16 & gusGreenMask) | (b16 & gusBlueMask);

            if ((usColor == 0) && ((r + g + b) != 0))
            {
                usColor = 0x0001;
            }

            p16BPPPalette[cnt] = usColor;
        }
        return (p16BPPPalette);
    }


    int? CreateEnemyGreyGlow16BPPPalette(List<SGPPaletteEntry> pPalette, int rscale, int gscale, bool fAdjustGreen)
    {
        int? p16BPPPalette, r16, g16, b16, usColor;
        int cnt, lumin;
        int rmod, gmod, bmod;
        int r, g, b;

        Debug.Assert(pPalette != null);

        //p16BPPPalette = MemAlloc(sizeof(int) * 256);

        for (cnt = 0; cnt < 256; cnt++)
        {
            lumin = (pPalette[cnt].peRed * 299 / 1000) + (pPalette[cnt].peGreen * 587 / 1000) + (pPalette[cnt].peBlue * 114 / 1000);
            rmod = (100 * lumin) / 256;
            gmod = (100 * lumin) / 256;
            bmod = (100 * lumin) / 256;



            rmod = Math.Max(rscale, rmod);

            if (fAdjustGreen)
            {
                gmod = Math.Max(gscale, gmod);
            }


            r = (int)Math.Min(rmod, 255);
            g = (int)Math.Min(gmod, 255);
            b = (int)Math.Min(bmod, 255);

            if (gusRedShift < 0)
            {
                r16 = ((int)r >> (-gusRedShift));
            }
            else
            {
                r16 = ((int)r << gusRedShift);
            }

            if (gusGreenShift < 0)
            {
                g16 = ((int)g >> (-gusGreenShift));
            }
            else
            {
                g16 = ((int)g << gusGreenShift);
            }

            if (gusBlueShift < 0)
            {
                b16 = ((int)b >> (-gusBlueShift));
            }
            else
            {
                b16 = ((int)b << gusBlueShift);
            }

            // Prevent creation of pure black color
            usColor = (r16 & gusRedMask) | (g16 & gusGreenMask) | (b16 & gusBlueMask);

            if ((usColor == 0) && ((r + g + b) != 0))
            {
                usColor = 0x0001;
            }

            p16BPPPalette[cnt] = usColor;
        }

        return (p16BPPPalette);
    }


    void ContinueMercMovement(SOLDIERTYPE? pSoldier)
    {
        int sAPCost;
        int sGridNo;

        sGridNo = pSoldier.sFinalDestination;

        // Can we afford this?
        if (pSoldier.bGoodContPath)
        {
            sGridNo = pSoldier.sContPathLocation;
        }
        else
        {
            // ATE: OK, don't cancel count, so pending actions are still valid...
            pSoldier.ubPendingActionAnimCount = 0;
        }

        // get a path to dest...
        if (FindBestPath(pSoldier, sGridNo, pSoldier.bLevel, pSoldier.usUIMovementMode, NO_COPYROUTE, 0))
        {
            sAPCost = PtsToMoveDirection(pSoldier, (int)guiPathingData[0]);

            if (EnoughPoints(pSoldier, sAPCost, 0, (bool)(pSoldier.bTeam == gbPlayerNum)))
            {
                // Acknowledge
                if (pSoldier.bTeam == gbPlayerNum)
                {
                    DoMercBattleSound(pSoldier, BATTLE_SOUND_OK1);

                    // If we have a face, tell text in it to go away!
                    if (pSoldier.iFaceIndex != -1)
                    {
                        gFacesData[pSoldier.iFaceIndex].fDisplayTextOver = FACE_ERASE_TEXT_OVER;
                    }
                }

                AdjustNoAPToFinishMove(pSoldier, false);

                SetUIBusy(pSoldier.ubID);

                // OK, try and get a path to out dest!
                EVENT_InternalGetNewSoldierPath(pSoldier, sGridNo, pSoldier.usUIMovementMode, false, true);
            }
        }
    }


    private static bool CheckForBreathCollapse(SOLDIERTYPE? pSoldier)
    {
        // Check if we are out of breath!
        // Only check if > 70
        if (pSoldier.bBreathMax > 70)
        {
            if (pSoldier.bBreath < 20 && !(pSoldier.usQuoteSaidFlags.HasFlag(SOLDIER_QUOTE.SAID_LOW_BREATH)) &&
                    gAnimControl[pSoldier.usAnimState].ubEndHeight == AnimationHeights.ANIM_STAND)
            {
                // WARN!
                TacticalCharacterDialogue(pSoldier, QUOTE.OUT_OF_BREATH);

                // Set flag indicating we were warned!
                pSoldier.usQuoteSaidFlags |= SOLDIER_QUOTE.SAID_LOW_BREATH;
            }
        }

        // Check for drowing.....
        //if ( pSoldier.bBreath < 10 && !(pSoldier.usQuoteSaidFlags.HasFlag(SOLDIER.QUOTE.SAID_DROWNING )) && pSoldier.bOverTerrainType == DEEP_WATER )
        //{
        // WARN!
        //	TacticalCharacterDialogue( pSoldier, QUOTE_DROWNING );			

        // Set flag indicating we were warned!
        //	pSoldier.usQuoteSaidFlags |= SOLDIER_QUOTE.SAID_DROWNING;

        // WISDOM GAIN (25):  Starting to drown
        //  StatChange( pSoldier, WISDOMAMT, 25, false );

        //}

        if (pSoldier.bBreath == 0 && !pSoldier.bCollapsed && !(pSoldier.uiStatusFlags & (SOLDIER.VEHICLE | SOLDIER.ANIMAL | SOLDIER.MONSTER)))
        {
            // Collapse!
            // OK, Set a flag, because we may still be in the middle of an animation what is not interruptable...
            pSoldier.bBreathCollapsed = true;

            return (true);
        }

        return (false);
    }

    public static bool InternalIsValidStance(SOLDIERTYPE? pSoldier, WorldDirections bDirection, AnimationHeights bNewStance)
    {
        int usOKToAddStructID = 0;
        STRUCTURE_FILE_REF? pStructureFileRef;
        AnimationSurfaceTypes usAnimSurface = 0;
        AnimationStates usAnimState;

        // Check, if dest is prone, we can actually do this!

        // If we are a vehicle, we can only 'stand'
        if ((pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE))
            && bNewStance != AnimationHeights.ANIM_STAND)
        {
            return (false);
        }

        // Check if we are in water?
        if (MercInWater(pSoldier))
        {
            if (bNewStance == AnimationHeights.ANIM_PRONE || bNewStance == AnimationHeights.ANIM_CROUCH)
            {
                return (false);
            }
        }

        if (pSoldier.ubBodyType == SoldierBodyTypes.ROBOTNOWEAPON
            && bNewStance != AnimationHeights.ANIM_STAND)
        {
            return (false);
        }

        // Check if we are in water?
        if (AM_AN_EPC(pSoldier))
        {
            if (bNewStance == AnimationHeights.ANIM_PRONE)
            {
                return (false);
            }
            else
            {
                return (true);
            }
        }


        if (pSoldier.bCollapsed)
        {
            if (bNewStance == AnimationHeights.ANIM_STAND
                || bNewStance == AnimationHeights.ANIM_CROUCH)
            {
                return (false);
            }
        }

        // Check if we can do this....
        if (pSoldier.pLevelNode is not null && pSoldier.pLevelNode.pStructureData is not null)
        {
            usOKToAddStructID = pSoldier.pLevelNode.pStructureData.usStructureID;
        }
        else
        {
            usOKToAddStructID = INVALID_STRUCTURE_ID;
        }

        switch (bNewStance)
        {
            case AnimationHeights.ANIM_STAND:

                usAnimState = AnimationStates.STANDING;
                break;

            case AnimationHeights.ANIM_CROUCH:

                usAnimState = AnimationStates.CROUCHING;
                break;


            case AnimationHeights.ANIM_PRONE:

                usAnimState = AnimationStates.PRONE;
                break;

            default:

                // Something gone funny here....
                usAnimState = pSoldier.usAnimState;
                Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG_BETAVERSION, "Wrong desired stance given: %d, %d.", bNewStance, pSoldier.usAnimState);
                break;
        }

        usAnimSurface = AnimationControl.DetermineSoldierAnimationSurface(pSoldier, usAnimState);

        // Get structure ref........
        pStructureFileRef = GetAnimationStructureRef(pSoldier.ubID, usAnimSurface, usAnimState);

        if (pStructureFileRef != null)
        {
            // Can we add structure data for this stance...?
            if (!WorldStructures.OkayToAddStructureToWorld(pSoldier.sGridNo, pSoldier.bLevel, (pStructureFileRef.pDBStructureRef[(int)gOneCDirection[bDirection]]), usOKToAddStructID))
            {
                return (false);
            }
        }

        return (true);
    }


    public static bool IsValidStance(SOLDIERTYPE? pSoldier, AnimationHeights bNewStance)
    {
        return (InternalIsValidStance(pSoldier, pSoldier.bDirection, bNewStance));
    }


    bool IsValidMovementMode(SOLDIERTYPE? pSoldier, int usMovementMode)
    {
        // Check, if dest is prone, we can actually do this!

        // Check if we are in water?
        if (MercInWater(pSoldier))
        {
            if (usMovementMode == RUNNING || usMovementMode == SWATTING || usMovementMode == CRAWLING)
            {
                return (false);
            }
        }

        return (true);
    }


    void SelectMoveAnimationFromStance(SOLDIERTYPE? pSoldier)
    {
        // Determine which animation to do...depending on stance and gun in hand...
        switch (gAnimControl[pSoldier.usAnimState].ubEndHeight)
        {
            case AnimationHeights.ANIM_STAND:
                EVENT_InitNewSoldierAnim(pSoldier, WALKING, 0, false);
                break;

            case AnimationHeights.ANIM_PRONE:
                EVENT_InitNewSoldierAnim(pSoldier, CRAWLING, 0, false);
                break;

            case AnimationHeights.ANIM_CROUCH:
                EVENT_InitNewSoldierAnim(pSoldier, SWATTING, 0, false);
                break;

        }

    }


    public static void GetActualSoldierAnimDims(SOLDIERTYPE? pSoldier, out int psHeight, out int psWidth)
    {
        int usAnimSurface;
        ETRLEObject? pTrav;

        usAnimSurface = GetSoldierAnimationSurface(pSoldier, pSoldier.usAnimState);

        if (usAnimSurface == INVALID_ANIMATION_SURFACE)
        {
            psHeight = (int)5;
            psWidth = (int)5;

            return;
        }

        if (gAnimSurfaceDatabase[usAnimSurface].hVideoObject == null)
        {
            psHeight = (int)5;
            psWidth = (int)5;
            return;
        }

        // OK, noodle here on what we should do... If we take each frame, it will be different slightly
        // depending on the frame and the value returned here will vary thusly. However, for the
        // uses of this function, we should be able to use just the first frame...

        if (pSoldier.usAniFrame >= gAnimSurfaceDatabase[usAnimSurface].hVideoObject.usNumberOfObjects)
        {
            int i = 0;
        }

        pTrav = (gAnimSurfaceDatabase[usAnimSurface].hVideoObject.pETRLEObject[pSoldier.usAniFrame]);

        psHeight = (int)pTrav.usHeight;
        psWidth = (int)pTrav.usWidth;
    }

    public static void GetActualSoldierAnimOffsets(SOLDIERTYPE? pSoldier, out int sOffsetX, out int sOffsetY)
    {
        int usAnimSurface;
        ETRLEObject? pTrav;

        usAnimSurface = GetSoldierAnimationSurface(pSoldier, pSoldier.usAnimState);

        if (usAnimSurface == INVALID_ANIMATION_SURFACE)
        {
            sOffsetX = (int)0;
            sOffsetY = (int)0;

            return;
        }

        if (gAnimSurfaceDatabase[usAnimSurface].hVideoObject == null)
        {
            sOffsetX = (int)0;
            sOffsetY = (int)0;
            return;
        }

        pTrav = (gAnimSurfaceDatabase[usAnimSurface].hVideoObject.pETRLEObject[pSoldier.usAniFrame]);

        sOffsetX = (int)pTrav.sOffsetX;
        sOffsetY = (int)pTrav.sOffsetY;
    }


    public static void SetSoldierLocatorOffsets(SOLDIERTYPE? pSoldier)
    {
        int sHeight, sWidth;
        int sOffsetX, sOffsetY;


        // OK, from our animation, get height, width
        GetActualSoldierAnimDims(pSoldier, out sHeight, out sWidth);
        GetActualSoldierAnimOffsets(pSoldier, out sOffsetX, out sOffsetY);

        // OK, here, use the difference between center of animation ( sWidth/2 ) and our offset!
        //pSoldier.sLocatorOffX = ( abs( sOffsetX ) ) - ( sWidth / 2 );

        pSoldier.sBoundingBoxWidth = sWidth;
        pSoldier.sBoundingBoxHeight = sHeight;
        pSoldier.sBoundingBoxOffsetX = sOffsetX;
        pSoldier.sBoundingBoxOffsetY = sOffsetY;

    }

    bool SoldierCarriesTwoHandedWeapon(SOLDIERTYPE? pSoldier)
    {
        Items usItem;

        usItem = pSoldier.inv[InventorySlot.HANDPOS].usItem;

        if (usItem != NOTHING && (Item[usItem].fFlags.HasFlag(ItemAttributes.ITEM_TWO_HANDED)))
        {
            return (true);
        }

        return (false);

    }



    int CheckBleeding(SOLDIERTYPE? pSoldier)
    {
        int bBandaged; //,savedOurTurn;
        int iBlood = NOBLOOD;

        if (pSoldier.bLife != 0)
        {
            // if merc is hurt beyond the minimum required to bleed, or he's dying
            if ((pSoldier.bBleeding > MIN_BLEEDING_THRESHOLD) || pSoldier.bLife < OKLIFE)
            {
                // if he's NOT in the process of being bandaged or DOCTORed
                if ((pSoldier.ubServiceCount == 0) && (AnyDoctorWhoCanHealThisPatient(pSoldier, HEALABLE_EVER) == null))
                {
                    // may drop blood whether or not any bleeding takes place this turn
                    if (pSoldier.bTilesMoved < 1)
                    {
                        iBlood = ((pSoldier.bBleeding - MIN_BLEEDING_THRESHOLD) / BLOODDIVISOR); // + pSoldier.dying;
                        if (iBlood > MAXBLOODQUANTITY)
                        {
                            iBlood = MAXBLOODQUANTITY;
                        }
                    }
                    else
                    {
                        iBlood = NOBLOOD;
                    }

                    // Are we in a different mode?
                    if (!(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED))
                        || !(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
                    {
                        pSoldier.dNextBleed -= (float)RT_NEXT_BLEED_MODIFIER;
                    }
                    else
                    {
                        // Do a single step descrease
                        pSoldier.dNextBleed--;
                    }

                    // if it's time to lose some blood
                    if (pSoldier.dNextBleed <= 0)
                    {
                        // first, calculate if soldier is bandaged
                        bBandaged = pSoldier.bLifeMax - pSoldier.bBleeding - pSoldier.bLife;

                        // as long as he's bandaged and not "dying"
                        if (bBandaged > 0 && pSoldier.bLife >= OKLIFE)
                        {
                            // just bleeding through existing bandages
                            pSoldier.bBleeding++;

                            SoldierBleed(pSoldier, true);
                        }
                        else    // soldier is either not bandaged at all or is dying
                        {
                            if (pSoldier.bLife < OKLIFE)       // if he's dying
                            {
                                // if he's conscious, and he hasn't already, say his "dying quote"
                                if ((pSoldier.bLife >= CONSCIOUSNESS) && !pSoldier.fDyingComment)
                                {
                                    TacticalCharacterDialogue(pSoldier, QUOTE.SERIOUSLY_WOUNDED);

                                    pSoldier.fDyingComment = true;
                                }

                                // can't permit lifemax to ever bleed beneath OKLIFE, or that
                                // soldier might as well be dead!
                                if (pSoldier.bLifeMax >= OKLIFE)
                                {
                                    // bleeding while "dying" costs a PERMANENT point of life each time!
                                    pSoldier.bLifeMax--;
                                    pSoldier.bBleeding--;
                                }
                            }
                        }

                        // either way, a point of life (health) is lost because of bleeding
                        // This will also update the life bar

                        SoldierBleed(pSoldier, false);


                        // if he's not dying (which includes him saying the dying quote just
                        // now), and he hasn't warned us that he's bleeding yet, he does so
                        // Also, not if they are being bandaged....
                        if ((pSoldier.bLife >= OKLIFE) && !pSoldier.fDyingComment && !pSoldier.fWarnedAboutBleeding && !gTacticalStatus.fAutoBandageMode && pSoldier.ubServiceCount == 0)
                        {
                            DialogControl.TacticalCharacterDialogue(pSoldier, QUOTE.STARTING_TO_BLEED);

                            // "starting to bleed" quote
                            pSoldier.fWarnedAboutBleeding = true;
                        }

                        pSoldier.dNextBleed = CalcSoldierNextBleed(pSoldier);

                    }
                }
            }
        }
        return (iBlood);
    }


    void SoldierBleed(SOLDIERTYPE? pSoldier, bool fBandagedBleed)
    {
        int bOldLife;

        // OK, here make some stuff happen for bleeding
        // A banaged bleed does not show damage taken , just through existing bandages	

        // ATE: Do this ONLY if buddy is in sector.....
        if ((pSoldier.bInSector && guiCurrentScreen == ScreenName.GAME_SCREEN) || guiCurrentScreen != ScreenName.GAME_SCREEN)
        {
            pSoldier.fFlashPortrait = FLASH_PORTRAIT.START;
            pSoldier.bFlashPortraitFrame = FLASH_PORTRAIT.STARTSHADE;
            RESETTIMECOUNTER(ref pSoldier.PortraitFlashCounter, (uint)FLASH_PORTRAIT.DELAY);

            // If we are in mapscreen, set this person as selected
            if (guiCurrentScreen == ScreenName.MAP_SCREEN)
            {
                SetInfoChar(pSoldier.ubID);
            }
        }

        bOldLife = pSoldier.bLife;

        // If we are already dead, don't show damage!
        if (!fBandagedBleed)
        {
            SoldierTakeDamage(pSoldier, AnimationHeights.ANIM_CROUCH, 1, 100, TAKE_DAMAGE.BLOODLOSS, NOBODY, NOWHERE, 0, true);
        }

    }


    public static void SoldierCollapse(SOLDIERTYPE? pSoldier)
    {
        bool fMerc = false;

        if (pSoldier.ubBodyType <= REGFEMALE)
        {
            fMerc = true;
        }

        // If we are an animal, etc, don't do anything....
        switch (pSoldier.ubBodyType)
        {
            case SoldierBodyTypes.ADULTFEMALEMONSTER:
            case SoldierBodyTypes.AM_MONSTER:
            case SoldierBodyTypes.YAF_MONSTER:
            case SoldierBodyTypes.YAM_MONSTER:
            case SoldierBodyTypes.LARVAE_MONSTER:
            case SoldierBodyTypes.INFANT_MONSTER:
            case SoldierBodyTypes.QUEENMONSTER:

                // Give breath back....
                DeductPoints(pSoldier, 0, (int)-5000);
                return;
                break;
        }

        pSoldier.bCollapsed = true;

        ReceivingSoldierCancelServices(pSoldier);

        // CC has requested - handle sight here...
        HandleSight(pSoldier, SIGHT.LOOK);

        // Check height
        switch (gAnimControl[pSoldier.usAnimState].ubEndHeight)
        {
            case AnimationHeights.ANIM_STAND:

                if (pSoldier.bOverTerrainType == TerrainTypeDefines.DEEP_WATER)
                {
                    EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.DEEP_WATER_DIE, 0, false);
                }
                else if (pSoldier.bOverTerrainType == TerrainTypeDefines.LOW_WATER)
                {
                    EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.WATER_DIE, 0, false);
                }
                else
                {
                    BeginTyingToFall(pSoldier);
                    EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.FALLFORWARD_FROMHIT_STAND, 0, false);
                }
                break;

            case AnimationHeights.ANIM_CROUCH:

                // Crouched or prone, only for mercs!
                BeginTyingToFall(pSoldier);

                if (fMerc)
                {
                    EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.FALLFORWARD_FROMHIT_CROUCH, 0, false);
                }
                else
                {
                    // For civs... use fall from stand...
                    EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.FALLFORWARD_FROMHIT_STAND, 0, false);
                }
                break;

            case ANIM_PRONE:

                switch (pSoldier.usAnimState)
                {
                    case AnimationStates.FALLFORWARD_FROMHIT_STAND:
                    case AnimationStates.ENDFALLFORWARD_FROMHIT_CROUCH:

                        ChangeSoldierState(pSoldier, AnimationStates.STAND_FALLFORWARD_STOP, 0, false);
                        break;

                    case AnimationStates.FALLBACK_HIT_STAND:
                        ChangeSoldierState(pSoldier, AnimationStates.FALLBACKHIT_STOP, 0, false);
                        break;

                    default:
                        EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.PRONE_LAY_FROMHIT, 0, false);
                        break;
                }
                break;
        }

        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.ENEMY))
        {

            if (!(gTacticalStatus.bPanicTriggerIsAlarm) && (gTacticalStatus.ubTheChosenOne == pSoldier.ubID))
            {
                // replace this guy as the chosen one!
                gTacticalStatus.ubTheChosenOne = NOBODY;
                MakeClosestEnemyChosenOne();
            }

            if ((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED))
                && (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
                && (pSoldier.uiStatusFlags.HasFlag(SOLDIER.UNDERAICONTROL)))
            {
#if TESTAICONTROL
            DebugAI(String("Ending turn for %d because of error from HandleItem", pSoldier.ubID));
#endif

                EndAIGuysTurn(pSoldier);
            }
        }

        // DON'T DE-SELECT GUY.....
        //else
        //{
        // Check if this is our selected guy...
        //	if ( pSoldier.ubID == gusSelectedSoldier )
        //	{
        //		SelectNextAvailSoldier( pSoldier );			
        //		}
        //}
    }


    public static float CalcSoldierNextBleed(SOLDIERTYPE? pSoldier)
    {
        int bBandaged;

        // calculate how many turns before he bleeds again
        // bleeding faster the lower life gets, and if merc is running around
        //pSoldier.nextbleed = 2 + (pSoldier.life / (10 + pSoldier.tilesMoved));  // min = 2

        // if bandaged, give 1/2 of the bandaged life points back into equation
        bBandaged = pSoldier.bLifeMax - pSoldier.bLife - pSoldier.bBleeding;

        return (1 + ((pSoldier.bLife + bBandaged / 2) / (10 + pSoldier.bTilesMoved)));  // min = 1
    }

    float CalcSoldierNextUnmovingBleed(SOLDIERTYPE? pSoldier)
    {
        int bBandaged;

        // calculate bleeding rate without the penalty for tiles moved

        // if bandaged, give 1/2 of the bandaged life points back into equation
        bBandaged = pSoldier.bLifeMax - pSoldier.bLife - pSoldier.bBleeding;

        return (1 + ((pSoldier.bLife + bBandaged / 2) / 10));  // min = 1
    }

    void HandlePlacingRoofMarker(SOLDIERTYPE? pSoldier, int sGridNo, bool fSet, bool fForce)
    {
        LEVELNODE? pRoofNode;
        LEVELNODE? pNode;

        if (pSoldier.bVisible == -1 && fSet)
        {
            return;
        }

        if (pSoldier.bTeam != gbPlayerNum)
        {
            //return;
        }

        // If we are on the roof, add roof UI peice!
        if (pSoldier.bLevel == SECOND_LEVEL)
        {
            // Get roof node
            pRoofNode = gpWorldLevelData[sGridNo].pRoofHead;

            // Return if we are still climbing roof....
            if (pSoldier.usAnimState == AnimationStates.CLIMBUPROOF && !fForce)
            {
                return;
            }

            if (pRoofNode != null)
            {
                if (fSet)
                {
                    if (gpWorldLevelData[sGridNo].uiFlags.HasFlag(MAPELEMENTFLAGS.REVEALED))
                    {
                        // Set some flags on this poor thing
                        //pRoofNode.uiFlags |= ( LEVELNODE_USEBESTTRANSTYPE | LEVELNODE_REVEAL | LEVELNODE_DYNAMIC  );
                        //pRoofNode.uiFlags |= ( LEVELNODE_DYNAMIC );
                        //pRoofNode.uiFlags &= ( ~LEVELNODE_HIDDEN );
                        //ResetSpecificLayerOptimizing( TILES_DYNAMIC_ROOF );
                    }
                }
                else
                {
                    if (gpWorldLevelData[sGridNo].uiFlags.HasFlag(MAPELEMENTFLAGS.REVEALED))
                    {
                        // Remove some flags on this poor thing
                        //pRoofNode.uiFlags &= ~( LEVELNODE_USEBESTTRANSTYPE | LEVELNODE_REVEAL | LEVELNODE_DYNAMIC );

                        //pRoofNode.uiFlags |= LEVELNODE_HIDDEN;
                    }
                }

                if (fSet)
                {
                    // If it does not exist already....
                    if (!IndexExistsInRoofLayer(sGridNo, TileDefines.FIRSTPOINTERS11))
                    {
                        pNode = AddRoofToTail(sGridNo, TileDefines.FIRSTPOINTERS11);
                        pNode.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                        pNode.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                    }
                }
                else
                {
                    RemoveRoof(sGridNo, TileDefines.FIRSTPOINTERS11);
                }
            }
        }
    }

    public static void PositionSoldierLight(SOLDIERTYPE? pSoldier)
    {
        // DO ONLY IF WE'RE AT A GOOD LEVEL
        if (ubAmbientLightLevel < MIN_AMB_LEVEL_FOR_MERC_LIGHTS)
        {
            return;
        }

        if (!pSoldier.bInSector)
        {
            return;
        }

        if (pSoldier.bTeam != gbPlayerNum)
        {
            return;
        }

        if (pSoldier.bLife < OKLIFE)
        {
            return;
        }

        //if the player DOESNT want the merc to cast light
        if (!GameSettings.fOptions[TOPTION.MERC_CASTS_LIGHT])
        {
            return;
        }

        if (pSoldier.iLight == -1)
        {
            CreateSoldierLight(pSoldier);
        }

        //if ( pSoldier.ubID == gusSelectedSoldier )
        {
            LightSpritePower(pSoldier.iLight, true);
            LightSpriteFake(pSoldier.iLight);

            LightSpritePosition(pSoldier.iLight, (int)(pSoldier.sX / CELL_X_SIZE), (int)(pSoldier.sY / CELL_Y_SIZE));
        }
    }

    public static void SetCheckSoldierLightFlag(SOLDIERTYPE? pSoldier)
    {
        PositionSoldierLight(pSoldier);
        //pSoldier.uiStatusFlags |= SOLDIER_RECHECKLIGHT;
    }


    void PickPickupAnimation(SOLDIERTYPE? pSoldier, Items iItemIndex, int sGridNo, int bZLevel)
    {
        WorldDirections bDirection;
        STRUCTURE? pStructure;
        bool fDoNormalPickup = true;


        // OK, Given the gridno, determine if it's the same one or different....
        if (sGridNo != pSoldier.sGridNo)
        {
            // Get direction to face....
            bDirection = GetDirectionFromGridNo(sGridNo, pSoldier);
            pSoldier.ubPendingDirection = bDirection;

            // Change to pickup animation
            EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.ADJACENT_GET_ITEM, 0, false);

            if (!(pSoldier.uiStatusFlags.HasFlag(SOLDIER.PC)))
            {
                // set "pending action" value for AI so it will wait
                pSoldier.bAction = AI_ACTION.PENDING_ACTION;
            }

        }
        else
        {
            // If in water....
            if (MercInWater(pSoldier))
            {
                UnSetUIBusy(pSoldier.ubID);
                HandleSoldierPickupItem(pSoldier, iItemIndex, sGridNo, bZLevel);
                SoldierGotoStationaryStance(pSoldier);
                if (!(pSoldier.uiStatusFlags.HasFlag(SOLDIER.PC)))
                {
                    // reset action value for AI because we're done!
                    ActionDone(pSoldier);
                }

            }
            else
            {
                // Don't show animation of getting item, if we are not standing
                switch (gAnimControl[pSoldier.usAnimState].ubHeight)
                {
                    case AnimationHeights.ANIM_STAND:

                        // OK, if we are looking at z-level >0, AND
                        // we have a strucxture with items in it
                        // look for orientation and use angle accordingly....
                        if (bZLevel > 0)
                        {
                            //#if 0
                            // Get direction to face....
                            if ((pStructure = StructureInternals.FindStructure(sGridNo, (STRUCTUREFLAGS.HASITEMONTOP | STRUCTUREFLAGS.OPENABLE))) != null)
                            {
                                fDoNormalPickup = false;

                                // OK, look at orientation
                                switch (pStructure.ubWallOrientation)
                                {
                                    case WallOrientation.OUTSIDE_TOP_LEFT:
                                    case WallOrientation.INSIDE_TOP_LEFT:

                                        bDirection = WorldDirections.NORTH;
                                        break;

                                    case WallOrientation.OUTSIDE_TOP_RIGHT:
                                    case WallOrientation.INSIDE_TOP_RIGHT:

                                        bDirection = WorldDirections.WEST;
                                        break;

                                    default:

                                        bDirection = pSoldier.bDirection;
                                        break;
                                }

                                //pSoldier.ubPendingDirection = bDirection;
                                EVENT_SetSoldierDesiredDirection(pSoldier, bDirection);
                                EVENT_SetSoldierDirection(pSoldier, bDirection);

                                // Change to pickup animation
                                EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.ADJACENT_GET_ITEM, 0, false);
                            }
                            //#endif
                        }

                        if (fDoNormalPickup)
                        {
                            EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.PICKUP_ITEM, 0, false);
                        }

                        if (!(pSoldier.uiStatusFlags.HasFlag(SOLDIER.PC)))
                        {
                            // set "pending action" value for AI so it will wait
                            pSoldier.bAction = AI_ACTION.PENDING_ACTION;
                        }
                        break;

                    case AnimationHeights.ANIM_CROUCH:
                    case AnimationHeights.ANIM_PRONE:

                        UnSetUIBusy(pSoldier.ubID);
                        HandleSoldierPickupItem(pSoldier, iItemIndex, sGridNo, bZLevel);
                        SoldierGotoStationaryStance(pSoldier);
                        if (!(pSoldier.uiStatusFlags.HasFlag(SOLDIER.PC)))
                        {
                            // reset action value for AI because we're done!
                            ActionDone(pSoldier);
                        }
                        break;
                }
            }
        }
    }

    void PickDropItemAnimation(SOLDIERTYPE? pSoldier)
    {
        // Don't show animation of getting item, if we are not standing
        switch (gAnimControl[pSoldier.usAnimState].ubHeight)
        {
            case AnimationHeights.ANIM_STAND:

                EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.DROP_ITEM, 0, false);
                break;

            case AnimationHeights.ANIM_CROUCH:
            case AnimationHeights.ANIM_PRONE:

                SoldierHandleDropItem(pSoldier);
                SoldierGotoStationaryStance(pSoldier);
                break;
        }
    }


    void EVENT_SoldierBeginCutFence(SOLDIERTYPE? pSoldier, int sGridNo, WorldDirections ubDirection)
    {
        // Make sure we have a structure here....
        if (IsCuttableWireFenceAtGridNo(sGridNo))
        {
            // CHANGE DIRECTION AND GOTO ANIMATION NOW
            EVENT_SetSoldierDesiredDirection(pSoldier, ubDirection);
            EVENT_SetSoldierDirection(pSoldier, ubDirection);

            //bool CutWireFence( int sGridNo )

            // SET TARGET GRIDNO
            pSoldier.sTargetGridNo = sGridNo;

            // CHANGE TO ANIMATION
            EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.CUTTING_FENCE, 0, false);
        }
    }


    void EVENT_SoldierBeginRepair(SOLDIERTYPE? pSoldier, int sGridNo, WorldDirections ubDirection)
    {
        int bRepairItem;
        int ubID;

        // Make sure we have a structure here....
        bRepairItem = IsRepairableStructAtGridNo(sGridNo, out ubID);

        if (bRepairItem)
        {
            // CHANGE DIRECTION AND GOTO ANIMATION NOW
            EVENT_SetSoldierDesiredDirection(pSoldier, ubDirection);
            EVENT_SetSoldierDirection(pSoldier, ubDirection);

            //bool CutWireFence( int sGridNo )

            // SET TARGET GRIDNO
            //pSoldier.sTargetGridNo = sGridNo;

            // CHANGE TO ANIMATION
            EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.GOTO_REPAIRMAN, 0, false);
            // SET BUDDY'S ASSIGNMENT TO REPAIR...

            // Are we a SAM site? ( 3 == SAM )
            if (bRepairItem == 3)
            {
                SetSoldierAssignment(pSoldier, Assignments.REPAIR, true, false, -1);
            }
            else if (bRepairItem == 2) // ( 2 == VEHICLE )
            {
                SetSoldierAssignment(pSoldier, Assignments.REPAIR, false, false, ubID);
            }

        }
    }

    void EVENT_SoldierBeginRefuel(SOLDIERTYPE? pSoldier, int sGridNo, WorldDirections ubDirection)
    {
        int bRefuelItem;
        int ubID;

        // Make sure we have a structure here....
        bRefuelItem = IsRefuelableStructAtGridNo(sGridNo, out ubID);

        if (bRefuelItem)
        {
            // CHANGE DIRECTION AND GOTO ANIMATION NOW
            EVENT_SetSoldierDesiredDirection(pSoldier, ubDirection);
            EVENT_SetSoldierDirection(pSoldier, ubDirection);

            //bool CutWireFence( int sGridNo )

            // SET TARGET GRIDNO
            //pSoldier.sTargetGridNo = sGridNo;

            // CHANGE TO ANIMATION
            EVENT_InitNewSoldierAnim(pSoldier, REFUEL_VEHICLE, 0, false);
            // SET BUDDY'S ASSIGNMENT TO REPAIR...
        }
    }


    void EVENT_SoldierBeginTakeBlood(SOLDIERTYPE? pSoldier, int sGridNo, int ubDirection)
    {
        ROTTING_CORPSE* pCorpse;


        // See if these is a corpse here....
        pCorpse = GetCorpseAtGridNo(sGridNo, pSoldier.bLevel);

        if (pCorpse != null)
        {
            pSoldier.uiPendingActionData4 = pCorpse.iID;

            // CHANGE DIRECTION AND GOTO ANIMATION NOW
            EVENT_SetSoldierDesiredDirection(pSoldier, ubDirection);
            EVENT_SetSoldierDirection(pSoldier, ubDirection);

            EVENT_InitNewSoldierAnim(pSoldier, TAKE_BLOOD_FROM_CORPSE, 0, false);
        }
        else
        {
            // Say NOTHING quote...
            DoMercBattleSound(pSoldier, BATTLE_SOUND_NOTHING);
        }
    }


    void EVENT_SoldierBeginAttachCan(SOLDIERTYPE? pSoldier, int sGridNo, WorldDirections ubDirection)
    {
        STRUCTURE? pStructure;
        DOOR_STATUS? pDoorStatus;

        // OK, find door, attach to door, do animation...., remove item....

        // First make sure we still have item in hand....
        if (pSoldier.inv[InventorySlot.HANDPOS].usItem != Items.STRING_TIED_TO_TIN_CAN)
        {
            return;
        }

        pStructure = StructureInternals.FindStructure(sGridNo, STRUCTUREFLAGS.ANYDOOR);

        if (pStructure == null)
        {
            return;
        }

        // Modify door status to make sure one is created for this door
        // Use the current door state for this
        if (!(pStructure.fFlags.HasFlag(STRUCTUREFLAGS.OPEN)))
        {
            ModifyDoorStatus(sGridNo, false, false);
        }
        else
        {
            ModifyDoorStatus(sGridNo, true, true);
        }

        // Now get door status...
        pDoorStatus = GetDoorStatus(sGridNo);
        if (pDoorStatus == null)
        {
            // SOmething wrong here...
            return;
        }

        // OK set flag!
        pDoorStatus.ubFlags |= DOOR_STATUS_FLAGS.HAS_TIN_CAN;

        // Do animation
        EVENT_SetSoldierDesiredDirection(pSoldier, ubDirection);
        EVENT_SetSoldierDirection(pSoldier, ubDirection);

        EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.ATTACH_CAN_TO_STRING, 0, false);

        // Remove item...
        ItemSubSystem.DeleteObj((pSoldier.inv[InventorySlot.HANDPOS]));
        fInterfacePanelDirty = DIRTYLEVEL2;

    }


    void EVENT_SoldierBeginReloadRobot(SOLDIERTYPE? pSoldier, int sGridNo, WorldDirections ubDirection, int ubMercSlot)
    {
        int ubPerson;

        // Make sure we have a robot here....
        ubPerson = WorldManager.WhoIsThere2(sGridNo, pSoldier.bLevel);

        if (ubPerson != NOBODY && MercPtrs[ubPerson].uiStatusFlags.HasFlag(SOLDIER.ROBOT))
        {
            // CHANGE DIRECTION AND GOTO ANIMATION NOW
            EVENT_SetSoldierDesiredDirection(pSoldier, ubDirection);
            EVENT_SetSoldierDirection(pSoldier, ubDirection);

            // CHANGE TO ANIMATION
            EVENT_InitNewSoldierAnim(pSoldier, RELOAD_ROBOT, 0, false);

        }
    }



    void ResetSoldierChangeStatTimer(SOLDIERTYPE? pSoldier)
    {
        pSoldier.uiChangeLevelTime = 0;
        pSoldier.uiChangeHealthTime = 0;
        pSoldier.uiChangeStrengthTime = 0;
        pSoldier.uiChangeDexterityTime = 0;
        pSoldier.uiChangeAgilityTime = 0;
        pSoldier.uiChangeWisdomTime = 0;
        pSoldier.uiChangeLeadershipTime = 0;
        pSoldier.uiChangeMarksmanshipTime = 0;
        pSoldier.uiChangeExplosivesTime = 0;
        pSoldier.uiChangeMedicalTime = 0;
        pSoldier.uiChangeMechanicalTime = 0;


        return;
    }


    void ChangeToFlybackAnimation(SOLDIERTYPE? pSoldier, int bDirection)
    {
        int usNewGridNo;

        // Get dest gridno, convert to center coords
        usNewGridNo = IsometricUtils.NewGridNo(pSoldier.sGridNo, IsometricUtils.DirectionInc(gOppositeDirection[bDirection]));
        usNewGridNo = IsometricUtils.NewGridNo(usNewGridNo, IsometricUtils.DirectionInc(gOppositeDirection[bDirection]));

        // Remove any previous actions
        pSoldier.ubPendingAction = NO_PENDING_ACTION;

        // Set path....
        pSoldier.usPathDataSize = 0;
        pSoldier.usPathIndex = 0;
        pSoldier.usPathingData[pSoldier.usPathDataSize] = gOppositeDirection[pSoldier.bDirection];
        pSoldier.usPathDataSize++;
        pSoldier.usPathingData[pSoldier.usPathDataSize] = gOppositeDirection[pSoldier.bDirection];
        pSoldier.usPathDataSize++;
        pSoldier.sFinalDestination = usNewGridNo;
        EVENT_InternalSetSoldierDestination(pSoldier, pSoldier.usPathingData[pSoldier.usPathIndex], false, FLYBACK_HIT);

        // Get a new direction based on direction
        EVENT_InitNewSoldierAnim(pSoldier, FLYBACK_HIT, 0, false);
    }

    void ChangeToFallbackAnimation(SOLDIERTYPE? pSoldier, int bDirection)
    {
        int usNewGridNo;

        // Get dest gridno, convert to center coords
        usNewGridNo = NewGridNo((int)pSoldier.sGridNo, DirectionInc(gOppositeDirection[bDirection]));
        //usNewGridNo = NewGridNo( (int)usNewGridNo, (int)(-1 * DirectionInc( bDirection ) ) );

        // Remove any previous actions
        pSoldier.ubPendingAction = NO_PENDING_ACTION;

        // Set path....
        pSoldier.usPathDataSize = 0;
        pSoldier.usPathIndex = 0;
        pSoldier.usPathingData[pSoldier.usPathDataSize] = gOppositeDirection[pSoldier.bDirection];
        pSoldier.usPathDataSize++;
        pSoldier.sFinalDestination = usNewGridNo;
        EVENT_InternalSetSoldierDestination(pSoldier, pSoldier.usPathingData[pSoldier.usPathIndex], false, FALLBACK_HIT_STAND);

        // Get a new direction based on direction
        EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.FALLBACK_HIT_STAND, 0, false);
    }


    public static void SetSoldierCowerState(SOLDIERTYPE? pSoldier, bool fOn)
    {
        // Robot's don't cower!
        if (pSoldier.ubBodyType == SoldierBodyTypes.ROBOTNOWEAPON)
        {
            //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("ERROR: Robot was told to cower!"));
            return;
        }

        // OK< set flag and do anim...
        if (fOn)
        {
            if (!(pSoldier.uiStatusFlags.HasFlag(SOLDIER.COWERING)))
            {
                EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.START_COWER, 0, false);

                pSoldier.uiStatusFlags |= SOLDIER.COWERING;

                pSoldier.ubDesiredHeight = AnimationHeights.ANIM_CROUCH;
            }
        }
        else
        {
            if ((pSoldier.uiStatusFlags.HasFlag(SOLDIER.COWERING)))
            {
                EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.END_COWER, 0, false);

                pSoldier.uiStatusFlags &= (~SOLDIER.COWERING);

                pSoldier.ubDesiredHeight = AnimationHeights.ANIM_STAND;
            }
        }
    }

    void MercStealFromMerc(SOLDIERTYPE? pSoldier, SOLDIERTYPE? pTarget)
    {
        int sActionGridNo, sGridNo, sAdjustedGridNo;
        int ubDirection;


        // OK, find an adjacent gridno....
        sGridNo = pTarget.sGridNo;

        // See if we can get there to punch	
        sActionGridNo = FindAdjacentGridEx(pSoldier, sGridNo, &ubDirection, &sAdjustedGridNo, true, false);
        if (sActionGridNo != -1)
        {
            // SEND PENDING ACTION
            pSoldier.ubPendingAction = MERC_STEAL;
            pSoldier.sPendingActionData2 = pTarget.sGridNo;
            pSoldier.bPendingActionData3 = ubDirection;
            pSoldier.ubPendingActionAnimCount = 0;

            // CHECK IF WE ARE AT THIS GRIDNO NOW
            if (pSoldier.sGridNo != sActionGridNo)
            {
                // WALK UP TO DEST FIRST
                SendGetNewSoldierPathEvent(pSoldier, sActionGridNo, pSoldier.usUIMovementMode);
            }
            else
            {
                EVENT_SetSoldierDesiredDirection(pSoldier, ubDirection);
                EVENT_InitNewSoldierAnim(pSoldier, STEAL_ITEM, 0, false);
            }

            // OK, set UI
            gTacticalStatus.ubAttackBusyCount++;
            // reset attacking item (hand)
            pSoldier.usAttackingWeapon = 0;
            // DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("!!!!!!! Starting STEAL attack, attack count now %d", gTacticalStatus.ubAttackBusyCount));

            SetUIBusy(pSoldier.ubID);
        }
    }

    bool PlayerSoldierStartTalking(SOLDIERTYPE? pSoldier, int ubTargetID, bool fValidate)
    {
        WorldDirections sFacingDir;
        int sXPos, sYPos, sAPCost;
        SOLDIERTYPE? pTSoldier;
        int uiRange;

        if (ubTargetID == NOBODY)
        {
            return (false);
        }

        pTSoldier = MercPtrs[ubTargetID];

        // Check distance again, to be sure
        if (fValidate)
        {
            // OK, since we locked this guy from moving
            // we should be close enough, so talk ( unless he is now dead )
            if (!IsValidTalkableNPC((int)ubTargetID, false, false, false))
            {
                return (false);
            }

            uiRange = GetRangeFromGridNoDiff(pSoldier.sGridNo, pTSoldier.sGridNo);

            if (uiRange > (NPC_TALK_RADIUS * 2))
            {
                // Todo here - should we follow dude?
                return (false);
            }


        }

        // Get APs...
        sAPCost = AP.TALK;

        // Deduct points from our guy....
        DeductPoints(pSoldier, sAPCost, 0);

        IsometricUtils.ConvertGridNoToXY(pTSoldier.sGridNo, out sXPos, out sYPos);

        // Get direction from mouse pos
        sFacingDir = GetDirectionFromXY(sXPos, sYPos, pSoldier);

        // Set our guy facing
        SendSoldierSetDesiredDirectionEvent(pSoldier, sFacingDir);

        // Set NPC facing
        SendSoldierSetDesiredDirectionEvent(pTSoldier, gOppositeDirection[sFacingDir]);

        // Stop our guys...
        EVENT_StopMerc(pSoldier, pSoldier.sGridNo, pSoldier.bDirection);

        // ATE; Check for normal civs...
        if (GetCivType(pTSoldier) != CIV_TYPE_NA)
        {
            StartCivQuote(pTSoldier);
            return (false);
        }


        // Are we an EPC that is being escorted?
        if (pTSoldier.ubProfile != NO_PROFILE && pTSoldier.ubWhatKindOfMercAmI == MERC_TYPE.EPC)
        {
            return (InitiateConversation(pTSoldier, pSoldier, APPROACH_EPC_WHO_IS_RECRUITED, 0));
            //Converse( pTSoldier.ubProfile, pSoldier.ubProfile, APPROACH_EPC_WHO_IS_RECRUITED, 0 );
        }
        else if (pTSoldier.IsNeutral)
        {
            switch (pTSoldier.ubProfile)
            {
                case NPCID.JIM:
                case NPCID.JACK:
                case NPCID.OLAF:
                case NPCID.RAY:
                case NPCID.OLGA:
                case NPCID.TYRONE:
                    // Start combat etc
                    DeleteTalkingMenu();
                    AIMain.CancelAIAction(pTSoldier, 1);
                    AddToShouldBecomeHostileOrSayQuoteList(pTSoldier.ubID);
                    break;
                default:
                    // Start talking!
                    return (InitiateConversation(pTSoldier, pSoldier, NPC_INITIAL_QUOTE, 0));
                    break;
            }
        }
        else
        {
            // Start talking with hostile NPC
            return (InitiateConversation(pTSoldier, pSoldier, APPROACH_ENEMY_NPC_QUOTE, 0));
        }

        return (true);
    }


    public static bool IsValidSecondHandShot(SOLDIERTYPE? pSoldier)
    {
        if (Item[pSoldier.inv[InventorySlot.SECONDHANDPOS].usItem].usItemClass == IC.GUN &&
                 !(Item[pSoldier.inv[InventorySlot.SECONDHANDPOS].usItem].fFlags.HasFlag(ItemAttributes.ITEM_TWO_HANDED)) &&
                 !pSoldier.bDoBurst &&
                 pSoldier.inv[InventorySlot.HANDPOS].usItem != Items.GLAUNCHER &&
                 Item[pSoldier.inv[InventorySlot.HANDPOS].usItem].usItemClass == IC.GUN &&
                 pSoldier.inv[InventorySlot.SECONDHANDPOS].bGunStatus >= USABLE &&
                 pSoldier.inv[InventorySlot.SECONDHANDPOS].ubGunShotsLeft > 0)
        {
            return (true);
        }

        return (false);
    }

    bool IsValidSecondHandShotForReloadingPurposes(SOLDIERTYPE? pSoldier)
    {
        // should be maintained as same as function above with line
        // about ammo taken out!
        if (Item[pSoldier.inv[InventorySlot.SECONDHANDPOS].usItem].usItemClass == IC.GUN &&
                 !pSoldier.bDoBurst &&
                 pSoldier.inv[InventorySlot.HANDPOS].usItem != Items.GLAUNCHER &&
                 Item[pSoldier.inv[InventorySlot.HANDPOS].usItem].usItemClass == IC.GUN &&
                 pSoldier.inv[InventorySlot.SECONDHANDPOS].bGunStatus >= USABLE //&&
                                                                                //			 pSoldier.inv[InventorySlot.SECONDHANDPOS].ubGunShotsLeft > 0 &&
                                                                                //			 gAnimControl[ pSoldier.usAnimState ].ubEndHeight != ANIM_PRONE )
                )
        {
            return (true);
        }

        return (false);
    }



    public static bool CanRobotBeControlled(SOLDIERTYPE pSoldier)
    {
        SOLDIERTYPE? pController;

        if (!(pSoldier.uiStatusFlags.HasFlag(SOLDIER.ROBOT)))
        {
            return (false);
        }

        if (pSoldier.ubRobotRemoteHolderID == NOBODY)
        {
            return (false);
        }

        pController = MercPtrs[pSoldier.ubRobotRemoteHolderID];

        if (pController.bActive)
        {
            if (ControllingRobot(pController))
            {
                // ALL'S OK!
                return (true);
            }
        }

        return (false);
    }


    public static bool ControllingRobot(SOLDIERTYPE? pSoldier)
    {
        SOLDIERTYPE? pRobot;
        InventorySlot bPos;

        if (!pSoldier.bActive)
        {
            return (false);
        }

        // EPCs can't control the robot (no inventory to hold remote, for one)
        if (AM_AN_EPC(pSoldier))
        {
            return (false);
        }

        // Don't require pSoldier.bInSector here, it must work from mapscreen!

        // are we in ok shape?
        if (pSoldier.bLife < OKLIFE || (pSoldier.bTeam != gbPlayerNum))
        {
            return (false);
        }

        // allow control from within vehicles - allows strategic travel in a vehicle with robot!
        if ((pSoldier.bAssignment >= Assignments.ON_DUTY) && (pSoldier.bAssignment != Assignments.VEHICLE))
        {
            return (false);
        }

        // is the soldier wearing a robot remote control?
        bPos = ItemSubSystem.FindObj(pSoldier, Items.ROBOT_REMOTE_CONTROL);
        if (bPos != InventorySlot.HEAD1POS && bPos != InventorySlot.HEAD2POS)
        {
            return (false);
        }

        // Find the robot
        pRobot = SoldierProfileSubSystem.FindSoldierByProfileID(NPCID.ROBOT, true);
        if (pRobot is null)
        {
            return (false);
        }

        if (pRobot.bActive)
        {
            // Are we in the same sector....?
            // ARM: CHANGED TO WORK IN MAPSCREEN, DON'T USE WorldSector HERE
            if (pRobot.sSectorX == pSoldier.sSectorX &&
                     pRobot.sSectorY == pSoldier.sSectorY &&
                     pRobot.bSectorZ == pSoldier.bSectorZ)
            {
                // they have to be either both in sector, or both on the road
                if (pRobot.fBetweenSectors == pSoldier.fBetweenSectors)
                {
                    // if they're on the road...
                    if (pRobot.fBetweenSectors)
                    {
                        // they have to be in the same squad or vehicle
                        if (pRobot.bAssignment != pSoldier.bAssignment)
                        {
                            return (false);
                        }

                        // if in a vehicle, must be the same vehicle
                        if (pRobot.bAssignment == VEHICLE && (pRobot.iVehicleId != pSoldier.iVehicleId))
                        {
                            return (false);
                        }
                    }

                    // all OK!
                    return (true);
                }
            }
        }

        return (false);
    }


    SOLDIERTYPE? GetRobotController(SOLDIERTYPE? pSoldier)
    {
        if (pSoldier.ubRobotRemoteHolderID == NOBODY)
        {
            return (null);
        }
        else
        {
            return (MercPtrs[pSoldier.ubRobotRemoteHolderID]);
        }
    }

    void UpdateRobotControllerGivenRobot(SOLDIERTYPE? pRobot)
    {
        SOLDIERTYPE? pTeamSoldier;
        int cnt = 0;

        // Loop through guys and look for a controller!

        // set up soldier ptr as first element in mercptrs list
        cnt = gTacticalStatus.Team[gbPlayerNum].bFirstID;

        // run through list
        for (pTeamSoldier = MercPtrs[cnt]; cnt <= gTacticalStatus.Team[gbPlayerNum].bLastID; cnt++, pTeamSoldier++)
        {
            if (pTeamSoldier.bActive)
            {
                if (ControllingRobot(pTeamSoldier))
                {
                    pRobot.ubRobotRemoteHolderID = pTeamSoldier.ubID;
                    return;
                }
            }
        }

        pRobot.ubRobotRemoteHolderID = NOBODY;
    }


    void UpdateRobotControllerGivenController(SOLDIERTYPE? pSoldier)
    {
        SOLDIERTYPE? pTeamSoldier;
        int cnt = 0;

        // First see if are still controlling the robot
        if (!ControllingRobot(pSoldier))
        {
            return;
        }

        // set up soldier ptr as first element in mercptrs list
        cnt = gTacticalStatus.Team[gbPlayerNum].bFirstID;

        // Loop through guys to find the robot....
        for (pTeamSoldier = MercPtrs[cnt]; cnt <= gTacticalStatus.Team[gbPlayerNum].bLastID; cnt++, pTeamSoldier++)
        {
            if (pTeamSoldier.bActive && (pTeamSoldier.uiStatusFlags.HasFlag(SOLDIER.ROBOT)))
            {
                pTeamSoldier.ubRobotRemoteHolderID = pSoldier.ubID;
            }
        }
    }


    public static void HandleSoldierTakeDamageFeedback(SOLDIERTYPE? pSoldier)
    {
        // Do sound.....
        // if ( pSoldier.bLife >= CONSCIOUSNESS )
        {
            // ATE: Limit how often we grunt...
            if ((GetJA2Clock() - pSoldier.uiTimeSinceLastBleedGrunt) > 1000)
            {
                pSoldier.uiTimeSinceLastBleedGrunt = GetJA2Clock();

                DoMercBattleSound(pSoldier, (int)(BATTLE_SOUND_HIT1 + Globals.Random.Next(2)));
            }
        }

        // Flash portrait....
        pSoldier.fFlashPortrait = FLASH_PORTRAIT.START;
        pSoldier.bFlashPortraitFrame = FLASH_PORTRAIT.STARTSHADE;
        RESETTIMECOUNTER(ref pSoldier.PortraitFlashCounter, (uint)FLASH_PORTRAIT.DELAY);
    }


    void HandleSystemNewAISituation(SOLDIERTYPE? pSoldier, bool fResetABC)
    {
        // Are we an AI guy?
        if (gTacticalStatus.ubCurrentTeam != gbPlayerNum && pSoldier.bTeam != gbPlayerNum)
        {
            if (pSoldier.bNewSituation == IS_NEW_SITUATION)
            {
                // Cancel what they were doing....
                pSoldier.usPendingAnimation = NO_PENDING_ANIMATION;
                pSoldier.usPendingAnimation2 = NO_PENDING_ANIMATION;
                pSoldier.fTurningFromPronePosition = false;
                pSoldier.ubPendingDirection = NO_PENDING_DIRECTION;
                pSoldier.ubPendingAction = NO_PENDING_ACTION;
                pSoldier.bEndDoorOpenCode = 0;

                // if this guy isn't under direct AI control, WHO GIVES A FLYING FLICK?
                if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.UNDERAICONTROL))
                {
                    if (pSoldier.fTurningToShoot)
                    {
                        pSoldier.fTurningToShoot = false;
                        // Release attacker			
                        // OK - this is hightly annoying , but due to the huge combinations of
                        // things that can happen - 1 of them is that sLastTarget will get unset
                        // after turn is done - so set flag here to tell it not to...
                        pSoldier.fDontUnsetLastTargetFromTurn = true;
                        //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("@@@@@@@ Reducing attacker busy count..., ending fire because saw something: DONE IN SYSTEM NEW SITUATION"));
                        ReduceAttackBusyCount(pSoldier.ubID, false);
                    }

                    if (pSoldier.pTempObject != null)
                    {
                        // Place it back into inv....
                        AutoPlaceObject(pSoldier, pSoldier.pTempObject, false);
                        MemFree(pSoldier.pTempObject);
                        pSoldier.pTempObject = null;
                        pSoldier.usPendingAnimation = NO_PENDING_ANIMATION;
                        pSoldier.usPendingAnimation2 = NO_PENDING_ANIMATION;

                        // Decrement attack counter...
                        //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("@@@@@@@ Reducing attacker busy count..., ending throw because saw something: DONE IN SYSTEM NEW SITUATION"));
                        ReduceAttackBusyCount(pSoldier.ubID, false);
                    }

                }
            }
        }
    }

    void InternalPlaySoldierFootstepSound(SOLDIERTYPE? pSoldier)
    {
        int ubRandomSnd;
        int bVolume = MIDVOLUME;
        // Assume outside
        SoundDefine ubSoundBase = SoundDefine.WALK_LEFT_OUT;
        int ubRandomMax = 4;

        // Determine if we are on the floor
        if (!(pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE)))
        {
            if (pSoldier.usAnimState == HOPFENCE)
            {
                bVolume = HIGHVOLUME;
            }

            if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.ROBOT))
            {
                //PlaySoldierJA2Sample(pSoldier.ubID, ROBOT_BEEP, RATE_11025, SoundVolume(bVolume, pSoldier.sGridNo), 1, SoundDir(pSoldier.sGridNo), true);
                return;
            }

            //if ( SoldierOnScreen( pSoldier.ubID ) )
            {
                if (pSoldier.usAnimState == TerrainTypeDefines.CRAWLING)
                {
                    ubSoundBase = SoundDefine.CRAWL_1;
                }
                else
                {
                    // Pick base based on terrain over....
                    if (pSoldier.bOverTerrainType == TerrainTypeDefines.FLAT_FLOOR)
                    {
                        ubSoundBase = SoundDefine.WALK_LEFT_IN;
                    }
                    else if (pSoldier.bOverTerrainType == TerrainTypeDefines.DIRT_ROAD || pSoldier.bOverTerrainType == PAVED_ROAD)
                    {
                        ubSoundBase = SoundDefine.WALK_LEFT_ROAD;
                    }
                    else if (pSoldier.bOverTerrainType == TerrainTypeDefines.LOW_WATER || pSoldier.bOverTerrainType == MED_WATER)
                    {
                        ubSoundBase = SoundDefine.WATER_WALK1_IN;
                        ubRandomMax = 2;
                    }
                    else if (pSoldier.bOverTerrainType == TerrainTypeDefines.DEEP_WATER)
                    {
                        ubSoundBase = SoundDefine.SWIM_1;
                        ubRandomMax = 2;
                    }
                }

                // Pick a random sound...
                do
                {
                    ubRandomSnd = (int)Globals.Random.Next(ubRandomMax);

                } while (ubRandomSnd == pSoldier.ubLastFootPrintSound);

                pSoldier.ubLastFootPrintSound = ubRandomSnd;

                // OK, if in realtime, don't play at full volume, because too many people walking around
                // sounds don't sound good - ( unless we are the selected guy, then always play at reg volume )
                if (!(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)) && (pSoldier.ubID != gusSelectedSoldier))
                {
                    bVolume = LOWVOLUME;
                }

                //PlaySoldierJA2Sample(pSoldier.ubID, ubSoundBase + pSoldier.ubLastFootPrintSound, RATE_11025, SoundVolume(bVolume, pSoldier.sGridNo), 1, SoundDir(pSoldier.sGridNo), true);
            }
        }
    }

    public static void PlaySoldierFootstepSound(SOLDIERTYPE? pSoldier)
    {
        // normally, not in stealth mode
        if (!pSoldier.bStealthMode)
        {
            InternalPlaySoldierFootstepSound(pSoldier);
        }
    }

    void PlayStealthySoldierFootstepSound(SOLDIERTYPE? pSoldier)
    {
        // even if in stealth mode
        InternalPlaySoldierFootstepSound(pSoldier);
    }



    void CrowsFlyAway(int ubTeam)
    {
        int cnt;
        SOLDIERTYPE? pTeamSoldier;

        for (cnt = gTacticalStatus.Team[ubTeam].bFirstID, pTeamSoldier = MercPtrs[cnt]; cnt <= gTacticalStatus.Team[ubTeam].bLastID; cnt++, pTeamSoldier++)
        {
            if (pTeamSoldier.bActive && pTeamSoldier.bInSector)
            {
                if (pTeamSoldier.ubBodyType == CROW && pTeamSoldier.usAnimState != CROW_FLY)
                {
                    // fly away even if not seen!
                    HandleCrowFlyAway(pTeamSoldier);
                }
            }
        }
    }


#if JA2BETAVERSION
void DebugValidateSoldierData()
{
    int cnt;
    SOLDIERTYPE? pSoldier;
    CHAR16 sString[1024];
    bool fProblemDetected = false;
    static uiFrameCount = 0;


    // this function is too slow to run every frame, so do the check only every 50 frames
    if (uiFrameCount++ < 50)
    {
        return;
    }

    // reset frame counter
    uiFrameCount = 0;


    // Loop through our team...
    cnt = gTacticalStatus.Team[gbPlayerNum].bFirstID;
    for (pSoldier = MercPtrs[cnt]; cnt <= gTacticalStatus.Team[gbPlayerNum].bLastID; cnt++, pSoldier++)
    {
        if (pSoldier.bActive)
        {
            // OK, first check for alive people
            // Don't do this check if we are a vehicle...
            if (pSoldier.IsAlive && !(pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE)))
            {
                // Alive -- now check for proper group IDs
                if (pSoldier.ubGroupID == 0 && pSoldier.bAssignment != IN_TRANSIT && pSoldier.bAssignment != ASSIGNMENT_POW && !(pSoldier.uiStatusFlags & (SOLDIER.DRIVER | SOLDIER.PASSENGER)))
                {
                    // This is bad!
                    wprintf(sString, "Soldier Data Error: Soldier %d is alive but has a zero group ID.", cnt);
                    fProblemDetected = true;
                }
                else if ((pSoldier.ubGroupID != 0) && (GetGroup(pSoldier.ubGroupID) == null))
                {
                    // This is bad!
                    wprintf(sString, "Soldier Data Error: Soldier %d has an invalid group ID of %d.", cnt, pSoldier.ubGroupID);
                    fProblemDetected = true;
                }
            }
            else
            {
                if (pSoldier.ubGroupID != 0 && (pSoldier.uiStatusFlags.HasFlag(SOLDIER.DEAD)))
                {
                    // Dead guys should have 0 group IDs
                    //wprintf( sString, "GroupID Error: Soldier %d is dead but has a non-zero group ID.", cnt );
                    //fProblemDetected = true;
                }
            }

            // check for invalid sector data
            if ((pSoldier.bAssignment != IN_TRANSIT) &&
                     ((pSoldier.sSectorX <= 0) || (pSoldier.sSectorX >= 17) ||
                         (pSoldier.sSectorY <= 0) || (pSoldier.sSectorY >= 17) ||
                         (pSoldier.bSectorZ < 0) || (pSoldier.bSectorZ > 3)))
            {
                wprintf(sString, "Soldier Data Error: Soldier %d is located at %d/%d/%d.", cnt, pSoldier.sSectorX, pSoldier.sSectorY, pSoldier.bSectorZ);
                fProblemDetected = true;
            }
        }

        if (fProblemDetected)
        {
            SAIReportError(sString);
            /*
                        if ( guiCurrentScreen == MAP.SCREEN )
                            DoMapMessageBox( MSG_BOX_BASIC_STYLE, sString, MAP.SCREEN, MSG_BOX_FLAG_OK, MapScreenDefaultOkBoxCallback );
                        else
                            DoMessageBox( MSG_BOX_BASIC_STYLE, sString, GAME_SCREEN, ( int )MSG_BOX_FLAG_OK, null, null );
            */
            break;
        }
    }


    // also do this
    ValidatePlayersAreInOneGroupOnly();
}
#endif



    public static void BeginTyingToFall(SOLDIERTYPE? pSoldier)
    {
        pSoldier.bStartFallDir = pSoldier.bDirection;
        pSoldier.fTryingToFall = 1;

        // Randomize direction 
        if (Globals.Random.Next(50) < 25)
        {
            pSoldier.fFallClockwise = true;
        }
        else
        {
            pSoldier.fFallClockwise = false;
        }
    }

    void SetSoldierAsUnderAiControl(SOLDIERTYPE? pSoldierToSet)
    {
        int cnt;

        if (pSoldierToSet == null)
        {
            return;
        }

        // Loop through ALL teams...
        cnt = gTacticalStatus.Team[OUR_TEAM].bFirstID;
        foreach (var pSoldier in MercPtrs)
        //for (pSoldier = MercPtrs[cnt]; cnt <= gTacticalStatus.Team[LAST_TEAM].bLastID; cnt++, pSoldier++)
        {
            if (pSoldier.bActive)
            {
                pSoldier.uiStatusFlags &= ~SOLDIER.UNDERAICONTROL;
            }
        }

        pSoldierToSet.uiStatusFlags |= SOLDIER.UNDERAICONTROL;
    }

    void HandlePlayerTogglingLightEffects(bool fToggleValue)
    {
        if (fToggleValue)
        {
            //Toggle light status
            GameSettings.fOptions[TOPTION.MERC_CASTS_LIGHT] ^= true;
        }

        //Update all the mercs in the sector
        EnableDisableSoldierLightEffects(GameSettings.fOptions[TOPTION.MERC_CASTS_LIGHT]);

        RenderWorld.SetRenderFlags(RenderingFlags.FULL);
    }


    void EnableDisableSoldierLightEffects(bool fEnableLights)
    {
        SOLDIERTYPE? pSoldier = null;
        int cnt;

        // Loop through player teams...
        cnt = gTacticalStatus.Team[OUR_TEAM].bFirstID;
        for (pSoldier = MercPtrs[cnt]; cnt <= gTacticalStatus.Team[OUR_TEAM].bLastID; cnt++, pSoldier++)
        {
            //if the soldier is in the sector
            if (pSoldier.bActive && pSoldier.bInSector && pSoldier.bLife >= OKLIFE)
            {
                //if we are to enable the lights
                if (fEnableLights)
                {
                    //Add the light around the merc
                    PositionSoldierLight(pSoldier);
                }
                else
                {
                    //Delete the fake light the merc casts
                    DeleteSoldierLight(pSoldier);

                    //Light up the merc though
                    SetSoldierPersonalLightLevel(pSoldier);
                }
            }
        }
    }

    void SetSoldierPersonalLightLevel(SOLDIERTYPE? pSoldier)
    {
        if (pSoldier == null)
        {
            return;
        }

        if (pSoldier.sGridNo == NOWHERE)
        {
            return;
        }

        //THe light level for the soldier
        gpWorldLevelData[pSoldier.sGridNo].pMercHead.ubShadeLevel = 3;
        gpWorldLevelData[pSoldier.sGridNo].pMercHead.ubSumLights = 5;
        gpWorldLevelData[pSoldier.sGridNo].pMercHead.ubMaxLights = 5;
        gpWorldLevelData[pSoldier.sGridNo].pMercHead.ubNaturalShadeLevel = 5;
    }
}

public enum InventorySlot
{
    HELMETPOS = 0,
    VESTPOS,
    LEGPOS,
    HEAD1POS,
    HEAD2POS,
    HANDPOS,
    SECONDHANDPOS,
    BIGPOCK1POS,
    BIGPOCK2POS,
    BIGPOCK3POS,
    BIGPOCK4POS,
    SMALLPOCK1POS,
    SMALLPOCK2POS,
    SMALLPOCK3POS,
    SMALLPOCK4POS,
    SMALLPOCK5POS,
    SMALLPOCK6POS,
    SMALLPOCK7POS,
    SMALLPOCK8POS, // = 18, so 19 pockets needed

    NUM_INV_SLOTS,
};

//used for color codes, but also shows the enemy type for debugging purposes
public enum SOLDIER_CLASS
{
    NONE,
    ADMINISTRATOR,
    ELITE,
    ARMY,
    GREEN_MILITIA,
    REG_MILITIA,
    ELITE_MILITIA,
    CREATURE,
    MINER,
};


// Soldier status flags
[Flags]
public enum SOLDIER : uint
{
    IS_TACTICALLY_VALID = 0x00000001,
    SHOULD_BE_TACTICALLY_VALID = 0x00000002,
    MULTI_SELECTED = 0x00000004,
    PC = 0x00000008,
    ATTACK_NOTICED = 0x00000010,
    PCUNDERAICONTROL = 0x00000020,
    UNDERAICONTROL = 0x00000040,
    DEAD = 0x00000080,
    GREEN_RAY = 0x00000100,
    LOOKFOR_ITEMS = 0x00000200,
    ENEMY = 0x00000400,
    ENGAGEDINACTION = 0x00000800,
    ROBOT = 0x00001000,
    MONSTER = 0x00002000,
    ANIMAL = 0x00004000,
    VEHICLE = 0x00008000,
    MULTITILE_NZ = 0x00010000,
    MULTITILE_Z = 0x00020000,
    MULTITILE = (MULTITILE_Z | MULTITILE_NZ),
    RECHECKLIGHT = 0x00040000,
    TURNINGFROMHIT = 0x00080000,
    BOXER = 0x00100000,
    LOCKPENDINGACTIONCOUNTER = 0x00200000,
    COWERING = 0x00400000,
    MUTE = 0x00800000,
    GASSED = 0x01000000,
    OFF_MAP = 0x02000000,
    PAUSEANIMOVE = 0x04000000,
    DRIVER = 0x08000000,
    PASSENGER = 0x10000000,
    NPC_DOING_PUNCH = 0x20000000,
    NPC_SHOOTING = 0x40000000,
    LOOK_NEXT_TURNSOLDIER = 0x80000000,
}

public enum WM
{
    NORMAL = 0,
    BURST,
    ATTACHED,
    NUM_WEAPON_MODES
}

public enum MERC
{
    OPENDOOR,
    OPENSTRUCT,
    PICKUPITEM,
    PUNCH,
    KNIFEATTACK,
    GIVEAID,
    GIVEITEM,
    WAITFOROTHERSTOTRIGGER,
    CUTFFENCE,
    DROPBOMB,
    STEAL,
    TALK,
    ENTER_VEHICLE,
    REPAIR,
    RELOADROBOT,
    TAKEBLOOD,
    ATTACH_CAN,
    FUEL_VEHICLE,

    NO_PENDING_ACTION = 255,
}

[Flags]
public enum SOLDIER_MISC
{
    HEARD_GUNSHOT = 0x01,
    // make sure soldiers (esp tanks) are not hurt multiple times by explosions
    HURT_BY_EXPLOSION = 0x02,
    // should be revealed due to xrays
    XRAYED = 0x04,
}

// reasons for being unable to continue movement
public enum REASON_STOPPED
{
    NO_APS,
    SIGHT,
};


[Flags]
public enum HIT_BY
{
    TEARGAS = 0x01,
    MUSTARDGAS = 0x02,
    CREATUREGAS = 0x04,
}

public enum SOLDIER_QUOTE
{
    SAID_IN_SHIT = 0x0001,
    SAID_LOW_BREATH = 0x0002,
    SAID_BEING_PUMMELED = 0x0004,
    SAID_NEED_SLEEP = 0x0008,
    SAID_LOW_MORAL = 0x0010,
    SAID_MULTIPLE_CREATURES = 0x0020,
    SAID_ANNOYING_MERC = 0x0040,
    SAID_LIKESGUN = 0x0080,
    SAID_DROWNING = 0x0100,
    SAID_ROTTINGCORPSE = 0x0200,
    SAID_SPOTTING_CREATURE_ATTACK = 0x0400,
    SAID_SMELLED_CREATURE = 0x0800,
    SAID_ANTICIPATING_DANGER = 0x1000,
    SAID_WORRIED_ABOUT_CREATURES = 0x2000,
    SAID_PERSONALITY = 0x4000,
    SAID_FOUND_SOMETHING_NICE = 0x8000,
    SAID_EXT_HEARD_SOMETHING = 0x0001,
    SAID_EXT_SEEN_CREATURE_ATTACK = 0x0002,
    SAID_EXT_USED_BATTLESOUND_HIT = 0x0004,
    SAID_EXT_CLOSE_CALL = 0x0008,
    SAID_EXT_MIKE = 0x0010,
    SAID_DONE_ASSIGNMENT = 0x0020,
    SAID_BUDDY_1_WITNESSED = 0x0040,
    SAID_BUDDY_2_WITNESSED = 0x0080,
    SAID_BUDDY_3_WITNESSED = 0x0100,
}

public enum TAKE_DAMAGE
{
    GUNFIRE = 1,
    BLADE = 2,
    HANDTOHAND = 3,
    FALLROOF = 4,
    BLOODLOSS = 5,
    EXPLOSION = 6,
    ELECTRICITY = 7,
    GAS = 8,
    TENTACLES = 9,
    STRUCTURE_EXPLOSION = 10,
    OBJECT = 11,
}

// TYPEDEFS FOR ANIMATION PROFILES
public class ANIM_PROF_TILE
{
    public uint usTileFlags;
    public int bTileX;
    public int bTileY;
};

public class ANIM_PROF_DIR
{
    public int ubNumTiles;
    public List<ANIM_PROF_TILE> pTiles;
};

public class ANIM_PROF
{
    public Dictionary<WorldDirections, ANIM_PROF_DIR> Dirs = new();
};

// An enumeration for playing battle sounds
public enum BATTLE_SOUND
{
    OK1,
    OK2,
    COOL1,
    CURSE1,
    HIT1,
    HIT2,
    LAUGH1,
    ATTN1,
    DIE1,
    HUMM,
    NOTHING,
    GOTIT,
    LOWMARALE_OK1,
    LOWMARALE_OK2,
    LOWMARALE_ATTN1,
    LOCKED,
    ENEMY,
    NUM_MERC_BATTLE_SOUNDS
};


[Flags]
public enum DELAYED_MOVEMENT_FLAG
{
    NOTHING = 0x00,
    PATH_THROUGH_PEOPLE = 0x01,
}
