using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.SubSystems;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class AIUtils
{
    //
    // CJC's DG->JA2 conversion notes
    //
    // Commented out:
    //
    // InWaterOrGas - gas stuff
    // RoamingRange - point patrol stuff

    public static Dictionary<STATUS, Dictionary<MORALE, URGENCY>> Urgency = new()
    {
        {
            STATUS.GREEN,
            new()
            {
                { MORALE.HOPELESS,  URGENCY.LOW },
                { MORALE.WORRIED,   URGENCY.LOW },
                { MORALE.NORMAL,    URGENCY.LOW },
                { MORALE.CONFIDENT, URGENCY.LOW },
                { MORALE.FEARLESS,  URGENCY.LOW },
            }
        },
        {
            STATUS.YELLOW,
            new()
            {
                { MORALE.HOPELESS,  URGENCY.HIGH },
                { MORALE.WORRIED,   URGENCY.MED },
                { MORALE.NORMAL,    URGENCY.MED },
                { MORALE.CONFIDENT, URGENCY.LOW },
                { MORALE.FEARLESS,  URGENCY.LOW }
            }
        },
        {
            STATUS.RED,
            new()
            {
                { MORALE.HOPELESS,  URGENCY.HIGH },
                { MORALE.WORRIED,   URGENCY.MED },
                { MORALE.NORMAL,    URGENCY.MED },
                { MORALE.CONFIDENT, URGENCY.MED },
                { MORALE.FEARLESS,  URGENCY.MED }
            }
        },
        {
            STATUS.BLACK,
            new()
            {
                { MORALE.HOPELESS,  URGENCY.HIGH },
                { MORALE.WORRIED,   URGENCY.HIGH },
                { MORALE.NORMAL,    URGENCY.HIGH },
                { MORALE.CONFIDENT, URGENCY.MED },
                { MORALE.FEARLESS,  URGENCY.MED }
            }
        }
    };

    public static AnimationStates[,] MovementMode =
    {
        { AnimationStates.WALKING,   AnimationStates.WALKING,  AnimationStates.WALKING }, // AI_ACTION.NONE
    
    	{ AnimationStates.WALKING,  AnimationStates.WALKING,  AnimationStates.WALKING }, // AI_ACTION.RANDOM_PATROL
    	{ AnimationStates.WALKING,  AnimationStates.RUNNING,  AnimationStates.RUNNING }, // AI_ACTION.SEEK_FRIEND
    	{ AnimationStates.WALKING,  AnimationStates.RUNNING,  AnimationStates.RUNNING }, // AI_ACTION.SEEK_OPPONENT
    	{ AnimationStates.RUNNING,  AnimationStates.RUNNING,  AnimationStates.RUNNING }, // AI_ACTION.TAKE_COVER
    	{ AnimationStates.WALKING,  AnimationStates.RUNNING,  AnimationStates.RUNNING }, // AI_ACTION.GET_CLOSER
    
    	{ AnimationStates.WALKING,  AnimationStates.WALKING,  AnimationStates.WALKING }, // AI_ACTION.POINT_PATROL,
    	{ AnimationStates.WALKING,  AnimationStates.RUNNING,  AnimationStates.RUNNING }, // AI_ACTION.LEAVE_WATER_GAS,
    	{ AnimationStates.WALKING,  AnimationStates.SWATTING, AnimationStates.RUNNING }, // AI_ACTION.SEEK_NOISE,
    	{ AnimationStates.RUNNING,  AnimationStates.RUNNING,  AnimationStates.RUNNING }, // AI_ACTION.ESCORTED_MOVE,
    	{ AnimationStates.WALKING,  AnimationStates.RUNNING,  AnimationStates.RUNNING }, // AI_ACTION.RUN_AWAY,
    
    	{ AnimationStates.RUNNING,  AnimationStates.RUNNING,  AnimationStates.RUNNING }, // AI_ACTION.KNIFE_MOVE
    	{ AnimationStates.WALKING,  AnimationStates.WALKING,  AnimationStates.WALKING }, // AI_ACTION.APPROACH_MERC
    	{ AnimationStates.RUNNING,  AnimationStates.RUNNING,  AnimationStates.RUNNING }, // AI_ACTION.TRACK
    	{ AnimationStates.RUNNING,   AnimationStates.RUNNING,  AnimationStates.RUNNING },// AI_ACTION.EAT 
    	{ AnimationStates.WALKING,   AnimationStates.RUNNING,  AnimationStates.SWATTING},// AI_ACTION.PICKUP_ITEM
    
    	{ AnimationStates.WALKING,   AnimationStates.WALKING,  AnimationStates.WALKING}, // AI_ACTION.SCHEDULE_MOVE
    	{ AnimationStates.WALKING,   AnimationStates.WALKING,  AnimationStates.WALKING}, // AI_ACTION.WALK
    	{ AnimationStates.RUNNING,   AnimationStates.RUNNING,  AnimationStates.RUNNING}, // AI_ACTION.MOVE_TO_CLIMB
    };

    NOSHOOT OKToAttack(SOLDIERTYPE pSoldier, int target)
    {
        // can't shoot yourself
        if (target == pSoldier.sGridNo)
        {
            return (NOSHOOT.MYSELF);
        }

        if (WorldManager.WaterTooDeepForAttacks(pSoldier.sGridNo))
        {
            return (NOSHOOT.WATER);
        }

        // make sure a weapon is in hand (FEB.8 ADDITION: tossable items are also OK)
        if (!WeaponInHand(pSoldier))
        {
            return (NOSHOOT.NOWEAPON);
        }

        // JUST PUT THIS IN ON JULY 13 TO TRY AND FIX OUT-OF-AMMO SITUATIONS

        if (Item[pSoldier.inv[InventorySlot.HANDPOS].usItem].usItemClass == IC.GUN)
        {
            if (pSoldier.inv[InventorySlot.HANDPOS].usItem == Items.TANK_CANNON)
            {
                // look for another tank shell ELSEWHERE IN INVENTORY
                if (FindLaunchable(pSoldier, Items.TANK_CANNON) == NO_SLOT)
                //if ( !ItemHasAttachments( &(pSoldier.inv[HANDPOS]) ) )
                {
                    return (NOSHOOT.NOLOAD);
                }
            }
            else if (pSoldier.inv[InventorySlot.HANDPOS].ubGunShotsLeft == 0)
            {
                return (NOSHOOT.NOAMMO);
            }
        }
        else if (Item[pSoldier.inv[InventorySlot.HANDPOS].usItem].usItemClass == IC.LAUNCHER)
        {
            if (FindLaunchable(pSoldier, pSoldier.inv[InventorySlot.HANDPOS].usItem) == NO_SLOT)
            //if ( !ItemHasAttachments( &(pSoldier.inv[HANDPOS]) ) )
            {
                return (NOSHOOT.NOLOAD);
            }
        }

        return (NOSHOOT)(1);
    }

    public static bool ConsiderProne(SOLDIERTYPE pSoldier)
    {
        int sOpponentGridNo;
        int bOpponentLevel;
        int iRange;

        if (pSoldier.bAIMorale >= MORALE.NORMAL)
        {
            return (false);
        }
        // We don't want to go prone if there is a nearby enemy
        ClosestKnownOpponent(pSoldier, out sOpponentGridNo, out bOpponentLevel);
        iRange = GetRangeFromGridNoDiff(pSoldier.sGridNo, sOpponentGridNo);
        if (iRange > 10)
        {
            return (true);
        }
        else
        {
            return (false);
        }
    }

    public static AnimationHeights StanceChange(SOLDIERTYPE pSoldier, int ubAttackAPCost)
    {
        // consider crouching or going prone

        if (PTR_STANDING(pSoldier))
        {
            if (pSoldier.bActionPoints - ubAttackAPCost >= AP.CROUCH)
            {
                if ((pSoldier.bActionPoints - ubAttackAPCost >= AP.CROUCH + AP.PRONE) && SoldierControl.IsValidStance(pSoldier, AnimationHeights.ANIM_PRONE) && ConsiderProne(pSoldier))
                {
                    return (AnimationHeights.ANIM_PRONE);
                }
                else if (SoldierControl.IsValidStance(pSoldier, AnimationHeights.ANIM_CROUCH))
                {
                    return (AnimationHeights.ANIM_CROUCH);
                }
            }
        }
        else if (PTR_CROUCHED(pSoldier))
        {
            if ((pSoldier.bActionPoints - ubAttackAPCost >= AP.PRONE) && SoldierControl.IsValidStance(pSoldier, AnimationHeights.ANIM_PRONE) && ConsiderProne(pSoldier))
            {
                return (AnimationHeights.ANIM_PRONE);
            }
        }
        return (0);
    }

    public static AnimationHeights ShootingStanceChange(SOLDIERTYPE pSoldier, ATTACKTYPE pAttack, WorldDirections bDesiredDirection)
    {
        // Figure out the best stance for this attack

        // We don't want to go through a lot of complex calculations here,
        // just compare the chance of the bullet hitting if we are 
        // standing, crouched, or prone

        AnimationStates usRealAnimState;
        AnimationStates usBestAnimState;
        int bBestStanceDiff = -1;
        int bLoop, bStanceNum, bStanceDiff, bAPsAfterAttack;
        int uiChanceOfDamage, uiBestChanceOfDamage, uiCurrChanceOfDamage;
        int uiStanceBonus, uiMinimumStanceBonusPerChange = 20 - 3 * (int)pAttack.ubAimTime;
        int iRange;

        bStanceNum = 0;
        uiCurrChanceOfDamage = 0;

        bAPsAfterAttack = pSoldier.bActionPoints - pAttack.ubAPCost - GetAPsToReadyWeapon(pSoldier, pSoldier.usAnimState);
        if (bAPsAfterAttack < AP.CROUCH)
        {
            return (0);
        }
        // Unfortunately, to get this to work, we have to fake the AI guy's
        // animation state so we get the right height values
        usRealAnimState = pSoldier.usAnimState;
        usBestAnimState = pSoldier.usAnimState;
        uiBestChanceOfDamage = 0;
        iRange = GetRangeInCellCoordsFromGridNoDiff(pSoldier.sGridNo, pAttack.sTarget);

        switch (gAnimControl[usRealAnimState].ubEndHeight)
        {
            // set a stance number comparable with our loop variable so we can easily compute
            // stance differences and thus AP cost
            case AnimationHeights.ANIM_STAND:
                bStanceNum = 0;
                break;
            case AnimationHeights.ANIM_CROUCH:
                bStanceNum = 1;
                break;
            case AnimationHeights.ANIM_PRONE:
                bStanceNum = 2;
                break;
        }
        for (bLoop = 0; bLoop < 3; bLoop++)
        {
            bStanceDiff = Math.Abs(bLoop - bStanceNum);
            if (bStanceDiff == 2 && bAPsAfterAttack < AP.CROUCH + AP.PRONE)
            {
                // can't consider this!
                continue;
            }

            switch (bLoop)
            {
                case 0:
                    if (!SoldierControl.InternalIsValidStance(pSoldier, bDesiredDirection, AnimationHeights.ANIM_STAND))
                    {
                        continue;
                    }
                    pSoldier.usAnimState = AnimationStates.STANDING;
                    break;
                case 1:
                    if (!SoldierControl.InternalIsValidStance(pSoldier, bDesiredDirection, AnimationHeights.ANIM_CROUCH))
                    {
                        continue;
                    }
                    pSoldier.usAnimState = AnimationStates.CROUCHING;
                    break;
                default:
                    if (!SoldierControl.InternalIsValidStance(pSoldier, bDesiredDirection, AnimationHeights.ANIM_PRONE))
                    {
                        continue;
                    }
                    pSoldier.usAnimState = AnimationStates.PRONE;
                    break;
            }

            uiChanceOfDamage = SoldierToLocationChanceToGetThrough(pSoldier, pAttack.sTarget, pSoldier.bTargetLevel, pSoldier.bTargetCubeLevel, pAttack.ubOpponent) * CalcChanceToHitGun(pSoldier, pAttack.sTarget, pAttack.ubAimTime, AIM_SHOT_TORSO) / 100;
            if (uiChanceOfDamage > 0)
            {
                uiStanceBonus = 0;
                // artificially augment "chance of damage" to reflect penalty to be shot at various stances
                switch (pSoldier.usAnimState)
                {
                    case AnimationStates.CROUCHING:
                        if (iRange > POINT_BLANK_RANGE + 10 * (AIM_PENALTY_TARGET_CROUCHED / 3))
                        {
                            uiStanceBonus = AIM_BONUS_CROUCHING;
                        }
                        else if (iRange > POINT_BLANK_RANGE)
                        {
                            // reduce chance to hit with distance to the prone/immersed target
                            uiStanceBonus = 3 * ((iRange - POINT_BLANK_RANGE) / CELL_X_SIZE); // penalty -3%/tile
                        }
                        break;
                    case AnimationStates.PRONE:
                        if (iRange <= MIN_PRONE_RANGE)
                        {
                            // HATE being prone this close!
                            uiChanceOfDamage = 0;
                        }
                        else //if (iRange > POINT_BLANK_RANGE)
                        {
                            // reduce chance to hit with distance to the prone/immersed target
                            uiStanceBonus = 3 * ((iRange - POINT_BLANK_RANGE) / CELL_X_SIZE); // penalty -3%/tile
                        }
                        break;
                    default:
                        break;
                }
                // reduce stance bonus according to how much we have to change stance to get there
                //uiStanceBonus = uiStanceBonus * (4 - bStanceDiff) / 4;
                uiChanceOfDamage += uiStanceBonus;
            }

            if (bStanceDiff == 0)
            {
                uiCurrChanceOfDamage = uiChanceOfDamage;
            }
            if (uiChanceOfDamage > uiBestChanceOfDamage)
            {
                uiBestChanceOfDamage = uiChanceOfDamage;
                usBestAnimState = pSoldier.usAnimState;
                bBestStanceDiff = bStanceDiff;
            }
        }

        pSoldier.usAnimState = usRealAnimState;

        // return 0 or the best height value to be at
        if (bBestStanceDiff == 0 || ((uiBestChanceOfDamage - uiCurrChanceOfDamage) / bBestStanceDiff) < uiMinimumStanceBonusPerChange)
        {
            // better off not changing our stance!
            return (0);
        }
        else
        {
            return (gAnimControl[usBestAnimState].ubEndHeight);
        }
    }


    public static AnimationStates DetermineMovementMode(SOLDIERTYPE pSoldier, AI_ACTION bAction)
    {
        if (pSoldier.fUIMovementFast)
        {
            return (AnimationStates.RUNNING);
        }
        else if (CREATURE_OR_BLOODCAT(pSoldier))
        {
            if (pSoldier.bAlertStatus == STATUS.GREEN)
            {
                return (AnimationStates.WALKING);
            }
            else
            {
                return (AnimationStates.RUNNING);
            }
        }
        else if (pSoldier.ubBodyType == SoldierBodyTypes.COW || pSoldier.ubBodyType == SoldierBodyTypes.CROW)
        {
            return (AnimationStates.WALKING);
        }
        else
        {
            if ((pSoldier.fAIFlags.HasFlag(AIDEFINES.AI_CAUTIOUS)) && (MovementMode[bAction, Urgency[pSoldier.bAlertStatus][pSoldier.bAIMorale]] == AnimationStates.RUNNING))
            {
                return (AnimationStates.WALKING);
            }
            else if (bAction == AI_ACTION.SEEK_NOISE && pSoldier.bTeam == CIV_TEAM && !IS_MERC_BODY_TYPE(pSoldier))
            {
                return (AnimationStates.WALKING);
            }
            else if ((pSoldier.ubBodyType == SoldierBodyTypes.HATKIDCIV || pSoldier.ubBodyType == SoldierBodyTypes.KIDCIV) && (pSoldier.bAlertStatus == STATUS.GREEN) && Globals.Random.Next(10) == 0)
            {
                return (AnimationStates.KID_SKIPPING);
            }
            else
            {
                return (MovementMode[bAction][Urgency[pSoldier.bAlertStatus][pSoldier.bAIMorale]]);
            }
        }
    }

    public static void NewDest(SOLDIERTYPE pSoldier, int usGridNo)
    {
        // ATE: Setting sDestination? Tis does not make sense...
        //pSoldier.sDestination = usGridNo;
        bool fSet = false;

        if (IS_MERC_BODY_TYPE(pSoldier)
            && pSoldier.bAction == AI_ACTION.TAKE_COVER
            && (pSoldier.bOrders == (Orders)Attitudes.DEFENSIVE || pSoldier.bOrders == (Orders)Attitudes.CUNNINGSOLO || pSoldier.bOrders == (Orders)Attitudes.CUNNINGAID)
            && (SoldierDifficultyLevel(pSoldier) >= 2))
        {
            AnimationStates usMovementMode;

            // getting real movement anim for someone who is going to take cover, not just considering
            usMovementMode = MovementMode[AI_ACTION.TAKE_COVER][Urgency[pSoldier.bAlertStatus][pSoldier.bAIMorale]];
            if (usMovementMode != AnimationStates.SWATTING)
            {
                // really want to look at path, see how far we could get on path while swatting
                if (EnoughPoints(pSoldier, RecalculatePathCost(pSoldier, AnimationStates.SWATTING), 0, false) || (pSoldier.bLastAction == AI_ACTION.TAKE_COVER && pSoldier.usUIMovementMode == AnimationStates.SWATTING))
                {
                    pSoldier.usUIMovementMode = AnimationStates.SWATTING;
                }
                else
                {
                    pSoldier.usUIMovementMode = usMovementMode;
                }
            }
            else
            {
                pSoldier.usUIMovementMode = usMovementMode;
            }
            fSet = true;
        }
        else
        {
            if (pSoldier.bTeam == ENEMY_TEAM && pSoldier.bAlertStatus == STATUS.RED)
            {
                switch (pSoldier.bAction)
                {
                    /*
                    case AI_ACTION.MOVE_TO_CLIMB:
                    case AI_ACTION.RUN_AWAY:
                        pSoldier.usUIMovementMode = DetermineMovementMode( pSoldier, pSoldier.bAction );
                        fSet = true;
                        break;*/
                    default:
                        if (PreRandom(5 - SoldierDifficultyLevel(pSoldier)) == 0)
                        {
                            int sClosestNoise = (int)MostImportantNoiseHeard(pSoldier, null, null, null);
                            if (sClosestNoise != NOWHERE && IsometricUtils.PythSpacesAway(pSoldier.sGridNo, sClosestNoise) < MaxDistanceVisible() + 10)
                            {
                                pSoldier.usUIMovementMode = AnimationStates.SWATTING;
                                fSet = true;
                            }
                        }
                        if (!fSet)
                        {
                            pSoldier.usUIMovementMode = DetermineMovementMode(pSoldier, pSoldier.bAction);
                            fSet = true;
                        }
                        break;
                }

            }
            else
            {
                pSoldier.usUIMovementMode = DetermineMovementMode(pSoldier, pSoldier.bAction);
                fSet = true;
            }

            if (pSoldier.usUIMovementMode == AnimationStates.SWATTING && !IS_MERC_BODY_TYPE(pSoldier))
            {
                pSoldier.usUIMovementMode = AnimationStates.WALKING;
            }
        }

        //EVENT_GetNewSoldierPath( pSoldier, pSoldier.sDestination, pSoldier.usUIMovementMode );
        // ATE: Using this more versitile version
        // Last paramater says whether to re-start the soldier's animation
        // This should be done if buddy was paused for fNoApstofinishMove...
        SoldierControl.EVENT_InternalGetNewSoldierPath(pSoldier, usGridNo, pSoldier.usUIMovementMode, 0, pSoldier.fNoAPToFinishMove);
    }


    public static bool IsActionAffordable(SOLDIERTYPE pSoldier)
    {
        int bMinPointsNeeded = 0;

        //NumMessage("AffordableAction - Guy#",pSoldier.ubID);

        switch (pSoldier.bAction)
        {
            case AI_ACTION.NONE:                  // maintain current position & facing
                                                  // no cost for doing nothing!
                break;

            case AI_ACTION.CHANGE_FACING:         // turn to face another direction
                bMinPointsNeeded = (int)GetAPsToLook(pSoldier);
                break;

            case AI_ACTION.RANDOM_PATROL:         // move towards a particular location
            case AI_ACTION.SEEK_FRIEND:           // move towards friend in trouble
            case AI_ACTION.SEEK_OPPONENT:         // move towards a reported opponent
            case AI_ACTION.TAKE_COVER:            // run for nearest cover from threat
            case AI_ACTION.GET_CLOSER:            // move closer to a strategic location
            case AI_ACTION.POINT_PATROL:          // move towards next patrol point
            case AI_ACTION.LEAVE_WATER_GAS:       // seek nearest spot of ungassed land
            case AI_ACTION.SEEK_NOISE:            // seek most important noise heard
            case AI_ACTION.ESCORTED_MOVE:         // go where told to by escortPlayer
            case AI_ACTION.RUN_AWAY:              // run away from nearby opponent(s)
            case AI_ACTION.APPROACH_MERC:
            case AI_ACTION.TRACK:
            case AI_ACTION.EAT:
            case AI_ACTION.SCHEDULE_MOVE:
            case AI_ACTION.WALK:
            case AI_ACTION.MOVE_TO_CLIMB:
                // for movement, must have enough APs to move at least 1 tile's worth
                bMinPointsNeeded = MinPtsToMove(pSoldier);
                break;

            case AI_ACTION.PICKUP_ITEM:           // grab things lying on the ground
                bMinPointsNeeded = Math.Max(MinPtsToMove(pSoldier), AP.PICKUP_ITEM);
                break;

            case AI_ACTION.OPEN_OR_CLOSE_DOOR:
            case AI_ACTION.UNLOCK_DOOR:
            case AI_ACTION.LOCK_DOOR:
                bMinPointsNeeded = MinPtsToMove(pSoldier);
                break;

            case AI_ACTION.DROP_ITEM:
                bMinPointsNeeded = AP.PICKUP_ITEM;
                break;

            case AI_ACTION.FIRE_GUN:              // shoot at nearby opponent
            case AI_ACTION.TOSS_PROJECTILE:       // throw grenade at/near opponent(s)
            case AI_ACTION.KNIFE_MOVE:            // preparing to stab adjacent opponent
            case AI_ACTION.THROW_KNIFE:
                // only FIRE_GUN currently actually pays extra turning costs!
                bMinPointsNeeded = MinAPsToAttack(pSoldier, pSoldier.usActionData, ADDTURNCOST);
                break;

            case AI_ACTION.PULL_TRIGGER:          // activate an adjacent panic trigger
                bMinPointsNeeded = AP.PULL_TRIGGER;
                break;

            case AI_ACTION.USE_DETONATOR:         // grab detonator and set off bomb(s)
                bMinPointsNeeded = AP.USE_REMOTE;
                break;

            case AI_ACTION.YELLOW_ALERT:          // tell friends opponent(s) heard
            case AI_ACTION.RED_ALERT:             // tell friends opponent(s) seen
            case AI_ACTION.CREATURE_CALL:                // for now
                bMinPointsNeeded = AP.RADIO;
                break;

            case AI_ACTION.CHANGE_STANCE:                // crouch
                bMinPointsNeeded = AP.CROUCH;
                break;

            case AI_ACTION.GIVE_AID:              // help injured/dying friend
                bMinPointsNeeded = 0;
                break;

            case AI_ACTION.CLIMB_ROOF:
                if (pSoldier.bLevel == 0)
                {
                    bMinPointsNeeded = AP.CLIMBROOF;
                }
                else
                {
                    bMinPointsNeeded = AP.CLIMBOFFROOF;
                }
                break;

            case AI_ACTION.COWER:
            case AI_ACTION.STOP_COWERING:
            case AI_ACTION.LOWER_GUN:
            case AI_ACTION.END_COWER_AND_MOVE:
            case AI_ACTION.TRAVERSE_DOWN:
            case AI_ACTION.OFFER_SURRENDER:
                bMinPointsNeeded = 0;
                break;

            default:
                break;
        }

        // check whether or not we can afford to do this action
        if (bMinPointsNeeded > pSoldier.bActionPoints)
        {
            return (false);
        }
        else
        {
            return (true);
        }
    }

    int RandomFriendWithin(SOLDIERTYPE pSoldier)
    {
        int uiLoop;
        int usMaxDist;
        int ubFriendCount, ubFriendID;
        int[] ubFriendIDs = new int[MAXMERCS];
        int usDirection;
        int ubDirsLeft;
        bool[] fDirChecked = new bool[8];
        bool fRangeRestricted = false, fFound = false;
        int usDest, usOrigin;
        SOLDIERTYPE pFriend;


        // obtain maximum roaming distance from soldier's origin
        usMaxDist = RoamingRange(pSoldier, out usOrigin);

        // if our movement range is restricted

        // CJC: since RandomFriendWithin is only used in non-combat, ALWAYS restrict range.
        fRangeRestricted = true;
        /*
        if (usMaxDist < MAX_ROAMING_RANGE)
        {
            fRangeRestricted = true;
        }
        */

        // if range is restricted, make sure origin is a legal gridno!
        if (fRangeRestricted && ((usOrigin < 0) || (usOrigin >= GRIDSIZE)))
        {
#if BETAVERSION
        NameMessage(pSoldier, "has illegal origin, but his roaming range is restricted!", 1000);
#endif
            return (0);
        }

        ubFriendCount = 0;

        // build a list of the guynums of all active, eligible friendly mercs

        // go through each soldier, looking for "friends" (soldiers on same side)
        for (uiLoop = 0; uiLoop < guiNumMercSlots; uiLoop++)
        {
            pFriend = MercSlots[uiLoop];

            // if this merc is inactive, not in sector, or dead
            if (!pFriend)
            {
                continue;
            }

            // skip ourselves
            if (pFriend.ubID == pSoldier.ubID)
            {
                continue;
            }

            // if this man not neutral, but is on my side, OR if he is neutral, but
            // so am I, then he's a "friend" for the purposes of random visitations
            if ((!pFriend.IsNeutral && (pSoldier.bSide == pFriend.bSide)) ||
                (pFriend.IsNeutral && pSoldier.IsNeutral))
            {
                // if we're not already neighbors
                if (IsometricUtils.SpacesAway(pSoldier.sGridNo, pFriend.sGridNo) > 1)
                {
                    // remember his guynum, increment friend counter
                    ubFriendIDs[ubFriendCount++] = pFriend.ubID;
                }
            }
        }


        while (ubFriendCount > 0 && !fFound)
        {
            // randomly select one of the remaining friends in the list
            ubFriendID = ubFriendIDs[PreRandom(ubFriendCount)];

            // if our movement range is NOT restricted, or this friend's within range
            // use distance - 1, because there must be at least 1 tile 1 space closer
            if (!fRangeRestricted ||
                (IsometricUtils.SpacesAway(usOrigin, Menptr[ubFriendID].sGridNo) - 1) <= usMaxDist)
            {
                // should be close enough, try to find a legal .sDestination within 1 tile

                // clear dirChecked flag for all 8 directions
                for (usDirection = 0; usDirection < 8; usDirection++)
                {
                    fDirChecked[usDirection] = false;
                }

                ubDirsLeft = 8;

                // examine all 8 spots around 'ubFriendID'
                // keep looking while directions remain and a satisfactory one not found
                while ((ubDirsLeft--) > 0 && !fFound)
                {
                    // randomly select a direction which hasn't been 'checked' yet
                    do
                    {
                        usDirection = Globals.Random.Next(8);
                    }
                    while (fDirChecked[usDirection]);

                    fDirChecked[usDirection] = true;

                    // determine the gridno 1 tile away from current friend in this direction
                    usDest = IsometricUtils.NewGridNo(Menptr[ubFriendID].sGridNo, IsometricUtils.DirectionInc((int)(usDirection + 1)));

                    // if that's out of bounds, ignore it & check next direction
                    if (usDest == Menptr[ubFriendID].sGridNo)
                    {
                        continue;
                    }

                    // if our movement range is NOT restricted
                    if (!fRangeRestricted || (IsometricUtils.SpacesAway(usOrigin, usDest) <= usMaxDist))
                    {
                        if (Movement.LegalNPCDestination(pSoldier, usDest, ENSURE_PATH, NOWATER, 0))
                        {
                            fFound = true;            // found a spot
                            pSoldier.usActionData = usDest;  // store this .sDestination
                            pSoldier.bPathStored = true;  // optimization - Ian
                            break;                   // stop checking in other directions
                        }
                    }
                }
            }

            if (!fFound)
            {
                ubFriendCount--;

                // if we hadn't already picked the last friend currently in the list
                if (ubFriendCount != ubFriendID)
                {
                    ubFriendIDs[ubFriendID] = ubFriendIDs[ubFriendCount];
                }
            }
        }

        return (fFound);
    }


    int RandDestWithinRange(SOLDIERTYPE pSoldier)
    {
        int sRandDest = NOWHERE;
        int usOrigin, usMaxDist;
        int ubTriesLeft;
        bool fLimited = false, fFound = false;
        int sMaxLeft, sMaxRight, sMaxUp, sMaxDown, sXRange, sYRange, sXOffset, sYOffset;
        int sOrigX, sOrigY;
        int sX, sY;
        int ubRoom = 0, ubTempRoom;

        sOrigX = sOrigY = -1;
        sMaxLeft = sMaxRight = sMaxUp = sMaxDown = sXRange = sYRange = -1;

        // Try to find a random .sDestination that's no more than maxDist away from
        // the given gridno of origin

        if (gfTurnBasedAI)
        {
            ubTriesLeft = 10;
        }
        else
        {
            ubTriesLeft = 1;
        }

        usMaxDist = RoamingRange(pSoldier, out usOrigin);

        if (pSoldier.bOrders <= Orders.CLOSEPATROL && (pSoldier.bTeam == CIV_TEAM || pSoldier.ubProfile != NO_PROFILE))
        {
            // any other combo uses the default of ubRoom == 0, set above
            if (!RenderFun.InARoom(pSoldier.usPatrolGrid[0], out ubRoom))
            {
                ubRoom = 0;
            }
        }

        // if the maxDist is truly a restriction
        if (usMaxDist < (MAXCOL - 1))
        {
            fLimited = true;

            // determine maximum horizontal limits
            sOrigX = usOrigin % MAXCOL;
            sOrigY = usOrigin / MAXCOL;

            sMaxLeft = Math.Min(usMaxDist, sOrigX);
            sMaxRight = Math.Min(usMaxDist, MAXCOL - (sOrigX + 1));

            // determine maximum vertical limits
            sMaxUp = Math.Min(usMaxDist, sOrigY);
            sMaxDown = Math.Min(usMaxDist, MAXROW - (sOrigY + 1));

            sXRange = sMaxLeft + sMaxRight + 1;
            sYRange = sMaxUp + sMaxDown + 1;
        }

        if (pSoldier.ubBodyType == SoldierBodyTypes.LARVAE_MONSTER)
        {
            // only crawl 1 tile, within our roaming range
            while ((ubTriesLeft--) > 0 && !fFound)
            {
                sXOffset = (int)Globals.Random.Next(3) - 1; // generates -1 to +1
                sYOffset = (int)Globals.Random.Next(3) - 1;

                if (fLimited)
                {
                    sX = pSoldier.sGridNo % MAXCOL + sXOffset;
                    sY = pSoldier.sGridNo / MAXCOL + sYOffset;
                    if (sX < sOrigX - sMaxLeft || sX > sOrigX + sMaxRight)
                    {
                        continue;
                    }
                    if (sY < sOrigY - sMaxUp || sY > sOrigY + sMaxDown)
                    {
                        continue;
                    }
                    sRandDest = usOrigin + sXOffset + (MAXCOL * sYOffset);
                }
                else
                {
                    sRandDest = usOrigin + sXOffset + (MAXCOL * sYOffset);
                }

                if (Movement.LegalNPCDestination(pSoldier, sRandDest, ENSURE_PATH, NOWATER, 0) == 0)
                {
                    sRandDest = NOWHERE;
                    continue;                   // try again!
                }

                // passed all the tests, .sDestination is acceptable
                fFound = true;
                pSoldier.bPathStored = true;   // optimization - Ian
            }
        }
        else
        {
            // keep rolling random .sDestinations until one's satisfactory or retries used
            while ((ubTriesLeft--) > 0 && !fFound)
            {
                if (fLimited)
                {
                    sXOffset = ((int)Globals.Random.Next(sXRange)) - sMaxLeft;
                    sYOffset = ((int)Globals.Random.Next(sYRange)) - sMaxUp;

                    sRandDest = usOrigin + sXOffset + (MAXCOL * sYOffset);

#if BETAVERSION
                if ((sRandDest < 0) || (sRandDest >= GRIDSIZE))
                {
                    NumMessage("RandDestWithinRange: ERROR - Gridno out of range! = ", sRandDest);
                    sRandDest = random(GRIDSIZE);
                }
#endif
                }
                else
                {
                    sRandDest = (int)PreRandom(GRIDSIZE);
                }

                if (ubRoom > 0 && RenderFun.InARoom(sRandDest, out ubTempRoom) && ubTempRoom != ubRoom)
                {
                    // outside of room available for patrol!
                    sRandDest = NOWHERE;
                    continue;
                }

                if (Movement.LegalNPCDestination(pSoldier, sRandDest, ENSURE_PATH, NOWATER, 0) == 0)
                {
                    sRandDest = NOWHERE;
                    continue;                   // try again!
                }

                // passed all the tests, .sDestination is acceptable
                fFound = true;
                pSoldier.bPathStored = true;   // optimization - Ian
            }
        }

        return (sRandDest); // defaults to NOWHERE
    }

    int ClosestReachableDisturbance(SOLDIERTYPE pSoldier, int ubUnconsciousOK, out bool pfChangeLevel)
    {
        int psLastLoc, pusNoiseGridNo;
        int pbLastLevel;
        int sGridNo = -1;
        int bLevel, bClosestLevel;
        bool fClimbingNecessary, fClosestClimbingNecessary = false;
        int iPathCost;
        int sClosestDisturbance = NOWHERE;
        int uiLoop;
        int closestConscious = NOWHERE, closestUnconscious = NOWHERE;
        int iShortestPath = 1000;
        int iShortestPathConscious = 1000, iShortestPathUnconscious = 1000;
        int pubNoiseVolume;
        int pbNoiseLevel;
        int pbPersOL, pbPublOL;
        int sClimbGridNo;
        SOLDIERTYPE pOpp;

        // CJC: can't trace a path to every known disturbance!
        // for starters, try the closest one as the crow flies
        int sClosestEnemy = NOWHERE, sDistToClosestEnemy = 1000, sDistToEnemy;

        pfChangeLevel = false;

        pubNoiseVolume = gubPublicNoiseVolume[pSoldier.bTeam];
        pusNoiseGridNo = gsPublicNoiseGridno[pSoldier.bTeam];
        pbNoiseLevel = gbPublicNoiseLevel[pSoldier.bTeam];

        // hang pointers at start of this guy's personal and public opponent opplists
        //	pbPersOL = &pSoldier.bOppList[0];
        //	pbPublOL = &(gbPublicOpplist[pSoldier.bTeam][0]);
        //	psLastLoc = &(gsLastKnownOppLoc[pSoldier.ubID][0]);

        // look through this man's personal & public opplists for opponents known
        for (uiLoop = 0; uiLoop < guiNumMercSlots; uiLoop++)
        {
            pOpp = MercSlots[uiLoop];

            // if this merc is inactive, at base, on assignment, or dead
            if (!pOpp)
            {
                continue;          // next merc
            }

            // if this merc is neutral/on same side, he's not an opponent
            if (CONSIDERED_NEUTRAL(pSoldier, pOpp) || (pSoldier.bSide == pOpp.bSide))
            {
                continue;          // next merc
            }

            pbPersOL = pSoldier.bOppList + pOpp.ubID;
            pbPublOL = gbPublicOpplist[pSoldier.bTeam] + pOpp.ubID;
            psLastLoc = gsLastKnownOppLoc[pSoldier.ubID] + pOpp.ubID;
            pbLastLevel = gbLastKnownOppLevel[pSoldier.ubID] + pOpp.ubID;

            // if this opponent is unknown personally and publicly
            if ((pbPersOL == NOT_HEARD_OR_SEEN) && (pbPublOL == NOT_HEARD_OR_SEEN))
            {
                continue;          // next merc
            }

            // this is possible if get here from BLACK AI in one of those rare
            // instances when we couldn't get a meaningful shot off at a guy in sight
            if ((pbPersOL == SEEN_CURRENTLY) && (pOpp.bLife >= OKLIFE))
            {
                // don't allow this to return any valid values, this guy remains a
                // serious threat and the last thing we want to do is approach him!
                return (NOWHERE);
            }

            // if personal knowledge is more up to date or at least equal
            if ((gubKnowledgeValue[pbPublOL - OLDEST_HEARD_VALUE, pbPersOL - OLDEST_HEARD_VALUE] > 0) ||
                 (pbPersOL == pbPublOL))
            {
                // using personal knowledge, obtain opponent's "best guess" gridno
                sGridNo = psLastLoc;
                bLevel = pbLastLevel;
            }
            else
            {
                // using public knowledge, obtain opponent's "best guess" gridno
                sGridNo = gsPublicLastKnownOppLoc[pSoldier.bTeam][pOpp.ubID];
                bLevel = gbPublicLastKnownOppLevel[pSoldier.bTeam][pOpp.ubID];
            }

            // if we are standing at that gridno (!, obviously our info is old...)
            if (sGridNo == pSoldier.sGridNo)
            {
                continue;          // next merc
            }

            if (sGridNo == NOWHERE)
            {
                // huh?
                continue;
            }

            sDistToEnemy = IsometricUtils.PythSpacesAway(pSoldier.sGridNo, sGridNo);
            if (sDistToEnemy < sDistToClosestEnemy)
            {
                ;
            }

            {
                sClosestEnemy = sGridNo;
                bClosestLevel = bLevel;
                sDistToClosestEnemy = sDistToEnemy;
            }

        }

        if (sClosestEnemy != NOWHERE)
        {
            iPathCost = EstimatePathCostToLocation(pSoldier, sClosestEnemy, bClosestLevel, false, out fClimbingNecessary, out sClimbGridNo);
            // if we can get there
            if (iPathCost != 0)
            {
                if (fClimbingNecessary)
                {
                    sClosestDisturbance = sClimbGridNo;
                }
                else
                {
                    sClosestDisturbance = sClosestEnemy;
                }
                iShortestPath = iPathCost;
                fClosestClimbingNecessary = fClimbingNecessary;
            }
        }

        // if any "misc. noise" was also heard recently
        if (pSoldier.sNoiseGridno != NOWHERE && pSoldier.sNoiseGridno != sClosestDisturbance)
        {
            // test this gridno, too
            sGridNo = pSoldier.sNoiseGridno;
            bLevel = pSoldier.bNoiseLevel;

            // if we are there (at the noise gridno)
            if (sGridNo == pSoldier.sGridNo)
            {
                pSoldier.sNoiseGridno = NOWHERE;        // wipe it out, not useful anymore
                pSoldier.ubNoiseVolume = 0;
            }
            else
            {
                // get the AP cost to get to the location of the noise
                iPathCost = EstimatePathCostToLocation(pSoldier, sGridNo, bLevel, false, out fClimbingNecessary, out sClimbGridNo);
                // if we can get there
                if (iPathCost != 0)
                {
                    if (fClimbingNecessary)
                    {
                        sClosestDisturbance = sClimbGridNo;
                    }
                    else
                    {
                        sClosestDisturbance = sGridNo;
                    }
                    iShortestPath = iPathCost;
                    fClosestClimbingNecessary = fClimbingNecessary;
                }
            }
        }


        // if any PUBLIC "misc. noise" was also heard recently
        if (pusNoiseGridNo != NOWHERE && pusNoiseGridNo != sClosestDisturbance)
        {
            // test this gridno, too
            sGridNo = pusNoiseGridNo;
            bLevel = pbNoiseLevel;

            // if we are not NEAR the noise gridno...
            if (pSoldier.bLevel != bLevel || IsometricUtils.PythSpacesAway(pSoldier.sGridNo, sGridNo) >= 6 || SoldierTo3DLocationLineOfSightTest(pSoldier, sGridNo, bLevel, 0, (int)MaxDistanceVisible(), false) == 0)
            // if we are NOT there (at the noise gridno)
            //	if (sGridNo != pSoldier.sGridNo)
            {
                // get the AP cost to get to the location of the noise
                iPathCost = EstimatePathCostToLocation(pSoldier, sGridNo, bLevel, false, out fClimbingNecessary, out sClimbGridNo);
                // if we can get there
                if (iPathCost != 0)
                {
                    if (fClimbingNecessary)
                    {
                        sClosestDisturbance = sClimbGridNo;
                    }
                    else
                    {
                        sClosestDisturbance = sGridNo;
                    }
                    iShortestPath = iPathCost;
                    fClosestClimbingNecessary = fClimbingNecessary;
                }
            }
            else
            {
                // degrade our public noise a bit
                pusNoiseGridNo -= 2;
            }
        }

#if DEBUGDECISIONS
    if (sClosestDisturbance != NOWHERE)
    {
        AINumMessage("CLOSEST DISTURBANCE is at gridno ", sClosestDisturbance);
    }
#endif

        pfChangeLevel = fClosestClimbingNecessary;
        return (sClosestDisturbance);
    }


    public static int ClosestKnownOpponent(SOLDIERTYPE pSoldier, out int psGridNo, out int pbLevel)
    {
        int psLastLoc, sGridNo, sClosestOpponent = NOWHERE;
        int uiLoop;
        int iRange, iClosestRange = 1500;
        int pbPersOL, pbPublOL;
        int bLevel, bClosestLevel;
        SOLDIERTYPE pOpp;

        bClosestLevel = -1;


        // NOTE: THIS FUNCTION ALLOWS RETURN OF UNCONSCIOUS AND UNREACHABLE OPPONENTS
        psLastLoc = (gsLastKnownOppLoc[pSoldier.ubID, 0]);

        // hang pointers at start of this guy's personal and public opponent opplists
        pbPersOL = pSoldier.bOppList[0];
        pbPublOL = (gbPublicOpplist[pSoldier.bTeam][0]);

        // look through this man's personal & public opplists for opponents known
        for (uiLoop = 0; uiLoop < guiNumMercSlots; uiLoop++)
        {
            pOpp = MercSlots[uiLoop];

            // if this merc is inactive, at base, on assignment, or dead
            if (!pOpp)
            {
                continue;          // next merc
            }

            // if this merc is neutral/on same side, he's not an opponent
            if (CONSIDERED_NEUTRAL(pSoldier, pOpp) || (pSoldier.bSide == pOpp.bSide))
            {
                continue;          // next merc
            }

            // Special stuff for Carmen the bounty hunter
            if (pSoldier.bAttitude == Attitudes.ATTACKSLAYONLY && pOpp.ubProfile != (NPCID.SLAY))
            {
                continue;  // next opponent
            }

            pbPersOL = pSoldier.bOppList + pOpp.ubID;
            pbPublOL = gbPublicOpplist[pSoldier.bTeam] + pOpp.ubID;
            psLastLoc = gsLastKnownOppLoc[pSoldier.ubID] + pOpp.ubID;

            // if this opponent is unknown personally and publicly
            if ((pbPersOL == NOT_HEARD_OR_SEEN) && (pbPublOL == NOT_HEARD_OR_SEEN))
            {
                continue;          // next merc
            }

            // if personal knowledge is more up to date or at least equal
            if ((gubKnowledgeValue[pbPublOL - OLDEST_HEARD_VALUE, pbPersOL - OLDEST_HEARD_VALUE] > 0) ||
                 (pbPersOL == pbPublOL))
            {
                // using personal knowledge, obtain opponent's "best guess" gridno
                sGridNo = gsLastKnownOppLoc[pSoldier.ubID, pOpp.ubID];
                bLevel = gbLastKnownOppLevel[pSoldier.ubID, pOpp.ubID];
            }
            else
            {
                // using public knowledge, obtain opponent's "best guess" gridno
                sGridNo = gsPublicLastKnownOppLoc[pSoldier.bTeam][pOpp.ubID];
                bLevel = gbPublicLastKnownOppLevel[pSoldier.bTeam][pOpp.ubID];
            }

            // if we are standing at that gridno(!, obviously our info is old...)
            if (sGridNo == pSoldier.sGridNo)
            {
                continue;          // next merc
            }

            // this function is used only for turning towards closest opponent or changing stance
            // as such, if they AI is in a building, 
            // we should ignore people who are on the roof of the same building as the AI
            if ((bLevel != pSoldier.bLevel) && SameBuilding(pSoldier.sGridNo, sGridNo))
            {
                continue;
            }

            // I hope this will be good enough; otherwise we need a fractional/world-units-based 2D distance function
            //sRange = PythSpacesAway( pSoldier.sGridNo, sGridNo);
            iRange = GetRangeInCellCoordsFromGridNoDiff(pSoldier.sGridNo, sGridNo);

            if (iRange < iClosestRange)
            {
                iClosestRange = iRange;
                sClosestOpponent = sGridNo;
                bClosestLevel = bLevel;
            }
        }

#if DEBUGDECISIONS
    if (sClosestOpponent != NOWHERE)
    {
        AINumMessage("CLOSEST OPPONENT is at gridno ", sClosestOpponent);
    }
#endif

        if (psGridNo)
        {
            psGridNo = sClosestOpponent;
        }
        if (pbLevel)
        {
            pbLevel = bClosestLevel;
        }

        return (sClosestOpponent);
    }

    int ClosestSeenOpponent(SOLDIERTYPE pSoldier, out int psGridNo, out int pbLevel)
    {
        int sGridNo, sClosestOpponent = NOWHERE;
        int uiLoop;
        int iRange, iClosestRange = 1500;
        int? pbPersOL;
        int bLevel, bClosestLevel;
        SOLDIERTYPE pOpp;

        bClosestLevel = -1;

        // look through this man's personal & public opplists for opponents known
        for (uiLoop = 0; uiLoop < guiNumMercSlots; uiLoop++)
        {
            pOpp = MercSlots[uiLoop];

            // if this merc is inactive, at base, on assignment, or dead
            if (!pOpp)
            {
                continue;          // next merc
            }

            // if this merc is neutral/on same side, he's not an opponent
            if (CONSIDERED_NEUTRAL(pSoldier, pOpp) || (pSoldier.bSide == pOpp.bSide))
            {
                continue;          // next merc
            }

            // Special stuff for Carmen the bounty hunter
            if (pSoldier.bAttitude == ATTACKSLAYONLY && pOpp.ubProfile != NPCID.SLAY)
            {
                continue;  // next opponent
            }

            pbPersOL = pSoldier.bOppList + pOpp.ubID;

            // if this opponent is not seen personally
            if (pbPersOL != SEEN_CURRENTLY)
            {
                continue;          // next merc
            }

            // since we're dealing with seen people, use exact gridnos
            sGridNo = pOpp.sGridNo;
            bLevel = pOpp.bLevel;

            // if we are standing at that gridno(!, obviously our info is old...)
            if (sGridNo == pSoldier.sGridNo)
            {
                continue;          // next merc
            }

            // this function is used only for turning towards closest opponent or changing stance
            // as such, if they AI is in a building, 
            // we should ignore people who are on the roof of the same building as the AI
            if ((bLevel != pSoldier.bLevel) && SameBuilding(pSoldier.sGridNo, sGridNo))
            {
                continue;
            }

            // I hope this will be good enough; otherwise we need a fractional/world-units-based 2D distance function
            //sRange = PythSpacesAway( pSoldier.sGridNo, sGridNo);
            iRange = GetRangeInCellCoordsFromGridNoDiff(pSoldier.sGridNo, sGridNo);

            if (iRange < iClosestRange)
            {
                iClosestRange = iRange;
                sClosestOpponent = sGridNo;
                bClosestLevel = bLevel;
            }
        }

#if DEBUGDECISIONS
    if (sClosestOpponent != NOWHERE)
    {
        AINumMessage("CLOSEST OPPONENT is at gridno ", sClosestOpponent);
    }
#endif

        if (psGridNo)
        {
            psGridNo = sClosestOpponent;
        }
        if (pbLevel)
        {
            pbLevel = bClosestLevel;
        }
        return (sClosestOpponent);
    }


    int ClosestPC(SOLDIERTYPE pSoldier, out int? psDistance)
    {
        // used by NPCs... find the closest PC

        // NOTE: skips EPCs!

        int ubLoop;
        SOLDIERTYPE pTargetSoldier;
        int sMinDist = (int)WORLD_MAX;
        int sDist;
        int sGridNo = NOWHERE;

        // Loop through all mercs on player team
        ubLoop = gTacticalStatus.Team[gbPlayerNum].bFirstID;

        for (; ubLoop <= gTacticalStatus.Team[gbPlayerNum].bLastID; ubLoop++)
        {
            pTargetSoldier = Menptr[ubLoop];

            if (!pTargetSoldier.bActive || !pTargetSoldier.bInSector)
            {
                continue;
            }

            // if not conscious, skip him
            if (pTargetSoldier.bLife < OKLIFE)
            {
                continue;
            }

            if (AM_AN_EPC(pTargetSoldier))
            {
                continue;
            }

            sDist = IsometricUtils.PythSpacesAway(pSoldier.sGridNo, pTargetSoldier.sGridNo);

            // if this PC is not visible to the soldier, then add a penalty to the distance
            // so that we weight in favour of visible mercs
            if (pTargetSoldier.bTeam != pSoldier.bTeam && pSoldier.bOppList[ubLoop] != SEEN_CURRENTLY)
            {
                sDist += 10;
            }

            if (sDist < sMinDist)
            {
                sMinDist = sDist;
                sGridNo = pTargetSoldier.sGridNo;
            }
        }

        if (psDistance is not null)
        {
            psDistance = sMinDist;
        }

        return (sGridNo);
    }

    public static int FindClosestClimbPointAvailableToAI(SOLDIERTYPE pSoldier, int sStartGridNo, int sDesiredGridNo, bool fClimbUp)
    {
        int sGridNo;
        int sRoamingOrigin;
        int sRoamingRange;

        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.PC))
        {
            sRoamingOrigin = pSoldier.sGridNo;
            sRoamingRange = 99;
        }
        else
        {
            sRoamingRange = RoamingRange(pSoldier, out sRoamingOrigin);
        }

        // since climbing necessary involves going an extra tile, we compare against 1 less than the roam range... 
        // or add 1 to the distance to the climb point

        sGridNo = FindClosestClimbPoint(sStartGridNo, sDesiredGridNo, fClimbUp);


        if (IsometricUtils.PythSpacesAway(sRoamingOrigin, sGridNo) + 1 > sRoamingRange)
        {
            return (NOWHERE);
        }
        else
        {
            return (sGridNo);
        }
    }

    bool ClimbingNecessary(SOLDIERTYPE pSoldier, int sDestGridNo, int bDestLevel)
    {
        if (pSoldier.bLevel == bDestLevel)
        {
            if ((pSoldier.bLevel == 0) || (gubBuildingInfo[pSoldier.sGridNo] == gubBuildingInfo[sDestGridNo]))
            {
                return (false);
            }
            else // different buildings!
            {
                return (true);
            }
        }
        else
        {
            return (true);
        }
    }

    public static int GetInterveningClimbingLocation(SOLDIERTYPE pSoldier, int sDestGridNo, int bDestLevel, out bool pfClimbingNecessary)
    {
        if (pSoldier.bLevel == bDestLevel)
        {
            if ((pSoldier.bLevel == 0) || (gubBuildingInfo[pSoldier.sGridNo] == gubBuildingInfo[sDestGridNo]))
            {
                // on ground or same building... normal!
                pfClimbingNecessary = false;
                return (NOWHERE);
            }
            else
            {
                // different buildings!
                // yes, pass in same gridno twice... want closest climb-down spot for building we are on!
                pfClimbingNecessary = true;
                return (FindClosestClimbPointAvailableToAI(pSoldier, pSoldier.sGridNo, pSoldier.sGridNo, false));
            }
        }
        else
        {
            pfClimbingNecessary = true;
            // different levels
            if (pSoldier.bLevel == 0)
            {
                // got to go UP onto building
                return (FindClosestClimbPointAvailableToAI(pSoldier, pSoldier.sGridNo, sDestGridNo, true));
            }
            else
            {
                // got to go DOWN off building
                return (FindClosestClimbPointAvailableToAI(pSoldier, pSoldier.sGridNo, pSoldier.sGridNo, false));
            }
        }
    }

    int EstimatePathCostToLocation(SOLDIERTYPE pSoldier, int sDestGridNo, int bDestLevel, bool fAddCostAfterClimbingUp, out bool pfClimbingNecessary, out int psClimbGridNo)
    {
        pfClimbingNecessary = false;
        psClimbGridNo = -1;

        int sPathCost;
        int sClimbGridNo;

        if (pSoldier.bLevel == bDestLevel)
        {
            if ((pSoldier.bLevel == 0) || (gubBuildingInfo[pSoldier.sGridNo] == gubBuildingInfo[sDestGridNo]))
            {
                // on ground or same building... normal!
                sPathCost = EstimatePlotPath(pSoldier, sDestGridNo, false, false, false, AnimationStates.WALKING, false, false, 0);
                pfClimbingNecessary = false;
                psClimbGridNo = NOWHERE;
            }
            else
            {
                // different buildings!
                // yes, pass in same gridno twice... want closest climb-down spot for building we are on!
                sClimbGridNo = FindClosestClimbPointAvailableToAI(pSoldier, pSoldier.sGridNo, pSoldier.sGridNo, false);
                if (sClimbGridNo == NOWHERE)
                {
                    sPathCost = 0;
                }
                else
                {
                    sPathCost = PathAI.PlotPath(pSoldier, sClimbGridNo, null, false, null, AnimationStates.WALKING, null, null, 0);
                    if (sPathCost != 0)
                    {
                        // add in cost of climbing down
                        if (fAddCostAfterClimbingUp)
                        {
                            // add in cost of later climbing up, too
                            sPathCost += AP.CLIMBOFFROOF + AP.CLIMBROOF;
                            // add in an estimate of getting there after climbing down
                            sPathCost += (AP.MOVEMENT_FLAT + WALKCOST) * IsometricUtils.PythSpacesAway(sClimbGridNo, sDestGridNo);
                        }
                        else
                        {
                            sPathCost += AP.CLIMBOFFROOF;
                            // add in an estimate of getting there after climbing down, *but not on top of roof*
                            sPathCost += (AP.MOVEMENT_FLAT + WALKCOST) * IsometricUtils.PythSpacesAway(sClimbGridNo, sDestGridNo) / 2;
                        }

                        pfClimbingNecessary = true;
                        psClimbGridNo = sClimbGridNo;
                    }
                }
            }
        }
        else
        {
            // different levels
            if (pSoldier.bLevel == 0)
            {
                //got to go UP onto building
                sClimbGridNo = FindClosestClimbPointAvailableToAI(pSoldier, pSoldier.sGridNo, sDestGridNo, true);
            }
            else
            {
                // got to go DOWN off building
                sClimbGridNo = FindClosestClimbPointAvailableToAI(pSoldier, pSoldier.sGridNo, pSoldier.sGridNo, false);
            }

            if (sClimbGridNo == NOWHERE)
            {
                sPathCost = 0;
            }
            else
            {
                sPathCost = PathAI.PlotPath(pSoldier, sClimbGridNo, null, false, null, AnimationStates.WALKING, null, null, 0);
                if (sPathCost != 0)
                {
                    // add in the cost of climbing up or down
                    if (pSoldier.bLevel == 0)
                    {
                        // must climb up
                        sPathCost += AP.CLIMBROOF;
                        if (fAddCostAfterClimbingUp)
                        {
                            // add to path a rough estimate of how far to go from the climb gridno to the friend
                            // estimate walk cost
                            sPathCost += (AP.MOVEMENT_FLAT + WALKCOST) * IsometricUtils.PythSpacesAway(sClimbGridNo, sDestGridNo);
                        }
                    }
                    else
                    {
                        // must climb down
                        sPathCost += AP.CLIMBOFFROOF;
                        // add to path a rough estimate of how far to go from the climb gridno to the friend
                        // estimate walk cost
                        sPathCost += (AP.MOVEMENT_FLAT + WALKCOST) * IsometricUtils.PythSpacesAway(sClimbGridNo, sDestGridNo);
                    }

                    pfClimbingNecessary = true;
                    psClimbGridNo = sClimbGridNo;
                }
            }
        }

        return (sPathCost);
    }

    bool GuySawEnemyThisTurnOrBefore(SOLDIERTYPE pSoldier)
    {
        TEAM ubTeamLoop;
        int ubIDLoop;

        for (ubTeamLoop = 0; ubTeamLoop < MAXTEAMS; ubTeamLoop++)
        {
            if (gTacticalStatus.Team[ubTeamLoop].bSide != pSoldier.bSide)
            {
                // consider guys in this team, which isn't on our side
                for (ubIDLoop = gTacticalStatus.Team[ubTeamLoop].bFirstID; ubIDLoop <= gTacticalStatus.Team[ubTeamLoop].bLastID; ubIDLoop++)
                {
                    // if this guy SAW an enemy recently...
                    if (pSoldier.bOppList[ubIDLoop] >= SEEN_CURRENTLY)
                    {
                        return (true);
                    }
                }
            }
        }

        return (false);
    }

    int ClosestReachableFriendInTrouble(SOLDIERTYPE pSoldier, out bool pfClimbingNecessary)
    {
        pfClimbingNecessary = false;
        int uiLoop;
        int sPathCost, sClosestFriend = NOWHERE, sShortestPath = 1000, sClimbGridNo;
        bool fClimbingNecessary, fClosestClimbingNecessary = false;
        SOLDIERTYPE? pFriend;

        // civilians don't really have any "friends", so they don't bother with this
        if (PTR_CIVILIAN(pSoldier))
        {
            return (sClosestFriend);
        }

        // consider every friend of this soldier (locations assumed to be known)
        for (uiLoop = 0; uiLoop < guiNumMercSlots; uiLoop++)
        {
            pFriend = MercSlots[uiLoop];

            // if this merc is inactive, at base, on assignment, or dead
            if (pFriend is null)
            {
                continue;          // next merc
            }

            // if this merc is neutral or NOT on the same side, he's not a friend
            if (pFriend.bNeutral > 0 || (pSoldier.bSide != pFriend.bSide))
            {
                continue;          // next merc
            }

            // if this "friend" is actually US
            if (pFriend.ubID == pSoldier.ubID)
            {
                continue;          // next merc
            }

            // CJC: restrict "last one to radio" to only if that guy saw us this turn or last turn

            // if this friend is not under fire, and isn't the last one to radio
            if (!(pFriend.bUnderFire > 0 || (pFriend.ubID == gTacticalStatus.Team[pFriend.bTeam].ubLastMercToRadio && GuySawEnemyThisTurnOrBefore(pFriend))))
            {
                continue;          // next merc
            }

            // if we're already neighbors
            if (IsometricUtils.SpacesAway(pSoldier.sGridNo, pFriend.sGridNo) == 1)
            {
                continue;          // next merc
            }

            // get the AP cost to go to this friend's gridno
            sPathCost = EstimatePathCostToLocation(pSoldier, pFriend.sGridNo, pFriend.bLevel, true, out fClimbingNecessary, out sClimbGridNo);

            // if we can get there
            if (sPathCost != 0)
            {
                //sprintf(tempstr,"Path cost to friend %s's location is %d",pFriend.name,pathCost);
                //PopMessage(tempstr);

                if (sPathCost < sShortestPath)
                {
                    if (fClimbingNecessary)
                    {
                        sClosestFriend = sClimbGridNo;
                    }
                    else
                    {
                        sClosestFriend = pFriend.sGridNo;
                    }

                    sShortestPath = sPathCost;
                    fClosestClimbingNecessary = fClimbingNecessary;
                }
            }
        }


#if DEBUGDECISIONS
    if (sClosestFriend != NOWHERE)
    {
        AINumMessage("CLOSEST FRIEND is at gridno ", sClosestFriend);
    }
#endif

        pfClimbingNecessary = fClosestClimbingNecessary;
        return (sClosestFriend);
    }

    public static int DistanceToClosestFriend(SOLDIERTYPE pSoldier)
    {
        // find the distance to the closest person on the same team
        int ubLoop;
        SOLDIERTYPE pTargetSoldier;
        int sMinDist = 1000;
        int sDist;

        // Loop through all mercs on player team
        ubLoop = gTacticalStatus.Team[pSoldier.bTeam].bFirstID;

        for (; ubLoop <= gTacticalStatus.Team[pSoldier.bTeam].bLastID; ubLoop++)
        {
            if (ubLoop == pSoldier.ubID)
            {
                // same guy - continue!
                continue;
            }

            pTargetSoldier = Menptr[ubLoop];

            if (pSoldier.bActive && pSoldier.bInSector)
            {
                if (!pTargetSoldier.bActive || !pTargetSoldier.bInSector)
                {
                    continue;
                }
                // if not conscious, skip him
                else if (pTargetSoldier.bLife < OKLIFE)
                {
                    continue;
                }
            }
            else
            {
                // compare sector #s
                if ((pSoldier.sSectorX != pTargetSoldier.sSectorX) ||
                    (pSoldier.sSectorY != pTargetSoldier.sSectorY) ||
                    (pSoldier.bSectorZ != pTargetSoldier.bSectorZ))
                {
                    continue;
                }
                else if (pTargetSoldier.bLife < OKLIFE)
                {
                    continue;
                }
                else
                {
                    // well there's someone who could be near
                    return (1);
                }
            }

            sDist = IsometricUtils.SpacesAway(pSoldier.sGridNo, pTargetSoldier.sGridNo);

            if (sDist < sMinDist)
            {
                sMinDist = sDist;
            }
        }

        return (sMinDist);
    }

    public static bool InWaterGasOrSmoke(SOLDIERTYPE pSoldier, int sGridNo)
    {
        if (WorldManager.WaterTooDeepForAttacks(sGridNo))
        {
            return (true);
        }

        // smoke
        if (gpWorldLevelData[sGridNo].ubExtFlags[pSoldier.bLevel].HasFlag(MAPELEMENTFLAGS_EXT.SMOKE))
        {
            return (true);
        }

        // tear/mustard gas
        if ((gpWorldLevelData[sGridNo].ubExtFlags[pSoldier.bLevel].HasFlag(MAPELEMENTFLAGS_EXT.TEARGAS | MAPELEMENTFLAGS_EXT.MUSTARDGAS))
            && (pSoldier.inv[InventorySlot.HEAD1POS].usItem != Items.GASMASK
            && pSoldier.inv[InventorySlot.HEAD2POS].usItem != Items.GASMASK))
        {
            return (true);
        }

        return (false);
    }

    public static bool InGasOrSmoke(SOLDIERTYPE pSoldier, int sGridNo)
    {
        // smoke
        if (gpWorldLevelData[sGridNo].ubExtFlags[pSoldier.bLevel].HasFlag(MAPELEMENTFLAGS_EXT.SMOKE))
        {
            return (true);
        }

        // tear/mustard gas
        if ((gpWorldLevelData[sGridNo].ubExtFlags[pSoldier.bLevel].HasFlag(MAPELEMENTFLAGS_EXT.TEARGAS | MAPELEMENTFLAGS_EXT.MUSTARDGAS))
            && (pSoldier.inv[InventorySlot.HEAD1POS].usItem != Items.GASMASK && pSoldier.inv[InventorySlot.HEAD2POS].usItem != Items.GASMASK))
        {
            return (true);
        }

        return (false);
    }


    bool InWaterOrGas(SOLDIERTYPE pSoldier, int sGridNo)
    {
        if (WorldManager.WaterTooDeepForAttacks(sGridNo))
        {
            return (true);
        }

        // tear/mustard gas
        if ((gpWorldLevelData[sGridNo].ubExtFlags[pSoldier.bLevel].HasFlag(MAPELEMENTFLAGS_EXT.TEARGAS | MAPELEMENTFLAGS_EXT.MUSTARDGAS))
            && (pSoldier.inv[InventorySlot.HEAD1POS].usItem != Items.GASMASK && pSoldier.inv[InventorySlot.HEAD2POS].usItem != Items.GASMASK))
        {
            return (true);
        }

        return (false);
    }

    public static bool InGas(SOLDIERTYPE pSoldier, int sGridNo)
    {
        // tear/mustard gas
        if ((gpWorldLevelData[sGridNo].ubExtFlags[pSoldier.bLevel].HasFlag(MAPELEMENTFLAGS_EXT.TEARGAS | MAPELEMENTFLAGS_EXT.MUSTARDGAS))
            && (pSoldier.inv[InventorySlot.HEAD1POS].usItem != Items.GASMASK
            && pSoldier.inv[InventorySlot.HEAD2POS].usItem != Items.GASMASK))
        {
            return (true);
        }

        return (false);
    }

    public static bool WearGasMaskIfAvailable(SOLDIERTYPE pSoldier)
    {
        InventorySlot bSlot, bNewSlot;

        bSlot = ItemSubSystem.FindObj(pSoldier, Items.GASMASK);
        if (bSlot == NO_SLOT)
        {
            return (false);
        }
        if (bSlot == InventorySlot.HEAD1POS || bSlot == InventorySlot.HEAD2POS)
        {
            return (false);
        }
        if (pSoldier.inv[InventorySlot.HEAD1POS].usItem == NOTHING)
        {
            bNewSlot = InventorySlot.HEAD1POS;
        }
        else if (pSoldier.inv[InventorySlot.HEAD2POS].usItem == NOTHING)
        {
            bNewSlot = InventorySlot.HEAD2POS;
        }
        else
        {
            // screw it, going in position 1 anyhow
            bNewSlot = InventorySlot.HEAD1POS;
        }

        RearrangePocket(pSoldier, bSlot, bNewSlot, true);
        return (true);
    }

    bool InLightAtNight(int sGridNo, int bLevel)
    {
        int ubBackgroundLightLevel;

        // do not consider us to be "in light" if we're in an underground sector
        if (gbWorldSectorZ > 0)
        {
            return (false);
        }

        ubBackgroundLightLevel = GetTimeOfDayAmbientLightLevel();

        if (ubBackgroundLightLevel < NORMAL_LIGHTLEVEL_DAY + 2)
        {
            // don't consider it nighttime, too close to daylight levels
            return (false);
        }

        // could've been placed here, ignore the light
        if (RenderFun.InARoom(sGridNo, out var _))
        {
            return (false);
        }

        // NB light levels are backwards, so a lower light level means the 
        // spot in question is BRIGHTER

        if (LightTrueLevel(sGridNo, bLevel) < ubBackgroundLightLevel)
        {
            return (true);
        }

        return (false);
    }

    MORALE CalcMorale(SOLDIERTYPE pSoldier)
    {
        int uiLoop, uiLoop2;
        int iOurTotalThreat = 0, iTheirTotalThreat = 0;
        int sOppThreatValue, sFrndThreatValue, sMorale;
        int iPercent;
        int bMostRecentOpplistValue;
        MORALE bMoraleCategory;
        int pSeenOpp; //,*friendOlPtr;
        int pbPersOL, pbPublOL;
        SOLDIERTYPE? pOpponent, pFriend;

        // if army guy has NO weapons left then panic!
        if (pSoldier.bTeam == ENEMY_TEAM)
        {
            if (FindAIUsableObjClass(pSoldier, IC.WEAPON) == NO_SLOT)
            {
                return (MORALE.HOPELESS);
            }
        }

        // hang pointers to my personal opplist, my team's public opplist, and my
        // list of previously seen opponents
        pSeenOpp = (gbSeenOpponents[pSoldier.ubID][0]);

        // loop through every one of my possible opponents
        for (uiLoop = 0; uiLoop < guiNumMercSlots; uiLoop++)
        {
            pOpponent = MercSlots[uiLoop];

            // if this merc is inactive, at base, on assignment, dead, unconscious
            if (pOpponent is null || (pOpponent.bLife < OKLIFE))
            {
                continue;          // next merc
            }

            // if this merc is neutral/on same side, he's not an opponent, skip him!
            if (CONSIDERED_NEUTRAL(pSoldier, pOpponent) || (pSoldier.bSide == pOpponent.bSide))
            {
                continue;          // next merc
            }

            // Special stuff for Carmen the bounty hunter
            if (pSoldier.bAttitude == Attitudes.ATTACKSLAYONLY && pOpponent.ubProfile != NPCID.SLAY)
            {
                continue;  // next opponent
            }

            pbPersOL = pSoldier.bOppList[pOpponent.ubID];
            pbPublOL = gbPublicOpplist[pSoldier.bTeam][pOpponent.ubID];
            pSeenOpp = gbSeenOpponents[pSoldier.ubID][pOpponent.ubID];

            // if this opponent is unknown to me personally AND unknown to my team, too
            if ((pbPersOL == NOT_HEARD_OR_SEEN) && (pbPublOL == NOT_HEARD_OR_SEEN))
            {
                // if I have never seen him before anywhere in this sector, either
                if ((pSeenOpp == 0))
                {
                    continue;        // next merc
                }

                // have seen him in the past, so he remains something of a threat
                bMostRecentOpplistValue = 0;        // uses the free slot for 0 opplist
            }
            else         // decide which opplist is more current
            {
                // if personal knowledge is more up to date or at least equal
                if ((gubKnowledgeValue[pbPublOL - OLDEST_HEARD_VALUE, pbPersOL - OLDEST_HEARD_VALUE] > 0) || (pbPersOL == pbPublOL))
                {
                    bMostRecentOpplistValue = pbPersOL;      // use personal
                }
                else
                {
                    bMostRecentOpplistValue = pbPublOL;      // use public
                }
            }

            iPercent = AIMain.ThreatPercent[bMostRecentOpplistValue - OLDEST_HEARD_VALUE];

            sOppThreatValue = (iPercent * CalcManThreatValue(pOpponent, pSoldier.sGridNo, false, pSoldier)) / 100;

            //sprintf(tempstr,"Known opponent %s, opplist status %d, percent %d, threat = %d",
            //           ExtMen[pOpponent.ubID].name,ubMostRecentOpplistValue,ubPercent,sOppThreatValue);
            //PopMessage(tempstr);

            // ADD this to their running total threatValue (decreases my MORALE)
            iTheirTotalThreat += sOppThreatValue;
            //NumMessage("Their TOTAL threat now = ",sTheirTotalThreat);

            // NOW THE FUN PART: SINCE THIS OPPONENT IS KNOWN TO ME IN SOME WAY,
            // ANY FRIENDS OF MINE THAT KNOW ABOUT HIM BOOST MY MORALE.  SO, LET'S GO
            // THROUGH THEIR PERSONAL OPPLISTS AND CHECK WHICH OF MY FRIENDS KNOW
            // SOMETHING ABOUT HIM AND WHAT THEIR THREAT VALUE TO HIM IS.

            for (uiLoop2 = 0; uiLoop2 < guiNumMercSlots; uiLoop2++)
            {
                pFriend = MercSlots[uiLoop2];

                // if this merc is inactive, at base, on assignment, dead, unconscious
                if (pFriend is null || (pFriend.bLife < OKLIFE))
                {
                    continue;        // next merc
                }

                // if this merc is not on my side, then he's NOT one of my friends

                // WE CAN'T AFFORD TO CONSIDER THE ENEMY OF MY ENEMY MY FRIEND, HERE!
                // ONLY IF WE ARE ACTUALLY OFFICIALLY CO-OPERATING TOGETHER (SAME SIDE)
                if (pFriend.IsNeutral && !(pSoldier.ubCivilianGroup != CIV_GROUP.NON_CIV_GROUP && pSoldier.ubCivilianGroup == pFriend.ubCivilianGroup))
                {
                    continue;        // next merc
                }

                if (pSoldier.bSide != pFriend.bSide)
                {
                    continue;        // next merc
                }

                // THIS TEST IS INVALID IF A COMPUTER-TEAM IS PLAYING CO-OPERATIVELY
                // WITH A NON-COMPUTER TEAM SINCE THE OPPLISTS INVOLVED ARE NOT
                // UP-TO-DATE.  THIS SITUATION IS CURRENTLY NOT POSSIBLE IN HTH/DG.

                // ALSO NOTE THAT WE COUNT US AS OUR (BEST) FRIEND FOR THESE CALCULATIONS

                // subtract HEARD_2_TURNS_AGO (which is negative) to make values start at 0 and
                // be positive otherwise
                iPercent = AIMain.ThreatPercent[pFriend.bOppList[pOpponent.ubID] - OLDEST_HEARD_VALUE];

                // reduce the percentage value based on how far away they are from the enemy, if they only hear him
                if (pFriend.bOppList[pOpponent.ubID] <= HEARD_LAST_TURN)
                {
                    iPercent -= IsometricUtils.PythSpacesAway(pSoldier.sGridNo, pFriend.sGridNo) * 2;
                    if (iPercent <= 0)
                    {
                        //ignore!
                        continue;
                    }
                }

                sFrndThreatValue = (iPercent * CalcManThreatValue(pFriend, pOpponent.sGridNo, false, pSoldier)) / 100;

                //sprintf(tempstr,"Known by friend %s, opplist status %d, percent %d, threat = %d",
                //         ExtMen[pFriend.ubID].name,pFriend.bOppList[pOpponent.ubID],ubPercent,sFrndThreatValue);
                //PopMessage(tempstr);

                // ADD this to our running total threatValue (increases my MORALE)
                // We multiply by sOppThreatValue to PRO-RATE this based on opponent's
                // threat value to ME personally.  Divide later by sum of them all.
                iOurTotalThreat += sOppThreatValue * sFrndThreatValue;
            }

            // this could get slow if I have a lot of friends...
            //KeepInterfaceGoing();
        }


        // if they are no threat whatsoever
        if (!iTheirTotalThreat)
        {
            sMorale = 500;        // our morale is just incredible
        }
        else
        {
            // now divide sOutTotalThreat by sTheirTotalThreat to get the REAL value
            iOurTotalThreat /= iTheirTotalThreat;

            // calculate the morale (100 is even, < 100 is us losing, > 100 is good)
            sMorale = (int)((100 * iOurTotalThreat) / iTheirTotalThreat);
        }


        if (sMorale <= 25)              // odds 1:4 or worse
        {
            bMoraleCategory = MORALE.HOPELESS;
        }
        else if (sMorale <= 50)         // odds between 1:4 and 1:2
        {
            bMoraleCategory = MORALE.WORRIED;
        }
        else if (sMorale <= 150)        // odds between 1:2 and 3:2
        {
            bMoraleCategory = MORALE.NORMAL;
        }
        else if (sMorale <= 300)        // odds between 3:2 and 3:1
        {
            bMoraleCategory = MORALE.CONFIDENT;
        }
        else                           // odds better than 3:1
        {
            bMoraleCategory = MORALE.FEARLESS;
        }

        switch (pSoldier.bAttitude)
        {
            case DEFENSIVE:
                bMoraleCategory--;
                break;
            case BRAVESOLO:
                bMoraleCategory += 2;
                break;
            case BRAVEAID:
                bMoraleCategory += 2;
                break;
            case CUNNINGSOLO:
                break;
            case CUNNINGAID:
                break;
            case AGGRESSIVE:
                bMoraleCategory++;
                break;
        }

        // make idiot administrators much more aggressive
        if (pSoldier.ubSoldierClass == SOLDIER_CLASS_ADMINISTRATOR)
        {
            bMoraleCategory += 2;
        }


        // if still full of energy
        if (pSoldier.bBreath > 75)
        {
            bMoraleCategory++;
        }
        else
        {
            // if getting a bit low on breath
            if (pSoldier.bBreath < 40)
            {
                bMoraleCategory--;
            }

            // if getting REALLY low on breath
            if (pSoldier.bBreath < 10)
            {
                bMoraleCategory--;
            }
        }


        // if still very healthy
        if (pSoldier.bLife > 75)
        {
            bMoraleCategory++;
        }
        else
        {
            // if getting a bit low on life
            if (pSoldier.bLife < 40)
            {
                bMoraleCategory--;
            }

            // if getting REALLY low on life
            if (pSoldier.bLife < 20)
            {
                bMoraleCategory--;
            }
        }


        // if soldier is currently not under fire
        if (!pSoldier.bUnderFire)
        {
            bMoraleCategory++;
        }


        // if adjustments made it outside the allowed limits
        if (bMoraleCategory < MORALE.HOPELESS)
        {
            bMoraleCategory = MORALE.HOPELESS;
        }
        else
        {
            if (bMoraleCategory > MORALE.FEARLESS)
            {
                bMoraleCategory = MORALE.FEARLESS;
            }
        }

        // if only 1/4 of side left, reduce morale
        // and do this after we've capped all those other silly values
        /*
        if ( pSoldier.bTeam == ENEMY_TEAM && gTacticalStatus.Team[ ENEMY_TEAM ].bMenInSector <= gTacticalStatus.bOriginalSizeOfEnemyForce / 4 )
        {
           bMoraleCategory -= 2;
         if (bMoraleCategory < MORALE_HOPELESS)
           bMoraleCategory = MORALE_HOPELESS;
        }
        */

        // brave guys never get hopeless, at worst they get worried
        if (bMoraleCategory == MORALE.HOPELESS &&
            (pSoldier.bAttitude == BRAVESOLO || pSoldier.bAttitude == BRAVEAID))
        {
            bMoraleCategory = MORALE.WORRIED;
        }


#if DEBUGDECISIONS
    DebugAI(String("Morale = %d (category %d), sOutTotalThreat %d, sTheirTotalThreat %d\n",
           morale, bMoraleCategory, sOutTotalThreat, sTheirTotalThreat));
#endif

        return (bMoraleCategory);
    }

    int CalcManThreatValue(SOLDIERTYPE pEnemy, int sMyGrid, bool ubReduceForCover, SOLDIERTYPE pMe)
    {
        int iThreatValue = 0;
        bool fForCreature = CREATURE_OR_BLOODCAT(pMe);

        // If man is inactive, at base, on assignment, dead, unconscious
        if (!pEnemy.bActive || !pEnemy.bInSector || pEnemy.bLife == 0)
        {
            // he's no threat at all, return a negative number
            iThreatValue = -999;
            return (iThreatValue);
        }

        // in boxing mode, let only a boxer be considered a threat.
        if ((gTacticalStatus.bBoxingState == BoxingStates.BOXING) && !(pEnemy.uiStatusFlags.HasFlag(SOLDIER.BOXER)))
        {
            iThreatValue = -999;
            return (iThreatValue);
        }

        if (fForCreature)
        {
            // health (1-100)
            iThreatValue += (int)pEnemy.bLife;
            // bleeding (more attactive!) (1-100)
            iThreatValue += (int)pEnemy.bBleeding;
            // decrease according to distance
            iThreatValue = (iThreatValue * 10) / (10 + IsometricUtils.PythSpacesAway(sMyGrid, pEnemy.sGridNo));

        }
        else
        {
            // ADD twice the man's level (2-20)
            iThreatValue += pEnemy.bExpLevel;

            // ADD man's total action points (10-35)
            iThreatValue += CalcActionPoints(pEnemy);

            // ADD 1/2 of man's current action points (4-17)
            iThreatValue += (pEnemy.bActionPoints / 2);

            // ADD 1/10 of man's current health (0-10)
            iThreatValue += (uint)(pEnemy.bLife / 10);

            if (pEnemy.bAssignment < Assignments.ON_DUTY)
            {
                // ADD 1/4 of man's protection percentage (0-25)
                iThreatValue += ArmourPercent(pEnemy) / 4;

                // ADD 1/5 of man's marksmanship skill (0-20)
                iThreatValue += (pEnemy.bMarksmanship / 5);

                if (Item[pEnemy.inv[InventorySlot.HANDPOS].usItem].usItemClass.HasFlag(IC.WEAPON))
                {
                    // ADD the deadliness of the item(weapon) he's holding (0-50)
                    iThreatValue += Weapon[pEnemy.inv[InventorySlot.HANDPOS].usItem].ubDeadliness;
                }
            }

            // SUBTRACT 1/5 of man's bleeding (0-20)
            iThreatValue -= (pEnemy.bBleeding / 5);

            // SUBTRACT 1/10 of man's breath deficiency (0-10)
            iThreatValue -= ((100 - pEnemy.bBreath) / 10);

            // SUBTRACT man's current shock value
            iThreatValue -= pEnemy.bShock;
        }

        // if I have a specifically defined spot where I'm at (sometime I don't!)
        if (sMyGrid != NOWHERE)
        {
            // ADD 10% if man's already been shooting at me
            if (pEnemy.sLastTarget == sMyGrid)
            {
                iThreatValue += (iThreatValue / 10);
            }
            else
            {
                // ADD 5% if man's already facing me
                if (pEnemy.bDirection == SoldierControl.atan8(IsometricUtils.CenterX(pEnemy.sGridNo), IsometricUtils.CenterY(pEnemy.sGridNo), IsometricUtils.CenterX(sMyGrid), IsometricUtils.CenterY(sMyGrid)))
                {
                    iThreatValue += (iThreatValue / 20);
                }
            }
        }

        // if this man is conscious
        if (pEnemy.bLife >= OKLIFE)
        {
            // and we were told to reduce threat for my cover
            if (ubReduceForCover > 0 && (sMyGrid != NOWHERE))
            {
                // Reduce iThreatValue to same % as the chance HE has shoot through at ME
                //iThreatValue = (iThreatValue * ChanceToGetThrough( pEnemy, myGrid, FAKE, ACTUAL, TESTWALLS, 9999, M9PISTOL, NOT_FOR_LOS)) / 100;
                //iThreatValue = (iThreatValue * SoldierTo3DLocationChanceToGetThrough( pEnemy, myGrid, FAKE, ACTUAL, TESTWALLS, 9999, M9PISTOL, NOT_FOR_LOS)) / 100;
                iThreatValue = (iThreatValue * SoldierToLocationChanceToGetThrough(pEnemy, sMyGrid, pMe.bLevel, 0, pMe.ubID)) / 100;
            }
        }
        else
        {
            // if he's still something of a threat
            if (iThreatValue > 0)
            {
                // drastically reduce his threat value (divide by 5 to 18)
                iThreatValue /= (4 + (OKLIFE - pEnemy.bLife));
            }
        }

        // threat value of any opponent can never drop below 1
        if (iThreatValue < 1)
        {
            iThreatValue = 1;
        }

        //sprintf(tempstr,"%s's iThreatValue = ",pEnemy.name);
        //NumMessage(tempstr,iThreatValue);

#if BETAVERSION    // unnecessary for real release                                   
    // NOTE: maximum is about 200 for a healthy Mike type with a mortar!
    if (iThreatValue > 250)
    {
        sprintf(tempstr, "CalcManThreatValue: WARNING - %d has a very high threat value of %d", pEnemy.ubID, iThreatValue);

#if RECORDNET
        fprintf(NetDebugFile, "\t%s\n", tempstr);
#endif

#if TESTVERSION
        PopMessage(tempstr);
#endif

    }
#endif

        return (iThreatValue);
    }

    public static int RoamingRange(SOLDIERTYPE pSoldier, out int pusFromGridNo)
    {
        if (CREATURE_OR_BLOODCAT(pSoldier))
        {
            if (pSoldier.bAlertStatus == STATUS.BLACK)
            {
                pusFromGridNo = pSoldier.sGridNo; // from current position!
                return (MAX_ROAMING_RANGE);
            }
        }
        if (pSoldier.bOrders == Orders.POINTPATROL || pSoldier.bOrders == Orders.RNDPTPATROL)
        {
            // roam near NEXT PATROL POINT, not from where merc starts out
            pusFromGridNo = pSoldier.usPatrolGrid[pSoldier.bNextPatrolPnt];
        }
        else
        {
            // roam around where mercs started
            //*pusFromGridNo = pSoldier.sInitialGridNo;
            pusFromGridNo = pSoldier.usPatrolGrid[0];
        }

        switch (pSoldier.bOrders)
        {
            // JA2 GOLD: give non-NPCs a 5 tile roam range for cover in combat when being shot at
            case Orders.STATIONARY:
                if (pSoldier.ubProfile != NO_PROFILE || (pSoldier.bAlertStatus < STATUS.BLACK && !(pSoldier.bUnderFire > 0)))
                {
                    return (0);
                }
                else
                {
                    return (5);
                }
            case Orders.ONGUARD:
                return (5);
            case Orders.CLOSEPATROL:
                return (15);
            case Orders.RNDPTPATROL:
            case Orders.POINTPATROL:
                return (10);     // from nextPatrolGrid, not whereIWas
            case Orders.FARPATROL:
                if (pSoldier.bAlertStatus < STATUS.RED)
                {
                    return (25);
                }
                else
                {
                    return (50);
                }
            case Orders.ONCALL:
                if (pSoldier.bAlertStatus < STATUS.RED)
                {
                    return (10);
                }
                else
                {
                    return (30);
                }
            case Orders.SEEKENEMY:
                pusFromGridNo = pSoldier.sGridNo; // from current position!
                return (MAX_ROAMING_RANGE);
            default:
                return (0);
        }
    }


    public static void RearrangePocket(SOLDIERTYPE pSoldier, InventorySlot bPocket1, InventorySlot bPocket2, bool bPermanent)
    {
        // NB there's no such thing as a temporary swap for now...
        SwapObjs((pSoldier.inv[bPocket1]), (pSoldier.inv[bPocket2]));
    }

    bool FindBetterSpotForItem(SOLDIERTYPE pSoldier, InventorySlot bSlot)
    {
        // looks for a place in the slots to put an item in a hand or armour
        // position, and moves it there.
        if (bSlot >= InventorySlot.BIGPOCK1POS)
        {
            return (false);
        }
        if (pSoldier.inv[bSlot].usItem == NOTHING)
        {
            // well that's just fine then!
            return (true);
        }

        if (Item[pSoldier.inv[bSlot].usItem].ubPerPocket == 0)
        {
            // then we're looking for a big pocket
            bSlot = FindEmptySlotWithin(pSoldier, InventorySlot.BIGPOCK1POS, InventorySlot.BIGPOCK4POS);
        }
        else
        {
            // try a small pocket first
            bSlot = FindEmptySlotWithin(pSoldier, InventorySlot.SMALLPOCK1POS, InventorySlot.SMALLPOCK8POS);
            if (bSlot == NO_SLOT)
            {
                bSlot = FindEmptySlotWithin(pSoldier, InventorySlot.BIGPOCK1POS, InventorySlot.BIGPOCK4POS);
            }
        }
        if (bSlot == NO_SLOT)
        {
            return (false);
        }

        RearrangePocket(pSoldier, InventorySlot.HANDPOS, bSlot, FOREVER);
        return (true);
    }

    QUOTE_ACTION_ID GetTraversalQuoteActionID(WorldDirections bDirection)
    {
        switch (bDirection)
        {
            case WorldDirections.NORTHEAST: // east
                return (QUOTE_ACTION_ID.TRAVERSE_EAST);

            case WorldDirections.SOUTHEAST: // south
                return (QUOTE_ACTION_ID.TRAVERSE_SOUTH);

            case WorldDirections.SOUTHWEST: // west
                return (QUOTE_ACTION_ID.TRAVERSE_WEST);

            case WorldDirections.NORTHWEST: // north
                return (QUOTE_ACTION_ID.TRAVERSE_NORTH);

            default:
                return (0);
        }
    }

    public static int SoldierDifficultyLevel(SOLDIERTYPE pSoldier)
    {
        int bDifficultyBase;
        int bDifficulty;

        // difficulty modifier ranges from 0 to 100
        // and we want to end up with a number between 0 and 4 (4=hardest)
        // to a base of 1, divide by 34 to get a range from 1 to 3
        bDifficultyBase = 1 + (CalcDifficultyModifier(pSoldier.ubSoldierClass) / 34);

        switch (pSoldier.ubSoldierClass)
        {
            case SOLDIER_CLASS.ADMINISTRATOR:
                bDifficulty = bDifficultyBase - 1;
                break;

            case SOLDIER_CLASS.ARMY:
                bDifficulty = bDifficultyBase;
                break;

            case SOLDIER_CLASS.ELITE:
                bDifficulty = bDifficultyBase + 1;
                break;

            // hard code militia;
            case SOLDIER_CLASS.GREEN_MILITIA:
                bDifficulty = 2;
                break;

            case SOLDIER_CLASS.REG_MILITIA:
                bDifficulty = 3;
                break;

            case SOLDIER_CLASS.ELITE_MILITIA:
                bDifficulty = 4;
                break;

            default:
                if (pSoldier.bTeam == CREATURE_TEAM)
                {
                    bDifficulty = bDifficultyBase + pSoldier.bLevel / 4;
                }
                else // civ...
                {
                    bDifficulty = (bDifficultyBase + pSoldier.bLevel / 4) - 1;
                }
                break;

        }

        bDifficulty = Math.Max(bDifficulty, 0);
        bDifficulty = Math.Min(bDifficulty, 4);

        return bDifficulty;
    }

    bool ValidCreatureTurn(SOLDIERTYPE pCreature, WorldDirections bNewDirection)
    {
        int bDirChange;
        WorldDirections bTempDir;
        int bLoop;
        bool fFound;

        bDirChange = (int)QuickestDirection(pCreature.bDirection, bNewDirection);

        for (bLoop = 0; bLoop < 2; bLoop++)
        {
            fFound = true;

            bTempDir = pCreature.bDirection;

            do
            {

                bTempDir += bDirChange;
                if (bTempDir < WorldDirections.NORTH)
                {
                    bTempDir = WorldDirections.NORTHWEST;
                }
                else if (bTempDir > WorldDirections.NORTHWEST)
                {
                    bTempDir = WorldDirections.NORTH;
                }
                if (!SoldierControl.InternalIsValidStance(pCreature, bTempDir, AnimationHeights.ANIM_STAND))
                {
                    fFound = false;
                    break;
                }

            } while (bTempDir != bNewDirection);

            if (fFound)
            {
                break;
            }
            else if (bLoop > 0)
            {
                // can't find a dir!
                return (false);
            }
            else
            {
                // try the other direction
                bDirChange = bDirChange * -1;
            }
        }

        return (true);
    }

    int RangeChangeDesire(SOLDIERTYPE pSoldier)
    {
        MORALE iRangeFactorMultiplier;

        iRangeFactorMultiplier = pSoldier.bAIMorale - 1;
        switch (pSoldier.bAttitude)
        {
            case Attitudes.DEFENSIVE:
                iRangeFactorMultiplier += -1;
                break;
            case Attitudes.BRAVESOLO:
                iRangeFactorMultiplier += 2;
                break;
            case Attitudes.BRAVEAID:
                iRangeFactorMultiplier += 2;
                break;
            case Attitudes.CUNNINGSOLO:
                iRangeFactorMultiplier += 0;
                break;
            case Attitudes.CUNNINGAID:
                iRangeFactorMultiplier += 0;
                break;
            case Attitudes.ATTACKSLAYONLY:
            case Attitudes.AGGRESSIVE:
                iRangeFactorMultiplier += 1;
                break;
        }
        if (gTacticalStatus.bConsNumTurnsWeHaventSeenButEnemyDoes > 0)
        {
            iRangeFactorMultiplier += gTacticalStatus.bConsNumTurnsWeHaventSeenButEnemyDoes;
        }

        return ((int)iRangeFactorMultiplier);
    }

    bool ArmySeesOpponents()
    {
        int cnt;
        SOLDIERTYPE pSoldier;

        for (cnt = gTacticalStatus.Team[ENEMY_TEAM].bFirstID; cnt <= gTacticalStatus.Team[ENEMY_TEAM].bLastID; cnt++)
        {
            pSoldier = MercPtrs[cnt];

            if (pSoldier.bActive && pSoldier.bInSector && pSoldier.bLife >= OKLIFE && pSoldier.bOppCnt > 0)
            {
                return (true);
            }
        }

        return (false);
    }
}
