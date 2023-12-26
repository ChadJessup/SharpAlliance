using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public partial class Globals
{

}

public class Knowledge
{
    void CallAvailableEnemiesTo(int sGridNo)
    {
        TEAM iLoop;
        int iLoop2;

        // All enemy teams become aware of a very important "noise" coming from here!
        for (iLoop = 0; iLoop < LAST_TEAM; iLoop++)
        {
            // if this team is active
            if (gTacticalStatus.Team[iLoop].IsTeamActive)
            {
                // if this team is computer-controlled, and isn't the CIVILIAN "team"
                if ((!gTacticalStatus.Team[iLoop].IsHuman) && (iLoop != CIV_TEAM))
                {
                    // make this team (publicly) aware of the "noise"
                    gsPublicNoiseGridno[iLoop] = sGridNo;
                    gubPublicNoiseVolume[iLoop] = MAX_MISC_NOISE_DURATION;

                    // new situation for everyone;
                    iLoop2 = gTacticalStatus.Team[iLoop].bFirstID;
                    foreach (var pSoldier in MercPtrs.Skip(iLoop2))//.Where(iLoop2 => iLoop2.bLastId <= gTacticalStatus.Team[iLoop].bLastID))
                    {
                        if (pSoldier.IsActive && pSoldier.bInSector && pSoldier.bLife >= OKLIFE)
                        {
                            AIMain.SetNewSituation(pSoldier);
                            AIUtils.WearGasMaskIfAvailable(pSoldier);
                        }

                        iLoop2++;
                    }
                }
            }
        }
    }

    void CallAvailableTeamEnemiesTo(int sGridno, TEAM bTeam)
    {
        int iLoop2;

        // All enemy teams become aware of a very important "noise" coming from here!
        // if this team is active
        if (gTacticalStatus.Team[bTeam].IsTeamActive)
        {
            // if this team is computer-controlled, and isn't the CIVILIAN "team"
            if (!gTacticalStatus.Team[bTeam].IsHuman && (bTeam != CIV_TEAM))
            {
                // make this team (publicly) aware of the "noise"
                gsPublicNoiseGridno[bTeam] = sGridno;
                gubPublicNoiseVolume[bTeam] = MAX_MISC_NOISE_DURATION;

                // new situation for everyone;
                iLoop2 = gTacticalStatus.Team[bTeam].bFirstID;
                //for (pSoldier = MercPtrs[iLoop2]; iLoop2 <= gTacticalStatus.Team[bTeam].bLastID; iLoop2++, pSoldier++)
                foreach (var pSoldier in MercPtrs.Skip(iLoop2))
                {
                    if (pSoldier.IsActive && pSoldier.bInSector && pSoldier.bLife >= OKLIFE)
                    {
                        AIMain.SetNewSituation(pSoldier);
                        AIUtils.WearGasMaskIfAvailable(pSoldier);
                    }
                }

            }
        }
    }

    void CallAvailableKingpinMenTo(int sGridNo)
    {
        // like call all enemies, but only affects civgroup KINGPIN guys with 
        // NO PROFILE

        int iLoop2;

        // All enemy teams become aware of a very important "noise" coming from here!
        // if this team is active
        if (gTacticalStatus.Team[CIV_TEAM].IsTeamActive)
        {
            // make this team (publicly) aware of the "noise"
            gsPublicNoiseGridno[CIV_TEAM] = sGridNo;
            gubPublicNoiseVolume[CIV_TEAM] = MAX_MISC_NOISE_DURATION;

            // new situation for everyone...

            iLoop2 = gTacticalStatus.Team[CIV_TEAM].bFirstID;
            //for (pSoldier = MercPtrs[iLoop2]; iLoop2 <= gTacticalStatus.Team[CIV_TEAM].bLastID; iLoop2++, pSoldier++)
            foreach (var pSoldier in MercPtrs)
            {
                if (pSoldier.IsActive
                    && pSoldier.bInSector
                    && pSoldier.bLife >= OKLIFE
                    && pSoldier.ubCivilianGroup == CIV_GROUP.KINGPIN_CIV_GROUP
                    && pSoldier.ubProfile == NO_PROFILE)
                {
                    AIMain.SetNewSituation(pSoldier);
                }
            }
        }
    }

    void CallEldinTo(int sGridNo)
    {
        // like call all enemies, but only affects Eldin
        SOLDIERTYPE? pSoldier;

        // Eldin becomes aware of a very important "noise" coming from here!
        // So long as he hasn't already heard a noise a sec ago...
        if (gTacticalStatus.Team[CIV_TEAM].IsTeamActive)
        {
            // new situation for Eldin
            pSoldier = SoldierProfileSubSystem.FindSoldierByProfileID(NPCID.ELDIN, false);
            if (pSoldier is not null && pSoldier.IsActive && pSoldier.bInSector && pSoldier.bLife >= OKLIFE && (pSoldier.bAlertStatus == STATUS.GREEN || pSoldier.ubNoiseVolume < (MAX_MISC_NOISE_DURATION / 2)))
            {
                if (LOS.SoldierToLocationLineOfSightTest(pSoldier, sGridNo, OppList.MaxDistanceVisible(), 1))
                {
                    // sees the player now!
                    NPC.TriggerNPCWithIHateYouQuote(NPCID.ELDIN);
                    AIMain.SetNewSituation(pSoldier);
                }
                else
                {
                    pSoldier.sNoiseGridno = sGridNo;
                    pSoldier.ubNoiseVolume = MAX_MISC_NOISE_DURATION;
                    pSoldier.bAlertStatus = STATUS.RED;
                    if ((pSoldier.bAction != AI_ACTION.GET_CLOSER) || Facts.CheckFact(FACT.MUSEUM_ALARM_WENT_OFF, 0) == false)
                    {
                        AIMain.CancelAIAction(pSoldier, 1);
                        pSoldier.bNextAction = AI_ACTION.GET_CLOSER;
                        pSoldier.usNextActionData = sGridNo;
                        RESETTIMECOUNTER(ref pSoldier.AICounter, 100);
                    }
                    // otherwise let AI handle this normally
                    //				SetNewSituation( pSoldier );
                    // reduce any delay to minimal
                }
                Facts.SetFactTrue(FACT.MUSEUM_ALARM_WENT_OFF);
            }
        }
    }


    public static int MostImportantNoiseHeard(SOLDIERTYPE pSoldier, ref int piRetValue, out bool pfClimbingNecessary, out bool pfReachable)
    {
        pfClimbingNecessary = false;
        pfReachable = false;

        int uiLoop;
        int pbPersOL;
        int pbPublOL;
        int psLastLoc, psNoiseGridNo;
        int pbNoiseLevel;
        int pbLastLevel;
        int pubNoiseVolume;
        int iDistAway;
        int iNoiseValue, iBestValue = -10000;
        int sBestGridNo = NOWHERE;
        int bBestLevel = 0;
        int sClimbingGridNo;
        bool fClimbingNecessary = false;
        SOLDIERTYPE? pTemp;

        pubNoiseVolume = gubPublicNoiseVolume[pSoldier.bTeam];
        psNoiseGridNo = gsPublicNoiseGridno[pSoldier.bTeam];
        pbNoiseLevel = gbPublicNoiseLevel[pSoldier.bTeam];

        psLastLoc = gsLastKnownOppLoc[pSoldier.ubID].First();

        // hang pointers at start of this guy's personal and public opponent opplists
        pbPersOL = pSoldier.bOppList.First();
        pbPublOL = gbPublicOpplist[pSoldier.bTeam].First();

        // look through this man's personal & public opplists for opponents heard
        for (uiLoop = 0; uiLoop < guiNumMercSlots; uiLoop++)
        {
            pTemp = MercSlots[uiLoop];

            // if this merc is inactive, at base, on assignment, or dead
            if (pTemp is not null || pTemp.bLife == 0)
            {
                continue;          // next merc
            }

            // if this merc is neutral/on same side, he's not an opponent
            if (CONSIDERED_NEUTRAL(pSoldier, pTemp) || (pSoldier.bSide == pTemp.bSide))
            {
                continue;          // next merc
            }

            pbPersOL = pSoldier.bOppList[pTemp.ubID];
            pbPublOL = gbPublicOpplist[pSoldier.bTeam][pTemp.ubID];
            psLastLoc = gsLastKnownOppLoc[pSoldier.ubID][pTemp.ubID];
            pbLastLevel = gbLastKnownOppLevel[pSoldier.ubID][pTemp.ubID];

            // if this guy's been personally heard within last 3 turns
            if (pbPersOL < NOT_HEARD_OR_SEEN)
            {
                // calculate how far this noise was, and its relative "importance"
                iDistAway = IsometricUtils.SpacesAway(pSoldier.sGridNo, psLastLoc);
                iNoiseValue = pbPersOL * iDistAway;               // always a negative number!

                if (iNoiseValue > iBestValue)
                {
                    iBestValue = iNoiseValue;
                    sBestGridNo = psLastLoc;
                    bBestLevel = pbLastLevel;
                }
            }

            // if this guy's been publicly heard within last 3 turns
            if (pbPublOL < NOT_HEARD_OR_SEEN)
            {
                // calculate how far this noise was, and its relative "importance"
                iDistAway = IsometricUtils.SpacesAway(pSoldier.sGridNo, gsPublicLastKnownOppLoc[pSoldier.bTeam][pTemp.ubID]);
                iNoiseValue = pbPublOL * iDistAway;               // always a negative number!

                if (iNoiseValue > iBestValue)
                {
                    iBestValue = iNoiseValue;
                    sBestGridNo = gsPublicLastKnownOppLoc[pSoldier.bTeam][pTemp.ubID];
                    bBestLevel = gbPublicLastKnownOppLevel[pSoldier.bTeam][pTemp.ubID];
                }
            }

        }

        // if any "misc. noise" was also heard recently
        if (pSoldier.sNoiseGridno != NOWHERE)
        {
            if (pSoldier.bNoiseLevel != pSoldier.bLevel
                || IsometricUtils.PythSpacesAway(pSoldier.sGridNo, pSoldier.sNoiseGridno) >= 6
                || !LOS.SoldierTo3DLocationLineOfSightTest(pSoldier, pSoldier.sNoiseGridno, pSoldier.bNoiseLevel, 0, OppList.MaxDistanceVisible(), 0))
            {
                // calculate how far this noise was, and its relative "importance"
                iDistAway = IsometricUtils.SpacesAway(pSoldier.sGridNo, pSoldier.sNoiseGridno);
                iNoiseValue = ((pSoldier.ubNoiseVolume / 2) - 6) * iDistAway;

                if (iNoiseValue > iBestValue)
                {
                    iBestValue = iNoiseValue;
                    sBestGridNo = pSoldier.sNoiseGridno;
                    bBestLevel = pSoldier.bNoiseLevel;
                }
            }
            else
            {
                // we are there or near
                pSoldier.sNoiseGridno = NOWHERE;        // wipe it out, not useful anymore
                pSoldier.ubNoiseVolume = 0;
            }
        }


        // if any recent PUBLIC "misc. noise" is also known
        if ((pSoldier.bTeam != CIV_TEAM) || (pSoldier.ubCivilianGroup == CIV_GROUP.KINGPIN_CIV_GROUP))
        {

            if (psNoiseGridNo != NOWHERE)
            {
                // if we are NOT there (at the noise gridno)
                if (pbNoiseLevel != pSoldier.bLevel
                    || IsometricUtils.PythSpacesAway(pSoldier.sGridNo, psNoiseGridNo) >= 6
                    || !LOS.SoldierTo3DLocationLineOfSightTest(pSoldier, psNoiseGridNo, pbNoiseLevel, 0, OppList.MaxDistanceVisible(), 0))
                {
                    // calculate how far this noise was, and its relative "importance"
                    iDistAway = IsometricUtils.SpacesAway(pSoldier.sGridNo, psNoiseGridNo);
                    iNoiseValue = ((pubNoiseVolume / 2) - 6) * iDistAway;

                    if (iNoiseValue > iBestValue)
                    {
                        iBestValue = iNoiseValue;
                        sBestGridNo = psNoiseGridNo;
                        bBestLevel = pbNoiseLevel;
                    }
                }
            }

        }

        if (sBestGridNo != NOWHERE && pfReachable)
        {
            pfReachable = true;

            // make civs not walk to noises outside their room if on close patrol/onguard
            if (pSoldier.bOrders <= Orders.CLOSEPATROL && (pSoldier.bTeam == CIV_TEAM || pSoldier.ubProfile != NO_PROFILE))
            {

                // any other combo uses the default of ubRoom == 0, set above
                if (RenderFun.InARoom(pSoldier.usPatrolGrid[0], out int ubRoom))
                {
                    if (!RenderFun.InARoom(pSoldier.usPatrolGrid[0], out int ubNewRoom) || ubRoom != ubNewRoom)
                    {
                        pfReachable = false;
                    }
                }
            }

            if (pfReachable)
            {
                // if there is a climb involved then we should store the location 
                // of where we have to climb to instead
                sClimbingGridNo = AIUtils.GetInterveningClimbingLocation(pSoldier, sBestGridNo, bBestLevel, out fClimbingNecessary);
                if (fClimbingNecessary)
                {
                    if (sClimbingGridNo == NOWHERE)
                    {
                        // can't investigate!
                        pfReachable = false;
                    }
                    else
                    {
                        sBestGridNo = sClimbingGridNo;
                        fClimbingNecessary = true;
                    }
                }
                else
                {
                    fClimbingNecessary = false;
                }
            }
        }

        if (piRetValue > 0)
        {
            piRetValue = iBestValue;
        }

        if (pfClimbingNecessary)
        {
            pfClimbingNecessary = fClimbingNecessary;
        }

# if DEBUGDECISIONS
        if (sBestGridNo != NOWHERE)
            AINumMessage("MOST IMPORTANT NOISE HEARD FROM GRID #", sBestGridNo);
#endif

        return sBestGridNo;
    }


    int WhatIKnowThatPublicDont(SOLDIERTYPE pSoldier, int ubInSightOnly)
    {
        int ubTotal = 0;
        int uiLoop;
        int pbPersOL, pbPublOL;
        SOLDIERTYPE? pTemp;

        // if merc knows of a more important misc. noise than his team does
        if (!CREATURE_OR_BLOODCAT(pSoldier) && (pSoldier.ubNoiseVolume > gubPublicNoiseVolume[pSoldier.bTeam]))
        {
            // the difference in volume is added to the "new info" total
            ubTotal += pSoldier.ubNoiseVolume - gubPublicNoiseVolume[pSoldier.bTeam];
        }

        // hang pointers at start of this guy's personal and public opponent opplists
        pbPersOL = pSoldier.bOppList[0];
        pbPublOL = gbPublicOpplist[pSoldier.bTeam][0];

        // for every opponent
        //	for (iLoop = 0; iLoop < MAXMERCS; iLoop++,pbPersOL++,pbPublOL++)
        //	{
        //	pTemp = &(Menptr[iLoop]);


        for (uiLoop = 0; uiLoop < guiNumMercSlots; uiLoop++)
        {
            pTemp = MercSlots[uiLoop];

            // if this merc is inactive, at base, on assignment, or dead
            if (pTemp is null)
            {
                continue;          // next merc
            }

            // if this merc is neutral/on same side, he's not an opponent
            if (CONSIDERED_NEUTRAL(pSoldier, pTemp) || (pSoldier.bSide == pTemp.bSide))
            {
                continue;          // next merc
            }

            pbPersOL = pSoldier.bOppList[pTemp.ubID];
            pbPublOL = gbPublicOpplist[pSoldier.bTeam][pTemp.ubID];


            // if we're only interested in guys currently is sight, and he's not
            if (ubInSightOnly > 0)
            {
                if ((pbPersOL == SEEN_CURRENTLY) && (pbPublOL != SEEN_CURRENTLY))
                {
                    // just count the number of them
                    ubTotal++;
                }
            }
            else
            {
                // add value of personal knowledge compared to public knowledge to total
                ubTotal += gubKnowledgeValue[pbPublOL - OLDEST_HEARD_VALUE, pbPersOL - OLDEST_HEARD_VALUE];
            }
        }

# if DEBUGDECISIONS
        if (ubTotal > 0)
        {
            AINumMessage("WHAT I KNOW THAT PUBLIC DON'T = ", ubTotal);
        }
#endif

        return ubTotal;
    }

}
