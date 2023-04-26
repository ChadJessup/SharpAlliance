using System;
using System.Diagnostics;
using SharpAlliance.Core.Managers;

using static SharpAlliance.Core.EnglishText;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class Campaign
{
    // give pSoldier usNumChances to improve ubStat.  If it's from training, it doesn't count towards experience level gain
    public static void StatChange(SOLDIERTYPE? pSoldier, Stat ubStat, int usNumChances, int ubReason)
    {
        Debug.Assert(pSoldier != null);
        Debug.Assert(pSoldier.bActive);

        // ignore non-player soldiers
        if (!PTR_OURTEAM(pSoldier))
        {
            return;
        }

        // ignore anything without a profile
        if (pSoldier.ubProfile == NPCID.NO_PROFILE)
        {
            return;
        }

        // ignore vehicles and robots
        if ((pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE))
            || (pSoldier.uiStatusFlags.HasFlag(SOLDIER.ROBOT)))
        {
            return;
        }

        if (pSoldier.bAssignment == Assignments.ASSIGNMENT_POW)
        {
            Messages.ScreenMsg(FontColor.FONT_ORANGE, MSG.BETAVERSION, "ERROR: StatChange: %s improving stats while POW! ubStat %d", pSoldier.name, ubStat.ToString());
            return;
        }

        // no points earned while somebody is unconscious (for assist XPs, and such)
        if (pSoldier.bLife < Globals.CONSCIOUSNESS)
        {
            return;
        }


        //# if TESTVERSION
        //        if (gTacticalStatus.fStatChangeCheatOn)
        //        {
        //            usNumChances = 100;
        //        }
        //#endif

        Campaign.ProcessStatChange((Globals.gMercProfiles[pSoldier.ubProfile]), ubStat, usNumChances, ubReason);

        // Update stats....right away... ATE
        UpdateStats(pSoldier);
    }


    // this is the equivalent of StatChange(), but for use with mercs not currently on player's team
    // give pProfile usNumChances to improve ubStat.  If it's from training, it doesn't count towards experience level gain
    void ProfileStatChange(MERCPROFILESTRUCT? pProfile, Stat ubStat, int usNumChances, int ubReason)
    {
        // dead guys don't do nuthin' !
        if (pProfile.bMercStatus == MercStatus.MERC_IS_DEAD)
        {
            return;
        }

        if (pProfile.bLife < Globals.OKLIFE)
        {
            return;
        }

        ProcessStatChange(pProfile, ubStat, usNumChances, ubReason);

        // Update stats....right away... ATE
        ProfileUpdateStats(pProfile);
    }


    public static void ProcessStatChange(MERCPROFILESTRUCT? pProfile, Stat ubStat, int usNumChances, int ubReason)
    {
        int uiCnt, uiEffLevel;
        int sSubPointChange = 0;
        int usChance = 0;
        int usSubpointsPerPoint;
        int usSubpointsPerLevel;
        int bCurrentRating;
        int psStatGainPtr;
        bool fAffectedByWisdom = true;

        Debug.Assert(pProfile != null);

        if (pProfile.bEvolution == CharacterEvolution.NO_EVOLUTION)
        {
            return;     // No change possible, quit right away
        }

        // if this is a Reverse-Evolving merc who attempting to train
        if ((ubReason == Globals.FROM_TRAINING) && (pProfile.bEvolution == CharacterEvolution.DEVOLVE))
        {
            return; // he doesn't get any benefit, but isn't penalized either
        }

        if (usNumChances == 0)
        {
            return;
        }

        usSubpointsPerPoint = SubpointsPerPoint(ubStat, pProfile.bExpLevel);
        usSubpointsPerLevel = SubpointsPerPoint(Stat.EXPERAMT, pProfile.bExpLevel);

        switch (ubStat)
        {
            case Stat.HEALTHAMT:
                bCurrentRating = pProfile.bLifeMax;
                psStatGainPtr = (pProfile.sLifeGain);
                // NB physical stat checks not affected by wisdom, unless training is going on
                fAffectedByWisdom = false;
                break;

            case Stat.AGILAMT:
                bCurrentRating = pProfile.bAgility;
                psStatGainPtr = (pProfile.sAgilityGain);
                fAffectedByWisdom = false;
                break;

            case Stat.DEXTAMT:
                bCurrentRating = pProfile.bDexterity;
                psStatGainPtr = (pProfile.sDexterityGain);
                fAffectedByWisdom = false;
                break;

            case Stat.WISDOMAMT:
                bCurrentRating = pProfile.bWisdom;
                psStatGainPtr = (pProfile.sWisdomGain);
                break;

            case Stat.MEDICALAMT:
                bCurrentRating = pProfile.bMedical;
                psStatGainPtr = (pProfile.sMedicalGain);
                break;

            case Stat.EXPLODEAMT:
                bCurrentRating = pProfile.bExplosive;
                psStatGainPtr = (pProfile.sExplosivesGain);
                break;

            case Stat.MECHANAMT:
                bCurrentRating = pProfile.bMechanical;
                psStatGainPtr = (pProfile.sMechanicGain);
                break;

            case Stat.MARKAMT:
                bCurrentRating = pProfile.bMarksmanship;
                psStatGainPtr = (pProfile.sMarksmanshipGain);
                break;

            case Stat.EXPERAMT:
                bCurrentRating = pProfile.bExpLevel;
                psStatGainPtr = (pProfile.sExpLevelGain);
                break;

            case Stat.STRAMT:
                bCurrentRating = (int)pProfile.bStrength;
                psStatGainPtr = (pProfile.sStrengthGain);
                fAffectedByWisdom = false;
                break;

            case Stat.LDRAMT:
                bCurrentRating = pProfile.bLeadership;
                psStatGainPtr = (pProfile.sLeadershipGain);
                break;

            default:
                // BETA message
                Messages.ScreenMsg(FontColor.FONT_ORANGE, MSG.BETAVERSION, "ERROR: ProcessStatChange: Rcvd unknown ubStat %d", ubStat.ToString());
                return;
        }


        if (ubReason == Globals.FROM_TRAINING)
        {
            // training always affected by wisdom
            fAffectedByWisdom = true;
        }


        // stats/skills of 0 can NEVER be improved!
        if (bCurrentRating <= 0)
        {
            return;
        }


        // loop once for each chance to improve
        for (uiCnt = 0; uiCnt < usNumChances; uiCnt++)
        {
            if (pProfile.bEvolution == CharacterEvolution.NORMAL_EVOLUTION)               // Evolves!
            {
                // if this is improving from a failure, and a successful roll would give us enough to go up a point
                if ((ubReason == Globals.FROM_FAILURE) && ((psStatGainPtr + 1) >= usSubpointsPerPoint))
                {
                    // can't improve any more from this statchange, because Ian don't want failures causin increases!
                    break;
                }

                if (ubStat != Stat.EXPERAMT)
                {
                    // NON-experience level changes, actual usChance depends on bCurrentRating
                    // Base usChance is '100 - bCurrentRating'
                    usChance = 100 - (bCurrentRating + (psStatGainPtr / usSubpointsPerPoint));

                    // prevent training beyond the training cap
                    if ((ubReason == Globals.FROM_TRAINING) && (bCurrentRating + (psStatGainPtr / usSubpointsPerPoint) >= Globals.TRAINING_RATING_CAP))
                    {
                        usChance = 0;
                    }
                }
                else
                {
                    // Experience level changes, actual usChance depends on level
                    // Base usChance is '100 - (10 * current level)'
                    usChance = 100 - 10 * (bCurrentRating + (psStatGainPtr / usSubpointsPerPoint));
                }

                // if there IS a usChance, adjust it for high or low wisdom (50 is avg)
                if (usChance > 0 && fAffectedByWisdom)
                {
                    usChance += (usChance * (pProfile.bWisdom + (pProfile.sWisdomGain / SubpointsPerPoint(Stat.WISDOMAMT, pProfile.bExpLevel)) - 50)) / 100;
                }

                /*
                      // if the stat is Marksmanship, and the guy is a hopeless shot
                      if ((ubStat == MARKAMT) && (pProfile.bSpecialTrait == HOPELESS_SHOT))
                            {
                                usChance /= 5;		// MUCH slower to improve, divide usChance by 5
                            }
                */

                // maximum possible usChance is 99%
                if (usChance > 99)
                {
                    usChance = 99;
                }

                if (PreRandom(100) < usChance)
                {
                    (psStatGainPtr)++;
                    sSubPointChange++;

                    // as long as we're not dealing with exp_level changes (already added above!)
                    // and it's not from training, and the exp level isn't max'ed out already
                    if ((ubStat != Stat.EXPERAMT) && (ubReason != Globals.FROM_TRAINING))
                    {
                        uiEffLevel = pProfile.bExpLevel + (pProfile.sExpLevelGain / usSubpointsPerLevel);

                        // if level is not at maximum
                        if (uiEffLevel < Globals.MAXEXPLEVEL)
                        {
                            // if this is NOT improving from a failure, OR it would NOT give us enough to go up a level
                            if ((ubReason != Globals.FROM_FAILURE) || ((pProfile.sExpLevelGain + 1) < usSubpointsPerLevel))
                            {
                                // all other stat changes count towards experience level changes (1 for 1 basis)
                                pProfile.sExpLevelGain++;
                            }
                        }
                    }
                }
            }
            else                          // Regresses!
            {
                // regression can happen from both failures and successes (but not training, checked above)

                if (ubStat != Stat.EXPERAMT)
                {
                    // NON-experience level changes, actual usChance depends on bCurrentRating
                    switch (ubStat)
                    {
                        case Stat.HEALTHAMT:
                        case Stat.AGILAMT:
                        case Stat.DEXTAMT:
                        case Stat.WISDOMAMT:
                        case Stat.STRAMT:
                            // Base usChance is 'bCurrentRating - 1', since these must remain at 1-100
                            usChance = bCurrentRating + (psStatGainPtr / usSubpointsPerPoint) - 1;
                            break;

                        case Stat.MEDICALAMT:
                        case Stat.EXPLODEAMT:
                        case Stat.MECHANAMT:
                        case Stat.MARKAMT:
                        case Stat.LDRAMT:
                            // Base usChance is 'bCurrentRating', these can drop to 0
                            usChance = bCurrentRating + (psStatGainPtr / usSubpointsPerPoint);
                            break;
                    }
                }
                else
                {
                    // Experience level changes, actual usChance depends on level
                    // Base usChance is '10 * (current level - 1)'
                    usChance = 10 * (bCurrentRating + (psStatGainPtr / usSubpointsPerPoint) - 1);

                    // if there IS a usChance, adjust it for high or low wisdom (50 is avg)
                    if (usChance > 0 && fAffectedByWisdom)
                    {
                        usChance -= (usChance * (pProfile.bWisdom + (pProfile.sWisdomGain / SubpointsPerPoint(Stat.WISDOMAMT, pProfile.bExpLevel)) - 50)) / 100;
                    }

                    // if there's ANY usChance, minimum usChance is 1% regardless of wisdom
                    if (usChance < 1)
                    {
                        usChance = 1;
                    }
                }

                if (PreRandom(100) < usChance)
                {
                    (psStatGainPtr)--;
                    sSubPointChange--;

                    // as long as we're not dealing with exp_level changes (already added above!)
                    // and it's not from training, and the exp level isn't max'ed out already
                    if ((ubStat != Stat.EXPERAMT) && (ubReason != Globals.FROM_TRAINING))
                    {
                        uiEffLevel = pProfile.bExpLevel + (pProfile.sExpLevelGain / usSubpointsPerLevel);

                        // if level is not at minimum
                        if (uiEffLevel > 1)
                        {
                            // all other stat changes count towards experience level changes (1 for 1 basis)
                            pProfile.sExpLevelGain--;
                        }
                    }
                }
            }
        }

//# if STAT_CHANGE_DEBUG
//        if (sSubPointChange != 0)
//        {
//            // debug message
//            Messages.ScreenMsg(MSG_FONT_RED, MSG.DEBUG, "%s's %s changed by %d", pProfile.zNickname, wDebugStatStrings[ubStat], sSubPointChange);
//        }
//#endif


        // exclude training, that's not under our control
        if (ubReason != Globals.FROM_TRAINING)
        {
            // increment counters that track how often stat changes are being awarded
            pProfile.usStatChangeChances[ubStat] += usNumChances;
            pProfile.usStatChangeSuccesses[ubStat] += Math.Abs(sSubPointChange);
        }
    }


    // convert hired mercs' stats subpoint changes into actual point changes where warranted
    private static void UpdateStats(SOLDIERTYPE? pSoldier)
    {
        ProcessUpdateStats((Globals.gMercProfiles[pSoldier.ubProfile]), pSoldier);
    }


    // UpdateStats version for mercs not currently on player's team
    private static void ProfileUpdateStats(MERCPROFILESTRUCT? pProfile)
    {
        ProcessUpdateStats(pProfile, null);
    }

    public static void ChangeStat(MERCPROFILESTRUCT? pProfile, SOLDIERTYPE? pSoldier, Stat ubStat, int sPtsChanged)
    {
        // this function changes the stat a given amount...
        int? psStatGainPtr = null;
        int? pbStatPtr = null;
        int? pbSoldierStatPtr = null;
        int? pbStatDeltaPtr = null;
        uint? puiStatTimerPtr = null;
        bool fChangeTypeIncrease;
        bool fChangeSalary;
        int uiLevelCnt;
        NPCID ubMercMercIdValue = 0;
        int usIncreaseValue = 0;
        int usSubpointsPerPoint;

        usSubpointsPerPoint = SubpointsPerPoint(ubStat, pProfile.bExpLevel);

        // build ptrs to appropriate profiletype stat fields
        switch (ubStat)
        {
            case Stat.HEALTHAMT:
                psStatGainPtr = (pProfile.sLifeGain);
                pbStatDeltaPtr = (pProfile.bLifeDelta);
                pbStatPtr = (pProfile.bLifeMax);
                break;

            case Stat.AGILAMT:
                psStatGainPtr = (pProfile.sAgilityGain);
                pbStatDeltaPtr = (pProfile.bAgilityDelta);
                pbStatPtr = (pProfile.bAgility);
                break;

            case Stat.DEXTAMT:
                psStatGainPtr = (pProfile.sDexterityGain);
                pbStatDeltaPtr = (pProfile.bDexterityDelta);
                pbStatPtr = (pProfile.bDexterity);
                break;

            case Stat.WISDOMAMT:
                psStatGainPtr = (pProfile.sWisdomGain);
                pbStatDeltaPtr = (pProfile.bWisdomDelta);
                pbStatPtr = (pProfile.bWisdom);
                break;

            case Stat.MEDICALAMT:
                psStatGainPtr = (pProfile.sMedicalGain);
                pbStatDeltaPtr = (pProfile.bMedicalDelta);
                pbStatPtr = (pProfile.bMedical);
                break;

            case Stat.EXPLODEAMT:
                psStatGainPtr = (pProfile.sExplosivesGain);
                pbStatDeltaPtr = (pProfile.bExplosivesDelta);
                pbStatPtr = (pProfile.bExplosive);
                break;

            case Stat.MECHANAMT:
                psStatGainPtr = (pProfile.sMechanicGain);
                pbStatDeltaPtr = (pProfile.bMechanicDelta);
                pbStatPtr = (pProfile.bMechanical);
                break;

            case Stat.MARKAMT:
                psStatGainPtr = (pProfile.sMarksmanshipGain);
                pbStatDeltaPtr = (pProfile.bMarksmanshipDelta);
                pbStatPtr = (pProfile.bMarksmanship);
                break;

            case Stat.EXPERAMT:
                psStatGainPtr = (pProfile.sExpLevelGain);
                pbStatDeltaPtr = (pProfile.bExpLevelDelta);
                pbStatPtr = (pProfile.bExpLevel);
                break;

            case Stat.STRAMT:
                psStatGainPtr = (pProfile.sStrengthGain);
                pbStatDeltaPtr = (pProfile.bStrengthDelta);
                pbStatPtr = (int?)(pProfile.bStrength);
                break;

            case Stat.LDRAMT:
                psStatGainPtr = (pProfile.sLeadershipGain);
                pbStatDeltaPtr = (pProfile.bLeadershipDelta);
                pbStatPtr = (pProfile.bLeadership);
                break;
        }


        // if this merc is currently on the player's team
        if (pSoldier != null)
        {
            // build ptrs to appropriate soldiertype stat fields
            switch (ubStat)
            {
                case Stat.HEALTHAMT:
                    pbSoldierStatPtr = (int?)(pSoldier.bLifeMax);
                    puiStatTimerPtr = (pSoldier.uiChangeHealthTime);
                    usIncreaseValue = Globals.HEALTH_INCREASE;
                    break;

                case Stat.AGILAMT:
                    pbSoldierStatPtr = (pSoldier.bAgility);
                    puiStatTimerPtr = (pSoldier.uiChangeAgilityTime);
                    usIncreaseValue = Globals.AGIL_INCREASE;
                    break;

                case Stat.DEXTAMT:
                    pbSoldierStatPtr = (pSoldier.bDexterity);
                    puiStatTimerPtr = (pSoldier.uiChangeDexterityTime);
                    usIncreaseValue = Globals.DEX_INCREASE;
                    break;

                case Stat.WISDOMAMT:
                    pbSoldierStatPtr = (pSoldier.bWisdom);
                    puiStatTimerPtr = (pSoldier.uiChangeWisdomTime);
                    usIncreaseValue = Globals.WIS_INCREASE;
                    break;

                case Stat.MEDICALAMT:
                    pbSoldierStatPtr = (pSoldier.bMedical);
                    puiStatTimerPtr = (pSoldier.uiChangeMedicalTime);
                    usIncreaseValue = Globals.MED_INCREASE;
                    break;

                case Stat.EXPLODEAMT:
                    pbSoldierStatPtr = (pSoldier.bExplosive);
                    puiStatTimerPtr = (pSoldier.uiChangeExplosivesTime);
                    usIncreaseValue = Globals.EXP_INCREASE;
                    break;

                case Stat.MECHANAMT:
                    pbSoldierStatPtr = (pSoldier.bMechanical);
                    puiStatTimerPtr = (pSoldier.uiChangeMechanicalTime);
                    usIncreaseValue = Globals.MECH_INCREASE;
                    break;

                case Stat.MARKAMT:
                    pbSoldierStatPtr = (pSoldier.bMarksmanship);
                    puiStatTimerPtr = (pSoldier.uiChangeMarksmanshipTime);
                    usIncreaseValue = Globals.MRK_INCREASE;
                    break;

                case Stat.EXPERAMT:
                    pbSoldierStatPtr = (pSoldier.bExpLevel);
                    puiStatTimerPtr = (pSoldier.uiChangeLevelTime);
                    usIncreaseValue = Globals.LVL_INCREASE;
                    break;

                case Stat.STRAMT:
                    pbSoldierStatPtr = ((int?)pSoldier.bStrength);
                    puiStatTimerPtr = (pSoldier.uiChangeStrengthTime);
                    usIncreaseValue = Globals.STRENGTH_INCREASE;
                    break;

                case Stat.LDRAMT:
                    pbSoldierStatPtr = (pSoldier.bLeadership);
                    puiStatTimerPtr = (pSoldier.uiChangeLeadershipTime);
                    usIncreaseValue = Globals.LDR_INCREASE;
                    break;
            }
        }

        // ptrs set up, now handle
        // if the stat needs to change
        if (sPtsChanged != 0)
        {
            // if a stat improved
            if (sPtsChanged > 0)
            {
                fChangeTypeIncrease = true;
            }
            else
            {
                fChangeTypeIncrease = false;
            }

            // update merc profile stat
            pbStatPtr += sPtsChanged;

            // if this merc is currently on the player's team (DON'T count increases earned outside the player's employ)
            if (pSoldier != null)
            {
                // also note the delta (how much this stat has changed since start of game)
                pbStatDeltaPtr += sPtsChanged;
            }

            // reduce gain to the unused subpts only
            psStatGainPtr = (psStatGainPtr) % usSubpointsPerPoint;


            // if the guy is employed by player
            if (pSoldier != null)
            {
                // transfer over change to soldiertype structure
                pbSoldierStatPtr = pbStatPtr;

                // if it's a level gain, or sometimes for other stats
                // ( except health; not only will it sound silly, but
                // also we give points for health on sector traversal and this would
                // probaby mess up battle handling too )
                if ((ubStat != Stat.HEALTHAMT) && ((ubStat == Stat.EXPERAMT) || Globals.Random.Next(100) < 25))
                //if ( (ubStat != EXPERAMT) && (ubStat != HEALTHAMT) && ( Globals.Random.Next( 100 ) < 25 ) )
                {
                    // Pipe up with "I'm getting better at this!"
//                    TacticalCharacterDialogueWithSpecialEventEx(pSoldier, 0, DIALOGUE_SPECIAL_EVENT.DISPLAY_STAT_CHANGE, fChangeTypeIncrease, sPtsChanged, ubStat);
                    DialogControl.TacticalCharacterDialogue(pSoldier, QUOTE.EXPERIENCE_GAIN);
                }
                else
                {
                    string wTempString;

                    // tell player about it
                    BuildStatChangeString(out wTempString, pSoldier.name, fChangeTypeIncrease, sPtsChanged, ubStat);
                    Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.INTERFACE, wTempString);
                }

                // update mapscreen soldier info panel
                fCharacterInfoPanelDirty = true;

                // remember what time it changed at, it's displayed in a different color for a while afterwards
                puiStatTimerPtr = Globals.GetJA2Clock();

                if (fChangeTypeIncrease)
                {
                    pSoldier.usValueGoneUp |= usIncreaseValue;
                }
                else
                {
                    pSoldier.usValueGoneUp &= ~(usIncreaseValue);
                }

                fInterfacePanelDirty = DIRTYLEVEL2;
            }


            // special handling for LIFEMAX
            if (ubStat == Stat.HEALTHAMT)
            {
                // adjust current health by the same amount as max health
                pProfile.bLife += sPtsChanged;

                // don't let this kill a guy or knock him out!!!
                if (pProfile.bLife < Globals.OKLIFE)
                {
                    pProfile.bLife = Globals.OKLIFE;
                }

                // if the guy is employed by player
                if (pSoldier != null)
                {
                    // adjust current health by the same amount as max health
                    pSoldier.bLife += sPtsChanged;

                    // don't let this kill a guy or knock him out!!!
                    if (pSoldier.bLife < Globals.OKLIFE)
                    {
                        pSoldier.bLife = Globals.OKLIFE;
                    }
                }
            }

            // special handling for EXPERIENCE LEVEL
            // merc salaries increase if level goes up (but they don't go down if level drops!)
            if ((ubStat == Stat.EXPERAMT) && fChangeTypeIncrease)
            {
                // if the guy is employed by player
                if (pSoldier != null)
                {
                    switch (pSoldier.ubWhatKindOfMercAmI)
                    {
                        case MERC_TYPE.AIM_MERC:
                            // A.I.M.
                            pSoldier.fContractPriceHasIncreased = true;
                            fChangeSalary = true;
                            break;

                        case MERC_TYPE.MERC:
                            // M.E.R.C.
                            ubMercMercIdValue = pSoldier.ubProfile;

                            // Biff's profile id ( 40 ) is the base
                            ubMercMercIdValue -= NPCID.BIFF;

                            // offset for the 2 profiles of Larry (we only have one email for Larry..but 2 profile entries
                            if (ubMercMercIdValue >= (NPCID)(NPCID.LARRY_DRUNK - NPCID.BIFF))
                            {
                                ubMercMercIdValue--;
                            }

                            //
                            // Send special E-mail
                            //

                            //	DEF: 03/06/99 Now sets an event that will be processed later in the day
                            //						ubEmailOffset = MERC_UP_LEVEL_BIFF + MERC_UP_LEVEL_LENGTH_BIFF * ( ubMercMercIdValue ); 
                            //						AddEmail( ubEmailOffset, MERC_UP_LEVEL_LENGTH_BIFF, SPECK_FROM_MERC, GetWorldTotalMin() );
                            GameEvents.AddStrategicEvent(EVENT.MERC_MERC_WENT_UP_LEVEL_EMAIL_DELAY, (uint)(GameClock.GetWorldTotalMin() + 60 + Globals.Random.Next(60)), ubMercMercIdValue);

                            fChangeSalary = true;
                            break;

                        default:
                            // others don't increase salary
                            fChangeSalary = false;
                            break;
                    }
                }
                else    // not employed by player
                {
                    // only AIM and M.E.R.C.s update stats when not on player's team, and both of them DO change salary
                    fChangeSalary = true;
                }

                if (fChangeSalary)
                {
                    // increase all salaries and medical deposits, once for each level gained
                    for (uiLevelCnt = 0; uiLevelCnt < sPtsChanged; uiLevelCnt++)
                    {
                        pProfile.sSalary = CalcNewSalary(pProfile.sSalary, fChangeTypeIncrease, Globals.MAX_DAILY_SALARY);
                        pProfile.uiWeeklySalary = CalcNewSalary(pProfile.uiWeeklySalary, fChangeTypeIncrease, Globals.MAX_LARGE_SALARY);
                        pProfile.uiBiWeeklySalary = CalcNewSalary(pProfile.uiBiWeeklySalary, fChangeTypeIncrease, Globals.MAX_LARGE_SALARY);
                        pProfile.sTrueSalary = CalcNewSalary(pProfile.sTrueSalary, fChangeTypeIncrease, Globals.MAX_DAILY_SALARY);
                        pProfile.sMedicalDepositAmount = CalcNewSalary(pProfile.sMedicalDepositAmount, fChangeTypeIncrease, Globals.MAX_DAILY_SALARY);

                        //if (pSoldier != null)
                        // DON'T increase the *effective* medical deposit, it's already been paid out
                        // pSoldier.usMedicalDeposit = pProfile.sMedicalDepositAmount;
                    }
                }
            }
        }

        return;
    }



    // pSoldier may be null!
    private static void ProcessUpdateStats(MERCPROFILESTRUCT? pProfile, SOLDIERTYPE? pSoldier)
    {
        // this function will run through the soldier's profile and update their stats based on any accumulated gain pts.
        Stat ubStat = 0;
        int psStatGainPtr = 0;
        int pbStatPtr = 0;
        int pbSoldierStatPtr;
        int pbStatDeltaPtr;
        int bMinStatValue;
        int bMaxStatValue;
        int usSubpointsPerPoint;
        int sPtsChanged;


        // if hired, not back at AIM
        if (pSoldier != null)
        {
            // ATE: if in the midst of an attack, if in the field, delay all stat changes until the check made after the 'attack'...
            if ((Globals.gTacticalStatus.ubAttackBusyCount > 0) && pSoldier.bInSector && (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
            {
                return;
            }

            // ignore non-player soldiers
            if (!PTR_OURTEAM(pSoldier))
            {
                return;
            }

            // ignore anything without a profile
            if (pSoldier.ubProfile == NPCID.NO_PROFILE)
            {
                return;
            }

            // ignore vehicles and robots
            if ((pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE)) || (pSoldier.uiStatusFlags.HasFlag(SOLDIER.ROBOT)))
            {
                return;
            }

            // delay increases while merc is dying
            if (pSoldier.bLife < Globals.OKLIFE)
            {
                return;
            }

            // ignore POWs - shouldn't ever be getting this far
            if (pSoldier.bAssignment == Assignments.ASSIGNMENT_POW)
            {
                return;
            }
        }
        else
        {
            // dead guys don't do nuthin' !
            if (pProfile.bMercStatus == MercStatus.MERC_IS_DEAD)
            {
                return;
            }

            if (pProfile.bLife < Globals.OKLIFE)
            {
                return;
            }
        }


        // check every attribute, skill, and exp.level, too
        for (ubStat = Stat.FIRST_CHANGEABLE_STAT; ubStat <= Stat.LAST_CHANGEABLE_STAT; ubStat++)
        {
            // set default min & max, subpoints/pt.
            bMinStatValue = 1;
            bMaxStatValue = Globals.MAX_STAT_VALUE;
            usSubpointsPerPoint = SubpointsPerPoint(ubStat, pProfile.bExpLevel);

            // build ptrs to appropriate profiletype stat fields
            switch (ubStat)
            {
                case Stat.HEALTHAMT:
                    psStatGainPtr = (pProfile.sLifeGain);
                    pbStatPtr = (pProfile.bLifeMax);

                    bMinStatValue = Globals.OKLIFE;
                    break;

                case Stat.AGILAMT:
                    psStatGainPtr = (pProfile.sAgilityGain);
                    pbStatPtr = (pProfile.bAgility);
                    break;

                case Stat.DEXTAMT:
                    psStatGainPtr = (pProfile.sDexterityGain);
                    pbStatPtr = (pProfile.bDexterity);
                    break;

                case Stat.WISDOMAMT:
                    psStatGainPtr = (pProfile.sWisdomGain);
                    pbStatPtr = (pProfile.bWisdom);
                    break;

                case Stat.MEDICALAMT:
                    psStatGainPtr = (pProfile.sMedicalGain);
                    pbStatPtr = (pProfile.bMedical);

                    bMinStatValue = 0;
                    break;

                case Stat.EXPLODEAMT:
                    psStatGainPtr = (pProfile.sExplosivesGain);
                    pbStatPtr = (pProfile.bExplosive);

                    bMinStatValue = 0;
                    break;

                case Stat.MECHANAMT:
                    psStatGainPtr = (pProfile.sMechanicGain);
                    pbStatPtr = (pProfile.bMechanical);

                    bMinStatValue = 0;
                    break;

                case Stat.MARKAMT:
                    psStatGainPtr = (pProfile.sMarksmanshipGain);
                    pbStatPtr = (pProfile.bMarksmanship);

                    bMinStatValue = 0;
                    break;

                case Stat.EXPERAMT:
                    psStatGainPtr = (pProfile.sExpLevelGain);
                    pbStatPtr = (pProfile.bExpLevel);

                    bMaxStatValue = Globals.MAXEXPLEVEL;
                    break;

                case Stat.STRAMT:
                    psStatGainPtr = (pProfile.sStrengthGain);
                    pbStatPtr = ((int)pProfile.bStrength);
                    break;

                case Stat.LDRAMT:
                    psStatGainPtr = (pProfile.sLeadershipGain);
                    pbStatPtr = (pProfile.bLeadership);
                    break;
            }


            // if this merc is currently on the player's team
            if (pSoldier != null)
            {
                // build ptrs to appropriate soldiertype stat fields
                switch (ubStat)
                {
                    case Stat.HEALTHAMT:
                        pbSoldierStatPtr = ((int)pSoldier.bLifeMax);
                        break;

                    case Stat.AGILAMT:
                        pbSoldierStatPtr = (pSoldier.bAgility);
                        break;

                    case Stat.DEXTAMT:
                        pbSoldierStatPtr = (pSoldier.bDexterity);
                        break;

                    case Stat.WISDOMAMT:
                        pbSoldierStatPtr = (pSoldier.bWisdom);
                        break;

                    case Stat.MEDICALAMT:
                        pbSoldierStatPtr = (pSoldier.bMedical);
                        break;

                    case Stat.EXPLODEAMT:
                        pbSoldierStatPtr = (pSoldier.bExplosive);
                        break;

                    case Stat.MECHANAMT:
                        pbSoldierStatPtr = (pSoldier.bMechanical);
                        break;

                    case Stat.MARKAMT:
                        pbSoldierStatPtr = (pSoldier.bMarksmanship);
                        break;

                    case Stat.EXPERAMT:
                        pbSoldierStatPtr = (pSoldier.bExpLevel);
                        break;

                    case Stat.STRAMT:
                        pbSoldierStatPtr = ((int)pSoldier.bStrength);
                        break;

                    case Stat.LDRAMT:
                        pbSoldierStatPtr = (pSoldier.bLeadership);
                        break;
                }
            }


            // ptrs set up, now handle

            // Calc how many full points worth of stat changes we have accumulated in this stat (positive OR negative!)
            // NOTE: for simplicity, this hopes nobody will go up more than one level at once, which would change the subpoints/pt
            sPtsChanged = (psStatGainPtr) / usSubpointsPerPoint;

            // gone too high or too low?..handle the fact
            if ((pbStatPtr + sPtsChanged) > bMaxStatValue)
            {
                // reduce change to reach max value and reset stat gain ptr
                sPtsChanged = bMaxStatValue - pbStatPtr;
                psStatGainPtr = 0;
            }
            else
            if ((pbStatPtr + sPtsChanged) < bMinStatValue)
            {
                // reduce change to reach min value and reset stat gain ptr
                sPtsChanged = bMinStatValue - pbStatPtr;
                psStatGainPtr = 0;
            }


            // if the stat needs to change
            if (sPtsChanged != 0)
            {
                // Otherwise, use normal stat increase stuff...
                ChangeStat(pProfile, pSoldier, ubStat, sPtsChanged);
            }
        }

        return;
    }

    public static void HandleAnyStatChangesAfterAttack()
    {
        int cnt;
        SOLDIERTYPE? pSoldier;

        // must check everyone on player's team, not just the shooter
        for (cnt = 0, pSoldier = Globals.MercPtrs[0]; cnt <= Globals.gTacticalStatus.Team[Globals.MercPtrs[0].bTeam].bLastID; cnt++)//, pSoldier++)
        {
            if (pSoldier.bActive)
            {
                ProcessUpdateStats((Globals.gMercProfiles[pSoldier.ubProfile]), pSoldier);
            }
        }
    }


    public static int CalcNewSalary(int uiOldSalary, bool fIncrease, int uiMaxLimit)
    {
        int uiNewSalary;

        // if he was working for free, it's still free!
        if (uiOldSalary == 0)
        {
            return (0);
        }

        if (fIncrease)
        {
            uiNewSalary = (int)(uiOldSalary * Globals.SALARY_CHANGE_PER_LEVEL);
        }
        else
        {
            uiNewSalary = (int)(uiOldSalary / Globals.SALARY_CHANGE_PER_LEVEL);
        }

        // round it off to a reasonable multiple
        uiNewSalary = RoundOffSalary(uiNewSalary);

        // let's set some reasonable limits here, lest it get silly one day
        if (uiNewSalary > uiMaxLimit)
        {
            uiNewSalary = uiMaxLimit;
        }

        if (uiNewSalary < 5)
        {
            uiNewSalary = 5;
        }

        return (uiNewSalary);
    }


    public static int RoundOffSalary(int uiSalary)
    {
        int uiMultiple;


        // determine what multiple value the salary should be rounded off to
        if (uiSalary <= 250)
        {
            uiMultiple = 5;
        }
        else if (uiSalary <= 500)
        {
            uiMultiple = 10;
        }
        else if (uiSalary <= 1000)
        {
            uiMultiple = 25;
        }
        else if (uiSalary <= 2000)
        {
            uiMultiple = 50;
        }
        else if (uiSalary <= 5000)
        {
            uiMultiple = 100;
        }
        else if (uiSalary <= 10000)
        {
            uiMultiple = 500;
        }
        else if (uiSalary <= 25000)
        {
            uiMultiple = 1000;
        }
        else if (uiSalary <= 50000)
        {
            uiMultiple = 2000;
        }
        else
        {
            uiMultiple = 5000;
        }


        // if the salary doesn't divide evenly by the multiple
        if (uiSalary % uiMultiple > 0)
        {
            // then we have to make it so, as Picard would say <- We have to wonder how much Alex gets out
            // and while we're at it, we round up to next higher multiple if halfway
            uiSalary = ((uiSalary + (uiMultiple / 2)) / uiMultiple) * uiMultiple;
        }

        return (uiSalary);
    }


    public static int SubpointsPerPoint(Stat ubStat, int bExpLevel)
    {
        int usSubpointsPerPoint;

        // figure out how many subpoints this type of stat needs to change
        switch (ubStat)
        {
            case Stat.HEALTHAMT:
            case Stat.AGILAMT:
            case Stat.DEXTAMT:
            case Stat.WISDOMAMT:
            case Stat.STRAMT:
                // attributes
                usSubpointsPerPoint = Globals.ATTRIBS_SUBPOINTS_TO_IMPROVE;
                break;

            case Stat.MEDICALAMT:
            case Stat.EXPLODEAMT:
            case Stat.MECHANAMT:
            case Stat.MARKAMT:
            case Stat.LDRAMT:
                // skills
                usSubpointsPerPoint = Globals.SKILLS_SUBPOINTS_TO_IMPROVE;
                break;

            case Stat.EXPERAMT:
                usSubpointsPerPoint = Globals.LEVEL_SUBPOINTS_TO_IMPROVE * bExpLevel;
                break;

            default:
                // BETA message
                Messages.ScreenMsg(FontColor.FONT_ORANGE, MSG.BETAVERSION, "SubpointsPerPoint: ERROR - Unknown ubStat %d", ubStat.ToString());
                return (100);
        }

        return (usSubpointsPerPoint);
    }


    // handles stat changes for mercs not currently working for the player
    void HandleUnhiredMercImprovement(MERCPROFILESTRUCT? pProfile)
    {
        int ubNumStats;
        Stat ubStat;
        int usNumChances;

        ubNumStats = Globals.LAST_CHANGEABLE_STAT - Globals.FIRST_CHANGEABLE_STAT + 1;

        // if he's working on another job
        if (pProfile.bMercStatus == MercStatus.MERC_WORKING_ELSEWHERE)
        {
            // if he did't do anything interesting today
            if (Globals.Random.Next(100) < 20)
            {
                // no chance to change today
                return;
            }

            // it's real on the job experience, counts towards experience

            // all stats (including experience itself) get an equal chance to improve
            // 80 wisdom gives 8 rolls per stat per day, 10 stats, avg success rate 40% = 32pts per day,
            // so about 10 working days to hit lvl 2.  This seems high, but mercs don't actually "work" that often, and it's twice
            // as long to hit level 3.  If we go lower, attribs & skills will barely move.
            usNumChances = (pProfile.bWisdom / 10);
            for (ubStat = Globals.FIRST_CHANGEABLE_STAT; ubStat <= Globals.LAST_CHANGEABLE_STAT; ubStat++)
            {
                ProfileStatChange(pProfile, ubStat, usNumChances, 0);
            }
        }
        else
        {
            // if the merc just takes it easy (high level or stupid mercs are more likely to)
            if (((int)Globals.Random.Next(10) < pProfile.bExpLevel) || ((int)Globals.Random.Next(100) > pProfile.bWisdom))
            {
                // no chance to change today
                return;
            }

            // it's just practise/training back home
            do
            {
                // pick ONE stat at random to focus on (it may be beyond training cap, but so what, too hard to weed those out)
                ubStat = (Stat.FIRST_CHANGEABLE_STAT + Globals.Random.Next(ubNumStats));
                // except experience - can't practise that!
            } while (ubStat == Stat.EXPERAMT);

            // try to improve that one stat
            ProfileStatChange(pProfile, ubStat, (int)(pProfile.bWisdom / 2), Globals.FROM_TRAINING);
        }

        ProfileUpdateStats(pProfile);
    }


    // handles possible death of mercs not currently working for the player
    void HandleUnhiredMercDeaths(NPCID iProfileID)
    {
        int ubMaxDeaths;
        int sChance;
        MERCPROFILESTRUCT? pProfile = (Globals.gMercProfiles[iProfileID]);


        // if the player has never yet had the chance to hire this merc
        if (!(pProfile.ubMiscFlags3.HasFlag(ProfileMiscFlags3.PROFILE_MISC_FLAG3_PLAYER_HAD_CHANCE_TO_HIRE)))
        {
            // then we're not allowed to kill him (to avoid really pissing off player by killing his very favorite merc)
            return;
        }

        // how many in total can be killed like this depends on player's difficulty setting
        switch (Globals.gGameOptions.ubDifficultyLevel)
        {
            case DifficultyLevel.Easy:
                ubMaxDeaths = 1;
                break;
            case DifficultyLevel.Medium:
                ubMaxDeaths = 2;
                break;
            case DifficultyLevel.Hard:
                ubMaxDeaths = 3;
                break;
            default:
                Debug.Assert(false);
                ubMaxDeaths = 0;
                break;
        }

        // if we've already hit the limit in this game, skip these checks
        if (Globals.gStrategicStatus.ubUnhiredMercDeaths >= ubMaxDeaths)
        {
            return;
        }


        // calculate this merc's (small) chance to get killed today (out of 1000)
        sChance = 10 - pProfile.bExpLevel;

        switch (pProfile.bPersonalityTrait)
        {
            case PersonalityTrait.FORGETFUL:
            case PersonalityTrait.NERVOUS:
            case PersonalityTrait.PSYCHO:
                // these guys are somewhat more likely to get killed (they have "problems")
                sChance += 2;
                break;
        }

        // stealthy guys are slightly less likely to get killed (they're careful)
        if (pProfile.bSkillTrait == SkillTrait.STEALTHY)
        {
            sChance -= 1;
        }
        if (pProfile.bSkillTrait2 == SkillTrait.STEALTHY)
        {
            sChance -= 1;
        }


        if ((int)PreRandom(1000) < sChance)
        {
            // this merc gets Killed In Action!!!
            pProfile.bMercStatus = MercStatus.MERC_IS_DEAD;
            pProfile.uiDayBecomesAvailable = 0;

            // keep count of how many there have been
            Globals.gStrategicStatus.ubUnhiredMercDeaths++;

            //send an email as long as the merc is from aim
            if (iProfileID < NPCID.BIFF)
            {
                //send an email to the player telling the player that a merc died
                Emails.AddEmailWithSpecialData(MERC_DIED_ON_OTHER_ASSIGNMENT, MERC_DIED_ON_OTHER_ASSIGNMENT_LENGTH, EmailAddresses.AIM_SITE, GameClock.GetWorldTotalMin(), 0, iProfileID);
            }
        }
    }

    // returns a number between 0-100, this is an estimate of how far a player has progressed through the game
    public static int CurrentPlayerProgressPercentage()
    {
        int uiCurrentIncome;
        int uiPossibleIncome;
        int ubCurrentProgress;
        int ubKillsPerPoint;
        int usKillsProgress;
        int usControlProgress;

        if (Globals.gfEditMode)
        {
            return 0;
        }

        // figure out the player's current mine income
        uiCurrentIncome = StrategicMines.PredictIncomeFromPlayerMines();

        // figure out the player's potential mine income
        uiPossibleIncome = StrategicMines.CalcMaxPlayerIncomeFromMines();

        // either of these indicates a critical failure of some sort
        Debug.Assert(uiPossibleIncome > 0);
        Debug.Assert(uiCurrentIncome <= uiPossibleIncome);

        // for a rough guess as to how well the player is doing,
        // we'll take the current mine income / potential mine income as a percentage

        //Kris:  Make sure you don't divide by zero!!!
        if (uiPossibleIncome > 0)
        {
            ubCurrentProgress = ((uiCurrentIncome * Globals.PROGRESS_PORTION_INCOME) / uiPossibleIncome);
        }
        else
        {
            ubCurrentProgress = 0;
        }

        // kills per point depends on difficulty, and should match the ratios of starting enemy populations (730/1050/1500)
        switch (Globals.gGameOptions.ubDifficultyLevel)
        {
            case DifficultyLevel.Easy:
                ubKillsPerPoint = 7;
                break;
            case DifficultyLevel.Medium:
                ubKillsPerPoint = 10;
                break;
            case DifficultyLevel.Hard:
                ubKillsPerPoint = 15;
                break;
            default:
                Debug.Assert(false);
                ubKillsPerPoint = 10;
                break;
        }

        usKillsProgress = Globals.gStrategicStatus.usPlayerKills / ubKillsPerPoint;
        if (usKillsProgress > Globals.PROGRESS_PORTION_KILLS)
        {
            usKillsProgress = Globals.PROGRESS_PORTION_KILLS;
        }

        // add kills progress to income progress
        ubCurrentProgress += usKillsProgress;


        // 19 sectors in mining towns + 3 wilderness SAMs each count double.  Balime & Meduna are extra and not required
        usControlProgress = CalcImportantSectorControl();
        if (usControlProgress > Globals.PROGRESS_PORTION_CONTROL)
        {
            usControlProgress = Globals.PROGRESS_PORTION_CONTROL;
        }

        // add control progress
        ubCurrentProgress += usControlProgress;

        return (ubCurrentProgress);
    }


    public static int HighestPlayerProgressPercentage()
    {
        if (Globals.gfEditMode)
        {
            return 0;
        }

        return (Globals.gStrategicStatus.ubHighestProgress);
    }


    // monitors the highest level of progress that player has achieved so far (checking hourly),
    // as opposed to his immediate situation (which may be worse if he's suffered a setback).
    void HourlyProgressUpdate()
    {
        int ubCurrentProgress;

        ubCurrentProgress = CurrentPlayerProgressPercentage();

        // if this is new high, remember it as that
        if (ubCurrentProgress > Globals.gStrategicStatus.ubHighestProgress)
        {
            // CJC:  note when progress goes above certain values for the first time

            // at 35% start the Madlab quest
            if (ubCurrentProgress >= 35 && Globals.gStrategicStatus.ubHighestProgress < 35)
            {
//                HandleScientistAWOLMeanwhileScene();
            }

            // at 50% make Mike available to the strategic AI
            if (ubCurrentProgress >= 50 && Globals.gStrategicStatus.ubHighestProgress < 50)
            {
                Facts.SetFactTrue(FACT.MIKE_AVAILABLE_TO_ARMY);
            }

            // at 70% add Iggy to the world
            if (ubCurrentProgress >= 70 && Globals.gStrategicStatus.ubHighestProgress < 70)
            {
                Globals.gMercProfiles[NPCID.IGGY].sSectorX = 5;
                Globals.gMercProfiles[NPCID.IGGY].sSectorY = MAP_ROW.C;
            }

            Globals.gStrategicStatus.ubHighestProgress = ubCurrentProgress;

            // debug message
            Messages.ScreenMsg(MSG_FONT_RED, MSG.DEBUG, "New player progress record: %d%%", Globals.gStrategicStatus.ubHighestProgress);
        }
    }



    // # if JA2TESTVERSION
    //    void TestDumpStatChanges()
    //    {
    //        int uiProfileId;
    //        int ubStat;
    //        char[] zPrintFileName = new char[60];
    //        //FILE* FDump;
    //        MERCPROFILESTRUCT? pProfile;
    //        bool fMercUsed;
    //        char[] cEvolutionChars = new char[3] { '+', '=', '-' };
    //        int[] uiTotalSuccesses = new int[12];
    //        int[] uiTotalChances = new int[12];
    //
    //
    //        // clear totals
    //        // memset(uiTotalSuccesses, 0, sizeof(uiTotalSuccesses));
    //        // memset(uiTotalChances, 0, sizeof(uiTotalChances));
    //
    //        // open output file
    //        strcpy(zPrintFileName, "C:\\Temp\\StatChanges.TXT");
    //        FDump = fopen(zPrintFileName, "wt");
    //
    //        if (FDump == null)
    //        {
    //            return;
    //        }
    //
    //
    //        // print headings
    //        fprintf(FDump, "   NAME   SRV EVL ");
    //        fprintf(FDump, "---HEALTH-- --AGILITY-- -DEXTERITY- ---WISDOM-- --MEDICAL-- --EXPLOSIV- --MECHANIC- --MARKSMAN- -EXP.LEVEL- --STRENGTH- -LEADERSHIP");
    //        fprintf(FDump, "\n");
    //
    //
    //        // loop through profiles
    //        for (uiProfileId = 0; uiProfileId < NUM_PROFILES; uiProfileId++)
    //        {
    //            pProfile = (gMercProfiles[uiProfileId]);
    //
    //            fMercUsed = false;
    //
    //            // see if this guy should be printed at all (only mercs actually used are dumped)
    //            for (ubStat = FIRST_CHANGEABLE_STAT; ubStat <= LAST_CHANGEABLE_STAT; ubStat++)
    //            {
    //                if (pProfile.usStatChangeChances[ubStat] > 0)
    //                {
    //                    fMercUsed = true;
    //                    break;
    //                }
    //            }
    //
    //            if (fMercUsed)
    //            {
    //                // print nickname
    //                fprintf(FDump, "%-10ls ", pProfile.zNickname);
    //                // print days served
    //                fprintf(FDump, "%3d ", pProfile.usTotalDaysServed);
    //                // print evolution type
    //                fprintf(FDump, "%c ", cEvolutionChars[pProfile.bEvolution]);
    //
    //                // now print all non-zero stats
    //                for (ubStat = FIRST_CHANGEABLE_STAT; ubStat <= LAST_CHANGEABLE_STAT; ubStat++)
    //                {
    //                    if (pProfile.usStatChangeChances[ubStat] > 0)
    //                    {
    //                        // print successes/chances
    //                        fprintf(FDump, " %5d/%-5d", pProfile.usStatChangeSuccesses[ubStat], pProfile.usStatChangeChances[ubStat]);
    //                    }
    //                    else
    //                    {
    //                        //
    //                        fprintf(FDump, "            ");
    //                    }
    //
    //                    uiTotalSuccesses[ubStat] += pProfile.usStatChangeSuccesses[ubStat];
    //                    uiTotalChances[ubStat] += pProfile.usStatChangeChances[ubStat];
    //                }
    //
    //                // newline
    //                fprintf(FDump, "\n");
    //            }
    //        }
    //
    //
    //        // print totals:
    //        fprintf(FDump, "TOTAL        ");
    //
    //        for (ubStat = FIRST_CHANGEABLE_STAT; ubStat <= LAST_CHANGEABLE_STAT; ubStat++)
    //        {
    //            fprintf(FDump, " %5d/%-5d", uiTotalSuccesses[ubStat], uiTotalChances[ubStat]);
    //        }
    //
    //        fprintf(FDump, "\n");
    //
    //
    //        fclose(FDump);
    //    }
    //#endif



    void AwardExperienceBonusToActiveSquad(EXP_BONUS ubExpBonusType)
    {
        int usXPs = 0;
        int ubGuynum;
        SOLDIERTYPE? pSoldier;


        Debug.Assert(ubExpBonusType < EXP_BONUS.NUM_EXP_BONUS_TYPES);

        switch (ubExpBonusType)
        {
            case EXP_BONUS.MINIMUM:
                usXPs = 25;
                break;
            case EXP_BONUS.SMALL:
                usXPs = 50;
                break;
            case EXP_BONUS.AVERAGE:
                usXPs = 100;
                break;
            case EXP_BONUS.LARGE:
                usXPs = 200;
                break;
            case EXP_BONUS.MAXIMUM:
                usXPs = 400;
                break;
        }

        // to do: find guys in sector on the currently active squad, those that are conscious get this amount in XPs
        for (ubGuynum = Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bFirstID, pSoldier = Globals.MercPtrs[ubGuynum];
                    ubGuynum <= Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bLastID;
                    ubGuynum++)//, pSoldier++)
        {
            if (pSoldier.bActive && pSoldier.bInSector
//                && IsMercOnCurrentSquad(pSoldier)
                && (pSoldier.bLife >= Globals.CONSCIOUSNESS) &&
                     !(pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE)) && !AM_A_ROBOT(pSoldier))
            {
                StatChange(pSoldier, Stat.EXPERAMT, usXPs, 0);
            }
        }
    }



    public static void BuildStatChangeString(out string wString, string wName, bool fIncrease, int sPtsChanged, Stat ubStat)
    {
        int ubStringIndex;

        Debug.Assert(sPtsChanged != 0);
        Debug.Assert(ubStat >= Stat.FIRST_CHANGEABLE_STAT);
        Debug.Assert(ubStat <= Stat.LAST_CHANGEABLE_STAT);

        // if just a 1 point change
        if (Math.Abs(sPtsChanged) == 1)
        {
            // use singular
            ubStringIndex = 2;
        }
        else
        {
            ubStringIndex = 3;
            // use plural
        }

        if (ubStat == Stat.EXPERAMT)
        {
            // use "level/levels instead of point/points
            ubStringIndex += 2;
        }

        wString = wprintf("%s %s %d %s %s", wName, sPreStatBuildString[fIncrease ? 1 : 0], Math.Abs(sPtsChanged),
                        sPreStatBuildString[ubStringIndex], sStatGainStrings[ubStat - Globals.FIRST_CHANGEABLE_STAT]);
    }



    private static int CalcImportantSectorControl()
    {
        int ubMapX;
        MAP_ROW ubMapY;
        int ubSectorControlPts = 0;


        for (ubMapX = 1; ubMapX < Globals.MAP_WORLD_X - 1; ubMapX++)
        {
            for (ubMapY = (MAP_ROW)1; (int)ubMapY < Globals.MAP_WORLD_Y - 1; ubMapY++)
            {
                // if player controlled
                if (Globals.strategicMap[StrategicMap.CALCULATE_STRATEGIC_INDEX(ubMapX, ubMapY)].fEnemyControlled == false)
                {
                    // towns where militia can be trained and SAM sites are important sectors
//                    if (MilitiaTrainingAllowedInSector(ubMapX, ubMapY, 0))
//                    {
//                        ubSectorControlPts++;
//
//                        // SAM sites count double - they have no income, but have significant air control value
//                        if (IsThisSectorASAMSector(ubMapX, ubMapY, 0))
//                        {
//                            ubSectorControlPts++;
//                        }
//                    }
                }
            }
        }

        return (ubSectorControlPts);
    }


    void MERCMercWentUpALevelSendEmail(int ubMercMercIdValue)
    {
        int ubEmailOffset = 0;

        ubEmailOffset = MERC_UP_LEVEL_BIFF + MERC_UP_LEVEL_LENGTH_BIFF * (ubMercMercIdValue);
        Emails.AddEmail(ubEmailOffset, MERC_UP_LEVEL_LENGTH_BIFF, EmailAddresses.SPECK_FROM_MERC, GameClock.GetWorldTotalMin());
    }
}

// types of experience bonus awards
public enum EXP_BONUS
{
    MINIMUM,
    SMALL,
    AVERAGE,
    LARGE,
    MAXIMUM,
    NUM_EXP_BONUS_TYPES,
};
