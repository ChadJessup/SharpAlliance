using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.SubSystems;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class Morale
{
    // macros
    public static bool SOLDIER_IN_SECTOR(SOLDIERTYPE pSoldier, int sX, MAP_ROW sY, int bZ)
        => (!pSoldier.fBetweenSectors && (pSoldier.sSectorX == sX) && (pSoldier.sSectorY == sY) && (pSoldier.bSectorZ == bZ));



    public static Dictionary<MoraleEventNames, MoraleEvent> gbMoraleEvent = new()
    {
    	// TACTICAL = Short Term Effect, STRATEGIC = Long Term Effect
    	{ MoraleEventNames.MORALE_KILLED_ENEMY, new(MoraleEventType.TACTICAL_MORALE_EVENT,          +4) },	//	MORALE_KILLED_ENEMY
    	{ MoraleEventNames.MORALE_SQUADMATE_DIED, new(MoraleEventType.TACTICAL_MORALE_EVENT,          -5) },	//	MORALE_SQUADMATE_DIED,		// in same sector (not really squad)... IN ADDITION to strategic loss of morale
    	{ MoraleEventNames.MORALE_SUPPRESSED, new(MoraleEventType.TACTICAL_MORALE_EVENT,            -1) },	//	MORALE_SUPPRESSED,				// up to 4 times per turn
    	{ MoraleEventNames.MORALE_AIRSTRIKE, new(MoraleEventType.TACTICAL_MORALE_EVENT,            -2) },	//	MORALE_AIRSTRIKE,
    	{ MoraleEventNames.MORALE_DID_LOTS_OF_DAMAGE, new(MoraleEventType.TACTICAL_MORALE_EVENT,            +2) },	//	MORALE_DID_LOTS_OF_DAMAGE,
    	{ MoraleEventNames.MORALE_TOOK_LOTS_OF_DAMAGE, new(MoraleEventType.TACTICAL_MORALE_EVENT,            -3) },	//	MORALE_TOOK_LOTS_OF_DAMAGE,
    	{ MoraleEventNames.MORALE_KILLED_CIVILIAN, new(MoraleEventType.STRATEGIC_MORALE_EVENT,           -5) },	//	MORALE_KILLED_CIVILIAN,
    	{ MoraleEventNames.MORALE_BATTLE_WON, new(MoraleEventType.STRATEGIC_MORALE_EVENT,           +4) },	//	MORALE_BATTLE_WON,
    	{ MoraleEventNames.MORALE_RAN_AWAY, new(MoraleEventType.STRATEGIC_MORALE_EVENT,           -5) },	//	MORALE_RAN_AWAY,
    	{ MoraleEventNames.MORALE_HEARD_BATTLE_WON, new(MoraleEventType.STRATEGIC_MORALE_EVENT,           +2) },	//	MORALE_HEARD_BATTLE_WON,
    	{ MoraleEventNames.MORALE_HEARD_BATTLE_LOST, new(MoraleEventType.STRATEGIC_MORALE_EVENT,           -2) },	//	MORALE_HEARD_BATTLE_LOST,
    	{ MoraleEventNames.MORALE_TOWN_LIBERATED, new(MoraleEventType.STRATEGIC_MORALE_EVENT,           +5) },	//	MORALE_TOWN_LIBERATED,
    	{ MoraleEventNames.MORALE_TOWN_LOST, new(MoraleEventType.STRATEGIC_MORALE_EVENT,           -5) },	//	MORALE_TOWN_LOST,
    	{ MoraleEventNames.MORALE_MINE_LIBERATED, new(MoraleEventType.STRATEGIC_MORALE_EVENT,           +8) },	//	MORALE_MINE_LIBERATED,
    	{ MoraleEventNames.MORALE_MINE_LOST, new(MoraleEventType.STRATEGIC_MORALE_EVENT,           -8) },	//	MORALE_MINE_LOST,
    	{ MoraleEventNames.MORALE_SAM_SITE_LIBERATED, new(MoraleEventType.STRATEGIC_MORALE_EVENT,           +3) },	//	MORALE_SAM_SITE_LIBERATED,
    	{ MoraleEventNames.MORALE_SAM_SITE_LOST, new(MoraleEventType.STRATEGIC_MORALE_EVENT,           -3) },	//	MORALE_SAM_SITE_LOST,
    	{ MoraleEventNames.MORALE_BUDDY_DIED, new(MoraleEventType.STRATEGIC_MORALE_EVENT,        -15) },	//	MORALE_BUDDY_DIED,
    	{ MoraleEventNames.MORALE_HATED_DIED, new(MoraleEventType.STRATEGIC_MORALE_EVENT,           +5) },	//	MORALE_HATED_DIED,
    	{ MoraleEventNames.MORALE_TEAMMATE_DIED, new(MoraleEventType.STRATEGIC_MORALE_EVENT,           -5) },	//	MORALE_TEAMMATE_DIED,			// not in same sector
    	{ MoraleEventNames.MORALE_LOW_DEATHRATE, new(MoraleEventType.STRATEGIC_MORALE_EVENT,           +5) },	//	MORALE_LOW_DEATHRATE,
    	{ MoraleEventNames.MORALE_HIGH_DEATHRATE, new(MoraleEventType.STRATEGIC_MORALE_EVENT,           -5) },	//	MORALE_HIGH_DEATHRATE,
    	{ MoraleEventNames.MORALE_GREAT_MORALE, new(MoraleEventType.STRATEGIC_MORALE_EVENT,           +2) },	//	MORALE_GREAT_MORALE,
    	{ MoraleEventNames.MORALE_POOR_MORALE, new(MoraleEventType.STRATEGIC_MORALE_EVENT,           -2) },	//	MORALE_POOR_MORALE,
    	{ MoraleEventNames.MORALE_DRUGS_CRASH, new(MoraleEventType.TACTICAL_MORALE_EVENT,         -10) },	//  MORALE_DRUGS_CRASH
    	{ MoraleEventNames.MORALE_ALCOHOL_CRASH, new(MoraleEventType.TACTICAL_MORALE_EVENT,         -10) },	//  MORALE_ALCOHOL_CRASH
    	{ MoraleEventNames.MORALE_MONSTER_QUEEN_KILLED, new(MoraleEventType.STRATEGIC_MORALE_EVENT,        +15) },	//  MORALE_MONSTER_QUEEN_KILLED
    	{ MoraleEventNames.MORALE_DEIDRANNA_KILLED, new(MoraleEventType.STRATEGIC_MORALE_EVENT,        +25) },	//  MORALE_DEIDRANNA_KILLED
    	{ MoraleEventNames.MORALE_CLAUSTROPHOBE_UNDERGROUND, new(MoraleEventType.TACTICAL_MORALE_EVENT,            -1) },	//	MORALE_CLAUSTROPHOBE_UNDERGROUND,
    	{ MoraleEventNames.MORALE_INSECT_PHOBIC_SEES_CREATURE, new(MoraleEventType.TACTICAL_MORALE_EVENT,            -5) },	//	MORALE_INSECT_PHOBIC_SEES_CREATURE,
    	{ MoraleEventNames.MORALE_NERVOUS_ALONE, new(MoraleEventType.TACTICAL_MORALE_EVENT,            -1) },	//	MORALE_NERVOUS_ALONE,
    	{ MoraleEventNames.MORALE_MERC_CAPTURED, new(MoraleEventType.STRATEGIC_MORALE_EVENT,          -5) },	//	MORALE_MERC_CAPTURED,
    	{ MoraleEventNames.MORALE_MERC_MARRIED, new(MoraleEventType.STRATEGIC_MORALE_EVENT,             -5) },	//	MORALE_MERC_MARRIED,
    	{ MoraleEventNames.MORALE_QUEEN_BATTLE_WON, new(MoraleEventType.STRATEGIC_MORALE_EVENT,             +8) },	//	MORALE_QUEEN_BATTLE_WON,
    	{ MoraleEventNames.MORALE_SEX, new(MoraleEventType.STRATEGIC_MORALE_EVENT,             +5) },	//  MORALE_SEX,
    };

    private static MORALE GetMoraleModifier(SOLDIERTYPE? pSoldier)
    {
        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.PC))
        {
            if (pSoldier.bMorale > 50)
            {
                // give +1 at 55, +3 at 65, up to +5 at 95 and above
                return (MORALE)((pSoldier.bMorale - 45) / 10);
            }
            else
            {
                // give penalties down to -20 at 0 (-2 at 45, -4 by 40...)
                return (MORALE)((pSoldier.bMorale - 50) * 2 / 5);
            }
        }
        else
        {
            // use AI morale
            return (MORALE)pSoldier.bAIMorale switch
            {
                MORALE.HOPELESS => (MORALE)(-15),
                MORALE.WORRIED => (MORALE)(-7),
                MORALE.CONFIDENT => (MORALE)(2),
                MORALE.FEARLESS => (MORALE)(5),
                _ => (0),
            };
        }
    }

    void DecayTacticalMorale(SOLDIERTYPE? pSoldier)
    {
        // decay the tactical morale modifier
        if (pSoldier.bTacticalMoraleMod != 0)
        {
            // decay the modifier!
            if (pSoldier.bTacticalMoraleMod > 0)
            {
                pSoldier.bTacticalMoraleMod = Math.Max(0, pSoldier.bTacticalMoraleMod - (8 - pSoldier.bTacticalMoraleMod / 10));
            }
            else
            {
                pSoldier.bTacticalMoraleMod = Math.Min(0, pSoldier.bTacticalMoraleMod + (6 + pSoldier.bTacticalMoraleMod / 10));
            }
        }
    }

    void DecayStrategicMorale(SOLDIERTYPE? pSoldier)
    {
        // decay the modifier!
        if (pSoldier.bStrategicMoraleMod > 0)
        {
            pSoldier.bStrategicMoraleMod = Math.Max(0, pSoldier.bStrategicMoraleMod - (8 - pSoldier.bStrategicMoraleMod / 10));
        }
        else
        {
            pSoldier.bStrategicMoraleMod = Math.Min(0, pSoldier.bStrategicMoraleMod + (6 + pSoldier.bStrategicMoraleMod / 10));
        }
    }

    void DecayTacticalMoraleModifiers()
    {
        int ubLoop, ubLoop2;
        bool fHandleNervous;

        ubLoop = gTacticalStatus.Team[gbPlayerNum].bFirstID;

        foreach (var pSoldier in MercPtrs)
        {
            //if the merc is active, in Arulco
            // CJC: decay modifiers while asleep! or POW!
            if (pSoldier.bActive
                && pSoldier.ubProfile != NO_PROFILE
                && !(pSoldier.bAssignment == Assignments.IN_TRANSIT || pSoldier.bAssignment == Assignments.ASSIGNMENT_DEAD))
            {
                // only let morale mod decay if it is positive while merc is a POW
                if (pSoldier.bAssignment == Assignments.ASSIGNMENT_POW && pSoldier.bTacticalMoraleMod < 0)
                {
                    continue;
                }

                switch (gMercProfiles[pSoldier.ubProfile].bPersonalityTrait)
                {
                    case PersonalityTrait.CLAUSTROPHOBIC:
                        if (pSoldier.bSectorZ > 0)
                        {
                            // underground, no recovery... in fact, if tact morale is high, decay
                            if (pSoldier.bTacticalMoraleMod > PHOBIC_LIMIT)
                            {
                                HandleMoraleEvent(pSoldier, MoraleEventNames.MORALE_CLAUSTROPHOBE_UNDERGROUND, pSoldier.sSectorX, pSoldier.sSectorY, pSoldier.bSectorZ);
                            }
                            continue;
                        }
                        break;
                    case PersonalityTrait.NERVOUS:
                        if (pSoldier.bMorale < 50)
                        {
                            if (pSoldier.ubGroupID != 0 && StrategicMovement.PlayerIDGroupInMotion(pSoldier.ubGroupID))
                            {
                                if (NumberOfPeopleInSquad(pSoldier.bAssignment) == 1)
                                {
                                    fHandleNervous = true;
                                }
                                else
                                {
                                    fHandleNervous = false;
                                }
                            }
                            else if (pSoldier.bActive && pSoldier.bInSector)
                            {
                                if (AIUtils.DistanceToClosestFriend(pSoldier) > NERVOUS_RADIUS)
                                {
                                    fHandleNervous = true;
                                }
                                else
                                {
                                    fHandleNervous = false;
                                }
                            }
                            else
                            {
                                // look for anyone else in same sector
                                fHandleNervous = true;
                                for (ubLoop2 = gTacticalStatus.Team[gbPlayerNum].bFirstID; ubLoop2 <= gTacticalStatus.Team[gbPlayerNum].bLastID; ubLoop2++)
                                {
                                    if (MercPtrs[ubLoop2] != pSoldier && MercPtrs[ubLoop2].bActive && MercPtrs[ubLoop2].sSectorX == pSoldier.sSectorX && MercPtrs[ubLoop2].sSectorY == pSoldier.sSectorY && MercPtrs[ubLoop2].bSectorZ == pSoldier.bSectorZ)
                                    {
                                        // found someone!
                                        fHandleNervous = false;
                                        break;
                                    }
                                }
                            }

                            if (fHandleNervous)
                            {
                                if (pSoldier.bTacticalMoraleMod == PHOBIC_LIMIT)
                                {
                                    // don't change morale
                                    continue;
                                }

                                // alone, no recovery... in fact, if tact morale is high, decay
                                if (!(pSoldier.usQuoteSaidFlags.HasFlag(SOLDIER_QUOTE.SAID_PERSONALITY)))
                                {
                                    DialogControl.TacticalCharacterDialogue(pSoldier, QUOTE.PERSONALITY_TRAIT);
                                    pSoldier.usQuoteSaidFlags |= SOLDIER_QUOTE.SAID_PERSONALITY;
                                }

                                HandleMoraleEvent(pSoldier, MoraleEventNames.MORALE_NERVOUS_ALONE, pSoldier.sSectorX, pSoldier.sSectorY, pSoldier.bSectorZ);
                                continue;
                            }
                        }
                        break;
                }

                DecayTacticalMorale(pSoldier);
                RefreshSoldierMorale(pSoldier);
            }
        }
    }

    void DecayStrategicMoraleModifiers()
    {
        //ubLoop = gTacticalStatus.Team[gbPlayerNum].bFirstID;

        foreach (var pSoldier in MercPtrs)
        {
            //if the merc is active, in Arulco
            // CJC: decay modifiers while asleep! or POW!
            if (pSoldier.bActive
                && pSoldier.ubProfile != NO_PROFILE
                && !(pSoldier.bAssignment == Assignments.IN_TRANSIT || pSoldier.bAssignment == Assignments.ASSIGNMENT_DEAD))
            {
                // only let morale mod decay if it is positive while merc is a POW
                if (pSoldier.bAssignment == Assignments.ASSIGNMENT_POW && pSoldier.bStrategicMoraleMod < 0)
                {
                    continue;
                }

                DecayStrategicMorale(pSoldier);
                RefreshSoldierMorale(pSoldier);
            }
        }
    }



    public static void RefreshSoldierMorale(SOLDIERTYPE? pSoldier)
    {
        int iActualMorale;

        if (pSoldier.fMercAsleep)
        {
            // delay this till later!
            return;
        }

        // CJC, April 19, 1999: added up to 20% morale boost according to progress
        iActualMorale = DEFAULT_MORALE + (int)pSoldier.bTeamMoraleMod + (int)pSoldier.bTacticalMoraleMod + (int)pSoldier.bStrategicMoraleMod + (int)(Campaign.CurrentPlayerProgressPercentage() / 5);

        // ATE: Modify morale based on drugs....
        iActualMorale += ((pSoldier.bDrugEffect[DRUG_TYPE_ADRENALINE] * DRUG_EFFECT_MORALE_MOD) / 100);
        iActualMorale += ((pSoldier.bDrugEffect[DRUG_TYPE_ALCOHOL] * ALCOHOL_EFFECT_MORALE_MOD) / 100);

        iActualMorale = Math.Min(100, iActualMorale);
        iActualMorale = Math.Max(0, iActualMorale);
        pSoldier.bMorale = (int)iActualMorale;

        // update mapscreen as needed
        fCharacterInfoPanelDirty = true;
    }


    public static void UpdateSoldierMorale(SOLDIERTYPE? pSoldier, MoraleEventType ubType, int bMoraleMod)
    {
        MERCPROFILESTRUCT? pProfile;
        int iMoraleModTotal;

        if (!pSoldier.bActive || (pSoldier.bLife < CONSCIOUSNESS) ||
             (pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE)) || AM_A_ROBOT(pSoldier) || AM_AN_EPC(pSoldier))
        {
            return;
        }

        if ((pSoldier.bAssignment == Assignments.ASSIGNMENT_DEAD)
            || (pSoldier.bAssignment == Assignments.ASSIGNMENT_POW)
            || (pSoldier.bAssignment == Assignments.IN_TRANSIT))
        {
            return;
        }


        if (pSoldier.ubProfile == NO_PROFILE)
        {
            return;
        }

        pProfile = (gMercProfiles[pSoldier.ubProfile]);

        if (bMoraleMod > 0)
        {
            switch (pProfile.bAttitude)
            {
                case ATT.OPTIMIST:
                case ATT.AGGRESSIVE:
                    bMoraleMod += 1;
                    break;
                case ATT.PESSIMIST:
                    bMoraleMod -= 1;
                    break;
                default:
                    break;
            }
            if (bMoraleMod < 0)
            {
                // can't change a positive event into a negative one!
                bMoraleMod = 0;
            }
        }
        else
        {
            switch (pProfile.bAttitude)
            {
                case ATT.OPTIMIST:
                    bMoraleMod += 1;
                    break;
                case ATT.PESSIMIST:
                    bMoraleMod -= 1;
                    break;
                case ATT.COWARD:
                    bMoraleMod -= 2;
                    break;
                default:
                    break;
            }
            if (pSoldier.bLevel == 1)
            {
                bMoraleMod--;
            }
            else if (pSoldier.bLevel > 5)
            {
                bMoraleMod++;
            }
            if (bMoraleMod > 0)
            {
                // can't change a negative event into a positive one!
                bMoraleMod = 0;
            }
        }
        // apply change!
        if (ubType == MoraleEventType.TACTICAL_MORALE_EVENT)
        {
            iMoraleModTotal = (int)pSoldier.bTacticalMoraleMod + (int)bMoraleMod;
            iMoraleModTotal = Math.Min(iMoraleModTotal, MORALE_MOD_MAX);
            iMoraleModTotal = Math.Max(iMoraleModTotal, -MORALE_MOD_MAX);
            pSoldier.bTacticalMoraleMod = (int)iMoraleModTotal;
        }
        else if (gTacticalStatus.fEnemyInSector && !pSoldier.bInSector) // delayed strategic
        {
            iMoraleModTotal = (int)pSoldier.bDelayedStrategicMoraleMod + (int)bMoraleMod;
            iMoraleModTotal = Math.Min(iMoraleModTotal, MORALE_MOD_MAX);
            iMoraleModTotal = Math.Max(iMoraleModTotal, -MORALE_MOD_MAX);
            pSoldier.bDelayedStrategicMoraleMod = (int)iMoraleModTotal;
        }
        else // strategic
        {
            iMoraleModTotal = (int)pSoldier.bStrategicMoraleMod + (int)bMoraleMod;
            iMoraleModTotal = Math.Min(iMoraleModTotal, MORALE_MOD_MAX);
            iMoraleModTotal = Math.Max(iMoraleModTotal, -MORALE_MOD_MAX);
            pSoldier.bStrategicMoraleMod = (int)iMoraleModTotal;
        }

        RefreshSoldierMorale(pSoldier);

        if (!pSoldier.fMercAsleep)
        {
            if (!gfSomeoneSaidMoraleQuote)
            {
                // Check if we're below a certain value and warn
                if (pSoldier.bMorale < 35)
                {
                    // Have we said this quote yet?
                    if (!(pSoldier.usQuoteSaidFlags.HasFlag(SOLDIER_QUOTE.SAID_LOW_MORAL)))
                    {
                        gfSomeoneSaidMoraleQuote = true;

                        // ATE: Amde it a DELAYED QUOTE - will be delayed by the dialogue Q until it's our turn...
                        DelayedTacticalCharacterDialogue(pSoldier, QUOTE.STARTING_TO_WHINE);
                        pSoldier.usQuoteSaidFlags |= SOLDIER_QUOTE.SAID_LOW_MORAL;
                    }
                }
            }
        }

        // Reset flag!
        if (pSoldier.bMorale > 65)
        {
            pSoldier.usQuoteSaidFlags &= (~SOLDIER_QUOTE.SAID_LOW_MORAL);
        }

    }


    public static void HandleMoraleEventForSoldier(SOLDIERTYPE? pSoldier, MoraleEventNames bMoraleEvent)
    {
        UpdateSoldierMorale(pSoldier, gbMoraleEvent[bMoraleEvent].ubType, gbMoraleEvent[bMoraleEvent].bChange);
    }


    public static void HandleMoraleEvent(SOLDIERTYPE? pSoldier, MoraleEventNames bMoraleEvent, int sMapX, MAP_ROW sMapY, int bMapZ)
    {
        int ubLoop;
        MERCPROFILESTRUCT? pProfile;

        gfSomeoneSaidMoraleQuote = false;

        // NOTE: Many morale events are NOT attached to a specific player soldier at all!
        // Those that do need it have Asserts on a case by case basis below
        if (pSoldier == null)
        {
            //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("Handling morale event %d at X=%d, Y=%d,Z=%d", bMoraleEvent, sMapX, sMapY, bMapZ));
        }
        else
        {
            //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("Handling morale event %d for %S at X=%d, Y=%d, Z=%d", bMoraleEvent, pSoldier.name, sMapX, sMapY, bMapZ));
        }


        switch (bMoraleEvent)
        {
            case MoraleEventNames.MORALE_KILLED_ENEMY:
            case MoraleEventNames.MORALE_DID_LOTS_OF_DAMAGE:
            case MoraleEventNames.MORALE_DRUGS_CRASH:
            case MoraleEventNames.MORALE_ALCOHOL_CRASH:
            case MoraleEventNames.MORALE_SUPPRESSED:
            case MoraleEventNames.MORALE_TOOK_LOTS_OF_DAMAGE:
            case MoraleEventNames.MORALE_HIGH_DEATHRATE:
            case MoraleEventNames.MORALE_SEX:
                // needs specific soldier!
                Debug.Assert(pSoldier is not null);
                // affects the soldier only
                HandleMoraleEventForSoldier(pSoldier, bMoraleEvent);
                break;

            case MoraleEventNames.MORALE_CLAUSTROPHOBE_UNDERGROUND:
            case MoraleEventNames.MORALE_INSECT_PHOBIC_SEES_CREATURE:
            case MoraleEventNames.MORALE_NERVOUS_ALONE:
                // needs specific soldier!
                Debug.Assert(pSoldier is not null);
                // affects the soldier only, should be ignored if tactical morale mod is -20 or less
                if (pSoldier.bTacticalMoraleMod > PHOBIC_LIMIT)
                {
                    HandleMoraleEventForSoldier(pSoldier, bMoraleEvent);
                }
                break;

            case MoraleEventNames.MORALE_BATTLE_WON:
                // affects everyone to varying degrees
                ubLoop = gTacticalStatus.Team[gbPlayerNum].bFirstID;

                //for (pTeamSoldier = MercPtrs[ubLoop]; ubLoop <= gTacticalStatus.Team[gbPlayerNum].bLastID; ubLoop++, pTeamSoldier++)
                foreach(var pTeamSoldier in MercPtrs)
                {
                    if (pTeamSoldier.bActive)
                    {
                        if (SOLDIER_IN_SECTOR(pTeamSoldier, sMapX, sMapY, bMapZ))
                        {
                            HandleMoraleEventForSoldier(pTeamSoldier, MoraleEventNames.MORALE_BATTLE_WON);
                        }
                        else
                        {
                            HandleMoraleEventForSoldier(pTeamSoldier, MoraleEventNames.MORALE_HEARD_BATTLE_WON);
                        }
                    }
                }
                break;
            case MoraleEventNames.MORALE_RAN_AWAY:
                // affects everyone to varying degrees
                ubLoop = gTacticalStatus.Team[gbPlayerNum].bFirstID;
                //for (pTeamSoldier = MercPtrs[ubLoop]; ubLoop <= gTacticalStatus.Team[gbPlayerNum].bLastID; ubLoop++, pTeamSoldier++)
                foreach(var pTeamSoldier in MercPtrs)
                {
                    if (pTeamSoldier.bActive)
                    {
                        // CJC: adding to SOLDIER_IN_SECTOR check special stuff because the old sector values might
                        // be appropriate (because in transit going out of that sector!)

                        if (SOLDIER_IN_SECTOR(pTeamSoldier, sMapX, sMapY, bMapZ)
                            || (pTeamSoldier.fBetweenSectors
                                && (((int)pTeamSoldier.ubPrevSectorID % 16) + 1) == sMapX
                                && ((MAP_ROW)((int)pTeamSoldier.ubPrevSectorID / 16) + 1) == sMapY
                                && (pTeamSoldier.bSectorZ == bMapZ)))
                        {
                            switch (gMercProfiles[pTeamSoldier.ubProfile].bAttitude)
                            {
                                case ATT.AGGRESSIVE:
                                    // double the penalty - these guys REALLY hate running away
                                    HandleMoraleEventForSoldier(pTeamSoldier, MoraleEventNames.MORALE_RAN_AWAY);
                                    HandleMoraleEventForSoldier(pTeamSoldier, MoraleEventNames.MORALE_RAN_AWAY);
                                    break;
                                case ATT.COWARD:
                                    // no penalty - cowards are perfectly happy to avoid fights!
                                    break;
                                default:
                                    HandleMoraleEventForSoldier(pTeamSoldier, MoraleEventNames.MORALE_RAN_AWAY);
                                    break;
                            }
                        }
                        else
                        {
                            HandleMoraleEventForSoldier(pTeamSoldier, MoraleEventNames.MORALE_HEARD_BATTLE_LOST);
                        }
                    }
                }
                break;

            case MoraleEventNames.MORALE_TOWN_LIBERATED:
            case MoraleEventNames.MORALE_TOWN_LOST:
            case MoraleEventNames.MORALE_MINE_LIBERATED:
            case MoraleEventNames.MORALE_MINE_LOST:
            case MoraleEventNames.MORALE_SAM_SITE_LIBERATED:
            case MoraleEventNames.MORALE_SAM_SITE_LOST:
            case MoraleEventNames.MORALE_KILLED_CIVILIAN:
            case MoraleEventNames.MORALE_LOW_DEATHRATE:
            case MoraleEventNames.MORALE_HEARD_BATTLE_WON:
            case MoraleEventNames.MORALE_HEARD_BATTLE_LOST:
            case MoraleEventNames.MORALE_MONSTER_QUEEN_KILLED:
            case MoraleEventNames.MORALE_DEIDRANNA_KILLED:
                // affects everyone, everywhere
                ubLoop = gTacticalStatus.Team[gbPlayerNum].bFirstID;
                //for (pTeamSoldier = MercPtrs[ubLoop]; ubLoop <= gTacticalStatus.Team[gbPlayerNum].bLastID; ubLoop++, pTeamSoldier++)
                foreach(var pTeamSoldier in MercPtrs)
                {
                    if (pTeamSoldier.bActive)
                    {
                        HandleMoraleEventForSoldier(pTeamSoldier, bMoraleEvent);
                    }
                }
                break;

            case MoraleEventNames.MORALE_POOR_MORALE:
            case MoraleEventNames.MORALE_GREAT_MORALE:
            case MoraleEventNames.MORALE_AIRSTRIKE:
                // affects every in sector
                ubLoop = gTacticalStatus.Team[gbPlayerNum].bFirstID;
                //for (pTeamSoldier = MercPtrs[ubLoop]; ubLoop <= gTacticalStatus.Team[gbPlayerNum].bLastID; ubLoop++, pTeamSoldier++)
                foreach(var pTeamSoldier in MercPtrs)
                {
                    if (pTeamSoldier.bActive && SOLDIER_IN_SECTOR(pTeamSoldier, sMapX, sMapY, bMapZ))
                    {
                        HandleMoraleEventForSoldier(pTeamSoldier, bMoraleEvent);
                    }
                }
                break;

            case MoraleEventNames.MORALE_MERC_CAPTURED:
                // needs specific soldier! (for reputation, not here)
                Debug.Assert(pSoldier is not null);

                // affects everyone
                ubLoop = gTacticalStatus.Team[gbPlayerNum].bFirstID;
                //for (pTeamSoldier = MercPtrs[ubLoop]; ubLoop <= gTacticalStatus.Team[gbPlayerNum].bLastID; ubLoop++, pTeamSoldier++)
                foreach (var pTeamSoldier in MercPtrs)
                {
                    if (pTeamSoldier.bActive)
                    {
                        HandleMoraleEventForSoldier(pTeamSoldier, bMoraleEvent);
                    }
                }
                break;
            case MoraleEventNames.MORALE_TEAMMATE_DIED:
                // needs specific soldier!
                Debug.Assert(pSoldier is not null);

                // affects everyone, in sector differently than not, extra bonuses if it's a buddy or hated merc
                ubLoop = gTacticalStatus.Team[gbPlayerNum].bFirstID;
                //for (pTeamSoldier = MercPtrs[ubLoop]; ubLoop <= gTacticalStatus.Team[gbPlayerNum].bLastID; ubLoop++, pTeamSoldier++)
                foreach(var pTeamSoldier in MercPtrs)
                {
                    if (pTeamSoldier.bActive && pTeamSoldier.ubProfile != NO_PROFILE)
                    {
                        pProfile = (gMercProfiles[pTeamSoldier.ubProfile]);

                        if (HATED_MERC(pProfile, gMercProfiles[pSoldier.ubProfile]))
                        {
                            // yesss!
                            HandleMoraleEventForSoldier(pTeamSoldier, MoraleEventNames.MORALE_HATED_DIED);
                        }
                        else
                        {
                            if (SOLDIER_IN_SECTOR(pTeamSoldier, sMapX, sMapY, bMapZ))
                            {
                                // mate died in my sector!  tactical morale mod
                                HandleMoraleEventForSoldier(pTeamSoldier, MoraleEventNames.MORALE_SQUADMATE_DIED);
                            }

                            // this is handled for everyone even if in sector, as it's a strategic morale mod
                            HandleMoraleEventForSoldier(pTeamSoldier, MoraleEventNames.MORALE_TEAMMATE_DIED);

                            if (BUDDY_MERC(pProfile, gMercProfiles[pSoldier.ubProfile]))
                            {
                                // oh no!  buddy died!
                                HandleMoraleEventForSoldier(pTeamSoldier, MoraleEventNames.MORALE_BUDDY_DIED);
                            }
                        }
                    }
                }
                break;

            case MoraleEventNames.MORALE_MERC_MARRIED:
                // female mercs get unhappy based on how sexist they are (=hate men)
                // gentlemen males get unhappy too

                ubLoop = gTacticalStatus.Team[gbPlayerNum].bFirstID;
                //for (pTeamSoldier = MercPtrs[ubLoop]; ubLoop <= gTacticalStatus.Team[gbPlayerNum].bLastID; ubLoop++, pTeamSoldier++)
                foreach (var pTeamSoldier in MercPtrs)
                {
                    if (pTeamSoldier.bActive && pTeamSoldier.ubProfile != NO_PROFILE)
                    {
//                        if (SoldierProfileSubSystem.WhichHated(pTeamSoldier.ubProfile, pSoldier.ubProfile) != -1)
//                        {
//                            // we hate 'em anyways
//                            continue;
//                        }

                        if (gMercProfiles[pTeamSoldier.ubProfile].bSex == Sexes.FEMALE)
                        {
                            switch (gMercProfiles[pTeamSoldier.ubProfile].bSexist)
                            {
                                case SexistLevels.SOMEWHAT_SEXIST:
                                    HandleMoraleEventForSoldier(pTeamSoldier, MoraleEventNames.MORALE_MERC_MARRIED);
                                    break;
                                case SexistLevels.VERY_SEXIST:
                                    // handle TWICE!
                                    HandleMoraleEventForSoldier(pTeamSoldier, MoraleEventNames.MORALE_MERC_MARRIED);
                                    HandleMoraleEventForSoldier(pTeamSoldier, MoraleEventNames.MORALE_MERC_MARRIED);
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            switch (gMercProfiles[pTeamSoldier.ubProfile].bSexist)
                            {
                                case SexistLevels.GENTLEMAN:
                                    HandleMoraleEventForSoldier(pTeamSoldier, MoraleEventNames.MORALE_MERC_MARRIED);
                                    break;
                                default:
                                    break;
                            }
                        }

                    }
                }
                break;

            default:
                // debug message
                Messages.ScreenMsg(MSG_FONT_RED, MSG_BETAVERSION, "Invalid morale event type = %d.  AM/CC-1", bMoraleEvent.ToString());
                break;
        }


        // some morale events also impact the player's reputation with the mercs back home
        switch (bMoraleEvent)
        {
            case MoraleEventNames.MORALE_HIGH_DEATHRATE:
                StrategicStatus.ModifyPlayerReputation(REPUTATION.HIGH_DEATHRATE);
                break;
            case MoraleEventNames.MORALE_LOW_DEATHRATE:
                StrategicStatus.ModifyPlayerReputation(REPUTATION.LOW_DEATHRATE);
                break;
            case MoraleEventNames.MORALE_POOR_MORALE:
                StrategicStatus.ModifyPlayerReputation(REPUTATION.POOR_MORALE);
                break;
            case MoraleEventNames.MORALE_GREAT_MORALE:
                StrategicStatus.ModifyPlayerReputation(REPUTATION.GREAT_MORALE);
                break;
            case MoraleEventNames.MORALE_BATTLE_WON:
                StrategicStatus.ModifyPlayerReputation(REPUTATION.BATTLE_WON);
                break;
            case MoraleEventNames.MORALE_RAN_AWAY:
            case MoraleEventNames.MORALE_HEARD_BATTLE_LOST:
                StrategicStatus.ModifyPlayerReputation(REPUTATION.BATTLE_LOST);
                break;
            case MoraleEventNames.MORALE_TOWN_LIBERATED:
                StrategicStatus.ModifyPlayerReputation(REPUTATION.TOWN_WON);
                break;
            case MoraleEventNames.MORALE_TOWN_LOST:
                StrategicStatus.ModifyPlayerReputation(REPUTATION.TOWN_LOST);
                break;
            case MoraleEventNames.MORALE_TEAMMATE_DIED:
                // impact depends on that dude's level of experience
                StrategicStatus.ModifyPlayerReputation((REPUTATION)(pSoldier.bExpLevel * (int)REPUTATION.SOLDIER_DIED));
                break;
            case MoraleEventNames.MORALE_MERC_CAPTURED:
                // impact depends on that dude's level of experience
                StrategicStatus.ModifyPlayerReputation((REPUTATION)(pSoldier.bExpLevel * (int)REPUTATION.SOLDIER_CAPTURED));
                break;
            case MoraleEventNames.MORALE_KILLED_CIVILIAN:
                StrategicStatus.ModifyPlayerReputation(REPUTATION.KILLED_CIVILIAN);
                break;
            case MoraleEventNames.MORALE_MONSTER_QUEEN_KILLED:
                StrategicStatus.ModifyPlayerReputation(REPUTATION.KILLED_MONSTER_QUEEN);
                break;
            case MoraleEventNames.MORALE_DEIDRANNA_KILLED:
                StrategicStatus.ModifyPlayerReputation(REPUTATION.KILLED_DEIDRANNA);
                break;

            default:
                // no reputation impact
                break;
        }
    }

    static int bStrategicMoraleUpdateCounter = 0;
    void HourlyMoraleUpdate()
    {
        int bMercID, bOtherID;
        int bActualTeamOpinion;
        int bTeamMoraleModChange, bTeamMoraleModDiff;
        int bOpinion = -1;
        int iTotalOpinions;
        int bNumTeamMembers;
        int bHighestTeamLeadership = 0;
        int bLastTeamID;
        //SOLDIERTYPE? pOtherSoldier;
        MERCPROFILESTRUCT? pProfile;
        bool fSameGroupOnly;
        bool fFoundHated = false;
        int bHated;

        bMercID = gTacticalStatus.Team[gbPlayerNum].bFirstID;
        bLastTeamID = gTacticalStatus.Team[gbPlayerNum].bLastID;

        // loop through all mercs to calculate their morale
        foreach (var pSoldier in MercPtrs)
        //for (pSoldier = MercPtrs[bMercID]; bMercID <= bLastTeamID; bMercID++, pSoldier++)
        {
            //if the merc is active, in Arulco, and conscious, not POW
            if (pSoldier.bActive
                && pSoldier.ubProfile != NO_PROFILE
                && !(pSoldier.bAssignment == Assignments.IN_TRANSIT
                   || pSoldier.fMercAsleep == true
                   || pSoldier.bAssignment == Assignments.ASSIGNMENT_DEAD
                   || pSoldier.bAssignment == Assignments.ASSIGNMENT_POW))
            {
                // calculate the guy's opinion of the people he is with
                pProfile = (gMercProfiles[pSoldier.ubProfile]);

                // if we're moving
                if (pSoldier.ubGroupID != 0 && PlayerIDGroupInMotion(pSoldier.ubGroupID))
                {
                    // we only check our opinions of people in our squad
                    fSameGroupOnly = true;
                }
                else
                {
                    fSameGroupOnly = false;
                }
                fFoundHated = false;

                // reset counts to calculate average opinion
                iTotalOpinions = 0;
                bNumTeamMembers = 0;

                // let people with high leadership affect their own morale
                bHighestTeamLeadership = SkillChecks.EffectiveLeadership(pSoldier);

                // loop through all other mercs
                bOtherID = gTacticalStatus.Team[gbPlayerNum].bFirstID;
                //for (pOtherSoldier = MercPtrs[bOtherID]; bOtherID <= bLastTeamID; bOtherID++, pOtherSoldier++)
                foreach(var pOtherSoldier in MercPtrs)
                {
                    // skip past ourselves and all inactive mercs
                    if (bOtherID != bMercID
                        && pOtherSoldier.bActive
                        && pOtherSoldier.ubProfile != NO_PROFILE
                        && !(pOtherSoldier.bAssignment == Assignments.IN_TRANSIT
                        || pOtherSoldier.fMercAsleep == true
                        || pOtherSoldier.bAssignment == Assignments.ASSIGNMENT_DEAD
                        || pOtherSoldier.bAssignment == Assignments.ASSIGNMENT_POW))
                    {
                        if (fSameGroupOnly)
                        {
                            // all we have to check is the group ID
                            if (pSoldier.ubGroupID != pOtherSoldier.ubGroupID)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            // check to see if the location is the same
                            if (pOtherSoldier.sSectorX != pSoldier.sSectorX ||
                                  pOtherSoldier.sSectorY != pSoldier.sSectorY ||
                                    pOtherSoldier.bSectorZ != pSoldier.bSectorZ)
                            {
                                continue;
                            }

                            // if the OTHER soldier is in motion then we don't do anything!
                            if (pOtherSoldier.ubGroupID != 0 && PlayerIDGroupInMotion(pOtherSoldier.ubGroupID))
                            {
                                continue;
                            }
                        }

                        bOpinion = pProfile.bMercOpinion[pOtherSoldier.ubProfile];
                        if (bOpinion == HATED_OPINION)
                        {

                            bHated = SoldierProfileSubSystem.WhichHated(pSoldier.ubProfile, pOtherSoldier.ubProfile);
                            if (bHated >= 2)
                            {
                                // learn to hate which has become full-blown hatred, full strength
                                fFoundHated = true;
                                break;
                            }
                            else
                            {
                                // scale according to how close to we are to snapping
                                //KM : Divide by 0 error found.  Wrapped into an if statement.
                                if (pProfile.bHatedTime[bHated] > 0)
                                {
                                    bOpinion = ((int)bOpinion) * (pProfile.bHatedTime[bHated] - pProfile.bHatedCount[bHated]) / pProfile.bHatedTime[bHated];
                                }

                                if (pProfile.bHatedCount[bHated] <= pProfile.bHatedTime[bHated] / 2)
                                {
                                    // Augh, we're teamed with someone we hate!  We HATE this!!  Ignore everyone else!		
                                    fFoundHated = true;
                                    break;
                                }
                                // otherwise just mix this opinion in with everyone else... 
                            }
                        }
                        iTotalOpinions += bOpinion;
                        bNumTeamMembers++;
                        if (SkillChecks.EffectiveLeadership(pOtherSoldier) > bHighestTeamLeadership)
                        {
                            bHighestTeamLeadership = SkillChecks.EffectiveLeadership(pOtherSoldier);
                        }
                    }
                }

                if (fFoundHated)
                {
                    // If teamed with someone we hated, team opinion is automatically minimum
                    bActualTeamOpinion = HATED_OPINION;
                }
                else if (bNumTeamMembers > 0)
                {
                    bActualTeamOpinion = (int)(iTotalOpinions / bNumTeamMembers);
                    // give bonus/penalty for highest leadership value on team
                    bActualTeamOpinion += (bHighestTeamLeadership - 50) / 10;
                }
                else // alone
                {
                    bActualTeamOpinion = 0;
                }

                // reduce to a range of HATED through BUDDY
                if (bActualTeamOpinion > BUDDY_OPINION)
                {
                    bActualTeamOpinion = BUDDY_OPINION;
                }
                else if (bActualTeamOpinion < HATED_OPINION)
                {
                    bActualTeamOpinion = HATED_OPINION;
                }

                // shift morale from team by ~10%

                // this should range between -75 and +75
                bTeamMoraleModDiff = bActualTeamOpinion - pSoldier.bTeamMoraleMod;
                if (bTeamMoraleModDiff > 0)
                {
                    bTeamMoraleModChange = 1 + bTeamMoraleModDiff / 10;
                }
                else if (bTeamMoraleModDiff < 0)
                {
                    bTeamMoraleModChange = -1 + bTeamMoraleModDiff / 10;
                }
                else
                {
                    bTeamMoraleModChange = 0;
                }
                pSoldier.bTeamMoraleMod += bTeamMoraleModChange;
                pSoldier.bTeamMoraleMod = Math.Min(pSoldier.bTeamMoraleMod, MORALE_MOD_MAX);
                pSoldier.bTeamMoraleMod = Math.Max(pSoldier.bTeamMoraleMod, -MORALE_MOD_MAX);

                // New, December 3rd, 1998, by CJC --
                // If delayed strategic modifier exists then incorporate it in strategic mod
                if (pSoldier.bDelayedStrategicMoraleMod > 0)
                {
                    pSoldier.bStrategicMoraleMod += pSoldier.bDelayedStrategicMoraleMod;
                    pSoldier.bDelayedStrategicMoraleMod = 0;
                    pSoldier.bStrategicMoraleMod = Math.Min(pSoldier.bStrategicMoraleMod, MORALE_MOD_MAX);
                    pSoldier.bStrategicMoraleMod = Math.Max(pSoldier.bStrategicMoraleMod, -MORALE_MOD_MAX);
                }

                // refresh the morale value for the soldier based on the recalculated team modifier
                RefreshSoldierMorale(pSoldier);
            }
        }

        bStrategicMoraleUpdateCounter++;

        if (bStrategicMoraleUpdateCounter == HOURS_BETWEEN_STRATEGIC_DECAY)
        {
            DecayStrategicMoraleModifiers();
            bStrategicMoraleUpdateCounter = 0;
        }

    }


    void DailyMoraleUpdate(SOLDIERTYPE? pSoldier)
    {
        if (pSoldier.ubProfile == NO_PROFILE)
        {
            return;
        }

        // CJC: made per hour now
        /*
            // decay the merc's strategic morale modifier
            if (pSoldier.bStrategicMoraleMod != 0)
            {
                // decay the modifier!
                DecayStrategicMorale( pSoldier );

                // refresh the morale value for the soldier based on the recalculated modifier
                RefreshSoldierMorale( pSoldier );
            }
        */

        // check death rate vs. merc's tolerance once/day (ignores buddies!)
        if (MercThinksDeathRateTooHigh(pSoldier.ubProfile))
        {
            // too high, morale takes a hit
            HandleMoraleEvent(pSoldier, MoraleEventNames.MORALE_HIGH_DEATHRATE, pSoldier.sSectorX, pSoldier.sSectorY, pSoldier.bSectorZ);
        }

        // check his morale vs. his morale tolerance once/day (ignores buddies!)
        if (MercThinksHisMoraleIsTooLow(pSoldier))
        {
            // too low, morale sinks further (merc's in a funk and things aren't getting better)
            HandleMoraleEvent(pSoldier, MoraleEventNames.MORALE_POOR_MORALE, pSoldier.sSectorX, pSoldier.sSectorY, pSoldier.bSectorZ);
        }
        else if (pSoldier.bMorale >= 75)
        {
            // very high morale, merc is cheerleading others
            HandleMoraleEvent(pSoldier, MoraleEventNames.MORALE_GREAT_MORALE, pSoldier.sSectorX, pSoldier.sSectorY, pSoldier.bSectorZ);
        }

    }
}

public enum MoraleEventNames
{
    MORALE_KILLED_ENEMY = 0,
    MORALE_SQUADMATE_DIED,
    MORALE_SUPPRESSED,
    MORALE_AIRSTRIKE,
    MORALE_DID_LOTS_OF_DAMAGE,
    MORALE_TOOK_LOTS_OF_DAMAGE, // 5
    MORALE_KILLED_CIVILIAN,
    MORALE_BATTLE_WON,
    MORALE_RAN_AWAY,
    MORALE_HEARD_BATTLE_WON,
    MORALE_HEARD_BATTLE_LOST,       // 10
    MORALE_TOWN_LIBERATED,
    MORALE_TOWN_LOST,
    MORALE_MINE_LIBERATED,
    MORALE_MINE_LOST,
    MORALE_SAM_SITE_LIBERATED,  // 15
    MORALE_SAM_SITE_LOST,
    MORALE_BUDDY_DIED,
    MORALE_HATED_DIED,
    MORALE_TEAMMATE_DIED,
    MORALE_LOW_DEATHRATE,               // 20
    MORALE_HIGH_DEATHRATE,
    MORALE_GREAT_MORALE,
    MORALE_POOR_MORALE,
    MORALE_DRUGS_CRASH,
    MORALE_ALCOHOL_CRASH,               // 25
    MORALE_MONSTER_QUEEN_KILLED,
    MORALE_DEIDRANNA_KILLED,
    MORALE_CLAUSTROPHOBE_UNDERGROUND,
    MORALE_INSECT_PHOBIC_SEES_CREATURE,
    MORALE_NERVOUS_ALONE, // 30
    MORALE_MERC_CAPTURED,
    MORALE_MERC_MARRIED,
    MORALE_QUEEN_BATTLE_WON,
    MORALE_SEX,
    NUM_MORALE_EVENTS
}

public enum MoraleEventType
{
    TACTICAL_MORALE_EVENT = 0,
    STRATEGIC_MORALE_EVENT
}


public record MoraleEvent(MoraleEventType ubType, int bChange);
