using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class PanicButtons
{
    void MakeClosestEnemyChosenOne()
    {
        int cnt;
        int sPathCost, sShortestPath = 1000;
        int bOldKeys = -1;
        int ubClosestEnemy = NOBODY;
        int bPanicTrigger;
        int sPanicTriggerGridNo;

        if (!(gTacticalStatus.fPanicFlags.HasFlag(PANIC.TRIGGERS_HERE)))
        {
            return;
        }

        if (!NeedToRadioAboutPanicTrigger())
        {
            // no active panic triggers
            return;
        }

        // consider every enemy, looking for the closest capable, unbusy one
        foreach (var pSoldier in MercSlots)
        {
            if (pSoldier is null)  // if this merc is inactive, or not here
            {
                continue;
            }

            // if this merc is unconscious, or dead
            if (pSoldier.bLife < OKLIFE)
            {
                continue;  // next soldier
            }

            // if this guy's too tired to go
            if (pSoldier.bBreath < OKBREATH)
            {
                continue;  // next soldier
            }

            if (gWorldSectorX == TIXA_SECTOR_X && gWorldSectorY == TIXA_SECTOR_Y)
            {
                if (pSoldier.ubProfile != NPCID.WARDEN)
                {
                    continue;
                }
            }
            else
            {
                // only consider for army guys
                if (pSoldier.bTeam != ENEMY_TEAM)
                {
                    continue;
                }
            }

            // if this guy is in battle with opponent(s)
            if (pSoldier.bOppCnt > 0)
            {
                continue;  // next soldier
            }

            // if this guy is still in serious shock
            if (pSoldier.bShock > 2)
            {
                continue;  // next soldier
            }

            if (pSoldier.bLevel != 0)
            {
                // screw having guys on the roof go for panic triggers!
                continue;  // next soldier
            }

            bPanicTrigger = ClosestPanicTrigger(pSoldier);
            if (bPanicTrigger == -1)
            {
                continue; // next soldier
            }

            sPanicTriggerGridNo = gTacticalStatus.sPanicTriggerGridNo[bPanicTrigger];
            if (sPanicTriggerGridNo == NOWHERE)
            {
                // this should never happen!
                continue;
            }

            // remember whether this guy had keys before
            //bOldKeys = pSoldier.bHasKeys;

            // give him keys to see if with them he can get to the panic trigger
            pSoldier.bHasKeys = (pSoldier.bHasKeys << 1) | 1;

            // we now path directly to the panic trigger


            // if he can't get to a spot where he could get at the panic trigger
            /*
			if ( FindAdjacentGridEx( pSoldier, gTacticalStatus.sPanicTriggerGridno, &ubDirection, &sAdjSpot, false, false ) == -1 )
			{
				pSoldier.bHasKeys = bOldKeys;
				continue;          // next merc
			}
			*/


            // ok, this enemy appears to be eligible

            // FindAdjacentGrid set HandGrid for us.  If we aren't at that spot already
            if (pSoldier.sGridNo != sPanicTriggerGridNo)
            {
                // get the AP cost for this enemy to go to target position
                sPathCost = PathAI.PlotPath(pSoldier, sPanicTriggerGridNo, null, false, null, AnimationStates.WALKING, null, null, 0);
            }
            else
            {
                sPathCost = 0;
            }

            // set his keys value back to what it was before this hack
            pSoldier.bHasKeys = (pSoldier.bHasKeys >> 1);

            // if he can get there (or is already there!)
            if (sPathCost > 0 || (pSoldier.sGridNo == sPanicTriggerGridNo))
            {
                if (sPathCost < sShortestPath)
                {
                    sShortestPath = sPathCost;
                    ubClosestEnemy = pSoldier.ubID;
                }
            }
            //else
            //NameMessage(pSoldier,"can't get there...");
        }

        // if we found have an eligible enemy, make him our "chosen one"
        if (ubClosestEnemy < NOBODY)
        {
            gTacticalStatus.ubTheChosenOne = ubClosestEnemy;       // flag him as the chosen one

            var pSoldier = MercPtrs[gTacticalStatus.ubTheChosenOne];
            if (pSoldier.bAlertStatus < STATUS.RED)
            {
                pSoldier.bAlertStatus = STATUS.RED;
                CheckForChangingOrders(pSoldier);
            }

            SetNewSituation(pSoldier);    // set new situation for the chosen one
            pSoldier.bHasKeys = (pSoldier.bHasKeys << 1) | 1; // cheat and give him keys to every door
                                                              //pSoldier.bHasKeys = true;         
        }
    }

    void PossiblyMakeThisEnemyChosenOne(SOLDIERTYPE? pSoldier)
    {
        int iAPCost, iPathCost;
        //int		bOldKeys;
        int bPanicTrigger;
        int sPanicTriggerGridNo;
        int uiPercentEnemiesKilled;

        if (!(gTacticalStatus.fPanicFlags.HasFlag(PANIC.TRIGGERS_HERE)))
        {
            return;
        }

        if (pSoldier.bLevel != 0)
        {
            // screw having guys on the roof go for panic triggers!
            return;
        }


        bPanicTrigger = ClosestPanicTrigger(pSoldier);
        if (bPanicTrigger == -1)
        {
            return;
        }

        sPanicTriggerGridNo = gTacticalStatus.sPanicTriggerGridNo[bPanicTrigger];

        uiPercentEnemiesKilled = (int)(100 * (int)(gTacticalStatus.ubArmyGuysKilled) / (int)(gTacticalStatus.Team[ENEMY_TEAM].bMenInSector + gTacticalStatus.ubArmyGuysKilled));
        if (gTacticalStatus.ubPanicTolerance[bPanicTrigger] > uiPercentEnemiesKilled)
        {
            // not yet... not yet
            return;
        }

        //bOldKeys = pSoldier.bHasKeys;	
        pSoldier.bHasKeys = (pSoldier.bHasKeys << 1) | 1;

        // if he can't get to a spot where he could get at the panic trigger
        iAPCost = AP.PULL_TRIGGER;
        if (pSoldier.sGridNo != sPanicTriggerGridNo)
        {
            iPathCost = PathAI.PlotPath(pSoldier, sPanicTriggerGridNo, null, false, null, AnimationStates.RUNNING, null, null, 0);
            if (iPathCost == 0)
            {
                //pSoldier.bHasKeys = bOldKeys;
                pSoldier.bHasKeys = (pSoldier.bHasKeys >> 1);
                return;
            }
            iAPCost += iPathCost;

        }

        if (iAPCost <= CalcActionPoints(pSoldier) * 2)
        {
            // go!!!
            gTacticalStatus.ubTheChosenOne = pSoldier.ubID;
            return;
        }
        // else return keys to normal
        //pSoldier.bHasKeys = bOldKeys;
        pSoldier.bHasKeys = (pSoldier.bHasKeys >> 1);
    }


    AI_ACTION PanicAI(SOLDIERTYPE? pSoldier, int ubCanMove)
    {
        bool fFoundRoute = false;
        InventorySlot bSlot;
        int iPathCost;
        int bPanicTrigger;
        int sPanicTriggerGridNo;

        // if there are panic bombs here
        if (gTacticalStatus.fPanicFlags.HasFlag(PANIC.BOMBS_HERE))
        {
            // if enemy is holding a portable panic bomb detonator, he tries to use it
            bSlot = FindObj(pSoldier, Items.REMOTEBOMBTRIGGER);
            if (bSlot != NO_SLOT)
            {
                //////////////////////////////////////////////////////////////////////
                // ACTIVATE DETONATOR: blow up sector's panic bombs
                //////////////////////////////////////////////////////////////////////

                // if we have enough APs to activate it now
                if (pSoldier.bActionPoints >= AP.USE_REMOTE)
                {
                    // blow up all the PANIC bombs!
                    return (AI_ACTION.USE_DETONATOR);
                }
                else     // otherwise, wait a turn
                {
                    pSoldier.usActionData = NOWHERE;
                    return (AI_ACTION.NONE);
                }
            }
        }

        // no panic bombs, or no portable detonator

        // if there's a panic trigger here (DOESN'T MATTER IF ANY PANIC BOMBS EXIST!)
        if (gTacticalStatus.fPanicFlags.HasFlag(PANIC.TRIGGERS_HERE))
        {
            // Have WE been chosen to go after the trigger?
            if (pSoldier.ubID == gTacticalStatus.ubTheChosenOne)
            {
                bPanicTrigger = ClosestPanicTrigger(pSoldier);
                if (bPanicTrigger == -1)
                {
                    // augh!
                    return (AI_ACTION)(-1);
                }
                sPanicTriggerGridNo = gTacticalStatus.sPanicTriggerGridNo[bPanicTrigger];

                // if not standing on the panic trigger
                if (pSoldier.sGridNo != sPanicTriggerGridNo)
                {
                    // determine whether we can still get there 
                    iPathCost = PathAI.PlotPath(pSoldier, sPanicTriggerGridNo, null, false, null, AnimationStates.RUNNING, null, null, 0);
                    if (iPathCost != 0)
                    {
                        fFoundRoute = true;
                    }
                }
                else
                {
                    fFoundRoute = true;
                }

                // if we managed to find an adjacent spot
                if (fFoundRoute)
                {

                    /*
                     *** COMMENTED OUT BECAUSE WE DON'T HAVE SUPPORT ROUTINES YET

                           // make sure it's not in water (those triggers can't be pulled)
                           if (Water(Terrain(gTacticalStatus.sHandGrid),Structure(gTacticalStatus.sHandGrid)))
                        {
                    #if BETAVERSION
                             PopMessage("BAD SCENARIO DESIGN: Enemies can't use this panic trigger!");
                    #endif
                             gTacticalStatus.ubTheChosenOne = NOBODY;   // strip him of his Chosen One status
                         // don't bother replacing him either, the next won't have more luck!
                             return(-1);
                        }

                        */
                    // if we are at that spot now
                    if (pSoldier.sGridNo == sPanicTriggerGridNo)
                    {
                        ////////////////////////////////////////////////////////////////
                        // PULL THE PANIC TRIGGER!
                        ////////////////////////////////////////////////////////////////

                        // and we have enough APs left to pull the trigger
                        if (pSoldier.bActionPoints >= AP.PULL_TRIGGER)
                        {
                            // blow up the all the PANIC bombs (or just the journal)
                            pSoldier.usActionData = sPanicTriggerGridNo;

                            return (AI_ACTION.PULL_TRIGGER);
                        }
                        else       // otherwise, wait a turn
                        {
                            pSoldier.usActionData = NOWHERE;
                            return (AI_ACTION.NONE);
                        }
                    }
                    else           // we are NOT at the HandGrid spot
                    {
                        // if we can move at least 1 square's worth
                        if (ubCanMove > 0)
                        {
                            // if we can get to the HandGrid spot to yank the trigger
                            // animations don't allow trigger-pulling from water, so we won't!
                            if (Movement.LegalNPCDestination(pSoldier, sPanicTriggerGridNo, ENSURE_PATH, NOWATER, 0))
                            {
                                pSoldier.usActionData = sPanicTriggerGridNo;
                                pSoldier.bPathStored = true;

                                return (AI_ACTION.GET_CLOSER);
                            }
                            else       // Oh oh, the chosen one can't get to the trigger!
                            {
                                gTacticalStatus.ubTheChosenOne = NOBODY;   // strip him of his Chosen One status
                                MakeClosestEnemyChosenOne();     // and replace him!
                            }
                        }
                        else         // can't move, wait 1 turn
                        {
                            pSoldier.usActionData = NOWHERE;
                            return (AI_ACTION.NONE);
                        }
                    }
                }
                else     // Oh oh, the chosen one can't get to the trigger!
                {
                    gTacticalStatus.ubTheChosenOne = NOBODY; // strip him of his Chosen One status
                    MakeClosestEnemyChosenOne();   // and replace him!
                }
            }
        }

        // no action decided
        return (AI_ACTION)(-1);
    }

    public static void InitPanicSystem()
    {
        // start by assuming there is no panic bombs or triggers here
        gTacticalStatus.ubTheChosenOne = NOBODY;
        WorldItems.FindPanicBombsAndTriggers();
    }

    int ClosestPanicTrigger(SOLDIERTYPE? pSoldier)
    {
        int bLoop;
        int sDistance;
        int sClosestDistance = 1000;
        int bClosestTrigger = -1;
        int uiPercentEnemiesKilled;

        uiPercentEnemiesKilled = (int)(100 * (int)(gTacticalStatus.ubArmyGuysKilled) / (int)(gTacticalStatus.Team[ENEMY_TEAM].bMenInSector + gTacticalStatus.ubArmyGuysKilled));

        for (bLoop = 0; bLoop < NUM_PANIC_TRIGGERS; bLoop++)
        {
            if (gTacticalStatus.sPanicTriggerGridNo[bLoop] != NOWHERE)
            {

                if (gTacticalStatus.ubPanicTolerance[bLoop] > uiPercentEnemiesKilled)
                {
                    // not yet... not yet...
                    continue; // next trigger
                }

                // in Tixa
                if (gWorldSectorX == TIXA_SECTOR_X && gWorldSectorY == TIXA_SECTOR_Y)
                {
                    // screen out everyone but the warden
                    if (pSoldier.ubProfile != NPCID.WARDEN)
                    {
                        break;
                    }

                    // screen out the second/later panic trigger if the first one hasn't been triggered
                    if (bLoop > 0 && gTacticalStatus.sPanicTriggerGridNo[bLoop - 1] != NOWHERE)
                    {
                        break;
                    }
                }

                sDistance = IsometricUtils.PythSpacesAway(pSoldier.sGridNo, gTacticalStatus.sPanicTriggerGridNo[bLoop]);
                if (sDistance < sClosestDistance)
                {
                    sClosestDistance = sDistance;
                    bClosestTrigger = bLoop;
                }
            }
        }

        return (bClosestTrigger);
    }

    bool NeedToRadioAboutPanicTrigger()
    {
        int uiPercentEnemiesKilled;
        int bLoop;

        if (!(gTacticalStatus.fPanicFlags.HasFlag(PANIC.TRIGGERS_HERE))
            || gTacticalStatus.ubTheChosenOne != NOBODY)
        {
            // already done!
            return (false);
        }

        if (gTacticalStatus.Team[ENEMY_TEAM].bMenInSector == 0)
        {
            return (false);
        }

        if (gWorldSectorX == TIXA_SECTOR_X && gWorldSectorY == TIXA_SECTOR_Y)
        {
            SOLDIERTYPE? pSoldier;
            pSoldier = SoldierProfileSubSystem.FindSoldierByProfileID(NPCID.WARDEN, false);
            if (pSoldier is null || pSoldier.ubID == gTacticalStatus.ubTheChosenOne)
            {
                return (false);
            }
        }


        uiPercentEnemiesKilled = (int)(100 * (int)(gTacticalStatus.ubArmyGuysKilled) / (int)(gTacticalStatus.Team[ENEMY_TEAM].bMenInSector + gTacticalStatus.ubArmyGuysKilled));

        for (bLoop = 0; bLoop < NUM_PANIC_TRIGGERS; bLoop++)
        {
            // if the bomb exists and its tolerance has been exceeded
            if ((gTacticalStatus.sPanicTriggerGridNo[bLoop] != NOWHERE) && (uiPercentEnemiesKilled >= gTacticalStatus.ubPanicTolerance[bLoop]))
            {
                return (true);
            }
        }

        return (false);
    }

    AI_ACTION HeadForTheStairCase(SOLDIERTYPE? pSoldier)
    {
        UNDERGROUND_SECTORINFO? pBasementInfo;

        pBasementInfo = QueenCommand.FindUnderGroundSector(3, MAP_ROW.P, 1);
        if (pBasementInfo is not null && pBasementInfo.uiTimeCurrentSectorWasLastLoaded != 0 && (pBasementInfo.ubNumElites + pBasementInfo.ubNumTroops + pBasementInfo.ubNumAdmins) < 5)
        {
            return (AI_ACTION.NONE);
        }

        if (IsometricUtils.PythSpacesAway(pSoldier.sGridNo, STAIRCASE_GRIDNO) < 2)
        {
            return (AI_ACTION.TRAVERSE_DOWN);
        }
        else
        {
            if (Movement.LegalNPCDestination(pSoldier, STAIRCASE_GRIDNO, ENSURE_PATH, WATEROK, 0))
            {
                pSoldier.usActionData = STAIRCASE_GRIDNO;
                return (AI_ACTION.GET_CLOSER);
            }
        }
        return (AI_ACTION.NONE);
    }
}
