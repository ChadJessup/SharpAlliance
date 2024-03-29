﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class Movement
{
    //
    // CJC's DG.JA2 conversion notes
    //
    // LegalNPCDestination - mode hardcoded to walking; C.O. tear gas related stuff commented out
    // TryToResumeMovement - C.O. EscortedMoveCanceled call
    // GoAsFarAsPossibleTowards - C.O. stuff related to current animation esp first aid
    // SetCivilianDestination - C.O. stuff for if we don't control the civ

    public static int LegalNPCDestination(SOLDIERTYPE? pSoldier, int sGridno, int ubPathMode, int ubWaterOK, PATH fFlags)
    {
        bool fSkipTilesWithMercs;

        if ((sGridno < 0) || (sGridno >= GRIDSIZE))
        {
            return 0;
        }

        // return false if gridno on different level from merc
        if (IsometricUtils.GridNoOnVisibleWorldTile(pSoldier.sGridNo) && gpWorldLevelData[pSoldier.sGridNo].sHeight != gpWorldLevelData[sGridno].sHeight)
        {
            return 0;
        }

        // skip mercs if turnbased and adjacent AND not doing an IGNORE_PATH check (which is used almost exclusively by GoAsFarAsPossibleTowards)
        fSkipTilesWithMercs = gfTurnBasedAI && ubPathMode != IGNORE_PATH && IsometricUtils.SpacesAway(pSoldier.sGridNo, sGridno) == 1;

        // if this gridno is an OK destination
        // AND the gridno is NOT in a tear-gassed tile when we have no gas mask
        // AND someone is NOT already standing there
        // AND we're NOT already standing at that gridno
        // AND the gridno hasn't been black-listed for us

        // Nov 28 98: skip people in destination tile if in turnbased
        if (Overhead.NewOKDestination(pSoldier, sGridno, fSkipTilesWithMercs, pSoldier.bLevel) &&
                       (!AIUtils.InGas(pSoldier, sGridno)) &&
                       (sGridno != pSoldier.sGridNo) &&
                       (sGridno != pSoldier.sBlackList))
        /*
        if ( ( NewOKDestination(pSoldier, sGridno, false, pSoldier.bLevel ) ) &&
                       ( !(gpWorldLevelData[ sGridno ].ubExtFlags[0] & (MAPELEMENTFLAGS.EXT_SMOKE | MAPELEMENTFLAGS.EXT_TEARGAS | MAPELEMENTFLAGS.EXT_MUSTARDGAS)) || ( pSoldier.inv[ HEAD1POS ].usItem == GASMASK || pSoldier.inv[ HEAD2POS ].usItem == GASMASK ) ) &&
                       ( sGridno != pSoldier.sGridNo ) &&
                       ( sGridno != pSoldier.sBlackList ) )*/
        /*
        if ( ( NewOKDestination(pSoldier,sGridno,ALLPEOPLE, pSoldier.bLevel ) ) &&
                       ( !(gpWorldLevelData[ sGridno ].ubExtFlags[0] & (MAPELEMENTFLAGS.EXT_SMOKE | MAPELEMENTFLAGS.EXT_TEARGAS | MAPELEMENTFLAGS.EXT_MUSTARDGAS)) || ( pSoldier.inv[ HEAD1POS ].usItem == GASMASK || pSoldier.inv[ HEAD2POS ].usItem == GASMASK ) ) &&
                       ( sGridno != pSoldier.sGridNo ) &&
                       ( sGridno != pSoldier.sBlackList ) )
                       */
        {

            // if water's a problem, and gridno is in a water tile (bridges are OK)
//            if (ubWaterOK == 0 && Water(sGridno))
            {
                return 0;
            }

            // passed all checks, now try to make sure we can get there!
            switch (ubPathMode)
            {
                // if finding a path wasn't asked for (could have already been done,
                // for example), don't bother
                case IGNORE_PATH:
                    return 1;

                case ENSURE_PATH:
//                    if (FindBestPath(pSoldier, sGridno, pSoldier.bLevel, AnimationStates.WALKING, COPYROUTE, fFlags))
                    {
                        return 1;        // legal destination
                    }
//                    else // got this far, but found no clear path,
                    {
                        // so test fails
                        return 1;
                    }
                // *** NOTE: movement mode hardcoded to WALKING !!!!!
                case ENSURE_PATH_COST:
                    return PathAI.PlotPath(pSoldier, sGridno, null, false, null, AnimationStates.WALKING, null, null, 0);

                default:
                    return 0;
            }
        }
        else  // something failed - didn't even have to test path
        {
            return 0;         // illegal destination
        }
    }




    int TryToResumeMovement(SOLDIERTYPE? pSoldier, int sGridno)
    {
        bool ubGottaCancel = false;
        int ubSuccess = 0;


        // have to make sure the old destination is still legal (somebody may
        // have occupied the destination gridno in the meantime!)
        if (LegalNPCDestination(pSoldier, sGridno, ENSURE_PATH, WATEROK, 0) > 0)
        {
            pSoldier.bPathStored = true;   // optimization - Ian

            // make him go to it (needed to continue movement across multiple turns)
            AIUtils.NewDest(pSoldier, sGridno);

            ubSuccess = 1;

            // make sure that it worked (check that pSoldier.sDestination == pSoldier.sGridNo)
            if (pSoldier.sDestination == sGridno)
            {
                ubSuccess = 1;
            }
            else
            {
                // must work even for escorted civs, can't just set the flag
                AIMain.CancelAIAction(pSoldier, FORCE);
            }

        }
        else
        {
            // don't black-list anything here, this situation can come up quite
            // legally if another soldier gets in the way between turns

            if (pSoldier.bUnderEscort == 0)
            {
                AIMain.CancelAIAction(pSoldier, DONTFORCE);    // no need to force this
            }
            else
            {
                // this is an escorted NPC, don't want to just completely stop
                // moving, try to find a nearby "next best" destination if possible
                pSoldier.usActionData = GoAsFarAsPossibleTowards(pSoldier, sGridno, pSoldier.bAction);

                // if it's not possible to get any closer
                if ((int)pSoldier.usActionData == NOWHERE)
                {
                    ubGottaCancel = true;
                }
                else
                {
                    // change his desired destination to this new one
                    sGridno = (int)pSoldier.usActionData;

                    // GoAsFar... sets pathStored true only if he could go all the way

                    // make him go to it (needed to continue movement across multiple turns)
                    AIUtils.NewDest(pSoldier, sGridno);


                    // make sure that it worked (check that pSoldier.sDestination == pSoldier.sGridNo)
                    if (pSoldier.sDestination == sGridno)
                    {
                        ubSuccess = 1;
                    }
                    else
                    {
                        ubGottaCancel = true;
                    }
                }

                if (ubGottaCancel)
                {
                    // can't get close, gotta abort the movement!
                    AIMain.CancelAIAction(pSoldier, FORCE);

                    // tell the player doing the escorting that civilian has stopped
                    //EscortedMoveCanceled(pSoldier,COMMUNICATE);
                }
            }
        }

        return ubSuccess;
    }



    int NextPatrolPoint(SOLDIERTYPE? pSoldier)
    {
        // patrol slot 0 is UNUSED, so max patrolCnt is actually only 9
        if ((pSoldier.bPatrolCnt < 1) || (pSoldier.bPatrolCnt >= MAXPATROLGRIDS))
        {
            return NOWHERE;
        }


        pSoldier.bNextPatrolPnt++;


        // if there are no more patrol points, return back to the first one
        if (pSoldier.bNextPatrolPnt > pSoldier.bPatrolCnt)
        {
            pSoldier.bNextPatrolPnt = 1;   // ZERO is not used!
        }

        return pSoldier.usPatrolGrid[pSoldier.bNextPatrolPnt];
    }



    bool PointPatrolAI(SOLDIERTYPE? pSoldier)
    {
        int sPatrolPoint;
        Orders bOldOrders;


        sPatrolPoint = pSoldier.usPatrolGrid[pSoldier.bNextPatrolPnt];

        // if we're already there, advance next patrol point
        if (pSoldier.sGridNo == sPatrolPoint || pSoldier.bNextPatrolPnt == 0)
        {
            // find next valid patrol point
            do
            {
                sPatrolPoint = this.NextPatrolPoint(pSoldier);
            }
            while ((sPatrolPoint != NOWHERE) && (!Overhead.NewOKDestination(pSoldier, sPatrolPoint, IGNOREPEOPLE, pSoldier.bLevel)));

            // if we're back where we started, then ALL other patrol points are junk!
            if (pSoldier.sGridNo == sPatrolPoint)
            {
                // force change of orders & an abort
                sPatrolPoint = NOWHERE;
            }
        }

        // if we don't have a legal patrol point
        if (sPatrolPoint == NOWHERE)
        {
            // over-ride orders to something safer
            pSoldier.bOrders = Orders.FARPATROL;
            return false;
        }


        // make sure we can get there from here at this time, if we can't get all
        // the way there, at least do our best to get close
        if (LegalNPCDestination(pSoldier, sPatrolPoint, ENSURE_PATH, WATEROK, 0) > 0)
        {
            pSoldier.bPathStored = true;       // optimization - Ian
            pSoldier.usActionData = sPatrolPoint;
        }
        else
        {
            // temporarily extend roaming range to infinity by changing orders, else
            // this won't work if the next patrol point is > 10 tiles away!
            bOldOrders = pSoldier.bOrders;
            pSoldier.bOrders = Orders.ONCALL;

            pSoldier.usActionData = GoAsFarAsPossibleTowards(pSoldier, sPatrolPoint, pSoldier.bAction);

            pSoldier.bOrders = bOldOrders;

            // if it's not possible to get any closer, that's OK, but fail this call
            if (NOWHERE == (int)pSoldier.usActionData)
            {
                return false;
            }
        }

        return true;
    }

    bool RandomPointPatrolAI(SOLDIERTYPE? pSoldier)
    {
        int sPatrolPoint;
        Orders bOldOrders;
        int bPatrolIndex;
        int bCnt;

        sPatrolPoint = pSoldier.usPatrolGrid[pSoldier.bNextPatrolPnt];

        // if we're already there, advance next patrol point
        if (pSoldier.sGridNo == sPatrolPoint || pSoldier.bNextPatrolPnt == 0)
        {
            // find next valid patrol point
            // we keep a count of the # of times we are in here to make sure we don't get into an endless
            //loop
            bCnt = 0;
            do
            {
                // usPatrolGrid[0] gets used for centre of close etc patrols, so we have to add 1 to the Random #
                bPatrolIndex = (int)PreRandom(pSoldier.bPatrolCnt) + 1;
                sPatrolPoint = pSoldier.usPatrolGrid[bPatrolIndex];
                bCnt++;
            }
            while ((sPatrolPoint == pSoldier.sGridNo) || ((sPatrolPoint != NOWHERE) && (bCnt < pSoldier.bPatrolCnt)
            && (!Overhead.NewOKDestination(pSoldier, sPatrolPoint, IGNOREPEOPLE, pSoldier.bLevel))));

            if (bCnt == pSoldier.bPatrolCnt)
            {
                // ok, we tried doing this randomly, didn't work well, so now do a linear search
                pSoldier.bNextPatrolPnt = 0;
                do
                {
                    sPatrolPoint = this.NextPatrolPoint(pSoldier);
                }
                while ((sPatrolPoint != NOWHERE)
                && (!Overhead.NewOKDestination(pSoldier, sPatrolPoint, IGNOREPEOPLE, pSoldier.bLevel)));
            }

            // do nothing this time around
            if (pSoldier.sGridNo == sPatrolPoint)
            {
                return false;
            }
        }

        // if we don't have a legal patrol point
        if (sPatrolPoint == NOWHERE)
        {
            // over-ride orders to something safer
            pSoldier.bOrders = Orders.FARPATROL;
            return false;
        }

        // make sure we can get there from here at this time, if we can't get all
        // the way there, at least do our best to get close
        if (LegalNPCDestination(pSoldier, sPatrolPoint, ENSURE_PATH, WATEROK, 0) > 0)
        {
            pSoldier.bPathStored = true;       // optimization - Ian
            pSoldier.usActionData = sPatrolPoint;
        }
        else
        {
            // temporarily extend roaming range to infinity by changing orders, else
            // this won't work if the next patrol point is > 10 tiles away!
            bOldOrders = pSoldier.bOrders;
            pSoldier.bOrders = Orders.SEEKENEMY;

            pSoldier.usActionData = GoAsFarAsPossibleTowards(pSoldier, sPatrolPoint, pSoldier.bAction);

            pSoldier.bOrders = bOldOrders;

            // if it's not possible to get any closer, that's OK, but fail this call
            if (NOWHERE == (int)pSoldier.usActionData)
            {
                return false;
            }
        }


        // passed all tests - start moving towards next patrol point
        return true;
    }

    public static int InternalGoAsFarAsPossibleTowards(SOLDIERTYPE? pSoldier, int sDesGrid, int bReserveAPs, AI_ACTION bAction, FLAG fFlags)
    {
        int sLoop, sAPCost;
        int sTempDest, sGoToGrid;
        int usMaxDist;
        int ubDirection, ubDirsLeft;
        int[] ubDirChecked = new int[8];
        int fFound = 0;
        int bAPsLeft = 0;
        PATH fPathFlags;
        int ubRoomRequired = 0;

        if (bReserveAPs == -1)
        {
            // default reserve points
            if (CREATURE_OR_BLOODCAT(pSoldier))
            {
                bReserveAPs = 0;
            }
            else
            {
                bReserveAPs = MAX_AP_CARRIED;
            }
        }

        sTempDest = -1;

        // obtain maximum roaming distance from soldier's sOrigin
        usMaxDist = AIUtils.RoamingRange(pSoldier, out int sOrigin);

        if (pSoldier.bOrders <= Orders.CLOSEPATROL && (pSoldier.bTeam == CIV_TEAM || pSoldier.ubProfile != NO_PROFILE))
        {
            if (RenderFun.InARoom(pSoldier.usPatrolGrid[0], out ubRoomRequired))
            {
                // make sure this doesn't interfere with pathing for scripts
                if (pSoldier.sAbsoluteFinalDestination != NOWHERE)
                {
                    ubRoomRequired = 0;
                }
            }
        }

//        pSoldier.usUIMovementMode = DetermineMovementMode(pSoldier, bAction);
        if (pSoldier.usUIMovementMode == AnimationStates.RUNNING && fFlags.HasFlag(FLAG.CAUTIOUS))
        {
            pSoldier.usUIMovementMode = AnimationStates.WALKING;
        }

        // if soldier is ALREADY at the desired destination, quit right away
        if (sDesGrid == pSoldier.sGridNo)
        {
            return NOWHERE;
        }

        // don't try to approach go after noises or enemies actually in water
        // would be too easy to throw rocks in water, etc. & distract the AI
//        if (Water(sDesGrid))
//        {
//            return (NOWHERE);
//        }

        fPathFlags = 0;
        if (CREATURE_OR_BLOODCAT(pSoldier))
        {   /*
		if ( PythSpacesAway( pSoldier.sGridNo, sDesGrid ) <= PATH_CLOSE_RADIUS )
		{
			// then do a limited range path search and see if we can get there
			gubNPCDistLimit = 10;
			if ( !LegalNPCDestination( pSoldier, sDesGrid, ENSURE_PATH, NOWATER, fPathFlags) )
			{
				gubNPCDistLimit = 0;
				return( NOWHERE );
			}
			else
			{
				// allow attempt to path without 'good enough' flag on
				gubNPCDistLimit = 0;
			}
		}
		else
		{
		*/
            fPathFlags = PATH.CLOSE_GOOD_ENOUGH;
            //}
        }

        // first step: try to find an OK destination at or near the desired gridno
        if (LegalNPCDestination(pSoldier, sDesGrid, ENSURE_PATH, NOWATER, fPathFlags) == 0)
        {
            if (CREATURE_OR_BLOODCAT(pSoldier))
            {
                // we tried to get close, failed; abort!
                return NOWHERE;
            }
            else
            {
                // else look at the 8 nearest gridnos to sDesGrid for a valid destination

                // clear ubDirChecked flag for all 8 directions
                for (ubDirection = 0; ubDirection < 8; ubDirection++)
                {
                    ubDirChecked[ubDirection] = 0;
                }

                ubDirsLeft = 8;

                // examine all 8 spots around 'sDesGrid'
                // keep looking while directions remain and a satisfactory one not found
                for (ubDirsLeft = 8; ubDirsLeft != 0; ubDirsLeft--)
                {
                    if (fFound > 0)
                    {
                        break;
                    }
                    // randomly select a direction which hasn't been 'checked' yet
                    do
                    {
                        ubDirection = Globals.Random.Next(8);
                    }
                    while (ubDirChecked[ubDirection] > 0);

                    ubDirChecked[ubDirection] = 1;

                    // determine the gridno 1 tile away from current friend in this direction
                    sTempDest = IsometricUtils.NewGridNo(sDesGrid, IsometricUtils.DirectionInc((int)(ubDirection + 1)));

                    // if that's out of bounds, ignore it & check next direction
                    if (sTempDest == sDesGrid)
                    {
                        continue;
                    }

                    if (LegalNPCDestination(pSoldier, sTempDest, ENSURE_PATH, NOWATER, 0) > 0)
                    {
                        fFound = 1;            // found a spot

                        break;                   // stop checking in other directions
                    }
                }

                if (fFound == 0)
                {
                    return NOWHERE;
                }

                // found a good grid #, this becomes our actual desired grid #
                sDesGrid = sTempDest;
            }
        }

        // HAVE FOUND AN OK destination AND PLOTTED A VALID BEST PATH TO IT

        sGoToGrid = pSoldier.sGridNo;      // start back where soldier is standing now
        sAPCost = 0;              // initialize path cost counter

        // we'll only go as far along the plotted route as is within our
        // permitted roaming range, and we'll stop as soon as we're down to <= 5 APs

        for (sLoop = 0; sLoop < (pSoldier.usPathDataSize - pSoldier.usPathIndex); sLoop++)
        {
            // what is the next gridno in the path?

            //sTempDest = NewGridNo( sGoToGrid,DirectionInc( (int) (pSoldier.usPathingData[sLoop] + 1) ) );
            sTempDest = IsometricUtils.NewGridNo(sGoToGrid, IsometricUtils.DirectionInc(pSoldier.usPathingData[sLoop]));
            //NumMessage("sTempDest = ",sTempDest);

            // this should NEVER be out of bounds
            if (sTempDest == sGoToGrid)
            {
                break;           // quit here, sGoToGrid is where we are going
            }

            // if this takes us beyond our permitted "roaming range"
            if (IsometricUtils.SpacesAway(sOrigin, sTempDest) > usMaxDist)
            {
                break;           // quit here, sGoToGrid is where we are going
            }

            if (ubRoomRequired > 0)
            {
                if (!(RenderFun.InARoom(sTempDest, out int ubTempRoom) && ubTempRoom == ubRoomRequired))
                {
                    // quit here, limited by room!
                    break;
                }
            }

//            if ((fFlags.HasFlag(FLAG.STOPSHORT)) && IsometricUtils.SpacesAway(sDesGrid, sTempDest) <= STOPSHORTDIST)
//            {
//                break;           // quit here, sGoToGrid is where we are going
//            }

            // if this gridno is NOT a legal NPC destination
            // DONT'T test path again - that would replace the traced path! - Ian
            // NOTE: It's OK to go *THROUGH* water to try and get to the destination!
            if (LegalNPCDestination(pSoldier, sTempDest, IGNORE_PATH, WATEROK, 0) == 0)
            {
                break;           // quit here, sGoToGrid is where we are going
            }


            // CAN'T CALL PathCost() HERE! IT CALLS findBestPath() and overwrites
            //       pathRouteToGo !!!  Gotta calculate the cost ourselves - Ian
            //
            //ubAPsLeft = pSoldier.bActionPoints - PathCost(pSoldier,sTempDest,false,false,false,false,false);

            if (gfTurnBasedAI)
            {
                // if we're just starting the "costing" process (first gridno)
                if (sLoop == 0)
                {
                    /*
                     // first, add any additional costs - such as intermediate animations, etc.
                     switch(pSoldier.anitype[pSoldier.anim])
                        {
                         // in theory, no NPC should ever be in one of these animations as
                         // things stand (they don't medic anyone), but leave it for robustness
                         case START_AID   :
                         case GIVING_AID  : sAnimCost = AP.STOP_FIRST_AID;
                            break;

                         case TWISTOMACH  :
                         case COLLAPSED   : sAnimCost = AP.GET_UP;
                            break;

                         case TWISTBACK   :
                         case UNCONSCIOUS : sAnimCost = (AP_ROLL_OVER + AP.GET_UP);
                            break;

                         default          : sAnimCost = 0;
                        }

                     // this is our first cost
                     sAPCost += sAnimCost;
                     */

                    if (pSoldier.usUIMovementMode == AnimationStates.RUNNING)
                    {
                        sAPCost += AP.START_RUN_COST;
                    }
                }

                // ATE: Direction here?
//                sAPCost += EstimateActionPointCost(pSoldier, sTempDest, (int)pSoldier.usPathingData[sLoop], pSoldier.usUIMovementMode, (int)sLoop, (int)pSoldier.usPathDataSize);

                bAPsLeft = pSoldier.bActionPoints - sAPCost;
            }

            // if after this, we have <= 5 APs remaining, that's far enough, break out
            // (the idea is to preserve APs so we can crouch or react if
            // necessary, and benefit from the carry-over next turn if not needed)
            // This routine is NOT used by any GREEN AI, so such caution is warranted!

            if (gfTurnBasedAI && (bAPsLeft < bReserveAPs))
            {
                break;
            }
            else
            {
                sGoToGrid = sTempDest;    // we're OK up to here

                // if exactly 5 APs left, don't bother checking any further
                if (gfTurnBasedAI && (bAPsLeft == bReserveAPs))
                {
                    break;
                }
            }
        }


        // if it turned out we couldn't go even 1 tile towards the desired gridno
        if (sGoToGrid == pSoldier.sGridNo)
        {
            return NOWHERE;             // then go nowhere
        }
        else
        {
            // possible optimization - stored path IS good if we're going all the way
            if (sGoToGrid == sDesGrid)
            {
                pSoldier.bPathStored = true;
                pSoldier.sFinalDestination = sGoToGrid;
            }
            else if (pSoldier.usPathIndex == 0)
            {
                // we can hack this surely! -- CJC
                pSoldier.bPathStored = true;
                pSoldier.sFinalDestination = sGoToGrid;
                pSoldier.usPathDataSize = sLoop + 1;
            }

            return sGoToGrid;
        }
    }

    public static int GoAsFarAsPossibleTowards(SOLDIERTYPE? pSoldier, int sDesGrid, AI_ACTION bAction)
    {
        return InternalGoAsFarAsPossibleTowards(pSoldier, sDesGrid, -1, bAction, 0);
    }

    void SoldierTriesToContinueAlongPath(SOLDIERTYPE pSoldier)
    {
        int usNewGridNo, bAPCost = 0;

        // turn off the flag now that we're going to do something about it...
        // ATE: USed to be redundent, now if called befroe NewDest can cause some side efects...
        // AdjustNoAPToFinishMove( pSoldier, false );

        if (pSoldier.bNewSituation == IS_NEW_SITUATION)
        {
            AIMain.CancelAIAction(pSoldier, DONTFORCE);
            return;
        }

        if ((int)pSoldier.usActionData >= NOWHERE)
        {
            AIMain.CancelAIAction(pSoldier, DONTFORCE);
            return;
        }

        if (!Overhead.NewOKDestination(pSoldier, (int)pSoldier.usActionData, true, pSoldier.bLevel))
        {
            AIMain.CancelAIAction(pSoldier, DONTFORCE);
            return;
        }

        if (AIUtils.IsActionAffordable(pSoldier))
        {
            if (pSoldier.bActionInProgress == 0)
            {
                // start a move that didn't even get started before...
                // hope this works...
                AIMain.NPCDoesAct(pSoldier);

                // perform the chosen action
                pSoldier.bActionInProgress = AIMain.ExecuteAction(pSoldier); // if started, mark us as busy
            }
            else
            {
                // otherwise we shouldn't have to do anything(?)
            }
        }
        else
        {
            AIMain.CancelAIAction(pSoldier, DONTFORCE);
        }

        usNewGridNo = IsometricUtils.NewGridNo(pSoldier.sGridNo, IsometricUtils.DirectionInc(pSoldier.usPathingData[pSoldier.usPathIndex]));

        // Find out how much it takes to move here!
//        bAPCost = EstimateActionPointCost(pSoldier, usNewGridNo, (int)pSoldier.usPathingData[pSoldier.usPathIndex], pSoldier.usUIMovementMode, (int)pSoldier.usPathIndex, (int)pSoldier.usPathDataSize);

        if (pSoldier.bActionPoints >= bAPCost)
        {
            // seems to have enough points...
            AIUtils.NewDest(pSoldier, usNewGridNo);
            // maybe we didn't actually start the action last turn...
            pSoldier.bActionInProgress = 1;
        }
        else
        {
            AIMain.CancelAIAction(pSoldier, DONTFORCE);
        }
    }

    public static void HaltMoveForSoldierOutOfPoints(SOLDIERTYPE? pSoldier)
    {
        // If a special move, ignore this!
        if (gAnimControl[pSoldier.usAnimState].uiFlags.HasFlag(ANIM.SPECIALMOVE))
        {
            return;
        }

        // record that this merc can no longer animate and why...
//        AdjustNoAPToFinishMove(pSoldier, true);

        // We'll keep his action intact though...
        //DebugAI(String("NO AP TO FINISH MOVE for %d (%d APs left)", pSoldier.ubID, pSoldier.bActionPoints));

        // if this dude is under AI right now, then pass the baton to someone else
        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.UNDERAICONTROL))
        {
            AIMain.EndAIGuysTurn(pSoldier);
        }
    }

    void SetCivilianDestination(int ubWho, int sGridno)
    {
        SOLDIERTYPE? pSoldier;


        pSoldier = MercPtrs[ubWho];

        /*
         // if we control the civilian
         if (PTR_OURCONTROL)
          {
        */
        // if the destination is different from what he has now
        if ((int)pSoldier.usActionData != sGridno)
        {
            // store his new destination
            pSoldier.usActionData = sGridno;

            // and cancel any movement in progress that he was still engaged in
            pSoldier.bAction = AI_ACTION.NONE;
            pSoldier.bActionInProgress = 0;
        }

        // only set the underEscort flag once you give him a destination
        // (that way AI can keep him appearing to act on his own until you
        // give him orders).
        //
        // Either way, once set, it should stay that way, preventing AI from
        // doing anything other than advance him towards destination.
        pSoldier.bUnderEscort = 1;

        // change orders to maximize roaming range so he can Go As Far As Possible
        pSoldier.bOrders = Orders.ONCALL;
        /*
          }

         else
          {
           NetSend.msgType = NET_CIV_DEST;
           NetSend.ubID  = pSoldier.ubID;
           NetSend.gridno  = gridno;

           // only the civilian's controller needs to know this
           SendNetData(pSoldier.controller);
          }
        */
    }

    public static int TrackScent(SOLDIERTYPE? pSoldier)
    {
        // This function returns the best gridno to go to based on the scent being followed,
        // and the soldier (creature/animal)'s current direction (which is used to resolve
        // ties.
        int iXDiff, iYDiff, iXIncr;
        int iStart, iXStart, iYStart;
        int iGridNo;
        WorldDirections bDir;
        int iBestGridNo = NOWHERE;
        int ubBestDirDiff = 5, ubBestStrength = 0;
        int ubDirDiff, ubStrength;
        int ubSoughtSmell;
        MAP_ELEMENT? pMapElement;

        iStart = pSoldier.sGridNo;
        iXStart = iStart % WORLD_COLS;
        iYStart = iStart / WORLD_COLS;

        if (CREATURE_OR_BLOODCAT(pSoldier)) // or bloodcats
        {
            // tracking humans; search the edges of a 7x7 square for the 
            // most promising tile
            ubSoughtSmell = HUMAN;
            for (iYDiff = -RADIUS; iYDiff < (RADIUS + 1); iYDiff++)
            {
                if (iYStart + iYDiff < 0)
                {
                    // outside of map! might be on map further down...
                    continue;
                }
                else if (iYStart + iYDiff > WORLD_ROWS)
                {
                    // outside of bottom of map! abort!
                    break;
                }
                if (iYDiff == -RADIUS || iYDiff == RADIUS)
                {
                    iXIncr = 1;
                }
                else
                {
                    // skip over the spots in the centre of the square
                    iXIncr = RADIUS * 2;
                }
                for (iXDiff = -RADIUS; iXDiff < (RADIUS + 1); iXDiff += iXIncr)
                {
                    iGridNo = iStart + iXDiff + iYDiff * WORLD_ROWS;
                    if (Math.Abs(iGridNo % WORLD_ROWS - iXStart) > RADIUS)
                    {
                        // wrapped across map!
                        continue;
                    }
                    if (LegalNPCDestination(pSoldier, (int)pSoldier.usActionData, ENSURE_PATH, WATEROK, 0) > 0)
                    {
                        // check this location out
                        pMapElement = gpWorldLevelData[iGridNo];
                        if (pMapElement.ubSmellInfo > 0 && (Smell.SMELL_TYPE(pMapElement.ubSmellInfo) == ubSoughtSmell))
                        {
                            ubStrength = Smell.SMELL_STRENGTH(pMapElement.ubSmellInfo);
                            if (ubStrength > ubBestStrength)
                            {
                                iBestGridNo = iGridNo;
                                ubBestStrength = ubStrength;
                                bDir = SoldierControl.atan8(iXStart, iYStart, iXStart + iXDiff, iYStart + iYDiff);
                                // now convert it into a difference in degree between it and our current dir
                                ubBestDirDiff = Math.Abs(pSoldier.bDirection - bDir);
                                if (ubBestDirDiff > 4) // dir 0 compared with dir 6, for instance
                                {
                                    ubBestDirDiff = 8 - ubBestDirDiff;
                                }
                            }
                            else if (ubStrength == ubBestStrength)
                            {
                                if (iBestGridNo == NOWHERE)
                                {
                                    // first place we've found with the same strength
                                    iBestGridNo = iGridNo;
                                    ubBestStrength = ubStrength;
                                }
                                else
                                {
                                    // use directions to decide between the two
                                    // start by calculating direction to the new gridno
                                    bDir = SoldierControl.atan8((int)iXStart, (int)iYStart, (int)(iXStart + iXDiff), (int)(iYStart + iYDiff));
                                    // now convert it into a difference in degree between it and our current dir
                                    ubDirDiff = Math.Abs(pSoldier.bDirection - bDir);
                                    if (ubDirDiff > 4) // dir 0 compared with dir 6, for instance
                                    {
                                        ubDirDiff = 8 - ubDirDiff;
                                    }
                                    if (ubDirDiff < ubBestDirDiff || ((ubDirDiff == ubBestDirDiff) && Globals.Random.Next(2) > 0))
                                    {
                                        // follow this trail as its closer to the one we're following!
                                        // (in the case of a tie, we tossed a coin)
                                        ubBestDirDiff = ubDirDiff;

                                    }
                                }
                            }
                        }
                    }
                }
                // go on to next tile
            }
            // go on to next row
        }
        else
        {
            // who else can track? 
        }
        if (iBestGridNo != NOWHERE)
        {
            pSoldier.usActionData = (int)iBestGridNo;
            return (int)iBestGridNo;
        }
        return 0;
    }

    /*
    int RunAway( SOLDIERTYPE * pSoldier )
    {
        // "Run away! Run away!!!"
        // This code should figure out which directions are safe for the enemy
        // to run in.  They shouldn't try to run off in directions which will
        // take them into enemy territory.  We must presume that they inform each
        // other by radio when sectors are taken by the player! :-)
        // The second wrinkle would be to look at the directions to known player
        // mercs and use that to influence the direction in which we run.

        // we can only flee in the cardinal directions (NESW) so start with an
        // alternating pattern of true/false
        int bOkayDir[8] = {true, false, true, false, true, false, true, false};
        int ubLoop, ubBestDir, ubDistToEdge, ubBestDistToEdge = WORLD_COLS;
        int	iSector, iSectorX, iSectorY;
        int iNewSectorX, iNewSectorY, iNewSector;
        int	iRunX, iRunY, iRunGridNo;
        SOLDIERTYPE * pOpponent;

        iSector = pSoldier.sSectorX + pSoldier.sSectorY * MAP_WORLD_X;

        // first start by scanning through opposing mercs and find out what directions are blocked.
        for (ubLoop = 0,pOpponent = Menptr; ubLoop < MAXMERCS; ubLoop++,pOpponent++)
        {
            // if this merc is inactive, at base, on assignment, or dead
            if (!pOpponent.bActive || !pOpponent.bInSector || !pOpponent.bLife)
            {
                continue;          // next merc
            }

            // if this man is neutral / on the same side, he's not an opponent
            if (pOpponent.bNeutral || (pSoldier.bSide == pOpponent.bSide))
            {
                continue;          // next merc
            }

            // we don't want to run in that direction!
            bOkayDir[ atan8( pSoldier.sX, pSoldier.sY, pOpponent.sX, pOpponent.sY ) ] = false;
        }

        for (ubLoop = 0; ubLoop < 8; ubLoop += 2)
        {
            if (bOkayDir[ubLoop])
            {
                // figure out sector # in that direction
                iNewSectorX = pSoldier.sSectorX + DirXIncrementer[ubLoop];
                iNewSectorY = pSoldier.sSectorY + DirYIncrementer[ubLoop];
                iNewSector = iSectorX + iSectorY * MAP_WORLD_X;
                // check movement
                if (TravelBetweenSectorsIsBlockedFromFoot( (int) iSector, (int) iNewSector ) || StrategicMap[iSector].fEnemyControlled)
                {
                    // sector inaccessible or controlled by the player; skip it!
                    continue;
                }
                switch( ubLoop )
                {
                    case 0:
                        ubDistToEdge = pSoldier.sGridNo / WORLD_COLS;
                        break;
                    case 2:
                        ubDistToEdge = WORLD_COLS - pSoldier.sGridNo % WORLD_COLS;
                        break;
                    case 4:
                        ubDistToEdge = WORLD_ROWS - pSoldier.sGridNo / WORLD_COLS;
                        break;
                    case 6:
                        ubDistToEdge = pSoldier.sGridNo % WORLD_COLS;
                        break;
                }
                if (ubDistToEdge < ubBestDistToEdge)
                {
                    ubBestDir = ubLoop;
                    ubBestDistToEdge = ubDistToEdge;
                }
            }
        }
        if (ubBestDistToEdge < WORLD_COLS)
        {	
            switch( ubBestDir )
            {
                case 0:
                    iRunX = pSoldier.sX + Random( 9 ) - 4;
                    iRunY = 0;
                    if (iRunX < 0)
                    {
                        iRunX = 0;
                    }
                    else if (iRunX >= WORLD_COLS)
                    {
                        iRunX = WORLD_COLS - 1;
                    }
                    break;
                case 2:
                    iRunX = WORLD_COLS;
                    iRunY = pSoldier.sY + Random( 9 ) - 4;
                    if (iRunY < 0)
                    {
                        iRunY = 0;
                    }
                    else if (iRunY >= WORLD_COLS)
                    {
                        iRunY = WORLD_ROWS - 1;
                    }
                    break;
                case 4:
                    iRunX = pSoldier.sX + Random( 9 ) - 4;
                    iRunY = WORLD_ROWS;
                    if (iRunX < 0)
                    {
                        iRunX = 0;
                    }
                    else if (iRunX >= WORLD_COLS)
                    {
                        iRunX = WORLD_COLS - 1;
                    }
                    break;
                case 6:
                    iRunX = 0;
                    iRunY = pSoldier.sY + Random( 9 ) - 4;
                    if (iRunY < 0)
                    {
                        iRunY = 0;
                    }
                    else if (iRunY >= WORLD_COLS)
                    {
                        iRunY = WORLD_ROWS - 1;
                    }
                    break;
            }
            iRunGridNo = iRunX + iRunY * WORLD_COLS;
            if (LegalNPCDestination( pSoldier, (int) iRunGridNo, ENSURE_PATH, true,0))
            {
                return( (int) iRunGridNo );
            }
            // otherwise we'll try again another time
        }
        return( NOWHERE );
    }
    */
}
