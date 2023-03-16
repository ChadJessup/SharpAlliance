using SharpAlliance.Core.SubSystems;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class DecideAction
{
    void DecideAlertStatus(SOLDIERTYPE pSoldier)
    {
        STATUS bOldStatus;
        int  iDummy;
        bool fClimbDummy, fReachableDummy;

        // THE FOUR (4) POSSIBLE ALERT STATUSES ARE:
        // GREEN - No one seen, no suspicious noise heard, go about regular duties
        // YELLOW - Suspicious noise was heard personally or radioed in by buddy
        // RED - Either saw opponents in person, or definite contact had been radioed
        // BLACK - Currently has one or more opponents in sight

        // save the man's previous status

        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.MONSTER))
        {
            CreatureDecideAlertStatus(pSoldier);
            return;
        }

        bOldStatus = pSoldier.bAlertStatus;

        // determine the current alert status for this category of man
        //if (!(pSoldier.uiStatusFlags & SOLDIER_PC))
        {
            if (pSoldier.bOppCnt > 0)        // opponent(s) in sight
            {
                pSoldier.bAlertStatus = STATUS.BLACK;
                AIMain.CheckForChangingOrders(pSoldier);
            }
            else                        // no opponents are in sight
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
                        if (!PTR_CIVILIAN && (gTacticalStatus.Team[pSoldier.bTeam].bAwareOfOpposition > 0 || pSoldier.bUnderFire > 0))
                        {
                            pSoldier.bAlertStatus = STATUS.RED;
                        }
                        else
                        {
                            // if we are NOT aware of any uninvestigated noises right now
                            // and we are not currently in the middle of an action
                            // (could still be on his way heading to investigate a noise!)
                            if ((MostImportantNoiseHeard(pSoldier, out iDummy, out fClimbDummy, out fReachableDummy) == NOWHERE)
                                && pSoldier.bActionInProgress == 0)
                            {
                                // then drop back to GREEN status
                                pSoldier.bAlertStatus = STATUS.GREEN;
                                AIMain.CheckForChangingOrders(pSoldier);
                            }
                        }
                        break;

                    case STATUS.GREEN:
                        // if all enemies have been RED alerted, or we're under fire
                        if (!PTR_CIVILIAN && (gTacticalStatus.Team[pSoldier.bTeam].bAwareOfOpposition > 0 || pSoldier.bUnderFire > 0))
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
        }


        if (gTacticalStatus.bBoxingState == BoxingStates.NOT_BOXING)
        {

            // if the man's alert status has changed in any way
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
                    AIMain.CheckForChangingOrders(pSoldier);
                }
            }
            else   // status didn't change
            {
                // only do this stuff in TB
                // if a guy on status GREEN or YELLOW is running low on breath
                if (((pSoldier.bAlertStatus == STATUS.GREEN) && (pSoldier.bBreath < 75)) ||
                    ((pSoldier.bAlertStatus == STATUS.YELLOW) && (pSoldier.bBreath < 50)))
                {
                    // as long as he's not in water (standing on a bridge is OK)
                    if (!MercInWater(pSoldier))
                    {
                        // force a NEW decision so that he can get some rest
                        SetNewSituation(pSoldier);

                        // current action will be canceled. if noise is no longer important
                        if ((pSoldier.bAlertStatus == STATUS.YELLOW) &&
                            (MostImportantNoiseHeard(pSoldier, out iDummy, out fClimbDummy, out fReachableDummy) == NOWHERE))
                        {
                            // then drop back to GREEN status
                            pSoldier.bAlertStatus = STATUS.GREEN;
                            AIMain.CheckForChangingOrders(pSoldier);
                        }
                    }
                }
            }
        }
    }
}
