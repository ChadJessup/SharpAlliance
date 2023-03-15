using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.SubSystems;

using static SharpAlliance.Core.EnglishText;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class DrugsAndAlcohol
{
    private static int[] ubDrugTravelRate = { 4, 2 };
    private static int[] ubDrugWearoffRate = { 2, 2 };
    private static int[] ubDrugEffect = { 15, 8 };
    private static int[] ubDrugSideEffect = { 20, 10 };
    private static int[] ubDrugSideEffectRate = { 2, 1 };

    int GetDrugType(Items usItem)
    {
        if (usItem == Items.ADRENALINE_BOOSTER)
        {
            return (DRUG_TYPE_ADRENALINE);
        }

        if (usItem == Items.REGEN_BOOSTER)
        {
            return (DRUG_TYPE_REGENERATION);
        }

        if (usItem == Items.ALCOHOL || usItem == Items.WINE || usItem == Items.BEER)
        {
            return (DRUG_TYPE_ALCOHOL);
        }


        return (NO_DRUG);
    }


    bool ApplyDrugs(SOLDIERTYPE? pSoldier, OBJECTTYPE? pObject)
    {
        int ubDrugType;
        int ubKitPoints;
        int bRegenPointsGained;
        Items usItem;

        usItem = (Items)pObject.usItem;

        // If not a syringe, return

        ubDrugType = GetDrugType(usItem);

        // Determine what type of drug....
        if (ubDrugType == NO_DRUG)
        {
            return (false);
        }

        // do switch for Larry!!
        if (pSoldier.ubProfile == NPCID.LARRY_NORMAL)
        {
            pSoldier = SoldierProfileSubSystem.SwapLarrysProfiles(pSoldier);
        }
        else if (pSoldier.ubProfile == NPCID.LARRY_DRUNK)
        {
            gMercProfiles[NPCID.LARRY_DRUNK].bNPCData = 0;
        }

        if (ubDrugType < NUM_COMPLEX_DRUGS)
        {

            // Add effects
            if ((pSoldier.bFutureDrugEffect[ubDrugType] + ubDrugEffect[ubDrugType]) < 127)
            {
                pSoldier.bFutureDrugEffect[ubDrugType] += ubDrugEffect[ubDrugType];
            }
            pSoldier.bDrugEffectRate[ubDrugType] = ubDrugTravelRate[ubDrugType];

            // Increment times used during lifetime...
            // CAP!
            if (ubDrugType == DRUG_TYPE_ADRENALINE)
            {
                if (gMercProfiles[pSoldier.ubProfile].ubNumTimesDrugUseInLifetime != 255)
                {
                    gMercProfiles[pSoldier.ubProfile].ubNumTimesDrugUseInLifetime++;
                }
            }

            // Reset once we sleep...
            pSoldier.bTimesDrugUsedSinceSleep[ubDrugType]++;

            // Increment side effects..
            if ((pSoldier.bDrugSideEffect[ubDrugType] + ubDrugSideEffect[ubDrugType]) < 127)
            {
                pSoldier.bDrugSideEffect[ubDrugType] += (ubDrugSideEffect[ubDrugType]);
            }
            // Stop side effects until were done....
            pSoldier.bDrugSideEffectRate[ubDrugType] = 0;


            if (ubDrugType == DRUG_TYPE_ALCOHOL)
            {
                // ATE: use kit points...
                if (usItem == Items.ALCOHOL)
                {
                    ubKitPoints = 10;
                }
                else if (usItem == Items.WINE)
                {
                    ubKitPoints = 20;
                }
                else
                {
                    ubKitPoints = 100;
                }

                DrugsAndAlcohol.UseKitPoints(pObject, ubKitPoints, pSoldier);
            }
            else
            {
                // Remove the object....
                ItemSubSystem.DeleteObj(pObject);

                // ATE: Make guy collapse from heart attack if too much stuff taken....
                if (pSoldier.bDrugSideEffectRate[ubDrugType] > (ubDrugSideEffect[ubDrugType] * 3))
                {
                    // Keel over...
                    Points.DeductPoints(pSoldier, 0, 10000);

                    // Permanently lower certain stats...
                    pSoldier.bWisdom -= 5;
                    pSoldier.bDexterity -= 5;
                    pSoldier.bStrength -= 5;

                    if (pSoldier.bWisdom < 1)
                    {
                        pSoldier.bWisdom = 1;
                    }

                    if (pSoldier.bDexterity < 1)
                    {
                        pSoldier.bDexterity = 1;
                    }

                    if (pSoldier.bStrength < 1)
                    {
                        pSoldier.bStrength = 1;
                    }

                    // export stat changes to profile
                    gMercProfiles[pSoldier.ubProfile].bWisdom = pSoldier.bWisdom;
                    gMercProfiles[pSoldier.ubProfile].bDexterity = pSoldier.bDexterity;
                    gMercProfiles[pSoldier.ubProfile].bStrength = pSoldier.bStrength;

                    // make those stats RED for a while...
                    pSoldier.uiChangeWisdomTime = GetJA2Clock();
                    pSoldier.usValueGoneUp &= ~(WIS_INCREASE);
                    pSoldier.uiChangeDexterityTime = GetJA2Clock();
                    pSoldier.usValueGoneUp &= ~(DEX_INCREASE);
                    pSoldier.uiChangeStrengthTime = GetJA2Clock();
                    pSoldier.usValueGoneUp &= ~(STRENGTH_INCREASE);
                }
            }
        }
        else
        {
            if (ubDrugType == DRUG_TYPE_REGENERATION)
            {
                // each use of a regen booster over 1, each day, reduces the effect
                bRegenPointsGained = REGEN_POINTS_PER_BOOSTER * pObject.bStatus[0] / 100;
                // are there fractional %s left over?
                if ((pObject.bStatus[0] % (100 / REGEN_POINTS_PER_BOOSTER)) != 0)
                {
                    // chance of an extra point
                    if (PreRandom(100 / REGEN_POINTS_PER_BOOSTER) < (int)(pObject.bStatus[0] % (100 / REGEN_POINTS_PER_BOOSTER)))
                    {
                        bRegenPointsGained++;
                    }
                }

                bRegenPointsGained -= pSoldier.bRegenBoostersUsedToday;
                if (bRegenPointsGained > 0)
                {
                    // can't go above the points you get for a full boost
                    pSoldier.bRegenerationCounter = Math.Min(pSoldier.bRegenerationCounter + bRegenPointsGained, REGEN_POINTS_PER_BOOSTER);
                }
                pSoldier.bRegenBoostersUsedToday++;
            }

            // remove object
            ItemSubSystem.DeleteObj(pObject);
        }

        if (ubDrugType == DRUG_TYPE_ALCOHOL)
        {
            Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG_INTERFACE, pMessageStrings[MSG.DRANK_SOME], pSoldier.name, ShortItemNames[usItem]);
        }
        else
        {
            Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG_INTERFACE, pMessageStrings[MSG.MERC_TOOK_DRUG], pSoldier.name);
        }

        // Dirty panel
        fInterfacePanelDirty = DIRTYLEVEL2;

        return (true);
    }

    public static int UseKitPoints(OBJECTTYPE? pObj, int usPoints, SOLDIERTYPE? pSoldier)
    {
        // start consuming from the last kit in, so we end up with fewer fuller kits rather than
        // lots of half-empty ones.
        int bLoop;
        int usOriginalPoints = usPoints;

        for (bLoop = pObj.ubNumberOfObjects - 1; bLoop >= 0; bLoop--)
        {
            if (usPoints < pObj.bStatus[bLoop])
            {
                pObj.bStatus[bLoop] -= usPoints;
                return (usOriginalPoints);
            }
            else
            {
                // consume this kit totally
                usPoints -= pObj.bStatus[bLoop];
                pObj.bStatus[bLoop] = 0;

                pObj.ubNumberOfObjects--;
            }
        }

        // check if pocket/hand emptied..update inventory, then update panel
        if (pObj.ubNumberOfObjects == 0)
        {
            // Delete object
            ItemSubSystem.DeleteObj(pObj);

            // dirty interface panel
            DirtyMercPanelInterface(pSoldier, DIRTYLEVEL2);
        }

        return (usOriginalPoints - usPoints);
    }

    void HandleEndTurnDrugAdjustments(SOLDIERTYPE? pSoldier)
    {
        int cnt, cnt2;
        int iNumLoops;
        //	int bBandaged;

        for (cnt = 0; cnt < NUM_COMPLEX_DRUGS; cnt++)
        {
            // If side effect aret is non-zero....
            if (pSoldier.bDrugSideEffectRate[cnt] > 0)
            {
                // Subtract some...
                pSoldier.bDrugSideEffect[cnt] -= pSoldier.bDrugSideEffectRate[cnt];

                // If we're done, we're done!
                if (pSoldier.bDrugSideEffect[cnt] <= 0)
                {
                    pSoldier.bDrugSideEffect[cnt] = 0;
                    fInterfacePanelDirty = DIRTYLEVEL1;
                }
            }

            // IF drug rate is -ve, it's being worn off...
            if (pSoldier.bDrugEffectRate[cnt] < 0)
            {
                pSoldier.bDrugEffect[cnt] -= (-1 * pSoldier.bDrugEffectRate[cnt]);

                // Have we run out?
                if (pSoldier.bDrugEffect[cnt] <= 0)
                {
                    pSoldier.bDrugEffect[cnt] = 0;

                    // Dirty panel
                    fInterfacePanelDirty = DIRTYLEVEL2;

                    // Start the bad news!
                    pSoldier.bDrugSideEffectRate[cnt] = ubDrugSideEffectRate[cnt];

                    // The drug rate is 0 now too
                    pSoldier.bDrugEffectRate[cnt] = 0;

                    // Once for each 'level' of crash....
                    iNumLoops = (pSoldier.bDrugSideEffect[cnt] / ubDrugSideEffect[cnt]) + 1;

                    for (cnt2 = 0; cnt2 < iNumLoops; cnt2++)
                    {
                        // OK, give a much BIGGER morale downer
                        if (cnt == DRUG_TYPE_ALCOHOL)
                        {
                            Morale.HandleMoraleEvent(pSoldier, MoraleEventNames.MORALE_ALCOHOL_CRASH, pSoldier.sSectorX, pSoldier.sSectorY, pSoldier.bSectorZ);
                        }
                        else
                        {
                            Morale.HandleMoraleEvent(pSoldier, MoraleEventNames.MORALE_DRUGS_CRASH, pSoldier.sSectorX, pSoldier.sSectorY, pSoldier.bSectorZ);
                        }
                    }
                }
            }

            // Add increase ineffect....
            if (pSoldier.bDrugEffectRate[cnt] > 0)
            {
                // Seap some in....
                pSoldier.bFutureDrugEffect[cnt] -= pSoldier.bDrugEffectRate[cnt];
                pSoldier.bDrugEffect[cnt] += pSoldier.bDrugEffectRate[cnt];

                // Refresh morale w/ new drug value...
                Morale.RefreshSoldierMorale(pSoldier);

                // Check if we need to stop 'adding'
                if (pSoldier.bFutureDrugEffect[cnt] <= 0)
                {
                    pSoldier.bFutureDrugEffect[cnt] = 0;
                    // Change rate to -ve..
                    pSoldier.bDrugEffectRate[cnt] = -ubDrugWearoffRate[cnt];
                }
            }
        }

        if (pSoldier.bRegenerationCounter > 0)
        {
            //		bBandaged = BANDAGED( pSoldier );

            // increase life
            pSoldier.bLife = Math.Min(pSoldier.bLife + LIFE_GAIN_PER_REGEN_POINT, pSoldier.bLifeMax);

            if (pSoldier.bLife == pSoldier.bLifeMax)
            {
                pSoldier.bBleeding = 0;
            }
            else if (pSoldier.bBleeding + pSoldier.bLife > pSoldier.bLifeMax)
            {
                // got to reduce amount of bleeding
                pSoldier.bBleeding = (pSoldier.bLifeMax - pSoldier.bLife);
            }

            // decrement counter
            pSoldier.bRegenerationCounter--;
        }
    }

    int GetDrugEffect(SOLDIERTYPE? pSoldier, int ubDrugType)
    {
        return (pSoldier.bDrugEffect[ubDrugType]);
    }


    int GetDrugSideEffect(SOLDIERTYPE? pSoldier, int ubDrugType)
    {
        // If we have a o-positive effect
        if (pSoldier.bDrugEffect[ubDrugType] > 0)
        {
            return (0);
        }
        else
        {
            return (pSoldier.bDrugSideEffect[ubDrugType]);
        }
    }

    void HandleAPEffectDueToDrugs(SOLDIERTYPE? pSoldier, out int pubPoints)
    {
        DrunkLevel bDrunkLevel;
        pubPoints = 0;
        int sPoints = (pubPoints);

        // Are we in a side effect or good effect?
        if (pSoldier.bDrugEffect[DRUG_TYPE_ADRENALINE] > 0)
        {
            // Adjust!
            sPoints += pSoldier.bDrugEffect[DRUG_TYPE_ADRENALINE];
        }
        else if (pSoldier.bDrugSideEffect[DRUG_TYPE_ADRENALINE] > 0)
        {
            // Adjust!
            sPoints -= pSoldier.bDrugSideEffect[DRUG_TYPE_ADRENALINE];

            if (sPoints < AP.MINIMUM)
            {
                sPoints = AP.MINIMUM;
            }
        }

        bDrunkLevel = GetDrunkLevel(pSoldier);

        if (bDrunkLevel == DrunkLevel.HUNGOVER)
        {
            // Reduce....
            sPoints -= HANGOVER_AP_REDUCE;

            if (sPoints < AP.MINIMUM)
            {
                sPoints = AP.MINIMUM;
            }
        }

      (pubPoints) = (int)sPoints;
    }


    void HandleBPEffectDueToDrugs(SOLDIERTYPE? pSoldier, out int psPointReduction)
    {
        DrunkLevel bDrunkLevel;
        psPointReduction = 0;

        // Are we in a side effect or good effect?
        if (pSoldier.bDrugEffect[DRUG_TYPE_ADRENALINE] > 0)
        {
            // Adjust!
            (psPointReduction) -= (pSoldier.bDrugEffect[DRUG_TYPE_ADRENALINE] * BP.RATIO_RED_PTS_TO_NORMAL);
        }
        else if (pSoldier.bDrugSideEffect[DRUG_TYPE_ADRENALINE] > 0)
        {
            // Adjust!
            (psPointReduction) += (pSoldier.bDrugSideEffect[DRUG_TYPE_ADRENALINE] * BP.RATIO_RED_PTS_TO_NORMAL);
        }

        bDrunkLevel = GetDrunkLevel(pSoldier);

        if (bDrunkLevel == DrunkLevel.HUNGOVER)
        {
            // Reduce....
            (psPointReduction) += HANGOVER_BP_REDUCE;
        }
    }


    public static DrunkLevel GetDrunkLevel(SOLDIERTYPE? pSoldier)
    {
        int bNumDrinks;

        // If we have a -ve effect ...
        if (pSoldier.bDrugEffect[DRUG_TYPE_ALCOHOL] == 0 && pSoldier.bDrugSideEffect[DRUG_TYPE_ALCOHOL] == 0)
        {
            return (DrunkLevel.SOBER);
        }

        if (pSoldier.bDrugEffect[DRUG_TYPE_ALCOHOL] == 0 && pSoldier.bDrugSideEffect[DRUG_TYPE_ALCOHOL] != 0)
        {
            return (DrunkLevel.HUNGOVER);
        }

        // Calculate how many dinks we have had....
        bNumDrinks = (pSoldier.bDrugEffect[DRUG_TYPE_ALCOHOL] / ubDrugEffect[DRUG_TYPE_ALCOHOL]);

        if (bNumDrinks <= 3)
        {
            return (DrunkLevel.FEELING_GOOD);
        }
        else if (bNumDrinks <= 4)
        {
            return (DrunkLevel.BORDERLINE);
        }
        else
        {
            return (DrunkLevel.DRUNK);
        }
    }

    public static int EffectStatForBeingDrunk(SOLDIERTYPE? pSoldier, int iStat)
    {
        return ((iStat * giDrunkModifier[GetDrunkLevel(pSoldier)] / 100));
    }


    bool MercUnderTheInfluence(SOLDIERTYPE? pSoldier)
    {
        // Are we in a side effect or good effect?
        if (pSoldier.bDrugEffect[DRUG_TYPE_ADRENALINE] > 0)
        {
            return (true);
        }
        else if (pSoldier.bDrugSideEffect[DRUG_TYPE_ADRENALINE] > 0)
        {
            return (true);
        }

        if (GetDrunkLevel(pSoldier) != DrunkLevel.SOBER)
        {
            return (true);
        }

        return (false);
    }
}
