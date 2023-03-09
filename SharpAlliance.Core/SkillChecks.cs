using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class SkillChecks
{

    public static int EffectiveStrength(SOLDIERTYPE? pSoldier)
    {
        int bBandaged;
        int iEffStrength;

        // Effective strength is:
        // 1/2 full strength
        // plus 1/2 strength scaled according to how hurt we are
        bBandaged = pSoldier.bLifeMax - pSoldier.bLife - pSoldier.bBleeding;
        iEffStrength = pSoldier.bStrength / 2;
        iEffStrength += (pSoldier.bStrength / 2) * (pSoldier.bLife + bBandaged / 2) / (pSoldier.bLifeMax);

        // ATE: Make sure at least 2...
        iEffStrength = Math.Max(iEffStrength, 2);

        return ((int)iEffStrength);
    }


    private static int EffectiveWisdom(SOLDIERTYPE? pSoldier)
    {
        int iEffWisdom;

        iEffWisdom = pSoldier.bWisdom;

        iEffWisdom = DrugsAndAlcohol.EffectStatForBeingDrunk(pSoldier, iEffWisdom);

        return ((int)iEffWisdom);
    }

    private static int EffectiveAgility(SOLDIERTYPE? pSoldier)
    {
        int iEffAgility;

        iEffAgility = pSoldier.bAgility;

        iEffAgility = DrugsAndAlcohol.EffectStatForBeingDrunk(pSoldier, iEffAgility);

        if (pSoldier.sWeightCarriedAtTurnStart > 100)
        {
            iEffAgility = (iEffAgility * 100) / pSoldier.sWeightCarriedAtTurnStart;
        }

        return ((int)iEffAgility);
    }


    private static int EffectiveMechanical(SOLDIERTYPE? pSoldier)
    {
        int iEffMechanical;

        iEffMechanical = pSoldier.bMechanical;

        iEffMechanical = DrugsAndAlcohol.EffectStatForBeingDrunk(pSoldier, iEffMechanical);

        return ((int)iEffMechanical);
    }

    private static int EffectiveExplosive(SOLDIERTYPE? pSoldier)
    {
        int iEffExplosive;

        iEffExplosive = pSoldier.bExplosive;

        iEffExplosive = DrugsAndAlcohol.EffectStatForBeingDrunk(pSoldier, iEffExplosive);

        return ((int)iEffExplosive);
    }


    private static int EffectiveMedical(SOLDIERTYPE? pSoldier)
    {
        int iEffMedical;

        iEffMedical = pSoldier.bMedical;

        iEffMedical = DrugsAndAlcohol.EffectStatForBeingDrunk(pSoldier, iEffMedical);

        return ((int)iEffMedical);
    }

    private static int EffectiveLeadership(SOLDIERTYPE? pSoldier)
    {
        int iEffLeadership;
        int bDrunkLevel;

        iEffLeadership = pSoldier.bLeadership;

        // if we are drunk, effect leader ship in a +ve way...
        bDrunkLevel = GetDrunkLevel(pSoldier);

        if (bDrunkLevel == FEELING_GOOD)
        {
            iEffLeadership = (iEffLeadership * 120 / 100);
        }

        return ((int)iEffLeadership);
    }

    private static int EffectiveExpLevel(SOLDIERTYPE? pSoldier)
    {
        int iEffExpLevel;
        int bDrunkLevel;
        int[] iExpModifier =
        {   0,	// SOBER
		    0,	// Feeling good
		   -1,	// Borderline
		   -2,	// Drunk
		   0,		// Hung
		};

        iEffExpLevel = pSoldier.bExpLevel;

        bDrunkLevel = GetDrunkLevel(pSoldier);

        iEffExpLevel = iEffExpLevel + iExpModifier[bDrunkLevel];

        if (pSoldier.ubProfile != NO_PROFILE)
        {
            if ((gMercProfiles[pSoldier.ubProfile].bPersonalityTrait == PersonalityTrait.CLAUSTROPHOBIC)
                && pSoldier.bActive && pSoldier.bInSector && gbWorldSectorZ > 0)
            {
                // claustrophobic!
                iEffExpLevel--;
            }
        }

        if (iEffExpLevel < 1)
        {
            // can't go below 1
            return (1);
        }
        else
        {
            return ((int)iEffExpLevel);
        }
    }

    private static int EffectiveMarksmanship(SOLDIERTYPE? pSoldier)
    {
        int iEffMarksmanship;

        iEffMarksmanship = pSoldier.bMarksmanship;

        iEffMarksmanship = DrugsAndAlcohol.EffectStatForBeingDrunk(pSoldier, iEffMarksmanship);

        return ((int)iEffMarksmanship);
    }

    private static int EffectiveDexterity(SOLDIERTYPE? pSoldier)
    {
        int iEffDexterity;

        iEffDexterity = pSoldier.bDexterity;

        iEffDexterity = DrugsAndAlcohol.EffectStatForBeingDrunk(pSoldier, iEffDexterity);

        return ((int)iEffDexterity);
    }

    public static int GetPenaltyForFatigue(SOLDIERTYPE? pSoldier)
    {
        int ubPercentPenalty;

        if (pSoldier.bBreathMax >= 85)
        {
            ubPercentPenalty = 0;
        }
        else if (pSoldier.bBreathMax >= 70)
        {
            ubPercentPenalty = 10;
        }
        else if (pSoldier.bBreathMax >= 50)
        {
            ubPercentPenalty = 25;
        }
        else if (pSoldier.bBreathMax >= 30)
        {
            ubPercentPenalty = 50;
        }
        else if (pSoldier.bBreathMax >= 15)
        {
            ubPercentPenalty = 75;
        }
        else if (pSoldier.bBreathMax > 0)
        {
            ubPercentPenalty = 90;
        }
        else
        {
            ubPercentPenalty = 100;
        }

        return (ubPercentPenalty);
    }

    private static void ReducePointsForFatigue(SOLDIERTYPE? pSoldier, ref int pusPoints)
    {
        pusPoints -= (pusPoints * GetPenaltyForFatigue(pSoldier)) / 100;
    }

    private static int GetSkillCheckPenaltyForFatigue(SOLDIERTYPE? pSoldier, int iSkill)
    {
        // use only half the full effect of fatigue for skill checks
        return (((iSkill * GetPenaltyForFatigue(pSoldier)) / 100) / 2);
    }

    public static int SkillCheck(SOLDIERTYPE? pSoldier, SKILLCHECKS bReason, int bChanceMod)
    {
        int iSkill;
        int iChance, iReportChance;
        int iRoll, iMadeItBy;
        InventorySlot bSlot;
        int iLoop;
        SOLDIERTYPE? pTeamSoldier;
        int bBuddyIndex;
        bool fForceDamnSound = false;

        iReportChance = -1;

        switch (bReason)
        {
            case SKILLCHECKS.LOCKPICKING_CHECK:
            case SKILLCHECKS.ELECTRONIC_LOCKPICKING_CHECK:

                fForceDamnSound = true;

                iSkill = EffectiveMechanical(pSoldier);
                if (iSkill == 0)
                {
                    break;
                }
                // adjust skill based on wisdom (knowledge) 
                iSkill = iSkill * (EffectiveWisdom(pSoldier) + 100) / 200;
                // and dexterity (clumsy?)
                iSkill = iSkill * (EffectiveDexterity(pSoldier) + 100) / 200;
                // factor in experience
                iSkill = iSkill + EffectiveExpLevel(pSoldier) * 3;
                if (HAS_SKILL_TRAIT(pSoldier, SkillTrait.LOCKPICKING))
                {
                    // if we specialize in picking locks...
                    iSkill += gbSkillTraitBonus[SkillTrait.LOCKPICKING] * NUM_SKILL_TRAITS(pSoldier, SkillTrait.LOCKPICKING);
                }
                if (bReason == SKILLCHECKS.ELECTRONIC_LOCKPICKING_CHECK && !(HAS_SKILL_TRAIT(pSoldier, SkillTrait.ELECTRONICS)))
                {
                    // if we are unfamiliar with electronics...
                    iSkill /= 2;
                }
                // adjust chance based on status of kit
                bSlot = FindObj(pSoldier, Items.LOCKSMITHKIT);
                if (bSlot == NO_SLOT)
                {
                    // this should never happen, but might as well check...
                    iSkill = 0;
                }
                iSkill = iSkill * pSoldier.inv[bSlot].bStatus[0] / 100;
                break;
            case SKILLCHECKS.ATTACHING_DETONATOR_CHECK:
            case SKILLCHECKS.ATTACHING_REMOTE_DETONATOR_CHECK:
                iSkill = EffectiveExplosive(pSoldier);
                if (iSkill == 0)
                {
                    break;
                }
                iSkill = (iSkill * 3 + EffectiveDexterity(pSoldier)) / 4;
                if (bReason == SKILLCHECKS.ATTACHING_REMOTE_DETONATOR_CHECK && !(HAS_SKILL_TRAIT(pSoldier, SkillTrait.ELECTRONICS)))
                {
                    iSkill /= 2;
                }
                break;
            case SKILLCHECKS.PLANTING_BOMB_CHECK:
            case SKILLCHECKS.PLANTING_REMOTE_BOMB_CHECK:
                iSkill = EffectiveExplosive(pSoldier) * 7;
                iSkill += EffectiveWisdom(pSoldier) * 2;
                iSkill += EffectiveExpLevel(pSoldier) * 10;
                iSkill = iSkill / 10; // bring the value down to a percentage

                if (bReason == SKILLCHECKS.PLANTING_REMOTE_BOMB_CHECK && !(HAS_SKILL_TRAIT(pSoldier, SkillTrait.ELECTRONICS)))
                {
                    // deduct only a bit...
                    iSkill = (iSkill * 3) / 4;
                }

                // Ok, this is really damn easy, so skew the values...
                // e.g. if calculated skill is 84, skewed up to 96
                // 51 to 84
                // 22 stays as is
                iSkill = (iSkill + 100 * (iSkill / 25)) / (iSkill / 25 + 1);
                break;

            case SKILLCHECKS.DISARM_TRAP_CHECK:

                fForceDamnSound = true;

                iSkill = EffectiveExplosive(pSoldier) * 7;
                if (iSkill == 0)
                {
                    break;
                }
                iSkill += EffectiveDexterity(pSoldier) * 2;
                iSkill += EffectiveExpLevel(pSoldier) * 10;
                iSkill = iSkill / 10; // bring the value down to a percentage
                                      // penalty based on poor wisdom
                iSkill -= (100 - EffectiveWisdom(pSoldier)) / 5;
                break;

            case SKILLCHECKS.DISARM_ELECTRONIC_TRAP_CHECK:

                fForceDamnSound = true;

                iSkill = Math.Max(EffectiveMechanical(pSoldier), EffectiveExplosive(pSoldier)) * 7;
                if (iSkill == 0)
                {
                    break;
                }
                iSkill += EffectiveDexterity(pSoldier) * 2;
                iSkill += EffectiveExpLevel(pSoldier) * 10;
                iSkill = iSkill / 10; // bring the value down to a percentage
                                      // penalty based on poor wisdom
                iSkill -= (100 - EffectiveWisdom(pSoldier)) / 5;

                if (!(HAS_SKILL_TRAIT(pSoldier, SkillTrait.ELECTRONICS)))
                {
                    iSkill = (iSkill * 3) / 4;
                }
                break;

            case SKILLCHECKS.OPEN_WITH_CROWBAR:
                // Add for crowbar...
                iSkill = EffectiveStrength(pSoldier) + 20;
                fForceDamnSound = true;
                break;

            case SKILLCHECKS.SMASH_DOOR_CHECK:
                iSkill = EffectiveStrength(pSoldier);
                break;
            case SKILLCHECKS.UNJAM_GUN_CHECK:
                iSkill = 30 + EffectiveMechanical(pSoldier) / 2;
                break;
            case SKILLCHECKS.NOTICE_DART_CHECK:
                // only a max of ~20% chance
                iSkill = EffectiveWisdom(pSoldier) / 10 + EffectiveExpLevel(pSoldier);
                break;
            case SKILLCHECKS.LIE_TO_QUEEN_CHECK:
                // competitive check vs the queen's wisdom and leadership... poor guy!
                iSkill = 50 * (EffectiveWisdom(pSoldier) + EffectiveLeadership(pSoldier)) / (gMercProfiles[QUEEN].bWisdom + gMercProfiles[QUEEN].bLeadership);
                break;
            case SKILLCHECKS.ATTACHING_SPECIAL_ITEM_CHECK:
            case SKILLCHECKS.ATTACHING_SPECIAL_ELECTRONIC_ITEM_CHECK:
                iSkill = EffectiveMechanical(pSoldier);
                if (iSkill == 0)
                {
                    break;
                }
                // adjust skill based on wisdom (knowledge) 
                iSkill = iSkill * (EffectiveWisdom(pSoldier) + 100) / 200;
                // and dexterity (clumsy?)
                iSkill = iSkill * (EffectiveDexterity(pSoldier) + 100) / 200;
                // factor in experience
                iSkill = iSkill + EffectiveExpLevel(pSoldier) * 3;
                if (bReason == SKILLCHECKS.ATTACHING_SPECIAL_ELECTRONIC_ITEM_CHECK && !(HAS_SKILL_TRAIT(pSoldier, SkillTrait.ELECTRONICS)))
                {
                    // if we are unfamiliar with electronics...
                    iSkill /= 2;
                }
                break;
            default:
                iSkill = 0;
                break;
        }

        iSkill -= GetSkillCheckPenaltyForFatigue(pSoldier, iSkill);

        iChance = iSkill + bChanceMod;

        switch (bReason)
        {
            case SKILLCHECKS.LOCKPICKING_CHECK:
            case SKILLCHECKS.ELECTRONIC_LOCKPICKING_CHECK:
            case SKILLCHECKS.DISARM_TRAP_CHECK:
            case SKILLCHECKS.DISARM_ELECTRONIC_TRAP_CHECK:
            case SKILLCHECKS.OPEN_WITH_CROWBAR:
            case SKILLCHECKS.SMASH_DOOR_CHECK:
            case SKILLCHECKS.ATTACHING_SPECIAL_ITEM_CHECK:
            case SKILLCHECKS.ATTACHING_SPECIAL_ELECTRONIC_ITEM_CHECK:
                // for lockpicking and smashing locks, if the chance isn't reasonable
                // we set it to 0 so they can never get through the door if they aren't
                // good enough
                if (iChance < 30)
                {
                    iChance = 0;
                    break;
                }
                break;
            // else fall through
            default:
                iChance += GetMoraleModifier(pSoldier);
                break;
        }

        if (iChance > 99)
        {
            iChance = 99;
        }
        else if (iChance < 0)
        {
            iChance = 0;
        }

        iRoll = PreRandom(100);
        iMadeItBy = iChance - iRoll;
        if (iMadeItBy < 0)
        {
            if ((pSoldier.bLastSkillCheck == bReason) && (pSoldier.sGridNo == pSoldier.sSkillCheckGridNo))
            {
                pSoldier.ubSkillCheckAttempts++;
                if (pSoldier.ubSkillCheckAttempts > 2)
                {
                    if (iChance == 0)
                    {
                        // do we realize that we just can't do this?
                        if ((100 - (pSoldier.ubSkillCheckAttempts - 2) * 20) < EffectiveWisdom(pSoldier))
                        {
                            // say "I can't do this" quote
                            TacticalCharacterDialogue(pSoldier, QUOTE.DEFINITE_CANT_DO);
                            return (iMadeItBy);
                        }
                    }
                }
            }
            else
            {
                pSoldier.bLastSkillCheck = bReason;
                pSoldier.ubSkillCheckAttempts = 1;
                pSoldier.sSkillCheckGridNo = pSoldier.sGridNo;
            }

            if (fForceDamnSound || Globals.Random.Next(100) < 40)
            {
                switch (bReason)
                {
                    case SKILLCHECKS.UNJAM_GUN_CHECK:
                    case SKILLCHECKS.NOTICE_DART_CHECK:
                    case SKILLCHECKS.LIE_TO_QUEEN_CHECK:
                        // silent check
                        break;
                    default:
                        DoMercBattleSound(pSoldier, BATTLE_SOUND_CURSE1);
                        break;
                }
            }

        }
        else
        {
            // A buddy might make a positive comment based on our success;
            // Increase the chance for people with higher skill and for more difficult tasks
            iChance = 15 + iSkill / 20 + (-bChanceMod) / 20;
            if (iRoll < iChance)
            {
                // If a buddy of this merc is standing around nearby, they'll make a positive comment.
                iLoop = gTacticalStatus.Team[gbPlayerNum].bFirstID;
                for (pTeamSoldier = MercPtrs[iLoop]; iLoop <= gTacticalStatus.Team[gbPlayerNum].bLastID; iLoop++, pTeamSoldier++)
                {
                    if (OK_INSECTOR_MERC(pTeamSoldier))
                    {
                        bBuddyIndex = WhichBuddy(pTeamSoldier.ubProfile, pSoldier.ubProfile);
                        if (bBuddyIndex >= 0 && SpacesAway(pSoldier.sGridNo, pTeamSoldier.sGridNo) < 15)
                        {
                            switch (bBuddyIndex)
                            {
                                case 0:
                                    // buddy #1 did something good!
                                    TacticalCharacterDialogue(pTeamSoldier, QUOTE.BUDDY_1_GOOD);
                                    break;
                                case 1:
                                    // buddy #2 did something good!
                                    TacticalCharacterDialogue(pTeamSoldier, QUOTE.BUDDY_2_GOOD);
                                    break;
                                case 2:
                                    // learn to like buddy did something good!
                                    TacticalCharacterDialogue(pTeamSoldier, QUOTE.LEARNED_TO_LIKE_WITNESSED);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
        }
        return (iMadeItBy);
    }


    int CalcTrapDetectLevel(SOLDIERTYPE? pSoldier, bool fExamining)
    {
        // return the level of trap which the guy is able to detect

        int bDetectLevel;

        // formula: 1 pt for every exp_level
        //     plus 1 pt for every 40 explosives
        //     less 1 pt for every 20 wisdom MISSING

        bDetectLevel = EffectiveExpLevel(pSoldier);
        bDetectLevel += (EffectiveExplosive(pSoldier) / 40);
        bDetectLevel -= ((100 - EffectiveWisdom(pSoldier)) / 20);

        // if the examining flag is true, this isn't just a casual glance
        // and the merc should have a higher chance
        if (fExamining)
        {
            bDetectLevel += (int)PreRandom(bDetectLevel / 3 + 2);
        }

        // if substantially bleeding, or still in serious shock, randomly lower value
        if ((pSoldier.bBleeding > 20) || (pSoldier.bShock > 1))
        {
            bDetectLevel -= (int)PreRandom(3);
        }

        if (bDetectLevel < 1)
        {
            bDetectLevel = 1;
        }

        return (bDetectLevel);
    }
}

public enum SKILLCHECKS
{
    NO_CHECK = 0,
    LOCKPICKING_CHECK,
    ELECTRONIC_LOCKPICKING_CHECK,
    ATTACHING_DETONATOR_CHECK,
    ATTACHING_REMOTE_DETONATOR_CHECK,
    PLANTING_BOMB_CHECK,
    PLANTING_REMOTE_BOMB_CHECK,
    OPEN_WITH_CROWBAR,
    SMASH_DOOR_CHECK,
    DISARM_TRAP_CHECK,
    UNJAM_GUN_CHECK,
    NOTICE_DART_CHECK,
    LIE_TO_QUEEN_CHECK,
    ATTACHING_SPECIAL_ITEM_CHECK,
    ATTACHING_SPECIAL_ELECTRONIC_ITEM_CHECK,
    DISARM_ELECTRONIC_TRAP_CHECK,
}
