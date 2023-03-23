using System.Diagnostics;
using SharpAlliance.Core.SubSystems;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class CreatureDecideAction
{
    public static void CreatureCall(SOLDIERTYPE pCaller)
    {
        CALLER ubCallerType = 0;
        int ubReceiver;
        int bFullPriority;
        int bPriority;
        SOLDIERTYPE? pReceiver;
        int usDistToCaller;
        // communicate call to all creatures on map through ultrasonics

        gTacticalStatus.Team[pCaller.bTeam].bAwareOfOpposition = 1;
        // bAction should be AI_ACTION_CREATURE_CALL (new)
        // usActionData is call enum #
        switch (pCaller.ubBodyType)
        {
            case SoldierBodyTypes.ADULTFEMALEMONSTER:
            case SoldierBodyTypes.YAF_MONSTER:
                ubCallerType = CALLER.FEMALE;
                break;
            case SoldierBodyTypes.QUEENMONSTER:
                ubCallerType = CALLER.QUEEN;
                break;
            // need to add male
            case SoldierBodyTypes.AM_MONSTER:
            case SoldierBodyTypes.YAM_MONSTER:
                ubCallerType = CALLER.MALE;
                break;
            default:
                ubCallerType = CALLER.FEMALE;
                break;
        }
        if (pCaller.bHunting > 0) // which should only be set for females outside of the hive
        {
            bFullPriority = gbHuntCallPriority[(CALL)pCaller.usActionData];
        }
        else
        {
            bFullPriority = gbCallPriority[(CALL)pCaller.usActionData][(int)ubCallerType];
        }

        // OK, do animation based on body type...
        switch (pCaller.ubBodyType)
        {
            case SoldierBodyTypes.ADULTFEMALEMONSTER:
            case SoldierBodyTypes.YAF_MONSTER:
            case SoldierBodyTypes.AM_MONSTER:
            case SoldierBodyTypes.YAM_MONSTER:

                SoldierControl.EVENT_InitNewSoldierAnim(pCaller, AnimationStates.MONSTER_UP, 0, false);
                break;

            case SoldierBodyTypes.QUEENMONSTER:

                SoldierControl.EVENT_InitNewSoldierAnim(pCaller, AnimationStates.QUEEN_CALL, 0, false);
                break;
        }


        for (ubReceiver = gTacticalStatus.Team[pCaller.bTeam].bFirstID; ubReceiver <= gTacticalStatus.Team[pCaller.bTeam].bLastID; ubReceiver++)
        {
            pReceiver = MercPtrs[ubReceiver];
            if (pReceiver.bActive && pReceiver.bInSector && (pReceiver.bLife >= OKLIFE) && (pReceiver != pCaller) && (pReceiver.bAlertStatus < STATUS.BLACK))
            {
                if (pReceiver.ubBodyType != SoldierBodyTypes.LARVAE_MONSTER && pReceiver.ubBodyType != SoldierBodyTypes.INFANT_MONSTER && pReceiver.ubBodyType != SoldierBodyTypes.QUEENMONSTER)
                {
                    usDistToCaller = IsometricUtils.PythSpacesAway(pReceiver.sGridNo, pCaller.sGridNo);
                    bPriority = bFullPriority - (int)(usDistToCaller / PRIORITY_DECR_DISTANCE);
                    if (bPriority > pReceiver.bCallPriority)
                    {
                        pReceiver.bCallPriority = bPriority;
                        pReceiver.bAlertStatus = STATUS.RED; // our status can't be more than red to begin with
                        pReceiver.ubCaller = pCaller.ubID;
                        pReceiver.sCallerGridNo = pCaller.sGridNo;
                        pReceiver.bCallActedUpon = 0;
                        CancelAIAction(pReceiver, FORCE);
                        if ((bPriority > FRENZY_THRESHOLD) && (pReceiver.ubBodyType == SoldierBodyTypes.ADULTFEMALEMONSTER || pReceiver.ubBodyType == SoldierBodyTypes.YAF_MONSTER))
                        {
                            // go berzerk!
                            pReceiver.bFrenzied = 1;
                        }
                    }
                }
            }
        }
    }

    public static AI_ACTION CreatureDecideActionGreen(SOLDIERTYPE? pSoldier)
    {
        int iChance;
        int iSneaky = 10;
        //int		bInWater;
        bool bInGas;

        //bInWater = MercInWater(pSoldier);

        // NB creatures would ignore smoke completely :-)

        if (pSoldier.bMobility == CREATURE.CRAWLER && pSoldier.bActionPoints < pSoldier.bInitialActionPoints)
        {
            return (AI_ACTION.NONE);
        }

        bInGas = AIUtils.InGas(pSoldier, pSoldier.sGridNo);

        if (pSoldier.bMobility == CREATURE.MOBILE)
        {

            if (TrackScent(pSoldier))
            {
                return (AI_ACTION.TRACK);
            }

            ////////////////////////////////////////////////////////////////////////////
            // POINT PATROL: move towards next point unless getting a bit winded
            ////////////////////////////////////////////////////////////////////////////

            // this takes priority over water/gas checks, so that point patrol WILL work
            // from island to island, and through gas covered areas, too
            if ((pSoldier.bOrders == Orders.POINTPATROL) && (pSoldier.bBreath >= 50))
            {
                if (PointPatrolAI(pSoldier))
                {
                    if (!gfTurnBasedAI)
                    {
                        // pause at the end of the walk!
                        pSoldier.bNextAction = AI_ACTION.WAIT;
                        pSoldier.usNextActionData = (int)REALTIME_CREATURE_AI_DELAY;
                    }

                    return (AI_ACTION.POINT_PATROL);
                }
            }

            if ((pSoldier.bOrders == Orders.RNDPTPATROL) && (pSoldier.bBreath >= 50))
            {
                if (RandomPointPatrolAI(pSoldier))
                {
                    if (!gfTurnBasedAI)
                    {
                        // pause at the end of the walk!
                        pSoldier.bNextAction = AI_ACTION.WAIT;
                        pSoldier.usNextActionData = (int)REALTIME_CREATURE_AI_DELAY;
                    }

                    return (AI_ACTION.POINT_PATROL);
                }
            }

            ////////////////////////////////////////////////////////////////////////////
            // WHEN LEFT IN WATER OR GAS, GO TO NEAREST REACHABLE SPOT OF UNGASSED LAND
            ////////////////////////////////////////////////////////////////////////////

            if ( /*bInWater || */ bInGas)
            {
                pSoldier.usActionData = FindNearestUngassedLand(pSoldier);

                if ((int)pSoldier.usActionData != NOWHERE)
                {
                    return (AI_ACTION.LEAVE_WATER_GAS);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////
        // REST IF RUNNING OUT OF BREATH
        ////////////////////////////////////////////////////////////////////////

        // if our breath is running a bit low, and we're not in the way or in water
        if ((pSoldier.bBreath < 75) /*&& !bInWater*/)
        {
            // take a breather for gods sake!
            pSoldier.usActionData = NOWHERE;
            return (AI_ACTION.NONE);
        }

        ////////////////////////////////////////////////////////////////////////////
        // RANDOM PATROL:  determine % chance to start a new patrol route
        ////////////////////////////////////////////////////////////////////////////

        if (pSoldier.bMobility != CREATURE.IMMOBILE)
        {
            iChance = 25;

            // set base chance according to orders
            switch (pSoldier.bOrders)
            {
                case Orders.STATIONARY:
                    iChance += -20;
                    break;
                case Orders.ONGUARD:
                    iChance += -15;
                    break;
                case Orders.ONCALL:
                    break;
                case Orders.CLOSEPATROL:
                    iChance += +15;
                    break;
                case Orders.RNDPTPATROL:
                case Orders.POINTPATROL:
                    iChance = 0;
                    break;
                case Orders.FARPATROL:
                    iChance += +25;
                    break;
                case Orders.SEEKENEMY:
                    iChance += -10;
                    break;
            }

            // modify chance of patrol (and whether it's a sneaky one) by attitude
            switch (pSoldier.bAttitude)
            {
                case Attitudes.DEFENSIVE:
                    iChance += -10;
                    break;
                case Attitudes.BRAVESOLO:
                    iChance += 5;
                    break;
                case Attitudes.BRAVEAID:
                    break;
                case Attitudes.CUNNINGSOLO:
                    iChance += 5;
                    iSneaky += 10;
                    break;
                case Attitudes.CUNNINGAID:
                    iSneaky += 5;
                    break;
                case Attitudes.AGGRESSIVE:
                    iChance += 10;
                    iSneaky += -5;
                    break;
            }

            // reduce chance for any injury, less likely to wander around when hurt
            iChance -= (int)(pSoldier.bLifeMax - pSoldier.bLife);

            // reduce chance if breath is down, less likely to wander around when tired
            iChance -= (100 - pSoldier.bBreath);

            // if we're in water with land miles (> 25 tiles) away,
            // OR if we roll under the chance calculated
            if ( /*bInWater ||*/ (PreRandom(100) < iChance))
            {
                pSoldier.usActionData = RandDestWithinRange(pSoldier);

                if ((int)pSoldier.usActionData != NOWHERE)
                {
                    if (!gfTurnBasedAI)
                    {
                        // pause at the end of the walk!
                        pSoldier.bNextAction = AI_ACTION.WAIT;
                        pSoldier.usNextActionData = (int)REALTIME_CREATURE_AI_DELAY;
                        if (pSoldier.bMobility == CREATURE.CRAWLER)
                        {
                            pSoldier.usNextActionData *= 2;
                        }
                    }

                    return (AI_ACTION.RANDOM_PATROL);
                }
            }

            /*
            if (pSoldier.bMobility == CREATURE_MOBILE)
            {
                ////////////////////////////////////////////////////////////////////////////
                // SEEK FRIEND: determine %chance for man to pay a friendly visit
                ////////////////////////////////////////////////////////////////////////////
                iChance = 25;

                // set base chance and maximum seeking distance according to orders
                switch (pSoldier.bOrders)
                {
                    case STATIONARY:     iChance += -20; break;
                    case ONGUARD:        iChance += -15; break;
                    case ONCALL:                         break;
                    case CLOSEPATROL:    iChance += +10; break;
                    case RNDPTPATROL:
                    case POINTPATROL:    iChance  = -10; break;
                    case FARPATROL:      iChance += +20; break;
                    case SEEKENEMY:      iChance += -10; break;
                }

                // modify for attitude
                switch (pSoldier.bAttitude)
                {
                    case DEFENSIVE:                       break;
                    case BRAVESOLO:      iChance /= 2;    break;  // loners
                    case BRAVEAID:       iChance += 10;   break;  // friendly
                    case CUNNINGSOLO:    iChance /= 2;    break;  // loners
                    case CUNNINGAID:     iChance += 10;   break;  // friendly
                    case AGGRESSIVE:                      break;
                }

                // reduce chance for any injury, less likely to wander around when hurt
                iChance -= (pSoldier.bLifeMax - pSoldier.bLife);

                // reduce chance if breath is down
                iChance -= (100 - pSoldier.bBreath);         // very likely to wait when exhausted

                if ((int) PreRandom(100) < iChance)
                {
                    if (RandomFriendWithin(pSoldier))
                    {
            #ifdef DEBUGDECISIONS
                     sprintf(tempstr,"%s - SEEK FRIEND at grid %d",pSoldier.name,pSoldier.usActionData);
                     AIPopMessage(tempstr);
            #endif

                        if (!gfTurnBasedAI)
                        {
                            // pause at the end of the walk!
                            pSoldier.bNextAction = AI_ACTION_WAIT;
                            pSoldier.usNextActionData = (int) REALTIME_CREATURE_AI_DELAY;
                        }

                        return(AI_ACTION_SEEK_FRIEND);
                    }
                }
            }
            */

            ////////////////////////////////////////////////////////////////////////////
            // LOOK AROUND: determine %chance for man to turn in place
            ////////////////////////////////////////////////////////////////////////////

            // avoid 2 consecutive random turns in a row
            if (pSoldier.bLastAction != AI_ACTION.CHANGE_FACING && (GetAPsToLook(pSoldier) <= pSoldier.bActionPoints))
            {
                iChance = 25;

                // set base chance according to orders
                if (pSoldier.bOrders == Orders.STATIONARY)
                {
                    iChance += 25;
                }

                if (pSoldier.bOrders == Orders.ONGUARD)
                {
                    iChance += 20;
                }

                if (pSoldier.bAttitude == Attitudes.DEFENSIVE)
                {
                    iChance += 25;
                }

                if (PreRandom(100) < iChance)
                {
                    // roll random directions (stored in actionData) until different from current
                    do
                    {
                        // if man has a LEGAL dominant facing, and isn't facing it, he will turn
                        // back towards that facing 50% of the time here (normally just enemies)
                        if ((pSoldier.bDominantDir >= 0) && (pSoldier.bDominantDir <= (WorldDirections)8)
                            && (pSoldier.bDirection != pSoldier.bDominantDir) && PreRandom(2) > 0)
                        {
                            pSoldier.usActionData = pSoldier.bDominantDir;
                        }
                        else
                        {
                            pSoldier.usActionData = PreRandom(8);
                        }
                    } while ((WorldDirections)pSoldier.usActionData == pSoldier.bDirection);

                    if (ValidCreatureTurn(pSoldier, (int)pSoldier.usActionData))

                    //InternalIsValidStance( pSoldier, (int) pSoldier.usActionData, ANIM_STAND ) )
                    {
                        if (!gfTurnBasedAI)
                        {
                            // pause at the end of the turn!
                            pSoldier.bNextAction = AI_ACTION.WAIT;
                            pSoldier.usNextActionData = (int)REALTIME_CREATURE_AI_DELAY;
                        }

                        return (AI_ACTION.CHANGE_FACING);
                    }
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        // NONE:
        ////////////////////////////////////////////////////////////////////////////

        // by default, if everything else fails, just stands in place without turning
        pSoldier.usActionData = NOWHERE;

        return (AI_ACTION.NONE);
    }

    public static AI_ACTION CreatureDecideActionYellow(SOLDIERTYPE pSoldier)
    {
        // monster AI - heard something 
        WorldDirections ubNoiseDir;
        int sNoiseGridNo;
        int iNoiseValue;
        int iChance, iSneaky;
        bool fClimb;
        bool fReachable;
        //	int sClosestFriend;

        if (pSoldier.bMobility == CREATURE.CRAWLER && pSoldier.bActionPoints < pSoldier.bInitialActionPoints)
        {
            return (AI_ACTION.NONE);
        }

        // determine the most important noise heard, and its relative value
        sNoiseGridNo = MostImportantNoiseHeard(pSoldier, out iNoiseValue, out fClimb, out fReachable);
        //NumMessage("iNoiseValue = ",iNoiseValue);

        if (sNoiseGridNo == NOWHERE)
        {
            // then we have no business being under YELLOW status any more!
            return (AI_ACTION.NONE);
        }

        ////////////////////////////////////////////////////////////////////////////
        // LOOK AROUND TOWARD NOISE: determine %chance for man to turn towards noise
        ////////////////////////////////////////////////////////////////////////////

        if (pSoldier.bMobility != CREATURE.IMMOBILE)
        {
            // determine direction from this soldier in which the noise lies
            ubNoiseDir = SoldierControl.atan8(IsometricUtils.CenterX(pSoldier.sGridNo), IsometricUtils.CenterY(pSoldier.sGridNo), IsometricUtils.CenterX(sNoiseGridNo), IsometricUtils.CenterY(sNoiseGridNo));

            // if soldier is not already facing in that direction,
            // and the noise source is close enough that it could possibly be seen
            if ((GetAPsToLook(pSoldier) <= pSoldier.bActionPoints)
                && (pSoldier.bDirection != ubNoiseDir)
                && IsometricUtils.PythSpacesAway(pSoldier.sGridNo, sNoiseGridNo) <= STRAIGHT)
            {
                // set base chance according to orders
                if ((pSoldier.bOrders == Orders.STATIONARY) || (pSoldier.bOrders == Orders.ONGUARD))
                {
                    iChance = 60;
                }
                else           // all other orders
                {
                    iChance = 35;
                }

                if (pSoldier.bAttitude == Attitudes.DEFENSIVE)
                {
                    iChance += 15;
                }

                if ((int)PreRandom(100) < iChance)
                {
                    pSoldier.usActionData = ubNoiseDir;
                    //if ( InternalIsValidStance( pSoldier, (int) pSoldier.usActionData, ANIM_STAND ) )
                    if (ValidCreatureTurn(pSoldier, (int)pSoldier.usActionData))
                    {
                        return (AI_ACTION.CHANGE_FACING);
                    }
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////
        // REST IF RUNNING OUT OF BREATH
        ////////////////////////////////////////////////////////////////////////

        // if our breath is running a bit low, and we're not in water
        if ((pSoldier.bBreath < 25) /*&& !MercInWater(pSoldier) */ )
        {
            // take a breather for gods sake!
            pSoldier.usActionData = NOWHERE;
            return (AI_ACTION.NONE);
        }

        if (pSoldier.bMobility != CREATURE.IMMOBILE && fReachable)
        {
            ////////////////////////////////////////////////////////////////////////////
            // SEEK NOISE
            ////////////////////////////////////////////////////////////////////////////

            // remember that noise value is negative, and closer to 0 => more important!
            iChance = 75 + iNoiseValue;
            iSneaky = 30;

            // set base chance according to orders
            switch (pSoldier.bOrders)
            {
                case Orders.STATIONARY:
                    iChance += -20;
                    break;
                case Orders.ONGUARD:
                    iChance += -15;
                    break;
                case Orders.ONCALL:
                    break;
                case Orders.CLOSEPATROL:
                    iChance += -10;
                    break;
                case Orders.RNDPTPATROL:
                case Orders.POINTPATROL:
                    break;
                case Orders.FARPATROL:
                    iChance += 10;
                    break;
                case Orders.SEEKENEMY:
                    iChance += 25;
                    break;
            }

            // modify chance of patrol (and whether it's a sneaky one) by attitude
            switch (pSoldier.bAttitude)
            {
                case Attitudes.DEFENSIVE:
                    iChance += -10;
                    iSneaky += 15;
                    break;
                case Attitudes.BRAVESOLO:
                    iChance += 10;
                    break;
                case Attitudes.BRAVEAID:
                    iChance += 5;
                    break;
                case Attitudes.CUNNINGSOLO:
                    iChance += 5;
                    iSneaky += 30;
                    break;
                case Attitudes.CUNNINGAID:
                    iSneaky += 30;
                    break;
                case Attitudes.AGGRESSIVE:
                    iChance += 20;
                    iSneaky += -10;
                    break;
            }

            // reduce chance if breath is down, less likely to wander around when tired
            iChance -= (100 - pSoldier.bBreath);

            if (PreRandom(100) < iChance)
            {
                pSoldier.usActionData = GoAsFarAsPossibleTowards(pSoldier, sNoiseGridNo, AI_ACTION.SEEK_NOISE);

                if ((int)pSoldier.usActionData != NOWHERE)
                {
                    return (AI_ACTION.SEEK_NOISE);
                }
            }
            // Okay, we're not following up on the noise... but let's follow any
            // scent trails available
            if (TrackScent(pSoldier))
            {
                return (AI_ACTION.TRACK);
            }
        }



        ////////////////////////////////////////////////////////////////////////////
        // DO NOTHING: Not enough points left to move, so save them for next turn
        ////////////////////////////////////////////////////////////////////////////

        // by default, if everything else fails, just stands in place without turning
        pSoldier.usActionData = NOWHERE;
        return (AI_ACTION.NONE);
    }

    public static AI_ACTION CreatureDecideActionRed(SOLDIERTYPE pSoldier, int ubUnconsciousOK)
    {
        // monster AI - hostile mammals somewhere around!
        int iChance, sClosestOpponent /*,sClosestOpponent,sClosestFriend*/;
        int sClosestDisturbance;
        int sDistVisible;
        int ubCanMove;
        WorldDirections ubOpponentDir;
        //int bInWater;
        bool bInGas;
        int bSeekPts = 0, bHelpPts = 0, bHidePts = 0;
        int sAdjustedGridNo;
        bool fChangeLevel;

        // if we have absolutely no action points, we can't do a thing under RED!
        if (pSoldier.bActionPoints == 0)
        {
            pSoldier.usActionData = NOWHERE;
            return (AI_ACTION.NONE);
        }

        if (pSoldier.bMobility == CREATURE.CRAWLER && pSoldier.bActionPoints < pSoldier.bInitialActionPoints)
        {
            return (AI_ACTION.NONE);
        }


        // can this guy move to any of the neighbouring squares ? (sets true/false)
        ubCanMove = ((pSoldier.bMobility != CREATURE.IMMOBILE) && (pSoldier.bActionPoints >= MinPtsToMove(pSoldier)));

        // determine if we happen to be in water (in which case we're in BIG trouble!)
        //bInWater = MercInWater(pSoldier);

        // check if standing in tear gas without a gas mask on
        bInGas = AIUtils.InGas(pSoldier, pSoldier.sGridNo);


        ////////////////////////////////////////////////////////////////////////////
        // WHEN IN GAS, GO TO NEAREST REACHABLE SPOT OF UNGASSED LAND
        ////////////////////////////////////////////////////////////////////////////

        if (bInGas && ubCanMove > 0)
        {
            pSoldier.usActionData = FindNearestUngassedLand(pSoldier);

            if ((int)pSoldier.usActionData != NOWHERE)
            {
                return (AI_ACTION.LEAVE_WATER_GAS);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        // CALL FOR AID IF HURT
        ////////////////////////////////////////////////////////////////////////////
        if (CAN_CALL(pSoldier))
        {
            if ((pSoldier.bActionPoints >= AP.RADIO) && (gTacticalStatus.Team[pSoldier.bTeam].bMenInSector > 1))
            {
                if (pSoldier.bLife < pSoldier.bOldLife)
                {
                    // got injured, maybe call
                    if ((pSoldier.bOldLife == pSoldier.bLifeMax) && (pSoldier.bOldLife - pSoldier.bLife > 10))
                    {
                        // hurt for first time!
                        pSoldier.usActionData = CALL.CRIPPLED;
                        pSoldier.bOldLife = pSoldier.bLife;  // don't want to call more than once	
                        return (AI_ACTION.CREATURE_CALL);
                    }
                    else if (pSoldier.bLifeMax / pSoldier.bLife > 2)
                    {
                        // crippled, 1/3 or less health!
                        pSoldier.usActionData = CALL.ATTACKED;
                        pSoldier.bOldLife = pSoldier.bLife;  // don't want to call more than once	
                        return (AI_ACTION.CREATURE_CALL);
                    }
                }
            }
        }


        ////////////////////////////////////////////////////////////////////////
        // CROUCH & REST IF RUNNING OUT OF BREATH
        ////////////////////////////////////////////////////////////////////////

        // if our breath is running a bit low, and we're not in water or under fire
        if ((pSoldier.bBreath < 25) /*&& !bInWater*/ && pSoldier.bUnderFire == 0)
        {
            pSoldier.usActionData = NOWHERE;
            return (AI_ACTION.NONE);
        }

        ////////////////////////////////////////////////////////////////////////////
        // CALL IN SIGHTING: determine %chance to call others and report contact
        ////////////////////////////////////////////////////////////////////////////

        // if we're a computer merc, and we have the action points remaining to RADIO
        // (we never want NPCs to choose to radio if they would have to wait a turn)
        if (CAN_CALL(pSoldier) && (gTacticalStatus.Team[pSoldier.bTeam].bAwareOfOpposition == 0))
        {
            if ((pSoldier.bActionPoints >= AP.RADIO) && (gTacticalStatus.Team[pSoldier.bTeam].bMenInSector > 1))
            {
                // if there hasn't been a general sighting call sent yet

                // might want to check the specifics of who we see 
                iChance = 20;

                if (iChance > 0)
                {
                    if ((int)PreRandom(100) < iChance)
                    {
                        pSoldier.usActionData = CALL.SINGLE_PREY;
                        return (AI_ACTION.CREATURE_CALL);
                    }
                }
            }
        }

        if (pSoldier.bMobility != CREATURE.IMMOBILE)
        {
            if (FindAIUsableObjClass(pSoldier, IC.WEAPON) == ITEM_NOT_FOUND)
            {
                // probably a baby bug... run away! run away!
                // look for best place to RUN AWAY to (farthest from the closest threat)
                pSoldier.usActionData = FindSpotMaxDistFromOpponents(pSoldier);

                if ((int)pSoldier.usActionData != NOWHERE)
                {
                    return (AI_ACTION.RUN_AWAY);
                }
                else
                {
                    return (AI_ACTION.NONE);
                }

            }

            // Respond to call if any
            if (CAN_LISTEN_TO_CALL(pSoldier) && pSoldier.ubCaller != NOBODY)
            {
                if (IsometricUtils.PythSpacesAway(pSoldier.sGridNo, pSoldier.sCallerGridNo) <= STOPSHORTDIST)
                {
                    // call completed... hmm, nothing found
                    pSoldier.ubCaller = NOBODY;
                }
                else
                {
                    pSoldier.usActionData = InternalGoAsFarAsPossibleTowards(pSoldier, pSoldier.sCallerGridNo, -1, AI_ACTION.SEEK_FRIEND, FLAG.STOPSHORT);

                    if ((int)pSoldier.usActionData != NOWHERE)
                    {
                        return (AI_ACTION.SEEK_FRIEND);
                    }
                }
            }

            // get the location of the closest reachable opponent
            sClosestDisturbance = ClosestReachableDisturbance(pSoldier, ubUnconsciousOK, out fChangeLevel);
            // if there is an opponent reachable
            if (sClosestDisturbance != NOWHERE)
            {
                //////////////////////////////////////////////////////////////////////
                // SEEK CLOSEST DISTURBANCE: GO DIRECTLY TOWARDS CLOSEST KNOWN OPPONENT
                //////////////////////////////////////////////////////////////////////

                // try to move towards him
                pSoldier.usActionData = GoAsFarAsPossibleTowards(pSoldier, sClosestDisturbance, AI_ACTION.SEEK_OPPONENT);

                // if it's possible
                if ((int)pSoldier.usActionData != NOWHERE)
                {
                    return (AI_ACTION.SEEK_OPPONENT);
                }
            }

            ////////////////////////////////////////////////////////////////////////////
            // TAKE A BITE, PERHAPS
            ////////////////////////////////////////////////////////////////////////////		
            if (pSoldier.bHunting > 0)
            {
                pSoldier.usActionData = FindNearestRottingCorpse(pSoldier);
                // need smell/visibility check?
                if (IsometricUtils.PythSpacesAway(pSoldier.sGridNo, (int)pSoldier.usActionData) < MAX_EAT_DIST)
                {
                    int sGridNo;

                    sGridNo = FindAdjacentGridEx(pSoldier, pSoldier.usActionData, out ubOpponentDir, out sAdjustedGridNo, false, false);

                    if (sGridNo != -1)
                    {
                        pSoldier.usActionData = sGridNo;
                        return (AI_ACTION.APPROACH_MERC);
                    }
                }
            }

            ////////////////////////////////////////////////////////////////////////////
            // TRACK A SCENT, IF ONE IS PRESENT
            ////////////////////////////////////////////////////////////////////////////		
            if (TrackScent(pSoldier))
            {
                return (AI_ACTION.TRACK);
            }


            ////////////////////////////////////////////////////////////////////////////
            // LOOK AROUND TOWARD CLOSEST KNOWN OPPONENT, IF KNOWN
            ////////////////////////////////////////////////////////////////////////////
            if (GetAPsToLook(pSoldier) <= pSoldier.bActionPoints)
            {
                // determine the location of the known closest opponent
                // (don't care if he's conscious, don't care if he's reachable at all)
                sClosestOpponent = ClosestKnownOpponent(pSoldier, null, null);

                if (sClosestOpponent != NOWHERE)
                {
                    // determine direction from this soldier to the closest opponent
                    ubOpponentDir = SoldierControl.atan8(IsometricUtils.CenterX(pSoldier.sGridNo), IsometricUtils.CenterY(pSoldier.sGridNo), IsometricUtils.CenterX(sClosestOpponent), IsometricUtils.CenterY(sClosestOpponent));

                    // if soldier is not already facing in that direction,
                    // and the opponent is close enough that he could possibly be seen
                    // note, have to change this to use the level returned from ClosestKnownOpponent
                    sDistVisible = OppList.DistanceVisible(pSoldier, WorldDirections.DIRECTION_IRRELEVANT, WorldDirections.DIRECTION_IRRELEVANT, sClosestOpponent, 0);

                    if ((pSoldier.bDirection != ubOpponentDir) && (IsometricUtils.PythSpacesAway(pSoldier.sGridNo, sClosestOpponent) <= sDistVisible))
                    {
                        // set base chance according to orders
                        if ((pSoldier.bOrders == Orders.STATIONARY) || (pSoldier.bOrders == Orders.ONGUARD))
                        {
                            iChance = 50;
                        }
                        else           // all other orders
                        {
                            iChance = 25;
                        }

                        if (pSoldier.bAttitude == Attitudes.DEFENSIVE)
                        {
                            iChance += 25;
                        }

                        //if ( (int)PreRandom(100) < iChance && InternalIsValidStance( pSoldier, ubOpponentDir, ANIM_STAND ) )
                        if ((int)PreRandom(100) < iChance && ValidCreatureTurn(pSoldier, ubOpponentDir))
                        {
                            pSoldier.usActionData = ubOpponentDir;
                            return (AI_ACTION.CHANGE_FACING);
                        }
                    }
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        // LEAVE THE SECTOR
        ////////////////////////////////////////////////////////////////////////////

        // NOT IMPLEMENTED

        ////////////////////////////////////////////////////////////////////////////
        // DO NOTHING: Not enough points left to move, so save them for next turn
        ////////////////////////////////////////////////////////////////////////////


        pSoldier.usActionData = NOWHERE;

        return (AI_ACTION.NONE);
    }


    public static AI_ACTION CreatureDecideActionBlack(SOLDIERTYPE? pSoldier)
    {
        // monster AI - hostile mammals in sense range
        int sClosestOpponent, sBestCover = NOWHERE;
        int sClosestDisturbance;
        int ubMinAPCost, ubCanMove/*,bInWater*/;
        bool bInGas;
        WorldDirections bDirection;
        AI_ACTION ubBestAttackAction;
        int bCanAttack;
        InventorySlot bSpitIn;
        InventorySlot bWeaponIn;
        int uiChance;
        ATTACKTYPE BestShot, BestStab, BestAttack, CurrStab;
        bool fRunAway = false;
        bool fChangeLevel;

        // if we have absolutely no action points, we can't do a thing under BLACK!
        if (pSoldier.bActionPoints == 0)
        {
            pSoldier.usActionData = NOWHERE;
            return (AI_ACTION.NONE);
        }

        if (pSoldier.bMobility == CREATURE.CRAWLER && pSoldier.bActionPoints < pSoldier.bInitialActionPoints)
        {
            return (AI_ACTION.NONE);
        }

        ////////////////////////////////////////////////////////////////////////////
        // CALL FOR AID IF HURT OR IF OTHERS ARE UNAWARE
        ////////////////////////////////////////////////////////////////////////////

        if (CAN_CALL(pSoldier))
        {
            if ((pSoldier.bActionPoints >= AP.RADIO) && (gTacticalStatus.Team[pSoldier.bTeam].bMenInSector > 1))
            {
                if (pSoldier.bLife < pSoldier.bOldLife)
                {
                    // got injured, maybe call
                    /*
                    // don't call when crippled and have target... save breath for attacking!
                    if ((pSoldier.bOldLife == pSoldier.bLifeMax) && (pSoldier.bOldLife - pSoldier.bLife > 10))
                    {
                        // hurt for first time!
                        pSoldier.usActionData = CALL_CRIPPLED;
                        pSoldier.bOldLife = pSoldier.bLife;  // don't want to call more than once	
                        return(AI_ACTION_CREATURE_CALL);
                    }
                    else 
                    */
                    if (pSoldier.bLifeMax / pSoldier.bLife > 2)
                    {
                        // crippled, 1/3 or less health!
                        pSoldier.usActionData = CALL.ATTACKED;
                        pSoldier.bOldLife = pSoldier.bLife;  // don't want to call more than once	
                        return (AI_ACTION.CREATURE_CALL);
                    }
                }
                else
                {
                    if (!(gTacticalStatus.Team[pSoldier.bTeam].bAwareOfOpposition > 0))
                    {
                        if (pSoldier.ubBodyType == SoldierBodyTypes.QUEENMONSTER)
                        {
                            uiChance = 100;
                        }
                        else
                        {
                            uiChance = 20 * pSoldier.bOppCnt;
                        }
                        if (Globals.Random.Next(100) < uiChance)
                        {
                            // alert! alert!
                            if (pSoldier.bOppCnt > 1)
                            {
                                pSoldier.usActionData = CALL.MULTIPLE_PREY;
                            }
                            else
                            {
                                pSoldier.usActionData = CALL.SINGLE_PREY;
                            }
                            return (AI_ACTION.CREATURE_CALL);
                        }
                    }
                }
            }
        }

        // can this guy move to any of the neighbouring squares ? (sets true/false)
        ubCanMove = ((pSoldier.bMobility != CREATURE.IMMOBILE) && (pSoldier.bActionPoints >= MinPtsToMove(pSoldier)));

        // determine if we happen to be in water (in which case we're in BIG trouble!)
        //bInWater = MercInWater(pSoldier);

        // check if standing in tear gas without a gas mask on
        bInGas = AIUtils.InGas(pSoldier, pSoldier.sGridNo);


        ////////////////////////////////////////////////////////////////////////////
        // IF GASSED, OR REALLY TIRED (ON THE VERGE OF COLLAPSING), TRY TO RUN AWAY
        ////////////////////////////////////////////////////////////////////////////

        // if we're desperately short on breath (it's OK if we're in water, though!)
        if (bInGas || (pSoldier.bBreath < 5))
        {
            // if soldier has enough APs left to move at least 1 square's worth
            if (ubCanMove > 0)
            {
                // look for best place to RUN AWAY to (farthest from the closest threat)
                pSoldier.usActionData = FindSpotMaxDistFromOpponents(pSoldier);

                if ((int)pSoldier.usActionData != NOWHERE)
                {
                    return (AI_ACTION.RUN_AWAY);
                }
            }

        }


        ////////////////////////////////////////////////////////////////////////////
        // STUCK IN WATER OR GAS, NO COVER, GO TO NEAREST SPOT OF UNGASSED LAND
        ////////////////////////////////////////////////////////////////////////////

        // if soldier in water/gas has enough APs left to move at least 1 square
        if ((/*bInWater ||*/ bInGas) && ubCanMove > 0)
        {
            pSoldier.usActionData = FindNearestUngassedLand(pSoldier);

            if ((int)pSoldier.usActionData != NOWHERE)
            {
                return (AI_ACTION.LEAVE_WATER_GAS);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        // SOLDIER CAN ATTACK IF NOT IN WATER/GAS AND NOT DOING SOMETHING TOO FUNKY
        ////////////////////////////////////////////////////////////////////////////

        // NPCs in water/tear gas without masks are not permitted to shoot/stab/throw
        if ((pSoldier.bActionPoints < 2) /*|| bInWater*/ || bInGas)
        {
            bCanAttack = 0;
        }
        else
        {
            bCanAttack = CanNPCAttack(pSoldier);
            if (bCanAttack != true)
            {
                if (bCanAttack == NOSHOOT_NOAMMO)
                {
                    pSoldier.inv[InventorySlot.HANDPOS].fFlags |= OBJECT.AI_UNUSABLE;

                    // try to find a bladed weapon
                    if (pSoldier.ubBodyType == SoldierBodyTypes.QUEENMONSTER)
                    {
                        bWeaponIn = ItemSubSystem.FindObjClass(pSoldier, IC.TENTACLES);
                    }
                    else
                    {
                        bWeaponIn = ItemSubSystem.FindObjClass(pSoldier, IC.BLADE);
                    }

                    if (bWeaponIn != NO_SLOT)
                    {
                        AIUtils.RearrangePocket(pSoldier, InventorySlot.HANDPOS, bWeaponIn, FOREVER);
                        bCanAttack = 1;
                    }
                    else
                    {
                        // infants who exhaust their spit should flee!
                        fRunAway = true;
                        bCanAttack = 0;
                    }

                }
                else
                {
                    bCanAttack = 0;
                }

            }
        }


        BestShot.ubPossible = 0;    // by default, assume Shooting isn't possible
        BestStab.ubPossible = 0;    // by default, assume Stabbing isn't possible

        BestAttack.ubChanceToReallyHit = 0;

        bSpitIn = NO_SLOT;


        // if we are able attack
        if (bCanAttack > 0)
        {
            //////////////////////////////////////////////////////////////////////////
            // FIRE A GUN AT AN OPPONENT
            //////////////////////////////////////////////////////////////////////////

            pSoldier.bAimShotLocation = AIM_SHOT_RANDOM;

            bWeaponIn = ItemSubSystem.FindObjClass(pSoldier, IC.GUN);

            if (bWeaponIn != NO_SLOT)
            {
                if (Item[pSoldier.inv[bWeaponIn].usItem].usItemClass == IC.GUN && pSoldier.inv[bWeaponIn].bGunStatus >= USABLE)
                {
                    if (pSoldier.inv[bWeaponIn].ubGunShotsLeft > 0)
                    {
                        bSpitIn = bWeaponIn;
                        // if it's in another pocket, swap it into his hand temporarily
                        if (bWeaponIn != InventorySlot.HANDPOS)
                        {
                            AIUtils.RearrangePocket(pSoldier, InventorySlot.HANDPOS, bWeaponIn, TEMPORARILY);
                        }

                        // now it better be a gun, or the guy can't shoot (but has other attack(s))

                        // get the minimum cost to attack the same target with this gun
                        ubMinAPCost = MinAPsToAttack(pSoldier, pSoldier.sLastTarget, DONTADDTURNCOST);

                        // if we have enough action points to shoot with this gun
                        if (pSoldier.bActionPoints >= ubMinAPCost)
                        {
                            // look around for a worthy target (which sets BestShot.ubPossible)
                            CalcBestShot(pSoldier, out BestShot);

                            if (BestShot.ubPossible > 0)
                            {
                                BestShot.bWeaponIn = bWeaponIn;

                                // if the selected opponent is not a threat (unconscious & !serviced)
                                // (usually, this means all the guys we see our unconscious, but, on
                                //  rare occasions, we may not be able to shoot a healthy guy, too)
                                if ((Menptr[BestShot.ubOpponent].bLife < OKLIFE) &&
                                    Menptr[BestShot.ubOpponent].bService == 0)
                                {
                                    // if our attitude is NOT aggressive
                                    if (pSoldier.bAttitude != Attitudes.AGGRESSIVE)
                                    {
                                        // get the location of the closest CONSCIOUS reachable opponent
                                        sClosestDisturbance = ClosestReachableDisturbance(pSoldier, false, fChangeLevel);

                                        // if we found one
                                        if (sClosestDisturbance != NOWHERE)
                                        {
                                            // don't bother checking GRENADES/KNIVES, he can't have conscious targets
                                            // then make decision as if at alert status RED, but make sure
                                            // we don't try to SEEK OPPONENT the unconscious guy!
                                            return (DecideActionRed(pSoldier, false));
                                        }
                                        // else kill the guy, he could be the last opponent alive in this sector
                                    }
                                    // else aggressive guys will ALWAYS finish off unconscious opponents
                                }

                                // now we KNOW FOR SURE that we will do something (shoot, at least)
                                AIMain.NPCDoesAct(pSoldier);

                            }
                        }
                        // if it was in his holster, swap it back into his holster for now
                        if (bWeaponIn != InventorySlot.HANDPOS)
                        {
                            AIUtils.RearrangePocket(pSoldier, InventorySlot.HANDPOS, bWeaponIn, TEMPORARILY);
                        }
                    }
                    else
                    {
                        // out of ammo! reload if possible!
                    }

                }

            }

            //////////////////////////////////////////////////////////////////////////
            // GO STAB AN OPPONENT WITH A KNIFE
            //////////////////////////////////////////////////////////////////////////

            // if soldier has a knife in his hand
            if (pSoldier.ubBodyType == SoldierBodyTypes.QUEENMONSTER)
            {
                bWeaponIn = ItemSubSystem.FindObjClass(pSoldier, IC.TENTACLES);
            }
            else if (pSoldier.ubBodyType == SoldierBodyTypes.BLOODCAT)
            {
                // 1 in 3 attack with teeth, otherwise with claws
                if (PreRandom(3) > 0)
                {
                    bWeaponIn = ItemSubSystem.FindObj(pSoldier, Items.BLOODCAT_CLAW_ATTACK);
                }
                else
                {
                    bWeaponIn = ItemSubSystem.FindObj(pSoldier, Items.BLOODCAT_BITE);
                }
            }
            else
            {
                if (bSpitIn != NO_SLOT && Globals.Random.Next(4) > 0)
                {
                    // spitters only consider a blade attack 1 time in 4
                    bWeaponIn = NO_SLOT;
                }
                else
                {
                    bWeaponIn = ItemSubSystem.FindObjClass(pSoldier, IC.BLADE);
                }
            }



            BestStab.iAttackValue = 0;

            // if the soldier does have a usable knife somewhere

            // spitters don't always consider using their claws
            if (bWeaponIn != NO_SLOT)
            {
                // if it's in his holster, swap it into his hand temporarily
                if (bWeaponIn != InventorySlot.HANDPOS)
                {
                    AIUtils.RearrangePocket(pSoldier, InventorySlot.HANDPOS, bWeaponIn, TEMPORARILY);
                }

                // get the minimum cost to attack with this knife
                ubMinAPCost = MinAPsToAttack(pSoldier, pSoldier.sLastTarget, DONTADDTURNCOST);

                //sprintf(tempstr,"%s - ubMinAPCost = %d",pSoldier.name,ubMinAPCost);
                //PopMessage(tempstr);

                // if we can afford the minimum AP cost to stab with this knife weapon
                if (pSoldier.bActionPoints >= ubMinAPCost)
                {
                    // then look around for a worthy target (which sets BestStab.ubPossible)

                    if (pSoldier.ubBodyType == SoldierBodyTypes.QUEENMONSTER)
                    {
                        CalcTentacleAttack(pSoldier, out CurrStab);
                    }
                    else
                    {
                        CalcBestStab(pSoldier, out CurrStab, true);
                    }

                    if (CurrStab.ubPossible > 0)
                    {
                        // now we KNOW FOR SURE that we will do something (stab, at least)
                        AIMain.NPCDoesAct(pSoldier);
                    }

                    // if it was in his holster, swap it back into his holster for now
                    if (bWeaponIn != InventorySlot.HANDPOS)
                    {
                        AIUtils.RearrangePocket(pSoldier, InventorySlot.HANDPOS, bWeaponIn, TEMPORARILY);
                    }

                    if (CurrStab.iAttackValue > BestStab.iAttackValue)
                    {
                        CurrStab.bWeaponIn = bWeaponIn;
                        //memcpy(&BestStab, &CurrStab, sizeof(BestStab));
                    }

                }

            }

            //////////////////////////////////////////////////////////////////////////
            // CHOOSE THE BEST TYPE OF ATTACK OUT OF THOSE FOUND TO BE POSSIBLE
            //////////////////////////////////////////////////////////////////////////
            if (BestShot.ubPossible > 0)
            {
                BestAttack.iAttackValue = BestShot.iAttackValue;
                ubBestAttackAction = AI_ACTION.FIRE_GUN;
            }
            else
            {
                BestAttack.iAttackValue = 0;
                ubBestAttackAction = AI_ACTION.NONE;
            }
            if (BestStab.ubPossible && BestStab.iAttackValue > (BestAttack.iAttackValue * 12) / 10)
            {
                BestAttack.iAttackValue = BestStab.iAttackValue;
                ubBestAttackAction = AI_ACTION.KNIFE_MOVE;
            }

            // if attack is still desirable (meaning it's also preferred to taking cover)
            if (ubBestAttackAction != AI_ACTION.NONE)
            {
                // copy the information on the best action selected into BestAttack struct
                switch (ubBestAttackAction)
                {
                    case AI_ACTION.FIRE_GUN:
                        // memcpy(&BestAttack, &BestShot, sizeof(BestAttack));
                        BestAttack = BestShot;
                        break;

                    case AI_ACTION.KNIFE_MOVE:
                        // memcpy(&BestAttack, &BestStab, sizeof(BestAttack));
                        BestAttack = BestStab;
                        break;

                }

                // if necessary, swap the weapon into the hand position
                if (BestAttack.bWeaponIn != InventorySlot.HANDPOS)
                {
                    // IS THIS NOT BEING SET RIGHT?????
                    AIUtils.RearrangePocket(pSoldier, InventorySlot.HANDPOS, BestAttack.bWeaponIn, FOREVER);
                }

                //////////////////////////////////////////////////////////////////////////
                // GO AHEAD & ATTACK!
                //////////////////////////////////////////////////////////////////////////

                pSoldier.usActionData = BestAttack.sTarget;
                pSoldier.bAimTime = BestAttack.ubAimTime;

                if (ubBestAttackAction == AI_ACTION.FIRE_GUN && BestAttack.ubChanceToReallyHit > 50)
                {
                    pSoldier.bAimShotLocation = AIM_SHOT_HEAD;
                }
                else
                {
                    pSoldier.bAimShotLocation = AIM_SHOT_RANDOM;
                }

                return (ubBestAttackAction);
            }
        }



        ////////////////////////////////////////////////////////////////////////////
        // CLOSE ON THE CLOSEST KNOWN OPPONENT or TURN TO FACE HIM
        ////////////////////////////////////////////////////////////////////////////

        if (!fRunAway)
        {
            if ((GetAPsToLook(pSoldier) <= pSoldier.bActionPoints))
            {
                // determine the location of the known closest opponent
                // (don't care if he's conscious, don't care if he's reachable at all)	 
                sClosestOpponent = ClosestKnownOpponent(pSoldier, null, null);
                // if we have a closest reachable opponent
                if (sClosestOpponent != NOWHERE)
                {
                    if (ubCanMove > 0 && IsometricUtils.PythSpacesAway(pSoldier.sGridNo, sClosestOpponent) > 2)
                    {
                        if (bSpitIn != NO_SLOT)
                        {
                            pSoldier.usActionData = AdvanceToFiringRange(pSoldier, sClosestOpponent);
                            if (pSoldier.usActionData == NOWHERE)
                            {
                                pSoldier.usActionData = GoAsFarAsPossibleTowards(pSoldier, sClosestOpponent, AI_ACTION.SEEK_OPPONENT);
                            }
                        }
                        else
                        {
                            pSoldier.usActionData = GoAsFarAsPossibleTowards(pSoldier, sClosestOpponent, AI_ACTION.SEEK_OPPONENT);
                        }
                    }
                    else
                    {
                        pSoldier.usActionData = NOWHERE;
                    }

                    if ((int)pSoldier.usActionData != NOWHERE) // charge!
                    {
                        return (AI_ACTION.SEEK_OPPONENT);
                    }
                    else if (GetAPsToLook(pSoldier) <= pSoldier.bActionPoints) // turn to face enemy
                    {
                        bDirection = SoldierControl.atan8(IsometricUtils.CenterX(pSoldier.sGridNo), IsometricUtils.CenterY(pSoldier.sGridNo), IsometricUtils.CenterX(sClosestOpponent), IsometricUtils.CenterY(sClosestOpponent));

                        // if we're not facing towards him
                        if (pSoldier.bDirection != bDirection && ValidCreatureTurn(pSoldier, bDirection))
                        {
                            pSoldier.usActionData = bDirection;

                            return (AI_ACTION.CHANGE_FACING);
                        }
                    }
                }
            }
        }
        else
        {
            // run away!
            if (ubCanMove > 0)
            {
                // look for best place to RUN AWAY to (farthest from the closest threat)
                //pSoldier.usActionData = RunAway( pSoldier );
                pSoldier.usActionData = FindSpotMaxDistFromOpponents(pSoldier);

                if ((int)pSoldier.usActionData != NOWHERE)
                {
                    return (AI_ACTION.RUN_AWAY);
                }
            }

        }
        ////////////////////////////////////////////////////////////////////////////
        // DO NOTHING: Not enough points left to move, so save them for next turn
        ////////////////////////////////////////////////////////////////////////////

        // by default, if everything else fails, just stand in place and wait
        pSoldier.usActionData = NOWHERE;
        return (AI_ACTION.NONE);
    }

    AI_ACTION CreatureDecideAction(SOLDIERTYPE? pSoldier)
    {
        AI_ACTION bAction = AI_ACTION.NONE;

        switch (pSoldier.bAlertStatus)
        {
            case STATUS.GREEN:
                bAction = CreatureDecideActionGreen(pSoldier);
                break;

            case STATUS.YELLOW:
                bAction = CreatureDecideActionYellow(pSoldier);
                break;

            case STATUS.RED:
                bAction = CreatureDecideActionRed(pSoldier, 1);
                break;

            case STATUS.BLACK:
                bAction = CreatureDecideActionBlack(pSoldier);
                break;
        }

        return (bAction);
    }

    public static void CreatureDecideAlertStatus(SOLDIERTYPE pSoldier)
    {
        STATUS bOldStatus;
        int iDummy;
        bool fClimbDummy, fReachableDummy;

        // THE FOUR (4) POSSIBLE ALERT STATUSES ARE:
        // GREEN - No one sensed, no suspicious noise heard, go about doing regular stuff
        // YELLOW - Suspicious noise was heard personally
        // RED - Either saw OPPONENTS in person, or definite contact had been called
        // BLACK - Currently has one or more OPPONENTS in sight

        // set mobility
        switch (pSoldier.ubBodyType)
        {
            case SoldierBodyTypes.ADULTFEMALEMONSTER:
            case SoldierBodyTypes.YAF_MONSTER:
            case SoldierBodyTypes.AM_MONSTER:
            case SoldierBodyTypes.YAM_MONSTER:
            case SoldierBodyTypes.INFANT_MONSTER:
                pSoldier.bMobility = CREATURE.MOBILE;
                break;
            case SoldierBodyTypes.QUEENMONSTER:
                pSoldier.bMobility = CREATURE.IMMOBILE;
                break;
            case SoldierBodyTypes.LARVAE_MONSTER:
                pSoldier.bMobility = CREATURE.CRAWLER;
                break;
        }


        if (pSoldier.ubBodyType == SoldierBodyTypes.LARVAE_MONSTER)
        {
            // larvae never do anything much!
            pSoldier.bAlertStatus = STATUS.GREEN;
            return;
        }

        // save the man's previous status
        bOldStatus = pSoldier.bAlertStatus;

        // determine the current alert status for this category of man
        if (pSoldier.bOppCnt > 0)        // opponent(s) in sight
        {
            // must search through list of people to see if any of them have
            // attacked us, or do some check to see if we have been attacked
            switch (bOldStatus)
            {
                case STATUS.GREEN:
                case STATUS.YELLOW:
                    pSoldier.bAlertStatus = STATUS.BLACK;
                    break;
                case STATUS.RED:
                case STATUS.BLACK:
                    pSoldier.bAlertStatus = STATUS.BLACK;
                    break;
            }

        }
        else // no opponents are in sight
        {
            switch (bOldStatus)
            {
                case STATUS.BLACK:
                    // then drop back to RED status
                    pSoldier.bAlertStatus = STATUS.RED;
                    break;

                case STATUS.RED:
                    // RED can never go back down below RED, only up to BLACK
                    break;

                case STATUS.YELLOW:
                    // if all enemies have been RED alerted, or we're under fire
                    if (gTacticalStatus.Team[pSoldier.bTeam].bAwareOfOpposition > 0 || pSoldier.bUnderFire > 0)
                    {
                        pSoldier.bAlertStatus = STATUS.RED;
                    }
                    else
                    {
                        // if we are NOT aware of any uninvestigated noises right now
                        // and we are not currently in the middle of an action
                        // (could still be on his way heading to investigate a noise!)
                        if ((MostImportantNoiseHeard(pSoldier, out iDummy, out fClimbDummy, out fReachableDummy) == NOWHERE) && !pSoldier.bActionInProgress)
                        {
                            // then drop back to GREEN status
                            pSoldier.bAlertStatus = STATUS.GREEN;
                        }
                    }
                    break;

                case STATUS.GREEN:
                    // if all enemies have been RED alerted, or we're under fire
                    if (gTacticalStatus.Team[pSoldier.bTeam].bAwareOfOpposition > 0 || pSoldier.bUnderFire > 0)
                    {
                        pSoldier.bAlertStatus = STATUS.RED;
                    }
                    else
                    {
                        // if we ARE aware of any uninvestigated noises right now
                        if (MostImportantNoiseHeard(pSoldier, out iDummy, out fClimbDummy, out fReachableDummy) != NOWHERE)
                        {
                            // then move up to YELLOW status
                            pSoldier.bAlertStatus = STATUS.YELLOW;
                        }
                    }
                    break;
            }
            // otherwise, RED stays RED, YELLOW stays YELLOW, GREEN stays GREEN
        }

        // if the creatures alert status has changed in any way
        if (pSoldier.bAlertStatus != bOldStatus)
        {
            // HERE ARE TRYING TO AVOID NPCs SHUFFLING BACK & FORTH BETWEEN RED & BLACK
            // if either status is < RED (ie. anything but RED.BLACK && BLACK.RED)
            if ((bOldStatus < STATUS.RED) || (pSoldier.bAlertStatus < STATUS.RED))
            {
                // force a NEW action decision on next pass through HandleManAI()
                SetNewSituation(pSoldier);
            }

            // if this guy JUST discovered that there were opponents here for sure...
            if ((bOldStatus < STATUS.RED) && (pSoldier.bAlertStatus >= STATUS.RED))
            {
                // might want to make custom to let them go anywhere
                CheckForChangingOrders(pSoldier);
            }
        }
        else   // status didn't change
        {
            // if a guy on status GREEN or YELLOW is running low on breath
            if (((pSoldier.bAlertStatus == STATUS.GREEN) && (pSoldier.bBreath < 75)) ||
                ((pSoldier.bAlertStatus == STATUS.YELLOW) && (pSoldier.bBreath < 50)))
            {
                // as long as he's not in water (standing on a bridge is OK)
                if (!SoldierControl.MercInWater(pSoldier))
                {
                    // force a NEW decision so that he can get some rest
                    SetNewSituation(pSoldier);

                    // current action will be canceled. if noise is no longer important
                    if ((pSoldier.bAlertStatus == STATUS.YELLOW) &&
                        (MostImportantNoiseHeard(pSoldier, out iDummy, out fClimbDummy, out fReachableDummy) == NOWHERE))
                    {
                        // then drop back to GREEN status
                        pSoldier.bAlertStatus = STATUS.GREEN;
                        CheckForChangingOrders(pSoldier);
                    }
                }
            }
        }
    }

    public static AI_ACTION CrowDecideActionRed(SOLDIERTYPE pSoldier)
    {
        // OK, Fly away!
        //HandleCrowFlyAway( pSoldier );
        if (!gfTurnBasedAI)
        {
            pSoldier.usActionData = 30000;
            return (AI_ACTION.WAIT);
        }
        else
        {
            return (AI_ACTION.NONE);
        }
    }

    public static AI_ACTION CrowDecideActionGreen(SOLDIERTYPE pSoldier)
    {
        int sCorpseGridNo;
        WorldDirections ubDirection;
        WorldDirections sFacingDir;

        // Look for a corse!
        sCorpseGridNo = FindNearestRottingCorpse(pSoldier);

        if (sCorpseGridNo != NOWHERE)
        {
            // Are we close, if so , peck!
            if (IsometricUtils.SpacesAway(pSoldier.sGridNo, sCorpseGridNo) < 2)
            {
                // Change facing
                sFacingDir = SoldierControl.GetDirectionFromGridNo(sCorpseGridNo, pSoldier);

                if (sFacingDir != pSoldier.bDirection)
                {
                    pSoldier.usActionData = sFacingDir;
                    return (AI_ACTION.CHANGE_FACING);
                }
                else if (!gfTurnBasedAI)
                {
                    pSoldier.usActionData = 30000;
                    return (AI_ACTION.WAIT);
                }
                else
                {
                    return (AI_ACTION.NONE);
                }
            }
            else
            {
                // Walk to nearest one!
                pSoldier.usActionData = FindGridNoFromSweetSpot(pSoldier, sCorpseGridNo, 4, out ubDirection);
                if ((int)pSoldier.usActionData != NOWHERE)
                {
                    return (AI_ACTION.GET_CLOSER);
                }
            }
        }

        return (AI_ACTION.NONE);
    }

    public static AI_ACTION CrowDecideAction(SOLDIERTYPE pSoldier)
    {
        if (pSoldier.usAnimState == AnimationStates.CROW_FLY)
        {
            return (AI_ACTION.NONE);
        }

        switch (pSoldier.bAlertStatus)
        {
            case STATUS.GREEN:
            case STATUS.YELLOW:
                return (CrowDecideActionGreen(pSoldier));

            case STATUS.RED:
            case STATUS.BLACK:
                return (CrowDecideActionRed(pSoldier));

            default:
                Debug.Assert(false);
                return (AI_ACTION.NONE);
        }
    }
}

public enum CALLER
{
    FEMALE = 0,
    MALE,
    INFANT,
    QUEEN,
    NUM_CREATURE_CALLERS
}

public enum CREATURE
{
    MOBILE = 0,
    CRAWLER,
    IMMOBILE
}
